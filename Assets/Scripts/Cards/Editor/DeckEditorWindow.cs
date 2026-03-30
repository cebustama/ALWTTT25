#if UNITY_EDITOR
using ALWTTT.Data;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// Deck authoring tool for ALWTTT.
    ///
    /// Supports two card entry modes in JSON:
    ///   - Reference existing card: { "cardId": "existing_id" }
    ///   - Create new card:         { "kind": "Action", "id": "new_id", "effects": [...] }
    ///
    /// New cards are staged in memory and saved alongside the deck asset on Save/Save As.
    /// Pending new cards are shown with a [NEW] badge in the staged list.
    /// Pending cards are NOT serialized and will be lost if Unity reloads scripts — save before compiling.
    ///
    /// Layout:
    ///   [Header / toolbar]
    ///   [Body: staged list | splitter | metadata + JSON]
    ///   [Horizontal splitter]
    ///   [Catalogue strip — full width, toggle Show Catalogue]
    ///   [Status bar]
    /// </summary>
    public sealed class DeckEditorWindow : EditorWindow
    {
        private const float SplitterWidth = 5f;
        private const float MinLeftPaneWidth = 260f;
        private const float MinRightPaneWidth = 320f;
        private const float PanePadding = 6f;
        private const float RowHeight = 20f;
        private const float StatusBarHeight = 24f;
        private const float CatalogueMinHeight = 80f;
        private const float CatalogueMaxHeight = 600f;

        // Serialized (survive domain reload)
        [SerializeField] private BandDeckData _targetDeckAsset;
        [SerializeField] private GigSetupConfigData _gigSetupConfig;
        [SerializeField] private ALWTTTProjectRegistriesSO _registries;
        [SerializeField] private StagedDeck _staged;
        [SerializeField, TextArea(5, 12)] private string _jsonText = "";
        [SerializeField] private bool _showCatalogue = false;
        [SerializeField] private bool _filterAction = true;
        [SerializeField] private bool _filterComposition = true;
        [SerializeField] private string _catalogueSearch = "";
        [SerializeField] private float _catalogueHeight = 220f;
        [SerializeField] private Vector2 _catalogueScroll;
        [SerializeField] private float _splitRatio = 0.42f;
        [SerializeField] private Vector2 _leftScroll;
        [SerializeField] private Vector2 _rightScroll;

        // Non-serialized runtime state
        private bool _isDraggingBodySplitter;
        private bool _isDraggingCatSplitter;
        private string _statusMessage = "";
        private bool _statusIsError = false;
        private string _importErrors = "";
        private string _importWarnings = "";

        private List<CardDefinition> _allCatalogueCards = new();
        private List<CardDefinition> _filteredCatalogueCards = new();
        private SearchField _searchField;
        private bool _catalogueDirty = true;
        private ValidationResult _validation;

        [MenuItem("ALWTTT/Cards/Deck Editor", priority = 11)]
        public static void Open() => GetWindow<DeckEditorWindow>("Deck Editor");

        private void OnEnable()
        {
            _searchField ??= new SearchField();
            _staged ??= new StagedDeck();

            // Clean up entries whose pending objects were lost on domain reload
            _staged.cards?.RemoveAll(e => e != null && !e.IsValid);

            ResolveRegistries();
            RefreshValidation();
            RefreshCatalogue();
        }

        private void ResolveRegistries()
        {
            if (_registries != null) return;
            _registries = ALWTTTProjectRegistriesSO.FindInResources();
            if (_registries != null) return;
            var guids = AssetDatabase.FindAssets("t:ALWTTTProjectRegistriesSO");
            if (guids?.Length > 0)
                _registries = AssetDatabase.LoadAssetAtPath<ALWTTTProjectRegistriesSO>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        // ------------------------------------------------------------------
        // OnGUI
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            _searchField ??= new SearchField();

            DrawHeader();

            Rect full = GUILayoutUtility.GetRect(
                0f, 100000f, 0f, 100000f,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (full.width <= 1f) return;

            SplitLayout(full,
                out Rect bodyRect,
                out Rect catSplitterRect,
                out Rect catRect,
                out Rect statusRect);

            DrawBody(bodyRect);

            if (_showCatalogue)
            {
                HandleCatalogueSplitter(full, catSplitterRect);
                DrawHorizontalSplitter(catSplitterRect);
                DrawCatalogueStrip(catRect);
            }

            DrawStatusBar(statusRect);
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private void SplitLayout(Rect full,
                                  out Rect body, out Rect catSplit, out Rect cat, out Rect status)
        {
            float splH = _showCatalogue ? SplitterWidth : 0f;
            float catH = _showCatalogue
                ? Mathf.Clamp(_catalogueHeight, CatalogueMinHeight, CatalogueMaxHeight)
                : 0f;
            float bodyH = Mathf.Max(60f, full.height - StatusBarHeight - splH - catH - 4f);

            body = new Rect(full.x, full.y, full.width, bodyH);
            catSplit = new Rect(full.x, body.yMax, full.width, splH);
            cat = new Rect(full.x, catSplit.yMax, full.width, catH);
            status = new Rect(full.x, cat.yMax + 2f, full.width, StatusBarHeight);
        }

        // ------------------------------------------------------------------
        // Header
        // ------------------------------------------------------------------

        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("ALWTTT — Band Deck Editor", EditorStyles.boldLabel);

                // Row 1: target deck + save/export
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Target Deck", GUILayout.Width(80));
                    var na = (BandDeckData)EditorGUILayout.ObjectField(_targetDeckAsset, typeof(BandDeckData), false, GUILayout.ExpandWidth(true));
                    if (na != _targetDeckAsset) _targetDeckAsset = na;

                    if (GUILayout.Button("Load", GUILayout.Width(50))) { DoLoadDeck(); GUIUtility.ExitGUI(); }
                    using (new EditorGUI.DisabledScope(_targetDeckAsset == null))
                        if (GUILayout.Button("Ping", GUILayout.Width(40))) EditorGUIUtility.PingObject(_targetDeckAsset);
                    using (new EditorGUI.DisabledScope(!CanSave()))
                        if (GUILayout.Button("Save", GUILayout.Width(45))) { DoSave(); GUIUtility.ExitGUI(); }
                    if (GUILayout.Button("Save As", GUILayout.Width(65))) { DoSaveAs(); GUIUtility.ExitGUI(); }
                    using (new EditorGUI.DisabledScope((_staged?.cards?.Count ?? 0) == 0))
                        if (GUILayout.Button("Export JSON", GUILayout.Width(88))) { DoExportJson(); GUIUtility.ExitGUI(); }

                    // Clear All
                    if (GUILayout.Button("Clear All", GUILayout.Width(68))) { DoClearAll(); GUIUtility.ExitGUI(); }
                }

                // Row 2: gig setup + registries + catalogue toggle
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Gig Setup Config", GUILayout.Width(110));
                    _gigSetupConfig = (GigSetupConfigData)EditorGUILayout.ObjectField(_gigSetupConfig, typeof(GigSetupConfigData), false, GUILayout.ExpandWidth(true));
                    bool canReg = _gigSetupConfig != null && _staged?.sourceAsset != null;
                    using (new EditorGUI.DisabledScope(!canReg))
                    {
                        if (GUILayout.Button("Add to Gig Setup", GUILayout.Width(120))) { DoAddToGigSetup(); GUIUtility.ExitGUI(); }
                        if (GUILayout.Button("Remove", GUILayout.Width(65))) { DoRemoveFromGigSetup(); GUIUtility.ExitGUI(); }
                    }

                    GUILayout.FlexibleSpace();

                    // Registries (needed for new-card creation with status effects)
                    EditorGUILayout.LabelField("Registries", GUILayout.Width(66));
                    var nr = (ALWTTTProjectRegistriesSO)EditorGUILayout.ObjectField(_registries, typeof(ALWTTTProjectRegistriesSO), false, GUILayout.Width(140));
                    if (nr != _registries) _registries = nr;

                    bool newShowCat = GUILayout.Toggle(_showCatalogue, "Show Catalogue", EditorStyles.toolbarButton, GUILayout.Width(110));
                    if (newShowCat != _showCatalogue) { _showCatalogue = newShowCat; if (_showCatalogue) RefreshCatalogue(); Repaint(); }
                }

                // Domain-reload warning if pending cards exist
                if (_staged != null && _staged.HasPendingNewCards)
                    EditorGUILayout.HelpBox("This deck contains new unsaved cards (shown as [NEW]). Save before recompiling scripts — pending cards are lost on domain reload.", MessageType.Warning);
            }
        }

        // ------------------------------------------------------------------
        // Body
        // ------------------------------------------------------------------

        private void DrawBody(Rect body)
        {
            CalculatePaneRects(body, out Rect left, out Rect splitter, out Rect right);
            HandleBodySplitter(body, splitter);
            DrawLeftPane(left);
            DrawVerticalSplitter(splitter);
            DrawRightPane(right);
        }

        // Left pane — staged deck list
        private void DrawLeftPane(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
            Rect inner = Inset(rect, PanePadding);

            int total = _staged?.cards?.Count ?? 0;
            string label = total > 0 ? $"Staged Deck  ({total} cards)" : "Staged Deck  (empty)";
            EditorGUI.LabelField(new Rect(inner.x, inner.y, inner.width, RowHeight), label, EditorStyles.boldLabel);

            float y = inner.y + RowHeight + 4f;

            if (total == 0)
            {
                EditorGUI.HelpBox(new Rect(inner.x, y, inner.width, 40f),
                    "No cards staged. Import JSON or load an existing deck.", MessageType.Info);
                return;
            }

            Rect sr = new Rect(inner.x, y, inner.width, Mathf.Max(40f, inner.yMax - y - 2f));
            float ch = total * (RowHeight + 2f);
            Rect vr = new Rect(0, 0, sr.width - 16f, Mathf.Max(ch, sr.height));

            _leftScroll = GUI.BeginScrollView(sr, _leftScroll, vr);
            float ry = 0f;
            for (int i = 0; i < _staged.cards.Count; i++)
            {
                DrawStagedCardRow(0f, ry, vr.width, i, _staged.cards[i]);
                ry += RowHeight + 2f;
            }
            GUI.EndScrollView();
        }

        private void DrawStagedCardRow(float x, float y, float width, int index, StagedCardEntry entry)
        {
            if (index % 2 == 0)
                EditorGUI.DrawRect(new Rect(x, y, width, RowHeight), new Color(0f, 0f, 0f, 0.05f));

            float removeW = 55f, pingW = 40f;
            bool isNew = entry?.IsNew ?? false;
            float badgeW = isNew ? 40f : 0f;
            float labelW = Mathf.Max(10f, width - removeW - pingW - badgeW - 10f);

            Rect lR = new Rect(x + 4f, y + 1f, labelW, RowHeight - 2f);
            Rect bR = new Rect(lR.xMax + 2f, y + 1f, badgeW, RowHeight - 2f);
            Rect pR = new Rect(bR.xMax + 2f, y + 1f, pingW, RowHeight - 2f);
            Rect rR = new Rect(pR.xMax + 2f, y + 1f, removeW, RowHeight - 2f);

            var card = entry?.ResolvedCard;
            if (card == null)
            {
                EditorGUI.LabelField(lR, "<invalid entry>", EditorStyles.miniLabel);
            }
            else
            {
                string domain = card.IsAction ? "A" : card.IsComposition ? "C" : "?";
                EditorGUI.LabelField(lR, $"[{domain}] {card.Id}  —  {card.DisplayName}", EditorStyles.miniLabel);

                if (isNew)
                {
                    // Highlight the [NEW] badge
                    EditorGUI.DrawRect(bR, new Color(0.2f, 0.6f, 0.2f, 0.25f));
                    EditorGUI.LabelField(bR, "[NEW]", EditorStyles.centeredGreyMiniLabel);
                }

                if (GUI.Button(pR, "Ping", EditorStyles.miniButton))
                    EditorGUIUtility.PingObject(card);
            }

            if (GUI.Button(rR, "Remove", EditorStyles.miniButton))
            {
                var e = _staged.cards[index];
                if (e.IsNew)
                {
                    if (e.pendingCard != null) UnityEngine.Object.DestroyImmediate(e.pendingCard);
                    if (e.pendingPayload != null) UnityEngine.Object.DestroyImmediate(e.pendingPayload);
                }
                _staged.cards.RemoveAt(index);
                _staged.isDirty = true;
                RefreshValidation();
                Repaint();
            }
        }

        // Right pane — metadata + JSON import
        private void DrawRightPane(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
            Rect inner = Inset(rect, PanePadding);

            float ch = EstimateRightPaneHeight(inner.width);
            Rect vr = new Rect(0, 0, inner.width - 16f, Mathf.Max(ch, inner.height));

            _rightScroll = GUI.BeginScrollView(inner, _rightScroll, vr);

            float w = vr.width, y = 0f;
            if (_staged == null) _staged = new StagedDeck();

            y = DrawSectionHeader(w, y, "Deck Metadata");
            y = DrawLabeledField(w, y, "Deck Id", ref _staged.deckId, out bool a);
            y = DrawLabeledField(w, y, "Display Name", ref _staged.displayName, out bool b);
            y = DrawTextAreaField(w, y, "Description", ref _staged.description, out bool c);
            if (a || b || c) { _staged.isDirty = true; RefreshValidation(); }
            y += 6f;

            y = DrawValidationResults(w, y);
            y += 4f;

            if (!string.IsNullOrEmpty(_importErrors))
            {
                float h = CalcHelpBoxHeight(_importErrors, w);
                EditorGUI.HelpBox(new Rect(0, y, w, h), _importErrors, MessageType.Error);
                y += h + 4f;
            }
            if (!string.IsNullOrEmpty(_importWarnings))
            {
                float h = CalcHelpBoxHeight(_importWarnings, w);
                EditorGUI.HelpBox(new Rect(0, y, w, h), _importWarnings, MessageType.Warning);
                y += h + 4f;
            }

            y = DrawSectionHeader(w, y, "JSON Import");

            // Show registries warning if needed for status effects
            if (_registries == null)
            {
                float warnH = 32f;
                EditorGUI.HelpBox(new Rect(0, y, w, warnH),
                    "Registries not found. Cards with ApplyStatusEffect will fail to import.", MessageType.Warning);
                y += warnH + 2f;
            }

            Rect jsonRect = new Rect(0f, y, w, 120f);
            _jsonText = EditorGUI.TextArea(jsonRect, _jsonText);
            y = jsonRect.yMax + 4f;

            if (GUI.Button(new Rect(0f, y, 75f, RowHeight), "Import")) { DoImportJson(); GUIUtility.ExitGUI(); }
            if (GUI.Button(new Rect(81f, y, 55f, RowHeight), "Clear")) { _jsonText = ""; _importErrors = ""; _importWarnings = ""; Repaint(); }
            if (GUI.Button(new Rect(142f, y, 55f, RowHeight), "Paste")) { _jsonText = GUIUtility.systemCopyBuffer; Repaint(); }

            GUI.EndScrollView();
        }

        private float EstimateRightPaneHeight(float w)
        {
            float h = RowHeight + RowHeight + 2f + RowHeight + 2f + 56f + 6f;
            if (_validation != null)
            {
                foreach (var e in _validation.Errors) h += CalcHelpBoxHeight(e, w) + 2f;
                foreach (var e in _validation.Warnings) h += CalcHelpBoxHeight(e, w) + 2f;
            }
            if (!string.IsNullOrEmpty(_importErrors)) h += CalcHelpBoxHeight(_importErrors, w) + 4f;
            if (!string.IsNullOrEmpty(_importWarnings)) h += CalcHelpBoxHeight(_importWarnings, w) + 4f;
            if (_registries == null) h += 36f;
            h += RowHeight + 120f + 4f + RowHeight + 8f;
            return h + 40f;
        }

        // ------------------------------------------------------------------
        // Catalogue strip — full width
        // ------------------------------------------------------------------

        private void DrawCatalogueStrip(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
            Rect inner = Inset(rect, PanePadding);

            EditorGUI.LabelField(
                new Rect(inner.x, inner.y, inner.width, RowHeight),
                $"Card Catalogue  ({_filteredCatalogueCards.Count} / {_allCatalogueCards.Count})",
                EditorStyles.boldLabel);

            float y = inner.y + RowHeight + 2f;

            Rect fr = new Rect(inner.x, y, inner.width, RowHeight);
            float sw = fr.width * 0.45f;

            EditorGUI.BeginChangeCheck();
            _catalogueSearch = _searchField.OnGUI(new Rect(fr.x, fr.y, sw, fr.height), _catalogueSearch);
            _filterAction = EditorGUI.ToggleLeft(new Rect(fr.x + sw + 6f, fr.y, 60f, fr.height), "Action", _filterAction);
            _filterComposition = EditorGUI.ToggleLeft(new Rect(fr.x + sw + 72f, fr.y, 90f, fr.height), "Composition", _filterComposition);
            if (EditorGUI.EndChangeCheck() || _catalogueDirty) { ApplyCatalogueFilter(); _catalogueDirty = false; }

            if (GUI.Button(new Rect(fr.xMax - 65f, fr.y, 65f, fr.height - 2f), "Refresh", EditorStyles.miniButton))
                RefreshCatalogue();

            y = fr.yMax + 2f;

            Rect lr = new Rect(inner.x, y, inner.width, Mathf.Max(30f, inner.yMax - y - 2f));
            float rh = _filteredCatalogueCards.Count * (RowHeight + 2f);
            Rect vr = new Rect(0, 0, lr.width - 16f, Mathf.Max(rh, lr.height));

            _catalogueScroll = GUI.BeginScrollView(lr, _catalogueScroll, vr);
            float ry = 0f;
            for (int i = 0; i < _filteredCatalogueCards.Count; i++)
            {
                var card = _filteredCatalogueCards[i];
                if (card == null) continue;

                if (i % 2 == 0)
                    EditorGUI.DrawRect(new Rect(0, ry, vr.width, RowHeight), new Color(0f, 0f, 0f, 0.04f));

                bool alreadyIn = _staged?.cards != null &&
                                 _staged.cards.Exists(e => e?.ResolvedCard == card);

                float addW = 40f, pingW = 40f;
                float lw = Mathf.Max(10f, vr.width - addW - pingW - 10f);

                Rect lR = new Rect(4f, ry + 1f, lw, RowHeight - 2f);
                Rect pR = new Rect(lR.xMax + 2f, ry + 1f, pingW, RowHeight - 2f);
                Rect aR = new Rect(pR.xMax + 2f, ry + 1f, addW, RowHeight - 2f);

                string domain = card.IsAction ? "A" : card.IsComposition ? "C" : "?";
                EditorGUI.LabelField(lR, $"[{domain}]  {card.Id}  —  {card.DisplayName}", EditorStyles.miniLabel);

                if (GUI.Button(pR, "Ping", EditorStyles.miniButton)) EditorGUIUtility.PingObject(card);

                using (new EditorGUI.DisabledScope(alreadyIn))
                {
                    if (GUI.Button(aR, alreadyIn ? "✓" : "Add", EditorStyles.miniButton))
                    {
                        if (_staged == null) _staged = new StagedDeck();
                        if (_staged.cards == null) _staged.cards = new List<StagedCardEntry>();
                        if (!_staged.cards.Exists(e => e?.ResolvedCard == card))
                        {
                            _staged.cards.Add(StagedCardEntry.FromExisting(card));
                            _staged.isDirty = true;
                            RefreshValidation();
                            Repaint();
                        }
                    }
                }

                ry += RowHeight + 2f;
            }
            GUI.EndScrollView();
        }

        // ------------------------------------------------------------------
        // Catalogue splitter
        // ------------------------------------------------------------------

        private void HandleCatalogueSplitter(Rect full, Rect sr)
        {
            Event e = Event.current;
            Rect hot = new Rect(sr.x, sr.y - 3f, sr.width, sr.height + 6f);
            EditorGUIUtility.AddCursorRect(hot, MouseCursor.ResizeVertical);
            switch (e.type)
            {
                case EventType.MouseDown: if (e.button == 0 && hot.Contains(e.mousePosition)) { _isDraggingCatSplitter = true; e.Use(); } break;
                case EventType.MouseDrag:
                    if (_isDraggingCatSplitter)
                    {
                        float nh = (full.yMax - StatusBarHeight - SplitterWidth) - e.mousePosition.y;
                        _catalogueHeight = Mathf.Clamp(nh, CatalogueMinHeight, CatalogueMaxHeight);
                        Repaint(); e.Use();
                    }
                    break;
                case EventType.MouseUp: if (_isDraggingCatSplitter) { _isDraggingCatSplitter = false; e.Use(); } break;
            }
        }

        private static void DrawHorizontalSplitter(Rect r)
        {
            EditorGUI.DrawRect(r, EditorGUIUtility.isProSkin
                ? new Color(0.25f, 0.25f, 0.25f, 1f)
                : new Color(0.60f, 0.60f, 0.60f, 1f));
        }

        // ------------------------------------------------------------------
        // Validation / status bar
        // ------------------------------------------------------------------

        private float DrawValidationResults(float w, float y)
        {
            if (_validation == null) return y;
            foreach (var e in _validation.Errors) { float h = CalcHelpBoxHeight(e, w); EditorGUI.HelpBox(new Rect(0, y, w, h), e, MessageType.Error); y += h + 2f; }
            foreach (var e in _validation.Warnings) { float h = CalcHelpBoxHeight(e, w); EditorGUI.HelpBox(new Rect(0, y, w, h), e, MessageType.Warning); y += h + 2f; }
            return y;
        }

        private void DrawStatusBar(Rect r)
        {
            EditorGUI.HelpBox(r,
                string.IsNullOrEmpty(_statusMessage) ? "Ready." : _statusMessage,
                _statusIsError ? MessageType.Error : MessageType.Info);
        }

        // ------------------------------------------------------------------
        // Body splitter
        // ------------------------------------------------------------------

        private void CalculatePaneRects(Rect body, out Rect left, out Rect splitter, out Rect right)
        {
            float total = Mathf.Max(1f, body.width);
            float eL = Mathf.Min(MinLeftPaneWidth, Mathf.Max(180f, total * 0.35f));
            float eR = Mathf.Min(MinRightPaneWidth, Mathf.Max(180f, total * 0.30f));
            float mL = total - eR - SplitterWidth;
            if (mL < eL) { eL = Mathf.Max(140f, total * 0.5f - SplitterWidth * 0.5f); eR = Mathf.Max(140f, total - eL - SplitterWidth); mL = Mathf.Max(eL, total - eR - SplitterWidth); }
            float lw = Mathf.Clamp(total * _splitRatio, eL, mL);
            float rw = Mathf.Max(0f, total - lw - SplitterWidth);
            left = new Rect(body.x, body.y, lw, body.height);
            splitter = new Rect(left.xMax, body.y, SplitterWidth, body.height);
            right = new Rect(splitter.xMax, body.y, rw, body.height);
        }

        private void HandleBodySplitter(Rect body, Rect splitter)
        {
            Event e = Event.current;
            Rect hot = new Rect(splitter.x - 3f, splitter.y, splitter.width + 6f, splitter.height);
            EditorGUIUtility.AddCursorRect(hot, MouseCursor.ResizeHorizontal);
            switch (e.type)
            {
                case EventType.MouseDown: if (e.button == 0 && hot.Contains(e.mousePosition)) { _isDraggingBodySplitter = true; e.Use(); } break;
                case EventType.MouseDrag:
                    if (_isDraggingBodySplitter)
                    {
                        float t = Mathf.Max(1f, body.width);
                        float eL = Mathf.Min(MinLeftPaneWidth, Mathf.Max(180f, t * 0.35f));
                        float eR = Mathf.Min(MinRightPaneWidth, Mathf.Max(180f, t * 0.30f));
                        _splitRatio = Mathf.Clamp01(Mathf.Clamp(e.mousePosition.x - body.x, eL, Mathf.Max(eL, t - eR - SplitterWidth)) / t);
                        Repaint(); e.Use();
                    }
                    break;
                case EventType.MouseUp: if (_isDraggingBodySplitter) { _isDraggingBodySplitter = false; e.Use(); } break;
            }
        }

        private static void DrawVerticalSplitter(Rect r) =>
            EditorGUI.DrawRect(r, EditorGUIUtility.isProSkin ? new Color(0.30f, 0.30f, 0.30f) : new Color(0.65f, 0.65f, 0.65f));

        // ------------------------------------------------------------------
        // Operations
        // ------------------------------------------------------------------

        private void DoLoadDeck()
        {
            if (_targetDeckAsset == null) { SetStatus("No deck asset assigned.", true); return; }

            var so = new SerializedObject(_targetDeckAsset);
            so.Update();

            DisposePendingCards(); // clean up any in-memory staged objects

            _staged = new StagedDeck();
            _staged.sourceAsset = _targetDeckAsset;
            _staged.deckId = so.FindProperty("deckId")?.stringValue ?? "";
            _staged.displayName = so.FindProperty("displayName")?.stringValue ?? "";
            _staged.description = so.FindProperty("description")?.stringValue ?? "";
            _staged.isDirty = false;

            if (_targetDeckAsset.Cards != null)
                foreach (var c in _targetDeckAsset.Cards)
                    if (c != null) _staged.cards.Add(StagedCardEntry.FromExisting(c));

            _importErrors = _importWarnings = "";
            RefreshValidation();
            SetStatus($"Loaded '{_targetDeckAsset.name}'  ({_staged.cards.Count} cards).");
        }

        private void DoClearAll()
        {
            if (_staged != null && _staged.HasPendingNewCards)
            {
                bool ok = EditorUtility.DisplayDialog(
                    "Clear All",
                    "This deck has unsaved new cards that will be permanently lost.\n\nClear everything?",
                    "Clear", "Cancel");
                if (!ok) return;
            }

            DisposePendingCards();
            _staged = new StagedDeck();
            _targetDeckAsset = null;
            _jsonText = "";
            _importErrors = "";
            _importWarnings = "";
            RefreshValidation();
            SetStatus("Cleared.");
        }

        private void DoImportJson()
        {
            _importErrors = _importWarnings = "";

            if (string.IsNullOrWhiteSpace(_jsonText))
            {
                _importErrors = "JSON text is empty.";
                SetStatus("Import failed: empty JSON.", true);
                return;
            }

            ResolveRegistries();
            var catalogue = _registries?.StatusCatalogue;

            var result = DeckJsonImportService.Import(_jsonText, catalogue);

            if (result.HasErrors) _importErrors = string.Join("\n", result.Errors);
            if (result.HasWarnings) _importWarnings = string.Join("\n", result.Warnings);

            if (result.Status == ImportResultStatus.Failed)
            {
                SetStatus("Import failed. See errors in right pane.", true);
                return;
            }

            // Preserve source asset link. Card list is fully replaced.
            DisposePendingCards();
            BandDeckData existingSource = _staged?.sourceAsset;
            _staged = result.StagedDeck;
            _staged.sourceAsset = existingSource;
            _staged.isDirty = true;

            RefreshValidation();

            int newCount = 0;
            foreach (var e in _staged.cards) if (e?.IsNew == true) newCount++;

            string statusMsg = $"Imported {_staged.cards.Count} card(s)";
            if (newCount > 0) statusMsg += $" ({newCount} new to be created on save)";
            if (result.HasWarnings) statusMsg += $" with {result.Warnings.Count} warning(s)";
            statusMsg += ".";
            SetStatus(statusMsg);
        }

        private void DoSave()
        {
            if (!CanSave()) return;
            HandleSaveResult(DeckAssetSaveService.Save(_staged));
        }

        private void DoSaveAs()
        {
            var v = DeckValidationService.Validate(_staged);
            if (!v.IsValid)
            {
                bool ok = EditorUtility.DisplayDialog("Save As — Validation Issues",
                    $"Validation errors:\n\n{string.Join("\n", v.Errors)}\n\nSave anyway?",
                    "Save Anyway", "Cancel");
                if (!ok) return;
            }

            // Pass the source asset's folder if known; otherwise null so the service
            // auto-discovers the deck folder via AssetDatabase.FindAssets("t:BandDeckData").
            string folder = _staged?.sourceAsset != null
                ? System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(_staged.sourceAsset))?.Replace('\\', '/')
                : null;

            var result = DeckAssetSaveService.SaveAs(_staged, folder ?? "Assets");
            if (result == null) { SetStatus("Save As cancelled."); return; }

            HandleSaveResult(result);
            if (result.Succeeded) _targetDeckAsset = result.SavedAsset;
        }

        private void HandleSaveResult(SaveResult r)
        {
            if (r == null) return;
            SetStatus(r.Succeeded
                ? $"Saved '{r.SavedAsset.name}' successfully."
                : $"Save failed: {r.Error}",
                isError: !r.Succeeded);
            RefreshValidation();
            Repaint();
        }

        private void DoExportJson()
        {
            string json = DeckJsonImportService.Export(_staged);
            _jsonText = json;
            GUIUtility.systemCopyBuffer = json;
            SetStatus("JSON exported to import field and copied to clipboard.");
        }

        private void DoAddToGigSetup()
        {
            if (_gigSetupConfig == null || _staged?.sourceAsset == null) return;
            bool added = DeckAssetSaveService.AddToGigSetupConfig(_gigSetupConfig, _staged.sourceAsset);
            SetStatus(added
                ? $"'{_staged.sourceAsset.name}' added to '{_gigSetupConfig.name}'."
                : $"'{_staged.sourceAsset.name}' is already in '{_gigSetupConfig.name}'.");
        }

        private void DoRemoveFromGigSetup()
        {
            if (_gigSetupConfig == null || _staged?.sourceAsset == null) return;
            bool removed = DeckAssetSaveService.RemoveFromGigSetupConfig(_gigSetupConfig, _staged.sourceAsset);
            SetStatus(removed
                ? $"'{_staged.sourceAsset.name}' removed from '{_gigSetupConfig.name}'."
                : $"'{_staged.sourceAsset.name}' was not found in '{_gigSetupConfig.name}'.");
        }

        // Destroy all in-memory staged card/payload objects to prevent Unity memory leaks
        private void DisposePendingCards()
        {
            if (_staged?.cards == null) return;
            foreach (var e in _staged.cards)
            {
                if (e == null || !e.IsNew) continue;
                if (e.pendingCard != null) UnityEngine.Object.DestroyImmediate(e.pendingCard);
                if (e.pendingPayload != null) UnityEngine.Object.DestroyImmediate(e.pendingPayload);
            }
        }

        // ------------------------------------------------------------------
        // Catalogue
        // ------------------------------------------------------------------

        private void RefreshCatalogue()
        {
            _allCatalogueCards.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:CardDefinition"))
            {
                var c = AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (c != null) _allCatalogueCards.Add(c);
            }
            _allCatalogueCards.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.OrdinalIgnoreCase));
            ApplyCatalogueFilter();
            _catalogueDirty = false;
        }

        private void ApplyCatalogueFilter()
        {
            _filteredCatalogueCards.Clear();
            string s = _catalogueSearch?.Trim().ToLowerInvariant() ?? "";
            foreach (var c in _allCatalogueCards)
            {
                if (c == null) continue;
                if (!_filterAction && c.IsAction) continue;
                if (!_filterComposition && c.IsComposition) continue;
                if (!string.IsNullOrEmpty(s) &&
                    c.Id?.ToLowerInvariant().Contains(s) != true &&
                    c.DisplayName?.ToLowerInvariant().Contains(s) != true)
                    continue;
                _filteredCatalogueCards.Add(c);
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private void RefreshValidation() => _validation = DeckValidationService.Validate(_staged);

        private bool CanSave() =>
            _staged != null && _staged.sourceAsset != null && _staged.isDirty &&
            (_validation == null || _validation.IsValid);

        private void SetStatus(string msg, bool isError = false)
        {
            _statusMessage = msg; _statusIsError = isError; Repaint();
        }

        private static Rect Inset(Rect r, float p) =>
            new Rect(r.x + p, r.y + p, Mathf.Max(0f, r.width - p * 2f), Mathf.Max(0f, r.height - p * 2f));

        private static float DrawSectionHeader(float w, float y, string title)
        {
            EditorGUI.LabelField(new Rect(0, y, w, RowHeight), title, EditorStyles.boldLabel);
            return y + RowHeight + 2f;
        }

        private static float DrawLabeledField(float w, float y, string label, ref string v, out bool changed)
        {
            EditorGUI.BeginChangeCheck();
            v = EditorGUI.TextField(new Rect(0, y, w, RowHeight), label, v ?? "");
            changed = EditorGUI.EndChangeCheck();
            return y + RowHeight + 2f;
        }

        private static float DrawTextAreaField(float w, float y, string label, ref string v, out bool changed)
        {
            EditorGUI.LabelField(new Rect(0, y, w, RowHeight), label, EditorStyles.miniLabel);
            y += RowHeight;
            EditorGUI.BeginChangeCheck();
            v = EditorGUI.TextArea(new Rect(0, y, w, 54f), v ?? "");
            changed = EditorGUI.EndChangeCheck();
            return y + 54f + 2f;
        }

        private static float CalcHelpBoxHeight(string msg, float w) =>
            Mathf.Max(36f, 20f + (string.IsNullOrEmpty(msg) ? 1 : msg.Split('\n').Length) * 16f);
    }
}
#endif
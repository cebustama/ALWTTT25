// AMW-1 (2026-09-05) — Audience Member Wizard.
// Repo path: Assets/Scripts/Characters/Editor/AudienceMemberWizard.cs
// Editor-only. No runtime type is modified (D-AMW-0).

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ALWTTT.Data;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Characters.Editor
{
    /// <summary>
    /// Inventory + inline editor + creator for <see cref="AudienceCharacterData"/> assets.
    ///
    /// - Layout: list + draggable splitter + edit panel (PartEffectEditorWindow precedent, D-AMW-2=A).
    /// - Editing: SerializedObject/SerializedProperty only — Undo and dirty come for free and the
    ///   runtime surface of AudienceCharacterData stays untouched (D-AMW-EDIT).
    /// - Usages: cached per Refresh via AssetDatabase.GetDependencies over every GigEncounterSO and
    ///   GigSetupRosterSO asset; no field names of those types are assumed (D-AMW-3=B).
    /// - Not part of CardAuthoringNav (D-AMW-4=A). Never simulates reactions (D-AMW-SIM).
    /// - Foreign SO types (AudienceIntentionData, StatusEffectSO, CharacterSfxProfileSO) are
    ///   referenced + pinged only. Inline data (abilities, actions, animation, taste) is edited,
    ///   because it lives inside the enemy's own .asset.
    /// </summary>
    public sealed class AudienceMemberWizard : EditorWindow
    {
        private const string DefaultCreateFolder = "Assets/Resources/Data/Characters/Audience";
        private const string LogTag = "[AudienceMemberWizard]";

        private const float LeftPanelMinWidth = 560f;
        private const float RightPanelMinWidth = 400f;
        private const float SplitterWidth = 4f;

        private static readonly string[] UsageSourceTypes = { "GigEncounterSO", "GigSetupRosterSO" };

        private static readonly string[] TasteFilterOptions = { "Taste: all", "Taste: configured", "Taste: neutral" };
        private static readonly string[] PrefabFilterOptions = { "Prefab: all", "Prefab: set", "Prefab: missing" };
        private static readonly string[] TallFilterOptions = { "Tall: all", "Tall: yes", "Tall: no" };

        // Badges (spec names in Spanish → UI in English, same as every other ALWTTT window)
        private const string BadgeNoAbilities = "NO ABILITIES";          // SIN HABILIDADES
        private const string BadgeNoPrefab = "NO PREFAB";                // SIN PREFAB
        private const string BadgeVibe = "MAXVIBE ≤ 0";                  // maxVibe ≤ 0
        private const string BadgeNeutral = "NEUTRAL TASTE";             // GUSTOS NEUTROS (info)
        private const string BadgePattern1 = "PATTERN WITH 1 ABILITY";   // PATRÓN CON 1 HABILIDAD
        private const string BadgeDupName = "DUPLICATE NAME";            // NOMBRE DUPLICADO

        // ──────────────────────────────────────────────────────────────────
        // Persistent window state
        // ──────────────────────────────────────────────────────────────────
        [SerializeField] private string _search = "";
        [SerializeField] private int _tasteFilter;
        [SerializeField] private int _prefabFilter;
        [SerializeField] private int _tallFilter;
        [SerializeField] private bool _vibeRangeOn;
        [SerializeField] private int _vibeMin;
        [SerializeField] private int _vibeMax = 999;

        [SerializeField] private Vector2 _leftScroll;
        [SerializeField] private Vector2 _rightScroll;
        [SerializeField, Range(0.2f, 0.8f)] private float _splitRatio = 0.55f;
        private bool _draggingSplitter;

        [SerializeField] private AudienceCharacterData _selected;

        [SerializeField] private bool _createOpen;
        [SerializeField] private string _createName = "";
        [SerializeField] private string _createFolder = DefaultCreateFolder;
        [SerializeField] private int _createTemplateIndex; // 0 = empty asset

        // ──────────────────────────────────────────────────────────────────
        // Caches
        // ──────────────────────────────────────────────────────────────────
        private sealed class Row
        {
            public AudienceCharacterData Asset;
            public string Path;
            public string Id;
            public string Name;
            public string Description;
            public int MaxVibe;
            public int AbilityCount;
            public bool FollowPattern;
            public bool IsTall;
            public bool HasPrefab;
            public bool HasSfx;
            public bool TasteNeutral;
            public string TasteSummary;
            public readonly List<string> Badges = new();
            public readonly List<UnityEngine.Object> UsedBy = new();
        }

        private readonly List<Row> _rows = new();
        private readonly Dictionary<AudienceCharacterData, Row> _rowByAsset = new();
        private readonly HashSet<string> _missingFields = new();
        private int _usageSourcesScanned;
        private string[] _templateOptions = { "(empty)" };

        private SerializedObject _serialized;

        private GUIStyle _badgeErrorStyle;
        private GUIStyle _badgeInfoStyle;
        private GUIStyle _cellLeft, _cellRight, _cellCenter, _cellMuted, _cellMutedCenter;
        private GUIStyle _headLeft, _headRight, _headCenter;
        private GUIStyle _chipError, _chipInfo;

        // ──────────────────────────────────────────────────────────────────
        // Entry points
        // ──────────────────────────────────────────────────────────────────
        [MenuItem("ALWTTT/Characters/Audience Member Wizard")]
        public static void Open()
        {
            var w = GetWindow<AudienceMemberWizard>();
            w.titleContent = new GUIContent("Audience Member Wizard");
            w.minSize = new Vector2(LeftPanelMinWidth + RightPanelMinWidth + SplitterWidth, 520f);
            w.Show();
        }

        /// <summary>Cross-link entry point: open and select a specific asset, pinging it.</summary>
        public static void OpenAndSelect(AudienceCharacterData data)
        {
            Open();
            var w = GetWindow<AudienceMemberWizard>();
            if (data == null) return;

            w._selected = data;
            Selection.activeObject = data;
            EditorGUIUtility.PingObject(data);
            w.Repaint();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            RefreshAssets();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            DisposeSerialized();
        }

        private void OnProjectChange()
        {
            RefreshAssets();
            Repaint();
        }

        private void OnUndoRedo()
        {
            RebuildRows();
            Repaint();
        }

        // ──────────────────────────────────────────────────────────────────
        // GUI root
        // ──────────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            EnsureStyles();
            DrawToolbar();

            if (_missingFields.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    "AudienceCharacterData no longer exposes these serialized fields: " +
                    string.Join(", ", _missingFields) +
                    ". The wizard is stale against the runtime type — update AudienceMemberWizard.cs.",
                    MessageType.Error);
            }

            float totalW = position.width;
            float leftW = Mathf.Clamp(
                totalW * _splitRatio,
                LeftPanelMinWidth,
                Mathf.Max(LeftPanelMinWidth, totalW - RightPanelMinWidth - SplitterWidth));
            float rightW = Mathf.Max(RightPanelMinWidth, totalW - leftW - SplitterWidth);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPanel(leftW);
                DrawSplitter(totalW);
                DrawRightPanel(rightW);
            }
        }

        private void EnsureStyles()
        {
            if (_badgeErrorStyle != null) return;

            _badgeErrorStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            _badgeErrorStyle.normal.textColor = ErrorColor;

            _badgeInfoStyle = new GUIStyle(EditorStyles.miniLabel);
            _badgeInfoStyle.normal.textColor = MutedColor;

            _cellLeft = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip };
            _cellRight = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight, clipping = TextClipping.Clip };
            _cellCenter = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip };

            _cellMuted = new GUIStyle(_cellLeft);
            _cellMuted.normal.textColor = MutedColor;
            _cellMutedCenter = new GUIStyle(_cellCenter);
            _cellMutedCenter.normal.textColor = MutedColor;

            _headLeft = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleLeft };
            _headRight = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleRight };
            _headCenter = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter };

            _chipError = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(5, 5, 0, 0),
                fontSize = 9
            };
            _chipError.normal.textColor = ErrorColor;

            _chipInfo = new GUIStyle(_chipError);
            _chipInfo.normal.textColor = MutedColor;
        }

        // ──────────────────────────────────────────────────────────────────
        // Column grid — rect-based so every row lands on the same x positions
        // ──────────────────────────────────────────────────────────────────
        private struct Cols
        {
            public Rect Name, Vibe, Abil, Pattern, Tall, Taste, Prefab, Sfx, Uses;
        }

        private const float CellPad = 4f;
        private const float RowHeight = 20f;
        private const float ChipLineHeight = 15f;

        private static readonly Color ErrorColor = new Color(0.95f, 0.45f, 0.35f);
        private static readonly Color MutedColor = new Color(0.60f, 0.60f, 0.60f);

        private static Cols BuildCols(Rect line)
        {
            const float wVibe = 46f, wAbil = 40f, wPattern = 58f, wTall = 34f,
                        wPrefab = 30f, wSfx = 30f, wUses = 40f;

            float fixedTotal = wVibe + wAbil + wPattern + wTall + wPrefab + wSfx + wUses + CellPad * 8f;
            float flex = Mathf.Max(180f, line.width - fixedTotal);
            float wName = Mathf.Round(flex * 0.45f);
            float wTaste = flex - wName;

            float x = line.x;
            Rect Next(float w) { var r = new Rect(x, line.y, w, line.height); x += w + CellPad; return r; }

            return new Cols
            {
                Name = Next(wName),
                Vibe = Next(wVibe),
                Abil = Next(wAbil),
                Pattern = Next(wPattern),
                Tall = Next(wTall),
                Taste = Next(wTaste),
                Prefab = Next(wPrefab),
                Sfx = Next(wSfx),
                Uses = Next(wUses)
            };
        }

        // ──────────────────────────────────────────────────────────────────
        // Splitter (verbatim pattern from PartEffectEditorWindow)
        // ──────────────────────────────────────────────────────────────────
        private void DrawSplitter(float totalW)
        {
            var rect = GUILayoutUtility.GetRect(
                SplitterWidth, SplitterWidth,
                GUILayout.ExpandHeight(true), GUILayout.Width(SplitterWidth));

            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.25f));
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);

            var e = Event.current;
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                _draggingSplitter = true;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && _draggingSplitter)
            {
                _draggingSplitter = false;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _draggingSplitter)
            {
                if (totalW > 1f)
                {
                    _splitRatio = Mathf.Clamp(e.mousePosition.x / totalW, 0.2f, 0.8f);
                    Repaint();
                }
                e.Use();
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Toolbar — search + filters + actions
        // ──────────────────────────────────────────────────────────────────
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Search:", GUILayout.Width(48));
                _search = EditorGUILayout.TextField(
                    _search, EditorStyles.toolbarSearchField,
                    GUILayout.MinWidth(120), GUILayout.MaxWidth(240));

                GUILayout.Space(6);
                _tasteFilter = EditorGUILayout.Popup(_tasteFilter, TasteFilterOptions,
                    EditorStyles.toolbarPopup, GUILayout.Width(120));
                _prefabFilter = EditorGUILayout.Popup(_prefabFilter, PrefabFilterOptions,
                    EditorStyles.toolbarPopup, GUILayout.Width(112));
                _tallFilter = EditorGUILayout.Popup(_tallFilter, TallFilterOptions,
                    EditorStyles.toolbarPopup, GUILayout.Width(84));

                GUILayout.Space(6);
                _vibeRangeOn = GUILayout.Toggle(_vibeRangeOn, "MaxVibe",
                    EditorStyles.toolbarButton, GUILayout.Width(64));
                using (new EditorGUI.DisabledScope(!_vibeRangeOn))
                {
                    _vibeMin = EditorGUILayout.IntField(_vibeMin, GUILayout.Width(40));
                    GUILayout.Label("–", GUILayout.Width(10));
                    _vibeMax = EditorGUILayout.IntField(_vibeMax, GUILayout.Width(40));
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("New…", EditorStyles.toolbarButton, GUILayout.Width(56)))
                    _createOpen = !_createOpen;

                if (GUILayout.Button("Export JSON", EditorStyles.toolbarButton, GUILayout.Width(96)))
                    ExportFilteredJson();

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(64)))
                    RefreshAssets();
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Left panel — inventory list + create box
        // ──────────────────────────────────────────────────────────────────
        private void DrawLeftPanel(float width)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(width), GUILayout.ExpandHeight(true)))
            {
                if (_createOpen)
                    DrawCreateBox();

                var filtered = GetFilteredRows();

                string countText = filtered.Count == _rows.Count
                    ? $"AudienceCharacterData assets: {_rows.Count}"
                    : $"AudienceCharacterData assets: {filtered.Count} / {_rows.Count}";
                EditorGUILayout.LabelField(
                    new GUIContent($"{countText}   ·   usage sources: {_usageSourcesScanned}",
                        "Usage sources = GigEncounterSO + GigSetupRosterSO assets scanned for references."),
                    EditorStyles.boldLabel);

                if (_usageSourcesScanned == 0)
                {
                    EditorGUILayout.HelpBox(
                        "No GigEncounterSO / GigSetupRosterSO assets found — the Uses column is blind.",
                        MessageType.Warning);
                }

                DrawListHeader();

                using (var s = new EditorGUILayout.ScrollViewScope(_leftScroll))
                {
                    _leftScroll = s.scrollPosition;

                    for (int i = 0; i < filtered.Count; i++)
                        DrawRow(filtered[i], i);

                    if (filtered.Count == 0)
                        EditorGUILayout.HelpBox(
                            "No AudienceCharacterData assets match the current filters.",
                            MessageType.Info);
                }
            }
        }

        private void DrawListHeader()
        {
            Rect line = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
            line = new Rect(line.x + CellPad, line.y, line.width - CellPad * 2f, line.height);
            var c = BuildCols(line);

            EditorGUI.LabelField(c.Name, "Name", _headLeft);
            EditorGUI.LabelField(c.Vibe, new GUIContent("Vibe", "maxVibe"), _headRight);
            EditorGUI.LabelField(c.Abil, new GUIContent("Abil", "number of abilities"), _headRight);
            EditorGUI.LabelField(c.Pattern, new GUIContent("Pattern", "followAbilityPattern: cyclic or random"), _headCenter);
            EditorGUI.LabelField(c.Tall, new GUIContent("Tall", "isTall"), _headCenter);
            EditorGUI.LabelField(c.Taste, new GUIContent("Taste", "active taste axes"), _headLeft);
            EditorGUI.LabelField(c.Prefab, new GUIContent("Pfb", "characterPrefab assigned"), _headCenter);
            EditorGUI.LabelField(c.Sfx, new GUIContent("Sfx", "sfxProfile assigned"), _headCenter);
            EditorGUI.LabelField(c.Uses, new GUIContent("Uses", "referenced by N GigEncounterSO / GigSetupRosterSO assets"), _headRight);

            var rule = new Rect(line.x, line.yMax - 1f, line.width, 1f);
            EditorGUI.DrawRect(rule, new Color(0f, 0f, 0f, 0.35f));
        }

        private void DrawRow(Row row, int index)
        {
            bool isSelected = row.Asset == _selected;
            var chips = VisibleBadges(row);
            bool hasChips = chips.Count > 0;

            float h = RowHeight + (hasChips ? ChipLineHeight : 0f);
            Rect full = GUILayoutUtility.GetRect(0f, h, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                if (isSelected)
                    EditorGUI.DrawRect(full, new Color(0.24f, 0.38f, 0.58f, 0.55f));
                else if ((index & 1) == 1)
                    EditorGUI.DrawRect(full, new Color(1f, 1f, 1f, 0.03f));
            }

            Rect line = new Rect(full.x + CellPad, full.y, full.width - CellPad * 2f, RowHeight);
            var c = BuildCols(line);

            string display = string.IsNullOrEmpty(row.Name) ? row.Asset.name : row.Name;
            EditorGUI.LabelField(c.Name, new GUIContent(display, $"{row.Asset.name}\n{row.Path}"), _cellLeft);
            EditorGUI.LabelField(c.Vibe, row.MaxVibe.ToString(), _cellRight);
            EditorGUI.LabelField(c.Abil, row.AbilityCount.ToString(), _cellRight);
            EditorGUI.LabelField(c.Pattern, row.FollowPattern ? "cyclic" : "random",
                row.FollowPattern ? _cellCenter : _cellMutedCenter);
            EditorGUI.LabelField(c.Tall, row.IsTall ? "✓" : "·", row.IsTall ? _cellCenter : _cellMutedCenter);
            EditorGUI.LabelField(c.Taste, new GUIContent(row.TasteSummary, row.TasteSummary),
                row.TasteNeutral ? _cellMuted : _cellLeft);
            EditorGUI.LabelField(c.Prefab, row.HasPrefab ? "✓" : "·", row.HasPrefab ? _cellCenter : _cellMutedCenter);
            EditorGUI.LabelField(c.Sfx, row.HasSfx ? "✓" : "·", row.HasSfx ? _cellCenter : _cellMutedCenter);
            EditorGUI.LabelField(c.Uses, row.UsedBy.Count.ToString(),
                row.UsedBy.Count > 0 ? _cellRight : _cellMutedCenter);

            if (hasChips)
                DrawChips(new Rect(c.Name.x, line.yMax - 2f, full.xMax - c.Name.x - CellPad, ChipLineHeight), chips);

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && full.Contains(e.mousePosition))
            {
                _selected = row.Asset;
                GUI.FocusControl(null);
                if (e.clickCount == 2) EditorGUIUtility.PingObject(row.Asset);
                e.Use();
                Repaint();
            }
        }

        /// <summary>Row chips carry only what a column cannot already say. NEUTRAL TASTE is
        /// suppressed here because the Taste column literally reads "neutral" (AMW-1b D6=A);
        /// it still appears in the right-hand Validation block.</summary>
        private static List<string> VisibleBadges(Row row)
        {
            var list = new List<string>(row.Badges.Count);
            foreach (var b in row.Badges)
            {
                if (b == BadgeNeutral) continue;
                list.Add(b);
            }
            return list;
        }

        private void DrawChips(Rect area, List<string> chips)
        {
            float x = area.x;
            foreach (var b in chips)
            {
                var style = b == BadgeNeutral ? _chipInfo : _chipError;
                var content = new GUIContent(b, ExplainBadge(b));
                float w = style.CalcSize(content).x;
                if (x + w > area.xMax) return; // out of room; the panel lists them all

                var r = new Rect(x, area.y, w, area.height - 2f);
                if (Event.current.type == EventType.Repaint)
                    EditorGUI.DrawRect(r, b == BadgeNeutral
                        ? new Color(1f, 1f, 1f, 0.05f)
                        : new Color(0.95f, 0.45f, 0.35f, 0.14f));
                EditorGUI.LabelField(r, content, style);
                x += w + 4f;
            }
        }

        private List<Row> GetFilteredRows()
        {
            var result = new List<Row>(_rows.Count);
            string q = (_search ?? "").Trim();
            bool hasQuery = q.Length > 0;

            foreach (var r in _rows)
            {
                if (r.Asset == null) continue;

                if (hasQuery)
                {
                    bool hit =
                        Contains(r.Name, q) || Contains(r.Id, q) ||
                        Contains(r.Description, q) || Contains(r.Asset.name, q);
                    if (!hit) continue;
                }

                if (_tasteFilter == 1 && r.TasteNeutral) continue;
                if (_tasteFilter == 2 && !r.TasteNeutral) continue;

                if (_prefabFilter == 1 && !r.HasPrefab) continue;
                if (_prefabFilter == 2 && r.HasPrefab) continue;

                if (_tallFilter == 1 && !r.IsTall) continue;
                if (_tallFilter == 2 && r.IsTall) continue;

                if (_vibeRangeOn && (r.MaxVibe < _vibeMin || r.MaxVibe > _vibeMax)) continue;

                result.Add(r);
            }
            return result;
        }

        private static bool Contains(string haystack, string needle) =>
            !string.IsNullOrEmpty(haystack) &&
            haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

        // ──────────────────────────────────────────────────────────────────
        // Create box
        // ──────────────────────────────────────────────────────────────────
        private void DrawCreateBox()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Create AudienceCharacterData", EditorStyles.boldLabel);

                _createName = EditorGUILayout.TextField("Asset / Character Name", _createName);
                _createFolder = EditorGUILayout.TextField("Folder", _createFolder);

                _createTemplateIndex = Mathf.Clamp(_createTemplateIndex, 0, _templateOptions.Length - 1);
                _createTemplateIndex = EditorGUILayout.Popup(
                    new GUIContent("Template", "Duplicate an existing enemy as the starting point. (empty) = blank asset."),
                    _createTemplateIndex, _templateOptions);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Reset Folder", GUILayout.Width(96)))
                        _createFolder = DefaultCreateFolder;
                    if (GUILayout.Button("Create", GUILayout.Width(80)))
                        CreateAsset();
                }
            }
            EditorGUILayout.Space(4);
        }

        private void CreateAsset()
        {
            string folder = string.IsNullOrWhiteSpace(_createFolder)
                ? DefaultCreateFolder
                : _createFolder.Trim().Replace("\\", "/").TrimEnd('/');
            EnsureFolderExists(folder);

            string baseName = string.IsNullOrWhiteSpace(_createName)
                ? "New AudienceCharacterData"
                : _createName.Trim();

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{baseName}.asset");

            AudienceCharacterData template = null;
            if (_createTemplateIndex > 0 && _createTemplateIndex - 1 < _rows.Count)
                template = _rows[_createTemplateIndex - 1].Asset;

            if (template != null)
            {
                string src = AssetDatabase.GetAssetPath(template);
                if (!AssetDatabase.CopyAsset(src, path))
                {
                    Debug.LogError($"{LogTag} Duplicate-as-template failed: {src} → {path}");
                    return;
                }
            }
            else
            {
                var instance = CreateInstance<AudienceCharacterData>();
                AssetDatabase.CreateAsset(instance, path);
            }

            AssetDatabase.SaveAssets();
            var created = AssetDatabase.LoadAssetAtPath<AudienceCharacterData>(path);
            if (created == null)
            {
                Debug.LogError($"{LogTag} Created asset could not be loaded: {path}");
                return;
            }

            // Name the character after the asset so a template copy does not
            // immediately trip DUPLICATE NAME. characterId is left untouched.
            using (var so = new SerializedObject(created))
            {
                var nameProp = so.FindProperty("characterName");
                if (nameProp != null)
                {
                    nameProp.stringValue = System.IO.Path.GetFileNameWithoutExtension(path);
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            AssetDatabase.SaveAssets();

            _selected = created;
            Selection.activeObject = created;
            EditorGUIUtility.PingObject(created);
            RefreshAssets();
            Debug.Log($"{LogTag} Created {path}" + (template != null ? $" (from template {template.name})" : ""));
        }

        // ──────────────────────────────────────────────────────────────────
        // Right panel — header, usages, validation, inline editor
        // ──────────────────────────────────────────────────────────────────
        private void DrawRightPanel(float width)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(width), GUILayout.ExpandHeight(true)))
            using (var s = new EditorGUILayout.ScrollViewScope(_rightScroll))
            {
                _rightScroll = s.scrollPosition;

                if (_selected == null)
                {
                    DisposeSerialized();
                    EditorGUILayout.HelpBox("Select an audience member from the list (or create one).", MessageType.Info);
                    return;
                }

                _rowByAsset.TryGetValue(_selected, out var row);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label(_selected.name, EditorStyles.boldLabel);
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.TextField("Path", AssetDatabase.GetAssetPath(_selected));

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Ping")) EditorGUIUtility.PingObject(_selected);
                        if (GUILayout.Button("Duplicate")) DuplicateSelected();
                        if (GUILayout.Button("Delete")) DeleteSelected();
                    }
                }

                if (row != null)
                {
                    DrawUsagesBlock(row);
                    DrawValidationBlock(row);
                }

                EditorGUILayout.Space(6);
                DrawInlineEditor();
            }
        }

        private void DrawUsagesBlock(Row row)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label($"Referenced by {row.UsedBy.Count} encounter/roster asset(s)", EditorStyles.miniBoldLabel);
                foreach (var u in row.UsedBy)
                {
                    if (u == null) continue;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"{u.name}  ({u.GetType().Name})", GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("Ping", GUILayout.Width(44)))
                            EditorGUIUtility.PingObject(u);
                    }
                }
            }
        }

        private void DrawValidationBlock(Row row)
        {
            if (row.Badges.Count == 0) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Validation", EditorStyles.miniBoldLabel);
                foreach (var b in row.Badges)
                    GUILayout.Label($"{b} — {ExplainBadge(b)}", b == BadgeNeutral ? _badgeInfoStyle : _badgeErrorStyle);
            }
        }

        private static string ExplainBadge(string badge) => badge switch
        {
            BadgeNoAbilities => "GetAbility() will LogError at runtime; the enemy does nothing.",
            BadgeNoPrefab => "no characterPrefab assigned (the gig spawns the member from it).",
            BadgeVibe => "Vibe pool starts depleted; Convinced = depleted pool (SSoT_Gig_Encounter §7).",
            BadgeNeutral => "every taste axis contributes 0 to loop impressions (informative, not an error).",
            BadgePattern1 => "cyclic pattern over a single ability is indistinguishable from random.",
            BadgeDupName => "another asset uses the same characterName.",
            _ => ""
        };

        private void DrawInlineEditor()
        {
            EnsureSerialized();
            if (_serialized == null) return;

            _serialized.Update();

            var pId = _serialized.FindProperty("characterId");
            var pName = _serialized.FindProperty("characterName");
            var pDesc = _serialized.FindProperty("characterDescription");
            var pVibe = _serialized.FindProperty("maxVibe");
            var pPrefab = _serialized.FindProperty("characterPrefab");
            var pTall = _serialized.FindProperty("isTall");
            var pAbilities = _serialized.FindProperty("abilityList");
            var pPattern = _serialized.FindProperty("followAbilityPattern");
            var pTaste = _serialized.FindProperty("taste");
            var pSfx = _serialized.FindProperty("sfxProfile");

            GUILayout.Label("Base", EditorStyles.boldLabel);
            Field(pId);
            Field(pName);
            Field(pDesc);

            EditorGUILayout.Space(4);
            GUILayout.Label("Audience", EditorStyles.boldLabel);
            Field(pVibe);
            FieldWithPing(pPrefab);

            EditorGUILayout.Space(4);
            GUILayout.Label("Abilities", EditorStyles.boldLabel);
            Field(pTall, new GUIContent("Is Tall", "TODO Generalize in runtime type (pre-existing); not resolved by AMW-1."));
            Field(pPattern, new GUIContent("Follow Ability Pattern", "true = cyclic (usedAbilityCount % count); false = random."));
            if (pAbilities != null) DrawAbilityList(pAbilities);

            EditorGUILayout.Space(4);
            GUILayout.Label("Taste preferences", EditorStyles.boldLabel);
            if (pTaste != null) DrawTasteBlock(pTaste);

            EditorGUILayout.Space(4);
            GUILayout.Label("Audio", EditorStyles.boldLabel);
            FieldWithPing(pSfx);

            if (_serialized.ApplyModifiedProperties())
                RebuildRows();
        }

        private static void Field(SerializedProperty p, GUIContent label = null)
        {
            if (p == null) return;
            if (label == null) EditorGUILayout.PropertyField(p, true);
            else EditorGUILayout.PropertyField(p, label, true);
        }

        /// <summary>Object reference + Ping. Foreign SO types are referenced here, never authored.</summary>
        private static void FieldWithPing(SerializedProperty p)
        {
            if (p == null) return;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(p, true);
                using (new EditorGUI.DisabledScope(p.objectReferenceValue == null))
                {
                    if (GUILayout.Button("Ping", GUILayout.Width(44)))
                        EditorGUIUtility.PingObject(p.objectReferenceValue);
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Ability list — explicit add / duplicate / delete / move
        // ──────────────────────────────────────────────────────────────────
        private void DrawAbilityList(SerializedProperty list)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label($"Ability list ({list.arraySize})", EditorStyles.miniBoldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ Add", GUILayout.Width(60)))
                {
                    list.arraySize++;
                    ResetAbility(list.GetArrayElementAtIndex(list.arraySize - 1));
                    list.GetArrayElementAtIndex(list.arraySize - 1).isExpanded = true;
                    CommitStructuralChange(list);
                }
            }

            for (int i = 0; i < list.arraySize; i++)
            {
                var el = list.GetArrayElementAtIndex(i);
                var pAbilityName = el.FindPropertyRelative("abilityName");
                string title = pAbilityName != null && !string.IsNullOrEmpty(pAbilityName.stringValue)
                    ? pAbilityName.stringValue
                    : $"Ability {i}";

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        el.isExpanded = EditorGUILayout.Foldout(el.isExpanded, $"[{i}] {title}", true);
                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledScope(i == 0))
                        {
                            if (GUILayout.Button("▲", EditorStyles.miniButtonLeft, GUILayout.Width(24)))
                            { list.MoveArrayElement(i, i - 1); CommitStructuralChange(list); }
                        }
                        using (new EditorGUI.DisabledScope(i == list.arraySize - 1))
                        {
                            if (GUILayout.Button("▼", EditorStyles.miniButtonMid, GUILayout.Width(24)))
                            { list.MoveArrayElement(i, i + 1); CommitStructuralChange(list); }
                        }
                        if (GUILayout.Button("Dup", EditorStyles.miniButtonMid, GUILayout.Width(36)))
                        { list.InsertArrayElementAtIndex(i); CommitStructuralChange(list); } // inserts a copy of [i]
                        if (GUILayout.Button("✕", EditorStyles.miniButtonRight, GUILayout.Width(24)))
                        { list.DeleteArrayElementAtIndex(i); CommitStructuralChange(list); }
                    }

                    if (!el.isExpanded) continue;

                    EditorGUI.indentLevel++;
                    Field(pAbilityName);
                    FieldWithPing(el.FindPropertyRelative("intention"));
                    Field(el.FindPropertyRelative("hideActionValue"));
                    Field(el.FindPropertyRelative("abilityDuration"));

                    var pActions = el.FindPropertyRelative("actionList");
                    Field(pActions); // nested reorderable list; statusEffect refs inside are referenced only
                    if (pActions != null && pActions.arraySize == 0)
                        EditorGUILayout.HelpBox("No actions: this ability plays its animation/SFX and does nothing else.", MessageType.Info);

                    Field(el.FindPropertyRelative("animation"));
                    FieldWithPing(el.FindPropertyRelative("abilitySfx"));
                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>Array size / order changed mid-frame. Commit (records Undo, marks dirty),
        /// refresh derived rows, and abort this OnGUI pass so IMGUI's layout cache is not
        /// compared against a control tree that no longer exists.</summary>
        private void CommitStructuralChange(SerializedProperty list)
        {
            list.serializedObject.ApplyModifiedProperties();
            RebuildRows();
            GUIUtility.ExitGUI();
        }

        /// <summary>A grown list element copies its predecessor; reset it to the type's defaults.</summary>
        private static void ResetAbility(SerializedProperty el)
        {
            SetString(el, "abilityName", "");
            SetObject(el, "intention", null);
            SetBool(el, "hideActionValue", false);
            SetFloat(el, "abilityDuration", 0f);
            var actions = el.FindPropertyRelative("actionList");
            if (actions != null) actions.arraySize = 0;
            var anim = el.FindPropertyRelative("animation");
            if (anim != null)
            {
                SetString(anim, "animatorTrigger", "");
                SetFloat(anim, "animationDuration", -1f);
                SetBool(anim, "disableBeatAnimator", true);
            }
            SetObject(el, "abilitySfx", null);
        }

        private static void SetString(SerializedProperty parent, string name, string v) { var p = parent.FindPropertyRelative(name); if (p != null) p.stringValue = v; }
        private static void SetBool(SerializedProperty parent, string name, bool v) { var p = parent.FindPropertyRelative(name); if (p != null) p.boolValue = v; }
        private static void SetFloat(SerializedProperty parent, string name, float v) { var p = parent.FindPropertyRelative(name); if (p != null) p.floatValue = v; }
        private static void SetObject(SerializedProperty parent, string name, UnityEngine.Object v) { var p = parent.FindPropertyRelative(name); if (p != null) p.objectReferenceValue = v; }

        // ──────────────────────────────────────────────────────────────────
        // Taste block — fields + prose reading (reads fields; never simulates)
        // ──────────────────────────────────────────────────────────────────
        private void DrawTasteBlock(SerializedProperty taste)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Tempo axis", EditorStyles.miniBoldLabel);
                Field(taste.FindPropertyRelative("tempoMatchOnFast"));
                Field(taste.FindPropertyRelative("preferAboveTempoScale"));
                Field(taste.FindPropertyRelative("tempoMismatchOnSlow"));
                Field(taste.FindPropertyRelative("dislikeBelowTempoScale"));

                GUILayout.Label("Density axis", EditorStyles.miniBoldLabel);
                Field(taste.FindPropertyRelative("roleCountMatchOnRich"));
                Field(taste.FindPropertyRelative("preferAtLeastRoles"));

                GUILayout.Label("Meter axis", EditorStyles.miniBoldLabel);
                Field(taste.FindPropertyRelative("preferredTimeSignatures"));
                Field(taste.FindPropertyRelative("dislikedTimeSignatures"));

                GUILayout.Label("Mode axis", EditorStyles.miniBoldLabel);
                Field(taste.FindPropertyRelative("preferredTonalities"));
                Field(taste.FindPropertyRelative("dislikedTonalities"));
            }

            EditorGUILayout.HelpBox(BuildTasteProse(taste), MessageType.None);
        }

        /// <summary>Plain-language restatement of the authored fields. Counts enabled axis sides
        /// only; the runtime (+1/−1 per axis, clamp [−2,+2]) is not re-implemented here.</summary>
        private static string BuildTasteProse(SerializedProperty taste)
        {
            var sb = new StringBuilder();
            int plusSides = 0, minusSides = 0;

            bool fast = GetBool(taste, "tempoMatchOnFast");
            bool slow = GetBool(taste, "tempoMismatchOnSlow");
            if (fast) { sb.Append($"Prefers tempo above {GetFloat(taste, "preferAboveTempoScale"):0.##}×. "); plusSides++; }
            if (slow) { sb.Append($"Dislikes tempo below {GetFloat(taste, "dislikeBelowTempoScale"):0.##}×. "); minusSides++; }

            if (GetBool(taste, "roleCountMatchOnRich"))
            { sb.Append($"Rewards arrangements with ≥{GetInt(taste, "preferAtLeastRoles")} active roles. "); plusSides++; }

            var prefTs = taste.FindPropertyRelative("preferredTimeSignatures");
            var disTs = taste.FindPropertyRelative("dislikedTimeSignatures");
            if (prefTs != null && prefTs.arraySize > 0) { sb.Append($"Prefers time signatures: {ListNames(prefTs)}. "); plusSides++; }
            if (disTs != null && disTs.arraySize > 0) { sb.Append($"Dislikes time signatures: {ListNames(disTs)}. "); minusSides++; }

            var prefTon = taste.FindPropertyRelative("preferredTonalities");
            var disTon = taste.FindPropertyRelative("dislikedTonalities");
            if (prefTon != null && prefTon.arraySize > 0) { sb.Append($"Prefers tonalities: {ListNames(prefTon)}. "); plusSides++; }
            if (disTon != null && disTon.arraySize > 0) { sb.Append($"Dislikes tonalities: {ListNames(disTon)}. "); minusSides++; }

            if (sb.Length == 0)
                return "Neutral archetype: every axis is disabled, so every loop impression is 0.";

            sb.Append($"\nEnabled axis sides: +{plusSides} / −{minusSides} (runtime sums and clamps to [−2, +2]).");
            return sb.ToString();
        }

        /// <summary>Compact one-cell summary for the inventory column.</summary>
        private static string BuildTasteSummary(SerializedProperty taste, out bool neutral)
        {
            var parts = new List<string>();
            if (GetBool(taste, "tempoMatchOnFast")) parts.Add($"tempo↑>{GetFloat(taste, "preferAboveTempoScale"):0.##}");
            if (GetBool(taste, "tempoMismatchOnSlow")) parts.Add($"tempo↓<{GetFloat(taste, "dislikeBelowTempoScale"):0.##}");
            if (GetBool(taste, "roleCountMatchOnRich")) parts.Add($"roles≥{GetInt(taste, "preferAtLeastRoles")}");

            int pTs = ArraySize(taste, "preferredTimeSignatures"), dTs = ArraySize(taste, "dislikedTimeSignatures");
            if (pTs + dTs > 0) parts.Add($"TS +{pTs}/−{dTs}");

            int pTon = ArraySize(taste, "preferredTonalities"), dTon = ArraySize(taste, "dislikedTonalities");
            if (pTon + dTon > 0) parts.Add($"ton +{pTon}/−{dTon}");

            neutral = parts.Count == 0;
            return neutral ? "neutral" : string.Join(" · ", parts);
        }

        private static string ListNames(SerializedProperty list)
        {
            var names = new List<string>(list.arraySize);
            for (int i = 0; i < list.arraySize; i++)
            {
                var el = list.GetArrayElementAtIndex(i);
                if (el.propertyType == SerializedPropertyType.Enum)
                {
                    var dn = el.enumDisplayNames;
                    int idx = el.enumValueIndex;
                    names.Add(idx >= 0 && idx < dn.Length ? dn[idx] : $"<enum {idx}>");
                }
                else names.Add(el.displayName);
            }
            return string.Join(", ", names);
        }

        private static bool GetBool(SerializedProperty parent, string name) { var p = parent.FindPropertyRelative(name); return p != null && p.boolValue; }
        private static float GetFloat(SerializedProperty parent, string name) { var p = parent.FindPropertyRelative(name); return p != null ? p.floatValue : 0f; }
        private static int GetInt(SerializedProperty parent, string name) { var p = parent.FindPropertyRelative(name); return p != null ? p.intValue : 0; }
        private static int ArraySize(SerializedProperty parent, string name) { var p = parent.FindPropertyRelative(name); return p != null && p.isArray ? p.arraySize : 0; }

        // ──────────────────────────────────────────────────────────────────
        // Asset actions
        // ──────────────────────────────────────────────────────────────────
        private void DuplicateSelected()
        {
            string src = AssetDatabase.GetAssetPath(_selected);
            if (string.IsNullOrEmpty(src)) return;

            string dst = AssetDatabase.GenerateUniqueAssetPath(src);
            if (!AssetDatabase.CopyAsset(src, dst))
            {
                Debug.LogError($"{LogTag} Duplicate failed: {src}");
                return;
            }

            AssetDatabase.SaveAssets();
            var copy = AssetDatabase.LoadAssetAtPath<AudienceCharacterData>(dst);
            _selected = copy;
            Selection.activeObject = copy;
            EditorGUIUtility.PingObject(copy);
            RefreshAssets();
        }

        private void DeleteSelected()
        {
            string path = AssetDatabase.GetAssetPath(_selected);
            _rowByAsset.TryGetValue(_selected, out var row);
            int uses = row?.UsedBy.Count ?? 0;

            string usageWarning = uses > 0
                ? $"\n\nWARNING: {uses} encounter/roster asset(s) reference this member. " +
                  "Deleting will leave null entries in their audience lists."
                : "\n\nNo encounter/roster references found.";

            bool ok = EditorUtility.DisplayDialog(
                "Delete AudienceCharacterData",
                $"Delete asset?\n  {path}{usageWarning}",
                "Delete", "Cancel");
            if (!ok) return;

            DisposeSerialized();
            if (!AssetDatabase.DeleteAsset(path))
            {
                Debug.LogError($"{LogTag} Delete failed: {path}");
                return;
            }

            _selected = null;
            AssetDatabase.SaveAssets();
            RefreshAssets();
        }

        // ──────────────────────────────────────────────────────────────────
        // Export JSON — informational, current filtered view, not re-importable
        // ──────────────────────────────────────────────────────────────────
        [Serializable] private class JsonAbility { public string name; public string intention; public int actionCount; public float duration; public string animatorTrigger; public bool hasSfx; }
        [Serializable]
        private class JsonMember
        {
            public string assetName; public string assetPath; public string characterId; public string characterName;
            public int maxVibe; public bool isTall; public bool followAbilityPattern; public bool hasPrefab; public bool hasSfxProfile;
            public string tasteSummary; public List<JsonAbility> abilities = new(); public List<string> badges = new(); public List<string> usedBy = new();
        }
        [Serializable] private class WrapMembers { public List<JsonMember> audienceMembers = new(); }

        private void ExportFilteredJson()
        {
            var filtered = GetFilteredRows();
            if (filtered.Count == 0)
            {
                Debug.LogWarning($"{LogTag} Nothing to export (filter matches 0 assets).");
                return;
            }

            string path = EditorUtility.SaveFilePanel(
                "Export AudienceCharacterData JSON", "", "AudienceMembers.json", "json");
            if (string.IsNullOrEmpty(path)) return;

            var wrap = new WrapMembers();
            foreach (var r in filtered)
            {
                var m = new JsonMember
                {
                    assetName = r.Asset.name,
                    assetPath = r.Path,
                    characterId = r.Id,
                    characterName = r.Name,
                    maxVibe = r.MaxVibe,
                    isTall = r.IsTall,
                    followAbilityPattern = r.FollowPattern,
                    hasPrefab = r.HasPrefab,
                    hasSfxProfile = r.HasSfx,
                    tasteSummary = r.TasteSummary
                };
                m.badges.AddRange(r.Badges);
                foreach (var u in r.UsedBy) if (u != null) m.usedBy.Add(u.name);

                var abilities = r.Asset.AbilityList;
                if (abilities != null)
                {
                    foreach (var a in abilities)
                    {
                        if (a == null) continue;
                        m.abilities.Add(new JsonAbility
                        {
                            name = a.AbilityName,
                            intention = a.Intention != null ? a.Intention.name : "<null>",
                            actionCount = a.ActionList?.Count ?? 0,
                            duration = a.AbilityDuration,
                            animatorTrigger = a.Animation?.AnimatorTrigger ?? "",
                            hasSfx = a.AbilitySfx != null
                        });
                    }
                }
                wrap.audienceMembers.Add(m);
            }

            File.WriteAllText(path, JsonUtility.ToJson(wrap, true));
            Debug.Log($"{LogTag} Exported {wrap.audienceMembers.Count} member(s) to {path}");
            EditorUtility.RevealInFinder(path);
        }

        // ──────────────────────────────────────────────────────────────────
        // Caches — assets, derived rows, badges, usage index
        // ──────────────────────────────────────────────────────────────────
        private void RefreshAssets()
        {
            _rows.Clear();
            _rowByAsset.Clear();

            foreach (var g in AssetDatabase.FindAssets("t:AudienceCharacterData"))
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var a = AssetDatabase.LoadAssetAtPath<AudienceCharacterData>(path);
                if (a == null) continue;
                var row = new Row { Asset = a, Path = path };
                _rows.Add(row);
                _rowByAsset[a] = row;
            }
            _rows.Sort(static (x, y) => string.CompareOrdinal(x.Asset.name, y.Asset.name));

            var opts = new List<string> { "(empty)" };
            foreach (var r in _rows) opts.Add(r.Asset.name);
            _templateOptions = opts.ToArray();

            RebuildRows();
            BuildUsageIndex();
        }

        /// <summary>Recomputes every derived column and badge from the assets' serialized state.
        /// Cheap for the current corpus (single digits); called on Refresh, project change,
        /// undo/redo and after each applied edit.</summary>
        private void RebuildRows()
        {
            _missingFields.Clear();
            foreach (var r in _rows)
            {
                if (r.Asset == null) continue;
                FillDerived(r);
            }
            ComputeBadges();
        }

        private void FillDerived(Row r)
        {
            using var so = new SerializedObject(r.Asset);

            r.Id = Str(so, "characterId");
            r.Name = Str(so, "characterName");
            r.Description = Str(so, "characterDescription");
            r.MaxVibe = Int(so, "maxVibe");
            r.HasPrefab = Obj(so, "characterPrefab") != null;
            r.IsTall = Bool(so, "isTall");
            r.FollowPattern = Bool(so, "followAbilityPattern");
            r.HasSfx = Obj(so, "sfxProfile") != null;

            var abil = Find(so, "abilityList");
            r.AbilityCount = abil != null && abil.isArray ? abil.arraySize : 0;

            var taste = Find(so, "taste");
            if (taste != null) r.TasteSummary = BuildTasteSummary(taste, out r.TasteNeutral);
            else { r.TasteSummary = "?"; r.TasteNeutral = false; }
        }

        private void ComputeBadges()
        {
            var nameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in _rows)
            {
                string n = (r.Name ?? "").Trim();
                if (n.Length == 0) continue;
                nameCounts[n] = nameCounts.TryGetValue(n, out int c) ? c + 1 : 1;
            }

            foreach (var r in _rows)
            {
                r.Badges.Clear();
                if (r.AbilityCount == 0) r.Badges.Add(BadgeNoAbilities);
                if (!r.HasPrefab) r.Badges.Add(BadgeNoPrefab);
                if (r.MaxVibe <= 0) r.Badges.Add(BadgeVibe);
                if (r.FollowPattern && r.AbilityCount == 1) r.Badges.Add(BadgePattern1);
                string n = (r.Name ?? "").Trim();
                if (n.Length > 0 && nameCounts[n] > 1) r.Badges.Add(BadgeDupName);
                if (r.TasteNeutral) r.Badges.Add(BadgeNeutral);
            }
        }

        /// <summary>Reverse index by asset dependency: an encounter/roster "uses" a member when the
        /// member's .asset is among its direct dependencies. Type-agnostic — survives field renames
        /// in GigEncounterSO / GigSetupRosterSO (D-AMW-3=B).</summary>
        private void BuildUsageIndex()
        {
            var byPath = new Dictionary<string, Row>();
            foreach (var r in _rows) { r.UsedBy.Clear(); byPath[r.Path] = r; }

            _usageSourcesScanned = 0;
            foreach (var typeName in UsageSourceTypes)
            {
                foreach (var g in AssetDatabase.FindAssets($"t:{typeName}"))
                {
                    string srcPath = AssetDatabase.GUIDToAssetPath(g);
                    var src = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(srcPath);
                    if (src == null) continue;
                    _usageSourcesScanned++;

                    foreach (var dep in AssetDatabase.GetDependencies(srcPath, false))
                    {
                        if (dep == srcPath) continue;
                        if (byPath.TryGetValue(dep, out var row) && !row.UsedBy.Contains(src))
                            row.UsedBy.Add(src);
                    }
                }
            }
        }

        private static SerializedProperty Find(SerializedObject so, string name) => so.FindProperty(name);
        private string Str(SerializedObject so, string n) { var p = Find(so, n); if (p == null) { _missingFields.Add(n); return ""; } return p.stringValue ?? ""; }
        private int Int(SerializedObject so, string n) { var p = Find(so, n); if (p == null) { _missingFields.Add(n); return 0; } return p.intValue; }
        private bool Bool(SerializedObject so, string n) { var p = Find(so, n); if (p == null) { _missingFields.Add(n); return false; } return p.boolValue; }
        private UnityEngine.Object Obj(SerializedObject so, string n) { var p = Find(so, n); if (p == null) { _missingFields.Add(n); return null; } return p.objectReferenceValue; }

        private void EnsureSerialized()
        {
            if (_serialized != null && _serialized.targetObject == _selected && _selected != null)
                return;

            DisposeSerialized();
            if (_selected == null) return;
            _serialized = new SerializedObject(_selected);
        }

        private void DisposeSerialized()
        {
            _serialized?.Dispose();
            _serialized = null;
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) return;
            folderPath = folderPath.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            var parts = folderPath.Split('/');
            if (parts.Length == 0) return;

            string cur = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
#if UNITY_EDITOR
using ALWTTT.Cards.Effects;
using ALWTTT.Enums;
using ALWTTT.Musicians;
using ALWTTT.Status;
using MidiGenPlay;
using MidiGenPlay.Composition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    public sealed partial class CardEditorWindow : EditorWindow
    {
        private const float LeftPanelMinWidth = 340f;
        private const float RightPanelMinWidth = 380f;

        [SerializeField] private ALWTTTProjectRegistriesSO _registries;

        [SerializeField] private MusicianCharacterType _selectedMusician = MusicianCharacterType.None;

        // Filters
        [SerializeField] private bool _filterShowAction = true;
        [SerializeField] private bool _filterShowComposition = true;

        [SerializeField] private bool _filterStarterOnly;
        [SerializeField] private bool _filterRewardOnly;
        [SerializeField] private bool _filterLockedOnly;

        [SerializeField] private bool _filterByStatusId;
        [SerializeField] private CharacterStatusId _filterStatusId = CharacterStatusId.DamageUpFlat;

        // Selection
        [SerializeField] private MusicianCharacterData _loadedMusicianData;
        [SerializeField] private MusicianCardCatalogData _loadedCatalog;
        [SerializeField] private int _selectedEntryIndex = -1;

        // Add existing
        [SerializeField] private CardDefinition _cardToAdd;

        // Create wizard
        [SerializeField] private bool _createWizardOpen;
        [SerializeField] private CardAssetFactory.CreateCardKind _newKind = CardAssetFactory.CreateCardKind.Action;
        [SerializeField] private string _newId;
        [SerializeField] private string _newDisplayName;
        [SerializeField] private string _newNameTag = "";
        [SerializeField] private bool _useCompactKindTokens = true;
        [SerializeField] private bool _newIdEditedByUser;
        [SerializeField] private string _lastAutoId;
        [SerializeField] private int _newInspirationCost = 1;
        [SerializeField] private int _newInspirationGenerated = 0;

        [SerializeField] private CardAcquisitionFlags _newEntryFlags = CardAcquisitionFlags.UnlockedByDefault;
        [SerializeField] private int _newStarterCopies = 1;
        [SerializeField] private string _newUnlockId;

        [SerializeField, Range(0.2f, 0.8f)]
        private float _splitRatio = 0.42f; // left panel % of window width

        [SerializeField] private bool _showCardDefinitionCommonFields = true;
        [SerializeField] private bool _showPayloadFields = true;
        [SerializeField] private bool _showActionPayloadFields = true;
        [SerializeField] private bool _showCompositionPayloadFields = true;

        private const float SplitterWidth = 4f;
        private bool _draggingSplitter;

        private Vector2 _leftScroll;
        private Vector2 _rightScroll;



        // Cache musician assets by enum
        private readonly Dictionary<MusicianCharacterType, MusicianCharacterData> _musicianCache = new();

        [MenuItem("ALWTTT/Cards/Card Editor", priority = 10)]
        public static void Open()
        {
            var w = GetWindow<CardEditorWindow>();
            w.titleContent = new GUIContent("Card Editor");
            w.minSize = new Vector2(LeftPanelMinWidth + RightPanelMinWidth, 520f);
            w.Show();
        }

        private void OnEnable()
        {
            ResolveRegistries();
            RefreshMusicianCache();
        }


        private void OnProjectChange()
        {
            ResolveRegistries();
            RefreshMusicianCache();
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            float totalW = position.width;

            // Clamp so both panels respect min widths
            float leftW = Mathf.Clamp(
                totalW * _splitRatio,
                LeftPanelMinWidth,
                totalW - RightPanelMinWidth - SplitterWidth);

            float rightW = Mathf.Max(RightPanelMinWidth, totalW - leftW - SplitterWidth);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPanel(leftW);
                DrawSplitter(totalW, leftW);
                DrawRightPanel(rightW);
            }
        }

        private void ResolveRegistries(bool force = false)
        {
            if (!force && _registries != null) return;

            // 1) Preferred: Resources (runtime-safe pattern)
            _registries = ALWTTTProjectRegistriesSO.FindInResources();

            if (_registries != null) return;

            // 2) Editor fallback: locate it once via AssetDatabase (avoids manual wiring)
            string[] guids = AssetDatabase.FindAssets("t:ALWTTTProjectRegistriesSO");

            if (guids != null && guids.Length > 1)
                Debug.LogWarning($"[CardEditorWindow] " +
                    $"Multiple ALWTTTProjectRegistriesSO found ({guids.Length}). " +
                    $"Using the first one.", this);

            if (guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _registries = AssetDatabase.LoadAssetAtPath<ALWTTTProjectRegistriesSO>(path);
            }
        }

        private void DrawToolbar()
        {
            // Wrap the toolbar row + the warning under it.
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    // ─────────────────────────────────────────────────────────────
                    // Musician selection
                    // ─────────────────────────────────────────────────────────────
                    GUILayout.Label("Musician:", GUILayout.Width(62));

                    var newMusician = (MusicianCharacterType)EditorGUILayout.EnumPopup(
                        _selectedMusician,
                        EditorStyles.toolbarPopup,
                        GUILayout.Width(180));

                    if (newMusician != _selectedMusician)
                    {
                        _selectedMusician = newMusician;
                        _loadedMusicianData = null;
                        _loadedCatalog = null;
                        _selectedEntryIndex = -1;
                        _cardToAdd = null;
                        Repaint();
                    }

                    GUILayout.FlexibleSpace();

                    // ─────────────────────────────────────────────────────────────
                    // Actions
                    // ─────────────────────────────────────────────────────────────
                    using (new EditorGUI.DisabledScope(_selectedMusician ==
                        MusicianCharacterType.None))
                    {
                        if (GUILayout.Button("Load",
                            EditorStyles.toolbarButton, GUILayout.Width(60)))
                            TryLoadSelectedMusicianAndCatalog();
                    }

                    if (GUILayout.Button("Refresh",
                        EditorStyles.toolbarButton, GUILayout.Width(70)))
                        RefreshMusicianCache();

                    // ─────────────────────────────────────────────────────────────
                    // Project Registries (Option 2)
                    // ─────────────────────────────────────────────────────────────
                    GUILayout.Space(10);
                    GUILayout.Label("Registries:", GUILayout.Width(68));

                    _registries = (ALWTTTProjectRegistriesSO)EditorGUILayout.ObjectField(
                        _registries,
                        typeof(ALWTTTProjectRegistriesSO),
                        false,
                        GUILayout.Width(240));

                    using (new EditorGUI.DisabledScope(_registries != null))
                    {
                        if (GUILayout.Button("Find",
                            EditorStyles.toolbarButton, GUILayout.Width(45)))
                        {
                            ResolveRegistries(force: true);
                            Repaint();
                        }
                    }

                    using (new EditorGUI.DisabledScope(_registries == null))
                    {
                        if (GUILayout.Button("Ping",
                            EditorStyles.toolbarButton, GUILayout.Width(45)))
                            EditorGUIUtility.PingObject(_registries);
                    }
                }

                if (_registries != null && (_registries.StatusCatalogue == null || _registries.CSO == null))
                {
                    EditorGUILayout.HelpBox(
                        "ALWTTTProjectRegistriesSO is assigned but missing " +
                        "CSO and/or StatusCatalogue references.\n" +
                        "Open the registries asset and assign the missing fields.",
                        MessageType.Warning);
                }
            }
        }



        private void DrawLeftPanel(float width)
        {
            using (new EditorGUILayout.VerticalScope(
                GUILayout.Width(width), GUILayout.ExpandHeight(true)))
            {
                DrawFiltersBox();
                EditorGUILayout.Space(6);

                using (var scroll = new EditorGUILayout.ScrollViewScope(_leftScroll))
                {
                    _leftScroll = scroll.scrollPosition;

                    if (_loadedMusicianData == null && _selectedMusician != MusicianCharacterType.None)
                    {
                        EditorGUILayout.HelpBox("No MusicianCharacterData loaded. Click Load.", MessageType.Info);
                        return;
                    }

                    if (_loadedCatalog == null)
                    {
                        DrawNoCatalogLoaded();
                        return;
                    }

                    DrawEntryList();
                }
            }
        }

        private void DrawSplitter(float totalW, float leftW)
        {
            var rect =
                GUILayoutUtility.GetRect(
                    SplitterWidth, SplitterWidth, GUILayout.ExpandHeight(true));
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);

            var e = Event.current;
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                _draggingSplitter = true;
                e.Use();
            }

            if (_draggingSplitter && e.type == EventType.MouseDrag)
            {
                _splitRatio = Mathf.Clamp(e.mousePosition.x / totalW, 0.2f, 0.8f);
                Repaint();
                e.Use();
            }

            if (e.type == EventType.MouseUp)
                _draggingSplitter = false;
        }


        private void DrawFiltersBox()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Filters", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _filterShowAction = GUILayout.Toggle(_filterShowAction, "Action", "Button");
                    _filterShowComposition = GUILayout.Toggle(_filterShowComposition, "Composition", "Button");
                }

                EditorGUILayout.Space(4);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _filterStarterOnly = GUILayout.Toggle(_filterStarterOnly, "Starter", "Button");
                    _filterRewardOnly = GUILayout.Toggle(_filterRewardOnly, "Rewards", "Button");
                    _filterLockedOnly = GUILayout.Toggle(_filterLockedOnly, "Locked", "Button");
                }

                EditorGUILayout.Space(4);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _filterByStatusId = EditorGUILayout.ToggleLeft("StatusId", _filterByStatusId, GUILayout.Width(80));
                    using (new EditorGUI.DisabledScope(!_filterByStatusId))
                        _filterStatusId = (CharacterStatusId)EditorGUILayout.EnumPopup(_filterStatusId);
                }

                EditorGUILayout.Space(4);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Show Everything", GUILayout.Width(120)))
                    {
                        _filterShowAction = true;
                        _filterShowComposition = true;
                        _filterStarterOnly = false;
                        _filterRewardOnly = false;
                        _filterLockedOnly = false;

                        _filterByStatusId = false;
                        _filterStatusId = CharacterStatusId.DamageUpFlat;

                        Repaint();
                    }

                    if (GUILayout.Button("Clear", GUILayout.Width(60)))
                    {
                        _filterShowAction = false;
                        _filterShowComposition = false;
                        _filterStarterOnly = false;
                        _filterRewardOnly = false;
                        _filterLockedOnly = false;

                        _filterByStatusId = false;
                        _filterStatusId = CharacterStatusId.DamageUpFlat;

                        _selectedEntryIndex = -1;
                        Repaint();
                    }

                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space(4);

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(
                        new GUIContent("Loaded Musician"),
                        _loadedMusicianData,
                        typeof(MusicianCharacterData),
                        false);

                    EditorGUILayout.ObjectField(
                        new GUIContent("Loaded Catalog"),
                        _loadedCatalog,
                        typeof(MusicianCardCatalogData),
                        false);
                }
            }
        }


        private void DrawNoCatalogLoaded()
        {
            EditorGUILayout.HelpBox(
                "No catalog loaded.\n\nClick 'Create Catalog' to generate one and assign it to the musician.",
                MessageType.Warning);

            using (new EditorGUI.DisabledScope(_loadedMusicianData == null))
            {
                if (GUILayout.Button("Create Catalog", GUILayout.Height(26)))
                    CreateAndAssignCatalog();
            }
        }

        private void DrawEntryList()
        {
            var entries = _loadedCatalog.Entries;
            if (entries == null)
            {
                EditorGUILayout.HelpBox("Catalog entries list is null.", MessageType.Error);
                return;
            }

            // Header
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label($"Entries: {entries.Count}", EditorStyles.miniBoldLabel);
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(_loadedCatalog == null || _loadedMusicianData == null))
                {
                    if (GUILayout.Button("Sync From Assets", GUILayout.Width(130)))
                        SyncFromAssets();
                }
            }

            EditorGUILayout.Space(6);

            // Add existing row + Create wizard toggle
            using (new EditorGUILayout.HorizontalScope())
            {
                _cardToAdd = (CardDefinition)EditorGUILayout.ObjectField(_cardToAdd, typeof(CardDefinition), false);

                using (new EditorGUI.DisabledScope(_cardToAdd == null))
                {
                    if (GUILayout.Button("Add", GUILayout.Width(60)))
                    {
                        if (!MusicianCatalogService.TryAddEntry(
                                _loadedCatalog,
                                _cardToAdd,
                                CardAcquisitionFlags.UnlockedByDefault,
                                1,
                                null,
                                out var newIndex,
                                out var err))
                        {
                            EditorUtility.DisplayDialog("Card Editor", err, "OK");
                        }
                        else
                        {
                            _selectedEntryIndex = newIndex;
                            EditorGUIUtility.PingObject(_cardToAdd);
                            Selection.activeObject = _cardToAdd;
                        }

                        _cardToAdd = null;
                    }
                }
            }

            EditorGUILayout.Space(4);

            EditorGUILayout.Space(6);
            DrawJsonImportBlock();
            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(_createWizardOpen ? "Hide Create Card" : "Create Card...", GUILayout.Height(22)))
                {
                    _createWizardOpen = !_createWizardOpen;
                    if (_createWizardOpen)
                        _newKind = CardAssetFactory.CreateCardKind.Action;
                }

                GUILayout.FlexibleSpace();
            }

            if (_createWizardOpen)
            {
                EditorGUILayout.Space(4);
                DrawCreateWizard();
            }

            EditorGUILayout.Space(8);

            // Entry list
            int shown = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                if (PassesFilters(entries[i])) shown++;
            }

            GUILayout.Label($"Entries (filtered): {shown} / {entries.Count}", EditorStyles.miniBoldLabel);
            EditorGUILayout.Space(4);

            // Entry list
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (!PassesFilters(e)) continue;

                bool isSelected = i == _selectedEntryIndex;

                string flags = "";
                if (e.IsStarter) flags += "S";
                if (e.IsReward) flags += "R";
                if (!e.UnlockedByDefault) flags += "L";

                string domain = e.card.IsAction ? "A" : (e.card.IsComposition ? "C" : "?");

                string label = string.IsNullOrEmpty(flags)
                    ? $"{domain} {e.card.DisplayName}"
                    : $"{domain} [{flags}] {e.card.DisplayName}";

                if (GUILayout.Toggle(isSelected, label, "Button"))
                {
                    if (!isSelected)
                    {
                        _selectedEntryIndex = i;
                        GUI.FocusControl(null);
                        Repaint();
                    }
                }
            }

            if (shown == 0)
                EditorGUILayout.HelpBox("No entries match the current filters.", MessageType.Info);
        }

        private bool PassesFilters(MusicianCardEntry e)
        {
            if (e == null || e.card == null) return false;

            if (!_filterShowAction && e.card.IsAction) return false;
            if (!_filterShowComposition && e.card.IsComposition) return false;

            if (_filterStarterOnly && !e.IsStarter) return false;
            if (_filterRewardOnly && !e.IsReward) return false;
            if (_filterLockedOnly && e.UnlockedByDefault) return false;

            if (_filterByStatusId)
            {
                var payload = e.card.Payload;
                if (payload == null) return false;

                var effects = payload.Effects;
                bool found = false;

                if (effects != null)
                {
                    for (int i = 0; i < effects.Count; i++)
                    {
                        if (effects[i] is ApplyStatusEffectSpec ase &&
                            ase.status != null &&
                            ase.status.EffectId == _filterStatusId)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found) return false;
            }

            return true;
        }


        private void DrawCreateWizard()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Create Card + Payload", EditorStyles.boldLabel);

                using (new EditorGUI.DisabledScope(_loadedCatalog == null || _loadedMusicianData == null))
                {
                    EditorGUI.BeginChangeCheck();

                    _newKind = (CardAssetFactory.CreateCardKind)EditorGUILayout.EnumPopup("Kind", _newKind);

                    string tagLabel = _newKind == CardAssetFactory.CreateCardKind.Composition
                        ? "Role / Tag"
                        : "Tag";

                    _newNameTag = EditorGUILayout.TextField(tagLabel, _newNameTag);
                    _newDisplayName = EditorGUILayout.TextField("Display Name", _newDisplayName);
                    _useCompactKindTokens = EditorGUILayout.ToggleLeft("Use compact kind tokens (A / C)", _useCompactKindTokens);

                    bool namingInputsChanged = EditorGUI.EndChangeCheck();

                    EditorGUILayout.HelpBox(
                        _newKind == CardAssetFactory.CreateCardKind.Composition
                            ? "Naming only. Examples: Rhythm, Backing, Melody, Bass, Harmony, Rth, Bck, Mel."
                            : "Naming only. Examples: Status, Buff, Draw, Utility.",
                        MessageType.None);

                    string suggested = BuildSuggestedId();
                    if (!_newIdEditedByUser)
                    {
                        if (namingInputsChanged || string.IsNullOrWhiteSpace(_newId) || _newId == _lastAutoId)
                        {
                            _newId = suggested ?? _newId;
                            _lastAutoId = _newId;
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        _newId = EditorGUILayout.TextField("Id", _newId);
                        if (EditorGUI.EndChangeCheck())
                            _newIdEditedByUser = true;

                        using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(suggested)))
                        {
                            if (GUILayout.Button("Auto", GUILayout.Width(52)))
                            {
                                _newIdEditedByUser = false;
                                _newId = suggested;
                                _lastAutoId = _newId;
                                GUI.FocusControl(null);
                            }
                        }
                    }

                    _newInspirationCost = EditorGUILayout.IntField("Inspiration Cost", _newInspirationCost);
                    _newInspirationGenerated = EditorGUILayout.IntField("Inspiration Generated", _newInspirationGenerated);

                    _newInspirationCost = Mathf.Max(0, _newInspirationCost);
                    _newInspirationGenerated = Mathf.Max(0, _newInspirationGenerated);

                    EditorGUILayout.Space(4);
                    GUILayout.Label("Catalog Entry Defaults", EditorStyles.miniBoldLabel);

                    _newEntryFlags = (CardAcquisitionFlags)EditorGUILayout.EnumFlagsField("Flags", _newEntryFlags);

                    using (new EditorGUI.DisabledScope((_newEntryFlags & CardAcquisitionFlags.StarterDeck) == 0))
                    {
                        _newStarterCopies = EditorGUILayout.IntField("Starter Copies", _newStarterCopies);
                        _newStarterCopies = Mathf.Max(1, _newStarterCopies);
                    }

                    using (new EditorGUI.DisabledScope((_newEntryFlags & CardAcquisitionFlags.UnlockedByDefault) != 0))
                        _newUnlockId = EditorGUILayout.TextField("Unlock Id", _newUnlockId);

                    EditorGUILayout.Space(6);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Cancel", GUILayout.Width(80)))
                        {
                            _createWizardOpen = false;
                            return;
                        }

                        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_newId)))
                        {
                            if (GUILayout.Button("Create", GUILayout.Width(80)))
                            {
                                CreateCardFromWizard();
                            }
                        }
                    }

                    if (_loadedCatalog == null || _loadedMusicianData == null)
                    {
                        EditorGUILayout.HelpBox("Load a musician + catalog first.", MessageType.Info);
                    }
                }
            }
        }

        private void CreateCardFromWizard()
        {
            int inspirationCost = Mathf.Max(0, _newInspirationCost);
            int inspirationGenerated = Mathf.Max(0, _newInspirationGenerated);

            bool starter = (_newEntryFlags & CardAcquisitionFlags.StarterDeck) != 0;
            int starterCopies = starter ? Mathf.Max(1, _newStarterCopies) : 1;

            bool unlockedByDefault = (_newEntryFlags & CardAcquisitionFlags.UnlockedByDefault) != 0;
            string unlockId = unlockedByDefault ? null : (_newUnlockId?.Trim());

            var req = new CardAssetFactory.CreateCardRequest
            {
                musicianType = _selectedMusician,
                musicianData = _loadedMusicianData,
                targetCatalog = _loadedCatalog,

                kind = _newKind,
                id = _newId.Trim(),
                displayName = string.IsNullOrWhiteSpace(_newDisplayName) ? _newId.Trim() : _newDisplayName.Trim(),

                inspirationCost = inspirationCost,
                inspirationGenerated = inspirationGenerated,

                // Let factory derive folder from catalog/musician
                targetFolder = null
            };

            if (!unlockedByDefault && string.IsNullOrWhiteSpace(_newUnlockId))
            {
                EditorUtility.DisplayDialog("Create Card",
                    "This card is not UnlockedByDefault, but Unlock Id is empty.\n\n" +
                    "Either set UnlockedByDefault or provide an Unlock Id.",
                    "OK");
                return;
            }

            if (!CardAssetFactory.TryCreateCard(req, out var created, out var err))
            {
                EditorUtility.DisplayDialog("Create Card", err, "OK");
                return;
            }

            RenameCreatedAssetsToMatchId(created.cardDefinition);

            if (!MusicianCatalogService.TryAddEntry(
                    _loadedCatalog,
                    created.cardDefinition,
                    _newEntryFlags,
                    starterCopies,
                    unlockId,
                    out var newIndex,
                    out var addErr))
            {
                EditorUtility.DisplayDialog("Create Card", addErr, "OK");
                return;
            }

            _selectedEntryIndex = newIndex;
            Selection.activeObject = created.cardDefinition;
            EditorGUIUtility.PingObject(created.cardDefinition);

            // Small QoL: reset id/display name for next creation
            _newId = "";
            _newIdEditedByUser = false;
            _lastAutoId = null;
            _newDisplayName = "";
            _newNameTag = "";
            _newUnlockId = "";
            _newStarterCopies = 1;
            _newEntryFlags = CardAcquisitionFlags.UnlockedByDefault;

            Repaint();
        }

        private void DrawRightPanel(float width)
        {
            using (new EditorGUILayout.VerticalScope(
                GUILayout.Width(width), GUILayout.ExpandHeight(true)))
            {
                using (var scroll = new EditorGUILayout.ScrollViewScope(_rightScroll))
                {
                    _rightScroll = scroll.scrollPosition;

                    DrawEntryPreviewBox();
                    EditorGUILayout.Space(8);
                    DrawCardInspectorBox();
                }
            }
        }

        private void DrawEntryPreviewBox()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Entry Editor", EditorStyles.boldLabel);

                if (_loadedCatalog == null)
                {
                    EditorGUILayout.HelpBox("No catalog loaded.", MessageType.Info);
                    return;
                }

                if (_selectedEntryIndex < 0)
                {
                    EditorGUILayout.HelpBox(
                        "Select an entry from the left list.", MessageType.Info);
                    return;
                }

                // Serialized editing (Undo-friendly for nested serializable entries)
                var so = new SerializedObject(_loadedCatalog);
                var entriesProp = so.FindProperty("entries");
                if (entriesProp == null || !entriesProp.isArray)
                {
                    EditorGUILayout.HelpBox(
                        "Could not find serialized 'entries' array on catalog.",
                        MessageType.Error);
                    return;
                }

                if (_selectedEntryIndex >= entriesProp.arraySize)
                {
                    EditorGUILayout.HelpBox(
                        "Selected entry index is out of range.", MessageType.Error);
                    return;
                }

                var entryProp = entriesProp.GetArrayElementAtIndex(_selectedEntryIndex);
                var cardProp = entryProp.FindPropertyRelative("card");
                var flagsProp = entryProp.FindPropertyRelative("flags");
                var copiesProp = entryProp.FindPropertyRelative("starterCopies");
                var unlockIdProp = entryProp.FindPropertyRelative("unlockId");

                // Draw the fields
                EditorGUILayout.PropertyField(cardProp);

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(flagsProp); // flags enum (supports [Flags] via EnumFlagsField internally)
                var flags = (CardAcquisitionFlags)flagsProp.intValue;

                using (new EditorGUI.DisabledScope((flags & CardAcquisitionFlags.StarterDeck) == 0))
                {
                    EditorGUILayout.PropertyField(
                        copiesProp, new GUIContent("Starter Copies"));

                    if (copiesProp.intValue < 1) copiesProp.intValue = 1;
                }

                using (new EditorGUI.DisabledScope(
                    (flags & CardAcquisitionFlags.UnlockedByDefault) != 0))
                {
                    EditorGUILayout.PropertyField(unlockIdProp, new GUIContent("Unlock Id"));
                }

                // Validation hints
                bool isLockedByDefault =
                    (flags & CardAcquisitionFlags.UnlockedByDefault) == 0;
                if (isLockedByDefault && string.IsNullOrWhiteSpace(unlockIdProp.stringValue))
                {
                    EditorGUILayout.HelpBox(
                        "This entry is NOT UnlockedByDefault, but Unlock Id is empty.\n" +
                        "Add an Unlock Id or enable UnlockedByDefault.",
                        MessageType.Warning);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    so.ApplyModifiedProperties();          // applies + registers Undo properly
                    EditorUtility.SetDirty(_loadedCatalog);
                    // Optional: Save immediately (safe, but can be noisy). You can remove if you prefer manual saves.
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private void DrawCardInspectorBox()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Card Inspector", EditorStyles.boldLabel);

                if (!TryGetSelectedEntry(out var entry) || entry.card == null)
                {
                    EditorGUILayout.HelpBox(
                        "Select an entry to preview its CardDefinition.", MessageType.Info);
                    return;
                }

                var card = entry.card;

                // 1) SUMMARY FIRST (read-only)
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(
                        "Card", card, typeof(CardDefinition), false);
                    EditorGUILayout.TextField(
                        "Domain", card.Domain.ToString());
                    EditorGUILayout.ObjectField(
                        "Payload", card.Payload, typeof(CardPayload), false);
                    EditorGUILayout.ObjectField(
                        "Sprite", card.CardSprite, typeof(Sprite), false);
                }

                EditorGUILayout.Space(8);

                // 2) COLLAPSIBLE COMMON FIELDS (editable)
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    _showCardDefinitionCommonFields =
                        EditorGUILayout.BeginFoldoutHeaderGroup(
                            _showCardDefinitionCommonFields,
                            "CardDefinition (Common Fields)");

                    EditorGUILayout.EndFoldoutHeaderGroup();

                    // IMPORTANT: draw contents AFTER the header group is closed
                    // (prevents nesting errors)
                    if (_showCardDefinitionCommonFields)
                    {
                        EditorGUI.indentLevel++;
                        DrawCardDefinitionCommonFields(card);
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.Space(8);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    _showPayloadFields =
                        EditorGUILayout.BeginFoldoutHeaderGroup(
                            _showPayloadFields, "Payload");

                    EditorGUILayout.EndFoldoutHeaderGroup();

                    // IMPORTANT: draw contents AFTER the header group is closed
                    // (prevents nesting errors)
                    if (_showPayloadFields)
                    {
                        EditorGUI.indentLevel++;
                        DrawPayloadEditors(card);
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.Space(6);

                // Actions
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Select"))
                        SelectAndPing(card);

                    if (GUILayout.Button("Rename Assets From Current Id"))
                        RenameCreatedAssetsToMatchId(card);

                    using (new EditorGUI.DisabledScope(_loadedCatalog == null ||
                        _selectedEntryIndex < 0))
                    {
                        if (GUILayout.Button("Delete"))
                            ConfirmAndDeleteSelectedCard();
                    }
                }
            }
        }


        private static void SelectAndPing(UnityEngine.Object obj)
        {
            if (obj == null) return;
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        private void ConfirmAndDeleteSelectedCard()
        {
            if (!TryGetSelectedEntry(out var entry) || entry == null || entry.card == null)
                return;

            if (_loadedCatalog == null)
                return;

            var card = entry.card;
            var payload = card.Payload;

            string cardPath = AssetDatabase.GetAssetPath(card);
            string payloadPath = payload != null ?
                AssetDatabase.GetAssetPath(payload) : null;

            string msg =
                "This will:\n" +
                $"- Remove the entry from catalog '{_loadedCatalog.name}'\n" +
                "- Optionally delete these assets:\n" +
                $"  • {cardPath}\n" +
                (string.IsNullOrEmpty(payloadPath) ? "" : $"  • {payloadPath}\n") +
                "\n\nContinue?";

            int choice = EditorUtility.DisplayDialogComplex(
                "Remove Card",
                msg + "\nTip: Use 'Remove Only' to avoid breaking references in other assets.",
                "Remove Only",
                "Cancel",
                "Delete Assets");

            if (choice == 1) // Cancel
                return;

            bool deleteAssets = (choice == 2);

            // 1) Remove entry from catalog (Undo supports this part)
            Undo.RecordObject(_loadedCatalog, "Remove Musician Card Entry");
            var entries = _loadedCatalog.Entries;

            if (entries != null && _selectedEntryIndex >= 0 && _selectedEntryIndex < entries.Count)
                entries.RemoveAt(_selectedEntryIndex);

            EditorUtility.SetDirty(_loadedCatalog);
            AssetDatabase.SaveAssets();

            // 2) Delete assets (Unity cannot Undo asset deletion)
            bool payloadOk = true;
            bool cardOk = true;

            if (deleteAssets)
            {
                if (!string.IsNullOrEmpty(payloadPath))
                    payloadOk = AssetDatabase.DeleteAsset(payloadPath);

                if (!string.IsNullOrEmpty(cardPath))
                    cardOk = AssetDatabase.DeleteAsset(cardPath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!payloadOk || !cardOk)
                {
                    EditorUtility.DisplayDialog(
                        "Delete Card",
                        "Delete completed, but one or more assets could not be deleted.\n" +
                        "Check the Console for details (file may be missing or read-only).",
                        "OK");

                    if (!payloadOk)
                        Debug.LogError($"[CardEditorWindow] " +
                            $"Failed to delete payload asset: {payloadPath}");

                    if (!cardOk)
                        Debug.LogError($"[CardEditorWindow] " +
                            $"Failed to delete card asset: {cardPath}");
                }
            }

            _selectedEntryIndex = -1;
            Repaint();
        }

        private void DrawCardDefinitionCommonFields(CardDefinition card)
        {
            if (card == null) return;

            var so = new SerializedObject(card);
            so.Update();

            // These names must match CardDefinition's private fields.
            var idProp = so.FindProperty("id");
            var displayNameProp = so.FindProperty("displayName");

            var performerRuleProp = so.FindProperty("performerRule");
            var musicianTypeProp = so.FindProperty("musicianCharacterType");

            var spriteProp = so.FindProperty("cardSprite");

            var inspirationCostProp = so.FindProperty("inspirationCost");
            var inspirationGeneratedProp = so.FindProperty("inspirationGenerated");

            var cardTypeProp = so.FindProperty("cardType");
            var rarityProp = so.FindProperty("rarity");
            var keywordsProp = so.FindProperty("keywords");

            var audioTypeProp = so.FindProperty("audioType");
            var musicianAnimProp = so.FindProperty("musicianAnimation"); // nested CardAnimationData

            var exhaustAfterPlayProp = so.FindProperty("exhaustAfterPlay");
            var overrideReqTargetProp = so.FindProperty("overrideRequiresTargetSelection");
            var reqTargetOverrideValueProp = so.FindProperty("requiresTargetSelectionOverrideValue");

            EditorGUI.BeginChangeCheck();

            // Card profile
            EditorGUILayout.PropertyField(idProp);
            EditorGUILayout.PropertyField(displayNameProp);

            EditorGUILayout.Space(6);

            // Character
            EditorGUILayout.PropertyField(performerRuleProp);

            // Only show fixed performer enum when rule is FixedMusicianType.
            if (performerRuleProp != null &&
                performerRuleProp.enumValueIndex == (int)CardPerformerRule.FixedMusicianType)
            {
                EditorGUILayout.PropertyField(musicianTypeProp, new GUIContent("Fixed Musician"));
            }

            EditorGUILayout.Space(6);

            // Visuals / Economy
            EditorGUILayout.PropertyField(spriteProp);
            EditorGUILayout.PropertyField(inspirationCostProp);
            EditorGUILayout.PropertyField(inspirationGeneratedProp);

            // clamps
            if (inspirationCostProp != null && inspirationCostProp.intValue < 0) inspirationCostProp.intValue = 0;
            if (inspirationGeneratedProp != null && inspirationGeneratedProp.intValue < 0) inspirationGeneratedProp.intValue = 0;

            EditorGUILayout.Space(6);

            // Meta / Synergies
            EditorGUILayout.PropertyField(cardTypeProp);
            EditorGUILayout.PropertyField(rarityProp);
            EditorGUILayout.PropertyField(keywordsProp, includeChildren: true);

            EditorGUILayout.Space(6);

            // FX / Animation
            EditorGUILayout.PropertyField(audioTypeProp);
            EditorGUILayout.PropertyField(musicianAnimProp, includeChildren: true);

            EditorGUILayout.Space(6);

            // Play rules
            EditorGUILayout.PropertyField(exhaustAfterPlayProp);

            EditorGUILayout.PropertyField(overrideReqTargetProp, new GUIContent("Override Requires Target?"));
            if (overrideReqTargetProp != null && overrideReqTargetProp.boolValue)
                EditorGUILayout.PropertyField(reqTargetOverrideValueProp, new GUIContent("Requires Target"));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(card, "Edit CardDefinition");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(card);
                AssetDatabase.SaveAssets();
            }
            else
            {
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }


        private void DrawPayloadEditors(CardDefinition card)
        {
            if (card == null) return;

            if (!card.HasPayload || card.Payload == null)
            {
                EditorGUILayout.HelpBox("This card has no payload assigned.", MessageType.Warning);
                return;
            }

            // Action payload
            if (card.ActionPayload != null)
            {
                _showActionPayloadFields = EditorGUILayout.Foldout(
                    _showActionPayloadFields,
                    "ActionCardPayload",
                    toggleOnLabelClick: true);

                if (_showActionPayloadFields)
                {
                    EditorGUI.indentLevel++;
                    DrawActionPayloadEditor(card.ActionPayload);
                    EditorGUI.indentLevel--;
                }

                return;
            }

            // Composition payload
            if (card.CompositionPayload != null)
            {
                _showCompositionPayloadFields = EditorGUILayout.Foldout(
                    _showCompositionPayloadFields,
                    "CompositionCardPayload",
                    toggleOnLabelClick: true);

                if (_showCompositionPayloadFields)
                {
                    EditorGUI.indentLevel++;
                    DrawCompositionPayloadEditor(card.CompositionPayload);
                    EditorGUI.indentLevel--;
                }

                return;
            }

            EditorGUILayout.HelpBox(
                $"Unknown payload type: {card.Payload.GetType().Name}",
                MessageType.Info);
        }


        private void DrawActionPayloadEditor(ActionCardPayload payload)
        {
            if (payload == null) return;

            var so = new SerializedObject(payload);
            so.Update();

            var timingProp = so.FindProperty("actionTiming");
            var conditionsProp = so.FindProperty("conditions");

            // ✅ NEW: polymorphic card effects (SerializeReference list on CardPayload base)
            var effectsProp = so.FindProperty("effects");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(timingProp);
            EditorGUILayout.PropertyField(conditionsProp, includeChildren: true);

            EditorGUILayout.Space(8);
            DrawEffectsBlock(effectsProp, _registries?.StatusCatalogue);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(payload, "Edit ActionCardPayload");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(payload);
                AssetDatabase.SaveAssets();
            }
            else
            {
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private void DrawCompositionPayloadEditor(CompositionCardPayload payload)
        {
            if (payload == null) return;

            var so = new SerializedObject(payload);
            so.Update();

            var primaryKindProp = so.FindProperty("primaryKind");
            var trackActionProp = so.FindProperty("trackAction");
            var partActionProp = so.FindProperty("partAction");
            var modifierEffectsProp = so.FindProperty("modifierEffects");

            // ✅ NEW: polymorphic card effects (SerializeReference list on CardPayload base)
            var effectsProp = so.FindProperty("effects");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(primaryKindProp);
            DrawTrackActionEditor(trackActionProp, payload);
            EditorGUILayout.PropertyField(partActionProp, includeChildren: true);
            EditorGUILayout.PropertyField(modifierEffectsProp, includeChildren: true);

            EditorGUILayout.Space(8);
            DrawEffectsBlock(effectsProp, _registries?.StatusCatalogue);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(payload, "Edit CompositionCardPayload");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(payload);
                AssetDatabase.SaveAssets();
            }
            else
            {
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // ---------------------------------------------------------------------
        // Composition payload QoL: TrackAction style-bundle creator
        // ---------------------------------------------------------------------

        private void DrawTrackActionEditor(SerializedProperty trackActionProp, CompositionCardPayload payload)
        {
            if (trackActionProp == null)
            {
                EditorGUILayout.HelpBox(
                    "CompositionCardPayload is missing expected property 'trackAction'.",
                    MessageType.Error);
                return;
            }

            var roleProp = trackActionProp.FindPropertyRelative("role");
            var styleBundleProp = trackActionProp.FindPropertyRelative("styleBundle");

            // If the descriptor shape changes, fall back to default drawing.
            if (roleProp == null || styleBundleProp == null)
            {
                EditorGUILayout.PropertyField(trackActionProp, includeChildren: true);
                return;
            }

            trackActionProp.isExpanded = EditorGUILayout.Foldout(
                trackActionProp.isExpanded,
                trackActionProp.displayName,
                toggleOnLabelClick: true);

            if (!trackActionProp.isExpanded)
                return;

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(roleProp);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(styleBundleProp);

                using (new EditorGUI.DisabledScope(styleBundleProp.objectReferenceValue != null))
                {
                    if (GUILayout.Button("Create", GUILayout.Width(60)))
                        CreateAndAssignStyleBundle(roleProp, styleBundleProp, payload);
                }

                using (new EditorGUI.DisabledScope(styleBundleProp.objectReferenceValue == null))
                {
                    if (GUILayout.Button("Ping", GUILayout.Width(44)))
                        EditorGUIUtility.PingObject(styleBundleProp.objectReferenceValue);
                }
            }

            // Future-proofing: if TrackActionDescriptor gains more fields later,
            // render them automatically (excluding role/styleBundle which we already drew).
            var iter = trackActionProp.Copy();
            var end = iter.GetEndProperty();
            bool enterChildren = true;

            while (iter.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iter, end))
            {
                enterChildren = false;

                if (SerializedProperty.EqualContents(iter, roleProp) ||
                    SerializedProperty.EqualContents(iter, styleBundleProp))
                    continue;

                EditorGUILayout.PropertyField(iter, includeChildren: true);
            }

            EditorGUI.indentLevel--;
        }

        private void CreateAndAssignStyleBundle(
            SerializedProperty roleProp,
            SerializedProperty styleBundleProp,
            CompositionCardPayload payload)
        {
            string roleName = GetEnumNameSafe(roleProp);
            var bundleType = ResolveBundleTypeForRole(roleName);

            // Decide folder based on where the payload asset lives (keeps things tidy per musician/card set).
            string folder = GetDefaultStyleBundleFolder(payload, roleName);
            EnsureFolderExists(folder);

            string fileName = $"{payload.name}_{roleName}_StyleBundle.asset";
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{fileName}");

            var bundle = ScriptableObject.CreateInstance(bundleType) as TrackStyleBundleSO;
            if (bundle == null)
            {
                Debug.LogError($"[CardEditorWindow] Failed to create style bundle of type '{bundleType?.Name}'.");
                return;
            }

            // Best-effort: tag the bundle with the role (if enum parsing matches).
            try
            {
                bundle.appliesTo = (TrackRole)Enum.Parse(typeof(TrackRole), roleName);
            }
            catch { /* ignore */ }

            AssetDatabase.CreateAsset(bundle, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Assign back into the payload
            Undo.RecordObject(payload, "Assign Track Style Bundle");
            styleBundleProp.objectReferenceValue = bundle;
            styleBundleProp.serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(payload);

            Selection.activeObject = bundle;
            EditorGUIUtility.PingObject(bundle);
        }

        private static Type ResolveBundleTypeForRole(string roleName)
        {
            // Use literal strings to avoid compile-time coupling to enum members.
            return roleName switch
            {
                "Backing" => typeof(BackingCardConfigSO),
                "Melody" => typeof(MelodyCardConfigSO),
                "Harmony" => typeof(HarmonyCardConfigSO),
                "Rhythm" => typeof(RhythmCardConfigSO),
                _ => typeof(TrackStyleBundleSO),
            };
        }

        private static string GetEnumNameSafe(SerializedProperty enumProp)
        {
            if (enumProp == null || enumProp.propertyType != SerializedPropertyType.Enum)
                return string.Empty;

            int idx = Mathf.Clamp(enumProp.enumValueIndex, 0, enumProp.enumNames.Length - 1);
            return enumProp.enumNames[idx];
        }

        private static string GetDefaultStyleBundleFolder(CompositionCardPayload payload, string roleName)
        {
            // Fallback if payload isn't an asset (should be rare in this tool).
            const string fallbackRoot = "Assets/Resources/Data/Cards/StyleBundles";

            string payloadPath = payload != null ? AssetDatabase.GetAssetPath(payload) : null;
            if (string.IsNullOrWhiteSpace(payloadPath))
                return $"{fallbackRoot}/{roleName}";

            payloadPath = payloadPath.Replace("\\", "/");
            string payloadFolder = Path.GetDirectoryName(payloadPath)?.Replace("\\", "/");
            if (string.IsNullOrWhiteSpace(payloadFolder))
                return $"{fallbackRoot}/{roleName}";

            // Expected: .../<Musician>_Cards/Payloads
            // Target:   .../<Musician>_Cards/StyleBundles/<Role>
            string musicianRoot = Path.GetDirectoryName(payloadFolder)?.Replace("\\", "/");
            if (string.IsNullOrWhiteSpace(musicianRoot))
                return $"{fallbackRoot}/{roleName}";

            return $"{musicianRoot}/StyleBundles/{roleName}";
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            folderPath = folderPath.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            // Create recursively: Assets/.../A/B/C
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

        private static bool _effectsFoldout = true;

        private static void DrawEffectsBlock(SerializedProperty effectsProp, StatusEffectCatalogueSO catalogue)
        {
            if (effectsProp == null)
            {
                EditorGUILayout.HelpBox(
                    "Effects list not found (expected private field 'effects' on CardPayload).",
                    MessageType.Error);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _effectsFoldout = EditorGUILayout.Foldout(
                    _effectsFoldout,
                    $"Effects (New) ({effectsProp.arraySize})",
                    toggleOnLabelClick: true);

                if (!_effectsFoldout)
                    return;

                EditorGUI.indentLevel++;

                // Render elements with small helper controls.
                for (int i = 0; i < effectsProp.arraySize; i++)
                {
                    var el = effectsProp.GetArrayElementAtIndex(i);
                    if (el == null) continue;

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label(BuildEffectLabel(el, i), EditorStyles.boldLabel);

                            GUILayout.FlexibleSpace();

                            using (new EditorGUI.DisabledScope(i == 0))
                            {
                                if (GUILayout.Button("↑", GUILayout.Width(26)))
                                {
                                    effectsProp.MoveArrayElement(i, i - 1);
                                    GUI.changed = true;
                                }
                            }

                            using (new EditorGUI.DisabledScope(i == effectsProp.arraySize - 1))
                            {
                                if (GUILayout.Button("↓", GUILayout.Width(26)))
                                {
                                    effectsProp.MoveArrayElement(i, i + 1);
                                    GUI.changed = true;
                                }
                            }

                            if (GUILayout.Button("Remove", GUILayout.Width(70)))
                            {
                                effectsProp.DeleteArrayElementAtIndex(i);
                                GUI.changed = true;
                                break; // stop rendering (array changed)
                            }
                        }

                        DrawEffectFields(el, catalogue, i);
                    }
                }

                EditorGUILayout.Space(4);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Add Effect…", GUILayout.Width(120)))
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Apply Status Effect"), false,
                            () => AddEffect(effectsProp, new ApplyStatusEffectSpec()));
                        menu.AddItem(new GUIContent("Modify Vibe"), false,
                            () => AddEffect(effectsProp, new ModifyVibeSpec()));
                        menu.AddItem(new GUIContent("Modify Stress"), false,
                            () => AddEffect(effectsProp, new ModifyStressSpec()));
                        menu.AddItem(new GUIContent("Draw Cards"), false,
                            () => AddEffect(effectsProp, new DrawCardsSpec()));
                        menu.ShowAsContext();
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private static void AddEffect(SerializedProperty effectsProp, CardEffectSpec instance)
        {
            if (effectsProp == null || instance == null) return;

            int idx = effectsProp.arraySize;
            effectsProp.InsertArrayElementAtIndex(idx);
            var el = effectsProp.GetArrayElementAtIndex(idx);
            el.managedReferenceValue = instance;

            // GenericMenu callbacks run outside the OnGUI pass that created
            // the SerializedObject, so EndChangeCheck never fires.
            // Commit immediately.
            effectsProp.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(effectsProp.serializedObject.targetObject);

            GUI.changed = true;
        }

        private static string BuildEffectLabel(SerializedProperty el, int index)
        {
            // managedReferenceFullTypename: "AssemblyName TypeName"
            var full = el.managedReferenceFullTypename ?? string.Empty;
            var typeName = full;
            int space = full.IndexOf(' ');
            if (space >= 0 && space < full.Length - 1)
                typeName = full.Substring(space + 1);

            // strip namespace for readability
            int lastDot = typeName.LastIndexOf('.');
            if (lastDot >= 0 && lastDot < typeName.Length - 1)
                typeName = typeName.Substring(lastDot + 1);

            // Special labels when possible
            if (typeName == nameof(DrawCardsSpec))
            {
                var countProp = el.FindPropertyRelative("count");
                int c = countProp != null ? countProp.intValue : 0;
                return $"[{index}] DrawCards x{c}";
            }

            if (typeName == nameof(ApplyStatusEffectSpec))
            {
                var statusProp = el.FindPropertyRelative("status");
                var so = statusProp != null ? statusProp.objectReferenceValue as StatusEffectSO : null;
                string name = so != null ? (string.IsNullOrWhiteSpace(so.DisplayName) ? so.name : so.DisplayName) : "<null>";
                return $"[{index}] ApplyStatus: {name}";
            }

            if (typeName == nameof(ModifyVibeSpec))
            {
                var amountProp = el.FindPropertyRelative("amount");
                var targetProp = el.FindPropertyRelative("targetType");
                int a = amountProp != null ? amountProp.intValue : 0;
                string tgt = targetProp != null ? targetProp.enumDisplayNames[targetProp.enumValueIndex] : "?";
                string sign = a >= 0 ? "+" : string.Empty;
                return $"[{index}] ModifyVibe {sign}{a} ({tgt})";
            }

            if (typeName == nameof(ModifyStressSpec))
            {
                var amountProp = el.FindPropertyRelative("amount");
                var targetProp = el.FindPropertyRelative("targetType");
                int a = amountProp != null ? amountProp.intValue : 0;
                string tgt = targetProp != null ? targetProp.enumDisplayNames[targetProp.enumValueIndex] : "?";
                string sign = a >= 0 ? "+" : string.Empty;
                return $"[{index}] ModifyStress {sign}{a} ({tgt})";
            }

            return $"[{index}] {typeName}";
        }

        private static void DrawEffectFields(SerializedProperty el, StatusEffectCatalogueSO catalogue, int row)
        {
            // ApplyStatusEffectSpec: show a catalogue-backed picker (DisplayName), and validate.
            var statusProp = el.FindPropertyRelative("status");
            if (statusProp != null)
            {
                DrawStatusEffectPicker(statusProp, catalogue);

                if (statusProp.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(
                        $"Row {row}: ApplyStatusEffectSpec requires a non-null StatusEffectSO reference.",
                        MessageType.Warning);
                }

                // Show remaining fields
                var targetProp = el.FindPropertyRelative("targetType");
                var stacksProp = el.FindPropertyRelative("stacksDelta");
                var delayProp = el.FindPropertyRelative("delay");

                if (targetProp != null) EditorGUILayout.PropertyField(targetProp);
                if (stacksProp != null) EditorGUILayout.PropertyField(stacksProp);
                if (delayProp != null) EditorGUILayout.PropertyField(delayProp);

                return;
            }

            // Default fallback: just draw children (works for simple specs like DrawCardsSpec)
            EditorGUILayout.PropertyField(el, includeChildren: true);
        }

        private static void DrawStatusEffectPicker(SerializedProperty statusProp, StatusEffectCatalogueSO catalogue)
        {
            // If no catalogue, fall back to a plain object field.
            if (catalogue == null || catalogue.Effects == null || catalogue.Effects.Count == 0)
            {
                EditorGUILayout.PropertyField(statusProp);
                return;
            }

            // Filter nulls for a cleaner UX
            var raw = catalogue.Effects;
            var list = new System.Collections.Generic.List<StatusEffectSO>(raw.Count);
            for (int i = 0; i < raw.Count; i++)
                if (raw[i] != null) list.Add(raw[i]);

            if (list.Count == 0)
            {
                EditorGUILayout.PropertyField(statusProp);
                return;
            }

            var options = new string[list.Count + 1];
            options[0] = "<None>";

            int current = 0;
            var currentObj = statusProp.objectReferenceValue as StatusEffectSO;

            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                string name = string.IsNullOrWhiteSpace(e.DisplayName) ? e.name : e.DisplayName;
                options[i + 1] = name;

                if (currentObj == e)
                    current = i + 1;
            }

            int next = EditorGUILayout.Popup("Status", current, options);
            if (next != current)
            {
                statusProp.objectReferenceValue = next <= 0 ? null : list[next - 1];
                GUI.changed = true;
            }
        }


        // --- Loading ----------------------------------------------------------

        private void RefreshMusicianCache()
        {
            _musicianCache.Clear();

            string[] guids = AssetDatabase.FindAssets("t:MusicianCharacterData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<MusicianCharacterData>(path);
                if (asset == null) continue;

                var type = asset.CharacterType;
                if (type == MusicianCharacterType.None) continue;

                _musicianCache[type] = asset;
            }
        }

        private void TryLoadSelectedMusicianAndCatalog()
        {
            _selectedEntryIndex = -1;
            _loadedCatalog = null;
            _loadedMusicianData = null;

            if (_selectedMusician == MusicianCharacterType.None)
                return;

            if (!_musicianCache.TryGetValue(_selectedMusician, out var musicianData) || musicianData == null)
            {
                Debug.LogWarning($"[CardEditorWindow] No MusicianCharacterData found for '{_selectedMusician}'.");
                return;
            }

            _loadedMusicianData = musicianData;
            _loadedCatalog = _loadedMusicianData.CardCatalog;

            Repaint();
        }

        private void CreateAndAssignCatalog()
        {
            if (_loadedMusicianData == null)
                return;

            if (_loadedMusicianData.CardCatalog != null)
            {
                _loadedCatalog = _loadedMusicianData.CardCatalog;
                return;
            }

            string musicianPath = AssetDatabase.GetAssetPath(_loadedMusicianData);
            string folder = Path.GetDirectoryName(musicianPath) ?? "Assets";
            string safeName = $"{_loadedMusicianData.CharacterType}_CardCatalogData";
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, safeName + ".asset"));

            var catalog = CreateInstance<MusicianCardCatalogData>();
            AssetDatabase.CreateAsset(catalog, assetPath);

            // Set musicianType (private) via SerializedObject
            var catalogSO = new SerializedObject(catalog);
            var musicianTypeProp = catalogSO.FindProperty("musicianType");
            if (musicianTypeProp != null)
                musicianTypeProp.enumValueIndex = (int)_loadedMusicianData.CharacterType;
            catalogSO.ApplyModifiedPropertiesWithoutUndo();

            // Assign to musician (private) via SerializedObject
            Undo.RecordObject(_loadedMusicianData, "Assign Musician Card Catalog");
            var musicianSO = new SerializedObject(_loadedMusicianData);
            var cardCatalogProp = musicianSO.FindProperty("cardCatalog");
            if (cardCatalogProp != null)
                cardCatalogProp.objectReferenceValue = catalog;
            musicianSO.ApplyModifiedProperties();

            EditorUtility.SetDirty(catalog);
            EditorUtility.SetDirty(_loadedMusicianData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _loadedCatalog = catalog;
            _selectedEntryIndex = -1;

            Debug.Log($"[CardEditorWindow] Created catalog: {assetPath}");
        }

        private bool TryGetSelectedEntry(out MusicianCardEntry entry)
        {
            entry = null;
            if (_loadedCatalog == null) return false;

            var entries = _loadedCatalog.Entries;
            if (entries == null) return false;

            if (_selectedEntryIndex < 0 || _selectedEntryIndex >= entries.Count) return false;

            entry = entries[_selectedEntryIndex];
            return entry != null;
        }

        private string BuildSuggestedId()
        {
            if (_loadedMusicianData == null || _loadedCatalog == null)
                return null;

            string musicianToken = ToSafeToken(
                string.IsNullOrWhiteSpace(_loadedMusicianData.CharacterId)
                    ? _selectedMusician.ToString()
                    : _loadedMusicianData.CharacterId,
                "Musician");

            string kindToken = GetKindToken(_newKind);

            string tagToken = ToSafeToken(
                string.IsNullOrWhiteSpace(_newNameTag)
                    ? GetDefaultTagForKind(_newKind)
                    : _newNameTag,
                "Card");

            string displayToken = ToSafeToken(
                string.IsNullOrWhiteSpace(_newDisplayName) ? "Card" : _newDisplayName,
                "Card");

            string prefix = $"{musicianToken}_{kindToken}_{tagToken}_";

            int max = 0;
            var entries = _loadedCatalog.Entries;
            if (entries != null)
            {
                foreach (var e in entries)
                {
                    var c = e?.card;
                    if (c == null) continue;

                    string id = c.Id;
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    if (!id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

                    if (TryParseSequenceAfterPrefix(id, prefix, out int seq))
                        max = Mathf.Max(max, seq);
                }
            }

            int next = max + 1;
            return $"{prefix}{next:D3}_{displayToken}";
        }

        private string GetKindToken(CardAssetFactory.CreateCardKind kind)
        {
            if (_useCompactKindTokens)
            {
                return kind switch
                {
                    CardAssetFactory.CreateCardKind.Action => "A",
                    CardAssetFactory.CreateCardKind.Composition => "C",
                    _ => kind.ToString()
                };
            }

            return kind.ToString();
        }

        private static string GetDefaultTagForKind(CardAssetFactory.CreateCardKind kind)
        {
            return kind switch
            {
                CardAssetFactory.CreateCardKind.Action => "Action",
                CardAssetFactory.CreateCardKind.Composition => "Composition",
                _ => "Card"
            };
        }

        private static bool TryParseSequenceAfterPrefix(string id, string prefix, out int seq)
        {
            seq = 0;

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(prefix))
                return false;

            if (!id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            string tail = id.Substring(prefix.Length);
            if (tail.Length < 3)
                return false;

            return int.TryParse(tail.Substring(0, 3), out seq);
        }

        private static string ToSafeToken(string raw, string fallback)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return fallback;

            var sb = new StringBuilder(raw.Length);
            bool upperNext = true;

            foreach (char ch in raw.Trim())
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(upperNext ? char.ToUpperInvariant(ch) : ch);
                    upperNext = false;
                }
                else
                {
                    upperNext = true;
                }
            }

            return sb.Length > 0 ? sb.ToString() : fallback;
        }

        private static void RenameCreatedAssetsToMatchId(CardDefinition card)
        {
            if (card == null) return;

            string baseName = ToSafeToken(card.Id, "Card");
            RenameAssetIfNeeded(card, baseName);

            if (card.Payload != null)
                RenameAssetIfNeeded(card.Payload, $"{baseName}_Payload");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void RenameAssetIfNeeded(UnityEngine.Object obj, string desiredName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(desiredName))
                return;

            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrWhiteSpace(path))
                return;

            string currentName = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(currentName, desiredName, StringComparison.Ordinal))
                return;

            string err = AssetDatabase.RenameAsset(path, desiredName);
            if (!string.IsNullOrEmpty(err))
                Debug.LogWarning($"[CardEditorWindow] Could not rename asset '{path}' -> '{desiredName}': {err}");
        }

        private void SyncFromAssets()
        {
            if (_loadedCatalog == null || _loadedMusicianData == null)
                return;

            // Keep selection stable by tracking the selected card asset (not the index).
            CardDefinition previouslySelected = null;
            if (TryGetSelectedEntry(out var selectedEntry) && selectedEntry?.card != null)
                previouslySelected = selectedEntry.card;

            // Build a set of existing cards to prevent duplicates.
            var existing = new HashSet<CardDefinition>();
            var entries = _loadedCatalog.Entries;
            if (entries != null)
            {
                foreach (var e in entries)
                    if (e?.card != null)
                        existing.Add(e.card);
            }

            // Find all CardDefinition assets in the project.
            string[] guids = AssetDatabase.FindAssets("t:CardDefinition");

            int added = 0;
            int skippedDuplicates = 0;
            int skippedNotForMusician = 0;

            Undo.RecordObject(_loadedCatalog, "Sync Musician Card Catalog");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
                if (card == null)
                    continue;

                // Only sync cards explicitly tied to a fixed performer of this musician.
                if (!card.RequiresFixedPerformer || card.FixedPerformerType != _selectedMusician)
                {
                    skippedNotForMusician++;
                    continue;
                }

                if (existing.Contains(card))
                {
                    skippedDuplicates++;
                    continue;
                }

                // Append new entry (does NOT reorder existing entries -> selection index remains valid)
                entries.Add(new MusicianCardEntry
                {
                    card = card,
                    flags = CardAcquisitionFlags.UnlockedByDefault,
                    starterCopies = 1,
                    unlockId = null
                });

                existing.Add(card);
                added++;
            }

            EditorUtility.SetDirty(_loadedCatalog);
            AssetDatabase.SaveAssets();

            // Restore selection by card reference (stable even if list changes).
            if (previouslySelected != null)
                _selectedEntryIndex = FindEntryIndexByCard(previouslySelected);

            Repaint();

            EditorUtility.DisplayDialog(
                "Sync From Assets",
                $"Added: {added}\n" +
                $"Skipped (duplicates): {skippedDuplicates}\n" +
                $"Skipped (not for this musician): {skippedNotForMusician}",
                "OK");
        }

        private int FindEntryIndexByCard(CardDefinition card)
        {
            if (_loadedCatalog == null || card == null)
                return -1;

            var entries = _loadedCatalog.Entries;
            if (entries == null)
                return -1;

            for (int i = 0; i < entries.Count; i++)
                if (entries[i]?.card == card)
                    return i;

            return -1;
        }

        #region Helpers
        private static bool TryParseEnum<T>(string s, out T value) where T : struct
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                value = default;
                return false;
            }
            return System.Enum.TryParse(s.Trim(), true, out value);
        }

        private static void SetString(SerializedObject so, string propName, string v)
        {
            var p = so.FindProperty(propName);
            if (p != null) p.stringValue = v;
        }

        private static void SetInt(SerializedObject so, string propName, int v)
        {
            var p = so.FindProperty(propName);
            if (p != null) p.intValue = v;
        }

        private static void SetBool(SerializedObject so, string propName, bool v)
        {
            var p = so.FindProperty(propName);
            if (p != null) p.boolValue = v;
        }

        private static void SetEnum<T>(
            SerializedObject so, string propName, T enumValue) where T : struct
        {
            var p = so.FindProperty(propName);
            if (p != null) p.enumValueIndex = (int)(object)enumValue;
        }

        private static void SetObject(SerializedObject so, string propName, UnityEngine.Object obj)
        {
            var p = so.FindProperty(propName);
            if (p != null) p.objectReferenceValue = obj;
        }

        #endregion
    }
}
#endif

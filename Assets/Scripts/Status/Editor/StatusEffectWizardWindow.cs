#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Status.Editor
{
    public sealed class StatusEffectWizardWindow : EditorWindow
    {
        private enum Tab
        {
            CreateNew = 0,
            EditExisting = 1
        }

        [SerializeField] private Tab _tab = Tab.CreateNew;

        private StatusEffectCatalogueSO _catalogue;
        private CharacterStatusPrimitiveDatabaseSO _primitiveDb;

        // Create
        private string _assetFolder = "Assets/Resources/Data/Status Effects/Effects";
        private CharacterStatusId _selectedId;
        private string _displayName;

        // Create: Behavior draft fields (what we write into the new asset)
        private StackMode _stackMode = StackMode.Additive;
        private int _maxStacks = 999;
        private DecayMode _decayMode = DecayMode.None;
        private int _durationTurns = 0;
        private TickTiming _tickTiming = TickTiming.None;
        private ValueType _valueType = ValueType.Flat;
        private bool _isBuff = true;

        // Edit existing
        private StatusEffectSO _selectedExisting;
        private int _selectedExistingIndex = -1;
        private SerializedObject _selectedExistingSO;
        private Vector2 _editScroll;

        // UI
        private Vector2 _createScroll;

        [MenuItem("ALWTTT/Status/Status Effect Wizard")]
        public static void Open()
        {
            var w = GetWindow<StatusEffectWizardWindow>("Status Effect Wizard");
            w.minSize = new Vector2(560, 520);
        }

        private void OnEnable()
        {
            TryAutoFindAssets();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);

            DrawHeader();

            EditorGUILayout.Space(8);
            DrawSharedAssetRefs();

            EditorGUILayout.Space(8);
            _tab = (Tab)GUILayout.Toolbar((int)_tab, new[] { "Create New", "Edit Existing" });

            EditorGUILayout.Space(8);
            switch (_tab)
            {
                case Tab.CreateNew:
                    DrawCreateTab();
                    break;

                case Tab.EditExisting:
                    DrawEditTab();
                    break;
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Status Effect Wizard", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Create & edit StatusEffectSO assets backed by the CSO.", EditorStyles.miniLabel);
        }

        private void DrawSharedAssetRefs()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                _catalogue = (StatusEffectCatalogueSO)EditorGUILayout.ObjectField(
                    "Catalogue", _catalogue, typeof(StatusEffectCatalogueSO), false);

                _primitiveDb = (CharacterStatusPrimitiveDatabaseSO)EditorGUILayout.ObjectField(
                    "CSO Primitive DB", _primitiveDb, typeof(CharacterStatusPrimitiveDatabaseSO), false);

                if (_catalogue == null)
                    EditorGUILayout.HelpBox("Assign a StatusEffectCatalogueSO to enable duplicate-prevention and existing selection.", MessageType.Info);

                if (_primitiveDb == null)
                    EditorGUILayout.HelpBox("Assign a CharacterStatusPrimitiveDatabaseSO to preview Category/Abstract Function + references.", MessageType.Info);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // CREATE TAB
        // ─────────────────────────────────────────────────────────────────────────────

        private void DrawCreateTab()
        {
            _createScroll = EditorGUILayout.BeginScrollView(_createScroll);

            EditorGUILayout.LabelField("Create a new StatusEffectSO (CSO-backed)", EditorStyles.boldLabel);

            EditorGUILayout.Space(6);
            _assetFolder = EditorGUILayout.TextField("Asset Folder", _assetFolder);

            EditorGUILayout.Space(8);
            DrawIdPickerCreate();

            EditorGUILayout.Space(8);
            _displayName = EditorGUILayout.TextField("Display Name", _displayName);

            EditorGUILayout.Space(10);
            DrawOntologyPreview(_selectedId);

            EditorGUILayout.Space(10);
            DrawBehaviorDraftEditor();

            EditorGUILayout.Space(12);
            DrawCreateButton();

            EditorGUILayout.Space(10);
            DrawQuickSelectExisting();

            EditorGUILayout.EndScrollView();
        }

        private void DrawIdPickerCreate()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("EffectId (CharacterStatusId)", EditorStyles.boldLabel);

                if (_catalogue == null)
                {
                    _selectedId = (CharacterStatusId)EditorGUILayout.EnumPopup("EffectId", _selectedId);
                    EnsureDefaultDisplayName();
                    return;
                }

                // Build a popup list of ids NOT already in the catalogue.
                var allIds = Enum.GetValues(typeof(CharacterStatusId));
                var available = new List<CharacterStatusId>(allIds.Length);

                foreach (CharacterStatusId id in allIds)
                {
                    if (!_catalogue.Contains(id))
                        available.Add(id);
                }

                if (available.Count == 0)
                {
                    EditorGUILayout.HelpBox("All CharacterStatusId values already exist in the catalogue.", MessageType.Warning);
                    return;
                }

                // Ensure selection is valid.
                if (!available.Contains(_selectedId))
                    _selectedId = available[0];

                var labels = new string[available.Count];
                for (int i = 0; i < available.Count; i++) labels[i] = available[i].ToString();

                int currentIndex = available.IndexOf(_selectedId);
                int newIndex = EditorGUILayout.Popup("EffectId", currentIndex, labels);
                _selectedId = available[newIndex];

                EnsureDefaultDisplayName();
            }
        }

        private void EnsureDefaultDisplayName()
        {
            if (string.IsNullOrWhiteSpace(_displayName))
                _displayName = _selectedId.ToString();
        }

        private void DrawOntologyPreview(CharacterStatusId id)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Ontology Preview (read-only)", EditorStyles.boldLabel);

                if (_primitiveDb == null)
                {
                    EditorGUILayout.HelpBox("Assign the CharacterStatusPrimitiveDatabaseSO to preview Category/Abstract Function.", MessageType.Info);
                    return;
                }

                if (!_primitiveDb.TryGet(id, out var entry) || entry == null)
                {
                    EditorGUILayout.HelpBox($"No primitive entry found for '{id}' in the CSO DB.", MessageType.Error);
                    return;
                }

                EditorGUILayout.LabelField("Category", entry.Category);
                EditorGUILayout.LabelField("Abstract Function");
                EditorGUILayout.HelpBox(entry.AbstractFunction ?? "", MessageType.None);

                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("References");
                EditorGUILayout.LabelField("• Slay the Spire", entry.SlayTheSpireReference);
                EditorGUILayout.LabelField("• Monster Train", entry.MonsterTrainReference);
                EditorGUILayout.LabelField("• Griftlands", entry.GriftlandsReference);
            }
        }

        private void DrawBehaviorDraftEditor()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Behavior (MVP) — values to write into the new asset", EditorStyles.boldLabel);

                _stackMode = (StackMode)EditorGUILayout.EnumPopup("Stack Mode", _stackMode);
                _maxStacks = Mathf.Max(1, EditorGUILayout.IntField("Max Stacks", _maxStacks));

                _decayMode = (DecayMode)EditorGUILayout.EnumPopup("Decay Mode", _decayMode);

                using (new EditorGUI.DisabledScope(_decayMode != DecayMode.DurationTurns))
                {
                    _durationTurns = Mathf.Max(0, EditorGUILayout.IntField("Duration Turns", _durationTurns));
                }

                _tickTiming = (TickTiming)EditorGUILayout.EnumPopup("Tick Timing", _tickTiming);
                _valueType = (ValueType)EditorGUILayout.EnumPopup("Value Type", _valueType);

                EditorGUILayout.Space(4);
                _isBuff = EditorGUILayout.Toggle("Is Buff", _isBuff);

                // Small assist hints
                if (_decayMode == DecayMode.DurationTurns && _durationTurns <= 0)
                {
                    EditorGUILayout.HelpBox("DecayMode.DurationTurns usually expects DurationTurns > 0.", MessageType.Warning);
                }

                if (_tickTiming == TickTiming.None && (_decayMode == DecayMode.LinearStacks || _decayMode == DecayMode.DurationTurns))
                {
                    EditorGUILayout.HelpBox("If the status decays, you usually want a TickTiming (e.g., EndOfTurn).", MessageType.Info);
                }
            }
        }

        private void DrawCreateButton()
        {
            using (new EditorGUI.DisabledScope(_catalogue == null))
            {
                if (GUILayout.Button("Create StatusEffectSO + Register in Catalogue", GUILayout.Height(36)))
                {
                    CreateAssetAndRegister();
                }
            }
        }

        private void DrawQuickSelectExisting()
        {
            if (_catalogue == null) return;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Quick Select (Existing)", EditorStyles.boldLabel);

                if (_catalogue.Effects == null || _catalogue.Effects.Count == 0)
                {
                    EditorGUILayout.HelpBox("Catalogue has no effects yet.", MessageType.Info);
                    return;
                }

                var effects = _catalogue.Effects;
                var labels = new string[effects.Count];
                for (int i = 0; i < effects.Count; i++)
                {
                    var e = effects[i];
                    labels[i] = e == null ? "<null>" : $"{e.EffectId} — {e.DisplayName}";
                }

                int idx = Mathf.Clamp(_selectedExistingIndex, -1, effects.Count - 1);
                int newIdx = EditorGUILayout.Popup("Pick existing", Mathf.Max(0, idx), labels);

                if (GUILayout.Button("Open in Inspector"))
                {
                    var e = effects[newIdx];
                    if (e != null)
                    {
                        EditorGUIUtility.PingObject(e);
                        Selection.activeObject = e;
                        _tab = Tab.EditExisting;
                        SetSelectedExisting(e);
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // EDIT TAB
        // ─────────────────────────────────────────────────────────────────────────────

        private void DrawEditTab()
        {
            EditorGUILayout.LabelField("Edit an existing StatusEffectSO", EditorStyles.boldLabel);

            if (_catalogue == null)
            {
                EditorGUILayout.HelpBox("Assign a Catalogue to select existing StatusEffect assets.", MessageType.Info);
                return;
            }

            DrawExistingPicker();

            if (_selectedExisting == null)
            {
                EditorGUILayout.HelpBox("Select an existing StatusEffectSO to edit.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(8);
            DrawOntologyPreview(_selectedExisting.EffectId);

            EditorGUILayout.Space(8);
            DrawSelectedExistingInspector();
        }

        private void DrawExistingPicker()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                var effects = _catalogue.Effects;

                if (effects == null || effects.Count == 0)
                {
                    EditorGUILayout.HelpBox("Catalogue has no effects.", MessageType.Warning);
                    return;
                }

                // Build labels
                var labels = new string[effects.Count];
                for (int i = 0; i < effects.Count; i++)
                {
                    var e = effects[i];
                    labels[i] = e == null ? "<null>" : $"{e.EffectId} — {e.DisplayName}";
                }

                // Initialize selection
                if (_selectedExisting == null)
                {
                    // pick first non-null
                    for (int i = 0; i < effects.Count; i++)
                    {
                        if (effects[i] != null)
                        {
                            _selectedExistingIndex = i;
                            SetSelectedExisting(effects[i]);
                            break;
                        }
                    }
                }

                // Keep index synced
                _selectedExistingIndex = Mathf.Clamp(_selectedExistingIndex, 0, effects.Count - 1);
                int newIndex = EditorGUILayout.Popup("Existing Effect", _selectedExistingIndex, labels);

                if (newIndex != _selectedExistingIndex)
                {
                    _selectedExistingIndex = newIndex;
                    SetSelectedExisting(effects[_selectedExistingIndex]);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Ping"))
                    {
                        if (_selectedExisting != null) EditorGUIUtility.PingObject(_selectedExisting);
                    }

                    if (GUILayout.Button("Select"))
                    {
                        if (_selectedExisting != null) Selection.activeObject = _selectedExisting;
                    }

                    if (GUILayout.Button("Open"))
                    {
                        if (_selectedExisting != null) AssetDatabase.OpenAsset(_selectedExisting);
                    }
                }
            }
        }

        private void SetSelectedExisting(StatusEffectSO effect)
        {
            _selectedExisting = effect;
            _selectedExistingSO = effect != null ? new SerializedObject(effect) : null;

            if (effect == null) return;

            // also update create defaults as a convenience (optional)
            _selectedId = effect.EffectId;
            _displayName = effect.DisplayName;
        }

        private void DrawSelectedExistingInspector()
        {
            if (_selectedExistingSO == null)
                _selectedExistingSO = new SerializedObject(_selectedExisting);

            _selectedExistingSO.Update();

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Edit Fields", EditorStyles.boldLabel);

                _editScroll = EditorGUILayout.BeginScrollView(_editScroll, GUILayout.MinHeight(220));

                // Draw only the fields we care about (keep Script hidden)
                DrawPropertyIfExists(_selectedExistingSO, "effectId", readOnly: true); // identity is CSO-backed; editing is dangerous
                DrawPropertyIfExists(_selectedExistingSO, "displayName");
                DrawPropertyIfExists(_selectedExistingSO, "primitiveDatabase");

                EditorGUILayout.Space(6);
                DrawPropertyIfExists(_selectedExistingSO, "stackMode");
                DrawPropertyIfExists(_selectedExistingSO, "maxStacks");
                DrawPropertyIfExists(_selectedExistingSO, "decayMode");
                DrawPropertyIfExists(_selectedExistingSO, "durationTurns");
                DrawPropertyIfExists(_selectedExistingSO, "tickTiming");
                DrawPropertyIfExists(_selectedExistingSO, "valueType");

                EditorGUILayout.Space(6);
                DrawPropertyIfExists(_selectedExistingSO, "isBuff");

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(8);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Revert"))
                    {
                        _selectedExistingSO.Update(); // discard UI
                        GUI.FocusControl(null);
                    }

                    if (GUILayout.Button("Apply"))
                    {
                        _selectedExistingSO.ApplyModifiedProperties();
                        EditorUtility.SetDirty(_selectedExisting);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }

        private static void DrawPropertyIfExists(SerializedObject so, string propName, bool readOnly = false)
        {
            var p = so.FindProperty(propName);
            if (p == null) return;

            using (new EditorGUI.DisabledScope(readOnly))
            {
                EditorGUILayout.PropertyField(p, includeChildren: true);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // CREATE IMPLEMENTATION
        // ─────────────────────────────────────────────────────────────────────────────

        private void CreateAssetAndRegister()
        {
            if (_catalogue == null)
            {
                Debug.LogError("[StatusEffectWizard] Missing catalogue.");
                return;
            }

            if (_catalogue.Contains(_selectedId))
            {
                Debug.LogError($"[StatusEffectWizard] Duplicate: catalogue already contains '{_selectedId}'.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(_assetFolder))
            {
                Debug.LogError($"[StatusEffectWizard] Folder does not exist: {_assetFolder}");
                return;
            }

            var asset = ScriptableObject.CreateInstance<StatusEffectSO>();

            // Set private fields via SerializedObject so we don't require setters.
            var so = new SerializedObject(asset);

            so.FindProperty("effectId").intValue = (int)_selectedId;
            so.FindProperty("displayName").stringValue =
                string.IsNullOrWhiteSpace(_displayName) ? _selectedId.ToString() : _displayName;

            if (_primitiveDb != null)
                so.FindProperty("primitiveDatabase").objectReferenceValue = _primitiveDb;

            // Behavior (MVP)
            SetEnumIfExists(so, "stackMode", _stackMode);
            SetIntIfExists(so, "maxStacks", Mathf.Max(1, _maxStacks));
            SetEnumIfExists(so, "decayMode", _decayMode);
            SetIntIfExists(so, "durationTurns", Mathf.Max(0, _durationTurns));
            SetEnumIfExists(so, "tickTiming", _tickTiming);
            SetEnumIfExists(so, "valueType", _valueType);
            SetBoolIfExists(so, "isBuff", _isBuff);

            so.ApplyModifiedPropertiesWithoutUndo();

            var safeName = $"StatusEffect_{_selectedId}";
            var path = AssetDatabase.GenerateUniqueAssetPath($"{_assetFolder}/{safeName}.asset");

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Register
            _catalogue.EditorTryAdd(asset);
            _catalogue.RebuildCache();

            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;

            Debug.Log($"[StatusEffectWizard] Created '{asset.name}' at '{path}' and registered in '{_catalogue.name}'.");
        }

        private static void SetEnumIfExists<TEnum>(SerializedObject so, string propName, TEnum value) where TEnum : Enum
        {
            var p = so.FindProperty(propName);
            if (p == null) return;
            p.intValue = Convert.ToInt32(value);
        }

        private static void SetIntIfExists(SerializedObject so, string propName, int value)
        {
            var p = so.FindProperty(propName);
            if (p == null) return;
            p.intValue = value;
        }

        private static void SetBoolIfExists(SerializedObject so, string propName, bool value)
        {
            var p = so.FindProperty(propName);
            if (p == null) return;
            p.boolValue = value;
        }

        private void TryAutoFindAssets()
        {
            // Auto-find 1 catalogue + 1 primitive DB if available.
            var catGuids = AssetDatabase.FindAssets("t:StatusEffectCatalogueSO");
            if (_catalogue == null && catGuids.Length > 0)
                _catalogue = AssetDatabase.LoadAssetAtPath<StatusEffectCatalogueSO>(AssetDatabase.GUIDToAssetPath(catGuids[0]));

            var dbGuids = AssetDatabase.FindAssets("t:CharacterStatusPrimitiveDatabaseSO");
            if (_primitiveDb == null && dbGuids.Length > 0)
                _primitiveDb = AssetDatabase.LoadAssetAtPath<CharacterStatusPrimitiveDatabaseSO>(AssetDatabase.GUIDToAssetPath(dbGuids[0]));
        }
    }
}
#endif

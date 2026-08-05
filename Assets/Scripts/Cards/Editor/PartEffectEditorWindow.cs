#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// AUTH-1 (D-AUTH1-0 ratified): dedicated editor window for the PartEffect
    /// asset family (InstrumentEffect, ModulationEffect, MeterEffect,
    /// TempoEffect, TonalityEffect, DensityEffect, FeelEffect, and any future
    /// PartEffect subtype — discovered via TypeCache, never a hardcoded list).
    ///
    /// Capabilities: list / filter / search all PartEffect assets, inline
    /// editing via the asset's default (or custom) inspector, Create with
    /// type + name + destination folder (default _PartEffects/), Duplicate,
    /// Delete with a reference-usage scan over
    /// CompositionCardPayload.modifierEffects, Find Usages, and Export JSON
    /// of the currently filtered list.
    ///
    /// Scope (D-AUTH1-2=A): PartEffect SO assets only. CardEffectSpec editing
    /// stays in CardEditorWindow's payload panel (specs are SerializeReference
    /// payload data, not assets).
    ///
    /// AUTH-1b revision: draggable splitter + expanding columns (long asset
    /// names were truncated and the window did not fill), Export JSON.
    /// </summary>
    public sealed class PartEffectEditorWindow : EditorWindow
    {
        private const string DefaultCreateFolder =
            "Assets/Resources/Data/Cards/Composition/_PartEffects";

        private const float LeftPanelMinWidth = 320f;
        private const float RightPanelMinWidth = 340f;
        private const float SplitterWidth = 4f;

        // Filters / search
        [SerializeField] private string _search = "";
        [SerializeField] private int _typeFilterIndex; // 0 = All
        [SerializeField] private Vector2 _leftScroll;
        [SerializeField] private Vector2 _rightScroll;

        [SerializeField, Range(0.2f, 0.8f)]
        private float _splitRatio = 0.5f;
        private bool _draggingSplitter;

        // Selection
        [SerializeField] private PartEffect _selected;

        // Create box
        [SerializeField] private bool _createOpen;
        [SerializeField] private int _createTypeIndex;
        [SerializeField] private string _createName = "";
        [SerializeField] private string _createFolder = DefaultCreateFolder;

        // Caches
        private readonly List<PartEffect> _assets = new();
        private List<Type> _concreteTypes = new();
        private string[] _typeFilterOptions = { "All" };
        private string[] _createTypeOptions = Array.Empty<string>();

        private UnityEditor.Editor _inlineEditor;
        private PartEffect _inlineEditorTarget;

        // Find Usages result (on demand, not per-frame)
        private readonly List<CompositionCardPayload> _usages = new();
        private PartEffect _usagesFor;

        [MenuItem("ALWTTT/Cards/Effect Editor", priority = 13)]
        public static void Open()
        {
            var w = GetWindow<PartEffectEditorWindow>();
            w.titleContent = new GUIContent("Effect Editor");
            w.minSize = new Vector2(
                LeftPanelMinWidth + RightPanelMinWidth + SplitterWidth, 480f);
            w.Show();
        }

        /// <summary>Cross-link entry point (AUTH-1): open and select a
        /// specific PartEffect asset, pinging it in the Project window.</summary>
        public static void OpenAndSelect(PartEffect effect)
        {
            Open();
            var w = GetWindow<PartEffectEditorWindow>();
            if (effect == null) return;

            w._selected = effect;
            Selection.activeObject = effect;
            EditorGUIUtility.PingObject(effect);
            w.Repaint();
        }

        private void OnEnable()
        {
            RefreshTypeCache();
            RefreshAssets();
        }

        private void OnDisable()
        {
            DestroyInlineEditor();
        }

        private void OnProjectChange()
        {
            RefreshAssets();
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();
            CardAuthoringNav.DrawNavStrip(CardAuthoringNav.Tool.EffectEditor);

            float totalW = position.width;

            float leftW = Mathf.Clamp(
                totalW * _splitRatio,
                LeftPanelMinWidth,
                Mathf.Max(LeftPanelMinWidth, totalW - RightPanelMinWidth - SplitterWidth));

            float rightW = Mathf.Max(
                RightPanelMinWidth, totalW - leftW - SplitterWidth);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPanel(leftW);
                DrawSplitter(totalW, leftW);
                DrawRightPanel(rightW);
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Splitter
        // ──────────────────────────────────────────────────────────────────
        private void DrawSplitter(float totalW, float leftW)
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
                    _splitRatio = Mathf.Clamp(
                        e.mousePosition.x / totalW, 0.2f, 0.8f);
                    Repaint();
                }
                e.Use();
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Toolbar
        // ──────────────────────────────────────────────────────────────────
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Type:", GUILayout.Width(38));
                _typeFilterIndex = EditorGUILayout.Popup(
                    _typeFilterIndex, _typeFilterOptions,
                    EditorStyles.toolbarPopup, GUILayout.Width(160));

                GUILayout.Space(6);
                GUILayout.Label("Search:", GUILayout.Width(48));
                _search = EditorGUILayout.TextField(
                    _search, EditorStyles.toolbarSearchField,
                    GUILayout.MinWidth(120), GUILayout.MaxWidth(280));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("New…", EditorStyles.toolbarButton, GUILayout.Width(56)))
                    _createOpen = !_createOpen;

                if (GUILayout.Button("Export JSON", EditorStyles.toolbarButton, GUILayout.Width(96)))
                    ExportFilteredJson();

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(64)))
                {
                    RefreshTypeCache();
                    RefreshAssets();
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Left panel — asset list + create box
        // ──────────────────────────────────────────────────────────────────
        private void DrawLeftPanel(float width)
        {
            using (new EditorGUILayout.VerticalScope(
                GUILayout.Width(width), GUILayout.ExpandHeight(true)))
            {
                if (_createOpen)
                    DrawCreateBox();

                var filtered = GetFilteredAssets();

                EditorGUILayout.LabelField(
                    filtered.Count == _assets.Count
                        ? $"PartEffect assets: {_assets.Count}"
                        : $"PartEffect assets: {filtered.Count} / {_assets.Count}",
                    EditorStyles.boldLabel);

                using (var s = new EditorGUILayout.ScrollViewScope(_leftScroll))
                {
                    _leftScroll = s.scrollPosition;

                    foreach (var fx in filtered)
                        DrawAssetRow(fx);

                    if (filtered.Count == 0)
                        EditorGUILayout.HelpBox(
                            "No PartEffect assets match the current filters.",
                            MessageType.Info);
                }
            }
        }

        /// <summary>Rows expand to the panel width: fixed columns only for the
        /// type badge and buttons; name + label share the remainder, so long
        /// asset names are readable (AUTH-1b).</summary>
        private void DrawAssetRow(PartEffect fx)
        {
            bool isSelected = fx == _selected;
            string typeName = fx.GetType().Name;

            using (new EditorGUILayout.HorizontalScope(
                isSelected ? EditorStyles.helpBox : GUIStyle.none))
            {
                if (GUILayout.Button(
                        isSelected ? "●" : "○",
                        EditorStyles.miniButton, GUILayout.Width(24)))
                {
                    _selected = fx;
                    GUI.FocusControl(null);
                }

                GUILayout.Label(
                    new GUIContent($"[{typeName}]", typeName),
                    EditorStyles.miniLabel, GUILayout.Width(130));

                GUILayout.Label(
                    new GUIContent(fx.name, AssetDatabase.GetAssetPath(fx)),
                    GUILayout.ExpandWidth(true), GUILayout.MinWidth(80));

                string label = SafeLabel(fx);
                GUILayout.Label(
                    new GUIContent(label, label),
                    EditorStyles.miniLabel,
                    GUILayout.ExpandWidth(true), GUILayout.MinWidth(60));

                if (GUILayout.Button("Ping", GUILayout.Width(44)))
                    EditorGUIUtility.PingObject(fx);
            }
        }

        private List<PartEffect> GetFilteredAssets()
        {
            string typeFilter =
                _typeFilterIndex > 0 && _typeFilterIndex < _typeFilterOptions.Length
                    ? _typeFilterOptions[_typeFilterIndex]
                    : null;

            var result = new List<PartEffect>();

            foreach (var fx in _assets)
            {
                if (fx == null) continue;
                if (typeFilter != null && fx.GetType().Name != typeFilter) continue;

                if (!string.IsNullOrWhiteSpace(_search) &&
                    fx.name.IndexOf(_search, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                result.Add(fx);
            }

            return result;
        }

        private void DrawCreateBox()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Create PartEffect", EditorStyles.boldLabel);

                if (_concreteTypes.Count == 0)
                {
                    EditorGUILayout.HelpBox(
                        "No concrete PartEffect types found via TypeCache.",
                        MessageType.Warning);
                    return;
                }

                _createTypeIndex = Mathf.Clamp(
                    _createTypeIndex, 0, _concreteTypes.Count - 1);

                _createTypeIndex = EditorGUILayout.Popup(
                    "Type", _createTypeIndex, _createTypeOptions);

                _createName = EditorGUILayout.TextField("Asset Name", _createName);
                _createFolder = EditorGUILayout.TextField("Folder", _createFolder);

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
            var type = _concreteTypes[_createTypeIndex];

            string folder = string.IsNullOrWhiteSpace(_createFolder)
                ? DefaultCreateFolder
                : _createFolder.Trim().Replace("\\", "/").TrimEnd('/');

            EnsureFolderExists(folder);

            string baseName = string.IsNullOrWhiteSpace(_createName)
                ? $"{type.Name}_New"
                : _createName.Trim();

            string path = AssetDatabase.GenerateUniqueAssetPath(
                $"{folder}/{baseName}.asset");

            var instance = CreateInstance(type) as PartEffect;
            if (instance == null)
            {
                Debug.LogError(
                    $"[PartEffectEditor] Failed to instantiate '{type.Name}'.");
                return;
            }

            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();

            _selected = instance;
            Selection.activeObject = instance;
            EditorGUIUtility.PingObject(instance);

            RefreshAssets();
            Debug.Log($"[PartEffectEditor] Created {type.Name}: {path}");
        }

        // ──────────────────────────────────────────────────────────────────
        // Right panel — inline inspector + asset actions
        // ──────────────────────────────────────────────────────────────────
        private void DrawRightPanel(float width)
        {
            using (new EditorGUILayout.VerticalScope(
                GUILayout.Width(width), GUILayout.ExpandHeight(true)))
            using (var s = new EditorGUILayout.ScrollViewScope(_rightScroll))
            {
                _rightScroll = s.scrollPosition;

                if (_selected == null)
                {
                    EditorGUILayout.HelpBox(
                        "Select a PartEffect from the list (or create one).",
                        MessageType.Info);
                    return;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label(
                        $"{_selected.name}  ({_selected.GetType().Name})",
                        EditorStyles.boldLabel);

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.TextField(
                            "Path", AssetDatabase.GetAssetPath(_selected));
                        EditorGUILayout.TextField("Label", SafeLabel(_selected));
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Ping"))
                            EditorGUIUtility.PingObject(_selected);

                        if (GUILayout.Button("Duplicate"))
                            DuplicateSelected();

                        if (GUILayout.Button("Find Usages"))
                            ScanUsages(_selected);

                        if (GUILayout.Button("Delete"))
                            DeleteSelected();
                    }
                }

                DrawUsagesBlock();

                EditorGUILayout.Space(6);
                GUILayout.Label("Inspector", EditorStyles.boldLabel);

                EnsureInlineEditor();
                if (_inlineEditor != null)
                    _inlineEditor.OnInspectorGUI();
            }
        }

        private void DrawUsagesBlock()
        {
            if (_usagesFor == null || _usagesFor != _selected)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(
                    $"Referenced by {_usages.Count} CompositionCardPayload(s)",
                    EditorStyles.miniBoldLabel);

                foreach (var p in _usages)
                {
                    if (p == null) continue;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(p.name, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("Ping", GUILayout.Width(44)))
                            EditorGUIUtility.PingObject(p);
                    }
                }
            }
        }

        private void DuplicateSelected()
        {
            string src = AssetDatabase.GetAssetPath(_selected);
            if (string.IsNullOrEmpty(src)) return;

            string dst = AssetDatabase.GenerateUniqueAssetPath(src);
            if (!AssetDatabase.CopyAsset(src, dst))
            {
                Debug.LogError($"[PartEffectEditor] Duplicate failed: {src}");
                return;
            }

            AssetDatabase.SaveAssets();
            var copy = AssetDatabase.LoadAssetAtPath<PartEffect>(dst);
            _selected = copy;
            Selection.activeObject = copy;
            EditorGUIUtility.PingObject(copy);
            RefreshAssets();
        }

        private void DeleteSelected()
        {
            ScanUsages(_selected);

            string path = AssetDatabase.GetAssetPath(_selected);
            string usageWarning = _usages.Count > 0
                ? $"\n\nWARNING: {_usages.Count} CompositionCardPayload(s) " +
                  "reference this effect. Deleting will leave null entries " +
                  "in their modifierEffects lists."
                : "\n\nNo CompositionCardPayload references found.";

            bool ok = EditorUtility.DisplayDialog(
                "Delete PartEffect",
                $"Delete asset?\n  {path}{usageWarning}",
                "Delete", "Cancel");

            if (!ok) return;

            if (!AssetDatabase.DeleteAsset(path))
            {
                Debug.LogError($"[PartEffectEditor] Delete failed: {path}");
                return;
            }

            _selected = null;
            _usagesFor = null;
            _usages.Clear();
            AssetDatabase.SaveAssets();
            RefreshAssets();
        }

        private void ScanUsages(PartEffect fx)
        {
            _usages.Clear();
            _usagesFor = fx;
            if (fx == null) return;

            string[] guids = AssetDatabase.FindAssets("t:CompositionCardPayload");
            foreach (var g in guids)
            {
                var p = AssetDatabase.LoadAssetAtPath<CompositionCardPayload>(
                    AssetDatabase.GUIDToAssetPath(g));
                if (p?.ModifierEffects == null) continue;

                foreach (var m in p.ModifierEffects)
                {
                    if (m == fx) { _usages.Add(p); break; }
                }
            }
        }

        /// <summary>One pass over every CompositionCardPayload, building a
        /// reverse index for the whole family (used by Export JSON).</summary>
        private static Dictionary<PartEffect, List<string>> BuildUsageIndex()
        {
            var map = new Dictionary<PartEffect, List<string>>();

            string[] guids = AssetDatabase.FindAssets("t:CompositionCardPayload");
            foreach (var g in guids)
            {
                var p = AssetDatabase.LoadAssetAtPath<CompositionCardPayload>(
                    AssetDatabase.GUIDToAssetPath(g));
                if (p?.ModifierEffects == null) continue;

                foreach (var m in p.ModifierEffects)
                {
                    if (m == null) continue;
                    if (!map.TryGetValue(m, out var list))
                    {
                        list = new List<string>();
                        map[m] = list;
                    }
                    if (!list.Contains(p.name)) list.Add(p.name);
                }
            }

            return map;
        }

        // ──────────────────────────────────────────────────────────────────
        // Export JSON (AUTH-1b) — exports the currently filtered list
        // ──────────────────────────────────────────────────────────────────
        [Serializable] private class JsonField { public string name; public string value; }

        [Serializable]
        private class JsonPartEffect
        {
            public string assetName;
            public string type;
            public string assetPath;
            public string scope;
            public string timing;
            public string label;
            public List<JsonField> fields = new();
            public List<string> usedBy = new();
        }

        [Serializable] private class WrapEffects { public List<JsonPartEffect> partEffects = new(); }

        private void ExportFilteredJson()
        {
            var filtered = GetFilteredAssets();
            if (filtered.Count == 0)
            {
                Debug.LogWarning("[PartEffectEditor] Nothing to export (filter matches 0 assets).");
                return;
            }

            string suffix = _typeFilterIndex > 0 && _typeFilterIndex < _typeFilterOptions.Length
                ? _typeFilterOptions[_typeFilterIndex]
                : "All";

            string path = EditorUtility.SaveFilePanel(
                "Export PartEffects JSON", "", $"PartEffects_{suffix}.json", "json");
            if (string.IsNullOrEmpty(path)) return;

            var usageIndex = BuildUsageIndex();
            var wrap = new WrapEffects();

            foreach (var fx in filtered)
            {
                var entry = new JsonPartEffect
                {
                    assetName = fx.name,
                    type = fx.GetType().Name,
                    assetPath = AssetDatabase.GetAssetPath(fx),
                    scope = fx.scope.ToString(),
                    timing = fx.timing.ToString(),
                    label = SafeLabel(fx)
                };

                AppendSerializedFields(entry.fields, fx);

                if (usageIndex.TryGetValue(fx, out var users))
                    entry.usedBy.AddRange(users);

                wrap.partEffects.Add(entry);
            }

            File.WriteAllText(path, JsonUtility.ToJson(wrap, true));
            Debug.Log(
                $"[PartEffectEditor] Exported {wrap.partEffects.Count} PartEffect(s) to {path}");
            EditorUtility.RevealInFinder(path);
        }

        /// <summary>Depth-1 dump of an asset's visible serialized fields.
        /// Mirrors the CardInventoryWindow detailed-print formatter (same
        /// conventions: script field skipped, arrays print size only).</summary>
        private static void AppendSerializedFields(List<JsonField> into, ScriptableObject so)
        {
            var ser = new SerializedObject(so);
            var it = ser.GetIterator();
            bool enter = true;

            while (it.NextVisible(enter))
            {
                enter = false;
                if (it.name == "m_Script") continue;
                if (it.name == "scope" || it.name == "timing") continue; // already top-level

                into.Add(new JsonField { name = it.name, value = DescribeProp(it) });
            }
        }

        private static string DescribeProp(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Integer: return p.intValue.ToString();
                case SerializedPropertyType.Boolean: return p.boolValue.ToString();
                case SerializedPropertyType.Float: return p.floatValue.ToString("0.###");
                case SerializedPropertyType.String: return p.stringValue;
                case SerializedPropertyType.Enum:
                    {
                        var names = p.enumNames;
                        int idx = p.enumValueIndex;
                        return idx >= 0 && idx < names.Length ? names[idx] : $"<enum {idx}>";
                    }
                case SerializedPropertyType.ObjectReference:
                    return p.objectReferenceValue != null
                        ? $"{p.objectReferenceValue.name} ({p.objectReferenceValue.GetType().Name})"
                        : "<null>";
                case SerializedPropertyType.Generic:
                    return p.isArray ? $"[{p.arraySize} items]" : "<struct>";
                default:
                    return $"<{p.propertyType}>";
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Caches / helpers
        // ──────────────────────────────────────────────────────────────────
        private void RefreshTypeCache()
        {
            _concreteTypes = new List<Type>();
            foreach (var t in TypeCache.GetTypesDerivedFrom<PartEffect>())
            {
                if (t.IsAbstract || t.IsGenericTypeDefinition) continue;
                _concreteTypes.Add(t);
            }
            _concreteTypes.Sort(static (a, b) =>
                string.CompareOrdinal(a.Name, b.Name));

            _createTypeOptions = new string[_concreteTypes.Count];
            var filterOpts = new List<string> { "All" };

            for (int i = 0; i < _concreteTypes.Count; i++)
            {
                _createTypeOptions[i] = _concreteTypes[i].Name;
                filterOpts.Add(_concreteTypes[i].Name);
            }

            _typeFilterOptions = filterOpts.ToArray();
            _typeFilterIndex = Mathf.Clamp(
                _typeFilterIndex, 0, _typeFilterOptions.Length - 1);
        }

        private void RefreshAssets()
        {
            _assets.Clear();
            string[] guids = AssetDatabase.FindAssets("t:PartEffect");
            foreach (var g in guids)
            {
                var a = AssetDatabase.LoadAssetAtPath<PartEffect>(
                    AssetDatabase.GUIDToAssetPath(g));
                if (a != null) _assets.Add(a);
            }
            _assets.Sort(static (a, b) =>
                string.CompareOrdinal(a.name, b.name));
        }

        private void EnsureInlineEditor()
        {
            if (_inlineEditor != null && _inlineEditorTarget == _selected)
                return;

            DestroyInlineEditor();
            if (_selected == null) return;

            _inlineEditor = UnityEditor.Editor.CreateEditor(_selected);
            _inlineEditorTarget = _selected;
        }

        private void DestroyInlineEditor()
        {
            if (_inlineEditor != null)
                DestroyImmediate(_inlineEditor);
            _inlineEditor = null;
            _inlineEditorTarget = null;
        }

        private static string SafeLabel(PartEffect fx)
        {
            try { return fx != null ? fx.GetLabel() : ""; }
            catch { return "<label error>"; }
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
#endif
// S7 / Lineup (2026-11-11) — compare and size every character at a glance (D6=A).
// Repo path: Assets/Scripts/Characters/Editor/CharacterLineupWindow.cs
// Editor-only. Menu: ALWTTT ▸ Art ▸ Character Lineup.
//
// Two views, both built from PREFAB INSTANCES that are never saved (HideFlags.DontSaveInEditor):
//   - "En fila": every musician and audience prefab on one floor, over a ruler, in the Lineup scene.
//   - "En el escenario": in the open gig scene, musicians at the BandStageLayout slots for the chosen
//     band size and an encounter's audience at the audience positions — exactly where GigManager puts them.
// Heights are edited in the window and written to each prefab (CharacterSizing.SetHeight); the
// instances update by themselves because they are linked. Nothing in the scenes is modified, so
// there are no forgotten overrides. The preview is removed before Play and when a scene closes.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Encounters;
using ALWTTT.Managers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ALWTTT.Characters.Editor
{
    [InitializeOnLoad]
    public sealed class CharacterLineupWindow : EditorWindow
    {
        private const string LineupScenePath = "Assets/Scenes/Tools/Lineup.unity";
        private const string MusicianFolder = "Assets/Prefabs/Characters/Musicians";
        private const string PreviewRootName = "[Lineup Preview]";
        private const float Gap = 0.5f;
        private const float GroupGap = 1.5f;

        private sealed class Entry
        {
            public string Label;
            public string Group;          // "Banda" / "Público"
            public GameObject Prefab;
            public string Path;
            public float Height;
        }

        private readonly List<Entry> _entries = new();
        private Vector2 _scroll;
        private int _mode;                // 0 = En fila, 1 = En el escenario
        private int _refIndex;
        private float _rulerStep = 0.5f;
        private bool _showRuler = true;

        // Stage view
        private int _bandSize = 3;
        private int _encounterIndex;
        private List<GigEncounterSO> _encounters = new();
        private readonly List<bool> _bandPick = new();

        static CharacterLineupWindow()
        {
            EditorApplication.playModeStateChanged += s =>
            {
                if (s == PlayModeStateChange.ExitingEditMode) ClearPreview();
            };
            EditorSceneManager.sceneClosing += (scene, removing) => ClearPreview();
        }

        [MenuItem("ALWTTT/Art/Character Lineup")]
        public static void Open() => GetWindow<CharacterLineupWindow>("Character Lineup");

        private bool _rebuildQueued;
        private bool _quiet;   // no log for automatic rebuilds while dragging

        private void OnEnable()
        {
            Refresh();
            SceneView.duringSceneGui += OnSceneGUI;
            Undo.postprocessModifications += OnSceneEdited;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Undo.postprocessModifications -= OnSceneEdited;
        }

        // Stage view follows the designer: dragging a slot (Duo 1, Audience Position 2…) re-places the
        // preview on the next editor tick. One rebuild per tick at most, however many modifications arrive.
        private UndoPropertyModification[] OnSceneEdited(UndoPropertyModification[] mods)
        {
            if (_mode == 1 && !_rebuildQueued && FindPreviewRoot() != null)
            {
                _rebuildQueued = true;
                EditorApplication.delayCall += () => { _rebuildQueued = false; if (FindPreviewRoot() != null) { _quiet = true; Rebuild(); _quiet = false; } };
            }
            return mods;
        }

        private void OnProjectChange() { Refresh(); Repaint(); }

        // ──────────────────────────────────────────────────────────────────
        // Data
        // ──────────────────────────────────────────────────────────────────
        private void Refresh()
        {
            _entries.Clear();

            foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { MusicianFolder }))
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (go == null || go.GetComponent<MusicianBase>() == null) continue;
                if (go.name.EndsWith(" - Base")) continue;
                _entries.Add(Make(go, p, "Banda"));
            }

            var seen = new HashSet<GameObject>();
            foreach (var g in AssetDatabase.FindAssets("t:AudienceCharacterData"))
            {
                var data = AssetDatabase.LoadAssetAtPath<AudienceCharacterData>(AssetDatabase.GUIDToAssetPath(g));
                if (data == null || data.CharacterPrefab == null) continue;
                var go = data.CharacterPrefab.gameObject;
                if (!seen.Add(go)) continue;
                _entries.Add(Make(go, AssetDatabase.GetAssetPath(go), "Público"));
            }
            _entries.Sort((a, b) => a.Group == b.Group ? string.CompareOrdinal(a.Label, b.Label) : (a.Group == "Banda" ? -1 : 1));

            int kid = _entries.FindIndex(e => e.Label == "Kid");
            if (_refIndex <= 0 || _refIndex >= _entries.Count) _refIndex = kid >= 0 ? kid : 0;

            _encounters = AssetDatabase.FindAssets("t:GigEncounterSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<GigEncounterSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(e => e != null).OrderBy(e => e.name).ToList();

            int bandCount = _entries.Count(e => e.Group == "Banda");
            while (_bandPick.Count < bandCount) _bandPick.Add(true);
        }

        private static Entry Make(GameObject go, string path, string group)
        {
            string label = go.name.Replace("AudienceCharacter - ", "").Replace("BandCharacter - ", "");
            return new Entry { Label = label, Group = group, Prefab = go, Path = path, Height = CharacterSizing.GetHeight(go) };
        }

        // ──────────────────────────────────────────────────────────────────
        // GUI
        // ──────────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            _mode = GUILayout.Toolbar(_mode, new[] { "En fila", "En el escenario" });
            EditorGUILayout.Space(4);

            if (_mode == 0) DrawRowControls(); else DrawStageControls();

            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                var labels = _entries.Select(e => e.Label).ToArray();
                _refIndex = EditorGUILayout.Popup("Referencia", Mathf.Clamp(_refIndex, 0, Mathf.Max(0, labels.Length - 1)), labels);
                if (GUILayout.Button("Recargar", GUILayout.Width(80))) { Refresh(); Rebuild(); }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                _showRuler = EditorGUILayout.ToggleLeft("Regla", _showRuler, GUILayout.Width(60));
                _rulerStep = Mathf.Max(0.1f, EditorGUILayout.FloatField("Paso", _rulerStep));
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Alturas (unidades del prefab; se escriben en el prefab)", EditorStyles.boldLabel);
            float refH = _entries.Count > 0 && _refIndex < _entries.Count ? _entries[_refIndex].Height : 1f;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            string group = null;
            foreach (var e in _entries)
            {
                if (e.Group != group) { group = e.Group; EditorGUILayout.LabelField(group, EditorStyles.miniBoldLabel); }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(e.Label, EditorStyles.label, GUILayout.Width(140))) EditorGUIUtility.PingObject(e.Prefab);
                    if (e.Height <= 0f) { EditorGUILayout.LabelField("sin hijo 'Sprite'"); continue; }

                    float h = EditorGUILayout.DelayedFloatField((float)System.Math.Round(e.Height, 2), GUILayout.Width(60));
                    if (GUILayout.Button("−", GUILayout.Width(22))) h = e.Height * 0.95f;
                    if (GUILayout.Button("+", GUILayout.Width(22))) h = e.Height * 1.05f;
                    if (GUILayout.Button("= ref", GUILayout.Width(44))) h = refH;
                    GUILayout.Label(refH > 0 ? $"×{e.Height / refH:0.00}" : "", GUILayout.Width(48));
                    if (e.Group == "Público" &&
                        GUILayout.Button(new GUIContent("Ajustar UI", "Sube intención, estados y telegrafía de Vibe a la cabeza (desde el Base)."), GUILayout.Width(74)) &&
                        CharacterSizing.FitHeadUI(e.Path))
                        EditorApplication.delayCall += Relayout;

                    if (Mathf.Abs(h - e.Height) > 0.006f &&CharacterSizing.SetHeight(e.Path, h))
                    {
                        e.Height = CharacterSizing.GetHeight(AssetDatabase.LoadAssetAtPath<GameObject>(e.Path));
                        EditorApplication.delayCall += Relayout;   // linked instances update first, then re-ground
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawRowControls()
        {
            EditorGUILayout.HelpBox("Todos los personajes sobre el mismo suelo, en la escena Lineup. " +
                "Cambia una altura abajo: se guarda en el prefab y la fila se actualiza.", MessageType.None);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Abrir escena Lineup")) OpenLineupScene();
                if (GUILayout.Button("Construir fila")) Rebuild();
                if (GUILayout.Button("Limpiar")) ClearPreview();
            }
        }

        private void DrawStageControls()
        {
            var gm = Object.FindFirstObjectByType<GigManager>();
            if (gm == null)
            {
                EditorGUILayout.HelpBox("Abre la escena del gig (3 - Gig Scene): esta vista usa las posiciones del GigManager.", MessageType.Info);
                if (GUILayout.Button("Abrir 3 - Gig Scene") && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene("Assets/Scenes/3 - Gig Scene.unity");
                return;
            }
            _bandSize = EditorGUILayout.IntSlider("Músicos", _bandSize, 1, 4);
            var band = _entries.Where(e => e.Group == "Banda").ToList();
            EditorGUILayout.LabelField("Banda (en orden; se usan los primeros marcados)", EditorStyles.miniLabel);
            using (new EditorGUILayout.HorizontalScope())
                for (int i = 0; i < band.Count; i++) _bandPick[i] = GUILayout.Toggle(_bandPick[i], band[i].Label, "Button");

            var names = _encounters.Select(e => string.IsNullOrEmpty(e.DisplayName) ? e.name : e.DisplayName).ToArray();
            _encounterIndex = EditorGUILayout.Popup("Encuentro", Mathf.Clamp(_encounterIndex, 0, Mathf.Max(0, names.Length - 1)), names);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Previsualizar")) Rebuild();
                if (GUILayout.Button("Limpiar")) ClearPreview();
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Preview
        // ──────────────────────────────────────────────────────────────────
        private static void OpenLineupScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(LineupScenePath)) { EditorSceneManager.OpenScene(LineupScenePath); return; }
            Directory.CreateDirectory(Path.GetDirectoryName(LineupScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, LineupScenePath);
        }

        private void Rebuild()
        {
            ClearPreview();
            var root = new GameObject(PreviewRootName) { hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild };
            if (_mode == 0) BuildRow(root); else BuildStage(root);
            SceneView.RepaintAll();
        }

        private void Relayout()
        {
            if (FindPreviewRoot() == null) return;
            Refresh();
            Rebuild();
        }

        private void BuildRow(GameObject root)
        {
            float x = 0f;
            string group = null;
            foreach (var e in _entries)
            {
                if (e.Height <= 0f) continue;
                if (group != null && e.Group != group) x += GroupGap;
                group = e.Group;

                float w = CharacterSizing.GetWidth(e.Prefab);
                var inst = Spawn(e.Prefab, root, Vector3.zero, 1f);
                Ground(inst, x + w * 0.5f, 0f);
                x += w + Gap;
            }
        }

        private void BuildStage(GameObject root)
        {
            var gm = Object.FindFirstObjectByType<GigManager>();
            if (gm == null) return;
            var so = new SerializedObject(gm);
            var layout = so.FindProperty("bandStageLayout").objectReferenceValue as BandStageLayout;
            var legacy = ReadTransforms(so.FindProperty("musicianPosList"));
            var audiencePos = ReadTransforms(so.FindProperty("audienceMemberPosList"));

            // Band — same priority as GigManager.BuildBand: formation/auto line → legacy list.
            var band = _entries.Where(e => e.Group == "Banda").Where((e, i) => i < _bandPick.Count && _bandPick[i]).Take(_bandSize).ToList();
            var positions = layout != null ? layout.GetSlotPositions(band.Count) : new List<Vector3>();
            string source = positions.Count >= band.Count && layout != null ? "layout" : "legacy";
            var scales = new List<float>();
            if (source == "legacy")
            {
                positions.Clear();
                for (int i = 0; i < band.Count && legacy.Count > 0; i++)
                {
                    var t = legacy[i < legacy.Count ? i : 0];
                    positions.Add(t.position); scales.Add(t.lossyScale.y);
                }
            }
            // Front-most (lowest y) drawn on top, like BandStageLayout.GetSortingOrders.
            var order = Enumerable.Range(0, positions.Count).OrderBy(i => positions[i].y).ToList();
            for (int i = 0; i < band.Count && i < positions.Count; i++)
            {
                var inst = Spawn(band[i].Prefab, root, positions[i], scales.Count > i ? scales[i] : 1f);
                SetOrder(inst, 10 - order.IndexOf(i) * 3);
            }

            // Audience of the chosen encounter at the audience slots.
            if (_encounters.Count > 0)
            {
                var enc = _encounters[Mathf.Clamp(_encounterIndex, 0, _encounters.Count - 1)];
                var members = enc.AudienceMemberList;
                for (int i = 0; i < members.Count && audiencePos.Count > 0; i++)
                {
                    if (members[i] == null || members[i].CharacterPrefab == null) continue;
                    var t = audiencePos[i < audiencePos.Count ? i : 0];
                    Spawn(members[i].CharacterPrefab.gameObject, root, t.position, t.lossyScale.y);
                }
            }
            if (!_quiet) Debug.Log($"[S7] Lineup stage preview | band={band.Count} source={source} | encounter={(_encounters.Count > 0 ? _encounters[_encounterIndex].name : "none")}");
        }

        private static List<Transform> ReadTransforms(SerializedProperty list)
        {
            var res = new List<Transform>();
            if (list == null) return res;
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue is Transform t) res.Add(t);
            return res;
        }

        private static GameObject Spawn(GameObject prefab, GameObject root, Vector3 pos, float slotScale)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.scene);
            inst.transform.SetParent(root.transform, false);
            inst.transform.position = pos;
            inst.transform.localScale = prefab.transform.localScale * slotScale;   // the slot's scale, as when parented in the gig
            foreach (var t in inst.GetComponentsInChildren<Transform>(true))
                t.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            return inst;
        }

        /// <summary>Moves the instance so its body's bottom sits on <paramref name="floorY"/>, centered on x.</summary>
        private static void Ground(GameObject inst, float centerX, float floorY)
        {
            var sr = CharacterSizing.FindSpriteRenderer(inst);
            if (sr == null) return;
            var b = sr.bounds;
            inst.transform.position += new Vector3(centerX - b.center.x, floorY - b.min.y, 0f);
        }

        private static void SetOrder(GameObject inst, int order)
        {
            var sr = CharacterSizing.FindSpriteRenderer(inst);
            if (sr != null) sr.sortingOrder = order;
        }

        private static GameObject FindPreviewRoot()
        {
            for (int s = 0; s < UnityEngine.SceneManagement.SceneManager.sceneCount; s++)
                foreach (var go in UnityEngine.SceneManagement.SceneManager.GetSceneAt(s).GetRootGameObjects())
                    if (go.name == PreviewRootName) return go;
            return null;
        }

        private static void ClearPreview()
        {
            GameObject r;
            while ((r = FindPreviewRoot()) != null) DestroyImmediate(r);
            SceneView.RepaintAll();
        }

        // ──────────────────────────────────────────────────────────────────
        // Scene view: ruler, reference line, labels
        // ──────────────────────────────────────────────────────────────────
        private void OnSceneGUI(SceneView view)
        {
            var root = FindPreviewRoot();
            if (root == null) return;

            var srs = new List<(string label, SpriteRenderer sr, GameObject inst)>();
            foreach (Transform child in root.transform)
            {
                var sr = CharacterSizing.FindSpriteRenderer(child.gameObject);
                if (sr != null) srs.Add((child.name.Replace("AudienceCharacter - ", "").Replace("BandCharacter - ", ""), sr, child.gameObject));
            }
            if (srs.Count == 0) return;

            float xMin = srs.Min(s => s.sr.bounds.min.x) - 0.5f;
            float xMax = srs.Max(s => s.sr.bounds.max.x) + 0.5f;
            float top = srs.Max(s => s.sr.bounds.max.y);

            if (_mode == 0 && _showRuler)
            {
                Handles.color = new Color(1f, 1f, 1f, 0.25f);
                for (float y = 0f; y <= top + _rulerStep; y += _rulerStep)
                {
                    Handles.DrawLine(new Vector3(xMin, y), new Vector3(xMax, y));
                    Handles.Label(new Vector3(xMin - 0.4f, y + 0.1f), y.ToString("0.0"));
                }
                if (_refIndex < _entries.Count && _entries[_refIndex].Height > 0f)
                {
                    float rh = _entries[_refIndex].Height;
                    Handles.color = new Color(1f, 0.85f, 0.2f, 0.9f);
                    Handles.DrawLine(new Vector3(xMin, rh), new Vector3(xMax, rh), 2f);
                    Handles.Label(new Vector3(xMax + 0.1f, rh), $"ref {_entries[_refIndex].Label} {rh:0.00}");
                }
            }

            var style = new GUIStyle(EditorStyles.whiteBoldLabel) { alignment = TextAnchor.LowerCenter };
            foreach (var (label, sr, inst) in srs)
            {
                var b = sr.bounds;
                float h = b.size.y;
                Handles.Label(new Vector3(b.center.x, b.max.y + 0.35f), $"{label}\n{h:0.00}", style);
                if (_mode == 1)
                {
                    Handles.color = new Color(0.3f, 0.9f, 1f, 0.6f);
                    Handles.DrawWireCube(b.center, b.size);
                }
            }
        }
    }
}

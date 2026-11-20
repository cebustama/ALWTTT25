using System.Collections.Generic;
using System.Linq;
using ALWTTT.Data;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.EditorTools
{
    /// <summary>
    /// [MUSICA-1 extra] Multiplica la vida del público (MaxVibe de cada AudienceCharacterData)
    /// por un factor y redondea al múltiplo más cercano (empates hacia arriba). Vista previa,
    /// casilla por miembro y «Aplicar» con Undo. Toca assets: se deshace con Ctrl+Z o git.
    /// </summary>
    public sealed class AudienceVibeScaler : EditorWindow
    {
        private const string Tag = "[MUSICA-1]";

        private float factor = 1.5f;
        private int step = 5;
        private int minimum = 5;
        private Vector2 scroll;
        private List<Row> rows = new List<Row>();

        private sealed class Row
        {
            public AudienceCharacterData data;
            public int current;
            public bool include = true;
        }

        [MenuItem("ALWTTT/Balance/Multiplicar vida del público (MaxVibe)")]
        private static void Open()
        {
            var w = GetWindow<AudienceVibeScaler>("Vida del público");
            w.minSize = new Vector2(420, 360);
            w.Refresh();
        }

        private void Refresh()
        {
            rows = AssetDatabase.FindAssets("t:AudienceCharacterData")
                .Select(g => AssetDatabase.LoadAssetAtPath<AudienceCharacterData>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(d => d != null)
                .Select(d => new Row { data = d, current = d.MaxVibe })
                .OrderByDescending(r => r.current)
                .ToList();
        }

        public static int Scaled(int value, float factor, int step, int minimum)
        {
            float raw = value * factor;
            int s = Mathf.Max(1, step);
            // Al múltiplo más cercano; empates (x,5) hacia arriba.
            int rounded = Mathf.FloorToInt(raw / s + 0.5f) * s;
            return Mathf.Max(minimum, rounded);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Multiplica MaxVibe («vida») de cada miembro del público y redondea " +
                "al múltiplo más cercano (empates hacia arriba). Cambia los assets.", MessageType.Info);

            factor = EditorGUILayout.Slider("Factor", factor, 0.25f, 4f);
            step = Mathf.Max(1, EditorGUILayout.IntField("Redondear a múltiplos de", step));
            minimum = Mathf.Max(1, EditorGUILayout.IntField("Mínimo", minimum));

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Releer assets")) Refresh();
                if (GUILayout.Button("Todos")) rows.ForEach(r => r.include = true);
                if (GUILayout.Button("Ninguno")) rows.ForEach(r => r.include = false);
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("", GUILayout.Width(18));
                GUILayout.Label("Miembro", EditorStyles.boldLabel);
                GUILayout.Label("Ahora", EditorStyles.boldLabel, GUILayout.Width(50));
                GUILayout.Label("Nuevo", EditorStyles.boldLabel, GUILayout.Width(50));
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var r in rows)
            {
                if (r.data == null) continue;
                using (new EditorGUILayout.HorizontalScope())
                {
                    r.include = EditorGUILayout.Toggle(r.include, GUILayout.Width(18));
                    if (GUILayout.Button(r.data.name, EditorStyles.label)) EditorGUIUtility.PingObject(r.data);
                    GUILayout.Label(r.current.ToString(), GUILayout.Width(50));
                    int next = Scaled(r.current, factor, step, minimum);
                    GUILayout.Label(r.include ? next.ToString() : "—", GUILayout.Width(50));
                }
            }
            EditorGUILayout.EndScrollView();

            int n = rows.Count(r => r.include && r.data != null);
            using (new EditorGUI.DisabledScope(n == 0))
            {
                if (GUILayout.Button($"Aplicar a {n} miembro(s)", GUILayout.Height(28))) Apply();
            }
        }

        private void Apply()
        {
            var changes = new List<string>();
            foreach (var r in rows.Where(r => r.include && r.data != null))
            {
                int next = Scaled(r.current, factor, step, minimum);
                if (next == r.current) continue;
                var so = new SerializedObject(r.data);
                var p = so.FindProperty("maxVibe");
                if (p == null) { Debug.LogWarning($"{Tag} {r.data.name}: sin campo maxVibe."); continue; }
                Undo.RecordObject(r.data, "Multiplicar MaxVibe");
                p.intValue = next;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(r.data);
                changes.Add($"{r.data.name} {r.current}→{next}");
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"{Tag} MaxVibe ×{factor:0.##} (múltiplos de {step}): " +
                (changes.Count > 0 ? string.Join(", ", changes) : "sin cambios") + ".");
            Refresh();
        }
    }
}

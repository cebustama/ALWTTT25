// S7 / ELENCO-1 (2026-11-11) — one-click BandStageLayout for the open gig scene.
// Repo path: Assets/Scripts/Characters/Editor/BandStageLayoutTools.cs
// Editor-only. Menu: ALWTTT ▸ S7 ▸ Crear colocación de la banda.
//
// Creates "Band Stage Layout" at the scene root with children Duo 1–2, Trio 1–3, Cuarteto 1–4,
// Auto Left and Auto Right (each with a colored gizmo label), fills the three formations and
// assigns GigManager.bandStageLayout. Starting positions are derived from the legacy
// musicianPosList so the dots land on the stage; the designer then drags them.
// Everything goes through Undo, so Ctrl+Z removes it.

using System.Collections.Generic;
using System.Linq;
using ALWTTT.Characters.Band;
using ALWTTT.Managers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ALWTTT.Characters.Editor
{
    public static class BandStageLayoutTools
    {
        [MenuItem("ALWTTT/S7/Crear colocación de la banda", priority = 1)]
        public static void CreateInOpenScene()
        {
            var gm = Object.FindFirstObjectByType<GigManager>();
            if (gm == null)
            {
                EditorUtility.DisplayDialog("Colocación de la banda", "Abre primero la escena «3 - Gig Scene».", "OK");
                return;
            }
            var existing = Object.FindFirstObjectByType<BandStageLayout>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                EditorUtility.DisplayDialog("Colocación de la banda",
                    "Ya hay un Band Stage Layout en la escena; lo he seleccionado.", "OK");
                return;
            }

            var gmSo = new SerializedObject(gm);
            var legacy = new List<Vector3>();
            var list = gmSo.FindProperty("musicianPosList");
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue is Transform t) legacy.Add(t.position);
            if (legacy.Count == 0) legacy.Add(Vector3.zero);

            float xL = legacy.Min(p => p.x), xR = legacy.Max(p => p.x);
            float yF = legacy.Min(p => p.y), yB = legacy.Max(p => p.y);
            if (xR - xL < 2f) { float c = (xL + xR) * 0.5f; xL = c - 2.5f; xR = c + 2.5f; }
            if (yB - yF < 0.5f) { yB = yF + 1.2f; }
            float yM = (yF + yB) * 0.5f;
            Vector3 P(float tx, float ty) => new Vector3(Mathf.Lerp(xL, xR, tx), Mathf.Lerp(yF, yB, ty), 0f);

            Undo.IncrementCurrentGroup();
            var root = new GameObject("Band Stage Layout");
            Undo.RegisterCreatedObjectUndo(root, "Crear colocación de la banda");
            var layout = Undo.AddComponent<BandStageLayout>(root);

            var duo = new[] { Slot(root, "Duo 1", P(0.35f, 0.4f), 1), Slot(root, "Duo 2", P(0.7f, 0.4f), 1) };
            var trio = new[] { Slot(root, "Trio 1", P(0.2f, 0.2f), 3), Slot(root, "Trio 2", P(0.5f, 0.8f), 3), Slot(root, "Trio 3", P(0.8f, 0.2f), 3) };
            var cuarteto = new[] { Slot(root, "Cuarteto 1", P(0.1f, 0.3f), 4), Slot(root, "Cuarteto 2", P(0.37f, 0.9f), 4),
                                   Slot(root, "Cuarteto 3", P(0.63f, 0.9f), 4), Slot(root, "Cuarteto 4", P(0.9f, 0.3f), 4) };
            var autoL = Slot(root, "Auto Left", new Vector3(xL, yM, 0f), 0);
            var autoR = Slot(root, "Auto Right", new Vector3(xR, yM, 0f), 0);

            var so = new SerializedObject(layout);
            var formations = so.FindProperty("formations");
            formations.arraySize = 3;
            Fill(formations.GetArrayElementAtIndex(0), 2, duo);
            Fill(formations.GetArrayElementAtIndex(1), 3, trio);
            Fill(formations.GetArrayElementAtIndex(2), 4, cuarteto);
            so.FindProperty("autoLeft").objectReferenceValue = autoL;
            so.FindProperty("autoRight").objectReferenceValue = autoR;
            so.ApplyModifiedProperties();

            gmSo.FindProperty("bandStageLayout").objectReferenceValue = layout;
            gmSo.ApplyModifiedProperties();

            Undo.SetCurrentGroupName("Crear colocación de la banda");
            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
            Debug.Log("[S7] Band Stage Layout created | formations=2,3,4 | wired to GigManager.bandStageLayout");
        }

        private static Transform Slot(GameObject root, string name, Vector3 pos, int labelColor)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Crear colocación de la banda");
            go.transform.SetParent(root.transform, false);
            go.transform.position = pos;
            var icon = EditorGUIUtility.IconContent($"sv_label_{labelColor}").image as Texture2D;
            if (icon != null) EditorGUIUtility.SetIconForObject(go, icon);
            return go.transform;
        }

        private static void Fill(SerializedProperty formation, int count, Transform[] slots)
        {
            formation.FindPropertyRelative("musicianCount").intValue = count;
            var s = formation.FindPropertyRelative("slots");
            s.arraySize = slots.Length;
            for (int i = 0; i < slots.Length; i++) s.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
        }
    }
}

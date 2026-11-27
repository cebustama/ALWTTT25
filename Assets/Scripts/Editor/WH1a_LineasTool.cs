// WH-1a (MidiGenPlay, 2026-11-27) — in-game listening tests for the line planner.
// Repo path: Assets/Scripts/Editor/WH1a_LineasTool.cs
// Checklist + recipes: MidiGenPlay/Documentation~/research/2026-11-26-wh1a-pruebas/pruebas-en-el-juego.md
//
// «Preparar pruebas» (re-runnable):
//   1. TEST_Chords_StyleBundle: Adopt Progression Tonality ✓ (the Roman degrees are read in the
//      mode each progression declares, so the intervals are the checklist's whatever the gig is).
//   2. Creates or overwrites the 7 test progressions WH1a_01 … WH1a_07 in Assets/Dev/TestCards/WH1a
//      (same importer as the Chord Progression Editor; literal quality mode, explicit suffixes).
// Line planner on / off and its variants are per render in the Banco de cartas (row «Line Planner»,
// recipe «chords … lines=on»), so A and B render together with «Renderizar las dos».
// Undo: git checkout Assets/Dev/TestCards/TEST_Chords_StyleBundle.asset and delete
// Assets/Dev/TestCards/WH1a/.

using System.Collections.Generic;
using MidiGenPlay;
using MidiGenPlay.Composition;
using UnityEditor;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.EditorTools
{
    public static class WH1a_LineasTool
    {
        private const string Folder = "Assets/Dev/TestCards/WH1a";
        private const string BundlePath = "Assets/Dev/TestCards/TEST_Chords_StyleBundle.asset";

        // Mode-relative Roman: the flat/sharp is applied to the degree OF THE MODE, so in Aeolian
        // III and VI are already E♭ and A♭ (iiim11, vim11 — no flat), while ♭II is D♭.
        private static readonly (string name, string roman, Tonality mode, string what)[] Tests =
        {
            ("WH1a_01", "im11 – biim11 – im11 – biim11",   Tonality.Aeolian, "F · ep. 3, a un semitono"),
            ("WH1a_02", "im11 – iiim11 – im11 – iiim11",   Tonality.Aeolian, "B · ep. 2, a una tercera menor"),
            ("WH1a_03", "im11 – vim11 – im11 – vim11",     Tonality.Aeolian, "D · ep. 1, a una tercera mayor"),
            ("WH1a_04", "im9 – biim9 – im9 – biim9",       Tonality.Aeolian, "E · semitono con novenas"),
            ("WH1a_05", "I – #IV – I – #IV",               Tonality.Ionian,  "Q · tritono"),
            ("WH1a_06", "I – V – vi – IV",                 Tonality.Ionian,  "L · pop diatónico"),
            ("WH1a_07", "iim7 – V7 – Imaj7 (2)",           Tonality.Ionian,  "M · ii–V–I con séptimas"),
        };

        [MenuItem("ALWTTT/Sesiones/WH-1a ▸ Preparar pruebas de líneas (7 progresiones)")]
        public static void Prepare()
        {
            if (!EditorUtility.DisplayDialog("WH-1a · Pruebas de líneas",
                    "1. TEST_Chords_StyleBundle: Adopt Progression Tonality ✓.\n" +
                    $"2. Crea o sobrescribe WH1a_01 … WH1a_07 en {Folder}.\n\n" +
                    "Line Planner se elige por render en el Banco de cartas (receta lines=).\n" +
                    "Los cambios quedan en git (reversibles).", "Preparar", "Cancelar"))
                return;

            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/Dev/TestCards", "WH1a");

            var bundle = AssetDatabase.LoadAssetAtPath<BackingCardConfigSO>(BundlePath);
            if (bundle == null)
            {
                Debug.LogError($"[WH-1a] No encuentro {BundlePath} (¿ALWTTT ▸ Dev ▸ Create Test Cards?).");
                return;
            }
            var s = new SerializedObject(bundle);
            s.FindProperty("adoptProgressionTonality").boolValue = true;
            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bundle);

            int ok = 0;
            foreach (var t in Tests)
                if (CreateProgression(t.name, t.roman, t.mode, t.what)) ok++;

            AssetDatabase.SaveAssets();
            Debug.Log($"[WH-1a] Listo: carta TEST con Adopt Progression Tonality, {ok}/{Tests.Length} progresiones en {Folder}. " +
                      "Pega las recetas del checklist en el Banco de cartas.");
        }

        private static bool CreateProgression(string name, string roman, Tonality mode, string what)
        {
            if (!ChordProgressionRuntimeImporter.TryParseRoman(
                    roman, TimeSignature.FourFour, 0, 1f, mode,
                    out var data, out List<string> warnings, AutoChordQualityMode.None))
            {
                Debug.LogError($"[WH-1a] {name} «{roman}» no se pudo crear: {string.Join(" | ", warnings)}");
                return false;
            }
            if (warnings.Count > 0)
                Debug.LogWarning($"[WH-1a] {name}: {string.Join(" | ", warnings)}");

            data.hideFlags = HideFlags.None; // the importer builds it DontSave
            data.name = name;
            string path = $"{Folder}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ChordProgressionData>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(data, existing);
                existing.name = name;
                existing.hideFlags = HideFlags.None;
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(data);
            }
            else
            {
                AssetDatabase.CreateAsset(data, path);
            }
            Debug.Log($"[WH-1a] {name} = «{roman}» en {mode} — {what}");
            return true;
        }
    }
}

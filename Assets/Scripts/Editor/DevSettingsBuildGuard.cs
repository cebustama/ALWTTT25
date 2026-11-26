#if UNITY_EDITOR
using System.Collections.Generic;
using ALWTTT.Data;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ALWTTT.EditorTools
{
    /// <summary>
    /// [S9c] Build guard. Before every player build, looks at every GigDevSettingsSO and
    /// stops if a test-only switch is still on: TEST cards dealt to the hand, the fixed
    /// deck (DEV-HAND-1) or the pinned song seed (MGP-REPRO-1). A demo build with any of
    /// them would ship a rigged game. Three choices: turn them off and build, cancel, or
    /// build anyway (a test build on purpose). In batch mode it only warns.
    /// </summary>
    public sealed class DevSettingsBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var found = new List<(GigDevSettingsSO so, List<string> on)>();
            foreach (var guid in AssetDatabase.FindAssets("t:GigDevSettingsSO"))
            {
                var so = AssetDatabase.LoadAssetAtPath<GigDevSettingsSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (so == null) continue;
                var on = new List<string>();
                if (so.TestCardsGiveMode != GigDevSettingsSO.TestCardsMode.Off) on.Add($"cartas TEST ({so.TestCardsGiveMode})");
                if (so.UseFixedDeck) on.Add("mazo fijo");
                if (so.UsePinnedSongSeed) on.Add($"semilla fijada ({so.PinnedSongSeed})");
                if (so.UsePinnedRunSeed) on.Add($"semilla de partida fijada ({so.PinnedRunSeed})"); // [S10]
                if (on.Count > 0) found.Add((so, on));
            }
            if (found.Count == 0)
            {
                Debug.Log("[S9c] Build: ajustes de prueba apagados (cartas TEST, mazo fijo, semilla).");
                return;
            }

            var lines = new List<string>();
            foreach (var f in found) lines.Add($"• {f.so.name}: {string.Join(", ", f.on)}");
            string summary = string.Join("\n", lines);

            if (Application.isBatchMode)
            {
                Debug.LogWarning("[S9c] Build con ajustes de prueba encendidos:\n" + summary);
                return;
            }

            int choice = EditorUtility.DisplayDialogComplex("Build con ajustes de prueba",
                "Siguen encendidos ajustes que solo sirven para probar:\n\n" + summary +
                "\n\nUna build para jugadores no debe llevarlos.",
                "Apagar y seguir", "Cancelar build", "Seguir así (build de prueba)");

            if (choice == 1)
                throw new BuildFailedException("[S9c] Build cancelada: ajustes de prueba encendidos.");

            if (choice == 0)
            {
                foreach (var f in found)
                {
                    var s = new SerializedObject(f.so);
                    s.FindProperty("testCardsMode").enumValueIndex = 0; // Off
                    s.FindProperty("useFixedDeck").boolValue = false;
                    s.FindProperty("usePinnedSongSeed").boolValue = false;
                    s.FindProperty("usePinnedRunSeed").boolValue = false; // [S10]
                    s.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(f.so);
                }
                AssetDatabase.SaveAssets();
                Debug.Log("[S9c] Build: ajustes de prueba apagados antes de construir:\n" + summary);
            }
            else
            {
                Debug.LogWarning("[S9c] Build de prueba a propósito, con:\n" + summary);
            }
        }
    }
}
#endif

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using ALWTTT.Cards;
using ALWTTT.Data;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.EditorTools
{
    /// <summary>
    /// [S5e / D2+D3] One-shot asset pass for the inspiration-economy change:
    ///
    ///  1. AUDIT     — report every CardDefinition asset with inspirationGenerated > 0
    ///                 and the inspirationCost distribution (Task 8 spot-check:
    ///                 at least one card with cost > 0 must survive per folder).
    ///  2. STRIP     — set inspirationGenerated = 0 on ALL CardDefinition assets
    ///                 (D3: does NOT touch inspirationCost). Undo-registered;
    ///                 prior values are logged so the pass is recoverable.
    ///  3. PER-LOOP  — set defaultInspirationPerLoop = 3 on every GigFlowSettingsSO
    ///                 and inspirationPerLoop = 3 on every DemoLaunchConfigSO asset
    ///                 (D2). Changing the C# field initializers alone does NOT
    ///                 update already-serialized assets — this pass does.
    ///
    /// Run order: Audit → Strip → Per-loop → Audit again to confirm.
    /// Proposed path: Assets/Scripts/Editor/S5e/S5e_InspirationEconomyTool.cs
    /// (confirm against the real editor-scripts folder before committing).
    /// </summary>
    public static class S5eInspirationEconomyTool
    {
        private const int TargetInspirationPerLoop = 3; // D2 — re-tuned at S5i

        [MenuItem("ALWTTT/S5e/1 — Audit inspiration fields (report only)")]
        public static void Audit()
        {
            var guids = AssetDatabase.FindAssets("t:CardDefinition");
            var sb = new StringBuilder();
            sb.AppendLine($"[S5e Audit] {guids.Length} CardDefinition assets found.");

            int genPositive = 0;
            var costByFolder = new Dictionary<string, (int total, int withCost)>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var def = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
                if (def == null) continue;

                var so = new SerializedObject(def);
                int gen = so.FindProperty("inspirationGenerated")?.intValue ?? 0;
                int cost = so.FindProperty("inspirationCost")?.intValue ?? 0;

                string folder = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "?";
                if (!costByFolder.TryGetValue(folder, out var agg)) agg = (0, 0);
                costByFolder[folder] = (agg.total + 1, agg.withCost + (cost > 0 ? 1 : 0));

                if (gen > 0)
                {
                    genPositive++;
                    sb.AppendLine($"  gen={gen} cost={cost}  {path}");
                }
            }

            sb.AppendLine($"[S5e Audit] {genPositive} assets with inspirationGenerated > 0.");
            sb.AppendLine("[S5e Audit] inspirationCost spot-check per folder (Task 8 — " +
                          "each catalog folder should keep ≥ 1 card with cost > 0):");
            foreach (var kv in costByFolder)
            {
                string flag = kv.Value.withCost > 0 ? "OK " : "⚠ NO SPENDER";
                sb.AppendLine($"  [{flag}] {kv.Value.withCost}/{kv.Value.total} cost>0  {kv.Key}");
            }

            Debug.Log(sb.ToString());
        }

        [MenuItem("ALWTTT/S5e/2 — Strip inspirationGenerated to 0 (Undo-able)")]
        public static void StripInspirationGenerated()
        {
            var guids = AssetDatabase.FindAssets("t:CardDefinition");
            int changed = 0;
            var sb = new StringBuilder();
            sb.AppendLine("[S5e Strip] Prior values (for recovery):");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var def = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
                if (def == null) continue;

                var so = new SerializedObject(def);
                var prop = so.FindProperty("inspirationGenerated");
                if (prop == null || prop.intValue == 0) continue;

                Undo.RecordObject(def, "S5e strip inspirationGenerated");
                sb.AppendLine($"  {path}  was gen={prop.intValue}");
                prop.intValue = 0;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(def);
                changed++;
            }

            AssetDatabase.SaveAssets();
            sb.AppendLine($"[S5e Strip] {changed} assets set to inspirationGenerated = 0. " +
                          "inspirationCost untouched (D3).");
            Debug.Log(sb.ToString());
        }

        [MenuItem("ALWTTT/S5e/3 — Set inspiration-per-loop = 3 on config assets")]
        public static void SetInspirationPerLoop()
        {
            int changed = 0;
            changed += SetIntOnAll<GigFlowSettingsSO>("defaultInspirationPerLoop",
                TargetInspirationPerLoop);
            changed += SetIntOnAll<DemoLaunchConfigSO>("inspirationPerLoop",
                TargetInspirationPerLoop);

            AssetDatabase.SaveAssets();
            Debug.Log($"[S5e PerLoop] {changed} config assets set to " +
                      $"per-loop inspiration = {TargetInspirationPerLoop} (D2).");
        }

        private static int SetIntOnAll<T>(string field, int value) where T : ScriptableObject
        {
            int changed = 0;
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset == null) continue;

                var so = new SerializedObject(asset);
                var prop = so.FindProperty(field);
                if (prop == null)
                {
                    Debug.LogWarning($"[S5e PerLoop] '{field}' not found on {path} — skipped.");
                    continue;
                }
                if (prop.intValue == value) continue;

                Undo.RecordObject(asset, "S5e inspiration per loop");
                Debug.Log($"[S5e PerLoop] {path}: {field} {prop.intValue} → {value}");
                prop.intValue = value;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(asset);
                changed++;
            }
            return changed;
        }
    }
}
#endif
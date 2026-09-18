#if UNITY_EDITOR
// Place at: Assets/Scripts/Editor/Text/CardTextDumpMenu.cs
// [TXT-4] Regression oracle for population 4. Writes, for every CardDefinition, its id, name,
// card-face text (GetDescription) and detail-modal text (GetDetailDescription), resolved against
// the glossary of ONE language, to a UTF-8 file OUTSIDE Assets/ (no import, no dirty state).
//
// Run it BEFORE the builder is replaced (ST-TXT4-0: the pre-TXT-4 builder ignores the glossary,
// so the EN dump is the current behaviour verbatim) and AFTER (the EN dump must be byte-identical;
// the ES dump must contain no raw ids). Editor tooling is the one place a glossary may be chosen
// by languageCode (authoring field, D2=B) instead of by the TooltipManager's inspector slot.
using System.IO;
using System.Linq;
using System.Text;
using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Tooltips;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.TextAuthoring
{
    internal static class CardTextDumpMenu
    {
        [MenuItem("ALWTTT/Text/Dump Card Text (EN)", priority = 60)]
        private static void DumpEn() => Dump("en");

        [MenuItem("ALWTTT/Text/Dump Card Text (ES)", priority = 61)]
        private static void DumpEs() => Dump("es");

        private static void Dump(string lang)
        {
            var glossary = AssetDatabase.FindAssets($"t:{nameof(ConceptGlossarySO)}")
                .Select(g => AssetDatabase.LoadAssetAtPath<ConceptGlossarySO>(AssetDatabase.GUIDToAssetPath(g)))
                .FirstOrDefault(g => g != null && string.Equals(g.LanguageCode?.Trim(), lang, System.StringComparison.OrdinalIgnoreCase));
            if (glossary == null)
                Debug.LogWarning($"[CardTextDump] No ConceptGlossarySO with languageCode '{lang}' — dumping with NO glossary (raw ids expected).");

            var defs = AssetDatabase.FindAssets($"t:{nameof(CardDefinition)}")
                .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(d => d != null)
                .OrderBy(d => d.Id, System.StringComparer.Ordinal)
                .ToList();

            var sb = new StringBuilder();
            GameText.SetEditorFallback(glossary);
            try
            {
                foreach (var d in defs)
                {
                    sb.Append("## ").Append(d.Id).Append('\n');
                    sb.Append("devLabel: ").Append(d.DisplayName).Append('\n');     // CardDefinition.displayName (developer label since TXT-4)
                    sb.Append("name: ").Append(GameText.CardName(d)).Append('\n');   // glossary card name (raw id before the CSV import)
                    sb.Append("face:\n").Append(d.GetDescription()).Append('\n');
                    sb.Append("detail:\n").Append(d.GetDetailDescription()).Append("\n\n");
                }
            }
            finally { GameText.SetEditorFallback(null); }

            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "CardTextDump"));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, $"CardText_{lang}_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Debug.Log($"[CardTextDump] {defs.Count} cards ({lang}) → {path}");
            EditorUtility.RevealInFinder(path);
        }
    }
}
#endif
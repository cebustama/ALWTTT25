// Place at: Assets/Scripts/Editor/GameSymbolsCoverageMenu.cs
// [CARD-FACE-2] The seventh closing check (SSoT_Game_Text section 8): sprite coverage in BOTH directions,
// plus the glyph-concept coverage nothing watched since CARD-FACE-1 (role_/scope_/timing_/mod_).
//   expected -> atlas : a name a producer may emit that the atlas lacks draws an empty box on a card.
//   atlas -> expected : a name in the atlas no producer emits is a dead cell, or a typo that hid a miss.
// The expected set is GameSymbols.AllSpriteNames (enum families) + st_<key> for every StatusEffectSO
// (asset-enumerated, via ConceptRegistryEditorIndex -- the Ability precedent). This class is ALSO the
// single source of the name list the dump menu prints, so the slice names and the check cannot drift.
using System;
using System.Collections.Generic;
using System.Linq;
using ALWTTT.TextAuthoring;
using ALWTTT.Tooltips;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.EditorTools
{
    public static class GameSymbolsCoverageMenu
    {
        public const string SpriteAssetPath = "Assets/Art/UI/Symbols/game_symbols.asset";

        /// <summary>Every sprite name a producer may emit, in DUMP ORDER: enum families first
        /// (GameSymbols.AllSpriteNames), then st_&lt;key&gt; in ordinal key order. The sheet's cell
        /// order IS this order (D-CF-9=A').</summary>
        public static List<string> ExpectedSpriteNames()
        {
            ConceptRegistryEditorIndex.Build();
            var list = new List<string>(GameSymbols.AllSpriteNames);
            foreach (var key in ConceptRegistryEditorIndex.StatusKeys.OrderBy(k => k, StringComparer.Ordinal))
                list.Add(GameSymbols.StatusName(key));
            return list;
        }

        [MenuItem("ALWTTT/Game Text/Check symbol coverage")]
        // [CARD-FACE-3] Public so `Sync sprite names from texture` can run the check as its last step:
        // a sync is judged by the same verdict as procedure step 5, not by its own log.
        public static void Check()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(SpriteAssetPath);
            if (asset == null) { Debug.LogError($"[GameSymbols] No TMP_SpriteAsset at {SpriteAssetPath}"); return; }

            var expected = ExpectedSpriteNames();
            var atlas = new HashSet<string>(asset.spriteCharacterTable.Select(c => c.name), StringComparer.Ordinal);
            var expectedSet = new HashSet<string>(expected, StringComparer.Ordinal);

            var missing = expected.Where(n => !atlas.Contains(n)).ToList();
            var extra = atlas.Where(n => !expectedSet.Contains(n)).OrderBy(n => n, StringComparer.Ordinal).ToList();

            // Glyph concepts: ids the card hover reads for a glyph, that no other coverage tracks.
            var conceptMissing = GameSymbols.AllConceptIds.Where(id => !ConceptRegistryEditorIndex.IsKnown(id)).ToList();

            string verdict = (missing.Count == 0 && extra.Count == 0 && conceptMissing.Count == 0) ? "OK" : "FAIL";
            Debug.Log($"[GameSymbols] Symbol coverage {verdict}: expected {expected.Count}, atlas {atlas.Count}.\n"
                + $"  missing in atlas ({missing.Count}): {string.Join(", ", missing)}\n"
                + $"  extra in atlas ({extra.Count}): {string.Join(", ", extra)}\n"
                + $"  glyph concepts missing in glossary ({conceptMissing.Count}): {string.Join(", ", conceptMissing)}");
        }
    }
}
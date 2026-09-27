// Place at: Assets/Scripts/Editor/GameSymbolsSyncMenu.cs
// [CARD-FACE-3] Editor-only. Copies each slice NAME from the source texture into the TMP sprite
// asset's character table, so the table can never disagree with the texture (D-CF-9=A': the slice
// names ARE the contract). It closes the gap F-CF2-2 left in SSoT_Editor_Authoring_Tools section
// 20.14 step 3: TMP fixes an entry's name when it CREATES it, so a slice named after the first
// `Update Sprite Asset` kept its birth name (game_symbols_50) and the only cure was a hand edit.
//
// How the two tables meet: a TMP_SpriteCharacter points at a TMP_SpriteGlyph by glyphIndex, and a
// TMP_SpriteGlyph keeps an OPTIONAL reference to the Sprite it was built from (TMP_SpriteGlyph.sprite,
// filled by Update Sprite Asset -- inferred from TMP's editor, verified by this menu's own report:
// a character whose glyph has no sprite reference is counted as NO SPRITE and left alone). The menu
// never invents a name. Every rename is logged old -> new; a new name no producer emits (not in
// ExpectedSpriteNames) is tagged UNEXPECTED: either a typo in the slice or a cell nothing will draw.
// The name lookup is rebuilt (UpdateLookupTables: the name setter does NOT recompute the lookup
// hashcode) and `Check symbol coverage` runs last, so the result is judged by procedure step 5.
//
// SOURCE IS PURE ASCII ON PURPOSE (F-T6-1 / F-SP1-2).
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.EditorTools
{
    public static class GameSymbolsSyncMenu
    {
        [MenuItem("ALWTTT/Game Text/Sync sprite names from texture")]
        private static void Sync()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(GameSymbolsCoverageMenu.SpriteAssetPath);
            if (asset == null)
            {
                Debug.LogError($"[GameSymbols] No TMP_SpriteAsset at {GameSymbolsCoverageMenu.SpriteAssetPath}");
                return;
            }

            var expected = new HashSet<string>(GameSymbolsCoverageMenu.ExpectedSpriteNames(), StringComparer.Ordinal);

            // glyphIndex -> glyph. The table is small (dozens), a dictionary keeps the loop O(n).
            var glyphsByIndex = new Dictionary<uint, TMP_SpriteGlyph>();
            foreach (var g in asset.spriteGlyphTable)
                glyphsByIndex[g.index] = g;

            Undo.RecordObject(asset, "Sync sprite names from texture");

            int renamed = 0, unchanged = 0, noSprite = 0, unexpected = 0;
            var log = new StringBuilder();
            var chars = asset.spriteCharacterTable;
            for (int i = 0; i < chars.Count; i++)
            {
                var c = chars[i];
                if (!glyphsByIndex.TryGetValue(c.glyphIndex, out var g) || g.sprite == null)
                {
                    noSprite++;
                    log.Append("  [NO SPRITE] ").Append(c.name).Append('\n');
                    continue;
                }

                string target = g.sprite.name;
                if (string.Equals(c.name, target, StringComparison.Ordinal))
                {
                    unchanged++;
                    continue;
                }

                bool isExpected = expected.Contains(target);
                if (!isExpected) unexpected++;
                log.Append(isExpected ? "  " : "  [UNEXPECTED] ")
                   .Append(c.name).Append(" -> ").Append(target).Append('\n');

                c.name = target;
                renamed++;
            }

            asset.UpdateLookupTables();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            Debug.Log($"[GameSymbols] Sync sprite names: {renamed} renamed, {unchanged} unchanged, "
                + $"{noSprite} without sprite reference, {unexpected} unexpected.\n" + log);

            if (noSprite > 0)
                Debug.LogWarning("[GameSymbols] Some characters have no Sprite reference on their glyph; "
                    + "their names were NOT touched. Run Update Sprite Asset first, then Sync again.");

            GameSymbolsCoverageMenu.Check();
        }
    }
}
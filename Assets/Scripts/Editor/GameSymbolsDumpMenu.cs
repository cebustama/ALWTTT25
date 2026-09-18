// [CARD-FACE-1] Editor-only. Prints GameSymbols.AllSpriteNames so the source texture's slice names
// are copied from TOOL OUTPUT, never typed from a document (F-TV-5).
using UnityEditor;
using UnityEngine;

namespace ALWTTT.EditorTools
{
    public static class GameSymbolsDumpMenu
    {
        [MenuItem("ALWTTT/Game Text/Dump sprite names (GameSymbols)")]
        private static void Dump()
            => Debug.Log("[GameSymbols] " + ALWTTT.Tooltips.GameSymbols.AllSpriteNames.Length + " names:\n"
                + string.Join("\n", ALWTTT.Tooltips.GameSymbols.AllSpriteNames));
    }
}
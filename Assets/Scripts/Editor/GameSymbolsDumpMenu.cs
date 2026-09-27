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
        {
            // [CARD-FACE-2] One list, two readers: the check and the slice names read the same method.
            var names = GameSymbolsCoverageMenu.ExpectedSpriteNames();
            Debug.Log("[GameSymbols] " + names.Count + " names (cell order):\n" + string.Join("\n", names));
        }
    }
}
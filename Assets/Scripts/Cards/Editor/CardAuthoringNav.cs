#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// AUTH-1 (D-AUTH1-3=A): shared cross-window navigation for the card
    /// authoring tool family. Each window draws the strip in its own toolbar
    /// region; the current tool's button is rendered pressed and inert.
    ///
    /// Deliberately a static utility, not a window: it extends the existing
    /// OpenAndSelect cross-link pattern (M1.1b) without adding lifecycle.
    /// </summary>
    internal static class CardAuthoringNav
    {
        internal enum Tool
        {
            CardEditor,
            EffectEditor,
            Inventory,
            DeckEditor
        }

        /// <summary>Draw the one-row nav strip. Call inside a vertical layout,
        /// typically directly under the window's main toolbar row.</summary>
        internal static void DrawNavStrip(Tool current)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Tools:", GUILayout.Width(42));

                DrawToolButton(current, Tool.CardEditor, "Card Editor",
                    static () => CardEditorWindow.Open());

                DrawToolButton(current, Tool.EffectEditor, "Effect Editor",
                    static () => PartEffectEditorWindow.Open());

                DrawToolButton(current, Tool.Inventory, "Inventory",
                    static () => CardInventoryWindow.Open());

                DrawToolButton(current, Tool.DeckEditor, "Deck Editor",
                    static () => DeckEditorWindow.Open());

                GUILayout.FlexibleSpace();
            }
        }

        private static void DrawToolButton(
            Tool current, Tool target, string label, System.Action open)
        {
            bool isCurrent = current == target;

            using (new EditorGUI.DisabledScope(isCurrent))
            {
                if (GUILayout.Toggle(isCurrent, label,
                        EditorStyles.toolbarButton, GUILayout.Width(96)) && !isCurrent)
                {
                    open?.Invoke();
                }
            }
        }
    }
}
#endif
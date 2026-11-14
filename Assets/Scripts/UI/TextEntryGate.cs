using TMPro;
using UnityEngine.EventSystems;

namespace ALWTTT.UI
{
    /// <summary>
    /// [FEEDBACK-2] "The player is typing in a text field." Debug hotkeys that read letters in
    /// Update (M = MidiGenPlay panel, D = instrument picker) ask this first, so writing
    /// "me da miedo" in the feedback box does not toggle them. Read-only, no state of its own:
    /// it asks the EventSystem which object is selected and whether its input field has focus.
    /// </summary>
    public static class TextEntryGate
    {
        public static bool IsTyping
        {
            get
            {
                var es = EventSystem.current;
                if (es == null) return false;
                var go = es.currentSelectedGameObject;
                if (go == null) return false;
                var field = go.GetComponent<TMP_InputField>();
                return field != null && field.isFocused;
            }
        }
    }
}

using MidiGenPlay;
using UnityEngine;

namespace ALWTTT
{
    public class MidiGenPlayPanelController : MonoBehaviour
    {
        [SerializeField] private GenerateMidiSongPanel panel;

        private void Update()
        {
            // [FEEDBACK-2] Typing "m" in a text field must not toggle the panel.
            if (Input.GetKeyDown(KeyCode.M) && !ALWTTT.UI.TextEntryGate.IsTyping)
            {
                panel.gameObject.SetActive(!panel.gameObject.activeSelf);
            }
        }
    }

}
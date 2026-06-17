using MidiGenPlay;
using UnityEngine;

namespace ALWTTT
{
    public class MidiGenPlayPanelController : MonoBehaviour
    {
        [SerializeField] private GenerateMidiSongPanel panel;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                panel.gameObject.SetActive(!panel.gameObject.activeSelf);
            }
        }
    }

}
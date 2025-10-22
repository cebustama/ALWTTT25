using TMPro;
using UnityEngine;

namespace ALWTTT.UI
{
    public class SongTrackElementUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text roleText; // e.g., Rhythm / Backing / ...
        [SerializeField] private TMP_Text infoText; // e.g., card/pattern name

        public void Bind(string role, string info)
        {
            if (roleText) roleText.text = $"{role}";
            if (infoText) infoText.text = $"{info}";
        }
    }
}
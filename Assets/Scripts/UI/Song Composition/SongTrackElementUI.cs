using TMPro;
using UnityEngine;

namespace ALWTTT.UI
{
    public class SongTrackElementUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text roleText; // e.g., Rhythm / Backing / ...
        [SerializeField] private TMP_Text infoText; // e.g., card/pattern name
        [SerializeField] private CanvasGroup cg;

        public void Bind(string role, string info, bool placeholder = false)
        {
            if (roleText) roleText.text = string.IsNullOrWhiteSpace(role) ? "—" : role.Trim();
            if (infoText) infoText.text = string.IsNullOrWhiteSpace(info) ? "" : info.Trim();

            if (!cg) cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            cg.alpha = placeholder ? 0.45f : 1f;
        }
    }
}
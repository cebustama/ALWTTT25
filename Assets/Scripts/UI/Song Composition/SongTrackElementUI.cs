using TMPro;
using UnityEngine;

namespace ALWTTT.UI
{
    public class SongTrackElementUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text roleText; // e.g., Rhythm / Backing
        [SerializeField] private TMP_Text infoText; // e.g., card/pattern name
        [SerializeField] private TMP_Text inspirationNextText; // [B1 / D-H3=α]
        [SerializeField] private CanvasGroup cg;

        [Header("Pending visual (D-H2=α)")]
        [SerializeField] private Color pendingColor = new Color(1f, 0.7f, 0.2f, 1f);

        // Captured the first time Bind runs so we can revert from pending tint.
        private Color _defaultRoleColor;
        private Color _defaultInfoColor;
        private bool _defaultsCaptured = false;

        private void CaptureDefaultsIfNeeded()
        {
            if (_defaultsCaptured) return;
            if (roleText) _defaultRoleColor = roleText.color;
            if (infoText) _defaultInfoColor = infoText.color;
            _defaultsCaptured = true;
        }

        public void Bind(
            string role,
            string info,
            bool placeholder = false,
            int inspirationNext = 0,
            bool pending = false)
        {
            CaptureDefaultsIfNeeded();

            if (roleText) roleText.text = string.IsNullOrWhiteSpace(role) ? "—" : role.Trim();
            if (infoText) infoText.text = string.IsNullOrWhiteSpace(info) ? "" : info.Trim();

            // [B1 / D-H3=α] Inspiration-next badge. Hidden for placeholders
            // (no track) and for tracks generating zero inspiration.
            if (inspirationNextText)
            {
                if (placeholder || inspirationNext <= 0)
                    inspirationNextText.text = "";
                else
                    inspirationNextText.text = $"+{inspirationNext}";
            }

            // [B1 / D-H1+H2=α] Pending tint. Suppressed for placeholders
            // (no track exists yet that could be "pending").
            bool effectivePending = pending && !placeholder;
            if (roleText) roleText.color = effectivePending ? pendingColor : _defaultRoleColor;
            if (infoText) infoText.color = effectivePending ? pendingColor : _defaultInfoColor;

            if (!cg) cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            cg.alpha = placeholder ? 0.45f : 1f;
        }
    }
}
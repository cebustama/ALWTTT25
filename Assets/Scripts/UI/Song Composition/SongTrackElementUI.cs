using ALWTTT.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ALWTTT.UI
{
    public class SongTrackElementUI : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
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

        // [B2 / #3] Source card for hover minicard preview. Null when placeholder
        // or when no card has populated this row yet.
        private CardDefinition _sourceCardDefinition;

        private void CaptureDefaultsIfNeeded()
        {
            if (_defaultsCaptured) return;
            if (roleText) _defaultRoleColor = roleText.color;
            if (infoText) _defaultInfoColor = infoText.color;
            _defaultsCaptured = true;
        }

        /// <summary>
        /// Bind a track row.
        /// </summary>
        /// <param name="role">Track role label (Rhythm / Backing / Melody / Harmony, or "—").</param>
        /// <param name="info">Card/pattern name or empty.</param>
        /// <param name="placeholder">No track present; render dim with no badge or tooltip.</param>
        /// <param name="inspirationNext">Per-loop inspiration generated; 0 hides badge.</param>
        /// <param name="pending">Apply pending tint (suppressed for placeholders).</param>
        /// <param name="sourceCard">
        /// [B2 / #3] Source <see cref="CardDefinition"/> for hover minicard preview.
        /// Null on placeholders or before any card has populated this slot.
        /// </param>
        public void Bind(
            string role,
            string info,
            bool placeholder = false,
            int inspirationNext = 0,
            bool pending = false,
            CardDefinition sourceCard = null)
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

            // [B2 / #3] Track the source card for hover preview. Placeholders never
            // preview anything (their "—" content is not a real card).
            _sourceCardDefinition = placeholder ? null : sourceCard;
        }

        #region [B2 / #3] Pointer hover → minicard preview

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_sourceCardDefinition == null) return;
            var ctrl = MinicardTooltipController.Instance;
            if (ctrl != null) ctrl.Show(_sourceCardDefinition);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var ctrl = MinicardTooltipController.Instance;
            if (ctrl != null) ctrl.Hide();
        }

        // If the row is destroyed while hovered (e.g., re-bind rebuilds rows),
        // ensure the preview hides; pointer-exit won't fire on destroyed objects.
        private void OnDisable()
        {
            var ctrl = MinicardTooltipController.Instance;
            if (ctrl != null) ctrl.Hide();
        }

        #endregion
    }
}
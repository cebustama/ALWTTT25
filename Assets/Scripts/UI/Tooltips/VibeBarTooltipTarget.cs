// Place at: Assets/Scripts/UI/Tooltips/VibeBarTooltipTarget.cs
using ALWTTT.Characters;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ALWTTT.Tooltips
{
    /// <summary>
    /// [BIGNUM-1r / D-BN-14=A + D-BN-15] Hover on the audience member's resistance bar
    /// shows the step-by-step Vibe breakdown, as its OWN tooltip — the bar is a bounded
    /// exception to the PRES-1 "one hover surface per character" invariant, same class
    /// as the status-icon tooltips. Unity delivers PointerEnter child-first up the
    /// parent chain, so this runs BEFORE AudienceCharacterCanvas.OnPointerEnter and
    /// can tell the canvas to stand down (NotifyBarHover) for that hover.
    /// Requires a raycast target on the bar (Slider Background Image, raycastTarget ON).
    /// Pattern: EconPipTooltipTarget (single TooltipManager source).
    /// </summary>
    public class VibeBarTooltipTarget : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Auto-resolved from parents when empty.")]
        [SerializeField] private AudienceCharacterCanvas canvas;

        private void Awake()
        {
            if (canvas == null) canvas = GetComponentInParent<AudienceCharacterCanvas>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (canvas == null) return;
            canvas.NotifyBarHover(true);
            canvas.ShowVibeTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (canvas == null) return;
            canvas.NotifyBarHover(false);
            canvas.HideVibeTooltip();
        }
    }
}
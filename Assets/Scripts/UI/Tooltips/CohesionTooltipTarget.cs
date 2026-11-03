// Place at: Assets/Scripts/UI/Tooltips/CohesionTooltipTarget.cs
using ALWTTT.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using static ALWTTT.Tooltips.GameTextTemplateIds;

namespace ALWTTT.Tooltips
{
    /// <summary>
    /// [GIG-UI-1] Hover on the cohesion hearts (COH-1). Reads PersistentGameplayData on enter,
    /// so it is always current. Needs a Graphic with raycastTarget ON on the same GameObject
    /// (a transparent Image on the CohesionPips root: the pips themselves have it OFF).
    /// The panel opens ABOVE the row, left-aligned with it, so it never covers the hearts:
    /// TooltipController places the panel's bottom-left corner at target + screenOffset, and
    /// the offset here is measured from this RectTransform's own top-left corner.
    /// Overlay canvas only (GigCanvas): world corners are already screen pixels.
    /// </summary>
    public class CohesionTooltipTarget : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Pixels between the top of the heart row and the bottom of the tooltip.")]
        [SerializeField] private float gapAbove = 8f;

        private readonly Vector3[] _corners = new Vector3[4];

        public void OnPointerEnter(PointerEventData eventData)
        {
            var tm = TooltipManager.Instance;
            var gm = GameManager.Instance;
            if (tm == null || gm == null || gm.PersistentGameplayData == null) return;

            int current = gm.PersistentGameplayData.BandCohesion;
            int max = gm.GameplayData != null ? gm.GameplayData.MaxCohesion : current;

            Vector2 offset = new Vector2(0f, gapAbove);
            if (transform is RectTransform rt)
            {
                rt.GetWorldCorners(_corners);   // 0 bottom-left, 1 top-left, 2 top-right, 3 bottom-right
                offset += (Vector2)(_corners[1] - transform.position);
            }

            tm.ShowTooltip(
                GameText.Format(UiGigCohesionBody),
                GameText.Format(UiGigCohesionHeader, current, max),
                transform, null, 0f, offset);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
        }
    }
}

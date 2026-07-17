// Place at: Assets/Scripts/UI/Tooltips/EconPipTooltipTarget.cs
using UnityEngine;
using UnityEngine.EventSystems;

namespace ALWTTT.Tooltips
{
    /// <summary>
    /// [DEMO-FIXES-A / DF-ECONTIP / D-DF-6=A] Hover tooltip on the ECON-1
    /// per-musician play-budget pips, through the existing TooltipManager
    /// pipeline (StatusIconBase pattern — single tooltip source).
    /// Auto-attached and fed by BandCharacterCanvas.UpdateTurnPlayBudget;
    /// no prefab wiring beyond the pip Image having raycastTarget ON.
    /// </summary>
    public class EconPipTooltipTarget : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        public enum PipKind { Action, Composition }

        [SerializeField] private PipKind kind = PipKind.Action;
        private int _remaining;

        public void Init(PipKind pipKind) => kind = pipKind;
        public void SetRemaining(int remaining) => _remaining = remaining;

        public void OnPointerEnter(PointerEventData eventData)
        {
            var tm = TooltipManager.Instance;
            if (tm == null) return;

            string noun = kind == PipKind.Action ? "action" : "composition";
            string header = kind == PipKind.Action ? "Action plays" : "Composition plays";
            string uses = _remaining == 1 ? "use" : "uses";

            tm.ShowTooltip(
                $"{_remaining} {noun} card {uses} left this turn.",
                header, transform, cam: null);
        }

        public void OnPointerExit(PointerEventData eventData) =>
            TooltipManager.Instance?.HideTooltip();
    }
}
// Place at: Assets/Scripts/UI/Tooltips/EconPipTooltipTarget.cs
using UnityEngine;
using UnityEngine.EventSystems;
using static ALWTTT.Tooltips.GameTextTemplateIds;

namespace ALWTTT.Tooltips
{
    /// <summary>
    /// [DEMO-FIXES-A / DF-ECONTIP / D-DF-6=A] Hover tooltip on the ECON-1
    /// per-musician play-budget pips, through the existing TooltipManager
    /// pipeline (StatusIconBase pattern, single tooltip source).
    /// Auto-attached and fed by BandCharacterCanvas.UpdateTurnPlayBudget;
    /// no prefab wiring beyond the pip Image having raycastTarget ON.
    /// [GIG-UI-1] Text from the glossary (ui_econ_*); it was hardcoded English.
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

            bool action = kind == PipKind.Action;
            string header = GameText.Format(action ? UiEconActionHeader : UiEconCompositionHeader);
            string body;
            if (_remaining <= 0)
                body = GameText.Format(action ? UiEconActionNone : UiEconCompositionNone);
            else if (_remaining == 1)
                body = GameText.Format(action ? UiEconActionOne : UiEconCompositionOne);
            else
                body = GameText.Format(action ? UiEconActionMany : UiEconCompositionMany, _remaining);

            tm.ShowTooltip(body, header, transform, cam: null);
        }

        public void OnPointerExit(PointerEventData eventData) =>
            TooltipManager.Instance?.HideTooltip();
    }
}

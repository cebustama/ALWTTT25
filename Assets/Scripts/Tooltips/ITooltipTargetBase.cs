using UnityEngine;

namespace ALWTTT.Tooltips
{
    public interface ITooltipTargetBase
    {
        void ShowTooltipInfo(TooltipManager tooltipManager, 
            string content, string header = "", 
            Transform tooltipStaticTransform = null, 
            Camera cam = null, float delayShow = 0
        );

        void HideTooltipInfo(TooltipManager tooltipManager);
    }
}
using UnityEngine;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(
        fileName = "MeterEffect_",
        menuName = "ALWTTT/Composition/Meter Effect")]
    public sealed class MeterEffect : PartEffect
    {
        [Tooltip("UI label like '4/4', '3/4', '6/8', '5/4'")]
        public string meterLabel = "4/4";
        public override string GetLabel() => $"Meter {meterLabel}";
    }
}
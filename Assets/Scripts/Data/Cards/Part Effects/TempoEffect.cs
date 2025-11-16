using UnityEngine;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(
        fileName = "TempoEffect_",
        menuName = "ALWTTT/Composition/Tempo Effect")]
    public sealed class TempoEffect : PartEffect
    {
        [Range(0.5f, 2.5f)]
        public float tempoScale = 1.0f; // 0.75, 1.25, etc.

        public override string GetLabel() => $"Tempo ×{tempoScale:0.##}";
    }
}

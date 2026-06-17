using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(
        fileName = "TempoEffect_",
        menuName = "ALWTTT/Composition/Tempo Effect")]
    public sealed class TempoEffect : PartEffect
    {
        public enum TempoEffectMode
        {
            Range,
            AbsoluteBpm,
            ScaleFactor
        }

        public TempoEffectMode mode = TempoEffectMode.Range;

        // Range
        public TempoRange tempoRange;
        public TempoRule tempoRule;

        // Absolute
        [Min(40)]
        public int absoluteBpm = 120;

        // Scale factor
        [Range(0.5f, 2.5f)]
        public float tempoScale = 1.0f; // 0.75, 1.25, etc.

        public override string GetLabel()
        {
            return mode switch
            {
                TempoEffectMode.Range => $"Tempo: {tempoRange}",
                TempoEffectMode.AbsoluteBpm => $"Tempo: {absoluteBpm} BPM",
                TempoEffectMode.ScaleFactor => $"Tempo ×{tempoScale:0.##}",
                _ => "Tempo"
            };
        }
    }
}

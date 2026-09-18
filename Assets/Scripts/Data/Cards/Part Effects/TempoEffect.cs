// Place at: Assets/Scripts/Data/Cards/Part Effects/TempoEffect.cs
// FULL REPLACEMENT (CARD-FACE-1). Built on the repo copy attached 2026-09-16 (R4).
// Behaviour-identical except GetLabel(), which is a DEVELOPER label since D-CF-5=B: the player line
// comes from PartEffectDescriptionBuilder. The file was ISO-8859 (a raw U+00D7 in the label);
// source is pure ASCII now (F-T6-1 / F-SP1-2).
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

        public override string GetLabel()   // developer label
        {
            return mode switch
            {
                TempoEffectMode.Range => $"Tempo: {tempoRange}",
                TempoEffectMode.AbsoluteBpm => $"Tempo: {absoluteBpm} BPM",
                TempoEffectMode.ScaleFactor => $"Tempo x{tempoScale:0.##}",
                _ => "Tempo"
            };
        }
    }
}
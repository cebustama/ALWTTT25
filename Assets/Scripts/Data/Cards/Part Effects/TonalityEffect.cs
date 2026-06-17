using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(
        fileName = "TonalityEffect_",
        menuName = "ALWTTT/Composition/Tonality Effect")]
    public sealed class TonalityEffect : PartEffect
    {
        public enum TonalityEffectMode
        {
            Explicit,       // use the specific Tonality set on the asset
            RandomAny,      // random from all modes (with weights)
            RandomMajorish, // random from Ionian/Lydian/Mixolydian
            RandomMinorish  // random from Dorian/Phrygian/Aeolian
        }

        [Header("Tonality Effect")]
        public TonalityEffectMode mode = TonalityEffectMode.Explicit;

        [Tooltip("Used when Mode = Explicit")]
        public Tonality tonality;

        public override string GetLabel()
        {
            return mode switch
            {
                TonalityEffectMode.Explicit =>
                    $"Mode {tonality}",

                TonalityEffectMode.RandomAny =>
                    "Random Mode",

                TonalityEffectMode.RandomMajorish =>
                    "Random Major-ish Mode",

                TonalityEffectMode.RandomMinorish =>
                    "Random Minor-ish Mode",

                _ => "Tonality Effect"
            };
        }
    }

}
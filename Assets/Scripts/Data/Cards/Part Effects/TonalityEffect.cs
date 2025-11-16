using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(
        fileName = "TonalityEffect_",
        menuName = "ALWTTT/Composition/Tonality Effect")]
    public sealed class TonalityEffect : PartEffect
    {
        public Tonality tonality;
        public override string GetLabel() => $"Mode {tonality.ToString()}";
    }

}
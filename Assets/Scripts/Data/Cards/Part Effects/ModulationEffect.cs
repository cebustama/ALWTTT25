using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;
using ScaleDegree = MidiGenPlay.MusicTheory.MusicTheory.ScaleDegree;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(
        fileName = "ModulationEffect_",
        menuName = "ALWTTT/Composition/Modulation Effect")]
    public sealed class ModulationEffect : PartEffect
    {
        public enum ModulationMode
        {
            AbsoluteKey,        // Jump directly to a specific root note (keep same mode)
            IntervalWithinScale,// Move to another scale degree of current key
            RandomAny,          // Random NoteName (ignores current scale)
            RandomWithinScale   // Random degree of current scale (optionally excluding tonic)
        }

        [Header("Mode")]
        public ModulationMode mode = ModulationMode.IntervalWithinScale;

        [Header("Absolute Key")]
        public NoteName absoluteRoot = NoteName.C;

        [Header("Scale-based")]
        public ScaleDegree targetDegree = ScaleDegree.Dominant;

        [Tooltip("When RandomWithinScale is used, avoid staying on the tonic.")]
        public bool excludeTonicOnRandomWithinScale = true;

        public override string GetLabel()
        {
            return mode switch
            {
                ModulationMode.AbsoluteKey =>
                    $"Key → {absoluteRoot}",

                ModulationMode.IntervalWithinScale =>
                    $"Mod → degree {targetDegree}",

                ModulationMode.RandomAny =>
                    "Mod → Random key",

                ModulationMode.RandomWithinScale =>
                    "Mod → Random in scale",

                _ => "Modulation"
            };
        }
    }
}



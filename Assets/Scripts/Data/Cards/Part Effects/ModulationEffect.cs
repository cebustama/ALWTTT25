using Melanchall.DryWetMidi.MusicTheory;
using MidiGenPlay.Composition;
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

        // ------------------------------------------------------------------
        // Direction (ALWTTT-MOD-DIR-2, 2026-05-22)
        // ------------------------------------------------------------------
        // One-shot directional intent for the first chord of the post-modulation
        // render. Consumed by MidiGenPlay's ChordTrackComposer via the transient
        // PartConfig.ModulationOctaveHint + PartConfig.PreviousRootNote pair
        // (see SSoT_Composer_Backing_Track.md §6).
        //
        // Default Auto preserves current voice-leading behavior bit-identically
        // for existing assets (e.g. ModulationEffect_KeyLift_Degree5.asset).
        // ------------------------------------------------------------------
        [Header("Direction (post-modulation first chord)")]
        [Tooltip(
            "One-shot directional intent for the first chord of the post-" +
            "modulation render. Auto preserves current voice-leading behavior " +
            "(minimum-distance octave). Up/Down forces the first chord above/" +
            "below the previous tonic anchor; subsequent chords voice-lead " +
            "from there. With targetDegree=Tonic (no key change), Up/Down " +
            "forces a one-octave register bump in the requested direction.")]
        public ModulationOctaveHint octaveHint = ModulationOctaveHint.Auto;

        public override string GetLabel()
        {
            string body = mode switch
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

            // D-A6=B — surface direction with a glyph suffix when non-default.
            // Auto keeps the legacy label byte-identical.
            string dirSuffix = octaveHint switch
            {
                ModulationOctaveHint.Up => " ↑",
                ModulationOctaveHint.Down => " ↓",
                _ => string.Empty
            };

            return body + dirSuffix;
        }
    }
}
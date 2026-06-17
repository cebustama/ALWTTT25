using System;
using UnityEngine;

namespace ALWTTT.Cards
{
    /// <summary>
    /// When an effect should be applied relative to playback / drafting.
    /// </summary>
    public enum ApplyTiming { Immediate = 0, OnNextLoop = 1, OnNextPartStart = 2 }

    /// <summary>
    /// Scope of an effect. TrackOnly applies to a specific musician's track;
    /// otherwise it affects the Part or the WholeSong (session-level default).
    /// </summary>
    public enum EffectScope { TrackOnly = 0, CurrentPart = 1, NextPart = 2, WholeSong = 3 }

    /// <summary>
    /// Base class for all composition effects. Extend to add new musical parameters
    /// (swing, density, syncopation, humanization, tonality shifts, etc.).
    /// </summary>
    [Serializable]
    public abstract class PartEffect : ScriptableObject
    {
        [Tooltip("Where this effect applies.")]
        public EffectScope scope = EffectScope.CurrentPart;

        [Tooltip("When to apply the effect.")]
        public ApplyTiming timing = ApplyTiming.OnNextLoop;

        /// <summary>
        /// Short UI label to show on the card and in the part inspector.
        /// </summary>
        public abstract string GetLabel();
    }

    [Serializable]
    public sealed class DensityEffect : PartEffect
    {
        [Range(0f, 1f)] public float sparsity01 = 0.5f;
        public override string GetLabel() => $"Density {(1f - sparsity01):0%}";
    }

    [Serializable]
    public sealed class FeelEffect : PartEffect
    {
        public enum FeelKind { Straight, Shuffle, Swing8, Swing16, LaidBack, PushAhead }
        public FeelKind feel = FeelKind.Straight;
        public override string GetLabel() => $"Feel {feel}";
    }
}
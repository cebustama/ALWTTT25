// Place at: Assets/Scripts/Sensory/MusicianStressHitEvent.cs
using ALWTTT.Characters.Band;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [TUT-R2 / tut_musician_breakdown bus source] Published from the single
    /// canonical receiver-side entry point for incoming musician Stress,
    /// <c>BandCharacterStats.ApplyIncomingStressWithComposure</c>, once per
    /// incoming hit (incomingStress &gt; 0). Because ALL stress sources funnel
    /// through that method (AddStressAction, CardBase co-effects, GigManager
    /// audience feedback), one publish site covers every producer — the same
    /// one-subscription rationale as StatusAppliedEvent (D-S4-SRC=A).
    ///
    /// Semantic payload only. Tutorial gate — tut_musician_breakdown: first
    /// fire with <see cref="Applied"/> &gt; 0 (an actual fortitude loss; a hit
    /// fully absorbed by Composure does not teach "your reserve depletes").
    /// </summary>
    public readonly struct MusicianStressHitEvent : ISensoryEvent
    {
        /// <summary>Receiver-side stats (fortitude pool that was hit).</summary>
        public BandCharacterStats Stats { get; }

        /// <summary>Stacks absorbed by Composure before the pool was touched.</summary>
        public int Absorbed { get; }

        /// <summary>Fortitude actually depleted this hit (post-Composure,
        /// post-Exposed amplification).</summary>
        public int Applied { get; }

        public MusicianStressHitEvent(BandCharacterStats stats, int absorbed, int applied)
        {
            Stats = stats;
            Absorbed = absorbed;
            Applied = applied;
        }
    }
}
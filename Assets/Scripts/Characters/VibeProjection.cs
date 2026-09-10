namespace ALWTTT.Characters
{
    /// <summary>
    /// [BIGNUM-1 / D-BN-2=A + D-BN-8=B] One audience member's projected song-end Vibe,
    /// step by step, in the order the payout applies it (SSoT_Scoring_and_Meters §6.1,
    /// §6.2, §7.1; gate per SSoT_Audience_and_Reactions §5.3 / SSoT_Status_Effects §5.8):
    ///
    ///     HypeBase (SongHype01 × MaxVibeFromSongHype)
    ///       × ImpressionFactor  → LAfterImpression   (§6.1, floored at 0)
    ///       × FlowMult          → LAfterFlow         (§7.1, L only)
    ///       + Sfx               → Subtotal           (§6.2, flat, after Flow)
    ///       gate: Indifferent → 0 · else × CaptivatedMult → Final
    ///     Blocked members are excluded upstream → Final = 0.
    ///
    /// Built ONLY by GigManager.BuildVibeProjection, which calls the same functions the
    /// payout calls. Pure presentation record: consumed by AudienceCharacterCanvas for
    /// the C3 number, the KO tint and the tooltip breakdown. No state, no math of its own.
    /// </summary>
    public struct VibeProjection
    {
        public VibeEffectiveness Tier;

        /// <summary>IsBlocked (tall-member obstruction). Excluded upstream at song end.</summary>
        public bool Blocked;
        /// <summary>Indifference stacks &gt; 0 on the member. Gate → 0.</summary>
        public bool Indifferent;

        /// <summary>SongHype01 × MaxVibeFromSongHype (band-wide, volatile).</summary>
        public float HypeBase;
        /// <summary>Live running mean impression for this member, clamped to [−2, +2].</summary>
        public float AvgImpression;
        /// <summary>1 + AvgImpression × 0.25 ∈ [0.5, 1.5].</summary>
        public float ImpressionFactor;
        /// <summary>round(HypeBase × ImpressionFactor), floored at 0 — the "L" part.</summary>
        public int LAfterImpression;

        public int FlowStacks;
        /// <summary>1 + FlowStacks × FlowVibeMultiplier; 1 when no Flow or L == 0.</summary>
        public float FlowMult;
        public int LAfterFlow;

        /// <summary>Banked flat venue-SFX bonus this song (same for every member).</summary>
        public int Sfx;
        /// <summary>LAfterFlow + Sfx — what is offered to the gate.</summary>
        public int Subtotal;

        public int CaptivatedStacks;
        /// <summary>1 + CaptivatedStacks × CaptivatedVibeBonusPerStack; 1 when none.</summary>
        public float CaptivatedMult;

        /// <summary>What would land at song end. Equals ApplyIncomingVibe's return value.</summary>
        public int Final;

        /// <summary>The member's remaining resistance when projected.</summary>
        public int TargetCurrentVibe;
        /// <summary>Final ≥ TargetCurrentVibe (and Final &gt; 0, not convinced): this song convinces them.</summary>
        public bool Ko;
        /// <summary>Final / TargetCurrentVibe, clamped to [0, 1]. 1 = KO.</summary>
        public float Magnitude01;
    }
}
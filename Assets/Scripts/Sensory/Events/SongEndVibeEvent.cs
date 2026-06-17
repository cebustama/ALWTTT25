using ALWTTT.Characters.Audience;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S2 D-S2-2=A] Published once per audience member that reaches song-end
    /// vibe resolution, from GigManager.RunSongVibeResolution, immediately
    /// after the canonical ApplyIncomingVibe routing. Coexists with the
    /// direct FT spawn at that site per D-S2-3=A.
    ///
    /// Scope note (matches S1 visual scope exactly): audiences filtered out
    /// inside ComputeSongVibeDeltas (zero/negative intended delta, or
    /// IsBlocked) produce neither FT nor event. Every published event has
    /// IntendedDelta &gt; 0 by construction, so event count == song-end FT
    /// count (ST-S2-2 parity).
    /// </summary>
    public readonly struct SongEndVibeEvent : ISensoryEvent
    {
        /// <summary>The audience member receiving (or being denied) vibe.</summary>
        public AudienceCharacterBase Audience { get; }

        /// <summary>Index in GigManager.CurrentAudienceCharacterList.</summary>
        public int AudienceIndex { get; }

        /// <summary>Stable id (<c>CharacterId</c>) for logging/dedup.</summary>
        public string AudienceId { get; }

        /// <summary>Per-audience delta from ComputeSongVibeDeltas
        /// (SongHype base × impressionFactor, D-F-3=β), pre-Flow.</summary>
        public int BaseDelta { get; }

        /// <summary>Post-Flow intended delta — what was sent into
        /// ApplyIncomingVibe. Always &gt; 0 for published events.</summary>
        public int IntendedDelta { get; }

        /// <summary>What ApplyIncomingVibe actually applied. 0 when blocked
        /// by Indifference (D-F-2=A).</summary>
        public int AppliedDelta { get; }

        /// <summary>Total band Flow stacks at resolution (0 = no Flow).</summary>
        public int FlowStacks { get; }

        /// <summary>Effective Flow multiplier (1.0 when FlowStacks == 0).
        /// FT string reconstruction uses this when FlowStacks &gt; 0.</summary>
        public float FlowMultiplier { get; }

        /// <summary>True when intended &gt; 0 but applied &lt;= 0, i.e. the
        /// Indifference-blocked case that renders "INDIFFERENT".</summary>
        public bool BlockedByIndifference { get; }

        public SongEndVibeEvent(
            AudienceCharacterBase audience,
            int audienceIndex,
            string audienceId,
            int baseDelta,
            int intendedDelta,
            int appliedDelta,
            int flowStacks,
            float flowMultiplier,
            bool blockedByIndifference)
        {
            Audience = audience;
            AudienceIndex = audienceIndex;
            AudienceId = audienceId;
            BaseDelta = baseDelta;
            IntendedDelta = intendedDelta;
            AppliedDelta = appliedDelta;
            FlowStacks = flowStacks;
            FlowMultiplier = flowMultiplier;
            BlockedByIndifference = blockedByIndifference;
        }
    }
}
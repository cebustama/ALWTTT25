// Place at: Assets/Scripts/Sensory/Events/AudienceVibeImpactEvent.cs
using ALWTTT.Cards;
using ALWTTT.Characters;
using ALWTTT.Characters.Audience;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [JUICE-PW D1=A] Published once PER AUDIENCE TARGET when a card's
    /// ModifyVibeSpec resolves against that target, from the effect-resolution
    /// site in <c>CardBase.ExecuteEffects</c> — i.e. strictly BEFORE
    /// <c>DeckManager.OnCardPlayed</c> fires <see cref="CardPlayedEvent"/> and
    /// before any tutorial hold-release keyed on it (beat 8 timing guarantee).
    ///
    /// This is NOT a generic MeterChangedEvent (deliberately rejected in S4 for
    /// over-firing on session-start / part-advance resets) and NOT a
    /// "FinisherPlayedEvent" (finisher-ness is a tutorial concept owned by
    /// TutorialGuidedDriver.IsFinisher). It is scoped to exactly one semantic:
    /// a card's Vibe effect landed (or was blocked) on one audience member.
    ///
    /// Semantic payload only (D-S2-4=A pattern: readonly struct, zero-alloc
    /// publish). FT text/colour lives in SensoryFtPresentation
    /// (TryBuildVibeImpactFt); the audio key mapping lives in
    /// SensorySfxPresentation (ForCardVibeImpact).
    ///
    /// S5e sign convention: positive deltas DEPLETE the target's persuasion
    /// resistance pool. Presentation renders damage-number style ("-N Vibe"),
    /// mirroring the SongEndVibeEvent surface.
    /// </summary>
    public readonly struct AudienceVibeImpactEvent : ISensoryEvent
    {
        /// <summary>The impacted audience member. FT consumers anchor on
        /// <c>Audience.TextSpawnRoot</c>; kick consumers use
        /// <c>Audience.CharacterAnimator</c> (null-guarded).</summary>
        public AudienceCharacterBase Audience { get; }

        /// <summary>Index in the gig's audience list at resolution time
        /// (-1 when not resolvable). Logging/dedup aid, not a hard key.</summary>
        public int AudienceIndex { get; }

        /// <summary>Stable id (<c>CharacterId</c>) for logging/dedup.</summary>
        public string AudienceId { get; }

        /// <summary>The card's performer (Sibi for Psychic Waves). May be null
        /// defensively; consumers null-guard.</summary>
        public CharacterBase Performer { get; }

        /// <summary>The played card's definition. Stable SO reference.</summary>
        public CardDefinition Card { get; }

        /// <summary>Authored spec amount before Flow scaling.</summary>
        public int BaseDelta { get; }

        /// <summary>Delta after Flow bonus/multiplier — the amount that was
        /// ATTEMPTED against the target (pre-Indifference).</summary>
        public int FinalDelta { get; }

        /// <summary>Amount actually applied. 0 with FinalDelta &gt; 0 means
        /// blocked by Indifference.</summary>
        public int AppliedDelta { get; }

        /// <summary>0-based position of this event within the card's AoE
        /// fan-out. Audio consumers play once by keying on 0; FT consumers
        /// use it for micro-stagger.</summary>
        public int FanoutIndex { get; }

        /// <summary>Total targets the card resolved against (fan-out size).</summary>
        public int TargetCount { get; }

        /// <summary>True when the attempt was fully gated by Indifference.</summary>
        public bool BlockedByIndifference => FinalDelta > 0 && AppliedDelta == 0;

        public AudienceVibeImpactEvent(
            AudienceCharacterBase audience,
            int audienceIndex,
            string audienceId,
            CharacterBase performer,
            CardDefinition card,
            int baseDelta,
            int finalDelta,
            int appliedDelta,
            int fanoutIndex,
            int targetCount)
        {
            Audience = audience;
            AudienceIndex = audienceIndex;
            AudienceId = audienceId;
            Performer = performer;
            Card = card;
            BaseDelta = baseDelta;
            FinalDelta = finalDelta;
            AppliedDelta = appliedDelta;
            FanoutIndex = fanoutIndex;
            TargetCount = targetCount;
        }
    }
}
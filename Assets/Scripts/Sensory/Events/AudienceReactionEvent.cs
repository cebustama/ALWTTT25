using ALWTTT.Characters.Audience;
using ALWTTT.Music;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S2 D-S2-2=A] Published once per audience member per resolved loop,
    /// from the loop-finished path in GigManager (the S1 per-audience
    /// micro-reaction site). Coexists with the direct FT spawn at that site
    /// per D-S2-3=A.
    ///
    /// Semantic payload only — FT text/color derivation lives in
    /// <see cref="SensoryFtPresentation"/>. Carries the full
    /// <see cref="LoopFeedbackContext"/> (a readonly struct; copy is cheap)
    /// so S3 SFX/animator consumers get the complete loop summary without an
    /// event-shape change (Standing Directive D1).
    /// </summary>
    public readonly struct AudienceReactionEvent : ISensoryEvent
    {
        /// <summary>The reacting audience member. Consumers needing the FT
        /// anchor read <c>Audience.TextSpawnRoot</c>.</summary>
        public AudienceCharacterBase Audience { get; }

        /// <summary>Index in GigManager.CurrentAudienceCharacterList.</summary>
        public int AudienceIndex { get; }

        /// <summary>Stable id (<c>CharacterId</c>) for logging/dedup.</summary>
        public string AudienceId { get; }

        /// <summary>Raw ResolveLoopEffect result before clamping.</summary>
        public int RawImpression { get; }

        /// <summary>Clamped impression in [-2, 2] (D-F-1=A). This is the
        /// value the S1 FT surface renders.</summary>
        public int Impression { get; }

        /// <summary>Full loop summary the impression was resolved against.</summary>
        public LoopFeedbackContext LoopContext { get; }

        public AudienceReactionEvent(
            AudienceCharacterBase audience,
            int audienceIndex,
            string audienceId,
            int rawImpression,
            int impression,
            LoopFeedbackContext loopContext)
        {
            Audience = audience;
            AudienceIndex = audienceIndex;
            AudienceId = audienceId;
            RawImpression = rawImpression;
            Impression = impression;
            LoopContext = loopContext;
        }
    }
}
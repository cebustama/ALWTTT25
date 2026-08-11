// Place at: Assets/Scripts/Sensory/Events/SpotlightRedirectEvent.cs
using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [PRES-1 / D-PRES1-2=A] Published from the Spotlight redirect hook in
    /// <c>AudienceCharacterBase.ResolveTargetsFor</c> when a single-target
    /// musician ability is redirected to the spotlit musician.
    ///
    /// Presentation only, per the Sensory Contract: publishing never alters
    /// targeting, and the publisher skips the event entirely when the redirect
    /// was a visual no-op (the default target already WAS the spotlit
    /// musician) — a "-&gt; himself" floater is noise, not information.
    ///
    /// <see cref="OriginalTarget"/> is NULL on the RandomMusician branch. This
    /// is deliberate and load-bearing: the would-have-been target does not
    /// exist until <c>Random.Range</c> is called, and calling it for
    /// presentation would consume global RNG state and shift every subsequent
    /// roll in the gig — a gameplay change wearing a presentation costume.
    /// Consumers anchor on the original when present, else on the protected
    /// musician.
    ///
    /// Semantic payload only (D-S2-4=A pattern: readonly struct, zero-alloc
    /// publish). Text/colour live in
    /// <see cref="SensoryFtPresentation.TryBuildSpotlightRedirectFt"/>.
    ///
    /// Invariant note: AllMusicians abilities are NEVER redirected
    /// (SSoT_Status_Effects §5.9), so this event can never fire for them.
    /// </summary>
    public readonly struct SpotlightRedirectEvent : ISensoryEvent
    {
        /// <summary>The audience member whose ability was redirected.</summary>
        public AudienceCharacterBase Source { get; }

        /// <summary>Who WOULD have been hit. Null on the RandomMusician
        /// branch (indeterminate without an RNG roll).</summary>
        public MusicianBase OriginalTarget { get; }

        /// <summary>The spotlit musician who absorbed the redirect.</summary>
        public MusicianBase ProtectedTarget { get; }

        public SpotlightRedirectEvent(
            AudienceCharacterBase source,
            MusicianBase originalTarget,
            MusicianBase protectedTarget)
        {
            Source = source;
            OriginalTarget = originalTarget;
            ProtectedTarget = protectedTarget;
        }
    }
}
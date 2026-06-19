using ALWTTT.Music;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S4 D-S4-BUS=B] Bus bridge of the existing C# event
    /// <c>CompositionSession.LoopFinished</c>. Published from
    /// <c>GigManager.OnCompositionLoopFinished</c> (the host-owned subscriber,
    /// co-located with the existing AudienceReactionEvent publish), once per
    /// resolved loop.
    ///
    /// Carries the full <see cref="LoopFeedbackContext"/> (readonly struct; cheap
    /// copy) so future SFX/FT consumers get the loop summary without a shape change
    /// (Standing Directive D1).
    ///
    /// Tutorial gate — beat 3 (tut_first_loop_inspiration): fires when
    /// <c>Context.InspirationGainedThisLoop &gt; 0</c>, i.e. the TRACK-derived
    /// per-loop gain (HandleLoopFinished → _perLoopInspirationCurrentPart). This is
    /// loop-scoped, so it cannot mis-fire on a card-generated inspiration gain the
    /// way a generic MeterChangedEvent(Inspiration,+) would.
    /// </summary>
    public readonly struct LoopResolvedEvent : ISensoryEvent
    {
        /// <summary>Full per-loop summary as resolved by CompositionSession.</summary>
        public LoopFeedbackContext Context { get; }

        public LoopResolvedEvent(LoopFeedbackContext context)
        {
            Context = context;
        }
    }
}

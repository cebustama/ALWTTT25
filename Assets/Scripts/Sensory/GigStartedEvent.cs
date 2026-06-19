namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S4 D-S4-SRC=A] Published once from <c>GigManager.StartGig</c> after the
    /// encounter is set up and gig inspiration is initialised, before the first
    /// player turn. Lifecycle moment surfaced on the bus so the tutorial controller
    /// keeps a single subscription surface.
    ///
    /// Tutorial gate — tut_welcome_to_gig: first fire.
    /// </summary>
    public readonly struct GigStartedEvent : ISensoryEvent
    {
        /// <summary>Songs required to win this gig (RequiredSongCount), for any
        /// consumer that wants run framing. Tutorial ignores it.</summary>
        public int RequiredSongCount { get; }

        public GigStartedEvent(int requiredSongCount)
        {
            RequiredSongCount = requiredSongCount;
        }
    }
}

namespace ALWTTT.Sensory
{
    /// <summary>[TUT-REDESIGN-B / D-TUTB-3=A] Why the gig was lost. None on a win.</summary>
    public enum GigLossCause
    {
        None = 0,
        UnconvincedAfterFinalSong,
        CohesionCollapse,
    }

    /// <summary>
    /// [S4 D-S4-SRC=A] Published from <c>GigManager.ResolveGigOutcomeAndEnd</c>
    /// (the normal-flow outcome site) immediately before WinGig()/LoseGig().
    ///
    /// Caveat: the editor Debug context-menu paths call WinGig()/LoseGig()
    /// directly and therefore do NOT publish this event. That is acceptable for
    /// the tutorial (debug-only paths). Move the publish into WinGig()/LoseGig()
    /// if those debug paths ever need to drive the tutorial.
    ///
    /// Tutorial gate — tut_first_gig_won: first fire where Won == true.
    /// </summary>
    public readonly struct GigOutcomeEvent : ISensoryEvent
    {
        /// <summary>True = win (all audience convinced), false = loss.</summary>
        public bool Won { get; }

        /// <summary>[TUT-REDESIGN-B] None when Won. Mandatory in the constructor
        /// (WINK-1 precedent: a future publisher must not compile with a default).</summary>
        public GigLossCause Cause { get; }

        public GigOutcomeEvent(bool won, GigLossCause cause)
        {
            Won = won;
            Cause = won ? GigLossCause.None : cause;
        }
    }
}

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [CROWD-SFX-1 / D-CS1=B] The crowd's verdict on one performed song. Published
    /// once per song by GigManager, a short SCALED delay after
    /// OnCompositionSessionEnded (D-CS4=A: clears the last loop's reaction jitter; a
    /// pause freezes the wait). Not published for an aborted song (no
    /// SongFeedbackContext) or a song with zero parts.
    ///
    /// Semantic payload only. GigManager decides the polarity against the
    /// GigFlowSettingsSO thresholds; SensorySfxPresentation.ForSongVerdict picks the
    /// key. Neutral verdicts are published too (Polarity 0) so logs and any future
    /// FT surface see every song; the audio adapter stays silent on them.
    /// </summary>
    public readonly struct SongVerdictEvent : ISensoryEvent
    {
        /// <summary>Mean, over members with samples, of each member's average loop
        /// impression for the song. Range [-2, 2].</summary>
        public float MeanImpression { get; }

        /// <summary>Audience members that contributed at least one loop impression.
        /// 0 → Polarity is always 0.</summary>
        public int MemberCount { get; }

        /// <summary>+1 applause, -1 boo, 0 neutral (silent).</summary>
        public int Polarity { get; }

        public SongVerdictEvent(float meanImpression, int memberCount, int polarity)
        {
            MeanImpression = meanImpression;
            MemberCount = memberCount;
            Polarity = polarity;
        }
    }
}
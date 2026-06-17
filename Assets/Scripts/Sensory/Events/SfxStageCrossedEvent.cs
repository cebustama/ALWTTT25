namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S3-audio D-SA-5] Published per upward SongHype stage crossing from
    /// GigManager.FireSongHypeStage, AFTER the direct stage VFX (D-S3-6=A) and the
    /// SFX→FlatVibe bonus. Additive (frozen shapes allow new types). Semantic only —
    /// clip selection lives in the audio consumer (SensorySfxPresentation), never
    /// here. Shape matches the §4 forward inventory: stage index + presentation tag.
    /// </summary>
    public readonly struct SfxStageCrossedEvent : ISensoryEvent
    {
        /// <summary>Crossed stage, 1..3 (1=lights, 2=smoke, 3=fire).</summary>
        public int Stage { get; }

        /// <summary>Presentation tag passed to ActivateSFX (e.g. lights/smoke/fire).</summary>
        public string SfxTag { get; }

        public SfxStageCrossedEvent(int stage, string sfxTag)
        {
            Stage = stage;
            SfxTag = sfxTag;
        }
    }
}
namespace ALWTTT.Enums
{
    /// <summary>
    /// [AUDIO-OST / D1=A] Selection key for OST (authored-clip) music tracks played by
    /// MusicDirector. Mirrors the enum-keyed audio pattern already used for SFX
    /// (AudioActionType, SensorySfxType) — type-safe call sites: Play(OstTrackId.MainMenu).
    ///
    /// None (0) is the explicit "no OST" sentinel: a scene mapped to None plays no OST,
    /// and unlisted scenes default to None (OST stops) — see MusicDirector.
    ///
    /// Grows additively as later OST contexts (Ship, ship rooms, gig pause, rewards) are
    /// authored. Authority: SSoT_Audio.md §4 (OST bus).
    /// </summary>
    public enum OstTrackId
    {
        None = 0,   // explicit "no OST" / stop sentinel
        MainMenu,   // Main Menu theme (AUDIO-OST first content)
    }
}
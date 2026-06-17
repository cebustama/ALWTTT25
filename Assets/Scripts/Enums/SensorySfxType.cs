namespace ALWTTT.Enums
{
    /// <summary>
    /// [S3-audio D-SA-6=A] Selection key for BUS-DRIVEN sensory audio, kept
    /// separate from AudioActionType (which is card-authored and exposed to the
    /// LLM card pipeline in three places — extending it would leak sensory tags
    /// into card authoring). Grows additively as S4 / Phase C surfaces gain audio;
    /// the canonical to-do list is the Design_Sensory_Contract §4 audit table.
    /// Members below cover the three surfaces S3-audio wires.
    /// </summary>
    public enum SensorySfxType
    {
        // Audience reaction (per-loop impression). Neutral (0) is intentionally
        // FT-only (the muted "…"); a sting per neutral loop would fatigue.
        ReactionPositive,   // impression >= 1
        ReactionNegative,   // impression <= -1

        // Song-end vibe conversion.
        SongEndVibe,        // applied > 0   ("+N Vibe")
        SongEndBlocked,     // intended > 0 but blocked by Indifference ("INDIFFERENT")

        // SongHype stage crossings (mirror the lights / smoke / fire VFX).
        StageCrossLights,   // stage 1
        StageCrossSmoke,    // stage 2
        StageCrossFire      // stage 3
    }
}
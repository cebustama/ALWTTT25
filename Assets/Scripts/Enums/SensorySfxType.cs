namespace ALWTTT.Enums
{
    /// <summary>
    /// [S3-audio D-SA-6=A] Selection key for BUS-DRIVEN sensory audio, kept
    /// separate from AudioActionType (which is card-authored and exposed to the
    /// LLM card pipeline in three places � extending it would leak sensory tags
    /// into card authoring). Grows additively as S4 / Phase C surfaces gain audio;
    /// the canonical to-do list is the Design_Sensory_Contract �4 audit table.
    /// Members below cover the three surfaces S3-audio wires.
    /// </summary>
    public enum SensorySfxType
    {
        // Audience reaction (per-loop impression). Neutral (0) is intentionally
        // FT-only (the muted "�"); a sting per neutral loop would fatigue.
        ReactionPositive,   // impression >= 1
        ReactionNegative,   // impression <= -1

        // Song-end vibe conversion.
        SongEndVibe,        // applied > 0   ("+N Vibe")
        SongEndBlocked,     // intended > 0 but blocked by Indifference ("INDIFFERENT")

        // SongHype stage crossings (mirror the lights / smoke / fire VFX).
        StageCrossLights,   // stage 1
        StageCrossSmoke,    // stage 2
        StageCrossFire,      // stage 3

        RewardOpened,       // [S5h] reward screen opens — end-of-gig payoff sting

        // [JUICE-PW] Card Vibe effect resolved against the audience (Psychic
        // Waves + any future ModifyVibe card). ONE sting per card play (the
        // audio adapter keys on FanoutIndex == 0), replacing the drop-time
        // AudioActionType SFX for these cards (asset authored AudioType=None
        // per D-PW-AUDIO — impact-time sting, not cast-time).
        CardVibeImpact,
        // [CROWD-SFX-1] Crowd one-shots. Played on AudioManager's crowd source (NOT
        // sfxSource) so the pause menu freezes them with the crowd bed (D-CS5=A).
        // APPENDED on purpose: SoundBankSO serializes SensorySoundEntry.type as an
        // int, so inserting a member above would silently re-key every authored entry.
        CrowdApplause,      // song verdict: mean impression >= applause threshold
        CrowdBoo,           // song verdict: mean impression <= boo threshold
        CrowdOvation,       // gig won  (GigOutcomeEvent)
        CrowdJeer,          // gig lost (GigOutcomeEvent)
    }
}
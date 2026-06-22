namespace ALWTTT.Characters
{
    /// <summary>
    /// [S5a] Qualitative read of one audience member's song-end Vibe effectiveness,
    /// derived from their live impressionFactor bucket. Pure presentation of
    /// SSoT_Scoring section 6 - adds no state. Immune covers BOTH gates: IsBlocked
    /// (tall-member obstruction) and Indifference (NegateIncomingPositive).
    /// See planning/Design_Vibe_Telegraph_v0_1.md section 4.
    /// </summary>
    public enum VibeEffectiveness
    {
        SuperEffective,
        Normal,
        NotVeryEffective,
        Immune
    }
}
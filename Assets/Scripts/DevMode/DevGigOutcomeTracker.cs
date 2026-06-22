namespace ALWTTT.DevMode
{
    /// <summary>
    /// [S5b / Item 5] Per-play-session win/loss tally for the dev Stats tab. Counts only
    /// normal-flow outcomes (driven by GigOutcomeEvent via DevModeController); the editor
    /// Debug context-menu Win/Lose paths bypass that event and are intentionally NOT
    /// counted. Static state resets on domain reload -- i.e. per play session -- which
    /// matches the "win-rate this session" intent for S5c playtest measurement.
    /// </summary>
    public static class DevGigOutcomeTracker
    {
        public static int Wins { get; private set; }
        public static int Losses { get; private set; }
        public static int Total => Wins + Losses;
        public static float WinRate01 => Total > 0 ? (float)Wins / Total : 0f;

        public static void Record(bool won)
        {
            if (won) Wins++;
            else Losses++;
        }

        public static void Reset()
        {
            Wins = 0;
            Losses = 0;
        }
    }
}
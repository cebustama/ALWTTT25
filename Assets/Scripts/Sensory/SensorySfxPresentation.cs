using ALWTTT.Enums;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S3-audio D-SA-3] Single source of truth for mapping sensory EVENTS to a
    /// SensorySfxType audio key — the audio analogue of SensoryFtPresentation (the
    /// same events map to FT text/colour there). Selection only; the
    /// SensorySfxType→AudioClip binding lives in SoundBankSO. Returns null when a
    /// surface is intentionally silent (e.g. neutral reaction stays FT-only).
    /// </summary>
    public static class SensorySfxPresentation
    {
        /// <summary>[S5h] Reward-screen-open sting.</summary>
        public static SensorySfxType? ForRewardOpened() => SensorySfxType.RewardOpened;

        /// <summary>Reaction sting by clamped impression [-2..2]. Neutral (0)
        /// returns null — the muted "…" FT carries it.</summary>
        public static SensorySfxType? ForReaction(int impression)
        {
            if (impression >= 1) return SensorySfxType.ReactionPositive;
            if (impression <= -1) return SensorySfxType.ReactionNegative;
            return null;
        }

        /// <summary>Song-end sting: landed vibe vs Indifference-blocked. Mirrors the
        /// SensoryFtPresentation.TryBuildSongEndVibeFt branch logic.</summary>
        public static SensorySfxType? ForSongEnd(in SongEndVibeEvent e)
        {
            if (e.AppliedDelta > 0) return SensorySfxType.SongEndVibe;
            if (e.IntendedDelta > 0) return SensorySfxType.SongEndBlocked;
            return null;
        }

        /// <summary>Stage-crossing sting by stage (1=lights, 2=smoke, 3=fire).</summary>
        public static SensorySfxType? ForStageCross(int stage)
        {
            switch (stage)
            {
                case 1: return SensorySfxType.StageCrossLights;
                case 2: return SensorySfxType.StageCrossSmoke;
                case 3: return SensorySfxType.StageCrossFire;
                default: return null;
            }
        }
    }
}
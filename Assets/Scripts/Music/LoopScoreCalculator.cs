using UnityEngine;

namespace ALWTTT.Music
{
    /// <summary>
    /// Pure scoring helpers for a finished loop.
    /// Turns a LoopFeedbackContext into numeric scores:
    /// - LoopScore: "how good was this loop?"
    /// - HypeDelta: "how much should SongHype move because of it?"
    /// </summary>
    public static class LoopScoreCalculator
    {
        /// <summary>
        /// Main quality metric for a loop. Uses the current arrangement snapshot
        /// (roles + complexity) from LoopFeedbackContext.
        /// </summary>
        public static float ComputeLoopScore(in LoopFeedbackContext ctx)
        {
            // TODO: Rethink all of this, maybe use maxScore for normalization?

            float score = 0f;

            // 1) Base density: more active tracks = richer sound.
            score += ctx.ActiveTracks * 1.5f;

            // 2) Core roles: drums, bass, harmony, melody
            if (ctx.HasRhythm) score += 3f;
            if (ctx.HasBass) score += 3f;
            if (ctx.HasHarmony) score += 3f;
            if (ctx.HasMelody) score += 3f;

            // 3) Slight bonus if it's the last loop of the part (climax).
            if (ctx.IsLastLoopOfPart)
                score += 2f;

            // 4) Complexity: we treat InspirationGenerated as "complexity value".
            //    Small bonus – we don't want complexity to dominate by itself.
            score += 0.5f * ctx.TotalComplexity;

            // TODO (future passes):
            //  - Add synergy bonuses based on ctx.Tracks + SynergyType.
            //  - Subtract "error" penalties using musician stats & stress.
            //  - Penalize heavy repetition by comparing with previous loops.

            return score;
        }

        /// <summary>
        /// Piecewise mapping from a LoopScore to a SongHype delta.
        /// These thresholds are just a first tuning pass and can be tweaked in playtests.
        /// </summary>
        public static float ComputeHypeDelta(float loopScore)
        {
            if (loopScore >= 25f) return 15f;   // amazing loop, big hype jump
            if (loopScore >= 15f) return 8f;   // very good
            if (loopScore >= 5f) return 3f;   // decent
            if (loopScore > -5f) return 0f;   // basically neutral
            if (loopScore > -15f) return -5f;   // meh / slightly bad
            return -12f;                        // really bad loop
        }
    }
}
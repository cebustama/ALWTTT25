using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALWTTT.Music
{
    // ── Scoring mode ──────────────────────────────────────────────

    public enum LoopScoringMode
    {
        /// <summary>
        /// Distributes the role budget based on how many of the band's
        /// possible roles are filled this loop.
        /// pointsAwarded = roleBudget × (filledRoles / possibleRoles)
        /// </summary>
        RoleNormalization,

        /// <summary>
        /// Distributes the role budget based on how many of the band's
        /// musicians are actively playing this loop.
        /// pointsAwarded = roleBudget × (playingMusicians / totalMusicians)
        /// </summary>
        MusicianParticipation
    }

    // ── Scoring config ────────────────────────────────────────────

    [Serializable]
    public struct LoopScoringConfig
    {
        [Header("Mode")]
        [Tooltip("RoleNormalization: rewards filling distinct roles. " +
                 "MusicianParticipation: rewards having more musicians active.")]
        public LoopScoringMode mode;

        [Header("Point budgets")]
        [Tooltip("Total points distributed across roles or musicians.")]
        public float roleBudget;

        [Tooltip("Points per active track (density).")]
        public float densityBonusPerTrack;

        [Tooltip("Bonus points for being the last loop of a part.")]
        public float lastLoopBonus;

        [Tooltip("Points per unit of complexity (InspirationGenerated).")]
        public float complexityMultiplier;

        [Header("Band context (set at gig start)")]
        [Tooltip("How many distinct roles the band's deck can produce. " +
                 "Auto-detected from composition cards at gig start.")]
        public int possibleRoleCount;

        [Tooltip("Total musicians in the band. " +
                 "Auto-detected from band roster at gig start.")]
        public int totalMusicians;

        public static LoopScoringConfig Default => new LoopScoringConfig
        {
            mode = LoopScoringMode.RoleNormalization,
            roleBudget = 12f,
            densityBonusPerTrack = 1.5f,
            lastLoopBonus = 2f,
            complexityMultiplier = 0.5f,
            possibleRoleCount = 4,
            totalMusicians = 2
        };
    }

    // ── Hype thresholds ───────────────────────────────────────────

    [Serializable]
    public struct HypeThresholds
    {
        [Header("Score thresholds (evaluated highest first)")]
        public float amazing;
        public float veryGood;
        public float decent;
        public float neutralFloor;
        public float mehFloor;

        [Header("Corresponding SongHype deltas")]
        public float amazingDelta;
        public float veryGoodDelta;
        public float decentDelta;
        public float neutralDelta;
        public float mehDelta;
        public float badDelta;

        public static HypeThresholds Default => new HypeThresholds
        {
            amazing = 25f,
            veryGood = 15f,
            decent = 5f,
            neutralFloor = -5f,
            mehFloor = -15f,
            amazingDelta = 15f,
            veryGoodDelta = 8f,
            decentDelta = 3f,
            neutralDelta = 0f,
            mehDelta = -5f,
            badDelta = -12f
        };
    }

    // ── Calculator ────────────────────────────────────────────────

    /// <summary>
    /// Pure scoring helpers for a finished loop.
    /// Turns a LoopFeedbackContext + config into numeric scores.
    /// </summary>
    public static class LoopScoreCalculator
    {
        /// <summary>
        /// Main quality metric for a loop.
        /// </summary>
        public static float ComputeLoopScore(
            in LoopFeedbackContext ctx,
            in LoopScoringConfig config)
        {
            float score = 0f;

            // 1) Density: more active tracks = richer sound.
            score += ctx.ActiveTracks * config.densityBonusPerTrack;

            // 2) Role / participation budget — adaptive.
            score += ComputeRoleBudgetScore(ctx, config);

            // 3) Last loop of part bonus (climax).
            if (ctx.IsLastLoopOfPart)
                score += config.lastLoopBonus;

            // 4) Complexity.
            score += config.complexityMultiplier * ctx.TotalComplexity;

            return score;
        }

        /// <summary>
        /// Piecewise mapping from LoopScore to SongHype delta,
        /// using Inspector-tuneable thresholds.
        /// </summary>
        public static float ComputeHypeDelta(
            float loopScore,
            in HypeThresholds t)
        {
            if (loopScore >= t.amazing) return t.amazingDelta;
            if (loopScore >= t.veryGood) return t.veryGoodDelta;
            if (loopScore >= t.decent) return t.decentDelta;
            if (loopScore > t.neutralFloor) return t.neutralDelta;
            if (loopScore > t.mehFloor) return t.mehDelta;
            return t.badDelta;
        }

        // ── internals ──

        private static float ComputeRoleBudgetScore(
            in LoopFeedbackContext ctx,
            in LoopScoringConfig config)
        {
            if (config.roleBudget <= 0f)
                return 0f;

            switch (config.mode)
            {
                case LoopScoringMode.RoleNormalization:
                    return RoleNormScore(ctx, config);

                case LoopScoringMode.MusicianParticipation:
                    return MusicianPartScore(ctx, config);

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// roleBudget × (distinct roles filled / possible roles).
        /// </summary>
        private static float RoleNormScore(
            in LoopFeedbackContext ctx,
            in LoopScoringConfig config)
        {
            int possible = Mathf.Max(1, config.possibleRoleCount);

            if (ctx.Tracks == null || ctx.Tracks.Count == 0)
                return 0f;

            int filled = CountDistinctRoles(ctx.Tracks);
            float ratio = Mathf.Clamp01((float)filled / possible);
            return config.roleBudget * ratio;
        }

        /// <summary>
        /// roleBudget × (distinct musicians playing / total musicians).
        /// </summary>
        private static float MusicianPartScore(
            in LoopFeedbackContext ctx,
            in LoopScoringConfig config)
        {
            int total = Mathf.Max(1, config.totalMusicians);

            if (ctx.Tracks == null || ctx.Tracks.Count == 0)
                return 0f;

            int playing = CountDistinctMusicians(ctx.Tracks);
            float ratio = Mathf.Clamp01((float)playing / total);
            return config.roleBudget * ratio;
        }

        private static int CountDistinctRoles(IReadOnlyList<LoopTrackSnapshot> tracks)
        {
            // Avoid LINQ allocation in hot path.
            // TrackRole values are small ints — use a simple bitfield.
            int mask = 0;
            for (int i = 0; i < tracks.Count; i++)
                mask |= 1 << (int)tracks[i].Role;
            return BitCount(mask);
        }

        private static int CountDistinctMusicians(IReadOnlyList<LoopTrackSnapshot> tracks)
        {
            // Small band sizes (2–5) — HashSet overhead acceptable.
            var seen = new HashSet<string>(tracks.Count);
            for (int i = 0; i < tracks.Count; i++)
            {
                var id = tracks[i].MusicianId;
                if (!string.IsNullOrEmpty(id))
                    seen.Add(id);
            }
            return seen.Count;
        }

        private static int BitCount(int x)
        {
            // Kernighan's algorithm.
            int count = 0;
            while (x != 0)
            {
                x &= x - 1;
                count++;
            }
            return count;
        }
    }
}
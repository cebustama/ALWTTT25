using UnityEngine;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S2 D-S2-7=A] Single source of truth for the FT presentation of the
    /// two S1 audience-reaction surfaces. BOTH the GigManager direct calls
    /// (live through S2 per D-S2-3=A coexistence) and the bus-side
    /// <see cref="SensoryFxAdapter"/> derive text/color/drift from here, so
    /// the two emission paths cannot drift before S3 deletes the direct
    /// calls.
    ///
    /// RGB values are CODE TRUTH as shipped in S1 (GigManager
    /// ImpressionToExclamation / ImpressionToColor, moved here verbatim).
    /// Note: the S1 changelog entry mis-recorded neutral as (0.75,0.75,0.78);
    /// shipped value is (0.55,0.55,0.55), darker than MEH — see S2 changelog
    /// correction.
    /// </summary>
    public static class SensoryFtPresentation
    {
        /// <summary>Drift direction used by the loop-reaction FT spawn.</summary>
        public static readonly Vector2 ReactionDrift = new Vector2(0f, 1.0f);

        /// <summary>Drift direction used by the song-end vibe FT spawn.</summary>
        public static readonly Vector2 SongEndDrift = new Vector2(0f, 1.0f);

        // ----- Per-loop impression surface (S1 D-F-4=A) ------------------

        // [B2 / #3, S1 D-F-4=A] Map clamped loop impression [-2..2] to a short
        // crowd reaction. Neutral (0) gets a muted ellipsis to satisfy the
        // Sensory Contract (D2) — every player-visible state change emits at
        // least an FT.
        public static string ImpressionExclamation(int impression)
        {
            switch (impression)
            {
                case 2: return "WOW!";
                case 1: return "YEAH";
                case 0: return "…";              // was null — Sensory Contract gap closer
                case -1: return "MEH";
                case -2: return "BORING";
            }
            return "…";                          // defensive fallthrough — also neutral
        }

        // [B2 / #3, S1 D-F-4=A] Color sign-coded so impressions read at a
        // glance. Neutral is darker than MEH so the two read as distinct at a
        // quick scan.
        public static Color ImpressionColor(int impression)
        {
            if (impression >= 2) return new Color(1.0f, 0.85f, 0.20f); // gold
            if (impression == 1) return new Color(0.40f, 1.0f, 0.40f); // green
            if (impression == 0) return new Color(0.55f, 0.55f, 0.55f); // muted grey (neutral)
            if (impression == -1) return new Color(0.80f, 0.80f, 0.80f); // light grey (mild dislike)
            if (impression <= -2) return new Color(1.0f, 0.30f, 0.30f); // red
            return Color.white;
        }

        // ----- Song-end vibe surface (B3 / D-F-2=A) ----------------------

        /// <summary>
        /// Builds the song-end FT payload from a <see cref="SongEndVibeEvent"/>.
        /// Mirrors the S1 branch logic exactly:
        /// applied &gt; 0 → "+N Vibe" / "+N Vibe (Flow ×M)" cyan;
        /// else intended &gt; 0 → "INDIFFERENT" grey.
        /// Returns false when no FT is due (cannot happen for published
        /// events, kept defensive).
        /// </summary>
        public static bool TryBuildSongEndVibeFt(
            in SongEndVibeEvent e, out string text, out Color color)
        {
            if (e.AppliedDelta > 0)
            {
                text = e.FlowStacks > 0
                    ? $"+{e.AppliedDelta} Vibe (Flow ×{e.FlowMultiplier:F2})"
                    : $"+{e.AppliedDelta} Vibe";
                color = Color.cyan;
                return true;
            }

            if (e.IntendedDelta > 0)
            {
                // Blocked by Indifference.
                text = "INDIFFERENT";
                color = new Color(0.6f, 0.6f, 0.6f);
                return true;
            }

            text = null;
            color = default;
            return false;
        }
    }
}
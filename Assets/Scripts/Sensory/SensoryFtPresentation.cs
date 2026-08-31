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
                // [S5e] enemy loses persuasion HP -- damage-number convention
                text = e.FlowStacks > 0
                    ? $"-{e.AppliedDelta} Vibe (Flow ×{e.FlowMultiplier:F2})"
                    : $"-{e.AppliedDelta} Vibe";
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

        // ----- Card Vibe impact surface (JUICE-PW D1=A/D3=A) --------------

        /// <summary>Drift direction for card-impact FT. Straight up, same as
        /// the reaction surface; the SHORT "-N" form (vs song-end's
        /// "-N Vibe") is what visually separates the two waves (ST-PW-7).</summary>
        public static readonly Vector2 VibeImpactDrift = new Vector2(0f, 1.0f);

        /// <summary>Per-target FT stagger step (seconds) for the AoE fan-out,
        /// consumed by SensoryFxAdapter as FanoutIndex * step.</summary>
        public const float VibeImpactStaggerStep = 0.07f;

        /// <summary>
        /// Builds the card-impact FT payload from an
        /// <see cref="AudienceVibeImpactEvent"/>. S5e damage-number
        /// convention (positive effect DEPLETES the resistance pool):
        /// applied &gt; 0 → "-N" cyan (short form; song-end keeps "-N Vibe");
        /// blocked by Indifference → "INDIFFERENT" grey — deliberately the
        /// SAME word/colour as the song-end blocked surface (one concept,
        /// one presentation).
        /// Negative-delta cards (resistance restore) → "+N" red-ish, the
        /// anti-player direction. Returns false for a zero no-op.
        /// </summary>
        public static bool TryBuildVibeImpactFt(
            in AudienceVibeImpactEvent e, out string text, out Color color)
        {
            if (e.AppliedDelta > 0)
            {
                text = $"-{e.AppliedDelta}";
                color = Color.cyan;
                return true;
            }

            if (e.BlockedByIndifference)
            {
                text = "INDIFFERENT";
                color = new Color(0.6f, 0.6f, 0.6f);
                return true;
            }

            if (e.FinalDelta < 0)
            {
                // Anti-player: the member REGAINS resistance.
                text = $"+{-e.FinalDelta}";
                color = new Color(1.0f, 0.45f, 0.35f);
                return true;
            }

            text = null;
            color = default;
            return false;
        }

        // ----- Card performed / status applied surfaces [WINK-1] -----------

        /// <summary>Drift for the performer "shout" FT. Straight up, like the
        /// other single-beat surfaces.</summary>
        public static readonly Vector2 CardPerformedDrift = new Vector2(0f, 1.0f);

        /// <summary>Drift for the status-applied FT on the receiving character.</summary>
        public static readonly Vector2 StatusAppliedDrift = new Vector2(0f, 1.0f);

        // 4-way palette (proposal, adjustable): side x IsBuff. Musician side
        // reuses the impression green/red so "good/bad for the band" reads with
        // the vocabulary the player already learned. Audience side gets its own
        // family — pink for player-favorable charms (Captivated hearts), cold
        // blue-grey for the rest — so a status landing on the crowd never
        // masquerades as a band impression.
        private static readonly Color StatusMusicianBuff = new Color(0.40f, 1.0f, 0.40f);
        private static readonly Color StatusMusicianDebuff = new Color(1.0f, 0.30f, 0.30f);
        private static readonly Color StatusAudienceBuff = new Color(1.0f, 0.55f, 0.75f);
        private static readonly Color StatusAudienceDebuff = new Color(0.55f, 0.65f, 0.85f);

        /// <summary>
        /// [WINK-1 D-WINK-6=B] Performer beat FT. Text is DERIVED — the card's
        /// DisplayName uppercased plus "!" — deliberately NOT an authorable
        /// CardDefinition field: an authoring contract with no real case behind
        /// it is pure cost. Reverses the direction hinted at in the plan
        /// session (authorable field with fallback); recorded at batch close.
        /// </summary>
        public static bool TryBuildCardPerformedFt(
            in CardPerformedEvent e, out string text, out Color color)
        {
            color = Color.white;
            string name = e.Card != null ? e.Card.DisplayName : null;
            if (string.IsNullOrEmpty(name))
            {
                text = null;
                return false;
            }
            text = name.ToUpperInvariant() + "!";
            return true;
        }

        /// <summary>
        /// [WINK-1] Status-applied FT on the receiving character.
        /// GATE (ST-W7): DeltaStacks must be &gt; 0 — the container publishes
        /// StatusAppliedEvent for ANY apply that leaves stacks &gt; 0, including
        /// a negative delta that merely reduces them; drawing on those would
        /// announce a "gain" on a loss. Colour = owner side x IsBuff.
        /// </summary>
        public static bool TryBuildStatusAppliedFt(
            in StatusAppliedEvent e, bool ownerIsMusician,
            out string text, out Color color)
        {
            text = null;
            color = default;

            if (e.DeltaStacks <= 0) return false;                 // ST-W7 gate
            var so = e.Effect;
            if (so == null || string.IsNullOrEmpty(so.DisplayName)) return false;

            text = "+" + so.DisplayName.ToUpperInvariant();
            color = ownerIsMusician
                ? (so.IsBuff ? StatusMusicianBuff : StatusMusicianDebuff)
                : (so.IsBuff ? StatusAudienceBuff : StatusAudienceDebuff);
            return true;
        }

        // ----- Spotlight redirect surface [PRES-1 / D-PRES1-2=A] ----------

        /// <summary>Drift for the redirect floater. Straight up, matching the
        /// other single-beat surfaces.</summary>
        public static readonly Vector2 SpotlightRedirectDrift = new Vector2(0f, 1.0f);

        /// <summary>Spotlight gold — reads as stage light, and is distinct from
        /// every colour already in use (cyan impact, green/red impression,
        /// grey indifference).</summary>
        public static readonly Color SpotlightGold = new Color(1.0f, 0.84f, 0.30f);

        /// <summary>
        /// Builds the Spotlight redirect floater from a
        /// <see cref="SpotlightRedirectEvent"/>. Two shapes by necessity:
        ///
        ///   OriginalTarget known (Musician branch) → "-&gt; {protected}" spawned
        ///   ON THE ORIGINAL. Reads as "the hit aimed at you went that way",
        ///   which is the information the player is missing.
        ///
        ///   OriginalTarget null (RandomMusician branch) → "¡Foco!" spawned on
        ///   the PROTECTED musician. The would-be target is genuinely
        ///   indeterminate there: naming one would require rolling the RNG,
        ///   which a presentation path must never do (it would shift every
        ///   later roll in the gig).
        ///
        /// ASCII arrow on purpose: "→" is not guaranteed in the FT font atlas,
        /// and a tofu box in the middle of a combat beat is worse than "-&gt;".
        /// ESP copy hardcoded, matching the Blocked tooltip and telegraph labels
        /// (D-S5f-7=A / D-S5f-8=A); migrates with them in the S5f-ext pass.
        /// </summary>
        public static bool TryBuildSpotlightRedirectFt(
            in SpotlightRedirectEvent e, out string text, out Color color)
        {
            color = SpotlightGold;

            if (e.OriginalTarget != null && e.ProtectedTarget != null)
            {
                text = $"-> {e.ProtectedTarget.CharacterName}";
                return true;
            }

            if (e.ProtectedTarget != null)
            {
                text = "¡Foco!";
                return true;
            }

            text = null;
            return false;
        }
    }
}
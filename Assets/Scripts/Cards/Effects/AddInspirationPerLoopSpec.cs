using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Cards.Effects
{
    /// <summary>
    /// [DF-INSPLOOP] Recurring Inspiration granted at each loop boundary
    /// WHILE THE TRACK CREATED BY THE CARRYING CARD IS ACTIVE in the looping
    /// part (D-INSP-1=D). The bonus is DERIVED from
    /// TrackEntry.sourceCardDefinition at EvalPerLoopInsp time — never written
    /// into inspirationGenerated (asset or runtime), so S5e's content
    /// deprecation and LoopScore complexity inertness hold by construction
    /// (D-INSP-3=A / D-INSP-4). Additive across distinct tracks (D-INSP-2=A);
    /// same (musician, role) replacement swaps the source card and with it the
    /// bonus. Only meaningful on Composition Track cards; inert elsewhere.
    /// MVP: data-only spec. Consumed at track-binding time, NOT by
    /// CardBase.ExecuteEffects (defensive no-op branch there).
    /// </summary>
    [Serializable]
    public sealed class AddInspirationPerLoopSpec : CardEffectSpec
    {
        [Tooltip("Inspiration granted each loop while this card's track is active. Must be >= 1.")]
        public int amountPerLoop = 1;

        /// <summary>
        /// Single point of truth for the derived per-loop bonus a card carries.
        /// Sums every AddInspirationPerLoopSpec in the card's payload effects.
        /// Null-safe; returns 0 when the card, payload, or effects are absent.
        /// </summary>
        public static int SumFor(ALWTTT.Cards.CardDefinition def)
        {
            IReadOnlyList<CardEffectSpec> effects =
                def != null && def.Payload != null ? def.Payload.Effects : null;
            if (effects == null) return 0;

            int sum = 0;
            for (int i = 0; i < effects.Count; i++)
                if (effects[i] is AddInspirationPerLoopSpec s && s.amountPerLoop > 0)
                    sum += s.amountPerLoop;
            return sum;
        }
    }
}
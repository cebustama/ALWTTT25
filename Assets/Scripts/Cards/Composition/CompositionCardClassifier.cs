using System;

namespace ALWTTT.Cards
{
    /// <summary>
    /// Centralized rules for interpreting composition payloads as
    /// "tempo/time signature/instrument/tonality/etc." cards.
    ///
    /// IMPORTANT:
    /// - This is UI / session-classification logic, not data.
    /// - Prefer concrete effect subclass checks over string matching.
    /// </summary>
    public static class CompositionCardClassifier
    {
        // -------------------------
        // High level
        // -------------------------

        public static bool AffectsSound(CompositionCardPayload comp)
        {
            if (comp == null) return false;

            // Any musical parameter effect counts as affecting sound.
            if (HasAnyEffect(comp))
                return true;

            // Track primary generally affects sound (instrument/role/style bundle).
            if (comp.PrimaryKind == CardPrimaryKind.Track)
                return true;

            // Structural part markings may affect arrangement/playback.
            if (comp.PrimaryKind == CardPrimaryKind.Part && comp.PartAction != null)
            {
                switch (comp.PartAction.action)
                {
                    case PartActionKind.MarkIntro:
                    case PartActionKind.MarkBridge:
                    case PartActionKind.MarkSolo:
                    case PartActionKind.MarkOutro:
                    case PartActionKind.MarkFinal:
                        return true;
                }
            }

            return false;
        }

        // -------------------------
        // Specific categories
        // -------------------------

        public static bool IsTempoCard(CompositionCardPayload comp)
        {
            return HasEffect<TempoEffect>(comp);
        }

        public static bool IsTimeSignatureCard(CompositionCardPayload comp)
        {
            // You model meter/time signature as a MeterEffect.
            return HasEffect<MeterEffect>(comp);
        }

        public static bool IsInstrumentCard(CompositionCardPayload comp)
        {
            // Two different reasons can imply "instrument card":
            // 1) Explicit instrument effect
            // 2) Track-primary cards (often imply an instrument/role change in your design)
            if (comp == null) return false;

            if (comp.PrimaryKind == CardPrimaryKind.Track)
                return true;

            return HasEffect<InstrumentEffect>(comp);
        }

        public static bool IsTonalityCard(CompositionCardPayload comp)
        {
            // Mode/scale selection
            return HasEffect<TonalityEffect>(comp);
        }

        public static bool IsModulationCard(CompositionCardPayload comp)
        {
            // Key change / modulation mechanics
            return HasEffect<ModulationEffect>(comp);
        }

        // -------------------------
        // Helpers
        // -------------------------

        private static bool HasAnyEffect(CompositionCardPayload comp)
        {
            if (comp == null) return false;
            var fxList = comp.ModifierEffects;
            return fxList != null && fxList.Count > 0;
        }

        private static bool HasEffect<T>(CompositionCardPayload comp) where T : PartEffect
        {
            if (comp == null) return false;

            var effects = comp.ModifierEffects;
            if (effects == null || effects.Count == 0) return false;

            foreach (var fx in effects)
            {
                if (fx is T)
                    return true;
            }

            return false;
        }
    }
}

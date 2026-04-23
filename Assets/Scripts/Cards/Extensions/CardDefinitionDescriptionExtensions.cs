using System.Collections.Generic;
using System.Text;
using ALWTTT.Cards.Effects;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;

namespace ALWTTT.Cards
{
    public static class CardDefinitionDescriptionExtensions
    {
        public static string GetDescription(
            this CardDefinition card, BandCharacterStats stats = null)
        {
            if (card == null) return string.Empty;
            if (!card.HasPayload) return "Missing payload.";

            if (card.Payload is CompositionCardPayload comp)
                return BuildCompositionDescription(comp);

            if (card.Payload is ActionCardPayload action)
            {
                // Action cards: all effects, one per line, via central builder.
                return CardEffectDescriptionBuilder.BuildList(action.Effects, stats);
            }

            return $"Unsupported payload: {card.Payload.GetType().Name}";
        }

        /// <summary>
        /// Full detail description for the right-click card detail modal (M1.10).
        /// Action cards: same as card face (effects via builder).
        /// Composition cards: primary kind, style bundle, part info,
        /// full modifier list with GetLabel(), and CardPayload.Effects.
        /// </summary>
        public static string GetDetailDescription(
            this CardDefinition card, BandCharacterStats stats = null)
        {
            if (card == null) return string.Empty;
            if (!card.HasPayload) return "Missing payload.";

            if (card.Payload is CompositionCardPayload comp)
                return BuildCompositionDetailDescription(comp, stats);

            if (card.Payload is ActionCardPayload action)
                return CardEffectDescriptionBuilder.BuildList(action.Effects, stats);

            return $"Unsupported payload: {card.Payload.GetType().Name}";
        }

        #region Card-face description (existing)

        private static string BuildCompositionDescription(CompositionCardPayload p)
        {
            var sb = new StringBuilder();
            switch (p.PrimaryKind)
            {
                case CardPrimaryKind.Track:
                    sb.Append(p.TrackAction != null ? p.TrackAction.role.ToString() : "Track");
                    break;
                case CardPrimaryKind.Part:
                    sb.Append(p.PartAction != null ? FormatPartAction(p.PartAction) : "Part");
                    break;
                default:
                    sb.Append("Composition");
                    break;
            }

            int modifierCount = 0;
            if (p.ModifierEffects != null)
                foreach (var fx in p.ModifierEffects) if (fx != null) modifierCount++;

            if (modifierCount > 0)
                sb.Append($"\n<size=70%>{modifierCount} modifier{(modifierCount == 1 ? "" : "s")}</size>");

            return sb.ToString();
        }

        #endregion

        #region Detail-view description (M1.10)

        private static string BuildCompositionDetailDescription(
            CompositionCardPayload p, BandCharacterStats stats)
        {
            var sb = new StringBuilder();

            // --- Primary kind header ---
            switch (p.PrimaryKind)
            {
                case CardPrimaryKind.Track:
                    sb.Append("<b>Track</b>");
                    if (p.TrackAction != null)
                    {
                        sb.Append($" — {p.TrackAction.role}");

                        // Style bundle reference
                        string bundleName = p.TrackAction.styleBundle != null
                            ? p.TrackAction.styleBundle.name
                            : null;
                        if (!string.IsNullOrEmpty(bundleName))
                            sb.Append($"\nStyle: {bundleName}");
                    }
                    break;

                case CardPrimaryKind.Part:
                    sb.Append("<b>Part</b>");
                    if (p.PartAction != null)
                    {
                        sb.Append($" — {FormatPartAction(p.PartAction)}");

                        if (!string.IsNullOrEmpty(p.PartAction.musicianId))
                            sb.Append($"\nMusician: {p.PartAction.musicianId}");
                    }
                    break;

                default:
                    sb.Append("<b>Composition</b>");
                    break;
            }

            // --- Modifier list ---
            if (p.ModifierEffects != null)
            {
                int modCount = 0;
                foreach (var fx in p.ModifierEffects)
                    if (fx != null) modCount++;

                if (modCount > 0)
                {
                    sb.Append("\n\n<b>Modifiers</b>");
                    foreach (var fx in p.ModifierEffects)
                    {
                        if (fx == null) continue;
                        sb.Append($"\n  • {fx.GetLabel()}");
                        sb.Append($"  <size=80%>({fx.scope}, {fx.timing})</size>");
                    }
                }
            }

            // --- CardPayload.Effects (shared effect pipeline) ---
            var effects = p.Effects;
            if (effects != null && effects.Count > 0)
            {
                string effectsBlock = CardEffectDescriptionBuilder.BuildList(effects, stats);
                if (!string.IsNullOrEmpty(effectsBlock) && effectsBlock != "No effects.")
                {
                    sb.Append("\n\n<b>Effects</b>\n");
                    sb.Append(effectsBlock);
                }
            }

            return sb.ToString();
        }

        #endregion

        private static string FormatPartAction(PartActionDescriptor pa) =>
            string.IsNullOrWhiteSpace(pa.customLabel)
                ? pa.action.ToString()
                : pa.customLabel;
    }
}
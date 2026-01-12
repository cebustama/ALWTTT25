using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ALWTTT.Cards
{
    /// <summary>
    /// UI-facing text helpers for CardDefinition.
    /// Keeps CardDefinition SRP-clean ("what a card is"),
    /// while still allowing CardDefinition.GetDescription(...) in UI code.
    /// </summary>
    public static class CardDefinitionDescriptionExtensions
    {
        public static string GetDescription(
            this CardDefinition card, BandCharacterStats stats = null)
        {
            if (card == null) return string.Empty;
            if (!card.HasPayload) return "Missing payload.";

            // Composition cards use composition description
            if (card.Payload is CompositionCardPayload comp)
                return GetCompositionDescription(comp);

            // Action cards (or anything else with actions) use action description
            if (card.Payload is ActionCardPayload action)
                return GetActionDescription(card, action, stats);

            return $"Unsupported payload: {card.Payload.GetType().Name}";
        }

        private static string GetActionDescription(
            CardDefinition card,
            ActionCardPayload payload,
            BandCharacterStats stats)
        {
            var statusActions = payload != null ? payload.StatusActions : null;
            if (statusActions == null || statusActions.Count == 0)
                return "No effects.";

            // Parity with previous behavior: describe the first effect only (for now).
            // TODO: Describe all status actions (and consider line breaks).
            var sa = statusActions[0];

            // “SynergyText” concept no longer applies unless you want to scale stacks by stats.
            // For now: show stacks delta as the primary magnitude.
            var magnitudeText = sa.StacksDelta.ToString();

            // EffectId is generic (ontology). DisplayName is thematic but lives on StatusEffectSO.
            // Here we only have ids, so show EffectId.
            // TODO: Access DisplayName
            var effectText = sa.EffectId.ToString();
            var targetText = sa.TargetType.ToString();

            return $"Apply {magnitudeText} {effectText} to {targetText}";
        }


        private static string GetCompositionDescription(CompositionCardPayload payload)
        {
            var sb = new StringBuilder("Composition: ");

            if (payload.PrimaryKind == CardPrimaryKind.Track)
            {
                string role = payload.TrackAction != null
                    ? payload.TrackAction.role.ToString()
                    : "Track";

                sb.Append($"Track [{role}]");
            }
            else if (payload.PrimaryKind == CardPrimaryKind.Part)
            {
                string action = payload.PartAction != null
                    ? payload.PartAction.action.ToString()
                    : "Part";

                sb.Append($"Part [{action}]");

                if (payload.PartAction != null &&
                    !string.IsNullOrWhiteSpace(payload.PartAction.customLabel))
                {
                    sb.Append($" (“{payload.PartAction.customLabel}”)");
                }
            }
            else
            {
                sb.Append("No primary action");
            }

            // Effects
            if (payload.ModifierEffects != null && payload.ModifierEffects.Count > 0)
            {
                var effects = new List<string>();
                foreach (var fx in payload.ModifierEffects)
                {
                    if (fx == null) continue;
                    effects.Add(fx.GetLabel());
                }

                if (effects.Count > 0)
                    sb.Append(" | Effects: " + string.Join(", ", effects));
            }

            return sb.ToString();
        }
    }
}


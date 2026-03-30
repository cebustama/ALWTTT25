#if UNITY_EDITOR
using System.Collections.Generic;

namespace ALWTTT.Cards.Editor
{
    internal static class DeckValidationService
    {
        public static ValidationResult Validate(StagedDeck staged)
        {
            var result = new ValidationResult();

            if (staged == null)
            {
                result.Status = ValidationResultStatus.Invalid;
                result.Errors.Add("No deck is currently staged.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(staged.deckId))
                result.Errors.Add("deckId must not be empty. Fill in the Deck Id field before saving.");

            if (staged.cards == null || staged.cards.Count == 0)
                result.Errors.Add("The deck has no cards. Add at least one card before saving.");

            if (staged.cards != null)
            {
                var seenCards = new HashSet<CardDefinition>();

                for (int i = 0; i < staged.cards.Count; i++)
                {
                    var entry = staged.cards[i];

                    if (entry == null || !entry.IsValid)
                    {
                        result.Errors.Add($"Card slot {i} is invalid or null. Remove it before saving.");
                        continue;
                    }

                    var card = entry.ResolvedCard;
                    if (card == null)
                    {
                        result.Errors.Add($"Card slot {i} resolved to null. Remove it before saving.");
                        continue;
                    }

                    if (!seenCards.Add(card))
                        result.Warnings.Add($"Card '{card.Id}' appears more than once. The runtime will deduplicate it.");

                    if (!card.IsAction && !card.IsComposition)
                        result.Warnings.Add($"Card '{card.Id}' ({card.DisplayName}) is neither Action nor Composition. SetBandDeck will drop it at runtime.");
                }
            }

            if (string.IsNullOrWhiteSpace(staged.displayName))
                result.Warnings.Add("displayName is empty. The deck will appear unnamed in Gig Setup.");

            result.Status = result.HasErrors ? ValidationResultStatus.Invalid : ValidationResultStatus.Valid;
            return result;
        }
    }
}
#endif

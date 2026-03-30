#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ALWTTT.Status;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// Parses a deck JSON string into a StagedDeck.
    ///
    /// Each card entry is handled in one of two modes:
    ///   - Reference existing card (kind absent): resolved by cardId / assetPath.
    ///   - Create new card (kind present): staged in memory via DeckCardCreationService.
    ///
    /// No disk writes. Returns errors and warnings for the caller to display.
    /// </summary>
    internal static class DeckJsonImportService
    {
        // ------------------------------------------------------------------
        // Import
        // ------------------------------------------------------------------

        public static ImportResult Import(string json, StatusEffectCatalogueSO catalogue)
        {
            var result = new ImportResult();

            if (string.IsNullOrWhiteSpace(json))
            {
                result.Status = ImportResultStatus.Failed;
                result.Errors.Add("JSON input is empty.");
                return result;
            }

            string trimmed = json.Trim();
            if (trimmed.StartsWith("["))
            {
                result.Status = ImportResultStatus.Failed;
                result.Errors.Add("Root JSON arrays are not supported. Wrap in an object: { \"deckId\": \"...\", \"cards\": [...] }");
                return result;
            }

            DeckJsonDto dto;
            try { dto = JsonUtility.FromJson<DeckJsonDto>(trimmed); }
            catch (Exception ex)
            {
                result.Status = ImportResultStatus.Failed;
                result.Errors.Add($"JSON parse error: {ex.Message}");
                return result;
            }

            if (dto == null) { result.Status = ImportResultStatus.Failed; result.Errors.Add("JSON parsed to null."); return result; }
            if (string.IsNullOrWhiteSpace(dto.deckId)) result.Errors.Add("'deckId' is required and must not be empty.");
            if (dto.cards == null || dto.cards.Length == 0) result.Errors.Add("'cards' array is required and must not be empty.");

            if (result.Errors.Count > 0) { result.Status = ImportResultStatus.Failed; return result; }

            var catalogue_ = catalogue; // capture
            var cardIndex  = BuildCardCatalogue();
            var entries    = new List<StagedCardEntry>();
            var seenCards  = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // by id, dedup check
            bool anyFailed = false;

            foreach (var entry in dto.cards)
            {
                if (entry == null) continue;

                bool isCreateMode = !string.IsNullOrWhiteSpace(entry.kind);

                if (isCreateMode)
                {
                    // --- Create new card ---
                    if (!DeckCardCreationService.TryStageNewCard(entry, catalogue_, out var staged, out var err))
                    {
                        result.Errors.Add($"New card '{entry.id ?? "<no id>"}': {err}");
                        anyFailed = true;
                        continue;
                    }

                    string newId = staged.ResolvedCard?.Id ?? "";
                    if (!string.IsNullOrEmpty(newId) && !seenCards.Add(newId))
                    {
                        result.Warnings.Add($"Duplicate card id '{newId}' in JSON was skipped.");
                        // Destroy the staged objects to avoid leaks
                        if (staged.pendingCard    != null) UnityEngine.Object.DestroyImmediate(staged.pendingCard);
                        if (staged.pendingPayload != null) UnityEngine.Object.DestroyImmediate(staged.pendingPayload);
                        continue;
                    }

                    entries.Add(staged);
                }
                else
                {
                    // --- Reference existing card ---
                    string cardId    = entry.cardId?.Trim()    ?? "";
                    string assetPath = entry.assetPath?.Trim() ?? "";

                    if (string.IsNullOrEmpty(cardId) && string.IsNullOrEmpty(assetPath))
                    {
                        result.Warnings.Add("Skipped a card entry with no cardId, no assetPath, and no kind.");
                        continue;
                    }

                    if (!ResolveExistingCard(cardId, assetPath, cardIndex, out var card, out var resolveErr))
                    {
                        result.Errors.Add(resolveErr);
                        anyFailed = true;
                        continue;
                    }

                    string resolvedId = card.Id ?? "";
                    if (!string.IsNullOrEmpty(resolvedId) && !seenCards.Add(resolvedId))
                    {
                        result.Warnings.Add($"Duplicate card reference '{resolvedId}' was normalized to one entry (runtime would deduplicate anyway).");
                        continue;
                    }

                    if (!card.IsAction && !card.IsComposition)
                        result.Warnings.Add($"Card '{card.Id}' is neither Action nor Composition. SetBandDeck will silently drop it at runtime.");

                    entries.Add(StagedCardEntry.FromExisting(card));
                }
            }

            if (anyFailed) { result.Status = ImportResultStatus.Failed; return result; }

            var staged_ = new StagedDeck
            {
                deckId      = dto.deckId?.Trim()      ?? "",
                displayName = dto.displayName?.Trim() ?? "",
                description = dto.description?.Trim() ?? "",
                cards       = entries,
                isDirty     = true
            };

            result.StagedDeck = staged_;
            result.Status = result.HasWarnings ? ImportResultStatus.OkWithWarnings : ImportResultStatus.Ok;
            return result;
        }

        // ------------------------------------------------------------------
        // Export
        // ------------------------------------------------------------------

        public static string Export(StagedDeck staged)
        {
            if (staged == null) return "{}";

            var entries = new DeckCardEntryJson[staged.cards?.Count ?? 0];
            for (int i = 0; i < entries.Length; i++)
            {
                var e = staged.cards[i];
                if (e == null) { entries[i] = new DeckCardEntryJson(); continue; }

                if (e.IsNew && e.pendingDto != null)
                {
                    // Round-trip the original creation DTO
                    entries[i] = e.pendingDto;
                }
                else
                {
                    var card = e.ResolvedCard;
                    entries[i] = new DeckCardEntryJson
                    {
                        cardId    = card?.Id ?? "",
                        assetPath = card != null ? AssetDatabase.GetAssetPath(card) : ""
                    };
                }
            }

            var dto = new DeckJsonDto
            {
                deckId      = staged.deckId,
                displayName = staged.displayName,
                description = staged.description,
                cards       = entries
            };

            return JsonUtility.ToJson(dto, prettyPrint: true);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static bool ResolveExistingCard(
            string cardId, string assetPath,
            Dictionary<string, CardDefinition> index,
            out CardDefinition card, out string error)
        {
            error = null;
            card  = null;

            if (!string.IsNullOrEmpty(cardId))
            {
                if (index.TryGetValue(cardId, out card)) return true;

                if (!string.IsNullOrEmpty(assetPath))
                {
                    var byPath = AssetDatabase.LoadAssetAtPath<CardDefinition>(assetPath);
                    if (byPath != null)
                    {
                        if (!string.Equals(byPath.Id, cardId, StringComparison.OrdinalIgnoreCase))
                        {
                            error = $"Card at '{assetPath}' has Id '{byPath.Id}', not '{cardId}'. Fix the JSON or the card asset.";
                            return false;
                        }
                        card = byPath;
                        return true;
                    }
                }

                error = $"Could not resolve cardId '{cardId}'. No CardDefinition with that Id was found in the project.";
                return false;
            }

            if (!string.IsNullOrEmpty(assetPath))
            {
                card = AssetDatabase.LoadAssetAtPath<CardDefinition>(assetPath);
                if (card != null) return true;
                error = $"Could not load CardDefinition from assetPath '{assetPath}'.";
                return false;
            }

            error = "Card entry has no cardId and no assetPath.";
            return false;
        }

        private static Dictionary<string, CardDefinition> BuildCardCatalogue()
        {
            var index = new Dictionary<string, CardDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach (string guid in AssetDatabase.FindAssets("t:CardDefinition"))
            {
                var c = AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (c == null || string.IsNullOrWhiteSpace(c.Id)) continue;
                if (index.ContainsKey(c.Id))
                    Debug.LogWarning($"[DeckJsonImportService] Duplicate CardDefinition Id '{c.Id}'. Last one wins.");
                index[c.Id] = c;
            }
            return index;
        }
    }
}
#endif

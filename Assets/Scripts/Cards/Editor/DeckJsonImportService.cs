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

        public static ImportResult Import(string json, ALWTTTProjectRegistriesSO registries)
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

            var registries_ = registries; // capture
            var cardIndex = BuildCardCatalogue();
            var entries = new List<StagedCardEntry>();
            // M4.4: tracks staged entries by id so we can combine duplicate
            // "reference existing" entries into a single entry with summed
            // count. Duplicate "create new" entries (kind != null) remain a
            // hard error � those are conflicting definitions, not copies.
            var byId = new Dictionary<string, StagedCardEntry>(StringComparer.OrdinalIgnoreCase);
            bool anyFailed = false;

            foreach (var entry in dto.cards)
            {
                if (entry == null) continue;

                bool isCreateMode = !string.IsNullOrWhiteSpace(entry.kind);
                int incomingCount = Math.Max(1, entry.count);

                if (isCreateMode)
                {
                    // --- Create new card ---
                    if (!DeckCardCreationService.TryStageNewCard(entry, registries_, out var staged, out var err))
                    {
                        result.Errors.Add($"New card '{entry.id ?? "<no id>"}': {err}");
                        anyFailed = true;
                        continue;
                    }

                    string newId = staged.ResolvedCard?.Id ?? "";
                    if (!string.IsNullOrEmpty(newId) && byId.ContainsKey(newId))
                    {
                        // Hard error: same id appearing as a fresh definition twice.
                        result.Errors.Add(
                            $"Duplicate card id '{newId}' as a new card definition. " +
                            $"To request multiple copies, use 'count' on a single entry.");
                        anyFailed = true;
                        if (staged.pendingCard != null) UnityEngine.Object.DestroyImmediate(staged.pendingCard);
                        if (staged.pendingPayload != null) UnityEngine.Object.DestroyImmediate(staged.pendingPayload);
                        continue;
                    }

                    staged.count = incomingCount;
                    entries.Add(staged);
                    if (!string.IsNullOrEmpty(newId)) byId[newId] = staged;
                }
                else
                {
                    // --- Reference existing card ---
                    string cardId = entry.cardId?.Trim() ?? "";
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
                    if (!string.IsNullOrEmpty(resolvedId) && byId.TryGetValue(resolvedId, out var existing))
                    {
                        // M4.4: combine duplicate cardId entries additively.
                        existing.count += incomingCount;
                        result.Warnings.Add(
                            $"Duplicate cardId '{resolvedId}' was combined into a single entry " +
                            $"(now �{existing.count}). Consider authoring 'count' explicitly.");
                        continue;
                    }

                    if (!card.IsAction && !card.IsComposition)
                        result.Warnings.Add($"Card '{card.Id}' is neither Action nor Composition. SetBandDeck will silently drop it at runtime.");

                    var newEntry = StagedCardEntry.FromExisting(card);
                    newEntry.count = incomingCount;
                    entries.Add(newEntry);
                    if (!string.IsNullOrEmpty(resolvedId)) byId[resolvedId] = newEntry;
                }
            }

            if (anyFailed) { result.Status = ImportResultStatus.Failed; return result; }

            var staged_ = new StagedDeck
            {
                deckId = dto.deckId?.Trim() ?? "",
                displayName = dto.displayName?.Trim() ?? "",
                description = dto.description?.Trim() ?? "",
                cards = entries,
                isDirty = true
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

                int copyCount = Math.Max(1, e.count);

                if (e.IsNew && e.pendingDto != null)
                {
                    // Round-trip the original creation DTO, with count synced to staged.
                    var dto = e.pendingDto;
                    dto.count = copyCount;
                    entries[i] = dto;
                }
                else
                {
                    var card = e.ResolvedCard;
                    entries[i] = new DeckCardEntryJson
                    {
                        cardId = card?.Id ?? "",
                        assetPath = card != null ? AssetDatabase.GetAssetPath(card) : "",
                        count = copyCount,
                    };
                }
            }

            var dto_ = new DeckJsonDto
            {
                deckId = staged.deckId,
                displayName = staged.displayName,
                description = staged.description,
                cards = entries
            };

            return JsonUtility.ToJson(dto_, prettyPrint: true);
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
            card = null;

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
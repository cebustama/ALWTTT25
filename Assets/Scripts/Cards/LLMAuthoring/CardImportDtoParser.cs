using UnityEngine;

namespace ALWTTT.Cards.LLMAuthoring
{
    /// <summary>
    /// Parses a JSON payload (single card object or batch wrapper) into
    /// <see cref="CardJsonImport"/> DTOs. Moved verbatim from
    /// <c>CardEditorWindow.TryParseJsonToCardDtos</c> (CE-L1 B1) so the LLM
    /// generation path and the legacy "Create from JSON" box share exactly one
    /// parse surface — the stage-4 importer of the LLM-authoring pattern.
    ///
    /// Pure: no Unity asset access, no editor-window state; JsonUtility only.
    /// Failures come back as a false return + a human-readable error, never an
    /// exception.
    /// </summary>
    public static class CardImportDtoParser
    {
        public static bool TryParse(string json, out CardJsonImport[] dtos, out string error)
        {
            dtos = null;
            error = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Invalid JSON: input is empty.";
                return false;
            }

            var trimmed = json.TrimStart();
            if (trimmed.StartsWith("["))
            {
                error =
                    "This importer does not accept a raw JSON array at the root.\n\n" +
                    "Use either:\n" +
                    "1) Single card object: { \"kind\": \"Action|Composition\", \"id\": \"...\", ... }\n" +
                    "2) Batch wrapper: { \"cards\": [ { ... }, { ... } ] }";
                return false;
            }

            // Try batch wrapper first: { "cards": [ ... ] }
            try
            {
                var batch = JsonUtility.FromJson<CardBatchJsonImport>(json);
                if (batch != null && batch.cards != null && batch.cards.Length > 0)
                {
                    // Merge batch-level defaultEntry into any card without its own entry.
                    if (batch.defaultEntry != null)
                    {
                        foreach (var card in batch.cards)
                        {
                            if (card != null &&
                                (card.entry == null ||
                                 string.IsNullOrWhiteSpace(card.entry.flags)))
                            {
                                card.entry = batch.defaultEntry;
                            }
                        }
                    }

                    dtos = batch.cards;
                    return true;
                }
            }
            catch
            {
                // ignored; will try single-card parse next
            }

            // Fallback: single card object
            try
            {
                var one = JsonUtility.FromJson<CardJsonImport>(json);
                if (one == null)
                {
                    error = "Invalid JSON: could not parse payload.";
                    return false;
                }

                dtos = new[] { one };
                return true;
            }
            catch (System.Exception ex)
            {
                error = "Invalid JSON: " + ex.Message;
                return false;
            }
        }
    }
}

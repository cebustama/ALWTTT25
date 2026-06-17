namespace ALWTTT.Cards.LLMAuthoring
{
    /// <summary>
    /// Pure translation of a <see cref="CardLLMResponseHandler.Outcome"/> into
    /// what the Card Editor window should do, plus the status line (CE-L1 B2;
    /// card sibling of <c>ChordLLMFieldPlan</c>, D-L4.7=A).
    ///
    /// Deliberately THINNER than the chord plan: the chord window owns its field
    /// writes, so its plan enumerates fields; the card window already has one
    /// staging path (<c>TryStageCardFromDto</c>) that maps a DTO onto the staged
    /// card, and duplicating that mapping here would create a second apply
    /// surface. This plan therefore decides WHETHER to stage, carries the
    /// validated DTO and the resolved palette to assign at Save, and formats the
    /// status — the window stays a thin applier with no decision logic.
    /// </summary>
    public readonly struct CardLLMFieldPlan
    {
        /// <summary>True when the window should stage <see cref="Dto"/> via its existing staging path.</summary>
        public readonly bool StageCard;

        /// <summary>The validated DTO to stage (null when !StageCard).</summary>
        public readonly CardJsonImport Dto;

        /// <summary>True when a resolved palette must be assigned to the role bundle at Save.</summary>
        public readonly bool AssignPalette;

        /// <summary>Descriptor id (asset path) of the palette to assign.</summary>
        public readonly string PaletteId;

        /// <summary>Display name of the palette (preview text).</summary>
        public readonly string PaletteDisplayName;

        /// <summary>Status line to display.</summary>
        public readonly string StatusMessage;

        /// <summary>True when the status should render as an error.</summary>
        public readonly bool StatusIsError;

        private CardLLMFieldPlan(
            bool stageCard, CardJsonImport dto,
            bool assignPalette, string paletteId, string paletteDisplayName,
            string statusMessage, bool statusIsError)
        {
            StageCard = stageCard;
            Dto = dto;
            AssignPalette = assignPalette;
            PaletteId = paletteId;
            PaletteDisplayName = paletteDisplayName;
            StatusMessage = statusMessage ?? string.Empty;
            StatusIsError = statusIsError;
        }

        /// <summary>Decide the plan for an outcome. Pure; no side effects.</summary>
        public static CardLLMFieldPlan From(CardLLMResponseHandler.Outcome outcome)
        {
            if (outcome.Kind != CardLLMResponseHandler.OutcomeKind.Staged || outcome.Dto == null)
            {
                return new CardLLMFieldPlan(
                    stageCard: false, dto: null,
                    assignPalette: false, paletteId: null, paletteDisplayName: null,
                    statusMessage: "Generation/import failed; see warnings. Nothing staged.",
                    statusIsError: true);
            }

            string id = string.IsNullOrWhiteSpace(outcome.Dto.id) ? "(card)" : outcome.Dto.id;
            string status = outcome.PaletteResolved
                ? $"Card '{id}' staged for review — Save (Create Assets) writes the card and " +
                  $"assigns palette '{outcome.ResolvedPaletteDisplayName}' to its bundle."
                : $"Card '{id}' staged for review — press Save (Create Assets) to write.";

            return new CardLLMFieldPlan(
                stageCard: true, dto: outcome.Dto,
                assignPalette: outcome.PaletteResolved,
                paletteId: outcome.ResolvedPaletteId,
                paletteDisplayName: outcome.ResolvedPaletteDisplayName,
                statusMessage: status,
                statusIsError: false);
        }
    }
}

using System.Collections.Generic;
using System.Text;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Tooltips;
using static ALWTTT.Tooltips.GameTextTemplateIds;

namespace ALWTTT.Cards.Effects
{
    /// <summary>
    /// Central formatter for CardEffectSpec → player-facing text.
    /// M1.3: replaces per-spec formatting that previously leaked
    /// CharacterStatusId enum names into card descriptions.
    ///
    /// Keeps CardEffectSpec strictly data-only (per SSoT_Card_System §6.1)
    /// by holding cross-cutting formatting (zero-delta hiding, target
    /// naming, rich-text coloring) in one place.
    ///
    /// [TXT-4 / D-TXT4-3=b′] The WORDS are no longer here. Every line is a
    /// template in the active ConceptGlossarySO (GameTextTemplateIds.fx_* /
    /// tgt_*), composed through GameText.Format with the numbers and status
    /// names already coloured. What stays in code is grammar-independent:
    /// which template a spec maps to, the zero-delta hiding, the sign, the
    /// colours. Status names come from the glossary by StatusKey
    /// (GameText.StatusName) — this builder used to read
    /// StatusEffectSO.DisplayName, a developer label since TXT-3 (F-TXT4-1).
    /// Rendered English is byte-identical to the pre-TXT-4 strings.
    /// </summary>
    public static class CardEffectDescriptionBuilder
    {
        // TMP rich-text colors. Tune if they clash with card art.
        private const string BuffColor = "<color=#8FD694>"; // soft green
        private const string DebuffColor = "<color=#D6858F>"; // soft red
        private const string NumberColor = "<color=#FFD084>"; // amber
        private const string ColorEnd = "</color>";

        /// <summary>Format every non-empty effect line in order, joined by '\n'.</summary>
        public static string BuildList(
            IReadOnlyList<CardEffectSpec> effects,
            BandCharacterStats stats = null)
        {
            if (effects == null || effects.Count == 0) return GameText.Format(FxNoEffects);

            var sb = new StringBuilder();
            for (int i = 0; i < effects.Count; i++)
            {
                var line = Build(effects[i], stats);
                if (string.IsNullOrEmpty(line)) continue;
                if (sb.Length > 0) sb.Append('\n');
                sb.Append(line);
            }
            return sb.Length == 0 ? GameText.Format(FxNoEffects) : sb.ToString();
        }

        /// <summary>[TXT-4] True when BuildList would produce at least one line (i.e. not the
        /// "no effects" text). Callers used to compare against the English literal.</summary>
        public static bool HasAnyLine(
            IReadOnlyList<CardEffectSpec> effects,
            BandCharacterStats stats = null)
        {
            if (effects == null || effects.Count == 0) return false;
            for (int i = 0; i < effects.Count; i++)
                if (!string.IsNullOrEmpty(Build(effects[i], stats))) return true;
            return false;
        }

        public static string Build(CardEffectSpec spec, BandCharacterStats stats = null)
        {
            if (spec is ApplyStatusEffectSpec ase) return BuildApplyStatus(ase);
            if (spec is ModifyVibeSpec vibe) return BuildModifyVibe(vibe);
            if (spec is ModifyStressSpec stress) return BuildModifyStress(stress);
            if (spec is DrawCardsSpec draw) return BuildDrawCards(draw);
            if (spec is AddInspirationPerLoopSpec inspLoop) return BuildAddInspirationPerLoop(inspLoop);
            if (spec is RevealPreferencesSpec reveal) return BuildRevealPreferences(reveal);
            if (spec is GrantBonusLoopSpec bonusLoop) return BuildGrantBonusLoop(bonusLoop);
            return spec.GetType().Name;   // developer diagnostic: an unmapped spec shows its type name (unchanged)
        }

        private static string BuildApplyStatus(ApplyStatusEffectSpec ase)
        {
            if (ase.status == null) return "Apply <missing status>.";   // authoring-error diagnostic, not player text
            if (ase.stacksDelta == 0) return string.Empty;

            var name = GameText.StatusName(ase.status);

            var colorTag = ase.status.IsBuff ? BuffColor : DebuffColor;
            var signed = ase.stacksDelta > 0
                ? $"{NumberColor}+{ase.stacksDelta}{ColorEnd}"
                : $"{NumberColor}{ase.stacksDelta}{ColorEnd}";
            var coloredName = $"{colorTag}{name}{ColorEnd}";

            if (ase.targetType == ActionTargetType.Self)
                return GameText.Format(FxGainStatus, signed, coloredName);

            return GameText.Format(FxApplyStatus, signed, coloredName, Target(ase.targetType));
        }

        private static string BuildModifyVibe(ModifyVibeSpec vibe)
        {
            if (vibe.amount == 0) return string.Empty;
            // [S5e] damage-verb convention, parallel to BuildModifyStress
            if (vibe.amount > 0)
                return GameText.Format(FxDealVibe, Number(vibe.amount), Target(vibe.targetType));
            return GameText.Format(FxRestoreVibe, Number(-vibe.amount), Target(vibe.targetType));
        }

        private static string BuildModifyStress(ModifyStressSpec stress)
        {
            if (stress.amount == 0) return string.Empty;
            if (stress.amount > 0)
                return GameText.Format(FxDealStress, Number(stress.amount), Target(stress.targetType));
            return GameText.Format(FxHealStress, Number(-stress.amount), Target(stress.targetType));
        }

        private static string BuildDrawCards(DrawCardsSpec draw)
        {
            if (draw.count <= 0) return string.Empty;
            return draw.count == 1
                ? GameText.Format(FxDrawOne, Number(1))
                : GameText.Format(FxDrawMany, Number(draw.count));
        }

        // [DF-INSPLOOP] Track-scoped recurring Inspiration. Surfaced on hover
        // tooltip / detail modal; composition card faces omit effects by design
        // (SSoT_Card_System §10.1).
        private static string BuildAddInspirationPerLoop(AddInspirationPerLoopSpec insp)
        {
            if (insp.amountPerLoop <= 0) return string.Empty;
            return GameText.Format(FxInspirationPerLoop, $"{NumberColor}+{insp.amountPerLoop}{ColorEnd}");
        }

        // [R4 / D-R0-1=A] Info-only effect: no number, no status name, so the line
        // is intentionally plain prose rather than the coloured number grammar the
        // meter-touching specs use.
        private static string BuildRevealPreferences(RevealPreferencesSpec reveal)
        {
            return GameText.Format(FxRevealTastes, Target(reveal.targetType));
        }

        // [R5-d / D-TXT4-8=A] Antes de TXT-4 esta carta caía al diagnóstico final de Build() y
        // pintaba "GrantBonusLoopSpec" en la cara (visible en golden A y B). Dos plantillas
        // completas en vez de una con fragmento opcional: la variante con solo cambia la frase
        // entera y su puntuación, que es gramática del idioma, no del builder.
        // El coste en Voltage NO se menciona aquí: vive en CardDefinition (D-R5-26=A) y lo
        // presenta la economía de la carta, no la línea de efecto.
        private static string BuildGrantBonusLoop(GrantBonusLoopSpec bonusLoop)
        {
            return GameText.Format(bonusLoop.soloOverBonusLoop
                ? FxGrantBonusLoopSolo
                : FxGrantBonusLoop);
        }

        private static string Number(int n) => $"{NumberColor}{n}{ColorEnd}";

        /// <summary>Target phrase from the glossary (tgt_*). An ActionTargetType with no
        /// template id keeps the pre-TXT-4 behaviour: its enum name.</summary>
        private static string Target(ActionTargetType t)
        {
            string id = TargetTemplateId(t);
            return id == null ? t.ToString() : GameText.Format(id);
        }

        private static string TargetTemplateId(ActionTargetType t)
        {
            switch (t)
            {
                case ActionTargetType.Self: return TgtSelf;
                case ActionTargetType.Musician: return TgtMusician;
                case ActionTargetType.AllMusicians: return TgtAllMusicians;
                case ActionTargetType.RandomMusician: return TgtRandomMusician;
                case ActionTargetType.AudienceCharacter: return TgtAudience;
                case ActionTargetType.AllAudienceCharacters: return TgtAllAudience;
                case ActionTargetType.RandomAudienceCharacter: return TgtRandomAudience;
                default: return null;
            }
        }
    }
}
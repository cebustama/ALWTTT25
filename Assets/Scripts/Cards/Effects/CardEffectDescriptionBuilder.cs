using System.Collections.Generic;
using System.Text;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;

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
            if (effects == null || effects.Count == 0) return "No effects.";

            var sb = new StringBuilder();
            for (int i = 0; i < effects.Count; i++)
            {
                var line = Build(effects[i], stats);
                if (string.IsNullOrEmpty(line)) continue;
                if (sb.Length > 0) sb.Append('\n');
                sb.Append(line);
            }
            return sb.Length == 0 ? "No effects." : sb.ToString();
        }

        public static string Build(CardEffectSpec spec, BandCharacterStats stats = null)
        {
            if (spec == null) return string.Empty;
            if (spec is ApplyStatusEffectSpec ase) return BuildApplyStatus(ase);
            if (spec is ModifyVibeSpec vibe) return BuildModifyVibe(vibe);
            if (spec is ModifyStressSpec stress) return BuildModifyStress(stress);
            if (spec is DrawCardsSpec draw) return BuildDrawCards(draw);
            return spec.GetType().Name;
        }

        private static string BuildApplyStatus(ApplyStatusEffectSpec ase)
        {
            if (ase.status == null) return "Apply <missing status>.";
            if (ase.stacksDelta == 0) return string.Empty;

            var name = string.IsNullOrWhiteSpace(ase.status.DisplayName)
                ? ase.status.name
                : ase.status.DisplayName;

            var colorTag = ase.status.IsBuff ? BuffColor : DebuffColor;
            var signed = ase.stacksDelta > 0
                ? $"{NumberColor}+{ase.stacksDelta}{ColorEnd}"
                : $"{NumberColor}{ase.stacksDelta}{ColorEnd}";

            if (ase.targetType == ActionTargetType.Self)
                return $"Gain {signed} {colorTag}{name}{ColorEnd}";

            return $"Apply {signed} {colorTag}{name}{ColorEnd} to {DescribeTarget(ase.targetType)}";
        }

        private static string BuildModifyVibe(ModifyVibeSpec vibe)
        {
            if (vibe.amount == 0) return string.Empty;
            var signed = vibe.amount > 0
                ? $"{NumberColor}+{vibe.amount}{ColorEnd}"
                : $"{NumberColor}{vibe.amount}{ColorEnd}";
            return $"{signed} Vibe on {DescribeTarget(vibe.targetType)}";
        }

        private static string BuildModifyStress(ModifyStressSpec stress)
        {
            if (stress.amount == 0) return string.Empty;
            if (stress.amount > 0)
                return $"Deal {NumberColor}{stress.amount}{ColorEnd} Stress to {DescribeTarget(stress.targetType)}";
            return $"Heal {NumberColor}{-stress.amount}{ColorEnd} Stress on {DescribeTarget(stress.targetType)}";
        }

        private static string BuildDrawCards(DrawCardsSpec draw)
        {
            if (draw.count <= 0) return string.Empty;
            return draw.count == 1
                ? $"Draw {NumberColor}1{ColorEnd} card"
                : $"Draw {NumberColor}{draw.count}{ColorEnd} cards";
        }

        private static string DescribeTarget(ActionTargetType t)
        {
            switch (t)
            {
                case ActionTargetType.Self: return "self";
                case ActionTargetType.Musician: return "a bandmate";
                case ActionTargetType.AllMusicians: return "all bandmates";
                case ActionTargetType.RandomMusician: return "a random bandmate";
                case ActionTargetType.AudienceCharacter: return "target";
                case ActionTargetType.AllAudienceCharacters: return "all audience";
                case ActionTargetType.RandomAudienceCharacter: return "a random audience member";
                default: return t.ToString();
            }
        }
    }
}
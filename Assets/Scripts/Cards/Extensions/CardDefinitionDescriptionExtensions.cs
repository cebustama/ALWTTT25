// Place at: Assets/Scripts/Cards/Extensions/CardDefinitionDescriptionExtensions.cs
// FULL REPLACEMENT (CARD-FACE-1 / Parte A'). Built on the Parte A file, which was built on the repo
// copy attached 2026-09-16 (R4).
//
// [D-CF-13=C] What changed and why:
//  - CARD FACE: the role is now the GLYPH ALONE. The word is gone. The explanation moves to the card
//    hover stack (CardBase.ShowTooltipInfo), which is the mechanism the keyword icons already use:
//    symbol on the face, panel on hover. The face is for density; the panel is for meaning.
//  - MODAL: every <link=...> tag is REMOVED. Nothing in the project parses TMP link tags
//    (F-CF-5: no FindIntersectingLink, no linkInfo reader anywhere), so those tags rendered as
//    nothing and promised a hover that could not fire. A tag no reader reads is noise in the golden
//    and a false claim in the source.
//  - MODAL: since there is no hover here, the opaque symbols carry their WORD beside them: the role
//    keeps glyph + name, and scope/timing become glyph + the concept's displayName (ui-side, from
//    the 18 concepts imported in step 15). The concepts' `description` field stays dormant until a
//    link handler exists; that is declared, not hidden.
//  - Modifier FAMILY glyphs (mod_*) keep no word: the value beside them already names the concept
//    ("5/4", "Modulate to Mediant"), and a word would be redundant on every single line.
//
// Unchanged from Parte A: keyword row on the face, DEV-gated asset names, ASCII source (F-T6-1).

using System.Collections.Generic;
using System.Text;
using ALWTTT.Cards.Effects;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Tooltips;
using MidiGenPlay;                                    // TrackRole
using static ALWTTT.Tooltips.GameTextTemplateIds;

namespace ALWTTT.Cards
{
    public static class CardDefinitionDescriptionExtensions
    {
        // U+00B7 mid dot, escaped on purpose (Latin-1, present in the SDF atlas).
        private const string Sep = " \u00B7 ";

        // [D-CF-14] Face modifier row. Formatting, not text: tune freely, no glossary involved.
        // tint=1 is what makes the colour reach the glyph at all (D-CF-11=B).
        private const int ModifierGlyphSize = 60;                   // % of the face's body size
        private const string ModifierGlyphColor = "<color=#B8C4D0>"; // desaturated: subordinate to the role glyph
        private const string ColorEnd = "</color>";
        private const string ModifierRowSep = "\n";                 // one modifier per line; " " puts them on one

        public static string GetDescription(
            this CardDefinition card, BandCharacterStats stats = null)
        {
            if (card == null) return string.Empty;
            if (!card.HasPayload) return "Missing payload.";   // authoring-error diagnostic

            string body;
            if (card.Payload is CompositionCardPayload comp)
                body = BuildCompositionDescription(comp);
            else if (card.Payload is ActionCardPayload action)
                body = CardEffectDescriptionBuilder.BuildList(action.Effects, stats);
            else
                return $"Unsupported payload: {card.Payload.GetType().Name}";

            return AppendKeywordRow(body, card);
        }

        /// <summary>
        /// Full detail description for the right-click card detail modal (M1.10).
        /// Keywords are not repeated here: the modal renders them through the glossary itself.
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

        #region Shared fragments

        /// <summary>[D-CF-13=C] Face: glyph only. The word lives in the card hover panel.</summary>
        private static string RoleGlyph(TrackRole role) => GameSymbols.Role(role);

        /// <summary>Modal: glyph + localized role name. No hover here, so the word is shown.</summary>
        private static string RoleGlyphAndName(TrackRole role)
            => GameSymbols.Role(role) + " " + GameText.Format(UiRole(role));

        /// <summary>[F-TXT-3-6] One glyph per keyword, appended as the last face line.</summary>
        private static string AppendKeywordRow(string body, CardDefinition card)
        {
            var kws = card.Keywords;
            if (kws == null || kws.Count == 0) return body;

            var sb = new StringBuilder(body);
            if (sb.Length > 0) sb.Append('\n');
            for (int i = 0; i < kws.Count; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(GameSymbols.Keyword(kws[i]));
            }
            return sb.ToString();
        }

        #endregion

        #region Card-face description

        private static string BuildCompositionDescription(CompositionCardPayload p)
        {
            var sb = new StringBuilder();
            switch (p.PrimaryKind)
            {
                case CardPrimaryKind.Track:
                    sb.Append(p.TrackAction != null ? RoleGlyph(p.TrackAction.role) : GameText.Format(UiCardKindTrack));
                    break;
                case CardPrimaryKind.Part:
                    sb.Append(p.PartAction != null ? FormatPartAction(p.PartAction) : GameText.Format(UiCardKindPart));
                    break;
                default:
                    sb.Append(GameText.Format(UiCardKindComposition));
                    break;
            }

            // [D-CF-14] The "N modifiers" sentence is replaced by one glyph per modifier. The COUNT
            // survives as repetition -- two tempo effects draw two glyphs -- and the KIND is added,
            // which the sentence never carried. Size and colour subordinate this row to the role
            // glyph above it: the role is what the card IS, the modifiers are what it CARRIES.
            // Both are formatting and stay in code, like CardEffectDescriptionBuilder's colours.
            // (The card hover explains each family, CardBase.ShowTooltipInfo.)
            if (p.ModifierEffects != null)
            {
                var row = new StringBuilder();
                foreach (var fx in p.ModifierEffects)
                {
                    if (fx == null) continue;
                    string glyph = PartEffectDescriptionBuilder.FamilySpriteName(fx);
                    if (glyph == null) continue;   // unmapped subclass: silent on the face, named in the modal
                    // [D-CF-14b] Glyph + VALUE. The glyph says which knob, the value says where it is
                    // set -- "meter" alone is a category, "5/4" is the card. The value comes from the
                    // same formatter the modal uses (ValueOnly), so hand and modal cannot disagree.
                    // One modifier per LINE: a card is taller than it is wide, and "Modulate to
                    // Mediant" on a shared line wraps badly. Swap ModifierRowSep to " " for one line.
                    if (row.Length > 0) row.Append(ModifierRowSep);
                    row.Append(GameSymbols.Tag(glyph)).Append(' ')
                       .Append(PartEffectDescriptionBuilder.ValueOnly(fx));
                }

                if (row.Length > 0)
                    sb.Append("\n<size=").Append(ModifierGlyphSize).Append("%>")
                      .Append(ModifierGlyphColor).Append(row).Append(ColorEnd)
                      .Append("</size>");
            }

            return sb.ToString();
        }

        #endregion

        #region Detail-view description (M1.10)

        private static string BuildCompositionDetailDescription(
            CompositionCardPayload p, BandCharacterStats stats)
        {
            var sb = new StringBuilder();

            switch (p.PrimaryKind)
            {
                case CardPrimaryKind.Track:
                    sb.Append("<b>").Append(GameText.Format(UiCardKindTrack)).Append("</b>");
                    if (p.TrackAction != null)
                    {
                        sb.Append(Sep).Append(RoleGlyphAndName(p.TrackAction.role));

#if ALWTTT_DEV
                        // [F-CF-2] Asset name: a developer label by DATA. Same gate as the track
                        // hover's Bundle line (D-T6-4=A).
                        string bundleName = p.TrackAction.styleBundle != null
                            ? p.TrackAction.styleBundle.name
                            : null;
                        if (!string.IsNullOrEmpty(bundleName))
                            sb.Append('\n').Append(GameText.Format(UiCardDetailStyle, bundleName));
#endif
                    }
                    break;

                case CardPrimaryKind.Part:
                    sb.Append("<b>").Append(GameText.Format(UiCardKindPart)).Append("</b>");
                    if (p.PartAction != null)
                    {
                        sb.Append(Sep).Append(FormatPartAction(p.PartAction));   // Parte B

#if ALWTTT_DEV
                        if (!string.IsNullOrEmpty(p.PartAction.musicianId))
                            sb.Append('\n').Append(GameText.Format(UiCardDetailMusician, p.PartAction.musicianId));
#endif
                    }
                    break;

                default:
                    sb.Append("<b>").Append(GameText.Format(UiCardKindComposition)).Append("</b>");
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
                    sb.Append("\n\n<b>").Append(GameText.Format(UiCardDetailModifiers)).Append("</b>");
                    foreach (var fx in p.ModifierEffects)
                    {
                        if (fx == null) continue;
                        sb.Append("\n  - ").Append(PartEffectDescriptionBuilder.Build(fx));
                        sb.Append("  <size=80%>").Append(PartEffectDescriptionBuilder.ScopeAndTiming(fx)).Append("</size>");
                    }
                }
            }

            // --- CardPayload.Effects (shared effect pipeline) ---
            var effects = p.Effects;
            if (effects != null && effects.Count > 0)
            {
                if (CardEffectDescriptionBuilder.HasAnyLine(effects, stats))
                {
                    string effectsBlock = CardEffectDescriptionBuilder.BuildList(effects, stats);
                    sb.Append("\n\n<b>").Append(GameText.Format(UiCardDetailEffects)).Append("</b>\n");
                    sb.Append(effectsBlock);
                }
            }

            return sb.ToString();
        }

        #endregion

        // [Parte B] Still an enum name / asset text. PartActionDescriptor.cs not yet in session.
        private static string FormatPartAction(PartActionDescriptor pa) =>
            string.IsNullOrWhiteSpace(pa.customLabel)
                ? pa.action.ToString()
                : pa.customLabel;
    }
}
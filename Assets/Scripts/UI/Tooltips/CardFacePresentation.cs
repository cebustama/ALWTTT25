// Place at: Assets/Scripts/UI/Tooltips/CardFacePresentation.cs
// [CARD-FACE-2 / D-CF-19=B] The text-vs-symbol switch the playtest asked for. It is a DESIGN
// COMPARISON tool, not a player option: runtime static so a Dev Mode toggle can flip it in the same
// session with the same hand, no rebuild, no persistence, no menu. Default = symbols, which is what
// ships and what the golden dump records. Only the CARD FACE reads it: the detail modal always shows
// glyph + word (D-CF-13=C) and is unaffected.
//
// Takes effect when a face is (re)built: CardBase.SetCard / UpdateDescription. Cards already in hand
// keep their text until they are rebuilt; the Dev tab says so.
//
// SOURCE IS PURE ASCII ON PURPOSE (F-T6-1 / F-SP1-2).

namespace ALWTTT.Tooltips
{
    public static class CardFacePresentation
    {
        /// <summary>True (default): categorical enums draw as glyphs on the card face.
        /// False: the same slots draw the glossary WORD instead (comparison only).</summary>
        public static bool UseSymbols = true;


        /// <summary>[CARD-FACE-2b / D-CF-25=B] False (default for now): the ACTION card type
        /// (Charisma, Technique...) is not shown to the player anywhere -- face glyph, hover panel and
        /// detail-modal line are gated by this one switch, so no glyph can survive without its
        /// explanation (D-CF-13=C). Code, templates, concepts and ct_* glyphs stay, inert (precedent
        /// ui_track_level). "Composition" is not a CardType and is not affected.</summary>
        public static bool ShowCardType = false;

        /// <summary>[CARD-FACE-2b / D-CF-26=A] Tooltip header for a panel that explains a face glyph:
        /// the glyph repeated before the name, so the reader links the symbol in the card text to its
        /// explanation. Follows UseSymbols: when the face shows words, the header shows the word alone.
        /// A null/empty glyph (unmapped) yields the bare name -- never a broken sprite tag.</summary>
        public static string HeaderWithGlyph(string glyphTag, string name)
            // [CARD-FACE-3 / F-CF2b-1] U+00A0 (no-break space) between glyph and name: TMP does not
            // wrap at it, so a long name ("Acompanamiento") can no longer leave the glyph alone on
            // the first line. Written as an escape to keep the source ASCII. The glyph, the space and
            // the name's FIRST word now form one unit for line breaking; a second word still wraps.
            => UseSymbols && !string.IsNullOrEmpty(glyphTag) ? glyphTag + "\u00A0" + name : name;
    }
}
// Place at: Assets/Scripts/UI/Tooltips/GameSymbols.cs
// [CARD-FACE-1 / D-CF-1, D-CF-4, D-CF-9=A'] The SYMBOL vocabulary: the names under which a glyph is
// looked up in the ONE global TMP Sprite Asset (SSoT_Game_Text section 2.1), derived from the same
// key that names the concept - an enum value or a family constant - never typed by hand per call
// site and never an atlas index. The source texture's slice names ARE this contract: art may be
// replaced under the same names; a name that disappears is a red row in the editor, not a silent
// blank on a card.
//
// Sprite ids live in a namespace SEPARATE from concept ids (section 2.1): "tt_melody" is a glyph,
// "role_melody" is a glossary entry. Both are derived here from the same enum value so that the
// two can never disagree about which value they describe.
//
// Coverage: `AllSpriteNames` is the set the editor's sprite pass must find in the atlas. It is
// enumerated at type init from the enums (F-TV-5: a derived family is never a hand-written list).
// The editor pass itself is not wired by CARD-FACE-1 (proposal against SSoT_Editor_Authoring_Tools).
//
// SOURCE IS PURE ASCII ON PURPOSE (F-T6-1 / F-SP1-2).

using System.Collections.Generic;
using ALWTTT.Cards;      // EffectScope, ApplyTiming
using ALWTTT.Enums;      // SpecialKeywords
using MidiGenPlay;       // TrackRole

namespace ALWTTT.Tooltips
{
    public static class GameSymbols
    {
        // -- Sprite-name prefixes (section 2.1 declares tt_, mod_, mt_; CARD-FACE-1 adds kw_, sc_, tm_) --
        public const string RolePrefix = "tt_";      // track type / TrackRole
        public const string KeywordPrefix = "kw_";   // SpecialKeywords
        public const string ScopePrefix = "sc_";     // EffectScope
        public const string TimingPrefix = "tm_";    // ApplyTiming
        public const string ModifierPrefix = "mod_"; // modifier family (fixed names below)

        // -- Modifier-family glyphs (fixed set: one per PartEffect family) --
        public const string ModMeter = "mod_meter";
        public const string ModTempo = "mod_tempo";
        public const string ModKey = "mod_key";
        public const string ModInstrument = "mod_instrument";
        public const string ModTonality = "mod_tonality";   // consumer arrives with TonalityEffect (Part B)
        public const string ModDirUp = "mod_dir_up";        // ModulationOctaveHint.Up
        public const string ModDirDown = "mod_dir_down";    // ModulationOctaveHint.Down

        // -- Sprite NAMES (for the coverage set and for Image lookups) --
        public static string RoleName(TrackRole r) => RolePrefix + r.ToString().ToLowerInvariant();
        public static string KeywordName(SpecialKeywords k) => KeywordPrefix + k.ToString().ToLowerInvariant();
        public static string ScopeName(EffectScope s) => ScopePrefix + s.ToString().ToLowerInvariant();
        public static string TimingName(ApplyTiming t) => TimingPrefix + t.ToString().ToLowerInvariant();

        // -- Inline TMP tags --
        // [D-CF-11=B] tint=1 makes the glyph inherit the surrounding text colour, so a monochrome
        // placeholder atlas stays visible on any background and can be coloured with <color=...>.
        public static string Tag(string spriteName) => "<sprite name=\"" + spriteName + "\" tint=1>";
        public static string Role(TrackRole r) => Tag(RoleName(r));
        public static string Keyword(SpecialKeywords k) => Tag(KeywordName(k));
        public static string Scope(EffectScope s) => Tag(ScopeName(s));
        public static string Timing(ApplyTiming t) => Tag(TimingName(t));

        // -- Concept ids for the hover detail (glossary entries, NOT sprite names) --
        // Same derivation, different namespace. A missing entry follows the section 3 miss policy:
        // the hover shows the id, never nothing.
        public static string RoleConcept(TrackRole r) => "role_" + r.ToString().ToLowerInvariant();
        public static string ScopeConcept(EffectScope s) => "scope_" + s.ToString().ToLowerInvariant();
        public static string TimingConcept(ApplyTiming t) => "timing_" + t.ToString().ToLowerInvariant();
        public const string ModMeterConcept = "mod_meter";
        public const string ModTempoConcept = "mod_tempo";
        public const string ModKeyConcept = "mod_key";
        public const string ModInstrumentConcept = "mod_instrument";
        public const string ModTonalityConcept = "mod_tonality";

        /// <summary>Wrap inline content in a hover link to a glossary concept.</summary>
        public static string Link(string conceptId, string inner) => "<link=" + conceptId + ">" + inner + "</link>";

        /// <summary>Coverage set for the editor's sprite pass: every name a producer may emit.</summary>
        public static readonly string[] AllSpriteNames = BuildAll();

        private static string[] BuildAll()
        {
            var seen = new HashSet<string>(System.StringComparer.Ordinal);
            var list = new List<string>(32);
            void Add(string n) { if (seen.Add(n)) list.Add(n); }
            Add(ModMeter); Add(ModTempo); Add(ModKey); Add(ModInstrument); Add(ModTonality);
            Add(ModDirUp); Add(ModDirDown);
            foreach (TrackRole r in System.Enum.GetValues(typeof(TrackRole))) Add(RoleName(r));
            foreach (SpecialKeywords k in System.Enum.GetValues(typeof(SpecialKeywords))) Add(KeywordName(k));
            foreach (EffectScope s in System.Enum.GetValues(typeof(EffectScope))) Add(ScopeName(s));
            foreach (ApplyTiming t in System.Enum.GetValues(typeof(ApplyTiming))) Add(TimingName(t));
            return list.ToArray();
        }
    }
}
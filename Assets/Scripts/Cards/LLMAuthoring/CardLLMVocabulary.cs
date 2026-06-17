using System.Collections.Generic;

using TimeSignature = MidiGenPlay.MusicTheory.MusicTheory.TimeSignature;

namespace ALWTTT.Cards.LLMAuthoring
{
    /// <summary>
    /// Stage-1 "vocabulary" for the card LLM adopter (CE-L1, D-CE-L1.4):
    /// a pure snapshot POCO assembled at generate time, NOT a hand-authored
    /// ScriptableObject like the drum/chord adopters use. Rationale: the card
    /// domain's vocabulary is live project state — enum members, registered
    /// status keys, existing palettes and part-effects — which would rot as a
    /// hand-edited asset. The window-side <c>CardLLMVocabularyBuilder</c>
    /// (Assembly-CSharp-Editor) assembles it; this assembly only defines the
    /// shape, keeping it free of ALWTTT game types so the pipeline stays
    /// unit-testable from the test asmdef.
    ///
    /// Everything is strings/numbers by design: this is exactly the alphabet
    /// the prompt declares and the response handler validates against.
    /// </summary>
    public sealed class CardLLMVocabulary
    {
        // ---- enum alphabets (names exactly as the staging path resolves them) ----
        public IReadOnlyList<string> CardKinds = new[] { "Action", "Composition" };
        public IReadOnlyList<string> PerformerRules;
        public IReadOnlyList<string> MusicianTypes;
        public IReadOnlyList<string> CardTypes;
        public IReadOnlyList<string> Rarities;
        public IReadOnlyList<string> AudioTypes;
        public IReadOnlyList<string> SpecialKeywords;
        public IReadOnlyList<string> ActionTargetTypes;
        public IReadOnlyList<string> ActionTimings;
        public IReadOnlyList<string> TrackRoles;
        public IReadOnlyList<string> PrimaryKinds;
        public IReadOnlyList<string> PartActionKinds;
        public IReadOnlyList<string> AcquisitionFlags;
        public IReadOnlyList<string> TimeSignatures;

        // ---- project-registered intent targets ----

        /// <summary>Canonical status keys (StatusEffectSO.StatusKey) the LLM may use in ApplyStatusEffect.</summary>
        public IReadOnlyList<string> StatusKeys;

        /// <summary>Existing PartEffect asset names the LLM may use in modifierEffectNames.</summary>
        public IReadOnlyList<string> ModifierEffectNames;

        /// <summary>Drum palettes available for Rhythm-role palette intent.</summary>
        public IReadOnlyList<PaletteDescriptor> RhythmPalettes;

        /// <summary>Chord palettes available for Backing-role palette intent.</summary>
        public IReadOnlyList<PaletteDescriptor> BackingPalettes;
    }

    /// <summary>
    /// Asset-free summary of one project palette, for prompt context and for
    /// deterministic intent resolution. Carries raw per-entry metric numbers
    /// (not precomputed <c>TsFeatures</c>) because the drum density feature is
    /// a function of the DESIRED time signature (CE-F1
    /// <c>PaletteSelector.DrumStartsPerBar</c> caps by the desired meter's
    /// grouping count), so features must be computed at resolve time.
    /// </summary>
    public sealed class PaletteDescriptor
    {
        /// <summary>
        /// Stable identity used to map back to the asset window-side AND as the
        /// deterministic sort key (project-scan order is not stable; the
        /// resolver sorts by this before picking). Use the asset path.
        /// </summary>
        public string Id;

        /// <summary>Palette display name (paletteDisplayName fallback asset name).</summary>
        public string DisplayName;

        /// <summary>Authoring notes (paletteNotes). Keyword matching runs over DisplayName + Notes.</summary>
        public string Notes;

        /// <summary>True for drum palettes (density = capped foundational onsets); false for chord palettes (density = harmonic rhythm).</summary>
        public bool IsDrumDomain;

        public List<PaletteEntryDescriptor> Entries = new List<PaletteEntryDescriptor>();
    }

    /// <summary>Raw metric shape of one palette entry (see <see cref="PaletteDescriptor"/>).</summary>
    public struct PaletteEntryDescriptor
    {
        public TimeSignature TimeSignature;
        public int Subdivisions;
        public int Measures;

        /// <summary>
        /// Structural onsets across the whole entry: chord-change count for
        /// chord progressions, foundational (kick) onset count for drum
        /// patterns — the same inputs CE-F1's typed finders feed to
        /// <c>PaletteSelector.StartsPerBar</c> / <c>DrumStartsPerBar</c>.
        /// </summary>
        public int StructuralOnsets;
    }
}

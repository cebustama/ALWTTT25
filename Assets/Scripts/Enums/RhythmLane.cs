namespace ALWTTT.Enums
{
    /// <summary>
    /// [RFX-1] Presentation-only grouping of MIDI events into particle VFX lanes.
    ///
    /// NOT a gameplay concept. Never key game logic, scoring, status effects or
    /// card resolution on this enum - it exists so a designer can tune "what the
    /// snare looks like" without touching the GM percussion table. If gameplay
    /// ever needs to know about drum elements, that belongs in a game-owned type
    /// under a governed SSoT, not here.
    ///
    /// Explicit numeric values: this enum is serialized inside RhythmFxConfigSO
    /// assets, so inserting a member in the middle without a value would silently
    /// re-map every authored lane entry.
    /// </summary>
    public enum RhythmLane
    {
        Kick = 0,
        Snare = 1,
        HiHatClosed = 2,
        HiHatOpen = 3,
        Tom = 4,
        Cymbal = 5,

        /// <summary>
        /// Catch-all for GM percussion notes with no dedicated lane. Deliberately
        /// UNAUTHORED in RhythmFxConfig.asset: reaching it produces a diagnostic
        /// line, not a particle. [RFX-2] That diagnostic now exists - see
        /// RhythmParticleMidiListener.NoteUnmappedToPerc.
        /// </summary>
        Perc = 6,

        /// <summary>
        /// [RFX-1] Legacy single harmony lane.
        ///
        /// [RFX-2 / D2=B] Retained as the FALLBACK for a ladder rung that is not
        /// yet authored (no ParticleSystem wired, or no enabled asset entry).
        /// Slated for retirement under D2=A once every rung is authored and
        /// RhythmParticleMidiListener.ChordLadderFallbacks reads 0 across a full
        /// song. The VALUE 7 stays reserved forever regardless of retirement:
        /// re-using it for a future lane would silently re-map any asset still
        /// carrying a Chord entry.
        /// </summary>
        Chord = 7,

        // ------------------------------------------------------------------
        // [RFX-2 / D-S2-CHORD=A / D1=B] Chord voice-count ladder.
        //
        // Keyed on the number of DISTINCT PITCH CLASSES in the chord, not on
        // raw ChordEvent.notes.Count and not on ChordEvent.quality:
        //   - quality is null on every LABEL MISS, so a quality-keyed ladder
        //     would have holes;
        //   - raw count is inflated by octave doubling, so a root/fifth/octave
        //     power chord would draw the triad sprite.
        //
        // Appended at 8..12. These values are serialized in
        // RhythmFxConfig.asset: append only, never renumber, never reuse.
        // ------------------------------------------------------------------

        /// <summary>1 distinct pitch class. Rare: only reachable as an
        /// octave-stacked unison of 3+ notes, because MidiMusicManager raises
        /// OnChord only when 2+ notes sound at the same tick.</summary>
        ChordSingle = 8,

        /// <summary>2 distinct pitch classes. The guitar power chord voiced
        /// root/fifth/octave-root lands here. A BARE two-note power chord does
        /// not reach the listener at all: chordMinNotes (D3=A) gates on raw note
        /// count and rejects it along with bass double-stops.</summary>
        ChordPower = 9,

        /// <summary>3 distinct pitch classes. The commonest rung; its asset
        /// entry is the tuning baseline the other four deviate from.</summary>
        ChordTriad = 10,

        /// <summary>4 distinct pitch classes.</summary>
        ChordSeventh = 11,

        /// <summary>5 or more distinct pitch classes.</summary>
        ChordExtended = 12
    }
}
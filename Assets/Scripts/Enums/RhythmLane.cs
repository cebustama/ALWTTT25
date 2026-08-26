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
        Perc = 6,
        Chord = 7
    }
}
namespace ALWTTT.Music
{
    /// <summary>
    /// [PAUSE-1] ALWTTT-owned suspension seam for the MIDI transport.
    ///
    /// Why a separate interface and not new members on IPlayMidi: IPlayMidi is declared
    /// package-side (MidiGenPlay); a repo-wide grep of Assets/ finds only its implementer
    /// (MidiToolkitAdapter) and its consumers, never its declaration. Adding members there
    /// would be a MidiGenPlay contract change. This interface lives in ALWTTT, is
    /// implemented by the ALWTTT adapter, and is discovered by MidiMusicManager with a
    /// type test ("player is IMidiTransportSuspend"). A player that does not implement it
    /// degrades to "music keeps playing while paused", logged once, never silently.
    ///
    /// Contract:
    /// - Suspend() freezes the sequencer IN PLACE: CurrentTick stops advancing and resumes
    ///   from the same tick on Resume(). No end-of-song event is raised by a suspension.
    /// - While suspended, IPlayMidi.IsPlaying keeps reporting true (the song has not
    ///   ended), so every "wait while playing" loop keeps waiting instead of finishing.
    /// - Suspend() while nothing plays is a no-op; Resume() while not suspended is a no-op.
    /// - Stop() and Play() clear the suspension.
    ///
    /// Speed control (SPEED-1, D7=B) is intentionally NOT part of this seam.
    /// Authority: SSoT_Audio.md (transport seam) / SSoT_Runtime_Flow.md (pause state).
    /// </summary>
    public interface IMidiTransportSuspend
    {
        bool IsSuspended { get; }
        void Suspend();
        void Resume();
    }
}
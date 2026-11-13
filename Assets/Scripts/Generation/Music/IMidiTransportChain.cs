namespace ALWTTT.Music
{
    /// <summary>
    /// [AUDIO-LOOP-1] ALWTTT-owned seam for chaining one MIDI after another WITHOUT cutting
    /// what is still ringing (cymbal tails, chord and bass releases) at the loop boundary.
    ///
    /// Same reasoning as IMidiTransportSuspend: IPlayMidi is declared package-side
    /// (MidiGenPlay), so this lives in ALWTTT, is implemented by MidiToolkitAdapter and is
    /// discovered by MidiMusicManager with a type test. A player without it degrades to the
    /// old hard cut (Stop + Play).
    ///
    /// Contract:
    /// - PlayChained(data) is only valid when the previous playback has ENDED NATURALLY
    ///   (IPlayMidi.IsPlaying == false). At that point every note-off of the previous MIDI has
    ///   been processed, so only releases/tails remain: letting them ring cannot leave a
    ///   hanging note. Interrupting a playing MIDI must keep using Stop() + Play().
    /// - Voices of the previous playback are released (envelope release, never a hard cut)
    ///   after a configurable safety delay, so a stray voice can never ring forever.
    /// - Sequencer, events (OnSongStarted, OnMidiEvents) and timing are identical to Play().
    /// Authority: SSoT_Audio.md (transport seam).
    /// </summary>
    public interface IMidiTransportChain
    {
        /// <summary>False when chaining is switched off (A/B debugging): callers fall back
        /// to Stop() + Play().</summary>
        bool ChainingEnabled { get; }

        void PlayChained(byte[] data);
    }
}

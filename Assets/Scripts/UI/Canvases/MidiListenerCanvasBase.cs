using ALWTTT.Managers;
using ALWTTT.Music;
using ALWTTT.UI;
using UnityEngine;

namespace ALWTTT.UI
{
    /// <summary>
    /// Implements all MIDI callbacks and auto-registers with MidiMusicManager.
    /// Override the protected virtual handlers in derived classes.
    /// </summary>
    public class MidiListenerCanvasBase : CanvasBase,
        IMidiNoteListener, IChordListener, IBeatGridListener,
        ITempoSignatureListener, IDrumKickListener
    {
        protected MidiMusicManager MidiMusicManager => MidiMusicManager.Instance;

        #region Auto-register
        protected virtual void OnEnable()
        {
            if (!MidiMusicManager) return;
            MidiMusicManager.Register((IMidiNoteListener)this);
            MidiMusicManager.Register((IChordListener)this);
            MidiMusicManager.Register((IBeatGridListener)this);
            MidiMusicManager.Register((IDrumKickListener)this);
            MidiMusicManager.Register((ITempoSignatureListener)this);
        }

        protected virtual void OnDisable()
        {
            if (!MidiMusicManager) return;
            MidiMusicManager.Unregister((IMidiNoteListener)this);
            MidiMusicManager.Unregister((IChordListener)this);
            MidiMusicManager.Unregister((IBeatGridListener)this);
            MidiMusicManager.Unregister((IDrumKickListener)this);
            MidiMusicManager.Unregister((ITempoSignatureListener)this);
        }
        #endregion

        #region Listeners
        void IMidiNoteListener.OnMidiNote(MidiTaggedEvent e) => OnMidiNote(e);
        protected virtual void OnMidiNote(MidiTaggedEvent e) { }
        void IChordListener.OnChord(ChordEvent e) => OnChord(e);
        protected virtual void OnChord(ChordEvent e) { }
        void IBeatGridListener.OnBeat(BeatGridEvent e) => OnBeat(e);
        void IBeatGridListener.OnDownbeat(BeatGridEvent e) => OnDownbeat(e);
        protected virtual void OnBeat(BeatGridEvent e) { }
        protected virtual void OnDownbeat(BeatGridEvent e) { }
        void ITempoSignatureListener.OnTempoChanged(double bpm) => OnTempoChanged(bpm);
        void ITempoSignatureListener.OnTimeSignatureChanged(int n, int d) => OnTimeSignatureChanged(n, d);
        protected virtual void OnTempoChanged(double bpm) { }
        protected virtual void OnTimeSignatureChanged(int numerator, int denominator) { }
        void IDrumKickListener.OnDrumKick(MidiTaggedEvent e) => OnDrumKick(e);
        protected virtual void OnDrumKick(MidiTaggedEvent e) { }
        #endregion
    }
}

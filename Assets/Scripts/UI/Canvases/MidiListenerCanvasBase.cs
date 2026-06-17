using ALWTTT.Managers;
using ALWTTT.Music;
using System.Collections;
using UnityEngine;

namespace ALWTTT.UI
{
    /// <summary>
    /// Implements all MIDI callbacks and auto-registers with MidiMusicManager.
    /// If the manager isn't ready at OnEnable, waits until it's available.
    /// </summary>
    public class MidiListenerCanvasBase : CanvasBase,
        IMidiNoteListener, IChordListener, IBeatGridListener,
        ITempoSignatureListener, IDrumKickListener
    {
        protected MidiMusicManager Midi => MidiMusicManager.Instance;

        [Header("Debug")]
        [SerializeField] private bool debugRegister = true;

        Coroutine _waitCo;
        bool _registered;

        #region Auto-register
        protected virtual void OnEnable()
        {
            TryRegister();
            if (!_registered) _waitCo = StartCoroutine(CoWaitAndRegister());
        }

        protected virtual void OnDisable()
        {
            if (_waitCo != null) { StopCoroutine(_waitCo); _waitCo = null; }
            TryUnregister();
        }

        IEnumerator CoWaitAndRegister()
        {
            if (debugRegister) Debug.Log($"[MLBase] Waiting for MidiMusicManager...", this);
            yield return new WaitUntil(() => MidiMusicManager.Instance != null);
            TryRegister();
            _waitCo = null;
        }

        void TryRegister()
        {
            var mm = MidiMusicManager.Instance;
            if (mm == null || _registered) return;

            mm.Register((IMidiNoteListener)this);
            mm.Register((IChordListener)this);
            mm.Register((IBeatGridListener)this);
            mm.Register((IDrumKickListener)this);
            mm.Register((ITempoSignatureListener)this);

            _registered = true;
            if (debugRegister) Debug.Log("[MLBase] Registered with MidiMusicManager.", this);
        }

        void TryUnregister()
        {
            if (!_registered) return;
            var mm = MidiMusicManager.Instance;
            if (mm != null)
            {
                mm.Unregister((IMidiNoteListener)this);
                mm.Unregister((IChordListener)this);
                mm.Unregister((IBeatGridListener)this);
                mm.Unregister((IDrumKickListener)this);
                mm.Unregister((ITempoSignatureListener)this);
            }
            _registered = false;
            if (debugRegister) Debug.Log("[MLBase] Unregistered from MidiMusicManager.", this);
        }
        #endregion

        #region Listeners (virtual forwards)
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

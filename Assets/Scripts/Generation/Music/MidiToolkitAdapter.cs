using MidiGenPlay;
using MidiPlayerTK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Music
{
    /// <summary>
    /// ALWTTT transport adapter over MPTK's MidiFilePlayer. Implements the package-side
    /// IPlayMidi contract plus the ALWTTT-side IMidiTransportSuspend seam (PAUSE-1).
    ///
    /// [PAUSE-1] Suspension = MPTK_Pause / MPTK_UnPause: the sequencer freezes in place and
    /// resumes from the same tick. While suspended, IsPlaying stays true on purpose (the
    /// song has not ended), independent of what MPTK_IsPlaying reports during a pause, so
    /// MidiMusicManager's beat-grid loop, WaitForEnd and tick-crossing guard keep waiting
    /// instead of treating the pause as a song end.
    /// </summary>
    public class MidiToolkitAdapter : MonoBehaviour, IPlayMidi, IMidiTransportSuspend,
        IMidiTransportChain
    {
        MidiFilePlayer _player;
        bool _suspended;

        [Header("AUDIO-LOOP-1 · Loop chaining")]
        [Tooltip("When on, a loop that follows a loop which ended on its own does not cut " +
                 "what is still ringing (crash tails, chord releases). Off = old hard cut.")]
        [SerializeField] bool chainWithoutCut = true;

        [Tooltip("Seconds after a chained start before the previous loop's voices are sent " +
                 "to release (envelope, not a cut). Safety net against a voice that never ends.")]
        [SerializeField, Min(0.1f)] float chainTailReleaseSeconds = 3f;

        [SerializeField] bool logChaining = true;

        float _lastEndRealtime = -1f;   // [AUDIO-LOOP-1] for the gap measure in the log


        public event Action<List<MPTKEvent>> OnMidiEvents;
        public event Action OnSongStarted;
        public event Action OnSongEnded;

        void Awake()
        {
            // look for the player anywhere in our children
            _player = GetComponentInChildren<MidiFilePlayer>();
            if (_player == null)
            {
                Debug.LogError(
                    $"[{name}] needs a child GameObject with MidiFilePlayer on it.", this);
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (GetComponentInChildren<MidiFilePlayer>(true) == null)
            {
                Debug.LogError(
                    $"{name} requires a child with MidiFilePlayer; please add one.",
                    this
                );
            }
        }
#endif

        private void OnEnable()
        {
            if (_player == null) return;
            _player.OnEventNotesMidi.AddListener(HandleNotes);       // fired per tick group
            _player.OnEventStartPlayMidi.AddListener(OnStart);
            _player.OnEventEndPlayMidi.AddListener(OnEnd);
        }

        private void OnDisable()
        {
            if (_player == null) return;
            _player.OnEventNotesMidi.RemoveListener(HandleNotes);
            _player.OnEventStartPlayMidi.RemoveListener(OnStart);
            _player.OnEventEndPlayMidi.RemoveListener(OnEnd);
        }

        void HandleNotes(List<MPTKEvent> evts) => OnMidiEvents?.Invoke(evts);
        void OnStart(string _) => OnSongStarted?.Invoke();

        void OnEnd(string midiName, EventEndMidiEnum reason)
        {
            // [PAUSE-1] Defensive: a suspension is not an end. If MPTK ever raised its end
            // event for a pause, forwarding it would clear the part markers and end the
            // composition loop under a frozen game. Stop() clears _suspended BEFORE
            // stopping, so a real stop always reaches this handler unsuspended.
            if (_suspended)
            {
                Debug.LogWarning(
                    $"[{name}] [PAUSE-1] end event while suspended (reason={reason}); ignored.",
                    this);
                return;
            }
            _lastEndRealtime = Time.realtimeSinceStartup;
            OnSongEnded?.Invoke();
        }

        public void Stop()
        {
            if (_player == null) return;
            ClearSuspension();
            _player.MPTK_Stop();
        }

        public void Play(byte[] data)
        {
            if (_player == null) return;
            ClearSuspension();
            _player.MPTK_ClearSoundOnStart = true;   // [AUDIO-LOOP-1] Maestro default
            _player.MPTK_Play(data);
        }

        #region [AUDIO-LOOP-1] IMidiTransportChain
        public bool ChainingEnabled => chainWithoutCut;

        public void PlayChained(byte[] data)
        {
            if (_player == null) return;
            if (!chainWithoutCut || _player.MPTK_IsPlaying || _suspended)
            {
                // Contract breach or switched off: keep the old, safe behaviour.
                Stop();
                Play(data);
                return;
            }

            // Voices of the loop that just ended carry this session id (Maestro bumps
            // IdSession when the next MIDI starts).
            int previousSession = _player.IdSession;
            // Active = includes voices in release (tails); MPTK_StatVoiceCountPlaying does not.
            int voicesRinging = _player.MPTK_StatVoiceCountActive;

            // Soft stop: resets the sequencer flags without ClearAllVoices.
            _player.MPTK_Stop(stopAllSound: false);
            _player.MPTK_ClearSoundOnStart = false;   // Maestro patch: skip the start clear
            _player.MPTK_Play(data);

            if (logChaining)
                Debug.Log($"[AUDIO-LOOP-1] Chained play: no cut. previousSession={previousSession} " +
                          $"gapMs={(_lastEndRealtime < 0f ? -1f : (Time.realtimeSinceStartup - _lastEndRealtime) * 1000f):0} " +
                          $"voicesRinging={voicesRinging} releaseIn={chainTailReleaseSeconds:0.0}s", this);

            // Not cancelled by later plays or stops: releasing a session that has no voice
            // left is a no-op, and each chained loop keeps its own safety net.
            StartCoroutine(ReleasePreviousSessionAfter(previousSession, voicesRinging));
        }

        IEnumerator ReleasePreviousSessionAfter(int session, int voicesAtChain)
        {
            yield return new WaitForSecondsRealtime(chainTailReleaseSeconds);
            if (_player == null) yield break;

            // Envelope release of that session only (NoteOffAll filtered by session id);
            // destroyAudioSource=false ⇒ no ClearAllVoices ⇒ no hard cut.
            Routine.RunCoroutine(_player.ThreadClearAllSound(false, session),
                Segment.RealtimeUpdate);
            if (logChaining)
                Debug.Log($"[AUDIO-LOOP-1] Safety release of previous loop voices " +
                          $"(session={session}, ringing at chain={voicesAtChain}).", this);
        }
        #endregion

        public bool IsPlaying => _player != null && (_player.MPTK_IsPlaying || _suspended);
        public long CurrentTick => _player != null ? _player.MPTK_TickCurrent : 0L;
        public double CurrentTimeSeconds => _player != null ?
            _player.MPTK_PlayTime.TotalSeconds : 0;
        public int DeltaTicksPerQuarterNote =>
            _player != null ? _player.MPTK_DeltaTicksPerQuarterNote : 96;

        #region [PAUSE-1] IMidiTransportSuspend
        public bool IsSuspended => _suspended;

        public void Suspend()
        {
            if (_player == null || _suspended) return;
            if (!_player.MPTK_IsPlaying) return;   // nothing to freeze (between songs)
            _player.MPTK_Pause();                  // indefinite pause, position kept
            _suspended = true;
        }

        public void Resume()
        {
            if (_player == null || !_suspended) return;
            _suspended = false;
            _player.MPTK_UnPause();
        }

        // A new Play or a Stop supersedes a suspension. Un-pausing first guarantees the
        // next song never inherits a paused sequencer.
        private void ClearSuspension()
        {
            if (!_suspended) return;
            _suspended = false;
            _player.MPTK_UnPause();
        }
        #endregion

        public void SetChannelVolume(int channel, int volume01_127)
        {
            if (_player == null) return;

            // Clamp channel and convert 0..127 -> 0..1
            channel = Mathf.Clamp(channel, 0, 15);
            float vol01 = Mathf.Clamp01(volume01_127 / 127f);

            // MidiFilePlayer exposes the channel controller via MPTK_Channels
            // (Volume, Mute/Enable, Pitch bend, ...)
            var channels = _player.MPTK_Channels;
            channels[channel].Volume = vol01;
        }

        public IEnumerator WaitForEnd()
        {
            // [PAUSE-1] Uses IsPlaying (not MPTK_IsPlaying) so a suspension keeps waiting.
            while (IsPlaying)
                yield return null; // next frame
        }
    }
}
using MidiGenPlay;
using MidiPlayerTK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Music
{
    public class MidiToolkitAdapter : MonoBehaviour, IPlayMidi
    {
        MidiFilePlayer _player;

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
        void OnEnd(string midiName, EventEndMidiEnum reason) => OnSongEnded?.Invoke();

        public void Stop() => _player?.MPTK_Stop();
        public void Play(byte[] data)
        {
            _player?.MPTK_Play(data);
        }

        public bool IsPlaying => _player != null && _player.MPTK_IsPlaying;

        public void SetChannelVolume(int channel, int volume01_127)
        {
            if (_player == null) return;

            // Clamp channel and convert 0–127 -> 0–1
            channel = Mathf.Clamp(channel, 0, 15);
            float vol01 = Mathf.Clamp01(volume01_127 / 127f);

            // MidiFilePlayer exposes the channel controller via MPTK_Channels
            // (Volume, Mute/Enable, Pitch bend, …)
            var channels = _player.MPTK_Channels;                 // MidiFilePlayer.cs comment shows this is the way to control per-channel settings
            channels[channel].Volume = vol01;                     // or channels.SetVolume(channel, vol01) if your wrapper exposes a setter
        }

        public IEnumerator WaitForEnd()
        {
            while (_player != null && _player.MPTK_IsPlaying)
                yield return null; // next frame
        }
    }
}
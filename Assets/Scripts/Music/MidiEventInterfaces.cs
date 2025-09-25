using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Music
{
    // Minimal, convenient payloads to broadcast around the game.
    public struct MidiTaggedEvent
    {
        public string musicianId;
        public int channel;
        public int note;      // 0..127
        public int velocity;  // 0..127
        public float time;    // seconds since song start (RealTime ms / 1000f)
        public Transform anchor; // optional: where to spawn FX/text
    }

    public struct ChordEvent
    {
        public string musicianId;
        public int channel;
        public List<int> notes; // chord notes at the same instant
        public float time;
        public Transform anchor;
    }

    public struct BeatEvent
    {
        public string sourceMusicianId; // usually drummer
        public int beatIndex;
        public float time;
        public Transform anchor;
    }

    public interface IMidiNoteListener { void OnMidiNote(MidiTaggedEvent e); }
    public interface IChordListener { void OnChord(ChordEvent e); }
    public interface IBeatSyncVFX { void OnBeat(BeatEvent e); }
}
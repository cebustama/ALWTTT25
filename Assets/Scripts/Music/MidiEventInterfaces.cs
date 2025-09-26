using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Music
{
    #region Structs
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

    public struct BeatGridEvent
    {
        public int barIndex;     // 0-based bar
        public int beatInBar;    // 0..(numerator-1)
        public float time;       // sec since song start
    }
    #endregion

    #region Interfaces
    public interface IMidiNoteListener { void OnMidiNote(MidiTaggedEvent e); }
    public interface IChordListener { void OnChord(ChordEvent e); }
    public interface IBeatGridListener
    {
        void OnBeat(BeatGridEvent e);       // fires every beat
        void OnDownbeat(BeatGridEvent e);   // fires on beatInBar == 0
    }
    // Specific drum elements — start with kick; easy to extend
    public interface IDrumKickListener
    {
        void OnDrumKick(MidiTaggedEvent e);
    }
    public interface ITempoSignatureListener
    {
        void OnTempoChanged(double bpm);
        void OnTimeSignatureChanged(int numerator, int denominator);
    }
    #endregion
}

using UnityEngine;

namespace ALWTTT.Music
{
    [RequireComponent(typeof(Characters.Band.MusicianBase))]
    public class MusicianMidiResponder : 
        MonoBehaviour, 
        IMidiNoteListener, 
        IChordListener,
        IBeatGridListener, // global beat grid (no per-musician ID)
        IDrumKickListener, // kick-based beats (with musician routing)
        ITempoSignatureListener // BPM & TS changes
    {
        [SerializeField] private Characters.Band.MusicianBase musician;

        [Header("React To")]
        public bool reactToNotes = true;
        public bool reactToChords = true;
        public bool reactToBeats = true;

        public void Init(Characters.Band.MusicianBase m) { musician = m; }

        void Awake()
        {
            if (!musician) musician = GetComponent<Characters.Band.MusicianBase>();
        }

        void OnEnable()
        {
            var mm = Managers.MidiMusicManager.Instance;
            if (!mm) return;
            mm.Register((IMidiNoteListener)this);
            mm.Register((IChordListener)this);
            mm.Register((IBeatGridListener)this);
            mm.Register((IDrumKickListener)this);
            mm.Register((ITempoSignatureListener)this);
        }

        void OnDisable()
        {
            var mm = Managers.MidiMusicManager.Instance;
            if (!mm) return;
            mm.Unregister((IMidiNoteListener)this);
            mm.Unregister((IChordListener)this);
            mm.Unregister((IBeatGridListener)this);
            mm.Unregister((IDrumKickListener)this);
            mm.Unregister((ITempoSignatureListener)this);
        }

        bool IsMine(string id) =>
            !string.IsNullOrEmpty(id) &&
            id == musician.MusicianCharacterData.CharacterId;

        public void OnMidiNote(MidiTaggedEvent e)
        {
            if (!reactToNotes || !IsMine(e.musicianId)) return;
            musician.TriggerNoteVFX(e.note, e.velocity);
        }

        public void OnChord(ChordEvent e)
        {
            if (!reactToChords || !IsMine(e.musicianId)) return;
            musician.TriggerChordVFX(e.notes);
        }

        public void OnBeat(BeatGridEvent e)
        {
            
        }

        public void OnDownbeat(BeatGridEvent e)
        {
            if (!reactToBeats) return;
            // Light “bar pulse” on every musician to show the global grid
            musician.TriggerBeatVFX(0);
        }

        public void OnDrumKick(MidiTaggedEvent e)
        {
            if (!reactToBeats || !IsMine(e.musicianId)) return;
            musician.TriggerBeatVFX(0);
        }

        public void OnTempoChanged(double bpm)
        {
            if (!reactToBeats) return;
            Debug.Log($"<color=cyan>[Responder]</color> {musician.MusicianCharacterData.CharacterName} tempo={bpm:0.0} BPM");
        }

        public void OnTimeSignatureChanged(int numerator, int denominator)
        {
            if (!reactToBeats) return;
            Debug.Log($"<color=cyan>[Responder]</color> {musician.MusicianCharacterData.CharacterName} TS={numerator}/{denominator}");
        }
    }
}
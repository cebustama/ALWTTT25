
using UnityEngine;

namespace ALWTTT.Music
{
    [RequireComponent(typeof(Characters.Band.MusicianBase))]
    public class MusicianMidiResponder : MonoBehaviour, IMidiNoteListener, IChordListener, IBeatSyncVFX
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
            mm.Register((IBeatSyncVFX)this);
        }

        void OnDisable()
        {
            var mm = Managers.MidiMusicManager.Instance;
            if (!mm) return;
            mm.Unregister((IMidiNoteListener)this);
            mm.Unregister((IChordListener)this);
            mm.Unregister((IBeatSyncVFX)this);
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

        public void OnBeat(BeatEvent e)
        {
            if (!reactToBeats || !IsMine(e.sourceMusicianId)) return;
            musician.TriggerBeatVFX(e.beatIndex);
        }
    }
}
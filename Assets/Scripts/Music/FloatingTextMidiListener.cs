using ALWTTT.Managers;
using ALWTTT.Music;
using UnityEngine;

public class FloatingTextMidiListener : 
    MonoBehaviour, IMidiNoteListener, IChordListener, IBeatSyncVFX
{
    [SerializeField] private bool showNotes = true;
    [SerializeField] private bool showChords = true;
    [SerializeField] private bool showBeats = true;

    void OnEnable()
    {
        var mm = FindFirstObjectByType<MidiMusicManager>();
        if (mm == null) return;
        mm.Register((IMidiNoteListener)this);
        mm.Register((IChordListener)this);
        mm.Register((IBeatSyncVFX)this);
    }

    void OnDisable()
    {
        var mm = FindFirstObjectByType<MidiMusicManager>();
        if (mm == null) return;
        mm.Unregister((IMidiNoteListener)this);
        mm.Unregister((IChordListener)this);
        mm.Unregister((IBeatSyncVFX)this);
    }

    public void OnMidiNote(MidiTaggedEvent e)
    {
        if (!showNotes || e.anchor == null) return;
        FxManager.Instance?.SpawnFloatingText(e.anchor, NoteName(e.note), 0, 1);
    }

    public void OnChord(ChordEvent e)
    {
        if (!showChords || e.anchor == null) return;
        var label = $"Chord ({string.Join(",", e.notes)})";
        FxManager.Instance?.SpawnFloatingText(e.anchor, label, 0, 1);
    }

    public void OnBeat(BeatEvent e)
    {
        if (!showBeats || e.anchor == null) return;
        FxManager.Instance?.SpawnFloatingText(e.anchor, $"• {e.beatIndex}", 0, 1);
    }

    private static string NoteName(int midi)
    {
        string[] n = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int oct = (midi / 12) - 1;
        return $"{n[midi % 12]}{oct}";
    }
}

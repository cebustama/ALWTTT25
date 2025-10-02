using ALWTTT.Managers;
using ALWTTT.Music;
using Melanchall.DryWetMidi.Standards;
using UnityEngine;

public class FloatingTextMidiListener :
    MonoBehaviour, 
    IMidiNoteListener, 
    IChordListener,
    IBeatGridListener, 
    IDrumKickListener,
    ITempoSignatureListener,
    IPartInfoListener
{
    [SerializeField] private bool showNotes = true;
    [SerializeField] private bool showChords = true;
    [SerializeField] private bool showBeats = true;
    [SerializeField] private bool labelPercussionByName = true; // "Kick", "Snare", ...

    [Header("Colors - Notes (12 pitch classes)")]
    [SerializeField]
    private Color[] pitchClassColors = new Color[12]
    {
        new Color(0.95f,0.30f,0.30f), // C
        new Color(1.00f,0.55f,0.25f), // C#
        new Color(1.00f,0.80f,0.25f), // D
        new Color(0.90f,0.90f,0.30f), // D#
        new Color(0.60f,0.95f,0.30f), // E
        new Color(0.25f,0.95f,0.50f), // F
        new Color(0.25f,0.85f,0.95f), // F#
        new Color(0.30f,0.60f,0.95f), // G
        new Color(0.55f,0.35f,0.95f), // G#
        new Color(0.85f,0.35f,0.95f), // A
        new Color(0.95f,0.35f,0.70f), // A#
        new Color(0.95f,0.45f,0.45f), // B
    };

    [Header("Colors - Chords & Beat")]
    [SerializeField] private Color chordColor = new Color(0.95f, 0.25f, 0.85f);
    [SerializeField] private Color beatColor = new Color(1.00f, 0.95f, 0.25f);

    [Header("Colors - Percussion (GM ch10)")]
    [SerializeField] private Color kickColor = new Color(1.00f, 0.55f, 0.20f);
    [SerializeField] private Color snareColor = new Color(1.00f, 0.25f, 0.25f);
    [SerializeField] private Color hhClosedColor = new Color(0.75f, 0.85f, 0.90f);
    [SerializeField] private Color hhOpenColor = new Color(0.55f, 0.75f, 0.90f);
    [SerializeField] private Color tomColor = new Color(0.40f, 0.70f, 1.00f);
    [SerializeField] private Color cymbalColor = new Color(1.00f, 0.85f, 0.40f);
    [SerializeField] private Color otherPercColor = new Color(0.85f, 0.85f, 0.85f);

    [Header("Directions")]
    [SerializeField] private Vector2 drumKickDir = new(-1f, 0.1f); // left
    [SerializeField] private Vector2 snareDir = new(1f, 0.0f); // right
    [SerializeField] private Vector2 hhClosedDir = new(0f, 1.0f); // up
    [SerializeField] private Vector2 hhOpenDir = new(0f, 1.0f); // up
    [SerializeField] private Vector2 tomDir = new(0.5f, 0.6f);
    [SerializeField] private Vector2 cymbalDir = new(0.0f, 0.8f);
    [SerializeField] private Vector2 noteDir = new(0.2f, 0.8f);
    [SerializeField] private Vector2 chordDir = new(0.0f, 1.0f);
    [SerializeField] private Vector2 beatDir = new(0.0f, 1.0f);
    [SerializeField] private Vector2 downbeatDir = new(0.0f, 1.2f);

    [Header("Tempo / Signature Display")]
    [SerializeField] private bool showTempoSignature = true;
    [SerializeField] private Transform hudAnchor; // (optional anchor)
    [SerializeField] private Color tempoColor = new(0.25f, 0.9f, 0.95f);
    [SerializeField] private Color tsColor = new(0.95f, 0.95f, 0.95f);
    [SerializeField] private Vector2 tempoDir = new(0f, 1.2f);
    [SerializeField] private Vector2 tsDir = new(0f, 1.0f);

    // Treat channel 9 as drums (GM channel 10), keep in sync with MidiMusicManager
    private const int DrumChannel = 9;

    void OnEnable()
    {
        var mm = FindFirstObjectByType<MidiMusicManager>();
        if (mm == null) return;
        mm.Register((IMidiNoteListener)this);
        mm.Register((IChordListener)this);
        mm.Register((IBeatGridListener)this);
        mm.Register((IDrumKickListener)this);
        mm.Register((ITempoSignatureListener)this);
        mm.Register((IPartInfoListener)this);
    }

    void OnDisable()
    {
        var mm = FindFirstObjectByType<MidiMusicManager>();
        if (mm == null) return;
        mm.Unregister((IMidiNoteListener)this);
        mm.Unregister((IChordListener)this);
        mm.Unregister((IBeatGridListener)this);
        mm.Unregister((IDrumKickListener)this);
        mm.Unregister((ITempoSignatureListener)this);
        mm.Unregister((IPartInfoListener)this);
    }

    #region Callbacks
    public void OnMidiNote(MidiTaggedEvent e)
    {
        if (!showNotes || e.anchor == null) return;
        var (label, color) = BuildNoteLabelAndColor(e);

        // If drum, choose direction by element
        if (e.channel == DrumChannel)
        {
            var gm = (GeneralMidiPercussion)Mathf.Clamp(e.note, 35, 81);
            var dir = gm switch
            {
                GeneralMidiPercussion.AcousticBassDrum or GeneralMidiPercussion.BassDrum1 => drumKickDir,
                GeneralMidiPercussion.AcousticSnare or GeneralMidiPercussion.ElectricSnare => snareDir,
                GeneralMidiPercussion.ClosedHiHat or GeneralMidiPercussion.PedalHiHat => hhClosedDir,
                GeneralMidiPercussion.OpenHiHat => hhOpenDir,
                GeneralMidiPercussion.LowTom or GeneralMidiPercussion.LowFloorTom or
                GeneralMidiPercussion.LowMidTom or GeneralMidiPercussion.HiMidTom or
                GeneralMidiPercussion.HighTom or GeneralMidiPercussion.HighFloorTom => tomDir,
                GeneralMidiPercussion.CrashCymbal1 or GeneralMidiPercussion.CrashCymbal2 or
                GeneralMidiPercussion.RideCymbal1 or GeneralMidiPercussion.RideCymbal2 or
                GeneralMidiPercussion.SplashCymbal or GeneralMidiPercussion.ChineseCymbal => cymbalDir,
                _ => noteDir
            };
            FxManager.Instance?.SpawnFloatingText(e.anchor, label, dir, color);
            return;
        }

        // Melodic
        // TODO: Only if lead (melody or harmony)
        //FxManager.Instance?.SpawnFloatingText(e.anchor, label, noteDir, color);
    }

    public void OnChord(ChordEvent e)
    {
        // ignore if hidden, no anchor, or this is the drum channel
        if (!showChords || e.anchor == null || e.channel == DrumChannel) return;

        // Prefer generator/manager-provided chord labels (e.g., "Cm7 (ii)")
        string label = null;
        if (!string.IsNullOrEmpty(e.symbol) && !string.IsNullOrEmpty(e.roman))
            label = $"{e.symbol} ({e.roman})";
        else if (!string.IsNullOrEmpty(e.symbol))
            label = e.symbol;
        else if (!string.IsNullOrEmpty(e.roman))
            label = $"({e.roman})";
        else
            label = $"Chord ({string.Join(",", e.notes)})"; // fallback

        FxManager.Instance?.SpawnFloatingText(e.anchor, label, chordDir, chordColor);

        // Helpful console trace while you’re tuning things:
        Debug.Log($"[FT] {label}  ch={e.channel}  mus='{e.musicianId}'");
    }

    public void OnBeat(BeatGridEvent e)
    {
        if (!showBeats) return;
        //FxManager.Instance?.SpawnFloatingText(transform, "Beat", downbeatDir, beatColor);
    }

    public void OnDownbeat(BeatGridEvent e)
    {
        if (!showBeats) return;
        // Example: emphasize downbeat somewhere (e.g., bigger dot).
        //FxManager.Instance?.SpawnFloatingText(transform, "Downbeat", downbeatDir, beatColor);
    }

    // Specific drum kick hook
    public void OnDrumKick(MidiTaggedEvent e)
    {
        if (e.anchor == null) return;
        FxManager.Instance?.SpawnFloatingText(e.anchor, "Kick", drumKickDir, kickColor);
    }

    public void OnTempoChanged(double bpm)
    {
        if (!showTempoSignature) return;
        var tr = hudAnchor != null ? hudAnchor : transform;
        FxManager.Instance?.SpawnFloatingText(tr, $"{bpm:0.#} BPM", tempoDir, tempoColor);
    }

    public void OnTimeSignatureChanged(int numerator, int denominator)
    {
        if (!showTempoSignature) return;
        var tr = hudAnchor != null ? hudAnchor : transform;
        FxManager.Instance?.SpawnFloatingText(tr, $"{numerator}/{denominator}", tsDir, tsColor);
    }

    public void OnPartStarted(PartInfoEvent e)
    {
        
    }
    #endregion

    #region Helpers

    // -- colors --
    private (string label, Color color) BuildNoteLabelAndColor(MidiTaggedEvent e)
    {
        // Drums → GM percussion name + color by instrument category
        if (e.channel == DrumChannel)
        {
            var gm = (GeneralMidiPercussion)Mathf.Clamp(e.note, 35, 81);
            string text = labelPercussionByName ? PercLabel(gm) : $"D{(int)gm}";
            return (text, ColorForPercussion(gm));
        }

        // Melodic → Pitch class color and "C4" style label
        var label = NoteName(e.note);
        var color = pitchClassColors[Mathf.Abs(e.note) % 12];
        return (label, color);
    }

    private Color ColorForPercussion(GeneralMidiPercussion gm)
    {
        switch (gm)
        {
            case GeneralMidiPercussion.AcousticBassDrum:
            case GeneralMidiPercussion.BassDrum1:
                return kickColor;

            case GeneralMidiPercussion.AcousticSnare:
            case GeneralMidiPercussion.ElectricSnare:
                return snareColor;

            case GeneralMidiPercussion.ClosedHiHat:
            case GeneralMidiPercussion.PedalHiHat:
                return hhClosedColor;

            case GeneralMidiPercussion.OpenHiHat:
                return hhOpenColor;

            case GeneralMidiPercussion.LowTom:
            case GeneralMidiPercussion.LowFloorTom:
            case GeneralMidiPercussion.LowMidTom:
            case GeneralMidiPercussion.HiMidTom:
            case GeneralMidiPercussion.HighTom:
            case GeneralMidiPercussion.HighFloorTom:
                return tomColor;

            case GeneralMidiPercussion.CrashCymbal1:
            case GeneralMidiPercussion.CrashCymbal2:
            case GeneralMidiPercussion.RideCymbal1:
            case GeneralMidiPercussion.RideCymbal2:
            case GeneralMidiPercussion.SplashCymbal:
            case GeneralMidiPercussion.ChineseCymbal:
                return cymbalColor;

            default:
                return otherPercColor;
        }
    }

    // -- labels --
    private static string NoteName(int midi)
    {
        string[] n = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int oct = (midi / 12) - 1;
        return $"{n[midi % 12]}{oct}";
    }

    private static string PercLabel(GeneralMidiPercussion gm)
    {
        switch (gm)
        {
            case GeneralMidiPercussion.AcousticBassDrum:
            case GeneralMidiPercussion.BassDrum1: return "Kick";
            case GeneralMidiPercussion.AcousticSnare:
            case GeneralMidiPercussion.ElectricSnare: return "Snare";
            case GeneralMidiPercussion.ClosedHiHat:
            case GeneralMidiPercussion.PedalHiHat: return "HH cl";
            case GeneralMidiPercussion.OpenHiHat: return "HH op";
            case GeneralMidiPercussion.CrashCymbal1:
            case GeneralMidiPercussion.CrashCymbal2:
            case GeneralMidiPercussion.RideCymbal1:
            case GeneralMidiPercussion.RideCymbal2:
            case GeneralMidiPercussion.SplashCymbal:
            case GeneralMidiPercussion.ChineseCymbal: return "Cymb";
            case GeneralMidiPercussion.LowTom:
            case GeneralMidiPercussion.LowFloorTom:
            case GeneralMidiPercussion.LowMidTom:
            case GeneralMidiPercussion.HiMidTom:
            case GeneralMidiPercussion.HighTom:
            case GeneralMidiPercussion.HighFloorTom: return "Tom";
            default: return "Perc";
        }
    }
    #endregion
}

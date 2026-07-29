using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MidiGenPlay;
using MidiGenPlay.Composition;
using UnityEngine;
using Vocal;
using static MidiGenPlay.MusicTheory.MusicTheory;

// Pink Trombone POC � Session 5, v7.
//
// Flow (D-S3-B=B): SmokeSetupSO -> SmokeSongConfigAssembler.Assemble ->
// Orchestrator.GenerateSinglePart (honors bpm + seed) -> select the melody
// chunk BY `mus:` TAG (D-POC-4, never a hard-coded channel) -> tempo-map to
// seconds -> sing once.
//
// ---- v5: Session 4 batches ------------------------------------------------
// D-S4-A(C) HALF-RATE SYNTHESIS, toggle-gated, DEFAULT OFF. The fork's
//           constructor already takes a sample rate; v5 can build the voice at
//           outputRate/2 (24 kHz on a 48 kHz device) and linearly upsample in
//           this file's callback. POC-side only � NO fork edit. Expected to
//           roughly halve model cost. Nyquist at 24 kHz is 12 kHz, far above
//           vocal-tract content; the 3.5 kHz tone stage sits downstream and
//           absorbs interpolation imaging. REQUIRES A LISTENING A/B against
//           full rate before any number from it enters the verdict.
//           The voice is rebuilt when the toggle changes, at the next arm
//           (Render && Sing / Test Scale / Sustain) � never mid-playback.
// D-S3-H    C-LITE EXPRESSIVITY, master toggle, DEFAULT OFF (Session 3
//           results stay reproducible). Three independent mappings, each with
//           its own toggle + amount, ALL precomputed per note on the main
//           thread from data the POC already holds (partContext.tonality +
//           rootNote + timeSignature + bpm + the MIDI notes). The audio
//           callback stays arithmetic-free: it only reads precomputed
//           per-note targets.
//             M1 vowel openness  <- metric weight + note duration
//                (Neutral -> Open morph on stressed/long notes; the model's
//                own glide, POC-FORK 2, does the morphing)
//             M2 vowel frontness <- contour direction
//                (ascending shifts tongue index forward, descending back)
//             M3 tenseness bias  <- scale-degree tension
//                (leading tone / subdominant / chromatic tense; tonic /
//                dominant relaxed; CLAMPED inside [tensenessAtVel0,
//                tensenessAtVel127] � the Test 6 window � and SKIPPED when
//                tensenessOverride is on, so Test 6 isolation still works)
//           NOT BUILT, BY DESIGN (�5): phrase boundaries, cadence, tension
//           arc, climax. That data lives in PhrasePlanner internal state and
//           never reaches the MidiFile. The gap IS the Phase D4 signal �
//           recorded as the PerformanceSlotInfo field list in deliverable 4.
// Defaults  tensenessAtVel0/127 updated to the settled Test 6 window
//           0.40/0.60 (was 0.45/0.68). NOTE: serialized scene values
//           override these � update the component in the scene too.
//
// ---- v6: Session 5 (pitch stability) --------------------------------------
// POC-FORK(6/7) JITTER GATES. Session 5 read the fork's F0 path for the
//           first time and found the wobble: Glottis.CalculateVibrato adds two
//           simplex jitter terms (0.02*S(4.07t) + 0.04*S(2.15t)) OUTSIDE both
//           the VibratoWobble gate and VibratoGain -- an always-on, previously
//           unreachable F0 wander measured at 34.7 cents SD / 150 cents p2p on
//           an 8 s sustain. (This is why vibratoGain=0 only partially helped:
//           it removed just the +/-8.6-cent periodic sine.) Tenseness has the
//           same pattern (+/-0.11 drift, over half the 0.40-0.60 window).
//           The fork now gates both: PitchJitterGain / TensenessJitterGain,
//           0..1, default 1 = bit-identical upstream parity (verified).
//           g=0 measures 0.01 cents SD. Sliders below are LIVE per block:
//           A/B on a Sustain Test without re-arming. Defaults 1, so the first
//           run after this upgrade sounds exactly like v5.
//
// ---- v7: Session 5, part 2 (voice character levers + field tooltips) ------
// VOICE CHARACTER  Six curated macro levers behind a master toggle
//           (characterEnabled, DEFAULT OFF = exact v6 behavior; every raw
//           field keeps working for diagnostics). ON = the levers DRIVE
//           their underlying fields and those raw fields are ignored:
//             looseness      -> both jitter gates (POC-FORK 6/7)
//             vibratoDepth   -> vibratoGain (x0.012), delay/ramp still gate
//             vibratoSpeedHz -> vibratoFrequency
//             diction        -> scales M1+M2+M3 amounts together (arm time)
//             mouth          -> base vowel preset; M1 morphs FROM this base
//                               toward Open (v6 always morphed from Neutral;
//                               with the toggle OFF that old path is intact)
//             brightness     -> tone cutoff on a log curve (1.2k..~10k Hz)
//           All levers live per block except diction/mouth's effect on
//           per-note targets (sampled at arm, like the C-lite amounts).
//           The curated lever catalog + character recipes + the draft
//           ALWTTT VoiceProfile schema live in PinkTrombone_Voice_Levers.md.
//
// ---- retained from v4 ------------------------------------------------------
// Per-block vowel/tenseness push (live sweeps), tenseness override + presets
// (Test 6), vowel presets (Test 7), loop playback, synthetic scale fixture
// (D-S3-G=C � tunes the INSTRUMENT, not verdict material), interval-scaled
// pitch lead (D-S3-F=B), internal IsTouched articulation (Test 4 winner),
// velocity -> Loudness AND tenseness (D-POC-5=B), monophonic last-note
// (D-POC-6), tone stage (disable for CPU tests), RenderReady event so backing
// attaches WITHOUT this file knowing MPTK.
//
// Audio thread: zero allocations, no locks, no Unity API calls. Voice and
// buffer are captured as locals at callback entry; rebuilds happen only while
// stopped. All 0..1 setters clamped BEFORE assignment.
//
// BOUNDARY: dev-time POC consuming MidiGenPlay.Runtime (D-POC-1=B). No
// runtime or SSoT change across four sessions.
[RequireComponent(typeof(AudioSource))]
public class PinkTromboneSinger : MonoBehaviour
{
    [Header("Song source (D-S3-B=B: governed smoke infra)")]
    [SerializeField] private SmokeSetupSO setup;

    [Header("D-S4-A � self-measured callback load (works in ANY build)")]
    [Tooltip("Time inside OnAudioFilterRead / duration of audio produced. " +
         "Isolates the singer. NOT comparable to Profiler audio CPU %.")]
    [SerializeField] private bool measureCallbackLoad = false;

    [Header("D-S4-A(C) � half-rate synthesis (CPU). LISTEN before trusting.")]
    [Tooltip("Build the vocal tract at HALF the device rate and upsample " +
             "here. ~halves model cost. Applied at the next Render/Test " +
             "press, never mid-playback. Run a listening A/B vs full rate.")]
    [SerializeField] private bool halfRateSynthesis = false;

    [Header("Intonation (D-S3-C)")]
    [Tooltip("REGISTER. Semitones added to the written melody. -12 (settled) " +
             "= sing an octave down; the tract reads as an adult voice and " +
             "strains when pushed high. With mouth/vowel, the main " +
             "'who is singing' control.")]
    [SerializeField, Range(-24, 12)] private int transposeSemitones = -12;
    [Tooltip("Portamento compensation: retargets F0 slightly BEFORE note-on " +
             "so the model's glide lands on time. 0.06 s at a fifth " +
             "(settled). 0 = audible scooping into large intervals; too " +
             "high = notes bloom early.")]
    [SerializeField, Range(0f, 0.3f)] private float pitchLeadSeconds = 0.06f;
    [Tooltip("Interval (semitones) at which the FULL lead is applied.")]
    [SerializeField, Range(1, 12)] private int leadFullInterval = 7;

    [Header("Articulation (D-S2-2)")]
    [Tooltip("ON = voicing persists through rests (hums through gaps). OFF " +
             "(settled) = the model's internal IsTouched envelope articulates " +
             "clean note onsets/offsets while breath noise persists.")]
    [SerializeField] private bool alwaysVoice = false;
    [Tooltip("Test 4 found this WORSE � clean silences expose the glide " +
             "arriving mid-note on fast passages. Left in for the record.")]
    [SerializeField] private bool hardGateOutput = false;
    [Tooltip("External gate attack. ONLY audible when Hard Gate Output is ON.")]
    [SerializeField, Range(0.5f, 500f)] private float attackMs = 10f;
    [Tooltip("External gate release. ONLY audible when Hard Gate Output is ON.")]
    [SerializeField, Range(5f, 1000f)] private float releaseMs = 60f;
    [Tooltip("Re-zero the external gate at each note-on. Only matters with " +
             "Hard Gate Output ON.")]
    [SerializeField] private bool retriggerOnNoteOn = true;

    [Header("Vowel base � live per block WHEN expressivity is OFF (Test 7)")]
    [Tooltip("Tongue constriction POSITION along the tract, 0 (throat) to 44 " +
             "(lips). This IS the vowel: ~13 neutral 'uh', ~14 open 'ah', " +
             "~27 front 'eh/ee', ~18 + big diameter back 'oh'. The presets " +
             "in the runtime panel map these.")]
    [SerializeField, Range(0f, 44f)] private float tongueIndex = 12.9f;
    [Tooltip("HIGHER = more open tube = less pinched/shrill. Try 2.8-3.2.")]
    [SerializeField, Range(0f, 3.5f)] private float tongueDiameter = 2.9f;
    [Tooltip("Upstream's huge 'drunk' F0 wander terms (up to +/-60%!). Keep " +
             "OFF forever; use Pitch Jitter Gain / looseness for CONTROLLED " +
             "instability instead.")]
    [SerializeField] private bool vibratoWobble = false;

    [Header("POC-FORK(6/7) jitter gates (Session 5). 1 = upstream wander")]
    [Tooltip("Scales the model's always-on simplex F0 jitter. 1 = upstream " +
             "(+/-~90 cents peak, ~35 cents SD -- THE wobble). 0 = stable " +
             "pitch (measured 0.01 cents SD). Live per block; A/B it on a " +
             "Sustain Test. Try 0.1-0.2 for a touch of life.")]
    [SerializeField, Range(0f, 1f)] private float pitchJitterGain = 1f;
    [Tooltip("Scales the model's always-on simplex tenseness drift. 1 = " +
             "upstream (+/-~0.11 -- over half the 0.40-0.60 window, wanders " +
             "timbre and internal loudness). 0 = steady. Live per block.")]
    [SerializeField, Range(0f, 1f)] private float tensenessJitterGain = 1f;

    [Header("VOICE CHARACTER � curated macro levers (Session 5, v7)")]
    [Tooltip("ON = the six levers below DRIVE their underlying fields " +
             "(both jitter gates, vibrato gain+rate, the three C-lite " +
             "amounts, base vowel, tone cutoff); those raw fields are " +
             "ignored while ON. OFF = exact v6 behavior. Levers are live " +
             "per block, except diction and mouth's effect on per-note " +
             "targets (sampled when a fixture is armed).")]
    [SerializeField] private bool characterEnabled = false;

    [Tooltip("Pitch + timbre instability (drives BOTH jitter gates). " +
             "0 = studio-stable (0.01 cents SD), 0.15 = human 'life' " +
             "(~5 cents SD), 1 = upstream drunk wander (~35 cents SD, " +
             "150 cents peak-to-peak). Measured values, Session 5.")]
    [SerializeField, Range(0f, 1f)] private float looseness = 0.15f;

    [Tooltip("Vibrato depth on held notes. 0 = none, 0.4 = settled subtle " +
             "(+/-~8 cents), 1 = pronounced/operatic (+/-~21 cents). The " +
             "delay/ramp fields below still gate it to LONG notes only.")]
    [SerializeField, Range(0f, 1f)] private float vibratoDepth = 0.4f;

    [Tooltip("Vibrato rate. 5-6.5 Hz reads as singing; below ~4.5 croons " +
             "(seasick at depth); above ~7 nervous bleat.")]
    [SerializeField, Range(3f, 9f)] private float vibratoSpeedHz = 6f;

    [Tooltip("Scales ALL THREE C-lite mappings together (M1 openness, M2 " +
             "contour color, M3 degree tension). 0 = static mouth / mumble, " +
             "1 = settled musical diction, 2 = theatrical over-enunciation. " +
             "Sampled at arm time.")]
    [SerializeField, Range(0f, 2f)] private float diction = 1f;

    [Tooltip("Base vowel color: 0 Neutral 'uh', 1 Open 'ah', 2 Front " +
             "'eh/ee', 3 Back 'oh'. M1 opens from THIS base toward 'ah' " +
             "on stressed/long notes. THE 'character of the voice' pick.")]
    [SerializeField, Range(0, 3)] private int mouth = 0;

    [Tooltip("Tone-stage cutoff on a log curve: 0 = 1.2 kHz muffled/warm/" +
             "distant, 0.5 = ~3.5 kHz settled, 1 = ~10 kHz present but " +
             "increasingly shrill.")]
    [SerializeField, Range(0f, 1f)] private float brightness = 0.5f;

    [Header("D-S3-H C-lite expressivity (per-note, precomputed at arm time)")]
    [Tooltip("Master switch. OFF = exact v4 behavior (Session 3 baseline). " +
             "ON = per-note vowel/tenseness targets rule; the vowel preset " +
             "buttons and inspector vowel sweeps have NO effect while a " +
             "song is playing. Sub-mapping toggles/amounts are read when a " +
             "fixture is ARMED; the master switch itself is live.")]
    [SerializeField] private bool expressivityEnabled = false;

    [Tooltip("M1: stressed/long notes morph Neutral -> Open. 0 = off.")]
    [SerializeField] private bool vowelOpennessEnabled = true;
    [Tooltip("How far stressed/long notes open toward 'ah'. THE single " +
             "biggest audible expressivity contributor: 0 = static mumble, " +
             "1 = fully open vowels on downbeats and held notes.")]
    [SerializeField, Range(0f, 1f)] private float vowelOpenAmount = 0.7f;
    [Tooltip("Note duration (seconds) that counts as fully 'long' for M1.")]
    [SerializeField, Range(0.2f, 3f)] private float longNoteSeconds = 0.8f;

    [Tooltip("M2: ascending lines shift tongue index forward (brighter), " +
             "descending back. Subtle by design.")]
    [SerializeField] private bool vowelContourEnabled = true;
    [Tooltip("Tongue-index shift per melodic DIRECTION (sign only, not " +
             "interval size). Audible as brighter color on rising lines, " +
             "darker on falling. 0 = off; above ~4 = caricature.")]
    [SerializeField, Range(0f, 6f)] private float vowelContourIdxShift = 2f;

    [Tooltip("M3: scale-degree tension biases tenseness INSIDE the Test 6 " +
             "window. Leading tone/chromatic = tense, tonic/dominant = " +
             "relaxed. Skipped when tensenessOverride is on.")]
    [SerializeField] private bool tensenessDegreeEnabled = true;
    [Tooltip("Bias size. At 0.08: leading tone / chromatic notes press " +
             "toward the window TOP (harder, buzzier), tonic sinks to the " +
             "BOTTOM (breathier, settled). Clamped inside the velocity " +
             "window, so it colors phrases without shouting.")]
    [SerializeField, Range(0f, 0.2f)] private float tensenessDegreeAmount = 0.08f;

    [Header("Vibrato � delayed, so short notes get none")]
    [Tooltip("Set to 0 for the wobble diagnostic: if a Sustain Test note is " +
             "then rock steady, all residual motion is the portamento glide.")]
    [SerializeField, Range(0f, 0.1f)] private float vibratoGain = 0.005f;
    [Tooltip("Rate. 5-6.5 Hz sings; lower croons; higher bleats.")]
    [SerializeField, Range(3f, 9f)] private float vibratoFrequency = 6f;
    [Tooltip("Hold time before vibrato starts � short notes get none, " +
             "like a real singer.")]
    [SerializeField, Range(0f, 2f)] private float vibratoDelaySeconds = 0.35f;
    [Tooltip("Fade-in from delay to full depth, so vibrato never " +
             "'switches on'.")]
    [SerializeField, Range(0.01f, 2f)] private float vibratoRampSeconds = 0.4f;

    [Header("Velocity mapping (D-POC-5=B). 0.40-0.60 = the Test 6 window.")]
    [Tooltip("Amplitude floor at velocity 0, so quiet notes stay present. " +
             "1 = velocity ignored.")]
    [SerializeField, Range(0f, 1f)] private float minLoudness = 0.15f;
    [Tooltip("Window BOTTOM (velocity 0). Lower = breathier, airier, " +
             "weaker fundamental.")]
    [SerializeField, Range(0f, 1f)] private float tensenessAtVel0 = 0.40f;
    [Tooltip("Window TOP (velocity 127). Higher = harder, buzzier, more " +
             "pitch definition; above ~0.8 turns harsh.")]
    [SerializeField, Range(0f, 1f)] private float tensenessAtVel127 = 0.60f;

    [Header("Test 6 � live tenseness override (isolates the variable)")]
    [Tooltip("ON = ignore velocity mapping AND the M3 degree bias, hold one " +
             "tenseness. Applied per block.")]
    [SerializeField] private bool tensenessOverride = false;
    [SerializeField, Range(0f, 1f)] private float tensenessOverrideValue = 0.6f;

    [Header("Tone stage � the missing mouth + room (DISABLE for CPU tests)")]
    [Tooltip("Consumer-side stand-in for lip radiation + room: a 2-pole " +
             "low-pass. OFF = raw tract (shrill, fizzy); only for CPU " +
             "measurements.")]
    [SerializeField] private bool toneEnabled = true;
    [Tooltip("Brightness. 3500 settled; toward 1500 = warm/muffled/far; " +
             "toward 8k+ = present but increasingly shrill.")]
    [SerializeField, Range(500f, 12000f)] private float toneCutoffHz = 3500f;

    [Header("Test-scale fixture (D-S3-G=C � tunes the voice, NOT the verdict)")]
    [SerializeField, Range(36, 72)] private int testScaleRootMidi = 60;
    [SerializeField, Range(1, 2)] private int testScaleOctaves = 1;
    [SerializeField, Range(60f, 1000f)] private float testNoteMs = 250f;
    [Tooltip("0 = legato (worst case for pitch lead); 60-100 = detached.")]
    [SerializeField, Range(0f, 200f)] private float testGapMs = 0f;
    [SerializeField, Range(0.5f, 8f)] private float testHoldSeconds = 3f;
    [SerializeField, Range(36, 84)] private int testNoteNumber = 60;

    [Header("Output")]
    [SerializeField, Range(0f, 1f)] private float gain = 0.5f;
    [Tooltip("Repeat the fixture so parameters can be swept without " +
             "restarting. Does NOT loop the MPTK backing.")]
    [SerializeField] private bool loopPlayback = false;
    [Tooltip("Delays the sung line. Positive = sing later; trims against " +
             "MPTK backing start latency.")]
    [SerializeField, Range(-500f, 500f)] private float syncTrimMs = 0f;
    [SerializeField, Range(1f, 4f)] private float uiScale = 2f;

    // Test 7 vowel presets: index / diameter. [0] and [1] are also the M1
    // morph endpoints (Neutral -> Open).
    private static readonly (string name, float idx, float dia)[] VowelPresets =
    {
        ("Neutral", 12.9f, 2.43f),
        ("Open",    14.0f, 2.90f),
        ("Front",   27.0f, 2.10f),
        ("Back",    18.0f, 3.20f),
    };

    private static readonly float[] TensenessPresets = { 0.40f, 0.60f, 0.80f, 0.95f };

    // ---- v7: effective values � the character levers, when enabled, drive
    // the underlying parameters; otherwise the raw fields rule (exact v6).
    // Called from the audio thread: fields + math only, no Unity API.
    private float EffPitchJitter =>
        Mathf.Clamp01(characterEnabled ? looseness : pitchJitterGain);
    private float EffTensenessJitter =>
        Mathf.Clamp01(characterEnabled ? looseness : tensenessJitterGain);
    private float EffVibratoGain =>
        characterEnabled ? vibratoDepth * 0.012f : vibratoGain;
    private float EffVibratoFrequency =>
        characterEnabled ? vibratoSpeedHz : vibratoFrequency;
    private float EffToneCutoffHz =>
        characterEnabled ? 1200f * Mathf.Pow(2f, 3.1f * brightness) : toneCutoffHz;
    private (float idx, float dia) EffBaseVowel()
    {
        if (!characterEnabled)
            return (Mathf.Clamp(tongueIndex, 0f, 44f),
                    Mathf.Clamp(tongueDiameter, 0f, 3.5f));
        var p = VowelPresets[Mathf.Clamp(mouth, 0, VowelPresets.Length - 1)];
        return (p.idx, p.dia);
    }

    private struct NoteEvt
    {
        public double start;   // seconds
        public double end;     // seconds
        public double lead;    // seconds of pitch pre-arm (D-S3-F=B)
        public int note;       // MIDI note number (pre-transpose)
        public int vel;        // 0..127
        // C-lite (precomputed on the main thread; audio thread only reads):
        public float vowelIdx; // per-note tongue target (== inspector base
        public float vowelDia; //   when the relevant mapping is off)
        public float tensBias; // additive tenseness bias, 0 when M3 off
    }

    /// <summary>
    /// Musical context for C-lite precompute. Built from SmokePartContext for
    /// real renders, synthesized for the test-scale fixture (Ionian on the
    /// fixture root, no metric info). Invalid ctx => all notes get neutral
    /// expressivity (inspector base, zero bias).
    /// </summary>
    private struct ExprContext
    {
        public bool valid;
        public int rootPc;          // pitch class of the key root
        public int[] scalePcs;      // 7 pitch classes in degree order
        public double beatSeconds;  // 0 => no metric info (fixture)
        public int beatsPerBar;
    }

    /// <summary>
    /// Fired on the main thread once a render's schedule is built and just
    /// before playback arms. Args: the rendered file, and the melody chunk the
    /// singer claimed. Listeners may mutate the file freely � the schedule is
    /// already a value-type snapshot. Exists so backing playback can attach
    /// WITHOUT this file knowing anything about MPTK.
    /// </summary>
    public event Action<MidiFile, TrackChunk> RenderReady;

    // ---- main-thread state ----
    private PinkThrombone _voice;
    private float[] _mono;
    private int _sampleRate;
    private bool _builtHalfRate;   // rate mode the CURRENT voice was built at
    private string _status = "Assign a SmokeSetupSO, then Render && Sing.";
    private GUISkin _scaledSkin;

    // ---- schedule ----
    private NoteEvt[] _events = Array.Empty<NoteEvt>();
    private double _songEnd;
    private volatile bool _playing;
    private volatile bool _sustainTest;

    // ---- audio-thread state ----
    private double _clock;
    private int _nextIdx, _nextPitchIdx;
    private bool _gateOpen;
    private double _activeEnd, _noteOnClock;
    private float _loudSmooth, _lp1, _lp2;
    private float _halfPrev;                  // upsampler continuity sample
    private float _curVowelIdx, _curVowelDia; // C-lite per-note targets
    private long _measTicks; private double _measFrames; private int _measBlocks;
    private volatile float _measLoadPct = -1f, _measPeakPct = -1f;

    private void Awake()
    {
        BuildVoice();
    }

    /// <summary>
    /// (Re)builds the voice at the rate implied by halfRateSynthesis.
    /// Main thread only, and only while stopped � the callback's !_playing
    /// early-out means the old voice is never mid-synthesis during a swap.
    /// </summary>
    private void BuildVoice()
    {
        _sampleRate = AudioSettings.outputSampleRate;
        int rate = halfRateSynthesis ? Mathf.Max(8000, _sampleRate / 2) : _sampleRate;
        AudioSettings.GetDSPBufferSize(out int len, out _);
        _mono = new float[len * 2];
        _halfPrev = 0f;
        _builtHalfRate = halfRateSynthesis;
        _voice = new PinkThrombone(rate, new StandardRandomSource());
        Debug.Log($"[PinkTromboneSinger] Voice built at {rate} Hz " +
                  $"(device {_sampleRate} Hz, halfRate={halfRateSynthesis}).");
    }

    /// <summary>Rebuild the voice if the rate-mode toggle changed. Called at
    /// every arm entry point, right after Stop().</summary>
    private void EnsureVoiceMatchesRateMode()
    {
        if (_voice == null || halfRateSynthesis != _builtHalfRate)
            BuildVoice();
    }

    // ================= real render (the integration test) =================

    [ContextMenu("Render && Sing")]
    public void RenderAndSing()
    {
        Stop();
        EnsureVoiceMatchesRateMode();

        if (setup == null) { Fail("No SmokeSetupSO assigned."); return; }
        var config = setup.config;
        if (config == null) { Fail("setup.config (MidiGenPlayConfig) is unassigned."); return; }
        if (setup.entries == null || setup.entries.Count == 0)
        { Fail("setup has no track entries."); return; }

        var specs = setup.entries
            .Where(e => e != null)
            .Select(e => SmokeRenderUtil.BuildEffectiveSpec(
                e.spec, e.chordExpression, e.arpeggioRate,
                e.randomRerollChance, e.randomFigureWeights))
            .ToList();

        SongConfig song;
        try { song = SmokeSongConfigAssembler.Assemble(setup.partContext, specs); }
        catch (ArgumentException ex) { Fail("Assembly failed: " + ex.Message); return; }

        MidiFile file;
        try
        {
            var gen = new MidiGenerator(config);
            var render = gen.Orchestrator.GenerateSinglePart(
                song.Parts[0], song.ChannelRoles, partIndex: 0,
                bpmOverride: setup.partContext.bpm,
                instrumentOverrides: null,
                seedOverride: setup.overrideSeed ? setup.seed : (int?)null);
            file = render?.merged;
        }
        catch (Exception ex) { Fail("Render threw: " + ex); return; }
        if (file == null) { Fail("GenerateSinglePart returned null."); return; }

        TrackChunk selected = null; string selId = null; TrackRole selRole = default;
        foreach (var wantedRole in new[] { TrackRole.Melody, TrackRole.Lead })
        {
            foreach (var chunk in file.GetTrackChunks())
            {
                var tag = chunk.Events.OfType<TextEvent>()
                    .FirstOrDefault(te => te.Text != null &&
                                          te.Text.StartsWith("mus:", StringComparison.Ordinal));
                if (tag == null) continue;
                if (!SongOrchestrator.TryParseMusicianTag(tag.Text, out var id, out var role)) continue;
                if (role != wantedRole) continue;
                selected = chunk; selId = id; selRole = role; break;
            }
            if (selected != null) break;
        }
        if (selected == null)
        { Fail("No chunk tagged mus:*:Melody or mus:*:Lead. Does the setup include a Melody row?"); return; }

        int loggedChannel = selected.Events.OfType<NoteOnEvent>()
            .Select(e => (int)e.Channel).DefaultIfEmpty(-1).First();

        var tempoMap = file.GetTempoMap();
        var list = new List<NoteEvt>();
        foreach (var n in selected.GetNotes())
        {
            list.Add(new NoteEvt
            {
                start = n.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 1_000_000.0,
                end = n.EndTimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 1_000_000.0,
                note = n.NoteNumber,
                vel = n.Velocity
            });
        }
        list.Sort((a, b) => a.start.CompareTo(b.start));

        if (list.Count == 0)
        {
            Fail($"Selected chunk mus:{selId}:{selRole} has zero notes. (MEL-NULL-1: " +
                 "a Melody track with no authored pattern and no phrase palette on " +
                 "config.melodicLeading renders empty.)");
            return;
        }

        double shortest = double.MaxValue; int maxLeap = 0;
        for (int i = 0; i < list.Count; i++)
        {
            shortest = Math.Min(shortest, list[i].end - list[i].start);
            if (i > 0) maxLeap = Math.Max(maxLeap, Math.Abs(list[i].note - list[i - 1].note));
        }

        Debug.Log($"[PinkTromboneSinger] Selected chunk mus:{selId}:{selRole} " +
                  $"(channel={loggedChannel}, notes={list.Count}, " +
                  $"length={list[list.Count - 1].end:F2}s, shortestNote={shortest * 1000:F0}ms, " +
                  $"maxLeap={maxLeap} semitones, transpose={transposeSemitones})");

        // C-lite: the real render carries the full musical context (�5 � all
        // of it already POC-side: tonality, root, meter, bpm).
        var ctx = BuildExprContext(setup.partContext);
        Arm(list, $"mus:{selId}:{selRole}", ctx);
        RenderReady?.Invoke(file, selected);
    }

    // ================= synthetic fixture (tunes the instrument) =============

    /// <summary>
    /// D-S3-G=C. Scale up, held top, scale down, held root, then leaps.
    /// Constant velocity 100 so pitch and timbre are isolated. Touches no
    /// MidiGenPlay code � a voice-tuning fixture, NOT verdict material.
    /// v5: carries an Ionian ExprContext on the fixture root (no metric
    /// info), so M1/M3 audibility can be judged on known material �
    /// constant velocity means ANY tenseness movement you hear is M3.
    /// </summary>
    [ContextMenu("Sing Test Scale")]
    public void SingTestScale()
    {
        Stop();
        EnsureVoiceMatchesRateMode();

        int[] major = { 0, 2, 4, 5, 7, 9, 11 };
        double step = testNoteMs / 1000.0;
        double gap = testGapMs / 1000.0;
        var list = new List<NoteEvt>();
        double t = 0;

        void Add(int midi, double dur)
        {
            list.Add(new NoteEvt
            {
                start = t,
                end = t + Math.Max(0.02, dur - gap),
                note = midi,
                vel = 100
            });
            t += dur;
        }

        int root = testScaleRootMidi;
        for (int oct = 0; oct < testScaleOctaves; oct++)
            foreach (var d in major) Add(root + oct * 12 + d, step);

        Add(root + testScaleOctaves * 12, testHoldSeconds);   // held top

        for (int oct = testScaleOctaves - 1; oct >= 0; oct--)
            for (int i = major.Length - 1; i >= 0; i--) Add(root + oct * 12 + major[i], step);

        Add(root, testHoldSeconds);                            // held root (vibrato)

        Add(root, step); Add(root + 12, step); Add(root, step);
        Add(root + 7, step); Add(root, step); Add(root + 12, step);
        Add(root, testHoldSeconds);

        Debug.Log($"[PinkTromboneSinger] Test scale: {list.Count} notes, {t:F1}s, " +
                  $"root={root}, {testNoteMs:F0}ms/note, gap={testGapMs:F0}ms, " +
                  $"hold={testHoldSeconds:F1}s, transpose={transposeSemitones}");

        // Fixture context: Ionian on the fixture root, beatSeconds = 0 (no
        // meter => M1 metric weight is neutral; duration still drives M1).
        int rootPc = ((root % 12) + 12) % 12;
        var ctx = new ExprContext
        {
            valid = true,
            rootPc = rootPc,
            scalePcs = major.Select(iv => (rootPc + iv) % 12).ToArray(),
            beatSeconds = 0,
            beatsPerBar = 4,
        };
        Arm(list, "TEST SCALE (fixture � not verdict material)", ctx);
    }

    /// <summary>Holds one note indefinitely. Settled D-S3-C (Test 0: PASS).</summary>
    [ContextMenu("Sustain Test Note")]
    public void SustainTestNote()
    {
        Stop();
        EnsureVoiceMatchesRateMode();
        ResetPlaybackState();
        ArmVoiceStatics();

        int n = testNoteNumber + transposeSemitones;
        _voice.SetMusicalNote(n - 69);
        _voice.Loudness = 1f;
        _voice.IsTouched = true;
        _gateOpen = true; _activeEnd = double.MaxValue; _noteOnClock = 0;

        float hz = 440f * Mathf.Pow(2f, (n - 69) / 12f);
        _status = $"SUSTAIN TEST: MIDI {n} = {hz:F1} Hz.";
        _sustainTest = true; _playing = true;
    }

    public void Stop()
    {
        _playing = false; _sustainTest = false;
        if (_voice != null) _voice.IsTouched = false;
    }

    // ================= C-lite precompute (main thread) =================

    private ExprContext BuildExprContext(SmokePartContext pc)
    {
        if (pc == null || pc.bpm <= 0) return default;

        // Governed helpers only � no theory reinvented POC-side.
        var names = GetTonalityNoteNames(pc.tonality, pc.rootNote);
        if (names == null || names.Count < 7) return default;

        var (beatsPerBar, beatUnit) = TimeSignatureProperties[pc.timeSignature];
        return new ExprContext
        {
            valid = true,
            rootPc = PitchClass(pc.rootNote),
            scalePcs = names.Take(7).Select(n => PitchClass(n)).ToArray(),
            // Beat length in seconds: bpm is quarter-note based; x/8 meters
            // count eighths, so scale by 4/beatUnit.
            beatSeconds = (60.0 / pc.bpm) * (4.0 / beatUnit),
            beatsPerBar = beatsPerBar,
        };
    }

    /// <summary>
    /// Stamps per-note vowel targets and tenseness bias. Always runs when the
    /// context is valid (cheap), regardless of the master toggle � so the
    /// master toggle is LIVE during playback. Sub-mapping toggles and amounts
    /// are sampled here, i.e. at arm time.
    /// �5 boundary: everything below derives from note numbers, times and the
    /// part context. Nothing here knows phrases, cadences, or tension arcs �
    /// that residue is deliverable 4's PerformanceSlotInfo field list.
    /// </summary>
    private void ApplyExpressivity(NoteEvt[] arr, ExprContext ctx)
    {
        // v7: with characterEnabled, diction scales all three mapping
        // amounts and M1 morphs from the MOUTH base (not always Neutral).
        // With it off, dic = 1 and morphFrom = Neutral � exact v6 path.
        float dic = characterEnabled ? diction : 1f;
        float m1Amount = vowelOpenAmount * dic;
        float m2Shift = vowelContourIdxShift * dic;
        float m3Amount = tensenessDegreeAmount * dic;
        var bvB = EffBaseVowel();
        float baseIdx = bvB.idx;
        float baseDia = bvB.dia;
        var morphFrom = characterEnabled
            ? VowelPresets[Mathf.Clamp(mouth, 0, VowelPresets.Length - 1)]
            : VowelPresets[0];
        var open = VowelPresets[1];

        for (int i = 0; i < arr.Length; i++)
        {
            arr[i].vowelIdx = baseIdx;
            arr[i].vowelDia = baseDia;
            arr[i].tensBias = 0f;
            if (!ctx.valid) continue;

            double dur = arr[i].end - arr[i].start;

            // Metric weight: 1.0 downbeat, 0.6 other on-beats, 0.25 offbeat,
            // 0.5 when the fixture has no meter.
            float w = 0.5f;
            if (ctx.beatSeconds > 0)
            {
                double beats = arr[i].start / ctx.beatSeconds;
                double inBar = beats % ctx.beatsPerBar;
                double toNearest = inBar - Math.Round(inBar);
                bool onBeat = Math.Abs(toNearest) < 0.10;
                int nearest = ((int)Math.Round(inBar)) % ctx.beatsPerBar;
                w = !onBeat ? 0.25f : (nearest == 0 ? 1f : 0.6f);
            }

            // M1 � vowel openness <- metric weight + duration.
            if (vowelOpennessEnabled)
            {
                float durNorm = Mathf.Clamp01((float)(dur / longNoteSeconds));
                float open01 = Mathf.Clamp01(
                    Mathf.Clamp01(0.5f * durNorm + 0.5f * w) * m1Amount); // v7: dic can push amount past 1
                arr[i].vowelIdx = Mathf.Lerp(morphFrom.idx, open.idx, open01);
                arr[i].vowelDia = Mathf.Lerp(morphFrom.dia, open.dia, open01);
            }

            // M2 � vowel frontness <- contour direction.
            if (vowelContourEnabled && i > 0)
            {
                int dir = Math.Sign(arr[i].note - arr[i - 1].note);
                arr[i].vowelIdx = Mathf.Clamp(
                    arr[i].vowelIdx + dir * m2Shift, 0f, 44f); // v7
            }

            // M3 � tenseness bias <- scale-degree tension. The bias is added
            // to the velocity-mapped value at note-on and clamped to the
            // Test 6 window there.
            if (tensenessDegreeEnabled)
            {
                int rel = (((arr[i].note % 12) - ctx.rootPc) % 12 + 12) % 12;
                int degree = Array.IndexOf(ctx.scalePcs, rel); // -1 = chromatic
                float b = degree switch
                {
                    0 => -1.0f,  // tonic       � maximally settled
                    4 => -0.6f,  // dominant    � stable
                    2 => -0.4f,  // mediant     � stable
                    5 => -0.2f,  // submediant
                    1 => 0.3f,   // supertonic  � mild pull
                    3 => 0.5f,   // subdominant � pull
                    6 => 1.0f,   // leading tone � maximal pull
                    _ => 0.8f,   // chromatic
                };
                arr[i].tensBias = b * m3Amount; // v7
            }
        }
    }

    // ================= arming =================

    private void Arm(List<NoteEvt> list, string label, ExprContext ctx)
    {
        // D-S3-F=B: precompute per-note pitch lead from interval size, capped
        // by the previous note's duration so we never slide out of a short
        // note. Main thread only � the audio callback stays arithmetic-free.
        var arr = list.ToArray();
        for (int i = 0; i < arr.Length; i++)
        {
            if (i == 0) { arr[i].lead = 0; continue; }
            int interval = Math.Abs(arr[i].note - arr[i - 1].note);
            double want = pitchLeadSeconds *
                          Mathf.Clamp01(interval / (float)Math.Max(1, leadFullInterval));
            double prevDur = arr[i - 1].end - arr[i - 1].start;
            arr[i].lead = Math.Min(want, prevDur * 0.6);
        }

        ApplyExpressivity(arr, ctx);

        _events = arr;
        _songEnd = arr[arr.Length - 1].end;
        ResetPlaybackState();
        ArmVoiceStatics();

        _status = $"Singing {label} � {arr.Length} notes, {_songEnd:F1}s" +
                  $"{(loopPlayback ? " [LOOP]" : "")}" +
                  $"{(_builtHalfRate ? " [HALF-RATE]" : "")}" +
                  $"{(expressivityEnabled ? " [EXPR]" : "")}";
        _sustainTest = false;
        _playing = true;
    }

    private void ResetPlaybackState()
    {
        _clock = -syncTrimMs / 1000.0;
        _nextIdx = 0; _nextPitchIdx = 0;
        _gateOpen = false; _activeEnd = 0; _noteOnClock = _clock;
        _loudSmooth = 0f; _lp1 = 0f; _lp2 = 0f;
        _halfPrev = 0f;
        var bv0 = EffBaseVowel();                        // v7: mouth lever
        _curVowelIdx = bv0.idx;
        _curVowelDia = bv0.dia;
    }

    private void ArmVoiceStatics()
    {
        _voice.Reset();
        _voice.VibratoGain = 0f;                 // ramped in per note
        _voice.VibratoFrequency = EffVibratoFrequency;   // v7: lever-aware
        _voice.VibratoWobble = vibratoWobble;
        _voice.PitchJitterGain = EffPitchJitter;         // POC-FORK(6), v7 lever-aware
        _voice.TensenessJitterGain = EffTensenessJitter; // POC-FORK(7), v7 lever-aware
        _voice.AlwaysVoice = alwaysVoice;
        _voice.IsTouched = false;
        // Tongue + tenseness are pushed per block in OnAudioFilterRead.
    }

    private void Fail(string msg)
    {
        _status = "ERROR: " + msg;
        Debug.LogError("[PinkTromboneSinger] " + msg);
    }

    // ================= audio thread =================

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_playing)
        {
            for (int i = 0; i < data.Length; i++) data[i] = 0f;
            return;
        }

        // Rebuilds happen only while stopped (arm entry points), so capturing
        // locals after the _playing check yields a consistent pair.
        var v = _voice;
        var mono = _mono;
        bool halfMode = _builtHalfRate;
        if (v == null || mono == null) return;

        int frames = data.Length / channels;
        if (mono.Length < frames) return;

        long _t0 = measureCallbackLoad ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        bool expr = expressivityEnabled && !_sustainTest;

        // v4: vowel applied EVERY block, so inspector edits and preset buttons
        // take effect live. The model glides toward these (POC-FORK 2), so a
        // change morphs rather than jumps.
        // v5: when C-lite is on, the per-note precomputed targets rule and the
        // glide performs the morph between notes.
        if (expr)
        {
            v.TongueIndex = _curVowelIdx;
            v.TongueDiameter = _curVowelDia;
        }
        else
        {
            var bv = EffBaseVowel();                     // v7: mouth lever
            v.TongueIndex = bv.idx;
            v.TongueDiameter = bv.dia;
        }

        // Test 6 override � hold one tenseness regardless of velocity AND M3.
        if (tensenessOverride)
            v.TargetTenseness = Mathf.Clamp01(tensenessOverrideValue);

        double blockEnd = _clock + (double)frames / _sampleRate;

        if (!_sustainTest)
        {
            while (_nextPitchIdx < _events.Length &&
                   _events[_nextPitchIdx].start - _events[_nextPitchIdx].lead < blockEnd)
            {
                var pv = _events[_nextPitchIdx];
                v.SetMusicalNote(pv.note + transposeSemitones - 69);
                // C-lite: vowel starts morphing at the pitch-lead point, so
                // the tongue arrives with the pitch, not after it.
                _curVowelIdx = pv.vowelIdx;
                _curVowelDia = pv.vowelDia;
                _nextPitchIdx++;
            }

            while (_nextIdx < _events.Length && _events[_nextIdx].start < blockEnd)
            {
                var ev = _events[_nextIdx];
                float vel01 = Mathf.Clamp01(ev.vel / 127f);
                v.Loudness = Mathf.Clamp01(minLoudness + (1f - minLoudness) * vel01);
                if (!tensenessOverride)
                {
                    float t = Mathf.Lerp(tensenessAtVel0, tensenessAtVel127, vel01);
                    if (expr)
                    {
                        // M3: bias inside the window, whichever way round the
                        // endpoints are configured.
                        float lo = Mathf.Min(tensenessAtVel0, tensenessAtVel127);
                        float hi = Mathf.Max(tensenessAtVel0, tensenessAtVel127);
                        t = Mathf.Clamp(t + ev.tensBias, lo, hi);
                    }
                    v.TargetTenseness = Mathf.Clamp01(t);
                }
                v.IsTouched = true;
                _gateOpen = true;
                _activeEnd = ev.end;
                _noteOnClock = _clock;
                if (retriggerOnNoteOn) _loudSmooth = 0f;
                _nextIdx++;
            }

            if (_gateOpen && _clock >= _activeEnd)
            {
                _gateOpen = false;
                v.IsTouched = false;
            }

            if (_nextIdx >= _events.Length && _clock >= _songEnd + 2.0)
            {
                if (loopPlayback)
                {
                    // Cursors only � do NOT Reset() the voice from the audio
                    // thread; the tract should carry over between passes.
                    _clock = 0; blockEnd = (double)frames / _sampleRate;
                    _nextIdx = 0; _nextPitchIdx = 0;
                    _gateOpen = false; _activeEnd = 0; _noteOnClock = 0;
                }
                else _playing = false;
            }
        }

        float held = (float)(_clock - _noteOnClock);
        float vr = vibratoRampSeconds <= 0f
            ? 1f
            : Mathf.Clamp01((held - vibratoDelaySeconds) / vibratoRampSeconds);
        // v7: all lever-aware (EffX = lever when characterEnabled, else raw).
        v.VibratoGain = Mathf.Clamp(EffVibratoGain * vr, 0f, 0.1f);
        v.VibratoFrequency = EffVibratoFrequency;
        // POC-FORK(6/7): live jitter gates. Model setters THROW outside 0..1
        // (Check01), so Eff* clamp before assignment per this file's convention.
        v.PitchJitterGain = EffPitchJitter;
        v.TensenessJitterGain = EffTensenessJitter;

        if (!halfMode)
        {
            var span = new Span<float>(mono, 0, frames);
            v.Synthesize(span);
        }
        else
        {
            // D-S4-A(C): synthesize half the samples at half the rate, then
            // expand 2x in place, back to front (write indices 2i, 2i+1 never
            // collide with unread indices <= i). Midpoint interpolation with
            // one sample of history across blocks (_halfPrev). Imaging from
            // the interpolation sits above the vocal band; the tone stage
            // downstream attenuates it further.
            int hn = (frames + 1) / 2;
            var hspan = new Span<float>(mono, 0, hn);
            v.Synthesize(hspan);
            float lastH = mono[hn - 1];
            float prev = _halfPrev;
            for (int i = hn - 1; i >= 0; i--)
            {
                float cur = mono[i];
                float before = i == 0 ? prev : mono[i - 1];
                int o = 2 * i;
                if (o < frames) mono[o] = 0.5f * (before + cur);
                if (o + 1 < frames) mono[o + 1] = cur;
            }
            _halfPrev = lastH;
        }

        float target = _gateOpen ? 1f : 0f;
        float ms = target > _loudSmooth ? attackMs : releaseMs;
        float k = 1f - Mathf.Exp(-1f / (_sampleRate * ms * 0.001f));
        bool applyExternal = hardGateOutput;

        bool tone = toneEnabled;
        float a = 1f - Mathf.Exp(-2f * Mathf.PI * EffToneCutoffHz / _sampleRate); // v7

        for (int i = 0; i < frames; i++)
        {
            _loudSmooth += (target - _loudSmooth) * k;
            float s = mono[i];
            if (tone)
            {
                _lp1 += a * (s - _lp1);
                _lp2 += a * (_lp1 - _lp2);
                s = _lp2;
            }
            s *= gain * (applyExternal ? _loudSmooth : 1f);
            for (int c = 0; c < channels; c++)
                data[i * channels + c] = s;
        }

        if (measureCallbackLoad)
        {
            _measTicks += System.Diagnostics.Stopwatch.GetTimestamp() - _t0;
            _measFrames += frames; _measBlocks++;
            if (_measBlocks >= 200)   // ~2 s at 1024-frame blocks
            {
                double cpuSec = (double)_measTicks / System.Diagnostics.Stopwatch.Frequency;
                double audioSec = _measFrames / _sampleRate;
                float pct = (float)(100.0 * cpuSec / audioSec);
                _measLoadPct = pct;
                if (pct > _measPeakPct) _measPeakPct = pct;
                _measTicks = 0; _measFrames = 0; _measBlocks = 0;
            }
        }

        _clock = blockEnd;
    }

    // ================= minimal IMGUI =================

    private void OnGUI()
    {
        var prev = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
                                   new Vector3(uiScale, uiScale, 1f));
        if (_scaledSkin == null)
        {
            _scaledSkin = Instantiate(GUI.skin);
            _scaledSkin.label.fontSize = 12;
            _scaledSkin.button.fontSize = 12;
        }
        GUI.skin = _scaledSkin;

        GUILayout.BeginArea(new Rect(10, 10, 500, 320), GUI.skin.box);
        GUILayout.Label("Pink Trombone Singer (Session 4, v5)");
        GUILayout.Label(_status);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Render && Sing")) RenderAndSing();
        if (GUILayout.Button("Test Scale")) SingTestScale();
        if (GUILayout.Button("Sustain")) SustainTestNote();
        if (GUILayout.Button("Stop")) Stop();
        GUILayout.EndHorizontal();

        loopPlayback = GUILayout.Toggle(loopPlayback, " Loop (singer only � backing won't loop)");

        measureCallbackLoad = GUILayout.Toggle(measureCallbackLoad, " Measure callback load");
        if (measureCallbackLoad)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"load {_measLoadPct:F1}%   peak {_measPeakPct:F1}%");
            if (GUILayout.Button("Reset peak")) { _measPeakPct = -1f; _measLoadPct = -1f; }
            GUILayout.EndHorizontal();
        }

        // --- D-S4-A: rate mode (applies at next Render/Test press) ---
        bool wantHalf = GUILayout.Toggle(halfRateSynthesis,
            $" Half-rate synthesis (D-S4-A C){(halfRateSynthesis != _builtHalfRate ? "  [applies on next Render/Test]" : "")}");
        halfRateSynthesis = wantHalf;

        // --- C-lite master (live; sub-mappings sampled at arm time) ---
        expressivityEnabled = GUILayout.Toggle(expressivityEnabled,
            " C-lite expressivity (per-note vowel + degree tenseness)" +
            (expressivityEnabled ? "  [vowel buttons inactive while singing]" : ""));

        // --- Test 6: tenseness ---
        GUILayout.Space(4);
        tensenessOverride = GUILayout.Toggle(tensenessOverride,
            $" Tenseness override: {tensenessOverrideValue:F2}" +
            (tensenessOverride ? "  [velocity + M3 mapping BYPASSED]" : ""));
        GUILayout.BeginHorizontal();
        for (int i = 0; i < TensenessPresets.Length; i++)
        {
            if (GUILayout.Button(TensenessPresets[i].ToString("F2")))
            {
                tensenessOverrideValue = TensenessPresets[i];
                tensenessOverride = true;
            }
        }
        GUILayout.EndHorizontal();

        // --- Test 7: vowel (base values; ruled out while EXPR sings) ---
        GUILayout.Space(4);
        GUILayout.Label($"Vowel base: idx {tongueIndex:F1} / dia {tongueDiameter:F2}");
        GUILayout.BeginHorizontal();
        foreach (var p in VowelPresets)
        {
            if (GUILayout.Button(p.name))
            {
                tongueIndex = p.idx;
                tongueDiameter = p.dia;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
        GUI.matrix = prev;
    }
}
using System.Collections.Generic;
using ALWTTT.Enums;
using ALWTTT.Fx;
using ALWTTT.Music;
using UnityEngine;

/// <summary>
/// [RFX-1] Play-mode test harness for RhythmParticleEmitter.
///
/// WHY THIS EXISTS
/// The editor's particle preview cannot show what RFX-1 produces. The preview
/// only runs what the ParticleSystem does on its own, and RFX-1 deliberately
/// tells it to do nothing: Rate over Time = 0, no bursts, and every meaningful
/// value (speed, size, lifetime, direction) overridden per emit by
/// RhythmParticleEmitter from RhythmFxConfigSO. Nothing calls Emit() in edit
/// mode, so the preview shows a dead system.
///
/// The authoring loop is Play mode instead:
///   1. Enter Play in the sandbox scene.
///   2. This component fires bursts on a fake groove.
///   3. Tune RhythmFxConfig.asset WHILE PLAYING.
///   4. Exit Play. The values survive, because an asset is not a component.
///
/// v2: the groove now reaches all seven RFX-1 lanes. v1 only played Kick,
/// Snare, HiHatClosed, Cymbal and Chord, which left Tom and HiHatOpen testable
/// only as isolated manual bursts - and an isolated burst is exactly the test
/// that cannot tell you whether a lane survives musical density.
///
/// ------------------------------------------------------------------
/// [RFX-2] v3: chord ladder cycle + MIDI note injection
/// ------------------------------------------------------------------
/// (1) chordLadderCycle drives the five ladder rungs in order, holding each for
///     barsPerRung bars. This is how you judge whether ADJACENT rungs read as
///     different, which is the only thing the ladder has to get right.
///
/// (2) InjectDrumNote pushes a synthetic MidiTaggedEvent through
///     RhythmParticleMidiListener.OnMidiNote. This exists because Emit()
///     BYPASSES LaneFor entirely - the note-to-lane mapping, and therefore the
///     RFX-2 clamp fix, is untestable through the groove driver. Injection is
///     the only way to smoke-test it outside a real MIDI file.
///
///     The sandbox has no MidiMusicManager, so the listener logs a subscribe
///     warning at enable. That is expected and harmless: OnMidiNote is a plain
///     public method and works whether or not registration ever happened.
///
/// (3) [RFX-2 v2] InjectChord does the same for the HARMONY path, and it is not
///     optional polish - it is the only way the chord logic can be tested at
///     all from the sandbox.
///
///     The groove driver above calls target.Emit(lane, ...) - the EMITTER,
///     directly, with the lane already chosen. It never touches the listener.
///     So chordMinNotes, change detection, LadderFor's pitch-class collapse,
///     IsLaneReady and the D2=B fallback are ALL bypassed by the groove driver,
///     and every listener counter stays at zero no matter what the groove does.
///     Injecting a synthetic ChordEvent through OnChord is the only sandbox
///     path that exercises them.
///
/// SCOPE: development only. Lives in the RhythmFx sandbox scene, never in the
/// gig scene and never on a musician prefab.
/// </summary>
[DisallowMultipleComponent]
public sealed class RhythmFxTester : MonoBehaviour
{
    public enum HiHatMode { Off, Quarters, Eighths, Sixteenths }

    /// <summary>[RFX-2] Synthetic chord shapes for OnChord injection. Each one
    /// targets a specific rung, including the two that real backing content may
    /// never produce on demand.</summary>
    public enum ChordPreset { Custom, Unison, PowerOctave, Triad, Seventh, Extended, Dyad }

    [Header("Target")]
    [Tooltip("Leave empty to find the first emitter in the scene at Start.")]
    [SerializeField] private RhythmParticleEmitter target;

    [Header("Manual burst")]
    [Tooltip("Lane used by the 'Burst Selected Lane' context-menu command.")]
    [SerializeField] private RhythmLane manualLane = RhythmLane.Kick;

    [Range(0f, 1f)]
    [SerializeField] private float manualVelocity = 1f;

    [Header("Auto groove")]
    [Tooltip("Fake 4/4 pattern so you can judge the effect at musical density " +
             "instead of one isolated hit.")]
    [SerializeField] private bool autoGroove = true;

    [Range(40f, 200f)]
    [SerializeField] private float bpm = 90f;

    [Header("Auto groove - lanes")]
    [SerializeField] private bool kickOn1And3 = true;
    [SerializeField] private bool snareOn2And4 = true;
    [SerializeField] private HiHatMode hiHat = HiHatMode.Eighths;

    [Tooltip("Open hat on the 'and' of beat 4 - the classic bar-turnaround " +
             "accent. Also the only place HiHatOpen appears in a real pattern.")]
    [SerializeField] private bool openHatOnTurnaround = false;

    [Tooltip("Four-tom fill across the last beat of every 4th bar. This is the " +
             "densest thing the Tom lane will ever have to survive.")]
    [SerializeField] private bool tomFillEveryFourBars = false;

    [SerializeField] private bool crashOnBarOne = false;

    [Tooltip("Legacy single Chord lane, once per bar. Ignored while " +
             "chordLadderCycle is on.")]
    [SerializeField] private bool chordEveryBar = false;

    [Header("Auto groove - chord ladder [RFX-2]")]
    [Tooltip("Cycle the five ladder rungs in order: Single, Power, Triad, " +
             "Seventh, Extended. Takes precedence over chordEveryBar so the " +
             "two can never double-burst on the same step.")]
    [SerializeField] private bool chordLadderCycle = false;

    [Tooltip("Bars each rung holds before advancing. 2 is the useful setting " +
             "when judging whether two ADJACENT rungs read apart - one bar each " +
             "cycles too fast to compare.")]
    [Range(1, 4)]
    [SerializeField] private int barsPerRung = 2;

    [Header("Velocity simulation")]
    [Tooltip("Accented downbeats and quieter offbeats, so velocityInfluence is " +
             "actually exercised instead of every hit arriving at 1.0. Note the " +
             "ladder rungs ignore this: they are authored with " +
             "velocityInfluence = 0 because the listener passes a flat 1f.")]
    [SerializeField] private bool simulateAccents = true;

    [Header("MIDI injection [RFX-2 / ST-RFX-11..14]")]
    [Tooltip("Leave empty to find the listener at injection time. Add a " +
             "RhythmParticleMidiListener to the sandbox scene for these tests.")]
    [SerializeField] private RhythmParticleMidiListener injectionTarget;

    [Tooltip("MIDI note number to inject. 31 = Sticks, the RFX-1 clamp's " +
             "favourite phantom kick. 36 = BassDrum1, the positive control. " +
             "39 = HandClap, which lands on the unmapped Perc bucket.")]
    [Range(0, 127)]
    [SerializeField] private int injectNote = 31;

    [Range(1, 127)]
    [SerializeField] private int injectVelocity = 100;

    [Tooltip("Must match the listener's drumChannel or OnMidiNote returns early.")]
    [SerializeField] private int injectChannel = 9;

    [Tooltip("How many injections 'Inject Drum Note x5' fires. Used by " +
             "ST-RFX-14 to prove the diagnostic logs once per note number.")]
    [Range(2, 20)]
    [SerializeField] private int injectRepeatCount = 5;

    [Header("Chord injection [RFX-2 / ST-RFX-18, 21..23]")]
    [Tooltip("Which synthetic chord to push through the listener's OnChord.\n\n" +
             "Unison      60,72,84    -> 3 raw notes, 1 pitch class  -> ChordSingle\n" +
             "PowerOctave 60,67,72    -> 3 raw notes, 2 pitch classes -> ChordPower\n" +
             "Triad       60,64,67    -> 3 raw, 3 classes -> ChordTriad\n" +
             "Seventh     60,64,67,70 -> 4 raw, 4 classes -> ChordSeventh\n" +
             "Extended    60,64,67,70,74 -> 5 raw, 5 classes -> ChordExtended\n" +
             "Dyad        60,67       -> 2 raw -> REJECTED by chordMinNotes\n\n" +
             "PowerOctave is the one that matters most: three raw notes that " +
             "must classify as Power, not Triad. That is D1=B in one test.")]
    [SerializeField] private ChordPreset chordPreset = ChordPreset.Triad;

    [Tooltip("Used only when chordPreset = Custom.")]
    [SerializeField] private List<int> customChordNotes = new List<int> { 60, 64, 67 };

    [Tooltip("Must NOT equal the listener's drumChannel or OnChord returns early.")]
    [SerializeField] private int injectChordChannel = 0;

    [Tooltip("Chord label. LEAVE EMPTY for throttle testing: an empty label is " +
             "the LABEL MISS path, which skips change detection and lets the " +
             "same chord reach the emitter twice. With a label set, the second " +
             "identical injection is suppressed as a repeat and never reaches " +
             "the throttle at all - which is correct behaviour, and which is " +
             "why it is the wrong setting for ST-RFX-18.")]
    [SerializeField] private string injectChordSymbol = "";

    [Header("Diagnostics")]
    [SerializeField] private bool logSteps = false;

    // [RFX-2] Ladder order. Static and readonly: this is the definition of
    // "adjacent rungs" for the visual comparison, not a tunable.
    private static readonly RhythmLane[] Ladder =
    {
        RhythmLane.ChordSingle,
        RhythmLane.ChordPower,
        RhythmLane.ChordTriad,
        RhythmLane.ChordSeventh,
        RhythmLane.ChordExtended
    };

    private float _stepDuration;   // one 16th
    private float _nextStepTime;
    private int _step;             // 0..15 within the bar
    private int _bar;

    private void Start()
    {
        if (target == null)
            target = FindFirstObjectByType<RhythmParticleEmitter>();

        if (target == null)
        {
            Debug.LogError("[RhythmFxTester] No RhythmParticleEmitter in the scene. " +
                           "Add the component to the RhythmFx object inside the " +
                           "musician PREFAB (not the scene instance).", this);
            enabled = false;
            return;
        }

        ResetClock();
        Debug.Log($"[RhythmFxTester] Driving '{target.name}' at {bpm} BPM. " +
                  $"Tune RhythmFxConfig.asset while playing; asset edits persist.", this);
    }

    private void ResetClock()
    {
        _stepDuration = 60f / Mathf.Max(1f, bpm) / 4f;
        _nextStepTime = Time.time;
        _step = 0;
        _bar = 0;
    }

    private void Update()
    {
        if (!autoGroove || target == null) return;

        // Recompute if the designer drags the BPM slider mid-play.
        float expected = 60f / Mathf.Max(1f, bpm) / 4f;
        if (!Mathf.Approximately(expected, _stepDuration))
            _stepDuration = expected;

        // while, not if: survives a frame hitch without dropping the grid.
        while (Time.time >= _nextStepTime)
        {
            FireStep(_step);
            _nextStepTime += _stepDuration;

            _step++;
            if (_step >= 16) { _step = 0; _bar++; }
        }
    }

    private void FireStep(int step)
    {
        bool fillBar = tomFillEveryFourBars && _bar % 4 == 3;

        // Beat map, 16th steps:  0=1  4=2  8=3  12=4
        if (kickOn1And3 && (step == 0 || step == 8))
            Fire(RhythmLane.Kick, Accent(step));

        // The tom fill replaces the snare on beat 4, like a real drummer would.
        if (snareOn2And4 && (step == 4 || (step == 12 && !fillBar)))
            Fire(RhythmLane.Snare, Accent(step));

        if (fillBar && step >= 12)
            Fire(RhythmLane.Tom, Accent(step));

        bool hat = hiHat switch
        {
            HiHatMode.Quarters => step % 4 == 0,
            HiHatMode.Eighths => step % 2 == 0,
            HiHatMode.Sixteenths => true,
            _ => false
        };
        // The open hat takes the slot, so you never get both hats at once.
        bool openHat = openHatOnTurnaround && step == 14;
        if (openHat) Fire(RhythmLane.HiHatOpen, 0.9f);
        else if (hat && !fillBar) Fire(RhythmLane.HiHatClosed, Accent(step) * 0.75f);

        if (crashOnBarOne && step == 0 && _bar % 4 == 0)
            Fire(RhythmLane.Cymbal, 1f);

        // [RFX-2] Harmony, once per bar. The ladder wins over the legacy lane so
        // the two cannot fire on the same step and be mistaken for one burst.
        if (step == 0)
        {
            if (chordLadderCycle)
                Fire(Ladder[(_bar / Mathf.Max(1, barsPerRung)) % Ladder.Length], 1f);
            else if (chordEveryBar)
                Fire(RhythmLane.Chord, 1f);
        }
    }

    // Downbeats loud, offbeats quieter, 16th subdivisions quieter still.
    private float Accent(int step)
    {
        if (!simulateAccents) return 1f;
        if (step % 8 == 0) return 1f;
        if (step % 4 == 0) return 0.85f;
        if (step % 2 == 0) return 0.7f;
        return 0.55f;
    }

    private void Fire(RhythmLane lane, float velocity)
    {
        bool emitted = target.Emit(lane, Mathf.Clamp01(velocity));
        if (logSteps)
            Debug.Log($"[RhythmFxTester] bar {_bar} step {_step:00} {lane} " +
                      $"vel={velocity:0.00} emitted={emitted}");
    }

    #region Context menu commands (right-click the component header in Play mode)

    [ContextMenu("Burst Selected Lane")]
    private void BurstSelectedLane()
    {
        if (!EnsurePlaying()) return;
        bool ok = target.Emit(manualLane, manualVelocity, ignoreThrottle: true);
        Debug.Log($"[RhythmFxTester] {manualLane} burst -> emitted={ok}" +
                  (ok ? "" : "  (lane disabled, unauthored, or no ParticleSystem in slots)"));
    }

    [ContextMenu("Burst Every Lane")]
    private void BurstEveryLane()
    {
        if (!EnsurePlaying()) return;
        foreach (RhythmLane lane in System.Enum.GetValues(typeof(RhythmLane)))
        {
            bool ok = target.Emit(lane, manualVelocity, ignoreThrottle: true);
            Debug.Log($"[RhythmFxTester] {lane} -> emitted={ok}");
        }
        Debug.Log("[RhythmFxTester] 'Perc' reporting false is EXPECTED: it has " +
                  "no authored lane entry by design. [RFX-2] A ladder rung " +
                  "reporting false means Task 4 authoring is incomplete for " +
                  "that rung - the gig scene will fall back to 'Chord' for it.");
    }

    [ContextMenu("Burst Ladder (all five rungs)")]
    private void BurstLadder()
    {
        if (!EnsurePlaying()) return;
        foreach (var lane in Ladder)
        {
            bool ok = target.Emit(lane, 1f, ignoreThrottle: true);
            Debug.Log($"[RhythmFxTester] {lane} -> emitted={ok}");
        }
        Debug.Log("[RhythmFxTester] All five rungs fired at once. Use " +
                  "chordLadderCycle for the version that is actually readable.");
    }

    [ContextMenu("Inject Drum Note")]
    private void InjectDrumNote() => Inject(1);

    [ContextMenu("Inject Drum Note x5")]
    private void InjectDrumNoteRepeat() => Inject(injectRepeatCount);

    /// <summary>
    /// [RFX-2] Push synthetic MidiTaggedEvents through the listener's note path.
    /// This is the ONLY way to exercise LaneFor and the GM range check from the
    /// sandbox: RhythmParticleEmitter.Emit takes a lane directly and never sees
    /// a note number.
    /// </summary>
    private void Inject(int times)
    {
        if (!EnsurePlaying()) return;

        if (injectionTarget == null)
            injectionTarget = FindFirstObjectByType<RhythmParticleMidiListener>();

        if (injectionTarget == null)
        {
            Debug.LogError("[RhythmFxTester] No RhythmParticleMidiListener in the " +
                           "sandbox scene. Add one (it will warn about the missing " +
                           "MidiMusicManager; that is expected) to run ST-RFX-11..14.",
                           this);
            return;
        }

        var anchor = target != null ? target.transform : transform;
        long before = injectionTarget.PercussionBursts;

        for (int i = 0; i < Mathf.Max(1, times); i++)
        {
            injectionTarget.OnMidiNote(new MidiTaggedEvent
            {
                musicianId = "sandbox",
                channel = injectChannel,
                note = injectNote,
                velocity = injectVelocity,
                time = Time.time,
                anchor = anchor
            });
        }

        Debug.Log($"[RhythmFxTester] injected note={injectNote} ch={injectChannel} " +
                  $"x{Mathf.Max(1, times)} -> PercussionBursts {before} -> " +
                  $"{injectionTarget.PercussionBursts}  " +
                  $"(delta={injectionTarget.PercussionBursts - before})");
    }

    [ContextMenu("Inject Chord")]
    private void InjectChordOnce() => InjectChord(1);

    [ContextMenu("Inject Chord x2 (throttle test)")]
    private void InjectChordTwice() => InjectChord(2);

    /// <summary>
    /// [RFX-2] Push a synthetic ChordEvent through the listener's harmony path.
    ///
    /// This is the ONLY sandbox route that exercises chordMinNotes, change
    /// detection, LadderFor, IsLaneReady and the D2=B fallback. The groove
    /// driver calls Emit(lane, ...) with the lane already decided, so it proves
    /// nothing about any of them.
    ///
    /// ChordEvent.quality is deliberately left at its default (null). That is
    /// not a shortcut - it is the LABEL MISS case, and the ladder is required to
    /// classify correctly without it. If a future change makes the ladder read
    /// quality, these injections start failing, which is the point.
    /// </summary>
    private void InjectChord(int times)
    {
        if (!EnsurePlaying()) return;

        if (injectionTarget == null)
            injectionTarget = FindFirstObjectByType<RhythmParticleMidiListener>();

        if (injectionTarget == null)
        {
            Debug.LogError("[RhythmFxTester] No RhythmParticleMidiListener in the " +
                           "sandbox scene. Add one to run ST-RFX-18 and 21..23.", this);
            return;
        }

        var notes = NotesFor(chordPreset);
        string symbol = string.IsNullOrEmpty(injectChordSymbol) ? null : injectChordSymbol;

        if (times > 1 && symbol != null)
            Debug.LogWarning("[RhythmFxTester] injectChordSymbol is set, so the " +
                             "second injection will be SUPPRESSED as a repeat and " +
                             "will never reach the throttle. Clear the field to run " +
                             "ST-RFX-18.", this);

        var anchor = target != null ? target.transform : transform;

        long b0 = injectionTarget.ChordBursts;
        long f0 = injectionTarget.ChordLadderFallbacks;
        long s0 = injectionTarget.ChordsSuppressed;
        long m0 = injectionTarget.ChordsBelowMinNotes;

        for (int i = 0; i < Mathf.Max(1, times); i++)
        {
            injectionTarget.OnChord(new ChordEvent
            {
                musicianId = "sandbox",
                channel = injectChordChannel,
                notes = new List<int>(notes),
                time = Time.time,
                anchor = anchor,
                symbol = symbol,
                roman = null,
                degreeIndex = 0
                // quality intentionally left null - LABEL MISS path
            });
        }

        Debug.Log($"[RhythmFxTester] injected chord {chordPreset} " +
                  $"[{string.Join(",", notes)}] ch={injectChordChannel} " +
                  $"label={(symbol ?? "(none)")} x{Mathf.Max(1, times)}\n" +
                  $"  ChordBursts        {b0} -> {injectionTarget.ChordBursts}" +
                  $"  (delta={injectionTarget.ChordBursts - b0})\n" +
                  $"  ChordLadderFallbacks {f0} -> {injectionTarget.ChordLadderFallbacks}" +
                  $"  (delta={injectionTarget.ChordLadderFallbacks - f0})\n" +
                  $"  ChordsSuppressed   {s0} -> {injectionTarget.ChordsSuppressed}" +
                  $"  (delta={injectionTarget.ChordsSuppressed - s0})\n" +
                  $"  ChordsBelowMinNotes {m0} -> {injectionTarget.ChordsBelowMinNotes}" +
                  $"  (delta={injectionTarget.ChordsBelowMinNotes - m0})");
    }

    private List<int> NotesFor(ChordPreset preset) => preset switch
    {
        ChordPreset.Unison => new List<int> { 60, 72, 84 },       // 1 pitch class
        ChordPreset.PowerOctave => new List<int> { 60, 67, 72 },  // 2 - THE D1=B case
        ChordPreset.Triad => new List<int> { 60, 64, 67 },        // 3
        ChordPreset.Seventh => new List<int> { 60, 64, 67, 70 },  // 4
        ChordPreset.Extended => new List<int> { 60, 64, 67, 70, 74 }, // 5
        ChordPreset.Dyad => new List<int> { 60, 67 },             // 2 raw - rejected
        _ => customChordNotes ?? new List<int>()
    };

    [ContextMenu("Audit Wiring")]
    private void AuditWiring()
    {
        if (target == null) target = FindFirstObjectByType<RhythmParticleEmitter>();
        if (target == null) { Debug.LogError("[RhythmFxTester] No emitter."); return; }

        var cfg = target.Config;
        var sb = new System.Text.StringBuilder("[RhythmFxTester] wiring audit\n");
        sb.AppendLine($"  emitter : {target.name}");
        sb.AppendLine($"  config  : {(cfg != null ? cfg.name : "NULL")}");

        foreach (RhythmLane lane in System.Enum.GetValues(typeof(RhythmLane)))
        {
            bool hasSlot = target.HasLane(lane);
            var entry = cfg != null ? cfg.For(lane) : null;
            string state = (hasSlot, entry != null) switch
            {
                (true, true) => entry.enabled ? "OK" : "asset entry DISABLED",
                (true, false) => "ParticleSystem wired, NO asset entry",
                (false, true) => "asset entry present, NO ParticleSystem in slots",
                (false, false) => "unauthored"
            };

            // [RFX-2] Play-mode readiness is the question the ladder fallback
            // actually asks; print it next to the authoring state.
            string ready = Application.isPlaying
                ? (target.IsLaneReady(lane) ? "  [ready]" : "  [NOT ready]")
                : "";

            sb.AppendLine($"  {lane,-14} {state}{ready}");
        }
        sb.AppendLine("  Perc 'unauthored' is expected by design.");
        Debug.Log(sb.ToString());
    }

    [ContextMenu("Log Burst Counts")]
    private void LogBurstCounts()
    {
        if (target == null) return;
        var sb = new System.Text.StringBuilder("[RhythmFxTester] burst counts: ");
        foreach (RhythmLane lane in System.Enum.GetValues(typeof(RhythmLane)))
            sb.Append($"{lane}={target.BurstsFor(lane)}  ");
        Debug.Log(sb.ToString());
    }

    [ContextMenu("Log Listener Counters")]
    private void LogListenerCounters()
    {
        if (injectionTarget == null)
            injectionTarget = FindFirstObjectByType<RhythmParticleMidiListener>();
        if (injectionTarget == null)
        {
            Debug.LogWarning("[RhythmFxTester] No RhythmParticleMidiListener.");
            return;
        }

        Debug.Log($"[RhythmFxTester] listener: " +
                  $"PercussionBursts={injectionTarget.PercussionBursts}  " +
                  $"ChordBursts={injectionTarget.ChordBursts}  " +
                  $"ChordsSuppressed={injectionTarget.ChordsSuppressed}  " +
                  $"ChordsBelowMinNotes={injectionTarget.ChordsBelowMinNotes}  " +
                  $"ChordLadderFallbacks={injectionTarget.ChordLadderFallbacks}");
    }

    [ContextMenu("Restart Groove Clock")]
    private void RestartClock()
    {
        if (!EnsurePlaying()) return;
        ResetClock();
    }

    private bool EnsurePlaying()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[RhythmFxTester] Enter Play mode first. Manual " +
                             "Emit in edit mode does not survive the editor's " +
                             "own particle resimulation.", this);
            return false;
        }
        if (target == null) target = FindFirstObjectByType<RhythmParticleEmitter>();
        return target != null;
    }

    #endregion
}
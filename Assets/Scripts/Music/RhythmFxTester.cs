using ALWTTT.Enums;
using ALWTTT.Fx;
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
/// v2: the groove now reaches all seven lanes. v1 only played Kick, Snare,
/// HiHatClosed, Cymbal and Chord, which left Tom and HiHatOpen testable only as
/// isolated manual bursts - and an isolated burst is exactly the test that
/// cannot tell you whether a lane survives musical density.
///
/// SCOPE: development only. Lives in the RhythmFx sandbox scene, never in the
/// gig scene and never on a musician prefab.
/// </summary>
[DisallowMultipleComponent]
public sealed class RhythmFxTester : MonoBehaviour
{
    public enum HiHatMode { Off, Quarters, Eighths, Sixteenths }

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
    [SerializeField] private bool chordEveryBar = false;

    [Header("Velocity simulation")]
    [Tooltip("Accented downbeats and quieter offbeats, so velocityInfluence is " +
             "actually exercised instead of every hit arriving at 1.0.")]
    [SerializeField] private bool simulateAccents = true;

    [Header("Diagnostics")]
    [SerializeField] private bool logSteps = false;

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

        if (chordEveryBar && step == 0)
            Fire(RhythmLane.Chord, 1f);
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
                  "no authored lane entry by design.");
    }

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
            sb.AppendLine($"  {lane,-13} {state}");
        }
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
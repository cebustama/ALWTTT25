// VibeReadoutBeatPulse.cs  — v3 (BIGNUM-1r45: probe + fallback tempo)
using ALWTTT.Music;
using System.Collections;
using UnityEngine;

namespace ALWTTT.UI
{
    /// <summary>
    /// [BIGNUM-1 / D-BN-3=A] Beat-synced pulse for the C1 Vibe readout. Same clock as
    /// <see cref="BeatPulseIndicator"/>: the MIDI beat grid via
    /// <see cref="MidiListenerCanvasBase"/> (OnBeat / OnDownbeat / OnTempoChanged).
    ///
    /// [BIGNUM-1r] v2 adds:
    ///  - a PROBE (logBeats): logs registration state, every incoming beat and every
    ///    StartPulse decision under the tag [VibePulse], so ST-BN-6 can be diagnosed
    ///    from the console instead of by eye. Turn OFF once ST-BN-6 is green.
    ///  - a FALLBACK (selfTimerWhenGridSilent, OFF by default — D-BN-3b, decide with
    ///    the probe's evidence): if the grid delivers no beat for 2 beats, tick from
    ///    the last known BPM. Only meant for the case "the grid is not emitted in the
    ///    gig scene"; it stays silent while real beats arrive.
    ///
    /// Owns the target's localScale. GigCanvas sets the BASE scale (magnitude → size)
    /// and the INTENSITY (magnitude → pop strength); this component pops on top of the
    /// base on every beat.
    /// </summary>
    public class VibeReadoutBeatPulse : MidiListenerCanvasBase
    {
        [Header("Target")]
        [Tooltip("RectTransform to scale. Defaults to this object's RectTransform.")]
        [SerializeField] private RectTransform target;
        [SerializeField] private bool useUnscaledTime = true;

        [Header("Pop Timing (fractions of one beat)")]
        [SerializeField, Range(0f, 1f)] private float growFrac = 0.20f;
        [SerializeField, Range(0f, 1f)] private float settleFrac = 0.45f;

        [Header("Pop Strength (relative to base scale)")]
        [Tooltip("Pop multiplier at intensity 0 — a small but visible tick.")]
        [SerializeField] private float minPop = 1.08f;
        [Tooltip("Pop multiplier at intensity 1 — a big beat.")]
        [SerializeField] private float maxPop = 1.45f;
        [Tooltip("Extra multiplier on downbeats.")]
        [SerializeField] private float downbeatBoost = 1.10f;

        [Header("Curve")]
        [SerializeField]
        private AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Probe (BIGNUM-1r — ST-BN-6)")]
        [Tooltip("Log [VibePulse] lines: enable/register state, first beats, pulse decisions.")]
        [SerializeField] private bool logBeats = true;

        [Header("Fallback (D-BN-3b — keep OFF unless the probe shows a silent grid)")]
        [Tooltip("If no grid beat arrives for 2 beats, tick from the last known BPM.")]
        [SerializeField] private bool selfTimerWhenGridSilent = false;

        // live tempo data (from ITempoSignatureListener via the base class)
        double _bpm = 120;
        int _den = 4;

        float SecPerBeat => (float)((60.0 / _bpm) * (4.0 / _den));

        float _baseScale = 1f;
        float _intensity01;
        Coroutine _pulseCo;
        Coroutine _timerCo;

        int _beatCount;
        float _lastGridBeatTime = -999f;

        /// <summary>Base (resting) scale — the magnitude-driven size set by GigCanvas.</summary>
        public void SetBaseScale(float scale)
        {
            _baseScale = Mathf.Max(0.01f, scale);
            if (_pulseCo == null && target)
                target.localScale = Vector3.one * _baseScale;
        }

        /// <summary>0 = faint tick, 1 = big beat. Set by GigCanvas from the C1 magnitude.</summary>
        public void SetIntensity01(float intensity01)
        {
            _intensity01 = Mathf.Clamp01(intensity01);
        }

        /// <summary>
        /// [BIGNUM-1r4 / D-BN-3c] Tempo for the FALLBACK timer, pushed by
        /// GigManager.ApplyBpmToStage while the beat grid is silent (F-BN-5 / BEAT-1).
        /// A real OnTempoChanged from the grid overwrites this the moment one arrives —
        /// the grid stays authoritative; this only fills the gap. The denominator keeps
        /// its last known value: the resolved-BPM path carries no time signature, so a
        /// non-4 denominator stays wrong until BEAT-1 closes (declared debt).
        /// </summary>
        public void SetFallbackBpm(int bpm)
        {
            if (bpm <= 0) return;
            _bpm = bpm;
            if (logBeats)
                Debug.Log($"[VibePulse] SetFallbackBpm({bpm}) secPerBeat={SecPerBeat:0.000}", this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!target) target = GetComponent<RectTransform>();
            if (target) target.localScale = Vector3.one * _baseScale; // never resume mid-pop
            _pulseCo = null;
            _beatCount = 0;

            if (logBeats)
                Debug.Log($"[VibePulse] OnEnable  target={(target ? target.name : "NULL")} " +
                          $"activeInHierarchy={gameObject.activeInHierarchy} " +
                          $"MidiMusicManager={(Midi != null ? "present" : "NULL")} " +
                          $"bpm={_bpm:0.#} den={_den}", this);

            if (selfTimerWhenGridSilent)
                _timerCo = StartCoroutine(SelfTimer());
        }

        protected override void OnDisable()
        {
            if (_timerCo != null) { StopCoroutine(_timerCo); _timerCo = null; }
            if (logBeats)
                Debug.Log($"[VibePulse] OnDisable  beatsReceived={_beatCount}", this);
            base.OnDisable();
        }

        protected override void OnTempoChanged(double bpm)
        {
            _bpm = bpm <= 0 ? 120 : bpm;
            if (logBeats) Debug.Log($"[VibePulse] OnTempoChanged bpm={_bpm:0.#}", this);
        }

        protected override void OnTimeSignatureChanged(int n, int d)
        {
            _den = Mathf.Max(1, d);
            if (logBeats) Debug.Log($"[VibePulse] OnTimeSignatureChanged {n}/{d}", this);
        }

        protected override void OnBeat(BeatGridEvent e)
        {
            NoteGridBeat(e, "beat");
            StartPulse(1f);
        }

        protected override void OnDownbeat(BeatGridEvent e)
        {
            NoteGridBeat(e, "DOWNBEAT");
            StartPulse(downbeatBoost);
        }

        void NoteGridBeat(BeatGridEvent e, string kind)
        {
            _beatCount++;
            _lastGridBeatTime = useUnscaledTime ? Time.unscaledTime : Time.time;
            if (logBeats && _beatCount <= 12)
                Debug.Log($"[VibePulse] {kind} #{_beatCount} bar={e.barIndex} beatInBar={e.beatInBar} " +
                          $"t={e.time:0.00} frame={Time.frameCount}", this);
        }

        // ─────────────────────────────────────────────────────────────────────

        IEnumerator SelfTimer()
        {
            // D-BN-3b fallback: silent while the grid is alive.
            while (true)
            {
                float secBeat = Mathf.Max(0.05f, SecPerBeat);
                yield return useUnscaledTime
                    ? new WaitForSecondsRealtime(secBeat)
                    : new WaitForSeconds(secBeat);

                float now = useUnscaledTime ? Time.unscaledTime : Time.time;
                bool gridSilent = (now - _lastGridBeatTime) > 2f * secBeat;
                if (gridSilent) StartPulse(1f, fromTimer: true);
            }
        }

        void StartPulse(float boost, bool fromTimer = false)
        {
            if (!target)
            {
                if (logBeats) Debug.Log("[VibePulse] StartPulse SKIPPED: target NULL", this);
                return;
            }
            if (!isActiveAndEnabled)
            {
                if (logBeats) Debug.Log("[VibePulse] StartPulse SKIPPED: not active/enabled", this);
                return;
            }
            if (_pulseCo != null) StopCoroutine(_pulseCo);

            // keep total pop shorter than a beat (cap at ~90% of beat) — same rule as
            // BeatPulseIndicator so the two pulses read as one clock.
            float secBeat = Mathf.Max(0.01f, SecPerBeat);
            float totalFrac = Mathf.Clamp01(growFrac + settleFrac);
            if (totalFrac <= 0f) totalFrac = 0.65f;
            float roomScale = Mathf.Min(0.90f, totalFrac) / totalFrac;

            float tGrow = growFrac * roomScale * secBeat;
            float tSettle = settleFrac * roomScale * secBeat;

            float pop = Mathf.Lerp(minPop, maxPop, _intensity01) * Mathf.Max(1f, boost);

            if (logBeats && _beatCount <= 12)
                Debug.Log($"[VibePulse] StartPulse{(fromTimer ? " (TIMER)" : "")} pop={pop:0.00} " +
                          $"base={_baseScale:0.00} intensity={_intensity01:0.00} " +
                          $"tGrow={tGrow:0.000}s tSettle={tSettle:0.000}s " +
                          $"scaleNow={target.localScale.x:0.00}", this);

            _pulseCo = StartCoroutine(PulseRoutine(pop, tGrow, tSettle));
        }

        IEnumerator PulseRoutine(float popScale, float tGrow, float tSettle)
        {
            var rt = target;

            // grow (0→1 via curve). Base scale is re-read every frame so a magnitude
            // change mid-pop lands immediately instead of after the beat.
            float t = 0f;
            while (t < tGrow)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float k = tGrow <= 0 ? 1f : Mathf.Clamp01(t / tGrow);
                float c = popCurve.Evaluate(k);
                rt.localScale = Vector3.one * (_baseScale * Mathf.LerpUnclamped(1f, popScale, c));
                yield return null;
            }

            // settle (1→0 via curve reversed)
            t = 0f;
            while (t < tSettle)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float k = tSettle <= 0 ? 1f : Mathf.Clamp01(t / tSettle);
                float c = popCurve.Evaluate(1f - k);
                rt.localScale = Vector3.one * (_baseScale * Mathf.LerpUnclamped(1f, popScale, c));
                yield return null;
            }

            rt.localScale = Vector3.one * _baseScale; // ready for the next beat
            _pulseCo = null;
        }
    }
}
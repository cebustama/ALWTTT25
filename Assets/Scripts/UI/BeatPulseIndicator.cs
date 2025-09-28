// BeatPulseIndicator.cs
using ALWTTT.Music;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    /// Pops once per beat with a curve-shaped scale, and colors by Downbeat/Beat.
    public class BeatPulseIndicator : MidiListenerCanvasBase
    {
        [Header("Target")]
        [SerializeField] private Image target;                 // assign a circle Image
        [SerializeField] private bool useUnscaledTime = true;  // UI-friendly

        [Header("Pop Timing (fractions of one beat)")]
        [Tooltip("0..1 of beat length for grow (base → peak)")]
        [SerializeField, Range(0f, 1f)] private float growFrac = 0.25f;
        [Tooltip("0..1 of beat length for settle (peak → base)")]
        [SerializeField, Range(0f, 1f)] private float settleFrac = 0.50f;

        [Header("Scale")]
        [SerializeField] private float beatPopScale = 1.12f;

        [Header("Curve")]
        [Tooltip("Shape used for both phases. 0→1 for grow; reversed for settle.")]
        [SerializeField]
        private AnimationCurve popCurve =
            AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Colors")]
        [SerializeField] private Color beatColor = new(1f, 0.92f, 0.25f);
        [SerializeField] private Color downbeatColor = new(0.95f, 0.35f, 0.85f);

        // live tempo data (from ITempoSignatureListener)
        double _bpm = 120;
        int _den = 4;

        float SecPerBeat => (float)((60.0 / _bpm) * (4.0 / _den));

        Vector3 _baseScale;
        Coroutine _pulseCo;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!target) target = GetComponent<Image>();
            if (target) _baseScale = target.rectTransform.localScale;
        }

        protected override void OnTempoChanged(double bpm) { _bpm = bpm <= 0 ? 120 : bpm; }
        protected override void OnTimeSignatureChanged(int n, int d) { _den = Mathf.Max(1, d); }

        // Color is set for the entire beat; the pop runs under the beat length.
        protected override void OnBeat(BeatGridEvent e)
        {
            if (!target) return;
            target.color = beatColor;
            StartPulse();
        }

        protected override void OnDownbeat(BeatGridEvent e)
        {
            if (!target) return;
            target.color = downbeatColor;
            StartPulse();
        }

        // ─────────────────────────────────────────────────────────────────────

        void StartPulse()
        {
            if (!target) return;
            if (_pulseCo != null) StopCoroutine(_pulseCo);

            // keep total pop shorter than a beat (cap at ~90% of beat)
            float secBeat = Mathf.Max(0.01f, SecPerBeat);
            float totalFrac = Mathf.Clamp01(growFrac + settleFrac);
            if (totalFrac <= 0f) totalFrac = 0.75f;
            float roomScale = Mathf.Min(0.90f, totalFrac) / totalFrac;

            float tGrow = growFrac * roomScale * secBeat;
            float tSettle = settleFrac * roomScale * secBeat;

            _pulseCo = StartCoroutine(PulseRoutine(beatPopScale, tGrow, tSettle));
        }

        System.Collections.IEnumerator PulseRoutine(float popScale, float tGrow, float tSettle)
        {
            var rt = target.rectTransform;
            var dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            Vector3 from = _baseScale;
            Vector3 to = _baseScale * popScale;

            // grow (0→1 via curve)
            float t = 0f;
            while (t < tGrow)
            {
                t += dt;
                float k = tGrow <= 0 ? 1f : Mathf.Clamp01(t / tGrow);
                float c = popCurve.Evaluate(k);
                rt.localScale = Vector3.LerpUnclamped(from, to, c);
                yield return null;
            }

            // settle (1→0 via curve reversed)
            t = 0f;
            while (t < tSettle)
            {
                t += dt;
                float k = tSettle <= 0 ? 1f : Mathf.Clamp01(t / tSettle);
                float c = popCurve.Evaluate(1f - k);
                rt.localScale = Vector3.LerpUnclamped(from, to, c);
                yield return null;
            }

            rt.localScale = _baseScale; // ready for the next beat
            _pulseCo = null;
        }
    }
}

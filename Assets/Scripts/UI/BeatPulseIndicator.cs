// BeatPulseIndicator.cs
using ALWTTT.Music;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class BeatPulseIndicator : MidiListenerCanvasBase
    {
        [Header("Target")]
        [SerializeField] private Image target;             // assign a white circle
        [SerializeField] private bool useUnscaledTime = true;

        [Header("Colors")]
        [SerializeField] private Color beatColor = new(1f, 0.92f, 0.25f);
        [SerializeField] private Color downbeatColor = new(0.95f, 0.35f, 0.85f);

        [Header("Pop as fraction of a beat")]
        [Tooltip("0..1 fraction of beat length for grow-in")]
        [SerializeField] private float growFrac = 0.25f;
        [Tooltip("0..1 fraction of beat length for settle-back")]
        [SerializeField] private float settleFrac = 0.50f;
        [SerializeField] private float beatPopScale = 1.12f;
        [SerializeField] private float downPopScale = 1.22f;

        // live tempo / TS
        double _bpm = 120;
        int _den = 4; // we only need denominator to compute sec/beat
        float SecPerBeat => (float)((60.0 / _bpm) * (4.0 / _den));

        Vector3 _baseScale;
        Coroutine _pulseCo;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!target) return;
            _baseScale = target.rectTransform.localScale;
        }

        protected override void OnTempoChanged(double bpm) { _bpm = bpm <= 0 ? 120 : bpm; }
        protected override void OnTimeSignatureChanged(int n, int d) { _den = Mathf.Max(1, d); }

        protected override void OnBeat(BeatGridEvent e)
        {
            if (!target) return;
            // regular beat: set color now, pop with beat timings
            target.color = beatColor;
            StartPulse(beatPopScale);
        }

        protected override void OnDownbeat(BeatGridEvent e)
        {
            if (!target) return;
            target.color = downbeatColor;
            StartPulse(downPopScale);
        }

        void StartPulse(float popScale)
        {
            if (_pulseCo != null) StopCoroutine(_pulseCo);

            // ensure durations < 1 beat, default 0.75 beat total
            float secBeat = Mathf.Max(0.01f, SecPerBeat);
            float tGrow = Mathf.Clamp01(growFrac) * secBeat;
            float tSettle = Mathf.Clamp01(settleFrac) * secBeat;
            tGrow = Mathf.Min(tGrow, secBeat * 0.9f);
            tSettle = Mathf.Min(tSettle, secBeat - tGrow);

            _pulseCo = StartCoroutine(PulseRoutine(popScale, tGrow, tSettle));
        }

        System.Collections.IEnumerator PulseRoutine(float popScale, float tGrow, float tSettle)
        {
            var rt = target.rectTransform;
            var dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            // grow
            float t = 0f;
            var start = _baseScale;
            var peak = _baseScale * popScale;
            while (t < tGrow)
            {
                t += dt;
                float k = tGrow <= 0 ? 1f : Mathf.Clamp01(t / tGrow);
                rt.localScale = Vector3.LerpUnclamped(start, peak, k);
                yield return null;
            }

            // settle
            t = 0f;
            while (t < tSettle)
            {
                t += dt;
                float k = tSettle <= 0 ? 1f : Mathf.Clamp01(t / tSettle);
                rt.localScale = Vector3.LerpUnclamped(peak, _baseScale, k);
                yield return null;
            }

            rt.localScale = _baseScale;   // leave color as last set by the event
            _pulseCo = null;
        }
    }
}

using ALWTTT.Music;
using TMPro;
using UnityEngine;

namespace ALWTTT.UI
{
    /// <summary>
    /// Shows "BPM  /  Time Signature" and (optionally) pulses on beats.
    /// Put this next to BeatPulseIndicator under your BeatHUD.
    /// </summary>
    public class TempoTextIndicator : MidiListenerCanvasBase
    {
        [Header("UI")]
        [SerializeField] private TMP_Text label;

        [Header("Format")]
        [SerializeField] private string bpmFormat = "{0:0.#} BPM";
        [SerializeField] private string tsFormat = "{0}/{1}";
        [SerializeField] private string joiner = "   •   ";

        [Header("Optional beat pulse")]
        [SerializeField] private bool pulseOnBeat = true;
        [SerializeField] private float beatScale = 1.06f;
        [SerializeField] private float downbeatScale = 1.12f;
        [SerializeField] private float lerpOutTime = 0.15f;

        double _bpm = 120;
        int _num = 4, _den = 4;
        Vector3 _baseScale;
        float _pulseTimer;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (label != null) _baseScale = label.rectTransform.localScale;
            Refresh();
        }

        protected override void OnTempoChanged(double bpm)
        {
            _bpm = bpm;
            Refresh();
        }

        protected override void OnTimeSignatureChanged(int numerator, int denominator)
        {
            _num = numerator; _den = denominator;
            Refresh();
        }

        protected override void OnBeat(BeatGridEvent e)
        {
            if (!pulseOnBeat || label == null) return;
            // small pop each beat, bigger on downbeat (handled below)
            _pulseTimer = Mathf.Max(_pulseTimer, 0.0001f);
            var target = (e.beatInBar == 0) ? downbeatScale : beatScale;
            label.rectTransform.localScale = _baseScale * target;
        }

        void Update()
        {
            if (label == null || _pulseTimer <= 0f) return;
            _pulseTimer += Time.unscaledDeltaTime;
            // ease back to base scale
            float k = Mathf.Clamp01(_pulseTimer / lerpOutTime);
            label.rectTransform.localScale =
                Vector3.LerpUnclamped(label.rectTransform.localScale, _baseScale, k);
            if (k >= 1f) _pulseTimer = 0f;
        }

        void Refresh()
        {
            if (label == null) return;
            label.text = string.Format(bpmFormat, _bpm) + joiner +
                         string.Format(tsFormat, _num, _den);
        }
    }
}

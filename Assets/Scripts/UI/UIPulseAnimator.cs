using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    /// <summary>
    /// Reusable scale + color flash animator for any UI element.
    ///
    /// Used by B2 / #4 to pop the Inspiration markers (value text and +N badge)
    /// on every <see cref="SongCompositionUI.SetInspiration"/> /
    /// <see cref="SongCompositionUI.SetPlusInspiration"/> mutation.
    ///
    /// Wiring: attach to the RectTransform you want to pop, assign
    /// <see cref="target"/> (defaults to self) and optional <see cref="colorTarget"/>
    /// (TMP_Text/Image — anything <see cref="Graphic"/>). Designer-tuned curve.
    ///
    /// The animator captures its base scale + base color on first <see cref="Pulse()"/>
    /// (or in Awake if present), so it must be activated before any external
    /// transforms mutate localScale or color, or the base will be wrong. Restores
    /// base on completion.
    /// </summary>
    public class UIPulseAnimator : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField, Tooltip("Transform to scale. Defaults to this.transform.")]
        private RectTransform target;

        [SerializeField, Tooltip("Optional Graphic (TMP_Text counts) tinted during the pulse. " +
            "Leave null to skip color flash.")]
        private Graphic colorTarget;

        [Header("Pulse")]
        [SerializeField, Tooltip("Peak scale multiplier (1.25 = 25% bigger).")]
        private float popScale = 1.25f;

        [SerializeField, Tooltip("Grow phase duration (seconds).")]
        private float growDuration = 0.08f;

        [SerializeField, Tooltip("Settle phase duration (seconds).")]
        private float settleDuration = 0.18f;

        [SerializeField]
        private AnimationCurve curve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField, Tooltip("Use unscaled time (recommended for UI).")]
        private bool useUnscaledTime = true;

        private Vector3 _baseScale = Vector3.one;
        private Color _baseColor = Color.white;
        private bool _basesCaptured;
        private Coroutine _co;

        private void Awake()
        {
            if (target == null) target = transform as RectTransform;
            CaptureBases();
        }

        private void OnDisable()
        {
            // If we disable mid-pulse, restore bases so we don't leak a weird scale.
            if (_co != null)
            {
                StopCoroutine(_co);
                _co = null;
                if (target != null) target.localScale = _baseScale;
                if (colorTarget != null) colorTarget.color = _baseColor;
            }
        }

        private void CaptureBases()
        {
            if (_basesCaptured) return;
            if (target != null) _baseScale = target.localScale;
            if (colorTarget != null) _baseColor = colorTarget.color;
            _basesCaptured = true;
        }

        /// <summary>Pulse without color flash.</summary>
        public void Pulse() => PulseInternal(null);

        /// <summary>Pulse with a transient color tint on <see cref="colorTarget"/>.</summary>
        public void Pulse(Color flashColor) => PulseInternal(flashColor);

        private void PulseInternal(Color? flashColor)
        {
            if (!isActiveAndEnabled || target == null) return;
            CaptureBases();
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(PulseRoutine(flashColor));
        }

        private IEnumerator PulseRoutine(Color? flashColor)
        {
            var rt = target;
            Vector3 from = _baseScale;
            Vector3 to = _baseScale * popScale;
            bool tintEnabled = flashColor.HasValue && colorTarget != null;

            // Grow (0 → 1).
            float t = 0f;
            while (t < growDuration)
            {
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t += dt;
                float k = growDuration <= 0f ? 1f : Mathf.Clamp01(t / growDuration);
                float c = curve.Evaluate(k);
                rt.localScale = Vector3.LerpUnclamped(from, to, c);
                if (tintEnabled)
                    colorTarget.color = Color.LerpUnclamped(_baseColor, flashColor.Value, c);
                yield return null;
            }

            // Settle (1 → 0).
            t = 0f;
            while (t < settleDuration)
            {
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t += dt;
                float k = settleDuration <= 0f ? 1f : Mathf.Clamp01(t / settleDuration);
                float c = curve.Evaluate(1f - k);
                rt.localScale = Vector3.LerpUnclamped(from, to, c);
                if (tintEnabled)
                    colorTarget.color = Color.LerpUnclamped(_baseColor, flashColor.Value, c);
                yield return null;
            }

            rt.localScale = _baseScale;
            if (tintEnabled) colorTarget.color = _baseColor;
            _co = null;
        }
    }
}
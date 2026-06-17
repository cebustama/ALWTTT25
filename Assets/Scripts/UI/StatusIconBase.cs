using ALWTTT.Status;
using ALWTTT.Status.Runtime;
using ALWTTT.Tooltips;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    /// <summary>
    /// Prefab component mounted on each status icon instance created by
    /// <c>CharacterCanvas</c>.
    ///
    /// M1.8 (2026-04-20): gained appear / disappear popup animations.
    /// Called from <c>CharacterCanvas.TryCreateIcon</c> (PlayAppear) and
    /// <c>CharacterCanvas.HandleStatusCleared</c> (PlayDisappear). The
    /// disappear animation is terminal — the GameObject self-destroys when
    /// it completes. Scope is intentionally limited to appear/disappear
    /// only; change-flash on stack delta is out of scope for M1.8.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class StatusIconBase : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private Image statusImage;
        [SerializeField] private TextMeshProUGUI statusValueText;

        [Header("Appear / Disappear Animation (M1.8)")]
        [Tooltip("Duration of the appear popup, in seconds. " +
                 "Typical UI feedback sits around 0.2–0.4s; 1.0s is a calm default — " +
                 "tune down for snappier feel.")]
        [SerializeField, Min(0.01f)] private float appearDuration = 1f;

        [Tooltip("Duration of the disappear popup, in seconds. " +
                 "GameObject is destroyed after this completes.")]
        [SerializeField, Min(0.01f)] private float disappearDuration = 1f;

        [Tooltip("Scale envelope for appear. X = normalized time (0→1), Y = scale multiplier. " +
                 "Default overshoots slightly past 1 for a bouncy pop.")]
        [SerializeField]
        private AnimationCurve appearScaleCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.7f, 1.15f),
            new Keyframe(1f, 1f));

        [Tooltip("Scale envelope for disappear. X = normalized time (0→1), Y = scale multiplier. " +
                 "Default holds then shrinks, with a small overshoot early for snap.")]
        [SerializeField]
        private AnimationCurve disappearScaleCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.3f, 1.1f),
            new Keyframe(1f, 0f));

        [Tooltip("Alpha envelope for appear. 0 = fully transparent, 1 = fully opaque.")]
        [SerializeField]
        private AnimationCurve appearAlphaCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Alpha envelope for disappear.")]
        [SerializeField]
        private AnimationCurve disappearAlphaCurve =
            AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        public Sprite CurrentSprite { get; private set; }
        public Image StatusImage => statusImage;
        public TextMeshProUGUI StatusValueText => statusValueText;

        /// <summary>
        /// True once PlayDisappear has begun. Prevents reuse, repeated
        /// disappear calls, or PlayAppear being called on a doomed icon.
        /// </summary>
        public bool IsDisappearing { get; private set; }

        private CanvasGroup _canvasGroup;
        private Coroutine _activeAnimation;

        // M1.3: tooltip binding. Populated by CharacterCanvas.TryCreateIcon.
        private StatusEffectSO _definition;
        private StatusEffectContainer _container;
        private CharacterStatusId _boundId;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Assign the icon sprite for this status.
        /// Sprite is sourced from <c>StatusEffectSO.IconSprite</c> by <c>CharacterCanvas</c>.
        /// </summary>
        public void SetStatus(Sprite sprite)
        {
            CurrentSprite = sprite;
            if (statusImage != null)
                statusImage.sprite = sprite;
        }

        public void SetStatusValue(int statusValue)
        {
            if (statusValueText != null)
                statusValueText.text = statusValue.ToString();
        }

        /// <summary>
        /// Play the appear popup. No-op if this icon is already disappearing.
        /// Interrupts any in-flight appear animation and restarts from frame 0.
        /// </summary>
        /// <remarks>
        /// Sets the starting visual state (scale 0, alpha 0 by default)
        /// synchronously before starting the coroutine, so the first rendered
        /// frame after instantiation is the animation start — no flash of a
        /// fully-visible icon before the animation begins.
        /// </remarks>
        public void PlayAppear()
        {
            if (IsDisappearing) return;
            if (_activeAnimation != null) StopCoroutine(_activeAnimation);

            // Synchronous initial state — first rendered frame is correct.
            transform.localScale = Vector3.one * appearScaleCurve.Evaluate(0f);
            if (_canvasGroup != null)
                _canvasGroup.alpha = Mathf.Clamp01(appearAlphaCurve.Evaluate(0f));

            _activeAnimation = StartCoroutine(AppearRoutine());
        }

        /// <summary>
        /// Play the disappear popup and destroy this GameObject when the
        /// animation completes. Idempotent: subsequent calls are ignored
        /// while the disappear is in flight.
        /// </summary>
        public void PlayDisappear()
        {
            if (IsDisappearing) return;
            IsDisappearing = true;

            if (_activeAnimation != null) StopCoroutine(_activeAnimation);
            _activeAnimation = StartCoroutine(DisappearAndDestroyRoutine());
        }

        private IEnumerator AppearRoutine()
        {
            float t = 0f;
            while (t < appearDuration)
            {
                float p = t / appearDuration;
                transform.localScale = Vector3.one * appearScaleCurve.Evaluate(p);
                if (_canvasGroup != null)
                    _canvasGroup.alpha = Mathf.Clamp01(appearAlphaCurve.Evaluate(p));
                t += Time.deltaTime;
                yield return null;
            }

            // Clean final state.
            transform.localScale = Vector3.one;
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            _activeAnimation = null;
        }

        private IEnumerator DisappearAndDestroyRoutine()
        {
            float t = 0f;
            while (t < disappearDuration)
            {
                float p = t / disappearDuration;
                transform.localScale = Vector3.one * disappearScaleCurve.Evaluate(p);
                if (_canvasGroup != null)
                    _canvasGroup.alpha = Mathf.Clamp01(disappearAlphaCurve.Evaluate(p));
                t += Time.deltaTime;
                yield return null;
            }

            if (gameObject != null)
                Destroy(gameObject);
        }

        /// <summary>
        /// M1.3: wire this icon to its backing status for tooltip lookup.
        /// Called by CharacterCanvas right after SetStatus and before PlayAppear.
        /// </summary>
        public void BindTooltipSource(
            StatusEffectSO definition,
            StatusEffectContainer container,
            CharacterStatusId id)
        {
            _definition = definition;
            _container = container;
            _boundId = id;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_definition == null) return;
            var tm = TooltipManager.Instance;
            if (tm == null) return;

            int stacks = _container != null ? _container.GetStacks(_boundId) : 0;
            var header = stacks > 1
                ? $"{_definition.DisplayName} ×{stacks}"
                : _definition.DisplayName;
            var body = string.IsNullOrWhiteSpace(_definition.Description)
                ? string.Empty
                : _definition.Description;

            tm.ShowTooltip(body, header, transform, cam: null);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var tm = TooltipManager.Instance;
            if (tm != null) tm.HideTooltip();
        }
    }
}
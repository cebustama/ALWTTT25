using ALWTTT.Cards;
using System.Collections;
using UnityEngine;

namespace ALWTTT.UI
{
    /// <summary>
    /// Singleton controller for hover-preview of composition-card minicards
    /// over track-row labels in <see cref="SongTrackElementUI"/>.
    ///
    /// Scene wiring (B2 / #3):
    /// - <see cref="follower"/>: RectTransform that follows mouse (root of preview).
    /// - <see cref="minicard"/>: scene-resident <see cref="CardUI"/> instance (scaled
    ///   smaller than a hand card; rescaled per designer preference).
    /// - <see cref="canvasGroup"/>: fade-in target.
    /// - <see cref="canvasRect"/>: parent canvas, used for edge-clamp.
    ///
    /// Pattern mirrors <see cref="ALWTTT.Tooltips.TooltipController"/> but renders
    /// a populated <see cref="CardUI"/> instead of <see cref="TooltipText"/>. The
    /// minicard reuses <see cref="CardBase.SetCard"/> with <c>isPlayable=false</c>;
    /// no card behavior runs (no pointer handlers fire because the host row
    /// receives the events, and the minicard sits over the cursor purely visual).
    ///
    /// Not a Tooltip in the SpecialKeyword / StatusEffect sense — those flow
    /// through <see cref="ALWTTT.Tooltips.TooltipManager"/> with text payloads.
    /// </summary>
    public class MinicardTooltipController : MonoBehaviour
    {
        public static MinicardTooltipController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private RectTransform follower;
        [SerializeField] private CardUI minicard;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform canvasRect;

        [Header("Behavior")]
        [SerializeField, Tooltip("Delay before the preview fades in (seconds).")]
        private float showDelaySec = 0.2f;

        [SerializeField, Tooltip("Pixel offset from the cursor (canvas-local).")]
        private Vector2 cursorOffset = new Vector2(20f, 20f);

        [SerializeField, Tooltip("Fade in/out duration. 0 = instant.")]
        private float fadeDuration = 0.1f;

        private Coroutine _fadeRoutine;

        #region Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            HideImmediate();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (follower == null || canvasRect == null) return;
            if (minicard == null || !minicard.gameObject.activeSelf) return;

            // Follow mouse with canvas-local clamp (mirrors TooltipController.SetPosition).
            Vector2 mouse = Input.mousePosition;
            Vector2 pos = mouse / Mathf.Max(0.0001f, canvasRect.localScale.x);
            pos += cursorOffset;

            // pivot (0, 1):
            var size = follower.rect.size;
            // X: pivot at left edge → pos.x is the left edge.
            if (pos.x + size.x > canvasRect.rect.width)
                pos.x = canvasRect.rect.width - size.x;
            if (pos.x < 0f) pos.x = 0f;
            // Y: pivot at top edge → pos.y is the top edge.
            if (pos.y > canvasRect.rect.height)
                pos.y = canvasRect.rect.height;
            if (pos.y < size.y) pos.y = size.y;

            follower.anchoredPosition = pos;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show the minicard preview populated from <paramref name="card"/>.
        /// Re-entrant: calling with a new card swaps the preview content.
        /// </summary>
        public void Show(CardDefinition card)
        {
            if (card == null) { Hide(); return; }
            if (minicard == null) return;

            minicard.gameObject.SetActive(true);
            minicard.SetCard(card, isPlayable: false);

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeInRoutine());
        }

        /// <summary>
        /// Hide the minicard preview. Safe to call repeatedly.
        /// </summary>
        public void Hide()
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }
            HideImmediate();
        }

        #endregion

        #region Internal

        private void HideImmediate()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (minicard != null) minicard.gameObject.SetActive(false);
        }

        private IEnumerator FadeInRoutine()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;

            // Show delay
            float delay = Mathf.Max(0f, showDelaySec);
            float t = 0f;
            while (t < delay)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // Fade in
            float fade = Mathf.Max(0f, fadeDuration);
            if (fade <= 0f || canvasGroup == null)
            {
                if (canvasGroup != null) canvasGroup.alpha = 1f;
                _fadeRoutine = null;
                yield break;
            }

            t = 0f;
            while (t < fade)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fade);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            _fadeRoutine = null;
        }

        #endregion
    }
}
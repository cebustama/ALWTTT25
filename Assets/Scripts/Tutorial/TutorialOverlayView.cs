using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// Spotlight placement spec built by TutorialController and consumed by the overlay.
    /// Either centre+auto-size from <see cref="Target"/>, or place at an explicit viewport
    /// centre; size can be auto (from target) or overridden per-axis for a circle/oval.
    /// </summary>
    public struct Spotlight
    {
        public bool Enabled;
        public Sprite HoleShape;

        // [TUT-R3] Fuente world-space (proyectada con WorldCamera). Precede a Target.
        public Transform WorldTarget;
        public Camera WorldCamera;
        public Bounds WorldBounds;
        public bool HasWorldBounds;

        /// <summary>Centre + auto-size source. Null = use <see cref="ManualCenterVp"/>.</summary>
        public RectTransform Target;
        /// <summary>Viewport centre, 0..1, origin bottom-left (used when Target is null).</summary>
        public Vector2 ManualCenterVp;
        /// <summary>Half-extents as a fraction of the smaller screen dimension. Equal x/y =
        /// circle, x≠y = oval. (0,0) = auto-size from the target.</summary>
        public Vector2 ManualRadiusFrac;

        public static Spotlight None => new Spotlight { Enabled = false };
    }

    /// <summary>
    /// [S4 R1 + R2 / D-TUT-4 / D3=B / D4=A] Dumb presentation view for one tutorial
    /// dialog. Owns no queue/fired-state logic — the controller drives it via
    /// <see cref="Show"/> and is called back on completion.
    ///
    /// R1 spotlight: a full-screen dark Image rendered with an instance of the
    /// ALWTTT/UI/TutorialSpotlight shader; the hole is centred on a RectTransform OR an
    /// explicit viewport point (Screen-Space-Overlay → camera = null).
    /// R2 bubble: captain portrait + message panel. Auto-place-opposite (D4) is opt-in
    /// via autoPlaceBubbleBySide; default is a fixed bottom-left, upright captain (D7).
    ///
    /// Scene wiring (set in the GigCanvas prefab):
    ///   - overlayImage: full-screen stretched Image, Raycast Target ON, material =
    ///     the TutorialSpotlight material (an instance is made at runtime).
    ///   - canvasGroup: on the overlay root, for show/hide.
    ///   - bubbleRoot: container for captainImage + panel + messageText + skipButton.
    ///   - skipButton: dismiss-immediately button (top-right of the panel).
    /// </summary>
    public class TutorialOverlayView : MonoBehaviour, IPointerClickHandler
    {
        [Header("Spotlight (R1)")]
        [SerializeField] private Image overlayImage;
        [SerializeField] private Sprite defaultHoleShape;
        [Tooltip("Keep the spotlight circular regardless of the target's aspect. " +
                 "Off = the hole matches the target rect (can look oval on wide elements).")]
        [SerializeField] private bool keepHoleCircular = true;
        [Tooltip("Max spotlight radius as a fraction of the smaller screen dimension. " +
                 "Stops a large target (e.g. a full panel) from carving out the whole overlay.")]
        [SerializeField] private float maxHoleRadiusFraction = 0.22f;

        [Header("Visibility")]
        [Tooltip("CanvasGroup on the overlay root. Hidden via alpha + blocksRaycasts " +
                 "instead of SetActive, so child graphics stay built and render reliably " +
                 "on the first modal of a burst.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Bubble (R2)")]
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform bubbleRoot;
        [SerializeField] private Image captainImage;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button skipButton;

        [Tooltip("Bottom-LEFT anchored position for the bubble (D7 default).")]
        [SerializeField] private Vector2 bubbleLeftPos = new Vector2(40f, 40f);
        [Tooltip("Bottom-RIGHT anchored position, used only when Auto Place Bubble By Side is on.")]
        [SerializeField] private Vector2 bubbleRightPos = new Vector2(-40f, 40f);
        [Tooltip("D4 'place opposite the spotlight + mirror the captain'. OFF by default: " +
                 "the captain flip pushes a center/left-pivoted portrait off-screen, and the " +
                 "right position assumes a right-anchored bubble. Keep off until those are set up.")]
        [SerializeField] private bool autoPlaceBubbleBySide = false;

        [Header("Reveal")]
        [Tooltip("Characters per second for the typed reveal (unscaled).")]
        [SerializeField] private float typeSpeed = 45f;

        [Header("Debug")]
        [SerializeField] private bool verboseLogging = true;
        private const string DebugTag = "<color=#ffd479>[TutorialOverlay]</color>";
        private void Log(string m) { if (verboseLogging) Debug.Log($"{DebugTag} {m}"); }

        private Material _matInstance;
        private Action _onComplete;
        private string[] _pages;
        private int _pageIndex;
        private Coroutine _typeRoutine;
        private bool _isTyping;

        public bool IsShowing { get; private set; }

        private void Awake()
        {
            // Own material instance so we never mutate the shared asset.
            if (overlayImage != null && overlayImage.material != null)
            {
                _matInstance = new Material(overlayImage.material);
                overlayImage.material = _matInstance;
            }
            if (skipButton != null)
                skipButton.onClick.AddListener(CompleteNow);

            // Hide via CanvasGroup, NOT SetActive — the GameObject stays active so all
            // child graphics stay built and render immediately when shown.
            SetGroupVisible(false);
        }

        private void SetGroupVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = visible;
                canvasGroup.interactable = visible;
            }
            else
            {
                if (visible) Log("NO CanvasGroup wired — using SetActive fallback. " +
                                  "Add a CanvasGroup to the overlay + assign it to remove the transition gap.");
                gameObject.SetActive(visible);
            }
        }

        private void OnDestroy()
        {
            if (skipButton != null) skipButton.onClick.RemoveListener(CompleteNow);
            if (_matInstance != null) Destroy(_matInstance);
        }

        /// <summary>Show one dialog with an optional spotlight spec.</summary>
        public void Show(
            TutorialDialogSO dialog,
            Spotlight spotlight,
            Sprite portrait,
            Action onComplete)
        {
            if (dialog == null) { onComplete?.Invoke(); return; }

            _onComplete = onComplete;
            _pages = (dialog.Pages != null && dialog.Pages.Count > 0)
                ? new string[dialog.Pages.Count]
                : new[] { string.Empty };
            for (int i = 0; i < _pages.Length && i < dialog.Pages.Count; i++)
                _pages[i] = TutorialTokenResolver.Resolve(dialog.Pages[i]); // [TUT-R2 / D8]

            _pageIndex = 0;
            IsShowing = true;

            SetGroupVisible(true);

            // Configure the portrait on the (always-active) object.
            if (captainImage != null)
            {
                var p = portrait != null ? portrait : dialog.Portrait;
                captainImage.sprite = p;
                captainImage.enabled = p != null;
                captainImage.SetAllDirty();
            }

            ApplySpotlight(spotlight);
            PlaceBubble(spotlight);
            ShowPage(_pageIndex);

            // Force the just-set graphics to rebuild THIS frame so the captain renders on
            // the first modal of a burst (the visibility-transition show), not only on
            // chained ones. Belt-and-suspenders with the CanvasGroup approach.
            Canvas.ForceUpdateCanvases();

            if (captainImage != null)
                Log($"Show '{dialog.TriggerId}': sprite='{(captainImage.sprite != null ? captainImage.sprite.name : "NULL")}' " +
                    $"compEnabled={captainImage.enabled} goActive={captainImage.gameObject.activeInHierarchy} " +
                    $"colorA={captainImage.color.a:F2} scaleX={captainImage.rectTransform.localScale.x:F2}");
        }

        public void Hide()
        {
            IsShowing = false;
            if (_typeRoutine != null) { StopCoroutine(_typeRoutine); _typeRoutine = null; }
            if (_matInstance != null) _matInstance.SetFloat("_HoleEnabled", 0f);
            SetGroupVisible(false);
        }

        // ---- Input: click anywhere advances (reveal-all → next page → complete) ----
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsShowing) return;
            Advance();
        }

        private void Advance()
        {
            if (_isTyping)
            {
                // First click completes the reveal of the current page.
                RevealAllNow();
                return;
            }
            _pageIndex++;
            if (_pageIndex >= _pages.Length) CompleteNow();
            else ShowPage(_pageIndex);
        }

        private void CompleteNow()
        {
            if (!IsShowing) return;
            var cb = _onComplete;
            _onComplete = null;
            // Controller decides whether to hide or chain the next dialog.
            cb?.Invoke();
        }

        // ---- Paging + typed reveal ----
        private void ShowPage(int index)
        {
            if (messageText == null) return;
            string text = (index >= 0 && index < _pages.Length) ? _pages[index] : string.Empty;
            messageText.text = text;
            messageText.ForceMeshUpdate();
            int total = messageText.textInfo.characterCount;

            if (_typeRoutine != null) StopCoroutine(_typeRoutine);
            _typeRoutine = StartCoroutine(TypeRoutine(total));
        }

        private IEnumerator TypeRoutine(int totalChars)
        {
            _isTyping = true;
            messageText.maxVisibleCharacters = 0;
            float shown = 0f;
            while (shown < totalChars)
            {
                shown += typeSpeed * Time.unscaledDeltaTime;
                messageText.maxVisibleCharacters = Mathf.Clamp((int)shown, 0, totalChars);
                yield return null;
            }
            messageText.maxVisibleCharacters = totalChars;
            _isTyping = false;
            _typeRoutine = null;
        }

        private void RevealAllNow()
        {
            if (_typeRoutine != null) { StopCoroutine(_typeRoutine); _typeRoutine = null; }
            if (messageText != null)
                messageText.maxVisibleCharacters = messageText.textInfo.characterCount;
            _isTyping = false;
        }

        // ---- R1 spotlight ----
        private void ApplySpotlight(Spotlight s)
        {
            if (_matInstance == null) return;

            if (!s.Enabled || s.HoleShape == null)
            {
                _matInstance.SetFloat("_HoleEnabled", 0f);
                return;
            }

            _matInstance.SetTexture("_HoleTex", s.HoleShape.texture);

            float w = Mathf.Max(1f, Screen.width);
            float h = Mathf.Max(1f, Screen.height);
            float minDim = Mathf.Min(w, h);

            // ---- centre: from the target rect, or an explicit viewport point ----
            Vector2 centerVp;
            float autoHalfPxX = 0f, autoHalfPxY = 0f;
            bool haveAuto = false;
            string srcDesc;
            if (s.WorldTarget != null)
            {
                var cam = s.WorldCamera != null ? s.WorldCamera : Camera.main;
                if (cam != null && s.HasWorldBounds)
                {
                    var b = s.WorldBounds; Vector3 c = b.center, e = b.extents;
                    float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
                    bool any = false;
                    for (int i = 0; i < 8; i++)
                    {
                        var corner = c + new Vector3((i & 1) == 0 ? -e.x : e.x,
                                                     (i & 2) == 0 ? -e.y : e.y,
                                                     (i & 4) == 0 ? -e.z : e.z);
                        var sp = cam.WorldToScreenPoint(corner);
                        if (sp.z <= 0f) continue;                 // detrás de la cámara
                        any = true;
                        if (sp.x < minX) minX = sp.x; if (sp.x > maxX) maxX = sp.x;
                        if (sp.y < minY) minY = sp.y; if (sp.y > maxY) maxY = sp.y;
                    }
                    if (any)
                    {
                        centerVp = new Vector2(((minX + maxX) * 0.5f) / w, ((minY + maxY) * 0.5f) / h);
                        autoHalfPxX = Mathf.Abs(maxX - minX) * 0.5f;
                        autoHalfPxY = Mathf.Abs(maxY - minY) * 0.5f;
                        haveAuto = true;
                    }
                    else { var sp = cam.WorldToScreenPoint(s.WorldTarget.position); centerVp = new Vector2(sp.x / w, sp.y / h); }
                }
                else if (cam != null)
                {
                    var sp = cam.WorldToScreenPoint(s.WorldTarget.position);
                    centerVp = new Vector2(sp.x / w, sp.y / h);
                }
                else { centerVp = s.ManualCenterVp; }             // sin cámara → degradar a manual
                srcDesc = $"world='{s.WorldTarget.name}'";
            }
            else if (s.Target != null)
            {
                var corners = new Vector3[4];
                s.Target.GetWorldCorners(corners);
                Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
                Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
                centerVp = new Vector2(((min.x + max.x) * 0.5f) / w, ((min.y + max.y) * 0.5f) / h);
                autoHalfPxX = Mathf.Abs(max.x - min.x) * 0.5f;
                autoHalfPxY = Mathf.Abs(max.y - min.y) * 0.5f;
                haveAuto = true;
                srcDesc = $"target='{s.Target.name}'";
            }
            else { centerVp = s.ManualCenterVp; srcDesc = $"manual centre=({centerVp.x:F2},{centerVp.y:F2})"; }

            // ---- size: explicit per-axis override, or auto from the target ----
            float halfPxX, halfPxY;
            bool manualSize = s.ManualRadiusFrac.x > 0f || s.ManualRadiusFrac.y > 0f;
            if (manualSize)
            {
                // Half-extents as a fraction of the smaller screen dimension (resolution-
                // independent). Equal x/y = circle; x≠y = oval. Setting only one axis
                // mirrors it to the other so a single value still gives a circle.
                halfPxX = s.ManualRadiusFrac.x * minDim;
                halfPxY = s.ManualRadiusFrac.y * minDim;
                if (halfPxX <= 0f) halfPxX = halfPxY;
                if (halfPxY <= 0f) halfPxY = halfPxX;
            }
            else if (haveAuto)
            {
                if (keepHoleCircular)
                {
                    float r = Mathf.Max(autoHalfPxX, autoHalfPxY) * 1.3f;
                    halfPxX = r; halfPxY = r;
                }
                else { halfPxX = autoHalfPxX * 1.15f; halfPxY = autoHalfPxY * 1.15f; }

                // Clamp ONLY auto-sizing (a large target shouldn't carve the whole overlay).
                // Manual sizes are the author's deliberate choice and bypass the clamp.
                float maxPx = minDim * Mathf.Max(0.05f, maxHoleRadiusFraction);
                halfPxX = Mathf.Min(halfPxX, maxPx);
                halfPxY = Mathf.Min(halfPxY, maxPx);
            }
            else
            {
                // Manual centre but no size given → sensible default radius.
                halfPxX = halfPxY = minDim * 0.12f;
            }

            Vector2 halfVp = new Vector2(halfPxX / w, halfPxY / h);
            float rot = (s.Target != null) ? -s.Target.eulerAngles.z * Mathf.Deg2Rad : 0f;

            _matInstance.SetVector("_HoleCenter", new Vector4(centerVp.x, centerVp.y, 0, 0));
            _matInstance.SetVector("_HoleHalfSize", new Vector4(halfVp.x, halfVp.y, 0, 0));
            _matInstance.SetFloat("_HoleRotation", rot);
            _matInstance.SetFloat("_HoleEnabled", 1f);

            Log($"spotlight {srcDesc} centreVp=({centerVp.x:F2},{centerVp.y:F2}) " +
                $"radiusPx=({halfPxX:F0}x{halfPxY:F0}) {(manualSize ? "manual" : "auto")}");
        }

        // ---- R2 bubble placement ----
        private void PlaceBubble(Spotlight spot)
        {
            if (bubbleRoot == null) return;

            // Default (D7): fixed bottom-left, captain UPRIGHT. The D4 "place opposite +
            // mirror" behaviour is opt-in — its captain flip pushes a center/left-pivoted
            // portrait off-screen, and bubbleRightPos assumes a right-anchored bubble.
            // Re-enable once the captain pivot/anchor + right-side anchor are set up.
            if (!autoPlaceBubbleBySide)
            {
                bubbleRoot.anchoredPosition = bubbleLeftPos;
                if (captainImage != null)
                {
                    var s = captainImage.rectTransform.localScale;
                    s.x = Mathf.Abs(s.x);            // never leave the captain flipped
                    captainImage.rectTransform.localScale = s;
                }
                return;
            }

            bool spotlightOnLeft = false;
            if (spot.Enabled)
            {
                float cxVp;
                if (spot.Target != null)
                {
                    var corners = new Vector3[4];
                    spot.Target.GetWorldCorners(corners);
                    float cxPx = (RectTransformUtility.WorldToScreenPoint(null, corners[0]).x +
                                  RectTransformUtility.WorldToScreenPoint(null, corners[2]).x) * 0.5f;
                    cxVp = cxPx / Mathf.Max(1f, Screen.width);
                }
                else cxVp = spot.ManualCenterVp.x;
                spotlightOnLeft = cxVp < 0.5f;
            }

            bool placeRight = spotlightOnLeft;
            bubbleRoot.anchoredPosition = placeRight ? bubbleRightPos : bubbleLeftPos;

            if (captainImage != null)
            {
                var s = captainImage.rectTransform.localScale;
                s.x = Mathf.Abs(s.x) * (placeRight ? -1f : 1f);
                captainImage.rectTransform.localScale = s;
            }
        }
    }
}
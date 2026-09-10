// Place at: Assets/Scripts/UI/Tooltips/VibeReadoutTooltipTarget.cs
using UnityEngine;
using UnityEngine.EventSystems;

namespace ALWTTT.Tooltips
{
    /// <summary>
    /// [BIGNUM-1r / D-BN-13=A] Hover tooltip on the big C1 Vibe number: the band-wide
    /// chain (Hype % → L, + SFX = N, and what one average member can take). Content is
    /// pushed by GigCanvas.SetVibeReadout at every projection refresh. Requires
    /// raycastTarget ON on the label (TMP → Extra Settings). Pattern: EconPipTooltipTarget.
    ///
    /// [BIGNUM-1r6 / D-BN-18=B″ — F-BN-8 + F-BN-9] POSITIONING, and why it is done this way.
    ///
    /// F-BN-8: TooltipController's static-target branch runs, every frame,
    ///     followPos = Camera.main.WorldToScreenPoint(target.position)
    /// and then clamps the result to the canvas rect. That is right for the per-character
    /// canvases (world space) and wrong for a target parented under a Screen Space -
    /// Overlay canvas: an overlay RectTransform's .position is already in pixels, so the
    /// projection returns a huge coordinate and the clamp pins the panel to the top-right
    /// corner — which is what we kept seeing.
    ///
    /// F-BN-9: the obvious fix (anchor on an empty in the world) cannot be wired in the
    /// Inspector. GigCanvas lives in the persistent ALWTTTCore scene, loaded additively by
    /// CoreLoader; a world anchor lives in the gig scene. Unity does not serialize
    /// cross-scene references, so the slot can never be filled by hand.
    ///
    /// Therefore the anchor is CREATED AT RUNTIME and owned by this component. Each time
    /// the tooltip is shown, the anchor is parked at the world point that projects back to
    /// the screen position we want: number's own screen position + a pixel offset. The
    /// round trip WorldToScreenPoint(ScreenToWorldPoint(p)) == p holds for both projection
    /// types, so the controller lands the panel exactly where the offset says, using its
    /// existing math and with no change to any shared file. The general fix — screen-space
    /// anchors in TooltipController — belongs to TIP-1.
    /// </summary>
    public class VibeReadoutTooltipTarget : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Placement")]
        [Tooltip("Pixel offset from the number's own screen position. Negative X = left, " +
                 "negative Y = down. Tunable in Play: the panel follows while hovered.")]
        [SerializeField] private Vector2 screenOffset = new Vector2(-260f, -60f);

        [Tooltip("Distance in front of the camera at which the runtime anchor is parked. " +
                 "Only needs to be > near clip; the screen round-trip is exact at any " +
                 "valid depth, and for an orthographic camera it is irrelevant.")]
        [SerializeField] private float anchorDepth = 10f;

        [Tooltip("OPTIONAL explicit WORLD-SPACE anchor. Normally left empty — see F-BN-9: " +
                 "it cannot be assigned across scenes. Kept for a future setup where the " +
                 "canvas and the anchor share a scene.")]
        [SerializeField] private Transform anchorOverride;

        private Transform _runtimeAnchor;
        private string _header;
        private string _body;
        private bool _hovered;

        private void Awake()
        {
            if (anchorOverride == null) return;

            // [F-BN-8] Guard the one mistake this component exists to avoid.
            var canvas = anchorOverride.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                Debug.LogWarning(
                    $"[VibeReadoutTooltipTarget] anchorOverride '{anchorOverride.name}' sits " +
                    "under a Screen Space - Overlay canvas. TooltipController projects it " +
                    "through Camera.main, so the panel will clamp to the screen corner. " +
                    "Leave the override empty and use screenOffset instead (F-BN-8 / TIP-1).",
                    this);
        }

        private void OnDestroy()
        {
            if (_runtimeAnchor != null) Destroy(_runtimeAnchor.gameObject);
        }

        public void SetContent(string header, string body)
        {
            _header = header;
            _body = body;
            if (_hovered) Refresh(reopening: true);
        }

        public void Clear()
        {
            _header = null;
            _body = null;
            if (_hovered) TooltipManager.Instance?.HideTooltip();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            Refresh();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            TooltipManager.Instance?.HideTooltip();
        }

        private void Update()
        {
            // Keeps the panel glued while hovered: the number rescales with magnitude and
            // screenOffset is meant to be tuned live in the Inspector during Play.
            if (_hovered) PlaceAnchor();
        }

        private Transform ResolveAnchor()
        {
            if (anchorOverride != null) return anchorOverride;

            if (_runtimeAnchor == null)
            {
                // [F-BN-9] Not parented to anything: parenting it under the overlay canvas
                // would put it back in pixel space, and parenting it into the gig scene
                // would make it die on scene unload. DontDestroyOnLoad matches the
                // lifetime of ALWTTTCore, where this canvas lives.
                var go = new GameObject("VibeReadoutTooltipAnchor (runtime)");
                DontDestroyOnLoad(go);
                _runtimeAnchor = go.transform;
            }

            return _runtimeAnchor;
        }

        private void PlaceAnchor()
        {
            if (anchorOverride != null) return; // caller owns its placement

            var cam = Camera.main;
            if (cam == null) return; // TooltipController falls back to mouse-follow

            // This object lives under a Screen Space - Overlay canvas, so transform.position
            // is ALREADY in screen pixels (that is the whole of F-BN-8). Offset it, then
            // convert to the world point that projects back to exactly that pixel.
            Vector3 screen = transform.position;
            screen.x += screenOffset.x;
            screen.y += screenOffset.y;
            screen.z = anchorDepth;

            ResolveAnchor().position = cam.ScreenToWorldPoint(screen);
        }

        private void Refresh(bool reopening = false)
        {
            var tm = TooltipManager.Instance;
            if (tm == null || string.IsNullOrEmpty(_body)) return;

            PlaceAnchor();

            // [BIGNUM-1r4 / F-BN-7] TooltipManager.ShowTooltip ACCUMULATES: it bumps
            // currentShownTooltipCount and activates one more TooltipText per call, and
            // only HideTooltip resets the counter. A live-updating surface must close
            // before it reopens or it stacks one panel per refresh (observed: two panels,
            // 16 % and 19 %, one per loop). Closing first replays the 0.5 s fade-in;
            // that is the price of a manager with no "update in place" entry point.
            if (reopening) tm.HideTooltip();

            tm.ShowTooltip(_body, _header, ResolveAnchor(), cam: null);
        }
    }
}
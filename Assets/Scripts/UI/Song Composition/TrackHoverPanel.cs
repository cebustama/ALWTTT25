// Placement: Assets/Scripts/UI/Song Composition/TrackHoverPanel.cs  (NEW)
//
// [HUD-COMP-1 / §5] The strip's hover contract: everything the rest state
// encodes as icon/pip/shape shows up here as plain text.
//
// WHY IT ANCHORES TO THE ROW AND NOT THE CURSOR:
// the existing minicard follows the pointer, which is right for cards in hand
// (they move) and wrong here (rows do not). A cursor-following panel over a
// static strip drifts into the hand and the audience — the two zones the player
// must keep reading while hovering. Anchoring to the row means the panel opens
// in one predictable place, and we can CLAMP it against the two forbidden
// zones instead of hoping the cursor stays out of them.
//
// Safe zone (reference canvas 1920x1080, see spec §5):
//   - never below minBottomY (keeps the hand strip readable)
//   - never right of maxRightX (keeps the audience third readable)
// Clamps are canvas-local and CENTER-origin (0 = mid-screen), because that is
// what canvasRect.InverseTransformPoint returns.
//
// Text uses ASCII separators only: the LiberationSans SDF atlas ships a
// Latin-1 character set, so U+00B7 (mid dot) renders but U+2014 (em dash)
// comes out as a missing-glyph box.

using System.Text;
using TMPro;
using UnityEngine;

namespace ALWTTT.UI
{
    public class TrackHoverPanel : MonoBehaviour
    {
        public static TrackHoverPanel Instance { get; private set; }

        [SerializeField] private RectTransform panel;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text bodyText;

        [Header("Placement")]
        [SerializeField, Tooltip("Horizontal gap from the strip's right edge.")]
        private float anchorGapX = 32f;
        [SerializeField, Tooltip("Canvas-local Y (CENTER-origin: 0 is mid-screen, " +
                 "-540 the bottom on a 1080-tall canvas). Keeps the panel above the hand strip.")]
        private float minBottomY = -300f;
        [SerializeField, Tooltip("Canvas-local X (CENTER-origin: 0 is mid-screen, " +
                 "+960 the right edge on a 1920-wide canvas). Keeps the panel out of the audience third.")]
        private float maxRightX = 300f;
        [SerializeField] private float fadeDuration = 0.12f;

        private float _targetAlpha;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            // [HUD-COMP-1 fix] `panel` MUST be this object's own RectTransform.
            // PlaceNextTo writes an anchoredPosition expressed in canvasRect
            // (center-origin) space, and anchoredPosition is relative to the
            // PARENT. This object is the direct child of the canvas; a child of
            // it is not, so pointing `panel` at the visual background would add
            // a second offset and park the panel near screen centre.
            var self = transform as RectTransform;
            if (panel != self)
            {
                if (panel != null)
                    Debug.LogWarning($"[TrackHoverPanel] `panel` pointed at " +
                        $"'{panel.name}'; it must be this object's own RectTransform. " +
                        "Overriding. Clear the field in the inspector.");
                panel = self;
            }
            SetAlphaImmediate(0f);
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        #region Public API

        public void ShowForTrack(SongTrackElementUI.RowData d, RectTransform anchor)
            => ShowRaw(BuildTrackText(d), anchor);

        public void ShowRaw(string body, RectTransform anchor)
        {
            if (bodyText) bodyText.text = body;
            PlaceNextTo(anchor);
            _targetAlpha = 1f;
            if (canvasGroup) canvasGroup.blocksRaycasts = false; // never steals hover
        }

        public void Hide() => _targetAlpha = 0f;

        #endregion

        #region Text

        private static string BuildTrackText(SongTrackElementUI.RowData d)
        {
            var sb = new StringBuilder();

            if (d.placeholder)
            {
                sb.Append(string.IsNullOrEmpty(d.musicianName) ? d.musicianId : d.musicianName);
                sb.Append(" · no track yet");
                return sb.ToString();
            }

            sb.Append(string.IsNullOrEmpty(d.musicianName) ? d.musicianId : d.musicianName);
            sb.Append(" · ").Append(d.role);
            if (d.pending) sb.Append(" · applies next loop");
            sb.AppendLine();

            sb.Append(d.info);
            // Lv1 is not printed: "Lv 1 / 3" reads as a deficiency on a track
            // that is simply normal. Only a real level prints. (D2 / D8=B)
            if (d.level >= 2)
            {
                sb.Append($" · Lv {d.level} / {d.maxLevel}");
                if (d.level >= d.maxLevel) sb.Append(" · max");
            }
            sb.AppendLine();

            if (d.inspirationNext > 0)
                sb.Append($"+{d.inspirationNext} inspiration / loop").AppendLine();

            sb.Append("Instrument: ")
              .Append(string.IsNullOrEmpty(d.instrumentName) ? "-" : d.instrumentName);

#if ALWTTT_DEV
            if (!string.IsNullOrEmpty(d.bundleName))
                sb.AppendLine().Append("Bundle: ").Append(d.bundleName);
#endif
            // Per-track PartEffects are not stored on TrackEntry today (explicit
            // TODO in the model). We print the row rather than omit it, so the
            // absence is visible as a known gap and not as an oversight.
            sb.AppendLine().Append("Modifiers: -");

            return sb.ToString();
        }

        #endregion

        #region Placement

        private void PlaceNextTo(RectTransform anchor)
        {
            if (!panel || !anchor || !canvasRect) return;

            // Anchor's canvas-local rect.
            Vector3[] corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            Vector2 rightEdge = canvasRect.InverseTransformPoint(corners[2]); // top-right
            Vector2 bottomRight = canvasRect.InverseTransformPoint(corners[3]);

            float centerY = (rightEdge.y + bottomRight.y) * 0.5f;
            float x = rightEdge.x + anchorGapX;
            float y = centerY;

            // Clamp 1: keep the whole panel above the band-character zone. If the
            // hovered row sits low, we anchor to its TOP edge instead of its
            // center so the panel grows upward.
            float halfH = panel.rect.height * 0.5f;
            if (y - halfH < minBottomY) y = minBottomY + halfH;

            // Clamp 2: never enter the audience third.
            float width = panel.rect.width;
            if (x + width > maxRightX) x = Mathf.Max(0f, maxRightX - width);

            // [HUD-COMP-1 fix] Anchors at the CENTER, because x/y above come
            // from canvasRect.InverseTransformPoint, which is center-origin
            // (X -half..+half, Y -half..+half). With anchors at (0,1) the same
            // numbers would be read as offsets from the top-left corner and the
            // panel would land ~(-halfW, +halfH) away from the row.
            // Consequence: this object must be a DIRECT child of the canvas
            // referenced by canvasRect, since anchoredPosition is relative to
            // the PARENT.
            panel.pivot = new Vector2(0f, 0.5f);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = new Vector2(x, y);
        }

        #endregion

        private void Update()
        {
            if (!canvasGroup) return;
            if (Mathf.Approximately(canvasGroup.alpha, _targetAlpha)) return;
            float step = fadeDuration <= 0f ? 1f : Time.unscaledDeltaTime / fadeDuration;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, _targetAlpha, step);
            // Visibility is alpha-only. Deactivating this GameObject would also
            // stop this Update(), so the panel could never fade back in.
        }

        private void SetAlphaImmediate(float a)
        {
            _targetAlpha = a;
            if (canvasGroup)
            {
                canvasGroup.alpha = a;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
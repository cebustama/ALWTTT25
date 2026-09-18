// Placement: Assets/Scripts/UI/Song Composition/TrackHoverPanel.cs
//
// [HUD-COMP-1 / spec section 5] The strip's hover contract: everything the rest
// state encodes as icon/pip/shape shows up here as plain text.
//
// WHY IT ANCHORS TO THE ROW AND NOT THE CURSOR:
// the existing minicard follows the pointer, which is right for cards in hand
// (they move) and wrong here (rows do not). A cursor-following panel over a
// static strip drifts into the hand and the audience -- the two zones the
// player must keep reading while hovering. Anchoring to the row means the panel
// opens in one predictable place, and we can CLAMP it against the two forbidden
// zones instead of hoping the cursor stays out of them.
//
// Safe zone (reference canvas 1920x1080, see spec section 5):
//   - never below minBottomY (keeps the hand strip readable)
//   - never right of maxRightX (keeps the audience third readable)
// Clamps are canvas-local and CENTER-origin (0 = mid-screen), because that is
// what canvasRect.InverseTransformPoint returns.
//
// SOURCE IS PURE ASCII ON PURPOSE (F-T6-1, re-opened as F-SP1-2 in SONG-PANEL-1):
// this file was saved as Windows-1252 twice. A BOM alone did not survive an
// editor round-trip, so every non-ASCII glyph is now an escape ("\u00B7" for
// the mid-dot separator) or a plain ASCII substitute in comments. The
// LiberationSans SDF atlas ships a Latin-1 set, so U+00B7 renders; U+2014 (em
// dash) does not and must not be used in player text.
//
// [TEXT-6 / F-T5-2] Every word in the panel comes from ConceptGlossarySO via
// GameText.Format (ui_track_* templates; "Modifiers" reuses ui_card_detail_modifiers,
// D-T6-3=B). What stays in code is punctuation (Sep, ": ", "-"), the TrackRole
// enum name (F-TXT4-2 -> CARD-FACE-1) and the DEV-only bundle line (D-T6-4=A,
// confirmed by D-SP1-2=B: the runtime toggle lives INSIDE the #if, so the line
// is still unreachable in player builds and stays out of the glossary).
//
// [SONG-PANEL-1 / ST-T6-2, ST-T6-4] Legibility only: bold on the identity line,
// the card name and the section label; a background Image with an explicit
// colour. No typographic redesign, no width or hierarchy change (HUD-COMP-2).
// Rich-text tags are FORMAT, not text: they are added here, never in the
// glossary, so a translator never sees them (same rule as ui_aud_taste_block_header).

using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ALWTTT.Tooltips;
using static ALWTTT.Tooltips.GameTextTemplateIds;   // [TEXT-6]

namespace ALWTTT.UI
{
    public class TrackHoverPanel : MonoBehaviour
    {
        public static TrackHoverPanel Instance { get; private set; }

        [SerializeField] private RectTransform panel;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text bodyText;

        [Header("Background [SONG-PANEL-1]")]
        [SerializeField, Tooltip("The child Image that paints behind the text (prefab child 'Panel'). " +
                 "Optional: when unassigned the panel keeps whatever the prefab paints.")]
        private Image background;
        [SerializeField, Tooltip("Applied to `background` once in Awake. Alpha is the point: " +
                 "the text must read over the composition grid.")]
        private Color backgroundColor = new Color(0.05f, 0.05f, 0.07f, 0.85f);

        [Header("Placement")]
        [SerializeField, Tooltip("Horizontal gap from the strip's right edge.")]
        private float anchorGapX = 32f;
        [SerializeField, Tooltip("Canvas-local Y (CENTER-origin: 0 is mid-screen, " +
                 "-540 the bottom on a 1080-tall canvas). Keeps the panel above the hand strip.")]
        private float minBottomY = -300f;
        [SerializeField, Tooltip("Canvas-local X (CENTER-origin: 0 is mid-screen, " +
                 "+960 the right edge on a 1920-wide canvas). Keeps the panel out of the audience third.")]
        private float maxRightX = 300f;
        [SerializeField, Tooltip("[SONG-PANEL-1b] Margin from the canvas edge for the TOP and LEFT " +
                 "clamps. Unlike minBottomY/maxRightX these are computed from the live canvas size, " +
                 "because they guard against leaving the SCREEN, not against covering a game zone.")]
        private float edgeMargin = 16f;
        [SerializeField] private float fadeDuration = 0.12f;

#if ALWTTT_DEV
        [Header("Dev [SONG-PANEL-1 / D-SP1-2=B]")]
        [SerializeField, Tooltip("Show the style-bundle asset name under the instrument line. " +
                 "DEV builds only: the field itself is compiled out of player builds, so the " +
                 "line can never reach a player and needs no glossary id (D-T6-4=A).")]
        private bool showDevInfo = true;
#endif

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
            // [SONG-PANEL-1] The background is a CHILD ('Panel'), never `panel`
            // itself: see the note above. Colour is set here, not in the prefab,
            // so the value is reviewable in source and survives a prefab revert.
            if (background != null)
            {
                background.color = backgroundColor;
                background.raycastTarget = false;   // never steals hover (same as the CanvasGroup)
            }
            if (bodyText != null) bodyText.richText = true; // <b> below is a no-op otherwise
            SetAlphaImmediate(0f);
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        #region Public API

        public void ShowForTrack(SongTrackElementUI.RowData d, RectTransform anchor)
            => ShowRaw(BuildTrackText(d), anchor);

        public void ShowRaw(string body, RectTransform anchor)
        {
            if (bodyText) bodyText.text = body;

            // [SONG-PANEL-1c / F-SP1-12] Measure AFTER the new text has been laid out.
            //
            // This object is content-sized (that is why the background child carries
            // "Ignore Layout"), so panel.rect.height and .width only catch up with new text on
            // the next layout pass - one frame late. PlaceNextTo reads both to clamp, so without
            // this rebuild the panel is positioned using the PREVIOUS tooltip's size: hovering a
            // short track row and then the taller context row placed the context panel with the
            // track panel's height and it rode off the top edge. Going straight to the context
            // row happened to work because the stale size was the prefab's, which is close enough.
            // Placement must not depend on hover history, so the rebuild is unconditional.
            if (bodyText) bodyText.ForceMeshUpdate();
            if (panel) LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

            PlaceNextTo(anchor);
            _targetAlpha = 1f;
            if (canvasGroup) canvasGroup.blocksRaycasts = false; // never steals hover
        }

        public void Hide() => _targetAlpha = 0f;

        #endregion

        #region Text

        // U+00B7 mid dot: Latin-1, present in the SDF atlas (see header). Escaped on purpose.
        private const string Sep = " \u00B7 ";
        private const string None = "-";
        private const string B = "<b>";
        private const string EndB = "</b>";

        // Instance method since SONG-PANEL-1: the DEV toggle is a serialized field.
        private string BuildTrackText(SongTrackElementUI.RowData d)
        {
            var sb = new StringBuilder();
            string who = string.IsNullOrEmpty(d.musicianName) ? d.musicianId : d.musicianName;

            if (d.placeholder)
            {
                sb.Append(B).Append(who).Append(Sep).Append(GameText.Format(UiTrackNoTrack)).Append(EndB);
                return sb.ToString();
            }

            // Line 1 (identity), bold. d.role is the TrackRole enum name: not a
            // template (F-TXT4-2 -> CARD-FACE-1).
            // [CARD-FACE-1 / F-TXT4-2] Role as glyph + glossary name (ui_role_<value>, derived).
            sb.Append(B).Append(who).Append(Sep)
              .Append(GameSymbols.Role(d.role)).Append(' ').Append(GameText.Format(UiRole(d.role)));
            if (d.pending) sb.Append(Sep).Append(GameText.Format(UiTrackPending));
            sb.Append(EndB).AppendLine();

            // Line 2 (card name), bold. Since D-SP1-1=A this is ALWAYS the glossary
            // card name (or a ui_track_* template for card-less rows): nothing
            // rewrites TrackEntry.info after creation any more (F-T6-8).
            sb.Append(B).Append(d.info).Append(EndB);
            // Lv1 is not printed: "Lv 1 / 3" reads as a deficiency on a track
            // that is simply normal. Only a real level prints. (D2 / D8=B)
            if (d.level >= 2)
            {
                sb.Append(Sep).Append(GameText.Format(UiTrackLevel, d.level, d.maxLevel));
                if (d.level >= d.maxLevel) sb.Append(Sep).Append(GameText.Format(UiTrackLevelMax));
            }
            sb.AppendLine();

            if (d.inspirationNext > 0)
                sb.Append(GameText.Format(UiTrackInspirationPerLoop, d.inspirationNext)).AppendLine();

            sb.Append(GameText.Format(UiTrackInstrument,
                string.IsNullOrEmpty(d.instrumentName) ? None : d.instrumentName));

#if ALWTTT_DEV
            // [TEXT-6 / D-T6-4=A] [SONG-PANEL-1 / D-SP1-2=B] Compiled out of player
            // builds AND toggleable in DEV ones: a diagnostic, not a player surface,
            // so it is not a template. Not a GAMETEXT-NO-DEV-LABEL read.
            if (showDevInfo && !string.IsNullOrEmpty(d.bundleName))
                sb.AppendLine().Append("Bundle: ").Append(d.bundleName);
#endif
            // Per-track PartEffects are not stored on TrackEntry today (explicit
            // TODO in the model). We print the row rather than omit it, so the
            // absence is visible as a known gap and not as an oversight.
            // [TEXT-6 / D-T6-3=B] Header reused from the detail modal: one concept, one home.
            sb.AppendLine().Append(B).Append(GameText.Format(UiCardDetailModifiers)).Append(EndB)
              .Append(": ").Append(None);

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

            // [SONG-PANEL-1b / ST-SP1-3] Clamps 3 and 4: stay ON SCREEN.
            // Clamps 1 and 2 guard GAME ZONES (hand, audience) and are authored against the
            // 1920x1080 reference canvas. These two guard the CANVAS EDGE, so they are computed
            // from the live rect instead: a row near the top of the strip - the context row, in
            // particular - pushed the panel past the top edge and the first line (the loop
            // counter) was unreadable. Nothing clamped upward because the original placement
            // assumed rows in the lower half.
            float halfCanvasH = canvasRect.rect.height * 0.5f;
            float halfCanvasW = canvasRect.rect.width * 0.5f;

            // Top wins over Clamp 1 when both cannot be satisfied: the panel's FIRST line carries
            // the identity/loop counter, so losing the bottom is cheaper than losing the head.
            float maxTopY = halfCanvasH - edgeMargin;
            if (y + halfH > maxTopY) y = maxTopY - halfH;

            // Left: pivot is (0, 0.5), so x IS the left edge.
            float minLeftX = -halfCanvasW + edgeMargin;
            if (x < minLeftX) x = minLeftX;

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
// Place at: Assets/Scripts/UI/Tooltips/TooltipManager.cs
//
// [TIP-1 / F-BN-7 closed / D-TIP-3=B] THREE ENTRIES, one contract each
// (authority: Docs/systems/SSoT_Tooltip_Pipeline.md section 2):
//
//   ShowTooltip   PUSHES one more panel onto the visible column and restarts the
//                 fade-in. Calling it N times without HideTooltip shows N panels.
//                 This is a CAPABILITY, not a leak: the card hover (one panel per
//                 keyword/status, SSoT_Card_System 10.2) and the audience canvas
//                 (status + intention, F-TXT4-11) stack on purpose. A surface that
//                 wants ONE panel calls HideTooltip first (the tutorial overlay does,
//                 SSoT_Game_Text section 4).
//   UpdateTooltip REWRITES the text of a panel that is already shown, in place:
//                 no new panel, no fade restart, no count change. Returns false when
//                 nothing is shown so the caller can fall back to ShowTooltip. This
//                 is the entry a live-updating surface (the C1 Vibe readout) uses on
//                 every refresh; before TIP-1 it had to Hide+Show and replay the fade.
//   HideTooltip   Clears the column: count to zero, every panel inactive, alpha 0.
//
// [S4b / D7=A] CONCEPT LINKS. Every body that enters a panel goes through
// ConceptTagRenderer.Decorate: known <link=id> words get the concept look (colour,
// bold). The ids linked by the panels a caller pushed are queued, and at LateUpdate
// (after every caller of the frame has finished building its column) one extra
// panel per linked concept is appended at the END of the column, unless a panel with
// that concept's name is already shown (the musician column already lists
// Compostura, so a link to it adds nothing). Depth 1: the bodies of the appended
// panels are decorated but their own links are not followed, so a column never
// cascades. UpdateTooltip decorates but never queues (the live Vibe readout calls it
// every refresh).
//
// [S9c / D3=C] CONCEPT PANELS ON DEMAND. A linked concept panel shows by itself
// only the first autoShowFirstTimes hovers that bring it up (counted per concept id
// in PlayerPrefs, once per hover). After that it is HELD BACK: the column ends in a
// one-line hint ("Hold Shift: concepts") and the held-back panels appear while
// Shift is held, and go away when it is released. Held-back and hint panels are
// always the LAST panels of the column; a new push removes them first and the next
// LateUpdate puts them back, so the order never breaks.
//
// Every text assignment ends in TooltipController.Reposition(): the panel is
// content-sized, so it must be REBUILT before it is MEASURED and PLACED
// (assign -> rebuild -> measure -> place, the F-SP1-12 rule).
//
// SOURCE IS PURE ASCII ON PURPOSE (F-T6-1 / F-SP1-2 family).
using ALWTTT.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Tooltips
{
    public class TooltipManager : MonoBehaviour
    {
        private const string DebugTag = "<color=green>[TooltipManager]</color>";


        public static TooltipManager Instance;

        [Header("References")]
        [SerializeField] private TooltipController tooltipController;
        //[SerializeField] private CursorController cursorController;
        [SerializeField] private TooltipText tooltipTextPrefab;
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("[TXT-3 / D-TXT3-5=A] Glossary of the running build's language. The ONLY " +
         "source of player-facing concept text: status icons (by StatusKey), card " +
         "keywords (by enum name) and <link=id> tags resolve here. Assign the same " +
         "language as the tutorial catalog and its glossary.")]
        [SerializeField] private ConceptGlossarySO conceptGlossary;
        [Tooltip("[GIG-UI-1] Colours per panel kind and the audience column offset. " +
         "Empty = every panel keeps the TooltipText prefab look.")]
        [SerializeField] private TooltipStyleSO tooltipStyle;

        [Header("Concept links (S4b / D7=A)")]
        [Tooltip("Colour of a known <link=id> word inside a tooltip body.")]
        [SerializeField] private Color conceptColor = new Color(1f, 0.82f, 0.35f, 1f);
        [SerializeField] private bool conceptBold = true;
        [SerializeField] private bool conceptUnderline = false;
        [Tooltip("Append one panel per linked concept below the column (Monster Train).")]
        [SerializeField] private bool appendLinkedPanels = true;
        [Tooltip("Most concept panels appended to one column.")]
        [SerializeField, Min(0)] private int maxLinkedPanels = 3;

        [Header("Concept panels on demand (S9c / D3=C)")]
        [Tooltip("Linked concept panels show by themselves only the first times; then only while the key is held.")]
        [SerializeField] private bool conceptPanelsOnDemand = true;
        [SerializeField] private KeyCode conceptKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode conceptKeyAlt = KeyCode.RightShift;
        [Tooltip("Hovers in which each concept panel still shows by itself (per concept, saved).")]
        [SerializeField, Min(0)] private int autoShowFirstTimes = 2;
        [Tooltip("End the column with a one-line hint when panels are held back.")]
        [SerializeField] private bool showConceptHint = true;

        /// <summary>[GIG-UI-1] Null when no style asset is assigned.</summary>
        public TooltipStyleSO Style => tooltipStyle;

        [Header("Settings")]
        [SerializeField] private AnimationCurve fadeCurve;
        [SerializeField] private float showDelayTime = 0.5f;

        /// <summary>[TXT-3] Active-language glossary. Null = nothing assigned (consumers show raw ids and warn once).</summary>
        public ConceptGlossarySO ConceptGlossary => conceptGlossary;


        /// <summary>[TIP-1] Number of panels currently shown (0 = nothing visible).</summary>
        public int ShownTooltipCount => currentShownTooltipCount;

        private List<TooltipText> tooltipTextList = new List<TooltipText>();
        private TooltipController TooltipController => tooltipController;

        private int currentShownTooltipCount;

        // [S4b / D7=A] concept-link state for the column being shown.
        private ConceptTooltipResolver _linkResolver;
        private readonly List<string> _pendingLinkIds = new List<string>();
        private readonly List<string> _shownHeaders = new List<string>();
        private int _linkedPanelsShown;
        private Transform _lastTarget;
        private Camera _lastCam;
        private Vector2 _lastOffset;
        private bool _lastGrowDown;
        private string _lastLinkLog;

        // [S9c / D3=C] on-demand state for the column being shown.
        private const string SeenPrefPrefix = "alwttt.concept_seen.";
        private readonly List<string> _heldBackIds = new List<string>();
        private readonly HashSet<string> _countedThisHover = new HashSet<string>();
        private readonly List<string> _onDemandHeaders = new List<string>();
        private int _onDemandPanels;
        private bool _hintShown;

        private void Awake()
        {
            if (Instance == null)
            {
                transform.parent = null;
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// PUSH one more panel and restart the fade-in. See the file header for the
        /// stacking contract. <paramref name="screenOffset"/> is a pixel offset from
        /// the target's screen point (static targets only; ignored when following the
        /// cursor).
        /// </summary>
        public void ShowTooltip(
            string contentText = "", string headerText = "",
            Transform tooltipTargetTransform = null,
            Camera cam = null, float delayShow = 0,
            Vector2 screenOffset = default(Vector2),
            bool growDown = false, TooltipKind kind = TooltipKind.Default)
        {
            // [GIG-UI-1] growDown: the target point is the column's TOP-left (it grows
            // downward), not its bottom-left. kind: which TooltipStyleSO entry colours this
            // panel; Default (or no asset) keeps the prefab look. Panels are pooled, so
            // every call applies a style or restores the prefab one.
            //Debug.Log($"{DebugTag} Showing tooltip for '{headerText}");

            StartCoroutine(ShowRoutine(delayShow));
            RemoveTrailingPanels(); // [S9c] hint / on-demand panels stay last

            _lastTarget = tooltipTargetTransform; _lastCam = cam;
            _lastOffset = screenOffset; _lastGrowDown = growDown;
            contentText = DecorateConcepts(contentText, queueLinks: true);
            PushPanel(contentText, headerText, kind);
        }

        // One panel at the end of the column (no fade restart).
        private void PushPanel(string contentText, string headerText, TooltipKind kind)
        {
            currentShownTooltipCount++;
            if (tooltipTextList.Count < currentShownTooltipCount)
            {
                var newTooltip = Instantiate(tooltipTextPrefab, TooltipController.transform);
                tooltipTextList.Add(newTooltip);
            }

            var panel = tooltipTextList[currentShownTooltipCount - 1];
            panel.gameObject.SetActive(true);
            panel.SetText(contentText, headerText);
            TooltipStyleSO.PanelStyle style = null;
            if (tooltipStyle != null && !tooltipStyle.TryGet(kind, out style)
                && kind == TooltipKind.Concept)
                tooltipStyle.TryGet(TooltipKind.Status, out style); // concept panels look like statuses until styled
            panel.ApplyStyle(style);
            if (!string.IsNullOrEmpty(headerText)) _shownHeaders.Add(headerText);

            TooltipController.SetFollowPos(_lastTarget, _lastCam, _lastOffset, _lastGrowDown);
            TooltipController.Reposition();
        }

        private ConceptTooltipResolver LinkResolver =>
            _linkResolver ??= new ConceptTooltipResolver(conceptGlossary);

        private string DecorateConcepts(string text, bool queueLinks)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf("<link=", System.StringComparison.Ordinal) < 0)
                return text;
            if (queueLinks && appendLinkedPanels)
            {
                foreach (var id in ConceptTagRenderer.ExtractIds(text))
                    if (!_pendingLinkIds.Contains(id)) _pendingLinkIds.Add(id);
            }
            return ConceptTagRenderer.Decorate(text, LinkResolver, conceptColor, conceptUnderline,
                id => Debug.LogWarning($"{DebugTag} [S4b] Unknown concept link '{id}' in a tooltip."),
                conceptBold);
        }

        // Appends the queued concept panels once the callers of this frame are done.
        private void LateUpdate()
        {
            if (currentShownTooltipCount == 0)
            {
                _pendingLinkIds.Clear();
                _heldBackIds.Clear();
                _countedThisHover.Clear(); // the hover is over: the next one counts again
                _onDemandHeaders.Clear();
                _onDemandPanels = 0;
                _hintShown = false;
                return;
            }

            if (_pendingLinkIds.Count > 0) AppendPendingLinks();
            if (conceptPanelsOnDemand) UpdateOnDemandPanels();
        }

        private void AppendPendingLinks()
        {
            RemoveTrailingPanels();
            List<string> added = null;
            foreach (var id in _pendingLinkIds)
            {
                if (_linkedPanelsShown >= maxLinkedPanels) break;
                if (LinkResolver.Resolve(id, out var header, out var body) == ConceptSource.None) continue;
                if (IsHeaderShown(header)) continue;
                if (!AutoShowConcept(id))
                {
                    if (!_heldBackIds.Contains(id)) _heldBackIds.Add(id); // [S9c] waits for Shift
                    continue;
                }
                PushPanel(DecorateConcepts(body, queueLinks: false), header, TooltipKind.Concept);
                _linkedPanelsShown++;
                (added ??= new List<string>()).Add(id);
            }
            _pendingLinkIds.Clear();
            string log = added != null ? string.Join(", ", added) : null;
            if (_heldBackIds.Count > 0)
                log = (log ?? "-") + " | [S9c] con Shift: " + string.Join(", ", _heldBackIds);
            if (log != null && log != _lastLinkLog)   // once per event, not once per rebuilt frame
                Debug.Log($"[S4b] Conceptos enlazados en el tooltip: {log}");
            _lastLinkLog = log;
        }

        // [S9c] True while the concept still shows by itself; counts the hover once.
        private bool AutoShowConcept(string id)
        {
            if (!conceptPanelsOnDemand) return true;
            if (_countedThisHover.Contains(id)) return true;
            string key = SeenPrefPrefix + id;
            int seen = PlayerPrefs.GetInt(key, 0);
            if (seen >= autoShowFirstTimes) return false;
            PlayerPrefs.SetInt(key, seen + 1);
            _countedThisHover.Add(id);
            Debug.Log($"[S9c] Concepto '{id}' visto {seen + 1}/{autoShowFirstTimes} veces sin Shift.");
            return true;
        }

        private bool ConceptKeyHeld() => Input.GetKey(conceptKey) || Input.GetKey(conceptKeyAlt);

        // [S9c] Held: held-back panels shown, no hint. Released: panels gone, hint shown.
        private void UpdateOnDemandPanels()
        {
            if (_heldBackIds.Count == 0) return;
            if (ConceptKeyHeld())
            {
                if (_hintShown) { PopPanels(1); _hintShown = false; }
                if (_onDemandPanels > 0) return;
                foreach (var id in _heldBackIds)
                {
                    if (_linkedPanelsShown >= maxLinkedPanels) break;
                    if (LinkResolver.Resolve(id, out var header, out var body) == ConceptSource.None) continue;
                    if (IsHeaderShown(header)) continue;
                    PushPanel(DecorateConcepts(body, queueLinks: false), header, TooltipKind.Concept);
                    _onDemandHeaders.Add(header);
                    _onDemandPanels++;
                    _linkedPanelsShown++;
                }
                if (_onDemandPanels > 0)
                    Debug.Log($"[S9c] Shift: {_onDemandPanels} conceptos a la vista ({string.Join(", ", _heldBackIds)}).");
            }
            else
            {
                if (_onDemandPanels > 0) RemoveOnDemandPanels();
                if (showConceptHint && !_hintShown)
                {
                    PushPanel(GameText.Format(GameTextTemplateIds.UiTipConceptsHint), string.Empty,
                        TooltipKind.Concept);
                    _hintShown = true;
                }
            }
        }

        // [S9c] Take the hint and the on-demand panels off the end of the column.
        private void RemoveTrailingPanels()
        {
            if (_hintShown) { PopPanels(1); _hintShown = false; }
            if (_onDemandPanels > 0) RemoveOnDemandPanels();
        }

        private void RemoveOnDemandPanels()
        {
            PopPanels(_onDemandPanels);
            _linkedPanelsShown = Mathf.Max(0, _linkedPanelsShown - _onDemandPanels);
            foreach (var h in _onDemandHeaders) _shownHeaders.Remove(h);
            _onDemandHeaders.Clear();
            _onDemandPanels = 0;
        }

        private void PopPanels(int n)
        {
            for (int k = 0; k < n && currentShownTooltipCount > 0; k++)
            {
                currentShownTooltipCount--;
                tooltipTextList[currentShownTooltipCount].gameObject.SetActive(false);
            }
            TooltipController.Reposition();
        }

        // "Compostura x3" counts as Compostura already shown.
        private bool IsHeaderShown(string header)
        {
            if (string.IsNullOrEmpty(header)) return false;
            foreach (var h in _shownHeaders)
                if (h.StartsWith(header, System.StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>
        /// [TIP-1 / F-BN-7] Rewrite a shown panel IN PLACE: no new panel, no fade
        /// restart, placement mode unchanged. <paramref name="panelIndex"/> is the
        /// panel's position in the column (0 = the first ShowTooltip since the last
        /// Hide). Returns false, and does nothing, when that panel is not shown.
        /// </summary>
        public bool UpdateTooltip(string contentText = "", string headerText = "", int panelIndex = 0)
        {
            if (panelIndex < 0 || panelIndex >= currentShownTooltipCount) return false;

            contentText = DecorateConcepts(contentText, queueLinks: false);
            tooltipTextList[panelIndex].SetText(contentText, headerText);
            TooltipController.Reposition();
            return true;
        }

        public void HideTooltip()
        {
            StopAllCoroutines();
            currentShownTooltipCount = 0;
            _pendingLinkIds.Clear();
            _shownHeaders.Clear();
            _linkedPanelsShown = 0;
            _heldBackIds.Clear();          // [S9c] the panels are gone; _countedThisHover stays
            _onDemandHeaders.Clear();      // until LateUpdate sees an empty column
            _onDemandPanels = 0;
            _hintShown = false;
            canvasGroup.alpha = 0;
            foreach (var tooltipText in tooltipTextList)
                tooltipText.gameObject.SetActive(false);
        }

        private IEnumerator ShowRoutine(float delay = 0)
        {
            var waitFrame = new WaitForEndOfFrame();
            var timer = 0f;

            canvasGroup.alpha = 0;

            yield return new WaitForSeconds(delay);

            while (true)
            {
                timer += Time.deltaTime;

                var invValue = Mathf.InverseLerp(0, showDelayTime, timer);
                canvasGroup.alpha = fadeCurve.Evaluate(invValue);

                if (timer >= showDelayTime)
                {
                    canvasGroup.alpha = 1;
                    break;
                }
                yield return waitFrame;
            }
        }
    }
}
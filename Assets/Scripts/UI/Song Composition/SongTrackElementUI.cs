// Placement: Assets/Scripts/UI/Song Composition/SongTrackElementUI.cs  (REPLACES existing file)
//
// [HUD-COMP-1] Composition-strip track row.
//
// WHY THE CLASS NAME AND FILE PATH ARE UNCHANGED: keeping the type identity
// keeps the .meta GUID, so every prefab that already has this component keeps
// it. Serialized field names change (roleText/infoText/inspirationNextText are
// gone), so the prefab MUST be re-wired — but it is a field re-wire, not a
// component swap, and SongPartElementUI's Instantiate path is untouched.
//
// CONTRACT CHANGE vs the previous version:
//   - `role` arrives as TrackRole, not string. The row renders an ICON; the
//     role NAME only exists in hover. (Spec §1.2.1 / restriction "texto minimo")
//   - the +N inspiration TMP is REMOVED from rest and moved to hover (§1.2.8)
//   - `pending` no longer tints text orange (gold = venue/SFX in the HUD color
//     system); it draws an animated dashed border instead (§1.2.7)
//   - level pips exist but nothing feeds them yet: RowData.level defaults to 1
//     and Lv1 draws NO pips (D2). R7 sets the field; this file needs no change.
//
// The row is a DISPLAY. It has no click handler and never disables hand drag.

using System.Collections;
using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Managers;
using MidiGenPlay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    /// <summary>Density tiers from Composition_View_Spec.md §6.</summary>
    public enum StripDensityTier
    {
        Normal = 0,   // R <= 4
        TightEmpties, // R >= 5  : empty rows shrink
        Compact,      // R >= 6  : row height / font / icon shrink
        HideEmpties,  // R >= 7  : empty rows collapse to 0
        PipsHoverOnly // R >= 8  : level pips leave rest
    }

    public class SongTrackElementUI : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        #region Bind payload

        /// <summary>
        /// One struct instead of nine positional args. Adding a field later
        /// (R7's real level source, a mute flag) does not churn every call site.
        /// </summary>
        public struct RowData
        {
            public TrackRole role;
            public string musicianId;
            public string musicianName;   // hover line 1
            public Sprite musicianIcon;   // empty-row content
            public string info;           // card name — the ONLY rest text
            public int level;             // 1..3; 1 draws no pips (D2)
            public int maxLevel;          // 3 (D1)
            public bool placeholder;      // roster musician with no track
            public bool pending;          // applies next loop
            public int inspirationNext;   // hover only
            public int partIndex;         // needed to re-resolve the instrument
            public string instrumentName; // hover only; refreshed on hover
            public string bundleName;     // hover only, DEV builds only
            public CardDefinition sourceCard;
        }

        #endregion

        #region Wiring

        [Header("Theme")]
        [SerializeField] private CompositionStripThemeSO theme;

        [Header("Visuals")]
        [SerializeField] private RectTransform rowRect;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private Image pillImage;
        [SerializeField] private Image borderImage;   // pending dashed border
        [SerializeField] private Image roleIcon;
        [SerializeField] private Image musicianIcon;  // empty rows only
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private CanvasGroup cg;

        [Header("Level (R7 feeds this; inert until then)")]
        [SerializeField] private RectTransform levelPipsRoot;
        [SerializeField] private Image[] levelPips = new Image[3];
        [SerializeField] private RectTransform levelUpFloater;

        #endregion

        #region State

        private RowData _data;
        private bool _hasData;
        private int _lastLevel = -1;
        private StripDensityTier _tier = StripDensityTier.Normal;
        private Coroutine _levelUpCo;
        private Coroutine _flashCo;
        private float _pendingPhase;

        public bool IsPlaceholder => _data.placeholder;
        public string CardName => _data.info;
        public TMP_Text NameText => nameText;   // SongPartElementUI measures this

        #endregion

        #region Bind

        /// <param name="suppressFx">
        /// True on a full rebuild (Bind of the whole part), where every row is
        /// freshly instantiated and a "level up" animation would be a lie.
        /// False on the incremental AddOrUpdateTrack path, which is the only
        /// place a real level transition can be observed.
        /// </param>
        public void Bind(RowData data, bool suppressFx = false)
        {
            EnsureRefs();
            bool levelRose = _hasData && !suppressFx &&
                             data.level > _lastLevel && _lastLevel > 0 &&
                             data.sourceCard == _data.sourceCard;
            bool cardChanged = _hasData && data.sourceCard != _data.sourceCard;

            _data = data;
            _hasData = true;

            if (data.placeholder) ApplyEmpty();
            else ApplyActive();

            ApplyDensity(_tier);

            if (levelRose) PlayLevelUp();
            else if (cardChanged && !suppressFx && !data.placeholder) PlayReplaceFlash();

            _lastLevel = data.placeholder ? -1 : Mathf.Max(1, data.level);
        }

        private void ApplyEmpty()
        {
            // An empty row's job is to say WHICH musician is silent — that is
            // the whole reason it earns pixels. The old build drew "—", which
            // said only "something is missing".
            if (pillImage)
            {
                pillImage.color = Mul(theme.pillFill, 0.55f);
                pillImage.enabled = true;
            }
            if (borderImage) borderImage.enabled = false;
            if (roleIcon) roleIcon.enabled = false;
            if (nameText) nameText.text = "";
            if (musicianIcon)
            {
                musicianIcon.sprite = _data.musicianIcon;
                musicianIcon.color = new Color(1f, 1f, 1f, 0.45f);
                musicianIcon.enabled = _data.musicianIcon != null;
            }
            SetPips(0);
            if (cg) cg.alpha = 1f;
        }

        private void ApplyActive()
        {
            if (pillImage) { pillImage.color = theme.pillFill; pillImage.enabled = true; }
            if (musicianIcon) musicianIcon.enabled = false;

            if (roleIcon)
            {
                theme.TryGetRole(_data.role, out var icon, out var tint, out _);
                roleIcon.sprite = icon;
                roleIcon.color = tint;         // tint lives on the GLYPH only (D6=B)
                roleIcon.enabled = icon != null;
            }

            if (nameText)
            {
                nameText.text = string.IsNullOrWhiteSpace(_data.info) ? "" : _data.info.Trim();
                nameText.color = theme.nameColor;   // never the pending tint
            }

            SetPips(_data.level);

            if (borderImage)
            {
                borderImage.enabled = _data.pending;
                borderImage.color = _data.pending
                    ? theme.pendingBorderColor
                    : theme.pillBorder;
            }
            if (cg) cg.alpha = 1f;
        }

        /// <summary>Lv1 draws nothing (D2). Empty pips are not drawn as rings —
        /// a ring would read as "missing", and Lv1 is not a deficiency.</summary>
        private void SetPips(int level)
        {
            bool hide = _tier == StripDensityTier.PipsHoverOnly;
            int shown = (hide || level < 2) ? 0 : Mathf.Clamp(level, 0, levelPips.Length);
            if (levelPipsRoot) levelPipsRoot.gameObject.SetActive(shown > 0);
            for (int i = 0; i < levelPips.Length; i++)
            {
                if (!levelPips[i]) continue;
                levelPips[i].enabled = i < shown;
                levelPips[i].color = theme.levelColor;
                levelPips[i].rectTransform.localScale = Vector3.one;
            }
        }

        #endregion

        #region Density + width

        public void ApplyDensity(StripDensityTier tier)
        {
            EnsureRefs();
            _tier = tier;
            bool compact = tier >= StripDensityTier.Compact;

            float h;
            if (_data.placeholder)
            {
                h = tier >= StripDensityTier.HideEmpties ? 0f
                  : tier >= StripDensityTier.TightEmpties ? theme.emptyRowHeightDense
                  : theme.emptyRowHeight;
            }
            else h = compact ? theme.rowHeightDense : theme.rowHeight;

            if (layoutElement) { layoutElement.preferredHeight = h; layoutElement.minHeight = h; }
            gameObject.SetActive(h > 0.01f || !_data.placeholder);

            // [HUD-COMP-1] Every icon size lives in the theme, not in the
            // prefab: the strip is tuned as ONE object, and chasing a size
            // across four prefabs is how a strip ends up visually incoherent.
            if (roleIcon)
            {
                float s = compact ? theme.roleIconSizeDense : theme.roleIconSize;
                roleIcon.rectTransform.sizeDelta = new Vector2(s, s);
            }
            if (musicianIcon)
            {
                float s = theme.musicianIconSize * (compact ? 0.85f : 1f);
                musicianIcon.rectTransform.sizeDelta = new Vector2(s, s);
            }
            for (int i = 0; i < levelPips.Length; i++)
            {
                if (!levelPips[i]) continue;
                levelPips[i].rectTransform.sizeDelta =
                    new Vector2(theme.levelPipSize, theme.levelPipSize);
            }
            if (nameText)
            {
                nameText.fontSize = compact ? theme.nameFontSizeDense : theme.nameFontSize;
                // Tier 4 truncation: overflow already ellipsizes; we only tighten
                // the budget so the strip cannot widen under pressure.
                nameText.overflowMode = TextOverflowModes.Ellipsis;
                nameText.enableWordWrapping = false;
            }
            if (!_data.placeholder) SetPips(_data.level);
        }

        /// <summary>Set by SongPartElementUI so every row shares one width.</summary>
        public void SetWidth(float width)
        {
            if (!rowRect) return;
            rowRect.sizeDelta = new Vector2(width, rowRect.sizeDelta.y);
            if (layoutElement) { layoutElement.preferredWidth = width; layoutElement.minWidth = width; }
        }

        #endregion

        #region FX

        private void PlayLevelUp()
        {
            if (_levelUpCo != null) StopCoroutine(_levelUpCo);
            _levelUpCo = StartCoroutine(LevelUpRoutine());
        }

        private IEnumerator LevelUpRoutine()
        {
            int idx = Mathf.Clamp(_data.level, 1, levelPips.Length) - 1;
            var pip = (idx >= 0 && idx < levelPips.Length) ? levelPips[idx] : null;
            if (levelUpFloater) levelUpFloater.gameObject.SetActive(true);

            float t = 0f;
            Vector2 floaterStart = levelUpFloater ? levelUpFloater.anchoredPosition : Vector2.zero;
            var floaterImg = levelUpFloater ? levelUpFloater.GetComponent<Image>() : null;

            while (t < theme.levelUpDuration)
            {
                t += Time.deltaTime;
                float k = t / theme.levelUpDuration;

                // pip overshoot 0 -> 1.4 -> 1 over the first 120 ms
                if (pip)
                {
                    float p = Mathf.Clamp01(t / 0.12f);
                    float s = p < 1f ? Mathf.Lerp(0f, 1.4f, p) : 1f;
                    if (t > 0.12f) s = Mathf.Lerp(1.4f, 1f, Mathf.Clamp01((t - 0.12f) / 0.12f));
                    pip.rectTransform.localScale = Vector3.one * s;
                }
                // border pulse: green 2px -> 0 over 600 ms
                if (borderImage && !_data.pending)
                {
                    float b = 1f - Mathf.Clamp01(t / 0.6f);
                    borderImage.enabled = b > 0.01f;
                    var c = theme.levelColor; c.a = b;
                    borderImage.color = c;
                }
                // floater rises 32px and fades
                if (levelUpFloater)
                {
                    levelUpFloater.anchoredPosition = floaterStart + new Vector2(0f, 32f * k);
                    if (floaterImg)
                    {
                        var c = theme.levelColor; c.a = 1f - k; floaterImg.color = c;
                    }
                }
                yield return null;
            }

            if (pip) pip.rectTransform.localScale = Vector3.one;
            if (levelUpFloater)
            {
                levelUpFloater.anchoredPosition = floaterStart;
                levelUpFloater.gameObject.SetActive(false);
            }
            if (borderImage)
            {
                borderImage.enabled = _data.pending;
                borderImage.color = _data.pending ? theme.pendingBorderColor : theme.pillBorder;
            }
            _levelUpCo = null;
        }

        private void PlayReplaceFlash()
        {
            if (_flashCo != null) StopCoroutine(_flashCo);
            _flashCo = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (!pillImage) yield break;
            var baseColor = theme.pillFill;
            float t = 0f;
            while (t < theme.replaceFlashDuration)
            {
                t += Time.deltaTime;
                float k = 1f - (t / theme.replaceFlashDuration);
                pillImage.color = Color.Lerp(baseColor, new Color(1f, 1f, 1f, 0.35f), k);
                yield return null;
            }
            pillImage.color = baseColor;
            _flashCo = null;
        }

        /// <summary>
        /// Pending marker. D-IMPL-DASH: true marching-ants needs a material with
        /// a scrolling UV. If the assigned border sprite has no such material,
        /// this degrades to an alpha pulse — still an unmistakable "this row is
        /// not what you are hearing yet", with zero shader work.
        /// </summary>
        private void Update()
        {
            if (!_hasData || !_data.pending || borderImage == null) return;
            _pendingPhase += Time.deltaTime / Mathf.Max(0.01f, theme.pendingPulsePeriod);
            float a = Mathf.Lerp(0.35f, 0.85f, (Mathf.Sin(_pendingPhase * Mathf.PI * 2f) + 1f) * 0.5f);
            var c = theme.pendingBorderColor; c.a = a;
            borderImage.color = c;

            var mat = borderImage.material;
            if (mat != null && mat.HasProperty("_DashOffset"))
                mat.SetFloat("_DashOffset", _pendingPhase % 1f);
        }

        #endregion

        #region Hover (§5)

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hasData) return;

            // [HUD-COMP-1 fix] Re-resolve the instrument HERE, not at Bind.
            // Bind runs when the card is PLAYED, which is strictly before the
            // loop that renders it — at that moment no instrument is pinned yet,
            // so a bind-time value is empty for the most recently played track
            // and only fills in later, by accident, when some other play forces
            // a rebuild. Hover is the first moment the answer can be correct.
            if (!_data.placeholder)
            {
                var gm = GigManager.Instance;
                if (gm != null && gm.TryGetResolvedInstrumentNameForUI(
                        _data.partIndex, _data.musicianId, _data.role, out var inst))
                    _data.instrumentName = inst;
            }

            // Facts panel: exists for EVERY row, including empty ones — "no track
            // yet" is information, and a row that swallows hover teaches the
            // player that hovering is unreliable.
            TrackHoverPanel.Instance?.ShowForTrack(_data, rowRect);

            // Minicard: only when a real card backs the row. Anchored to the row
            // (not the cursor) so the panel + card open as one block over the
            // stage art, never over hand or audience.
            if (_data.sourceCard != null)
                MinicardTooltipController.Instance?.Show(_data.sourceCard, rowRect);
        }

        public void OnPointerExit(PointerEventData eventData) => HideHover();

        private void OnDisable() => HideHover();

        private void HideHover()
        {
            TrackHoverPanel.Instance?.Hide();
            MinicardTooltipController.Instance?.Hide();
        }

        #endregion

        #region Helpers

        private void EnsureRefs()
        {
            if (!rowRect) rowRect = transform as RectTransform;
            if (!cg) cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            if (!layoutElement) layoutElement = GetComponent<LayoutElement>()
                ?? gameObject.AddComponent<LayoutElement>();
            if (theme == null)
                Debug.LogError($"[SongTrackElementUI] No theme assigned on {name}. " +
                               "Assign CompositionStripTheme on the row prefab.");
        }

        private static Color Mul(Color c, float alphaScale)
            => new Color(c.r, c.g, c.b, c.a * alphaScale);

        #endregion
    }
}
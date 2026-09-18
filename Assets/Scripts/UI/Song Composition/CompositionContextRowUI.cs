// Placement: Assets/Scripts/UI/Song Composition/CompositionContextRowUI.cs  (NEW)
//
// [HUD-COMP-1] The single row above the tracks: loop pips + meter/tempo/mood chips.
//
// [SONG-PANEL-1b / F-SP1-7] Source is PURE ASCII and saved with a BOM: this file was
// Windows-1252 (six 0xB7 mid-dots, one 0x97 em-dash) and compiled only by the compiler's
// code-page fallback. The mid-dot is now the escape "\u00B7" (Sep). Same fix as
// TrackHoverPanel.cs (F-T6-1 / F-SP1-2): a BOM alone does not survive an editor round-trip,
// ASCII purity does.
//
// [SONG-PANEL-1b / F-SP1-8] Population 9: every WORD in the hover comes from
// ConceptGlossarySO via GameText.Format. What stays in code is punctuation (Sep,
// parentheses) and the Tonality enum name. STILL OPEN: meterLabel / tempoLabel / mood come
// from CompositionStripThemeSO -- asset-authored English, population 10, decision D-SP1-6.
// [TEXT-VIBE-1 / D-SP1-6=B] RESOLVED for meter and tempo: the label is a template keyed by the enum
// VALUE (GameTextTemplateIds.UiCtxMeter / UiCtxTempo); the theme asset carries no text for them.
// Census correction: "Happy" / "Sad" (F-SP1-13) were never asset text -- MoodTag is an ENUM printed
// by ToString(), so mood joins Tonality under F-TXT4-2 (CARD-FACE-1), not under population 10.
//
// This row replaces the old "SixEight - VerySlow - Ionian" string AND adds the
// one thing the current build never showed: how much of this part is left.
// That omission matters because the final loop LOCKS composition
// (UnplayableReason.FinalLoopLock) -- the player was being denied a play with no
// prior warning on screen.
//
// Shape carries the warning, not just color: the last pip is a DIAMOND, always.
// Red is the reinforcement, not the signal. A colorblind player still sees the
// silhouette change.

using ALWTTT.Data;
using ALWTTT.Enums;
using MidiGenPlay;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ALWTTT.Tooltips;                               // [SONG-PANEL-1b]
using static MidiGenPlay.MusicTheory.MusicTheory;
using static ALWTTT.Tooltips.GameTextTemplateIds;    // [SONG-PANEL-1b]

namespace ALWTTT.UI
{
    public class CompositionContextRowUI : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        #region Bind payload

        public struct ContextData
        {
            public int loopCurrent;      // 1-based
            public int loopTotal;
            public bool finalLoopLocks;  // session says composition is locked now

            public TimeSignature meter;
            public TempoRange tempo;
            public int? absoluteBpm;
            public float tempoScale;

            public bool hasRenderedTonality;   // false before the first render
            public Tonality tonality;
            public string rootLabel;           // "C", "F#", ... formatted by the seam

            public string partLabel;           // glossary part label ("Parte A")
            public int partIndex;              // 0-based
            public int partTotal;
            public int silentMusicians;        // density tier 3 hover addendum
        }

        #endregion

        #region Wiring

        [SerializeField] private CompositionStripThemeSO theme;
        [SerializeField] private RectTransform rowRect;

        [Header("Part (hidden while partsPerSong == 1)")]
        [SerializeField] private TMP_Text partLetterText;

        [Header("Loop pips")]
        [SerializeField] private RectTransform loopPipsRoot;
        [SerializeField] private GameObject loopPipPrefab;      // round pip
        [SerializeField] private GameObject loopPipFinalPrefab; // diamond pip

        [Header("Chips")]
        [SerializeField] private Image meterChip;
        [SerializeField] private Image tempoChip;
        [SerializeField] private Image moodChip;
        [SerializeField] private Image tempoOverrideDot;

        #endregion

        // U+00B7 mid dot: Latin-1, present in the LiberationSans SDF atlas. Escaped so the
        // SOURCE stays ASCII (see header). Punctuation stays in code, never in the glossary.
        private const string Sep = " \u00B7 ";

        private ContextData _data;
        private bool _hasData;
        private readonly System.Collections.Generic.List<GameObject> _pips = new();
        private float _pulse;

        #region Bind

        public void Bind(ContextData data)
        {
            _data = data;
            _hasData = true;

            // Part letter: with parts-per-song == 1 (demo cut) this conveys
            // nothing, so it does not get pixels. It is not deleted, because a
            // multi-part song makes it load-bearing again.
            if (partLetterText)
            {
                bool show = data.partTotal > 1;
                partLetterText.gameObject.SetActive(show);
                if (show) partLetterText.text = PartLetter(data.partIndex);
            }

            BuildLoopPips(data);

            ApplyChipSize(meterChip); ApplyChipSize(tempoChip); ApplyChipSize(moodChip);

            if (meterChip)
            {
                theme.TryGetMeter(data.meter, out var glyph);
                meterChip.sprite = glyph;
                meterChip.enabled = glyph != null;
            }
            if (tempoChip)
            {
                theme.TryGetTempo(data.tempo, out var icon);
                tempoChip.sprite = icon;
                tempoChip.enabled = icon != null;
            }
            if (tempoOverrideDot)
                tempoOverrideDot.enabled = data.absoluteBpm.HasValue;

            if (moodChip)
            {
                // D3=C: mood follows the RENDERED tonality. Before the first
                // render there is nothing honest to show, so we fall back to the
                // model and mark it "pending render" in hover rather than
                // asserting a mood we cannot back.
                theme.TryGetMood(data.tonality, out _, out var glyph, out var tint);
                moodChip.sprite = glyph;
                moodChip.color = data.hasRenderedTonality
                    ? tint
                    : new Color(tint.r, tint.g, tint.b, tint.a * 0.55f);
                moodChip.enabled = glyph != null;
            }

            if (rowRect && theme)
                rowRect.sizeDelta = new Vector2(rowRect.sizeDelta.x, theme.contextRowHeight);
        }

        private void ApplyChipSize(Image chip)
        {
            if (chip == null || theme == null) return;
            chip.rectTransform.sizeDelta = new Vector2(theme.chipSize, theme.chipSize);
        }

        public void SetWidth(float width)
        {
            if (!rowRect) return;
            rowRect.sizeDelta = new Vector2(width, rowRect.sizeDelta.y);
            var le = GetComponent<LayoutElement>();
            if (le) { le.preferredWidth = width; le.minWidth = width; }
        }

        private void BuildLoopPips(ContextData d)
        {
            if (!loopPipsRoot || !loopPipPrefab) return;

            int total = Mathf.Max(0, d.loopTotal);
            if (_pips.Count != total)
            {
                foreach (var p in _pips) if (p) Destroy(p);
                _pips.Clear();
                for (int i = 0; i < total; i++)
                {
                    bool isLast = i == total - 1;
                    var prefab = (isLast && loopPipFinalPrefab != null)
                        ? loopPipFinalPrefab : loopPipPrefab;
                    var go = Instantiate(prefab, loopPipsRoot);
                    go.SetActive(true);
                    // Size from the theme, not from the pip prefab: two prefabs
                    // (round + diamond) that must always match are two places to
                    // forget. One token keeps them identical by construction.
                    if (go.transform is RectTransform prt)
                        prt.sizeDelta = new Vector2(theme.loopPipSize, theme.loopPipSize);
                    _pips.Add(go);
                }
            }

            for (int i = 0; i < _pips.Count; i++)
            {
                var img = _pips[i] ? _pips[i].GetComponent<Image>() : null;
                if (!img) continue;
                bool isCurrent = (i + 1) == d.loopCurrent;
                bool isPast = (i + 1) < d.loopCurrent;
                bool isFinal = i == _pips.Count - 1;

                Color c;
                if (isFinal && isCurrent && d.finalLoopLocks) c = theme.finalLoopColor;
                else if (isPast || isCurrent) c = new Color(1f, 1f, 1f, 0.90f);
                else c = new Color(1f, 1f, 1f, 0.40f);
                img.color = c;
                img.transform.localScale = Vector3.one;
            }
        }

        // Halo pulse on the current pip. Cheap: one scale lerp, no allocation.
        private void Update()
        {
            if (!_hasData || _pips.Count == 0) return;
            int idx = Mathf.Clamp(_data.loopCurrent - 1, 0, _pips.Count - 1);
            var t = _pips[idx] ? _pips[idx].transform : null;
            if (!t) return;
            _pulse += Time.deltaTime / 1.2f;
            float s = 1f + 0.10f * (Mathf.Sin(_pulse * Mathf.PI * 2f) + 1f) * 0.5f;
            t.localScale = Vector3.one * s;
        }

        #endregion

        #region Hover

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hasData) return;
            TrackHoverPanel.Instance?.ShowRaw(BuildHoverText(), rowRect);
        }

        public void OnPointerExit(PointerEventData eventData)
            => TrackHoverPanel.Instance?.Hide();

        private void OnDisable() => TrackHoverPanel.Instance?.Hide();

        private string BuildHoverText()
        {
            var sb = new StringBuilder();
            // [SONG-PANEL-1c / F-SP1-11] loopTotal is 0 until TryGetLoopProgressForUI answers
            // (RefreshContextRow leaves the field at its default before the first render), and the
            // old line printed "Loop 1 de 0" - a count that asserts something false. With no total
            // there is nothing honest to say about the total, so it is not said. Same principle as
            // D3=C on the mood chip: before the render, do not assert what cannot be backed.
            sb.Append(_data.loopTotal > 0
                ? GameText.Format(UiCtxLoop, _data.loopCurrent, _data.loopTotal)
                : GameText.Format(UiCtxLoopNoTotal, _data.loopCurrent));
            if (_data.finalLoopLocks) sb.Append(Sep).Append(GameText.Format(UiCtxFinalLoopLocks));
            sb.AppendLine();

            // [TEXT-VIBE-1 / D-SP1-6=B] Population 10 RESOLVED for meter and tempo: the label is a
            // template keyed by the enum VALUE (ui_ctx_meter_<value> / ui_ctx_tempo_<value>), so the
            // theme asset no longer carries player text and the enum name cannot leak into the label
            // ("Four Four (FourFour)" was exactly that leak). Absolute BPM still replaces the range.
            string meterLabel = GameText.Format(UiCtxMeter(_data.meter));
            string tempoLabel = _data.absoluteBpm.HasValue
                ? GameText.Format(UiCtxBpmAbsolute, _data.absoluteBpm.Value)
                : GameText.Format(UiCtxTempo(_data.tempo));
            sb.Append(meterLabel).Append(Sep).Append(tempoLabel);
            if (!Mathf.Approximately(_data.tempoScale, 1f) && _data.tempoScale > 0f)
                sb.Append(Sep).Append("x").Append(GameText.Number(_data.tempoScale, "0.##"));
            sb.AppendLine();

            // mood (MoodTag) and _data.tonality (Tonality) are ENUM names (F-TXT4-2 -> CARD-FACE-1): not templates.
            theme.TryGetMood(_data.tonality, out var mood, out _, out _);
            sb.Append(mood).Append(Sep).Append(_data.tonality);
            if (!string.IsNullOrEmpty(_data.rootLabel)) sb.Append($" ({_data.rootLabel})");
            if (!_data.hasRenderedTonality) sb.Append(Sep).Append(GameText.Format(UiCtxPendingRender));

            if (_data.partTotal > 1)
                sb.AppendLine().Append(_data.partLabel).Append(Sep)
                  .Append(GameText.Format(UiCtxPartOf, _data.partIndex + 1, _data.partTotal));

            // [SONG-PANEL-1b] One/many split, as ui_card_modifier_one/many already does. The old
            // line printed "1 musicians silent": a grammar bug invisible while the string was a
            // literal, and unwritable once each count has its own template.
            if (_data.silentMusicians > 0)
                sb.AppendLine().Append(GameText.Format(
                    _data.silentMusicians == 1 ? UiCtxMusicianSilentOne : UiCtxMusiciansSilentMany,
                    _data.silentMusicians));

            return sb.ToString();
        }

        #endregion

        private static string PartLetter(int index)
            => index >= 0 && index < 26
                ? ((char)('A' + index)).ToString()
                : (index + 1).ToString();
    }
}
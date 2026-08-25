// Placement: Assets/Scripts/UI/Song Composition/CompositionContextRowUI.cs  (NEW)
//
// [HUD-COMP-1] The single row above the tracks: loop pips + meter/tempo/mood chips.
//
// This row replaces the old "SixEight - VerySlow - Ionian" string AND adds the
// one thing the current build never showed: how much of this part is left.
// That omission matters because the final loop LOCKS composition
// (UnplayableReason.FinalLoopLock) — the player was being denied a play with no
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
using static MidiGenPlay.MusicTheory.MusicTheory;

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

            public string partLabel;           // "Part A"
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

            if (meterChip)
            {
                theme.TryGetMeter(data.meter, out var glyph, out _);
                meterChip.sprite = glyph;
                meterChip.enabled = glyph != null;
            }
            if (tempoChip)
            {
                theme.TryGetTempo(data.tempo, out var icon, out _);
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
            sb.Append($"Loop {_data.loopCurrent} of {_data.loopTotal}");
            if (_data.finalLoopLocks) sb.Append(" · final loop locks composition");
            sb.AppendLine();

            theme.TryGetMeter(_data.meter, out _, out var meterLabel);
            theme.TryGetTempo(_data.tempo, out _, out var tempoLabel);
            if (_data.absoluteBpm.HasValue)
                tempoLabel = $"{_data.absoluteBpm.Value} BPM (absolute override)";
            sb.Append($"{meterLabel} · {tempoLabel}");
            if (!Mathf.Approximately(_data.tempoScale, 1f) && _data.tempoScale > 0f)
                sb.Append($" · x{_data.tempoScale:0.##}");
            sb.AppendLine();

            theme.TryGetMood(_data.tonality, out var mood, out _, out _);
            sb.Append($"{mood} · {_data.tonality}");
            if (!string.IsNullOrEmpty(_data.rootLabel)) sb.Append($" ({_data.rootLabel})");
            if (!_data.hasRenderedTonality) sb.Append(" · pending render");

            if (_data.partTotal > 1)
                sb.AppendLine().Append(
                    $"{_data.partLabel} · {_data.partIndex + 1} of {_data.partTotal}");

            if (_data.silentMusicians > 0)
                sb.AppendLine().Append($"{_data.silentMusicians} musicians silent");

            return sb.ToString();
        }

        #endregion

        private static string PartLetter(int index)
            => index >= 0 && index < 26
                ? ((char)('A' + index)).ToString()
                : (index + 1).ToString();
    }
}
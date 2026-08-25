// Placement: Assets/Scripts/Data/UI/CompositionStripThemeSO.cs
//
// [HUD-COMP-1 / D-IMPL-SO] Single authoring surface for the composition strip:
// role lexicon, meter glyphs, tempo icons, mood map, and layout tokens.
//
// DEVIATION FROM Composition_View_Spec.md §7 (stated, not silent): the spec
// listed four separate SOs. They are consolidated here because all four are
// consumed together by the same prefab, and three of the four fail SILENTLY
// when unassigned (a missing sprite just doesn't draw). One asset = one null
// check = one place to retune the palette.
//
// Enum->asset lookups are authored as LISTS, not dictionaries (Unity cannot
// serialize dictionaries) and cached into dictionaries on first access.
// Every lookup has a defined fallback: a package-side enum value we have never
// seen must degrade to a neutral chip, never to a null-ref.

using ALWTTT.Enums;
using MidiGenPlay;
using System.Collections.Generic;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.Data
{
    [CreateAssetMenu(
        fileName = "CompositionStripTheme",
        menuName = "ALWTTT/UI/Composition Strip Theme")]
    public class CompositionStripThemeSO : ScriptableObject
    {
        #region Authored entries

        [System.Serializable]
        public class RoleEntry
        {
            public TrackRole role;
            public Sprite icon;
            [Tooltip("Glyph tint ONLY. Never applied to the pill — pill/border " +
                     "colors are reserved for STATE (pending / level-up / final loop).")]
            public Color tint = Color.white;
            [Tooltip("Hover label. Defaults to the enum name when empty.")]
            public string displayName;
        }

        [System.Serializable]
        public class MeterEntry
        {
            public TimeSignature meter;
            public Sprite glyph;
            [Tooltip("Hover label, e.g. \"6/8\". Enum name is appended automatically.")]
            public string displayName;
        }

        [System.Serializable]
        public class TempoEntry
        {
            public TempoRange tempo;
            public Sprite icon;
            [Tooltip("Hover label, e.g. \"Very Slow\".")]
            public string displayName;
        }

        [System.Serializable]
        public class MoodEntry
        {
            public Tonality tonality;
            public MoodTag mood;
            public Sprite glyph;
            public Color tint = Color.white;
        }

        [Header("Track role lexicon (§2)")]
        [SerializeField] private List<RoleEntry> roles = new();

        [Header("Meter glyphs (§1.1.3)")]
        [SerializeField] private List<MeterEntry> meters = new();

        [Header("Tempo icons (§1.1.4)")]
        [SerializeField] private List<TempoEntry> tempos = new();

        [Header("Mood map — keyed on the RENDERED tonality (§1.3 / D3=C)")]
        [SerializeField] private List<MoodEntry> moods = new();

        [Header("Fallbacks (used for any enum value with no authored entry)")]
        [SerializeField] private Sprite unknownGlyph;
        [SerializeField] private Color unknownTint = new Color(0.60f, 0.63f, 0.69f, 1f);

        #endregion

        #region Layout tokens (§3) — retunable without recompiling

        [Header("Pill")]
        [Tooltip("D-IMPL-BLUR=B: no runtime blur. Legibility over the stage art " +
                 "comes from LOW LUMINANCE, not from defocus — the current build's " +
                 "panel fails because it is light, not because it is sharp.")]
        public Color pillFill = new Color(0.031f, 0.039f, 0.094f, 0.68f);
        public Color pillBorder = new Color(1f, 1f, 1f, 0.10f);
        public float pillRadius = 12f;      // consumed by the rounded sprite / material
        public float emptyPillRadius = 8f;

        [Header("Metrics (reference canvas 1920x1080)")]
        public float rowHeight = 44f;
        public float rowHeightDense = 36f;   // density tier 2
        public float contextRowHeight = 36f;
        public float emptyRowHeight = 26f;
        public float emptyRowHeightDense = 14f;
        public float rowGap = 6f;
        public float minWidth = 220f;
        public float maxWidth = 340f;
        public float maxWidthDense = 300f;
        public float roleIconSize = 22f;
        public float roleIconSizeDense = 18f;
        public float nameFontSize = 22f;
        public float nameFontSizeDense = 18f;
        public float paddingLeft = 12f;
        public float paddingRight = 14f;
        public float nameLeftInset = 46f;    // paddingLeft + icon + gap

        [Header("State colors")]
        public Color nameColor = new Color(1f, 1f, 1f, 0.92f);
        [Tooltip("Level-up / buff green.")]
        public Color levelColor = new Color(0.561f, 0.839f, 0.580f, 1f);
        [Tooltip("Final-loop danger. Also tints the diamond loop pip.")]
        public Color finalLoopColor = new Color(1f, 0.541f, 0.420f, 1f);
        [Tooltip("Pending-render border. White dashed — NOT the old orange text " +
                 "tint, which collided with gold = venue/SFX.")]
        public Color pendingBorderColor = new Color(1f, 1f, 1f, 0.70f);

        [Header("Animation (seconds)")]
        public float rowGrowDuration = 0.16f;
        public float widthDuration = 0.16f;
        public float levelUpDuration = 1.2f;
        public float replaceFlashDuration = 0.12f;
        public float pendingPulsePeriod = 1.0f;

        #endregion

        #region Lookups

        private Dictionary<TrackRole, RoleEntry> _roleMap;
        private Dictionary<TimeSignature, MeterEntry> _meterMap;
        private Dictionary<TempoRange, TempoEntry> _tempoMap;
        private Dictionary<Tonality, MoodEntry> _moodMap;

        private void OnEnable() => Invalidate();

        /// <summary>Drop cached maps. Call after editing the lists at runtime.</summary>
        public void Invalidate()
        {
            _roleMap = null; _meterMap = null; _tempoMap = null; _moodMap = null;
        }

        public bool TryGetRole(TrackRole role, out Sprite icon, out Color tint, out string label)
        {
            if (_roleMap == null)
            {
                _roleMap = new Dictionary<TrackRole, RoleEntry>();
                foreach (var e in roles) if (e != null) _roleMap[e.role] = e;
            }
            if (_roleMap.TryGetValue(role, out var entry) && entry.icon != null)
            {
                icon = entry.icon;
                tint = entry.tint;
                label = string.IsNullOrWhiteSpace(entry.displayName)
                    ? role.ToString() : entry.displayName;
                return true;
            }
            icon = unknownGlyph; tint = unknownTint; label = role.ToString();
            return false;
        }

        public bool TryGetMeter(TimeSignature meter, out Sprite glyph, out string label)
        {
            if (_meterMap == null)
            {
                _meterMap = new Dictionary<TimeSignature, MeterEntry>();
                foreach (var e in meters) if (e != null) _meterMap[e.meter] = e;
            }
            if (_meterMap.TryGetValue(meter, out var entry) && entry.glyph != null)
            {
                glyph = entry.glyph;
                label = string.IsNullOrWhiteSpace(entry.displayName)
                    ? meter.ToString() : $"{entry.displayName} ({meter})";
                return true;
            }
            glyph = unknownGlyph;
            label = meter.ToString();
            return false;
        }

        public bool TryGetTempo(TempoRange tempo, out Sprite icon, out string label)
        {
            if (_tempoMap == null)
            {
                _tempoMap = new Dictionary<TempoRange, TempoEntry>();
                foreach (var e in tempos) if (e != null) _tempoMap[e.tempo] = e;
            }
            if (_tempoMap.TryGetValue(tempo, out var entry) && entry.icon != null)
            {
                icon = entry.icon;
                label = string.IsNullOrWhiteSpace(entry.displayName)
                    ? tempo.ToString() : $"{entry.displayName} ({tempo})";
                return true;
            }
            icon = unknownGlyph;
            label = tempo.ToString();
            return false;
        }

        public bool TryGetMood(
            Tonality tonality, out MoodTag mood, out Sprite glyph, out Color tint)
        {
            if (_moodMap == null)
            {
                _moodMap = new Dictionary<Tonality, MoodEntry>();
                foreach (var e in moods) if (e != null) _moodMap[e.tonality] = e;
            }
            if (_moodMap.TryGetValue(tonality, out var entry))
            {
                mood = entry.mood;
                glyph = entry.glyph != null ? entry.glyph : unknownGlyph;
                tint = entry.tint;
                return true;
            }
            mood = MoodTag.Unknown; glyph = unknownGlyph; tint = unknownTint;
            return false;
        }

        #endregion
    }
}
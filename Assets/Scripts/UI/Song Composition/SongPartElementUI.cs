// Placement: Assets/Scripts/UI/Song Composition/SongPartElementUI.cs  (REPLACES existing file)
//
// [HUD-COMP-1] Owns one part's strip: the context row plus its track rows.
//
// PRESERVED FROM THE PREVIOUS VERSION (do not "simplify" these away):
//   - rows are keyed "musicianId|role" so one musician can host two rows
//     (Melody + Bassline). That is BASS-1 / D1=A.
//   - a roster musician with no track gets ONE placeholder row keyed by
//     musicianId alone, upgraded in place by the first AddOrUpdateTrack.
//   - pending granularity is per-musician: all of a musician's rows show
//     pending together (conservative-correct — a re-render refreshes the part).
//
// ADDED HERE:
//   - the context row (loop pips + meter/tempo/mood chips)
//   - a single shared width for every pill, recomputed after each bind
//   - the density tiers from spec §6
//   - per-row hover payload (musician name/icon, resolved instrument) pulled
//     from GigManager read-only seams, not from private session state.
//
// WHY WIDTH IS COMPUTED HERE AND NOT BY A LAYOUT GROUP: a ContentSizeFitter
// would give every pill its OWN width, producing a ragged right edge that reads
// as noise on a 4-8 row stack. The strip must look like one object. So we
// measure the widest name once and push that width to all rows.

using System.Collections.Generic;
using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Managers;
using MidiGenPlay;
using UnityEngine;

namespace ALWTTT.UI
{
    public class SongPartElementUI : MonoBehaviour
    {
        #region Wiring

        [Header("Theme")]
        [SerializeField] private CompositionStripThemeSO theme;

        [Header("Context row")]
        [SerializeField] private CompositionContextRowUI contextRow;

        [Header("Tracks")]
        [SerializeField] private Transform tracksRoot;
        [SerializeField] private SongTrackElementUI trackPrefab;

        [Header("Dev")]
        [SerializeField] private bool useLogs = false;

        #endregion

        #region State

        private readonly Dictionary<string, SongTrackElementUI> trackByMusicianRole = new();
        private readonly List<SongTrackElementUI> _rows = new();
        private List<string> rosterOrder = new();
        private SongCompositionUI.PartEntry boundModel;
        private HashSet<string> _boundPendingSet;
        private int _partIndex = -1;
        private int _partTotal = 1;

        private static string RowKey(string musicianId, string role) => $"{musicianId}|{role}";

        private void Log(string log) { if (useLogs) Debug.Log(log); }

        #endregion

        #region Public API

        /// <summary>
        /// Part identity. `total` decides whether the part letter earns pixels:
        /// with parts-per-song == 1 (the demo cut) "Part A" is pure noise, which
        /// is exactly why the owner asked for it to go. It is suppressed, not
        /// deleted — a multi-part song makes it load-bearing again.
        /// </summary>
        public void SetPartContext(int index, int total)
        {
            _partIndex = index;
            _partTotal = Mathf.Max(1, total);
            if (boundModel != null) RefreshContextRow(boundModel);
        }

        public void SetRosterOrder(List<string> order)
        {
            rosterOrder = order != null ? new List<string>(order) : new List<string>();
            if (boundModel != null) Bind(boundModel, _boundPendingSet);
        }

        public void Bind(
            SongCompositionUI.PartEntry model,
            HashSet<string> pendingMusicianIds = null)
        {
            boundModel = model;
            _boundPendingSet = pendingMusicianIds;

            foreach (Transform c in tracksRoot) Destroy(c.gameObject);
            trackByMusicianRole.Clear();
            _rows.Clear();

            if (rosterOrder != null && rosterOrder.Count > 0)
            {
                foreach (var id in rosterOrder)
                {
                    bool isPending = pendingMusicianIds != null && pendingMusicianIds.Contains(id);
                    var owned = model.tracks?.FindAll(x => x.musicianId == id);

                    if (owned == null || owned.Count == 0)
                    {
                        var ph = SpawnRow();
                        trackByMusicianRole[id] = ph;   // placeholder key
                        ph.Bind(BuildPlaceholderData(id), suppressFx: true);
                        continue;
                    }

                    foreach (var t in owned)
                    {
                        var ui = SpawnRow();
                        trackByMusicianRole[RowKey(id, t.role.ToString())] = ui;
                        ui.Bind(BuildRowData(t, isPending), suppressFx: true);
                    }
                }
            }
            else if (model.tracks != null)
            {
                // Fallback: roster unknown -> just render existing tracks.
                foreach (var t in model.tracks)
                {
                    var ui = SpawnRow();
                    trackByMusicianRole[RowKey(t.musicianId, t.role.ToString())] = ui;
                    bool isPending = pendingMusicianIds != null
                        && pendingMusicianIds.Contains(t.musicianId);
                    ui.Bind(BuildRowData(t, isPending), suppressFx: true);
                }
            }

            RefreshContextRow(model);
            RelayoutStrip();
        }

        public void AddOrUpdateTrack(
            string musicianId, TrackRole role, string info,
            int inspirationNext = 0, bool pending = false,
            CardDefinition sourceCard = null, int level = 1)
        {
            var key = RowKey(musicianId, role.ToString());

            if (!trackByMusicianRole.TryGetValue(key, out var trackUI))
            {
                // Placeholder row for this musician -> upgrade it in place.
                if (trackByMusicianRole.TryGetValue(musicianId, out trackUI))
                {
                    trackByMusicianRole.Remove(musicianId);
                    trackByMusicianRole[key] = trackUI;
                }
                else
                {
                    trackUI = SpawnRow();
                    trackByMusicianRole[key] = trackUI;
                    PlaceRowNearSiblings(trackUI, musicianId);
                }
            }

            var data = new SongTrackElementUI.RowData
            {
                role = role,
                musicianId = musicianId,
                info = info,
                level = Mathf.Max(1, level),
                maxLevel = 3,
                placeholder = false,
                pending = pending,
                inspirationNext = inspirationNext,
                sourceCard = sourceCard,
                bundleName = null,
                partIndex = Mathf.Max(0, _partIndex),
            };
            ResolveMusicianDisplay(musicianId, ref data);
            data.instrumentName = ResolveInstrumentName(musicianId, role);

            // suppressFx: false — this is the ONLY path where a genuine level
            // transition is observable, because the row object survives it.
            trackUI.Bind(data, suppressFx: false);

            if (boundModel != null) RefreshContextRow(boundModel);
            RelayoutStrip();
        }

        /// <summary>Called by the strip driver when loop / tonality changed.</summary>
        public void RefreshContext()
        {
            if (boundModel != null) RefreshContextRow(boundModel);
        }

        #endregion

        #region Row construction

        private SongTrackElementUI SpawnRow()
        {
            var ui = Instantiate(trackPrefab, tracksRoot);
            ui.gameObject.SetActive(true);
            _rows.Add(ui);
            return ui;
        }

        private SongTrackElementUI.RowData BuildPlaceholderData(string musicianId)
        {
            var d = new SongTrackElementUI.RowData
            {
                musicianId = musicianId,
                placeholder = true,
                level = 1,
                partIndex = Mathf.Max(0, _partIndex),
                maxLevel = 3,
            };
            ResolveMusicianDisplay(musicianId, ref d);
            return d;
        }

        private SongTrackElementUI.RowData BuildRowData(
            SongCompositionUI.TrackEntry t, bool isPending)
        {
            var d = new SongTrackElementUI.RowData
            {
                role = t.role,
                musicianId = t.musicianId,
                info = t.info,
                // R7 hook: replace with t.level once TrackEntry carries it.
                // Until then every track is Lv1, which draws no pips (D2), so
                // the strip is correct rather than merely non-crashing.
                level = 1,
                maxLevel = 3,
                placeholder = false,
                pending = isPending,
                inspirationNext = t.inspirationGenerated +
                    ALWTTT.Cards.Effects.AddInspirationPerLoopSpec.SumFor(t.sourceCardDefinition),
                sourceCard = t.sourceCardDefinition,
                bundleName = t.styleBundle != null ? t.styleBundle.name : null,
                partIndex = Mathf.Max(0, _partIndex),
            };
            ResolveMusicianDisplay(t.musicianId, ref d);
            d.instrumentName = ResolveInstrumentName(t.musicianId, t.role);
            return d;
        }

        private void PlaceRowNearSiblings(SongTrackElementUI trackUI, string musicianId)
        {
            int siblingIdx = -1;
            var prefix = musicianId + "|";
            foreach (var kv in trackByMusicianRole)
            {
                if (kv.Value == trackUI) continue;
                if (kv.Key == musicianId || kv.Key.StartsWith(prefix))
                    siblingIdx = Mathf.Max(siblingIdx, kv.Value.transform.GetSiblingIndex());
            }
            if (siblingIdx >= 0)
            {
                trackUI.transform.SetSiblingIndex(
                    Mathf.Min(siblingIdx + 1, tracksRoot.childCount - 1));
                return;
            }
            if (rosterOrder != null && rosterOrder.Count > 0)
            {
                int target = rosterOrder.IndexOf(musicianId);
                if (target >= 0)
                    trackUI.transform.SetSiblingIndex(
                        Mathf.Clamp(target, 0, tracksRoot.childCount - 1));
            }
        }

        #endregion

        #region Seams (read-only; the session stays encapsulated)

        private void ResolveMusicianDisplay(string musicianId, ref SongTrackElementUI.RowData d)
        {
            var gm = GigManager.Instance;
            if (gm != null && gm.TryGetMusicianDisplayById(
                    musicianId, out var icon, out var displayName))
            {
                d.musicianIcon = icon;
                d.musicianName = displayName;
            }
            else d.musicianName = musicianId;
        }

        private string ResolveInstrumentName(string musicianId, TrackRole role)
        {
            var gm = GigManager.Instance;
            // partIndex is required: the pin maps are per-part-config, and the
            // key depends on this track's instrument overrides, which only the
            // model row knows.
            return (gm != null && gm.TryGetResolvedInstrumentNameForUI(
                        Mathf.Max(0, _partIndex), musicianId, role, out var n)) ? n : "";
        }

        private void RefreshContextRow(SongCompositionUI.PartEntry model)
        {
            if (contextRow == null || model == null) return;
            var gm = GigManager.Instance;

            var d = new CompositionContextRowUI.ContextData
            {
                meter = model.timeSignature,
                tempo = model.tempoRangeOverride,
                absoluteBpm = model.absoluteBpmOverride,
                tempoScale = model.tempoScale,
                partLabel = model.label,
                partIndex = Mathf.Max(0, _partIndex),
                partTotal = _partTotal,
                loopCurrent = 1,
                loopTotal = 0,
                // Fall back to the MODEL tonality only until the first render.
                // After that the RENDERED value wins — they legitimately differ
                // when a Backing card adopts (the model still says Ionian while
                // the chords sound Lydian). Showing the model there would be a
                // lie precisely when the player needs the truth.
                tonality = model.tonality,
                hasRenderedTonality = false,
                rootLabel = null,
            };

            if (gm != null)
            {
                if (gm.TryGetLoopProgressForUI(out int cur, out int total, out bool locks))
                {
                    d.loopCurrent = cur; d.loopTotal = total; d.finalLoopLocks = locks;
                }
                if (gm.TryGetRenderedTonalityForUI(_partIndex, out var ton, out var root))
                {
                    d.tonality = ton; d.rootLabel = root; d.hasRenderedTonality = true;
                }
            }

            d.silentMusicians = CountPlaceholders();
            contextRow.Bind(d);
        }

        private int CountPlaceholders()
        {
            int n = 0;
            foreach (var r in _rows) if (r != null && r.IsPlaceholder) n++;
            return n;
        }

        #endregion

        #region Layout: one width, one density tier (§3 / §6)

        private void RelayoutStrip()
        {
            if (theme == null) return;

            var tier = ResolveTier(_rows.Count);
            foreach (var r in _rows) if (r) r.ApplyDensity(tier);

            float maxName = 0f;
            foreach (var r in _rows)
            {
                if (r == null || r.IsPlaceholder || r.NameText == null) continue;
                maxName = Mathf.Max(maxName, r.NameText.GetPreferredValues(r.NameText.text).x);
            }

            float ceiling = tier >= StripDensityTier.Compact
                ? theme.maxWidthDense : theme.maxWidth;
            float width = Mathf.Clamp(
                theme.nameLeftInset + maxName + theme.paddingRight + 26f, // 26 = level pip gutter
                theme.minWidth, ceiling);

            foreach (var r in _rows) if (r) r.SetWidth(width);
            if (contextRow) contextRow.SetWidth(width);
        }

        private static StripDensityTier ResolveTier(int rowCount)
        {
            if (rowCount >= 8) return StripDensityTier.PipsHoverOnly;
            if (rowCount >= 7) return StripDensityTier.HideEmpties;
            if (rowCount >= 6) return StripDensityTier.Compact;
            if (rowCount >= 5) return StripDensityTier.TightEmpties;
            return StripDensityTier.Normal;
        }

        #endregion
    }
}
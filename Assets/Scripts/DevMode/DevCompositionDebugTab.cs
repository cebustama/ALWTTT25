#if ALWTTT_DEV
using ALWTTT.Cards;                 // [CSV-3] CardDefinition for R2a card debug-play
using ALWTTT.Managers;
using ALWTTT.Music;
using ALWTTT.UI;
using MidiGenPlay;
using MidiGenPlay.Composition;
using MidiGenPlay.Composition.Phrases;
using MidiGenPlay.Interfaces;
using MidiGenPlay.Services;
using System;
using System.Collections.Generic;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.DevMode
{
    /// <summary>
    /// [DBG-C1] Dev Mode composition-debug tab — read side (D1=B):
    ///  - Handoff-intent fields (SongCompositionUI.TrackEntry) drawn every pass.
    ///  - Resolved fields (MidiMusicManager.LastResolvedByTrack) serial-polled.
    ///  - '*' convention per Design_Composition_Debug_Tab_v0_1 §3.1 (A1
    ///    CONFIRMED at DBG-C2 open: per-field refinement of the §3.1 prose —
    ///    '*' marks composer-picked truth; deterministic-from-intent values
    ///    render bare).
    ///  - Copy fingerprint, chd: dump, seed pin, infinite-loop toggle.
    ///
    /// [DBG-C2] Interactive write side (D-C2-1..4=A):
    ///  - Per-track pattern override dropdowns (Ask C): full-registry source
    ///    via IPatternRepository, TS-filtered to the current part, off-band
    ///    assets annotated (D-C2-2=A). Bassline is vetoed in the UI (bass
    ///    renders the shared progression — override Backing instead;
    ///    package-side Bassline entries are warn+ignore by contract).
    ///  - Roman field (Ask D): ChordProgressionRuntimeImporter.TryParseRoman
    ///    with part TS + editable tonality/measures/default-duration; verdict
    ///    surfaced verbatim, hard-fail applies nothing (D-C2-1=A). The built
    ///    instance is DontSave and NEVER persisted.
    ///  - R2a debug-play (D-C2-3=A): bumps the override stamp so the session
    ///    re-renders the part through the normal seeded PlaySinglePartLoop
    ///    path at the next loop start (hot-swap; resolved log refreshes via
    ///    LastRenderSerial).
    ///  - Cache interaction (D-C2-4=A): overrides are never part of any cache
    ///    key. MMM bypasses stem/bundle caches when overrides are supplied;
    ///    the session invalidates PartCache on stamp mismatch
    ///    (keepTempo + keepInstruments).
    ///
    /// [CSV-3] Additions:
    ///  - Resolved meter/tonality/root read line (DrawResolvedIdentityLine):
    ///    the TS/Tonality/Root the render ACTUALLY used, from MidiMusicManager's
    ///    LastRenderResolved* surface. Read-only; annotates ChordTrack step-2b
    ///    tonality alignment and flags any TS/Root drift from model intent.
    ///  - R2a CARD debug-play (DrawCardDebugPlaySection): inject any catalogue
    ///    card's MUSICAL side (no cost, no gameplay effects) via
    ///    CompositionSession.DevInjectCompositionCard (D-CSV-8=A, live model).
    ///    Distinct from the DBG-C2 "Re-render part now" button above, which is
    ///    the *pattern-override* re-render (§18.7 disambiguation).
    ///
    /// All override state lives on CompositionSession dev statics
    /// (DevPatternOverrides / DevOverrideStamp), reset at song boundary.
    /// Compiles only under ALWTTT_DEV; production footprint zero.
    /// </summary>
    internal static class DevCompositionDebugTab
    {
        private static Vector2 _scroll;

        // Dirty-cache for the resolved block (C1, unchanged).
        private static int _lastSerialSeen = -1;
        private static int _lastPartSeen = -1;
        private static bool _lastFullSeen;
        private static string _resolvedBlock = string.Empty;

        // ---- [DBG-C2] override-UI state ----
        private static PatternRepositoryResources _repo;
        private static MusicianTrackKey? _pickOpenFor;   // which track row's list is expanded
        private static Vector2 _pickScroll;
        private static bool _romanOpen;
        private static string _romanText = string.Empty;
        private static float _romanDefaultDur = 1f;
        private static int _romanMeasures;               // 0 = derive from durations
        private static Tonality _romanTonality;
        private static bool _romanTonalitySeeded;        // seeded once from the part
        private static List<string> _romanWarnings = new();
        private static ChordProgressionData _romanInstance; // DontSave; ours to Destroy
        private static bool _browseOpen;

        // ---- [CSV-3] R2a card debug-play state ----
        private static bool _r2aOpen;
        private static Vector2 _r2aScroll;
        private static int _r2aCardIdx = -1;
        private static int _r2aTargetIdx;
        private static string _r2aLastMsg = string.Empty;

        // ---- [CSV-2 / D-CSV-5=A] dev instrument-override state ----
        // Mechanism: write TrackEntry.overrideMelodicInstrument /
        // overridePercussionInstrument directly — the fields ALREADY
        // participate in trackInputsHash (SongConfigBuilder) so the MMM stem
        // cache is coherent by construction, and precedence inside FromUI is
        // identical to a card override. The record below exists only for
        // (a) the [dev] annotation in the intent log, (b) Clear-with-restore
        // of the pre-dev field values, and (c) stomp detection: if
        // SongCompositionUI.ApplyInstrumentEffect later writes the same track
        // (card play), the field no longer matches `applied`; the row shows
        // "superseded by card" and the record is dropped WITHOUT restoring —
        // card truth is newer and owns the field.
        // Cache interaction (refinement of D-CSV-5 forced by code): assign and
        // clear go through CompositionSession.DevInvalidateForInstrumentOverride,
        // which invalidates the part cache with keepInstruments=FALSE (mirror
        // of the instrument-card path). The pattern-stamp path preserves
        // resolved instruments and would re-feed the stale
        // cache.resolvedMelInstByTrack map into the render as
        // instrumentOverrides — wrong for instrument changes.
        // Session pins: BuildMelodicPinKey/PercussionPinKey return null while
        // an explicit override is set, so pins are skipped and the stale pin
        // survives in the map — which is exactly what makes Clear restore the
        // original voice byte-identically under a pinned seed.
        private struct DevInstRecord
        {
            public UnityEngine.Object applied;                 // what we wrote
            public MIDIInstrumentSO prevMelodic;
            public MIDIPercussionInstrumentSO prevPercussion;
            public bool prevHasType;
            public InstrumentType prevType;
        }
        private static readonly Dictionary<MusicianTrackKey, DevInstRecord>
            _devInst = new();
        private static InstrumentRepositoryResources _instRepo;
        private static MusicianTrackKey? _instPickOpenFor;
        private static Vector2 _instPickScroll;

        // Off-band annotation set (D-C2-2=A): assets reachable from the
        // part's assigned style bundles (direct override refs + palette
        // entries, per the DBG-2 display-metadata contract). Rebuilt when
        // the part changes.
        private static int _inBandPartSeen = -1;
        private static readonly HashSet<UnityEngine.Object> _inBand = new();

        public static void Draw()
        {
            var gm = GigManager.Instance;
            if (gm == null) { GUILayout.Label("GigManager.Instance is null."); return; }

            var mm = MidiMusicManager.Instance;
            if (mm == null) { GUILayout.Label("MidiMusicManager.Instance is null."); return; }

            var session = gm.CompositionSession;
            if (session == null || !session.IsActive)
            {
                // [CSV-2] TrackEntry fields die with the model at song end;
                // never let dev instrument records leak across songs.
                if (_devInst.Count > 0) _devInst.Clear();
                GUILayout.Label("No active CompositionSession (start a song).");
                DrawLastRenderHeader(mm);
                DrawResolvedIdentityLine(mm, null); // [CSV-3] last-known resolved identity
                return;
            }

            // [DBG-C1] Seed pin (reproducibility for the BC-gate byte-diff).
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Pinned seed:", GUILayout.Width(90));
                string cur = CompositionSession.DevPinnedSongSeed?.ToString() ?? "";
                string next = GUILayout.TextField(cur, GUILayout.Width(120));
                if (next != cur)
                    CompositionSession.DevPinnedSongSeed =
                        int.TryParse(next, out var s) ? s : (int?)null;
            }

            DrawInfiniteLoopToggle(session);
            GUILayout.Space(6);

            int partIndex = session.DevCurrentPartIndex;
            int loopsDone = session.DevLoopsTotalForPart - session.DevLoopsRemainingForPart;
            GUILayout.Label(
                $"Part {partIndex}  |  seed={session.DevSongSeed?.ToString() ?? "-"}  |  " +
                $"loop {loopsDone}/{session.DevLoopsTotalForPart}");
            DrawLastRenderHeader(mm);

            var ui = session.DevCompositionUI;
            var partEntry = (ui != null && ui.Model?.parts != null
                             && partIndex >= 0 && partIndex < ui.Model.parts.Count)
                ? ui.Model.parts[partIndex]
                : null;
            var tracks = partEntry?.tracks;

            // [CSV-3] Resolved meter/tonality/root the render actually used.
            DrawResolvedIdentityLine(mm, partEntry);

            // ---- [DBG-C2] OVERRIDES ----
            DrawOverridesSection(session, mm, partEntry, tracks, partIndex);
            GUILayout.Space(4);

            // ---- [CSV-3] R2a CARD debug-play ----
            DrawCardDebugPlaySection(session);
            GUILayout.Space(4);

            bool full = ResolveFullFlag(gm);
            RefreshResolvedIfDirty(mm, session, partIndex, full);

            GUILayout.Label("— INTENT (handoff) —", Bold());
            if (tracks == null || tracks.Count == 0)
                GUILayout.Label("(no tracks in current part)");
            else
                foreach (var t in tracks)
                {
                    // [CSV-2 / D-CSV-5=A] The field write is indistinguishable
                    // from card truth by design; the [dev-inst] suffix is the
                    // agreed disambiguator, applied here (dev-tab only) so
                    // GenerationDebugFormatter stays untouched.
                    string line = GenerationDebugFormatter.FormatIntentLine(t);
                    if (t != null && !string.IsNullOrEmpty(t.musicianId)
                        && _devInst.TryGetValue(
                            new MusicianTrackKey(t.musicianId, t.role), out var rec)
                        && (ReferenceEquals(rec.applied, t.overrideMelodicInstrument)
                            || ReferenceEquals(rec.applied, t.overridePercussionInstrument)))
                        line += "  [dev-inst]";
                    GUILayout.Label(line);
                }

            GUILayout.Space(4);

            GUILayout.Label("— RESOLVED (last render) —", Bold());
            GUILayout.Label(_resolvedBlock);

            if (GUILayout.Button("Copy part fingerprint"))
            {
                GUIUtility.systemCopyBuffer = GenerationDebugFormatter.BuildFingerprint(
                    seed: session.DevSongSeed,
                    partIndex: mm.LastRenderPartIndex,
                    bpm: mm.LastRenderBpm,
                    fromCache: mm.LastRenderFromCache,
                    resolved: mm.LastResolvedByTrack,
                    pinned: mm.LastPinnedByTrack);
                Debug.Log("<color=lime>[DevMode][CompDebug]</color> Fingerprint copied to clipboard.");
            }

            if (GUILayout.Button("Dump chd: timeline"))
            {
                var tl = MidiMusicManager.Instance.GetChordTimelineSnapshot();
                var sb = new System.Text.StringBuilder("[DBG-C1] chd: timeline\n");
                foreach (var ch in tl)
                {
                    sb.Append("ch=").Append(ch.Key).Append(": ");
                    foreach (var e in ch.Value)
                        sb.Append(e.Roman).Append('(').Append(e.Symbol).Append(") ");
                    sb.AppendLine();
                }
                Debug.Log(sb.ToString());
            }

            DrawCatalogBrowse();
        }

        // ===============================================================
        // [CSV-3] Resolved meter / tonality / root line
        // ===============================================================

        private static void DrawResolvedIdentityLine(
            MidiMusicManager mm, SongCompositionUI.PartEntry partEntry)
        {
            // Marker semantics:
            //  - "aligned from intent X" on Tonality = ChordTrackComposer
            //    step-2b alignment fired (progression constrained tonalities).
            //    Expected and informative — not a defect.
            //  - "DRIFT" on TS/Root = resolved differs from model intent, which
            //    today should be impossible. If it shows, that is itself a
            //    finding (record it).
            string tsNote = "", tonNote = "", rootNote = "";
            if (partEntry != null)
            {
                if (partEntry.timeSignature != mm.LastRenderResolvedTimeSignature)
                    tsNote = $"  (intent {partEntry.timeSignature} — DRIFT)";
                if (partEntry.tonality != mm.LastRenderResolvedTonality)
                    tonNote = $"  (aligned from intent {partEntry.tonality})";
                if (partEntry.rootNote != mm.LastRenderResolvedRootNote)
                    rootNote = $"  (intent {partEntry.rootNote} — DRIFT)";
            }

            var style = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
            GUILayout.Label(
                $"Resolved (last render): TS={mm.LastRenderResolvedTimeSignature}{tsNote}  |  " +
                $"Tonality={mm.LastRenderResolvedTonality}{tonNote}  |  " +
                $"Root={mm.LastRenderResolvedRootNote}{rootNote}", style);
        }

        // ===============================================================
        // [CSV-3] R2a card debug-play section (musical side only)
        // ===============================================================

        private static void DrawCardDebugPlaySection(CompositionSession session)
        {
            _r2aOpen = GUILayout.Toggle(_r2aOpen,
                " R2a card debug-play (catalogue card, musical side only — no cost, no effects)");
            if (!_r2aOpen) return;

            // Same source this dev's Catalogue tab spawns from (band union
            // with AllCardsList fallback), filtered to composition cards.
            var all = DevCardCatalogueTab.GetBandUnionOrFallback(out var sourceLabel);
            var comps = new List<CardDefinition>();
            if (all != null)
                for (int i = 0; i < all.Count; i++)
                    if (all[i] != null && all[i].IsComposition) comps.Add(all[i]);

            if (comps.Count == 0)
            {
                GUILayout.Label($"No composition cards available ({sourceLabel}).");
                return;
            }

            GUILayout.Label($"source: {sourceLabel}",
                new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Italic });

            // Card pick list.
            _r2aCardIdx = Mathf.Clamp(_r2aCardIdx, -1, comps.Count - 1);
            _r2aScroll = GUILayout.BeginScrollView(_r2aScroll, GUILayout.Height(110));
            for (int i = 0; i < comps.Count; i++)
            {
                bool sel = i == _r2aCardIdx;
                string label = (sel ? "▶ " : "   ") +
                    (string.IsNullOrEmpty(comps[i].DisplayName) ? comps[i].name : comps[i].DisplayName);
                if (GUILayout.Button(label))
                    _r2aCardIdx = sel ? -1 : i;
            }
            GUILayout.EndScrollView();

            if (_r2aCardIdx < 0)
            {
                GUILayout.Label("(pick a card to audition)");
                return;
            }

            var def = comps[_r2aCardIdx];
            var comp = def.CompositionPayload;
            string targetId = null;

            // Target picker: only for musician-targeting cards that are NOT
            // fixed-performer (those auto-resolve by type inside DevInject).
            if (comp != null && comp.RequiresMusicianTarget)
            {
                if (def.RequiresFixedPerformer)
                {
                    GUILayout.Label($"Target: fixed performer ({def.FixedPerformerType}) — auto-resolved.");
                }
                else
                {
                    var band = session.DevBand;
                    if (band != null && band.Count > 0)
                    {
                        _r2aTargetIdx = Mathf.Clamp(_r2aTargetIdx, 0, band.Count - 1);
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Target:", GUILayout.Width(50));
                            for (int i = 0; i < band.Count; i++)
                            {
                                string nm = band[i]?.MusicianCharacterData?.CharacterName ?? $"#{i}";
                                if (GUILayout.Toggle(_r2aTargetIdx == i, nm, "Button"))
                                    _r2aTargetIdx = i;
                            }
                        }
                        targetId = band[_r2aTargetIdx]?.MusicianCharacterData?.CharacterId;
                    }
                    else
                    {
                        GUILayout.Label("Target required but no band available.");
                    }
                }
            }

            if (GUILayout.Button($"Inject musical side of " +
                $"'{(string.IsNullOrEmpty(def.DisplayName) ? def.name : def.DisplayName)}'"))
            {
                bool ok = session.DevInjectCompositionCard(def, targetId, out var reason);
                _r2aLastMsg = ok
                    ? $"Injected — audible at next loop start."
                    : $"Refused: {reason}";
            }

            if (!string.IsNullOrEmpty(_r2aLastMsg))
                GUILayout.Label(_r2aLastMsg,
                    new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true });

            if (!CompositionSession.DevInfiniteCompositionLoop)
                GUILayout.Label("Tip: enable Infinite composition loop above so auditions keep looping.",
                    new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic, fontSize = 11 });
        }

        // ===============================================================
        // [DBG-C2] Overrides section
        // ===============================================================

        private static void DrawOverridesSection(
            CompositionSession session, MidiMusicManager mm,
            SongCompositionUI.PartEntry partEntry,
            List<SongCompositionUI.TrackEntry> tracks, int partIndex)
        {
            GUILayout.Label("— OVERRIDES (dev, per-render) —", Bold());

            if (partEntry == null || tracks == null || tracks.Count == 0)
            {
                GUILayout.Label("(no tracks in current part — overrides unavailable)");
                return;
            }

            EnsureRepo(session);
            RebuildInBandIfDirty(tracks, partIndex);

            foreach (var t in tracks)
            {
                if (t == null || string.IsNullOrEmpty(t.musicianId)) continue;
                var key = new MusicianTrackKey(t.musicianId, t.role);

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label($"{t.role}[{t.musicianId}]", GUILayout.Width(160));

                    if (t.role == TrackRole.Bassline)
                    {
                        // Handoff (c)3: never target Bassline. Package-side it
                        // is warn+ignore; the UI vetoes it outright.
                        bool prevEnabled = GUI.enabled;
                        GUI.enabled = false;
                        GUILayout.Label("(no direct override — bass renders the shared " +
                                        "progression; override Backing instead)");
                        GUI.enabled = prevEnabled;
                        continue;
                    }
                    if (t.role == TrackRole.Harmony)
                    {
                        bool prevEnabled = GUI.enabled;
                        GUI.enabled = false;
                        GUILayout.Label("(Harmony has no pattern-override channel in v1)");
                        GUI.enabled = prevEnabled;
                        continue;
                    }

                    bool assigned = CompositionSession.DevPatternOverrides
                        .TryGetValue(key, out var cur) && cur != null;
                    GUILayout.Label(assigned ? $"→ {cur.name}" : "→ (none)",
                        GUILayout.MinWidth(140));

                    bool open = _pickOpenFor.HasValue && _pickOpenFor.Value.Equals(key);
                    if (GUILayout.Button(open ? "Close" : "Pick…", GUILayout.Width(52)))
                    {
                        _pickOpenFor = open ? (MusicianTrackKey?)null : key;
                        _pickScroll = Vector2.zero;
                    }
                    if (assigned && GUILayout.Button("Clear", GUILayout.Width(48)))
                        ClearOverride(key);
                }

                if (_pickOpenFor.HasValue && _pickOpenFor.Value.Equals(new MusicianTrackKey(t.musicianId, t.role)))
                    DrawPickList(t.role, new MusicianTrackKey(t.musicianId, t.role), partEntry.timeSignature);
            }

            DrawInstrumentOverridesSection(session, tracks, partIndex);

            DrawRomanSection(partEntry, tracks);

            GUILayout.Space(2);
            using (new GUILayout.HorizontalScope())
            {
                // [DBG-C2 / D-C2-3=A] R2a debug-play: force a fresh render of
                // the current part through the normal session path at the next
                // loop start. With a pinned seed the result is bit-reproducible;
                // the resolved log refreshes via LastRenderSerial either way.
                // NOTE: this is the *pattern-override* re-render — distinct from
                // the [CSV-3] R2a *card* debug-play section above (§18.7).
                if (GUILayout.Button("Re-render part now (applies at next loop start)"))
                {
                    CompositionSession.DevBumpOverrideStamp();
                    Debug.Log("<color=lime>[DBG-C2][R2a]</color> Override stamp bumped — " +
                              $"part {partIndex} re-renders at next loop start " +
                              $"(seed={session.DevSongSeed?.ToString() ?? "unpinned"}).");
                }
                if ((CompositionSession.DevPatternOverrides.Count > 0
                     || _devInst.Count > 0) &&
                    GUILayout.Button("Clear ALL overrides", GUILayout.Width(140)))
                {
                    ClearAllOverrides();
                    ClearAllInstrumentOverrides(session, tracks, partIndex); // [CSV-2]
                }
            }
            if (!CompositionSession.DevInfiniteCompositionLoop)
            {
                var style = new GUIStyle(GUI.skin.label)
                { fontStyle = FontStyle.Italic, fontSize = 11 };
                GUILayout.Label("Tip: turn on Infinite composition loop above to iterate " +
                                "overrides quickly.", style);
            }
        }

        private static void DrawPickList(
            TrackRole role, MusicianTrackKey key, TimeSignature ts)
        {
            IReadOnlyList<PatternDataSO> candidates = role switch
            {
                TrackRole.Rhythm => AsPatternList(_repo.GetDrumPatterns(ts)),
                TrackRole.Backing => AsPatternList(_repo.GetChordProgressions(ts)),
                TrackRole.Melody => AsPatternList(_repo.GetMelodyPatterns(ts)),
                _ => Array.Empty<PatternDataSO>()
            };

            if (candidates.Count == 0)
            {
                GUILayout.Label($"  (no {role} patterns found for TS={ts} — " +
                                "check Resources roots / Refresh)");
                // [CSV-3 / D-CSV-13=A] The Backing dropdown is repository-fed
                // (PatternRepositoryResources), kept runtime-honest. Local chord
                // content is off-root until the CSV-5 scan-root fix (D-CSV-14),
                // so this list is empty/small by measurement, not by bug — use
                // the Roman free-text override below meanwhile.
                if (role == TrackRole.Backing)
                    GUILayout.Label("  (Backing is repository-fed; local progressions are " +
                                    "off-root until the CSV-5 fix — use Roman override below)",
                        new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Italic });
                if (GUILayout.Button("Refresh catalog", GUILayout.Width(110)))
                    _repo.Refresh();
                return;
            }

            _pickScroll = GUILayout.BeginScrollView(_pickScroll, GUILayout.Height(90));
            foreach (var p in candidates)
            {
                if (p == null) continue;
                string label = _inBand.Contains(p) ? p.name : $"{p.name}  (off-band)";
                if (GUILayout.Button(label))
                {
                    AssignOverride(key, p);
                    _pickOpenFor = null;
                }
            }
            GUILayout.EndScrollView();
        }

        private static void DrawRomanSection(
            SongCompositionUI.PartEntry partEntry,
            List<SongCompositionUI.TrackEntry> tracks)
        {
            _romanOpen = GUILayout.Toggle(_romanOpen,
                " Roman progression → Backing override (Ask D)");
            if (!_romanOpen) return;

            if (!_romanTonalitySeeded)
            {
                _romanTonality = partEntry.tonality;
                _romanTonalitySeeded = true;
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Roman:", GUILayout.Width(60));
                _romanText = GUILayout.TextField(_romanText, GUILayout.MinWidth(220));
            }
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"TS={partEntry.timeSignature} (part)", GUILayout.Width(110));

                GUILayout.Label("Tonality:", GUILayout.Width(60));
                var tonValues = (Tonality[])Enum.GetValues(typeof(Tonality));
                int ti = Array.IndexOf(tonValues, _romanTonality);
                if (GUILayout.Button("<", GUILayout.Width(22)))
                    _romanTonality = tonValues[(ti - 1 + tonValues.Length) % tonValues.Length];
                GUILayout.Label(_romanTonality.ToString(), GUILayout.Width(90));
                if (GUILayout.Button(">", GUILayout.Width(22)))
                    _romanTonality = tonValues[(ti + 1) % tonValues.Length];

                GUILayout.Label("defDur:", GUILayout.Width(46));
                string dd = GUILayout.TextField(
                    _romanDefaultDur.ToString("0.##"), GUILayout.Width(44));
                if (float.TryParse(dd, out var ddv)) _romanDefaultDur = ddv;

                GUILayout.Label("measures(0=derive):", GUILayout.Width(120));
                string mstr = GUILayout.TextField(_romanMeasures.ToString(), GUILayout.Width(34));
                if (int.TryParse(mstr, out var mv)) _romanMeasures = mv;
            }

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply to Backing", GUILayout.Width(120)))
                    ApplyRoman(partEntry, tracks);
                if (_romanInstance != null &&
                    GUILayout.Button("Clear Roman override", GUILayout.Width(150)))
                    ClearRoman(tracks);
            }

            foreach (var w in _romanWarnings)
            {
                var warnStyle = new GUIStyle(GUI.skin.label)
                { fontSize = 11, wordWrap = true };
                warnStyle.normal.textColor = new Color(1f, 0.75f, 0.4f);
                GUILayout.Label("⚠ " + w, warnStyle);
            }
        }

        private static void ApplyRoman(
            SongCompositionUI.PartEntry partEntry,
            List<SongCompositionUI.TrackEntry> tracks)
        {
            _romanWarnings.Clear();

            var backing = tracks.Find(t =>
                t != null && t.role == TrackRole.Backing && !string.IsNullOrEmpty(t.musicianId));
            if (backing == null)
            {
                _romanWarnings.Add("No Backing track in the current part — nothing to override.");
                return;
            }

            // [DBG-C2 / D-C2-1=A] Importer verdict verbatim: hard-fail applies
            // nothing; warnings (fatal or not) surface unmodified. No
            // ALWTTT-side reduction — the D-L4.5 zero-warning guard is the
            // package's policy and we do not reinterpret it.
            bool ok = ChordProgressionRuntimeImporter.TryParseRoman(
                _romanText,
                partEntry.timeSignature,
                _romanMeasures,
                _romanDefaultDur,
                _romanTonality,
                out var data,
                out var warnings);
            if (warnings != null) _romanWarnings.AddRange(warnings);

            if (!ok || data == null)
            {
                Debug.Log("<color=orange>[DBG-C2][Roman]</color> Rejected: " +
                          string.Join(" | ", _romanWarnings));
                return; // nothing applied (D-C2-1=A)
            }

            var key = new MusicianTrackKey(backing.musicianId, TrackRole.Backing);

            // Replace any previous runtime instance we created (dev hygiene;
            // the instance is DontSave and never persisted — Ask D contract).
            if (_romanInstance != null && !ReferenceEquals(_romanInstance, data))
                UnityEngine.Object.Destroy(_romanInstance);
            _romanInstance = data;

            AssignOverride(key, data);
            Debug.Log($"<color=lime>[DBG-C2][Roman]</color> Applied '{data.name}' " +
                      $"to {key.MusicianId}:{key.Role} (non-fatal warnings: " +
                      $"{_romanWarnings.Count}).");
        }

        private static void ClearRoman(List<SongCompositionUI.TrackEntry> tracks)
        {
            var backing = tracks?.Find(t => t != null && t.role == TrackRole.Backing);
            if (backing != null)
                ClearOverride(new MusicianTrackKey(backing.musicianId, TrackRole.Backing));
            if (_romanInstance != null)
            {
                UnityEngine.Object.Destroy(_romanInstance);
                _romanInstance = null;
            }
            _romanWarnings.Clear();
        }

        private static void AssignOverride(MusicianTrackKey key, PatternDataSO asset)
        {
            CompositionSession.DevPatternOverrides[key] = asset;
            CompositionSession.DevBumpOverrideStamp();
            Debug.Log($"<color=lime>[DBG-C2]</color> Override set: {key.MusicianId}:{key.Role} " +
                      $"→ '{asset.name}' (applies at next loop start).");
        }

        private static void ClearOverride(MusicianTrackKey key)
        {
            if (CompositionSession.DevPatternOverrides.Remove(key))
            {
                CompositionSession.DevBumpOverrideStamp();
                Debug.Log($"<color=lime>[DBG-C2]</color> Override cleared: " +
                          $"{key.MusicianId}:{key.Role}.");
            }
        }

        private static void ClearAllOverrides()
        {
            CompositionSession.DevPatternOverrides.Clear();
            CompositionSession.DevBumpOverrideStamp();
            if (_romanInstance != null)
            {
                UnityEngine.Object.Destroy(_romanInstance);
                _romanInstance = null;
            }
            _romanWarnings.Clear();
            Debug.Log("<color=lime>[DBG-C2]</color> All overrides cleared.");
        }

        // ===============================================================
        // [CSV-2 / D-CSV-5=A] Instrument overrides section
        // ===============================================================

        private static void DrawInstrumentOverridesSection(
            CompositionSession session,
            List<SongCompositionUI.TrackEntry> tracks, int partIndex)
        {
            GUILayout.Label("— INSTRUMENT OVERRIDES (dev, per-render) —", Bold());
            EnsureInstrumentRepo(session);

            foreach (var t in tracks)
            {
                if (t == null || string.IsNullOrEmpty(t.musicianId)) continue;
                var key = new MusicianTrackKey(t.musicianId, t.role);
                bool percTrack = t.role == TrackRole.Rhythm;

                bool hasRecord = _devInst.TryGetValue(key, out var rec);
                UnityEngine.Object currentField = percTrack
                    ? (UnityEngine.Object)t.overridePercussionInstrument
                    : t.overrideMelodicInstrument;

                // Stomp detection: a card wrote the field after us. Drop the
                // record without restoring — card truth is newer (D-CSV-5=A).
                bool superseded = hasRecord && !ReferenceEquals(rec.applied, currentField);
                if (superseded)
                {
                    _devInst.Remove(key);
                    hasRecord = false;
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label($"{t.role}[{t.musicianId}]", GUILayout.Width(160));

                    string state =
                        hasRecord ? $"→ [dev] {rec.applied.name}" :
                        currentField != null ? $"→ (card) {currentField.name}" :
                        t.hasOverrideInstrumentType
                            ? $"→ (card, type) {t.overrideInstrumentType}" :
                        "→ (none)";
                    if (superseded) state += "  (dev override superseded by card)";
                    GUILayout.Label(state, GUILayout.MinWidth(180));

                    bool open = _instPickOpenFor.HasValue && _instPickOpenFor.Value.Equals(key);
                    if (GUILayout.Button(open ? "Close" : "Pick…", GUILayout.Width(52)))
                    {
                        _instPickOpenFor = open ? (MusicianTrackKey?)null : key;
                        _instPickScroll = Vector2.zero;
                    }
                    if (hasRecord && GUILayout.Button("Clear", GUILayout.Width(48)))
                        ClearInstrumentOverride(session, key, t, partIndex);
                }

                if (_instPickOpenFor.HasValue && _instPickOpenFor.Value.Equals(key))
                    DrawInstrumentPickList(session, key, t, partIndex, percTrack);
            }
        }

        private static void DrawInstrumentPickList(
            CompositionSession session, MusicianTrackKey key,
            SongCompositionUI.TrackEntry t, int partIndex, bool percTrack)
        {
            // Full catalog for counterfactual probing (batch task 6). Melodic
            // entries outside InstrumentRules.GetPermittedMelodic(musician,
            // role) are annotated, not hidden. Percussion has no permitted
            // rule in v1 — the full percussion catalog is offered unannotated.
            _instPickScroll = GUILayout.BeginScrollView(_instPickScroll, GUILayout.Height(110));

            if (percTrack)
            {
                var all = _instRepo.GetPercussionInstruments();
                if (all == null || all.Count == 0)
                    GUILayout.Label("  (no percussion instruments found — check " +
                                    "resourcesInstrumentsPath / package root)");
                else
                    foreach (var p in all)
                    {
                        if (p == null) continue;
                        if (GUILayout.Button(p.name))
                        {
                            AssignInstrumentOverride(session, key, t, partIndex,
                                melodic: null, percussion: p);
                            _instPickOpenFor = null;
                        }
                    }
            }
            else
            {
                var all = _instRepo.GetMelodicInstruments();
                var permitted = new HashSet<MIDIInstrumentSO>();
                var mus = session.DevResolveMusicianById(key.MusicianId);
                var pool = InstrumentRules.GetPermittedMelodic(mus, key.Role, _instRepo);
                if (pool != null)
                    foreach (var i in pool) if (i != null) permitted.Add(i);

                if (all == null || all.Count == 0)
                    GUILayout.Label("  (no melodic instruments found — check " +
                                    "resourcesInstrumentsPath / package root)");
                else
                    foreach (var i in all)
                    {
                        if (i == null || i is MIDIPercussionInstrumentSO) continue;
                        string label = permitted.Contains(i)
                            ? i.name
                            : $"{i.name}  (outside permitted set)";
                        if (GUILayout.Button(label))
                        {
                            AssignInstrumentOverride(session, key, t, partIndex,
                                melodic: i, percussion: null);
                            _instPickOpenFor = null;
                        }
                    }
            }
            GUILayout.EndScrollView();
        }

        private static void AssignInstrumentOverride(
            CompositionSession session, MusicianTrackKey key,
            SongCompositionUI.TrackEntry t, int partIndex,
            MIDIInstrumentSO melodic, MIDIPercussionInstrumentSO percussion)
        {
            // Capture pre-dev field state once (first dev touch on this key)
            // so Clear can restore card truth, not just null.
            if (!_devInst.ContainsKey(key))
                _devInst[key] = new DevInstRecord
                {
                    prevMelodic = t.overrideMelodicInstrument,
                    prevPercussion = t.overridePercussionInstrument,
                    prevHasType = t.hasOverrideInstrumentType,
                    prevType = t.overrideInstrumentType
                };

            // Exclusive-set discipline, mirroring ApplyInstrumentEffect.
            t.overrideMelodicInstrument = melodic;
            t.overridePercussionInstrument = percussion;
            t.hasOverrideInstrumentType = false;

            var rec = _devInst[key];
            rec.applied = (UnityEngine.Object)melodic ?? percussion;
            _devInst[key] = rec;

            // keepInstruments=FALSE invalidation + stamp bump (see state-block
            // comment). Re-render happens at next loop start via the normal
            // seeded path — identical UX to pattern overrides.
            session.DevInvalidateForInstrumentOverride(partIndex);
            Debug.Log($"<color=lime>[CSV-2]</color> Dev instrument override set: " +
                      $"{key.MusicianId}:{key.Role} → '{rec.applied.name}' " +
                      "(applies at next loop start).");
        }

        private static void ClearInstrumentOverride(
            CompositionSession session, MusicianTrackKey key,
            SongCompositionUI.TrackEntry t, int partIndex)
        {
            if (!_devInst.TryGetValue(key, out var rec)) return;
            t.overrideMelodicInstrument = rec.prevMelodic;
            t.overridePercussionInstrument = rec.prevPercussion;
            t.hasOverrideInstrumentType = rec.prevHasType;
            t.overrideInstrumentType = rec.prevType;
            _devInst.Remove(key);
            session.DevInvalidateForInstrumentOverride(partIndex);
            Debug.Log($"<color=lime>[CSV-2]</color> Dev instrument override cleared: " +
                      $"{key.MusicianId}:{key.Role} (previous field state restored).");
        }

        private static void ClearAllInstrumentOverrides(
            CompositionSession session,
            List<SongCompositionUI.TrackEntry> tracks, int partIndex)
        {
            if (_devInst.Count == 0 || tracks == null) return;
            foreach (var t in tracks)
            {
                if (t == null || string.IsNullOrEmpty(t.musicianId)) continue;
                var key = new MusicianTrackKey(t.musicianId, t.role);
                if (_devInst.TryGetValue(key, out var rec))
                {
                    // Restore only if we still own the field; if a card
                    // superseded us, leave card truth alone.
                    UnityEngine.Object currentField = t.role == TrackRole.Rhythm
                        ? (UnityEngine.Object)t.overridePercussionInstrument
                        : t.overrideMelodicInstrument;
                    if (ReferenceEquals(rec.applied, currentField))
                    {
                        t.overrideMelodicInstrument = rec.prevMelodic;
                        t.overridePercussionInstrument = rec.prevPercussion;
                        t.hasOverrideInstrumentType = rec.prevHasType;
                        t.overrideInstrumentType = rec.prevType;
                    }
                    _devInst.Remove(key);
                }
            }
            _devInst.Clear(); // records for tracks no longer present
            session.DevInvalidateForInstrumentOverride(partIndex);
            Debug.Log("<color=lime>[CSV-2]</color> All dev instrument overrides cleared.");
        }

        private static void EnsureInstrumentRepo(CompositionSession session)
        {
            if (_instRepo != null) return;
            _instRepo = new InstrumentRepositoryResources(session.DevMidiConfig);
            _instRepo.Refresh();
        }

        // ===============================================================
        // [DBG-C2] Catalog plumbing
        // ===============================================================

        private static void EnsureRepo(CompositionSession session)
        {
            if (_repo != null) return;
            _repo = new PatternRepositoryResources(session.DevMidiConfig);
            _repo.Refresh();
        }

        private static IReadOnlyList<PatternDataSO> AsPatternList<T>(IReadOnlyList<T> src)
            where T : PatternDataSO
        {
            if (src == null || src.Count == 0) return Array.Empty<PatternDataSO>();
            var list = new List<PatternDataSO>(src.Count);
            foreach (var p in src) list.Add(p);
            return list;
        }

        // [D-C2-2=A] In-band = reachable from the part's assigned bundles:
        // direct override refs + palette entries (both are DBG-2 contract
        // display metadata; no package internals are interpreted here).
        private static void RebuildInBandIfDirty(
            List<SongCompositionUI.TrackEntry> tracks, int partIndex)
        {
            if (partIndex == _inBandPartSeen) return;
            _inBandPartSeen = partIndex;
            _inBand.Clear();

            foreach (var t in tracks)
            {
                switch (t?.styleBundle)
                {
                    case RhythmCardConfigSO r:
                        if (r.patternOverride != null) _inBand.Add(r.patternOverride);
                        if (r.patternPalette?.entries != null)
                            foreach (var e in r.patternPalette.entries)
                                if (e?.pattern != null) _inBand.Add(e.pattern);
                        break;
                    case BackingCardConfigSO b:
                        if (b.progressionOverride != null) _inBand.Add(b.progressionOverride);
                        if (b.progressionPalette?.entries != null)
                            foreach (var e in b.progressionPalette.entries)
                                if (e?.progression != null) _inBand.Add(e.progression);
                        break;
                    case MelodyCardConfigSO m:
                        if (m.patternOverride != null) _inBand.Add(m.patternOverride);
                        break;
                }
            }
        }

        // [Ask B, informational] Palette / phrase-vocabulary enumeration via
        // the documented stores. Degrades to a note while the two package
        // asset moves (E-1b/E-2b) are pending.
        private static void DrawCatalogBrowse()
        {
            _browseOpen = GUILayout.Toggle(_browseOpen, " Catalog browse (Ask B)");
            if (!_browseOpen) return;

            var drumPal = new TrackPatternConfigStoreResources<DrumPatternPaletteSO>("Drums");
            var chordPal = new TrackPatternConfigStoreResources<ChordProgressionPaletteSO>("Chords");
            var phrasePal = new TrackPatternConfigStoreResources<PhrasePaletteSO>("Phrases");
            drumPal.Refresh(); chordPal.Refresh(); phrasePal.Refresh();

            var dp = drumPal.GetAll(); var cp = chordPal.GetAll(); var pp = phrasePal.GetAll();
            GUILayout.Label($"Drum palettes: {dp.Count} | Chord palettes: {cp.Count} | " +
                            $"Phrase palettes: {pp.Count}");
            if (cp.Count == 0 || pp.Count == 0)
                GUILayout.Label("(empty stores usually mean the E-1b/E-2b package asset " +
                                "moves are still pending)");
            foreach (var p in dp) if (p != null) GUILayout.Label($"  [Drums] {p.GetDisplayName()}");
            foreach (var p in cp) if (p != null) GUILayout.Label($"  [Chords] {p.GetDisplayName()}");
            foreach (var p in pp) if (p != null) GUILayout.Label($"  [Phrases] {p.name}");
        }

        // ===============================================================
        // C1 helpers (unchanged)
        // ===============================================================

        private static void DrawInfiniteLoopToggle(CompositionSession session)
        {
            bool prev = CompositionSession.DevInfiniteCompositionLoop;
            CompositionSession.DevInfiniteCompositionLoop = GUILayout.Toggle(
                prev,
                " Infinite composition loop (countdown resets; per-loop hooks keep firing)");
            if (CompositionSession.DevInfiniteCompositionLoop != prev)
            {
                Debug.Log(
                    $"<color=lime>[DevMode][InfLoop]</color> Infinite composition loop → " +
                    $"{CompositionSession.DevInfiniteCompositionLoop}");
            }
            if (CompositionSession.DevInfiniteCompositionLoop)
            {
                var style = new GUIStyle(GUI.skin.label)
                { fontStyle = FontStyle.Italic, fontSize = 11 };
                GUILayout.Label(
                    "Part never advances / song never ends while ON. Draw + inspiration " +
                    "per loop keep firing (D2=A). Resets at song boundary.", style);
            }
        }

        private static void DrawLastRenderHeader(MidiMusicManager mm)
        {
            if (mm.LastRenderSerial <= 0)
            {
                GUILayout.Label("No render yet this session.");
                return;
            }
            GUILayout.Label(
                $"Last render: part={mm.LastRenderPartIndex} bpm={mm.LastRenderBpm} " +
                $"{(mm.LastRenderFromCache ? "(bundle-cache replay)" : "(fresh)")} " +
                $"serial={mm.LastRenderSerial}");
        }

        private static bool ResolveFullFlag(GigManager gm)
        {
            var dev = gm.DevSettings;
            if (dev == null)
            {
                GUILayout.Label("(GigDevSettings not wired — Compact format)");
                return false;
            }
            dev.CompositionDebugFull = GUILayout.Toggle(
                dev.CompositionDebugFull, " Full format");
            return dev.CompositionDebugFull;
        }

        private static void RefreshResolvedIfDirty(
            MidiMusicManager mm, CompositionSession session, int partIndex, bool full)
        {
            if (mm.LastRenderSerial == _lastSerialSeen
                && partIndex == _lastPartSeen
                && full == _lastFullSeen)
                return;

            _lastSerialSeen = mm.LastRenderSerial;
            _lastPartSeen = partIndex;
            _lastFullSeen = full;

            if (mm.LastRenderSerial <= 0)
            {
                _resolvedBlock = "(awaiting first render)";
                return;
            }
            if (mm.LastRenderPartIndex != partIndex)
            {
                _resolvedBlock =
                    $"(last render was part {mm.LastRenderPartIndex}; current part " +
                    $"{partIndex} not rendered yet)";
                return;
            }

            _resolvedBlock = GenerationDebugFormatter.FormatResolvedBlock(
                mm.LastResolvedByTrack, mm.LastPinnedByTrack, full);
        }

        private static GUIStyle Bold() =>
            new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
    }
}
#endif
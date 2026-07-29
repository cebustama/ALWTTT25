#if UNITY_EDITOR && ALWTTT_DEV
// [CSV-1 / D-CSV-7=A] Gate is literal per batch constraint ("Editor-only y
// #if ALWTTT_DEV"). Relaxing to plain UNITY_EDITOR (CardInventoryWindow
// precedent) is a one-line change and keeps zero production footprint.
using MidiGenPlay;
using MidiGenPlay.Composition;
using MidiGenPlay.Composition.Phrases;
using MidiGenPlay.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.DevMode.Editor
{
    /// <summary>
    /// [CSV-1] Read-only inventory browser over the composition asset families.
    ///
    /// **Discovery (CSV-1b + CSV-1c).** Every family is the UNION of the runtime
    /// read path and AssetDatabase, plus a reference harvest:
    ///   - patterns:     PatternRepositoryResources u AssetDatabase u harvest
    ///   - palettes:     TrackPatternConfigStoreResources u AssetDatabase
    ///   - instruments:  InstrumentRepositoryResources u AssetDatabase
    ///   - bundles / libraries / archetypes: AssetDatabase (no repository exists)
    /// The runtime repositories scan only their configured Resources roots, and
    /// several in-use asset families live outside them, so repository-only
    /// discovery silently omitted real content and produced false ORPHAN flags.
    /// Repository membership is still recorded and surfaced as the OFF-ROOT flag:
    /// an OFF-ROOT asset exists and may be played through a direct palette or
    /// bundle reference, but cannot appear in the dev pattern/instrument pickers
    /// (SSoT_Dev_Mode 18.4 / 18.9), which are repository-fed. HARVESTED means no
    /// scan found it at all - it is listed only because something references it.
    ///
    /// Derived "health" columns turn the listing into a curation worklist
    /// (CSV-3..CSV-6 input): orphans, content-duplicates, measures-vs-TS and
    /// measures-vs-part-length (the static face of the CR-7 "bass ends early"
    /// symptom - bass is single-pass, no repeat-to-fill, per
    /// SSoT_Composer_Bass_Track 1), beatsPerMeasure-vs-TS drift, instrument
    /// soundfont/bank/patch/octave/volume surface, and the discovery flags above.
    ///
    /// STRICTLY read-only: never mutates, renames, moves, or saves any asset.
    /// Print, Export JSON and Export All follow the CardInventoryWindow pattern
    /// ([Serializable] wrappers per view, file dialog, Debug.Log of path).
    /// The Names Report view is the CSV-4 naming-convention input.
    /// </summary>
    public sealed class CompositionInventoryWindow : EditorWindow
    {
        private enum View
        {
            StyleBundles,
            DrumPatterns,
            ChordProgressions,
            MelodyAndPhrases,
            MelodicInstruments,
            PercussionInstruments,
            NamesReport
        }

        [SerializeField] private View _view = View.StyleBundles;
        [SerializeField] private Vector2 _scroll;

        // ---- Filters ----
        [SerializeField] private int _tsFilterIndex;        // 0 = All
        [SerializeField] private string _textFilter = "";
        [SerializeField] private int _sourceFilterIndex;    // 0=All 1=Package 2=Local
        [SerializeField] private bool _onlyOrphans;
        [SerializeField] private bool _onlyDuplicates;
        [SerializeField] private bool _onlyFlagged;
        [SerializeField] private bool _onlyBundleReachable;
        [SerializeField] private int _referencePartMeasures = 8; // PartEntry default

        private static readonly string[] SourceOptions = { "All", "Package", "Local" };

        // ---- Catalog snapshot (rebuilt on Refresh) ----
        private bool _loaded;
        private MidiGenPlayConfig _cfg;
        private PatternRepositoryResources _repo;
        private InstrumentRepositoryResources _instRepo;

        private List<DrumPatternData> _drums = new();
        private List<ChordProgressionData> _chords = new();
        private List<MelodyPatternData> _melodies = new();
        private List<DrumPatternPaletteSO> _drumPalettes = new();
        private List<ChordProgressionPaletteSO> _chordPalettes = new();
        private List<PhrasePaletteSO> _phrasePalettes = new();
        private List<ChordProgressionLibrarySO> _libraries = new();
        private List<TrackStyleBundleSO> _bundles = new();
        private List<PhraseArchetypeSO> _archetypes = new();
        private List<MIDIInstrumentSO> _melInstruments = new();
        private List<MIDIPercussionInstrumentSO> _percInstruments = new();

        // Reference index: asset -> list of "<kind>:<owner name>" strings.
        private readonly Dictionary<UnityEngine.Object, List<string>> _refs = new();
        // Assets reachable from any style bundle (direct override ref, or via a
        // palette that some bundle references). Same reachability notion as the
        // §18.6 in-band convention, extended project-wide.
        private readonly HashSet<UnityEngine.Object> _bundleReachable = new();
        // Content-duplicate groups: asset -> group id (only members of groups >1).
        private readonly Dictionary<UnityEngine.Object, int> _dupGroup = new();
        // [CSV-1c] Assets the runtime repositories can actually resolve. Anything
        // outside this set exists in the project but is invisible to
        // PatternRepositoryResources / InstrumentRepositoryResources, and therefore
        // also invisible to the dev pattern/instrument pickers
        // (SSoT_Dev_Mode §18.4 / §18.9) which are fed by those repositories.
        // Surfaced as the OFF-ROOT flag — the measurement behind D-CSV-13.
        private readonly HashSet<UnityEngine.Object> _repoResolvable = new();
        // [CSV-1c] Assets that no scan found and that were recovered only by
        // walking palette / library / bundle references.
        private readonly HashSet<UnityEngine.Object> _harvested = new();

        [MenuItem("ALWTTT/Dev/Composition Inventory", priority = 30)]
        public static void Open()
        {
            var w = GetWindow<CompositionInventoryWindow>();
            w.titleContent = new GUIContent("Composition Inventory");
            w.minSize = new Vector2(860, 460);
            w.Show();
        }

        private void OnGUI()
        {
            if (!_loaded) RefreshCatalog();

            DrawToolbar();
            DrawFilterBar();
            EditorGUILayout.Space(4);

            using (var s = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = s.scrollPosition;
                switch (_view)
                {
                    case View.StyleBundles: DrawStyleBundles(); break;
                    case View.DrumPatterns: DrawDrumPatterns(); break;
                    case View.ChordProgressions: DrawChordProgressions(); break;
                    case View.MelodyAndPhrases: DrawMelodyAndPhrases(); break;
                    case View.MelodicInstruments: DrawMelodicInstruments(); break;
                    case View.PercussionInstruments: DrawPercussionInstruments(); break;
                    case View.NamesReport: DrawNamesReport(); break;
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // Catalog acquisition — existing read paths only
        // ══════════════════════════════════════════════════════════════════
        private void RefreshCatalog()
        {
            _cfg = MidiGenPlayConfig.FindInResources();
            _repo = new PatternRepositoryResources(_cfg);
            _repo.Refresh();

            // [CSV-1c / D-CSV-12=A+B] PATTERN discovery is the UNION of the
            // repository and AssetDatabase, for the same reason CSV-1b unioned
            // the palettes — one layer down.
            //
            // PatternRepositoryResources scans only {resourcesPatternsRoot}/<type>
            // (default "ScriptableObjects/Patterns/Chords|Drums|Melodies") plus the
            // package mirror. The project's in-use chord progressions live under
            // "ScriptableObjects/Chord Progressions/" — a SIBLING of Patterns/,
            // never scanned. The CSV-1b export made this visible: the 5 chord
            // palettes held 29 entries and not one matched any of the 13
            // progressions the repository returned, because the repository was
            // returning only the obsolete set (empty package defaults, Untitled,
            // generated Prog_Ionian_*). Drums did not show the defect only because
            // drum patterns happen to live under Patterns/Drums.
            //
            // The repository result is retained FIRST in each union so its
            // membership can be recorded (see _repoResolvable / the OFF-ROOT flag);
            // it also keeps any asset that is Resources-loadable but not
            // AssetDatabase-visible.
            _repoResolvable.Clear();
            RecordRepoResolvable(_repo.GetAllDrumPatterns());
            RecordRepoResolvable(_repo.GetAllChordProgressions());
            RecordRepoResolvable(_repo.GetAllMelodyPatterns());

            _drums = UnionAssets(_repo.GetAllDrumPatterns(), FindAllAssets<DrumPatternData>());
            _chords = UnionAssets(_repo.GetAllChordProgressions(), FindAllAssets<ChordProgressionData>());
            _melodies = UnionAssets(_repo.GetAllMelodyPatterns(), FindAllAssets<MelodyPatternData>());

            // [CSV-1b] Palette discovery is the UNION of the Resources store and
            // AssetDatabase discovery. The store alone scans only
            // Resources/ScriptableObjects/Patterns/<type>, and most project
            // palettes live outside it (Assets/Resources/Data/...). Scanning the
            // store alone made the reference index incomplete, which in turn made
            // the refs column and the ORPHAN flag wrong: the first real export
            // flagged 38/40 drum patterns and 13/13 progressions as orphans while
            // style bundles referenced 5 drum / 4 chord / 2 phrase palettes that
            // were never scanned. The store is retained in the union so a palette
            // that is Resources-loadable but somehow not AssetDatabase-visible
            // (e.g. inside a read-only package with an unusual import setup) is
            // still listed.
            var drumPalStore = new TrackPatternConfigStoreResources<DrumPatternPaletteSO>("Drums");
            var chordPalStore = new TrackPatternConfigStoreResources<ChordProgressionPaletteSO>("Chords");
            var phrasePalStore = new TrackPatternConfigStoreResources<PhrasePaletteSO>("Phrases");
            drumPalStore.Refresh(); chordPalStore.Refresh(); phrasePalStore.Refresh();
            _drumPalettes = UnionAssets(drumPalStore.GetAll(), FindAllAssets<DrumPatternPaletteSO>());
            _chordPalettes = UnionAssets(chordPalStore.GetAll(), FindAllAssets<ChordProgressionPaletteSO>());
            _phrasePalettes = UnionAssets(phrasePalStore.GetAll(), FindAllAssets<PhrasePaletteSO>());

            _instRepo = new InstrumentRepositoryResources(_cfg);
            _instRepo.Refresh();
            RecordRepoResolvable(_instRepo.GetMelodicInstruments());
            RecordRepoResolvable(_instRepo.GetPercussionInstruments());
            // Defensive split: MIDIPercussionInstrumentSO derives MIDIInstrumentSO.
            // [CSV-1c] Same union rationale as the patterns: an instrument outside
            // resourcesInstrumentsPath is invisible to the repository, therefore
            // also invisible to the §18.4/§18.9 dev pickers, and that is exactly
            // what the OFF-ROOT flag is for.
            _melInstruments = UnionAssets(
                    _instRepo.GetMelodicInstruments(), FindAllAssets<MIDIInstrumentSO>())
                .Where(i => !(i is MIDIPercussionInstrumentSO))
                .ToList();
            _percInstruments = UnionAssets(
                _instRepo.GetPercussionInstruments(), FindAllAssets<MIDIPercussionInstrumentSO>());

            // Families with no repo/store: discovery via AssetDatabase (finds
            // derived types; covers Assets/ and Packages/).
            _bundles = FindAllAssets<TrackStyleBundleSO>();
            _libraries = FindAllAssets<ChordProgressionLibrarySO>();
            _archetypes = FindAllAssets<PhraseArchetypeSO>();
            if (_cfg != null && _cfg.progressionLibrary != null
                && !_libraries.Contains(_cfg.progressionLibrary))
                _libraries.Add(_cfg.progressionLibrary);

            HarvestReferencedPatterns();
            BuildReferenceIndex();
            BuildDuplicateIndex();
            _loaded = true;
        }

        private void RecordRepoResolvable<T>(IEnumerable<T> src) where T : UnityEngine.Object
        {
            if (src == null) return;
            foreach (var x in src) if (x != null) _repoResolvable.Add(x);
        }

        // [CSV-1c / D-CSV-12=B] Second safety net. The union above catches assets
        // that AssetDatabase can see; this catches anything a palette, library or
        // style bundle actually REFERENCES that is still missing from the lists —
        // regardless of where it lives, including assets nested inside other
        // assets. Anything added here is by definition in use, so it must appear
        // in the inventory even if no scan root covers it.
        private void HarvestReferencedPatterns()
        {
            _harvested.Clear();

            void AddDrum(DrumPatternData p)
            {
                if (p == null || _drums.Contains(p)) return;
                _drums.Add(p); _harvested.Add(p);
            }
            void AddChord(ChordProgressionData p)
            {
                if (p == null || _chords.Contains(p)) return;
                _chords.Add(p); _harvested.Add(p);
            }
            void AddMelody(MelodyPatternData p)
            {
                if (p == null || _melodies.Contains(p)) return;
                _melodies.Add(p); _harvested.Add(p);
            }
            void AddArchetype(PhraseArchetypeSO a)
            {
                if (a == null || _archetypes.Contains(a)) return;
                _archetypes.Add(a); _harvested.Add(a);
            }

            foreach (var pal in _drumPalettes)
                if (pal.entries != null)
                    foreach (var e in pal.entries) AddDrum(e?.pattern);

            foreach (var pal in _chordPalettes)
                if (pal.entries != null)
                    foreach (var e in pal.entries) AddChord(e?.progression);

            foreach (var lib in _libraries)
                if (lib.entries != null)
                    foreach (var e in lib.entries) AddChord(e?.progression);

            foreach (var pal in _phrasePalettes)
                if (pal.archetypes != null)
                    foreach (var a in pal.archetypes) AddArchetype(a?.archetype);

            foreach (var b in _bundles)
            {
                switch (b)
                {
                    case RhythmCardConfigSO r: AddDrum(r.patternOverride); break;
                    case BackingCardConfigSO bk: AddChord(bk.progressionOverride); break;
                    case MelodyCardConfigSO m: AddMelody(m.patternOverride); break;
                }
            }
        }

        private void BuildReferenceIndex()
        {
            _refs.Clear();
            _bundleReachable.Clear();

            void AddRef(UnityEngine.Object target, string owner)
            {
                if (target == null) return;
                if (!_refs.TryGetValue(target, out var list))
                    _refs[target] = list = new List<string>();
                list.Add(owner);
            }

            foreach (var pal in _drumPalettes)
                if (pal.entries != null)
                    foreach (var e in pal.entries)
                        AddRef(e?.pattern, $"drumPal:{pal.name}");

            foreach (var pal in _chordPalettes)
                if (pal.entries != null)
                    foreach (var e in pal.entries)
                        AddRef(e?.progression, $"chordPal:{pal.name}");

            foreach (var lib in _libraries)
                if (lib.entries != null)
                    foreach (var e in lib.entries)
                        AddRef(e?.progression, $"lib:{lib.name}");

            foreach (var pal in _phrasePalettes)
                if (pal.archetypes != null)
                    foreach (var a in pal.archetypes)
                        AddRef(a?.archetype, $"phrasePal:{pal.name}");

            // Bundles: direct refs + reachability closure through palettes.
            foreach (var b in _bundles)
            {
                switch (b)
                {
                    case RhythmCardConfigSO r:
                        AddRef(r.patternOverride, $"bundle:{r.name}");
                        AddRef(r.patternPalette, $"bundle:{r.name}");
                        if (r.patternOverride != null) _bundleReachable.Add(r.patternOverride);
                        if (r.patternPalette != null)
                        {
                            _bundleReachable.Add(r.patternPalette);
                            if (r.patternPalette.entries != null)
                                foreach (var e in r.patternPalette.entries)
                                    if (e?.pattern != null) _bundleReachable.Add(e.pattern);
                        }
                        break;
                    case BackingCardConfigSO bk:
                        AddRef(bk.progressionOverride, $"bundle:{bk.name}");
                        AddRef(bk.progressionPalette, $"bundle:{bk.name}");
                        if (bk.progressionOverride != null) _bundleReachable.Add(bk.progressionOverride);
                        if (bk.progressionPalette != null)
                        {
                            _bundleReachable.Add(bk.progressionPalette);
                            if (bk.progressionPalette.entries != null)
                                foreach (var e in bk.progressionPalette.entries)
                                    if (e?.progression != null) _bundleReachable.Add(e.progression);
                        }
                        break;
                    case MelodyCardConfigSO m:
                        AddRef(m.patternOverride, $"bundle:{m.name}");
                        AddRef(m.phrasePaletteOverride, $"bundle:{m.name}");
                        if (m.patternOverride != null) _bundleReachable.Add(m.patternOverride);
                        if (m.phrasePaletteOverride != null)
                        {
                            _bundleReachable.Add(m.phrasePaletteOverride);
                            if (m.phrasePaletteOverride.archetypes != null)
                                foreach (var a in m.phrasePaletteOverride.archetypes)
                                    if (a?.archetype != null) _bundleReachable.Add(a.archetype);
                        }
                        break;
                        // HarmonyCardConfigSO holds no pattern/palette asset refs.
                }
            }
        }

        // Content-duplicate detection. Per-family content signatures deliberately
        // EXCLUDE naming/metadata (asset name, DisplayName, originalInput,
        // songReferences) so renames don't hide duplicates and duplicates don't
        // hide behind different names.
        private void BuildDuplicateIndex()
        {
            _dupGroup.Clear();
            int group = 0;

            void Group<T>(IEnumerable<T> items, Func<T, string> sig) where T : UnityEngine.Object
            {
                foreach (var g in items.Where(i => i != null)
                                       .GroupBy(sig)
                                       .Where(g => g.Count() > 1))
                {
                    group++;
                    foreach (var m in g) _dupGroup[m] = group;
                }
            }

            Group(_chords, ProgressionSignature);
            Group(_drums, DrumSignature);
            Group(_melodies, MelodySignature);
            Group(_melInstruments, InstrumentSignature);
            Group(_percInstruments, i => InstrumentSignature(i) + "|maps=" +
                string.Join(",", (i.percussionMappings ?? new List<MIDIPercussionInstrumentSO.PercussionMapping>())
                    .Select(m => $"{m.percussionType}:{m.noteName}{m.octave}")));
        }

        private static string ProgressionSignature(ChordProgressionData p)
        {
            var sb = new StringBuilder();
            sb.Append(p.TimeSignature).Append('|').Append(p.Measures)
              .Append('|').Append(p.subdivisions).Append('|');
            if (p.events != null)
                foreach (var e in p.events.OrderBy(e => e.startStep).ThenBy(e => e.degree))
                    sb.Append(e.startStep).Append(',').Append(e.lengthSteps).Append(',')
                      .Append(e.degree).Append(',').Append(e.quality).Append(',')
                      .Append(e.degreeAccidental).Append(';');
            return sb.ToString();
        }

        private static string DrumSignature(DrumPatternData d)
        {
            var sb = new StringBuilder();
            sb.Append(d.TimeSignature).Append('|').Append(d.Measures).Append('|')
              .Append(d.beatsPerMeasure).Append('|').Append(d.subdivisions).Append('|');
            if (d.lanes != null)
                foreach (var l in d.lanes)
                {
                    sb.Append(l.instrument).Append(':').Append(l.defaultVelocity).Append(':');
                    if (l.steps != null)
                        foreach (var s in l.steps)
                            sb.Append(s.active ? (char)('0' + Mathf.Clamp(s.velocity / 16, 0, 9)) : '.');
                    sb.Append(';');
                }
            return sb.ToString();
        }

        private static string MelodySignature(MelodyPatternData m)
        {
            var sb = new StringBuilder();
            sb.Append(m.TimeSignature).Append('|').Append(m.Measures).Append('|')
              .Append(m.beatsPerMeasure).Append('|').Append(m.subdivisions).Append('|');
            if (m.notes != null)
                foreach (var n in m.notes.OrderBy(n => n.startBeat).ThenBy(n => n.degree))
                    sb.Append(n.startBeat.ToString("0.###")).Append(',')
                      .Append(n.durationBeats.ToString("0.###")).Append(',')
                      .Append(n.degree).Append(',').Append(n.octaveOffset).Append(',')
                      .Append(n.velocity).Append(';');
            return sb.ToString();
        }

        private static string InstrumentSignature(MIDIInstrumentSO i)
            => $"{i.SelectedSoundFont}|{i.BankName}|{i.PatchName}|{i.PatchIndex}";

        // ══════════════════════════════════════════════════════════════════
        // Health flags
        // ══════════════════════════════════════════════════════════════════
        private List<string> ProgressionFlags(ChordProgressionData p)
        {
            var flags = new List<string>();
            int beats = TimeSignatureProperties[p.TimeSignature].BeatsPerMeasure;
            int total = p.TotalSteps(beats);
            int span = 0;
            if (p.events != null)
                foreach (var e in p.events)
                    span = Mathf.Max(span, e.startStep + Mathf.Max(1, e.lengthSteps));

            if (p.events == null || p.events.Count == 0) flags.Add("EMPTY");
            else
            {
                if (span < total) flags.Add($"SHORT-TAIL span={span}/{total} steps");
                if (span > total) flags.Add($"OVERFLOW span={span}/{total} steps");
            }
            // CR-7 static face: bass renders the progression once (no
            // repeat-to-fill, SSoT_Composer_Bass_Track §1). Any progression
            // shorter than the reference part length leaves the bass silent
            // for the remainder of the part.
            if (p.Measures < _referencePartMeasures)
                flags.Add($"BASS-GAP {p.Measures}m < part {_referencePartMeasures}m");
            else if (p.Measures > _referencePartMeasures)
                flags.Add($"LONGER-THAN-PART {p.Measures}m > {_referencePartMeasures}m");
            if (IsOrphan(p)) flags.Add("ORPHAN");
            if (_dupGroup.TryGetValue(p, out var g)) flags.Add($"DUP#{g}");
            AppendDiscoveryFlags(p, flags);
            return flags;
        }

        private List<string> DrumFlags(DrumPatternData d)
        {
            var flags = new List<string>();
            int tsBeats = TimeSignatureProperties[d.TimeSignature].BeatsPerMeasure;
            if (d.beatsPerMeasure != tsBeats)
                flags.Add($"BPMEAS-MISMATCH field={d.beatsPerMeasure} TS={tsBeats}");
            if (d.lanes == null || d.lanes.Count == 0) flags.Add("NO-LANES");
            else if (d.lanes.All(l => l.steps == null || l.steps.All(s => !s.active)))
                flags.Add("ALL-SILENT");
            if (IsOrphan(d)) flags.Add("ORPHAN");
            if (_dupGroup.TryGetValue(d, out var g)) flags.Add($"DUP#{g}");
            AppendDiscoveryFlags(d, flags);
            return flags;
        }

        private List<string> MelodyFlags(MelodyPatternData m)
        {
            var flags = new List<string>();
            int tsBeats = TimeSignatureProperties[m.TimeSignature].BeatsPerMeasure;
            if (m.beatsPerMeasure != tsBeats)
                flags.Add($"BPMEAS-MISMATCH field={m.beatsPerMeasure} TS={tsBeats}");
            if (m.notes == null || m.notes.Count == 0) flags.Add("EMPTY");
            else
            {
                float lastEnd = m.notes.Max(n => n.startBeat + n.durationBeats);
                if (lastEnd > m.TotalBeats + 0.001f)
                    flags.Add($"OVERFLOW {lastEnd:0.##}b > {m.TotalBeats:0.##}b");
            }
            if (IsOrphan(m)) flags.Add("ORPHAN");
            if (_dupGroup.TryGetValue(m, out var g)) flags.Add($"DUP#{g}");
            AppendDiscoveryFlags(m, flags);
            return flags;
        }

        private List<string> InstrumentFlags(MIDIInstrumentSO i)
        {
            var flags = new List<string>();
            if (string.IsNullOrEmpty(i.SelectedSoundFont)) flags.Add("NO-SOUNDFONT");
            if (i.octaveMin > i.octaveMax) flags.Add("OCTAVE-RANGE-INVERTED");
            if (i.volume01 <= 0f) flags.Add("VOLUME-ZERO");
            if (_dupGroup.TryGetValue(i, out var g)) flags.Add($"DUP#{g}");
            AppendDiscoveryFlags(i, flags);
            return flags;
        }

        // [CSV-1c] OFF-ROOT: exists in the project, but no runtime repository can
        // resolve it (outside every configured Resources scan root). Such an asset
        // cannot appear in the dev §18.4 pattern picker or the §18.9 instrument
        // picker, even though the game may still play it via a direct palette or
        // bundle reference. HARVESTED additionally means no scan found it at all —
        // it is present here only because something references it.
        private void AppendDiscoveryFlags(UnityEngine.Object o, List<string> flags)
        {
            if (!_repoResolvable.Contains(o)) flags.Add("OFF-ROOT");
            if (_harvested.Contains(o)) flags.Add("HARVESTED");
        }

        private bool IsOrphan(UnityEngine.Object o)
            => !_refs.TryGetValue(o, out var list) || list.Count == 0;

        // ══════════════════════════════════════════════════════════════════
        // Toolbar + filters
        // ══════════════════════════════════════════════════════════════════
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                void Tab(View v, string label, float w)
                {
                    if (GUILayout.Toggle(_view == v, label, EditorStyles.toolbarButton,
                            GUILayout.Width(w)))
                        _view = v;
                }
                Tab(View.StyleBundles, "Style Bundles", 100);
                Tab(View.DrumPatterns, "Drum Patterns", 100);
                Tab(View.ChordProgressions, "Chord Progressions", 130);
                Tab(View.MelodyAndPhrases, "Melody / Phrases", 115);
                Tab(View.MelodicInstruments, "Melodic Instr.", 100);
                Tab(View.PercussionInstruments, "Percussion Instr.", 115);
                Tab(View.NamesReport, "Names Report", 100);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
                { _loaded = false; }
                if (GUILayout.Button("Print", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    PrintCurrentView();
                if (GUILayout.Button("Export JSON", EditorStyles.toolbarButton, GUILayout.Width(90)))
                    ExportCurrentView();
                if (GUILayout.Button("Export All", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    ExportAllViews();
            }
        }

        private void DrawFilterBar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var tsValues = (TimeSignature[])Enum.GetValues(typeof(TimeSignature));
                var tsOptions = new string[tsValues.Length + 1];
                tsOptions[0] = "TS: All";
                for (int i = 0; i < tsValues.Length; i++) tsOptions[i + 1] = "TS: " + tsValues[i];
                _tsFilterIndex = EditorGUILayout.Popup(_tsFilterIndex, tsOptions, GUILayout.Width(110));

                _sourceFilterIndex = EditorGUILayout.Popup(
                    _sourceFilterIndex, SourceOptions, GUILayout.Width(80));

                GUILayout.Label("Filter:", GUILayout.Width(40));
                _textFilter = EditorGUILayout.TextField(_textFilter, GUILayout.MinWidth(120));

                _onlyOrphans = GUILayout.Toggle(_onlyOrphans, "Orphans", GUILayout.Width(70));
                _onlyDuplicates = GUILayout.Toggle(_onlyDuplicates, "Dups", GUILayout.Width(55));
                _onlyFlagged = GUILayout.Toggle(_onlyFlagged, "Flagged", GUILayout.Width(70));
                _onlyBundleReachable = GUILayout.Toggle(
                    _onlyBundleReachable, "Bundle-reachable", GUILayout.Width(120));

                GUILayout.Label("Ref part measures:", GUILayout.Width(110));
                _referencePartMeasures = Mathf.Max(1,
                    EditorGUILayout.IntField(_referencePartMeasures, GUILayout.Width(34)));
            }
        }

        private TimeSignature? TsFilter()
        {
            if (_tsFilterIndex <= 0) return null;
            var vals = (TimeSignature[])Enum.GetValues(typeof(TimeSignature));
            return vals[_tsFilterIndex - 1];
        }

        private bool PassesCommonFilters(
            UnityEngine.Object asset, string displayName,
            TimeSignature? ts, IReadOnlyList<string> flags)
        {
            if (ts.HasValue && TsFilter().HasValue && ts.Value != TsFilter().Value) return false;
            if (!string.IsNullOrEmpty(_textFilter))
            {
                string hay = (asset.name + " " + (displayName ?? "")).ToLowerInvariant();
                if (!hay.Contains(_textFilter.ToLowerInvariant())) return false;
            }
            if (_sourceFilterIndex != 0)
            {
                bool isPkg = IsPackageAsset(asset);
                if (_sourceFilterIndex == 1 && !isPkg) return false;
                if (_sourceFilterIndex == 2 && isPkg) return false;
            }
            if (_onlyOrphans && !IsOrphan(asset)) return false;
            if (_onlyDuplicates && !_dupGroup.ContainsKey(asset)) return false;
            if (_onlyFlagged && (flags == null || flags.Count == 0)) return false;
            if (_onlyBundleReachable && !_bundleReachable.Contains(asset)) return false;
            return true;
        }

        private static bool IsPackageAsset(UnityEngine.Object o)
            => AssetDatabase.GetAssetPath(o).StartsWith("Packages/", StringComparison.Ordinal);

        private static string SourceTag(UnityEngine.Object o)
            => IsPackageAsset(o) ? "pkg" : "local";

        private string RefsLabel(UnityEngine.Object o)
            => _refs.TryGetValue(o, out var list) && list.Count > 0
                ? string.Join(", ", list.Distinct())
                : "—";

        // ══════════════════════════════════════════════════════════════════
        // Views
        // ══════════════════════════════════════════════════════════════════
        private void DrawStyleBundles()
        {
            EditorGUILayout.LabelField(
                $"TrackStyleBundleSO assets: {_bundles.Count}", EditorStyles.boldLabel);
            foreach (var b in _bundles)
            {
                if (b == null) continue;
                if (!PassesCommonFilters(b, b.name, null, null)) continue;
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label($"[{SourceTag(b)}]", GUILayout.Width(40));
                    GUILayout.Label(b.GetType().Name, GUILayout.Width(160));
                    GUILayout.Label(b.name, GUILayout.Width(220));
                    GUILayout.Label($"role={b.appliesTo}", GUILayout.Width(110));
                    GUILayout.Label(BundleRefsSummary(b));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ping", GUILayout.Width(44)))
                        EditorGUIUtility.PingObject(b);
                }
            }
        }

        private static string BundleRefsSummary(TrackStyleBundleSO b) => b switch
        {
            RhythmCardConfigSO r =>
                $"override={(r.patternOverride ? r.patternOverride.name : "—")} " +
                $"palette={(r.patternPalette ? r.patternPalette.name : "—")}",
            BackingCardConfigSO bk =>
                $"override={(bk.progressionOverride ? bk.progressionOverride.name : "—")} " +
                $"palette={(bk.progressionPalette ? bk.progressionPalette.name : "—")}",
            MelodyCardConfigSO m =>
                $"pattern={(m.patternOverride ? m.patternOverride.name : "—")} " +
                $"phrasePal={(m.phrasePaletteOverride ? m.phrasePaletteOverride.name : "—")}",
            _ => "(no pattern refs)"
        };

        private void DrawDrumPatterns()
        {
            EditorGUILayout.LabelField(
                $"DrumPatternData: {_drums.Count}  |  Drum palettes: {_drumPalettes.Count}",
                EditorStyles.boldLabel);
            foreach (var d in _drums)
            {
                var flags = DrumFlags(d);
                if (!PassesCommonFilters(d, d.DisplayName, d.TimeSignature, flags)) continue;
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label($"[{SourceTag(d)}]", GUILayout.Width(40));
                    GUILayout.Label(d.name, GUILayout.Width(200));
                    GUILayout.Label($"{d.TimeSignature} {d.Measures}m sub={d.subdivisions} " +
                                    $"lanes={d.lanes?.Count ?? 0}", GUILayout.Width(200));
                    GUILayout.Label($"refs: {RefsLabel(d)}", GUILayout.MinWidth(140));
                    DrawFlags(flags);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ping", GUILayout.Width(44)))
                        EditorGUIUtility.PingObject(d);
                }
            }
            EditorGUILayout.Space(4);
            foreach (var pal in _drumPalettes)
                DrawPaletteRow(pal, pal.GetDisplayName(),
                    $"entries={pal.entries?.Count ?? 0}");
        }

        private void DrawChordProgressions()
        {
            EditorGUILayout.LabelField(
                $"ChordProgressionData: {_chords.Count}  |  Palettes: {_chordPalettes.Count}" +
                $"  |  Libraries: {_libraries.Count}", EditorStyles.boldLabel);
            foreach (var p in _chords)
            {
                var flags = ProgressionFlags(p);
                if (!PassesCommonFilters(p, p.DisplayName, p.TimeSignature, flags)) continue;
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label($"[{SourceTag(p)}]", GUILayout.Width(40));
                    GUILayout.Label(p.name, GUILayout.Width(200));
                    GUILayout.Label($"{p.TimeSignature} {p.Measures}m sub={p.subdivisions} " +
                                    $"ev={p.events?.Count ?? 0}", GUILayout.Width(180));
                    GUILayout.Label($"refs: {RefsLabel(p)}", GUILayout.MinWidth(140));
                    DrawFlags(flags);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ping", GUILayout.Width(44)))
                        EditorGUIUtility.PingObject(p);
                }
            }
            EditorGUILayout.Space(4);
            foreach (var pal in _chordPalettes)
                DrawPaletteRow(pal, pal.GetDisplayName(), $"entries={pal.entries?.Count ?? 0}");
            foreach (var lib in _libraries)
                DrawPaletteRow(lib, lib.name, $"entries={lib.entries?.Count ?? 0}" +
                    (_cfg != null && _cfg.progressionLibrary == lib ? "  [config-wired]" : ""));
        }

        private void DrawMelodyAndPhrases()
        {
            EditorGUILayout.LabelField(
                $"MelodyPatternData: {_melodies.Count}  |  Archetypes: {_archetypes.Count}" +
                $"  |  Phrase palettes: {_phrasePalettes.Count}", EditorStyles.boldLabel);
            foreach (var m in _melodies)
            {
                var flags = MelodyFlags(m);
                if (!PassesCommonFilters(m, m.DisplayName, m.TimeSignature, flags)) continue;
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label($"[{SourceTag(m)}]", GUILayout.Width(40));
                    GUILayout.Label(m.name, GUILayout.Width(200));
                    GUILayout.Label($"{m.TimeSignature} {m.Measures}m sub={m.subdivisions} " +
                                    $"notes={m.notes?.Count ?? 0}", GUILayout.Width(190));
                    GUILayout.Label($"refs: {RefsLabel(m)}", GUILayout.MinWidth(140));
                    DrawFlags(flags);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ping", GUILayout.Width(44)))
                        EditorGUIUtility.PingObject(m);
                }
            }
            EditorGUILayout.Space(4);
            foreach (var a in _archetypes)
            {
                if (a == null || !PassesCommonFilters(a, a.name, null, null)) continue;
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label($"[{SourceTag(a)}]", GUILayout.Width(40));
                    GUILayout.Label($"[Archetype] {a.name} ({a.GetType().Name})",
                        GUILayout.Width(340));
                    GUILayout.Label($"refs: {RefsLabel(a)}" + (IsOrphan(a) ? "  ORPHAN" : ""));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ping", GUILayout.Width(44)))
                        EditorGUIUtility.PingObject(a);
                }
            }
            foreach (var pal in _phrasePalettes)
                DrawPaletteRow(pal, pal.name, $"archetypes={pal.archetypes?.Count ?? 0}");
        }

        private void DrawMelodicInstruments() => DrawInstrumentList(_melInstruments, null);

        private void DrawPercussionInstruments()
            => DrawInstrumentList(_percInstruments.Cast<MIDIInstrumentSO>().ToList(),
                i => $"maps={((MIDIPercussionInstrumentSO)i).percussionMappings?.Count ?? 0}");

        private void DrawInstrumentList(
            List<MIDIInstrumentSO> list, Func<MIDIInstrumentSO, string> extra)
        {
            EditorGUILayout.LabelField($"Instruments: {list.Count}", EditorStyles.boldLabel);
            foreach (var i in list)
            {
                var flags = InstrumentFlags(i);
                if (!PassesCommonFilters(i, i.InstrumentName, null, flags)) continue;
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label($"[{SourceTag(i)}]", GUILayout.Width(40));
                    GUILayout.Label(i.name, GUILayout.Width(180));
                    GUILayout.Label($"{i.InstrumentType}", GUILayout.Width(110));
                    GUILayout.Label(
                        $"sf={i.SelectedSoundFont ?? "—"} bank={i.BankName ?? "—"} " +
                        $"patch={i.PatchName ?? "—"}({i.PatchIndex}) " +
                        $"oct={i.octaveMin}-{i.octaveMax} vol={i.volume01:0.##}" +
                        (extra != null ? " " + extra(i) : ""),
                        GUILayout.MinWidth(300));
                    DrawFlags(flags);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ping", GUILayout.Width(44)))
                        EditorGUIUtility.PingObject(i);
                }
            }
        }

        // CSV-4 input: current names per family. Read-only report — this batch
        // renames nothing.
        private void DrawNamesReport()
        {
            EditorGUILayout.HelpBox(
                "Naming-convention input for CSV-4. Export JSON for the full report. " +
                "No renames are performed by this window.", MessageType.Info);
            void Family<T>(string title, IEnumerable<T> items,
                Func<T, string> display) where T : UnityEngine.Object
            {
                var arr = items.Where(x => x != null).ToList();
                EditorGUILayout.LabelField($"{title} ({arr.Count})", EditorStyles.boldLabel);
                foreach (var x in arr)
                    GUILayout.Label($"  [{SourceTag(x)}] {x.name}" +
                        (display(x) is string d && !string.IsNullOrEmpty(d) && d != x.name
                            ? $"  (display: {d})" : ""));
            }
            Family("Style bundles", _bundles, b => "");
            Family("Drum patterns", _drums, d => d.DisplayName);
            Family("Drum palettes", _drumPalettes, p => p.GetDisplayName());
            Family("Chord progressions", _chords, c => c.DisplayName);
            Family("Chord palettes", _chordPalettes, p => p.GetDisplayName());
            Family("Chord libraries", _libraries, l => "");
            Family("Melody patterns", _melodies, m => m.DisplayName);
            Family("Phrase archetypes", _archetypes, a => "");
            Family("Phrase palettes", _phrasePalettes, p => "");
            Family("Melodic instruments", _melInstruments, i => i.InstrumentName);
            Family("Percussion instruments", _percInstruments, i => i.InstrumentName);
        }

        private void DrawPaletteRow(UnityEngine.Object pal, string display, string detail)
        {
            if (pal == null || !PassesCommonFilters(pal, display, null, null)) return;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label($"[{SourceTag(pal)}]", GUILayout.Width(40));
                GUILayout.Label($"[{pal.GetType().Name}] {display}", GUILayout.Width(340));
                GUILayout.Label(detail, GUILayout.Width(120));
                GUILayout.Label($"refs: {RefsLabel(pal)}" + (IsOrphan(pal) ? "  ORPHAN" : ""));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Ping", GUILayout.Width(44)))
                    EditorGUIUtility.PingObject(pal);
            }
        }

        private static void DrawFlags(List<string> flags)
        {
            if (flags == null || flags.Count == 0) return;
            var style = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            style.normal.textColor = new Color(1f, 0.72f, 0.35f);
            GUILayout.Label("⚠ " + string.Join(" | ", flags), style, GUILayout.MinWidth(160));
        }

        // ══════════════════════════════════════════════════════════════════
        // Print (Console) — CardInventoryWindow pattern
        // ══════════════════════════════════════════════════════════════════
        private void PrintCurrentView()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== COMPOSITION INVENTORY — {_view} ===");
            switch (_view)
            {
                case View.StyleBundles:
                    foreach (var b in _bundles)
                        sb.AppendLine($"  [{SourceTag(b)}] {b.GetType().Name} | {b.name} | " +
                            $"role={b.appliesTo} | {BundleRefsSummary(b)} | " +
                            $"{AssetDatabase.GetAssetPath(b)}");
                    break;
                case View.DrumPatterns:
                    foreach (var d in _drums)
                        sb.AppendLine($"  [{SourceTag(d)}] {d.name} | {d.TimeSignature} " +
                            $"{d.Measures}m sub={d.subdivisions} lanes={d.lanes?.Count ?? 0} | " +
                            $"refs={RefsLabel(d)} | flags={string.Join(";", DrumFlags(d))}");
                    break;
                case View.ChordProgressions:
                    foreach (var p in _chords)
                        sb.AppendLine($"  [{SourceTag(p)}] {p.name} | {p.TimeSignature} " +
                            $"{p.Measures}m sub={p.subdivisions} ev={p.events?.Count ?? 0} | " +
                            $"refs={RefsLabel(p)} | flags={string.Join(";", ProgressionFlags(p))}");
                    break;
                case View.MelodyAndPhrases:
                    foreach (var m in _melodies)
                        sb.AppendLine($"  [{SourceTag(m)}] {m.name} | {m.TimeSignature} " +
                            $"{m.Measures}m notes={m.notes?.Count ?? 0} | " +
                            $"refs={RefsLabel(m)} | flags={string.Join(";", MelodyFlags(m))}");
                    foreach (var a in _archetypes)
                        sb.AppendLine($"  [{SourceTag(a)}] [Archetype] {a.name} | refs={RefsLabel(a)}");
                    foreach (var p in _phrasePalettes)
                        sb.AppendLine($"  [{SourceTag(p)}] [PhrasePalette] {p.name} | " +
                            $"archetypes={p.archetypes?.Count ?? 0}");
                    break;
                case View.MelodicInstruments:
                    foreach (var i in _melInstruments) sb.AppendLine(PrintInstrumentLine(i));
                    break;
                case View.PercussionInstruments:
                    foreach (var i in _percInstruments) sb.AppendLine(PrintInstrumentLine(i));
                    break;
                case View.NamesReport:
                    AppendNames(sb);
                    break;
            }
            Debug.Log(sb.ToString());
        }

        private string PrintInstrumentLine(MIDIInstrumentSO i)
            => $"  [{SourceTag(i)}] {i.name} | {i.InstrumentType} | sf={i.SelectedSoundFont} " +
               $"bank={i.BankName} patch={i.PatchName}({i.PatchIndex}) " +
               $"oct={i.octaveMin}-{i.octaveMax} vol={i.volume01:0.##} | " +
               $"flags={string.Join(";", InstrumentFlags(i))} | {AssetDatabase.GetAssetPath(i)}";

        private void AppendNames(StringBuilder sb)
        {
            void F<T>(string t, IEnumerable<T> xs, Func<T, string> d) where T : UnityEngine.Object
            {
                sb.AppendLine($"  -- {t} --");
                foreach (var x in xs.Where(x => x != null))
                    sb.AppendLine($"    [{SourceTag(x)}] {x.name}" +
                        (d(x) is string dd && !string.IsNullOrEmpty(dd) && dd != x.name
                            ? $" (display: {dd})" : "") +
                        $" | {AssetDatabase.GetAssetPath(x)}");
            }
            F("StyleBundles", _bundles, b => "");
            F("DrumPatterns", _drums, x => x.DisplayName);
            F("DrumPalettes", _drumPalettes, x => x.GetDisplayName());
            F("ChordProgressions", _chords, x => x.DisplayName);
            F("ChordPalettes", _chordPalettes, x => x.GetDisplayName());
            F("ChordLibraries", _libraries, x => "");
            F("MelodyPatterns", _melodies, x => x.DisplayName);
            F("PhraseArchetypes", _archetypes, x => "");
            F("PhrasePalettes", _phrasePalettes, x => "");
            F("MelodicInstruments", _melInstruments, x => x.InstrumentName);
            F("PercussionInstruments", _percInstruments, x => x.InstrumentName);
        }

        // ══════════════════════════════════════════════════════════════════
        // Export JSON — CardInventoryWindow pattern ([Serializable] wrappers,
        // SaveFilePanel, Debug.Log of path, RevealInFinder)
        // ══════════════════════════════════════════════════════════════════
        [Serializable] private class JsonBundle { public string type; public string assetName; public string appliesTo; public string overrideRef; public string paletteRef; public string source; public string assetPath; }
        [Serializable] private class JsonPattern { public string assetName; public string displayName; public string timeSignature; public int measures; public int subdivisions; public int contentCount; public string source; public string refs; public string flags; public string assetPath; }
        [Serializable] private class JsonPalette { public string type; public string assetName; public string displayName; public int entryCount; public string refs; public bool orphan; public string source; public string assetPath; }
        [Serializable] private class JsonInstrument { public string assetName; public string instrumentName; public string instrumentType; public string soundFont; public string bank; public string patch; public int patchIndex; public int octaveMin; public int octaveMax; public float volume01; public int percussionMappings; public string flags; public string source; public string assetPath; }
        [Serializable] private class JsonName { public string family; public string assetName; public string displayName; public string source; public string assetPath; }

        [Serializable] private class WrapBundles { public List<JsonBundle> bundles = new(); }
        [Serializable] private class WrapPatterns { public List<JsonPattern> patterns = new(); public List<JsonPalette> palettes = new(); }
        [Serializable] private class WrapInstruments { public List<JsonInstrument> instruments = new(); }
        [Serializable] private class WrapNames { public List<JsonName> names = new(); }

        private void ExportCurrentView()
        {
            string defaultName = $"CompositionInventory_{_view}.json";
            string path = EditorUtility.SaveFilePanel(
                "Export Composition Inventory JSON", "", defaultName, "json");
            if (string.IsNullOrEmpty(path)) return;

            File.WriteAllText(path, BuildJsonForView(_view));
            Debug.Log($"[CompositionInventory] Exported to {path}");
            EditorUtility.RevealInFinder(path);
        }

        // [CSV-1c] Export All — one folder pick, all seven views written in a
        // single pass with the same filenames and schemas the per-view Export
        // produces. Requested because tab-by-tab export made a full re-baseline
        // (the CSV-1b/CSV-1c workflow) needlessly slow. Per-view Export is
        // unchanged; both call BuildJsonForView.
        private void ExportAllViews()
        {
            string folder = EditorUtility.SaveFolderPanel(
                "Export ALL Composition Inventory JSON", "", "");
            if (string.IsNullOrEmpty(folder)) return;

            int written = 0;
            var failures = new List<string>();
            foreach (View v in Enum.GetValues(typeof(View)))
            {
                string file = Path.Combine(folder, $"CompositionInventory_{v}.json");
                try
                {
                    File.WriteAllText(file, BuildJsonForView(v));
                    written++;
                }
                catch (Exception e)
                {
                    // One bad view must not abort the batch — record and continue.
                    failures.Add($"{v}: {e.Message}");
                }
            }

            Debug.Log($"[CompositionInventory] Exported {written} view(s) to {folder}" +
                      (failures.Count > 0
                          ? $"\n  FAILED: {string.Join(" | ", failures)}"
                          : string.Empty));
            EditorUtility.RevealInFinder(folder);
        }

        private string BuildJsonForView(View view)
        {
            string json;
            switch (view)
            {
                case View.StyleBundles:
                    {
                        var w = new WrapBundles();
                        foreach (var b in _bundles)
                            w.bundles.Add(new JsonBundle
                            {
                                type = b.GetType().Name,
                                assetName = b.name,
                                appliesTo = b.appliesTo.ToString(),
                                overrideRef = BundleOverrideName(b),
                                paletteRef = BundlePaletteName(b),
                                source = SourceTag(b),
                                assetPath = AssetDatabase.GetAssetPath(b)
                            });
                        json = JsonUtility.ToJson(w, true);
                        break;
                    }
                case View.DrumPatterns:
                    json = JsonUtility.ToJson(BuildPatternWrap(
                        _drums, d => (d.DisplayName, d.TimeSignature, d.Measures,
                            d.subdivisions, d.lanes?.Count ?? 0, DrumFlags(d)),
                        _drumPalettes.Cast<UnityEngine.Object>(),
                        pal => (((DrumPatternPaletteSO)pal).GetDisplayName(),
                                ((DrumPatternPaletteSO)pal).entries?.Count ?? 0)), true);
                    break;
                case View.ChordProgressions:
                    {
                        var w = BuildPatternWrap(
                            _chords, p => (p.DisplayName, p.TimeSignature, p.Measures,
                                p.subdivisions, p.events?.Count ?? 0, ProgressionFlags(p)),
                            _chordPalettes.Cast<UnityEngine.Object>(),
                            pal => (((ChordProgressionPaletteSO)pal).GetDisplayName(),
                                    ((ChordProgressionPaletteSO)pal).entries?.Count ?? 0));
                        foreach (var lib in _libraries)
                            w.palettes.Add(new JsonPalette
                            {
                                type = nameof(ChordProgressionLibrarySO),
                                assetName = lib.name,
                                displayName = lib.name,
                                entryCount = lib.entries?.Count ?? 0,
                                refs = RefsLabel(lib),
                                orphan = IsOrphan(lib),
                                source = SourceTag(lib),
                                assetPath = AssetDatabase.GetAssetPath(lib)
                            });
                        json = JsonUtility.ToJson(w, true);
                        break;
                    }
                case View.MelodyAndPhrases:
                    {
                        var w = BuildPatternWrap(
                            _melodies, m => (m.DisplayName, m.TimeSignature, m.Measures,
                                m.subdivisions, m.notes?.Count ?? 0, MelodyFlags(m)),
                            _phrasePalettes.Cast<UnityEngine.Object>(),
                            pal => (((PhrasePaletteSO)pal).name,
                                    ((PhrasePaletteSO)pal).archetypes?.Count ?? 0));
                        foreach (var a in _archetypes)
                            w.palettes.Add(new JsonPalette
                            {
                                type = a.GetType().Name,
                                assetName = a.name,
                                displayName = a.name,
                                entryCount = 0,
                                refs = RefsLabel(a),
                                orphan = IsOrphan(a),
                                source = SourceTag(a),
                                assetPath = AssetDatabase.GetAssetPath(a)
                            });
                        json = JsonUtility.ToJson(w, true);
                        break;
                    }
                case View.MelodicInstruments:
                    json = JsonUtility.ToJson(BuildInstrumentWrap(_melInstruments), true);
                    break;
                case View.PercussionInstruments:
                    json = JsonUtility.ToJson(BuildInstrumentWrap(
                        _percInstruments.Cast<MIDIInstrumentSO>()), true);
                    break;
                case View.NamesReport:
                    {
                        var w = new WrapNames();
                        void F<T>(string fam, IEnumerable<T> xs, Func<T, string> d)
                            where T : UnityEngine.Object
                        {
                            foreach (var x in xs.Where(x => x != null))
                                w.names.Add(new JsonName
                                {
                                    family = fam,
                                    assetName = x.name,
                                    displayName = d(x),
                                    source = SourceTag(x),
                                    assetPath = AssetDatabase.GetAssetPath(x)
                                });
                        }
                        F("StyleBundle", _bundles, b => "");
                        F("DrumPattern", _drums, x => x.DisplayName);
                        F("DrumPalette", _drumPalettes, x => x.GetDisplayName());
                        F("ChordProgression", _chords, x => x.DisplayName);
                        F("ChordPalette", _chordPalettes, x => x.GetDisplayName());
                        F("ChordLibrary", _libraries, x => "");
                        F("MelodyPattern", _melodies, x => x.DisplayName);
                        F("PhraseArchetype", _archetypes, x => "");
                        F("PhrasePalette", _phrasePalettes, x => "");
                        F("MelodicInstrument", _melInstruments, x => x.InstrumentName);
                        F("PercussionInstrument", _percInstruments, x => x.InstrumentName);
                        json = JsonUtility.ToJson(w, true);
                        break;
                    }
                default: json = "{}"; break;
            }

            return json;
        }

        private WrapPatterns BuildPatternWrap<T>(
            IEnumerable<T> patterns,
            Func<T, (string display, TimeSignature ts, int measures, int subdiv,
                     int content, List<string> flags)> map,
            IEnumerable<UnityEngine.Object> palettes,
            Func<UnityEngine.Object, (string display, int entries)> palMap)
            where T : PatternDataSO
        {
            var w = new WrapPatterns();
            foreach (var p in patterns.Where(p => p != null))
            {
                var (display, ts, measures, subdiv, content, flags) = map(p);
                w.patterns.Add(new JsonPattern
                {
                    assetName = p.name,
                    displayName = display,
                    timeSignature = ts.ToString(),
                    measures = measures,
                    subdivisions = subdiv,
                    contentCount = content,
                    source = SourceTag(p),
                    refs = RefsLabel(p),
                    flags = string.Join(";", flags),
                    assetPath = AssetDatabase.GetAssetPath(p)
                });
            }
            foreach (var pal in palettes.Where(p => p != null))
            {
                var (display, entries) = palMap(pal);
                w.palettes.Add(new JsonPalette
                {
                    type = pal.GetType().Name,
                    assetName = pal.name,
                    displayName = display,
                    entryCount = entries,
                    refs = RefsLabel(pal),
                    orphan = IsOrphan(pal),
                    source = SourceTag(pal),
                    assetPath = AssetDatabase.GetAssetPath(pal)
                });
            }
            return w;
        }

        private WrapInstruments BuildInstrumentWrap(IEnumerable<MIDIInstrumentSO> list)
        {
            var w = new WrapInstruments();
            foreach (var i in list.Where(i => i != null))
                w.instruments.Add(new JsonInstrument
                {
                    assetName = i.name,
                    instrumentName = i.InstrumentName,
                    instrumentType = i.InstrumentType.ToString(),
                    soundFont = i.SelectedSoundFont,
                    bank = i.BankName,
                    patch = i.PatchName,
                    patchIndex = i.PatchIndex,
                    octaveMin = i.octaveMin,
                    octaveMax = i.octaveMax,
                    volume01 = i.volume01,
                    percussionMappings = (i as MIDIPercussionInstrumentSO)
                        ?.percussionMappings?.Count ?? 0,
                    flags = string.Join(";", InstrumentFlags(i)),
                    source = SourceTag(i),
                    assetPath = AssetDatabase.GetAssetPath(i)
                });
            return w;
        }

        private static string BundleOverrideName(TrackStyleBundleSO b) => b switch
        {
            RhythmCardConfigSO r => r.patternOverride ? r.patternOverride.name : null,
            BackingCardConfigSO bk => bk.progressionOverride ? bk.progressionOverride.name : null,
            MelodyCardConfigSO m => m.patternOverride ? m.patternOverride.name : null,
            _ => null
        };

        private static string BundlePaletteName(TrackStyleBundleSO b) => b switch
        {
            RhythmCardConfigSO r => r.patternPalette ? r.patternPalette.name : null,
            BackingCardConfigSO bk => bk.progressionPalette ? bk.progressionPalette.name : null,
            MelodyCardConfigSO m => m.phrasePaletteOverride ? m.phrasePaletteOverride.name : null,
            _ => null
        };

        // [CSV-1b] Order-preserving de-duplicated union. Reference equality is
        // the right identity here: both sources return the same loaded instance
        // for the same asset.
        private static List<T> UnionAssets<T>(IEnumerable<T> a, IEnumerable<T> b)
            where T : UnityEngine.Object
        {
            var seen = new HashSet<T>();
            var result = new List<T>();
            foreach (var src in new[] { a, b })
            {
                if (src == null) continue;
                foreach (var x in src)
                    if (x != null && seen.Add(x)) result.Add(x);
            }
            return result;
        }

        private static List<T> FindAllAssets<T>() where T : ScriptableObject
        {
            var list = new List<T>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var a = AssetDatabase.LoadAssetAtPath<T>(p);
                if (a != null) list.Add(a);
            }
            return list;
        }
    }
}
#endif
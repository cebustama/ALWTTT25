using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Music;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MidiGenPlay;
using MidiGenPlay.Composition; // MGP-ALWTTT-DBG-1: MusicianTrackKey
using MidiGenPlay.Interfaces;
using MidiGenPlay.MusicTheory;
using MidiGenPlay.Services;
using MidiPlayerTK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;
using static UnityEngine.Rendering.STP;

namespace ALWTTT.Managers
{
    public class MidiMusicManager : MonoBehaviour
    {
        private const string DebugTag = "<color=white>[MidiMusicManager]</color>";

        public static MidiMusicManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private MidiGenPlayConfig settings;

        // [SOLO-1 / D-R2-6=B] Host default harmony for backing-less parts
        // (MGP-ALWTTT-BASS-SOLO-1). Consumed by RenderSinglePart.
        [Header("Harmony Defaults")]
        [SerializeField, Tooltip(
            "Fallback harmony palette for parts that have a harmony consumer " +
            "(Bassline/Melody/Harmony) but NO Backing track. One progression is " +
            "picked per (songSeed, partIndex): stable within a song (cache-" +
            "coherent), varies across songs. UNASSIGNED = legacy behavior (such " +
            "parts render harmony silence). Leave unassigned in the demo scene " +
            "config: S5i inertness by construction.")]
        private ChordProgressionPaletteSO defaultProgressionPalette;

        [Header("Refs")]
        [SerializeField] private MonoBehaviour playerBehaviour; // IPlayMidi (MPTK)

        [Header("Options")]
        // [LOG-1] Master switch, but the inspector value decides nothing: it
        // is overwritten unconditionally at boot from
        // MidiGenPlayConfig.logMidiMusicManager (see Init below). Hidden so it
        // stops inviting people to flip a control that has no effect.
        [HideInInspector, SerializeField] private bool logDebug = true;

        // [LOG-1 / D-LOG-3=B] Second tier, host-owned, NOT overwritten at
        // boot. Gates only the chatty per-render dumps. The six test-bearing
        // lines stay on logDebug alone and must remain visible with this OFF:
        //   [ORDER-1] . [B1][stemCache] (WITHOUT [DIAG]) .
        //   [DBG-C2/CacheBypass] . "Timeline ch="
        [SerializeField, Tooltip("Second logging tier for this manager. " +
            "Leave OFF for a readable console. Does NOT gate [ORDER-1], " +
            "[B1][stemCache], [DBG-C2/CacheBypass] or 'Timeline ch='.")]
        private bool logVerbose = false;
        public bool MetronomeEnabled { get; private set; }
        private const string CacheEpoch = "v2-metro";

        // Cache (song key -> data + duration)
        private readonly Dictionary<string, SongCacheEntry> cache = new();

        [Serializable]
        private class SongCacheEntry
        {
            public byte[] data;
            public float seconds;
        }

        // ─────────────────────────────────────────────────────────────
        // [B1] Per-track stem cache + per-part bundle cache
        // SSoT: SSoT_Runtime_CompositionSession_Integration.md
        // Decisions: D2=B (per-track persistence), D6=A (per-track scope),
        // D7=B (per-song lifetime), D-A1=A, D-A2=A, D-C=α (two-dict).
        //
        // _stemCache: verbatim per-musician stem bytes, keyed on
        //   "{musicianId}:{role}|{trackInputsHash}|{partMeterHash}". Survives
        //   PartCache invalidations within a song; reset at song boundary
        //   via ResetStemCache().
        //
        // _partBundleCache: full RenderSinglePart output keyed on
        //   "{partMeterHash}@@{sorted musician:role#trackHash csv}". When every
        //   track input is identical to a prior render in this song, we
        //   skip GenerateSinglePart entirely and replay the cached bundle.
        //
        // Invalidation on F-4 Stage A catch is per-part: every entry that
        // contains the affected partMeterHash drops. Other parts in the
        // same song untouched.
        // ─────────────────────────────────────────────────────────────
        private readonly Dictionary<string, byte[]> _stemCache = new();
        private readonly Dictionary<string, PartBundleCacheEntry> _partBundleCache = new();

        private class PartBundleCacheEntry
        {
            public byte[] mergedBytes;
            // [DBG-C1] Re-keyed on (musicianId, TrackRole) end-to-end.
            public Dictionary<MusicianTrackKey, byte[]> stemsByTrack;
            public float seconds;
            public int bpmChosen;
            public Dictionary<MusicianTrackKey, MIDIInstrumentSO> pinned;
            // [DBG-C1 / D-DBG5=A] Snapshot of the render's resolvedByTrack so
            // a bundle-cache replay still surfaces the ORIGINAL render's truth
            // (replayed bytes == original bytes ⇒ original choices are still
            // the truth; no dry resolve exists or is wanted).
            public Dictionary<MusicianTrackKey, ResolvedTrackChoice> resolvedByTrack;
            // [BAL-1 task 4] CC7 actually emitted per gained track for these bytes.
            public Dictionary<MusicianTrackKey, int> appliedCc7ByTrack; // [BAL-1] existing
            // [ORDER-1 / R2d] Which source won the shared progression channel for
            // these bytes, and the asset behind it. Same D-DBG5=A logic as above:
            // replayed bytes == original bytes ⇒ the original verdict still holds.
            // Verification surface only — NEVER a cache-key input (see the
            // harmony-identity token in RenderSinglePart for why).
            public ResolvedSource sharedProgressionSource;
            public string sharedProgressionAssetName;

            // [JAM-1 / MGP-MEL-1b P7] Runtime clone of the harmony that won the
            // shared channel for these bytes. Same D-DBG5=A logic as its two
            // siblings above: replayed bytes == original bytes ⇒ the original
            // progression is still the truth for this entry.
            public MidiGenPlay.ChordProgressionData sharedProgressionData;
#if ALWTTT_DEV
            // [CSV-3] Resolved musical identity of the ORIGINAL render — D-DBG5=A analogue of
            // appliedCc7ByTrack: replay bytes == original ⇒ resolved identity identical.
            public MidiGenPlay.MusicTheory.MusicTheory.TimeSignature resolvedTs;
            public MidiGenPlay.MusicTheory.MusicTheory.Tonality resolvedTonality;
            public Melanchall.DryWetMidi.MusicTheory.NoteName resolvedRoot;
#endif
        }

        private GameManager GameManager => GameManager.Instance;

        private MidiGenerator generator;

        private IPlayMidi player;
        private IMixController mix;
        private IInstrumentRepository instrumentRepo;
        private IPatternRepository patternRepo;

        private Action _onSongStartedHandler;
        private Action _onSongEndedHandler;

        private string _seedKeyForThisGeneration;
        private bool _bypassCacheNext = false;

        #region Registries (loaded once)
        private bool registriesLoaded;

        private List<MIDIInstrumentSO> allInstruments = new();
        private List<MIDIInstrumentSO> melodicInstruments = new();
        private List<MIDIPercussionInstrumentSO> percussionInstruments = new();

        private List<DrumPatternData> allDrumPatterns = new();
        private List<ChordProgressionData> allChordPatterns = new();
        private List<MelodyPatternData> allMelodyPatterns = new();

        // Read-only accessors
        public IReadOnlyList<MIDIInstrumentSO> AllInstruments =>
            new ReadOnlyCollection<MIDIInstrumentSO>(allInstruments);
        public IReadOnlyList<MIDIInstrumentSO> MelodicInstruments =>
            new ReadOnlyCollection<MIDIInstrumentSO>(melodicInstruments);
        public IReadOnlyList<MIDIPercussionInstrumentSO> PercussionInstruments =>
            new ReadOnlyCollection<MIDIPercussionInstrumentSO>(percussionInstruments);
        public IReadOnlyList<DrumPatternData> DrumPatterns =>
            new ReadOnlyCollection<DrumPatternData>(allDrumPatterns);
        public IReadOnlyList<ChordProgressionData> ChordPatterns =>
            new ReadOnlyCollection<ChordProgressionData>(allChordPatterns);
        public IReadOnlyList<MelodyPatternData> MelodyPatterns =>
            new ReadOnlyCollection<MelodyPatternData>(allMelodyPatterns);
        #endregion

        #region Midi
        private readonly Dictionary<string, List<string>> channelOwnersByKey = new(); // channel idx -> musicianId
        private readonly Dictionary<string, List<TrackRole>> channelRolesByKey = new();

        // channel index -> musicianId for the CURRENT arrangement (full band)
        private readonly List<string> _channelOwners = new();
        private readonly Dictionary<string, Transform> _musicianAnchors = new();

        // Subscriber lists (scene systems register/unregister)
        private readonly List<IMidiNoteListener> _noteSubs = new();
        private readonly List<IChordListener> _chordSubs = new();
        private readonly List<IBeatGridListener> _gridSubs = new();
        private readonly List<IDrumKickListener> _kickSubs = new();
        private readonly HashSet<IPartInfoListener> _partListeners = new();

        private Coroutine _beatGridCo;
        private readonly List<ITempoSignatureListener> _tempoSigSubs = new();

        // Beat detection (very simple: kick = beat)
        [SerializeField] private int drumChannel = 9; // MIDI ch 10 (0-based = 9)
        [SerializeField] private int[] kickNotes = new[] { 35, 36 }; // Acoustic/Bass drum
        private int _beatIndex = 0;

        // Chord labels
        private readonly Dictionary<int, Dictionary<long, ChordLabel>>
            _chordLabelsByTrack = new();
        private readonly Dictionary<int, Dictionary<long, ChordLabel>>
            _chordLabelsByChannel = new();

        // ordered timelines & cursors per channel
        private TempoMap _tempoMapForCurrentSong;
        private readonly Dictionary<int, List<(long tick, ChordLabel label)>>
            _chordTimelineByChannel = new();
        private readonly Dictionary<int, int> _chordTimelineCursor = new();
        private readonly Dictionary<int, ChordLabel> _currentChordByChannel = new();

        // [LOG-1] One chd:-damage report per BuildChordMarkers call, not one
        // per marker. Reset at the top of BuildChordMarkers.
        private bool _chordTagDamageReported;

        private struct ChordLabel
        {
            public string sym;      // "Cm7"
            public string roman;    // "ii" / "IV"
            public int deg;         // 1..7 (0 si no aplica)
            public ChordQuality? quality; // null si no aplica
        }

        private struct PartMarker
        {
            public long tick;
            public PartInfoEvent info;
            public bool fired;
        }
        private List<PartMarker> _partMarkers = new();

        public void Register(IMidiNoteListener l)
        {
            if (l != null && !_noteSubs.Contains(l)) _noteSubs.Add(l);
        }
        public void Unregister(IMidiNoteListener l)
        {
            _noteSubs.Remove(l);
        }

        public void Register(IChordListener l)
        {
            if (l != null && !_chordSubs.Contains(l)) _chordSubs.Add(l);
        }
        public void Unregister(IChordListener l)
        {
            _chordSubs.Remove(l);
        }

        public void Register(IBeatGridListener l)
        { if (l != null && !_gridSubs.Contains(l)) _gridSubs.Add(l); }
        public void Unregister(IBeatGridListener l) { _gridSubs.Remove(l); }

        public void Register(IDrumKickListener l)
        { if (l != null && !_kickSubs.Contains(l)) _kickSubs.Add(l); }
        public void Unregister(IDrumKickListener l) { _kickSubs.Remove(l); }

        public void Register(ITempoSignatureListener l)
        {
            if (l != null && !_tempoSigSubs.Contains(l)) _tempoSigSubs.Add(l);
        }
        public void Unregister(ITempoSignatureListener l) { _tempoSigSubs.Remove(l); }

        public void Register(IPartInfoListener l) => _partListeners.Add(l);
        public void Unregister(IPartInfoListener l) => _partListeners.Remove(l);

        private readonly Dictionary<string, Dictionary<int, string>> trackOwnersByKey =
            new(); // cacheKey -> (trackIndex -> musicianId)
        private string _currentKey;

        // index = channel, value = musicianId
        public void SetChannelOwners(List<string> owners)
        {
            _channelOwners.Clear();
            if (owners != null) _channelOwners.AddRange(owners);

            //Debug.Log($"<color=red>CHANNEL OWNERS SET {string.Join(", ", owners)}</color>");
        }
        public void RegisterMusicianAnchor(string musicianId, Transform anchor)
        {
            if (string.IsNullOrEmpty(musicianId) || anchor == null) return;
            _musicianAnchors[musicianId] = anchor;
        }

        #endregion

        #region Setup
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            player = playerBehaviour as IPlayMidi ??
                FindFirstObjectByType<MidiToolkitAdapter>();

            if (player == null)
            {
                Debug.LogError(
                    $"{DebugTag} No IPlayMidi found. Add MidiToolkitAdapter to Core.");
                return;
            }

            mix = new PassthroughMixController(player);
            for (int ch = 0; ch < 16; ch++) _lastKnownVol01[ch] = 1f;
            _lastKnownVol01[MidiGenerator.MetronomeChannel] = 0f; // default: metronome off

            // Global MGP Settings
            if (settings == null) settings = MidiGenPlayConfig.FindInResources();
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<MidiGenPlayConfig>();
            }
            logDebug = settings != null && settings.logMidiMusicManager;

            instrumentRepo = new InstrumentRepositoryResources(settings);
            patternRepo = new PatternRepositoryResources(settings);

            // MIDI EVENTS
            player.OnMidiEvents += HandleMidiEvents;

            _onSongStartedHandler = OnSongStartedInternal;
            _onSongEndedHandler = OnSongEndedInternal;
            player.OnSongStarted += _onSongStartedHandler;
            player.OnSongEnded += _onSongEndedHandler;

            var actualVoicer = new MidiGenPlay.Composition.BasicVoiceLeadingVoicer();

            generator = new MidiGenerator(settings, actualVoicer);

            EnsureRegistriesLoaded();
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnMidiEvents -= HandleMidiEvents;
                if (_onSongStartedHandler != null)
                    player.OnSongStarted -= _onSongStartedHandler;
                if (_onSongEndedHandler != null)
                    player.OnSongEnded -= _onSongEndedHandler;
            }
        }
        private void EnsureRegistriesLoaded()
        {
            if (registriesLoaded) return;

            instrumentRepo.Refresh();
            patternRepo.Refresh();

            var mel = instrumentRepo.GetMelodicInstruments().ToList();
            var perc = instrumentRepo.GetPercussionInstruments().ToList();

            melodicInstruments = mel;
            percussionInstruments = perc;
            allInstruments = mel.Cast<MIDIInstrumentSO>().Concat(perc).ToList();

            allDrumPatterns = patternRepo.GetAllDrumPatterns().ToList();
            allChordPatterns = patternRepo.GetAllChordProgressions().ToList();
            allMelodyPatterns = patternRepo.GetAllMelodyPatterns().ToList();

            registriesLoaded = true;
            if (logDebug)
                Debug.Log($"{DebugTag} Registries loaded. " +
                          $"Instruments mel:{mel.Count} perc:{perc.Count} | " +
                          $"Patterns chords:{allChordPatterns.Count} drums:{allDrumPatterns.Count} " +
                          $"melodies:{allMelodyPatterns.Count}");
        }

        // stable 32-bit FNV-1a hash (deterministic across runs)
        private static int StableHash32(string s)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in s)
                {
                    hash ^= c;
                    hash *= 16777619;
                }
                return (int)hash;
            }
        }

        public void BypassCacheOnce() { _bypassCacheNext = true; }
        #endregion

        private void Update()
        {
            if (player == null || !player.IsPlaying || _partMarkers.Count == 0) return;

            long curTick = player.CurrentTick; // via IPlayMidi
            for (int i = 0; i < _partMarkers.Count; i++)
            {
                if (!_partMarkers[i].fired && curTick >= _partMarkers[i].tick)
                {
                    EmitPartStarted(_partMarkers[i].info);
                    var pm = _partMarkers[i];
                    pm.fired = true;
                    _partMarkers[i] = pm;
                }
            }
        }

        #region Public Methods
        public void GenerateSongs(IEnumerable<SongData> songs)
        {
            EnsureRegistriesLoaded();

            if (logDebug)
                Debug.Log($"{DebugTag} Generating songs...");

            var band = GameManager.PersistentGameplayData.MusicianList;

            foreach (var s in songs)
            {
                var key = CacheKey(s, band);
                if (cache.ContainsKey(key)) continue;

                var entry = GenerateSongEntry(s);   // uses current band internally (PD.MusicianList)
                if (entry != null)
                    cache[key] = entry;
            }

            if (logDebug)
                Debug.Log($"{DebugTag} Pre-generated {cache.Count} songs in cache.");
        }

        public float Play(SongData song)
        {
            var (key, entry) = EnsureInCache(song, null);
            if (entry == null) return 0f;

            var bytes = ApplyMetronomeVolumeToBytes(entry.data, MetronomeEnabled);
            LogPlayTraceSummary(key, $"'{song.SongTitle}'", entry.seconds, bytes?.Length ?? 0);

            return PlayBytes(key, bytes, entry.seconds, $"'{song.SongTitle}'");
        }

        public float PlayRaw(byte[] data, float seconds, string titleForLogs)
        {
            if (data == null || data.Length == 0) return 0f;
            var bytes = ApplyMetronomeVolumeToBytes(data, MetronomeEnabled);
            return PlayBytes($"jam-raw::{Guid.NewGuid():N}", bytes, seconds, titleForLogs);
        }

        public float PlaySameArrangementSubsetByMusicians(
            SongData song, IReadOnlyList<string> entranceOrderIds, int takeCount)
        {
            var (key, full) = EnsureInCache(song, null);
            if (full == null) return 0f;

            if (!channelOwnersByKey.TryGetValue(key, out var owners) || owners == null)
                return PlayBytes(key, full.data, full.seconds,
                    $"'{song.SongTitle}' (fallback full)");

            var allowed = new HashSet<int>();
            for (int i = 0; i < takeCount && i < entranceOrderIds.Count; i++)
            {
                var id = entranceOrderIds[i];
                // [BASS-1 / R15] A musician may own MULTIPLE channels (one per
                // role-track) — unmute all of them, not just the first.
                for (int ch = 0; ch < owners.Count; ch++)
                    if (owners[ch] == id) allowed.Add(ch);
            }
            if (allowed.Count == 0)
                return PlayBytes(key, full.data, full.seconds,
                    $"'{song.SongTitle}' (fallback full)");

            var maskedData = BuildChannelMaskedData(full.data, allowed);
            maskedData = ApplyMetronomeVolumeToBytes(maskedData, MetronomeEnabled);

            // DEBUG: confirm metro channel is still present
            using (var ms = new MemoryStream(maskedData))
            {
                var mf = MidiFile.Read(ms);
                var kept = string.Join(",", GetUsedChannels(mf));
                /*Debug.Log($"{DebugTag} subset kept channels [{kept}] " +
                    $"(must include {MidiGenerator.MetronomeChannel})");*/
            }

            LogPlayTraceSummary(key, $"subset[{allowed.Count}] '{song.SongTitle}'",
                full.seconds, maskedData?.Length ?? 0);

            return PlayBytes(key, maskedData, full.seconds,
                $"subset[{allowed.Count}] '{song.SongTitle}'");
        }

        // ─────────────────────────────────────────────────────────────
        // [BASS-1 / R13] Channel stamping helpers. ChannelMusicianOrder,
        // ChannelRoles, and channelMap are index-parallel lists (seeded from
        // Part 0 by SongConfigBuilder). A musician may now own MULTIPLE
        // channels (one per role-track), so a musician-only dictionary is
        // last-wins-ambiguous. Primary lookup is (musicianId, role); the
        // musician-only map (first-wins) remains as fallback for tracks whose
        // role wasn't present in the Part-0 seed. Identical behavior to the
        // legacy code for single-track-per-musician content.
        // ─────────────────────────────────────────────────────────────
        private static void BuildChannelOwnerLookups(
            SongConfig config, List<int> channelMap,
            out Dictionary<string, int> byMusicianRole,
            out Dictionary<string, int> byMusician)
        {
            byMusicianRole = new Dictionary<string, int>();
            byMusician = new Dictionary<string, int>();
            var roles = config.ChannelRoles;
            for (int i = 0;
                 i < config.ChannelMusicianOrder.Count && i < channelMap.Count; i++)
            {
                var id = config.ChannelMusicianOrder[i] ?? "";
                if (string.IsNullOrEmpty(id)) continue;
                if (roles != null && i < roles.Count)
                    byMusicianRole[$"{id}|{roles[i]}"] = channelMap[i];
                if (!byMusician.ContainsKey(id)) byMusician[id] = channelMap[i];
            }
        }

        private static void StampChannel(
            SongConfig.PartConfig.TrackConfig tr,
            Dictionary<string, int> byMusicianRole,
            Dictionary<string, int> byMusician)
        {
            if (tr == null || string.IsNullOrEmpty(tr.MusicianId)) return;
            if (byMusicianRole.TryGetValue($"{tr.MusicianId}|{tr.Role}", out var ch)
                || byMusician.TryGetValue(tr.MusicianId, out ch))
                tr.Channel = ch;
        }

        public float PlayFromConfig(
            SongConfig config, string title, IList<Characters.Band.MusicianBase> band)
        {
            EnsureRegistriesLoaded();
            if (config == null)
            {
                Debug.LogError($"{DebugTag} PlayFromConfig: null config");
                return 0f;
            }

            // Build channel map and ownership just like the cached path
            var channelMap = BuildChannelMap(config.ChannelRoles ?? new List<TrackRole>());
            BuildChannelOwnerLookups(config, channelMap,
                out var byMusicianRole, out var byMusician); // [BASS-1 / R13]

            foreach (var part in config.Parts)
                foreach (var tr in part.Tracks)
                    StampChannel(tr, byMusicianRole, byMusician);

            // Persist routing maps under a unique ephemeral key (so OnMidiEvents works)
            var key = $"jam::{Guid.NewGuid():N}";
            {
                int maxCh = channelMap.Count > 0 ? channelMap.Max() : 0;
                var perChannel =
                    Enumerable.Repeat(string.Empty, Math.Max(16, maxCh + 1)).ToList();
                for (int i = 0;
                    i < config.ChannelMusicianOrder.Count && i < channelMap.Count; i++)
                {
                    int ch = channelMap[i];
                    if (ch >= 0 && ch < perChannel.Count)
                        perChannel[ch] = config.ChannelMusicianOrder[i];
                }
                channelOwnersByKey[key] = perChannel;
                channelRolesByKey[key] = config.ChannelRoles?.ToList() ??
                    new List<TrackRole>();
            }

            // Build a deterministic seed key
            //var bandSig = ComputeBandSignature(band);
            //var seedKey = $"{title ?? "jam"}::{bandSig}";
            //settings.defaultSeed = StableHash32(seedKey);

            // Generate MIDI
            var midi = generator.GenerateSong(config);

            // Compute seconds and serialize to bytes
            var tempoMap = midi.GetTempoMap();
            var last = midi.GetTrackChunks()
                           .SelectMany(c => c.GetTimedEvents())
                           .Select(te => te.Time).DefaultIfEmpty(0).Max();
            var seconds =
                (float)TimeConverter.ConvertTo<MetricTimeSpan>(last, tempoMap).TotalSeconds;

            using var ms = new MemoryStream();
            midi.Write(ms);
            var data = ms.ToArray();

            // Respect metronome toggle and play
            var bytes = ApplyMetronomeVolumeToBytes(data, MetronomeEnabled);
            LogPlayTraceSummary(key, $"'{title}'", seconds, bytes?.Length ?? 0);
            return PlayBytes(key, bytes, seconds, $"'{title}'");
        }

        public void Stop()
        {
            player?.Stop();
            if (logDebug) Debug.Log($"{DebugTag} Stop");
            ClearMarkers();
        }

        public IReadOnlyList<string> GetChannelOwnerIdsFor(SongData song)
        {
            var key = CacheKey(song, GameManager.PersistentGameplayData.MusicianList);
            if (channelOwnersByKey.TryGetValue(key, out var list)) return list;
            // force-generate if missing (shouldn't happen if GenerateSongs/Play called first)
            var entry =
                GenerateSongEntry(song, GameManager.PersistentGameplayData.MusicianList);
            cache[key] = entry;
            return channelOwnersByKey[key];
        }

        public IReadOnlyList<string> GetChannelOwnerIdsFor(SongConfig config)
        {
            if (config == null)
            {
                if (logDebug && logVerbose)   // [LOG-1] verbose
                    Debug.Log($"{DebugTag} GetChannelOwnerIdsFor(SongConfig): " +
                        $"config is NULL");
                return Array.Empty<string>();
            }

            var channelRoles = config.ChannelRoles ?? new List<TrackRole>();
            //Debug.Log($"<color=red>Channel Roles {string.Join(", ", channelRoles)}</color>");

            var channelMap = BuildChannelMap(channelRoles);
            //Debug.Log($"<color=red>Channel Map {string.Join(", ", channelMap)}</color>");

            var musicianOrder = config.ChannelMusicianOrder ?? new List<string>();
            //Debug.Log($"<color=red>Musician Order {string.Join(", ", musicianOrder)}</color>");

            int maxCh = channelMap.Count > 0 ? channelMap.Max() : 0;
            var perChannel = Enumerable
                .Repeat(string.Empty, Math.Max(16, maxCh + 1))
                .ToList();

            // For each logical channel index in the config, map it to the
            // actual MIDI channel number and assign the corresponding musicianId.
            for (int i = 0; i < musicianOrder.Count && i < channelMap.Count; i++)
            {
                int ch = channelMap[i];
                if (ch < 0 || ch >= perChannel.Count)
                    continue;

                var id = musicianOrder[i] ?? string.Empty;
                perChannel[ch] = id;
            }

            if (logDebug && logVerbose)   // [LOG-1] verbose: 16-column dump
            {
                var summary = string.Join(
                    ", ",
                    perChannel.Select((id, ch) => $"{ch}:{(string.IsNullOrEmpty(id) ? "-" : id)}"));
                Debug.Log($"{DebugTag} GetChannelOwnerIdsFor(SongConfig) -> [{summary}]");
            }

            return perChannel;
        }

        /// <summary>
        /// [R5-d] MIDI channel a given (musicianId, role) track resolves to under
        /// this config's layout. Same lookup StampChannel uses — composite key
        /// first, musician-wide fallback second — exposed so the host can address
        /// one track's channel (e.g. to exempt it from a duck). Returns -1 when
        /// the musician is absent from the layout entirely.
        /// </summary>
        public int GetChannelForTrack(SongConfig cfg, string musicianId, TrackRole role)
        {
            if (cfg == null || string.IsNullOrEmpty(musicianId)) return -1;
            if (cfg.ChannelMusicianOrder == null) return -1;

            var channelMap = BuildChannelMap(cfg.ChannelRoles ?? new List<TrackRole>());
            BuildChannelOwnerLookups(cfg, channelMap,
                out var byMusicianRole, out var byMusician);

            if (byMusicianRole.TryGetValue($"{musicianId}|{role}", out var ch))
                return ch;

            return byMusician.TryGetValue(musicianId, out ch) ? ch : -1;
        }

        public IEnumerator WaitForEnd()
        {
            if (player == null) yield break;
            yield return player.WaitForEnd();
        }

        public void SetMetronomeEnabled(bool enabled)
        {
            MetronomeEnabled = enabled;
            var metro01 = enabled ? Mathf.Clamp01((settings?.metronomeChannelVolume ?? 110) / 127f) : 0f;
            _lastKnownVol01[MidiGenerator.MetronomeChannel] = metro01;
            mix?.SetChannelVolume01(MidiGenerator.MetronomeChannel, metro01);
        }

        /// <summary>
        /// Returns true if the underlying MIDI player is currently playing any song/part.
        /// Passthrough of IPlayMidi.IsPlaying.
        /// Safe to call every frame.
        /// </summary>
        public bool IsAnySongPlaying()
        {
            return player != null && player.IsPlaying;
        }

        /// <summary>
        /// Current MIDI tick position of the active playback, or 0 if nothing is playing.
        /// </summary>
        public long GetCurrentTick()
        {
            return player != null ? player.CurrentTick : 0L;
        }

        /// <summary>
        /// Returns true if we have a valid MIDI player reference at all.
        /// </summary>
        public bool HasPlayer()
        {
            return player != null;
        }

        /// <summary>
        /// Render exactly one part from a full SongConfig (using its channel ordering),
        /// returning merged bytes, per-musician stems, and the duration in seconds.
        /// </summary>
        public (byte[] merged,
                Dictionary<MusicianTrackKey, byte[]> stemsByTrack,
                float seconds,
                int bpmChosen,
                Dictionary<MusicianTrackKey, MIDIInstrumentSO> pinned)
            RenderSinglePart(
                SongConfig fullCfg,
                int partIndex,
                int? bpmOverride = null,
                // [DBG-C1] All per-track surfaces keyed (musicianId, TrackRole).
                Dictionary<MusicianTrackKey, MIDIInstrumentSO> instrumentOverrides = null,
                Dictionary<MusicianTrackKey, string> trackInputsHashByTrack = null,
                int? seedOverride = null,   // [S5g / MGP-ALWTTT-SEED-1] per-song seed, host policy
                                            // [DBG-C1 / D-C1-1] Inert passthrough this batch (always null);
                                            // DBG-C2 wires the override UI. Precedence step 0 package-side.
                IReadOnlyDictionary<MusicianTrackKey, PatternDataSO> patternOverrides = null)
        {
            EnsureRegistriesLoaded();
            if (fullCfg == null || partIndex < 0 || partIndex >= fullCfg.Parts.Count)
                return (null, null, 0f, 0, null);

            // Build channel map from the global ChannelRoles of this config
            var channelMap = BuildChannelMap(fullCfg.ChannelRoles ?? new List<TrackRole>());

            // [BAL-1] Channel-gain snapshot for the live plane (D-BAL-6=B).
            // Computed on EVERY render — including bundle-cache hits — so the
            // following PlayBytes adopts the gains of exactly these bytes.
            _pendingChannelGains = BuildChannelGains(fullCfg, channelMap);

            // Stamp channels into each track (like PlayFromConfig)
            BuildChannelOwnerLookups(fullCfg, channelMap,
                out var byMusicianRole, out var byMusician); // [BASS-1 / R13]

            var part = fullCfg.Parts[partIndex];
            foreach (var tr in part.Tracks)
                StampChannel(tr, byMusicianRole, byMusician);

            int? effectiveOverride = bpmOverride;
            if (!effectiveOverride.HasValue)
            {
                string mode;
                int baseBpm;

                if (part.ExplicitBpm.HasValue)
                {
                    // Caso 1: BPM fijo
                    baseBpm = part.ExplicitBpm.Value;
                    mode = $"explicit({baseBpm})";
                }
                else
                {
                    // Caso 2: rango
                    baseBpm = MusicTheory.GetBPMFromRange(part.TempoRange, TempoRule.MultiplesOfTen);
                    mode = $"range({part.TempoRange})";
                }

                // Aplicar escala si procede
                if (!Mathf.Approximately(part.TempoScale, 1f))
                {
                    baseBpm = Mathf.RoundToInt(baseBpm * part.TempoScale);
                    baseBpm = Mathf.Max(40, baseBpm); // safety floor
                    mode += $" * scale({part.TempoScale:0.##})";
                }

                effectiveOverride = baseBpm;

                if (logDebug && logVerbose)   // [LOG-1] verbose: duplicated by
                {                             // "[BPM] Part=N resolved BPM=", which stays
                    Debug.Log(
                        $"{DebugTag} [BPM] Resolve part {partIndex} '{part.Name}': " +
                        $"mode={mode} -> BPM={baseBpm} | " +
                        $"Explicit={part.ExplicitBpm?.ToString() ?? "null"}, " +
                        $"TempoRange={part.TempoRange}, TempoScale={part.TempoScale:0.##}");
                }
            }
            else if (logDebug && logVerbose)   // [LOG-1] verbose
            {
                Debug.Log(
                    $"{DebugTag} [BPM] RenderSinglePart part={partIndex} '{part.Name}' " +
                    $"using cached override BPM={effectiveOverride.Value}");
            }

            // ───────────────────────────────────────────────────────────
            // [SOLO-1 / D-R2-6=B; guard rewritten ORDER-1 / R2d 2026-07-31]
            // Seed a host default progression whenever the part has ANY
            // harmony consumer and a palette is assigned.
            //
            // The old `!hasBacking` skip is GONE. Since MGP-ALWTTT-BASS-ORDER-1
            // the package sniffs whether the Backing row actually CARRIES a
            // harmony source (per-render override, card progressionOverride,
            // palette with a valid weighted entry, or an authored Pattern) and
            // only then discards the default. An articulation-only Backing card
            // (future bossa / ska / power-chord cards) therefore no longer
            // suppresses the default — the Backing composer consumes it, and as
            // a bonus meter-normalizes and re-qualifies it, which the raw SOLO-1
            // path did not do. Backing is consequently a CONSUMER here, not a
            // disqualifier: a part holding only an articulation-only Backing row
            // must still be seeded.
            //
            // We no longer replicate the package's sniff client-side. It just
            // changed once; duplicating it guarantees drift.
            // Deterministic pick per (songSeed, partIndex).
            // ───────────────────────────────────────────────────────────
            ChordProgressionData defaultProgression = null;
            string defaultProgressionToken = null;
            {
                bool hasHarmonyConsumer = part.Tracks != null &&
                    part.Tracks.Any(t => t.Role == TrackRole.Backing
                                      || t.Role == TrackRole.Bassline
                                      || t.Role == TrackRole.Melody
                                      || t.Role == TrackRole.Harmony);

                if (hasHarmonyConsumer && defaultProgressionPalette != null)
                {
                    var dpRng = new System.Random(unchecked(
                        ((seedOverride ?? 0) * 486187739) ^ (partIndex * 1000003)));
                    defaultProgression =
                        defaultProgressionPalette.PickRandomProgression(dpRng); // clone=true: el asset nunca se muta

                    if (defaultProgression != null)
                    {
                        defaultProgressionToken =
                            $"dp:{defaultProgressionPalette.name}:{seedOverride ?? 0}:{partIndex}";
                        if (logDebug)
                            Debug.Log($"{DebugTag} <color=#88ff88>[SOLO-1]</color> part={partIndex} " +
                                $"'{part.Name}' offering default progression from palette " +
                                $"'{defaultProgressionPalette.name}' ({defaultProgressionToken}). " +
                                $"Whether it WINS the shared channel is decided package-side — " +
                                $"read [ORDER-1] harmony source below.");
                    }
                }
            }

            // ───────────────────────────────────────────────────────────
            // [B1 / D-E=α'] Compute cache keys from caller-supplied
            // track-inputs hash map. The map is computed by
            // SongConfigBuilder.ComputeTrackInputsHashesForPart from
            // UI-stable TrackEntry fields, so it survives the random
            // instrument resolution that happens inside FromUI.
            //
            // If trackInputsHashByMusician is null (e.g. legacy caller),
            // the cache is disabled for this call: nothing is read from
            // or written to _stemCache / _partBundleCache, but the render
            // still produces correct output.
            // ───────────────────────────────────────────────────────────
            string partMeterHash = ComputePartMeterHash(part);

            // ───────────────────────────────────────────────────────────
            // [D-R2-10=A / R2d 2026-07-31] SHARED-HARMONY IDENTITY TOKEN.
            //
            // Everything derived from partMeterHash — stem keys, partBundleKey,
            // the per-part invalidation sweep — must move when the harmony in
            // effect moves, because that harmony is baked into every consumer's
            // bytes.
            //
            // Why NOT `sharedProgressionSource` (the package's suggestion):
            // it is READBACK. It exists after the render. This hash decides
            // whether a render happens at all. Circular; not implementable as a
            // key. The readback is used instead as a VERIFICATION surface
            // (published below, asserted by ST-R2d-1/2).
            //
            // Two pre-render segments, both already in hand:
            //
            //  dp: — the palette/seed/part that MAY win the shared channel.
            //        Constant within a song (seed and partIndex are fixed,
            //        D-S5gb-2=B), so it never churns mid-song.
            //
            //  bk: — the Backing row's trackInputsHash. Exact proxy for "which
            //        Backing card is present", which is what decides whether
            //        the default is displaced and what the winning progression
            //        is. This segment fixes **F-HARM-STALE-1**, a latent defect
            //        since B1 and unrelated to SOLO-1: swapping the Backing card
            //        (Wormus Major → Minor) changes the BACKING track's hash but
            //        NOT the bass's, and previously not partMeterHash either —
            //        so the bass stem was served from cache with the OLD chords
            //        baked in. Silent wrong output, not silence.
            //
            // Accepted cost: over-invalidation. Swapping to an articulation-only
            // Backing card re-renders the harmony consumers even though the
            // effective harmony (the default) did not change. One extra render,
            // song-scoped. Preferred over serving stale bytes.
            //
            // BC: both segments absent ⇒ hash string byte-identical to pre-R2c
            // (no palette assigned AND no Backing row).
            //
            // Known limitation (recorded, not a defect): dp: identifies the
            // palette ASSET, seed and partIndex — not the palette's contents.
            // Editing its entries/weights mid-session does not invalidate.
            // Harmless: the caches are per-song (D7=B) and die with the song.
            // ───────────────────────────────────────────────────────────
            if (defaultProgressionToken != null)
                partMeterHash += "|" + defaultProgressionToken;

            if (trackInputsHashByTrack != null && part.Tracks != null)
            {
                var backingTrack = part.Tracks
                    .FirstOrDefault(t => t.Role == TrackRole.Backing);

                if (backingTrack != null && trackInputsHashByTrack.TryGetValue(
                        new MusicianTrackKey(backingTrack.MusicianId ?? "_", TrackRole.Backing),
                        out var backingHash))
                {
                    partMeterHash += "|bk:" + backingHash;
                }
            }

            var trackHashes = new Dictionary<MusicianTrackKey, string>();
            var stemKeysByTrack = new Dictionary<MusicianTrackKey, string>();
            var bundleEntries = new List<string>();
            bool cacheEnabled =
                trackInputsHashByTrack != null
                && part.Tracks != null
                && part.Tracks.Count > 0;

            if (cacheEnabled)
            {
                foreach (var tr in part.Tracks)
                {
                    if (tr == null || string.IsNullOrEmpty(tr.MusicianId))
                    {
                        cacheEnabled = false;
                        break;
                    }
                    // [DBG-C1] Composite keying: a musician holding two roles
                    // yields two independent cache identities. The BASS-1
                    // "multi-track ⇒ omit hash ⇒ cache disabled" carve-out is
                    // retired; the any-track-without-hash guard below remains
                    // as a general integrity gate only.
                    var trackKey = new MusicianTrackKey(tr.MusicianId, tr.Role);
                    if (!trackInputsHashByTrack.TryGetValue(trackKey, out var th)
                        || string.IsNullOrEmpty(th))
                    {
                        cacheEnabled = false;
                        break;
                    }
                    trackHashes[trackKey] = th;
                    stemKeysByTrack[trackKey] = BuildStemKey(trackKey, th, partMeterHash);
                    bundleEntries.Add($"{trackKey.MusicianId}:{trackKey.Role}#{th}");
                }
            }

            // [ALWTTT-MOD-DIR-3] Modulation transients are [NonSerialized]
            // one-shot state and are NOT part of partMeterHash / partBundleKey.
            // If the cache were consulted when transients are non-default, the
            // cache would either replay pre-modulation bytes (dropping the
            // directional intent — the SM-DIR-5 failure mode) or, once cached,
            // replay modulated bytes on a subsequent Auto render with the same
            // RootNote (wrong direction on the way back). Bypass the cache
            // entirely whenever a transient is staged. The composer consumes
            // and clears the transients in the same call, so the bypass is
            // itself one-shot — a subsequent Auto render with the same inputs
            // can cache and replay normally.
            if (part.ModulationOctaveHint !=
                    MidiGenPlay.Composition.ModulationOctaveHint.Auto
                || part.PreviousRootNote.HasValue)
            {
                cacheEnabled = false;
                if (logDebug)
                {
                    Debug.Log(
                        $"{DebugTag} <color=#ff8844>[Mod-DIR/CacheBypass]</color> " +
                        $"part={partIndex} '{part.Name}' " +
                        $"hint={part.ModulationOctaveHint} " +
                        $"prevRoot={(part.PreviousRootNote.HasValue ? part.PreviousRootNote.Value.ToString() : "null")} " +
                        $"→ cache bypassed for this render");
                }
            }

            // [DBG-C2 / D-C2-4=A] Per-render pattern overrides are
            // counterfactual, debug-only state that is deliberately NOT part
            // of any cache key. Mirror the Mod-DIR rule: when any override is
            // supplied, bypass the stem/bundle caches entirely (no read, no
            // write) so the override is always audible and cached identities
            // are never polluted. patternOverrides == null (production, or
            // dev with idle controls) leaves this path byte-for-byte
            // identical to post-C1.
            if (patternOverrides != null && patternOverrides.Count > 0)
            {
                cacheEnabled = false;
                if (logDebug)
                {
                    Debug.Log(
                        $"{DebugTag} <color=#ff8844>[DBG-C2/CacheBypass]</color> " +
                        $"part={partIndex} '{part.Name}' " +
                        $"overrides={patternOverrides.Count} " +
                        $"→ stem/bundle cache bypassed for this render");
                }
            }

            string partBundleKey = cacheEnabled
                ? BuildPartBundleKey(partMeterHash, bundleEntries)
                : null;

            // [B1] DIAG: dump full hash info for every render (so we can see WHY
            // the bundle hits or misses). Per-track + part-level shape.
            //
            // [LOG-1] VERBOSE. Six lines per render. Note carefully: this is
            // [B1][stemCache][DIAG] and is NOT the protected [B1][stemCache]
            // line that ST-A6 / ST-C1 read -- the [DIAG] suffix is the whole
            // difference, and a grep for "[B1][stemCache]" confuses the two.
            if (logDebug && logVerbose)
            {
                var partDiag =
                    $"part={partIndex} '{part.Name}' " +
                    $"TS={part.TimeSignature} Ton={part.Tonality} Root={part.RootNote} " +
                    $"Measures={part.Measures} TR={part.TempoRange} " +
                    $"ExplicitBpm={(part.ExplicitBpm.HasValue ? part.ExplicitBpm.Value.ToString() : "null")} " +
                    $"Scale={part.TempoScale:0.##}";
                var trackDiag = part.Tracks != null
                    ? string.Join(" || ", part.Tracks.Select(t =>
                    {
                        var styleName = t.Parameters?.Style != null
                            ? t.Parameters.Style.name : "_";
                        var instName = t.Instrument != null
                            ? t.Instrument.name : "_";
                        var percName = t.PercussionInstrument != null
                            ? t.PercussionInstrument.name : "_";
                        var th = cacheEnabled && trackHashes.TryGetValue(
                                new MusicianTrackKey(t.MusicianId ?? "_", t.Role), out var h)
                            ? h : "(no-hash)";
                        return $"mus={t.MusicianId} role={t.Role} " +
                               $"style={styleName} inst={instName} perc={percName} " +
                               $"trackHash={th}";
                    }))
                    : "(no tracks)";
                Debug.Log(
                    $"{DebugTag} <color=cyan>[B1][stemCache][DIAG]</color> {partDiag}\n" +
                    $"  cacheEnabled={cacheEnabled}\n" +
                    $"  partMeterHash='{partMeterHash}'\n" +
                    $"  partBundleKey='{(partBundleKey ?? "(none)")}'\n" +
                    $"  tracks: {trackDiag}\n" +
                    $"  stemCache size={_stemCache.Count} bundleCache size={_partBundleCache.Count}");
            }

            // [B1 / D-E=α'] Fast path: only attempted if cache is enabled for this call.
            if (cacheEnabled && _partBundleCache.TryGetValue(partBundleKey, out var bundleHit))
            {
                if (logDebug)
                {
                    Debug.Log(
                        $"{DebugTag} <color=cyan>[B1][stemCache]</color> bundle HIT " +
                        $"part={partIndex} key='{partBundleKey}' → fast-path replay " +
                        $"(seconds={bundleHit.seconds:0.##}, bpm={bundleHit.bpmChosen})");
                }
                // [DBG-C1] Replay publishes the ORIGINAL render's resolved
                // truth (D-DBG5=A): bytes are identical, choices are identical.
                LastAppliedCc7ByTrack = bundleHit.appliedCc7ByTrack; // [BAL-1]

                // [ORDER-1 / R2d] Republish the ORIGINAL render's harmony verdict.
                LastSharedProgressionSource = bundleHit.sharedProgressionSource;
                LastSharedProgressionAssetName = bundleHit.sharedProgressionAssetName;
                LastSharedProgressionData = bundleHit.sharedProgressionData;  // [JAM-1]
                if (logDebug)
                    Debug.Log($"{DebugTag} <color=#88ff88>[ORDER-1]</color> part={partIndex} " +
                        $"harmony source={LastSharedProgressionSource} " +
                        $"asset='{LastSharedProgressionAssetName ?? "_"}' (bundle replay)");

#if ALWTTT_DEV
                LastRenderResolvedTimeSignature = bundleHit.resolvedTs;
                LastRenderResolvedTonality = bundleHit.resolvedTonality;
                LastRenderResolvedRootNote = bundleHit.resolvedRoot;
#endif

                PublishLastRender(partIndex, bundleHit.bpmChosen,
                    bundleHit.resolvedByTrack, bundleHit.pinned, fromCache: true);
                return (
                    bundleHit.mergedBytes,
                    new Dictionary<MusicianTrackKey, byte[]>(bundleHit.stemsByTrack),
                    bundleHit.seconds,
                    bundleHit.bpmChosen,
                    new Dictionary<MusicianTrackKey, MIDIInstrumentSO>(bundleHit.pinned));
            }

            // [LOG-1] The F-4 per-render ENTRY dump was removed at F-4 closure
            // (it fired on every single render). These three counters survive
            // it deliberately: the catch-path dump below still reports them,
            // and that one fires ONLY when the package throws, so it costs
            // nothing in console volume and is the repro spec for that crash.
            // The D2-A try-catch itself is production-quality and permanent.
            int channelRolesCount = fullCfg.ChannelRoles?.Count ?? -1;
            int channelOwnersCount = fullCfg.ChannelMusicianOrder?.Count ?? -1;
            int tracksAtPart = part.Tracks?.Count ?? -1;

            // F-4 D2-A defense: try-catch around the package-internal orchestrator
            // call and its serialization. On catch, dump the failing args + stack
            // trace, INVALIDATE STEM CACHE FOR THIS PART [B1], and return the
            // failure tuple — the caller (PlaySinglePartLoop) already routes a
            // (null, null, 0f, ...) result through the existing graceful-fail
            // branch (merged==null || seconds<=0f). PRODUCTION-QUALITY: the
            // try-catch is permanent. Only the [F-4]-tagged log lines are
            // stripped at F-4 closure.
            byte[] mergedBytes;
            Dictionary<MusicianTrackKey, byte[]> stemsOut;
            float seconds;
            int bpmChosen;
            Dictionary<MusicianTrackKey, MIDIInstrumentSO> pinned;
            Dictionary<MusicianTrackKey, ResolvedTrackChoice> resolvedSnapshot;
            Dictionary<MusicianTrackKey, int> appliedCc7 = null; // [BAL-1]
            int b1Hits = 0, b1Misses = 0;
            try
            {
                // [DBG-C1] Consumer is composite-keyed end-to-end; the caller's
                // override map passes straight through. The old id→key
                // expansion shim (TODO(BASS-1)) is retired.
                var render = generator.Orchestrator.GenerateSinglePart(
                    part,
                    fullCfg.ChannelRoles,
                    partIndex,
                    effectiveOverride,
                    instrumentOverrides,
                    seedOverride: seedOverride,          // [S5g / MGP-ALWTTT-SEED-1]
                    patternOverrides: patternOverrides,  // [DBG-C1 / D-C1-1] inert this batch
                    mixGains: _gigMixGains,              // [BAL-1 / MGP-MIX-1] null ⇒ byte-identical
                                                         // [SOLO-1 / D-R2-6=B] Host channel into the shared
                                                         // progression cache for backing-less parts. null ⇒
                                                         // byte-identical. Package-side guard (D-SOLO-GUARD=A)
                                                         // warns+ignores if the part HAS backing; we already
                                                         // skip that case client-side.
                    defaultProgression: defaultProgression);

                // [BAL-1 task 4] readback of the CC7 actually emitted.
                appliedCc7 = render.appliedCc7ByTrack;

                // [ORDER-1 / R2d] Which source actually won the shared progression
                // channel. This is the answer to "did our default get used?" — the
                // question the old `!hasBacking` proxy answered wrongly once an
                // articulation-only Backing card could coexist with a live default.
                LastSharedProgressionSource = render.sharedProgressionSource;
                LastSharedProgressionAssetName = render.sharedProgressionAssetName;
                LastSharedProgressionData = render.sharedProgressionData;  // [JAM-1] may be null
                if (logDebug)
                    Debug.Log($"{DebugTag} <color=#88ff88>[ORDER-1]</color> part={partIndex} " +
                        $"harmony source={LastSharedProgressionSource} " +
                        $"asset='{LastSharedProgressionAssetName ?? "_"}' (fresh render)");

                if (logDebug)
                    Debug.Log($"{DebugTag} [BPM] Part={partIndex} resolved BPM={render.bpm}");

                // Serialize merged
                using (var ms = new MemoryStream())
                {
                    render.merged.Write(ms);
                    mergedBytes = ms.ToArray();
                }

                // [DBG-C1] Stems stay composite-keyed verbatim — the collision
                // the package re-key removed cannot re-enter here.
                stemsOut = new Dictionary<MusicianTrackKey, byte[]>();
                foreach (var kv in render.stemsByMusician)
                {
                    using var ms = new MemoryStream();
                    kv.Value.Write(ms);
                    stemsOut[kv.Key] = ms.ToArray();
                }

                seconds = ComputeDurationSeconds(render.merged);
                bpmChosen = render.bpm;
                pinned = render.melInstByMusician != null
                    ? new Dictionary<MusicianTrackKey, MIDIInstrumentSO>(render.melInstByMusician)
                    : new Dictionary<MusicianTrackKey, MIDIInstrumentSO>();
                // [DBG-C1] Snapshot the package readback for the dev truth
                // surface + the bundle-cache entry.
                resolvedSnapshot = render.resolvedByTrack != null
                    ? new Dictionary<MusicianTrackKey, ResolvedTrackChoice>(render.resolvedByTrack)
                    : new Dictionary<MusicianTrackKey, ResolvedTrackChoice>();

                // ───────────────────────────────────────────────────────
                // [B1 / D-E=α'] Per-track persistence — only when cache
                // is enabled. For each musician with a hash, swap fresh
                // stem with cached one if hash matches; otherwise store
                // fresh stem.
                // ───────────────────────────────────────────────────────
                if (cacheEnabled)
                {
                    var stemKeysSnapshot = stemsOut.Keys.ToList();
                    foreach (var trackKey in stemKeysSnapshot)
                    {
                        if (!stemKeysByTrack.TryGetValue(trackKey, out var stemKey))
                            continue;
                        if (_stemCache.TryGetValue(stemKey, out var cachedStem))
                        {
                            stemsOut[trackKey] = cachedStem;
                            b1Hits++;
                        }
                        else
                        {
                            _stemCache[stemKey] = stemsOut[trackKey];
                            b1Misses++;
                        }
                    }
                }

                // ───────────────────────────────────────────────────────
                // [B1] If we swapped any cached stems in, the merged bytes
                // from the orchestrator no longer match the stem set. Rebuild
                // merged from stemsOut. If no swaps happened (all misses or
                // empty hash map), keep orchestrator's merged as-is.
                // ───────────────────────────────────────────────────────
                if (b1Hits > 0)
                {
                    var ordered = new List<byte[]>();
                    var seen = new HashSet<MusicianTrackKey>();
                    // [DBG-C1] Channel order pairs ChannelMusicianOrder[i] with
                    // ChannelRoles[i]; a musician's two role-stems keep their
                    // own channel positions instead of collapsing.
                    if (fullCfg.ChannelMusicianOrder != null && fullCfg.ChannelRoles != null)
                    {
                        int n = Math.Min(fullCfg.ChannelMusicianOrder.Count,
                                         fullCfg.ChannelRoles.Count);
                        for (int i = 0; i < n; i++)
                        {
                            var musId = fullCfg.ChannelMusicianOrder[i];
                            if (string.IsNullOrEmpty(musId)) continue;
                            var key = new MusicianTrackKey(musId, fullCfg.ChannelRoles[i]);
                            if (stemsOut.TryGetValue(key, out var b) && seen.Add(key))
                                ordered.Add(b);
                        }
                    }
                    foreach (var kv in stemsOut)
                    {
                        if (seen.Add(kv.Key)) ordered.Add(kv.Value);
                    }

                    var rebuiltMerged = MergeStems(ordered);
                    if (rebuiltMerged != null && rebuiltMerged.Length > 0)
                    {
                        mergedBytes = rebuiltMerged;
                        // Recompute duration from rebuilt midi.
                        using var msReread = new MemoryStream(mergedBytes);
                        var rereadMidi = MidiFile.Read(msReread);
                        seconds = ComputeDurationSeconds(rereadMidi);
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"{DebugTag} <color=cyan>[B1][stemCache]</color> " +
                            $"MergeStems returned null/empty for part={partIndex}; " +
                            $"falling back to orchestrator merged (cached stems ignored).");
                        // Restore orchestrator stems for ALL tracks (no verbatim
                        // persistence this turn) so stems and merged stay consistent.
                        stemsOut = new Dictionary<MusicianTrackKey, byte[]>();
                        foreach (var kv in render.stemsByMusician)
                        {
                            using var ms = new MemoryStream();
                            kv.Value.Write(ms);
                            stemsOut[kv.Key] = ms.ToArray();
                        }
                    }
                }

                if (logDebug)
                {
                    Debug.Log(
                        $"{DebugTag} <color=cyan>[B1][stemCache]</color> " +
                        $"part={partIndex} cacheEnabled={cacheEnabled} " +
                        $"stem hits={b1Hits} misses={b1Misses} " +
                        $"rebuilt_merged={(b1Hits > 0)} " +
                        $"stemCache_size_after={_stemCache.Count} " +
                        $"bundleCache_size_after={_partBundleCache.Count}");
                }
            }
            catch (Exception ex)
            {
                // Diagnostic dump on catch — full per-track detail so the
                // package owner has a clean repro spec if root cause is
                // package-internal. [LOG-1] KEPT (correcting the LOG-1 spec,
                // which had listed it for deletion): this is an error path, not
                // a per-render path.  Retagged off [F-4] since that batch is
                // closed.
                var perTrack = part.Tracks != null
                    ? string.Join(", ", part.Tracks.Select((t, i) =>
                        $"[{i}: ch={t.Channel} role={t.Role} mus={(string.IsNullOrEmpty(t.MusicianId) ? "-" : t.MusicianId)}]"))
                    : "(null)";
                var rolesDump = fullCfg.ChannelRoles != null
                    ? string.Join(",", fullCfg.ChannelRoles)
                    : "(null)";
                var ownersDump = fullCfg.ChannelMusicianOrder != null
                    ? string.Join(",", fullCfg.ChannelMusicianOrder.Select(s => string.IsNullOrEmpty(s) ? "-" : s))
                    : "(null)";
                Debug.LogError(
                    $"{DebugTag} SongOrchestrator.GenerateSinglePart caught " +
                    $"{ex.GetType().Name}: {ex.Message}\n" +
                    $"  partIndex={partIndex} parts={fullCfg.Parts.Count}\n" +
                    $"  channelRoles[{channelRolesCount}]={rolesDump}\n" +
                    $"  channelOwners[{channelOwnersCount}]={ownersDump}\n" +
                    $"  channelMap[{channelMap.Count}]\n" +
                    $"  tracksAtPart[{tracksAtPart}]={perTrack}\n" +
                    $"  bpm={effectiveOverride?.ToString() ?? "null"} " +
                    $"instOverrides={instrumentOverrides?.Count ?? 0}\n" +
                    $"{ex.StackTrace}");

                // [B1] D2 locked spec: on F-4 Stage A catch, all stems for the
                // affected part invalidate. Other parts untouched.
                InvalidateStemCacheForPart(partMeterHash);

                return (null, null, 0f, 0, null);
            }

            // [B1 / D-E=α'] Cache the bundle only if cache is enabled.
            if (cacheEnabled)
            {
                _partBundleCache[partBundleKey] = new PartBundleCacheEntry
                {
                    mergedBytes = mergedBytes,
                    stemsByTrack = new Dictionary<MusicianTrackKey, byte[]>(stemsOut),
                    seconds = seconds,
                    bpmChosen = bpmChosen,
                    pinned = new Dictionary<MusicianTrackKey, MIDIInstrumentSO>(pinned),
                    resolvedByTrack =
                        new Dictionary<MusicianTrackKey, ResolvedTrackChoice>(resolvedSnapshot),
                    appliedCc7ByTrack = appliedCc7 != null   // [BAL-1] existing
                        ? new Dictionary<MusicianTrackKey, int>(appliedCc7) : null,
                    sharedProgressionSource = LastSharedProgressionSource,   // [ORDER-1 / R2d]
                    sharedProgressionAssetName = LastSharedProgressionAssetName,
                    sharedProgressionData = LastSharedProgressionData,       // [JAM-1]
#if ALWTTT_DEV
                    resolvedTs = part.TimeSignature,
                    resolvedTonality = part.Tonality,   // post step-2b alignment
                    resolvedRoot = part.RootNote,
#endif
                };
            }

            LastAppliedCc7ByTrack = appliedCc7; // [BAL-1] null when ungained

#if ALWTTT_DEV
            LastRenderResolvedTimeSignature = part.TimeSignature;
            LastRenderResolvedTonality = part.Tonality;
            LastRenderResolvedRootNote = part.RootNote;
#endif

            // [DBG-C1] Publish the fresh render's truth for the dev tab.
            PublishLastRender(partIndex, bpmChosen, resolvedSnapshot, pinned, fromCache: false);

            return (mergedBytes, stemsOut, seconds, bpmChosen, pinned);
        }

        #endregion

        #region Private Methods

        private IEnumerator RunBeatGrid(string key, float duration)
        {
            if (!cache.TryGetValue(key, out var entry)) yield break;

            using var ms = new MemoryStream(entry.data);
            var midi = MidiFile.Read(ms);
            var tempoMap = midi.GetTempoMap();

            // Collect change times
            var tsChanges = tempoMap.GetTimeSignatureChanges().ToList(); // ValueChange<TimeSignature>
            if (logDebug)
            {
                Debug.Log($"{DebugTag} TS changes: " +
                          string.Join(", ", tsChanges.Select(c =>
                            $"{c.Value.Numerator}/{c.Value.Denominator}@{c.Time}")));
            }

            var tempoChanges = tempoMap.GetTempoChanges().ToList();         // ValueChange<Tempo>

            var boundaries = new SortedSet<long> { 0 };
            foreach (var c in tsChanges) boundaries.Add(c.Time);
            foreach (var c in tempoChanges) boundaries.Add(c.Time);

            // last tick in the song
            long lastTick = 0;
            foreach (var chunk in midi.GetTrackChunks())
            {
                var evts = chunk.GetTimedEvents();
                if (evts.Count > 0) lastTick = Math.Max(lastTick, evts.Last().Time);
            }
            boundaries.Add(lastTick);

            var pts = boundaries.ToList();
            var tsSet = new HashSet<long>(tsChanges.Select(c => c.Time)); // fast "is this a TS change?" check

            // state
            int barIndex = 0;
            int beatInBar = 0;
            double songTimeSec = 0.0;

            // previous values for change notifications (BPM/TS)
            double prevBpm = -1;
            int prevNum = -1, prevDen = -1;

            for (int i = 0; i < pts.Count - 1; i++)
            {
                long startTick = pts[i];
                long endTick = pts[i + 1];

                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(startTick));
                var ts = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(startTick));

                double bpm = 60000000.0 / tempo.MicrosecondsPerQuarterNote;
                int numerator = ts.Numerator;
                int denominator = ts.Denominator;

                // Notify BPM/TS changes
                if (!Mathf.Approximately((float)bpm, (float)prevBpm))
                {
                    foreach (var s in _tempoSigSubs) s?.OnTempoChanged(bpm);
                    prevBpm = bpm;
                }
                if (numerator != prevNum || denominator != prevDen)
                {
                    foreach (var s in _tempoSigSubs)
                        s?.OnTimeSignatureChanged(numerator, denominator);

                    prevNum = numerator; prevDen = denominator;
                }

                // If this segment starts at a TS change (or song start), align to a new bar
                bool tsStartsHere = (startTick == 0) || tsSet.Contains(startTick);
                if (tsStartsHere) beatInBar = 0;

                // segment duration in seconds and seconds-per-beat
                var segStart = TimeConverter.ConvertTo<MetricTimeSpan>(startTick, tempoMap).TotalSeconds;
                var segEnd = TimeConverter.ConvertTo<MetricTimeSpan>(endTick, tempoMap).TotalSeconds;
                double segSeconds = Math.Max(0, segEnd - segStart);

                double spq = 60.0 / bpm;
                double spb = spq * (4.0 / denominator);

                // Emit beats within this segment
                double tLocal = 0.0;
                while (tLocal + 1e-6 < segSeconds && player != null && player.IsPlaying)
                {
                    // Beat event at (songTimeSec + tLocal)
                    var ev = new BeatGridEvent
                    {
                        barIndex = barIndex,
                        beatInBar = beatInBar,
                        time = (float)(songTimeSec + tLocal)
                    };

                    foreach (var g in _gridSubs) g?.OnBeat(ev);
                    if (beatInBar == 0)
                        foreach (var g in _gridSubs) g?.OnDownbeat(ev);

                    // wait one beat, then advance counters
                    yield return new WaitForSeconds((float)spb);
                    tLocal += spb;

                    beatInBar++;
                    if (beatInBar >= numerator)
                    {
                        beatInBar = 0;
                        barIndex++;
                    }
                }

                songTimeSec += segSeconds;
                if (songTimeSec >= duration) break;
            }
        }

        private SongCacheEntry GenerateSongEntry(SongData song)
            => GenerateSongEntry(song, GameManager.PersistentGameplayData.MusicianList);

        private SongCacheEntry GenerateSongEntry(
            SongData song, IList<Characters.Band.MusicianBase> band)
        {
            EnsureRegistriesLoaded();

            // Convert to the concrete type GenerateConfig expects
            var bandList = band as List<Characters.Band.MusicianBase>
                           ?? band?.ToList();

            var seedBase = $"{song.Id}::{ComputeBandSignature(bandList)}";
            _seedKeyForThisGeneration = seedBase;

            // Obtain config and key
            var config = song.GenerateConfig(bandList);

            var key = CacheKey(song, bandList);

            // Channel mapping
            var channelMap = BuildChannelMap(config.ChannelRoles);

            // Build (musicianId, role) -> channel lookup [BASS-1 / R13]
            BuildChannelOwnerLookups(config, channelMap,
                out var byMusicianRole, out var byMusician);

            // Stamp channels into tracks for debugging clarity
            foreach (var part in config.Parts)
                foreach (var tr in part.Tracks)
                    StampChannel(tr, byMusicianRole, byMusician);

            // Owners map
            int maxCh = channelMap.Count > 0 ? channelMap.Max() : 0;
            var perChannel =
                Enumerable.Repeat(string.Empty, Math.Max(16, maxCh + 1)).ToList();
            for (int i = 0; i < config.ChannelMusicianOrder.Count && i < channelMap.Count; i++)
            {
                int ch = channelMap[i];
                if (ch >= 0 && ch < perChannel.Count)
                    perChannel[ch] = config.ChannelMusicianOrder[i];
            }
            channelOwnersByKey[key] = perChannel;

            // Roles mapping
            channelRolesByKey[key] = config.ChannelRoles?.ToList() ?? new List<TrackRole>();

            DumpConfigTrace(song, config, channelMap);

            // Midi Generation
            var midi = generator.GenerateSong(config);

            // Ensure Part 1 Time Signature
            var part1Ts = config.Parts[0].TimeSignature;
            int tsNum = MusicTheory.TimeSignatureProperties[part1Ts].BeatsPerMeasure;
            int tsDen = MusicTheory.TimeSignatureProperties[part1Ts].BeatUnit;
            EnsureTimeSignatureAtZero(midi, tsNum, tsDen);

            // Musician channel owners
            var owners = new Dictionary<int, string>();
            int ti = 0;
            foreach (var chunk in midi.GetTrackChunks())
            {
                var tag = chunk.Events.OfType<TextEvent>().FirstOrDefault(
                    te => te.Text != null && te.Text.StartsWith("mus:"));

                if (tag != null) owners[ti] = tag.Text.Substring(4);

                ti++;
            }
            trackOwnersByKey[key] = owners;

            // Markers
            _tempoMapForCurrentSong = midi.GetTempoMap();
            BuildPartMarkers(midi);
            BuildChordMarkers(midi);

            byte[] data;
            using (var ms = new MemoryStream())
            {
                midi.Write(ms);
                data = ms.ToArray();
            }

            var seconds = ComputeDurationSeconds(midi);
            if (logDebug)
            {
                var used = string.Join(",", GetUsedChannels(midi));
                Debug.Log($"{DebugTag} Generated '{song.SongTitle}' " +
                    $"tracks:{midi.GetTrackChunks().Count()} ch:[{used}] dur:{seconds:0.00}s");
            }

            _seedKeyForThisGeneration = null;

            return new SongCacheEntry { data = data, seconds = seconds };
        }

        private float ComputeDurationSeconds(MidiFile midi)
        {
            var tempoMap = midi.GetTempoMap();

            long last = 0;
            foreach (var chunk in midi.GetTrackChunks())
            {
                var events = chunk.GetTimedEvents();
                if (events.Count == 0) continue;
                long end = events.Last().Time;
                if (end > last) last = end;
            }

            var metric = TimeConverter.ConvertTo<MetricTimeSpan>(last, tempoMap);
            return (float)metric.TotalSeconds;
        }

        private string ComputeBandSignature(IList<Characters.Band.MusicianBase> band)
        {
            if (band == null) return "";
            var ids = band
                .Select(m => m?.MusicianCharacterData?.CharacterId)
                .Where(id => !string.IsNullOrEmpty(id))
                .OrderBy(id => id);
            return string.Join("+", ids);
        }

        private string CacheKey(SongData song, IList<Characters.Band.MusicianBase> band = null)
        {
            var b = band ?? GameManager.PersistentGameplayData.MusicianList;
            var sig = ComputeBandSignature(b);
            var baseKey = string.IsNullOrEmpty(sig) ? song.Id : $"{song.Id}::{sig}";
            return $"{baseKey}::{CacheEpoch}";
        }

        public void ClearCache() { cache.Clear(); }

        private List<int> GetUsedChannels(MidiFile midi)
        {
            var set = new HashSet<int>();
            foreach (var chunk in midi.GetTrackChunks())
            {
                foreach (var ev in chunk.Events)
                {
                    if (ev is Melanchall.DryWetMidi.Core.ChannelEvent ce)
                        set.Add((int)ce.Channel);
                }
            }
            var list = set.ToList();
            list.Sort();
            return list;
        }

        // Build a new midi that only keeps events on the allowed channels.
        private byte[] BuildChannelMaskedData(byte[] fullData, HashSet<int> allowed)
        {
            using var msIn = new MemoryStream(fullData);
            var midi = MidiFile.Read(msIn);

            foreach (var chunk in midi.GetTrackChunks())
            {
                var toRemove = new List<MidiEvent>();
                foreach (var ev in chunk.Events)
                {
                    if (ev is ChannelEvent ce)
                    {
                        int ch = (int)ce.Channel;

                        // never remove the metronome channel
                        if (ch == MidiGenerator.MetronomeChannel) continue;

                        if (!allowed.Contains(ch)) toRemove.Add(ev);
                    }
                }
                foreach (var ev in toRemove) chunk.Events.Remove(ev);
            }

            using var msOut = new MemoryStream();
            midi.Write(msOut);
            return msOut.ToArray();
        }

        private (string key, SongCacheEntry entry)
            EnsureInCache(SongData song, IList<Characters.Band.MusicianBase> band)
        {
            EnsureRegistriesLoaded();

            var b = band ?? GameManager.PersistentGameplayData.MusicianList;
            var key = CacheKey(song, b);

            if (_bypassCacheNext)
            {
                var fresh = GenerateSongEntry(song, b);
                _bypassCacheNext = false;
                return (key, fresh);
            }

            if (!cache.TryGetValue(key, out var entry))
            {
                entry = GenerateSongEntry(song, b);
                if (entry == null) return (null, null);
                cache[key] = entry;
                if (logDebug) Debug.Log($"{DebugTag} " +
                    $"Cached '{song.SongTitle}' key:{key} " +
                    $"bytes:{entry.data?.Length} dur:{entry.seconds:0.00}s");
            }
            return (key, entry);
        }
        #endregion

        #region Midi Handling
        void HandleMidiEvents(List<MPTKEvent> evts)
        {
            if (evts == null || evts.Count == 0) return;

            // Prefer per-track ownership (supports multiple drummers on the same channel)
            Dictionary<int, string> ownersByTrack = null;
            if (!string.IsNullOrEmpty(_currentKey))
                trackOwnersByKey.TryGetValue(_currentKey, out ownersByTrack);

            // Group NoteOn-by-velocity>0 per channel at this tick
            var byCh = evts.Where(e => e.Command == MPTKCommand.NoteOn && e.Velocity > 0)
                           .GroupBy(e => e.Channel);

            foreach (var grp in byCh)
            {
                int ch = grp.Key;
                var notes = grp.ToList();

                // ── Chord: omit for drum channel (GM ch10 -> index 9)
                if (ch != drumChannel && notes.Count > 1)
                {
                    var n0 = notes[0];
                    string musId = null;

                    if (ownersByTrack != null && ownersByTrack.TryGetValue(
                        (int)n0.Track, out var idByTrack))
                        musId = idByTrack;
                    else if (ch >= 0 && ch < _channelOwners.Count)
                        musId = _channelOwners[ch];

                    _musicianAnchors.TryGetValue(musId ?? "", out var anchor);

                    // try to label this chord from our index
                    string labelSym = null, labelRoman = null;
                    int degreeIndex = 0;
                    ChordQuality? qual = null;

                    // --- Primary: per-channel timeline (robust across repeats) ---
                    if (_chordTimelineByChannel.TryGetValue(ch, out var timeline))
                    {
                        int cur = _chordTimelineCursor.TryGetValue(ch, out var c) ? c : 0;
                        int tol = Mathf.Max(0, settings != null ?
                            settings.chordLabelTickTolerance : 2);
                        long tickNow = n0.Tick;

                        // advance cursor while next marker is at/before current tick + tolerance
                        while (cur < timeline.Count && timeline[cur].tick <= tickNow + tol)
                        {
                            _currentChordByChannel[ch] = timeline[cur].Item2;
                            cur++;
                        }
                        _chordTimelineCursor[ch] = cur;

                        if (_currentChordByChannel.TryGetValue(ch, out var curLabel))
                        {
                            labelSym = curLabel.sym;
                            labelRoman = curLabel.roman;
                            degreeIndex = curLabel.deg;
                            qual = curLabel.quality;
                        }
                    }

                    if (labelSym == null && settings != null && settings.logMidiMusicManager)
                    {
                        int tol = Mathf.Max(0, settings.chordLabelTickTolerance);
                        long tickNow = n0.Tick;
                        string near = "(no timeline for channel)";
                        if (_chordTimelineByChannel.TryGetValue(ch, out var tl) && tl.Count > 0)
                        {
                            int idx = tl.BinarySearch((tickNow, default(ChordLabel)),
                                Comparer<(long, ChordLabel)>.Create((a, b) => a.Item1.CompareTo(b.Item1)));
                            if (idx < 0) idx = ~idx;
                            var prev = idx > 0 ? tl[idx - 1].Item1 : -1;
                            var next = idx < tl.Count ? tl[idx].Item1 : -1;
                            near = $"prev={FormatTick(prev)} next={FormatTick(next)}";
                        }

                        Debug.LogWarning($"[MidiMusicManager] LABEL MISS ch={ch} track={(int)n0.Track} tickNow={FormatTick(n0.Tick)} tol=±{tol} " +
                                         $"timeline={(_chordTimelineByChannel.TryGetValue(ch, out var tl2) ? tl2.Count : 0)} | {near}");
                    }

                    // --- Optional fallback: legacy per-track map (unchanged) ---
                    if (labelSym == null &&
                        _chordLabelsByTrack.TryGetValue((int)n0.Track, out var perTickTrk))
                    {
                        long tick = n0.Tick;
                        if (!perTickTrk.TryGetValue(tick, out var lab))
                        {
                            int tol = Mathf.Max(0, settings != null ?
                                settings.chordLabelTickTolerance : 2);
                            for (long d = -tol; d <= tol; d++)
                                if (perTickTrk.TryGetValue(tick + d, out lab)) break;
                        }

                        if (lab.sym != null)
                        {
                            labelSym = lab.sym; labelRoman = lab.roman;
                            degreeIndex = lab.deg; qual = lab.quality;
                        }
                    }

                    var chord = new ChordEvent
                    {
                        musicianId = musId,
                        channel = ch,
                        notes = notes.Select(e => e.Value).ToList(),
                        time = n0.RealTime / 1000f,
                        anchor = anchor,
                        // [LOG-1 / D-LOG-1=B] A damaged symbol is dropped
                        // rather than shown; FloatingTextMidiListener.OnChord
                        // already has a roman-only branch, so the label
                        // degrades to "(Imaj7)" instead of lying with
                        // "C?maj7". The raw value survives in the label and in
                        // the "Timeline ch=" line, which is where ST-LOG-2
                        // reads it.
                        symbol = LooksDamaged(labelSym) ? null : labelSym,
                        roman = labelRoman,
                        degreeIndex = degreeIndex,
                        quality = qual
                    };
                    foreach (var s in _chordSubs) s?.OnChord(chord);
                }

                // ── Notes & Beats: resolve per note using TRACK (fallback to CHANNEL)
                foreach (var n in notes)
                {
                    string musId = null;

                    if (ownersByTrack != null && ownersByTrack.TryGetValue(
                        (int)n.Track, out var idByTrack))
                        musId = idByTrack;
                    else if (ch >= 0 && ch < _channelOwners.Count)
                        musId = _channelOwners[ch];

                    _musicianAnchors.TryGetValue(musId ?? "", out var anchor);

                    var tagged = new MidiTaggedEvent
                    {
                        musicianId = musId,
                        channel = ch,
                        note = n.Value,
                        velocity = n.Velocity,
                        time = n.RealTime / 1000f,
                        anchor = anchor
                    };
                    foreach (var s in _noteSubs) s?.OnMidiNote(tagged);

                    if (ch == drumChannel && kickNotes.Contains(n.Value))
                    {
                        var beat = new BeatEvent
                        {
                            beatIndex = _beatIndex++,
                            time = tagged.time,
                            sourceMusicianId = musId,
                            anchor = anchor
                        };
                        foreach (var k in _kickSubs) k?.OnDrumKick(tagged); // new specific
                    }
                }
            }
        }

        private static List<int> BuildChannelMap(List<TrackRole> roles)
        {
            var map = Enumerable.Repeat(-1, roles?.Count ?? 0).ToList();
            var used = new HashSet<int>();

            for (int i = 0; i < map.Count; i++)
                if (roles[i] == TrackRole.Rhythm) { map[i] = 9; used.Add(9); }

            int Next()
            {
                for (int ch = 0; ch < 16; ch++)
                    if (ch != 9 && !used.Contains(ch)) { used.Add(ch); return ch; }
                return 0;
            }
            for (int i = 0; i < map.Count; i++) if (map[i] == -1) map[i] = Next();
            return map;
        }

        // [BAL-1] (musicianId, role) gains → per-channel array. Channel 9 is
        // never gained (shared Rhythm channel, D-BAL-5=A). No map ⇒ all 1.
        private float[] BuildChannelGains(SongConfig fullCfg, List<int> channelMap)
        {
            var g = new float[16];
            for (int i = 0; i < 16; i++) g[i] = 1f;
            var roles = fullCfg.ChannelRoles;
            var owners = fullCfg.ChannelMusicianOrder;
            if (_gigMixGains == null || roles == null || owners == null) return g;

            int n = Mathf.Min(roles.Count, Mathf.Min(owners.Count, channelMap.Count));
            for (int i = 0; i < n; i++)
            {
                int ch = channelMap[i];
                if (ch == 9) continue;
                if (_gigMixGains.TryGetValue(
                        new MusicianTrackKey(owners[i], roles[i]), out var gain))
                    g[ch] = Mathf.Clamp(gain, 0f, 1.27f);
            }
            return g;
        }

        private float PlayBytes(string key, byte[] data, float seconds, string label)
        {
            if (player == null) { Debug.LogError($"{DebugTag} No IPlayMidi."); return 0f; }

            player.Stop();            // will trigger OnSongEnded → ClearMarkers()
            _currentKey = key;

            // [BAL-1] Adopt the channel gains of exactly these bytes. Non-gig
            // paths (Play(SongData)/jam — no preceding RenderSinglePart) reset
            // to identity so the live plane composes against gain 1.0.
            var _bal1Src = _pendingChannelGains;
            for (int _bal1Ch = 0; _bal1Ch < 16; _bal1Ch++)
                _bakedGain01ByChannel[_bal1Ch] = _bal1Src != null ? _bal1Src[_bal1Ch] : 1f;
            _pendingChannelGains = null;

            // Rebuild markers/timelines from the exact bytes we are going to play
            RebuildMarkersFromData(data);

            // dump the exact payload we’ll play (if enabled)
            DevDumpMidi(key, data, label);

            player.Play(data);

            if (logDebug)
                Debug.Log($"{DebugTag} Play {label} key:{key} " +
                          $"bytes:{data?.Length} dur:{seconds:0.00}s " +
                          $"IsPlaying:{player.IsPlaying}");

            return seconds;
        }

        private void NotifyTempoSignatureAtStart(string key)
        {
            if (!cache.TryGetValue(key, out var entry)) return;

            using var ms = new MemoryStream(entry.data);
            var midi = MidiFile.Read(ms);
            var tempoMap = midi.GetTempoMap();

            var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(0));
            var ts = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(0));

            double bpm = 60000000.0 / tempo.MicrosecondsPerQuarterNote;
            int numerator = ts.Numerator;
            int denominator = ts.Denominator;

            foreach (var s in _tempoSigSubs)
                s?.OnTempoChanged(bpm);

            foreach (var s in _tempoSigSubs)
                s?.OnTimeSignatureChanged(numerator, denominator);
        }

        private static void EnsureTimeSignatureAtZero(MidiFile midi, int numerator, int denominator)
        {
            var track0 = midi.GetTrackChunks().FirstOrDefault();
            if (track0 == null)
            {
                track0 = new TrackChunk();
                midi.Chunks.Add(track0);
            }

            // Work in absolute time; don't touch existing TS changes.
            using var mgr = track0.ManageTimedEvents();
            bool hasAtZero = mgr.Objects.OfType<TimedEvent>()
                .Any(te => te.Event is TimeSignatureEvent && te.Time == 0);

            if (!hasAtZero)
            {
                mgr.Objects.Add(new TimedEvent(
                    new TimeSignatureEvent((byte)numerator, (byte)denominator, 24, 8), 0));
            }
        }

        private byte[] ApplyMetronomeVolumeToBytes(byte[] data, bool enable)
        {
            using var ms = new MemoryStream(data);
            var midi = MidiFile.Read(ms);
            MidiGenerator.ApplyChannelVolume(midi, MidiGenerator.MetronomeChannel, enable ? 110 : 0);
            using var ms2 = new MemoryStream();
            midi.Write(ms2);
            return ms2.ToArray();
        }

        private void BuildPartMarkers(MidiFile file)
        {
            _partMarkers.Clear();

            foreach (var chunk in file.GetTrackChunks())
            {
                foreach (var te in chunk.GetTimedEvents())
                {
                    if (te.Event is Melanchall.DryWetMidi.Core.TextEvent txt &&
                        TryParsePartTag(txt.Text, out var info))
                    {
                        _partMarkers.Add(new PartMarker { tick = te.Time, info = info, fired = false });
                    }
                }
            }
            _partMarkers.Sort((a, b) => a.tick.CompareTo(b.tick));
            // [LOG-1] verbose: a count, and "Timeline ch=" already reports
            // whether the timeline was built at all.
            if (settings != null && settings.logMidiMusicManager && logVerbose)
                Debug.Log($"[MidiMusicManager] Built part markers: {_partMarkers.Count}");
        }

        private string FormatTick(long tick)
        {
            if (_tempoMapForCurrentSong == null) return tick.ToString();
            var m = TimeConverter.ConvertTo<MetricTimeSpan>(tick, _tempoMapForCurrentSong);
            return $"{tick} ({m.Minutes:D2}:{m.Seconds:D2}.{m.Milliseconds:D3})";
        }

        private void RebuildMarkersFromData(byte[] data)
        {
            try
            {
                ClearMarkers(); // ensure clean state in case OnSongEnded isn’t synchronous yet
                using var ms = new MemoryStream(data);
                var midi = MidiFile.Read(ms);

                _tempoMapForCurrentSong = midi.GetTempoMap();
                BuildPartMarkers(midi);   // keeps _partMarkers in sync with this playback
                BuildChordMarkers(midi);  // fills _chordLabelsByChannel + timelines

                if (logDebug && logVerbose)   // [LOG-1] verbose
                {
                    int tl = _chordTimelineByChannel.Values.Sum(t => t?.Count ?? 0);
                    Debug.Log($"{DebugTag} Rebuilt markers from bytes. " +
                              $"parts={_partMarkers?.Count ?? 0}, chord-timeline-entries={tl}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{DebugTag} RebuildMarkersFromData failed: {ex.Message}");
            }
        }

        private void BuildChordMarkers(MidiFile file)
        {
            _chordLabelsByTrack.Clear();
            _chordLabelsByChannel.Clear();
            _chordTagDamageReported = false;   // [LOG-1]

            int trackIndex = 0;
            foreach (var chunk in file.GetTrackChunks())
            {
                int countHere = 0;

                foreach (var te in chunk.GetTimedEvents())
                {
                    if (te.Event is TextEvent txt)
                    {
                        if (TryParseChordTag(txt.Text, out var tagCh, out var label))
                        {
                            // Track map (fallback / debugging)
                            if (!_chordLabelsByTrack.TryGetValue(trackIndex, out var byTickTrack))
                                _chordLabelsByTrack[trackIndex] =
                                    byTickTrack = new Dictionary<long, ChordLabel>();
                            byTickTrack[te.Time] = label;

                            // Channel map (primary) — from tag
                            if (tagCh >= 0)
                            {
                                if (!_chordLabelsByChannel.TryGetValue(tagCh, out var byTickCh))
                                    _chordLabelsByChannel[tagCh] =
                                        byTickCh = new Dictionary<long, ChordLabel>();
                                byTickCh[te.Time] = label;
                            }

                            countHere++;
                        }
                        else if (txt.Text.StartsWith("chd:", StringComparison.OrdinalIgnoreCase) &&
                                 settings != null && settings.logMidiMusicManager)
                        {
                            Debug.LogWarning($"[MidiMusicManager] Found chd text but couldn't " +
                                $"parse: '{txt.Text}'");
                        }
                    }
                }

                // [LOG-1] verbose: per-track tick dump, redundant with the
                // per-channel "Timeline ch=" line below (which is PROTECTED).
                if (settings != null && settings.logMidiMusicManager && logVerbose
                    && countHere > 0)
                {
                    var sample = _chordLabelsByTrack[trackIndex].Keys.OrderBy(t => t).Take(8)
                                 .Select(FormatTick);
                    Debug.Log($"[MidiMusicManager] chd tags in Track#{trackIndex} count={countHere} " +
                              $"ticks: {string.Join(", ", sample)}{(_chordLabelsByTrack[trackIndex].Count > 8 ? ", ..." : "")}");
                }

                trackIndex++;
            }

            // Build ordered timelines & reset cursors (unchanged)
            _chordTimelineByChannel.Clear();
            _chordTimelineCursor.Clear();
            _currentChordByChannel.Clear();

            foreach (var kv in _chordLabelsByChannel)
            {
                var ordered = kv.Value.Select(p => (tick: p.Key, label: p.Value))
                                      .OrderBy(p => p.tick).ToList();
                _chordTimelineByChannel[kv.Key] = ordered;
                _chordTimelineCursor[kv.Key] = 0;

                if (settings != null && settings.logMidiMusicManager && ordered.Count > 0)
                {
                    var sample = ordered.Take(8)
                        .Select(p => $"{FormatTick(p.tick)}:{p.label.sym}({p.label.roman})");
                    Debug.Log($"[MidiMusicManager] Timeline ch={kv.Key} count={ordered.Count} " +
                              $"first: {string.Join(" | ", sample)}{(ordered.Count > 8 ? " | ..." : "")}");
                }
            }

            // [LOG-1] verbose: counts only.
            if (settings != null && settings.logMidiMusicManager && logVerbose)
                Debug.Log($"[MidiMusicManager] Built chord labels: " +
                    $"tracks={_chordLabelsByTrack.Count} channels={_chordLabelsByChannel.Count}");
        }

        // ---- [LOG-1 / D-LOG-1=B] Chord-label typography -------------------
        //
        // The chd: marker's `roman` and `symbol` fields can arrive with a
        // non-ASCII glyph already destroyed: the major-seventh triangle, the
        // half-diminished slashed o, sharp and flat signs. MIDI text events
        // are written in a 7-bit alphabet by default, and any unmappable
        // character is replaced by a literal '?' AT WRITE TIME. By the time we
        // read the bytes the original character is gone; no amount of cleaning
        // recovers it, and a repaired-looking "I7" would mean a DIFFERENT
        // chord from the "Imaj7" that actually sounds.
        //
        // So we never DISPLAY the marker's glyph. The marker is
        // self-describing: it also carries `deg` (an int) and `quality`
        // (ChordQuality.ToString()), both ASCII by construction. The roman
        // numeral is rebuilt from `quality` through the table below, which is
        // ALWTTT-owned typography. Boundary position: MidiGenPlay decides what
        // sounds, ALWTTT decides how it is spelled on screen.
        //
        // The table keys on the raw quality STRING, not on the ChordQuality
        // enum, deliberately. That enum is package-owned and append-only: a
        // value switch would fail to compile on a rename and go silently
        // non-exhaustive on an addition. A string switch cannot do either --
        // an unknown name falls to the default, which reports itself once per
        // render so the table can be completed from a log line.
        private static string RomanSuffixForQuality(string q)
        {
            if (string.IsNullOrEmpty(q)) return "";

            switch (q)
            {
                // Confirmed against the MidiGenPlay authoring SSoT, 4.1.
                case "Major": return "";       // numeral case carries it
                case "Minor": return "";
                case "Major7": return "maj7";
                case "Minor7": return "m7";
                case "Dominant7": return "7";
                case "Major6": return "6";
                case "Minor6": return "m6";
                case "Dominant7sus4": return "7sus4";
                case "Dominant9": return "9";
                case "Major9": return "maj9";
                case "Minor9": return "m9";

                // NOT verified against MusicTheory.ChordQuality. These are the
                // expected member names for the qualities whose Roman suffixes
                // the authoring SSoT documents. If one is wrong the default
                // branch fires and the [LOG-1] warning prints the real name;
                // add it here then. A case that never matches costs nothing.
                case "Diminished": return "dim";
                case "Diminished7": return "dim7";
                case "Augmented": return "aug";
                case "HalfDiminished7": return "m7b5";
                case "Sus2": return "sus2";
                case "Sus4": return "sus4";

                default: return null;   // unmapped -> bare numeral + report
            }
        }

        /// <summary>[LOG-1] Rebuild the roman we show: authored accidental
        /// (kept only when it is ASCII 'b' or '#') + numeral core + a suffix
        /// derived from the quality field. A leading glyph that is neither is
        /// dropped rather than displayed.</summary>
        private static string NormalizeRoman(string raw, string qualityRaw,
                                             out bool unmapped)
        {
            unmapped = false;
            if (string.IsNullOrEmpty(raw)) return raw;

            const string Numerals = "IVXivx";

            string acc = "";
            int i = 0;
            if (Numerals.IndexOf(raw[0]) < 0)
            {
                if (raw[0] == 'b' || raw[0] == '#') acc = raw[0].ToString();
                i = 1;                                  // consume it either way
            }

            int j = i;
            while (j < raw.Length && Numerals.IndexOf(raw[j]) >= 0) j++;
            if (j == i) return raw;                     // no numeral core found

            string suffix = RomanSuffixForQuality(qualityRaw);
            if (suffix == null) { unmapped = true; suffix = ""; }

            return acc + raw.Substring(i, j - i) + suffix;
        }

        /// <summary>[LOG-1] A field that carries a replacement marker is a
        /// field whose original character is unrecoverable.</summary>
        private static bool LooksDamaged(string s) =>
            !string.IsNullOrEmpty(s) &&
            (s.IndexOf('?') >= 0 || s.IndexOf('\uFFFD') >= 0);

        /// <summary>[LOG-1] The diagnostic instrument. Fires at most once per
        /// render and prints the RAW fields, which is how we tell the two
        /// possible causes apart: if `raw sym` still shows a real sharp or
        /// flat sign, the MIDI transport is fine and the package wrote the
        /// '?' itself; if `raw sym` is damaged too, the transport is
        /// destroying the whole class of glyphs.</summary>
        private void ReportChordTagDamage(string rawRoman, string rawSym,
                                          string qualityRaw, string shownRoman,
                                          bool unmapped)
        {
            if (_chordTagDamageReported) return;
            if (settings == null || !settings.logMidiMusicManager) return;
            if (!unmapped && !LooksDamaged(rawRoman) && !LooksDamaged(rawSym))
                return;

            _chordTagDamageReported = true;
            Debug.LogWarning(
                $"{DebugTag} <color=#ff8844>[LOG-1]</color> chd tag repaired - " +
                $"raw roman='{rawRoman}' raw sym='{rawSym}' " +
                $"quality='{qualityRaw}' => shown roman='{shownRoman}'" +
                (unmapped
                    ? "  ** QUALITY NOT IN SUFFIX TABLE - add this name to " +
                      "RomanSuffixForQuality **"
                    : ""));
        }

        // Supports new and old formats:
        //   new: chd:<channel>:<roman>:<symbol>:<deg>:<quality>
        //   old: chd:<roman>:<symbol>:<deg>:<quality>
        //   old: chd:<roman>:<symbol>
        private bool TryParseChordTag(string s, out int ch, out ChordLabel label)
        {
            ch = -1; label = default;

            if (string.IsNullOrEmpty(s) || !s.StartsWith("chd:", StringComparison.OrdinalIgnoreCase))
                return false;

            var parts = s.Split(':'); // don’t limit; we want the real count

            // New format with channel first
            if (parts.Length >= 6 && int.TryParse(parts[1], out var chParsed))
            {
                ch = chParsed;
                label.sym = parts[3];
                label.deg = (parts.Length >= 5 && int.TryParse(parts[4], out var d)) ? d : 0;
                label.quality = (parts.Length >= 6 &&
                                 Enum.TryParse<ChordQuality>(parts[5], true, out var q))
                                ? q : (ChordQuality?)null;

                // [LOG-1 / D-LOG-1=B] Rebuild the roman from `quality` instead
                // of trusting the marker's glyph. `sym` is deliberately left
                // RAW here: it is the evidence the ST-LOG-2 discriminator
                // reads, and it is sanitised at display time instead.
                label.roman = NormalizeRoman(parts[2], parts[5], out var unmapped);
                ReportChordTagDamage(parts[2], label.sym, parts[5],
                                     label.roman, unmapped);
                return true;
            }

            // Back-compat (no channel)
            if (parts.Length >= 3)
            {
                label.sym = parts[2];
                label.deg = (parts.Length >= 4 && int.TryParse(parts[3], out var d2)) ? d2 : 0;
                label.quality = (parts.Length >= 5 &&
                                 Enum.TryParse<ChordQuality>(parts[4], true, out var q2))
                                ? q2 : (ChordQuality?)null;

                // [LOG-1] Same treatment on the legacy shape. The quality
                // field may be absent here; NormalizeRoman then returns the
                // bare numeral, which is still better than a '?' on screen.
                string qRaw2 = parts.Length >= 5 ? parts[4] : null;
                label.roman = NormalizeRoman(parts[1], qRaw2, out var unmapped2);
                ReportChordTagDamage(parts[1], label.sym, qRaw2,
                                     label.roman, unmapped2);
                return true;
            }

            if (settings != null && settings.logMidiMusicManager)
                Debug.LogWarning($"[MidiMusicManager] chd tag parse failed: '{s}'");
            return false;
        }

        private static bool TryParsePartTag(string tag, out PartInfoEvent info)
        {
            info = default;
            if (string.IsNullOrEmpty(tag) || !tag.StartsWith("part:",
                System.StringComparison.OrdinalIgnoreCase))
                return false;

            // part:<index>:<name>:<tonality>:<root>
            var parts = tag.Split(new[] { ':' }, 5);
            if (parts.Length < 5) return false;

            if (!int.TryParse(parts[1], out var idx)) return false;

            info.partIndex = idx;
            info.partName = parts[2];

            // Tonality / Root parse with safe fallbacks
            if (!System.Enum.TryParse(parts[3], out MidiGenPlay.MusicTheory.MusicTheory.Tonality ton))
                ton = MidiGenPlay.MusicTheory.MusicTheory.Tonality.Ionian;
            info.tonality = ton;

            if (!System.Enum.TryParse(parts[4], out Melanchall.DryWetMidi.MusicTheory.NoteName root))
                root = Melanchall.DryWetMidi.MusicTheory.NoteName.C;
            info.rootNote = root;
            return true;
        }


        private void EmitPartStarted(PartInfoEvent e)
        {
            foreach (var l in _partListeners) l.OnPartStarted(e);
            if (settings != null && settings.logMidiMusicManager)
                Debug.Log($"[MidiMusicManager] PartStart idx={e.partIndex} '{e.partName}'  " +
                    $"Tonality={e.tonality} Root={e.rootNote}");
        }

        private void ClearMarkers()
        {
            _partMarkers?.Clear();
            _chordLabelsByTrack?.Clear();
            _chordLabelsByChannel?.Clear();
            _chordTimelineByChannel?.Clear();
            _chordTimelineCursor?.Clear();
            _currentChordByChannel?.Clear();
        }

        private void OnSongStartedInternal()
        {
            if (logDebug) Debug.Log($"{DebugTag} OnSongStarted key={_currentKey} " +
                    $"metronome={MetronomeEnabled}");

            _beatIndex = 0;

            // re-apply metronome volume for this playback
            var metro01 = MetronomeEnabled ?
                Mathf.Clamp01((settings?.metronomeChannelVolume ?? 110) / 127f) :
                0f;
            mix.SetChannelVolume01(MidiGenerator.MetronomeChannel, metro01);
            _lastKnownVol01[MidiGenerator.MetronomeChannel] = metro01;

            // [BAL-1 task 0] M-AUDIO-MIX re-assert — DEFERRED and COMPOSED.
            // MPTK VERIFIABLY resets every channel CC7 to 100 on each play
            // (MPTK_Play → MPTK_InitSynth → new MPTKChannels →
            // fluid_channel_init_ctrl; EnableResetChannel default true), so the
            // loop cannot be retired. But OnEventStartPlayMidi fires BEFORE the
            // sequencer processes tick-0 events — an immediate write here lands
            // BEFORE the baked MIX-1 preamble CC7 and is overwritten by it. We
            // therefore re-assert only after the preamble is consumed. Writes
            // go through WriteChannelVolume01 (D-BAL-6=B): identity balance
            // reproduces the baked CC7 exactly (idempotent); non-identity
            // balance composes multiplicatively on top of the baked gain.
            if (_mixReassertCo != null) StopCoroutine(_mixReassertCo);
            _mixReassertCo = StartCoroutine(ReassertLiveMixAfterPreamble());


            // stop previous grid if any
            if (_beatGridCo != null) { StopCoroutine(_beatGridCo); _beatGridCo = null; }

            if (!string.IsNullOrEmpty(_currentKey) && cache.TryGetValue(_currentKey, out var entry))
            {
                NotifyTempoSignatureAtStart(_currentKey); // push BPM/TS immediately
                _beatGridCo = StartCoroutine(RunBeatGrid(_currentKey, entry.seconds));
                ApplyDeferredHighlightIfAny(); // apply highlight that was queued before channels were known
            }
            else if (logDebug)
                Debug.LogWarning($"{DebugTag} OnSongStarted but key/cache missing.");
        }

        // [BAL-1 task 0] Deferred, composed live-mix re-assert. Runs after the
        // tick-0 baked preamble is consumed so persisted/dev balance lands on
        // top of the baked CC7 rather than under it.
        private IEnumerator ReassertLiveMixAfterPreamble()
        {
            // Bounded wait: the tick-0 preamble is processed within the first
            // sequencer slices. 30 frames is a hard cap, not an expectation.
            for (int f = 0;
                 f < 30 && (player == null || !player.IsPlaying
                            || player.CurrentTick <= 0);
                 f++)
                yield return null;

            for (int ch = 0; ch < 16; ch++)
            {
                if (ch == MidiGenerator.MetronomeChannel) continue;
                WriteChannelVolume01(ch, Mathf.Clamp01(_lastKnownVol01[ch]));
            }
            _mixReassertCo = null;
        }

        private void OnSongEndedInternal()
        {
            ClearMarkers();
        }

        private void DevDumpMidi(string key, byte[] data, string label)
        {
            if (settings == null || !settings.debugDumpMidi
                || data == null || data.Length == 0)
                return;

            try
            {
                var dir = Path.Combine(Application.persistentDataPath, "MidiDumps");
                Directory.CreateDirectory(dir);

                // Safe-ish filename
                string safeLabel = Regex.Replace(label ?? "song", @"[^a-zA-Z0-9_\-]+", "_");
                string safeKey = Regex.Replace(key ?? "key", @"[^a-zA-Z0-9_\-]+", "_");
                string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{safeLabel}_{safeKey}.mid";

                var path = Path.Combine(dir, fileName);
                File.WriteAllBytes(path, data);

                if (logDebug && logVerbose)   // [LOG-1] verbose
                    Debug.Log($"{DebugTag} DevDumpMidi -> {path}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{DebugTag} DevDumpMidi failed: {ex.Message}");
            }
        }

        // Small helper to unify debug logging for contexts
        private void LogTrace(string msg)
        {
            if (logDebug && !string.IsNullOrEmpty(msg))
                Debug.Log($"{DebugTag} {msg}");
        }
        #endregion

        #region Mix and Highlight API
        public enum HighlightMode { None, DuckOthers, Solo }

        private string _highlightMusicianId;
        private HighlightMode _highlightMode = HighlightMode.None;
        private readonly float[] _lastKnownVol01 = new float[16];

        // ── [BAL-1] Bytes-plane mix gains (Boundary §8.3) ────────────────────────────
        // D-BAL-3=A: fixed per gig, set once by GigManager at gig start. Mutable dict
        // so the Dev lever can override at runtime (hash covers gains ⇒ next render
        // re-keys and re-renders deterministically).
        private Dictionary<MusicianTrackKey, float> _gigMixGains;
        public IReadOnlyDictionary<MusicianTrackKey, float> GigMixGains => _gigMixGains;

        // Channel-gain snapshot of the bytes currently PLAYING (live-plane compose
        // input, D-BAL-6=B). _pendingChannelGains is produced by RenderSinglePart and
        // adopted by PlayBytes; non-gig plays adopt identity.
        private float[] _pendingChannelGains;
        private readonly float[] _bakedGain01ByChannel =
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 };

        // ── [R5-d] Duck plane (third live-compose factor) ────────────────────────────
        // 1 = untouched. Never written by Highlight and never read by it: the
        // two compose through WriteChannelVolume01 instead of fighting over
        // _savedVol01. A duck therefore survives a Highlight save/restore cycle
        // unchanged, and vice versa.
        private readonly float[] _duck01ByChannel =
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 };

        /// <summary>
        /// [R5-d] Duck every channel except the soloist's, for the duration of a
        /// solo. The metronome is exempt (separate semantics, no gains — same
        /// carve-out WriteChannelVolume01 documents). Idempotent: calling it
        /// again with a new level simply re-composes.
        /// </summary>
        public void SetSoloDuck(int soloChannel, float duck01)
        {
            duck01 = Mathf.Clamp01(duck01);
            for (int ch = 0; ch < 16; ch++)
            {
                _duck01ByChannel[ch] =
                    (ch == soloChannel || ch == MidiGenerator.MetronomeChannel)
                        ? 1f : duck01;
            }
            ReapplyLiveVolumes();

            if (logDebug)
                Debug.Log($"{DebugTag} <color=#ffd084>[R5-d]</color> Solo duck "
                    + $"x{duck01:0.##} on every channel except ch{soloChannel}.");
        }

        /// <summary>[R5-d] Remove the duck. No-op when nothing is ducked, so it is
        /// safe to call at every loop boundary, part change and session end.</summary>
        public void ClearSoloDuck()
        {
            bool any = false;
            for (int ch = 0; ch < 16; ch++)
            {
                if (!Mathf.Approximately(_duck01ByChannel[ch], 1f))
                {
                    _duck01ByChannel[ch] = 1f;
                    any = true;
                }
            }

            if (!any) return;
            ReapplyLiveVolumes();

            if (logDebug)
                Debug.Log($"{DebugTag} [R5-d] Solo duck cleared.");
        }

        /// <summary>[R5-d] Re-send every channel's live intent through the write
        /// boundary so a change to the duck plane takes effect immediately.
        /// Reads _lastKnownVol01, which Awake seeds to 1f, so this can never
        /// silence a channel that was never explicitly set.</summary>
        private void ReapplyLiveVolumes()
        {
            for (int ch = 0; ch < 16; ch++)
            {
                if (ch == MidiGenerator.MetronomeChannel) continue;
                WriteChannelVolume01(ch, Mathf.Clamp01(_lastKnownVol01[ch]));
            }
        }

        // [BAL-1 task 4] Package readback: CC7 actually emitted per gained track for
        // the last render (fresh or bundle replay). Null ⇒ ungained render.
        public IReadOnlyDictionary<MusicianTrackKey, int> LastAppliedCc7ByTrack
        { get; private set; }

        // [ORDER-1 / R2d 2026-07-31] Package readback (MGP-ALWTTT-BASS-ORDER-1):
        // which source WON the shared progression channel in the last render, and
        // the asset behind it. `ResolvedSource.HostDefault` means our own
        // defaultProgressionPalette supplied the harmony everyone played over.
        //
        // Truth-only. This is deliberately NOT a cache-key input: it is produced
        // by the render, and the cache key must exist before the render. The key
        // uses the pre-render harmony-identity token instead (D-R2-10=A). Do not
        // "improve" this by feeding it back into the hash — it cannot work.
        public ResolvedSource LastSharedProgressionSource { get; private set; }
        public string LastSharedProgressionAssetName { get; private set; }

        // [JAM-1 / P7] Runtime CLONE of the progression that won the shared
        // channel on the last render. Session-lifetime only: it is a runtime
        // ScriptableObject, NOT an asset. Never serialize it, never write it
        // to disk, never expect it to survive a domain reload.
        public MidiGenPlay.ChordProgressionData LastSharedProgressionData { get; private set; }

#if ALWTTT_DEV
        /// <summary>[CSV-3] Musical identity the last render ACTUALLY used, read from the
        /// PartConfig AFTER generation (post ChordTrack step-2b tonality alignment).
        /// Read-only truth; never a gameplay input. Dev-only (batch constraint).</summary>
        public MidiGenPlay.MusicTheory.MusicTheory.TimeSignature LastRenderResolvedTimeSignature { get; private set; }
        public MidiGenPlay.MusicTheory.MusicTheory.Tonality LastRenderResolvedTonality { get; private set; }
        public Melanchall.DryWetMidi.MusicTheory.NoteName LastRenderResolvedRootNote { get; private set; }
#endif

        private Coroutine _mixReassertCo;

        public void SetGigMixGains(IReadOnlyDictionary<MusicianTrackKey, float> gains)
        {
            _gigMixGains = (gains != null && gains.Count > 0)
                ? new Dictionary<MusicianTrackKey, float>(gains)
                : null;
            if (logDebug) Debug.Log($"{DebugTag} [BAL-1] gig mix gains set: " +
                $"{_gigMixGains?.Count ?? 0} entries");
        }

        // [BAL-1 task 4] Dev-only runtime override. Rhythm rejected here too.
        public void DevOverrideMixGain(MusicianTrackKey key, float gain)
        {
            if (key.Role == TrackRole.Rhythm)
            {
                Debug.LogWarning($"{DebugTag} [BAL-1] Rhythm gain rejected (D-BAL-5=A).");
                return;
            }
            _gigMixGains ??= new Dictionary<MusicianTrackKey, float>();
            _gigMixGains[key] = Mathf.Clamp(gain, 0f, 1.27f);
        }

        private readonly float[] _savedVol01 = new float[16];
        private bool _hasSavedMix = false;
        private string _pendingHighlightMusicianId;
        private HighlightMode _pendingHighlightMode = HighlightMode.None;

        // Live mix (runtime only; uses IMixController; no MIDI byte changes)
        public void SetChannelVolume(int channel, float volume01)
        {
            volume01 = Mathf.Clamp01(volume01);
            _lastKnownVol01[channel] = volume01;
            WriteChannelVolume01(channel, volume01); // runtime mix only [BAL-1 composed]
            // [LOG-1] verbose: fires per channel, per render.
            if (logDebug && logVerbose)
                Debug.Log($"{DebugTag} SetChannelVolume ch={channel} vol={volume01:0.##}");
        }

        public void SetMusicianVolume01(string musicianId, float volume01)
        {
            if (string.IsNullOrEmpty(musicianId))
                return;



            volume01 = Mathf.Clamp01(volume01);

            var channels = ResolveChannelsForMusician(musicianId);
            foreach (var ch in channels)
            {
                if (ch == MidiGenerator.MetronomeChannel)
                    continue;

                _lastKnownVol01[ch] = volume01;
                WriteChannelVolume01(ch, volume01); // [BAL-1 composed]
                //if (logDebug)
                Debug.Log($"{DebugTag} SetMusicianVolume musician={musicianId} " +
                    $"ch={ch} vol={volume01:0.##}");
            }
        }

        // [BAL-1 / D-BAL-6=B] SINGLE write boundary for musician-channel live volume.
        // The live intent composes multiplicatively with the baked bytes-plane gain of
        // the playing part:  composedCc7 ≈ round(live01 × gain × 100).
        // Identity: live 1.0 × gain 1.0 → CC7 100 — the MPTK channel default AND the
        // baked identity, so identity writes are idempotent against the preamble.
        // IMixController semantics are untouched (v01 → v127); composition lives here.
        // Metronome writes deliberately bypass this (separate semantics, no gains).
        private void WriteChannelVolume01(int ch, float live01)
        {
            // [R5-d] Third gain plane. It multiplies INSIDE this method, so
            // D-BAL-6=B still holds: ONE write boundary, now composing three
            // planes (live intent x baked bytes gain x duck). Deliberately NOT
            // a save/restore of raw volumes — Highlight owns the single
            // _savedVol01 snapshot slot, and a second saver would restore the
            // other's values whenever the two interleave.
            float composed = Mathf.Clamp01(live01)
                * _bakedGain01ByChannel[ch]
                * _duck01ByChannel[ch]
                * (100f / 127f);
            mix?.SetChannelVolume01(ch, Mathf.Clamp01(composed));
            // [BAL-1 test 4] Dev readout: the LIVE-COMPOSED CC7 actually sent
            // (distinct from the baked LastAppliedCc7ByTrack). Lets the Audio
            // Mix tab prove compose (~25) vs stomp (~64) numerically.
            if (ch >= 0 && ch < 16)
                _lastComposedCc7ByChannel[ch] = Mathf.RoundToInt(composed * 127f);
        }

        // [BAL-1 test 4] Per-channel live-composed CC7 (dev-facing readout only).
        private readonly int[] _lastComposedCc7ByChannel = new int[16];

        /// <summary>[BAL-1 test 4] Live-composed CC7 last written to this channel
        /// (live01 × bakedGain × 100). Dev diagnostic; not part of the bytes plane.</summary>
        public int GetLiveComposedCc7(int channel)
            => (channel >= 0 && channel < 16) ? _lastComposedCc7ByChannel[channel] : -1;

        // Highlight: apply now if possible, else remember & apply at next OnSongStarted.
        public void Highlight(string musicianId, HighlightMode mode)
        {
            // Clear request or invalid id -> restore snapshot if any
            if (mode == HighlightMode.None || string.IsNullOrEmpty(musicianId))
            {
                RestoreSavedMix();
                _highlightMusicianId = null;
                _highlightMode = HighlightMode.None;
                _pendingHighlightMusicianId = null;
                _pendingHighlightMode = HighlightMode.None;
                if (logDebug) Debug.Log($"{DebugTag} Highlight cleared.");
                return;
            }

            // Idempotency: if same state already active, avoid re-sending CCs
            if (player != null && player.IsPlaying &&
                string.Equals(_highlightMusicianId, musicianId, StringComparison.Ordinal) &&
                _highlightMode == mode)
            {
                if (logDebug) Debug.Log($"{DebugTag} Highlight already active for {musicianId} " +
                    $"mode={mode}. Skipping.");
                return;
            }

            // Resolve channels now if possible
            var channels = new HashSet<int>(ResolveChannelsForMusician(musicianId));
            if (channels.Count == 0 || player == null || !player.IsPlaying)
            {
                _pendingHighlightMusicianId = musicianId;
                _pendingHighlightMode = mode;
                if (logDebug)
                    Debug.Log($"{DebugTag} Highlight deferred for {musicianId} " +
                        $"mode={mode} (no active channel map yet).");
                return;
            }

            // Take a one-time snapshot of the mix before we modify it
            if (!_hasSavedMix)
                SaveCurrentMixSnapshot();

            ApplyHighlightNow(channels, mode);
            _highlightMusicianId = musicianId;
            _highlightMode = mode;

            if (logDebug)
                Debug.Log($"{DebugTag} Highlight applied for {musicianId} mode={mode} " +
                    $"ch=[{string.Join(",", channels)}].");
        }

        // ----- Internal -----
        private IEnumerable<int> ResolveChannelsForMusician(string musicianId)
        {
            // Prefer current arrangement channel owners, else per-key cache if available.
            // 1) Try current full-band owners list (SetChannelOwners was called post-gen)
            if (!string.IsNullOrEmpty(musicianId) && _channelOwners != null && _channelOwners.Count > 0)
            {
                for (int ch = 0; ch < _channelOwners.Count; ch++)
                    if (string.Equals(_channelOwners[ch], musicianId, StringComparison.Ordinal))
                        yield return ch;
            }

            // 2) Fallback to per-key cache mapping if song key known
            if (!string.IsNullOrEmpty(_currentKey) &&
                channelOwnersByKey.TryGetValue(_currentKey, out var owners) &&
                owners != null && owners.Count > 0)
            {
                for (int ch = 0; ch < owners.Count; ch++)
                    if (string.Equals(owners[ch], musicianId, StringComparison.Ordinal))
                        yield return ch;
            }
        }

        private void ApplyHighlightNow(ISet<int> targetChannels, HighlightMode mode)
        {
            // Build the intended mix in one pass, excluding metronome channel.
            for (int ch = 0; ch < 16; ch++)
            {
                if (ch == MidiGenerator.MetronomeChannel) continue;

                bool isTarget = targetChannels.Contains(ch);
                float vol = 1f;

                switch (mode)
                {
                    case HighlightMode.DuckOthers:
                        vol = isTarget ? 1f : 0.7f;
                        break;
                    case HighlightMode.Solo:
                        vol = isTarget ? 1f : 0.2f;
                        break;
                }

                _lastKnownVol01[ch] = vol;
                WriteChannelVolume01(ch, vol); // [BAL-1 composed]
            }
        }

        private void SaveCurrentMixSnapshot()
        {
            // Snapshot all non-metronome channels from our last-known tracker
            for (int ch = 0; ch < 16; ch++)
            {
                if (ch == MidiGenerator.MetronomeChannel) continue;
                _savedVol01[ch] = _lastKnownVol01[ch];
            }
            _hasSavedMix = true;
        }

        private void RestoreSavedMix()
        {
            if (_hasSavedMix)
            {
                for (int ch = 0; ch < 16; ch++)
                {
                    if (ch == MidiGenerator.MetronomeChannel) continue;
                    var vol = Mathf.Clamp01(_savedVol01[ch]);
                    _lastKnownVol01[ch] = vol;
                    WriteChannelVolume01(ch, vol); // [BAL-1 composed]
                }
                _hasSavedMix = false; // snapshot consumed
                if (logDebug) Debug.Log($"{DebugTag} Restored saved mix after highlight.");
            }
            else
            {
                // No snapshot—fallback to neutral (preserves metronome separately)
                for (int ch = 0; ch < 16; ch++)
                {
                    if (ch == MidiGenerator.MetronomeChannel) continue;
                    _lastKnownVol01[ch] = 1f;
                    WriteChannelVolume01(ch, 1f); // [BAL-1 composed]
                }
                if (logDebug) Debug.Log($"{DebugTag} Restored neutral mix (no snapshot).");
            }
        }

        // Hook this near the end of OnSongStartedInternal so deferred highlight applies seamlessly.
        private void ApplyDeferredHighlightIfAny()
        {
            if (!string.IsNullOrEmpty(_highlightMusicianId) &&
                _pendingHighlightMode == _highlightMode &&
                string.Equals(_pendingHighlightMusicianId, _highlightMusicianId,
                StringComparison.Ordinal))
            {
                // Already applied; clear pending and return
                _pendingHighlightMusicianId = null;
                _pendingHighlightMode = HighlightMode.None;
                return;
            }

            if (string.IsNullOrEmpty(_pendingHighlightMusicianId)
                || _pendingHighlightMode == HighlightMode.None)
                return;

            var channels = ResolveChannelsForMusician(_pendingHighlightMusicianId).ToList();
            if (channels.Count > 0)
            {
                ApplyHighlightNow(new HashSet<int>(channels), _pendingHighlightMode);
                _highlightMusicianId = _pendingHighlightMusicianId;
                _highlightMode = _pendingHighlightMode;
                if (logDebug) Debug.Log($"{DebugTag} Deferred highlight applied for " +
                    $"{_pendingHighlightMusicianId} mode={_pendingHighlightMode}.");
            }

            _pendingHighlightMusicianId = null;
            _pendingHighlightMode = HighlightMode.None;
        }

        private void LogPlayTraceSummary(string key, string label, float seconds, int byteLen)
        {
            string owners = "(unknown)";
            if (channelOwnersByKey.TryGetValue(key, out var list) && list != null && list.Count > 0)
                owners = string.Join(", ", list.Select((id, ch) => $"{ch}:{id}"));

            string hiId = _pendingHighlightMusicianId ?? _highlightMusicianId ?? "(none)";

            Debug.Log(
                $"{DebugTag} TRACE " +
                $"label={label} key={key} dur={seconds:0.00}s bytes={byteLen} | " +
                $"metronome={MetronomeEnabled} | " +
                $"highlight={hiId}:{_pendingHighlightMode} | owners[{owners}]"
            );
        }


        private static string TsToString(MidiGenPlay.MusicTheory.MusicTheory.TimeSignature ts)
        {
            var p = MidiGenPlay.MusicTheory.MusicTheory.TimeSignatureProperties[ts];
            return $"{p.BeatsPerMeasure}/{p.BeatUnit}";
        }

        #endregion

        private void DumpConfigTrace(SongData song, SongConfig cfg, List<int> channelMap)
        {
            if (!logDebug || cfg == null) return;

            // Channels summary
            var roles = cfg.ChannelRoles ?? new List<TrackRole>();
            var owners = cfg.ChannelMusicianOrder ?? new List<string>();
            var chLines = new List<string>();
            for (int i = 0; i < Mathf.Max(roles.Count, owners.Count); i++)
            {
                var role = i < roles.Count ? roles[i].ToString() : "-";
                var own = i < owners.Count ? owners[i] : "-";
                var ch = i < channelMap.Count ? channelMap[i] : -1;
                chLines.Add($"{i}->{ch}:{role}/{own}");
            }

            Debug.Log($"{DebugTag} CONFIG '{song.SongTitle}' " +
                      $"parts={cfg.Parts?.Count ?? 0} " +
                      $"ch-map: {string.Join(" | ", chLines)}");

            // Parts & tracks
            for (int p = 0; p < (cfg.Parts?.Count ?? 0); p++)
            {
                var pc = cfg.Parts[p];
                Debug.Log($"{DebugTag} [Part#{p}] '{pc.Name}' TS={TsToString(pc.TimeSignature)} " +
                    $"rep={pc.Repetitions} Ton={pc.Tonality}/{pc.RootNote}");
                for (int t = 0; t < (pc.Tracks?.Count ?? 0); t++)
                {
                    var tr = pc.Tracks[t];
                    Debug.Log($"{DebugTag}    trk#{t} {tr}"); // uses TrackConfig.ToString()
                }
            }
        }

        #region [DBG-C1] Read-only truth surface (last render + chord timeline)

        /// <summary>Monotonic serial, bumped on EVERY RenderSinglePart return —
        /// fresh render and bundle-cache replay alike. Poll to detect refresh.</summary>
        public int LastRenderSerial { get; private set; }
        public int LastRenderPartIndex { get; private set; } = -1;
        public int LastRenderBpm { get; private set; }
        /// <summary>True when the last return was a bundle-cache replay; the
        /// resolved snapshot is then the ORIGINAL render's (D-DBG5=A).</summary>
        public bool LastRenderFromCache { get; private set; }

        private Dictionary<MusicianTrackKey, ResolvedTrackChoice> _lastResolvedByTrack = new();
        private Dictionary<MusicianTrackKey, MIDIInstrumentSO> _lastPinnedByTrack = new();

        /// <summary>[DBG-C1] Package readback of the last rendered part —
        /// what each composer actually resolved. Read-only truth; never an
        /// input to gameplay.</summary>
        public IReadOnlyDictionary<MusicianTrackKey, ResolvedTrackChoice> LastResolvedByTrack
            => _lastResolvedByTrack;
        public IReadOnlyDictionary<MusicianTrackKey, MIDIInstrumentSO> LastPinnedByTrack
            => _lastPinnedByTrack;

        private void PublishLastRender(
            int partIndex,
            int bpm,
            Dictionary<MusicianTrackKey, ResolvedTrackChoice> resolved,
            Dictionary<MusicianTrackKey, MIDIInstrumentSO> pinned,
            bool fromCache)
        {
            LastRenderPartIndex = partIndex;
            LastRenderBpm = bpm;
            LastRenderFromCache = fromCache;
            _lastResolvedByTrack = resolved != null
                ? new Dictionary<MusicianTrackKey, ResolvedTrackChoice>(resolved)
                : new Dictionary<MusicianTrackKey, ResolvedTrackChoice>();
            _lastPinnedByTrack = pinned != null
                ? new Dictionary<MusicianTrackKey, MIDIInstrumentSO>(pinned)
                : new Dictionary<MusicianTrackKey, MIDIInstrumentSO>();
            LastRenderSerial++;
        }

        /// <summary>[DBG-C1 / Task 3] One entry of the parsed chd: chord
        /// timeline. Public DTO of the private ChordLabel.</summary>
        public readonly struct ChordTimelineEntry
        {
            public readonly long Tick;
            public readonly string Symbol;   // "Cm7"
            public readonly string Roman;    // "ii" / "IV"
            public readonly int Degree;      // 1..7 (0 when n/a)
            public readonly string Quality;  // ChordQuality name or null

            public ChordTimelineEntry(
                long tick, string symbol, string roman, int degree, string quality)
            {
                Tick = tick; Symbol = symbol; Roman = roman;
                Degree = degree; Quality = quality;
            }
        }

        /// <summary>
        /// [DBG-C1 / Task 3] Read-only snapshot of the chd:-derived chord
        /// timeline per MIDI channel for the CURRENTLY LOADED playback.
        /// Built from the governed chd: marker contract
        /// (MGP SSoT_Composer_Backing_Track §2.1). Snapshot copy — safe to
        /// hold across frames; empty when nothing is loaded.
        /// </summary>
        public Dictionary<int, List<ChordTimelineEntry>> GetChordTimelineSnapshot()
        {
            var result = new Dictionary<int, List<ChordTimelineEntry>>();
            foreach (var kv in _chordTimelineByChannel)
            {
                var list = new List<ChordTimelineEntry>(kv.Value.Count);
                foreach (var (tick, label) in kv.Value)
                {
                    list.Add(new ChordTimelineEntry(
                        tick, label.sym, label.roman, label.deg,
                        label.quality?.ToString()));
                }
                result[kv.Key] = list;
            }
            return result;
        }

        #endregion

        #region [B1] Stem cache (per-track persistence + DryWetMidi merge)

        /// <summary>
        /// [B1 / D7=B] Reset the per-song stem and part-bundle caches.
        /// Called from CompositionSession.Begin() and End() — entry/exit
        /// of a song lifecycle. Does NOT clear the legacy `cache` (full
        /// pre-rendered SongData), which has its own lifetime.
        /// </summary>
        public void ResetStemCache()
        {
            int s = _stemCache.Count;
            int b = _partBundleCache.Count;
            _stemCache.Clear();
            _partBundleCache.Clear();
            if (logDebug)
            {
                Debug.Log(
                    $"{DebugTag} <color=cyan>[B1][stemCache]</color> Reset " +
                    $"stems_cleared={s} bundles_cleared={b}");
            }
        }

        /// <summary>
        /// [B1 / D-A2=A] Hash of part-level structural inputs. A change
        /// here invalidates every track stem for the part (full regen).
        /// Excludes Repetitions (sequencing only, not generation).
        /// </summary>
        private static string ComputePartMeterHash(SongConfig.PartConfig part)
        {
            if (part == null) return "_";
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            return string.Join("|",
                part.TimeSignature.ToString(),
                part.Tonality.ToString(),
                part.RootNote.ToString(),
                part.Measures.ToString(inv),
                part.TempoRange.ToString(),
                part.ExplicitBpm.HasValue
                    ? part.ExplicitBpm.Value.ToString(inv)
                    : "_",
                part.TempoScale.ToString("F4", inv));
        }

        // [DBG-C1] Stem identity is (musicianId, role): a musician holding two
        // roles caches two independent stems.
        private static string BuildStemKey(
            MusicianTrackKey key, string trackInputsHash, string partMeterHash)
            => $"{key.MusicianId}:{key.Role}|{trackInputsHash}|{partMeterHash}";

        private static string BuildPartBundleKey(
            string partMeterHash, IEnumerable<string> musicianAndTrackHashEntries)
        {
            // Each entry: "musicianId#trackInputsHash". Sort for determinism;
            // the bundle key must be invariant under track ordering.
            var arr = musicianAndTrackHashEntries.ToArray();
            System.Array.Sort(arr, System.StringComparer.Ordinal);
            return $"{partMeterHash}@@{string.Join(",", arr)}";
        }

        /// <summary>
        /// [B1] Drop every stem-cache and bundle-cache entry tied to the
        /// given partMeterHash. Called from F-4 Stage A catch (locked spec)
        /// and any future per-part invalidation needs. Other parts in the
        /// same song are not touched.
        /// </summary>
        private void InvalidateStemCacheForPart(string partMeterHash)
        {
            if (string.IsNullOrEmpty(partMeterHash)) return;

            var stemSuffix = "|" + partMeterHash;
            var stemRemove = _stemCache.Keys
                .Where(k => k != null && k.EndsWith(stemSuffix, StringComparison.Ordinal))
                .ToList();
            foreach (var k in stemRemove) _stemCache.Remove(k);

            var bundlePrefix = partMeterHash + "@@";
            var bundleRemove = _partBundleCache.Keys
                .Where(k => k != null && k.StartsWith(bundlePrefix, StringComparison.Ordinal))
                .ToList();
            foreach (var k in bundleRemove) _partBundleCache.Remove(k);

            if (logDebug)
            {
                Debug.Log(
                    $"{DebugTag} <color=cyan>[B1][stemCache]</color> " +
                    $"InvalidateStemCacheForPart partMeterHash='{partMeterHash}' " +
                    $"stems_removed={stemRemove.Count} bundles_removed={bundleRemove.Count}");
            }
        }

        /// <summary>
        /// [B1] Combine multiple stem MidiFile byte arrays into one merged
        /// MidiFile. Each stem is parsed, its TrackChunks are cloned and
        /// appended to a single output file with the time-division of the
        /// first non-empty stem. Tempo/TS events at time 0 may duplicate
        /// across stems; DryWetMidi handles that gracefully (latest wins
        /// at a given time, identical events collapse). Returns null on
        /// empty input.
        /// </summary>
        private static byte[] MergeStems(IEnumerable<byte[]> stemBytesOrdered)
        {
            if (stemBytesOrdered == null) return null;

            MidiFile output = null;
            foreach (var bytes in stemBytesOrdered)
            {
                if (bytes == null || bytes.Length == 0) continue;
                using var ms = new MemoryStream(bytes);
                var midi = MidiFile.Read(ms);
                if (output == null)
                {
                    output = new MidiFile { TimeDivision = midi.TimeDivision };
                }
                foreach (var chunk in midi.GetTrackChunks())
                {
                    var cloned = chunk.Clone();
                    if (cloned is TrackChunk tc)
                        output.Chunks.Add(tc);
                }
            }

            if (output == null || output.Chunks.Count == 0) return null;
            using var msOut = new MemoryStream();
            output.Write(msOut);
            return msOut.ToArray();
        }

        #endregion
    }
}
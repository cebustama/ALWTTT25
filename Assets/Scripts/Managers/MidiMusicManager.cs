using ALWTTT.Data;
using ALWTTT.Music;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MidiGenPlay;
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

namespace ALWTTT.Managers
{
    public class MidiMusicManager : MonoBehaviour
    {
        private const string DebugTag = "<color=white>[MidiMusicManager]</color>";

        public static MidiMusicManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private MidiGenPlayConfig settings;

        [Header("Refs")]
        [SerializeField] private MonoBehaviour playerBehaviour; // IPlayMidi (MPTK)

        [Header("Options")]
        [SerializeField] private bool logDebug = true;
        [SerializeField] private bool _postProcEnabled = false;
        [SerializeField] private bool _personalityBiasEnabled = false;

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

        private GameManager GameManager => GameManager.Instance;

        private MidiGenerator generator;

        private IPlayMidi player;
        private IMixController mix;
        private IInstrumentRepository instrumentRepo;
        private IPatternRepository patternRepo;

        private IReadOnlyDictionary<string, IMusicianPersonality> _personalities = 
            Array.Empty<KeyValuePair<string, IMusicianPersonality>>()
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        private Action _onSongStartedHandler;
        private Action _onSongEndedHandler;

        public void SetPostProcessingEnabled(bool enabled) => _postProcEnabled = enabled;
        public void SetPersonalityBiasEnabled(bool enabled) => _personalityBiasEnabled = enabled;

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

            generator = new MidiGenerator(settings);

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

        public void SetMusicianPersonalities(
            IReadOnlyDictionary<string, IMusicianPersonality> personalities)
        {
            _personalities = personalities ?? new Dictionary<string, IMusicianPersonality>();
        }
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
                int ch = owners.IndexOf(id);
                if (ch >= 0) allowed.Add(ch);
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

            return PlayBytes(key, maskedData, full.seconds, 
                $"subset[{allowed.Count}] '{song.SongTitle}'");
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

            // Obtain config and key
            var config = song.GenerateConfig(bandList);

            // pre-generation (arrangement) mutations to adjust parts/tracks/tempo/etc.
            config = ApplyArrangementMutations(config);

            var key = CacheKey(song, bandList);

            // Channel mapping
            var channelMap = BuildChannelMap(config.ChannelRoles);
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

            // Midi Generation
            var midi = generator.GenerateSong(config);

            // post-generation (MIDI) processors to modify events/tracks/velocities/etc.
            midi = ApplyPostProcessing(midi);

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
                Debug.Log($"{DebugTag} Generated '{song.SongTitle}' tracks:{midi.GetTrackChunks().Count()} ch:[{used}] dur:{seconds:0.00}s");
            }
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

        private (string key, SongCacheEntry entry) EnsureInCache(SongData song, IList<Characters.Band.MusicianBase> band)
        {
            EnsureRegistriesLoaded();

            var b = band ?? GameManager.PersistentGameplayData.MusicianList;
            var key = CacheKey(song, b);

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
                        { labelSym = lab.sym; labelRoman = lab.roman; 
                            degreeIndex = lab.deg; qual = lab.quality; }
                    }

                    var chord = new ChordEvent
                    {
                        musicianId = musId,
                        channel = ch,
                        notes = notes.Select(e => e.Value).ToList(),
                        time = n0.RealTime / 1000f,
                        anchor = anchor,
                        symbol = labelSym,
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

        private float PlayBytes(string key, byte[] data, float seconds, string label)
        {
            if (player == null) { Debug.LogError($"{DebugTag} No IPlayMidi."); return 0f; }

            player.Stop();            // will trigger OnSongEnded → ClearMarkers()
            _currentKey = key;

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
            if (settings != null && settings.logMidiMusicManager)
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

                if (logDebug)
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
                                _chordLabelsByTrack[trackIndex] = byTickTrack = new Dictionary<long, ChordLabel>();
                            byTickTrack[te.Time] = label;

                            // Channel map (primary) — from tag
                            if (tagCh >= 0)
                            {
                                if (!_chordLabelsByChannel.TryGetValue(tagCh, out var byTickCh))
                                    _chordLabelsByChannel[tagCh] = byTickCh = new Dictionary<long, ChordLabel>();
                                byTickCh[te.Time] = label;
                            }

                            countHere++;
                        }
                        else if (txt.Text.StartsWith("chd:", StringComparison.OrdinalIgnoreCase) &&
                                 settings != null && settings.logMidiMusicManager)
                        {
                            Debug.LogWarning($"[MidiMusicManager] Found chd text but couldn't parse: '{txt.Text}'");
                        }
                    }
                }

                if (settings != null && settings.logMidiMusicManager && countHere > 0)
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
                    var sample = ordered.Take(8).Select(p => $"{FormatTick(p.tick)}:{p.label.sym}({p.label.roman})");
                    Debug.Log($"[MidiMusicManager] Timeline ch={kv.Key} count={ordered.Count} " +
                              $"first: {string.Join(" | ", sample)}{(ordered.Count > 8 ? " | ..." : "")}");
                }
            }

            if (settings != null && settings.logMidiMusicManager)
                Debug.Log($"[MidiMusicManager] Built chord labels: tracks={_chordLabelsByTrack.Count} channels={_chordLabelsByChannel.Count}");
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
                label.roman = parts[2];
                label.sym = parts[3];
                label.deg = (parts.Length >= 5 && int.TryParse(parts[4], out var d)) ? d : 0;
                label.quality = (parts.Length >= 6 &&
                                 Enum.TryParse<ChordQuality>(parts[5], true, out var q))
                                ? q : (ChordQuality?)null;
                return true;
            }

            // Back-compat (no channel)
            if (parts.Length >= 3)
            {
                label.roman = parts[1];
                label.sym = parts[2];
                label.deg = (parts.Length >= 4 && int.TryParse(parts[3], out var d2)) ? d2 : 0;
                label.quality = (parts.Length >= 5 &&
                                 Enum.TryParse<ChordQuality>(parts[4], true, out var q2))
                                ? q2 : (ChordQuality?)null;
                return true;
            }

            if (settings != null && settings.logMidiMusicManager)
                Debug.LogWarning($"[MidiMusicManager] chd tag parse failed: '{s}'");
            return false;
        }

        private static bool TryParsePartTag(string tag, out PartInfoEvent info)
        {
            info = default;
            if (string.IsNullOrEmpty(tag) || !tag.StartsWith("part:", System.StringComparison.OrdinalIgnoreCase))
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

        // Tiny private context implementation
        private sealed class ArrangementContext : IArrangementContext
        {
            public IReadOnlyDictionary<string, IMusicianPersonality> Personalities { get; }
            public System.Random Rng { get; }

            private readonly Action<string> _log;

            public ArrangementContext(IReadOnlyDictionary<string, IMusicianPersonality> pers, System.Random rng, Action<string> log)
            {
                Personalities = pers ?? new Dictionary<string, IMusicianPersonality>();
                Rng = rng ?? new System.Random();
                _log = log ?? (_ => { });
            }

            public void Log(string message) => _log?.Invoke(message);
        }

        private SongConfig ApplyArrangementMutations(SongConfig cfg)
        {
            if (cfg == null) return null;

            // TODO (Phase 2+): seed from final cache key for cross-run determinism of arrangement mutations
            var seed = (_currentKey ?? "song").GetHashCode();
            var rng = new System.Random(seed);

            var persForPhase1 = _personalityBiasEnabled
                ? _personalities
                : new Dictionary<string, IMusicianPersonality>();
            var ctx = new ArrangementContext(persForPhase1, rng, LogTrace);

            // Sort and run
            if (_pendingArrangementMutators.Count > 0 && logDebug)
                Debug.Log($"{DebugTag} ArrangementMutators: {_pendingArrangementMutators.Count}");

            foreach (var m in _pendingArrangementMutators.OrderBy(m => m.Order))
            {
                try
                {
                    var before = cfg;
                    cfg = m.Mutate(cfg, ctx) ?? cfg;
                    if (logDebug) LogTrace($"[Arrange] {m.Name} {(ReferenceEquals(before, cfg) ? "(in-place)" : "(new cfg)")}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"{DebugTag} Mutator '{m.Name}' failed: {ex.Message}");
                }
            }

            return cfg;
        }

        private sealed class PostProcessContext : IPostProcessContext
        {
            public TempoMap TempoMap { get; }
            public IReadOnlyDictionary<string, IMusicianPersonality> Personalities { get; }
            public System.Random Rng { get; }
            private readonly Action<string> _log;

            public PostProcessContext(TempoMap tempoMap,
                                      IReadOnlyDictionary<string, IMusicianPersonality> pers,
                                      System.Random rng,
                                      Action<string> log)
            {
                TempoMap = tempoMap;
                Personalities = pers ?? new Dictionary<string, IMusicianPersonality>();
                Rng = rng ?? new System.Random();
                _log = log ?? (_ => { });
            }

            public void Log(string message) => _log?.Invoke(message);
        }

        // Returns the same MIDI when there are no post-processors or _postProcEnabled is false.
        private MidiFile ApplyPostProcessing(MidiFile midi)
        {
            if (midi == null || !_postProcEnabled) return midi;

            var tempoMap = midi.GetTempoMap();
            var seed = ((_currentKey ?? "song") + "::post").GetHashCode();
            var rng = new System.Random(seed);

            var persForPhase1 = _personalityBiasEnabled
                ? _personalities
                : new Dictionary<string, IMusicianPersonality>();
            var ctx = new PostProcessContext(tempoMap, persForPhase1, rng, LogTrace);

            if (_pendingPostProcessors.Count > 0 && logDebug)
                Debug.Log($"{DebugTag} PostProcessors: {_pendingPostProcessors.Count}");

            foreach (var p in _pendingPostProcessors.OrderBy(p => p.Order))
            {
                try
                {
                    var before = midi;
                    midi = p.Process(midi, ctx) ?? midi;
                    if (logDebug) LogTrace($"[Post] {p.Name} {(ReferenceEquals(before, midi) ? "(in-place)" : "(new midi)")}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"{DebugTag} PostProcessor '{p.Name}' failed: {ex.Message}");
                }
            }
            return midi;
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

                if (logDebug) Debug.Log($"{DebugTag} DevDumpMidi -> {path}");
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

        #region New API
        public enum IntroStyle { CountIn, Riff, Pad }
        public enum SoloStyle { Emotional, Virtuoso, Facemelter }
        public enum HighlightMode { None, DuckOthers, Solo }

        public struct HumanizeOptions
        {
            public int maxTickOffset;   // e.g., 0..10 ticks
            public int velocityJitter;  // e.g., 0..12
            public int lengthJitter;    // e.g., 0..8
        }

        public struct MistakeProfile
        {
            public float frequency01;   // 0..1 how often
            public float severity01;    // 0..1 how noticeable
            public bool affectRhythm;   // melodic-only vs. include drums
        }

        public enum Scope { CurrentSong, Global }

        // Optional “strategy” override by string id until a stronger type exists.
        public readonly struct StrategyOverride
        {
            public readonly string StrategyId;
            public StrategyOverride(string id) { StrategyId = id; }
            public override string ToString() => StrategyId ?? "(none)";
        }

        private class IntroIntent 
        { 
            public string musicianId; 
            public int measures; 
            public IntroStyle style; 
        }

        private class SoloIntent 
        { 
            public string musicianId; 
            public int measures; 
            public SoloStyle style; 
        }

        private class ReplaceTrackIntent 
        { 
            public int partIndexOrAll; 
            public string musicianId; 
            public string strategyId; 
        }

        private class HumanizeIntent { public HumanizeOptions options; }

        private class MistakeIntent 
        { 
            public string target; 
            public MistakeProfile profile; 
            public Scope scope; 
        }

        private readonly List<object> _pendingArrangementIntents = new(); // IntroIntent, SoloIntent, ReplaceTrackIntent, etc.
        private readonly List<object> _pendingPostProcIntents = new(); // HumanizeIntent, MistakeIntent, etc.
        private readonly List<CardData> _pendingCards = new(); // CardData queue (mapping happens later)

        private readonly List<IArrangementMutator> _pendingArrangementMutators = new();
        private readonly List<IMidiPostProcessor> _pendingPostProcessors = new();

        private float? _pendingTempoScaleNextSong = null;

        private string _highlightMusicianId;
        private HighlightMode _highlightMode = HighlightMode.None;
        private readonly float[] _lastKnownVol01 = new float[16]; // tracks any mix writes we do
        private readonly float[] _savedVol01 = new float[16]; // snapshot before highlight
        private bool _hasSavedMix = false;                       // true if snapshot is valid
        private string _pendingHighlightMusicianId; // apply at next OnSongStarted if needed
        private HighlightMode _pendingHighlightMode = HighlightMode.None;

        public bool HasPendingMutations =>
            _pendingArrangementMutators.Count > 0 ||
            _pendingPostProcessors.Count > 0 ||
            _pendingArrangementIntents.Count > 0 ||
            _pendingPostProcIntents.Count > 0 ||
            _pendingCards.Count > 0 ||
            _pendingTempoScaleNextSong.HasValue;

        // ----- Public methods -----

        // Cards will be translated to intents
        public void ApplyCards(IEnumerable<CardData> cards)
        {
            _pendingCards.Clear();
            if (cards != null) _pendingCards.AddRange(cards);
            if (logDebug)
                Debug.Log($"{DebugTag} ApplyCards queued count={_pendingCards.Count} (Phase 1: no structural effect yet).");
        }

        // Arrangement / structure
        public void AddIntro(string musicianId, int measures, IntroStyle style = IntroStyle.CountIn)
        {
            _pendingArrangementIntents.Add(new IntroIntent
            {
                musicianId = musicianId,
                measures = Mathf.Max(1, measures),
                style = style
            });
            if (logDebug) Debug.Log($"{DebugTag} AddIntro queued for {musicianId} measures={measures} style={style}.");
        }

        public void AppendSoloPart(string musicianId, SoloStyle style, int measures)
        {
            _pendingArrangementIntents.Add(new SoloIntent
            {
                musicianId = musicianId,
                measures = Mathf.Max(1, measures),
                style = style
            });
            if (logDebug) Debug.Log($"{DebugTag} AppendSoloPart queued for {musicianId} measures={measures} style={style}.");
        }

        public void ReplaceTrack(int partIndexOrAll, string musicianId, StrategyOverride? newStrategy)
        {
            _pendingArrangementIntents.Add(new ReplaceTrackIntent
            {
                partIndexOrAll = partIndexOrAll, // use -1 for “all parts”
                musicianId = musicianId,
                strategyId = newStrategy?.StrategyId
            });
            if (logDebug) Debug.Log($"{DebugTag} ReplaceTrack queued for {musicianId} part={partIndexOrAll} strategy={newStrategy}.");
        }

        // Change BPM
        public void ScheduleNextSongTempoScale(float factor)
        {
            _pendingTempoScaleNextSong = factor;
            if (logDebug) Debug.Log($"{DebugTag} Scheduled next-song tempo scale x{factor:0.###} (Phase 1: not applied yet).");
        }

        // Post-processing
        public void EnableHumanization(HumanizeOptions options)
        {
            _pendingPostProcIntents.Add(new HumanizeIntent { options = options });
            if (logDebug) Debug.Log($"{DebugTag} EnableHumanization queued (Phase 1: post-proc disabled by default).");
        }

        public void EnableMistakes(
            string musicianIdOrAll, MistakeProfile profile, Scope scope = Scope.CurrentSong)
        {
            _pendingPostProcIntents.Add(new MistakeIntent
            {
                target = musicianIdOrAll,
                profile = profile,
                scope = scope
            });
            if (logDebug) Debug.Log($"{DebugTag} EnableMistakes queued target='{musicianIdOrAll}' scope={scope}.");
        }

        // Live mix (runtime only; uses IMixController; no MIDI byte changes)
        public void SetChannelVolume(int channel, float volume01)
        {
            volume01 = Mathf.Clamp01(volume01);
            _lastKnownVol01[channel] = volume01;
            mix?.SetChannelVolume01(channel, volume01); // runtime mix only
            if (logDebug) Debug.Log($"{DebugTag} SetChannelVolume ch={channel} vol={volume01:0.##}");
        }

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
                if (logDebug) Debug.Log($"{DebugTag} Highlight already active for {musicianId} mode={mode}. Skipping.");
                return;
            }

            // Resolve channels now if possible
            var channels = new HashSet<int>(ResolveChannelsForMusician(musicianId));
            if (channels.Count == 0 || player == null || !player.IsPlaying)
            {
                _pendingHighlightMusicianId = musicianId;
                _pendingHighlightMode = mode;
                if (logDebug)
                    Debug.Log($"{DebugTag} Highlight deferred for {musicianId} mode={mode} (no active channel map yet).");
                return;
            }

            // Take a one-time snapshot of the mix before we modify it
            if (!_hasSavedMix)
                SaveCurrentMixSnapshot();

            ApplyHighlightNow(channels, mode);
            _highlightMusicianId = musicianId;
            _highlightMode = mode;

            if (logDebug)
                Debug.Log($"{DebugTag} Highlight applied for {musicianId} mode={mode} ch=[{string.Join(",", channels)}].");
        }

        // Optional: clear all queued intents (useful between songs/tests)
        public void ClearPendingMutations()
        {
            _pendingArrangementIntents.Clear();
            _pendingPostProcIntents.Clear();
            _pendingTempoScaleNextSong = null;
            _pendingCards.Clear();
            _pendingArrangementMutators.Clear();
            _pendingPostProcessors.Clear();
            if (logDebug) 
                Debug.Log($"{DebugTag} " +
                    $"Cleared all pending intents and mutator/postproc registries.");
        }

        public void EnqueueArrangementMutator(IArrangementMutator mut)
        {
            if (mut != null) _pendingArrangementMutators.Add(mut);
        }
        public void EnqueuePostProcessor(IMidiPostProcessor pass)
        {
            if (pass != null) _pendingPostProcessors.Add(pass);
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
                mix?.SetChannelVolume01(ch, vol);
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
                    mix?.SetChannelVolume01(ch, vol);
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
                    mix?.SetChannelVolume01(ch, 1f);
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
                if (logDebug) Debug.Log($"{DebugTag} Deferred highlight applied for {_pendingHighlightMusicianId} mode={_pendingHighlightMode}.");
            }

            _pendingHighlightMusicianId = null;
            _pendingHighlightMode = HighlightMode.None;
        }

        private void LogPlayTraceSummary(string key, string label, float seconds, int byteLen)
        {
            // owners summary (if known)
            string owners = "(unknown)";
            if (channelOwnersByKey.TryGetValue(key, out var list) && list != null && list.Count > 0)
                owners = string.Join(", ", list.Select((id, ch) => $"{ch}:{id}"));

            // highlight status
            string hiId = _pendingHighlightMusicianId ?? _highlightMusicianId ?? "(none)";

            // pre-format the tricky values (avoid nested ternary + escapes inside the interpolation)
            string tempoNext = _pendingTempoScaleNextSong.HasValue
                ? _pendingTempoScaleNextSong.Value.ToString("0.###")
                : "-";
            int personalityCount = (_personalities != null) ? _personalities.Count : 0;

            Debug.Log(
                $"{DebugTag} TRACE " +
                $"label={label} key={key} dur={seconds:0.00}s bytes={byteLen} | " +
                $"mutators={_pendingArrangementMutators.Count} post={_pendingPostProcessors.Count} " +
                $"intents.arr={_pendingArrangementIntents.Count} intents.post={_pendingPostProcIntents.Count} " +
                $"cards={_pendingCards.Count} tempoNext={tempoNext} | " +
                $"metronome={MetronomeEnabled} pers={personalityCount} | " +
                $"highlight={hiId}:{_pendingHighlightMode} | owners[{owners}]"
            );
        }
        #endregion
    }
}
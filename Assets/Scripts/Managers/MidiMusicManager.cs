using ALWTTT.Data;
using ALWTTT.Music;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MidiGenPlay;
using MidiGenPlay.MusicTheory;
using MidiPlayerTK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;

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

        private IPlayMidi player;
        private MidiGenerator generator;

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

        private void EnsureRegistriesLoaded()
        {
            if (registriesLoaded) return;

            // Instruments
            allInstruments = Resources.LoadAll<MIDIInstrumentSO>(
                settings.resourcesInstrumentsPath).ToList();

            percussionInstruments = 
                allInstruments.OfType<MIDIPercussionInstrumentSO>().ToList();
            melodicInstruments = 
                allInstruments.Where(i => !(i is MIDIPercussionInstrumentSO)).ToList();

            // Patterns
            allDrumPatterns = Resources.LoadAll<DrumPatternData>(
                settings.ResourcesDrumsPath).ToList();
            allChordPatterns = Resources.LoadAll<ChordProgressionData>(
                settings.ResourcesChordsPath).ToList();
            allMelodyPatterns = Resources.LoadAll<MelodyPatternData>(
                settings.ResourcesMelodiesPath).ToList();

            registriesLoaded = true;
            if (logDebug) Debug.Log($"{DebugTag} Registries loaded.");
        }
        #endregion

        #region Midi Events
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

        private Coroutine _beatGridCo;
        private readonly List<ITempoSignatureListener> _tempoSigSubs = new();

        // Beat detection (very simple: kick = beat)
        [SerializeField] private int drumChannel = 9; // MIDI ch 10 (0-based = 9)
        [SerializeField] private int[] kickNotes = new[] { 35, 36 }; // Acoustic/Bass drum
        private int _beatIndex = 0;

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
        public void Register(ITempoSignatureListener l) { if (l != null && !_tempoSigSubs.Contains(l)) _tempoSigSubs.Add(l); }
        public void Unregister(ITempoSignatureListener l) { _tempoSigSubs.Remove(l); }

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

            // Global MGP Settings
            if (settings == null) settings = MidiGenPlayConfig.FindInResources();
            if (settings == null) 
            { 
                settings = ScriptableObject.CreateInstance<MidiGenPlayConfig>(); 
            }
            logDebug = settings != null && settings.logMidiMusicManager;

            player.OnMidiEvents += HandleMidiEvents;
            player.OnSongStarted += () =>
            {
                if (logDebug) Debug.Log($"{DebugTag} OnSongStarted key={_currentKey} " +
                    $"metronome={MetronomeEnabled}");

                _beatIndex = 0;

                // re-apply metronome volume for this playback
                player.SetChannelVolume(MidiGenerator.MetronomeChannel,
                    MetronomeEnabled ? 110 : 0);

                // stop previous grid if any
                if (_beatGridCo != null) { StopCoroutine(_beatGridCo); _beatGridCo = null; }

                if (!string.IsNullOrEmpty(_currentKey) && cache.TryGetValue(_currentKey, out var entry))
                {
                    NotifyTempoSignatureAtStart(_currentKey); // push BPM/TS immediately
                    _beatGridCo = StartCoroutine(RunBeatGrid(_currentKey, entry.seconds)); 
                }
                else if (logDebug) 
                    Debug.LogWarning($"{DebugTag} OnSongStarted but key/cache missing.");
            };
            player.OnSongEnded += () => { /* optional reset */ };

            generator = new MidiGenerator();



            EnsureRegistriesLoaded();
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnMidiEvents -= HandleMidiEvents;
                player.OnSongStarted -= () => _beatIndex = 0;
                player.OnSongEnded -= () => { };
            }
        }
        #endregion

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
                Debug.Log($"{DebugTag} subset kept channels [{kept}] " +
                    $"(must include {MidiGenerator.MetronomeChannel})");
            }

            return PlayBytes(key, maskedData, full.seconds, 
                $"subset[{allowed.Count}] '{song.SongTitle}'");
        }

        public void Stop()
        {
            player?.Stop();
            if (logDebug) Debug.Log($"{DebugTag} Stop");
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
            // Apply immediately if a song is playing
            player?.SetChannelVolume(MidiGenerator.MetronomeChannel, enabled ? 110 : 0);
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
                    foreach (var s in _tempoSigSubs) s?.OnTimeSignatureChanged(numerator, denominator);
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

        private string MidiNoteToName(int note)
        {
            int octave = (note / 12) - 1;
            string[] names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            return $"{names[note % 12]}{octave}";
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

            // Ensure Part 1 Time Signature
            var part1Ts = config.Parts[0].TimeSignature;
            int tsNum = MusicTheory.TimeSignatureProperties[part1Ts].BeatsPerMeasure;
            int tsDen = MusicTheory.TimeSignatureProperties[part1Ts].BeatUnit;
            EnsureTimeSignatureAtZero(midi, tsNum, tsDen);

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

                    var chord = new ChordEvent
                    {
                        musicianId = musId,
                        channel = ch,
                        notes = notes.Select(e => e.Value).ToList(),
                        time = n0.RealTime / 1000f,
                        anchor = anchor
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

            player.Stop();
            _currentKey = key;
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
        #endregion
    }
}
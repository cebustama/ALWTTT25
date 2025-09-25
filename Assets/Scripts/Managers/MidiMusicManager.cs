using ALWTTT.Data;
using ALWTTT.Music;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MidiGenPlay;
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

        [Header("Refs")]
        [SerializeField] private MonoBehaviour playerBehaviour; // IPlayMidi (MPTK)

        [Header("Options")]
        [SerializeField] private bool logDebug = true;

        private IPlayMidi player;
        private MidiGenerator generator;

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
            allInstruments = 
                Resources.LoadAll<MIDIInstrumentSO>(
                    "ScriptableObjects/MIDI Instruments").ToList();
            percussionInstruments = 
                allInstruments.OfType<MIDIPercussionInstrumentSO>().ToList();
            melodicInstruments = 
                allInstruments.Where(i => !(i is MIDIPercussionInstrumentSO)).ToList();

            // Patterns
            allDrumPatterns = 
                Resources.LoadAll<DrumPatternData>(
                    "ScriptableObjects/Patterns/Drums").ToList();
            allChordPatterns = 
                Resources.LoadAll<ChordProgressionData>(
                    "ScriptableObjects/Patterns/Chords").ToList();
            allMelodyPatterns = 
                Resources.LoadAll<MelodyPatternData>(
                    "ScriptableObjects/Patterns/Melodies").ToList();

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

        // Simple subscriber lists (scene systems register/unregister)
        private readonly List<IMidiNoteListener> _noteSubs = new();
        private readonly List<IChordListener> _chordSubs = new();
        private readonly List<IBeatSyncVFX> _beatSubs = new();

        // Beat detection (very simple: kick = beat)
        [SerializeField] private int drumChannel = 9; // MIDI ch 10 (0-based = 9)
        [SerializeField] private int[] kickNotes = new[] { 35, 36 }; // Acoustic/Bass drum
        private int _beatIndex = 0;

        public void Register(IMidiNoteListener l) 
        { 
            if (l != null && !_noteSubs.Contains(l)) 
                _noteSubs.Add(l); 
        }

        public void Unregister(IMidiNoteListener l) 
        { 
            _noteSubs.Remove(l); 
        }

        public void Register(IChordListener l) 
        { 
            if (l != null && !_chordSubs.Contains(l)) 
                _chordSubs.Add(l); 
        }

        public void Unregister(IChordListener l) 
        { 
            _chordSubs.Remove(l); 
        }

        public void Register(IBeatSyncVFX l) 
        { 
            if (l != null && !_beatSubs.Contains(l)) 
                _beatSubs.Add(l); 
        }

        public void Unregister(IBeatSyncVFX l) { _beatSubs.Remove(l); }

        public void SetChannelOwners(List<string> owners)  // index = channel, value = musicianId
        {
            _channelOwners.Clear();
            if (owners != null) _channelOwners.AddRange(owners);
        }

        public void RegisterMusicianAnchor(string musicianId, Transform anchor)
        {
            if (string.IsNullOrEmpty(musicianId) || anchor == null) return;
            _musicianAnchors[musicianId] = anchor;
        }

        private readonly Dictionary<string, Dictionary<int, string>> trackOwnersByKey = 
            new(); // cacheKey -> (trackIndex -> musicianId)
        private string _currentKey;

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

            player.OnMidiEvents += HandleMidiEvents;
            player.OnSongStarted += () => _beatIndex = 0;
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

        public void GenerateSongs(
            IEnumerable<SongData> songs, IList<Characters.Band.MusicianBase> bandOverride)
        {
            EnsureRegistriesLoaded();
            var band = bandOverride ?? GameManager.PersistentGameplayData.MusicianList;

            foreach (var s in songs)
            {
                var key = CacheKey(s, band);
                if (cache.ContainsKey(key)) continue;

                var entry = GenerateSongEntry(s, band);
                if (entry != null) cache[key] = entry;
            }
        }

        public float Play(SongData song)
        {
            var (key, entry) = EnsureInCache(song, null);
            if (entry == null) return 0f;
            return PlayBytes(key, entry.data, entry.seconds, $"'{song.SongTitle}'");
        }

        public float Play(SongData song, IList<Characters.Band.MusicianBase> bandOverride)
        {
            var (key, entry) = EnsureInCache(song, bandOverride);
            if (entry == null) return 0f;

            var label = $"'{song.SongTitle}' " +
                $"band[{(bandOverride?.Count ?? GameManager.PersistentGameplayData.MusicianList.Count)}]";
            return PlayBytes(key, entry.data, entry.seconds, label);
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

        public IEnumerator DebugOverlayNotesForLoop(
            SongData song,
            IReadOnlyList<string> entranceOrderIds,
            int takeCount,
            Dictionary<string, Transform> anchorById)
        {
            EnsureRegistriesLoaded();

            var fullBand = GameManager.PersistentGameplayData.MusicianList;
            var key = CacheKey(song, fullBand);

            if (!cache.TryGetValue(key, out var full))
            {
                full = GenerateSongEntry(song, fullBand);
                if (full == null) yield break;
                cache[key] = full;
            }

            // Allowed channel set for this loop
            if (!channelOwnersByKey.TryGetValue(key, out var owners)) yield break;
            var allowed = new HashSet<int>();
            for (int i = 0; i < takeCount && i < entranceOrderIds.Count; i++)
            {
                var id = entranceOrderIds[i];
                int ch = owners.IndexOf(id);
                if (ch >= 0) allowed.Add(ch);
            }
            if (allowed.Count == 0) yield break;

            // Parse MIDI into time-sorted NoteOn events
            var events = new List<(double t, int ch, int note, int vel)>();
            MidiFile midi;
            using (var ms = new MemoryStream(full.data)) midi = MidiFile.Read(ms);
            var tempoMap = midi.GetTempoMap();

            foreach (var chunk in midi.GetTrackChunks())
            {
                foreach (var e in chunk.Events)
                {
                    if (e is ChannelEvent ce && ce is NoteOnEvent on && on.Velocity > 0)
                    {
                        var metric = TimeConverter.ConvertTo<MetricTimeSpan>(e.DeltaTime, tempoMap);
                        // IMPORTANT: e.DeltaTime is relative to previous event in the chunk.
                        // Better: use TimedEvents to get absolute times:
                    }
                }
            }

            // Proper absolute times:
            events.Clear();
            foreach (var chunk in midi.GetTrackChunks())
            {
                var timed = chunk.GetTimedEvents();
                foreach (var te in timed)
                {
                    if (te.Event is NoteOnEvent on && on.Velocity > 0)
                    {
                        var sec = TimeConverter.ConvertTo<MetricTimeSpan>(te.Time, tempoMap).TotalSeconds;
                        int ch = (int)((ChannelEvent)te.Event).Channel;
                        int note = on.NoteNumber;
                        int vel = on.Velocity;
                        if (allowed.Contains(ch))
                            events.Add((sec, ch, note, vel));
                    }
                }
            }
            events.Sort((a, b) => a.t.CompareTo(b.t));

            // Fire texts at the right musician (channel -> musicianId -> Transform)
            foreach (var ev in events)
            {
                // Find musician id for this channel
                string id = (owners != null && ev.ch < owners.Count) ? owners[ev.ch] : null;
                if (id != null && anchorById.TryGetValue(id, out var t))
                {
                    string label = MidiNoteToName(ev.note); // "C4", etc.
                    FxManager.Instance?.SpawnFloatingText(t, label, 0, 1);
                }
                yield return new WaitForSeconds((float)(ev.t - (events.Count > 0 ? events[0].t : 0)));
            }
        }

        public IEnumerator WaitForEnd()
        {
            if (player == null) yield break;
            yield return player.WaitForEnd();
        }

        #endregion

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

        private string CacheKey(
            SongData song, IList<Characters.Band.MusicianBase> band = null)
        {
            var b = band ?? GameManager.PersistentGameplayData.MusicianList;
            var sig = ComputeBandSignature(b);
            return string.IsNullOrEmpty(sig) ? song.Id : $"{song.Id}::{sig}";
        }

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
                        var ch = (int)ce.Channel;
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

                    // Per-drummer BEAT: kick notes become beat events for that drummer
                    if (ch == drumChannel && kickNotes.Contains(n.Value))
                    {
                        var beat = new BeatEvent
                        {
                            beatIndex = _beatIndex++,
                            time = tagged.time,
                            sourceMusicianId = musId,
                            anchor = anchor
                        };
                        foreach (var b in _beatSubs) b?.OnBeat(beat);
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
            player.Play(data);
            _currentKey = key;

            if (logDebug)
                Debug.Log($"{DebugTag} Play {label} key:{key} " +
                    $"bytes:{data?.Length} dur:{seconds:0.00}s " +
                    $"IsPlaying:{player.IsPlaying}");

            return seconds;
        }
        #endregion
    }
}
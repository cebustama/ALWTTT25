using ALWTTT.Data;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MidiGenPlay;
using System;
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
        [SerializeField] private MonoBehaviour midiPlayerAdapter; // IPlayMidi (MPTK)

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

            player = midiPlayerAdapter as IPlayMidi;
            if (player == null)
                Debug.LogError($"{nameof(midiPlayerAdapter)} must implement IPlayMidi.");

            generator = new MidiGenerator();

            EnsureRegistriesLoaded();
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

        public void GenerateSongs(IEnumerable<SongData> songs, IList<ALWTTT.Characters.Band.MusicianBase> bandOverride)
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

        /// <summary>
        /// Play the given song. Returns the real duration (seconds) measured from the MIDI.
        /// If missing from cache, it will be generated on-demand.
        /// </summary>
        public float Play(SongData song)
        {
            EnsureRegistriesLoaded();

            var band = GameManager.PersistentGameplayData.MusicianList;
            var key = CacheKey(song, band);

            if (!cache.TryGetValue(key, out var entry))
            {
                entry = GenerateSongEntry(song);
                if (entry == null) return 0f;
                cache[key] = entry;
            }

            player.Stop();
            player.Play(entry.data);

            if (logDebug)
                Debug.Log($"{DebugTag} Playing '{song.SongTitle}' ({entry.seconds:0.00}s)");

            return entry.seconds;
        }

        public float Play(SongData song, IList<Characters.Band.MusicianBase> bandOverride)
        {
            EnsureRegistriesLoaded();

            var band = bandOverride ?? GameManager.PersistentGameplayData.MusicianList;
            var key = CacheKey(song, band);

            if (!cache.TryGetValue(key, out var entry))
            {
                entry = GenerateSongEntry(song, band);
                if (entry == null) return 0f;
                cache[key] = entry;
            }

            player.Stop();
            player.Play(entry.data);

            if (logDebug)
                Debug.Log($"{DebugTag} Playing '{song.SongTitle}' for band[{band.Count}] ({entry.seconds:0.00}s)");

            return entry.seconds;
        }

        /// <summary>
        /// Play the same full-band arrangement but only keep the first 'channelsToKeep'
        /// MIDI channels (by ascending channel index). Returns duration seconds.
        /// </summary>
        public float PlaySameArrangementSubset(SongData song, int channelsToKeep)
        {
            EnsureRegistriesLoaded();

            // Ensure full-band is in cache
            var fullBand = GameManager.PersistentGameplayData.MusicianList;
            var fullKey = CacheKey(song, fullBand);

            if (!cache.TryGetValue(fullKey, out var full))
            {
                full = GenerateSongEntry(song, fullBand);
                if (full == null) return 0f;
                cache[fullKey] = full;
            }

            // Discover which channels exist in this generated performance
            MidiFile midiFull;
            using (var ms = new MemoryStream(full.data))
                midiFull = MidiFile.Read(ms);

            var used = GetUsedChannels(midiFull);
            if (used.Count == 0) return Play(song);   // fallback

            channelsToKeep = Mathf.Clamp(channelsToKeep, 1, used.Count);
            var allowed = new HashSet<int>(used.Take(channelsToKeep));

            // Build a masked variant deterministically from the same full bytes
            var maskedData = BuildChannelMaskedData(full.data, allowed);

            player.Stop();
            player.Play(maskedData);

            // Same duration as the full file
            if (logDebug)
                Debug.Log($"{DebugTag} PlaySameArrangementSubset '{song.SongTitle}' channels={channelsToKeep}/{used.Count} ({full.seconds:0.00}s)");
            return full.seconds;
        }

        public void Stop()
        {
            player?.Stop();
            if (logDebug) Debug.Log($"{DebugTag} Stop");
        }
        #endregion

        private SongCacheEntry GenerateSongEntry(SongData song)
            => GenerateSongEntry(song, GameManager.PersistentGameplayData.MusicianList);

        private SongCacheEntry GenerateSongEntry(SongData song, IList<ALWTTT.Characters.Band.MusicianBase> band)
        {
            EnsureRegistriesLoaded();

            // Convert to the concrete type GenerateConfig expects
            var bandList = band as List<ALWTTT.Characters.Band.MusicianBase>
                           ?? band?.ToList();

            var config = song.GenerateConfig(bandList); // OK now
            var midi = generator.GenerateSong(config);

            byte[] data;
            using (var ms = new System.IO.MemoryStream())
            {
                midi.Write(ms);
                data = ms.ToArray();
            }

            var seconds = ComputeDurationSeconds(midi);
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
    }
}
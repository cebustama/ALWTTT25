using ALWTTT.Data;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MidiGenPlay;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            foreach (var s in songs)
            {
                var key = s.Id;
                if (cache.ContainsKey(key)) continue;

                var entry = GenerateSongEntry(s);
                if (entry != null)
                    cache[key] = entry;
            }

            if (logDebug) 
                Debug.Log($"{DebugTag} Pre-generated {cache.Count} songs in cache.");
        }

        /// <summary>
        /// Play the given song. Returns the real duration (seconds) measured from the MIDI.
        /// If missing from cache, it will be generated on-demand.
        /// </summary>
        public float Play(SongData song)
        {
            EnsureRegistriesLoaded();

            // Generate if not in cache
            var key = song.Id;
            if (!cache.TryGetValue(key, out var entry))
            {
                entry = GenerateSongEntry(song);
                if (entry == null) return 0f;
                cache[key] = entry;
            }

            player.Stop();
            player.Play(entry.data);

            if (logDebug) 
                Debug.Log($"{DebugTag} " +
                    $"Playing '{song.SongTitle}' ({entry.seconds:0.00}s)");

            return entry.seconds;
        }

        public void Stop()
        {
            player?.Stop();
            if (logDebug) Debug.Log($"{DebugTag} Stop");
        }
        #endregion

        private SongCacheEntry GenerateSongEntry(SongData song)
        {
            EnsureRegistriesLoaded();

            var config = song.GenerateConfig(GameManager.PersistentGameplayData.MusicianList);
            var midi = generator.GenerateSong(config);

            if (logDebug) Debug.Log($"{DebugTag} Finished generating song {song.SongTitle}");

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
    }
}
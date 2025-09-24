using ALWTTT.Characters.Band;
using Melanchall.DryWetMidi.MusicTheory;
using MidiGenPlay;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "New Song", menuName = "ALWTTT/Songs/SongData")]
    public class SongData : ScriptableObject
    {
        private const string DebugTag = "<color=orange>[SongData]</color>";

        [Header("Song Profile")]
        [SerializeField] private string id;
        [SerializeField] private string songTitle;
        [SerializeField] private SongGenre genre;
        [SerializeField] private LyricsTheme theme;
        [SerializeField] private ComplexityLevel complexity;
        [SerializeField] private SongPopularity popularity;
        [SerializeField] private SongStatus status;

        [Header("Audio")]
        [SerializeField] private SongTemplate genProfile;
        [SerializeField] private float duration = 5f;
        [SerializeField] private int bpm = 80;

        #region Encapsulation
        public string Id => id;
        public string SongTitle => songTitle;
        public string SongGenre => genre.ToString();
        public string SongTheme => theme.ToString();
        public string Complexity => complexity.ToString();
        public float Duration => duration;
        public int BPM => bpm;
        public SongTemplate GenProfile => genProfile;
        #endregion

        #region Public Methods
        public string GetDropdownText()
        {
            return $"{songTitle} ({GetThemeColorText()})";
        }

        public int GetSongBaseVibe()
        {
            // TODO: Other factors
            return GetPopularityVibe();
        }

        // TODO: Think this through
        public int GetPopularityVibe()
        {
            switch (popularity)
            {
                case SongPopularity.Unknown:
                    return 1;
                case SongPopularity.Familiar:
                    return 5;
                case SongPopularity.Famous:
                    return 15;
                default:
                    return 0;
            }
        }
        #endregion

        #region Midi
        public SongConfig GenerateConfig(IReadOnlyList<MusicianBase> musicians)
        {
            if (genProfile == null || genProfile.parts == null || genProfile.parts.Count == 0)
            {
                Debug.LogWarning($"[SongData] '{name}' has no SongTemplate parts.");
                return new SongConfig
                {
                    Parts = new List<SongConfig.PartConfig>(),
                    Structure = new List<SongConfig.PartSequenceEntry>()
                };
            }

            Debug.Log($"{DebugTag} Generating SongConfig for {SongTitle}");

            var config = new SongConfig
            {
                Parts = new List<SongConfig.PartConfig>(),
                Structure = new List<SongConfig.PartSequenceEntry>()
            };

            // Deterministic RNG
            int seed = genProfile.seed != 0 ? genProfile.seed :
                       (!string.IsNullOrEmpty(id) ? id.GetHashCode() :
                       (!string.IsNullOrEmpty(songTitle) ? songTitle.GetHashCode() : 58008));
            var rnd = new System.Random(seed);

            // Registries
            var midi = ALWTTT.Managers.MidiMusicManager.Instance;
            var melodicInstruments = midi.MelodicInstruments;
            var percInstruments = midi.PercussionInstruments;
            var allDrumPatterns = midi.DrumPatterns;
            var allChordPatterns = midi.ChordPatterns;
            var allMelodyPatterns = midi.MelodyPatterns;

            // Use interface type throughout
            IReadOnlyList<MusicianBase> band = musicians ?? (IReadOnlyList<MusicianBase>)new List<MusicianBase>();
            var resolvedRoles = ResolveRoles(band, rnd); // (m, role)

            // Helper to pick a melodic instrument by type
            MIDIInstrumentSO PickInstrumentByType(List<InstrumentType> wanted, System.Random r)
            {
                if (wanted == null || wanted.Count == 0 || melodicInstruments.Count == 0) return null;

                var candidates = new List<MIDIInstrumentSO>();
                foreach (var so in melodicInstruments)
                {
                    var it = so.InstrumentType;
                    if (wanted.Contains(it)) candidates.Add(so);
                }
                if (candidates.Count == 0) return null;
                return candidates[r.Next(candidates.Count)];
            }

            // Build parts and tracks
            for (int i = 0; i < genProfile.parts.Count; i++)
            {
                var pt = genProfile.parts[i];

                var drumPatterns = allDrumPatterns.Where(p => p.timeSignature == pt.timeSignature).ToList();
                var chordPatterns = allChordPatterns.Where(p => p.timeSignature == pt.timeSignature).ToList();
                var melodyPatterns = allMelodyPatterns.Where(p => p.timeSignature == pt.timeSignature).ToList();

                var part = new SongConfig.PartConfig
                {
                    Name = string.IsNullOrEmpty(pt.name) ? $"Part {i + 1}" : pt.name,
                    Tonality = pt.tonality,
                    RootNote = pt.rootNote,
                    TempoRange = pt.tempoRange,
                    TimeSignature = pt.timeSignature,
                    Measures = Mathf.Max(1, pt.measures),
                    Tracks = new List<SongConfig.PartConfig.TrackConfig>()
                };

                foreach (var (m, role) in resolvedRoles)
                {
                    var prof = m?.MusicianCharacterData?.Profile;

                    if (role == TrackRole.Rhythm)
                    {
                        var drumKit = percInstruments.Count > 0 ? percInstruments[rnd.Next(percInstruments.Count)] : null;
                        var drumPat = drumPatterns.Count > 0 ? (PatternDataSO)drumPatterns[rnd.Next(drumPatterns.Count)] : null;

                        part.Tracks.Add(new SongConfig.PartConfig.TrackConfig
                        {
                            Role = TrackRole.Rhythm,
                            PercussionInstrument = drumKit,
                            Instrument = null,
                            Parameters = new TrackParameters { Pattern = drumPat }
                        });
                    }
                    else
                    {
                        MIDIInstrumentSO pickedInstrument = null;
                        PatternDataSO pickedPattern = null;

                        if (role == TrackRole.Lead)
                        {
                            var leadTypes = prof?.leadInstruments ?? new List<InstrumentType>();
                            pickedInstrument = PickInstrumentByType(leadTypes, rnd);
                            pickedPattern = melodyPatterns.Count > 0 ? (PatternDataSO)melodyPatterns[rnd.Next(melodyPatterns.Count)] : null;
                        }
                        else // Backing
                        {
                            var backingTypes = prof?.backingInstruments ?? new List<InstrumentType>();
                            if (backingTypes == null || backingTypes.Count == 0)
                                backingTypes = prof?.leadInstruments ?? new List<InstrumentType>();

                            pickedInstrument = PickInstrumentByType(backingTypes, rnd);
                            pickedPattern = chordPatterns.Count > 0 ? (PatternDataSO)chordPatterns[rnd.Next(chordPatterns.Count)] : null;
                        }

                        part.Tracks.Add(new SongConfig.PartConfig.TrackConfig
                        {
                            Role = role,
                            Instrument = pickedInstrument,
                            PercussionInstrument = null,
                            Parameters = new TrackParameters { Pattern = pickedPattern }
                        });
                    }
                }

                config.Parts.Add(part);
            }

            // Structure (1-based -> 0-based)
            if (genProfile.structure == null || genProfile.structure.Count == 0)
            {
                config.Structure.Add(new SongConfig.PartSequenceEntry { PartIndex = 0, RepeatCount = 1 });
            }
            else
            {
                foreach (var oneBased in genProfile.structure)
                {
                    int idx = Mathf.Clamp(oneBased - 1, 0, Mathf.Max(0, config.Parts.Count - 1));
                    config.Structure.Add(new SongConfig.PartSequenceEntry { PartIndex = idx, RepeatCount = 1 });
                }
            }

            return config;
        }

        private List<(MusicianBase m, TrackRole role)> ResolveRoles(IReadOnlyList<MusicianBase> band, System.Random rnd)
        {
            var roles = new List<(MusicianBase, TrackRole)>();
            if (band == null || band.Count == 0) return roles;

            bool IsPercType(InstrumentType t) => t == InstrumentType.Drums;

            bool IsDrummer(MusicianBase m)
            {
                var prof = m?.MusicianCharacterData?.Profile;
                if (prof == null) return false;
                bool any(List<InstrumentType> list) => list != null && list.Any(IsPercType);
                return any(prof.backingInstruments) || any(prof.leadInstruments);
            }

            var drummers = band.Where(IsDrummer).ToList();

            var used = new HashSet<MusicianBase>();
            if (drummers.Count > 0)
            {
                var d = drummers[rnd.Next(drummers.Count)];
                roles.Add((d, TrackRole.Rhythm));
                used.Add(d);
                Debug.Log("Rhythm: " + d.MusicianCharacterData.CharacterName);
            }

            var remaining = band.Where(b => !used.Contains(b)).ToList();
            if (remaining.Count > 0)
            {
                var leadIdx = rnd.Next(remaining.Count);
                var lead = remaining[leadIdx];
                roles.Add((lead, TrackRole.Lead));
                used.Add(lead);
                Debug.Log("Lead: " + lead.MusicianCharacterData.CharacterName);

                foreach (var m in remaining.Where(x => !ReferenceEquals(x, lead)))
                {
                    roles.Add((m, TrackRole.Backing));
                    Debug.Log("Backing: " + m.MusicianCharacterData.CharacterName);
                }
            }

            return roles;
        }
        #endregion

        private string GetThemeColorText()
        {
            switch (theme)
            {
                case LyricsTheme.Love:
                    return "<color=red>Love</color>";
                case LyricsTheme.Injustice:
                    return "<color=purple>Injustice</color>";
                case LyricsTheme.Partying:
                    return "<color=blue>Partying</color>";
            }

            return "Theme Color Not Found.";
        }
    }

    [Serializable]
    public class SongTemplate
    {
        public int seed = 0;

        public List<PartTemplate> parts = new List<PartTemplate>();
        public List<int> structure = new List<int> { 1, 1, 2, 1 };

        [Serializable]
        public class PartTemplate
        {
            [Header("Musical Setup")]
            public string name = "Part A";
            public Tonality tonality = Tonality.Ionian;
            public NoteName rootNote = NoteName.C;
            public TempoRange tempoRange = TempoRange.Fast;
            public TimeSignature timeSignature = TimeSignature.FourFour;
            public int measures = 4;
        }
    }

    #region Enums
    public enum SongGenre
    {
        Rock,
        Pop,
        Jazz
    }

    public enum LyricsTheme
    {
        Love,
        Injustice,
        Partying
    }

    public enum ComplexityLevel
    {
        NotComplex,
        Complex,
        VeryComplex
    }

    public enum SongStatus
    {
        New,
        Rehearsed,
        NotRehearsed,
        Forgotten
    }

    public enum SongPopularity
    {
        Unknown,
        Familiar,
        Famous
    }
    #endregion
}
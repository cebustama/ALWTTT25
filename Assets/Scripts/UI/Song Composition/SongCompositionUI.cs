using ALWTTT.Characters.Band;
using MidiGenPlay.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ALWTTT.CardData;
using static UnityEngine.EventSystems.EventTrigger;

namespace ALWTTT.UI
{
    public class SongCompositionUI : MonoBehaviour
    {
        private const string DebugTag = "<color=green>[SongCompositionUI]</color>:";

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI songTitleText;
        [SerializeField] private TextMeshProUGUI songThemeText;

        [Header("Layouts")]
        [SerializeField] private SongPartsLayoutUI partsLayout;

        [Header("Prefabs")]
        [SerializeField] private GameObject partElementPrefab;
        [SerializeField] private Image musicianIconTemplateFromScene;      // optional override

        [Header("Visual Defaults")]
        [SerializeField] private string defaultSongTitle = "Untitled Jam";
        [SerializeField] private string defaultTheme = "Instrumental";
        [SerializeField] private string defaultPartLabel = "Part A";

        [Header("Dev")]
        [SerializeField] private bool useLogs = false;

        [Serializable]
        public class TrackEntry
        {
            public string musicianId;
            public string role;        // Rhythm / Backing / Bassline / Melody / Harmony
            public string info;        // “Funk Groove”, “Pentatonic”, card name, etc

            // per-track style overrides captured from the card or musician
            public bool hasMelodyStrategyOverride;
            public MelodyStrategyId melodyStrategyIdOverride;
            public bool hasMelodicLeadingOverride;
            public MelodicLeadingConfig melodicLeadingOverride;

            public bool hasHarmonyStrategyOverride;
            public HarmonyStrategyId harmonyStrategyIdOverride;
            public bool hasHarmonicLeadingOverride;
            public HarmonicLeadingConfig harmonicLeadingOverride;
        }

        [Serializable]
        public class PartEntry
        {
            public string label = "Part";
            public string timeSignature = "4/4";
            public string tempo = "Very Fast";
            public string tonality = "Ionian";
            public int measures = 8;
            public List<TrackEntry> tracks = new();
        }

        [Serializable]
        public class SongModel
        {
            public string title = "";
            public string theme = "";
            public readonly List<PartEntry> parts = new();

            public int CurrentPartIndex => 
                Mathf.Clamp(parts.Count - 1, 0, Mathf.Max(parts.Count - 1, 0));

            public PartEntry CurrentPart => 
                parts.Count == 0 ? null : parts[CurrentPartIndex];
        }

        private SongModel model = new();
        private readonly List<SongPartElementUI> partUIs = new();
        private List<string> rosterOrder = new();
        private readonly Dictionary<string, Image> iconById = new();

        #region Encapsulation
        public SongModel Model => model;
        public event Action<SongModel> OnChanged; // any change
        public event Action<PartEntry> OnPartChanged; // current part changes
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (songTitleText) songTitleText.text = defaultSongTitle;
            if (songThemeText) songThemeText.text = $"Theme: {defaultTheme}";
        }
        #endregion

        #region Public API
        public void ResetSession()
        {
            model = new SongModel
            {
                title = defaultSongTitle,
                theme = defaultTheme
            };
            RedrawAll();
            RaiseChanged();
        }

        public void SetTheme(string theme)
        {
            model.theme = string.IsNullOrWhiteSpace(theme) ? defaultTheme : theme;
            if (songThemeText) songThemeText.text = $"Theme: {model.theme}";
            RaiseChanged();
        }

        /// <summary>
        /// Apply a Composition card to the current song model and update UI.
        /// Returns true if the card was applied.
        /// </summary>
        public bool ApplyCard(CardBase card, MusicianBase target)
        {
            if (card == null || card.CardData == null) return false;
            var data = card.CardData;
            if (!data.IsComposition) return false;

            string tgtId = target != null ? 
                target.MusicianCharacterData.CharacterId : null;
            string tgtName = target != null ? 
                target.MusicianCharacterData.CharacterName : null;

            switch (data.CompositionType)
            {
                // ---------- Theme ----------
                case CompositionCardType.Theme_Love: SetTheme("Love"); return true;
                case CompositionCardType.Theme_Injustice: SetTheme("Injustice"); return true;
                case CompositionCardType.Theme_Party: SetTheme("Party"); return true;

                // ---------- Time Signature ----------
                case CompositionCardType.TimeSignature_4_4: return SetTimeSignature("4/4");
                case CompositionCardType.TimeSignature_3_4: return SetTimeSignature("3/4");
                case CompositionCardType.TimeSignature_6_8: return SetTimeSignature("6/8");
                case CompositionCardType.TimeSignature_5_4: return SetTimeSignature("5/4");

                // ---------- Tempo ----------
                case CompositionCardType.Tempo_Slow: return SetTempo("Slow");
                case CompositionCardType.Tempo_Fast: return SetTempo("Fast");
                case CompositionCardType.Tempo_VeryFast: return SetTempo("Very Fast");

                // ---------- Tracks ----------
                case CompositionCardType.Track_Rhythm: 
                    return TryAddTrack(tgtId, tgtName, "Rhythm", data.CardName, data);
                case CompositionCardType.Track_Backing: 
                    return TryAddTrack(tgtId, tgtName, "Backing", data.CardName, data);
                case CompositionCardType.Track_Bassline: 
                    return TryAddTrack(tgtId, tgtName, "Bassline", data.CardName, data);
                case CompositionCardType.Track_Melody: 
                    return TryAddTrack(tgtId, tgtName, "Melody", data.CardName, data);
                case CompositionCardType.Track_Harmony: 
                    return TryAddTrack(tgtId, tgtName, "Harmony", data.CardName, data);

                // ---------- Parts ----------
                case CompositionCardType.Part_Intro: return TryAddIntro(tgtId, tgtName);
                case CompositionCardType.Part_Solo: return TryAddSolo(tgtId, tgtName);
                case CompositionCardType.Part_Outro: return TryAddOutro(tgtId, tgtName);

                // ---------- Tonality ----------
                case CompositionCardType.Tonality_Ionian:       return SetTonality("Ionian");
                case CompositionCardType.Tonality_Dorian:       return SetTonality("Dorian");
                case CompositionCardType.Tonality_Phrygian:     return SetTonality("Phrygian");
                case CompositionCardType.Tonality_Lydian:       return SetTonality("Lydian");
                case CompositionCardType.Tonality_Mixolydian:   return SetTonality("Mixolydian");
                case CompositionCardType.Tonality_Aeolian:      return SetTonality("Aeolian");
                case CompositionCardType.Tonality_Locrian:      return SetTonality("Locrian");
            }

            return false;
        }

        public void PopulateMusicianIcons(IEnumerable<MusicianBase> band)
        {
            if (partsLayout == null) return;
            var root = partsLayout.MusicianIconsRoot;
            var template = musicianIconTemplateFromScene != null
                ? musicianIconTemplateFromScene
                : partsLayout.MusicianIconTemplate;
            if (root == null || template == null) return;

            // Clear old rows but keep the hidden template
            foreach (Transform c in root)
                if (c != template.transform) Destroy(c.gameObject);

            rosterOrder.Clear();
            iconById.Clear();

            if (band == null) return;

            foreach (var m in band)
            {
                if (m == null || m.MusicianCharacterData == null) continue;

                var id = m.MusicianCharacterData.CharacterId;
                var icon = m.MusicianCharacterData.CharacterIcon;

                var row = Instantiate(template, root);
                row.sprite = icon;

                // Start hidden; we will enable per current-part tracks
                row.gameObject.SetActive(false);

                rosterOrder.Add(id);
                iconById[id] = row;
            }

            // Give order to all existing parts
            foreach (var p in partUIs) p.SetRosterOrder(rosterOrder);

            // Sync icons with the CURRENT part (likely empty → all hidden)
            UpdateIconsForCurrentPart();
        }
        #endregion

        #region Rules
        private bool EnsureFirstPart()
        {
            if (model.parts.Count > 0) return true;

            var p = new PartEntry
            {
                label = defaultPartLabel,
                timeSignature = "4/4",
                tempo = "Very Fast"
            };
            model.parts.Add(p);
            AddPartUI(p);
            return true;
        }

        private bool SetTimeSignature(string ts)
        {
            Log($"Setting Time Signature {ts}");

            if (!EnsureFirstPart()) return false;
            var part = model.CurrentPart;
            part.timeSignature = ts;
            partUIs[model.CurrentPartIndex].Bind(part);
            RaisePartChanged();
            return true;
        }

        private bool SetTempo(string label)
        {
            Log($"Setting Tempo {label}");

            if (!EnsureFirstPart()) return false;
            var part = model.CurrentPart;
            part.tempo = label;
            partUIs[model.CurrentPartIndex].Bind(part);
            RaisePartChanged();
            return true;
        }

        private bool SetTonality(string tonality)
        {
            Log($"Setting Tonality {tonality}");

            if (!EnsureFirstPart()) return false;
            var part = model.CurrentPart;
            part.tonality = tonality;
            partUIs[model.CurrentPartIndex].Bind(part);
            RaisePartChanged();
            return true;
        }

        private bool TryAddTrack(
            string musicianId, string musicianName, 
            string role, string info, CardData sourceCard)
        {
            if (string.IsNullOrEmpty(musicianId)) return false;

            Log($"Adding Track id:{musicianId} role:{role} info:{info}");

            // Rule: If no parts → create first part then add track
            EnsureFirstPart();
            var part = model.CurrentPart;

            // Rule: If musician already has a track in this part → deny (for now)
            if (part.tracks.Any(t => t.musicianId == musicianId))
                return false;

            var entry = new TrackEntry
            {
                musicianId = musicianId,
                role = role,
                info = info,

                hasMelodyStrategyOverride =
                    sourceCard != null && sourceCard.OverrideMelodyStrategy,
                hasMelodicLeadingOverride =
                    sourceCard != null && sourceCard.OverrideMelodicLeading,
                hasHarmonyStrategyOverride =
                    sourceCard != null && sourceCard.OverrideHarmonyStrategy,
                hasHarmonicLeadingOverride =
                    sourceCard != null && sourceCard.OverrideHarmonicLeading,

            };

            // Only copy specific fields if they were flagged
            // TODO: Better way to handle these fields? Before adding more for each track role
            if (entry.hasMelodyStrategyOverride)
            {
                entry.melodyStrategyIdOverride = sourceCard.MelodyStrategyIdOverride;
            }
            if (entry.hasMelodicLeadingOverride)
            {
                entry.melodicLeadingOverride = sourceCard.MelodicLeadingOverride;
            }
            if (entry.hasHarmonyStrategyOverride)
            {
                entry.harmonyStrategyIdOverride = sourceCard.HarmonyStrategyIdOverride;
            }
            if (entry.hasHarmonicLeadingOverride)
            {
                entry.harmonicLeadingOverride = sourceCard.HarmonicLeadingOverride;
            }

            // Add new track entry
            part.tracks.Add(entry);

            // Update UI
            partUIs[model.CurrentPartIndex].AddOrUpdateTrack(musicianId, role, info);

            UpdateIconsForCurrentPart();
            RaisePartChanged();
            return true;
        }

        // Rule: Part cards (intro/solo/outro) require an existing part first
        private bool AnyPartExists() => model.parts.Count > 0;

        private bool TryAddIntro(string musicianId, string musicianName)
        {
            if (!AnyPartExists()) return false;

            var intro = new PartEntry
            {
                label = "Intro",
                timeSignature = model.parts[0].timeSignature,
                tempo = model.parts[0].tempo
            };

            // Single track for intro (musician who played it)
            if (!string.IsNullOrEmpty(musicianId))
            {
                intro.tracks.Add(new TrackEntry
                {
                    musicianId = musicianId,
                    role = "Intro",
                    info = "Lead-in"
                });
            }

            // Insert at the beginning
            model.parts.Insert(0, intro);
            RedrawAll();
            UpdateIconsForCurrentPart();
            RaisePartChanged();
            return true;
        }

        private bool TryAddOutro(string musicianId, string musicianName)
        {
            if (!AnyPartExists()) return false;

            var outro = new PartEntry
            {
                label = "Outro",
                timeSignature = model.parts.Last().timeSignature,
                tempo = model.parts.Last().tempo
            };
            if (!string.IsNullOrEmpty(musicianId))
            {
                outro.tracks.Add(new TrackEntry
                {
                    musicianId = musicianId,
                    role = "Outro",
                    info = "Finale"
                });
            }

            // Append at the end
            model.parts.Add(outro);
            AddPartUI(outro);
            UpdateIconsForCurrentPart();
            RaisePartChanged();
            return true;
        }

        private bool TryAddSolo(string musicianId, string musicianName)
        {
            if (!AnyPartExists()) return false;

            // Clone the current part
            var basePart = model.CurrentPart;
            var solo = new PartEntry
            {
                label = "Solo",
                timeSignature = basePart.timeSignature,
                tempo = basePart.tempo,
                tracks = basePart.tracks.Select(t => new TrackEntry
                {
                    musicianId = t.musicianId,
                    role = t.role,
                    info = t.info
                }).ToList()
            };

            // Replace (or insert) the selected musician track with a "Solo" entry
            if (!string.IsNullOrEmpty(musicianId))
            {
                var existing = solo.tracks.FirstOrDefault(t => t.musicianId == musicianId);
                if (existing != null)
                {
                    existing.role = "Solo";
                    existing.info = "Improvisation";
                }
                else
                {
                    solo.tracks.Add(new TrackEntry
                    {
                        musicianId = musicianId,
                        role = "Solo",
                        info = "Improvisation"
                    });
                }
            }

            // Insert after the current part
            int insertAt = model.CurrentPartIndex + 1;
            model.parts.Insert(insertAt, solo);
            RedrawAll();
            UpdateIconsForCurrentPart();
            RaisePartChanged();
            return true;
        }

        public bool CanApply(CardBase card, MusicianBase target, out string reason)
        {
            reason = null;
            if (card == null || card.CardData == null || !card.CardData.IsComposition)
            { reason = "Not a composition card."; return false; }

            var type = card.CardData.CompositionType;
            bool isTrack = type == CompositionCardType.Track_Rhythm
                        || type == CompositionCardType.Track_Backing
                        || type == CompositionCardType.Track_Bassline
                        || type == CompositionCardType.Track_Melody
                        || type == CompositionCardType.Track_Harmony;

            if (isTrack && target == null)
            { reason = "Select a musician."; return false; }

            // Enforce “one track per musician per part” MVP rule
            if (isTrack && model.CurrentPart != null &&
                model.CurrentPart.tracks.Any(t => t.musicianId == target.MusicianCharacterData.CharacterId))
            { reason = "This musician already has a track in this part."; return false; }

            // Part cards require an existing part
            bool isPartCard = type == CompositionCardType.Part_Intro
                           || type == CompositionCardType.Part_Solo
                           || type == CompositionCardType.Part_Outro;
            if (isPartCard && model.parts.Count == 0)
            { reason = "Create a part first (play any Track/Tempo/TimeSig card)."; return false; }

            return true;
        }
        #endregion

        #region Rendering
        private void RedrawAll()
        {
            // song title + theme
            if (songTitleText) songTitleText.text = model.title;
            if (songThemeText) songThemeText.text = $"Theme: {model.theme}";

            // clear parts UI
            foreach (var p in partUIs) if (p) Destroy(p.gameObject);
            partUIs.Clear();

            if (partsLayout == null || partElementPrefab == null) return;

            for (int i = 0; i < model.parts.Count; i++)
                AddPartUI(model.parts[i]);

            UpdateIconsForCurrentPart();
        }

        private void AddPartUI(PartEntry p)
        {
            if (partsLayout == null || partElementPrefab == null) return;

            var element = Instantiate(partElementPrefab, partsLayout.ContentRoot);
            var ui = element.GetComponent<SongPartElementUI>();
            if (ui == null) return;

            ui.SetRosterOrder(rosterOrder);  // << important
            ui.Bind(p);
            partUIs.Add(ui);
        }
        #endregion

        #region Helpers
        private void UpdateIconsForCurrentPart()
        {
            if (iconById.Count == 0) return;

            var part = model.CurrentPart;
            if (part == null)
            {
                // no parts -> hide all
                foreach (var kv in iconById) kv.Value.gameObject.SetActive(false);
                return;
            }

            // Which musicians have tracks in this part?
            var activeIds = new HashSet<string>(part.tracks.Select(t => t.musicianId));

            foreach (var kv in iconById)
                kv.Value.gameObject.SetActive(activeIds.Contains(kv.Key));
        }

        private void RaiseChanged() => OnChanged?.Invoke(model);
        private void RaisePartChanged()
        {
            var p = model.CurrentPart;
            if (p != null) OnPartChanged?.Invoke(p);
        }
        #endregion

        #region Debug
        private void Log(string log)
        {
            if (useLogs) Debug.Log($"{DebugTag} {log}");
        }
        #endregion
    }
}
using ALWTTT.Cards;
using ALWTTT.Characters.Band;
using MidiGenPlay.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ALWTTT.Cards.CardData;
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

        [Header("Inspiration")]
        [SerializeField] private GameObject inspirationRoot;
        [SerializeField] private TextMeshProUGUI inspirationText;
        [SerializeField] private TextMeshProUGUI plusInspirationText;

        [Header("Dev")]
        [SerializeField] private bool useLogs = false;

        #region Model Data
        [Serializable]
        public class TrackEntry
        {
            public string musicianId;
            public string role;                 // Rhythm / Backing / Melody / Harmony
            public string info;                 // “Funk Groove”, “Pentatonic”, card name, etc
            public int inspirationGenerated;    // per-loop gain contributed by this track

            public TrackStyleBundleSO styleBundle;

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
            public bool isFinal = false;
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
        #endregion

        private SongModel model = new();
        private readonly List<SongPartElementUI> partUIs = new();
        private List<string> rosterOrder = new();
        private readonly Dictionary<string, Image> iconById = new();
        private int iconReferencePartIndex = 0;

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
            return ApplyCardToPart(card, target, model.CurrentPartIndex);
        }


        public bool ApplyCardToPart(CardBase card, MusicianBase target, int partIndex)
        {
            if (card == null || card.CardData == null || !card.CardData.IsComposition)
                return false;

            // create the part if needed (fixes parts=0 on first cards)
            var part = EnsurePartAt(partIndex);
            if (part == null)
            {
                Log($"ApplyCardToPart: could not ensure part at index={partIndex}");
                return false;
            }

            var data = card.CardData;

            string tgtId = target != null ? target.MusicianCharacterData.CharacterId : null;
            string tgtName = target != null ? target.MusicianCharacterData.CharacterName : null;

            // Route by card type, but always operate on (partIndex, part)
            switch (data.CompositionType)
            {
                // Theme is global (unchanged)
                case CompositionCardType.Theme_Love: SetTheme("Love"); return true;
                case CompositionCardType.Theme_Injustice: SetTheme("Injustice"); return true;
                case CompositionCardType.Theme_Party: SetTheme("Party"); return true;

                // Structure on a specific part
                case CompositionCardType.TimeSignature_4_4: return SetTimeSignatureOnPart(part, partIndex, "4/4");
                case CompositionCardType.TimeSignature_3_4: return SetTimeSignatureOnPart(part, partIndex, "3/4");
                case CompositionCardType.TimeSignature_6_8: return SetTimeSignatureOnPart(part, partIndex, "6/8");
                case CompositionCardType.TimeSignature_5_4: return SetTimeSignatureOnPart(part, partIndex, "5/4");

                case CompositionCardType.Tempo_Slow: return SetTempoOnPart(part, partIndex, "Slow");
                case CompositionCardType.Tempo_Fast: return SetTempoOnPart(part, partIndex, "Fast");
                case CompositionCardType.Tempo_VeryFast: return SetTempoOnPart(part, partIndex, "Very Fast");

                case CompositionCardType.Tonality_Ionian: return SetTonalityOnPart(part, partIndex, "Ionian");
                case CompositionCardType.Tonality_Dorian: return SetTonalityOnPart(part, partIndex, "Dorian");
                case CompositionCardType.Tonality_Phrygian: return SetTonalityOnPart(part, partIndex, "Phrygian");
                case CompositionCardType.Tonality_Lydian: return SetTonalityOnPart(part, partIndex, "Lydian");
                case CompositionCardType.Tonality_Mixolydian: return SetTonalityOnPart(part, partIndex, "Mixolydian");
                case CompositionCardType.Tonality_Aeolian: return SetTonalityOnPart(part, partIndex, "Aeolian");
                case CompositionCardType.Tonality_Locrian: return SetTonalityOnPart(part, partIndex, "Locrian");

                // Tracks on a specific part (add or replace)
                case CompositionCardType.Track_Rhythm: return TryAddOrReplaceTrackOnPart(part, partIndex, tgtId, tgtName, "Rhythm", data.CardName, data);
                case CompositionCardType.Track_Backing: return TryAddOrReplaceTrackOnPart(part, partIndex, tgtId, tgtName, "Backing", data.CardName, data);
                case CompositionCardType.Track_Bassline: return TryAddOrReplaceTrackOnPart(part, partIndex, tgtId, tgtName, "Bassline", data.CardName, data);
                case CompositionCardType.Track_Melody: return TryAddOrReplaceTrackOnPart(part, partIndex, tgtId, tgtName, "Melody", data.CardName, data);
                case CompositionCardType.Track_Harmony: return TryAddOrReplaceTrackOnPart(part, partIndex, tgtId, tgtName, "Harmony", data.CardName, data);

                // Part cards (Intro/Solo/Outro) keep current behavior for now
                case CompositionCardType.Part_Intro: return TryAddIntro(tgtId, tgtName);
                case CompositionCardType.Part_Solo: return TryAddSolo(tgtId, tgtName);
                case CompositionCardType.Part_Outro: return TryAddOutro(tgtId, tgtName);
            }

            return false;
        }

        /// <summary>
        /// Ensure there is a part at 'partIndex'. Creates missing parts up to that index,
        /// binds UI, and returns the target part. Returns null if partIndex < 0.
        /// </summary>
        private PartEntry EnsurePartAt(int partIndex)
        {
            if (partIndex < 0) return null;

            while (model.parts.Count <= partIndex)
            {
                var label = model.parts.Count == 0 ? 
                    defaultPartLabel : 
                    $"Part {model.parts.Count + 1}";
                var p = new PartEntry
                {
                    label = label,
                    timeSignature = "4/4",
                    tempo = "Very Fast",
                    tonality = "Ionian",
                    measures = 8,
                    tracks = new List<TrackEntry>()
                };
                model.parts.Add(p);
                AddPartUI(p);
                Log($"[EnsurePartAt] Created part '{p.label}' at " +
                    $"index={model.parts.Count - 1}");
            }

            return model.parts[partIndex];
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

        public void SetInspiration(int value)
        {
            if (inspirationText != null)
                inspirationText.text = value.ToString();
        }

        public void SetPlusInspiration(int amount)
        {
            if (plusInspirationText == null) return;
            plusInspirationText.text = amount > 0 ? $"+{amount}" : string.Empty;
        }

        public int BeginDraftNextPart(string customLabel = null)
        {
            // The new part index is the current count (append to the end)
            int newIndex = model.parts.Count;

            // Inherit some aspects of the last part
            // TODO: When solo, inherit all other tracks, reduce volume
            var inherit = model.parts.Count > 0 ? model.parts[model.parts.Count - 1] : null;

            var label = !string.IsNullOrWhiteSpace(customLabel)
                ? customLabel
                : (newIndex == 0 ? defaultPartLabel : $"Part {newIndex + 1}");

            var p = new PartEntry
            {
                label = label,
                timeSignature = inherit != null ? inherit.timeSignature : "4/4",
                tempo = inherit != null ? inherit.tempo : "Very Fast",
                tonality = inherit != null ? inherit.tonality : "Ionian",
                measures = inherit != null ? inherit.measures : 8,
                tracks = new List<TrackEntry>()
            };

            model.parts.Add(p);
            AddPartUI(p);
            SetPartVisible(newIndex, false);
            UpdateIconsForCurrentPart();
            RaisePartChanged();

            Log($"[Draft] Created next draft part '{p.label}' at index={newIndex}", true);

            return newIndex;
        }

        public bool PartHasAnyTrack(int index)
        {
            if (index < 0 || index >= model.parts.Count) return false;
            return model.parts[index].tracks != null && model.parts[index].tracks.Count > 0;
        }

        public bool HasPlayableNextPart(int afterIndex)
        {
            int next = afterIndex + 1;
            return next >= 0
                && next < model.parts.Count
                && PartHasAnyTrack(next);
        }

        public bool IsPartFinal(int index)
        {
            if (index < 0 || index >= model.parts.Count) return false;
            return model.parts[index].isFinal;
        }

        public void SetPartFinal(int index, bool value)
        {
            if (index < 0 || index >= model.parts.Count) return;
            model.parts[index].isFinal = value;
            Log($"Marked part[{index}] as Final={value}");
        }

        public void SetIconReferencePartIndex(int partIndex)
        {
            iconReferencePartIndex = 
                Mathf.Clamp(partIndex, 0, Mathf.Max(0, model.parts.Count - 1));
            UpdateIconsForCurrentPart();
        }


        #endregion

        #region Rules

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

            // Part cards require an existing part
            bool isPartCard = type == CompositionCardType.Part_Intro
                           || type == CompositionCardType.Part_Solo
                           || type == CompositionCardType.Part_Outro;
            if (isPartCard && model.parts.Count == 0)
            { reason = "Create a part first (play any Track/Tempo/TimeSig card)."; return false; }

            return true;
        }

        private bool SetTimeSignatureOnPart(PartEntry part, int partIndex, string ts)
        {
            Log($"[ApplyCardToPart] TS={ts} on partIndex={partIndex}");
            if (part == null) return false;
            part.timeSignature = ts;
            partUIs[partIndex].Bind(part);
            RaisePartChanged();
            return true;
        }

        private bool SetTempoOnPart(PartEntry part, int partIndex, string label)
        {
            Log($"[ApplyCardToPart] Tempo={label} on partIndex={partIndex}");
            if (part == null) return false;
            part.tempo = label;
            partUIs[partIndex].Bind(part);
            RaisePartChanged();
            return true;
        }

        private bool SetTonalityOnPart(PartEntry part, int partIndex, string tonality)
        {
            Log($"[ApplyCardToPart] Tonality={tonality} on partIndex={partIndex}");
            if (part == null) return false;
            part.tonality = tonality;
            partUIs[partIndex].Bind(part);
            RaisePartChanged();
            return true;
        }

        private bool TryAddOrReplaceTrackOnPart(
            PartEntry part, int partIndex,
            string musicianId, string musicianName,
            string role, string info, CardData sourceCard)
        {
            if (part == null || string.IsNullOrEmpty(musicianId)) return false;

            Log($"[ApplyCardToPart] Track '{role}' for " +
                $"'{musicianName}' ({musicianId}) on partIndex={partIndex}", true);

            int beforeCount = part.tracks != null ? part.tracks.Count : 0;

            var existing = part.tracks.FirstOrDefault(t => t.musicianId == musicianId);
            if (existing != null)
            {
                // REPLACE (update metadata + overrides)
                existing.role = role;
                existing.info = info;

                // TODO: How to refactor this to avoid adding multiple variables
                // we need rhythm strategies, bassline strategies, etc
                existing.hasMelodyStrategyOverride = 
                    sourceCard != null && sourceCard.OverrideMelodyStrategy;
                existing.hasMelodicLeadingOverride = 
                    sourceCard != null && sourceCard.OverrideMelodicLeading;
                existing.hasHarmonyStrategyOverride = 
                    sourceCard != null && sourceCard.OverrideHarmonyStrategy;
                existing.hasHarmonicLeadingOverride = 
                    sourceCard != null && sourceCard.OverrideHarmonicLeading;

                if (existing.hasMelodyStrategyOverride) 
                    existing.melodyStrategyIdOverride = sourceCard.MelodyStrategyIdOverride;
                if (existing.hasMelodicLeadingOverride) 
                    existing.melodicLeadingOverride = sourceCard.MelodicLeadingOverride;
                if (existing.hasHarmonyStrategyOverride) 
                    existing.harmonyStrategyIdOverride = sourceCard.HarmonyStrategyIdOverride;
                if (existing.hasHarmonicLeadingOverride) 
                    existing.harmonicLeadingOverride = sourceCard.HarmonicLeadingOverride;

                existing.inspirationGenerated = 
                    Mathf.Max(0, sourceCard != null ? sourceCard.GrooveGenerated : 0);

                // Style budles
                if (sourceCard != null && sourceCard.IsTrackCard)
                {
                    if (role == "Melody" && sourceCard.MelodyCardConfig != null)
                        existing.styleBundle = sourceCard.MelodyCardConfig;
                }
            }
            else
            {
                // ADD (new entry)
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
                    inspirationGenerated = 
                    Mathf.Max(0, sourceCard != null ? sourceCard.GrooveGenerated : 0),
                };

                if (entry.hasMelodyStrategyOverride) 
                    entry.melodyStrategyIdOverride = sourceCard.MelodyStrategyIdOverride;
                if (entry.hasMelodicLeadingOverride) 
                    entry.melodicLeadingOverride = sourceCard.MelodicLeadingOverride;
                if (entry.hasHarmonyStrategyOverride) 
                    entry.harmonyStrategyIdOverride = sourceCard.HarmonyStrategyIdOverride;
                if (entry.hasHarmonicLeadingOverride) 
                    entry.harmonicLeadingOverride = sourceCard.HarmonicLeadingOverride;

                // Style budles
                if (sourceCard != null && sourceCard.IsTrackCard)
                {
                    if (role == "Melody" && sourceCard.MelodyCardConfig != null)
                        entry.styleBundle = sourceCard.MelodyCardConfig;
                }

                part.tracks.Add(entry);
            }

            if (beforeCount == 0)
                SetPartVisible(partIndex, true);

            // UI + icons refresh for the *indexed* part
            partUIs[partIndex].AddOrUpdateTrack(musicianId, role, info);
            UpdateIconsForCurrentPart();  // (icons follow CurrentPart; OK for MVP)
            RaisePartChanged();
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

        public void SetInspirationVisible(bool visible)
        {
            if (inspirationRoot != null)
                inspirationRoot.SetActive(visible);
        }
        #endregion

        #region Helpers
        private void UpdateIconsForCurrentPart()
        {
            if (iconById == null || iconById.Count == 0) return;
            var part = (iconReferencePartIndex >= 0 && iconReferencePartIndex < model.parts.Count)
                ? model.parts[iconReferencePartIndex]
                : null;

            // Always keep icons active; dim/hide according to the referenced part’s tracks.
            // If you really prefer hide/show, flip between SetActive(true/false) below.
            HashSet<string> activeIds = new();
            if (part != null && part.tracks != null)
                foreach (var t in part.tracks) 
                    if (!string.IsNullOrEmpty(t.musicianId)) activeIds.Add(t.musicianId);

            foreach (var kv in iconById)
            {
                var img = kv.Value;
                if (!img) continue;

                bool hasTrack = activeIds.Contains(kv.Key);
                img.gameObject.SetActive(true);               // always visible
                var cg = img.GetComponent<CanvasGroup>() ?? 
                    img.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = hasTrack ? 1f : 0.35f;             // dim if not playing in this part
            }
        }

        private void RaiseChanged() => OnChanged?.Invoke(model);
        private void RaisePartChanged()
        {
            var p = model.CurrentPart;
            if (p != null) OnPartChanged?.Invoke(p);
        }

        private void SetPartVisible(int partIndex, bool visible)
        {
            if (partIndex < 0 || partIndex >= partUIs.Count) return;
            if (partUIs[partIndex] != null)
                partUIs[partIndex].gameObject.SetActive(visible);
        }
        #endregion

        #region Debug
        private void Log(string log, bool highlight = false)
        {
            if (useLogs)
            {
                if (highlight) Debug.Log($"{DebugTag} <color=yellow>{log}</color>");
                else Debug.Log($"{DebugTag} {log}");
            }
        }
        #endregion
    }
}
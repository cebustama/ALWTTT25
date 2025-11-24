using ALWTTT.Cards;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using MidiGenPlay;
using MidiGenPlay.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ALWTTT.Cards.CardData;
using static ALWTTT.Cards.CardData.PartActionDescriptor;
using static MidiGenPlay.MusicTheory.MusicTheory;
using static UnityEngine.EventSystems.EventTrigger;

namespace ALWTTT.UI
{
    public class SongCompositionUI : MonoBehaviour
    {
        private const string DebugTag = "<color=green>[SongCompositionUI]</color>:";

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI songTitleText;
        [SerializeField] private TextMeshProUGUI songThemeText;

        [Header("Controls")]
        [SerializeField] private Button playButton;

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
            public TrackRole role;                 // Rhythm / Backing / Melody / Harmony
            public string info;                 // “Funk Groove”, “Pentatonic”, card name, etc
            public int inspirationGenerated;    // per-loop gain contributed by this track

            public CardType synergyType;

            public TrackStyleBundleSO styleBundle;

            // TODO: Maybe some sort of PartEffectBundle/Context?
            // Instrument overrides written by InstrumentEffect
            public MIDIInstrumentSO overrideMelodicInstrument;
            public MIDIPercussionInstrumentSO overridePercussionInstrument;
            // Type-level override (Bass / Guitar / etc.)
            public bool hasOverrideInstrumentType;
            public InstrumentType overrideInstrumentType;
        }

        [Serializable]
        public class PartEntry
        {
            public string label = "Part";
            public string tempo = "Very Fast";

            public int measures = 8;
            public List<TrackEntry> tracks = new();
            public bool isFinal = false;

            // Tonality
            public Tonality tonality = Tonality.Ionian;

            // Key root
            public NoteName rootNote = NoteName.C;
            public bool hasExplicitRootNote = false;

            // Time Signature
            public TimeSignature timeSignature;

            // Tempo
            public TempoRange tempoRangeOverride = TempoRange.Fast;
            public int? absoluteBpmOverride = null;
            public float tempoScale = 1f;
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

        public void HookPlayButton(Action onPlay)
        {
            if (!playButton) return;

            playButton.onClick.RemoveAllListeners();
            if (onPlay != null)
                playButton.onClick.AddListener(() => onPlay());
        }

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

        /// <summary>
        /// Applies a Composition card to a specific part in the current song,
        /// using the NEW data-driven model (PrimaryKind + TrackAction / PartAction + ModifierEffects).
        ///
        /// HOW THE METHOD WORKS:
        /// ---------------------------------------------------------
        /// 1. Validates the card and ensures a PartEntry exists at partIndex.
        /// 2. Resolves the target musician (when required).
        ///
        /// 3. PRIMARY ACTION:
        ///    - If PrimaryKind == Track:
        ///         • Adds or replaces a TrackEntry for the given musician.
        ///         • Copies all style overrides from the TrackActionDescriptor.
        ///         • Stores the StyleBundle (MelodyCardConfigSO, RhythmConfigSO, etc.).
        ///
        ///    - If PrimaryKind == Part:
        ///         • Executes structural actions (Create Part, Intro, Solo, Outro, Bridge, Final).
        ///         • Uses existing helpers: BeginDraftNextPart, TryAddIntro, TryAddSolo, etc.
        ///
        /// 4. SECONDARY EFFECTS:
        ///    - Applies all ModifierEffects (Tempo, Meter, Tonality, Feel, Density)
        ///      via ApplyEffectToModel(fx, partIndex, target).
        ///
        /// 5. UI + EVENTS:
        ///    - Rebinds the updated PartEntry to the UI widget.
        ///    - Raises 'PartChanged' events so other systems (e.g. MIDI) refresh their state.
        ///
        /// Returns:
        ///     true  -> card successfully applied
        ///     false -> invalid card, missing target, invalid part, or failed helper action
        /// </summary>
        public bool ApplyCardToPart(CardBase card, MusicianBase target, int partIndex)
        {
            // ---------------------------------------------------------
            // 0) BASIC VALIDATION
            // ---------------------------------------------------------
            if (card == null || card.CardData == null || !card.CardData.IsComposition)
                return false;

            var data = card.CardData;

            // Only the new model is supported now
            if (!data.UsesNewCompositionModel)
            {
                Log("ApplyCardToPart: legacy composition model is no longer supported.");
                return false;
            }

            // ---------------------------------------------------------
            // 1) ENSURE PART EXISTS
            // ---------------------------------------------------------
            var part = EnsurePartAt(partIndex);
            if (part == null)
            {
                Log($"ApplyCardToPart: could not ensure part at index={partIndex}");
                return false;
            }

            // ---------------------------------------------------------
            // 2) RESOLVE TARGET MUSICIAN (IF REQUIRED)
            // ---------------------------------------------------------
            string tgtId = target != null ? target.MusicianCharacterData.CharacterId : null;
            string tgtName = target != null ? target.MusicianCharacterData.CharacterName : null;

            // ---------------------------------------------------------
            // 3) PRIMARY ACTION: TRACK
            // ---------------------------------------------------------
            if (data.PrimaryKind == CardPrimaryKind.Track)
            {
                var desc = data.TrackAction;
                if (desc == null)
                {
                    Log("ApplyCardToPart: Track card has no TrackAction descriptor.");
                    return false;
                }

                // Track cards always require a musician target
                if (target == null)
                {
                    Log("Track card requires a musician target (new model).");
                    return false;
                }

                TrackRole role = desc.role;

                bool ok = TryAddOrReplaceTrackOnPart(
                    part,
                    partIndex,
                    tgtId,
                    tgtName,
                    role,
                    data.CardName,
                    data);

                if (!ok) return false;

                // Copy style overrides into the TrackEntry
                var last = part.tracks.FirstOrDefault(t => t.musicianId == tgtId);
                if (last != null)
                {
                    // Store the full style bundle for track-specific composition
                    last.styleBundle = desc.styleBundle;
                }
            }

            // ---------------------------------------------------------
            // 4) PRIMARY ACTION: PART
            // ---------------------------------------------------------
            else if (data.PrimaryKind == CardPrimaryKind.Part)
            {
                var pa = data.PartAction;
                if (pa == null)
                {
                    Log("ApplyCardToPart: Part card has no PartAction descriptor.");
                    return false;
                }

                switch (pa.action)
                {
                    case PartActionKind.CreatePart:
                        // Create a blank part at the end (optional custom label)
                        BeginDraftNextPart(
                            string.IsNullOrWhiteSpace(pa.customLabel) ? null : pa.customLabel);
                        break;

                    case PartActionKind.MarkIntro:
                        if (!TryAddIntro(pa.musicianId, null)) return false;
                        break;

                    case PartActionKind.MarkSolo:
                        if (!TryAddSolo(pa.musicianId, null)) return false;
                        break;

                    case PartActionKind.MarkOutro:
                        if (!TryAddOutro(pa.musicianId, null)) return false;
                        break;

                    case PartActionKind.MarkBridge:
                        // Create a "Bridge" part (or custom label)
                        BeginDraftNextPart(
                            string.IsNullOrWhiteSpace(pa.customLabel) ? "Bridge" : pa.customLabel);
                        break;

                    case PartActionKind.MarkFinal:
                        SetPartFinal(partIndex, true);
                        break;
                }
            }

            // ---------------------------------------------------------
            // 5) SECONDARY MODIFIER EFFECTS
            // ---------------------------------------------------------
            if (data.ModifierEffects != null)
            {
                foreach (var fx in data.ModifierEffects)
                {
                    ApplyEffectToModel(fx, partIndex, target);
                }
            }

            // ---------------------------------------------------------
            // 6) REFRESH UI + TRIGGER EVENTS
            // ---------------------------------------------------------
            if (partIndex >= 0 && partIndex < partUIs.Count && partUIs[partIndex] != null)
                partUIs[partIndex].Bind(model.parts[partIndex]);

            RaisePartChanged();
            return true;
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
                    timeSignature = TimeSignature.FourFour,
                    tempo = "Very Fast",
                    tonality = Tonality.Ionian,
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
                timeSignature = inherit != null 
                    ? inherit.timeSignature : TimeSignature.FourFour,
                tempo = inherit != null ? inherit.tempo : "Very Fast",
                tonality = inherit != null ? inherit.tonality : Tonality.Ionian,
                measures = inherit != null ? inherit.measures : 8,
                tracks = new List<TrackEntry>(),
                rootNote = inherit != null ? inherit.rootNote : NoteName.C,
                hasExplicitRootNote = inherit != null && inherit.hasExplicitRootNote
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

        public string GetPartLabel(int partIndex)
        {
            // Defensive: no model or no parts → default label
            if (model == null || model.parts == null || model.parts.Count == 0)
                return defaultPartLabel;

            // Clamp index into range so we never blow up on edges
            int clampedIndex = Mathf.Clamp(partIndex, 0, model.parts.Count - 1);

            var part = model.parts[clampedIndex];
            var label = part != null ? part.label : null;

            // Fallbacks if label is empty / null
            if (string.IsNullOrWhiteSpace(label))
            {
                label = (clampedIndex == 0)
                    ? defaultPartLabel
                    : $"Part {clampedIndex + 1}";
            }

            return label;
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
                    existing.info = "Improvisation";
                }
                else
                {
                    solo.tracks.Add(new TrackEntry
                    {
                        musicianId = musicianId,
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
            {
                reason = "Not a composition card.";
                return false;
            }

            var data = card.CardData;

            // Track cards always need a musician (unless they auto-resuelven por tipo)
            if (data.IsTrackCard && target == null)
            {
                reason = "Select a musician.";
                return false;
            }

            // Part cards que NO crean parte sí necesitan que exista al menos una
            if (data.IsPartCard && model.parts.Count == 0)
            {
                var pa = data.PartAction;
                bool createsPart =
                    pa != null &&
                    pa.action == PartActionDescriptor.PartActionKind.CreatePart;

                if (!createsPart)
                {
                    reason = "Create a part first (play any Track or Part-Create card).";
                    return false;
                }
            }

            return true;
        }

        private bool TryAddOrReplaceTrackOnPart(
            PartEntry part, int partIndex,
            string musicianId, string musicianName,
            TrackRole role, string info, CardData sourceCard)
        {
            if (part == null || string.IsNullOrEmpty(musicianId)) return false;

            Log($"[ApplyCardToPart] Track '{role}' for " +
                $"'{musicianName}' ({musicianId}) on partIndex={partIndex}", true);

            int beforeCount = part.tracks != null ? part.tracks.Count : 0;

            var existing = part.tracks.FirstOrDefault(t => t.musicianId == musicianId);
            int complexity = 
                Mathf.Max(0, sourceCard != null ? sourceCard.InspirationGenerated : 0);
            var synergy = sourceCard != null ? sourceCard.CardType : CardType.None;

            if (existing != null)
            {
                // REPLACE (update metadata + overrides)
                existing.role = role;
                existing.info = info;
                existing.inspirationGenerated = 
                    Mathf.Max(0, sourceCard != null ? sourceCard.InspirationGenerated : 0);
                existing.synergyType = synergy;

                // Style budles
                if (sourceCard != null && sourceCard.IsTrackCard &&
                    sourceCard.TrackAction?.styleBundle != null)
                {
                    existing.styleBundle = sourceCard.TrackAction.styleBundle;
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
                    inspirationGenerated = 
                        Mathf.Max(0, sourceCard != null ? sourceCard.InspirationGenerated : 0),
                    synergyType = synergy,
                };


                // Style budles
                if (sourceCard != null && sourceCard.IsTrackCard &&
                    sourceCard.TrackAction?.styleBundle != null)
                {
                    entry.styleBundle = sourceCard.TrackAction.styleBundle;
                }

                part.tracks.Add(entry);
            }

            if (beforeCount == 0)
                SetPartVisible(partIndex, true);

            // UI + icons refresh for the *indexed* part
            partUIs[partIndex].AddOrUpdateTrack(musicianId, role.ToString(), info);
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

        /// <summary>
        /// Applies one effect to the composition model, honoring scope and timing.
        /// For now, we resolve timing as:
        /// - Immediate: apply to 'partIndex'
        /// - OnNextLoop: apply to 'partIndex' (UI-side, loop engine decides when audible)
        /// - OnNextPartStart: apply to 'partIndex+1' (auto-ensure)
        /// In a later pass we can forward 'timing' into ShipInteriorManager's cache invalidation logic.
        /// </summary>
        private bool ApplyEffectToModel(PartEffect fx, int partIndex, MusicianBase target)
        {
            if (fx == null) return false;

            // Resolve target index by timing
            int idx = partIndex;
            if (fx.timing == ApplyTiming.OnNextPartStart)
                idx = partIndex + 1;

            var part = EnsurePartAt(idx);
            if (part == null) return false;

            switch (fx)
            {
                case TempoEffect t:
                {
                    switch (t.mode)
                    {
                        case TempoEffect.TempoEffectMode.Range:
                            part.tempoRangeOverride = t.tempoRange;
                            part.absoluteBpmOverride = null;
                            // label friendly
                            part.tempo = t.tempoRange.ToString();
                            break;

                        case TempoEffect.TempoEffectMode.AbsoluteBpm:
                            part.absoluteBpmOverride = t.absoluteBpm;
                            part.tempoRangeOverride = TempoRange.Fast;
                            part.tempo = $"{t.absoluteBpm} BPM";
                            break;

                        case TempoEffect.TempoEffectMode.ScaleFactor:
                            // Componer factores si se juegan varias cartas
                            part.tempoScale *= t.tempoScale;
                            part.tempo = $"×{part.tempoScale:0.##}";
                            break;
                    }

                    partUIs[partIndex].Bind(part);
                    RaisePartChanged();
                    break;
                }

                case MeterEffect m:
                    part.timeSignature = m.timeSignature;
                    break;

                case TonalityEffect ton:
                {
                    Tonality chosen = ton.tonality;

                    switch (ton.mode)
                    {
                        case TonalityEffect.TonalityEffectMode.Explicit:
                            chosen = ton.tonality;
                            break;

                        case TonalityEffect.TonalityEffectMode.RandomAny:
                            chosen = GetRandomAnyTonality();
                            break;

                        case TonalityEffect.TonalityEffectMode.RandomMajorish:
                            chosen = GetRandomMajorishTonality();
                            break;

                        case TonalityEffect.TonalityEffectMode.RandomMinorish:
                            chosen = GetRandomMinorishTonality();
                            break;
                    }

                    part.tonality = chosen;

                    // If you eventually show tonality in UI, rebind:
                    var ui = partUIs.ElementAtOrDefault(idx);
                    if (ui != null) ui.Bind(part);

                    RaisePartChanged();
                    break;
                }

                case InstrumentEffect instFx:
                    ApplyInstrumentEffect(instFx, target, part, partIndex);
                    break;

                case ModulationEffect mod:
                {
                    // Current key context
                    var currentMode = part.tonality;
                    var currentRoot = part.rootNote;

                    NoteName newRoot = currentRoot;

                    switch (mod.mode)
                    {
                        case ModulationEffect.ModulationMode.AbsoluteKey:
                            newRoot = mod.absoluteRoot;
                            break;

                        case ModulationEffect.ModulationMode.IntervalWithinScale:
                            {
                                var scale = GetScaleFromTonality(currentMode, currentRoot);
                                if (GetNoteFromScale(
                                    scale, mod.targetDegree, currentRoot, 4, out var note))
                                    newRoot = note.NoteName;
                                break;
                            }

                        case ModulationEffect.ModulationMode.RandomAny:
                            newRoot = GetRandomNote();   // uses MusicTheory helper
                            break;

                        case ModulationEffect.ModulationMode.RandomWithinScale:
                            {
                                var scale = GetScaleFromTonality(currentMode, currentRoot);
                                var notes = GetNotesFromScale(scale, currentRoot, 4, 7);
                                if (notes != null && notes.Count > 0)
                                {
                                    int startIdx = 0;
                                    int endIdx = notes.Count;

                                    // Optionally avoid staying on tonic
                                    if (mod.excludeTonicOnRandomWithinScale && notes.Count > 1)
                                    {
                                        startIdx = 1; // skip degree 1
                                    }

                                    int idxInScale = UnityEngine.Random.Range(startIdx, endIdx);
                                    newRoot = notes[idxInScale].NoteName;
                                }
                                break;
                            }
                    }

                    part.rootNote = newRoot;
                    part.hasExplicitRootNote = true;

                    // Re-bind UI for that part if you eventually display key info
                    var ui = partUIs.ElementAtOrDefault(idx);
                    if (ui != null) ui.Bind(part);

                    RaisePartChanged();
                    break;
                }
            }

            return true;
        }

        private void ApplyInstrumentEffect(
            InstrumentEffect fx,
            MusicianBase target,
            PartEntry part,
            int partIndex)
        {
            if (fx == null) return;

            // For now: we only support TrackOnly and require a target musician
            if (fx.scope == EffectScope.TrackOnly && target != null)
            {
                var track = part.tracks.FirstOrDefault(t =>
                    t.musicianId == target.MusicianCharacterData.CharacterId);

                if (track == null)
                {
                    Log($"[InstrumentEffect] No track found for target " +
                        $"'{target.name}' in part {partIndex}", false);
                    return;
                }

                // Clear existing overrides
                track.overrideMelodicInstrument = null;
                track.overridePercussionInstrument = null;
                track.hasOverrideInstrumentType = false;

                switch (fx.mode)
                {
                    case InstrumentEffect.InstrumentTargetMode.SpecificMelodic:
                        track.overrideMelodicInstrument = fx.melodicInstrument;
                        break;

                    case InstrumentEffect.InstrumentTargetMode.SpecificPercussion:
                        track.overridePercussionInstrument = fx.percussionInstrument;
                        break;

                    case InstrumentEffect.InstrumentTargetMode.InstrumentType:
                        track.hasOverrideInstrumentType = true;
                        track.overrideInstrumentType = fx.instrumentType;
                        break;
                }

                // Optional: reflect something in the info label
                if (track.styleBundle != null && fx.melodicInstrument != null)
                {
                    track.info = $"{track.styleBundle.name} - " +
                        $"{fx.melodicInstrument.InstrumentName}";
                }

                var ui = partUIs.ElementAtOrDefault(partIndex);
                if (ui != null) ui.Bind(part);

                RaisePartChanged();
            }
            else
            {
                // Later: handle broader scopes if you want
                Log("[InstrumentEffect] Non-TrackOnly scopes not implemented yet.");
            }
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
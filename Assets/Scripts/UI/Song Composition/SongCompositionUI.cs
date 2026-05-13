using ALWTTT.Cards;
using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Managers;
using Melanchall.DryWetMidi.MusicTheory;
using MidiGenPlay;
using MidiGenPlay.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static MidiGenPlay.MusicTheory.MusicTheory;

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

        [Header("Inspiration Pulse [B2 / #4]")]
        [SerializeField, Tooltip("Optional pulse animator attached to the Inspiration value text. " +
            "Pops on every SetInspiration value change. Tint reflects sign of delta.")]
        private UIPulseAnimator inspirationValuePulse;
        [SerializeField, Tooltip("Optional pulse animator attached to the +N inspiration badge. " +
            "Pops when transitioning into a non-empty value (i.e. a new positive +N appears).")]
        private UIPulseAnimator plusInspirationPulse;
        [SerializeField] private Color inspirationGainColor = new Color(0.4f, 1.0f, 0.4f);
        [SerializeField] private Color inspirationLossColor = new Color(1.0f, 0.35f, 0.35f);
        [SerializeField] private Color plusInspirationFlashColor = new Color(0.55f, 0.95f, 1.0f);

        [Header("Composition Floating Text [B2 / #3]")]
        [SerializeField, Tooltip("Optional anchor for floating-text spawned on successful card apply " +
            "(TEMPO! / METER! / KEY! / RHYTHM! / etc). Leave null to skip composition floating text.")]
        private Transform compositionFxAnchor;
        [SerializeField] private Vector2 compositionFxDir = new Vector2(0f, 1.2f);
        [SerializeField, Tooltip("Per-category enabled/label/color config. " +
            "Leave null to skip composition floating text (same effect as null anchor).")]
        private CompositionFxConfigSO fxConfig;

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

            // [B2 / #3] Source card definition for hover-tooltip minicard preview.
            // Non-serialized: TrackEntry is a runtime-only struct, never written to
            // asset; we don't need Unity to persist this reference.
            [NonSerialized] public CardDefinition sourceCardDefinition;
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

        // [B1 / #1+#2 / D-H1+H4=α] Pending-track state per partIndex.
        // Populated by MarkTrackPending / MarkAllTracksPending (called by
        // CompositionSession when a sound-affecting card is played during
        // an active loop). Cleared by OnRenderCompleted when a fresh render
        // succeeds for that part.
        private readonly Dictionary<int, HashSet<string>> _pendingByPart = new();
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
            if (card == null || card.CardDefinition == null || !card.CardDefinition.IsComposition)
                return false;

            var data = card.CardDefinition;
            var comp = data.CompositionPayload;

            if (comp == null)
            {
                Log("ApplyCardToPart: card is marked Composition but has no CompositionPayload.");
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

            // [B2 / #3] Capture pre-apply state for diff-driven fx classification.
            // Must run BEFORE any mutation (sections 3-5 below). Cheap copy of
            // ~10 value-typed fields; not a perf concern.
            var snapshot = CaptureSnapshot(part, target);

            // ---------------------------------------------------------
            // 2) RESOLVE TARGET MUSICIAN (IF REQUIRED)
            // ---------------------------------------------------------
            string tgtId = target != null ? target.MusicianCharacterData.CharacterId : null;
            string tgtName = target != null ? target.MusicianCharacterData.CharacterName : null;

            // If this composition card requires a musician (track targeting / TrackOnly fx), enforce it here.
            if (comp.RequiresMusicianTarget && target == null)
            {
                Log("ApplyCardToPart: composition card requires a musician target, but none was provided.");
                return false;
            }

            // ---------------------------------------------------------
            // 3) PRIMARY ACTION: TRACK
            // ---------------------------------------------------------
            if (comp.PrimaryKind == CardPrimaryKind.Track)
            {
                var desc = comp.TrackAction;
                if (desc == null)
                {
                    Log("ApplyCardToPart: Track card has no TrackAction descriptor.");
                    return false;
                }

                // Track primary always requires a musician target (redundant with RequiresMusicianTarget, but clearer)
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
                    data.DisplayName,
                    data,
                    desc.styleBundle);

                if (!ok) return false;
            }

            // ---------------------------------------------------------
            // 4) PRIMARY ACTION: PART
            // ---------------------------------------------------------
            else if (comp.PrimaryKind == CardPrimaryKind.Part)
            {
                var pa = comp.PartAction;
                if (pa == null)
                {
                    Log("ApplyCardToPart: Part card has no PartAction descriptor.");
                    return false;
                }

                switch (pa.action)
                {
                    case PartActionKind.CreatePart:
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
                        BeginDraftNextPart(
                            string.IsNullOrWhiteSpace(pa.customLabel) ? "Bridge" : pa.customLabel);
                        break;

                    case PartActionKind.MarkFinal:
                        SetPartFinal(partIndex, true);
                        break;
                }
            }
            else
            {
                Log($"ApplyCardToPart: unsupported PrimaryKind '{comp.PrimaryKind}'");
                return false;
            }

            // ---------------------------------------------------------
            // 5) SECONDARY MODIFIER EFFECTS
            // ---------------------------------------------------------
            var effects = comp.ModifierEffects;
            if (effects != null)
            {
                foreach (var fx in effects)
                    ApplyEffectToModel(fx, partIndex, target);
            }

            // ---------------------------------------------------------
            // 6) REFRESH UI + TRIGGER EVENTS
            // ---------------------------------------------------------
            RefreshPartUI(partIndex);

            // [B2 / #3] Composition floating text on successful apply.
            // Diff-driven: spawns at most one label, based on what actually
            // changed between snapshot and current part state.
            SpawnCompositionFx(data, comp, snapshot, partIndex, target);

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

        // [B2 / #4] Track previous values so we can detect mutations and pulse.
        private int _lastInspirationValue = int.MinValue;
        private string _lastPlusInspirationText;

        public void SetInspiration(int value)
        {
            if (inspirationText != null)
                inspirationText.text = value.ToString();

            // [B2 / #4] Pulse on change. Skip first-set (no prior baseline).
            if (_lastInspirationValue != int.MinValue
                && value != _lastInspirationValue
                && inspirationValuePulse != null)
            {
                var c = value > _lastInspirationValue
                    ? inspirationGainColor
                    : inspirationLossColor;
                inspirationValuePulse.Pulse(c);
            }
            _lastInspirationValue = value;
        }

        public void SetPlusInspiration(int amount)
        {
            if (plusInspirationText == null) return;

            string newText = amount > 0 ? $"+{amount}" : string.Empty;
            bool changed = !string.Equals(_lastPlusInspirationText, newText);
            plusInspirationText.text = newText;

            // [B2 / #4] Pulse only when the badge transitions into / changes a
            // non-empty +N (e.g. a track is added, or inspirationGenerated changes).
            // Suppressed when the badge clears (avoids a pop on hide).
            if (changed && amount > 0 && plusInspirationPulse != null)
                plusInspirationPulse.Pulse(plusInspirationFlashColor);

            _lastPlusInspirationText = newText;
        }

        /// <summary>
        /// [B2 / #4] Pop the inspiration value text in the loss color without
        /// changing the underlying value. Used as a "denied — not enough" signal
        /// when the player attempts to play a card they can't afford.
        /// </summary>
        public void FlashInspirationDenied()
        {
            if (inspirationValuePulse != null)
                inspirationValuePulse.Pulse(inspirationLossColor);
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

            if (card == null || card.CardDefinition == null || !card.CardDefinition.IsComposition)
            {
                reason = "Not a composition card.";
                return false;
            }

            var data = card.CardDefinition;
            var comp = data.CompositionPayload;
            if (comp == null)
            {
                reason = "Missing composition payload.";
                return false;
            }

            // Track cards always need a musician (unless you later auto-resolve by type)
            if (comp.PrimaryKind == CardPrimaryKind.Track && target == null)
            {
                reason = "Select a musician.";
                return false;
            }

            // Part cards that do NOT create a part require that at least one part exists
            if (comp.PrimaryKind == CardPrimaryKind.Part && model.parts.Count == 0)
            {
                var pa = comp.PartAction;
                bool createsPart = pa != null && pa.action == PartActionKind.CreatePart;

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
    TrackRole role, string info,
    CardDefinition sourceCard,
    TrackStyleBundleSO styleBundle)
        {
            if (part == null || string.IsNullOrEmpty(musicianId)) return false;

            Log($"[ApplyCardToPart] Track '{role}' for " +
                $"'{musicianName}' ({musicianId}) on partIndex={partIndex}", true);

            int beforeCount = part.tracks != null ? part.tracks.Count : 0;

            int complexity = Mathf.Max(0, sourceCard != null ? sourceCard.InspirationGenerated : 0);
            var synergy = sourceCard != null ? sourceCard.CardType : CardType.None;

            var existing = part.tracks.FirstOrDefault(t => t.musicianId == musicianId);

            if (existing != null)
            {
                existing.role = role;
                existing.info = info;
                existing.inspirationGenerated = complexity;
                existing.synergyType = synergy;
                existing.styleBundle = styleBundle;
                existing.sourceCardDefinition = sourceCard; // [B2 / #3]
            }
            else
            {
                var entry = new TrackEntry
                {
                    musicianId = musicianId,
                    role = role,
                    info = info,
                    inspirationGenerated = complexity,
                    synergyType = synergy,
                    styleBundle = styleBundle,
                    sourceCardDefinition = sourceCard, // [B2 / #3]
                };

                part.tracks.Add(entry);
            }

            if (beforeCount == 0)
                SetPartVisible(partIndex, true);

            partUIs[partIndex].AddOrUpdateTrack(
                musicianId, role.ToString(), info,
                inspirationNext: complexity,
                sourceCard: sourceCard); // [B2 / #3]
            UpdateIconsForCurrentPart();
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

        // ─────────────────────────────────────────────────────────────
        // [B1 / #1+#2 / D-H1+H4=α] Pending track visualization API.
        // Called by CompositionSession.TryPlayCompositionCard (when a
        // sound-affecting card is played during an active loop) and
        // PlaySinglePartLoop (clears pending after a successful fresh
        // render).
        // ─────────────────────────────────────────────────────────────
        public void MarkTrackPending(int partIndex, string musicianId)
        {
            if (partIndex < 0 || partIndex >= partUIs.Count) return;
            if (string.IsNullOrEmpty(musicianId)) return;

            if (!_pendingByPart.TryGetValue(partIndex, out var set))
            {
                set = new HashSet<string>();
                _pendingByPart[partIndex] = set;
            }
            if (set.Add(musicianId))
                RefreshPartUI(partIndex);
        }

        public void MarkAllTracksPending(int partIndex)
        {
            if (partIndex < 0 || partIndex >= partUIs.Count) return;
            if (partIndex >= model.parts.Count) return;
            var part = model.parts[partIndex];
            if (part?.tracks == null || part.tracks.Count == 0) return;

            if (!_pendingByPart.TryGetValue(partIndex, out var set))
            {
                set = new HashSet<string>();
                _pendingByPart[partIndex] = set;
            }
            bool changed = false;
            foreach (var t in part.tracks)
            {
                if (!string.IsNullOrEmpty(t?.musicianId))
                    changed |= set.Add(t.musicianId);
            }
            Debug.Log($"[B1/Pending] MarkAllTracksPending partIndex={partIndex} " +
                $"trackCount={part.tracks.Count} markedSetSize={set.Count} changed={changed}");

            if (changed) RefreshPartUI(partIndex);
        }

        public void OnRenderCompleted(int partIndex)
        {
            if (_pendingByPart.Remove(partIndex))
                RefreshPartUI(partIndex);
        }

        private void RefreshPartUI(int partIndex)
        {
            if (partIndex < 0 || partIndex >= partUIs.Count) return;
            if (partUIs[partIndex] == null) return;
            if (partIndex >= model.parts.Count) return;
            _pendingByPart.TryGetValue(partIndex, out var pendingSet);
            partUIs[partIndex].Bind(model.parts[partIndex], pendingSet);
        }

        private void AddPartUI(PartEntry p)
        {
            if (partsLayout == null || partElementPrefab == null) return;

            var element = Instantiate(partElementPrefab, partsLayout.ContentRoot);
            var ui = element.GetComponent<SongPartElementUI>();
            if (ui == null) return;

            ui.SetRosterOrder(rosterOrder);
            partUIs.Add(ui);
            RefreshPartUI(partUIs.Count - 1);
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

                        RefreshPartUI(partIndex);
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

                        RefreshPartUI(idx);

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

                        RefreshPartUI(idx);

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

                RefreshPartUI(partIndex);

                RaisePartChanged();
            }
            else
            {
                // Later: handle broader scopes if you want
                Log("[InstrumentEffect] Non-TrackOnly scopes not implemented yet.");
            }
        }
        #endregion

        #region [B2 / #3] Composition Floating Text

        /// <summary>
        /// Pre-apply snapshot of the part fields the classifier diffs against.
        /// Captured at the top of <see cref="ApplyCardToPart"/> before any mutation.
        /// </summary>
        private readonly struct PartChangeSnapshot
        {
            public readonly bool valid;

            // Part-level
            public readonly TempoRange prevTempoRange;
            public readonly int? prevAbsoluteBpm;
            public readonly string prevTimeSignatureStr;
            public readonly Tonality prevTonality;
            public readonly NoteName prevRootNote;
            public readonly bool prevHadExplicitRoot;

            // Track-level (for the target musician, when present)
            public readonly bool prevTrackExisted;
            public readonly CardDefinition prevTrackSourceCard;
            public readonly MIDIInstrumentSO prevMelodicInstrument;
            public readonly MIDIPercussionInstrumentSO prevPercussionInstrument;
            public readonly bool prevHadInstrumentTypeOverride;
            public readonly InstrumentType prevInstrumentType;

            public PartChangeSnapshot(
                TempoRange tempoRange, int? absBpm, string tsStr,
                Tonality tonality, NoteName root, bool hadExplicitRoot,
                bool trackExisted,
                CardDefinition trackSource,
                MIDIInstrumentSO melodic, MIDIPercussionInstrumentSO percussion,
                bool hadInstType, InstrumentType instType)
            {
                valid = true;
                prevTempoRange = tempoRange;
                prevAbsoluteBpm = absBpm;
                prevTimeSignatureStr = tsStr;
                prevTonality = tonality;
                prevRootNote = root;
                prevHadExplicitRoot = hadExplicitRoot;
                prevTrackExisted = trackExisted;
                prevTrackSourceCard = trackSource;
                prevMelodicInstrument = melodic;
                prevPercussionInstrument = percussion;
                prevHadInstrumentTypeOverride = hadInstType;
                prevInstrumentType = instType;
            }
        }

        /// <summary>
        /// Capture the diff-relevant pre-apply state for a part + optional target.
        /// Must be called BEFORE any mutation in <see cref="ApplyCardToPart"/>.
        /// </summary>
        private PartChangeSnapshot CaptureSnapshot(PartEntry part, MusicianBase target)
        {
            if (part == null) return default;

            TrackEntry existing = null;
            if (target != null)
            {
                string tgtId = target.MusicianCharacterData.CharacterId;
                existing = part.tracks?.FirstOrDefault(t => t.musicianId == tgtId);
            }

            return new PartChangeSnapshot(
                tempoRange: part.tempoRangeOverride,
                absBpm: part.absoluteBpmOverride,
                tsStr: part.timeSignature.ToString(),
                tonality: part.tonality,
                root: part.rootNote,
                hadExplicitRoot: part.hasExplicitRootNote,
                trackExisted: existing != null,
                trackSource: existing?.sourceCardDefinition,
                melodic: existing?.overrideMelodicInstrument,
                percussion: existing?.overridePercussionInstrument,
                hadInstType: existing != null && existing.hasOverrideInstrumentType,
                instType: existing != null && existing.hasOverrideInstrumentType
                    ? existing.overrideInstrumentType : default
            );
        }

        /// <summary>
        /// Spawn at most one floating text classifying the change carried by a
        /// successfully-applied composition card. No-op if the anchor or config
        /// is unset, the master toggle is off, or no real diff was detected.
        /// </summary>
        private void SpawnCompositionFx(
            CardDefinition card, CompositionCardPayload comp,
            PartChangeSnapshot before, int partIndex, MusicianBase target)
        {
            if (compositionFxAnchor == null) return;
            if (fxConfig == null || !fxConfig.Enabled) return;
            if (card == null || comp == null) return;
            if (FxManager.Instance == null) return;
            if (!before.valid) return;

            var entry = SelectFxEntry(comp, before, partIndex, target);
            if (entry == null || !entry.enabled || string.IsNullOrEmpty(entry.label))
                return;

            FxManager.Instance.SpawnFloatingText(
                compositionFxAnchor, entry.label, compositionFxDir, entry.color);
        }

        /// <summary>
        /// Diff-driven category selection. Counts how many post-apply fields
        /// actually changed vs <paramref name="before"/>:
        /// - 0 diffs → null (no fx)
        /// - 1 diff  → that category's entry (TEMPO!, METER!, KEY!, RHYTHM!, etc)
        /// - 2+ diffs → MajorChange entry
        ///
        /// Tonality distinguishes first-set ("TONALITY!") from mid-song
        /// modulation ("KEY!") using the <c>prevHadExplicitRoot</c> flag.
        /// </summary>
        private CompositionFxConfigSO.FxEntry SelectFxEntry(
            CompositionCardPayload comp, PartChangeSnapshot before,
            int partIndex, MusicianBase target)
        {
            if (model?.parts == null
                || partIndex < 0 || partIndex >= model.parts.Count)
                return null;

            var part = model.parts[partIndex];

            int diffCount = 0;
            CompositionFxConfigSO.FxEntry single = null;

            // -- Tempo diff --
            bool tempoChanged =
                part.tempoRangeOverride != before.prevTempoRange
                || part.absoluteBpmOverride != before.prevAbsoluteBpm;
            if (tempoChanged)
            {
                diffCount++;
                single = fxConfig.Tempo;
            }

            // -- Meter diff (string compare to avoid TimeSignature equality assumptions) --
            string nowTs = part.timeSignature.ToString();
            if (!string.Equals(nowTs, before.prevTimeSignatureStr,
                StringComparison.Ordinal))
            {
                diffCount++;
                single = fxConfig.Meter;
            }

            // -- Tonality / Modulation diff --
            // [B2 / #3] Refinement: "highlight CHANGES" — a first-time tonality set
            // (no explicit root before) is INITIAL SETUP, not a change. Silent.
            // Only mid-song modulation (explicit → different explicit) reads as a change.
            if (before.prevHadExplicitRoot)
            {
                bool tonalityChanged =
                    part.tonality != before.prevTonality
                    || part.rootNote != before.prevRootNote;
                if (tonalityChanged)
                {
                    diffCount++;
                    single = fxConfig.Modulation;
                }
            }
            // else: first explicit set — silent. fxConfig.Tonality entry retained
            // for future use if "first set" semantics are ever desired.

            // -- Track diff (only when comp targets a musician via TrackAction) --
            // [B2 / #3] Refinement: "highlight CHANGES" — a fresh track add to an
            // empty slot is INITIAL SETUP, not a change. Silent. Only firing on
            // REPLACEMENT (slot had a different card before) reads as a change.
            if (target != null && comp.PrimaryKind == CardPrimaryKind.Track)
            {
                string tgtId = target.MusicianCharacterData.CharacterId;
                var nowTrack = part.tracks?.FirstOrDefault(t => t.musicianId == tgtId);

                bool isReplacedTrack =
                    before.prevTrackExisted
                    && nowTrack != null
                    && before.prevTrackSourceCard != nowTrack.sourceCardDefinition;

                if (isReplacedTrack)
                {
                    diffCount++;
                    single = nowTrack.role switch
                    {
                        TrackRole.Rhythm => fxConfig.Rhythm,
                        TrackRole.Backing => fxConfig.Backing,
                        TrackRole.Melody => fxConfig.Melody,
                        TrackRole.Harmony => fxConfig.Harmony,
                        _ => fxConfig.Fallback,
                    };
                }
                else if (before.prevTrackExisted && nowTrack != null)
                {
                    // Track unchanged at source-card level — check for instrument-only override change.
                    bool instOverrideChanged =
                        nowTrack.overrideMelodicInstrument != before.prevMelodicInstrument
                        || nowTrack.overridePercussionInstrument != before.prevPercussionInstrument
                        || nowTrack.hasOverrideInstrumentType != before.prevHadInstrumentTypeOverride
                        || (nowTrack.hasOverrideInstrumentType
                            && nowTrack.overrideInstrumentType != before.prevInstrumentType);
                    if (instOverrideChanged)
                    {
                        diffCount++;
                        single = fxConfig.Instrument;
                    }
                }
            }

            if (diffCount == 0) return null;       // no real change → silent
            if (diffCount == 1) return single;     // one diff → its specific entry
            return fxConfig.MajorChange;           // 2+ real diffs → MAJOR CHANGE
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
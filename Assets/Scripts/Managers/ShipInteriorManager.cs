using ALWTTT.Characters;
using ALWTTT.Characters.Band;
using ALWTTT.Music;
using ALWTTT.UI;
using ALWTTT.Utils;

using MidiGenPlay;
using MidiGenPlay.Composition;
using MidiGenPlay.Interfaces;
using MidiGenPlay.Services;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using static ALWTTT.CardData;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.Managers
{
    public class ShipInteriorManager : MonoBehaviour
    {
        private const string DebugTag = "<color=green>[Rehearsal]</color>";

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> musicianPosList;

        [Header("Cards / Hand")]
        [SerializeField] private HandController shipHand;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera handCamera;

        [Header("Composition")]
        [SerializeField] private SongCompositionUI compositionUI;
        [SerializeField] private MidiGenPlayConfig midiGenPlayConfig;
        [SerializeField] private bool useProceduralRhythm = true;
        [SerializeField] private bool useProceduralBacking = true;

        [Header("Melody (Dev)")]
        [SerializeField] private MelodicLeadingConfig melodicConfig;
        [SerializeField] private MelodyStrategyId melodyStrategyId;
        [SerializeField] private HarmonicLeadingConfig harmonicConfig;
        [SerializeField] private HarmonyStrategyId harmonyStrategyId;

        [Header("Refs")]
        [SerializeField] private ShipInteriorCanvas shipCanvas;
        [SerializeField] private SceneChanger sceneChanger;

        [Header("Dev")]
        [SerializeField] private bool useLogs = false;
        [SerializeField] private bool composeUseLayeredEntrances = true;
        [SerializeField] private bool composeEnablePostProcessing = true;
        [SerializeField] private bool composeUsePersonalityBias = true;
        [SerializeField] private bool composeAddIntro = false;
        [SerializeField] private bool composeAddOutro = false;

        [SerializeField] private MidiMusicManager.HighlightMode defaultHighlightMode =
            MidiMusicManager.HighlightMode.DuckOthers;
        private string _nextHighlightMusicianId;

        private bool isPlaying;
        private bool inRehearsal = false;

        private readonly List<MusicianBase> _spawned = new();
        public SongCompositionUI.SongModel GetCurrentComposition() => compositionUI?.Model;

        #region Cache
        private GameManager GameManager => GameManager.Instance;
        private MidiMusicManager MidiMusicManager => MidiMusicManager.Instance;
        #endregion

        private IInstrumentRepository instrumentRepo;
        private IPatternRepository patternRepo;
        private System.Random rng = new System.Random();

        private void Log(string log, bool highlight = false)
        {
            if (useLogs)
            {
                if (highlight) Debug.Log($"{DebugTag} <color=yellow>{log}</color>");
                else Debug.Log($"{DebugTag} {log}");
            }
        }

        private void Start()
        {
            RebindDeckToShipHand();
            BuildBand();

            if (shipCanvas)
            {
                shipCanvas.Setup(onCompose: OnCompose, onRelax: OnRelax, onBandTalk: OnBandTalk);
                shipCanvas.HookPlayButton(OnPlayPressed);

                shipCanvas.HookMetronomeToggle(
                    OnMetronomeToggled,
                    MidiMusicManager != null && MidiMusicManager.MetronomeEnabled);

                // Populate dropdowns with spawned musicians
                var items = _spawned
                    .Select(m => (m.MusicianCharacterData.CharacterId,
                                  m.MusicianCharacterData.CharacterName))
                    .ToList();

                shipCanvas.PopulateHighlightDropdown(items, id => _nextHighlightMusicianId = id, true);

                /*
                shipCanvas.PopulateIntroDropdown(
                    items, id => { _nextIntroMusicianId = id; composeAddIntro = !string.IsNullOrEmpty(id); }, includeNoneOption: true);
                shipCanvas.PopulateOutroDropdown(
                    items, id => { _nextOutroMusicianId = id; composeAddOutro = !string.IsNullOrEmpty(id); }, includeNoneOption: true);
                shipCanvas.PopulateSoloDropdown(
                    items, id => _nextSoloMusicianId = id, includeNoneOption: true);*/

                shipCanvas.HookLayeredEntranceToggle(v => composeUseLayeredEntrances = v, composeUseLayeredEntrances);
                shipCanvas.HookEnablePPToggle(v => composeEnablePostProcessing = v, composeEnablePostProcessing);
                shipCanvas.HookEnablePersonalityToggle(v => composeUsePersonalityBias = v, composeUsePersonalityBias);

                // Tempo scale choices for the *next* song (they enqueue in MidiMusicManager)
                var tempoOptions = new List<(string label, float factor)>
                {
                    ("0.75× (laid-back)", 0.75f), ("1.00× (default)", 1.00f),
                    ("1.25×", 1.25f),             ("1.50×", 1.50f), ("2.00×", 2.00f)
                };
                shipCanvas.PopulateTempoScaleDropdown(tempoOptions,
                    factor => MidiMusicManager?.ScheduleNextSongTempoScale(factor), defaultIndex: 1);

                /*
                // Alternate Track dropdowns (keep for legacy UI testing)
                shipCanvas.PopulateAlternateTrackDropdown(items, id => _nextAltTrackMusicianId = id, includeNoneOption: true);
                var strategies = new List<(string id, string label)>
                {
                    ("busier", "Busier"), ("sparser", "Sparser"), ("rotate1", "Rotate 1 beat"), ("rotate2", "Rotate 2 beats"),
                };
                shipCanvas.PopulateAlternateStrategyDropdown(strategies, id => _nextAltStrategyId = id, includeNoneOption: true);*/
            }

            SetHandVisible(false);
            SetCompositionVisible(false);

            // Services
            instrumentRepo = new InstrumentRepositoryResources(midiGenPlayConfig);
            patternRepo = new PatternRepositoryResources(midiGenPlayConfig);
            instrumentRepo.Refresh();
            patternRepo.Refresh();

            Log("[Jam] Repositories refreshed.");
            Log($"[Jam] Melodic instruments: {instrumentRepo.GetMelodicInstruments().Count()}");
            Log($"[Jam] Percussion instruments: {instrumentRepo.GetPercussionInstruments().Count()}");
            Log($"[Jam] Drum patterns: {patternRepo.GetAllDrumPatterns().Count()}");
            Log($"[Jam] Chord progressions: {patternRepo.GetAllChordProgressions().Count()}");
            Log($"[Jam] Melody patterns: {patternRepo.GetAllMelodyPatterns().Count()}");
        }

        private void PrepareRehearsalDeck()
        {
            var gd = GameManager.GameplayData;
            var pool = gd.CompositionCardPool != null && gd.CompositionCardPool.Count > 0
                ? gd.CompositionCardPool
                : gd.AllCardsList.FindAll(c => c != null && c.IsComposition);

            var deck = DeckManager.Instance;
            deck.ClearAll();
            deck.AddToDrawPile(pool);
            deck.DrawCards(gd.DrawCount);
        }

        private void BuildBand()
        {
            var pd = GameManager.PersistentGameplayData;
            _spawned.Clear();

            for (int i = 0; i < pd.MusicianList.Count; i++)
            {
                var prefab = pd.MusicianList[i];
                var parent = musicianPosList != null && musicianPosList.Count > i
                           ? musicianPosList[i]
                           : transform;

                var clone = Instantiate(prefab, parent);
                clone.BuildCharacter(); // same pattern as GigManager  :contentReference[oaicite:2]{index=2}

                var responder = clone.gameObject.GetComponent<MusicianMidiResponder>();
                if (responder == null) responder =
                        clone.gameObject.AddComponent<MusicianMidiResponder>();
                responder.Init(clone);

                _spawned.Add(clone);

                MidiMusicManager?.RegisterMusicianAnchor(
                    clone.MusicianCharacterData.CharacterId, clone.transform);
            }
        }

        private void OnCompose()
        {
            if (inRehearsal) return;

            shipCanvas?.SetMainButtonsVisible(false);
            BeginRehearsalSession();
        }

        private void OnPlayPressed()
        {
            if (isPlaying) return;
            if (!inRehearsal) return;

            SetHandVisible(false);
            SetCompositionVisible(false);

            // Go play using the already-queued intents (Intro/Solo/Outro/Tempo/ReplaceTrack)
            StartCoroutine(ComposeAndPreviewRoutine());
        }

        private void BeginRehearsalSession()
        {
            inRehearsal = true;
            SetHandVisible(true);
            SetCompositionVisible(true);

            // Clean any pending music intents from a previous session
            try
            {
                // TODO: implement in MidiMusicManager
                //MidiMusicManager?.ClearQueuedIntents(); 
            }
            catch { /* safe no-op if method doesn't exist yet */ }

            // Use composition pool from GameplayData and draw a fresh hand
            PrepareRehearsalDeck();

            if (compositionUI) compositionUI.ResetSession();
            compositionUI?.PopulateMusicianIcons(_spawned);
        }

        private void EndRehearsalSession()
        {
            inRehearsal = false;
            SetHandVisible(false);
            SetCompositionVisible(false);

            // clear queued intents (fresh start next time)
            //try { MidiMusicManager?.ClearQueuedIntents(); } catch { }
        }

        private IEnumerator ComposeAndPreviewRoutine()
        {
            isPlaying = true;

            var mm = MidiMusicManager.Instance;
            if (mm == null) 
            { 
                Debug.LogError($"{DebugTag} MidiMusicManager is null"); 
                isPlaying = false; 
                yield break; 
            }

            // Build config from the UI
            var cfg = BuildSongConfigFromCompositionUI();
            if (cfg == null)
            {
                Debug.LogError($"{DebugTag} " +
                    $"BuildSongConfigFromCompositionUI returned null (nothing to play).");
                isPlaying = false; 
                yield break;
            }

            // Personalities (MVP: Neutral per musician)
            // TODO: Get based on each MusicianCharacterData (base) or persistant state
            var personalityMap = new Dictionary<string, IMusicianPersonality>();
            foreach (var m in _spawned)
            {
                var id = m.MusicianCharacterData.CharacterId;
                personalityMap[id] = new NeutralPersonality(id);
            }
            MidiMusicManager.SetMusicianPersonalities(personalityMap);

            // PLAY the config
            var model = compositionUI.Model;
            // RANDOMIZE SEED, COMMENT IN BUILDS?
            midiGenPlayConfig.defaultSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var seconds = mm.PlayFromConfig(cfg, model.title, _spawned);
            if (seconds <= 0f) { isPlaying = false; yield break; }

            // Live routing: retrieve the owners from the manager using the last key OR
            // rebuild the ordered list from ChannelMusicianOrder.
            var orderedMusicians = cfg.ChannelMusicianOrder
                .Select(id => _spawned.FirstOrDefault(
                    s => s.MusicianCharacterData.CharacterId == id))
                .Where(s => s != null)
                .ToList();

            if (orderedMusicians.Count == 0)
            {
                Debug.LogError($"{DebugTag} No spawned musician matched channel order.");
                isPlaying = false; yield break;
            }

            // Tell manager the same order so it can route realtime events to the right responder
            mm.SetChannelOwners(cfg.ChannelMusicianOrder.ToList());

            // Wait for playback to end, then auto-reset the Jam UI + hand
            yield return MidiMusicManager.WaitForEnd();
            ResetAfterPlayback();

            isPlaying = false;

            /*
            var mm = MidiMusicManager.Instance;
            var pd = GameManager.PersistentGameplayData;

            Debug.Log($"{DebugTag} ComposeAndPreview: begin");

            if (mm == null)
            {
                Debug.LogError($"{DebugTag} MidiMusicManager is null");
                isPlaying = false; yield break;
            }

            // 1) Generate song
            var newSong = pd.GenerateSong();
            if (newSong == null)
            {
                Debug.LogError($"{DebugTag} GenerateSong returned null");
                isPlaying = false; yield break;
            }
            Debug.Log($"{DebugTag} Obtained '{newSong.SongTitle}' song data from pool.");

            var personalityMap = new Dictionary<string, IMusicianPersonality>();
            foreach (var m in _spawned)
            {
                var id = m.MusicianCharacterData.CharacterId;
                // TODO: Different personality per musician
                personalityMap[id] = new NeutralPersonality(id);
            }
            MidiMusicManager.SetMusicianPersonalities(personalityMap);

            if (composeAddIntro && !string.IsNullOrEmpty(_nextIntroMusicianId))
            {
                MidiMusicManager.AddIntro(
                    musicianId: _nextIntroMusicianId,
                    measures: 1,
                    style: IntroStyle.CountIn);
            }

            if (composeAddOutro && !string.IsNullOrEmpty(_nextOutroMusicianId))
            {
                MidiMusicManager.AddOutro(
                    musicianId: _nextOutroMusicianId,
                    measures: 2,                 // tweak if desired
                    style: IntroStyle.Pad);      // reuse styles; Pad is a good default for outro
            }

            // Optional SOLO part: only if a musician was picked (dropdown != NONE)
            if (!string.IsNullOrEmpty(_nextSoloMusicianId))
            {
                MidiMusicManager.AppendSoloPart(
                    musicianId: _nextSoloMusicianId,
                    style: SoloStyle.Virtuoso, // default; adjust later if you add a style UI
                    measures: 8); // default; tweak as you prefer (8/12/16)
            }

            if (!string.IsNullOrEmpty(_nextAltTrackMusicianId) &&
                !string.IsNullOrEmpty(_nextAltStrategyId))
            {
                MidiMusicManager.ReplaceTrack(
                    partIndexOrAll: -1, // apply to all parts for MVP; add a scope control later if desired
                    musicianId: _nextAltTrackMusicianId,
                    new MidiMusicManager.StrategyOverride(_nextAltStrategyId));

                Debug.Log($"{DebugTag} AlternateTrack queued: musician={_nextAltTrackMusicianId}, strategy={_nextAltStrategyId}");
            }

            // 2) Channel owners for this arrangement (index=channel → musicianId)
            var owners = mm.GetChannelOwnerIdsFor(newSong);
            if (owners == null || owners.Count == 0)
            {
                Debug.LogError($"{DebugTag} GetChannelOwnerIdsFor returned empty.");
                isPlaying = false; yield break;
            }

            Debug.Log($"{DebugTag} Channel owners (ch->id): " +
                string.Join(", ", owners.Select((id, i) => $"{i}:{id}")));


            // 3) Wire live routing (so realtime events reach the right musician)
            mm.SetChannelOwners(owners?.ToList());
            Debug.Log($"{DebugTag} SetChannelOwners OK");

            // 4) Build visual entrance order => band objects by id
            var byId = _spawned.ToDictionary(m => m.MusicianCharacterData.CharacterId, m => m);
            var orderedMusicians = owners
                .Select(id => byId.TryGetValue(id, out var m) ? m : null)
                .Where(m => m != null)
                .ToList();

            if (orderedMusicians.Count == 0)
            {
                Debug.LogError($"{DebugTag} No spawned musician matched channel owners.");
                isPlaying = false; yield break;
            }

            var entranceIdsOrdered = orderedMusicians
                .Select(m => m.MusicianCharacterData.CharacterId)
                .ToList();

            var names = string.Join(", ",
                orderedMusicians.Select(m => m.MusicianCharacterData.CharacterName));
            Debug.Log($"{DebugTag} Ordered musicians: {names}");

            // 5) Layered preview
            float lastDuration = 0f;
            if (composeUseLayeredEntrances)
            {
                for (int k = 1; k <= orderedMusicians.Count; k++)
                {
                    var newcomer = orderedMusicians[k - 1];
                    Debug.Log($"{DebugTag} Loop {k}/{orderedMusicians.Count} " +
                        $"newcomer='{newcomer.MusicianCharacterData.CharacterName}'");

                    // Play subset by first k musicians in 'owners'
                    lastDuration =
                        mm.PlaySameArrangementSubsetByMusicians(
                            newSong, entranceIdsOrdered, k);

                    // Wait for actual end
                    yield return MidiMusicManager.WaitForEnd();
                    Debug.Log($"{DebugTag} Loop {k} ended.");
                }
            }

            // queue highlight if selected
            if (!string.IsNullOrEmpty(_nextHighlightMusicianId))
                mm.Highlight(_nextHighlightMusicianId, defaultHighlightMode);

            // Afinal full-band pass with seams flags ON
            mm.SetPostProcessingEnabled(composeEnablePostProcessing);

            // queue humanization for *this* song generation
            if (composeEnablePostProcessing)
            {
                mm.EnableHumanization(new MidiMusicManager.HumanizeOptions
                {
                    maxTickOffset = 6,   // ≈ six ticks timing jitter
                    velocityJitter = 12,  // ≈ ±12 velocity
                    lengthJitter = 6    // ≈ six ticks note-length jitter
                });
            }

            mm.SetPersonalityBiasEnabled(composeUsePersonalityBias);

            // Play the exact same SongData with NO subset (whole band)
            lastDuration = mm.Play(newSong);
            yield return MidiMusicManager.WaitForEnd();
            Debug.Log($"{DebugTag} Full-band pass ended.");

            // 6) New Song panel
            var newNames = orderedMusicians.Select(b => b.MusicianCharacterData.CharacterName)
                .ToArray();
            Debug.Log($"{DebugTag} Showing NewSongPanel for '{newSong.SongTitle}'");
            shipCanvas.NewSongPanel?.Show(newSong, newNames, lastDuration, onClose: () =>
            {
                Debug.Log($"{DebugTag} NewSongPanel closed, returning to map.");
                SetHandVisible(false);
                ReturnToMap();
            });

            isPlaying = false;
            Debug.Log($"{DebugTag} ComposeAndPreview: end");*/
        }

        private void OnRelax()
        {
            shipCanvas?.SetMainButtonsVisible(false);
            SetHandVisible(false);
            SetCompositionVisible(false);
            // TODO: restore stress to all band members
            ReturnToMap();
        }

        private void OnBandTalk()
        {
            shipCanvas?.SetMainButtonsVisible(false);
            SetHandVisible(false);
            SetCompositionVisible(false);

            var pd = GameManager.PersistentGameplayData;
            var gd = GameManager.GameplayData;

            if (pd.BandConflicts != null && pd.BandConflicts.Count > 0)
            {
                // Remove all current conflicts
                pd.BandConflicts.Clear();
            }
            else
            {
                // Restore cohesion (clamped)
                pd.BandCohesion = Mathf.Clamp(
                    pd.BandCohesion + gd.CohesionRestoredByBandTalk,
                    0, gd.MaxCohesion);
            }

            ReturnToMap();
        }

        private void ReturnToMap()
        {
            // Mark node completed (consume the Rehearsal) – adjust if you want revisits
            var pd = GameManager.PersistentGameplayData;
            var state = pd.CurrentSectorMapState;
            if (state != null)
            {
                var node = state.GetNode(pd.LastMapNodeId);
                if (node != null) node.Completed = true;
            }

            sceneChanger.OpenMapScene();
        }

        private void OnMetronomeToggled(bool enabled)
        {
            MidiMusicManager?.SetMetronomeEnabled(enabled);
        }

        public CharacterBase GetSelectedMusicianOrDefault()
        {
            // TODO: Use raycast or other method
            if (!string.IsNullOrEmpty(_nextHighlightMusicianId))
            {
                var found = _spawned.FirstOrDefault(m =>
                    m.MusicianCharacterData.CharacterId == _nextHighlightMusicianId);
                if (found) return found;
            }
            return _spawned.Count > 0 ? _spawned[0] : null;
        }

        public bool TryPlayCompositionCard(CardBase card, MusicianBase target)
        {
            var mm = MidiMusicManager.Instance;
            if (mm == null || card == null) return false;

            var c = card.CardData;
            var musicianId = target != null
                ? target.MusicianCharacterData.CharacterId
                : _nextHighlightMusicianId;

            // ---------- QUICK PRE-FILTER (role/musician sanity) ----------
            bool isTrackCard =
                c.CompositionType == CompositionCardType.Track_Rhythm ||
                c.CompositionType == CompositionCardType.Track_Backing ||
                c.CompositionType == CompositionCardType.Track_Bassline ||
                c.CompositionType == CompositionCardType.Track_Melody ||
                c.CompositionType == CompositionCardType.Track_Harmony;

            if (isTrackCard && target == null)
                return false; // needs a concrete musician

            if (isTrackCard && target != null)
            {
                bool isDrummer = IsDrummer(target);
                if (c.CompositionType == CompositionCardType.Track_Rhythm && !isDrummer) return false;
                if (c.CompositionType != CompositionCardType.Track_Rhythm && isDrummer) return false;
            }

            // ---------- CAN APPLY? (UI/model is the source of truth for session rules) ----------
            if (compositionUI != null && !compositionUI.CanApply(card, target, out var reason))
            {
                Debug.LogWarning($"[Rehearsal] Cannot play card: {reason}");
                return false;
            }

            // ---------- UPDATE UI / MODEL ----------
            bool uiApplied = compositionUI == null 
                || compositionUI.ApplyCard(card, target);

            // If UI accepted the card, consider it played.
            // (midiApplied can be false for TimeSig until the generator hook exists)
            return uiApplied;
        }

        // Helper: heuristic drummer detection based on profile instruments
        private bool IsDrummer(MusicianBase m)
        {
            var prof = m?.MusicianCharacterData?.Profile;
            if (prof == null) return false;

            bool inBacking = prof.backingInstruments != null &&
                             prof.backingInstruments.Exists(i => i.ToString().ToLower().Contains("drum"));
            bool inLead = prof.leadInstruments != null &&
                             prof.leadInstruments.Exists(i => i.ToString().ToLower().Contains("drum"));

            return inBacking || inLead;
        }

        private void SetHandVisible(bool visible)
        {
            if (shipHand != null)
            {
                shipHand.gameObject.SetActive(visible);
                if (visible) shipHand.EnableDragging();
                else shipHand.DisableDragging();
            }
        }

        private void RebindDeckToShipHand()
        {
            if (DeckManager.Instance != null && shipHand != null)
                DeckManager.Instance.SetHandController(shipHand);
        }

        private void SetCompositionVisible(bool visible)
        {
            if (compositionUI != null)
                compositionUI.gameObject.SetActive(visible);
        }

        private SongConfig BuildSongConfigFromCompositionUI()
        {
            if (compositionUI == null)
            {
                Log("[Jam] compositionUI is null.");
                return null;
            }

            var model = compositionUI.Model;
            if (model == null || model.parts.Count == 0)
            {
                Log("[Jam] No parts in composition model.");
                return null;
            }

            // Defensive refresh
            instrumentRepo.Refresh();
            patternRepo.Refresh();

            // Band ordering = current spawned list (used for channel ownership)
            var bandIds = _spawned.Select(m => m.MusicianCharacterData.CharacterId).ToList();
            Log($"[Jam] Band order (channels): [{string.Join(", ", bandIds)}]");

            var cfg = new SongConfig
            {
                ChannelMusicianOrder = bandIds,
                ChannelRoles = new List<TrackRole>(),
                Parts = new List<SongConfig.PartConfig>(),
                Structure = new List<SongConfig.PartSequenceEntry>()
            };

            var firstPartRoles = new List<TrackRole>();
            int partIndex = 0;

            foreach (var p in model.parts)
            {
                var tr = GetTempoRangeFromLabel(p.tempo);
                var ts = GetTimeSignatureFromLabel(p.timeSignature);
                var tonality = GetTonalityFromLabel(p.tonality);

                var part = new SongConfig.PartConfig
                {
                    Name = string.IsNullOrWhiteSpace(p.label) ? $"Part {partIndex + 1}" : p.label,
                    Measures = p.measures <= 0 ? 8 : p.measures,
                    TempoRange = tr,
                    TimeSignature = ts,
                    Tracks = new List<SongConfig.PartConfig.TrackConfig>(),
                    Tonality = tonality,
                    RootNote = Melanchall.DryWetMidi.MusicTheory.NoteName.C
                };

                Log($"[Jam] Building Part[{partIndex}] '{part.Name}'  " +
                    $"TS={p.timeSignature}  Tempo={p.tempo}  Measures={part.Measures}");

                Log($"[Jam] Part tonality: {part.Tonality} over {part.RootNote}", true);

                // one track per musician that has a placed card in this part
                foreach (var trModel in p.tracks)
                {
                    var role = GetRoleFromLabel(trModel.role);
                    var musicianId = trModel.musicianId;
                    if (string.IsNullOrEmpty(musicianId))
                    {
                        Log($"[Jam]   - Skipping track with empty musicianId (role {role}).");
                        continue;
                    }

                    var musician = _spawned.FirstOrDefault(m =>
                        m.MusicianCharacterData.CharacterId == musicianId);

                    MIDIInstrumentSO melInst = null;
                    MIDIPercussionInstrumentSO percInst = null;
                    PatternDataSO pattern = null;
                    IEnumerable<MIDIInstrumentSO> candidates = null;

                    RhythmRecipe recipe = null;
                    BackingRecipe backingRecipe = null;

                    switch (role)
                    {
                        case TrackRole.Rhythm:
                            percInst = instrumentRepo.GetPercussionInstruments()
                                .OrderBy(_ => rng.Next()).FirstOrDefault();

                            if (useProceduralRhythm)
                            {
                                pattern = null; // null tells RhythmTrackComposer to go procedural

                                recipe = new RhythmRecipe
                                {
                                    HatDensity = RhythmRecipe.HiHatDensity.From_Style,
                                    HatMode = RhythmRecipe.HatDensityMode.Fixed
                                };

                                Log($"[Jam] Rhythm: PROCEDURAL for " +
                                    $"mus={musicianId} " +
                                    $"kit='{percInst?.InstrumentName ?? "-"}'");
                            }
                            else
                            {
                                pattern = patternRepo.GetDrumPatterns(ts)
                                                     .OrderBy(_ => rng.Next()).FirstOrDefault();
                                Log($"[Jam] Rhythm: PATTERN='{pattern?.name ?? "-"}' " +
                                    $"for mus={musicianId} " +
                                    $"kit='{percInst?.InstrumentName ?? "-"}'");
                            }
                            break;

                        case TrackRole.Backing:

                            candidates = GetPermittedInstruments(musician, role).ToList();
                            melInst = candidates.OrderBy(_ => rng.Next()).FirstOrDefault();

                            if (useProceduralBacking)
                            {
                                pattern = null;

                                backingRecipe = new BackingRecipe
                                {

                                };

                                Log($"[Jam] Backing: PROCEDURAL for " +
                                    $"mus={musicianId} " +
                                    $"inst='{melInst?.InstrumentName ?? "-"}'", true);
                            }
                            else 
                            {
                                pattern = patternRepo.GetChordProgressions(ts)
                                    .OrderBy(_ => rng.Next()).FirstOrDefault();

                                Log($"[Jam] Backing: PATTERN='{pattern?.name ?? "-"}' " +
                                    $"for mus={musicianId} " +
                                    $"inst='{melInst?.InstrumentName ?? "-"}'");
                            }
                                
                            break;

                        case TrackRole.Bassline:

                            candidates = GetPermittedInstruments(musician, role).ToList();
                            melInst = candidates.OrderBy(_ => rng.Next()).FirstOrDefault();

                            pattern = patternRepo.GetAllMelodyPatterns()
                                .OrderBy(_ => rng.Next()).FirstOrDefault();
                            break;

                        case TrackRole.Melody:
                        case TrackRole.Harmony:

                            candidates = GetPermittedInstruments(musician, role).ToList();
                            melInst = candidates.OrderBy(_ => rng.Next()).FirstOrDefault();

                            pattern = patternRepo.GetAllMelodyPatterns()
                                .OrderBy(_ => rng.Next()).FirstOrDefault();
                            break;
                    }

                    var instName = melInst != null ? melInst.InstrumentName :
                                   percInst != null ? percInst.InstrumentName : "(none)";
                    var pattName = pattern != null ? pattern.name : "(none)";

                    Log($"[Jam] Track role={role} mus={musicianId} " +
                        $"inst='{instName}' patt='{pattName}'");

                    // Look up persistent per-musician gameplay state
                    var pd = GameManager.PersistentGameplayData;
                    var mgd = pd != null
                        ? pd.GetMusicianGameplayData(musicianId)
                        : null;

                    // BASELINE configs for this track = musician's current profile
                    MelodicLeadingConfig baseMelodicCfg =
                        (mgd != null && mgd.CurrentMelodicLeading != null)
                            ? mgd.CurrentMelodicLeading
                            : melodicConfig;

                    HarmonicLeadingConfig baseHarmonicCfg =
                        (mgd != null && mgd.CurrentHarmonicLeading != null)
                            ? mgd.CurrentHarmonicLeading
                            : harmonicConfig;

                    var baseMelodyId = melodyStrategyId;
                    var baseHarmonyId = harmonyStrategyId;

                    // CARD OVERRIDES from the composition UI model
                    var finalMelodicCfg = baseMelodicCfg;
                    var finalMelStrategyId = baseMelodyId;
                    var finalHarmonicCfg = baseHarmonicCfg;
                    var finalHarStrategyId = baseHarmonyId;

                    // Melody
                    if (trModel.hasMelodicLeadingOverride 
                        && trModel.melodicLeadingOverride != null)
                    {
                        finalMelodicCfg = trModel.melodicLeadingOverride;
                    }
                    if (trModel.hasMelodyStrategyOverride)
                    {
                        finalMelStrategyId = trModel.melodyStrategyIdOverride;
                    }

                    // Harmony
                    if (trModel.hasHarmonicLeadingOverride
                        && trModel.harmonicLeadingOverride != null)
                    {
                        finalHarmonicCfg = trModel.harmonicLeadingOverride;
                    }
                    if (trModel.hasHarmonyStrategyOverride)
                    {
                        finalHarStrategyId = trModel.harmonyStrategyIdOverride;
                    }

                    // Build track config
                    var tcfg = new SongConfig.PartConfig.TrackConfig
                    {
                        Role = role,
                        MusicianId = musicianId,
                        Instrument = melInst,
                        PercussionInstrument = percInst,
                        Parameters = new TrackParameters
                        {
                            Pattern = pattern,
                            RhythmRecipe = recipe,

                            melodyStrategyId = finalMelStrategyId,
                            melodicLeadingOverride = finalMelodicCfg,

                            harmonyStrategyId = finalHarStrategyId,
                            harmonicLeadingOverride = finalHarmonicCfg,
                        }
                    };

                    part.Tracks.Add(tcfg);

                    // Remember roles present in Part 0 to seed ChannelRoles (layout)
                    if (cfg.Parts.Count == 0)
                        firstPartRoles.Add(role);
                }

                cfg.Parts.Add(part);

                // Structure: by default add this part once
                cfg.Structure.Add(new SongConfig.PartSequenceEntry
                {
                    PartIndex = partIndex,
                    RepeatCount = 1
                });

                partIndex++;
            }

            // If ChannelRoles not provided yet, seed it from the first part’s roles
            if (cfg.ChannelRoles.Count == 0)
                cfg.ChannelRoles.AddRange(firstPartRoles);

            // Final trace
            Log($"[Jam] Built SongConfig: parts={cfg.Parts.Count} " +
                $"structure={cfg.Structure.Count}");
            for (int i = 0; i < cfg.Parts.Count; i++)
            {
                var p = cfg.Parts[i];
                var tracks = p.Tracks?.Select(t =>
                    $"{t.Role}/mus={t.MusicianId}" +
                    $"/inst={(t.Instrument ? t.Instrument.InstrumentName : t.PercussionInstrument ? t.PercussionInstrument.InstrumentName : "-")}/pat={(t.Parameters?.Pattern ? t.Parameters.Pattern.name : "-")}");
                
                Log($"[Jam]   Part[{i}] '{p.Name}' TS={p.TimeSignature} " +
                    $"meas={p.Measures} " +
                    $"tracks=[{string.Join(" | ", tracks ?? Array.Empty<string>())}]");
            }

            return cfg;
        }

        // TODO: Standarize, move to correct class, etc
        private TempoRange GetTempoRangeFromLabel(string label)
        {
            switch (label)
            {
                case "Slow": return TempoRange.Slow;
                case "Fast": return TempoRange.Fast;
                case "Very Fast": return TempoRange.VeryFast;
                default: return TempoRange.Moderate;
            }
        }

        private TimeSignature GetTimeSignatureFromLabel(string label)
        {
            switch (label)
            {
                case "4/4": return TimeSignature.FourFour;
                case "3/4": return TimeSignature.ThreeFour;
                case "6/8": return TimeSignature.SixEight;
                case "5/4": return TimeSignature.FiveFour;
                default: return TimeSignature.FourFour;
            }
        }

        private Tonality GetTonalityFromLabel(string label)
        {
            switch (label)
            {
                case "Ionian":      return Tonality.Ionian;
                case "Dorian":      return Tonality.Dorian;
                case "Phrygian":    return Tonality.Phrygian;
                case "Lydian":      return Tonality.Lydian;
                case "Mixolydian":  return Tonality.Mixolydian;
                case "Aeolian":     return Tonality.Aeolian;
                case "Locrian":     return Tonality.Locrian;
                default:            return Tonality.Ionian;
            }
        }

        private TrackRole GetRoleFromLabel(string label)
        {
            switch (label)
            {
                case "Rhythm": return TrackRole.Rhythm;
                case "Backing": return TrackRole.Backing;
                case "Bassline": return TrackRole.Bassline;
                case "Melody": return TrackRole.Melody;
                case "Harmony": return TrackRole.Harmony;
                default: return TrackRole.Melody;
            }
        }

        private void ResetAfterPlayback()
        {
            BeginRehearsalSession();
        }

        // Returns all melodic instruments allowed by this musician
        private IEnumerable<MIDIInstrumentSO> GetPermittedInstruments(MusicianBase musician)
        {
            // Defensive: if anything is missing, fall back to all melodic instruments.
            var allMelodic = instrumentRepo.GetMelodicInstruments();
            if (musician == null || musician.MusicianCharacterData == null)
                return allMelodic;

            var prof = musician.MusicianCharacterData.Profile; // MusicianProfileData
            if (prof == null) return allMelodic;

            var allowedTypes = new HashSet<InstrumentType>();
            if (prof.backingInstruments != null)
                foreach (var t in prof.backingInstruments) allowedTypes.Add(t);
            if (prof.leadInstruments != null)
                foreach (var t in prof.leadInstruments) allowedTypes.Add(t);

            var filtered = allMelodic.Where(i => allowedTypes.Contains(i.InstrumentType)).ToList();

            // If the profile was empty or yielded nothing, don't block the flow.
            return filtered.Count > 0 ? filtered : allMelodic;
        }

        // Role-aware
        private IEnumerable<MIDIInstrumentSO> GetPermittedInstruments(
            MusicianBase musician, TrackRole role)
        {
            var allMelodic = instrumentRepo.GetMelodicInstruments();
            if (musician == null || musician.MusicianCharacterData == null) return allMelodic;

            var prof = musician.MusicianCharacterData.Profile;
            if (prof == null) return allMelodic;

            // Choose the primary list based on role, with a secondary fallback.
            List<InstrumentType> primary = null;
            List<InstrumentType> secondary = null;

            switch (role)
            {
                case TrackRole.Backing:
                case TrackRole.Bassline:   // many bands treat bass as part of the backline
                    primary = prof.backingInstruments;
                    secondary = prof.leadInstruments;
                    break;

                case TrackRole.Melody:
                case TrackRole.Harmony:
                    primary = prof.leadInstruments;
                    secondary = prof.backingInstruments;
                    break;

                default:
                    primary = prof.backingInstruments;
                    secondary = prof.leadInstruments;
                    break;
            }

            IEnumerable<MIDIInstrumentSO> FilterBy(List<InstrumentType> list) =>
                (list == null || list.Count == 0)
                    ? Enumerable.Empty<MIDIInstrumentSO>()
                    : allMelodic.Where(i => list.Contains(i.InstrumentType));

            var filtered = FilterBy(primary).ToList();
            if (filtered.Count == 0) filtered = FilterBy(secondary).ToList();

            return filtered.Count > 0 ? filtered : allMelodic;
        }
    }
}
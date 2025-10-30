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

        #region Jam
        private enum JamState
        {
            Idle,                   // Not in a jam session
            BuildingCurrentPart,    // Player is building the first (or current) part before any playback
            PlayingCurrentPart,     // A confirmed part is currently playing in loop
            BuildingNextPart,       // While the current part is looping, player is drafting the next part
            Ended                   // Jam is over
        }

        [Serializable]
        public class JamRules
        {
            // How many loops each part should play before we either transition or stop
            public int loopsPerPart = 3;

            // How many cards to draw at the start of each build phase
            public int drawPerPart = 5;

            // How much "energy" the player has to spend on cards each build phase
            public int inspirationPerPart = 3;
        }

        [Serializable]
        private sealed class PartCache
        {
            public byte[] mergedBytes;
            public float seconds;
            public Dictionary<string, byte[]> stemsByMusician = new(); // future: micro-variations / single-track replace
        }

        #endregion

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> musicianPosList;

        [Header("Jam Rules / Runtime")]
        [SerializeField] private JamRules jamRules = new JamRules();

        [Header("Cards / Hand")]
        [SerializeField] private HandController shipHand;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera handCamera;

        [Header("Composition")]
        [SerializeField] private SongCompositionUI compositionUI;
        // TODO: Access through SongCompositionUI? Or not?
        [SerializeField] private LoopsTimerUI loopsTimerUI;
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

        // Jamming
        private JamState jamState = JamState.Idle;
        private int currentInspiration = 0;
        private int buildingPartInspirationPerLoop = 0; // sum of GrooveGenerated while drafting part
        private int currentPartInspiration = 0;
        private int loopsRemainingForCurrentPart = 0;
        private int loopsTotalForCurrentPart = 0;
        private int currentPartIndex = -1;
        private bool nextPartIsReady = false;

        private float currentLoopStartTime = 0f;
        private float currentLoopDurationSeconds = 0f;


        private readonly List<MusicianBase> _spawned = new();
        public SongCompositionUI.SongModel GetCurrentComposition() => compositionUI?.Model;
        private readonly Dictionary<int, PartCache> _partCache = new();

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

        private void Update()
        {
            // Only care about jam looping logic if we are mid-jam
            if (!inRehearsal) return;
            if (jamState != JamState.BuildingNextPart &&
                jamState != JamState.PlayingCurrentPart) return;

            var mm = MidiMusicManager;
            if (mm == null) return;

            bool midiIsPlaying = mm.IsAnySongPlaying();

            if (!midiIsPlaying && isPlaying)
            {
                HandleLoopFinished();
            }

            // Update loop progress UI in real time
            if (midiIsPlaying && isPlaying && loopsTimerUI != null 
                && currentLoopDurationSeconds > 0f)
            {
                // How much of the CURRENT loop run has elapsed?
                float elapsed = Time.time - currentLoopStartTime;
                float pct = Mathf.Clamp01(elapsed / currentLoopDurationSeconds);

                // Which numbered loop are we in right now (0-based)?
                int loopsCompleted = loopsTotalForCurrentPart - loopsRemainingForCurrentPart;
                int currentLoopIdx0 = Mathf.Max(0, loopsCompleted);
                loopsTimerUI.SetProgress(currentLoopIdx0, pct);
            }
        }

        /// <summary>
        /// Called when one pass of the currently playing part just ended.
        /// Handles decrementing loopsRemainingForCurrentPart, deciding whether to replay,
        /// or transition to the next part / end the jam.
        /// </summary>
        private void HandleLoopFinished()
        {
            isPlaying = false;

            loopsRemainingForCurrentPart--;

            // Grant per-loop Inspiration now
            if (currentPartInspiration > 0)
            {
                currentInspiration += currentPartInspiration;
                compositionUI?.SetInspiration(currentInspiration);
                compositionUI?.SetPlusInspiration(currentPartInspiration);
                Log($"[Jam] Awarded +{currentPartInspiration} " +
                    $"Inspiration for loop completion. Total={currentInspiration}");
            }

            Log("[Jam] Loop finished. loopsRemainingForCurrentPart=" 
                + loopsRemainingForCurrentPart);

            if (loopsRemainingForCurrentPart > 0)
            {
                // Replay the same part again
                PlaySinglePartLoop(currentPartIndex);
                return;
            }

            // No loops left for this part.
            // If we already have a next part confirmed and ready (nextPartIsReady),
            // we advance to that part and reset loop counters.
            if (nextPartIsReady)
            {
                AdvanceToNextPart();
                return;
            }

            // Otherwise, end the jam.
            EndJam();
        }

        /// <summary>
        /// Move from the old confirmed part (A) to the newly confirmed next part (B),
        /// reset the loop counter, start looping B, and give the player cards/energy
        /// to draft the following part (C).
        /// </summary>
        private void AdvanceToNextPart()
        {
            Log("[Jam] TODO: AdvanceToNextPart()");

            // 1. currentPartIndex++
            //    currentPartIndex = currentPartIndex + 1;

            // 2. loopsRemainingForCurrentPart = jamRules.loopsPerPart;
            //    loopsTotalForCurrentPart    = jamRules.loopsPerPart;

            // 3. float secs = PlaySinglePartLoop(currentPartIndex);
            //    currentLoopDurationSeconds = secs;
            //    currentLoopStartTime = Time.time;

            // 4. loopsTimerUI.BuildBars(loopsTotalForCurrentPart);
            //    loopsTimerUI.SetProgress(0, 0f);

            // 5. PrepareRehearsalDeck();
            //    currentEnergy = jamRules.energyPerPart;
            //    jamState = JamState.BuildingNextPart;

            // NOTE: this depends on SongCompositionUI having a concept of
            // "the next drafted part" being committed into Model.parts.
        }

        /// <summary>
        /// Ends the jam session:
        /// - Hide hand and composition UI
        /// - Reset state machine
        /// - Optionally go back to map or just idle in the ship
        /// </summary>
        private void EndJam()
        {
            Log("[Jam] Ending Jam.");

            jamState = JamState.Ended;
            inRehearsal = false;
            isPlaying = false;

            SetHandVisible(false);
            SetCompositionVisible(false);

            if (loopsTimerUI != null)
            {
                loopsTimerUI.ClearProgress();
            }

            // For MVP we just stop and leave you in the ship.
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
            if (inRehearsal || jamState != JamState.Idle)
                return;

            shipCanvas?.SetMainButtonsVisible(false);
            //BeginRehearsalSession();
            StartJam();
        }

        private void OnPlayPressed()
        {
            /*
            if (isPlaying) return;
            if (!inRehearsal) return;

            SetHandVisible(false);
            SetCompositionVisible(false);

            // Go play using the already-queued intents (Intro/Solo/Outro/Tempo/ReplaceTrack)
            StartCoroutine(ComposeAndPreviewRoutine());*/

            if (!inRehearsal) return;

            // Case 1: first time we press Play
            if (jamState == JamState.BuildingCurrentPart)
            {
                // lock Part A and start looping it
                ConfirmCurrentPartAndStartPlaying();
                return;
            }

            // Case 2: later presses may confirm the drafted "next part"
            // TODO: wire this in once SongCompositionUI supports draftPart.
            if (jamState == JamState.BuildingNextPart)
            {
                // TODO: ConfirmNextDraftPartAndQueueForPlayback();
                Log("[Jam] TODO: confirm next drafted part on Play press.");
                return;
            }

            // Fallback / legacy behavior (non-jam preview path)
            if (jamState == JamState.Idle)
            {
                if (isPlaying) return;

                // Legacy path: hide hand & composition and preview the entire song in one go.
                SetHandVisible(false);
                SetCompositionVisible(false);
                StartCoroutine(ComposeAndPreviewRoutine());
                return;
            }
        }

        /// <summary>
        /// Begins an interactive Jam session:
        /// - Shows the composition panel and the hand
        /// - Draws the initial hand
        /// - Resets energy
        /// - Prepares Part A as an editable part in SongCompositionUI
        /// State after this call: BuildingCurrentPart
        /// </summary>
        private void StartJam()
        {
            Log("[Jam] StartJam()", true);

            inRehearsal = true; // legacy flag
            jamState = JamState.BuildingCurrentPart;

            SetHandVisible(true);
            SetCompositionVisible(true);

            if (compositionUI) compositionUI.ResetSession();
            compositionUI?.PopulateMusicianIcons(_spawned);

            PrepareRehearsalDeck();

            currentInspiration = jamRules.inspirationPerPart;
            compositionUI?.SetInspirationVisible(true);
            compositionUI?.SetInspiration(currentInspiration);
            buildingPartInspirationPerLoop = 0;
            compositionUI?.SetPlusInspiration(0);

            if (loopsTimerUI != null)
            {
                loopsTimerUI.ClearProgress();
                loopsTimerUI.SetBarsVisible(false);
            }

            Log("[Jam] Now in BuildingCurrentPart. " +
                "Player can play composition cards into the first part.");
        }

        /// <summary>
        /// Player pressed Play for the first time:
        /// Lock in the current built part as Part[0],
        /// start looping it for jamRules.loopsPerPart,
        /// and transition to PlayingCurrentPart / BuildingNextPart phase.
        /// </summary>
        private void ConfirmCurrentPartAndStartPlaying()
        {
            if (jamState != JamState.BuildingCurrentPart)
            {
                Log("[Jam] ConfirmCurrentPartAndStartPlaying() ignored: " +
                    "not in BuildingCurrentPart");
                return;
            }

            Log("[Jam] Confirming first part and starting loop playback.");

            currentPartIndex = 0;
            loopsTotalForCurrentPart = jamRules.loopsPerPart;
            loopsRemainingForCurrentPart = jamRules.loopsPerPart;

            // start playback of Part[0] and remember its duration
            float loopSeconds = PlaySinglePartLoop(currentPartIndex);
            if (loopSeconds <= 0f)
            {
                Log("[Jam] Failed to start first loop.");
                return;
            }

            currentLoopDurationSeconds = loopSeconds;
            currentLoopStartTime = Time.time;

            // build and show the timeline
            if (loopsTimerUI != null)
            {
                loopsTimerUI.BuildBars(loopsTotalForCurrentPart);
                loopsTimerUI.SetProgress(loopIndex0: 0, loopProgress01: 0f);
                loopsTimerUI.SetBarsVisible(true);
            }

            var playingPart = compositionUI.Model.parts[currentPartIndex];
            currentPartInspiration = EvaluatePerLoopInspirationGain(playingPart);
            compositionUI?.SetPlusInspiration(currentPartInspiration);
            buildingPartInspirationPerLoop = 0; // reset for drafting next part

            //  Part A is looping and the player is drafting Part B.
            jamState = JamState.BuildingNextPart;

            // Give the player new energy + new cards for drafting the NEXT part
            currentInspiration = jamRules.inspirationPerPart;
            PrepareRehearsalDeck();
            compositionUI?.SetInspiration(currentInspiration);

            // nextPartIsReady = false; // they haven't confirmed Part B yet
            Log("[Jam] Now looping Part 0 and allowing the player to draft the next part.");
        }

        /*
        /// <summary>
        /// Build a SongConfig that only contains the requested partIndex,
        /// tell MidiMusicManager to play it once,
        /// and return the duration (in seconds) of that generated part.
        /// Also flips isPlaying = true so Update() can watch for loop end.
        /// </summary>
        private float PlaySinglePartLoop(int partIndex)
        {
            var mm = MidiMusicManager.Instance;
            if (mm == null)
            {
                Debug.LogError($"{DebugTag} MidiMusicManager is null");
                return 0f;
            }

            var fullCfg = BuildSongConfigFromCompositionUI();
            if (fullCfg == null)
            {
                Debug.LogError($"{DebugTag} BuildSongConfigFromCompositionUI() " +
                    $"returned null or empty.");
                return 0f;
            }

            if (partIndex < 0 || partIndex >= fullCfg.Parts.Count)
            {
                Debug.LogError($"{DebugTag} Invalid partIndex " + partIndex);
                return 0f;
            }

            // Create a shallow "single-part" cfg
            var singleCfg = new SongConfig
            {
                ChannelMusicianOrder    = fullCfg.ChannelMusicianOrder.ToList(),
                ChannelRoles            = fullCfg.ChannelRoles.ToList(),
                Parts                   = new List<SongConfig.PartConfig>(),
                Structure               = new List<SongConfig.PartSequenceEntry>()
            };

            // Copy ONLY the partIndex we want
            singleCfg.Parts.Add(fullCfg.Parts[partIndex]);
            singleCfg.Structure.Add(new SongConfig.PartSequenceEntry
            {
                PartIndex   = 0,
                RepeatCount = 1
            });

            midiGenPlayConfig.defaultSeed = 
                UnityEngine.Random.Range(int.MinValue, int.MaxValue);

            var partName = compositionUI.Model.parts[partIndex].label;
            float seconds = mm.PlayFromConfig(singleCfg, partName, _spawned);
            if (seconds <= 0f)
            {
                Debug.LogError($"{DebugTag} " +
                    $"Failed to start playback for partIndex " + partIndex);
                return 0f;
            }

            mm.SetChannelOwners(singleCfg.ChannelMusicianOrder.ToList());

            isPlaying = true;

            // Reset timing info for this new loop iteration
            currentLoopStartTime = Time.time;
            currentLoopDurationSeconds = seconds;

            Log("[Jam] Playing single part '" + partName + "' " +
                "(" + partIndex + ") once. Duration " + seconds + "s");

            return seconds;
        }
        */

        /// <summary>
        /// Play exactly one iteration of the given part index.
        /// Uses a per-part cache (merged MIDI bytes + seconds). If the cache doesn't exist,
        /// render the part once via MidiMusicManager and store it.
        /// Returns the part duration in seconds (0 on failure).
        /// </summary>
        private float PlaySinglePartLoop(int partIndex)
        {
            var mm = MidiMusicManager.Instance;
            if (mm == null)
            {
                Debug.LogError($"{DebugTag} MidiMusicManager is null");
                return 0f;
            }

            // Build the full config from the current UI/model
            var fullCfg = BuildSongConfigFromCompositionUI();
            if (fullCfg == null)
            {
                Debug.LogError($"{DebugTag} BuildSongConfigFromCompositionUI() returned null or empty.");
                return 0f;
            }

            if (partIndex < 0 || partIndex >= fullCfg.Parts.Count)
            {
                Debug.LogError($"{DebugTag} Invalid partIndex " + partIndex);
                return 0f;
            }

            // Ensure we have a cached render for this part
            if (!_partCache.TryGetValue(partIndex, out var cache) || cache == null || cache.mergedBytes == null || cache.mergedBytes.Length == 0)
            {
                // Render exactly one repetition of this part, using the session's channel layout
                var (merged, stems, seconds) = mm.RenderSinglePart(fullCfg, partIndex);
                if (merged == null || merged.Length == 0 || seconds <= 0f)
                {
                    Debug.LogError($"{DebugTag} RenderSinglePart failed for partIndex {partIndex}");
                    return 0f;
                }

                cache = new PartCache
                {
                    mergedBytes = merged,
                    seconds = seconds,
                    stemsByMusician = stems ?? new Dictionary<string, byte[]>()
                };
                _partCache[partIndex] = cache;
            }

            // Replay the same bytes for this loop iteration
            var partName = compositionUI.Model.parts[partIndex].label;
            var duration = mm.PlayRaw(cache.mergedBytes, cache.seconds, $"Jam Part {partIndex} (cached:{partName})");
            if (duration <= 0f) return 0f;

            isPlaying = true;
            currentLoopStartTime = Time.time;
            currentLoopDurationSeconds = duration;

            Log($"[Jam] Looping cached part '{partName}' ({partIndex}) once. Duration {duration:0.###}s");
            return duration;
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

            // ---------- INSPIRATION: block if not enough ----------
            // (Only for Composition cards; Action cards are Gig-only.)
            if (c.IsComposition)
            {
                int cost = Mathf.Max(0, c.GrooveCost);   // stored cost field
                if (cost > currentInspiration)
                {
                    Debug.LogWarning($"{DebugTag} Not enough Inspiration: " +
                        $"need {cost}, have {currentInspiration}. " +
                        $"Card '{c.CardName}' was not played.");
                    return false;
                }
            }

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
                if (c.CompositionType == CompositionCardType.Track_Rhythm && !isDrummer) 
                    return false;
                if (c.CompositionType != CompositionCardType.Track_Rhythm && isDrummer) 
                    return false;
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
            if (!uiApplied) return false;

            // ---------- INVALIDATE CURRENT PART CACHE IF NEEDED ----------
            // Playing a Track_* card while the current part is looping means
            // the part's content has changed. Nuke the cache so the next loop
            // will regenerate and include the new/updated track.
            bool playedTrackCard =
                c.CompositionType == CompositionCardType.Track_Rhythm ||
                c.CompositionType == CompositionCardType.Track_Backing ||
                c.CompositionType == CompositionCardType.Track_Bassline ||
                c.CompositionType == CompositionCardType.Track_Melody ||
                c.CompositionType == CompositionCardType.Track_Harmony;

            if (playedTrackCard && jamState == JamState.PlayingCurrentPart)
            {
                InvalidatePartCache(currentPartIndex);
            }

            bool editedStructureCard =
                c.CompositionType == CompositionCardType.TimeSignature_4_4 ||
                c.CompositionType == CompositionCardType.TimeSignature_3_4 ||
                c.CompositionType == CompositionCardType.TimeSignature_6_8 ||
                c.CompositionType == CompositionCardType.TimeSignature_5_4 ||
                c.CompositionType == CompositionCardType.Tempo_Slow ||
                c.CompositionType == CompositionCardType.Tempo_Fast ||
                c.CompositionType == CompositionCardType.Tempo_VeryFast ||
                c.CompositionType == CompositionCardType.Tonality_Ionian ||
                c.CompositionType == CompositionCardType.Tonality_Dorian ||
                c.CompositionType == CompositionCardType.Tonality_Phrygian ||
                c.CompositionType == CompositionCardType.Tonality_Lydian ||
                c.CompositionType == CompositionCardType.Tonality_Mixolydian ||
                c.CompositionType == CompositionCardType.Tonality_Aeolian ||
                c.CompositionType == CompositionCardType.Tonality_Locrian;

            if (editedStructureCard && jamState == JamState.PlayingCurrentPart)
            {
                InvalidatePartCache(currentPartIndex);
            }

            // ---------- INSPIRATION: spend on success + update UI ----------
            if (c.IsComposition)
            {
                int cost = Mathf.Max(0, c.GrooveCost);
                currentInspiration = Mathf.Max(0, currentInspiration - cost);
                compositionUI?.SetInspiration(currentInspiration);
                Debug.Log($"{DebugTag} Played '{c.CardName}' for {cost} Inspiration. " +
                    $"Remaining: {currentInspiration}.");

                int gen = Mathf.Max(0, c.GrooveGenerated);
                if (gen > 0)
                {
                    buildingPartInspirationPerLoop += gen;
                    Debug.Log($"{DebugTag} This part now generates " +
                        $"+{buildingPartInspirationPerLoop} Inspiration per loop.");
                }
            }

            if (playedTrackCard 
                && jamState != JamState.BuildingCurrentPart 
                && compositionUI != null)
            {
                if (currentPartIndex >= 0 // Safety: if no part is playing yet, do nothing 
                    && currentPartIndex < compositionUI.Model.parts.Count)
                {
                    currentPartInspiration = EvaluatePerLoopInspirationGain(
                        compositionUI.Model.parts[currentPartIndex]);

                    compositionUI.SetPlusInspiration(currentPartInspiration);
                    Log($"[Jam] Recalculated per-loop Inspiration = " +
                        $"+{currentPartInspiration} after track update.");
                }
            }

            return true;
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

        private int EvaluatePerLoopInspirationGain(SongCompositionUI.PartEntry part)
        {
            if (part == null || part.tracks == null) return 0;
            // MVP rule: sum grooveGenerated for each active track in this part
            int sum = 0;
            foreach (var t in part.tracks)
                sum += Mathf.Max(0, t.inspirationGenerated);
            return sum;
        }

        /// <summary>
        /// Drop the cached render for a given part so the next loop regenerates it.
        /// Safe to call even if nothing is cached.
        /// </summary>
        private void InvalidatePartCache(int partIndex)
        {
            if (_partCache == null) return;
            if (_partCache.Remove(partIndex))
                Debug.Log($"{DebugTag} Invalidated cached MIDI for part #{partIndex}.");
        }
    }
}
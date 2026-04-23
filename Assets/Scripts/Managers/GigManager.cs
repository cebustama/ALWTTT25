using ALWTTT.Backgrounds;
using ALWTTT.Cards;
using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Encounters;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Music;
using ALWTTT.UI;
using ALWTTT.Utils;
using ALWTTT.Status;

using MidiGenPlay;
using MidiGenPlay.Services;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ALWTTT.Managers.GigRunContext;

#if ALWTTT_DEV
using ALWTTT.DevMode;
#endif

namespace ALWTTT.Managers
{
    public class GigManager : MonoBehaviour
    {
        public const string DebugTag = "<color=magenta>GigManager:</color>";

        public static GigManager Instance;

        [Header("Composition Rules")]
        [SerializeField] private JamRules jamRules = new JamRules();
        [SerializeField] private float maxSongHype = 100f;
        [SerializeField, Tooltip("Raw starting SongHype points for each new song.")]
        private float startingSongHype = 10f;

        [Header("Cards / Hand")]
        [SerializeField] private HandController gigHand;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera handCamera;

        [Header("MVP / Action Card Gating")]
        [SerializeField, Tooltip("MVP: when Play is pressed, discard remaining Action cards from the hand.")]
        private bool discardActionCardsOnPlay = true;

        [SerializeField, Tooltip("Allow Action cards with timing=Always to be playable during performance.")]
        private bool allowActionCardsDuringPerformance = false;

        [Header("Vibe / Hype Balancing")]
        [SerializeField] private int maxVibeFromSongHype = 20;
        [SerializeField] private float songHypeDeltaMultiplier = 1f;

        [Header("Flow / Composure (MVP)")]
        [SerializeField, Tooltip("(Legacy) Each Flow stack increases loop→SongHype delta by (1 + stacks * this).")]
        private float flowSongHypeMultiplierPerStack = 0.10f;

        [SerializeField, Tooltip("(Legacy) If enabled, Flow stacks multiply loop→SongHype delta. Disable if using Strength-like Flow→Vibe.")]
        private bool flowAffectsSongHype = false;

        [SerializeField] private bool debugFlowSongHype = false;

        [Header("Flow → Vibe (Strength-like MVP)")]
        [SerializeField, Tooltip("If enabled, each Flow stack adds a flat Vibe bonus to *any* positive Vibe gain (cards + song resolution).")]
        private bool flowAddsFlatVibeBonus = true;

        [SerializeField, Tooltip("Flat Vibe bonus per Flow stack (applied when flowAddsFlatVibeBonus is enabled).")]
        private int flowVibeFlatBonusPerStack = 1;

        public bool FlowAddsFlatVibeBonus => flowAddsFlatVibeBonus;
        public int FlowVibeFlatBonusPerStack => flowVibeFlatBonusPerStack;

        [Header("Breakdown")]
        [SerializeField, Range(0f, 1f)] private float breakdownStressResetFraction = 0.5f;
        public float BreakdownStressResetFraction => breakdownStressResetFraction;

        [Header("Gig End Behavior")]
        [SerializeField] private bool skipAudienceActionsAfterFinalSong = true;

        [Header("Audience Beat Response")]
        [SerializeField]
        private AnimationCurve audienceJumpIntensityCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField][Range(0f, 1f)] private float audienceJumpThreshold = 0.1f;

        [Header("Animation and Effects")]
        [SerializeField] private int idleBpm = 120;

        [Header("Timing")]
        [SerializeField] private float songEndPause = 3f;
        [SerializeField] private float perAudienceVibeDelay = 1f;
        [SerializeField] private float perAudienceActionDelay = 1f;
        [SerializeField] private float barFillDelay = 3f;

        [Header("Composition UI")]
        [SerializeField] private SongCompositionUI compositionUI;
        [SerializeField] private LoopsTimerUI loopsTimerUI;
        [SerializeField] private MidiGenPlayConfig midiGenPlayConfig;

        [Header("References")]
        [SerializeField] private BackgroundContainer backgroundContainer;
        [SerializeField] private List<Transform> musicianPosList;
        [SerializeField] private List<Transform> audienceMemberPosList;
        [SerializeField] private SceneChanger sceneChanger;

        [Header("Dev / Composition")]
        [SerializeField] private bool useLogs = true;
        [SerializeField] private bool useCompositionLogs = true;
        [SerializeField] private bool debugSongHype = false;
        [SerializeField] private Slider songHypeDebugSlider;

        [Header("Dev / Instruments")]
        [SerializeField] private bool debugInstrumentPicker = false;
        [SerializeField] private bool debugMusicianVolume = false;

        private GigPhase currentGigPhase;
        private List<SongData> playedSongs = new List<SongData>();

        private readonly List<MusicianBase> _spawned = new();

        private System.Random _rng = new System.Random();

        private bool _isSongPlaying;
        private bool _isBetweenSongs;
        private bool _actionWindowOpen = true;
        private bool _returningToMap = false;

        private int _currentBpm;

        // PartIndex -> (AudienceIndex -> List of impressions per loop)
        private readonly Dictionary<int, Dictionary<int, List<int>>>
            _audienceLoopImpressionsByPart = new();

        // Enriched part feedback (with audience data) for the current song
        private readonly List<PartFeedbackContext> _gigPartsForCurrentSong =
            new List<PartFeedbackContext>();

        private int _requiredSongCount = 1;

        private float _songHype;

        private SongFeedbackContext? _lastSongFeedback;

        private InstrumentRepositoryResources _instrumentRepo;

        private readonly Dictionary<string, float> _musicianVolume01
            = new Dictionary<string, float>();

        #region Encapsulation / Cache
        public GigEncounter CurrentGigEncounter { get; private set; }

        public List<MusicianBase> CurrentMusicianCharacterList
        {
            get;
            private set;
        } = new List<MusicianBase>();

        public List<AudienceCharacterBase> CurrentAudienceCharacterList
        {
            get;
            private set;
        } = new List<AudienceCharacterBase>();

        private GameManager GameManager => GameManager.Instance;
        private DeckManager DeckManager => DeckManager.Instance;
        private UIManager UIManager => UIManager.Instance;
        private MidiMusicManager MidiMusicManager => MidiMusicManager.Instance;

        private bool IsGigComplete
        {
            get
            {
#if ALWTTT_DEV
                if (DevModeController.InfiniteTurnsEnabled) return false;
#endif
                return GameManager.PersistentGameplayData.CurrentSongIndex >= _requiredSongCount;
            }
        }

        public List<Transform> MusicianPosList => musicianPosList;
        public List<Transform> AudienceMemberPosList => audienceMemberPosList;

        // TODO: Implement dual target cards Musician -> Audience Character
        public MusicianBase SelectedMusician =>
            CurrentMusicianCharacterList.Count > 0 ?
            CurrentMusicianCharacterList[0] : null;

        public GigPhase CurrentGigPhase
        {
            get => currentGigPhase;
            private set
            {
                ExecuteGigPhase(value);
                currentGigPhase = value;
            }
        }

        public float SongHype => _songHype;
        public float SongHype01 =>
            maxSongHype <= 0f ? 0f : Mathf.Clamp01(_songHype / maxSongHype);
        public int RequiredSongCount => _requiredSongCount;

        public int SongsLeft
        {
            get
            {
                var pd = GameManager.PersistentGameplayData;
                return Mathf.Max(0, _requiredSongCount - pd.CurrentSongIndex);
            }
        }
        #endregion

        #region Callbacks

        public Action OnPlayerTurnStarted;
        public Action OnSongPerformanceStarted;
        public Action OnEnemyTurnStarted;

        public event Action<float> OnSongHypeChanged01;
        public event Action<int, int> OnSongsLeftChanged; // (songsLeft, requiredSongCount)

        // Gig-level events that expose *enriched* contexts
        public event Action<PartFeedbackContext> OnGigPartFeedbackReady;
        public event Action<SongFeedbackContext> OnGigSongFeedbackReady;

        #endregion

        #region Composition
        private CompositionSession _session;

        private class GigContext : ICompositionContext
        {
            private readonly GigManager _host;
            public GigContext(GigManager host) { _host = host; }

            public SongCompositionUI CompositionUI => _host?.compositionUI;
            public LoopsTimerUI LoopsTimerUI => _host?.loopsTimerUI;
            public DeckManager Deck => DeckManager.Instance;
            public MidiMusicManager Music => MidiMusicManager.Instance;
            public IReadOnlyList<MusicianBase> Band => _host.CurrentMusicianCharacterList;

            public void ShowCompositionUI(bool visible) =>
                _host?.compositionUI?.gameObject.SetActive(visible);

            public void ShowHand(bool visible) => _host?.SetHandVisible(visible);

            public MusicianBase ResolveMusicianByType(MusicianCharacterType type) =>
                _host.ResolveMusicianByType(type);

            public MusicianBase ResolveMusicianById(string id) =>
                _host.CurrentMusicianCharacterList.FirstOrDefault(m =>
                    m && m.MusicianCharacterData.CharacterId == id);

            public bool TryGetPartCache(
                int partIndex, out CompositionSession.PartCache cache)
            {
                cache = null;
                if (_host._session == null) return false;
                return _host._session.TryGetPartCache(partIndex, out cache);
            }

            public CompositionSession.PartCache GetOrCreatePartCache(int partIndex)
            {
                if (_host._session == null)
                    return new CompositionSession.PartCache();

                return _host._session.GetOrCreatePartCache(partIndex);
            }

            public void OnSessionStarted()
            {
                _host.Log($"[GigContext] Session started.");
            }

            public void OnSessionEnded()
            {
                _host.OnCompositionSessionEnded();
            }

            public void Log(string msg, bool highlight = false) =>
                _host.Log(msg, highlight);

            public void OnPartBpmResolved(int partIndex, int bpm)
            {
                _host.ApplyBpmToStage(partIndex, bpm);
            }
        }

        #endregion

        private void Log(string log, bool highlight = false, string customColor = "")
        {
            if (useLogs)
            {
                if (highlight)
                    Debug.Log($"{DebugTag} <color=yellow>{log}</color>");
                else if (!string.IsNullOrWhiteSpace(customColor))
                    Debug.Log($"{DebugTag} <color={customColor}>{log}</color>");
                else
                    Debug.Log($"{DebugTag} {log}");
            }
        }

        #region Setup
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
                currentGigPhase = GigPhase.PrepareGig;
            }

            // Init repositories
            if (midiGenPlayConfig != null)
            {
                _instrumentRepo = new InstrumentRepositoryResources(midiGenPlayConfig);
            }
        }

        private void Start()
        {
            StartGig();
            SetupSongHypeDebugUI();
        }

        private void OnDestroy()
        {
            foreach (var m in _spawned)
            {
                if (m != null) m.UnbindFromGigContext();
            }
        }

        private void StartGig()
        {
            if (useLogs) Debug.Log($"{DebugTag} Starting gig...");

            SetupEncounter();

            _requiredSongCount = ResolveRequiredSongCount();

            var pd = GameManager.PersistentGameplayData;
            pd.CurrentInspiration = pd.InitialGigInspiration;

            Debug.Log($"{DebugTag} StartGig Inspiration init → " +
                $"TurnStartingInspiration={pd.TurnStartingInspiration}, " +
                $"CurrentInspiration={pd.CurrentInspiration}, " +
                $"KeepBetweenTurns={pd.KeepInspirationBetweenTurns}");

            Debug.Log($"{DebugTag} RequiredSongCount resolved = {_requiredSongCount} " +
                $"(PD.CurrentEncounter.NumberOfSongs=" +
                $"{GameManager.PersistentGameplayData.CurrentEncounter?.NumberOfSongs ?? -1}, " +
                $"PD.CurrentSongIndex={GameManager.PersistentGameplayData.CurrentSongIndex})");

            OnSongsLeftChanged?.Invoke(SongsLeft, _requiredSongCount);

            BuildBackground();
            BuildBand();
            BuildAudience();

            if (UIManager != null && UIManager.GigCanvas != null)
                UIManager.GigCanvas.gameObject.SetActive(true);

            // Deck + hand binding
            SetupDeck();

            // MVP: initial planning window is open.
            _actionWindowOpen = true;
            _isSongPlaying = false;
            _isBetweenSongs = true;

            // IMPORTANT: Do NOT set the gig phase for now
            CurrentGigPhase = GigPhase.PlayerTurn;

            //_isSongPlaying = false;
            //_isBetweenSongs = _session != null;
        }

        private enum DeckSetupSource
        {
            Auto = 0,
            PersistentGameplayData = 1,
            GameplayDataInitialDeck = 2,
            RunContextBandDeck = 3,
            OverrideList = 4
        }

        private void SetupDeck(
    DeckSetupSource source = DeckSetupSource.Auto,
    IReadOnlyList<CardDefinition> overrideActionCards = null,
    IReadOnlyList<CardDefinition> overrideCompositionCards = null)
        {
            if (GameManager == null || DeckManager == null)
            {
                Debug.LogError($"{DebugTag} SetupDeck failed: missing GameManager or DeckManager.");
                return;
            }

            var pd = GameManager.PersistentGameplayData;
            if (pd == null)
            {
                Debug.LogError($"{DebugTag} SetupDeck failed: PersistentGameplayData is null.");
                return;
            }

            // Ensure the hand is enabled BEFORE any card objects are instantiated by DrawCards.
            SetHandVisible(true);
            RebindDeckToGigHand();

            var resolvedActions = new List<CardDefinition>(16);
            var resolvedCompositions = new List<CardDefinition>(16);

            void AddValid(IEnumerable<CardDefinition> src)
            {
                if (src == null) return;

                foreach (var c in src)
                {
                    if (c == null) continue;

                    if (c.IsAction) resolvedActions.Add(c);
                    else if (c.IsComposition) resolvedCompositions.Add(c);
                }
            }

            switch (source)
            {
                case DeckSetupSource.OverrideList:
                    AddValid(overrideActionCards);
                    AddValid(overrideCompositionCards);
                    break;

                case DeckSetupSource.RunContextBandDeck:
                    if (GigRunContext.Instance != null &&
                        GigRunContext.Instance.TryGetBandDeck(out var runDeck) &&
                        runDeck != null)
                    {
                        AddValid(runDeck.Cards);
                    }
                    break;

                case DeckSetupSource.GameplayDataInitialDeck:
                    {
                        var a = GameManager.GameplayData?.InitialActionDeck;
                        var c = GameManager.GameplayData?.InitialCompositionDeck;

                        if (a != null) AddValid(a.GetValidCards());
                        if (c != null) AddValid(c.GetValidCards());
                    }
                    break;

                case DeckSetupSource.PersistentGameplayData:
                    AddValid(pd.CurrentActionCards);
                    AddValid(pd.CurrentCompositionCards);
                    break;

                default:
                    // Auto: prefer PD (already populated by GameManager.SetInitialDeck() or GigSetup)
                    AddValid(pd.CurrentActionCards);
                    AddValid(pd.CurrentCompositionCards);

                    // Fallback: GigSetupScene run deck
                    if (resolvedActions.Count == 0 && resolvedCompositions.Count == 0 &&
                        GigRunContext.Instance != null &&
                        GigRunContext.Instance.TryGetBandDeck(out var ctxDeck) &&
                        ctxDeck != null)
                    {
                        AddValid(ctxDeck.Cards);
                    }

                    // Fallback: GameplayData initial decks
                    if (resolvedActions.Count == 0 && resolvedCompositions.Count == 0)
                    {
                        var a = GameManager.GameplayData?.InitialActionDeck;
                        var c = GameManager.GameplayData?.InitialCompositionDeck;

                        if (a != null) AddValid(a.GetValidCards());
                        if (c != null) AddValid(c.GetValidCards());
                    }
                    break;
            }

            int total = resolvedActions.Count + resolvedCompositions.Count;
            if (total == 0)
            {
                Debug.LogError($"{DebugTag} SetupDeck failed: resolved deck is empty. Source={source}");
                return;
            }

            // Keep PersistentGameplayData as the single source of truth for runtime decks.
            pd.CurrentActionCards ??= new List<CardDefinition>();
            pd.CurrentCompositionCards ??= new List<CardDefinition>();

            pd.CurrentActionCards.Clear();
            pd.CurrentActionCards.AddRange(resolvedActions);

            pd.CurrentCompositionCards.Clear();
            pd.CurrentCompositionCards.AddRange(resolvedCompositions);

            Debug.Log(
                $"{DebugTag} SetupDeck resolved Action={pd.CurrentActionCards.Count}, " +
                $"Composition={pd.CurrentCompositionCards.Count}, Total={total} " +
                $"(Source={source}, InitialActionDeck='{GameManager.GameplayData?.InitialActionDeck?.name}', " +
                $"InitialCompositionDeck='{GameManager.GameplayData?.InitialCompositionDeck?.name}')");

            DeckManager.ClearAll();
            DeckManager.SetGameDeck();
        }

        private void SetupEncounter()
        {
            if (useLogs) Debug.Log($"{DebugTag} Setting up gig encounter...");

            var pd = GameManager.PersistentGameplayData;

            // 1) GigSetupScene / dev run
            if (GigRunContext.Instance != null &&
                GigRunContext.Instance.TryGetEncounter(out var ctxEncounter))
            {
                CurrentGigEncounter = ctxEncounter;
            }
            // 2) PersistentGameplayData
            else if (pd.CurrentEncounter != null)
            {
                CurrentGigEncounter = pd.CurrentEncounter;
            }
            // 3) Mapa/sector (normal flow)
            else
            {
                CurrentGigEncounter = GameManager.EncounterData
                    .GetGigEncounterByIndex(
                        pd.CurrentSectorId,
                        pd.CurrentEncounterId,
                        pd.IsFinalEncounter
                    );
            }

            pd.CurrentEncounter = CurrentGigEncounter;

            UIManager.SetupEncounterUI(CurrentGigEncounter);
        }

        private void BuildBackground()
        {
            if (useLogs) Debug.Log($"{DebugTag} Building background...");
            backgroundContainer.OpenSelectedBackground();
            backgroundContainer.SetBPM(0);
        }

        private void BuildBand()
        {
            if (useLogs) Debug.Log($"{DebugTag} Building band and musicians...");

            for (var i = 0;
                i < GameManager.PersistentGameplayData.MusicianList.Count; i++)
            {
                MusicianBase clone = Instantiate(
                    GameManager.PersistentGameplayData.MusicianList[i],
                    MusicianPosList.Count >= i ? MusicianPosList[i] : MusicianPosList[0]
                );

                clone.BuildCharacter();

                var responder = clone.gameObject.GetComponent<MusicianMidiResponder>();
                if (responder == null) responder =
                        clone.gameObject.AddComponent<MusicianMidiResponder>();
                responder.Init(clone);

                clone.BindToGigContext();
                _spawned.Add(clone);

                MidiMusicManager?.RegisterMusicianAnchor(
                    clone.MusicianCharacterData.CharacterId, clone.transform);

                // Front or Back of the Stage
                // TODO: Use a single layer per musician
                if (i < 2) clone.SetSpriteLayerOrder(10);
                else clone.SetSpriteLayerOrder(0);
            }

            CurrentMusicianCharacterList = _spawned;

            SetupBandDebugElements();
        }

        private void BuildAudience()
        {
            if (useLogs) Debug.Log($"{DebugTag} Building audience...");
            var audienceMemberList = CurrentGigEncounter.AudienceMemberList;
            for (var i = 0; i < audienceMemberList.Count; i++)
            {
                var clone = Instantiate(
                    audienceMemberList[i].CharacterPrefab,
                    AudienceMemberPosList.Count >= i ?
                        AudienceMemberPosList[i] :
                        AudienceMemberPosList[0]
                );

                clone.BuildCharacter();

                clone.ColumnIndex = Mathf.Min(i, AudienceMemberPosList.Count - 1);

                if (clone.IsTall) clone.AudienceStats.ApplyStatus(StatusType.Tall, 1);

                CurrentAudienceCharacterList.Add(clone);
            }

            RecalculateAudienceObstructions();
        }
        #endregion

        private void Update()
        {
            if (_session != null)
            {
                _session.Tick(Time.deltaTime);

                // Session might have ended inside Tick()
                if (_session == null)
                {
                    _isSongPlaying = false;
                    _isBetweenSongs = false;
                    return;
                }

                bool playingNow = _session.IsLoopPlaying;
                bool betweenNow = _session.IsActive && !playingNow;

                if (playingNow != _isSongPlaying || betweenNow != _isBetweenSongs)
                {
                    _isSongPlaying = playingNow;
                    _isBetweenSongs = betweenNow;

                    Log($"[Gig] isSongPlaying={_isSongPlaying}, " +
                        $"isBetweenSongs={_isBetweenSongs}", customColor: "cyan");
                }
            }
            else
            {
                _isSongPlaying = false;
                _isBetweenSongs = false;
            }

            // Keep debug slider visibility in sync with the flag
            if (songHypeDebugSlider != null &&
                songHypeDebugSlider.gameObject.activeSelf != debugSongHype)
            {
                songHypeDebugSlider.gameObject.SetActive(debugSongHype);

                if (debugSongHype)
                    songHypeDebugSlider.SetValueWithoutNotify(_songHype);
            }

            // toggle instrument picker + volume debug with D
            if (Input.GetKeyDown(KeyCode.D))
            {
                debugInstrumentPicker = !debugInstrumentPicker;
                debugMusicianVolume = !debugMusicianVolume;

                if (useLogs)
                {
                    Debug.Log($"{DebugTag} [Dev] Toggled debug UI → " +
                              $"Instruments={debugInstrumentPicker}, Volume={debugMusicianVolume}");
                }

                SetupBandDebugElements();
            }
        }

        private void OnPlayPressed()
        {
            // MVP: once Play is pressed, action cards are no longer usable.
            _actionWindowOpen = false;

            // MVP: optionally discard Action cards from hand when starting performance.
            if (discardActionCardsOnPlay && DeckManager != null)
            {
                DeckManager.DiscardHandWhere(card =>
                    card != null &&
                    card.CardDefinition != null &&
                    card.CardDefinition.IsAction);
            }

            // Inject dev overrides into the UI model before building the SongConfig
            ApplyDebugInstrumentOverridesToCompositionModel();

            _session?.ConfirmCurrentPartAndStart();

            // show the hype bar when music starts
            if (UIManager != null && UIManager.GigCanvas != null)
            {
                UIManager.GigCanvas.SetSongHypeVisible(true);
                UIManager.GigCanvas.SetSongHype(SongHype01);
            }
        }

        public void EndTurn()
        {
            if (useLogs) Debug.Log($"{DebugTag} Ending turn...");

            CurrentGigPhase = GigPhase.SongPerformance;
        }

        public void HighlightCardTarget(ActionTargetType targetType)
        {
            // TODO
            switch (targetType)
            {
                case ActionTargetType.AudienceCharacter:

                    break;
                case ActionTargetType.Musician:

                    break;
                case ActionTargetType.AllAudienceCharacters:

                    break;
                case ActionTargetType.AllMusicians:

                    break;
                case ActionTargetType.RandomAudienceCharacter:

                    break;
                case ActionTargetType.RandomMusician:

                    break;
            }
        }

        public void DeactivateCardHighlights()
        {
            // TODO
            // Foreach enemy canvas SetHighlight(false)
            // Foreach ally canvas SetHighlight(false)
        }

        private void ExecuteGigPhase(GigPhase targetGigPhase)
        {
            // TEMP: while porting composition, we ignore the gig state machine.
            if (_session != null)
                return;

            if (useLogs)
                Debug.Log($"{DebugTag} Executing gig phase: {targetGigPhase}");

            switch (targetGigPhase)
            {
                case GigPhase.PrepareGig:
                    break;

                case GigPhase.PlayerTurn:

#if ALWTTT_DEV
                    Debug.Log($"{DebugTag} <color=lime>[DevMode] Entering PlayerTurn. " +
                              $"CurrentSongIndex={GameManager.PersistentGameplayData.CurrentSongIndex} " +
                              $"RequiredSongCount={_requiredSongCount} " +
                              $"InfiniteTurnsEnabled={DevModeController.InfiniteTurnsEnabled} " +
                              $"_session null? {_session == null} " +
                              $"_isSongPlaying={_isSongPlaying} " +
                              $"_isBetweenSongs={_isBetweenSongs}</color>");
#endif

                    if (GameManager.PersistentGameplayData.CurrentSongIndex >=
                        _requiredSongCount
#if ALWTTT_DEV
                        && !DevModeController.InfiniteTurnsEnabled
#endif
                        )
                    {
                        bool win = true;
                        foreach (var audienceCharacter in CurrentAudienceCharacterList)
                        {
                            if (!audienceCharacter.Stats.IsConvinced)
                            {
                                win = false;
                                break;
                            }
                        }

                        if (win)
                        {
                            WinGig();
                        }
                        else
                        {
                            LoseGig();
                        }

                        return;
                    }

#if ALWTTT_DEV
                    Debug.Log($"{DebugTag} <color=lime>[DevMode] PlayerTurn completion check passed (not ending). Continuing to turn init.</color>");
#endif

                    // Reset per-turn flags for this PlayerTurn. Without these resets,
                    // _actionWindowOpen stays false after the first OnPlayPressed in a
                    // multi-song gig, blocking ALL action cards for song 2+. Pre-existing
                    // latent bug; surfaced 2026-04-20 by M1.5 Phase 2 Feedback smoke test.
                    _actionWindowOpen = true;
                    _isBetweenSongs = true;

                    // Decision B: tick musician statuses at PlayerTurn start
#if ALWTTT_DEV
                    // Dev Mode: reset convinced audience so they keep acting in infinite mode
                    if (DevModeController.InfiniteTurnsEnabled &&
                        DevModeController.Instance != null)
                    {
                        DevModeController.Instance.OnPlayerTurnStartInfiniteMode();
                    }
#endif

                    foreach (var m in CurrentMusicianCharacterList)
                    {
                        m?.Statuses?.Tick(TickTiming.PlayerTurnStart);
                    }

                    // Decision A: Composure is turn-scoped — clear at each PlayerTurn start
                    foreach (var m in CurrentMusicianCharacterList)
                    {
                        m?.Statuses?.Clear(CharacterStatusId.TempShieldTurn);
                    }

                    OnPlayerTurnStarted?.Invoke();
                    GameManager.PersistentGameplayData.SongModifierCardsList.Clear();

                    // --- Inspiration + Draw Logic ---
                    var pd = GameManager.PersistentGameplayData;

                    if (!pd.KeepInspirationBetweenTurns)
                        pd.CurrentInspiration = pd.TurnStartingInspiration;

                    // TODO: Special case for first turn (e.g. Deployment Phase in Monster Train 2)
                    DeckManager.DrawCards(GameManager.PersistentGameplayData.DrawCount);
                    GameManager.PersistentGameplayData.CanSelectCards = true;
                    // ---

                    if (_session == null)
                    {
                        Log($"{DebugTag} [Gig] Starting new live " +
                            $"composition session for next song.");

                        StartCompositionSession();

                        if (compositionUI != null)
                        {
                            compositionUI.HookPlayButton(OnPlayPressed);
                        }

                        _isSongPlaying = false;
                        _isBetweenSongs = _session != null; // true once session is created
                    }

                    break;
                case GigPhase.SongPerformance:

                    OnSongPerformanceStarted?.Invoke();

                    GameManager.PersistentGameplayData.CurrentSongIndex++;
                    OnSongsLeftChanged?.Invoke(SongsLeft, _requiredSongCount);

                    if (GameManager.PersistentGameplayData.DiscardHandBetweenTurns)
                    {
                        DeckManager.DiscardHand();
                    }

                    StartCoroutine(SongPerformanceRoutine());

                    GameManager.PersistentGameplayData.CanSelectCards = false;
                    break;
                case GigPhase.AudienceTurn:

                    OnEnemyTurnStarted?.Invoke();

                    StartCoroutine(AudienceTurnRoutine());

                    GameManager.PersistentGameplayData.CanSelectCards = false;
                    break;
                case GigPhase.EndGig:

                    GameManager.PersistentGameplayData.CanSelectCards = false;
                    break;
            }
        }

        private int ResolveRequiredSongCount()
        {
            int fallback = GameManager.PersistentGameplayData.CurrentEncounter != null
                ? GameManager.PersistentGameplayData.CurrentEncounter.NumberOfSongs
                : 2;

            var ctx = GigRunContext.Instance;
            if (ctx != null && ctx.HasActiveRun)
                return ctx.ResolveRequiredSongCount(fallback);

            return fallback;
        }

        private IEnumerator SongPerformanceRoutine()
        {
            // Activate SFX cards
            foreach (var smCard in
                GameManager.PersistentGameplayData.SongModifierCardsList)
            {
                if (smCard.CardType == CardType.SFX)
                {
                    // TODO: Generalize
                    backgroundContainer.ActivateSFX("lights");
                }
            }

            var song = GameManager.PersistentGameplayData.CurrentSong;

            playedSongs.Add(song);
            backgroundContainer.SetBPM(song.BPM);

            // TODO: Playing Musician Animator Settings
            foreach (var musician in CurrentMusicianCharacterList)
            {
                musician.CharacterAnimator.SetBPM(song.BPM);
                musician.CharacterAnimator.SkipEveryNBeats = 1;
                musician.CharacterAnimator.BeatOffsetBeats =
                    UnityEngine.Random.Range(0f, 0.15f);
                musician.CharacterAnimator.JumpOnBeat = true;
                musician.CharacterAnimator.RotateOnBeat = false;
                musician.CharacterAnimator.EmitOnBeat = true;
            }

            // Set mapping so live MIDI events can be routed to the right musician
            var owners = MidiMusicManager.GetChannelOwnerIdsFor(song);
            MidiMusicManager.SetChannelOwners(owners?.ToList());

            var songDuration = MidiMusicManager.Play(song);

            Debug.Log($"Playing {song.SongTitle} for {songDuration}[s]");

            yield return MidiMusicManager.WaitForEnd();

            backgroundContainer.SetBPM(0);
            foreach (var musician in CurrentMusicianCharacterList)
            {
                musician.CharacterAnimator.SetBPM(120);
                musician.CharacterAnimator.SkipEveryNBeats = 2;
                musician.CharacterAnimator.BeatOffsetBeats =
                    UnityEngine.Random.Range(0.45f, 0.55f);
                musician.CharacterAnimator.JumpOnBeat = false;
                musician.CharacterAnimator.RotateOnBeat = true;
                musician.CharacterAnimator.EmitOnBeat = false;
            }

            var reactionDuration = 5f;

            Debug.Log("Audience Reaction");

            // TODO: Apply equipped SongModifier Effects
            foreach (var smCard in
                GameManager.PersistentGameplayData.SongModifierCardsList)
            {

            }

            // TODO: Apply Vibe to enemies
            yield return new WaitForSeconds(reactionDuration);

            if (CurrentGigPhase == GigPhase.EndGig)
                yield break;

            if (IsGigComplete && skipAudienceActionsAfterFinalSong)
            {
                // Reaction already happened. Now end without Audience actions.
                ResolveGigOutcomeAndEnd();
                yield break;
            }

            CurrentGigPhase = GigPhase.AudienceTurn;
        }

        private IEnumerator AudienceTurnRoutine()
        {
            var waitDelay = new WaitForSeconds(perAudienceActionDelay);

            if (_lastSongFeedback.HasValue)
            {
                yield return RunSongVibeResolution(_lastSongFeedback.Value);
                _lastSongFeedback = null;

                // --- reset & hide the hype bar ---
                ResetSongHype();  // numeric reset + anim intensity reset
                if (UIManager != null && UIManager.GigCanvas != null)
                {
                    UIManager.GigCanvas.ClearSongHype();
                }
            }

            // Decision B: tick audience statuses at AudienceTurn start
            foreach (var a in CurrentAudienceCharacterList)
            {
                a?.Statuses?.Tick(TickTiming.AudienceTurnStart);
            }

            // Decision E: Feedback DoT — applies to musicians only (audience Stress not yet implemented)
            foreach (var m in CurrentMusicianCharacterList)
            {
                if (m?.Statuses == null) continue;
                int feedbackStacks = m.Statuses.GetStacks(CharacterStatusId.DamageOverTime);
                if (feedbackStacks > 0)
                    m.Stats?.ApplyIncomingStressWithComposure(m.Statuses, feedbackStacks);
            }

            // Snapshot so actions can reorder/destroy without breaking enumeration
            var turnOrder =
                new List<AudienceCharacterBase>(CurrentAudienceCharacterList);

            foreach (var currentCharacter in turnOrder)
            {
                if (currentCharacter == null)
                    continue; // might have been destroyed

                if (!currentCharacter.gameObject.activeInHierarchy)
                    continue; // or deactivated

                if (currentCharacter.AudienceStats.IsConvinced)
                    continue; // already convinced

                yield return currentCharacter.StartCoroutine(
                    nameof(AudienceCharacterSimple.AbilityRoutine));

                yield return waitDelay;
            }

            CurrentAudienceCharacterList.Sort((a, b) =>
                a.ColumnIndex.CompareTo(b.ColumnIndex));

            if (CurrentGigPhase != GigPhase.EndGig)
            {
                CurrentGigPhase = GigPhase.PlayerTurn;
            }
        }

        public void LoseGig()
        {
#if ALWTTT_DEV
            if (DevModeController.InfiniteTurnsEnabled)
            {
                Debug.Log($"{DebugTag} <color=lime>[DevMode] LoseGig suppressed (infinite turns).</color>");
                return;
            }
#endif

            var pd = GameManager.PersistentGameplayData;

            var encounter = pd.CurrentEncounter ?? CurrentGigEncounter; // CurrentGigEncounter = your runtime one
            int penalty = encounter != null ? encounter.CohesionPenaltyOnLoss : 0;

            UIManager.GigCanvas.OnLossConfirm = () => ReturnToMap(false);
            UIManager.GigCanvas.ShowLoss(
                title: "Gig Lost",
                body: "You didn’t convince the crowd this time, but the journey continues.\n" +
                      $"Cohesion decreased by {penalty}."
            );

            foreach (var m in _spawned)
                if (m != null) m.UnbindFromGigContext();
        }


        private void WinGig()
        {
#if ALWTTT_DEV
            if (DevModeController.InfiniteTurnsEnabled)
            {
                Debug.Log($"{DebugTag} <color=lime>[DevMode] WinGig suppressed (infinite turns).</color>");
                return;
            }
#endif
            if (CurrentGigPhase == GigPhase.EndGig) return;
            CurrentGigPhase = GigPhase.EndGig;

            // Keep current stress
            foreach (var musicianBase in CurrentMusicianCharacterList)
            {
                GameManager.PersistentGameplayData.SetMusicianHealthData(
                    musicianBase.MusicianCharacterData.CharacterId,
                    musicianBase.MusicianStats.CurrentStress,
                    musicianBase.MusicianStats.MaxStress);
            }

            DeckManager.ClearPiles();

            if (GameManager.PersistentGameplayData.IsFinalEncounter)
            {
                UIManager.GigCanvas.WinPanel.SetActive(true);
            }
            else
            {
                foreach (var musicianBase in CurrentMusicianCharacterList)
                {
                    musicianBase.MusicianStats.ClearAllStatus();
                }

                GameManager.PersistentGameplayData.CurrentEncounterId++;
                UIManager.GigCanvas.gameObject.SetActive(false);

                UIManager.RewardCanvas.gameObject.SetActive(true);
                UIManager.RewardCanvas.PrepareCanvas();
                UIManager.RewardCanvas.BuildReward(RewardType.Card);
                UIManager.RewardCanvas.OnRewardFinished = () => ReturnToMap(true);
            }

            // Musicians unsubscribe to gig events
            foreach (var m in _spawned)
            {
                if (m != null) m.UnbindFromGigContext();
            }

            GameManager.PersistentGameplayData.GigsWon++;
        }

        public void RecalculateAudienceObstructions()
        {
            // Clear all
            foreach (var c in CurrentAudienceCharacterList)
            {
                c.IsBlocked = false;
            }

            for (int i = 0; i < CurrentAudienceCharacterList.Count; i++)
            {
                var member = CurrentAudienceCharacterList[i];
                if (member.IsTall && !member.Stats.IsConvinced)
                {
                    // Block all non-tall audience members behind tall one
                    for (int j = i + 1; j < CurrentAudienceCharacterList.Count; j++)
                    {
                        var otherMember = CurrentAudienceCharacterList[j];
                        if (!otherMember.IsTall)
                        {
                            otherMember.IsBlocked = true;
                        }
                    }

                    break;
                }
            }
        }

        private void ReturnToMap(bool won)
        {
            if (_returningToMap)
            {
                Debug.LogWarning($"{DebugTag} ReturnToMap called more than once. Ignoring.");
                return;
            }
            _returningToMap = true;

            var pd = GameManager.PersistentGameplayData;

            // Decide where to return BEFORE clearing context
            var returnDest = GigRunContext.GigReturnDestination.Map;

            var ctx = GigRunContext.Instance;

            Debug.Log(
                $"[GigManager] ReturnToMap | " +
                $"ctxNull={ctx == null} | " +
                $"ctxId={(ctx != null ? ctx.GetInstanceID() : -1)} | " +
                $"HasActiveRun={(ctx != null && ctx.HasActiveRun)} | " +
                $"CurrentNull={(ctx != null ? ctx.Current == null : true)} | " +
                $"ReturnDest={(ctx != null && ctx.Current != null ? ctx.Current.returnDestination.ToString() : "N/A")}"
            );

            if (ctx != null && ctx.HasActiveRun && ctx.Current != null)
            {
                returnDest = ctx.Current.returnDestination;
            }

            // If we're in the real map flow, mark the node completed
            var state = pd.CurrentSectorMapState;
            if (state != null)
            {
                var node = state.GetNode(state.CurrentNodeId);
                if (node != null && node.Type == Enums.NodeType.Gig)
                    node.Completed = true;
            }

            // Use whichever encounter we actually have
            var encounter = pd.CurrentEncounter ?? CurrentGigEncounter;

            if (encounter != null)
            {
                if (won)
                {
                    pd.Fans += encounter.FansOnWin;
                }
                else
                {
                    pd.BandCohesion = Mathf.Max(0, pd.BandCohesion - encounter.CohesionPenaltyOnLoss);
                }
            }
            else
            {
                Debug.LogWarning($"{DebugTag} ReturnToMap: Encounter is null, skipping fans/cohesion adjustments.");
            }

            // Clear pointers
            pd.CurrentEncounter = null;
            if (ctx != null) ctx.Clear();

            // Route back
            if (sceneChanger != null)
            {
                if (returnDest == GigRunContext.GigReturnDestination.GigSetup) sceneChanger.OpenGigSetupScene();
                else sceneChanger.OpenMapScene();
            }
            else
            {
                SceneManager.LoadScene(returnDest == GigRunContext.GigReturnDestination.GigSetup ? "GigSetup" : "Map");
            }
        }



        private void SetHandVisible(bool visible)
        {
            if (gigHand != null)
            {
                gigHand.gameObject.SetActive(visible);
                if (visible) gigHand.EnableDragging();
                else gigHand.DisableDragging();
            }
        }

        private void RebindDeckToGigHand()
        {
            if (DeckManager.Instance != null && gigHand != null)
            {
                DeckManager.Instance.SetHandController(gigHand);
                // Same resolver pattern as ShipInteriorManager
                gigHand.SetTargetResolver(ResolveMusicianByType);
            }
        }

        private MusicianBase ResolveMusicianByType(MusicianCharacterType t)
        {
            if (t == MusicianCharacterType.None || CurrentMusicianCharacterList == null)
                return null;

            return CurrentMusicianCharacterList.FirstOrDefault(m =>
                m?.MusicianCharacterData != null &&
                m.MusicianCharacterData.CharacterType == t);
        }

        public bool TryPlayCompositionCard(
            CardBase card, MusicianBase target, CardDropZone zone)
        {
            if (_session == null)
            {
                Log("No active CompositionSession; cannot play composition card.");
                return false;
            }

            // One-shot composition card animation
            if (target != null && card != null && card.CardDefinition != null)
            {
                target.PlayCardOneShotAnimation(card.CardDefinition);
            }

            return _session?.TryPlayCompositionCard(card, target, zone) ?? false;
        }

        private void StartCompositionSession()
        {
            if (_session != null)
            {
                _session.LoopFinished -= OnCompositionLoopFinished;
                _session.PartFinished -= OnCompositionPartFinished;
                _session.SongFinished -= OnCompositionSongFinished;
                _session.End();
            }

            // Full reset before a new song
            ResetSongHype();
            ResetSongScopedStatuses();

            _session = new CompositionSession();
            _session.LoopFinished += OnCompositionLoopFinished;
            _session.PartFinished += OnCompositionPartFinished;
            _session.SongFinished += OnCompositionSongFinished;

            var ctx = new GigContext(this);
            _session.Begin(ctx, jamRules, midiGenPlayConfig, _rng);

            // starting hype before the first loop finishes
            // IDEAS
            // PersistentGameplayData.Fans (more fans → higher baseline hype).
            // Venue type / difficulty (small club vs arena).
            if (startingSongHype > 0f)
                AddSongHype(startingSongHype);

            _isSongPlaying = false;
            _isBetweenSongs = true;
        }

        private void ResetSongHype()
        {
            _songHype = 0f;
            UpdateAudienceBeatIntensity();
            OnSongHypeChanged01?.Invoke(SongHype01);
        }

        private void OnCompositionLoopFinished(LoopFeedbackContext loopCtx)
        {
            TriggerAudienceMicroReactions(loopCtx);
        }

        private void OnCompositionPartFinished(PartFeedbackContext partCtx)
        {
            Log($"{DebugTag} [Gig] Part finished → {partCtx}", customColor: "orange");

            // Retrieve aggregated impressions for this part (if we have any)
            _audienceLoopImpressionsByPart
                .TryGetValue(partCtx.PartIndex, out var perAudience);

            // Build an enriched PartFeedbackContext that includes audience data
            var enriched = new PartFeedbackContext(
                partIndex: partCtx.PartIndex,
                partLabel: partCtx.PartLabel,
                loops: partCtx.Loops,
                audienceLoopImpressions: perAudience
            );

            // Keep it for song-level aggregation
            _gigPartsForCurrentSong.Add(enriched);

            // No need to keep raw per-loop impressions for this part anymore
            _audienceLoopImpressionsByPart.Remove(partCtx.PartIndex);

            // Notify gig-level listeners with the *enriched* context
            OnGigPartFeedbackReady?.Invoke(enriched);
        }

        /// <summary>
        /// Song finished, store data
        /// </summary>
        /// <param name="songCtx"></param>
        private void OnCompositionSongFinished(SongFeedbackContext songCtx)
        {
            Log($"{DebugTag} [Gig] Song finished → {songCtx}", customColor: "yellow");

            // Build an enriched SongFeedbackContext using the gig's part list
            var enrichedSong = new SongFeedbackContext(_gigPartsForCurrentSong.ToList());

            // Save for AudienceTurn
            _lastSongFeedback = enrichedSong;

            // Macro reaction – final SongHype +aggregated impressions → Vibe
            //ApplySongHypeToAudience(enrichedSong);

            // Notify gig-level listeners
            OnGigSongFeedbackReady?.Invoke(enrichedSong);

            // Clear per-song state for the next song
            _gigPartsForCurrentSong.Clear();
            _audienceLoopImpressionsByPart.Clear();

            // === END OF SONG: count it ===
            GameManager.PersistentGameplayData.CurrentSongIndex++;
            OnSongsLeftChanged?.Invoke(SongsLeft, _requiredSongCount);

            // === END OF SONG: discard hand between songs (if enabled) ===
            if (GameManager.PersistentGameplayData.DiscardHandBetweenTurns)
            {
                DeckManager.DiscardHand();
            }

            // Reset BPM/Animation
            ResetStageToIdle();
        }

        public bool CanPlayActionCard(CardDefinition card)
        {
            if (card == null) return false;
            if (!card.IsAction) return false;

            var actionPayload = card.ActionPayload;
            if (actionPayload == null) return false;
            var actionTiming = actionPayload.ActionTiming;

            // During performance we default to disabling action cards in the MVP,
            // except those explicitly marked as Always (and only if enabled).
            if (_isSongPlaying)
                return allowActionCardsDuringPerformance && actionTiming == CardActionTiming.Always;

            // If the player already pressed Play, action cards are locked.
            if (!_actionWindowOpen)
                return false;

            if (_isBetweenSongs)
                return true; // (keep your existing logic)

            // No session / no gig context -> no action cards
            return false;
        }

        private int GetTotalFlowStacks()
        {
            int total = 0;

            if (CurrentMusicianCharacterList == null)
                return 0;

            for (int i = 0; i < CurrentMusicianCharacterList.Count; i++)
            {
                var m = CurrentMusicianCharacterList[i];
                if (m == null || m.Statuses == null) continue;

                total += Mathf.Max(0, m.Statuses.GetStacks(CharacterStatusId.DamageUpFlat));
            }

            return total;
        }

        private void ResetSongScopedStatuses()
        {
            // MVP: we treat these as song-scoped.
            // Flow == CharacterStatusId.DamageUpFlat
            // Composure == CharacterStatusId.TempShieldTurn
            if (CurrentMusicianCharacterList == null)
                return;

            for (int i = 0; i < CurrentMusicianCharacterList.Count; i++)
            {
                var m = CurrentMusicianCharacterList[i];
                if (m == null || m.Statuses == null) continue;

                m.Statuses.Clear(CharacterStatusId.DamageUpFlat);
                m.Statuses.Clear(CharacterStatusId.TempShieldTurn);
            }
        }

        // Called whenever one full loop finishes (including the last loop of the song)
        private void TriggerAudienceMicroReactions(LoopFeedbackContext loopCtx)
        {
            if (CurrentAudienceCharacterList == null
                || CurrentAudienceCharacterList.Count == 0)
                return;

            // Compute loop score and SongHype delta using the pure calculator
            float loopScore = LoopScoreCalculator.ComputeLoopScore(loopCtx);
            float baseHypeDelta = LoopScoreCalculator.ComputeHypeDelta(loopScore);

            // Global scalar for testing
            float hypeDelta = baseHypeDelta * songHypeDeltaMultiplier;

            // FLOW (Legacy): optionally allow Flow stacks to multiply loop→SongHype conversion.
            // Default is OFF when using Strength-like Flow→Vibe.
            int flowStacks = GetTotalFlowStacks();
            if (flowAffectsSongHype && flowStacks > 0)
            {
                float mult = 1f + (flowStacks * flowSongHypeMultiplierPerStack);
                hypeDelta *= mult;

                if (debugFlowSongHype)
                {
                    Debug.Log($"{DebugTag} [Flow→SongHype] stacks={flowStacks} mult={mult:0.00} baseΔ={baseHypeDelta:0.00} finalΔ={hypeDelta:0.00}");
                }
            }


            AddSongHype(hypeDelta);

            Log($"{DebugTag} [Gig] Loop finished. " +
                $"Score={loopScore:F1}, ΔHype={hypeDelta:F1}, " +
                $"SongHype={SongHype:F1}");

            // Part index from the loop context
            int partIndex = loopCtx.PartIndex;

            // Ensure per-part map exists
            if (!_audienceLoopImpressionsByPart.TryGetValue(partIndex, out var perAudience))
            {
                perAudience = new Dictionary<int, List<int>>();
                _audienceLoopImpressionsByPart[partIndex] = perAudience;
            }

            for (int i = 0; i < CurrentAudienceCharacterList.Count; i++)
            {
                var audience = CurrentAudienceCharacterList[i];
                if (audience == null) continue;

                // Each audience member resolves the loop into an impression [-2..2]
                int raw = audience.ResolveLoopEffect(loopCtx);
                int clamped = Mathf.Clamp(raw, -2, 2);

                Debug.Log(
                    $"<color=red>{DebugTag} [Gig]   - {audience.CharacterId} " +
                    $"impression={clamped} for {loopCtx}</color>");

                // Aggregate impressions per audience, per loop
                if (!perAudience.TryGetValue(i, out var impressions))
                {
                    impressions = new List<int>();
                    perAudience[i] = impressions;
                }

                impressions.Add(clamped);

                // TODO plug things like:
                // audience.Stats.AddVibe(clamped);
                // trigger VFX/SFX, etc.
            }
        }

        internal void OnCompositionSessionEnded()
        {
            if (_session != null)
            {
                _session.LoopFinished -= OnCompositionLoopFinished;
                _session.PartFinished -= OnCompositionPartFinished;
                _session.SongFinished -= OnCompositionSongFinished;
            }

            Log($"{DebugTag} [Gig] Composition session ended. Starting Audience phase.");

            // Session is done; detach so the gig state machine can run again
            _session = null;
            _isSongPlaying = false;
            _isBetweenSongs = false;

#if ALWTTT_DEV
            Debug.Log($"{DebugTag} <color=lime>[DevMode] OnCompositionSessionEnded reached. " +
                      $"InfiniteTurnsEnabled={DevModeController.InfiniteTurnsEnabled}, " +
                      $"DeckManager.Instance null? {DeckManager.Instance == null}, " +
                      $"IsGigComplete={IsGigComplete}, " +
                      $"skipAudienceActionsAfterFinalSong={skipAudienceActionsAfterFinalSong}, " +
                      $"gigHand.activeSelf={(gigHand != null ? gigHand.gameObject.activeSelf.ToString() : "n/a")}</color>");

            if (DevModeController.InfiniteTurnsEnabled && DeckManager.Instance != null)
            {
                int destroyed = DeckManager.Instance.DevForceHandResetToDiscard();
                Debug.Log($"{DebugTag} <color=lime>[DevMode] Forced hand reset between song cycles: {destroyed} card(s).</color>");
            }
            else if (DevModeController.InfiniteTurnsEnabled)
            {
                Debug.LogWarning($"{DebugTag} <color=lime>[DevMode] Infinite turns ON but DeckManager.Instance is null — hand NOT reset.</color>");
            }

            // Re-show the hand before the next PlayerTurn draws. The composition session
            // hid it via ShowHand(false); nothing else turns it back on during the gap
            // between song end and the next composition session's Begin(). In normal
            // single-song gigs this gap doesn't exist because the gig ends. Infinite-turns
            // exposes it: cards drawn under an inactive DrawTransform are inactive and
            // produce ghost/untappable cards.
            if (DevModeController.InfiniteTurnsEnabled)
            {
                SetHandVisible(true);
                Debug.Log($"{DebugTag} <color=lime>[DevMode] Re-enabled hand visibility between song cycles. " +
                          $"gigHand.activeSelf now={(gigHand != null ? gigHand.gameObject.activeSelf.ToString() : "n/a")}</color>");
            }
#endif

            if (IsGigComplete && skipAudienceActionsAfterFinalSong)
            {
                ResolveGigOutcomeAndEnd();
                return;
            }

            // Hand control to the existing gig phase system:
            // this will call AudienceTurnRoutine() and run all audience actions.
            CurrentGigPhase = GigPhase.AudienceTurn;
        }

        private void AddSongHype(float delta)
        {
            if (debugSongHype)
                return;

            _songHype = Mathf.Clamp(_songHype + delta, 0f, maxSongHype);
            UpdateAudienceBeatIntensity();

            // Keep the slider in sync (even if it's hidden)
            if (songHypeDebugSlider != null)
                songHypeDebugSlider.SetValueWithoutNotify(_songHype);

            OnSongHypeChanged01?.Invoke(SongHype01);
        }

        private struct AudienceVibeDelta
        {
            public int AudienceIndex;
            public int Delta;
        }

        private List<AudienceVibeDelta> ComputeSongVibeDeltas(
            SongFeedbackContext enrichedSong)
        {
            var result = new List<AudienceVibeDelta>();
            if (CurrentAudienceCharacterList == null ||
                CurrentAudienceCharacterList.Count == 0)
                return result;

            int audienceCount = CurrentAudienceCharacterList.Count;

            // Aggregate impressions per audience member across all parts/loops
            var totalImpression = new float[audienceCount];
            var sampleCounts = new int[audienceCount];

            foreach (var part in enrichedSong.Parts)
            {
                var perAudience = part.AudienceLoopImpressions;
                if (perAudience == null) continue;

                foreach (var kv in perAudience)
                {
                    int index = kv.Key;
                    if (index < 0 || index >= audienceCount) continue;

                    var impressions = kv.Value;
                    if (impressions == null || impressions.Count == 0) continue;

                    foreach (var v in impressions)
                    {
                        totalImpression[index] += v;   // v is in [-2, 2]
                        sampleCounts[index] += 1;
                    }
                }
            }

            // Base vibe from final SongHype (0..maxSongHype → 0..maxVibeFromSongHype)
            float baseVibe = SongHype01 * maxVibeFromSongHype;

            // 3) Convert to per-audience vibe deltas
            for (int i = 0; i < audienceCount; i++)
            {
                var audience = CurrentAudienceCharacterList[i];
                if (audience == null) continue;
                // Blocked chars get no vibe
                if (audience.IsBlocked) continue;

                float avgImpression =
                    sampleCounts[i] > 0 ? totalImpression[i] / sampleCounts[i] : 0f; // [-2, 2]

                // Map avgImpression [-2,2] → multiplier [0.5, 1.5]
                float impressionFactor = 1f + (avgImpression * 0.25f);

                float vibeFloat = baseVibe * impressionFactor;
                int vibeDelta = Mathf.RoundToInt(vibeFloat);

                if (vibeDelta <= 0)
                {
                    // For MVP: no negative macro Vibe, just “no gain”.
                    continue;
                }

                result.Add(new AudienceVibeDelta
                {
                    AudienceIndex = i,
                    Delta = vibeDelta
                });
            }

            return result;
        }

        // TODO: Move all animation logic to its own class ie "BandAnimator" etc
        public void ApplyBpmToStage(int partIndex, int bpm)
        {
            _currentBpm = bpm;

            if (useLogs)
                Debug.Log($"{DebugTag} [Gig] Part {partIndex} BPM resolved → {bpm}");

            // 1) Background pulse
            if (backgroundContainer != null)
            {
                backgroundContainer.SetBPM(bpm);
            }

            // 2) Band animators
            foreach (var musician in CurrentMusicianCharacterList)
            {
                if (musician == null || musician.CharacterAnimator == null)
                    continue;

                var anim = musician.CharacterAnimator;

                anim.SetBPM(bpm);
                anim.SkipEveryNBeats = 1;
                anim.BeatOffsetBeats = UnityEngine.Random.Range(0f, 0.15f);
                anim.JumpOnBeat = true;
                anim.RotateOnBeat = false; // or mix per musician if you want
                anim.EmitOnBeat = true;
            }

            // 3) Audience animators – follow BPM, intensity handled by SongHype
            foreach (var audience in CurrentAudienceCharacterList)
            {
                if (audience == null || audience.CharacterAnimator == null)
                    continue;

                var anim = audience.CharacterAnimator;

                anim.SetBPM(bpm);
                anim.SkipEveryNBeats = 1;
                anim.BeatOffsetBeats = UnityEngine.Random.Range(0f, 0.15f);
            }
        }

        private void ResetStageToIdle()
        {
            if (backgroundContainer != null)
                backgroundContainer.SetBPM(0);

            foreach (var musician in CurrentMusicianCharacterList)
            {
                if (musician == null || musician.CharacterAnimator == null)
                    continue;

                var anim = musician.CharacterAnimator;

                anim.SetBPM(idleBpm);
                anim.SkipEveryNBeats = 2;
                anim.BeatOffsetBeats = UnityEngine.Random.Range(0.45f, 0.55f);
                anim.JumpOnBeat = false;
                anim.RotateOnBeat = true;
                anim.EmitOnBeat = false;
            }

            // Audience back to their "idle" animation
            foreach (var audience in CurrentAudienceCharacterList)
            {
                if (audience == null || audience.CharacterAnimator == null)
                    continue;

                var anim = audience.CharacterAnimator;

                anim.SetBPM(idleBpm);
                anim.SkipEveryNBeats = 2;
                anim.BeatOffsetBeats = UnityEngine.Random.Range(0.45f, 0.55f);

                anim.JumpOnBeat = false;   // hype jumping only during composition
                anim.RotateOnBeat = true;    // gentle idle sway
                anim.EmitOnBeat = false;

                // Reset hype multiplier so next song starts from rest
                anim.SetJumpIntensity01(0f);
            }
        }

        private void UpdateAudienceBeatIntensity()
        {
            if (CurrentAudienceCharacterList == null ||
                CurrentAudienceCharacterList.Count == 0)
                return;

            float t = SongHype01; // 0..1 based on current SongHype/maxSongHype
            float intensity = audienceJumpIntensityCurve != null &&
                              audienceJumpIntensityCurve.length > 0
                ? Mathf.Clamp01(audienceJumpIntensityCurve.Evaluate(t))
                : t * t; // fallback: simple ease-in quadratic

            foreach (var audience in CurrentAudienceCharacterList)
            {
                if (audience == null || audience.CharacterAnimator == null)
                    continue;

                var anim = audience.CharacterAnimator;

                // Scale their jump height
                anim.SetJumpIntensity01(intensity);

                // They only actually “jump” when hype passes a threshold
                anim.JumpOnBeat = (intensity >= audienceJumpThreshold);

                // Optionally: you could also turn on rotation/particles here
                // anim.RotateOnBeat = false;
                // anim.EmitOnBeat = false;
            }
        }

        // DEBUGGING
        private void SetupSongHypeDebugUI()
        {
            if (songHypeDebugSlider == null) return;

            songHypeDebugSlider.minValue = 0f;
            songHypeDebugSlider.maxValue = maxSongHype;
            songHypeDebugSlider.wholeNumbers = false;

            // Start synced to current hype
            songHypeDebugSlider.SetValueWithoutNotify(_songHype);

            // Avoid double-registering
            songHypeDebugSlider.onValueChanged.RemoveListener(OnDebugSongHypeSliderChanged);
            songHypeDebugSlider.onValueChanged.AddListener(OnDebugSongHypeSliderChanged);

            // Only visible when debug mode is ON
            songHypeDebugSlider.gameObject.SetActive(debugSongHype);
        }

        private void OnDebugSongHypeSliderChanged(float value)
        {
            // Only override the game when debug mode is enabled
            if (!debugSongHype) return;

            _songHype = Mathf.Clamp(value, 0f, maxSongHype);

            OnSongHypeChanged01?.Invoke(SongHype01);
            UpdateAudienceBeatIntensity();
        }

        private void SetupBandDebugElements()
        {
            if (CurrentMusicianCharacterList == null ||
                CurrentMusicianCharacterList.Count == 0)
                return;

            // Refresh repo once if we’re going to use it
            if (debugInstrumentPicker && _instrumentRepo != null)
                _instrumentRepo.Refresh();

            foreach (var m in CurrentMusicianCharacterList)
            {
                if (m == null) continue;

                var canvas = m.BandCharacterCanvas;
                var profile = m.MusicianCharacterData?.Profile;

                if (canvas == null)
                    continue;

                // ------------------------------------------------------------------
                // 1) INSTRUMENT DEV (dropdowns) – controlled by debugInstrumentPicker
                // ------------------------------------------------------------------
                if (debugInstrumentPicker && profile != null && _instrumentRepo != null)
                {
                    Debug.Log("<color=blue> I AM HERE </color>");

                    if (profile.IsPercussionist())
                    {
                        // Drummer: use percussion options
                        var percOptions = profile.GetDebugPercussionInstrumentOptions(_instrumentRepo);

                        canvas.SetupPercussionInstrumentDebugDropdown(
                            true,
                            percOptions,
                            chosen =>
                            {
                                m.DebugOverridePercussionInstrument = chosen;
                                m.DebugOverrideInstrument = null;

                                if (useLogs)
                                {
                                    var label = chosen != null
                                        ? (!string.IsNullOrEmpty(chosen.InstrumentName)
                                            ? chosen.InstrumentName
                                            : chosen.name)
                                        : "None (random drums)";

                                    Debug.Log($"{DebugTag} [Dev] {m.CharacterName} debug percussion → {label}");
                                }
                            });
                    }
                    else
                    {
                        // Melodic musician: use melodic options
                        var melOptions = profile.GetDebugMelodicInstrumentOptions(m, _instrumentRepo);

                        canvas.SetupInstrumentDebugDropdown(
                            true,
                            melOptions,
                            chosen =>
                            {
                                m.DebugOverrideInstrument = chosen;
                                m.DebugOverridePercussionInstrument = null;

                                if (useLogs)
                                {
                                    var label = chosen != null
                                        ? (!string.IsNullOrEmpty(chosen.InstrumentName)
                                            ? chosen.InstrumentName
                                            : chosen.name)
                                        : "None (random melodic)";

                                    Debug.Log($"{DebugTag} [Dev] {m.CharacterName} debug melodic → {label}");
                                }
                            });
                    }
                }
                else
                {
                    // Instrument debug OFF → hide dropdowns & clear overrides
                    canvas.SetupInstrumentDebugDropdown(false, null, _ => { });
                    canvas.SetupPercussionInstrumentDebugDropdown(false, null, _ => { });

                    m.DebugOverrideInstrument = null;
                    m.DebugOverridePercussionInstrument = null;
                }

                // ------------------------------------------------------------------
                // 2) VOLUME DEV (slider) – independent flag debugMusicianVolume
                // ------------------------------------------------------------------
                if (debugMusicianVolume)
                {
                    float initial =
                        _musicianVolume01.TryGetValue(m.CharacterId, out var stored)
                            ? stored
                            : 1f;

                    canvas.SetupVolumeDebugSlider(
                        true,
                        v => OnMusicianVolumeSliderChanged(m, v),
                        initial);
                }
                else
                {
                    // Hide slider and detach callbacks
                    canvas.SetupVolumeDebugSlider(false, null, 1f);
                }
            }
        }


        private void ApplyDebugInstrumentOverridesToCompositionModel()
        {
            if (!debugInstrumentPicker) return;
            if (compositionUI == null) return;

            var model = compositionUI.Model;
            if (model == null || model.parts == null) return;

            foreach (var part in model.parts)
            {
                if (part == null || part.tracks == null) continue;

                foreach (var track in part.tracks)
                {
                    if (track == null) continue;

                    var musician = CurrentMusicianCharacterList
                        .FirstOrDefault(m =>
                            m.MusicianCharacterData != null &&
                            m.MusicianCharacterData.CharacterId == track.musicianId);

                    if (musician == null) continue;

                    // Reset both first so we don't leak an old choice
                    track.overrideMelodicInstrument = null;
                    track.overridePercussionInstrument = null;

                    bool isPercTrack =
                        track.role == TrackRole.Rhythm
                        // optionally more roles:
                        // || track.role == TrackRole.Percussion
                        ;

                    if (isPercTrack && musician.DebugOverridePercussionInstrument != null)
                    {
                        track.overridePercussionInstrument =
                            musician.DebugOverridePercussionInstrument;
                    }
                    else if (musician.DebugOverrideInstrument != null)
                    {
                        track.overrideMelodicInstrument =
                            musician.DebugOverrideInstrument;
                    }
                }
            }
        }

        private float ComputeEffectiveMusicianVolume01(
            MusicianBase musician, float musicianVolume01)
        {
            float global = GameManager.GameplayData != null
                ? Mathf.Clamp01(GameManager.GameplayData.globalMusicVolume01)
                : 1f;

            float instrument = 1f; // TODO: Get instrumentSO volume

            float final = global * musicianVolume01 * instrument;
            return Mathf.Clamp01(final);
        }

        private void OnMusicianVolumeSliderChanged(
            MusicianBase musician, float sliderValue)
        {
            if (musician == null || MidiMusicManager == null)
                return;

            sliderValue = Mathf.Clamp01(sliderValue);
            _musicianVolume01[musician.CharacterId] = sliderValue;

            float finalVol = ComputeEffectiveMusicianVolume01(musician, sliderValue);
            MidiMusicManager.SetMusicianVolume01(musician.CharacterId, finalVol);

            if (useLogs)
                Debug.Log($"{DebugTag} [Dev] Volume slider for {musician.CharacterId} " +
                    $"slider={sliderValue:0.00} final={finalVol:0.00}");
        }

        private IEnumerator RunSongVibeResolution(SongFeedbackContext songCtx)
        {
            if (songCtx.PartCount == 0)
                yield break;

            yield return new WaitForSeconds(songEndPause);

            var deltas = ComputeSongVibeDeltas(songCtx);

            foreach (var entry in deltas)
            {
                var audience = CurrentAudienceCharacterList[entry.AudienceIndex];
                if (audience == null) continue;

                int baseDelta = entry.Delta;
                int flowStacks = 0;
                int flowBonus = 0;
                if (flowAddsFlatVibeBonus && baseDelta > 0)
                {
                    flowStacks = GetTotalFlowStacks();
                    flowBonus = flowStacks * flowVibeFlatBonusPerStack;
                }

                int finalDelta = baseDelta + flowBonus;

                // Floating text
                FxManager.Instance?.SpawnFloatingText(
                    audience.TextSpawnRoot,
                    flowBonus > 0
                        ? $"+{finalDelta} Vibe (Flow +{flowBonus})"
                        : $"+{finalDelta} Vibe",
                    0, 1, Color.cyan);

                // 1) apply Vibe
                audience.AudienceStats.AddVibe(finalDelta, duration: barFillDelay);

                if (useLogs && flowBonus > 0)
                    Debug.Log($"{DebugTag} [Flow→Vibe] {audience.CharacterId} base=+{baseDelta} flowStacks={flowStacks} perStack={flowVibeFlatBonusPerStack} bonus=+{flowBonus} final=+{finalDelta}");
                //Debug.Log($"{audience.CharacterId} filling Vibe in {barFillDelay}[s]");
                yield return new WaitForSeconds(barFillDelay);
                //Debug.Log($"{audience.CharacterId} bar filled");

                // TODO: Animate Vibe bar, emote, SFX, etc

                yield return new WaitForSeconds(perAudienceVibeDelay);
            }
        }

        private void ResolveGigOutcomeAndEnd()
        {
            bool win = true;
            foreach (var audienceCharacter in CurrentAudienceCharacterList)
            {
                if (!audienceCharacter.Stats.IsConvinced)
                {
                    win = false;
                    break;
                }
            }

            if (win) WinGig();
            else LoseGig();
        }

        #region Context Menus

        [ContextMenu("Debug/Force Win (Return Immediately)")]
        private void DebugForceWin_ReturnImmediately()
        {
            ReturnToMap(true);
        }

        [ContextMenu("Debug/Force Lose (Return Immediately)")]
        private void DebugForceLose_ReturnImmediately()
        {
            ReturnToMap(false);
        }

        [ContextMenu("Debug/Win (Normal Flow)")]
        private void DebugWin_NormalFlow()
        {
            WinGig();
        }

        [ContextMenu("Debug/Lose (Normal Flow)")]
        private void DebugLose_NormalFlow()
        {
            LoseGig();
        }

        #endregion
    }
}
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
using ALWTTT.Sensory;
using ALWTTT.Characters; // [S5a] VibeEffectiveness + AudienceCharacterCanvas telegraph

using MidiGenPlay;
using MidiGenPlay.Services;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ALWTTT.Tutorial;



#if ALWTTT_DEV
using ALWTTT.DevMode;
#endif

namespace ALWTTT.Managers
{
    public class GigManager : MonoBehaviour
    {
        public const string DebugTag = "<color=magenta>GigManager:</color>";

        public static GigManager Instance;

        // ─── Settings (M4.6F-2: split into 4 SOs) ─────────────────────────
        // All gameplay-tuning, presentation, and dev toggles formerly authored
        // as inline [SerializeField] fields now live on dedicated SOs. Each
        // GigManager scene still serializes references to one asset per SO.
        // Scene refs (cameras, hand, UI, scene changer, MidiGenPlayConfig,
        // songHypeDebugSlider) remain inline below.

        [Header("Settings (assets)")]
        [SerializeField, Tooltip("Composition rules + Action card gating + " +
            "Gig End behavior + setup-screen defaults.")]
        private GigFlowSettingsSO flow;

        [SerializeField, Tooltip("SongHype caps/seed, Vibe/Hype balance, " +
            "Flow→Vibe bifurcation, Loop scoring, Breakdown reset.")]
        private MeterTuningSO meters;

        [SerializeField, Tooltip("Audience beat curve/threshold, idle BPM, " +
            "sequence pacing values.")]
        private GigPresentationSO presentation;

        [SerializeField, Tooltip("Inspector-time dev toggles (logs + debug surfaces).")]
        private GigDevSettingsSO dev;

        [SerializeField] private AudioMixSettingsSO audioMix;   // M-AUDIO-MIX (D-MIX-HOME=B)

        // Public façade properties — preserved for callers written before F-2.
        public bool FlowActionFlatBonus => meters != null && meters.FlowActionFlatBonus;
        public int FlowActionVibeBonusPerStack => meters != null ? meters.FlowActionVibeBonusPerStack : 0;
        public float FlowVibeMultiplier => meters != null ? meters.FlowVibeMultiplier : 0f;
        public float BreakdownStressResetFraction => meters != null ? meters.BreakdownStressResetFraction : 0.5f;

        // ─── Scene refs (kept inline) ─────────────────────────────────────

        [Header("Cards / Hand")]
        [SerializeField] private HandController gigHand;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera handCamera;

        [Header("Composition UI")]
        [SerializeField] private SongCompositionUI compositionUI;
        [SerializeField] private LoopsTimerUI loopsTimerUI;
        [SerializeField] private MidiGenPlayConfig midiGenPlayConfig;

        [Header("References")]
        [SerializeField] private BackgroundContainer backgroundContainer;
        [SerializeField] private List<Transform> musicianPosList;
        [SerializeField] private List<Transform> audienceMemberPosList;
        [SerializeField] private SceneChanger sceneChanger;

        [SerializeField, Tooltip("[§5.3.5] Single spawn anchor for the " +
            "'+N Vibe!' floating text shown when SFX→FlatVibe fires on a " +
            "SongHype stage crossing. DC-SFX-Route=A: one band-canvas " +
            "floater, not per-audience. If unset, falls back to the first " +
            "band musician's TextSpawnRoot.")]
        private Transform sfxBonusVibeTextSpawnRoot;

        [Header("Dev Slider (scene UI)")]
        [SerializeField] private Slider songHypeDebugSlider;

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

        // [B2 / #6] Monotonic SongHype stage tracker (0..3). Reset on each new song
        // via ResetSongHype. Used by AddSongHype to fire venue SFX exactly once on
        // upward threshold crossings.
        private int _songHypeStage;

        // [S5a/T1] Song-scoped SFX Vibe accumulator (D-S5-VIBE=B refined). Stage
        // crossings bank their flat bonus here instead of applying mid-song; paid
        // out once at song end in RunSongVibeResolution. Reset alongside
        // _songHypeStage in ResetSongHype (covers song-start prep + post-payout).
        private int _pendingSfxVibe;

        private SongFeedbackContext? _lastSongFeedback;

        private InstrumentRepositoryResources _instrumentRepo;

        private readonly Dictionary<string, float> _musicianVolume01
            = new Dictionary<string, float>();
        private float _globalMusicVolume01 = 1f;   // M-AUDIO-MIX: runtime global; SO is persistence/default

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
        public float SongHype01
        {
            get
            {
                float max = meters != null ? meters.MaxSongHype : 0f;
                return max <= 0f ? 0f : Mathf.Clamp01(_songHype / max);
            }
        }
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
        // [B2 / #4] Read-only accessor for HandController + dev tooling. Null between
        // songs and before the first composition session of the gig.
        public CompositionSession CompositionSession => _session;

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
            if (dev != null && dev.UseLogs)
            {
                if (highlight)
                    Debug.Log($"{DebugTag} <color=yellow>{log}</color>");
                else if (!string.IsNullOrWhiteSpace(customColor))
                    Debug.Log($"{DebugTag} <color={customColor}>{log}</color>");
                else
                    Debug.Log($"{DebugTag} {log}");
            }
        }

        private bool UseLogs => dev != null && dev.UseLogs;

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
            // AUDIO-AMBIENCE: stop the crowd bed on gig teardown so it doesn't bleed into
            // the reward/menu scene. The fade runs on the DontDestroyOnLoad AudioManager,
            // so it completes across the scene unload.
            AudioManager.Instance?.StopAmbience();

            foreach (var m in _spawned)
            {
                if (m != null) m.UnbindFromGigContext();
            }
        }

        private void StartGig()
        {
            if (UseLogs) Debug.Log($"{DebugTag} Starting gig...");

            SetupEncounter();

            _requiredSongCount = ResolveRequiredSongCount();

            // [S4 D-S4-SRC=A] Lifecycle moment on the bus (tutorial welcome).
            StartCoroutine(PublishGigStartedDeferred());

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
            ApplyPersistedAudioMix();
            // AUDIO-AMBIENCE: crowd present from gig open — fade the looping bed in.
            AudioManager.Instance?.FadeInAmbience();

            if (UIManager != null && UIManager.GigCanvas != null)
                UIManager.GigCanvas.gameObject.SetActive(true);

            // Deck + hand binding
            SetupDeck();
            InitLoopScoringConfig();

            // MVP: initial planning window is open.
            _actionWindowOpen = true;
            _isSongPlaying = false;
            _isBetweenSongs = true;

            // IMPORTANT: Do NOT set the gig phase for now
            CurrentGigPhase = GigPhase.PlayerTurn;

            //_isSongPlaying = false;
            //_isBetweenSongs = _session != null;
        }

        private System.Collections.IEnumerator PublishGigStartedDeferred()
        {
            // One-shot lifecycle event: defer a frame so late-activated UI
            // (GigCanvas / TutorialController) is subscribed before it fires.
            yield return null;
            SensoryEventBus.Instance?.Publish(new GigStartedEvent(_requiredSongCount));
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
                        AddValid(runDeck.EnumerateCards());
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
                        AddValid(ctxDeck.EnumerateCards());
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
            if (UseLogs) Debug.Log($"{DebugTag} Setting up gig encounter...");

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
            if (UseLogs) Debug.Log($"{DebugTag} Building background...");
            backgroundContainer.OpenSelectedBackground();
            backgroundContainer.SetBPM(0);
        }

        private void BuildBand()
        {
            if (UseLogs) Debug.Log($"{DebugTag} Building band and musicians...");

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
            if (UseLogs) Debug.Log($"{DebugTag} Building audience...");
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
            bool debugHypeOn = dev != null && dev.DebugSongHype;
            if (songHypeDebugSlider != null &&
                songHypeDebugSlider.gameObject.activeSelf != debugHypeOn)
            {
                songHypeDebugSlider.gameObject.SetActive(debugHypeOn);

                if (debugHypeOn)
                    songHypeDebugSlider.SetValueWithoutNotify(_songHype);
            }

            // toggle instrument picker + volume debug with D
            if (Input.GetKeyDown(KeyCode.D) && dev != null)
            {
                dev.DebugInstrumentPicker = !dev.DebugInstrumentPicker;
                dev.DebugMusicianVolume = !dev.DebugMusicianVolume;

                if (UseLogs)
                {
                    Debug.Log($"{DebugTag} [Dev] Toggled debug UI → " +
                              $"Instruments={dev.DebugInstrumentPicker}, " +
                              $"Volume={dev.DebugMusicianVolume}");
                }

                SetupBandDebugElements();
            }
        }

        private void OnPlayPressed()
        {
            // MVP: once Play is pressed, action cards are no longer usable.
            _actionWindowOpen = false;

            // [B1 / #8] Honor `flow.DiscardActionCardsOnPlay` from
            // GigFlowSettingsSO. Configurability lives in the SO inspector
            // ("Action Card Gating (MVP)" header). Default is true → Action
            // cards discard at Play so they don't add noise during loop play.
            // Set to false on the SO to keep them in hand (combined with
            // flow.AllowActionCardsDuringPerformance for playability gating).
            bool shouldDiscardActions = flow != null && flow.DiscardActionCardsOnPlay;
            if (shouldDiscardActions && DeckManager != null)
            {
                DeckManager.DiscardHandWhere(card =>
                    card != null &&
                    card.CardDefinition != null &&
                    card.CardDefinition.IsAction);
            }

            // [B1 / D-J] Draw N cards when Play is pressed. Mirrors the
            // DrawPerLoop pattern. Configurable via GigFlowSettings SO
            // ("Composition" header → "Draw Cards On Play").
            int drawOnPlay = flow != null ? flow.DrawCardsOnPlay : 0;
            if (drawOnPlay > 0 && DeckManager.Instance != null)
            {
                DeckManager.Instance.DrawCards(drawOnPlay);
            }

            // Inject dev overrides into the UI model before building the SongConfig
            ApplyDebugInstrumentOverridesToCompositionModel();

            _session?.ConfirmCurrentPartAndStart();

            // AUDIO-AMBIENCE: the band starts the song → duck the crowd bed under the music.
            AudioManager.Instance?.FadeOutAmbience();

            // show the hype bar when music starts
            // [S5f / #6a] Gated by presentation.ShowSongHypeBar: the simplified
            // first-gig shape hides the bar (and the C1 readout under it).
            // SongHype accrual, stage SFX, and Vibe conversion are unaffected.
            // Null presentation preserves legacy behavior (show).
            if (UIManager != null && UIManager.GigCanvas != null &&
                (presentation == null || presentation.ShowSongHypeBar))
            {
                UIManager.GigCanvas.SetSongHypeVisible(true);
                UIManager.GigCanvas.SetSongHype(SongHype01);
            }

            // [S5a/T8+T11] Seed the projection (C1 readout + C2/C3 telegraph) now;
            // refreshed thereafter at each loop boundary and SFX stage crossing.
            if (UseLogs)
            {
                Debug.Log($"{DebugTag} [S5a-SMOKE] SONG-START pendingVibe(SFX)={_pendingSfxVibe} " +
                    $"(expect 0) SongHype01={SongHype01:F3}");
                if (CurrentAudienceCharacterList != null)
                    foreach (var a in CurrentAudienceCharacterList)
                        if (a != null)
                            Debug.Log($"{DebugTag} [S5a-SMOKE] WIRING '{a.CharacterId}' " +
                                $"canvas={(a.AudienceCharacterCanvas != null)} telegraphWired=" +
                                $"{(a.AudienceCharacterCanvas != null && a.AudienceCharacterCanvas.IsVibeTelegraphWired)}");
            }
            RefreshVibeProjection("song-start");
        }

        public void EndTurn()
        {
            if (UseLogs) Debug.Log($"{DebugTag} Ending turn...");

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

            if (UseLogs)
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

                    // [B3] Audience containers also tick on PlayerTurnStart. First user:
                    // Indifference (NegateIncomingPositive), which decays here so its
                    // gate weakens one stack per player turn. Pre-B3 the audience side
                    // only ticked on AudienceTurnStart (Earworm decay site); this loop
                    // closes the symmetry. Safe for existing audience statuses — Earworm
                    // uses AudienceTurnStart tick timing, so its decay path is unchanged.
                    foreach (var a in CurrentAudienceCharacterList)
                    {
                        a?.Statuses?.Tick(TickTiming.PlayerTurnStart);
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

                    // M4.5: bidirectional guaranteed draws (subtractive). Total drawn ≤ DrawCount.
                    // Guarantees ≥1 Composition and ≥1 Action in hand when piles allow.
                    // See SSoT_Runtime_Flow §4.2 and DeckManager.DrawCardsForPlayerTurn.
                    DeckManager.DrawCardsForPlayerTurn(GameManager.PersistentGameplayData.DrawCount);
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

                    // [S4 D-S4-SRC=A] Bridge OnEnemyTurnStarted onto the bus.
                    SensoryEventBus.Instance?.Publish(new AudienceTurnStartedEvent());

                    StartCoroutine(AudienceTurnRoutine());
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
                // [B2.5 / #3] BPM broadcasts to body + any sub-animators (instrument, etc.).
                musician.BroadcastBPM(song.BPM);
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

            ReapplyMusicianMix();

            Debug.Log($"Playing {song.SongTitle} for {songDuration}[s]");

            yield return MidiMusicManager.WaitForEnd();

            backgroundContainer.SetBPM(0);
            foreach (var musician in CurrentMusicianCharacterList)
            {
                // [B2.5 / #3] BPM broadcasts to body + any sub-animators.
                musician.BroadcastBPM(120);
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

            if (IsGigComplete && flow != null && flow.SkipAudienceActionsAfterFinalSong)
            {
                // [B3-demo-polish / F4] Run final-song Vibe conversion before
                // ending. The reaction wait above is the "audience reacts to song"
                // animation; this is the actual Vibe delta application.
                if (_lastSongFeedback.HasValue)
                {
                    Debug.Log($"{DebugTag} <color=lime>[F4] Running final-song Vibe " +
                              $"conversion before gig outcome resolution (site 1).</color>");
                    yield return RunSongVibeResolution(_lastSongFeedback.Value);
                    _lastSongFeedback = null;
                }

                ResolveGigOutcomeAndEnd();
                yield break;
            }

            CurrentGigPhase = GigPhase.AudienceTurn;
        }

        /// <summary>
        /// [B3-demo-polish / F4] On the final song with SkipAudienceActionsAfterFinalSong,
        /// run the song-end Vibe conversion FIRST (so the just-finished song's
        /// SongHype reaches audience CurrentVibe), THEN resolve the gig outcome.
        /// Without this, AudienceTurnRoutine is bypassed entirely and the final
        /// song's Vibe is never applied — causing premature loss declarations
        /// even when the player would have convinced the audience.
        /// </summary>
        private IEnumerator RunFinalSongVibeThenEnd()
        {
            if (_lastSongFeedback.HasValue)
            {
                Debug.Log($"{DebugTag} <color=lime>[F4] Running final-song Vibe " +
                          $"conversion before gig outcome resolution.</color>");
                yield return RunSongVibeResolution(_lastSongFeedback.Value);
                _lastSongFeedback = null;
            }
            else
            {
                Debug.LogWarning($"{DebugTag} [F4] No _lastSongFeedback at " +
                                 $"final-song outcome resolution — Vibe conversion skipped.");
            }

            ResolveGigOutcomeAndEnd();
        }

        private IEnumerator AudienceTurnRoutine()
        {
            // [S4 D-TUT-4] Cooperative suspend: hold the audience turn while a tutorial
            // modal is on screen. Animations/MIDI keep running (no timeScale freeze);
            // only this turn boundary waits.
            yield return new WaitUntil(() => !TutorialModalGate.IsActive);

            var waitDelay = new WaitForSeconds(
                presentation != null ? presentation.PerAudienceActionDelay : 1f);

            if (_lastSongFeedback.HasValue)
            {
                yield return RunSongVibeResolution(_lastSongFeedback.Value);
                _lastSongFeedback = null;

                // [B2.5 / D-8] Numeric + UI + beat intensity reset runs AFTER
                // RunSongVibeResolution has read SongHype01 to compute deltas.
                // The DeactivateAllSFX inside ResetSongHype is idempotent —
                // venue SFX were already cleared at audio-end via
                // OnCompositionSongFinished. The cycle is:
                //   Audio end → DeactivateAllSFX (D-5 intent: lights off now)
                //   AudienceTurn entry → RunSongVibeResolution (reads SongHype)
                //   AudienceTurn entry → ResetSongHype (zero everything else)
                //   AudienceTurn entry → ClearSongHype (UI hide)
                ResetSongHype();

                if (UIManager != null && UIManager.GigCanvas != null)
                {
                    UIManager.GigCanvas.ClearSongHype();
                }

                // [S5a] Hide the per-enemy telegraph too (the C1 readout hides with
                // songHypeRoot via ClearSongHype). Payout already consumed pendingVibe.
                HideVibeProjection();
            }

            // M4.3 (Earworm): apply per-stack Vibe gain to audience members holding Earworm,
            // BEFORE the AudienceTurnStart tick decays the stacks. Order matters here:
            // read stacks → apply Vibe → decay (handled by the existing Tick call below).
            // IsBlocked audiences are skipped per Design_Audience_Status_v1 §3.8 (consistent
            // with ComputeSongVibeDeltas). Convinced audiences tick harmlessly — AddVibe
            // [S5e] clamps at 0 and CheckConvincedThreshold guards re-firing.
            //
            // [B2.5 / #1] Staggered: a yield return waitDelay fires after each holder is
            // processed, so floating texts pace on the same cadence as audience actions
            // instead of piling up in one frame at the top of AudienceTurnRoutine.
            // The deferral hypothesis from B2 closure ("real Earworm tick lives elsewhere
            // in StatusEffectSO") was incorrect on inspection: StatusEffectContainer.Tick
            // only decays stacks; the only Earworm vibe-gain site is this block. The fix
            // was visual pacing, not relocation. See B2.5 D-B2.5-1 = A.
            foreach (var a in CurrentAudienceCharacterList)
            {
                if (a == null || a.Stats == null || a.Statuses == null) continue;
                if (a.IsBlocked) continue;

                if (!a.Statuses.TryGet(CharacterStatusId.DamageOverTime, out var inst)) continue;
                if (inst == null || inst.Stacks <= 0 || inst.Definition == null) continue;

                // Disambiguate from any future DamageOverTime variant on audience.
                if (!string.Equals(inst.Definition.StatusKey, "earworm",
                        System.StringComparison.OrdinalIgnoreCase))
                    continue;

                int beforeVibe = a.Stats.CurrentVibe;
                // [B3] Route through ApplyIncomingVibe canonical path. Indifference
                // (NegateIncomingPositive stacks > 0) blocks the Vibe gain entirely;
                // Earworm stack decay still happens via the container Tick call below
                // (independent of Vibe application — matches IsBlocked precedent per
                // Design_Audience_Status_v1 §3.8).
                int appliedEarworm = a.Stats.ApplyIncomingVibe(a.Statuses, inst.Stacks);
                int afterVibe = a.Stats.CurrentVibe;

                // [B2 / #3] Multiplier-with-icon floating text. Magenta tint
                // distinguishes Earworm-sourced Vibe gain from card-sourced
                // Vibe (cyan) and Flow-amplified Vibe (cyan with ×mult).
                // [B3] Branch on applied: blocked → grey INDIFFERENT (EARWORM).
                if (FxManager.Instance != null && a.TextSpawnRoot != null)
                {
                    if (appliedEarworm > 0)
                    {
                        FxManager.Instance.SpawnFloatingText(
                            a.TextSpawnRoot,
                            $"-{appliedEarworm} VIBE (EARWORM)", // [S5e] damage-number convention
                            new Vector2(-0.4f, 1.0f),
                            new Color(0.85f, 0.35f, 1.0f));
                    }
                    else
                    {
                        FxManager.Instance.SpawnFloatingText(
                            a.TextSpawnRoot,
                            "INDIFFERENT (EARWORM)",
                            new Vector2(-0.4f, 1.0f),
                            new Color(0.6f, 0.6f, 0.6f));
                    }
                }

                Debug.Log(
                    $"<color=lime>[Earworm] {a.CharacterId} stacks={inst.Stacks} " +
                    $"→ intended=+{inst.Stacks} applied=+{appliedEarworm} " +
                    $"(Vibe: {beforeVibe}→{afterVibe})</color>");

                // [B2.5 / #1] Pace per holder. Only yields when an Earworm holder was
                // actually processed (continues above skip without delay), so non-holder
                // audiences don't add latency to the turn.
                yield return waitDelay;
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

            // [B3-demo-polish / D-UX2] Wire Retry/Exit buttons on loss panel.
            UIManager.GigCanvas.OnLossRetry = HandleRetry;
            UIManager.GigCanvas.OnLossExit = HandleExit;

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
                // [B3-demo-polish / D-UX2] Wire Retry/Exit buttons on win panel.
                UIManager.GigCanvas.OnWinRetry = HandleRetry;
                UIManager.GigCanvas.OnWinExit = HandleExit;

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

        // [B3-demo-polish / D-UX2] Routed from GigCanvas.OnWinRetry / OnLossRetry.
        // Full scene reload. GigRunContext + PersistentGameplayData are
        // DontDestroyOnLoad so the same encounter config replays from scratch.
        // [F5] PD.CurrentSongIndex MUST be reset before reload — PD persists
        // across scene reload, and if CurrentSongIndex carries over from the
        // just-finished gig, the next gig instantly registers as IsGigComplete
        // on player turn entry (line 777-803), triggering an auto-loss before
        // the player can play a card. Resetting CurrentSongIndex=0 puts the
        // gig back to "fresh start" state. Other PD fields (Fans, BandCohesion,
        // GigsWon) intentionally NOT reset — Retry preserves earned progress.
        // IsFinalEncounter stays true (set by GigSetupController A6 patch).
        private void HandleRetry()
        {
            var pd = GameManager.PersistentGameplayData;
            int previousSongIndex = pd != null ? pd.CurrentSongIndex : -1;

            if (pd != null)
            {
                pd.CurrentSongIndex = 0;
            }

            Debug.Log($"{DebugTag} [B3-demo-polish / F5] HandleRetry → " +
                      $"PD.CurrentSongIndex reset {previousSongIndex} → 0, " +
                      $"IsFinalEncounter={pd?.IsFinalEncounter ?? false}. " +
                      $"Reloading current scene.");

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // [B3-demo-polish / D-UX2] Routed from GigCanvas.OnWinExit / OnLossExit.
        // Returns to the main menu / gig setup scene.
        private void HandleExit()
        {
            Debug.Log($"{DebugTag} [B3-demo-polish] HandleExit → returning to main menu.");
            UIManager.ChangeScene(0); // TODO Use reference id
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

        /// <summary>
        /// [S5b / Item 4 / D-S5b-ICON-RESOLVER=A] Owner icon for a card's fixed-performer
        /// type, resolved from the *current band*. Returns null for None / AnyMusician /
        /// any type not present in this gig's band -> the caller shows the no-icon fallback.
        /// In-gig only; outside a gig (no band) this returns null by design. The
        /// registry-backed variant that would also cover the inventory is the
        /// D-S5b-ICON-RESOLVER=B follow-up.
        /// </summary>
        public Sprite TryGetMusicianIcon(MusicianCharacterType type)
        {
            var mus = ResolveMusicianByType(type);
            return mus != null ? mus.MusicianCharacterData?.CharacterIcon : null;
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
            _session.Begin(ctx, flow != null ? flow.JamRules : new JamRules(),
                midiGenPlayConfig, _rng);

            // starting hype before the first loop finishes
            // IDEAS
            // PersistentGameplayData.Fans (more fans → higher baseline hype).
            // Venue type / difficulty (small club vs arena).
            // [S3 D-S3-7=A] Seed the per-song baseline silently — must NOT fire
            // stage crossings or the SFX->Vibe bonus during prep (between songs).
            float startHype = meters != null ? meters.StartingSongHype : 0f;
            if (startHype > 0f)
                SeedSongHype(startHype);

            _isSongPlaying = false;
            _isBetweenSongs = true;
        }

        private void ResetSongHype()
        {
            _songHype = 0f;
            _songHypeStage = 0; // [B2 / #6] reset stage on song boundary
            _pendingSfxVibe = 0; // [S5a/T1] banked SFX is song-scoped; clear with the stage
            UpdateAudienceBeatIntensity();
            OnSongHypeChanged01?.Invoke(SongHype01);

            // [B2.5 / #2] Clear any active venue SFX so the next song starts visually
            // fresh. Lights/smoke/fire only re-fire on the next AddSongHype crossing.
            if (backgroundContainer != null)
                backgroundContainer.DeactivateAllSFX();
        }

        private void OnCompositionLoopFinished(LoopFeedbackContext loopCtx)
        {
            TriggerAudienceMicroReactions(loopCtx);

            // [S5a/T8+T11] Loop-boundary refresh of the Vibe projection (runs before
            // the F-3 early-return below, so it is independent of per-loop draw config).
            RefreshVibeProjection("loop");

            // [S4 D-S4-BUS=B] beat 3 bridge of CompositionSession.LoopFinished.
            // Carries InspirationGainedThisLoop (track gain); fires before the F-3
            // early-return below so beat 3 is independent of per-loop draw/insp config.
            SensoryEventBus.Instance?.Publish(new LoopResolvedEvent(loopCtx));

            // M4.6F-3: per-loop draw + per-loop inspiration consumption.
            // Hook lives here (host-owned subscriber) to respect the
            // CompositionSession deck-mutation invariant ([Obsolete] guards
            // on CompositionSession.PrepareDeck and ICompositionContext.Deck).
            int drawN = flow != null ? flow.DrawPerLoop : 0;
            var pd = GameManager != null ? GameManager.PersistentGameplayData : null;
            int inspN = pd != null ? pd.InspirationPerLoop : 0;

            // Skip silently when F-3 is fully disabled.
            if (drawN <= 0 && inspN <= 0) return;

            if (drawN > 0 && DeckManager.Instance != null)
            {
                // DeckManager.DrawCards clamps to MaxCardsOnHand internally.
                DeckManager.Instance.DrawCards(drawN);
            }

            int inspirationDelta = 0;
            if (inspN > 0 && _session != null)
            {
                // Canonical path: AddCurrentInspiration clamps to MaxInspiration,
                // mirrors to pd.CurrentInspiration, returns the actual delta.
                inspirationDelta = _session.AddCurrentInspiration(inspN);
            }

            if (dev != null && dev.UseLogs && dev.UseCompositionLogs)
            {
                int handAfter = DeckManager.Instance != null
                    ? DeckManager.Instance.HandPile.Count
                    : -1;
                int currentInsp = pd != null ? pd.CurrentInspiration : -1;
                Debug.Log($"{DebugTag} <color=cyan>[F-3]</color> LoopFinished hook: " +
                    $"drawRequested={drawN}, " +
                    $"inspirationDelta={inspirationDelta}, " +
                    $"hand={handAfter}, " +
                    $"inspiration={currentInsp}");
            }
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

            // [B2.5 / D-8] Surgical split of the original D-5 move. Only kill the
            // venue SFX here — preserves the lights-off-at-audio-end UX that D-5
            // was aiming for. The rest of ResetSongHype() (which zeroes _songHype
            // numeric, fires OnSongHypeChanged01, updates audience beat intensity)
            // CANNOT run here: RunSongVibeResolution reads SongHype01 downstream
            // to compute baseVibe for ComputeSongVibeDeltas. Resetting numeric
            // state before that read produced empty deltas (D-5 regression).
            // Full ResetSongHype() now runs in AudienceTurnRoutine AFTER
            // RunSongVibeResolution has consumed the value.
            if (backgroundContainer != null)
                backgroundContainer.DeactivateAllSFX();

            // Reset BPM/Animation
            ResetStageToIdle();
        }

        public bool CanPlayActionCard(CardDefinition card)
        {
            if (card == null) return false;
            if (!card.IsAction) return false;

            var actionPayload = card.ActionPayload;
            if (actionPayload == null) return false;

            // During performance we default to disabling action cards in the MVP.
            // [§5.3.5 demo unblock] When AllowActionCardsDuringPerformance is on,
            // ALL action cards are playable during performance — the original
            // Always-tag co-condition is dropped because per-loop-drawn action
            // cards in the starter deck aren't all tagged Always, and the demo
            // needs the broad path to keep DrawPerLoop draws useful. Flag-off
            // semantic unchanged (returns false). Always-specific precision
            // gating is deferred to a post-demo content batch if needed.
            if (_isSongPlaying)
            {
                bool allowDuringPerformance = flow != null && flow.AllowActionCardsDuringPerformance;
                return allowDuringPerformance;
            }

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

        private void InitLoopScoringConfig()
        {
            if (meters == null)
            {
                Debug.LogError($"{DebugTag} InitLoopScoringConfig: MeterTuningSO " +
                    "is unwired; loop scoring will use struct defaults.");
                return;
            }

            // Total musicians
            meters.LoopScoringConfigRef.totalMusicians =
                CurrentMusicianCharacterList?.Count ?? 2;

            // Possible roles: scan composition cards in deck for distinct TrackRoles
            var possibleRoles = new HashSet<TrackRole>();
            var deck = GameManager.PersistentGameplayData?.CurrentCompositionCards;
            if (deck != null)
            {
                foreach (var card in deck)
                {
                    var comp = card?.CompositionPayload;
                    if (comp?.TrackAction != null)
                        possibleRoles.Add(comp.TrackAction.role);
                }
            }

            meters.LoopScoringConfigRef.possibleRoleCount =
                Mathf.Max(1, possibleRoles.Count);

            Debug.Log($"{DebugTag} [Scoring] Init: " +
                $"mode={meters.LoopScoringConfig.mode} " +
                $"possibleRoles={meters.LoopScoringConfig.possibleRoleCount} " +
                $"totalMusicians={meters.LoopScoringConfig.totalMusicians}");
        }

        // Called whenever one full loop finishes (including the last loop of the song)
        private void TriggerAudienceMicroReactions(LoopFeedbackContext loopCtx)
        {
            if (CurrentAudienceCharacterList == null
                || CurrentAudienceCharacterList.Count == 0)
                return;

            float loopScore = LoopScoreCalculator.ComputeLoopScore(
                loopCtx, meters != null ? meters.LoopScoringConfig : LoopScoringConfig.Default);
            float baseHypeDelta = LoopScoreCalculator.ComputeHypeDelta(
                loopScore, meters != null ? meters.HypeThresholds : HypeThresholds.Default);
            float hypeDelta = baseHypeDelta * (meters != null ? meters.SongHypeDeltaMultiplier : 1f);

            // Flow → SongHype path RETIRED (M4.2). Removed entirely.

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

                // [S3 D-S3-4=A] Direct FT call deleted; the bus + SensoryFxAdapter
                // (Spawn mode) is now the sole FT source for this surface.
                SensoryEventBus.Instance?.Publish(new AudienceReactionEvent(
                    audience, i, audience.CharacterId, raw, clamped, loopCtx));

                // TODO plug things like:
                // audience.Stats.AddVibe(clamped);
                // trigger VFX/SFX, etc.
            }
        }

        internal void OnCompositionSessionEnded()
        {
            bool wasPlaying = _isSongPlaying;   // AUDIO-AMBIENCE: was a song performing?

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

            // AUDIO-AMBIENCE: song ended → return the crowd bed. The session is nulled
            // synchronously inside CompositionSession.Tick, so GigManager.Update takes the
            // `_session == null` early-return and the Update song-edge is bypassed — the
            // return must fire here, the deterministic song-end chokepoint.
            if (wasPlaying) AudioManager.Instance?.FadeInAmbience();

#if ALWTTT_DEV
            Debug.Log($"{DebugTag} <color=lime>[DevMode] OnCompositionSessionEnded reached. " +
                      $"InfiniteTurnsEnabled={DevModeController.InfiniteTurnsEnabled}, " +
                      $"DeckManager.Instance null? {DeckManager.Instance == null}, " +
                      $"IsGigComplete={IsGigComplete}, " +
                      $"skipAudienceActionsAfterFinalSong={(flow != null && flow.SkipAudienceActionsAfterFinalSong)}, " +
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

            if (IsGigComplete && flow != null && flow.SkipAudienceActionsAfterFinalSong)
            {
                // [B3-demo-polish / F4] Run Vibe conversion via coroutine before
                // resolving outcome. Previously called ResolveGigOutcomeAndEnd
                // directly, which bypassed RunSongVibeResolution entirely.
                StartCoroutine(RunFinalSongVibeThenEnd());
                return;
            }

            // Hand control to the existing gig phase system:
            // this will call AudienceTurnRoutine() and run all audience actions.
            CurrentGigPhase = GigPhase.AudienceTurn;
        }

        private void AddSongHype(float delta)
        {
            if (dev != null && dev.DebugSongHype)
                return;
            AddSongHypeCore(delta);
        }

        /// <summary>
        /// Core SongHype mutation: bounds-clamps, syncs UI, fires threshold
        /// detection, raises change event. Bypasses the DebugSongHype guard.
        /// Gameplay path uses <see cref="AddSongHype"/> (which respects the
        /// guard); dev tools use DevAddSongHype / DevResetSongHype
        /// (which bypass it; compile-gated under ALWTTT_DEV per S5f/#15).
        /// </summary>
        private void AddSongHypeCore(float delta)
        {
            float max = meters != null ? meters.MaxSongHype : 0f;
            float beforeNorm = max <= 0f ? 0f : Mathf.Clamp01(_songHype / max);
            _songHype = Mathf.Clamp(_songHype + delta, 0f, max);
            float afterNorm = max <= 0f ? 0f : Mathf.Clamp01(_songHype / max);

            UpdateAudienceBeatIntensity();

            // Keep the slider in sync (even if it's hidden)
            if (songHypeDebugSlider != null)
                songHypeDebugSlider.SetValueWithoutNotify(_songHype);

            // [B2 / #6] Threshold crossings → venue SFX.
            EvaluateSongHypeThresholds(beforeNorm, afterNorm);

            OnSongHypeChanged01?.Invoke(SongHype01);
        }

#if ALWTTT_DEV
        /// <summary>
        /// [B2 / #6] Dev tool: adjust SongHype by an absolute delta, bypassing
        /// the DebugSongHype guard. Routes through threshold detection so
        /// stage SFX fire correctly on upward crossings.
        /// [S5f / #15] Compile-gated: dev-only surface, stripped from
        /// non-dev builds (only caller is DevModeController).
        /// </summary>
        public void DevAddSongHype(float delta) => AddSongHypeCore(delta);

        /// <summary>
        /// [B2 / #6] Dev tool: reset SongHype to 0 and reset the per-song stage
        /// counter, simulating a new-song boundary. Lets devs re-test threshold
        /// crossings without finishing a real song.
        /// [S5f / #15] Compile-gated (see DevAddSongHype).
        /// </summary>
        public void DevResetSongHype() => ResetSongHype();
#endif

        /// <summary>
        /// [S3 D-S3-7=A] Seeds the per-song starting hype WITHOUT firing stage
        /// crossings or the SFX->Vibe bonus, and fast-forwards the stage counter
        /// past any threshold the seed already meets, so only performance-driven
        /// hype beyond the seed triggers venue VFX. Respects the DebugSongHype
        /// guard, matching the AddSongHype path it replaces at the seed site.
        /// </summary>
        private void SeedSongHype(float value)
        {
            if (dev != null && dev.DebugSongHype) return;

            float max = meters != null ? meters.MaxSongHype : 0f;
            _songHype = Mathf.Clamp(value, 0f, max);
            _songHypeStage = StageForNormalized(SongHype01);

            UpdateAudienceBeatIntensity();
            if (songHypeDebugSlider != null)
                songHypeDebugSlider.SetValueWithoutNotify(_songHype);

            OnSongHypeChanged01?.Invoke(SongHype01);
        }

        /// <summary>[S3 D-S3-7=A] Highest stage (0..3) a normalized hype already satisfies.</summary>
        private int StageForNormalized(float norm)
        {
            if (presentation == null) return 0;
            int s = 0;
            if (norm >= presentation.SongHypeStage1Threshold) s = 1;
            if (norm >= presentation.SongHypeStage2Threshold) s = 2;
            if (norm >= presentation.SongHypeStage3Threshold) s = 3;
            return s;
        }

        /// <summary>
        /// [B2 / #6] Fires venue SFX exactly once per upward threshold crossing
        /// (1/3 → lights, 2/3 → smoke, 3/3 → fire). Stage tracker resets per song
        /// via ResetSongHype. Downward motion does not re-trigger.
        /// </summary>
        private void EvaluateSongHypeThresholds(float before, float after)
        {
            if (presentation == null) return;
            if (after <= before) return; // upward only

            float t1 = presentation.SongHypeStage1Threshold;
            float t2 = presentation.SongHypeStage2Threshold;
            float t3 = presentation.SongHypeStage3Threshold;

            if (_songHypeStage < 1 && after >= t1 && before < t1)
            {
                _songHypeStage = 1;
                FireSongHypeStage(1, presentation.SongHypeStage1SfxTag);
            }
            if (_songHypeStage < 2 && after >= t2 && before < t2)
            {
                _songHypeStage = 2;
                FireSongHypeStage(2, presentation.SongHypeStage2SfxTag);
            }
            if (_songHypeStage < 3 && after >= t3 && before < t3)
            {
                _songHypeStage = 3;
                FireSongHypeStage(3, presentation.SongHypeStage3SfxTag);
            }
        }

        private void FireSongHypeStage(int stage, string sfxTag)
        {
            if (backgroundContainer != null && !string.IsNullOrEmpty(sfxTag))
                backgroundContainer.ActivateSFX(sfxTag);

            // [§5.3.5] SFX → FlatVibe bonus. Routes per-audience through
            // ApplyIncomingVibe (DC-SFX-Route=A): Indifference still blocks
            // per member, but the floater is a single band-canvas "+N Vibe!"
            // (not per-audience). Bonus value is flat — bypasses Flow
            // multiplier — and tuned on GigPresentationSO.
            ApplySfxBonusVibe(stage);

            // [S5a/T8+T11] pendingVibe just grew -> refresh the readout/telegraph so
            // the banked SFX shows immediately at the crossing.
            RefreshVibeProjection("stage");

            // [S3-audio D-SA-5]
            SensoryEventBus.Instance?.Publish(new SfxStageCrossedEvent(stage, sfxTag));

            Debug.Log($"{DebugTag} <color=yellow>[B2 / #6] SongHype stage {stage} " +
                $"reached (tag='{sfxTag}'). _songHype={_songHype:F2}, " +
                $"max={(meters != null ? meters.MaxSongHype : 0f):F2}</color>");
        }

        /// <summary>
        /// [§5.3.5] Applies the per-stage SFX bonus Vibe (D-DCP-2=A defaults
        /// 3/6/10). Per DC-SFX-Route=A: each audience member receives +N
        /// via the canonical <see cref="IAudienceStats.ApplyIncomingVibe"/>
        /// path, so Indifference stacks still gate per member (D-DCP-6=A).
        /// A single "+N Vibe!" floating text is spawned at the band-canvas
        /// anchor (or the first valid musician's TextSpawnRoot as fallback)
        /// — and only if at least one audience member accepted the bonus
        /// (avoids a misleading floater when every audience is Indifferent).
        /// </summary>
        private void ApplySfxBonusVibe(int stage)
        {
            if (presentation == null) return;

            float bonus = presentation.GetSfxBonusVibe(stage);
            int bonusInt = Mathf.RoundToInt(bonus);
            if (bonusInt <= 0) return;

            // [S5a/T2] D-S5-VIBE=B refined: do NOT apply Vibe mid-song. Bank the flat
            // per-stage bonus into the song-scoped accumulator and pay it out once at
            // song end (RunSongVibeResolution), gated per-member there. Total is
            // identical to the pre-refactor sum of per-stage applications (regression
            // ST-S5a-1); the only change is WHEN it lands (and no double-application).
            _pendingSfxVibe += bonusInt;

            // Gold "+N Vibe!" floater retained as a "banked" cue - it no longer implies
            // application. Shown whenever the stage fired, regardless of per-member
            // Indifference: banking is global; gating happens at song end.
            Transform spawn = ResolveSfxBonusVibeSpawnRoot();
            if (spawn != null && FxManager.Instance != null)
            {
                FxManager.Instance.SpawnFloatingText(
                    spawn,
                    $"+{bonusInt} Vibe!",
                    0, 1,
                    new Color(1f, 0.85f, 0.25f)); // warm gold - banked SFX cue
            }

            if (UseLogs)
                Debug.Log($"{DebugTag} <color=yellow>[S5a-SMOKE] BANK stage={stage} +{bonusInt} " +
                    $"pendingVibe={_pendingSfxVibe} (banked, NOT applied this turn)</color>");
        }

        /// <summary>
        /// [§5.3.5] Resolves the spawn root for the SFX→FlatVibe "+N Vibe!"
        /// floater. Prefers the serialized inspector field; otherwise falls
        /// back to the first band musician's <see cref="CharacterBase.TextSpawnRoot"/>.
        /// Returns null if neither is available (caller suppresses the spawn).
        /// </summary>
        private Transform ResolveSfxBonusVibeSpawnRoot()
        {
            if (sfxBonusVibeTextSpawnRoot != null)
                return sfxBonusVibeTextSpawnRoot;

            foreach (var m in CurrentMusicianCharacterList)
            {
                if (m != null && m.TextSpawnRoot != null)
                    return m.TextSpawnRoot;
            }
            return null;
        }

        /// <summary>
        /// Production-path inspiration mutator. Routes through
        /// CompositionSession.AddCurrentInspiration when a session is active so
        /// pd.CurrentInspiration, _session._currentInspiration, and the composition
        /// UI all stay in sync. Falls back to a direct pd write when no session is
        /// active (between gigs / pre-StartGig).
        ///
        /// Returns the actual delta applied (post-clamp). Clamps to
        /// [0, pd.MaxInspiration]. Pass a negative delta to spend, positive to
        /// generate.
        ///
        /// MB4 (2026-05-08) — closes the action-card dual-siting symmetric to F-3's
        /// closure of the comp-card / per-loop-gain dual-siting. See
        /// SSoT_Dev_Mode §13.4.
        /// </summary>
        public int AdjustInspiration(int delta)
        {
            if (delta == 0) return 0;

            if (_session != null && _session.IsActive)
                return _session.AddCurrentInspiration(delta);

            var pd = GameManager.PersistentGameplayData;
            if (pd == null) return 0;

            int before = pd.CurrentInspiration;
            int after = Mathf.Clamp(before + delta, 0, pd.MaxInspiration);
            pd.CurrentInspiration = after;
            return after - before;
        }

#if ALWTTT_DEV
        /// <summary>Dev Mode: expose max for UI slider bounds. Dev-only.</summary>
        public float MaxSongHype => meters != null ? meters.MaxSongHype : 0f;

        /// <summary>
        /// Dev Mode: current authoritative inspiration value for the Stats-tab slider.
        /// Returns the live session budget when a CompositionSession is active,
        /// otherwise the persistent PD value. See SSoT_Dev_Mode §13.2 / §13.4.
        /// </summary>
        public int LiveInspiration =>
            (_session != null && _session.IsActive)
                ? _session.CurrentInspiration
                : (GameManager.PersistentGameplayData?.CurrentInspiration ?? 0);

        /// <summary>
        /// Dev Mode: true when a CompositionSession exists and is between
        /// Begin() and End(). Used by the Stats-tab raw [PD/Session] readout
        /// to decide whether to show a session value at all.
        /// </summary>
        public bool IsCompositionSessionActive => _session != null && _session.IsActive;

        /// <summary>
        /// Dev Mode: set SongHype directly. Bypasses debugSongHype guard (unlike AddSongHype).
        /// Fires OnSongHypeChanged01 so subscribed UI (hype bar, audience beat intensity) repaints.
        /// </summary>
        public void DevSetSongHype(float value) => DevSetSongHypeAbsolute(value);

        // ── M-AUDIO-MIX Dev wrappers (Audio Mix tab) ──────────────────────────
        public float DevGlobalMusicVolume01 => Mathf.Clamp01(_globalMusicVolume01);

        public void DevSetGlobalMusicVolume01(float v)
        {
            _globalMusicVolume01 = Mathf.Clamp01(v);
            ReapplyMusicianMix();                                  // live: folds into every musician (gig music)
            MusicDirector.Instance?.RefreshMusicLevel();          // AUDIO-OST: same Music level scales OST
            if (audioMix != null) audioMix.GlobalMusicVolume01 = _globalMusicVolume01;
            PersistAudioMixInEditor();
        }

        public float DevGetMusicianVolume01(string musicianId) =>
            _musicianVolume01.TryGetValue(musicianId, out var v) ? v : 1f;

        public void DevSetMusicianVolume01(MusicianBase musician, float v)
        {
            if (musician == null) return;
            v = Mathf.Clamp01(v);
            _musicianVolume01[musician.CharacterId] = v;
            if (MidiMusicManager != null)
                MidiMusicManager.SetMusicianVolume01(
                    musician.CharacterId, ComputeEffectiveMusicianVolume01(musician, v));
            if (audioMix != null) audioMix.SetMusicianVolume01(musician.CharacterId, v);
            PersistAudioMixInEditor();
        }

        public float DevMasterSfxVolume01 =>
            AudioManager.Instance != null
                ? AudioManager.Instance.SfxVolume01
                : (audioMix != null ? audioMix.MasterSfxVolume01 : 1f);

        public void DevSetMasterSfxVolume01(float v)
        {
            v = Mathf.Clamp01(v);
            if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume01(v);   // live, SO-independent
            if (audioMix != null) audioMix.MasterSfxVolume01 = v;
            PersistAudioMixInEditor();
        }

        public bool DevHasAudioMixAsset => audioMix != null;

        private void PersistAudioMixInEditor()
        {
#if UNITY_EDITOR
            if (audioMix == null) return;
            UnityEditor.EditorUtility.SetDirty(audioMix);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        /// <summary>
        /// [S3 D-S3-8=A] Absolute Dev set that ALSO fires stage-threshold crossings
        /// (venue VFX + SFX->Vibe bonus), restoring parity with the ±10% buttons.
        /// Both Dev SongHype sliders route here; prior to S3 they set the value
        /// directly and skipped crossing detection (Dev regression).
        /// </summary>
        private void DevSetSongHypeAbsolute(float value)
        {
            float max = meters != null ? meters.MaxSongHype : 0f;
            float beforeNorm = max <= 0f ? 0f : Mathf.Clamp01(_songHype / max);
            _songHype = Mathf.Clamp(value, 0f, max);
            float afterNorm = max <= 0f ? 0f : Mathf.Clamp01(_songHype / max);

            UpdateAudienceBeatIntensity();
            if (songHypeDebugSlider != null)
                songHypeDebugSlider.SetValueWithoutNotify(_songHype);

            EvaluateSongHypeThresholds(beforeNorm, afterNorm);
            OnSongHypeChanged01?.Invoke(SongHype01);
        }

        /// <summary>
        /// Dev Mode: set CurrentInspiration, clamped to [0, MaxInspiration].
        /// Always writes to pd.CurrentInspiration. When a CompositionSession is active,
        /// also routes to CompositionSession.DevSetCurrentInspiration so the live
        /// composition budget (read by the card-cost gate and composition UI) reflects
        /// the slider write. See SSoT_Dev_Mode §13.2.
        /// </summary>
        public void DevSetInspiration(int value)
        {
            var pd = GameManager.PersistentGameplayData;
            if (pd == null) return;

            int clamped = Mathf.Clamp(value, 0, pd.MaxInspiration);
            int before = pd.CurrentInspiration;
            pd.CurrentInspiration = clamped;

            bool sessionRouted = _session != null && _session.IsActive;
            if (sessionRouted)
                _session.DevSetCurrentInspiration(clamped);

            Debug.Log($"{DebugTag} <color=lime>[DevMode] DevSetInspiration " +
                $"before={before} → after={clamped} sessionRouted={(sessionRouted ? "Y" : "N")}</color>");
        }

        /// <summary>
        /// Dev Mode: set BandCohesion. No upper cap. Floor at 0.
        /// On reaching 0, dispatches LoseGig() per the symmetric-consequences principle
        /// (SSoT_Dev_Mode §13.2 / §13.3). LoseGig's Infinite-Turns suppression branch applies:
        /// OFF → triggers the loss panel; ON → logs suppression and continues the gig.
        /// Mirrors the natural Breakdown → Cohesion−1 → LoseGig path in MusicianBase.OnBreakdown.
        /// </summary>
        public void DevSetBandCohesion(int value)
        {
            var pd = GameManager.PersistentGameplayData;
            if (pd == null) return;
            pd.BandCohesion = Mathf.Max(0, value);
            if (pd.BandCohesion == 0) LoseGig();
        }

        /// <summary>
        /// Dev Mode: expose aggregate Flow stacks for UI readout.
        /// Sum across all live musicians' DamageUpFlat stacks — identical to the
        /// private GetTotalFlowStacks path used by scoring.
        /// </summary>
        public int TotalFlowStacks => GetTotalFlowStacks();

        /// <summary>
        /// Dev Mode: apply a Flow stack delta to every live musician's status container.
        /// Flow is song/band-scoped in gameplay terms (aggregated via GetTotalFlowStacks,
        /// reset via ResetSongScopedStatuses); editing it gig-wide means applying the
        /// same delta uniformly to each musician. StatusEffectContainer clamps ≤0 by
        /// auto-clearing, so negative deltas below 0 are safe.
        /// Resolves the "flow" SO via the first available StatusCatalogue.
        /// No-op if no musician has a catalogue assigned or the key is missing.
        /// </summary>
        public void DevAddFlowToAllMusicians(int delta)
        {
            if (delta == 0) return;
            if (CurrentMusicianCharacterList == null) return;

            StatusEffectSO flowSO = null;
            for (int i = 0; i < CurrentMusicianCharacterList.Count; i++)
            {
                var m = CurrentMusicianCharacterList[i];
                if (m == null || m.StatusCatalogue == null) continue;
                if (m.StatusCatalogue.TryGetByKey("flow", out flowSO) && flowSO != null)
                    break;
            }
            if (flowSO == null) return;

            for (int i = 0; i < CurrentMusicianCharacterList.Count; i++)
            {
                var m = CurrentMusicianCharacterList[i];
                if (m == null || m.Statuses == null) continue;

                // Pre-guard: skip Apply(-N) when stacks==0 to avoid spurious transient
                // instance creation + immediate clear (fires OnStatusCleared on an
                // entry that never really existed).
                if (delta < 0 && m.Statuses.GetStacks(CharacterStatusId.DamageUpFlat) <= 0)
                    continue;

                m.Statuses.Apply(flowSO, delta);
            }
        }
#endif

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
            int maxVibeFromHype = meters != null ? meters.MaxVibeFromSongHype : 0;
            float baseVibe = SongHype01 * maxVibeFromHype;

            // 3) Convert to per-audience vibe deltas
            for (int i = 0; i < audienceCount; i++)
            {
                var audience = CurrentAudienceCharacterList[i];
                if (audience == null) continue;
                // Blocked chars get no vibe
                if (audience.IsBlocked)
                {
                    if (UseLogs)
                        Debug.Log($"{DebugTag} [S5a-SMOKE] SONG-END   i={i} " +
                            $"'{audience.CharacterId}' reason=IsBlocked (Immune, 0 Vibe)");
                    continue;
                }

                float avgImpression =
                    sampleCounts[i] > 0 ? totalImpression[i] / sampleCounts[i] : 0f; // [-2, 2]

                // Map avgImpression [-2,2] → multiplier [0.5, 1.5]
                float impressionFactor = 1f + (avgImpression * 0.25f);

                float vibeFloat = baseVibe * impressionFactor;
                int vibeDelta = Mathf.RoundToInt(vibeFloat);

                // [S5a/T4] Floor the L-part at 0 (no negative macro Vibe) but KEEP every
                // non-blocked member - even L==0 - so SFX-only members still pay out at
                // song end. Zero-total suppression now lives at the apply site in
                // RunSongVibeResolution (combined L+SFX <= 0 -> no event/FT).
                int lPart = Mathf.Max(0, vibeDelta);

                result.Add(new AudienceVibeDelta
                {
                    AudienceIndex = i,
                    Delta = lPart
                });
            }

            return result;
        }

        // TODO: Move all animation logic to its own class ie "BandAnimator" etc
        public void ApplyBpmToStage(int partIndex, int bpm)
        {
            _currentBpm = bpm;

            if (UseLogs)
                Debug.Log($"{DebugTag} [Gig] Part {partIndex} BPM resolved → {bpm}");

            // 1) Background pulse
            if (backgroundContainer != null)
            {
                backgroundContainer.SetBPM(bpm);
            }

            // 2) Band animators — [S5b / Item 3 / D-S5b-QUERY-HOME=A] gate the "playing"
            // animation to musicians who own a track in the part being played this loop.
            // Non-performers get the idle treatment (gentle sway, no jump, no particles)
            // so a silent musician doesn't read as "playing". The active set is recomputed
            // every loop, so a track added mid-loop animates on the same loop its stem
            // first re-renders.
            var activeMusicianIds = _session != null
                ? _session.GetMusicianIdsWithTrackInPart(partIndex)
                : null;

            foreach (var musician in CurrentMusicianCharacterList)
            {
                if (musician == null || musician.CharacterAnimator == null)
                    continue;

                var anim = musician.CharacterAnimator;
                var musicianId = musician.MusicianCharacterData != null
                    ? musician.MusicianCharacterData.CharacterId : null;

                bool performing = activeMusicianIds != null
                    && !string.IsNullOrEmpty(musicianId)
                    && activeMusicianIds.Contains(musicianId);

                // [B2.5 / #3] BPM broadcasts to body + sub-animators (everyone stays on clock).
                musician.BroadcastBPM(bpm);

                if (performing)
                {
                    anim.SetBeatAnimationEnabled(true);   // [S5b / D-S5b-IDLE] resume if previously stopped
                    anim.SkipEveryNBeats = 1;
                    anim.BeatOffsetBeats = UnityEngine.Random.Range(0f, 0.15f);
                    anim.JumpOnBeat = true;
                    anim.RotateOnBeat = false;
                    anim.EmitOnBeat = true;
                }
                else
                {
                    // [S5b / D-S5b-IDLE] Not performing this loop -> stop all beat motion
                    // and settle to rest pose (no sway, no particles). The master gate
                    // keeps the authored style intact for when this musician plays again.
                    anim.SetBeatAnimationEnabled(false);
                }
            }

            // 3) Audience animators – follow BPM, intensity handled by SongHype
            foreach (var audience in CurrentAudienceCharacterList)
            {
                if (audience == null || audience.CharacterAnimator == null)
                    continue;

                var anim = audience.CharacterAnimator;

                // [B2.5 / #3] BPM broadcasts to body + any sub-animators.
                audience.BroadcastBPM(bpm);
                anim.SkipEveryNBeats = 1;
                anim.BeatOffsetBeats = UnityEngine.Random.Range(0f, 0.15f);
            }
        }

        private void ResetStageToIdle()
        {
            if (backgroundContainer != null)
                backgroundContainer.SetBPM(0);

            int idleBpmLocal = presentation != null ? presentation.IdleBpm : 120;

            foreach (var musician in CurrentMusicianCharacterList)
            {
                if (musician == null || musician.CharacterAnimator == null)
                    continue;

                var anim = musician.CharacterAnimator;

                // [B2.5 / #3] BPM broadcasts to body + any sub-animators.
                musician.BroadcastBPM(idleBpmLocal);
                anim.SetBeatAnimationEnabled(true);   // [S5b / D-S5b-IDLE] resume idle sway between songs
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

                // [B2.5 / #3] BPM broadcasts to body + any sub-animators.
                audience.BroadcastBPM(idleBpmLocal);
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
            var curve = presentation != null ? presentation.AudienceJumpIntensityCurve : null;
            float threshold = presentation != null ? presentation.AudienceJumpThreshold : 0.1f;

            float intensity = curve != null && curve.length > 0
                ? Mathf.Clamp01(curve.Evaluate(t))
                : t * t; // fallback: simple ease-in quadratic

            foreach (var audience in CurrentAudienceCharacterList)
            {
                if (audience == null || audience.CharacterAnimator == null)
                    continue;

                var anim = audience.CharacterAnimator;

                // Scale their jump height
                anim.SetJumpIntensity01(intensity);

                // They only actually “jump” when hype passes a threshold
                anim.JumpOnBeat = (intensity >= threshold);

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
            songHypeDebugSlider.maxValue = meters != null ? meters.MaxSongHype : 0f;
            songHypeDebugSlider.wholeNumbers = false;

            // Start synced to current hype
            songHypeDebugSlider.SetValueWithoutNotify(_songHype);

            // Avoid double-registering
            songHypeDebugSlider.onValueChanged.RemoveListener(OnDebugSongHypeSliderChanged);
            songHypeDebugSlider.onValueChanged.AddListener(OnDebugSongHypeSliderChanged);

            // Only visible when debug mode is ON
            songHypeDebugSlider.gameObject.SetActive(dev != null && dev.DebugSongHype);
        }

        private void OnDebugSongHypeSliderChanged(float value)
        {
            // Only override the game when debug mode is enabled
            if (dev == null || !dev.DebugSongHype) return;

            // [S3 D-S3-8=A] Route through the absolute Dev setter so dragging the
            // slider fires venue VFX crossings (parity with the ±10% buttons).
            DevSetSongHypeAbsolute(value);
        }

        private void SetupBandDebugElements()
        {
            if (CurrentMusicianCharacterList == null ||
                CurrentMusicianCharacterList.Count == 0)
                return;

            // Refresh repo once if we’re going to use it
            if ((dev != null && dev.DebugInstrumentPicker) && _instrumentRepo != null)
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
                if ((dev != null && dev.DebugInstrumentPicker) && profile != null && _instrumentRepo != null)
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

                                if (UseLogs)
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

                                if (UseLogs)
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
                if (dev != null && dev.DebugMusicianVolume)
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
            if (dev == null || !dev.DebugInstrumentPicker) return;
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
            float global = Mathf.Clamp01(_globalMusicVolume01);   // seeded from SO at gig start (§ApplyPersistedAudioMix)

            float instrument = 1f; // TODO: Get instrumentSO volume

            float final = global * musicianVolume01 * instrument;
            return Mathf.Clamp01(final);
        }

        /// <summary>
        /// M-AUDIO-MIX (D-VOL=B): load the persisted balance at gig start. Seeds the
        /// per-musician dict from AudioMixSettingsSO and pushes master SFX to AudioManager.
        /// Global is applied implicitly via ComputeEffectiveMusicianVolume01 (reads the SO).
        /// Per-musician channel CC is deferred to ReapplyMusicianMix at song start (channels
        /// are not owned yet at gig start).
        /// </summary>
        private void ApplyPersistedAudioMix()
        {
            if (audioMix != null)
            {
                _globalMusicVolume01 = audioMix.GlobalMusicVolume01;

                foreach (var m in CurrentMusicianCharacterList)
                {
                    if (m == null) continue;
                    _musicianVolume01[m.CharacterId] = audioMix.GetMusicianVolume01(m.CharacterId);
                }

                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetSfxVolume01(audioMix.MasterSfxVolume01);

                if (UseLogs) Debug.Log(
                    $"{DebugTag} [AudioMix] Loaded balance: global={_globalMusicVolume01:0.00} " +
                    $"sfx={audioMix.MasterSfxVolume01:0.00} musicians={_musicianVolume01.Count}");
            }
            else if (UseLogs)
            {
                Debug.LogWarning(
                    $"{DebugTag} [AudioMix] No AudioMixSettingsSO wired; live mix works but will NOT " +
                    $"persist. global={_globalMusicVolume01:0.00} (default).");
            }
        }

        /// <summary>
        /// M-AUDIO-MIX: re-applies the current per-musician balance to the live channels.
        /// Called right after Play(song) once SetChannelOwners has run, so the
        /// persisted/dev balance lands on each song (the per-musician axis is not otherwise
        /// re-applied across song transitions).
        /// </summary>
        private void ReapplyMusicianMix()
        {
            if (MidiMusicManager == null) return;
            foreach (var m in CurrentMusicianCharacterList)
            {
                if (m == null) continue;
                float stored = _musicianVolume01.TryGetValue(m.CharacterId, out var v) ? v : 1f;
                MidiMusicManager.SetMusicianVolume01(
                    m.CharacterId, ComputeEffectiveMusicianVolume01(m, stored));
            }
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

            if (UseLogs)
                Debug.Log($"{DebugTag} [Dev] Volume slider for {musician.CharacterId} " +
                    $"slider={sliderValue:0.00} final={finalVol:0.00}");
        }

        private IEnumerator RunSongVibeResolution(SongFeedbackContext songCtx)
        {
            if (songCtx.PartCount == 0)
                yield break;

            float songEndPauseLocal = presentation != null ? presentation.SongEndPause : 3f;
            float barFillDelayLocal = presentation != null ? presentation.BarFillDelay : 3f;
            float perAudVibeDelayLocal = presentation != null ? presentation.PerAudienceVibeDelay : 1f;
            float flowMult = meters != null ? meters.FlowVibeMultiplier : 0f;

            yield return new WaitForSeconds(songEndPauseLocal);

            var deltas = ComputeSongVibeDeltas(songCtx);

            // [S5a/T3] Capture the banked SFX once; pay it flat per member, added AFTER
            // the Flow multiply (D-S5-SFX-SCALE=A: SFX bypasses Flow).
            int sfx = _pendingSfxVibe;

            if (UseLogs)
                Debug.Log($"{DebugTag} [S5a-SMOKE] SONG-END begin: bankedSFX={sfx} " +
                    $"flowStacks={GetTotalFlowStacks()} SongHype01={SongHype01:F3} members={deltas.Count}");

            foreach (var entry in deltas)
            {
                var audience = CurrentAudienceCharacterList[entry.AudienceIndex];
                if (audience == null) continue;

                // L-part (SongHype x impressionFactor), pre-Flow, from ComputeSongVibeDeltas.
                int lPart = entry.Delta;

                // Flow amplifies the L-part only; SFX is flat.
                int flowStacks = lPart > 0 ? GetTotalFlowStacks() : 0;
                int lAfterFlow = ApplyFlowToLPart(lPart);

                // [S5a/T3] Single combined delta = Flow(L) + flat SFX (D1=A: one cyan number).
                int finalDelta = lAfterFlow + sfx;

                // [S5a/T4] Zero-total suppression (was inside ComputeSongVibeDeltas).
                // Preserves event<->FT parity: no event/FT when nothing lands.
                if (finalDelta <= 0)
                {
                    if (UseLogs)
                        Debug.Log($"{DebugTag} [S5a-SMOKE] SONG-END   i={entry.AudienceIndex} " +
                            $"'{audience.CharacterId}' L={lPart} +SFX{sfx} = 0 (no Vibe, skipped)");
                    continue;
                }

                // [B3] Canonical ApplyIncomingVibe path. Indifference gates the WHOLE
                // combined delta to 0 (single gate point; L and SFX blocked together).
                int appliedDelta = audience.AudienceStats.ApplyIncomingVibe(
                    audience.Statuses, finalDelta, duration: barFillDelayLocal);

                // FT (D1=A): cyan "+{applied} Vibe" - applied total already includes SFX.
                // The "(Flow xM)" suffix (flowStacks>0) describes the L-part's boost; it
                // slightly over-implies for the SFX portion - accepted for the demo (the
                // readout/telegraph carry the precise L vs SFX split).
                float displayMult = flowStacks > 0 ? 1f + (flowStacks * flowMult) : 0f;
                var vibeEvt = new SongEndVibeEvent(
                    audience, entry.AudienceIndex, audience.CharacterId,
                    lPart, finalDelta, appliedDelta,
                    flowStacks,
                    flowStacks > 0 ? displayMult : 1f,
                    blockedByIndifference: appliedDelta <= 0 && finalDelta > 0);

                SensoryEventBus.Instance?.Publish(vibeEvt);

                if (UseLogs)
                    Debug.Log($"{DebugTag} [S5a-SMOKE] SONG-END   i={entry.AudienceIndex} " +
                        $"'{audience.CharacterId}' L={lPart} " +
                        $"flowx{(flowStacks > 0 ? displayMult : 1f):F2}->{lAfterFlow} " +
                        $"+SFX{sfx} intended={finalDelta} applied={appliedDelta} " +
                        $"indiff={(appliedDelta <= 0 && finalDelta > 0)}");

                yield return new WaitForSeconds(barFillDelayLocal);

                // TODO: Animate Vibe bar, emote, SFX, etc

                yield return new WaitForSeconds(perAudVibeDelayLocal);
            }
        }

        // [S5a/T6] Live running average impression for one audience member across the
        // current song: closed parts (_gigPartsForCurrentSong) + the open part
        // (_audienceLoopImpressionsByPart). Mirrors the song-end aggregation in
        // ComputeSongVibeDeltas but reads live state, so the telegraph converges to the
        // song-end delta by the last loop boundary. Neutral (0) when no samples yet.
        public float GetLiveAvgImpression(int audienceIndex)
        {
            float sum = 0f;
            int count = 0;

            foreach (var part in _gigPartsForCurrentSong)
            {
                if (part.AudienceLoopImpressions == null) continue;
                if (part.AudienceLoopImpressions.TryGetValue(audienceIndex, out var list)
                    && list != null)
                {
                    foreach (var v in list) { sum += v; count++; }
                }
            }

            foreach (var kv in _audienceLoopImpressionsByPart)
            {
                if (kv.Value != null
                    && kv.Value.TryGetValue(audienceIndex, out var list)
                    && list != null)
                {
                    foreach (var v in list) { sum += v; count++; }
                }
            }

            return count > 0 ? sum / count : 0f;
        }

        // [S5a/T3+T11] Flow amplifies the L-part only (SFX stays flat). Shared by the
        // song-end payout and the C3 projection so the two cannot drift.
        private int ApplyFlowToLPart(int lPart)
        {
            if (lPart <= 0) return lPart;
            int flowStacks = GetTotalFlowStacks();
            if (flowStacks <= 0) return lPart;
            float flowMult = meters != null ? meters.FlowVibeMultiplier : 0f;
            return Mathf.RoundToInt(lPart * (1f + flowStacks * flowMult));
        }

        // [S5a/T8+T11] Loop-boundary projection refresh: C1 global "L + SFX" readout +
        // per-enemy C2 effectiveness / C3 projected number. Called at each loop
        // resolution, at SFX stage crossings, and at song start. Not per-frame.
        private void RefreshVibeProjection(string reason)
        {
            int maxVibeFromHype = meters != null ? meters.MaxVibeFromSongHype : 0;
            float baseVibe = SongHype01 * maxVibeFromHype;   // volatile L (band-wide)
            int lPart = Mathf.RoundToInt(baseVibe);
            int sfxPart = _pendingSfxVibe;                   // banked SFX (monotonic)

            if (UIManager != null && UIManager.GigCanvas != null)
                UIManager.GigCanvas.SetVibeReadout(lPart, sfxPart);

            if (UseLogs)
                Debug.Log($"{DebugTag} [S5a-SMOKE] PROJ ({reason}) readout L={lPart} " +
                    $"SFX={sfxPart} N={lPart + sfxPart} SongHype01={SongHype01:F3}");

            if (CurrentAudienceCharacterList == null) return;

            for (int i = 0; i < CurrentAudienceCharacterList.Count; i++)
            {
                var aud = CurrentAudienceCharacterList[i];
                if (aud == null || aud.AudienceCharacterCanvas == null) continue;

                bool indifferent = aud.Statuses != null
                    && aud.Statuses.GetStacks(CharacterStatusId.NegateIncomingPositive) > 0;

                float avg = Mathf.Clamp(GetLiveAvgImpression(i), -2f, 2f);

                VibeEffectiveness tier;
                int projected = 0;

                if (aud.IsBlocked || indifferent)
                {
                    tier = VibeEffectiveness.Immune; // both gates -> no Vibe lands at song end
                }
                else
                {
                    float factor = 1f + (avg * 0.25f);
                    int lDelta = Mathf.RoundToInt(baseVibe * factor);
                    projected = ApplyFlowToLPart(lDelta) + sfxPart; // mirrors song-end math

                    tier = avg > 0.0001f ? VibeEffectiveness.SuperEffective
                         : avg < -0.0001f ? VibeEffectiveness.NotVeryEffective
                         : VibeEffectiveness.Normal;
                }

                aud.AudienceCharacterCanvas.SetVibeTelegraph(tier, projected, showNumber: true);

                if (UseLogs)
                    Debug.Log($"{DebugTag} [S5a-SMOKE] PROJ ({reason})   i={i} " +
                        $"'{aud.CharacterId}' tier={tier} projected={projected} " +
                        $"avg={avg:F2} blocked={aud.IsBlocked} indiff={indifferent}");
            }
        }

        // [S5a] Hide the per-enemy telegraph between songs / during the audience turn.
        private void HideVibeProjection()
        {
            if (CurrentAudienceCharacterList == null) return;
            foreach (var aud in CurrentAudienceCharacterList)
                if (aud != null && aud.AudienceCharacterCanvas != null)
                    aud.AudienceCharacterCanvas.HideVibeTelegraph();
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

            // [S4 D-S4-SRC=A] Normal-flow outcome (debug WinGig/LoseGig menus bypass this).
            SensoryEventBus.Instance?.Publish(new GigOutcomeEvent(win));

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
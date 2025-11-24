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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace ALWTTT.Managers
{
    public class GigManager : MonoBehaviour
    {
        public const string DebugTag = "<color=magenta>GigManager:</color>";

        public static GigManager Instance;

        [Header("Composition Rules")]
        [SerializeField] private JamRules jamRules = new JamRules();
        [SerializeField] private float maxSongHype = 100f;

        [Header("Cards / Hand")]
        [SerializeField] private HandController gigHand;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera handCamera;

        [Header("Vibe / Hype Balancing")]
        [SerializeField] private int maxVibeFromSongHype = 20;

        [Header("Composition UI")]
        [SerializeField] private SongCompositionUI compositionUI;
        [SerializeField] private LoopsTimerUI loopsTimerUI;
        [SerializeField] private MidiGenPlayConfig midiGenPlayConfig;

        [Header("References")]
        [SerializeField] private BackgroundContainer backgroundContainer;
        [SerializeField] private List<Transform> musicianPosList;
        [SerializeField] private List<Transform> audienceMemberPosList;
        [SerializeField] private SceneChanger sceneChanger;

        [Header("Composition Dev")]
        [SerializeField] private bool useLogs = true;
        [SerializeField] private bool useCompositionLogs = true;

        private GigPhase currentGigPhase;
        private List<SongData> playedSongs = new List<SongData>();

        private readonly List<MusicianBase> _spawned = new();

        private System.Random _rng = new System.Random();

        private bool _isSongPlaying;
        private bool _isBetweenSongs;

        // PartIndex -> (AudienceIndex -> List of impressions per loop)
        private readonly Dictionary<int, Dictionary<int, List<int>>>
            _audienceLoopImpressionsByPart = new();

        // Enriched part feedback (with audience data) for the current song
        private readonly List<PartFeedbackContext> _gigPartsForCurrentSong =
            new List<PartFeedbackContext>();

        // Gig-level events that expose *enriched* contexts
        public event Action<PartFeedbackContext> OnGigPartFeedbackReady;
        public event Action<SongFeedbackContext> OnGigSongFeedbackReady;

        private float _songHype;
        public float SongHype => _songHype;
        public float SongHype01 => 
            maxSongHype <= 0f ? 0f : Mathf.Clamp01(_songHype / maxSongHype);


        #region Cache
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
        #endregion

        #region Callbacks

        public Action OnPlayerTurnStarted;
        public Action OnSongPerformanceStarted;
        public Action OnEnemyTurnStarted;

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
        }

        private void Start()
        {
            //MidiMusicManager.GenerateSongs(GameManager.PersistentGameplayData.CurrentSongList);
            StartGig();
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
            BuildBackground();
            BuildBand();
            BuildAudience();

            // For now, ignore the dedicated gig deck / battle design
            //DeckManager.SetGameDeck();

            UIManager.GigCanvas.gameObject.SetActive(true);

            // Bind deck to gig hand for composition
            RebindDeckToGigHand();

            // IMPORTANT: Do NOT set the gig phase for now
            CurrentGigPhase = GigPhase.PlayerTurn;

            //_isSongPlaying = false;
            //_isBetweenSongs = _session != null;
        }

        private void SetupEncounter()
        {
            if (useLogs) Debug.Log($"{DebugTag} Setting up gig encounter...");

            var pd = GameManager.PersistentGameplayData;

            CurrentGigEncounter = GameManager.EncounterData
                .GetGigEncounterByIndex(
                    pd.CurrentSectorId, pd.CurrentEncounterId, pd.IsFinalEncounter);

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
                    MusicianPosList.Count >= i ?
                        MusicianPosList[i] :
                        MusicianPosList[0]
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

                CurrentMusicianCharacterList.Add(clone);
            }
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
        }

        private void OnPlayPressed()
        {
            _session?.ConfirmCurrentPartAndStart();
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

                    if (GameManager.PersistentGameplayData.CurrentSongIndex >=
                        GameManager.PersistentGameplayData.CurrentEncounter.NumberOfSongs)
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

                    OnPlayerTurnStarted?.Invoke();
                    GameManager.PersistentGameplayData.SongModifierCardsList.Clear();

                    // --- Groove + Draw Logic ---
                    GameManager.PersistentGameplayData.CurrentGroove =
                        GameManager.PersistentGameplayData.TurnStartingGroove;
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

            if (CurrentGigPhase != GigPhase.EndGig)
            {
                CurrentGigPhase = GigPhase.AudienceTurn;
            }
        }

        private IEnumerator AudienceTurnRoutine()
        {
            var waitDelay = new WaitForSeconds(1f);

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
                    nameof(AudienceCharacterSimple.ActionRoutine));

                yield return waitDelay;
            }

            CurrentAudienceCharacterList.Sort((a, b) => 
                a.ColumnIndex.CompareTo(b.ColumnIndex));

            if (CurrentGigPhase != GigPhase.EndGig)
            {
                CurrentGigPhase = GigPhase.PlayerTurn;
            }
        }

        private void LoseGig()
        {
            var pd = GameManager.PersistentGameplayData;
            UIManager.GigCanvas.OnLossConfirm = () => ReturnToMap(false);
            UIManager.GigCanvas.ShowLoss(
                title: "Gig Lost",
                body: "You didn’t convince the crowd this time, but the journey continues.\n" +
                       $"Cohesion decreased by {pd.CurrentEncounter.CohesionPenaltyOnLoss}."
            );

            foreach (var m in _spawned)
            {
                if (m != null) m.UnbindFromGigContext();
            }
        }

        private void WinGig()
        {
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
            var pd = GameManager.PersistentGameplayData;
            var state = pd.CurrentSectorMapState;
            if (state != null)
            {
                var node = state.GetNode(state.CurrentNodeId);
                if (node != null && node.Type == Enums.NodeType.Gig)
                {
                    node.Completed = true; // cannot replay regular gigs
                }
            }

            if (won)
            {
                // Reward fans (or use CurrentGigEncounter.FansReward if present)
                pd.Fans += pd.CurrentEncounter.FansOnWin;
            }
            else
            {
                // Reduce cohesion (clamp >= 0)
                pd.BandCohesion = Mathf.Max(
                    0, pd.BandCohesion - pd.CurrentEncounter.CohesionPenaltyOnLoss);
            }

            // Clear encounter pointer
            pd.CurrentEncounter = null;

            // Back to Map
            if (sceneChanger) sceneChanger.OpenMapScene();
            else UnityEngine.SceneManagement.SceneManager.LoadScene("Map"); // fallback
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

            return _session?.TryPlayCompositionCard(card, target, zone) ?? false;
        }

        private void StartCompositionSession()
        {
            if (_session != null)
            {
                _session.LoopFinished -= OnCompositionLoopFinished;
                _session.End();
            }

            _songHype = 0f;
            UpdateSongHypeUI();

            _session = new CompositionSession();
            _session.LoopFinished += OnCompositionLoopFinished;
            _session.PartFinished += OnCompositionPartFinished;
            _session.SongFinished += OnCompositionSongFinished;

            var ctx = new GigContext(this);
            _session.Begin(ctx, jamRules, midiGenPlayConfig, _rng);

            _isSongPlaying = false;
            _isBetweenSongs = true;
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

        private void OnCompositionSongFinished(SongFeedbackContext songCtx)
        {
            Log($"{DebugTag} [Gig] Song finished → {songCtx}", customColor: "yellow");

            // Build an enriched SongFeedbackContext using the gig's part list
            var enrichedSong = new SongFeedbackContext(_gigPartsForCurrentSong);

            // Macro reaction – final SongHype +aggregated impressions → Vibe
            ApplySongHypeToAudience(enrichedSong);

            // Notify gig-level listeners
            OnGigSongFeedbackReady?.Invoke(enrichedSong);

            // Clear per-song state for the next song
            _gigPartsForCurrentSong.Clear();
            _audienceLoopImpressionsByPart.Clear();
        }

        public bool CanPlayActionCard(CardData card)
        {
            if (!card.IsAction) return false;

            if (_isSongPlaying)
                return card.actionTiming == CardData.ActionTiming.Always;

            if (_isBetweenSongs)
                return true; // both Always and BetweenSongsOnly

            // No session / no gig context → no action cards
            return false;
        }

        // Called whenever one full loop finishes (including the last loop of the song)
        private void TriggerAudienceMicroReactions(LoopFeedbackContext loopCtx)
        {
            if (CurrentAudienceCharacterList == null 
                || CurrentAudienceCharacterList.Count == 0)
                return;

            // Compute loop score and SongHype delta using the pure calculator
            float loopScore = LoopScoreCalculator.ComputeLoopScore(loopCtx);
            float hypeDelta = LoopScoreCalculator.ComputeHypeDelta(loopScore);

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

            // Hand control to the existing gig phase system:
            // this will call AudienceTurnRoutine() and run all audience actions.
            CurrentGigPhase = GigPhase.AudienceTurn;
        }

        // TODO: Call aftear each LoopScore is obtained
        private void AddSongHype(float delta)
        {
            _songHype = Mathf.Clamp(_songHype + delta, 0f, maxSongHype);
            UpdateSongHypeUI();
        }

        private void UpdateSongHypeUI()
        {
            var gigCanvas = UIManager != null ? UIManager.GigCanvas : null;
            if (gigCanvas == null) return;

            gigCanvas.SetSongHype(SongHype01);   // 0–1 normalized
        }

        private void ApplySongHypeToAudience(SongFeedbackContext enrichedSong)
        {
            if (CurrentAudienceCharacterList == null ||
                CurrentAudienceCharacterList.Count == 0)
                return;

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

            // 3) Apply per-audience modifiers and actually add Vibe
            for (int i = 0; i < audienceCount; i++)
            {
                var audience = CurrentAudienceCharacterList[i];
                if (audience == null) continue;
                // blocked chars get no vibe
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

                audience.AudienceStats.AddVibe(vibeDelta);

                Log($"{DebugTag} [Gig] Final SongHype → Audience {audience.CharacterId} " +
                    $"avgImpression={avgImpression:F2}, ΔVibe={vibeDelta}",
                    customColor: "green");
            }
        }
    }
}


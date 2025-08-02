using ALWTTT.Backgrounds;
using ALWTTT.Enums;
using ALWTTT.Encounters;
using System;
using UnityEngine;
using System.Collections.Generic;
using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;

namespace ALWTTT.Managers
{
    public class GigManager : MonoBehaviour
    {
        private const string DebugTag = "<color=cyan>GigManager:</color>";

        public static GigManager Instance;

        public bool debug = true;

        [Header("References")]
        [SerializeField] private BackgroundContainer backgroundContainer;
        [SerializeField] private List<Transform> musicianPosList;
        [SerializeField] private List<Transform> audienceMemberPosList;

        private GigPhase currentGigPhase;

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

        public List<Transform> MusicianPosList => musicianPosList;
        public List<Transform> AudienceMemberPosList => audienceMemberPosList;

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
            StartGig();
        }

        private void StartGig()
        {
            if (debug) Debug.Log($"{DebugTag} Starting gig...");

            SetupEncounter();
            BuildBackground();
            BuildBand();
            BuildAudience();

            DeckManager.SetGameDeck();

            UIManager.GigCanvas.gameObject.SetActive(true);

            CurrentGigPhase = GigPhase.PlayerTurn;
        }

        private void SetupEncounter()
        {
            if (debug) Debug.Log($"{DebugTag} Setting up gig encounter...");

            CurrentGigEncounter = GameManager.EncounterData.GetGigEncounter(
                GameManager.PersistentGameplayData.CurrentSectorId,
                GameManager.PersistentGameplayData.CurrentEncounterId,
                GameManager.PersistentGameplayData.IsFinalEncounter
            );
        }

        private void BuildBackground()
        {
            if (debug) Debug.Log($"{DebugTag} Building background...");
            backgroundContainer.OpenSelectedBackground();
            backgroundContainer.SetBPM(60); // TODO: OnSongPerfomance
        }

        private void BuildBand()
        {
            if (debug) Debug.Log($"{DebugTag} Building band and musicians...");
            for (var i = 0; 
                i < GameManager.PersistentGameplayData.MusicianList.Count; i++)
            {
                var clone = Instantiate(
                    GameManager.PersistentGameplayData.MusicianList[i],
                    MusicianPosList.Count >= i ?
                        MusicianPosList[i] :
                        MusicianPosList[0]
                );

                clone.BuildCharacter();
                CurrentMusicianCharacterList.Add(clone);
            }
        }

        private void BuildAudience()
        {
            if (debug) Debug.Log($"{DebugTag} Building audience...");
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
                CurrentAudienceCharacterList.Add(clone);
            }
        }

        private void ExecuteGigPhase(GigPhase targetGigPhase)
        {
            if (debug) 
                Debug.Log($"{DebugTag} Executing gig phase: {targetGigPhase}");

            switch (targetGigPhase)
            {
                case GigPhase.PrepareGig:
                    break;
                case GigPhase.PlayerTurn:

                    OnPlayerTurnStarted?.Invoke();

                    // TODO: Stunned

                    // Initial groove per turn
                    GameManager.PersistentGameplayData.CurrentGroove =
                        GameManager.PersistentGameplayData.TurnStartingGroove;
                    // TODO: Special case for first turn

                    DeckManager.DrawCards(GameManager.PersistentGameplayData.DrawCount);


                    break;
                case GigPhase.SongPerformance:
                    break;
                case GigPhase.AudienceTurn:
                    break;
            }
        }

        public void EndTurn()
        {
            if (debug) Debug.Log($"{DebugTag} Ending turn...");

            currentGigPhase = GigPhase.SongPerformance;
        }
    }
}


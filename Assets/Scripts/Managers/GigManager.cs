using ALWTTT.Enums;
using System;
using UnityEngine;

namespace ALWTTT.Managers
{
    public class GigManager : MonoBehaviour
    {
        private const string DebugTag = "<color=cyan>GigManager:</color>";

        public static GigManager Instance;

        private GigPhase currentGigPhase;

        #region Cache
        public EnemyEncounter CurrentEncounter { get; private set; }

        private GameManager GameManager => GameManager.Instance;
        private DeckManager DeckManager => DeckManager.Instance;
        private UIManager UIManager => UIManager.Instance;

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
            Debug.Log($"{DebugTag} Starting gig...");

            BuildEnemies();
            BuildAllies();

            DeckManager.SetGameDeck();

            UIManager.GigCanvas.gameObject.SetActive(true);

            CurrentGigPhase = GigPhase.PlayerTurn;
        }

        private void BuildEnemies()
        {

        }

        private void BuildAllies()
        {

        }

        private void ExecuteGigPhase(GigPhase targetGigPhase)
        {
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
            Debug.Log($"{DebugTag} Ending turn...");

            currentGigPhase = GigPhase.SongPerformance;
        }
    }
}


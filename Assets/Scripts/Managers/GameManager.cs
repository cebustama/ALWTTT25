using ALWTTT.Extentions;
using ALWTTT.Data;
using UnityEngine;
using ALWTTT.Cards;

namespace ALWTTT.Managers
{
    public class GameManager : MonoBehaviour
    {
        private const string DebugTag = "<color=white>GameManager:</color>";

        public static GameManager Instance;

        [Header("Settings")]
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private EncounterData encounterData;

        public GameplayData GameplayData => gameplayData;
        public EncounterData EncounterData => encounterData;
        public PersistentGameplayData PersistentGameplayData { get; private set; }
        
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                // Move to root of hierarchy
                transform.parent = null;

                Instance = this;
                DontDestroyOnLoad(gameObject);

                CardActionProcessor.Initialize();
                // EnemyActionProcessor.Initialize();

                InitGameplayData();
                SetInitialDeck();
            }
        }

        #region Public Methods
        public void SetInitialDeck()
        {
            Debug.Log($"{DebugTag} Setting initial deck...");
            PersistentGameplayData.CurrentCardsList.Clear();

            // Randomized starting deck
            if (PersistentGameplayData.IsRandomDeck)
            {
                for (int i = 0; i < GameplayData.RandomCardCount; i++)
                {
                    PersistentGameplayData.CurrentCardsList.Add(
                        GameplayData.AllCardsList.RandomItem()
                    );
                }
            }
            // Add from Deck Data
            else
            {
                foreach (var cardData in GameplayData.InitialDeck.CardList)
                {
                    PersistentGameplayData.CurrentCardsList.Add(cardData);
                }
            }
        }

        public CardBase BuildAndGetCard(CardData targetData, Transform parent)
        {
            var newCard = Instantiate(GameplayData.CardPrefab, parent);
            newCard.SetCard(targetData);
            return newCard;
        }
        #endregion

        #region Private Methods
        private void InitGameplayData()
        {
            PersistentGameplayData = new PersistentGameplayData(gameplayData);
            
            // TODO: UIManager.InformationCanvas.ResetCanvas();
        }
        #endregion
    }
}
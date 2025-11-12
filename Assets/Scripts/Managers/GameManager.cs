using ALWTTT.Extentions;
using ALWTTT.Data;
using UnityEngine;
using ALWTTT.Actions;


namespace ALWTTT.Managers
{
    public class GameManager : MonoBehaviour
    {
        private const string DebugTag = "<color=white>GameManager:</color>";

        public static GameManager Instance;

        [Header("Settings")]
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private EncounterData encounterData;
        [SerializeField] private SceneData sceneData;

        #region Encapsultaion
        public GameplayData GameplayData => gameplayData;
        public EncounterData EncounterData => encounterData;
        public SceneData SceneData => sceneData;
        public PersistentGameplayData PersistentGameplayData { get; private set; }
        #endregion

        #region Cache
        public UIManager UIManager => UIManager.Instance;
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
                // Move to root of hierarchy
                transform.parent = null;

                Instance = this;
                DontDestroyOnLoad(gameObject);

                CharacterActionProcessor.Initialize();

                InitGameplayData();
                SetInitialDeck();
            }
        }

        #region Public Methods
        public void InitGameplayData()
        {
            PersistentGameplayData = new PersistentGameplayData(gameplayData);
            // TODO: UIManager.InformationCanvas.ResetCanvas();
        }

        public void SetInitialDeck()
        {
            Debug.Log($"{DebugTag} Setting initial deck...");
            PersistentGameplayData.CurrentActionCards.Clear();

            // Randomized starting deck
            if (PersistentGameplayData.IsRandomDeck)
            {
                for (int i = 0; i < GameplayData.RandomCardCount; i++)
                {
                    PersistentGameplayData.CurrentActionCards.Add(
                        GameplayData.AllCardsList.RandomItem()
                    );
                }
            }
            // Add from Deck Data
            else
            {
                foreach (var cardData in GameplayData.InitialActionDeck.CardList)
                {
                    PersistentGameplayData.CurrentActionCards.Add(cardData);
                }
            }
        }

        public CardBase BuildAndGetCard(CardData targetData, Transform parent)
        {
            var newCard = Instantiate(GameplayData.CardPrefab, parent);
            newCard.SetCard(targetData);
            return newCard;
        }

        public void OnExitApp()
        {

        }
        #endregion
    }
}
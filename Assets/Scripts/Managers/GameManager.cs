using ALWTTT.Extentions;
using ALWTTT.Data;
using UnityEngine;
using ALWTTT.Actions;
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

            // Move to root of hierarchy
            transform.parent = null;

            Instance = this;
            DontDestroyOnLoad(gameObject);

            CharacterActionProcessor.Initialize();

            InitGameplayData();
            SetInitialDeck();
        }

        #region Public Methods
        public void InitGameplayData()
        {
            PersistentGameplayData = new PersistentGameplayData(gameplayData);
            // TODO: UIManager.InformationCanvas.ResetCanvas();
        }

        /// <summary>
        /// Populate PersistentGameplayData.CurrentActionCards + CurrentCompositionCards
        /// from GameplayData initial decks (or randomized deck, if enabled).
        /// </summary>
        public void SetInitialDeck()
        {
            Debug.Log($"{DebugTag} Setting initial deck...");

            var pd = PersistentGameplayData;
            if (pd == null)
            {
                Debug.LogError($"{DebugTag} PersistentGameplayData is null; cannot set initial deck.");
                return;
            }

            pd.CurrentActionCards ??= new System.Collections.Generic.List<CardDefinition>();
            pd.CurrentCompositionCards ??= new System.Collections.Generic.List<CardDefinition>();

            pd.CurrentActionCards.Clear();
            pd.CurrentCompositionCards.Clear();

            // Randomized starting deck (mixed domains)
            if (pd.IsRandomDeck)
            {
                for (int i = 0; i < GameplayData.RandomCardCount; i++)
                {
                    var c = GameplayData.AllCardsList.RandomItem();
                    if (c == null) continue;

                    if (c.IsAction) pd.CurrentActionCards.Add(c);
                    else if (c.IsComposition) pd.CurrentCompositionCards.Add(c);
                }
            }
            // Add from Deck Data assets
            else
            {
                if (GameplayData.InitialActionDeck != null)
                {
                    foreach (var cardData in GameplayData.InitialActionDeck.CardList)
                        if (cardData != null) pd.CurrentActionCards.Add(cardData);
                }

                if (GameplayData.InitialCompositionDeck != null)
                {
                    foreach (var cardData in GameplayData.InitialCompositionDeck.CardList)
                        if (cardData != null) pd.CurrentCompositionCards.Add(cardData);
                }
            }

            Debug.Log($"{DebugTag} Initial deck resolved: Action={pd.CurrentActionCards.Count}, Composition={pd.CurrentCompositionCards.Count}");
        }

        public CardBase BuildAndGetCard(CardDefinition targetData, Transform parent)
        {
            Debug.Log($"{DebugTag} Building Card {targetData.DisplayName} with parent {parent.name}");
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

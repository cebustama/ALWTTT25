using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Managers
{
    public class DeckManager : MonoBehaviour
    {
        private const string DebugTag = "<color=magenta>DeckManager:</color>";

        public static DeckManager Instance;

        [Header("Controllers")]
        [SerializeField] private HandController handController;

        public List<CardData> DrawPile { get; private set; } = new List<CardData>();
        public List<CardData> HandPile { get; private set; } = new List<CardData>();
        public List<CardData> DiscardPile { get; private set; } = new List<CardData>();
        public List<CardData> ExhaustPile { get; private set; } = new List<CardData>();

        public HandController HandController => handController;

        private GameManager GameManager => GameManager.Instance;
        private UIManager UIManager => UIManager.Instance;

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
            }
        }

        public void SetGameDeck()
        {
            Debug.Log($"{DebugTag} Setting game deck...");
            foreach (var i in GameManager.PersistentGameplayData.CurrentCardsList)
            {
                DrawPile.Add(i);
            }
        }

        public void DrawCards(int targetDrawCount)
        {
            Debug.Log($"{DebugTag} Drawing {targetDrawCount} cards...");

            int currentDrawCount = 0;

            for (int i = 0; i < targetDrawCount; i++)
            {
                // Cards on hand limit
                if (GameManager.GameplayData.MaxCardsOnHand <= HandPile.Count)
                {
                    Debug.Log($"{DebugTag} Max number of cards on hand reached.");
                    return;
                }

                // Reshuffle if no more cards in Draw Pile
                if (DrawPile.Count <= 0)
                {
                    var nDrawCount = targetDrawCount - currentDrawCount;

                    // If not enough cards left in deck, draw max possible
                    if (nDrawCount >= DiscardPile.Count)
                    {
                        nDrawCount = DiscardPile.Count;
                    }

                    ReshuffleDiscardPile();
                    DrawCards(nDrawCount);
                    break;
                }

                var randomCard = DrawPile[Random.Range(0, DrawPile.Count)];

                // Instantiate card as child of HandController.DrawTransform
                var card = GameManager.BuildAndGetCard(
                    randomCard, HandController.DrawTransform);

                HandController.AddCardToHand(card);
                HandPile.Add(randomCard);
                DrawPile.Add(randomCard);
                currentDrawCount++;

                UIManager.GigCanvas.SetPileTexts();
            }

            // TODO: Update card texts based on status effects, etc
            foreach (var cardObject in HandController.Hand)
            {
                // cardObject.UpdateCardText();
            }
        }

        public void ClearPiles()
        {
            DiscardPile.Clear();
            DrawPile.Clear();
            HandPile.Clear();
            ExhaustPile.Clear();
            HandController.Hand.Clear();
        }

        private void ReshuffleDiscardPile()
        {
            Debug.Log($"{DebugTag} Reshuffling discard pile...");
            Debug.LogError("IMPLEMENT");
        }
    }
}
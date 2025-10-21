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

        #region Cache
        public HandController HandController => handController;
        private GameManager GameManager => GameManager.Instance;
        private UIManager UIManager => UIManager.Instance;
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
                DrawPile.Remove(randomCard);
                currentDrawCount++;

                if (UIManager != null && UIManager.GigCanvas != null)
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
            DiscardHand();
            DiscardPile.Clear();
            DrawPile.Clear();
            HandPile.Clear();
            ExhaustPile.Clear();
            HandController.Hand.Clear();
        }

        public void OnCardPlayed(CardBase targetCard)
        {
            Debug.Log($"{DebugTag} On Card Played...");

            if (targetCard.CardData.ExhaustAfterPlay)
            {
                targetCard.Exhaust();
            }
            else
            {
                targetCard.Discard();
            }
                
            // TODO: UpdateCardText for all other cards
            // Ex. when +STR increase DMG for all cards
        }

        public void DiscardHand()
        {
            foreach (var cardBase in HandController.Hand)
            {
                cardBase.Discard();
            }

            HandController.Hand.Clear();
        }

        public void OnCardDiscarded(CardBase targetCard)
        {
            HandPile.Remove(targetCard.CardData);
            DiscardPile.Add(targetCard.CardData);

            if (UIManager != null && UIManager.GigCanvas != null)
                UIManager.GigCanvas.SetPileTexts();
        }

        public void ClearAll()
        {
            DiscardHand();
            DiscardPile.Clear();
            DrawPile.Clear();
            HandPile.Clear();
            ExhaustPile.Clear();

            if (UIManager != null && UIManager.GigCanvas != null)
                UIManager.GigCanvas.SetPileTexts();
        }

        public void AddToDrawPile(IEnumerable<CardData> cards, bool shuffle = true)
        {
            if (cards == null) return;

            foreach (var c in cards)
                if (c != null) DrawPile.Add(c);

            if (shuffle && DrawPile.Count > 1)
            {
                // random swap shuffle
                for (int i = 0; i < DrawPile.Count; i++)
                {
                    int j = Random.Range(0, DrawPile.Count);
                    (DrawPile[i], DrawPile[j]) = (DrawPile[j], DrawPile[i]);
                }
            }

            if (UIManager != null && UIManager.GigCanvas != null)
                UIManager.GigCanvas.SetPileTexts();
        }

        public void AddToDrawPile(CardData card)
        {
            if (card != null) DrawPile.Add(card);

            if (UIManager != null && UIManager.GigCanvas != null)
                UIManager.GigCanvas.SetPileTexts();
        }


        private void ReshuffleDiscardPile()
        {
            Debug.Log($"{DebugTag} Reshuffling discard pile...");
            foreach (var i in DiscardPile)
            {
                DrawPile.Add(i);
            }

            DiscardPile.Clear();
        }

        public void SetHandController(HandController controller)
        {
            handController = controller;
        }
    }
}
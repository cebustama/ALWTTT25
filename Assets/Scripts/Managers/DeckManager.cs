using ALWTTT.Cards;
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

        public List<CardDefinition> DrawPile { get; private set; } =
            new List<CardDefinition>();
        public List<CardDefinition> HandPile { get; private set; } =
            new List<CardDefinition>();
        public List<CardDefinition> DiscardPile { get; private set; } =
            new List<CardDefinition>();
        public List<CardDefinition> ExhaustPile { get; private set; } =
            new List<CardDefinition>();

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
            foreach (var i in GameManager.PersistentGameplayData.CurrentActionCards)
            {
                DrawPile.Add(i);
            }
        }

        public void DrawCards(int targetDrawCount)
        {
            if (targetDrawCount <= 0)
            {
                Debug.Log($"{DebugTag} Drawing 0 cards... (requested={targetDrawCount})");
                return;
            }

            if (HandController == null)
            {
                Debug.LogError($"{DebugTag} Cannot draw: HandController is null.");
                return;
            }

            if (HandController.DrawTransform == null)
            {
                Debug.LogError($"{DebugTag} Cannot draw: HandController.DrawTransform is null.");
                return;
            }

            int available = DrawPile.Count + DiscardPile.Count;
            int drawCount = Mathf.Min(targetDrawCount, available);

            Debug.Log(
                $"{DebugTag} Drawing {drawCount} cards... " +
                $"(requested={targetDrawCount}, draw={DrawPile.Count}, discard={DiscardPile.Count})"
            );

            for (int i = 0; i < drawCount; i++)
            {
                // Cards on hand limit
                if (GameManager.GameplayData.MaxCardsOnHand <= HandPile.Count)
                {
                    Debug.Log($"{DebugTag} Max number of cards on hand reached.");
                    break;
                }

                // Reshuffle if no more cards in Draw Pile
                if (DrawPile.Count <= 0)
                {
                    if (DiscardPile.Count <= 0)
                    {
                        Debug.LogWarning(
                            $"{DebugTag} Cannot continue draw: DrawPile empty and DiscardPile empty.");
                        break;
                    }

                    ReshuffleDiscardPile();

                    if (DrawPile.Count <= 0)
                    {
                        Debug.LogError(
                            $"{DebugTag} Reshuffle produced empty DrawPile. Aborting draw.");
                        break;
                    }
                }

                var randomCard = DrawPile[Random.Range(0, DrawPile.Count)];

                if (randomCard == null)
                {
                    Debug.LogWarning($"{DebugTag} Encountered null CardDefinition in DrawPile. Removing.");
                    DrawPile.Remove(randomCard);
                    i--;
                    continue;
                }

                // Instantiate card as child of HandController.DrawTransform
                var card = GameManager.BuildAndGetCard(
                    randomCard, HandController.DrawTransform);

                if (card == null)
                {
                    Debug.LogError(
                        $"{DebugTag} BuildAndGetCard returned null for '{randomCard.name}'.");
                }
                else
                {
                    if (!card.gameObject.activeInHierarchy)
                    {
                        Debug.LogWarning(
                            $"{DebugTag} Drew '{randomCard.name}' but card GameObject is inactive. " +
                            $"(ParentActive={HandController.DrawTransform.gameObject.activeInHierarchy})"
                        );
                    }

                    HandController.AddCardToHand(card);
                }

                HandPile.Add(randomCard);
                DrawPile.Remove(randomCard);

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
            //Debug.Log($"{DebugTag} On Card Played...");

            if (targetCard.CardDefinition.ExhaustAfterPlay)
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
            HandPile.Remove(targetCard.CardDefinition);
            DiscardPile.Add(targetCard.CardDefinition);

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

        public void AddToDrawPile(IEnumerable<CardDefinition> cards, bool shuffle = true)
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

        public void AddToDrawPile(CardDefinition card)
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
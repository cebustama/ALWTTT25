using ALWTTT.Cards;
using ALWTTT.Enums;
using System;
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

            var gm = GameManager;
            var pd = gm != null ? gm.PersistentGameplayData : null;
            if (pd == null)
            {
                Debug.LogError($"{DebugTag} Cannot set deck: PersistentGameplayData is null.");
                return;
            }

            void Add(IEnumerable<CardDefinition> src, string label)
            {
                if (src == null) return;
                foreach (var c in src)
                {
                    if (c == null) continue;

                    // Solo metemos cartas v�lidas para el deck mixto
                    if (!c.IsAction && !c.IsComposition)
                    {
                        Debug.LogWarning($"{DebugTag} Skipping non Action/Composition card '{c.name}' from {label}.");
                        continue;
                    }

                    DrawPile.Add(c);
                }
            }

            Add(pd.CurrentActionCards, "Action");
            Add(pd.CurrentCompositionCards, "Composition");

            Debug.Log($"{DebugTag} Deck built: total={DrawPile.Count} " +
                      $"(Action={pd.CurrentActionCards?.Count ?? 0}, Composition={pd.CurrentCompositionCards?.Count ?? 0})");
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
                $"(requested={targetDrawCount}, draw={DrawPile.Count}, discard={DiscardPile.Count}, " +
                $"handBefore={HandController.Hand.Count})"
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

                var randomCard = DrawPile[UnityEngine.Random.Range(0, DrawPile.Count)];

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

            Debug.Log($"{DebugTag} Draw complete. handAfter={HandController.Hand.Count} handPile={HandPile.Count} draw={DrawPile.Count} discard={DiscardPile.Count}");

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

#if ALWTTT_DEV
        /// <summary>
        /// Dev Mode only. Hard-resets the hand without going through CardBase.Discard(),
        /// which gates on IsPlayable/IsExhausted and runs an async coroutine that may
        /// leave ghost GameObjects in the scene when the next PlayerTurn draws new cards.
        /// Moves HandPile entries to DiscardPile and destroys the CardBase GameObjects
        /// immediately. Returns the number of cards destroyed.
        /// </summary>
        public int DevForceHandResetToDiscard()
        {
            const string DevTag = "<color=lime>[DevMode]</color>";

            if (HandController == null)
            {
                Debug.LogWarning($"{DevTag} DeckManager.DevForceHandResetToDiscard: HandController is null. Aborting.");
                return 0;
            }
            if (HandController.Hand == null)
            {
                Debug.LogWarning($"{DevTag} DeckManager.DevForceHandResetToDiscard: HandController.Hand is null. Aborting.");
                return 0;
            }

            Debug.Log($"{DevTag} DeckManager.DevForceHandResetToDiscard: " +
                      $"BEFORE  hand={HandController.Hand.Count}  handPile={HandPile.Count}  " +
                      $"discard={DiscardPile.Count}  draw={DrawPile.Count}");

            // Also sweep any stray card GameObjects parented under DrawTransform but
            // not in the Hand list. These are candidates for "ghost" cards left over
            // from a Discard() coroutine that was abandoned mid-flight.
            int strayDestroyed = 0;
            if (HandController.DrawTransform != null)
            {
                var parent = HandController.DrawTransform;
                for (int i = parent.childCount - 1; i >= 0; i--)
                {
                    var child = parent.GetChild(i);
                    var cb = child != null ? child.GetComponent<CardBase>() : null;
                    if (cb == null) continue;
                    if (HandController.Hand.Contains(cb)) continue; // still tracked, let main loop handle
                    Debug.Log($"{DevTag}   Stray card under DrawTransform destroyed: {child.name}");
                    UnityEngine.Object.Destroy(child.gameObject);
                    strayDestroyed++;
                }
            }

            int destroyed = 0;
            var snapshot = new List<CardBase>(HandController.Hand);
            foreach (var card in snapshot)
            {
                if (card == null) continue;

                if (card.CardDefinition != null)
                {
                    HandPile.Remove(card.CardDefinition);
                    DiscardPile.Add(card.CardDefinition);
                }

                if (card.gameObject != null)
                    UnityEngine.Object.Destroy(card.gameObject);

                destroyed++;
            }

            HandController.Hand.Clear();

            if (UIManager != null && UIManager.GigCanvas != null)
                UIManager.GigCanvas.SetPileTexts();

            Debug.Log($"{DevTag} DeckManager.DevForceHandResetToDiscard: " +
                      $"AFTER  hand={HandController.Hand.Count}  handPile={HandPile.Count}  " +
                      $"discard={DiscardPile.Count}  draw={DrawPile.Count}  " +
                      $"destroyed={destroyed} strayDestroyed={strayDestroyed}");

            return destroyed;
        }

        // -----------------------------------------------------------------
        // Phase 2 — card spawner (2026-04-17)
        // -----------------------------------------------------------------

        /// <summary>
        /// Dev Mode only. Spawns <paramref name="def"/> directly into the hand
        /// through the normal play pipeline. Gated on PlayerTurn phase, on the
        /// MaxCardsOnHand cap, and on an active HandController.DrawTransform
        /// (the hand-visibility gap from Phase 1, SSoT_Dev_Mode §5).
        ///
        /// The spawned card is added to HandPile (and not to DrawPile), exactly
        /// mirroring the per-card tail of DrawCards(). On play it moves through
        /// HandPile → DiscardPile via the normal OnCardDiscarded path, which
        /// means after one reshuffle cycle the card enters DrawPile organically.
        /// Decision U1 (2026-04-17): accept this "enters deck after reshuffle"
        /// behavior; no ephemeral tagging.
        ///
        /// Returns true iff the card was spawned.
        /// </summary>
        public bool DevSpawnCardToHand(CardDefinition def)
        {
            const string DevTag = "<color=lime>[DevMode]</color>";

            if (def == null)
            {
                Debug.LogWarning($"{DevTag} DevSpawnCardToHand: CardDefinition is null.");
                return false;
            }

            if (!CanDevSpawnToHand(out string reason))
            {
                Debug.Log($"{DevTag} DevSpawnCardToHand skipped ('{def.DisplayName}'): {reason}");
                return false;
            }

            var built = GameManager.BuildAndGetCard(def, HandController.DrawTransform);
            if (built == null)
            {
                Debug.LogError(
                    $"{DevTag} DevSpawnCardToHand: BuildAndGetCard returned null for '{def.DisplayName}'.");
                return false;
            }

            if (!built.gameObject.activeInHierarchy)
            {
                // Same defensive log DrawCards emits. Should never fire given the gate
                // above, but logged loudly if it does.
                Debug.LogWarning(
                    $"{DevTag} DevSpawnCardToHand: spawned '{def.DisplayName}' but GameObject is inactive. " +
                    $"(ParentActive={HandController.DrawTransform.gameObject.activeInHierarchy})");
            }

            HandController.AddCardToHand(built);
            HandPile.Add(def);

            if (UIManager != null && UIManager.GigCanvas != null)
                UIManager.GigCanvas.SetPileTexts();

            int max = GameManager != null && GameManager.GameplayData != null
                ? GameManager.GameplayData.MaxCardsOnHand
                : -1;

            Debug.Log(
                $"{DevTag} DevSpawnCardToHand: '{def.DisplayName}' → " +
                $"hand={HandController.Hand.Count}/{max}  handPile={HandPile.Count}  " +
                $"discard={DiscardPile.Count}  draw={DrawPile.Count}");

            return true;
        }

        /// <summary>
        /// Dev Mode only. Returns true iff DevSpawnCardToHand would succeed right now.
        /// Overload without the out parameter for callers that only need the bool.
        /// </summary>
        public bool CanDevSpawnToHand() => CanDevSpawnToHand(out _);

        /// <summary>
        /// Dev Mode only. Returns true iff DevSpawnCardToHand would succeed right now,
        /// and sets <paramref name="reason"/> to a short human-readable explanation
        /// either way. Centralizes all gate checks so the overlay stays dumb.
        /// </summary>
        public bool CanDevSpawnToHand(out string reason)
        {
            if (HandController == null)
            {
                reason = "HandController is null";
                return false;
            }
            if (HandController.DrawTransform == null)
            {
                reason = "HandController.DrawTransform is null";
                return false;
            }
            if (!HandController.DrawTransform.gameObject.activeInHierarchy)
            {
                reason = "HandController.DrawTransform inactive (hand hidden)";
                return false;
            }
            if (GameManager == null || GameManager.GameplayData == null)
            {
                reason = "GameManager.GameplayData is null";
                return false;
            }
            if (GigManager.Instance == null)
            {
                reason = "GigManager.Instance is null";
                return false;
            }
            if (GigManager.Instance.CurrentGigPhase != GigPhase.PlayerTurn)
            {
                reason = $"Not PlayerTurn (current: {GigManager.Instance.CurrentGigPhase})";
                return false;
            }
            if (HandPile.Count >= GameManager.GameplayData.MaxCardsOnHand)
            {
                reason = $"Hand full ({HandPile.Count}/{GameManager.GameplayData.MaxCardsOnHand})";
                return false;
            }

            reason = "ready";
            return true;
        }
#endif

        /// <summary>
        /// Discard only the cards in hand that satisfy the predicate.
        /// Useful for MVP rules like: when Play is pressed, discard Action cards.
        /// </summary>
        public void DiscardHandWhere(Func<CardBase, bool> predicate)
        {
            if (HandController == null || HandController.Hand == null)
                return;

            if (predicate == null)
                return;

            for (int i = HandController.Hand.Count - 1; i >= 0; i--)
            {
                var card = HandController.Hand[i];
                if (card == null)
                {
                    HandController.Hand.RemoveAt(i);
                    continue;
                }

                if (!predicate(card))
                    continue;

                card.Discard();
                HandController.Hand.RemoveAt(i);
            }

            if (UIManager != null && UIManager.GigCanvas != null)
                UIManager.GigCanvas.SetPileTexts();
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
                    int j = UnityEngine.Random.Range(0, DrawPile.Count);
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
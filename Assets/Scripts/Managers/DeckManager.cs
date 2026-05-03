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

        // M4.5: last-turn summary surfaced to Dev Mode overlay.
        private string _lastTurnGuaranteeSummary = "";
        public string LastTurnGuaranteeSummary => _lastTurnGuaranteeSummary;

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

        /// <summary>
        /// M4.5 — Player-turn draw with bidirectional domain guarantees (subtractive).
        /// Total drawn ≤ budget. Reserves up to 2 slots for ≥1 Composition and ≥1 Action
        /// in hand when piles allow. Re-evaluates after Phase 1 normal draws so reserved
        /// slots are not wasted. Tie-break when budget cannot fit both guarantees:
        /// Composition wins.
        /// </summary>
        public void DrawCardsForPlayerTurn(int budget)
        {
            if (HandController == null)
            {
                Debug.LogError($"{DebugTag} [M4.5] HandController null; falling back to DrawCards.");
                DrawCards(budget);
                return;
            }

            int handPileBefore = HandPile.Count;
            int maxHand = GameManager.GameplayData.MaxCardsOnHand;
            int available = DrawPile.Count + DiscardPile.Count;
            int effectiveBudget = Mathf.Min(budget, maxHand - HandPile.Count, available);

            if (effectiveBudget <= 0)
            {
                _lastTurnGuaranteeSummary =
                    $"needs=[--] reserved=0 fired=[--] drawn=0/{effectiveBudget} (budget=0)";
                Debug.Log($"{DebugTag} [M4.5] {_lastTurnGuaranteeSummary}");
                return;
            }

            bool needComp = !HandHas(c => c.IsComposition) && PilesHave(c => c.IsComposition);
            bool needAction = !HandHas(c => c.IsAction) && PilesHave(c => c.IsAction);

            // Composition-first tie-break when budget cannot fit both guarantees.
            int reserved = (needComp ? 1 : 0) + (needAction ? 1 : 0);
            if (reserved > effectiveBudget)
            {
                if (needComp && needAction) needAction = false;
                reserved = (needComp ? 1 : 0) + (needAction ? 1 : 0);
            }

            int phase1 = Mathf.Max(0, effectiveBudget - reserved);

            Debug.Log($"{DebugTag} [M4.5] Begin turn draw: budget={budget} effective={effectiveBudget} " +
                      $"needComp={needComp} needAction={needAction} reserved={reserved} phase1={phase1}");

            // Phase 1: normal draws holding back `reserved` slots.
            if (phase1 > 0)
                DrawCards(phase1);

            // Phase 2: re-evaluate guarantees against current hand.
            bool firedComp = false, firedAction = false;

            if (needComp && !HandHas(c => c.IsComposition) && PilesHave(c => c.IsComposition) &&
                HandPile.Count < maxHand)
            {
                firedComp = DrawCardFiltered(c => c.IsComposition, "M4.5 GuaranteeComp");
            }

            if (needAction && !HandHas(c => c.IsAction) && PilesHave(c => c.IsAction) &&
                HandPile.Count < maxHand)
            {
                firedAction = DrawCardFiltered(c => c.IsAction, "M4.5 GuaranteeAction");
            }

            // Phase 3: any unused reserved slots become normal draws.
            int drawnSoFar = HandPile.Count - handPileBefore;
            int leftover = effectiveBudget - drawnSoFar;
            if (leftover > 0)
                DrawCards(leftover);

            int drawnTotal = HandPile.Count - handPileBefore;

            _lastTurnGuaranteeSummary =
                $"needs=[{(needComp ? "C" : "-")}{(needAction ? "A" : "-")}] " +
                $"reserved={reserved} " +
                $"fired=[{(firedComp ? "C" : "-")}{(firedAction ? "A" : "-")}] " +
                $"drawn={drawnTotal}/{effectiveBudget}";

            Debug.Log($"{DebugTag} [M4.5] End turn draw: {_lastTurnGuaranteeSummary}");
        }

        /// <summary>
        /// M4.5 — Draws exactly one card matching the predicate. Reshuffles
        /// DiscardPile into DrawPile once if no match exists in DrawPile.
        /// Returns false when no match exists in either pile, or when prerequisites
        /// (HandController, MaxCardsOnHand) block the draw.
        /// </summary>
        public bool DrawCardFiltered(Func<CardDefinition, bool> predicate, string reasonTag = null)
        {
            if (predicate == null) return false;

            string tag = string.IsNullOrEmpty(reasonTag) ? "[Filtered]" : $"[{reasonTag}]";

            if (HandController == null || HandController.DrawTransform == null)
            {
                Debug.LogError($"{DebugTag} {tag} HandController/DrawTransform null. Aborting filtered draw.");
                return false;
            }

            if (GameManager.GameplayData.MaxCardsOnHand <= HandPile.Count)
            {
                Debug.Log($"{DebugTag} {tag} Hand at MaxCardsOnHand. Skipping filtered draw.");
                return false;
            }

            int matchIndex = FindRandomMatchIndex(DrawPile, predicate);

            if (matchIndex < 0)
            {
                // DrawPile has no match. Reshuffle iff DiscardPile has at least one match.
                bool discardHasMatch = false;
                for (int i = 0; i < DiscardPile.Count; i++)
                {
                    if (DiscardPile[i] != null && predicate(DiscardPile[i])) { discardHasMatch = true; break; }
                }

                if (!discardHasMatch)
                {
                    Debug.Log($"{DebugTag} {tag} No matching card in DrawPile or DiscardPile. Skipping.");
                    return false;
                }

                ReshuffleDiscardPile();
                matchIndex = FindRandomMatchIndex(DrawPile, predicate);

                if (matchIndex < 0)
                {
                    Debug.LogError($"{DebugTag} {tag} Reshuffle did not surface a match. Aborting.");
                    return false;
                }
            }

            var matchCard = DrawPile[matchIndex];

            if (matchCard == null)
            {
                Debug.LogWarning($"{DebugTag} {tag} Null reference at match index {matchIndex}. Removing.");
                DrawPile.RemoveAt(matchIndex);
                return false;
            }

            var cardObj = GameManager.BuildAndGetCard(matchCard, HandController.DrawTransform);

            if (cardObj == null)
            {
                Debug.LogError($"{DebugTag} {tag} BuildAndGetCard returned null for '{matchCard.name}'.");
                return false;
            }

            HandController.AddCardToHand(cardObj);
            HandPile.Add(matchCard);
            DrawPile.RemoveAt(matchIndex);

            Debug.Log($"{DebugTag} {tag} Drew filtered card '{matchCard.name}'. " +
                      $"hand={HandController.Hand.Count} draw={DrawPile.Count} discard={DiscardPile.Count}");

            if (UIManager != null && UIManager.GigCanvas != null)
                UIManager.GigCanvas.SetPileTexts();

            return true;
        }

        /// <summary>
        /// M4.5 — Returns true iff at least one card in the current hand matches the predicate.
        /// </summary>
        public bool HandHas(Func<CardDefinition, bool> predicate)
        {
            if (predicate == null || HandController == null || HandController.Hand == null) return false;
            for (int i = 0; i < HandController.Hand.Count; i++)
            {
                var cb = HandController.Hand[i];
                if (cb != null && cb.CardDefinition != null && predicate(cb.CardDefinition))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// M4.5 — Returns true iff at least one card in DrawPile or DiscardPile matches the predicate.
        /// HandPile and ExhaustPile are intentionally excluded — guarantees check what is drawable now.
        /// </summary>
        public bool PilesHave(Func<CardDefinition, bool> predicate)
        {
            if (predicate == null) return false;
            for (int i = 0; i < DrawPile.Count; i++)
                if (DrawPile[i] != null && predicate(DrawPile[i])) return true;
            for (int i = 0; i < DiscardPile.Count; i++)
                if (DiscardPile[i] != null && predicate(DiscardPile[i])) return true;
            return false;
        }

        private static int FindRandomMatchIndex(
            List<CardDefinition> pile, Func<CardDefinition, bool> predicate)
        {
            if (pile == null || pile.Count == 0) return -1;

            // Reservoir-style: collect indices, pick one uniformly. Cheap for hand-scale piles.
            var matches = new List<int>(pile.Count);
            for (int i = 0; i < pile.Count; i++)
            {
                if (pile[i] != null && predicate(pile[i]))
                    matches.Add(i);
            }

            if (matches.Count == 0) return -1;
            return matches[UnityEngine.Random.Range(0, matches.Count)];
        }
    }
}
using ALWTTT.Cards;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S4 D-S4-BUS=B] Published once per played card from the single unified
    /// play funnel <c>DeckManager.OnCardPlayed</c> — the one site that observes
    /// BOTH action/SFX cards (via CardBase.Use) and composition cards (which
    /// bypass CardBase.Use and are routed through GigManager.TryPlayCompositionCard,
    /// with HandController calling OnCardPlayed for them — HandController.cs:599-602).
    ///
    /// This corrects the §3 forward-inventory producer ("CardBase.ExecuteEffects /
    /// CardUseRoutine"), which never sees composition cards and would have left
    /// jam beats 1 &amp; 5 dead.
    ///
    /// Semantic payload only. Tutorial gates:
    ///   - beat 1 (tut_first_composition_card): IsComposition
    ///   - beat 2 (tut_first_inspiration_spend): InspirationCost &gt; 0
    ///   - beat 5 (tut_first_sound_card):       IsComposition &amp;&amp; tempo/modulation
    ///     (classified by the consumer via the ALWTTT-owned CompositionCardClassifier)
    ///   - standalone tut_first_action_card:    IsAction
    /// </summary>
    public readonly struct CardPlayedEvent : ISensoryEvent
    {
        /// <summary>The played card's definition. Stable SO reference (safe even
        /// after the card GameObject is discarded/exhausted in OnCardPlayed).</summary>
        public CardDefinition Definition { get; }

        /// <summary>True when payload is a CompositionCardPayload.</summary>
        public bool IsComposition { get; }

        /// <summary>True when payload is an ActionCardPayload.</summary>
        public bool IsAction { get; }

        /// <summary>Authored inspiration cost (def.InspirationCost). Beat 2 uses
        /// &gt; 0 so a free composition card doesn't teach "cards cost Inspiration".</summary>
        public int InspirationCost { get; }

        public CardPlayedEvent(
            CardDefinition definition,
            bool isComposition,
            bool isAction,
            int inspirationCost)
        {
            Definition = definition;
            IsComposition = isComposition;
            IsAction = isAction;
            InspirationCost = inspirationCost;
        }
    }
}

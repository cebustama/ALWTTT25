// Place at: Assets/Scripts/Sensory/PlayDeniedEvent.cs
using ALWTTT.Cards;
using ALWTTT.Characters.Band;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [TUT-REDESIGN-B / D-TUTR-5, D-TUTB-1=A] Why a card play was refused.
    /// Typed so tutorial beats subscribe by reason, never by matching the
    /// on-screen string (which is copy and may change).
    /// </summary>
    public enum PlayDenyReason
    {
        Other = 0,
        EconBudget,             // ECON-1: this musician has no play of that kind left this period
        FinalLoopLock,          // CARD-UX-1: composition on the last loop would never be heard
        ResourceCost,           // R5: e.g. Voltage
        InspirationCost,        // not enough inspiration
        HarmonyPrecondition,    // D-R6-4: Harmony needs the same musician's Melody on the part
        NoTarget,               // card requires a musician target and none resolved
        Timing,                 // action card outside its timing window
        BonusLoopPrecondition,  // R5-d: nothing running to extend
    }

    /// <summary>
    /// [TUT-REDESIGN-B / D-TUTR-5] Published from the single denial funnel
    /// <c>GigManager.ReportPlayDenied</c> whenever a play is refused and the
    /// player is told why. Same pattern as <see cref="AudienceBlockedEvent"/>:
    /// a beat needed a fact the bus did not carry. Semantic payload only;
    /// <see cref="DisplayText"/> is what the message UI showed, for logs.
    ///
    /// Tutorial gates — tut_play_budget: first EconBudget · tut_final_loop_lock:
    /// first FinalLoopLock · tut_harmony_denied: first HarmonyPrecondition.
    /// </summary>
    public readonly struct PlayDeniedEvent : ISensoryEvent
    {
        public PlayDenyReason Reason { get; }
        public string DisplayText { get; }
        /// <summary>May be null (denied before the card resolved).</summary>
        public CardDefinition Card { get; }
        /// <summary>May be null (denied before the payer resolved).</summary>
        public MusicianBase Payer { get; }

        public PlayDeniedEvent(PlayDenyReason reason, string displayText,
            CardDefinition card, MusicianBase payer)
        {
            Reason = reason;
            DisplayText = displayText;
            Card = card;
            Payer = payer;
        }
    }
}
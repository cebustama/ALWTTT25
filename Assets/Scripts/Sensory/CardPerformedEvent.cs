using ALWTTT.Cards;
using ALWTTT.Characters.Band;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [WINK-1 D-WINK-1=A] Published at the two card COMMIT sites
    /// (HandController action path, GigManager composition path), right next
    /// to the existing PlayCardOneShotAnimation calls — i.e. AFTER every
    /// denial gate (timing, budget, resource, final-loop lock) and BEFORE
    /// card effects resolve. CardPlayedEvent cannot serve this beat: it
    /// fires post-effects, too late for a performer "shout".
    ///
    /// Semantic payload only; presentation derives everything
    /// (SensoryFtPresentation.TryBuildCardPerformedFt, D-WINK-6=B).
    /// </summary>
    public readonly struct CardPerformedEvent : ISensoryEvent
    {
        /// <summary>Musician that commits the play. Never null from the two
        /// publishers (both sit inside a MusicianBase-guarded branch).</summary>
        public MusicianBase Performer { get; }

        /// <summary>Definition of the committed card.</summary>
        public CardDefinition Card { get; }

        public CardPerformedEvent(MusicianBase performer, CardDefinition card)
        {
            Performer = performer;
            Card = card;
        }
    }
}
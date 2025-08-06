using ALWTTT.Characters;
using ALWTTT.Enums;
using ALWTTT.Managers;

namespace ALWTTT.Cards
{
    public class CardActionParameters
    {
        public readonly float Value;
        public readonly CharacterBase PerformerCharacter;
        public readonly CharacterBase TargetCharacter;
        public readonly CardData CardData;
        public readonly CardBase CardBase;

        public CardActionParameters(float value, 
            CharacterBase performer, CharacterBase target, 
            CardData cardData, CardBase cardBase)
        {
            Value = value;
            PerformerCharacter = performer;
            TargetCharacter = target;
            CardData = cardData;
            CardBase = cardBase;
        }
    }

    public abstract class CardActionBase
    {
        protected CardActionBase() { }
        public abstract CardActionType ActionType { get; }
        public abstract void DoAction(CardActionParameters actionParameters);

        protected GameManager GameManager => GameManager.Instance;
        protected GigManager GigManager => GigManager.Instance;
        protected DeckManager DeckManager => DeckManager.Instance;
    }
}
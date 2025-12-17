using ALWTTT.Cards;
using ALWTTT.Characters;
using UnityEngine;

namespace ALWTTT.Actions
{
    public class CharacterActionParameters
    {
        public readonly float Value;
        public readonly CharacterBase PerformerCharacter; // who triggers the action
        public readonly CharacterBase TargetCharacter;    // who is affected
        public readonly CharacterActionContext Context;   // optional, typed
        public readonly float Duration;

        public CharacterActionParameters(
            float value,
            CharacterBase performer,
            CharacterBase target,
            CharacterActionContext context = null,
            float duration = 2f)
        {
            Value = value;
            PerformerCharacter = performer;
            TargetCharacter = target;
            Context = context;
            Duration = duration;
        }
    }

    // Marker/base for optional caller-specific data
    public abstract class CharacterActionContext { }

    public sealed class CardActionContext : CharacterActionContext
    {
        public readonly CardDefinition CardDefinition;
        public readonly CardBase CardBase;

        public CardActionContext(CardDefinition cardData, CardBase cardBase)
        {
            CardDefinition = cardData;
            CardBase = cardBase;
        }
    }

    public sealed class AudienceActionContext : CharacterActionContext
    {
        public AudienceActionContext()
        {

        }
    }
}
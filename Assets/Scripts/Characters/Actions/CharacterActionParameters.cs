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

        public CharacterActionParameters(
            float value,
            CharacterBase performer,
            CharacterBase target,
            CharacterActionContext context = null)
        {
            Value = value;
            PerformerCharacter = performer;
            TargetCharacter = target;
            Context = context;
        }
    }

    // Marker/base for optional caller-specific data
    public abstract class CharacterActionContext { }

    public sealed class CardActionContext : CharacterActionContext
    {
        public readonly CardData CardData;
        public readonly CardBase CardBase;

        public CardActionContext(CardData cardData, CardBase cardBase)
        {
            CardData = cardData;
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
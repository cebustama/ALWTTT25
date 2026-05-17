using ALWTTT.Cards;
using ALWTTT.Characters;
using ALWTTT.Status;
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

        // [B3-content-audience pass2 / D10=A] Carries the SO to apply when
        // the dispatched action is ApplyStatusEffect. Null for all other actions.
        // Sourced from CharacterActionData.StatusEffect by AudienceCharacterBase
        // ExecuteActionWithTiming. Convention: meaningful only when dispatching
        // CharacterActionType.ApplyStatusEffect.
        public readonly StatusEffectSO StatusEffect;

        public CharacterActionParameters(
            float value,
            CharacterBase performer,
            CharacterBase target,
            CharacterActionContext context = null,
            float duration = 2f,
            StatusEffectSO statusEffect = null)
        {
            Value = value;
            PerformerCharacter = performer;
            TargetCharacter = target;
            Context = context;
            Duration = duration;
            StatusEffect = statusEffect;
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
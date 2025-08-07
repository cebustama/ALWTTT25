using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Characters.Audience.Actions
{
    public abstract class AudienceActionBase
    {
        protected AudienceActionBase() { }

        public abstract CardActionType ActionType { get; }
        public abstract void DoAction(AudienceActionParameters actionParameters);
    }

    public class AudienceActionParameters
    {
        public readonly float Value;
        public readonly CharacterBase TargetCharacter;
        public readonly CharacterBase SelfCharacter;

        public AudienceActionParameters(float value, CharacterBase target, CharacterBase self)
        {
            Value = value;
            TargetCharacter = target;
            SelfCharacter = self;
        }
    }
}


using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Cards.Actions
{
    public class AddVibeAction : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.AddVibe;

        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var performerCharacter = actionParameters.PerformerCharacter;
            var targetCharacter = actionParameters.TargetCharacter;

            var value = actionParameters.Value; // TODO: Character Stats & Statuses


        }
    }
}
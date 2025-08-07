using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Cards.Actions
{
    public class RemoveVibeAction : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.RemoveVibe;

        public override string ActionName => "Remove Vibe";

        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var performerCharacter = actionParameters.PerformerCharacter;
            var targetCharacter = actionParameters.TargetCharacter;
            Debug.Log($"[{ActionName}] Target: " + targetCharacter);
            Debug.Log($"[{ActionName}] Stats: {targetCharacter.AudienceStats.ToString()}");

            if (targetCharacter.AudienceStats is { } audienceStats) 
            {
                int vibeToRemove = Mathf.RoundToInt(actionParameters.Value);
                audienceStats.RemoveVibe(vibeToRemove);
            }
            else
            {
                Debug.LogWarning("Target does not have AudienceStats — " +
                    $"[{ActionName}] skipped.");
            }
        }
    }
}
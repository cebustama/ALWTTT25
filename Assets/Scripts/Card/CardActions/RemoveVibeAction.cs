using ALWTTT.Actions;
using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Cards.Actions
{
    public class RemoveVibeAction : CharacterActionBase
    {
        public override CharacterActionType ActionType => CharacterActionType.RemoveVibe;

        public override string ActionName => "Remove Vibe";

        public override void DoAction(CharacterActionParameters actionParameters)
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
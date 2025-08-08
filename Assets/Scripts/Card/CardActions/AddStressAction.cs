using ALWTTT.Actions;
using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Cards.Actions 
{
    public class AddStressAction : CharacterActionBase
    {
        public override CharacterActionType ActionType => CharacterActionType.AddStress;

        public override string ActionName => "Add Stress";

        public override void DoAction(CharacterActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var performerCharacter = actionParameters.PerformerCharacter;
            var targetCharacter = actionParameters.TargetCharacter;
            Debug.Log($"[{ActionName}] Target: " + targetCharacter);
            Debug.Log($"[{ActionName}] Stats: {targetCharacter.MusicianStats.ToString()}");

            if (targetCharacter.MusicianStats is { } musicianStats)
            {
                int stressToAdd = Mathf.RoundToInt(actionParameters.Value);
                musicianStats.AddStress(stressToAdd);
            }
            else
            {
                Debug.LogWarning("Target does not have MusicianStats — " +
                    $"{ActionName} skipped.");
            }
        }
    }
}
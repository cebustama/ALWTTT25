using ALWTTT.Actions;
using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Cards.Actions
{
    public class HealStressAction : CharacterActionBase
    {
        public override CharacterActionType ActionType => CharacterActionType.HealStress;

        public override string ActionName => "Heal Stress";

        public override void DoAction(CharacterActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var performerCharacter = actionParameters.PerformerCharacter;
            var targetCharacter = actionParameters.TargetCharacter;
            Debug.Log($"[{ActionName}] Target: " + targetCharacter);
            Debug.Log($"[{ActionName}] Stats: {targetCharacter.MusicianStats.ToString()}");

            if (targetCharacter.MusicianStats is { } musicianStats)
            {
                int stressToHeal = Mathf.RoundToInt(actionParameters.Value);
                musicianStats.HealStress(stressToHeal);
            }
            else
            {
                Debug.LogWarning("Target does not have MusicianStats — " +
                    $"{ActionName} skipped.");
            }
        }
    }
}

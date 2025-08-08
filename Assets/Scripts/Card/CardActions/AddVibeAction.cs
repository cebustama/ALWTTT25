using ALWTTT.Actions;
using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Cards.Actions
{
    public class AddVibeAction : CharacterActionBase
    {
        public override CharacterActionType ActionType => CharacterActionType.AddVibe;

        public override string ActionName => "Add Vibe";

        public override void DoAction(CharacterActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var performerCharacter = actionParameters.PerformerCharacter;
            var targetCharacter = actionParameters.TargetCharacter;
            Debug.Log($"[{ActionName}] Target: " + targetCharacter);
            Debug.Log($"[{ActionName}] Stats: {targetCharacter.AudienceStats.ToString()}");

            if (targetCharacter.AudienceStats is { } audienceStats)
            {
                int vibeToAdd = Mathf.RoundToInt(actionParameters.Value);
                audienceStats.AddVibe(vibeToAdd);

                /*
                FxManager?.PlayFx(target.transform, FxType.Buff);
                AudioManager?.PlayOneShot(actionParameters.CardData.AudioType);
                */
            }
            else
            {
                Debug.LogWarning("Target does not have AudienceStats — " +
                    $"{ActionName} skipped.");
            }
        }
    }
}
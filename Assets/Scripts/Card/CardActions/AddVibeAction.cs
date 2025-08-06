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
            Debug.Log("[AddVibeAction] Target: " + targetCharacter);
            Debug.Log($"[AddVibeAction] Stats: {targetCharacter.AudienceStats.ToString()}");

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
                    "AddVibeAction skipped.");
            }
        }
    }
}
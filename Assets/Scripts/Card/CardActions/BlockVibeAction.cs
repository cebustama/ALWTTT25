using ALWTTT.Actions;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using UnityEngine;

namespace ALWTTT.Cards.Actions
{
    public class BlockVibeAction : CharacterActionBase
    {
        public override CharacterActionType ActionType => CharacterActionType.BlockVibe;

        public override string ActionName => "Block Vibe";

        public override void DoAction(CharacterActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var performerCharacter = actionParameters.PerformerCharacter;
            var targetCharacter = actionParameters.TargetCharacter;

            if (targetCharacter.AudienceStats is { } audienceStats)
            {
                int vibeBlockToAdd = Mathf.RoundToInt(actionParameters.Value);
                    // Add Dexterity
                    //+ audienceStats.statusDict[StatusType.Dexterity].StatusValue);

                audienceStats.ApplyStatus(StatusType.Skeptical, vibeBlockToAdd);

                FxManager.PlayFx(targetCharacter.HeadRoot, FxType.BlockVibe);

                if (actionParameters.Context is AudienceActionContext audienceCtx)
                {
                    // TODO: Audience Ability AudioType
                    //AudioManager.PlayOneShot(audienceCtx);
                }
            }
            else
            {
                Debug.LogWarning("Target does not have AudienceStats — " +
                    $"{ActionName} skipped.");
            }
        }
    }
}
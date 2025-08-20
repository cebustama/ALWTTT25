using ALWTTT.Actions;
using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Cards.Actions
{
    public class BlockStressAction : CharacterActionBase
    {
        public override CharacterActionType ActionType => CharacterActionType.BlockStress;

        public override string ActionName => "Block Stress";

        public override void DoAction(CharacterActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var performerCharacter = actionParameters.PerformerCharacter;
            var targetCharacter = actionParameters.TargetCharacter;

            if (targetCharacter.MusicianStats is { } musicianStats)
            {
                // Base Value
                int chillToAdd = Mathf.RoundToInt(actionParameters.Value);
                    // Add Dexterity
                    //+ musicianStats.StatusDict[StatusType.Dexterity].StatusValue);

                // Apply
                musicianStats.ApplyStatus(StatusType.Chill, chillToAdd);

                FxManager.PlayFx(targetCharacter.HeadRoot, FxType.BlockStress);

                if (actionParameters.Context is CardActionContext cardCtx)
                {
                    AudioManager.PlayOneShot(cardCtx.CardData.AudioType);
                }
            }
            else
            {
                Debug.LogWarning("Target does not have MusicianStats — " +
                    $"{ActionName} skipped.");
            }
        }
    }
}
using ALWTTT.Actions;
using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Actions
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
                
                if (actionParameters.Context is CardActionContext cardCtx)
                {
                    switch (cardCtx.CardData.CardType)
                    {
                        case CardType.CHR:
                            chillToAdd =
                                Mathf.RoundToInt(
                                    musicianStats.Charm * actionParameters.Value);
                            break;
                        case CardType.TCH:
                            chillToAdd =
                                Mathf.RoundToInt(
                                    musicianStats.Technique * actionParameters.Value);
                            break;
                        case CardType.EMT:
                            chillToAdd =
                                Mathf.RoundToInt(
                                    musicianStats.Emotion * actionParameters.Value);
                            break;
                        default:
                            chillToAdd = Mathf.RoundToInt(actionParameters.Value);
                            break;
                    }
                }

                // Apply
                musicianStats.ApplyStatus(StatusType.Chill, chillToAdd);

                FxManager.PlayFx(targetCharacter.HeadRoot, FxType.BlockStress);

                if (actionParameters.Context is CardActionContext cardCtx2)
                {
                    AudioManager.PlayOneShot(cardCtx2.CardData.AudioType);
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
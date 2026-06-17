using ALWTTT.Actions;
using ALWTTT.Characters.Audience;
using ALWTTT.Enums;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace ALWTTT.Actions
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

                if (actionParameters.Context is CardActionContext cardCtx
                    && performerCharacter.MusicianStats is { } musicianStats)
                {
                    switch (cardCtx.CardDefinition.CardType)
                    {
                        case CardType.CHR:
                            vibeToAdd =
                                Mathf.RoundToInt(
                                    musicianStats.Charm * actionParameters.Value);
                            break;
                        case CardType.TCH:
                            vibeToAdd =
                                Mathf.RoundToInt(
                                    musicianStats.Technique * actionParameters.Value);
                            break;
                        case CardType.EMT:
                            vibeToAdd =
                                Mathf.RoundToInt(
                                    musicianStats.Emotion * actionParameters.Value);
                            break;
                        default:
                            vibeToAdd = Mathf.RoundToInt(actionParameters.Value);
                            break;
                    }
                }

                // [B3] Route through ApplyIncomingVibe canonical path. Suppress
                // ReceiveVibe FX on blocked (Indifference) to match the absence of
                // visible Vibe gain — no false positive feedback.
                int applied = 0;
                if (vibeToAdd > 0)
                {
                    applied = audienceStats.ApplyIncomingVibe(
                        targetCharacter.Statuses, vibeToAdd);
                }
                else
                {
                    audienceStats.AddVibe(vibeToAdd);
                    applied = vibeToAdd;
                }

                if (applied > 0)
                {
                    FxManager.PlayFx(targetCharacter.HeadRoot, FxType.ReceiveVibe);
                }
                else if (vibeToAdd > 0)
                {
                    Debug.Log(
                        $"<color=#888888>[{ActionName}] BLOCKED (Indifference) on " +
                        $"{targetCharacter}: intended=+{vibeToAdd} applied=0</color>");
                }

                //AudioManager?.PlayOneShot(actionParameters.CardData.AudioType);
            }
            else
            {
                Debug.LogWarning("Target does not have AudienceStats � " +
                    $"{ActionName} skipped.");
            }
        }
    }
}
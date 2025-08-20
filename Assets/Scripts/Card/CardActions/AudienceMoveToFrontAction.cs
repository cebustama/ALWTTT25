using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Actions
{
    // TODO: Use AudienceActionBase instead for all audience-specific actions
    public class AudienceMoveToFrontAction : CharacterActionBase
    {
        public override CharacterActionType ActionType => CharacterActionType.MoveToFront;

        public override string ActionName => "Move To Front";

        public override void DoAction(CharacterActionParameters actionParameters)
        {
            var performerCharacter = actionParameters.PerformerCharacter;
            Debug.Log($"[{ActionName}] Performer: " + performerCharacter);

            var positions = GigManager.AudienceMemberPosList;

            //var performerPosition = positions[performerCharacter.]
        }
    }
}

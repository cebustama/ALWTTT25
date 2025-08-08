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
            throw new System.NotImplementedException();
        }
    }
}
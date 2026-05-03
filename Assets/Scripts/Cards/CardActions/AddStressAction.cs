using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Managers;
using UnityEngine;

namespace ALWTTT.Actions
{
    public class AddStressAction : CharacterActionBase
    {
        public override CharacterActionType ActionType => CharacterActionType.AddStress;

        public override string ActionName => "Add Stress";

        public override void DoAction(CharacterActionParameters p)
        {
            if (!p.TargetCharacter) return;

            var performerCharacter = p.PerformerCharacter;
            var targetCharacter = p.TargetCharacter;
            Debug.Log($"[{ActionName}] Target: " + targetCharacter);
            Debug.Log($"[{ActionName}] Stats: {targetCharacter.MusicianStats.ToString()}");

            if (targetCharacter.MusicianStats is BandCharacterStats musicianStats)
            {
                int stressToAdd = Mathf.RoundToInt(p.Value);

                // M4.1: Route through the unified Stress path so Composure absorbs
                // and Exposed amplifies — same pipeline used by card effects.
                var (absorbed, applied) = musicianStats.ApplyIncomingStressWithComposure(
                    targetCharacter.Statuses,
                    stressToAdd,
                    p.Duration);

                Debug.Log($"[{ActionName}] Incoming={stressToAdd}  Absorbed={absorbed}  Applied={applied}");

                FxManager.PlayFx(targetCharacter.HeadRoot, FxType.ReceiveStress);

                if (p.Context is CardActionContext cardCtx)
                {
                    AudioManager.PlayOneShot(cardCtx.CardDefinition.AudioType);
                }
                else if (p.Context is AudienceActionContext audienceCtx)
                {
                    // TODO: 
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
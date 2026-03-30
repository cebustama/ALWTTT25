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

            if (targetCharacter.MusicianStats is { } musicianStats)
            {
                int stressToAdd = Mathf.RoundToInt(p.Value);

                // Use the unified Composure-aware stress path
                var bandStats = musicianStats as BandCharacterStats;
                if (bandStats != null)
                {
                    var (absorbed, applied) = bandStats.ApplyIncomingStressWithComposure(
                        p.TargetCharacter.Statuses, stressToAdd, p.Duration);

                    Debug.Log($"[{ActionName}] incoming={stressToAdd} absorbed={absorbed} applied={applied}");
                }
                else
                {
                    // Fallback: direct AddStress if cast fails (shouldn't happen for band musicians)
                    musicianStats.AddStress(stressToAdd, p.Duration);
                }

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
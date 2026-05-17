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
                int baseStress = Mathf.RoundToInt(p.Value);

                // [B3-content-audience pass1] Apply attacker-side outgoing modifiers
                // (Hyped today) BEFORE the receiver-side incoming pipeline.
                // Pipeline order: outgoing-modify → Composure-absorb → Exposed-amplify
                // → apply remainder. This ordering means Hyped raises the outgoing
                // amount that Composure absorbs against, and Exposed amplifies what
                // Composure couldn't absorb.
                var attackerStatuses = performerCharacter != null
                    ? performerCharacter.Statuses
                    : null;
                int modifiedStress = BandCharacterStats.ApplyOutgoingStressWithModifiers(
                    attackerStatuses, baseStress);

                // [M4.1] Canonical receiver-side helper preserved.
                var (absorbed, applied) = musicianStats.ApplyIncomingStressWithComposure(
                    targetCharacter.Statuses,
                    modifiedStress,
                    p.Duration);

                Debug.Log($"[{ActionName}] Base={baseStress}  Modified={modifiedStress}  " +
                          $"Absorbed={absorbed}  Applied={applied}");

                FxManager.PlayFx(targetCharacter.HeadRoot, FxType.ReceiveStress);

                if (p.Context is CardActionContext cardCtx)
                {
                    AudioManager.PlayOneShot(cardCtx.CardDefinition.AudioType);
                }
                else if (p.Context is AudienceActionContext audienceCtx)
                {
                    // TODO: audience-side reaction audio
                }
            }
            else
            {
                Debug.LogWarning("Target does not have MusicianStats - " +
                    $"{ActionName} skipped.");
            }
        }
    }
}
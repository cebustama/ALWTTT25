using ALWTTT.Enums;
using ALWTTT.Status;
using UnityEngine;

namespace ALWTTT.Actions
{
    /// <summary>
    /// Audience-ability counterpart to the card pipeline's ApplyStatusEffectSpec
    /// (B3-content-audience pass2 / D10=A). Reads StatusEffectSO from
    /// CharacterActionParameters.StatusEffect (sourced from CharacterActionData.StatusEffect
    /// by the dispatcher). Stacks delta is CharacterActionParameters.Value rounded
    /// to nearest int. Negative values allowed (enables future "remove stacks" abilities).
    /// </summary>
    public class ApplyStatusEffectAction : CharacterActionBase
    {
        public override CharacterActionType ActionType => CharacterActionType.ApplyStatusEffect;

        public override string ActionName => "Apply Status Effect";

        public override void DoAction(CharacterActionParameters p)
        {
            if (!p.TargetCharacter)
            {
                Debug.LogWarning($"[{ActionName}] Null TargetCharacter, skipping.");
                return;
            }

            var so = p.StatusEffect;
            if (so == null)
            {
                Debug.LogWarning(
                    $"[{ActionName}] CharacterActionData.StatusEffect is null on " +
                    $"performer '{p.PerformerCharacter?.name}'. Authoring error: " +
                    $"ApplyStatusEffect action requires a StatusEffectSO reference.");
                return;
            }

            if (p.TargetCharacter.Statuses == null)
            {
                Debug.LogWarning(
                    $"[{ActionName}] Target '{p.TargetCharacter.name}' has no Statuses container. " +
                    $"Skipping application of '{so.DisplayName}'.");
                return;
            }

            int stacksDelta = Mathf.RoundToInt(p.Value);
            if (stacksDelta == 0)
            {
                Debug.LogWarning(
                    $"[{ActionName}] stacksDelta=0 (Value={p.Value}) for SO '{so.DisplayName}'. " +
                    "No-op — was that the intent?");
                return;
            }

            p.TargetCharacter.Statuses.Apply(so, stacksDelta);

            Debug.Log(
                $"[{ActionName}] '{p.PerformerCharacter?.name}' applied " +
                $"{stacksDelta:+#;-#;0}x '{so.DisplayName}' (key='{so.StatusKey}', " +
                $"id={so.EffectId}) to '{p.TargetCharacter.name}'.");
        }
    }
}
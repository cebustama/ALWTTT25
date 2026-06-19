using ALWTTT.Status;
using ALWTTT.Status.Runtime;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S4 D-S4-SRC=A] Published from <c>StatusEffectContainer.Apply</c>, right
    /// after the existing per-container <c>OnStatusApplied</c> C# event. A bus
    /// event is used (rather than the per-container C# event) so the tutorial can
    /// observe "first status applied to ANYONE" from a single subscription instead
    /// of subscribing to every musician's and audience member's container.
    ///
    /// Semantic payload only. Tutorial gate — tut_first_status_applied: first fire.
    /// </summary>
    public readonly struct StatusAppliedEvent : ISensoryEvent
    {
        /// <summary>The container the status was applied to (musician or audience).
        /// StatusEffectContainer is a plain sealed class, not a MonoBehaviour, so
        /// consumers needing the owner read it via the container's own API.</summary>
        public StatusEffectContainer Source { get; }

        /// <summary>Primitive status id that changed.</summary>
        public CharacterStatusId Status { get; }

        /// <summary>Stacks delta applied this call.</summary>
        public int DeltaStacks { get; }

        public StatusAppliedEvent(
            StatusEffectContainer source,
            CharacterStatusId status,
            int deltaStacks)
        {
            Source = source;
            Status = status;
            DeltaStacks = deltaStacks;
        }
    }
}

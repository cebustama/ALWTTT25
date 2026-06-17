using System;

namespace ALWTTT.Status.Runtime
{
    /// <summary>
    /// Runtime state for a status on a specific character.
    /// Holds stacks + remaining duration (if used).
    /// </summary>
    [Serializable]
    public sealed class StatusEffectInstance
    {
        public CharacterStatusId Id { get; }
        public StatusEffectSO Definition { get; }

        public int Stacks { get; private set; }
        public int RemainingTurns { get; private set; } // Used when DecayMode.DurationTurns

        public bool IsActive => Stacks > 0;

        public StatusEffectInstance(StatusEffectSO definition, int initialStacks)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Id = definition.EffectId;

            Stacks = Math.Max(0, initialStacks);

            // Initialize duration if relevant
            RemainingTurns = definition.Decay == DecayMode.DurationTurns
                ? Math.Max(0, definition.DurationTurns)
                : 0;
        }

        public void SetStacks(int stacks) => Stacks = Math.Max(0, stacks);

        public void AddStacks(int delta)
        {
            if (delta == 0) return;
            Stacks = Math.Max(0, Stacks + delta);
        }

        public void RefreshDuration()
        {
            if (Definition.Decay != DecayMode.DurationTurns) return;
            RemainingTurns = Math.Max(0, Definition.DurationTurns);
        }

        public void TickDurationDown()
        {
            if (Definition.Decay != DecayMode.DurationTurns) return;
            if (RemainingTurns > 0) RemainingTurns--;
        }
    }
}

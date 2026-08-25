using ALWTTT.Sensory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Status.Runtime
{
    /// <summary>
    /// Runtime container that stores only ACTIVE statuses for a character.
    /// Data-driven behavior: stacking/decay/tick timing comes from StatusEffectSO.
    /// </summary>
    public sealed class StatusEffectContainer
    {
        private readonly Dictionary<CharacterStatusId, StatusEffectInstance> _active =
            new Dictionary<CharacterStatusId, StatusEffectInstance>(32);

        public IReadOnlyDictionary<CharacterStatusId, StatusEffectInstance> Active => _active;

        /// <summary>[TUT-R2] The character that owns this container (set once by
        /// CharacterBase at construction). Typed as object to keep this runtime
        /// class free of a Characters dependency; consumers pattern-match
        /// (MusicianBase / AudienceCharacterBase).</summary>
        public object Owner { get; private set; }

        /// <summary>Called only by CharacterBase immediately after construction.</summary>
        public void SetOwner(object owner) => Owner = owner;

        public event Action<CharacterStatusId, int> OnStatusChanged; // (id, newStacks)
        public event Action<CharacterStatusId> OnStatusCleared;      // (id)
        public event Action<CharacterStatusId, int> OnStatusApplied; // (id, deltaStacks)

        public bool HasActive(CharacterStatusId id)
            => _active.TryGetValue(id, out var inst) && inst != null && inst.IsActive;

        public int GetStacks(CharacterStatusId id)
            => _active.TryGetValue(id, out var inst) && inst != null ? inst.Stacks : 0;

        public bool TryGet(CharacterStatusId id, out StatusEffectInstance instance)
            => _active.TryGetValue(id, out instance);

        public void Clear(CharacterStatusId id)
        {
            if (!_active.ContainsKey(id)) return;
            _active.Remove(id);
            OnStatusCleared?.Invoke(id);
        }

        public void ClearAll()
        {
            // Copy keys to avoid modifying while iterating
            var keys = new List<CharacterStatusId>(_active.Keys);
            for (int i = 0; i < keys.Count; i++) Clear(keys[i]);
        }

        /// <summary>
        /// Apply stacks of a StatusEffectSO to this character.
        /// </summary>
        public void Apply(StatusEffectSO effect, int stacks)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));
            if (stacks == 0) return;

            var id = effect.EffectId;

            if (!_active.TryGetValue(id, out var inst) || inst == null)
            {
                inst = new StatusEffectInstance(effect, 0);
                _active[id] = inst;
            }

            int before = inst.Stacks;
            ApplyStackingPolicy(inst, stacks);
            ApplyDurationPolicy(inst);

            if (inst.Stacks <= 0)
            {
                Clear(id);
                return;
            }

            if (before != inst.Stacks)
                OnStatusChanged?.Invoke(id, inst.Stacks);

            OnStatusApplied?.Invoke(id, stacks);

            // [S4 D-S4-SRC=A] Global bus mirror so the tutorial observes "first status
            // applied to anyone" from one subscription instead of every container.
            SensoryEventBus.Instance?.Publish(new StatusAppliedEvent(this, id, stacks));
        }

        /// <summary>
        /// Called by the combat loop at specific timing boundaries.
        /// Applies decay/tick rules to all active statuses.
        /// </summary>
        public void Tick(TickTiming timing)
        {
            if (_active.Count == 0) return;

            // Copy ids to safely mutate while iterating
            var ids = new List<CharacterStatusId>(_active.Keys);

            for (int i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                if (!_active.TryGetValue(id, out var inst) || inst == null) continue;

                var def = inst.Definition;
                if (def == null)
                {
                    Clear(id);
                    continue;
                }

                // Only tick statuses that match this timing (if defined).
                if (def.Tick != TickTiming.None && def.Tick != timing)
                    continue;

                int before = inst.Stacks;

                switch (def.Decay)
                {
                    case DecayMode.None:
                        break;

                    case DecayMode.LinearStacks:
                        // “Decrease stacks by 1 at tick boundary”
                        inst.AddStacks(-1);
                        break;

                    case DecayMode.DurationTurns:
                        inst.TickDurationDown();
                        if (inst.RemainingTurns <= 0)
                            inst.SetStacks(0);
                        break;

                    case DecayMode.ConsumeOnTrigger:
                        // Does NOT auto-consume on Tick().
                        // Must be consumed by explicit ConsumeOnTrigger() call.
                        break;
                }

                if (inst.Stacks <= 0)
                {
                    Clear(id);
                    continue;
                }

                if (before != inst.Stacks)
                    OnStatusChanged?.Invoke(id, inst.Stacks);
            }
        }

        /// <summary>
        /// Explicit consumption for one-shot statuses (e.g., NegateNextHit).
        /// Call this from the relevant gameplay hook (OnHit/OnTakeDamage/etc.).
        /// </summary>
        public void ConsumeOnTrigger(CharacterStatusId id, int consumeStacks = 1)
        {
            if (consumeStacks <= 0) return;
            if (!_active.TryGetValue(id, out var inst) || inst == null) return;

            if (inst.Definition == null || inst.Definition.Decay != DecayMode.ConsumeOnTrigger)
                return;

            int before = inst.Stacks;
            inst.AddStacks(-consumeStacks);

            if (inst.Stacks <= 0)
            {
                Clear(id);
                return;
            }

            if (before != inst.Stacks)
                OnStatusChanged?.Invoke(id, inst.Stacks);
        }

        /// <summary>
        /// [R5-c / D-R5-18=C] Explicit resource spend for counter-style statuses.
        /// Unlike <see cref="ConsumeOnTrigger"/>, this does NOT require
        /// <see cref="DecayMode.ConsumeOnTrigger"/> — it is the spend path for
        /// resources that never decay on their own (Voltage: DecayMode.None).
        /// Spends min(stacks held, requested) and returns what was actually spent.
        /// Fires OnStatusChanged / OnStatusCleared so icons stay in sync, and
        /// deliberately does NOT fire OnStatusApplied nor publish
        /// StatusAppliedEvent: spending a resource is not applying a status.
        /// </summary>
        public int SpendStacks(CharacterStatusId id, int stacks)
        {
            if (stacks <= 0) return 0;
            if (!_active.TryGetValue(id, out var inst) || inst == null) return 0;

            int held = inst.Stacks;
            if (held <= 0) return 0;

            int spent = Math.Min(held, stacks);
            inst.AddStacks(-spent);

            if (inst.Stacks <= 0)
            {
                Clear(id);   // raises OnStatusCleared
                return spent;
            }

            OnStatusChanged?.Invoke(id, inst.Stacks);
            return spent;
        }

        private static void ApplyStackingPolicy(StatusEffectInstance inst, int deltaStacks)
        {
            var def = inst.Definition;
            int max = Math.Max(1, def.MaxStacks);

            switch (def.Stacking)
            {
                case StackMode.Additive:
                    inst.AddStacks(deltaStacks);
                    break;

                case StackMode.AdditiveClamped:
                    inst.AddStacks(deltaStacks);
                    inst.SetStacks(Math.Min(inst.Stacks, max));
                    break;

                case StackMode.Replace:
                    inst.SetStacks(Math.Clamp(deltaStacks, 0, max));
                    break;

                case StackMode.RefreshDuration:
                    inst.AddStacks(deltaStacks);
                    inst.SetStacks(Math.Min(inst.Stacks, max));
                    inst.RefreshDuration();
                    break;

                default:
                    inst.AddStacks(deltaStacks);
                    break;
            }

            // Always clamp upper bound (even for Additive) if author set maxStacks intentionally.
            if (inst.Stacks > max)
                inst.SetStacks(max);
        }

        private static void ApplyDurationPolicy(StatusEffectInstance inst)
        {
            // If decay is duration-based and duration is set, ensure it’s initialized.
            if (inst.Definition.Decay == DecayMode.DurationTurns && inst.RemainingTurns <= 0)
                inst.RefreshDuration();
        }
    }
}

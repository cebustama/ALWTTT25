using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Status;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Characters
{
    /// <summary>
    /// [LEGIB-1b / D5=A] What each pending audience ability will do to the band on the
    /// coming audience turn, if every member still acts. One per audience member.
    /// </summary>
    public sealed class AudienceIntentSummary
    {
        /// <summary>Musicians the ability touches (damage or not, D10=A), in band order.</summary>
        public readonly List<MusicianBase> Targets = new();

        /// <summary>Stress per target after the ATTACKER's modifiers (Hyped), before the
        /// receiver's (Composure, Exposed): the receiver side lives on each musician's bar.</summary>
        public int Amount;

        /// <summary>False when the ability deals no Stress or hides its value (D2=A).</summary>
        public bool ShowAmount;

        internal void Clear()
        {
            Targets.Clear();
            Amount = 0;
            ShowAmount = false;
        }
    }

    /// <summary>[LEGIB-1b] Result container, reused frame to frame (no per-frame garbage).</summary>
    public sealed class StressProjection
    {
        /// <summary>Fortitude each musician is projected to lose (current − projected final).</summary>
        public readonly Dictionary<MusicianBase, int> IncomingByMusician = new();

        public readonly Dictionary<AudienceCharacterBase, AudienceIntentSummary> ByAudience = new();

        internal AudienceIntentSummary SummaryFor(AudienceCharacterBase a)
        {
            if (!ByAudience.TryGetValue(a, out var s))
            {
                s = new AudienceIntentSummary();
                ByAudience[a] = s;
            }
            s.Clear();
            return s;
        }
    }

    /// <summary>
    /// [LEGIB-1b / D5=A] Projects the band's Stress for the coming audience turn.
    ///
    /// Principle (from S3): previsto = real. This is a dry run of the SAME rules, in the
    /// SAME order, on copied numbers — nothing here touches live state, RNG or the bus:
    ///   1. AudienceTurnRoutine: Feedback DoT on each musician (through Composure/Exposed).
    ///   2. Each audience member in turn order (CurrentAudienceCharacterList), skipping the
    ///      convinced, the inactive and the stunned, runs its pending ability action by action:
    ///        AddStress   → BandCharacterStats.ApplyOutgoingStressWithModifiers (Hyped +1/stack)
    ///                    → ApplyIncomingStressWithComposure (Composure absorbs, then Exposed
    ///                      multiplies what is left, rounded up).
    ///        ApplyStatus → Exposed / Composure on musicians and Hyped on audience update the
    ///                      copies with the container's stacking policy, so Kid's "Egg Him On"
    ///                      before Cool Dude's "Heckle" raises Heckle exactly as the real turn would.
    ///
    /// Targets come from AudienceCharacterBase.GetProjectedMusicianTargets, which the real
    /// turn also uses (locked at intention time, Spotlight applied on top). If a rule here
    /// and in AddStressAction / BandCharacterStats ever disagree, the bar lies: change both.
    ///
    /// Not projected on purpose: the song itself. A member convinced at song end skips its
    /// turn, so the bar shows "what happens if you do not convince them".
    /// </summary>
    public static class StressProjector
    {
        private sealed class Sim
        {
            public int Stress;
            public int Composure;
            public int Exposed;
            public float ExposedMult;
        }

        private static readonly Dictionary<MusicianBase, Sim> _band = new();
        private static readonly Dictionary<AudienceCharacterBase, int> _hyped = new();
        private static readonly List<AudienceCharacterBase> _audienceTargets = new();

        public static void Build(
            IReadOnlyList<MusicianBase> band,
            IReadOnlyList<AudienceCharacterBase> audience,
            StressProjection result)
        {
            result.IncomingByMusician.Clear();
            _band.Clear();
            _hyped.Clear();

            if (band == null) return;

            foreach (var m in band)
            {
                if (m == null || m.Stats == null) continue;
                var sim = new Sim
                {
                    Stress = m.Stats.CurrentStress,
                    ExposedMult = m.Stats.ExposedStressMultiplierPerStack,
                };
                sim.Composure = ReadStacks(m.Statuses, CharacterStatusId.TempShieldTurn);
                sim.Exposed = ReadStacks(m.Statuses, CharacterStatusId.DamageTakenUpFlat);
                _band[m] = sim;
            }

            // 1. Feedback DoT at AudienceTurn start (GigManager.AudienceTurnRoutine).
            foreach (var kv in _band)
            {
                int feedback = kv.Key.Statuses != null
                    ? kv.Key.Statuses.GetStacks(CharacterStatusId.DamageOverTime)
                    : 0;
                if (feedback > 0) Hit(kv.Value, feedback);
            }

            // 2. Audience in turn order.
            if (audience != null)
            {
                foreach (var a in audience)
                {
                    if (a == null) continue;
                    _hyped[a] = ReadHyped(a.Statuses);
                }

                foreach (var a in audience)
                {
                    if (a == null) continue;
                    var summary = result.SummaryFor(a);

                    if (!a.gameObject.activeInHierarchy) continue;
                    if (a.Stats == null || a.Stats.IsConvinced) continue;
                    if (a.Stats.IsStunned) continue; // ConsumeStun skips the whole ability

                    var ability = a.PendingAbility;
                    if (ability == null || ability.ActionList == null) continue;

                    bool anyStress = false;

                    foreach (var action in ability.ActionList)
                    {
                        if (action == null) continue;

                        var targets = a.GetProjectedMusicianTargets(action);
                        foreach (var t in targets)
                            if (t != null && !summary.Targets.Contains(t)) summary.Targets.Add(t);

                        switch (action.CardActionType)
                        {
                            case CharacterActionType.AddStress:
                                {
                                    // [S7 / Croar] Per target: a duet doubles the base for the
                                    // musician both singers hit (AudienceCharacterBase.FindDuetPartner,
                                    // the same call the real hit makes), then Hyped is added.
                                    int amountShown = 0;
                                    foreach (var t in targets)
                                    {
                                        if (t == null) continue;
                                        int baseStress = Mathf.RoundToInt(action.ActionValue * a.DuetMultiplier(action, t));
                                        // Mirror of ApplyOutgoingStressWithModifiers: only a positive
                                        // base is modified, Hyped adds +1 per stack.
                                        int outgoing = baseStress;
                                        if (baseStress > 0) outgoing += Mathf.Max(0, _hyped[a]);
                                        amountShown = Mathf.Max(amountShown, outgoing);
                                        if (_band.TryGetValue(t, out var sim)) Hit(sim, outgoing);
                                    }

                                    if (targets.Count > 0)
                                    {
                                        anyStress = true;
                                        summary.Amount += Mathf.Max(0, amountShown);
                                    }
                                    break;
                                }

                            case CharacterActionType.ApplyStatusEffect:
                                {
                                    var so = action.StatusEffect;
                                    int delta = Mathf.RoundToInt(action.ActionValue);
                                    if (so == null || delta == 0) break;

                                    if (so.EffectId == CharacterStatusId.DamageTakenUpFlat)
                                    {
                                        foreach (var t in targets)
                                            if (t != null && _band.TryGetValue(t, out var sim))
                                                sim.Exposed = Stack(so, sim.Exposed, delta);
                                    }
                                    else if (so.EffectId == CharacterStatusId.TempShieldTurn)
                                    {
                                        foreach (var t in targets)
                                            if (t != null && _band.TryGetValue(t, out var sim))
                                                sim.Composure = Stack(so, sim.Composure, delta);
                                    }
                                    else if (so.EffectId == CharacterStatusId.DamageUpFlat &&
                                             IsHyped(so))
                                    {
                                        a.GetProjectedAudienceTargets(action, _audienceTargets);
                                        foreach (var t in _audienceTargets)
                                            if (t != null && _hyped.ContainsKey(t))
                                                _hyped[t] = Stack(so, _hyped[t], delta);
                                    }
                                    break;
                                }
                        }
                    }

                    summary.ShowAmount = anyStress && !ability.HideActionValue && summary.Amount > 0;

                    // Keep the icons in band order whatever order the actions listed them.
                    summary.Targets.Sort((x, y) => IndexIn(band, x).CompareTo(IndexIn(band, y)));
                }
            }

            foreach (var kv in _band)
            {
                int loss = Mathf.Max(0, kv.Key.Stats.CurrentStress - kv.Value.Stress);
                result.IncomingByMusician[kv.Key] = loss;
            }
        }

        /// <summary>Dry-run mirror of BandCharacterStats.ApplyIncomingStressWithComposure.</summary>
        private static void Hit(Sim sim, int incoming)
        {
            if (incoming <= 0) return;

            int remaining = incoming;
            if (sim.Composure > 0)
            {
                int absorbed = Mathf.Min(sim.Composure, remaining);
                sim.Composure -= absorbed;
                remaining -= absorbed;
            }

            if (sim.Exposed > 0)
                remaining = Mathf.CeilToInt(remaining * (1f + sim.Exposed * sim.ExposedMult));

            if (remaining > 0)
                sim.Stress = Mathf.Max(0, sim.Stress - remaining);
        }

        /// <summary>Mirror of StatusEffectContainer.ApplyStackingPolicy on a plain int.</summary>
        private static int Stack(StatusEffectSO def, int current, int delta)
        {
            int max = Math.Max(1, def.MaxStacks);
            int next;
            switch (def.Stacking)
            {
                case StackMode.Replace:
                    next = Math.Clamp(delta, 0, max);
                    break;
                case StackMode.AdditiveClamped:
                case StackMode.RefreshDuration:
                    next = Math.Min(current + delta, max);
                    break;
                default:
                    next = current + delta;
                    break;
            }
            return Math.Max(0, next);
        }

        private static int ReadStacks(
            Status.Runtime.StatusEffectContainer statuses, CharacterStatusId id)
        {
            if (statuses == null) return 0;
            return statuses.TryGet(id, out var inst) && inst != null ? Mathf.Max(0, inst.Stacks) : 0;
        }

        /// <summary>Same guard as ApplyOutgoingStressWithModifiers: DamageUpFlat keyed "hyped".</summary>
        private static int ReadHyped(Status.Runtime.StatusEffectContainer statuses)
        {
            if (statuses == null) return 0;
            if (statuses.TryGet(CharacterStatusId.DamageUpFlat, out var inst) &&
                inst != null && inst.Stacks > 0 && IsHyped(inst.Definition))
                return inst.Stacks;
            return 0;
        }

        private static bool IsHyped(StatusEffectSO so) =>
            so != null && string.Equals(so.StatusKey, "hyped", StringComparison.OrdinalIgnoreCase);

        private static int IndexIn(IReadOnlyList<MusicianBase> band, MusicianBase m)
        {
            for (int i = 0; i < band.Count; i++) if (band[i] == m) return i;
            return int.MaxValue;
        }
    }
}

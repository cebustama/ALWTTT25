using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Status;
using ALWTTT.Status.Runtime;
using System;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    /// <summary>
    /// [S5e / D1] INVERTED METER SEMANTICS.
    /// CurrentStress is now the musician's remaining MENTAL FORTITUDE
    /// (an HP-style pool): it starts at MaxStress and is DEPLETED by
    /// incoming Stress. Breakdown fires at CurrentStress == 0
    /// (mirror of the BandCohesion deplete-to-0 pattern in GigManager).
    ///
    /// API contract is magnitude-preserving and direction-agnostic for
    /// callers: AddStress(n) still means "take n incoming Stress",
    /// HealStress(n) still means "recover n". Only the internal storage
    /// direction and the threshold boundary changed. Field names
    /// (CurrentStress/MaxStress) are retained to avoid a rename cascade
    /// across the API surface (D-S5e-2); a rename can be revisited at S5i+.
    /// </summary>
    public class BandCharacterStats : CharacterStats, IMusicianStats
    {
        public int CurrentStress { get; set; }
        public int MaxStress { get; set; }

        public int Charm { get; set; }
        public int Technique { get; set; }
        public int Emotion { get; set; }

        public bool IsBreakdown { get; private set; }

        public Action OnBreakdown;
        public Action<int, int> OnStressChanged;

        private BandCharacterCanvas bandCharacterCanvas;

        private float _exposedMultiplierPerStack = 0.25f;
        public float ExposedStressMultiplierPerStack
        {
            get => _exposedMultiplierPerStack;
            set => _exposedMultiplierPerStack = value;
        }

        // ─── [ECON-1 / D-ECON-2=A] Per-turn play budget ──────────────────
        // One Action + one Composition play per musician per PERIOD
        // (pre-song PlayerTurn window, and each performance loop).
        // Maxima are runtime-only, seeded from GigFlowSettingsSO defaults at
        // gig setup (D-ECON-5=A); no per-musician authoring field yet.
        // Orthogonal to the Inspiration cost gate (2a.5) — this budget never
        // replaces or masks it.

        public int MaxActionPlays { get; private set; }
        public int MaxCompositionPlays { get; private set; }
        public int ActionPlaysRemaining { get; private set; }
        public int CompositionPlaysRemaining { get; private set; }

        /// <summary>(actionRemaining, compositionRemaining). Fired on
        /// Init / Reset / successful consume — mirrors the OnStressChanged →
        /// canvas push pattern.</summary>
        public Action<int, int> OnTurnPlayBudgetChanged;

        /// <summary>Seed runtime maxima and fill the pools. Call once at gig
        /// setup (GigManager.BuildBand) with the GigFlowSettingsSO defaults.</summary>
        public void InitTurnPlayBudget(int maxActionPlays, int maxCompositionPlays)
        {
            MaxActionPlays = Mathf.Max(0, maxActionPlays);
            MaxCompositionPlays = Mathf.Max(0, maxCompositionPlays);
            ResetTurnPlayBudget();
        }

        /// <summary>Refill both pools to their maxima. Idempotent — double
        /// resets at overlapping seams are harmless by design.</summary>
        public void ResetTurnPlayBudget()
        {
            ActionPlaysRemaining = MaxActionPlays;
            CompositionPlaysRemaining = MaxCompositionPlays;
            NotifyTurnPlayBudgetChanged();
        }

        public bool CanConsumePlay(bool isComposition) =>
            isComposition ? CompositionPlaysRemaining > 0 : ActionPlaysRemaining > 0;

        /// <summary>Consume one play of the given kind. Returns false (and
        /// mutates nothing) when the pool is empty. Callers must invoke this
        /// only once nothing else in the play pipeline can fail — budget burns
        /// exclusively on successful plays (batch constraint).</summary>
        public bool TryConsumePlay(bool isComposition)
        {
            if (!CanConsumePlay(isComposition)) return false;

            if (isComposition) CompositionPlaysRemaining--;
            else ActionPlaysRemaining--;

            NotifyTurnPlayBudgetChanged();
            return true;
        }

        private void NotifyTurnPlayBudgetChanged()
        {
            OnTurnPlayBudgetChanged?.Invoke(
                ActionPlaysRemaining, CompositionPlaysRemaining);
        }

        public override string ToString()
        {
            return $"[Musician Stats] Fortitude(Stress): {CurrentStress}/{MaxStress}, " +
               $"CHR: {Charm}, THC: {Technique}, EMT: {Emotion}";
        }

        #region Setup
        public BandCharacterStats(int chr, int tch, int emt,
            int maxStress, BandCharacterCanvas characterCanvas)
        {
            Charm = chr;
            Technique = tch;
            Emotion = emt;

            bandCharacterCanvas = characterCanvas;
            Setup(characterCanvas, maxStress);
        }

        protected override void Setup(CharacterCanvas canvas, int maxHp)
        {
            base.Setup(canvas, maxHp);

            MaxStress = maxHp;
            // [S5e] Inverted meter: start at full fortitude, deplete toward 0.
            CurrentStress = MaxStress;

            OnStressChanged += bandCharacterCanvas.UpdateHealthText;

            // [ECON-1] Budget → pips push. Wired here (not in MusicianBase)
            // to mirror the OnStressChanged subscription above exactly.
            OnTurnPlayBudgetChanged += bandCharacterCanvas.UpdateTurnPlayBudget;
        }
        #endregion

        public override void Dispose()
        {
            base.Dispose();
            if (bandCharacterCanvas != null)
            {
                OnStressChanged -= bandCharacterCanvas.UpdateHealthText;

                OnTurnPlayBudgetChanged -= bandCharacterCanvas.UpdateTurnPlayBudget;
            }
        }

        public void SetCurrentStress(int targetCurrentStress, float duration = 1f)
        {
            CurrentStress =
                targetCurrentStress < 0 ? 0 :
                    targetCurrentStress > MaxStress ?
                        MaxStress :
                        targetCurrentStress;

            bandCharacterCanvas.SetCurrentStress(CurrentStress, MaxStress, duration);
            bandCharacterCanvas.UpdateVisibility();

            OnStressChanged?.Invoke(CurrentStress, MaxStress);
        }

        /// <summary>
        /// [S5e] Take <paramref name="amount"/> incoming Stress: DEPLETES the
        /// fortitude pool. Magnitude semantics unchanged for callers.
        /// </summary>
        public void AddStress(int amount, float duration = 1f)
        {
            SetCurrentStress(CurrentStress - amount, duration);
            CheckBreakdownThreshold();
        }

        /// <summary>
        /// Shared Breakdown-threshold check. Called from every path that sets
        /// CurrentStress (play: AddStress; dev: DevSetCurrentStress/DevSetMaxStress).
        /// Preserves the !IsBreakdown guard — Breakdown is a sticky state transition.
        /// [S5e] Fires when the fortitude pool is EMPTY (deplete-to-0-collapses,
        /// mirror of the BandCohesion == 0 → LoseGig pattern in GigManager).
        /// </summary>
        private void CheckBreakdownThreshold()
        {
            if (CurrentStress <= 0 && !IsBreakdown)
            {
                IsBreakdown = true;
                OnBreakdown?.Invoke();
            }
        }

        /// <summary>
        /// [S5e] Recover <paramref name="amount"/> fortitude (heal). Clamped to
        /// MaxStress inside SetCurrentStress.
        /// </summary>
        public void HealStress(int amount, float duration = 1f)
        {
            SetCurrentStress(CurrentStress + amount, duration);
        }

        /// <summary>
        /// Legacy status application via StatusType enum.
        /// M1.2: No longer drives icon display. Icons are now event-driven from
        /// StatusEffectContainer. This method is retained for any remaining legacy
        /// callers but should be phased out. New code should use
        /// CharacterBase.Statuses.Apply(StatusEffectSO, stacks) instead.
        /// </summary>
        [Obsolete("Use CharacterBase.Statuses.Apply(StatusEffectSO, stacks) instead. Legacy StatusType path.")]
        public void ApplyStatus(StatusType targetStatus, int value)
        {
            if (statusDict[targetStatus].IsActive)
            {
                statusDict[targetStatus].StatusValue += value;
                OnStatusChanged?.Invoke(targetStatus, statusDict[targetStatus].StatusValue);
            }
            else
            {
                statusDict[targetStatus].StatusValue = value;
                statusDict[targetStatus].IsActive = true;
                OnStatusApplied?.Invoke(targetStatus, statusDict[targetStatus].StatusValue);
            }
        }

        protected override void DamagePoison()
        {
            throw new NotImplementedException();
        }

        protected override void CheckStunStatus()
        {
            throw new NotImplementedException();
        }

        protected override void TriggerStatus(StatusType targetStatus)
        {

        }

        /// <summary>
        /// Single canonical entry point for incoming positive Stress.
        /// 1. Absorbs Composure (TempShieldTurn) from the SO-based StatusEffectContainer.
        /// 2. Applies remainder via AddStress (which depletes fortitude and
        ///    triggers the Breakdown check at 0). [S5e]
        /// Call from card effects AND audience actions.
        /// </summary>
        public (int absorbed, int applied) ApplyIncomingStressWithComposure(
            StatusEffectContainer statuses,
            int incomingStress,
            float duration = 1f)
        {
            if (incomingStress <= 0)
                return (0, 0);

            int remaining = incomingStress;
            int absorbed = 0;

            // Step 1: Composure absorption (SO-based container)
            if (statuses != null &&
                statuses.TryGet(CharacterStatusId.TempShieldTurn, out var compInst) &&
                compInst != null && compInst.Stacks > 0)
            {
                absorbed = Mathf.Min(compInst.Stacks, remaining);
                if (absorbed > 0)
                {
                    statuses.Apply(compInst.Definition, -absorbed);
                    remaining -= absorbed;
                }
            }

            // Decision E: Exposed amplifies remaining stress
            if (statuses != null &&
                statuses.TryGet(CharacterStatusId.DamageTakenUpFlat, out var exposedInst) &&
                exposedInst != null && exposedInst.Stacks > 0)
            {
                float mult = 1f + (exposedInst.Stacks * _exposedMultiplierPerStack);
                remaining = Mathf.CeilToInt(remaining * mult);
            }

            // Step 2: Apply remainder (triggers Breakdown check via AddStress)
            if (remaining > 0)
            {
                AddStress(remaining, duration);
            }

            // [TUT-R2] Single canonical publish for incoming musician stress —
            // this method is the funnel for ALL producers (AddStressAction,
            // CardBase co-effects, GigManager audience feedback), so one site
            // covers everything (same rationale as StatusAppliedEvent).
            ALWTTT.Sensory.SensoryEventBus.Instance?.Publish(
                new ALWTTT.Sensory.MusicianStressHitEvent(this, absorbed, remaining));

            return (absorbed, remaining);
        }

        /// <summary>
        /// Applies attacker-side outgoing-Stress modifiers to a base Stress amount
        /// BEFORE it reaches the receiver-side incoming pipeline. Returns the
        /// modified amount; no state mutation.
        ///
        /// Static because the receiver instance is irrelevant — modifiers come
        /// from the attacker's container, which may be musician-side or
        /// audience-side. Symmetric in name + location to
        /// <see cref="ApplyIncomingStressWithComposure"/> (the receiver-side
        /// counterpart) without forcing a misleading instance method.
        ///
        /// B3 scope: hardcoded Hyped check via DamageUpFlat + StatusKey == "hyped".
        /// Each Hyped stack adds 1 flat to outgoing Stress. Other DamageUpFlat
        /// statuses (notably musician-side Flow) are intentionally ignored — Flow's
        /// outgoing semantics differ and live in the card pipeline.
        ///
        /// TODO (post-B3): generalize to an SO-driven outgoing-modifier list so
        /// new outgoing-Stress modifiers can ship via authoring alone.
        /// </summary>
        public static int ApplyOutgoingStressWithModifiers(
            StatusEffectContainer attackerStatuses,
            int baseAmount)
        {
            if (baseAmount <= 0) return baseAmount;
            if (attackerStatuses == null) return baseAmount;

            int modified = baseAmount;

            // Hyped (audience-side, B3): each stack adds +1 flat to outgoing Stress.
            if (attackerStatuses.TryGet(CharacterStatusId.DamageUpFlat, out var hypedInst) &&
                hypedInst != null && hypedInst.Stacks > 0 &&
                hypedInst.Definition != null &&
                string.Equals(hypedInst.Definition.StatusKey, "hyped",
                    System.StringComparison.OrdinalIgnoreCase))
            {
                modified += hypedInst.Stacks;
            }

            return modified;
        }

#if ALWTTT_DEV
        /// <summary>
        /// Resets IsBreakdown so a subsequent AddStress depleting the pool to 0
        /// re-triggers the full Breakdown path. Dev Mode only. [S5e]
        /// </summary>
        public void DevResetBreakdown()
        {
            IsBreakdown = false;
        }

        /// <summary>
        /// Dev Mode: Set Stress (fortitude) directly to a clamped target value.
        /// [S5e] Fires Breakdown if the target reaches 0 and the musician is not
        /// yet broken. Skips
        /// animation (duration=0f) for instant dev-UI feedback.
        /// Symmetric-consequences per SSoT_Dev_Mode §13.3.
        /// </summary>
        public void DevSetCurrentStress(int target)
        {
            SetCurrentStress(target, duration: 0.1f);
            CheckBreakdownThreshold();
        }

        /// <summary>
        /// Dev Mode: Set MaxStress to a new value (floor 1). If CurrentStress
        /// exceeds the new max, Current is clamped down. [S5e] Under inverted
        /// semantics, changing MaxStress can no longer trigger Breakdown by
        /// itself (the clamp floor keeps Current ≥ 1); the threshold re-check
        /// is retained as a harmless invariant guard.
        /// </summary>
        public void DevSetMaxStress(int newMax)
        {
            MaxStress = Mathf.Max(1, newMax);
            // SetCurrentStress clamps internally AND refreshes canvas.
            // Passing current value when already ≤ MaxStress is a harmless refresh.
            SetCurrentStress(CurrentStress, duration: 0.1f);
            CheckBreakdownThreshold();
        }
#endif
    }
}
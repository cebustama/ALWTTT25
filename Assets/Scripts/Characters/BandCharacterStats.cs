using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Status;
using ALWTTT.Status.Runtime;
using System;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
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

        public override string ToString()
        {
            return $"[Musician Stats] Stress: {CurrentStress}/{MaxStress}, " +
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
            CurrentStress = 0;

            OnStressChanged += bandCharacterCanvas.UpdateHealthText;
        }
        #endregion

        public override void Dispose()
        {
            base.Dispose();
            if (bandCharacterCanvas != null)
            {
                OnStressChanged -= bandCharacterCanvas.UpdateHealthText;
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

        public void AddStress(int amount, float duration = 1f)
        {
            SetCurrentStress(CurrentStress + amount, duration);
            CheckBreakdownThreshold();
        }

        /// <summary>
        /// Shared Breakdown-threshold check. Called from every path that sets
        /// CurrentStress (play: AddStress; dev: DevSetCurrentStress/DevSetMaxStress).
        /// Preserves the !IsBreakdown guard — Breakdown is a sticky state transition.
        /// </summary>
        private void CheckBreakdownThreshold()
        {
            if (CurrentStress >= MaxStress && !IsBreakdown)
            {
                IsBreakdown = true;
                OnBreakdown?.Invoke();
            }
        }

        public void HealStress(int amount, float duration = 1f)
        {
            SetCurrentStress(Mathf.Max(0, CurrentStress - amount), duration);
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
        /// 2. Applies remainder via AddStress (which triggers Breakdown check).
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

            return (absorbed, remaining);
        }

#if ALWTTT_DEV
        /// <summary>
        /// Resets IsBreakdown so a subsequent AddStress reaching MaxStress
        /// re-triggers the full Breakdown path. Dev Mode only.
        /// </summary>
        public void DevResetBreakdown()
        {
            IsBreakdown = false;
        }

        /// <summary>
        /// Dev Mode: Set Stress directly to a clamped target value. Fires Breakdown
        /// if the target reaches MaxStress and musician is not yet broken. Skips
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
        /// exceeds the new max, Current is clamped down. Re-checks Breakdown
        /// threshold — reducing MaxStress to current's value triggers Breakdown.
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
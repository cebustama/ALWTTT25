using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Characters.Audience
{
    public class AudienceCharacterStats : CharacterStats, IAudienceStats
    {
        public int MaxVibe { get; set; } // "HP"
        public int CurrentVibe { get; private set; }
        public bool IsConvinced { get; private set; } // "Death"
        public bool IsStunned { get; private set; }

        public Action OnConvinced;
        public Action<int, int> OnVibeChanged;

        public Dictionary<StatusType, StatusStats> statusDict => base.statusDict;

        public override string ToString()
        {
            return $"[Audience Stats] Vibe: {CurrentVibe}/{MaxVibe}, " +
                $"IsConvinced: {IsConvinced}";
        }

        #region Setup
        public AudienceCharacterStats(int maxVibe, CharacterCanvas canvas)
        {
            Setup(canvas, maxVibe);
        }

        protected override void Setup(CharacterCanvas canvas, int maxHp)
        {
            base.Setup(canvas, maxHp);

            MaxVibe = maxHp;
            CurrentVibe = 0;

            OnVibeChanged += characterCanvas.UpdateHealthText;
        }
        #endregion

        #region Public Methods
        public override void Dispose()
        {
            base.Dispose();

            if (characterCanvas != null)
            {
                OnVibeChanged -= characterCanvas.UpdateHealthText;
            }
        }

        public void SetCurrentVibe(int targetCurrentVibe, float duration = 2f)
        {
            CurrentVibe =
                targetCurrentVibe < 0 ? 0 :
                    targetCurrentVibe > MaxVibe ?
                        MaxVibe :
                        targetCurrentVibe;

            characterCanvas.SetCurrentVibe(targetCurrentVibe, MaxVibe, duration);
            characterCanvas.UpdateVisibility();

            OnVibeChanged?.Invoke(CurrentVibe, MaxVibe);
        }

        public void AddVibe(int amount, float duration = 2f)
        {
            SetCurrentVibe(CurrentVibe + amount, duration);
            CheckConvincedThreshold();
        }

        /// <summary>
        /// Shared Convinced-threshold check. Called from every path that sets
        /// CurrentVibe (play: AddVibe; dev: DevSetCurrentVibe/DevSetMaxVibe).
        /// Preserves the !IsConvinced guard — Convinced is a sticky state transition.
        /// </summary>
        private void CheckConvincedThreshold()
        {
            if (CurrentVibe >= MaxVibe && !IsConvinced)
            {
                IsConvinced = true;

                ApplyStatus(StatusType.Convinced, 1);
                ClearStatus(StatusType.Tall);

                OnConvinced?.Invoke();
            }
        }

        public void RemoveVibe(int amount, float duration = 2f)
        {
            SetCurrentVibe(CurrentVibe - amount, duration);
        }

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
            var s = statusDict[StatusType.Breakdown]; // TODO: Audience specific name?
            if (!s.IsActive || s.StatusValue <= 0) return;

            IsStunned = true;
        }

        public bool ConsumeStun()
        {
            if (!IsStunned) return false;
            IsStunned = false;
            return true;
        }

        protected override void TriggerStatus(StatusType targetStatus)
        {
            base.TriggerStatus(targetStatus);
        }

#if ALWTTT_DEV
        /// <summary>
        /// Resets IsConvinced and clears the Convinced status entry so a subsequent
        /// AddVibe reaching MaxVibe re-triggers the full Convinced path. Used by
        /// Infinite-Turns mode (DevModeController.ResetConvincedAudience).
        /// Does NOT restore Tall — that state is not recoverable here; apply via
        /// status picker if needed.
        /// </summary>
        public void DevResetConvinced()
        {
            IsConvinced = false;
            ClearStatus(StatusType.Convinced);
        }

        /// <summary>
        /// Dev Mode: Set Vibe directly to a clamped target value. Fires Convinced
        /// if the target reaches MaxVibe and audience is not yet convinced.
        /// Skips animation (duration=0f) for instant dev-UI feedback.
        /// Symmetric-consequences per SSoT_Dev_Mode §13.3.
        /// </summary>
        public void DevSetCurrentVibe(int target)
        {
            SetCurrentVibe(target, duration: 0.1f);
            CheckConvincedThreshold();
        }

        /// <summary>
        /// Dev Mode: Set MaxVibe to a new value (floor 1). If CurrentVibe
        /// exceeds the new max, Current is clamped down. Re-checks Convinced
        /// threshold — reducing MaxVibe to current's value triggers Convinced.
        /// </summary>
        public void DevSetMaxVibe(int newMax)
        {
            MaxVibe = Mathf.Max(1, newMax);
            // SetCurrentVibe clamps internally AND refreshes canvas.
            SetCurrentVibe(CurrentVibe, duration: 0.1f);
            CheckConvincedThreshold();
        }
#endif

        #endregion
    }
}
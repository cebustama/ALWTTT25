using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Status;
using ALWTTT.Status.Runtime;
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
        /// Single canonical entry point for incoming POSITIVE Vibe.
        /// Mirror of BandCharacterStats.ApplyIncomingStressWithComposure (M4.1 pattern).
        ///
        /// Currently registered statuses:
        /// - Indifference (CharacterStatusId.NegateIncomingPositive): while stacks > 0,
        ///   blocks 100% of incoming Vibe. Stack decay happens via the container's Tick
        ///   per its DecayMode/TickTiming config, NOT per-application — a block doesn't
        ///   reduce future blocking.
        ///
        /// Call from card effects (ModifyVibeSpec positive), audience actions
        /// (AddVibeAction), DoT ticks (Earworm), and song-end macro Vibe.
        ///
        /// Returns the amount actually applied. Callers use this to drive floating text:
        /// applied &gt; 0 → normal feedback; (applied == 0 &amp;&amp; incoming &gt; 0) → blocked.
        ///
        /// Negative-Vibe paths (RemoveVibe / negative ModifyVibeSpec) DO NOT route
        /// through this helper — Indifference does not modulate them in B3. Documented
        /// limitation; revisit when negative-Vibe content lands.
        ///
        /// Dev paths (DevSetCurrentVibe) bypass this helper by calling SetCurrentVibe
        /// directly, per B3 D8=B (escape hatch for testing).
        /// </summary>
        public int ApplyIncomingVibe(
            StatusEffectContainer statuses,
            int incoming,
            float duration = 2f)
        {
            if (incoming <= 0) return 0;

            // Indifference block: stacks > 0 → gate all incoming Vibe to 0.
            int indiffStacks = 0;
            if (statuses != null)
            {
                indiffStacks = statuses.GetStacks(
                    CharacterStatusId.NegateIncomingPositive);
            }

            if (indiffStacks > 0)
            {
#if UNITY_EDITOR
                Debug.Log(
                    $"<color=#888888>[ApplyIncomingVibe] BLOCKED " +
                    $"incoming={incoming} indiffStacks={indiffStacks}</color>");
#endif
                return 0;
            }

            AddVibe(incoming, duration);
            return incoming;
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

        /// <summary>
        /// Shared Convinced-threshold check. Called from every path that sets
        /// CurrentVibe and could push the audience over MaxVibe (play: AddVibe;
        /// dev: DevSetCurrentVibe / DevSetMaxVibe).
        /// Preserves the !IsConvinced guard — Convinced is a sticky state
        /// transition; the !IsConvinced gate prevents OnConvinced re-firing.
        /// Mirror of BandCharacterStats.CheckBreakdownThreshold (M4.1 pattern).
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
        /// Dev Mode: Reset Convinced state and clear the legacy Convinced status.
        /// Used by DevModeController.ResetConvincedAudience() in infinite-turns mode.
        /// Symmetric-consequences per SSoT_Dev_Mode §13.3 — Convinced sticky bit
        /// AND the legacy status dict entry both go back to inactive.
        /// </summary>
        public void DevResetConvinced()
        {
            IsConvinced = false;
            ClearStatus(StatusType.Convinced);
        }

        /// <summary>
        /// Dev Mode: Set Vibe directly to a clamped target value. Fires Convinced
        /// if the target reaches MaxVibe and audience is not yet convinced.
        /// Skips animation (duration=0.1f) for instant dev-UI feedback.
        /// Symmetric-consequences per SSoT_Dev_Mode §13.3.
        ///
        /// [B3 D8=B] Bypasses ApplyIncomingVibe. Indifference does NOT gate this
        /// path — dev tools are testing escape hatches.
        /// </summary>
        public void DevSetCurrentVibe(int target)
        {
            SetCurrentVibe(target, duration: 0.1f);
            CheckConvincedThreshold();
        }

        /// <summary>
        /// Dev Mode: Set MaxVibe to a new value (floor 1). If CurrentVibe
        /// exceeds the new max, Current is clamped down via SetCurrentVibe.
        /// Re-checks Convinced threshold — reducing MaxVibe to current's value
        /// triggers Convinced.
        /// </summary>
        public void DevSetMaxVibe(int newMax)
        {
            MaxVibe = Mathf.Max(1, newMax);
            // SetCurrentVibe clamps internally AND refreshes canvas.
            // Passing current value when already ≤ MaxVibe is a harmless refresh.
            SetCurrentVibe(CurrentVibe, duration: 0.1f);
            CheckConvincedThreshold();
        }
#endif

        #endregion
    }
}
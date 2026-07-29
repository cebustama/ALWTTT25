using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.Status;
using ALWTTT.Status.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Characters.Audience
{
    /// <summary>
    /// [S5e / D1] INVERTED METER SEMANTICS.
    /// CurrentVibe is now the audience member's remaining PERSUASION
    /// RESISTANCE (enemy-HP-style pool): it starts at MaxVibe and is
    /// DEPLETED by incoming Vibe. Convinced ("conquered") fires at
    /// CurrentVibe == 0 (deplete-to-0 pattern, mirror of BandCohesion).
    ///
    /// API contract is magnitude-preserving and direction-agnostic for
    /// callers: AddVibe(n) still means "n persuasion lands on this member"
    /// (now depletes), RemoveVibe(n) still means "this member regains n
    /// resistance" (now restores). Negative amounts keep their prior
    /// player-facing meaning through the sign flip (the ModifyVibeSpec
    /// negative path relies on this). Field names retained per D-S5e-2.
    /// </summary>
    public class AudienceCharacterStats : CharacterStats, IAudienceStats
    {
        public int MaxVibe { get; set; } // "HP"
        public int CurrentVibe { get; private set; }
        public bool IsConvinced { get; private set; } // "Death" (pool empty)

        // [R1] Fallback for CaptivatedVibeBonusPerStack when no GigManager /
        // MeterTuningSO is available (tests, detached construction). The
        // tuned value lives on MeterTuningSO (D-R1-2=A).
        public const float DefaultCaptivatedBonusPerStack = 0.25f;

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
            // [S5e] Inverted meter: start at full resistance, deplete toward 0.
            CurrentVibe = MaxVibe;

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

            // [S5e] Pass the CLAMPED value to the canvas. Under deplete-to-0
            // semantics, overkill damage would otherwise push a negative raw
            // target into the bar animation.
            characterCanvas.SetCurrentVibe(CurrentVibe, MaxVibe, duration);
            characterCanvas.UpdateVisibility();

            OnVibeChanged?.Invoke(CurrentVibe, MaxVibe);
        }

        /// <summary>
        /// [S5e] Apply <paramref name="amount"/> incoming persuasion: DEPLETES
        /// the resistance pool. Negative amounts restore resistance (legacy
        /// negative-ModifyVibeSpec path) — sign meaning preserved through the
        /// inversion. Magnitude semantics unchanged for callers.
        /// </summary>
        public void AddVibe(int amount, float duration = 2f)
        {
            SetCurrentVibe(CurrentVibe - amount, duration);
            CheckConvincedThreshold();
        }

        /// <summary>
        /// Single canonical entry point for incoming POSITIVE Vibe.
        /// Mirror of BandCharacterStats.ApplyIncomingStressWithComposure (M4.1 pattern).
        ///
        /// Currently registered statuses:
        /// - Indifference (CharacterStatusId.NegateIncomingPositive): while stacks > 0,
        ///   blocks 100% of incoming Vibe damage. Stack decay happens via the container's Tick
        ///   per its DecayMode/TickTiming config, NOT per-application — a block doesn't
        ///   reduce future blocking.
        /// - Captivated (CharacterStatusId.DamageTakenUpMultiplier, key "captivated"):
        ///   amplifies incoming Vibe by ×(1 + stacks × CaptivatedVibeBonusPerStack).
        ///   Layered AFTER the Indifference gate; blocked stays 0.
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

            // [R1] Captivated amplification layer (Design_Audience_Status_v1 §4
            // → SSoT_Status_Effects §5.8). Sits AFTER the Indifference gate by
            // design (D-DCP-6=A invariant): blocked is blocked, regardless of
            // Captivated stacks. Applies to ALL positive Vibe routed through
            // this helper — cards, Earworm ticks, SFX FlatVibe, song-end macro
            // Vibe (D-R1-1=A, scope broadened vs the original card-only design
            // wording). StatusKey guard mirrors the Earworm disambiguation
            // pattern against future DamageTakenUpMultiplier variants.
            int modified = incoming;
            if (statuses != null &&
                statuses.TryGet(CharacterStatusId.DamageTakenUpMultiplier,
                    out var captivated) &&
                captivated != null && captivated.Stacks > 0 &&
                captivated.Definition != null &&
                string.Equals(captivated.Definition.StatusKey, "captivated",
                    StringComparison.OrdinalIgnoreCase))
            {
                var gm = GigManager.Instance;
                float perStack = gm != null
                    ? gm.CaptivatedVibeBonusPerStack
                    : DefaultCaptivatedBonusPerStack;

                float mult = 1f + captivated.Stacks * perStack;
                modified = Mathf.RoundToInt(incoming * mult);

#if UNITY_EDITOR
                Debug.Log(
                    $"<color=#888888>[ApplyIncomingVibe] CAPTIVATED ×{mult:0.##} " +
                    $"stacks={captivated.Stacks} incoming={incoming} " +
                    $"applied={modified}</color>");
#endif
            }

            AddVibe(modified, duration);
            return modified;
        }

        /// <summary>
        /// [S5e] The member regains <paramref name="amount"/> persuasion
        /// resistance (anti-player effect, as before). RESTORES the pool.
        /// </summary>
        public void RemoveVibe(int amount, float duration = 2f)
        {
            SetCurrentVibe(CurrentVibe + amount, duration);
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
        /// CurrentVibe and could deplete it to 0 (play: AddVibe;
        /// dev: DevSetCurrentVibe / DevSetMaxVibe). [S5e]
        /// Preserves the !IsConvinced guard — Convinced is a sticky state
        /// transition; the !IsConvinced gate prevents OnConvinced re-firing.
        /// Mirror of BandCharacterStats.CheckBreakdownThreshold (M4.1 pattern).
        /// </summary>
        private void CheckConvincedThreshold()
        {
            if (CurrentVibe <= 0 && !IsConvinced)
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
        /// Dev Mode: Set Vibe (resistance) directly to a clamped target value.
        /// [S5e] Fires Convinced if the target reaches 0 and the audience is not
        /// yet convinced.
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
        /// [S5e] Under inverted semantics, changing MaxVibe can no longer
        /// trigger Convinced by itself (clamp floor keeps Current ≥ 1); the
        /// threshold re-check is retained as a harmless invariant guard.
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
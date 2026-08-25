using ALWTTT.Interfaces;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Gameplay-flow rules and setup-screen defaults consumed by GigManager
    /// (runtime sequencing) and GigSetupController (setup-screen defaults).
    /// One of four SOs produced by M4.6F-2 from the former monolithic GigManager
    /// inspector / GigSetupConfigData "Default Values" header.
    ///
    /// Authority: SSoT_Gig_Combat_Core (runtime) + SSoT_Gig_Encounter (defaults
    /// consumption by setup screen). PersistentGameplayData.ApplyRunConfig reads
    /// the Default* properties as fallbacks when RunConfig overrides are absent.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GigFlowSettings",
        menuName = "ALWTTT/Gig/GigFlowSettings",
        order = 10)]
    public sealed class GigFlowSettingsSO : ScriptableObject
    {
        // --- Composition / Jam ---

        [Header("Composition")]
        [SerializeField] private JamRules jamRules = new JamRules();
        public JamRules JamRules => jamRules;

        [SerializeField, Tooltip("Cards drawn after each loop completion. " +
            "0 = disabled. Hand-cap clamp is applied by DeckManager.DrawCards.")]
        private int drawPerLoop = 0;
        public int DrawPerLoop => drawPerLoop;

        [SerializeField, Tooltip("Cards drawn when Play is pressed (start of " +
            "the jam, before the first loop renders). 0 = disabled. " +
            "Independent of DrawPerLoop � set equal if you want them to match. " +
            "Hand-cap clamp is applied by DeckManager.DrawCards.")]
        private int drawCardsOnPlay = 0;
        public int DrawCardsOnPlay => drawCardsOnPlay;

        // --- Action card gating ---

        [Header("Action Card Gating (MVP)")]
        [SerializeField, Tooltip("MVP: when Play is pressed, discard remaining " +
            "Action cards from the hand.")]
        private bool discardActionCardsOnPlay = true;
        public bool DiscardActionCardsOnPlay => discardActionCardsOnPlay;

        [SerializeField, Tooltip("Allow Action cards with timing=Always to be " +
            "playable during performance.")]
        private bool allowActionCardsDuringPerformance = false;
        public bool AllowActionCardsDuringPerformance => allowActionCardsDuringPerformance;

        // --- Gig end behavior ---

        [Header("Gig End Behavior")]
        [SerializeField] private bool skipAudienceActionsAfterFinalSong = true;
        public bool SkipAudienceActionsAfterFinalSong => skipAudienceActionsAfterFinalSong;

        // --- Setup-screen defaults (formerly on GigSetupConfigData) ---

        [Header("Setup Defaults � Inspiration")]
        [SerializeField] private int defaultInitialGigInspiration = 0;
        [SerializeField] private int defaultInspirationPerLoop = 3;   // [S5e / D2] was 0

        public int DefaultInitialGigInspiration => defaultInitialGigInspiration;
        public int DefaultInspirationPerLoop => defaultInspirationPerLoop;

        /// <summary>
        /// Legacy alias preserved for callers written before F-2: matches
        /// GigSetupConfigData.DefaultStartingInspiration (which itself
        /// aliased DefaultInitialGigInspiration). Equivalent to
        /// <see cref="DefaultInitialGigInspiration"/>.
        /// </summary>
        public int DefaultStartingInspiration => defaultInitialGigInspiration;

        // ─── [ECON-1] Per-turn play economy defaults ─────────────────────
        [Header("Setup Defaults — Per-Turn Play Economy (ECON-1)")]
        [SerializeField, Min(0), Tooltip("Action-card plays each musician gets " +
            "per PERIOD (pre-song action window, and each performance loop). " +
            "D-ECON-4=A: strict 1 in all periods. 0 disables Action plays entirely.")]
        private int defaultActionPlaysPerTurn = 1;

        [SerializeField, Min(0), Tooltip("Composition-card plays each musician " +
            "gets per PERIOD. D-ECON-4=A: strict 1 in all periods. " +
            "0 disables Composition plays entirely.")]
        private int defaultCompositionPlaysPerTurn = 1;

        public int DefaultActionPlaysPerTurn => Mathf.Max(0, defaultActionPlaysPerTurn);
        public int DefaultCompositionPlaysPerTurn => Mathf.Max(0, defaultCompositionPlaysPerTurn);

        // ─── [R5-b / D-R5-12=A] Voltage generation switch ────────────────
        [Header("Gig Rules — Voltage (R5)")]
        [SerializeField, Tooltip("When ON, every GENUINELY consumed play by " +
            "Conito (action or composition, any Inspiration cost including 0) " +
            "grants +1 Voltage. When OFF, Voltage is only applied by cards " +
            "that carry an explicit ApplyStatusEffectSpec for it. Read per " +
            "play, so toggling during Play mode takes effect on the next play.")]
        private bool generateVoltageOnConsumedPlay = true;

        public bool GenerateVoltageOnConsumedPlay => generateVoltageOnConsumedPlay;

        // ─── [R5-c / D-R5-13..17] Overload — consumidor de Voltage ───────
        [SerializeField, Tooltip("When ON, a loop boundary where the Voltage " +
            "bearer holds >= OverloadThreshold stacks discharges Overload: it " +
            "spends OverloadCost stacks and multiplies THAT loop's SongHype " +
            "contribution by OverloadHypeFactor. Read once per loop boundary, " +
            "so toggling during Play takes effect on the next finished loop.")]
        // [R5-d / D-R5-20=B] DEFAULT FLIPPED to false: the player-facing
        // Overload is the Action card. This automatic discharge survives as a
        // tuning/dev fallback only. NOTE: flipping this default does NOT change
        // an already-serialized asset — the checkbox on the live
        // GigFlowSettingsSO must be unticked by hand. Review at R8: if still
        // OFF, retire the consumer (D-R5-20 option C).
        private bool overloadConsumerEnabled = false;


        [SerializeField, Min(1), Tooltip("Voltage stacks required to discharge " +
            "Overload. Tuning rule (D-R5-10 rider): if Overload never fires in " +
            "short songs, LOWER this — never raise Voltage generation.")]
        private int overloadThreshold = 6;

        [SerializeField, Min(1), Tooltip("Voltage stacks spent per discharge. " +
            "Surplus above the cost survives and keeps accumulating. Values " +
            "above the threshold are harmless: the spend clamps to the stacks " +
            "actually held.")]
        private int overloadCost = 6;

        [SerializeField, Tooltip("Multiplier applied to the loop's hype delta " +
            "on discharge. Applied in the expression, AFTER ComputeHypeDelta " +
            "and after MeterTuningSO.SongHypeDeltaMultiplier — the asset is " +
            "never mutated. Clamped to >= 1: Overload cannot make a loop worse.")]
        private float overloadHypeFactor = 1.5f;

        public bool OverloadConsumerEnabled => overloadConsumerEnabled;
        public int OverloadThreshold => Mathf.Max(1, overloadThreshold);
        public int OverloadCost => Mathf.Max(1, overloadCost);
        public float OverloadHypeFactor => Mathf.Max(1f, overloadHypeFactor);

        // ─── [R5-d] Bonus loop (Overload card) ───────────────────────────
        [Header("Gig Rules — Bonus Loop (R5-d)")]
        [SerializeField, Min(0), Tooltip("Maximum bonus loops a single part may " +
            "receive (D-R5-21 rider). 1 stops a banked resource from chaining " +
            "several extra loops into one part. 0 disables bonus loops entirely.")]
        private int maxBonusLoopsPerPart = 1;

        [SerializeField, Tooltip("When on, a bonus loop is rendered with the " +
            "soloist's extra track layered over the unchanged base. When off, " +
            "the bonus loop is a plain repeat. Read at grant time.")]
        private bool bonusSoloEnabled = true;

        [SerializeField, Range(0f, 1f), Tooltip("Live volume applied to every " +
            "non-solo channel for the duration of the solo loop. 1 = no duck. " +
            "Composes multiplicatively with the mix; it does not overwrite it.")]
        private float bonusSoloDuck01 = 0.55f;

        public int MaxBonusLoopsPerPart => Mathf.Max(0, maxBonusLoopsPerPart);
        public bool BonusSoloEnabled => bonusSoloEnabled;
        public float BonusSoloDuck01 => Mathf.Clamp01(bonusSoloDuck01);


        [Header("Setup Defaults � Hand / Inspiration Policies")]
        [SerializeField] private bool defaultDiscardHandBetweenTurns = false;
        [SerializeField] private bool defaultKeepInspirationBetweenTurns = false;

        public bool DefaultDiscardHandBetweenTurns => defaultDiscardHandBetweenTurns;
        public bool DefaultKeepInspirationBetweenTurns => defaultKeepInspirationBetweenTurns;

        [Header("Setup Defaults � Required Songs")]
        [SerializeField] private bool allowOverrideRequiredSongCount = true;
        [SerializeField, Min(1)] private int defaultRequiredSongCount = 1;

        public bool AllowOverrideRequiredSongCount => allowOverrideRequiredSongCount;
        public int DefaultRequiredSongCount => Mathf.Max(1, defaultRequiredSongCount);
    }
}
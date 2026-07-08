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
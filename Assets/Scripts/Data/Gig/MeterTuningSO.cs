using ALWTTT.Music;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Meter-math and scoring tuning consumed by GigManager. Owns SongHype
    /// caps/seed, Vibe/Hype balancing, Flow→Vibe (bifurcated MVP), loop
    /// scoring config, hype thresholds, and Breakdown reset fraction.
    ///
    /// Authority: SSoT_Scoring_and_Meters (semantic) + SSoT_Gig_Combat_Core
    /// (combat economy). The inner LoopScoringConfig / HypeThresholds structs
    /// remain owned by ALWTTT.Music.LoopScoreCalculator — this SO hosts an
    /// instance of each.
    /// </summary>
    [CreateAssetMenu(
        fileName = "MeterTuning",
        menuName = "ALWTTT/Gig/MeterTuning",
        order = 11)]
    public sealed class MeterTuningSO : ScriptableObject
    {
        // --- SongHype ---

        [Header("SongHype")]
        [SerializeField] private float maxSongHype = 100f;
        [SerializeField, Tooltip("Raw starting SongHype points for each new song.")]
        private float startingSongHype = 10f;

        public float MaxSongHype => maxSongHype;
        public float StartingSongHype => startingSongHype;

        // --- Vibe / Hype balancing ---

        [Header("Vibe / Hype Balancing")]
        [SerializeField] private int maxVibeFromSongHype = 20;
        [SerializeField] private float songHypeDeltaMultiplier = 1f;

        public int MaxVibeFromSongHype => maxVibeFromSongHype;
        public float SongHypeDeltaMultiplier => songHypeDeltaMultiplier;

        // --- Flow → Vibe (bifurcated MVP, M4.2) ---

        [Header("Flow → Vibe (Bifurcated MVP)")]
        [SerializeField, Tooltip("Action cards: each Flow stack adds this flat " +
            "Vibe bonus to positive Vibe gains.")]
        private int flowActionVibeBonusPerStack = 1;

        [SerializeField, Tooltip("If enabled, Action cards get flat Flow→Vibe " +
            "bonus (original MVP path).")]
        private bool flowActionFlatBonus = true;

        [SerializeField, Tooltip("Composition cards + Song End: Vibe multiplier " +
            "per Flow stack. finalVibe = base × (1 + flowStacks × this).")]
        private float flowVibeMultiplier = 0.08f;

        public int FlowActionVibeBonusPerStack => flowActionVibeBonusPerStack;
        public bool FlowActionFlatBonus => flowActionFlatBonus;
        public float FlowVibeMultiplier => flowVibeMultiplier;

        // --- Loop scoring + hype thresholds ---

        [Header("Loop Scoring")]
        [SerializeField] private LoopScoringConfig loopScoringConfig = LoopScoringConfig.Default;
        [SerializeField] private HypeThresholds hypeThresholds = HypeThresholds.Default;

        /// <summary>
        /// Live reference. GigManager.InitLoopScoringConfig writes
        /// possibleRoleCount / totalMusicians at gig start. Returning a
        /// readonly copy would force authors to roundtrip through the SO
        /// to mutate gig-time context — keep direct access.
        /// </summary>
        public ref LoopScoringConfig LoopScoringConfigRef => ref loopScoringConfig;
        public LoopScoringConfig LoopScoringConfig => loopScoringConfig;
        public HypeThresholds HypeThresholds => hypeThresholds;

        // --- Breakdown ---

        [Header("Breakdown")]
        [SerializeField, Range(0f, 1f)] private float breakdownStressResetFraction = 0.5f;
        public float BreakdownStressResetFraction => breakdownStressResetFraction;
    }
}
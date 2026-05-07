using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Visible-pacing and animation values consumed by GigManager during runtime
    /// orchestration. These values affect player-perceived feel (curve shapes,
    /// idle BPM, tween durations) but not gameplay rules.
    ///
    /// Authority: SSoT_Gig_Combat_Core. Separated from MeterTuningSO and
    /// GigFlowSettingsSO so that visual designers can tune timing/curves
    /// without touching meter math or flow rules.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GigPresentation",
        menuName = "ALWTTT/Gig/GigPresentation",
        order = 12)]
    public sealed class GigPresentationSO : ScriptableObject
    {
        // --- Audience beat response ---

        [Header("Audience Beat Response")]
        [SerializeField]
        private AnimationCurve audienceJumpIntensityCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField, Range(0f, 1f)] private float audienceJumpThreshold = 0.1f;

        public AnimationCurve AudienceJumpIntensityCurve => audienceJumpIntensityCurve;
        public float AudienceJumpThreshold => audienceJumpThreshold;

        // --- Animation defaults ---

        [Header("Animation")]
        [SerializeField, Tooltip("Fallback BPM driving idle musician/audience animation " +
            "when no song is playing.")]
        private int idleBpm = 120;
        public int IdleBpm => idleBpm;

        // --- Sequence pacing ---

        [Header("Timing — Sequence Pacing")]
        [SerializeField, Tooltip("Pause after a song ends before vibe resolution begins.")]
        private float songEndPause = 3f;

        [SerializeField, Tooltip("Delay between vibe applications to consecutive " +
            "audience members at song end.")]
        private float perAudienceVibeDelay = 1f;

        [SerializeField, Tooltip("Delay between consecutive audience action " +
            "executions during AudienceTurn.")]
        private float perAudienceActionDelay = 1f;

        [SerializeField, Tooltip("Tween duration passed to AudienceStats.AddVibe — " +
            "drives the bar fill animation.")]
        private float barFillDelay = 3f;

        public float SongEndPause => songEndPause;
        public float PerAudienceVibeDelay => perAudienceVibeDelay;
        public float PerAudienceActionDelay => perAudienceActionDelay;
        public float BarFillDelay => barFillDelay;
    }
}
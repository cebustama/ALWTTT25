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

        // --- SongHype stage thresholds (B2 / #6) ---

        [Header("SongHype Stage Thresholds [B2 / #6]")]
        [SerializeField, Range(0f, 1f),
         Tooltip("First stage threshold as fraction of MaxSongHype. " +
            "Fires SFX tag once on upward crossing per song. Default ≈1/3.")]
        private float songHypeStage1Threshold = 0.34f;

        [SerializeField, Range(0f, 1f),
         Tooltip("Second stage threshold as fraction of MaxSongHype. " +
            "Fires SFX tag once on upward crossing per song. Default ≈2/3.")]
        private float songHypeStage2Threshold = 0.67f;

        [SerializeField, Range(0f, 1f),
         Tooltip("Third stage threshold as fraction of MaxSongHype. " +
            "Fires SFX tag once on upward crossing per song. Default = 1.0.")]
        private float songHypeStage3Threshold = 1.0f;

        [SerializeField, Tooltip("Tag passed to BackgroundContainer.ActivateSFX on stage 1.")]
        private string songHypeStage1SfxTag = "lights";

        [SerializeField, Tooltip("Tag passed to BackgroundContainer.ActivateSFX on stage 2.")]
        private string songHypeStage2SfxTag = "smoke";

        [SerializeField, Tooltip("Tag passed to BackgroundContainer.ActivateSFX on stage 3.")]
        private string songHypeStage3SfxTag = "fire";

        public float SongHypeStage1Threshold => songHypeStage1Threshold;
        public float SongHypeStage2Threshold => songHypeStage2Threshold;
        public float SongHypeStage3Threshold => songHypeStage3Threshold;
        public string SongHypeStage1SfxTag => songHypeStage1SfxTag;
        public string SongHypeStage2SfxTag => songHypeStage2SfxTag;
        public string SongHypeStage3SfxTag => songHypeStage3SfxTag;

        // ------------------------------------------------------------------

        [Header("SongHype Bar Visibility [S5f / #6a]")]
        [SerializeField, Tooltip("Master switch for the SongHype bar UI " +
            "(includes the 'L + SFX = N' readout under it). OFF = the bar " +
            "never shows during performance — used for the simplified first " +
            "gig. SongHype still accrues, stage SFX still fire, and song-end " +
            "Vibe conversion is unchanged; only the readout is hidden.")]
        private bool showSongHypeBar = true;

        public bool ShowSongHypeBar => showSongHypeBar;

        // --- SFX → FlatVibe bonus [§5.3.5] ---

        [Header("SFX → FlatVibe Bonus [§5.3.5]")]
        [SerializeField, Min(0f),
         Tooltip("Vibe granted to each audience member when stage 1 (lights) " +
            "fires. DC-SFX-Route=A: routed through ApplyIncomingVibe per " +
            "member so Indifference still blocks. One band-canvas '+N Vibe!' " +
            "floater (not per-audience). D-DCP-2=A default: 3.")]
        private float sfxBonusVibeStage1 = 3f;

        [SerializeField, Min(0f),
         Tooltip("Vibe granted to each audience member when stage 2 (smoke) " +
            "fires. D-DCP-2=A default: 6.")]
        private float sfxBonusVibeStage2 = 6f;

        [SerializeField, Min(0f),
         Tooltip("Vibe granted to each audience member when stage 3 (fire) " +
            "fires. D-DCP-2=A default: 10. Scaled to reward 'encore' threshold.")]
        private float sfxBonusVibeStage3 = 10f;

        public float SfxBonusVibeStage1 => sfxBonusVibeStage1;
        public float SfxBonusVibeStage2 => sfxBonusVibeStage2;
        public float SfxBonusVibeStage3 => sfxBonusVibeStage3;

        /// <summary>
        /// [§5.3.5] Per-stage bonus lookup. Returns 0 for invalid stage indices.
        /// Used by GigManager.ApplySfxBonusVibe (called from FireSongHypeStage).
        /// </summary>
        public float GetSfxBonusVibe(int stage)
        {
            switch (stage)
            {
                case 1: return sfxBonusVibeStage1;
                case 2: return sfxBonusVibeStage2;
                case 3: return sfxBonusVibeStage3;
                default: return 0f;
            }
        }
    }
}
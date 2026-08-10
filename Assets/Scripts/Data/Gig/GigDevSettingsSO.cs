using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Inspector-time dev toggles consumed by GigManager. Strictly scoped
    /// per slate D6: log gating, debug-mode flags, and per-feature debug
    /// pickers. Unrelated to DevModeController (runtime cheat menu).
    ///
    /// [S5.3.5] Also hosts the demo-build auto-start switch + reference to
    /// the baked DemoLaunchConfigSO. GigSetupController reads these in
    /// Start() and bypasses the picker UI when the switch is on. Locality
    /// per DC-1=C (dev settings, not flow settings).
    ///
    /// Authority: SSoT_Gig_Combat_Core. Toggles can flip live during play
    /// (e.g. the D-key handler in GigManager.Update flips
    /// debugInstrumentPicker / debugMusicianVolume); ScriptableObject fields
    /// support that mutation pattern at edit-time and at runtime.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GigDevSettings",
        menuName = "ALWTTT/Gig/GigDevSettings",
        order = 13)]
    public sealed class GigDevSettingsSO : ScriptableObject
    {
        // --- Logging ---

        [Header("Logging")]
        [SerializeField] private bool useLogs = true;
        [SerializeField] private bool useCompositionLogs = true;

        // [LOG-1 / D-LOG-3=B] Second logging tier. UseLogs is the master
        // switch; this one gates the chatty per-render / per-loop dumps whose
        // owning milestone is already closed (S5a smoke, B3 LoopCtx, cache
        // DIAG blocks, theory tables).
        //
        // The six test-bearing lines do NOT hang off this flag and must stay
        // visible with LogVerbose OFF:
        //   [ORDER-1] . [JAM-1] . [JAM-2] . [B1][stemCache] (WITHOUT [DIAG])
        //   . [DBG-C2/CacheBypass] . "Timeline ch="
        // Authority: SSoT_Dev_Mode (log levels section).
        [SerializeField, Tooltip("Second logging tier. Leave OFF for a " +
            "readable console; turn ON to restore the per-render diagnostic " +
            "dumps. Does NOT gate the lines the smoke tests depend on.")]
        private bool logVerbose = false;

        public bool UseLogs
        {
            get => useLogs;
            set => useLogs = value;
        }

        public bool UseCompositionLogs
        {
            get => useCompositionLogs;
            set => useCompositionLogs = value;
        }

        /// <summary>[LOG-1] Verbose tier. Read together with UseLogs: a line
        /// is verbose-visible only when UseLogs AND LogVerbose are both true.
        /// Setter exposed so Dev Mode / editor scripts can flip it at runtime,
        /// matching the pattern of the debug-surface flags below.</summary>
        public bool LogVerbose
        {
            get => logVerbose;
            set => logVerbose = value;
        }

        // --- Debug surfaces ---

        [Header("Debug Surfaces")]
        [SerializeField] private bool debugSongHype = false;
        [SerializeField] private bool debugInstrumentPicker = false;
        [SerializeField] private bool debugMusicianVolume = false;

        /// <summary>Setter exposed because GigManager.AddSongHype / OnDebugSongHypeSliderChanged
        /// gate on this and the D-key handler (and editor scripts) flip it at runtime.</summary>
        public bool DebugSongHype
        {
            get => debugSongHype;
            set => debugSongHype = value;
        }

        /// <summary>Setter exposed because GigManager.Update toggles this on D-key.</summary>
        public bool DebugInstrumentPicker
        {
            get => debugInstrumentPicker;
            set => debugInstrumentPicker = value;
        }

        /// <summary>Setter exposed because GigManager.Update toggles this on D-key.</summary>
        public bool DebugMusicianVolume
        {
            get => debugMusicianVolume;
            set => debugMusicianVolume = value;
        }

        // --- Demo-build auto-start [S5.3.5] ---

        [Header("Demo Auto-start [S5.3.5]")]
        [SerializeField, Tooltip("When true AND DemoLaunchConfig is non-null, " +
            "GigSetupController.Start() bypasses the picker UI and immediately " +
            "launches the gig using the baked values. Production builds keep " +
            "this OFF - manual GigSetup interaction is preserved. DC-1=C locks " +
            "locality on GigDevSettings (not GigFlowSettings).")]
        private bool autoStartFromDefaults = false;

        [SerializeField, Tooltip("Baked demo-launch values consumed when " +
            "AutoStartFromDefaults is true. If null, auto-start is suppressed " +
            "and a warning logs. Wire DemoLaunchConfig.asset here.")]
        private DemoLaunchConfigSO demoLaunchConfig;

        public bool AutoStartFromDefaults => autoStartFromDefaults;
        public DemoLaunchConfigSO DemoLaunchConfig => demoLaunchConfig;

        // --- Composition debug [DBG-C1] ---

        [Header("Composition Debug [DBG-C1]")]
        [SerializeField, Tooltip("Dev composition tab format. Full = every " +
            "resolved field per track (figures, per-span archetypes). " +
            "Compact = one line per track with counts. The Copy/fingerprint " +
            "export is always Full regardless of this flag.")]
        private bool compositionDebugFull = false;

        /// <summary>Setter exposed because DevCompositionDebugTab flips it
        /// from the runtime overlay.</summary>
        public bool CompositionDebugFull
        {
            get => compositionDebugFull;
            set => compositionDebugFull = value;
        }
    }
}
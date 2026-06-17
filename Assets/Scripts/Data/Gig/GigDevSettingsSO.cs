using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Inspector-time dev toggles consumed by GigManager. Strictly scoped
    /// per slate D6: log gating, debug-mode flags, and per-feature debug
    /// pickers. Unrelated to DevModeController (runtime cheat menu).
    ///
    /// [§5.3.5] Also hosts the demo-build auto-start switch + reference to
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

        // --- Demo-build auto-start [§5.3.5] ---

        [Header("Demo Auto-start [§5.3.5]")]
        [SerializeField, Tooltip("When true AND DemoLaunchConfig is non-null, " +
            "GigSetupController.Start() bypasses the picker UI and immediately " +
            "launches the gig using the baked values. Production builds keep " +
            "this OFF — manual GigSetup interaction is preserved. DC-1=C locks " +
            "locality on GigDevSettings (not GigFlowSettings).")]
        private bool autoStartFromDefaults = false;

        [SerializeField, Tooltip("Baked demo-launch values consumed when " +
            "AutoStartFromDefaults is true. If null, auto-start is suppressed " +
            "and a warning logs. Wire DemoLaunchConfig.asset here.")]
        private DemoLaunchConfigSO demoLaunchConfig;

        public bool AutoStartFromDefaults => autoStartFromDefaults;
        public DemoLaunchConfigSO DemoLaunchConfig => demoLaunchConfig;
    }
}
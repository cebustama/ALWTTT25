using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Managers
{
    /// <summary>
    /// [§5.3.5 / D-FAST-1=C] Main Menu scene controller. Replaces the prior
    /// pattern of wiring Main Menu buttons directly to <c>SceneChanger</c>
    /// Inspector OnClick events.
    ///
    /// Start button (<see cref="OnStartPressed"/>):
    /// - If <see cref="GigDevSettingsSO.AutoStartFromDefaults"/> is on AND
    ///   the wired <see cref="DemoLaunchConfigSO"/> is valid → launch
    ///   directly via <see cref="GigLauncher.Launch"/>, bypassing the
    ///   GigSetup scene. Single fade cycle to Gig (no intermediate
    ///   GigSetup waypoint, no flicker). Demo build path.
    /// - Otherwise → load GigSetup normally for manual picker UI. Dev /
    ///   production path.
    ///
    /// Quit button (<see cref="OnQuitPressed"/>): routes through
    /// <see cref="UIManager.QuitGame"/> for parity with the ESC-on-MainMenu
    /// quit semantics established in B3-demo-polish F8.
    ///
    /// Forward-design note for ladder mode (post-§5.3.5): when ladder mode
    /// lands, this controller's auto-launch branch likely either (a) routes
    /// to a separate <c>LadderRunner</c> scene/object that orchestrates the
    /// queue, or (b) directly dispatches the first encounter and a
    /// DontDestroyOnLoad LadderRunner picks up subsequent ones from a
    /// gig-won event. Either way <see cref="GigLauncher.Launch"/> remains
    /// the single launch site.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private const string CallerTag = "MainMenu §5.3.5";

        [Header("References")]
        [SerializeField, Tooltip("SceneChanger in the Main Menu scene. " +
            "Resolved via FindFirstObjectByType if unset.")]
        private SceneChanger sceneChanger;

        [Header("Auto-launch Wiring")]
        [SerializeField, Tooltip("Dev settings SO. AutoStartFromDefaults + " +
            "DemoLaunchConfig live here. Same asset wired on the Gig scene's " +
            "GigManager (DC-1=C: dev settings locality).")]
        private GigDevSettingsSO devSettings;

        [SerializeField, Tooltip("Setup roster SO required by GigLauncher / " +
            "PersistentGameplayData.ApplyRunConfig. Same asset wired on the " +
            "GigSetup scene's controller — referenced here so MainMenu can " +
            "auto-launch without traversing GigSetup.")]
        private GigSetupRosterSO setupRoster;

        [SerializeField, Tooltip("Flow settings SO required by GigLauncher / " +
            "PersistentGameplayData.ApplyRunConfig. Same asset wired on the " +
            "GigSetup scene + Gig scene.")]
        private GigFlowSettingsSO flowSettings;

        private void Awake()
        {
            if (sceneChanger == null)
                sceneChanger = FindFirstObjectByType<SceneChanger>();
        }

        /// <summary>
        /// Wire to Main Menu Start button OnClick in the Inspector.
        /// </summary>
        public void OnStartPressed()
        {
            if (TryAutoLaunch()) return;

            // Fall through to manual flow.
            if (sceneChanger != null)
            {
                sceneChanger.OpenGigSetupScene();
            }
            else
            {
                Debug.LogError($"[{CallerTag}] SceneChanger reference missing; " +
                    "cannot route to GigSetup.");
            }
        }

        /// <summary>
        /// Wire to Main Menu Quit button OnClick in the Inspector.
        /// </summary>
        public void OnQuitPressed()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.QuitGame();
            else
                Application.Quit();
        }

        /// <summary>
        /// Try to take the auto-launch path. Returns false (with a log) if
        /// any precondition fails — the caller should fall through to the
        /// manual GigSetup flow.
        /// </summary>
        private bool TryAutoLaunch()
        {
            if (devSettings == null)
                return false; // Silent: dev settings simply not wired on this scene.

            if (!devSettings.AutoStartFromDefaults)
                return false; // Silent: feature is off.

            var demo = devSettings.DemoLaunchConfig;
            if (demo == null)
            {
                Debug.LogWarning($"[{CallerTag}] AutoStartFromDefaults is ON but " +
                    "DemoLaunchConfig is unset on GigDevSettings. " +
                    "Falling back to manual GigSetup flow.");
                return false;
            }

            if (!demo.IsValid(out string reason))
            {
                Debug.LogError($"[{CallerTag}] DemoLaunchConfig is invalid: {reason} " +
                    "Falling back to manual GigSetup flow.");
                return false;
            }

            if (setupRoster == null || flowSettings == null)
            {
                Debug.LogError($"[{CallerTag}] Missing setupRoster or flowSettings " +
                    "reference; cannot auto-launch. " +
                    "Falling back to manual GigSetup flow.");
                return false;
            }

            var runConfig = demo.ToRunConfig();
            if (runConfig == null) return false; // ToRunConfig already logged.

            List<MusicianBase> roster = demo.ResolvedBandRoster;

            Debug.Log($"[{CallerTag}] Auto-launch engaged. " +
                $"DemoConfig='{demo.name}', Encounter='{demo.Encounter.GetLabel()}', " +
                $"Roster={roster.Count}, RequiredSongs={demo.RequiredSongCount}.");

            return GigLauncher.Launch(
                runConfig: runConfig,
                bandRoster: roster,
                setupRoster: setupRoster,
                flowSettings: flowSettings,
                isFinalEncounter: true, // Demo build: single encounter, WinPanel path.
                sceneChanger: sceneChanger,
                callerTag: CallerTag);
        }
    }
}
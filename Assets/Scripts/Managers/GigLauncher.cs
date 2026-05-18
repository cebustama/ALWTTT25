using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Encounters;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Managers
{
    /// <summary>
    /// [§5.3.5] Single entry point for "leave current scene, enter Gig with
    /// this RunConfig". Replaces the inlined launch tail that previously
    /// lived in <c>GigSetupController.OnStartPressed</c> and the auto-start
    /// scaffolding briefly explored on the same controller.
    ///
    /// Consumers (current + planned):
    /// - <c>MainMenuController.OnStartPressed</c> — auto-launch path (Demo
    ///   build), invoked when <see cref="GigDevSettingsSO.AutoStartFromDefaults"/>
    ///   is true and a valid <see cref="DemoLaunchConfigSO"/> is wired.
    ///   Bypasses GigSetup entirely → single fade cycle to Gig.
    /// - <c>GigSetupController.OnStartPressed</c> — manual picker UI path,
    ///   invoked when the dev / production build wants to configure a gig.
    /// - <c>LadderRunner</c> (post-§5.3.5) — multi-encounter ladder mode
    ///   that queues <c>EncounterLaunchConfigSO</c> instances and dispatches
    ///   them through <see cref="Launch"/> one at a time, preserving band
    ///   roster between encounters by passing <c>bandRoster: null</c>.
    ///
    /// Design constraints:
    /// - Stateless. No singleton, no MonoBehaviour. Caller owns all dependencies.
    /// - Validates inputs and logs failures, but does not throw — calling
    ///   code should treat a <c>false</c> return as "launch did not happen"
    ///   and decide whether to retry / fall through.
    /// - The <see cref="GigRunContext"/> singleton is lazily ensured so the
    ///   service works regardless of whether a prior gig has already run.
    /// </summary>
    public static class GigLauncher
    {
        /// <summary>
        /// Apply a RunConfig to <see cref="PersistentGameplayData"/> and
        /// navigate to the Gig scene.
        /// </summary>
        /// <param name="runConfig">The RunConfig to apply. Must be non-null
        /// and have a non-null <c>encounter</c>.</param>
        /// <param name="bandRoster">Band roster to set on PD before applying
        /// the run config. Pass <c>null</c> (or empty) to preserve the
        /// current <c>pd.MusicianList</c> — useful for ladder mode where
        /// the band carries over between encounters.</param>
        /// <param name="setupRoster">Setup roster SO required by
        /// <c>pd.ApplyRunConfig</c> for generic-starter assembly.</param>
        /// <param name="flowSettings">Flow settings SO required by
        /// <c>pd.ApplyRunConfig</c> for default values.</param>
        /// <param name="isFinalEncounter">B3-demo-polish A6 hack carrier:
        /// when true, WinGig routes through WinPanel (Retry/Exit) instead
        /// of mid-run RewardCanvas → ReturnToMap. Demo build always passes
        /// true; ladder mode passes true only for the last encounter in
        /// the queue.</param>
        /// <param name="sceneChanger">SceneChanger reference for the final
        /// <c>OpenGigScene</c> call.</param>
        /// <param name="callerTag">Log prefix for tracing which caller
        /// originated the launch (e.g. "MainMenu §5.3.5", "GigSetup",
        /// "LadderRunner"). Defaults to "GigLauncher".</param>
        /// <returns>True if the launch was dispatched; false if any
        /// precondition failed.</returns>
        public static bool Launch(
            GigRunContext.RunConfig runConfig,
            IList<MusicianBase> bandRoster,
            GigSetupRosterSO setupRoster,
            GigFlowSettingsSO flowSettings,
            bool isFinalEncounter,
            ALWTTT.Utils.SceneChanger sceneChanger,
            string callerTag = "GigLauncher")
        {
            if (runConfig == null)
            {
                Debug.LogError($"[{callerTag}] Launch aborted: runConfig is null.");
                return false;
            }
            if (runConfig.encounter == null)
            {
                Debug.LogError($"[{callerTag}] Launch aborted: runConfig.encounter is null.");
                return false;
            }
            if (setupRoster == null)
            {
                Debug.LogError($"[{callerTag}] Launch aborted: setupRoster is null.");
                return false;
            }
            if (flowSettings == null)
            {
                Debug.LogError($"[{callerTag}] Launch aborted: flowSettings is null.");
                return false;
            }
            if (sceneChanger == null)
            {
                Debug.LogError($"[{callerTag}] Launch aborted: sceneChanger is null.");
                return false;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError($"[{callerTag}] Launch aborted: GameManager.Instance is null.");
                return false;
            }

            var persistentData = gameManager.PersistentGameplayData;
            if (persistentData == null)
            {
                Debug.LogError($"[{callerTag}] Launch aborted: PersistentGameplayData is null.");
                return false;
            }

            // Optional band roster apply. Null/empty = preserve current
            // pd.MusicianList (ladder-mode carry-over path).
            if (bandRoster != null && bandRoster.Count > 0)
            {
                persistentData.SetBandRoster(bandRoster);
            }

            // Ensure GigRunContext singleton exists (lazy).
            var runContext = GigRunContext.Instance;
            if (runContext == null)
            {
                var go = new GameObject("GigRunContext");
                runContext = go.AddComponent<GigRunContext>();
            }
            runContext.BeginRun(runConfig);

            Debug.Log(
                $"[{callerTag}] Stored RunConfig | " +
                $"RunContextId={runContext.GetInstanceID()} | " +
                $"ReturnDest={runConfig.returnDestination}");

            persistentData.ApplyRunConfig(runConfig, setupRoster, flowSettings);

            // [B3-demo-polish / A6] Single-encounter hack carrier. Removed
            // when meta-progression sectors land.
            persistentData.IsFinalEncounter = isFinalEncounter;

            Debug.Log(
                $"[{callerTag}] Launching gig | " +
                $"Deck={runConfig.deckLabel}, " +
                $"Encounter={runConfig.encounter.GetLabel()}, " +
                $"RequiredSongs={runConfig.requiredSongCount}, " +
                $"IsFinalEncounter={isFinalEncounter}");

            sceneChanger.OpenGigScene();
            return true;
        }
    }
}
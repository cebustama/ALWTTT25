using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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


        [Header("Options (PAUSE-1 / D4=B)")]
        [SerializeField, Tooltip("Instance of the shared OptionsPanel prefab placed in this " +
            "scene's canvas. Opened by OnOptionsPressed.")]
        private ALWTTT.UI.OptionsPanelController optionsPanel;

        [Header("Tutorial (S10 / D1a=A)")]
        [SerializeField, Tooltip("Gig played by the «Tutorial» button: the full quartet and an " +
            "easy audience. Its band roster is used as is (the dormitory is skipped).")]
        private DemoLaunchConfigSO tutorialLaunchConfig;

        private void Awake()
        {
            if (sceneChanger == null)
                sceneChanger = FindFirstObjectByType<SceneChanger>();
        }

        // [S10 / D3=A, menú] The menu buttons live in the MainMenuCanvas prefab (with scene
        // overrides), so they are rearranged here at runtime instead of editing the nested
        // prefab: «Continue» (PlayButton, disabled since the demo has no saves) becomes
        // «Tutorial», and a small «Pruebas» button is added in the bottom-left corner.
        private void Start()
        {
            var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Button continueButton = null, exitButton = null, tests = null;
            foreach (var b in buttons)
            {
                if (b.name == "PlayButton") continueButton = b;
                else if (b.name == "ExitButton") exitButton = b;
                else if (b.name == "TestsButton") tests = b;
            }

            if (continueButton != null)
            {
                Rewire(continueButton, "Tutorial", OnTutorialPressed);
                continueButton.name = "TutorialButton";
                continueButton.transform.SetAsFirstSibling();
            }
            else Debug.LogWarning("[S10] Menú: no encuentro el botón «Continue» (PlayButton).");

            if (tests == null && exitButton != null)
            {
                var canvas = exitButton.GetComponentInParent<Canvas>().rootCanvas;
                tests = Instantiate(exitButton, canvas.transform, false);
                tests.name = "TestsButton";
                var rt = (RectTransform)tests.transform;
                rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
                rt.anchoredPosition = new Vector2(24f, 24f);
                rt.sizeDelta = new Vector2(170f, 46f);
                var le = tests.GetComponent<LayoutElement>();
                if (le != null) le.ignoreLayout = true;
                Rewire(tests, "Pruebas", OnTestsPressed);
                var label = tests.GetComponentInChildren<TMP_Text>(true);
                if (label != null) { label.enableAutoSizing = false; label.fontSize = 26f; }
            }

            Debug.Log($"[S10] Menú | Tutorial={(continueButton != null ? "ok" : "falta")} | " +
                      $"Pruebas={(tests != null ? "ok" : "falta")} | " +
                      $"config tutorial={(tutorialLaunchConfig != null ? tutorialLaunchConfig.name : "SIN ASIGNAR")}");
        }

        // Turns off the Inspector (persistent) OnClick calls and wires a new one.
        private static void Rewire(Button b, string text, UnityAction action)
        {
            for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
                b.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(action);
            b.interactable = true;
            var label = b.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = text;
        }

        /// <summary>
        /// Wire to Main Menu Start button OnClick in the Inspector.
        /// </summary>
        public void OnStartPressed()
        {
            // [S5 NAVE-1] The demo starts on the ship hub (D2=A); the captain's
            // cabin opens the linear map. Same fresh-run reset as below.
            if (devSettings != null && devSettings.StartOnShipHub && sceneChanger != null)
            {
                var gm0 = GameManager.Instance;
                if (gm0 != null)
                {
                    gm0.InitGameplayData();
                    gm0.SetInitialDeck();
                    ALWTTT.Map.LinearMapController.EnsureRunSeed(gm0.PersistentGameplayData, devSettings); // [S10]
                }

                Debug.Log($"[S5] Menu → Ship | fresh run");
                sceneChanger.OpenShipScene();
                return;
            }

            // [MAP-DEMO-1] The demo starts on the linear map (D-MAP-1..3).
            if (devSettings != null && devSettings.StartOnLinearMap && sceneChanger != null)
            {
                // Fresh run, same as SceneChanger's MainMenu path: Win/Loss
                // "Exit" (GigManager.HandleExit) loads the menu without it,
                // so the previous run's map progress would otherwise survive.
                var gm = GameManager.Instance;
                if (gm != null)
                {
                    gm.InitGameplayData();
                    gm.SetInitialDeck();
                    ALWTTT.Map.LinearMapController.EnsureRunSeed(gm.PersistentGameplayData, devSettings); // [S10]
                }

                Debug.Log($"[{CallerTag}] StartOnLinearMap → fresh run, opening map scene.");
                sceneChanger.OpenMapScene();
                return;
            }

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
        /// [PAUSE-1 / D4=B] Wire to the Main Menu Options button OnClick.
        /// </summary>
        public void OnOptionsPressed()
        {
            if (optionsPanel != null) optionsPanel.Open();
            else Debug.LogWarning($"[{CallerTag}] Options panel not wired on MainMenuController.");
        }

        /// <summary>
        /// [S10 / D1a=A] «Tutorial» button: a separate path from «Play». Fresh run, the
        /// full quartet of <see cref="tutorialLaunchConfig"/>, tutorial ON without the
        /// opt-in prompt, and the minimal dialog allow-list (PD.TutorialRun, D1b=A).
        /// Final encounter: a win ends on the WinPanel → «Volver al menú».
        /// </summary>
        public void OnTutorialPressed()
        {
            var cfg = tutorialLaunchConfig;
            string reason = "tutorialLaunchConfig is not wired.";
            if (cfg == null || !cfg.IsValid(out reason))
            {
                Debug.LogError($"[S10] Tutorial: no se puede lanzar: {reason}");
                return;
            }
            if (setupRoster == null || flowSettings == null)
            {
                Debug.LogError("[S10] Tutorial: faltan setupRoster o flowSettings en MainMenuController.");
                return;
            }

            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.InitGameplayData();
                gm.SetInitialDeck();
            }
            var pd = gm != null ? gm.PersistentGameplayData : null;
            if (pd != null)
            {
                EnsureSeed(pd);
                pd.TutorialRun = true;
                pd.TutorialEnabled = true;
                pd.TutorialOptInAnswered = true; // no "Tutorial?" prompt in this path
            }

            var runConfig = cfg.ToRunConfig();
            if (runConfig == null) return;
            var roster = cfg.ResolvedBandRoster;

            Debug.Log($"[S10] Menú → Tutorial | gig='{cfg.Encounter.GetLabel()}' | " +
                      $"músicos={roster.Count} | canciones={cfg.RequiredSongCount}");

            GigLauncher.Launch(
                runConfig: runConfig,
                bandRoster: roster,
                setupRoster: setupRoster,
                flowSettings: flowSettings,
                isFinalEncounter: true,
                sceneChanger: sceneChanger,
                callerTag: "S10 Tutorial");
        }

        /// <summary>
        /// [S10 / D3=A] «Pruebas» button: the GigSetup scene (pick deck, band and audience).
        /// </summary>
        public void OnTestsPressed()
        {
            if (sceneChanger == null)
            {
                Debug.LogError($"[{CallerTag}] SceneChanger missing; cannot open GigSetup.");
                return;
            }
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.InitGameplayData();
                gm.SetInitialDeck();
                EnsureSeed(gm.PersistentGameplayData);
            }
            Debug.Log("[S10] Menú → Pruebas (GigSetup)");
            sceneChanger.OpenGigSetupScene();
        }

        private void EnsureSeed(PersistentGameplayData pd) =>
            ALWTTT.Map.LinearMapController.EnsureRunSeed(pd, devSettings);

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
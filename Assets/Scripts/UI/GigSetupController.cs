using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Encounters;
using ALWTTT.Managers;
using ALWTTT.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ALWTTT.Managers.GigRunContext;

namespace ALWTTT.UI
{
    public class GigSetupController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GigSetupConfigData setupConfig;

        [Header("UI")]
        [SerializeField] private TMP_Dropdown bandDeckDropdown;
        [SerializeField] private TMP_Dropdown encounterDropdown;

        [Header("Optional Overrides (Dev/Test)")]
        [SerializeField] private Toggle overrideSongsToggle;
        [SerializeField] private TMP_InputField songsToWinInput;

        [SerializeField] private Toggle overrideStartingInspirationToggle;
        [SerializeField] private TMP_InputField startingInspirationInput;

        [SerializeField] private Toggle overrideInspirationPerLoopToggle;
        [SerializeField] private TMP_InputField inspirationPerLoopInput;

        [SerializeField] private Toggle overrideDiscardHandBetweenTurnsToggle;
        [SerializeField] private Toggle discardHandBetweenTurnsToggle;

        [SerializeField] private Toggle overrideKeepInspirationBetweenTurnsToggle;
        [SerializeField] private Toggle keepInspirationBetweenTurnsToggle;

        [Header("Actions")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;

        [Header("Navigation")]
        [SerializeField] private SceneChanger sceneChanger;

        private void Awake()
        {
            if (sceneChanger == null)
                sceneChanger = FindFirstObjectByType<SceneChanger>();

            BuildBandDeckDropdown();
            BuildEncounterDropdown();
            SetupDefaultUIValues();

            if (startButton != null) startButton.onClick.AddListener(OnStartPressed);
            if (backButton != null) backButton.onClick.AddListener(OnBackPressed);
        }

        private void BuildBandDeckDropdown()
        {
            if (bandDeckDropdown == null || setupConfig == null) return;

            bandDeckDropdown.ClearOptions();

            var opts = new List<string>();
            foreach (var d in setupConfig.AvailableBandDecks)
                opts.Add(d != null ? d.name : "(null deck)");

            bandDeckDropdown.AddOptions(opts);
        }

        private void BuildEncounterDropdown()
        {
            if (encounterDropdown == null || setupConfig == null) return;

            encounterDropdown.ClearOptions();

            var opts = new List<string>();
            foreach (var e in setupConfig.AvailableEncounters)
                opts.Add(e != null ? e.GetLabel() : "(null encounter)");

            encounterDropdown.AddOptions(opts);
        }

        private void SetupDefaultUIValues()
        {
            if (setupConfig == null) return;

            if (songsToWinInput != null)
                songsToWinInput.text = setupConfig.DefaultRequiredSongCount.ToString();

            if (startingInspirationInput != null)
                startingInspirationInput.text = 
                    setupConfig.DefaultStartingInspiration.ToString();

            // Default: overrides OFF. If you want to override, you must enable manually.
            if (overrideSongsToggle != null)
            {
                overrideSongsToggle.isOn = false;
                overrideSongsToggle.interactable = 
                    setupConfig.AllowOverrideRequiredSongCount;
            }

            if (overrideStartingInspirationToggle != null)
                overrideStartingInspirationToggle.isOn = false;

            if (overrideInspirationPerLoopToggle != null)
                overrideInspirationPerLoopToggle.isOn = false;

            if (overrideDiscardHandBetweenTurnsToggle != null)
                overrideDiscardHandBetweenTurnsToggle.isOn = false;

            if (overrideKeepInspirationBetweenTurnsToggle != null)
                overrideKeepInspirationBetweenTurnsToggle.isOn = false;
        }

        private void OnBackPressed()
        {
            if (sceneChanger == null)
            {
                Debug.LogError("[GigSetup] Missing SceneChanger reference.");
                return;
            }

            sceneChanger.OpenMainMenuScene();
        }

        private void OnStartPressed()
        {
            if (setupConfig == null)
            {
                Debug.LogError("[GigSetup] Missing GigSetupConfigData.");
                return;
            }

            var selectedDeck = GetSelectedDeck();
            if (selectedDeck == null)
            {
                Debug.LogError("[GigSetup] Selected BandDeckData is null.");
                return;
            }

            var selectedEncounterSO = GetSelectedEncounterSO();
            if (selectedEncounterSO == null)
            {
                Debug.LogError("[GigSetup] Selected GigEncounter is null.");
                return;
            }

            var selectedEncounter = selectedEncounterSO.BuildRuntime();
            if (selectedEncounter == null)
            {
                Debug.LogError("[GigSetup] Failed to build runtime GigEncounter from SO.");
                return;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[GigSetup] GameManager.Instance is null.");
                return;
            }

            var persistentData = gameManager.PersistentGameplayData;
            if (persistentData == null)
            {
                Debug.LogError("[GigSetup] PersistentGameplayData is null.");
                return;
            }

            // --- Ensure GigRunContext exists ---
            var runContext = GigRunContext.Instance;
            if (runContext == null)
            {
                var go = new GameObject("GigRunContext");
                runContext = go.AddComponent<GigRunContext>();
            }

            // --- Build run configuration ---
            var runConfig = new GigRunContext.RunConfig
            {
                bandDeck = selectedDeck,
                encounter = selectedEncounter,

                // Gig requirement
                overrideRequiredSongCount =
                    overrideSongsToggle != null && overrideSongsToggle.isOn,

                requiredSongCount =
                    ParseIntSafe(
                        songsToWinInput,
                        setupConfig.DefaultRequiredSongCount,
                        min: 1),

                // Inspiration
                overrideInitialGigInspiration =
                    overrideStartingInspirationToggle != null &&
                    overrideStartingInspirationToggle.isOn,

                initialGigInspiration =
                    ParseIntSafe(
                        startingInspirationInput,
                        setupConfig.DefaultStartingInspiration,
                        min: 0),

                overrideInspirationPerLoop =
                    overrideInspirationPerLoopToggle != null &&
                    overrideInspirationPerLoopToggle.isOn,

                inspirationPerLoop =
                    ParseIntSafe(
                        inspirationPerLoopInput,
                        setupConfig.DefaultInspirationPerLoop,
                        min: 0),

                // Policies
                overrideDiscardHandBetweenTurns =
                    overrideDiscardHandBetweenTurnsToggle != null &&
                    overrideDiscardHandBetweenTurnsToggle.isOn,

                discardHandBetweenTurns =
                    discardHandBetweenTurnsToggle != null &&
                    discardHandBetweenTurnsToggle.isOn,

                overrideKeepInspirationBetweenTurns =
                    overrideKeepInspirationBetweenTurnsToggle != null &&
                    overrideKeepInspirationBetweenTurnsToggle.isOn,

                keepInspirationBetweenTurns =
                    keepInspirationBetweenTurnsToggle != null &&
                    keepInspirationBetweenTurnsToggle.isOn,

                returnDestination = GigReturnDestination.GigSetup
            };

            // --- Store run context (debug / resolution layer) ---
            runContext.BeginRun(runConfig);

            Debug.Log(
                $"[GigSetup] Stored RunConfig | " +
                $"RunContextId={runContext.GetInstanceID()} | " +
                $"ReturnDest={runConfig.returnDestination}"
            );

            // --- Apply ALL gameplay state atomically ---
            persistentData.ApplyRunConfig(runConfig, setupConfig);

            Debug.Log(
                $"[GigSetup] Starting gig | " +
                $"Deck={selectedDeck.name}, " +
                $"Encounter={selectedEncounterSO.GetLabel()}, " +
                $"RequiredSongs={runConfig.requiredSongCount}, " +
                $"DiscardBetweenTurns={runConfig.discardHandBetweenTurns}, " +
                $"KeepInspiration={runConfig.keepInspirationBetweenTurns}"
            );

            // --- Navigate ---
            if (sceneChanger == null)
            {
                Debug.LogError("[GigSetup] SceneChanger reference is missing.");
                return;
            }

            sceneChanger.OpenGigScene();
        }

        private BandDeckData GetSelectedDeck()
        {
            var list = setupConfig.AvailableBandDecks;
            if (list == null || list.Count == 0) return null;
            int i = Mathf.Clamp(bandDeckDropdown != null ? bandDeckDropdown.value : 0, 0, list.Count - 1);
            return list[i];
        }

        private GigEncounterSO GetSelectedEncounterSO()
        {
            var list = setupConfig.AvailableEncounters;
            if (list == null || list.Count == 0) return null;
            int i = 
                Mathf.Clamp(encounterDropdown != null ? 
                encounterDropdown.value : 0, 0, list.Count - 1);
            return list[i];
        }

        private int ParseIntSafe(TMP_InputField field, int fallback, int min)
        {
            if (field == null) return fallback;
            if (!int.TryParse(field.text, out int v)) v = fallback;
            return Mathf.Max(min, v);
        }
    }
}

using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Encounters;
using ALWTTT.Characters.Band;
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

        [Header("Auto-assembly (M4.6-prep batch 2)")]
        [SerializeField] private Toggle useMusicianStartersToggle;

        [SerializeField] private Toggle overrideStartingInspirationToggle;
        [SerializeField] private TMP_InputField startingInspirationInput;

        [SerializeField] private Toggle overrideInspirationPerLoopToggle;
        [SerializeField] private TMP_InputField inspirationPerLoopInput;

        [SerializeField] private Toggle overrideDiscardHandBetweenTurnsToggle;
        [SerializeField] private Toggle discardHandBetweenTurnsToggle;

        [SerializeField] private Toggle overrideKeepInspirationBetweenTurnsToggle;
        [SerializeField] private Toggle keepInspirationBetweenTurnsToggle;

        // M4.6-prep merged (1)/(4): roster pickers.
        [Header("Band Roster Picker (M4.6-prep merged 1/4)")]
        [SerializeField] private Transform musicianPickerContent;
        [SerializeField] private GameObject musicianPickerRowPrefab;
        [SerializeField] private TMP_Text musicianPickerCountLabel;

        [Header("Audience Roster Picker (M4.6-prep merged 1/4)")]
        [SerializeField] private Transform audiencePickerContent;
        [SerializeField] private GameObject audiencePickerRowPrefab;
        [SerializeField] private TMP_Text audiencePickerCountLabel;

        [Header("Actions")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;

        [Header("Navigation")]
        [SerializeField] private SceneChanger sceneChanger;

        // Picker runtime state
        private readonly List<MusicianPickerRow> _musicianRows = new();
        private readonly List<AudiencePickerRow> _audienceRows = new();
        private bool _audienceUserCustomized; // tracks if user touched audience picker since last encounter swap

        // Soft caps
        private const int BandMinCount = 1;
        private const int BandMaxCount = 4;
        private const int BandWarnIfBelow = 2; // warn (not block) on single-musician bands

        private void Awake()
        {
            if (sceneChanger == null)
                sceneChanger = FindFirstObjectByType<SceneChanger>();

            BuildBandDeckDropdown();
            BuildEncounterDropdown();
            SetupDefaultUIValues();

            BuildMusicianPicker();
            BuildAudiencePicker();

            if (encounterDropdown != null)
                encounterDropdown.onValueChanged.AddListener(OnEncounterDropdownChanged);

            if (startButton != null) startButton.onClick.AddListener(OnStartPressed);
            if (backButton != null) backButton.onClick.AddListener(OnBackPressed);
        }

        // ----------------------------------------------------------------------
        // Dropdowns (existing)
        // ----------------------------------------------------------------------

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

        // ----------------------------------------------------------------------
        // Picker construction (M4.6-prep merged 1/4)
        // ----------------------------------------------------------------------

        private void BuildMusicianPicker()
        {
            if (musicianPickerContent == null || musicianPickerRowPrefab == null)
            {
                Debug.LogWarning(
                    "[GigSetup] Musician picker content or row prefab unset. " +
                    "Skipping band picker build.");
                return;
            }

            // Clear existing rows
            for (int i = _musicianRows.Count - 1; i >= 0; i--)
            {
                if (_musicianRows[i] != null)
                    Destroy(_musicianRows[i].gameObject);
            }
            _musicianRows.Clear();

            // Resolve roster source
            var gameplayData = GameManager.Instance != null
                ? GameManager.Instance.GameplayData
                : null;
            if (gameplayData == null || gameplayData.AllMusiciansList == null)
            {
                Debug.LogError("[GigSetup] GameplayData.AllMusiciansList unavailable.");
                UpdateMusicianCountLabel();
                return;
            }

            // Resolve current selection: prefer pd.MusicianList if non-empty
            // (returning visitors), else fall back to InitialMusicianList.
            var pd = GameManager.Instance.PersistentGameplayData;
            HashSet<MusicianBase> initialSelection = new();
            if (pd != null && pd.MusicianList != null && pd.MusicianList.Count > 0)
            {
                foreach (var m in pd.MusicianList)
                    if (m != null) initialSelection.Add(m);
            }
            else if (gameplayData.InitialMusicianList != null)
            {
                foreach (var m in gameplayData.InitialMusicianList)
                    if (m != null) initialSelection.Add(m);
            }

            // Build rows
            foreach (var musician in gameplayData.AllMusiciansList)
            {
                if (musician == null) continue;

                var rowGo = Instantiate(musicianPickerRowPrefab, musicianPickerContent);
                var row = rowGo.GetComponent<MusicianPickerRow>();
                if (row == null)
                {
                    Debug.LogError(
                        "[GigSetup] Musician picker row prefab is missing " +
                        "MusicianPickerRow component.");
                    Destroy(rowGo);
                    continue;
                }

                row.Init(musician, initialSelection.Contains(musician));
                row.OnSelectionChanged += OnMusicianRowChanged;
                _musicianRows.Add(row);
            }

            UpdateMusicianCountLabel();
        }

        private void BuildAudiencePicker()
        {
            if (audiencePickerContent == null || audiencePickerRowPrefab == null)
            {
                Debug.LogWarning(
                    "[GigSetup] Audience picker content or row prefab unset. " +
                    "Skipping audience picker build.");
                return;
            }

            // Clear existing rows
            for (int i = _audienceRows.Count - 1; i >= 0; i--)
            {
                if (_audienceRows[i] != null)
                    Destroy(_audienceRows[i].gameObject);
            }
            _audienceRows.Clear();

            // Resolve current encounter for default selection
            var selectedEncounterSO = GetSelectedEncounterSO();
            HashSet<AudienceCharacterData> defaultSelection = new();
            if (selectedEncounterSO != null && selectedEncounterSO.AudienceMemberList != null)
            {
                foreach (var a in selectedEncounterSO.AudienceMemberList)
                    if (a != null) defaultSelection.Add(a);
            }

            // Pool = setupConfig.availableAudienceCharacters ∪ encounter.AudienceMemberList
            var pool = new List<AudienceCharacterData>();
            var seen = new HashSet<AudienceCharacterData>();

            if (setupConfig != null && setupConfig.AvailableAudienceCharacters != null)
            {
                foreach (var a in setupConfig.AvailableAudienceCharacters)
                {
                    if (a == null) continue;
                    if (seen.Add(a)) pool.Add(a);
                }
            }
            if (selectedEncounterSO != null && selectedEncounterSO.AudienceMemberList != null)
            {
                foreach (var a in selectedEncounterSO.AudienceMemberList)
                {
                    if (a == null) continue;
                    if (seen.Add(a)) pool.Add(a);
                }
            }

            // Build rows
            foreach (var audience in pool)
            {
                var rowGo = Instantiate(audiencePickerRowPrefab, audiencePickerContent);
                var row = rowGo.GetComponent<AudiencePickerRow>();
                if (row == null)
                {
                    Debug.LogError(
                        "[GigSetup] Audience picker row prefab is missing " +
                        "AudiencePickerRow component.");
                    Destroy(rowGo);
                    continue;
                }

                row.Init(audience, defaultSelection.Contains(audience));
                row.OnSelectionChanged += OnAudienceRowChanged;
                _audienceRows.Add(row);
            }

            _audienceUserCustomized = false;
            UpdateAudienceCountLabel();
        }

        private void OnEncounterDropdownChanged(int _)
        {
            // If user customized audience selection since last encounter pick,
            // warn that we're resetting to the new encounter's defaults.
            if (_audienceUserCustomized)
            {
                Debug.LogWarning(
                    "[GigSetup] Encounter changed; audience selection reset to " +
                    "the new encounter's baked AudienceMemberList. " +
                    "Previous audience customization discarded.");
            }

            BuildAudiencePicker();
        }

        private void OnMusicianRowChanged(MusicianPickerRow _)
        {
            UpdateMusicianCountLabel();
        }

        private void OnAudienceRowChanged(AudiencePickerRow _)
        {
            _audienceUserCustomized = true;
            UpdateAudienceCountLabel();
        }

        private int UpdateMusicianCountLabel()
        {
            int count = 0;
            for (int i = 0; i < _musicianRows.Count; i++)
                if (_musicianRows[i] != null && _musicianRows[i].IsSelected) count++;

            if (musicianPickerCountLabel != null)
                musicianPickerCountLabel.text =
                    $"selected: {count} / {BandMinCount}-{BandMaxCount}";

            return count;
        }

        private int UpdateAudienceCountLabel()
        {
            int count = 0;
            for (int i = 0; i < _audienceRows.Count; i++)
                if (_audienceRows[i] != null && _audienceRows[i].IsSelected) count++;

            int max = setupConfig != null ? setupConfig.MaxAudienceCount : 4;
            if (audiencePickerCountLabel != null)
                audiencePickerCountLabel.text = $"selected: {count} / 1-{max}";

            return count;
        }

        private List<MusicianBase> GetSelectedMusicians()
        {
            var picked = new List<MusicianBase>();
            for (int i = 0; i < _musicianRows.Count; i++)
            {
                var row = _musicianRows[i];
                if (row != null && row.IsSelected && row.Musician != null)
                    picked.Add(row.Musician);
            }
            return picked;
        }

        private List<AudienceCharacterData> GetSelectedAudience()
        {
            var picked = new List<AudienceCharacterData>();
            for (int i = 0; i < _audienceRows.Count; i++)
            {
                var row = _audienceRows[i];
                if (row != null && row.IsSelected && row.Audience != null)
                    picked.Add(row.Audience);
            }
            return picked;
        }

        // ----------------------------------------------------------------------
        // Existing flow
        // ----------------------------------------------------------------------

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

            // M4.6-prep batch (2): determine deck source based on toggle.
            bool useAutoAssembly =
                useMusicianStartersToggle != null && useMusicianStartersToggle.isOn;

            BandDeckData selectedDeck = null;
            if (!useAutoAssembly)
            {
                selectedDeck = GetSelectedDeck();
                if (selectedDeck == null)
                {
                    Debug.LogError("[GigSetup] Selected BandDeckData is null.");
                    return;
                }
            }

            var selectedEncounterSO = GetSelectedEncounterSO();
            if (selectedEncounterSO == null)
            {
                Debug.LogError("[GigSetup] Selected GigEncounter is null.");
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

            // M4.6-prep merged (1)/(4): apply band picker selection to pd.MusicianList
            // BEFORE the auto-assembly path runs. SetBandRoster handles roster
            // identity (MusicianList, AvailableMusiciansList, health, gameplay data)
            // without touching cards.
            var pickedMusicians = GetSelectedMusicians();
            int bandCount = pickedMusicians.Count;

            if (bandCount < BandMinCount)
            {
                Debug.LogError(
                    $"[GigSetup] Band picker selected {bandCount} musicians; " +
                    $"minimum is {BandMinCount}. Cannot start gig.");
                return;
            }
            if (bandCount > BandMaxCount)
            {
                Debug.LogError(
                    $"[GigSetup] Band picker selected {bandCount} musicians; " +
                    $"maximum is {BandMaxCount}. Cannot start gig.");
                return;
            }
            if (bandCount < BandWarnIfBelow)
            {
                Debug.LogWarning(
                    $"[GigSetup] Band picker selected {bandCount} musician " +
                    $"(below recommended minimum of {BandWarnIfBelow}). " +
                    $"Continuing.");
            }

            persistentData.SetBandRoster(pickedMusicians);

            // M4.6-prep merged (1)/(4): audience picker results + override decision.
            var pickedAudience = GetSelectedAudience();
            int audienceCount = pickedAudience.Count;
            int audienceMax = setupConfig.MaxAudienceCount;

            if (audienceCount < 1)
            {
                Debug.LogError(
                    "[GigSetup] Audience picker selected 0 members; " +
                    "minimum is 1. Cannot start gig.");
                return;
            }
            if (audienceCount > audienceMax)
            {
                Debug.LogError(
                    $"[GigSetup] Audience picker selected {audienceCount} members; " +
                    $"max ({audienceMax}, from GigSetupConfigData.MaxAudienceCount) " +
                    "exceeded. Cannot start gig. Either deselect members or " +
                    "increase MaxAudienceCount to match the GigScene's " +
                    "AudienceMemberPosList size.");
                return;
            }

            // Decide whether to pass an override: only when picker selection
            // differs from encounter's baked list (set-equal comparison).
            List<AudienceCharacterData> audienceOverride = null;
            if (DiffersFromEncounterAudience(pickedAudience, selectedEncounterSO))
            {
                audienceOverride = pickedAudience;
            }

            var selectedEncounter = selectedEncounterSO.BuildRuntime(audienceOverride);
            if (selectedEncounter == null)
            {
                Debug.LogError("[GigSetup] Failed to build runtime GigEncounter from SO.");
                return;
            }

            // Auto-assembly empty-roster guard (kept for defense in depth even though
            // SetBandRoster has already populated MusicianList).
            if (useAutoAssembly)
            {
                if (persistentData.MusicianList == null ||
                    persistentData.MusicianList.Count == 0)
                {
                    Debug.LogError(
                        "[GigSetup] Auto-assembly enabled but " +
                        "PersistentGameplayData.MusicianList is empty " +
                        "after SetBandRoster. This should not happen; " +
                        "investigate picker pipeline.");
                    return;
                }
            }

            // M4.6-prep batch (2): build a human-readable deck label for logs.
            string deckLabel;
            if (useAutoAssembly)
            {
                var roster = persistentData.MusicianList;
                var idParts = new List<string>(roster.Count);
                for (int i = 0; i < roster.Count; i++)
                {
                    var m = roster[i];
                    if (m == null || m.MusicianCharacterData == null) continue;
                    idParts.Add(m.MusicianCharacterData.CharacterId);
                }
                deckLabel = idParts.Count > 0
                    ? "<auto:" + string.Join("+", idParts) + ">"
                    : "<auto:<empty>>";
            }
            else
            {
                deckLabel = selectedDeck != null ? selectedDeck.name : "<no-deck>";
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
                useMusicianStarters = useAutoAssembly,
                deckLabel = deckLabel,
                encounter = selectedEncounter,

                // M4.6-prep merged (1)/(4): audience override (null when
                // picker matches encounter's baked list)
                audienceOverride = audienceOverride,

                overrideRequiredSongCount =
                    overrideSongsToggle != null && overrideSongsToggle.isOn,

                requiredSongCount =
                    ParseIntSafe(
                        songsToWinInput,
                        setupConfig.DefaultRequiredSongCount,
                        min: 1),

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
                $"Deck={runConfig.deckLabel}, " +
                $"AutoAssembly={useAutoAssembly}, " +
                $"Band={bandCount} musicians, " +
                $"Audience={audienceCount} (override={audienceOverride != null}), " +
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

        private static bool DiffersFromEncounterAudience(
            IList<AudienceCharacterData> picked,
            GigEncounterSO encounter)
        {
            if (encounter == null) return picked != null && picked.Count > 0;

            var baked = encounter.AudienceMemberList;
            int bakedCount = baked != null ? baked.Count : 0;
            int pickedCount = picked != null ? picked.Count : 0;

            if (bakedCount != pickedCount) return true;
            if (pickedCount == 0) return false;

            // Set comparison (order-independent, by reference)
            var bakedSet = new HashSet<AudienceCharacterData>();
            for (int i = 0; i < bakedCount; i++)
                if (baked[i] != null) bakedSet.Add(baked[i]);

            for (int i = 0; i < pickedCount; i++)
            {
                if (picked[i] == null) return true;
                if (!bakedSet.Contains(picked[i])) return true;
            }
            return false;
        }

        private BandDeckData GetSelectedDeck()
        {
            var list = setupConfig.AvailableBandDecks;
            if (list == null || list.Count == 0) return null;
            int i = Mathf.Clamp(
                bandDeckDropdown != null ? bandDeckDropdown.value : 0, 0, list.Count - 1);
            return list[i];
        }

        private GigEncounterSO GetSelectedEncounterSO()
        {
            var list = setupConfig.AvailableEncounters;
            if (list == null || list.Count == 0) return null;
            int i = Mathf.Clamp(
                encounterDropdown != null ? encounterDropdown.value : 0, 0, list.Count - 1);
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
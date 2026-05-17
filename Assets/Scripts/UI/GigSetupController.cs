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
        [SerializeField, Tooltip("Selectable roster (decks, encounters, audience pool, " +
            "generic starter catalog, max audience). M4.6F-2: split out from " +
            "the former GigSetupConfigData.")]
        private GigSetupRosterSO setupRoster;

        [SerializeField, Tooltip("Setup-screen default values (required songs, " +
            "starting inspiration, between-turns policies). M4.6F-2: split out " +
            "from the former GigSetupConfigData defaults header.")]
        private GigFlowSettingsSO flowSettings;

        [Tooltip("Direct reference to the GameplayData SO. Used as the primary " +
         "source for the band picker's musician roster. Avoids singleton " +
         "Awake-order issues with GameManager. Falls back to " +
         "GameManager.GameplayData if unset.")]
        [SerializeField] private GameplayData gameplayData;

        [SerializeField, Tooltip("[§5.3.5] Dev settings asset. Read in Start() " +
            "to decide whether to auto-launch with DemoLaunchConfig (demo " +
            "build) or fall through to manual picker UI (dev / production). " +
            "If null, auto-start is suppressed silently.")]
        private GigDevSettingsSO devSettings;

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
            // [§5.3.5 polish] If this scene-load will auto-start straight into
            // the gig, pre-blacken the screen BEFORE pickers build + render.
            // The SceneChanger.OpenGigScene fade-in (~1s on fadeSpeed=1) then
            // runs invisible (alpha already 1), and the scene-entry fade-out
            // reveals the Gig scene normally. UIManager survives via
            // DontDestroyOnLoad from the Main Menu scene, so its singleton is
            // guaranteed non-null here when running the demo build flow.
            if (WillAutoStart())
                UIManager.Instance?.ShowFaderImmediate();

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
        // [§5.3.5] Demo auto-start
        // ----------------------------------------------------------------------

        /// <summary>
        /// [§5.3.5 polish] Pre-Start predicate: will <see cref="Start"/> route
        /// to the auto-start path? Called from <see cref="Awake"/> so the
        /// fader can be pre-blackened before pickers visibly render. Mirrors
        /// the gating in Start exactly — any divergence here would either
        /// leave the screen stuck black (predicate true, Start fails) or let
        /// pickers flicker (predicate false, Start succeeds).
        /// </summary>
        private bool WillAutoStart()
        {
            if (devSettings == null) return false;
            if (!devSettings.AutoStartFromDefaults) return false;
            var demo = devSettings.DemoLaunchConfig;
            if (demo == null) return false;
            return demo.IsValid(out _);
        }

        /// <summary>
        /// [§5.3.5] After Awake builds the pickers, check whether the demo
        /// auto-start switch is on. If both <see cref="GigDevSettingsSO.AutoStartFromDefaults"/>
        /// is true AND a <see cref="DemoLaunchConfigSO"/> is wired on the dev
        /// settings asset, bypass the picker UI and immediately route to the
        /// gig with the baked DemoLaunchConfig values.
        ///
        /// Production builds keep AutoStartFromDefaults OFF — manual GigSetup
        /// interaction is preserved through the normal startButton/OnStartPressed
        /// path. Per DC-1=C.
        /// </summary>
        private void Start()
        {
            if (devSettings == null) return;
            if (!devSettings.AutoStartFromDefaults) return;

            var demo = devSettings.DemoLaunchConfig;
            if (demo == null)
            {
                Debug.LogWarning(
                    "[GigSetup §5.3.5] AutoStartFromDefaults is ON but " +
                    "DemoLaunchConfig is unset on GigDevSettings. " +
                    "Falling back to manual picker UI.");
                return;
            }

            if (!demo.IsValid(out string reason))
            {
                Debug.LogError(
                    $"[GigSetup §5.3.5] DemoLaunchConfig is invalid: {reason} " +
                    "Falling back to manual picker UI.");
                return;
            }

            // One-frame delay so any other Start()s in the scene complete first
            // (GameManager wiring, etc). Mirrors the deferred-invoke pattern that
            // F9 used as a stopgap; the new path replaces F9's local field with
            // an SO-driven gate.
            StartCoroutine(AutoStartRoutine(demo));
        }

        private System.Collections.IEnumerator AutoStartRoutine(DemoLaunchConfigSO demo)
        {
            yield return null; // wait one frame
            ApplyDemoLaunchConfigAndStart(demo);
        }

        /// <summary>
        /// [§5.3.5] Single-shot demo launch. Bypasses the picker UI entirely:
        /// reads roster + encounter + tuning from <paramref name="demo"/>,
        /// builds the RunConfig, applies to PersistentGameplayData, navigates
        /// to GigScene.
        ///
        /// Intentionally duplicates the RunConfig-assembly tail of OnStartPressed
        /// rather than refactoring a shared helper, to keep §5.3.5 a minimal
        /// additive change. Consolidation to a shared LaunchGigWithConfig helper
        /// is a post-demo refactor candidate.
        /// </summary>
        private void ApplyDemoLaunchConfigAndStart(DemoLaunchConfigSO demo)
        {
            Debug.Log($"[GigSetup §5.3.5] Auto-start engaged. " +
                $"DemoConfig='{demo.name}', BandRoster={demo.BandRoster.Count}, " +
                $"Encounter='{demo.Encounter.GetLabel()}', " +
                $"RequiredSongs={demo.RequiredSongCount}, " +
                $"InitialInspiration={demo.InitialGigInspiration}, " +
                $"InspirationPerLoop={demo.InspirationPerLoop}");

            if (setupRoster == null || flowSettings == null)
            {
                Debug.LogError(
                    "[GigSetup §5.3.5] Missing GigSetupRoster or GigFlowSettings. " +
                    "Auto-start aborted.");
                return;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[GigSetup §5.3.5] GameManager.Instance is null.");
                return;
            }
            var persistentData = gameManager.PersistentGameplayData;
            if (persistentData == null)
            {
                Debug.LogError("[GigSetup §5.3.5] PersistentGameplayData is null.");
                return;
            }

            // --- Band roster (auto-assembly path, mirrors the toggle=true branch
            // of OnStartPressed). Bypasses the BandDeckData asset. ---
            var roster = new List<MusicianBase>(demo.BandRoster.Count);
            foreach (var m in demo.BandRoster)
                if (m != null) roster.Add(m);

            if (roster.Count == 0)
            {
                Debug.LogError("[GigSetup §5.3.5] DemoLaunchConfig.BandRoster " +
                    "resolved to 0 valid musicians. Auto-start aborted.");
                return;
            }
            persistentData.SetBandRoster(roster);

            // --- Encounter (audience baked on the encounter asset; no override). ---
            var selectedEncounter = demo.Encounter.BuildRuntime(audienceOverride: null);
            if (selectedEncounter == null)
            {
                Debug.LogError(
                    "[GigSetup §5.3.5] Failed to build runtime GigEncounter " +
                    $"from '{demo.Encounter.name}'. Auto-start aborted.");
                return;
            }

            // --- GigRunContext singleton (matches OnStartPressed pattern). ---
            var runContext = GigRunContext.Instance;
            if (runContext == null)
            {
                var go = new GameObject("GigRunContext");
                runContext = go.AddComponent<GigRunContext>();
            }

            // --- Deck label (auto-assembly idParts format, matches OnStartPressed). ---
            var idParts = new List<string>(roster.Count);
            for (int i = 0; i < roster.Count; i++)
            {
                var m = roster[i];
                if (m == null || m.MusicianCharacterData == null) continue;
                idParts.Add(m.MusicianCharacterData.CharacterId);
            }
            string deckLabel = idParts.Count > 0
                ? "<auto:" + string.Join("+", idParts) + ">"
                : "<auto:<empty>>";

            // --- Build RunConfig with demo overrides. ---
            var runConfig = new GigRunContext.RunConfig
            {
                bandDeck = null,
                useMusicianStarters = true,
                deckLabel = deckLabel,
                encounter = selectedEncounter,
                audienceOverride = null,

                overrideRequiredSongCount = true,
                requiredSongCount = demo.RequiredSongCount,

                overrideInitialGigInspiration = true,
                initialGigInspiration = demo.InitialGigInspiration,

                overrideInspirationPerLoop = true,
                inspirationPerLoop = demo.InspirationPerLoop,

                overrideDiscardHandBetweenTurns = false,
                discardHandBetweenTurns = false,

                overrideKeepInspirationBetweenTurns = false,
                keepInspirationBetweenTurns = false,

                returnDestination = GigReturnDestination.GigSetup
            };

            runContext.BeginRun(runConfig);
            persistentData.ApplyRunConfig(runConfig, setupRoster, flowSettings);

            // [B3-demo-polish / A6] Demo build is single-encounter — force the
            // WinGig branch through the WinPanel (Retry/Exit) instead of the
            // mid-run reward → ReturnToMap flow. Same hack as OnStartPressed.
            persistentData.IsFinalEncounter = true;

            // --- Navigate. ---
            if (sceneChanger == null)
            {
                Debug.LogError("[GigSetup §5.3.5] SceneChanger reference missing.");
                return;
            }
            sceneChanger.OpenGigScene();
        }

        // ----------------------------------------------------------------------
        // Dropdowns (existing)
        // ----------------------------------------------------------------------

        private void BuildBandDeckDropdown()
        {
            if (bandDeckDropdown == null || setupRoster == null) return;

            bandDeckDropdown.ClearOptions();

            var opts = new List<string>();
            foreach (var d in setupRoster.AvailableBandDecks)
                opts.Add(d != null ? d.name : "(null deck)");

            bandDeckDropdown.AddOptions(opts);
        }

        private void BuildEncounterDropdown()
        {
            if (encounterDropdown == null || setupRoster == null) return;

            encounterDropdown.ClearOptions();

            var opts = new List<string>();
            foreach (var e in setupRoster.AvailableEncounters)
                opts.Add(e != null ? e.GetLabel() : "(null encounter)");

            encounterDropdown.AddOptions(opts);
        }

        private void SetupDefaultUIValues()
        {
            if (setupRoster == null) return;

            if (songsToWinInput != null)
                songsToWinInput.text = flowSettings.DefaultRequiredSongCount.ToString();

            if (startingInspirationInput != null)
                startingInspirationInput.text =
                    flowSettings.DefaultStartingInspiration.ToString();

            if (overrideSongsToggle != null)
            {
                overrideSongsToggle.isOn = false;
                overrideSongsToggle.interactable =
                    flowSettings.AllowOverrideRequiredSongCount;
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

            // Resolve roster source. Prefer the serialized field (set in inspector);
            // fall back to GameManager static accessor if available. Avoids
            // Awake-order issues where GameManager.Instance.GameplayData may be
            // unset when GigSetupController.Awake runs.
            var gd = gameplayData;
            if (gd == null && GameManager.Instance != null)
                gd = GameManager.Instance.GameplayData;

            if (gd == null || gd.AllMusiciansList == null)
            {
                Debug.LogError(
                    "[GigSetup] GameplayData unavailable. Wire the 'Gameplay Data' " +
                    "field on GigSetupController in the inspector, or ensure " +
                    "GameManager.GameplayData is populated before this scene loads.");
                UpdateMusicianCountLabel();
                return;
            }

            // Resolve current selection: prefer pd.MusicianList if non-empty
            // (returning visitors), else fall back to InitialMusicianList.
            var pd = GameManager.Instance != null
                ? GameManager.Instance.PersistentGameplayData
                : null;
            HashSet<MusicianBase> initialSelection = new();
            if (pd != null && pd.MusicianList != null && pd.MusicianList.Count > 0)
            {
                foreach (var m in pd.MusicianList)
                    if (m != null) initialSelection.Add(m);
            }
            else if (gd.InitialMusicianList != null)
            {
                foreach (var m in gd.InitialMusicianList)
                    if (m != null) initialSelection.Add(m);
            }

            // Build rows
            foreach (var musician in gd.AllMusiciansList)
            {
                if (musician == null) continue;

                var rowGo = Instantiate(musicianPickerRowPrefab);
                rowGo.transform.SetParent(musicianPickerContent, worldPositionStays: false);

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

            // Pool = setupRoster.availableAudienceCharacters ∪ encounter.AudienceMemberList
            var pool = new List<AudienceCharacterData>();
            var seen = new HashSet<AudienceCharacterData>();

            if (setupRoster != null && setupRoster.AvailableAudienceCharacters != null)
            {
                foreach (var a in setupRoster.AvailableAudienceCharacters)
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

            int max = setupRoster != null ? setupRoster.MaxAudienceCount : 4;
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
            if (setupRoster == null)
            {
                Debug.LogError("[GigSetup] Missing GigSetupRosterSO + GigFlowSettingsSO.");
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
            int audienceMax = setupRoster.MaxAudienceCount;

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
                    $"max ({audienceMax}, from GigSetupRosterSO.MaxAudienceCount) " +
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
                        flowSettings.DefaultRequiredSongCount,
                        min: 1),

                overrideInitialGigInspiration =
                    overrideStartingInspirationToggle != null &&
                    overrideStartingInspirationToggle.isOn,

                initialGigInspiration =
                    ParseIntSafe(
                        startingInspirationInput,
                        flowSettings.DefaultStartingInspiration,
                        min: 0),

                overrideInspirationPerLoop =
                    overrideInspirationPerLoopToggle != null &&
                    overrideInspirationPerLoopToggle.isOn,

                inspirationPerLoop =
                    ParseIntSafe(
                        inspirationPerLoopInput,
                        flowSettings.DefaultInspirationPerLoop,
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
            persistentData.ApplyRunConfig(runConfig, setupRoster, flowSettings);

            // [B3-demo-polish / A6] Demo build is single-encounter. Force IsFinalEncounter=true
            // so WinGig routes to the WinPanel branch (which has Retry/Exit) instead of the
            // mid-run RewardCanvas → ReturnToMap flow. Remove this when meta-progression
            // sectors / multi-encounter runs are wired.
            persistentData.IsFinalEncounter = true;

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

            // Multiset-blind comparison: the picker UI collapses duplicate
            // AudienceCharacterData entries to a single row (HashSet dedup in
            // BuildAudiencePicker), so a no-customization run yields
            // pickedCount == unique-count of baked, not raw bakedCount.
            // Comparing pickedCount against bakedSet.Count (not bakedCount)
            // keeps override null when the user didn't customize, and
            // GigEncounterSO.BuildRuntime falls back to the baked list with
            // duplicates intact. Encounter authors retain control over
            // audience multiplicity. Multiplicity-aware picker UI (count
            // spinner per row) is a future concern; deviating from baked via
            // the picker still loses duplicate information for that run.
            var bakedSet = new HashSet<AudienceCharacterData>();
            for (int i = 0; i < bakedCount; i++)
                if (baked[i] != null) bakedSet.Add(baked[i]);

            if (bakedSet.Count != pickedCount) return true;
            if (pickedCount == 0) return false;

            for (int i = 0; i < pickedCount; i++)
            {
                if (picked[i] == null) return true;
                if (!bakedSet.Contains(picked[i])) return true;
            }
            return false;
        }

        private BandDeckData GetSelectedDeck()
        {
            var list = setupRoster.AvailableBandDecks;
            if (list == null || list.Count == 0) return null;
            int i = Mathf.Clamp(
                bandDeckDropdown != null ? bandDeckDropdown.value : 0, 0, list.Count - 1);
            return list[i];
        }

        private GigEncounterSO GetSelectedEncounterSO()
        {
            var list = setupRoster.AvailableEncounters;
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
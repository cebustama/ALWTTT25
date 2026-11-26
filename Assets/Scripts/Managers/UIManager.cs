using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Encounters;
using ALWTTT.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ALWTTT.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Canvases")]
        [SerializeField] private GigCanvas gigCanvas;
        [SerializeField] private RewardCanvas rewardCanvas;
        [SerializeField] private InventoryCanvas inventoryCanvas;
        [SerializeField] private Transform beatHUD;

        [Header("Fader")]
        [SerializeField] private CanvasGroup fader;
        [SerializeField] private float fadeSpeed = 1f;

        [Header("Scene Indices")]
        // [B3-demo-polish / D-UX3=D] Build index of the main menu / gig setup scene.
        // ESC returns here from any other scene. If already on this scene, ESC quits.
        [Tooltip("Build index of the main menu / gig setup scene. ESC returns here " +
                 "from any other scene. If already on this scene, ESC quits the game.")]
        [SerializeField] private int mainMenuSceneIndex = 0;

        #region Encapsulation

        public GigCanvas GigCanvas => gigCanvas;
        public RewardCanvas RewardCanvas => rewardCanvas;
        public InventoryCanvas InventoryCanvas => inventoryCanvas;
        public GameManager GameManager => GameManager.Instance;
        public int MainMenuSceneIndex => mainMenuSceneIndex;
        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                transform.parent = null; // why?
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {

        }

        private float _lastUpdateLog;

        private void Update()
        {
            // [F8 diagnostic] Confirm Update runs + ESC detection works.
            if (Time.unscaledTime - _lastUpdateLog > 1f)
            {
                _lastUpdateLog = Time.unscaledTime;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscapeKey();
            }

            // [S10 / D9] Space pauses the gig (frozen world, the hand can still be looked at).
            if (Input.GetKeyDown(KeyCode.Space) && !TypingInAField())
            {
                var pause = PauseMenuController.Instance;
                if (pause != null) pause.HandleSpace();
            }
        }

        // [S10 / D9] Space is also the UI "Submit" key: a button left selected after a click
        // (Play!) would fire again on Space. In a gig nothing needs keyboard focus but text fields.
        private void LateUpdate()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null || GigManager.Instance == null) return;
            if (PauseGate.IsPaused && !PauseGate.InspectOnly) return;   // the pause menu keeps its focus
            var sel = es.currentSelectedGameObject;
            if (sel != null && sel.GetComponent<UnityEngine.UI.Selectable>() != null && !TypingInAField())
                es.SetSelectedGameObject(null);
        }

        private static bool TypingInAField()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            var sel = es != null ? es.currentSelectedGameObject : null;
            return sel != null && (sel.GetComponent<TMPro.TMP_InputField>() != null ||
                                   sel.GetComponent<UnityEngine.UI.InputField>() != null);
        }

        // [B3-demo-polish / D-UX3=D] Simple two-state ESC behavior:
        // - Not on main menu: return to main menu.
        // - On main menu: quit the game.
        // [PAUSE-1] D-UX3=A implemented: in a gig, Esc opens the pause menu (layer 3
        // below); the scene rule only runs where no layer consumed the key.
        private void HandleEscapeKey()
        {
            // [CARD-FACE-3 / O-CF3-2 / D5=B] Esc has ONE listener and an explicit priority chain.
            // Before this batch, CardDetailViewController had its own Update watching Esc, so a single
            // press was consumed twice in the same frame: the modal closed AND the gig was abandoned
            // for the main menu. Which of the two ran first was undefined (Unity does not order Update
            // between scripts unless Script Execution Order says so), so a guard on either side would
            // have been a race. Topmost layer first; add future layers (pause, tutorial overlay) HERE,
            // above the scene rule, not as new Update listeners.
            var detail = CardDetailViewController.Instance;
            if (detail != null && detail.IsVisible)
            {
                detail.Hide();
                return;
            }

            // [PAUSE-1] Layers 2 and 3, still above the scene rule:
            // 2) any open Options panel (pause-hosted or Main-Menu-hosted) closes first, so
            //    Esc on the Main Menu with Options open does NOT quit (ST-CF3-15 holds);
            // 3) the pause menu: cancels its confirm, resumes, or opens the pause in a gig.
            //    Outside a gig it declines and the scene rule below runs unchanged.
            if (OptionsPanelController.TryCloseTopmost()) return;
            // [FEEDBACK-2] The feedback panel sits on the pause like Options: Esc closes it
            // (text kept) and goes back to the pause menu, never resumes the gig directly.
            if (FeedbackPanelController.TryCloseOpen()) return;

            var pause = PauseMenuController.Instance;
            if (pause != null && pause.HandleEscape()) return;

            int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;

            Debug.Log($"[UIManager] HandleEscapeKey -> activeSceneIndex={activeSceneIndex}, " +
                $"mainMenuSceneIndex={mainMenuSceneIndex}");

            if (activeSceneIndex == mainMenuSceneIndex)
            {
                QuitGame();
            }
            else
            {
                ChangeScene(mainMenuSceneIndex);
            }
        }

        public void QuitGame()
        {
            Debug.Log("[UIManager] Quitting game.");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public void SetupEncounterUI(GigEncounter encounter)
        {

        }

        public void SetCanvas(CanvasBase targetCanvas, bool open, bool reset = false)
        {
            if (reset)
                targetCanvas.ResetCanvas();

            if (open)
                targetCanvas.OpenCanvas();
            else
                targetCanvas.CloseCanvas();
        }

        public void ChangeScene(int index)
        {
            StartCoroutine(ChangeSceneRoutine(index));
        }

        public void OpenCardsInventory(List<CardDefinition> cardList, string title)
        {
            SetCanvas(InventoryCanvas, true, true);
            InventoryCanvas.ChangeTitle(title);
            InventoryCanvas.SetCards(cardList);
        }

        public void OpenSongsInventory(List<SongData> songList, string title)
        {
            SetCanvas(InventoryCanvas, true, true);
            InventoryCanvas.ChangeTitle(title);
            InventoryCanvas.SetSongs(songList);
        }

        private IEnumerator ChangeSceneRoutine(int index)
        {
            SceneManager.LoadScene(index);
            yield return StartCoroutine(Fade(false));
        }

        public IEnumerator Fade(bool isIn)
        {
            // [§5.3.5] Fix: previous version's `if (timer >= 1f) break;` only
            // exited on Fade(true). Fade(false) decremented timer from 1 toward
            // 0 (and negative) without ever hitting the exit condition, so the
            // coroutine never terminated — it kept writing fader.alpha every
            // frame until the host MonoBehaviour was destroyed. CanvasGroup
            // clamping to [0,1] masked the visual effect, but stale Fade(false)
            // coroutines accumulated across scene transitions and fought live
            // Fade(true) coroutines, contributing to the messy frame-by-frame
            // behaviour seen during the auto-start path investigation.
            var waitFrame = new WaitForEndOfFrame();
            var timer = isIn ? 0f : 1f;

            while (true)
            {
                timer += Time.deltaTime * (isIn ? fadeSpeed : -fadeSpeed);

                fader.alpha = timer;

                if (isIn ? timer >= 1f : timer <= 0f) break;

                yield return waitFrame;
            }

            // Snap to exact endpoint so the CanvasGroup doesn't sit at e.g.
            // 1.003 / -0.002 indefinitely.
            fader.alpha = isIn ? 1f : 0f;
        }
    }
}
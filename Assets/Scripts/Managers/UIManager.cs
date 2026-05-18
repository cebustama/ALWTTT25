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
        }

        // [B3-demo-polish / D-UX3=D] Simple two-state ESC behavior:
        // - Not on main menu: return to main menu.
        // - On main menu: quit the game.
        // TODO (post-demo): replace with proper pause menu (D-UX3=A).
        private void HandleEscapeKey()
        {
            int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;

            Debug.Log($"[UIManager] HandleEscapeKey → activeSceneIndex={activeSceneIndex}, " +
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
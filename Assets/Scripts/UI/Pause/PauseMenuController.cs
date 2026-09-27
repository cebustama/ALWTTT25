using ALWTTT.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ALWTTT.UI
{
    /// <summary>
    /// [PAUSE-1] Pause menu + pause model. Lives on PauseCanvas, a child of the UIManager
    /// prefab (DontDestroyOnLoad), with its own Canvas sorted above the card detail modal
    /// (D3=B). Esc reaches it only through UIManager.HandleEscapeKey (single Esc listener,
    /// CARD-FACE-3 chain); this class never polls input.
    ///
    /// Pause model (D1=A, D2=A): gameplay AND music freeze and resume where they were, at
    /// any moment of a gig, including mid-song.
    ///   - Time.timeScale = 0          -> scaled coroutines, Time.time, Time.deltaTime freeze
    ///                                   (turn waits, loop clock, beat grid, animations).
    ///   - PauseGate.IsPaused = true   -> systems that read input in Update stop (hand).
    ///   - MidiMusicManager.SuspendPlayback -> sequencer frozen in place (MPTK_Pause) and
    ///                                   non-MPTK music sources (singer) paused.
    ///   - AudioManager.PauseAmbience  -> crowd loop paused, fade/duck state kept (D11=C).
    /// Resume undoes the same four in the same frame. Unscaled fades (OST, ambience) keep
    /// running by design.
    ///
    /// Exits that must never leave the game frozen: Resume, "Volver al menu" (confirmed,
    /// D5=B), any non-additive scene load, and destruction of this object.
    /// </summary>
    public sealed class PauseMenuController : MonoBehaviour
    {
        public static PauseMenuController Instance { get; private set; }

        [Header("Panels")]
        [SerializeField, Tooltip("Full-screen root (raycast blocker + panels). Hidden when " +
            "not paused.")]
        private GameObject root;
        [SerializeField, Tooltip("Continuar / Opciones / Volver al menu.")]
        private GameObject mainPanel;
        [SerializeField, Tooltip("Confirmation for Volver al menu (D5=B).")]
        private GameObject confirmPanel;
        [SerializeField, Tooltip("Instance of the shared OptionsPanel prefab (D4=B).")]
        private OptionsPanelController options;

        [Header("Debug")]
        [SerializeField, Tooltip("One [PAUSE-1] line per pause/resume (smoke-test evidence).")]
        private bool logPause = true;

        private const string Tag = "[PAUSE-1]";
        private float _prevTimeScale = 1f;

        public bool IsPaused => PauseGate.IsPaused;

        #region Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            HideAll();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (PauseGate.IsPaused) ResumeInternal("destroyed");
            Instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) return;
            // Safety net: a scene change must never inherit a frozen game or an open menu.
            if (options != null) options.Close(invokeCallback: false);
            HideAll();
            if (PauseGate.IsPaused) ResumeInternal("scene-loaded");
        }
        #endregion

        #region Esc layer (called by UIManager.HandleEscapeKey)
        /// <summary>
        /// Returns true when Esc was consumed. Order inside this layer: cancel the confirm
        /// -> resume -> open the pause (gig only). Options panels are closed one layer
        /// above (OptionsPanelController.TryCloseTopmost).
        /// </summary>
        public bool HandleEscape()
        {
            if (!PauseGate.IsPaused)
            {
                if (!CanPauseHere()) return false;   // menu, setup, map: scene rule applies
                Pause();
                return true;
            }

            if (confirmPanel != null && confirmPanel.activeSelf)
            {
                OnConfirmNoPressed();
                return true;
            }

            Resume();
            return true;
        }

        private static bool CanPauseHere() => GigManager.Instance != null;
        #endregion

        #region Pause / resume
        public void Pause()
        {
            if (PauseGate.IsPaused) return;

            _prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            PauseGate.Set(true);

            // A card held outside the hand would otherwise stay glued to the cursor and
            // miss its mouse-up while paused; the next click after resume would drop it.
            var deck = DeckManager.Instance;
            if (deck != null && deck.HandController != null)
                deck.HandController.ReleaseHeldCardToHand();

            if (MidiMusicManager.Instance != null) MidiMusicManager.Instance.SuspendPlayback();
            if (AudioManager.Instance != null) AudioManager.Instance.PauseAmbience();

            ShowMain();
            Log("PAUSE");
        }

        public void Resume()
        {
            if (!PauseGate.IsPaused) return;
            if (options != null) options.Close(invokeCallback: false);
            HideAll();
            ResumeInternal("resume");
        }

        private void ResumeInternal(string why)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.ResumeAmbience();
            if (MidiMusicManager.Instance != null) MidiMusicManager.Instance.ResumePlayback();
            Time.timeScale = _prevTimeScale;
            PauseGate.Set(false);
            Log("RESUME(" + why + ")");
        }
        #endregion

        #region Buttons (wire in the prefab)
        public void OnContinuePressed() => Resume();

        public void OnOptionsPressed()
        {
            if (options == null)
            {
                Debug.LogWarning($"{Tag} Options panel not wired on PauseMenuController.");
                return;
            }
            if (mainPanel != null) mainPanel.SetActive(false);
            options.Open(onClosed: ShowMain);
        }

        public void OnReturnToMenuPressed()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(true);
        }

        public void OnConfirmNoPressed() => ShowMain();

        public void OnConfirmYesPressed()
        {
            HideAll();

            // Stop the gig music BEFORE un-freezing time, so the resume does not replay a
            // slice of the song. Stop also clears the transport suspension.
            if (MidiMusicManager.Instance != null) MidiMusicManager.Instance.Stop();
            ResumeInternal("return-to-menu");

            // Same destination as the pre-PAUSE-1 Esc rule and GigManager.HandleExit.
            // Time is running again, so UIManager.Fade (scaled time) can complete.
            var ui = UIManager.Instance;
            if (ui != null) ui.ChangeScene(ui.MainMenuSceneIndex);
            else SceneManager.LoadScene(0);
        }
        #endregion

        #region Panels
        private void ShowMain()
        {
            if (root != null) root.SetActive(true);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);
        }

        private void HideAll()
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(false);
            if (root != null) root.SetActive(false);
        }
        #endregion

        private void Log(string evt)
        {
            if (!logPause) return;
            var mmm = MidiMusicManager.Instance;
            var gig = GigManager.Instance;
            Debug.Log($"{Tag} {evt} " +
                $"tick={(mmm != null ? mmm.GetCurrentTick() : -1L)} " +
                $"suspended={(mmm != null && mmm.IsPlaybackSuspended)} " +
                $"songPlaying={(gig != null && gig.IsSongPlayingNow)} " +
                $"phase={(gig != null ? gig.CurrentGigPhase.ToString() : "-")} " +
                $"timeScale={Time.timeScale:0.##}");
        }
    }
}
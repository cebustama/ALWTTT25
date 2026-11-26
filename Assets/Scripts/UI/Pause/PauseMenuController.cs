using ALWTTT.Data;
using ALWTTT.Managers;
using System;
using System.IO;
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
        [SerializeField, Tooltip("[FEEDBACK-2] Feedback panel (built by ALWTTT ▸ FEEDBACK-2 ▸ Crear panel).")]
        private FeedbackPanelController feedbackPanel;

        [Header("[S10] SFX de pausa")]
        [SerializeField, Tooltip("Plays on every pause (Space and the Esc menu): a vinyl " +
            "stopping or a scratch, so the silence is not abrupt. Empty = silent.")]
        private AudioClip pauseSfx;
        [SerializeField, Tooltip("Optional: plays on resume (a vinyl spinning up). Empty = silent.")]
        private AudioClip resumeSfx;
        [SerializeField, Range(0f, 1f), Tooltip("[S10] Volume of the pause/resume SFX, on top of the SFX slider.")]
        private float pauseSfxVolume = 0.35f;
        [SerializeField, Range(10f, 22000f), Tooltip("[S10] Low-pass cutoff (Hz) for the pause/resume SFX. " +
            "Lower = more muffled. 22000 = no filter.")]
        private float pauseSfxLowPassHz = 1800f;
        [SerializeField, Range(0.5f, 3f), Tooltip("[S10] Playback speed of the pause/resume SFX. " +
            "Above 1 = shorter and higher (like speeding up a tape).")]
        private float pauseSfxPitch = 1.4f;

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

            // [S10 / D9] Esc during the Space pause opens the menu; the world stays frozen.
            if (PauseGate.InspectOnly)
            {
                PauseGate.SetInspectOnly(false);
                ShowBanner(false);
                ShowMain();
                Log("PAUSE (Espacio -> menu)");
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
            Freeze();
            ShowMain();
            Log("PAUSE");
        }

        // The four freezes of the pause model, shared by the menu pause and the Space pause.
        private void Freeze()
        {
            _prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            PauseGate.Set(true);
            PlayPauseSfx(pauseSfx, "pausa");

            // A card held outside the hand would otherwise stay glued to the cursor and
            // miss its mouse-up while paused; the next click after resume would drop it.
            var deck = DeckManager.Instance;
            if (deck != null && deck.HandController != null)
                deck.HandController.ReleaseHeldCardToHand();

            if (MidiMusicManager.Instance != null) MidiMusicManager.Instance.SuspendPlayback();
            if (AudioManager.Instance != null) AudioManager.Instance.PauseAmbience();
        }

        // [S10] Own AudioSource with a low-pass filter, so the pause sound is muffled and has
        // its own volume without touching the shared SFX source. A one-shot does not depend
        // on Time.timeScale. Volume = pauseSfxVolume x the player's SFX slider.
        // The clips are preloaded in Start: with «Preload Audio Data» off, the first play
        // waits for the clip to load, which is the small delay heard on pause.
        private AudioSource _pauseSfxSource;
        private AudioLowPassFilter _pauseSfxLowPass;

        private void Start()
        {
            if (pauseSfx != null) pauseSfx.LoadAudioData();
            if (resumeSfx != null) resumeSfx.LoadAudioData();
        }

        private void PlayPauseSfx(AudioClip clip, string what)
        {
            if (clip == null) return;
            if (_pauseSfxSource == null)
            {
                _pauseSfxSource = gameObject.AddComponent<AudioSource>();
                _pauseSfxSource.playOnAwake = false;
                _pauseSfxSource.spatialBlend = 0f;
                _pauseSfxSource.ignoreListenerPause = true;
                _pauseSfxLowPass = gameObject.AddComponent<AudioLowPassFilter>();
            }
            _pauseSfxLowPass.cutoffFrequency = pauseSfxLowPassHz;
            _pauseSfxSource.pitch = pauseSfxPitch;
            float vol = Mathf.Clamp01(pauseSfxVolume * PlayerAudioPrefs.Sfx01);
            _pauseSfxSource.PlayOneShot(clip, vol);
            Debug.Log($"[S10] SFX de {what}: {clip.name} | vol {vol:0.00} | paso bajo {pauseSfxLowPassHz:0} Hz | velocidad x{pauseSfxPitch:0.00}");
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
            bool wasInspect = PauseGate.InspectOnly;
            ShowBanner(false);
            if (AudioManager.Instance != null) AudioManager.Instance.ResumeAmbience();
            if (MidiMusicManager.Instance != null) MidiMusicManager.Instance.ResumePlayback();
            Time.timeScale = _prevTimeScale;
            PauseGate.Set(false);
            PlayPauseSfx(resumeSfx, "reanudar");
            Log("RESUME(" + why + ")");
            if (wasInspect) InspectPauseChanged?.Invoke(false, why);
        }
        #endregion

        #region [S10 / D9] Space pause: frozen world, the hand can still be looked at
        /// <summary>(on, who): "jugador" (Space) or a tutorial dialog id.</summary>
        public static event Action<bool, string> InspectPauseChanged;

        /// <summary>Space, from UIManager (the single key listener). True when consumed.</summary>
        public bool HandleSpace()
        {
            if (!CanPauseHere()) return false;
            if (!PauseGate.IsPaused) { PauseInspect("jugador"); return true; }
            if (PauseGate.InspectOnly) { ResumeInspect("jugador"); return true; }
            return false;   // the menu is open: Space does nothing there
        }

        public void PauseInspect(string who)
        {
            if (PauseGate.IsPaused) return;
            Freeze();
            PauseGate.SetInspectOnly(true);
            ShowBanner(true);
            Log("PAUSA-ESPACIO(" + who + ")");
            Debug.Log($"[S10] Pausa con Espacio ON ({who}): todo quieto, la mano se puede mirar.");
            InspectPauseChanged?.Invoke(true, who);
        }

        public void ResumeInspect(string who)
        {
            if (!PauseGate.IsPaused || !PauseGate.InspectOnly) return;
            Debug.Log($"[S10] Pausa con Espacio OFF ({who}).");
            ResumeInternal(who);
        }

        private GameObject _banner;

        private void ShowBanner(bool on)
        {
            if (on && _banner == null) _banner = BuildBanner();
            if (_banner != null) _banner.SetActive(on);
        }

        // Built by code (no prefab edit): a non-blocking strip near the top of the screen.
        private GameObject BuildBanner()
        {
            var go = new GameObject("S10 Pausa Espacio", typeof(RectTransform), typeof(Canvas));
            // [S10] Root object: a nested canvas ignores its own scaler and follows the
            // parent's rect (it showed mid-screen). Destroyed with this controller.
            go.transform.SetParent(null, false);
            DontDestroyOnLoad(go);   // lives as long as this (persistent) controller
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 400;
            var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var bg = new GameObject("Fondo", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var bgRt = (RectTransform)bg.transform;
            bgRt.SetParent(go.transform, false);
            bgRt.anchorMin = new Vector2(0.5f, 1f);
            bgRt.anchorMax = new Vector2(0.5f, 1f);
            bgRt.pivot = new Vector2(0.5f, 1f);
            bgRt.anchoredPosition = new Vector2(0f, -70f);   // above the loop bars
            bgRt.sizeDelta = new Vector2(820f, 130f);
            var img = bg.GetComponent<UnityEngine.UI.Image>();
            img.color = new Color(0f, 0f, 0f, 0.72f);
            img.raycastTarget = false;

            var txtGo = new GameObject("Texto", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            var tRt = (RectTransform)txtGo.transform;
            tRt.SetParent(bgRt, false);
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = tRt.offsetMax = Vector2.zero;
            var tmp = txtGo.GetComponent<TMPro.TextMeshProUGUI>();
            tmp.text = "<size=64><b>PAUSA</b></size>\n<size=28>mira tus cartas con calma  ·  <b>Espacio</b> para seguir</size>";
            tmp.fontSize = 28f;
            tmp.lineSpacing = -10f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return go;
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

        // [FEEDBACK-2] The button opens the in-game panel (posts to a Google Form). The S9c
        // mailto: is gone: on machines without a mail client it only opened the browser.
        public void OnFeedbackPressed()
        {
            if (feedbackPanel == null)
            {
                Debug.LogWarning($"{FeedbackForm.Tag} Feedback panel not wired on PauseMenuController.");
                return;
            }
            if (mainPanel != null) mainPanel.SetActive(false);
            feedbackPanel.Open(onClosed: ShowMain);
        }

        /// <summary>
        /// [S9c / D6=C, kept by FEEDBACK-2 D3=B] Fallback when the form cannot be reached:
        /// address, subject and body (message + context + telemetry folder) for the clipboard.
        /// </summary>
        public static string BuildMailFallback(string message)
        {
            string subject = $"[ALWTTT demo {Application.version}] Feedback";
            string body =
                (string.IsNullOrWhiteSpace(message) ? "Cuéntanos qué pasó / Tell us what happened:\n\n" : message.Trim()) +
                "\n\n---\n" +
                $"Versión / Version: {Application.version}\n" +
                $"Fecha / Date: {DateTime.Now:yyyy-MM-dd HH:mm}\n" +
                $"Sistema / OS: {SystemInfo.operatingSystem}\n" +
                "Si puedes, adjunta el último gig_runs_*.jsonl de / If you can, attach the latest gig_runs_*.jsonl from:\n" +
                TelemetryFolder() + "\n";
            return $"{FeedbackAddress}\n{subject}\n\n{body}";
        }

        public void OnOpenTelemetryFolderPressed()
        {
            string folder = TelemetryFolder();
            try { Directory.CreateDirectory(folder); }
            catch (Exception e) { Debug.LogWarning($"[S9c] No se pudo crear {folder}: {e.Message}"); }
            Debug.Log($"[S9c] Abriendo la carpeta de telemetría: {folder}");
            Application.OpenURL(new Uri(folder).AbsoluteUri);
        }

        public const string FeedbackAddress = "allwtttcmm@gmail.com";

        private static string TelemetryFolder()
        {
#if ALWTTT_DEV
            return ALWTTT.DevMode.DevRunTelemetryLogger.OutputFolder;
#else
            return Path.Combine(Application.persistentDataPath, "DevTelemetry");
#endif
        }

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
            if (feedbackPanel != null) feedbackPanel.Close(invokeCallback: false);
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
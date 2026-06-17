using ALWTTT.Data;
using ALWTTT.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ALWTTT.Managers
{
    /// <summary>
    /// [AUDIO-OST / D-OST-HOME=B] Dedicated singleton owning OST (authored-clip) music.
    /// One OST track audible at a time. Output is scaled by the app-wide Music level
    /// (AudioMixSettingsSO.GlobalMusicVolume01) — the SAME level that scales gig music
    /// (per-channel via GigManager.ComputeEffectiveMusicianVolume01). One Music level, two
    /// consumers (gig music + OST); see SSoT_Audio §4.
    ///
    /// Gig-vs-OST split (kept on purpose): MidiMusicManager owns generated "gig music";
    /// MusicDirector owns authored "OST music". They never both play music — a scene that
    /// plays gig music is simply NOT listed below (unlisted → OstTrackId.None → OST stops),
    /// so entering a gig fades the menu OST out.
    ///
    /// Crossfade (D2=A): this director owns TWO AudioSources that ping-pong, so CrossfadeTo
    /// is a real overlap, not a fade-out-then-in. There is no AudioMixer in ALWTTT, so the
    /// per-source volume IS the level: volume = musicLevel01 * track.defaultLevel01.
    ///
    /// Scene reaction (D3=A): subscribes to SceneManager.sceneLoaded and consults a serialized
    /// scene→track map (build-index keyed). No dependency on SceneChanger / SceneData — robust
    /// to every entry path (Start button, ESC-return-to-menu, gig restart).
    ///
    /// Placement: put ONE MusicDirector GameObject in the first-loaded scene (Main Menu, build
    /// index 0 per UIManager.mainMenuSceneIndex). DontDestroyOnLoad carries it across scenes;
    /// the singleton guard destroys any duplicates placed in later scenes.
    ///
    /// Lives in Managers/ alongside AudioManager + MidiMusicManager (D3).
    /// Authority: SSoT_Audio.md §4 (OST bus).
    /// </summary>
    public sealed class MusicDirector : MonoBehaviour
    {
        public enum OstTransition { Crossfade, HardCut }

        [System.Serializable]
        public struct SceneOstBinding
        {
            [Tooltip("Scene BUILD INDEX (match Build Settings). Main Menu is 0 per " +
                     "UIManager.mainMenuSceneIndex.")]
            public int sceneBuildIndex;
            public OstTrackId track;
            public OstTransition transition;
        }

        public static MusicDirector Instance { get; private set; }

        [Header("Content")]
        [SerializeField] private OstCatalogSO catalog;

        [SerializeField, Tooltip("Music level source — the SAME AudioMixSettings asset the Dev " +
            "Audio Mix tab edits. If null, OST plays unscaled at 1.0 (mix still works; " +
            "D-MIX-FALLBACK parity with AudioManager).")]
        private AudioMixSettingsSO audioMix;

        [Header("Scene → OST map (build-index keyed)")]
        [SerializeField, Tooltip("Scenes NOT listed here default to OstTrackId.None (OST stops). " +
            "This guarantees gig scenes (which play gig music) have no OST overlap. Wire the Main " +
            "Menu (build index 0) → MainMenu here.")]
        private List<SceneOstBinding> sceneBindings = new();

        [Header("Transition")]
        [SerializeField, Range(0f, 5f), Tooltip("Default crossfade / fade-out length (seconds).")]
        private float defaultCrossfadeSeconds = 0.75f;

        [SerializeField, Tooltip("Verbose OST logging (diagnostic).")]
        private bool logOst = false;

        private const string Tag = "[MusicDirector]";

        private AudioSource _a;
        private AudioSource _b;
        private AudioSource _active;                 // currently audible (or fading in)
        private OstTrackId _currentId = OstTrackId.None;
        private float _currentTrackLevel01 = 1f;
        private Coroutine _transition;

        #region Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            transform.parent = null;
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _a = gameObject.AddComponent<AudioSource>();
            _b = gameObject.AddComponent<AudioSource>();
            ConfigureSource(_a);
            ConfigureSource(_b);
            _active = _a;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            // sceneLoaded does NOT fire for the scene already active when this object spawned,
            // so evaluate the active scene once on boot (first-launch Main Menu song). Use an
            // immediate start here (no fade-from-silence on the very first frame).
            ApplyForScene(SceneManager.GetActiveScene().buildIndex, immediateOnFirst: true);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                Instance = null;
            }
        }

        private static void ConfigureSource(AudioSource s)
        {
            s.playOnAwake = false;
            s.spatialBlend = 0f;   // 2D (UI/menu music)
            s.volume = 0f;
        }
        #endregion

        #region Scene reaction
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) return;   // ignore overlay loads
            ApplyForScene(scene.buildIndex, immediateOnFirst: false);
        }

        private void ApplyForScene(int buildIndex, bool immediateOnFirst)
        {
            OstTrackId target = OstTrackId.None;
            OstTransition transition = OstTransition.Crossfade;

            for (int i = 0; i < sceneBindings.Count; i++)
            {
                if (sceneBindings[i].sceneBuildIndex == buildIndex)
                {
                    target = sceneBindings[i].track;
                    transition = sceneBindings[i].transition;
                    break;
                }
            }

            if (logOst)
                Debug.Log($"{Tag} scene {buildIndex} → {target} ({transition})");

            if (target == OstTrackId.None) { Stop(immediate: false); return; }

            // Already playing the target → keep it (no restart on re-entry of the same scene).
            if (target == _currentId && _active != null && _active.isPlaying) return;

            PlayInternal(target, hardCut: transition == OstTransition.HardCut || immediateOnFirst);
        }
        #endregion

        #region Public API
        public OstTrackId CurrentTrack => _currentId;

        /// <summary>Play an OST track. hardCut=true switches immediately; otherwise crossfades.</summary>
        public void Play(OstTrackId id, bool hardCut = false) => PlayInternal(id, hardCut);

        /// <summary>Crossfade to a track over <paramref name="seconds"/> (null → default).</summary>
        public void CrossfadeTo(OstTrackId id, float? seconds = null) =>
            PlayInternal(id, hardCut: false, seconds);

        /// <summary>Stop OST. immediate=true cuts; otherwise fades out over the default length.</summary>
        public void Stop(bool immediate = false)
        {
            _currentId = OstTrackId.None;

            if (immediate)
            {
                StopTransition();
                if (_a != null) { _a.Stop(); _a.volume = 0f; }
                if (_b != null) { _b.Stop(); _b.volume = 0f; }
                return;
            }

            if (_active != null && _active.isPlaying)
                StartTransition(FadeOutRoutine(_active, defaultCrossfadeSeconds));
        }

        /// <summary>
        /// Re-apply the Music level to the audible OST track. No-op if nothing is playing.
        /// Called by GigManager.DevSetGlobalMusicVolume01 so a live Music-slider drag updates a
        /// currently-playing OST track. (The Dev Audio Mix tab is gig-only, so this only bites
        /// when gig-scene OST exists; for the menu song the level is read at play time.)
        /// </summary>
        public void RefreshMusicLevel()
        {
            if (_active != null && _active.isPlaying)
                _active.volume = TargetVolume();
        }
        #endregion

        #region Internal playback
        private float MusicLevel01() => audioMix != null ? audioMix.GlobalMusicVolume01 : 1f;

        private float TargetVolume() =>
            Mathf.Clamp01(MusicLevel01() * Mathf.Clamp01(_currentTrackLevel01));

        private void PlayInternal(OstTrackId id, bool hardCut, float? seconds = null)
        {
            if (id == OstTrackId.None) { Stop(immediate: hardCut); return; }

            if (catalog == null || !catalog.TryGet(id, out var entry) || entry.clip == null)
            {
                // Content gap, not a crash — consistent with the SFX null-safety invariant.
                Debug.LogWarning($"{Tag} No clip for OstTrackId.{id}; playing nothing. " +
                                 $"(Content gap — wire it on the OstCatalog asset.)");
                return;
            }

            // Idempotent: re-requesting the active track keeps it playing (just refresh level).
            if (id == _currentId && _active != null && _active.isPlaying)
            {
                _currentTrackLevel01 = entry.defaultLevel01;
                _active.volume = TargetVolume();
                return;
            }

            _currentId = id;
            _currentTrackLevel01 = entry.defaultLevel01;

            AudioSource from = _active;
            AudioSource to = (_active == _a) ? _b : _a;

            to.clip = entry.clip;
            to.loop = entry.loop;
            to.volume = hardCut ? TargetVolume() : 0f;
            to.Play();
            _active = to;

            float dur = Mathf.Max(0f, seconds ?? defaultCrossfadeSeconds);
            if (hardCut || dur <= 0f)
            {
                StopTransition();
                to.volume = TargetVolume();
                if (from != null) { from.Stop(); from.volume = 0f; }
            }
            else
            {
                StartTransition(CrossfadeRoutine(from, to, dur));
            }
        }

        private void StartTransition(IEnumerator routine)
        {
            StopTransition();
            _transition = StartCoroutine(routine);
        }

        private void StopTransition()
        {
            if (_transition != null) { StopCoroutine(_transition); _transition = null; }
        }

        // Unscaled time: OST is UI/menu music and must fade independently of gameplay timeScale.
        private IEnumerator CrossfadeRoutine(AudioSource from, AudioSource to, float dur)
        {
            float t = 0f;
            float fromStart = from != null ? from.volume : 0f;

            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float toTarget = TargetVolume();          // re-read so a mid-fade slider drag is honored
                if (to != null) to.volume = toTarget * k;
                if (from != null) from.volume = fromStart * (1f - k);
                yield return null;
            }

            if (to != null) to.volume = TargetVolume();
            if (from != null) { from.Stop(); from.volume = 0f; }
            _transition = null;
        }

        private IEnumerator FadeOutRoutine(AudioSource src, float dur)
        {
            if (src == null) yield break;

            float start = src.volume;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                src.volume = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / dur));
                yield return null;
            }

            src.Stop();
            src.volume = 0f;
            _transition = null;
        }
        #endregion
    }
}
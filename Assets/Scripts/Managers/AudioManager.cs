using ALWTTT.Data;
using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALWTTT.Managers
{
    public class AudioManager : MonoBehaviour
    {
        private AudioManager() { }
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource buttonSource;

        // [S3-audio D-SA-7=B] Central audio inventory (card + sensory profiles +
        // coverage audit). Replaces the former inline List<SoundProfileData>.
        [SerializeField] private SoundBankSO soundBank;
        [SerializeField] private AudioMixSettingsSO audioMix;   // AUDIO-SFX-FIX: app-global SFX level

        [Header("SFX behaviour")]
        [SerializeField, Tooltip("Max random delay (s) added to SFX one-shots that OPT IN to " +
            "jitter (audience-reaction fan-out). 0 = off. Card SFX and single-source cues are " +
            "always immediate. (AUDIO-SFX-FIX #6 / D-SFX-JITTER-SCOPE=B)")]
        private float sfxMaxJitterSeconds = 0.15f;
        [SerializeField, Tooltip("Verbose per-trigger SFX logging (diagnostic).")]
        private bool logSfx = false;

        // ── AUDIO-AMBIENCE (D-AMB-BUS=A) ─────────────────────────────────────────
        // Looping crowd bed. Lives under the SFX group: master SFX scales it
        // (effective = masterSfx × ambienceLevel × fade). The gig drives its
        // lifecycle (present while idle / between songs, ducks under a performing
        // song). Single loop for now (D-AMB-CLIP=A); per-venue would move to a
        // catalogue SO (mirror OstCatalogSO) — NOT into SoundBankSO (one-shot
        // inventory). A missing clip is a content gap (warn-once + no-op).
        [Header("Ambience (SFX group — AUDIO-AMBIENCE)")]
        [SerializeField, Tooltip("Looping crowd-ambience clip. Authored as a seamless loop " +
            "(loop-point click is a clip property, not a system one). Single loop for now " +
            "(D-AMB-CLIP=A). Null → ambience no-ops (content gap).")]
        private AudioClip ambienceClip;
        [SerializeField, Range(0f, 1f), Tooltip("Ambience trim under the master-SFX level. " +
            "Effective = masterSfx × this × fade. Default 1.0.")]
        private float ambienceLevel01 = 1f;
        [SerializeField, Range(0f, 5f), Tooltip("Fade-IN length (s): return at song end / crowd " +
            "present at gig open. Gentle swell — default 1.2.")]
        private float ambienceFadeInSeconds = 1.2f;
        [SerializeField, Range(0f, 5f), Tooltip("Fade-OUT length (s): duck when the band starts a " +
            "song. Quicker so the song reads clearly — default 0.6.")]
        private float ambienceFadeOutSeconds = 0.6f;
        // ── [CROWD-SFX-1] Crowd one-shots (D-CS5=A) ──────────────────────────────
        // Applause / boos / ovation / jeers. Own source, NOT sfxSource: an applause
        // lasts seconds and a one-shot on sfxSource cannot be paused or stopped on its
        // own. Same SFX group as the bed: effective = masterSfx × crowdLevel01 × fade.
        [Header("Crowd one-shots (SFX group — CROWD-SFX-1)")]
        [SerializeField, Range(0f, 1f), Tooltip("Applause/boo trim under the master-SFX level. " +
            "Read at each crowd play, so a Play-mode tweak applies to the next one. " +
            "0 = crowd one-shots silent (A/B against the pre-batch mix).")]
        private float crowdLevel01 = 1f;
        [SerializeField, Min(0f), Tooltip("CROWD-CAP: duración MÁXIMA (s, fundido incluido) " +
            "del veredicto de canción (CrowdApplause / CrowdBoo). Por defecto 2.5: debe caber " +
            "en el hueco de SongEndPause antes del primer +Vibe. 0 = sin tope (el clip entero).")]
        private float crowdVerdictMaxSeconds = 2.5f;

        [SerializeField, Min(0f), Tooltip("CROWD-CAP: duración MÁXIMA (s, fundido incluido) " +
            "del sonido de fin de gig (CrowdOvation / CrowdJeer). 0 = sin tope.")]
        private float crowdOutcomeMaxSeconds = 5f;

        [SerializeField, Min(0.01f), Tooltip("CROWD-CAP: longitud del fundido de salida al " +
            "llegar al tope. Se recorta al tope si es mayor.")]
        private float crowdCapFadeSeconds = 0.5f;

        private readonly Dictionary<AudioActionType, SoundProfileData> audioDict = new();
        private readonly Dictionary<SensorySfxType, SoundBankSO.SensorySoundEntry> sensoryDict = new();

        // Warn-once-per-type so the asset-less phase (D-SA-2) doesn't flood the
        // console — reactions fire per-audience-per-loop. The SoundBankSO coverage
        // audit is the canonical gap view; these are just a runtime breadcrumb.
        private readonly HashSet<AudioActionType> _warnedCard = new();
        private readonly HashSet<SensorySfxType> _warnedSensory = new();

        // AUDIO-AMBIENCE runtime state. ambienceSource is self-provisioned (mirrors
        // MusicDirector's owned sources). _masterSfx01 is cached from SetSfxVolume01
        // (the SFX-group cap); _ambienceFade01 is the 0..1 envelope the fade coroutine
        // drives. Effective volume is recomposed (ApplyAmbienceVolume) whenever any
        // factor changes — so the SFX slider scales ambience for free.
        private AudioSource ambienceSource;
        private float _masterSfx01 = 1f;
        private float _ambienceFade01;
        private Coroutine _ambienceFadeCo;
        private bool _ambienceClipWarned;


        // [PAUSE-1] Design-level master SFX (what SetSfxVolume01 receives from the SO /
        // Dev tab), kept apart from the effective level (design x player plane, D10=B).
        // _ambiencePausedByPause marks a crowd loop paused by the pause menu (D11=C).
        private float _designSfx01 = 1f;
        private bool _ambiencePausedByPause;
        // [CROWD-SFX-1] Crowd one-shot runtime state (self-provisioned in Awake).
        private AudioSource crowdSource;
        private float _crowdFade01 = 1f;
        private Coroutine _crowdStopCo;
        private bool _crowdPausedByPause;
        private Coroutine _crowdCapCo;   // CROWD-CAP: playhead-driven cap + fade

        #region Setup
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            transform.parent = null;
            Instance = this;
            DontDestroyOnLoad(gameObject);

            PlayerAudioPrefs.Changed += OnPlayerAudioPrefsChanged;   // [PAUSE-1 / D10=B]

            BuildDictionaries();

            // AUDIO-AMBIENCE: provision the ambience source BEFORE the SFX apply below,
            // so the cached master-SFX cap composes onto it from boot.
            ProvisionAmbienceSource();
            ProvisionCrowdSource();   // [CROWD-SFX-1] before the SFX apply, like the bed

            // AUDIO-SFX-FIX (D-SFX-APPLY=A): apply the persisted master SFX level app-wide.
            // AudioManager is DontDestroyOnLoad and lives in every scene, so SFX volume is
            // correct from boot (Main Menu included), not only inside a gig. GigManager
            // re-applies at gig start; the Dev tab drives it live.
            if (audioMix != null)
                SetSfxVolume01(audioMix.MasterSfxVolume01);
        }

        private void BuildDictionaries()
        {
            audioDict.Clear();
            sensoryDict.Clear();

            if (soundBank == null)
            {
                Debug.LogWarning(
                    "[AudioManager] No SoundBankSO assigned; all audio will no-op. " +
                    "Assign one on the AudioManager component.");
                return;
            }

            foreach (AudioActionType t in Enum.GetValues(typeof(AudioActionType)))
                audioDict[t] = soundBank.CardProfiles?
                    .FirstOrDefault(x => x != null && x.AudioType == t);

            foreach (SensorySfxType t in Enum.GetValues(typeof(SensorySfxType)))
                sensoryDict[t] = soundBank.SensoryProfiles?
                    .FirstOrDefault(x => x != null && x.Type == t);
        }
        #endregion

        #region Public Methods

        // [S3-audio] Card-action audio. Null-safe: a missing profile or an empty
        // clip list is a CONTENT gap (clip not authored yet, D-SA-2), so warn once
        // and no-op — never throw. None is explicit silence (no warning).
        // Card SFX is ALWAYS immediate (tight to the click) — never jittered.
        public void PlayOneShot(AudioActionType type)
        {
            if (type == AudioActionType.None) return;
            audioDict.TryGetValue(type, out var profile);
            var clip = profile != null ? profile.GetRandomClip() : null;
            if (logSfx)
                Debug.Log($"[AudioManager][SFX] card AudioType={type} → " +
                          $"clip={(clip ? clip.name : "<none>")}");
            if (clip == null) { WarnCardOnce(type); return; }
            sfxSource.PlayOneShot(clip);                 // immediate (card feedback)
        }

        // Default sensory one-shot is IMMEDIATE. Fan-out callers (audience reactions)
        // use the jitter overload below.
        public void PlayOneShot(SensorySfxType type) => PlayOneShot(type, jitter: false);

        // AUDIO-SFX-FIX (D-SFX-JITTER-SCOPE=B): sensory one-shot. jitter=true spreads
        // simultaneous fan-out (one loop → many audience members → many one-shots on the
        // same frame). Single-source cues (song-end, stage cross) and card SFX pass
        // jitter=false. Jitter is the CALLER's decision, not the sink's.
        public void PlayOneShot(SensorySfxType type, bool jitter)
        {
            sensoryDict.TryGetValue(type, out var entry);
            var clip = entry != null ? entry.GetRandomClip() : null;
            if (logSfx)
                Debug.Log($"[AudioManager][SFX] sensory Type={type} jitter={jitter} → " +
                          $"clip={(clip ? clip.name : "<none>")}");
            if (clip == null) { WarnSensoryOnce(type); return; }
            if (jitter) PlayOneShotWithJitter(clip);
            else sfxSource.PlayOneShot(clip);
        }

        public void PlayOneShot(AudioClip clip)
        {
            if (clip) sfxSource.PlayOneShot(clip);        // immediate
        }

        // AUDIO-CHAR-PROFILES (D-CHAR-SFX-SEAM): play a pre-resolved clip with optional
        // jitter. Lets the per-character reaction path reuse the SAME fan-out jitter as
        // the SensorySfxType reaction path — only the clip SOURCE changes (a character
        // profile clip vs the global bank); the staggering is identical (inv.10). The
        // resolver (SensoryAudioAdapter) decides jitter; the sink stays dumb and learns
        // nothing about characters.
        public void PlayOneShot(AudioClip clip, bool jitter)
        {
            if (clip == null) return;
            if (logSfx)
                Debug.Log($"[AudioManager][SFX] clip={clip.name} jitter={jitter} (pre-resolved)");
            if (jitter) PlayOneShotWithJitter(clip);   // routes to immediate if sfxMaxJitterSeconds <= 0
            else sfxSource.PlayOneShot(clip);
        }

        // AUDIO-SFX-FIX #6: small random delay so opted-in simultaneous one-shots
        // (a loop's audience reactions) don't stack on the same frame and saturate.
        // Only reached when a caller passes jitter=true. UI clicks (PlayOneShotButton)
        // and card SFX are never routed here.
        private void PlayOneShotWithJitter(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;
            float j = Mathf.Max(0f, sfxMaxJitterSeconds);
            if (j <= 0f) { sfxSource.PlayOneShot(clip); return; }   // jitter off → immediate
            StartCoroutine(DelayedOneShot(clip, UnityEngine.Random.Range(0f, j)));
        }

        private System.Collections.IEnumerator DelayedOneShot(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (sfxSource != null) sfxSource.PlayOneShot(clip);
        }

        public void PlayOneShotButton(AudioClip clip)
        {
            if (clip)
                buttonSource.PlayOneShot(clip);
        }

        // M-AUDIO-MIX / AUDIO-SFX-FIX: master SFX level. Drives sfxSource.volume AND
        // buttonSource.volume (card + sensory SFX + UI clicks). Applied app-wide at
        // Awake from AudioMixSettingsSO, re-applied at gig start, live from the Dev tab.
        // [PAUSE-1] Returns the DESIGN level (what the Dev tab set), not sfxSource.volume,
        // which now also carries the player plane. Identical while the player level is 1.
        public float SfxVolume01 => _designSfx01;

        public void SetSfxVolume01(float volume01)
        {
            _designSfx01 = Mathf.Clamp01(volume01);                     // [PAUSE-1] design level
            ApplySfxLevels();                                            // design x player -> sources
        }
        #endregion

        #region Ambience (SFX group — AUDIO-AMBIENCE)
        public bool IsAmbiencePlaying => ambienceSource != null && ambienceSource.isPlaying;

        /// <summary>
        /// Start (if needed) and fade the crowd ambience IN. Used at gig open (crowd
        /// present) and at song end (return). No-op if no clip is assigned.
        /// </summary>
        public void FadeInAmbience() => FadeAmbienceTo(1f, ambienceFadeInSeconds, startIfStopped: true);

        /// <summary>
        /// Fade the crowd ambience OUT (duck under a performing song). The loop keeps
        /// running silently so the return has NO restart transient — use StopAmbience()
        /// to fully stop (gig exit).
        /// </summary>
        public void FadeOutAmbience() => FadeAmbienceTo(0f, ambienceFadeOutSeconds, startIfStopped: false);

        /// <summary>Set the ambience trim (0..1) under the master-SFX level. Live-safe.</summary>
        public void SetAmbienceLevel01(float level01)
        {
            ambienceLevel01 = Mathf.Clamp01(level01);
            ApplyAmbienceVolume();
        }

        /// <summary>
        /// Fade out then fully stop the ambience (gig exit). The coroutine runs on this
        /// DontDestroyOnLoad object, so the fade completes across the gig-scene unload
        /// instead of cutting abruptly.
        /// </summary>
        public void StopAmbience()
        {
            if (ambienceSource == null) return;
            StartAmbienceFade(StopAmbienceRoutine(ambienceFadeOutSeconds));
            StopCrowd(ambienceFadeOutSeconds);   // [CROWD-SFX-1] no applause bleeds out of the gig
        }

        private void ProvisionAmbienceSource()
        {
            // Self-provisioned (mirrors MusicDirector) so there is no inspector-wiring
            // failure mode — the only content to assign is the clip.
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.playOnAwake = false;
            ambienceSource.loop = true;
            ambienceSource.spatialBlend = 0f;   // 2D crowd bed
            ambienceSource.clip = ambienceClip;
            ambienceSource.volume = 0f;
            _ambienceFade01 = 0f;
        }

        private void ApplyAmbienceVolume()
        {
            if (ambienceSource == null) return;
            ambienceSource.volume =
                Mathf.Clamp01(_masterSfx01) *
                Mathf.Clamp01(ambienceLevel01) *
                Mathf.Clamp01(_ambienceFade01);
        }

        private void FadeAmbienceTo(float targetFade01, float seconds, bool startIfStopped)
        {
            if (ambienceSource == null) return;

            if (ambienceSource.clip == null)
            {
                // Content gap, not a crash — consistent with the SFX null-safety invariant.
                if (!_ambienceClipWarned)
                {
                    _ambienceClipWarned = true;
                    Debug.LogWarning(
                        "[AudioManager] No ambience clip assigned; ambience no-ops. " +
                        "(Content gap — assign 'ambienceClip' on the AudioManager.)");
                }
                return;
            }

            if (startIfStopped && !ambienceSource.isPlaying)
                ambienceSource.Play();

            StartAmbienceFade(FadeAmbienceRoutine(targetFade01, seconds));
        }

        private void StartAmbienceFade(System.Collections.IEnumerator routine)
        {
            if (_ambienceFadeCo != null) StopCoroutine(_ambienceFadeCo);
            _ambienceFadeCo = StartCoroutine(routine);
        }

        // Unscaled time so a paused gig (timeScale == 0) doesn't freeze a duck/return
        // mid-fade (parity with MusicDirector's unscaled OST fades).
        private System.Collections.IEnumerator FadeAmbienceRoutine(float target01, float dur)
        {
            float start = _ambienceFade01;
            target01 = Mathf.Clamp01(target01);
            dur = Mathf.Max(0f, dur);

            if (dur <= 0f)
            {
                _ambienceFade01 = target01;
                ApplyAmbienceVolume();
                _ambienceFadeCo = null;
                yield break;
            }

            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                _ambienceFade01 = Mathf.Lerp(start, target01, Mathf.Clamp01(t / dur));
                ApplyAmbienceVolume();
                yield return null;
            }

            _ambienceFade01 = target01;
            ApplyAmbienceVolume();
            _ambienceFadeCo = null;
        }

        private System.Collections.IEnumerator StopAmbienceRoutine(float dur)
        {
            float start = _ambienceFade01;
            dur = Mathf.Max(0f, dur);

            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                _ambienceFade01 = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / dur));
                ApplyAmbienceVolume();
                yield return null;
            }

            _ambienceFade01 = 0f;
            ApplyAmbienceVolume();
            if (ambienceSource != null) ambienceSource.Stop();
            _ambienceFadeCo = null;
        }
        #endregion

        #region [CROWD-SFX-1] Crowd one-shots (applause / boos / ovation / jeers)
        // D-CS5=A: the crowd's sounds get their OWN self-provisioned source, one voice at a time. They
        // pause, resume and stop with the crowd bed (PauseAmbience / ResumeAmbience /
        // StopAmbience), so the pause caller learns nothing new. AudioManager stays the
        // dumb sink: the caller (SensoryAudioAdapter) decides which key plays.
        public void PlayCrowd(SensorySfxType type)
        {
            sensoryDict.TryGetValue(type, out var entry);
            var clip = entry != null ? entry.GetRandomClip() : null;
            if (logSfx)
                Debug.Log($"[AudioManager][SFX] crowd Type={type} → " +
                          $"clip={(clip ? clip.name : "<none>")}");
            if (clip == null) { WarnSensoryOnce(type); return; }
            if (crowdSource == null) return;

            // CROWD-CAP: a new crowd sound REPLACES the previous one. Silence the old
            // voice BEFORE restoring the level below, so a tail caught mid-cap-fade
            // never jumps back to full volume for an audio block.
            if (_crowdCapCo != null) { StopCoroutine(_crowdCapCo); _crowdCapCo = null; }
            crowdSource.Stop();
            // A pending stop-fade (gig exit) loses to a new crowd sound.
            if (_crowdStopCo != null) { StopCoroutine(_crowdStopCo); _crowdStopCo = null; }
            _crowdFade01 = 1f;
            ApplyCrowdVolume();   // also picks up a Play-mode crowdLevel01 tweak
            // CROWD-CAP: ONE voice (clip + Play), not PlayOneShot. The source's
            // playhead belongs to this clip, which is what the cap reads, and
            // Pause()/UnPause() act on it directly.
            crowdSource.clip = clip;
            crowdSource.Play();

            float max = CrowdMaxSecondsFor(type);
            if (max > 0f && clip.length > max)
                _crowdCapCo = StartCoroutine(CrowdCapRoutine(clip, max));
        }

        // CROWD-CAP: which cap applies is decided by the key group. The sink learns
        // only that the crowd has two groups; it still learns nothing about gameplay.
        private float CrowdMaxSecondsFor(SensorySfxType type) =>
            (type == SensorySfxType.CrowdOvation || type == SensorySfxType.CrowdJeer)
                ? crowdOutcomeMaxSeconds
                : crowdVerdictMaxSeconds;

        // CROWD-CAP: the cap is driven by the clip's PLAYHEAD (crowdSource.time), not by
        // a clock. A paused source does not advance its playhead, so the pause menu
        // freezes the cap and its fade for free, whatever timeScale is doing.
        // The sound is gone BY maxSeconds: the fade occupies [max - fade, max].
        private System.Collections.IEnumerator CrowdCapRoutine(AudioClip clip, float maxSeconds)
        {
            float fade = Mathf.Min(Mathf.Max(0.01f, crowdCapFadeSeconds), maxSeconds);
            float fadeStart = maxSeconds - fade;

            while (crowdSource != null && crowdSource.clip == clip &&
                   (crowdSource.isPlaying || _crowdPausedByPause))
            {
                float into = crowdSource.time - fadeStart;
                if (into >= 0f)
                {
                    _crowdFade01 = Mathf.Clamp01(1f - into / fade);
                    ApplyCrowdVolume();
                    if (_crowdFade01 <= 0f) { crowdSource.Stop(); break; }
                }
                yield return null;
            }

            _crowdFade01 = 1f;   // the next crowd sound starts at full level
            ApplyCrowdVolume();
            _crowdCapCo = null;
        }

        private void ProvisionCrowdSource()
        {
            crowdSource = gameObject.AddComponent<AudioSource>();
            crowdSource.playOnAwake = false;
            crowdSource.loop = false;
            crowdSource.spatialBlend = 0f;   // 2D, like the bed
            _crowdFade01 = 1f;
            ApplyCrowdVolume();
        }

        private void ApplyCrowdVolume()
        {
            if (crowdSource == null) return;
            crowdSource.volume =
                Mathf.Clamp01(_masterSfx01) *
                Mathf.Clamp01(crowdLevel01) *
                Mathf.Clamp01(_crowdFade01);
        }

        // Called from PauseAmbience / ResumeAmbience BEFORE their own guards: the bed
        // may be stopped or already paused while an applause still rings.
        private void PauseCrowd()
        {
            if (crowdSource == null || _crowdPausedByPause) return;
            crowdSource.Pause();   // one voice (clip + Play): freezes playhead, cap and fade
            _crowdPausedByPause = true;
        }

        private void ResumeCrowd()
        {
            if (!_crowdPausedByPause) return;
            _crowdPausedByPause = false;
            if (crowdSource != null) crowdSource.UnPause();
        }

        private void StopCrowd(float fadeSeconds)
        {
            if (crowdSource == null) return;
            // CROWD-CAP: the gig-exit stop owns the fade from here on.
            if (_crowdCapCo != null) { StopCoroutine(_crowdCapCo); _crowdCapCo = null; }
            if (_crowdStopCo != null) StopCoroutine(_crowdStopCo);
            _crowdStopCo = StartCoroutine(StopCrowdRoutine(Mathf.Max(0f, fadeSeconds)));
        }

        // Unscaled, like StopAmbienceRoutine: "Volver al menú" runs at timeScale 0.
        private System.Collections.IEnumerator StopCrowdRoutine(float dur)
        {
            float start = _crowdFade01;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                _crowdFade01 = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / dur));
                ApplyCrowdVolume();
                yield return null;
            }

            if (crowdSource != null) crowdSource.Stop();
            _crowdPausedByPause = false;
            _crowdFade01 = 1f;   // ready for the next gig; the source is stopped (silent)
            ApplyCrowdVolume();
            _crowdStopCo = null;
        }
        #endregion

        #region [PAUSE-1] Pause + player SFX plane
        /// <summary>
        /// D11=C: pause the crowd loop in place. Source-level Pause keeps volume, fade
        /// envelope and loop position, so it is agnostic to whether a song had ducked it.
        /// No-op if the loop is not audible-playing.
        /// </summary>
        public void PauseAmbience()
        {
            PauseCrowd();   // [CROWD-SFX-1] before the bed's guards (inv. 23)
            if (ambienceSource == null || _ambiencePausedByPause) return;
            if (!ambienceSource.isPlaying) return;
            ambienceSource.Pause();
            _ambiencePausedByPause = true;
        }

        /// <summary>Undo PauseAmbience. No-op if the pause menu did not pause it.</summary>
        public void ResumeAmbience()
        {
            ResumeCrowd();  // [CROWD-SFX-1] before the bed's guard
            if (!_ambiencePausedByPause) return;
            _ambiencePausedByPause = false;
            if (ambienceSource != null) ambienceSource.UnPause();
        }

        // Effective SFX-group level = design (SO / Dev tab) x player (Options). Applied to
        // sfx + button sources and recomposed onto ambience, exactly where the pre-PAUSE-1
        // SetSfxVolume01 applied the design level alone.
        private void ApplySfxLevels()
        {
            float effective = Mathf.Clamp01(_designSfx01 * PlayerAudioPrefs.Sfx01);
            _masterSfx01 = effective;                                    // AUDIO-AMBIENCE: SFX-group cap
            if (sfxSource != null) sfxSource.volume = effective;
            if (buttonSource != null) buttonSource.volume = effective;   // AUDIO-SFX-FIX: UI under SFX
            ApplyAmbienceVolume();                                       // AUDIO-AMBIENCE: ambience under SFX
            ApplyCrowdVolume();                                          // [CROWD-SFX-1] crowd under SFX
        }

        private void OnPlayerAudioPrefsChanged() => ApplySfxLevels();

        private void OnDestroy()
        {
            if (Instance == this) PlayerAudioPrefs.Changed -= OnPlayerAudioPrefsChanged;
        }
        #endregion

        #region Warn-once helpers
        // A missing profile / empty clip list is a content gap (D-SA-2), not an error:
        // warn one time per type and no-op. The SoundBankSO "Audit SFX Coverage" menu
        // is the canonical gap view.
        private void WarnCardOnce(AudioActionType type)
        {
            if (_warnedCard.Add(type))
                Debug.LogWarning(
                    $"[AudioManager] No clips for AudioActionType.{type}; playing nothing. " +
                    $"(Authoring gap — SoundBankSO ▸ Audit SFX Coverage.)");
        }

        private void WarnSensoryOnce(SensorySfxType type)
        {
            if (_warnedSensory.Add(type))
                Debug.LogWarning(
                    $"[AudioManager] No clips for SensorySfxType.{type}; playing nothing. " +
                    $"(Authoring gap — SoundBankSO ▸ Audit SFX Coverage.)");
        }
        #endregion
    }
}
using ALWTTT.Enums;
using ALWTTT.Managers;
using UnityEngine;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S3-audio D-SA-4=A / D-SA-5] Thin bus→audio adapter. Subscribes to the
    /// sensory events, resolves each to a SensorySfxType via SensorySfxPresentation,
    /// and plays it through AudioManager — the single playback sink shared with the
    /// card-action path (D-SA-4=A). Mirrors SensoryFxAdapter's lifecycle and
    /// placement: scene-placed in the gig scene (NOT on the DDOL managers object) so
    /// OnEnable/OnDisable exercise subscribe/unsubscribe across scene reloads.
    ///
    /// Audio is the SFX upgrade over the FT floor (D2). FT and audio are independent
    /// bus handlers, so this never affects the FT adapter (per-handler isolation,
    /// D-S3-3=A). With no clips authored (D-SA-2) AudioManager warns once per type
    /// and no-ops — the build stays showable (silent), never crashes.
    ///
    /// AUDIO-SFX-FIX (D-SFX-JITTER-SCOPE=B): only the AudienceReactionEvent path opts
    /// into jitter — one loop fans out to every audience member, so those one-shots
    /// are staggered to avoid a saturated stack on a single frame. Song-end and
    /// stage-cross are single-source events and play immediately.
    ///
    /// AUDIO-CHAR-PROFILES (#5, phase 1): the reaction handler keeps jitter=true and
    /// resolves the clip from the reacting character's CharacterSfxProfileSO first,
    /// falling back to the global SoundBankSO per polarity. Only the clip SOURCE
    /// changed — the staggering and the SensorySfxType reaction keys are unchanged.
    /// </summary>
    public class SensoryAudioAdapter : MonoBehaviour
    {
        [Tooltip("Log each handled event with its resolved SensorySfxType " +
                 "(infra smoke tests ST-SA-6/7). Safe to disable once audio ships.")]
        [SerializeField] private bool logVerification = true;

        public long ReactionEventsHandled { get; private set; }
        public long VibeEventsHandled { get; private set; }
        public long StageEventsHandled { get; private set; }

        private SensoryEventBus _bus;

        private void OnEnable()
        {
            _bus = SensoryEventBus.Instance;
            if (_bus == null)
            {
                Debug.LogWarning(
                    "[SensoryAudioAdapter] No SensoryEventBus available — " +
                    "adapter inactive this session.");
                return;
            }

            _bus.Subscribe<AudienceReactionEvent>(OnAudienceReaction);
            _bus.Subscribe<SongEndVibeEvent>(OnSongEndVibe);
            _bus.Subscribe<SfxStageCrossedEvent>(OnStageCrossed);

            Debug.Log(
                "[SensoryAudioAdapter] Subscribed to bus " +
                "(AudienceReaction + SongEndVibe + SfxStageCrossed).");
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<AudienceReactionEvent>(OnAudienceReaction);
            _bus.Unsubscribe<SongEndVibeEvent>(OnSongEndVibe);
            _bus.Unsubscribe<SfxStageCrossedEvent>(OnStageCrossed);
            _bus = null;
        }

        private void OnAudienceReaction(AudienceReactionEvent e)
        {
            ReactionEventsHandled++;

            // Resolve polarity first. Neutral (impression 0) → null → intentionally
            // silent (the muted "…" FT carries it). Unchanged from S3-audio.
            var polarity = SensorySfxPresentation.ForReaction(e.Impression);
            if (polarity == null)
            {
                if (logVerification)
                    Debug.Log($"[SensoryAudioAdapter] reaction impression={e.Impression} " +
                              "→ no sting (intentionally silent).");
                return;
            }

            // AUDIO-CHAR-PROFILES (D-CHAR-SFX): try the reacting character's profile as
            // the clip SOURCE first; fall back to the global bank PER POLARITY when it
            // has nothing for this polarity (a positive-only profile still gets the
            // bank's negative sting). jitter:true is preserved on BOTH paths — the
            // reaction fan-out staggers regardless of where the clip came from (inv.10).
            var profile = (e.Audience != null && e.Audience.AudienceCharacterData != null)
                ? e.Audience.AudienceCharacterData.SfxProfile
                : null;
            var clip = profile != null ? profile.GetClipFor(polarity.Value) : null;

            if (clip != null)
            {
                if (logVerification)
                    Debug.Log($"[SensoryAudioAdapter] reaction impression={e.Impression} " +
                              $"→ {polarity.Value} via profile '{profile.name}' " +
                              $"clip '{clip.name}' (jitter).");
                AudioManager.Instance?.PlayOneShot(clip, jitter: true);
                return;
            }

            // No per-character clip → global bank for this polarity (the existing path).
            Play(polarity, $"reaction impression={e.Impression} (bank fallback)", jitter: true);
        }

        private void OnSongEndVibe(SongEndVibeEvent e)
        {
            VibeEventsHandled++;
            // Single source → immediate.
            Play(SensorySfxPresentation.ForSongEnd(in e),
                 $"song-end applied={e.AppliedDelta} blocked={e.BlockedByIndifference}");
        }

        private void OnStageCrossed(SfxStageCrossedEvent e)
        {
            StageEventsHandled++;
            // Single source → immediate.
            Play(SensorySfxPresentation.ForStageCross(e.Stage),
                 $"stage={e.Stage} tag={e.SfxTag}");
        }

        private void Play(SensorySfxType? sfx, string context, bool jitter = false)
        {
            if (sfx == null)
            {
                if (logVerification)
                    Debug.Log($"[SensoryAudioAdapter] {context} → no sting (intentionally silent).");
                return;
            }

            if (logVerification)
                Debug.Log($"[SensoryAudioAdapter] {context} → {sfx.Value}{(jitter ? " (jitter)" : "")}.");

            AudioManager.Instance?.PlayOneShot(sfx.Value, jitter);
        }
    }
}
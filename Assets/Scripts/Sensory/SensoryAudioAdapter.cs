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
        public long RewardChoicesOpened { get; private set; }

        /// <summary>[JUICE-PW] Card-impact events handled (ST-PW parity).</summary>
        public long VibeImpactEventsHandled { get; private set; }

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
            _bus.Subscribe<RewardChoiceOpenedEvent>(OnRewardOpened);
            _bus.Subscribe<AudienceVibeImpactEvent>(OnVibeImpact);

            Debug.Log(
                "[SensoryAudioAdapter] Subscribed to bus " +
                "(AudienceReaction + SongEndVibe + SfxStageCrossed + AudienceVibeImpact).");
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<AudienceReactionEvent>(OnAudienceReaction);
            _bus.Unsubscribe<SongEndVibeEvent>(OnSongEndVibe);
            _bus.Unsubscribe<SfxStageCrossedEvent>(OnStageCrossed);
            _bus.Unsubscribe<RewardChoiceOpenedEvent>(OnRewardOpened);
            _bus.Unsubscribe<AudienceVibeImpactEvent>(OnVibeImpact);
            _bus = null;
        }

        /// <summary>
        /// [JUICE-PW D3=A] Card Vibe impact: ONE sting per card play, not one
        /// per AoE target — SensorySfxPresentation.ForCardVibeImpact returns a
        /// key only for FanoutIndex 0 and null for the rest (the per-member
        /// FT wave carries the fan-out visually). No jitter: single-source by
        /// construction. Plays even when the first target was blocked — the
        /// card resolving is the audible moment, per the D2 audio floor.
        /// </summary>
        private void OnVibeImpact(AudienceVibeImpactEvent e)
        {
            VibeImpactEventsHandled++;
            Play(SensorySfxPresentation.ForCardVibeImpact(in e),
                 $"card-vibe-impact card='{e.Card?.DisplayName}' " +
                 $"applied={e.AppliedDelta} fanout={e.FanoutIndex}/{e.TargetCount}");
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

        private void OnRewardOpened(RewardChoiceOpenedEvent e)
        {
            RewardChoicesOpened++;
            // Single source → immediate.
            Play(SensorySfxPresentation.ForRewardOpened(),
                 $"reward opened.");
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
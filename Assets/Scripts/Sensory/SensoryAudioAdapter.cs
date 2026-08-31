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


        /// <summary>[WINK-1] Status-applied events handled (ST-W3/W6 parity).</summary>
        public long StatusAppliedEventsHandled { get; private set; }

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
            _bus.Subscribe<StatusAppliedEvent>(OnStatusApplied);   // [WINK-1]

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
            _bus.Unsubscribe<StatusAppliedEvent>(OnStatusApplied);   // [WINK-1]
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

        /// <summary>
        /// [WINK-1 D2=C] Target beat (4), audible half. Two-layer clip
        /// resolution: receiving character's CharacterSfxProfileSO by StatusKey
        /// -> StatusEffectSO.applySfx -> DELIBERATE silence. No warn on the
        /// silent tail — a status may be mute by design; this diverges from
        /// SSoT_Audio inv.3 (warn-once) ON PURPOSE and is recorded at batch
        /// close, not normalized quietly. No new SensorySfxType key
        /// (precedent D-ABILITY-SFX-HOME=(i)): the clip is played direct,
        /// AudioManager stays a dumb sink.
        ///
        /// jitter:true (D-WINK-3=A): the event carries no fan-out index, so an
        /// AoE status application would stack N identical clips on one frame
        /// without it.
        ///
        /// Profile layer is audience-only for now: MusicianCharacterData has no
        /// sfxProfile slot (out of WINK-1 scope) — musician statuses resolve
        /// SO.applySfx -> silence.
        /// </summary>
        private void OnStatusApplied(StatusAppliedEvent e)
        {
            StatusAppliedEventsHandled++;

            if (e.DeltaStacks <= 0 || e.Effect == null)
            {
                if (logVerification)
                    Debug.Log($"[SensoryAudioAdapter] status={e.Status} " +
                              $"delta={e.DeltaStacks} → no sting (non-positive delta).");
                return;
            }

            // Layer 1: per-character profile, keyed by StatusKey.
            var profile =
                (e.Source?.Owner is ALWTTT.Characters.Audience.AudienceCharacterBase aud
                 && aud.AudienceCharacterData != null)
                    ? aud.AudienceCharacterData.SfxProfile
                    : null;
            var clip = profile != null
                ? profile.GetClipForStatus(e.Effect.StatusKey)
                : null;
            string source = clip != null ? $"profile '{profile.name}'" : null;


            if (clip == null && logVerification)
                Debug.Log($"[SensoryAudioAdapter] status layer-1 miss: " +
                          $"profile={(profile != null ? profile.name : "NONE on character data")} " +
                          $"key='{e.Effect.StatusKey}'.");

            // Layer 2: the status variant's own base clip.
            if (clip == null && e.Effect.ApplySfx != null)
            {
                clip = e.Effect.ApplySfx;
                source = "SO.applySfx";
            }

            // Layer 3: deliberate silence. NOT a warn (diverges from inv.3 on purpose).
            if (clip == null)
            {
                if (logVerification)
                    Debug.Log($"[SensoryAudioAdapter] status '{e.Effect.StatusKey}' " +
                              "→ deliberately silent (no profile override, no applySfx).");
                return;
            }

            if (logVerification)
                Debug.Log($"[SensoryAudioAdapter] status '{e.Effect.StatusKey}' " +
                          $"delta={e.DeltaStacks} → clip '{clip.name}' via {source} (jitter).");

            AudioManager.Instance?.PlayOneShot(clip, jitter: true);   // D-WINK-3=A
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
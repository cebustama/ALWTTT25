using ALWTTT.Managers;
using UnityEngine;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S2 D-S2-6=A] Thin bus->FT adapter. Subscribes to the two S2 event
    /// types and converts them into FloatingText payloads identical to the
    /// S1 direct-call output (single-sourced via SensoryFtPresentation).
    ///
    /// [D-S2-7=A] Dedup during coexistence: default mode is VerifyOnly — the
    /// adapter computes the exact payload and logs it but does NOT spawn,
    /// because the GigManager direct calls (live per D-S2-3=A) are the visual
    /// source through S2. S3 flips the mode to Spawn when it deletes the
    /// direct calls.
    ///
    /// [D-S2-INIT=C] Subscription happens in OnEnable. The bus auto-initializes
    /// (lazy singleton + DefaultExecutionOrder), so Instance is non-null here
    /// in play mode and the previous intermittent "never subscribed" failure
    /// is gone. A success line is logged so init health is visible.
    ///
    /// [PRES-1 / D-PRES1-2=A] Adds the Spotlight redirect surface. Like every
    /// other handler here it honours <see cref="mode"/>: an adapter parked in
    /// VerifyOnly must go visually silent across ALL surfaces, or a debugging
    /// session sees one lone floater and cannot tell which path drew it.
    ///
    /// Placement: scene-placed in the gig scene (NOT on the DDOL managers
    /// object), so OnEnable/OnDisable exercise subscribe/unsubscribe across
    /// scene reloads (ST-S2-4) — same pattern as FloatingTextMidiListener.
    /// </summary>
    public class SensoryFxAdapter : MonoBehaviour
    {
        public enum AdapterMode
        {
            /// <summary>Compute + log payloads; do not spawn (S2 coexistence).</summary>
            VerifyOnly,
            /// <summary>Spawn FT via FxManager (S3, post direct-call deletion).</summary>
            Spawn
        }

        [Header("S3: Spawn — bus is the sole FT source after direct-call deletion (D-S3-4=A)")]
        [SerializeField] private AdapterMode mode = AdapterMode.Spawn;

        [Tooltip("Log every handled event with its reconstructed payload " +
                 "(ST-S2-1/2/3 verification). Safe to disable once S2 closes.")]
        [SerializeField] private bool logVerification = true;

        /// <summary>Counters for smoke-test parity checks.</summary>
        public long ReactionEventsHandled { get; private set; }
        public long VibeEventsHandled { get; private set; }

        /// <summary>[JUICE-PW] Card-impact events handled (ST-PW parity).</summary>
        public long VibeImpactEventsHandled { get; private set; }

        /// <summary>[PRES-1] Spotlight redirect events handled (ST-PRES1-4/5/6
        /// parity — in particular ST-PRES1-6, where the expected count is 0).</summary>
        public long SpotlightRedirectEventsHandled { get; private set; }

        [Header("JUICE-PW: card Vibe impact presentation")]
        [Tooltip("One-shot kick intensity [0..1] for the impacted audience " +
                 "member's CharacterAnimator (null-guarded per prefab).")]
        [SerializeField][Range(0f, 1f)] private float impactKickIntensity = 0.8f;

        [Tooltip("One-shot kick intensity [0..1] for the performer (Sibi). " +
                 "Fires once per card play (FanoutIndex == 0).")]
        [SerializeField][Range(0f, 1f)] private float performerKickIntensity = 1f;

        [Tooltip("Extra particles burst on the performer at impact (0 = off).")]
        [SerializeField][Min(0)] private int performerBurstParticles = 14;

        [Tooltip("Particles burst on each impacted member (0 = off). Blocked " +
                 "(INDIFFERENT) members get no burst — the grey FT carries it.")]
        [SerializeField][Min(0)] private int targetBurstParticles = 6;

        private SensoryEventBus _bus;

        private void OnEnable()
        {
            _bus = SensoryEventBus.Instance;
            if (_bus == null)
            {
                // Should not happen in play mode now that the bus
                // auto-initializes; kept as a defensive guard.
                Debug.LogWarning(
                    "[SensoryFxAdapter] No SensoryEventBus available — " +
                    "adapter inactive this session.");
                return;
            }

            _bus.Subscribe<AudienceReactionEvent>(OnAudienceReaction);
            _bus.Subscribe<SongEndVibeEvent>(OnSongEndVibe);
            _bus.Subscribe<AudienceVibeImpactEvent>(OnVibeImpact);
            _bus.Subscribe<SpotlightRedirectEvent>(OnSpotlightRedirect); // [PRES-1]

            Debug.Log(
                $"[SensoryFxAdapter] Subscribed to bus " +
                $"(AudienceReaction + SongEndVibe + AudienceVibeImpact + " +
                $"SpotlightRedirect). Mode={mode}, " +
                $"logVerification={logVerification}.");
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<AudienceReactionEvent>(OnAudienceReaction);
            _bus.Unsubscribe<SongEndVibeEvent>(OnSongEndVibe);
            _bus.Unsubscribe<AudienceVibeImpactEvent>(OnVibeImpact);
            _bus.Unsubscribe<SpotlightRedirectEvent>(OnSpotlightRedirect); // [PRES-1]
            _bus = null;
        }

        private void OnAudienceReaction(AudienceReactionEvent e)
        {
            ReactionEventsHandled++;

            string text = SensoryFtPresentation.ImpressionExclamation(e.Impression);
            Color color = SensoryFtPresentation.ImpressionColor(e.Impression);

            if (mode == AdapterMode.Spawn)
            {
                if (!string.IsNullOrEmpty(text)
                    && e.Audience != null
                    && e.Audience.TextSpawnRoot != null
                    && FxManager.Instance != null)
                {
                    FxManager.Instance.SpawnFloatingText(
                        e.Audience.TextSpawnRoot, text,
                        SensoryFtPresentation.ReactionDrift, color);
                }
            }
            else if (logVerification)
            {
                Debug.Log(
                    $"[SensoryFxAdapter][Verify] AudienceReaction " +
                    $"{e.AudienceId} impression={e.Impression} " +
                    $"(raw={e.RawImpression}) part={e.LoopContext.PartIndex} " +
                    $"loop={e.LoopContext.LoopIndexWithinPart} " +
                    $"-> \"{text}\" rgba=({color.r:F2},{color.g:F2},{color.b:F2})");
            }
        }

        private void OnSongEndVibe(SongEndVibeEvent e)
        {
            VibeEventsHandled++;

            if (!SensoryFtPresentation.TryBuildSongEndVibeFt(
                    in e, out string text, out Color color))
                return;

            if (mode == AdapterMode.Spawn)
            {
                if (e.Audience != null
                    && e.Audience.TextSpawnRoot != null
                    && FxManager.Instance != null)
                {
                    // [S3 D-S3-5=A] Int overload (xDir=0 → FxManager randomizes ±1)
                    // reproduces the S1 song-end random-diagonal drift; the Vector2
                    // overload would force straight-up and break ST-S2-3 parity.
                    FxManager.Instance.SpawnFloatingText(
                        e.Audience.TextSpawnRoot, text, 0, 1, color);
                }
            }
            else if (logVerification)
            {
                Debug.Log(
                    $"[SensoryFxAdapter][Verify] SongEndVibe {e.AudienceId} " +
                    $"intended=+{e.IntendedDelta} applied=+{e.AppliedDelta} " +
                    $"flowStacks={e.FlowStacks} " +
                    $"blocked={e.BlockedByIndifference} " +
                    $"-> \"{text}\" rgba=({color.r:F2},{color.g:F2},{color.b:F2})");
            }
        }

        // ----- Card Vibe impact (JUICE-PW D1=A / D2=B / D3=A) --------------

        /// <summary>
        /// Per-target handler for the AoE fan-out. FT is staggered by
        /// FanoutIndex (per-member floaters, D3=A); the performer's kick +
        /// burst fires once on FanoutIndex 0 (D2=B: procedural one-shot, no
        /// clip system). Audio is NOT handled here — SensoryAudioAdapter owns
        /// the single CardVibeImpact sting (per-handler isolation, D-S3-3=A).
        /// </summary>
        private void OnVibeImpact(AudienceVibeImpactEvent e)
        {
            VibeImpactEventsHandled++;

            if (!SensoryFtPresentation.TryBuildVibeImpactFt(
                    in e, out string text, out Color color))
                return;

            if (mode == AdapterMode.Spawn)
            {
                float delay = e.FanoutIndex * SensoryFtPresentation.VibeImpactStaggerStep;
                if (delay > 0f)
                    StartCoroutine(SpawnVibeImpactDeferred(e, text, color, delay));
                else
                    SpawnVibeImpactNow(e, text, color);
            }
            else if (logVerification)
            {
                Debug.Log(
                    $"[SensoryFxAdapter][Verify] VibeImpact {e.AudienceId} " +
                    $"card='{e.Card?.DisplayName}' base={e.BaseDelta} " +
                    $"final={e.FinalDelta} applied={e.AppliedDelta} " +
                    $"blocked={e.BlockedByIndifference} " +
                    $"fanout={e.FanoutIndex}/{e.TargetCount} " +
                    $"-> \"{text}\" rgba=({color.r:F2},{color.g:F2},{color.b:F2})");
            }
        }

        private System.Collections.IEnumerator SpawnVibeImpactDeferred(
            AudienceVibeImpactEvent e, string text, Color color, float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnVibeImpactNow(e, text, color);
        }

        private void SpawnVibeImpactNow(
            AudienceVibeImpactEvent e, string text, Color color)
        {
            // FT on the impacted member (anchor may have been destroyed while
            // the stagger delay elapsed — every ref is re-guarded here).
            if (!string.IsNullOrEmpty(text)
                && e.Audience != null
                && e.Audience.TextSpawnRoot != null
                && FxManager.Instance != null)
            {
                FxManager.Instance.SpawnFloatingText(
                    e.Audience.TextSpawnRoot, text,
                    SensoryFtPresentation.VibeImpactDrift, color);
            }

            // Target kick + burst — only when the vibe actually landed;
            // blocked members stay visually inert under the grey FT.
            if (e.AppliedDelta > 0 && e.Audience != null
                && e.Audience.CharacterAnimator != null)
            {
                e.Audience.CharacterAnimator.PlayImpactKick(impactKickIntensity);
                if (targetBurstParticles > 0)
                    e.Audience.CharacterAnimator.BurstParticles(targetBurstParticles);
            }

            // Performer (Sibi) kick + burst, once per card play.
            if (e.FanoutIndex == 0 && e.Performer != null
                && e.Performer.CharacterAnimator != null)
            {
                e.Performer.CharacterAnimator.PlayImpactKick(performerKickIntensity);
                if (performerBurstParticles > 0)
                    e.Performer.CharacterAnimator.BurstParticles(performerBurstParticles);
            }
        }

        // ----- Spotlight redirect [PRES-1 / D-PRES1-2=A] -------------------

        /// <summary>
        /// Announces a taunt redirect that until now only existed in the console
        /// log. Anchor priority: the ORIGINAL target's TextSpawnRoot when the
        /// event names one (the floater belongs where the hit was aimed), else
        /// the protected musician's. Honours <see cref="mode"/> like every other
        /// handler — see the class summary for why that matters.
        ///
        /// The publisher already filters the visual no-op case, so any event
        /// arriving here is worth drawing.
        /// </summary>
        private void OnSpotlightRedirect(SpotlightRedirectEvent e)
        {
            SpotlightRedirectEventsHandled++;

            if (!SensoryFtPresentation.TryBuildSpotlightRedirectFt(
                    e, out string text, out Color color))
                return;

            if (mode == AdapterMode.Spawn)
            {
                ALWTTT.Characters.CharacterBase owner =
                    e.OriginalTarget != null
                        ? (ALWTTT.Characters.CharacterBase)e.OriginalTarget
                        : e.ProtectedTarget;

                if (owner == null || FxManager.Instance == null) return;

                var anchor = owner.TextSpawnRoot != null
                    ? owner.TextSpawnRoot
                    : owner.transform;

                FxManager.Instance.SpawnFloatingText(
                    anchor, text,
                    SensoryFtPresentation.SpotlightRedirectDrift, color);
            }
            else if (logVerification)
            {
                Debug.Log(
                    $"[SensoryFxAdapter][Verify] SpotlightRedirect " +
                    $"source={e.Source?.CharacterId} " +
                    $"original={(e.OriginalTarget != null ? e.OriginalTarget.CharacterName : "RANDOM/none")} " +
                    $"protected={e.ProtectedTarget?.CharacterName} " +
                    $"-> \"{text}\" rgba=({color.r:F2},{color.g:F2},{color.b:F2})");
            }
        }
    }
}
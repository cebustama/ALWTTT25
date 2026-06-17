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

            Debug.Log(
                $"[SensoryFxAdapter] Subscribed to bus " +
                $"(AudienceReaction + SongEndVibe). Mode={mode}, " +
                $"logVerification={logVerification}.");
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<AudienceReactionEvent>(OnAudienceReaction);
            _bus.Unsubscribe<SongEndVibeEvent>(OnSongEndVibe);
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
    }
}
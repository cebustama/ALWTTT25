// Place at: Assets/Scripts/Sensory/PsychicWaveOverlayController.cs
using ALWTTT.Cards;
using ALWTTT.Characters;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [R4 / D-R4-5=A -> PRES-1 / D-PRES1-5=A] Full-screen "psychic wave" for
    /// Psychic Wave, v4.
    ///
    /// v4 EFFECT MODEL - one inverted region, two phases, both anchored on the
    /// performer (Sibi):
    ///   Phase 1 (cover):   an inverted DISC expands from the anchor until the
    ///                      whole screen is colour-inverted.
    ///   Hold:              configurable pause at full cover.
    ///   Phase 2 (uncover): a hole expands from the same anchor, undoing the
    ///                      inversion until the screen is clean.
    /// Rendered by a single Image using ALWTTT/UI/PsychicWaveInvert, whose
    /// inverted area is the annulus between _InnerRadius and _OuterRadius.
    /// See the shader header for the invert-blend algebra (D-PRES1-1=B+).
    ///
    /// v4 RETIRES THE v2/v3 TINT FRONT (TutorialSpotlight overlay). The "colour
    /// oval" it produced was the un-punched remainder of a screen-wide tint,
    /// which contradicted the cover-then-uncover reading. The wave's colour
    /// identity now lives in <see cref="waveColor"/>, which tints the inversion
    /// itself. If the legacy tint Image is still wired it is force-disabled at
    /// Awake - no scene rewiring required.
    ///
    /// COVER RADIUS IS COMPUTED, NOT AUTHORED: the radius needed to cover the
    /// screen depends on where the anchor is (a corner anchor needs ~2x a
    /// centred one). Each play computes the aspect-corrected distance from the
    /// anchor to the farthest screen corner, so cover is always total and there
    /// is no max-size field to mis-tune.
    ///
    /// Presentation-only bus listener, per the Sensory Contract: gameplay never
    /// calls this class. It subscribes to <see cref="AudienceVibeImpactEvent"/> -
    /// the same event JUICE-PW already publishes once per audience target from
    /// CardBase.ExecuteEffects - and fires exactly once per card play by keying
    /// on FanoutIndex == 0, the play-once key JUICE-PW validated for the audio
    /// sting (D3=A). Card-scoped: the event carries the played CardDefinition,
    /// so the wave only triggers for the configured asset.
    ///
    /// Timing guarantee (tutorial beat 8): impact events are published at effect
    /// resolution, i.e. BEFORE DeckManager.OnCardPlayed fires CardPlayedEvent,
    /// and the beat-8 hold release keys on CardPlayedEvent. The wave is
    /// therefore always on screen before the held loop is let go. NOTE the v4
    /// total duration is cover + hold + uncover - keep it comfortably under the
    /// post-play flow so the uncover is not cut by a scene change.
    ///
    /// Safety: raycastTarget is forced off in Awake so the wave can never eat a
    /// click during the tutorial's finisher-only window. An unwired component
    /// (null image / material / trigger card) is a LOUD no-op: silence here is
    /// indistinguishable from a broken event pipeline, so wiring state is
    /// logged at Awake and OnEnable.
    ///
    /// Known scope limit: if every audience member is IsBlocked, no impact
    /// events are published and no wave plays. Accepted - a card that landed on
    /// nobody should not present a climax.
    /// </summary>
    public class PsychicWaveOverlayController : MonoBehaviour
    {
        [Header("Wiring (v4)")]
        [Tooltip("Full-screen stretched Image using ALWTTT/UI/PsychicWaveInvert. " +
                 "THE wave surface. Null -> component is inert (loud).")]
        [SerializeField] private Image invertImage;

        [Tooltip("The CardDefinition whose resolution triggers the wave " +
                 "(Psychic Waves). Null -> component is inert.")]
        [SerializeField] private CardDefinition triggerCard;

        [Header("Legacy tint front (v2/v3 - RETIRED)")]
        [Tooltip("The old TutorialSpotlight tint Image. No longer part of the " +
                 "effect; force-disabled at Awake if still assigned. Kept so " +
                 "existing scene wiring does not dangle.")]
        [SerializeField] private Image overlayImage;

        [Tooltip("Legacy hole sprite for the retired tint front. Unused in v4.")]
        [SerializeField] private Sprite holeShape;

        [Header("Tuning (v4)")]
        [Tooltip("Seconds for phase 1: the inverted disc expanding from the " +
                 "anchor to full cover.")]
        [Min(0.05f)]
        [SerializeField] private float coverDuration = 0.5f;

        [Tooltip("Seconds the screen stays fully inverted between phases. " +
                 "0 = uncover starts immediately.")]
        [Min(0f)]
        [SerializeField] private float holdDuration = 0.3f;

        [Tooltip("Seconds for phase 2: the hole expanding from the anchor " +
                 "until the effect is undone.")]
        [Min(0.05f)]
        [SerializeField] private float uncoverDuration = 0.7f;

        [Tooltip("Soft-edge width of both fronts, in viewport units. Wider = " +
                 "dreamier, narrower = snappier.")]
        [Min(0.005f)]
        [SerializeField] private float edgeWidth = 0.12f;

        [Tooltip("White = pure inversion. Any other colour tints the inverted " +
                 "result toward that hue - useful on dark scenes, where pure " +
                 "inversion reads as white.")]
        [SerializeField] private Color waveColor = Color.white;

        [Header("Diagnostics")]
        [Tooltip("Log wiring state at Awake/OnEnable and one line per trigger. " +
                 "Leave ON until ST-PRES1-1..3 pass.")]
        [SerializeField] private bool logDiagnostics = true;

        private static readonly int WaveEnabledId = Shader.PropertyToID("_WaveEnabled");
        private static readonly int WaveCenterId = Shader.PropertyToID("_WaveCenter");
        private static readonly int OuterRadiusId = Shader.PropertyToID("_OuterRadius");
        private static readonly int InnerRadiusId = Shader.PropertyToID("_InnerRadius");
        private static readonly int EdgeWidthId = Shader.PropertyToID("_EdgeWidth");
        private static readonly int AspectId = Shader.PropertyToID("_Aspect");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int RingColorId = Shader.PropertyToID("_RingColor");

        // Runtime material instance. Never the shared asset: writing shader
        // properties on a shared material would dirty the asset in the editor.
        private Material _invertMat;
        private Coroutine _running;

        /// <summary>[R4-SMOKE] True when image + material + trigger card are set.</summary>
        public bool IsWired => invertImage != null && _invertMat != null && triggerCard != null;

        /// <summary>[PRES1-SMOKE] v4: the ring IS the wave, so this is an alias
        /// of <see cref="IsWired"/>. Kept so R4/PRES-1 smoke notes still read.</summary>
        public bool IsRingWired => IsWired;

        /// <summary>[R4-SMOKE] Number of waves played this session.</summary>
        public int TriggerCount { get; private set; }

        private void Awake()
        {
            if (invertImage == null)
            {
                Debug.LogWarning("[PRES-1][PsychicWave] invertImage is NOT assigned. " +
                                 "The wave will never play. Assign the full-screen " +
                                 "Image using ALWTTT/UI/PsychicWaveInvert.");
            }
            else if (invertImage.material == null)
            {
                Debug.LogWarning("[PRES-1][PsychicWave] invertImage has no material. " +
                                 "Assign a material using ALWTTT/UI/PsychicWaveInvert.");
            }
            else
            {
                _invertMat = new Material(invertImage.material);
                invertImage.material = _invertMat;

                // Vertex colour multiplies the wave alpha in the shader; white
                // keeps the authored intensity intact.
                invertImage.color = Color.white;

                // Never intercept input: the beat-8 window is finisher-only and a
                // full-screen raycast target would swallow the drag that follows.
                invertImage.raycastTarget = false;
                invertImage.enabled = false;
            }

            // [v4] Retire the legacy tint front without demanding a scene edit.
            if (overlayImage != null)
            {
                overlayImage.enabled = false;
                if (logDiagnostics)
                    Debug.Log("[PRES-1][PsychicWave] Legacy tint Image is still " +
                              "wired - force-disabled (v4 retires the tint front).");
            }

            if (logDiagnostics)
                Debug.Log($"[PRES-1][PsychicWave] Awake - " +
                          $"image={(invertImage != null ? "OK" : "MISSING")} " +
                          $"material={(_invertMat != null ? "OK" : "MISSING")} " +
                          $"card={(triggerCard != null ? triggerCard.name : "MISSING")}.");
        }

        private void OnEnable()
        {
            var bus = SensoryEventBus.Instance;
            if (bus == null)
            {
                Debug.LogWarning("[R4][PsychicWave] No SensoryEventBus at OnEnable; " +
                                 "not subscribed. The wave will never fire.");
                return;
            }

            bus.Subscribe<AudienceVibeImpactEvent>(OnVibeImpact);

            if (logDiagnostics)
                Debug.Log("[R4][PsychicWave] Subscribed to AudienceVibeImpactEvent " +
                          $"(handlers now {bus.HandlerCount<AudienceVibeImpactEvent>()}). " +
                          $"IsWired={IsWired}.");
        }

        private void OnDisable()
        {
            SensoryEventBus.Instance?.Unsubscribe<AudienceVibeImpactEvent>(OnVibeImpact);
        }

        private void OnVibeImpact(AudienceVibeImpactEvent e)
        {
            if (!IsWired) return;

            // Play-once key: one wave per card play, not one per AoE target.
            if (e.FanoutIndex != 0) return;

            // Card-scoped: stable SO reference comparison.
            if (e.Card != triggerCard)
            {
                if (logDiagnostics)
                    Debug.Log($"[R4][PsychicWave] Ignored impact from card " +
                              $"'{(e.Card != null ? e.Card.name : "null")}' " +
                              $"(trigger is '{triggerCard.name}').");
                return;
            }

            PlayWave(e.TargetCount, ResolveViewportAnchor(e.Performer));
        }

        /// <summary>
        /// [PRES-1] Performer world position -> viewport UV for the wave anchor.
        /// Dual-path by necessity: a performer parented under a Screen-Space-
        /// Overlay canvas already has screen-pixel coordinates in
        /// transform.position (no camera involved), while a world performer must
        /// project through the camera. Anything unresolvable falls back to
        /// screen centre and never throws.
        /// </summary>
        private static Vector2 ResolveViewportAnchor(CharacterBase performer)
        {
            var center = new Vector2(0.5f, 0.5f);
            if (performer == null) return center;

            var canvas = performer.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Vector3 p = performer.transform.position;
                return new Vector2(
                    Mathf.Clamp01(p.x / Mathf.Max(1, Screen.width)),
                    Mathf.Clamp01(p.y / Mathf.Max(1, Screen.height)));
            }

            var cam = Camera.main;
            if (cam == null) return center;

            Vector3 vp = cam.WorldToViewportPoint(performer.transform.position);
            if (vp.z < 0f) return center;   // behind the camera: mirrored garbage

            return new Vector2(Mathf.Clamp01(vp.x), Mathf.Clamp01(vp.y));
        }

        /// <summary>
        /// [v4] Aspect-corrected distance from the anchor to the farthest screen
        /// corner - the outer radius that guarantees FULL cover from any anchor.
        /// Must use the same distance space as the shader (x scaled by aspect).
        /// </summary>
        private static float ComputeCoverRadius(Vector2 anchor, float aspect)
        {
            float maxSq = 0f;
            for (int cx = 0; cx <= 1; cx++)
            {
                for (int cy = 0; cy <= 1; cy++)
                {
                    float dx = (cx - anchor.x) * aspect;
                    float dy = cy - anchor.y;
                    float sq = dx * dx + dy * dy;
                    if (sq > maxSq) maxSq = sq;
                }
            }
            return Mathf.Sqrt(maxSq);
        }

        private void PlayWave(int targetCount, Vector2 anchor)
        {
            TriggerCount++;
            if (logDiagnostics)
                Debug.Log($"[R4][PsychicWave] Overlay triggered (targets={targetCount}, " +
                          $"trigger #{TriggerCount}, anchor=({anchor.x:F2},{anchor.y:F2})).");

            // Re-trigger restarts rather than stacking: two overlapping waves on
            // one Image would read as a flicker.
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(WaveRoutine(anchor));
        }

        private IEnumerator WaveRoutine(Vector2 anchor)
        {
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);

            // +edgeWidth so the soft edge itself clears the farthest corner:
            // at radius == corner distance the smoothstep edge would leave a
            // faint uninverted sliver there.
            float targetRadius = ComputeCoverRadius(anchor, aspect) + edgeWidth;

            _invertMat.SetVector(WaveCenterId, new Vector4(anchor.x, anchor.y, 0f, 0f));
            _invertMat.SetFloat(AspectId, aspect);
            _invertMat.SetFloat(EdgeWidthId, edgeWidth);
            _invertMat.SetColor(RingColorId, waveColor);
            _invertMat.SetFloat(IntensityId, 1f);
            _invertMat.SetFloat(OuterRadiusId, 0f);
            _invertMat.SetFloat(InnerRadiusId, 0f);
            _invertMat.SetFloat(WaveEnabledId, 1f);
            invertImage.enabled = true;

            // ---- Phase 1: COVER - inverted disc expands from the anchor ----
            // Unscaled time throughout: a future pause must never freeze a
            // full-screen inversion on screen.
            float t = 0f;
            while (t < coverDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / coverDuration);
                _invertMat.SetFloat(OuterRadiusId,
                    Mathf.SmoothStep(0f, 1f, k) * targetRadius);
                yield return null;
            }
            _invertMat.SetFloat(OuterRadiusId, targetRadius);

            // ---- Hold: fully inverted ----
            if (holdDuration > 0f)
            {
                t = 0f;
                while (t < holdDuration)
                {
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            // ---- Phase 2: UNCOVER - hole expands from the same anchor ----
            t = 0f;
            while (t < uncoverDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / uncoverDuration);
                _invertMat.SetFloat(InnerRadiusId,
                    Mathf.SmoothStep(0f, 1f, k) * targetRadius);
                yield return null;
            }

            invertImage.enabled = false;
            _invertMat.SetFloat(WaveEnabledId, 0f);
            _running = null;
        }

        /// <summary>
        /// [R4-SMOKE] Force-hide the overlay (gig teardown / dev reset). Safe to
        /// call when nothing is running. A stranded wave would leave part of the
        /// screen permanently inverted.
        /// </summary>
        public void ForceHide()
        {
            if (_running != null)
            {
                StopCoroutine(_running);
                _running = null;
            }
            if (invertImage != null) invertImage.enabled = false;
            if (_invertMat != null) _invertMat.SetFloat(WaveEnabledId, 0f);
            if (overlayImage != null) overlayImage.enabled = false;
        }

        /// <summary>
        /// [R4-SMOKE] Fire the wave without playing a card. Right-click the
        /// component header in play mode. Isolates "VFX broken" from "event
        /// never arrived" in one click. Fires at screen centre: it has no
        /// event, hence no performer - use a real card play to verify the
        /// anchor (ST-PRES1-1).
        /// </summary>
        [ContextMenu("R4: Test Wave")]
        private void TestWave()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[R4][PsychicWave] Test Wave only works in play mode.");
                return;
            }
            if (!IsWired)
            {
                Debug.LogWarning($"[R4][PsychicWave] Not wired - image={(invertImage != null)} " +
                                 $"material={(_invertMat != null)} card={(triggerCard != null)}.");
                return;
            }
            PlayWave(-1, new Vector2(0.5f, 0.5f));
        }
    }
}
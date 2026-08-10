// Place at: Assets/Scripts/Sensory/PsychicWaveOverlayController.cs
using ALWTTT.Cards;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [R4 / D-R4-5=A] Full-screen "psychic wave" mask for Psychic Wave v2.
    ///
    /// Presentation-only bus listener, per the Sensory Contract: gameplay never
    /// calls this class. It subscribes to <see cref="AudienceVibeImpactEvent"/> -
    /// the same event JUICE-PW already publishes once per audience target from
    /// CardBase.ExecuteEffects - and fires exactly once per card play by keying on
    /// FanoutIndex == 0, the play-once key JUICE-PW validated for the audio sting
    /// (D3=A). Card-scoped: the event carries the played CardDefinition, so the
    /// wave only triggers for the configured asset and no other Vibe card gets it.
    ///
    /// Timing guarantee (tutorial beat 8): impact events are published at effect
    /// resolution, i.e. BEFORE DeckManager.OnCardPlayed fires CardPlayedEvent, and
    /// the beat-8 hold release keys on CardPlayedEvent. The wave is therefore
    /// always on screen before the held loop is let go.
    ///
    /// RENDERING - reuses the ALWTTT/UI/TutorialSpotlight shader. Three facts about
    /// that shader drive this implementation:
    ///   1. The fragment does `color = tex(_MainTex) * (vertexColor * _Color)`, so
    ///      the Image's own `color` MULTIPLIES the tint. Forced to white in Awake;
    ///      a grey Image would silently darken the wave.
    ///   2. `_HoleTex` defaults to "black" (alpha 0) => no hole is ever punched and
    ///      the screen stays tinted for the whole routine. The hole sprite is a
    ///      serialized field here rather than a material setting, so a forgotten
    ///      texture assignment is impossible to make silently.
    ///   3. The hole is a QUAD mapped to the sprite's UV: at large half-sizes the
    ///      sprite's transparent corners leave tint in the screen corners. The tint
    ///      alpha therefore fades to zero alongside the expansion (v2 change) so the
    ///      wave always exits clean regardless of hole shape.
    ///
    /// Safety: raycastTarget is forced off in Awake so the wave can never eat a
    /// click during the tutorial's finisher-only window. An unwired component
    /// (null image / null trigger card) is a no-op, but unlike the S5a telegraph
    /// slots it is a LOUD no-op: silence here is indistinguishable from a broken
    /// event pipeline, so wiring state is logged at Awake and OnEnable.
    ///
    /// Known scope limit: if every audience member is IsBlocked, no impact events
    /// are published and no wave plays. Accepted - a card that landed on nobody
    /// should not present a climax.
    /// </summary>
    public class PsychicWaveOverlayController : MonoBehaviour
    {
        [Header("Wiring")]
        [Tooltip("Full-screen stretched Image whose material uses the " +
                 "ALWTTT/UI/TutorialSpotlight shader. Null -> component is inert.")]
        [SerializeField] private Image overlayImage;

        [Tooltip("Hole shape sprite (white-on-transparent, e.g. a soft circle). " +
                 "Written to _HoleTex at Awake. Without it the shader punches no " +
                 "hole and the screen just tints.")]
        [SerializeField] private Sprite holeShape;

        [Tooltip("The CardDefinition whose resolution triggers the wave " +
                 "(Psychic Waves). Null -> component is inert.")]
        [SerializeField] private CardDefinition triggerCard;

        [Header("Tuning")]
        [SerializeField] private Color maskTint = new Color(0.55f, 0.20f, 0.85f, 0.65f);

        [Tooltip("Seconds for the hole to expand from nothing to maxHalfSize.")]
        [Min(0.05f)]
        [SerializeField] private float expandDuration = 0.8f;

        [Tooltip("Final hole half-extent in viewport units. > 0.75 clears the " +
                 "screen corners at 16:9.")]
        [Min(0.1f)]
        [SerializeField] private float maxHalfSize = 1.2f;

        [Tooltip("Fraction of the duration after which the tint starts fading out. " +
                 "0.35 = tint holds for the first third, then dissolves.")]
        [Range(0f, 1f)]
        [SerializeField] private float fadeStartFraction = 0.35f;

        [Header("Diagnostics")]
        [Tooltip("Log wiring state at Awake/OnEnable and one line per trigger. " +
                 "Leave ON until ST-R4-4 and ST-R4-5 pass.")]
        [SerializeField] private bool logDiagnostics = true;

        private static readonly int HoleEnabledId = Shader.PropertyToID("_HoleEnabled");
        private static readonly int HoleCenterId = Shader.PropertyToID("_HoleCenter");
        private static readonly int HoleHalfSizeId = Shader.PropertyToID("_HoleHalfSize");
        private static readonly int HoleRotationId = Shader.PropertyToID("_HoleRotation");
        private static readonly int HoleTexId = Shader.PropertyToID("_HoleTex");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        // Runtime material instance. Never the shared asset: writing shader
        // properties on a shared material would leak into the tutorial overlay
        // and dirty the asset in the editor.
        private Material _mat;
        private Coroutine _running;

        /// <summary>[R4-SMOKE] True when image + material + trigger card are all set.</summary>
        public bool IsWired => overlayImage != null && _mat != null && triggerCard != null;

        /// <summary>[R4-SMOKE] Number of waves played this session.</summary>
        public int TriggerCount { get; private set; }

        private void Awake()
        {
            if (overlayImage == null)
            {
                Debug.LogWarning("[R4][PsychicWave] overlayImage is NOT assigned. " +
                                 "The wave will never play. Assign the full-screen Image.");
                return;
            }

            if (overlayImage.material == null)
            {
                Debug.LogWarning("[R4][PsychicWave] overlayImage has no material. " +
                                 "Assign a material using the ALWTTT/UI/TutorialSpotlight shader.");
                return;
            }

            _mat = new Material(overlayImage.material);
            overlayImage.material = _mat;

            if (holeShape != null && holeShape.texture != null)
                _mat.SetTexture(HoleTexId, holeShape.texture);
            else if (logDiagnostics)
                Debug.LogWarning("[R4][PsychicWave] holeShape is NOT assigned. The shader " +
                                 "will tint the screen but punch no hole.");

            // The shader multiplies vertex colour into the tint; a non-white Image
            // colour would silently darken or wash out maskTint.
            overlayImage.color = Color.white;

            // Never intercept input: the beat-8 window is finisher-only and a
            // full-screen raycast target would swallow the drag that follows.
            overlayImage.raycastTarget = false;
            overlayImage.enabled = false;

            if (logDiagnostics)
                Debug.Log($"[R4][PsychicWave] Awake - image=OK material=OK " +
                          $"hole={(holeShape != null ? "OK" : "MISSING")} " +
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

            PlayWave(e.TargetCount);
        }

        private void PlayWave(int targetCount)
        {
            TriggerCount++;
            if (logDiagnostics)
                Debug.Log($"[R4][PsychicWave] Overlay triggered (targets={targetCount}, " +
                          $"trigger #{TriggerCount}).");

            // Re-trigger restarts rather than stacking: two overlapping waves on
            // one Image would read as a flicker.
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(WaveRoutine());
        }

        private IEnumerator WaveRoutine()
        {
            _mat.SetVector(HoleCenterId, new Vector4(0.5f, 0.5f, 0f, 0f));
            _mat.SetVector(HoleHalfSizeId, Vector4.zero);
            _mat.SetFloat(HoleRotationId, 0f);
            _mat.SetColor(ColorId, maskTint);
            _mat.SetFloat(HoleEnabledId, 1f);
            overlayImage.enabled = true;

            float t = 0f;
            while (t < expandDuration)
            {
                // Unscaled time: the tutorial hold does not pause time today, but
                // any future pause must not freeze a full-screen tint on screen.
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / expandDuration);

                float half = Mathf.SmoothStep(0f, 1f, k) * maxHalfSize;
                _mat.SetVector(HoleHalfSizeId, new Vector4(half, half, 0f, 0f));

                // [v2] Fade the tint out as the hole grows. Without this the hole
                // sprite's transparent corners leave tint in the screen corners at
                // the end of the expansion, and the wave "snaps" off.
                float fade = fadeStartFraction >= 1f
                    ? 1f
                    : 1f - Mathf.Clamp01((k - fadeStartFraction) / (1f - fadeStartFraction));
                var c = maskTint;
                c.a = maskTint.a * fade;
                _mat.SetColor(ColorId, c);

                yield return null;
            }

            overlayImage.enabled = false;
            _mat.SetFloat(HoleEnabledId, 0f);
            _running = null;
        }

        /// <summary>
        /// [R4-SMOKE] Force-hide the overlay (gig teardown / dev reset). Safe to
        /// call when nothing is running.
        /// </summary>
        public void ForceHide()
        {
            if (_running != null)
            {
                StopCoroutine(_running);
                _running = null;
            }
            if (overlayImage != null) overlayImage.enabled = false;
            if (_mat != null) _mat.SetFloat(HoleEnabledId, 0f);
        }

        /// <summary>
        /// [R4-SMOKE] Fire the wave without playing a card. Right-click the
        /// component header in play mode. Isolates "VFX broken" from
        /// "event never arrived" in one click - use it first when ST-R4-4 fails.
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
                Debug.LogWarning($"[R4][PsychicWave] Not wired - image={(overlayImage != null)} " +
                                 $"material={(_mat != null)} card={(triggerCard != null)}.");
                return;
            }
            PlayWave(-1);
        }
    }
}
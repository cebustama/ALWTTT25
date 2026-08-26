using System;
using System.Collections.Generic;
using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// [RFX-1 / D7=A / D8=A] Per-lane particle tuning consumed by
    /// RhythmParticleEmitter.
    ///
    /// Authority: operational presentation tuning, NOT an SSoT-governed contract.
    /// Same classification as CompositionFxConfigSO.
    /// Asset location: Assets/Settings/Gig/RhythmFxConfig.asset
    ///
    /// ------------------------------------------------------------------
    /// THE SEAM: what lives here vs what lives in the ParticleSystem
    /// ------------------------------------------------------------------
    /// This asset owns everything NUMERIC about how a hit feels:
    ///   count, direction, speed, spread, size, lifetime, tint, throttle.
    ///
    /// The ParticleSystem owns everything VISUAL that a curve or a texture
    /// expresses better than a number:
    ///   the sprite/material, Color over Lifetime, Size over Lifetime,
    ///   Rotation over Lifetime, Gravity Modifier, sorting, simulation space.
    ///
    /// The seam moved here in the v3 revision. Previously the emitter multiplied
    /// the ParticleSystem's authored Start Size and inherited its Start Lifetime,
    /// which split "how far does a kick particle travel" across two assets:
    ///   distance = speed (asset) x lifetime (prefab).
    /// Tuning reach meant opening a prefab. Now speed AND lifetime are both here
    /// and reach is tunable from one inspector.
    ///
    /// Consequence: the ParticleSystem's Start Speed, Start Size and Start
    /// Lifetime are IGNORED at runtime - the emitter overrides all three per
    /// emit. Keep them roughly in sync with these values anyway so the editor's
    /// own preview is not wildly misleading, but they are not the source of truth.
    ///
    /// Gravity Modifier stays in the ParticleSystem: EmitParams has no gravity
    /// override, so per-emit control is not available.
    /// </summary>
    [CreateAssetMenu(
        fileName = "RhythmFxConfig",
        menuName = "ALWTTT/Gig/RhythmFxConfig",
        order = 14)]
    public sealed class RhythmFxConfigSO : ScriptableObject
    {
        [Serializable]
        public class LaneEntry
        {
            public RhythmLane lane = RhythmLane.Kick;

            [Tooltip("Disable to silence this lane without unwiring the ParticleSystem.")]
            public bool enabled = true;

            [Header("Burst")]
            [Min(1), Tooltip("Particles emitted per hit at full MIDI velocity.")]
            public int burstCount = 8;

            [Tooltip("Multiplied into the particle's colour. Requires the " +
                     "material's Color Mode = Multiply to be visible.")]
            public Color tint = Color.white;

            [Header("Motion")]
            [Tooltip("World-space direction, in the ParticleSystem's XY plane. " +
                     "This is the readable 'language' of the groove: kick left, " +
                     "snare right, hi-hat up. Mirrors the direction vectors in " +
                     "FloatingTextMidiListener.")]
            public Vector2 direction = Vector2.up;

            [Min(0f), Tooltip("Launch speed in world units per second.")]
            public float speed = 2.6f;

            [Range(0f, 1f), Tooltip("Per-particle random speed scatter. " +
                                    "0.3 = each particle gets 70%-130% of 'speed'.")]
            public float speedJitter = 0.3f;

            [Range(0f, 90f), Tooltip("Half-angle of the fan around 'direction'.")]
            public float spreadDegrees = 20f;

            [Min(0.01f), Tooltip("Seconds each particle lives. Reach in world " +
                                 "units is roughly speed x lifetime, minus gravity drag.")]
            public float lifetime = 0.45f;

            [Header("Size")]
            [Min(0.01f), Tooltip("Particle size in WORLD UNITS at full velocity. " +
                                 "Absolute, not a multiplier - this overrides the " +
                                 "ParticleSystem's Start Size entirely.")]
            public float size = 0.35f;

            [Header("Throttle")]
            [Min(0f), Tooltip("Hard rate limit in seconds. 0.03 survives 16th " +
                              "notes at 180 BPM; raise it to thin out busy hats.")]
            public float minInterval = 0.03f;

            [Range(0f, 1f), Tooltip("0 = ignore MIDI velocity (every hit identical); " +
                                    "1 = count, speed and size fully driven by velocity.")]
            public float velocityInfluence = 0.5f;
        }

        [Header("Master")]
        [SerializeField, Tooltip("Master toggle. Off = no rhythm particles at all.")]
        private bool enabled = true;

        [Header("Lanes")]
        [SerializeField]
        private List<LaneEntry> lanes = new List<LaneEntry>();

        private Dictionary<RhythmLane, LaneEntry> _map;

        public bool Enabled => enabled;
        public IReadOnlyList<LaneEntry> Lanes => lanes;

        /// <summary>Returns the entry for a lane, or null if unauthored.</summary>
        public LaneEntry For(RhythmLane lane)
        {
            if (_map == null)
            {
                _map = new Dictionary<RhythmLane, LaneEntry>();
                foreach (var l in lanes)
                {
                    if (l == null) continue;
                    _map[l.lane] = l; // last-wins; OnValidate warns about duplicates
                }
            }
            return _map.TryGetValue(lane, out var e) ? e : null;
        }

        private void OnValidate()
        {
            // Rebuild lazily after any inspector edit. This is what makes
            // Play-mode tuning work: edit the asset while the game runs and the
            // next burst uses the new numbers immediately - and because this is
            // an asset, not a component, the values SURVIVE exiting Play mode.
            _map = null;

            var seen = new HashSet<RhythmLane>();
            foreach (var l in lanes)
            {
                if (l == null) continue;
                if (!seen.Add(l.lane))
                    Debug.LogWarning(
                        $"[RhythmFxConfig] Duplicate lane entry '{l.lane}' in " +
                        $"'{name}'. The later entry wins.", this);
            }
        }
    }
}
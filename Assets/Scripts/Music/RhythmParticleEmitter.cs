using System.Collections.Generic;
using ALWTTT.Characters;
using ALWTTT.Data;
using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Fx
{
    /// <summary>
    /// [RFX-1] Per-musician bank of pre-instantiated ParticleSystems, one per
    /// VFX lane. Bursts are produced with ParticleSystem.Emit; nothing is
    /// instantiated at runtime.
    ///
    /// Why Emit and not a prefab spawn: this runs at MIDI note rate. A hi-hat on
    /// 16ths at 140 BPM is ~9 hits/second, per musician. The floating-text path
    /// it replaces did Instantiate + Destroy of a TextMeshProUGUI prefab per hit.
    ///
    /// Placement: child of the musician prefab root - the transform GigManager
    /// registers with MidiMusicManager.RegisterMusicianAnchor.
    ///
    /// ------------------------------------------------------------------
    /// v3: the SO now overrides lifetime, size AND velocity per emit
    /// ------------------------------------------------------------------
    /// Earlier drafts multiplied the ParticleSystem's authored Start Size and
    /// inherited its Start Lifetime. That split the single most important feel
    /// parameter - how far a particle travels - across an asset and a prefab
    /// (distance = speed x lifetime). All three now come from RhythmFxConfigSO,
    /// so the ParticleSystem's Start Speed / Start Size / Start Lifetime are
    /// ignored at runtime.
    ///
    /// Side effect worth knowing: the old "Start Size must be a constant" trap
    /// is gone. Nothing reads ps.main.startSize any more.
    ///
    /// ------------------------------------------------------------------
    /// SORTING
    /// ------------------------------------------------------------------
    /// (1) The character's SpriteRenderer is NOT an ancestor of this object.
    ///     SpriteParent is a SIBLING of RhythmFx, so GetComponentInParent
    ///     &lt;SpriteRenderer&gt;() never finds it. We resolve through
    ///     CharacterBase.SpriteRenderer, the serialized reference that
    ///     MusicianBase.SetSpriteLayerOrder actually writes to.
    ///
    /// (2) Sorting is copied in Start(), not Awake(). GigManager.BuildBand does
    ///     Instantiate (Awake runs synchronously) and only THEN calls
    ///     SetSpriteLayerOrder. Reading in Awake would miss the front/back stage
    ///     assignment. Anyone changing a musician's order later calls SyncSorting().
    ///
    /// ------------------------------------------------------------------
    /// [RFX-2] IsLaneReady
    /// ------------------------------------------------------------------
    /// The chord ladder needs to know whether a lane is AUTHORED, which is a
    /// different question from whether Emit() succeeded. See the method.
    /// </summary>
    public sealed class RhythmParticleEmitter : MonoBehaviour
    {
        [System.Serializable]
        public class LaneSlot
        {
            public RhythmLane lane = RhythmLane.Kick;
            public ParticleSystem system;
        }

        [Header("Config")]
        [SerializeField] private RhythmFxConfigSO config;

        [Header("Lanes")]
        [Tooltip("One ParticleSystem per lane this musician can produce. " +
                 "[RFX-2] Author all eleven emitting lanes: the six percussion " +
                 "lanes, the legacy Chord fallback, and the five chord-ladder " +
                 "rungs. Perc is intentionally left unauthored - it is a " +
                 "diagnostic bucket, not a visual. Any musician can end up " +
                 "owning the rhythm or backing track depending on band " +
                 "composition, so every prefab carries every lane.")]
        [SerializeField] private List<LaneSlot> slots = new List<LaneSlot>();

        [Header("Sorting")]
        [Tooltip("Leave empty to resolve through CharacterBase.SpriteRenderer. " +
                 "Only set by hand if this emitter is not under a CharacterBase " +
                 "(for example in the RhythmFx sandbox scene).")]
        [SerializeField] private SpriteRenderer sortingSourceOverride;

        [SerializeField] private int sortingOrderOffset = 1;

        [Header("Diagnostics")]
        [SerializeField] private bool logSetup = false;

        private readonly Dictionary<RhythmLane, ParticleSystem> _byLane =
            new Dictionary<RhythmLane, ParticleSystem>();

        private readonly Dictionary<RhythmLane, float> _lastEmit =
            new Dictionary<RhythmLane, float>();

        private readonly Dictionary<RhythmLane, long> _bursts =
            new Dictionary<RhythmLane, long>();

        private SpriteRenderer _sortingSource;

        public RhythmFxConfigSO Config => config;
        public bool HasLane(RhythmLane lane) => _byLane.ContainsKey(lane);
        public long BurstsFor(RhythmLane lane) =>
            _bursts.TryGetValue(lane, out var n) ? n : 0L;

        /// <summary>
        /// [RFX-2] True when this lane can produce a burst as a matter of
        /// AUTHORING: a ParticleSystem is wired for it AND the config carries an
        /// enabled entry for it.
        ///
        /// Deliberately distinct from Emit() returning false, which ALSO happens
        /// on throttle. The chord ladder needs "is this rung authored?" without
        /// conflating it with "did this rung just fire?" - otherwise the D2=B
        /// fallback would fire a SECOND burst on RhythmLane.Chord every time a
        /// rung was merely inside its minInterval, and the throttle would end up
        /// producing more particles instead of fewer.
        ///
        /// Cheap by construction: a dictionary probe plus the config's own
        /// lazily-built lane map. Safe to call on the per-chord path.
        /// </summary>
        public bool IsLaneReady(RhythmLane lane)
        {
            if (config == null || !config.Enabled) return false;
            if (!_byLane.TryGetValue(lane, out var ps) || ps == null) return false;
            var e = config.For(lane);
            return e != null && e.enabled;
        }

        private void Awake()
        {
            foreach (var s in slots)
            {
                if (s == null || s.system == null) continue;
                if (_byLane.ContainsKey(s.lane))
                    Debug.LogWarning(
                        $"[RhythmFx] '{name}' has two slots for lane '{s.lane}'. " +
                        $"The later one wins.", this);
                _byLane[s.lane] = s.system;
            }

            _sortingSource = sortingSourceOverride != null
                ? sortingSourceOverride
                : GetComponentInParent<CharacterBase>()?.SpriteRenderer;

            if (logSetup)
                Debug.Log($"[RhythmFx] '{name}' ready with {_byLane.Count} lane(s), " +
                          $"config={(config != null ? config.name : "NULL")}, " +
                          $"sortingSource={(_sortingSource != null ? _sortingSource.name : "NULL")}.",
                          this);
        }

        // Deliberately Start, not Awake. See class comment, sorting note (2).
        private void Start() => SyncSorting();

        /// <summary>
        /// Copy the character's sorting layer/order onto every lane renderer.
        /// Call again if a musician's stage position changes mid-gig.
        /// </summary>
        public void SyncSorting()
        {
            if (_sortingSource == null)
            {
                if (logSetup)
                    Debug.LogWarning(
                        $"[RhythmFx] '{name}' has no sorting source. Particles keep " +
                        $"their authored renderer order.", this);
                return;
            }

            foreach (var ps in _byLane.Values)
            {
                var r = ps.GetComponent<ParticleSystemRenderer>();
                if (r == null) continue;
                r.sortingLayerID = _sortingSource.sortingLayerID;
                r.sortingOrder = _sortingSource.sortingOrder + sortingOrderOffset;
            }

            if (logSetup)
                Debug.Log($"[RhythmFx] '{name}' sorting synced to order " +
                          $"{_sortingSource.sortingOrder + sortingOrderOffset}.", this);
        }

        /// <summary>Emit one burst on a lane.</summary>
        /// <param name="velocity01">Normalised MIDI velocity, or 1 when unknown.</param>
        /// <param name="ignoreThrottle">Test harness only - bypasses minInterval.</param>
        /// <returns>True if a burst was actually emitted.</returns>
        public bool Emit(RhythmLane lane, float velocity01 = 1f, bool ignoreThrottle = false)
        {
            if (config == null || !config.Enabled) return false;

            var e = config.For(lane);
            if (e == null || !e.enabled) return false;

            if (!_byLane.TryGetValue(lane, out var ps) || ps == null) return false;

            float now = Time.time;
            if (!ignoreThrottle
                && _lastEmit.TryGetValue(lane, out var last)
                && now - last < e.minInterval)
                return false;
            _lastEmit[lane] = now;

            // velocityInfluence = 0 -> every hit identical.
            // velocityInfluence = 1 -> ghost notes visibly smaller and slower.
            float v = Mathf.Lerp(1f, Mathf.Clamp01(velocity01), e.velocityInfluence);
            int count = Mathf.Max(1, Mathf.RoundToInt(e.burstCount * v));

            Vector2 baseDir = e.direction.sqrMagnitude > 0.0001f
                ? e.direction.normalized
                : Vector2.up;

            var p = new ParticleSystem.EmitParams
            {
                startColor = e.tint,
                startSize = e.size * Mathf.Lerp(0.8f, 1f, v),
                startLifetime = e.lifetime
            };

            // Per-particle velocity carries the directional language.
            // Position still comes from the Shape module: we never set
            // EmitParams.position, so the small Sphere radius scatters origins.
            for (int i = 0; i < count; i++)
            {
                float ang = Random.Range(-e.spreadDegrees, e.spreadDegrees) * Mathf.Deg2Rad;
                float cos = Mathf.Cos(ang);
                float sin = Mathf.Sin(ang);

                var dir = new Vector2(
                    baseDir.x * cos - baseDir.y * sin,
                    baseDir.x * sin + baseDir.y * cos);

                float sp = e.speed * v *
                           Random.Range(1f - e.speedJitter, 1f + e.speedJitter);

                p.velocity = new Vector3(dir.x * sp, dir.y * sp, 0f);
                ps.Emit(p, 1);
            }

            _bursts[lane] = BurstsFor(lane) + 1;
            return true;
        }
    }
}
using DemoMPTK;
using UnityEngine;

namespace ALWTTT.Characters
{
    public class CharacterAnimator : MonoBehaviour
    {
        [Header("Beat")]
        [SerializeField] private int bpm = 120;
        [Tooltip("0 = downbeat, .5 = upbeat")]
        [SerializeField][Range(0f, 1f)] private float beatOffsetBeats = 0f;
        [SerializeField][Min(1)] private int skipEveryNBeats = 1;

        [Header("Jump")]
        [SerializeField] private bool jumpOnBeat = true;
        [SerializeField] private Transform jumpRoot;
        [SerializeField] private float jumpHeight = 0.25f;
        [SerializeField] private AnimationCurve jumpCurve;

        [Header("External Intensity")]
        [SerializeField][Range(0f, 2f)] private float jumpIntensityMultiplier = 1f;

        [Header("Rotation")]
        [SerializeField] private bool rotateOnBeat = true;
        [SerializeField] private float rotationAmplitude = 6f;
        [SerializeField] private AnimationCurve rotationCurve;

        [Header("Scale Pop (Beat) [B2 / #14]")]
        [Tooltip("Robot-style pop: ease-in/out scale on beat (no vertical jump). " +
            "Independent of JumpOnBeat. For Robot C2 use this with JumpOnBeat = false.")]
        [SerializeField] private bool scaleOnBeat = false;
        [Tooltip("Peak amplitude as a fraction (0.15 = grows to 1.15� base scale).")]
        [SerializeField][Range(0f, 1f)] private float scaleAmplitude = 0.15f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Stretch & Squash (Beat) [B2 / #15]")]
        [Tooltip("Worm-style vertical stretch + horizontal compress on beat. " +
            "Asymmetric: Y grows, X shrinks (classic squash-and-stretch). " +
            "Independent of JumpOnBeat and ScaleOnBeat � typically used with " +
            "JumpOnBeat = false on Gusano. For instrument sub-animator (#16), " +
            "attach a second CharacterAnimator to the instrument GO with its " +
            "own settings.")]
        [SerializeField] private bool stretchOnBeat = false;
        [SerializeField][Range(0f, 1f)] private float stretchYAmplitude = 0.25f;
        [SerializeField][Range(0f, 1f)] private float stretchXAmplitude = 0.15f;
        [SerializeField] private AnimationCurve stretchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Particles")]
        [SerializeField] private ParticleSystem particleSystemRef;
        [SerializeField] private bool emitoOnBeat = true;
        [SerializeField] private int particlesPerBurst = 10;
        [SerializeField][Min(1)] private int emitEveryNBeats = 1;
        [Tooltip("0 = downbeat, .5 = upbeat")]
        [SerializeField][Range(0f, 1f)] private float particleBeatOffsetBeats = 0f;

        private int beatCounter = 0;
        private float beatInterval; // seconds/beat
        private int lastBpm = -1;
        private float timer; // seconds since start
        private Vector3 originalLocalPos;
        private Vector3 originalLocalScale; // [B2 / #14, #15]
        private float originalZ;
        private float nextParticleTime; // absolute
        private int beatsSinceLastEmit;

        private float baseJumpHeight;

        private int animBeatCounter = 0;
        private float nextAnimBeatTime;

        // [S5b / D-S5b-IDLE] Master gate for all beat-driven motion + particle bursts.
        // Authored per-toggle settings are preserved across disable/enable.
        private bool beatAnimationEnabled = true;

        // [JUICE-PW D2=B] One-shot impact kick (procedural, no clip system).
        // Applied as a LateUpdate OVERLAY on top of whatever pose the beat
        // loop wrote this frame, with snapshot/restore so it never
        // accumulates when the beat loop skips a frame (skipEveryNBeats,
        // idle gate, or all pose toggles off).
        [Header("Impact Kick (one-shot) [JUICE-PW]")]
        [Tooltip("Peak scale amplitude of the kick as a fraction (0.25 = up to 1.25×).")]
        [SerializeField][Range(0f, 1f)] private float kickScaleAmplitude = 0.25f;
        [Tooltip("Peak vertical hop of the kick in local units.")]
        [SerializeField][Min(0f)] private float kickHopHeight = 0.12f;
        [Tooltip("Default kick duration in seconds.")]
        [SerializeField][Min(0.05f)] private float kickDuration = 0.35f;

        private float _kickEndTime = -1f;
        private float _kickActiveDuration;
        private float _kickIntensity;
        private bool _kickOverlayApplied;
        private Vector3 _preKickScale, _postKickScale;
        private Vector3 _preKickPos, _postKickPos;

        // Public 
        #region Encapsulation
        public int SkipEveryNBeats
        {
            get => skipEveryNBeats;
            set => skipEveryNBeats = value;
        }

        public float BeatOffsetBeats
        {
            get => beatOffsetBeats;
            set => beatOffsetBeats = value;
        }

        public bool JumpOnBeat
        {
            get => jumpOnBeat;
            set => jumpOnBeat = value;
        }

        public bool RotateOnBeat
        {
            get => rotateOnBeat;
            set => rotateOnBeat = value;
        }

        public bool EmitOnBeat
        {
            get => emitoOnBeat;
            set => emitoOnBeat = value;
        }

        // [B2 / #14] Robot pop toggle. Runtime-tweakable.
        public bool ScaleOnBeat
        {
            get => scaleOnBeat;
            set => scaleOnBeat = value;
        }

        // [B2 / #15] Worm stretch toggle. Runtime-tweakable.
        public bool StretchOnBeat
        {
            get => stretchOnBeat;
            set => stretchOnBeat = value;
        }

        // [S5b / D-S5b-IDLE] Master gate for all beat-driven motion (jump, rotation,
        // scale pop, stretch/squash) and particle bursts. Authored per-toggle settings
        // (JumpOnBeat, ScaleOnBeat, etc.) are preserved, so re-enabling restores the
        // musician's full animation style. Disabling settles the transform to its
        // authored rest pose so the character doesn't freeze mid-animation.
        public bool BeatAnimationEnabled => beatAnimationEnabled;

        public void SetBeatAnimationEnabled(bool enabled)
        {
            if (beatAnimationEnabled == enabled) return;
            beatAnimationEnabled = enabled;
            if (!enabled)
            {
                ResetToRestPose();
            }
            else
            {
                // Resync schedules so re-enabling doesn't fire a catch-up jump/burst.
                ScheduleNextAnimBeat(true);
                ScheduleNextParticle(true);
            }
        }

        // [S5b / D-S5b-IDLE] Restore the authored rest transform captured in Awake
        // (local position, Z rotation, scale).
        public void ResetToRestPose()
        {
            if (jumpRoot == null) return;
            jumpRoot.localPosition = originalLocalPos;
            var e = jumpRoot.localEulerAngles;
            e.z = originalZ;
            jumpRoot.localEulerAngles = e;
            jumpRoot.localScale = originalLocalScale;
        }
        #endregion

        private void Awake()
        {
            if (jumpRoot == null)
                jumpRoot = transform;

            originalLocalPos = jumpRoot.localPosition;
            originalZ = jumpRoot.localEulerAngles.z;
            originalLocalScale = jumpRoot.localScale; // [B2 / #14, #15]

            baseJumpHeight = jumpHeight;

            RecalcBeatInterval();
            ScheduleNextParticle();
            ScheduleNextAnimBeat(true);
        }

        private void Update()
        {
            if (!Mathf.Approximately(lastBpm, bpm))
            {
                RecalcBeatInterval();
                ScheduleNextParticle(true);
                ScheduleNextAnimBeat(true); // resync anim beats too
            }

            if (beatInterval <= 0f) return;

            // [S5b / D-S5b-IDLE] When gated off, no beat motion or particles this frame.
            if (!beatAnimationEnabled) return;

            while (Time.time >= nextAnimBeatTime)
            {
                animBeatCounter++;
                nextAnimBeatTime += beatInterval;
            }

            timer += Time.deltaTime;

            float tBeat =
                Mathf.Repeat(timer + beatOffsetBeats * beatInterval, beatInterval)
                / beatInterval;
            float pingPong = Mathf.PingPong(tBeat, .5f) * 2f;

            if (skipEveryNBeats <= 1 || (animBeatCounter % skipEveryNBeats) == 0)
            {
                // Jumping
                if (jumpOnBeat)
                {
                    float jump =
                        jumpCurve.Evaluate(pingPong) *
                        baseJumpHeight *
                        jumpIntensityMultiplier;

                    jumpRoot.localPosition = originalLocalPos + Vector3.up * jump;
                }

                // Rotation
                if (rotateOnBeat)
                {
                    float r = rotationCurve.Evaluate(pingPong) * rotationAmplitude;
                    var e = jumpRoot.localEulerAngles;
                    e.z = originalZ + r;
                    jumpRoot.localEulerAngles = e;
                }

                // [B2 / #14] Scale Pop (Robot). Independent of stretch.
                // If both ScaleOnBeat and StretchOnBeat are true, Stretch wins
                // (last-write wins downstream below); designers should pick one.
                if (scaleOnBeat)
                {
                    float k = scaleCurve != null ? scaleCurve.Evaluate(pingPong) : pingPong;
                    float s = 1f + (scaleAmplitude * k * jumpIntensityMultiplier);
                    jumpRoot.localScale = originalLocalScale * s;
                }

                // [B2 / #15] Stretch & Squash (Worm). Asymmetric, volume-preserving feel.
                if (stretchOnBeat)
                {
                    float k = stretchCurve != null ? stretchCurve.Evaluate(pingPong) : pingPong;
                    float intensity = jumpIntensityMultiplier;
                    float sy = 1f + (stretchYAmplitude * k * intensity);
                    float sx = 1f - (stretchXAmplitude * k * intensity);

                    var s = originalLocalScale;
                    s.x *= Mathf.Max(0.01f, sx);
                    s.y *= sy;
                    jumpRoot.localScale = s;
                }
            }

            while (Time.time >= nextParticleTime)
            {
                // Particle emission
                if (emitoOnBeat && particleSystemRef != null)
                {
                    if (beatsSinceLastEmit == 0)
                    {
                        particleSystemRef.Emit(particlesPerBurst);
                    }
                    beatsSinceLastEmit = (beatsSinceLastEmit + 1) % emitEveryNBeats;
                }

                // Increment beat counter for jump/rotation skipping
                beatCounter++;

                nextParticleTime += beatInterval;
            }
        }

        public void SetBPM(int newBpm)
        {
            bpm = newBpm;
            RecalcBeatInterval();
            ScheduleNextParticle(true);
        }

        public void SetBeatOffsetBeats(float beats)
        {
            beatOffsetBeats = beats;
            ScheduleNextAnimBeat(true);
        }

        public void SetParticleBeatOffsetBeats(float beats)
        {
            particleBeatOffsetBeats = beats;
            ScheduleNextParticle(true);
        }

        public void SetJumpIntensity01(float t)
        {
            // 0 = no jump, 1 = full configured height
            t = Mathf.Clamp01(t);
            jumpIntensityMultiplier = t;
        }

        public void BurstParticles(int count)
        {
            if (particleSystemRef != null && count > 0)
            {
                particleSystemRef.Emit(count);
            }
        }

        /// <summary>
        /// [JUICE-PW D2=B] Fire a one-shot impact kick: a fast-attack,
        /// eased-decay scale pop + vertical hop overlaid on the beat pose.
        /// Safe to call while the beat loop is running, gated off (idle), or
        /// mid-kick (restarts). Duration &lt;= 0 uses the authored default.
        /// </summary>
        public void PlayImpactKick(float intensity01 = 1f, float duration = -1f)
        {
            _kickIntensity = Mathf.Clamp01(intensity01);
            _kickActiveDuration = duration > 0f ? duration : kickDuration;
            _kickEndTime = Time.time + _kickActiveDuration;
        }

        // [JUICE-PW D2=B] Overlay runs in LateUpdate, after this component's
        // Update wrote the beat pose. Snapshot/restore contract:
        //   1. Undo LAST frame's overlay first — but only if the value still
        //      equals what we wrote (i.e. no beat writer rewrote it this
        //      frame). This kills accumulation on frames where the beat loop
        //      skipped (skipEveryNBeats), is idle-gated, or has all pose
        //      toggles off.
        //   2. Apply this frame's overlay on the clean base and re-snapshot.
        // On kick end with the idle gate off, the pose is restored to rest so
        // no stale kick offset survives (mirrors SetBeatAnimationEnabled).
        private void LateUpdate()
        {
            if (jumpRoot == null) return;

            // (1) Undo the previous frame's overlay if it survived intact.
            if (_kickOverlayApplied)
            {
                if (jumpRoot.localScale == _postKickScale)
                    jumpRoot.localScale = _preKickScale;
                if (jumpRoot.localPosition == _postKickPos)
                    jumpRoot.localPosition = _preKickPos;
                _kickOverlayApplied = false;
            }

            if (_kickEndTime < 0f) return;

            if (Time.time >= _kickEndTime)
            {
                _kickEndTime = -1f;
                if (!beatAnimationEnabled) ResetToRestPose();
                return;
            }

            // (2) Apply this frame's overlay: fast attack (first 20%), eased
            // decay (remaining 80%).
            float t = 1f - (_kickEndTime - Time.time) / _kickActiveDuration;
            float k = t < 0.2f ? (t / 0.2f) : (1f - (t - 0.2f) / 0.8f);
            k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(k)) * _kickIntensity;

            _preKickScale = jumpRoot.localScale;
            _preKickPos = jumpRoot.localPosition;

            jumpRoot.localScale = _preKickScale * (1f + kickScaleAmplitude * k);
            jumpRoot.localPosition = _preKickPos + Vector3.up * (kickHopHeight * k);

            _postKickScale = jumpRoot.localScale;
            _postKickPos = jumpRoot.localPosition;
            _kickOverlayApplied = true;
        }

        #region Private Methods

        private void RecalcBeatInterval()
        {
            lastBpm = bpm;
            beatInterval = (bpm > 0) ? 60f / bpm : 0f;
        }

        private void ScheduleNextParticle(bool resync = false)
        {
            beatsSinceLastEmit = 0;

            if (beatInterval <= 0f)
            {
                nextParticleTime = float.PositiveInfinity;
                return;
            }

            float now = Time.time;
            float beatPhaseSeconds = Mathf.Repeat(now, beatInterval);

            float targetPhaseSeconds = Mathf.Repeat(particleBeatOffsetBeats *
                beatInterval, beatInterval);

            float delta = targetPhaseSeconds - beatPhaseSeconds;

            if (delta <= 0f) delta += beatInterval;

            nextParticleTime = resync ? now + delta :
                now + Mathf.Max(0.01f, delta);
        }

        private void ScheduleNextAnimBeat(bool resync = false)
        {
            animBeatCounter = 0;

            if (beatInterval <= 0f)
            {
                nextAnimBeatTime = float.PositiveInfinity;
                return;
            }

            float now = Time.time;
            float beatPhaseSeconds = Mathf.Repeat(now, beatInterval);
            float targetPhaseSeconds = Mathf.Repeat(beatOffsetBeats * beatInterval, beatInterval);
            float delta = targetPhaseSeconds - beatPhaseSeconds;
            if (delta <= 0f) delta += beatInterval;

            nextAnimBeatTime = resync ? now + delta : now + Mathf.Max(0.01f, delta);
        }
        #endregion
    }
}
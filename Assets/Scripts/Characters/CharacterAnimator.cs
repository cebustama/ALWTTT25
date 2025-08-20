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

        [Header("Rotation")]
        [SerializeField] private bool rotateOnBeat = true;
        [SerializeField] private float rotationAmplitude = 6f;
        [SerializeField] private AnimationCurve rotationCurve;

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
        private float originalZ;
        private float nextParticleTime; // absolute
        private int beatsSinceLastEmit;

        private int animBeatCounter = 0;
        private float nextAnimBeatTime;

        // Public 
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

        private void Awake()
        {
            if (jumpRoot == null)
                jumpRoot = transform;

            originalLocalPos = jumpRoot.localPosition;

            originalZ = jumpRoot.localEulerAngles.z;

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
                    float jump = jumpCurve.Evaluate(pingPong) * jumpHeight;
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

        public void BurstParticles(int count)
        {
            if (particleSystemRef != null && count > 0)
            {
                particleSystemRef.Emit(count);
            }
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

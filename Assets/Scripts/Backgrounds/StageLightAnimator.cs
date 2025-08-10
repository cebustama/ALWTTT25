using UnityEngine;

namespace ALWTTT.Backgrounds
{
    public class StageLightAnimator : MonoBehaviour
    {
        [Header("Beat Settings")]
        [SerializeField] private float bpm = 120f;
        [Tooltip("Offset in beats (0..1).")]
        [Range(0f, 1f)][SerializeField] private float beatOffset = 0f;

        [Header("Rotation")]
        [SerializeField] private float minZ = -10f;
        [SerializeField] private float maxZ = 25f;

        [Header("Opacity")]
        [Range(0f, 1f)][SerializeField] private float minAlpha = 0.25f;
        [Range(0f, 1f)][SerializeField] private float maxAlpha = 0.85f;

        private float beatInterval;
        private float timer;
        private float lastBpm;
        private SpriteRenderer sr;
        private bool paused;
        private bool alphaOverrideActive;
        private float alphaOverrideValue;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            CalculateBeatInterval();
            lastBpm = bpm;
        }

        private void Update()
        {
            if (paused)
                return;

            if (!Mathf.Approximately(lastBpm, bpm))
            {
                CalculateBeatInterval();
                lastBpm = bpm;
            }

            timer += Time.deltaTime;

            float localTime = Mathf.Repeat(timer + beatOffset * beatInterval, beatInterval);
            float t = Mathf.PingPong(localTime / beatInterval, 0.5f) * 2f;

            // rotation
            float z = Mathf.Lerp(minZ, maxZ, t);
            var rot = transform.localEulerAngles;
            rot.z = z;
            transform.localEulerAngles = rot;

            // alpha
            var c = sr.color;
            if (alphaOverrideActive)
                c.a = Mathf.Clamp01(alphaOverrideValue);
            else
                c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
            sr.color = c;
        }

        public void SetBPM(float newBpm)
        {
            if (newBpm <= 0f)
            {
                paused = true;
                bpm = 0f;
                return;
            }

            paused = false;
            bpm = newBpm;
            beatInterval = 60f / bpm;
        }

        public void TurnOff()
        {
            alphaOverrideActive = true;
            alphaOverrideValue = 0f;
        }

        public void TurnOn(float alpha)
        {
            alphaOverrideActive = true;
            alphaOverrideValue = Mathf.Clamp01(alpha);
        }

        public void TurnOn()
        {
            alphaOverrideActive = false;
        }

        private void CalculateBeatInterval()
        {
            beatInterval = 60f / Mathf.Max(1f, bpm);
            timer = 0f; // resync on bpm change
        }
    }
}
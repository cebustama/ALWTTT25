using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Backgrounds
{
    public class ForegroundAnimator : MonoBehaviour
    {
        [Header("Beat Settings")]
        [SerializeField] private float bpm = 120f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpHeight = 0.5f;
        [SerializeField] private AnimationCurve jumpCurve =
            AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Foreground Targets")]
        [SerializeField] private Transform foregroundLayerRoot;

        [Header("TODO")]
        [SerializeField] private List<ForegroundLayerConfig> layerConfigList;

        // TODO: multiple layers with different timing offsets
        [Serializable] public class ForegroundLayerConfig
        {
            public Transform layerRoot;
            public float bpm;
            public float jumpHeight;
            public AnimationCurve jumpCurve;
        }

        private float beatInterval;
        private float timer;
        private Vector3 originalPosition;
        private float lastBPM;

        private void Start()
        {
            originalPosition = foregroundLayerRoot.localPosition;
            CalculateBeatInterval();
            lastBPM = bpm;
        }

        private void Update()
        {
            // Detect runtime changes to BPM via Inspector
            if (!Mathf.Approximately(bpm, lastBPM))
            {
                CalculateBeatInterval();
                lastBPM = bpm;
            }

            timer += Time.deltaTime;

            // Loop every beat
            if (timer >= beatInterval)
            {
                timer -= beatInterval;
            }

            // Normalized ping-pong value: 0 → 1 → 0 (alt + numpad 26)
            float pingPong = Mathf.PingPong(timer / beatInterval, 0.5f) * 2f;

            // Evaluate using custom curve
            float curved = jumpCurve.Evaluate(pingPong);
            float jumpOffset = curved * jumpHeight;

            foregroundLayerRoot.localPosition = originalPosition + Vector3.up * jumpOffset;
        }

        public void SetBPM(float newBpm)
        {
            bpm = newBpm;
            CalculateBeatInterval();
        }

        private void CalculateBeatInterval()
        {
            beatInterval = 60f / bpm;
        }
    }

}
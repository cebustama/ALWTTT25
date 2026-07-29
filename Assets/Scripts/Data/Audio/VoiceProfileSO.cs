using UnityEngine;

namespace ALWTTT.Music.Voice
{
    /// <summary>
    /// [SINGER-1] Serialized voice identity for the Pink Trombone singer.
    /// Schema source: PinkTrombone_Voice_Levers.md §5 (Session 5, singer v7).
    /// The profile is the RESTING state; gameplay animates the levers at
    /// runtime (lever doc §3 tier 2). Tier 3 (phrase-metadata automation)
    /// is Phase D4 and stays deferred.
    /// </summary>
    [CreateAssetMenu(fileName = "VoiceProfile",
        menuName = "ALWTTT/Audio/Voice Profile")]
    public class VoiceProfileSO : ScriptableObject
    {
        public enum MouthPreset { Neutral = 0, Open = 1, Front = 2, Back = 3 }

        [Header("Macro levers (the designed surface)")]
        [Range(0f, 1f)] public float looseness = 0.15f;
        [Range(0f, 1f)] public float vibratoDepth = 0.4f;
        [Range(3f, 9f)] public float vibratoSpeedHz = 6f;
        [Range(0f, 2f)] public float diction = 1f;
        public MouthPreset mouth = MouthPreset.Neutral;
        [Range(0f, 1f)] public float brightness = 0.5f;

        [Header("Identity (rarely changed — lever doc §2)")]
        public int transposeSemitones = -12;
        [Range(0f, 1f)] public float tensenessAtVel0 = 0.40f;
        [Range(0f, 1f)] public float tensenessAtVel127 = 0.60f;
        [Range(0f, 2f)] public float vibratoDelaySeconds = 0.35f;
        [Range(0f, 2f)] public float vibratoRampSeconds = 0.4f;
        [Range(0f, 0.3f)] public float pitchLeadSeconds = 0.06f;
        [Range(1, 12)] public int leadFullInterval = 7;
        [Range(0f, 1f)] public float minLoudness = 0.15f;

        [Header("Transport (D1=A residual constant)")]
        [Tooltip("Per-profile start trim vs MPTK, ms. Positive = sing later. " +
                 "Measured once per device class via ST-V2; expected small " +
                 "and constant (MPTK synth latency after OnSongStarted).")]
        [Range(-250f, 250f)] public float startTrimMs = 0f;

        [Header("Output")]
        [Range(0f, 2f)] public float gain = 1f;

        // Gameplay-modulation hooks (lever doc §5 note): deliberately absent
        // in SINGER-1. Section-level "when" is driven by gameplay code calling
        // the live levers on SingerVoice, not by data here. Add per-state
        // overrides only when a concrete consumer exists.
    }
}
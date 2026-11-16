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
        [Tooltip("VOZ-1 (D-VOZ-2): range widened 0-1 -> 0-3 so the workshop modes fit " +
                 "(Opera 2.6, Crooner 1.4, Borracho 1.2). Values up to 1 sound as before.")]
        [Range(0f, 3f)] public float vibratoDepth = 0.4f;
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

        // ------------------------------------------------------------------
        // VOZ-1 (D-VOZ-2 = A, 2026-11-14): VoiceProfile v2. The singer v8 is the
        // C# port of the Taller de melodias' cantarV2 (VOZ-2). These fields are
        // read ONLY when engine = V8; with V7 the profile sounds exactly as before.
        // Names and defaults follow the workshop ("Copiar para la carta" ->
        // propuestaVoiceProfileV2; motor-canto.js BASE), so an imported mode maps 1:1.
        // ------------------------------------------------------------------
        public enum SingerEngine { V7_CLite = 0, V8_Workshop = 1 }
        public enum BaseVowel { Neutral = 0, A = 1, E = 2, I = 3, O = 4, U = 5 }

        [Header("v2 - engine (D-VOZ-7: V8 for the demo, V7 is the opt-out)")]
        [Tooltip("V7_CLite = today's singer (vowels only). V8_Workshop = the workshop's " +
                 "singer: syllables, scoop, fall, glide, X1-X3 (VoiceRenderer, VOZ-2). Live: " +
                 "Inspector edits are heard while it sings.")]
        public SingerEngine engine = SingerEngine.V7_CLite;

        [Header("v2 - throat (Pink Trombone X2/X3, POC-FORK 9-10)")]
        [Tooltip("X2 tract sections. 44 = adult; longer = low larynx, dark (Opera 50, Death 52); " +
                 "shorter = high larynx, bright (Punk 40, Falsete 38).")]
        [Range(36, 56)] public int tractLength = 44;
        [Tooltip("X3 aspiration gain. 1 = upstream; Tierno 1.8, Death 3.")]
        [Range(0f, 3f)] public float breath = 1f;
        [Tooltip("X3 period doubling (subharmonic growl). Death metal 0.85.")]
        [Range(0f, 1f)] public float growl = 0f;
        [Tooltip("X3 per-cycle roughness: jitter = roughness, shimmer = 0.8 x roughness.")]
        [Range(0f, 1f)] public float roughness = 0f;
        [Tooltip("Mic saturation (tanh drive) after the voice. Punk 0.45.")]
        [Range(0f, 1f)] public float drive = 0f;

        [Header("v2 - phrasing (singer v8)")]
        [Tooltip("Note entry from below, in semitones.")]
        [Range(0f, 3f)] public float scoopSemitones = 0f;
        [Range(10f, 250f)] public float scoopMs = 90f;
        [Tooltip("Fall at the end of a phrase, in semitones.")]
        [Range(0f, 4f)] public float fallSemitones = 0f;
        [Tooltip("Portamento between notes. 0 = jumps (uses pitch lead instead).")]
        [Range(0f, 250f)] public float glideMs = 0f;
        [Tooltip("Extra tenseness on the first 80 ms of a note (pushed attack).")]
        [Range(0f, 0.3f)] public float attackPress = 0f;
        [Tooltip("How close notes must be to tie without re-articulating.")]
        [Range(0f, 1f)] public float legato = 0.3f;
        [Tooltip("Vowel used when a note has no syllable.")]
        public BaseVowel baseVowel = BaseVowel.Neutral;

        [Header("v2 - syllables (D-VOZ-4, VOZ-2)")]
        [Tooltip("What the v8 singer says: zigues, a weighted list or a text. " +
                 "Empty = every note on the base vowel.")]
        public SyllableBankSO syllableBank;

        /// <summary>
        /// [VOZ-2] Copies the levers into the v8 renderer's live parameters (no allocation).
        /// Called at arm time and every frame while the voice sings, so an Inspector edit in
        /// Play mode is heard at once (tract length and legato wait for the next loop).
        /// </summary>
        public void ToV8Params(V8.VoiceV8Params p)
        {
            p.Looseness = looseness; p.VibratoDepth = vibratoDepth; p.VibratoSpeedHz = vibratoSpeedHz;
            p.VibratoDelay = vibratoDelaySeconds; p.VibratoRamp = vibratoRampSeconds; p.Brightness = brightness;
            p.Transpose = transposeSemitones; p.TensLo = tensenessAtVel0; p.TensHi = tensenessAtVel127;
            p.MinLoudness = minLoudness; p.PitchLead = pitchLeadSeconds; p.TractLength = tractLength;
            p.Breath = breath; p.Growl = growl; p.Roughness = roughness; p.Drive = drive;
            p.Scoop = scoopSemitones; p.ScoopMs = scoopMs; p.Fall = fallSemitones; p.GlideMs = glideMs;
            p.Attack = attackPress; p.Legato = legato; p.BaseVowel = VowelChar(baseVowel);
        }

        public static char VowelChar(BaseVowel v)
        {
            switch (v)
            {
                case BaseVowel.A: return 'a';
                case BaseVowel.E: return 'e';
                case BaseVowel.I: return 'i';
                case BaseVowel.O: return 'o';
                case BaseVowel.U: return 'u';
                default: return (char)0x0259;   // schwa, the workshop's neutral vowel
            }
        }

        // Gameplay-modulation hooks (lever doc §5 note): deliberately absent
        // in SINGER-1. Section-level "when" is driven by gameplay code calling
        // the live levers on SingerVoice, not by data here. Add per-state
        // overrides only when a concrete consumer exists.
    }
}
// [VOZ-2] Singer v8 — the levers of the Taller de melodías' cantarV2 (motor-canto.js BASE).
// Pure C# (no UnityEngine): compiled by Unity AND by the dotnet parity harness
// (Docs/reference/taller-melodia/paridad).
//
// LIVE LEVERS (D7 = A, 2026-11-14): VoiceRenderer reads these fields at every 256-sample
// control block, so editing them while the voice sings is heard within ~5 ms. The main
// thread writes plain float/int fields (atomic on every Unity target); the audio thread
// only reads. Exceptions, fixed at arm time: TractLength (rebuilds the model) and Legato
// (decides which notes tie, part of the note plan).
namespace ALWTTT.Music.Voice.V8
{
    public sealed class VoiceV8Params
    {
        // ---- the workshop's BASE (same defaults) ----
        public float Looseness = 0.15f;
        public float VibratoDepth = 0.4f;
        public float VibratoSpeedHz = 6f;
        public float VibratoDelay = 0.35f;
        public float VibratoRamp = 0.4f;
        public float Brightness = 0.5f;
        public int Transpose = -12;
        public float TensLo = 0.4f;
        public float TensHi = 0.6f;
        public float MinLoudness = 0.15f;
        public float PitchLead = 0.06f;
        public int TractLength = 44;          // arm time only
        public float Breath = 1f;             // JS aire
        public float Growl = 0f;
        public float Roughness = 0f;          // JS aspereza (jitter; shimmer = 0.8 x)
        public float Drive = 0f;              // JS saturacion
        public float Scoop = 0f;              // semitones
        public float ScoopMs = 90f;
        public float Fall = 0f;               // JS caida, semitones
        public float GlideMs = 0f;
        public float Attack = 0f;             // JS ataque (extra tenseness, first 80 ms)
        public float Legato = 0.3f;           // arm time only
        public char BaseVowel = 'ə';          // vowel for notes without a syllable

        // ---- gameplay live offsets (neutral = parity with the workshop) ----
        /// <summary>Added to the tenseness after velocity mapping. + = pressed/closed throat, - = breathy.</summary>
        public float LiveTensenessOffset = 0f;
        /// <summary>Multiplies the vibrato depth. 1 = profile.</summary>
        public float LiveVibratoScale = 1f;
        /// <summary>Added to Growl (clamped 0..1).</summary>
        public float LiveGrowlOffset = 0f;

        public VoiceV8Params Clone() => (VoiceV8Params)MemberwiseClone();

        /// <summary>Copies every lever from <paramref name="o"/> (live-edit path; no allocation).</summary>
        public void CopyFrom(VoiceV8Params o)
        {
            Looseness = o.Looseness; VibratoDepth = o.VibratoDepth; VibratoSpeedHz = o.VibratoSpeedHz;
            VibratoDelay = o.VibratoDelay; VibratoRamp = o.VibratoRamp; Brightness = o.Brightness;
            Transpose = o.Transpose; TensLo = o.TensLo; TensHi = o.TensHi; MinLoudness = o.MinLoudness;
            PitchLead = o.PitchLead; TractLength = o.TractLength; Breath = o.Breath; Growl = o.Growl;
            Roughness = o.Roughness; Drive = o.Drive; Scoop = o.Scoop; ScoopMs = o.ScoopMs; Fall = o.Fall;
            GlideMs = o.GlideMs; Attack = o.Attack; Legato = o.Legato; BaseVowel = o.BaseVowel;
        }
    }

    /// <summary>One sung note, in seconds from the loop start (the workshop's aSegundos shape).</summary>
    public struct SungNote
    {
        public double Start, End;   // seconds
        public int Note;            // MIDI, before transpose
        public int Vel;             // 0..127
        public double StartBeats;   // beats from the loop start (phrase-end bar grid)
        public double DurBeats;     // for the syllable rules (the workshop works in beats)
        public bool PhraseEnd;      // JS finFrase (deduced from the stem in the game: D1 = B)
        public bool Fall;           // JS caida (forced fall >= 2 st at the end of the note)
        public string Syllable;     // null/"" = base vowel
    }
}

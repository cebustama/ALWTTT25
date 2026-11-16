// [VOZ-2] The singer v8: C# port of motor-canto.js cantarV2 (Taller de melodías), with
// JS = C# parity (Docs/reference/taller-melodia/paridad/cantante). Pure C# (D-VOZ-6): the
// same class sings in real time (SingerVoice.OnAudioFilterRead) and offline (editor, VOZ-3).
//
// Control rate (D3 = A): decisions every 256 samples, ALWAYS, whatever the caller's buffer
// size. Render() keeps a 256-sample carry so a 1024-, 512- or 441-frame DSP buffer and an
// offline render produce the same samples.
//
// Threads: Prepare() runs on the main thread (allocates: the model, the plan, the syllables).
// Render() runs on the audio thread and allocates nothing: constrictions go through a fixed
// buffer, the fricative turbulence point is one pre-built object reused.
//
// D4 = A: no C-lite here (no M1–M3, no interval-scaled lead). Syllables move the mouth.
using System;
using Vocal;

namespace ALWTTT.Music.Voice.V8
{
    public sealed class VoiceRenderer
    {
        public const int ControlBlock = 256;

        struct PlanNote
        {
            public double Start, End, CStart;
            public int Note, Vel;
            public bool LegatoNext, Fall;
            public int C, C2;            // onset consonant indices (Phonetics.Cons), -1 none
            public int Vowel, Vowel2, Coda;
            public bool Hum;
        }

        readonly int sampleRate;
        readonly PinkThrombone v;
        readonly double sc;
        readonly PlanNote[] ev;
        readonly Constriction[] constr = new Constriction[8];   // TractShaper.MaxConstrictions (internal to the fork assembly)
        readonly TurbulencePoint turbObj = new TurbulencePoint();
        readonly float[] block = new float[ControlBlock];

        /// <summary>The live levers. Read every control block; see VoiceV8Params.</summary>
        public readonly VoiceV8Params P;

        // ---- render state (audio thread) ----
        long blockIndex;
        double t0;
        int cur = -1, lastNote = -1;
        bool hasSemis, touched, turbActive;
        double semis, lp1, lp2, noteOnT;
        int carryPos = ControlBlock;   // == ControlBlock: no carried samples

        public int NoteCount => ev.Length;
        public double EndTime { get; }
        public int TractLength => v.TractLength;

        /// <summary>
        /// Main thread. Builds the model (tract length from <paramref name="p"/>) and the note plan.
        /// <paramref name="p"/> is kept by reference: the caller may keep writing its fields live.
        /// <paramref name="startTime"/> is the loop time of the first rendered sample (0 = parity).
        /// </summary>
        public VoiceRenderer(SungNote[] notes, VoiceV8Params p, int sampleRate, int noiseSeed, double startTime = 0)
        {
            this.sampleRate = sampleRate;
            P = p;
            t0 = startTime;
            v = new PinkThrombone(sampleRate, new StandardRandomSource(noiseSeed), (int)Math.Round((double)p.TractLength));
            sc = v.TractLength / 44.0;
            v.Growl = p.Growl; v.PitchRoughness = p.Roughness; v.AmpRoughness = p.Roughness * 0.8; v.Breath = p.Breath;
            v.VibratoWobble = false;
            v.PitchJitterGain = Clamp01f(p.Looseness); v.TensenessJitterGain = Clamp01f(p.Looseness);
            v.AlwaysVoice = false; v.IsTouched = false; v.VibratoFrequency = p.VibratoSpeedHz;

            ev = new PlanNote[notes.Length];
            string baseVowel = p.BaseVowel.ToString();
            for (int i = 0; i < notes.Length; i++)
            {
                var n = notes[i];
                var s = Phonetics.Parse(string.IsNullOrEmpty(n.Syllable) ? baseVowel : n.Syllable);
                ev[i] = new PlanNote
                {
                    Start = n.Start, End = n.End, Note = n.Note, Vel = n.Vel, Fall = n.Fall,
                    C = s.Onset0, C2 = s.Onset1, Vowel = s.Vowel, Vowel2 = s.Vowel2, Coda = s.Coda, Hum = s.IsHum,
                };
            }
            for (int i = 0; i < ev.Length; i++)
            {
                double prevEnd = i > 0 ? ev[i - 1].End : -1;
                double pre = ev[i].C >= 0 ? PreOf(Phonetics.Cons[ev[i].C].Kind) : 0;
                ev[i].CStart = ev[i].Start - Math.Min(pre, Math.Max(0.02, (ev[i].Start - prevEnd) + 0.06));
                ev[i].LegatoNext = i < ev.Length - 1 && ev[i + 1].Start - ev[i].End < 0.03 + 0.15 * p.Legato;
            }
            EndTime = ev.Length > 0 ? ev[ev.Length - 1].End : 0;
            v.Reset();
        }

        static double PreOf(ConsKind k)
        {
            switch (k)
            {
                case ConsKind.Stop: return 0.075;
                case ConsKind.Nasal: return 0.08;
                case ConsKind.Fricative: return 0.1;
                case ConsKind.Approximant: return 0.07;
                case ConsKind.Trill: return 0.03;
                case ConsKind.Aspirate: return 0.07;
                default: return 0.06;
            }
        }

        static float Clamp01f(float x) => x < 0 ? 0 : (x > 1 ? 1 : x);
        static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);
        static double Clamp(double x, double a, double b) => Math.Min(b, Math.Max(a, x));

        /// <summary>
        /// Audio thread, once, at the dsp anchor and before the first Render: the loop time of
        /// the first sample (negative = the start trim still to wait; positive = a late start).
        /// </summary>
        public void StartAt(double loopTime) { if (blockIndex == 0 && carryPos >= ControlBlock) t0 = loopTime; }

        /// <summary>Loop time (s) of the next control block.</summary>
        public double Time => t0 + blockIndex * (double)ControlBlock / sampleRate;

        /// <summary>Audio thread. Fills <paramref name="dst"/> with finished samples (tone stage and drive included).</summary>
        public void Render(Span<float> dst)
        {
            int w = 0;
            while (w < dst.Length)
            {
                if (carryPos >= ControlBlock) { RenderBlock(); carryPos = 0; }
                int n = Math.Min(ControlBlock - carryPos, dst.Length - w);
                new ReadOnlySpan<float>(block, carryPos, n).CopyTo(dst.Slice(w, n));
                carryPos += n; w += n;
            }
        }

        void RenderBlock()
        {
            const int B = ControlBlock;
            var V = P;
            double SR = sampleRate;
            double t = Time;
            int nc = 0;

            // live levers that the workshop sets once (same value every block = parity)
            v.Growl = Clamp01((double)V.Growl + V.LiveGrowlOffset); v.PitchRoughness = V.Roughness; v.AmpRoughness = V.Roughness * 0.8; v.Breath = V.Breath;
            v.PitchJitterGain = Clamp01f(V.Looseness); v.TensenessJitterGain = Clamp01f(V.Looseness);
            v.VibratoFrequency = V.VibratoSpeedHz;

            while (cur + 1 < ev.Length && ev[cur + 1].CStart <= t) cur++;
            bool hasE = cur >= 0;
            double? velo = null;
            bool wantTouch = false;
            // JS works in double: read every float lever into a double before any arithmetic
            // (int - float or float - float would round to float and break parity).
            double tensLo = V.TensLo, tensHi = V.TensHi, minLoud = V.MinLoudness, scoop = V.Scoop, scoopS = V.ScoopMs / 1000.0;
            double fallP = V.Fall, glideMs = V.GlideMs, pitchLead = V.PitchLead, attack = V.Attack;
            int transpose = V.Transpose;
            double tens = tensLo, loud = 0.5;
            if (hasE)
            {
                ref PlanNote e = ref ev[cur];
                bool enCons = t < e.Start;
                double hasta = e.LegatoNext ? ev[cur + 1].CStart : e.End;
                bool vivo = t < hasta;
                double vel01 = Clamp01(e.Vel / 127.0);
                loud = Clamp01(minLoud + (1 - minLoud) * vel01);
                tens = tensLo + (tensHi - tensLo) * vel01;
                if (t - e.Start < 0.08 && t >= e.Start) tens += attack;     // pushed attack
                // vowel
                var vo = Phonetics.Vowels[e.Vowel];
                v.TongueIndex = vo.Index; v.TongueDiameter = vo.Diameter;
                if (vo.HasLip && vo.Lip < 1.5) constr[nc++] = new Constriction(41, vo.Lip);
                // second vowel (diphthong) from the middle of the note
                if (e.Vowel2 >= 0 && t > (e.Start + e.End) / 2)
                {
                    var v2 = Phonetics.Vowels[e.Vowel2];
                    v.TongueIndex = v2.Index; v.TongueDiameter = v2.Diameter;
                    nc = 0;
                    if (v2.HasLip) constr[nc++] = new Constriction(41, v2.Lip);
                }
                if (e.Hum) { constr[nc++] = new Constriction(41, 0); velo = 0.4; }
                // onset consonant
                if (e.C >= 0 && enCons)
                {
                    var c = (e.C2 >= 0 && e.Start - t < 0.035) ? Phonetics.Cons[e.C2] : Phonetics.Cons[e.C];
                    if (c.Kind == ConsKind.Aspirate) { tens = 0.08; wantTouch = true; }
                    else
                    {
                        constr[nc++] = new Constriction(c.Index, c.Diameter);
                        if (c.Kind == ConsKind.Nasal) velo = 0.4;
                        wantTouch = c.Voiced;
                    }
                }
                else if (vivo) wantTouch = true;
                // coda in the last 80 ms
                if (e.Coda >= 0 && vivo && hasta - t < 0.08)
                {
                    var c = Phonetics.Cons[e.Coda];
                    if (c.HasIndex && c.Index != 0 && nc < constr.Length) constr[nc++] = new Constriction(c.Index, c.Diameter);
                }
                if (!vivo) wantTouch = false;
                // pitch: scoop, fall, glide
                double target = e.Note + transpose;
                double sinceOn = t - e.Start;
                if (scoop > 0 && sinceOn >= 0 && sinceOn < scoopS) target -= scoop * (1 - sinceOn / scoopS);
                bool phraseFinal = !e.LegatoNext;
                if ((fallP > 0 && phraseFinal) || e.Fall)
                {
                    double fall = e.Fall ? Math.Max(fallP, 2) : fallP, fw = 0.18, left = hasta - t;
                    if (left < fw && left >= 0) { double q = 1 - left / fw; target -= fall * (q * q); }
                }
                if (cur != lastNote) { noteOnT = e.Start; lastNote = cur; if (!hasSemis) { semis = target; hasSemis = true; } }
                if (!hasSemis || glideMs <= 0) { semis = target; hasSemis = true; }
                else semis += (target - semis) * (1 - Math.Exp(-((double)B / SR) / (glideMs / 1000.0)));
                if (pitchLead > 0 && cur + 1 < ev.Length && ev[cur + 1].Start - t < pitchLead && glideMs <= 0)
                    semis = ev[cur + 1].Note + transpose - (scoop > 0 ? scoop : 0);
                v.SetMusicalNote((float)(semis - 69));
            }
            // fricative noise at the consonant's place, while it lasts (one live point at a time)
            bool fric = hasE && ev[cur].C >= 0 && Phonetics.Cons[ev[cur].C].Kind == ConsKind.Fricative && t < ev[cur].Start;
            var tps = v.TurbulencePoints;
            if (fric && !turbActive)
            {
                var c = Phonetics.Cons[ev[cur].C];
                turbObj.Position = c.Index * sc; turbObj.Diameter = c.Diameter;
                turbObj.StartTime = v.TractTime; turbObj.EndTime = float.NaN;
                tps.Add(turbObj); turbActive = true;
            }
            if (!fric && turbActive && float.IsNaN(turbObj.EndTime)) turbObj.EndTime = v.TractTime;
            if (turbActive && !float.IsNaN(turbObj.EndTime) && (double)v.TractTime - turbObj.EndTime > 0.15)
            { tps.Remove(turbObj); turbActive = false; }

            v.SetConstrictions(new ReadOnlySpan<Constriction>(constr, 0, nc));
            v.VelumOverride = velo;
            if (wantTouch != touched) { v.IsTouched = wantTouch; touched = wantTouch; }
            v.Loudness = (float)loud;
            v.TargetTenseness = (float)Clamp01(tens + V.LiveTensenessOffset);
            double vibRamp = V.VibratoRamp, vibDelay = V.VibratoDelay, vibDepth = (double)V.VibratoDepth * V.LiveVibratoScale;
            double held = t - noteOnT, vr = vibRamp <= 0 ? 1 : Clamp01((held - vibDelay) / vibRamp);
            v.VibratoGain = (float)Clamp(vibDepth * 0.012 * vr, 0, 0.1);

            v.Synthesize(new Span<float>(block, 0, B));

            double bright = V.Brightness;
            double a = 1 - Math.Exp(-2 * Math.PI * (1200 * Math.Pow(2, 3.1 * bright)) / SR);
            double drive = V.Drive, k = 1 + 12 * drive, norm = Math.Tanh(k * 0.25);
            for (int i = 0; i < B; i++)
            {
                lp1 += a * (block[i] - lp1); lp2 += a * (lp1 - lp2);
                double s = lp2;
                if (drive > 0) s = Math.Tanh(k * s) / norm * 0.25;
                block[i] = (float)s;
            }
            blockIndex++;
        }
    }
}

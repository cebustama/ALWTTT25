namespace Vocal
{
    using System;
    using System.Collections.Generic;


    using static PinkTrombone.Arg;

    public sealed class PinkThrombone
    {
        const int maxBlockLength = 512;

        readonly Glottis glottis;
        readonly Tract tract;
        readonly TractShaper shaper;
        readonly int sampleRate;

        public PinkThrombone(int sampleRate, IRandomSource random)
        {
            if (sampleRate <= 0 || sampleRate >= int.MaxValue / 2)
                throw new ArgumentOutOfRangeException(nameof(sampleRate));
            if (random is null) throw new ArgumentNullException(nameof(random));

            this.sampleRate = sampleRate;
            this.glottis = new Glottis(sampleRate, random);
            // tract runs at twice the sample rate
            this.tract = new Tract(this.glottis, sampleRate: 2 * sampleRate, random);
            this.shaper = new TractShaper(this.tract);
        }

        /// <summary>
        /// -1..+1
        /// </summary>
        [Obsolete("NotImplemented", error: true)]
        public float Noise
        {
            get => throw new NotImplementedException();
            set
            {
                if (value < -1 || value > 1)
                    throw new ArgumentOutOfRangeException(nameof(this.Noise));
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// 0..1
        /// </summary>
        public float Intensity
        {
            get => this.glottis.Intensity;
            set => this.glottis.Intensity = Check01(value);
        }
        /// <summary>
        /// 0..1. POC-FORK(1): user amplitude (e.g. MIDI velocity). Multiplies the
        /// waveform independently of the tenseness-derived internal loudness.
        /// </summary>
        public float Loudness
        {
            get => this.glottis.Loudness;
            set => this.glottis.Loudness = Check01(value);
        }

        // POC-FORK(3): surface the glottis articulation gate. With AlwaysVoice=false,
        // IsTouched drives the internal intensity envelope (+0.13/block on,
        // -0.05/block off), giving note-on/off articulation inside the model while
        // breath persists through rests.
        public bool IsTouched
        {
            get => this.glottis.IsTouched;
            set => this.glottis.IsTouched = value;
        }
        public bool AlwaysVoice
        {
            get => this.glottis.AlwaysVoice;
            set => this.glottis.AlwaysVoice = value;
        }
        /// <summary>
        /// 0..
        /// </summary>
        public float TargetFrequency
        {
            get => this.glottis.TargetFrequency;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(this.TargetFrequency));
                this.glottis.TargetFrequency = value;
            }
        }
        /// <summary>
        /// 0..1
        /// </summary>
        public float TargetTenseness
        {
            get => this.glottis.TargetTenseness;
            set => this.glottis.TargetTenseness = Check01(value);
        }
        /// <summary>
        /// 0..44 <see cref="Tract.n"/>
        /// </summary>
        public double TongueIndex
        {
            get => this.shaper.TongueIndex;
            set
            {
                if (value < -1 || value > Tract.n + 1)
                    throw new ArgumentOutOfRangeException(nameof(this.TongueIndex));
                this.shaper.TongueIndex = value;
            }
        }
        /// <summary>
        /// 0..3(?)
        /// </summary>
        public double TongueDiameter
        {
            get => this.shaper.TongueDiameter;
            set => this.shaper.TongueDiameter = value;
        }

        /// <summary>
        /// 0..
        /// </summary>
        public float VibratoGain
        {
            get => this.glottis.VibratoAmount;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(this.VibratoGain));
                this.glottis.VibratoAmount = value;
            }
        }
        /// <summary>
        /// 0..
        /// </summary>
        public float VibratoFrequency
        {
            get => this.glottis.VibratoFrequency;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(this.VibratoFrequency));
                this.glottis.VibratoFrequency = value;
            }
        }
        public bool VibratoWobble
        {
            get => this.glottis.AutoWobble;
            set => this.glottis.AutoWobble = value;
        }
        /// <summary>
        /// 0..1. POC-FORK(6): scales the model's always-on simplex F0 jitter
        /// (upstream fixed at 1, ±~90 cents peak). 0 = stable pitch.
        /// </summary>
        public float PitchJitterGain
        {
            get => this.glottis.PitchJitterGain;
            set => this.glottis.PitchJitterGain = Check01(value);
        }
        /// <summary>
        /// 0..1. POC-FORK(7): scales the model's always-on simplex tenseness
        /// drift (upstream fixed at 1, ±~0.11). 0 = steady timbre.
        /// </summary>
        public float TensenessJitterGain
        {
            get => this.glottis.TensenessJitterGain;
            set => this.glottis.TensenessJitterGain = Check01(value);
        }
        public IList<TurbulencePoint> TurbulencePoints => this.tract.turbulencePoints;

        /// <summary>
        /// Set <see cref="TargetFrequency"/> to the specified musical note.
        /// </summary>
        /// <param name="semitone">Semitone, based at A4.</param>
        public void SetMusicalNote(float semitone)
        {
            this.glottis.SetMusicalNote(semitone);
        }

        public void Synthesize(Span<float> buf)
        {
            int p = 0;
            while (p < buf.Length)
            {
                int blockLength = Math.Min(maxBlockLength, buf.Length - p);
                var blockBuf = buf.Slice(p, blockLength);
                this.SynthesizeBlock(blockBuf);
                p += blockLength;
            }
        }

        public void Reset()
        {
            this.CalculateNewBlockParameters(0);
        }

        int totalBlocks = 0;
        void SynthesizeBlock(Span<float> buf)
        {
            float deltaTime = buf.Length * 1f / this.sampleRate;
            this.CalculateNewBlockParameters(deltaTime);
            for (int i = 0; i < buf.Length; i++)
            {
                double lambda1 = i * 1.0 / buf.Length;
                double lambda2 = (i + 0.5) / buf.Length;
                double glottalOutput = this.glottis.Step((float)lambda1);
                float vocalOutput1 = this.tract.Step(glottalOutput, lambda1);
                float vocalOutput2 = this.tract.Step(glottalOutput, lambda2);
                buf[i] = (vocalOutput1 + vocalOutput2) * 0.125f;
            }
            this.totalBlocks++; // POC-FORK(4): removed per-block Debug.WriteLine (fired ~90x/s in-editor)
        }

        void CalculateNewBlockParameters(float deltaTime)
        {
            this.glottis.AdjustParameters(deltaTime);
            this.shaper.AdjustTractShape(deltaTime);
            this.tract.CalculateNewBlockParameters();
        }
    }
}
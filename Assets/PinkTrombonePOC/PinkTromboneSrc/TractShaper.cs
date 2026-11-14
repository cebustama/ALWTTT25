namespace Vocal {
    using System;
    internal class TractShaper {
        const double gridOffset = 1.7;

        readonly Tract tract;

        const double movementSpeed = 15;
        public float VelumOpenTarget { get; set; } = 0.4f;
        public float VelumClosedTarget { get; set; } = 0.01f;

        readonly double[] targetDiameter;
        public float VelumTarget { get; set; }

        // POC-FORK(2): upstream computed targetDiameter[] only in the constructor
        // (ShapeMainTract), freezing the vowel at the defaults forever. The tongue
        // setters now raise a dirty flag; AdjustTractShape refills targetDiameter[]
        // (targets ONLY — not the live tract.diameter[], so the existing MoveTowards
        // glide slides the vowel instead of snapping it). The flag is written on the
        // main thread and consumed on the audio thread; a benign race can delay the
        // recompute by at most one block — acceptable at POC tier.
        bool tongueDirty;
        double tongueIndex = 12.9;
        public double TongueIndex {
            get => this.tongueIndex;
            set { if (value != this.tongueIndex) { this.tongueIndex = value; this.tongueDirty = true; } }
        }
        double tongueDiameter = 2.43;
        public double TongueDiameter {
            get => this.tongueDiameter;
            set { if (value != this.tongueDiameter) { this.tongueDiameter = value; this.tongueDirty = true; } }
        }

        int lastObstruction = -1;

        // POC-FORK(11) X1: free constrictions (consonants), the touch algorithm of the
        // original web Pink Trombone (TractUI), ported from motor-voz.js. Index is in the
        // 44-space (scaled to the tract length); diameter 0 = closed. An empty set leaves
        // the upstream path untouched. Fixed capacity: no allocation on the audio thread.
        public const int MaxConstrictions = 8;
        readonly Constriction[] constrictions = new Constriction[MaxConstrictions];
        int constrictionCount;
        bool constrDirty;
        /// <summary>POC-FORK(11): null = upstream velum; a value overrides the velum target (0.4 = open, nasals).</summary>
        public double? VelumOverride { get; set; }

        public void SetConstrictions(ReadOnlySpan<Constriction> list) {
            int c = Math.Min(list.Length, MaxConstrictions);
            for (int i = 0; i < c; i++) this.constrictions[i] = list[i];
            this.constrictionCount = c;
            this.constrDirty = true;
        }

        public TractShaper(Tract tract) {
            this.tract = tract ?? throw new ArgumentNullException(nameof(tract));

            this.targetDiameter = new double[tract.n];
            this.ShapeNoise(true);
            tract.CalculateNoseReflections(); // (nose reflections are calculated only once, but with open velum)
            this.ShapeNoise(false);
            this.ShapeMainTract();
        }

        void ShapeMainTract() {
            for (int i = 0; i < this.tract.n; i++) {
                double d = this.GetRestDiameter(i);
                this.tract.diameter[i] = d;
                this.targetDiameter[i] = d;
            }
        }

        public double GetRestDiameter(int i) {
            var T = this.tract;
            if (i < T.GlottalEnd) return 0.6;
            if (i < T.BladeStart) return 1.1;
            if (i >= T.LipStart) return 1.5;

            double ti = T.n == Tract.DefaultLength ? this.TongueIndex : this.TongueIndex * T.n / 44; // POC-FORK(10)
            double t = 1.1 * Math.PI * (ti - i) / (T.TipStart - T.BladeStart);
            double fixedTongueDiameter = 2 + (this.TongueDiameter - 2) / 1.5;
            double curve = (1.5 - fixedTongueDiameter + gridOffset) * Math.Cos(t);
            if (i == T.BladeStart - 2 || i == T.LipStart - 1) {
                curve *= 0.8;
            }
            if (i == T.BladeStart || i == T.LipStart - 2) {
                curve *= 0.94;
            }
            return 1.5 - curve;
        }

        // Adjusts the shape of the tract towards the target values.
        public void AdjustTractShape(double deltaTime) {
            var T = this.tract;
            // POC-FORK(2): recompute targets when the tongue moved.
            // POC-FORK(11): also every block while constrictions are set.
            if (this.tongueDirty || this.constrDirty) {
                this.tongueDirty = false;
                for (int i = 0; i < T.n; i++)
                    this.targetDiameter[i] = this.GetRestDiameter(i);
                if (this.constrictionCount > 0) this.ApplyConstrictions();
                this.constrDirty = this.constrictionCount > 0;
            }
            double amount = deltaTime * movementSpeed;
            int newLastObstruction = -1;
            for (int i = 0; i < T.n; i++) {
                double diameter = this.tract.diameter[i];
                double targetDiameter = this.targetDiameter[i];
                if (diameter <= 0) {
                    newLastObstruction = i;
                }
                double slowReturn;
                if (i < T.NoseStart)
                    slowReturn = 0.6;
                else if (i >= T.TipStart)
                    slowReturn = 1;
                else
                    slowReturn = 0.6 + 0.4 * (i - T.NoseStart) / (T.TipStart - T.NoseStart);

                this.tract.diameter[i] = MathX.MoveTowards(diameter, targetDiameter, slowReturn * amount, 2 * amount);
            }
            if (this.lastObstruction >= 0 && newLastObstruction < 0 && this.tract.noseDiameter[0] < 0.223) {
                this.AddTransient(this.lastObstruction);
            }
            this.lastObstruction = newLastObstruction;
            double velum = this.VelumOverride ?? this.VelumTarget; // POC-FORK(11)
            this.tract.noseDiameter[0] = MathX.MoveTowards(this.tract.noseDiameter[0], velum, amount * 0.25, amount * 0.1);
        }

        // POC-FORK(11): TractUI.setRestDiameter's touch loop (the UI's "diameter -= 0.3"
        // is already applied by the caller: 0 = closed). Only narrows, never widens.
        void ApplyConstrictions() {
            var T = this.tract;
            int n = T.n;
            double sc = n / 44.0;
            for (int c = 0; c < this.constrictionCount; c++) {
                double index = this.constrictions[c].Index * sc;
                double diameter = Math.Max(0, this.constrictions[c].Diameter);
                double width;
                if (index < 25 * sc) width = 10 * sc;
                else if (index >= T.TipStart) width = 5 * sc;
                else width = (10 - 5 * (index - 25 * sc) / (T.TipStart - 25 * sc)) * sc;
                if (!(index >= 2 && index < n && diameter < 3)) continue;
                int ii = (int)Math.Floor(index + 0.5); // JS Math.round
                for (int i = -(int)Math.Ceiling(width) - 1; i < width + 1; i++) {
                    int k = ii + i;
                    if (k < 0 || k >= n) continue;
                    double rel = Math.Abs(k - index) - 0.5, shrink;
                    if (rel <= 0) shrink = 0;
                    else if (rel > width) shrink = 1;
                    else shrink = 0.5 * (1 - Math.Cos(Math.PI * rel / width));
                    if (diameter < this.targetDiameter[k])
                        this.targetDiameter[k] = diameter + (this.targetDiameter[k] - diameter) * shrink;
                }
            }
        }

        void AddTransient(int position) {
            this.tract.transients.Add(new Transient {
                Position = position,
                StartTime = this.tract.time,
                LifeTime = 0.2,
                Strength = 0.3,
                Exponent = 200,
            });
        }

        void ShapeNoise(bool velumOpen) {
            this.VelumTarget = velumOpen ? this.VelumOpenTarget : this.VelumClosedTarget;
            for (int i = 0; i < this.tract.NoseLength; i++) {
                double d = i * 2.0 / this.tract.NoseLength;
                double diameter;
                if (i == 0)
                    diameter = this.VelumTarget;
                else if (d < 1)
                    diameter = 0.4 + 1.6 * d;
                else
                    diameter = 0.5 + 1.5 * (2 - d);

                diameter = Math.Min(diameter, 1.9);
                this.tract.noseDiameter[i] = diameter;
            }
        }
    }

    /// <summary>POC-FORK(11): a constriction in the 44-space. Diameter 0 = closed, 0.55 = fricative.</summary>
    public readonly struct Constriction {
        public readonly double Index, Diameter;
        public Constriction(double index, double diameter) { this.Index = index; this.Diameter = diameter; }
    }
}

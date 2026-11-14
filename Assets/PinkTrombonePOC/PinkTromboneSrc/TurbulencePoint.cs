namespace Vocal {
    using System;
    // POC-FORK(8): upstream's Position setter validated the OLD value (initially 0),
    // so it threw on every assignment and no fricative could ever be added. Position
    // and Diameter are now double (the JS reference keeps them in double) and range
    // checks live in Tract.AddTurbulenceNoise, which already skips points outside
    // 2..n; the valid range depends on the tract length (POC-FORK 10).
    public sealed class TurbulencePoint {
        double diameter;

        /// <summary>Index in the tract's own space (2..n).</summary>
        public double Position { get; set; }
        /// <summary>
        /// 0..
        /// </summary>
        public double Diameter {
            get => this.diameter;
            set {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.Diameter));
                this.diameter = value;
            }
        }
        public float StartTime { get; set; }
        public float EndTime { get; set; } = float.NaN;
    }
}

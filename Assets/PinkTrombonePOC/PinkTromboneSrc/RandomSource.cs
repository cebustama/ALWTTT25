namespace Vocal {
    using System;

    // POC-FORK(5): upstream depended on the Troschuetz.Random NuGet package purely
    // for two members — IGenerator.Seed and IGenerator.NextDouble(). That package
    // was only ever present in this project as a transitive dependency of the
    // PinkTrombone NuGet package, so removing PinkTrombone (required by the source
    // fork, to avoid a duplicate Vocal.PinkThrombone type) removed it too, and it
    // does not surface as a standalone package in NuGetForUnity's search.
    //
    // Vendoring the two-member surface here removes the external dependency
    // entirely. Signatures are otherwise unchanged; swap IRandomSource back for
    // Troschuetz.Random.IGenerator if the fork is ever pushed upstream.

    /// <summary>Minimal random-source surface used by the vocal model.</summary>
    public interface IRandomSource {
        /// <summary>Seed value; used to initialise the simplex noise generator.</summary>
        uint Seed { get; }
        /// <summary>Uniform double in [0, 1).</summary>
        double NextDouble();
    }

    /// <summary>Default <see cref="IRandomSource"/> backed by System.Random.</summary>
    public sealed class StandardRandomSource : IRandomSource {
        readonly Random random;

        public uint Seed { get; }

        /// <summary>Time-seeded (non-deterministic) source.</summary>
        public StandardRandomSource() : this(Environment.TickCount) { }

        /// <summary>Explicitly seeded source — same seed gives the same noise.</summary>
        public StandardRandomSource(int seed) {
            this.Seed = unchecked((uint)seed);
            this.random = new Random(seed);
        }

        public double NextDouble() => this.random.NextDouble();
    }
}

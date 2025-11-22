using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Music
{
    /// <summary>
    /// Aggregated info for a single part: which loops were played,
    /// and  how each audience member reacted to each loop.
    /// </summary>
    public readonly struct PartFeedbackContext
    {
        public int PartIndex { get; }
        public string PartLabel { get; }

        /// <summary>Immutable view over all loop snapshots in this part.</summary>
        public IReadOnlyList<LoopFeedbackContext> Loops { get; }

        /// <summary>
        /// Container for per-audience member impressions per loop.
        /// Key: audience member index / id
        /// Value: list of impression values (-2..2) for each loop.
        /// For now we leave it empty; GigManager or another system can fill it later.
        /// </summary>
        public Dictionary<int, List<int>> AudienceLoopImpressions { get; }

        public int TotalLoops => Loops?.Count ?? 0;

        public PartFeedbackContext(
            int partIndex,
            string partLabel,
            IReadOnlyList<LoopFeedbackContext> loops,
            Dictionary<int, List<int>> audienceLoopImpressions)
        {
            PartIndex = partIndex;
            PartLabel = partLabel ?? $"Part {partIndex}";
            Loops = loops ?? System.Array.Empty<LoopFeedbackContext>();
            AudienceLoopImpressions =
                audienceLoopImpressions ?? new Dictionary<int, List<int>>();
        }

        public override string ToString()
        {
            return $"[PartFeedback] Part={PartIndex} ({PartLabel}) Loops={TotalLoops}";
        }
    }
}
using UnityEngine;

namespace ALWTTT.Music
{
    /// <summary>
    /// Snapshot of information about a loop that just finished playing.
    /// This is what gets broadcast to audience members so they can
    /// evaluate it and turn it into an impression (-2..2).
    /// </summary>
    public readonly struct LoopFeedbackContext
    {
        public int PartIndex { get; }
        /// <summary>0-based index of this loop within the current part.</summary>
        public int LoopIndexWithinPart { get; }
        public int LoopsInPart { get; }

        public string PartLabel { get; }

        /// <summary>How much inspiration this loop granted (if any).</summary>
        public int InspirationGainedThisLoop { get; }
        /// <summary>Total inspiration after this loop.</summary>
        public int InspirationAfterLoop { get; }

        public bool IsLastLoopOfPart => LoopIndexWithinPart == LoopsInPart - 1;

        public LoopFeedbackContext(
            int partIndex,
            int loopIndexWithinPart,
            int loopsInPart,
            string partLabel,
            int inspirationGainedThisLoop,
            int inspirationAfterLoop)
        {
            PartIndex = partIndex;
            LoopIndexWithinPart = loopIndexWithinPart;
            LoopsInPart = loopsInPart;
            PartLabel = partLabel ?? $"Part {partIndex}";
            InspirationGainedThisLoop = inspirationGainedThisLoop;
            InspirationAfterLoop = inspirationAfterLoop;
        }

        public override string ToString()
        {
            return $"[LoopFeedback] Part={PartIndex} ({PartLabel}) " +
                   $"Loop={LoopIndexWithinPart + 1}/{LoopsInPart}, " +
                   $"ΔInsp={InspirationGainedThisLoop}, Insp={InspirationAfterLoop}";
        }
    }
}
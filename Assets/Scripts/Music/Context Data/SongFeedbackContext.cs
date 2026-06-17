using System.Collections.Generic;

namespace ALWTTT.Music
{
    /// <summary>
    /// Aggregated info for an entire live-composed song (one CompositionSession).
    /// </summary>
    public readonly struct SongFeedbackContext
    {
        public IReadOnlyList<PartFeedbackContext> Parts { get; }

        public int PartCount => Parts?.Count ?? 0;

        public SongFeedbackContext(IReadOnlyList<PartFeedbackContext> parts)
        {
            Parts = parts ?? System.Array.Empty<PartFeedbackContext>();
        }

        public override string ToString()
        {
            return $"[SongFeedback] Parts={PartCount}";
        }
    }
}
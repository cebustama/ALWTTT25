// Place at: Assets/Scripts/Tutorial/TutorialLoopHoldGate.cs
namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [TUT-R2 / beat 8 holdLoop] Cooperative gate that holds the FINAL loop of
    /// the current part: while armed, CompositionSession.HandleLoopFinished
    /// replays the last loop instead of resolving it. The held repeat is
    /// invisible to loop accounting — no loop decrement, NO inspiration re-grant
    /// (neither track-derived nor the host per-loop flat grant, because
    /// LoopFinished is not raised), no LoopResolvedEvent, no micro-reactions.
    ///
    /// Armed by TutorialGuidedDriver at the start of the last loop when the
    /// finisher is available and affordable (degrade path (b) otherwise);
    /// released when the finisher is played, on song end, or defensively when
    /// the session ends / the driver disables. CompositionSession also releases
    /// it on a failed loop re-render before ending the session gracefully.
    /// </summary>
    public static class TutorialLoopHoldGate
    {
        public static bool IsArmed { get; private set; }

        /// <summary>Called only by TutorialGuidedDriver.</summary>
        public static void Arm() => IsArmed = true;

        /// <summary>Called by TutorialGuidedDriver (satisfy/degrade/disable) and
        /// defensively by CompositionSession on End/render failure.</summary>
        public static void Release() => IsArmed = false;
    }
}
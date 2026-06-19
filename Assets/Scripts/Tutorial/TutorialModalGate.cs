namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [S4 D-TUT-4 modal behavior] Cooperative gate that lets a showing tutorial
    /// modal suspend gameplay without a timeScale freeze (so in-progress animations
    /// and MIDI audio keep running, per Design_Tutorial_System §4).
    ///
    /// <see cref="TutorialController"/> raises <see cref="IsActive"/> while any
    /// modal is on screen and lowers it when the queue drains. Consumers cooperate:
    ///   - GigManager.AudienceTurnRoutine yields at its top while active
    ///     (suspends audience turns).
    ///   - The controller also calls HandController.DisableDragging while active
    ///     (cards undraggable).
    ///
    /// Static + scene-agnostic so the one-line GigManager guard needs no reference
    /// wiring. Reset to false defensively on controller disable.
    /// </summary>
    public static class TutorialModalGate
    {
        /// <summary>True while a tutorial modal is on screen.</summary>
        public static bool IsActive { get; private set; }

        /// <summary>Called only by TutorialController.</summary>
        public static void Set(bool active) => IsActive = active;
    }
}

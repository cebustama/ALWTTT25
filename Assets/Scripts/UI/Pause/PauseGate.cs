using UnityEngine;

namespace ALWTTT.UI
{
    /// <summary>
    /// [PAUSE-1 / D12=C] Global "the game is paused" condition, readable from anywhere.
    ///
    /// Why a gate of its own: Time.timeScale = 0 does NOT stop Update() or Input polling,
    /// so any system that reads input in Update (HandController) must ask explicitly. The
    /// existing HandController.IsDraggingActive flag is already shared by two owners
    /// (TutorialController's modal gate and GigManager.SetHandVisible); a third owner
    /// toggling it would re-enable dragging that someone else had disabled. This gate is
    /// orthogonal: readers AND it with their own conditions, nobody restores anybody.
    ///
    /// Single writer: PauseMenuController. Same pattern as TutorialModalGate.
    /// </summary>
    public static class PauseGate
    {
        public static bool IsPaused { get; private set; }

        /// <summary>Only PauseMenuController calls this.</summary>
        public static void Set(bool paused) => IsPaused = paused;

        // Enter-Play-Mode without domain reload keeps statics alive; start unpaused.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad() => IsPaused = false;
    }
}
using UnityEngine;

namespace ALWTTT.Core
{
    /// <summary>
    /// [LOG-1] Console hygiene, applied before any scene loads.
    ///
    /// Measured on the R3 reference capture (log11.txt): 762 of 864 console
    /// lines were stack traces attached to plain Debug.Log calls. That is 88%
    /// of the volume carrying no information — every host log line is already
    /// tagged with the class that emitted it, so the trace only repeats what
    /// the tag says.
    ///
    /// Warning and Error keep their traces, which is where a trace is actually
    /// wanted: those fire on paths you did not expect to reach.
    ///
    /// This removes no log line and degrades no log line. It is the only
    /// LOG-1 change with zero information cost, which is why it lands first
    /// and is measured on its own before anything is demoted to verbose.
    /// </summary>
    public static class AlwtttLogSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => SetPlainLogTraces(false);

        /// <summary>
        /// Escape hatch. Call with true from Dev Mode (or the editor menu
        /// below) when you need to find where an untagged Debug.Log is coming
        /// from. The setting is global and lasts for the play session.
        /// </summary>
        public static void SetPlainLogTraces(bool enabled)
        {
            Application.SetStackTraceLogType(
                LogType.Log,
                enabled ? StackTraceLogType.ScriptOnly : StackTraceLogType.None);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("ALWTTT/Debug/Log stack traces/Enable")]
        private static void EnableTraces() => SetPlainLogTraces(true);

        [UnityEditor.MenuItem("ALWTTT/Debug/Log stack traces/Disable")]
        private static void DisableTraces() => SetPlainLogTraces(false);
#endif
    }
}
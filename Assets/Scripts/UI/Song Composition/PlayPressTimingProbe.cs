// Place at: Assets/Scripts/UI/Song Composition/PlayPressTimingProbe.cs
using System;
using ALWTTT.Data;
using ALWTTT.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    /// <summary>
    /// [PLAY-TIMING-0] Log-only probe: how close was the Play press to a bar line of the
    /// loop that is playing (in practice the song tail, D-ST7=C)? No gameplay effect.
    ///
    ///   D-PT1=A  target = the bar line that CLOSES the current loop (early side) or the one
    ///            that OPENED it (late side, D-PT2=B) - whichever is nearer.
    ///   D-PT3=A  grades in absolute ms (GigFlowSettingsSO): PERFECT / EXCELLENT / GOOD
    ///            (= the window), else FUERA DE VENTANA; a calibration offset is subtracted.
    ///   D-PT4=B  measured at POINTER-DOWN on the Play button (Button.onClick fires on
    ///            release, 50-150 ms later); GigManager.OnPlayPressed grades and logs it.
    ///            Fallback when no down was seen (keyboard, touch edge cases): measure at
    ///            the click and label it "(click)".
    ///
    /// Added to the Play button by SongCompositionUI.HookPlayButton. Reads
    /// MidiMusicManager.TryGetLoopTiming only; never writes game state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayPressTimingProbe : MonoBehaviour, IPointerDownHandler
    {
        private const string Tag = "[PLAY-TIMING]";

        /// <summary>A down older than this is not the down of the current click.</summary>
        private const float MaxPressToClickSeconds = 2f;

        private struct Snapshot
        {
            public bool hasLoop;
            public bool approximate;
            public double toClose;      // s until the closing bar line (+inf if unknown)
            public double sinceStart;   // s since the opening bar line
            public double loopSeconds;
            public float unscaledTime;
        }

        private static bool s_hasPress;
        private static Snapshot s_press;

        private Button _button;

        private void Awake() => _button = GetComponent<Button>();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (_button != null && !_button.IsInteractable()) return;
            s_press = Capture();
            s_hasPress = true;
        }

        private static Snapshot Capture()
        {
            var s = new Snapshot { unscaledTime = Time.unscaledTime };
            var mm = MidiMusicManager.Instance;
            s.hasLoop = mm != null && mm.TryGetLoopTiming(
                out s.toClose, out s.sinceStart, out s.loopSeconds, out s.approximate);
            return s;
        }

        /// <summary>Called once per accepted Play by GigManager.OnPlayPressed, before the
        /// synchronous render. Consumes the pointer-down snapshot of this click.</summary>
        public static void LogForPlay(GigFlowSettingsSO flow)
        {
            bool fromDown = s_hasPress &&
                            Time.unscaledTime - s_press.unscaledTime <= MaxPressToClickSeconds;
            Snapshot s = fromDown ? s_press : Capture();
            s_hasPress = false;

            if (flow == null || !flow.PlayTimingProbeEnabled) return;

            string src = fromDown ? "pointer-down" : "click";
            if (!s.hasLoop)
            {
                Debug.Log($"{Tag} ({src}) sin loop sonando: sin medida.");
                return;
            }

            // Signed error to the nearest bar line: negative = early (before the closing
            // bar line), positive = late (after the opening one).
            double raw = s.toClose <= s.sinceStart ? -s.toClose : s.sinceStart;
            double adj = raw - flow.PlayTimingOffsetMs / 1000.0;
            double absMs = Math.Abs(adj) * 1000.0;
            double windowMs = flow.PlayTimingWindowSeconds * 1000.0;

            string grade = absMs <= flow.PlayTimingPerfectMs ? "PERFECT"
                         : absMs <= flow.PlayTimingExcellentMs ? "EXCELLENT"
                         : absMs <= windowMs ? "GOOD"
                         : "FUERA DE VENTANA";
            // [PLAY-TIMING-0 fix F-PT-1] Outside the window, EARLY/LATE only says which edge
            // of the loop is nearer, which reads as a verdict. Report both distances instead.
            if (absMs > windowMs)
            {
                string toClose = double.IsInfinity(s.toClose) ? "?" : $"{s.toClose:0.00} s";
                Debug.Log($"{Tag} ({src}) FUERA DE VENTANA: {s.sinceStart:0.00} s desde el inicio " +
                          $"del loop, faltaban {toClose} para su cierre (ventana ±{windowMs:0} ms, " +
                          $"loop {s.loopSeconds:0.00} s)");
                return;
            }
            string side = adj < 0 ? "EARLY" : "LATE";
            string approx = s.approximate ? "~" : "";

            Debug.Log($"{Tag} ({src}) {grade} {side} {approx}{adj * 1000.0:+0;-0} ms " +
                      $"(bruto {approx}{raw * 1000.0:+0;-0} ms, offset {flow.PlayTimingOffsetMs:0} ms, " +
                      $"ventana ±{windowMs:0} ms, loop {s.loopSeconds:0.00} s)");
        }
    }
}
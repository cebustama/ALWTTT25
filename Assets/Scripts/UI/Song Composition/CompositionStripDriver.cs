// Placement: Assets/Scripts/UI/Song Composition/CompositionStripDriver.cs  (NEW)
//
// [HUD-COMP-1] Keeps the context row's loop pips honest.
//
// WHY POLLING AND NOT AN EVENT:
// CompositionSession publishes LoopFinished / PartFinished / SongFinished, but
// there is NO "loop started" event. The pip that must light up is the CURRENT
// loop, which changes at loop START. We could add a LoopStarted event, but that
// widens a governed runtime contract for a purely cosmetic consumer — a bad
// trade for a UI batch. So we poll a cheap struct four times a second and only
// touch the UI when the values actually changed.
//
// If a LoopStarted event is ever added for gameplay reasons, this class becomes
// a two-line subscriber and the polling goes away. Recorded as a follow-up, not
// a debt we are hiding.

using System.Collections.Generic;
using ALWTTT.Managers;
using UnityEngine;

namespace ALWTTT.UI
{
    public class CompositionStripDriver : MonoBehaviour
    {
        [SerializeField] private List<SongPartElementUI> partElements = new();
        [SerializeField, Tooltip("Seconds between polls. 0.25 is well under one " +
                                 "loop at any tempo the game ships.")]
        private float pollInterval = 0.25f;

        private float _timer;
        private int _lastLoop = -1;
        private int _lastTotal = -1;
        private bool _lastLocked;

        public void Register(SongPartElementUI ui)
        {
            if (ui != null && !partElements.Contains(ui)) partElements.Add(ui);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < pollInterval) return;
            _timer = 0f;

            var gm = GigManager.Instance;
            if (gm == null) return;
            if (!gm.TryGetLoopProgressForUI(out int cur, out int total, out bool locked)) return;

            if (cur == _lastLoop && total == _lastTotal && locked == _lastLocked) return;
            _lastLoop = cur; _lastTotal = total; _lastLocked = locked;

            // RedrawAll() destroys and re-creates every part element, so this
            // list accumulates Unity-null entries. Prune before iterating —
            // cheap, and it keeps the list from growing across songs.
            for (int i = partElements.Count - 1; i >= 0; i--)
                if (partElements[i] == null) partElements.RemoveAt(i);

            foreach (var p in partElements) if (p != null) p.RefreshContext();
        }
    }
}
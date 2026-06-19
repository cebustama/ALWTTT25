using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Managers;
using ALWTTT.Sensory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [S4 D-TUT-3 / D-TUT-10 / D-TUT-11] First-time tutorial driver.
    ///
    /// Subscribes to the Sensory bus (one subscription surface, D-S4-SRC=A), maps
    /// each event to candidate trigger ids, and for any candidate not yet in the
    /// persisted firedDialogs set enqueues its authored dialog. A single-modal FIFO
    /// queue ordered by authored priority (D-TUT-10) drains one dialog at a time;
    /// while any dialog shows, the gameplay gate is raised (audience turns suspend,
    /// cards lock). NOT an ordered step-state machine — gameplay causality enforces
    /// the §6A jam arc.
    ///
    /// Fired state persists via PersistentGameplayData (in-memory across scene loads
    /// within a run; there is no disk save system in the project today).
    ///
    /// Deviation note: the audience turn is hard-suspended via one cooperative guard
    /// (TutorialModalGate, read by GigManager.AudienceTurnRoutine). The composition
    /// loop / MIDI playback is NOT paused under a modal (pausing mid-loop would
    /// desync audio); any loop event that fires while a modal is up simply queues.
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private TutorialDialogCatalogSO catalog;
        [SerializeField] private TutorialOverlayView overlay;
        [SerializeField] private Sprite defaultPortrait;
        [SerializeField] private Sprite defaultHoleShape;

        [Header("Debug")]
        [Tooltip("Logs every event received, enqueue decision, dialog shown, and gate " +
                 "change. Turn OFF before demo/ship.")]
        [SerializeField] private bool verboseLogging = true;

        private const string DebugTag = "<color=#7fd4ff>[Tutorial]</color>";
        private void Log(string msg) { if (verboseLogging) Debug.Log($"{DebugTag} {msg}"); }

        [Serializable]
        public class HighlightBinding
        {
            [Tooltip("Matches TutorialDialogSO.highlightKey.")]
            public string key;
            [Tooltip("Optional per-binding hole sprite; null uses the default.")]
            public Sprite holeShape;

            [Tooltip("Centre + auto-size the spotlight from this RectTransform.")]
            public RectTransform target;

            [Tooltip("Ignore the target; place the spotlight at the manual viewport centre below.")]
            public bool useManualCenter;
            [Tooltip("Viewport centre, 0..1, origin bottom-left. (0.5,0.5)=screen centre, " +
                     "(0.5,0.85)=top-centre. Resolution-independent.")]
            public Vector2 manualCenter = new Vector2(0.5f, 0.5f);

            [Tooltip("Override size: half-extents as a fraction of the smaller screen dimension. " +
                     "Equal x/y = circle; x≠y = oval; set only x to widen. (0,0) = auto-size from target.")]
            public Vector2 manualRadius = Vector2.zero;
        }

        [Header("Highlight targets (R1)")]
        [SerializeField] private List<HighlightBinding> highlightBindings = new();

        // Queue ordered by priority (lower first). Small N; linear insert is fine.
        private readonly List<TutorialDialogSO> _queue = new();
        private bool _showing;
        // [D-S4-DEDUP] Id of the dialog currently on screen (null = none). A dialog is
        // dequeued in PumpQueue BEFORE it is marked fired in OnDialogComplete, so without
        // this guard an event that republishes while its own modal is up — SongEndVibeEvent
        // fires once per audience member, SfxStageCrossedEvent once per crossing — passes
        // both HasFired and QueueContains and re-enqueues, showing the modal twice.
        private string _showingId;
        private bool _sfxStageSeen;
        // Debounced pump handle. Co-occurring triggers (e.g. an action card that also
        // applies a status) arrive as separate events across a frame or two; we collect
        // them all into the priority-sorted queue, then pump, so PRIORITY decides display
        // order rather than which event was published first.
        private Coroutine _pumpCo;

        // ---------------- lifecycle ----------------
        private void OnEnable()
        {
            catalog?.BuildIndex();
            var bus = SensoryEventBus.Instance;
            if (bus == null)
            {
                Log("OnEnable: SensoryEventBus.Instance is NULL — NOT subscribed. " +
                    "(If this prints, the bus didn't exist yet when the controller enabled.)");
                return;
            }

            bus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            bus.Subscribe<LoopResolvedEvent>(OnLoopResolved);
            bus.Subscribe<SfxStageCrossedEvent>(OnSfxStage);
            bus.Subscribe<SongEndVibeEvent>(OnSongEnd);
            bus.Subscribe<AudienceTurnStartedEvent>(OnAudienceTurn);
            bus.Subscribe<StatusAppliedEvent>(OnStatusApplied);
            bus.Subscribe<GigStartedEvent>(OnGigStarted);
            bus.Subscribe<GigOutcomeEvent>(OnGigOutcome);
            Log($"OnEnable: subscribed to 8 events (frame {Time.frameCount}).");
        }

        private void OnDisable()
        {
            var bus = SensoryEventBus.Instance;
            if (bus != null)
            {
                bus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
                bus.Unsubscribe<LoopResolvedEvent>(OnLoopResolved);
                bus.Unsubscribe<SfxStageCrossedEvent>(OnSfxStage);
                bus.Unsubscribe<SongEndVibeEvent>(OnSongEnd);
                bus.Unsubscribe<AudienceTurnStartedEvent>(OnAudienceTurn);
                bus.Unsubscribe<StatusAppliedEvent>(OnStatusApplied);
                bus.Unsubscribe<GigStartedEvent>(OnGigStarted);
                bus.Unsubscribe<GigOutcomeEvent>(OnGigOutcome);
            }
            // Defensive: never leave gameplay gated if disabled mid-modal.
            if (_pumpCo != null) { StopCoroutine(_pumpCo); _pumpCo = null; }
            ReleaseGate();
        }

        // ---------------- event → trigger mapping ----------------
        private void OnGigStarted(GigStartedEvent e)
        {
            Log($"event GigStartedEvent (requiredSongs={e.RequiredSongCount}, frame {Time.frameCount})");
            TryEnqueue(TutorialTriggerId.WelcomeToGig);
        }

        private void OnCardPlayed(CardPlayedEvent e)
        {
            Log($"event CardPlayedEvent (card='{(e.Definition != null ? e.Definition.name : "null")}', " +
                $"comp={e.IsComposition}, action={e.IsAction}, cost={e.InspirationCost})");
            if (e.IsAction)
                TryEnqueue(TutorialTriggerId.FirstActionCard);
            if (e.IsComposition)
            {
                TryEnqueue(TutorialTriggerId.FirstCompositionCard);          // beat 1
                if (e.InspirationCost > 0)
                    TryEnqueue(TutorialTriggerId.FirstInspirationSpend);     // beat 2
                if (IsSoundCard(e.Definition))
                    TryEnqueue(TutorialTriggerId.FirstSoundCard);            // beat 5
            }
        }

        private void OnLoopResolved(LoopResolvedEvent e)
        {
            Log($"event LoopResolvedEvent (inspirationGainedThisLoop={e.Context.InspirationGainedThisLoop})");
            if (e.Context.InspirationGainedThisLoop > 0)
                TryEnqueue(TutorialTriggerId.FirstLoopInspiration);          // beat 3
        }

        private void OnSfxStage(SfxStageCrossedEvent e)
        {
            Log("event SfxStageCrossedEvent (stage-seen flag set)");
            _sfxStageSeen = true;
            TryEnqueue(TutorialTriggerId.FirstSfxStage);                     // beat 4
        }

        private void OnSongEnd(SongEndVibeEvent e)
        {
            Log("event SongEndVibeEvent");
            TryEnqueue(TutorialTriggerId.FirstSongEnd);                      // beat 6 (fires once via fired-guard)
        }

        private void OnAudienceTurn(AudienceTurnStartedEvent e)
        {
            // Design: first audience turn AFTER first SongHype stage crossing.
            Log($"event AudienceTurnStartedEvent (stageSeen={_sfxStageSeen})");
            if (_sfxStageSeen)
                TryEnqueue(TutorialTriggerId.FirstAudienceAction);
        }

        private void OnStatusApplied(StatusAppliedEvent e)
        {
            Log($"event StatusAppliedEvent (status={e.Status}, delta={e.DeltaStacks})");
            TryEnqueue(TutorialTriggerId.FirstStatusApplied);
        }

        private void OnGigOutcome(GigOutcomeEvent e)
        {
            Log($"event GigOutcomeEvent (won={e.Won})");
            if (e.Won) TryEnqueue(TutorialTriggerId.FirstGigWon);
        }

        private static bool IsSoundCard(ALWTTT.Cards.CardDefinition def)
        {
            var comp = def != null ? def.CompositionPayload : null;
            if (comp == null) return false;
            return CompositionCardClassifier.IsTempoCard(comp)
                || CompositionCardClassifier.IsModulationCard(comp);
        }

        // ---------------- queue ----------------
        // Pump is DEBOUNCED: each enqueue (re)schedules a short coalescing window, and
        // the queue pumps once a couple of frames pass with no further enqueue. This
        // gathers all triggers that originate from one player action — including ones
        // published at different points of the same card-play pipeline (an action
        // card's effect applies a status BEFORE DeckManager.OnCardPlayed fires) — into
        // the priority-sorted queue first, so the lowest-priority dialog shows first
        // regardless of publish order (D-TUT-10). Beat 1 before beat 2; action (15)
        // before status (25).
        private void SchedulePump()
        {
            if (_showing) return;                       // resumes on dialog complete
            if (_pumpCo != null) StopCoroutine(_pumpCo);
            _pumpCo = StartCoroutine(PumpAfterCoalesce());
        }

        private System.Collections.IEnumerator PumpAfterCoalesce()
        {
            // Two-frame quiet window: long enough to catch a status published one frame
            // before the card-played event, short enough to be imperceptible (~33ms).
            yield return null;
            yield return null;
            _pumpCo = null;
            PumpQueue();
        }

        private void TryEnqueue(string triggerId)
        {
            if (string.IsNullOrEmpty(triggerId)) return;
            if (HasFired(triggerId)) { Log($"  ↳ '{triggerId}' skipped (already fired)"); return; }
            if (QueueContains(triggerId)) { Log($"  ↳ '{triggerId}' skipped (already queued)"); return; }
            if (_showing && _showingId == triggerId) { Log($"  ↳ '{triggerId}' skipped (already showing)"); return; }

            var dialog = catalog != null ? catalog.Get(triggerId) : null;
            if (dialog == null)
            {
                Debug.LogWarning($"{DebugTag} No dialog authored for trigger " +
                                 $"'{triggerId}' — skipping (coverage gap). " +
                                 $"Did you run the catalog's Seed menu / assign the catalog?");
                return;
            }

            InsertByPriority(dialog);
            Log($"  ↳ '{triggerId}' ENQUEUED (priority {dialog.Priority}, queue size {_queue.Count})");
            SchedulePump();
        }

        private void InsertByPriority(TutorialDialogSO dialog)
        {
            int i = 0;
            while (i < _queue.Count && _queue[i].Priority <= dialog.Priority) i++;
            _queue.Insert(i, dialog);
        }

        private bool QueueContains(string id)
        {
            for (int i = 0; i < _queue.Count; i++)
                if (_queue[i] != null && _queue[i].TriggerId == id) return true;
            return false;
        }

        private void PumpQueue()
        {
            if (_showing) return;
            if (_queue.Count == 0) { ReleaseGate(); return; }

            RaiseGate();
            var next = _queue[0];
            _queue.RemoveAt(0);
            _showing = true;
            _showingId = next.TriggerId;   // [D-S4-DEDUP]

            var spot = ResolveHighlight(next);
            Log($"SHOW '{next.TriggerId}' (highlight='{(next.HasHighlight ? next.HighlightKey : "none")}', " +
                $"spotlight={(spot.Enabled ? (spot.Target != null ? spot.Target.name : "manual") : "none")})");
            overlay.Show(next, spot, defaultPortrait,
                onComplete: () => OnDialogComplete(next));
        }

        private void OnDialogComplete(TutorialDialogSO dialog)
        {
            // Skip and normal completion both record the dialog as fired (D-TUT-2).
            MarkFired(dialog.TriggerId);
            Log($"COMPLETE '{dialog.TriggerId}' → marked fired (remaining queue {_queue.Count})");
            _showing = false;
            _showingId = null;             // [D-S4-DEDUP]

            if (_queue.Count > 0)
            {
                PumpQueue();           // chain next without dropping the gate
            }
            else
            {
                overlay.Hide();
                ReleaseGate();
            }
        }

        private Spotlight ResolveHighlight(TutorialDialogSO dialog)
        {
            if (dialog == null || !dialog.HasHighlight) return Spotlight.None;
            for (int i = 0; i < highlightBindings.Count; i++)
            {
                var b = highlightBindings[i];
                if (b == null || b.key != dialog.HighlightKey) continue;

                bool hasTarget = b.target != null;
                if (!hasTarget && !b.useManualCenter) return Spotlight.None; // unbound → no spotlight

                return new Spotlight
                {
                    Enabled = true,
                    HoleShape = b.holeShape != null ? b.holeShape : defaultHoleShape,
                    Target = b.useManualCenter ? null : b.target,
                    ManualCenterVp = b.manualCenter,
                    ManualRadiusFrac = b.manualRadius
                };
            }
            return Spotlight.None;
        }

        // ---------------- gameplay gate ----------------
        private void RaiseGate()
        {
            if (TutorialModalGate.IsActive) return;
            TutorialModalGate.Set(true);
            DeckManager.Instance?.HandController?.DisableDragging();
            Log("gate RAISED (cards locked; audience turn will wait at its next boundary)");
        }

        private void ReleaseGate()
        {
            if (!TutorialModalGate.IsActive) return;
            TutorialModalGate.Set(false);
            DeckManager.Instance?.HandController?.EnableDragging();
            Log("gate RELEASED");
        }

        // ---------------- persistence ----------------
        private PersistentGameplayData PD =>
            GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;

        private bool HasFired(string id)
        {
            var pd = PD;
            return pd != null && pd.HasFiredTutorial(id);
        }

        private void MarkFired(string id)
        {
            PD?.MarkTutorialFired(id);
        }

        // ---------------- public API (revisit / reset, D-TUT-2 / D-TUT-11) ----------------
        /// <summary>Fired dialogs only (already-encountered), newest authoring order.</summary>
        public IReadOnlyList<TutorialDialogSO> GetFiredDialogs()
        {
            var result = new List<TutorialDialogSO>();
            var pd = PD;
            if (pd == null || catalog == null) return result;
            foreach (var d in catalog.Dialogs)
                if (d != null && pd.HasFiredTutorial(d.TriggerId))
                    result.Add(d);
            return result;
        }

        /// <summary>Replay a dialog modally WITHOUT re-marking or gating gameplay
        /// (used from the revisit submenu; does not re-trigger).</summary>
        public void ReplayDialog(TutorialDialogSO dialog)
        {
            if (dialog == null || _showing) return;
            var spot = ResolveHighlight(dialog);
            // No gate, no fired-state change; just present and hide on complete.
            overlay.Show(dialog, spot, defaultPortrait,
                onComplete: () => overlay.Hide());
        }

        /// <summary>Clear the entire fired set so every tutorial re-shows on its next
        /// trigger (pause-menu "Reset tutorials", after a confirm prompt upstream).</summary>
        public void ResetAllTutorials()
        {
            PD?.ClearFiredTutorials();
        }
    }
}
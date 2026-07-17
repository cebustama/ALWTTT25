using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Managers;
using ALWTTT.Sensory;
using ALWTTT.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

            [Tooltip("Opcional: UIPulseAnimator a pulsar rítmicamente mientras " +
                     "el diálogo de este highlight esté en pantalla (botón Play, " +
                     "contador de Inspiración, icono de estado...). El componente " +
                     "se añade al GameObject destino y se referencia aquí.")]
            public UIPulseAnimator pulse;

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
        private Coroutine _pulseCo; // [TUT-R2b] pulso del highlight activo

        private Coroutine _persistPulseCo; // [DEMO-FIXES-A / CT1]

        // [CT1] A directive is "alive" while an input gate or the loop hold is
        // armed. Beats 3/5 arm inside the DialogCompleted invoke; beat 8 arms
        // BEFORE its dialog shows — both shapes are covered by checking AFTER
        // the invoke in OnDialogComplete.
        private static bool DirectiveActive =>
            TutorialInputGate.IsActive || TutorialLoopHoldGate.IsArmed;

        private void TryStartPersistentPulse(TutorialDialogSO dialog)
        {
            // Never re-target while one runs: an unrelated reactive dialog
            // completing mid-directive must not steal the directive's pulse.
            if (_persistPulseCo != null) return;
            if (!DirectiveActive) return;

            var pulse = ResolvePulse(dialog);
            if (pulse == null) return;

            _persistPulseCo = StartCoroutine(PulseWhileDirective(pulse));
            Log($"persistent pulse STARTED for '{dialog.TriggerId}' (until directive clears)");
        }

        private void StopPersistentPulse()
        {
            if (_persistPulseCo != null) { StopCoroutine(_persistPulseCo); _persistPulseCo = null; }
        }

        private System.Collections.IEnumerator PulseWhileDirective(UIPulseAnimator pulse)
        {
            var wait = new WaitForSeconds(0.9f);
            while (DirectiveActive && pulse != null)
            {
                // Don't fight a modal's own highlight pulse while one shows.
                if (!_showing) pulse.Pulse();
                yield return wait;
            }
            _persistPulseCo = null;
        }

        // [TUT-R2 / D3=B] Trigger ids suppressed while the guided driver is
        // active (superseded S4 reactives, TUT-R1 §6 ledger). Runtime
        // retirement ahead of the TUT-R3 asset retirement.
        private readonly HashSet<string> _suppressed = new();

        /// <summary>[TUT-R2] Raised after a dialog completes and is marked
        /// fired. The guided driver chains dismissal-triggered beats off this.</summary>
        public event Action<string> DialogCompleted;

        /// <summary>[TUT-R2] Driver-facing enqueue: same fired/dedup/priority
        /// path as reactive triggers. Guided ids are never suppressed.</summary>
        public void EnqueueGuided(string triggerId) => TryEnqueue(triggerId);

        /// <summary>[TUT-R2] Mark a trigger consumed WITHOUT showing it (beat-8
        /// degrade path (b): no hold, no popup, never re-fires).</summary>
        public void MarkFiredWithoutShow(string triggerId)
        {
            if (string.IsNullOrEmpty(triggerId) || HasFired(triggerId)) return;
            MarkFired(triggerId);
            Log($"'{triggerId}' marked fired WITHOUT show (degrade path)");
        }

        /// <summary>[TUT-R2] Replace the suppressed-trigger set (guided driver).</summary>
        public void SetSuppressedTriggers(IEnumerable<string> ids)
        {
            _suppressed.Clear();
            if (ids == null) return;
            foreach (var id in ids)
                if (!string.IsNullOrEmpty(id)) _suppressed.Add(id);
        }

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
            bus.Subscribe<RewardChoiceOpenedEvent>(OnRewardOpened);
            bus.Subscribe<MusicianStressHitEvent>(OnMusicianStressHit);
            bus.Subscribe<AudienceBlockedEvent>(OnAudienceBlocked);
            Log($"OnEnable: subscribed to 11 events (frame {Time.frameCount}).");
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
                bus.Unsubscribe<RewardChoiceOpenedEvent>(OnRewardOpened);
                bus.Unsubscribe<MusicianStressHitEvent>(OnMusicianStressHit);
                bus.Unsubscribe<AudienceBlockedEvent>(OnAudienceBlocked);
            }
            // Defensive: never leave gameplay gated if disabled mid-modal.
            if (_pumpCo != null) 
            { 
                StopCoroutine(_pumpCo);
                StopHighlightPulse();
                StopPersistentPulse();
                _pumpCo = null; 
            }
            ReleaseGate();
        }

        // ---------------- event → trigger mapping ----------------
        private void OnGigStarted(GigStartedEvent e)
        {
            Log($"event GigStartedEvent (requiredSongs={e.RequiredSongCount}, frame {Time.frameCount})");
        }

        private void OnCardPlayed(CardPlayedEvent e)
        {
            Log($"event CardPlayedEvent (card='{(e.Definition != null ? e.Definition.name : "null")}', " +
                $"comp={e.IsComposition}, action={e.IsAction}, cost={e.InspirationCost})");

            if (e.IsComposition)
            {
                if (IsSoundCard(e.Definition))
                    TryEnqueue(TutorialTriggerId.FirstSoundCard);            // beat 5
            }
        }

        private void OnLoopResolved(LoopResolvedEvent e)
        {
            Log($"event LoopResolvedEvent (inspirationGainedThisLoop={e.Context.InspirationGainedThisLoop})");
        }

        private void OnSfxStage(SfxStageCrossedEvent e)
        {
            Log("event SfxStageCrossedEvent (stage-seen flag set)");
            _sfxStageSeen = true;
            // [TUT-R2 / D9] Auto-gate: the dialog talks about the hype bar; when
            // GigPresentationSO hides it, the dialog would point at nothing.
            var pres = GigManager.Instance != null ? GigManager.Instance.Presentation : null;
            if (pres == null || pres.ShowSongHypeBar)
                TryEnqueue(TutorialTriggerId.FirstSfxStage);
            else
                Log("  ↳ 'tut_first_sfx_stage' auto-gated OFF (ShowSongHypeBar=false, D9)");
        }

        private void OnSongEnd(SongEndVibeEvent e)
        {
            Log("event SongEndVibeEvent");
        }

        private void OnAudienceTurn(AudienceTurnStartedEvent e)
        {
            // Design: first audience turn AFTER first SongHype stage crossing.
            Log($"event AudienceTurnStartedEvent (stageSeen={_sfxStageSeen})");
        }

        private void OnStatusApplied(StatusAppliedEvent e)
        {
            Log($"event StatusAppliedEvent (status={e.Status}, delta={e.DeltaStacks})");

            if (e.DeltaStacks <= 0 || e.Source == null) return;
            var owner = e.Source.Owner; // [TUT-R2] set by CharacterBase

            if (owner is ALWTTT.Characters.Band.MusicianBase)
            {
                // Composure has its own dedicated dialog (TUT-R1 §4.4).
                if (e.Status == ALWTTT.Status.CharacterStatusId.TempShieldTurn)
                {
                    TryEnqueue(TutorialTriggerId.Composure);
                    return;
                }
                // Buff/debuff polarity from the SO's semantic flag.
                bool isBuff = e.Source.TryGet(e.Status, out var inst) &&
                              inst?.Definition != null && inst.Definition.IsBuff;
                if (isBuff)
                    TryEnqueue(TutorialTriggerId.StatusBuffMusician);
                // Musician debuffs: no dedicated dialog (breakdown beat covers
                // the pressure narrative).
            }
            else if (owner is ALWTTT.Characters.Audience.AudienceCharacterBase)
            {
                // Player-origin proxy (documented limitation): the bus event
                // carries no source actor; in the demo cut, container statuses
                // on audience members originate from player cards (Earworm et
                // al.). Blocked never reaches here — it is a bool, not a status
                // (see AudienceBlockedEvent).
                TryEnqueue(TutorialTriggerId.StatusDebuffAudience);
            }
        }

        private void OnGigOutcome(GigOutcomeEvent e)
        {
            Log($"event GigOutcomeEvent (won={e.Won})");
            TryEnqueue(e.Won ? TutorialTriggerId.GigWon : TutorialTriggerId.GigLost);
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

            // [DEMO-FIXES-A] Belt guard (see driver note). ReplayDialog (revisit,
            // MainMenu-hosted) does not pass through here and stays unaffected.
            var pdGuard = PD;
            if (pdGuard != null && !pdGuard.TutorialEnabled)
            { Log($"  ↳ '{triggerId}' skipped (tutorial disabled)"); return; }

            if (_suppressed.Contains(triggerId))
            { Log($"  ↳ '{triggerId}' skipped (superseded — suppressed by guided driver)"); return; }

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
            StartHighlightPulse(next); // [TUT-R2b]
        }

        private void OnDialogComplete(TutorialDialogSO dialog)
        {
            StopHighlightPulse(); // [TUT-R2b]

            // Skip and normal completion both record the dialog as fired (D-TUT-2).
            MarkFired(dialog.TriggerId);

            DialogCompleted?.Invoke(dialog.TriggerId);

            // [DEMO-FIXES-A / CT1] If a directive outlives the modal, keep the
            // dialog's highlight target pulsing until it clears. Pulse only:
            // the dim/spotlight overlay still closes with the modal
            // (deliberate — keeping the dim screen would obstruct play).
            TryStartPersistentPulse(dialog);


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

        private void OnRewardOpened(RewardChoiceOpenedEvent e)
        {
            Log("event RewardChoiceOpenedEvent");
            TryEnqueue(TutorialTriggerId.FirstRewardChoice);
        }

        private void OnMusicianStressHit(MusicianStressHitEvent e)
        {
            Log($"event MusicianStressHitEvent (absorbed={e.Absorbed}, applied={e.Applied})");
            if (e.Applied > 0) // an actual fortitude loss (TUT-R1 §4.3)
            {
                // [CARD-UX-1 / D3=B] Precision re-register: last-registered wins,
                // so point the key at the musician that was actually hit BEFORE
                // the dialog resolves its highlight.
                ReregisterMusicianHighlight(e.Stats);
                TryEnqueue(TutorialTriggerId.MusicianBreakdown);
            }
        }

        private void OnAudienceBlocked(AudienceBlockedEvent e)
        {
            Log("event AudienceBlockedEvent");
            // [CARD-UX-1 / D3=B] Point status_icon_blocked at the blocked member.
            if (e.Audience != null)
                ReregisterKeyOn(e.Audience.gameObject, "status_icon_blocked");
            TryEnqueue(TutorialTriggerId.StatusBlockedFront);
        }

        // ---- [CARD-UX-1 / D3=B] highlight precision helpers ----

        private static void ReregisterMusicianHighlight(
            ALWTTT.Characters.Band.BandCharacterStats stats)
        {
            var gig = GigManager.Instance;
            if (stats == null || gig == null || gig.CurrentMusicianCharacterList == null)
                return;

            for (int i = 0; i < gig.CurrentMusicianCharacterList.Count; i++)
            {
                var m = gig.CurrentMusicianCharacterList[i];
                if (m == null || !ReferenceEquals(m.Stats, stats)) continue;
                ReregisterKeyOn(m.gameObject, "musician_stress_bar");
                return;
            }
        }

        private static void ReregisterKeyOn(GameObject host, string key)
        {
            if (host == null) return;
            var targets = host.GetComponentsInChildren<TutorialHighlightTarget>(true);
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null || targets[i].Key != key) continue;
                TutorialHighlightRegistry.Register(targets[i]); // last-registered wins
                return;
            }
        }

        private Spotlight ResolveHighlight(TutorialDialogSO dialog)
        {
            if (dialog != null && dialog.HasHighlight &&
                TutorialHighlightRegistry.TryGet(dialog.HighlightKey, out var sceneTarget))
            {
                if (sceneTarget.IsWorldSpace) // [TUT-R3] world→screen
                {
                    bool hasB = sceneTarget.TryGetWorldBounds(out var wb);
                    return new Spotlight
                    {
                        Enabled = true,
                        HoleShape = defaultHoleShape,
                        WorldTarget = sceneTarget.WorldTarget,
                        WorldCamera = sceneTarget.WorldCamera,        // null → overlay usa Camera.main
                        WorldBounds = wb,
                        HasWorldBounds = hasB,
                        // sin renderer → radio manual sensato para que no sea un punto.
                        ManualRadiusFrac = hasB ? Vector2.zero : new Vector2(0.10f, 0.10f)
                    };
                }
                if (sceneTarget.MaskTarget != null)
                {
                    return new Spotlight
                    {
                        Enabled = true,
                        HoleShape = defaultHoleShape,
                        Target = sceneTarget.MaskTarget,
                        ManualCenterVp = default,
                        ManualRadiusFrac = Vector2.zero
                    };
                }
                // registry hit sin world ni MaskTarget → cae al fallback de bindings.
            }

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

        private void StartHighlightPulse(TutorialDialogSO dialog)
        {
            StopHighlightPulse();
            var pulse = ResolvePulse(dialog);
            if (pulse != null)
                _pulseCo = StartCoroutine(PulseWhileShowing(pulse, dialog.TriggerId));
        }

        // [DEMO-FIXES-A / CT1] Factored out of StartHighlightPulse so the
        // persistent-pulse path shares the exact same resolution order:
        // scene registry first, serialized bindings fallback.
        private UIPulseAnimator ResolvePulse(TutorialDialogSO dialog)
        {
            if (dialog == null || !dialog.HasHighlight) return null;

            if (TutorialHighlightRegistry.TryGet(dialog.HighlightKey, out var t) &&
                t.Pulse != null)
                return t.Pulse;

            for (int i = 0; i < highlightBindings.Count; i++)
            {
                var b = highlightBindings[i];
                if (b != null && b.key == dialog.HighlightKey && b.pulse != null)
                    return b.pulse;
            }
            return null;
        }

        private void StopHighlightPulse()
        {
            if (_pulseCo != null) { StopCoroutine(_pulseCo); _pulseCo = null; }
        }

        private System.Collections.IEnumerator PulseWhileShowing(
            UIPulseAnimator pulse, string dialogId)
        {
            var wait = new WaitForSeconds(0.9f);
            while (_showing && _showingId == dialogId && pulse != null)
            {
                pulse.Pulse();
                yield return wait;
            }
            _pulseCo = null;
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
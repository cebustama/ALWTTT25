// Place at: Assets/Scripts/Tutorial/TutorialGuidedDriver.cs
// [TUT-R2b] v2 — REPLACES the TUT-R2 file. Fixes:
//   FIX-1 (ST6 root cause): beat-8 affordability was evaluated on the
//     LoopResolvedEvent, but GigManager.OnCompositionLoopFinished publishes
//     that event BEFORE the per-loop flat inspiration grant (code order:
//     Publish → ResetAllTurnPlayBudgets → AddCurrentInspiration). So at the
//     final-loop boundary the check read 1 less inspiration than the player
//     actually has during the final loop → degrade (b) fired silently on the
//     NORMAL path ("available=True affordable=False"). Beat 8 now evaluates
//     one frame deferred, after the grant has landed.
//   FIX-2 (latent swallow): the beat-7 branch keyed on HasFired(), which only
//     flips at dialog COMPLETION. If the beat-7 modal was still up when the
//     next loop resolved (loops don't freeze under modals), the branch
//     swallowed that event with an early return and beat 8's window
//     (loopsRemaining==1) could be missed entirely — fatal with loopsPerPart=3,
//     where beat 8's window is the very next boundary. Now: driver-local
//     request flags + non-exclusive evaluation (both beats can react to the
//     same event).
//   FIX-3: clearer beat-7 logging (the DeckManager-side [TUT-R2
//     ScriptedFinisher] tag only appears when the finisher was NOT already in
//     the opening hand; the driver now says which case happened).
using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Managers;
using ALWTTT.Sensory;
using MidiGenPlay;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [TUT-R2 / D3=B] Gig-1 guided-curriculum driver, layered OVER the S4
    /// reactive catalog/HashSet system (D-TUT-3 is NOT retired). See TUT-R2
    /// batch notes; v2 header above lists the TUT-R2b fixes.
    /// </summary>
    public class TutorialGuidedDriver : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private TutorialController controller;

        [Header("Content refs")]
        [Tooltip("Psychic Waves (Starter_psychic_waves). Exact id + inspiration " +
                 "cost for beat 7's scripted draw and beat 8's affordability check.")]
        [SerializeField] private CardDefinition finisherCard;

        [Tooltip("Forced initial hand for beat 2, in order: Default Mode, " +
                 "Wormus Major, Finger Bass, Rise Up (one composition per musician). Role fallbacks " +
                 "when an exact card is missing; full miss → M4.5 guarantee (D1).")]
        [SerializeField] private List<CardDefinition> forcedInitialHand = new();

        [Header("Debug")]
        [SerializeField] private bool verboseLogging = true;

        private const string DebugTag = "<color=#ffd47f>[TutGuided]</color>";
        private void Log(string msg) { if (verboseLogging) Debug.Log($"{DebugTag} {msg}"); }

        private static readonly string[] SupersededIds = System.Array.Empty<string>();


        // [TUT-REDESIGN-B / D-TUTB-5=A] Card-anchored reactives stay silent while
        // the guided gig runs: the four identities are HEARD in gig 1 and NAMED
        // from gig 2 on. Every event here recurs (Voltage per Conito play, Wink,
        // Harmony rows, Earworm ticks…), so suppression defers, never loses.
        // Installed by PrepareForGig when the forced hand is queued; cleared
        // (back to SupersededIds) on every later gig.
        private static readonly string[] Gig1SuppressedReactives =
        {
            TutorialTriggerId.EarwormTick, TutorialTriggerId.Captivated,
            TutorialTriggerId.VoltageFirst, TutorialTriggerId.OverloadReady,
            TutorialTriggerId.BonusLoop, TutorialTriggerId.Spotlight,
            TutorialTriggerId.ReadTheRoom, TutorialTriggerId.HarmonyTrack,
            TutorialTriggerId.HarmonyDenied, TutorialTriggerId.SungMelody,
        };

        // [S10 / D1b=A] The menu's «Tutorial» run shows ONLY these: 8 guided beats plus
        // the end of the gig. TracksByMusician and LoopsStructure are skipped (their
        // content is folded into PressPlay); every reactive dialog stays silent.
        private static readonly string[] MinimalTutorialAllow =
        {
            TutorialTriggerId.JamWelcome, TutorialTriggerId.YourTurn,
            TutorialTriggerId.PlayComposition, TutorialTriggerId.PressPlay,
            TutorialTriggerId.InspirationEconomy, TutorialTriggerId.PlayFinisher,
            TutorialTriggerId.PlayFinisherEarly, TutorialTriggerId.SongEndVibe,
            TutorialTriggerId.AudienceTurn, TutorialTriggerId.GigWon, TutorialTriggerId.GigLost,
            TutorialTriggerId.AbilitiesIntro, TutorialTriggerId.StatusIntro,   // [S10 / D11=A]
            TutorialTriggerId.OnYourOwn,
            TutorialTriggerId.LoopMore, TutorialTriggerId.PauseHint,           // [S10] 25-nov
            TutorialTriggerId.TracksIntro, TutorialTriggerId.LayersIntro,
        };

        private static bool IsTutorialRun
        {
            get
            {
                var pd = GameManager.Instance != null
                    ? GameManager.Instance.PersistentGameplayData : null;
                return pd != null && pd.TutorialRun;
            }
        }

        private bool _finisherPlayedThisSong;
        // [TUT-R2b FIX-2] Request flags: set when the driver ACTS on a beat,
        // independent of when the dialog completes (HasFired flips only on
        // completion; loops keep running under modals).
        private bool _beat7Requested;
        private bool _beat8Requested;

        // [DEMO-FIXES-A] Belt guard: the primary off-switch is the tutorial
        // root's active state (OnDisable teardown), but per the single-flag
        // rule the driver also consults PD.TutorialEnabled directly, so a
        // mismatched scene state (root active, flag off) stays inert.
        private static bool TutorialEnabledFlag
        {
            get
            {
                var pd = GameManager.Instance != null
                    ? GameManager.Instance.PersistentGameplayData : null;
                return pd == null || pd.TutorialEnabled;
            }
        }

        // ---------------- lifecycle ----------------

        private void Awake()
        {
            if (forcedInitialHand.Count > 0 && forcedInitialHand[0] != null)
                TutorialHighlightSpawnHook.RegisterCardKey(
                    forcedInitialHand[0].Id, "card_default_mode");
            if (finisherCard != null)
                TutorialHighlightSpawnHook.RegisterCardKey(
                    finisherCard.Id, "card_psychic_waves");

            // [DEMO-FIXES-A] Forced-hand fill MOVED to PrepareForGig (called by
            // GigManager AFTER the opt-in answer). Awake ran at an unknown point
            // relative to the answer (GigCanvas activation timing is core-owned),
            // so filling here could read a stale flag / force the hand on "No".
        }

        /// <summary>[DEMO-FIXES-A / DEMO-TUT-TOGGLE] Called by GigManager right
        /// after the opt-in answer, before StartGig's initial draw. Timing-immune
        /// single fill point: clears any stale directive, and only queues the
        /// forced hand when the tutorial is enabled AND its opening beat hasn't
        /// already fired in a prior gig this launch.</summary>
        public void PrepareForGig(bool tutorialEnabled)
        {
            TutorialInputGate.Clear();
            TutorialLoopHoldGate.Release();
            TutorialScriptedDrawQueue.Clear();

            // [S10 / D11=A] Minimal tutorial: scripted hands, drum-solo finisher.
            TutorialScriptedDrawQueue.ScriptOnly = tutorialEnabled && IsTutorialRun;
            TutorialScriptedDrawQueue.SuppressRefills = TutorialScriptedDrawQueue.ScriptOnly;
            TutorialScriptedDrawQueue.FillCompositionsTo = 0;
            _tutAudienceTurns = 0;
            _pendingTurnIntro = null;
            _pendingTurnIntro2 = null;
            _loopDraws.Clear();
            _lessonLoopSong = -1;
            _lateLessons = false;
            _lastSongFinishersDrawn = false;
            _pausedForDialog = null;
            TutorialScriptedDrawQueue.RefillHandCap = 0;
            TutorialScriptedDrawQueue.OneCompositionPerLoop = false;
            TutorialScriptedDrawQueue.CompositionUsedThisLoop = false;
            TutorialCardPulse.ResetAll();
            if (TutorialScriptedDrawQueue.ScriptOnly)
            {
                var drums = Resources.Load<CardDefinition>(TutorialFinisherPath);
                if (drums != null)
                {
                    finisherCard = drums;
                    TutorialHighlightSpawnHook.RegisterCardKey(drums.Id, "card_psychic_waves");
                }
                TutorialTrace.Note("guion de cartas activo | remate=" +
                                   (finisherCard != null ? finisherCard.Id : "NINGUNO"));
            }

            if (tutorialEnabled && !HasFired(TutorialTriggerId.YourTurn))
            {
                FillForcedInitialHand();
                Log("PrepareForGig: tutorial ENABLED — forced hand queued.");
            }
            else
            {
                Log($"PrepareForGig: {(tutorialEnabled ? "already-fired" : "DISABLED")} " +
                                    "— no forced hand.");
            }

            // [TUT-REDESIGN-B / D-TUTB-6=B] Una sola llamada, después de decidir la
            // mano forzada: el estado del arco manda, no la rama.
            ApplySuppressionForArcState();
        }

        private void OnEnable()
        {
            // [TUT-REDESIGN-B / D-TUTB-6=B] Antes: SetSuppressedTriggers(SupersededIds)
            // incondicional, que vaciaba lo instalado por PrepareForGig.
            ApplySuppressionForArcState();

            var bus = SensoryEventBus.Instance;
            if (bus == null)
            {
                Log("OnEnable: SensoryEventBus.Instance is NULL — driver NOT subscribed.");
                return;
            }
            bus.Subscribe<GigStartedEvent>(OnGigStarted);
            bus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            bus.Subscribe<LoopResolvedEvent>(OnLoopResolved);
            bus.Subscribe<SongEndVibeEvent>(OnSongEnd);
            bus.Subscribe<AudienceTurnStartedEvent>(OnAudienceTurn);

            TutorialInputGate.PlayPressed += OnPlayPressed;
            if (controller != null) controller.DialogCompleted += OnDialogCompleted;
            if (controller != null) controller.DialogShown += OnDialogShown;            // [S10]
            ALWTTT.UI.PauseMenuController.InspectPauseChanged += OnInspectPause;      // [S10]
        }

        private void OnDisable()
        {
            var bus = SensoryEventBus.Instance;
            if (bus != null)
            {
                bus.Unsubscribe<GigStartedEvent>(OnGigStarted);
                bus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
                bus.Unsubscribe<LoopResolvedEvent>(OnLoopResolved);
                bus.Unsubscribe<SongEndVibeEvent>(OnSongEnd);
                bus.Unsubscribe<AudienceTurnStartedEvent>(OnAudienceTurn);
            }
            TutorialInputGate.PlayPressed -= OnPlayPressed;
            if (controller != null) controller.DialogCompleted -= OnDialogCompleted;
            if (controller != null) controller.DialogShown -= OnDialogShown;            // [S10]
            ALWTTT.UI.PauseMenuController.InspectPauseChanged -= OnInspectPause;      // [S10]
            TutorialCardPulse.ResetAll();

            TutorialInputGate.Clear();
            TutorialLoopHoldGate.Release();
            TutorialScriptedDrawQueue.Clear();
            TutorialScriptedDrawQueue.ScriptOnly = false;   // [S10] never leaks into a normal gig
        }

        // ---------------- beat 2 forced hand ----------------

        private void FillForcedInitialHand()
        {
            TutorialScriptedDrawQueue.Clear();

            var fallbacks = new System.Func<CardDefinition, bool>[]
            {
                c => IsCompositionWithRole(c, TrackRole.Rhythm),
                c => IsCompositionWithRole(c, TrackRole.Backing),
                c => IsCompositionWithRole(c, TrackRole.Bassline),   // [TUT-REDESIGN-B] Conito
                c => IsCompositionWithRole(c, TrackRole.Melody),     // [TUT-REDESIGN-B] Zig (was: any Action)
            };

            for (int i = 0; i < 4; i++)
            {
                var def = i < forcedInitialHand.Count ? forcedInitialHand[i] : null;
                var primary = def != null
                    ? TutorialScriptedDrawQueue.ById(def.Id)
                    : fallbacks[i];
                var entry = new TutorialScriptedDrawQueue.Entry(
                    label: def != null ? def.name : $"forced-slot-{i}",
                    primary: primary,
                    fallback: fallbacks[i]);
                // [S10] Minimal tutorial: start with 2 (drums, bass); organ and voice arrive
                // one per loop, so the first loop offers few choices.
                if (TutorialScriptedDrawQueue.ScriptOnly && (i == 1 || i == 3))
                    _loopDraws.Enqueue(entry);
                else
                    TutorialScriptedDrawQueue.Enqueue(entry);
            }
            Log($"Forced initial hand queued ({TutorialScriptedDrawQueue.Count} entries).");
        }

        private static bool IsCompositionWithRole(CardDefinition c, TrackRole role)
        {
            var comp = c != null ? c.CompositionPayload : null;
            return comp != null &&
                   comp.TrackAction != null &&
                   comp.TrackAction.role == role;
        }

        // ---------------- event handlers ----------------

        private void OnGigStarted(GigStartedEvent e)
        {
            TutorialTrace.ResetCount(); // [S10]
            TutorialTrace.Note($"empieza el gig | tutorial={(TutorialEnabledFlag ? "sí" : "no")} " +
                               $"mínimo={(IsTutorialRun ? "sí" : "no")} | {TutorialTrace.StateSnapshot()}");
            TryBeat(TutorialTriggerId.JamWelcome);
            TryBeat(TutorialTriggerId.YourTurn);
        }

        private void OnDialogCompleted(string id)
        {
            if (!TutorialEnabledFlag) return;

            // [S10] A dialog that paused the game gives the time back when it closes.
            if (_pausedForDialog != null && _pausedForDialog == id)
            {
                _pausedForDialog = null;
                ALWTTT.UI.PauseMenuController.Instance?.ResumeInspect(id);
            }

            switch (id)
            {
                case TutorialTriggerId.YourTurn:
                    TryBeat(TutorialTriggerId.PlayComposition);
                    break;

                case TutorialTriggerId.PlayComposition:
                    {
                        // [TUT-R2c] Allow-list = las composiciones de la MANO
                        // FORZADA (Default Mode / Wormus Major / Singing Field):
                        // cartas que ESTABLECEN una pista. Modificadoras (Key Lift,
                        // Push It, Half Time) quedan fuera de la primera lección
                        // aunque caigan en el 5º robo. Degrade en dos niveles (D2):
                        //   1) ninguna básica en mano → permitir CUALQUIER
                        //      composición (enseñar > bloquear);
                        //   2) ninguna composición → liberar gate (TUT-R2).
                        var deck = DeckManager.Instance;
                        var allowedIds = new List<string>();
                        foreach (var def in forcedInitialHand)
                            if (def != null && def.IsComposition) allowedIds.Add(def.Id);

                        bool anyBasicInHand = deck != null && allowedIds.Count > 0 &&
                            deck.HandHas(c => c.IsComposition &&
                                allowedIds.Contains(c.Id));
                        bool anyCompositionInHand = deck != null &&
                            deck.HandHas(c => c.IsComposition);

                        if (anyBasicInHand)
                        {
                            TutorialInputGate.Set(TutorialInputGate.GateMode.CompositionOnly);
                            TutorialInputGate.SetCompositionAllowList(allowedIds);
                            Log("beat 3 gate ARMED (CompositionOnly, allow-list básicas)");
                        }
                        else if (anyCompositionInHand)
                        {
                            TutorialInputGate.Set(TutorialInputGate.GateMode.CompositionOnly);
                            TutorialInputGate.SetCompositionAllowList(null);
                            Log("beat 3 gate ARMED degradado nivel 1 (cualquier composición)");
                        }
                        else
                        {
                            Log("beat 3 gate DEGRADED nivel 2 (sin composiciones) — released");
                        }
                        break;
                    }

                case TutorialTriggerId.TracksByMusician:
                    TryBeat(TutorialTriggerId.PressPlay);
                    break;

                case TutorialTriggerId.PressPlay:
                    var gig = GigManager.Instance;
                    if (gig != null && !gig.IsSongPlayingNow)
                    {
                        TutorialInputGate.Set(TutorialInputGate.GateMode.PlayOnly);
                        Log("beat 5 gate ARMED (PlayOnly)");
                    }
                    else
                    {
                        Log("beat 5 gate DEGRADED (song already playing) — released");
                    }
                    break;
            }
        }

        private void OnCardPlayed(CardPlayedEvent e)
        {
            if (e.Definition != null) TutorialCardPulse.Release(e.Definition.Id);   // [S10]
            if (e.IsComposition && (e.Definition == null || !e.Definition.IsFinisher) &&
                TutorialScriptedDrawQueue.OneCompositionPerLoop)
                TutorialScriptedDrawQueue.CompositionUsedThisLoop = true;           // [S10] song 2

            if (e.IsComposition &&
                TutorialInputGate.Mode == TutorialInputGate.GateMode.CompositionOnly)
            {
                TutorialInputGate.Clear();
                Log("beat 3 gate SATISFIED (composition played)");
                // [S10] Minimal run: straight to Play (TracksByMusician is not shown).
                TryBeat(IsTutorialRun ? TutorialTriggerId.PressPlay
                                      : TutorialTriggerId.TracksByMusician);
            }

            if (IsFinisher(e.Definition))
            {
                _finisherPlayedThisSong = true;
                if (TutorialLoopHoldGate.IsArmed)
                {
                    TutorialLoopHoldGate.Release();
                    // [CARD-UX-1] Release the finisher-only directive with the hold.
                    if (TutorialInputGate.Mode == TutorialInputGate.GateMode.SingleCardOnly)
                    {
                        TutorialInputGate.Clear();
                        Log("beat 8 finisher-only gate RELEASED (finisher played)");
                    }
                }
            }
        }

        private void OnPlayPressed()
        {
            OnTutorialPlay();   // [S10] every real press, before the beat-5 gate check
            if (TutorialInputGate.Mode != TutorialInputGate.GateMode.PlayOnly) return;
            TutorialInputGate.Clear();
            Log("beat 5 gate SATISFIED (Play pressed)");
            TryBeat(TutorialTriggerId.LoopsStructure);
        }

        private void OnLoopResolved(LoopResolvedEvent e)
        {
            if (!TutorialEnabledFlag) return;

            // [S10] Minimal tutorial, first song: the next forced composition, one per loop.
            if (TutorialScriptedDrawQueue.ScriptOnly && _loopDraws.Count > 0 && DeckManager.Instance != null)
            {
                var entry = _loopDraws.Dequeue();
                bool ok = DeckManager.Instance.DrawCardFiltered(entry.Primary, "TUT loop: " + entry.Label);
                if (!ok && entry.Fallback != null)
                    DeckManager.Instance.DrawCardFiltered(entry.Fallback, "TUT loop (alternativa): " + entry.Label);
            }

            // [S10] A new loop: song 2's one-composition allowance comes back.
            TutorialScriptedDrawQueue.CompositionUsedThisLoop = false;
            // [S10] Song 3, end of the first loop: the track list and the combos.
            if (IsTutorialRun && _lessonLoopSong != CurrentSongIndex())
            {
                _lessonLoopSong = CurrentSongIndex();
                if (_lessonLoopSong == 2) TryBeat(TutorialTriggerId.TracksIntro);
            }

            var ctx = e.Context;

            // [TUT-R2b FIX-2] Non-exclusive branches, request-flag guarded.
            // beat 7: first resolved loop of the gig.
            if (!_beat7Requested && !HasFired(TutorialTriggerId.InspirationEconomy))
            {
                _beat7Requested = true;
                bool alreadyInHand = finisherCard != null && DeckManager.Instance != null &&
                                     DeckManager.Instance.HandHas(IsFinisher);
                bool drawn = false;
                if (!alreadyInHand && finisherCard != null && DeckManager.Instance != null)
                {
                    drawn = DeckManager.Instance.DrawCardFiltered(
                        IsFinisher, "TUT-R2 ScriptedFinisher");
                }
                // [FIX-3] Explicit three-way log: the DeckManager-side tag only
                // prints on an actual draw call.
                Log($"beat 7 fire — scripted finisher: " +
                    (alreadyInHand ? "already in opening hand (draw skipped)"
                     : drawn ? "drawn OK" : "MISS (not in piles?)"));
                TryBeat(TutorialTriggerId.InspirationEconomy);
            }

            // beat 8: start of the LAST loop. [TUT-R2b FIX-1] evaluation is
            // deferred one frame: GigManager publishes this event BEFORE the
            // per-loop flat grant, so the affordability check must run after
            // the grant lands.
            int loopsRemaining = ctx.LoopsInPart - (ctx.LoopIndexWithinPart + 1);

            // [S10] Last song, last loop: the tutorial ends with a finisher, guaranteed.
            if (IsTutorialRun && loopsRemaining == 1 && !_lastSongFinishersDrawn &&
                CurrentSongIndex() == 2 && DeckManager.Instance != null)
            {
                _lastSongFinishersDrawn = true;
                int drawn = 0;
                for (int i = 0; i < 2; i++)
                    if (DeckManager.Instance.DrawCardFiltered(IsFinisher, "TUT última canción: remate")) drawn++;
                Debug.Log($"[S10][TUT] última canción, último loop: {drawn} remate(s) robados | " +
                          TutorialTrace.StateSnapshot());
            }
            if (loopsRemaining == 1 && !_beat8Requested &&
                !HasFired(TutorialTriggerId.PlayFinisher))
            {
                _beat8Requested = true;
                StartCoroutine(FireFinisherBeatDeferred());
            }
        }

        private System.Collections.IEnumerator FireFinisherBeatDeferred()
        {
            yield return null; // let OnCompositionLoopFinished finish its grant
            FireFinisherBeat();
        }

        private void FireFinisherBeat()
        {
            if (_finisherPlayedThisSong)
            {
                // [TUT-R2c / RT5] Guiño del manager: el jugador se adelantó.
                // La variante sustituye al popup estándar; el beat 8 canónico
                // se marca consumido para que no re-dispare en canciones
                // posteriores (la lección ya ocurrió, solo que antes de tiempo).
                Log("beat 8 fire — degrade (a): finisher ya jugado; variante 'early'");
                TutorialTrace.Note("el remate ya se jugó antes del último loop: sale la variante 'early'"); // [S10]
                TryBeat(TutorialTriggerId.PlayFinisherEarly);
                controller?.MarkFiredWithoutShow(TutorialTriggerId.PlayFinisher);
                return;
            }

            var deck = DeckManager.Instance;
            var pd = GameManager.Instance != null
                ? GameManager.Instance.PersistentGameplayData : null;

            // [DEMO-FIXES-A / R1 / D-DF-4=A] Hand-only availability. Held loops
            // grant no draw (no LoopResolvedEvent), so a finisher sitting in a
            // pile is hold-unreachable (§9.2). PilesHave dropped: if the beat-7
            // scripted draw failed, this now degrades via path (b) instead of
            // arming an unwinnable hold. The later HandHas guard on the
            // SingleCardOnly gate is redundant post-R1 but kept as belt.
            bool available = finisherCard != null && deck != null &&
                deck.HandHas(IsFinisher);

            bool affordable = pd != null && finisherCard != null &&
                pd.CurrentInspiration >= finisherCard.InspirationCost;

            // [S10] Minimal tutorial: the finisher lesson always happens. If ★ fall short,
            // the manager lends what is missing (first smoke: ★=1 of 3, the beat was lost).
            if (IsTutorialRun && available && !affordable && pd != null && GigManager.Instance != null)
            {
                int before = pd.CurrentInspiration;
                GigManager.Instance.DevSetInspiration(finisherCard.InspirationCost);
                affordable = pd.CurrentInspiration >= finisherCard.InspirationCost;
                TutorialTrace.Note($"el tutorial presta ★ para el remate: {before} → {pd.CurrentInspiration}");
            }

            if (!available || !affordable)
            {
                TutorialTrace.Note($"el remate NO sale: en mano={(available ? "sí" : "no")}, " +
                    $"★={(pd != null ? pd.CurrentInspiration : -1)} de {(finisherCard != null ? finisherCard.InspirationCost : -1)} " +
                    $"| {TutorialTrace.StateSnapshot()}"); // [S10]
                Log($"beat 8 DEGRADE (b): available={available} affordable={affordable} " +
                    $"(insp={(pd != null ? pd.CurrentInspiration : -1)} vs " +
                    $"cost={(finisherCard != null ? finisherCard.InspirationCost : -1)}) " +
                    "— no hold, marked fired silently");
                controller?.MarkFiredWithoutShow(TutorialTriggerId.PlayFinisher);
                return;
            }

            TutorialLoopHoldGate.Arm();
            Log("beat 8 holdLoop ARMED");

            // [CARD-UX-1] Finisher-only directive: while the loop is held, the
            // finisher is the ONLY playable card. Everything else gets the red
            // unplayable overlay (UnplayableReason.TutorialGate) and is
            // undraggable — this is also what gates compositions in the final
            // loop of the tutorial run (the FinalLoopLock is exempt under a hold).
            // GUARD: `available` above also accepts "in a pile"; arming the gate
            // with the finisher NOT in hand would leave zero playable cards.
            if (deck.HandHas(IsFinisher))
            {
                TutorialInputGate.SetSingleCard(finisherCard.Id);
                Log("beat 8 finisher-only gate ARMED");
            }
            else
            {
                Log("beat 8 finisher-only gate NOT armed (finisher not in hand)");
            }

            TryBeat(TutorialTriggerId.PlayFinisher);
        }

        private void OnSongEnd(SongEndVibeEvent e)
        {
            TutorialScriptedDrawQueue.SuppressRefills = false;   // [S10] refills back after song 1
            _loopDraws.Clear();
            // [S10] NOT a song counter: this event fires once per audience member.
            TutorialScriptedDrawQueue.CompositionUsedThisLoop = false;
            TutorialCardPulse.ClearWanted();
            TryBeat(TutorialTriggerId.SongEndVibe);

            // [TUT-REDESIGN-B / D-TUTB-6=B] Red de seguridad: si el arco se completó
            // por otra vía (skip, revisit, beat ya disparado en un lanzamiento
            // anterior), la frontera de canción también libera. Idempotente.
            ApplySuppressionForArcState();

            // Song boundary: reset per-song tracking so beat 8 can degrade/fire
            // correctly in later songs of gig 1 if it hasn't completed yet.
            _finisherPlayedThisSong = false;
            _beat8Requested = HasFired(TutorialTriggerId.PlayFinisher);
            if (TutorialLoopHoldGate.IsArmed)
            {
                TutorialLoopHoldGate.Release();
                Log("holdLoop released defensively at song end");

                // [CARD-UX-1] Never carry the finisher-only gate across a song boundary.
                if (TutorialInputGate.Mode == TutorialInputGate.GateMode.SingleCardOnly)
                    TutorialInputGate.Clear();
            }
        }

        // ---------------- [S10] 25-nov: per-song lessons, card pulse, Space pause ----------------
        private int _lessonLoopSong = -1;
        private bool _lateLessons;              // [S10] song 3: Spotlight + TrackReplaced allowed
        private bool _lastSongFinishersDrawn;   // [S10] song 3, last loop: finishers guaranteed
        private string _pausedForDialog;

        private static int CurrentSongIndex()
        {
            var pd = GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;
            return pd != null ? pd.CurrentSongIndex : -1;
        }

        /// <summary>[S10] Play pressed in the minimal tutorial. Song 1: the next composition
        /// arrives right away and «one more each loop» is said WHILE loop 1 plays. Song 2: one
        /// composition per loop, Capas and the Space pause, with the game paused.</summary>
        private void OnTutorialPlay()
        {
            if (!TutorialEnabledFlag || !IsTutorialRun) return;
            TutorialScriptedDrawQueue.CompositionUsedThisLoop = false;
            int song = CurrentSongIndex();
            if (song == 0 && !HasFired(TutorialTriggerId.LoopMore))
            {
                if (TutorialScriptedDrawQueue.ScriptOnly && _loopDraws.Count > 0 && DeckManager.Instance != null)
                {
                    var entry = _loopDraws.Dequeue();
                    bool ok = DeckManager.Instance.DrawCardFiltered(entry.Primary, "TUT Tocar: " + entry.Label);
                    if (!ok && entry.Fallback != null)
                        DeckManager.Instance.DrawCardFiltered(entry.Fallback, "TUT Tocar (alternativa): " + entry.Label);
                }
                TryBeat(TutorialTriggerId.LoopMore);
            }
            else if (song == 1) TryBeat(TutorialTriggerId.LayersIntro);
        }

        private void OnDialogShown(string id)
        {
            bool lesson = id == TutorialTriggerId.TracksIntro || id == TutorialTriggerId.LayersIntro;
            bool late = id == TutorialTriggerId.Spotlight || id == TutorialTriggerId.TrackReplaced;
            var gm = ALWTTT.Managers.GigManager.Instance;
            if (!lesson && !(late && IsTutorialRun && gm != null && gm.IsSongPlayingNow)) return;
            // A lot to read: the game waits, and the cyan «new musician» hint starts.
            if (lesson) TutorialCardPulse.MissingTrackMode = true;
            var pause = ALWTTT.UI.PauseMenuController.Instance;
            if (pause != null && !ALWTTT.UI.PauseGate.IsPaused)
            {
                _pausedForDialog = id;
                pause.PauseInspect(id);
            }
            TutorialTrace.Note(id == TutorialTriggerId.LayersIntro
                ? "una por loop: pausa automática y contorno celeste en las composiciones de un músico nuevo"
                : id == TutorialTriggerId.TracksIntro ? "lista de pistas y combos: pausa automática"
                : $"'{id}' con la canción sonando: pausa automática");
        }

        private void OnInspectPause(bool on, string who)
        {
            if (who == "jugador")
                TutorialTrace.Note($"el jugador {(on ? "PAUSÓ" : "QUITÓ la pausa")} con Espacio | {TutorialTrace.StateSnapshot()}");
        }

        // ---------------- [S10 / D11=A] scripted hands, turns 2 and 3 ----------------
        private const string TutorialFinisherPath = "Data/Characters/Musicians/c2_redoble_final";
        private int _tutAudienceTurns;
        private string _pendingTurnIntro;
        private string _pendingTurnIntro2;
        private readonly Queue<TutorialScriptedDrawQueue.Entry> _loopDraws =
            new Queue<TutorialScriptedDrawQueue.Entry>();

        private static void EnqueueScripted(string label, string id)
        {
            TutorialScriptedDrawQueue.Enqueue(new TutorialScriptedDrawQueue.Entry(
                label, TutorialScriptedDrawQueue.ById(id), null));
        }

        private static void EnqueueAnyComposition(string label)
        {
            TutorialScriptedDrawQueue.Enqueue(new TutorialScriptedDrawQueue.Entry(
                label, c => c != null && c.IsComposition && !c.IsFinisher, null));
        }

        private void QueueNextTurnHand()
        {
            _tutAudienceTurns++;
            // A turn budget smaller than the script drops the tail; it never spills into the
            // next turn (the key cards go first in each list).
            TutorialScriptedDrawQueue.Clear();
            if (_tutAudienceTurns == 1)
            {
                EnqueueScripted("turno 2: Calm Down", "crd_sibi_calm_down");
                EnqueueScripted("turno 2: Static Rush", "starter_static_rush");
                _pendingTurnIntro = TutorialTriggerId.AbilitiesIntro;
                _pendingTurnIntro2 = TutorialTriggerId.OnYourOwn;
                // Song 2: one composition per loop (Capas), few cards in hand.
                TutorialScriptedDrawQueue.OneCompositionPerLoop = true;
                TutorialScriptedDrawQueue.RefillHandCap = 4;
            }
            else if (_tutAudienceTurns == 2)
            {
                EnqueueScripted("turno 3: Spotlight", "crd_c2_spotlight");
                EnqueueScripted("turno 3: Guino", "wink");
                _pendingTurnIntro = TutorialTriggerId.StatusIntro;
                // Song 3: free play (combos need several entries in the same loop).
                TutorialScriptedDrawQueue.OneCompositionPerLoop = false;
                TutorialScriptedDrawQueue.RefillHandCap = 5;
                _lateLessons = true;
                ApplySuppressionForArcState();
            }
            else return;
            TutorialScriptedDrawQueue.FillCompositionsTo = 3;   // + compositions up to 3 in hand
            TutorialTrace.Note($"mano del turno {_tutAudienceTurns + 1} en cola ({TutorialScriptedDrawQueue.Count} cartas)");
        }

        // The intro waits for the player's turn (there is no PlayerTurn event: polled, cheap).
        private void Update()
        {
            if (_pendingTurnIntro == null) return;
            var gig = GigManager.Instance;
            if (gig == null || gig.CurrentGigPhase != ALWTTT.Enums.GigPhase.PlayerTurn) return;
            var id = _pendingTurnIntro;
            var id2 = _pendingTurnIntro2;
            _pendingTurnIntro = null;
            _pendingTurnIntro2 = null;
            TryBeat(id);
            if (id2 != null) TryBeat(id2);
        }

        private void OnAudienceTurn(AudienceTurnStartedEvent e)
        {
            if (TutorialScriptedDrawQueue.ScriptOnly) QueueNextTurnHand();   // [S10 / D11=A]
            TryBeat(TutorialTriggerId.AudienceTurn);
            // [TUT-REDESIGN-B / D-TUTB-6=B] AudienceTurn es el último beat guiado:
            // en cuanto dispara, el arco terminó y las identidades se sueltan.
            ApplySuppressionForArcState();
        }

        /// <summary>
        /// [TUT-REDESIGN-B / D-TUTB-6=B] ÚNICO punto de instalación del set suprimido.
        /// Mientras el arco guiado está pendiente, los reactivos anclados al personaje
        /// callan (si no, Voltage salta durante el beat 3, cuando el jugador aún no
        /// sabe qué es un músico). En cuanto el arco cierra, el set se vacía y esos
        /// beats disparan en la canción siguiente.
        ///
        /// Se llama desde OnEnable Y desde PrepareForGig a propósito: el orden entre
        /// ambos no está garantizado (GigCanvas se habilita al abrir la gig), y con un
        /// solo punto idempotente da igual cuál gane. Antes esto vivía en dos sitios
        /// con valores distintos y el segundo borraba al primero.
        /// </summary>
        private void ApplySuppressionForArcState()
        {
            if (controller == null) return;

            // [S10 / D1b=A] Minimal run: the allow-list decides for the whole gig.
            if (IsTutorialRun)
            {
                controller.SetSuppressedTriggers(SupersededIds);
                // [S10] Song 3 adds two reactive lessons: reading Spotlight, replacing a track.
                var allow = new List<string>(MinimalTutorialAllow);
                if (_lateLessons) { allow.Add(TutorialTriggerId.Spotlight); allow.Add(TutorialTriggerId.TrackReplaced); }
                controller.SetAllowOnly(allow);
                Log($"[S10] tutorial mínimo: solo {allow.Count} diálogos permitidos");
                return;
            }
            controller.SetAllowOnly(null);

            bool arcPending = TutorialEnabledFlag &&
                              !HasFired(TutorialTriggerId.AudienceTurn);

            controller.SetSuppressedTriggers(
                arcPending ? Gig1SuppressedReactives : SupersededIds);

            Log(arcPending
                ? $"suppression ON ({Gig1SuppressedReactives.Length} card-anchored ids) — guided arc pending"
                : "suppression OFF — guided arc complete");
        }

        // ---------------- helpers ----------------

        private bool IsFinisher(CardDefinition def) =>
            def != null && finisherCard != null &&
            (def == finisherCard ||
             string.Equals(def.Id, finisherCard.Id, System.StringComparison.OrdinalIgnoreCase));

        private void TryBeat(string id)
        {
            if (!TutorialEnabledFlag) return;

            if (HasFired(id)) return;
            controller?.EnqueueGuided(id);
        }

        private bool HasFired(string id)
        {
            var pd = GameManager.Instance != null
                ? GameManager.Instance.PersistentGameplayData : null;
            return pd != null && pd.HasFiredTutorial(id);
        }
    }
}
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
                 "Wormus Major, Singing Field, Warm Up. Role/domain fallbacks " +
                 "when an exact card is missing; full miss → M4.5 guarantee (D1).")]
        [SerializeField] private List<CardDefinition> forcedInitialHand = new();

        [Header("Debug")]
        [SerializeField] private bool verboseLogging = true;

        private const string DebugTag = "<color=#ffd47f>[TutGuided]</color>";
        private void Log(string msg) { if (verboseLogging) Debug.Log($"{DebugTag} {msg}"); }

        private static readonly string[] SupersededIds =
        {
            TutorialTriggerId.WelcomeToGig,
            TutorialTriggerId.FirstCompositionCard,
            TutorialTriggerId.FirstInspirationSpend,
            TutorialTriggerId.FirstLoopInspiration,
            TutorialTriggerId.FirstSongEnd,
            TutorialTriggerId.FirstAudienceAction,
            TutorialTriggerId.FirstActionCard,
            TutorialTriggerId.FirstStatusApplied,
            TutorialTriggerId.FirstGigWon,
        };

        private bool _finisherPlayedThisSong;
        // [TUT-R2b FIX-2] Request flags: set when the driver ACTS on a beat,
        // independent of when the dialog completes (HasFired flips only on
        // completion; loops keep running under modals).
        private bool _beat7Requested;
        private bool _beat8Requested;

        // ---------------- lifecycle ----------------

        private void Awake()
        {
            if (!HasFired(TutorialTriggerId.YourTurn))
                FillForcedInitialHand();
        }

        private void OnEnable()
        {
            controller?.SetSuppressedTriggers(SupersededIds);

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

            TutorialInputGate.Clear();
            TutorialLoopHoldGate.Release();
            TutorialScriptedDrawQueue.Clear();
        }

        // ---------------- beat 2 forced hand ----------------

        private void FillForcedInitialHand()
        {
            TutorialScriptedDrawQueue.Clear();

            var fallbacks = new System.Func<CardDefinition, bool>[]
            {
                c => IsCompositionWithRole(c, TrackRole.Rhythm),
                c => IsCompositionWithRole(c, TrackRole.Backing),
                c => IsCompositionWithRole(c, TrackRole.Melody),
                c => c != null && c.IsAction,
            };

            for (int i = 0; i < 4; i++)
            {
                var def = i < forcedInitialHand.Count ? forcedInitialHand[i] : null;
                var primary = def != null
                    ? TutorialScriptedDrawQueue.ById(def.Id)
                    : fallbacks[i];
                TutorialScriptedDrawQueue.Enqueue(new TutorialScriptedDrawQueue.Entry(
                    label: def != null ? def.name : $"forced-slot-{i}",
                    primary: primary,
                    fallback: fallbacks[i]));
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
            TryBeat(TutorialTriggerId.JamWelcome);
            TryBeat(TutorialTriggerId.YourTurn);
        }

        private void OnDialogCompleted(string id)
        {
            switch (id)
            {
                case TutorialTriggerId.YourTurn:
                    TryBeat(TutorialTriggerId.PlayComposition);
                    break;

                case TutorialTriggerId.PlayComposition:
                    if (DeckManager.Instance != null &&
                        DeckManager.Instance.HandHas(c => c.IsComposition))
                    {
                        TutorialInputGate.Set(TutorialInputGate.GateMode.CompositionOnly);
                        Log("beat 3 gate ARMED (CompositionOnly)");
                    }
                    else
                    {
                        Log("beat 3 gate DEGRADED (no composition in hand) — released");
                    }
                    break;

                case TutorialTriggerId.TracksThree:
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
            if (e.IsComposition &&
                TutorialInputGate.Mode == TutorialInputGate.GateMode.CompositionOnly)
            {
                TutorialInputGate.Clear();
                Log("beat 3 gate SATISFIED (composition played)");
                TryBeat(TutorialTriggerId.TracksThree);
            }

            if (IsFinisher(e.Definition))
            {
                _finisherPlayedThisSong = true;
                if (TutorialLoopHoldGate.IsArmed)
                {
                    TutorialLoopHoldGate.Release();
                    Log("beat 8 holdLoop RELEASED (finisher played)");
                }
            }
        }

        private void OnPlayPressed()
        {
            if (TutorialInputGate.Mode != TutorialInputGate.GateMode.PlayOnly) return;
            TutorialInputGate.Clear();
            Log("beat 5 gate SATISFIED (Play pressed)");
            TryBeat(TutorialTriggerId.LoopsStructure);
        }

        private void OnLoopResolved(LoopResolvedEvent e)
        {
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
                Log("beat 8 fire — degrade (a): finisher already played; popup, no hold");
                TryBeat(TutorialTriggerId.PlayFinisher);
                return;
            }

            var deck = DeckManager.Instance;
            var pd = GameManager.Instance != null
                ? GameManager.Instance.PersistentGameplayData : null;

            bool available = finisherCard != null && deck != null &&
                (deck.HandHas(IsFinisher) || deck.PilesHave(IsFinisher));
            bool affordable = pd != null && finisherCard != null &&
                pd.CurrentInspiration >= finisherCard.InspirationCost;

            if (!available || !affordable)
            {
                Log($"beat 8 DEGRADE (b): available={available} affordable={affordable} " +
                    $"(insp={(pd != null ? pd.CurrentInspiration : -1)} vs " +
                    $"cost={(finisherCard != null ? finisherCard.InspirationCost : -1)}) " +
                    "— no hold, marked fired silently");
                controller?.MarkFiredWithoutShow(TutorialTriggerId.PlayFinisher);
                return;
            }

            TutorialLoopHoldGate.Arm();
            Log("beat 8 holdLoop ARMED");
            TryBeat(TutorialTriggerId.PlayFinisher);
        }

        private void OnSongEnd(SongEndVibeEvent e)
        {
            TryBeat(TutorialTriggerId.SongEndVibe);

            // Song boundary: reset per-song tracking so beat 8 can degrade/fire
            // correctly in later songs of gig 1 if it hasn't completed yet.
            _finisherPlayedThisSong = false;
            _beat8Requested = HasFired(TutorialTriggerId.PlayFinisher);
            if (TutorialLoopHoldGate.IsArmed)
            {
                TutorialLoopHoldGate.Release();
                Log("holdLoop released defensively at song end");
            }
        }

        private void OnAudienceTurn(AudienceTurnStartedEvent e)
        {
            TryBeat(TutorialTriggerId.AudienceTurn);
        }

        // ---------------- helpers ----------------

        private bool IsFinisher(CardDefinition def) =>
            def != null && finisherCard != null &&
            (def == finisherCard ||
             string.Equals(def.Id, finisherCard.Id, System.StringComparison.OrdinalIgnoreCase));

        private void TryBeat(string id)
        {
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
# SSoT_Dev_Mode — ALWTTT

**Status:** Active governed SSoT
**Scope:** Dev Mode tooling for playtest iteration: scripting-define gating, overlay, infinite turns, convinced-audience reset, between-song hand reset, hand-visibility re-enable, runtime card spawning from catalogue.
**Owns:** Dev Mode compile-time gating, overlay interaction surface, infinite-turns semantics, runtime card spawn entry point, Dev Mode entry points into gameplay managers, documented hand-visibility gap in production code.
**Does not own:** GigManager phase flow (`runtime/SSoT_Runtime_Flow.md`), CompositionSession boundary (`runtime/SSoT_Runtime_CompositionSession_Integration.md`), status system (`systems/SSoT_Status_Effects.md`), card authoring (`systems/SSoT_Card_Authoring_Contracts.md`).

---

## 1. Purpose

Dev Mode is the tooling layer that makes ALWTTT playtestable. Without it, the Combat MVP is technically complete but cannot be iterated on: every gig ends after one song, convinced audience members drop out of the turn loop, and runtime state cannot be nudged during play.

Phase 1 (2026-04-17) delivered the minimum unblocker for QA iteration: **infinite turns**, **convinced-audience reset**, and a **clean hand reset between song cycles**. Phase 2 (2026-04-20) added the first interactive runtime mutation: **arbitrary card spawning from the full catalogue into the active hand**. Later phases (stat editing, modifier toggles, audience-reaction transparency, Breakdown entry point) build on the same infrastructure.

This SSoT is the primary authority for Dev Mode's compile-time gating, overlay behavior, runtime entry points, and known limitations.

---

## 2. Compile-time gating

Dev Mode uses a scripting define, not an assembly definition, to keep the patch surface minimal and to allow gradual coexistence with production code.

- **Scripting define:** `ALWTTT_DEV`. Set in Project Settings → Player → Other Settings → Scripting Define Symbols for the target build platform.
- **Guard style:** file-level `#if ALWTTT_DEV` at the top of dedicated Dev Mode files; block-level `#if ALWTTT_DEV ... #endif` around all Dev Mode hooks injected into production files.
- **No asmdef:** Dev Mode code lives alongside production code in the same assembly. The scripting define is the only boundary.
- **Namespace:** `ALWTTT.DevMode`.
- **Production builds:** omit `ALWTTT_DEV` from Scripting Define Symbols. All Dev Mode code and hooks disappear at compile time. No release-build leakage.

Verification: the presence of any `<color=lime>[DevMode]</color>` log line in the Console confirms Dev Mode is compiled. Absence of those lines in a Dev Mode code path means the define is not set.

---

## 3. Overlay

Dev Mode's interaction surface is an IMGUI overlay on the existing gig scene.

- **Entry point:** `DevModeController : MonoBehaviour`, singleton (`DevModeController.Instance`), placed on a persistent scene object.
- **Toggle:** `F12`. Overlay defaults to off.
- **Scale:** `_overlayScale` inspector field (`Range(1f, 4f)`, default `2.0f`). Applies a uniform `GUI.matrix` scale so IMGUI remains legible at modern resolutions.
- **Window size:** `480×380` (default). Grown in Phase 2 to accommodate the Catalogue list comfortably at any scale; Infinite tab has room to spare.
- **Tab toolbar (Phase 2):** `GUILayout.Toolbar` at the top of the window switches between `Infinite` and `Catalogue`. One draggable window, one toggle key, one scale — tab content switches in-place.
- **Verbose logging:** `_verboseLogs` inspector flag. When on, the overlay reports Hand/HandPile/Draw/Discard counts and the current `GigPhase`, and `OnPlayerTurnStartInfiniteMode` logs its call context.
- **Awake guard:** duplicate-instance destruction; `InfiniteTurnsEnabled` is reset on `OnDestroy` so it does not leak across scene reloads.

Tab content:

**Infinite tab (Phase 1):**
- Infinite Turns checkbox (see §4).
- "Reset Convinced Audience Now" button for manual reset (the automatic reset still runs on PlayerTurn).
- Auto-reset counter for the current gig.
- Song / Required counter, Cohesion, and (verbose) Hand/DrawPile/DiscardPile/HandPile counts + current phase.

**Catalogue tab (Phase 2):**
- Text search + Action/Composition kind toggles (Deck-Editor parity filter set).
- Gate status line: "Ready. Hand: N/MAX (shown/total)" when spawn is allowed, otherwise "Spawn gated: <reason> ..." with the reason returned by `DeckManager.CanDevSpawnToHand(out reason)`.
- Scrollable card list — one row per filtered `CardDefinition`. Row shows kind badge (`[A]` / `[C]` / `[?]`), display name, cost, Spawn button. Spawn button routes through `DeckManager.DevSpawnCardToHand(def)`. See §11.
- Filter is cache-invalidated on dirty detection (source count, search string, toggle state); it does not reallocate every frame.

Phases 3+ will reuse this overlay as the shell for stat editing and audience transparency.

---

## 4. Infinite Turns — runtime semantics

When `DevModeController.InfiniteTurnsEnabled` is true:

### 4.1 Gig never terminates by completion check
`GigManager.IsGigComplete` returns `false` unconditionally while Infinite Turns is on. The normal `CurrentSongIndex >= _requiredSongCount` check is bypassed.

### 4.2 Win/Lose suppression
Both `GigManager.WinGig` and `GigManager.LoseGig` early-return with a `[DevMode] … suppressed (infinite turns)` log. Cohesion and musician-health persistence are skipped; the gig keeps running. Normal flow resumes the moment Infinite Turns is toggled off (the next end-of-song will resolve via the normal path).

### 4.3 Convinced-audience auto-reset at PlayerTurn start
At `GigManager.ExecuteGigPhase(GigPhase.PlayerTurn)`, if Infinite Turns is on, `DevModeController.Instance.OnPlayerTurnStartInfiniteMode()` runs before musician status ticks. It iterates `GigManager.CurrentAudienceCharacterList` and calls `DevResetConvinced()` on any member whose `IsConvinced` is true. After any resets, `GigManager.RecalculateAudienceObstructions()` is invoked so seating/blocking reflects the un-convinced state.

`AudienceCharacterStats.DevResetConvinced()` performs the reset: `IsConvinced = false`, `SetCurrentVibe(0)`, `ClearStatus(StatusType.Convinced)` (legacy enum path retained for the status icon pipeline — see §7).

### 4.4 Between-song reset in `OnCompositionSessionEnded`
When a composition session ends and Infinite Turns is on, `GigManager.OnCompositionSessionEnded` runs three Dev Mode steps **in this order**:

1. **Diagnostic dump.** Logs `InfiniteTurnsEnabled`, `DeckManager.Instance` null-state, `IsGigComplete`, `skipAudienceActionsAfterFinalSong`, and `gigHand.activeSelf` before any flip.
2. **Hard hand reset.** `DeckManager.DevForceHandResetToDiscard()` destroys any `CardBase` GameObjects currently tracked in `HandController.Hand`, moves their `CardDefinition` entries from `HandPile` to `DiscardPile`, and sweeps stray `CardBase` children under `HandController.DrawTransform` that are not in the tracked hand. Returns the number of cards destroyed. Rationale: `CardBase.Discard()` gates on `IsPlayable` / `IsExhausted` and uses an async coroutine that can abandon GameObjects mid-flight when the scene transitions phases quickly. The hard reset bypasses this gate.
3. **Hand-visibility re-enable.** `SetHandVisible(true)` forces `gigHand.gameObject.SetActive(true)` and re-enables dragging. See §5.

After these three steps, control returns to the phase machine via `CurrentGigPhase = GigPhase.AudienceTurn`, and the normal gig loop continues.

---

## 5. Hand-visibility gap in production code — load-bearing fact

CompositionSession calls `ShowHand(false)` through `ICompositionContext` during session setup to hide the hand while the song is playing. In single-song gigs this is invisible: the gig ends at `OnCompositionSessionEnded` and the scene tears down.

In Infinite Turns, the gig does not end. The next PlayerTurn will call `DeckManager.DrawCards`, which instantiates `CardBase` GameObjects as children of `HandController.DrawTransform`. If `DrawTransform.activeInHierarchy` is false, the newly-instantiated cards are inactive and unusable — visually present as "ghost" sprites, not draggable, not playable.

Production code has no symmetric `ShowHand(true)` between song-end and the next PlayerTurn because the single-song path never needs one. Dev Mode Phase 1 bridges this gap explicitly in `OnCompositionSessionEnded` (see §4.4 step 3).

This is the load-bearing fact Phase 1 codified. If Infinite Turns is ever re-implemented from scratch, or if a non-Dev-Mode multi-song flow is ever added to production, the same gap must be bridged on that path.

**Phase 2 corollary.** `DeckManager.CanDevSpawnToHand` refuses to spawn while `HandController.DrawTransform.activeInHierarchy` is false. The spawn gate is the mirror of the Phase 1 bridge: it prevents re-entry of the ghost-card state that Phase 1 fixes on the reset path. See §11.

---

## 6. Entry points and modified surfaces

**New files:**
- `Assets/Scripts/DevMode/DevModeController.cs` — file-level `#if ALWTTT_DEV`. Singleton, overlay, tab toolbar, infinite-turns state, `OnPlayerTurnStartInfiniteMode`, `ResetConvincedAudience`.
- `Assets/Scripts/DevMode/DevCardCatalogueTab.cs` — file-level `#if ALWTTT_DEV`. Phase 2 static helper that renders the Catalogue tab body. Holds filter state, reads from `GameplayData.AllCardsList`, delegates spawn to `DeckManager.DevSpawnCardToHand`. No runtime mutation outside that delegation.

**Modified production files (block-level `#if ALWTTT_DEV` patches only):**
- `Assets/Scripts/Managers/GigManager.cs` — five patches:
  1. `using ALWTTT.DevMode;` import.
  2. `IsGigComplete` returns false under Infinite Turns.
  3. `ExecuteGigPhase(GigPhase.PlayerTurn)` — completion-check bypass, diagnostic logs, `OnPlayerTurnStartInfiniteMode` invocation.
  4. `WinGig` and `LoseGig` — early-return suppression with log.
  5. `OnCompositionSessionEnded` — diagnostic dump, `DevForceHandResetToDiscard`, `SetHandVisible(true)`.
- `Assets/Scripts/Managers/DeckManager.cs`:
  - `using ALWTTT.Enums;` (unconditional — needed by `CanDevSpawnToHand` for the `GigPhase` reference; free in production builds).
  - `DevForceHandResetToDiscard()` method (Phase 1).
  - `DevSpawnCardToHand(CardDefinition) : bool` method (Phase 2) — runtime spawn entry point. Gated by `CanDevSpawnToHand` (see §11).
  - `CanDevSpawnToHand() : bool` and `CanDevSpawnToHand(out string reason) : bool` methods (Phase 2) — centralized gate predicate.
  - `DrawCards` entry — optional diagnostic log dumping `HandController.gameObject` and `DrawTransform` active-state at draw-time entry (kept for future debugging).
- `Assets/Scripts/Characters/AudienceCharacterStats.cs` — `DevResetConvinced()` method.

All Dev Mode hooks are `#if ALWTTT_DEV`-guarded. No production behavior change when the define is absent.

---

## 7. Legacy `StatusType.Convinced` coexistence

`DevResetConvinced` clears the legacy `StatusType.Convinced` enum status in addition to flipping `IsConvinced`. This is deliberate. The SO-based catalogue is the authority going forward (see `SSoT_Status_Effects.md` §3.3), but some icon and UI callers still consult the legacy `statusDict`. Until those call sites migrate, Dev Mode must keep both views in sync. No new work should rely on the legacy enum.

---

## 8. Caveats and known limitations

### 8.1 Direct Gig Scene entry path (deferred)
Starting the Gig Scene directly — bypassing `GigSetupScene` — can reproduce ghost-card behavior under Infinite Turns that does not occur via the normal entry path. Cause unverified; likely related to `SetupDeck` initialization ordering when `GigSetupController` has not run. **Out of scope for Phase 1.** Dev Mode is intended to be entered via the normal scene-entry flow. If direct-start becomes needed (e.g. for editor-driven tests), a separate investigation is required.

### 8.2 `CardBase` GameObject accumulation under `DiscardPos` (cosmetic, deferred)
`CardBase.Discard()` reparents GameObjects to `DiscardPos` rather than destroying them. In single-song gigs this is invisible because scene teardown sweeps them. In Infinite Turns, over many cycles, `DiscardPos` accumulates orphaned GameObjects. Phase 2 worsens this cosmetically because spawning is the fastest way to create+discard many cards in a session. Gameplay state (`HandPile`, `DiscardPile`, `Hand`) remains correct. **Not fixed in Phase 2.** Separate ticket; candidate for destruction-on-Discard refactor later.

### 8.3 `GigPhase.SongPerformance` appears unused in logs
Neither the Phase 1 smoke runs nor historical runs show `Executing gig phase: SongPerformance`. The song plays inside `PlayerTurn` via `CompositionSession` ticks in `Update`, and `CurrentSongIndex` is updated on `OnCompositionSongFinished`. If `SongPerformance` is dead code, that is a separate runtime cleanup — out of Dev Mode scope.

### 8.4 `CardActionTiming` on debug cards (Phase 2)
Action cards without an explicit `CardActionTiming` inherit a default that does not permit play during the current PlayerTurn. Because Dev Mode spawn is PlayerTurn-gated (see §11), a debug action card whose timing excludes PlayerTurn cannot be spawned and played in one session — producing a `Cannot play action card '<name>' in current timing. Returning to hand.` log.

Any new debug card authored for Dev Mode use should set `actionTiming: "Always"` (or an explicit timing that includes PlayerTurn) in its JSON import. This was the root cause of Phase 2's T7 deferral (see §9).

### 8.5 Spawn accepted pollution (Phase 2 decision U1)
Cards spawned via `DevSpawnCardToHand` are added to `HandPile` only. On play they route through the normal `OnCardDiscarded` path to `DiscardPile`. After one reshuffle cycle the spawned card enters `DrawPile` organically — at which point it is indistinguishable from an originally-drawn card.

This is **accepted pollution**, decided 2026-04-20 in the Phase 2 sub-roadmap. The tradeoff is simplicity (one code path, identical to `DrawCards`) vs. ephemerality (requires a parallel tracked-list in `DeckManager`). If tests need ephemeral behavior, add a Phase 3 override; do not branch the normal path.

### 8.6 `test_pass_turn` composition card obsolete
`test_pass_turn` was authored 2026-03-23 as a `CompositionCardPayload` with no effects and `PrimaryKind = None`, intended to provide a zero-cost "pass turn" in the composition phase. The current `SongCompositionUI.ApplyCardToPart` rejects `PrimaryKind == None` explicitly (`"unsupported PrimaryKind 'None'"` → returns false), so the card cannot legally advance a composition.

The asset was removed on 2026-04-20 during Phase 2 closure (see changelog-ssot). ST-P2-4 was re-validated using the `Waltz` composition card from `TestDeck_FullCoverage`, which produces the same runtime surface (composition card spawned via DevSpawn, dropped in a part zone, successfully queued).

If a "no-op composition card" concept is ever needed again, it requires an explicit design decision to extend `ApplyCardToPart` to accept `PrimaryKind == None` with no modifier effects. Not scheduled.

---

## 9. Smoke-test coverage

### 9.1 Phase 1 (GigSetupScene → Gig Scene normal entry)

| Test | Subject | Result |
|---|---|---|
| ST-3 | Song transitions under Infinite Turns — no ghost cards, `DrawTransform.activeInHierarchy=True` at draw entry | ✅ PASS 2026-04-17 |
| ST-4 | Convinced-audience auto-reset at PlayerTurn start | ✅ PASS 2026-04-17 |
| ST-5 | Win/Lose suppression under Infinite Turns | ✅ PASS 2026-04-17 |
| ST-7 | Overlay telemetry tracks hand/pile counts and phase across cycles | ✅ PASS 2026-04-17 |
| Regression | Normal single-song gig unaffected when Infinite Turns is off and overlay is closed | ✅ PASS 2026-04-17 |

### 9.2 Phase 2 (card spawner)

| Test | Subject | Result |
|---|---|---|
| ST-P2-1 | PlayerTurn gate — spawn disabled outside PlayerTurn, reason surfaced in gate status line | ✅ PASS 2026-04-20 |
| ST-P2-2 | Max-hand gate — spawn disabled at `HandPile.Count >= MaxCardsOnHand` | ✅ PASS 2026-04-20 |
| ST-P2-3 | Normal play pipeline — spawned action card plays, applies effects, routes to DiscardPile | ✅ PASS 2026-04-20 |
| ST-P2-4 | Composition card spawning — spawned composition card queues into composition session correctly | ✅ PASS 2026-04-20 (validated with `Waltz`; `test_pass_turn` obsolete — see §8.6) |
| ST-P2-5 | Filter correctness — search is case-insensitive on DisplayName; kind toggles live-update | ✅ PASS 2026-04-20 |
| ST-P2-6 | Infinite-mode regression — Phase 1 behavior preserved when Phase 2 is active | ✅ PASS 2026-04-20 |
| ST-P2-7 | Production-build strip — `ALWTTT_DEV` removed → clean compile, no residual symbols | ✅ PASS 2026-04-20 |

### 9.3 Multi-turn status tests deferred from M1.2 (runnable against Phase 2)

| Test | Subject | Result |
|---|---|---|
| T5 | Choke decay at `PlayerTurnStart` tick | ✅ PASS 2026-04-20. Bonus finding: stunned targets reject further targeting via `HandController.TryResolveCardTarget`, indirectly confirming `CharacterBase.IsStunned` tracks `DisableActions` stacks. |
| T7 | Shaken expiry across a song cycle at `AudienceTurnStart` tick | ⏸️ DEFERRED to Phase 3. `debug_apply_shaken_to_musician` could not be played because its `CardActionTiming` default excludes PlayerTurn (see §8.4). Shaken status itself is implemented and applied via `MusicianBase.OnBreakdown`; validation requires a Dev Mode entry point to force Breakdown directly (Phase 3 scope) or a debug card with `actionTiming: "Always"`. |
| T8 | Feedback DoT accumulation across `AudienceTurnRoutine` | ✅ PASS 2026-04-20. Each stack applies +1 stress per AudienceTurn via `ApplyIncomingStressWithComposure`; stacks persist turn-to-turn. Finding: Feedback has no tick decay — by design; the SO is configured with `DecayMode.None`. `SSoT_Status_Effects.md` §5.6 does not document this explicitly and should be updated (tracked as an open doc gap). |

Phase 2 closes 2 of the 3 deferred M1.2 tests. T7 is explicitly deferred with a documented unblock path (Phase 3 Breakdown entry point).

---

## 10. Update rule

This SSoT must be updated when any of the following change:
- The scripting define name or gating strategy.
- The overlay's compositional surface (new controls, removed controls, changed toggle key, added/removed tab).
- The set of Dev Mode entry points into production code (new Dev-prefixed method; changed signature or gate).
- The hand-visibility bridge semantics in `OnCompositionSessionEnded`.
- The spawn-gate predicate in `CanDevSpawnToHand`.
- Any new Phase (3+) that adds or modifies runtime-mutation surfaces.

Updating `SSoT_Dev_Mode.md` typically implies companion updates to `CURRENT_STATE.md` (operational slice) and `changelog-ssot.md` (semantic/authority).

---

## 11. Phase 2 — card spawner

### 11.1 Capability

Arbitrary instantiation of any `CardDefinition` from the game's runtime catalogue into the active hand during PlayerTurn, via the normal card-play pipeline. Primary iteration surface for card balance and multi-turn status validation.

### 11.2 Catalogue source

`GameManager.GameplayData.AllCardsList`. Runtime-authoritative; already used by `GameManager.SetInitialDeck` for the random-deck branch. No new SO. If a card is absent from `AllCardsList` it is not considered "real" by the game; the deck editor's catalogue (which scans `AssetDatabase`) may surface cards `AllCardsList` does not — this is an acceptable asymmetry. Cards authored for Dev Mode use must be added to `AllCardsList` on the `GameplayData` SO explicitly.

### 11.3 Spawn pipeline

`DeckManager.DevSpawnCardToHand(CardDefinition)` is the sole Dev Mode entry point. Overlay code (`DevCardCatalogueTab`) does not mutate hand state directly.

On a successful call:
1. `GameManager.BuildAndGetCard(def, HandController.DrawTransform)` — identical to the per-card tail of `DrawCards`.
2. `HandController.AddCardToHand(built)` — adds to the visible hand list.
3. `HandPile.Add(def)` — tracks the card as "in hand" for the runtime deck model.
4. `UIManager.GigCanvas.SetPileTexts()` if available.
5. Lime `[DevMode] DevSpawnCardToHand: '<name>' → hand=n/max handPile=n discard=n draw=n` log.

On any gate failure, `DevSpawnCardToHand` returns false and logs `DevSpawnCardToHand skipped ('<name>'): <reason>` without mutating any state. Overlay's Spawn button is `GUI.enabled = CanDevSpawnToHand()`.

### 11.4 Gate predicate

`CanDevSpawnToHand(out string reason)` is the authoritative gate. It checks, in order:
- `HandController != null` (otherwise "HandController is null").
- `HandController.DrawTransform != null` (otherwise "HandController.DrawTransform is null").
- `HandController.DrawTransform.gameObject.activeInHierarchy` (otherwise "HandController.DrawTransform inactive (hand hidden)"). This is the mirror of the Phase 1 hand-visibility bridge — see §5.
- `GameManager.GameplayData != null` (otherwise "GameManager.GameplayData is null").
- `GigManager.Instance != null` (otherwise "GigManager.Instance is null").
- `GigManager.Instance.CurrentGigPhase == GigPhase.PlayerTurn` (otherwise "Not PlayerTurn (current: <phase>)").
- `HandPile.Count < MaxCardsOnHand` (otherwise "Hand full (N/N)").

Any change to the gate's order, checks, or error strings must update this section and the `DevCardCatalogueTab` gate-status display.

### 11.5 Spawned-card lifecycle (decision U1)

Spawned cards enter `HandPile` on spawn, `DiscardPile` on play, and `DrawPile` on next reshuffle — see §8.5. This is accepted pollution.

### 11.6 Overlay filter state

`DevCardCatalogueTab` keeps the filter state (`_search`, `_filterAction`, `_filterComposition`, `_scroll`) as static fields so filter settings persist while the overlay is open across tab switches. Filter cache is invalidated on dirty detection (source count, search string, toggle state) to avoid per-frame reallocation.

### 11.7 Unblocks

Phase 2 unblocked the following that Phase 1 alone could not:
- **Multi-turn status validation tests T5/T8 from M1.2** — passed 2026-04-20 (§9.3).
- **Arbitrary starting conditions for combat tuning** — any card can be tested without deck re-authoring.
- **Composition card spawning during a live gig** — used for ST-P2-4 regression.

Phase 2 does **not** unblock:
- T7 Shaken validation (still requires either a `actionTiming: "Always"` debug card or a Dev entry point for Breakdown — deferred to Phase 3).
- Stat editing (Inspiration, LoopScore, SongHype, Cohesion, per-character stats — Phase 3 scope).
- Encounter-modifier toggles (Phase 3 scope).

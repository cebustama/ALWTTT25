# SSoT_Dev_Mode — ALWTTT

**Status:** Active governed SSoT
**Scope:** Dev Mode tooling for playtest iteration: scripting-define gating, overlay, infinite turns, convinced-audience reset, between-song hand reset, hand-visibility re-enable, runtime card spawning from catalogue, Breakdown entry point, gig-wide stat editing, per-character stat editing, status apply/remove picker.
**Owns:** Dev Mode compile-time gating, overlay interaction surface, infinite-turns semantics, runtime card spawn entry point, Breakdown force-trigger entry point, gig-wide stat-editing entry points (SongHype, Inspiration, BandCohesion), per-character stat-editing entry points (Stress, MaxStress, Composure, Vibe, MaxVibe, Flow), status apply/remove picker surface, Dev Mode entry points into gameplay managers, documented hand-visibility gap in production code.
**Does not own:** GigManager phase flow (`runtime/SSoT_Runtime_Flow.md`), CompositionSession boundary (`runtime/SSoT_Runtime_CompositionSession_Integration.md`), status system (`systems/SSoT_Status_Effects.md`), card authoring (`systems/SSoT_Card_Authoring_Contracts.md`).

---

## 1. Purpose

Dev Mode is the tooling layer that makes ALWTTT playtestable. Without it, the Combat MVP is technically complete but cannot be iterated on: every gig ends after one song, convinced audience members drop out of the turn loop, and runtime state cannot be nudged during play.

Phase 1 (2026-04-17) delivered the minimum unblocker for QA iteration: **infinite turns**, **convinced-audience reset**, and a **clean hand reset between song cycles**. Phase 2 (2026-04-20) added the first interactive runtime mutation: **arbitrary card spawning from the full catalogue into the active hand**. Phase 3.1 (2026-04-23) added the **Breakdown entry point**, closing the last deferred M1.2 validation test. Phase 3.2 (2026-04-23) added **gig-wide stat editing** (SongHype, Inspiration, BandCohesion) with session-aware Inspiration routing and symmetric `LoseGig` dispatch on Cohesion 0. Phase 3.3a (2026-04-23) added **per-character stat editing** (Stress, MaxStress, Composure on musicians; Vibe, MaxVibe on audience) and extended gig-wide editing with Flow. Phase 3.3b (2026-04-24) added the **status apply/remove picker**, closing the state-editing gap. Deferred phases (P3.4 audience transparency panel, encounter modifier toggles) build on the same infrastructure.

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
- **Tab toolbar (Phase 2+):** `GUILayout.Toolbar` at the top of the window switches between `Infinite`, `Catalogue`, and `Stats`. One draggable window, one toggle key, one scale — tab content switches in-place.
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

**Stats tab (Phase 3.1–3.3b):**
- Breakdown section (P3.1): musician selector grid (shows `{Name} [{Stress}/{MaxStress}]`, appends `(BD)` when `IsBreakdown` is true), stress/stun/status readout for selected musician, "Force Breakdown → {Name}" button.
- Status readout iterates `StatusEffectContainer.Active` and displays `{DisplayName}×{Stacks}` for each active entry.
- Gig-Wide Stats section (P3.2 + P3.3a): SongHype slider `[0, MaxSongHype]`, Inspiration slider `[0, MaxInspiration]` bound to `LiveInspiration` (session value when composing, PD value otherwise), BandCohesion stepper (`−`/`+`, floor 0, no upper cap), Flow stepper (`−`/`+`, uniform ±1 applied to every musician's `DamageUpFlat` stack; aggregate readout via `GigManager.TotalFlowStacks`). Sliders fire through `GigManager.DevSet…` wrappers. Slider idle-epsilon on SongHype (`0.01f`) avoids per-frame event spam.
- Per-Character section (P3.3a + P3.3b): musician selector grid (shares index with Breakdown section) + per-musician Stress slider, MaxStress stepper (floor 1), Composure stepper (backed by `TempShieldTurn` status stacks — see §14.3). Audience selector grid + per-audience Vibe slider, MaxVibe stepper (floor 1). All stat writes fire through `DevSet…` wrappers on the respective stats classes.
- Per-Character status editing (P3.3b): each character's subsection (musician, audience) includes a status picker below the stat controls. **Active readout:** lists all active statuses on the selected character (`{DisplayName} ×{Stacks}`) with `[−1]` (decrements via `container.Apply(def, -1)`, auto-clears at 0) and `[Clear]` (immediate full removal via `container.Clear(id)`) buttons per row. **Catalogue picker:** `[◄][►]` buttons cycle through non-null entries in the character's `StatusCatalogue.Effects`; selected entry shown as `{DisplayName} ({EffectId})`; `[+1]` button calls `container.Apply(selectedSO, 1)` with a lime `[DevMode]` log. No `DevSet…` wrappers needed — the existing `StatusEffectContainer` public API is sufficient. Falls back gracefully: "(no catalogue — assign on prefab)" when `StatusCatalogue` is null on the character.

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
- `Assets/Scripts/DevMode/DevStatsTab.cs` — file-level `#if ALWTTT_DEV`. Phase 3.1/3.2/3.3a/3.3b static helper that renders the Stats tab body. Breakdown section (P3.1) + Gig-Wide Stats section (P3.2 + Flow row added P3.3a) + Per-Character section (P3.3a stat controls + P3.3b status picker). Dispatches to `GigManager.DevSet…`, `BandCharacterStats.DevSet…`, `AudienceCharacterStats.DevSet…` wrappers for stat editing, and directly to `StatusEffectContainer.Apply`/`Clear` for the Composure stepper and P3.3b status picker. Phase 3.3b additions: `DrawStatusPicker(CharacterBase, ref int)` method, `_musicianStatusPickerIndex` and `_audienceStatusPickerIndex` static fields, `using ALWTTT.Characters` directive.

**Modified production files (block-level `#if ALWTTT_DEV` patches only):**
- `Assets/Scripts/Managers/GigManager.cs` — seven patches:
  1. `using ALWTTT.DevMode;` import.
  2. `IsGigComplete` returns false under Infinite Turns.
  3. `ExecuteGigPhase(GigPhase.PlayerTurn)` — completion-check bypass, diagnostic logs, `OnPlayerTurnStartInfiniteMode` invocation.
  4. `WinGig` and `LoseGig` — early-return suppression with log.
  5. `OnCompositionSessionEnded` — diagnostic dump, `DevForceHandResetToDiscard`, `SetHandVisible(true)`.
  6. Phase 3.2 block after `AddSongHype`: `MaxSongHype` getter, `LiveInspiration` getter, `DevSetSongHype(float)`, `DevSetInspiration(int)` (routes to `CompositionSession.DevSetCurrentInspiration` when session is active), `DevSetBandCohesion(int)` (dispatches `LoseGig()` on 0; Infinite-Turns suppression inherited from `LoseGig`).
  7. Phase 3.3a additions to the same `#if ALWTTT_DEV` block: `TotalFlowStacks` getter (public wrapper over `GetTotalFlowStacks`), `DevAddFlowToAllMusicians(int delta)` — resolves the `"flow"` SO from the first available musician catalogue and applies the delta to every musician's `StatusEffectContainer`; pre-guards `Apply(-N)` on zero-stack containers to avoid spurious `OnStatusCleared` events.
- `Assets/Scripts/Managers/DeckManager.cs`:
  - `using ALWTTT.Enums;` (unconditional — needed by `CanDevSpawnToHand` for the `GigPhase` reference; free in production builds).
  - `DevForceHandResetToDiscard()` method (Phase 1).
  - `DevSpawnCardToHand(CardDefinition) : bool` method (Phase 2) — runtime spawn entry point. Gated by `CanDevSpawnToHand` (see §11).
  - `CanDevSpawnToHand() : bool` and `CanDevSpawnToHand(out string reason) : bool` methods (Phase 2) — centralized gate predicate.
  - `DrawCards` entry — optional diagnostic log dumping `HandController.gameObject` and `DrawTransform` active-state at draw-time entry (kept for future debugging).
- `Assets/Scripts/Characters/Audience/AudienceCharacterStats.cs` — Phase 3.1 surface: `DevResetConvinced()` method (implementation landed in P3.3a — previously doc-declared but unimplemented; see §7). Phase 3.3a surface: `CheckConvincedThreshold()` private helper extracted from `AddVibe`, `DevSetCurrentVibe(int)`, `DevSetMaxVibe(int)` — all route through the shared threshold-check so Dev and play paths cannot drift.
- `Assets/Scripts/Characters/Band/BandCharacterStats.cs` — Phase 3.1 surface: `DevResetBreakdown()` method. Sets `IsBreakdown = false` so `AddStress` can re-trigger the Breakdown path. Phase 3.3a surface: `CheckBreakdownThreshold()` private helper extracted from `AddStress`, `DevSetCurrentStress(int)`, `DevSetMaxStress(int)` (floor 1, clamps Current down, re-checks threshold). Dev Mode only; production code never un-breaks a musician.
- `Assets/Scripts/Characters/Band/MusicianBase.cs` — `DevForceBreakdown()` method. Calls `DevResetBreakdown()` then `AddStress(MaxStress)`. Routes through the natural Breakdown path (Cohesion−1, Stress reset, Shaken apply, IsStunned). Re-triggerable.
- `Assets/Scripts/Runtime/CompositionSession.cs` — Phase 3.2 block: `CurrentInspiration` getter + `DevSetCurrentInspiration(int)` method. Sets the session's live `_currentInspiration` field and calls `_ctx.CompositionUI?.SetInspiration(value)` to refresh the composition UI. Does not write back to `PersistentGameplayData` — caller (`GigManager.DevSetInspiration`) owns that side.

All Dev Mode hooks are `#if ALWTTT_DEV`-guarded. No production behavior change when the define is absent.

Phase 3.3b does not add any new production-class entry points. The status picker operates entirely through the existing `StatusEffectContainer.Apply`/`Clear` public API and `CharacterBase.StatusCatalogue` property. No new `DevSet…` wrappers on gameplay classes.

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
Action cards without an explicit `CardActionTiming` inherit a default that does not permit play during the current PlayerTurn. Because Dev Mode spawn is PlayerTurn-gated (see §11), a debug action card whose timing excludes PlayerTurn cannot be spawned and played in one session — producing a `Cannot play action card '<n>' in current timing. Returning to hand.` log.

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
| T7 | Shaken expiry across a song cycle | ✅ PASS 2026-04-23 (Phase 3.1). `DevForceBreakdown` applied Shaken; status decayed and cleared at expected tick boundary. Icon removed on expiry. Musician stun cleared. |
| T8 | Feedback DoT accumulation across `AudienceTurnRoutine` | ✅ PASS 2026-04-20. Each stack applies +1 stress per AudienceTurn via `ApplyIncomingStressWithComposure`; stacks persist turn-to-turn. Finding: Feedback has no tick decay — by design; the SO is configured with `DecayMode.None`. `SSoT_Status_Effects.md` §5.6 does not document this explicitly and should be updated (tracked as an open doc gap). |

All three deferred M1.2 tests now closed. No remaining multi-turn validation gap.

### 9.4 Phase 3.1 (Breakdown entry point)

| Test | Subject | Result |
|---|---|---|
| ST-P31-1 | Force Breakdown on healthy musician — Stress jumps to max then resets to fraction, Shaken icon appears, Cohesion drops by 1, musician stunned | ✅ PASS 2026-04-23 |
| ST-P31-2 | Re-trigger Breakdown on already-broken-down musician — Cohesion drops again, Shaken re-applies | ✅ PASS 2026-04-23 |
| ST-P31-3 | Force Breakdown triggers LoseGig at Cohesion 0 (Infinite Turns OFF) | ✅ PASS 2026-04-23 |
| ST-P31-4 | Force Breakdown suppresses LoseGig (Infinite Turns ON) — gig continues | ✅ PASS 2026-04-23 |
| ST-P31-5 | Musician selector updates dynamically — (BD) tag only on broken-down musician | ✅ PASS 2026-04-23 |
| ST-P31-6 | Status readout matches active StatusEffectContainer entries | ✅ PASS 2026-04-23 |

### 9.5 Phase 3.2 (gig-wide stat editing)

| Test | Subject | Result |
|---|---|---|
| ST-P32-1 | SongHype slider affects `_songHype` and audience beat intensity live | ✅ PASS 2026-04-23 |
| ST-P32-2 | Inspiration slider clamps to `MaxInspiration` and updates composition UI live | ✅ PASS 2026-04-23 |
| ST-P32-3 | Scrubbing Inspiration unblocks a previously-uncastable cost-1 composition card | ✅ PASS 2026-04-23 |
| ST-P32-4 | BandCohesion stepper to 0 dispatches `LoseGig` (Infinite Turns OFF) | ⚠️ Retroactively invalidated 2026-04-24 — see §9.8 ST-MB1-1 |
| ST-P32-5 | BandCohesion stepper to 0 is suppressed (Infinite Turns ON) | ⚠️ Retroactively invalidated 2026-04-24 — see §9.8 ST-MB1-2 |
| ST-P32-6 | P3.1 Breakdown tests still pass (regression) | ✅ PASS 2026-04-23 |
| ST-P32-7 | Inspiration slider outside composition session writes PD, no crash | ✅ PASS 2026-04-23 |

### 9.6 Phase 3.3a (per-character stat editing + Flow gig-wide)

| Test | Subject | Result |
|---|---|---|
| ST-P33a-1 | Musician Stress slider → Max triggers Breakdown + Shaken (visual bar syncs via 0.1f tween) | ✅ PASS 2026-04-23 |
| ST-P33a-2 | Stress slider down after Breakdown — sticky state preserved (regression) | ✅ PASS 2026-04-23 |
| ST-P33a-3 | MaxStress stepper reduced to CurrentStress — Breakdown fires at boundary | ✅ PASS 2026-04-23 |
| ST-P33a-4 | MaxStress stepper floored at 1 | ✅ PASS 2026-04-23 |
| ST-P33a-5 | Composure stepper + applies `TempShieldTurn` stacks; icon appears; incoming Stress absorbs correctly | ✅ PASS 2026-04-23 |
| ST-P33a-6 | Composure stepper `−` disabled at 0 (no spurious `OnStatusCleared`) | ✅ PASS 2026-04-23 |
| ST-P33a-7 | Audience Vibe slider → Max triggers Convinced + status applied + Tall cleared | ✅ PASS 2026-04-23 |
| ST-P33a-8 | Audience MaxVibe reduced to CurrentVibe — Convinced fires at boundary | ✅ PASS 2026-04-23 |
| ST-P33a-9 | Flow gig-wide ± applies uniform delta to every musician; aggregate = sum | ✅ PASS 2026-04-23 |
| ST-P33a-10 | Song-end reset clears Flow and Composure (regression) | ✅ PASS 2026-04-23 |

### 9.7 Phase 3.3b (status apply/remove picker)

| Test | Subject | Result |
|---|---|---|
| ST-P33b-1 | Apply status to musician — icon appears, readout shows ×1, lime log | ✅ PASS 2026-04-24 |
| ST-P33b-2 | Stack status on musician — readout increments, no duplicate icon | ✅ PASS 2026-04-24 |
| ST-P33b-3 | Decrement via −1 — stacks decrease by 1, icon persists | ✅ PASS 2026-04-24 |
| ST-P33b-4 | Decrement to zero — auto-clear, icon removed with disappear animation | ✅ PASS 2026-04-24 |
| ST-P33b-5 | Clear button removes status entirely regardless of stack count | ✅ PASS 2026-04-24 |
| ST-P33b-6 | Apply status to audience member — icon appears, readout shows entry | ✅ PASS 2026-04-24 |
| ST-P33b-7 | Picker navigation wraps around (last → first, first → last) | ✅ PASS 2026-04-24 |
| ST-P33b-8 | No catalogue on character — active readout still works, apply section shows fallback label | ✅ PASS 2026-04-24 |
| ST-P33b-9 | Regression — Composure stepper still works, TempShieldTurn visible in active readout | ✅ PASS 2026-04-24 |
| ST-P33b-10 | Regression — Breakdown section status readout consistent with Per-Character active readout | ✅ PASS 2026-04-24 |

### 9.8 MB1 — DevSetBandCohesion dispatch alignment (2026-04-24)

Retroactive correction of §9.5. On 2026-04-24 it was discovered that `GigManager.DevSetBandCohesion` never contained the `LoseGig()` dispatch described in §13.2/§13.3, despite ST-P32-4 and ST-P32-5 being recorded as PASS on 2026-04-23. Those two entries were not honest observations. MB1 added the one-line dispatch + corrected the stale XML comment. Tests re-run under the corrected code:

| Test | Subject | Result |
|---|---|---|
| ST-MB1-1 | BandCohesion stepper to 0 dispatches `LoseGig` (Infinite Turns OFF) — loss panel appears | ✅ PASS 2026-04-24 |
| ST-MB1-2 | BandCohesion stepper to 0 is suppressed under Infinite Turns ON — lime log, gig continues | ✅ PASS 2026-04-24 |
| ST-MB1-3 | Cohesion up/down without hitting 0 — no `LoseGig` dispatch (regression) | ✅ PASS 2026-04-24 |
| ST-MB1-4 | Production build compiles with `ALWTTT_DEV` undefined; natural Breakdown → LoseGig path unchanged | ✅ PASS 2026-04-24 |

### 9.9 MB2 — catalogue split (2026-04-24)

Resolves §15.4 finding. Original `StatusEffectCatalogueSO.asset` split into `StatusEffectCatalogue_Musicians.asset` (6 canonical statuses: flow, composure, choke, shaken, exposed, feedback) and `StatusEffectCatalogue_Audience.asset` (empty at MVP; populated at M4.3 with Earworm). Musician and audience prefabs reassigned. No code change.

| Test | Subject | Result |
|---|---|---|
| ST-MB2-1 | Musician status picker lists exactly 6 canonical entries with wrap-around | ✅ PASS 2026-04-24 |
| ST-MB2-2 | Audience status picker shows graceful empty-catalogue fallback; no crash | ✅ PASS 2026-04-24 |
| ST-MB2-3 | Regression — Force Breakdown still applies Shaken (musician catalogue contains `shaken`) | ✅ PASS 2026-04-24 |
| ST-MB2-4 | Regression — `DevAddFlowToAllMusicians` still resolves `flow` key from musician catalogue | ✅ PASS 2026-04-24 |
| ST-MB2-5 | Regression — Feedback DoT applies and ticks (musician catalogue contains `feedback`) | ✅ PASS 2026-04-24 |
| ST-MB2-6 | No missing-reference warnings after scene reload — all prefab catalogue fields bound | ✅ PASS 2026-04-24 |

---

## 10. Update rule

This SSoT must be updated when any of the following change:
- The scripting define name or gating strategy.
- The overlay's compositional surface (new controls, removed controls, changed toggle key, added/removed tab).
- The set of Dev Mode entry points into production code (new Dev-prefixed method; changed signature or gate).
- The hand-visibility bridge semantics in `OnCompositionSessionEnded`.
- The spawn-gate predicate in `CanDevSpawnToHand`.
- Stats tab content changes (new sections, new controls, layout changes).
- New Dev-prefixed methods on gameplay classes (BandCharacterStats, MusicianBase, CompositionSession, etc.).
- The `LiveInspiration` routing contract (which field Dev reads/writes when composition is active vs. not) — if that rule ever changes, update §13 and this list.
- The Dev-setter animation-duration convention (currently `0.1f` as a workaround for `HealthBarController.SetCurrentValue(duration=0f)` no-op behavior; see §14.5). If the underlying component is fixed to handle zero durations correctly, the Dev setters may revert to `0f`.
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
5. Lime `[DevMode] DevSpawnCardToHand: '<n>' → hand=n/max handPile=n discard=n draw=n` log.

On any gate failure, `DevSpawnCardToHand` returns false and logs `DevSpawnCardToHand skipped ('<n>'): <reason>` without mutating any state. Overlay's Spawn button is `GUI.enabled = CanDevSpawnToHand()`.

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

Phase 2 did **not** unblock T7 Shaken validation — that required the Phase 3.1 Breakdown entry point (now closed).
- Stat editing (Inspiration, LoopScore, SongHype, Cohesion, per-character stats — Phase 3 scope).
- Encounter-modifier toggles (Phase 3 scope).

---

## 12. Phase 3.1 — Breakdown entry point (2026-04-23)

### 12.1 Capability

Stats tab added to the overlay. Phase 3.1 delivers one section: Breakdown. The musician selector grid shows all spawned musicians with live stress readout and `(BD)` tags. "Force Breakdown" button triggers a full Breakdown via the natural stress path.

### 12.2 Entry points

- `BandCharacterStats.DevResetBreakdown()` — sets `IsBreakdown = false`. Dev Mode only; production code never un-breaks a musician.
- `MusicianBase.DevForceBreakdown()` — resets IsBreakdown, then `AddStress(MaxStress)`. Clamps to cap, triggers Breakdown since IsBreakdown was just cleared. Downstream effects fire normally: Cohesion−1, Stress reset to `BreakdownStressResetFraction`, Shaken applied via catalogue lookup, `IsStunned` set.
- Re-triggerable: calling twice produces two Breakdown events (two Cohesion drops, Shaken re-application).
- With Infinite Turns on, `LoseGig` from Cohesion ≤ 0 is suppressed per §4.2.

### 12.3 Unblocks

- T7 Shaken expiry validation (deferred since M1.2, 2026-04-14). Now passed — M1.2 multi-turn validation gap fully closed.

### 12.4 Smoke tests

ST-P31-1 through ST-P31-6 + T7: all passed 2026-04-23. See §9.3 and §9.4.

---

## 13. Phase 3.2 — gig-wide stat editing (2026-04-23)

### 13.1 Capability

Stats tab gains a Gig-Wide Stats section. Three live controls at P3.2 close: SongHype slider, Inspiration slider, BandCohesion stepper. Flow stepper added in P3.3a (see §14.4). Changes take effect immediately and propagate to downstream gameplay consumers (UI, cost gate, loss condition) via the natural event / routing paths — not direct UI pokes.

### 13.2 Entry points

- `GigManager.MaxSongHype` (getter, Dev-only) — upper bound for the SongHype slider.
- `GigManager.LiveInspiration` (getter, Dev-only) — returns `_session.CurrentInspiration` when a `CompositionSession` is active, otherwise `PersistentGameplayData.CurrentInspiration`. This is what the slider reads so that the displayed value matches what the card-cost gate actually uses.
- `GigManager.DevSetSongHype(float)` — clamps to `[0, maxSongHype]`, calls `UpdateAudienceBeatIntensity()`, fires `OnSongHypeChanged01`. Bypasses the `debugSongHype` early-return that `AddSongHype` observes.
- `GigManager.DevSetInspiration(int)` — clamps to `[0, pd.MaxInspiration]`. Writes to `pd.CurrentInspiration`. If `_session != null && _session.IsActive`, also calls `_session.DevSetCurrentInspiration(clamped)`. This is the routing that makes the Dev slider affect the live composition budget.
- `GigManager.DevSetBandCohesion(int)` — floor at 0, no upper cap. Writes to `pd.BandCohesion`. If the new value is 0, calls `LoseGig()`. `LoseGig`'s existing Infinite-Turns suppression branch applies — OFF triggers the loss panel, ON logs suppression and continues the gig. This is the symmetric Dev counterpart to the natural Breakdown → Cohesion−1 → LoseGig path (see `MusicianBase.OnBreakdown`).
- `CompositionSession.CurrentInspiration` (getter, Dev-only).
- `CompositionSession.DevSetCurrentInspiration(int)` — sets `_currentInspiration`, calls `_ctx?.CompositionUI?.SetInspiration(value)`. Does not write to `PersistentGameplayData` — the caller (`GigManager.DevSetInspiration`) owns that.

### 13.3 Dev Mode principle — symmetric consequences

Dev Mode mutations reproduce the natural gameplay consequences of the same state change. `DevSetBandCohesion(0)` triggers `LoseGig()` the same way a Breakdown-driven cohesion drop would. This principle resolves the question that arose during P3.2 implementation: should Dev setters be "pure value editors" or "simulate natural writes"? The latter is what makes Dev Mode useful for playtest — you're testing the real game, not a detached debug view.

The Infinite-Turns suppression already built into `LoseGig`/`WinGig` (see §4.2) is the correct escape hatch for when you want to scrub values without gig-ending.

### 13.4 Inspiration dual-siting (architectural finding)

Inspiration lives in two fields that are not continuously synchronized:

- `PersistentGameplayData.CurrentInspiration` — the persistent / between-session field. Initialized by `GigManager.StartGig` from `InitialGigInspiration`; reset at PlayerTurn start from `TurnStartingInspiration` when `KeepInspirationBetweenTurns` is false.
- `CompositionSession._currentInspiration` — the live session budget. Set at `Begin()` and `ConfirmCurrentPartAndStart()` to `_rules.inspirationPerPart`. This is the value the composition card cost gate reads (`TryPlayCompositionCard` step 1) and the value the composition UI displays (`ui.SetInspiration`).

During an active composition, writes to `pd.CurrentInspiration` alone are invisible to gameplay. P3.2's `DevSetInspiration` therefore writes to both, and the Stats-tab slider reads `LiveInspiration` (which returns whichever is authoritative for the current moment).

This is a finding, not a contract change — the gameplay behavior is unchanged. If the dual-siting is ever reconciled into a single source of truth, this section and the `LiveInspiration` routing in §13.2 should be revisited. `SSoT_Gig_Combat_Core` §4.2 may want a one-line note surfacing this implementation reality; that's a separate doc pass.

### 13.5 Smoke tests

ST-P32-1 through ST-P32-7: all passed 2026-04-23. See §9.5.

### 13.6 Unblocks

Full live playtest of gig-wide meters without having to re-author cards or encounter configs. Unblocks tuning-in-play for SongHype curve shape, Inspiration tightness, and cohesion-loss sensitivity. P3.3 (per-character edits) is the remaining stat-editing gap.

---

## 14. Phase 3.3a — per-character stat editing + Flow gig-wide (2026-04-23)

### 14.1 Capability

Stats tab gains a Per-Character section. Two subsections: **Musician** (Stress slider, MaxStress stepper, Composure stepper) and **Audience** (Vibe slider, MaxVibe stepper). The existing Gig-Wide Stats section is extended with a Flow row (aggregate readout + uniform ± stepper). All writes route through the natural state-change paths — Breakdown and Convinced thresholds still fire, status icons animate, song-end reset still clears song-scoped stacks.

### 14.2 Entry points

- `BandCharacterStats.DevSetCurrentStress(int)` — clamps via `SetCurrentStress(target, duration: 0.1f)`, calls `CheckBreakdownThreshold()`. Sticky: setting Stress down after Breakdown does not un-break.
- `BandCharacterStats.DevSetMaxStress(int)` — floors at 1, clamps `CurrentStress` down via `SetCurrentStress(CurrentStress, duration: 0.1f)`, re-checks threshold.
- `BandCharacterStats.CheckBreakdownThreshold()` — private; extracted from `AddStress`. Single source of truth for the Breakdown trigger; called by `AddStress`, `DevSetCurrentStress`, `DevSetMaxStress`.
- `AudienceCharacterStats.DevSetCurrentVibe(int)` — clamps via `SetCurrentVibe(target, duration: 0.1f)`, calls `CheckConvincedThreshold()`.
- `AudienceCharacterStats.DevSetMaxVibe(int)` — floors at 1, clamps `CurrentVibe` down via `SetCurrentVibe(CurrentVibe, duration: 0.1f)`, re-checks threshold.
- `AudienceCharacterStats.CheckConvincedThreshold()` — private; extracted from `AddVibe`. Single source of truth for the Convinced trigger (sets `IsConvinced`, applies `StatusType.Convinced`, clears `StatusType.Tall`, fires `OnConvinced`).
- `GigManager.TotalFlowStacks` — public getter wrapping the existing private `GetTotalFlowStacks()`.
- `GigManager.DevAddFlowToAllMusicians(int delta)` — resolves the `"flow"` SO from the first available musician's catalogue, applies `delta` to every musician's `StatusEffectContainer`. Pre-guards `Apply(-N)` on zero-stack containers.

### 14.3 Composure as status-stack

Composure is not a first-class field on `BandCharacterStats`. It is modeled as stacks of `CharacterStatusId.TempShieldTurn` on the musician's `StatusEffectContainer`. The Per-Character section surfaces it as a meter-like stepper for parity with Stress/MaxStress, but the backing write is `statuses.Apply(composureSO, delta)`, not a setter on the stats class. The SO's authored `MaxStacks` is respected by the container's stacking policy; the UI disables the `−` button at 0 to avoid spurious `OnStatusCleared` events from the container's empty-apply path.

### 14.4 Flow gig-wide semantics

Flow is song/band-scoped in gameplay terms (see `SSoT_Gig_Combat_Core.md` §6.1) but is stored per-musician as `CharacterStatusId.DamageUpFlat` stacks. The gig-wide surface:
- **Display:** aggregate via `TotalFlowStacks` (sum across all musicians' DamageUpFlat stacks — identical to what the scoring path reads).
- **Edit:** uniform delta via `DevAddFlowToAllMusicians` — one stepper press applies ±1 to every musician's container.
- **Reset:** unchanged. `GigManager.ResetSongScopedStatuses` clears both Flow and Composure at song end, and the regression test (ST-P33a-10) confirms the Dev-added stacks reset correctly.

Editing individual musician Flow is not exposed. If per-musician Flow becomes meaningful later, the surface can be extended into the Per-Character section; until then, gig-wide is the authoritative affordance.

### 14.5 Animation-duration workaround

`DevSet…` setters pass `duration: 0.1f` (not `0f`) to the clamp-setters. `HealthBarController.SetCurrentValue(duration=0f)` does not write the final value through to the visual fill — internal state advances, the tweened bar stays at its previous position. This is a latent issue in `HealthBarController`, outside the Dev Mode scope, exposed for the first time by the Dev-driven jump-cut paths (no gameplay caller uses `0f`). The `0.1f` workaround animates over ~6 frames at 60fps — imperceptible as a delay but non-degenerate as a tween window. Revert to `0f` if/when the underlying component is fixed to write the final value on zero-duration calls. See §10 Update rule.

### 14.6 Smoke tests

ST-P33a-1 through ST-P33a-10: all passed 2026-04-23. See §9.6.

### 14.7 Side-resolution: `DevResetConvinced` implementation

P3.3a testing surfaced a pre-existing doc-vs-code drift: §6 and §7 of this SSoT have declared `AudienceCharacterStats.DevResetConvinced` as an existing Dev Mode entry point since P3.1 closure, but the method was never implemented. `DevModeController.ResetConvincedAudience` called it, which silently compile-failed only under `ALWTTT_DEV`. P3.3a adds the implementation — sets `IsConvinced = false` and `ClearStatus(StatusType.Convinced)` — which matches the contract §7 has always described. No authority change; only code caught up.

### 14.8 Unblocks

Full live playtest of per-character meters and per-musician status stacks (for the Composure case) without authoring changes. Unblocks tuning-in-play for Breakdown thresholds, audience persuasion curves, and Flow pacing. P3.3b (generic status apply/remove via picker) is the remaining state-editing gap.

---

## 15. Phase 3.3b — status apply/remove picker (2026-04-24)

### 15.1 Capability

Per-Character section gains generic status editing for both musicians and audience. The picker is embedded directly in each character's subsection (below the existing stat controls), reusing the existing character selectors. Two affordances per character:

1. **Active-status readout:** iterates `StatusEffectContainer.Active` keys, displays each entry as `{DisplayName} ×{Stacks}` with `[−1]` and `[Clear]` buttons. `[−1]` calls `container.Apply(inst.Definition, -1)`, which lets the stacking policy handle decrement and auto-clears at 0 stacks. `[Clear]` calls `container.Clear(id)` for immediate full removal.
2. **Catalogue-backed apply picker:** `[◄][►]` buttons cycle through non-null entries in `character.StatusCatalogue.Effects`. Selected entry displayed as `{DisplayName} ({EffectId})`. Wrap-around navigation. `[+1]` button calls `container.Apply(selectedSO, 1)` and emits a lime `[DevMode] StatusPicker: Applied {name} ×1 to {character}. Stacks now: {n}` log.

Graceful fallback: when `StatusCatalogue` is null on the character, the apply section shows "(no catalogue — assign on prefab)". The active-status readout still works (it reads from the container directly, not the catalogue).

### 15.2 No production-class patches

Unlike P3.3a, P3.3b does not add any `DevSet…` wrappers on gameplay classes. The existing `StatusEffectContainer` public API (`Apply`, `Clear`, `GetStacks`, `Active`) and `CharacterBase.StatusCatalogue` property provide everything the picker needs. The symmetric-consequences principle (§13.3) does not apply — status application through the container fires `OnStatusApplied`/`OnStatusChanged`/`OnStatusCleared` events, which update icons automatically via the existing `CharacterCanvas.BindStatusContainer` wiring.

### 15.3 Gameplay-flag asymmetry (known limitation)

Applying a status via the picker sets stacks on the `StatusEffectContainer` and fires icon events, but does **not** trigger gameplay-flag side effects that the natural paths would set:

- Applying `Convinced` (or the SO mapped to it) via the picker does **not** set `AudienceCharacterStats.IsConvinced = true`. The `IsConvinced` flag is only set by `CheckConvincedThreshold()`, which is called by `AddVibe` and `DevSetCurrentVibe/DevSetMaxVibe`.
- Applying `Shaken` via the picker does **not** set `BandCharacterStats.IsBreakdown = true` or trigger the Cohesion/Stress-reset/stun path. To test those, use `DevForceBreakdown` (§12).
- Clearing `DisableActions` via the picker **does** clear `IsStunned` — the getter derives from `Statuses.HasActive(CharacterStatusId.DisableActions)` and the `SyncLegacyStunFromStatuses` callback fires on `OnStatusCleared`.

This is acceptable for Dev Mode. The picker is a state-injection tool, not a gameplay-simulation tool. Users who want the full gameplay consequences should use the dedicated Dev actions (Breakdown, Vibe slider to max for Convinced, etc.).

### 15.4 Catalogue scope finding

P3.3b testing surfaced that musicians and audience may share the same `StatusEffectCatalogueSO` instance on their prefabs. This means the picker shows all statuses (including musician-only ones like Flow, Composure) on audience members and vice versa. Applying a musician-only status to an audience member is harmless (the container accepts it, an icon appears, but no gameplay code reads that primitive from audience) but is confusing.

**Resolved 2026-04-24 (MB2):** catalogue split into `StatusEffectCatalogue_Musicians.asset` (6 canonical musician statuses: flow, composure, choke, shaken, exposed, feedback) and `StatusEffectCatalogue_Audience.asset` (empty at MVP; populated at M4.3 with Earworm). Musician and audience prefabs reassigned to their respective catalogues. The picker now shows only character-type-appropriate statuses. Audience picker currently displays the `(no catalogue — assign on prefab)` fallback text from `DrawStatusPicker` because the audience catalogue is non-null but empty — this message is misleading but harmless. Minor UX polish deferred: distinguish null-catalogue from empty-catalogue fallback text. See §9.9 for validation.

### 15.5 Smoke tests

ST-P33b-1 through ST-P33b-10: all passed 2026-04-24. See §9.7.

### 15.6 Unblocks

Arbitrary status application and removal on any character without card authoring or encounter changes. Closes the state-editing gap identified in §14.8. Full Dev Mode stat/state toolset now covers: infinite turns, card spawning, gig-wide meters, per-character stats, and per-character status stacks. Remaining deferred: P3.4 audience transparency panel, encounter modifier toggles.

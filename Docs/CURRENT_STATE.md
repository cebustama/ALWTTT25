# CURRENT_STATE — ALWTTT

This file tracks the currently validated slice and immediate documentary obligations.

---

## 1. Active project slice now

### Gameplay / combat slice — Combat MVP closed (2026-03-23)
- deck/hand pipeline operating in play mode
- all four card effect types working end-to-end: `ModifyVibe`, `ModifyStress`, `ApplyStatusEffect`, `DrawCards`
- Composure absorption validated — positive Stress routes through `ApplyIncomingStressWithComposure`
- Breakdown validated — triggers at Stress ≥ MaxStress after Composure absorption
- Breakdown consequences implemented: Cohesion−1, Stress reset (configurable fraction), Shaken application
- LoseGig() triggered at Cohesion ≤ 0
- Exposed stress multiplier wired on musicians
- Feedback DoT wired on musicians (AudienceTurnRoutine)
- Tick timing wired: PlayerTurnStart (musicians) + AudienceTurnStart (audience)
- Choke decay validated (B5), Composure clear validated (B6), song-end reset validated (B7)
- All Phase 4 decisions (A–H) resolved and implemented

### Status effects — canonical MVP set active
Six SO entries confirmed in catalogue: `flow`, `composure`, `exposed`, `feedback`, `choke`, `shaken`

### Composition / music slice
The project contains a working ALWTTT-side runtime surface:
- `GigManager`, `MidiMusicManager`, `CompositionSession`, `SongConfigBuilder`, `LoopScoreCalculator`
- `test_pass_turn` composition card functional — routes through session pipeline and advances turn
- CompositionSession bypass of phase machine documented (see `SSoT_Runtime_Flow`)

### Docs tree
Governance migration is complete. All subsystem SSoTs are active and replacement-ready.
Phase 4 doc sync complete as of 2026-03-23.

---

## 2. What was just completed

### Combat MVP — Phase 4 (complete 2026-03-23)

| Decision | Change | Validation |
|---|---|---|
| A | Composure clears at PlayerTurn start in GigManager | ✅ B6 |
| B | Tick(PlayerTurnStart) for musicians, Tick(AudienceTurnStart) for audience. TickTiming enum extended (8=PlayerTurnStart, 9=AudienceTurnStart). | ✅ B5, B6 |
| B-asset | Choke SO Tick Timing → PlayerTurnStart | ✅ |
| C | OnBreakdown: Cohesion−1 + Stress reset to configurable fraction (default 0.5) + Shaken applied via catalogue key `"shaken"` | ✅ implemented |
| C-asset | ShakenRestriction=503 in CharacterStatusId, case in CharacterStatusPrimitiveDatabaseSO, DB repopulated, Shaken SO created (AudienceTurnStart tick, MaxStacks=1, LinearStacks) | ✅ |
| D | LoseGig() called when BandCohesion≤0 inside OnBreakdown. LoseGig() made public. | ✅ |
| E | Exposed: stress multiplier _exposedMultiplierPerStack=0.25f on BandCharacterStats | ✅ |
| E | Feedback DoT: musician-only in AudienceTurnRoutine | ✅ / audience version deferred |

### Phases 0–3 (completed 2026-03-21 through 2026-03-23)
- Phase 0: StatusEffectSO catalogue verified (six SO entries)
- Phase 1: Unified Stress path via `ApplyIncomingStressWithComposure`
- Phase 2: `DrawCardsSpec` stub replaced with `DeckManager.DrawCards(count)`
- Phase 3: Testing deck (9 action cards) imported and validated; all B/C/S checks passed

### Documentation governance — Batches 01–06
Completed in prior sessions. Governed docs tree is replacement-ready.

---

## 3. What is next

### Post-MVP work (not blocking closure, ordered by value)

1. **Composition session testing — highest priority**
   - Test Composition cards end-to-end in a real play session with music playing
   - Validate that card effects fire during composition and that the song actually sounds different
   - Focus on deck speed: add `DrawCards` effects to ensure players can cycle through composition cards efficiently
   - Design and test a first real testing deck (not just a stub deck) with varied composition card sets
   - This is the next major unknown — timing (card plays during a live loop) and deck speed are open design questions

2. **Shaken restrictions enforcement**
   - Design decision still open: what Shaken actually prevents (intended: cannot play Action cards during action window)
   - Composure penalty during Shaken (intended: 50% reduction) also not yet enforced
   - Status applies and expires correctly; restrictions are the follow-up

3. **Status effect icons + stack display on character UI**
   - Distinct icons per status, stack count on character canvas (musician + audience)
   - `SSoT_Status_Effects` should define icon/display contract before implementation

4. **Audience Feedback DoT**
   - Requires a Stress path on `AudienceCharacterBase` — does not exist
   - Explicitly deferred until audience pressure model expands

---

## 4. Current blockers / residual risks

### Open items (non-blocking)
- **Shaken restrictions:** unenforced. Status applies and expires; no gameplay gate checks it yet.
- **Audience Feedback DoT:** no Stress path on audience. Deferred explicitly.
- **Composure penalty during Shaken:** design intent only; not code-enforced.

### Residual risk B (retained)
Legacy `StatusType` enum coexists with the SO container. Both are active in places. New work goes through SO container only. Legacy calls (`ApplyStatus(StatusType.Breakdown, 1)` in `OnBreakdown`) are migration coexistence.

### Residual risk C (retained)
Phase machine bypass during `CompositionSession` is documented but not yet tested with a real composition card that has gameplay effects. This is the core of the composition testing work in §3.

---

## 5. Docs that must be edited next

After the next meaningful technical change, edit:
- the primary affected SSoT
- `CURRENT_STATE.md` if the active operational slice changed
- `changelog-ssot.md` if meaning/authority changed
- `coverage-matrix.md` only if the primary home changed

---

## 6. Working rule

`CURRENT_STATE.md` answers:
- what is active now
- what was just completed
- what comes next
- what is blocked
- which docs need editing next

It does **not** replace subsystem SSoTs.

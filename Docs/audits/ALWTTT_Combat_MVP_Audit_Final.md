# ALWTTT Combat MVP — Definitive Technical & Documentary Audit

**Date:** 2026-03-20  
**Scope:** Combat MVP branch closure audit  
**Evidence base:** All SSoT docs, governance docs, roadmap, and 40+ source files  
**Governance authority:** Multi-Project Documentation Governance System v0.4 → Documentation Update Loop Local Addendum

---

## 1) Combat MVP Reconstructed Scope

Based on the active governed docs (SSoT_Gig_Combat_Core, SSoT_Card_System, SSoT_Scoring_and_Meters, SSoT_Audience_and_Reactions, SSoT_Gig_Encounter, SSoT_Runtime_Flow) and the Roadmap_Combat_MVP planning doc, the Combat MVP slice is:

**One Gig encounter (multi-song), where a band plays cards (Action + Composition), generates music loop-by-loop, accumulates SongHype, converts it to Vibe on audience members at Song End, takes Stress pressure from audience actions, and wins by convincing all required audience members (Vibe ≥ VibeGoal) before running out of songs or (aspirationally) before Cohesion hits 0.**

The MVP runtime loop is:

```
Gig Setup
  → [PlayerTurn (action window + composition)
     → Performance (loops, LoopScore → SongHype)
     → Song End (Vibe resolution via SongHype01 + impressions)
     → Audience Turn (Stress pressure + abilities)
     → next song or gig end] × N songs
  → Win/Loss resolution
```

Key MVP features per docs: effects-first card model, Flow (amplifies momentum), Composure (absorbs Stress), Breakdown/Shaken mechanic, Inspiration economy, LoopScore → SongHype → Vibe conversion chain, audience intentions/abilities, win/loss resolution.

---

## 2) System-by-System Audit

### A. Cards / Card Execution

**Documented truth (SSoT_Card_System):** Effects-first model. `CardDefinition` → `CardPayload` (Action or Composition) → `List<CardEffectSpec>`. Built-in specs: `ApplyStatusEffectSpec`, `ModifyVibeSpec`, `ModifyStressSpec`, `DrawCardsSpec`.

**Code truth — confirmed from all files:**

- `CardDefinition.cs`: References `CardPayload`, derives `IsAction`/`IsComposition` from payload type. Performer rule, exhaust, targeting override — all present and functional.
- `CardEffectSpec.cs`: Abstract base, empty body — data-only as documented.
- `ApplyStatusEffectSpec.cs`: Fields: `status` (StatusEffectSO), `targetType`, `stacksDelta`, `delay`.
- `ModifyVibeSpec.cs`: Fields: `targetType`, `amount`.
- `ModifyStressSpec.cs`: Fields: `targetType`, `amount`.
- `DrawCardsSpec.cs`: Field: `count`. Data-only.
- `ActionCardPayload.cs`: Has `ActionTiming` and `Conditions`.
- `CompositionCardPayload.cs`: Has `PrimaryKind`, `TrackAction`, `PartAction`, `ModifierEffects`, `RequiresMusicianTarget`.

**Card execution pipeline — `CardBase.cs` (CRITICAL FILE):**

`CardBase.ExecuteEffects()` is a coroutine that iterates `payload.Effects` and handles each spec via type checks:

| Spec | Status | Runtime behavior |
|---|---|---|
| `ApplyStatusEffectSpec` | ✅ WORKING | Calls `trg.Statuses.Apply(ase.status, ase.stacksDelta)` — uses new SO-based `StatusEffectContainer` |
| `ModifyVibeSpec` | ✅ WORKING | Resolves audience targets, applies Flow→Vibe flat bonus, calls `audience.AudienceStats.AddVibe(finalDelta)` |
| `ModifyStressSpec` | ✅ WORKING (with caveats) | Absorbs Composure first via `musician.Statuses.TryGet(TempShieldTurn)`, then applies remainder — BUT bypasses Breakdown check (see §2E) |
| `DrawCardsSpec` | ❌ STUB ONLY | Logs warning, no-op. Not implemented. |

**Card routing — `HandController.cs`:**

- Action cards: routed through `heldCard.Use()` → `CardUseRoutine` → `ExecuteEffects`
- Composition cards: routed through `GigManager.TryPlayCompositionCard()` → `CompositionSession`
- Targeting: raycast + fixed-performer fallback. Validation only checks `ApplyStatusEffectSpec` for musician/audience single-target

**Card authoring — `CardEditorWindow.cs`, `CardEditorWindow_JsonImport.cs`:**

- JSON import pipeline is fully functional for all four effect specs
- Rejects legacy `statusActions` and `action.actions` formats
- Supports batch import
- Effects vocabulary in JSON: `ApplyStatusEffect`, `ModifyVibe`, `ModifyStress`, `DrawCards`

**Verdict:** Card data model and execution pipeline are implemented and operational for the three core specs. DrawCardsSpec is data-only stub. The card system is the most complete subsystem.

---

### B. Status Effects — Dual System Reality

**Documented truth (SSoT_Status_Effects):** StatusEffectSO-based system with catalogue lookup, primitive identity separation via `CharacterStatusId`, and themed variants. Stacking, duration, decay modes.

**Code truth — TWO PARALLEL SYSTEMS coexist:**

#### System 1: Legacy `StatusType` enum + `statusDict`

- **`StatusType.cs`**: Enum with None, Chill, Skeptical, Strength, Hooked, Breakdown, Heckled, Shaken, Dazzled, Dexterity, Blocked, Convinced, Tall
- **`CharacterStats.cs`**: Base class with `Dictionary<StatusType, StatusStats> statusDict`. Initializes all enum values. Has `TriggerAllStatus()`, `ClearStatus()`, per-type behaviors (Skeptical clears at turn, Breakdown triggers stun check).
- **`BandCharacterStats.cs`**: `ApplyStatus(StatusType, int)` modifies `statusDict`.
- **`AudienceCharacterStats.cs`**: Same `ApplyStatus(StatusType, int)`.

**Used by:** Audience event flags (Convinced, Tall, Blocked), Breakdown state on musicians, BlockStressAction (Chill), BlockVibeAction (Skeptical), legacy turn-trigger system (`TriggerAllStatus` called on `OnPlayerTurnStarted` for musicians and `OnEnemyTurnStarted` for audience).

#### System 2: New `CharacterStatusId` enum + `StatusEffectSO` + `StatusEffectContainer`

- **`CharacterStatusId.cs`**: CSO enum with numeric ranges: Offensive (100s), Defense (400s: TempShieldTurn=400, TempShieldPersistent=401), Control (500s), etc.
- **`StatusEffectSO.cs`**: ScriptableObject with `statusKey`, `effectId` (CharacterStatusId), `displayName`, StackMode, DecayMode, TickTiming, MaxStacks, DurationTurns. Validates against `CharacterStatusPrimitiveDatabaseSO`.
- **`StatusEffectCatalogueSO.cs`**: Catalogue with key-based and primitive-based lookup. Supports variants. Case-insensitive keys.
- **`StatusEffectInstance.cs`**: Runtime state: Stacks, RemainingTurns, IsActive.
- **`StatusEffectContainer.cs`**: Runtime container on each character. `Apply(StatusEffectSO, stacks)`, `TryGet(id)`, `GetStacks(id)`, `HasActive(id)`, `Tick(timing)`, `Clear(id)`. Full stacking policy, decay policy, consume-on-trigger support.
- **`CharacterBase.cs`**: `Statuses` property = `StatusEffectContainer`, created in `Awake()`. Has legacy stun sync: `IsStunned` reads `Statuses.HasActive(CharacterStatusId.DisableActions)` if container exists, falls back to legacy field.

**Used by:** CardBase.ExecuteEffects (ApplyStatusEffectSpec, Composure lookup via TempShieldTurn, Flow lookup via DamageUpFlat), GigManager.GetTotalFlowStacks(), GigManager.ResetSongScopedStatuses().

**MVP Mappings (hardcoded in code):**
- Flow = `CharacterStatusId.DamageUpFlat` (100)
- Composure = `CharacterStatusId.TempShieldTurn` (400)

#### Divergence: SSoT describes only System 2, code runs both

- **SSoT_Status_Effects** describes the SO-based model as "the" status system
- Code uses BOTH in parallel: legacy for event-flags/turn-triggers, new SO for card-authored effects
- The two systems do NOT communicate: applying Flow via SO container does not appear in `statusDict`, and applying Breakdown via legacy does not appear in `StatusEffectContainer`
- **Governance violation:** SSoT_Status_Effects claims authority over a unified model that doesn't exist yet

---

### C. Scoring and Meters

**Documented truth (SSoT_Scoring_and_Meters, SSoT_Gig_Combat_Core):** LoopScore → SongHype (modified by Flow) → SongHype01 → VibeDelta per audience at Song End.

**Code truth — fully implemented chain:**

| Layer | Implementation | File | Status |
|---|---|---|---|
| LoopScore | `LoopScoreCalculator.ComputeLoopScore()`: scores on active tracks, role presence, complexity, last-loop bonus | `LoopScoreCalculator.cs` | ✅ |
| LoopScore → SongHype delta | `LoopScoreCalculator.ComputeHypeDelta()`: piecewise mapping | `LoopScoreCalculator.cs` | ✅ |
| SongHype accumulation | `GigManager.AddSongHype()`: clamps to maxSongHype, fires event | `GigManager.cs` | ✅ |
| SongHype01 | `Mathf.Clamp01(_songHype / maxSongHype)` | `GigManager.cs` | ✅ |
| Song-end → Vibe | `GigManager.ComputeSongVibeDeltas()`: baseVibe = SongHype01 × maxVibeFromSongHype, adjusted by impression factor | `GigManager.cs` | ✅ |
| Vibe application | `GigManager.RunSongVibeResolution()` → `audience.AudienceStats.AddVibe()` | `GigManager.cs` | ✅ |
| Convinced check | `AudienceCharacterStats.AddVibe()`: triggers at CurrentVibe >= MaxVibe | `AudienceCharacterStats.cs` | ✅ |
| Audience impressions | `AudienceCharacterBase.ResolveLoopEffect()`: returns 0 always (stub) | `AudienceCharacterBase.cs` | ⚠️ Always neutral |

**Flow interaction with scoring:**

| Path | Code config | Default | Status |
|---|---|---|---|
| Flow → SongHype multiplier | `flowAffectsSongHype` | **false** | Exists but OFF |
| Flow → Vibe flat bonus (song resolution) | `flowAddsFlatVibeBonus` | **true** | ✅ ACTIVE |
| Flow → Vibe flat bonus (card ModifyVibe) | inline in CardBase.ExecuteEffects | **true** | ✅ ACTIVE |

**Divergence:** SSoT_Gig_Combat_Core §6.1 documents Flow as amplifying LoopScore→SongHype multiplicatively. The active behavior is Flow→Vibe flat bonus. The documented behavior exists in code but is disabled. Neither SSoT nor Roadmap documents the active behavior.

**Inspiration:** Initialized at gig start (`pd.CurrentInspiration = pd.InitialGigInspiration`). Per-loop inspiration in CompositionSession. Card spending/generation via `CardBase.SpendInspiration()` / `GenerateInspiration()`. Operational.

---

### D. Audience Behavior

**Documented truth (SSoT_Audience_and_Reactions):** Each audience member has Vibe, VibeGoal, Preferences, Abilities, Intention. Audience acts at song-end. Convinced when Vibe ≥ VibeGoal.

**Code truth:**

**Audience model:**
- `AudienceCharacterBase.cs`: Owns `AudienceCharacterStats` (stats), `NextAbility`, `ColumnIndex`, `IsBlocked`.
- `AudienceCharacterStats.cs`: `MaxVibe` (=VibeGoal), `CurrentVibe`, `IsConvinced`. `AddVibe()` triggers Convinced.
- `AudienceCharacterSimple.cs`: Empty subclass — adds nothing.

**Ability/intention system — WORKING:**
- `ShowNextAbility()`: Called on `OnPlayerTurnStarted`. Reads from `AudienceCharacterData.GetAbility(usedAbilityCount)`. Displays intent sprite and action value on canvas. Increments `usedAbilityCount`.
- `AbilityRoutine()`: Iterates `NextAbility.ActionList`, calls `ExecuteActionWithTiming()` per action. Handles stun skip, animation, delay.
- `ExecuteActionWithTiming()`: Resolves targets via `ResolveTargetsFor()`, dispatches to `CharacterActionProcessor.GetAction(action.CardActionType).DoAction(p)`.

**Target resolution for audience actions:**
- `Musician`: heuristic — lowest current Stress
- `RandomMusician`: random
- `AllMusicians`: full list
- `AudienceCharacter`: lowest-Vibe ally (excluding self)
- Self, Random, All — supported

**Concrete audience action classes (CharacterActionProcessor):**

| Action | ActionType | What it does | Composure-aware? | Breakdown-trigger? |
|---|---|---|---|---|
| `AddStressAction` | AddStress | `musicianStats.AddStress(amount)` | ❌ NO | ✅ YES (via AddStress) |
| `HealStressAction` | HealStress | `musicianStats.HealStress(amount)` | N/A | N/A |
| `AddVibeAction` | AddVibe | `audienceStats.AddVibe(amount)` — scales by musician stat if card context | N/A | N/A |
| `RemoveVibeAction` | RemoveVibe | `audienceStats.RemoveVibe(amount)` | N/A | N/A |
| `BlockStressAction` | BlockStress | `musicianStats.ApplyStatus(StatusType.Chill, amount)` (legacy) | N/A | N/A |
| `BlockVibeAction` | BlockVibe | `audienceStats.ApplyStatus(StatusType.Skeptical, amount)` (legacy) | N/A | N/A |
| `AudienceMoveToFrontAction` | MoveToFront | Repositions audience member to front of row | N/A | N/A |

**CRITICAL FINDING: `AddStressAction` calls `musicianStats.AddStress()` which triggers Breakdown check but does NOT absorb Composure.** This is the audience's primary pressure channel and it bypasses the defensive mechanic.

**Impressions:** `ResolveLoopEffect()` always returns 0. Preference-based loop impression is not implemented. The impression aggregation and factor calculation in `ComputeSongVibeDeltas()` works, but with all inputs at 0, the impression factor is always 1.0 (neutral).

---

### E. Stress Paths — The Central Unresolved Issue

There are **two independent Stress application paths** in the codebase, and **neither path does both Composure absorption AND Breakdown triggering:**

#### Path 1: Card effects (CardBase.ExecuteEffects → ModifyStressSpec)

```
incoming stress (positive)
  → read Composure stacks from Statuses.TryGet(TempShieldTurn)
  → absorb min(composure, incoming)
  → consume Composure stacks
  → apply remainder via musician.Stats.SetCurrentStress(before + remaining)
```

- ✅ Composure absorption: YES
- ❌ Breakdown trigger: NO (`SetCurrentStress` does not check threshold)
- Used by: card-authored ModifyStressSpec effects

#### Path 2: Audience actions (AddStressAction → musicianStats.AddStress)

```
incoming stress
  → musicianStats.AddStress(amount)
  → SetCurrentStress(CurrentStress + amount)
  → if (CurrentStress >= MaxStress && !IsBreakdown) → trigger OnBreakdown
```

- ❌ Composure absorption: NO
- ✅ Breakdown trigger: YES (AddStress checks threshold)
- Used by: audience AddStressAction, any other caller of `AddStress()`

#### What Breakdown does when triggered (MusicianBase.OnBreakdown):

```csharp
stats.ApplyStatus(StatusType.Breakdown, 1);  // legacy status
IsStunned = true;                             // skip next turn
```

**Missing from Breakdown implementation:**
- ❌ Cohesion −1 (documented in SSoT_Gig_Combat_Core §6.3)
- ❌ Stress reset to ceil(StressMax/2) (documented in SSoT_Gig_Combat_Core §6.3)
- ❌ Shaken state lasting until end of next Song (documented)
- ❌ Shaken restricting Action card play (documented)
- ❌ Shaken halving Composure grants (documented)

The Breakdown implementation is minimal: it applies a legacy status and sets `IsStunned`. The full Breakdown/Shaken mechanic described in the SSoT is not implemented.

---

### F. Gig / Encounter Runtime Loop

**Documented truth (SSoT_Runtime_Flow, SSoT_Gig_Encounter):** 7-phase model. GigManager orchestrates.

**Code truth — dual flow system:**

**Phase state machine (`GigManager.ExecuteGigPhase`):** Has `PrepareGig`, `PlayerTurn`, `SongPerformance`, `AudienceTurn`, `EndGig`. BUT line 736: `if (_session != null) return;` — **the phase machine is bypassed entirely when a CompositionSession is active.**

**CompositionSession-driven flow (CURRENT ACTIVE PATH):**
1. `StartCompositionSession()`: Creates session, resets SongHype and song-scoped statuses
2. Session builds parts, player composes, presses Play
3. Session plays loops → `OnCompositionLoopFinished` → `TriggerAudienceMicroReactions` (LoopScore → SongHype + impressions)
4. Session finishes parts → `OnCompositionPartFinished` → enriches with audience data
5. Session finishes song → `OnCompositionSongFinished` → stores `_lastSongFeedback`, increments song index
6. `OnCompositionSessionEnded` → sets `CurrentGigPhase = AudienceTurn`
7. `AudienceTurnRoutine` → `RunSongVibeResolution` (Vibe payout) → audience ability execution → back to PlayerTurn

**Legacy `SongPerformanceRoutine`:** Still exists, plays via `MidiMusicManager.Play(song)`, has `// TODO: Apply Vibe to enemies`. This is the OLD path, not used when CompositionSession is active.

**Victory:** `ResolveGigOutcomeAndEnd()` checks all audience members `IsConvinced`. Works.

**Loss via Cohesion:** `BandCohesion` is decremented only in `ReturnToMap(false)` from `CohesionPenaltyOnLoss`. **No mid-gig Cohesion check exists.** The documented failure condition (Cohesion ≤ 0 during gig from Breakdown events) is NOT implemented.

**Loss via songs exhausted:** When `IsGigComplete` and all songs played, if not all audience convinced, `LoseGig()` is called. This works.

---

### G. Composition / Runtime Bridge

**CompositionSession** manages build/play/loop cycle. **SongConfigBuilder** converts model to playback config. **MidiMusicManager** hosts playback. Loop/Part/Song feedback callbacks fire and GigManager consumes them.

This subsystem is functional and not a Combat MVP blocker. The only intersection with combat is through the feedback pipeline (LoopScore calculation, audience micro-reactions), which works.

---

## 3) Definitive Gap Matrix

### Legend
- ✅ = Confirmed working
- ⚠️ = Partially working or with caveats
- ❌ = Not implemented or broken
- ❓ = Cannot determine from shared files

| # | Subsystem / Mechanic | Documented? | Implemented? | Runtime-Validated? | Confidence | Issue | Primary Doc Home | Next Action |
|---|---|---|---|---|---|---|---|---|
| 1 | CardDefinition + Payload model | ✅ | ✅ | ✅ | HIGH | — | SSoT_Card_System | None |
| 2 | ApplyStatusEffectSpec execution | ✅ | ✅ | ✅ | HIGH | Uses SO container correctly | SSoT_Card_System | None |
| 3 | ModifyVibeSpec execution | ✅ | ✅ | ✅ | HIGH | Flow→Vibe bonus inline | SSoT_Card_System | None |
| 4 | ModifyStressSpec execution | ✅ | ⚠️ | ⚠️ | HIGH | Composure works, Breakdown bypassed | SSoT_Card_System | Fix Breakdown trigger |
| 5 | DrawCardsSpec execution | ✅ | ❌ | ❌ | CONFIRMED | Stub only, logs warning | SSoT_Card_System | Implement or scope-out |
| 6 | Composure absorbs Stress (card path) | ✅ | ✅ | ✅ | HIGH | Works in CardBase | SSoT_Gig_Combat_Core | None (card path) |
| 7 | Composure absorbs Stress (audience path) | ✅ | ❌ | ❌ | CONFIRMED | AddStressAction bypasses Composure | SSoT_Gig_Combat_Core | **Unify stress paths** |
| 8 | Breakdown trigger (card stress) | ✅ | ❌ | ❌ | CONFIRMED | CardBase calls SetCurrentStress not AddStress | SSoT_Gig_Combat_Core | **Fix: use AddStress or add check** |
| 9 | Breakdown trigger (audience stress) | ✅ | ✅ | ✅ | HIGH | AddStressAction → AddStress → OnBreakdown | SSoT_Gig_Combat_Core | None |
| 10 | Breakdown → Cohesion −1 | ✅ | ❌ | ❌ | CONFIRMED | OnBreakdown does not decrement Cohesion | SSoT_Gig_Combat_Core | Implement or scope-out |
| 11 | Breakdown → Stress reset to ceil(Max/2) | ✅ | ❌ | ❌ | CONFIRMED | Not in OnBreakdown | SSoT_Gig_Combat_Core | Implement or scope-out |
| 12 | Shaken state (full mechanic) | ✅ | ❌ | ❌ | CONFIRMED | No code evidence | SSoT_Gig_Combat_Core | Implement or scope-out |
| 13 | Cohesion mid-gig failure gate | ✅ | ❌ | ❌ | CONFIRMED | No mid-gig check; only at gig-end loss | SSoT_Gig_Encounter | Implement or scope-out |
| 14 | Flow → SongHype multiplier | ✅ | ✅ (OFF) | ❌ | MED | Code exists, disabled by default | SSoT_Gig_Combat_Core | Update doc |
| 15 | Flow → Vibe flat bonus | ❌ undocumented | ✅ | ✅ | HIGH | Active path, not in any SSoT | SSoT_Gig_Combat_Core | **Add to doc** |
| 16 | LoopScore → SongHype chain | ✅ | ✅ | ✅ | HIGH | — | SSoT_Scoring_and_Meters | None |
| 17 | SongHype → Vibe conversion | ✅ | ✅ | ✅ | HIGH | — | SSoT_Scoring_and_Meters | None |
| 18 | Audience impressions (preference-based) | ✅ | ❌ (always 0) | ❌ | CONFIRMED | ResolveLoopEffect returns 0 | SSoT_Audience_and_Reactions | Implement or scope-out |
| 19 | Audience Vibe / Convinced | ✅ | ✅ | ✅ | HIGH | — | SSoT_Audience_and_Reactions | None |
| 20 | Audience Stress pressure (AddStressAction) | ✅ | ✅ | ✅ | HIGH | Works via CharacterActionProcessor | SSoT_Audience_and_Reactions | None |
| 21 | Audience intention/telegraphing | ✅ | ✅ | ✅ | HIGH | ShowNextAbility shows sprite + value | SSoT_Audience_and_Reactions | None |
| 22 | Win condition (all convinced) | ✅ | ✅ | ✅ | HIGH | ResolveGigOutcomeAndEnd | SSoT_Gig_Encounter | None |
| 23 | Loss condition (songs exhausted) | ✅ | ✅ | ✅ | HIGH | IsGigComplete → LoseGig | SSoT_Gig_Encounter | None |
| 24 | Dual status system documented | ❌ | ✅ (both live) | ✅ | HIGH | Two systems coexist, undocumented split | SSoT_Status_Effects | **Document** |
| 25 | StatusEffectContainer (SO runtime) | ✅ | ✅ | ✅ | HIGH | Full Apply/TryGet/Tick/Clear | SSoT_Status_Effects | None |
| 26 | Phase state machine vs session | ⚠️ | ✅ (bypassed) | ✅ | MED | Phase machine dead when session active | SSoT_Runtime_Flow | Make explicit |
| 27 | Encounter setup / roster | ✅ | ✅ | ✅ | HIGH | — | SSoT_Gig_Encounter | None |
| 28 | Inspiration economy | ✅ | ✅ | ✅ | HIGH | Init, spend, generate all work | SSoT_Gig_Combat_Core | None |
| 29 | Song-scoped status reset | ✅ | ✅ | ✅ | HIGH | Flow + Composure cleared per song | SSoT_Gig_Combat_Core | None |

---

## 4) Divergences and Governance Issues

### D1. TWO STRESS PATHS — NEITHER COMPLETE (CRITICAL)

| Path | Composure absorption | Breakdown trigger | Used by |
|---|---|---|---|
| CardBase.ModifyStressSpec | ✅ YES | ❌ NO | Card effects |
| AddStressAction | ❌ NO | ✅ YES | Audience abilities |

Neither path implements the full documented Stress contract. The Roadmap explicitly recommends a single canonical `ApplyIncomingStressWithComposure(...)` helper. This is the most important implementation issue blocking MVP closure.

### D2. Status system dual model (SIGNIFICANT)

- SSoT_Status_Effects describes only the SO-based model
- Code runs two parallel systems: legacy `StatusType`/`statusDict` for event-flags, new `CharacterStatusId`/`StatusEffectContainer` for card-authored effects
- The two systems do not communicate
- **Action required:** SSoT_Status_Effects must acknowledge both models and explain the boundary

### D3. Flow documentation vs implementation (MODERATE)

- SSoT_Gig_Combat_Core §6.1: Flow amplifies LoopScore→SongHype multiplicatively
- Code: that path exists but is OFF (`flowAffectsSongHype = false`)
- Active path: Flow→Vibe flat bonus per stack (`flowAddsFlatVibeBonus = true`)
- The active path exists in TWO places: GigManager.RunSongVibeResolution AND CardBase.ModifyVibeSpec handler
- **Action required:** Update SSoT_Gig_Combat_Core to describe the actual active behavior

### D4. Breakdown/Shaken mechanic mostly unimplemented (MODERATE)

- SSoT_Gig_Combat_Core §6.3 documents a full mechanic: Breakdown triggers Cohesion−1, Stress reset, Shaken state with duration/restrictions
- Code: `OnBreakdown` only does `ApplyStatus(StatusType.Breakdown, 1)` + `IsStunned = true`
- Missing: Cohesion decrement, Stress reset, Shaken state, action card restriction, Composure penalty
- **Action required:** Either implement or explicitly scope out, then update SSoT

### D5. Cohesion as mid-gig failure gate — not implemented (MODERATE)

- SSoT_Gig_Encounter §7.3: `Cohesion <= 0` is the gig failure condition
- Code: `BandCohesion` only decremented at gig-end via `CohesionPenaltyOnLoss`
- No mid-gig check, no link from Breakdown to Cohesion
- **Action required:** Implement or scope out, update SSoT_Gig_Encounter

### D6. Phase state machine bypassed (LOW)

- SSoT_Runtime_Flow §4 describes a clean 7-phase model
- Code: `ExecuteGigPhase()` short-circuits when `_session != null`
- Not harmful but should be documented honestly

### D7. Roadmap is stale on Composure (LOW)

- Roadmap checklist marks `[ ] Composure absorbs positive Stress before application`
- This is partially done (card path). Roadmap should be updated.

### D8. Audience impressions always neutral (LOW)

- SSoT_Audience_and_Reactions §5–6 describes preference-based loop impressions
- `ResolveLoopEffect()` always returns 0
- The aggregation math works, but inputs are always neutral

### D9. BlockStressAction applies StatusType.Chill, not Composure (INFORMATIONAL)

- `BlockStressAction` applies `StatusType.Chill` (legacy system), not `CharacterStatusId.TempShieldTurn` (new system)
- This means "Block Stress" cards using the legacy action path do not grant Composure in the SO container that CardBase reads for absorption
- Cards authored with `ApplyStatusEffectSpec` referencing a TempShieldTurn SO DO correctly grant Composure
- This is another symptom of the dual-system issue

---

## 5) Closure Criteria

Derived from active SSoTs, Roadmap, and code evidence. Ordered by priority.

### Tier 1 — Must fix for honest MVP closure

| # | Criterion | Current status | Blocking issue |
|---|---|---|---|
| C1 | Single canonical Stress path with Composure + Breakdown | ❌ | Two paths, neither complete |
| C2 | Flow→Vibe documented as active MVP contract | ❌ | Active behavior undocumented |
| C3 | Dual status model acknowledged in SSoT | ❌ | SSoT claims unified model that doesn't exist |
| C4 | Breakdown/Shaken scope explicitly decided | ❌ | Docs claim full mechanic, code has minimal stub |

### Tier 2 — Should fix for confident MVP closure

| # | Criterion | Current status | Blocking issue |
|---|---|---|---|
| C5 | Cohesion mid-gig failure explicitly decided | ❌ | Documented but not implemented |
| C6 | DrawCardsSpec explicitly decided | ❌ | Stub only |
| C7 | Roadmap checklist updated to reality | ❌ | Stale on Composure, missing Flow→Vibe |

### Tier 3 — Already done

| # | Criterion | Status |
|---|---|---|
| C8 | ModifyVibeSpec end-to-end | ✅ |
| C9 | ModifyStressSpec end-to-end (data flow) | ✅ |
| C10 | ApplyStatusEffectSpec applies stacks | ✅ |
| C11 | LoopScore → SongHype → Vibe chain | ✅ |
| C12 | Audience abilities execute | ✅ |
| C13 | Audience Stress pressure exists | ✅ |
| C14 | Win/loss resolution | ✅ |
| C15 | Full loop playable | ✅ |
| C16 | Intention/telegraphing | ✅ |

---

## 6) Recommended Next Sequence

### Step 1: Create unified Stress application helper (CRITICAL — blocks C1)

**What:** Create a single method like `ApplyIncomingStress(MusicianBase musician, int amount)` that:
1. Reads Composure from `musician.Statuses.TryGet(TempShieldTurn)`
2. Absorbs Composure first
3. Applies remainder via `musician.Stats.AddStress(remaining)` (which triggers Breakdown check)

**Then:** Route both `CardBase.ExecuteEffects` (ModifyStressSpec positive path) and `AddStressAction.DoAction` through this helper.

**Files to modify:** Create helper on `MusicianBase` or a static utility. Modify `CardBase.cs` and `AddStressAction.cs`.

**Unblocks:** C1 (Stress path unification), fixes D1.

**Estimated effort:** Small. The logic already exists in CardBase — it just needs to be extracted and reused, and the final call changed from `SetCurrentStress` to `AddStress`.

### Step 2: Decide and implement Breakdown/Shaken scope (blocks C4)

**Decision needed:** Is the full Breakdown/Shaken mechanic (Cohesion−1, Stress reset, Shaken duration, action restriction, Composure penalty) in or out of MVP?

**If IN (recommended minimal version):**
- In `MusicianBase.OnBreakdown()`, add: `stats.SetCurrentStress(Mathf.CeilToInt(stats.MaxStress / 2f))` (Stress reset)
- Add `GameManager.PersistentGameplayData.BandCohesion -= 1` and check for gig failure
- Defer Shaken (duration, action restriction, Composure penalty) to post-MVP

**If OUT:**
- Update SSoT_Gig_Combat_Core §6.3 to mark full mechanic as post-MVP
- Keep the minimal stub (stun for one turn)

**Files:** `MusicianBase.cs`, possibly `GigManager.cs` for Cohesion check.
**Doc updates:** SSoT_Gig_Combat_Core §6.3, SSoT_Gig_Encounter §7.3.

### Step 3: Document Flow→Vibe as active MVP contract (blocks C2)

**Change:** SSoT_Gig_Combat_Core §6.1:
- State that Flow's active MVP behavior is: each Flow stack adds a flat Vibe bonus to any positive Vibe gain (both card-based and song-resolution)
- Mark Flow→SongHype multiplier as available but inactive (legacy toggle)
- Note the config fields: `flowAddsFlatVibeBonus`, `flowVibeFlatBonusPerStack`

**Files:** `SSoT_Gig_Combat_Core.md`
**Change type:** Semantic

### Step 4: Document dual status system (blocks C3)

**Change:** SSoT_Status_Effects §2–3:
- Acknowledge two coexisting runtime layers:
  - Legacy `StatusType`/`statusDict`: used for game-event flags (Convinced, Tall, Blocked, Breakdown, Skeptical, Chill) and turn-trigger behaviors
  - New `CharacterStatusId`/`StatusEffectSO`/`StatusEffectContainer`: used for card-authored gameplay statuses (Flow=DamageUpFlat, Composure=TempShieldTurn)
- State that cards always use the new system via `ApplyStatusEffectSpec`
- State that audience actions and legacy event handling use the old system
- Mark full unification as a future goal, not current truth

**Files:** `SSoT_Status_Effects.md`
**Change type:** Semantic + authority

### Step 5: Decide Cohesion mid-gig failure scope (blocks C5)

**If Breakdown→Cohesion−1 is implemented in Step 2:**
- Add a mid-gig Cohesion check after each AudienceTurn or after each Breakdown
- Wire it to `LoseGig()` or equivalent

**If scoped out:**
- Update SSoT_Gig_Encounter §7.3 to state that MVP loss is songs-exhausted only
- Mark Cohesion as gig-end penalty only

### Step 6: Scope-decide DrawCardsSpec (blocks C6)

**If implementing:** Add `DeckManager.Instance?.DrawCards(draw.count)` in CardBase's DrawCardsSpec handler. DeckManager.DrawCards likely already exists.

**If scoping out:** Update SSoT_Card_System §6.2 to note DrawCardsSpec as post-MVP.

### Step 7: Update Roadmap and CURRENT_STATE (blocks C7)

After steps 1–6:
- Update Roadmap checklist to reflect actual completion state
- Update CURRENT_STATE §1 to accurately describe validated slice
- Update changelog-ssot for all semantic changes

---

## 7) Documentation Update Map

| Document | What to update | Change type | Priority |
|---|---|---|---|
| **SSoT_Gig_Combat_Core** | §6.1: Flow→Vibe as active MVP path, Flow→SongHype as legacy toggle. §6.2: Composure works on card path, audience path needs unification. §6.3: Breakdown/Shaken — either full specification or explicit MVP scoping. §11: Remove stale "known validation gaps" — Composure card path now works. | Semantic | **HIGHEST** |
| **SSoT_Status_Effects** | §2–4: Document dual system (legacy StatusType + new SO/CSO). Explain boundary: cards use new, events use legacy. Mark unification as future goal. | Semantic + Authority | **HIGH** |
| **Roadmap_Combat_MVP** | Update checklist: Composure (card) ✅. Add: Breakdown bypass fix, stress unification, Flow→Vibe documentation. Mark Flow→SongHype as legacy. | Planning | **HIGH** |
| **CURRENT_STATE** | §1: Specify which effects confirmed (Vibe ✅, Stress ✅, Status ✅, DrawCards ❌). Add: dual stress path issue, Breakdown bypass, dual status model as known gaps. §2: Record this audit as completed work. §3: Set next work = Steps 1–6 above. | Operational | **HIGH** |
| **SSoT_Runtime_Flow** | §4: State that phase machine is bypassed during CompositionSession; describe callback-driven flow as current truth. | Semantic | MED |
| **SSoT_Gig_Encounter** | §7.3: If Cohesion mid-gig loss not implemented, mark as post-MVP. Clarify that current loss = songs exhausted without convincing. | Semantic | MED |
| **SSoT_Scoring_and_Meters** | §7.1: Note Flow's current active scoring path is Vibe bonus, not SongHype multiplier. | Semantic | LOW |
| **SSoT_Card_System** | §6.2: Note DrawCardsSpec is stub-only. | Semantic | LOW |
| **SSoT_Audience_and_Reactions** | §5: Note that ResolveLoopEffect is stub (always 0). Preference-based impressions are post-MVP. | Semantic | LOW |
| **coverage-matrix** | No authority ownership changes needed unless status system primary home changes. | — | LOW |
| **SSoT_INDEX** | No structural changes needed. | — | NONE |
| **changelog-ssot** | Record all semantic changes from above. | Lifecycle | After all edits |

---

## 8) Validation Checklist for Runtime Confirmation

Once Steps 1–2 are implemented, the following should be manually validated in play mode:

- [ ] Play an Action card with ModifyStressSpec (positive) → Composure absorbs first, remainder becomes Stress
- [ ] Stress exceeding MaxStress after Composure → triggers Breakdown (musician stunned)
- [ ] Audience turn → AddStressAction → Composure absorbs first, remainder becomes Stress, Breakdown triggers if threshold crossed
- [ ] Play an Action card with ModifyVibeSpec → Vibe increases on target audience member(s), Flow bonus visible if Flow stacks present
- [ ] Play an Action card with ApplyStatusEffectSpec → status stacks appear on target
- [ ] Complete a song → SongHype01 converts to Vibe per audience member, Flow bonus applied
- [ ] Audience turn → abilities execute with correct targeting and timing
- [ ] All audience members convinced → WinGig fires
- [ ] All songs exhausted without convincing → LoseGig fires
- [ ] Song-scoped statuses (Flow, Composure) reset between songs
- [ ] If Breakdown→Cohesion implemented: Cohesion reaches 0 mid-gig → gig fails

---

## 9) Files Analyzed

### Governance / Index
- `SSoT_INDEX.md`, `SSoT_CONTRACTS.md`, `CURRENT_STATE.md`, `coverage-matrix.md`
- `MultiProject_Documentation_Governance_System_v0_4.md`, `Documentation_Update_Loop_Local_Addendum_v0_4.md`

### SSoT Documents
- `SSoT_Gig_Combat_Core.md`, `SSoT_Card_System.md`, `SSoT_Card_Authoring_Contracts.md`
- `SSoT_Audience_and_Reactions.md`, `SSoT_Status_Effects.md`, `SSoT_Scoring_and_Meters.md`
- `SSoT_Gig_Encounter.md`, `SSoT_Runtime_Flow.md`
- `SSoT_Runtime_CompositionSession_Integration.md`, `SSoT_ALWTTT_MidiGenPlay_Boundary.md`

### Planning
- `Roadmap_Combat_MVP.md`

### Code — Card System
- `CardDefinition.cs`, `CardPayload.cs` (via ActionCardPayload/CompositionCardPayload)
- `ActionCardPayload.cs`, `CompositionCardPayload.cs`
- `CardEffectSpec.cs`, `ApplyStatusEffectSpec.cs`, `ModifyVibeSpec.cs`, `ModifyStressSpec.cs`, `DrawCardsSpec.cs`
- `CardBase.cs` (effect execution pipeline)
- `CardEditorWindow.cs`, `CardEditorWindow_JsonImport.cs`

### Code — Character / Stats
- `CharacterBase.cs`, `CharacterStats.cs`, `StatusStats.cs`
- `BandCharacterStats.cs`, `MusicianBase.cs`
- `AudienceCharacterStats.cs`, `AudienceCharacterBase.cs`, `AudienceCharacterSimple.cs`

### Code — Status System
- `StatusType.cs` (legacy enum), `CharacterStatusId.cs` (CSO enum)
- `StatusEffectSO.cs`, `StatusEffectCatalogueSO.cs`
- `StatusEffectInstance.cs`, `StatusEffectContainer.cs`

### Code — Actions (CharacterActionProcessor)
- `CharacterActionProcessor.cs`, `CharacterActionData.cs`
- `AddStressAction.cs`, `HealStressAction.cs`
- `AddVibeAction.cs`, `RemoveVibeAction.cs`
- `BlockStressAction.cs`, `BlockVibeAction.cs`
- `AudienceMoveToFrontAction.cs`

### Code — Runtime / Encounter
- `GigManager.cs`, `GigRunContext.cs`, `GigEncounter.cs`, `GigEncounterSO.cs`
- `CompositionSession.cs`, `SongConfigBuilder.cs`, `MidiMusicManager.cs`
- `LoopScoreCalculator.cs`
- `HandController.cs`
- `GigSetupConfigData.cs`, `AudienceIntentionData.cs`

---

## 10) Summary

**What works today (confirmed with code evidence):**
- Card play pipeline end-to-end (action + composition routing)
- ModifyVibeSpec, ModifyStressSpec, ApplyStatusEffectSpec execution
- Composure absorption on the card-effect stress path
- LoopScore → SongHype → SongHype01 → Vibe conversion chain
- Song-end Vibe resolution with Flow→Vibe bonus
- Audience ability execution with intention telegraphing
- Audience Stress pressure via AddStressAction (but without Composure)
- Win (all convinced) and loss (songs exhausted) resolution
- Song-scoped status reset (Flow, Composure)
- SO-based status container (StatusEffectContainer) fully functional
- Card authoring pipeline (editor + JSON import)

**What doesn't work or is missing:**
- **Unified Stress path** — the single most important gap
- DrawCardsSpec — stub only
- Full Breakdown/Shaken mechanic — minimal stub
- Cohesion as mid-gig failure gate — not implemented
- Preference-based audience impressions — always neutral
- Flow→Vibe not documented in any SSoT
- Dual status model not documented

**Honest assessment:** The Combat MVP is approximately **75–80% functional**. The core loop works. Cards work. Scoring works. Audience acts. The missing piece is stress path unification — a targeted fix that unblocks honest closure. The Breakdown/Shaken/Cohesion decisions are scope decisions, not bugs. Once the stress path is unified and the documentation catches up to reality, the MVP can be called closed with a clear and honest boundary.

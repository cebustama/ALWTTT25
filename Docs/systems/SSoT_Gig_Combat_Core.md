# SSoT_Gig_Combat_Core — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Current ALWTTT gig/combat economy contract for the MVP line  
**Owns:** combat phases, resources, failure/win logic, time scales, conversion hooks, combat-facing status roles  
**Does not own:** detailed card data model (`SSoT_Card_System.md`), authoring/import rules (`SSoT_Card_Authoring_Contracts.md`), package-side MidiGenPlay internals

---

## 1. Purpose

This document is the primary gameplay authority for **how a Gig works as a combat encounter** in ALWTTT.

It replaces the previous split authority between:
- `reference/Gig_Combat.md`
- `canon/SSoT_Combat.md`
- scattered backlog notes about SongHype / Vibe / Flow / Composure

`CURRENT_STATE.md` may record implementation gaps, but the structural combat contract lives here.

---

## 2. Core fantasy

- Combat is **Band vs Audience Members**.
- The room is the opponent, not a traditional enemy lineup.
- The player creates musical momentum loop by loop.
- Audience response converts that performance into pressure and progress.
- The player wins by **convincing** the audience, not by killing enemies.

---

## 3. Canonical time scales

ALWTTT combat operates across five nested scales.

### 3.1 Loop
The smallest evaluation unit.

Each loop may:
- evaluate musical output into **LoopScore**
- convert LoopScore into **ΔSongHype**
- generate **Impression** per audience member
- generate **Inspiration** from the active musical context

### 3.2 Part
A musical block such as Intro / Verse / Chorus.

A Part:
- is configured during composition
- may loop multiple times
- is the immediate musical context for loop-level evaluation

### 3.3 Song
A song is a sequence of Parts.

At **Song End**:
- SongHype + audience impressions convert into **VibeDelta**
- audience executes its action phase
- song-scoped resources and statuses prepare for reset on the next Song

### 3.4 Gig
A Gig is the encounter unit: a sequence of Songs with shared band/audience stakes.

### 3.5 Run
Long-term progression sits outside this SSoT.

---

## 4. Core combat state

### 4.1 Gig / band-level
| Variable | Meaning |
|---|---|
| `Cohesion` | Band-wide durability. Gig fails at 0. |
| `GigScore` | Aggregate encounter score, derived from song outcomes. |
| `SongsRemaining` | Encounter pacing / remaining structure. |

### 4.2 Song-level
| Variable | Meaning |
|---|---|
| `Inspiration` | Song-scoped tactical resource for composition spending. |
| `SongHype` | Current song quality / momentum meter. |
| `SongHype01` | Normalized SongHype used for conversion. |
| `PartIndex` | Current part in the song. |
| `LoopIndex` | Current loop inside the active part. |
| `Song/Band statuses` | Song-scoped stackables such as Flow. |

### 4.3 Musician-level
| Variable | Meaning |
|---|---|
| `CHR` / `TCH` / `EMT` | Musician-facing stats. |
| `Stress` | Pressure meter. |
| `StressMax` | Threshold for Breakdown. |
| `BreakdownState` | `None` or `Shaken` in the MVP contract. |
| `Musician statuses` | Stackable statuses such as Composure. |

### 4.4 Audience-level
| Variable | Meaning |
|---|---|
| `Vibe` | Remaining resistance pool. Starts full at `MaxVibe` and is **depleted** by incoming Vibe (S5e inversion). |
| `MaxVibe` | Resistance pool size ("HP"). `VibeGoal` was retired into `MaxVibe` at S5e. |
| `Preferences` | Reaction bias by card/performance style. |
| `Abilities` | Telegraphed audience actions / pressure patterns. |
| `Audience statuses` | Optional / future-capable, not required for baseline MVP. |

---

## 5. Primary resources and meters

### 5.1 Inspiration
**Owner:** Band, scoped per Song.

Rules:
- resets at Song start to a base value
- increases from loop-level musical structure and composition contributions
- is spent primarily on **Composition cards**

Design role:
- governs tempo and budget of composition decisions
- lets musical structure feed back into tactical capacity

Per-loop inspiration gain is wired post-M4.6F-3 via `pd.InspirationPerLoop` (default-sourced from `GigFlowSettingsSO.DefaultInspirationPerLoop` at `ApplyRunConfig`). Consumed by the `LoopFinished` subscriber in `GigManager.OnCompositionLoopFinished`, which calls `_session.AddCurrentInspiration(inspN)`. The canonical mutator clamps to `pd.MaxInspiration` and mirrors to `pd.CurrentInspiration`, so PD and `CompositionSession._currentInspiration` stay in sync after every gain. Per-loop card draw is wired in the same hook via `flow.DrawPerLoop` (new field on `GigFlowSettingsSO`); `DeckManager.DrawCards` clamps to `MaxCardsOnHand` internally.

### 5.2 SongHype
**Owner:** Song.

Rules:
- changes loop by loop
- is derived from LoopScore conversion
- measures structural musical strength, not audience taste directly

**SFX→FlatVibe hook (§5.3.5).** On each upward stage crossing,
`FireSongHypeStage` invokes `ApplySfxBonusVibe(stage)` immediately after
`BackgroundContainer.ActivateSFX`. The per-stage bonus value (configured
on `GigPresentationSO.sfxBonusVibeStage{1,2,3}`, defaults 3/6/10) is
applied per-audience-member through `AudienceCharacterStats.ApplyIncomingVibe`,
so Indifference still blocks per-member (consistent with the Indifference
invariant in §5.3). A single aggregate "+N Vibe!" floating text spawns at
`GigManager.sfxBonusVibeTextSpawnRoot` (with first-musician-TextSpawnRoot
fallback). The bonus is FLAT — it bypasses the Flow multiplier; "post-Flow"
in the §5.3.5 spec refers to bonus magnitude not temporal ordering.

### 5.3 Vibe
**Owner:** each audience member.

Rules:
- Vibe gain happens at Song End
- VibeDelta derives from:
  - `SongHype01`
  - audience-specific impression data accumulated across loops
- an audience member is **Convinced** when `Vibe <= 0` *(S5e inversion, corrected here 2026-07-31, D-S5e-DOC-D: Vibe is an enemy-HP-style resistance pool starting at `MaxVibe`, depleted by incoming Vibe; `VibeGoal` retired into `MaxVibe`; code: `AudienceCharacterStats.CheckConvincedThreshold`)*

### 5.4 Stress
**Owner:** each musician.

Rules:
- Stress is the main incoming pressure channel
- positive Stress is absorbed by **Composure** before being applied
- Breakdown triggers when `Stress >= StressMax` **after** Composure absorption

**Implementation (unified 2026-04-26):** all positive incoming Stress — whether from card effects (`ModifyStressSpec`), audience actions (`AddStressAction`), or DoT ticks (`Feedback`) — routes through `BandCharacterStats.ApplyIncomingStressWithComposure(StatusEffectContainer, int, float)`. This single entry point handles: Composure absorption → Exposed amplification → `AddStress` remainder → Breakdown threshold check.

### 5.5 GigScore
**Owner:** Gig.

Current contract:
- GigScore is an encounter-level aggregate derived from song outcomes
- MVP-friendly default is the sum of song-level results such as final SongHype
- exact reward interpretation belongs to encounter/progression tuning, not this document

---

## 6. Combat-facing status roles

This document defines **combat meaning**, not the full status ontology.

### 6.1 Flow
**Scope:** Song/Band  
**Reset:** resets each Song (via explicit GigManager song-end reset, not tick decay)  
**Combat meaning:** amplifies positive Vibe gains. Bifurcated by card domain (M4.2, 2026-04-28).

**Action cards** — flat bonus using the **performer's individual** Flow stacks:
```text
finalVibeΔ = baseΔ + performerFlowStacks × flowActionVibeBonusPerStack
```

**Composition cards** — multiplier using the **band-wide** Flow stacks:
```text
finalVibeΔ = round(baseΔ × (1 + bandFlowStacks × flowVibeMultiplier))
```

**Song End conversion** — multiplier using band-wide Flow stacks (read before song-end reset):
```text
VibeDelta_i = round(baseVibe × impressionFactor × (1 + bandFlowStacks × flowVibeMultiplier))
```

Initial tuning: `flowActionVibeBonusPerStack = 1`, `flowVibeMultiplier = 0.08f`.

Rules:
- Flow interacts with the **Vibe layer**, not the SongHype layer
- the Flow → SongHype multiplicative path (documented pre-M4.2, never active in runtime) has been **retired and removed from code** as of M4.2
- Action-card path uses per-performer stacks; Composition-card and Song End paths use band-wide aggregate stacks
- future penalties for mistakes may interact with Flow, but that is not part of the baseline MVP contract

### 6.2 Composure
**Scope:** Musician  
**Reset:** clears at the start of each Player Turn (`PlayerTurnStart` tick in `GigManager.OnPlayerTurnStarted`)  
**Combat meaning:** absorbs incoming positive Stress before Stress is applied.

Conceptually:

```text
incomingStress -> consume Composure first -> apply remainder to Stress
```

Rules:
- Composure is a defensive buffer, not permanent healing
- Composure clears every Player Turn, not every Song — this is more frequent than song-scoped reset
- Composure absorbs audience pressure (via `AddStressAction`) as of M4.1 (2026-04-26) — previously only card-path stress was absorbed
- when a musician is in `Shaken`, new Composure granted to that musician is reduced by 50% (round down) — **design intent; not yet enforced in runtime**

### 6.3 Breakdown / Shaken

Breakdown is not just flavor; it is a combat-visible threshold event.

**Trigger:**
- when `Stress >= StressMax` after Composure absorption

**Immediate MVP consequences (in order):**
1. `Cohesion − 1`
2. If `Cohesion <= 0` after step 1: call `GigManager.LoseGig()` immediately — **steps 3–4 are skipped**
3. Apply `Shaken` status (1 stack via `StatusEffectCatalogueSO` key `"shaken"`)
4. Reset `Stress = floor(StressMax * breakdownStressResetFraction)` — default fraction is `0.5`, configured on `MeterTuningSO.breakdownStressResetFraction` (M4.6F-2; previously authored on `GigManager` directly).

**Shaken MVP runtime behavior:**
- SO config: Replace, MaxStacks=1, LinearStacks, `AudienceTurnStart` tick
- Duration: applied at Audience Turn of Song N → expires at the **start** of the Audience Turn of Song N+1
- Active through: rest of Audience Turn N, Player Turn N+1 (action window), Composition N+1, Performance N+1, Song End N+1
- This is one complete song cycle from the musician's next action window through the following song's end

**Shaken gameplay restrictions (design intent — not yet enforced in runtime):**
- the affected musician cannot play **Action cards** in the Between-Songs window while Shaken
- Composure granted to that musician is reduced by 50% (round down)
- these restrictions are pending a follow-up design/implementation pass

### 6.4 Exposed
**Scope:** Musician  
**Combat meaning:** amplifies incoming Stress. Each Exposed stack adds `0.25` to the stress multiplier in `ApplyIncomingStressWithComposure` (`_exposedMultiplierPerStack = 0.25f` on `BandCharacterStats`).

Rules:
- Exposed applies to musicians only
- there is no Stress path on `AudienceCharacterBase`; Exposed has no audience equivalent in MVP

### 6.5 Feedback DoT
**Scope:** Musician (MVP); Audience deferred  
**Combat meaning:** per-turn stress damage applied during `AudienceTurnRoutine` in `GigManager`.

Rules:
- each Feedback stack applies 1 incoming stress per tick, routed through `m.Stats.ApplyIncomingStressWithComposure`
- applies to musicians only in current implementation
- audience Feedback DoT requires a Stress path on `AudienceCharacterBase`, which does not exist — explicitly deferred

---

## 7. Canonical combat phase flow

### Phase 0 — Gig setup
- load band roster, audience roster, and encounter config
- initialize gig-level state such as Cohesion and score tracking
- prepare initial deck/hand state as required by the runtime flow

### Phase 1 — Between-Songs Action Window
- player may play **Action cards** subject to timing rules
- cleanup / prep decisions occur here
- musician restrictions such as `Shaken` apply here

### Phase 2 — Composition
- player plays **Composition cards**
- composition choices define or modify the musical structure for the coming performance
- composition may also apply immediate systemic effects when authored to do so

### Phase 3 — Performance
- the song runs through one or more loops
- loop-level evaluation updates SongHype, impression, and related song-scoped meters
- Inspiration generation hooks live here

### Phase 4 — Song End conversion
- accumulated loop outcome converts into audience-facing progress
- update `Vibe`, `Convinced`, and song-result summary values
- update GigScore trackers as needed

### Phase 5 — Audience Turn
- each audience member executes one telegraphed ability/action pattern
- audience pressure is applied primarily through Stress and disruption
- check victory/failure conditions before advancing

---

## 8. Conversion hooks (thin contract)

This SSoT owns the structural hook points.
Detailed tuning can move into a future scoring SSoT without changing combat authority.

### 8.1 Loop -> SongHype
- LoopScore is converted into a base SongHype delta via `LoopScoreCalculator.ComputeHypeDelta` using Inspector-tuneable `HypeThresholds`
- LoopScore uses adaptive role-budget scoring: the 12-point role budget is distributed proportionally based on either distinct roles filled (RoleNormalization mode) or musicians actively playing (MusicianParticipation mode), selectable via `LoopScoringConfig.mode`
- `possibleRoleCount` and `totalMusicians` are auto-detected at gig start from the band's deck composition cards and roster
- Flow no longer modifies this delta (retired M4.2). Flow interacts with the Vibe layer instead (see §6.1)

### 8.2 Song End -> Vibe
- SongHype01 + audience impression aggregate produce VibeDelta
- VibeDelta is applied per audience member

### 8.3 Song/Gig -> rewards and progression
- GigScore aggregates song-level outcome for reward/progression purposes
- exact reward economy is outside this SSoT

---

## 9. Card roles inside combat

This combat SSoT does **not** own the full card data model.
It owns only the card roles inside the combat loop.

### 9.1 Action cards
- played in action windows
- primarily express tactical control, recovery, crowd interaction, and immediate combat-state changes

### 9.2 Composition cards
- played during composition
- primarily express musical structure and future loop shaping
- may also carry immediate systemic card effects in the MVP contract

Per-musician play frequency for both roles is governed by the per-turn play
economy (§14): 1 Action + 1 Composition per musician per period.

The canonical card model and payload semantics live in `SSoT_Card_System.md`.

---

## 10. Explicit exclusions for the baseline MVP

The following are **not** part of the governed MVP combat core unless explicitly promoted later:
- Tall / Blocking / row-based tactical positioning
- advanced movement vocabulary
- special multi-bar audience boss mechanics as baseline rules
- deep reward/progression balance
- package-internal MIDI composition behavior

These can exist as planning/reference material without overriding this SSoT.

---

## 11. Implementation status (Phase 4 complete — 2026-03-23)

| Feature | Status |
|---|---|
| Composure absorption (positive Stress) | ✅ Validated (B1/B2) |
| Composure clear at PlayerTurnStart | ✅ Validated (B6) |
| Flow stacks boost Vibe per card play | ✅ Validated (B3) |
| Song-end Flow + Composure reset | ✅ Validated (B7) |
| Breakdown → Cohesion−1 + Stress reset + Shaken application | ✅ Implemented (Decision C) |
| LoseGig on Cohesion ≤ 0 | ✅ Implemented (Decision D) |
| Exposed stress multiplier on musicians | ✅ Implemented (Decision E) |
| Feedback DoT on musicians (AudienceTurnRoutine) | ✅ Implemented (Decision E) |
| Audience Feedback DoT | ⛔ Deferred — no Stress path on audience |
| Shaken gameplay restrictions enforcement | ⬜ Pending design decision |
| Composure penalty during Shaken | ⬜ Pending — design intent only |
| Flow bifurcation (flat Action / mult Composition+SongEnd) | ✅ Implemented (M4.2) |
| Per-performer Flow on Action cards | ✅ Implemented (M4.2) |
| Adaptive LoopScore (role-budget normalization) | ✅ Implemented (M4.2) |
| Flow → SongHype path | ❌ Retired and removed (M4.2) |

---

## 12. Configuration architecture (M4.6F-2)

`GigManager` is the runtime orchestrator for combat; it does not own gameplay tuning, presentation pacing, or dev toggles as inline-serialized fields. As of M4.6F-2 those values live on four ScriptableObject assets that GigManager references:

| SO | Concerns |
|---|---|
| `GigFlowSettingsSO` | JamRules, Action card gating, Gig End behavior, setup-screen defaults |
| `MeterTuningSO` | SongHype caps/seed, Vibe/Hype balance, Flow→Vibe (bifurcated MVP), `LoopScoringConfig`, `HypeThresholds`, `breakdownStressResetFraction` |
| `GigPresentationSO` | Audience beat curve/threshold, idle BPM, sequence pacing values, SongHype bar visibility (`showSongHypeBar`, S5f/#6a — OFF hides the bar + the C1 "L + SFX = N" readout; SongHype accrual, stage SFX, and song-end Vibe conversion are unaffected) |
| `GigDevSettingsSO` | Inspector-time toggles only: `useLogs`, `useCompositionLogs`, `debugSongHype`, `debugInstrumentPicker`, `debugMusicianVolume` |

Scene-instance references (cameras, hand, composition UI, position lists, scene changer, MidiGenPlayConfig boundary, songHypeDebugSlider, background container) remain inline-serialized on `GigManager` — they cannot be assets.

Façade properties on `GigManager` (`FlowActionFlatBonus`, `FlowActionVibeBonusPerStack`, `FlowVibeMultiplier`, `BreakdownStressResetFraction`) are preserved for callers written before F-2 and now delegate to `MeterTuningSO`.

> **Audio-mix note (M-AUDIO-MIX, 2026-06-15):** `GigManager` also carries an `audioMix` (`AudioMixSettingsSO`) inline-serialized ref and the audio-mix load/re-apply wiring (`ApplyPersistedAudioMix`, `ReapplyMusicianMix`, `ComputeEffectiveMusicianVolume01`, and the `#if ALWTTT_DEV` `DevSet…` audio wrappers). These are **governed by `SSoT_Audio.md`**, not here — locality is on the audio SSoT; cross-referenced from this section only because the wiring physically lives on `GigManager.cs`. `GameplayData.globalMusicVolume01` was removed in the same batch (global music migrated to `AudioMixSettingsSO`).

---

## 13. Launch contract (§5.3.5 / D-FAST-1=C)

A gig is initiated by exactly one code path: `GigLauncher.Launch` (static
service under `Assets/Scripts/Managers/GigLauncher.cs`). Three callers
exist or are planned:

1. **Manual GigSetup** — `GigSetupController.OnStartPressed` builds a
   `RunConfig` from picker UI state and dispatches.
2. **Main Menu auto-launch** — `MainMenuController.OnStartPressed` reads
   `GigDevSettingsSO.AutoStartFromDefaults` and a wired
   `DemoLaunchConfigSO`, converts the SO to a `RunConfig` via
   `DemoLaunchConfigSO.ToRunConfig`, and dispatches. Bypasses GigSetup
   entirely for the demo cut (single fade cycle MainMenu → Gig).
3. **LadderRunner** (post-§5.3.5) — multi-encounter ladder mode that
   dequeues per-encounter launch configs and dispatches one at a time,
   passing `bandRoster: null` so the band carries over between
   encounters.

GigLauncher's responsibilities (atomic):
- Apply `bandRoster` to `PersistentGameplayData.SetBandRoster` (skipped
  if null/empty — supports inter-encounter band carry-over).
- Ensure `GigRunContext` singleton; call `BeginRun(runConfig)`.
- Call `PersistentGameplayData.ApplyRunConfig`.
- Set `PersistentGameplayData.IsFinalEncounter = isFinalEncounter` (B3-
  demo-polish A6 hack carrier; removed when meta-progression sectors land).
- Navigate via `SceneChanger.OpenGigScene`.

Failure mode: GigLauncher returns `bool`. Callers treat `false` as
"launch did not happen" and decide whether to fall through (e.g.
MainMenuController falls through to manual GigSetup on auto-launch
failure).

`SSoT_Scoring_and_Meters` retains semantic authority for the meter-stack contract; this section governs only where the values are authored.

---

## 14. Per-turn play economy (ECON-1)

Batch ECON-1 (2026-07-07). Decisions D-ECON-1..5 locked 2026-07-06.

### 14.1 Rule
Each musician may play at most **1 Action card and 1 Composition card per
period**. The two pools are independent (consuming one never gates the other).

### 14.2 Period definition
A **period** is either:
- the pre-song `PlayerTurn` action/composition window (counts as ONE period), or
- each individual performance loop.

D-ECON-4=A: the limit is a strict 1 in ALL periods — the pre-song window gets
no larger allowance. Consequence: a musician cannot both anchor a song
pre-song (e.g. Wormus) and add a second composition card (e.g. Singing Field)
before Play; the second card enters as a mid-song add and is audible from the
loop after its drop (≥2). Under playtest observation; the fallback
(pre-song = 2 composition plays) is a one-line config change.

### 14.3 Reset seams (code truth)
Budgets refill for every live musician at three seams in `GigManager`:
- **Seam A** — `GigPhase.PlayerTurn` case, immediately before
  `OnPlayerTurnStarted` fires (gig start + every between-songs turn).
- **Seam B** — `OnPlayPressed()` (opens the loop-1 performance period).
  NOTE: the `GigPhase.SongPerformance` phase case is NOT a live seam — it is
  bypassed while `_session != null` (ExecuteGigPhase TEMP guard); the plan's
  original anchor was corrected during implementation.
- **Seam C** — `OnCompositionLoopFinished`, placed BEFORE the F-3 draw/insp
  early-return so the per-loop refill is config-independent.
Resets are idempotent refills; overlapping seams are harmless.

### 14.4 State and API homes
- Pools: `BandCharacterStats` — `Max/Remaining ×2`, `InitTurnPlayBudget`,
  `ResetTurnPlayBudget`, `TryConsumePlay`, `OnTurnPlayBudgetChanged`
  (D-ECON-2=A).
- Central gate: `GigManager.CanConsumePlay / TryConsumePlay(MusicianBase, bool)`.
- Enforcement: Action path consumes in `HandController.TryPlayInGig` (branch 2,
  after timing + inspiration + target resolution, before animation/Use);
  Composition path checks in `GigManager.TryPlayCompositionCard` BEFORE
  delegating and consumes ONLY when the session accepts the play.
- Maxima seeded at gig setup from `GigFlowSettingsSO`
  (`DefaultActionPlaysPerTurn` / `DefaultCompositionPlaysPerTurn`, both 1);
  no per-musician authoring field yet (D-ECON-5=A).

### 14.5 Attribution
Cards with `AnyMusician` performer bill the musician the play pipeline
resolves: fixed performer → hover (composition only) → `SelectedMusician`
fallback (D-ECON-3=A). The pips are the player-facing feedback for this
attribution.

**Overlay scoping (CARD-UX-1 / D5, 2026-07-13).** The unplayable-card overlay
(`SSoT_Card_System.md` §10.5) takes budget into account **only when the payer is
statically resolvable** (`FixedPerformerType != None`). `AnyMusician` cards —
whose payer depends on hover / `SelectedMusician` at drop time — are excluded
from the overlay's budget input until **D-ECON-GENERIC** resolves. Enforcement is
unchanged: `TryConsumePlay` still denies the drop. Rationale: a false red on a
card that *is* playable against another musician is worse than a false green on
an advisory overlay. ECON-1's rule itself is untouched by this scoping.

### 14.6 Relation to Inspiration
Inspiration cost is an ORTHOGONAL gate (HandController 2a.5 / session step 1)
and is untouched by ECON-1. Budget burns only on successful plays: a play
denied for cost does not consume budget, and a composition drop the session
rejects does not consume budget. Cards that keep a cost > 0 read as "finishers"
(see `Design_Action_Economy_v1.md` and D-ECON-6). The finisher layer is
designed but not yet populated: under D-ECON-6=DEFER (2026-07-07) every starter
card is cost 0, and which cards become finishers (cost > 0) is deferred to a
future design batch (finisher costs tuned in S5i).

### 14.7 UI contract
Two pips per musician on `BandCharacterCanvas` (Action / Composition), pushed
via `OnTurnPlayBudgetChanged`. Lit = play available; dimmed = consumed;
re-lit at every seam reset. Steady-state visible (not hover-gated, not under
the full-bar concealment root).

**Hover tooltip (DEMO-FIXES-A, 2026-07-15, D-DF-6=A).** Each pip carries an
`EconPipTooltipTarget` (`IPointerEnter/ExitHandler` → the existing `TooltipManager`
pipeline, `StatusIconBase` pattern — single tooltip source), auto-attached and fed the
remaining count by `BandCharacterCanvas.UpdateTurnPlayBudget`. Copy: "N action/composition
card use(s) left this turn." Presentation only — zero semantic change; requires the pip
`Image.raycastTarget = ON`. Closes the ECON-1 transparency debt on the pips. ST-DF-12 PASS.

The pips remain the budget's **primary** player-facing feedback. The card overlay
(`SSoT_Card_System.md` §10.5) is a **second, partial** surface — statically-resolvable
payers only, per §14.5 — and is advisory; the pips and the overlay must agree on the
frame after a play (regression ST-CU-10, CARD-UX-1).

### 14.8 Scope notes
- Dev Mode card spawner adds cards to hand; plays still route through
  `TryPlayInGig`, so the budget applies to spawned cards (T0b audit,
  2026-07-07). Ship/rehearsal context has no budget (out of gig scope).
- Decision table: D-ECON-1=A (batch slot S5g→ECON-1→S5h), D-ECON-2=A (state
  home), D-ECON-3=A (attribution), D-ECON-4=A (strict Y=1), D-ECON-5=A
  (flow-default maxima), D-ECON-6=DEFER (2026-07-07: all starter costs → 0;
  finisher card designation deferred to a future batch).

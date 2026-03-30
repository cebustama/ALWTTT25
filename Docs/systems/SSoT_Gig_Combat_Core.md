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
| `Vibe` | Current engagement / persuasion progress. |
| `VibeGoal` | Threshold required to convince that audience member. |
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

### 5.2 SongHype
**Owner:** Song.

Rules:
- changes loop by loop
- is derived from LoopScore conversion
- measures structural musical strength, not audience taste directly

### 5.3 Vibe
**Owner:** each audience member.

Rules:
- Vibe gain happens at Song End
- VibeDelta derives from:
  - `SongHype01`
  - audience-specific impression data accumulated across loops
- an audience member is **Convinced** when `Vibe >= VibeGoal`

### 5.4 Stress
**Owner:** each musician.

Rules:
- Stress is the main incoming pressure channel
- positive Stress is absorbed by **Composure** before being applied
- Breakdown triggers when `Stress >= StressMax` **after** Composure absorption

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
**Combat meaning:** amplifies Loop → SongHype conversion.

Conceptually:

```text
LoopScore -> BaseDeltaSongHype
DeltaSongHype = BaseDeltaSongHype * (1 + FlowStacks * FlowMultiplier)
```

Rules:
- Flow affects SongHype growth, not Inspiration directly
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
- when a musician is in `Shaken`, new Composure granted to that musician is reduced by 50% (round down) — **design intent; not yet enforced in runtime**

### 6.3 Breakdown / Shaken

Breakdown is not just flavor; it is a combat-visible threshold event.

**Trigger:**
- when `Stress >= StressMax` after Composure absorption

**Immediate MVP consequences (in order):**
1. `Cohesion − 1`
2. If `Cohesion <= 0` after step 1: call `GigManager.LoseGig()` immediately — **steps 3–4 are skipped**
3. Apply `Shaken` status (1 stack via `StatusEffectCatalogueSO` key `"shaken"`)
4. Reset `Stress = floor(StressMax * breakdownStressResetFraction)` — default fraction is `0.5`, configurable on `GigManager`

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
- LoopScore is converted into a base SongHype delta
- Flow modifies that delta multiplicatively in the baseline contract

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

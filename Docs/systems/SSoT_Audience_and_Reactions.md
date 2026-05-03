# SSoT_Audience_and_Reactions — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Audience entities, persuasion-side progress, preferences, intentions, and reaction contracts for the MVP line  
**Owns:** what an Audience Member is, how audience progress is tracked, when audience reacts, how preferences and intentions are interpreted at game level  
**Does not own:** full gig phase execution (`runtime/SSoT_Runtime_Flow.md`), full scoring conversion math (`systems/SSoT_Scoring_and_Meters.md`), status ontology internals (`systems/SSoT_Status_Effects.md`)

---

## 1. Purpose

This document is the primary authority for **what the audience is and how it behaves in ALWTTT**.

It replaces the previous implicit authority of:
- `reference/AudienceMember.md`
- parts of encounter and scoring notes
- audience-related backlog summaries

This SSoT defines the audience as a **persuasion-side opponent/target**, not as a traditional enemy abstraction.

---

## 2. Core fantasy

- The player is not killing enemies; the player is **winning over people in the room**.
- Each Audience Member is an agent with:
  - a persuasion progress track,
  - tastes/preferences,
  - a pressure pattern,
  - and a telegraphed next move.
- Audience state is part of encounter truth, but audience reaction meaning lives here.

---

## 3. Canonical Audience Member model

Each Audience Member owns these gameplay-facing concepts:

| Concept | Meaning |
|---|---|
| `Vibe` | current persuasion / engagement progress |
| `VibeGoal` | threshold required to fully convince that audience member |
| `Preferences` | bias toward or against certain performance/card styles |
| `Abilities` | audience-turn action patterns |
| `Intention` | telegraphed next action category/value |
| `State flags` | e.g. `Convinced` or encounter-specific modifiers |

Rule:
- the audience is a **reactive and pressuring system**
- it does not need a traditional HP/damage model to serve as combat structure

---

## 4. Persuasion progress

### 4.1 Vibe
`Vibe` is the canonical persuasion-side progress meter.

Rules:
- tracked per audience member
- increases mainly at **Song End**
- persists across songs within a Gig unless an encounter-specific modifier says otherwise

### 4.2 VibeGoal
`VibeGoal` is the threshold needed to convince that audience member.

When:
```text
Vibe >= VibeGoal
```
that member becomes **Convinced**.

### 4.3 Convinced state
Convinced means:
- that audience member has reached its persuasion threshold
- it should no longer count as an unresolved persuasion target for encounter completion
- any residual pressure behavior is encounter-specific and must be stated explicitly by the encounter rules

---

## 5. Reaction timing

Audience meaning spans two time scales.

### 5.1 Loop scale
At loop level, each audience member contributes to or stores **Impression-side reaction context**.

This is the micro layer:
- how the audience reads the current loop
- how tastes/preferences color that reading
- what later becomes part of song-end persuasion conversion

### 5.2 Song-end / audience-turn scale
At Song End:
- loop-level accumulated reaction context is converted into persuasion progress
- then audience executes its pressure turn or equivalent encounter-side reaction step

This timing must stay aligned with:
- `systems/SSoT_Scoring_and_Meters.md`
- `runtime/SSoT_Runtime_Flow.md`

---

## 6. Preferences

Preferences are the canonical way the audience distinguishes one performance style from another.

MVP-friendly preference axes may include gameplay-facing style categories such as:
- `CHR`
- `TCH`
- `EMT`
- `SFX`

Rules:
- preferences influence **reaction/impression**
- preferences do not directly replace SongHype or LoopScore
- preferences are allowed to be simple in the MVP line:
  - likes X
  - dislikes Y
  - bonus/malus to reaction strength

This document owns the meaning of preferences as audience interpretation bias.
Exact numeric conversion belongs to scoring/tuning.

---

## 7. Intentions and telegraphing

Each Audience Member may expose an **Intention** before its pressure step.

The intention exists to make the room readable.

Typical intention families:
- Stress pressure
- anti-momentum / disruption
- focus-fire / single-target pressure
- wide pressure / all-band pressure
- encounter-specific special actions

Rules:
- intentions should be visible before the audience acts
- intention is presentation of the next meaningful audience action, not hidden simulation state

---

## 8. Audience abilities

Audience abilities are packaged action patterns used during the audience reaction phase.

MVP categories:
1. **Stress pressure**  
   Main pressure channel against the band.
2. **Breakdown shaping**  
   Pressure patterns that push toward Breakdown timing.
3. **Disruption / anti-momentum**  
   Effects that conceptually fight momentum, confidence, or consistency.
4. **Special encounter actions**  
   Rare, encounter-specific exceptions.

Audience members may also carry player-applied statuses that shape their state during the audience turn — Earworm (M4.3) is the first. These statuses are not abilities; they are persistent effects whose runtime contract is owned by `systems/SSoT_Status_Effects.md` and whose tick hooks live in `GigManager.AudienceTurnRoutine`. The audience-side status surface is data-extensible through `StatusEffectCatalogue_Audience.asset`.

This SSoT owns the audience-facing gameplay meaning of those categories.
Detailed execution timing belongs to runtime.
Any status semantics used by those abilities belong to the status SSoT.

---

## 9. Audience and encounter relationship

Audience Members live **inside** the Gig encounter, but they are not the same thing as encounter structure.

Split of authority:
- `systems/SSoT_Gig_Encounter.md` owns the encounter envelope:
  - roster framing,
  - song count,
  - victory/failure structure,
  - gig-scoped modifiers
- this doc owns what an Audience Member is and how it reacts/persuades/pressures

---

## 10. MVP rules

For the baseline MVP, keep these rules true:

- each Audience Member has `Vibe` + `VibeGoal`
- audience persuasion progress persists across songs within a Gig
- audience preferences are simple and readable
- audience pressure is primarily expressed through Stress-oriented abilities
- intention telegraphing is favored over hidden surprise logic
- audience-side statuses are part of the MVP baseline; Earworm (M4.3) is the first active audience-side status. Future audience-side statuses extend this set through `StatusEffectCatalogue_Audience`.

---

## 11. Update rule

Update this document when a technical/design change affects:
- what counts as an Audience Member
- how persuasion progress is defined
- how preferences or intentions are interpreted
- what categories of audience pressure exist
- what “Convinced” means at gameplay level

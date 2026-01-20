# Gig — Encounter Spec (ALWTTT)

> **Scope note:** This document defines what a **Gig** is (encounter structure, victory/failure, and key variables).
> The detailed phase order + combat economy lives in **Gig Combat — Unified Spec** and should be treated as the source of truth.

**Terminology:** We use **Audience / Audience Members** (not “Crowd”) as the canonical game-design language.

**Status of this doc:** Updated to (1) remove drift/ellipsis placeholders, (2) clarify Stress + Breakdown at the encounter level,
and (3) reserve **Fan / second-bar mechanics** for special audience members (mini-bosses), not the baseline MVP.

---

## Overview

A **Gig** is a multi-song encounter where the band attempts to convince an **Audience** across a fixed number of songs.

Each **Song** is a self-contained performance loop (composition → performance loops → outcome),
and the **Gig** aggregates the results across its song sequence.

A Gig is the encounter unit that:
- defines the **length** (song count),
- defines the **opposition** (audience roster + their goals),
- defines the **pressure envelope** (how much Stress and disruption you face),
- and produces **rewards / progression** on completion.

---

## Base Identity (data model intent)

A Gig can be represented by a single encounter definition asset/data record.

Suggested fields (design-level; exact structs are implementation-specific):

| Field | Type | Meaning |
|---|---:|---|
| `gigId` | string | Stable identifier |
| `displayName` | string | UI-friendly name |
| `difficultyTier` | enum/int | Pacing/pressure tuning tier |
| `songCount` | int | Number of Songs in the Gig (encounter length) |
| `audienceRoster` | list | Audience Members present (with per-member VibeGoal, intent patterns, abilities) |
| `bandRoster` | list | Musicians present (with per-musician Stress, stats, deck ownership as appropriate) |
| `gigModifiers` | list | Optional global mutators (future; e.g., +Stress, +Inspiration, +Hype caps, etc.) |
| `rewards` | reward table | What the Gig can drop/grant (future) |

### Special Audience Members (mini-bosses)

By default (MVP), an Audience Member is modeled with **Vibe / VibeGoal** and ability pressure (mostly Stress).

If you introduce **special** encounters (mini-bosses / critics / VIPs), they may optionally use:
- a second engagement bar (e.g., **Fan** / **Anti-Fan**),
- unique passive auras/buffs/debuffs,
- additional telegraphed mechanics.

This is intentionally **out of baseline MVP** unless a Gig explicitly opts into it.

---

## Core Objectives

### Primary win condition (per Audience Member)

- Each Audience Member has **Vibe** (current engagement) and a **VibeGoal** (required engagement).
- At **Song End**, each Audience Member gains **VibeDelta** derived from the song outcome
  (per the conversion rules in **Gig Combat — Unified Spec**).
- An Audience Member is considered **Convinced** when `Vibe ≥ VibeGoal`.

### Gig-level win condition

**MVP default:** The Gig is cleared when all required Audience Members are convinced by the end of the final Song.

**Optional modes (future):**
- Clear when a percentage threshold is reached (e.g., ≥ 70% convinced).
- Clear when a GigScore threshold is reached (even if not everyone is convinced).
- Branching: convince “key targets” only (critics / VIPs) while others are optional.

### Gig-level scoring

A Gig may track an aggregate **GigScore** for rewards and meta-progression.

**MVP default:** `GigScore = Σ(SongHypeFinal)` (as per the unified contract), with room for tuning.

---

## Failure Conditions

### Cohesion (band-wide)

- The band has **Cohesion**: a Gig-level durability resource.
- The Gig fails when `Cohesion == 0`.

**Design intent:** Cohesion is the “you lose the gig / you can’t keep playing” bar.
It can be impacted by:
- explicit Audience abilities,
- hard encounter modifiers,
- (optionally) Breakdown consequences.

### Stress (per musician) and Breakdown (encounter-level rules)

- Each Musician has **Stress** (pressure) and a **StressMax** cap.
- **Composure** (a per-song defensive status) absorbs incoming Stress before it is applied.
- If Stress rises too high, it can trigger a **Breakdown**.

#### Breakdown (MVP definition)

Breakdown is an encounter-visible failure spike that turns Stress into a concrete, legible consequence.

**Trigger (MVP):**
- When `Stress >= StressMax`, the musician enters **Breakdown** immediately.

**MVP consequences (choose a minimal set and keep it consistent):**
- Apply a **BreakdownDebuff** to that musician for the rest of the current Song
  (e.g., cannot gain Flow, or reduced Inspiration efficiency, or reduced stats).
- Apply a small **Cohesion penalty** (e.g., `-1` or `-X%`) to reflect gig-wide fallout.
- Clamp/reset Stress to a safe post-breakdown value (e.g., `Stress = ceil(StressMax * 0.5)`)
  to avoid instant repeated Breakdowns in the same beat.

> The exact penalty profile is a tuning knob. The key is that Breakdown is (1) telegraphed,
> (2) costly enough to matter, and (3) readable enough that players learn to manage Stress.

**Future inspiration:** you can evolve Breakdown toward Darkest Dungeon-style “afflictions/virtues”,
barks, relationship fallout, and persistent traits — but MVP should stay deterministic and legible.

---

## Gig Structure (high-level)

A Gig proceeds as a fixed sequence of Songs.

### 1) Gig setup
- Load band roster and audience roster.
- Initialize Gig-wide state (e.g., Cohesion, encounter modifiers, score trackers).

### 2) For each Song (1..songCount)
At a high level (details live in Gig Combat — Unified):

1. **Song Start**
   - Initialize/reset **Song-scoped resources** (e.g., Inspiration, SongHype).
   - Reset/initialize **Song-scoped statuses** (e.g., Flow on Song/Band).
   - Initialize **Composure** state for musicians (per-song defensive buffer).

2. **Composition (player decision phase)**
   - Play Composition cards to build the part(s) to be performed.

3. **Performance (loops)**
   - Execute multiple loops.
   - Each loop updates SongHype, impressions, and applies ability pressure (e.g., Stress).

4. **Song End conversion**
   - Convert song results into **VibeDelta** for each Audience Member.
   - Update Convinced flags.
   - Update GigScore trackers.

5. **Between Songs**
   - Resolve between-song actions (Action cards, limited decisions, cleanup as per contract).
   - Check for Gig end conditions (victory/failure).

### 3) Gig resolution
- If victory: compute rewards and progression.
- If failure: apply failure consequences and exit.

---

## MVP Guidance (what this doc expects)

For MVP, validate the Gig loop with:

- A small initial card kit:
  - at least one Flow generator and one Composure generator,
  - 1 panic/recovery option,
  - 1 trade-off (tempo vs safety).
- 2 test Audience archetypes:
  1) **Stress applier**
  2) **anti-momentum** (punishes Flow / disrupts SongHype / blocks Vibe gain conceptually)
- Minimal UI exposure of:
  - SongHype + Flow stacks (Song/Band)
  - Stress + Composure (per musician)
  - Vibe vs VibeGoal per Audience Member
  - a clear Breakdown warning state (telegraph when near StressMax)

---

## Notes on Tactical Positioning (future / optional)

Stage positioning (band rows, audience rows, Tall/Blocking, movement keywords) is supported as a design direction,
but is **out of scope for the first MVP** unless you opt-in.

See **Gig Combat — Unified Spec (Part B)** for the positioning addendum.

# Gig — Encounter Spec (ALWTTT)

> **Scope note:** This document defines what a **Gig** is (encounter structure, victory/failure, key variables).
> Detailed combat economy + phase rules live in **Gig Combat (Unified)**.

---

## Overview

A **Gig** is a multi-song encounter where the band attempts to convince an audience across a fixed number of songs.
Each song is a self-contained performance loop (composition → performance loops → conversion), and the Gig aggregates the results.

**Design goal:** Keep the MVP readable and low-drift by treating the **Gig Combat Economy Contract** as the source of truth for resources, phase order, and conversions.

---

## Base Identity (data model intent)

| Field | Type | Description |
|---|---|---|
| `name` | string | Gig title (e.g., “Café Nebula”) |
| `venue` | string | Location / planet / stage identity |
| `gigDifficulty` | int/enum | Difficulty tier that influences pacing / pressure |
| `songCount` | int | Number of songs in the Gig (encounter length) |
| `audienceRoster` | list | Enemy/audience members present (with per-member VibeGoal) |
| `bandRoster` | list | Musicians in the band (with per-musician Stress) |

> Implementation details (exact structs/enums) are intentionally out of scope here.

---

## Core Objectives

### Primary win condition (per audience member)
- Audience members have **Vibe** and a **VibeGoal**.
- At **Song End**, each audience member gains Vibe based on the song outcome.
- An audience member is “convinced” when `Vibe ≥ VibeGoal`.

### Gig-level scoring
- Each song contributes to an aggregate **GigScore** (derived from per-song results).
- The Gig can be scored even if you don’t “perfect” every audience member (tunable per mode).

---

## Failure Conditions

### Cohesion (band-wide)
- The band has **Cohesion** (Gig-level durability).
- The Gig fails when `Cohesion == 0`.

### Stress (per musician)
- Each musician accumulates **Stress** as the Gig progresses.
- Stress sources/sinks are defined by the combat economy contract and card effects.

> **MVP baseline status effects** (Flow, Composure) are defined in the Contract + Card docs.

---

## Gig Structure (high-level)

A Gig is a sequence of `songCount` songs.

Each Song follows the same phase flow:

1) **Between-Songs Action Window** (MVP: 1 Action per musician; cost 0)
2) **Composition (Player Turn)** (spend Inspiration on Composition cards / parts)
3) **Performance (Song Loops)** (loops generate SongHype + impressions)
4) **Song End Conversion** (SongHype/impressions → VibeDelta)
5) **Audience Turn** (enemies apply pressure: Stress, debuffs, etc.)

---

## MVP Guidance (what this doc expects)

For MVP, the Gig design should be validated with:
- A small initial card kit (builders/spenders + 1 panic + 1 trade-off)
- 2 test enemy types (Stress applier, “anti-momentum” / SongHype disruptor)
- Minimal UI exposure of:
  - SongHype (and Flow stacks)
  - Stress (and Composure stacks)
  - Vibe vs VibeGoal per audience member

---

## Notes on Tactical Positioning (future / optional)

The game supports stage positioning concepts (band rows, audience rows, Tall/Blocking, movement keywords).
These are documented in **Gig Combat (Unified)** as an *addendum* and are explicitly **out of scope for the first MVP** unless you decide otherwise.

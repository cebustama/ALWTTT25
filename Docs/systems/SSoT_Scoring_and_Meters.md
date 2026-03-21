# SSoT_Scoring_and_Meters — ALWTTT

**Status:** Active governed SSoT  
**Scope:** LoopScore, SongHype, SongHype01, Vibe conversion meaning, and meter relationships across the MVP slice  
**Owns:** what each meter means, how the layers relate conceptually, and where conversion responsibility lives  
**Does not own:** full audience entity semantics (`systems/SSoT_Audience_and_Reactions.md`), full combat phase flow (`runtime/SSoT_Runtime_Flow.md`), package-side musical generation internals

---

## 1. Purpose

This document is the primary authority for **how ALWTTT interprets musical performance into gameplay-facing meters**.

It replaces the previous implicit authority of:
- `backlog/ideas/loopscore_songhype_vibe.md`
- scattered SongHype / Flow / Composure wording in combat notes
- audience summaries that partially described conversion

---

## 2. Canonical layer split

ALWTTT scoring operates across three conceptual layers.

### 2.1 Loop quality layer
This is the smallest evaluative unit.

Canonical meter:
- `LoopScore`

Meaning:
- how successful the current musical loop was from the game’s scoring perspective

### 2.2 Song momentum layer
This is the within-song momentum layer.

Canonical meters:
- `SongHype`
- `SongHype01`

Meaning:
- `SongHype` is the raw current-song momentum/quality meter
- `SongHype01` is the normalized form used in later conversions

### 2.3 Audience persuasion layer
This is the audience-facing progress layer.

Canonical meter:
- `Vibe` (per audience member)

Meaning:
- cumulative persuasion/engagement progress across the Gig
- not reset every loop
- not identical to SongHype

---

## 3. LoopScore

`LoopScore` is the primary loop-level performance score.

Typical influences may include:
- musical completeness/coherence
- synergy bonuses
- error penalties
- repetition penalties
- relevant gameplay/status modifiers

This SSoT does **not** freeze one exact numeric formula for all time.
What it freezes is the meaning:

- LoopScore is the loop-level input to song momentum
- it is allowed to be positive, neutral, or negative
- it is not itself the audience persuasion meter

---

## 4. SongHype

`SongHype` measures momentum and quality **inside the current song**.

Rules:
- it changes during the song, especially at loop resolution
- it is not gig-persistent across all songs by default
- it is the bridge between repeated loop success and song-end persuasion payout

`SongHype01` is the normalized representation used where a 0..1 or similar bounded signal is needed.

---

## 5. Vibe

`Vibe` is the persuasion result on each audience member.

Rules:
- Vibe is applied per audience member
- Vibe persists across songs within a Gig by default
- Vibe is not a duplicate of SongHype:
  - SongHype = current-song momentum
  - Vibe = cumulative persuasion progress

This split is essential and must not be collapsed.

---

## 6. Canonical conversion chain

The governed meaning is:

```text
Loop performance
    -> LoopScore
    -> SongHype / SongHype01
    -> Song-end persuasion conversion
    -> VibeDelta per audience member
```

Additional audience-specific modifiers may influence the last step:
- taste/preferences
- impression accumulation
- encounter modifiers
- other explicit reaction rules

---

## 7. Flow, Composure, and related meters

### 7.1 Flow
Flow interacts primarily with the **LoopScore -> SongHype** layer.

Canonical meaning:
- Flow strengthens momentum conversion
- Flow is not itself SongHype
- Flow is not Inspiration

### 7.2 Composure
Composure does **not** directly live in the musical scoring chain.
It is a defensive meter/status that absorbs Stress before Stress application.
Its main home is combat/status semantics, not scoring.

### 7.3 Stress
Stress is pressure against performers.
It is not a positive scoring meter.
It may indirectly affect loop quality or card/action availability, but it is not part of the positive conversion chain.

---

## 8. Payout timing

MVP-friendly rule:
- the main persuasion payout occurs at **Song End**
- loop-level scoring matters because it builds toward song-end conversion
- encounter-specific modifiers may layer additional effects, but they must not break this core distinction silently

---

## 9. What this doc intentionally does not own

This doc does not own:
- exact audience member definitions
- exact runtime manager orchestration
- package-side composition/generation algorithms
- deep balance tuning spreadsheets or temporary constants

It owns the **semantic contract** of the meter stack.

---

## 10. Update rule

Update this document when a change affects:
- what LoopScore means
- how SongHype differs from Vibe
- where normalized song momentum is used
- when persuasion payout occurs
- which layer Flow or similar positive-momentum statuses modifies

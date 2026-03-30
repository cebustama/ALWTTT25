# SSoT_Gig_Encounter — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Current ALWTTT authority for encounter-level gig structure, roster framing, victory/failure conditions, and encounter-scoped state  
**Owns:** what a Gig is, encounter identity/data intent, song-count structure, audience/band roster framing, gig-scoped win/fail logic, encounter modifiers, and resolution envelope  
**Does not own:** detailed combat economy (`systems/SSoT_Gig_Combat_Core.md`), detailed runtime phase execution (`runtime/SSoT_Runtime_Flow.md`), audience reaction math (`systems/SSoT_Audience_and_Reactions.md` / `systems/SSoT_Scoring_and_Meters.md`), or package-side MidiGenPlay internals

---

## 1. Purpose

This document is the primary authority for **what a Gig is as an encounter unit in ALWTTT**.

It promotes and stabilizes encounter-level truth that previously lived mainly in:
- `reference/Gig.md`
- encounter-related sections of `reference/Gig_Combat.md`
- assorted planning and backlog notes that described songs, victory/failure, and room structure

This SSoT exists so encounter structure no longer has to be inferred indirectly from combat or runtime docs.

---

## 2. Core definition

A **Gig** is a multi-song encounter where the band attempts to convince an **Audience** across a fixed song sequence.

A Gig is the encounter unit that defines:
- the **length** of the performance (`songCount` / equivalent)
- the **room/opposition** (audience roster and their pressure profile)
- the **band context** participating in the encounter
- the **encounter-level stakes** (Cohesion, GigScore, required persuasion targets, modifiers)
- the **resolution envelope** (victory, failure, rewards/progression handoff)

A Song is a unit **inside** a Gig.  
A Gig is not the same thing as:
- one loop,
- one part,
- one song,
- or one isolated gameplay phase.

---

## 3. Scope boundary

This SSoT answers:
- what data/identity an encounter needs
- what state is gig-scoped rather than song-scoped
- how the audience/band are framed at encounter level
- what counts as victory/failure
- how songs chain together into one encounter
- where encounter modifiers and special room rules belong

This SSoT does **not** define:
- the detailed combat phase order inside a song (`systems/SSoT_Gig_Combat_Core.md`)
- the runtime lifecycle that executes the encounter (`runtime/SSoT_Runtime_Flow.md`)
- the detailed scoring formulas that convert songs into persuasion (`systems/SSoT_Scoring_and_Meters.md`)
- package internals used for playback/generation (`integrations/midigenplay/` + MidiGenPlay docs)

---

## 4. Encounter identity / data intent

A Gig should be representable by a single encounter definition asset or equivalent data record.

Suggested design-facing fields:

| Field | Meaning |
|---|---|
| `gigId` | stable encounter identifier |
| `displayName` | UI-facing encounter name |
| `difficultyTier` | pressure/pacing tier |
| `songCount` | number of Songs in the encounter |
| `audienceRoster` | audience members present, with their goals/abilities/preferences as needed |
| `bandRoster` | musicians participating in the encounter |
| `gigModifiers` | optional encounter-wide mutators |
| `rewardProfile` | encounter reward/progression handoff metadata |

This document owns the **existence and role** of these fields as encounter concepts.  
It does not lock one exact implementation struct/API.

---

## 5. Canonical encounter-scoped state

### 5.1 Band / gig-side
| Variable | Meaning |
|---|---|
| `Cohesion` | band-wide durability for the whole Gig |
| `GigScore` | encounter-level aggregate score/output |
| `SongsRemaining` | pacing/progression state across the encounter |
| `GigModifiers` | encounter-wide mutators or special rules |

### 5.2 Audience-side at encounter scope
| Variable | Meaning |
|---|---|
| `AudienceRoster` | the set of audience members present in the Gig |
| `RequiredTargets` | which audience members must be convinced to clear the Gig |
| `Vibe` / `VibeGoal` persistence | audience persuasion state carried across songs inside the Gig |

### 5.3 Song relation
Each Song is nested inside the Gig and contributes to encounter progress through:
- song-end persuasion results
- pressure accumulation and recovery boundaries
- encounter score/progression tracking

---

## 6. Encounter lifecycle (high level)

### 6.1 Gig setup
Encounter setup must define:
- the participating band members
- the audience roster
- initial gig-scoped state such as Cohesion and score trackers
- encounter-specific modifiers or special rules

### 6.2 Song sequence
A Gig is composed of a fixed or otherwise explicitly defined sequence of Songs.

For each Song:
1. initialize song-scoped state
2. run the composition/performance/combat flow
3. resolve song-end persuasion and audience pressure
4. check encounter-level progress
5. either advance to the next Song or end the Gig

### 6.3 Gig resolution
At encounter end:
- compute victory/failure outcome
- finalize GigScore or equivalent encounter result
- hand off rewards/consequences to the larger progression layer if present

The detailed runtime sequencing belongs to `runtime/SSoT_Runtime_Flow.md`.  
This document owns the encounter-level structure that runtime is expected to execute.

---

## 7. Victory and failure

### 7.1 Audience-member victory rule
Each audience member has:
- current `Vibe`
- a target `VibeGoal`

An audience member is **Convinced** when:

```text
Vibe >= VibeGoal
```

### 7.2 Gig-level victory rule
**Baseline governed rule:** the Gig is cleared when all required audience members are convinced by the end of the encounter.

This keeps encounter success tied to persuasion rather than destruction.

Optional future encounter modes may include:
- percentage thresholds
- key-target-only clears
- score-threshold clears

Those remain encounter variants until explicitly promoted.

### 7.3 Gig-level failure rule
The Gig fails when the band can no longer sustain the encounter.

The baseline governed failure condition is:

```text
Cohesion <= 0
```

Combat/status docs specify **how** this pressure is applied.  
This document owns the fact that Cohesion is the encounter-level durability gate.

**Implementation:** failure is triggered by calling `GigManager.LoseGig()` (public method).  
This is invoked from `MusicianBase.OnBreakdown()` immediately after `PersistentGameplayData.BandCohesion` is decremented to `<= 0`.

**Note:** There is no method named `TriggerGigLoss`. The correct runtime call is `LoseGig()`.

---

## 8. Encounter modifiers and special room rules

### 8.1 Gig modifiers
Gig modifiers are encounter-wide mutators that alter how the room behaves.

Examples:
- higher incoming Stress pressure
- altered Inspiration economy
- SongHype caps/floors
- special audience pacing behavior

This SSoT owns the concept that such modifiers belong at encounter scope.

### 8.2 Special audience members / mini-bosses
Baseline MVP assumes ordinary audience members use the normal persuasion model:
- `Vibe`
- `VibeGoal`
- ability pressure
- preferences / reaction logic

A Gig may explicitly opt into **special audience members** such as:
- critics
- VIPs
- mini-boss-style anchors
- room leaders with special passive effects

These may add:
- special bars
- special passive auras
- custom telegraphed mechanics

They are valid encounter-level extensions, but not required for the baseline governed line.

---

## 9. What belongs to adjacent docs

### 9.1 `systems/SSoT_Gig_Combat_Core.md`
Owns:
- time scales such as loop/part/song/gig in combat terms
- combat phases
- combat-facing resources and failure pressure
- Breakdown/Stress/Flow/Composure meaning

### 9.2 `runtime/SSoT_Runtime_Flow.md`
Owns:
- which runtime actors execute the Gig
- phase transitions
- session creation/destruction
- feedback emission timing

### 9.3 `systems/SSoT_Audience_and_Reactions.md`
Owns:
- what an audience member is
- how audience members interpret loops/songs
- ability archetypes and audience-side reaction semantics

### 9.4 `systems/SSoT_Scoring_and_Meters.md`
Owns:
- LoopScore / SongHype / VibeDelta relationships
- song-end persuasion conversion semantics

This split prevents Gig docs from becoming another giant mixed authority document.

---

## 10. Invariants

1. A Gig is always the encounter unit, not the song unit.
2. Audience persuasion persists across Songs inside a Gig.
3. Cohesion is gig-scoped durability and the baseline fail gate.
4. Song results feed encounter progress; they do not replace encounter structure.
5. Encounter modifiers belong at Gig scope, not hidden inside subsystem docs.
6. Special audience/room mechanics must be expressed as explicit encounter variants, not silent defaults.

---

## 11. Migration note

This SSoT absorbs the encounter-structure truth of `reference/Gig.md` into the governed system.

After this promotion:
- `reference/Gig.md` should be treated as a migration source / superseded reference, not a primary home
- combat and runtime docs no longer need to carry encounter structure by implication
- the remaining cleanup work is mostly reference/archive normalization rather than missing primary authority

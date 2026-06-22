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

### 3.1 Semantic meaning (frozen)

- LoopScore is the loop-level input to song momentum
- it is allowed to be positive, neutral, or negative
- it is not itself the audience persuasion meter

### 3.2 Current formula (M4.2, 2026-04-28)

`LoopScoreCalculator.ComputeLoopScore` computes:

```text
score = (ActiveTracks × densityBonusPerTrack)
      + (roleBudget × fillRatio)
      + (IsLastLoop ? lastLoopBonus : 0)
      + (complexityMultiplier × TotalComplexity)
```

Where `fillRatio` depends on `LoopScoringMode`:
- **RoleNormalization** (default): `distinctRolesFilled / possibleRoleCount`
- **MusicianParticipation**: `distinctMusiciansPlaying / totalMusicians`

`possibleRoleCount` and `totalMusicians` are auto-detected at gig start from the band's composition cards and roster. This makes the scorer adaptive to any band size and role configuration.

### 3.3 HypeDelta conversion

`ComputeHypeDelta` maps LoopScore to a SongHype delta via a piecewise threshold table (`HypeThresholds` struct). Both thresholds and deltas are Inspector-tuneable on `MeterTuningSO` (M4.6F-2; authored on `GigManager` pre-F-2). Defaults: Amazing ≥25 → +15, VeryGood ≥15 → +8, Decent ≥5 → +3, Neutral >−5 → 0, Meh >−15 → −5, Bad else → −12.

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

Additional audience-specific modifiers may influence the last step.

### 6.1 Per-audience impression modifier (D-F-3=β, S1)

The song-end conversion (`GigManager.ComputeSongVibeDeltas`) applies a
per-audience multiplier derived from accumulated loop impressions:

    avgImpression    = mean of that member's per-loop impressions   ∈ [−2, +2]
    impressionFactor = 1 + (avgImpression × 0.25)                   ∈ [0.5, 1.5]
    baseVibe         = SongHype01 × MeterTuningSO.MaxVibeFromSongHype
    vibeDelta        = round(baseVibe × impressionFactor)
                       (floor 0 — MVP: no negative macro Vibe)

Placement rationale (D-F-3=β over loop-level scaling): the baseline stays
SongHype-derived, so performance quality remains the primary reward and the
audience layer skews it per-individual — preserving the §2 layer split.
The 0.25 coefficient is tuning, not contract; it may move during balance
passes without a semantic change entry.

Other modifiers on the last step: Flow song-end multiplier (§7.1, applied to
the impression-modified base above — the "L" part — only), the flat SFX venue
bonus added after Flow (§6.2), Indifference gate at `ApplyIncomingVibe`
(see `SSoT_Audience_and_Reactions.md` §5.3), encounter modifiers.

### 6.2 SFX venue bonus + song-end delivery (S5a)

The SongHype "venue energy" bonus (per-stage magnitudes on
`GigPresentationSO.sfxBonusVibeStage{1,2,3}`; see `Design_Demo_Cut_v1.md` §3.1)
is **flat** — it is not impression-scaled (D-S5-SFX-SCALE=A) and not Flow-scaled.

Delivery (D-S5-VIBE=B, S5a): the bonus is **not** applied mid-song. Each upward
stage crossing banks its flat amount into a song-scoped accumulator
(`GigManager._pendingSfxVibe`, reset per song in `ResetSongHype`). The accumulated
total is paid **once at Song End**, combined with the per-audience conversion in
`GigManager.RunSongVibeResolution`. The full per-member song-end delta is:

    perMemberDelta = round(lPart × flowSongEndMult) + pendingSfx

where `lPart` is the §6.1 result (`round(baseVibe × impressionFactor)`, floored
at 0), `flowSongEndMult` is the band-wide Flow multiplier (§7.1) applied to the L
part only, and `pendingSfx` is the flat banked total added after Flow. The combined
delta is gated once at `ApplyIncomingVibe` (Indifference → 0); `IsBlocked` members
are excluded upstream. The banked total equals the pre-S5a sum of per-stage
applications — S5a moved **when** the bonus lands (mid-song → song-end), not
the amount.

`_pendingSfxVibe` is bespoke/song-scoped, shaped so the planned Pending Effects layer
(`Design_Pending_Effects_v1.md`) can absorb it (D-S5-VIBE-ARCH=A). The player-facing
readout of `lPart` ("L") + `pendingSfx` ("SFX") is the Vibe telegraph
(`planning/Design_Vibe_Telegraph_v0_1.md`).

---

## 7. Flow, Composure, and related meters

### 7.1 Flow
Flow interacts with the **Vibe layer** — not the LoopScore or SongHype layers. Bifurcated by card domain (M4.2, 2026-04-28):

- **Action cards:** flat per-performer Flow bonus on positive `ModifyVibe` effects
- **Composition cards:** multiplier using band-wide Flow stacks on positive `ModifyVibe` effects
- **Song End:** multiplier using band-wide Flow stacks on per-audience VibeDelta

Canonical meaning:
- Flow amplifies Vibe gains, differentiated by card domain
- Flow is not itself SongHype (the Flow → SongHype path was retired and removed in M4.2)
- Flow is not Inspiration

Flow tuning lives on `MeterTuningSO.flowActionFlatBonus`, `flowActionVibeBonusPerStack`, and `flowVibeMultiplier`. Initial tuning: `flowActionFlatBonus = true`, `flowActionVibeBonusPerStack = 1`, `flowVibeMultiplier = 0.08f` (M4.6F-2).

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
- the SFX venue bonus is banked during the song and paid at Song End with the
  conversion (S5a, §6.2) — a single per-member payout, no mid-song Vibe application
- loop-level scoring matters because it builds toward song-end conversion
- encounter-specific modifiers may layer additional effects, but they must not break this core distinction silently

---

## 9. What this doc intentionally does not own

This doc does not own:
- exact audience member definitions
- exact runtime manager orchestration
- package-side composition/generation algorithms
- deep balance tuning spreadsheets or temporary constants
- the SO assets that host loop-scoring config / hype thresholds / Flow-Vibe values (those are governed for asset locality by `SSoT_Gig_Combat_Core` §12; this doc owns their semantic contract only)

It owns the **semantic contract** of the meter stack.

---

## 10. Update rule

Update this document when a change affects:
- what LoopScore means
- how SongHype differs from Vibe
- where normalized song momentum is used
- when persuasion payout occurs
- which layer Flow or similar positive-momentum statuses modifies

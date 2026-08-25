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
This is the audience-facing persuasion layer.

Canonical meter:
- `Vibe` (per audience member)

Meaning:
- each member's remaining persuasion resistance (enemy-HP pool), depleted across the Gig (see §5)
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

**[S5e note]** The complexity term (`0.5 × TotalComplexity`, sourced from
per-track `inspirationGenerated`) is **inert as of S5e** (D-S5e-1=A): the
starter deck's `inspirationGenerated` was stripped to 0 project-wide (D3),
so `TotalComplexity = 0` for all current content and the term contributes
nothing to any LoopScore. Retained in code; S5i owns its replacement or
removal when the inspiration economy is re-tuned.

**[DF-INSPLOOP note, 2026-07-16]** DF-INSPLOOP reintroduces per-loop Inspiration generation as a **card-gated, track-scoped** bonus (`AddInspirationPerLoopSpec`, derived via `EvalPerLoopInsp`), NOT by re-activating `inspirationGenerated`. The bonus never enters `LoopTrackSnapshot`, so `TotalComplexity` remains 0 and this complexity term remains inert. The two are orthogonal: DF-INSPLOOP feeds the Inspiration economy (per-loop grant), the complexity term feeds LoopScore. A dead-state field, `CompositionSession._buildingPartInspirationPerLoop` (write-only since S5e), is left in place; its cleanup belongs to S5i.

**Per-loop Inspiration sources (current runtime).** (1) Basal flat grant = **1/loop** (`GigFlowSettings.asset` `defaultInspirationPerLoop`; applied by `GigManager.OnCompositionLoopFinished`). (2) Card-gated track-scoped bonus (DF-INSPLOOP, +N per active carrying track, applied via `CompositionSession` on the same per-loop path the S5e basal generation used before it was zeroed). The composition UI's global `+INS` badge shows the **total** (basal + derived), un-clamped/nominal per S5e; the per-track badge shows that track's derived contribution.

### 3.3 HypeDelta conversion

`ComputeHypeDelta` maps LoopScore to a SongHype delta via a piecewise threshold table (`HypeThresholds` struct). Both thresholds and deltas are Inspector-tuneable on `MeterTuningSO` (M4.6F-2; authored on `GigManager` pre-F-2). Defaults: Amazing ≥25 → +15, VeryGood ≥15 → +8, Decent ≥5 → +3, Neutral >−5 → 0, Meh >−15 → −5, Bad else → −12.

**Full expression, including the Overload factor (R5-c).** A loop's contribution to SongHype is

```text
hypeDelta = ComputeHypeDelta(loopScore, HypeThresholds) × SongHypeDeltaMultiplier
            [× OverloadHypeFactor  if Overload discharged at this boundary AND hypeDelta > 0]
```

The factor lives in the expression inside `GigManager.TriggerAudienceMicroReactions`;
`MeterTuningSO.SongHypeDeltaMultiplier` is persistent encounter configuration and is **not**
mutated at runtime. The multiplier applies to the **delta**, not to the `loopScore`:
`ComputeHypeDelta` is a step function, so scaling its input produces unpredictable jumps —
sometimes nothing, sometimes a whole band. The positive-delta condition is preventive
(D-R5-19=B): with the current calculator negative deltas are unreachable, and the cost is paid
on threshold crossing regardless. Overload semantics, threshold and cost live in
`SSoT_Status_Effects.md` §5.10; this document owns only where the factor sits in the chain.

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

`Vibe` is each audience member's remaining **persuasion resistance** — an
enemy-HP-style pool. **[S5e inversion, 2026-07-02]**

Rules:
- tracked per audience member; starts at `MaxVibe` (full resistance)
- **depleted** by incoming persuasion (song-end conversion, card effects,
  Earworm ticks); reaching **0 = Convinced ("conquered")**
- persists across songs within a Gig by default
- Vibe is not a duplicate of SongHype:
  - SongHype = current-song momentum (unchanged by S5e)
  - Vibe = remaining resistance to cumulative persuasion

This split is essential and must not be collapsed.

Magnitude semantics of the conversion chain are direction-agnostic: a
"VibeDelta of N" means "N persuasion damage" — the same N as before the
inversion.

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

Since S5e, `VibeDelta per audience member` is applied as **depletion** of
that member's Vibe pool (persuasion damage), not accumulation toward a goal.

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
(see `SSoT_Audience_and_Reactions.md` §5.3), Captivated amplification at that
same gate (audience-side, `×(1 + N × MeterTuningSO.captivatedVibeBonusPerStack)`,
applied after the Indifference gate — `SSoT_Status_Effects.md §5.8`, R1),
encounter modifiers.

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
delta is gated once at `ApplyIncomingVibe` (Indifference → 0, then Captivated
amplification if the member holds stacks — `SSoT_Status_Effects.md §5.8`);
`IsBlocked` members are excluded upstream. Consequence worth noting for tuning:
the flat SFX bonus is *not* Flow-scaled but *is* Captivated-scaled, because
Captivated applies at the gate, downstream of the `lPart + pendingSfx` sum. The banked total equals the pre-S5a sum of per-stage
applications — S5a moved **when** the bonus lands (mid-song → song-end), not
the amount.

`_pendingSfxVibe` is bespoke/song-scoped, shaped so the planned Pending Effects layer
(`Design_Pending_Effects_v1.md`) can absorb it (D-S5-VIBE-ARCH=A). The player-facing
readout of `lPart` ("L") + `pendingSfx` ("SFX") is the Vibe telegraph
(`planning/Design_Vibe_Telegraph_v0_1.md`).

**Venue-SFX unlock gate (S5h/D8, #6b-lite).** From S5h the flat SFX bonus is
additionally gated by per-threshold **unlock state**
(`PersistentGameplayData.IsSfxStageUnlocked(stage)`; run-scoped, reset in
`ApplyRunConfig`, granted as a gig reward). A **locked** stage produces nothing
at its crossing — no banked `pendingSfx`, no VFX, no `SfxStageCrossedEvent` —
so a fresh run's first gig has **no SFX Vibe layer** and the "SFX" readout term
stays 0 until a threshold is unlocked (demo: unlocks apply from the next gig /
Retry). This changes *whether* the banked bonus exists per stage; it does not
change the §6.1/§6.2 math when a stage is unlocked. The unlock is the demo slice
of the future venue-SFX-equipment system (Phase C).

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
**[S5e inversion]** The Stress meter stores the musician's remaining
**mental fortitude** (HP-style pool): starts at `MaxStress`, depleted by
incoming Stress, **0 = Breakdown (collapse)** — the same deplete-to-0-loses
pattern as Band Cohesion. Incoming-Stress magnitudes, Composure absorption
(shield semantics unchanged), and Exposed amplification are numerically
identical to pre-S5e; only storage direction and the threshold boundary
changed. Stress remains outside the positive scoring chain.

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

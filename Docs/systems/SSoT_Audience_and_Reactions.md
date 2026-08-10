# SSoT_Audience_and_Reactions — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Audience entities, persuasion-side state, preferences, intentions, and reaction contracts for the MVP line  
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
  - a persuasion resistance track,
  - tastes/preferences,
  - a pressure pattern,
  - and a telegraphed next move.
- Audience state is part of encounter truth, but audience reaction meaning lives here.

---

## 3. Canonical Audience Member model

Each Audience Member owns these gameplay-facing concepts:

| Concept | Meaning |
|---|---|
| `Vibe` | remaining persuasion resistance (enemy-HP pool); `0` = Convinced (see §4.1) |
| `MaxVibe` | total persuasion resistance (tuning knob — a tougher member = higher `MaxVibe`); replaces the retired `VibeGoal` (see §4.2) |
| `Preferences` | bias toward or against certain performance/card styles |
| `Abilities` | audience-turn action patterns |
| `Intention` | telegraphed next action category/value |
| `State flags` | e.g. `Convinced` or encounter-specific modifiers |

Rule:
- the audience is a **reactive and pressuring system**
- it does not need a traditional HP/damage model to serve as combat structure

---

## 4. Persuasion (Vibe)

### 4.1 Vibe
`Vibe` is the canonical persuasion meter, stored **inverted** since S5e:
each member's remaining persuasion resistance (enemy HP).

Rules:
- tracked per audience member; initialized to `MaxVibe`
- **decreases** mainly at **Song End** (persuasion damage)
- persists across songs within a Gig unless an encounter-specific modifier
  says otherwise

### 4.2 Convince condition
The convince condition is **pool depletion**:

```text
Vibe <= 0
```

that member becomes **Convinced**. The former `VibeGoal` threshold concept
is retired: `MaxVibe` itself is the member's total resistance and plays the
old VibeGoal's tuning role (a tougher member = higher `MaxVibe`).

### 4.3 Convinced state
Convinced means:
- that audience member's persuasion resistance has been fully depleted
  ("conquered")
- it should no longer count as an unresolved persuasion target for encounter completion
- any residual pressure behavior is encounter-specific and must be stated explicitly by the encounter rules

---

## 5. Reaction timing

Audience meaning spans two time scales.

### 5.1 Loop scale
At loop-finished, `GigManager.TriggerAudienceMicroReactions` calls
`ResolveLoopEffect(LoopFeedbackContext)` on every audience member. Each
returns a per-loop impression in [−2, +2] (see §6).

Player-facing surface (per Sensory Contract D2):
- Per-audience floating text spawned at the audience member's
  `TextSpawnRoot`: `WOW!` (+2, gold), `YEAH` (+1, green), `…` (0, muted
  grey — darker than MEH), `MEH` (−1, light grey), `BORING` (−2, red). Word
  exclamations, not numerals (D-F-4=A). Neutral emits FT too — no silent
  impressions.
- Audio SFX deferred to S3-audio (D-S3-1=B; no audio subsystem yet).
- Reaction animator deferred (D-F-5b): no reaction animator state exists;
  only the audience *ability* trigger (e.g. Kid "Tantrum", restored in S3a
  per D-F-5a) does.
- Neutral "…" legibility against busy venue backgrounds is addressed by a TMP
  outline material on the FloatingText prefab (D-S3-2=A, applied Unity-side).

Impressions are accumulated per audience per part
(`GigManager._audienceLoopImpressionsByPart`) and consumed at song-end (§5.2).
Emission is **bus-only** as of S3a: `GigManager` publishes `AudienceReactionEvent`
and `SensoryFxAdapter` (Spawn) is the sole FT source; the S1 direct call is
deleted (D-S3-4=A). Song-end FT uses the int `SpawnFloatingText` overload to
preserve the S1 random-diagonal drift (D-S3-5=A).

### 5.2 Song-end / audience-turn scale
At Song End:
- loop-level accumulated reaction context is converted into persuasion damage (Vibe depletion)
- then audience executes its pressure turn or equivalent encounter-side reaction step

This timing must stay aligned with:
- `systems/SSoT_Scoring_and_Meters.md`
- `runtime/SSoT_Runtime_Flow.md`

### 5.3 Indifference interaction (D-F-2=A, S1)

Indifference does **not** zero `ResolveLoopEffect` output. Loop-level
impressions remain live and visible (the player still sees how the
audience reads the music). Indifference gates only the song-end Vibe
conversion, at the canonical `ApplyIncomingVibe` path, where the blocked
conversion surfaces as an `INDIFFERENT` floater instead of `+N Vibe`.
Rationale: Indifference means "doesn't get convinced", not "doesn't
perceive music". Single gate = no double-blocking, and keeps
`LoopScoreCalculator` / SongHype decoupled from per-audience status state.

**Captivated layering (R1, 2026-07-23).** The same `ApplyIncomingVibe` gate hosts the Captivated
amplification layer, applied strictly *after* the Indifference check: blocked stays 0 regardless of
Captivated stacks. Captivated likewise does not touch `ResolveLoopEffect` output — it modulates only
what crosses the Vibe boundary, keeping the single-gate property intact. Status spec:
`SSoT_Status_Effects.md §5.8`.

---

## 6. Preferences (taste profiles)

Preferences are the canonical way the audience distinguishes one performance
style from another. As of S1 (B3-slate-F, 2026-06-12), they are implemented
as **taste profiles** authored inline on
`AudienceCharacterData.TastePreferences` and consumed by
`AudienceCharacterBase.ResolveLoopEffect(LoopFeedbackContext)`.

### 6.1 Taste axes (4, frozen at S1)

| Axis | Loop input | Match (+1) | Mismatch (−1) |
|---|---|---|---|
| Tempo | `LoopFeedbackContext.TempoScale` (cumulative TempoEffect ScaleFactor; 1.0 = authored default) | `tempoMatchOnFast` && TempoScale > `preferAboveTempoScale` | `tempoMismatchOnSlow` && TempoScale < `dislikeBelowTempoScale` |
| Arrangement density | `LoopFeedbackContext.ActiveTracks` | `roleCountMatchOnRich` && ActiveTracks ≥ `preferAtLeastRoles` | (one-sided; no mismatch branch) |
| Time signature | `LoopFeedbackContext.TimeSignature` | TS ∈ `preferredTimeSignatures` | TS ∈ `dislikedTimeSignatures` |
| Tonality | `LoopFeedbackContext.Tonality` | Tonality ∈ `preferredTonalities` | Tonality ∈ `dislikedTonalities` |

`RootNote` is intentionally NOT a taste axis (D-F-1, S1): pitch class without
modal context carries no archetype meaning; Tonality covers the modal axis.

**Presentation note (F-R4-2, no batch assigned).** Time-signature and tonality
values are currently shown to the player as their **enum names** (`SixEight`,
`FourFour`, `Aeolian`) — legible for the team, not for the player. A mapping to
domain text (`6/8`) is missing. Registered as F-R4-2 at R4 (2026-08-10).

### 6.2 Combination rule (D-F-1=A, discrete per-axis count)

Per loop, each enabled axis contributes +1 / −1 / 0. The sum is clamped to
**[−2, +2]** — the per-loop impression. Empty / disabled axes contribute 0,
so an asset with no taste fields set is a **neutral archetype** (always 0).
This is also the backward-compat path for any audience asset authored pre-B3.

### 6.3 Authority split

- This doc owns: axis meaning, combination rule, the [−2, +2] impression
  contract, neutral-on-empty rule.
- `systems/SSoT_Scoring_and_Meters.md` §6 owns: how accumulated impressions
  convert to Vibe at song-end (the impressionFactor multiplier).
- Asset-side thresholds (per-archetype values) are content, not contract.

### 6.4 Preference reveal (R4, 2026-08-10)

- Runtime state: `AudienceCharacterBase.PreferencesRevealed`, instance scope
  (GameObject lifetime ⇒ per-gig), **not persisted across gigs**.
- Idempotent: revealing the same member twice is a deliberate silent no-op.
- Surface: `AudienceCharacterCanvas.ShowTastePanel(TastePreferences)`. An
  unwired prefab degrades to a no-op (S5a telegraph pattern);
  `IsTastePanelWired` exposes it.
- Authority: the reveal spec carries **no taste data**. `AudienceCharacterData`
  owns the data; the canvas owns the presentation. A spec carrying text or axes
  would duplicate §6.1 and desynchronize on the first retune.
- **Open debt (D-R4-10):** the panel is persistent for the rest of the gig; the
  recommended direction is a persistent icon + detail on hover.

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

**Targeting redirect hook (R4, 2026-08-10 — Spotlight).** `AudienceCharacterBase.ResolveTargetsFor` now has a **prior step** on the `Musician` and `RandomMusician` branches: if any musician carries an active Spotlight, the target is substituted by the holder. `AllMusicians` does not pass through the hook. This is the single targeting funnel, so every future audience ability inherits the redirect with no per-ability edit. Status semantics: `systems/SSoT_Status_Effects.md` §5.9.

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

- each Audience Member has `Vibe` + `MaxVibe` (the retired `VibeGoal` folded into `MaxVibe`, see §4.2)
- audience Vibe (persuasion resistance) persists across songs within a Gig
- audience preferences are simple and readable
- audience pressure is primarily expressed through Stress-oriented abilities
- intention telegraphing is favored over hidden surprise logic
- audience-side statuses are part of the MVP baseline. Active set: **Earworm** (M4.3, first audience-side status — Vibe DoT), **Indifference** (B3 — blocks all incoming Vibe), **Captivated** (R1 — amplifies all incoming Vibe, ×(1 + N×0.25)). Future audience-side statuses extend this set through `StatusEffectCatalogue_Audience`. Semantics for all three: `SSoT_Status_Effects.md` §5.7–§5.8 (Indifference is documented alongside the `ApplyIncomingVibe` gate, §5.3 above).

---

## 11. Update rule

Update this document when a technical/design change affects:
- what counts as an Audience Member
- how persuasion resistance (Vibe) is defined
- how preferences or intentions are interpreted
- what categories of audience pressure exist
- what “Convinced” means at gameplay level

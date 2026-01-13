# ALWTTT — Crowd / Fan / Hype / Stress Design Notes (Conversation Summary)

This document compiles all ideas discussed in the conversation (from the “Vulnerable” split in the primitives table to Fans/Hecklers and the “nemesis-lite” scope creep), categorizes them via a justified ontology, and summarizes them in detail.

---

## Context: why this document exists

We are designing **ALWTTT Status Effects** by starting from **Slay the Spire (STS) analogs** and formalizing them as a **canonical ontology of primitives**.

- **Goal:** build a shared vocabulary for effects (what they *do*), independent of final ALWTTT naming, visuals, or flavor.
- **Method:** map common STS statuses and combat patterns to **primitives** described by:
  - **EffectId** (stable identifier in the ontology)
  - **Category** (design intent cluster)
  - **Abstract Function** (mechanical meaning, system-agnostic)
  - **STS reference** (anchor for intuition and balance expectations)

This lets ALWTTT keep its own fantasy (Stress/Vibe/Hype/Fans) while retaining the proven “deckbuilder grammar” players already understand.

### Ontology table (STS-only)

| Slay the Spire (STS) | EffectId | Category | Abstract Function |
|---|---|---|---|
| Strength | DamageUpFlat | Offensive | +X damage per hit / action |
| Strength scaling relics | DamageUpMultiplier | Offensive | Multiplies outgoing damage |
| Weak (approx) | DamageDownFlat | Offensive Control | -X damage from attacker |
| Weak | DamageDownMultiplier | Offensive Control | -% damage from attacker |
| Vulnerable (approx) | DamageTakenUpFlat | Burst | Target receives +X extra damage |
| Vulnerable | DamageTakenUpMultiplier | Burst | Target receives +% extra damage |
| Block | TempShieldTurn | Defense | Temporary shield, resets per turn |
| Plated Armor | TempShieldPersistent | Defense | Shield persists until depleted |
| Intangible (partial) | NegateNextHit | Defense | Negates next damage instance |
| Artifact (analog) | NegateNextNInstances | Defense | Negates N damage instances |
| Frail | AntiShieldGain | Control | Reduces shield generation |
| Thorns | DamageReflection | Control | Reflects damage back |
| Poison | DamageOverTime | Pressure | Automatic periodic damage |
| Entangle / stun-like | DisableActions | Tempo Control | Target cannot act |
| — | DisableMovement | Control | Prevents movement / state changes |
| — | InitiativeBoost | Tempo | Acts earlier |
| Multi-hit cards | MultiHitModifier | Scaling | Grants additional hits |
| Penetrating attacks | PiercingDamage | Penetration | Ignores shields / mitigation |
| Artifact | DebuffImmunityStacks | Resistance | Blocks debuff application |
| Cleanse effects | DebuffCleanse | Recovery | Removes debuffs |
| Form powers / relic synergies | ArchetypeAmplifier | Meta | Boosts card archetypes |
| Energy relics / draw engines | TempoAcceleration | Meta | Faster loops / turns |
| Energy / draw scaling | ResourceGenerationModifier | Meta | Modifies resource generation |

---

## Ontology proposed (and justification)

To avoid mixing “beautiful ideas” with “implementable rules,” the notes are organized into **7 categories**. This splits the design into clean layers:

1) **Principles & conceptual frame**  
   The “philosophy” of the system: how to think ALWTTT, what makes it distinct.

2) **Core resources & metrics**  
   The bars/counters that are the numeric language of the game.

3) **States & transitions**  
   When an entity “changes type,” and why.

4) **Status effects: primitives + target adaptation**  
   The CSO ontology: one canonical primitive expressed differently on Musician vs Crowd.

5) **Combat loop / turn economy**  
   How progress is earned per loop, how rewards/punishments trigger, and Hype’s role.

6) **UI/UX & semantics (names, tooltips, visual telegraphing)**  
   How players understand the system without a manual.

7) **Roadmap & controlled scope creep**  
   What’s MVP vs “later,” and how to avoid breaking the prototype.

---

## 1) Principles & conceptual frame

### 1.1 Not “two versions of every status”; one primitive with two interfaces
- You **do not** need duplicate “musician-version” and “crowd-version” for every status effect.
- Instead:
  - **One canonical primitive** (e.g., `DamageTakenUpMultiplier` / “take more damage”),
  - expressed through **two different interfaces** depending on target:
    - **Musicians** → interact with **Stress** (and band-side constraints: execution, pressure, breakdowns).
    - **Crowd** → interact with **Vibe** (and crowd-side constraints: attention, resistance, conversion, buffs).

This avoids forced symmetry: some effects should remain symmetric (cleanse, immunity stacks), others should be intentionally asymmetric (Breakdown vs Conversion).

### 1.2 ALWTTT is about “winning people,” not “killing enemies”
The strongest framing is: the Crowd starts as an obstacle, but the goal is to **win them over**.

- “I defeated them” → wrong fantasy for a gig.
- “I convinced them / won them / hooked them” → correct fantasy.

### 1.3 Separate *progress* from *spectacle*
Your direction cleanly separates:
- **Vibe** = immediate progress toward winning the gig.
- **Hype** = spectacle / momentum resource (enables signature moves, requirements, bonuses).

This prevents Vibe and Hype from becoming the same thing with different skins.

---

## 2) Core resources & metrics

### 2.1 Stress (Musicians)
- Stress is the band-side danger bar.
- Key rule: **Stress reaching max → Breakdown** (a predictable, telegraphed punishment).

Why it’s strong:
- Creates a clear “overheat” rule.
- Enables “pressure management” gameplay that fits the gig fantasy (fatigue, focus, ego, tension).

### 2.2 Vibe (Crowd)
- Vibe is the crowd-side progress bar.
- Key decision: **Vibe is gained per loop** (immediate), not mainly via “song-end conversion.”

Why:
- Immediate feedback: actions feel like progress now.
- Avoids end-of-song spikes and delayed gratification.
- Matches deckbuilder feel: progress per turn.

### 2.3 Hype (Songs)
- Hype bar stays, but is no longer the main “funnel” into Vibe.
- New role: **bonus / gating / requirements / costs** for powerful signature cards.

Conceptual examples:
- Requirement: “Signature: requires 6 Hype.”
- Cost: “Spend 3 Hype to…”
- Scaling: “Effect scales with current Hype.”
- Bonus: “If Hype ≥ X, apply extra.”

Intent: Hype = showmanship / peak moments, not “delayed damage.”

### 2.4 Fan meter as “second bar” (or overflow)
A new layer above Vibe: **Fan** as an “ideal state” reachable via:
- Overflow of Vibe (“second health bar”).
- Hype spending (signature moments).
- Stackable statuses.
- Requests / requirements (future system).

This creates “excellence” on top of baseline progress.

---

## 3) States & transitions

### 3.1 Crowd as a gig-scope state machine
Minimal form discussed:

- **Neutral → Engaged → Fan**
- (if conditions fail) → **Heckler / Hater / Hostile Fan** (optional / future)

The important part: **Fan is not only a number**, it’s a qualitative state change.

### 3.2 Fan as the “ideal” state
Definition:
- Crowd members become Fans when conditions are met (overflow Vibe, Hype threshold, etc.).
- Thematically and mechanically, it makes sense that Fans become **buff sources**.

### 3.3 Breakdown as a telegraphed musician transition
- When Stress hits max → **Breakdown**.
- Breakdowns should be:
  - predictable,
  - explainable in UI,
  - something the player can plan around.

Goal: player skill expression, not random punishment.

---

## 4) Status effects: primitives + target adaptation

### 4.1 Why “Vulnerable” had two entries (one “approx”)
The primitives list splits “take more damage” into two separate mechanics:
- **Flat**: +X extra per instance
- **Multiplier**: +% extra

In STS, Vulnerable is a **multiplier**, so the flat entry is only a conceptual analog → **“Vulnerable (approx)”**.

This split matters because flat vs % balance differently:
- Flat spikes with multi-hit.
- Multiplier preserves proportionality and spikes with big hits.

### 4.2 ALWTTT translation: Crowd “Vulnerable” = “gain more Vibe”
Proposed crowd-side equivalent:
- They’re so absorbed that they **receive more Vibe**.

Naming:
- “Dazzled” felt long.
- Alternatives discussed: **Rapt**, **Awe**, Hooked, Starstruck, Locked-In.
- Recommended: **Rapt** or **Awe** (short, readable).

### 4.3 Tooltip rule: semantics must declare target interface
To make “one primitive, two interfaces” intuitive:
- Crowd tooltip: “Rapt: +25% Vibe gained this loop” (example)
- Musician tooltip (if relevant): declare how it affects Stress/actions/breakdown risk (example)

Key: UI must explicitly state what the effect means on that target type.

---

## 5) Combat loop / turn economy

### 5.1 Vibe-per-loop as the core rule
- Each loop, actions produce Vibe directly.
- Song-end conversion is discarded **for now** (can return later with a distinct role).

Result: the system feels like a deckbuilder, not like a delayed score tally.

### 5.2 Hype as signature gating/bonus/cost
- Signature cards require / spend / scale with Hype.
- Hype becomes the “peak moment” resource that enables big plays.

### 5.3 Fan conversion as payoff and build direction
Fans can be earned via:
- Hype accumulation (easy MVP use of existing resource),
- Vibe overflow (mechanically obvious),
- later: requests/requirements.

This naturally defines archetypes:
- Consistent Vibe builders
- Hype spike / signature-focused builds
- Crowd control builds (apply Rapt, etc.)

---

## 6) UI/UX & semantics

### 6.1 Renaming “Audience Members”
Shorter terms discussed:
- **Crowd** (best overall: short + clear)
- Fans (as a state)
- Hecklers (as a hostile state)
- Punters (UK-coded), Attendees/Spectators (longer)

Recommendation: use **Crowd** as the system baseline term.

### 6.2 Explain without walls of text
Suggested UI pattern:
- Crowd shows:
  - **Vibe bar**
  - **Fan bar** beneath (thin) or clear overflow indicator
- Clear thresholds:
  - VibeMax
  - Fan threshold
- Minimal tooltips:
  - “Vibe: progress this gig.”
  - “Fan: when full, becomes a Fan and buffs you.”
- Strong conversion feedback:
  - icon change + animation + buff aura appears

### 6.3 Stress “Breakdown line”
- Stress bar includes a clear cap line.
- Tooltip at cap: “At max Stress: Breakdown.”
- Makes punishment feel like a rule, not arbitrary.

---

## 7) Roadmap & controlled scope creep

### 7.1 MVP set (implementable without extra systems)
Minimal, identity-defining set:
1) **Vibe is gained per loop**.
2) **Hype exists** and is used as requirement/bonus/cost for some cards.
3) **Crowd overflow → Fan** (simple Fan meter).
4) **Fans grant a buff** (recommended: +Hype burst + small passive).
5) **Stress max → Breakdown** (simple, telegraphed consequence).
6) **Rapt/Awe** status: “+% Vibe gained.”

This already gives ALWTTT a distinct identity without building requests/persistence.

### 7.2 “Super scope creep” direction (future)
Proposed future:
- **Satisfied Fans** persist and can reappear at later gigs with a base boost.
- If you fail to satisfy them, they stop being fans (relationship degradation).
- They can become **Hecklers / Hostile Fans** and appear with negative buffs.
- This becomes a **nemesis-lite** social system: reappearance + memory + relationship state.

### 7.3 Main risk to monitor: snowballing
Fans that give Hype → enables signatures → creates more Fans → exponential loop.

Common mitigations (later):
- Gig-scope caps.
- Diminishing returns.
- Upkeep / expectation failure causing degradation.
- Real costs for signature use.

---

## One-line synthesis
ALWTTT should make it explicit that **Musicians and Crowd don’t share “health”**: they operate on **two different economies (Stress vs Vibe)**. Status effects are **canonical primitives** that are **interpreted differently by target**. Progress should happen **per loop via Vibe**, while **Hype** becomes a distinct showmanship/momentum resource used for **signature gating/bonuses/costs**. The Crowd can transition into **Fans** as an ideal state that turns progress into **buffs**, enabling a future path toward **Satisfied/Hecklers** and a “nemesis-lite” social layer.

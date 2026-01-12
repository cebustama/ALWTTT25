# Audience Member — Game Design Spec (ALWTTT)

> **Scope note:** This document defines the **design intent** for **Audience Members** (formerly “Enemies”).
> It is aligned with:
> - **Gig Combat — Unified Spec (v0.2.2)** for economy + phase flow  
> - **Cards** for `CardType`-based preferences and the “Vibe vs Stress” MVP axis  
>
> **Deliberate omission:** This version **does not** include a Data Model section yet (we’ll add it after the code cross-check pass).

---

## 1) What an Audience Member is

An **Audience Member** is an agent in the room that:

- Has a **Vibe** progress bar and a **VibeGoal** (win condition per member).
- Has **Preferences** that determine how they “read” what you play (based on `CardType`).
- Has an **Ability pattern** (their “turn”) that applies **pressure** on the band (mostly via **Stress** and debuffs).
- Optionally carries **Status Effects** or **Positional Traits** (future, out of MVP unless explicitly enabled).

You are not “killing enemies”; you are **convincing people**.

---

## 2) Core resources & win logic (MVP)

### 2.1 Vibe (Audience progress)

- Each Audience Member has **Vibe** (current engagement) and **VibeGoal** (required engagement).
- At **Song End**, each Audience Member receives a **VibeDelta** derived from:
  - **SongHype01**
  - **AvgImpression** across loops

When `Vibe ≥ VibeGoal`, that Audience Member becomes **Convinced**.

### 2.2 Stress (Band pressure)

Audience Members are the main source of **Stress** and anti-tempo pressure:
- direct Stress application,
- debuffs/statuses,
- momentum disruption (future).

> MVP simplification: you can validate the whole loop with 2 archetypes:
> 1) **Stress Applier** (simple pressure)
> 2) **Anti-momentum** (punishes Flow / hurts SongHype / blocks Vibe gain conceptually)

---

## 3) Time placement in the Gig loop

Audience Members matter at two distinct time scales:

1) **During loops** (micro):  
   They compute an **Impression** each loop ([-2..2] or similar), which later influences Song End Vibe conversion.

2) **On Audience Turn** (macro, per song):  
   After Song End conversion, **each Audience Member executes one Ability**.

This matches the contract’s phase flow:
- Performance loops → Song End conversion → **Audience Turn**.

---

## 4) Preferences (synergy axis)

Audience preferences are expressed using **CardType** (not “Funny/Touching/Raw”):

- `CHR` — crowd work, frontman energy  
- `TCH` — precision, complexity  
- `EMT` — emotional impact, vulnerability, catharsis  
- `SFX` — stagecraft, production, spectacle  

**Design intent:**
- Preferences should influence **Impression** (loop-level) and/or **Ability selection** (future).
- Preference logic can remain *simple* for MVP:
  - e.g., “Likes CHR: +1 impression on CHR loop; dislikes TCH: -1”, etc.

---

## 5) Intent system (telegraphing)

Each Audience Member should telegraph their next move via an **Intention**:

- An **intent icon** communicates the **category** (Stress attack, debuff, support, etc.).
- Optionally, an **action value** is shown (e.g., “x3”).
- The intention is shown **before** the Audience Turn (so the player can plan).

This is a UX cornerstone for “readable pressure” in deckbuilders.

---

## 6) Abilities (what they do on their turn)

An **Ability** is a packaged enemy “turn” with:
- a name (presentation),
- an intention (telegraph),
- one or more atomic actions.

### 6.1 Ability categories (design vocabulary)

Use a small set of categories to keep MVP legible:

1) **Pressure (Stress)**
   - Add Stress to a musician / random musician / all musicians.

2) **Disruption**
   - Reduce Song momentum (future: Flow), interfere with conversions, apply “anti-Vibe” states.

3) **Debuff / Control**
   - Apply “Skeptical/Chill/Heckled/Hooked” style combat states (only if needed for MVP).

4) **Support (Audience-side)**
   - Buff another Audience Member, protect a Tall member, etc. (future / optional).

> MVP recommendation: implement Pressure first; add one Disruption archetype second.

---

## 7) Status Effects (audience-side) — future by default

The economy contract does **not** require Audience-side status effects for MVP baseline.
However, the system can support them later to increase tactical depth.

Guideline:
- If you introduce an Audience status, define:
  - its scope (Audience Member),
  - how it stacks,
  - how/when it resolves,
  - and its decay/reset rule.

---

## 8) Requests & reactive triggers — future system (optional)

A richer Audience model can include:
- **Requests** (“Play something CHR next song”, “No SFX for 2 loops”, etc.)
- **Triggers** reacting to events:
  - `CardType played`, `unmet request`, `high Flow`, `musician stressed`, etc.
- **Reactions** (Cheer, Boo, Heckle…) as presentation + gameplay hooks.

**Important:** This is explicitly **out of MVP** unless you decide otherwise.
For MVP, you can fake “requests” as simple Intent-driven abilities.

---

## 9) Positional traits (Tall/Blocking) — future addendum

Tactical layout features (rows, Tall, Blocking, bypasses) live in **Gig Combat — Unified** addendum and are:
- **kept** as future design material,
- **disabled** for MVP unless enabled intentionally.

---

## 10) MVP checklist for Audience Member authoring

For the first playable end-to-end slice, each Audience Member should have:

- A **VibeGoal** (tuning knob for difficulty)
- 2–3 **Abilities** total (simple pattern is fine)
- Clear **Intent** icons for those abilities
- (Optional) 1 preference axis (e.g., loves CHR, dislikes TCH) wired only to loop Impression

That’s enough to validate:
- Song loop → SongHype → Vibe conversion
- Readable pressure loop (Stress)
- Deckbuilding choices matter (via preferences and timing)

---

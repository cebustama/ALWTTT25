# Audience Member — Game Design Spec (ALWTTT) — UPDATED (v0.2.3 aligned)

> **Scope note:** This document defines the **design intent** for **Audience Members** (formerly “Enemies”).
> It is aligned with:
> - **Gig — Encounter Spec (UPDATED)** (`Gig_UPDATED.md`)
> - **Gig Combat — Unified Spec (Economy Contract v0.2.3)** (`Gig_Combat_v0.2.3_UPDATED.md`)
>
> **Terminology:** We use **Audience / Audience Members** as canonical language.
>
> **Note on data model:** This is a design spec. Implementation structs/enums are intentionally not locked here unless explicitly stated.

---

## 1) What an Audience Member is

An **Audience Member** is an agent in the room that:

- Has a **Vibe** progress bar and a **VibeGoal** (win condition per member).
- Has **Preferences** that influence how they “read” what you play (via `CardType`).
- Has an **Ability pattern** (their “turn”) that applies **pressure** to the band (primarily via **Stress**, and optionally “anti-momentum” disruption later).
- May optionally carry **Status Effects** or **Positional Traits** in future expansions, but **Audience-side statuses are not required for the MVP baseline**.

You are not “killing enemies”; you are **convincing people**.

---

## 2) Core resources & win logic (MVP)

### 2.1 Vibe (Audience progress)

- Each Audience Member has **Vibe** (current engagement) and a **VibeGoal** (required engagement).
- At **Song End**, each Audience Member receives a **VibeDelta** derived from:
  - `SongHype01`
  - `AvgImpression` across loops

When `Vibe ≥ VibeGoal`, that Audience Member becomes **Convinced**.

**Persistence across the Gig (MVP assumption):**
- **Vibe persists across songs** within a Gig (it’s the gig-level “HP-like” progress).
- Decay / regression is future tuning (not assumed for MVP unless a specific Audience Member says so).

### 2.2 Stress (Band pressure) & why Audience Members matter

Audience Members are the main source of **Stress** pressure (the band-side “damage”).

Key Stress rules (MVP baseline):
- **Range:** `0 … StressMax` per musician (default `StressMax = 10`, tunable).
- **Composure absorbs incoming Stress before it is applied** (Block-first semantics).
- If Stress reaches max, it triggers a **Breakdown** (telegraphed, deterministic).

#### Breakdown → Shaken (MVP contract)

- **Trigger:** when `Stress >= StressMax` for any musician (after Composure absorption).
- **Immediate effects (MVP):**
  1) **Cohesion -1** (gig-wide durability hit)
  2) That musician enters `BreakdownState = Shaken`
  3) That musician’s `Stress = ceil(StressMax / 2)` (prevents instant re-trigger loops)
- **Shaken duration:** lasts **until the end of the next Song**.
- **Shaken rules:**
  - The musician **cannot play Action cards** in the Between-Songs window.
  - Any Composure granted to that musician is **reduced by 50% (round down)**.

This makes Audience pressure readable: the player manages Stress proactively via Composure and safer lines, rather than being surprised by invisible failure.

### 2.3 Cohesion (gig-wide loss condition)

- The Gig fails when **Cohesion == 0**.
- Audience Members influence Cohesion indirectly (via Breakdown triggers) and later may do so directly (special abilities / modifiers).

---

## 3) Where Audience Members act in the Gig loop

Audience Members matter at two distinct time scales:

1) **During loops (micro): Impression**
- Each loop, each Audience Member computes an **Impression** signal that later contributes to `AvgImpression`.

2) **On Audience Turn (macro, per song): Ability**
- After Song End conversion, **each Audience Member executes one Ability**.

Phase flow (contract):
**Performance loops → Song End conversion → Audience Turn**.

---

## 4) Preferences (synergy axis)

Audience preferences are expressed using **CardType**:

- `CHR` — crowd work, frontman energy  
- `TCH` — precision, complexity  
- `EMT` — emotional impact, vulnerability, catharsis  
- `SFX` — stagecraft, production, spectacle  

**MVP intent:**
- Preferences primarily influence **Impression** (loop-level).
- Keep logic simple:
  - “Likes CHR: +1 impression on CHR-dominant loops”
  - “Dislikes TCH: -1 impression on TCH-dominant loops”
- More complex preference bundles can be layered later.

---

## 5) Intent system (telegraphing)

Each Audience Member should telegraph their next move via an **Intention**:

- An **intent icon** communicates the category (Stress, disruption, control, etc.).
- Optionally show an **action value** (e.g., “+3 Stress”, “x2 hits”, “targets all”).

**Timing rule:** The intention is shown **before** the Audience Turn so the player can plan.

---

## 6) Abilities (what they do on their turn)

An **Ability** is a packaged “enemy turn” with:
- A name (presentation),
- An intention (telegraph),
- One or more atomic actions (targeting + effect).

### 6.1 Ability categories (design vocabulary)

Keep a small set so MVP stays legible:

1) **Pressure (Stress) — MVP core**
- Add Stress to:
  - a single musician,
  - a random musician,
  - all musicians (rare / tuned carefully).

2) **Breakdown-shaping patterns (still Stress)**
- Multi-hit Stress (“+1 Stress x3”) that interacts with Composure differently than a single hit.
- Spike Stress (“+4 Stress”) to threaten the cap line.

3) **Disruption (second MVP archetype)**
- “Anti-momentum” pressure that conceptually fights:
  - Flow value,
  - SongHype stability,
  - or the reliability of Impressions.

4) **Debuff / Control (future; only if needed)**
- Apply controlled negative states (e.g., “Insecure”-like effects for musicians), only after MVP is stable.

5) **Cohesion hit / gig-level consequences (future / special)**
- Direct Cohesion damage should be rare, readable, and mostly reserved for special Audience Members or explicit modifiers.

---

## 7) Status Effects on Audience Members (future by default)

Audience Member status effects are **optional** and **not required for the MVP baseline**.

If/when you introduce them:
- Define scope (Audience Member),
- Stack and decay rules,
- And exactly how they modify conversion or behavior.

---

## 8) Special Audience Members (mini-bosses / critics / VIPs)

Baseline MVP Audience Members are modeled with **Vibe / VibeGoal** + an ability turn (mostly Stress).

**Special Audience Members** may optionally introduce:
- A second engagement bar (e.g., **Fan / Anti-Fan**),
- Unique passive auras/buffs/debuffs,
- Additional telegraphed mechanics.

This is intentionally **out of baseline MVP** unless a Gig explicitly opts in.

---

## 9) Positional traits (Tall/Blocking) — optional addendum

Rows, Tall/Blocking, movement keywords, and targeting constraints are **optional / out of scope for MVP** unless enabled.

If enabled later, Audience Members may have `PositionalTraits` like Tall/Blocking and abilities that push/pull/advance.

---

## 10) MVP authoring checklist (Audience Member)

For the first end-to-end slice, each Audience Member should have:

- **VibeGoal** (difficulty knob)
- **2–3 Abilities** total (simple patterns are fine)
- Clear **Intent** icons for those abilities
- Optional: **1 preference** rule wired only to loop Impression (keep it simple)

Recommended MVP pair for validation:
1) **Stress Applier** (clean, readable pressure)  
2) **Disruptor / anti-momentum** (second archetype after pressure works)

---

## 11) Archetype templates (ready-to-author)

These are intentionally **template-like**: you can copy/paste them into your internal authoring docs or use them as a checklist for building ScriptableObjects.

### 11.1 Archetype A — “Heckler” (Stress Applier)

**Role:** Primary pressure unit. Teaches the player that **Composure is Block**, Stress is damage, and Breakdown is the cliff.

**Core fantasy:** Someone in the room notedly stresses the band out: shouting, mocking, heckling.

**Suggested MVP stats**
- `VibeGoal`: **8–12** (low-medium)
- `Preferences`: 1 simple rule (optional)
  - Example: Likes `CHR` (+1 Impression on CHR loops), Dislikes `TCH` (-1 Impression on TCH loops)

**Ability set (2–3 total)**
1) **“Shout Over the Verse”** *(Intent: Stress / Single Target)*
   - Add **+3 Stress** to the **highest Stress** musician.
   - Purpose: pressures the weakest point and creates “triage” decisions.

2) **“Relentless Chatter”** *(Intent: Stress / Multi-hit)*
   - Add **+1 Stress x3** to a **random** musician (roll target each hit or lock target—pick one and keep consistent).
   - Purpose: teaches multi-hit vs Composure behavior.

3) **(Optional) “Room Turns”** *(Intent: Stress / All)*
   - Add **+1 Stress** to **all** musicians.
   - Use sparingly; tune carefully.

**Tuning notes**
- If players trigger Breakdown too often, reduce spikes: change +3 → +2, or multi-hit x3 → x2.
- If too easy, increase “smart targeting” frequency (e.g., more often hits highest Stress).

**UI / Readability**
- Intent should show exact Stress value (e.g., “+3”) so players can calculate “Do I Breakdown next?”

---

### 11.2 Archetype B — “Distractor” (Anti-momentum Disruptor)

**Role:** Makes the band’s performance less reliable without simply “more Stress”.
This is your second MVP archetype after stress pressure is working.

**Core fantasy:** The room is distracted, disengaged, noisy—your **momentum** doesn’t land cleanly.

**Suggested MVP stats**
- `VibeGoal`: **10–15** (medium)
- `Preferences`: optional but recommended (so they “care” about something)
  - Example: Likes `SFX`, Dislikes `EMT`

**Ability set (2–3 total)**  
Pick one of these disruption directions for MVP, not all of them.

**Option 1 — Impression disruption (recommended MVP simplest)**
1) **“Talking Through the Hook”** *(Intent: Disruption)*
   - Apply a **Song-level modifier**: `ImpressionPenalty = -1` for **next Song** (or for next N loops if your system prefers loop-scoped modifiers).
   - Purpose: reduces AvgImpression → reduces VibeDelta; feels “my song didn’t land.”

2) **“Phones Out”** *(Intent: Disruption)*
   - For **next Song**, reduce `AvgImpression` contribution by **-20%** (or clamp to -1 per loop, depending on how you represent Impression).
   - Purpose: a stronger, rarer disruption turn.

**Option 2 — Hype reliability disruption (only if you already have the hook)**
1) **“Momentum Break”** *(Intent: Disruption)*
   - Reduce `SongHype` by **-1** (or `SongHype01` by a small delta) at the start of the next Song.
   - Purpose: attacks the “quality” axis rather than Stress.

**Option 3 — Flow pressure (only if Flow is already visible & meaningful)**
1) **“Throws Off the Groove”** *(Intent: Disruption)*
   - Reduce Band `Flow` by **-1** (or prevent Flow gain next Song).
   - Purpose: attacks the “Strength-like” axis.

**Tuning notes**
- Start with **Option 1** for MVP: it is easiest to understand and keeps the contract intact (Vibe is still a function of Hype + Impression).
- Ensure disruption effects have clear durations: “next song only” is clean and learnable.

**UI / Readability**
- Intent icon should be distinct from Stress.
- A small “Next Song: Impression -1” banner is enough for MVP.

---

### 11.3 Archetype C (Optional) — “Critic” (Special Audience Member, mini-boss)

> **This is out of baseline MVP**. Use only if the Gig explicitly opts into “Special Audience Member”.

**Role:** A signature enemy that changes the rules a bit.

**Core fantasy:** A critic can sway the room: tension rises, confidence shakes, and the whole band feels watched.

**Suggested special mechanics**
- Enables an **Anti-Fan** track (second bar) OR a persistent room modifier.
- Provides a **passive aura**:
  - Example: “Band starts each Song with +1 Stress”
  - Or “Composure gains are 25% less while Critic is unconvinced”
- Has a telegraphed “review” attack:
  - **“Harsh Review”**: add +2 Stress to all musicians and apply a short debuff (“Shaken chance”, “Insecure”, etc.)

**Design warning**
- This archetype can snowball (especially if it hits Cohesion). Keep direct Cohesion damage off the table unless you want a boss-level fail threat.

---

## 12) Quick template block (copy/paste)

Use this as a “fillable” authoring stub:

- **Name:**
- **Role (Pressure / Disruption / Special):**
- **VibeGoal:**
- **Preferences:**
- **Ability List:**
  - Ability A — Intent — Targeting — Effect — Duration
  - Ability B — Intent — Targeting — Effect — Duration
  - Ability C (optional) —
- **Readability notes:** (what the UI must show)
- **Tuning knobs:** (values you can easily tweak)


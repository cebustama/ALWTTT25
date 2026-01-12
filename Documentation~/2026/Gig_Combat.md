# Gig Combat — Unified Spec (ALWTTT)

This document is a **single, unified** reference for Gig Combat.
It contains:

- **Part A:** the authoritative **Gig Combat Economy Contract** (verbatim)
- **Part B:** a tactical **addendum** (positioning, targeting, Tall/Blocking) kept for future use and explicitly **out of scope for the first MVP** unless enabled.

---

# Part A — Gig Combat Economy Contract (Authoritative)

# A Long Way to the Top (ALWTTT)

# Gig Combat Economy Contract --- v0.2.2

**Status:** Draft (Authoritative Design Baseline)\
**Scope:** Live Gig Combat, Inspiration-only Economy\
**Audience:** Systems Design, Card Design, Class / Archetype Design

**Changelog (v0.2.2):**
- Added MVP baseline Status Effects: Flow (Song/Band) and Composure (Musician).
- Added band-wide targeting simplification for MVP (apply-to-all-musicians helper).
- Updated conversion rule (Loop → SongHype) to include Flow.
- Updated MVP validation milestone slice.

------------------------------------------------------------------------

## 1. Purpose

This document defines the **authoritative combat economy contract** for
ALWTTT.

Its goals are:

-   Establish a single source of truth for:
    -   Resources
    -   Phases
    -   State variables
    -   Production / spending rules
    -   Conversion math
-   Eliminate legacy drift (notably: Groove → Inspiration).
-   Provide a stable foundation for:
    -   Designing cards
    -   Designing archetypes
    -   Balancing tempo and difficulty
    -   Building skeleton decks for MVP validation

This document intentionally avoids deep balance numbers and focuses on
**structural correctness and clarity**.

------------------------------------------------------------------------

## 2. Core Fantasy

-   Combat is **Band vs Audience Members**.
-   The opponent is the room: multiple audience agents, each with
    preferences, abilities, and reactions.
-   The player composes music live, loop by loop.
-   Musical structure generates momentum.
-   Audience feedback creates pressure and tactical consequences.

The player is not "attacking enemies" --- they are shaping perception,
energy, and emotional response in real time.

------------------------------------------------------------------------

## 3. Time Scales

The system operates across five nested time scales.

### 3.1 Loop (Micro)

-   Smallest evaluation unit.
-   During each loop:
    -   LoopScore is computed.
    -   SongHype changes.
    -   Each audience member generates an impression.
    -   Inspiration increases based on the active Part.

### 3.2 Part (Within Song)

-   A configured musical block (Intro, Verse, Chorus, etc.).
-   A Part loops multiple times.
-   The player composes the Part during the Composition Phase.

### 3.3 Song

-   A sequence of Parts.
-   At Song End:
    -   SongHype and impressions convert into Vibe changes.
    -   Audience performs its action phase.

### 3.4 Gig

-   A sequence of Songs.
-   Ends when victory or failure conditions are met.
-   Generates rewards.

### 3.5 Run (Meta)

-   Long-term progression (Fans, upgrades, unlocks).
-   Out of scope for this contract.

------------------------------------------------------------------------

## 4. State Variables

### 4.1 Band / Gig-Level

  -----------------------------------------------------------------------
  Variable                           Description
  ---------------------------------- ------------------------------------
  Cohesion                           Band-wide durability. Gig fails at
                                     0.

  GigScore                           Aggregate score across all songs
                                     (derived from SongHype totals).

  SongsRemaining                     Encounter pacing / structure.
  -----------------------------------------------------------------------

### 4.2 Song-Level

  Variable        Description
  --------------  --------------------------------------
  SongHype         Current song quality meter (0..max).
  SongHype01       Normalized SongHype (0..1).
  PartIndex        Current part index.
  LoopIndex        Current loop index inside part.
  StatusEffects    Song/Band-scoped stackables (MVP: includes Flow).

### 4.3 Musician-Level

  Variable        Description
  --------------- -------------------
  CHR             Charisma
  TCH             Technical
  EMT             Emotional
  Stress          Pressure meter
  StatusEffects   Stackable effects

### 4.4 Audience-Level

  Variable           Description
  ------------------ ------------------------------------
  Vibe               Current engagement
  VibeGoal           Required Vibe to convince / defeat
  Preferences        CardType preferences (CHR/TCH/EMT/SFX)
  Abilities          Conditional action sets
  StatusEffects      Stackable effects
  PositionalTraits   Optional (Blocking, Tall, etc.)

------------------------------------------------------------------------

## 5. Resources

### 5.1 Inspiration (Primary Tactical Resource)

**Owner:** Band (scoped per Song)\
**Reset Rule:**\
- At the start of each Song:\
`Inspiration = BaseInspiration` (e.g., 4)

**Production:**\
- After each loop, Inspiration increases based on the active Part's
properties and Composition cards played (incl. per-loop contributions from
Composition cards; see `InspirationGenerated`).

**Spending:**\
- Used to play Composition cards (and potentially future systems).

**Design Intent:** - Inspiration defines tempo, complexity, and
strategic depth. - Player decisions determine future budget through
musical structure.

------------------------------------------------------------------------

### 5.2 SongHype (Quality Signal)

**Owner:** Song\
**Production:**\
- Each loop modifies SongHype via LoopScore.

**Purpose:**\
- Measures structural musical strength independently of audience taste.

------------------------------------------------------------------------

### 5.3 Vibe (Audience Outcome Resource)

**Owner:** Each audience member\
**Production:**\
- At Song End: - Vibe gain derives from: - SongHype01 - Average
impressions across loops

**Victory Condition:**\
- Audience member is convinced when `Vibe ≥ VibeGoal`.

------------------------------------------------------------------------

### 5.4 Stress (Pressure Resource)

**Owner:** Each musician\
**Sources:**\
- Audience abilities
- Risky play
- Overextension

**Sinks:**\
- Recovery cards
- **Composure** (absorbs incoming Stress before it is applied)
- Other defensive statuses (future)

**Failure Pressure:**\
- High Stress risks Breakdowns and persistent penalties.


------------------------------------------------------------------------

### 5.5 Status Effects (MVP Baseline)

Status Effects are **stackable** modifiers applied to one of the following scopes:

- **Song/Band** (active Song only; resets at Song start)
- **Musician** (resets at Song start unless specified otherwise)
- **Audience Member** (future; none required for MVP baseline)

**Design rule:** A Status Effect definition must specify:
- **Scope** (Song/Band, Musician, Audience Member)
- **Stack type** (integer stacks for MVP)
- **Reset/decay rule**
- **Resolution rule** (how it modifies existing conversions/resources)

**MVP simplification (to reduce multi-archetype complexity):**
- Cards may be authored with a **Band-wide targeting helper** that applies a Musician-scoped status to **all musicians**.
- This keeps the data model "per musician" while letting you balance **one shared deck** for the band during early validation.

**Guardrail (MVP balance):** If a status-gain card can be played once per musician per Song, band-wide application can stack quickly (up to `#Musicians` times per Song). Tune numbers with that upper bound in mind.

#### 5.5.1 Flow (Song/Band)

**Scope:** Song/Band (active Song)\
**Stacks:** integer, starts at `0` each Song, **no decay** during the Song; may accumulate indefinitely until Song End (unbounded for MVP).\
**Fantasy:** the band "locks in" and everything lands better.

**Primary effect (MVP):** Flow amplifies **Loop → SongHype** conversion.

Conceptually:

    LoopScore → BaseΔSongHype
    ΔSongHype = BaseΔSongHype * (1 + Flow * FlowMultiplier)
    SongHype = clamp(SongHype + ΔSongHype)

`FlowMultiplier` is tunable.

**Notes:**
- Flow does **not** affect Inspiration.
- Future extension (non-MVP): shared-flow loss on mistakes (band-wide penalty).

#### 5.5.2 Composure (Musician)

**Scope:** Musician (default), resets each Song\
**Stacks:** integer, starts at `0` each Song.\
**Fantasy:** focus, breathing, confidence; prevents spiraling.

**Primary effect (MVP):** Composure absorbs incoming Stress before Stress increases.

When applying incoming Stress `S` to a musician:

    absorbed = min(Composure, S)
    Composure -= absorbed
    Stress += (S - absorbed)

**MVP targeting:** a card may grant "Composure to the Band" by applying the same Composure stacks to **each** musician.

------------------------------------------------------------------------

### 5.6 GigScore (Reward Signal)

**Owner:** Gig\
**Production:**\
- Sum or aggregate of SongHype results across all songs.

**Usage:**\
- Drives Fans, unlocks, rewards.

------------------------------------------------------------------------

## 6. Phase Flow

### Phase 0 --- Setup

-   Initialize band, audience, UI, deck.

### Phase 1 --- Between-Songs Action Window

-   Optional Action cards may be played.
-   Limit: **One Action per musician per song.**
-   Action cards:
    -   Do NOT cost Inspiration (Option A).
    -   Manipulate statuses, positioning, requests, tension, etc.
    -   Represent non-musical interactions.

**Future Option B (not active for MVP):**
- Action cards may contribute to future Inspiration budget (e.g., bonus to the upcoming Song's `BaseInspiration`).
- Must include strict caps and tradeoffs.
- Data model supports additional timing modes (e.g., `Always`, `OnlyDuringSong`) for future experimentation.

### Phase 2 --- Composition (Player Turn)

-   Player spends Inspiration to play Composition cards.
-   Cards populate tracks and define the upcoming Part.
-   Player confirms and starts performance.

### Phase 3 --- Performance (Song Loops)

-   Part loops multiple times.
-   Each loop:
    -   Updates SongHype.
    -   Computes impressions.
    -   Generates Inspiration.

### Phase 4 --- Song End Conversion

-   Convert SongHype + impressions into Vibe deltas.

### Phase 5 --- Audience Turn

-   Each audience member executes one ability.

Repeat until Gig resolution.

------------------------------------------------------------------------

## 7. Conversion Rules (Conceptual)

### Loop → SongHype

    LoopScore → BaseΔSongHype
    ΔSongHype = BaseΔSongHype * (1 + Flow * FlowMultiplier) → clamp

### Song End → Vibe

    BaseVibe = SongHype01 * BaseMultiplier
    ImpressionFactor = AvgImpression * ImpressionMultiplier
    VibeDelta = BaseVibe + ImpressionFactor

### GigScore

    GigScore = Σ(SongHypeFinal)

Exact constants remain tunable.

------------------------------------------------------------------------

## 8. Card Taxonomy (Design ↔ Data Model Mapping)

This section exists to prevent drift between **design language** and the **current code model**.

### 8.1 Canonical Terms (use these in docs)

**Domain** *(code: `CardPayload.Domain`)*
- `Action | Composition`

**ActionTiming** *(Action-only; code: `ActionCardPayload.actionTiming`)*
- `BetweenSongsOnly` — **MVP**
- `Always` — future (actions playable outside Between Songs)
- `OnlyDuringSong` — future (actions playable only while a Song is running)

> The enum is intentionally extensible. MVP rules constrain authoring and playability.

**InspirationCost** *(code: `CardDefinition.inspirationCost`)*
- Cost to play the card.
- **MVP:** Action = 0 (Option A); Composition spends Inspiration.

**InspirationGenerated** *(code: `CardDefinition.inspirationGenerated`)*
- **Composition:** per-loop Inspiration budget contribution (becomes available **next loop**).
- **Action (MVP):** must be 0.
- **Action (future):** may contribute to *future budget* (e.g., upcoming Song `BaseInspiration`) with caps/tradeoffs.

**Synergy Axis / Preferences**
- **CardType** *(code: `CardDefinition.cardType`)* = `CHR | TCH | EMT | SFX | None`  
  `SongMod` is **deprecated** and should be removed.
- **Audience Preferences** are expressed in terms of these axes (and can evolve into “Preference Bundles” later).

**Composition Role Tags**
- Track role + style bundle live in `TrackActionDescriptor` (`role`, `styleBundle`) for Composition cards.

**Targets & Target Selection**
- There is no single “Targets” field that must be authored for every card.
- Target selection is a **derived property** of the payload (and sometimes an override):
  - **Action:** derived from `CharacterActionData.ActionTargetType` (with optional manual override).
  - **Composition:** derived from `primaryKind==Track` OR any `PartEffect` with `EffectScope.TrackOnly`.

**Effects**
- **Action:** atomic lists of `CharacterActionData` (+ optional `CardConditionData`).
- **Composition:** `primaryKind` + `trackAction` / `partAction` + `modifierEffects` (list of `PartEffect`).

---

### 8.2 Composition Cards

**Domain:** `Composition`  
**Playable:** during **Phase 2 — Composition**  
**Cost:** spends `InspirationCost`

Core responsibilities:
- Build musical structure (Parts + track roles).
- Provide modifiers (`PartEffect`) that influence LoopScore / impressions / other loop-level signals.
- Contribute to Inspiration pacing via `InspirationGenerated` (applied at loop end → available next loop).

**Targeting rule:**
- Requires selecting a musician/track when `primaryKind==Track` or when any effect is `TrackOnly`.

---

### 8.3 Action Cards

**Domain:** `Action`  
**Playable (MVP):** **Phase 1 — Between-Songs Action Window**  
**Cost (MVP Option A):** `InspirationCost = 0`  
**Limit:** **1 Action per musician per Song**

Core responsibilities:
- Tactical modulation (statuses, positioning, crowd management, tension, requests).
- Not primary musical structure.

**Timing flexibility (future):**
- `Always` and `OnlyDuringSong` exist to support future exploration of in-song actions, but are **not part of MVP constraints**.


------------------------------------------------------------------------



## 9. MVP Base Card Set (Initial Validation Kit)

### Composition Cards (8)

1.  Kick Pattern (Rhythm, low cost, stable inspiration gain)
2.  Pocket Line (Bass, stabilizes LoopScore)
3.  Chord Bed (Harmony, boosts structure)
4.  Hook Riff (Melody, boosts impressions)
5.  Vocal Phrase (Vocals, CHR synergy)
6.  Tight Fill (Rhythm, TCH synergy)
7.  Raw Burst (EMT synergy, volatile impressions)
8.  Build-Up (High impact, higher cost)

### Action Cards (4)

1.  Wink --- Soft crowd manipulation
2.  Joke --- CHR-aligned crowd interaction (risk vs critics)
3.  Tune Up --- Gain Composure (MVP: apply to band-wide via helper)
4.  Callout --- Positional / targeting manipulation

This set validates: - Inspiration economy - Loop feedback - SongHype →
Vibe conversion - Audience reaction pressure

------------------------------------------------------------------------

## 10. Archetype Design Direction (Non-Final)

These mechanics support later skeleton decks for:

-   **Frontman** --- Spotlight risk/reward, audience targeting
-   **The Pocket** --- Stability, tempo smoothing
-   **Engine / Heart** --- Power generation, risk scaling
-   **Texture** --- Perception manipulation, interference

Archetype systems must plug directly into: - Inspiration flow - SongHype
manipulation - Audience reactions - Stress pressure

------------------------------------------------------------------------

## 11. Milestone Breakdown

### Short Term (MVP Validation)

-   Finalize this contract as canonical.
-   Remove Groove references from docs (anti-drift).
-   **Add Flow + Composure baseline (Status Effects).**
-   Implement MVP card set (including at least one Flow generator and one Composure generator).
-   Add a tiny validation slice:
    -   2 test enemies (one Stress-focused, one anti-Flow / anti-Hype)
    -   UI readouts for Flow (Song) and Composure (per musician; MVP may mirror band-wide)
-   Validate:
    -   Inspiration pacing
    -   SongHype readability (and that Flow meaningfully changes it per loop)
    -   Audience pressure clarity (and that Composure provides a tactical buffer)
-   Only after the slice passes: create first skeleton decks per archetype (minimal).

### Medium Term (System Expansion)

-   Expand status effects.
-   Add more audience archetypes.
-   Introduce limited Action → Inspiration interactions (Option B).
-   Refine archetype identity mechanics.
-   Add early class evolution hooks.

### Long Term (Depth & Expression)

-   Deep archetype specialization trees.
-   Advanced audience behaviors and crowd dynamics.
-   Meta-progression integration.
-   Procedural deck evolution.
-   Narrative-driven class evolution.

------------------------------------------------------------------------

## 12. Non-Goals (For Now)

-   Full balance tuning.
-   Monetization or economy loops.
-   UI polish.
-   Multiplayer concerns.
-   Content scale.

------------------------------------------------------------------------

## 13. Glossary

-   **Inspiration:** Global energy resource per song.
-   **SongHype:** Structural quality meter.
-   **Vibe:** Audience engagement / HP.
-   **GigScore:** Aggregate performance score.
-   **Loop:** Repeated evaluation cycle.
-   **Part:** Musical configuration block.
-   **Action Card:** Non-musical tactical card.
-   **Composition Card:** Musical construction card.
-   **Flow:** Song/Band-scoped stackable that amplifies Loop → SongHype conversion for the active Song.
-   **Composure:** Musician-scoped stackable that absorbs incoming Stress before it is applied.
-   **Band-wide targeting helper (MVP):** Authoring/runtime shortcut that applies a Musician-scoped status to all musicians to simplify early balance.

------------------------------------------------------------------------

End of document.

---

# Part B — Tactical Addendum (Positioning, Targeting, Blocking)

> **Scope note (MVP):** Everything in Part B is **optional / out of scope** for the first playable MVP, unless explicitly enabled.
> The MVP can ship with “no positioning” (or a simplified version) while still validating economy (Inspiration → SongHype → Vibe) and pressure (Stress).

This addendum exists to preserve the valuable tactical concepts from the legacy `GigCombat.md` document
(stage layout, targeting vocabulary, Tall/Blocking), without reintroducing economy drift.

---

## B1) Stage Layout and Positioning

The stage is divided into **rows/lanes** for both the Band and the Audience.

### Band Positions
- Each musician can occupy **Front** or **Back**.
- Position can modify card effectiveness (design intent):
  - **CHR**-leaning effects are often stronger in the **Front Row**.
  - **TCH/EMT** may be neutral or gain different contextual bonuses (future tuning).

### Audience Rows
- Audience members can be arranged in multiple rows (Front → Back).
- Audience members may move between rows (e.g., some types drift forward every N turns).
- Some mechanics check **row location**, especially **Blocking** interactions.

---

## B2) Blocking and the Tall Mechanic

While **Vibe** is the core audience outcome, it is applied at **Song End** (see Part A).
However, many games benefit from *tactical targeting constraints* during the performance and action windows.
Blocking is one such constraint.

### Blocking Logic (concept)
- Some audience members are marked as **Tall**.
- If a **Tall** audience member stands in front of another, it may **block or reduce** effectiveness against enemies behind.

### Scaling Variants (future)
A blocking system may support different scaling types (conceptual list):
- `FlatReduction`
- `PercentageReduction`
- `RangeLimited`
- `CustomFormula`
- `StatusBased`

### Keyword hook
- Cards/effects may gain `BypassesBlocking` to ignore Tall obstruction.

---

## B3) Advanced Conditions and Targeting (vocabulary)

Cards may include advanced condition/targeting concepts such as:

- `OnlyIfFrontRow`
- `OnlyIfUnblocked`
- `TargetBackRow`
- `TargetTallEnemy`
- `PushEnemyBack`
- `PullEnemyForward`

### Targeting Flow (UI intent)
1) Select performer (musician)
2) Validate conditions (position / state)
3) Resolve targeting mode (manual vs auto)
4) Show **preview** (e.g., which targets are affected, whether they are blocked)

> Note: The contract does **not** require previews for MVP, but this is a strong UX target.

---

## B4) Combat-Related Keywords (quick reference)

| Keyword | Effect |
|---|---|
| `Advance` | Move a musician forward one row |
| `Retreat` | Move a musician back one row |
| `BypassesBlocking` | Ignores Tall enemies' obstruction |
| `OnlyIfFrontRow` | Requires performer to be in the front row |
| `TargetBackRow` | Prioritizes enemies in the back row |
| `PushEnemyBack` | Moves target enemy to the rear row |
| `PullEnemyForward` | Moves target enemy to the front row |

---

## B5) Legacy cleanup notes (to prevent drift)

- **Groove** is deprecated. The only energy currency is **Inspiration** (Part A).
- Legacy status lists (Inspired/Weakened/etc.) are removed from the baseline; MVP uses **Flow** and **Composure** only.


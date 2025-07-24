# Gig Class - A Long Way to the Top

## Overview

A **Gig** is a key encounter in *A Long Way to the Top*, where the Band performs songs in front of distinct audience groups (enemies) with the goal of winning them over through Vibe. Each Gig challenges the player's strategic use of songs, cards, and musician abilities across a limited number of turns.

---

## Base Identity

| Field          | Type    | Description |
|----------------|---------|-------------|
| `Name`         | string  | Title of the Gig (e.g., "Café Nebula") |
| `Venue`        | string  | Description of the location/planet |
| `Difficulty`   | int     | Difficulty rating or tier |
| `SongCount`    | int     | Number of songs (i.e., turns) required |
| `Waves`        | list    | Configuration of audience/enemy groups across songs |

---

## Gig Turn Phases

Each song turn within a Gig follows three clear phases:

### 1. Player Phase

- Select the Song to be Performed this turn. Song tooltips display projected Vibe output and eny other effects such as Stress impact.

- Play **Action Cards** (generate Groove, apply buffs/debuffs, mitigate stress).
- Assign **Musicians** to perform.
- Use Modifier Cards to prepare the upcoming song.

- **UX: Cards show a preview of expected outcome if played.**
- **UX: Show “Preferred” or “Disliked” tags on cards when hovering over enemies. Enemies pulse when a card they like is being hovered/played.**

- **(IDEA) One card per musician per turn**. This would force the player to make important choices regarding the use of certain cards with higher synergies with specific musicians.

### 2. Song Performance Phase
- The selected song is performed.
- Card Modifiers are resolved.
- Audience Enemies receive **Vibe** based on total performance.
- Band Score increases.
- Stress is applied depending on musician exertion and difficulty.

### 3. Audience (Enemy) Phase
- Each enemy activates its behavior from its **Action Pool**:
  - Apply **Stress** to musicians.
  - Inflict **Debuffs** or Status Effects.
  - Trigger **Requests** (i.e., song demands).
  - **Buff** allies or themselves.

---

## Failure Conditions

- You **do not need to defeat all enemies** to succeed, but defeated enemies add bonus Score and rewards.
- Songs **must** be performed each turn; failure to prepare yields poor results.
- Failed gigs will affect **Cohesion**. If **Band Cohesion** drops to 0, the Band breaks up, **Game Over**.

---

## Player Responsibilities During a Gig

1. Select and modify a Song for each turn.
2. Distribute Action cards among Musicians.
3. Track Groove generation and usage.
4. Manage individual **Musician Stress**.
5. Fulfill or ignore **Enemy Requests**.
6. Keep **Band Cohesion** healthy.
7. Target enemies' **Vibe** (HP).
8. Strategically decide which enemies to focus on and which to ignore.
9. Monitor cumulative **Gig Score** for progression and rewards.

---

## Resource Economy

| Resource       | Source                     | Usage |
|----------------|----------------------------|-------|
| `Groove`       | Gained via Action Cards, enhanced by Inspired status | Spent to play cards |
| `Stress`       | Gained via enemy attacks, complex performances | Limits card plays, lowers Song performance |
| `Band Score`   | Increased via successful Song Performance | Used for Fan Gain and Rewards |
| `Vibe`         | Inflicted on Enemies via Song modifiers, card effects | Fill each **enemy’s individual Vibe bar** to "convince" them |
| `Cohesion`     | Lost via unresolved conflicts or high stress | Determines Band integrity and survival |

---

## Gig Design Guidelines

- Use **Song Count** to control battle duration.
- Combine Enemies with complementary actions to create tension.
- Ensure at least:
  - 1 Stackable Buff effect in play (e.g., Calm, Inspired)
  - 1 Stackable Debuff to mitigate (e.g., Weakened, Frustrated)
  - 1 Enemy Request mechanic
  - Opportunities to both generate and spend Groove
  - Musicians at risk of high Stress if poorly managed

---

## Player Onboarding & Strategic Clarity

Gigs in A Long Way to the Top require mastering both performance and persuasion. To reduce early cognitive load and promote learning through play, the following guidelines shape player introduction:

# Core Gig Objectives
* Perform a set number of songs, choosing one per turn.
* Convince all visible Enemies by filling their individual Vibe bars.
* Manage Stress, Groove, and Cohesion.

# Key Concepts to Introduce Early
* Groove: Resource used to play cards.
* Card Types: Funny, Raw, Technical, etc. These define both stat synergy and enemy preferences.
* Vibe Bars: Each enemy has their own. Filling it “defeats” them.
* Stress: Increases with conflict or enemy actions. Breakdown is a real threat.
* Gig Score: Cumulative performance score; contributes to rewards but not always victory.

# Suggested Complexity Curve

|Phase  |Introduce|
|-------|---------|
|Gig 1  | Groove, Stress, Card Types, Single Song per turn, One Enemy with Vibe bar |
|Gig 2–3  | Multiple enemies, simple Requests (Funny/Raw), Song Modifier Cards |
|Gig 4–5  | 	Status Effects, Song Popularity consequences, Minor Conflicts |
|Midgame  | Empire-aligned enemies, Bosses with multi-stage mechanics |
|Late game | Song Traits in Requests, Strategic enemy targeting, multi-turn synergies |

# Communicating Systems Clearly

* Icons or hover-tooltips for card type affinities (e.g. 🎭 = Funny)
* Color-coded Enemy Preferences
* Clear status readouts for Groove, Stress, Vibe, Score

# What the Player Has to Keep Track Of

|Element  | Why It Matters  |
|---------|-----------------|
|Card Cost & Type  | Groove economy and stat synergy |
|Musician Stress  | Influences song performance |
|Song Popularity  | Can trigger enemy effects (good or bad) |
|Vibe per Enemy  | Progress tracker for Gig success |
|Enemy Preferences  | Such as Card Types, Song synergie, etc |
|Card synergy | Powerful combos can shift the battle |
|Song Modifier Slots  | Decide which bonuses to bring |
|Remaining Songs  | Total turns available |

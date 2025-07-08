# 🎤 Gig Class - A Long Way to the Top

## 🎪 Overview

A **Gig** is a key encounter in *A Long Way to the Top*, where the Band performs songs in front of distinct audience groups (enemies) with the goal of winning them over through Vibe. Each Gig challenges the player's strategic use of songs, cards, and musician abilities across a limited number of turns.

---

## 🪪 Base Identity

| Field          | Type    | Description |
|----------------|---------|-------------|
| `Name`         | string  | Title of the Gig (e.g., "Café Nebula") |
| `Venue`        | string  | Description of the location/planet |
| `Difficulty`   | int     | Difficulty rating or tier |
| `SongCount`    | int     | Number of songs (i.e., turns) required |
| `Waves`        | list    | Configuration of audience/enemy groups across songs |

---

## 📆 Gig Turn Phases

Each song turn within a Gig follows three clear phases:

### 🎸 1. Player Phase
- Play **Action Cards** (generate Groove, apply buffs/debuffs, mitigate stress).
- Assign **Musicians** to perform.
- Use Modifier Cards to prepare the upcoming song.

### 🎼 2. Song Performance Phase
- The selected song is performed.
- Card Modifiers are resolved.
- Audience Enemies receive **Vibe** based on total performance.
- Band Score increases.
- Stress is applied depending on musician exertion and difficulty.

### 😠 3. Audience (Enemy) Phase
- Each enemy activates its behavior from its **Action Pool**:
  - Apply **Stress** to musicians.
  - Inflict **Debuffs** or Status Effects.
  - Trigger **Requests** (i.e., song demands).
  - Buff allies or themselves.

---

## 💥 Failure Conditions

- If **Band Cohesion** drops to 0 → the Band breaks up, **Game Over**.
- You **do not need to defeat all enemies** to succeed, but defeated enemies add bonus Score and rewards.
- Songs **must** be performed each turn; failure to prepare yields poor results.

---

## 🧠 Player Responsibilities During a Gig

1. Select and modify a Song for each turn.
2. Distribute Action cards among Musicians.
3. Track Groove generation and usage.
4. Manage individual **Musician Stress**.
5. Fulfill or ignore **Enemy Requests**.
6. Keep **Band Cohesion** healthy.
7. Target high-priority enemies with Vibe when needed.
8. Strategically decide which enemies to focus on and which to ignore.
9. Monitor cumulative **Gig Score** for progression and rewards.

---

## 💰 Resource Economy

| Resource       | Source                     | Usage |
|----------------|----------------------------|-------|
| `Groove`       | Gained via Action Cards, Inspired status | Spent to play cards |
| `Stress`       | Gained via enemy attacks, complex performances | Limits card plays, lowers Song performance |
| `Band Score`   | Increased via successful Song Performance | Used for Fan Gain and Rewards |
| `Vibe`         | Inflicted on Enemies via Song modifiers, card effects | Fill each enemy’s individual Vibe bar to "convince" them |
| `Cohesion`     | Lost via unresolved conflicts or high stress | Determines Band integrity and survival |

---

## 📚 Gig Design Guidelines

- Use **Song Count** to control battle duration.
- Combine Enemies with complementary actions to create tension.
- Ensure at least:
  - 1 Stackable Buff effect in play (e.g., Calm, Inspired)
  - 1 Stackable Debuff to mitigate (e.g., Weakened, Frustrated)
  - 1 Enemy Request mechanic
  - Opportunities to both generate and spend Groove
  - Musicians at risk of high Stress if poorly managed

---

## 🔗 Relationship to Other Systems

- Pulls from `Song.md`, `Card.md`, `Musician.md` for turn logic and performance outcome.
- Enemies are defined externally in `Enemy.md`, but referenced per Wave.

---
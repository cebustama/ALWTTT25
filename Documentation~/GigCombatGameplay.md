# Gig Combat Gameplay - A Long Way to the Top

This file documents the **moment-to-moment tactical gameplay** during Gigs in *A Long Way to the Top*. It focuses on combat mechanics such as stage positioning, targeting, action resolution, Vibe calculation, and the Audience phase.

---

## Sections
- [1. Stage Layout and Positioning](#1-stage-layout-and-positioning)
- [2. Player Phase](#2-player-phase)
- [3. Song Performance Phase](#3-song-performance-phase)
- [4. Audience Phase](#4-audience-phase)
- [5. Vibe, Blocking, and the Tall Mechanic](#5-vibe-blocking-and-the-tall-mechanic)
- [6. Advanced Conditions and Targeting](#6-advanced-conditions-and-targeting)
- [7. Combat-Related Keywords](#7-combat-related-keywords)

---

## 1. Stage Layout and Positioning

The stage is divided into **rows or lanes**, both for the Band and the Audience.

### Band Positions
- Each Band Member can be positioned in **Front** or **Back**.
- Card effectiveness depends on position:
  - `Charisma (CHR)` cards are more effective from the **Front Row**.
  - `Technique (TCH)` and `Emotion (EMT)` cards may have neutral or alternate bonuses.

### Audience Rows
- Audience members are also arranged in rows.
- Enemies can move between rows (e.g., forward every 2 turns).
- Some mechanics check enemy **row location**, especially those affected by the `Tall` trait.

---

## 2. Player Phase

During the Player Phase, the following occurs:

1. **Song Selection** – Choose one Song to perform.
2. **Action Card Play** – Use pre-song Action Cards to apply buffs, debuffs, generate Groove.
3. **Assign Performers** – Choose which Band Members play which cards.
4. **Target Audience** – Based on card targeting logic (manual, automatic, random, AoE).
5. **Stage Position Adjustments** – Use cards or abilities to move Band Members (Advance, Retreat).

---

## 3. Song Performance Phase

- The selected Song is played.
- All attached **Song Modifier Cards** resolve.
- Performance calculates:
  - **Vibe dealt** to Audience members.
  - **Stress** gained by Musicians.
  - **Score** contributed to the Gig.

Modifiers affected by:
- Musician Stats
- Position (e.g., Front Row CHR bonuses)
- Status Effects (Inspired, Weakened, Hyped, etc.)
- Song Traits and Theme synergies

---

## 4. Audience Phase

Each Audience Member (Enemy) performs one or more actions:

- Apply **Stress** to Band Members
- Inflict **Debuffs** or Status Effects
- Trigger **Requests** (e.g., "Play something Funny or I Boo!")
- React to past performance (e.g., Cheer, Boo, Chant)
- Execute **Movement** (e.g., advance to front row)

Enemy behaviors may be affected by:
- Song Popularity
- Card Types played
- Band’s Empire Alignment
- Musician Status Effects

---

## 5. Vibe, Blocking, and the Tall Mechanic

Vibe is the primary "damage" dealt to Audience enemies.

### Blocking Logic
- Some enemies are marked as `Tall` ([see Enemy.md](Enemy.md#positional-traits-tall-and-blocking))
- If a **Tall** enemy stands in front of another, it **blocks or reduces** Vibe to enemies behind.

#### Block Scaling Types
See `EnemyBlockScalingType` enum for variations:
- `FlatReduction`
- `PercentageReduction`
- `RangeLimited`
- `CustomFormula`
- `StatusBased`

> Cards or effects may gain the keyword `BypassesBlocking` to ignore this mechanic.

---

## 6. Advanced Conditions and Targeting

Card effects may include advanced targeting logic:
- `OnlyIfFrontRow`
- `OnlyIfUnblocked`
- `TargetBackRow`
- `PushEnemyBack`
- `TargetTallEnemy`

### Card Targeting Flow
1. Select Performer
2. Evaluate card conditions (stat requirements, status)
3. Resolve targeting mode (manual, random, auto)
4. Show Vibe preview and Block indicators

---

## 7. Combat-Related Keywords

| Keyword | Effect |
|---------|--------|
| `Advance` | Move Band Member forward one row |
| `Retreat` | Move Band Member back one row |
| `BypassesBlocking` | Ignores `Tall` enemies' obstruction |
| `OnlyIfFrontRow` | Requires Band Member to be up front |
| `TargetBackRow` | Prioritizes enemies in back row |
| `PushEnemyBack` | Moves target enemy to rear row |
| `PullEnemyForward` | Moves target enemy to front row |

> These keywords influence how effects resolve and how cards interact with the stage layout.

---

## Next Steps
- Integrate combat positioning effects into Card config structure.
- Ensure SFX cards can manipulate rows and visibility.
- Sync targeting logic with new `IBlockEffect` system from `Enemy.md`.
- Add visual indicators for position, blocking, and status during Gig gameplay.

---

This file will evolve alongside combat system implementation. Please update it with every new mechanic that affects row positioning, visibility, targeting, and Vibe resolution.

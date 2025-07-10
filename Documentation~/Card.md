# 🃏 Card Class - ALWTTT

## 🪪 Base Identity
- `Name`: *string* – The card's title
- `Phase`: *enum* – Indicates when the card is used: `ActionPhase` (before song) or `ModifierPhase` (on song)
- `Rarity`: *enum* – `Common`, `Rare`, `Epic`, `Legendary`

---

## 🎶 Core Mechanics

| Attribute        | Type            | Description |
|------------------|------------------|-------------|
| `GrooveCost`     | int              | Groove required to play the card |
| `GrooveGenerated`| int              | Groove generated (used only for Action cards) |
| `Types`          | List<VibeType>   | Tags like `Funny`, `Touching`, `Callout`, `Technical`, `Raw` |
| `Keywords`       | List<Keyword>    | Mechanics like `Innate`, `Exhaust`, `Retain`, `Combo`, `Calm`, `Stacking` |
| `Conditions`     | List<Condition>  | Usage requirements, e.g., “If current song is Pop” or “Played by CHR > 8” |
| `Effects`        | List<Effect>     | Card actions: Apply Hype, boost Song Score, reduce Stress, apply StatusEffect, etc. |

## Card Types

- Card Types can also be referenced by Enemies and used to fulfill Requests. Understanding card types is central to understanding Enemy behavior and designing strategies.

| Type name | Musician Synergies | Song Themes |
|-----------|--------------------|----------------|
| Funny | CHR | - |
| Virtuous | TCH | - |
| Touching | EMT | - |
| TODO | CHR, TCH | - |
| TODO | CHR, EMT | - |
| TODO | TCH, EMT | - |
| Raw | EMT, High Stress | - |

- Card Types could use a unique color for fast identification, also highlighted in Enemy Preferences with the same color.

## Card Keywords

| Keyword | Description |
|-|-|
| Improvise | Discard and choose from pool of cards with low-cost or no cost. Reduces frustration from bad hands or unmet enemy Requests. |

---

## 🔗 Synergies

### 🎸 Musician Stats
- `CHR` strengthens `Funny`, `Callout` type cards (audience focus)
- `TCH` improves `Technical`, `Solo` cards (precision and power)
- `EMT` enhances `Touching`, `Emotional`, `Raw` cards (impact and buff duration)

### 🎼 Song Stats
- **Genre**: Some cards are stronger when matching the song’s Genre
- **Theme**: Certain cards may unlock bonus effects or evolve if aligned
- **Complexity**: High-Complexity songs may increase risk/reward of some Modifier cards

### 🧠 Status & Combo System
- Stackable effects like:
  - `Calm`: Absorbs next X Stress
  - `Inspired`: Boosts Groove gain next turn
  - `Tuned`: Reduces Groove Cost
  - `Weakened`: Reduces next Effect output
  - `Hyped`: Multiplies next song’s Vibe impact

---

## ✅ Example Fields Summary

```csharp
class Card {
    string Name;
    CardPhase Phase; // ActionPhase or ModifierPhase
    Rarity Rarity;
    int GrooveCost;
    int GrooveGenerated;
    List<VibeType> Types; // Funny, Touching, Technical, etc.
    List<Keyword> Keywords; // Innate, Exhaust, etc.
    List<Condition> Conditions;
    List<Effect> Effects;
}
```

Cards are the tactical expression of the band’s musical and emotional abilities. Their careful use determines whether a gig turns into legend or disaster.
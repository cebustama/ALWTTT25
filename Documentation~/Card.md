# Card Class - ALWTTT

## Base Identity
- `Name`: *string* – The card's title
- `Phase`: *enum* – Indicates when the card is used: `ActionPhase` (before song) or `ModifierPhase` (on song)
- `Rarity`: *enum* – `Common`, `Rare`, `Epic`, `Legendary`

---

## Core Mechanics

| Attribute        | Type            | Description |
|------------------|------------------|-------------|
| `GrooveCost`     | int              | Groove required to play the card |
| `GrooveGenerated`| int              | Groove generated (used only for Action cards) |
| `Types`          | List<PersonalityType>   | Tags like `Funny`, `Touching`, `Callout`, `Technical`, `Raw` |
| `Keywords`       | List<Keyword>    | Mechanics like `Innate`, `Exhaust`, `Retain`, `Combo`, `Calm`, `Stacking` |
| `Conditions`     | List<Condition>  | Usage requirements, e.g., “If current song is Pop” or “Played by CHR > 8” |
| `Effects`        | List<Effect>     | Card actions: Apply Hype, boost Song Score, reduce Stress, apply StatusEffect, etc. |

- Conditions could be strict and tied to spefic Songs, SFX or other actions by musicians

## Card Types

### Genre Card Packs
Cards correspond to a specific set of Genre cards, starting with: Rock, Jazz and Pop. Cards can be upgraded and move from one Genre to another. For example, a Rock card can be transformed into a Post-Rock card, modifying the effects and unlocking the new sub-genre.

### Action Cards

- Card Types can also be referenced by Enemies and used to fulfill Requests. Understanding card types is central to understanding Enemy behavior and designing strategies.

| Type name | Musician Synergies | Song Themes |
|-----------|--------------------|----------------|
| Funny | CHR | - |
| Virtuous | TCH | - |
| Touching | EMT | - |
| Raw | EMT, High Stress | - |

- Card Types could use a unique color for fast identification, also highlighted in Enemy Preferences with the same color.

## SFX Cards - Stage Effects - Action Card Thematic Subtype

SFX Cards represent **stagecraft and production effects** — *lighting, projections, fog, pyrotechnics, holograms, etc*.

They are **ActionPhase cards** played before a song to modify the upcoming performance's atmosphere, enhance stats, trigger conditional buffs, or interact with enemy preferences.

These cards simulate the **theatrical aspect of a concert**, giving the player more strategic control over how a performance is perceived.

- **Conditions**:	Optional. Can be related to Song Stats (e.g. *“Only if Complexity: High”*) or Venue Type (e.g. *Size, Capacity, Open Air vs Closed, Gravity vs No-Gravity, etc*)

- **Effects**:	Target the *next Song Phase*: buff stats, alter Vibe output, trigger enemy reactions, boost card types. Can also affect Enemy actions, for example "Fog" could reduce Sniper-Type enemies' accuracy, Strobes can leave enemies stunned, etc. 

- **Synergies**: 
Enhance specific Card Types (Funny, Raw, Technical, etc.). Reward Song Traits or Themes. Manipulate enemy reactions (Cynics hate Pyro, Moshers love Fog). Temporarily alter the ModifierSlot.

#### Ideas
| Name | Effect |
| - | - |
| Laser Barrage | +2 CHR to all CHR based Synergies |
| Holographic Backdrop | Adds +1 temporary Modifier Slot to next song |
| Rolling Fog | +3 EMT to all EMT based Synergies |
| Controlled Pyro Finale | Multiply final Vibe output by 1.25x |
| Silent Darkness | All musicians gain Calm; +1 Groove next turn |

#### Design Guidelines

- Limit number of SFX cards usable per gig (e.g. 1–2 per gig) Optional: introduce a new resource or slot system (e.g. “Stage Slots”) for pre-song planning

- Enemies may develop preferences or triggers based on visible SFX (TODO: integrate with Enemy.Trigger and Enemy.Reactions)

- Can be obtained as permanent **Gear**, or as one-time-use **Consumables**

### Song Modifier Cards


## Card Keywords

| Keyword | Description |
|-|-|
| Improvise | Discard and choose from pool of cards with low-cost or no cost. Reduces frustration from bad hands or unmet enemy Requests. |

---

## Synergies

### Musician Stats
- `CHR` strengthens `Funny`, `Callout` type cards (audience focus)
- `TCH` improves `Technical`, `Solo` cards (precision and power)
- `EMT` enhances `Touching`, `Emotional`, `Raw` cards (impact and buff duration)

### Song Stats
- **Genre**: Some cards are stronger when matching the song’s Genre
- **Theme**: Certain cards may unlock bonus effects or evolve if aligned
- **Complexity**: High-Complexity songs may increase risk/reward of some Modifier cards

### Status & Combo System
- Stackable effects like:
  - `Calm`: Absorbs next X Stress
  - `Inspired`: Boosts Groove gain next turn
  - `Tuned`: Reduces Groove Cost
  - `Weakened`: Reduces next Effect output
  - `Hyped`: Multiplies next song’s Vibe impact

---

## Example Fields Summary

```csharp
class Card {
    string Name;
    CardPhase Phase; // ActionPhase or ModifierPhase
    Rarity Rarity;
    int GrooveCost;
    int GrooveGenerated;
    List<PersonalityType> Types; // Funny, Touching, Technical, etc.
    List<Keyword> Keywords; // Innate, Exhaust, etc.
    List<Condition> Conditions;
    List<Effect> Effects;
}
```

## Card Targeting Flow

In *A Long Way to the Top*, card targeting is a two-step process that reflects the performative nature of a concert. Each card represents a musical or performative action that must be assigned to a **Musician**, and then—depending on the card's targeting logic—either to specific **Audience Members** (Enemies) or automatically applied.

---

### Step 1: Select the Performer (Musician)

- Every card must be assigned to a **Musician** in the Band.
- The **Musician's stats** (Charm, Technique, Emotion) directly influence the **effectiveness** of the card based on its type (e.g., *Funny*, *Touching*).
- Some cards can only be played by certain Musicians due to conditions or instrument synergy.
- This step always occurs **before** target resolution.

**Example:**
> The player selects the highly Charismatic vocalist to perform a *Funny* card that targets a cynical audience member.

---

### Step 2: Resolve Targeting (Manual or Automatic)

Targeting behavior is defined by the card's `TargetingMode` or `ActionTargetType`. Once a **Performer** is selected, the game proceeds to resolve the effect:

#### **Manual Targeting**
- The player is prompted to select a valid target.
- Used for cards targeting **specific audience members**.
- Targets are highlighted based on card synergies (e.g., enemies who are weak to `Funny` will glow).

#### **Automatic Targeting**
- No manual input needed after Performer selection.
- Effects are resolved on:
  - All enemies (`AllAudience`)
  - A random enemy (`RandomAudience`)
  - The Performer or Band itself (`Self`, `Band`)

---

### Targeting Modes

| Mode               | Description                                                   |
|--------------------|---------------------------------------------------------------|
| `ManualAudience`   | Player selects an audience member to receive the effect       |
| `AutoAllAudience`  | All audience members receive the effect                       |
| `AutoRandomAudience` | One random enemy is affected                                |
| `Self`             | The effect is applied to the selected Musician                |
| `Band`             | The entire Band is affected (e.g., buffs, Groove gain)        |
| `NoTarget`         | Card has an immediate effect, such as drawing cards or SFX    |

> Cards may include Conditions that restrict targeting. For instance, a card may only be playable if the current Song is a Ballad, or if the performer's EMT is 8+.

---

### Design Goals

- Reinforce the *gig-as-combat* metaphor with clear performative flow.
- Allow players to make meaningful choices by assigning the *right card to the right Musician*.
- Enable enemy synergies, reactions, and resistances based on **who** played the card and **who** was targeted.

---

### Example Turn Flow with Targeting

1. Player drags a *Touching* card from their hand.
2. UI prompts: “Select a Musician to perform this card.”
3. Player selects the keyboardist with high Emotion (EMT).
4. Game highlights audience members who are susceptible to *Touching*.
5. Player clicks a *Wallflower*-type enemy.
6. Card is played. The effect is resolved based on EMT and synergies.
7. Enemy reacts based on Tags (e.g., *Cheer* if liked, *Boo* if disliked).

---

### Implementation Notes (Technical)

- Targeting logic should be defined in a `TargetResolver` system, decoupled from card display logic.
- Cards can specify their `TargetingMode` as a field.
- Manual targeting can be skipped entirely if `TargetingMode` is automatic.
- Effects should always resolve *after* performer selection.

---

Cards in ALWTTT are not just spells or actions — they are **performances**. The Targeting Flow ensures that each card becomes a dynamic interaction between **Musician**, **Audience**, and **Stage**.

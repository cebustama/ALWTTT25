# Band Class - ALWTTT

## Base Identity
- `BandName`: *string* – The name of the band, chosen or generated.
- `StartingGenre`: *enum* – Determines starting deck and available cards (e.g., Rock, Jazz, Pop).

---

## Members
- `Members`: List of `Musician` objects.
  - Each with stats: Charm (CHR), Technique (TCH), Emotion (EMT), Creativity (CRT), Leadership (LDR), Stress (STS), and more.

---

## Songs
- `Songs`: List of `Song` objects available for gigs.
  - Songs are required to perform.
  - Obtained via composing, resolving conflicts, or events.

---

## Deck of Cards
- `Deck`: Main gameplay mechanic during Gigs.
  - **Action Cards** – Played between songs (support, tactics). Generate **Groove**.
  - **Song Modifier Cards** – Played during songs (tempo, solos, etc). Costs **Groove**.

Enemy units have a required amount of **Vibe** for defeat, like **HP** in other games. Cards and Song Perfomances can inflict **Vibe** onto enemies to defeat them.

Starting deck is defined by chosen **Genre**.
Rewards during the run are pulled from genre-specific pools.

---

## Fans and Level
- `Fans`: EXP equivalent, increases as gigs are completed and reputation grows.
- `BandLevel`: Derived from `Fans`, used for **Progression* including:
  - New cards
  - Perks
  - Relics
  - Band traits
  - Genres or characters

---

## Cohesion
- `Cohesion`: Acts as Band-wide HP.
  - Dropped by stress, conflict, bad gigs, or event outcomes.
  - **If it reaches 0, the band breaks up and the run ends.**

Replenished by:
- Positive events
- Gig synergies and final results
- Rehearsal actions

---

## World State & Travel
- `CurrentLocation`: Current planet or node on the map.
- `UpcomingGig`: Data for the next planned performance.
- `ShipState` (idea): Could hold facilities (deck view, rest lounge, upgrades, etc.)
- `EmpireAlignmentScore`: Tracks the Band's history and reputation with the empire. Narrative consequences (i.e. events, cards, upgrades, rewards and punishments)

---

## Optional Features
| Feature         | Use Case |
|-----------------|----------|
| `Equipment`     | Music equipment, Instruments, Amplifiers, Pedals, etc. Tour-wide (playthrough) *persistent* upgrades |
| `Consumables`   | *Temporary* upgrades, for example for a single Gig or single Song Performance |
| `BandTraits` or `BandReputation`  | E.g., “Tight-knit”, “Unpredictable”, “Trendsetters” |
| `RunHistory`    | High scores, endings, performances |

---

## Example Fields Summary

```csharp
class Band {
    string BandName;
    Genre StartingGenre;
    List<Musician> Members;
    List<Song> Songs;
    List<Card> Deck;
    int Fans;
    int BandLevel;
    float Cohesion;
    Location CurrentLocation;
    Gig UpcomingGig;
    int EmpireAlignmentScore;
    List<Equipment> Equipments;
    List<BandTraits> Reputation;
}
```
# 🛰️ Band Class - ALWTTT

## 🪪 Base Identity
- `BandName`: *string* – The name of the band, chosen or generated.
- `StartingGenre`: *enum* – Determines starting deck and available cards (e.g., Rock, Jazz, Pop).

---

## 🎸 Members
- `Members`: List of `Musician` objects.
  - Each with stats: Charm (CHR), Technique (TCH), Emotion (EMT), Creativity (CRT), Leadership (LDR), Stress (STS), and more.

---

## 🎼 Songs
- `Songs`: List of `Song` objects available for gigs.
  - Songs are required to perform.
  - Obtained via composing, events, or rehearsals.

Each `Song` could include:
- `Title`
- `Sections`: Number of phases in performance
- `Mood`
- `Difficulty`

---

## 🎴 Deck of Cards
- `Deck`: Main gameplay mechanic during Gigs.
  - **Action Cards** – Played between songs (support, tactics). Generate **Groove**.
  - **Song Modifier Cards** – Played during songs (tempo, solos, etc). Costs **Groove**.

Enemy units have a required amount of **Vibe** for defeat, like **HP** in other games. Cards and Song Perfomances can inflict **Vibe** onto enemies to defeat them.

Starting deck is defined by chosen **Genre**.
Rewards during the run are pulled from genre-specific pools.

---

## 📈 Fans and Level
- `Fans`: EXP equivalent, increases as gigs are completed and reputation grows.
- `BandLevel`: Derived from `Fans`, used to unlock:
  - New cards
  - Perks
  - Relics
  - Band traits
  - Genres or characters

---

## 💔 Cohesion
- `Cohesion`: Acts as Band-wide HP.
  - Dropped by stress, conflict, bad gigs, or event outcomes.
  - If it reaches 0, the band breaks up and the run ends.

Replenished by:
- Positive events
- Gig synergies
- Rehearsal actions

---

## 🗺️ World State & Travel
- `CurrentLocation`: Current planet or node on the map.
- `UpcomingGig`: Data for the next planned performance.
- `ShipState` (optional): Could hold facilities (deck view, rest lounge, upgrades, etc.)

---

## 🧩 Optional Features
| Feature         | Use Case |
|-----------------|----------|
| `Relics`        | Passive boosts and traits for the band |
| `Inventory`     | Setlists, consumables, instruments (if added) |
| `BandTraits`    | E.g., “Tight-knit”, “Unpredictable”, “Trendsetters” |
| `RunHistory`    | High scores, endings, performances |

---

## ✅ Example Fields Summary

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
    List<Relic> Relics;         // optional
    List<StatusEffect> Status;  // optional
}
```

The Band is the beating heart of the run: it holds your strategy, your story, and your soul.
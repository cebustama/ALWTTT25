# 🎼 Song Class - ALWTTT

## 🪪 Base Identity
- `Title`: *string* – Name of the song (generated or chosen)

---

## 🎶 Musical Attributes

| Attribute        | Type        | Description |
|------------------|-------------|-------------|
| `Genres`         | List<Genre> | Style tags (e.g. Rock, Pop, Funk) that interact with deck synergies and audience types |
| `Themes`         | List<Theme> | Narrative motifs (e.g. Love, Protest, Loss) that influence event interactions and audience resonance |
| `Complexity`     | Enum        | Difficulty to perform (Low, Medium, High); affects failure chance and synergy requirements |
| `Status`         | Enum        | Rehearsal level: `New`, `NotRehearsed`, or `Rehearsed` — affects performance reliability |
| `Popularity`     | Int         | Indicates how well-known the song is, ranging from "Totally Unknown" to "All-Time Classic". Affects Gig Score, Hype potential, and triggers enemy reactions (e.g., Fanboys love popular songs, Haters boo them). Popularity may evolve across the run, increasing with successful gigs or viral events. High popularity might attract Empire surveillance or shift audience archetypes. |
| `ModifierSlots`  | Int         | Number of Song Modifier Cards that can be attached before performing the song |

---

## 🧠 Song Creation – Modular Composition Minigame
// TODO: Maybe move this to Rehearsal.md

Songs are most often created during **Rehearsal Days** via the **Compose** action, using a roguelike-style minigame. The process includes:

### 🎵 **Song Idea Generation**
- At the start of a Rehearsal, some Musicians may gain a **Song Idea** (visualized with a lightbulb or other icon).
- You may choose to pursue an idea or skip it.
- You can **consider up to 3 ideas per Rehearsal** before losing the opportunity.

### 🧩 **Composition Flow**
If an idea is selected:
1. **Choose a Theme** (from available pools or a hybrid, e.g., Hope, Breakup, Rebellion)
2. **Choose a Genre** (from available pools or a hybrid)
3. **Assign Musician(s)** to contribute based on `Creativity (CRT)`
   - Higher CRT yields more options, higher Popularity, or better stats
4. **Structure Selection** (Short vs Long, ModifierSlots count, Mood impact; from available pools or a hybrid)
5. **Final Song Generated**
   - Output includes: `Title`, `Genres`, `Themes`, `Complexity`, `Status`, `ModifierSlots`, `Popularity`, and optional **Traits**

---

## 🎭 Other Sources of Songs

| Source           | Result |
|------------------|--------|
| **Conflict Resolution** | A unique song reflecting the emotional resolution of a disagreement |
| **Random Events**       | Drafts, lost recordings, memory triggers leading to song ideas |
| **Practicing Other Songs** | Can spark new inspiration mid-rehearsal |

---

## ✨ Optional Song Traits (Advanced Layer)
Add uniqueness and passive effects to songs. Can be referenced in events or used in Request (i.e asking for a Fan Favourite Song)

| Trait            | Effect |
|------------------|--------|
| `Fan Favourite` | If any Enemy is a Fan, inflict bonus **Vibe** or **Effects** |
| `Crowd Pleaser` | Bonus Hype when played |
| `Difficult Solo` | Adds Groove cost to performance |
| `Anthemic`       | Buffs band morale when played |
| `Raw`            | More effective when Stress is high |

Traits can also affect card interactions and **Groove**/**Vibe** scaling.
Traits can also be referenced by enemies in Requests or Conditional Effects.

## Empire Alignment
Songs can be either Anti Empire, Pro Empire, or Neutral, all of which hold narrative consequences during the **Band**'s playthrough.
Could also be implemented as a Trait instead (i.e. either Pro Empire or Anti Empire, being neutral if neither is included).

---

## ✅ Example Fields Summary

```csharp
class Song {
    string Title;
    List<Genre> Genres;
    List<Theme> Themes;
    ComplexityLevel Complexity;
    RehearsalStatus Status;
    int Popularity;
    int ModifierSlots;
    List<SongTrait> Traits;
}
```

Songs are the emotional core of ALWTTT’s gigs — strategic choices that reflect the band’s growth, conflicts, and audience mastery.
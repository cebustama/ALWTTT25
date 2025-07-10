
# 👥 Enemy Class - A Long Way to the Top

## 🪪 Base Identity

| Field       | Type        | Description |
|-------------|-------------|-------------|
| `Name`      | string      | Name of the concertgoer/enemy |
| `Type`      | enum        | Enemy archetype (e.g., *Fanboy*, *Cynic*, *Hater*, *Wallflower*, *Critic*, *Wildcard*) |
| `VibeGoal`  | int         | Amount of Vibe needed to fully "convince" this enemy |
| `PersonaTags` | list<CardType> | Preferred Card Types: *Funny*, *Touching*, *Technical*, *Raw*, etc. |
| `Triggers`  | list<Trigger> | Reactions to CardTypes, Song stats, or Player Actions |
| `Actions`   | list<EnemyAction> | Behaviors executed each Audience Phase |

---

## Enemy Synergy Design Philosophy

- Enemy preferences align with existing card types (Funny, Raw, Touching, etc.), which relate to Musician stats (CHT, TCH, EMT)
- Enemies can also respond to Song Popularity, Themes or the Band's Empire Alignment

## 🎭 Personality Tags & Synergies

Enemies now **react** to the same **Card Types** that players use:

| Enemy Type  | Behavior Example |
|-------------|------------------|
| **Fanboy**     | Loves *Popular* Songs, gives bonus Vibe if favored Genre is used |
| **Cynic**      | Rejects *Touching* or *Raw* cards; appreciates *Technical* or *Funny* ones |
| **Hater**      | Instantly boos *High-Popularity* Songs; lowers Cohesion if ignored |
| **Kid**        | Throws *Tantrums* (stress attacks), but loves *Funny* cards |
| **Wallflower** | Hard to reach without *Emotion*-based cards (*Touching*, *Raw*) |
| **Critic**     | Likes complex songs and *Technical* cards, punishes poor execution |
| **Mosher**     | Boosts Vibe when *High-Tempo* or *Energetic* performances are played |

---

## 📣 Concertgoer Reactions (Keywords)

Enemies can trigger **concert-style reactions**:

| Keyword   | Description |
|-----------|-------------|
| `Cheer`   | Temporarily boosts Band Score or Groove gain |
| `Clap`    | Applies *Inspired* to one Musician |
| `Boo`     | Applies *Stress* or *Weakened* to Band |
| `Mosh`    | Boosts Audience Vibe intake or buffs other enemies |
| `Heckle`  | Applies *Frustrated* or reduces Vibe gained next turn |
| `Chant`   | Causes a delayed Request that must be fulfilled or penalized |

These reactions are triggered via card type matches, unmet Requests, high Combo chains, or event triggers.

---

## 🎭 Triggers

Each enemy has a set of **Triggers**, which respond to specific in-game events:

| Trigger Type | Example |
|--------------|---------|
| `CardType Played` | “If *Funny*, Cheer” |
| `Song Popularity` | “If >80 Popularity, Boo” |
| `Ignored Request` | “If not fulfilled by next song, apply -2 Cohesion” |
| `StatusEffect` | “If Musician is *Inspired*, target them with Heckle” |

---

## 🌀 Enemy Actions

Enemies act during the **Audience Phase** in each Gig Turn:

### Action Categories:

1. **Attacks (Stress or Debuff)**
   - e.g., *Tantrum*: +3 Stress to a random Musician
2. **Buffs**
   - e.g., *Encore*: +1 Vibe gain from all Cheer reactions
3. **Debuffs / Status Effects**
   - e.g., *Critique*: Apply `Frustrated` to top-performing Musician
4. **Requests**
   - Asks for a *Genre*, *Theme*, *CardType*, or *SongStat*
   - e.g., “Play something *Funny* or I walk!” — If fulfilled, +5 Vibe
5. **Narrative Options / Special Interactions**
   - Can inject event-style choices, offering a bonus card with unique effect or decision-driven outcome

---

## ✅ Example Enemy Summary

```csharp
class Enemy {
    string Name;
    EnemyType Type; // Fanboy, Hater, Critic, etc.
    int VibeGoal;
    List<CardType> PersonaTags; // Funny, Raw, Technical, etc.
    List<Trigger> Triggers;
    List<EnemyAction> Actions;
}
```

---

## 🧠 Design Philosophy

- **Shared vocabulary** (CardTypes, StatusEffects) reinforces player understanding across systems.
- **Concertgoer reactions** (Cheer, Boo, Chant) add emotional texture to Gigs.
- Enemy **Types** and **Tags** guide simple heuristics for players ("Use Funny cards to win over Kids").
- **Trigger-based design** supports emergent strategy and card synergy discovery.

---

## 📚 Guidelines for Designing Enemies

- Tie each Enemy's **Vibe preferences** to one or more **CardTypes** the player is already familiar with.
- Use **contrasting enemy types** to create friction (e.g., Hater + Fanboy in same gig).
- At least 1 Enemy per Gig should trigger **Audience Reactions** for flavor.
- Avoid too many layered mechanics early on—build complexity across Acts.


# A Long Way to the Top – Audience & Song Composition System Summary

## 1. Audience System Overview
The audience replaces traditional enemies. Each spectator has emotional states, affinities, and a VIBE bar instead of HP. Player aims to convince them emotionally through music.

### Key Elements
- **VIBE**: Main metric; fills when spectator enjoys the music.
- **Actions**: Future behaviors shown via icons (ovation, booing, distraction).
- **Preferences**: Genre, mood, lyrical theme, complexity, performer bias.
- **States**: Hyped, Bored, Hostile, Resonating, Distracted, Chanting.
- **Contagion**: VIBE spreads between spectators.

### How They React
Music evaluation is based on:
- Affinity to musical attributes
- Execution quality
- Synergies between musicians
- Contextual factors (tempo, time signature, mood, theme)

---

## 2. Card-Based Song Composition

Players compose songs live using cards.

### Structure
Song → Parts → Tracks (one per musician) → Procedural MIDI events.

### Card Types
- **Instant**: Immediate impact (clean hostility, boost hype).
- **Sustained**: Stay active during a part.
- **Mutators**: Change structure (intros, solos, key changes, tempo).
- **Post-Processors**: Affect execution (swing, humanization).
- **Thematic**: Set lyrical themes like Love, Party, Injustice.

---

## 3. Integration Between Song & Audience

Each spectator evaluates the music every beat/part:

```
ΔVIBE = Σ_tracks[
   BasePerTrack × Affinity × Context × Synergy × Exposure × Execution
] − Penalties
```

### Synergy Examples
- **LockGroove**: Bass + drums create powerful groove.
- **Spotlight**: Highlighted musician amplifies emotional impact.
- **ThemeChain**: Repeating a lyrical theme boosts momentum.
- **Progressive Pulse**: Odd meters + complexity reward certain spectators.
- **Unity Encore**: Political theme + climax can trigger massive applause.

---

## 4. Core Resources

| Resource | Description |
|---------|-------------|
| **VIBE** | Convincing spectators |
| **Momentum** | Show energy, boosts combos |
| **Stress** | Affects execution quality |
| **Complexity** | Higher risk, higher reward |
| **Synergy State** | Active combos |

---

## 5. Game Loop

1. **Compose** – Player plays cards to build song structure.
2. **Perform** – Song is executed; spectators react.
3. **Adjust** – Player responds with new cards.
4. **Finalize** – Evaluate show success, get rewards.

---

## 6. Conditions & Risks

- **Momentum** increases with coherent/expressive music.
- **Errors** penalize performance and increase stress.
- **Coherence** boosts thematic/mood synergy.
- **Variety** prevents boredom.
- **Emotional feedback loop** affects both players and audience.

---

## 7. Gameplay Examples

### Example 1: "Groove Revolution"
- **Cards**: Kick the Beat, Slap Bass Groove, Voice of the Streets, Tempo Surge, Unity Speech
- **Effects**: LockGroove + ThemeChain (Injustice)
- **Outcome**: High-energy groove; some audiences love it, mainstream dislikes.

### Example 2: "Dream Sequence"
- **Cards**: Ethereal Synth Pad, Soft Drumming, Ballad of the Heart, Humanize Timing
- **Effects**: Spotlight Vocal, Theme Love
- **Outcome**: Emotional ballad; resonates with sensitive spectators.

### Example 3: "Fractured Time"
- **Cards**: Odd Meter Groove, PolyBass Riff, Noise Wall Synth, Solo Mutator
- **Effects**: Progressive Pulse (7/8 + complexity)
- **Outcome**: Critics love it, casual audience confused.

---

## 8. Executive Summary (Pitch)

*A Long Way to the Top* is a musical roguelike where the player battles not monsters but **the hearts of the audience**.  
Songs are composed live using cards that represent artistic decisions—riffs, solos, lyrics, tempo shifts.  
The audience reacts dynamically to the structure and emotion of each song part, and strategic synergies between musicians, styles, and themes create unique emergent compositions.  
Every concert becomes an expressive, strategic performance where **music IS the combat**.


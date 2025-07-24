# Musician Class - ALWTTT

## Base Identity
- `ArtistName`: *string*
- `InstrumentType`: *enum or string* (optional)

---

## Core Gig Stats
Stats that directly influence gig combat performance:

| Stat         | Code | Description                                     |
|--------------|------|-------------------------------------------------|
| Charm        | CHR  | Audience engagement / crowd control             |
| Technique    | TCH  | Skill, timing, and execution                    |
| Emotion      | EMT  | Expressiveness and resonance with the crowd     |

These stats directly affect Card Effects when played by a Musician.

---

## Mental Durability: Stress
- `Stress` (`STS`): Builds during gigs, conflicts, or negative events.
  - If Stress reaches its max, the musician suffers a **Breakdown**.
  - Breakdowns apply semi-permanent debuffs (e.g., *Stage Fright*, *Insecurity*).

> Rehearsal **Band Talk** can reduce Stress as well as resolve conflicts.

---

## Status Effects
- `StatusEffects`: List of **stackable, turn-based effects**.
- Inspired by *Slay the Spire* / *Monster Train* mechanics.
  - Some decay at End of Turn (EOT)
  - Some are spent to block damage or trigger actions
  - Examples: `Calm`, `Anxious`, `Inspired`, `Weakened`, `Panicked`, `Hyped`

---

## Meta Stats

| Stat         | Code | Description |
|--------------|------|-------------|
| Creativity   | CRT  | Rate of generating new song ideas during **Compose** Rehearsal Action |
| Leadership   | LDR  | Governs team coordination, **Band Talk** effectiveness, and Leadership Style |
| Talent       | TLT  | *(Hidden)* Modifier for XP gain, event triggers, and potential arcs |

---

## Conflict System

Conflicts occur between musicians and come in levels:

| Level             | Effects                                                            |
|------------------|--------------------------------------------------------------------|
| Light Disagreement | Prevents Stress recovery                                          |
| Tension             | Passive Stress gain, moderate Cohesion loss                      |
| Feud                | Large Stress gain, disables Practice for affected members        |
| Blood Feud          | Major penalties, chance of walkouts, locks cards                 |

Each Musician can track a `ConflictLevel` with other bandmates.
Conflict can also affect the Musician's performance during gigs (e.g. refusing to play, skipping turns or misfiring cards).

---

## Design Summary

- **CHR/TCH/EMT** define gig performance identity.
- **Stress** + **Breakdown** introduce emotional tension.
- **StatusEffects** add tactical roguelike layering.
- **Leadership** governs social and strategic dimensions.
- **Creativity** + **Talent** support growth and synergy.
- **ConflictLevel** drives interpersonal story arcs.
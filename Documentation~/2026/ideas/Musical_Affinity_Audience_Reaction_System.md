# Musical Affinity & Audience Reaction System (Draft SSoT)

## Overview

This document defines a lightweight design for a **Musical Affinity /
Audience Reaction System** in *A Long Way to the Top (ALWTTT)*.\
The goal is to capture meaningful cultural resonance between musical
styles and audiences (similar in spirit to elemental systems like
Pokémon), **without exposing complexity to the UI or discouraging
creative genre mixing.**

The system is intentionally **felt, not explained** to the player.

------------------------------------------------------------------------

## Core Design Intent

### Design Goals

-   Preserve intuitive cultural logic:
    -   Rock audiences tend to resonate with Metal.
    -   Punk audiences may resist Jazz.
-   Encourage experimentation and hybrid styles.
-   Avoid hard counters, visible multipliers, or explicit type tables.
-   Communicate only emotional outcomes to the player.
-   Allow long-term cultural change and narrative consequences.

### Non‑Goals

-   No visible "genre effectiveness" charts.
-   No numerical genre modifiers shown to the player.
-   No requirement for players to min-max genre matchups.

------------------------------------------------------------------------

## Player-Facing Mental Model

From the player's perspective:

-   A concert is a **battle against the audience's mood.**
-   The player only sees:
    -   Audience reactions (animations, colors, text).
    -   A single abstract meter (e.g., *Crowd Energy*).
    -   Short narrative feedback after the show.
-   Genre logic is learned implicitly through play, not tutorials.

Example feedback: \> "They weren't expecting this... but something
clicked."\
\> "The crowd looked confused at first, then curious."\
\> "Not their scene tonight."

------------------------------------------------------------------------

## Hidden System Model (Internal Only)

### Audience Profile

Each audience has an internal cultural profile:

-   Preferred Genres (weighted)
-   Resistant Genres (weighted)
-   Openness (how fast attitudes can change)
-   Curiosity (how much novelty is tolerated)
-   Cultural Memory (persistent changes over time)

These values are never directly exposed.

### Song Profile

Each song/card contains: - One or more genres (weighted blend) - Energy
/ Intensity - Attitude / Emotional tone - Special tags (experimental,
traditional, provocative, etc.)

### Resolution Logic (Simplified)

Internally: - Song genre blend is compared to audience profile. -
Resonance increases immediate crowd energy. - Friction reduces immediate
impact but may increase curiosity or long-term openness. - Contextual
actions (performer actions, stage effects, narrative history) can
transform friction into intrigue.

Externally: - Player only sees emotional reactions and outcome pacing.

------------------------------------------------------------------------

## Genre Mixing Philosophy

Mixing genres is never directly punished.

Instead: - Familiar elements create fast momentum. - Unfamiliar elements
create slower momentum but generate: - Curiosity - Reputation shifts -
Future receptiveness - Narrative consequences

This allows: - Short-term losses that create long-term gains. -
Risk-taking to be meaningfully rewarded over time. - Cultural evolution
inside the game world.

------------------------------------------------------------------------

## UI & Communication Principles

### Golden Rule

> If a mechanic cannot be communicated emotionally, it should not appear
> in the UI.

### Visible UI Elements

-   Single crowd state meter (e.g., Cold → Engaged → Hyped)
-   Crowd animations and sound reactions
-   One-line emotional feedback text
-   Post-show narrative summary

### Hidden UI Elements

-   No genre percentages
-   No effectiveness numbers
-   No affinity tables
-   No explicit counters

------------------------------------------------------------------------

## Pros & Cons

### Pros

-   Preserves depth without UI complexity.
-   Encourages creativity and experimentation.
-   Aligns with the artistic message of musical exploration.
-   Supports long-term narrative evolution.
-   Reduces onboarding friction for players.

### Cons / Risks

-   Harder to debug and balance internally.
-   Player learning curve relies on intuition rather than clarity.
-   Some players may desire more transparency.
-   Requires careful tuning of feedback language and pacing.

Mitigation: - Strong playtesting. - Rich qualitative feedback writing. -
Internal debug visualization tools (developer-only).

------------------------------------------------------------------------

## Technical Specification (High Level)

### Data Structures (Conceptual)

AudienceProfile - preferredGenres : Dictionary\<Genre, Weight\> -
resistedGenres : Dictionary\<Genre, Weight\> - openness : float -
curiosity : float - culturalMemory : PersistentState

SongProfile - genreBlend : Dictionary\<Genre, Weight\> - energy :
float - attitude : Enum - tags : List`<Tag>`{=html}

### Runtime Flow

1.  Song played.
2.  Compute resonance and friction internally.
3.  Update hidden audience state.
4.  Convert result into:
    -   Crowd meter delta
    -   Animation state
    -   Feedback text key
5.  Persist long-term cultural memory if thresholds reached.

### Debug Mode (Developer Only)

-   Optional overlay showing internal scores.
-   Logging of resonance vs friction.
-   Tuning tools for designers.

------------------------------------------------------------------------

## MVP Scope Recommendation

Phase 1: - Single dominant genre per song. - Single dominant genre per
audience. - Simple resonance vs friction calculation. - Emotional
feedback only.

Phase 2: - Multi-genre blending. - Persistent audience memory. -
Narrative consequences.

Phase 3: - Planetary cultural shifts. - Emergent musical movements.

------------------------------------------------------------------------

## Status

Draft --- conceptual alignment document.\
Intended to evolve into a formal SSoT once integrated with CardEffects
and Audience systems.

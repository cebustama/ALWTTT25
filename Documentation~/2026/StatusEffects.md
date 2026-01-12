# ALWTTT --- Status Effects System (CSO)

## Purpose

This document defines the **Status Effects system** used in *A Long Way
to the Top* (ALWTTT).\
It describes how abstract gameplay effects are modeled, authored,
catalogued, and applied at runtime.

The system is intentionally designed to:

-   Separate **gameplay semantics** from **presentation and theme**.
-   Enable fast iteration through data rather than code.
-   Avoid duplicated mechanics under different names.
-   Remain stable as content scales.
-   Serve as a shared mental model for designers and engineers.

The authoritative ontology lives in:

> **ALWTTT_CSO_StatusEffects_Primitives_with_References.md**

This document focuses on how the ontology is used inside the game.

------------------------------------------------------------------------

## Core Concepts

### Character Status Ontology (CSO)

A **Character Status** represents a temporary or persistent modification
applied to a character-like entity (Band, Musician, AudienceMember, NPC,
etc.) that alters:

-   Damage output or intake
-   Survivability and mitigation
-   Tempo and control
-   Resource flow
-   Behavioral constraints

Each status is identified by a stable technical identifier:

    CharacterStatusId

This identifier maps to a canonical primitive in the CSO document and
never encodes theme or UI language.

------------------------------------------------------------------------

### StatusEffectSO (Content Asset)

A **StatusEffectSO** is the concrete, tunable implementation of a CSO
primitive inside ALWTTT.

It defines:

-   `EffectId` (CharacterStatusId) --- must exist in the ontology.
-   `DisplayName` --- thematic, cosmetic, localizable.
-   Stack behavior (additive, capped, refresh, etc.).
-   Decay behavior (per turn, per loop, persistent, etc.).
-   Timing semantics (tick timing, trigger conditions).
-   Optional metadata (tags, flags).

**Important:** - Logic is driven by `EffectId` and behavior
parameters. - DisplayName may change freely without breaking gameplay
logic.

------------------------------------------------------------------------

### StatusEffectCatalogueSO

The **Catalogue** is a registry that contains all StatusEffectSO assets
available in a given context.

Responsibilities:

-   Enforce uniqueness of `EffectId`.
-   Provide fast lookup from `CharacterStatusId → StatusEffectSO`.
-   Serve as the runtime source of truth for effect resolution.
-   Act as a validation boundary for authoring tools.

Characters reference a catalogue in order to resolve effect definitions
at runtime.

------------------------------------------------------------------------

### Runtime Container

Each character owns a **StatusEffectContainer** at runtime.

The container:

-   Stores active status instances.
-   Manages stacks and decay.
-   Evaluates timing ticks.
-   Exposes events for UI and debugging.
-   Answers queries such as:
    -   Is a status active?
    -   How many stacks exist?
    -   Should a status consume on trigger?

This container is the only runtime authority for status state.

------------------------------------------------------------------------

## Applying Status Effects from Cards

### StatusEffectActionData

Cards apply status effects using a data-only structure:

**StatusEffectActionData**

Fields:

-   `EffectId` --- which CSO primitive to apply.
-   `TargetType` --- how targets are selected (ActionTargetType).
-   `StacksDelta` --- how many stacks to add/remove.
-   `Delay` --- optional delay before application.

This structure is intentionally parallel to legacy action data, but is
now fully CSO-driven.

------------------------------------------------------------------------

### CardPayload Integration

All cards carry status effects through:

    CardPayload.StatusActions : List<StatusEffectActionData>

This applies to both:

-   **Action cards**
-   **Composition cards**

There is no separate "status pipeline" anymore --- cards directly
express their gameplay effects as CSO status applications.

------------------------------------------------------------------------

### Runtime Execution

#### Action Cards

When an Action card is played:

1.  Performer and target are resolved by the HandController.
2.  Inspiration cost and generation are processed.
3.  All `StatusActions` are executed:
    -   Targets are resolved using `ActionTargetType`.
    -   The StatusEffectSO is resolved via the StatusEffectCatalogueSO.
    -   Stacks are applied to the target's StatusEffectContainer.

Status effects therefore become the primary mechanical payload of Action
cards.

#### Composition Cards (MVP Behavior)

When a Composition card is played:

-   Its `StatusActions` are currently applied **immediately on card
    play**.
-   This is an explicit MVP simplification and may evolve later into
    loop‑based or part‑based timing.

This behavior is documented intentionally to avoid ambiguity.

------------------------------------------------------------------------

### Targeting Semantics

Target selection is derived exclusively from:

    StatusEffectActionData.TargetType

Only single-target modes require explicit player selection:

-   Musician
-   AudienceCharacter

Group and random targets do not require manual targeting:

-   AllMusicians
-   AllAudienceCharacters
-   RandomMusician
-   RandomAudienceCharacter

Cards may override target requirements manually if needed.

------------------------------------------------------------------------

## Legacy Pipeline (Deprecated)

The previous system based on:

-   CharacterActionData
-   CharacterActionType
-   Legacy StatusType

has been fully removed from the card pipeline.

All new gameplay effects must be expressed exclusively through:

    StatusEffectActionData + CSO

Reintroducing legacy patterns is explicitly discouraged.

------------------------------------------------------------------------

## UI & Debugging Notes

### Current Limitation (TODO)

Action card descriptions currently render **only the first
StatusAction**.

This is a temporary technical limitation.

**TODO (Future):** - Support multi-effect descriptions. - Resolve
StatusEffectSO.DisplayName instead of showing raw EffectId. - Introduce
a clean, dependency-safe way for UI systems to access the
StatusEffectCatalogue without tight coupling or global singletons.

------------------------------------------------------------------------

## Core Minimal Status Set (MVP Reminder)

The current MVP assumes a minimal but expressive set of effects:

-   Flow (DamageUpFlat)
-   Composure (TempShieldTurn)
-   Feedback (DamageOverTime)
-   Exposed (DamageTakenUpFlat)
-   Choke (DisableActions)

This set is sufficient to prototype meaningful gameplay loops while
keeping complexity controlled.

------------------------------------------------------------------------

End of document.

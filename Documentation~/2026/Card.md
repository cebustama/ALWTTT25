# ALWTTT — Card System

## Purpose

Cards are the primary interaction surface between the player and the game systems in *A Long Way to the Top*.

They express:

- Musical intent.
- Tactical decisions.
- Narrative flavor.
- Systemic gameplay effects.

This document describes the conceptual design and technical structure of cards as they exist today.

The focus is intentionally **design-first**, while remaining precise enough to align with implementation.

---

## Card Domains

Cards exist in two distinct domains, determined by the **payload type**.

### Action Cards

- Played during the gig loop (between songs / action windows).
- Represent tactical decisions, crowd interaction, and moment-to-moment control.
- Express gameplay via a list of declarative **Card Effects**.

### Composition Cards

- Played during composition.
- Define or modify the structure of the song (track / part actions + modifiers).
- May also include **Card Effects** (e.g., apply statuses immediately in MVP).

---

## CardDefinition

A **CardDefinition** represents the stable identity and metadata of a card.

Key responsibilities:

- Identity (Id, DisplayName).
- Presentation (Sprite, audio/animation metadata).
- Economy (Inspiration cost / generation).
- Synergies (CardType, keywords, rarity).
- Performer rule (Any musician vs Fixed type).
- Play rules (exhaust after play, targeting override).
- Reference to a **CardPayload** (which defines the mechanical meaning).

**Domain derivation:**

- Action vs Composition is derived from `payload.Domain`.

---

## CardPayload (Base)

All card payloads inherit from **CardPayload**.

Common fields:

- `Effects : IReadOnlyList<CardEffectSpec>`

Implementation detail:

- Stored as `[SerializeReference] List<CardEffectSpec> effects`.
- This is the unified, extensible representation of what a card *does*.

Both Action and Composition cards share this effect model.

---

## ActionCardPayload

ActionCardPayload represents Action-domain cards.

Fields:

- `ActionTiming`
  - When the card can be played.
- `Conditions`
  - Reserved for future gating logic.
- `Effects` (inherited from CardPayload)

Action cards do not execute procedural action code per-card. Their mechanical meaning is expressed through the `Effects` list.

---

## CompositionCardPayload

CompositionCardPayload represents Composition-domain cards.

Composition-specific fields include:

- `PrimaryKind`
- `TrackAction`
- `PartAction`
- `ModifierEffects`
- `RequiresMusicianTarget` (derived from composition semantics)

In addition, Composition cards inherit `Effects` from CardPayload, allowing systemic gameplay effects to be attached to composition choices.

---

## Effect Model (New)

### CardEffectSpec

`CardEffectSpec` is the abstract base type for all card effects.

Rules:

- Specs hold **data only** (no runtime logic).
- Runtime execution is handled by a separate executor/interpreter.
- New effects are added by introducing new `CardEffectSpec` subclasses.

### Built-in Effect Specs (current)

#### ApplyStatusEffectSpec

Applies a specific authored `StatusEffectSO` variant.

Fields:

- `StatusEffectSO status`
- `ActionTargetType targetType` (default: Self)
- `int stacksDelta` (can be negative)
- `float delay` (seconds)

This design supports **status variants** because the effect stores the full StatusEffectSO asset reference, not just an enum ID.

#### DrawCardsSpec

A minimal example of a non-status effect.

Fields:

- `int count`

(Execution may still be TODO depending on runtime integration.)

---

## Status Effects Authoring (StatusEffectSO)

`StatusEffectSO` is ALWTTT's authored gameplay layer for statuses.

It:

- References the CSO primitive via `CharacterStatusId effectId`.
- Provides designer-facing naming (`displayName`).
- Provides gameplay-facing tuning (stacking/decay/tick/value semantics).

### Variants

The **card effect model** (`ApplyStatusEffectSpec.status`) supports variants naturally:

- You may author multiple `StatusEffectSO` assets that share the same `EffectId` but differ in tuning (e.g., Strength vs SuperStrength).

**Important tooling note:** the current `StatusEffectCatalogueSO` implementation assumes **one StatusEffectSO per EffectId** (duplicate detection + first-wins cache). If you want true variant support in the catalogue, it must evolve to a key-based or multi-map structure.

---

## Targeting Model

Target selection is derived primarily from the card's **Effects**.

For MVP, the key rule is:

- `ApplyStatusEffectSpec.targetType` determines whether player targeting is required.

Suggested targeting rules:

- Single-target types require explicit player selection:
  - `Musician`
  - `AudienceCharacter`
- Types that do not require selection:
  - `Self`
  - `AllMusicians`, `AllAudienceCharacters`
  - `RandomMusician`, `RandomAudienceCharacter`

Cards may override targeting explicitly via `CardDefinition.overrideRequiresTargetSelection`.

Composition cards may also require a musician target via `CompositionCardPayload.RequiresMusicianTarget`, independent of effects.

---

## Runtime Execution Pipeline

### Action Cards

1. Player selects a card from hand.
2. HandController resolves:
   - Performer (selected musician).
   - Target (if required).
3. Card runtime executes the `Effects` list:
   - For `ApplyStatusEffectSpec`:
     - Optional delay.
     - Resolve targets based on `targetType`.
     - Apply `StatusEffectSO` with `stacksDelta` to the target's runtime status container.
   - For other effect specs:
     - An executor interprets the spec (some may be TODO during MVP).
4. Card is moved to discard / appropriate pile.

### Composition Cards (MVP)

1. Player plays a composition card.
2. CompositionSession validates placement and applies composition data to the model.
3. `Effects` (if any) may be applied immediately (MVP behavior for systemic effects).

This timing is explicitly MVP and may evolve later into loop-based semantics.

---

## UI Description Behavior (MVP)

Action card descriptions currently render **only the first effect**, prioritizing `ApplyStatusEffectSpec` when present.

**TODO:**

- Multi-effect descriptions.
- Better wording/formatting per effect type.
- Localization-friendly description generation.

---

## Tooling: Card Editor + JSON Pipeline

The Card Editor Window supports a staged authoring flow:

1. Parse JSON.
2. Stage a CardDefinition + Payload in memory.
3. Review/edit in the UI.
4. Create assets (CardDefinition + Payload).
5. Add to catalog.

**Important:** the JSON schema is being migrated to match the new `Effects` model. Because `Effects` is polymorphic via `[SerializeReference]`, the importer must represent effect types explicitly (e.g., a `type` discriminator) or use a custom import strategy.

Legacy schemas that authored `StatusActions` are deprecated in the new model.

---

## Design Philosophy

- Cards express intent, not implementation.
- All gameplay effects are declarative and data-driven.
- Effects are extensible without multiplying code paths.
- Ontology prevents semantic drift while authoring at scale.
- The system favors clarity and composability over micro-optimization.

Cards should feel expressive, readable, and systemically coherent — like musical phrases rather than isolated buttons.

---

End of document.

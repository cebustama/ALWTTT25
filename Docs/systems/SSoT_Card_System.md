# SSoT_Card_System — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Current ALWTTT card gameplay semantics and card runtime model  
**Owns:** card domains, card identity/payload split, effect-first semantics, performer/targeting rules, runtime card behavior  
**Does not own:** editor/import/json workflow contracts (`SSoT_Card_Authoring_Contracts.md`), package-side MidiGenPlay internals

---

## 1. Purpose

This document is the primary authority for what a **card means in ALWTTT gameplay**.

It replaces the previous mixed role of:
- `reference/Card.md`
- portions of `reference/Gig_Combat.md`
- legacy card-model assumptions still visible in older docs

This SSoT defines the current card system in gameplay/runtime terms.

---

## 2. Card domains

Cards exist in two canonical gameplay domains, derived from payload type.

### 2.1 Action cards
Action cards are played in the gig loop's action windows.

They represent:
- crowd interaction
- tactical pressure / relief
- immediate systemic changes
- between-song decisions and control

Action cards express their mechanics through declarative **Card Effects**.

### 2.2 Composition cards
Composition cards are played during composition.

They represent:
- track/part-level musical decisions
- arrangement structure
- composition-specific modifiers
- future loop shaping

Composition cards may also include normal card effects when the game needs composition choices to apply immediate gameplay consequences.

---

## 3. CardDefinition vs CardPayload

### 3.1 CardDefinition
`CardDefinition` is the stable identity and presentation layer of a card.

It is responsible for:
- stable identity (`Id`, display naming)
- presentation metadata
- economy metadata such as cost / generation fields
- synergies (type / keywords / rarity)
- performer rule
- play rules such as exhaust and targeting overrides
- reference to exactly one `CardPayload`

### 3.2 CardPayload
`CardPayload` is the mechanical meaning of the card.

Core contract:
- a `CardDefinition` references exactly one payload asset
- domain is derived from `payload.Domain`
- both Action and Composition cards share the same effect-first base model

---

## 4. The current canonical model is effects-first

All gameplay mechanics authored on a card live under:

```text
CardPayload.Effects : IReadOnlyList<CardEffectSpec>
```

Implementation-facing storage may use:

```text
[SerializeReference] List<CardEffectSpec> effects
```

Meaning of this rule:
- cards are not defined by per-card procedural scripts
- mechanics are represented as declarative specs
- extending the system means adding new `CardEffectSpec` subclasses plus supporting editor/runtime handling

This is the current canonical model.
Legacy action-list language is not primary truth anymore.

---

## 5. Payload types

### 5.1 ActionCardPayload
ActionCardPayload represents Action-domain cards.

Current owned semantics:
- `ActionTiming` controls when the card can be played
- `Conditions` are reserved for gating/requirements
- inherited `Effects` define the actual gameplay outcome

Rule:
- Action cards do not need per-card procedural logic to express their meaning
- their gameplay meaning is the interpreted result of their authored effect list

### 5.2 CompositionCardPayload
CompositionCardPayload represents Composition-domain cards.

Gameplay-facing fields include:
- `PrimaryKind`
- `TrackAction`
- `PartAction`
- `ModifierEffects`
- `RequiresMusicianTarget` when composition semantics require a musician selection

Rule:
- these fields define **ALWTTT gameplay semantics of composition cards**
- they do **not** make ALWTTT the authority over MidiGenPlay package internals

If a composition card references track/bundle/composer structures, ALWTTT owns the gameplay meaning of that choice, while package-internal generation details belong to MidiGenPlay.

---

## 6. CardEffectSpec model

### 6.1 Base rule
`CardEffectSpec` is the abstract base type for card effects.

Rules:
- specs are **data-only**
- runtime logic is handled by an executor/interpreter layer
- new mechanics are added by creating new spec subclasses plus authoring/runtime support

### 6.2 Built-in effect specs currently in active vocabulary
The active effect vocabulary currently includes at least the following concepts:
- status application
- vibe modification
- stress modification
- card draw

Representative built-ins:
- `ApplyStatusEffectSpec`
- `ModifyVibeSpec`
- `ModifyStressSpec`
- `DrawCardsSpec`

### 6.3 ApplyStatusEffectSpec
This effect applies a concrete authored `StatusEffectSO` variant.

Canonical fields:
- `status`
- `targetType`
- `stacksDelta`
- `delay`

Rule:
- a card applies a concrete status asset, not just a primitive enum id
- this allows multiple tuned variants of the same abstract status primitive

---

## 7. Status interaction from cards

Cards may apply statuses as ordinary effects.
That means status application is not a separate parallel card-mechanics system.

Rule:
- status application is just another `CardEffectSpec`
- status runtime semantics themselves belong to the status SSoT when promoted
- card-side meaning stays here

---

## 8. Performer and targeting rules

### 8.1 Performer rule
A card may be:
- playable by any musician
- restricted to a fixed performer type

This belongs to the gameplay identity of the card and therefore lives in ALWTTT card truth.

### 8.2 Effect-driven targeting
Targeting is derived primarily from authored effects.

MVP-facing rule set:
- single-target effect types require explicit target selection
- self / all / random group target types do not require player selection
- card-level overrides may exist when the card definition explicitly forces targeting behavior

### 8.3 Composition targeting
Composition cards may require a musician target independently of their effects if the composition semantics require it.

---

## 9. Runtime execution pipeline

### 9.1 Action cards
Canonical sequence:
1. player selects a card from hand
2. runtime resolves performer and target(s)
3. the card executes its `Effects` list
4. each effect is interpreted by runtime systems
5. the card moves to the appropriate post-play state

### 9.2 Composition cards
Canonical MVP sequence:
1. player plays a composition card
2. composition/session systems validate and apply composition data to the song model
3. authored `Effects`, if any, may apply immediately as systemic gameplay consequences

Important boundary rule:
- ALWTTT owns the gameplay/runtime meaning of playing the composition card
- MidiGenPlay owns package-side implementation details for internal music generation behavior

---

## 10. UI / description behavior

UI wording and description rendering are secondary to the card contract itself.

Current practical rule:
- description generation may lag behind full multi-effect expressiveness
- poor or partial description rendering does not redefine the card contract

If description logic changes, it should reflect this SSoT rather than become a competing source of truth.

---

## 11. Legacy model handling

The project surface still shows legacy `CardData`-style material alongside the newer effect/payload-based model.

Governance rule:
- the **current primary model** is `CardDefinition + CardPayload + CardEffectSpec`
- legacy `CardData`-style material must be treated as:
  - legacy compatibility,
  - transitional coexistence,
  - or archived/superseded material

Legacy material must never silently overrule this SSoT.

---

## 12. Explicit boundaries

### This SSoT owns
- what Action vs Composition means in ALWTTT
- what a card is structurally in gameplay/runtime terms
- how cards express mechanics via effects
- performer and targeting semantics
- the ALWTTT-side meaning of composition-related card choices

### This SSoT does not own
- JSON/editor pipeline details
- catalogue import rules
- package-side composer internals
- lower-level MidiGenPlay algorithm details

Those belong elsewhere even if the same card touches them indirectly.

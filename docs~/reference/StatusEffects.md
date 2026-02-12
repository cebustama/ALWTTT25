# ALWTTT — Status Effects System (CSO) — CardEffects-aligned

> **Update note:** This document is aligned with the **new Card Effects model** (`CardPayload.Effects` + `ApplyStatusEffectSpec`) and the **variants-capable** `StatusEffectCatalogueSO`.
> It replaces older language that assumed cards stored `StatusActions : List<StatusEffectActionData>`.

---

## Purpose

This document defines the **Status Effects system** used in *A Long Way to the Top* (ALWTTT).
It explains how abstract gameplay modifiers are modeled (CSO primitives), authored (StatusEffectSO), organized (Catalogue/Registry), and applied at runtime.

Design goals:

- Separate **gameplay semantics** from **theme/presentation**.
- Keep identifiers stable and JSON/tooling-friendly.
- Avoid duplicated mechanics under different names.
- Support scaling content (many cards, many statuses) without brittle coupling.

---

## 1) Core concepts

### 1.1 Character Status Ontology (CSO) primitives

A **CSO primitive** is an abstract gameplay modifier that can be applied to a character-like entity.

- Canonical identifier: `CharacterStatusId`.
- **Numeric backing values are part of the serialization contract**: never reorder or change existing values; only append new ones.

The primitive id must never encode theme. Theme lives in authored assets (StatusEffectSO).

### 1.2 CSO registry asset (optional but recommended)

`CharacterStatusPrimitiveDatabaseSO` is an editor-facing registry asset that stores a browsable list of ontology entries.

It is used for:

- Validation (does a StatusEffectSO reference a primitive that exists?)
- Designer browsing (category, abstract function, references)

It is *not* the gameplay definition of a status. It’s the ontology manifest.

### 1.3 StatusEffectSO (authored gameplay layer)

`StatusEffectSO` is ALWTTT’s authored, tunable status definition.

It contains:

- **Variant identity (ALWTTT):**
  - `statusKey` (string): stable, human-readable key for this specific variant (unique in a catalogue).
  - `isDefaultVariant` (bool): if multiple variants share the same primitive id, marks the default variant.
- **Primitive identity (CSO):**
  - `effectId : CharacterStatusId`
- **Presentation:**
  - `displayName` (designer-facing; can become a localization key later)
- **Behavior (MVP):**
  - stacking policy (`StackMode`, `maxStacks`)
  - decay policy (`DecayMode`, `durationTurns`)
  - tick semantics (`TickTiming`, `ValueType`)
  - semantic hint (`isBuff`)
- **Validation hook:**
  - optional `primitiveDatabase` reference (used to verify the primitive exists)

Key rule: runtime semantics are anchored to `effectId` + the behavior parameters; `displayName` can change freely.

### 1.4 StatusEffectCatalogueSO (variants + key lookup)

`StatusEffectCatalogueSO` is a registry of available `StatusEffectSO` assets in a given context.

Current responsibilities:

- Provide fast lookup by **human key**: `StatusKey -> StatusEffectSO` (unique).
- Provide variant grouping by **primitive id**: `CharacterStatusId -> List<StatusEffectSO>`.
- Provide a **default** for primitive-based lookups for backwards compatibility:
  - prefer `IsDefaultVariant == true`, otherwise first encountered.

Validation policy:

- Duplicate `StatusKey` is a hard error.
- Multiple variants per primitive id are allowed.
- Multiple defaults for the same primitive id are a warning.

### 1.5 Runtime state: StatusEffectContainer + StatusEffectInstance

At runtime, each character owns a `StatusEffectContainer` which stores only **active** statuses.

- Storage key: `CharacterStatusId` (primitive id).
- Stored value: `StatusEffectInstance`:
  - `Definition : StatusEffectSO` (the authored tuning)
  - `Stacks`
  - `RemainingTurns` (only for duration-based decay)

`StatusEffectContainer` is responsible for:

- applying stacks with the correct stacking policy
- handling decay and timing ticks (`Tick(TickTiming)`)
- explicit consumption for one-shot statuses (`ConsumeOnTrigger`)

#### Important current limitation (variants at runtime)

Because the container is keyed by **primitive id**:

1) A character can only have **one active status per `EffectId`** at a time.

2) If you apply a different **variant** (same `EffectId`, different `StatusEffectSO`) while one is already active, the current code keeps the **first applied `Definition`** and does *not* switch tuning automatically.

Practical consequences:

- Variants are fully supported for authoring (cards can reference different StatusEffectSO assets), but per character, a primitive behaves like a single “slot”.
- If you need “variant switching”, you must either clear the status before applying the new variant, or extend the container to replace the instance definition when applying a different asset.

---

## 2) Applying statuses from cards (Card Effects model)

### 2.1 Cards store effects, not status-actions

Cards express gameplay through a unified list:

- `CardPayload.Effects : IReadOnlyList<CardEffectSpec>`
- Implementation: `[SerializeReference] List<CardEffectSpec> effects`

Status application is **one effect type** among many.

### 2.2 ApplyStatusEffectSpec (status application effect)

`ApplyStatusEffectSpec` represents “apply stacks of a status to targets”.

Fields:

- `StatusEffectSO status` (direct reference to the authored variant)
- `ActionTargetType targetType`
- `int stacksDelta` (can be negative)
- `float delay`

Runtime execution (conceptual):

1) Resolve targets from `targetType`
2) Optionally wait `delay`
3) Apply to each target’s `StatusEffectContainer`:
   - `container.Apply(status, stacksDelta)`

Because the spec stores the **StatusEffectSO reference**, runtime does not need to resolve from the catalogue.

### 2.3 Non-status effects

Other `CardEffectSpec` subclasses represent non-status gameplay effects (e.g., `DrawCardsSpec`).

This is the core reason the model scales: adding “Draw”, “Discard”, “Shuffle Curse”, “Generate”, etc. is introducing new spec types without changing card payload storage.

### 2.4 JSON / tooling contract (recommended)

For JSON import and tooling, you generally want to reference statuses by a stable, human key:

- `statusKey` (string) -> resolve via `StatusEffectCatalogueSO.TryGetByKey`.

Optional fallback (useful for quick prototyping / backwards compatibility):

- `effectId` (int or enum name) -> resolve to **default** variant via `StatusEffectCatalogueSO.TryGet(CharacterStatusId)`.

After import, the staged card should contain a concrete `ApplyStatusEffectSpec` with the resolved `StatusEffectSO` reference.

---

## 3) Targeting model

Target selection is derived from `ApplyStatusEffectSpec.targetType`.

As a rule of thumb:

- **Manual single-target selection** required:
  - `Musician`
  - `AudienceCharacter`
- **No manual selection** (group/random/self):
  - `Self`
  - `AllMusicians`, `AllAudienceCharacters`
  - `RandomMusician`, `RandomAudienceCharacter`

Cards may still override targeting explicitly via `CardDefinition.overrideRequiresTargetSelection`.

---

## 4) Timing, ticks, and decay

`StatusEffectSO` defines how a status behaves over time:

- Stacking policy (`StackMode`, `maxStacks`)
- Decay policy (`DecayMode`, `durationTurns`)
- Tick semantics (`TickTiming`)

The combat loop is responsible for calling:

- `StatusEffectContainer.Tick(TickTiming timing)` at the relevant timing boundaries.

One-shot statuses use `DecayMode.ConsumeOnTrigger` and must be consumed by explicit gameplay hooks via `ConsumeOnTrigger(...)`.

---

## 5) BreakdownState is NOT a Status Effect (v0.2.3 rule)

ALWTTT’s MVP includes the explicit **Stress → Breakdown → Shaken** rule.
This is modeled as a separate runtime variable:

- `BreakdownState: None | Shaken`

This is not a StatusEffectSO / CSO primitive by default, because it has special encounter-level consequences.

**Shaken (BreakdownState) — MVP**

- Trigger: when `Stress >= StressMax` (after Composure absorption)
- Immediate: `Cohesion -1`, set `Shaken`, set `Stress = ceil(StressMax/2)`
- Duration: until end of the next Song
- Rules:
  - cannot play **Action cards** Between-Songs
  - Composure granted to that musician is reduced by **50% (round down)**

If you ever unify it with the status pipeline later, do it deliberately as a special-case primitive with hard-coded rules.

---

## 6) Naming, theme, and drift guardrails

- `CharacterStatusId` is technical and stable.
- `StatusEffectSO.DisplayName` is theme/presentation and may evolve.

To avoid naming drift in docs/UI, maintain a canonical “current name” per primitive (even if you later add variants).

**Current canonical MVP naming (v0.2.3)**

- `DamageUpFlat` → **Flow**
- `TempShieldTurn` → **Composure**

Avoid creating separate primitives for “audience vs musician” versions. Use one primitive and interpret it through the target’s interface.

---

## 7) MVP baseline set (required)

Per the current combat contract, the MVP baseline requires only:

### 7.1 Flow (EffectId: `DamageUpFlat`) — Song/Band

- Scope: Song/Band (active Song)
- Resets: at Song start
- Stacks: integer
- Primary effect: amplifies **Loop → SongHype** conversion

### 7.2 Composure (EffectId: `TempShieldTurn`) — Musician

- Scope: Musician
- Resets: at Song start
- Stacks: integer
- Primary effect: absorbs incoming Stress before Stress increases (Block semantics)

---

## 8) Recommended tier-2 set (optional)

Add only after the MVP slice is stable.

### 8.1 Vulnerable-like (`DamageTakenUpMultiplier`)

- Audience canonical name: **Entranced** (gain more Vibe)
- Musician canonical name: **Exposed** (take more Stress)

### 8.2 Poison-like (`DamageOverTime`)

- Audience canonical name: **Hooked** (end-of-song bonus Vibe per stack; decays)
- Musician canonical name: **Feedback** (loop-start Stress per stack; decays)

### 8.3 Weak-like (`DamageDownMultiplier`)

- Canonical name: **Insecure** (reduces effectiveness)

### 8.4 Disable actions (`DisableActions`)

- Canonical name: **Choke** (hard control window)

---

## 9) UI & debugging notes

- Action card descriptions currently tend to render only the **first effect**, prioritizing `ApplyStatusEffectSpec` when present.
- Preferred UI strings should come from `StatusEffectSO.DisplayName` (not from the enum name).

TODO (future):

- multi-effect descriptions
- consistent formatting per effect type
- localization-friendly description generation

---

## 10) Legacy / deprecations

### StatusEffectActionData

`StatusEffectActionData` (the struct that stores `{ effectId, targetType, stacksDelta, delay }`) is now **legacy** relative to card authoring.

- Cards should not author `StatusActions` lists anymore.
- New work should author `CardPayload.Effects` (with `ApplyStatusEffectSpec` for statuses).

If any older runtime code still reads `StatusActions`, migrate it to interpret `CardEffectSpec` instead.

---

End of document.

# SSoT_Card_Authoring_Contracts — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Authoring, serialization, staged editor workflow, JSON/import contracts, status-variant identity for cards  
**Owns:** how cards and authored statuses are represented and imported  
**Does not own:** combat feel, phase timing, balance, or runtime economy semantics

---

## 1. Purpose

This document is the governed promotion of the previous **Appendix — Authoring & Data Contracts**.

It is the primary authority for:
- card authoring contracts
- `CardDefinition` / `CardPayload` representation rules
- effect-list representation
- JSON import schema rules
- staged editor workflow invariants
- status variant identity used by card authoring/import

---

## 2. Scope boundaries

### 2.1 This SSoT is normative for
- how cards are represented as assets/data
- how card effects are serialized/authored
- how the import pipeline should populate the modern effect list
- how status variants are resolved from authored/imported data

### 2.2 This SSoT is not normative for
- combat pacing and phase order
- loop/song/gig semantics
- balance numbers
- audience/combat feel
- package-side MidiGenPlay internal tooling

Combat/runtime meaning lives in subsystem/runtime SSoTs.

---

## 3. Core representation contracts

### 3.1 CardDefinition vs CardPayload
Contract:
- a `CardDefinition` holds identity/presentation/economy/catalog-facing metadata
- a `CardPayload` holds mechanics
- a `CardDefinition` references exactly one payload asset

### 3.2 Unified effect list
All authored card mechanics live under the effect-first contract:

```text
CardPayload.effects : List<CardEffectSpec>
```

Runtime/editor read access may expose this as `IReadOnlyList<CardEffectSpec> Effects`.

Extension rule:
- adding a new mechanic means adding a new `CardEffectSpec` subclass plus matching editor/import/runtime support

### 3.3 Status application is a normal effect
Status application is authored as a card effect type, not as a separate parallel system.

Canonical example shape:

```text
ApplyStatusEffectSpec {
  StatusEffectSO status;
  ActionTargetType targetType;
  int stacksDelta;
  float delay;
}
```

Rule:
- cards reference a **concrete `StatusEffectSO` asset**
- they do not reference only an abstract primitive id when authored/imported

---

## 4. Status variant identity contract

### 4.1 Problem being solved
Multiple authored statuses may share the same abstract primitive while differing in tuning/config.
A catalogue keyed only by primitive id does not represent this cleanly.

### 4.2 Required governed fix
Each `StatusEffectSO` must support a stable unique variant identifier:

```text
statusKey : string
```

### 4.3 Catalogue requirements
The catalogue must support:
- primary lookup by `statusKey`
- optional grouping by primitive id
- optional default-per-primitive fallback only for backwards compatibility

Validation rules:
- duplicate `statusKey` is a hard authoring/import error
- multiple variants sharing the same primitive id are allowed

---

## 5. JSON schema contract (v2 direction)

### 5.1 Core rule
JSON import must populate **`effects`**.
Legacy shapes like separate `actions` / `statusActions` are not ongoing primary schema.

### 5.2 Enum serialization rule
Use stable string names for enums by default.
Case-insensitive parsing is acceptable if applied consistently.

### 5.3 Minimal top-level card schema
Required:
- `kind`
- `id`
- `displayName`
- `effects`

Optional:
- `description`
- `audioType`
- `rarity`
- catalog-entry defaults and other authoring metadata

### 5.4 Effect object rule
Each effect entry must contain:
- a stable discriminator such as `type`
- only the fields required by that effect type

### 5.5 Example — ApplyStatusEffect
```json
{
  "type": "ApplyStatusEffect",
  "statusKey": "Exposed",
  "targetType": "Self",
  "stacksDelta": 2,
  "delay": 0.0
}
```

Resolution contract:
- `statusKey` resolves via the status catalogue
- unresolved status references must fail import clearly
- optional fallback identifiers may exist, but are not preferred primary authoring paths

### 5.6 Example — DrawCards
```json
{
  "type": "DrawCards",
  "count": 2
}
```

---

## 6. Backwards compatibility policy

WAUC-style rule:
- do not preserve legacy schema branches forever
- if old content needs migration, provide a **one-time conversion path**
- do not let the long-term importer silently support multiple conflicting shapes indefinitely

Practical implication:
- legacy `dto.action.actions`-style content should be converted, not normalized forever inside the active pipeline

---

## 7. Staged CardEditorWindow workflow invariants

The canonical editor workflow is:

```text
Parse -> Stage in memory -> Review/Edit -> Save -> Add to catalog
```

### 7.1 Stage invariants
- staged objects are temporary/in-memory
- nothing writes to disk before Save
- temporary objects should not persist as accidental assets

### 7.2 Save invariants
Save must:
- create the `.asset` files
- wire payload/effect references correctly
- insert the resulting card into the intended catalog or registry

### 7.3 Effects editing rule
The editor must edit the authoritative `effects` collection directly.

Capabilities expected:
- add/remove/reorder effect specs
- show type-appropriate UI per spec
- allow human-friendly status picking while storing the asset reference

---

## 8. Validation rules

Minimum governed validations include:
- duplicate card id detection
- duplicate `statusKey` detection
- unresolved status references are hard failures
- required top-level fields must exist before Save/import succeeds
- catalog insertion must not silently create conflicting entries

---

## 9. Extension rule for new effect types

Whenever a new `CardEffectSpec` subclass is added, update all four layers:
1. data class
2. editor authoring support
3. JSON/import support
4. runtime execution support

A new effect type is not fully integrated until all four exist or the missing pieces are explicitly documented.

---

## 10. Relationship to other docs

- `SSoT_Card_System.md` owns gameplay/runtime card meaning
- `SSoT_Gig_Combat_Core.md` owns combat economy/phase semantics
- a future status SSoT will own deeper runtime status semantics

This document owns the **authoring and data-contract side** only.

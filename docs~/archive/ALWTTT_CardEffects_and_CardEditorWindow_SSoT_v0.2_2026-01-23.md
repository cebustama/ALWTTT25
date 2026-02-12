# ALWTTT — Card Effects + CardEditorWindow Authoring SSoT (v0.2)

**Status:** Draft / Active  
**Last revised (design decisions):** 2026-01-23  
**Scope:** Normative design decisions and contracts for **Card Effects** (data model + JSON contract) and **CardEditorWindow authoring workflow invariants**.  
**Not in scope:** “what we’re doing next / current step / task tracking” → see the **Roadmap (Live)** doc.

**Roadmap (Live):** `ALWTTT_CardEffects_and_CardEditorWindow_Roadmap_Live_v0.1_2026-01-23.md`

---

## 0) Why this SSoT exists

We had multiple documents describing overlapping slices of the same milestone (Card Effects migration + CardEditorWindow + JSON pipeline).  
This SSoT is the **single source of truth for decisions/contracts**.

If something isn’t in this doc, it’s either:
- out-of-scope, or
- implementation detail that may change without changing the contract.

---

## 1) Core contracts (must remain true)

### 1.1 CardDefinition vs CardPayload
- **CardDefinition**: identity, presentation, economy, unlock metadata, catalog entry wiring.
- **CardPayload**: mechanics (“what the card does”).

Contract: a `CardDefinition` references exactly one `CardPayload` asset.

### 1.2 Unified effect list (SOLID/WAUC baseline)
All gameplay mechanics that a card applies are represented by:

- `CardPayload.effects : List<CardEffectSpec>` (polymorphic, `[SerializeReference]`)
- Exposed as `IReadOnlyList<CardEffectSpec> Effects` for runtime/editor read.

Contract: adding new mechanics happens by introducing new `CardEffectSpec` subclasses (data-only) + editor authoring + runtime execution.

### 1.3 Status application is “just another effect”
Status application is authored as an effect type:

- `ApplyStatusEffectSpec { StatusEffectSO status; ActionTargetType targetType; int stacksDelta; float delay; }`

Contract: cards reference a **concrete `StatusEffectSO` variant asset**, not only a primitive id.

---

## 2) Status variants: required identity + catalogue contract

### 2.1 Problem we are solving
We want multiple `StatusEffectSO` variants that share the same CSO primitive (`CharacterStatusId`) but differ in tuning/config:
- example: Strength vs SuperStrength  
- example: “Damage Up Flat” used by multiple systems with different caps/decay

A catalogue keyed only by primitive id cannot represent that cleanly.

### 2.2 Required fix (minimal, stable)
Introduce a stable, unique **variant identifier** on `StatusEffectSO`:

- `statusKey : string` (unique per `StatusEffectSO`)

Catalogue must support:
- Primary lookup by key:
  - `bool TryGetByKey(string key, out StatusEffectSO effect)`
- Optional secondary grouping by primitive id:
  - `IReadOnlyList<StatusEffectSO> GetAllByPrimitive(CharacterStatusId primitiveId)`
- A default-per-primitive rule (only for backwards compatibility; avoid using it in new content):
  - “default” = `IsDefaultVariant` else first encountered.

Validation:
- Duplicate `statusKey` is a hard authoring error.
- Multiple variants per primitive id are allowed.

---

## 3) JSON schema (v2) for Card Effects

### 3.1 Core rule
JSON import must populate **`effects`** (not any legacy “actions/statusActions” shape).

### 3.2 Enum serialization strategy
For JSON, use **stable strings** for enums by default:
- `ActionTargetType` is serialized by enum name (case-insensitive parse).

(If you later decide to use backing ints, do it consistently for *all* enums; the key is consistency.)

### 3.3 Proposed minimal schema
Top-level required fields:

- `kind` : "Action" | "Composition"
- `id` : string (unique card id)
- `displayName` : string
- `effects` : array of effect objects

Optional:
- `description`, `audioType`, `rarity`, etc (depends on your existing importer DTOs)
- `entry` object for catalog insertion defaults.

Effect objects:
- `type` : discriminator string (stable)
- per-type fields.

#### 3.3.1 ApplyStatusEffect
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
- `statusKey` resolves via `StatusEffectCatalogueSO.TryGetByKey`.
- Optional fallbacks (allowed but not preferred): `statusGuid` / `statusPath`
- If the status cannot be resolved: JSON stage fails with a clear error.

#### 3.3.2 DrawCards
```json
{
  "type": "DrawCards",
  "count": 2
}
```

### 3.4 Backwards compatibility policy (WAUC)
- Do not support legacy `dto.action.actions` in perpetuity.
- If migration help is needed, implement a one-time editor conversion tool (separate command), not ongoing schema branching.

---

## 4) CardEditorWindow authoring workflow invariants

### 4.1 The staged pipeline is canonical
**Parse → Stage in memory → Review/Edit (same UI) → Save (create assets) → Add to catalog**

Invariants:
- staging creates temporary `ScriptableObject`s with `HideFlags.DontSaveInEditor`
- nothing writes to disk until Save
- Save creates `.asset` files, wires references, inserts catalog entry

### 4.2 Editing effects in the UI
CardEditorWindow must:
- edit `effects` through `SerializedObject.FindProperty("effects")`
- allow Add/Remove/Reorder
- for ApplyStatusEffect, pick by human-friendly name but store the asset reference

---

## 5) Source-of-truth boundaries (avoid doc collisions)

This SSoT is narrowly scoped to Card Effects + authoring invariants.

For broader system docs:
- **Cards official doc:** `Card.md` (overall card system, domains, runtime execution, targeting rules)
- **Status official doc:** `StatusEffects.md` (CSO primitives + StatusEffectSO semantics and tick/decay rules)

Rule: when content overlaps, the official docs should hold the deeper domain theory, while this SSoT holds the **authoring + data contract decisions**.

---

## 6) Appendix: effect-type extension rule

When adding a new effect type:
1) Add new `CardEffectSpec` subclass (data-only)
2) Add editor drawer support
3) Add JSON schema entry + importer support
4) Add runtime executor support


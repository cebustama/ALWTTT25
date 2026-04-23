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

### 3.4 Action timing and testability

Action cards declare an `actionTiming` field (`CardActionTiming` enum) that gates when the card is legal to play. The default value **excludes `PlayerTurn`**, which means action cards authored without an explicit `actionTiming` are not playable during the standard player turn.

This default is intentional for most authored combat content — action timing is typically declared deliberately per card — but it creates a sharp edge for **testing/debug cards intended to be spawned into the hand via Dev Mode**: without `actionTiming: Always`, the spawned card sits unplayable in the hand regardless of other conditions.

**Convention for testing/debug cards:**  
Any action card authored primarily to exercise runtime behavior (effect validation, status application tests, meter manipulation) via the Dev Mode card spawner must declare `actionTiming: Always` explicitly. This makes the card playable during any phase that permits action-card play.

Cross-reference: Dev Mode gating around card spawn is governed in `SSoT_Dev_Mode.md` §8.4 and §11.4.

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
- `kind` — `"Action"` or `"Composition"`
- `id`
- `displayName`
- `effects`

Optional:
- `performerRule`, `fixedMusician`
- `cardType`, `rarity`, `audioType`
- `inspirationCost`, `inspirationGenerated`
- `exhaustAfterPlay`
- `keywords` — string array of `SpecialKeywords` enum names (e.g. `["Exhaust", "Consume"]`). Case-insensitive parsing. Unknown values emit a warning and are skipped. See §5.8 for coherence rules.
- `overrideRequiresTargetSelection`, `requiresTargetSelectionOverrideValue`
- `cardSpritePath`
- `action`, `composition` — domain-specific payload blocks
- `entry` — catalog-entry defaults (see §5.6)

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

### 5.7 Batch wrapper schema

Multiple cards can be imported in a single JSON payload using a batch wrapper:

```json
{
  "defaultEntry": { "flags": "StarterDeck,UnlockedByDefault", "starterCopies": 2 },
  "cards": [
    { "kind": "Action", "id": "card_a", "displayName": "Card A", ... },
    { "kind": "Action", "id": "card_b", "displayName": "Card B",
      "entry": { "flags": "UnlockedByDefault", "starterCopies": 1 } }
  ]
}
```

**`defaultEntry`** (optional): An `EntryJson` object applied to any card in the batch whose own `entry` block is absent or has empty/null `flags`. Per-card `entry` blocks override the default entirely (not merged field-by-field).

**`EntryJson` fields:**
- `flags` — comma-separated `CardAcquisitionFlags` names (e.g. `"StarterDeck,UnlockedByDefault"`). Synonyms: `"Reward"` / `"Rewards"` → `"RewardPool"`.
- `starterCopies` — integer, default 1. Only meaningful when `StarterDeck` flag is set.
- `unlockId` — string. Required when `UnlockedByDefault` is not set.

**Merge rule:** Unity's `JsonUtility` default-constructs class fields even when absent from JSON. The importer uses `flags` as the discriminator: if a card's `entry.flags` is null or whitespace, the entry is treated as absent and the batch `defaultEntry` is used instead.

### 5.8 Keyword coherence rules

The JSON importer emits non-blocking `Debug.LogWarning` messages when `exhaustAfterPlay` and the `Exhaust` keyword diverge:

- `exhaustAfterPlay: true` without `"Exhaust"` in `keywords` → warning: players won't see an Exhaust tooltip.
- `"Exhaust"` in `keywords` without `exhaustAfterPlay: true` → warning: tooltip says Exhaust but card won't exhaust.

These warnings do not block import. They flag an authoring gap that will be resolved when keywords drive runtime behavior directly (see `SSoT_Card_System.md` §3.3.3).

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

### 7.4 Create wizard defaults

The manual "Create Card + Payload" wizard resets `Kind` to `Action` each time it is opened. This prevents stale serialized state from defaulting to Composition after previous use. The user can switch to Composition during the session.

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

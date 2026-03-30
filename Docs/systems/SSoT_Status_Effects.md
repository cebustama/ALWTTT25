# SSoT_Status_Effects — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Runtime status semantics, catalogue boundary, and canonical MVP status meaning  
**Owns:** what a status is in ALWTTT, how status identity/theme are split, runtime container semantics, tick timing system, and the current canonical status set  
**Does not own:** full card-import/editor workflow (`systems/SSoT_Card_Authoring_Contracts.md`), encounter structure (`systems/SSoT_Gig_Encounter.md`), raw primitive catalog reference (`reference/CSO_Primitives_Catalog.md`)

---

## 1. Purpose

This document is the primary authority for **status-effect truth in ALWTTT**.

It replaces the previous mixed role of:
- `reference/StatusEffects.md`
- status sections embedded in card/combat notes
- primitive-only catalogs that were reference material rather than live authority

---

## 2. Core model

A status in ALWTTT has two layers:

### 2.1 Primitive / stable identity layer
The stable systemic identity of a status must not depend on theme text.

This layer exists so that:
- code/runtime can reason about status meaning,
- serialization/import stays stable,
- different themed assets can still map to the same underlying systemic primitive.

### 2.2 Authored/theme layer
The themed, display-facing, balance-facing representation of a status lives in authored assets such as `StatusEffectSO`.

This layer may control:
- display name
- presentation
- tuning/default values
- duration/stacking metadata
- variant-specific authoring choices

Rule:
- stable identity and themed presentation must not be conflated.

---

## 3. Runtime ownership

The runtime status surface includes:
- `StatusEffectSO`
- `StatusEffectCatalogueSO`
- runtime containers / instances (`StatusEffectContainer` on `CharacterBase.Statuses`)
- catalogue lookup and variant selection by key
- stack application / decay / refresh according to the authored contract

This SSoT owns the gameplay/runtime meaning of that surface.

### 3.1 Tick timing system

Status decay/expiry is driven by a `TickTiming` enum on `StatusEffectSO`. The canonical values in current use:

| Value | Enum name | Meaning |
|---|---|---|
| 1 | `StartOfTurn` | Legacy value — retained for back-compatibility only. Do not use for new statuses. |
| 8 | `PlayerTurnStart` | Ticks at the start of the Player Turn (`GigManager.OnPlayerTurnStarted`) |
| 9 | `AudienceTurnStart` | Ticks at the start of the Audience Turn (`GigManager.AudienceTurnRoutine`) |

**Wiring:**
- Musicians: `TriggerAllStatus` is bound to `GigManager.OnPlayerTurnStarted` via `MusicianBase.BindToGigContext()`. Called at Player Turn start for each musician.
- Audience: `TriggerAllStatus` is called from `GigManager.AudienceTurnRoutine` at audience turn start.

Use `PlayerTurnStart` or `AudienceTurnStart` for all new statuses.

### 3.2 Dual status system (migration note)

The codebase currently has two coexisting status surfaces:

1. **Legacy:** `StatusType` enum + `BandCharacterStats.ApplyStatus(StatusType, int)` — used in some legacy display/state paths (e.g. `OnBreakdown` calls `ApplyStatus(StatusType.Breakdown, 1)` for UI). Not the primary authority.
2. **Current:** `StatusEffectSO` + `StatusEffectContainer` (`CharacterBase.Statuses`) + `StatusEffectCatalogueSO` (`CharacterBase.StatusCatalogue`).

The current runtime model (2) is the governed model. All new statuses must go through the SO + container route. Legacy calls that remain are migration coexistence, not the primary path.

---

## 4. Catalogue and variants

The catalogue-facing contract exists so cards and effects can apply statuses without hardcoding one-off logic.

Canonical rule:
- cards reference a direct `StatusEffectSO` asset in `ApplyStatusEffectSpec.status` (resolved at design time)
- runtime code (e.g. `OnBreakdown`) resolves by string key at runtime via `StatusEffectCatalogueSO.TryGetByKey(key, out so)`
- variants may share a primitive but differ in authored tuning or presentation

`StatusEffectCatalogueSO` keys are case-insensitive and trimmed. Duplicate keys within one catalogue are a hard error (flagged in `OnValidate`).

The `StatusCatalogue` field on `CharacterBase` is Inspector-assigned and optional for card play, but **required** for any runtime code that resolves statuses by key (e.g. `MusicianBase.OnBreakdown` applying Shaken). Musician prefabs must have the catalogue assigned for Shaken application to function.

---

## 5. Canonical MVP status set

### 5.1 Flow
**Primitive:** `DamageUpFlat` (`CharacterStatusId = 100`)  
**Key:** `"flow"`  
**Scope:** Song / Band  
**Tick timing:** Not tick-decayed per turn. Resets at song end via explicit `GigManager` song-end reset.  
**Combat meaning:** amplifies positive Loop → SongHype conversion multiplicatively. Each stack adds `FlowMultiplier` to the multiplier.  
**Applies to:** musicians  
**Validated:** ✅ B3 (Flow stacks boost Vibe per card play), B7 (song-end reset)

### 5.2 Composure
**Primitive:** `TempShieldTurn` (`CharacterStatusId = 400`)  
**Key:** `"composure"`  
**Scope:** Musician  
**Tick timing:** `PlayerTurnStart` — clears at the start of each Player Turn  
**Combat meaning:** absorbs incoming positive Stress before Stress is applied. Consumed first; remainder becomes Stress.  
**Applies to:** musicians  
**Validated:** ✅ B1 (absorbs Stress), B6 (clears at turn start)

### 5.3 Choke
**Primitive:** `DisableActions` (`CharacterStatusId = 700`)  
**Key:** `"choke"`  
**Scope:** Musician  
**Tick timing:** `PlayerTurnStart` — decays each Player Turn start  
**Combat meaning:** stuns the musician (disables actions). `CharacterBase.IsStunned` derives from `DisableActions` stacks when the runtime container is present.  
**Applies to:** musicians only. No audience crowd-control status exists in MVP.  
**Validated:** ✅ B5 (stacks decay after turn)

### 5.4 Shaken
**Primitive:** `ShakenRestriction` (`CharacterStatusId = 503`)  
**Key:** `"shaken"`  
**Scope:** Musician  
**SO config:** Replace, MaxStacks=1, LinearStacks, `AudienceTurnStart` tick, IsBuff=true  
**Tick timing:** `AudienceTurnStart` — expires at the start of the Audience Turn of the following song  
**Duration:** Applied at Audience Turn of Song N → active through Player Turn N+1, Composition N+1, Performance N+1, Song End N+1 → expires at start of Audience Turn N+1. One complete song cycle from application.  
**Combat meaning:** marks a musician as shaken post-Breakdown  
**Applied by:** `MusicianBase.OnBreakdown()` via `StatusCatalogue.TryGetByKey("shaken")`  
**Gameplay restrictions:** open design decision — **not yet enforced in runtime**. Intended restrictions (cannot play Action cards during action window while Shaken; Composure granted is reduced by 50%) are a pending follow-up pass.

### 5.5 Exposed
**Primitive:** `DamageTakenUpFlat` (`CharacterStatusId = 300`)  
**Key:** `"exposed"`  
**Scope:** Musician  
**Tick timing:** not specified — decays per configured SO  
**Combat meaning:** each Exposed stack adds `0.25` to the incoming stress multiplier in `BandCharacterStats.ApplyIncomingStressWithComposure` (`_exposedMultiplierPerStack = 0.25f`).  
**Applies to:** musicians only. No Stress path exists on `AudienceCharacterBase`.

### 5.6 Feedback
**Primitive:** `DamageOverTime` (`CharacterStatusId = 600`)  
**Key:** `"feedback"`  
**Scope:** Musician (MVP); Audience deferred  
**Tick timing:** evaluated during `GigManager.AudienceTurnRoutine`  
**Combat meaning:** each Feedback stack applies 1 incoming stress per audience turn, routed through `m.Stats.ApplyIncomingStressWithComposure`. This means Feedback-triggered Stress respects Composure and can trigger Breakdown.  
**Applies to:** musicians only in current implementation. Audience Feedback DoT requires a Stress path on `AudienceCharacterBase`, which does not exist. Explicitly deferred.

---

## 6. Stacks, duration, and lifecycle

Status behavior may vary by authored configuration, but the following invariants hold:

- statuses may be stack-based
- duration/expiry must be explicit in runtime or authoring semantics
- a status application path must resolve through one canonical runtime route
- multiple variants must not become multiple silent meanings for the same gameplay concept

If a new status changes how stacking/expiry works system-wide, update this SSoT and the changelog.

---

## 7. Relationship with cards

Cards apply statuses through declarative effect specs.
That means:
- card gameplay meaning belongs in `SSoT_Card_System.md`
- import/editor schema belongs in `SSoT_Card_Authoring_Contracts.md`
- status meaning itself belongs here

This split is non-negotiable.

---

## 8. Relationship with the CSO primitive catalog

`reference/CSO_Primitives_Catalog.md` is useful, but it is not the live runtime authority.

Use the split like this:
- this doc = what statuses mean and how the runtime/status catalogue surface works now
- reference catalog = explanatory primitive catalog, examples, and broader ontology support

If they conflict, this doc wins for current ALWTTT status truth.

---

## 9. MVP governance rules

- do not let themed display names become primary identity
- do not scatter status semantics across card docs, combat docs, and reference notes
- keep one canonical application path for runtime status application
- keep variants explicit through the catalogue/status key route
- document new cross-cutting status semantics here before treating them as done

---

## 10. Update rule

Update this document when a change affects:
- status identity rules
- catalogue/variant semantics
- runtime container semantics
- tick timing system
- canonical status meanings
- system-wide stack/duration/expiry behavior

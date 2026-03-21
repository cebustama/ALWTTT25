# SSoT_Status_Effects — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Runtime status semantics, catalogue boundary, and canonical MVP status meaning  
**Owns:** what a status is in ALWTTT, how status identity/theme are split, runtime container semantics, and the current canonical status set  
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

The runtime status surface includes concepts such as:
- `StatusEffectSO`
- `StatusEffectCatalogueSO`
- runtime containers / instances
- catalogue lookup and variant selection by key
- stack application / decay / refresh according to the authored contract

This SSoT owns the gameplay/runtime meaning of that surface.

---

## 4. Catalogue and variants

The catalogue-facing contract exists so cards and effects can apply statuses without hardcoding one-off logic.

Canonical rule:
- cards and effects reference a **status key / variant-capable identifier**
- runtime resolves that into the authored status asset/configuration
- variants may share a primitive but differ in authored tuning or presentation

This is why the primitive catalog and the live status system must stay separate:
- primitive catalog = reference/support
- status runtime semantics = active authority here

---

## 5. Canonical MVP status meanings

The governed docs currently rely on a compact canonical MVP set.

### 5.1 Flow
**Typical scope:** Song / Band  
**Meaning:** amplifies positive loop-to-song momentum conversion.

Flow is covered here as a status concept and in scoring/combat docs as a gameplay participant.
If wording conflicts:
- status identity/stack behavior lives here
- scoring relationship lives in `SSoT_Scoring_and_Meters.md`
- combat role lives in `SSoT_Gig_Combat_Core.md`

### 5.2 Composure
**Typical scope:** Musician  
**Meaning:** absorbs incoming positive Stress before Stress is applied.

Again:
- status semantics live here
- combat role and Breakdown interaction are reinforced in combat/core docs

### 5.3 Stress-adjacent negative states
Negative performer states such as Breakdown-following `Shaken` are gameplay-significant and may be represented either as explicit state or status-like state.

Rule:
- if represented through the status runtime surface, this doc owns its systemic identity
- if represented as special state, combat/runtime docs own the surrounding event flow

### 5.4 Future statuses
The system is allowed to grow, but new statuses must be added without reintroducing duplicated authority or theme-as-identity coupling.

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
- canonical status meanings
- system-wide stack/duration/expiry behavior

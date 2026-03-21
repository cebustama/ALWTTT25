# CSO_Primitives_Catalog — ALWTTT reference

**Status:** Reference support doc  
**Scope:** Stable primitive-level status-effect catalog and examples  
**Authority:** Secondary / explanatory  
**Rule:** If this file conflicts with `systems/SSoT_Status_Effects.md`, the SSoT wins.

---

## 1. Purpose

This document preserves the broader **Character Status Ontology (CSO)** primitive catalog as reference material.

Use it for:
- understanding the wider primitive vocabulary
- comparing themed status assets against underlying systemic roles
- future extension planning

Do **not** use it as the primary source of current runtime status truth.

---

## 2. Why this is reference, not primary authority

The active status runtime truth lives in:
- `systems/SSoT_Status_Effects.md`

This file is intentionally secondary because:
- it is broader than the current MVP slice
- it includes explanatory/reference taxonomy
- it may contain primitives not yet promoted into current ALWTTT runtime truth

---

## 3. Core primitive examples

| Primitive | Abstract function | Example themed names |
|---|---|---|
| `DamageUpFlat` | flat outgoing damage increase | Drive |
| `DamageDownFlat` | flat outgoing damage reduction | Offbeat |
| `DamageTakenUpFlat` | target takes extra damage | Exposed Groove |
| `TempShieldTurn` | turn-limited shield/mitigation | Composure |
| `TempShieldPersistent` | persistent shield until consumed | Stage Presence |
| `NegateNextHit` | negate next incoming hit/event | Perfect Timing |
| `DamageOverTime` | periodic automatic pressure | Feedback |
| `DisableActions` | prevent action use | Choke |
| `DamageReflection` | reflect pressure back | Backlash |
| `InitiativeBoost` | act earlier / priority boost | On The One |
| `MultiHitModifier` | extra hit count / repeats | Polyrhythm |
| `DebuffCleanse` | remove negative effects | Reset |
| `ResourceGenerationModifier` | change resource gain behavior | Engine / Groove Engine |

---

## 4. MVP-friendly core subset

The governed MVP slice mainly cares about a small status subset and adjacent state concepts.

Useful MVP-support primitives include:
- `TempShieldTurn` → maps well to **Composure**
- positive momentum amplifier primitives → support **Flow-like** concepts
- performer pressure/debuff primitives where needed by future cards
- cleanse / recovery / disruption primitives as controlled extensions

---

## 5. Usage rule

When adding or revising a status:
1. decide whether the change affects live runtime meaning
2. if yes, update `systems/SSoT_Status_Effects.md` first
3. only update this reference catalog if the broader explanatory primitive mapping also changed

A status change is not complete if it only touched this file.

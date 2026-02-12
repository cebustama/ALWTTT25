# ALWTTT — Combat Pipeline Roadmap (v0.1)

**Date:** 2026-02-12  
**Scope:** Card-effects combat slice: authoring → deck → hand → play → effects → statuses → loop scoring → conversion.

---

## 0) Current state (from docs)

### Tooling / data model (mostly aligned)
- Card payload is **effects-first**: `CardPayload.effects : List<CardEffectSpec>` (SerializeReference).
- EditorWindow + JSON import are **effects-only**.
- Implemented effect specs (confirmed in roadmap snapshot): `ApplyStatusEffectSpec`, `DrawCardsSpec` (execution may be stubbed).
- Status system supports variants-capable catalogue + key lookup (statusKey).

### Runtime (partially)
- Card play pipeline exists conceptually, but execution coverage is minimal.
- Status ticking exists as a concept but needs explicit loop integration.

---

## 1) Immediate milestone (Today): MVP validation slice

### Goal
Be able to **author and play** Action cards that produce visible state changes:

- Add Vibe
- Add/Remove Stress
- Add Flow (affects SongHype growth per loop)
- Add Composure (mitigates Stress)

### Deliverables
1) **New CardEffectSpec**: `ModifyVibeSpec`
2) **New CardEffectSpec**: `ModifyStressSpec`
3) Runtime execution support for both specs (in the current executor location, even if that’s `CardBase.ExecuteEffects` for now).
4) Ensure **Flow** and **Composure** StatusEffectSO assets exist and are in the catalogue.
5) Create **one test card per effect** and add them to the initial deck source of truth.
6) Play-mode validation checklist (below).

### Acceptance criteria (DoD)
- The CardEditorWindow can add these effects and save assets.
- Starting a gig loads a deck containing the test cards.
- The player can play each test card and observe:
  - Vibe changes on target audience.
  - Stress changes on performer (and Composure absorption works).
  - Flow changes and then accelerates SongHype gain during loops.
- No silent failures (missing status, missing target, etc. must log clear errors).

### Play-mode validation checklist
- [ ] Start gig → hand contains test cards.
- [ ] Play Add Composure → musician shows Composure stacks.
- [ ] Play Add Stress → Composure stacks decrease first, then Stress increases.
- [ ] Play Remove Stress → Stress decreases (not below 0).
- [ ] Play Add Flow → Flow stacks increase (band-wide).
- [ ] Start performance loops → SongHype gain increases vs baseline when Flow > 0.

---

## 2) Short-term milestone (1–2 weeks): “Combat loop correctness”

### Objectives
- Make status timing real and regression-safe.

### Tasks
- Wire `StatusEffectContainer.Tick(TickTiming)` at the following boundaries:
  - SongStart, LoopStart, LoopEnd, SongEnd, TurnStart/TurnEnd (if those exist in the combat flow).
- Implement DrawCards runtime execution against DeckManager (remove stub).
- Add minimal UI/logging surfaces:
  - Show SongHype, Flow stacks, Composure stacks, Stress per musician, Vibe per audience.

### DoD
- A duration/decay status can be authored and demonstrably decays via tick calls.
- DrawCards test card works end-to-end.

---

## 3) Mid-term milestone (1–2 months): “Effect coverage + triggered statuses”

### Objectives
- Expand the expressive range of card effects without breaking authoring.

### Tasks
- Add effect specs (data-only) + runtime execution:
  - Discard, ShuffleIntoDrawPile, GainInspiration, Exhaust, etc.
- Implement Triggered Statuses (OnLoopStart/OnSongEnd etc.) in a minimal, WAUC-friendly way.
- Add a **regression deck** (one card per effect) and run it as a repeatable validation harness.

### DoD
- “Effect Coverage Test Set” exists and stays green after refactors.

---

## 4) Long-term milestone: “Composition integration and balancing”

### Objectives
- Make Composition cards and loop-based systems coherent with the same effects/status architecture.

### Tasks
- Decide exact timing rules for Composition-card `effects`:
  - on play (composition phase) vs on loop boundaries.
- Introduce Song/Band scoped status container (optional) to avoid band-wide helper hacks.
- Balance archetype skeleton decks using Flow/Composure as a stable common language.


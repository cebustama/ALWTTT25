# ALWTTT -- Card Effects Pipeline Stabilization & Validation

**Technical Summary**

## Objective

Complete the migration to the new **Card Effects pipeline**, where cards
no longer store raw status effects directly, but instead define a
unified set of **effects/actions** capable of representing: - Status
application (with stacks, duration, modifiers, etc.) - Generic actions
(draw, discard, vibe modification, etc.) - Future extensible gameplay
effects.

The goal is to ensure correctness across **compilation, tooling,
authoring, data loading, and runtime execution**.

------------------------------------------------------------------------

## Phase 1 --- Compilation Stabilization

**Goal:** Project compiles cleanly after migration.

### Tasks

-   Fix all compilation errors caused by legacy `status` fields removal
    and API changes.
-   Update runtime and editor references still assuming the old model.

### Typical Impacted Areas

-   `CardDefinitionDescriptionExtensions`
-   `HandController`
-   `CompositionSession`
-   Editor drawers and inspectors relying on deprecated fields.

### Exit Criteria

-   No compiler errors or blocking warnings.
-   Play Mode starts successfully.

------------------------------------------------------------------------

## Phase 2 --- Authoring Tooling Validation (Editor + JSON)

**Goal:** Cards can be authored reliably via JSON import and Editor UI.

### Requirements

-   JSON import supports:
    -   `statusKey` (human-readable key).
    -   New effect-based payload model only (no legacy fields).
    -   Clear validation and error reporting.
-   EditorWindow supports:
    -   Editing card effects after creation.
    -   Editing parameters, targets, stacks, etc.
    -   Optional payload type switching (Action ↔ Composition) if
        feasible.

### Exit Criteria

-   A card can be created via JSON, visualized correctly in the Editor,
    and edited without recreation.

------------------------------------------------------------------------

## Phase 3 --- Effect Coverage Test Set

**Goal:** Validate the expressive range of the effect model.

### Deliverable

Create **one test card per supported effect**, for example: - Draw
card - Discard card - Apply `Flow +N` - Apply `Compulsion +N` - Modify
`Vibe` - Any additional supported effects

These cards form a minimal regression deck.

### Exit Criteria

-   All supported effect types are represented by at least one concrete
    asset.

------------------------------------------------------------------------

## Phase 4 --- Card Source of Truth (Deck Loading)

**Goal:** Make card loading deterministic and explicit.

### Tasks

-   Inspect Geek Scene and Combat Scene:
    -   Identify how cards are currently loaded.
    -   Detect duplicated or implicit sources.
-   Select and enforce a single authoritative source:
    -   `CardCatalogueSO`, `DeckSO`, or equivalent.
-   Ensure the test cards are included in the runtime deck.

### Exit Criteria

-   Clear answer to: *"Where do combat cards come from?"*
-   Deck contents can be changed intentionally and predictably.

------------------------------------------------------------------------

## Phase 5 --- Runtime Effect Validation

**Goal:** Ensure every effect executes correctly in real gameplay.

### Tasks

For each test card: - Validate targeting resolution. - Validate
execution order. - Validate status stacking and parameter correctness. -
Validate UI and state synchronization.

### Exit Criteria

-   All test cards behave correctly in combat.
-   System is regression-safe for future effect extensions.

------------------------------------------------------------------------

## Outcome

A fully stabilized, tool-supported, test-covered Card Effects pipeline,
ready for scalable content authoring and future gameplay expansion.

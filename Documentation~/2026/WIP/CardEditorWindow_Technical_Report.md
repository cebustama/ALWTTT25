# CardEditorWindow Technical State Report

## ALWTTT --- Card Authoring Tooling (CSO Era)

This document evaluates the current **CardEditorWindow** implementation
against the desired "CardWizardWindow" goals, after the migration to the
**CSO StatusActions pipeline**.

It clearly separates: - What is already supported - What is missing or
broken - What the next best technical steps are - How to evolve the tool
while respecting SOLID and avoiding unnecessary complexity

------------------------------------------------------------------------

## 1. Current Authoring Model Snapshot

### CardPayload (CSO-first)

-   All cards express gameplay exclusively through:
    -   `CardPayload.StatusActions : List<StatusEffectActionData>`
-   Each StatusAction contains:
    -   `CharacterStatusId EffectId`
    -   `ActionTargetType TargetType`
    -   `int StacksDelta`
    -   `float Delay`
-   ActionCardPayload no longer contains legacy procedural actions.
-   CompositionCardPayload inherits StatusActions and adds composition
    metadata.

**Conclusion:** The data model is clean, declarative, and CSO-aligned.

------------------------------------------------------------------------

## 2. What CardEditorWindow Already Supports

### ✅ A) Catalog-first workflow

-   Select musician
-   Load or create `MusicianCardCatalogData`
-   Automatically assign catalog to musician

Designers work at the catalog level instead of raw assets.

------------------------------------------------------------------------

### ✅ B) Browse and select existing cards

-   Displays catalog entries
-   Filters by:
    -   Domain (Action / Composition)
    -   Acquisition flags (Starter / Reward / Locked)

------------------------------------------------------------------------

### ✅ C) Add existing cards to catalog

-   Drag-and-drop CardDefinition
-   Uses centralized validation via `MusicianCatalogService`

------------------------------------------------------------------------

### ✅ D) Create new cards + payload assets

-   Creates CardDefinition + correct payload asset
-   Assigns into catalog
-   Handles Undo, saving, and folder structure

------------------------------------------------------------------------

### ✅ E) Edit CardDefinition fields in-window

-   ID, display name
-   Performer rules
-   Visuals
-   Inspiration cost/generation
-   Card type, rarity, keywords
-   Audio / animation
-   Exhaust, targeting overrides

No Inspector or code edits required.

------------------------------------------------------------------------

### ⚠️ F) Payload editing is partially broken

-   Composition payload UI works.
-   Action payload editor still expects legacy `actions` field.
-   CSO `StatusActions` are NOT currently exposed for editing.

This is the most critical gap.

------------------------------------------------------------------------

### ⚠️ G) JSON import/export

-   Import likely exists in older partial class, but compatibility with
    CSO not verified.
-   Export not confirmed.
-   Needs audit once StatusActions UI is fixed.

------------------------------------------------------------------------

## 3. Missing vs Desired Wizard Features

### ❌ Filter by StatusId

Not implemented. Currently only domain and acquisition filters exist.

------------------------------------------------------------------------

### ❌ Friendly StatusActions authoring

Designers cannot currently author gameplay effects in the editor UI.

------------------------------------------------------------------------

### ❌ JSON Export

Not present in visible code.

------------------------------------------------------------------------

### ⚠️ Composition deep authoring

Correctly out of MVP scope. Current structure is future-safe but not
expanded.

------------------------------------------------------------------------

## 4. Architectural Recommendation

### Adapt the existing CardEditorWindow

Do NOT rewrite.

Reasons: - Already contains stable asset plumbing - Catalog lifecycle is
solved - Creation and editing pipeline exists - Risk of regression is
low

Instead, evolve it incrementally.

------------------------------------------------------------------------

### Minimal SOLID-aligned refactor

1.  **Filtering helper**
    -   Extract filtering logic into a small helper function or service.
    -   Enables StatusId filtering cleanly.
2.  **Payload drawers**
    -   Separate drawers per payload type.
    -   Introduce a shared StatusActions drawer (base payload concern).
3.  **Status display resolver (optional)**
    -   Map CharacterStatusId → DisplayName using
        StatusEffectCatalogueSO or primitive DB.

Keep responsibilities small and composable.

------------------------------------------------------------------------

## 5. Next Best Steps

### Step 1 --- Fix payload editing (highest priority)

Expose `statusActions` in the editor UI for both Action and Composition
payloads.

------------------------------------------------------------------------

### Step 2 --- Add StatusId filtering

Add enum filter and inspect payload StatusActions.

------------------------------------------------------------------------

### Step 3 --- Improve StatusActions UX

Optional but valuable: - Clear row layout - Effect dropdown - Target
dropdown - Stack and delay fields - Optional display name resolution

------------------------------------------------------------------------

### Step 4 --- JSON import/export audit

Update import/export to serialize StatusActions cleanly.

------------------------------------------------------------------------

### Step 5 --- Remove remaining legacy editor exposure

Ensure StatusType and legacy concepts are not visible to designers.

------------------------------------------------------------------------

## 6. Support Matrix

### Supported

-   Musician selection and catalog lifecycle
-   Browse and filter by domain/acquisition
-   Add existing cards
-   Create new cards and payloads
-   Edit CardDefinition fields
-   Composition payload editing
-   CSO data model

### Not Supported / Broken

-   Editing StatusActions in UI
-   Filter by StatusId
-   JSON export
-   Confirmed CSO JSON import compatibility

------------------------------------------------------------------------

End of report.

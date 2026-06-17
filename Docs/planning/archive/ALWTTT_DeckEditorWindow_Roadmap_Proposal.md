# ALWTTT — DeckEditorWindow / Deck Authoring Tool Proposal and Roadmap

## Purpose

This document proposes a dedicated **DeckEditorWindow** for ALWTTT focused on fast deck authoring for testing, with special priority on:

- creating a **new `BandDeckData` asset directly from JSON**
- reviewing/editing the staged deck before writing to disk
- saving it safely into project assets
- optionally integrating it into the existing **Gig Setup** test flow
- later supporting catalogue-driven deck editing without JSON

The proposal is based on the current runtime deck contract, the existing `CardEditorWindow` JSON pipeline, and the UI/layout patterns already validated in the separate `DiagnosticRuleCatalogueWindow` reference.

---

## Executive Summary

The fastest and safest path is **not** to invent a completely separate authoring architecture.

The new tool should:

1. **reuse the same authoring philosophy** already present in `CardEditorWindow`
   - parse JSON
   - stage in memory
   - review/edit
   - save to assets only when explicit save is pressed
2. **adapt to the real current deck contract**, which is much simpler than expected:
   - `BandDeckData` currently stores a **flat `List<CardDefinition>`** plus metadata fields for `deckId`, `displayName`, and `description`
   - it does **not** currently store structured deck entries or explicit copy counts
3. treat the current runtime as the source of truth:
   - `PersistentGameplayData.SetBandDeck` clears runtime deck state and loads cards from `BandDeckData`
   - but it also **deduplicates cards by reference**, which means repeated identical references are not preserved as multiple copies in runtime
4. acknowledge one key design fork up front:
   - **MVP path**: accept current unique-card semantics and build the editor around that
   - **extended path**: if true card copies matter, first evolve the deck asset/runtime contract before promising copy-based authoring

Because of that, the recommended roadmap is:

- first lock the contract
- then build a JSON-first deck staging/save MVP
- then add catalogue browsing/manual editing
- then add integration conveniences like “add to GigSetupConfig”

---

## Current Technical Reality

### 1) `BandDeckData` is currently a very simple asset

The asset contains:

- `deckId`
- `displayName`
- `description`
- `List<CardDefinition> cards`

It only publicly exposes `Cards` as an `IReadOnlyList<CardDefinition>`, so editor tooling will need to write those serialized private fields via `SerializedObject`, helper utilities, or controlled asset construction. There is no public mutator API today.

### 2) Runtime deck loading currently assumes a flat card list

`PersistentGameplayData.SetBandDeck(BandDeckData bandDeck)`:

- clears `currentActionCards` and `currentCompositionCards`
- resets granted-card provenance
- reads `bandDeck.Cards`
- routes cards into runtime action/composition lists using `card.IsAction` and `card.IsComposition`
- skips nulls
- **skips duplicates by reference** using `Contains`

This means the current runtime interpretation of a `BandDeckData` asset is effectively a **set-like unique card collection**, not a multiset with reliable copy counts.

### 3) Deck selection already exists in the test/dev gig setup flow

`GigSetupConfigData` contains `availableBandDecks`, and `GigSetupController` builds the deck dropdown directly from that list. When the user presses Start, the selected `BandDeckData` is placed into `GigRunContext.RunConfig.bandDeck`, then applied to persistent gameplay state.

This means a saved deck asset is not automatically testable just by existing in the project. It becomes available in Gig Setup only when it is included in `GigSetupConfigData.availableBandDecks`.

### 4) A reusable authoring pattern already exists in `CardEditorWindow`

The current card importer already implements the right high-level workflow:

- user pastes JSON
- importer parses the JSON
- a single card is staged in memory
- user reviews/edits staged fields
- explicit save creates assets on disk
- failures trigger cleanup to avoid orphaned assets

It also already supports a JSON contract style with:

- single object import
- batch wrapper import (`{ "cards": [...] }`)
- explicit rejection of raw root arrays

This is the strongest direct implementation reference for the deck tool.

### 5) A reusable UI/layout reference already exists in `DiagnosticRuleCatalogueWindow`

The rule catalogue window already demonstrates useful editor UX patterns for a deck tool:

- top toolbar/header with Find / Refresh / Import / Export
- search field
- filters block with clear filters
- split layout with draggable divider
- left list pane + right details pane
- result counts and filtered counts

That is a strong visual/interaction reference for the future deck catalogue UI.

---

## Most Important Design Constraint

## Copies are not currently first-class in the deck contract

This is the biggest finding from the code review.

The user-facing idea of a deck editor naturally suggests entries like:

- card A × 2
- card B × 3
- card C × 1

But the current data/runtime contract is not built around that.

### Current state

- `BandDeckData` stores only `List<CardDefinition> cards`
- `PersistentGameplayData.SetBandDeck` deduplicates identical card references before populating runtime decks

### Consequence

If you build a deck editor that exposes `copies`, one of these must be true:

#### Option A — MVP / current-contract mode
Treat a deck as a **unique list of card references**.

In this mode:
- duplicate card references are not meaningful
- JSON import should normalize duplicates away or warn clearly
- the UI may show only presence/absence, not copies

#### Option B — future true-deck mode
Promote deck entries to a real contract, for example:

- `BandDeckEntry { CardDefinition card; int copies; }`
- `BandDeckData` stores `List<BandDeckEntry>`
- `SetBandDeck` is updated to respect copies
- any runtime deck/draw logic that depends on multiplicity is aligned accordingly

### Recommendation

For the first implementation pass, use **Option A** unless you explicitly want to evolve the runtime deck contract now.

That keeps the tool highly useful for fast testing while avoiding a misleading “copies” UI that the runtime cannot currently honor.

---

## Proposed Product Definition

## Tool Name

**`DeckEditorWindow`**

Optional user-facing subtitle:

**“Deck Wizard”** or **“Band Deck Editor”**

---

## Primary User Goals

### Goal 1 — JSON-first testing speed

A designer/developer wants to:

- paste JSON describing a deck
- stage it immediately
- review/edit it in the editor
- save it as a new `BandDeckData` asset
- optionally register it into `GigSetupConfigData`
- run a gig test quickly

### Goal 2 — Visual deck editing

A designer wants to:

- load an existing deck asset
- browse a catalogue of cards
- filter/search available cards
- add/remove cards from the deck
- save changes or save a variant

### Goal 3 — Safe authoring

The tool must not leave broken or orphaned assets if a save path, reference resolution, or configuration step fails.

---

## Proposed Scope

## In Scope for the first roadmap

- create/load/edit/save `BandDeckData`
- stage deck in memory before asset creation
- JSON import to staged deck
- JSON export from staged deck / existing deck
- catalogue browser for existing cards
- optional integration helper for `GigSetupConfigData.availableBandDecks`

## Explicitly Out of Scope for the first MVP

- editing card assets themselves inside the deck tool
- inline creation of missing cards from nested deck JSON
- introducing true copy semantics into runtime unless contract evolution is explicitly approved
- automatic gameplay balance validation
- scene/runtime simulation inside the deck tool itself

---

## Proposed UX / Window Layout

## 1) Toolbar/Header

Top toolbar should include:

- `Target Deck` object field
- `Find`
- `Load`
- `Refresh`
- `Save`
- `Save As`
- `Import JSON`
- `Export JSON`
- `Ping`

Optional secondary fields:

- `GigSetupConfigData` object field
- `Add to Gig Setup`
- `Card Source` / `Registries` / `Catalogue` source selector if needed later

### Behavior notes

- `Load` loads the selected deck into staged editable state
- `Save` writes back to the loaded asset
- `Save As` creates a new deck asset path
- `Find` can locate the first deck asset or a relevant config asset
- `Ping` selects/highlights the asset in the Project view

---

## 2) Left Pane — Card Catalogue Browser

Purpose:
- browse available `CardDefinition` assets already in the project
- filter and search them
- add them to the staged deck

### Suggested filters

Minimum useful filters:

- show `Action`
- show `Composition`
- search text

Likely follow-up filters, borrowed from existing card editor patterns:

- Starter only
- Reward only
- Locked only
- Filter by status / effect presence
- Filter by musician / performer
- Filter by rarity
- Filter by card type

### Row actions

Each visible card row may support:

- `Add`
- `Remove`
- `Ping`
- `Open in Card Editor`

If MVP remains unique-card-based, the row state can simply reflect whether the card is already present in the staged deck.

---

## 3) Right Pane — Deck Inspector / Staging Area

### Section A — Metadata

Fields:

- `deckId`
- `displayName`
- `description`
- optional `suggestedSaveFolder`
- optional notes/debug source

### Section B — Card List

Shows the cards currently in the staged deck.

Actions:

- remove card
- ping card
- open card editor
- reorder for authoring readability if desired

If the current contract is preserved, this list is a unique card list.

### Section C — Summary

Useful summary info:

- total cards
- action cards
- composition cards
- unresolved references count
- warnings count

### Section D — Validation Panel

Human-readable warnings/errors:

- empty deck
- duplicate JSON references normalized away
- unresolved card IDs
- null card refs
- invalid save folder
- save path collision / overwrite warning

---

## 4) JSON Import / Export Block

This is the main MVP feature.

### Import block

- large `TextArea`
- `Create from JSON`
- `Clear`
- `Load JSON file`

### Export block

- `Export current deck to JSON`
- `Copy JSON`
- `Save JSON to file`

### Workflow

1. user pastes JSON
2. parse and resolve cards
3. create staged in-memory deck model
4. show warnings/errors
5. allow review/edit
6. explicit save creates or updates `BandDeckData` asset

---

## Proposed JSON Contract (MVP)

Because the current runtime contract is flat and unique-card-oriented, the safest first JSON contract should also be simple.

## Recommended MVP JSON shape

```json
{
  "deckId": "testing_composition_alpha",
  "displayName": "Testing Composition Alpha",
  "description": "Deck for composition session tests",
  "saveFolder": "Assets/Resources/Data/Decks/Testing",
  "cards": [
    { "cardId": "Cantante_C_Rhythm_001_TestPassTurn" },
    { "cardId": "Cantante_A_StressRelief_001" },
    { "assetPath": "Assets/Resources/Data/Cards/Cantante/MyCard.asset" }
  ]
}
```

## Recommended parse rules

- raw root arrays should be rejected, matching the current card importer philosophy
- allow exactly one root object for single-deck import
- optionally allow a future batch wrapper later:

```json
{ "decks": [ { ... }, { ... } ] }
```

### Resolution priority

For each card entry:

1. resolve by `cardId` if present
2. fallback to `assetPath` if present
3. optionally fallback to GUID later

### Duplicate handling

Under current contract semantics:

- repeated card references in JSON should not silently become “copies”
- they should either:
  - be normalized to one entry with a visible warning, or
  - fail validation if strict mode is on

### Empty deck handling

Empty deck should fail validation in MVP.

---

## Suggested Internal Architecture

## Guiding Principle

Do **not** make `DeckEditorWindow` depend directly on `CardEditorWindow` internals.

The card editor currently contains a lot of useful logic, but it is embedded in window state and UI flow. The healthier reuse pattern is:

- extract shared editor-only services
- let both windows consume them

## Proposed services

### 1) `DeckJsonImportService`

Responsibilities:
- parse deck JSON DTOs
- reject invalid root shapes
- validate required fields
- create staged in-memory deck model

### 2) `DeckCardResolutionService`

Responsibilities:
- resolve card references by `cardId`
- fallback by `assetPath`
- later support GUID
- surface clear diagnostics on unresolved entries

### 3) `DeckAssetSaveService`

Responsibilities:
- create new `BandDeckData` asset
- update existing asset
- write serialized fields safely
- `SetDirty`, `SaveAssets`, `Refresh`
- protect against partial/broken save outcomes

### 4) `DeckValidationService`

Responsibilities:
- validate metadata
- validate non-empty card list
- validate no unresolved refs
- normalize duplicate references according to current policy

### 5) Optional shared service extraction from `CardEditorWindow`

If desired, extract card-side utilities like:
- shared JSON parsing helpers
- shared save/cleanup conventions
- common editor file path helpers

But the deck tool should remain independently usable.

---

## Save Strategy

The save behavior should copy the strongest guarantees from the card importer.

## Save rules

1. **Nothing is written before Save**
   - import only stages data in memory

2. **Save validates first**
   - invalid staged deck cannot be written

3. **Asset write is explicit**
   - create new asset or overwrite existing one only when user chooses

4. **Use `SerializedObject` for asset writes**
   - because `BandDeckData` currently exposes no public mutators for editor writes

5. **Refresh the editor/project state after save**
   - `EditorUtility.SetDirty`
   - `AssetDatabase.SaveAssets()`
   - `AssetDatabase.Refresh()`

6. **Optional Gig Setup integration step is separate**
   - asset save should not be blocked by whether a setup config is selected

---

## Optional Integration Helper: Gig Setup Registration

Because the test flow uses `GigSetupConfigData.availableBandDecks`, the deck tool should optionally support:

- selecting a `GigSetupConfigData`
- pressing `Add to Gig Setup`
- inserting the saved deck if missing
- pinging the config asset

This is not core to deck authoring, but it is very valuable for test speed.

### Why it matters

Without it, the designer still has to:

- find the config asset
- add the new deck manually
- return to GigSetupScene to test

That is small, but repetitive.

---

## Explicit Requirements

## Functional Requirements

### FR-1 — Create deck from JSON
The tool must create a staged deck from JSON and allow saving it as a `BandDeckData` asset.

### FR-2 — Load existing deck
The tool must load an existing `BandDeckData` into editable staged state.

### FR-3 — Save and Save As
The tool must support both overwrite and new asset save flows.

### FR-4 — Catalogue editing
The tool must allow adding/removing existing `CardDefinition` assets from the staged deck through a browsable UI.

### FR-5 — Export JSON
The tool must serialize the current staged deck or loaded deck back into JSON.

### FR-6 — Validation visibility
The tool must show validation errors/warnings before save.

### FR-7 — Project asset persistence
Saved decks must become normal project assets, visible in the Project window.

### FR-8 — Optional Gig Setup registration
The tool should optionally add a saved deck to `GigSetupConfigData.availableBandDecks`.

## Non-Functional Requirements

### NFR-1 — Editor-only
The tool must remain inside editor-only code paths.

### NFR-2 — No orphaned/broken state from failed save
Even if save/config integration fails, deck asset persistence should remain predictable and recoverable.

### NFR-3 — Human-readable diagnostics
All validation issues should be understandable by designers, not only programmers.

### NFR-4 — Stable under domain reload / recompilation
Window state should survive typical editor recompilation as much as reasonably possible.

### NFR-5 — No misleading copies UI in MVP
The UI must not imply runtime copy semantics that are not actually supported yet.

---

## Recommended Product Decisions

## Decision 1 — MVP should use unique-card semantics

Because current runtime deduplicates by reference, the first shipped version of the deck tool should behave like a **unique card collection editor**.

This is honest, fast, and useful for testing.

## Decision 2 — Save path should default predictably

Suggested default folder:

- either a configurable editor preference
- or the folder of the currently loaded deck
- or a project-standard deck folder such as `Assets/.../Decks/`

## Decision 3 — Gig Setup registration should be optional but easy

It is not part of the deck contract itself, but it is part of the real testing workflow.

## Decision 4 — Keep card authoring separate from deck authoring

The deck tool should be able to open the card editor, but it should not try to replace it.

---

## Risks and Design Traps

### Risk 1 — Showing “copies” before the runtime supports copies
This would create a deceptive authoring experience.

### Risk 2 — Coupling the tool to `CardEditorWindow` private state
That would make the new tool fragile and hard to maintain.

### Risk 3 — Hardcoding one card lookup source too early
Card resolution should be abstracted so the lookup strategy can evolve.

### Risk 4 — Saving a deck but forgetting test registration
A user may assume the deck is ready in Gig Setup when it still needs config registration.

### Risk 5 — Overengineering batch and inline nested-card creation too early
That slows delivery of the real value: faster deck test iteration.

---

## Detailed Incremental Roadmap

## Phase 0 — Lock the deck contract

### Goal
Remove ambiguity about what a deck means **today**.

### Tasks
- confirm `BandDeckData` as the current authoritative deck asset
- confirm current runtime interpretation through `PersistentGameplayData.SetBandDeck`
- explicitly decide MVP semantics:
  - unique cards only
  - no true copies yet
- document whether duplicate references in JSON are:
  - warnings + normalization
  - or validation failures
- document save field mapping:
  - `deckId`
  - `displayName`
  - `description`
  - `cards`

### Deliverables
- short contract note / SSoT addendum for deck authoring
- explicit MVP decision on duplicate/copy semantics

### DoD
- there is no ambiguity left about how saved deck assets are interpreted by runtime

---

## Phase 1 — Extract or define editor services

### Goal
Create a maintainable base for the new window.

### Tasks
- create `DeckJsonImportService`
- create `DeckCardResolutionService`
- create `DeckValidationService`
- create `DeckAssetSaveService`
- optionally extract common editor helpers from current card importer where it is genuinely shared

### Notes
Do not block this phase on large-scale refactoring of `CardEditorWindow`. Only extract what is clearly reusable.

### Deliverables
- editor services with focused responsibilities
- unit-testable or at least manually testable service boundaries

### DoD
- deck staging and save logic can exist outside the window UI class

---

## Phase 2 — Skeleton `DeckEditorWindow`

### Goal
Establish the new authoring surface.

### Tasks
- create the editor window class
- build toolbar/header
- build left/right split layout
- add target deck object field
- add basic Load / Refresh / Ping actions
- create staged deck model held in window state

### UI reference use
- borrow the split-layout ergonomics and header organization from `DiagnosticRuleCatalogueWindow`
- borrow left/right authoring ergonomics from `CardEditorWindow`

### Deliverables
- window opens and persists basic state
- load existing deck into staged state

### DoD
- a user can open the tool and inspect/load a deck asset without editing yet

---

## Phase 3 — JSON-first MVP

### Goal
Ship the highest-value workflow first: **paste JSON -> save deck asset**.

### Tasks
- implement deck JSON DTO
- implement root-shape validation
- reject raw root arrays
- support single deck object import
- resolve cards by `cardId` / fallback by `assetPath`
- stage deck in memory
- show validation results
- support `Save` and `Save As`
- write `BandDeckData` asset fields safely

### Recommended behavior
- if duplicate card refs are found, normalize them and show a visible warning
- if unresolved cards remain, save must stay disabled
- deck must not save if empty

### Deliverables
- working deck JSON importer
- working `BandDeckData` asset creation flow

### DoD
- user can paste valid JSON and produce a usable deck asset in the project without manual asset creation

---

## Phase 4 — Manual deck editing via catalogue browser

### Goal
Allow deck editing without JSON.

### Tasks
- populate a browsable list of available `CardDefinition` assets
- add search
- add action/composition filters
- add/remove selected cards from staged deck
- show deck summary counts
- optionally add filters borrowed from current `CardEditorWindow` patterns

### Deliverables
- deck can be assembled visually from existing cards

### DoD
- user can fully edit a deck without touching JSON

---

## Phase 5 — Gig Setup integration helper

### Goal
Reduce test friction after saving a deck.

### Tasks
- allow selecting a `GigSetupConfigData`
- implement `Add to Gig Setup`
- avoid duplicate config entries
- optionally offer `Remove from Gig Setup`
- show confirmation if registration succeeded

### Deliverables
- saved deck can be made available in Gig Setup from inside the tool

### DoD
- after save, user can register the deck for test runs without manual config editing

---

## Phase 6 — Export / Roundtrip polish

### Goal
Make the tool portable and easy to iterate with.

### Tasks
- export deck JSON from asset
- export staged deck JSON
- allow copy to clipboard
- optionally load JSON from disk
- add overwrite confirmation dialogs where needed

### Deliverables
- roundtrip between asset and JSON is supported

### DoD
- user can edit through both JSON and visual tooling without losing data unexpectedly

---

## Phase 7 — Cross-tool integration and polish

### Goal
Connect deck authoring smoothly to existing card tooling.

### Tasks
- add `Open in Card Editor` for selected card
- add `Ping Card`
- optionally add `Create Missing Card...` handoff into `CardEditorWindow`
- improve warning/help text
- add last-used folders / convenience persistence

### Deliverables
- smoother day-to-day authoring workflow

### DoD
- deck editing and card editing work together without duplicated effort or confusion

---

## Phase 8 — Contract evolution for true copies (optional future phase)

### Goal
Only if desired: make the deck asset/runtime support real multiplicity.

### Tasks
- define a real `BandDeckEntry` structure
- update `BandDeckData`
- update `PersistentGameplayData.SetBandDeck`
- audit any draw/deck logic that depends on duplicates
- update JSON schema accordingly
- update UI to support counts honestly

### Deliverables
- explicit copy-aware deck contract

### DoD
- duplicates/copies authored in the tool are preserved meaningfully in runtime

---

## Suggested Implementation Order

If the objective is maximum usefulness soonest, the best order is:

1. **Phase 0** — lock contract
2. **Phase 1** — services
3. **Phase 3** — JSON-first MVP
4. **Phase 2** — finalize window skeleton where needed
5. **Phase 4** — manual catalogue editing
6. **Phase 5** — Gig Setup integration helper
7. **Phase 6** — export/polish
8. **Phase 7** — cross-tool convenience
9. **Phase 8** — only if copies must become real

This ordering intentionally prioritizes the exact value you highlighted:

> create a new deck asset directly from JSON and save it to project assets quickly for test iteration.

---

## Files / Areas Likely Touched

## New files (suggested)

- `DeckEditorWindow.cs`
- `DeckJsonImportService.cs`
- `DeckValidationService.cs`
- `DeckCardResolutionService.cs`
- `DeckAssetSaveService.cs`
- optional `DeckEditorDtos.cs`

## Existing files likely touched lightly

- `GigSetupConfigData.cs` only if helper APIs are desired
- optional shared card editor utilities if extracted cleanly

## Existing files to review but not necessarily change in MVP

- `BandDeckData.cs`
- `PersistentGameplayData.cs`
- `GigRunContext.cs`
- `GigSetupController.cs`
- `CardEditorWindow.cs`
- `CardEditorWindow.JsonImport.cs`

## Existing files that should change only if copies become a real requirement

- `BandDeckData.cs`
- `PersistentGameplayData.SetBandDeck`
- any runtime deck/draw consumers of current action/composition card lists

---

## Recommended Definition of Done for the First Useful Release

A first release should be considered successful when all of this is true:

- the user can open `DeckEditorWindow`
- paste valid deck JSON
- resolve all card references
- review deck metadata and card list in staged state
- save a new `BandDeckData` asset into the project
- load that asset again later
- optionally register it into `GigSetupConfigData`
- use it in the current Gig Setup testing flow
- see clear errors if any card cannot be resolved or if the deck is empty
- not be misled about copies if copies are not yet supported by runtime

---

## Final Recommendation

Build the deck tool now, but build it **honestly around the current runtime contract**.

That means:

- JSON-first staged save flow first
- unique-card semantics first
- safe asset persistence first
- test-flow integration second
- copy-aware deck semantics only after an explicit contract/runtime change

This will still accelerate test iteration a lot, while keeping the tool aligned with what ALWTTT actually supports today.

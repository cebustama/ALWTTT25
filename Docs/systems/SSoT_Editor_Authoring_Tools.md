# SSoT_Editor_Authoring_Tools — ALWTTT

**Status:** Active governed SSoT
**Scope:** Unity editor tools for authoring cards, decks, status effects, and musical data
**Owns:** tool inventory, capabilities, menu paths, data flows, supporting services, known gaps
**Does not own:** card data contracts (see `SSoT_Card_Authoring_Contracts`), runtime card/status semantics, MidiGenPlay internals

---

## 1. Purpose

This document is the primary authority for what editor authoring tools exist in ALWTTT, what each tool does, how they relate to each other, and what their current limitations are.

It does not duplicate the data representation rules in `SSoT_Card_Authoring_Contracts.md`. Instead it documents the tools that implement those contracts.

---

## 2. Scope boundaries

### 2.1 This SSoT is normative for

- which editor tools exist and what they do
- menu paths and access points
- per-tool capabilities and workflows
- supporting editor services and their responsibilities
- file locations of tool source code
- known gaps, limitations, and planned improvements
- cross-tool integration points

### 2.2 This SSoT is not normative for

- card/payload data contracts and JSON schema → `SSoT_Card_Authoring_Contracts.md`
- status effect runtime semantics → `SSoT_Status_Effects.md`
- card gameplay semantics → `SSoT_Card_System.md`
- MidiGenPlay-owned chord/pattern data types → MidiGenPlay docs
- runtime phase flow and deck/hand pipeline → `SSoT_Runtime_Flow.md`

---

## 3. Tool inventory

| Tool | Class | Menu path | Primary purpose |
|---|---|---|---|
| Card Editor | `CardEditorWindow` | ALWTTT → Cards → Card Editor | Single-card authoring: create, inspect, edit cards within a musician catalog |
| Deck Editor | `DeckEditorWindow` | ALWTTT → Cards → Deck Editor | Deck composition: create/load/edit BandDeckData assets, JSON import/export |
| Status Effect Wizard | `StatusEffectWizardWindow` | ALWTTT → Status → Status Effect Wizard | Create and edit StatusEffectSO assets backed by the CSO primitive database |
| Chord Progression Catalogue | `ChordProgressionCatalogueWizard` | MidiGenPlay → Chord Progression Catalogue Wizard... | Read-only browser for ChordProgressionData and ChordProgressionPaletteSO assets |

All four are `#if UNITY_EDITOR` gated `EditorWindow` subclasses. None ship in builds.

---

## 4. Card Editor (`CardEditorWindow`)

**File:** `Assets/Scripts/Cards/Editor/CardEditorWindow.cs` (partial class, with `CardEditorWindow.JsonImport.cs`)
**Namespace:** `ALWTTT.Cards.Editor`

### 4.1 What it does

The Card Editor is the primary tool for authoring individual `CardDefinition` + `CardPayload` assets within a musician's card catalog (`MusicianCardCatalogData`).

### 4.2 Layout

Two-panel split with draggable splitter:

- **Left panel** — Musician selector, catalog entry list with filters, card selection, "Add Existing" field, "Create New Card" wizard.
- **Right panel** — Inspector-style editor for the selected card: CardDefinition common fields, payload-specific fields (action or composition), with foldout sections.

### 4.3 Key workflows

**Select musician → browse catalog → inspect/edit card:**
The musician dropdown loads the corresponding `MusicianCharacterData` and its `MusicianCardCatalogData`. The catalog list shows entries filtered by kind (action/composition), acquisition flags (starter/reward/locked), and status effect reference. Selecting an entry shows its `CardDefinition` and payload fields in the right panel for direct editing.

**Create new card (wizard):**
The inline create wizard collects: kind (Action/Composition), id, display name, name tag, inspiration cost/generated, and catalog entry defaults (acquisition flags, starter copies, unlock id). On Create, it delegates to `CardAssetFactory.TryCreateCard` which creates both the `CardDefinition` and the correct `CardPayload` subclass asset, wires them together, and saves to disk. The entry is then added to the catalog via `MusicianCatalogService.TryAddEntry`.

**Add existing card:**
An object field lets the user drag an existing `CardDefinition` and add it to the current musician's catalog.

**JSON batch import:**
The partial class `CardEditorWindow.JsonImport.cs` provides batch card creation from JSON. This follows the schema defined in `SSoT_Card_Authoring_Contracts.md`.

### 4.4 Filters

- Show Action / Show Composition (kind toggle)
- Starter Only, Reward Only, Locked Only (acquisition flag filters)
- Filter by StatusId (shows only cards whose effects reference a specific `CharacterStatusId`)

### 4.5 Dependencies

- `CardAssetFactory` — asset creation logic (separated from the window)
- `MusicianCatalogService` — catalog mutation (add entry with Undo support)
- `ALWTTTProjectRegistriesSO` — auto-resolved singleton for musician/catalog lookup
- `MusicianCharacterData`, `MusicianCardCatalogData` — musician-scoped data assets

---

## 5. Deck Editor (`DeckEditorWindow`)

**File:** `Assets/Scripts/Cards/Editor/DeckEditorWindow.cs`
**Namespace:** `ALWTTT.Cards.Editor`

### 5.1 What it does

The Deck Editor creates and edits `BandDeckData` assets — the deck containers used by the runtime deck/hand pipeline. It supports both visual catalogue-based editing and JSON-first workflows.

### 5.2 Layout

Three-zone vertical layout:

- **Header/toolbar** — Target deck asset field, GigSetupConfigData field, Load/Save/Save As/New/Find/Ping buttons, Import JSON/Export JSON buttons.
- **Body (split)** — Left: staged card list with badges. Right: deck metadata fields (deckId, displayName, description) and JSON text area.
- **Catalogue strip** — Toggleable full-width catalogue browser below the body, with action/composition filter toggles and text search.
- **Status bar** — Validation messages and warnings.

### 5.3 Key workflows

**Load existing deck → edit → save:**
Set the Target Deck field and press Load. The deck is staged in memory as a `StagedDeck`. Add/remove cards via the catalogue or directly. Save writes back to the same asset; Save As creates a new asset.

**JSON import (reference existing + create new):**
Paste JSON into the text area and press Import. Two entry modes are supported: `{ "cardId": "existing_id" }` to reference an existing `CardDefinition` by id, and `{ "kind": "Action", "id": "new_id", "effects": [...] }` to create a new card. New cards are staged in memory with a `[NEW]` badge and are only persisted when the deck is saved. Pending new cards are lost on domain reload — the window warns about this.

**Catalogue browsing:**
The catalogue strip scans all `CardDefinition` assets in the project. Filter by action/composition toggles and free-text search. Each row has Add/Remove/Ping actions.

**GigSetup registration:**
With a `GigSetupConfigData` assigned, the "Add to Gig Setup" button registers the saved deck in `GigSetupConfigData.availableBandDecks` so it appears in the gig setup scene.

**JSON export:**
Exports the current staged deck to JSON in the text area.

### 5.4 Current deck contract

Decks are unique card lists (Option A from the original design proposal). Duplicate card references are normalized away. True copy semantics (Option B) are not implemented. See `SSoT_Card_Authoring_Contracts.md` §5 for the JSON schema contract.

### 5.5 Supporting services

| Service | File | Responsibility |
|---|---|---|
| `DeckJsonImportService` | `DeckJsonImportService.cs` | Parses JSON → staged deck entries, resolves card references, stages new-card creation requests |
| `DeckCardCreationService` | `DeckCardCreationService.cs` | Creates CardDefinition + payload assets for staged new cards during save |
| `DeckValidationService` | `DeckValidationService.cs` | Validates staged deck: null refs, empty deck, unresolved ids, save path issues |
| `DeckAssetSaveService` | `DeckAssetSaveService.cs` | Writes staged deck to BandDeckData asset, handles Save/Save As paths |

All services are in `Assets/Scripts/Cards/Editor/` and are `#if UNITY_EDITOR` gated.

### 5.6 DTOs

`DeckEditorDtos.cs` defines the serialization DTOs used by `DeckJsonImportService`: `DeckJsonDto`, `DeckCardEntryDto`, `CardEffectDto`. These are the intermediate representations between raw JSON and the staged deck model.

---

## 6. Status Effect Wizard (`StatusEffectWizardWindow`)

**File:** `Assets/Scripts/Status/Editor/StatusEffectWizardWindow.cs`
**Namespace:** `ALWTTT.Status.Editor`

### 6.1 What it does

Creates and edits `StatusEffectSO` assets backed by the CSO (Character Status Ontology) primitive database. Ensures catalogue uniqueness and auto-registers new effects.

### 6.2 Layout

Two tabs with shared header:

- **Shared header** — Catalogue reference (`StatusEffectCatalogueSO`), CSO Primitive DB reference (`CharacterStatusPrimitiveDatabaseSO`). Auto-resolved on window open.
- **Create New tab** — Asset folder, EffectId picker (filtered to exclude ids already in catalogue), display name, ontology preview (category/abstract function from CSO), behavior draft fields (stack mode, max stacks, decay mode, duration turns, tick timing, value type, isBuff), Create button.
- **Edit Existing tab** — Dropdown of all effects in the catalogue, Ping/Select/Open buttons, inline property editor for all behavior fields with Revert/Apply.

### 6.3 Key workflows

**Create new status effect:**
Pick an unused `CharacterStatusId` from the filtered dropdown, set display name and behavior parameters, press Create. The wizard creates the `StatusEffectSO` asset, writes all fields via `SerializedObject`, saves it to disk, and registers it in the catalogue via `EditorTryAdd`. The asset is pinged and selected.

**Edit existing status effect:**
Select from the catalogue dropdown, edit fields in the inline inspector, Apply to persist changes.

### 6.4 Validation

- Duplicate `CharacterStatusId` prevention — the Create tab only shows ids not already in the catalogue.
- Missing catalogue/primitive DB warnings shown as help boxes.
- Invalid asset folder path rejected on create.

---

## 7. Chord Progression Catalogue Wizard (`ChordProgressionCatalogueWizard`)

**File:** `Assets/Scripts/Cards/Editor/ChordProgressionCatalogueWizard.cs`
**Namespace:** (global — should be namespaced, noted as minor cleanup)
**Menu:** MidiGenPlay → Chord Progression Catalogue Wizard...

### 7.1 What it does

Read-only browser for `ChordProgressionData` and `ChordProgressionPaletteSO` assets. Designers use it to find, filter, and inspect chord progressions by musical metadata. It does not create or edit assets — selecting a row pings/selects the asset in the Project window for standard Inspector editing.

### 7.2 Capabilities

- Configurable scan folders (defaults: `Assets/Resources/ScriptableObjects/Chord Progressions` and `Assets/Resources/Chord Progressions`).
- View modes: All, Progressions Only, Palettes Only.
- Sort by: Name, Path, Measures, Time Signature, Entry Count, Events Count. Ascending/descending.
- Filters: free-text search (blob-matched), time signature, measure range, subdivision range, tonality (multi-select with "include any-tonality" toggle), chord quality (multi-select).
- Palette rows show aggregated metadata from their contained progressions.

### 7.3 Boundary note

This tool operates on MidiGenPlay-owned data types (`ChordProgressionData`, `ChordProgressionPaletteSO`, `Tonality`, `ChordQuality`, `TimeSignature`). It is an ALWTTT-side convenience browser, not a MidiGenPlay authoring tool. It does not modify MidiGenPlay assets.

---

## 8. Card asset factory (`CardAssetFactory`)

**File:** `Assets/Scripts/Cards/Editor/CardAssetFactory.cs`
**Namespace:** `ALWTTT.Cards.Editor`

Static utility class that separates card asset creation logic from the editor windows.

**Contract:**
`TryCreateCard(CreateCardRequest, out CreateCardResult, out string error)` → creates a `CardDefinition` + correct `CardPayload` subclass (`ActionCardPayload` or `CompositionCardPayload`), wires them together via `SerializedObject`, saves to disk.

**Responsibilities:**
- Derive default folder from catalog location if not specified.
- Create nested Payloads subfolder.
- Wire: id, displayName, performerRule, musicianCharacterType, cardSprite, inspirationCost, inspirationGenerated, payload reference.
- Safe file naming.
- Error reporting if serialized field names change.

**Used by:** `CardEditorWindow` (create wizard), `DeckCardCreationService` (deck save with new cards).

---

## 9. Musician catalog service (`MusicianCatalogService`)

**File:** `Assets/Scripts/Cards/Editor/MusicianCatalogService.cs`
**Namespace:** `ALWTTT.Cards.Editor`

Static editor-only helpers for safe catalog mutation.

- `ContainsCard(catalog, card)` — duplicate check.
- `TryAddEntry(catalog, card, flags, starterCopies, unlockId, out index, out error)` — adds entry with Undo support and dirty marking.

**Used by:** `CardEditorWindow` (add existing card, create wizard post-creation).

---

## 10. Composition card classifier (`CompositionCardClassifier`)

**File:** `Assets/Scripts/Cards/Composition/CompositionCardClassifier.cs`
**Namespace:** `ALWTTT.Cards`

Runtime-available (not editor-only) static classifier for composition card payloads. Used for UI classification and session logic, not for data authoring.

Classification methods: `AffectsSound`, `IsTempoCard`, `IsTimeSignatureCard`, `IsInstrumentCard`, `IsTonalityCard`, `IsModulationCard`.

Checks are based on concrete effect subclass types (`TempoEffect`, `MeterEffect`, `InstrumentEffect`, `TonalityEffect`, `ModulationEffect`) and `CardPrimaryKind` (Track/Part).

**Relevance to editor tools:** Can be used by editor tools for card preview info and catalogue filtering (M1.1 planned work).

---

## 11. Composition descriptors

### 11.1 `PartActionDescriptor`

**File:** `Assets/Scripts/Cards/Composition/PartActionDescriptor.cs`

Serializable descriptor for structural part actions: `PartActionKind` (CreatePart, MarkIntro, MarkBridge, MarkSolo, MarkOutro, MarkFinal), optional custom label, optional musician id for solos.

### 11.2 `TrackActionDescriptor`

**File:** `Assets/Scripts/Cards/Composition/TrackActionDescriptor.cs`

Serializable descriptor for track targeting: `TrackRole` (Rhythm/Backing/Bassline/Melody/Harmony), optional `TrackStyleBundleSO` reference.

Both are authored as fields on `CompositionCardPayload` and edited via the Card Editor's composition payload section.

---

## 12. File location summary

All editor tool source files:

```
Assets/Scripts/Cards/Editor/
  CardEditorWindow.cs              (partial)
  CardEditorWindow.JsonImport.cs   (partial)
  CardAssetFactory.cs
  MusicianCatalogService.cs
  DeckEditorWindow.cs
  DeckEditorDtos.cs
  DeckJsonImportService.cs
  DeckCardCreationService.cs
  DeckValidationService.cs
  DeckAssetSaveService.cs
  ChordProgressionCatalogueWizard.cs

Assets/Scripts/Cards/Composition/
  CompositionCardClassifier.cs
  PartActionDescriptor.cs
  TrackActionDescriptor.cs

Assets/Scripts/Status/Editor/
  StatusEffectWizardWindow.cs
```

---

## 13. Known gaps and limitations

### 13.1 Deck Editor polish (M1.1)

- Catalogue filters are basic: action/composition toggle + text search only. No filtering by musician, effect type, rarity, or acquisition flags.
- Staged card list shows card name only — no effect summary, cost, or kind badge.
- No cross-tool integration: cannot Open in Card Editor or Ping Card in Project from the staged list.

### 13.2 Status Icons pipeline disconnected (M1.2)

`StatusIconsData` and `StatusIconData` (in `Assets/Scripts/Data/UI/StatusIconsData.cs`) are keyed on the legacy `StatusType` enum. None of the six Combat MVP statuses (`flow`, `composure`, `exposed`, `feedback`, `choke`, `shaken`) exist in that enum. The icon pipeline is currently completely disconnected from all working status effects. Migration to SO-based keying is required.

### 13.3 Tooltip pipeline limited (M1.3)

Card tooltips use `CardDefinition.Keywords` → `TooltipManager.SpecialKeywordData` lookup only. No connection to card effects, status effect descriptions, or composition card modifiers.

### 13.4 ChordProgressionCatalogueWizard namespace

The class is in the global namespace. Should be moved to `ALWTTT.Cards.Editor` or a dedicated namespace for consistency.

### 13.5 True card copies in decks

The current deck contract is unique-card-list only. True copy semantics (`BandDeckEntry { card, copies }`) are not implemented in either the editor or runtime. This is a known future evolution, not a current bug.

### 13.6 No Dev Mode gig scene (M1.5)

No sandbox scene exists for runtime card/status/composition testing. All testing currently requires configuring a full gig encounter.

---

## 14. Cross-references

| Topic | Governed home |
|---|---|
| Card data contracts, JSON schema, effect list representation | `SSoT_Card_Authoring_Contracts.md` |
| Card gameplay semantics (action vs composition, payload model) | `SSoT_Card_System.md` |
| Status effect runtime semantics, catalogue, SO model | `SSoT_Status_Effects.md` |
| Composition session and song pipeline | `SSoT_Runtime_CompositionSession_Integration.md` |
| ALWTTT ↔ MidiGenPlay boundary | `SSoT_ALWTTT_MidiGenPlay_Boundary.md` |
| Runtime phase flow, deck/hand pipeline | `SSoT_Runtime_Flow.md` |
| Active roadmap (M1 tasks referencing these tools) | `Roadmap_ALWTTT.md` |

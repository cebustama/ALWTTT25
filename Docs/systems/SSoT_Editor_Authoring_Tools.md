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
| Card Inventory | `CardInventoryWindow` | ALWTTT → Cards → Card Inventory | Read-only browser for `CardDefinition`, `MusicianCardCatalogData`, and `GenericCardCatalogSO` assets, with Print to Console + Export JSON per view |
| Status Effect Wizard | `StatusEffectWizardWindow` | ALWTTT → Status → Status Effect Wizard | Create and edit StatusEffectSO assets backed by the CSO primitive database |
| Chord Progression Catalogue | `ChordProgressionCatalogueWizard` | MidiGenPlay → Chord Progression Catalogue Wizard... | Read-only browser for ChordProgressionData and ChordProgressionPaletteSO assets |

All five are `#if UNITY_EDITOR` gated `EditorWindow` subclasses. None ship in builds.

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

### 4.6 Per-row Starter / Copies columns (batch (3), 2026-05-03)

Each row in the catalog entry list renders an inline `Starter` checkbox (~38 px wide) and a `Copies` IntField (~40 px wide) before the row's selectable name button. Both controls write through `SerializedObject(_loadedCatalog)` → `entries.GetArrayElementAtIndex(i)` → `FindPropertyRelative("flags")` / `FindPropertyRelative("starterCopies")`, with `ApplyModifiedProperties()` per frame. This gives Undo registration and asset-dirty propagation for free, identical to the right-side inspector path.

Behavior:

- The `Starter` checkbox toggles `CardAcquisitionFlags.StarterDeck` directly on the entry. The `[S]` flag indicator is no longer included in the row's text label because the checkbox column is the canonical indicator. `[R]` (reward) and `[L]` (locked) tokens remain in the label.
- The `Copies` IntField is greyed out when `Starter` is off, editable when on. On commit, the field clamps to `Mathf.Max(1, value)` (mirrors `MusicianCatalogService.TryAddEntry` and the `[Min(1)]` attribute on `MusicianCardEntry.starterCopies`).
- IMGUI controls consume their own input events, so clicking the checkbox or IntField on a non-selected row does not change `_selectedEntryIndex`. Selection still requires clicking the row's name-label button.
- When the `Starter Only` filter is active and the user un-checks Starter on a visible row, the row disappears from the filtered view on the next Repaint. This is intentional silent behavior matching the right-side inspector workflow; no HelpBox is displayed.
- Single-step Undo (`Ctrl+Z`) reverts both the flag and the copies value as one operation, because both writes happen inside the same `SerializedObject` transaction.

The dogfood acceptance test (ST-AT3-8) confirmed this UI is materially faster for the M4.6 starter-deck cleanup workflow than the previous "open right-side inspector → click `EnumFlagsField` dropdown → uncheck Starter" path.

### 4.7 Print button (batch (3), 2026-05-03)

The Card Editor toolbar gains a `Print` button (after the Registries Ping button, separated by a `GUILayout.Space(10)`). When pressed with a catalog loaded, it produces a multi-line `Debug.Log` of the catalog contents:

```
=== CARD EDITOR — CATALOG DUMP ===
Musician: Conito
Catalog: Conito_CardCatalogData (Assets/Resources/Data/Characters/Musicians/Conito_CardCatalogData.asset)
Entries: 10 (starter entries: 10, total starter copies: 10)

[1] test_draw_cards — Action, flags=[StarterDeck, UnlockedByDefault], copies=1, unlockId=<none>
[2] test_modify_vibe — Action, flags=[StarterDeck, UnlockedByDefault], copies=1, unlockId=<none>
...
```

The button is disabled when no catalog is loaded. Symmetric to `DeckEditorWindow`'s `Print` button (§5.7).

### 4.8 Registries surface (post-MB2)

`ALWTTTProjectRegistriesSO` exposes both status catalogues separately:
- `StatusCatalogueMusicians` — musician-side statuses (flow, composure, choke, shaken, exposed, feedback).
- `StatusCatalogueAudience` — audience-side statuses (earworm; future audience-side statuses).

Plus two cross-catalogue lookup helpers used by tooling:
- `TryGetStatusEffectByKey(string, out StatusEffectSO)` — probes musicians first, then audience.
- `TryGetStatusEffectByPrimitive(CharacterStatusId, out StatusEffectSO)` — same probe order. Note: when both catalogues hold a variant of the same primitive (e.g. Feedback / Earworm both use `DamageOverTime`), this returns the musicians variant. Prefer key-based lookup for unambiguous audience-side resolution.

A legacy `StatusCatalogue` alias is retained for source compatibility with pre-MB2 callers; it returns the musicians catalogue. New tooling code should use the explicit `…Musicians` / `…Audience` properties or the `TryGet…` helpers.

---

## 5. Deck Editor (`DeckEditorWindow`)

**File:** `Assets/Scripts/Cards/Editor/DeckEditorWindow.cs`
**Namespace:** `ALWTTT.Cards.Editor`

### 5.1 What it does

The Deck Editor creates and edits `BandDeckData` assets — the deck containers used by the runtime deck/hand pipeline. It supports both visual catalogue-based editing and JSON-first workflows.

### 5.2 Layout

Three-zone vertical layout:

- **Header/toolbar** — Target deck asset field, GigSetupConfigData field, Load/Save/Save As/New/Find/Ping buttons, Import JSON/Export JSON buttons, Print button (batch (3), §5.7).
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

Decks are unique card lists with M4.4 multiplicity support: `BandDeckData` is now a multiset, with `BandDeckEntry { card, count }` as the per-entry shape. See `SSoT_Card_System.md §13` and `SSoT_Card_Authoring_Contracts.md §5.10` for the full multiplicity contract. The Deck Editor edits `count` via inline +/- controls per staged entry.

### 5.5 Supporting services

| Service | File | Responsibility |
|---|---|---|
| `DeckJsonImportService` | `DeckJsonImportService.cs` | Parses JSON → staged deck entries, resolves card references, stages new-card creation requests |
| `DeckCardCreationService` | `DeckCardCreationService.cs` | Creates CardDefinition + payload assets for staged new cards during save |
| `DeckValidationService` | `DeckValidationService.cs` | Validates staged deck: null refs, empty deck, unresolved ids, save path issues |
| `DeckAssetSaveService` | `DeckAssetSaveService.cs` | Writes staged deck to BandDeckData asset, handles Save/Save As paths |

All services are in `Assets/Scripts/Cards/Editor/` and are `#if UNITY_EDITOR` gated.

### 5.6 DTOs

`DeckEditorDtos.cs` defines the staged deck model used by the window: `StagedDeck` and `StagedCardEntry`. `StagedCardEntry` exposes `existingCard` (serialized `CardDefinition` reference), `pendingCard`/`pendingPayload` (in-memory only, lost on domain reload), and `count` (M4.4 multiplicity). Property `ResolvedCard` returns either the existing or the pending card, whichever is set. Properties `IsNew`, `IsExisting`, `IsValid` discriminate the two modes.

### 5.7 Print button (batch (3), 2026-05-03)

The Deck Editor toolbar gains a `Print` button immediately after the `Export JSON` button on row 1. When pressed it produces a multi-line `Debug.Log` of the staged deck:

```
=== DECK EDITOR — STAGED DECK DUMP ===
Asset: MyDeck (Assets/Resources/Data/Decks/MyDeck.asset)
deckId: my_deck
displayName: My Deck
description: ...
Entries: 5 (total copies: 8)

[1] card_id_1 ×2 — Action
[2] card_id_2 ×1 — Composition
...
```

The formatter uses `StagedCardEntry.ResolvedCard` (which transparently picks the right reference for both existing and pending entries) and reports `count` per row. Pending new cards display a trailing `[NEW]` suffix. Symmetric to `CardEditorWindow`'s `Print` button (§4.7).

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

## 8. Card Inventory Window (`CardInventoryWindow`) — batch (3), 2026-05-03

**File:** `Assets/Scripts/Cards/Editor/CardInventoryWindow.cs`
**Namespace:** `ALWTTT.Cards.Editor`
**Menu:** ALWTTT → Cards → Card Inventory (priority 12, immediately after Card Editor and Deck Editor)

### 8.1 What it does

Read-only inventory browser for the project's card-related ScriptableObject assets. Surfaces a quick visual overview without requiring the user to navigate the Project window or open individual catalogs in the Card Editor. Each view supports a `Print` action (multi-line `Debug.Log`) and an `Export JSON` action (file dialog → `JsonUtility.ToJson(_, prettyPrint: true)`).

The window does not mutate any asset. All editing flows continue to live in the Card Editor and Deck Editor.

### 8.2 Layout

Single window with a top toolbar selecting one of four views:

- **All CardDefinitions** — every `CardDefinition` asset in the project, listed with kind badge (`[A]`/`[C]`/`[?]`), id, display name, inspiration cost, and a Ping button per row.
- **All Musician Catalogs** — every `MusicianCardCatalogData` asset, listed with musician type, asset name, total entry count, starter entry count, total starter copies (sum of `starterCopies` across starter-flagged entries), and a Ping button per row.
- **One Musician** — full entry list of a single musician's catalog, selected via a dropdown in the toolbar that appears only on this view. Shows id, starter status (`S×{copies}` when flagged, `—` otherwise), reward marker, unlocked-by-default marker, and unlock id.
- **All Generic Catalogs** — every `GenericCardCatalogSO` asset, each rendered as a heading row with entry count + Ping button followed by the same per-entry shape as the One Musician view. `GenericCardCatalogSO.Entries` reuses `MusicianCardEntry`, so the entry rendering is shared between the two views.

The toolbar's right-aligned actions (`Print`, `Export JSON`) operate on whichever view is currently selected.

### 8.3 Print to Console

Produces a multi-line `=== CARD INVENTORY — {ViewName} ===` block in the Console:

- View 1 (All CardDefinitions): one line per card with id, kind, cost, asset path.
- View 2 (All Musician Catalogs): one line per catalog with musician type, asset name, entry count, starter count, starter copies total.
- View 3 (One Musician): asset-name header followed by indented per-entry lines with id, flags, starter copies, unlock id.
- View 4 (All Generic Catalogs): same per-catalog shape as View 3, repeated for each generic catalog.

### 8.4 Export JSON

`EditorUtility.SaveFilePanel` → writes a pretty-printed JSON file via `JsonUtility.ToJson(_, true)`. The export schema is **informational and human-readable; it is not designed to be re-imported through `DeckJsonImportService` or any catalog import path**. It exists for debugging, audit logging, and external review (e.g. paste into a sheet, diff between two snapshots, share with another developer).

Per-view schemas:

- View 1: `{ "cardDefinitions": [{ "id", "displayName", "kind", "inspirationCost", "assetPath" }, ...] }`
- View 2: `{ "catalogs": [{ "musicianType", "assetName", "entryCount", "starterCount", "starterCopiesTotal" }, ...] }`
- View 3: `{ "catalogs": [{ "assetName", "musicianType", "entries": [{ "cardId", "flags", "starterCopies", "unlockId" }, ...] }] }`
- View 4: same shape as View 3, with `"musicianType": "<generic>"`.

After save, the file is auto-revealed in the OS file browser via `EditorUtility.RevealInFinder`.

### 8.5 Asset discovery

Uses `AssetDatabase.FindAssets("t:{TypeName}")` for all four asset types, then `AssetDatabase.LoadAssetAtPath<T>` per result. No caching — discovery runs every render frame. This is acceptable because the tool is editor-only and the asset counts are small (low tens to low hundreds of assets project-wide).

### 8.6 Boundary note

This tool is a **viewer**, not an authoring surface. It does not own any asset semantics. The Card Editor (§4) remains the authority for `MusicianCardCatalogData` editing; the Deck Editor (§5) remains the authority for `BandDeckData` editing; standard Unity Inspector handles `GenericCardCatalogSO` editing (or future tooling promotes it).

---

## 9. Card asset factory (`CardAssetFactory`)

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

## 10. Musician catalog service (`MusicianCatalogService`)

**File:** `Assets/Scripts/Cards/Editor/MusicianCatalogService.cs`
**Namespace:** `ALWTTT.Cards.Editor`

Static editor-only helpers for safe catalog mutation.

- `ContainsCard(catalog, card)` — duplicate check.
- `TryAddEntry(catalog, card, flags, starterCopies, unlockId, out index, out error)` — adds entry with Undo support and dirty marking.

**Used by:** `CardEditorWindow` (add existing card, create wizard post-creation).

The Card Editor's per-row Starter / Copies columns (§4.6) do not call into this service because they mutate existing entries rather than adding new ones; they go through `SerializedObject` + `ApplyModifiedProperties` directly, which provides equivalent Undo and dirty-flag guarantees.

---

## 11. Composition card classifier (`CompositionCardClassifier`)

**File:** `Assets/Scripts/Cards/Composition/CompositionCardClassifier.cs`
**Namespace:** `ALWTTT.Cards`

Runtime-available (not editor-only) static classifier for composition card payloads. Used for UI classification and session logic, not for data authoring.

Classification methods: `AffectsSound`, `IsTempoCard`, `IsTimeSignatureCard`, `IsInstrumentCard`, `IsTonalityCard`, `IsModulationCard`.

Checks are based on concrete effect subclass types (`TempoEffect`, `MeterEffect`, `InstrumentEffect`, `TonalityEffect`, `ModulationEffect`) and `CardPrimaryKind` (Track/Part).

**Relevance to editor tools:** Can be used by editor tools for card preview info and catalogue filtering (M1.1 planned work).

---

## 12. Composition descriptors

### 12.1 `PartActionDescriptor`

**File:** `Assets/Scripts/Cards/Composition/PartActionDescriptor.cs`

Serializable descriptor for structural part actions: `PartActionKind` (CreatePart, MarkIntro, MarkBridge, MarkSolo, MarkOutro, MarkFinal), optional custom label, optional musician id for solos.

### 12.2 `TrackActionDescriptor`

**File:** `Assets/Scripts/Cards/Composition/TrackActionDescriptor.cs`

Serializable descriptor for track targeting: `TrackRole` (Rhythm/Backing/Bassline/Melody/Harmony), optional `TrackStyleBundleSO` reference.

Both are authored as fields on `CompositionCardPayload` and edited via the Card Editor's composition payload section.

---

## 13. File location summary

All editor tool source files:

```
Assets/Scripts/Cards/Editor/
  CardEditorWindow.cs              (partial)
  CardEditorWindow.JsonImport.cs   (partial)
  CardInventoryWindow.cs           (batch (3), 2026-05-03)
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

## 14. Known gaps and limitations

### 14.1 Status Icons pipeline disconnected (M1.2 — RESOLVED 2026-04-14)

`StatusIconsData` legacy lookup retired; sprite authority now lives on `StatusEffectSO.IconSprite`. See `SSoT_Status_Effects.md §3.3`.

### 14.2 Tooltip pipeline limited (M1.3a/b/c — RESOLVED 2026-04-23)

Card-effect text builder, hover tooltips with stacked keywords + statuses, right-click detail modal all shipped under M1.3 decomposition. Composition card face minimal display remains by design (covered by detail modal §10.3 in `SSoT_Card_System.md`).

### 14.3 ChordProgressionCatalogueWizard namespace

The class is in the global namespace. Should be moved to `ALWTTT.Cards.Editor` or a dedicated namespace for consistency.

### 14.4 No Dev Mode gig scene (M1.5)

No sandbox scene exists for runtime card/status/composition testing. M1.5 Phase 1–3 shipped Dev Mode overlay capabilities; the standalone sandbox scene remains a separate item.

### 14.5 Inventory viewer two-prefab arrangement (logged 2026-05-02 from UI-fix-A; appendix to batch (3) deferred 2026-05-03)

`CardUI : CardBase {}` is an empty subclass formalizing a two-prefab arrangement (gameplay card prefab + `CardUI.prefab` for the inventory canvas). Every `[SerializeField]` field added to `CardBase` going forward must be wired on both prefabs or the inventory side will NRE on `CardBase.SetCard`. Cleanup options logged in `CURRENT_STATE.md §4`: (α) collapse to a single prefab with view-only mode, (β) `CardUI.prefab` as Prefab Variant. A "Validate `CardBase` prefab variants" Card Editor action — which would reflect over `[SerializeField]` fields and report unwired refs at authoring time — was considered as a candidate appendix to batch (3) and deferred. Logged here as a candidate authoring-tool addition for a future QoL pass.

### 14.6 Card Editor inline effects-block UI on legacy catalogue alias (logged 2026-05-01)

`CardEditorWindow.cs` `DrawEffectsBlock` calls receive `_registries?.StatusCatalogue` — the legacy alias that exposes only the musicians catalogue. Audience-side statuses (e.g. `earworm`) are not visible in the inline effect-row dropdown for direct card editing. Affects authoring UX, not import resolution. No timeline; track until it bites.

### 14.7 True card copies in decks — RESOLVED (M4.4, 2026-04-29)

`BandDeckData` is now a multiset; the Deck Editor edits `count` per staged entry. See `SSoT_Card_System.md §13` and `SSoT_Card_Authoring_Contracts.md §5.10`.

---

## 15. Cross-references

| Topic | Governed home |
|---|---|
| Card data contracts, JSON schema, effect list representation | `SSoT_Card_Authoring_Contracts.md` |
| Card gameplay semantics (action vs composition, payload model) | `SSoT_Card_System.md` |
| Status effect runtime semantics, catalogue, SO model | `SSoT_Status_Effects.md` |
| Composition session and song pipeline | `SSoT_Runtime_CompositionSession_Integration.md` |
| ALWTTT ↔ MidiGenPlay boundary | `SSoT_ALWTTT_MidiGenPlay_Boundary.md` |
| Runtime phase flow, deck/hand pipeline | `SSoT_Runtime_Flow.md` |
| Active roadmap (M1 tasks referencing these tools) | `Roadmap_ALWTTT.md` |

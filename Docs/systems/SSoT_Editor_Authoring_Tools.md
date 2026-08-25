# SSoT_Editor_Authoring_Tools — ALWTTT

**Status:** Active governed SSoT
**Scope:** Unity editor tools for authoring cards, decks, status effects, and musical data, plus read-only inventory/curation browsers over card and composition assets
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
| Composition Inventory | `CompositionInventoryWindow` | ALWTTT → Dev → Composition Inventory | Read-only browser over every composition asset family (style bundles, drum/chord/melody patterns, palettes, libraries, phrase archetypes, melodic + percussion instruments) with filters, derived health columns, Print + Export JSON, and a naming report (CSV-1, 2026-07-18; see §17) |
| Effect Editor | `PartEffectEditorWindow` | ALWTTT → Cards → Effect Editor | Authoring surface for the PartEffect asset family: list/filter/search, inline editing, Create (default `_PartEffects/`), Duplicate, Delete with usage scan, Find Usages, Export JSON (AUTH-1, 2026-07-31; see §18) |

All seven are `#if UNITY_EDITOR` gated `EditorWindow` subclasses. None ship in builds. `CompositionInventoryWindow` additionally requires `ALWTTT_DEV` (D-CSV-10=A).

---

## 4. Card Editor (`CardEditorWindow`)

**File:** `Assets/Scripts/Cards/Editor/CardEditorWindow.cs` (partial class, with `CardEditorWindow.JsonImport.cs` and `CardEditorWindow.LLM.cs`)
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

**Role presets (CE-E1).** Alongside the generic *New Action* / *New Composition* buttons, the create panel offers one button per track role — **Rhythm / Backing / Melody / Harmony / Bassline**. A role preset sets `trackAction.role` on the new payload and auto-mints a correctly-typed `TrackStyleBundleSO` for it via `CreateAndAssignStyleBundle` (`ResolveBundleTypeForRole`: Bassline ⇒ `BasslineCardConfigSO`, Backing ⇒ `BackingCardConfigSO`, Melody ⇒ `MelodyCardConfigSO`, Harmony ⇒ `HarmonyCardConfigSO`, Rhythm ⇒ `RhythmCardConfigSO`). This is the shortest path to a working Track card, and it guarantees the bundle is non-null — which, per `SSoT_Card_Authoring_Contracts.md` §5.14, is what makes a Track card create a track at all.

**Add existing card:**
An object field lets the user drag an existing `CardDefinition` and add it to the current musician's catalog.

**JSON batch import:**
The partial class `CardEditorWindow.JsonImport.cs` provides batch card creation from JSON. This follows the schema defined in `SSoT_Card_Authoring_Contracts.md`. Since CE-L1 B3 (2026-06-11), parsing delegates to the shared `CardImportDtoParser` (in `Assets/Scripts/Cards/LLMAuthoring/`) — the same DTO parse the LLM routes use — and `TryStageCardFromDto` additionally resolves `composition.modifierEffectNames` (name → `PartEffect` asset, exact case-insensitive, all-or-nothing). Since BASS-CARD-1 (2026-07-12) the batch route also honours `composition.trackAction.styleBundleCreate` — it **mints a role-typed style bundle** at Save (via the same `CreateAndAssignStyleBundle` the wizard's role buttons use) and applies type-coerced field writes to it, with unknown field names failing loudly and listing the bundle's valid fields. This is what makes Bassline cards authorable from JSON. The batch route still does **not** resolve `composition.palette` intent; that remains LLM-route-only (see §4.10 and `SSoT_Card_Authoring_Contracts.md` §5.12–§5.13).

### 4.4 Filters

- Show Action / Show Composition (kind toggle)
- Starter Only, Reward Only, Locked Only (acquisition flag filters)
- Filter by StatusId (shows only cards whose effects reference a specific `CharacterStatusId`)

### 4.5 Dependencies

- `CardAssetFactory` — asset creation logic (separated from the window)
- `MusicianCatalogService` — catalog mutation (add entry with Undo support)
- `ALWTTTProjectRegistriesSO` — auto-resolved singleton for musician/catalog lookup
- `MusicianCharacterData`, `MusicianCardCatalogData` — musician-scoped data assets
- `ALWTTT.Cards.LLMAuthoring` (editor-only asmdef, `Assets/Scripts/Cards/LLMAuthoring/`) — shared DTOs/parser, vocabulary snapshot, palette-intent resolver, prompt builder, generator, response handler, field plan (used by §4.10) (CE-L1, 2026-06-11)
- `BCS.LLM.Core` — `LLMClientData` + `PromptExecutionHelper` LLM client seam (used by §4.10) (CE-L1, 2026-06-11)

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

### 4.9 Catalog Source toggle and classified status dropdown (M4.6-prep cleanup, 2026-05-06)

**Catalog Source toggle.** A `CatalogSource { Musician, Generic }` enum toggle at the top of the toolbar selects which catalog the editor reads. `Musician` mode preserves prior behavior (musician dropdown loads the corresponding `MusicianCardCatalogData` via `MusicianCharacterData.CardCatalog`). `Generic` mode auto-loads a `GenericCardCatalogSO` asset via `AssetDatabase.FindAssets("t:GenericCardCatalogSO")` with a name-heuristic preference for assets whose filename does not contain "Test".

The toggle handler at the source-change site clears `_loadedCatalog` and `_loadedMusicianData` when switching to Generic, and clears `_loadedGenericCatalog` when switching back to Musician, so the loaded-state cache cannot mis-route writes between modes. Combined with the write-disable guards (writes are blocked while `_catalogSource == Generic` for all of Create Card / JSON Import / Add Existing / Sync From Assets), the toggle is safe in its current shape.

Generic mode renders the entry list with the per-row `Starter` checkbox + `Copies` IntField from batch (3.A). Read-side parity with Musician mode is full. Write paths (Create Card, JSON Import, Add Existing, Sync From Assets) are **NOT** Generic-aware in this iteration — they remain disabled in Generic mode, and Generic write-side support is deferred as a future tooling QoL batch (touches `CardAssetFactory.CreateCardKindParams` and `MusicianCatalogService` signatures, both currently typed to `MusicianCardCatalogData`).

**Classified status dropdown.** `DrawStatusEffectPicker` (the inline status picker used inside `ApplyStatusEffectSpec` effect rows on a card payload) now consumes an `ALWTTTProjectRegistriesSO` (instead of the legacy single `StatusEffectCatalogueSO` alias) and reads from both `StatusCatalogueMusicians` and `StatusCatalogueAudience`. UI is rendered via `EditorGUILayout.DropdownButton` + `GenericMenu` with hierarchical paths `Musicians/<DisplayName>` and `Audience/<DisplayName>`, plus a `<None>` entry at the top of the menu. Closes the post-MB2 friction documented in `CURRENT_STATE.md §4` ("Card Editor inline effects-block UI on legacy catalogue alias"): the inline effects-block UI now matches the JSON-import path's two-catalogue probing.

### 4.10 "Generate with LLM" panel (CE-L1, 2026-06-11)

**File:** `Assets/Scripts/Cards/Editor/CardEditorWindow.LLM.cs` (partial-class extension)

A foldout panel providing LLM-assisted single-card authoring. Pattern adopted from MidiGenPlay's drum/chord LLM panels (third adopter) — pattern authority is MidiGenPlay `authoring/SSoT_Authoring_LLM_Generation.md` §7 (link, do not duplicate). End-to-end mechanism **ya no tiene documento**: `reference/Report_CardLLM_Pipeline.md` fue retirado y confirmado inexistente el 2026-08-08 (MANIFEST-1, señal F16). El código es hoy la única descripción del pipeline de siete etapas. Re-documentarlo aquí o aceptar solo-código es una decisión pendiente, no una limpieza.

**Panel inputs:** optional `LLMClientData` override (when empty, the first `LLMClientData` asset in the project is used); free-text brief; card-kind hint; track-role hint (Backing / Melody / Harmony / Rhythm / Bassline, or "let the model decide"); **intent seed** (seeds the deterministic palette pick — same payload + same seed ⇒ same palette; Randomize button); max-prompt-chars budget (pre-network cost cap; `0` = no cap; an over-budget prompt fails before anything is sent).

**Buttons:** *Generate* (single-shot LLM call; `async void`, non-blocking — never `.Result`/`.Wait()`, which would deadlock the editor main thread) and *Import from clipboard* (same validation pipeline, no LLM call). Both converge on `CardLLMResponseHandler` and stage through the window's existing `TryStageCardFromDto` — bit-for-bit the path a hand-pasted JSON takes. Nothing touches disk until the user presses the existing **Save (Create Assets)** button.

**Boundary rules (normative):**

- The LLM fills **structured fields only**. Asset references are banned: any non-empty `cardSpritePath`, `trackAction.styleBundle`, or `modifierEffects` path entry — a path/guid-shaped string smuggled into `modifierEffectNames` — or a `trackAction.styleBundleCreate` intent (BASS-CARD-1: its `fields` can carry asset paths, and the LLM's bundle is minted by the field plan anyway) — is a hard failure listing every violation. Nothing is staged.
- Every emitted token is re-validated against a live vocabulary snapshot built at click time by `CardLLMVocabularyBuilder` (enum alphabets, status keys from both registry catalogues, `PartEffect` modifier names, palette descriptors). Out-of-alphabet tokens are hard failures (every violation listed at once); omitted fields legitimately take defaults.
- The LLM never names a palette asset. It emits **palette intent** (`composition.palette` — `requested`, optional `timeSignature`, optional `keywords`), resolved deterministically and seeded by `CardPaletteIntentResolver` over the project's real palettes via the CE-F1 `PaletteSelector`. Rhythm role → drum palettes, Backing role → chord palettes; Melody / Harmony / **Bassline** intent fails loudly (no palette type exists for those roles — a `BasslineCardConfigSO` carries articulation only); unmatched keywords fail listing the available palettes.
- Modifier effects are referenced **by exact name** via `composition.modifierEffectNames`; resolution at staging is case-insensitive and all-or-nothing (missing fails listing available, ambiguous fails listing colliders).
- At Save, `ApplyLlmPlanOnSave` (called by `SaveStagedJsonToAssetsAndAddToCatalog` after the card/payload assets exist) creates the role bundle via the existing `CreateAndAssignStyleBundle` and assigns the resolved palette to the bundle's `patternPalette` / `progressionPalette`. No-op for non-LLM saves. Discarding the staged card clears the pending plan.
- The card sprite is always the staging path's **musician default** — never LLM-chosen.

**Status surface:** info/error status line (`HelpBox`), separate warning box, and a `Last call: N input / N output tokens` label. A leading warning is shown when registries are unresolved (status keys then come from an asset scan rather than the catalogues — an unregistered key fails loudly at staging, never silently wrong).

**Known gap:** there is no UI affordance to view the exact prompt; a "Copy prompt" button across all three LLM panels is noted future work (see the reference report).

---

### 4.11 Nav strip + PartEffect shortcuts (AUTH-1, 2026-07-31)

The toolbar gains the shared nav strip (§19). The composition payload editor
draws, under the `modifierEffects` PropertyField, a read-only "PartEffects"
shortcut box: one row per assigned effect (type + `GetLabel()` + Edit + Ping)
plus a "New Effect…" jump into the Effect Editor (§18). Assignment itself stays
on the PropertyField; the shortcut box never mutates the list.

**D-AUTH1-1=A:** the embedded payload panel remains the payload-editing
authority — no separate payload window. **D-AUTH1-0** (ratified at AUTH-1 open):
the PartEffect asset family gets its own window rather than growing this one,
because PartEffects are a shared asset family referenced across cards while this
window is card-scoped.

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

### 5.8 Nav strip (AUTH-1, 2026-07-31)

`OnGUI` opens with the shared nav strip (§19). No other Deck Editor behaviour
changed in AUTH-1; the existing validation panel and layout are untouched.

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
Pick an unused `CharacterStatusId` from the filtered dropdown, set display name and behavior parameters, press Create. The wizard creates the `StatusEffectSO` asset, writes `effectId`, `displayName`, `statusKey` (D-R5-3=A), `primitiveDatabase` and the Behavior block via `SerializedObject`, saves it to disk, and registers it in the catalogue via `EditorTryAdd`. The asset is pinged and selected.

**Mandatory manual steps after Create (R5-a, 2026-08-21).** The wizard does **not** write
`isDefaultVariant`, `iconSprite` or `description`. All three must be filled in by hand in the
Inspector before the status counts as authored; none of the three raises an error when missing:

- `iconSprite` null ⇒ the status applies correctly but is **invisible** in the HUD; only a
  `CharacterCanvas` `LogWarning` reveals it.
- `description` empty ⇒ the card tooltip renders with an empty body.
- `isDefaultVariant = false` ⇒ harmless today, because `RebuildCache` adopts the first one seen
  as the default; it breaks silently as soon as a second variant of the same primitive exists.

**The asset is named after the primitive, not the variant.** `CreateAsset` names it
`StatusEffect_{EffectId}`, so a second status on the same primitive yields
`StatusEffect_ResourceCounter 1` via `GenerateUniqueAssetPath` — already registered in the
catalogue. Rename to `StatusEffect_<Variant>` after creating; the GUID is stable and catalogue
references do not break. An accidental duplicate stays registered in the catalogue and
`RebuildCache` indexes the first one seen, leaving the second unreachable via `TryGetByKey`.

*(Observed in R5-a, 2026-08-21, while authoring Voltage. Tool debt: write the three fields from
the wizard and name the asset after `displayName`.)*

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

### 8.7 Detailed print + cross-links (AUTH-1, 2026-07-31)

A toolbar `Detailed` toggle (default ON) extends **Print** only — the on-screen
rows and the JSON export schemas (§8.4) are unchanged, and with Detailed OFF the
print output is the pre-AUTH-1 format. When ON, each card line is followed by an
indented parameter dump:

- **Common:** display name, performer rule (or fixed musician), inspiration
  generated, rarity, card type, exhaust flag.
- **Action:** `actionTiming`, legacy `conditions` count, and the `effects` list
  with per-spec parameters (plain-text formatter, deliberately separate from
  `CardEffectDescriptionBuilder`, which emits TMP rich text for players; unknown
  spec types fall back to the type name).
- **Composition:** `primaryKind`; `trackAction` role + bundle asset (name +
  concrete type) + a depth-1 dump of the bundle's visible serialized fields
  (read-only reflection over MidiGenPlay-owned SOs — no type coupling, arrays
  print size only; boundary-safe); `partAction` (action / customLabel /
  musicianId); `modifierEffects` (type, asset name, scope, timing, `GetLabel()`);
  and the `effects` list as above.
- **Catalog entries** (Views 3/4): flags, starter copies, unlock id as before,
  with the card dump nested under each entry.

Cross-links: every card row gains an **Edit** button →
`CardEditorWindow.OpenAndSelect`. The window still mutates nothing (§8.6
stands); Edit is navigation, not editing.

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

Serializable descriptor for track targeting: `TrackRole` (Rhythm/Backing/Bassline/Melody/Harmony) + a `TrackStyleBundleSO` reference. The bundle field is **nullable at the serialization level but not optional in meaning** (BASS-1 D4=A, 2026-07-12): a Track card with a null `styleBundle` never creates a track — it augments the target musician's existing track *of that same role* if one exists, and is otherwise a PartEffect carrier only. Authoring contract: `SSoT_Card_Authoring_Contracts.md` §5.14; authority: `runtime/SSoT_Runtime_CompositionSession_Integration.md` §11.

Both are authored as fields on `CompositionCardPayload` and edited via the Card Editor's composition payload section.

---

## 13. File location summary

All editor tool source files:

```
Assets/Scripts/Cards/Editor/
  CardEditorWindow.cs              (partial)
  CardEditorWindow.JsonImport.cs   (partial)
  CardEditorWindow.LLM.cs          (partial; CE-L1, 2026-06-11)
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
  PartEffectEditorWindow.cs        (AUTH-1, 2026-07-31)
  CardAuthoringNav.cs              (AUTH-1, 2026-07-31)
  InstrumentEffectEditor.cs        (AUTH-1b, 2026-07-31; [CustomEditor] for InstrumentEffect)

Assets/Scripts/DevMode/Editor/
  CompositionInventoryWindow.cs    (CSV-1, 2026-07-18; #if UNITY_EDITOR && ALWTTT_DEV)

Assets/Scripts/Cards/Editor/LLM/
  CardLLMVocabularyBuilder.cs      (Stage 1 — the only game-type toucher; CE-L1)

Assets/Scripts/Cards/LLMAuthoring/  (editor-only asmdef pair; CE-L1, 2026-06-11)
  ALWTTT.Cards.LLMAuthoring.asmdef
  AssemblyInfo.cs
  CardImportDtos.cs                (shared DTO schema)
  CardImportDtoParser.cs           (shared parse: JSON box + LLM routes)
  CardLLMVocabulary.cs             (snapshot POCO + palette descriptors)
  CardPaletteDescriptorScanner.cs
  CardPaletteIntentResolver.cs
  CardLLMPromptBuilder.cs
  CardLLMGenerator.cs
  CardLLMResponseHandler.cs
  CardLLMFieldPlan.cs
  Tests/                           (asmdef + fixtures + FakeLLMClient; 77 tests)

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

### 14.8 Composition Inventory discovery under-reported — RESOLVED (2026-07-18, CSV-1b + CSV-1c)

Palette and then pattern discovery were both narrowed to the Resources scan roots,
producing incomplete listings and false `ORPHAN` flags. Fixed by unioning every family
with `AssetDatabase` plus a reference harvest; chord progressions went 13 → 48 and the
orphan column is now trustworthy. The residual runtime-side finding (the repositories
still cannot resolve most in-use content) is surfaced as the `OFF-ROOT` flag and tracked
as D-CSV-13 / D-CSV-14, not as a tooling gap. Full account in §17.7.

### 14.9 No instrument authoring tool (logged 2026-07-18)

Neither project has an editor window for `MIDIInstrumentSO` / `MIDIPercussionInstrumentSO`; `SSoT_Authoring_Tools.md §3` lists chord/drum/melody editors only. Instruments are authored through the standard Inspector. `CompositionInventoryWindow` §17 gives the first structured **view** of the family (soundfont/bank/patch/octave/volume + health flags), which is what CSV-4 curation needs, but it is not an authoring surface. If instrument authoring is ever tooled, the owning side must be decided first — the assets are package-owned.

---

(AUTH-1, 2026-07-31: the game-owned PartEffect family is now tooled — §18 — and
`InstrumentEffect` has a mode-conditional inspector — §18.9. This gap concerns
package-owned `MIDIInstrumentSO` / `MIDIPercussionInstrumentSO` assets and
stands unchanged.)

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
| LLM card-generation pipeline mechanism (CE-L1, §4.10) | **sin hogar documental** — `reference/Report_CardLLM_Pipeline.md` retirado 2026-08-08 (F16); solo código |
| LLM-generation pattern authority (cross-project) | MidiGenPlay `authoring/SSoT_Authoring_LLM_Generation.md` §7 |
| PartEffect runtime semantics (scope, timing, application order) | `SSoT_Runtime_CompositionSession_Integration.md` |
| `InstrumentEffect.RandomFromList` semantics (pick-once-then-persist, D-R2-7) | `SSoT_Runtime_CompositionSession_Integration.md` §11 |
---

## 16. Configurable runtime SO surfaces (Inspector-only)

This section documents data ScriptableObjects that drive runtime behavior but are authored exclusively through Unity's standard Inspector (no dedicated editor window). They are listed here so the field surface and intent are governed alongside the editor tooling, even though no tool owns them.

### 16.1 `GigSetupConfigData` — picker configuration (M4.6-prep merged (1)/(4))

The asset wired into `GigSetupController.setupConfig` that drives the Gig Setup scene's selectable content.

| Field | Type | Purpose |
|---|---|---|
| `availableBandDecks` | `List<BandDeckData>` | Dev/QA fallback decks for the legacy path (`useMusicianStartersToggle = OFF`). |
| `availableEncounters` | `List<GigEncounterSO>` | Encounter dropdown contents. |
| `genericStarterCatalog` | `GenericCardCatalogSO` | Optional generic ("Owner: Any") starter cards added on top of per-musician catalogs in the auto-assembly path (M4.6-prep batch (2)). Null is valid. |
| `availableAudienceCharacters` | `List<AudienceCharacterData>` | Selectable audience pool for the audience picker. Runtime unions with the selected encounter's `AudienceMemberList`. |
| `maxAudienceCount` | `int` (min 1) | Mirror of GigScene's `AudienceMemberPosList.Count`. Audience picker validates against this on Start; selecting more blocks gig start with a clear error. |
| `defaultInitialGigInspiration` | `int` | Default starting Inspiration for new runs. |
| `defaultInspirationPerLoop` | `int` | Default Inspiration generated per loop. |
| `defaultDiscardHandBetweenTurns` | `bool` | Default per-turn discard policy. |
| `defaultKeepInspirationBetweenTurns` | `bool` | Default Inspiration carry-over policy. |
| `allowOverrideRequiredSongCount` | `bool` | Whether the setup scene exposes an override for required song count. |
| `defaultRequiredSongCount` | `int` (min 1) | Default required song count when not overridden. |

**Notes:**
- `availableBandDecks` continues to serve as the dev/QA fallback when `useMusicianStartersToggle = OFF`.
- `genericStarterCatalog` is unchanged from batch (2). Its provenance rule (generics are not recorded in `musicianGrantedActionCards` / `musicianGrantedCompositionCards`) lives in `GenericCardCatalogSO` doc-comments.
- `maxAudienceCount` is a manual mirror; if the GigScene's position list grows or shrinks, this field must be updated to match. Editor-time validator deferred to a future tooling batch.
- The picker boundary semantics (band/audience override decision rules, multiset-blind comparator, encounter-swap reset) are governed by `SSoT_Gig_Encounter.md §7`. This section only catalogues the field surface.

**Boundary note:** This is a runtime data SO, not an editor tool. It has no dedicated editor window; all fields are authored via Unity's standard Inspector. If a future authoring batch promotes a custom Inspector or wizard for this SO, that tooling gets a tool-section above (§4–§12 style), and this entry becomes a cross-reference to it.

---

## 17. Composition Inventory Window (`CompositionInventoryWindow`) — CSV-1, 2026-07-18

**File:** `Assets/Scripts/DevMode/Editor/CompositionInventoryWindow.cs`
**Namespace:** `ALWTTT.DevMode.Editor`
**Menu:** ALWTTT → Dev → Composition Inventory (priority 30)
**Gate:** `#if UNITY_EDITOR && ALWTTT_DEV` (D-CSV-10=A — literal compliance with the batch constraint; relaxing to plain `UNITY_EDITOR` is a one-line change and would still ship nothing)

### 17.1 Documentary home (D-CSV-6=A, locked 2026-07-18)

This window browses **package-owned** assets (MidiGenPlay patterns, palettes, instruments) alongside ALWTTT-owned style bundles, which raised the question of where it is documented. `SSoT_Authoring_Tools.md §4` (MidiGenPlay) assigns package documentation to tools that **author or edit** package assets. This tool authors nothing and edits nothing — it is a game-side curation browser — so it is documented here, mirroring `CardInventoryWindow` (§8). The boundary rule is unaffected: nothing in this section defines package asset semantics; the window only reports fields the package already owns.

### 17.2 What it does

Read-only inventory and **curation worklist** over every composition asset family. It exists because CSV-3..CSV-6 cannot judge musical content that cannot first be listed, and because no naming convention exists yet (CR-9). Each view supports `Print` (multi-line `Debug.Log`) and `Export JSON` (`SaveFilePanel` → `JsonUtility.ToJson(_, prettyPrint: true)` → `Debug.Log` of the path → `RevealInFinder`), following the `CardInventoryWindow` pattern verbatim. **`Export All` (CSV-1c)** takes one `SaveFolderPanel` and writes all seven views in a single pass using the same filenames and schemas; both export paths share `BuildJsonForView(View)`, and a view that throws is recorded and skipped rather than aborting the batch. It exists because a full re-baseline (the CSV-1b/1c workflow) is a seven-dialog operation otherwise.

The window mutates nothing: no rename, no move, no `SetDirty`, no save (ST-CSV-7 PASS).

**Read-only invariant reaffirmed (D-CSV-19=A, 2026-07-20).** Rename and lifecycle operations belong to a **separate editor window** (batch **CSV-4b**), not to a mode inside this one. The reason is not purity: this window is the independent verification surface used *before and after* a bulk rename run, and a tool that both renames and reports cannot be trusted to report on its own renames. **Do not add mutation here.**

### 17.3 Views

Seven toolbar views:

- **Style Bundles** — every `TrackStyleBundleSO` (all subclasses), with `appliesTo` role and a summary of its direct pattern/palette references.
- **Drum Patterns** — `DrumPatternData` + `DrumPatternPaletteSO`.
- **Chord Progressions** — `ChordProgressionData` + `ChordProgressionPaletteSO` + `ChordProgressionLibrarySO` (the `MidiGenPlayConfig.progressionLibrary` reference is annotated `[config-wired]`).
- **Melody / Phrases** — `MelodyPatternData` + `PhraseArchetypeSO` + `PhrasePaletteSO`.
- **Melodic Instruments** / **Percussion Instruments** — split defensively, since `MIDIPercussionInstrumentSO` derives `MIDIInstrumentSO`.
- **Names Report** — every asset in every family with asset name, display name (when it differs), source tag and path. This is the input artifact for the CSV-4 naming convention; the window performs no renames.

### 17.4 Discovery (CSV-1b + CSV-1c)

Every family is the **union** of the runtime read path and `AssetDatabase`, plus a
reference harvest. The runtime repositories are still consulted first, and their
membership is recorded — that recording is what produces the `OFF-ROOT` flag.

- Patterns: `PatternRepositoryResources` ∪ `AssetDatabase.FindAssets("t:{Type}")` ∪ harvest.
- Palettes: `TrackPatternConfigStoreResources<T>("Drums"/"Chords"/"Phrases")` ∪ `AssetDatabase`.
- Instruments: `InstrumentRepositoryResources` ∪ `AssetDatabase`.
- Style bundles, chord libraries, phrase archetypes: `AssetDatabase` only — no repository exists for these families.
- **Harvest:** after discovery, every palette, library and style bundle is walked and any referenced pattern or archetype still missing is added. Anything referenced by content in use belongs in the inventory regardless of where it lives.

**Why the union is necessary.** The repositories scan only their configured Resources
roots (`{resourcesPatternsRoot}/Chords|Drums|Melodies`, `resourcesInstrumentsPath`).
Several in-use families live outside them, so repository-only discovery silently omitted
real content — measured at CSV-1c: chord progressions went 13 → 48, and the 13 the
repository could see were *all* dead assets. The harvest has so far always been a no-op
(`HARVESTED = 0`); it is retained as a safety net for assets no scan root covers.

### 17.5 Filters

Time signature, source (`All` / `Package` / `Local`, derived from an asset path prefix of `Packages/`), free-text over asset + display name, orphan-only, duplicate-only, flagged-only, bundle-reachable-only, and an editable **reference part measures** field (default 8, matching `SongCompositionUI.PartEntry.measures`) that feeds the length-comparison flags. "Bundle-reachable" is the §18.6 `(off-band)` notion generalised project-wide: reachable from any style bundle's direct reference or via a palette that a bundle references.

### 17.6 Derived health flags

The point of the tool — what turns a listing into a worklist.

| Flag | Family | Meaning |
| --- | --- | --- |
| `EMPTY` | progressions, melody | no events / no notes |
| `SHORT-TAIL` / `OVERFLOW` | progressions | authored event span shorter / longer than `Measures × beatsPerMeasure × subdivisions` |
| `BASS-GAP` | progressions | `Measures` < reference part measures. **Static face of CR-7's "bass ends early"**: the bass renders the progression once with no repeat-to-fill (`SSoT_Composer_Bass_Track §1`), so a shorter progression leaves the bass silent for the remainder of the part. The backing track tiles and does not show this. |
| `LONGER-THAN-PART` | progressions | `Measures` > reference part measures |
| `BPMEAS-MISMATCH` | drums, melody | the asset's `beatsPerMeasure` field disagrees with its declared `TimeSignature` |
| `NO-LANES` / `ALL-SILENT` | drums | no lanes, or no active step in any lane |
| `OVERFLOW` | melody | last note ends past `TotalBeats` |
| `ORPHAN` | all patterns/palettes/archetypes | not referenced by any discovered palette, library, or style bundle. Reliable since CSV-1c. **Direct only** — an asset referenced solely by an *orphan* palette or library is dead by transitivity and is not flagged; that inference has to be made by reading the `refs` column. |
| `DUP#n` | all | content-duplicate group id |
| `OFF-ROOT` | patterns, instruments | exists in the project, but **no runtime repository can resolve it** — outside every configured Resources scan root. It may still play via a direct palette/bundle reference, but it cannot appear in the dev pattern (§18.4) or instrument (§18.9) pickers, which are repository-fed. Measured 2026-07-18 against the (now superseded) 230-asset export: **30/30 live chord progressions are `OFF-ROOT`**, 1/41 drums, 2/14 melody, 0 instruments. Those family counts belong to the pre-cleanup inventory; the current baseline is the 183-asset export of 2026-07-20 (`CSV_Composition_Validation_Sub_Roadmap.md` §4.1.1). |
| `HARVESTED` | patterns, archetypes | no scan found it at all; listed only because something references it. The strongest possible signal that a scan root is wrong. |
| `NO-SOUNDFONT` / `OCTAVE-RANGE-INVERTED` / `VOLUME-ZERO` | instruments | authoring defects |

**Origin classification — a third `source` value is owed (2026-07-20, implementation is CSV-4b).** `source` currently resolves to `local` | `pkg` by path prefix. Package assets under `Packages/**/Samples/` are outside `Resources/` and therefore invisible to every runtime repository, yet remain visible to this window's `AssetDatabase` discovery — so they are listed permanently and are re-investigated on every inventory pass. They must be classified as a third origin, **`sample`**. Six assets are in this state as of MidiGenPlay 1.1.0 (`Samples/ExampleCatalogue/ChordProgressions/`). This is expected behaviour of the discovery union, not a regression: the union is deliberately broader than `Resources`, which is exactly why the 1.1.0 move did not reduce the consumer count.

**Duplicate signatures deliberately exclude naming and metadata** (asset name, `DisplayName`, `originalInput`, `songReferences`) so a rename cannot hide a duplicate and a duplicate cannot hide behind a different name. Progressions compare TS + measures + subdivisions + ordered events; drums compare grid + per-lane step/velocity strings; melody compares grid + ordered notes; instruments compare the `soundfont|bank|patch|patchIndex` quadruple (percussion additionally compares the mapping list).

### 17.7 Discovery defect — RESOLVED (CSV-1b + CSV-1c, 2026-07-18)

Recorded because the failure mode is subtle and will recur if discovery is ever
narrowed back to a single source.

The original window discovered palettes through `TrackPatternConfigStoreResources` and
patterns through `PatternRepositoryResources`, both of which scan only
`Resources/ScriptableObjects/Patterns/<type>`. Most project palettes and most in-use
chord progressions live outside that root. Two consecutive failures resulted:

- **CSV-1b (palettes).** First export found 1 drum / 1 chord / 1 phrase palette while
  style bundles referenced 5 / 4 / 2 by name. The reference index was therefore
  incomplete and `ORPHAN` was wrong: 38/40 drum patterns and 13/13 progressions
  falsely flagged. Fixed by unioning with `AssetDatabase`; drum orphans fell to 13/40
  and archetype orphans to 0.
- **CSV-1c (patterns).** Chords still showed 13/13 orphan afterwards, because the
  progressions those palettes reference were not in the pattern list at all — they
  live under `ScriptableObjects/Chord Progressions/{Major,Minor,Modal,Tests}`, a
  sibling of `Patterns/`. Fixed the same way: 13 → 48 progressions, orphans 13/13 →
  14/48, and the 14 remaining are genuinely dead.

**The residue is a runtime finding, not a tooling one.** The repositories still cannot
see that content, which is why `OFF-ROOT` exists (§17.6). It does not break playback —
palettes and bundles hold direct references — but it does mean the dev pattern-override
dropdown (`SSoT_Dev_Mode §18.6`) is fed a list that excludes every in-use progression.
Tracked as **D-CSV-13** (dropdown source) and **D-CSV-14** (whether the scan roots are
corrected); neither belongs to this window.

**D-CSV-14 scope reduced (2026-07-20, CSV-4).** The package-side chord-progression
Resources root no longer exists — it moved to `Samples/` in MidiGenPlay 1.1.0 — so
`Patterns/{Chords,Drums,Melodies}` are now the only package-side scan roots and the
remaining mismatch is **exclusively Assets-side**: local chords under
`ScriptableObjects/Chord Progressions/*` and two local melody patterns under
`Patterns/Melody` (singular) rather than `Patterns/Melodies` (plural). D-CSV-14 is
**no longer cross-boundary**; it stays with CSV-5.

### 17.8 Export schemas

- Bundles: `{ "bundles": [{ "type", "assetName", "appliesTo", "overrideRef", "paletteRef", "source", "assetPath" }] }`
- Pattern views: `{ "patterns": [{ "assetName", "displayName", "timeSignature", "measures", "subdivisions", "contentCount", "source", "refs", "flags", "assetPath" }], "palettes": [{ "type", "assetName", "displayName", "entryCount", "refs", "orphan", "source", "assetPath" }] }`
- Instrument views: `{ "instruments": [{ "assetName", "instrumentName", "instrumentType", "soundFont", "bank", "patch", "patchIndex", "octaveMin", "octaveMax", "volume01", "percussionMappings", "flags", "source", "assetPath" }] }`
- Names Report: `{ "names": [{ "family", "assetName", "displayName", "source", "assetPath" }] }`

As with `CardInventoryWindow` (§8.4), these schemas are **informational and human-readable; they are not designed to be re-imported** through any authoring path.

### 17.9 Boundary note

Viewer only. Pattern, progression and melody **authoring** remains package-side (`SSoT_Authoring_Tools.md §3`); style-bundle authoring remains card-side (`SSoT_Card_Authoring_Contracts.md`). No instrument editor exists in either project — the instrument views are the only structured view of that family that currently exists, and that remains a viewing surface, not a promotion of ownership.

### 17.10 Known gap — no card → bundle reverse index (logged 2026-07-18)

The window indexes references *downward* (bundle → palette → pattern) and can therefore
say whether a pattern is used by a bundle. It does **not** index `CardDefinition →
TrackStyleBundleSO`, so it cannot say whether a bundle itself is reachable from any
card. Consequence: `TrackStyleBundleSO` rows carry no orphan status, and bundle cleanup
cannot be decided from this export — several visibly test-flavoured bundles
(`Rhythm - Card Config SO`, `Melody Card Config - Test`, `TEST Bassline Card Config SO`,
`2CBacking001TestProg_…`, the two `Backing Card Config [roman]` assets) may or may not
be live content.

`CardInventoryWindow` (§8) does not supply this either — it lists cards but not their
bundle references. Closing the gap means either a bundle-usage column here (walk every
`CardDefinition`'s composition payload) or a card-side column there.

**Escalated to blocking (D-CSV-16=A, 2026-07-20).** CSV-4 did need it. After the local
test bundles were deleted, the liveness of the Modal and Test chord palettes could only
be established **from the user's statement, not from tooling** — the reachable-set figure
that CSV-4 records (14 of 33 progressions) therefore rests on recollection. The index is
owed; the decision is locked as A, but **no batch owns it yet**. Arc home:
`planning/active/CSV_Composition_Validation_Sub_Roadmap.md` §3.

### 17.11 Instrument curation is pool-level, not asset-level (D-CSV-18=A, 2026-07-20)

All 79 instruments (70 melodic + 9 percussion) report `source: pkg`. Under **D-CSV-7=A** (asset ownership is location-based: `Assets/` is ALWTTT's, `Packages/` is MidiGenPlay's), ALWTTT can neither rename, delete, nor retune them — `volume01` included.

Instrument curation is therefore a **pool-level** activity. A listening verdict resolves to an edit of `InstrumentRules` and the per-musician whitelists, which are **ALWTTT-owned**; it never resolves to an edit of the instrument asset. Asset-level defects become package asks instead — the open one is **D-BAG-3 / MGP-MIX-1** (per-instrument mix balance; `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §4.3).

This is the boundary rule applied correctly, not a workaround: **the package owns the instrument, ALWTTT owns who plays it.**

Consequence for the window: the instrument views remain a *reporting* surface. They tell you which instrument to remove from a pool; they are not, and must not become, an instrument editor (§17.9).

---

### 17.12 Barrido de huérfanos post-R3 (LOG-1, 2026-08-08)

**Alcance del barrido (D-LOG-2=B): solo los huérfanos que R3 creó.** No es una pasada de
inventario general. Retirados: **`Chord Palette - Test` y sus 4 progresiones muertas**, que
quedaron sin referencia al cerrar R3. El destino del trabajo de curación general sigue siendo
**CSV-6**; este barrido no lo adelanta ni lo sustituye.

**Recordatorio operativo — `ORPHAN` es DIRECTO, y la transitividad sigue sin marcarse.** El
flag `ORPHAN` (§17.6) marca un asset no referenciado por ninguna paleta, librería o style
bundle *descubierto*. Un asset referenciado **únicamente por una paleta huérfana** está muerto
por transitividad y **no se marca**: esa inferencia hay que hacerla leyendo la columna `refs`.
Es precisamente el caso que se acaba de barrer a mano —una paleta huérfana arrastrando cuatro
progresiones que el flag no señalaba— y seguirá siendo manual hasta que CSV-6 aporte cierre
transitivo. **Un conteo de `ORPHAN` no es un conteo de assets muertos; es un suelo.**

**Conteo de inventario — 232 assets (export del 2026-08-08).** Línea base vigente, medida con
`Export All` sobre las siete vistas. El `Names Report` devuelve 232 filas y cuadra exactamente
con la suma por familia, así que las vistas no se solapan ni pierden nada. Sustituye a la línea
base de **183 del 2026-07-20**, que pasa a **histórica y no citable como actual**.

| Familia | 183 (2026-07-20) | **232 (2026-08-08)** | Δ |
| --- | --- | --- | --- |
| Progresiones de acordes | 33 | **48** | +15 |
| Paletas / librerías de acordes | 4 | **6** | +2 (5 `ChordProgressionPaletteSO` + 1 `ChordProgressionLibrarySO`) |
| Patrones de percusión | 27 | **36** | +9 |
| Paletas de percusión | 5 | **5** | = |
| Patrones de melodía | 3 | **5** | +2 |
| Paletas de frases | 3 | **4** | +1 |
| Arquetipos de frase | 9 | **14** | +5 |
| Style bundles | 19 | **35** | +16 (14 Bassline · 8 Melody · 7 Rhythm · 6 Backing) |
| Instrumentos melódicos | 70 | **70** | = |
| Instrumentos de percusión | 9 | **9** | = |
| **Total** | **183** | **232** | **+49** |

**Los +49 no deben leerse como «contenido nuevo bueno».** Cubren R0–R3 completos (bundles de
Conito y de las tres Wormus, patrón de melodía, paleta de frases), la consolidación de
progresiones de CONT-B y los 8 patrones de percusión de 8 compases. Atribuir cada delta asset
por asset es trabajo de curación, no de conteo: pertenece a CSV-4b/CSV-6, no a esta sección.

**Salud medida en el mismo export** (recordatorio de arriba: `ORPHAN` es un **suelo**, no un
censo de assets muertos):

- **15 patrones `ORPHAN`** (11 de ellos progresiones) y **4 paletas o frases huérfanas**:
  `Chord Palette - Bass Defaults`, `Chord Palette - Test`, `_ChordProgressionLibrary`,
  `PhrasePalette_SingingField`.
- **`BASS-GAP` en 14 progresiones** (8× 4m, 4× 2m, 2× 1m frente a una parte de 8 compases) —
  la señal que motiva el estándar de 8 compases de **CR-10 / D-CSV-23**.
- **3 `LONGER-THAN-PART`** (2× 16m, 1× 24m) y **1 `SHORT-TAIL`** (span 255/256 pasos).
- **3 `DUP#1`**, dos de ellos los `ProgSmoke_*` locales.
- **7 `OFF-ROOT`, y su reparto cierra la verificación pendiente de D-CSV-14:** cinco son
  package-side bajo `Packages/…/Samples/ExampleCatalogue/ChordProgressions/` (efecto del
  movimiento a `Samples/` de MidiGenPlay 1.1.0 — no es de ALWTTT arreglarlo), y los **dos
  locales son exactamente** `Melody_4-4_2m_8n` y `Melody_6-4_2m_11n`, bajo `Patterns/Melody`
  (singular). **Cero `OFF-ROOT` locales en progresiones y percusión**, tal como anticipaba la
  resolución de CONT-B. El residuo es de dos ficheros; su alineación a `Patterns/Melodies` es
  trabajo de CSV-4b/CSV-5.

## 18. Part Effect Editor (`PartEffectEditorWindow`) — AUTH-1, 2026-07-31

**File:** `Assets/Scripts/Cards/Editor/PartEffectEditorWindow.cs`
**Namespace:** `ALWTTT.Cards.Editor`
**Menu:** ALWTTT → Cards → Effect Editor (priority 13, after Card Inventory)

### 18.1 What it does

First authoring surface for the PartEffect asset family, which was
Inspector-only until AUTH-1 — the friction surfaced during R2/Conito authoring.
Lists every `PartEffect`-derived asset in the project; supports type filter +
name search, inline editing, Create, Duplicate, Delete with a reference-usage
warning, an on-demand Find Usages scan, and Export JSON.

### 18.2 Scope (D-AUTH1-2=A)

PartEffect **SO assets only**. `CardEffectSpec` entries are `[SerializeReference]`
payload data with no asset identity; their editing remains in the Card Editor's
payload panel (§4). "Spec templates/presets" are explicitly out of scope for v1.

### 18.3 Type discovery

Concrete types come from `TypeCache.GetTypesDerivedFrom<PartEffect>()`
(abstract/generic excluded) — never a hardcoded list. Family at close:
`InstrumentEffect`, `MeterEffect`, `ModulationEffect`, `TempoEffect`,
`TonalityEffect`, plus `DensityEffect` and `FeelEffect` (declared in
`PartEffect.cs`; no assets authored). New subtypes appear in the filter and
Create menus automatically.

### 18.4 Create / Duplicate / Delete

- **Create:** type dropdown + asset name + destination folder, defaulting to
  `Assets/Resources/Data/Cards/Composition/_PartEffects/` (the canonical family
  folder). Folder auto-created if missing; the new asset is selected and pinged.
- **Duplicate:** `AssetDatabase.CopyAsset` with `GenerateUniqueAssetPath`.
- **Delete:** runs the usage scan first; the confirm dialog reports how many
  `CompositionCardPayload.modifierEffects` lists reference the asset and warns
  that deletion leaves null entries in them.

### 18.5 Inline editing

The right panel hosts `UnityEditor.Editor.CreateEditor(asset)` — the asset's
default **or custom** inspector — so field coverage tracks the type definitions
with no per-type window code, and any `[CustomEditor]` written for a PartEffect
type (§18.9) is honoured identically here and in the standard Inspector.
`GetLabel()` is shown per row and in the header, try-wrapped so a malformed
asset cannot break the list.

### 18.6 Find Usages

On demand (button, never per-frame): scans every `CompositionCardPayload` for
reference-equality hits in `ModifierEffects`; results listed with Ping.

### 18.7 Boundary note

PartEffects are game-owned (`ALWTTT.Cards`). The window reads and writes only
ALWTTT assets. It does not author MidiGenPlay instruments — §14.9 stands.

### 18.8 Export JSON (AUTH-1b)

`EditorUtility.SaveFilePanel` → pretty-printed `JsonUtility.ToJson`. Exports the
**currently filtered** list (type filter + search), matching the Card Inventory
"current view" convention (§8.4); like that export it is informational and not
designed for re-import. Schema:

{ "partEffects": [ { "assetName", "type", "assetPath", "scope", "timing",
                     "label",
                     "fields": [ { "name", "value" }, ... ],
                     "usedBy": [ "<payload asset name>", ... ] } ] }

`fields` is the same depth-1 serialized dump the Inventory detailed print uses
(script field skipped, `scope`/`timing` lifted to top level, arrays print size
only). `usedBy` comes from one reverse-index pass over every
`CompositionCardPayload.modifierEffects`, so an export doubles as a
**card → PartEffect reverse index**. (Distinct from the card → style-bundle
reverse index still missing at §17.10; that gap is unchanged.)

### 18.9 Layout and per-type inspectors (AUTH-1b)

Two-panel layout with a draggable splitter (same pattern as `CardEditorWindow`);
list rows use expanding name/label columns with tooltips carrying the full asset
name and path, so long names are readable at any window width. The list header
shows `N / total` while a filter is active.

`InstrumentEffectEditor` (`[CustomEditor(typeof(InstrumentEffect))]`,
`Assets/Scripts/Cards/Editor/InstrumentEffectEditor.cs`) draws only the fields
the selected `mode` consumes — the `RandomFromList` pool no longer renders under
`SpecificMelodic` / `SpecificPercussion` / `InstrumentType` — plus validation
hints: unassigned instrument for the two Specific modes, empty pool (matching
the runtime warn-and-no-op, R2c / D-R2-7), and single-entry pool (equivalent to
`SpecificMelodic`, no variety). It falls back to the default inspector if any
field is renamed, rather than silently hiding data.

**D-AUTH1-4=A:** implemented as a custom inspector rather than window-local
field filtering, so the standard Inspector and the Effect Editor cannot diverge.
Presentation-only — hidden fields keep their serialized values and no runtime
behaviour changes. This is the only per-type editor in the family; every other
PartEffect type uses the default inspector.

---

## 19. Cross-window navigation (`CardAuthoringNav`) — AUTH-1, 2026-07-31

**File:** `Assets/Scripts/Cards/Editor/CardAuthoringNav.cs` (internal static utility)

A one-row toolbar strip drawn by each of the four card-tooling windows (Card
Editor, Effect Editor, Card Inventory, Deck Editor); the current tool's button
renders pressed and inert. **D-AUTH1-3=A:** a static utility extending the M1.1b
`OpenAndSelect` pattern, deliberately not a window — no lifecycle, and each
window stays self-contained.

Context-carrying cross-links delivered with it:
- Card Inventory rows → `CardEditorWindow.OpenAndSelect(card)` ("Edit" buttons,
  both the All-CardDefinitions rows and the shared entry-list rows).
- Card Editor composition payload → per-entry
  `PartEffectEditorWindow.OpenAndSelect(fx)` + a "New Effect…" shortcut (§4.11).
- `PartEffectEditorWindow.OpenAndSelect(PartEffect)` selects and pings the asset.

Navigation only: no cross-link mutates an asset.

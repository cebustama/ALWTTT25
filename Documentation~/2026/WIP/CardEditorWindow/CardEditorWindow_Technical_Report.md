# 🎴 ALWTTT — CardEditorWindow Technical State Report (CSO Era)

**Last audited against code:** 2026-01-19

This report describes the current state of the **ALWTTT Card Authoring Tooling** in the CSO era:

- Card gameplay effects are authored as **CSO StatusActions** on `CardPayload`
- The authoring surface is the **CardEditorWindow** (catalog-first)
- JSON import exists as an editor-only **stage → review/edit → save assets → add to catalog** pipeline

It separates:
- **What is supported (today)**
- **What remains missing / risky**
- **How the system is wired** (catalog + registries)
- **Next best incremental steps** (SOLID-aligned, no overengineering)

---

## 1) Current authoring model snapshot

### 1.1 CardPayload is CSO-first (declarative)

All CSO gameplay effects authored on cards live in:

- `CardPayload.StatusActions : List<StatusEffectActionData>`

Each `StatusEffectActionData` row represents one atomic status application:

- `CharacterStatusId EffectId`
- `ActionTargetType TargetType`
- `int StacksDelta`
- `float Delay`

**Key property:** card payloads store **status IDs** (and targeting), not `StatusEffectSO` references.

---

## 2) Project-wide wiring root

### 2.1 `ALWTTTProjectRegistriesSO`

A single ScriptableObject that centralizes project-wide asset wiring.

Relevant to Card Editor UX / validation:
- `StatusEffectCatalogueSO` (status assets lookup; used for validation)
- `CharacterStatusPrimitiveDatabaseSO` (CSO primitives / ontology reference; present in registries, not yet used directly by the Card Editor drawer)

The Card Editor exposes a registries slot and warns when references are missing.

---

## 3) CardEditorWindow — current capabilities

### ✅ A) Catalog-first workflow

- Select musician (`MusicianCharacterType`)
- Load `MusicianCharacterData`
- Load or create `MusicianCardCatalogData`
- Designers operate through **catalog entries** (not loose assets)

### ✅ B) Browse and filter existing cards

Left panel lists `MusicianCardCatalogData.Entries` with filters:
- Domain: Action / Composition
- Acquisition flags: Starter / Reward / Locked
- **StatusId filter**: shows cards whose payload includes a `StatusActions` row matching the selected `CharacterStatusId`

### ✅ C) Create new cards + payload assets

Create wizard generates:
- `CardDefinition`
- correct payload asset (`ActionCardPayload` or `CompositionCardPayload`)
- inserts a catalog entry with the chosen acquisition defaults

### ✅ D) Edit CardDefinition + payloads in-window

Right panel edits:
- CardDefinition common fields (identity, visuals, costs, performer rules, etc.)
- Action payload fields: `actionTiming`, `conditions`, and **StatusActions (CSO)**
- Composition payload fields: composition metadata + modifier effects, and **StatusActions (CSO)**

### ✅ E) StatusActions authoring + validation

A shared drawer renders `statusActions` and validates when catalogue wiring exists:
1) invalid enum backing value (`Enum.IsDefined`) → warning
2) valid enum but missing from `StatusEffectCatalogueSO` → warning

**Minor UX note:** the drawer currently prints a header label and then renders a `PropertyField` that prints the list label again, so the header appears duplicated. (Easy fix: use `GUIContent.none` for the PropertyField label, or remove the manual label.)

---

## 4) JSON import pipeline — audited state

### ✅ What JSON import currently does

The partial `CardEditorWindow.JsonImport` implements:
- Paste JSON → parse DTO → create staged in-memory `CardDefinition` + `CardPayload`
- Apply CardDefinition fields via `SerializedObject` (private-SerializeField-safe)
- Show staged objects in the same editors used for normal assets
- Save creates real `.asset` files and adds the card to the loaded catalog
- Supports staged catalog entry defaults (`flags`, `starterCopies`, `unlockId`) and gates Save when locked-without-unlockId

### ✅ What it currently supports in payloads

- Action payload:
  - applies `actionTiming`
  - applies `conditions`
- Composition payload:
  - applies composition fields (`primaryKind`, `trackAction`, `partAction`, `modifierEffects` by path/guid)

### ⚠️ What it does *not* yet support (CSO gap)

- **It does not import `statusActions`** (even though the UI can edit them).
  - Result: JSON-imported cards require manual authoring of `StatusActions` after import.

### ⚠️ Legacy mismatch still present in the DTO

- The DTO defines `action.actions[]`, and `ApplyActionJson(...)` attempts to write to a serialized property named `"actions"`.
- The current `ActionCardPayload` does **not** have an `actions` list.

Impact:
- `action.actions[]` is effectively ignored.

---

## 5) Support matrix

### Supported (today)

- Musician selection + catalog lifecycle
- Browse/filter by domain + acquisition
- Filter by StatusId presence in payload
- Add existing cards to catalog
- Create new card + payload
- Edit CardDefinition fields in-window
- Edit Action & Composition payloads
- Author CSO StatusActions in UI
- Validate StatusActions against `StatusEffectCatalogueSO` when registries are wired
- JSON import pipeline for staging + asset creation + catalog insertion (**metadata + many payload fields**) 

### Not supported / needs work

- JSON export
- CSO-compatible JSON import of `statusActions`
- ActionJson `actions[]` (until the payload supports it again, or it is removed from the schema)
- StatusActions UI enhancements (DisplayName picker, compact rows, presets)

---

## 6) Next best incremental steps (recommended)

1) **Decide the Action payload direction** (small, explicit decision)
   - If you keep the old action execution system: re-add an `actions` list to `ActionCardPayload` and wire runtime.
   - If CSO is the only gameplay effect system: remove `action.actions[]` from the JSON DTO and delete its writer.

2) **Implement CSO-aligned JSON import** (minimal, no overengineering)
   - Extend the root DTO with `statusActions[]` containing `{ effectId (int), targetType (string), stacksDelta (int), delay (float) }`
   - In `TryStageCardFromJson`, write into `statusActions` via `SerializedObject`.
   - Minimal backwards compatibility: if `statusActions` exists use it; otherwise skip.

3) **Add JSON export** (optional but valuable)
   - Export the same schema you import, focusing first on:
     - CardDefinition common fields
     - payload + `statusActions`
     - entry defaults

---

End of report.

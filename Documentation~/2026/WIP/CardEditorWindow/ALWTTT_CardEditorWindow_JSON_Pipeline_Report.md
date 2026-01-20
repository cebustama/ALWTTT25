# ALWTTT — CardEditorWindow JSON Pipeline (Systemic Technical Report)

**Scope:** Editor-only JSON import pipeline as implemented in `CardEditorWindow` + `CardEditorWindow.JsonImport`.

**Intent:** Describe components, responsibilities, and interactions (**system view**) and document **the current supported JSON surface area** + **known gaps** toward a CSO-aligned schema.

**Last audited against code:** 2026-01-19

---

## 1) High-level overview

The JSON pipeline is an **in-editor “stage → review/edit → save” workflow** embedded inside `CardEditorWindow`:

1. **Designer pastes JSON** in the *Create from JSON* block.
2. The window **parses JSON into a DTO**, creates **in-memory staged ScriptableObjects** (`CardDefinition` + `CardPayload`), and **applies JSON fields via `SerializedObject`** (safe even with private `[SerializeField]` fields).
3. The designer can **review/edit the staged objects using the same editor UI** used for normal cards.
4. Clicking **Save (Create Assets)**:
   - creates real `.asset` files on disk (definition + payload),
   - ensures the card references the saved payload asset,
   - adds the card to the currently loaded `MusicianCardCatalogData` via `MusicianCatalogService`.

This design is strong: it’s incremental, testable, and avoids half-created assets until “Save”.

---

## 2) Components and responsibilities

### A) `CardEditorWindow` (UI composition root for the authoring session)

**Responsibilities**
- Owns editor session state:
  - selected musician (`_selectedMusician`)
  - loaded musician data (`_loadedMusicianData`)
  - loaded catalog (`_loadedCatalog`)
  - selection index (`_selectedEntryIndex`)
- Renders:
  - toolbar (load musician/catalog, registries selector)
  - entry list / selection
  - create wizard (non-JSON)
  - JSON import block (delegated to partial)
  - right-side detail panel (CardDefinition + payload editors)

**Key interactions**
- Calls `DrawJsonImportBlock()` (from the partial) inside the entry list area.
- Reuses `DrawCardDefinitionCommonFields()` and `DrawPayloadEditors()` to edit both:
  - real assets (existing catalog cards)
  - staged in-memory objects (JSON workflow)

---

### B) `CardEditorWindow.JsonImport` (staging + persistence controller)

**Responsibilities**
- Owns JSON import state:
  - `_jsonImportText`, `_jsonImportError`
  - staged objects: `_stagedJsonCard`, `_stagedJsonPayload`
  - staged catalog-entry defaults: `_stagedJsonEntryFlags`, `_stagedJsonStarterCopies`, `_stagedJsonUnlockId`
- DTO definitions for parsing JSON via `JsonUtility.FromJson<T>()`
- Implements the JSON import workflow:
  - `TryStageCardFromJson(json)`
  - `SaveStagedJsonToAssetsAndAddToCatalog()`
  - `DiscardStagedJson()`

**Key design choices**
- **Staging objects are not assets**:
  - created via `CreateInstance<>`
  - marked with `HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy`
- **Applies data via `SerializedObject`** to safely set private serialized fields by name.
- **Uses `EditorUtility.CopySerialized`** when converting staged objects into real assets.

---

### C) `CardDefinition` (stable “card identity” asset)

**Responsibilities**
- Stores stable metadata: id, displayName, visuals, cost, rarity, keywords, performer rules, etc.
- Holds a reference to a `CardPayload` asset.

**Notable coupling**
- `RequiresTargetSelection` currently infers targeting by inspecting `payload.StatusActions` and checking `ActionTargetType`.

---

### D) `CardPayload` + derived payloads (domain data)

**Responsibilities**
- `CardPayload` is the base for all domains and owns CSO effect rows:
  - `statusActions : List<StatusEffectActionData>` (private serialized list)
  - `StatusActions : IReadOnlyList<StatusEffectActionData>` (public read-only API)
- `ActionCardPayload` adds:
  - `actionTiming`, `conditions`
- `CompositionCardPayload` adds:
  - composition descriptors and modifier effects

---

### E) `MusicianCatalogService` (editor-only catalog mutation helper)

**Responsibilities**
- Adds a `CardDefinition` to `MusicianCardCatalogData` safely with:
  - duplicate checks
  - Undo recording
  - dirtying + save assets
- Returns the resulting entry index for UI selection.

---

## 3) JSON pipeline sequence (what happens, in order)

### 3.1 Stage (parse + create in-memory objects)

**Entry point:** `TryStageCardFromJson(string json)`

1. Clears previous staged objects (`DiscardStagedJson()`).
2. Parses JSON DTO: `dto = JsonUtility.FromJson<CardJsonImport>(json)`.
3. Validates minimum fields: `kind`, `id`.
4. Creates staged objects in memory:
   - `_stagedJsonCard = CreateInstance<CardDefinition>()`
   - `_stagedJsonPayload = CreatePayloadInstance(dto.kind)` (Action vs Composition)
5. Applies CardDefinition fields:
   - id / displayName
   - performer rule + fixed musician
   - sprite by path (or fallback to musician default sprite)
   - costs, rarity, audio type, exhaust/target overrides
   - assigns payload reference (`payload` field)
6. Applies payload fields:
   - `ApplyActionJson()` OR `ApplyCompositionJson()` using `SerializedObject`
7. Parses optional `entry` defaults (flags, starterCopies, unlockId) and normalizes them.

**Result:** staged objects become editable using the same UI drawers as real assets.

---

### 3.2 Review/edit (UI)

**Entry point:** `DrawJsonImportBlock()`

If `_stagedJsonCard != null`, the block:
- Shows staged entry defaults UI:
  - Flags (`CardAcquisitionFlags`)
  - Starter Copies (only enabled if StarterDeck flag is set)
  - Unlock Id (only enabled if NOT unlocked-by-default)
- Calls:
  - `DrawCardDefinitionCommonFields(_stagedJsonCard)`
  - `DrawPayloadEditors(_stagedJsonCard)`
- Enables/disables **Save** depending on:
  - UnlockedByDefault → Save allowed
  - Locked → requires non-empty Unlock Id

---

### 3.3 Save (create assets + catalog entry)

**Entry point:** `SaveStagedJsonToAssetsAndAddToCatalog()`

1. Validates required context: staged objects + loaded catalog.
2. Validates staged entry rules:
   - if not `UnlockedByDefault`, `unlockId` must be present
   - if not starter, force starterCopies = 1
3. Chooses paths next to the catalog:
   - `{id}.asset` for CardDefinition
   - `{id}_Payload.asset` for payload
4. Creates real instances:
   - `cardAsset = CreateInstance<CardDefinition>()`
   - `payloadAsset = CreatePayloadInstance(kind)`
5. Copies staged data into real instances:
   - `EditorUtility.CopySerialized(_stagedJsonCard, cardAsset)`
   - `EditorUtility.CopySerialized(_stagedJsonPayload, payloadAsset)`
6. Writes assets to disk:
   - `AssetDatabase.CreateAsset(payloadAsset, payloadPath)`
   - `AssetDatabase.CreateAsset(cardAsset, cardPath)`
7. Fixes reference wiring:
   - forces `cardAsset.payload = payloadAsset` (ensures it’s not pointing to staged instance)
8. Adds to catalog via `MusicianCatalogService.TryAddEntry(...)`
   - on failure, deletes created assets (cleanup)
9. Selects/pings the new card and clears staged state.

---

## 4) Supported JSON schema (CURRENT)

The current DTO surface is:

### Root (`CardJsonImport`)

Required:
- `kind`: `"Action" | "Composition"`
- `id`: string

Optional (commonly used):
- `displayName`: string
- `performerRule`: string enum name (e.g. `"FixedMusicianType"`)
- `fixedMusician`: string enum name for `MusicianCharacterType` (defaults to selected musician)
- `cardType`: string enum name for `CardType` (e.g. `"CHR"`)
- `rarity`: string enum name for `RarityType` (e.g. `"Common"`)
- `audioType`: string enum name for `AudioActionType` (e.g. `"Button"`)
- `inspirationCost`: int (>= 0)
- `inspirationGenerated`: int (>= 0)
- `exhaustAfterPlay`: bool
- `overrideRequiresTargetSelection`: bool
- `requiresTargetSelectionOverrideValue`: bool
- `cardSpritePath`: string (AssetDatabase path to `Sprite`)
- `entry`: object (catalog defaults)

Payload blocks:
- `action`: object (used when kind == Action)
- `composition`: object (used when kind == Composition)

### Action payload (`ActionJson`)

- `actionTiming`: string enum name (`CardActionTiming`)
- `conditions`: `ConditionJson[]` (applied)
  - `type`: string (preferred; enum name)
  - `typeIndex`: int (fallback)
  - `value`: float
- `actions`: `CharacterActionJson[]` (**defined but NOT applied unless payload has an `actions` field**)
  - `actionType`: string (enum name)
  - `targetType`: string (enum name)
  - `value`: float
  - `delay`: float

### Composition payload (`CompositionJson`)

- `primaryKind`: string enum name
- `trackAction`: object
  - `role`: string enum name
  - `styleBundle`: string (asset path or guid)
- `partAction`: object
  - `action`: string enum name
  - `customLabel`: string
  - `musicianId`: string
- `modifierEffects`: string[] (each is asset path or guid; loaded as object references)

### Catalog entry defaults (`EntryJson`)

- `flags`: string, comma-separated, e.g. `"UnlockedByDefault,StarterDeck"`
- `starterCopies`: int (>= 1)
- `unlockId`: string

---

## 5) Known mismatches / current gaps

### A) ActionJson still defines `actions[]` but ActionCardPayload does not expose an `actions` list

- `ApplyActionJson(...)` tries to write to a serialized property named `"actions"`.
- In the current code, `ActionCardPayload` only has `actionTiming` and `conditions`.

**Impact:** `ActionJson.actions` is effectively ignored.

**Options (pick one, incremental):**
1) Remove `ActionJson.actions` from schema + code (if the project has moved away from that system).
2) Re-introduce an `actions` list in `ActionCardPayload` (if it’s still desired).

---

### B) StatusActions are editable in the Card Editor UI, but JSON import does not populate them yet

- `CardEditorWindow` draws `statusActions` via `DrawStatusActionsBlock(...)`.
- The JSON DTO does not include `statusActions` and `TryStageCardFromJson` does not write them.

**Impact:** A JSON-imported card can be staged and saved, but its CSO `StatusActions` must be authored manually after import.

---

### C) Header duplication (minor UX)

In the current drawer implementation, the UI prints a bold label and then uses `PropertyField` which prints the property label again.

**Fix:** change the `PropertyField` call to use `GUIContent.none` or remove the explicit `LabelField`.

---

## 6) Recommended next incremental step (toward CSO-aligned JSON)

To reach a CSO-aligned schema *without overengineering*:

1) Add to root DTO:

```json
"statusActions": [
  { "effectId": 100, "targetType": "Self", "stacksDelta": 2, "delay": 0 }
]
```

2) In `TryStageCardFromJson`, after payload creation, write into `statusActions` via `SerializedObject`:
- clear array
- append rows
- validate:
  - `effectId` is a valid `CharacterStatusId` (stable int)
  - `targetType` is a valid `ActionTargetType` (string enum name)

3) Keep legacy compatibility minimal:
- if `statusActions` exists → use it
- else → do nothing (designer authors effects manually)

---

## Appendix A — Example JSON (CURRENTLY WORKING)

### A1) Minimal Action card (imports metadata + conditions; StatusActions must be authored manually)

```json
{
  "kind": "Action",
  "id": "T_Action_Minimal",
  "displayName": "A Test Action",
  "performerRule": "FixedMusicianType",
  "fixedMusician": "Cantante",
  "cardType": "CHR",
  "rarity": "Common",
  "audioType": "Button",
  "inspirationCost": 1,
  "inspirationGenerated": 0,
  "exhaustAfterPlay": false,
  "action": {
    "actionTiming": "Always",
    "conditions": [
      { "typeIndex": -1, "value": 0 }
    ]
  },
  "entry": {
    "flags": "UnlockedByDefault,StarterDeck",
    "starterCopies": 1
  }
}
```

### A2) Composition card with modifierEffects by path/guid

```json
{
  "kind": "Composition",
  "id": "T_Comp_Minimal",
  "displayName": "A Test Composition",
  "cardType": "CHR",
  "rarity": "Common",
  "audioType": "Button",
  "composition": {
    "primaryKind": "WhateverEnumValueYouUse",
    "trackAction": { "role": "Guitar", "styleBundle": "Assets/ALWTTT/.../MyStyle.asset" },
    "partAction": { "action": "None", "customLabel": "", "musicianId": "" },
    "modifierEffects": [
      "Assets/ALWTTT/.../SomePartEffect.asset"
    ]
  },
  "entry": { "flags": "UnlockedByDefault" }
}
```


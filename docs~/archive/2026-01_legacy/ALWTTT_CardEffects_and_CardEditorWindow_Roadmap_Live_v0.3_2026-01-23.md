# ALWTTT — Card Effects + CardEditorWindow Roadmap (Live) (v0.3)

**Status:** Live / updated as work progresses  
**Date:** 2026-01-23  
**Companion SSoT:** `ALWTTT_CardEffects_and_CardEditorWindow_SSoT_v0.2_2026-01-23.md`

---

## 0) Current situation (audit snapshot)

### Code reality (as of latest working state)
- `CardPayload` is **effects-first**: `[SerializeReference] List<CardEffectSpec> effects`
- `ApplyStatusEffectSpec` stores a **direct reference to a `StatusEffectSO` asset** (supports variants like “Strength” vs “Super Strength”)
- `CardEditorWindow` is **effects-only for authoring + filtering** (no `statusActions` dependency)
- `CardEditorWindow.JsonImport` is now **effects-only end-to-end**:
  - JSON contract targets root-level `effects[]`
  - Import instantiates managed-reference specs into `effects` via `SerializedProperty.managedReferenceValue`
  - `ApplyStatusEffect` resolves a **concrete `StatusEffectSO`** via `StatusEffectCatalogueSO`:
    - Current resolver: `statusKey` matches `StatusEffectSO.DisplayName` OR `asset.name` (case-insensitive)
    - Optional fallback: `effectId` (CharacterStatusId backing int) if `statusKey` is missing
    - If multiple assets match the same key, importer logs a warning and picks the first
  - Legacy JSON (`statusActions`, `action.actions`) is **rejected with clear errors** (no silent fallback)
- The staged workflow remains intact: **parse → stage (in memory) → edit in UI → save assets → add to catalog**

### UX note (important, to avoid confusion)
The window shows **two different “worlds”**:
- Left panel (JSON stage/review) shows the **staged in-memory card** (`_stagedJsonCard` / `_stagedJsonPayload`)
- Right panel (Card Inspector) shows the **currently selected catalog entry asset**

So it’s possible to see different IDs/effects until you select the created entry. (Optional polish: auto-select the newly created entry after Save.)

---

## 1) Milestones (small, testable)

### Milestone A — Tooling alignment to effects-only
**Goal:** Editor + importer operate on `effects` exclusively.

#### A1 — CardEditorWindow: effects-only (DONE)
**Done**
- ✅ Filter by StatusId scans `payload.Effects` for `ApplyStatusEffectSpec.status.EffectId`
- ✅ Payload inspector draws `effects` with Add/Remove/Reorder
- ✅ ApplyStatusEffect uses catalogue-backed picker by DisplayName but stores the **asset reference**

**Regression tests**
1) UI authoring persistence (manual)
   - Create card → add ApplyStatusEffect + DrawCards → Save assets → reopen and confirm the same effects.
2) Filtering
   - Toggle “Filter by StatusId” and confirm cards appear/disappear based on status in effects.

#### A2 — JsonImport: effects-only (DONE)
**Done**
- ✅ JSON DTO uses canonical `effects[]`
- ✅ Importer creates managed-reference instances (`ApplyStatusEffectSpec`, `DrawCardsSpec`) into `CardPayload.effects`
- ✅ ApplyStatusEffect resolves **StatusEffectSO variant** via catalogue using `statusKey` (DisplayName / asset name)
- ✅ Rejects legacy `statusActions` and `action.actions` with clear “unsupported” error messages
- ✅ Staged workflow preserved: parse → stage → edit → save assets → add to catalog

**Tests (confirmed)**
1) JSON import → stage preview
   - Paste JSON example below, click “Create from JSON”
   - Expected: staged payload shows `effects` populated (ApplyStatusEffect + DrawCards)
2) JSON import → save assets → reopen
   - Save (Create Assets) and reopen/inspect created payload asset: effects persist
3) JSON import validation
   - Unknown statusKey: import fails with precise path `effects[i]...`
   - Unknown effect type: import fails with allowed types list

---

### Milestone B — Stable StatusEffect key (optional hardening / future-proofing)
**Goal:** Avoid ambiguity if `DisplayName` changes or duplicates appear.

**Why optional now**
- Today, JSON uses `statusKey` == DisplayName (or asset name), which works and is simple.
- If you later have multiple variants with the same DisplayName (or you want localization / renames), a dedicated key becomes valuable.

**Files touched (when you choose to do it)**
- `StatusEffectSO.cs`
- `StatusEffectCatalogueSO.cs`

**Tasks**
- [ ] Add `statusKey : string` to `StatusEffectSO` (required, unique within catalogue)
- [ ] Catalogue builds:
  - [ ] `Dictionary<string, StatusEffectSO> byKey`
- [ ] Add API:
  - [ ] `bool TryGetByKey(string key, out StatusEffectSO so)`
- [ ] Validation:
  - [ ] Empty key → error
  - [ ] Duplicate key → error (hard fail)

**Tests**
- Two StatusEffectSO assets share the same `EffectId` but different `statusKey` → both resolvable from JSON.
- Duplicate `statusKey` → clear error.

---

### Milestone C — Editor picker polish (optional improvements)
**Goal:** Better UX without changing the data model.

**Tasks**
- [ ] Sort dropdown by `DisplayName` (alphabetical)
- [ ] Display “(Missing)” / “(None)” consistently
- [ ] Optional: show primitive id in label (e.g., “Exposed (DamageUpFlat)”)
- [ ] Optional: after Save, auto-select the newly created catalog entry to align left/right panels.

---

### Milestone D — JSON contract hardening (post key / as needed)
**Goal:** JSON is stable and round-trips cleanly long-term.

**Tasks**
- [ ] Decide single enum strategy:
  - Current (implemented): `targetType` as string enum name (e.g., `"Self"`) with default `Self`
  - Later option: also accept ints for compactness and stability, but normalize to enums in importer
- [ ] Expand effects union as you add new specs (data-only first)

---

### Milestone E — Runtime execution layer (when ready)
**Goal:** Execute `CardEffectSpec` list in combat.

**Tasks**
- [ ] Effect executor dispatcher (data specs → runtime commands)
- [ ] Start with:
  - ApplyStatusEffectSpec
  - DrawCardsSpec

---

## 2) Canonical JSON contract (v0.3)

### Top-level (minimal)
```json
{
  "kind": "Action",
  "id": "TEST_EFFECTS_01",
  "displayName": "Test Effects 01",
  "effects": [ ... ]
}
```

### effects[] — tagged union by `type`

#### DrawCards
```json
{
  "type": "DrawCards",
  "count": 2
}
```

#### ApplyStatusEffect (current resolver: DisplayName / asset name)
```json
{
  "type": "ApplyStatusEffect",
  "statusKey": "Exposed",
  "targetType": "Self",
  "stacksDelta": 2,
  "delay": 0.0
}
```

**Notes**
- `statusKey` currently resolves to a concrete `StatusEffectSO` by matching:
  - `StatusEffectSO.DisplayName` OR `asset.name` (case-insensitive)
- Optional fallback (if you want it): `effectId` (CharacterStatusId backing int) if `statusKey` is omitted.
- `targetType` uses the enum name string. If missing/empty, importer defaults to `Self`.

---

## 3) Mid-term expansion (data-only first)
Add new effect specs (repeat the loop: Spec → Editor → JSON → Runtime):
- DiscardFromHandSpec
- DealDamageSpec
- ShuffleCurseSpec
- GenerateChoicePoolSpec / ChooseFromPoolSpec
- ModifyEnergySpec / ModifyCostSpec (if applicable)

---

## 4) Next concrete session plan (minimum)
1) (Optional) Milestone C: polish picker + auto-select newly created entry after Save (reduce confusion)
2) Add 1–2 new non-status effect specs (e.g., DiscardFromHandSpec)
3) Start Milestone E: runtime dispatcher for ApplyStatusEffect + DrawCards

# SSoT_Card_Authoring_Contracts — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Authoring, serialization, staged editor workflow, JSON/import contracts, status-variant identity for cards  
**Owns:** how cards and authored statuses are represented and imported  
**Does not own:** combat feel, phase timing, balance, or runtime economy semantics

---

## 1. Purpose

This document is the governed promotion of the previous **Appendix — Authoring & Data Contracts**.

It is the primary authority for:
- card authoring contracts
- `CardDefinition` / `CardPayload` representation rules
- effect-list representation
- JSON import schema rules
- staged editor workflow invariants
- status variant identity used by card authoring/import

---

## 2. Scope boundaries

### 2.1 This SSoT is normative for
- how cards are represented as assets/data
- how card effects are serialized/authored
- how the import pipeline should populate the modern effect list
- how status variants are resolved from authored/imported data

### 2.2 This SSoT is not normative for
- combat pacing and phase order
- loop/song/gig semantics
- balance numbers
- audience/combat feel
- package-side MidiGenPlay internal tooling

Combat/runtime meaning lives in subsystem/runtime SSoTs.

---

## 3. Core representation contracts

### 3.1 CardDefinition vs CardPayload
Contract:
- a `CardDefinition` holds identity/presentation/economy/catalog-facing metadata
- a `CardPayload` holds mechanics
- a `CardDefinition` references exactly one payload asset

### 3.2 Unified effect list
All authored card mechanics live under the effect-first contract:

```text
CardPayload.effects : List<CardEffectSpec>
```

Runtime/editor read access may expose this as `IReadOnlyList<CardEffectSpec> Effects`.

Extension rule:
- adding a new mechanic means adding a new `CardEffectSpec` subclass plus matching editor/import/runtime support

### 3.3 Status application is a normal effect
Status application is authored as a card effect type, not as a separate parallel system.

Canonical example shape:

```text
ApplyStatusEffectSpec {
  StatusEffectSO status;
  ActionTargetType targetType;
  int stacksDelta;
  float delay;
}
```

Rule:
- cards reference a **concrete `StatusEffectSO` asset**
- they do not reference only an abstract primitive id when authored/imported

### 3.4 Action timing and testability

Action cards declare an `actionTiming` field (`CardActionTiming` enum) that gates when the card is legal to play. The default value **excludes `PlayerTurn`**, which means action cards authored without an explicit `actionTiming` are not playable during the standard player turn.

This default is intentional for most authored combat content — action timing is typically declared deliberately per card — but it creates a sharp edge for **testing/debug cards intended to be spawned into the hand via Dev Mode**: without `actionTiming: Always`, the spawned card sits unplayable in the hand regardless of other conditions.

**Convention for testing/debug cards:**  
Any action card authored primarily to exercise runtime behavior (effect validation, status application tests, meter manipulation) via the Dev Mode card spawner must declare `actionTiming: Always` explicitly. This makes the card playable during any phase that permits action-card play.

Cross-reference: Dev Mode gating around card spawn is governed in `SSoT_Dev_Mode.md` §8.4 and §11.4.

---

## 4. Status variant identity contract

### 4.1 Problem being solved
Multiple authored statuses may share the same abstract primitive while differing in tuning/config.
A catalogue keyed only by primitive id does not represent this cleanly.

### 4.2 Required governed fix
Each `StatusEffectSO` must support a stable unique variant identifier:

```text
statusKey : string
```

### 4.3 Catalogue requirements
The catalogue must support:
- primary lookup by `statusKey`
- optional grouping by primitive id
- optional default-per-primitive fallback only for backwards compatibility

Validation rules:
- duplicate `statusKey` is a hard authoring/import error
- multiple variants sharing the same primitive id are allowed

---

## 5. JSON schema contract (v2 direction)

### 5.1 Core rule
JSON import must populate **`effects`**.
Legacy shapes like separate `actions` / `statusActions` are not ongoing primary schema.

### 5.2 Enum serialization rule
Use stable string names for enums by default.
Case-insensitive parsing is acceptable if applied consistently.

### 5.3 Minimal top-level card schema

Required:
- `kind` — `"Action"` or `"Composition"`
- `id`
- `displayName`
- `effects`

Optional:
- `performerRule`, `fixedMusician`
- `cardType`, `rarity`, `audioType`
- `inspirationCost` — unchanged; the inspiration economy is now
  **fixed-income**: a flat per-loop grant (3 at S5e; re-tuned at S5i) plus
  card costs. **[S5e]**
- `inspirationGenerated` — **deprecated for content as of S5e (D3)**: parser
  and DTOs still accept it (import coexistence), but all starter-deck
  content is authored at 0 and new content must author 0. Basal
  card-driven generation is removed; a possible future `+INS` CardEffect is
  design-note only (explicitly deferred, not implemented). **[S5e-ext]** The
  card UI (hand + detail view) hides the gen badge entirely when this field
  is 0 — since all current content is at 0, the badge is presently never
  shown; it will reappear automatically for any future card authored above 0.
- `exhaustAfterPlay`
- `keywords` — string array of `SpecialKeywords` enum names (e.g. `["Exhaust", "Consume"]`). Case-insensitive parsing. Unknown values emit a warning and are skipped. See §5.8 for coherence rules.
- `overrideRequiresTargetSelection`, `requiresTargetSelectionOverrideValue`
- `cardSpritePath`
- `action`, `composition` — domain-specific payload blocks (composition-block CE-L1 fields `modifierEffectNames` + `palette` intent are documented in §5.12)
- `entry` — catalog-entry defaults (see §5.6)
- `resourceCostStatusKey`, `resourceCostAmount` — resource cost pair (see §5.3a)

### 5.3a Resource cost (R5-d, D-R5-26=A)

Two serialized fields on `CardDefinition`, authorable on **any** card of either domain:

| Field | Type | Default | Meaning |
|---|---|---|---|
| `resourceCostStatusKey` | `string` | `""` | `StatusKey` of the status spent on play. Resolved from the **payer's** catalogue. |
| `resourceCostAmount` | `int` (≥0) | `0` | Stacks spent. |

`HasResourceCost` is true only when both halves are authored. The key must match the
`StatusEffectSO`'s `StatusKey` exactly; a mismatch reads at runtime as "not enough resource",
so it is the first thing to check when a card is permanently unplayable.

**The cost is not an effect spec.** Specs execute after the play is committed; a cost there
could be paid with nothing. See `SSoT_Status_Effects.md` §5.10.


**Orden de puertas — defecto abierto (F-R5f-1, 2026-08-28; D5=A, sin construir → R5-g).** El par
resuelve contra el catálogo del **pagador**, pero *cuándo* se cobra difiere por ruta. En la ruta
de **composición** el recurso se comprueba **antes** del presupuesto ECON-1 (correcto,
ST-R5f-16 PASS). En la de **acción** se comprueba **después**, y como el hook de generación de
Voltage vive dentro de `TryConsumePlay`, una carta de coste N jugada por un músico que genera ese
mismo recurso cuesta **N−1** efectivo. Al autorar un coste, contar con esa diferencia hasta que
R5-g la elimine. Corrige la afirmación «net-neutral» de D-R5-26=A, cierta para el saldo y falsa
para la puerta.

### 5.4 Effect object rule
Each effect entry must contain:
- a stable discriminator such as `type`
- only the fields required by that effect type

### 5.5 Example — ApplyStatusEffect
```json
{
  "type": "ApplyStatusEffect",
  "statusKey": "Exposed",
  "targetType": "Self",
  "stacksDelta": 2,
  "delay": 0.0
}
```

Resolution contract:
- `statusKey` resolves via the status catalogue
- unresolved status references must fail import clearly
- optional fallback identifiers may exist, but are not preferred primary authoring paths

### 5.6 Example — DrawCards
```json
{
  "type": "DrawCards",
  "count": 2
}
```

### 5.6a Example — AddInspirationPerLoop (DF-INSPLOOP, 2026-07-16)
```json
{
  "type": "AddInspirationPerLoop",
  "amount": 2
}
```
Grants `amount` Inspiration at each loop boundary while the carrying card's track is active (track-scoped). `amount` must be `>= 1`; import rejects `< 1`. Meaningful only on **Composition Track** cards — on an Action card the spec is inert (defensive no-op branch in `CardBase.ExecuteEffects`, never bound as a track source). The importer does not hard-gate card kind; a kind mismatch is harmless, not an error.

### 5.6b Example — RevealPreferences (R4, 2026-08-10)
```json
{ "type": "RevealPreferences", "targetType": "AudienceCharacter" }
```
Reuses `targetType`; no new DTO fields. Reveals the target's `TastePreferences` on the audience canvas (runtime meaning: `SSoT_Audience_and_Reactions.md` §6.4).

### 5.6c Example — GrantBonusLoop (R5-d, 2026-08-26)
```json
{ "type": "GrantBonusLoop", "soloOverBonusLoop": true }
```
Extends the running part by one loop (`CompositionSession.TryGrantBonusLoop`), capped by
`GigFlowSettingsSO.MaxBonusLoopsPerPart`. `soloOverBonusLoop` (default `true`) asks for a
render-scope soloist track over a byte-identical base
(`SSoT_Runtime_CompositionSession_Integration.md` §8 inv 14). Meaningful only while a part is
looping; outside that window the play is denied as `UnplayableReason.NoRunningLoop`.

**Supported effect discriminators are now seven:** `ApplyStatusEffect`, `ModifyVibe`,
`ModifyStress`, `DrawCards`, `AddInspirationPerLoop`, `RevealPreferences`, `GrantBonusLoop`.

> **Authoring-route caveat (verified 2026-08-26).** The JSON import path supports all seven.
> The **LLM generation route does not**: `CardLLMPromptBuilder` enumerates the discriminators
> by hand and lists only six. Until `GrantBonusLoop` is added there, the generator will never
> emit it. That is a **code** gap, not a documentation one — see `Report_CardLLM_Pipeline.md`.

### 5.7 Batch wrapper schema

Multiple cards can be imported in a single JSON payload using a batch wrapper:

```json
{
  "defaultEntry": { "flags": "StarterDeck,UnlockedByDefault", "starterCopies": 2 },
  "cards": [
    { "kind": "Action", "id": "card_a", "displayName": "Card A", ... },
    { "kind": "Action", "id": "card_b", "displayName": "Card B",
      "entry": { "flags": "UnlockedByDefault", "starterCopies": 1 } }
  ]
}
```

**`defaultEntry`** (optional): An `EntryJson` object applied to any card in the batch whose own `entry` block is absent or has empty/null `flags`. Per-card `entry` blocks override the default entirely (not merged field-by-field).

**`EntryJson` fields:**
- `flags` — comma-separated `CardAcquisitionFlags` names (e.g. `"StarterDeck,UnlockedByDefault"`). Synonyms: `"Reward"` / `"Rewards"` → `"RewardPool"`.
- `starterCopies` — integer, default 1. Only meaningful when `StarterDeck` flag is set. Authoring-only metadata at M4.4; runtime consumption is scheduled for M4.6 when catalogue → starter-deck auto-assembly is implemented (at that point, `starterCopies` becomes the source of `BandDeckEntry.count` per card).
- `unlockId` — string. Required when `UnlockedByDefault` is not set.

**Merge rule:** Unity's `JsonUtility` default-constructs class fields even when absent from JSON. The importer uses `flags` as the discriminator: if a card's `entry.flags` is null or whitespace, the entry is treated as absent and the batch `defaultEntry` is used instead.

### 5.8 Keyword coherence rules

The JSON importer emits non-blocking `Debug.LogWarning` messages when `exhaustAfterPlay` and the `Exhaust` keyword diverge:

- `exhaustAfterPlay: true` without `"Exhaust"` in `keywords` → warning: players won't see an Exhaust tooltip.
- `"Exhaust"` in `keywords` without `exhaustAfterPlay: true` → warning: tooltip says Exhaust but card won't exhaust.

These warnings do not block import. They flag an authoring gap that will be resolved when keywords drive runtime behavior directly (see `SSoT_Card_System.md` §3.3.3).

### 5.9 — Audience-side status authoring (M4.3)

`ApplyStatusEffect` JSON resolution probes both catalogues on `ALWTTTProjectRegistriesSO` (musicians-first, then audience) via `TryGetStatusEffectByKey` / `TryGetStatusEffectByPrimitive`. Authoring a card that applies an audience-side status (e.g. `statusKey: "earworm"`) requires both `StatusCatalogueMusicians` and `StatusCatalogueAudience` fields populated on the registries asset. The Card Editor toolbar surfaces a warning enumerating any missing field. Single-catalogue setups will report a key-not-found error at import time.

The Card Editor and Deck Editor JSON import paths apply this contract uniformly. Both `CardEditorWindow_JsonImport.ApplyEffectsJson` and `DeckCardCreationService.ApplyEffectsJson` consume `ALWTTTProjectRegistriesSO` directly and use the same registries-helper resolution (M4.6-prep-A, closed 2026-05-01).

### 5.10 — Deck-level multiplicity (M4.4)

Deck JSON entries support a per-entry `count` field, integer, default 1. The field applies to both "reference existing" entries (`{ "cardId": "...", "count": 3 }`) and "create new" entries (`{ "kind": "Action", "id": "...", "count": 2, ... }`).

**Duplicate `cardId` references are combined additively** with a non-blocking warning: `"Duplicate cardId 'X' was combined into a single entry (now ×N). Consider authoring 'count' explicitly."` Two `{ "cardId": "x" }` entries (or one `count: 2` plus one bare entry) collapse into a single staged entry with summed count.

**Duplicate `kind`-bearing ids remain a hard error.** Two `{ "kind": "Action", "id": "foo", ... }` entries in the same deck JSON are conflicting definitions, not copies, and import fails. The pendingCard / pendingPayload of the rejected duplicate is destroyed before the failure to avoid in-memory leaks.

**Round-trip:** Export emits `count` on every entry. Import respects it. A deck authored as `×3 Steady Beat` round-trips through Deck Editor → asset → JSON export → JSON re-import → asset with the count preserved.

Runtime semantics live in `SSoT_Card_System.md §13`.

### 5.11 — Per-musician starter deck auto-assembly (M4.6-prep batch (2))

Auto-assembly is the runtime path that builds the gig deck from each musician's `MusicianCardCatalogData` plus an optional `GenericCardCatalogSO`, as an alternative to the legacy `BandDeckData` asset path.

**Selection rule (per-musician).** For each musician in `pd.MusicianList`, read `MusicianCharacterData.CardCatalog`. For each `MusicianCardEntry`: if `entry.IsStarter` (i.e. `flags & StarterDeck != 0`) AND `entry.card != null` AND `entry.starterCopies >= 1`, contribute `entry.starterCopies` independent references to the appropriate domain pile (Action vs Composition, derived from `entry.card.IsAction` / `entry.card.IsComposition`).

**Selection rule (generic).** If `GigSetupConfigData.GenericStarterCatalog != null`, apply the same selection rule to each `MusicianCardEntry` in `GenericCardCatalogSO.Entries`. Entry shape is identical (`MusicianCardEntry` is reused, not duplicated).

**`starterCopies` semantics.** Editor-time clamps: `MusicianCatalogService.TryAddEntry` applies `Mathf.Max(1, starterCopies)`; `MusicianCardEntry.starterCopies` carries `[Min(1)]`. Runtime defensive: `starterCopies <= 0` AND starter-flagged entries are warn-skipped (logged as `skippedZeroCopies`), not silently coerced. Authoring should always produce `starterCopies >= 1`.

**Provenance contract.**
- Per-musician contributions populate `PersistentGameplayData.musicianGrantedActionCards` / `musicianGrantedCompositionCards` keyed by `MusicianCharacterData.CharacterId`. `RemoveMusicianFromBand(id)` strips them when the musician departs.
- Generic-catalogue contributions do NOT populate provenance. They are not "from" any specific musician, so they survive `RemoveMusicianFromBand` correctly.
- **Subtle case:** when the same `CardDefinition` lives in both a per-musician catalog AND the generic catalog, removal strips the per-musician copy and leaves the generic copy. Provenance follows the contribution path, not card identity. This is the intended contract.

**Deck reset semantics.** `SetBandDeckFromMusicians` mirrors `SetBandDeck(BandDeckData)`: clears `currentActionCards`/`currentCompositionCards`, resets both granted-cards inventories, sets `isRandomDeck = false`. Empty roster is warn-and-continue (logs an empty-roster warning, leaves piles empty).

**`MusicianCharacterData.BaseActionCards` / `BaseCompositionCards`.** These remain on the type as transitional helpers; both flatten `CardCatalog` entries via the same `IsStarter` filter and `starterCopies` expansion. `CardCatalog` is the single source of truth — there is no parallel system. `AddMusicianToBand(MusicianCharacterData)` (sector-map flow, `GrantCardsToMusician`) consumes the helpers; `SetBandDeckFromMusicians` (Gig Setup flow) reads the catalogue directly. Both paths produce identical card sets.

**Toggle and selection.** `GigSetupConfigData.AvailableBandDecks` (legacy `BandDeckData` asset list) is demoted to a dev/test fallback. `GigRunContext.RunConfig.useMusicianStarters` (set from `GigSetupController.useMusicianStartersToggle`, default ON) selects between auto-assembly and the legacy path inside `PersistentGameplayData.ApplyRunConfig`. Both paths must produce a well-formed deck; emptiness is checked separately via `GigSetupController.OnStartPressed`'s pre-flight roster guard (auto-assembly path only).

Deck-label logging: `RunConfig.deckLabel` carries either `bandDeck.name` (legacy) or `<auto:idA+idB+...>` (auto). Used in `[GigRunContext] BeginRun`, `[PersistentGameplayData] ApplyRunConfig`, and `[GigSetup] Starting gig` log lines for traceability.

### 5.12 — Composition payload block: `modifierEffectNames` + palette intent (CE-L1, 2026-06-11)

The `composition` payload block gained two CE-L1 authoring fields. Both are parsed by the shared `CardImportDtoParser` (in `Assets/Scripts/Cards/LLMAuthoring/`), which both the JSON batch-import box and the Card Editor's LLM routes (§4.10 of `SSoT_Editor_Authoring_Tools.md`) feed through. Their *resolution*, however, differs by route — see the route-scope note below.

**`composition.modifierEffectNames`** — string array of `PartEffect` asset **names** (not paths). Resolved at staging inside `TryStageCardFromDto`: exact case-insensitive name match, **all-or-nothing** — a missing name fails listing the available effects, an ambiguous name fails listing the colliders. Preferred over the legacy `composition.modifierEffects` path/guid array for new authoring (the path array is retained for backwards compatibility; the LLM route bans non-empty path entries outright). Resolution is uniform across both import routes.

**`composition.palette`** — palette *intent*, not a palette asset reference:

```json
{ "palette": { "requested": true, "timeSignature": "ThreeFour", "keywords": ["waltz"] } }
```

- `requested` (bool) — explicit-presence flag. Required because Unity's `JsonUtility` default-constructs absent nested objects, so the palette object's mere presence is not a signal. Intent is recognized content-based (`CardLLMResponseHandler.HasPaletteIntent`); "any palette, no constraints" is expressed by `requested: true` with no other fields.
- `timeSignature` (optional, `TimeSignature` enum name) and `keywords` (optional string array) filter the candidate palettes.
- Resolution is deterministic and seeded: `CardPaletteIntentResolver` selects over the project's real `DrumPatternPaletteSO` / `ChordProgressionPaletteSO` assets via the CE-F1 `PaletteSelector` (exact-meter tier → heuristic tier → raw weights), seeded by the panel's intent seed (same payload + same seed ⇒ same pick). Rhythm role → drum palettes; Backing role → chord palettes; Melody/Harmony intent fails loudly (no palette types exist); unmatched keywords fail listing the available palettes. The payload never names an asset.

**Route scope (normative, code-verified 2026-06-12).** Palette intent is resolved **only** on the LLM routes — the Card Editor's *Generate* and *Import from clipboard* buttons, whose outcome carries a field plan consumed at Save by `ApplyLlmPlanOnSave`. The plain JSON **batch-import box** parses `composition.palette` into the DTO but **ignores it**: there is no field plan on that path, so `ApplyLlmPlanOnSave` is a no-op and no palette is resolved. *(Partially superseded by §5.13, BASS-CARD-1, 2026-07-12: the batch box **can** now mint and configure a style bundle via `trackAction.styleBundleCreate`. What it still cannot do is resolve palette **intent** — a palette can only be attached on that route by naming the asset, never by describing it.)* Do not author `composition.palette` in batch JSON expecting it to take effect; on that route either point `trackAction.styleBundle` at an existing bundle asset, mint one inline with `trackAction.styleBundleCreate`, or use the editor's bundle creator. `modifierEffectNames`, by contrast, resolves identically on both routes.

### 5.13 — `composition.trackAction.styleBundleCreate` (BASS-CARD-1, 2026-07-12)

Before this, a Composition card imported from JSON could only **point at** an existing `TrackStyleBundleSO` (`trackAction.styleBundle` = asset path or guid). It could neither create one nor set any field on it. That made **Bassline cards unauthorable from JSON**: a `BasslineCardConfigSO` carries no palette — only articulation — so there was nothing to point at until someone hand-made the asset.

`trackAction.styleBundleCreate` mints a **role-typed** bundle at Save and optionally writes fields on it:

```json
{
  "composition": {
    "primaryKind": "Track",
    "trackAction": {
      "role": "Bassline",
      "styleBundleCreate": {
        "requested": true,
        "fields": [
          { "name": "chordExpression", "value": "ArpeggioUp" },
          { "name": "arpeggioRate",    "value": "Eighth" }
        ]
      }
    }
  }
}
```

- **`requested`** (bool) — explicit-presence flag, for the same reason as `palette.requested` (§5.12): `JsonUtility` default-constructs absent nested objects, so mere presence is not a signal. Intent is recognized content-based (`requested == true` **or** `fields` non-empty). "Mint with all defaults" is `{"requested": true}`.
- **`fields`** (optional) — `{ name, value }` pairs. `name` is a **serialized field name** on the bundle SO. `value` is always a string and is coerced by the target property's type: enum **by name** (case-insensitive), int, float, bool, string, or object reference by asset path/guid.
- **Bundle type is derived from `role`**, via the Card Editor's existing `ResolveBundleTypeForRole` — the same map behind the wizard's role buttons. `Bassline` ⇒ `BasslineCardConfigSO`, `Backing` ⇒ `BackingCardConfigSO`, `Melody` ⇒ `MelodyCardConfigSO`, `Harmony` ⇒ `HarmonyCardConfigSO`, `Rhythm` ⇒ `RhythmCardConfigSO`.

**Normative rules.**

1. **Mutually exclusive with `styleBundle`.** Both present ⇒ hard failure at staging.
2. **Requires `role`** (the bundle type is derived from it) and a **Composition** card. Either missing ⇒ hard failure at staging.
> **Snapshot de campos caducado — F-R5f-7 / F-R5f-8 (2026-08-28).** La enumeración de campos de
> `BasslineCardConfigSO` que aparece arriba está fechada 2026-07-31 y **no es censo**. Medición
> por Inspector en R5-f: la superficie viva incluye además `selfPocketPhraseLengthBars`,
> `selfPocketBarSubstitutions`, `selfPocketVariantSelection`, `hammerOffsetDegrees`,
> `pullOffsetDegrees`, `ghostVelocityFactor`, `ghostPopVelocityFactor`,
> `hammerOnVelocityFactor`, `pullOffVelocityFactor` y `ghostGateBeats`. **No es divergencia de
> contrato** — la regla 6 dice que la superficie es propiedad del paquete y crece, y que esta
> SSoT no la enumera. Pero un snapshot caducado engaña igual que un error, así que se marca en
> vez de actualizarse: **actualizar el snapshot solo reinicia el reloj**. La cura estructural es
> el export por **reflexión** sobre campos serializados propuesto en R5-f, que hace innecesaria
> toda lista escrita a mano.
>
> **Ejemplo trabajado obsoleto.** El ejemplo de Slap Bass v1 (D-R2-11) describe `chordExpression
> = Offbeat`, patrón `[Slap, Pop]` @ `Beat` y boosts `0/+12`. El asset vivo de
> `starter_slap_bass` es `BasslineCardConfig_SlapV1` (repuntado en R5-f desde `SlapV3`) con
> boosts a **0/0**. Los documentos que dicen `SlapV5` —CSV-4c y `SSoT_Editor_Authoring_Tools`
> §17— nunca describieron el estado vivo. Reautorar el ejemplo contra el asset real es trabajo de
> **CARD-VAR-1**, no de R5-f.
3. **Unknown field names are hard errors, never silent skips.** The importer logs the offending name *and the bundle's full list of valid serialized field names* — a wrong guess is self-correcting.
4. **Banned from LLM output.** `styleBundleCreate.fields` can carry asset paths (object-reference coercion), which is precisely the channel the §3.3 banned-asset-path guard exists to close. The LLM route does not need it: its bundle is minted by the field plan (`ApplyLlmPlanOnSave`) and its asset intent travels through `composition.palette`. **Hand-authored JSON only.** Enforced in `CardLLMResponseHandler.ApplyBannedFieldGuard`; covered by EditMode tests.
5. **Applied at Save, not at staging** — `ApplyJsonBundleCreateOnSave` runs after the payload asset exists on disk, because the bundle's folder is derived from the payload's asset path. Consumed exactly once; cleared by `DiscardStagedJson`.
6. **The bundle's field set is package-owned and grows.** `styleBundleCreate.fields` writes
   serialized fields by name, so the authorable surface is whatever the package version defines
   — it is **not** enumerated in this SSoT, and a stale mental model produces a hard import
   error, not silent wrong content (rule 3). As of MidiGenPlay 2026-07-31,
   `BasslineCardConfigSO` accepts: `chordExpression`, `arpeggioRate`, `arpeggioToneMode`
   (`RepeatedNote` / `ChordToneWalk` / `ImprovisedWalk`), `randomRerollChance`,
   `randomFigureWeights`, `velocityJitter`, the pocket block (`pocketMode` =
   `Off` / `SlapPocket` / `SelfPocket`, `pocketSlapBoost`, `pocketPopBoost`, `pocketCustomLanes`,
   `pocketSlapLanes`, `pocketPopLanes`) and the SelfPocket block (`selfPocketPattern`,
   `selfPocketSubdivision`). Contract and adoption status:
   `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.4 + §8.6.

   **Three authoring hazards at this surface.**
   (i) In `ChordExpressionType`, the member named **`Bossa` is not the register-selective
   bass/upper split** — that is `BassUpperSplit`. Authoring `Bossa` for a split imports cleanly
   and sounds wrong.
   (ii) **`pocketMode = SlapPocket` triggers a consumer-side cache duty** (boundary §8.4): the
   bass stem then depends on the Rhythm track's resolved pattern, which the Bassline
   `trackInputsHash` does not include. Do not author a `SlapPocket` bundle without the
   accompanying hash change in the same batch. **`SelfPocket` reads no other track and is free
   of this duty** — prefer it unless drum-locked coupling is specifically wanted.
   (iii) **SelfPocket boosts are additive on top of the chord-event velocity and clamp at 127.**
   Large symmetric boosts (`+64/+64`) saturate every hit and flatten the dynamic contour. Tuned
   default: `0` slap / `+12` pop.

   **Worked examples (R2 / R2d).**
   *Finger Bass v1 (D-R2-4=A):* `chordExpression = ArpeggioUp`, `arpeggioRate = PerBeat`,
   `arpeggioToneMode = ChordToneWalk`. On a monophonic line `ArpeggioUp` at rate `PerBeat`
   places exactly one note per beat — the same rhythmic placement as the `PerBeat` figure — so
   switching to a walk changes **pitch content only** and preserves the card's rhythmic
   identity. `arpeggioRate` has no "quarter" member; one-note-per-beat **is**
   `ArpeggioRate.PerBeat`.
   *Slap Bass v1 (D-R2-11):* `chordExpression = Offbeat`, `pocketMode = SelfPocket`,
   `selfPocketPattern = [Slap, Pop]`, `selfPocketSubdivision = Beat`, boosts `0` / `+12`.

### 5.14 — Track cards: `styleBundle` is what creates the track (BASS-1 D4=A, 2026-07-12)

**A Composition card with `primaryKind: Track` and no `trackAction.styleBundle` does not create a track.** It augments the target musician's existing track *of that same role* if one exists, and otherwise applies only its part effect (`modifierEffects` / `modifierEffectNames`).

This is intentional and is the runtime semantic D4=A, whose authority is `SSoT_Runtime_CompositionSession_Integration.md` §11 — restated here because it is the single rule most likely to trip a card author.

**Authoring consequences.**

- A Track card meant to **add a track** must carry a bundle: either `trackAction.styleBundle` (existing asset) or `trackAction.styleBundleCreate` (§5.13). This is not optional. A bass card with an empty bundle slot is **inert** — it will silently add nothing.
- A Track card meant as a **PartEffect carrier** (Key Lift, Push It, Half Time) correctly leaves the bundle empty. Its `role` now selects *which* of the target musician's tracks it augments, which is a strengthening: previously it retargeted whatever single track the musician had, regardless of role.
- The Card Editor's wizard role buttons and the LLM route both mint a bundle automatically, so only hand-written JSON can produce a bundle-less Track card by accident.

### 5.15 — Card audio: drop-time vs impact-time (JUICE-PW / D-PW-AUDIO, 2026-07-13)

`audioType` (the card-authored `AudioActionType`, §5.3) plays **when the card is dropped**, in
`CardBase.Use`, and is **opt-in by type**: `Button` (the enum's 0-value and the default for any
unset field) and `None` play nothing.

Since JUICE-PW a second, **bus-side** producer exists for one card effect: a card carrying a
`ModifyVibe` effect publishes `AudienceVibeImpactEvent` at effect resolution, and
`SensoryAudioAdapter` plays a single `CardVibeImpact` sting **at impact**. Semantics and key
authority: `SSoT_Audio.md` §3 + invariant 18.

**Authoring rule.** The two paths are not mutually exclusive in code. A card whose sound should be
its *impact* (a finisher, an AoE payoff) must be authored **`audioType: None`** — otherwise it
sounds twice, once on the drop and once on the hit. A card whose sound should be its *cast* keeps a
clip-backed `audioType` and gets no impact sting beyond what the bus already provides for its
targets' reactions.

**Currently authored this way:** `Psychic Waves` (Sibi, AoE `ModifyVibe +5`; the tutorial's beat-8
finisher — `Design_Starter_Deck_v1.md` §5.17).

**Not a new schema field.** This rule constrains the *value* of an existing field; no DTO, no
importer, and no LLM-facing vocabulary changes.

### 5.16 — Meter authoring: a part's meter is owned by a `MeterEffect` (D-MEL-1=A, CSV-3, 2026-07-22)

A part's meter is a **model-construction default** (`FourFour`) mutated **only** by a
`MeterEffect`. A rhythm card that presents a non-4/4 identity (e.g. a "6/8" card) **must**
carry a matching `MeterEffect` in its `CompositionCardPayload.modifierEffects`, or the part
stays 4/4 and **every** track (drums included, via TS normalization) renders in 4/4 regardless
of the card's apparent meter.

**Precedent:** Pentameter — a Rhythm card that also sets TS=5/4. **Not a new schema field:**
this constrains the *composition* of an existing effect list. Runtime authority for the
resolution rule: `SSoT_Runtime_CompositionSession_Integration.md` §12.

---

### 5.17 — Composition cards: tonalidad, adopción y armonía compartida (R3, 2026-08-08)

**D4=A — regla normativa.** Una carta **no debe** llevar a la vez un `TonalityEffect` y un
bundle de Backing con `adoptProgressionTonality`. La adopción gana en silencio en tiempo de
compose y el efecto se convierte en una mentira que el autor no puede ver. Validador de dos
capas: editor al guardar (`DrawCompositionPayloadEditor`) + aviso en runtime al jugar
(`TryPlayCompositionCard`).

**Regla de autoría modal.** Escribir grados planos, sin alteraciones: en Frigio, el grado II
*es* ♭II. Escribir `bII` lo baja dos veces. El color viene de la tonalidad adoptada, no de la
notación.

**Pre-flight de adopción.** Toda entrada de paleta destinada a adoptar necesita una lista
`tonalities` no vacía y con un **modo real en `[0]`**. Que la lista esté poblada no basta:
un esqueleto mode-agnostic con Ionian en primera posición adopta Ionian y no hace nada.

**Peligro de `subdivisions`.** Es el denominador de unidad, derivado en la ruta Roman.
Editarlo a mano reescala los eventos; el síntoma es acordes que cambian muy rápido seguidos
de silencio.

**Tonalidad — regla de persistencia.** La tonalidad de la parte la mueven **solo** las cartas
con autoridad tonal (`TonalityEffect`, `ModulationEffect`), que mutan el modelo de UI antes
del render. `adoptProgressionTonality` **no** es una de esas cartas: la adopción muta el
`PartConfig` por render durante el compose y **nunca alcanza el modelo**. Un modo adoptado
dura por tanto exactamente lo que dura la carta que lo adopta — *la carta es el modo* — y se
propaga al resto de la parte únicamente a través de **JAM-2** mientras haya una entrada de
jam viva.

> ⚠ **Esto CORRIGE, y no reemplaza en silencio, a D-R3C-3 tal como quedó recogida en el
> handoff §2.** La decisión (**A**) sigue en pie; lo que era falso es su **premisa** escrita
> ("la tonalidad de la parte persiste hasta que una carta con autoridad tonal la mueva"): eso
> era cierto del modelo, pero se estaba leyendo como cierto también de la adopción. Evidencia:
> toda línea de adopción lee `Ionian -> X`, y `[SongConfigBuilder]` reporta `Tonality: Ionian`
> en cada render, incluidos los que adoptan. La decisión pasa a citarse como **D-R3C-3=A′**.

**NO "arreglar" el aviso de desajuste modal.** Cuando JAM-2 no está en juego, el paquete
puede emitir:

> `Progression '…' was authored for [Lydian] but part 'X' is Ionian; rendering AsAuthored in
> the part's tonality (card wins). Consider qualityRenderPolicy=DiatonicToPart on the asset
> if it should adapt diatonically.`

**No apliques `DiatonicToPart` a entradas de paleta modal.** Bajo esa política, el II de un
vamp lidio se reproyectaría a un acorde menor cuando la parte lee Ionian: la carta de estilo
cambiaría en silencio los acordes de la canción — exactamente el fallo que JAM-1 existe para
evitar. `AsAuthored` es lo correcto para vamps ligados a un modo. `DiatonicToPart` es para
progresiones pensadas para adaptarse (p. ej. `Test - Lab Progression`).

**Nota acotada sobre `IV9` y grados desnudos.** Verificado en vivo: un asset modal almacenado
que contiene `IV9` renderiza correctamente (`GSharp9(IV⁹)`). El peligro es **solo de
re-importación**: `9` está fuera del alfabeto de calidades v1 y los grados desnudos se
resuelven por inferencia de mayúsculas/minúsculas, así que re-importar esas cadenas Roman
puede no reproducir el asset almacenado. Los assets almacenados son seguros: trátalos como
artefactos autorados, no como texto round-trippable.

### 5.18 — Arquetipo de carta articulation-only (R3, 2026-08-08)

**No requiere funcionalidad nueva.** Una carta de Backing articulation-only ya es legal hoy:
un `BackingCardConfigSO` con `progressionOverride` **y** `progressionPalette` en `None` es una
carta que funciona (R2d reescribió la guarda del paquete para que el default del host solo se
descarte cuando la fila de Backing *lleva armonía de verdad*).

**JAM-1 es lo que hace bueno el arquetipo.** Antes, una carta articulation-only caía a la
paleta por defecto del host y por tanto traía *acordes distintos*: el jugador cambiaba el
estilo y perdía la canción en silencio. Con JAM-1 la armonía almacenada se impone sobre la
pista de Backing, así que la carta entrega exactamente lo que promete: **misma progresión,
ropa nueva.** JAM-2 extiende esa garantía a contextos modales.

**Figuras disponibles** (`ChordExpressionType`, todas autorables hoy, cero código):
`Block` · `PerBeat` · `Offbeat` (upstroke ska/reggae) · `Staccato` · `ArpeggioUp` /
`ArpeggioDown` (usan `arpeggioRate`) · `Random` (pool ponderado vía `randomFigureWeights`) ·
`PowerChord` · `Chugging` (palm-mute; reutiliza `arpeggioRate` como tasa de pulso) ·
`BassUpperSplit` · `Bossa`.

> ⚠ **Trampa de nombres.** `BassUpperSplit` (valor 9) se llamaba antes `Bossa` y se renombró
> porque su alternancia regular downbeat/offbeat suena como un upstroke ska tranquilo
> (hallazgo F-BOSSA-FEEL). La figura de comping auténtica es `Bossa` (valor 10): pulso LOW en
> 0.0 y **2.0 fuerte** (el peso de surdo cae en el tiempo 2, no en el downbeat), UPPERS en 0.0
> y 1.0 más la síncopa en 2.5 sostenida hasta el fin de ciclo, sin ataque en el tiempo 3.
> `arpeggioRate` se ignora. **Autorar `Bossa` esperando ska, o `BassUpperSplit` esperando
> bossa, es el error por defecto de esta familia.**

**El bajo es articulation-only por construcción.** `BasslineCardConfigSO` no tiene canal de
progresión en absoluto — solo `chordExpression`, `arpeggioRate`, `randomFigureWeights`,
`randomRerollChance`, `velocityJitter`. El bajo siempre lee la progresión compartida, y Dev
Mode veta de plano los overrides de patrón de bajo (*"bass renders the shared progression;
override Backing instead"*).

**Slap vs finger son dos cosas a la vez:** el timbre viene del `InstrumentEffect`
(`InstrumentEffect_SlapBass`, hoy `RandomFromList {SB1, SB2}`); la figura viene del bundle.
Una carta de slap convincente suele llevar ambos. Preferir `PocketCouplingMode.SelfPocket`
para cartas nuevas: es autónomo y **no** arma el deber de caché de la frontera §8.4 que sí
arma `SlapPocket`.

**Desequilibrio conocido (CONT-B, D9):** Fingered mide por debajo de Slap, y la causa es
*payload, no instrumento* — los boosts de Pocket son exclusivos de slap/pop. Contar con esa
diferencia al autorar el par; no es un defecto nuevo.

## 6. Backwards compatibility policy

WAUC-style rule:
- do not preserve legacy schema branches forever
- if old content needs migration, provide a **one-time conversion path**
- do not let the long-term importer silently support multiple conflicting shapes indefinitely

Practical implication:
- legacy `dto.action.actions`-style content should be converted, not normalized forever inside the active pipeline

---

## 7. Staged CardEditorWindow workflow invariants

The canonical editor workflow is:

```text
Parse -> Stage in memory -> Review/Edit -> Save -> Add to catalog
```

### 7.1 Stage invariants
- staged objects are temporary/in-memory
- nothing writes to disk before Save
- temporary objects should not persist as accidental assets
- each staged card slot carries a per-entry `count` (M4.4); count is editable inline via the Deck Editor's `−` / `+` controls and round-trips through the JSON `count` field

### 7.2 Save invariants
Save must:
- create the `.asset` files
- wire payload/effect references correctly
- insert the resulting card into the intended catalog or registry

### 7.3 Effects editing rule
The editor must edit the authoritative `effects` collection directly.

Capabilities expected:
- add/remove/reorder effect specs
- show type-appropriate UI per spec
- allow human-friendly status picking while storing the asset reference

### 7.4 Create wizard defaults

The manual "Create Card + Payload" wizard resets `Kind` to `Action` each time it is opened. This prevents stale serialized state from defaulting to Composition after previous use. The user can switch to Composition during the session.

---

## 8. Validation rules

Minimum governed validations include:
- duplicate card id detection
- duplicate `statusKey` detection
- unresolved status references are hard failures
- required top-level fields must exist before Save/import succeeds
- catalog insertion must not silently create conflicting entries

---

## 9. Extension rule for new effect types

Whenever a new `CardEffectSpec` subclass is added, update all four layers:
1. data class
2. editor authoring support
3. JSON/import support
4. runtime execution support

A new effect type is not fully integrated until all four exist or the missing pieces are explicitly documented.

**DF-INSPLOOP (2026-07-16) — conformant.** `AddInspirationPerLoopSpec` satisfies all four layers: data class (`Cards/Effects/AddInspirationPerLoopSpec.cs`), editor authoring (`CardEditorWindow` add-menu + generic field render + `BuildEffectLabel`; DeckEditor `DeckCardCreationService` branch), JSON/import (`CardEditorWindow.JsonImport` + `CardImportDtos` discriminator; LLM path `CardLLMPromptBuilder` vocabulary + `CardLLMResponseHandler` validation), and runtime execution — the runtime layer is **track-binding** (`CompositionSession.EvalPerLoopInsp` via `TrackEntry.sourceCardDefinition`), not the `CardBase` effect pipeline (defensive no-op branch only).

**R4 (2026-08-10) — conformant: `RevealPreferencesSpec`.** All four layers:

| Layer | Site |
|---|---|
| 1 · data | `Assets/Scripts/Cards/Effects/RevealPreferencesSpec.cs` |
| 2 · editor | `CardEditorWindow` add-menu + `BuildEffectLabel`; `DeckCardCreationService` import branch |
| 3 · import | `CardEditorWindow.JsonImport` branch + `CardImportDtos` discriminator; LLM via `CardLLMPromptBuilder` + `CardLLMResponseHandler` |
| 4 · runtime | `CardBase.ExecuteEffects` branch **+ the two targeting derivations** |

**New normative, transferable note:** the layers are four, but layer 4 has **two targeting sites** that until R4 only `ApplyStatusEffectSpec` knew about: `CardDefinition.RequiresTargetSelection` and `HandController.TryResolveCardTarget`. A single-target spec that does not update both produces a playable card **with no hover** that resolves an empty target list and **silently no-ops** — the worst possible failure mode, because there is no error. Every future spec with a single-target `targetType` must touch both.

**Import hardening (2026-07-16).** `ApplyCompositionJson` previously assigned a null `styleBundle` **silently** when a `trackAction.styleBundle` path failed to resolve; a null-bundle Track card is augment-only and will not create a track (D4=A), which surfaced as a "card played, no track created" bug during DF-INSPLOOP validation. The importer now logs a warning when a non-empty `styleBundle` path resolves to null. Authoring guidance: assigning the bundle by drag in the Inspector is the reliable path; the JSON path is a convenience that requires exact case/spacing.

---

## 10. Relationship to other docs

- `SSoT_Card_System.md` owns gameplay/runtime card meaning
- `SSoT_Gig_Combat_Core.md` owns combat economy/phase semantics
- a future status SSoT will own deeper runtime status semantics

This document owns the **authoring and data-contract side** only.

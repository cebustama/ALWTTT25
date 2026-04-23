# SSoT_Status_Effects — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Runtime status semantics, catalogue boundary, and canonical MVP status meaning  
**Owns:** what a status is in ALWTTT, how status identity/theme are split, runtime container semantics, tick timing system, icon presentation authority, and the current canonical status set  
**Does not own:** full card-import/editor workflow (`systems/SSoT_Card_Authoring_Contracts.md`), encounter structure (`systems/SSoT_Gig_Encounter.md`), raw primitive catalog reference (`reference/CSO_Primitives_Catalog.md`)

---

## 1. Purpose

This document is the primary authority for **status-effect truth in ALWTTT**.

It replaces the previous mixed role of:
- `reference/StatusEffects.md`
- status sections embedded in card/combat notes
- primitive-only catalogs that were reference material rather than live authority

---

## 2. Core model

A status in ALWTTT has two layers:

### 2.1 Primitive / stable identity layer
The stable systemic identity of a status must not depend on theme text.

This layer exists so that:
- code/runtime can reason about status meaning,
- serialization/import stays stable,
- different themed assets can still map to the same underlying systemic primitive.

### 2.2 Authored/theme layer
The themed, display-facing, balance-facing representation of a status lives in authored assets such as `StatusEffectSO`.

This layer may control:
- display name
- presentation (including icon sprite — see §3.3)
- tuning/default values
- duration/stacking metadata
- variant-specific authoring choices

Rule:
- stable identity and themed presentation must not be conflated.

---

## 3. Runtime ownership

The runtime status surface includes:
- `StatusEffectSO`
- `StatusEffectCatalogueSO`
- runtime containers / instances (`StatusEffectContainer` on `CharacterBase.Statuses`)
- catalogue lookup and variant selection by key
- stack application / decay / refresh according to the authored contract
- icon presentation (sprite authority on `StatusEffectSO`, event-driven rendering on `CharacterCanvas`)

This SSoT owns the gameplay/runtime meaning of that surface.

### 3.1 Tick timing system

Status decay/expiry is driven by a `TickTiming` enum on `StatusEffectSO`. The canonical values in current use:

| Value | Enum name | Meaning |
|---|---|---|
| 1 | `StartOfTurn` | Legacy value — retained for back-compatibility only. Do not use for new statuses. |
| 8 | `PlayerTurnStart` | Ticks at the start of the Player Turn (`GigManager.OnPlayerTurnStarted`) |
| 9 | `AudienceTurnStart` | Ticks at the start of the Audience Turn (`GigManager.AudienceTurnRoutine`) |

**Wiring:**
- Musicians: `TriggerAllStatus` is bound to `GigManager.OnPlayerTurnStarted` via `MusicianBase.BindToGigContext()`. Called at Player Turn start for each musician.
- Audience: `TriggerAllStatus` is called from `GigManager.AudienceTurnRoutine` at audience turn start.

Use `PlayerTurnStart` or `AudienceTurnStart` for all new statuses.

### 3.2 Dual status system (migration note)

The codebase currently has two coexisting status surfaces:

1. **Legacy:** `StatusType` enum + `BandCharacterStats.ApplyStatus(StatusType, int)` — retained as `[Obsolete]` coexistence for any remaining non-icon legacy callers. Not the primary authority. The icon path no longer touches this layer as of M1.2.
2. **Current:** `StatusEffectSO` + `StatusEffectContainer` (`CharacterBase.Statuses`) + `StatusEffectCatalogueSO` (`CharacterBase.StatusCatalogue`).

The current runtime model (2) is the governed model. All new statuses must go through the SO + container route. Legacy calls that remain are migration coexistence, not the primary path.

**M1.2 (2026-04-14) completed the icon pipeline migration.** Legacy icon calls removed from `MusicianBase.OnBreakdown` (previously called `ApplyStatus(StatusType.Breakdown, 1)` for UI purposes) and from `AudienceCharacterBase.IsBlocked` setter (previously called `ApplyStatus(StatusType.Blocked, 1)`). The blocked visual is now sprite tint only; see M1.2 closure notes for Decision E3.

### 3.3 Icon presentation authority

Icon sprite authority lives on `StatusEffectSO.IconSprite`. Each StatusEffectSO carries its own icon directly.

**Rendering path:**
1. A status is applied via `CharacterBase.Statuses.Apply(StatusEffectSO, stacks)`.
2. `StatusEffectContainer` fires `OnStatusApplied(CharacterStatusId, deltaStacks)` / `OnStatusChanged(CharacterStatusId, newStacks)` / `OnStatusCleared(CharacterStatusId)`.
3. `CharacterCanvas` (subscribed via `BindStatusContainer`) resolves the active `StatusEffectInstance.Definition` from the container and reads `Definition.IconSprite`.
4. `CharacterCanvas` instantiates a `StatusIconBase` prefab under `statusIconRoot`, assigns the sprite, and updates stack count text on every change.

**Key design decisions (M1.2):**
- No lookup table asset. The former `StatusIconsData` / `StatusIconData` layer was removed; it added indirection without value once the SO owned the sprite.
- `StatusIconBase` prefab is assigned directly on the `CharacterCanvas` component (`statusIconBasePrefab` field). Not configured via a separate container asset.
- Icon display is lazy: icons are created on the first status application and destroyed on status clear. No pre-population of a fixed icon set.
- Missing sprite → warning log in `CharacterCanvas.TryCreateIcon`. No silent failure.
- Missing prefab reference on the canvas → warning log. No silent failure.

**Boundary with `CharacterStats`:**
`CharacterStats` no longer wires delegates to `CharacterCanvas.ApplyStatus/UpdateStatusText/ClearStatus`. Those methods have been removed from `CharacterCanvas`. Icon display is end-to-end event-driven from the SO container, not from the legacy `StatusType` path.

**Wiring points:**
- `MusicianBase.BuildCharacter()` calls `bandCharacterCanvas.BindStatusContainer(Statuses)` after stats construction.
- `AudienceCharacterBase.BuildCharacter()` calls `AudienceCharacterCanvas.BindStatusContainer(Statuses)` after stats construction.
- Both bind to `CharacterBase.Statuses`, which is created in `CharacterBase.Awake()`.

**Status tooltip content (M1.3a + M1.3c, 2026-04-23):**

`StatusEffectSO` carries a `description` field (`[TextArea(2, 4)]`, public getter `Description`) authored per status. Description text is the single source for tooltip body text. `DisplayName` is the single source for tooltip header text.

Two runtime hosts surface status tooltips:

1. **Per-icon hover (M1.3a):** `StatusIconBase` implements `IPointerEnter/ExitHandler`. `CharacterCanvas.TryCreateIcon` calls `BindTooltipSource(StatusEffectSO, StatusEffectContainer, CharacterStatusId)` immediately after `SetStatus`. Hovering a status icon shows `{DisplayName}` (or `{DisplayName} ×N` when stacks > 1) as header and `Description` as body, via `TooltipManager.ShowTooltip`.

2. **Card-hover extraction (M1.3c):** `CardBase.ShowTooltipInfo()` iterates `CardDefinition.Keywords` (resolved against `TooltipManager.SpecialKeywordData`) then extracts unique `StatusEffectSO` references from `CardDefinition.Payload.Effects` filtered to `ApplyStatusEffectSpec.status`. Dedupe via `HashSet<StatusEffectSO>`. Display order: keywords first, statuses second. Each unique SO produces one `ShowTooltip` call with `DisplayName` header + `Description` body. Tooltip follows the mouse cursor (no static anchor).

`CardBase` is the assembly point for card-hover tooltips but does not own the data — `StatusEffectSO` owns description text, `SpecialKeywordData` owns keyword text.

---

## 4. Catalogue and variants

The catalogue-facing contract exists so cards and effects can apply statuses without hardcoding one-off logic.

Canonical rule:
- cards reference a direct `StatusEffectSO` asset in `ApplyStatusEffectSpec.status` (resolved at design time)
- runtime code (e.g. `OnBreakdown`) resolves by string key at runtime via `StatusEffectCatalogueSO.TryGetByKey(key, out so)`
- variants may share a primitive but differ in authored tuning or presentation

`StatusEffectCatalogueSO` keys are case-insensitive and trimmed. Duplicate keys within one catalogue are a hard error (flagged in `OnValidate`).

**M1.2 catalogue validation fix:** `StatusEffectCatalogueSO.OnValidate` now defers deep validation via `EditorApplication.delayCall` and skips entirely during import-worker runs (`AssetDatabase.IsAssetImportWorkerProcess`). This eliminates spurious "empty StatusKey" errors that previously fired when selecting prefabs that reference the catalogue, caused by a serialization-order race during asset import.

**M1.2 asset hygiene:** `StatusEffectSO` auto-renames its asset file to `StatusEffect_{DisplayName}_{EffectId}` whenever `DisplayName` or `EffectId` changes. The rename is deferred to `EditorApplication.delayCall` since `AssetDatabase.RenameAsset` is illegal inside `OnValidate`. Collisions and import-worker runs are handled defensively.

The `StatusCatalogue` field on `CharacterBase` is Inspector-assigned and optional for card play, but **required** for any runtime code that resolves statuses by key (e.g. `MusicianBase.OnBreakdown` applying Shaken). Musician prefabs must have the catalogue assigned for Shaken application to function.

---

## 5. Canonical MVP status set

### 5.1 Flow
**Primitive:** `DamageUpFlat` (`CharacterStatusId = 100`)  
**Key:** `"flow"`  
**Scope:** Song / Band  
**Tick timing:** Not tick-decayed per turn. Resets at song end via explicit `GigManager` song-end reset.  
**Combat meaning:** amplifies positive Loop → SongHype conversion multiplicatively. Each stack adds `FlowMultiplier` to the multiplier.  
**Applies to:** musicians  
**Validated:** ✅ B3 (Flow stacks boost Vibe per card play), B7 (song-end reset)

### 5.2 Composure
**Primitive:** `TempShieldTurn` (`CharacterStatusId = 400`)  
**Key:** `"composure"`  
**Scope:** Musician  
**Tick timing:** `PlayerTurnStart` — clears at the start of each Player Turn  
**Combat meaning:** absorbs incoming positive Stress before Stress is applied. Consumed first; remainder becomes Stress.  
**Applies to:** musicians  
**Validated:** ✅ B1 (absorbs Stress), B6 (clears at turn start)

### 5.3 Choke
**Primitive:** `DisableActions` (`CharacterStatusId = 700`)  
**Key:** `"choke"`  
**Scope:** Musician  
**Tick timing:** `PlayerTurnStart` — decays each Player Turn start  
**Combat meaning:** stuns the musician (disables actions). `CharacterBase.IsStunned` derives from `DisableActions` stacks when the runtime container is present.  
**Applies to:** musicians only. No audience crowd-control status exists in MVP.  
**Validated:** ✅ B5 (stacks decay after turn)

**Design decision — Choke on stunned target (2026-04-20):**  
`HandController.TryResolveCardTarget` refuses to target a stunned musician, so Choke cannot be re-applied while the target is already stunned (`DisableActions` active). This is intentional for MVP: stun is binary (the status is either present or not), and Choke stacks represent decay duration, not additive stun strength. Re-applying Choke to an already-stunned musician is redundant under current encounter pacing.

If future encounter design requires extending stun via additional Choke stacks, one of the following must change: (a) `TryResolveCardTarget` relaxes the stunned-target refusal for Choke specifically, or (b) Choke's stacking semantics are reinterpreted so that additional stacks add duration beyond the initial trigger. Neither is in MVP scope. Revisit when audience pressure or encounter-length tuning makes prolonged stun valuable.

### 5.4 Shaken
**Primitive:** `ShakenRestriction` (`CharacterStatusId = 503`)  
**Key:** `"shaken"`  
**Scope:** Musician  
**SO config:** Replace, MaxStacks=1, LinearStacks, `AudienceTurnStart` tick, IsBuff=true  
**Tick timing:** `AudienceTurnStart` — expires at the start of the Audience Turn of the following song  
**Duration:** Applied at Audience Turn of Song N → active through Player Turn N+1, Composition N+1, Performance N+1, Song End N+1 → expires at start of Audience Turn N+1. One complete song cycle from application.  
**Combat meaning:** marks a musician as shaken post-Breakdown  
**Applied by:** `MusicianBase.OnBreakdown()` via `StatusCatalogue.TryGetByKey("shaken")`  
**Gameplay restrictions:** open design decision — **not yet enforced in runtime**. Intended restrictions (cannot play Action cards during action window while Shaken; Composure granted is reduced by 50%) are a pending follow-up pass.

### 5.5 Exposed
**Primitive:** `DamageTakenUpFlat` (`CharacterStatusId = 300`)  
**Key:** `"exposed"`  
**Scope:** Musician  
**Tick timing:** not specified — decays per configured SO  
**Combat meaning:** each Exposed stack adds `0.25` to the incoming stress multiplier in `BandCharacterStats.ApplyIncomingStressWithComposure` (`_exposedMultiplierPerStack = 0.25f`).  
**Applies to:** musicians only. No Stress path exists on `AudienceCharacterBase`.

### 5.6 Feedback
**Primitive:** `DamageOverTime` (`CharacterStatusId = 600`)  
**Key:** `"feedback"`  
**Scope:** Musician (MVP); Audience deferred  
**SO config:** `DecayMode = LinearStacks`, `TickTiming = PlayerTurnStart`, `StackMode = Additive`, `MaxStacks = 999`  
**Decay timing:** stacks decay by 1 at the start of each Player Turn, via `StatusEffectContainer.Tick(PlayerTurnStart)` invoked by `GigManager.OnPlayerTurnStarted` → `TriggerAllStatus` on musicians.  
**Damage resolution:** each active Feedback stack applies 1 incoming stress during `GigManager.AudienceTurnRoutine`, routed through `BandCharacterStats.ApplyIncomingStressWithComposure`. Damage respects Composure and can trigger Breakdown.  
**Poison-like semantics:** damage is applied during the audience turn using the current stack count; decay occurs at the start of the following player turn, so the first audience turn after application deals full-stack damage before any decay. Total damage over the full decay of N initial stacks is `N(N+1)/2` — e.g. 3 stacks → 3 + 2 + 1 = **6 total damage** over 3 audience turns.  
**Applies to:** musicians only in current implementation. Audience Feedback DoT requires a Stress path on `AudienceCharacterBase`, which does not exist. Explicitly deferred.  
**Validation history:** Phase 2 test T8 (2026-04-17) observed stacks persisting turn-to-turn with no decay. Root cause identified 2026-04-20: the Feedback SO had `TickTiming = EndOfTurn` configured, which is declared in the enum but not invoked by the runtime phase machine (only `PlayerTurnStart` and `AudienceTurnStart` are wired — see §3.1). Fixed by changing Tick Timing to `PlayerTurnStart`. Post-fix smoke test validated the `N(N+1)/2` damage curve and icon clear-on-zero.

---

## 6. Stacks, duration, and lifecycle

Status behavior may vary by authored configuration, but the following invariants hold:

- statuses may be stack-based
- duration/expiry must be explicit in runtime or authoring semantics
- a status application path must resolve through one canonical runtime route
- multiple variants must not become multiple silent meanings for the same gameplay concept

If a new status changes how stacking/expiry works system-wide, update this SSoT and the changelog.

---

## 7. Relationship with cards

Cards apply statuses through declarative effect specs.
That means:
- card gameplay meaning belongs in `SSoT_Card_System.md`
- import/editor schema belongs in `SSoT_Card_Authoring_Contracts.md`
- status meaning itself belongs here

This split is non-negotiable.

---

## 8. Relationship with the CSO primitive catalog

`reference/CSO_Primitives_Catalog.md` is useful, but it is not the live runtime authority.

Use the split like this:
- this doc = what statuses mean and how the runtime/status catalogue surface works now
- reference catalog = explanatory primitive catalog, examples, and broader ontology support

If they conflict, this doc wins for current ALWTTT status truth.

---

## 9. MVP governance rules

- do not let themed display names become primary identity
- do not scatter status semantics across card docs, combat docs, and reference notes
- keep one canonical application path for runtime status application
- keep variants explicit through the catalogue/status key route
- document new cross-cutting status semantics here before treating them as done

---

## 10. Update rule

Update this document when a change affects:
- status identity rules
- catalogue/variant semantics
- runtime container semantics
- tick timing system
- canonical status meanings
- system-wide stack/duration/expiry behavior
- icon presentation authority or rendering path

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

**One instance per primitive and holder (code truth).** The container indexes by
`CharacterStatusId`, not by `StatusKey` (`StatusEffectContainer._active` is a
`Dictionary<CharacterStatusId, StatusEffectInstance>`). Two variants authored on the same
primitive **do not coexist** on the same character: the second application writes over the
first one's instance. Two rules already in use follow from this: (a) the dual guard on
`StatusKey` in every consumer, and (b) every semantically new status asks for its own
primitive (append-only), even when an existing one seems to "fit". *(Verified in R5-inv,
2026-08-11 — D6.)*

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

### 3.0 Container contract — explicit resource spend (`SpendStacks`, R5-c)

`StatusEffectContainer.SpendStacks(CharacterStatusId id, int n) → int` — explicit resource
spend for counter statuses (`DecayMode.None`). It does **not** require
`DecayMode.ConsumeOnTrigger`; the `ConsumeOnTrigger` guard is untouched. It spends
`min(stacks, n)`, fires `OnStatusChanged` / `OnStatusCleared`, and **does not** fire
`OnStatusApplied` nor publish `StatusAppliedEvent` — spending a resource is not applying a
status. It returns the stacks actually spent, so the caller can tell "I spent" from "there was
nothing there" without re-reading the container. First consumer: Overload (R5-c, §5.10).

Why this is its own API rather than a reuse: `ConsumeOnTrigger` guards on
`Definition.Decay == ConsumeOnTrigger`, so on a `DecayMode.None` status it is a **silent
no-op** — the consumer would get its effect without paying (F-R5c-1). And `Apply(-n)` would
publish a `StatusAppliedEvent` with a negative delta, i.e. a false statement on the sensory
bus. (D-R5-18=C.)

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

**S5f addendum (2026-07-04):** the Blocked tint now carries a textual legend — hovering the tinted sprite shows a "Bloqueado" tooltip (`AudienceCharacterCanvas.ShowBlockedTooltip`/`HideBlockedTooltip`, wired from `AudienceCharacterBase.OnPointerEnter`/`OnPointerExit`). Still no status icon; Decision E3 intact. ESP copy hardcoded per D-S5f-7=A; migrates to the localization structure in S5f-ext. (ST-S5f-R1..R3 PASS.)

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
**Combat meaning (M4.2, 2026-04-28):** amplifies positive Vibe gains, bifurcated by card domain. Action cards: flat bonus using the performer's individual Flow stacks (`finalΔ = baseΔ + performerFlow × bonusPerStack`). Composition cards + Song End: multiplier using band-wide Flow stacks (`finalΔ = round(baseΔ × (1 + bandFlow × flowVibeMultiplier))`). The previously-documented Flow → SongHype multiplicative path has been retired and removed from code.  
**Applies to:** musicians  
**Validated:** ✅ B3 (Flow→Vibe per card play), B7 (song-end reset), ST-M42-1/1c (Action flat + per-performer), ST-M42-3 (Song End multiplier), ST-M42-10 (tuneable thresholds)

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

### 5.7 Earworm
**Primitive:** `DamageOverTime`  
**Key:** `"earworm"`  
**Scope:** single Audience member  
**Tick timing:** `AudienceTurnStart`  
**SO config:** `StackMode = Additive`, `DecayMode = LinearStacks`, `MaxStacks = 99`, `IsBuff = false`, `ValueType = Flat`  
**Combat meaning:** at each `AudienceTurnStart`, the affected audience member gains `+N Vibe` where `N` is the current stack count, **then** the container's `Tick(AudienceTurnStart)` decays stacks by 1. The runtime ordering — read-then-decay within the same method — is enforced by `GigManager.AudienceTurnRoutine`: the Earworm read-and-apply loop runs *before* the audience `Tick(AudienceTurnStart)` loop. Inverting that ordering would undercount Vibe by 1 stack.

**Poison-like semantics on the Vibe path:** Vibe is gained using the current stack count before decay. Total Vibe gain over the full decay of N initial stacks is `N(N+1)/2`. Example: Earworm `+3` → audience gains `+3`, `+2`, `+1` on three successive audience turns, total `+6` Vibe.

**Applies to:** audience members only. `StatusEffectCatalogue_Musicians` does not contain Earworm; the runtime hook iterates `CurrentAudienceCharacterList` only. Authoring a card that targets musicians with Earworm is silently a no-op at runtime.

**Interaction with Flow:** none. Earworm ticks do not pass through the `ModifyVibeSpec` Flow-bifurcation pipeline; the tick amount is the raw stack count. This parallels the musician-side Feedback DoT, which also bypasses Stress modifiers on its tick path. Validated against ST-M43-7.

**Routing correction (B3, 2026-05-18 — recorded at R1):** the tick no longer calls `AddVibe(stacks)` directly. `GigManager.AudienceTurnRoutine` routes it through `AudienceCharacterStats.ApplyIncomingVibe`, so Earworm ticks are Indifference-blockable and — since R1 — Captivated-amplifiable (§5.8). Flow remains excluded. Read-then-decay ordering is unchanged.

**Interaction with `IsBlocked`:** Blocked audiences are skipped for Vibe gain (consistent with `ComputeSongVibeDeltas`). Stack decay continues normally on the same audience turn.

**AoE application and `IsBlocked` (R4 — behavior, not a bug):** on AoE application, the `AllAudienceCharacters` branch of `CardBase.DetermineTargets` excludes `IsBlocked` members, so audience blocked by positional Indifference receive **no Earworm** (and no Vibe) from Psychic Wave v2. Verified ST-R4-2. Precision to avoid a misreading: an **unblocked** member carrying Indifference as a *status* does receive Earworm; what gets blocked afterwards are its ticks, at the `ApplyIncomingVibe` gate (see the routing correction above).

**Interaction with `IsConvinced`:** ticks are harmless. `AddVibe` clamps at `MaxVibe`; `CheckConvincedThreshold`'s `!IsConvinced` guard prevents `OnConvinced` re-firing. The icon may linger on a Convinced audience until natural expiry — known cosmetic, deferred to UI polish.

**Variant relationship with Feedback:** Earworm and Feedback share the `DamageOverTime` primitive but live in separate catalogues post-MB2. Both are marked `IsDefaultVariant = true` in their respective catalogues. The runtime hook in `GigManager.AudienceTurnRoutine` disambiguates by `StatusKey == "earworm"` for defensive isolation against future audience-side `DamageOverTime` variants.

**Validation history:** ST-M43-1a/1b/2/3/4/5/6/7/8 all PASS 2026-04-28 (M4.3 closure). Initial implementation shipped with a copy-paste duplicate `Tick(AudienceTurnStart)` block in `AudienceTurnRoutine` producing -2/turn decay; caught by stack-count observation in ST-M43-2/3; resolved by deletion of the duplicate block before closure.

**Applied by:** Mind Tap (M4.6 — pending) and **Psychic Wave v2** (R4, 2026-08-10 — `ApplyStatusEffect(earworm, +2, AllAudienceCharacters)` on top of the AoE `ModifyVibe`). `Assets/Resources/Data/Characters/Musicians/{Musician}_Cards/TestEarworm.asset` retained as a dev-only regression card (`[TEST]` prefix on DisplayName, `inspirationCost: 0`, `actionTiming: Always`).

### 5.8 Captivated
**Primitive:** `DamageTakenUpMultiplier` (`CharacterStatusId = 301`)
**Key:** `"captivated"`
**Scope:** single Audience member
**Tick timing:** `AudienceTurnStart`
**SO config:** `StackMode = Additive`, `DecayMode = LinearStacks`, `MaxStacks = 5`, `IsBuff = false`, `ValueType = Flat`
**Catalogue:** `StatusEffectCatalogue_Audience`, `IsDefaultVariant = true` for the primitive on the audience side.

**Combat meaning:** while the holder has `N > 0` stacks, incoming positive Vibe is amplified to `round(incoming × (1 + N × CaptivatedVibeBonusPerStack))`. Initial tuning `0.25` → 2 stacks = ×1.5. The amplification is computed inside `AudienceCharacterStats.ApplyIncomingVibe`, immediately after the Indifference gate. Fantasía: they're locked in; every push lands harder.

**Scope of amplification (R1, D-R1-1=A):** helper-wide. Every source that routes through `ApplyIncomingVibe` is amplified — card `ModifyVibeSpec` positives, `AddVibeAction`, Earworm ticks, the SFX→FlatVibe stage bonus, and the song-end macro conversion. This is broader than the original design wording (`planning/Design_Audience_Status_v1.md §4.2` scoped it to "`ModifyVibeSpec` positive"); the broadening is deliberate and follows the single-canonical-entry-point architecture Indifference already relies on. Negative-Vibe paths (`RemoveVibe`, negative `ModifyVibeSpec`) are **not** amplified — they do not route through the helper (§5.7 precedent; same documented limitation as Indifference).

**Interaction with Indifference (D-DCP-6=A invariant, preserved):** Indifference is checked first and returns 0 unconditionally. A holder who is both Indifferent and Captivated takes 0 Vibe regardless of stack count, and no CAPTIVATED log line is emitted. Validated ST-R1-4.

**Interaction with Flow:** compounding, not merged. Flow's flat bonus / multiplier is applied upstream in `CardBase.ExecuteEffects` (band-wide song-end multiplier in `GigManager`), producing an already-rounded `finalDelta`; Captivated then multiplies that value and rounds again. Two rounding steps, in that order.

**Rounding:** `Mathf.RoundToInt` (banker's rounding — `.5` → even). Same convention as the Flow multiplier path. Pinned by ST-R1-2: incoming 5 at 2 stacks → `7.5` → **8**.

**Tuning home:** `MeterTuningSO.captivatedVibeBonusPerStack` (Inspector), surfaced as `GigManager.CaptivatedVibeBonusPerStack`; `AudienceCharacterStats.DefaultCaptivatedBonusPerStack = 0.25f` is the fallback when no `GigManager` is present (tests, detached construction). Mirrors the Flow→Vibe tuning pattern (`SSoT_Scoring_and_Meters.md §7.1`).

**Variant disambiguation:** the runtime guards on `StatusKey == "captivated"` in addition to the primitive id, mirroring the Earworm `DamageOverTime` guard, so a future audience-side `DamageTakenUpMultiplier` variant cannot silently inherit the amplification.

**Decay:** generic. `StatusEffectContainer.Tick(AudienceTurnStart)` handles `LinearStacks` decay; R1 added **no** `GigManager` code. Icon clears at 0 stacks. Validated ST-R1-3.

**Applies to:** audience members only. `StatusEffectCatalogue_Musicians` does not contain Captivated, and nothing musician-side reads `DamageTakenUpMultiplier` on a Vibe path.

**Applied by:** **Wink** (Zig, `Cantante_CardCatalogData`; Action, cost 0, `ApplyStatusEffect(captivated, +2, AudienceCharacter)`). Authored R1 and deliberately **unreachable in the demo build** — the Cantante catalog is outside the demo band roster, and both `PersistentGameplayData.SetBandDeckFromMusicians` and `BuildRewardCardPool` are band-scoped. `Singalong` (Captivated +1 AoE, D-R0-9) is queued for R8.

**Validation history:** ST-R1-1..6 all PASS 2026-07-23 (R1 closure). ST-R1-2 pinned the rounding; ST-R1-4 pinned Indifference precedence; ST-R1-5 pinned the helper-wide scope (Earworm 2 stacks → +3 applied); ST-R1-6 pinned demo-inertness (starter deck and reward pool unchanged from the S5i baseline).

### 5.9 Spotlight
**Primitive:** `RedirectIncoming` (`CharacterStatusId = 504`, Control range; 404 is taken by `NegateIncomingPositive` — no collision)
**Key:** `"spotlight"`. `IsDefaultVariant = true`.
**Scope:** single Musician (C2 finisher carrier)
**Tick timing:** `PlayerTurnStart`
**SO config:** `StackMode = Replace`, `MaxStacks = 1`, `DecayMode = LinearStacks`, `TickTiming = PlayerTurnStart`
**Catalogue:** `StatusEffectCatalogue_Musicians`

**Combat meaning:** while the holder has `stacks > 0`, audience-side single-target hostile targeting (`ActionTargetType.Musician` and `RandomMusician` in `AudienceCharacterBase.ResolveTargetsFor`) is redirected to the holder.

**Explicit rule — `AllMusicians` is NOT redirected.** An AoE already includes the holder; redirecting it would collapse the ability to single-target, which is substituting the ability, not taunting it. Consequently no `SpotlightRedirectEvent` can fire for `AllMusicians` either: the invariant covers both targeting and its presentation. Regressed by ST-PRES1-9.

**Dual guard:** the runtime checks the primitive **and** `StatusKey == "spotlight"` (Earworm/Captivated precedent, §2.1), so a future `RedirectIncoming` variant does not inherit the taunt by accident.

**Lifecycle:** applied on Player Turn N (whose tick has already passed), survives Audience Turn N, decays at the opening of Player Turn N+1 ⇒ "1 audience turn" with no bespoke expiry code. Same cycle as Composure (§5.2).

**Legibility (D-R4-8, closed at PRES-1 · D-PRES1-2=A):** a redirect publishes `SpotlightRedirectEvent` (presentation-only, sensory bus) and `SensoryFxAdapter` draws a gold floater.
- `Musician` branch → `"-> {protected}"` anchored on the ORIGINAL target, named by the same pure selector the normal path uses (`SelectDefaultMusicianTarget`), so the floater cannot contradict the game's own choice.
- `RandomMusician` branch → `"¡Foco!"` anchored on the PROTECTED musician. The would-be target is genuinely indeterminate there: naming one would require rolling `Random.Range`, consuming global RNG state and shifting every later roll in the gig. A presentation path must never do that.
- No-op suppression: when the default target already WAS the spotlit musician, no event is published ("→ himself" is noise, not information).

Publishing never alters targeting. The suppression is logged (`redirect SUPPRESSED (visual no-op)`), so a correctly-suppressed redirect is distinguishable from a broken presentation path. That log is load-bearing for ST-PRES1-6 and must not be pruned as debug noise.

> **Selector note (R5, 2026-08-11 — D-R5-2=A).** `SelectDefaultMusicianTarget` picks the musician **closest to Breakdown** (lowest remaining fortitude under the S5e inverted meter). Until R5 it picked the highest `CurrentStress`, i.e. the healthiest — the comparator read `CurrentStress` raw, outside the direction-agnostic API S5e protected, and was silently inverted by the storage flip (finding F-PRES1b-1). Spotlight's value as a taunt depends on this: redirecting hits that were already landing on the healthiest musician protects nothing.

**Applied by:** **Spotlight** (C2 finisher, starter v2 row 6, cost 2). Authored and validated at R4 (2026-08-10, ST-R4 suite).

### 5.10 Voltage
**Primitive:** `ResourceCounter` (`CharacterStatusId = 993`, Meta range)
**Key:** `"voltage"`. `IsDefaultVariant = true`.
**Scope:** single Musician (Conito).
**Tick timing:** `None`
**SO config:** `StackMode = Additive`, `MaxStacks = 9`, `DecayMode = None`,
`DurationTurns = 0`, `ValueType = Flat`, `IsBuff = true`
**Catalogue:** `StatusEffectCatalogue_Musicians`

**Combat meaning:** a pure counter. It has no intrinsic effect: its meaning comes from its
consumer. Sole consumer as of R5-c: **Overload (passive)** — see the consumption rule below.

**Generation (D-R5-5=A · D-R5-9=A · D-R5-10=A).** Every play **genuinely consumed** by Conito
generates **+1 Voltage**, action or composition, regardless of inspiration cost (including cost
0). The hook lives in the consumed branch of `GigManager.TryConsumePlay` (`if (ok)`), not in its
callers: the trigger condition and the budget consumption are the same control-flow fact and
cannot be allowed to diverge. Generation is restricted to Conito by musician identity
(`MusicianCharacterData.CharacterType`) — there is no authorable marker; generalizing is a
one-line change at a single seam.

What does **not** generate Voltage: a play denied by the ECON-1 budget (`ok == false`); one
denied by timing, inspiration cost or target resolution (never reaches `TryConsumePlay`); a
composition drop **rejected by the session** (`GigManager` only calls `TryConsumePlay` when
`played == true`); and the no-attributable-musician branch (`musician == null`), which returns
`true` **without consuming** and exits before the hook.

Generation can be switched off with `GigFlowSettingsSO.GenerateVoltageOnConsumedPlay` (default
ON, read per play, hot-swappable in Play — D-R5-12=A). With the flag off, Voltage is applied
only by cards carrying an explicit `ApplyStatusEffectSpec` (*Amp Up*, R8, needs no new code).

**Generation follows attribution, not card authorship (D19).** A card with an `AnyMusician`
performer generates Voltage if the payer the play pipeline resolves (fixed → hover →
`SelectedMusician`, D-ECON-3=A) turns out to be Conito, even though it is not "a Conito card".
**Whoever pays the budget generates.** Conversely, a Conito card billed to another musician does
not generate. *(Verified as a non-failure in ST-R5b-4.)*

**Per-period ceiling.** ECON-1 grants 1 action play + 1 composition play per period and
musician, so pure generation contributes **at most +2 Voltage per period**. Under `MaxStacks 9`
reaching the cap takes ~5 periods.

**SO resolution:** by `StatusKey` from the **holder's** catalogue (`CharacterBase.StatusCatalogue`),
the `MusicianBase` / `"shaken"` pattern. The check that the primitive is `ResourceCounter` is a
defensive authoring tripwire, not the authority guard.

**Consumption — Overload auto-discharge (fallback, R5-c).** At the close of every composition loop, if the
Voltage holder has stacks ≥ `OverloadThreshold` (default 6), Overload discharges automatically:
it spends `OverloadCost` stacks (default 6, via `SpendStacks` — §3.0) and multiplies **that
loop's** contribution to SongHype by `OverloadHypeFactor` (default ×1.5). The factor is applied
to `hypeDelta` (after `ComputeHypeDelta` and after the encounter's `SongHypeDeltaMultiplier`),
only if the delta is positive (D-R5-19=B), and it is strictly local to one loop: no pending
state survives between loops. Surplus above the cost survives and keeps accumulating. At most
one discharge per boundary, whatever the roster. Switchable off with
`GigFlowSettingsSO.OverloadConsumerEnabled` (**default OFF** since R5-d, D-R5-20=B; read per loop). Every consumer keeps
the **dual guard**: primitive `ResourceCounter` **and** `StatusKey == "voltage"`, checked
against the live container instance — not against the catalogue: the catalogue says what the
holder *could* have, the container says what it has.

**Status after R5-d (D-R5-20=B).** This automatic discharge is **off by default** and is no
longer the player-facing Overload — the Action card is (see the consumption rule below). It
survives as a tuning/dev fallback behind `GigFlowSettingsSO.OverloadConsumerEnabled`. Two
consumers of one resource is a deliberate, bounded exception: with the flag off there is
exactly one Overload the player can observe. **Review trigger:** if the flag is still off at
R8, retire the consumer entirely (D-R5-20 option C). Note that flipping the field's default in
code does NOT change an already-serialized `GigFlowSettingsSO` asset — the live asset must be
edited by hand.

**Consumption — card resource cost (R5-d, D-R5-26=A).** The player-facing sink is a card
cost. `CardDefinition` carries a generic pair — `resourceCostStatusKey` (string) +
`resourceCostAmount` (int) — and **not** a `voltageCost` field: the container is keyed by
primitive while the variant is the authority, so a string key plus an int cover Voltage today
and any future character resource without migrating the asset again. Empty key or amount 0 ⇒
no cost, which is how every pre-R5-d card deserializes.

The cost is a **definition field, never a `CardEffectSpec`.** Effect specs run in
`CardBase.ExecuteEffects`, *after* the play is committed — inspiration paid, ECON-1 budget
consumed, animation fired. A cost living there would let the card be played with an empty
resource and then fail silently: card spent, effect not. The gate therefore mirrors
Inspiration's shape exactly.

Resolution and spend live on the host: `GigManager.CanPayResourceCost` /
`TryPayResourceCost`, keeping the **dual guard** (primitive `ResourceCounter` **and**
`StatusKey` match) checked against the live container instance. The spend goes through
`SpendStacks` (§3.0), not through a negative `Apply`. Both play paths gate before any spend;
the Action path spends after `TryConsumePlay` and the Composition path only on a
session-accepted drop. Order against the R5-b generation hook is net-neutral: a `+1` grant and
a `−N` spend both occur regardless of which runs first. The overlay surfaces a shortfall as
`UnplayableReason.Resource` (`SSoT_Card_System.md` §10.5).

First card: **Overload** (Action, Conito, inspiration 2, Voltage 3). D-R5-22 admits
composition cards as sinks with the same pair and no further code.

> **Scope note (F-R5c-4).** This passive Overload is **not** the Overload of the accepted R5
> scope. The accepted scope (D-R0-5=A, D-R0-12, D-R5-4=A) is a playable **Action-domain card**
> that grants a bonus loop with a Conito guitar solo over the base. That is **not built**; it
> remains R5 scope and continues in **R5-d**. What R5-c shipped is an additional layer on the
> same resource. See `RosterExpansion_Sub_Roadmap.md` §2 (substitution note) and §3 (R5 row).
>
> **Superseded 2026-08-26 (R5-d).** The Action card described above **is built**: code shipped
> at R5-d (`conito_overload`, `GrantBonusLoopSpec`, render-scope solo, duck plane). The note is
> kept because it records why two consumers of one resource coexist. Smoke ST-R5d-1..15 was
> **not yet run** at the time of this documentation pass.

**No decay, no reset — GIG scope (D-R5-8=A, verified 2026-08-21).** `DecayMode.None` means
`StatusEffectContainer.Tick` never alters the stacks at any boundary. Note that
`TickTiming.None` does **not** exclude the status from the tick loop (see §6): immunity to decay
comes from `DecayMode` alone. The song boundary does **not** clear it either:
`GigManager.ResetSongScopedStatuses` is an allowlist of two primitives (`DamageUpFlat` = Flow,
`TempShieldTurn` = Composure), not a sweep by category (see `SSoT_Gig_Combat_Core.md` §3.3.1).
Voltage survives turns, loops, parts and songs, bounded only by `MaxStacks = 9`. Voltage is
therefore a resource **bankable across the gig**, not a threat the song resets. The number of
stored charges follows the *card's* cost, which is per-card (D-R5-21): at cost 3 the cap holds
three, at cost 6 it holds one. Chaining is bounded structurally rather than by the resource —
`GigFlowSettingsSO.MaxBonusLoopsPerPart` (default 1) caps how many bonus loops one part may
take, so a full bank cannot turn a 4-loop part into a 7-loop one.

**Primitive occupancy.** `StatusEffectContainer._active` is keyed by primitive, not by
`StatusKey` (§2.1, D6). While Voltage is active on a holder, **no other `ResourceCounter`
variant can coexist on it**: a second variant would add stacks to the Voltage instance. Register
here any second variant authored in the future.

**Tuning home:** `GigFlowSettingsSO` — `generateVoltageOnConsumedPlay` (ON) ·
`overloadConsumerEnabled` (**OFF** since R5-d, D-R5-20=B) · `overloadThreshold` (6) · `overloadCost` (6) ·
`overloadHypeFactor` (1.5, clamped ≥1 in the getter — a factor < 1 would turn Overload into a
silent punishment). All read at the loop boundary, so they are hot-adjustable. Inherited tuning
rule: if Overload never fires in short songs, **lower the threshold**; never raise generation
(that would reopen D-R5-10 and invalidate the R5-a/R5-b runbook arithmetic). With `flow == null`
the consumer does not fire, deliberately diverging from generation (which mirrors the SO's
default ON): generating needs one bool, consuming needs three authored numbers, and inventing
those in code would stop the SO from being the tuning authority.

**Dev-card interaction (operational note, R5-b).** With `GenerateVoltageOnConsumedPlay = ON` the
`DEV_Voltage_*` cards stop being neutral instruments: they apply their `stacksDelta` **and** fire
the hook (`DEV_Voltage_Plus1` played by Conito = **+2**). The ST-R5a-1..4 arithmetic (1→2→3,
7+4=9, 2−2=0) is only valid with the toggle **OFF**; turning it off is the standard procedure for
re-running the R5-a suite. *(Primary home is the R5-a runbook header; recorded here because that
runbook is not a governed document.)*

**Verification:** ST-R5a-1..5 + ST-R5a-6R PASS (2026-08-21) — absence of decay checked at seven
boundaries with a positive control, not by reading the SO; the clamp at 9 checked from 7 with
`+4` and from 9 with `+1`. ST-R5b-1..6 + ST-R5b-7R PASS (2026-08-21) — generation, denial paths,
attribution, toggle. ST-R5c-1..9 PASS first time (2026-08-21) — threshold, spend, ×1.5 on
`hypeDelta`, one discharge per boundary, no pending state.

---

## 6. Stacks, duration, and lifecycle

Status behavior may vary by authored configuration, but the following invariants hold:

- statuses may be stack-based
- duration/expiry must be explicit in runtime or authoring semantics
- a status application path must resolve through one canonical runtime route
- multiple variants must not become multiple silent meanings for the same gameplay concept
- **`MaxStacks` always caps, in every `StackMode`.** The final clamp in
  `ApplyStackingPolicy` sits **outside** the switch, so `Additive` and `AdditiveClamped`
  produce the same observable result. A status that must be genuinely unbounded needs a
  deliberately high `MaxStacks`, not a different stacking mode. *(Verified against code in
  R5-inv, 2026-08-11 — D5.)*
- **`TickTiming.None` means "at every timing", not "at none".** The filter in
  `StatusEffectContainer.Tick` only discards statuses whose timing is defined *and* does not
  match the current boundary (`if (def.Tick != TickTiming.None && def.Tick != timing) continue;`),
  so a `None` timing always passes the filter and falls through to `switch (def.Decay)`. What
  prevents decay is **`DecayMode.None`**, never `TickTiming`. A status that must be immune to
  the passage of time is authored with `DecayMode = None`; setting `TickTiming = None` is at
  most redundant. *(Verified against code in R5-a, 2026-08-21; regressed by ST-R5a-2 — D13.)*

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

**Operational note (R4, 2026-08-10 — context for F-R4-3):** a new primitive requires **two** writes, not one — the value in `CharacterStatusId` (serialization contract) **and** the `case` in `CharacterStatusPrimitiveDatabaseSO.TryGetCanonicalData` (navigable registry). Without the second, `Populate From CSO Canonical` skips the entry with a warning and the gap silently reopens after any regeneration (per D-R4-6=A, the CSO registry is repopulated from canonical after adding the `case`).

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

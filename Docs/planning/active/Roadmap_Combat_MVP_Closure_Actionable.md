# Roadmap — Combat MVP Closure (Actionable)

**Date:** 2026-03-21  
**Last updated:** 2026-03-23 (Combat MVP CLOSED — all phases complete including doc sync)  
**Derived from:** ALWTTT_Combat_MVP_Audit_Final.md  
**Status:** Planning only — does not define implementation truth  
**Goal:** Close the Combat MVP as quickly as possible with a testable, honest boundary

---

## Overview: 5 phases, dependency-ordered

| Phase | Name | Goal | Estimated effort | Status |
|---|---|---|---|---|
| **0** | StatusEffectSO prerequisites | Ensure catalogue has the SO assets the testing deck needs | Tiny (asset verification) | ✅ Done |
| **1** | Unified Stress path | Fix the critical dual-path bug | Small (one helper + two call-site changes) | ✅ Done |
| **2** | DrawCardsSpec implementation | Complete the last stub effect | Tiny (one block in CardBase) | ✅ Done |
| **3** | Testing deck + validation | Import batch JSON deck, verify each effect and status behaviour | Small (JSON import + play session) | ✅ Done — all B/C/S checks passed |
| **4** | Scope decisions + doc sync | Decide Breakdown/Shaken/Cohesion scope, update all docs | Medium (code changes + doc edits) | ✅ Done — all decisions A–H applied, doc sync complete |

---

## Phase 0 — StatusEffectSO Prerequisites ✅ COMPLETE

**Completed:** 2026-03-22

### Outcome
Catalogue verified. Six SO entries confirmed with real keys:

| statusKey | EffectId (CharacterStatusId) | DisplayName | IsDefaultVariant |
|---|---|---|---|
| `flow` | DamageUpFlat (100) | Flow | ✅ |
| `composure` | TempShieldTurn (400) | Composure | ✅ |
| `exposed` | DamageTakenUpFlat (300) | Exposed | ❌ |
| `feedback` | DamageOverTime (600) | Feedback | ❌ |
| `choke` | DisableActions (700) | Choke | ❌ |
| `shaken` | ShakenRestriction (503) | Shaken | ❌ |

### Decision recorded
Original roadmap keys `damage_up` and `vulnerability` do not exist. Invalid. All JSON must use the six keys above only.

---

## Phase 1 — Unified Stress Path (CRITICAL) ✅ COMPLETE

**Completed:** 2026-03-23

### Problem (was)
Two stress paths existed. Neither was complete:
- **CardBase.ModifyStressSpec**: absorbed Composure ✅, triggered Breakdown ❌
- **AddStressAction**: absorbed Composure ❌, triggered Breakdown ✅

### Solution applied
Added `ApplyIncomingStressWithComposure()` to `BandCharacterStats`. Both paths now route through it.

### Files modified

| File | Change |
|---|---|
| `BandCharacterStats.cs` | Added `ApplyIncomingStressWithComposure()` + 2 usings |
| `CardBase.cs` | Replaced ModifyStressSpec positive branch |
| `AddStressAction.cs` | Replaced AddStress call + 1 using |

### Verified path table

| Entry point | Composure absorbs? | Breakdown fires? |
|---|---|---|
| Card plays positive Stress | ✅ | ✅ |
| Audience AddStress action | ✅ | ✅ |
| Card plays negative Stress (healing) | N/A | N/A |
| HealStressAction | N/A | N/A |

---

## Phase 2 — DrawCardsSpec Implementation ✅ COMPLETE

**Completed:** 2026-03-23

### Problem (was)
`DrawCardsSpec` handler in `CardBase.ExecuteEffects()` logged a warning and did nothing.

### Solution applied
Replaced stub with `DeckManager.DrawCards(draw.count)`.

**Verified:** `DeckManager.DrawCards(int)` confirmed public. `DeckManager` was already a cached property on `CardBase`. No new usings required.

### File modified

| File | Change |
|---|---|
| `CardBase.cs` | Replaced DrawCardsSpec stub (~4 lines → ~9 lines) |

---

## Phase 3 — Testing Deck + Validation ✅ COMPLETE

**Completed:** 2026-03-23

### 3.1 — Import pipeline contract

The import tool is `CardEditorWindow` (ALWTTT → Cards → Card Editor).

**Batch format — the only valid root shape:**
```json
{
  "cards": [ { ... }, { ... } ]
}
```

Raw JSON arrays `[ ... ]` at the root are explicitly rejected by the parser.

**Mandatory fields per card:**
```json
{
  "kind": "Action",
  "id": "test_card_id",
  "displayName": "Human Name",
  "inspirationCost": 0,
  "entry": { "flags": "UnlockedByDefault" }
}
```

**Effect types supported by the importer (all four map to implemented code):**

| JSON `type` | Maps to | Required fields |
|---|---|---|
| `"ApplyStatusEffect"` | `ApplyStatusEffectSpec` | `statusKey` (string), `targetType`, `stacksDelta` |
| `"ModifyVibe"` | `ModifyVibeSpec` | `targetType`, `amount` |
| `"ModifyStress"` | `ModifyStressSpec` | `targetType`, `amount` |
| `"DrawCards"` | `DrawCardsSpec` | `count` |

**Valid `targetType` values:** `Self`, `Musician`, `AudienceCharacter`, `AllAudienceCharacters`, `AllMusicians`, `RandomAudienceCharacter`, `RandomMusician`

**Valid `statusKey` values:** `flow`, `composure`, `exposed`, `feedback`, `choke`, `shaken`

### 3.2 — Card effect analysis: designed vs implemented vs testable

| Effect type | Spec class | Designed | Implemented | Runtime-validated | Notes |
|---|---|---|---|---|---|
| Apply Status Effect | `ApplyStatusEffectSpec` | ✅ | ✅ | ✅ S1–S5 confirmed | |
| Modify Vibe | `ModifyVibeSpec` | ✅ | ✅ | ✅ C2/C3 confirmed | |
| Modify Stress | `ModifyStressSpec` | ✅ | ✅ | ✅ C1/C4 confirmed | |
| Draw Cards | `DrawCardsSpec` | ✅ | ✅ Phase 2 | ✅ confirmed | |

### 3.3 — Status effect validation results

| Test | Status | Notes |
|---|---|---|
| S1: Flow +1 on musician | ✅ PASS | |
| S2: Composure +5 on musician | ✅ PASS | |
| S3: Exposed +1 on musician | ✅ PASS | target corrected from Audience (Decision F) |
| S4: Feedback +2 on musician | ✅ PASS | |
| S5: Choke +1 on musician | ✅ PASS | target corrected from Audience (Decision G) |
| B1: Composure absorbs incoming Stress | ✅ PASS | |
| B2: Breakdown triggers when Stress ≥ MaxStress after absorption | ✅ PASS | |
| B3: Flow stacks boost Vibe per card play | ✅ PASS | flowVibeFlatBonusPerStack=1 |
| B5: Choke on musician → stun → stacks decay via Tick | ✅ PASS | |
| B6: Composure clears at PlayerTurn start via Inspector snapshot | ✅ PASS | Decision A |
| B7: Song-end Flow + Composure reset via Inspector snapshot | ✅ PASS | |

### 3.4 — Composition card: test_pass_turn ✅ COMPLETE

`test_pass_turn` composition card asset created manually in Unity Editor. `inspirationCost: 0`, `UnlockedByDefault`, `CompositionCardPayload` with no effects. Added to `TestingDeck_CombatMVP`. Verified: playing it routes through composition pipeline and advances turn.

---

## Phase 4 — Scope Decisions + Implementation ✅ COMPLETE

**Completed:** 2026-03-23

### 4.1 — Fix log

| Fix | File | Change | Status |
|---|---|---|---|
| Fix 3.7a | `HandController.cs` | Action card performer resolution via FixedPerformerType | ✅ Done |
| Fix 3.7b | `MusicianCharacterSimple.cs` | Editor status snapshot field | ✅ Done |

### 4.2 — Decisions: all resolved and all code applied ✅

#### Decision A: Composure expiry behaviour ✅ CODE APPLIED
**Decision:** Composure clears at the start of each `PlayerTurn`. It is a per-turn buffer, not a persistent resource.

**Code applied in `GigManager.ExecuteGigPhase()`, `GigPhase.PlayerTurn` branch:**
```csharp
// Decision A: Composure is turn-scoped — clear at each PlayerTurn start
foreach (var m in CurrentMusicianCharacterList)
{
    m?.Statuses?.Clear(CharacterStatusId.TempShieldTurn);
}
```

**Runtime-validated:** B6 ✅ PASS — Composure cleared at turn start confirmed via Inspector snapshot.

---

#### Decision B: Status Tick wiring ✅ CODE APPLIED
**Decision:** Wire `StatusEffectContainer.Tick()` at turn boundaries.

**Code applied in `GigManager.ExecuteGigPhase()` and `AudienceTurnRoutine()`:**
```csharp
// PlayerTurn — tick musician statuses
foreach (var m in CurrentMusicianCharacterList)
    m?.Statuses?.Tick(TickTiming.PlayerTurnStart);

// AudienceTurn — tick audience statuses
foreach (var a in CurrentAudienceCharacterList)
    a?.Statuses?.Tick(TickTiming.AudienceTurnStart);
```

**TickTiming enum updated:** `PlayerTurnStart = 8` and `AudienceTurnStart = 9` added to `StatusEffectSO.cs`. `StartOfTurn = 1` retained for back-compat. Choke SO `Tick Timing` updated to `Player Turn Start` in Inspector.

**Runtime-validated:** B5 ✅ PASS — Choke stacks decay correctly after one turn.

---

#### Decision C: Breakdown severity ✅ CODE APPLIED + WIRED
**Decision:** On Breakdown: Cohesion−1, Stress resets to configurable fraction (default 0.5), Shaken applied.

**Code applied in `MusicianBase.OnBreakdown()`:**
```csharp
protected void OnBreakdown()
{
    stats.ApplyStatus(StatusType.Breakdown, 1); // legacy UI
    IsStunned = true;

    var pd = GameManager.PersistentGameplayData;
    if (pd != null)
    {
        pd.BandCohesion = Mathf.Max(0, pd.BandCohesion - 1);
        if (pd.BandCohesion <= 0)
        {
            GigManager.Instance?.LoseGig();
            return;
        }
    }

    float resetFraction = GigManager.Instance != null
        ? GigManager.Instance.BreakdownStressResetFraction
        : 0.5f;
    int resetTarget = Mathf.FloorToInt(stats.MaxStress * resetFraction);
    stats.SetCurrentStress(resetTarget);

    // Decision C: Apply Shaken via catalogue key lookup
    if (StatusCatalogue != null &&
        StatusCatalogue.TryGetByKey("shaken", out var shakenSO))
    {
        Statuses?.Apply(shakenSO, 1);
    }
    else
    {
        Debug.LogWarning(
            $"[MusicianBase] Shaken SO not found. " +
            $"Add 'shaken' to StatusEffectCatalogueSO and assign the catalogue to this musician prefab. " +
            $"Musician='{name}'");
    }
}
```

**`breakdownStressResetFraction` field on `GigManager`:**
```csharp
[Header("Breakdown")]
[SerializeField, Range(0f, 1f)] private float breakdownStressResetFraction = 0.5f;
public float BreakdownStressResetFraction => breakdownStressResetFraction;
```

**Shaken SO config (final):**
- `ShakenRestriction = 503` in `CharacterStatusId.cs`
- `ShakenRestriction` case in `CharacterStatusPrimitiveDatabaseSO.TryGetCanonicalData`
- `CharacterStatusPrimitiveDatabase` asset repopulated
- `Status Effect_Shaken` SO: key=`shaken`, EffectId=`ShakenRestriction`, Replace, MaxStacks=1, LinearStacks, **AudienceTurnStart** tick, IsBuff=true

**Shaken duration:** Applied at Audience Turn of Song N → expires at start of Audience Turn N+1. Active through: rest of Audience Turn N, Player Turn N+1 (action window + composition), Performance N+1, Song End N+1.

**Inspector requirement:** `StatusCatalogue` field on each `MusicianBase` prefab must have the `StatusEffectCatalogueSO` asset assigned. Without this, Shaken will not apply and a LogWarning fires.

**Open design questions (non-blocking):**
- What gameplay restrictions does Shaken impose at runtime? (intended: cannot play Action cards in action window; Composure granted reduced 50%) — deferred to a follow-up pass.

---

#### Decision D: Cohesion as mid-gig failure condition ✅ CODE APPLIED
**Decision:** Cohesion ≤ 0 → `LoseGig()`. Triggered by each Breakdown (Decision C).

**Code applied:** Inside `MusicianBase.OnBreakdown()` — see Decision C code above. `LoseGig()` confirmed present and public in `GigManager`.

**Note:** Original roadmap reference to `TriggerGigLoss()` was incorrect — that method does not exist. Correct method is `LoseGig()`.

---

#### Decision E: Exposed and Feedback ✅ CODE APPLIED
**Decision:** Both wired. Feedback is musician-only for MVP (audience has no Stress path).

**Exposed — code applied in `BandCharacterStats.ApplyIncomingStressWithComposure()`:**
```csharp
// Decision E: Exposed amplifies remaining stress
if (statuses != null &&
    statuses.TryGet(CharacterStatusId.DamageTakenUpFlat, out var exposedInst) &&
    exposedInst != null && exposedInst.Stacks > 0)
{
    float mult = 1f + (exposedInst.Stacks * _exposedMultiplierPerStack);
    remaining = Mathf.CeilToInt(remaining * mult);
}
```
`_exposedMultiplierPerStack = 0.25f` field added to `BandCharacterStats`. Configurable via `ExposedStressMultiplierPerStack` property.

**Feedback — code applied in `GigManager.AudienceTurnRoutine()`:**
```csharp
// Decision E: Feedback DoT — applies to musicians only (audience Stress not yet implemented)
foreach (var m in CurrentMusicianCharacterList)
{
    if (m?.Statuses == null) continue;
    int feedbackStacks = m.Statuses.GetStacks(CharacterStatusId.DamageOverTime);
    if (feedbackStacks > 0)
        m.Stats?.ApplyIncomingStressWithComposure(m.Statuses, feedbackStacks);
}
```

**Audience Feedback scope note:** `AudienceCharacterStats` has no `AddStress` or Stress resource. A different status with the same DoT mechanic will be designed for audience members in a future pass. Explicitly out of scope for MVP.

**Compile fix applied:** `m.MusicianStats` (returns `IMusicianStats`) replaced with `m.Stats` (returns concrete `BandCharacterStats`) to resolve CS1061.

---

#### Decisions F–H ✅ RESOLVED (previous sessions)
- F: Exposed = musician-only. Test card target corrected.
- G: Choke = musician-only. Audience crowd-control = future unscoped status.
- H: Action card performer = FixedPerformerType (Fix 3.7a).

---

### 4.3 — Post-MVP milestone: Status effect icons and tooltips

Not a blocker for MVP closure. First post-MVP UI pass when ready.

**Scope:**
- Distinct icons for all status effects
- Stack display on character canvas (musician + audience)
- Tooltip text per status
- Dependency: SSoT_Status_Effects should define icon/display contract before implementation

---

### 4.4 — Documentation updates ✅ COMPLETE

| Document | Update | Status |
|---|---|---|
| **SSoT_Gig_Combat_Core** | Composure reset timing, Breakdown consequences, Shaken duration, Exposed §6.4, Feedback §6.5, §11 implementation status | ✅ |
| **SSoT_Status_Effects** | Tick system §3.1, dual system §3.2, canonical MVP set §5 rewritten, all six statuses specified | ✅ |
| **SSoT_Card_System** | DrawCardsSpec confirmed, performer rule clarified, built-ins table | ✅ |
| **SSoT_Gig_Encounter** | LoseGig() implementation note in §7.3 | ✅ |
| **SSoT_Runtime_Flow** | ExecuteGigPhase bypass §4 + invariant #7 | ✅ |
| **CURRENT_STATE** | Phase 4 completion record, post-MVP work ordered | ✅ |
| **changelog-ssot** | Phase 4 semantic changes recorded | ✅ |

---

### 4.5 — MVP checklist ✅ ALL COMPLETE

```markdown
- [x] Start gig → initial hand is drawn
- [x] Action card play path works end-to-end
- [x] DrawCardsSpec draws cards at runtime (Phase 2)
- [x] ModifyVibe works (all-audience targeting confirmed)
- [x] ModifyStress works — positive and negative
- [x] ApplyStatusEffect applies stacks to correct character
- [x] Composure absorbs positive Stress before application (Phase 1 + B1/B2 confirmed)
- [x] Breakdown triggers when Stress ≥ MaxStress after Composure (Phase 1)
- [x] Flow stacks boost Vibe per card play (B3 confirmed, flowVibeFlatBonusPerStack=1)
- [x] Action card performer resolves via FixedPerformerType — card owner is caster (Fix 3.7a)
- [x] Status snapshot visible in MusicianCharacterSimple Inspector (Fix 3.7b)
- [x] Test deck: all 9 action cards confirmed functional in play session
- [x] Exposed target type corrected to Musician (Decision F)
- [x] Choke target type corrected to Musician (Decision G)
- [x] All Phase 4 design decisions resolved (A–H)
- [x] test_pass_turn composition card created and functional (Task 3.5c)
- [x] B5: Choke on musician → stun → stacks decay after turn via Tick ✅ PASS
- [x] B6: Composure clears at turn start confirmed via Inspector snapshot ✅ PASS (Decision A)
- [x] B7: Song-end Flow + Composure reset confirmed via Inspector snapshot ✅ PASS
- [x] Decision A code: wire Composure clear at PlayerTurn start in GigManager ✅
- [x] Decision B code: wire Tick() — PlayerTurnStart (musicians) + AudienceTurnStart (audience) ✅
- [x] Decision B pre-check: choke SO — LinearStacks + PlayerTurnStart ✅
- [x] Decision B infra: TickTiming enum updated with PlayerTurnStart=8 + AudienceTurnStart=9 ✅
- [x] Decision C code: Breakdown → Cohesion−1 + Stress reset (configurable 0.5) ✅
- [x] Decision C asset: ShakenRestriction=503 added to CharacterStatusId ✅
- [x] Decision C asset: ShakenRestriction case added to CharacterStatusPrimitiveDatabaseSO.TryGetCanonicalData ✅
- [x] Decision C asset: CharacterStatusPrimitiveDatabase repopulated ✅
- [x] Decision C asset: shaken StatusEffectSO created (AudienceTurnStart tick, MaxStacks=1, LinearStacks) ✅
- [x] Decision C asset: shaken SO added to StatusEffectCatalogueSO ✅
- [x] Decision C wire: Shaken applied in MusicianBase.OnBreakdown via TryGetByKey("shaken") ✅
- [x] Decision D code: Cohesion ≤ 0 → LoseGig() in MusicianBase.OnBreakdown ✅
- [x] Decision E code: wire Exposed stress multiplier in ApplyIncomingStressWithComposure ✅
- [x] Decision E code: wire Feedback DoT (musician-only) in AudienceTurnRoutine ✅
- [x] All SSoT docs updated to match code reality (Phase 4 doc sync) ✅
```

---

## Summary: Total files modified across all phases

| Phase | File | Change | Status |
|---|---|---|---|
| 0 | StatusEffectSO assets + catalogue | Verify SO entries | ✅ Done |
| 1 | `BandCharacterStats.cs` | Add `ApplyIncomingStressWithComposure()` + 2 usings | ✅ Done |
| 1 | `CardBase.cs` | Replace ModifyStressSpec positive branch | ✅ Done |
| 1 | `AddStressAction.cs` | Replace AddStress call + 1 using | ✅ Done |
| 2 | `CardBase.cs` | Replace DrawCardsSpec stub | ✅ Done |
| 3 | Card assets + deck | Import 9-card deck, BandDeckData, validation | ✅ Done |
| 3 (Fix 3.7a) | `HandController.cs` | Action card performer resolution via FixedPerformerType | ✅ Done |
| 3 (Fix 3.7b) | `MusicianCharacterSimple.cs` | Editor status snapshot field | ✅ Done |
| 3 (Task 3.5c) | `test_pass_turn` asset | Composition card for turn advancement | ✅ Done |
| 4-A | `GigManager.cs` | Clear Composure at PlayerTurn start | ✅ Done |
| 4-B | `GigManager.cs` | Wire Tick() at PlayerTurnStart (musicians) + AudienceTurnStart (audience) | ✅ Done |
| 4-B | `StatusEffectSO.cs` | Add PlayerTurnStart=8 + AudienceTurnStart=9 to TickTiming enum | ✅ Done |
| 4-B | `choke` StatusEffectSO | Update Tick Timing to PlayerTurnStart | ✅ Done |
| 4-C | `MusicianBase.cs` | Breakdown → Cohesion−1 + Stress reset + Shaken wired via TryGetByKey | ✅ Done |
| 4-C | `CharacterStatusId.cs` | Add ShakenRestriction=503 | ✅ Done |
| 4-C | `CharacterStatusPrimitiveDatabaseSO.cs` | Add ShakenRestriction case to TryGetCanonicalData | ✅ Done |
| 4-C | `CharacterStatusPrimitiveDatabase` asset | Repopulate via context menu | ✅ Done |
| 4-C | `Status Effect_Shaken` SO | Create shaken SO asset (AudienceTurnStart tick) | ✅ Done |
| 4-C | `StatusEffectCatalogueSO` | Add shaken SO to catalogue | ✅ Done |
| 4-D | `GigManager.cs` | LoseGig() made public | ✅ Done |
| 4-D | `MusicianBase.cs` | Cohesion ≤ 0 → LoseGig() (inside OnBreakdown) | ✅ Done |
| 4-E | `BandCharacterStats.cs` | Wire Exposed stress multiplier + _exposedMultiplierPerStack field | ✅ Done |
| 4-E | `GigManager.cs` | Wire Feedback DoT (musician-only) in AudienceTurnRoutine | ✅ Done |
| 4-E | `GigManager.cs` | breakdownStressResetFraction field + BreakdownStressResetFraction property | ✅ Done |
| 4-doc | 6 SSoT files + CURRENT_STATE + changelog | Documentary sync | ✅ Done |

---

## Post-MVP work (separate roadmap when ready)

| Item | Priority | Notes |
|---|---|---|
| Composition session testing + real deck design | 🔴 High | First real design question after MVP. Timing, deck speed, card variety. |
| Shaken restrictions enforcement | 🟡 Medium | Design decision pending. Status applies; restrictions not enforced. |
| Status effect icons + stack display UI | 🟡 Medium | Visual polish. SSoT_Status_Effects should define icon contract first. |
| Audience Feedback DoT | 🟢 Low | Requires Stress path on AudienceCharacterBase. Deferred. |

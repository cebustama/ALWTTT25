# ALWTTT — Card Effects Extensions Backlog + Triggered Mechanics (WAUC-aligned)
**Version:** v0.1  
**Date:** 2026-02-11  
**Status:** Draft (implementation-facing)  
**Companion SSoT (authoritative contracts):** `ALWTTT_CardEffects_and_CardEditorWindow_SSoT_v0.2_2026-01-23.md`  
**Companion Roadmap (live tracking):** `ALWTTT_CardEffects_and_CardEditorWindow_Roadmap_Live_v0.3_2026-01-23.md`

> This document **replaces** the outdated parts of `SOLID_WAUC_CardEffects_and_TriggeredStatuses_TechnicalReport_v2_HandwrittenCards.md`
> that assumed a `CardEffectId + CardEffectActionData` “two-lane” payload (`cardEffects` + `statusActions`).
> The current codebase design is **effects-only**: `CardPayload.effects : List<CardEffectSpec>`.

---

## 0) Why this doc exists
We already have a stable, scalable authoring contract:
- Cards store a polymorphic list of **data-only** specs (`CardEffectSpec`) via `[SerializeReference]`.
- `ApplyStatusEffectSpec` stores a **direct** `StatusEffectSO` asset reference (supports variants via `statusKey`).

What we *don’t* have consolidated in one place is:
- the **future effect types** we want to add (deck ops, discard/choose, curses, conditionals, repeats),
- and a WAUC plan for **triggered mechanics** (OnDraw, OnDiscard, OnSongEnd, etc.)
  without building a full rules engine.

This backlog is based on:
1) The handwritten card concepts, and  
2) The additional “Excel list” of desired card effects (Draw/Discard/Conditional/etc.).

---

## 1) Current baseline (do not contradict)
These are “already decided” contracts:

### 1.1 Effects-only payload
`CardPayload.effects : List<CardEffectSpec>` is the single main mechanic representation.

### 1.2 Status application is just another effect
`ApplyStatusEffectSpec { StatusEffectSO status; ActionTargetType targetType; int stacksDelta; float delay; }`

### 1.3 JSON import and CardEditorWindow are effects-only
Importer must populate `effects[]` and resolve statuses via `statusKey` → `StatusEffectCatalogueSO`.

---

## 2) Effect types backlog (from the Excel list)
Each entry below follows the extension rule:
> **One new mechanic** = one new `CardEffectSpec` subclass (data-only) + (1) editor UI, (2) JSON schema, (3) runtime executor.

### 2.1 Hand / Deck operations (high priority)
#### A) Discard X cards
**Spec:** `DiscardCardsSpec`
- `int count`
- `DiscardMode mode` = `Random | Choose` *(start with Random)*
- `ActionTargetType owner` *(default: Self / PlayerHand owner)*

**Notes:** `Choose` requires UI selection; keep `Random` as MVP.

#### B) Choose X random cards (“Discovery”)
Interpretation: show a **choice of random cards** from a pool; player picks one (or more).
**Spec:** `DiscoverCardsSpec`
- `int offerCount` *(how many you show; e.g., 3)*
- `int pickCount` *(how many player chooses; default 1)*
- `DiscoverPoolKind pool` = `FromDrawPile | FromDiscardPile | FromGlobalPool(tagged)` *(decide)*
- `DiscoverResolution resolution` = `AddToHand | AddToDiscard | AddToDrawTop`

#### C) Scry
**Spec:** `ScrySpec`
- `int count` *(look at top N draw cards)*
- `ScryResolution` = `DiscardSelected | KeepOrder | Reorder` *(MVP: DiscardSelected only)*

#### D) Put next card you play this turn on top of draw pile
**Spec:** `PutNextPlayedCardOnTopSpec`
- `int uses` *(default 1)*
- `bool consumeOnUse` *(default true)*

Implementation hint: model as a temporary “turn flag” in the hand/deck service.

#### E) Shuffle C into draw/discard pile (curses, created cards)
**Spec:** `ShuffleCardIntoPileSpec`
- `CardReference card` *(by id/guid/catalog ref)*
- `PileKind destination` = `Draw | Discard`
- `int count`
- `ShuffleMode mode` = `InsertRandom | InsertTop | InsertBottom` *(MVP: InsertRandom)*

#### F) Exhaust
**Decision:** Prefer a **card-level flag** (not an effect) when it’s “this card exhausts after play”.
But you may still want:
**Spec:** `ExhaustTargetCardsSpec` for “exhaust selected cards” future design.

---

### 2.2 Damage / AoE / repeats (medium-high priority)
#### A) Deal Y to all enemies X times
**Spec:** `DealDamageSpec`
- `int amount`
- `ActionTargetType targetType` *(Single / AllEnemies / RandomEnemy / AllAudience etc.)*
- `RepeatRule repeat` *(optional; see 2.4)*

> “Enemies” in ALWTTT currently maps to **Audience Members**; keep naming consistent in the spec/UI.

---

### 2.3 Status ops beyond ApplyStatus (medium priority)
#### A) Quitar Buffs (remove buffs)
**Spec:** `RemoveStatusesSpec`
- `StatusFilter filter` = `BuffsOnly | DebuffsOnly | Any`
- `int stacksDelta` *(optional; if omitted, remove all stacks)*
- `ActionTargetType targetType`

Requires that `StatusEffectSO` exposes `isBuff` (or equivalent) reliably.

#### B) Add X STATUS to random
**Spec:** `ApplyRandomStatusFromSetSpec`
- `StatusSetReference set` *(a ScriptableObject or tag-based set)*
- `int stacksDelta`
- `ActionTargetType targetType`
- `int pickCount` *(default 1)*

This avoids “random from the entire catalogue”.

#### C) “Composure is not removed at the start of next Song”
**Recommended design:** model as a **separate StatusEffectSO variant** of Composure
with a different decay/reset rule (e.g., does not reset on SongStart).
Then this is just:
- `ApplyStatusEffectSpec(status = Composure_Persistent, ...)`

---

### 2.4 WAUC conditionals (avoid generic graphs)
We want “If X then Y” *without* building an effect language.

#### A) If Card Discarded, then X
Interpretation: “When this card is discarded, do X.”
**Recommended:** this is a **card lifecycle trigger**, not a play effect.

**Spec:** `OnDiscardTriggerSpec`
- `List<CardEffectSpec> effects` *(nested; acceptable because it’s still data-only)*
- (optional) `TriggerLimit limit` *(once per turn, etc.)*

#### B) Draw X cards. If you draw a T type, Y
**Spec:** `DrawCardsThenIfMatchSpec`
- `int drawCount`
- `CardType matchType`
- `List<CardEffectSpec> onMatchEffects`

MVP simplification: `onMatchEffects` can be a single spec field instead of a list.

#### C) STATUS affects this card X times / Repeat by status stacks
This is the “repeat count depends on something”.

**WAUC option 1 (recommended):** a small wrapper spec:
`RepeatEffectSpec`
- `RepeatCountSource source` = `Constant | ByStatusStacks`
- `int constantCount`
- `CharacterStatusId statusId`
- `int multiplier` *(default 1)*
- `int maxRepeats` *(safety cap; optional)*
- `CardEffectSpec inner`

**WAUC option 2:** embed repeat fields directly into `DealDamageSpec` (and a few others).
Less flexible, but simpler.

---

### 2.5 “Power” style persistent effects (future)
Your Excel includes “At the end of combat, X” and “When enemy is convinced, X”.
These are naturally modeled as **triggered statuses / passives**, not one-shot effects.

**Recommended approach (WAUC):**
- Implement “Power cards” by applying a long-lived `StatusEffectSO` variant
  with trigger metadata (OnSongEnd, OnGigEnd, OnAudienceConvinced, etc.),
  and use a `StatusTriggerDispatcher`.

This keeps the card payload simple and pushes long-lived behavior into the status system.

---

## 3) Trigger taxonomy (needed to formalize anything “when/at end”)
Before implementing many of the above, we need a small canonical list of triggers that exist in runtime:

- `OnCardDrawn`
- `OnCardDiscarded`
- `OnCardPlayed`
- `OnLoopStart / OnLoopEnd`
- `OnSongStart / OnSongEnd`
- `OnGigEnd` *(“end of combat” equivalent)*
- `OnAudienceConvinced`

If this list is stable, we can author both:
- card lifecycle triggers (OnDraw / OnDiscard specs), and/or
- triggered statuses (StatusEffectSO trigger block).

---

## 4) Open decisions (call them out explicitly)
1) **Do we allow nested specs?**  
   Needed for `RepeatEffectSpec`, `OnDiscardTriggerSpec`, conditional composites.  
   (Unity managed references support it, but editor UX needs care.)

2) **Where do “lifecycle triggers” live?**  
   - CardDefinition-level lists (`onDrawEffects`, `onDiscardEffects`) **or**
   - Wrapper specs inside `effects` list.  
   Either can be WAUC; pick one and standardize.

3) **Discover pool definition**  
   Needs an explicit source (draw pile vs global pool). Recommend a small `CardSetSO`.

---

## 5) Minimal next slice (suggested)
If you want maximum implementation ROI for the next MVP iteration:

1) `DiscardCardsSpec (Random)`
2) `DealDamageSpec` (Single + AllAudience)
3) `ShuffleCardIntoPileSpec` (InsertRandom → Draw pile)
4) Card flags: `isUnplayable`, `exhaustAfterPlay`
5) Trigger point: `OnCardDrawn` (to support Dead Mic-like curses)

That unlocks a big chunk of the handwritten + excel set with minimal infra.

---

## 6) Appendix — Excel-derived backlog list (raw)
Card Effects listed in the Excel:
- Draw X cards
- Discard X cards
- Choose X random cards
- Quitar Buffs
- Shuffle C into Draw / Discard pile
- If X, then Y
- Unplayable
- STATUS affects this card X times
- When Enemy is convinced, X
- Deal Y to all enemies X times
- Draw X cards. If you draw a T type, Y
- If Card Discarded, then X
- Composure is not removed at the start of next Song
- Exhaust
- Add X STATUS to random.
- Scry
- Put next card you play this turn on top of draw pile.
- At the end of combat, X

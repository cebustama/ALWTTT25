# SSoT_Card_System — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Current ALWTTT card gameplay semantics and card runtime model  
**Owns:** card domains, card identity/payload split, effect-first semantics, performer/targeting rules, runtime card behavior  
**Does not own:** editor/import/json workflow contracts (`SSoT_Card_Authoring_Contracts.md`), package-side MidiGenPlay internals

---

## 1. Purpose

This document is the primary authority for what a **card means in ALWTTT gameplay**.

It replaces the previous mixed role of:
- `reference/Card.md`
- portions of `reference/Gig_Combat.md`
- legacy card-model assumptions still visible in older docs

This SSoT defines the current card system in gameplay/runtime terms.

---

## 2. Card domains

Cards exist in two canonical gameplay domains, derived from payload type.

### 2.1 Action cards
Action cards are played in the gig loop's action windows.

They represent:
- crowd interaction
- tactical pressure / relief
- immediate systemic changes
- between-song decisions and control

Action cards express their mechanics through declarative **Card Effects**.

### 2.2 Composition cards
Composition cards are played during composition.

They represent:
- track/part-level musical decisions
- arrangement structure
- composition-specific modifiers
- future loop shaping

Composition cards may also include normal card effects when the game needs composition choices to apply immediate gameplay consequences.

---

## 3. CardDefinition vs CardPayload

### 3.1 CardDefinition
`CardDefinition` is the stable identity and presentation layer of a card.

It is responsible for:
- stable identity (`Id`, display naming)
- presentation metadata
- economy metadata such as cost / generation fields
- synergies (type / keywords / rarity)
- performer rule (`FixedPerformerType`)
- play rules such as exhaust and targeting overrides
- reference to exactly one `CardPayload`

### 3.2 CardPayload
`CardPayload` is the mechanical meaning of the card.

Core contract:
- a `CardDefinition` references exactly one payload asset
- domain is derived from `payload.Domain`
- both Action and Composition cards share the same effect-first base model

### 3.3 SpecialKeywords

`CardDefinition.Keywords` is a serialized `List<SpecialKeywords>` that tags a card with player-facing trait and mechanic keywords. Keywords serve two purposes: they generate tooltip entries on card hover (via `SpecialKeywordData` lookup in `CardBase.ShowTooltipInfo`), and they will eventually drive runtime behavior (see §3.3.2).

#### 3.3.1 Canonical keyword inventory

The `SpecialKeywords` enum contains exactly 7 values, divided into two categories:

**Card-trait keywords** — describe play rules or lifecycle behavior of the card itself:
- `Consume` — card is permanently removed from the deck after playing. Does not return.
- `Exhaust` — card moves to the exhaust pile after playing. Returns to the deck for the next gig.
- `Ethereal` — card is discarded at end of turn if not played.

**Resource / mechanic / audience keywords** — describe concepts that are not status effects and have no `StatusEffectSO` equivalent:
- `Stress` — musician resource (HP).
- `Vibe` — audience member resource (HP).
- `Convinced` — audience win-state (defeated).
- `Tall` — audience layout trait (blocks characters behind).

#### 3.3.2 Keyword modeling rule

Keywords are NOT status effects. Any game concept that has a `StatusEffectSO` representation (e.g. Flow, Composure, Choke, Shaken, Exposed, Feedback) is surfaced through the status-effect tooltip pipeline (`CardEffectDescriptionBuilder` + `StatusIconBase`), not through `SpecialKeywords`. Legacy keyword entries that duplicated status concepts (`Chill`, `Skeptical`, `Heckled`, `Hooked`, `Blocked`, `Stunned`) were removed in M1.3b (2026-04-23).

#### 3.3.3 Runtime coupling gap (known, not yet resolved)

`ExhaustAfterPlay` (a bool on `CardDefinition`) and the `Exhaust` keyword are currently independent. A card can have the bool without the keyword (silent exhaust, no tooltip) or the keyword without the bool (tooltip says Exhaust but card does not exhaust). The JSON importer emits a coherence warning when these diverge.

The planned resolution is to retire per-keyword bools in favor of runtime checks against `Keywords.Contains(...)`, making the keywords list the single source of both tooltip and behavior. This has not been implemented yet; it touches the card-play pipeline and belongs in a dedicated batch.

---

## 4. The current canonical model is effects-first

All gameplay mechanics authored on a card live under:

```text
CardPayload.Effects : IReadOnlyList<CardEffectSpec>
```

Implementation-facing storage may use:

```text
[SerializeReference] List<CardEffectSpec> effects
```

Meaning of this rule:
- cards are not defined by per-card procedural scripts
- mechanics are represented as declarative specs
- extending the system means adding new `CardEffectSpec` subclasses plus supporting editor/runtime handling

This is the current canonical model.
Legacy action-list language is not primary truth anymore.

---

## 5. Payload types

### 5.1 ActionCardPayload
ActionCardPayload represents Action-domain cards.

Current owned semantics:
- `ActionTiming` controls when the card can be played
- `Conditions` are reserved for gating/requirements
- inherited `Effects` define the actual gameplay outcome

Rule:
- Action cards do not need per-card procedural logic to express their meaning
- their gameplay meaning is the interpreted result of their authored effect list

### 5.2 CompositionCardPayload
CompositionCardPayload represents Composition-domain cards.

Gameplay-facing fields include:
- `PrimaryKind`
- `TrackAction`
- `PartAction`
- `ModifierEffects`
- `RequiresMusicianTarget` when composition semantics require a musician selection

Rule:
- these fields define **ALWTTT gameplay semantics of composition cards**
- they do **not** make ALWTTT the authority over MidiGenPlay package internals

If a composition card references track/bundle/composer structures, ALWTTT owns the gameplay meaning of that choice, while package-internal generation details belong to MidiGenPlay.

---

## 6. CardEffectSpec model

### 6.1 Base rule
`CardEffectSpec` is the abstract base type for card effects.

Rules:
- specs are **data-only**
- runtime logic is handled by an executor/interpreter layer
- new mechanics are added by creating new spec subclasses plus authoring/runtime support

### 6.2 Built-in effect specs currently in active vocabulary

All four effect types below are implemented and runtime-validated.

| Spec class | JSON `type` | Status | Notes |
|---|---|---|---|
| `ApplyStatusEffectSpec` | `"ApplyStatusEffect"` | ✅ Implemented + validated | Applies a direct `StatusEffectSO` reference |
| `ModifyVibeSpec` | `"ModifyVibe"` | ✅ Implemented + validated | Targets audience characters |
| `ModifyStressSpec` | `"ModifyStress"` | ✅ Implemented + validated | Routes through `ApplyIncomingStressWithComposure` for positive values |
| `DrawCardsSpec` | `"DrawCards"` | ✅ Implemented + validated | Calls `DeckManager.DrawCards(count)` at effect execution time |

### 6.3 ApplyStatusEffectSpec
This effect applies a concrete authored `StatusEffectSO` variant.

Canonical fields:
- `status` — direct SO reference (resolved at design time in the Inspector / JSON import)
- `targetType`
- `stacksDelta`
- `delay`

Rule:
- a card applies a concrete status asset, not just a primitive enum id
- this allows multiple tuned variants of the same abstract status primitive
- contrast with runtime code paths (e.g. `OnBreakdown`) that must resolve by string key via the catalogue

---

## 7. Status interaction from cards

Cards may apply statuses as ordinary effects.
That means status application is not a separate parallel card-mechanics system.

Rule:
- status application is just another `CardEffectSpec`
- status runtime semantics themselves belong to `SSoT_Status_Effects.md`
- card-side meaning stays here

---

## 8. Performer and targeting rules

### 8.1 Performer rule
A card may be:
- playable by any musician (no restriction)
- restricted to a fixed performer type (`FixedPerformerType` field on `CardDefinition`)

**Current implementation:** performer is resolved at play time via `FixedPerformerType` — the musician who owns or plays the card is the performer. In effect targeting, `Self` = card owner/performer. This was validated in Fix 3.7a.

### 8.2 Effect-driven targeting
Targeting is derived primarily from authored effects.

MVP-facing rule set:
- single-target effect types require explicit target selection (`Musician`, `AudienceCharacter`)
- self / all / random group target types do not require player selection
- card-level overrides may exist when the card definition explicitly forces targeting behavior

**Valid `targetType` values:** `Self`, `Musician`, `AudienceCharacter`, `AllAudienceCharacters`, `AllMusicians`, `RandomAudienceCharacter`, `RandomMusician`

### 8.3 Composition targeting
Composition cards may require a musician target independently of their effects if the composition semantics require it.

---

## 9. Runtime execution pipeline

### 9.1 Action cards
Canonical sequence:
1. player selects a card from hand
2. runtime resolves performer (`FixedPerformerType`) and target(s)
3. the card executes its `Effects` list via `CardBase.ExecuteEffects()`
4. each effect is interpreted by the runtime branch in `ExecuteEffects`
5. the card moves to the appropriate post-play state (discard/exhaust)

### 9.2 Composition cards
Canonical MVP sequence:
1. player plays a composition card
2. composition/session systems validate and apply composition data to the song model
3. authored `Effects`, if any, may apply immediately as systemic gameplay consequences

Important boundary rule:
- ALWTTT owns the gameplay/runtime meaning of playing the composition card
- MidiGenPlay owns package-side implementation details for internal music generation behavior

---

## 10. UI / description behavior

UI wording and description rendering are secondary to the card contract itself. If description logic changes, it should reflect this SSoT rather than become a competing source of truth.

### 10.1 Card-face text rendering (M1.3a)

`CardEffectDescriptionBuilder` (static class, `ALWTTT.Cards.Effects`) is the single owner of card-effect text formatting. `CardEffectSpec` remains data-only (§6.1) — no virtual `Describe()` method.

`CardDefinitionDescriptionExtensions.GetDescription` delegates the action-card branch to `CardEffectDescriptionBuilder.BuildList(action.Effects, stats)`. The builder handles `ApplyStatusEffectSpec`, `ModifyVibeSpec`, `ModifyStressSpec`, and `DrawCardsSpec`. TMP rich-text tokens: buff `#8FD694`, debuff `#D6858F`, numbers `#FFD084`. Zero-delta effects render as empty strings and are filtered out. Target-type phrasing is centralized.

Composition card faces use a separate path: role/part + modifier count badge. `CardPayload.Effects` on composition cards are not surfaced on the card face (intentional density reduction, 2026-04-21). They are discoverable via card-hover tooltips (§10.2).

### 10.2 Card-hover tooltips (M1.3c)

`CardBase.ShowTooltipInfo()` aggregates tooltips from two sources on pointer enter:

1. `CardDefinition.Keywords` — each keyword resolved against `TooltipManager.SpecialKeywordData`. One tooltip per matched keyword.
2. `CardDefinition.Payload.Effects` — unique `StatusEffectSO` references extracted from `ApplyStatusEffectSpec.status` entries. Dedupe via `HashSet<StatusEffectSO>`. One tooltip per unique SO, showing `DisplayName` as header and `StatusEffectSO.Description` as body.

Display order: keywords first, statuses second. Tooltips follow the mouse cursor. `TooltipController` prefab uses `VerticalLayoutGroup` (Upper Left, spacing 5, ControlChildSize Width+Height) + `ContentSizeFitter` (Preferred Size on both axes) for stacking.

`CardBase` is the assembly point but does not own the data. `StatusEffectSO` owns description text (`SSoT_Status_Effects.md` §3.3). `SpecialKeywordData` owns keyword text.

### 10.3 Card detail modal (M1.10)

`CardDetailViewController` (singleton, `ALWTTT.UI`) manages a dedicated Screen Space – Overlay canvas triggered by right-click (`PointerEventData.InputButton.Right`) on any `CardBase` in hand. The canvas sits at a sort order above the tooltip canvas and is disabled by default.

`CardDefinitionDescriptionExtensions.GetDetailDescription()` owns the detail text:

- **Action cards:** delegates to `CardEffectDescriptionBuilder.BuildList` — identical content to the card face, rendered at modal scale.
- **Composition cards:** multi-line block comprising primary kind + role/action label, style-bundle asset name (Track only), part custom label + musician id (Part only), full modifier list (one line per `PartEffect` via `GetLabel()`, with scope and timing tags), and `CardPayload.Effects` via `CardEffectDescriptionBuilder.BuildList`.

`CardBase.OnPointerDown` discriminates button: right-click calls `CardDetailViewController.Toggle(CardDefinition)`, left-click retains existing behavior. `HandController.DisableDragging()` while modal is open; `EnableDragging()` on dismiss. Dismiss paths: background click (Button on DimBackground Image), Esc key, or right-click toggle on the same card.

The detail modal is the third card-information surface, after card-face text (§10.1) and card-hover tooltips (§10.2). It is the designated home for any composition detail cut from the card face.

---

## 11. Legacy model handling

The project surface still shows legacy `CardData`-style material alongside the newer effect/payload-based model.

Governance rule:
- the **current primary model** is `CardDefinition + CardPayload + CardEffectSpec`
- legacy `CardData`-style material must be treated as:
  - legacy compatibility,
  - transitional coexistence,
  - or archived/superseded material

Legacy material must never silently overrule this SSoT.

---

## 12. Explicit boundaries

### This SSoT owns
- what Action vs Composition means in ALWTTT
- what a card is structurally in gameplay/runtime terms
- how cards express mechanics via effects
- performer and targeting semantics
- the ALWTTT-side meaning of composition-related card choices

### This SSoT does not own
- JSON/editor pipeline details
- catalogue import rules
- package-side composer internals
- lower-level MidiGenPlay algorithm details

Those belong elsewhere even if the same card touches them indirectly.

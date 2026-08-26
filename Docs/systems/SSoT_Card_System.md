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

### 5.2.1 Card → palette bindings (authoritative table)

> **Cantante (Zig) — R3, cerrado 2026-08-08.** Dos comps nuevas, ambas StarterDeck, 1 copia:
>
> | Carta | Músico | Rol | Binding | Ruta |
> | --- | --- | --- | --- | --- |
> | Rise Up | Cantante (Zig) | Melody | `MelodyCardConfig_RiseUp` → `patternOverride: MelodyPattern_RiseUp_44_8m` | `ComposeFromPattern` (verbatim). Los campos `leadingOverride` / `phrasePaletteOverride` / `style` quedan deliberadamente vacíos: el patrón los silencia incondicionalmente. |
> | Showtime | Cantante (Zig) | Melody | `MelodyCardConfig_Showtime` → `phrasePaletteOverride: PhrasePalette_Showtime` + `style: MelodicStyle_Showtime` + `leadingOverride: MelodicLeading-Showtime` | Ruta procedural. `patternOverride` **debe** quedar en null. Operativa desde ST-R3-11 PASS (2026-08-08); la resolución de MGP-MEL-1 no cambió qué campos deben poblarse (B4 verificado, sin cambio). |
>
> **Invariante de autoría (R3).** En un `MelodyCardConfigSO`, `patternOverride` y el
> trío (leading / paleta / estilo) son **mutuamente excluyentes en la práctica**: si
> el patrón está presente, `MelodyTrackComposer` toma la rama `ComposeFromPattern` y
> retorna antes de leer los otros tres, sin aviso. Poblar ambos lados produce una
> carta cuyo authoring aparente no corresponde a lo que suena.

Authoritative game-side table (MidiGenPlay mirrors; PCE-PROP D1=A). Closed at S5g
(2026-07-06), ST-S5g-1..5 PASS.

| Card | Role | Binding asset | Palette / override | Entries |
|---|---|---|---|---|
| Default Mode | Rhythm | `StyleBundles/Rhythm/starter_default_mode_Payload_Rhythm_StyleBundle` | `DrumPatternPalette-FourOnTheFloor` | 6 |
| Waltz Protocol | Rhythm | `StyleBundles/Rhythm/starter_waltz_protocol_Payload_Rhythm_StyleBundle` | `DrumPatternPalette-WaltzLilt` | 6 |
| Pentameter (reward pool) | Rhythm | `StyleBundles/Rhythm/starter_pentameter_Payload_Rhythm_StyleBundle` | `DrumPatternPalette-OddMeterAngular` | 6 |
| Compound Cycle (reward pool) | Rhythm | `StyleBundles/Rhythm/starter_compound_cycle_Payload_Rhythm_StyleBundle` | `DrumPatternPalette-CompoundSwing` | 6 |
| Push It / Half Time | Rhythm (tempo) | payload TrackAction, `styleBundle: null` | **no palette — D-TEMPO=null** (PCE §6 Option A confirmed) | — |
| Wormus Minor | Backing | (unchanged at S5g) | Chord Palette - Core Minor | 6 |
| Wormus Major | Backing | (unchanged at S5g) | Chord Palette - Core Major | 8 |
| Singing Field | Melody | `Melody Configs/Melody Singing Field - Hook` (MelodyCardConfigSO) | `phrasePaletteOverride = PhrasePalette_SingingField` (5 parametric PhraseArchetypes, uniform weights 1.0) | 5 |
| Singing Field (note) | — | — | D-AUTH-1=A: procedural route; MelodyPatternData ×N (route B) noted as a MidiGenPlay backlog candidate (authoring dossier §6.5) | — |

`DrumPatternPalette-SyncopatedPocket` remains **unbound** (1 entry; reserved for
the §5 experiment in `Palette_Card_Identity_Design.md`).

---

## 6. CardEffectSpec model

### 6.1 Base rule
`CardEffectSpec` is the abstract base type for card effects.

Rules:
- specs are **data-only**
- runtime logic is handled by an executor/interpreter layer
- new mechanics are added by creating new spec subclasses plus authoring/runtime support

### 6.2 Built-in effect specs currently in active vocabulary

All six effect types below are implemented and runtime-validated.

| Spec class | JSON `type` | Status | Notes |
|---|---|---|---|
| `ApplyStatusEffectSpec` | `"ApplyStatusEffect"` | ✅ Implemented + validated | Applies a direct `StatusEffectSO` reference |
| `ModifyVibeSpec` | `"ModifyVibe"` | ✅ Implemented + validated | Targets audience characters |
| `ModifyStressSpec` | `"ModifyStress"` | ✅ Implemented + validated | Routes through `ApplyIncomingStressWithComposure` for positive values |
| `DrawCardsSpec` | `"DrawCards"` | ✅ Implemented + validated | Calls `DeckManager.DrawCards(count)` at effect execution time |
| `AddInspirationPerLoopSpec` | `"AddInspirationPerLoop"` | ✅ Implemented + validated (DF-INSPLOOP) | **Not executed by the `CardBase` pipeline.** Consumed at track-binding time: `CompositionSession.EvalPerLoopInsp` reads it off `TrackEntry.sourceCardDefinition` via `AddInspirationPerLoopSpec.SumFor`. Grants `amount` Inspiration/loop while the card's track is active (track-scoped, D-INSP-1=D). Composition Track cards only; inert elsewhere. |
| `RevealPreferencesSpec` | `"RevealPreferences"` | ✅ Implemented + validated (R4, 2026-08-10) | Reveals the target audience member's `TastePreferences` on the audience canvas. Single-target (`AudienceCharacter`), per-gig, idempotent; the spec carries no taste data. Runtime meaning: `SSoT_Audience_and_Reactions.md` §6.4; four-layer conformance + the two-targeting-sites rule: `SSoT_Card_Authoring_Contracts.md` §9 |

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

**Per-spec resolution (R4 correction, F-R4-1, 2026-08-10).** Each spec on a card resolves its **own** target list: `CardBase.ExecuteEffects` calls `DetermineTargets` per spec. There is no shared target list serving all specs on a card. For `All*` targets the result is equivalent (deterministic, same snapshot); for `Random*` it is **not** — two random specs of the same type on one card may resolve to different members. (The R0 verification record previously stated "one shared target list serves all specs on a card"; that wording is corrected here and in the two documents that cite it: `RosterExpansion_Sub_Roadmap.md` §9 and `Design_Starter_Deck_v2_DRAFT.md` §7.)

**`AllAudienceCharacters` excludes `IsBlocked` members (R4).** The `AllAudienceCharacters` branch of `CardBase.DetermineTargets` **excludes** blocked members. Intentional behavior, verified ST-R4-2, not an omission.

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

**Performance-time playability gate (§5.3.5 demo unblock).** Action cards
played during composition performance are gated by
`GigFlowSettingsSO.allowActionCardsDuringPerformance`. As of §5.3.5, when
this flag is ON, **all** action cards in hand become playable during
performance — the prior `CardActionTiming.Always`-only co-condition was
relaxed because per-loop-drawn action cards in the starter deck aren't
all tagged `Always`, and the demo cut needs the broad path. The `Always`
enum value remains in `CardActionTiming` for future precision-gating
needs but does not load-bear in the current gate logic.

### 9.2 Composition cards
Canonical MVP sequence:
1. player plays a composition card
2. composition/session systems validate and apply composition data to the song model
3. authored `Effects`, if any, may apply immediately as systemic gameplay consequences

Important boundary rule:
- ALWTTT owns the gameplay/runtime meaning of playing the composition card
- MidiGenPlay owns package-side implementation details for internal music generation behavior

### 9.3 OnCardPlayed pile transition contract (M4.6F-1, 2026-05-07)

Each successful card play results in **exactly one** `DeckManager.OnCardPlayed(CardBase)` call. The single call site varies by card type:

- **Composition cards.** `HandController.PlayCard` calls `OnCardPlayed` after `TryPlayCompositionCard` (via `GigManager`) returns true. Composition cards do **not** enter `CardBase.Use`.
- **SFX action cards** (`CardType.SFX`). `CardBase.Use` (line 93 of `CardBase.cs`) calls `OnCardPlayed` synchronously inside its SFX branch.
- **Non-SFX action cards.** `CardBase.CardUseRoutine` (line 131) calls `OnCardPlayed` after `ExecuteEffects` yields, so effects resolve before `DiscardRoutine` destroys the GameObject.

Calling `OnCardPlayed` redundantly across these sites doubles the discard: `HandPile.Remove` and `DiscardPile.Add` fire twice on the same `CardBase` instance, removing two distinct entries from `HandPile` because pile multiplicity tracks `CardDefinition` references (asset references) rather than `CardBase` instances. The `IsExhausted`/`IsPlayable` guards in `CardBase.Discard` do not catch the second call because `DiscardRoutine` animates over `discardDuration` before `Destroy(gameObject)`, leaving the GameObject playable during the animation window.

This invariant was implicit in the original architecture and was violated for action cards prior to M4.6F-1 (`HandController.PlayCard:580-581` called `OnCardPlayed` unconditionally on `played == true`, in addition to the per-card-type internal call). Fix: `HandController.PlayCard` gates its `OnCardPlayed` call on `heldCard.CardDefinition.IsComposition`. See `CURRENT_STATE.md` M4.6F-1 closure block.

---

## 10. UI / description behavior

UI wording and description rendering are secondary to the card contract itself. If description logic changes, it should reflect this SSoT rather than become a competing source of truth.

Surfaces: card-face text (§10.1), hover tooltips (§10.2), detail modal (§10.3), card-face visual identity (§10.4), and **playability display** (§10.5 — the unplayable overlay and its single source of truth).

### 10.1 Card-face text rendering (M1.3a)

`CardEffectDescriptionBuilder` (static class, `ALWTTT.Cards.Effects`) is the single owner of card-effect text formatting. `CardEffectSpec` remains data-only (§6.1) — no virtual `Describe()` method.

`CardDefinitionDescriptionExtensions.GetDescription` delegates the action-card branch to `CardEffectDescriptionBuilder.BuildList(action.Effects, stats)`. The builder handles `ApplyStatusEffectSpec`, `ModifyVibeSpec`, `ModifyStressSpec`, `DrawCardsSpec`, and `AddInspirationPerLoopSpec` (DF-INSPLOOP: renders "Gain +N Inspiration each loop while this track plays"). Composition card faces still omit effect text by design (density reduction, 2026-04-21) — the `AddInspirationPerLoop` line is surfaced via the card-hover tooltip (§10.2) and the detail modal (§10.3), not the card face. TMP rich-text tokens: buff `#8FD694`, debuff `#D6858F`, numbers `#FFD084`. Zero-delta effects render as empty strings and are filtered out. Target-type phrasing is centralized.

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

### 10.4 Card-face visual identity (S5b, 2026-06-20)

Two card-face visual cues are toggled in `CardBase.SetCard`, alongside the existing
text label (`typeTextField`):

1. **Action / Composition background.** `CardBase` carries two SerializeField
   `GameObject` references — `actionBackground` and `compositionBackground`.
   `SetCard` → `ApplyTypeBackground(def)` activates exactly one by payload domain:
   `actionBackground` when `def.IsAction`, `compositionBackground` when
   `def.IsComposition`. A card that is **neither** (no payload / a non-card payload)
   shows neither background and falls back to the base card frame
   (**D-S5b-BG-NEITHER = A**). The text type label is preserved; this is an additive
   readability cue, not a new source of domain truth.

2. **Owner icon.** `CardBase` carries a SerializeField `Image` `ownerIconImage`.
   `SetCard` → `ApplyOwnerIcon(def)` resolves it from the card's performer rule
   (§8.1): when `def.FixedPerformerType != MusicianCharacterType.None`, the icon is
   fetched via `GigManager.TryGetMusicianIcon(FixedPerformerType)`, which maps the
   type to `MusicianCharacterData.CharacterIcon` through the **current gig band**
   (`CurrentMusicianCharacterList`). No fixed performer, an unresolved type, or no
   active gig → no icon (the `Image` is hidden). **D-S5b-ICON-RESOLVER = A.**

**Resolver-scope boundary (known limit).** Icon resolution is **in-gig only** — it
reads the live band, so cards shown outside a gig (e.g. the inventory viewer,
`CardUI.prefab`) get no owner icon even when the card has a fixed performer. A global
`MusicianCharacterType → MusicianCharacterData` registry that would cover the
out-of-gig surfaces is **follow-up B**, not implemented (not prioritized).

**Prefab contract.** `actionBackground`, `compositionBackground`, and `ownerIconImage`
are part of the card prefab's wired contract. All three are null-guarded in `CardBase`
and must be wired on **every** card prefab (gameplay prefab + `CardUI.prefab`) — the
same two-prefab recurrence vector recorded for prior `[SerializeField]` additions (see
`CURRENT_STATE.md` §4, "CardUI : CardBase {} two-prefab arrangement"). Wired on both at
S5b close. **DEMO-FIXES-A (2026-07-15, D-DF-5=A)** adds `inspirationCostBadgeRoot` to the
same two-prefab wired contract: an optional root for the Inspiration-**cost** badge, hidden
in `CardBase.SetCard` when `InspirationCost == 0` (symmetric toggle; recovers on pooled reuse
if a cost returns). This mirrors the S5e-ext gen-badge auto-hide (`inspirationGenBadgeRoot`,
hidden at `InspirationGenerated == 0`). If unassigned, only `inspirationCostTextField`'s
GameObject is toggled. Motivation: post-ECON-1 every starter is cost 0
(`SSoT_Gig_Combat_Core.md` §14.6), so the badge was noise on every starter face. Wired on
both prefabs at DEMO-FIXES-A close. ST-DF-10/11 PASS (gameplay + `CardUI` inventory, no NRE).

The domain authority remains `payload.Domain` (§3.2); §10.4 is presentation only. The
play-animation gating that shipped with S5b (only musicians with a track in the played
part animate) is runtime/presentation behavior with **no SSoT home yet** — tracked
operationally in `CURRENT_STATE.md` §4 until a presentation/animation SSoT exists.

### 10.5 Playability display — single source of truth (CARD-UX-1, 2026-07-13)

A card in hand that cannot be played is covered by a **red unplayable overlay**. The overlay
is **advisory display only** — enforcement stays where it already was (the play paths). What
CARD-UX-1 adds is that display and enforcement can no longer disagree, because the display
reads one aggregator instead of re-deriving the rules.

**Authority.** `GigManager.EvaluateCardPlayability(CardDefinition) → UnplayableReason` is the
**only** playability computation for display. It *aggregates* the gates the play paths already
consult; it does not duplicate their logic and it never consumes anything (`CanConsumePlay` /
`CanAffordInspiration`, never `TryConsume` / spend), so per-frame polling is side-effect free.

**Invariant.** No consumer computes playability locally. `HandController` polls the evaluator
and pushes the boolean to `CardBase.SetUnplayableOverlay(bool)`; `CardBase` only renders.
Adding a new reason means adding it to the enum and to the evaluator — never to a consumer.

**Reasons, in evaluation order** (first match wins; the order is the precedence, and it is
deliberate — a tutorial directive outranks a domain rule):

| # | `UnplayableReason` | Gate consulted | Owner doc |
| --- | --- | --- | --- |
| 1 | `TutorialGate` | `TutorialInputGate.BlocksCardDrag` (beat-3 allow-list, beat-5 PlayOnly, beat-8 SingleCardOnly) | `Design_Tutorial_System_v0_2` §4 |
| 2 | `ActionTiming` | `GigManager.CanPlayActionCard` | this SSoT §9.1 |
| 3 | `FinalLoopLock` | `CompositionSession.IsFinalLoopRunning` | `SSoT_Runtime_CompositionSession_Integration` §5.4 |
| 4 | `NoRunningLoop` | `GigManager.CanGrantBonusLoop` (bonus-loop cards only) | `SSoT_Runtime_CompositionSession_Integration` §5.4 |
| 5 | `Resource` | `GigManager.CanPayResourceCost` | `SSoT_Status_Effects` §5.10 |
| 6 | `Inspiration` | `CompositionSession.CanAffordInspiration` | `SSoT_Gig_Combat_Core` §5.1 / §14.6 |
| 7 | `Budget` | `GigManager.CanConsumePlay` (ECON-1) | `SSoT_Gig_Combat_Core` §14 |
| — | `None` | playable | — |

**Precedence note (R5-d).** `NoRunningLoop` and `Resource` are evaluated **before**
`Inspiration` on purpose — a card that cannot exist at this moment (no loop running) or that
has nothing to pay itself with should read as such, not as "not enough inspiration".

**`Budget` is scoped** to cards whose payer is statically resolvable (`FixedPerformerType !=
None`). `AnyMusician` cards and hover-attributed compositions are **excluded** from the
overlay's budget input until **D-ECON-GENERIC** resolves — a false red on a card that *is*
playable against another musician is worse than a false green on an advisory overlay, and the
`TryConsumePlay` drop-denial remains the enforcement. See `SSoT_Gig_Combat_Core.md` §14.5.

**`Resource` is scoped the same way** (R5-d): only cards whose payer is statically resolvable
(`FixedPerformerType != None`) can go red on a resource shortfall. `AnyMusician` cards are
excluded from the overlay's resource input for the identical reason — a false red on a card
that *is* payable by another musician is worse than a false green — and the play-path denial
remains the enforcement. `NoRunningLoop` carries no such scoping: it is a session-state fact,
independent of who would pay.

**Presentation.** The overlay reuses the existing `passiveImage` / `SetInactiveMaterialState`
mechanism (red restyle of the asset). **No new serialized field** — deliberate: a new `Image`
field would have to be wired on both the gameplay card prefab *and* `CardUI.prefab` (the
UI-fix-A recurrence vector, `CURRENT_STATE.md` §4, "CardUI : CardBase {} two-prefab
arrangement"). The minicard/inventory `CardUI` (`IsPlayable = false`) is inert by the same
guard that already protects `SetInactiveMaterialState`.

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
- deck-level multiplicity (multiset shape, runtime expansion, pile-lifecycle invariance under play and reshuffle)
- card-face visual identity (type background + owner icon) and its prefab-wiring contract (§10.4); the owner-icon resolver is in-gig only — a global registry is follow-up B
- playability **display** — the unplayable overlay and its single source of truth (`GigManager.EvaluateCardPlayability` → `UnplayableReason`, §10.5); consumers render, they do not derive. The *rules* aggregated there remain owned by their own SSoTs (tutorial gate, action timing, inspiration, final-loop lock, ECON-1 budget)

### This SSoT does not own
- JSON/editor pipeline details
- catalogue import rules
- package-side composer internals
- lower-level MidiGenPlay algorithm details

Those belong elsewhere even if the same card touches them indirectly.

---

## 13. Deck multiplicity (M4.4)

A band's deck is a multiset of cards, not a set. The data layer authority lives in `BandDeckData`, expressed as `List<BandDeckEntry> entries`, where each entry pairs a `CardDefinition` with an integer `count` ≥ 1. Two entries pointing to the same `CardDefinition` is not the intended shape — the Deck Editor and the JSON importer combine them on save.

**Runtime materialization.** `PersistentGameplayData.SetBandDeck` expands counts into independent references: a count-3 entry contributes three references to `CurrentActionCards` (or `CurrentCompositionCards` for a Composition card). The pre-M4.4 dedup-by-reference rule in `SetBandDeck` is gone; multiplicity is the contract.

**Pile lifecycle preserves identity.** `DeckManager` operates on `List<CardDefinition>` for `DrawPile`, `HandPile`, `DiscardPile`, `ExhaustPile`. `List.Remove(cardDef)` removes the *first* matching reference. Drawing one of three Steady Beat copies into the hand and playing it removes one reference — the other two remain accessible across draw, discard, and reshuffle. Total references across all piles is invariant under play and reshuffle.

**Legacy migration is lazy.** Pre-M4.4 assets serialized a flat `List<CardDefinition> cards` field. That field is preserved as `legacyCards` via `[FormerlySerializedAs("cards")]`. `BandDeckData.Entries` returns the new `entries` list when populated, otherwise materializes a count-1 view from `legacyCards` on access. The Deck Editor's save path writes `entries` and clears `legacyCards`, so an asset upgrades the first time it is saved through the editor. No batch migration script.

**Helper for flat consumers.** `BandDeckData.EnumerateCards()` yields a flat `IEnumerable<CardDefinition>` with multiplicity expanded — used by `GigManager`'s deck-source resolution paths (`RunContextBandDeck`, `Auto` fallback). `SetBandDeck` operates directly on `Entries` because it computes per-domain totals for logging.

**Runtime guarantee cross-reference (M4.5).** The PlayerTurn-entry hand draw guarantees at least one Action and one Composition card in hand when `DrawPile ∪ DiscardPile` allow, without exceeding `DrawCount`. Multiplicity (this section) makes "piles allow" near-universal in practice for the v1 starter, since multiple copies of the same domain card spread across piles after a few turns. The guarantee mechanism itself is owned by `SSoT_Runtime_Flow.md §4.2`.

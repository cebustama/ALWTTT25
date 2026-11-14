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

`CardDefinition.Keywords` is a serialized `List<SpecialKeywords>` that tags a card with player-facing trait and mechanic keywords. Keywords serve two purposes: they generate tooltip entries on card hover (see §10.2), and they will eventually drive runtime behavior (see §3.3.2).

> **Keyword text moved out of this SSoT (TXT-3, 2026-09-10).** Until 2026-09-10 the wording of a keyword lived in `SpecialKeywordData` (`SpecialKeywordBase.contentText`), one language slot, edited only in the Inspector. **TXT-3 moved it to `ConceptGlossarySO`**, one asset per language, keyed by the enum name lowercased (`exhaust`, `vibe`, …), and **retired `SpecialKeywordData` — asset and class**. The `SpecialKeywords` enum is unchanged and remains the id space; it simply no longer needs a registry asset to carry strings. Authority for the wording is `systems/SSoT_Game_Text.md` §1/§3; what a keyword *means* stays here. A keyword with no glossary entry shows its raw id and logs once, and the editor reports it as a coverage gap.

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

`PartActionDescriptor` carries `PartActionKind` and an optional musician id. **`customLabel` was
deleted at CARD-FACE-2 (2026-09-21)**: a part's player label is derived from its `PartActionKind`
(`ui_partaction_*`, `systems/SSoT_Game_Text.md` §1 row 12). `PartActionKind.Custom` still has **no
handler** — declared, not a gap discovered later.

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
> **Cantante (Zig) — R6, cerrado 2026-09-01.** Una comp nueva, StarterDeck, 1 copia:
>
> | Carta | Músico | Rol | Binding | Ruta |
> | --- | --- | --- | --- | --- |
> | Double Harmony | Cantante (Zig) | **Harmony** | `HarmonyCardConfig_DoubleHarmony` — **autorado vacío a propósito** | `HarmonyTrackComposer` con la configuración GLOBAL (`MGP_HarmonicLeadingConfig`). Coste 3, finisher. |
>
> **Invariante de autoría (R6) — el bundle de Harmony es inerte.** A fecha 2026-09-01
> MidiGenPlay **no lee** `HarmonyCardConfigSO`: sus campos no tienen consumidor
> package-side (ítem 3 del ask `MGP-ALWTTT-HARMONY-1`, diferido). El asset debe existir
> —una fila de pista sin bundle no se crea (D4=A)— pero poblar sus campos es **autoría
> fantasma**: produce una carta cuyo authoring aparente no corresponde a nada.
> El carácter de la armonía lo fija en su totalidad `HarmonicLeadingConfig.minDistanceFromMelody`
> (global, valor actual 3). Corolario: **una segunda carta de armonía que suene distinta, o
> niveles de carta sobre el rol, son imposibles** hasta que se pida el ítem 3.
>
> **Precondición de jugada (D-R6-4=A).** Double Harmony se **deniega** si el músico no
> sostiene ya una fila `Melody` propia en la parte destino. Motivo: el composer prefiere la
> melodía del propio músico y sólo cae a «la primera de la lista» si no la encuentra; permitir
> la jugada sin melodía propia devolvería la dependencia de orden de lista que `D-H1-5a=B`
> eliminó, o emitiría un fichero vacío. La regla vive en `SongCompositionUI.TryGetHarmonyDenial`,
> consultada desde el pre-flight (`CanApplyDefinition`) y desde el punto de aplicación.
>
> **Consecuencia sonora registrada.** El `SingerVoiceDirector` filtra por rol `Melody`/`Lead`,
> así que la pista Harmony **no** pasa por la voz articulatoria: suena como instrumento GM
> (sorteado entre los melódicos permitidos del músico, `SongConfigBuilder`). La segunda voz
> *cantada* es Tier B y está diferida a R8.

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

All **seven** effect types below are implemented and runtime-validated. "Active vocabulary" and
"implemented" are the same set; there is no narrower reading, which is the question F-DOC4-1 left
open and TEXT-5 closed by reading the file instead of the path index.

| Spec class | JSON `type` | Status | Notes |
|---|---|---|---|
| `ApplyStatusEffectSpec` | `"ApplyStatusEffect"` | ✅ Implemented + validated | Applies a direct `StatusEffectSO` reference |
| `ModifyVibeSpec` | `"ModifyVibe"` | ✅ Implemented + validated | Targets audience characters |
| `ModifyStressSpec` | `"ModifyStress"` | ✅ Implemented + validated | Routes through `ApplyIncomingStressWithComposure` for positive values |
| `DrawCardsSpec` | `"DrawCards"` | ✅ Implemented + validated | Calls `DeckManager.DrawCards(count)` at effect execution time |
| `AddInspirationPerLoopSpec` | `"AddInspirationPerLoop"` | ✅ Implemented + validated (DF-INSPLOOP) | **Not executed by the `CardBase` pipeline.** Consumed at track-binding time: `CompositionSession.EvalPerLoopInsp` reads it off `TrackEntry.sourceCardDefinition` via `AddInspirationPerLoopSpec.SumFor`. Grants `amount` Inspiration/loop while the card's track is active (track-scoped, D-INSP-1=D). Composition Track cards only; inert elsewhere. |
| `RevealPreferencesSpec` | `"RevealPreferences"` | ✅ Implemented + validated (R4, 2026-08-10) | Reveals the target audience member's `TastePreferences` on the audience canvas. Single-target (`AudienceCharacter`), per-gig, idempotent; the spec carries no taste data. Runtime meaning: `SSoT_Audience_and_Reactions.md` §6.4; four-layer conformance + the two-targeting-sites rule: `SSoT_Card_Authoring_Contracts.md` §9 |
| `GrantBonusLoopSpec` | `"GrantBonusLoop"` | ✅ Implemented (R5-d, 2026-08-26) + runtime-validated (R5-f, `ST-R5f-1..18`, 2026-08-28) | Extends the running part by one loop via `CompositionSession.TryGrantBonusLoop`, capped by `GigFlowSettingsSO.MaxBonusLoopsPerPart`. `soloOverBonusLoop` (default `true`) requests a render-scope soloist track over a byte-identical base (`SSoT_Runtime_CompositionSession_Integration.md` §8 inv 14). **Companion gate:** meaningful only while a part is looping; outside that window the play is denied as `UnplayableReason.NoRunningLoop` (`GigManager.CanGrantBonusLoop`). Authored on `conito_overload` (StarterDeck). Authoring contract and JSON example: `SSoT_Card_Authoring_Contracts.md` §5.6c. **Added at TEXT-5-DOC (2026-09-14), closing F-DOC4-1** — the divergence was detectable from a path index but not resolvable from one: the JSON `type` and the validation state came from reading the file and three other documents |

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
- **`ModifyStressSpec` counts too (S9c, 2026-11-14).** A `ModifyStress` with `targetType` `Musician` or `AudienceCharacter` makes `CardDefinition.RequiresTargetSelection` true, and `HandController.TryResolveCardTarget` resolves `Musician` as a single musician under the cursor (before S9c only `ApplyStatusEffectSpec` was looked at, so a heal-one-musician card had no target). First consumer: **Calm Down** (`crd_sibi_calm_down`, ES «Cálmate», provisional name): Sibi's action card, `ModifyStress −4` to the musician it is dropped on. It replaces Read the Room in Sibi's starter deck (closes D9 of the 12-nov meeting); Read the Room and Zig's *Double Harmony* leave their starter decks but stay unlocked (catalog flags 5 → 4). Data applied by «ALWTTT ▸ S9c ▸ Aplicar datos de la demo» (`S9c_DemoFixDataTool`, idempotent).

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

`CardDefinitionDescriptionExtensions.GetDescription` (`Assets/Scripts/Cards/Extensions/`) delegates the action-card branch to `CardEffectDescriptionBuilder.BuildList(action.Effects, stats)`. **Since TXT-4 (2026-09-11) the builder composes, it does not format.** Every line is a **template fragment** taken from `ConceptGlossarySO.templates` and filled through positional placeholders with the spec's own numbers (`systems/SSoT_Game_Text.md` §1, §5). No English sentence exists anywhere in the builder, and a balance retune changes the rendered line without touching a language column. The set of specs that produce text went from **six to seven** at TXT-4 with the addition of `GrantBonusLoopSpec` (`fx_grant_bonus_loop` / `fx_grant_bonus_loop_solo`, D-TXT4-8) — previously it fell through to the diagnostic fallback and printed its own class name. `AddInspirationPerLoopSpec` keeps its DF-INSPLOOP behaviour ("Gain +N Inspiration each loop while this track plays"). Composition card faces still omit effect text by design (density reduction, 2026-04-21) — the `AddInspirationPerLoop` line is surfaced via the card-hover tooltip (§10.2) and the detail modal (§10.3), not the card face. TMP rich-text tokens: buff `#8FD694`, debuff `#D6858F`, numbers `#FFD084`; the full declared rich-text subset and the sprite-naming rules are `SSoT_Game_Text.md` §2.1. Zero-delta effects render as empty strings and are filtered out. Target-type phrasing is centralized in the `tgt_*` fragments, which by authoring rule **never begin with a definite article** (F-TXT4-5, `SSoT_Game_Text.md` §2.1).

**Two builder details that are deliberate and must not be "cleaned up".** First, the emptiness test is `HasAnyLine`, not a comparison against an English literal: the previous code asked whether the composed string equalled a hardcoded English phrase, which silently answered "not empty" for every non-English build. Second, the fallback for a spec with no template is still `spec.GetType().Name`. That looks like a bug and is a **diagnostic**: a class name on a card face is unmistakable in a playtest, whereas an empty line reads as an intentionally text-free card and survives for months — which is exactly how `GrantBonusLoopSpec` went unnoticed until TXT-4.

Composition card faces use a separate path. `CardPayload.Effects` on composition cards are not surfaced on the card face (intentional density reduction, 2026-04-21); they are discoverable via card-hover tooltips (§10.2).

**Card-face glyphs (CARD-FACE-1, 2026-09-18; CARD-FACE-2, 2026-09-21).** The face speaks in symbols from
the global atlas (`systems/SSoT_Game_Text.md` §2.1) and the card hover explains them:
- **Composition card:** the track role is a `tt_*` glyph **with no word** (D-CF-13=C); then one line per
  modifier, `family glyph + value`, at 60% size in `#B8C4D0` (D-CF-14=B, D-CF-14b). The phrase
  «N modifiers» is no longer emitted — the count survives as repetition of glyphs, and the type is
  gained. A Part card shows its `pa_*` glyph.
- **Action card:** the card type would be a `ct_*` glyph, but **the action card type is not shown to
  the player** (D-CF-25=B): `CardFacePresentation.ShowCardType = false` empties the face field and
  switches off the hover panel and the modal line together. Ids, concepts and glyphs stay live and
  inert; one bool re-enables all three. «Composition» is not a `CardType` and is unaffected.
- **Status names in effect lines** are `st_*` glyphs inside the buff/debuff colour of the line
  (`tint=1`).
- **Any card:** a row of `kw_*` keyword glyphs at the foot of the face (closes F-TXT-3-6).
- **Symbols or words** is a Dev Mode comparison toggle, `CardFacePresentation.UseSymbols`
  (D-CF-19=B, `SSoT_Dev_Mode.md` §3), not a player option.

**One formatter, two readers.** The map «`PartEffect` subclass → family» lives once
(`PartEffectDescriptionBuilder.FamilySpriteName` / `FamilyConceptId`), and so does the value
(`ValueOnly`), because face and modal read the same formatter. A subclass the builder does not map
falls to a visible diagnostic, never to silence; since CARD-FACE-2 every shipped subclass is mapped.

**Consecuencia registrada (R6, 2026-09-01): una carta de composición no tiene canal en su
propia cara para una regla de jugabilidad.** `CardDefinition` no lleva campo de descripción y
las caras de composición omiten el texto de efecto por la reducción de densidad de arriba. Cuando
una carta de composición tiene una **precondición** —Double Harmony exige una fila Melody propia
en la parte (§5.2.1, D-R6-4=A)— el jugador no puede leerla en la carta. El único canal es el
**aviso de denegación** (`GigMessageUI`, D-R6-7). Toda precondición nueva de carta de composición
debe, por tanto, o venir con su texto de denegación, o aceptar ser invisible.

**Regla de la fila del panel de canción (SONG-PANEL-1, 2026-09-14, D-SP1-1=A).** La etiqueta de una
fila de pista es **el nombre de carta del glosario** (`GameText.CardName`), y se escribe **una sola
vez**: al crear la pista. Ningún efecto la reescribe después. La regla nace de **F-T6-8**, donde un
`InstrumentEffect` reasignaba `TrackEntry.info` a `"{styleBundle.name} - {instrumentName}"` y la
fila acababa mostrando un nombre de asset (`BassCard_W2_Improv - Fingered Bass`) mientras la
minicard de al lado mostraba `Walking Bass`.

Lo que se corrigió no fue la redacción sino **la existencia de un segundo escritor** de un campo que
ya tenía dueño. Por eso el arreglo es una supresión y no una condición: cualquier variante que deje
viva una segunda escritura —condicionada, formateada o traducida— conserva la clase de fallo, porque
el siguiente efecto que quiera «reflejar algo» en la fila tomará el mismo camino. El instrumento
efectivo ya tenía campo propio (`overrideMelodicInstrument`) y lector propio (el hover de la fila),
de modo que la reasignación era además redundante. La verificación que importa es la **segunda**
aplicación sobre la misma pista (ST-SP1-2): es donde un arreglo con guarda habría fallado.

### 10.2 Card-hover tooltips (M1.3c)

`CardBase.ShowTooltipInfo()` aggregates tooltips from two sources on pointer enter. Since **TXT-3 (2026-09-10)** both sources take their text from the same place — the active `ConceptGlossarySO`, via `ConceptTooltipResolver`:

1. `CardDefinition.Keywords` — one tooltip per keyword, resolved by enum name (`TryGetKeywordText`). Behaviour change from M1.3c: a keyword with no entry no longer skips silently, it shows its raw id and logs once.
2. `CardDefinition.Payload.Effects` — unique `StatusEffectSO` references extracted from `ApplyStatusEffectSpec.status` entries. Dedupe via `HashSet<StatusEffectSO>`. One tooltip per unique SO, resolved by `StatusKey` (`TryGetStatusText`).

Display order: keywords first, statuses second. Tooltips follow the mouse cursor. `TooltipController` prefab uses `VerticalLayoutGroup` (Upper Left, spacing 5, ControlChildSize Width+Height) + `ContentSizeFitter` (Preferred Size on both axes) for stacking.

**One `ShowTooltip` per entry is deliberate here.** This surface stacks panels on purpose (M1.3c) and is *not* governed by the single-panel hover contract of `SSoT_Game_Text.md` §4, which applies to concept tags in the tutorial overlay. TXT-3 changed the source of the text and nothing about the assembly. **TIP-1 (2026-09-15) made that deliberateness a contract instead of a convention:** `ShowTooltip` is declared a **push** — N calls show N panels — in `systems/SSoT_Tooltip_Pipeline.md` §2 and in the hard invariant `TOOLTIP-SHOW-IS-A-PUSH`. Before TIP-1 the stacking was the *side effect* of not calling `HideTooltip`, and it was on record as a defect (F-BN-7) because a live-updating surface tripped over it; the fix gave that surface its own entry (`UpdateTooltip`) rather than removing the stacking this one depends on. `CardBase` was not edited and its assembly is unchanged: the guarantee is that a future batch cannot "fix" the stacking without breaking a written invariant. The transport — where the column is placed and how a panel is measured — is governed there; what the panels say stays here and in `SSoT_Game_Text.md`.

**Stack after CARD-FACE-1/2 (code order, `CardBase.ShowTooltipInfo`):** track role (Track cards) →
card type (Action cards, only while `ShowCardType`) → part action (Part cards) → modifier families
(**deduplicated by family**: on the face repetition is information, in the stack it would be two
identical panels) → keywords → statuses. Role, card type, part action and families are read from the
glossary by **derived concept id** through `GameText.ConceptName/ConceptDescription`, not through
`ConceptTooltipResolver`, which keys by `StatusKey` and keyword name — adding a case there would have
been a change to the tooltip pipeline, out of scope by D-CF-2. Unmapped `PartEffect` subclasses add no
panel.

**Header rule (D-CF-26=A, CARD-FACE-2b).** Every card-hover panel that explains a glyph on the face
**repeats that glyph before its name** (`CardFacePresentation.HeaderWithGlyph`) — role, modifier
families, part action, keywords, statuses, and card type when switched on. It follows `UseSymbols`.
It is consumer-side: the tooltip pipeline does not change. **Status-icon tooltips on a character do
NOT carry the glyph, on purpose**: the reader is already pointing at the icon. Known defect: a long
name splits glyph and name across lines (**F-CF2b-1**, CLOSED 2026-09-22 by CARD-FACE-3). Two
changes, both needed: `HeaderWithGlyph` joins glyph and name with **U+00A0** (written as an escape,
the source stays ASCII), and the panel's `Preferred Width` went 250 → **280** (`SSoT_Tooltip_Pipeline`
§2). The space alone was not enough and the batch observed why: once glyph, space and word are one
unbreakable unit, a unit wider than the line has no legal break point, so TMP broke it **by
character** instead — «Acompañamient / o» (ST-CF3-6 FAIL before the width change, PASS after).

`CardBase` is the assembly point but does not own the data — the glossary does (`systems/SSoT_Game_Text.md` §1). `StatusEffectSO` and the `SpecialKeywords` enum supply **identity** (`StatusKey`, enum name), not strings.

**Card names and descriptions were closed by TXT-4 (2026-09-11) — F-TXT-3-5 CLOSED.** Card names moved into `ConceptGlossarySO.cardNames` (60 ids, one entry per language) and are resolved by `GameText.CardName(definition)`. `CardDefinition.displayName` is now a **developer label** on exactly the same terms as `StatusEffectSO.displayName` since TXT-3: inspector lists, logs, editor tooling, asset identification — **no player surface may read it**. Card descriptions gained per-language storage without gaining stored sentences: the glossary holds 47 fragments and the builder composes (§10.1). **Three surfaces still read the developer label and are open defects, not exceptions** (**F-TXT4-3** → TEXT-5): the played-card floater (`SensoryFtPresentation`), this document's §10.3 modal header, and the song-panel track label (`SongCompositionUI`). Structural home for all of it: `systems/SSoT_Game_Text.md` §1. Card descriptions remain untagged: concept tags inside a composed fragment are possible but out of scope until a later batch.

### 10.3 Card detail modal (M1.10)

`CardDetailViewController` (singleton, `ALWTTT.UI`) manages a dedicated Screen Space – Overlay canvas triggered by right-click (`PointerEventData.InputButton.Right`) on any `CardBase` in hand. The canvas sits at a sort order above the tooltip canvas and is disabled by default.

`CardDefinitionDescriptionExtensions.GetDetailDescription()` owns the detail text:

- **Action cards:** delegates to `CardEffectDescriptionBuilder.BuildList` — identical content to the card face, rendered at modal scale.
- **Composition cards:** multi-line block comprising primary kind + role/part-action (**glyph + word**, D-CF-13=C), full modifier list (one line per `PartEffect` composed by `PartEffectDescriptionBuilder` — **not** `GetLabel()`, which is a developer label since D-CF-5=B — with scope and timing as glyph + concept `displayName`), and `CardPayload.Effects` via `CardEffectDescriptionBuilder.BuildList`. Style-bundle asset name (Track) and musician id (Part) are formatted **only under `ALWTTT_DEV`** (F-CF-2: one datum, one policy with the track hover). Meter, tempo and BPM use the context row's ids (`ui_ctx_*`), so the modal and the strip cannot disagree.
- **Action cards, type line:** hidden with the face (D-CF-25=B); with the toggle on, glyph + word.

**Why the modal pairs glyph and word while the face shows the glyph alone.** The face has a hover to
explain its glyphs; the modal has none, because no card surface reads `<link>` (F-CF-5). A glyph with
no hover and no word is unreadable.

**O-CF2b-2 — CLOSED 2026-09-22 by CARD-FACE-3, cause confirmed by observation.** Of the two
candidates recorded at 2b close, the first was right and the second was **refuted**: the canvas order
is exactly what §4 states (tooltip canvas 10, detail canvas 100, both root Screen Space – Overlay;
inspector-verified 2026-09-22), and the panel was drawn **below** the dim background, not above it —
the original wording «above the modal» was imprecise. The real mechanism is uGUI's pointer capture:
the RELEASE is delivered to the object that took the PRESS, even when the pointer has since left it.
A right press hid the stack and opened the modal; the pointer then moved onto `DimBackground`, firing
`OnPointerExit` (a second, correct hide); and the release still reached the card, whose `OnPointerUp`
called `ShowTooltipInfo()` unconditionally. With the pointer over the modal, nothing would ever hide
it again. **Rule now in `CardBase.OnPointerUp`:** re-show only on a **left** release and only while
`CardDetailViewController.Instance.IsVisible` is false. ST-CF3-1..5 PASS, the last two being the
regressions for the left click and for plain hover.

`CardBase.OnPointerDown` discriminates button: right-click calls `CardDetailViewController.Toggle(CardDefinition)`, left-click retains existing behavior. `HandController.DisableDragging()` while modal is open; `EnableDragging()` on dismiss. Dismiss paths: background click (Button on `DimBackground` Image) and **Esc**. Esc is **not read by
this class**: since CARD-FACE-3 (D5=B) it has a single runtime listener, `UIManager.HandleEscapeKey`,
which closes this modal first and only then falls through to its scene rule (return to main menu, or
quit if already there). Before that, both classes polled Esc in their own `Update`, so one
press closed the modal AND abandoned the gig, in an order Unity does not define; a guard on either
side would have been a race that no execution order fixes (**O-CF3-2**, CLOSED 2026-09-22; ST-CF3-13,
14, 15 PASS). **Since PAUSE-1 (2026-09-25) the chain has four layers**, topmost first: (1) this modal; (2) any open
Options panel (`OptionsPanelController.TryCloseTopmost`, pause-hosted or Main-Menu-hosted — so Esc on the
Main Menu with Options open closes Options instead of quitting); (3) the pause menu (cancel its confirm →
resume → open the pause, in a gig only); (4) the scene rule, which now only runs where no layer consumed
the key. ST-CF3-13 and ST-CF3-15 re-run as ST-P1-8 / ST-P1-9, PASS. While paused, `HandController.Update`
also returns early on `PauseGate.IsPaused` — a gate **orthogonal** to `IsDraggingActive`, which already has
three owners (this modal, the tutorial modal gate, `GigManager.SetHandVisible`) and must not get a fourth.

**O-CF3-3 — two branches of `Toggle` are unreachable, ACCEPTED as-is (D7, 2026-09-22).** While the
modal is open, `DimBackground` captures every raycast, so no `CardBase` receives a right-click:
neither «visible + same card → hide» nor «visible + different card → swap» can fire (observed,
ST-CF3-16). The modal is modal in the strict sense. The code is left in place — it is correct if the
dim ever stops blocking — but this document no longer claims those gestures work.

**Text sources of the modal (TEXT-5, 2026-09-11 — F-TXT4-3 closed).** Three of its strings resolve through the glossary and none of them through a developer label: the **header** via `GameText.CardName(definition)`; the **type label** of a composition card via the `ui_card_type_composition` template (D-T5-3=A — the same id `CardBase.typeTextField` uses, so face and modal cannot drift); and the **keyword line** via `GameText.ConceptName` per keyword (D-T5-4=A), which replaced `Keywords[i].ToString()` — an enum name on a player surface that already had a mandatory glossary entry, so the modal read «Tall» while the hover read «Alto». The defect this paragraph recorded until TXT-4 (modal header «Double Harmony» over a card face «Doble Armonía», ST-TXT4-12) is gone. **Still an enum name here, by design:** the type label of an *Action* card, which is F-TXT4-2 → CARD-FACE-1 and explicitly outside `GAMETEXT-NO-DEV-LABEL` (`ssot_manifest.yaml`). **Unverified path:** no card in the catalogue carries a keyword, so the keyword line is live and empty — ST-T5-5 is **deferred, not passed** (`SSoT_Game_Text.md` §7, F-TXT-3-6).

The detail modal is the third card-information surface, after card-face text (§10.1) and card-hover tooltips (§10.2). It is the designated home for any composition detail cut from the card face.

**The modifier lines are an uncensused text producer (F-TV-7, TEXT-VIBE-1, 2026-09-16).** The
`GetLabel()` of each `PartEffect` — named above as the source of one line per modifier — emits English
that reaches the player: `golden D` shows `Meter 5/4`, `Tempo: Slow`, `Mode Ionian`,
`Mod → degree Mediant ↑`, `Random Instrument (2)`, `Set Instrument: Fingered Bass`. The **enum half**
(`5/4`, `Ionian`, `Slow`) is already F-TXT4-2 and its fix is symbolic; the **literal half** (`Meter`,
`Tempo:`, `Mode`, `degree`, `Random Instrument`, `Set Instrument:`) is words written in the effect
classes, a location no text census has visited. It also creates a **second home for one concept**: the
composition strip's context row prints `5/4` from a glossary template while this modal prints
`Meter 5/4` from code, which is the divergence `systems/SSoT_Game_Text.md` D-T6-3=B exists to prevent.
Assigned to **CARD-FACE-1**, which must read the effect classes before counting anything — whether
these are literals or `ToString()` is **not** established code truth today. **CLOSED 2026-09-18 by
CARD-FACE-1** (census: seven classes; the modal now composes through `PartEffectDescriptionBuilder`;
see the composition bullet above).

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
| 1 | `TutorialGate` | `TutorialInputGate.BlocksCardDrag` (beat-3 allow-list, beat-5 PlayOnly, beat-8 SingleCardOnly) | `Design_Tutorial_System_v0_3` §4 (v0_2 until 2026-09-04) |
| 2 | `ActionTiming` | `GigManager.CanPlayActionCard` | this SSoT §9.1 |
| 3 | `FinalLoopLock` | `CompositionSession.IsFinalLoopRunning` | `SSoT_Runtime_CompositionSession_Integration` §5.4 |
| 4 | `NoRunningLoop` | `GigManager.CanGrantBonusLoop` (bonus-loop cards only) | `SSoT_Runtime_CompositionSession_Integration` §5.4 |
| 5 | `Resource` | `GigManager.CanPayResourceCost` | `SSoT_Status_Effects` §5.10 |
| 6 | `Inspiration` | `CompositionSession.CanAffordInspiration` | `SSoT_Gig_Combat_Core` §5.1 / §14.6 |
| 7 | `Budget` | `GigManager.CanConsumePlay` (ECON-1) | `SSoT_Gig_Combat_Core` §14 |
| — | `None` | playable | — |

**Enforcement now agrees with this order (R5-g, 2026-08-29).** Until R5-g the action play path
ran its resource gate **after** ECON-1, i.e. in the opposite order to rows 5 and 7 of this table,
and — because the R5-b Voltage grant fires inside `TryConsumePlay` — it compared against a
post-grant balance. The overlay (which calls the non-consuming `CanPayResourceCost` and therefore
sees the pre-grant balance) said red while the drop path allowed the play: both were internally
right and they disagreed, which is precisely the failure mode CARD-UX-1 exists to remove.
`HandController` step **2b.5** now performs the same non-consuming check ahead of the budget gate.
The overlay's precedence is unchanged; what changed is that execution follows it.

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

**Denials now carry a message (TUT-REDESIGN-B, 2026-09-03).** The overlay is advisory; the
*denial* is what the play path says when the drop is refused. Since this batch **every action-card
denial shows its reason on screen**: timing (`ActionTiming`), inspiration (via the existing flash,
**no text**), bonus-loop precondition (`NoRunningLoop`), resource cost (`Resource`) and ECON-1
(`Budget`). Before this batch the action path was **silent at every gate** — `GigMessageUI`
(D-R6-7) covered composition only. All of them go through the single funnel
`GigManager.ReportPlayDenied` (`SSoT_Gig_Combat_Core.md` §14.4), which shows the text and publishes
`PlayDeniedEvent` with a typed `PlayDenyReason`; the tutorial routes on that reason
(`Design_Tutorial_System_v0_3.md` §3.1). Rule for new gates: a gate that denies without calling
the funnel is a defect, not a style choice — the overlay and the message must agree with the
enforcement, and the funnel is how they do.

**The reason is shown on hover (GIG-UI-1, 2026-11-03).** Hovering a red card pushes a «No se puede
jugar» panel (`TooltipKind.Warning`, red header) whose body is `ui_unplayable_<reason>`. `Budget` is
split by the card's domain into «Este músico ya usó una carta de acción / composición este turno»
(`ui_unplayable_budget_action` / `_composition`): naming *which* play was spent is what lets the
player read the ECON-1 rule without any other UI. **D-UI-3=A:** the per-musician play pips
(`BandCharacterCanvas` `actionPlayPip` / `compositionPlayPip`, under `PlayBudgetRoot`) are
**deactivated in the prefab**; the code still writes their alpha, harmlessly. Only the visual
changes — the ECON-1 rule, its enforcement and the overlay are untouched (`SSoT_Gig_Combat_Core.md`
§14). Re-evaluate in DEMO-1211 S4 (LEGIB-1b) whether the player needs the remaining budget visible
before trying a card. **Reviewed and closed in S4b (2026-11-11, D2=A): the pips stay hidden.**

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

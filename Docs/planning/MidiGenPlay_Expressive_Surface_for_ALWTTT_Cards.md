# MidiGenPlay Expressive Surface for ALWTTT Cards

**Status:** Planning / reference — **NOT** a governed SSoT.
**Scope:** The observable musical expressive surface available to ALWTTT composition cards, mapped against MidiGenPlay package contracts.
**Purpose:** One reference document that answers "what can a composition card affect musically, and how does that change reach audible output". Supports card design, starter-deck authoring decisions, and roadmap prioritization of integration work.
**Does not own:**
- MidiGenPlay composer algorithms (package-internal)
- ALWTTT card system structure (see `systems/SSoT_Card_System.md`)
- Governance of the integration boundary (see `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md`)
- Card-authoring contracts (see `systems/SSoT_Card_Authoring_Contracts.md`)

**Related governed docs:** listed in §9.

---

## 1. Purpose and reading order

When designing a composition card, the first design question is usually practical and specific: *what levers does this card push, and what will a listener actually hear?*

Answering that today requires reading across three bodies of truth:

1. The ALWTTT composition-card payload (`CompositionCardPayload`, `TrackActionDescriptor`, `PartActionDescriptor`, `List<PartEffect> modifierEffects`).
2. The handoff payload (`SongConfig` + `TrackParameters`).
3. The MidiGenPlay-side consumers (composers, style bundles, tonality profiles, progression palettes).

This document consolidates that view in one place. It is **reference material** and does not redefine any contract.

Recommended reading order: §3 (the matrix) → §4 (precedence) → §5 (per-role contracts) → §6 (cross-track emergent behavior). §7–§8 are for design / gap-tracking.

## 2. Not in scope

- Redefining contracts owned by governed SSoTs cited in §9.
- Composer-internal algorithms.
- Cards' gameplay-side effects (`CardPayload.effects`). Those are standard card runtime; see `systems/SSoT_Card_System.md`.
- Specific starter-deck card authoring decisions. Those belong to `Design_Starter_Deck_v1.md` and related planning.

---

## 3. Expressive axes — the primary matrix

This is the core reference. Each row is an independent musical axis a card can potentially control.

Legend for **"Per-card today?"**:
- ✅ = card can address this axis via an existing contract.
- ⚠ = card can address it via a path that exists but is not fully closed in runtime semantics (see §8).
- ❌ = no per-card channel today (see §8 for the gap).

| # | Axis | ALWTTT carrier | Reaches audio via | Package SO consumed | Per-card today? |
|---|---|---|---|---|---|
| 1 | Tonality (mode enum) | `TonalityEffect` (PartEffect) | `PartConfig.Tonality` | resolved internally → `TonalityProfileSO` lookup | ✅ |
| 2 | Tonality (rich profile / vamps / weights) | — | — | `TonalityProfileSO` (resolved via `ctx.GetTonalityProfileForPart`) | ❌ (gap §8.1) |
| 3 | Root note | probably effect (schema not confirmed) | `PartConfig.RootNote` | — | ⚠ (confirm) |
| 4 | Tempo (range) | `TempoEffect` Range mode | `PartConfig.TempoRange` | — | ✅ |
| 5 | Tempo (absolute BPM) | `TempoEffect` AbsoluteBpm mode | `PartConfig.ExplicitBpm` | — | ✅ |
| 6 | Tempo (multiplicative) | `TempoEffect` ScaleFactor mode | `PartConfig.TempoScale` | — | ✅ |
| 7 | Meter / time signature | `MeterEffect` | `PartConfig.TimeSignature` | `TimeSignatureProperties` (package) | ✅ |
| 8 | Modulation (key change) | `ModulationEffect` | `PartConfig.RootNote` / scale-relative | — | ✅ |
| 9 | Instrument (specific melodic) | `InstrumentEffect` SpecificMelodic | `TrackConfig.Instrument` | `MIDIInstrumentSO` | ✅ |
| 10 | Instrument (specific percussion) | `InstrumentEffect` SpecificPercussion | `TrackConfig.PercussionInstrument` | `MIDIPercussionInstrumentSO` | ✅ |
| 11 | Instrument (by type) | `InstrumentEffect` InstrumentType | `TrackConfig.Instrument` (resolved) | — | ✅ |
| 12 | Chord progression (explicit) | `TrackActionDescriptor.styleBundle` as `BackingCardConfigSO` | `TrackParameters.Style` | `ChordProgressionData` (progressionOverride) | ✅ |
| 13 | Chord progression (palette) | `TrackActionDescriptor.styleBundle` as `BackingCardConfigSO` | `TrackParameters.Style` | `ChordProgressionPaletteSO` | ✅ |
| 14 | Voice leading | `BackingCardConfigSO.voiceLeadingOverride` | via Style | `VoiceLeadingConfig` | ✅ |
| 15 | Drum pattern (explicit asset) | `RhythmCardConfigSO.patternOverride` via styleBundle | `TrackParameters.Style` | `DrumPatternData` | ✅ |
| 16 | Rhythm recipe (procedural) | `RhythmCardConfigSO.recipeOverride` via styleBundle | `TrackParameters.Style` / `.RhythmRecipe` | — | ✅ |
| 17 | Rhythm density/feel fields | `RhythmCardConfigSO.{kickDensity, snareGhostNoteChance, hatSubdivisionBias, fillEveryNMeasures, lastMeasuresAsFill}` | `TrackParameters.Style` | — | ⚠ (gap §8.5) |
| 18 | Feel (straight/swing/shuffle/laid-back/push) | `FeelEffect` (PartEffect, inline in PartEffect.cs) | unknown — not observed consumed by composers | — | ⚠ (gap §8.4) |
| 19 | Density (sparsity) | `DensityEffect` (PartEffect, inline) | unknown — not observed consumed by composers | — | ⚠ (gap §8.4) |
| 20 | Rhythm style id | `RhythmCardConfigSO.styleIdOverride` via styleBundle | `RhythmStyleRegistry.ChooseById` | — | ⚠ (gap §8.3) |
| 21 | Melody strategy | `MelodyCardConfigSO.style` → `MelodicStyleSO.baseStrategy` | `TrackParameters.Style` | `MelodicStyleSO` | ✅ |
| 22 | Per-phrase melodic directives | `MelodyCardConfigSO.style` → `MelodicStyleSO.perPhraseDirectives` | via Style | `WeightedPhraseDirective`, `InterPhraseIntervalDirective` | ✅ |
| 23 | Phrase palette (archetypes) | `MelodyCardConfigSO.phrasePaletteOverride` | via Style | `PhrasePaletteSO` → `PhraseArchetypeSO` | ✅ |
| 24 | Melodic leading | `MelodyCardConfigSO.leadingOverride` | via Style | `MelodicLeadingConfig` | ✅ |
| 25 | Harmony strategy | `HarmonyCardConfigSO.strategyIdOverride` via styleBundle | `TrackParameters.Style` | — | ✅ |
| 26 | Harmonic leading | `HarmonyCardConfigSO.leadingOverride` | via Style | `HarmonicLeadingConfig` | ✅ |

### 3.1 Reading the matrix

- Rows 1, 4–11 are **`PartEffect`-driven** axes (atomic, one effect = one axis). These mutate `PartConfig` directly.
- Rows 12–17, 20–26 are **`styleBundle`-driven** axes. One bundle per card per role; multiple fields inside the bundle are applied together. These mutate `TrackParameters.Style`.
- Rows 18–19 are `PartEffect` assets whose audible consumption is not observed in the current composer code.

The split between PartEffect (Part-scoped) and styleBundle (Track-scoped) is not arbitrary: it mirrors the split in `SongConfig` between `PartConfig.*` fields (shared across all tracks of the part) and `TrackConfig.Parameters.*` fields (per-track).

---

## 4. Resolution precedence, by role (observable from code)

The precedences below are derived from reading the composer `.cs` files directly. They are **current behavior**, not design intent. If the code changes, this section is stale and must be re-read.

### 4.1 Chord progression resolution (ChordTrackComposer)

Observed order (highest → lowest priority):

1. `BackingCardConfigSO.progressionOverride` — single explicit asset wins outright.
2. `BackingCardConfigSO.progressionPalette.PickRandomProgression(rng)` — weighted palette pick (TS-aware by default).
3. `ctx.GetProgressionForPart(part)` — previously cached for this part (shared from another track's earlier decision).
4. `cfg.Parameters.Pattern as ChordProgressionData` — legacy pattern surface on TrackParameters.
5. `ctx.Settings.progressionLibrary` template pick (`PickTemplateForPart(profile, lib)`).
6. `BuildProceduralProgressionWithProfile` — profile-aware procedural generation.
7. `BuildDegreeWeights` — pure procedural fallback without profile.

Once any of 1–6 resolves, the result is **shared with other tracks** via `ctx.SetProgressionForPart(part, prog)` (see §6.1).

### 4.2 Rhythm resolution (RhythmTrackComposer)

Observed precedence for pattern data:

1. `RhythmCardConfigSO.patternOverride` (DrumPatternData).
2. `TrackParameters.Pattern as DrumPatternData`.

Observed precedence for recipe:

1. `RhythmCardConfigSO.recipeOverride`.
2. `TrackParameters.RhythmRecipe`.

If no pattern resolves, procedural path runs:

1. `RhythmCardConfigSO.styleIdOverride` (if non-empty) → `RhythmStyleRegistry.ChooseById`.
2. Otherwise `RhythmStyleRegistry.Choose(timeSignature, recipe, ...)`.

Fills are governed by `RhythmCardConfigSO.fillEveryNMeasures` and `.lastMeasuresAsFill` at the bundle level (semantic closure pending — gap §8.5).

### 4.3 Melody resolution (MelodyTrackComposer)

The melody composer reads `TrackParameters.Style as MelodyCardConfigSO` and then, inside, resolves:

- `leadingOverride` → falls back to default `MelodicLeadingConfig` if null.
- `phrasePaletteOverride` → wins over palette inside leading if set.
- `style` → provides `MelodicStyleSO` (base strategy + per-phrase directives).

`MelodicStyleSO.usePerPhraseOverrides` switches on the `WeightedPhraseDirective` list, where each directive can override strategy, constrain contour (None / AscendingOnly / DescendingOnly), repeat last notes, or apply inter-phrase interval patterns.

### 4.4 Tonality resolution

1. `PartConfig.Tonality` (set by `TonalityEffect` or default).
2. If a Backing progression with a restricted `tonalities` list is chosen, and the part tonality is not in it, the composer may **override `part.Tonality`** at generation time to align with the progression (see §6.2).
3. Regardless of the enum, the rich `TonalityProfileSO` is resolved internally via `ctx.GetTonalityProfileForPart(part)` — not reachable per-card today (gap §8.1).

### 4.5 Instrument resolution

`TrackConfig.Instrument` / `.PercussionInstrument` are set at build time by `SongConfigBuilder` based on ALWTTT-side model state, which in turn is affected by `InstrumentEffect` on played cards. Package-side does not re-resolve instruments.

---

## 5. Per-role bundle contracts

For each `TrackRole`, exactly one `TrackStyleBundleSO` subclass is the canonical carrier. A card targeting a given role attaches that bundle via `TrackActionDescriptor.styleBundle`.

### 5.1 Rhythm — `RhythmCardConfigSO`

Consumed by `RhythmTrackComposer`. Carries drum-specific identity:

- `patternOverride: DrumPatternData` — authored grid/pattern asset.
- `recipeOverride: RhythmRecipe` — procedural high-level recipe (style id, hat density).
- `styleIdOverride: string` — force a specific style id (semantic not fully closed — gap §8.3).
- `fillEveryNMeasures: int`, `lastMeasuresAsFill: int` — fill cadence controls (semantic not fully closed).
- `kickDensity`, `snareGhostNoteChance`, `hatSubdivisionBias` — continuous feel fields (semantic not fully closed).

Expressiveness note: using `patternOverride` gives the most deterministic rhythmic identity today. Procedural path via recipe/styleId gives variety but is the most work-in-progress area of the rhythm runtime.

### 5.2 Backing — `BackingCardConfigSO` — canonical home for chord progressions

Consumed by `ChordTrackComposer`. This is the most important bundle for harmonic identity:

- `progressionOverride: ChordProgressionData` — single explicit progression.
- `progressionPalette: ChordProgressionPaletteSO` — weighted set, picked with TS-awareness.
- `voiceLeadingOverride: VoiceLeadingConfig` — voicing rules for generated voicings.

The pick logic (`PickProgressionOverride`) is already TS-aware and supports a two-tier policy via `ChordProgressionPaletteSO.preferExactTsMatches`.

**Design consequence:** for a chord-playing musician like Sibi, the carrier of "this card sounds minor / major / blues" is a `BackingCardConfigSO` with a curated `progressionPalette`. The `TonalityEffect` is additive (constrains the mode) but the progression palette is where genre-specific identity lives.

### 5.3 Harmony — `HarmonyCardConfigSO` — thin by design

Consumed by harmony-layer code. Carries:

- `leadingOverride: HarmonicLeadingConfig`.
- `strategyIdOverride: HarmonyStrategyId`.

This bundle is deliberately thin because `TrackRole.Harmony` in the package is "how to harmonize an existing line", not "what progression to play". The progression home is Backing. Previous drafts of this analysis wrongly flagged this bundle as a gap; the gap was in my mapping, not in the bundle.

### 5.4 Melody — `MelodyCardConfigSO`

Consumed by `MelodyTrackComposer`. The richest bundle:

- `leadingOverride: MelodicLeadingConfig`.
- `phrasePaletteOverride: PhrasePaletteSO` — wins over the palette referenced by leading if set.
- `style: MelodicStyleSO` — base strategy + per-phrase directives.

The combined expressiveness of `MelodicStyleSO.perPhraseDirectives` + `PhrasePaletteSO.archetypes` is substantial: contour constraints, inter-phrase interval patterns, motif repetition, archetype selection. This is where a melody card can express distinct character without changing tonality or progression.

---

## 6. Cross-track emergent mechanics

These are behaviors that arise from how composers coordinate via `GenContext`, not from any single card or bundle.

### 6.1 Progression is shared across tracks of the same Part

After `ChordTrackComposer` resolves a progression, it calls `ctx.SetProgressionForPart(part, prog)`. `MelodyTrackComposer` and bass/harmony tracks can then read `ctx.GetProgressionForPart(part)` during their own generation.

**Design consequence:** a single Backing card that selects a progression palette effectively coordinates Backing + Melody + Bass simultaneously on the same harmonic grid. One card, multi-track coherence. This is precisely the property needed for genre-pack cards without requiring multiple cards to be coordinated by the player.

### 6.2 Progression can override `part.Tonality`

If a chosen progression has a non-empty `tonalities` list that excludes the current part tonality, `ChordTrackComposer` picks a compatible tonality from the progression and mutates `part.Tonality` in place.

**Design consequence:** a palette authored with `tonalities = [Aeolian]` on each entry will pull the part into Aeolian even if the `TonalityEffect` set it to Ionian. Progression can be the **dominant** axis, not the tonality enum. Useful, but needs explicit awareness when designing cards that also set tonality.

### 6.3 `TonalityProfileSO` flows through to `PhraseArchetype`

`PhraseArchetypeSO.Build(...)` takes a `TonalityProfileSO profile` parameter. The same profile resolved for the chord composer reaches the phrase planner that decides melodic phrasing.

**Design consequence:** if and when §8.1's gap is closed (per-card tonality profile injection), the effect will be felt pervasively — chords, melody contour, phrase shape — from a single point of mutation.

---

## 7. Identity packs — building blocks and reference prototype

A common design question is: *can a single card express "minor blues" or "synthwave" holistically?* The building blocks for that exist; there are two current routes and one deferred route.

### 7.1 Atomic building blocks

The identity-defining SOs in the package are:

- `TonalityProfileSO` — modal identity (characteristic degrees, cadence rules, vamps).
- `ChordProgressionPaletteSO` — harmonic vocabulary for a genre/mood.
- `ChordProgressionData` — concrete progression with `tonalities` restriction.
- `MelodicStyleSO` + `PhrasePaletteSO` + `PhraseArchetypeSO` — melodic grammar.
- `RhythmCardConfigSO` (as bundle) — rhythmic identity.
- `MIDIInstrumentSO` / `MIDIPercussionInstrumentSO` — timbral identity.

### 7.2 `EmotionMusicalData` as prototype of a complete pack

`EmotionMusicalData` already composes most of the above into a single asset:

- `possiblePercussionInstruments`, `possibleBackingInstruments`, `possibleLeadInstruments`
- `possibleTonalities` (list of enum)
- `possibleTempoRanges`, `possibleTimeSignatures`
- `ChordProgressionsList`, `DrumPatternsList`, `MelodyPatternsList`
- `emotionColor`

It is keyed by a fixed `MusicalEmotion` enum. Conceptually this is already an identity pack — just scoped to emotion keys rather than to modes or genres. Any future "musical identity pack" type can take this shape as a starting point.

### 7.3 Current route A — compose the pack across multiple cards

A pack like "Minor Blues" can be authored today as a coordinated set:

- One card per role, each carrying a `*CardConfigSO` with palette/pattern/style that fits Minor Blues.
- Each card also carrying a consistent `TonalityEffect` (Aeolian) and appropriate `TempoEffect`.

Pros: no new architecture. Cons: the pack is logical, not physical — the player can mix-and-match partial packs, which is either a feature or a bug depending on design intent.

### 7.4 Current route B — compose the pack in a single Backing card

Because progression sharing (§6.1) and progression-overrides-tonality (§6.2) both exist, a single Backing card with a well-curated `ChordProgressionPaletteSO` whose entries constrain `tonalities` can pull the whole part into a specific modal world. Melody and bass inherit the progression automatically.

Pros: single-card pack. Cons: limited to what progression + tonality + voicing can express; does not control rhythm pattern, melodic style, or instrument timbre.

### 7.5 Deferred route — `MusicalIdentityPackSO` + `IdentityEffect`

A future `PartEffect` subclass that carries a pack SO (something like `MusicalIdentityPackSO`) and applies it across roles simultaneously. This would unify routes A and B but requires:

- A new SO wrapper (or generalization of `EmotionMusicalData`).
- A new `PartEffect` subclass with multi-role application logic.
- Possibly extension to the handoff to carry per-card `TonalityProfileSO`, closing §8.1.

Not a current priority. Recorded here for future roadmap consideration.

---

## 8. Known contractual gaps

### 8.1 `TonalityProfileSO` not injectable per card

**Observation:** only `PartConfig.Tonality` (enum) crosses the handoff. The rich profile with characteristic degrees, weighted degree bonuses, vamp candidates, and cadence rules is resolved **package-side** via the `ctx.GetTonalityProfileForPart` delegate. There is no per-card or per-part channel to override which profile is used for a given tonality enum.

**Consequence for cards:** a card that wants "Phrygian with the `i–♭II` vamp promoted" cannot do so today. It can only set enum `Tonality.Phrygian` and hope the default profile resolver returns something suitable.

**Mitigation available:** the default profiles for the 7 diatonic modes already produce very distinct sonic results thanks to their characteristic degrees and cadence rules. For most identity-design purposes, enum-level tonality + progression palette together are sufficient.

**Status:** gap **documented, decision deferred**. Promotion to a roadmap item would happen only when a concrete card design requires non-default profile behavior.

### 8.2 No `ProgressionEffect` or `PaletteEffect` as a `PartEffect`

**Observation:** chord progressions and palettes reach audio only through `TrackActionDescriptor.styleBundle` as `BackingCardConfigSO`. There is no atomic `PartEffect` that carries a progression or palette.

**Consequence:** a card that wants to inject a palette *without* also being a Backing-role card has no channel. Practically, this means progression-as-modifier (e.g., a melody card that imposes a progression on the whole part) is not expressible today.

**Mitigation:** progression sharing via `ctx.SetProgressionForPart` (§6.1) means a Backing card *can* affect other tracks' output — so the restriction is about card identity ("this is a melody card but also carries a progression"), not about cross-track coordination.

**Status:** gap documented. Low priority; current contracts cover the main design cases.

### 8.3 `RhythmCardConfigSO.styleIdOverride` semantically ambiguous

**Observation:** the field exists with a `// How would this work?` comment literal in the source code. `RhythmStyleRegistry.ChooseById` is called with it, but the set of valid ids and their musical meanings is not documented here or in the package SSoTs.

**Consequence:** using this field in card authoring requires knowledge of the registry contents that isn't exposed as a design contract.

**Status:** gap. Recommend not using `styleIdOverride` in card authoring until semantics are closed.

### 8.4 `DensityEffect` and `FeelEffect` present without confirmed audible wiring

**Observation:** both are `PartEffect` subclasses defined inline in `PartEffect.cs` as sealed types. No composer in the current code reads them directly. `RhythmTrackComposer` consumes density/feel fields from `RhythmCardConfigSO`, not from `PartEffect` assets.

**Consequence:** authoring cards with `DensityEffect` or `FeelEffect` gives correct UI labels (`GetLabel()` works) but **audible effect is not verified**. A viewer will see "Feel Swing8" on the card face but may not hear swing.

**Status:** gap in audible closure. Requires either (a) wiring these effects in `SongConfigBuilder` to translate into `RhythmCardConfigSO` field values, or (b) direct consumption in composers.

### 8.5 `RhythmCardConfigSO` feel/density fields present but not semantically closed

**Observation:** the package SSoT `SSoT_Composer_Rhythm_Track.md §6` explicitly states that `fillEveryNMeasures`, `lastMeasuresAsFill`, `kickDensity`, `snareGhostNoteChance`, `hatSubdivisionBias` are "real package-facing inputs, but their full musical meaning is still an active implementation area."

**Consequence:** authoring a card that relies on these fields for a specific feel is contingent on package-side closure, which is scheduled for MidiGenPlay Phase 9 (phrasing / feel semantics).

**Status:** tracked in MidiGenPlay roadmap. Not an ALWTTT gap.

---

## 9. Related governed docs

This planning doc **references** and must stay consistent with:

**ALWTTT-side (authoritative):**
- `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md` — ownership split.
- `integrations/midigenplay/ALWTTT_Uses_MidiGenPlay_Quick_Path.md` — handoff flow.
- `runtime/SSoT_Runtime_CompositionSession_Integration.md` — ALWTTT-side composition bridge.
- `systems/SSoT_Card_System.md` — card model structure.
- `systems/SSoT_Card_Authoring_Contracts.md` — per-kind authoring invariants.

**MidiGenPlay-side (package truth, read-only from ALWTTT):**
- `Documentation~/runtime/SSoT_Runtime_Song_Model_and_Config.md` — `SongConfig` / `TrackParameters`.
- `Documentation~/runtime/SSoT_Runtime_Generation_Orchestration.md` — `MidiGenerator`, `SongOrchestrator`, `GenContext`.
- `Documentation~/runtime/SSoT_Composer_Rhythm_Track.md` — rhythm runtime.
- `Documentation~/runtime/SSoT_Composer_Backing_Track.md` — backing runtime.
- `Documentation~/runtime/SSoT_Composer_Melody_Track.md` — melody runtime.
- `Documentation~/authoring/SSoT_Authoring_Chord_Progressions.md` — progression authoring.
- `Documentation~/authoring/SSoT_Authoring_Rhythm_Patterns.md` — rhythm pattern authoring.

**Missing reference (flagged for future governance work):**
- `integrations/midigenplay/SSoT_ALWTTT_MidiMusicManager_Integration.md` — referenced by multiple SSoTs but not present in the repo. Tracked in `ssot_manifest.yaml` as finding F1.

---

## 10. Update triggers

Update this document when:

- A new `PartEffect` subclass is added or an existing one is re-scoped.
- A new `TrackStyleBundleSO` subclass is added, or an existing role's bundle gains/loses meaningful fields.
- `SongConfig` / `TrackParameters` gains a new field that alters the expressive surface.
- A gap in §8 is closed (remove from §8, update matrix §3).
- Package-side composer precedence changes in a way that flips the matrix columns.
- A `TonalityProfileSO` per-card injection path is added (closes gap §8.1 — significant rewrite).
- `DensityEffect` / `FeelEffect` gain confirmed audible wiring (closes gap §8.4).
- A `MusicalIdentityPackSO` or equivalent is promoted from §7.5 to implementation.

Do **not** update this document for:

- Changes to internal composer algorithms that don't alter the observable matrix.
- Changes to MidiGenPlay authoring tooling UX.
- Gameplay-side balance changes to specific cards.

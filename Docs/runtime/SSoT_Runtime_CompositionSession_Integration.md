# SSoT_Runtime_CompositionSession_Integration — ALWTTT

**Status:** Active governed SSoT  
**Scope:** ALWTTT-side runtime bridge from Composition card play to composition-model mutation, playback rebuild, and loop feedback  
**Owns:** ALWTTT observable runtime contract for `CompositionSession`, `SongCompositionUI`, `SongConfigBuilder`, and playback rebuild decisions  
**Does not own:** package-internal composer precedence, algorithmic track generation internals, or generic MidiGenPlay authoring tools

---

## 1. Purpose

This document is the primary ALWTTT authority for **how live composition works at runtime**.

It promotes the ALWTTT-owned parts of the previous runtime bridge material while deliberately removing package-owned internal generation truth.

---

## 2. Core separation that must hold

When a **Composition card** is played, two distinct pipelines may run.

### 2.1 Musical modifier pipeline
Source:
- `CompositionCardPayload.modifierEffects`

ALWTTT meaning:
- these mutate the editable composition model used for upcoming playback/build output

### 2.2 Gameplay effect pipeline
Source:
- `CardPayload.effects`

ALWTTT meaning:
- these apply immediate gameplay/systemic consequences owned by normal card runtime execution

### 2.3 Non-negotiable rule
Musical modifier effects and gameplay effects must not be silently conflated.

- musical modifiers are **not** normal gameplay effects
- gameplay effects are **not** interpreted as music-model changes unless explicitly represented on the composition side

---

## 3. ALWTTT-owned runtime surfaces

### 3.1 CompositionSession
Owns the song-scoped runtime state machine for composition and playback progression.

CompositionSession exposes `AddCurrentInspiration(int delta) → int` as the canonical session-budget mutator. It clamps to `PersistentGameplayData.MaxInspiration`, refreshes the composition UI, mirrors the result to `PersistentGameplayData.CurrentInspiration` (closing the dual-siting drift documented in `SSoT_Dev_Mode §13.4` at the production-path level), and returns the actual delta applied post-clamp. Track-derived per-loop gain (`HandleLoopFinished`) and host-driven per-loop gain (M4.6F-3 `OnCompositionLoopFinished`) both route through this method.

CompositionSession also derives and owns the per-song render seed (`_songSeed`) consumed by every `RenderSinglePart` call for that song; see §10.

### 3.2 SongCompositionUI
Owns the editable song/part/track model that composition cards mutate.

### 3.3 SongConfigBuilder
Owns the ALWTTT-side conversion from editable model state into runtime playback/build input.

### 3.4 MidiMusicManager
Owns the game-side hosting/control of playback for the current scene/session.

### 3.5 Source-of-truth boundary (must stay explicit)
There are **two different truths on two different sides of the handoff**.

- **ALWTTT game-side editable/session truth** lives in `SongCompositionUI` + `CompositionSession`.
  - This is the model/session state the player is actively editing during the gig.
  - It is the authoritative ALWTTT-side truth **before** package handoff/build.
- **MidiGenPlay package-side runtime truth** lives in `SongConfig` + `SongConfigManager`.
  - This is the package runtime representation produced **after** ALWTTT has built and handed off playback input.
  - It is authoritative for package runtime behavior, not for ALWTTT gameplay/session ownership.

These are complementary, not competing, sources of truth.

---

## 4. Canonical live-composition data flow

### Step 1 — Player plays a Composition card
The player acts through the normal ALWTTT hand/controller surface.

### Step 2 — Runtime forwards to the active CompositionSession
The gig runtime validates that a song/session context exists and routes the card into that active session.

### Step 3 — Musical modifiers mutate the ALWTTT composition model
The session/UI layer applies the card's composition-owned data to the editable song model.

Examples of observable ALWTTT-side mutations include:
- track/role activation or change
- part structure changes
- tempo / meter / tonality / root-note style changes represented on the ALWTTT model side
- instrument/style selections authored through ALWTTT-facing card payloads

### Step 4 — Gameplay effects may also execute
If the card includes normal card effects, those may apply immediate systemic consequences without redefining the music-model mutation step.

### Step 5 — Runtime decides whether playback state must rebuild
If the card changes audible output, ALWTTT runtime triggers rebuild/invalidation behavior for the relevant upcoming playback state.

### Step 6 — SongConfigBuilder rebuilds playback input
The current editable ALWTTT model is transformed into the runtime build/playback configuration handed off to MidiGenPlay.

At this handoff point, authority changes layers:
- ALWTTT still owns the gameplay/session meaning of the change
- MidiGenPlay becomes the package-side runtime owner of the built `SongConfig` state

### Step 7 — MidiMusicManager hosts playback of the rebuilt state
Playback is started, resumed, or rebuilt through the game-owned music runtime surface.

### Step 8 — Loop/part/song feedback is emitted back to ALWTTT
The session produces feedback contexts used by ALWTTT scoring, hype, audience, and encounter logic.

---

## 5. Timing semantics at the ALWTTT-observable level

ALWTTT owns the **observable meaning** of timing on composition modifiers, even if package internals later realize the audio generation.

### 5.1 Immediate
Meaning:
- the card mutates the currently relevant composition model state now
- audible result depends on the current rebuild/playback point, but the model mutation is immediate

### 5.2 OnNextLoop
Meaning:
- the card is intended to affect the next loop boundary of the current relevant musical context
- ALWTTT runtime must ensure the necessary rebuild/invalidation path exists for the effect to become audible on the intended boundary

### 5.3 OnNextPartStart
Meaning:
- the card is intended to affect the next part boundary rather than the already-committed current part

Rule:
- ALWTTT documents the intended runtime boundary behavior here
- precise generator-side implementation details do not belong in this SSoT

---

## 6. Audible-change vs non-audible-change behavior

### 6.1 Audible changes
If a played card changes what should be heard, ALWTTT runtime must treat it as a playback-affecting mutation.

Examples:
- track/part musical structure change
- tempo / meter / tonality / modulation change
- instrument/style change that changes playback output

### 6.2 Non-audible systemic changes
A Composition card may still apply immediate gameplay effects without requiring a music rebuild.

Rule:
- a card can be musically inert yet gameplay-active
- a card can be musically active and gameplay-active at the same time

---

## 7. TrackStyleBundle and package-facing references

Composition cards may reference `TrackStyleBundleSO`-style data as part of ALWTTT gameplay/runtime selection.

ALWTTT owns:
- the gameplay meaning of choosing that bundle via a card
- the fact that the selected bundle participates in the upcoming runtime build/playback

ALWTTT does **not** own:
- the package-internal precedence rules of every bundle field
- composer algorithms that interpret those bundles after the ALWTTT handoff

Those details belong to MidiGenPlay.

---

## 8. Runtime invariants

1. `CompositionSession` is the ALWTTT runtime host for song-scoped live composition.
2. `SongCompositionUI` is the ALWTTT-side editable truth for composition model state.
3. `SongConfigBuilder` is the canonical ALWTTT-side transformation step into playback/build input.
4. Playback-affecting composition cards must trigger the correct ALWTTT rebuild/invalidation path.
5. `MidiMusicManager` is documented as a game runtime integration component, not package-owned truth.
6. Loop/part/song feedback emitted after playback belongs to the ALWTTT runtime contract.
7. Per-loop card draw and per-loop inspiration consumption are host-owned (`GigManager.OnCompositionLoopFinished`), not inside `CompositionSession.HandleLoopFinished`. `CompositionSession` remains the deck-non-mutating invariant holder per the `[Obsolete]` guards on `CompositionSession.PrepareDeck` and `ICompositionContext.Deck`. The host hook fires synchronously from `CompositionSession.HandleLoopFinished`'s `LoopFinished?.Invoke(ctx)`, before the `_loopsRemainingForPart > 0` branch.
8. `CompositionSession.AddCurrentInspiration` is the canonical session-budget mutator. All production-path inspiration deltas during an active session route through it. It clamps to `pd.MaxInspiration` and mirrors to `pd.CurrentInspiration`. Dev-path mutation (`GigManager.DevSetInspiration` → `CompositionSession.DevSetCurrentInspiration`) is a parallel surface tracked separately in `SSoT_Dev_Mode §13`.
9. **Per-track stem persistence + session-level instrument continuity** (B1, 2026-05-12). `MidiMusicManager` maintains a per-song stem cache keyed on `(musicianId, trackInputsHash, partMeterHash)` where `trackInputsHash` is computed ALWTTT-side from UI `TrackEntry` fields (role + StyleBundle GUID + override-melodic/percussion-instrument GUIDs + override-instrument-type) and passed as the 5th parameter of `RenderSinglePart`. Resolved runtime fields (`tcfg.Instrument`, `tcfg.PercussionInstrument`) are NOT in the hash — they are randomized per render by `SongConfigBuilder.FromUI` for the no-override path. Stable instrument continuity across cache invalidations within a song is maintained by `CompositionSession`'s session-level pin maps (`_sessionMelodicPin`/`_sessionPercussionPin`) keyed on `"musicianId|role|override-state"`. Cards with explicit SO override (`overrideMelodicInstrument`/`overridePercussionInstrument`) skip the pin (deterministic by definition). Cards with type-override pin the random pick within the type. Reset semantics: stem cache and instrument pins both clear at song boundary in `Begin()`/`End()`. Boundary: `SongConfig` (MidiGenPlay-owned) is NOT modified to carry the hash; the parameter travels as a per-call argument per `SSoT_ALWTTT_MidiGenPlay_Boundary §3`.

---

## 9. Out-of-scope package internals

The following are intentionally **not** governed here:
- composer-internal precedence chains
- package repository loading rules
- package authoring/editor internals
- package-side TS normalization algorithms
- how the package turns a given seed into a palette-entry pick (selection-mechanism internals)

Those belong in MidiGenPlay docs. ALWTTT's per-song render-**seed policy** — when a new seed is drawn and what it must never depend on — is ALWTTT truth and is governed in §10, not here.

---

## 10. Per-song render seed

`CompositionSession` derives a single per-song render seed (`_songSeed`) once per song, in `Begin()`, from run entropy (`unchecked((int)DateTime.UtcNow.Ticks)`). The value is logged as `[Session] SongSeed=<n>` (musical-bug repro path) and passed on every `mm.RenderSinglePart(..., seedOverride: _songSeed)` call for that song. `_songSeed` is cleared in `End()`.

**Guaranteed properties.**
- **Intra-song stability** — the same seed drives the same pick across re-renders caused by cache invalidation within the same song (smoke-tested `ST-S5gb-2`).
- **Cross-song variety** — a fresh seed is drawn per song (smoke-tested `ST-S5gb-1`).

This deliberately replaces the accidental stability previously produced by the package's constant `defaultSeed`.

**Prohibition.** The seed must never be derived from anything that changes between re-renders of the same song (a per-render clock read, a per-render counter). Doing so would break intra-song stability.

**Cache key unaffected (D-S5gb-1=A).** `trackInputsHash` (invariant 9, §8) keeps its existing meaning — player-controlled inputs only. Cross-song isolation is guaranteed by the stem-cache/pin clear in `Begin()`/`End()` already documented in invariant 9; that claim is now **verified at runtime** by `ST-S5gb-3` (2026-07-05), moving invariant 9 from documented truth to observed truth for the cross-song-isolation portion of its claim. Documented fallback if this isolation is ever found to fail: fold the seed into the cache key itself (pattern `MOD-DIR-3`).

**Package contract consumed, not redefined.** `GenerateSinglePart(..., int? seedOverride = null)`; the package resolves `baseSeed = seedOverride ?? settings.defaultSeed` once per render. `seedOverride: null` is bit-identical to pre-adoption behavior. Authority for the package-side mechanism: MidiGenPlay orchestration SSoT §5.1 (cross-project reference, read-only — not redefined here, per `SSoT_ALWTTT_MidiGenPlay_Boundary.md`).

**Dev override.** A dev-only pin surface (`CompositionSession.DevPinnedSongSeed`) exists for reproducible songs; see `SSoT_Dev_Mode.md §8.7`.

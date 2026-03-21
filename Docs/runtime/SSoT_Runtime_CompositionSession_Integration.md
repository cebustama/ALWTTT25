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

---

## 9. Out-of-scope package internals

The following are intentionally **not** governed here:
- composer-internal precedence chains
- package repository loading rules
- package authoring/editor internals
- package-side TS normalization algorithms

Those belong in MidiGenPlay docs.

# SSoT_ALWTTT_MidiGenPlay_Boundary — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Explicit ownership split and integration boundary between ALWTTT game/runtime truth and MidiGenPlay package truth  
**Owns:** what ALWTTT may define, what it must reference, and how older mixed docs are classified  
**Does not own:** full ALWTTT subsystem semantics or MidiGenPlay package internals themselves

---

## 1. Purpose

This document exists to stop ALWTTT and MidiGenPlay from becoming **two competing primary homes for the same concept**.

It is the governing answer to:
- what belongs to ALWTTT
- what belongs to MidiGenPlay
- what remains a thin integration contract only

---

## 2. Ownership split

### 2.1 ALWTTT owns
- gig runtime orchestration
- card gameplay semantics
- composition card gameplay meaning
- active song/session runtime lifecycle
- audience/scoring/hype/vibe/stress behavior as game systems
- ALWTTT-side composition model state
- runtime rebuild/invalidation decisions observable from the game
- `MidiMusicManager` as a game runtime integration component

### 2.2 MidiGenPlay owns
- package-internal composer algorithms
- bundle-field precedence inside package generation pipelines
- repository loading/selection internals owned by the package
- generic package-side music authoring tools
- package-side TS normalization/adaptation algorithms
- generic pattern/instrument generation internals that are not game-owned runtime behavior

### 2.2.1 Source-of-truth split across the handoff
This is the most important anti-confusion rule in the integration boundary.

- **Before handoff/build**: ALWTTT owns the active editable/session truth via `SongCompositionUI` + `CompositionSession`.
- **After handoff/build**: MidiGenPlay owns the package-side runtime truth via `SongConfig` + `SongConfigManager`.

This is not duplicate authority. It is a deliberate split between:
- game-owned editing/session truth
- package-owned runtime song/generation truth

Neither side should silently redefine the other.

### 2.3 Shared integration surface
The shared surface is thin and contractual.

It includes concepts such as:
- ALWTTT building playback input from its composition model
- ALWTTT passing bundle/style/instrument choices into the music package pipeline
- ALWTTT receiving playback/generation results as part of its live gig runtime

Rule:
- ALWTTT may document the **observable contract and handoff**
- ALWTTT must not silently redefine package internals

---

## 3. Boundary rules for composition cards

### 3.1 ALWTTT-owned side
ALWTTT owns:
- what a Composition card means to gameplay
- when it can be played
- what part of the composition model it changes
- whether it also applies normal gameplay effects
- whether the card should trigger an audible rebuild path in the game runtime

### 3.2 MidiGenPlay-owned side
MidiGenPlay owns:
- how a referenced style bundle or authored musical asset is interpreted deep inside package composition/generation pipelines
- how package-side composers select, adapt, and render internal musical content after the ALWTTT handoff

---

## 4. Boundary rules for runtime playback

### 4.1 ALWTTT-owned side
ALWTTT owns:
- when a song/session exists
- when playback should start/stop/rebuild
- which game runtime component hosts playback
- how loop/part/song feedback feeds back into the game

### 4.2 MidiGenPlay-owned side
MidiGenPlay owns the package-level machinery that turns build input into generated musical output.

---

## 5. Classification of older mixed docs

### 5.1 `SSoT_Runtime_CompositionSession_Bridge.md`
Classification:
- mostly **ALWTTT runtime integration truth**

New governed home:
- `runtime/SSoT_Runtime_CompositionSession_Integration.md`

### 5.2 `SSoT_CompositionCards_TrackStyleBundles.md`
Classification:
- mixed doc
- partly ALWTTT card/gameplay selection truth
- partly package-owned bundle/composer interpretation truth

New governed treatment:
- ALWTTT gameplay/runtime selection truth stays with ALWTTT card/runtime docs
- package-owned internals must be referenced, not duplicated

### 5.3 `SSoT_CompositionAuthoringTools.md`
Classification:
- package-authoring heavy / cross-project reference

New governed treatment:
- not a primary ALWTTT SSoT
- belongs as reference or in MidiGenPlay docs

### 5.4 `SSoT_Composer_BackingChordTrack.md` and `SSoT_Composer_RhythmTrack.md`
Classification:
- package-owned composer internals

New governed treatment:
- do not remain ALWTTT authorities
- reference MidiGenPlay instead

### 5.5 `ALWTTT_MidiGenPlay_TS_Normalization_Roadmap.md`
Classification:
- cross-project historical/planning material

New governed treatment:
- archive/reference only
- package-side normalization truth belongs to MidiGenPlay, not ALWTTT

---

## 6. Update rules at the boundary

### 6.1 Update ALWTTT docs when
- ALWTTT observable gameplay/runtime behavior changes
- ALWTTT changes what it sends into the music pipeline
- ALWTTT changes how it interprets playback/session feedback
- the game-owned `MidiMusicManager` contract changes

### 6.2 Update MidiGenPlay docs when
- package internals change without changing ALWTTT-observable behavior
- package algorithms/composers/tools change within their own authority surface

### 6.3 Update both sides when
- the handoff contract changes
- field meanings at the boundary change
- a previously mixed concept is reassigned to a clearer owner

---

## 7. Non-negotiable rule

**One concept must have one primary home.**

If a concept is primarily package truth, ALWTTT may reference it but must not silently redefine it.
If a concept is primarily game/runtime truth, MidiGenPlay may mention it but ALWTTT remains authority for the game-owned side.

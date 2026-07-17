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

### 4.3 Known package constraints ALWTTT works around

Recorded here so the workaround is not mistaken for ALWTTT design, and so the request to the package is written down once.

**Per-musician stem and instrument readback (BASS-1, 2026-07-12).** `PartRender.stemsByMusician` and `PartRender.melInstByMusician` are keyed by `musicianId`, and the `instrumentOverrides` parameter of `RenderSinglePart` is likewise musician-keyed. Since BASS-1, an ALWTTT musician may hold **more than one role-track in a part** (Melody + Bassline, say), and these maps cannot represent that: the package returns one stem and one instrument per musician (last role wins).

ALWTTT does **not** redefine or patch the package. It degrades safely instead: multi-track musicians are omitted from the track-inputs hash map and from the instrument-override argument, which disables the stem/bundle cache for the affected part and forces a fresh render. Per-role voice consistency is preserved by ALWTTT's own session pins, which are keyed `"musicianId|role|override-state"`. Detail: `runtime/SSoT_Runtime_CompositionSession_Integration.md` §8 invariant 9 and §11.

**Request to MidiGenPlay (open, not blocking).** Re-key `PartRender.stemsByMusician` / `melInstByMusician` and the `instrumentOverrides` parameter by `(musicianId, TrackRole)`. That would let ALWTTT re-enable per-track stem caching for multi-track musicians. Until then the degradation above stands and is correct.

**Not a constraint:** channel allocation. The package's `BuildChannelMap` already allocates per **track index**, not per musician, so two tracks belonging to one musician receive distinct channels natively. ALWTTT's `(musicianId, role)` channel stamping mirrors that.

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

---

## 8. Cross-project contract elements (delivered/adopted log)

Concrete, dated instances of the shared integration surface (§2.3) that were filed against MidiGenPlay's tracker and resolved. Each entry records the lifecycle (filed → delivered → adopted) and any ALWTTT-side decision about how the delivered element is consumed.

### 8.1 MGP-ALWTTT-SEED-1 — per-render seed parameter

**Cross-project. Filed → delivered → adopted 2026-07-05 (same day).**

MidiGenPlay added an optional per-render seed parameter to `GenerateSong`/`GenerateSinglePart` (`int? seedOverride`; `null` = pre-adoption bit-identical behavior). Ownership split at this element: the **seed policy** (when a new seed is drawn, what it must not depend on) is ALWTTT truth, governed in `SSoT_Runtime_CompositionSession_Integration.md §10`; the **selection mechanism** that turns a seed into a palette pick stays MidiGenPlay truth.

MidiGenPlay's own package-side **D4** (deterministic anti-repeat exclusion) was **declined** — clone-on-pick makes exclusion-by-reference infeasible package-side. ALWTTT accepted this and operates **probabilistically** instead: with palettes of ≥6 entries, consecutive repetition is ~1/N and is accepted for the demo (`D-S5g-7=C`). If deterministic non-repetition is ever required, it returns as a package-side batch with an explicit palette-entry-identity decision.

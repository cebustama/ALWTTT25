# SSoT_Runtime_Flow — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Current ALWTTT gig runtime flow and orchestration across setup, card play, song execution, feedback, and audience response  
**Owns:** runtime phase flow, manager/session responsibilities, card entry points into runtime, loop/part/song feedback lifecycle  
**Does not own:** detailed combat tuning (`systems/SSoT_Gig_Combat_Core.md`), detailed card semantics (`systems/SSoT_Card_System.md`), or package-internal MidiGenPlay generation algorithms

---

## 1. Purpose

This document is the primary authority for **how ALWTTT runs a gig at runtime**.

It promotes runtime truth that was previously scattered across:
- `reference/Gig.md`
- `backlog/ideas/gig_pipeline_summary.md`
- `reference/subsystems/Composition Pipeline/SSoT_Runtime_CompositionSession_Bridge.md`

This SSoT is about **execution flow**, not just design intent.

---

## 2. Runtime scope boundary

This SSoT answers:
- which runtime components orchestrate a gig
- how the player moves from hand/cards into composition and playback
- when loop/part/song feedback is produced
- where audience reaction hooks are inserted
- where song-level state begins and ends

This SSoT does **not** define the full balance meaning of SongHype, Vibe, Stress, or statuses.
Those semantics live in system SSoTs.

---

## 3. Canonical runtime actors

### 3.1 GigManager
`GigManager` is the gig-scene orchestrator.

It owns the high-level runtime responsibilities for:
- gig setup
- phase transitions
- creating and ending the active song/session runtime
- holding references to runtime UI/services needed by the gig
- consuming loop/part/song feedback and routing it into ALWTTT gameplay state

### 3.2 DeckManager + HandController
These own the runtime hand/deck surface.

They are responsible for:
- current playable hand state
- initiating card play from the UI
- forwarding play attempts to the relevant game runtime host

### 3.3 SongCompositionUI
`SongCompositionUI` is the authoritative editable song/part/track model surface inside ALWTTT runtime.

It is responsible for:
- presenting the current composition state to the player
- receiving composition card mutations
- exposing the current model used to build playback configuration

### 3.4 CompositionSession
`CompositionSession` is the song-scoped runtime state machine.

It owns:
- part progression inside the active song
- loop counting within the active part
- composition-time resource flow owned by the current song/session
- generation of loop/part/song feedback snapshots
- ALWTTT-side composition card application lifecycle

### 3.5 SongConfigBuilder
`SongConfigBuilder` converts ALWTTT's editable composition model into the runtime playback/build configuration consumed by the music runtime.

### 3.6 MidiMusicManager
`MidiMusicManager` is an **ALWTTT runtime integration component**.

It is not the authority for package generation internals.
Its job is to host/control the music playback surface used by the gig runtime.

---

## 4. Canonical gig runtime phases

### Phase 0 — Gig setup
Runtime responsibilities:
- initialize the gig scene
- prepare band and audience runtime objects
- wire deck/hand/runtime UI references
- prepare the runtime services needed for composition and playback

### Phase 1 — Between-songs action window
Purpose:
- allow non-composition card play in the song transition window when the current rules permit it
- apply immediate systemic effects before the next song composition begins

Contract:
- this is where **Action cards** belong in the baseline governed flow
- if the current implementation slice only partially exposes this window, `CURRENT_STATE.md` records the gap; this SSoT still owns the intended runtime home

### Phase 2 — Composition phase
Purpose:
- player edits the current song/part state through composition cards and composition UI

Runtime rule:
- composition card play enters the runtime through hand/controller -> gig runtime host -> active `CompositionSession`
- the composition model is edited before the song/part is committed to playback

### Phase 3 — Song performance phase
Purpose:
- the currently configured part/song plays
- loop-level feedback is emitted
- audience micro-reaction hooks can evaluate the current music output

Runtime rule:
- the player is no longer editing the currently committed loop in the same way as the composition phase
- loop completion emits loop-level feedback snapshots

### Phase 4 — Song end resolution
Purpose:
- finalize song-scoped feedback
- convert accumulated loop/song results into downstream game state updates
- prepare audience turn / next-song transition

### Phase 5 — Audience response phase
Purpose:
- the audience acts back on the band based on accumulated song performance and encounter rules

### Phase 6 — Next song or end gig
Purpose:
- either initialize the next song-scoped runtime session or terminate the gig encounter

---

## 5. Card entry points into runtime

### 5.1 Action cards
Canonical runtime sequence:
1. player selects an action card from hand
2. runtime validates timing/targeting
3. runtime interprets the card's gameplay effect list
4. post-play state is applied (discard/exhaust/etc. according to card rules)

### 5.2 Composition cards
Canonical runtime sequence:
1. player selects/plays a composition card from hand
2. the active gig runtime forwards the request to the active `CompositionSession`
3. ALWTTT composition data is validated and applied to the editable song model
4. any gameplay-side card effects owned by the card may also execute as immediate systemic consequences
5. if the card changes audible output, runtime rebuild/invalidation behavior is triggered according to the integration contract

---

## 6. Session lifecycle

### 6.1 Session creation
A `CompositionSession` is created per active song runtime.

Runtime invariant:
- there is at most **one active composition session per active song**.

### 6.2 Session active state
While active, the session owns:
- the current part index
- current loop index / remaining loop tracking
- current editable/committed song state needed for playback
- production of feedback contexts

### 6.3 Session completion
At song end:
- the session emits song-finished feedback
- the gig runtime consumes the result
- song-scoped runtime state is cleaned up or reset for the next song

---

## 7. Feedback lifecycle

### 7.1 Loop-level
At loop completion, runtime produces a `LoopFeedbackContext`-style payload representing the just-played loop.

Runtime use:
- loop scoring hooks
- SongHype delta computation hooks
- audience micro-reaction hooks
- loop-based inspiration/resource hooks where applicable

### 7.2 Part-level
At part completion, runtime produces a `PartFeedbackContext`-style payload for the completed part.

### 7.3 Song-level
At song completion, runtime produces a `SongFeedbackContext`-style payload.

This is the canonical runtime handoff used to drive:
- song-end conversion into audience-facing progress
- encounter-level scoring hooks
- next-phase transitions

---

## 8. Runtime invariants

1. `GigManager` owns gig-scene orchestration and phase transitions.
2. `CompositionSession` owns active song/session progression and feedback emission.
3. `SongCompositionUI` is the ALWTTT-side source of editable composition state.
4. `SongConfigBuilder` converts ALWTTT model state into music playback build input.
5. `MidiMusicManager` is part of the ALWTTT runtime surface and must be documented here/on the integration side, not as package truth.
6. Loop/part/song feedback belongs to the ALWTTT runtime contract even when downstream systems use MidiGenPlay for playback/generation.

---

## 9. Migration note

This SSoT intentionally absorbs the **runtime-flow** truth from older design/runtime docs while leaving package-internal composer logic out of ALWTTT authority.
The detailed composition/playback bridge is further specified in:
- `runtime/SSoT_Runtime_CompositionSession_Integration.md`
- `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md`

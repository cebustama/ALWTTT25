# ALWTTT_Uses_MidiGenPlay_Quick_Path

**Status:** quick operational guide  
**Purpose:** one-page explanation of how ALWTTT uses MidiGenPlay during live gig composition

This file is intentionally short.
Use it when you need the fastest possible understanding of the handoff.

---

## 1. The shortest version

A **Composition card** in ALWTTT can do two things at once:

1. change the **game-side composition model**
2. apply immediate **gameplay effects**

The musical side then flows through:

```text
Composition card
-> CompositionSession
-> SongCompositionUI model mutation
-> SongConfigBuilder
-> MidiGenPlay SongConfig / SongConfigManager side
-> generation / playback hosting
-> loop / part / song feedback
-> ALWTTT gameplay consequences
```

---

## 2. Who owns what

### ALWTTT owns
- card meaning in gameplay
- gig/runtime phase flow
- the active editable composition/session state
- deciding whether a change must rebuild audible playback
- feeding loop/part/song results back into hype/audience/combat systems
- `MidiMusicManager` as the game-side playback/integration surface

### MidiGenPlay owns
- the package-side runtime song representation after handoff
- generation orchestration internals
- composer internals
- package interpretation of bundles/patterns/recipes after ALWTTT has handed them off

---

## 3. The crucial source-of-truth rule

There are **two truths on two sides of the boundary**:

### Before handoff
**ALWTTT editable/session truth** = `SongCompositionUI` + `CompositionSession`

This is the model the player is actively manipulating in the gig.

### After handoff
**MidiGenPlay package runtime truth** = `SongConfig` + `SongConfigManager`

This is the package-side runtime state built from the ALWTTT model.

These do not compete.
They describe different ownership layers of the same live composition flow.

---

## 4. What happens when a Composition card is played

### A. Gameplay side
Normal card effects may execute immediately.
Examples:
- statuses
- draw
- stress/vibe/economy changes

### B. Musical side
The card's composition-specific data changes the ALWTTT composition model.
Examples:
- track/style changes
- part structure changes
- tempo/meter/tonality/root-note changes

### C. Rebuild decision
If the change should alter audible output, ALWTTT runtime triggers the necessary rebuild/invalidation path.

### D. Package handoff
`SongConfigBuilder` transforms ALWTTT's model into the package-side build/runtime input used by MidiGenPlay.

### E. Feedback returns to the game
Loop/part/song playback feedback returns to ALWTTT, where the game uses it for:
- SongHype / LoopScore style systems
- audience response
- encounter progression

---

## 5. Which docs to open next

If you need more detail:

- ALWTTT runtime flow:
  - `runtime/SSoT_Runtime_Flow.md`
- ALWTTT composition/runtime bridge:
  - `runtime/SSoT_Runtime_CompositionSession_Integration.md`
- ALWTTT ↔ MidiGenPlay boundary:
  - `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md`
- `MidiMusicManager` as game runtime surface:
  - `integrations/midigenplay/SSoT_ALWTTT_MidiMusicManager_Integration.md`
- MidiGenPlay package-side runtime truth:
  - `Documentation~/runtime/SSoT_Runtime_Song_Model_and_Config.md`
  - `Documentation~/runtime/SSoT_Runtime_Generation_Orchestration.md`

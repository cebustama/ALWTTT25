# runtime/ — ALWTTT runtime SSoTs

This folder contains the **governed runtime truth** for ALWTTT.

What belongs here:
- scene/runtime orchestration
- phase flow through a gig
- ownership of runtime managers and session lifecycles
- ALWTTT-side composition/session integration behavior
- playback/rebuild/invalidation behavior owned by the game runtime

What does **not** belong here:
- long-term planning and ideas
- combat balance/tuning that is not specifically runtime flow
- package-internal MidiGenPlay composer algorithms
- generic package authoring tools
- historical or superseded flow descriptions

## Conflict rule

- `systems/` owns gameplay semantics.
- `runtime/` owns how those semantics are executed through the current game runtime.
- `integrations/` owns cross-project ownership boundaries.

If a runtime doc appears to redefine a gameplay rule, the relevant `systems/` SSoT wins.
If a runtime doc appears to redefine a package internal, the boundary doc plus MidiGenPlay docs win on the package side.

## Update rule

Update this folder when a technical change affects:
- gig phase flow
- manager responsibilities
- session lifecycle
- loop/part/song feedback events
- runtime rebuild / playback behavior observable from ALWTTT

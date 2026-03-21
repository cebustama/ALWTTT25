# systems/ — ALWTTT subsystem SSoTs

This folder contains the **governed subsystem truth** for ALWTTT.

What belongs here:
- current gameplay-system rules
- current card/combat/runtime-facing subsystem contracts
- state/phase/resource semantics that define how ALWTTT works now

What does **not** belong here:
- future ideas and backlog items
- roadmap-only material
- legacy snapshots
- MidiGenPlay package internals
- purely explanatory reference text that does not define truth

## Conflict rule

If a `systems/` SSoT conflicts with a planning or reference doc, the `systems/` SSoT wins.
If two `systems/` docs appear to conflict, `SSoT_CONTRACTS.md` + `coverage-matrix.md` decide the primary home.

## Update rule

Update this folder when a technical change affects:
- gameplay semantics
- card semantics
- combat economy
- encounter structure / victory-failure rules
- targeting rules
- authoring/runtime contracts owned by ALWTTT gameplay

Do **not** update this folder for package-only MidiGenPlay internal changes unless the ALWTTT-observable contract changed.

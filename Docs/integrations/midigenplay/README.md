# integrations/midigenplay/ — ALWTTT ↔ MidiGenPlay boundary docs

This folder contains the **governed boundary contract** between ALWTTT and MidiGenPlay.

What belongs here:
- explicit ownership split
- observable handoff contracts
- ALWTTT runtime integration behavior that touches MidiGenPlay
- migration redirects from older mixed docs

What does **not** belong here:
- full ALWTTT gameplay truth
- full ALWTTT phase flow
- MidiGenPlay composer internals
- MidiGenPlay generic authoring tools

## Conflict rule

When a concept has one game-owned side and one package-owned side:
- ALWTTT owns the game-owned side
- MidiGenPlay owns the package-owned side
- this folder defines the boundary and handoff, not duplicate full ownership on both sides

## Quick path

Use `ALWTTT_Uses_MidiGenPlay_Quick_Path.md` for the shortest end-to-end explanation of how ALWTTT hands composition changes into MidiGenPlay during a gig.

## Anti-drift rule

If a concept is duplicated here and in MidiGenPlay cross-project reference docs, the **ALWTTT-side primary home** wins for game-owned behavior. MidiGenPlay may keep a preserved reference copy, but that copy must not become a second primary authority.

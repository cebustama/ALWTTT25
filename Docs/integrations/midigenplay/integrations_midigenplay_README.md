# integrations/midigenplay/ — ALWTTT ↔ MidiGenPlay boundary docs

This folder contains the **governed boundary contract** between ALWTTT and MidiGenPlay.

What belongs here:
- explicit ownership split
- observable handoff contracts
- ALWTTT runtime integration behavior that touches MidiGenPlay
- analytical maps of the package's expressive surface as consumed by ALWTTT card design
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

## Docs in this folder

- `README.md` — this file.
- `SSoT_ALWTTT_MidiGenPlay_Boundary.md` — the governed boundary SSoT (ownership split, handoff contracts, classification of older mixed docs).
- `ALWTTT_Uses_MidiGenPlay_Quick_Path.md` — one-page operational guide for the composition-card → MidiGenPlay handoff.
- `MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` — analytical map of the observable musical expressive surface MidiGenPlay offers ALWTTT card design (26-axis matrix, observed precedences, per-role bundle contracts, 5 documented gaps; per-musician SO whitelist precedence added 2026-05-20). Moved here from `planning/` on 2026-05-20 because it analyzes the package boundary, not ALWTTT-side planning.

## Anti-drift rule

If a concept is duplicated here and in MidiGenPlay cross-project reference docs, the **ALWTTT-side primary home** wins for game-owned behavior. MidiGenPlay may keep a preserved reference copy, but that copy must not become a second primary authority.

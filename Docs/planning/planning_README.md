# planning/ — ALWTTT

This folder contains planning-only material, organized by *kind*:

- **`planning/` root** holds **standing and long-term design pillars** —
  directives, design pillars, and post-MVP design intent that biases work
  across many batches but does not itself have a near-term batch slot.
- **`planning/active/`** holds **near-term batched work** — the active
  roadmap, demo-cut planning, starter-deck design, and other docs whose
  content is being acted on inside foreseeable batches.
- **`planning/archive/`** holds completed or superseded planning.

A doc at the root is *durable direction*; a doc in `active/` is *current
work-in-progress*. Both are planning-only — neither defines implementation
truth.

## What belongs at `planning/` root

- standing directives (e.g. `Design_Project_Directives_v0_1.md`)
- long-term design pillars without a current batch slot
  (e.g. `Design_Pending_Effects_v1.md`, `Design_Tempo_Identity_v1.md`,
  `Design_Song_Parts_Library_v0_1.md`)

## What belongs at `planning/active/`

- the live roadmap (`Roadmap_ALWTTT.md`)
- design docs for in-progress or near-term batches
  (e.g. `Design_Demo_Cut_v1.md`, `Design_Starter_Deck_v1.md`,
  `Design_Audience_Status_v1.md` while any of its sections remain active)

## What does **not** belong in planning at all

- current subsystem truth (lives in `systems/`)
- current runtime contracts (lives in `runtime/` or `integrations/`)
- current authoritative gameplay rules (lives in the relevant SSoT)
- analytical / reference material about external integrations
  (lives in `integrations/<package>/` or `reference/`)

## Conflict rule

If a planning doc conflicts with a subsystem SSoT or `CURRENT_STATE.md`,
the planning doc loses.

## Update rule

Update this folder when:
- declaring a new standing directive or design pillar
- a planning doc enters or exits an active batch slot
  (root ↔ active migration)
- archiving a superseded planning doc

When a doc moves between root and `active/` (or to `archive/`), update
`SSoT_INDEX.md` and any path-based cross-references in other docs.

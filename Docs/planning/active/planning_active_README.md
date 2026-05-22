# planning/active/ — ALWTTT

This folder contains planning docs for **near-term batched work** — the
roadmap and any design doc whose content is being acted on inside the
current or next foreseeable batch.

What belongs here:
- the live roadmap (`Roadmap_ALWTTT.md`)
- design docs for in-progress batches
- design docs for the next foreseeable batch
- design docs for milestones that are partially shipped and partially
  pending implementation (e.g. starter-deck work where authoring is
  ongoing; audience-status design where at least one section remains
  active design intent)

What does **not** belong here:
- standing directives and long-term design pillars without a near-term
  batch slot — those live at `planning/` root
- completed or superseded planning — that lives at `planning/archive/`
- current subsystem truth, runtime contracts, or gameplay rules
- analytical / reference material about integrations

## Conflict rule

If a doc here conflicts with any active SSoT, the SSoT wins.
If it conflicts with `CURRENT_STATE.md`, treat `CURRENT_STATE.md` as the
operational snapshot and then repair the planning doc.

## Update rule

Update this folder when:
- a planning doc gains a near-term batch slot (moved from root or
  newly authored)
- a planning doc completes (move to `planning/archive/`)
- a planning doc's near-term relevance ends but design intent remains
  long-term (move to `planning/` root)

When a doc moves, update `SSoT_INDEX.md` and any path-based
cross-references in other docs.

# integrations/ — ALWTTT cross-project boundaries

This folder contains **governed cross-project integration truth** for ALWTTT.

What belongs here:
- explicit authority boundaries between ALWTTT and external/internal packages
- observable contracts at integration points
- rules for what ALWTTT may document vs what it must reference elsewhere

What does **not** belong here:
- full subsystem truth that belongs in `systems/` or `runtime/`
- package-internal implementation detail
- planning-only ideas
- legacy historical notes unless explicitly preserved as migration context

## Conflict rule

- `systems/` owns ALWTTT gameplay semantics.
- `runtime/` owns ALWTTT execution flow.
- `integrations/` owns cross-project ownership boundaries and observable contract splits.

If a package-side internal is documented here as if ALWTTT owned it, the relevant package docs win on the package side.

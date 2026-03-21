# ALWTTT Documentation

This folder is the governed documentation root for **A Long Way to the Top (ALWTTT)**.

It is the result of a safe governance migration from the previous documentation set.
The pre-governance material is no longer treated as a second docs tree; it is preserved as historical trace material only.

---

## What this docs set must answer

This documentation system exists to make it easy to answer:

- what is true **today** in ALWTTT,
- what belongs to **ALWTTT gameplay/runtime truth**,
- what belongs to **MidiGenPlay package truth**,
- what is only **integration boundary truth**,
- what is **planned but not implemented**,
- and what changed recently.

---

## Authority order

When documents disagree, use this order:

1. `SSoT_CONTRACTS.md`
2. Relevant subsystem SSoT listed in `SSoT_INDEX.md`
3. `CURRENT_STATE.md` for latest validated active slice / immediate next step
4. `coverage-matrix.md` for authority lookup
5. `planning/active/` for future work only
6. `reference/` for explanatory/supporting material
7. `archive/` for legacy/historical context only

---

## Cross-project rule: ALWTTT vs MidiGenPlay

### ALWTTT owns
- game runtime truth
- card gameplay semantics
- gig/combat/session truth
- audience and scoring truth
- scene/runtime integration behavior
- `MidiMusicManager` as a game runtime/integration component

### MidiGenPlay owns
- package internals for procedural MIDI generation
- composer internals
- package-side authoring internals
- algorithmic details that are not game-owned behavior

### Shared boundary rule
If a concept crosses both systems:
- ALWTTT may document the **observable contract and runtime handoff**
- ALWTTT must **not silently redefine** MidiGenPlay package internals
- MidiGenPlay may mention ALWTTT usage, but ALWTTT remains authority for game behavior

---

## Root governance files

- `SSoT_INDEX.md` — master map of authoritative docs and intended homes
- `SSoT_CONTRACTS.md` — documentary rules and authority boundaries
- `CURRENT_STATE.md` — active validated implementation slice, recent completion, next step, blockers
- `coverage-matrix.md` — subsystem-to-authority mapping
- `changelog-ssot.md` — semantic/documentary changes only

---

## Folder guide

- `systems/` — current gameplay-system truth
- `runtime/` — current runtime/orchestration truth
- `integrations/` — cross-project ownership boundaries and observable handoff contracts
- `planning/active/` — live roadmap and future-work sequencing
- `reference/` — explanatory/support docs that never override SSoTs
- `archive/` — superseded/history/traceability
- `archive/absorbed/` — old->new supersession map
- `archive/snapshots/` — snapshot policy and snapshot metadata, not active docs

---

## Migration status

The governance migration is now **functionally complete through Batch 06**.

Completed batches:
- **Batch 01** — governance spine
- **Batch 02** — combat / card / authoring authorities
- **Batch 03** — runtime + MidiGenPlay boundary authorities
- **Batch 04** — audience / status / scoring authorities
- **Batch 05** — encounter authority + supersession map
- **Batch 06** — planning/reference/archive normalization + snapshot policy + final navigation sweep

The active docs tree should now be treated as the working documentation system.
The pre-governance snapshot remains useful only as historical backup.

---

## Working rule

After every meaningful technical change:

1. identify what concept actually changed
2. find its primary home in `coverage-matrix.md`
3. update that primary SSoT first
4. then update:
   - `CURRENT_STATE.md` if operational reality changed
   - `changelog-ssot.md` if meaning or authority changed
   - `coverage-matrix.md` only if the concept’s primary home changed
   - reference/support docs only if needed

A technical change is not complete until the required documentation updates are done.

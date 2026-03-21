# SSoT_CONTRACTS — ALWTTT Documentary Governance Rules

This file defines the documentary rules for ALWTTT.

These rules are normative for documentation structure and authority.

---

## 1. One concept = one primary home

A major concept must have **one primary authoritative home**.

Examples:
- card gameplay semantics → ALWTTT card SSoT
- gig/combat rules → ALWTTT combat SSoT
- package-side MIDI composer internals → MidiGenPlay docs

Cross-links are allowed.
Competing primary definitions are not.

---

## 2. SSoT vs CURRENT_STATE vs planning

### SSoT
Defines what is structurally true in the project now.
Use for stable rules, contracts, ownership, and subsystem behavior.

### CURRENT_STATE
Defines:
- what is actively validated right now,
- what just changed,
- what is next,
- what is blocked,
- and which docs must be updated next.

`CURRENT_STATE.md` does **not** replace subsystem SSoTs.
It is a live status layer.

### Planning
Planning docs define:
- intended changes,
- future architecture,
- backlog,
- experiments,
- drafts.

Planning docs must **never** be treated as current implementation truth.

---

## 3. Reference docs do not override SSoTs

Reference material can explain, summarize, teach, or preserve context.
It cannot overrule a governed SSoT.

If a reference doc still contains important truth:
- migrate the truth into the correct SSoT,
- keep the reference only as secondary explanatory material,
- or archive it if superseded.

---

## 4. Archive does not define truth

Archived documents exist for:
- history,
- context,
- migration traceability,
- recovery of ideas,
- comparison with previous architecture.

Archive content is **not authoritative**.
Every archived doc should ideally carry a header such as:

- `STATUS: LEGACY` or `STATUS: SUPERSEDED`
- `Replaced by: <new doc>`

---

## 5. ALWTTT vs MidiGenPlay authority boundary

### ALWTTT owns
- gameplay truth
- encounter/session/runtime truth
- audience and scoring truth
- card semantics as player-facing game mechanics
- scene/runtime integration
- `MidiMusicManager` as a game runtime integration component

### MidiGenPlay owns
- package internals for MIDI generation
- composer internals
- package normalization logic
- package authoring internals not owned by ALWTTT gameplay

### Shared concepts
Shared concepts must be split into:
- ALWTTT-side observable contract / runtime handoff
- package-side internal implementation

ALWTTT docs may reference MidiGenPlay docs.
ALWTTT docs must not silently redefine package truth.

---

## 6. Runtime integration rule

A runtime integration component that lives in the ALWTTT game runtime is documented in ALWTTT, even if it depends on MidiGenPlay.

This applies especially to:
- `MidiMusicManager`
- `CompositionSession`
- game scene/runtime bridge behavior
- ALWTTT-side playback/session orchestration

---

## 7. Legacy model rule

If both legacy and current models exist in docs, the docs must explicitly say which one is current.

Example:
- legacy `CardData`-style material must not be mixed into current card-system truth without a visible legacy marker.

Preferred handling:
- current model → SSoT
- legacy model → archive or legacy appendix

---

## 8. Mandatory updates after technical change

After a meaningful technical change:

### If gameplay/card/combat changed
Update:
- relevant subsystem SSoT
- `CURRENT_STATE.md`
- `changelog-ssot.md`

### If runtime/integration changed
Update:
- relevant runtime SSoT
- `CURRENT_STATE.md`
- `changelog-ssot.md`
- boundary doc if ownership/contract changed

### If only planning changed
Update:
- planning docs only

### If only legacy cleanup occurred
Update:
- archive headers / migration notes as needed

---

## 9. Conflict resolution rule

When two docs disagree, resolve in this order:

1. `SSoT_CONTRACTS.md`
2. Relevant subsystem SSoT
3. `CURRENT_STATE.md` for latest validated active slice
4. `coverage-matrix.md`
5. planning/reference/archive

If a conflict remains unresolved, record it explicitly in `CURRENT_STATE.md` until fixed.

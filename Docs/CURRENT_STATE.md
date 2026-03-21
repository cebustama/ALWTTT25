# CURRENT_STATE — ALWTTT

This file tracks the currently validated slice and immediate documentary obligations.

---

## 1. Active project slice now

### Gameplay / combat slice currently validated in docs
- deck/hand pipeline is operating in play mode
- cards can be played end-to-end
- effect execution is working in the current MVP slice
- validated effect examples include vibe, stress, and status application
- encounter structure now has an explicit governed home separate from combat and runtime flow

### Composition / music slice currently present in project structure
The project currently contains ALWTTT-side runtime components for:
- `GigManager`
- `MidiMusicManager`
- `CompositionSession`
- `SongConfigBuilder`
- `LoopScoreCalculator`
- composition UI/prefabs

This means ALWTTT owns a real runtime music/session surface and has governed runtime/integration docs for that surface.

### Audience / scoring / status / encounter slice now governed
The docs set now has governed homes for:
- audience entities and reaction contracts
- runtime status semantics and the canonical MVP status set
- LoopScore / SongHype / Vibe conversion meaning
- encounter-level gig structure, victory/failure, and gig-scoped state

---

## 2. What was just completed

### Documentation governance — Batch 06
Completed in this batch:
- restored the missing governed homes for audience, status, scoring, and the CSO reference catalog so the actual tree matches the root governance docs
- normalized planning into `planning/active/` and `planning/archive/`
- converted the old combat-roadmap path into a compatibility pointer
- added explicit snapshot-retention policy and snapshot metadata home under `archive/`
- completed the final navigation/alignment sweep across root docs

This is the batch where the governed docs tree became **replacement-ready** rather than merely migration-in-progress.

---

## 3. What is next

### Recommended next work
The governance migration is functionally complete.

Next work should be normal maintenance, not another migration batch:
1. use the governed tree as the active docs system
2. update docs through the normal SSoT maintenance loop after technical changes
3. only revisit migration if a new old-source cluster is discovered outside the current supersession map

---

## 4. Current blockers / residual risks

### Residual risk A — code/document alignment still depends on implementation changes
The docs tree is now structurally coherent, but implementation may still evolve.
That is a normal maintenance concern, not a migration blocker.

### Residual risk B — legacy/runtime coexistence remains a visible codebase risk
The codebase still shows both current and legacy-style surfaces in places, so docs must keep calling out which model is current whenever ambiguity can arise.

### Residual risk C — snapshot is still useful for forensic comparison
The snapshot is no longer needed day-to-day, but it is still worth keeping as historical backup until normal maintenance settles.

---

## 5. Docs that must be edited next

After the next meaningful technical change, edit:
- the primary affected SSoT
- `CURRENT_STATE.md` if the active operational slice changed
- `changelog-ssot.md` if meaning/authority changed
- `coverage-matrix.md` only if the primary home changed

---

## 6. Working rule

`CURRENT_STATE.md` answers:
- what is active now
- what was just completed
- what comes next
- what is blocked
- which docs need editing next

It does **not** replace subsystem SSoTs.

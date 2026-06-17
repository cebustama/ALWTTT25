# Combat_MVP_Roadmap — ALWTTT

**Status:** Planning only  
**Source:** Reclassified from previous `canon/Roadmap_Combat.md` patch/addendum  
**Rule:** This document does not define current implementation truth. It tracks recommended next work for the combat MVP slice.

---

## What was completed in the prior combat slice
- Deck/Hand pipeline stabilized enough for play-mode validation
- Cards can be played end-to-end through the current gig drop/use path
- `ModifyVibe` and `ModifyStress` were validated as working
- `ApplyStatusEffect` was validated as applying stacks

---

## What remains to close the MVP validation slice

### 1) Composure absorption
Implement a single canonical path for incoming positive Stress so that:
- Composure is consumed first
- only the remainder becomes Stress
- the same path is used everywhere Stress can increase

### 2) Flow hook validation in loop scoring
Flow should visibly increase loop-level SongHype conversion when Flow stacks are present.

Recommended approaches:
- add the minimal MIDI/runtime data needed to let loop-end execute normally, or
- provide a debug loop-end simulation path that triggers the same scoring logic

### 3) Minimal visibility/logging
Useful readouts/logs for the slice:
- current Stress per musician
- current Composure stacks
- current Flow stacks
- loop-end `DeltaSongHype`

---

## Updated checklist
- [x] Start gig -> initial hand is drawn
- [x] Action card play path works end-to-end
- [x] `ModifyVibe` has visible effect
- [x] `ModifyStress` has visible effect
- [x] `ApplyStatusEffect` applies stacks
- [ ] Composure absorbs positive Stress before application
- [ ] Flow visibly amplifies loop-level SongHype conversion

---

## Recommended immediate task

Create or route all positive Stress through a single helper such as:

```text
ApplyIncomingStressWithComposure(...)
```

That keeps the MVP combat contract aligned with the governed docs and prevents duplicate stress paths.

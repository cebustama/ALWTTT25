# Design — Fill Window v0.1 (registered idea)

**Status:** Registered idea — **not scheduled**. Out of the Roster Expansion campaign (R0–R8); candidate post-campaign C2 "path" mechanic.
**Scope:** An end-of-loop timing window in which the player may play "fill" cards that produce drum fills; generalizable as a windowed-timing card mechanic.
**Classification:** `reference (planning)` — not a SSoT.
**Created:** 2026-07-23 (Roster Expansion planning session; idea by Matías).
**Placement:** `Docs/planning/Design_Fill_Window_v0_1.md`

---

## 1. Concept

Near the end of each performance loop, a visible **window** opens on the loop progress bar. During that window, designated fill cards (initially C2 / Rhythm) become playable and produce a drum fill into the transition. First consumer: rhythm fills. The primitive — *a card class playable only inside a timed window* — is deliberately generic and reusable (e.g. future stingers, transitions, tempo pushes).

## 2. Constraint analysis (why this is not a quick batch)

1. **Session invariant collision.** Card mutations never touch the currently playing loop; they land in the part cache and are heard on the next loop (`SSoT_Runtime_CompositionSession_Integration` — the singer SSoT restates it as its inv 5). A fill audible *in the current loop's tail* violates this by definition. Two routes:
   - **(a) Live overlay, consumer-side.** A dsp-anchored one-shot player renders the fill (short MIDI, `RenderSinglePart`-class or pre-rendered clip) and plays it over the tail while ducking the drum channel via `MidiMusicManager.SetChannelVolume`. Pattern precedent: the singer subsystem (arm → dsp anchor → per-channel mute). Real-time feel preserved; cost = a new bounded subsystem + duck/restore care (Highlight×mute-family risk, currently a deferred validation).
   - **(b) Next-loop fill.** The fill card schedules a fill into the *end of the next loop's* render. Fully invariant-respecting, no new audio machinery; loses part of the real-time fantasy (the window becomes a scheduling window, not a performance window). Musically coherent.
2. **MidiGenPlay gap.** The package has no "fill measure" concept — drum patterns repeat-to-fill the part. Route (a) sidesteps the package entirely; route (b) done *inside* generation would be a package feature ask (fill-measure / last-measure-variant on the rhythm composer). Do not design that here (boundary rule).
3. **UI surface.** Loop progress bar with a marked window region + card-playability gating tied to loop time — a new presentation + input surface (`GigCanvas` / hand gating; relates to the CARD-UX-1 single-playability-source contract, which would gain a *temporal* predicate).
4. **Economy.** ECON-1: does a fill consume the composition budget, the action budget, or a third windowed budget? Undecided; must be resolved before implementation (D-ECON family).

## 3. Positioning

Post-campaign. Natural fit as a **C2 path**: the jazz/EDM palette rewards define *what* the kit plays; fills define *how it breathes*. If pursued, open with a decision batch choosing route (a) vs (b) and the economy answer; route (b)-in-generation additionally files the MGP ask with acceptance criteria.

## 4. Open questions

Window length/placement (musical: last measure? last N beats?) · miss/late feedback · fill content authoring (`DrumPatternData` short patterns? dedicated fill assets?) · interaction with the final-loop composition lock (a fill on the final loop is exactly the classic musical case — route (a) supports it, route (b) cannot) · reuse contract for the generic windowed-timing primitive.

## 5. Update rule

Update when the idea is scheduled (it then gets a batch + decision ledger) or when a dependency shifts (loop-bar UI, ECON changes, MGP fill support). Until then this note is the single record; no SSoT references it.

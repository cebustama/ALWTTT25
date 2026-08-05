# Design_Action_Economy_v1 — ALWTTT

**Status:** design record (v1) · **Date:** 2026-07-06 (session with Matías,
post audience tests) · **Implemented by:** batch ECON-1 (2026-07-07)
**Authority:** design rationale only. Implemented truth lives in
`SSoT_Gig_Combat_Core.md §14`. Where this doc and the SSoT diverge, the SSoT wins.

## 1. Problem
Pre-ECON-1, play frequency was gated only by Inspiration cost and timing
windows. With per-loop inspiration income (S5e: 3/loop) and several 0-cost
cards, a single musician could dump multiple cards per window, flattening the
band fantasy (turns didn't read as "each musician contributes") and making
cost the only pacing knob — a knob S5i would then have to over-tighten.

## 2. Rule (D&D-style action economy)
Max **1 Action + 1 Composition play per musician per period**. Period =
pre-song PlayerTurn window, or each performance loop. Pools independent.

## 3. Why Inspiration stays untouched
Cost remains an orthogonal gate. With the budget carrying the pacing load,
cost is repurposed as a RARITY/impact knob: cards with cost > 0 read as
"finishers" you save inspiration for; the baseline kit trends to cost 0.
This is deliberate layering — budget = tempo, cost = spike.

## 4. D-ECON-4 (strict Y=1) and the Wormus/Singing Field case
The sharpest consequence: Sibi cannot play Wormus AND Singing Field in the
same pre-song window. The hook enters as a mid-song add, audible from loop ≥2.
We chose the strict rule (A) over pre-song Y=2 (B) because the delayed hook is
itself expressive — the song grows a melody mid-performance — and because B is
a one-line revert if playtest reads the delay as punishment.
**Observation plan:** watch Matías's playtests specifically for frustration on
this line; log verdict here when observed.

## 5. Attribution (D-ECON-3=A)
AnyMusician cards bill the resolved performer (fixed → hover → SelectedMusician).
This matches the pre-existing pipeline attribution; the pips make it legible.
Known consequence: with SelectedMusician = list[0], generic actions default to
billing the frontman unless a fixed/hover target overrides — acceptable for
demo, revisit if selection UI evolves.

## 6. UI language
Two pips over each musician (Action / Composition). Lit → available;
dim → spent; refill at each period boundary. Goal: the economy should be
self-explanatory by watching the pips across one loop boundary
(demo-readiness criterion of the batch).

## 7. Deferred / debt
- **D12 — merge Sibi backing + bassline (CONT-B, 2026-07-31 — OPEN, deferred by user decision).**
  Whether Sibi backing cards auto-emit his bass line. Session decision: **keep A (status quo)** —
  the bass stays a separate card: a **player decision** and an economy lever. **B (merge)** is
  documented as the open alternative: musically correct (pianist's left/right hand) and it sounded
  better than solo piano in the CONT-B listening pass. Estimated cost of B, for when it is evaluated:
  touches `SongCompositionUI` (two tracks per card), per-musician role assignment, and Sibi's card
  economy; it is not a content change. Trade-off on record: merging converts a player decision into a
  default.
- Per-musician authoring of maxima (D-ECON-5 revisit) — e.g. a drummer with
  2 composition plays as identity.
- Pip denial flash (polish; only logs for now).
- Tutorial S4 copy review if it contradicts the new economy (debt noted at
  batch open; not rewritten in ECON-1).
- D-ECON-6 starter cost audit — **resolved 2026-07-07 as DEFER**: all starter
  cards set to cost 0; which cards become "finishers" (cost > 0) is deferred to
  a future design batch. Finisher costs will be tuned in S5i. See
  `Design_Starter_Deck_v1.md` §4 and `SSoT_Gig_Combat_Core.md` §14.6.
- **D-ECON-GENERIC — open.** Who spends the ECON-1 per-musician budget for an
  `AnyMusician` card (today: fixed → hover → `SelectedMusician`, §5). Blocks Keep
  Cool's move to the generic catalog (`Design_Tutorial_System_v0_2.md` §9.2). It now
  **also** gates half of the unplayable-card overlay: **CARD-UX-1 (2026-07-13) shipped
  the overlay's budget input partially scoped** — statically-resolvable payers only
  (`FixedPerformerType != None`); `AnyMusician` cards are excluded from the overlay's
  budget check because a false red is worse than a false green on an advisory surface.
  Enforcement is unaffected (`TryConsumePlay` still denies the drop). Resolving
  D-ECON-GENERIC unblocks the `AnyMusician` half. See `SSoT_Gig_Combat_Core.md` §14.5
  and `SSoT_Card_System.md` §10.5.

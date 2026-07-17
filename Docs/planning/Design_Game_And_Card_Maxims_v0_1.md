# Design_Game_And_Card_Maxims_v0_1 — ALWTTT

**Status:** Active — consolidated design maxims (philosophy/intent).
**Scope:** The durable game-design and card-design maxims that guide ALWTTT authoring going forward — the six we already had (lifted from where they lived) plus twelve derived from a cross-reference of how successful roguelike deckbuilders are balanced.
**Classification:** `reference (design philosophy)` — **not a SSoT**. These are principles and intent. Per the governance authority order, philosophy/intent *does not override* the normative baseline, any subsystem SSoT, or any accepted contract. When a maxim and a SSoT appear to conflict, the SSoT wins and the conflict is surfaced, not silently resolved here.
**Created:** 2026-07-16 (BALANCE-XREF). See `changelog-ssot.md` 2026-07-16.

---

## 1. Purpose

ALWTTT already had good design maxims, but they were scattered across `Design_Starter_Deck_v1`, `Design_Project_Directives_v0_1`, `Design_Action_Economy_v1`, and the project objective. A deckbuilder balance-research study (Slay the Spire 1/2, Monster Train 1/2, Griftlands; secondary references Balatro, Cobalt Core, Across the Obelisk, and others) produced a second, larger set of principles that repeat across every successful game in the genre.

This doc consolidates both into one scannable place so that card authoring, encounter design, reward/economy tuning, and Phase C / meta-progression decisions can be checked against a shared list. It does **not** invent policy: existing maxims keep their operational home (each is cross-referenced), and new maxims are evidence-tagged so a reader can weigh them.

**This is the home of the summarized maxims list** (§6). If you want the one-screen version, read §6.

---

## 2. How these maxims relate to the governed docs

- **Existing maxims (E1–E6)** are *lifted, not moved*. Their operational authority still lives where it did (E1/E2 in `Design_Starter_Deck_v1`; E3/E4 in `Design_Project_Directives_v0_1` as Standing Directives D1/D2; E5 in `Design_Action_Economy_v1` + `SSoT_Gig_Combat_Core §14`; E6 in the project objective). This doc restates them so the list is complete.
- **New maxims (N1–N12)** are design *intent* derived from research. They are not contracts. Where one implies a change, that change is filed to the batch that owns it (the sub-roadmap for demo-cut items, `Design_Vertical_Slice` for Phase C) — not enacted here.
- **Evidence priority** (per the research brief): developer statements > GDC talks > official blogs > interviews > community analysis. Every N-maxim cites its strongest source. Where the evidence is community-level, it is labelled as such.

---

## 3. The maxims

### A. Card design

**E1 — Mínimas cartas, máxima expresividad.** Each composition card plants **one distinct axis of musical contrast** (tonality, meter, tempo, progression palette, rhythmic feel, melodic style). Two cards of the same musician sit on different axes, or at maximum contrast on the same axis. *"Same card, slightly different number" fails and does not ship.*
*Home:* `Design_Starter_Deck_v1` → Design principle. *Existing.*

**E2 — The blind-listener test.** The acceptance test for any composition card: *would an untrained listener distinguish two songs that differ only in which of these two cards was played?* If not, the axis is too weak or the contrast too small. E2 is also the **culling knife** for N10.
*Home:* `Design_Starter_Deck_v1`. *Existing.*

**N1 — Every card must have a place — not equal power.** The goal is not that all cards are equally strong; it is that every card is *ever the right pick*. Build-around cards, staples, and finishers are all legitimate roles. ALWTTT gloss: composition cards are inherently content-bearing (they change the music), so a card earns its place by owning an axis, not by matching another card's power.
*Evidence:* StS "every card should have a place" (GDC 2019, Giovannetti); Balatro (LocalThunk — watch for cards "too good [that] cannibalise adjacent strategies" or "so bad there are few reasons to take it"). → Enforcement: N2 (pick-rate) + the TLM-1 logger.

**N4 — Cap the floor, not the ceiling: let the band break the game, rarely, by the player's own cleverness.** Occasional *earned* overpowered-ness is core genre appeal ("poggable moments"). Design at least one aspirational, multi-piece, hard-to-assemble combo per content wave; make going *infinite* genuinely rare, but do not flatten every peak. ALWTTT gloss: the current stack (ECON-1 + song-end banking + Indifference gates + flat SFX) caps every ceiling — the first candidate ceiling to build back is the deferred **Earworm × Captivated** pairing (the ALWTTT "Corruption + Dead Branch").
*Evidence:* StS Corruption + Dead Branch as a celebrated rare combo (GDC 2019 + PC Gamer); "going infinite is the number one thing we try to make really rare" (Gamasutra); Monster Train "poggable moments" (Cooke, Screen Rant). → Forward-pointed: Phase C / roster expansion (`CURRENT_STATE §4`).

**N6 — Prefer soft synergy; ration hard synergy.** Most of the pool should reward *discovered, implied* relationships (soft synergy); hard-locked pairs (a card that only works with one specific other card) are seasoning, not the meal. Too many hard synergies turn deckbuilding into a formality. ALWTTT gloss: Singing Field's hard dependence on a Wormus progression is fine at starter scale — keep the *majority* of future cards soft.
*Evidence:* Griftlands critique (hard-synergy-dominated pools render two-thirds of the cardpool inert for a given build); the soft/hard/flex taxonomy (community analysis, labelled). → Applies at catalog-growth time (Conito/Ziggy authoring, reward pool).

**N7 — Duplication is a legitimate tool — for onboarding and for consistency.** Shipping **multiple exact copies of one card** is allowed and useful: it gives card velocity, makes turn 1 look like turn 4 (onboarding), and lets a player lean into a consistency pick. This does **not** conflict with E1 — see §4.
*Evidence:* StS heavy starter duplication (5 Strike / 4 Defend); the research's finding that duplication is an onboarding device, not only a power lever. → ALWTTT already does this (Default Mode ×2, Wormus ×2); sanctioned as the S5i-L1 fallback if the 15-unique starter reads as confusing.

### B. System & run design

**E5 — Budget = tempo, cost = spike.** The per-musician play budget (1 Action + 1 Composition per period, ECON-1) paces the turn; Inspiration **cost** is an orthogonal knob repurposed as a rarity/impact marker — cost-0 cards are the baseline kit, cost->0 cards read as *finishers* you save for.
*Home:* `Design_Action_Economy_v1` + `SSoT_Gig_Combat_Core §14`. *Existing.*

**N8 — A run must feel like a rising arc.** The band should be visibly and audibly stronger at the boss than at gig 1. Rewards and relic-equivalents (SFX-as-equipment) are the *spine* of that growth; without it a run is a sequence of fights, not a run. ALWTTT gloss: Phase C as currently scoped (3 gigs + boss, ~3 picks from a ~2-card pool) has almost no spine — this is the single biggest Phase C design gap.
*Evidence:* felt power growth Act 1→3 as the defining run property (across StS, Monster Train scaling, community analyses). → Forward-pointed: `Design_Vertical_Slice §3.1 / §11` (run power curve).

**N9 — Skipping a reward is a real choice.** The player must be able to *decline* a card to protect deck consistency; skip is a first-class lever, not a failure state. ALWTTT gloss: matters more here than in StS — with guaranteed draws and a 17-card deck, every forced pick dilutes draw slots.
*Evidence:* StS 3-card-choice + skip ("skipping is the right choice more often than you might think"); Legends of Runeterra's *inability* to skip is a deliberate, different design (community analysis). → Locked BR-D1=B; implemented Phase C S6 (`Design_Vertical_Slice §3.1`).

**N11 — Difficulty tiers are also a measurement instrument.** A Covenant/Ascension-style ladder is not only challenge — it *segments telemetry by skill*, letting you check win rates per player tier. Design it as data infrastructure, not a bolt-on. ALWTTT gloss: plan it early in meta-progression, the day the tester pool exceeds a handful.
*Evidence:* StS Ascension used to sort metric data ("look at ascension 20 players … make sure win rates are where I want them", Giovannetti podcast + GDC 2019); Monster Train Covenant; Griftlands Prestige. → Forward-pointed: meta-progression (`CURRENT_STATE §4`).

**N12 — Every legal band is a viable band.** No roster combination should be a dead pick at any difficulty. ALWTTT gloss: musician-picking *is* Monster Train's clan-pair choice; this is the archetype-balance North Star as the roster grows past 2.
*Evidence:* Shiny Shoe's stated target — "clan combinations viable up to the highest difficulty level"; most clans share near-identical win rates (Cooke / gamepressure; PC Gamer MT2 Q&A). → Applies at roster expansion (Conito/Ziggy) and encounter balance.

### C. Process & data

**E6 — Always keep a working, showable build.** Every milestone produces something demonstrable. This is the project objective, restated as a maxim because it governs sequencing decisions (e.g., BR-D4: consolidate now, don't pause the near-tag build for a rebuild).
*Home:* project objective / project instructions. *Existing.*

**E3 — Sound design is a maximum priority.** Audible identity is designed *in*, not polished *on*; it overrides authoring convenience when they conflict. (Standing Directive D1.)
*Home:* `Design_Project_Directives_v0_1 §D1`. *Existing.*

**E4 — Every player-visible state change produces a sensory artifact.** (Standing Directive D2 — the Sensory Contract.) Listed here because it is also a *card-design* constraint: a card's effect must be perceivable.
*Home:* `Design_Project_Directives_v0_1 §D2` + `Design_Sensory_Contract_v0_1`. *Existing.*

**N2 — A card that is never picked is not in the game.** If telemetry or observation shows a card is essentially never chosen, it must be buffed, re-identified, or cut — "too low [a pick rate] and it's basically not a card in our game." ALWTTT gloss: this is *the* reason TLM-1 exists.
*Evidence:* StS pick-rate as one of the two primary metrics (Gamasutra, Yano/Giovannetti); Monster Train "look for underpowered or not-being-picked content and make it more interesting" (Cooke). → Enforcement: TLM-1 logger → S5i.

**N3 — Data is evidence, not a verdict.** Telemetry is one tool among several; cross-check it against feel, and beware confounds. With ALWTTT's n≈2 tester population, tune *economy and pacing* off small samples — never individual card magnitudes. Watch late-run confounds specifically.
*Evidence:* StS "don't rely on metrics too much" (GDC 2019); the Madness card (looked overpowered only because it appears late, after weak decks are already dead, PCGamesN); super-playtester sampling bias (GDC 2019). ALWTTT gloss: the identical trap is already built — SFX Vibe banks at song end and the "fire" stage fires late-gig, so any song-3 card spuriously correlates with wins. This is why TLM-1 logs **song-index-at-play-time**. → `S5_DemoCutClose_Sub_Roadmap` → TLM-1 confound guard.

**N5 — Numbers are the cheapest thing to change; identity is the most expensive.** Revalue freely and iterate aggressively; don't fear breaking things, and expect non-linear progress. The one carve-out: when a nerf collides with a card's *identity*, find a different nerf. ALWTTT gloss: doc-update overhead must never make a number change feel expensive — numbers should be the cheapest thing in the project to change.
*Evidence:* STS2 balance loop — "pick gut-level values … tweak or rework and go back" (AMA); "no change is necessarily permanent" and the **Prepared** rollback, where a nerf that hit the Silent's "core identity" was reverted in favour of "a different approach" (PC Gamer, March 2026); Balatro "almost all balance = changing one number". → Governs S5i tuning posture and every future balance pass.

**N10 — Over-generate, then cull.** Author 2–3× candidate cards per shipped card; use E2 (the blind-listener test) as the culling knife. ALWTTT gloss: the CardLLM pipeline is the generation engine; the listener test is the filter. Plan for this when authoring Conito/Ziggy catalogs and the reward pool.
*Evidence:* StS/STS2 — "you generate thousands … 'these are bad,' and you just cut them all away … constant culling process" ("it's kind of like you're a butcher", ~100–200 ideas per character pared to ~60; PC Gamer). → Applies at catalog authoring.

---

## 4. The E1 ↔ N7 reconciliation (read this before authoring copies)

E1 ("*mínimas cartas*") and N7 ("duplication is legitimate") govern **different things** and do not conflict:

- **E1 is about axis distinctness.** It forbids two *different* cards that occupy the *same axis with a small contrast* — "same card, slightly different number." Two cards should never be near-duplicates of each other.
- **N7 is about copy count.** It permits shipping **multiple exact copies of one card** — Default Mode ×2, Wormus Minor ×2, Wormus Major ×2 — when the copies serve card velocity, onboarding legibility, or deck consistency.

The Wormus ×2 and Default Mode ×2 multiplicities already in the starter are N7 in action, and were never an E1 violation. The practical consequence for S5i: if the 15-unique starter reads as confusing to a first-time player (observation lens **L1**), **adding exact copies of the most legible cards is the sanctioned fix** — it reduces variety without collapsing any axis.

---

## 5. What the research changes for ALWTTT (compact)

The full cross-reference (alignments, gaps, warnings, staged recommendations) is in the BALANCE-XREF analysis; the operational decisions live in the sub-roadmap (BR-D1..4) and `Design_Vertical_Slice`. In brief:

**Strong alignments (keep):** E2's blind-listener test is a *stricter, testable* version of the genre's top card-quality bar (no referent has an equivalent objective test — it is a genuine asset). ECON-1 makes infinites architecturally impossible (stronger than tuning-dependent StS). Band-picking = Monster Train's clan-pair system (→ N12). SFX-as-equipment is the ALWTTT relic layer (→ N8).

**Gaps (act on):** no reward skip (→ N9, BR-D1); no per-card telemetry (→ N2/N3, TLM-1/BR-D2); no run power curve (→ N8, Phase C); no "break-the-game" ceiling (→ N4, Phase C).

**Warnings (heed):** super-playtester bias applies maximally at n≈2 — tune economy/pacing, not card magnitudes (N3). The Madness/SFX late-run confound is already built (→ log song-index). The zero-margin finisher (Psychic Waves at exactly 3-loop income) risks reading as a trap (→ S5i-L3, N5). Tuning conservatism ≠ contract conservatism — numbers should be cheap to change (N5).

---

## 6. The summarized list

The maxims in one screen. **E# = existing (already governed); N# = new (research-derived, 2026-07-16).**

**Card design**
- **E1.** Mínimas cartas, máxima expresividad — one distinct axis of contrast per composition card.
- **E2.** The blind-listener test — two songs differing only by one card must be distinguishable by an untrained ear.
- **N1.** Every card must have a place — not equal power.
- **N4.** Cap the floor, not the ceiling — let the band break the game, rarely, by the player's own cleverness.
- **N6.** Prefer soft synergy; ration hard synergy.
- **N7.** Duplication is a legitimate tool — for onboarding and for consistency (see §4 for the E1 reconciliation).

**System & run design**
- **E5.** Budget = tempo, cost = spike.
- **N8.** A run must feel like a rising arc.
- **N9.** Skipping a reward is a real choice.
- **N11.** Difficulty tiers are also a measurement instrument.
- **N12.** Every legal band is a viable band.

**Process & data**
- **E6.** Always keep a working, showable build.
- **E3.** Sound design is a maximum priority.
- **E4.** Every player-visible state change produces a sensory artifact.
- **N2.** A card that is never picked is not in the game.
- **N3.** Data is evidence, not a verdict.
- **N5.** Numbers are the cheapest thing to change; identity is the most expensive.
- **N10.** Over-generate, then cull.

---

## 7. Research grounding & sources

The N-maxims derive from a developer-primary study prioritizing: GDC 2019 "Slay the Spire: Metrics Driven Design and Balance" (Giovannetti); the Slay the Spire 2 Reddit AMA and Mega Crit's 2026 beta-balance statements (incl. the Prepared rollback); Gamasutra/Game Developer's data-balance interview (Giovannetti + Yano); Shiny Shoe interviews (Cooke — Screen Rant, gamepressure) and the Monster Train 2 PC Gamer Q&A; and Klei's Griftlands developer posts (Forbes). Secondary/community sources (Balatro's LocalThunk interview; the Slay the Spire official data-dump analyses; genre design critiques) are labelled as lower-confidence where cited. The consolidated source database, design-principle evidence table, and comparison matrix are held in the BALANCE-XREF research artifact referenced from `changelog-ssot.md` 2026-07-16.

*This doc is philosophy/intent. It does not govern runtime truth, does not override any SSoT or contract, and should be revised as playtest evidence accumulates.*

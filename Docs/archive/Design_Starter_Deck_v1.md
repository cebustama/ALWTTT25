# Design_Starter_Deck_v1 — ALWTTT

**Status:** Active design proposal — planning only, subject to playtest revision  
**Scope:** Composition, per-card effect spec, per-musician identity mapping, tuning rationale, and open questions for the first starter deck the player uses at the start of a run  
**Classification:** `reference (planning)` — **not a SSoT**. This document expresses design intent. When M4.6 authoring closes, the authored `.asset` files become the runtime-authoritative version; this document is then retained as historical rationale.  
> **Classification clarified 2026-08-08 (MANIFEST-1, D10=B).** This file lives at
> `Docs/archive/Design_Starter_Deck_v1.md`, and that placement is **correct**: by this
> document's own Classification clause, once M4.6 authoring closed the authored `.asset`
> files became the runtime-authoritative version and this document became retained
> rationale. `SSoT_INDEX.md` had been listing it under *Active planning docs* at
> `planning/active/` and calling it "authoritative for the live S5 demo starter" — that
> wording was the error, not the folder, and it has been corrected. **What this document
> still is:** the design rationale for the 17-card, 2-musician S5 demo starter — why each
> card exists, the per-musician identity mapping, the tuning reasoning. **What it is not:**
> a definition of what the build ships. That is the `.asset` files. Recorded as manifest
> finding **F18**. Forward intent lives in `planning/active/Design_Starter_Deck_v2_DRAFT.md`,
> which takes over as the design record at R8 closure.

**Last updated:** 2026-05-20 (cross-reference paths updated for MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md move to `integrations/midigenplay/`; no content change to design intent)

**v1.1 (2026-05-22).** Three new cards added to starter (Push It, Half Time, Key Lift). Wormus pair multiplicity doubled (2 each) per D-STARTER-1=B. Compound Cycle + Pentameter moved out of starter into reward pool per D-STARTER-2=B. Starter size 12 → 15 cards. See `changelog-ssot.md` 2026-05-22 entry for context.

**v1.2 (2026-07-07).** All starter Inspiration costs set to 0 per ECON-1 / D-ECON-6=DEFER (Warm Up, Mind Tap, Push It, Half Time, Key Lift: cost 1→0; gen unchanged). The "finisher" layer (cost ≥1 cards) is designed but its card assignments are deferred to a future batch; see the §4 note, §8, and `SSoT_Gig_Combat_Core.md` §14.6. Play-economy truth: `SSoT_Gig_Combat_Core.md` §14.

**v1.3 (2026-07-09).** Two cards added for tutorial coverage (TUT-REBUILD): **Psychic Waves** (Sibi, Action/EMT, cost 3, `ModifyVibe +5` to `AllAudienceCharacters`) — the guided finisher (beat 8); and **Keep Cool** (**C2**, Action/CHR, cost 0, `ApplyStatusEffect composure +3 Self`) — the didactic Composure source, pairing with Take Five as the defensive heal/block pair. **Starter size 15 → 17.** Keep Cool is authored **C2-owned** (`FixedMusicianType`), not generic — the generic-catalog move is deferred pending **D-ECON-GENERIC** (who spends the ECON-1 per-musician action budget for an `AnyMusician` card; home `Design_Action_Economy_v1` / `SSoT_Gig_Combat_Core §14`). Psychic Waves partially closes the finisher layer (one card, cost ≥1); deep tuning stays S5i. See `changelog-ssot.md` 2026-07-09.

**v1.4 (2026-07-16).** No card changes. Documentation-only pass from the deckbuilder balance-research cross-reference (BALANCE-XREF; see `changelog-ssot.md` 2026-07-16 and the new `planning/Design_Game_And_Card_Maxims_v0_1.md`). Two edits: (1) the §4 "Note on uniqueness ratio" was **corrected** — it still read the stale "12 cards / 10 unique" from before the v1.1/v1.3 growth to 17/15 — and re-framed as a *deliberate* divergence from genre norms whose comprehension cost is verified in S5i; (2) the *mínimas cartas* principle below now cross-references the governed maxims doc, which reconciles it with the newly-adopted maxim that **exact-copy duplication is a legitimate onboarding/consistency tool** (this is what the Wormus ×2 / Default Mode ×2 multiplicities already do — E1 forbids near-*duplicate axes*, not multiple copies of one card).

> **Terminology correction (2026-07-31, D-S5e-DOC-D):** every `VibeGoal` reference in this document predates the S5e inversion. Runtime truth since S5e: `VibeGoal` was retired into `MaxVibe`; audience Vibe is a resistance pool that starts at `MaxVibe`, is depleted by incoming Vibe, and Convinced fires at `Vibe <= 0`. The tuning numbers here (ranges 15–20 / 35–45, ST-SD-8) map to `MaxVibe` values. Historical design record — text not rewritten.


## Design principle: mínimas cartas, máxima expresividad

The guiding principle for composition-card authoring in ALWTTT is: **minimal number of cards, maximal musical expressiveness per card**.

Operationally:
- Each composition card should plant one *axis of musical contrast* (tonality, meter, tempo, progression palette, rhythmic feel, melodic style, etc.).
- Two cards of the same musician should either sit on different axes, or sit at maximum contrast on the same axis.
- "Same card with a slightly different number" fails this principle and should not ship.

The acceptance test for a composition card is: *would an untrained listener distinguish two songs that differ only in which of these two cards was played?* If not, the axis chosen is too weak or the contrast is too small.

The catalogue of axes a card can actually affect — with the carrier (PartEffect or styleBundle), the `SongConfig` field reached, and the per-card controllability status — is in `integrations/midigenplay/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` §3. The starter cards' axis assignments (now resolved, see §5) used that matrix as evidence base.

This principle is binding for the starter deck and recommended as default for any future composition-card authoring in ALWTTT.

> **Governed maxim (2026-07-16).** *Mínimas cartas, máxima expresividad* and the blind-listener test are now catalogued as maxims **E1** and **E2** in `planning/Design_Game_And_Card_Maxims_v0_1.md` (the project's consolidated game/card-design maxims, philosophy-class). That doc also records the reconciliation with maxim **N7** (*duplication is a legitimate tool — for onboarding and for consistency*): E1 forbids two cards that occupy the *same axis with a small contrast* ("same card, slightly different number"), whereas shipping **multiple exact copies of one card** — Default Mode ×2, Wormus Minor/Major ×2 — is explicitly permitted when it serves card velocity, onboarding legibility, or deck consistency. The two maxims do not conflict; they govern different things (axis distinctness vs. copy count).

### Tempo-lean as design intent (not runtime)

Per `planning/Design_Tempo_Identity_v1.md`, ALWTTT carries a long-term design direction where cards interact with song tempo to produce fast-favoring vs slow-favoring deck identities ("metal" / "fast jazz" / etc.). That system is not implemented and is not scheduled for MVP. **However**, starter deck card naming, flavor, and per-musician card-catalog shape should reflect a soft tempo lean per musician now, so that later tempo-coupled card additions retrofit naturally rather than requiring renames or behavior swaps.

This is **design intent, not runtime**. No starter-deck card reads tempo at play-time. No starter-deck card has tempo-conditional output. The lean lives in:
- card names (`"Steady Beat"` reads slow; `"Quick Riff"` reads fast),
- card flavor / fantasy descriptions,
- the rough shape of each musician's catalog.

When tempo coupling eventually lands, individual cards may receive tempo-conditional behavior in subsequent design passes. The starter cards themselves remain unchanged in the immediate post-MVP timeframe.

Per-musician tempo lean (rough sketch — refine during M4.6 starter authoring):
- *(One sentence per musician — Ziggy / Conito / Robot C2 / Gusano. Author during M4.6 starter design pass; this paragraph is a placeholder for that work.)*

---

## 1. Purpose

This document is the design record of the first starter deck for ALWTTT, produced by the design session on 2026-04-21 and refined on 2026-04-26 to resolve the per-card axis assignments using the matrix in `MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md`.

It exists so that, when M4.6 authoring begins (after M4.1–M4.5 land), the author does not have to reconstruct the design from session reports or from memory. It also surfaces explicit open questions that must be resolved before authoring — these are listed in §9.

It is a planning document. Numbers, card names, and effect magnitudes are first-guess design choices and expected to shift at playtest. Card names marked *(working name)* are placeholders pending naming-pass.

---

## 2. Design intent and constraints

### 2.1 What this deck needs to do

- Be the deck the player uses at the **start** of a run in the MVP demo.
- Express the composition-first identity of ALWTTT (the game is about making music, not about combat).
- Give two musicians (C2 and Sibi) legible mechanical identities.
- Be jugable end-to-end: a full gig with audience conviction possible and audience pressure survivable.
- Fit within the runtime contract **as it will be** after M4.1–M4.5 land (NOT as it is today). This is why M4 sequences infrastructure before authoring.
- Be honestly tunable, not hypothetically tunable — requires C1 fixed (M4.1) and runtime values received from the user.

### 2.2 What this deck is not

- **Not a final deck design.** Expected to be revised substantially after first playtest.
- **Not a design that assumes the full 4-musician band.** That is a post-MVP scope decision; see Roster Expansion in `Roadmap_ALWTTT.md`.
- **Not a SSoT.** It does not govern runtime truth. When the `.asset` files exist, runtime truth is those files.

### 2.3 Constraints honored

- Deck contains **copies of cards**, not only unique references. Requires M4.4 (Deck Contract Evolution).
- Uses only `CardEffectSpec` types already in the active vocabulary: `ApplyStatusEffect`, `ModifyVibe`, `ModifyStress`, `DrawCards`. No new ALWTTT-side effect types introduced.
- Composition cards use existing MidiGenPlay-side `PartEffect` types (`MeterEffect`) and existing `TrackStyleBundleSO` subclasses (`BackingCardConfigSO`, `MelodyCardConfigSO`) — no new package-side types introduced.
- Audience-side status usage limited to **Earworm** only (added in M4.3). Captivated and `ApplyIncomingVibe` are explicitly deferred.
- Flow mechanics align with the bifurcated model introduced in M4.2 (flat on Action, multiplier on Composition + Song End).
- Bidirectional guaranteed-draw mitigation (M4.5) is assumed active; without it, the 8:4 ratio misfires at ~7% of action windows.

---

## 3. Roster

### 3.1 Included in starter

**Robot C2** — Drummer / drum machine. Cyborg-armored "música genuina con fallos" math-jazz scholar; turned himself immortal to play music forever.
- Fantasía: metronomic precision and mathematical curiosity. C2 picks **the song's clock** — the meter the band locks into. Each meter is a different machine mode.
- Role in composition pipeline: Rhythm (percussion).
- Stat bias: TCH dominant.
- Mechanical identity in starter: **meter-axis controller** (axis 7 — `PartConfig.TimeSignature` via `MeterEffect`) and **source of Flow** for the band via the default 4/4 mode. Drives momentum that amplifies both action-card Vibe bonuses (flat) and composition-card + song-end Vibe conversion (multiplier).

**Sibi** — Keyboardist, hive-mind worm-like entity from the Asteroid Singing Fields with empathic / expectation-shaping affinity.
- Fantasía: the color and the hook. Reads and shapes audience emotional expectation. Earworm is literally what she does — her songs get stuck in the audience's head, and her species' origin is a singing asteroid field.
- Role in composition pipeline: Backing (chord progression palette) for composition; Melody (phrase palette) for the hook.
- Stat bias: CHR + EMT mix.
- Mechanical identity in starter: **mode-keeper** (axis 13 — picks the song's emotional world via `BackingCardConfigSO.progressionPalette`) and **earworm applicator** on single-target audience members (only audience-state manipulator in MVP). Her composition and her action vocabulary say the same thing in two registers — *make the song memorable*.

### 3.2 Deferred to roster expansion

**Conito** — Bassist, flight + electricity. Bass pipeline validation not yet on critical path.
**Ziggy** — Vocalist, multiharmony. Carries Captivated as identity status (post-MVP).

Both are documented in `Roadmap_ALWTTT.md` → Future Milestones → Roster Expansion.

### 3.3 Selection mechanism (M4.6-prep merged (1)/(4))

The C2 + Sibi roster above is the **default** starter band, not a hardcoded one. At runtime the band is composed per-run through the Gig Setup band picker (`GigSetupController`); the picker reads `gameplayData.AllMusiciansList` for available musicians and uses `gameplayData.InitialMusicianList` only as the first-visit default selection. Subsequent visits remember the previous run's roster via `pd.MusicianList`.

**For shipping this design as the fresh-install starter:** edit `gameplayData.InitialMusicianList` to contain C2 + Sibi. This sets the default selection on the first GigSetupScene load. It is not a per-run knob — runtime users compose the band via the picker.

Picker semantics (override decision, validation rules, multiset-blind audience comparator) are governed by `SSoT_Gig_Encounter.md §7` and `SSoT_Editor_Authoring_Tools.md §16.1`.

---

## 4. Deck composition — the 17 cards

**Starter deck size: 17 cards.** Domain split: **6 Action / 11 Composition**. Composition by performer: 5 C2 + 6 Sibi (no Cantante card in the starter). *(VERIFY-DOC-STARTER-1 resolved: the earlier "5 Action / 12 Composition" was wrong — both new cards are Action.)*

| # | Card | Unique? | Copies | Domain | Owner | Effect (authoritative for MVP) | Inspiration cost / gen | Notes |
|---|---|---|---|---|---|---|---|---|
| 1 | Warm Up | ✓ | 2 | Action | Any | `DrawCards(2)` | 0 / — | Didactic generic — the "Strike" analog |
| 2 | Take Five | ✓ | 1 | Action | Any | `ModifyStress(-3, Self)` | 0 / — | Defensive generic |
| 3 | Mind Tap | ✓ | 1 | Action | Sibi | `ModifyVibe(+5, AudienceCharacter)` + `ApplyStatusEffect(earworm, +2, AudienceCharacter)` | 0 / — | Sibi identity Action — plants Earworm |
| 4 | Default Mode *(working name)* | ✓ | 2 | Composition | C2 | Rhythm. `MeterEffect(4/4)` PartEffect + `ApplyStatusEffect(flow, +1, Self)` co-effect on play | — / 3 | C2 default heartbeat + Flow source |
| 5 | Waltz Protocol *(working name)* | ✓ | 1 | Composition | C2 | Rhythm. `MeterEffect(3/4)` PartEffect | — / 3 | C2 waltz mode |
| 6 | Push It *(new, v1.1)* | ✓ | 1 | Composition | C2 | Rhythm. `TempoEffect.ScaleFactor(×1.5)` PartEffect | 0 / 2 | C2 BPM card — speeds up the song |
| 7 | Half Time *(new, v1.1)* | ✓ | 1 | Composition | C2 | Rhythm. `TempoEffect.ScaleFactor(×0.66)` PartEffect | 0 / 2 | C2 BPM card — slows the song |
| 8 | Wormus Minor | ✓ | 2 | Composition | Sibi | Backing. `BackingCardConfigSO` with minor-mode `progressionPalette` (entries with `tonalities = [Aeolian/Dorian/Phrygian]`) | — / 2 | Sibi minor-mode anchor — multiplicity 2 per D-STARTER-1=B |
| 9 | Wormus Major | ✓ | 2 | Composition | Sibi | Backing. `BackingCardConfigSO` with major-mode `progressionPalette` (entries with `tonalities = [Ionian/Mixolydian/Lydian]`) | — / 2 | Sibi major-mode anchor — multiplicity 2 per D-STARTER-1=B |
| 10 | Singing Field *(working name)* | ✓ | 1 | Composition | Sibi | Melody. `MelodyCardConfigSO` with `phrasePaletteOverride` set to a hook-shaped `PhrasePaletteSO` (specific asset TBD at authoring) | — / 3 | Sibi melodic hook — inherits progression from Wormus card via shared-progression mechanic |
| 11 | Key Lift *(new, v1.1)* | ✓ | 1 | Composition | Sibi | Backing. `ModulationEffect.IntervalWithinScale(degree=5)` PartEffect | 0 / 2 | Sibi Modulation card — shifts root up a fifth |
| 12 | Psychic Waves *(new, v1.3)* | ✓ | 1 | Action | Sibi | `ModifyVibe(+5, AllAudienceCharacters)` | 3 / — | Guided finisher (TUT-REBUILD beat 8) — AoE, no target select |
| 13 | Keep Cool *(new, v1.3)* | ✓ | 1 | Action | C2 | `ApplyStatusEffect(composure, +3, Self)` | 0 / — | Didactic Composure source — heal/block pair with Take Five |

**Derived counts (v1.1):**
**Cost semantics post-ECON-1 (D-ECON-6=DEFER, 2026-07-07):** all starter cards are cost 0 today. The "finisher" layer (cards with cost ≥1) is designed but its card assignments are deferred to a future batch — see `SSoT_Gig_Combat_Core.md` §14.6 and `planning/active/Design_Action_Economy_v1.md` §3/§7. Finisher costs will be tuned in S5i.

- 6 Action (total copies): 2 Warm Up + 1 Take Five + 1 Mind Tap + 1 Psychic Waves (Sibi) + 1 Keep Cool (C2).
- 11 Composition (total copies): 2 Default Mode + 1 Waltz Protocol + 1 Push It + 1 Half Time + 2 Wormus Minor + 2 Wormus Major + 1 Singing Field + 1 Key Lift.
- C2 composition coverage: 5 copies (2 + 1 + 1 + 1).
- Sibi composition coverage: 6 copies (2 + 2 + 1 + 1).

### Reward pool (cards not in starter)

Cards held in the per-musician catalog with `flags: UnlockedByDefault | RewardPool` and `StarterDeck` flag cleared. **As of S5h (2026-07-07, D2=B) this flag IS the runtime reward source:** `PersistentGameplayData.BuildRewardCardPool()` collects `RewardPool ∩ UnlockedByDefault` entries from the current band's catalogs, excluding cards already in the run deck (D9). The old `RewardContainerData` card-list path is retired for card rewards (the asset is now presentation-only: box sprite + description). Reward cards granted via the screen route through the provenance-aware `GrantRewardCard` (D4), so a composition reward files into the composition deck, not the action deck.

- **Compound Cycle (6/8)** — moved 2026-05-22 per D-STARTER-2=B. Compound meter reserved as a post-demo unlock to give the meter axis somewhere to go across runs.
- **Pentameter (5/4)** — moved 2026-05-22 per D-STARTER-2=B. Asymmetric meter reserved as reward-pool card.

**Ratio asymmetry is intentional:** C2 picks the song's clock (more copies, broader meter coverage, with 4/4 as soft default at ×2). Sibi picks the song's emotional world and its hook (fewer copies, each card a distinct expressive lever).

**Note on uniqueness ratio (corrected 2026-07-16).** The current starter is **17 cards / 15 unique** (v1.3). *(The prior text here read "12 cards / 10 unique", stale since the v1.1 growth to 15 and the v1.3 growth to 17 — corrected in the v1.4 doc pass.)* This is far heavier on variety than typical deckbuilder starters (Slay the Spire ~10 cards / 4 unique; Monster Train 2 ~15 cards / 5 unique). The high unique count is a direct consequence of *mínimas cartas, máxima expresividad* (maxim E1): composition cards are **content-bearing** — each one changes the music on a distinct axis — so a duplicate adds far less here than a duplicated Strike does in StS.

**This divergence is deliberate, but it carries a known cost the balance research flags.** Heavy starter duplication is not only a power lever in the referents — it is an **onboarding device**: turn 1 looks like turn 4, so new players learn the loop faster. A 15-unique starter works *against* that, and the demo's success signal (D-REPLAN-1) is *unassisted comprehension*. The mitigations already carrying that load are the guaranteed-draw fallbacks (M4.5), the guided tutorial (TUT-REBUILD), and the 2-musician roster. **S5i must explicitly verify comprehension holds at this uniqueness level** (it is one of the three S5i observation lenses added by BALANCE-XREF — see `planning/active/S5_DemoCutClose_Sub_Roadmap.md` → S5i). If comprehension fails, *reducing starter variety by adding exact copies of the most legible cards is a permitted fallback* under maxim N7, not a violation of E1 — see the "Governed maxim" note in the Design principle section above.

---

## 5. Per-card design rationale

### 5.1 Warm Up (Action, generic, ×2)

`DrawCards(2)` at cost 0 (ECON-1 / D-ECON-6=DEFER; was 1).

- Didactic: the first Action most players will play. Teaches the concept of hand management.
- 2 copies so the deck always has card velocity.
- The analog of StS's Strike not in damage, but in ubiquity.

### 5.2 Take Five (Action, generic, ×1)

`ModifyStress(-3, Self)` at cost 0.

- Defensive generic. Recovery tool.
- 1 copy because the starter is intentionally "piano" — scaling stress management happens at deck expansion, not at starter.
- Targets `Self`, which in runtime resolves to the card's performer (whoever plays it).

### 5.3 Hype It Up — RETIRED FROM STARTER

`ModifyVibe(+3, AudienceCharacter)` was originally proposed as a third generic Action. Removed after session-end review: Mind Tap covers the "direct Vibe push" niche more strongly by bundling Earworm, and a fourth Action copy would dilute the composition-first ratio. Kept here only as historical note; not in the deck.

### 5.4 Mind Tap (Action, Sibi, ×1)

`ModifyVibe(+5, AudienceCharacter)` + `ApplyStatusEffect(earworm, +2, AudienceCharacter)` at cost 0 (ECON-1 / D-ECON-6=DEFER; was 1).

- Sibi's only Action. Her main way of impacting the audience directly during action windows.
- The +5 instant Vibe and 2 Earworm stacks are both audible, visible, immediate wins — a "signature spell" feel.
- The two effects compose cleanly: the instant Vibe gets amplified by Flow (flat path, from M4.2). The Earworm stacks generate +2 Vibe on the next audience turn, then +1, then 0 (assuming no refresh). Total expected Vibe contribution over the hit and 2 audience turns: 5 + 2 + 1 = **8 Vibe** (ignoring Flow amplification, ignoring Captivated which doesn't exist yet).
- 1 copy is intentional — Sibi being "rare" in the Action phase maps to her fantasía of mental precision.
- **FixedPerformerType: Sibi.** Only Sibi can play this card.

### 5.5 C2 rhythm card set — overview

C2's four composition cards plant a single axis: **axis 7 — Meter / time signature** (per `MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` §3, ✅ per-card via `MeterEffect` PartEffect → `PartConfig.TimeSignature`).

The four cards sit at four well-separated points on this axis: 4/4, 3/4, 5/4, 6/8. These are the four meters currently first-class supported in the MidiGenPlay rhythm runtime (pending §9 #2 confirmation). They are arguably the four most audibly distinguishable meters in the practical Western space — they don't shade the listener experience, they restructure it.

**Carrier:** each card carries a single `MeterEffect` in its `modifierEffects: List<PartEffect>`. The card's `TrackActionDescriptor.styleBundle` may be a shared-or-empty `RhythmCardConfigSO` (no per-card rhythm-pattern differentiation in v1 — that surface is reserved for later cards). This means six fewer SOs to author than originally projected.

**Null-preservation semantics (post-2026-05-22, D-MOD-FIX=A).** An empty (null) `styleBundle` on a `TrackActionDescriptor` now **preserves** the prior track's bundle rather than wiping it. This makes "TrackAction-as-PartEffect-carrier" cards safe: Push It, Half Time, and Key Lift all use `TrackAction { role: <Rhythm|Backing>, styleBundle: null }` to sit in their respective per-musician pools while applying only a `PartEffect` (TempoEffect or ModulationEffect). The previous null-wipes-existing behavior caused Wormus Major's progression override to be dropped on the loop after a Key Lift play; the fix lands in `SongCompositionUI.TryAddOrReplaceTrackOnPart` existing-track branch. To explicitly clear a bundle, a dedicated sentinel would be needed (no such use case today).

**Blind-listener test for any C2 pair:** an untrained listener can distinguish the songs immediately — meter is structural, not ornamental.

### 5.6 Default Mode (Composition, C2, ×2) *(working name)*

Rhythm part. `MeterEffect(TimeSignature.FourFour)` in `modifierEffects`, plus `ApplyStatusEffect(flow, +1, Self)` co-effect on play. `inspirationGenerated = 3`.

- The default machine heartbeat — the meter listeners unconsciously expect from pop/rock. Robot-fiction reading: the factory clock, the boot sequence.
- Highest-copy C2 card (×2). Soft default — present in most songs but not guaranteed every song.
- Sole Flow source in the deck. Two copies → up to 2 Flow per song (matches original design ceiling).
- Composition-card co-effects on play are **verified working** (see §11 source material; ST-M13c-6 confirmed `CardPayload.Effects` execute end-to-end on composition cards). No fallback design needed.
- Trade-off framing: choosing Default Mode = comfortable groove + Flow gain. Choosing an odd-meter card = audibly distinct song but no Flow.
- **FixedPerformerType: C2.**

### 5.7 Waltz Protocol (Composition, C2, ×1) *(working name)*

Rhythm part. `MeterEffect(TimeSignature.ThreeFour)` in `modifierEffects`. `inspirationGenerated = 3`.

- The waltz mode. 3/4 is the meter the audience hears as "lilting" or "circular" — fundamentally different shape from 4/4.
- 1 copy — appears often enough to surface in playtest, rare enough to feel like a deliberate choice when played.
- Robot-fiction reading: a legacy protocol from C2's civilization's pre-machine-music era.
- **FixedPerformerType: C2.**

### 5.8 Pentameter (Composition, C2, ×1) *(working name)* — **moved to reward pool, v1.1**

Rhythm part. `MeterEffect(TimeSignature.FiveFour)` in `modifierEffects`. `inspirationGenerated = 3`.

- The math-jazz mode. 5/4 is the canonical odd meter (literally Brubeck's "Take Five" — note the deliberate echo of the action card name).
- 1 copy — deeply distinct sound profile, deserves to be a "signature pick."
- Robot-fiction reading: anomalous mode, the kind of meter only a math-jazz scholar like C2 would deploy.
- **FixedPerformerType: C2.**

### 5.9 Compound Cycle (Composition, C2, ×1) *(working name)* — **moved to reward pool, v1.1**

Rhythm part. `MeterEffect(TimeSignature.SixEight)` in `modifierEffects`. `inspirationGenerated = 3`.

- The compound mode. 6/8 sits in a different metric category from 4/4 and 3/4 — it's a *compound* meter (groups of three within a duple feel). Listener experience: rolling, triplet-pulsed, often associated with Coltrane-era jazz or sea-shanty/Celtic feels.
- 1 copy — third distinct C2 odd-meter pick.
- Robot-fiction reading: cyclic mode, recursive within itself.
- **FixedPerformerType: C2.**

### 5.10 Sibi backing card set — overview

Sibi's two backing cards plant a single axis: **axis 13 — Chord progression palette** (per `MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` §3, ✅ per-card via `BackingCardConfigSO.progressionPalette`).

The two cards sit at maximum contrast on this axis: minor-mode palette vs major-mode palette. Mode (minor vs major) is the most basic emotional-expectation lever in Western music.

**Carrier:** each card's `TrackActionDescriptor.styleBundle` is a `BackingCardConfigSO` with `progressionPalette` set to a `ChordProgressionPaletteSO` whose entries have appropriate `tonalities` restrictions. Per matrix §6.2, when the Backing composer picks a progression with a `tonalities` list that excludes the current part tonality, it mutates `part.Tonality` in place to a compatible tonality from the progression. Per §6.3, this flows through to melodic phrasing via `TonalityProfileSO` resolution. **One Sibi backing card decision propagates pervasively** — chords, melody contour, phrase shape — making the card decisively audible.

**TrackRole:** `TrackRole.Backing` for both cards. (Note: this corrects an earlier design intent in v0 that targeted `TrackRole.Harmony` — the matrix's §5.2 establishes that `BackingCardConfigSO` is the canonical chord-progression home; `HarmonyCardConfigSO` is "how to harmonize an existing line," not "what progression to play.")

**Blind-listener test:** the same melody and rhythm played over Wormus Minor vs Wormus Major sound *fundamentally different* — minor vs major is the most universally recognizable mode contrast in Western tonal music. Test passes trivially.

### 5.11 Wormus Minor (Composition, Sibi, ×1)

Backing part. `BackingCardConfigSO` with `progressionPalette = ChordProgressionPaletteSO_Minor` (specific asset TBD at authoring). `inspirationGenerated = 2`.

- Sibi's minor-mode anchor. Worm-shaped constellation parody of Ursa Minor — fits her hive-from-Singing-Fields fiction.
- Palette entries should be Aeolian-leaning, with optional Dorian / Phrygian variants for color. Each entry's `tonalities` field carries the appropriate restriction so the composer can override `part.Tonality` on selection.
- 1 copy — paired with Wormus Major as the deck's emotional-mode dichotomy.
- **FixedPerformerType: Sibi.**

### 5.12 Wormus Major (Composition, Sibi, ×1)

Backing part. `BackingCardConfigSO` with `progressionPalette = ChordProgressionPaletteSO_Major` (specific asset TBD at authoring). `inspirationGenerated = 2`.

- Sibi's major-mode anchor. Worm-shaped constellation parody of Ursa Major.
- Palette entries should be Ionian-leaning, with optional Mixolydian / Lydian variants for color. Same `tonalities` restriction approach as Wormus Minor.
- 1 copy — paired with Wormus Minor.
- **FixedPerformerType: Sibi.**

### 5.13 Singing Field (Composition, Sibi, ×1) *(working name)*

Melody part. `MelodyCardConfigSO` with `phrasePaletteOverride = PhrasePaletteSO_Hook` (specific asset TBD at authoring). `inspirationGenerated = 3`.

- Sibi's melodic hook. The "memorable shape" axis — what a hook *is*. (Axis 23 in the matrix: phrase palette → archetypes → contour, motif repetition, inter-phrase intervals.)
- Inherits whichever progression a Wormus card established for the part via the shared-progression mechanic (matrix §6.1, `GenContext.SetProgressionForPart`). Composes naturally: one Sibi backing card sets the harmony, this Sibi melody card surfs that harmony with a hook. The fiction "the band plays together" shows up in the mechanics.
- Achieves Melody role coverage in `LoopFeedbackContext` (resolves v0 §9 #5 — Hook Theme's classification ambiguity is closed by an unambiguous `TrackRole.Melody` choice).
- Sibi-fiction coherence: her Action card (Mind Tap) plants Earworm on an audience member; her Composition melody card plants a memorable phrase shape on the song. Both register the same fantasy — *make this stick* — at different scales.
- 1 copy — the rarity in Sibi's composition pool, deliberately.
- **FixedPerformerType: Sibi.**

> **Blocked until BASS-1 (2026-07-12) — now resolved.** This card's whole premise — "inherits whichever progression a Wormus card established for the part via the shared-progression mechanic" — was **not achievable** in the shipped build. Both Wormus and Singing Field are `FixedPerformerType: Sibi`, and a fixed-performer composition card ignores hover and always resolves onto its own musician. Under the pre-BASS-1 one-track-per-musician model, playing Singing Field after a Wormus card **retargeted Sibi's Backing track into a Melody track**, destroying the progression — and since C2 carries only Rhythm cards, the song lost its harmony entirely. BASS-1 re-keyed tracks by `(musicianId, role)`, so Sibi now holds Backing and Melody simultaneously and the mechanic works as designed. Verified ST-BASS-9 (2026-07-12). Authority: `runtime/SSoT_Runtime_CompositionSession_Integration.md` §11.

### 5.14 Push It (Composition, C2, ×1) *(new, v1.1)*

- **Asset:** `Starter_push_it.asset` + `Starter_push_it_Payload.asset`.
- **Performer rule:** `FixedMusicianType` → C2.
- **Card type:** TCH (math-jazz scholar; fits tempo-manipulation theme).
- **Costs:** inspirationCost = 0 (ECON-1 / D-ECON-6=DEFER; was 1), inspirationGenerated = 2.
- **Carrier:** `PrimaryKind = Track`, `TrackAction { role: Rhythm, styleBundle: null }`.
- **Effect:** `ModifierEffects: [TempoEffect_PushIt.asset]` — `mode: ScaleFactor`, `tempoScale: 1.5`, `timing: Immediate`.
- **Audible effect:** the song speeds up. `part.tempoScale *= 1.5` compositionally; "TEMPO!" floater fires on play.

### 5.15 Half Time (Composition, C2, ×1) *(new, v1.1)*

- **Asset:** `Starter_half_time.asset` + payload.
- **Performer rule:** `FixedMusicianType` → C2.
- **Card type:** TCH.
- **Costs:** inspirationCost = 0 (ECON-1 / D-ECON-6=DEFER; was 1), inspirationGenerated = 2.
- **Carrier:** `PrimaryKind = Track`, `TrackAction { role: Rhythm, styleBundle: null }`.
- **Effect:** `ModifierEffects: [TempoEffect_HalfTime.asset]` — `mode: ScaleFactor`, `tempoScale: 0.66`, `timing: Immediate`.
- **Audible effect:** the song slows down. Safety floor `Mathf.Max(40, baseBpm * tempoScale)` clamps extreme stacking.

### 5.16 Key Lift (Composition, Sibi, ×1) *(new, v1.1)*

- **Asset:** `Starter_key_lift.asset` + payload.
- **Performer rule:** `FixedMusicianType` → Sibi.
- **Card type:** EMT (emotional/expectation-shaping affinity; modulation is the canonical "lift" move).
- **Costs:** inspirationCost = 0 (ECON-1 / D-ECON-6=DEFER; was 1), inspirationGenerated = 2.
- **Carrier:** `PrimaryKind = Track`, `TrackAction { role: Backing, styleBundle: null }`.
- **Effect:** `ModifierEffects: [ModulationEffect_KeyLift_Degree5.asset]` — `mode: IntervalWithinScale`, `targetDegree: 5`, `timing: Immediate`.
- **Audible effect:** the song's root pitch class shifts by a fifth. All stems for the part regenerate in the new key (per D-MOD-2=A); the styleBundle (e.g., Wormus Major's progression) is preserved through the play per D-MOD-FIX=A. "KEY!" floater fires on play when the previous render had an explicit root.
- **Known limitation.** Octave of the new root is voice-leader-driven; the modulation may audibly ascend or descend depending on minimum voice-leading distance. Tracked as MGP-ALWTTT-MOD-DIR-1.

### 5.17 Psychic Waves (Action, Sibi, ×1) *(new, v1.3)*

- Owner Sibi (`FixedMusicianType`, D-TUT-R1-2). `ModifyVibe(+5, AllAudienceCharacters)`, cost 3 (D-TUT-R1-1=A).
- Magnitude **5** (revised 4→5, D-TUT-R2c): 5×3 = 15 total Vibe; affordability gated by cost, not magnitude.
- AoE ⇒ no target select ⇒ ideal for the beat-8 gate.
- **Presentation (JUICE-PW, 2026-07-13).** The card is the demo's mechanical climax and now reads as
  one: at *effect resolution* it fires a per-member `-N` floater (grey `INDIFFERENT` on a blocked
  member), one `CardVibeImpact` sting, an impact kick + particle burst on each landed member, and a
  kick + burst on Sibi. The FX is structurally guaranteed to precede the beat-8 loop-hold release.
- **`audioType: None` — authored deliberately, do not "fix".** The card's sound is its **impact**
  (the bus sting), not its **drop**. Giving it a clip-backed `audioType` would make it sound twice.
  Rule: `SSoT_Card_Authoring_Contracts.md` §5.15; key: `SSoT_Audio.md` §3 + invariant 18.
- **Cost/magnitude are untouched by JUICE-PW** — finisher economy tuning stays S5i.

### 5.18 Keep Cool (Action, C2, ×1) *(new, v1.3)*

- Owner **C2** (`FixedMusicianType`), target Self. `ApplyStatusEffect(composure, +3, Self)`, cost 0 (ECON-1 starter layer).
- Composure clears at PlayerTurn start (`SSoT_Gig_Combat_Core §6.2`) → +3 reads against one turn-cycle; parity with Take Five (−3 Stress).
- **Provisional:** C2-owned (not generic); C2 always in the demo band (roster = C2 + Sibi); generic-catalog move deferred (D-ECON-GENERIC).

### 5.19 Sibi Bassline cards (BASS-CARD-1, 2026-07-12) — **starter status UNDECIDED**

Two Bassline cards were authored for Sibi to validate BASS-1 end-to-end:

| Card | Bundle | Purpose |
|---|---|---|
| **Worm Walk** | `BasslineCardConfigSO`, `chordExpression = Block` (default) | Baseline — sustained bass, bit-identical to legacy bass output |
| **Worm Pulse** | `BasslineCardConfigSO`, `chordExpression = ArpeggioUp`, `arpeggioRate = Eighth` | Articulated — repeated-note eighth pulse on the monophonic line |

Both were flagged `UnlockedByDefault | StarterDeck` **as a testing convenience**, which puts the starter at 19 cards and contradicts §4. **This is not a design decision.** Open item — resolve before demo:

- **(A)** drop the `StarterDeck` flag (cards remain authored, inert);
- **(B)** move to `RewardPool` — bass becomes a run unlock, giving Sibi a third expressive axis to grow into;
- **(C)** promote bass to real starter content and re-derive §4's counts and the C2 : Sibi ratio.

Recommendation: **(B)**. It costs nothing now, keeps §4 intact, and the "add a whole new voice" fantasy is a strong reward. (A) is the zero-thought fallback. (C) needs a tuning pass the demo cut does not have room for.

---

## 6. Expected deck behavior

### 6.1 Opening hand (5 cards)

- P(no Composition) = `C(4,5) / C(12,5)` = 0 (mathematically impossible — only 4 Action cards).
- P(no Action) = `C(8,5) / C(12,5)` = 56 / 792 ≈ 7.1%.
- The bidirectional guaranteed-draw mitigation (M4.5) eliminates the "no Action in hand at action window" case.

### 6.2 Mid-gig dynamics

- Average hand composition per 5-card draw: ~1.67 Action, ~3.33 Composition.
- Action window: the player has a narrow selection (1–2 Action cards typically). This is by design — the composition phase is where player choice lives in ALWTTT.
- Composition phase: the player has a broader selection (3–4 Composition cards typically). Rhythm is near-always available (5 C2 copies across four meter cards). Backing is usually available (2 Sibi backing copies across Wormus Minor/Major). Melody depends on drawing Singing Field (1 copy).
- Per-song variety ceiling: each song picks at most one C2 meter (via the played card) and at most one Sibi mode (via the played Wormus). Across a 3-song gig, the player is likely to hear multiple meters and at least one mode swap — the "every song sounds different" target is achievable with the v1 starter.

### 6.3 Flow accumulation

- Flow sources in the deck: Default Mode (co-effect on play, +1 each), 2 copies. Maximum 2 Flow per song from starter. Same ceiling as the v0 design (which used Four on the Floor for this).
- At 2 Flow stacks, M4.2 multipliers: composition card Vibe modifier is `× (1 + 2 × 0.08) = ×1.16`. Song-end conversion is `× 1.16` on top of base Vibe. Effect is ~16% momentum amplification at Flow cap.
- This is deliberately modest for the starter. Deeper Flow plays belong to future cards unlocked via progression.

### 6.4 Earworm plants

- Earworm source: Mind Tap (+2 stacks per play), 1 copy per song before reshuffle.
- Target: single audience member. Over the course of a song, the player can Earworm at most one audience per Mind Tap play, then reshuffle to get it back.
- Expected total Vibe contribution per Earworm plant: `2 + 1 = 3` (stacks tick down linearly each audience turn) plus the instant `+5` from Mind Tap's direct Vibe effect.

### 6.5 Cross-card emergent behavior

- **Wormus + Singing Field synergy.** When the player plays a Wormus card and Singing Field in the same song, the melody automatically follows the established progression (matrix §6.1). The two-card combo "feels integrated" without requiring explicit linking — emergent from the shared-progression mechanic.
- **Wormus → tonality override.** A Wormus card whose chosen progression has a restricted `tonalities` list will *override* whatever tonality enum is currently set on the part (matrix §6.2). This means a Wormus card is the *dominant* tonal axis in the current starter — no other card in the deck sets tonality, so this is uncontested.
- **C2 meter independence.** The C2 meter axis is independent of the Sibi mode axis. Any combination is valid — Wormus Minor in 5/4 produces a different song from Wormus Major in 4/4, etc. Cross-product variety: 4 meters × 2 modes = 8 distinct "song-shape" combinations available from a 7-composition-card deck, before considering Singing Field's contribution.

---

## 7. Test encounter pairing

The starter deck is designed against two audience archetypes as encounter placeholders (final tuning after runtime values received):

**The Heckler** (primary pressure archetype)
- Low VibeGoal (15–20 after retune).
- Aggressive `AddStressAction` pressure. Post-M4.1, absorbed properly by Composure.
- Easy to convince, punishes a band that doesn't defend.

**The Critic** (spread pressure archetype)
- High VibeGoal (35–45 after retune).
- Lower but spread `AddStressAction`.
- Hard to convince; rewards sustained Vibe output.

Test encounter: 3 Hecklers + 1 Critic, 3 songs, Cohesion 3. Final tuning depends on runtime values and post-M4.1 behavior.

---

## 8. Tuning knobs exposed

To be set in Inspector during M4.6 tuning pass:
- `flowVibeMultiplier` (from M4.2) — initial 0.08.
- `flowBonusPerStackPerCard` (existing) — initial value kept; verified against runtime telemetry.
- Per-card `inspirationCost` and `inspirationGenerated` — initial values in §4, revisable. Note: all four C2 rhythm cards currently set to gen 3 (uniform); if Default Mode dominates pick rate in playtest, drop its gen to 2 to rebalance against odd meters.
- Per-card cost is now the finisher knob; all starter costs are currently 0 under D-ECON-6=DEFER (finisher designation deferred, costs tuned in S5i). Per-period play frequency is governed by `SSoT_Gig_Combat_Core.md` §14 and is NOT a per-card knob.
- Encounter VibeGoals for Heckler/Critic — initial ranges in §7, final values after runtime values received.
- Wormus Minor/Major palette contents (which `ChordProgressionData` entries are included in each `ChordProgressionPaletteSO`) — see §9 #7.
- Singing Field phrase palette content (which `PhraseArchetypeSO` entries) — see §9 #6.
- `captivatedBonusPerStack` — not in starter, deferred to roster expansion.
- **Finisher layer partially closed (v1.3):** **Psychic Waves** = one cost-≥1 card authored; deep finisher tuning stays S5i. Defensive heal/block pair on record: **Take Five (−3 Stress) / Keep Cool (+3 Composure)**.

---

## 9. Open questions (gate M4.6 authoring)

These must be resolved before the starter is authored. Ordered by priority. Closed items retained with strikethrough for traceability.

1. ~~**`CompositionCardPayload.effects` support**~~ — **CLOSED 2026-04-23.** Verified working end-to-end via ST-M13c-6 (M1.3c closure): `CardPayload.Effects` execute on composition cards, status tooltip appears on hover, effect authored via Card Editor. Default Mode's Flow co-effect is viable; no fallback design needed. See `CURRENT_STATE.md` §4 "Open items" entry for `CompositionCardPayload.effects`.

2. **Composition pipeline vocabulary** — needed for v1 authoring:
   - `MeterEffect` PartEffect first-class with `TimeSignature.FourFour / ThreeFour / FiveFour / SixEight` (for the four C2 cards). Confirm rhythm runtime composes audibly distinct output for each.
   - `BackingCardConfigSO.progressionPalette` first-class, with `ChordProgressionPaletteSO` entries supporting `tonalities` restrictions on `ChordProgressionData` (for the two Wormus cards).
   - `MelodyCardConfigSO.phrasePaletteOverride` first-class, with `PhrasePaletteSO` → `PhraseArchetypeSO` content available (for Singing Field).
   - May not be fully answered until M2 closes.

3. ~~LoopScoreCalculator retune for 2-musician baseline~~ **Closed (M4.2, 2026-04-28).** Adaptive scoring implemented: role-budget normalization with `possibleRoleCount` auto-detected from deck. Inspector-tuneable `HypeThresholds`. No manual retune needed per roster.

4. **Runtime values from user** — `maxVibeFromSongHype`, `MaxCardsOnHand`, draw-per-turn (`targetDrawCount` at PlayerTurn start), `flowBonusPerStackPerCard` current value, any other GigManager/PersistentGameplayData defaults relevant to starter tuning.

5. ~~**Hook Theme → Melody vs Harmony classification**~~ — **CLOSED 2026-04-26.** Hook Theme retired; replaced by Singing Field which targets `TrackRole.Melody` unambiguously per the resolved axis decision (axis 23, phrase palette).

6. **Sibi target constraint on Mind Tap** — does the runtime enforce that Mind Tap's `AudienceCharacter` target must be selected interactively, or does it auto-resolve to a specific audience member? Mind Tap is high-value; auto-resolution matters for the play feel. Likely Option A (player selects), consistent with other `ModifyVibe(AudienceCharacter)` cards.

7. **Singing Field phrase palette content** *(new 2026-04-26)* — which specific `PhrasePaletteSO` asset (and its constituent `PhraseArchetypeSO` entries) should Singing Field use? Depends on what's already authored in the project's library, and on what "hook-shaped" means in archetype terms (high motif repetition? large inter-phrase intervals? specific contour constraints?). Defer to authoring time; revisit when M2 inventory is known.

8. **Wormus Minor/Major progression palette content** *(new 2026-04-26)* — which specific `ChordProgressionData` entries comprise each palette? Depends on what's already in the chord-progression library and on whether new minor- and major-mode progressions need authoring. Each entry's `tonalities` field must carry the restriction needed for the §6.2 tonality-override behavior. Defer to authoring time.

9. **Card naming pass** *(new 2026-04-26)* — three C2 rhythm card names and the Sibi melody card name are *(working name)* placeholders: Default Mode, Waltz Protocol, Pentameter, Compound Cycle, Singing Field. Final names should be locked at authoring time, ideally consistent with C2's robot/machine theming and Sibi's hive/asteroid theming. Wormus Minor and Wormus Major are locked.

---

## 10. Smoke tests (ST-SD-*) for M4.6 closure

1. **ST-SD-1 — Deck loads with correct multiplicities.** `StarterDeck_v1.asset` imported via Deck Editor, registered in `GigSetupConfigData.availableBandDecks`, gig started. `PersistentGameplayData.CurrentActionCards.Count == 4`, `CurrentCompositionCards.Count == 8`, no null references, no warnings about duplicate dedup.

2. **ST-SD-2 — Reshuffle preserves counts.** Play through a full song, verify all 12 cards cycle through `DrawPile` → `HandPile` → `DiscardPile` → `DrawPile` correctly. At gig start, draw + discard piles sum to 12 minus cards in hand.

3. **ST-SD-3 — Mind Tap applies Earworm.** Sibi plays Mind Tap on an audience member. Audience gains +5 Vibe visibly. Earworm icon appears on audience canvas with stack count 2. Next `AudienceTurnStart`: audience gains +2 Vibe, Earworm stack count drops to 1. Next audience turn: +1 Vibe, stack count drops to 0, icon disappears.

4. **ST-SD-4 — Default Mode applies Flow on play.** C2 plays Default Mode during composition phase. Flow icon appears on C2 (or on band-level display, depending on presentation choice) with stack count 1. Playing a second copy in the same song: stack count 2. Flow persists through the song, resets at song end. Cross-reference: ST-M13c-6 (2026-04-23) already verified the underlying mechanism (`CardPayload.Effects` on composition cards execute end-to-end); ST-SD-4 confirms the design-level integration.

5. **ST-SD-5 — C2 meter cards produce audibly distinct songs.** Play a 4-song gig, force-playing a different C2 rhythm card in each song (Default Mode, Waltz Protocol, Pentameter, Compound Cycle). Listener test: an untrained observer can distinguish all four songs by metric feel. Verifies axis 7 (`MeterEffect` → `PartConfig.TimeSignature`) reaches audible output via the rhythm composer. **Fail criterion:** any two songs sound metrically interchangeable.

6. **ST-SD-6 — Sibi backing cards produce audibly distinct mode.** Play a 2-song gig, force-playing Wormus Minor in song 1 and Wormus Major in song 2. Listener test: songs sound minor vs major to an untrained observer. Verifies axis 13 (`BackingCardConfigSO.progressionPalette` → `ChordProgressionData.tonalities`) reaches audible output and overrides part tonality per matrix §6.2. **Fail criterion:** songs sound modally interchangeable; or `part.Tonality` does not change at composer-time when palette tonalities restrict it.

7. **ST-SD-7 — Singing Field inherits progression.** Play Wormus Minor and Singing Field in the same song. Listener test: melody from Singing Field follows the minor-mode progression set by Wormus Minor (no melodic notes outside the chord/scale palette established by Wormus Minor). Verifies matrix §6.1 (`GenContext.SetProgressionForPart` → `ctx.GetProgressionForPart`). **Fail criterion:** melody plays a major-mode contour over a minor progression, or melody appears uncoordinated with backing.

8. **ST-SD-8 — Full gig end-to-end.** Start gig with `StarterDeck_v1`. Play a full 3-song gig against 3 Hecklers + 1 Critic placeholder. At least one audience member reaches Convinced (Vibe ≥ VibeGoal). Gig resolves either in Victory (all convinced) or in Loss (songs exhausted or Cohesion ≤ 0). No silent failures. Final Vibe totals, Stress totals, and Cohesion visible.

---

## 11. Source material

- Session report: `Session_Report_Starter_Deck_Design.md` (2026-04-21).
- Axis-resolution session: 2026-04-26 (no separate report; outcome captured in this document, §3.1, §4, §5.5–§5.13, §9 #5/#7/#8/#9).
- Combat MVP audit (source of C1 and other findings): `planning/archive/ALWTTT_Combat_MVP_Audit_Final.md` (2026-03-20).
- Expressive surface evidence base: `integrations/midigenplay/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` (2026-04-24; per-musician SO whitelist precedence added 2026-05-20) — §3 (axis matrix), §5 (per-role bundle contracts), §6 (cross-track emergents).
- Composition-card-effect verification: M1.3c closure (2026-04-23), ST-M13c-6 — confirmed `CardPayload.Effects` execute on composition cards. Closes §9 #1.
- Runtime contract references (at time of design):
  - `SSoT_Gig_Combat_Core.md` §6.1 (Flow), §5.4 (Stress), §7 (phase flow)
  - `SSoT_Scoring_and_Meters.md` §§4–7 (meter semantics)
  - `SSoT_Status_Effects.md` §5 (canonical MVP status set)
  - `SSoT_Audience_and_Reactions.md` §§3–5, §10 (Vibe, Convinced, MVP rules)
  - `SSoT_Card_System.md` §6.1 (`CardEffectSpec` data-only), §9 (card roles in combat)
- Referents consulted:
  - Slay the Spire 1 & 2 starter decks (3–4 unique / 10–12 total, heavy duplication).
  - Monster Train 2 starter (~5 unique / ~15 total, primary + allied clan Starter cards at ×5 each).

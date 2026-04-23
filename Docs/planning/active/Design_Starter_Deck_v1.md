# Design_Starter_Deck_v1 — ALWTTT

**Status:** Active design proposal — planning only, subject to playtest revision  
**Scope:** Composition, per-card effect spec, per-musician identity mapping, tuning rationale, and open questions for the first starter deck the player uses at the start of a run  
**Classification:** `reference (planning)` — **not a SSoT**. This document expresses design intent. When M4.6 authoring closes, the authored `.asset` files become the runtime-authoritative version; this document is then retained as historical rationale.  
**Last updated:** 2026-04-21

---

## 1. Purpose

This document is the frozen design record of the first starter deck for ALWTTT, produced by the design session on 2026-04-21.

It exists so that, when M4.6 authoring begins (after M4.1–M4.5 land), the author does not have to reconstruct the design from the session report or from memory. It also surfaces explicit open questions that must be resolved before authoring — these are listed in §9.

It is a planning document. Numbers, card names, and effect magnitudes are first-guess design choices and expected to shift at playtest.

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
- Uses only `CardEffectSpec` types already in the active vocabulary: `ApplyStatusEffect`, `ModifyVibe`, `ModifyStress`, `DrawCards`. No new effect types introduced.
- Audience-side status usage limited to **Earworm** only (added in M4.3). Captivated and `ApplyIncomingVibe` are explicitly deferred.
- Flow mechanics align with the bifurcated model introduced in M4.2 (flat on Action, multiplier on Composition + Song End).
- Bidirectional guaranteed-draw mitigation (M4.5) is assumed active; without it, the 8:4 ratio misfires at ~7% of action windows.

---

## 3. Roster

### 3.1 Included in starter

**Robot C2** — Drummer / drum machine.
- Fantasía: metronomic, inhuman consistency, the engine of the band. BPM made incarnate.
- Role in composition pipeline: Rhythm (percussion).
- Stat bias: TCH dominant.
- Mechanical identity in starter: **source of Flow** for the band. Drives momentum that amplifies both action-card Vibe bonuses (flat) and composition-card + song-end Vibe conversion (multiplier).

**Sibi** — Keyboardist, worm-like entity with psychic affinity.
- Fantasía: the color and the hook. Mind/pattern manipulation at a distance. Earworm is literally what she does — her songs get stuck in the audience's head.
- Role in composition pipeline: Harmony or Melody-adjacent (keyboard, synth textures).
- Stat bias: CHR + EMT mix.
- Mechanical identity in starter: **Earworm applicator** on single-target audience members. The only audience-state manipulator in MVP.

### 3.2 Deferred to roster expansion

**Conito** — Bassist, flight + electricity. Bass pipeline validation not yet on critical path.
**Ziggy** — Vocalist, multiharmony. Carries Captivated as identity status (post-MVP).

Both are documented in `Roadmap_ALWTTT.md` → Future Milestones → Roster Expansion.

---

## 4. Deck composition — the 12 cards

**Total:** 12 cards, 7 unique, ratio 8 Composition : 4 Action.

| # | Card | Unique? | Copies | Domain | Owner | Effect (authoritative for MVP) | Inspiration cost / gen | Notes |
|---|---|---|---|---|---|---|---|---|
| 1 | Warm Up | ✓ | 2 | Action | Any | `DrawCards(2)` | 1 / — | Didactic generic — the "Strike" analog |
| 2 | Take Five | ✓ | 1 | Action | Any | `ModifyStress(-3, Self)` | 0 / — | Defensive generic |
| 3 | Mind Tap | ✓ | 1 | Action | Sibi | `ModifyVibe(+5, AudienceCharacter)` + `ApplyStatusEffect(earworm, +2, AudienceCharacter)` | 1 / — | Sibi identity Action — plants Earworm |
| 4 | Steady Beat | ✓ | 3 | Composition | C2 | Rhythm part. PrimaryKind/TrackAction/PartAction TBD at authoring. | — / 3 | C2 identity Composition — the engine, repeats |
| 5 | Four on the Floor | ✓ | 2 | Composition | C2 | Rhythm variant + `ApplyStatusEffect(flow, +1, Self)` as co-effect on play | — / 2 | C2 identity Composition — Flow source |
| 6 | Synth Pad | ✓ | 2 | Composition | Sibi | Harmony part. PrimaryKind/TrackAction/PartAction TBD at authoring. | — / 2 | Sibi identity Composition — harmonic layer |
| 7 | Hook Theme | ✓ | 1 | Composition | Sibi | Melody-adjacent part. PrimaryKind/TrackAction/PartAction TBD at authoring. | — / 3 | Sibi identity Composition — hook |

**Derived counts:**
- 4 Action (total copies): 2 Warm Up + 1 Take Five + 1 Mind Tap.
- 8 Composition (total copies): 3 Steady Beat + 2 Four on the Floor + 2 Synth Pad + 1 Hook Theme.
- C2 composition coverage: 5 copies (3 + 2).
- Sibi composition coverage: 3 copies (2 + 1).

**Ratio asymmetry is intentional:** C2 is the engine (more copies, repeats), Sibi is the color (fewer copies, more variety per card).

---

## 5. Per-card design rationale

### 5.1 Warm Up (Action, generic, ×2)

`DrawCards(2)` at cost 1.

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

`ModifyVibe(+5, AudienceCharacter)` + `ApplyStatusEffect(earworm, +2, AudienceCharacter)` at cost 1.

- Sibi's only Action. Her main way of impacting the audience directly during action windows.
- The +5 instant Vibe and 2 Earworm stacks are both audible, visible, immediate wins — a "signature spell" feel.
- The two effects compose cleanly: the instant Vibe gets amplified by Flow (flat path, from M4.2). The Earworm stacks generate +2 Vibe on the next audience turn, then +1, then 0 (assuming no refresh). Total expected Vibe contribution over the hit and 2 audience turns: 5 + 2 + 1 = **8 Vibe** (ignoring Flow amplification, ignoring Captivated which doesn't exist yet).
- 1 copy is intentional — Sibi being "rare" in the Action phase maps to her fantasía of mental precision.
- **FixedPerformerType: Sibi.** Only Sibi can play this card.

### 5.5 Steady Beat (Composition, C2, ×3)

Rhythm part, `inspirationGenerated = 3`.

- The engine. Highest-copy card in the deck.
- 3 copies guarantees almost every song has at least one Steady Beat available during composition phase — the Rhythm role is load-bearing for LoopScore.
- Exact `PrimaryKind` / `TrackAction` / `PartAction` / `styleBundle` values are TBD at authoring. Constraint: must be compatible with C2's percussionist role and the composition pipeline as validated in M2.
- **FixedPerformerType: C2.** Only C2 can play this card.

### 5.6 Four on the Floor (Composition, C2, ×2)

Rhythm variant, `inspirationGenerated = 2`, with co-effect `ApplyStatusEffect(flow, +1, Self)` on play.

- The Flow generator. Every copy played accrues a Flow stack for the band.
- Lower `inspirationGenerated` than Steady Beat because the Flow is the payoff — it's the "invest in momentum" choice vs Steady Beat's "invest in loop quality" choice.
- Requires `CompositionCardPayload.effects` to support co-effects alongside the musical payload. **This is a gating open question — see §9.**
- 2 copies so the player can plausibly stack Flow across a song.
- **FixedPerformerType: C2.**

### 5.7 Synth Pad (Composition, Sibi, ×2)

Harmony part, `inspirationGenerated = 2`.

- Harmony support. Gives the band Harmony coverage in the loop.
- Important: with only 2 musicians, Bass is always absent from the loop (no Conito), and Harmony is only present when a Sibi composition card is played. Synth Pad is the most reliable Harmony source in the starter.
- 2 copies to reliably cover Harmony across multi-song gigs.
- **FixedPerformerType: Sibi.**

### 5.8 Hook Theme (Composition, Sibi, ×1)

Melody-adjacent part, `inspirationGenerated = 3`.

- Melody coverage. The "hook" that the audience remembers.
- 1 copy — it's the rarity in Sibi's composition pool, deliberately.
- Whether this counts as Melody or Harmony in `LoopScoreCalculator`'s role-coverage bitmap depends on the `PrimaryKind` chosen at authoring. If it registers as Melody, the starter has periodic Melody coverage via draws of Hook Theme; if it registers as Harmony, the starter has near-constant Harmony coverage and no Melody coverage at all (since no other card is Melody).
- **FixedPerformerType: Sibi.**

---

## 6. Expected deck behavior

### 6.1 Opening hand (5 cards)

- P(no Composition) = `C(4,5) / C(12,5)` = 0 (mathematically impossible — only 4 Action cards).
- P(no Action) = `C(8,5) / C(12,5)` = 56 / 792 ≈ 7.1%.
- The bidirectional guaranteed-draw mitigation (M4.5) eliminates the "no Action in hand at action window" case.

### 6.2 Mid-gig dynamics

- Average hand composition per 5-card draw: ~1.67 Action, ~3.33 Composition.
- Action window: the player has a narrow selection (1–2 Action cards typically). This is by design — the composition phase is where player choice lives in ALWTTT.
- Composition phase: the player has a broader selection (3–4 Composition cards typically). Rhythm is near-always available (5 copies between Steady Beat and Four on the Floor). Harmony is usually available (2 Synth Pad). Melody depends on drawing Hook Theme (1 copy).

### 6.3 Flow accumulation

- Flow sources in the deck: Four on the Floor (co-effect on play, +1 each), 2 copies. Maximum 2 Flow per song from starter.
- At 2 Flow stacks, M4.2 multipliers: composition card Vibe modifier is `× (1 + 2 × 0.08) = ×1.16`. Song-end conversion is `× 1.16` on top of base Vibe. Effect is ~16% momentum amplification at Flow cap.
- This is deliberately modest for the starter. Deeper Flow plays belong to future cards unlocked via progression.

### 6.4 Earworm plants

- Earworm source: Mind Tap (+2 stacks per play), 1 copy per song before reshuffle.
- Target: single audience member. Over the course of a song, the player can Earworm at most one audience per Mind Tap play, then reshuffle to get it back.
- Expected total Vibe contribution per Earworm plant: `2 + 1 = 3` (stacks tick down linearly each audience turn) plus the instant `+5` from Mind Tap's direct Vibe effect.

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
- Per-card `inspirationCost` and `inspirationGenerated` — initial values in §4, revisable.
- Encounter VibeGoals for Heckler/Critic — initial ranges in §7, final values after runtime values received.
- `captivatedBonusPerStack` — not in starter, deferred to roster expansion.

---

## 9. Open questions (gate M4.6 authoring)

These must be resolved before the starter is authored. Ordered by priority.

1. **`CompositionCardPayload.effects` support** — does the runtime pipeline execute `CardEffectSpec` list on a composition card when the card is played (in addition to the musical payload)? The SSoT says yes (composition cards "may also apply immediate systemic effects when authored to do so" — `SSoT_Gig_Combat_Core.md` §7 / §9). Needs code verification. If unsupported, Four on the Floor's Flow-on-play co-effect relocates to an Action card (e.g., split Four on the Floor into two cards, or fold Flow into a third Action identity for C2).

2. **Composition pipeline vocabulary** — what specific `PrimaryKind`, `TrackAction`, `PartAction`, `styleBundle` values are available and validated for each of: Rhythm, Harmony, Melody? This is M2 scope and may not be fully answered until M2 closes.

3. **`LoopScoreCalculator` post-retune values** — M4.2 retunes thresholds for 2-musician baseline. The retuned values shape the expected per-loop SongHype delta, which shapes the song-end Vibe baseline, which shapes encounter VibeGoal tuning. Authoring pass depends on retuned values being in place.

4. **Runtime values from user** — `maxVibeFromSongHype`, `MaxCardsOnHand`, draw-per-turn (`targetDrawCount` at PlayerTurn start), `flowBonusPerStackPerCard` current value, any other GigManager/PersistentGameplayData defaults relevant to starter tuning.

5. **Hook Theme → Melody vs Harmony classification** — affects whether the starter achieves any Melody coverage at all. Decided at authoring by the `PrimaryKind` choice.

6. **Sibi target constraint on Mind Tap** — does the runtime enforce that Mind Tap's `AudienceCharacter` target must be selected interactively, or does it auto-resolve to a specific audience member? Mind Tap is high-value; auto-resolution matters for the play feel. Likely Option A (player selects), consistent with other `ModifyVibe(AudienceCharacter)` cards.

---

## 10. Smoke tests (ST-SD-*) for M4.6 closure

1. **ST-SD-1 — Deck loads with correct multiplicities.** `StarterDeck_v1.asset` imported via Deck Editor, registered in `GigSetupConfigData.availableBandDecks`, gig started. `PersistentGameplayData.CurrentActionCards.Count == 4`, `CurrentCompositionCards.Count == 8`, no null references, no warnings about duplicate dedup.

2. **ST-SD-2 — Reshuffle preserves counts.** Play through a full song, verify all 12 cards cycle through `DrawPile` → `HandPile` → `DiscardPile` → `DrawPile` correctly. At gig start, draw + discard piles sum to 12 minus cards in hand.

3. **ST-SD-3 — Mind Tap applies Earworm.** Sibi plays Mind Tap on an audience member. Audience gains +5 Vibe visibly. Earworm icon appears on audience canvas with stack count 2. Next `AudienceTurnStart`: audience gains +2 Vibe, Earworm stack count drops to 1. Next audience turn: +1 Vibe, stack count drops to 0, icon disappears.

4. **ST-SD-4 — Four on the Floor applies Flow on play.** C2 plays Four on the Floor during composition phase. Flow icon appears on C2 (or on band-level display, depending on presentation choice) with stack count 1. Playing a second copy in the same song: stack count 2. Flow persists through the song, resets at song end.

5. **ST-SD-5 — Composition cards repeat across songs without runtime warnings.** Play a 3-song gig. Steady Beat is played at least twice across different songs (starter has 3 copies). No `Debug.LogWarning` about duplicate references. Song 2 and Song 3 sound different despite identical composition card plays (confirms musical randomization works at the MIDI level).

6. **ST-SD-6 — Full gig end-to-end.** Start gig with `StarterDeck_v1`. Play a full 3-song gig against 3 Hecklers + 1 Critic placeholder. At least one audience member reaches Convinced (Vibe ≥ VibeGoal). Gig resolves either in Victory (all convinced) or in Loss (songs exhausted or Cohesion ≤ 0). No silent failures. Final Vibe totals, Stress totals, and Cohesion visible.

---

## 11. Source material

- Session report: `Session_Report_Starter_Deck_Design.md` (2026-04-21).
- Combat MVP audit (source of C1 and other findings): `planning/archive/ALWTTT_Combat_MVP_Audit_Final.md` (2026-03-20).
- Runtime contract references (at time of design):
  - `SSoT_Gig_Combat_Core.md` §6.1 (Flow), §5.4 (Stress), §7 (phase flow)
  - `SSoT_Scoring_and_Meters.md` §§4–7 (meter semantics)
  - `SSoT_Status_Effects.md` §5 (canonical MVP status set)
  - `SSoT_Audience_and_Reactions.md` §§3–5, §10 (Vibe, Convinced, MVP rules)
  - `SSoT_Card_System.md` §6.1 (`CardEffectSpec` data-only), §9 (card roles in combat)
- Referents consulted:
  - Slay the Spire 1 & 2 starter decks (3–4 unique / 10–12 total, heavy duplication).
  - Monster Train 2 starter (~5 unique / ~15 total, primary + allied clan Starter cards at ×5 each).

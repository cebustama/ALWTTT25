# Design_Vertical_Slice_v0_1 — ALWTTT

**Status:** Active design — opens for S6 (run structure), S7 (run content I), S8 (run content II / boss + closing) per the 2026-05-23 planning reframe.
**Scope:** Phase C of the ALWTTT roadmap — publisher-showable multi-encounter run.
**Classification:** `reference (planning)` — **not a SSoT**. Becomes runtime-authoritative when shipped; this doc is then retained as historical rationale.
**Created:** 2026-05-23

---

## 1. Purpose

The 2026-05-23 planning reframe extended the original 4-session demo-cut polish plan to 8 sessions across two milestones: demo cut (S1-S5) + vertical slice (S6-S8). The vertical slice is Phase C — a publisher-showable multi-encounter run with venue / encounter variety, a ship hub stub, a boss encounter, and full tutorial coverage.

This doc captures the six decisions locked at reframe (D-RUN-1..6) and the per-session scope so that S6, S7, and S8 execute against a shared baseline rather than rederiving the venue / encounter / boss design each session.

---

## 2. Scope (D-RUN-1 = A, narrow demo cut now; vertical slice as Phase C)

### In scope (S6-S8)

- Multi-encounter run structure: 3 encounters + boss (D-RUN-3 ideal), 2 encounters + boss (D-RUN-3 minimum).
- Minimal ship hub stub (D-RUN-2 = β: dedicated S6, not folded into S5 — keeps demo cut narrow).
- 2 new venue scenes (asset images 7 + 10) + existing venue retained as third.
- 4 new audience archetypes (sprites available from the 2026-05-23 asset drop) joining Cool Dude + Kid in the encounter pool.
- Audience state machine (idle / hostile / vibing).
- Boss encounter (D-RUN-4 = α: subclasses `AudienceCharacterBase`, bespoke abilities only, no new character infrastructure).
- Pilot / manager character (asset image 1) as ship-hub interlocutor (D-RUN-5).
- Tutorial coverage spanning S6-S8 (per Design_Tutorial_System §7 with D-TUT-5).
- Scene transitions: gig → reward → ship → next gig; boss → closing sequence.

### Out of scope (D-RUN-6 = deferred to post-vertical-slice)

- Ship interior batches (bar / chill / rehearsal rooms).
- Space map.
- Meta-progression (run-to-run persistence, deck evolution across runs).
- Full audio pass.
- Ladder mode formalization beyond what `GigLauncher` (§5.3.5) already enables. The vertical-slice run uses a simple encounter queue; full ladder-mode work is post-vertical-slice.

---

## 3. Sessions S6 / S7 / S8

### 3.1 S6 — Run structure (ship hub stub + scene transitions)

**Goal.** Gig → reward → ship → next gig flow plays end-to-end. Player can complete more than one encounter in sequence.

**Deliverables.**
- Minimal ship hub scene (single screen, pilot portrait visible).
- Encounter dispatcher (reuses or extends `GigLauncher`; if extension is needed it lands as a Phase C-internal batch, not a separate architectural slot).
- Reward flow stub (existing `RewardCanvas` reused if functional by S6 open; otherwise stubbed to a single "continue" panel).
- **Reward skip option (BR-D1 = B, locked 2026-07-16, BALANCE-XREF).** The end-of-gig 3-card offer gains an explicit *skip* alongside the choices — the Slay-the-Spire model, in which skipping is a first-class deck-consistency lever, not a failure state. Deferred from the demo cut deliberately (skip is inert in a single-gig demo); it lands here because S6 is the first multi-gig context and already owns full reward-selection. This matters more in ALWTTT than in StS: with guaranteed draws (M4.5) and a 17-card starter, every forced pick measurably dilutes draw slots. Wire the skip into `RewardCanvas` (a "Skip" affordance beside `ChoicePanel`), route it to `FinishReward` without granting, and cover it with `tut_first_reward_choice`. See maxim **N9** (`planning/Design_Game_And_Card_Maxims_v0_1.md`).
- Run-state persistence across encounter boundaries (band roster, deck, run progress).

**Tutorial.** `tut_ship_hub_intro`, `tut_first_reward_choice` (per Design_Tutorial_System §7).

**Sensory Contract.** Ship-hub UI changes produce sensory artifacts per Standing Directive #2. New consumer code (if any) subscribes to the bus established in S2.

**Open at S6 batch open.**
- Encounter selection mechanic: player-choice vs fixed sequence.
- Reward content scope: existing reward types (if any) vs stub with `tut_first_reward_choice` covering whatever lands.
- **Run power curve — what makes the band feel stronger at the boss than at gig 1 (BALANCE-XREF, 2026-07-16).** The research is unanimous that felt power growth Act 1 → boss is what makes a run *a run*, not a sequence of fights (maxim **N8**). As currently scoped, a Phase C run is 3 gigs + boss with ~3 reward picks from a ~2-card pool (Compound Cycle, Pentameter) — the player would end the run with nearly the starter deck. The vertical slice's "publisher-showable multi-encounter run" claim depends on giving the run a **power spine**. Candidate spine: the full **SFX-as-equipment** system (#6b, deferred to Phase C — the ALWTTT relic/artifact layer) + **reward-pool growth** (more than 2 cards). Decide at S6 open how much of that spine S6/S7 must deliver. Acceptance target to add to the Phase C demo-readiness check (§10 / `Roadmap §7.4`): *a viewer watching gig 3 sees the band visibly and audibly stronger than at gig 1 — more SFX stages lit, 2–3 new cards audibly in the mix.*

### 3.2 S7 — Run content I (2 venues + 2 encounters + audience state machine)

**Goal.** Two new playable venues with two distinct encounters using the 4 new audience archetypes. Audience state machine shipped.

**Deliverables.**
- 2 new venue scenes (asset images 7 + 10) — see §5 for venue spec.
- 2 new `GigEncounterSO` assets.
- 4 new `AudienceCharacterData` assets (sprite + state sprites + minimal data per archetype) — see §6.
- Audience state machine implementation: idle / hostile / vibing state transitions, sprite swap + Animator trigger per transition (see §6 for details).

**Sensory Contract.** State transitions are bus events (`AudienceStateChangedEvent` per Design_Sensory_Contract §3). Sprite swap + animator trigger are the minimum sensory artifact; SFX coverage is additive (per directive: FT minimum, FT+SFX preferred, FT+SFX+animator/shader/particle ideal).

**Tutorial.** `tut_audience_state_machine` (per Design_Tutorial_System §7).

**Open at S7 batch open.**
- State-transition trigger rules (see §6.3 for current proposal).
- Whether Cool Dude / Kid are added to the encounter pool or reserved for demo-cut encounter only.

### 3.3 S8 — Run content II (boss + closing sequence)

**Goal.** Boss encounter playable. Boss defeat triggers run-complete closing sequence.

**Deliverables.**
- 1 new `AudienceCharacterBase` subclass for boss per D-RUN-4 = α (subclasses existing infrastructure; no new character class hierarchy).
- 3-5 bespoke boss abilities (specifics deferred to S8 authoring; viability tuned via playtest).
- Boss-specific portrait + state sprites.
- Closing sequence (scene or modal) acknowledging run completion.

**Sensory Contract.** Boss abilities + closing sequence produce FT / SFX / animator triggers per Standing Directive #2.

**Tutorial.** `tut_first_boss_encounter`, `tut_run_complete` (per Design_Tutorial_System §7).

**Open at S8 batch open.**
- Boss ability inventory.
- Boss-specific status (if any — e.g. a "Headliner Stage" buff that activates per phase).
- Closing-sequence format: scene-level vs modal-overlay.

---

## 4. Run shape (D-RUN-3)

| Tier | Encounters | Venues | Notes |
| --- | --- | --- | --- |
| **Ideal** | 3 + boss | 3 (2 new + existing) | Full target per D-RUN-3. |
| **Minimum** | 2 + boss | 2 (1 new + existing) | Acceptable fallback if S7 content scope tightens. |

### Encounter ordering

- Run begins with one of the regular encounters (random or fixed per S6 design decision).
- Boss is always last (encounter index = `requiredEncounterCount - 1`, or a new `GigEncounterSO.IsBoss` flag).
- Existing demo encounter (2× Kid + 1× CoolDude, authored in B3) may be reused as one of the 3 regular slots; the 2 new encounters use the 4 new audience archetypes as the primary content.

### Run-end conditions

- **Win:** beat boss → closing sequence → return to MainMenu.
- **Loss:** lose any encounter (Cohesion 0 or band-wide Breakdown threshold) → run ends → return to MainMenu.
- No mid-run save/resume in vertical slice. Crash-recovery is out of scope.

---

## 5. Venues (asset images 7 + 10 + existing)

| Venue | Source | Status | Notes |
| --- | --- | --- | --- |
| Existing venue | current Gig scene background | retained | Third venue option; no asset work needed. |
| Venue B | asset image 7 (S7 authoring) | new | Layout TBD at S7; reuses existing `StageLightAnimator` and `BackgroundContainer` patterns. |
| Venue C | asset image 10 (S7 authoring) | new | Same as Venue B. |

### Venue scene authoring approach

Scene-prefab + background-image-swap (lightweight). Each venue has:
- Background sprite.
- Audience position list (sprite placement on the background — `AudienceMemberPosList` pattern).
- Stage light positions (existing `StageLightAnimator`).
- Smoke / fire VFX anchor points (S3 sensory coverage; per Design_Sensory_Contract §5).

### Lighting / sensory continuity across venues

- Stage lights (existing `StageLightAnimator`) reused, possibly with per-venue color tuning.
- Smoke / fire VFX threshold tuning per venue acceptable (bigger venue → more intense smoke), but the underlying mechanic (SFX threshold crossings) is shared.
- All venues comply with Sensory Contract Standing Directive #2 — sensory artifacts for every player-visible state change.

---

## 6. Audience archetypes (4 new + Cool Dude + Kid)

### 6.1 New archetypes (S7 authoring)

4 new `AudienceCharacterData` SOs, each authored with:
- Base sprite + idle / hostile / vibing state sprites.
- Default ability set (1-2 abilities per archetype).
- Taste profile (consumed by `ResolveLoopEffect` from S1; archetypes have distinct loop preferences).
- Animator with state-transition triggers (see §6.3).

### 6.2 Encounter authoring uses these + Cool Dude + Kid

S7's 2 new encounters primarily use the 4 new archetypes (e.g. encounter B: 3 new archetypes; encounter C: 2 new archetypes + 1 returning). Cool Dude / Kid distribution across the new encounters is a design decision at S7 open; current default is "available in pool but not required" — gives S7 authoring flexibility.

### 6.3 Audience state machine (idle / hostile / vibing)

**States.**
- `Idle` — default. Neutral expression / sprite.
- `Hostile` — disapproval state. Triggered by negative loop reactions or repeated mismatch with taste.
- `Vibing` — approval state. Triggered by positive loop reactions or repeated match with taste.

**Transition rules (proposed at reframe; refined at S7).**
- `Idle → Hostile`: last loop produced negative `ResolveLoopEffect` for this audience AND audience is not currently Convinced.
- `Idle → Vibing`: last loop produced positive `ResolveLoopEffect` AND audience is not currently Convinced.
- `Hostile → Idle`: timeout (N loops) or neutral loop.
- `Vibing → Idle`: timeout or neutral loop.
- `Hostile ↔ Vibing` direct transitions: allowed when the reaction sign flips strongly.
- `Any → Convinced`: existing Convinced state takes precedence; no further state transitions during the gig.

**Sensory artifacts per transition.**
- Sprite swap.
- Animator trigger (per state).
- Bus event (`AudienceStateChangedEvent`) — consumed by FT / SFX / animator/shader/particle subscribers per Sensory Contract.

**Builds on S1 (B3-slate-F).** State transitions are downstream of `ResolveLoopEffect`'s real values (currently returning 0; S1 fixes). State machine implementation depends on S1 having shipped meaningful per-audience reaction values.

### 6.4 Coexistence with existing status / lifecycle

- Indifference (existing audience-side status): blocks Vibe per `D-DCP-6 = A`; does not directly drive state-machine transitions. An Indifferent audience can still be in any of idle / hostile / vibing — the status orthogonal to the state.
- Earworm: orthogonal — passive Vibe ticks continue irrespective of state.
- Convinced: precedence over state machine (see above).
- Cool Dude / Kid: when added to encounters that use the new state machine, they participate in state transitions like any audience character. Existing Cool Dude / Kid sprites may need state-sprite extensions if S7 authoring chooses to include them in the new encounters.

---

## 7. Boss design (D-RUN-4 = α)

### Approach

- Subclass `AudienceCharacterBase` (existing infrastructure).
- New `AudienceCharacterData` SO with boss-specific data fields (larger thresholds, bespoke ability list).
- **No new character class hierarchy.** D-RUN-4 = α rejects new infrastructure; boss is "an audience character with bigger numbers and rare abilities", not a new entity type.

### Boss properties (specifics deferred to S8 authoring)

- Larger Vibe / Stress thresholds than regular audience characters. Numbers tuned via S8 playtest.
- 3-5 bespoke abilities. Inventory authored at S8 open.
- Optional: 1 boss-specific status that activates per phase (e.g. "Headliner Stage" — buff that scales boss output as the song progresses).
- Distinct portrait + state sprites (idle / hostile / vibing); state-machine compliance maintained.
- May have a phase transition mechanic if S8 design surfaces value (e.g. "stage 2 starts when Vibe ≥ 50%").

### Boss in run flow

- Always the final encounter.
- Defeat → closing sequence → MainMenu.
- Loss in boss encounter → run loss → MainMenu (same as any encounter loss).

---

## 8. Scene transitions (D-RUN-2 = β + D-RUN-5)

### Scene flow

```
MainMenu
  → (optional pilot intro / first-run-only tutorial — S6)
  → first gig
    [Gig won] → RewardCanvas (S6) → ShipHub (S6)
       → next gig
       → ...
    [Boss won] → ClosingSequence (S8) → MainMenu
    [Any loss] → MainMenu
```

### Ship hub (D-RUN-2 = β — dedicated S6, not folded into S5)

Scope at vertical-slice fidelity:
- Single screen.
- Pilot portrait (image 1, D-RUN-5) prominent — interlocutor for tutorials and run progress.
- Minimal UI: continue button. Possibly a "current run status" panel (encounter index, deck preview, audience-archetypes-faced list).
- No interior rooms (bar / chill / rehearsal explicitly out of scope per D-RUN-6).

### Pilot integration (D-RUN-5)

- Asset image 1 confirmed as ship pilot / band manager.
- Used in ship hub (S6) and tutorial dialogues (S4 + S6-S8).
- **No separate character-intro batch needed** — integration folds into S4 (tutorial wiring) and S6 (ship hub).
- Single static portrait sufficient for vertical slice; expression variants deferred.

### Builds on §5.3.5 `GigLauncher`

The single non-Gig→Gig scene transition entry point established by §5.3.5 (`GigLauncher.Launch`) is reused for every gig launch in the run. Encounter dispatch (which encounter is next) is owned by a new minimal `RunController` or extension of the existing demo-launch path; the precise dispatch surface is a S6 batch-open decision (full ladder formalization is deferred per §2 out-of-scope).

---

## 9. Tutorial coverage during Phase C

Per Standing Directive #3 (Tutorial-as-mandatory) and D-TUT-5:

| Session | Dialogues (IDs reserved in Design_Tutorial_System §7) |
| --- | --- |
| S6 | `tut_ship_hub_intro`, `tut_first_reward_choice` |
| S7 | `tut_audience_state_machine` |
| S8 | `tut_first_boss_encounter`, `tut_run_complete` |

Each session is closed only when its tutorial coverage is registered and a smoke test confirms first-trigger fire. See `Design_Tutorial_System_v0_1.md §8` for the full DoD.

---

## 10. Acceptance / Phase C DoD

See `Roadmap_ALWTTT.md §7.4` for the canonical Phase C DoD checklist. Highlights:

- Ship hub stub functional; gig → reward → ship → next gig flow plays end-to-end.
- 2 new venues authored (existing venue retained as third).
- 4 new audience archetypes authored; audience state machine shipped and Sensory-Contract compliant.
- Boss authored per D-RUN-4 = α.
- Tutorial coverage extends across S6-S8 per D-TUT-5.
- All Phase C features produce sensory artifacts per Standing Directive #2.
- No regressions against demo-cut DoD (S5 closure remains valid).

---

## 11. Open questions deferred to per-session resolution

- Whether encounter selection is player-choice or fixed (S6).
- Whether rewards exist by S6 close or are stubs (S6).
- Specific hostile / vibing transition rules — exact triggers, timeouts (S7).
- Boss ability inventory (S8).
- Closing-sequence format — scene vs modal (S8).
- Whether ladder-mode formalization happens during or after Phase C. **Current default: after.**
- **Duplicate rewards — BR-D3 revisit trigger (recorded 2026-07-16, BALANCE-XREF).** Owned-card exclusion in `PersistentGameplayData.BuildRewardCardPool` (D9) is absolute for the demo (every reward is a new axis; *mínimas cartas*). In a multi-gig run with a small reward pool, that exclusion can **empty the pool** — the `RewardCanvas.FinishIfEmpty` path fires and a gig yields no card. The disposition to weigh when this triggers: allow exact **duplicate** rewards (consistency picks — maxim **N7**), matching how the referents let a staple recur. Fires when: reward-pool size minus owned-cards approaches zero across the run. Tie this to the reward-pool-growth decision above (a large enough pool defers the trigger).
- **Run power curve source (S6/S7) — see §3.1 S6 "Open at S6 batch open".** Whether the SFX-as-equipment spine (#6b) + reward-pool growth land in S6/S7 or slip; the "publisher-showable" claim depends on the run feeling like a rising arc (maxim N8).

---

## 12. Cross-references

- `planning/active/Design_Tutorial_System_v0_1.md §7` (tutorial coverage across S6-S8).
- `planning/active/Design_Sensory_Contract_v0_1.md` (event bus; audience state machine compliance).
- `planning/Design_Project_Directives_v0_1.md §D2 / §D3` (Sensory Contract + Tutorial-as-mandatory standing directives).
- `planning/active/Design_Demo_Cut_v1.md` (demo cut as predecessor; S1-S5 closes before S6 opens).
- `Roadmap_ALWTTT.md §7` (Phase C section).
- `Roadmap_ALWTTT.md "Future milestones / Ladder mode"` (deferred ladder formalization; `GigLauncher` foundation suffices for vertical slice).
- `systems/SSoT_Audience_and_Reactions.md` (audience contract; state-machine addition will need a semantic change entry when S7 ships).
- `systems/SSoT_Gig_Encounter.md` (encounter / roster surface; updated when S6 ships if encounter dispatch needs SO-side support).
- `planning/Design_Game_And_Card_Maxims_v0_1.md` (game/card-design maxims, 2026-07-16; N8 rising-arc, N9 reward-skip, N7 duplication, N4 break-the-game combo, N11 difficulty-tiers-as-telemetry all bear on Phase C scope decisions).
- `planning/active/S5_DemoCutClose_Sub_Roadmap.md` → BALANCE-XREF ledger (BR-D1..BR-D4) and TLM-1 (the run logger that will be live before Phase C playtests).

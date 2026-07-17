# S5 — Demo Cut Close — Sub-Roadmap

**Status:** Planning-only. Decomposes `CURRENT_STATE.md §3` row **S5** and `Roadmap_ALWTTT.md §5.4 / §5.5`. Does **not** define implementation truth. Created 2026-06-18. **Revised 2026-07-01** — tester-driven replanning re-sequenced the batch list: S5c→S5i, S5d→S5j, and four new pre-tuning batches S5e–S5h inserted (ledger **D-REPLAN-1..6** below).
**Pattern:** Same role as `M1_5_Dev_Mode_Sub_Roadmap.md` — a durable multi-batch plan + decision ledger; per-batch **deep scope + rehydration prompt are generated at batch-open**, not duplicated here.

S5 is the last demo-cut session: it closes Phase B (§5.5 DoD) and tags the demo, then unblocks Phase C (S6–S8).

---

## Decisions fixed (batch-open ledger)

Vibe delivery & transparency:
- **D-S5-VIBE = B (refined)** — all Vibe delivered at song end; SFX bonus accumulates into a song-scoped `pendingVibe`; new accumulated-Vibe readout. The song-end **conversion already fires once per song** (confirmed in `GigManager`, guarded by `_lastSongFeedback`) — there is **no conversion bug**; the perceived "4 applications in 3 loops" was the mid-song SFX stage bonus (≤3) + 1 conversion.
- **D-S5-VIBE-ARCH = A** — `pendingVibe` bespoke/song-scoped, shaped so the planned Pending Effects layer can absorb it.
- **D-S5-COUNTER = B** — live projection, expanded into a per-audience transparency system (see `Design_Vibe_Telegraph_v0_1.md`).
- **D-S5-TELEGRAPH-SCOPE = B** — C1 (global `L+SFX`) → C2 (per-enemy effectiveness) → C3 (per-enemy number), in that order; degrades cleanly to C1+C2 if time-boxed.
- **D-S5-SFX-SCALE = A** — SFX bonus stays flat (not impression-scaled).
- **D-S5-TELEGRAPH-HOME = `planning/Design_Vibe_Telegraph_v0_1.md`** (created).

Card clarity:
- **D-S5-ICON = A** — resolve musician icon via `MusicianCharacterType → MusicianCharacterData.CharacterIcon` at `SetCard`; fallback no-icon for AnyMusician / generic cards.

Structure / docs:
- **D-S5-CLARITY-SPLIT → split** — Vibe-transparency work (S5a) separated from card-face/animation work (S5b). (Supersedes the earlier "single S5a" once the counter grew into a telegraph system.)
- **D-DOC1 = A** — batch scopes live in this sub-roadmap (durable source + ledger); rehydration prompts derive from it.
- **D-DOC2 = A** — CURRENT_STATE §3 gets only a minimal "decomposed → sub-roadmap" pointer now; detailed §3 + Roadmap §5 update at each batch **close**.
- **D-DOC3** — canonical labels: **S5a** Vibe + transparency · **S5b** Card clarity + animation · **S5e** Core semantics (meter inversion + inspiration simplification) · **S5f** Spanish onboarding + first-gig shape · **S5g** Music variety · **S5h** Reward screen · **S5i** Win-rate loop (was S5c) · **S5j** Presentation + §5.4 + close (was S5d).

## D-REPLAN ledger (2026-07-01 tester-driven re-sequence)

The demo cut is functionally built and got its first real tester round. S5c (win-rate) was deferred in favour of core gameplay/UX/legibility work. This ledger records the re-sequencing decisions; the batches follow in the section list below. All planning-only — no authority/SSoT/contract promotion here; the semantic edits land in **S5e** at close.

- **D-REPLAN-1** — Success signal = **unassisted comprehension + non-monotonous music**, NOT win-rate. Concretely: a Spanish-speaking tester completes the first gig without developer help, understanding that (i) meters deplete to 0 with clear win/lose meaning, (ii) each composition card moves one track of one musician, (iii) the simplified inspiration economy; and the music does not feel repetitive.
- **D-REPLAN-2** — **Four** pre-tuning batches (S5e→S5f→S5g→S5h); findings exceeded the earlier "1–2" estimate; **S5e opens first** (it gates both the tutorial and the tuning).
- **D-REPLAN-3** — **Split S5d**: the presentation half (reward screen) is pulled earlier as **S5h**; the §5.4-readiness + tag + close half stays last as **S5j**. (Tagging the demo before win-rate is tuned is rejected.)
- **D-REPLAN-4** — B3-slate remainder **split**: E-lite (Blocked/"oscurito" tooltip) and #15 (`#if ALWTTT_DEV` gate on `DevAddSongHype`/`DevResetSongHype`, now relevant because S5f adds a SongHype toggle) fold into S5f; G (filter draws during composition) is an S5e/S5f candidate; C/D/#12–14 stay deferred (editor/dev hygiene → S5i filler or post-demo).
- **D-REPLAN-5** — **Minimal Phase C pull**: the demo cut absorbs only the reward screen (S5h, single gig); cross-gig SFX unlock (#6b), 3rd enemy (#9) and 2nd venue (#10) stay in Phase C (with #9/#10 as fast-follow, assets already exist). **The Phase C entry gate is unchanged** — it still opens on demo-cut close / §5.4 pass. **Amended 2026-07-07 (user):** a simplified slice of #6b (venue-SFX unlock-as-reward + persistence flag + bar activation) was pulled into S5h. The rest of #6b — SFX as obtainable equipment across gigs — remains Phase C.
- **D-REPLAN-6** — Sequential re-label (S5c→S5i, S5d→S5j) rather than leaving alphabetic gaps; win-rate/close content is intact, only re-labelled and repositioned.

## BALANCE-XREF ledger (2026-07-16 — deckbuilder balance-research integration)

A comprehensive study of how successful roguelike deckbuilders (Slay the Spire 1/2, Monster Train 1/2, Griftlands) are balanced was cross-referenced against the ALWTTT baseline. The consolidation is documentation-only (no gameplay change) and lands three artifacts: the governed maxims doc `planning/Design_Game_And_Card_Maxims_v0_1.md`, the **TLM-1** batch below (opened, code deferred to run before S5i), and the S5i observation lenses (folded into the S5i section). Full context: `changelog-ssot.md` 2026-07-16 (BALANCE-XREF). Decisions locked:

- **BR-D1 = B (defer, implement later).** The end-of-gig card reward **will** gain an explicit *skip* option (the Slay-the-Spire model — skip is a first-class deck-consistency lever, not a failure state). It is **inert while the demo is single-gig**, so implementation is deferred to Phase C **S6**, which already owns full reward-selection. The *decision* is locked now so S6 does not re-derive it. Home for the implementation scope: `Design_Vertical_Slice_v0_1.md` §3.1.
- **BR-D2 = A (agreed).** A minimal, `ALWTTT_DEV`-gated run **telemetry logger** is added **before S5i** as batch **TLM-1** (below). Rationale: "build the metric server early" is the single loudest research finding; the sensory bus already carries the needed events (`GigOutcomeEvent`, `CardPlayedEvent`, `AudienceVibeImpactEvent`), and the logger's real payoff arrives the day external testers exist — retrofitting later loses every early run. It converts S5i's dead-card / pick-rate observations from opinion (n≈2) into recorded fact.
- **BR-D3 = A (keep exclusion; revisit later).** Owned-card exclusion in `BuildRewardCardPool` (D9) **stays absolute for the demo** — every reward is a new axis (*mínimas cartas*). **Revisit trigger (Phase C):** with a multi-gig run and a small reward pool, exclusion can empty the pool (the `RewardCanvas.FinishIfEmpty` path fires); allowing exact **duplicate** rewards (maxim N7 — consistency picks) is the disposition to weigh then. Trigger recorded in `Design_Vertical_Slice_v0_1.md` §11.
- **BR-D4 = B (no dedicated replan session).** The research is consolidated **now** (this doc pass + the maxims doc), its cheap now-relevant pieces folded into the existing sequence (TLM-1 + the S5i lenses), and its expensive structural items (run power curve, a "break-the-game" combo, a Covenant-style difficulty ladder) **forward-pointed** to the batches that own them (Phase C / meta-progression), not given a session. **S5j is *not* augmented** — it stays §5.4 + tag. A dedicated rebuild was rejected as the broad premature rework the project working-rules warn against on an accepted, near-tag baseline.

## Sequence & dependencies

```
S5a (Vibe + transparency)  ─┐
                            ├─→ S5e ─→ S5f ─→ S5g ─→ ECON-1 ─→ S5h ─→ TLM-1 ─→ S5i ─→ S5j
S5b (card clarity + anim)  ─┘     (semantics)(onboarding)(music)(rewards)(telemetry)(analysis)(close)
```
*(Backbone view — the many inserted batches between S5h and S5i, TUT-REBUILD / BASS-1 / CARD-UX-1 / JUICE-PW / DEMO-FIXES-A, are omitted here for readability; see the "Inserted demo-cut-close batches" section and the reconciled sequence line at the ECON-1 entry. TLM-1 (BALANCE-XREF, 2026-07-16) is the newest insertion, immediately before S5i — **closed 2026-07-16**; the live front is now S5i.)*

S5a and S5b are closed. **S5e opens first** — meter inversion + inspiration simplification are foundational: S5f authors tutorial dialogue against the new semantics, and S5i tunes the new economy, so both depend on S5e landing. S5g (music variety) and S5h (reward screen) are independent of each other but both wanted before the S5i tuning pass (tuning should measure the final content). **S5i** (was S5c) needs the new semantics + economy + music + reward screen in place. **TLM-1** (BALANCE-XREF, BR-D2=A) slots immediately before S5i so the structured playtest has a run logger from its first session. **S5j** (was S5d) is the gate: §5.4 readiness + tag + close Phase B + unblock Phase C.

---

## S5a — Vibe delivery + transparency

**Objective.** Make the Vibe→audience causal chain legible and deliver all Vibe at song end. Implements `Design_Vibe_Telegraph_v0_1.md`.

**Tasks (plan-level).**
1. Refactor `ApplySfxBonusVibe` to accumulate into a song-scoped `pendingVibe`; pay out once at song end alongside the conversion. Repurpose the mid-song `+N Vibe!` gold floater to feed the readout, not apply Vibe.
2. Add a live `avgImpression(i)` accessor (current part running impressions + closed parts).
3. C1 — global `L + SFX` readout under the SongHype bar (loop-boundary cadence).
4. C2 — per-enemy effectiveness telegraph (Super / Normal / Not-very / Immune) from live `avgImpression`.
5. C3 — per-enemy projected number on each audience (small add-on over C2).

**DoD.** All Vibe lands at song end; readout shows accurate `L + SFX`; per-enemy effectiveness + number track live impression and match the song-end deltas; no double-application; consistent with existing cyan/gold floater language.
**Smoke outline.** Verify single song-end payout (not per-loop); readout matches applied deltas; effectiveness flips with impression; blocked/indifferent shows Immune; regression: SFX total unchanged vs pre-refactor.
**Docs at close.** `Design_Demo_Cut §3.1` (SFX deferred + readout), `SSoT_Scoring_and_Meters §6` (note: SFX paid at song-end via pendingVibe; conversion was already once), coverage-matrix + SSoT_INDEX (Vibe-telegraph concept row).

## S5b — Card clarity + animation  *(recommended first)*

**Objective.** Remove the two readability blockers playtesters hit (card type, card owner) and fix the playing-animation correctness; instrument win-rate for S5c.

**Tasks (plan-level).**
1. **Item 1 — card-type backgrounds.** Toggle `Action Bg` / `Composition Bg` in `CardBase.SetCard` by `def.IsAction` / `def.IsComposition`.
2. **Item 4 — musician icon on card.** Resolve `MusicianCharacterType → MusicianCharacterData.CharacterIcon`, assign to a card `Image`; no-icon fallback.
3. **Item 3 — playing-animation gating.** Beat/playing animation only for musicians with an **active track this loop**. *First sub-task: confirm the track→musician mapping + the live active-track query in `CompositionSession` (the one real unknown).*
4. **Win-rate instrumentation.** Dev-surfaced gig-outcome readout (W/L count per session) so S5c is measurable.
5. **Prefab parity.** Add the two bg children + the icon `Image` to `CardUI.prefab` and any other card prefab (currently only in `Card3D`); wire on `CardBase`; **null-guard** (UI-fix-A NRE recurrence vector).

**DoD.** Action vs composition visually unmistakable on every card prefab; correct musician icon (or none); only actually-playing musicians animate; outcome readout works; no NRE on any card prefab.
**Smoke outline.** Each prefab shows correct bg per type; icon matches owner / absent for AnyMusician; play a loop where a subset of musicians have tracks → only those animate; inventory + gameplay prefabs both render.
**Docs at close.** `SSoT_Card_System` (card presentation: bg-by-type + owner icon).

## S5e — Core semantics: meter inversion + inspiration simplification  *(opens first)*

**Objective.** Align meters with VJ convention (deplete-to-0) and collapse the inspiration economy to fixed-supply + special-card spend, so the first gig is intuitive and the S5i tuning surface is stable.

**Tasks (plan-level).**
1. Invert Stress → musician "mental fortitude" HP (0 = collapse) and Vibe/audience-persuasion → enemy HP (0 = convinced/conquered). Same mechanics, inverted sense. *Open at batch-open: decide whether Composure / Flow / SongHype / LoopScore / Cohesion also flip or stay, for consistency (D1).*
2. Flip UI meter fills to the new direction; re-touch the S5a C1/C2/C3 telegraph for inverted direction + labels (telegraph infrastructure stays).
3. Fixed inspiration-per-loop (start 2 or 3, tuneable on `GigFlowSettings` / `DemoLaunchConfig`); strip basal inspiration-generation from card definitions.
4. Keep / redesign inspiration **cost** on special cards; ensure ≥1 spender remains so the resource stays live. **+INS as a CardEffect is DEFERRED** (design-note only this batch — needs design + playtest).

**DoD.** Meters deplete to 0 with correct win/lose semantics; telegraph reads correct inverted; economy = fixed + special-spend with ≥1 live spender; regression-clean vs pre-inversion (LoopScore magnitudes + S5a telegraph totals unchanged — only direction/semantics flip).
**Docs at close (authority-class — this batch, not deferred).** `SSoT_Scoring_and_Meters` (inverted meter semantics), `SSoT_Audience_and_Reactions` (persuasion-to-0), `SSoT_Card_Authoring_Contracts` (basal gen retired + `+INS`-deferred note), CURRENT_STATE + changelog.

## S5f — Spanish onboarding + first-gig shape  *(depends on S5e)*

**Objective.** A Spanish-speaking tester learns the inverted meters, the simplified economy, and the track-based composition model unassisted, in a simplified first gig.

**Tasks (plan-level).**
1. Refactor `TutorialDialogSO` / `TutorialDialogCatalogSO` to per-language dialogue keys (ESP + ENG); re-author existing dialogues in Spanish; ESP default in the tester build.
2. Tutorial content — enemy HP→0 = convinced/conquered, musician HP→0 = collapse (#2); song = tracks (RHYTHM / BACKING / MELODY), each composition card moves ONE track of ONE musician, real card effect explicit (#5); simplified inspiration explained (#3-tutorial).
3. Start the first gig **without** the SongHype bar + a config toggle to enable/disable it (#6a).
4. Fold-ins (D-REPLAN-4): E-lite (Blocked / "oscurito" tooltip), #15 (`#if ALWTTT_DEV` gate on `DevAddSongHype` / `DevResetSongHype`, now relevant because of the SongHype toggle); G (draw filtering) if the batch touches that surface.

**DoD.** All tutorial content authored in Spanish via the localization structure, ENG fallback present; covers meters / economy / tracks; SongHype bar hidden + toggle in gig 1; D3 coverage for the changed mechanics; D2 sensory artifacts for the collapse / convinced events (if not already delivered by S5e).
**Docs at close.** `Design_Tutorial_System` (localization structure + Spanish coverage; §6A/§8 status), `SSoT_Editor_Authoring_Tools` if the dialog-SO schema is governed there, CURRENT_STATE + changelog.

**Status (2026-07-04, CLOSED):** S5f complete and formally closed 2026-07-04. Spanish-dialogue sub-batch (dual catalog D-S5f-2=B, copy voice D-S5f-1, pagination cap D-S5f-5=B, post-S5e copy corrections, parity check) smoke-verified — **ST-S5f-1..9 all PASS**. First-gig-shape riders (D-REPLAN-4 fold-ins) implemented and smoke-verified — **ST-S5f-R1..R9 all PASS**: E-lite Blocked tooltip (sprite-hover legend, ESP, D-S5f-7=A; no status icon — M1.2 E3 intact), #15 `#if ALWTTT_DEV` gate on `DevAddSongHype`/`DevResetSongHype`, gig-1 SongHype bar toggle (`GigPresentationSO.ShowSongHypeBar`, D-S5f-6=B; demo asset OFF), telegraph effectiveness labels ESP (D-S5f-8=A). DoD met. Next in sequence: **S5g** (music variety) — opens with the mandatory boundary-scoping step below.

### S5f-ext — tutorial content extension *(queued; depends on S5f; D-S5f-3=B)*

Token system `{$concept}` for gameplay-term substitution in dialog pages (enables rich-text/color/size styling of concept names in `TutorialOverlayView`), then the new dialogs written against it: per-track-type intros (Rhythm / Backing / Melody; Bassline / Harmony when they land), "each character has their own cards", and a multi-language authoring/parity editor window. Scope guard: presentation + content only; no gameplay control.

### Post-demo (vertical slice) — guided tutorial *(D-S5f-4=B)*

Scripted-hand seeding + action-wait gameplay freeze for a fully guided first gig ("clarity without our explanation"). Deliberately post-demo: real design surface (deck seeding, gate extension to await-player-action states); demo playtest confusion data feeds its design.

## S5g — Music variety  *(PRIORITY; boundary-scoping + seed wiring done, authoring pending)*

**Objective.** Kill the monotony — each composition card produces **≥5** distinctive musical patterns (#8).

**Tasks (plan-level).**
1. **Mandatory opening step (boundary scoping).** Read the composer SSoTs (`SSoT_Composer_Backing/Rhythm/Melody_Track`, `SSoT_Runtime_Generation_Orchestration`, `SSoT_ALWTTT_MidiGenPlay_Boundary`) + the bundle / palette SOs (`TrackStyleBundleSO`, `ChordProgressionLibrary/PaletteSO`, `PhrasePalette/ArchetypeSO`, `RhythmCardConfigSO`, `MelodicStyleSO`). **Confirm whether MidiGenPlay already selects with variety across multiple patterns per card, or whether variety-selection needs a mechanism** → this determines whether #8 is pure ALWTTT palette-authoring or partly cross-project.
2. Author: Sibi Minor + Major (≥5 progressions each); C2 time-signatures (≥5 rhythm patterns per TS); Sibi Melody (≥5 melodic patterns). Use the `chord-progression-importer` + `rhythm-pattern-generator` skills.

**DoD.** Each composition card yields ≥5 audibly distinct patterns across loops; no immediate repetition within a song; boundary held (ALWTTT authors palette content; MidiGenPlay owns selection — if selection needs a change it is filed cross-project, like MGP-ALWTTT-MOD-DIR-1). Backed by **D1 (Sound Design Priority)**.
**Docs at close.** the palette-binding home (`SSoT_Card_System §5.2.1` or wherever the card→palette table lives) for the expanded content; any new #8 design doc indexed in this batch; boundary-doc note if a cross-project ask is filed; CURRENT_STATE + changelog.

**Status (2026-07-06, CLOSED).** Boundary-scoping + seed wiring closed in the 2026-07-05 sub-batch (`MGP-ALWTTT-SEED-1` adopted; `ST-S5gb-1..5` PASS). Authoring pass complete: Sibi Minor/Major progressions, C2 rhythm patterns per time signature (4 palettes × 6 entries), Sibi melodic hooks (`PhrasePalette_SingingField` ×5). Card→palette bindings in `SSoT_Card_System.md §5.2.1`; `ALWTTT-PCE-PROP` resolved (`CURRENT_STATE.md §1`). Closing smokes `ST-S5g-1..5` all PASS. DoD met (each composition card ≥5 audibly distinct patterns; boundary held). Next in sequence: **ECON-1** (per-turn play economy) — see below.

## ECON-1 — Action Economy v1  *(inserted 2026-07-06, design with Matías; CLOSED 2026-07-07)*

1 Action card + 1 Composition card per musician per period (pre-song PlayerTurn window counts as one period; reset per loop). Inspiration cost intact — cards with cost > 0 read as "finishers". Includes pip UI (2 circular sprites per musician on `BandCharacterCanvas`), budget checks on both card-play paths, resets at `PlayerTurn` / `OnPlayPressed` / loop-finished seams, and a starter cost audit (baseline → 0).

**Status (2026-07-07, CLOSED).** Code (T1–T6) applied and validated; `ST-ECON-1..7` PASS. Decisions D-ECON-1..5=A; **D-ECON-6=DEFER** (all starter costs 0 today; finisher card designation deferred to a future batch). Primary home `SSoT_Gig_Combat_Core.md §14`; rationale `Design_Action_Economy_v1.md`. **S5i inherits the note:** tune finisher costs over the new economy (fewer total spends → current costs may be too cheap). Documentation closed in the 2026-07-07 DOCUMENTATION pass.

Resulting sequence: S5g ✅ → ECON-1 ✅ → **S5h ✅** → [TUT-REBUILD ✅ → inserted demo-cut-close batches: BASS-1/BASS-CARD-1 ✅ · CARD-UX-1 ✅ · JUICE-PW ✅ · **DEMO-FIXES-A ✅** (DF-INSPLOOP / DF-ARTIC backlog riders)] → **TLM-1** (BALANCE-XREF, opens before S5i) → S5i → S5j. *(Reconciled at doc-pass apply, 2026-07-13: the DIFF-S5h-4c text named a pending "[Tutorial rebuild — design]" step; that arc has since run and closed — TUT-REBUILD, 2026-07-10.)*

## Inserted demo-cut-close batches (TUT-REBUILD spillover, 2026-07-09)

The TUT-R* arc lives in `TUT-REBUILD_Sub_Roadmap.md`; these three fell out of it and register here (non-blocking polish; slot around S5i, land before demo showing).

- **CARD-UX-1** — ✅ **CLOSED 2026-07-13.** Unplayable-card red overlay with **one playability source of truth** (`GigManager.EvaluateCardPlayability` → `UnplayableReason`; primary home `SSoT_Card_System.md` §10.5). Landed: the 5 playability inputs (tutorial gate, action timing, inspiration, **new** final-loop composition lock, ECON-1 budget — the latter **partially scoped**, `AnyMusician` excluded pending **D-ECON-GENERIC**); the **spawn hook** (option C) that registers `TutorialHighlightTarget` on runtime characters / status icons / hand cards, closing the world-character + hand-card highlights deferred from TUT-R3; per-event highlight precision (D3=B); the T3b world→screen edits applied in build; and — **added in-batch** — the beat-8 `SingleCardOnly` finisher-only gate, which is what actually gates compositions in the tutorial's final loop (the lock is exempt under a hold). Decisions D1=C / D2=A / D3=B / D4=A / D5 / D6=A. Smokes ST-R3b-2/5 + ST-CU-1..13 green. Doc pass: `CARD-UX-1_Doc_Diffs_2026-07-13.md`, applied 2026-07-13.
- **JUICE-PW** — ✅ **CLOSED 2026-07-13.** Psychic Waves presentation, on the sensory bus. Landed: new per-target bus event **`AudienceVibeImpactEvent`** published from the `ModifyVibeSpec` branch of `CardBase.ExecuteEffects` (D1=A) — **not** `CardPlayedEvent`-driven as originally scoped here, because that event fires *after* resolution, once per card, with no per-target delta and no way to express an Indifference block; per-member staggered FT (`-N` / grey `INDIFFERENT`) + one `CardVibeImpact` sting per card play (D3=A: visual fans out, audio does not); procedural `CharacterAnimator.PlayImpactKick` + particle burst on each landed member and on Sibi (D2=B — no clip system); `AudioType = None` on the card so the impact sting replaces the drop sting (D-PW-AUDIO). The publish site guarantees the FX precedes the beat-8 hold release. `CharacterSfxProfileSO` was **not** touched (it stays reaction-only, phase 1). Smokes ST-PW-1..10 green, no deferrals. Doc pass: `JUICE-PW_Doc_Diffs_2026-07-13.md`. **Open:** the sting's clip is a placeholder (`Telephone`) → D1 (final SFX authoring).
- **DEMO-FIXES** — rolling demo fixes/adjustments backlog; recommend a lightweight backlog doc. **Registered items (registry expanded 2026-07-13, user — not all need to land in one session; DF-INSPLOOP and DF-ARTIC may split out):**
  - **DEMO-FIXES-A — ✅ CLOSED 2026-07-15.** Landed: **DEMO-TUT-TOGGLE** (gig-open opt-in prompt; `PD.TutorialEnabled` single flag; `TutorialGuidedDriver.PrepareForGig`; runtime driver resolution via `UIManager.GigCanvas`, D-DF-8=A) · **R1** (beat-8 `HandHas`, D-DF-4=A) · **CT1** (persistent highlight pulse survives modal close) · **DF-COST0** (hide cost badge at cost 0, D-DF-5=A) · **DF-ECONTIP** (pip hover tooltip, D-DF-6=A) · **DF-CATALOG** (runtime band-catalog union; `AllCardsList` → fallback, D-DF-7=A). Decisions D-DF-1..8 all = A. Smokes ST-DF-1..6 + 8..13 PASS; ST-DF-7 (finisher-in-pile) deferred to Dev Mode / M1.5 (unreachable in normal play). Doc pass: `DEMO-FIXES-A_Doc_Diffs_2026-07-15.md`. First real test of the final-loop composition lock on a non-tutorial path (ST-DF-3 PASS).
    **Still open in DEMO-FIXES:** `DF-INSPLOOP` (new effect spec — own batch, ideally before S5i) · `DF-ARTIC` / `MGP-ALWTTT-ARTIC-1` (cross-boundary MidiGenPlay).
  - **DF-INSPLOOP** — "+INSP per loop" as a **card effect** (behaves like `inspirationGenerated > 0` on the session's per-loop gain). New effect spec ⇒ goes through the `SSoT_Card_Authoring_Contracts.md` §9 extension rule; open design decisions to surface at batch open: duration (song-scoped? part-scoped? permanent?), stacking, and interaction with the retired basal generation (S5e D-S5e-1=A locked `inspirationGenerated = 0` project-wide; this effect is the deliberate, card-gated way back in) + the inert LoopScore complexity term (CURRENT_STATE §4, owned by S5i).
  - **DF-ARTIC** *(cross-boundary)* — **randomized chord articulation.** Today a card binds exactly one articulation; wanted: a Random option, potentially re-rolled per loop and per chord. The randomization mechanism is **MidiGenPlay-internal** (composer/config side) — per the boundary rule it is filed as a cross-project ask (**MGP-ALWTTT-ARTIC-1**, same pattern as SEED-1 / MOD-DIR-1); the ALWTTT side owns only the authoring surface that requests it and adopts after the package ships.

## S5h — Reward screen  *(IMPORTANT; pulled-forward presentation half of old S5d, per D-REPLAN-3)*

**Objective.** Implement the end-of-gig reward screen (graphic asset exists, code missing — #7) so a gig has payoff and a sense of progression.

**Tasks (plan-level).**
1. Wire `RewardCanvas`; de-bypass / branch the demo `IsFinalEncounter → WinPanel` hack; author demo reward content / flow for a single gig.
2. D2 sensory + D3 tutorial (`tut_first_reward_choice`-style, Spanish via S5f).
3. **#6b deferred here** — SFX-as-reward + "from gig 2" needs multi-gig → Phase C S6; leave the SFX-unlock design noted. Cover refresh (old S5d) is an optional rider here or with S5j.

**DoD.** Reward / victory screen renders from the existing asset at gig end; reward flow works in a single gig; sensory + Spanish-tutorial coverage; no regression in WinGig routing. Overlaps S6 — full reward-selection + multi-gig stay in S6; this is the pulled-forward slice.
**Docs at close.** presentation home (CURRENT_STATE interim per the S5b presentation-debt precedent, unless a presentation SSoT is promoted), CURRENT_STATE + changelog.

**Status (2026-07-07, CLOSED).** Reward screen wired: final-encounter routing
de-bypassed (Win → RewardCanvas → WinPanel, D1=A); card sourcing moved to the
flag-driven `BuildRewardCardPool` (D2=B) with owned-card exclusion (D9) and a
provenance-aware `GrantRewardCard` (D4, fixes the `ChoiceCard` action-list
mis-file). **#6b-lite absorbed by user decision (2026-07-07):** venue-SFX
**unlock as a reward** — sequential lights→smoke→fire (D6=A), run-scoped
persistence surviving Retry (D7=A), locked thresholds fully inert (D8=A), and
SongHype-bar activation once any threshold is unlocked (S5f toggle untouched;
unlock is a second activation source). New `RewardChoiceOpenedEvent` +
`SensorySfxType.RewardOpened` + `tut_first_reward_choice` (ES/EN). Closing
smokes `ST-S5h-1..10` all PASS. DoD met (reward screen renders from the asset
at gig end; single-gig flow; sensory + Spanish-tutorial coverage; no WinGig
routing regression). **S5i inherits the D8 balance note:** gig 1 now has **no**
SFX Vibe bonus — evaluate first-gig win-rate without it. **Full SFX-equipment
system (shops/events/per-slot assignment) stays Phase C.** Next in sequence:
S5i (structured playtest) — but a tutorial-rebuild design session is being run
first (user, 2026-07-07). *(Doc pass applied 2026-07-13 — `S5h_Doc_Diffs_2026-07-07.md`
retired; the tutorial-rebuild session named above has since run and closed as
TUT-REBUILD, 2026-07-10.)*

## TLM-1 — Run telemetry logger ✅  *(inserted 2026-07-16, BALANCE-XREF / BR-D2=A; **closed 2026-07-16**)*

**Closed 2026-07-16.** `DevRunTelemetryLogger` (Assets/Scripts/DevMode/, whole file `#if ALWTTT_DEV`) shipped: read-only sensory-bus subscriber writing one JSON-Lines record per gig on `GigOutcomeEvent`. Decisions **D-TLM-1=A** (JSON Lines), **D-TLM-2=B** (mandated fields + `schemaVersion`/`timestampUtc`/`sessionId`/`requiredSongCount` + per-play `isComposition`/`inspirationCost` + per-audience `convinced`), **D-TLM-3=A** (cohesion-collapse losses NOT logged — that path never publishes `GigOutcomeEvent`; same blind spot as the session tally; `lossCause` constant `"unconvinced_after_final_song"` for logged losses). **ST-TLM-1..4 + ST-TLM-R1 all PASS.** Surface documented in `SSoT_Dev_Mode.md` §17 (+ §9.12 smoke rows). No gameplay/behavioral change; MidiGenPlay untouched; strips clean without `ALWTTT_DEV`.

> **Optional rider — TLM-1b (opened 2026-07-16, backlog, not scheduled).** Publisher-side fix for the D-TLM-3=A gap: add a `GigOutcomeEvent` publish on the cohesion-collapse loss path (`MusicianBase.OnBreakdown → BandCohesion 0 → LoseGig()`) so those losses are recorded. Cost: a `GigOutcomeEvent`-per-gig double-fire latch (else `ResolveGigOutcomeAndEnd` + `LoseGig` double-publish), plus review of the `DevGigOutcomeTracker` tally semantics and the `tut_first_gig_won` debug-path exposure. Open it only if S5i playtests actually hit cohesion losses; otherwise the gap stays documented.

**Objective.** Give S5i (and every later playtest, internal or external) a structured, per-gig record of what was played and how the gig ended — so pick-rate / dead-card / pacing observations become recorded fact instead of n≈2 recollection. Dev-only surface; **zero production impact; zero gameplay change.**

**Why now (research grounding).** "Build the metric server early, even in prototype" is the most-repeated finding across the study (Mega Crit's in-house server ran from the prototype stage and grew from 3 graphs to 90+; Shiny Shoe and Klei both run analytics + in-game feedback pipes). The two metrics every studio names as primary are **pick rate** ("too low and it's basically not a card in our game") and **appearance-in-winning-runs**. ALWTTT currently has only `DevGigOutcomeTracker` (a per-session W/L tally) — S5i would otherwise run blind on both primary metrics. The logger is cheap because the sensory bus already carries the needed events.

**Tasks (plan-level).**
1. A `#if ALWTTT_DEV`-gated logger (subscriber to `SensoryEventBus`) that writes **one record per gig** on `GigOutcomeEvent`. Minimum fields: outcome (win/lose) + loss cause if derivable; songs survived / loops played; per-audience end-Vibe (resistance remaining); **cards played with the song index at play time** (the confound guard — see below); per-card play counts for the gig. Roster + encounter id for grouping.
2. Output as local JSON or CSV under a dev path (not shipped; not in `Resources`). Append-per-gig; human-readable.
3. Provenance discipline: log the *player's* plays, not effects the engine auto-resolves. Reuse existing events (`CardPlayedEvent` for plays, `AudienceVibeImpactEvent` for per-target deltas) — do **not** add new gameplay events.
4. Optional (only if cheap): surface a one-line "last gig written to …" in the Dev Stats tab next to the existing tally.

**Confound guard (the "Madness"/SFX lesson).** The research's central metrics caution is that late-appearing content shows up in winning decks *because it appears late, after weak decks are already dead* (StS's Madness). ALWTTT has the identical trap already built: SFX Vibe banks at song end and the "fire" stage fires late-gig, so any card played in song 3 will spuriously correlate with wins. **Logging song-index-at-play-time is mandatory** precisely so late-run confounds are separable at analysis time.

**Constraints.** `ALWTTT_DEV`-gated (strips clean from Player builds — the S5f `#if ALWTTT_DEV` precedent, ST-S5f-R4/R5); no semantic or gameplay change; MidiGenPlay untouched; the logger only *reads* the bus. Future home for the surface: `SSoT_Dev_Mode.md` (sibling of `DevGigOutcomeTracker` / `DevStatsTab`).

**DoD.** A valid record is written on **both** a win and a loss in a normal gig; song-index-at-play-time is present and correct; nothing is logged outside `ALWTTT_DEV`; no gameplay/behavioral change (regression: a normal gig with the logger active is byte-identical in outcome to one without). Smoke set **ST-TLM-1..4** (win record written; loss record written; song-index correctness across a multi-song gig; production-build strip = clean compile, no residual symbols).

**Docs at close.** `SSoT_Dev_Mode.md` (new logger surface, sibling to the outcome tracker) · `CURRENT_STATE.md` (§2 active + §4 if it clears any open item) · `changelog-ssot.md`. Reference/operational only — no authority or contract change.

## S5i — Gameplay design analysis + structured playtest  *(was S5c "win-rate tuning loop"; reframed 2026-07-05 per D-S5g-5=A)*

**Objective.** Structured playtest and gameplay-design analysis against the **final** content (inverted meters + simplified economy + music variety + reward screen) — surfacing friction points, weightless decisions, dead cards, and pacing issues. Win-rate 60–80% on the demo encounter is one output signal among several, **not** the batch's sole objective.

**Guard.** Only adjustments that (a) do not reopen semantics already closed earlier in S5 (S5e's meter/economy inversion, S5f's onboarding content) and (b) do not block S5j may be implemented inside S5. Everything else is filed to Phase C, backed by the playtest evidence gathered here.

**Tasks (plan-level).** Several short play→measure→adjust rounds (not closeable in one sitting; bounded by playtest throughput). Now backed by the **TLM-1** run logger (BR-D2). Levers available within the guard above: fixed inspiration-per-loop + special-card costs, `sfxBonusVibeStage1/2/3`, `MaxVibeFromSongHype`, impression band, encounter tuning. The **[S5a-SMOKE] hype-24–53% observation** applies here but only to **gig-2+** (S5f hides the SongHype bar in gig 1, so most of that range is deliberately out of the first-gig experience). B3-slate C / D / #12–14 can ride as non-blocking filler.

**Observation lenses (added 2026-07-16, BALANCE-XREF).** Beyond the standing friction/dead-card/pacing analysis, S5i explicitly records three research-derived signals. These are *lenses on the existing playtest*, not new work:
- **L1 — Comprehension at the 15-unique starter.** The starter's uniqueness ratio (17 cards / 15 unique) is a deliberate divergence from genre norms (StS ~10/4), and heavy duplication is an onboarding aid the demo forgoes (see `Design_Starter_Deck_v1.md` §4). Verify unassisted comprehension (D-REPLAN-1) *holds* at this ratio. **If it fails, adding exact copies of the most legible cards is the sanctioned fallback (maxim N7), not an E1 violation.**
- **L2 — The poggable-moment audit.** The research is unanimous that occasional *earned* overpowered-ness is core genre appeal, and ECON-1 + song-end banking + Indifference gates + flat SFX currently cap every ceiling. Ask of each playtest: *does any moment feel powerful?* If nothing ever does, that is a finding filed to Phase C (the natural candidate is the deferred Earworm × Captivated combo — the ALWTTT "Corruption + Dead Branch"). See maxim N4.
- **L3 — The zero-margin finisher (D-DEMO-1).** Psychic Waves costs exactly the 3-loop income (1+1+1 = 3, zero margin). Research framing: a finisher only *exactly* affordable with perfect play reads as a trap, not a payoff. Watch specifically for new-player frustration on this line; margin-1 is the default posture to consider if it reads as punitive (this echoes the D-DEMO-1 option-B intent). See maxim N5 (numbers are cheap to change).

Also inherited: the ECON-1 note (fewer total spends ⇒ current finisher costs may be too cheap) and the S5h D8 note (gig 1 has no SFX Vibe layer — evaluate first-gig win-rate without it).

**DoD.** Friction points, weightless decisions, dead cards, and pacing issues are catalogued from structured playtest sessions; win-rate lands in or is deliberately tuned toward 60–80% as one of the recorded signals; every implemented adjustment satisfies the guard above; everything else is filed to Phase C with its supporting evidence.

## S5j — Presentation close + §5.4 readiness + tag  *(was S5d; the gate)*

**Objective.** Run the §5.4 readiness checklist + invariant re-check (F-1/F-3/F-4, MB1–4, M4.5); cover refresh if not already done; close docs (CURRENT_STATE / Roadmap §5 / changelog); tag the demo. Closing S5 closes Phase B and unblocks Phase C. (The reward-screen presentation moved earlier to S5h per D-REPLAN-3, so this batch is closure + readiness, not reward-UI build.)

---

*Per-batch deep scope (verifiable task list + full smoke tests + final DoD) and the rehydration prompt are produced when each batch is opened (M1_5 pattern).*

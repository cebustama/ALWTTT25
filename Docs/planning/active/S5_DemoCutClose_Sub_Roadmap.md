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
- **D-REPLAN-5** — **Minimal Phase C pull**: the demo cut absorbs only the reward screen (S5h, single gig); cross-gig SFX unlock (#6b), 3rd enemy (#9) and 2nd venue (#10) stay in Phase C (with #9/#10 as fast-follow, assets already exist). **The Phase C entry gate is unchanged** — it still opens on demo-cut close / §5.4 pass.
- **D-REPLAN-6** — Sequential re-label (S5c→S5i, S5d→S5j) rather than leaving alphabetic gaps; win-rate/close content is intact, only re-labelled and repositioned.

## Sequence & dependencies

```
S5a (Vibe + transparency)  ─┐
                            ├─→  S5e ─→ S5f ─→ S5g ─→ S5h ─→ S5i ─→ S5j
S5b (card clarity + anim)  ─┘      (semantics)(onboarding)(music)(rewards)(win-rate)(close)
```

S5a and S5b are closed. **S5e opens first** — meter inversion + inspiration simplification are foundational: S5f authors tutorial dialogue against the new semantics, and S5i tunes the new economy, so both depend on S5e landing. S5g (music variety) and S5h (reward screen) are independent of each other but both wanted before the S5i tuning pass (tuning should measure the final content). **S5i** (was S5c) needs the new semantics + economy + music + reward screen in place. **S5j** (was S5d) is the gate: §5.4 readiness + tag + close Phase B + unblock Phase C.

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

**Status (2026-07-05, IN PROGRESS — not closed).** Boundary-scoping step complete: confirmed MidiGenPlay's variety mechanism is a per-render seed parameter (`seedOverride`), not an internal selection change; filed and adopted same day as `MGP-ALWTTT-SEED-1` (`SSoT_ALWTTT_MidiGenPlay_Boundary.md §8.1`). Seed wiring implemented in `CompositionSession` + `MidiMusicManager.RenderSinglePart`; smoke-tested — `ST-S5gb-1..5` all PASS. Remaining before this batch can close: the authoring pass above (Sibi Minor/Major progressions, C2 rhythm patterns per time signature, Sibi melodic patterns) + closing smokes `ST-S5g-1..5`.

## S5h — Reward screen  *(IMPORTANT; pulled-forward presentation half of old S5d, per D-REPLAN-3)*

**Objective.** Implement the end-of-gig reward screen (graphic asset exists, code missing — #7) so a gig has payoff and a sense of progression.

**Tasks (plan-level).**
1. Wire `RewardCanvas`; de-bypass / branch the demo `IsFinalEncounter → WinPanel` hack; author demo reward content / flow for a single gig.
2. D2 sensory + D3 tutorial (`tut_first_reward_choice`-style, Spanish via S5f).
3. **#6b deferred here** — SFX-as-reward + "from gig 2" needs multi-gig → Phase C S6; leave the SFX-unlock design noted. Cover refresh (old S5d) is an optional rider here or with S5j.

**DoD.** Reward / victory screen renders from the existing asset at gig end; reward flow works in a single gig; sensory + Spanish-tutorial coverage; no regression in WinGig routing. Overlaps S6 — full reward-selection + multi-gig stay in S6; this is the pulled-forward slice.
**Docs at close.** presentation home (CURRENT_STATE interim per the S5b presentation-debt precedent, unless a presentation SSoT is promoted), CURRENT_STATE + changelog.

## S5i — Gameplay design analysis + structured playtest  *(was S5c "win-rate tuning loop"; reframed 2026-07-05 per D-S5g-5=A)*

**Objective.** Structured playtest and gameplay-design analysis against the **final** content (inverted meters + simplified economy + music variety + reward screen) — surfacing friction points, weightless decisions, dead cards, and pacing issues. Win-rate 60–80% on the demo encounter is one output signal among several, **not** the batch's sole objective.

**Guard.** Only adjustments that (a) do not reopen semantics already closed earlier in S5 (S5e's meter/economy inversion, S5f's onboarding content) and (b) do not block S5j may be implemented inside S5. Everything else is filed to Phase C, backed by the playtest evidence gathered here.

**Tasks (plan-level).** Several short play→measure→adjust rounds (not closeable in one sitting; bounded by playtest throughput). Levers available within the guard above: fixed inspiration-per-loop + special-card costs, `sfxBonusVibeStage1/2/3`, `MaxVibeFromSongHype`, impression band, encounter tuning. The **[S5a-SMOKE] hype-24–53% observation** applies here but only to **gig-2+** (S5f hides the SongHype bar in gig 1, so most of that range is deliberately out of the first-gig experience). B3-slate C / D / #12–14 can ride as non-blocking filler.

**DoD.** Friction points, weightless decisions, dead cards, and pacing issues are catalogued from structured playtest sessions; win-rate lands in or is deliberately tuned toward 60–80% as one of the recorded signals; every implemented adjustment satisfies the guard above; everything else is filed to Phase C with its supporting evidence.

## S5j — Presentation close + §5.4 readiness + tag  *(was S5d; the gate)*

**Objective.** Run the §5.4 readiness checklist + invariant re-check (F-1/F-3/F-4, MB1–4, M4.5); cover refresh if not already done; close docs (CURRENT_STATE / Roadmap §5 / changelog); tag the demo. Closing S5 closes Phase B and unblocks Phase C. (The reward-screen presentation moved earlier to S5h per D-REPLAN-3, so this batch is closure + readiness, not reward-UI build.)

---

*Per-batch deep scope (verifiable task list + full smoke tests + final DoD) and the rehydration prompt are produced when each batch is opened (M1_5 pattern).*

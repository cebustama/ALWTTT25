# changelog-ssot — ALWTTT

This changelog records **semantic / documentary changes** (meaning, contracts, authority,
promotions/demotions, splits/merges/supersessions, implemented-semantics changes that required
doc updates). Cosmetic / grammar / formatting-only edits are not logged here.

> **Rotated 2026-07-01 (D-DOC-ROTATE=C).** The full project history from **2026-03-18 →
> 2026-06-22** (Governance migration Batch 01 through S5a) was archived **verbatim** to
> `archive/changelog-ssot_2026-03-18_to_2026-06-22.md`. This active file was restarted with the
> milestone index below + go-forward entries. Nothing was summarised destructively — the archive
> is the complete record; this file is the scannable current-window log. Newest entries at top.
> Go-forward header convention: `## YYYY-MM-DD — TITLE` (the archived newest region used a plain
> `YYYY-MM-DD — TITLE` line without `##`; do not replicate that drift here).

---

## Milestone index of the archived history (2026-03-18 → 2026-06-22)

Navigation only — dates + labels. Full entries are in
`archive/changelog-ssot_2026-03-18_to_2026-06-22.md`.

- **Demo-cut close — S-sequence + audio + tutorial (2026-06-12 → 2026-06-22).** S5a Vibe
  delivery + transparency (06-22) · S5b card clarity + animation (06-20) · S4 first-time
  tutorial controller + guided jam (06-17) · DOC-HYGIENE CURRENT_STATE §1 prune (06-16) ·
  TUT-JAM-SEQ guided-jam design (06-16) · audio stream: AUDIO-CHAR-PROFILES 1/2, AUDIO-AMBIENCE,
  AUDIO-OST, AUDIO-SFX-FIX, M-AUDIO-MIX + `SSoT_Audio` (06-15/16) · S3-audio SFX layer (06-14) ·
  S3a sensory polish visual (06-14) · S2 sensory event bus foundation (06-14) · S1 (B3-slate-F)
  audience reactions ratified + neutral FT (06-12).
- **CE-L1 — LLM-assisted card authoring in the Card Editor (2026-06-11).**
- **Planning reframe + B3 content + Sibi identity + modulation (2026-05-20 → 2026-05-23).**
  Planning-reframe: demo cut S1–S5 + vertical slice S6–S8, standing directives D1/D2/D3 (05-23) ·
  ALWTTT-MOD-DIR-2/3 directional-modulation hint (05-22) · B3-content-cards: BPM (Push It / Half
  Time) + Key Lift modulation (05-22) · planning-reorg doc split (05-20) · B3-content-sibi +
  followup: Sibi voice identity (05-20).
- **Phase B — gameplay loop polish (2026-05-09 → 2026-05-18).** §5.3.5 demo cut prep: GigLauncher
  + SFX→FlatVibe + zero-click entry (05-18) · B3-content-audience + B3-demo-polish: Cool Dude +
  Kid + Indifference + demo encounter (05-17) · planning batches opened: audience pool + demo
  prep + pitch deck + sound directive (05-15) · B2.5 polish refinements + cleanup (05-15) · B2
  polish layer (05-13) · B1 loop simplification + track persistence + UI rework (05-12) · Phase A
  closed / Phase B opened (05-09).
- **M4.6-followup — F-1..F-5 + MB3/MB4 (2026-05-07 → 2026-05-08).** SongOrchestrator IOOR
  defense (F-4 Stage A), action-card inspiration session routing (MB4), inspiration Dev-path
  drift + session-start carry-over (MB3), per-loop draw + canonical AddCurrentInspiration (F-3),
  GigSettings 4-SO refactor (F-2), action-card double-discard (F-1).
- **M4.6-prep — starter deck + editor tooling (2026-05-01 → 2026-05-06).** Cleanup: starter deck
  authoring + Card Editor tooling (05-06) · Gig Setup roster pickers merged (1)/(4) (05-04) ·
  authoring tooling QoL batch (3) (05-03) · UI-fix-A + UI-fix-B inventory viewer (05-02) ·
  per-musician starter deck auto-assembly batch (2) (05-02) · DeckCardCreationService MB2
  catalogue migration (05-01).
- **M4 — Starter Deck Foundations (2026-04-26 → 2026-04-30).** M4.5 bidirectional guaranteed
  draws (04-30) · M4.4 deck contract evolution / card copies (04-29) · post-MVP planning:
  Pending Effects + Tempo-coupled identity (04-28) · M4.3 Earworm first audience-side status
  (04-28) · M4.2 Flow bifurcation + adaptive LoopScoreCalculator (04-28) · M4.1 Fix C1 unified
  Stress + M1 milestone close (04-26) · starter deck axis resolution / repetition→variety pivot
  (04-26).
- **Milestone 1 — Authoring & Testing Infrastructure (2026-04-08 → 2026-04-26).** M1.1 Deck
  Editor polish + M1 close (04-26) · MB1/MB2 dispatch alignment + catalogue split (04-24) ·
  MidiGenPlay expressive-surface reference docs + design maxim (04-24) · M1.5 Phase 3.x Dev Mode
  stat/state editing (04-23/24) · M1.9 card sizing, M1.3b SpecialKeywords, M1.10 detail modal,
  M1.3c stacked tooltips, M1.3a status descriptions (04-20 → 04-23) · M1.8 status icon animations
  + M1.7 hover highlight (04-20) · M1.5 Phase 1/2 Dev Mode (04-17/20) · M1.2 status icon SO
  migration (04-14) · `SSoT_Editor_Authoring_Tools` created (M1.4, 04-08).
- **Project scope broadened Combat MVP → full ALWTTT game project (2026-04-08).**
- **Combat MVP — Phase 4 closure (2026-03-23).**
- **Governance migration + boundary hardening (2026-03-18 → 2026-03-19).** Batches 01–06
  (subsystem SSoTs, runtime + music-integration authority, audience/status/scoring authority,
  encounter structure, final-tree normalization) + ALWTTT ↔ MidiGenPlay boundary hardening
  micro-pass.

---

## 2026-07-05 — S5g (seed-wiring sub-batch): per-song render seed + MGP-ALWTTT-SEED-1 adopted

**Type:** lifecycle. S5g remains **open** — this entry records a wiring sub-batch, not a closure. Authoring (drum patterns × TS, melody Singing Field) and closing smokes `ST-S5g-1..5` are still outstanding.

- **Authority (cross-project).** `MGP-ALWTTT-SEED-1` filed, delivered, and adopted 2026-07-05, same day: MidiGenPlay added `int? seedOverride` to `GenerateSong`/`GenerateSinglePart`. Seed **policy** is ALWTTT truth (`SSoT_Runtime_CompositionSession_Integration.md §10`); the selection mechanism stays MidiGenPlay's. Boundary entry: `SSoT_ALWTTT_MidiGenPlay_Boundary.md §8.1`.
- **Semantic.** `CompositionSession` now derives one render seed per song in `Begin()` (run entropy), passes it to every `RenderSinglePart` call for that song, and clears it in `End()`. This replaces the accidental stability of the package's constant `defaultSeed` with an explicit contract: intra-song stable, cross-song varied.
- **Operational.** Wiring implemented in `MidiMusicManager.RenderSinglePart` + `CompositionSession`. Dev override `CompositionSession.DevPinnedSongSeed` added, code/debugger-only for now (`SSoT_Dev_Mode.md §8.7`; tab-wiring tracked in the new idea backlog, §16 of the same doc).
- **Decisions locked at this batch's open.** `D-S5g-2=A`, `D-S5g-4=A`, `D-S5g-5=A`, `D-S5g-7=C`, `D-S5gb-1=A` (one line each below). `D-S5g-1`, `D-S5g-3`, `D-S5g-6`, `D-S5g-8`, `D-S5gb-2`, `D-S5gb-3` were also locked at this batch's open per the handoff note, but their one-line resolutions were not present in this session's working context — recorded here as a gap, not fabricated; pull them forward from the batch-open record at the next docs pass.
  - `D-S5g-2=A` — a future MidiGenPlay post-generation per-loop micro-variation feature was noted as a forward possibility, not actioned this batch.
  - `D-S5g-4=A` — the six PCE-PROP paste-ready doc blocks apply at S5g's close, together with the authoring pass (see Part B of this doc-update package, deferred).
  - `D-S5g-5=A` — `S5i` reframed from win-rate-only tuning to gameplay-design-analysis + structured playtest, with win-rate as one output signal among several (`S5_DemoCutClose_Sub_Roadmap.md`).
  - `D-S5g-7=C` — deterministic anti-repeat declined package-side (MidiGenPlay's own D4); ALWTTT accepts probabilistic non-repetition with ≥6-entry palettes for the demo.
  - `D-S5gb-1=A` — the `trackInputsHash` stem-cache key is unchanged by the seed; cross-song isolation continues to rely on the `Begin()`/`End()` clear (Integration SSoT invariant 9), now runtime-verified.
- **Smoke status.** `ST-S5gb-1..5` all PASS. Three are described in the source package — cross-song seed variety (`ST-S5gb-1`), intra-song stability under re-render (`ST-S5gb-2`), runtime-verified cross-song cache isolation (`ST-S5gb-3`); `ST-S5gb-4`/`ST-S5gb-5` are recorded PASS but their individual assertions are not in this session's context. Closing smokes `ST-S5g-1..5` (authoring-dependent) remain outstanding.
- **Docs touched.** `SSoT_Runtime_CompositionSession_Integration.md` (new §10 + §3.1/§9 cross-refs), `SSoT_ALWTTT_MidiGenPlay_Boundary.md` (new §8.1), `SSoT_Dev_Mode.md` (§6 entry-points bullet + new §8.7 + new §16 idea backlog), `CURRENT_STATE.md` (§2/§3 active-work line + §4 open item), `S5_DemoCutClose_Sub_Roadmap.md` (S5g status note + S5i reframe).
- **Not touched this entry.** The `[GAP — UNVERIFIED] ALWTTT-PCE-PROP` stub in `CURRENT_STATE.md §1` resolves at S5g's close, together with Part B of this doc-update package — left untouched here on purpose. **Reconciliation note for that closure:** PCE-PROP's own `D3=A` ("determinism = deterministic per build, package-threaded seed") predates and is superseded in spirit by this batch's seed-variety policy (§10 of the Integration SSoT); the close-out pass should note that supersession explicitly rather than let the two stand as unreconciled claims about what the seed is for.
- **Placement deviation.** The idea-backlog item originally specified for `M1_5_Dev_Mode_Sub_Roadmap.md` was placed in `SSoT_Dev_Mode.md §16` instead — that roadmap is archived per `SSoT_INDEX.md` (superseded by this SSoT), and adding fresh planning content to an archived doc would silently reopen a retired planning surface.

---

## 2026-07-04 — S5f CLOSED: first-gig-shape riders + formal close

- **Operational (closure).** S5f formally closed. **ST-S5f-1..9 (dialogue) all PASS** (resolves the pending confirmation noted in the dialogue entry below) and **ST-S5f-R1..R9 (riders) all PASS**. Active focus advanced to S5g; `CURRENT_STATE.md §2/§3` flipped S5f→S5g (S5g opens with the mandatory boundary-scoping step).
- **Semantic (first-gig shape, D-REPLAN-4 fold-ins).** Four riders landed (`GigManager.cs`, `GigPresentationSO.cs`, `AudienceCharacterCanvas.cs`, `AudienceCharacterBase.cs` + 1 asset):
  1. Blocked "oscurito" hover tooltip on audience sprites (`AudienceCharacterBase.OnPointerEnter/Exit` + `AudienceCharacterCanvas.ShowBlockedTooltip`/`HideBlockedTooltip`; ESP hardcoded per D-S5f-7=A; **no status icon — M1.2 E3 intact**).
  2. `#if ALWTTT_DEV` gate on `GigManager.DevAddSongHype`/`DevResetSongHype` (#15) — stripped from non-dev builds. The Dev-Mode Gig-Wide Stats SongHype **slider** (`DevSetSongHypeAbsolute`, guarded by `GigDevSettingsSO.debugSongHype`) is a separate path and was already gated.
  3. `GigPresentationSO.ShowSongHypeBar` toggle gating the single `SetSongHypeVisible(true)` call site in `GigManager.OnPlayPressed` (D-S5f-6=B; demo asset OFF for gig 1). SongHype accrual, stage SFX, and song-end Vibe conversion unaffected — only the bar + C1 "L + SFX = N" readout hide.
  4. Telegraph effectiveness labels ESP (D-S5f-8=A, `AudienceCharacterCanvas.LabelFor`): SuperEffective → "¡Súper!", NotVeryEffective → "Resiste", Immune → "Inmune", Normal → "Normal".
- **Reference / home correction (D1=A).** The D-S5f-8 telegraph-label ESP note was filed to its authoritative home **`Design_Vibe_Telegraph_v0_1.md §4`**, not `SSoT_Scoring_and_Meters §6` — the `VibeEffectiveness` enum + effectiveness mapping live in the telegraph design doc; §6 owns only the L+SFX conversion math.
- **Docs touched.** `SSoT_Gig_Combat_Core §12` (GigPresentationSO concerns cell — `showSongHypeBar` visibility), `SSoT_Dev_Mode` (#15 gate note + slider/Add-Reset distinction), `SSoT_Status_Effects §3.2` (Blocked-legend addendum), `Design_Vibe_Telegraph_v0_1.md §4` (ESP labels), `S5_DemoCutClose_Sub_Roadmap` (S5f status → CLOSED), `CURRENT_STATE` (§2/§3 active pointer + B3-slate E-lite/#15 RESOLVED).
- **Anchor-drift note.** The riders' paste-ready doc edits (`S5f_Riders_Doc_Edits_2026-07-04.md`) were reconciled against live docs at apply time: #15 had no standalone bullet (annotated inline within the design-gaps-(4) list), the §12 `GigPresentationSO` cell was shorter than the edit assumed (S5f addition appended to the actual cell text), the SSoT_Dev_Mode target documented the SongHype **slider** not the Add/Reset pair (note reworded), and the telegraph-label edit was retargeted per D1=A. **Pre-existing (separate follow-up, NOT folded into S5f):** the §12 `GigPresentationSO` cell still omits the S5a SFX→FlatVibe / SongHype-stage-threshold concerns documented in §5.2/§5.3.5.

---

## 2026-07-04 — S5f (dialogue sub-batch): Spanish onboarding + dual tutorial catalog

- **Semantic.** Tutorial copy for `tut_first_audience_action`,
  `tut_first_song_end`, `tut_first_loop_inspiration` rewritten in EN + ES to
  the S5e inverted semantics (depleting Stress/Vibe pools; fixed
  inspiration-per-loop). No pre-inversion "fill/climb the bar" language
  remains in any authored copy.
- **Structural (minor).** `TutorialDialogCatalogSO` seeder split per
  language (`SeedDemoCutDialogsEN` / `SeedDemoCutDialogsES`, parameterized
  seed dir); new ES catalog asset + 11 ES dialog assets under
  `Assets/Resources/Data/Tutorial/Dialogs/ES/`; editor-only parity check
  menu (`ALWTTT/Tutorial/Validate catalog language parity`); dialog pages
  capped at 2 per trigger (D-S5f-5=B), rhetorical-cut authoring + auto-fit
  fallback. Runtime surface unchanged; trigger ids unchanged (persisted
  `firedDialogs` compatible).
- **Decisions.** D-S5f-1 (tú, condescending/reverent voice), D-S5f-2=B
  (dual catalog), D-S5f-3=B (tokens + track/character dialogs → S5f-ext),
  D-S5f-4=B (guided tutorial → post-demo), D-S5f-5=B (2-page cap,
  rhetorical cut, auto-fit as fallback). Ledger + voice rule:
  `Design_Tutorial_System_v0_1.md §5A`.
- **Smoke status.** ST-S5f-1..9 confirmation pending as of this entry —
  this entry records the authored/structural change, not a batch-closure
  claim; `CURRENT_STATE.md §2` active-work line is intentionally left
  unchanged pending that confirmation. **(Resolved 2026-07-04: ST-S5f-1..9
  all PASS; S5f closed and `CURRENT_STATE §2` advanced to S5g — see the
  S5f-close entry above.)**

## 2026-07-02 — S5e core-semantics inversion + S5e-ext visibility rider

Semantic: SSoT_Scoring_and_Meters §5/§6/§7.3 (+§3.2 note),
SSoT_Audience_and_Reactions §4.1–4.3, SSoT_Card_Authoring_Contracts
inspiration fields — plus a C=B consistency sweep propagating the inversion
through Scoring §2.3 and Audience §2 / §3 (model table: `VibeGoal` row →
`MaxVibe`) / §4 heading / §4.2 heading / §5.2 / §10 / §11, so the retired
`VibeGoal` and pre-inversion "progress" wording are removed project-doc-wide
within these files. Stress → depleting mental-fortitude pool (0 =
Breakdown); Vibe → depleting persuasion-resistance pool (0 = Convinced);
VibeGoal concept retired into MaxVibe. Inspiration economy: fixed 3/loop
(D2), `inspirationGenerated` content-deprecated (D3), `+INS` CardEffect
deferred. Composure/Flow/SongHype/LoopScore/Cohesion semantics unchanged
(D1). LoopScore complexity term deliberately inert (D-S5e-1=A, locked).

Operational: `PersistentGameplayData` musician-seed fix (Current now seeds
at Max, not 0). Meter-bar visibility policy (hidden-if-full,
visible-if-damaged, hover reveals) and card gen-badge auto-hide (S5e-ext,
same close).

Deferred (D-S5e-DOC-D, pending): the convince-condition inversion is **not**
yet reflected in `SSoT_Gig_Combat_Core.md` or `SSoT_Gig_Encounter.md` (both
still state `Vibe >= VibeGoal`), nor in `Design_Starter_Deck_v1.md` /
`ALWTTT_Combat_MVP_Audit_Final.md`. Code is already inverted, so this is a
docs-lag flagged in CURRENT_STATE §4, not a code divergence.

All 10 S5e smoke tests + 7 S5e-ext smoke tests passed.

## 2026-07-01 — S5 REPLANNING: tester-driven re-sequence of the demo-cut close (planning-only) + changelog rotation

**Type:** lifecycle / structural (planning). No code. **No authority / SSoT / contract
promotion** — the semantic edits this session sets up land in **S5e** at that batch's close.

**Context.** The demo cut is functionally built and got its first real tester round (Spanish
testers). S5c (win-rate tuning) was deferred in favour of core gameplay / UX / legibility work.
A one-session replanning clustered 11 tester findings and re-sequenced the remaining demo-cut
work. Framed throughout as improving the existing, playable build.

**Re-sequence** (see `planning/active/S5_DemoCutClose_Sub_Roadmap.md`, ledger **D-REPLAN-1..6**):
- Four new pre-tuning batches inserted: **S5e** (meter inversion + inspiration simplification) →
  **S5f** (Spanish onboarding + first-gig shape) → **S5g** (≥5 musical patterns per composition
  card) → **S5h** (end-of-gig reward screen).
- **S5c → S5i** (win-rate tuning; content intact, repositioned after the four new batches).
- **S5d → S5j** (§5.4 readiness + tag + close); its presentation half (reward screen) was pulled
  forward to S5h per the D-REPLAN-3 split.
- **D-REPLAN-1** success signal = unassisted comprehension + non-monotonous music, **not**
  win-rate.
- **D-REPLAN-5** Phase C entry unchanged: still opens on demo-cut close / §5.4 pass; the reward
  screen moving to S5h narrows S6's reward work to selection + multi-gig carry-over.
- Deferred out of the demo cut: cross-gig SFX unlock (#6b → S6); 3rd enemy (#9) + 2nd venue
  (#10) → Phase C S7 fast-follow (assets already exist, with a "how to author enemies/venues"
  doc each); design-idea backlog (per-character action+composition split, C2 "tank" ability,
  Sibi mind-read card, breakdown rebellion).

**Docs edited this session (all planning-only).** `CURRENT_STATE.md` §2/§3/§4;
`Roadmap_ALWTTT.md` §5.4/§5.5 (stale B3 + demo-cut-prep checkboxes flipped; S5e–S5i DoD items
added; SSoT-edit line corrected) + §7.1 (S6 reward-scope note);
`S5_DemoCutClose_Sub_Roadmap.md` (D-REPLAN ledger + S5e–S5j sections + re-sequenced diagram).

**Lifecycle — changelog rotation (D-DOC-ROTATE=C).** This file was rotated at this point. The
full **2026-03-18 → 2026-06-22** history (Governance migration through S5a) was archived
**verbatim** to `archive/changelog-ssot_2026-03-18_to_2026-06-22.md`, and this active file was
restarted with the milestone index above + go-forward entries. Rationale: the changelog had
grown to ~3,961 lines / ~350 KB; per governance §E it is the full semantic history, and §15.3
forensic value is preserved by the verbatim archive while the active file stays small and
scannable. A compressed *content* summary (rotation option B) was rejected as a drift vector
(a second, divergent representation of history). Active filename is unchanged, so
`SSoT_INDEX.md` / `coverage-matrix.md` need no edit; the supersession trail (governance §18.6)
lives in the archive header + this preamble + this entry.

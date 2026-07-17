# TUT-REBUILD — Sub-Roadmap

**Status:** **CLOSED (2026-07-10).** TUT-R1 / R2 / R2b / R2c / R3 landed. Retirement of the 9 superseded ids applied in-build (parity green, `extra:[]`); world→screen + per-beat highlight registration applied; `ReservedUnauthored` exemption + `tut_first_reward_choice` authored. All smokes green (ST-R3-1..7, ST-R3a-1..3, ST-R3b-1/3/4/6). T3a doc riders applied 2026-07-10. Remaining tutorial work lives outside this arc: world-character + hand-card highlights → **CARD-UX-1** (`S5_DemoCutClose_Sub_Roadmap`); **TUT-R4** stays event-driven (below).
**Parent:** `Roadmap_ALWTTT.md §5.4 / §5.5` (demo-cut close; sibling of `S5_DemoCutClose_Sub_Roadmap.md`). *Confirm the exact § pointer against Roadmap_ALWTTT — not re-read at creation.*
**Authority:** Implementation truth = `planning/active/Design_Tutorial_System_v0_2.md` + code (`Assets/Scripts/Tutorial/`, `Assets/Scripts/Sensory/`). This planning doc tracks the TUT-R\* arc only.
**Pattern:** Same role as `M1_5_Dev_Mode_Sub_Roadmap.md` / `S5_DemoCutClose_Sub_Roadmap.md` — a durable multi-batch registry + decision ledger; per-batch **deep scope + rehydration prompt are generated at batch-open**, not duplicated here.
**Created:** 2026-07-09 — retroactive home. The arc was previously tracked only via the TUT-R1 doc + rehydration prompts, with no entry in `Roadmap_ALWTTT.md` or a sub-roadmap (the gap this doc closes).

---

## Goal

Replace the reactive first-time tutorial **as the gig-1 primary path** with a driver-sequenced **guided curriculum** ("clarity without our explanation"), while retaining the reactive layer as fallback + post-song-1 coverage of the beats with no guided equivalent (D3=B). **Supersedes D-S5f-4=B** — the guided/scripted tutorial was pulled from the vertical slice into the demo cut.

---

## Batches

- **TUT-R1 — curriculum design.** ✅ ES/EN copy + per-beat spec for the 10-beat gig-1 arc + `tut_composure` + rewritten reactives. Decisions **D-TUT-R1-1=A** (Psychic Waves cost 3) · **-2** (owner Sibi) · **-3** (Keep Cool) · **-4** (`tut_flow` reserved). *Copy authority migrated to the `TutorialDialogCatalog` seeders; the TUT-R1 copy doc is retired from the PK.*
- **TUT-R2 — guided infra.** ✅ `TutorialGuidedDriver`, `TutorialScriptedDrawQueue` (M4.5 seam), input gates (beats 3/5), holdLoop (beat 8), D8 tokens, `MusicianStressHitEvent` + `AudienceBlockedEvent` (code-truth findings **F1**/**F2**), runtime suppression of superseded ids, provisional seeders. CT1–CT6 + RT1–RT8 + ST1–ST12 green.
- **TUT-R2b — hotfix.** ✅ Driver v2 (**FIX-1** publish-before-grant ordering; **FIX-2** beat-7 swallow), pacing **D-TUT-R2b-1=B** (audio runs, progression holds), pulse highlights, `GigPresentationSO` telegraph toggles, seeders v2 (de-dashed, Psychic Waves naming, Blocked de-iconified).
- **TUT-R2c — polish.** ✅ `TutorialHighlightTarget` registry (serialized-binding fallback), beat-3 basic-composition allow-list, `tut_play_finisher_early` (prio 81). Naming **Gran Final → Psychic Waves**; finisher magnitude **4→5**.
- **TUT-R3 — doc pass + retirement + copy + highlights.** ✅ Closed 2026-07-10.
  - **T1** — superseded retirement spec (9 ids: constants + call sites + `SupersededIds` + seeders + 18 `.asset`s + parity `ReservedUnauthored`) + de-dash of the 2 retained reactives.
  - **T2** — `Design_Tutorial_System_v0_2.md` (**approved**).
  - **T3a** — cross-doc riders (Starter_Deck v1.3, Demo_Cut §1.1, changelog ×2, CURRENT_STATE, coverage-matrix) + this sub-roadmap + **Keep Cool → C2-owned** (generic-catalog move deferred, see open).
  - **T3b** ✅ — world→screen mask + per-beat `TutorialHighlightTarget` registration (the one bit of new runtime). Static UI/hand/audience_area wired; world-character + hand-card highlights deferred to CARD-UX-1.
  - **Closure:** ✅ retirement applied in-build; ST11/ST12 + all TUT-R3 smokes green; T3a riders applied (2026-07-10). `TUT-R3_Doc_Diffs_2026-07-09.md` + `TUT-R3_T3a_Doc_Diffs_2026-07-10.md` retired from the PK at close.
- **TUT-R4 — Tutorial Browser EditorWindow.** ⏳ Event-driven; opens when the portrait/viñeta art arrives. ES/EN side-by-side; trigger/prio/cat/highlight visible; viñeta assignment. `TutorialDialogSO.portrait` is per-dialog today; **per-page viñetas would be a schema change** (decide at open).

---

## Decision ledger

Authoritative ledger: **`Design_Tutorial_System_v0_2.md §10`.** Arc summary: D1–D9 (TUT-REBUILD) · D-TUT-R1-1..4 · **D-TUT-R2-1=B** (starter gens→0) · **D-TUT-R2b-1=B** (pacing) · **D-TUT-R2c** (naming / magnitude 4→5 / registry / beat-3 allow-list / early variant) · **O1=A** (Blocked de-iconified) · **O2=A** (parity `ReservedUnauthored`) · **D-DEMO-1=4 loops** · VERIFY-DOC-STARTER-1 → 6 Action / 11 Composition = 17.

### Open (surfaced TUT-R3, not owned here)
- **D-ECON-GENERIC** — who spends the ECON-1 per-musician action budget when an **AnyMusician** card is played. Home: `Design_Action_Economy_v1.md` / `SSoT_Gig_Combat_Core §14`. Blocks the generic-catalog move (Keep Cool workaround: assign to C2). Also gates **CARD-UX-1**'s "spent per-character budget" playability input.

---

## Cross-references

- `planning/active/Design_Tutorial_System_v0_2.md` (implementation-design authority).
- `S5_DemoCutClose_Sub_Roadmap.md` (sibling; hosts CARD-UX-1 / JUICE-PW / DEMO-FIXES).
- `changelog-ssot.md` (2026-07-09 TUT-REBUILD infra + TUT-R3 entries).
- `coverage-matrix.md` line 33 (tutorial row).
- `Roadmap_ALWTTT.md §5.4/§5.5` (parent, demo-cut close).

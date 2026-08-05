# DOC-APPLY-1 — Application report (2026-07-31)

**Batch:** DOC-APPLY-1 — apply the full pending diff stack (R0 → R1 → R2(+R2c/R2d) → CONT-B(+CTX-1/1b) → AUTH-1(+1b)).
**Mode:** DOCUMENTATION. No code, no assets, no smoke tests (documentation-only, zero behavior change).
**Result:** all five packages applied (2 hunks partial/adapted, 1 deferred — detailed below). 27 documents emitted complete. Five diff packages marked for retirement.

---

## 1. Session-open decision resolutions (D-DOC-A1..A5, input 7)

| ID | Resolution | Basis |
|---|---|---|
| **D-DOC-A1** | **Case (b) — real doc-vs-doc drift.** The changelog's `2026-07-23 — ROSTER-XP planning` entry claims the roadmap's Roster Expansion section was repointed; the live `Roadmap_ALWTTT.md` (Last updated 2026-05-20) still carried the pre-repoint text verbatim (bass "currently not on any critical path"; Captivated + `ApplyIncomingVibe` "deferred from M4.3"). **Full P7 applied** (starter-ratio bullet + both prerequisite bullets). Correction recorded here per the package's instruction. |
| **D-DOC-A2** | **One document, revised.** Only `RosterExpansion_R2_Doc_Diffs_2026-07-31.md` exists in the PK; its header declares it **supersedes** the never-applied 07-30 draft (whose §8.4 was wrong about `SelfPocket` adoption). R2c and R2d are folded into the 07-31 package (title: "R2 · R2c · R2d"). AUTH-1's reference to the 07-30 filename is a stale internal reference (AUTH-1 was written against the draft); no separate packages exist. Nothing to discard — the 07-30 file was never in the PK. |
| **D-DOC-A3** | **Included** (recommendation adopted). One-line amendment on the campaign ledger: sub-roadmap §2 `D1 = C` now carries "**Amended 2026-07-31 (D-SEQ-3=A):** R4+ open after the **snapshot tag**, not the demo-cut (S5j) tag." The three freshly-inserted texts that restated the S5j gate (R1-11's CURRENT_STATE block, R2-8's roadmap append) carry an inline amendment parenthetical so no document contradicts the ledger. |
| **D-DOC-A4** | **(i) included, (ii) excluded** (recommendation adopted). The D-S5e-DOC-D convince-condition sweep was applied: `SSoT_Gig_Combat_Core.md` (§4.4 meter table + §5.3 rule) and `SSoT_Gig_Encounter.md` (§5.2 / §8.1 / §9.2) corrected in place to the code truth (`Vibe` is an enemy-HP-style resistance pool starting at `MaxVibe`, depleted by incoming Vibe; Convinced at `Vibe <= 0`; `VibeGoal` retired into `MaxVibe` — verified against `AudienceCharacterStats.cs`, `CheckConvincedThreshold`). `Design_Starter_Deck_v1.md` and `ALWTTT_Combat_MVP_Audit_Final.md` received dated **correction banners** (historical texts deliberately not rewritten). The three stale `VibeGoal` cached invariants in `ssot_manifest.yaml` were swept (extends R1-16b's own sweep note). The CURRENT_STATE §4 D-S5e-DOC-D bullet is now marked **RESOLVED 2026-07-31**. CSV-3-DOC stays out of scope. |
| **D-DOC-A5** | **No anchor collision.** AUTH-1 touches CURRENT_STATE §2 (operational bullets) + changelog top; CONT-B touches §1 table + §2 + §4 + §5 + changelog top. Distinct insertion points throughout; the only shared surface is the changelog's 2026-07-31 date band. **Order fixed as recommended: CONT-B above AUTH-1** (semantic content change above editor-only), with R2 (also dated 07-31 in its FINAL package, contra D2's assumed 07-30) below both. Final changelog order, top→bottom: CONT-B → AUTH-1 → R2+R2c+R2d → R1 → R0 → ROSTER-XP planning. In CURRENT_STATE §2 the CONT-B listening bullet sits above the AUTH-1 bullet, mirroring the changelog. |
| **Input 7** | `CONT-B_Returns_MidiGenPlay_2026-07-31.md` **does not exist in the PK.** Per the rehydration instruction ("NO se edita aquí — solo confirmar que existe") it was **not generated** this session. The CONT-B changelog entry's reference to it stands; the dangling state is recorded as an **open obligation** (§6 below), and the three CONT-B asks registered in the CSV sub-roadmap §5 are marked "to file — via the returns doc (pending)". Its full content spec survives in retired-package form only via CONT-B §7 — producing the file is the highest-priority follow-up, since retirement of the CONT-B package otherwise orphans the deviation record (D3/D5/D5b/D6/D8/D9). |

---

## 2. Application matrix — what was applied, per package

### R0 (`RosterExpansion_R0_Doc_Diffs_2026-07-23.md`) — retired on apply

| Hunk | Status | Notes |
|---|---|---|
| P1.1–P1.5 (`Design_Track_Card_Levels_v0_1.md` → v0.2) | **APPLIED** | All 5 hunks + §7 heading retitle, verbatim. |
| P2.1–P2.8 (sub-roadmap) | **APPLIED** | Verbatim; P2.5's Zig row subsequently updated by R1-14(a) as designed. |
| P2.9 (§11 R0→R1 rehydration prompt) | **PARTIAL** | The hunk requires the R1 prompt "verbatim from the R0 session close" — that text is **not in the PK** and was not reconstructed (no-invention rule). §11 replaced with a supersession record: R1 and R2/R2c/R2d have since closed; the next prompt owed is R3's, at R3 open. Loss is historical-only. |
| P3 (changelog) / P4.1, P4.3 (INDEX) / P5 (manifest) / P6 (CURRENT_STATE §3) | **APPLIED** | Verbatim. P6's content was later subsumed by R1-11's block replacement (see R1 below). |
| P4.2 (INDEX sub-roadmap status) | **APPLIED (reconciled)** | Verbatim text would read "next batch R1", false at emit time (R1/R2 closed by this same stack). Status cell reconciled to "R0 closed 07-23; R1 closed 07-23; R2+R2c+R2d closed 07-31 — next batch R3". Neither R1 nor R2 owned an INDEX edit, so this staleness was the stack's own artifact. |
| P7 (roadmap) | **APPLIED — full, case (b)** | See D-DOC-A1. |
| P8 (cross-boundary doc-correction note) | **FILED ALWTTT-side** | The note's queue home is now `CSV_Composition_Validation_Sub_Roadmap.md` §5 (the established cross-boundary ask queue), verbatim, labelled as a doc correction. The sub-roadmap §9 V4 pointer "see §8 note P8 in the R0 diff file" was repointed there (the diff file is retired). No MidiGenPlay document was edited (boundary rule). |
| P9 (placement + no-edit decisions) | **APPLIED** | `Design_Starter_Deck_v2_DRAFT.md` renamed to **`Design_Starter_Deck_v2.md`** (D5: the DRAFT is the final doc), header adjusted with a dated finalization note, placement `planning/active/`. The P9 no-edit list respected in full: no R0-driven edits to `Design_Starter_Deck_v1` (its only edit this session is the unrelated D-S5e banner), `coverage-matrix`, the four runtime SSoTs, `Design_Audience_Status_v1` (its edits are R1's), or the maxims doc. |

### R1 (`RosterExpansion_R1_Doc_Diffs_2026-07-23.md`) — retired on apply

| Hunk | Status | Notes |
|---|---|---|
| R1-1 (SSoT_Status_Effects §5.8 Captivated) | **APPLIED** | Anchor verified (§5.7 final "Applied by" line → before `---` / §6). Asset names `StatusEffectCatalogue_Audience` / `Cantante_CardCatalogData` accepted as corroborated (precondition 2); no contradicting evidence in the PK. Apply-time check DIFF-R1-7: **no §5.x Indifference spec exists** in SSoT_Status_Effects, so the §10 bullet's parenthetical pointer (Indifference documented at the `ApplyIncomingVibe` gate, §5.3) was kept as written. |
| R1-2 (§5.7 Earworm routing correction) | **APPLIED** | Verbatim. |
| R1-3/4/5 (Design_Audience_Status_v1 full supersession) | **APPLIED** | Verbatim. |
| R1-6/7 (Audience_and_Reactions §5.3/§10) | **APPLIED** | Verbatim. Note: these two docs were **not** in the rehydration input list but are R1 targets present in the PK; applied and flagged. Same applies to Scoring_and_Meters and Design_Audience_Status_v1. |
| R1-8/9 (Scoring_and_Meters §6.1/§6.2) | **APPLIED** | Verbatim. |
| R1-10/11/12 (CURRENT_STATE §1/§3/§4) | **APPLIED** | **R1-11 stacked-anchor note:** R1's package claimed anchor-independence from R0, but R1-11's OLD block is the pre-R0 §3 text, which R0-P6 had just modified. Resolved by replacing the R0-modified block wholesale with R1-11's NEW block, which subsumes the R0 closure content (its own R0 bullet). R0-P6's longer verification detail survives in the R0 changelog entry, so nothing is lost from the record. The block's S5j-gate line carries the D-SEQ-3=A amendment parenthetical (D-DOC-A3). |
| R1-13 (CURRENT_STATE §5) | **APPLIED (converted)** | The hunk's text describes the package as *pending* — false at the moment of application. Per the package's own apply checklist ("on apply: log the applied set in §5"), it was applied in **applied form**: the consolidated DOC-APPLY-1 §5 entry logs all five packages, their diff counts, variants chosen, and retirements. |
| R1-14 | **APPLIED — variant (a)** | R0's P2.5 landed first, as the stack order guarantees. |
| R1-15a/15b, R1-16a/16b (manifest), R1-17 (changelog) | **APPLIED** | Verbatim. 16b hand-edited (no regeneration); its `VibeGoal`-sweep note executed as part of D-DOC-A4(i) — all three stale manifest invariants swept, `yaml.safe_load` clean. |

### R2 + R2c + R2d (`RosterExpansion_R2_Doc_Diffs_2026-07-31.md`) — retired on apply

| Hunk | Status | Notes |
|---|---|---|
| R2-1/2/3 (boundary §8.4/§8.5/§8.6) | **APPLIED** | §8.3 runs to EOF in the live doc, so the three sections were appended in order at end of file — structurally identical to "after §8.3". Asset-name assumptions (`DefaultHarmonyPalette`, `Conito_CardCatalogData`) carried as written; the second is corroborated (package precondition 2), the first remains an assumption — flagged for correction if the authored name differs. Precondition 4 (SSoT_Editor_Authoring_Tools in PK) **now satisfied**; the `RandomFromList` pointer it asked for is delivered by AUTH1-1i. |
| R2-3b (integration §8 inv 9) | **APPLIED** | Inserted exactly between the BAL-1 paragraph and the Clear/restore paragraph, sibling indentation preserved. |
| R2-4 (integration §11) / R2-5a/b (authoring contracts §5.13) | **APPLIED** | Verbatim. `SSoT_Card_Authoring_Contracts.md` was not in the rehydration input list but is an R2 target present in the PK; applied and flagged. |
| R2-6 (Starter Deck v2 §3 rows 11/12 + built-note) | **APPLIED** | Effect cells replaced; built-note added under the §3 preamble. Emitted under the final name `Design_Starter_Deck_v2.md`. |
| R2-7a–e (sub-roadmap) | **APPLIED** | (a) R2 row status marker matched to R1's convention (`✅ CLOSED 2026-07-31`) + scope append. (c) The NEW block's items 1–3 replaced in place; its trailing "Filed and delivered / Items 4–5 / Registered" paragraphs placed **after** the existing items 4–5 (item 4 keeps R0-P2.7's narrowed text — the two are consistent). (d) R2/R2c/R2d ledger appended under §2 after the R0 ledger. (e) Conito coverage cells marked `✅ built`, reconciled over R0-P2.5's wording as the hunk instructs. |
| R2-8 (roadmap append) | **APPLIED** | With the D-SEQ-3=A amendment parenthetical on its "R4+ still gated on the S5j tag" claim (D-DOC-A3). |
| R2-9a (CURRENT_STATE §1 row) | **APPLIED** | Inserted at table top; CONT-B's row later inserted above it (both 07-31; CONT-B-over order mirrors the changelog). |
| R2-9b (§3 "No campaign code…" replacement) | **APPLIED (reconciled)** | **Task item 3 verification: yes, R2-9b is the hunk that covers the "No campaign code is in the build" drift** — but its literal anchor no longer existed after R1-11's block replacement. Reconciled by replacing R1-11's "**Next:** R2 …" bullet with R2-9b's full text (which records R1 closed + R2/R2c/R2d closed + R3 next + live front), and refreshing the block heading to "…R0, R1, R2 + R2c + R2d closed". The stale sentence is gone; the drift AUTH-1 §0.3 recorded is closed. |
| R2-9c (§4 open items ×4) / R2-9d | **APPLIED** / no-op (07-30 draft never applied — nothing to remove). |
| R2-10 (changelog) | **APPLIED (composed)** | The package specifies "a condensed form of DIFF-R2-9's row plus three facts". Composed accordingly — all content sourced from R2-9's row and the three enumerated facts; nothing invented. Dated 2026-07-31 per the FINAL package (contra the rehydration D2's assumed 07-30); placed below AUTH-1, above R1. |
| R2-11 (coverage-matrix smoke rows) | **DEFERRED — anchor does not exist** | `coverage-matrix.md` contains **no per-batch smoke-suite section**; it is a concept→authority matrix whose only ST-\* mentions are inline in Notes cells. Per the batch constraint ("si un anclaje no casa, PARAR y reportar; no improvisar la ubicación") the hunk was not applied. The full smoke record (ST-R2-\*/R2c/R2d statuses, ST-R2-1 supersession, ST-R2-3b deferral to R5) is preserved verbatim in the R2 changelog entry, so no information is lost. Disposition owed: decide whether the matrix grows a smoke-suite section (a structural change needing its own decision) or whether the changelog remains the smoke record of record. |
| §0.5 (Editor-Authoring-Tools pointer request) | **RETIRED by AUTH1-1i** | The `RandomFromList` cross-reference row now exists in `SSoT_Editor_Authoring_Tools.md` §15, applied after R2 so the §11 target section exists (no dangling reference; the DIFF-AUTH1-1i ordering gate was satisfied by the stack order — D1). |

### CONT-B (+CTX-1/1b) (`CONT-B_Doc_Diffs_2026-07-31.md`) — retired on apply

| Hunk | Status | Notes |
|---|---|---|
| 1.1 (§1 closed-batches row) | **APPLIED** | First row (above R2's). Spanish text verbatim (CURRENT_STATE is now bilingual in that table; the package specified the text). |
| 1.2 (§4 six findings + blocking note) | **APPLIED** | F-KIT-1/F-VEL-1/F-TEMPO-1/F-DNB-1/F-METAL-1/F-QUAL-1 verbatim at the top of Open items, with the F-KIT-1 blocking note as a blockquote directly after the block. |
| 1.3 (§2 listening bullet + retire the CSV-4 block-mark) | **APPLIED (half no-op)** | The new EJECUTADA bullet inserted at the top of §2. The "retirar la marca de bloqueo" half was a **no-op**: §2's live text was already flipped to UNBLOCKED by CSV-3 (2026-07-22); the only remaining "blocked — see §2" string sits inside the historical CSV-4 closed-batch row in §1, which the project convention says not to rewrite. Verified, nothing removed. |
| 1.4 (§5 queue) | **APPLIED (converted)** | Same conversion as R1-13: recorded as *applied*, in the consolidated DOC-APPLY-1 §5 entry, not as a pending queue. |
| 2 (changelog) | **APPLIED** | Verbatim, top of file (above AUTH-1 per D-DOC-A5). Its reference to `CONT-B_Returns_MidiGenPlay_2026-07-31.md` retained — see Input 7 / §6. |
| 3.1/3.2/3.3 (SSoT_Dev_Mode §18.12 / §9.17 / file list) | **APPLIED** | All three anchors were ANCLA VERIFICADA and matched. §18.12 after §18.11; §9.17 after §9.16's table; file-list line extended inline. |
| 4.1/4.2 (CSV sub-roadmap D-CSV-14 / D-CSV-18) | **APPLIED (format-adapted)** | Both anchors are **table rows**; the multi-paragraph blocks were flattened to single-line cell appends (markdown tables cannot hold paragraphs). Content verbatim, whitespace-only transformation. |
| 4.3 (CSV §5 three asks) | **APPLIED (labels applier-assigned)** | CONT-B §7 names no MGP-\* identifiers; three rows added with explicit `*(label applier-assigned)*` markers ("CONT-B ask 1/2/3 — …") and status "to file — via the returns doc (pending)". Real MGP-\* names to be assigned at filing. |
| 5 (SSoT_Audio §4.6 granularity limit) | **APPLIED — anchor CONFIRMED** | The ANCLA POR CONFIRMAR resolved positively: §4.6 (`MixGainProfileSO` / BAL-1) exists; subsection appended at its end, before §5. |
| 6.1 (D8 = LOCKED A) | **RECORDED** | Carried inside the CONT-B changelog entry + returns-doc content (deviation D8); no separate doc row needed. |
| 6.2–6.4 (D9/D10/D11) + 6.5 (D12) + §9b (CTX-2) | **APPLIED** | Registered in CURRENT_STATE §4 (task item 4; composed English registrations sourced strictly from CONT-B §6/§9b, options + recommendations transcribed). D12 additionally registered in `Design_Action_Economy_v1.md` §7 — that ANCLA POR CONFIRMAR also resolved positively (§7 "Deferred / debt" is the idiomatic home). |
| 7 (returns doc) | **NOT PRODUCED** | Per rehydration instruction. Open obligation (§6). |
| 8 (Design_Asset_Naming drum-naming record) | **APPLIED — anchor CONFIRMED + tension flagged** | Inserted at the end of §3 (per-family convention). **Governance note:** the applied CONT-B naming (palette prefix `FF_`/`SP_` + `8c` suffix) contradicts the doc's own draft schema (`Drum_<TS>_<Nm>_<Style>`) and §4 ("why the prefixes go"). A reconciliation-owed blockquote was added directly under the insert so the contradiction is explicit, not silent; resolution belongs to CSV-4b. |
| 9 (non-doc content queue) | **REGISTERED, not executed** | New "Pending content queue" block in CURRENT_STATE §3 (the home CONT-B itself designates), items 1–4 verbatim in substance, including the F-KIT-1 blocking flip on item 4 and the §9c cost notes. **No content or code was touched.** |
| 9c / §10 | **RESPECTED** | The `MGP-20260729_*` mirror was **not cited as truth** in any edit (declared obsolete by §9c.1). The §10 no-change list held: SSoT_INDEX, coverage-matrix, SSoT_Card_System, `SSoT_Runtime_CompositionSession_Integration` and the boundary doc received **no CONT-B edits** (their edits this session are R2's, which §10 does not cover). The §10 conditional (Runtime_CompositionSession §8, if the CTX-1b log identifies the reversion mechanism) is **not triggered** — no such log exists in the PK; recorded as a watch item. |

### AUTH-1 (+AUTH-1b) (`AUTH-1_Doc_Diffs_2026-07-31.md`) — retired on apply

| Hunk | Status | Notes |
|---|---|---|
| 1a (tool inventory) | **APPLIED (anchor variance)** | The diff's OLD sentence is a paraphrase; the live sentence reads "All six are `#if UNITY_EDITOR` gated `EditorWindow` subclasses. None ship in builds. …". Only the count word changed (six→seven); surrounding sentences preserved. Row inserted after the Composition Inventory row. |
| 1b–1h | **APPLIED** | §18 (incl. §18.8/§18.9 AUTH-1b), §19, §4.11, §5.8, §8.7, §13 file lines, §14.9 clarifier — all verbatim at their specified anchors. |
| 1i (§15 rows) | **APPLIED after R2** | Ordering gate satisfied by stack order; the §11 `RandomFromList` target exists. Retires R2 §0.5. |
| 2 (DeckEditor proposal Phase 7 note) | **APPLIED** | Appended at Phase 7's end. Doc stays planning-only. |
| 3 (CURRENT_STATE §2 bullet) | **APPLIED** | Below the CONT-B listening bullet (07-31 pair mirrors the changelog order). Its wording deliberately makes no campaign-doc-state claim; the §0.3 drift it recorded is closed by R2-9b in this same session. |
| 4 (changelog) | **APPLIED** | Below CONT-B, above R2 (D-DOC-A5). |
| "Not needed" checks | **VERIFIED** | Manifest: no section-level cached invariants exist for `SSoT_Editor_Authoring_Tools.md` beyond its entry — no invariants touch needed. Coverage-matrix: no rows added (consistent with the R2-11 deferral rationale). Appendix B content observations remain recorded in the changelog entry + retired package; each needs its own later batch. |

---

## 3. Cross-package reconciliations (applier judgment, all flagged above)

1. **CURRENT_STATE §3 stacking** (R0-P6 → R1-11 → R2-9b): three packages targeted the same block with mutually stale anchors. Resolved newest-subsumes-oldest; no recorded fact lost (details live in the per-batch changelog entries).
2. **D-SEQ-3=A propagation**: ledger amendment (authoritative) + two inline parentheticals on freshly-inserted S5j-gate claims, so no live doc contradicts the amended gate.
3. **SSoT_INDEX P4.2 status cell** updated to end-of-stack truth instead of R0-moment truth.
4. **Changelog same-date ordering**: CONT-B > AUTH-1 > R2 within 2026-07-31 (D-DOC-A5 + R2-FINAL date).
5. **Table-cell flattening** for CONT-B 4.1/4.2 (whitespace-only).
6. **R0-P8 queue home** fixed as CSV sub-roadmap §5; sub-roadmap §9 V4 pointer repointed off the retired diff file.

## 4. Packages retired (explicit, per convention D3)

Mark for retirement from the PK — all applied or explicitly dispositioned this session:

1. `RosterExpansion_R0_Doc_Diffs_2026-07-23.md` — applied (P2.9 partial, recorded in-doc).
2. `RosterExpansion_R1_Doc_Diffs_2026-07-23.md` — applied (variant (a); R1-13 converted).
3. `RosterExpansion_R2_Doc_Diffs_2026-07-31.md` — applied (R2-11 deferred with its record preserved in the changelog). *(The 07-30 draft was already superseded and never present.)*
4. `CONT-B_Doc_Diffs_2026-07-31.md` — applied. **Caution:** §7 is the only current source of the returns-doc content; produce `CONT-B_Returns_MidiGenPlay_2026-07-31.md` before physically deleting this file, or retire-by-archive rather than delete.
5. `AUTH-1_Doc_Diffs_2026-07-31.md` — applied.

The obsolete `MGP-20260729_*` mirror is **not** retired here (out of DOC-APPLY-1 scope); CONT-B §9c.1's re-export action stands as a cross-project follow-up.

## 5. Documents emitted (complete, downloadable — 27 + this report)

CURRENT_STATE.md · changelog-ssot.md · SSoT_INDEX.md · ssot_manifest.yaml · Roadmap_ALWTTT.md · RosterExpansion_Sub_Roadmap.md · Design_Track_Card_Levels_v0_1.md · **Design_Starter_Deck_v2.md** (renamed from `_DRAFT`, D5) · SSoT_Status_Effects.md · Design_Audience_Status_v1.md · SSoT_Audience_and_Reactions.md · SSoT_Scoring_and_Meters.md · SSoT_ALWTTT_MidiGenPlay_Boundary.md · SSoT_Runtime_CompositionSession_Integration.md · SSoT_Card_Authoring_Contracts.md · SSoT_Dev_Mode.md · CSV_Composition_Validation_Sub_Roadmap.md · SSoT_Audio.md · Design_Asset_Naming_v0_1.md · Design_Action_Economy_v1.md · SSoT_Editor_Authoring_Tools.md · ALWTTT_DeckEditorWindow_Roadmap_Proposal.md · SSoT_Gig_Combat_Core.md · SSoT_Gig_Encounter.md · Design_Starter_Deck_v1.md · ALWTTT_Combat_MVP_Audit_Final.md · coverage-matrix.md *(emitted unmodified — R2-11 deferred; included so the delivered set is the complete post-session baseline)*.

File placement on adoption: each file replaces its PK counterpart at its existing project path; `Design_Starter_Deck_v2.md` goes to `Docs/planning/active/` and `Design_Starter_Deck_v2_DRAFT.md` is deleted/renamed.

## 6. Open documentary obligations after DOC-APPLY-1

1. **`CONT-B_Returns_MidiGenPlay_2026-07-31.md` — produce and file** (cross-project, reference-only). Content spec: CONT-B §7 (6 deviations D3/D5/D5b/D6/D8/A3-confirmed + 3 asks + zero-warnings confirmation). Until it exists, the CONT-B changelog reference dangles and the CSV §5 "to file" rows have no vehicle. **Highest priority — gates package retirement #4.**
2. **R2-11 disposition** — decide whether `coverage-matrix.md` grows a per-batch smoke section (structural decision) or the changelog stays the smoke record of record.
3. **`Design_Asset_Naming_v0_1.md` §3 vs §4 reconciliation** — the applied CONT-B drum naming contradicts the unapplied draft schema; owned by CSV-4b.
4. **CSV-3-DOC** doc-pass — pre-existing, explicitly out of DOC-APPLY-1 scope (D-DOC-A4(ii)).
5. **CTX-1b watch item** — if the drift log identifies a per-loop card-intent reapplication, an undocumented runtime invariant exists and its home is `SSoT_Runtime_CompositionSession_Integration.md` §8 (CONT-B §10 conditional).
6. **Two unverified asset-name assumptions** carried into governed text: `DefaultHarmonyPalette` (R2) — correct in place if authored differently. (`Cantante_CardCatalogData`, `Conito_CardCatalogData`, `StatusEffectCatalogue_Audience` are corroborated.)
7. **D-CSV-14 formal closure** — CONT-B marks it "resolución probable, verificar antes de cerrar" (confirm the `Patterns/Melody` singular residue, then close; D-CSV-13 dissolves with it).
8. **MGP mirror re-export** (CONT-B §9c.1) — cross-project; not an ALWTTT doc edit.

## 7. Non-documentary work queue — registered, NOT executed

Registered in CURRENT_STATE §3 "Pending content queue (CONT-B)": (1) `Modal Drift` card → `Sibi_CardCatalogData` (D-INSP-6=A'; read CPE-META-1 D2 before reimporting `Prog_Maj_Ragtime_SECDOM`); (2) `FF_Metal8c` crash lightening; (3) `SP_DnB8c` re-author at `subdivisions = 8`; (4) `FF_LatinSon32_8c` **retire from palette** — F-KIT-1 is **BLOCKING** while it stays demo-reachable.

## 8. Constraints compliance

- Order gate: full stack applied in D1 order; AUTH1-1i after R2 (no dangling §11 reference; R2 §0.5 retired).
- No documentary content invented: two ambiguous/unavailable hunks (P2.9, R2-11) were reported/deferred, not filled.
- No planning material promoted to authority: R0 touched no runtime SSoT; the four Roster-Expansion runtime SSoT edits this session are R1/R2's own semantic diffs.
- Boundary rule: zero MidiGenPlay documents edited; CONT-B §7 asks registered ALWTTT-side as open requests; P8 filed as a queued correction, not applied cross-boundary.
- `MGP-20260729_*` mirror not cited in any edit.
- No code, no assets touched. S5i remains PARKED (D-SEQ-2=A); nothing here closes S5i or claims §5.4 passed.
- No smoke tests: documentation-only, zero behavior change.

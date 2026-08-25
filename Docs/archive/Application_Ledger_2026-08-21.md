# Islands.PCG — Documentation application ledger, 2026-08-21

Scope: two pending doc-update queues consumed — `W_b_Pending_Doc_Updates.md` (applied in
full) and `Phase_T2_Pending_Doc_Updates.md` (5 of 7 items applied, 2 blocked).
**Zero code changes. Zero tests run. Zero asset or preset changes.** Governed documents
edited: 6. Queues closed: 1 APPLIED, 1 PARTIALLY APPLIED.

This ledger is **evidence, not authority**. It records what this session did and on what
basis. Nothing in it supersedes a governed surface.

---

## 1. Evidence base — what was verified here, and what was not

### Verified in this session, by direct file read or by mechanical check

| Claim | How it was checked |
|---|---|
| The W.b **code surface** is present in the package | `MapGenerationPreset.cs` L112/117/189/194/207 (five promoted fields), L532-533 / L583-584 / L588 (`ToJson()` emission); `MapGenerationPresetJsonImporter.cs` L170-174 (three value keys); `MapGenerationPresetTests.cs` L149 (`Defaults_WbPromotedFields_MatchPrePromotionEffectiveValues`); `MapGenerationPresetJsonRoundTripTests.cs` L46/85/87 (non-default fixture); `MapGenerationPresetWizard.cs` L40-44 (`HelpBox` narrowed to `hydroEpsilon`); `PCGMapTilemapVisualization.cs` L371-374 / L1827-1831 / L1913-1917 (`last*` declarations, `CacheParams`, `ParamsChanged` by effective value) |
| The 2026-08-20 applications are still present in the governed files | `changelog-ssot.md` carries the five entries written that day (W-aux.g / W-aux.f / W-aux.e / W-aux.d / X1.a); `PCG_Roadmap.md` carries Phase X1 in both the status snapshot and its own section; `SSoT_CONTRACTS.md` carries "Shadow defaults in test fixtures (W-aux.f)" — the retargeted §7 recorded in the 2026-08-20 ledger §3; `CURRENT_STATE.md` status date is 2026-08-20; `coverage-matrix.md` carries the X1 rows and the declared gaps |
| `Phase_T2_Pending_Doc_Updates.md` is in project knowledge | read in full, 266 lines |
| Every anchor of both queues, before substitution | 19 anchors located; occurrence counts recorded in §2 |
| Every substitution, after application | for each item: REPLACE / INSERTED TEXT present exactly once; SEARCH absent, except where the queue's own REPLACE deliberately retains the SEARCH text as its first or last line (§2.2a, §2.2b, §3.1, §3.2 of W.b) — for those the expected surviving count is 1 and was 1 |
| The substantive claim of W.b §4.3 item 3 | `PCGMapVisualization.cs` L442-444 + L452-461 and `PCGMapCompositeVisualization.cs` L397-399 + L407-416 assign `enableBiomeStage` and the biome climate fields from inline component fields, with no `preset != null ?` ternary. Claim holds; its numbers did not — see §3 |
| `Phase_T2_Design.md` does not exist | absent from the package file listing and from project knowledge |

### Carried forward WITHOUT verification in this session

| Claim | Source | Why not verified here |
|---|---|---|
| The W.b EditMode suite is **green** | user confirmation, 2026-08-21 (and previously 2026-08-20) | this session has no Unity and ran no tests. The *code* was verified present; *green* is user confirmation only |
| W.b console goldens at seed 243 res 256 identical before/after | user confirmation, 2026-08-20, restated in the W.b queue's Evidence section | same reason. This claim is now written into `changelog-ssot.md` §W.b as user confirmation, and is labelled as such there |
| Everything the 2026-08-20 ledger says about code state | that ledger, which itself declares its code claims carried from its own baseline | not re-derived. Spot-checks above cover the *document* applications only |

---

## 2. Anchor check — all 19 anchors, before substitution

No substitution was made before this table was complete.

### `W_b_Pending_Doc_Updates.md` — 15 edits

| Item | Target | Anchor kind | Occurrences | Result |
|---|---|---|---|---|
| §1.1 `MapTunables2D` claim correction | `PCG_Roadmap.md` | SEARCH | 1 | applied |
| §1.2 close W.b (heading) | `PCG_Roadmap.md` | SEARCH | 1 | applied |
| §1.2 outcome block | `PCG_Roadmap.md` | INSERT AFTER | 1 | applied |
| §1.3 status line (pair 1) | `PCG_Roadmap.md` | SEARCH | 1 | applied |
| §1.3 status line (pair 2) | `PCG_Roadmap.md` | SEARCH | 1 | applied |
| §2.1 N5.b promoted surface | `map-pipeline-by-layers-ssot.md` | INSERT BEFORE | 1 | applied |
| §2.2 M2a-9 clause (a) | `map-pipeline-by-layers-ssot.md` | SEARCH | 1 | applied |
| §2.2 M2a-9 clause (d-bis) | `map-pipeline-by-layers-ssot.md` | SEARCH | 1 | applied |
| §2.3 hydrology authoring coupling | `map-pipeline-by-layers-ssot.md` | **prose only** | — | anchor resolved, see below |
| §3.1 additive schema extension | `SSoT_CONTRACTS.md` | SEARCH | 1 | applied |
| §3.2 M2.a verdict table | `SSoT_CONTRACTS.md` | SEARCH | 1 | applied |
| §4.1 close `moistureModulation` | `CURRENT_STATE.md` | SEARCH | 1 | applied |
| §4.2 close `waterThreshold01` | `CURRENT_STATE.md` | SEARCH | 1 | applied |
| §4.3 three new observations | `CURRENT_STATE.md` | **prose only** | — | anchor resolved, see below; **applied with a correction, see §3** |
| §5 changelog entry | `changelog-ssot.md` | **prose only** | — | anchor resolved, see below |

**Prose-described insertion points, resolved to unique literal anchors.** Each was checked
for uniqueness before use. Recorded here because the queue does not contain them, so a
future rollback needs this table:

- §2.3 → INSERT AFTER `- \`MapPipelineRunner2DGoldenLMTests.cs\` — F0→G→L→M pipeline golden captured`
  (last line of the Phase L §Golden coverage block, immediately before `### Phase M2.b`). 1 occurrence.
- §4.3 → INSERT AFTER `  \`StageBaseTerrain2DTests.NoShapeTunables()\`. Never confirmed.`
  (last bullet of §Open observations, immediately before `## Measured calibration baselines and climate reachability`). 1 occurrence.
- §5 → INSERT BEFORE `## W-aux.g — Hills window recalibration (area-quantile thresholds, F3b′)`
  (the file is newest-first; W-aux.g was the head entry). 1 occurrence.

### `Phase_T2_Pending_Doc_Updates.md` — 5 applied, 2 blocked

| Item | Target | Anchor kind | Occurrences | Result |
|---|---|---|---|---|
| §1.1 Phase T2 branch | `PCG_Roadmap.md` | INSERT BEFORE | 1 | applied |
| §1.2 status snapshot | `PCG_Roadmap.md` | SEARCH | 1 | applied |
| §1.3 annotate Phase T1 | `PCG_Roadmap.md` | SEARCH | 1 | applied |
| §1.4 design-doc table row | `PCG_Roadmap.md` | SEARCH | 1 | **NOT APPLIED — conditional** |
| §2.1 adapter-track sentence | `CURRENT_STATE.md` | SEARCH | **0** | **applied after re-anchor, see §3** |
| §2.2 deferred list | `CURRENT_STATE.md` | SEARCH | 1 | applied |
| §3 index entry | `SSoT_INDEX.md` | SEARCH | 1 | **NOT APPLIED — conditional** |

No collision between the two queues. W.b §1.3 pair 1 edits `PCG_Roadmap.md` L127-129 and
T2 §1.2 edits L126 — adjacent lines, disjoint strings; neither substitution destroys the
other's anchor, in either order.

---

## 3. Deviations from queue text — two, both by user decision

### 3.1 — W.b §4.3: source references corrected before insertion

The queue's third observation cited `PCGMapVisualization` L445–454,
`PCGMapCompositeVisualization` L405–414, and "the twelve `biome*` climate fields". Read
against the files this session, the climate assignment blocks are at **L452–461** and
**L407–416**, and there are **ten** such assignments, not twelve.

Applied text was corrected on both counts (**user decision, 2026-08-21**). The claim itself
— neither component resolves biome climate from the preset, so the same preset yields
different climate on different components — was verified and is unchanged.

Why this was corrected rather than applied verbatim: `CURRENT_STATE.md` is the operational
present tense, and this observation is a pointer to code someone will open. A pointer that
is off by seven lines and wrong about the count sends the reader to the wrong place and
makes them doubt the claim that *was* right. The correction is of the same class as the
verification that produced it, not a rewrite of the queue's argument.

### 3.2 — T2 §2.1: re-anchored

The queue's SEARCH matched **zero** times. Cause: `CURRENT_STATE.md` wraps the sentence
after `remain`, the queue wraps it after `phases`. Content identical, break point different.

Under the no-approximation rule the item was stopped and reported rather than fuzzy-matched.
**User approved the re-anchor**; the corrected pair (anchored on
`distilled from observing W-generated worlds. Adapter-track phases T1 and Q2 remain`) matched
exactly once and was applied. The semantic change is exactly the one the item asked for:
`T1 and Q2` → `T1, T2 and Q2`. The re-anchor is recorded in place inside the queue.

---

## 4. Per-queue final state

### `W_b_Pending_Doc_Updates.md` — **APPLIED. Closed; ready to archive.**

15 of 15 edits applied across five governed documents. No blocker. Precondition confirmed
by the user before applying: W.b code applied and suite green.

| Item | Target | Disposition |
|---|---|---|
| §1.1 | `PCG_Roadmap.md` | applied — planning claim about `MapTunables2D` superseded by implementation evidence |
| §1.2 | `PCG_Roadmap.md` | applied — heading closed, outcome block inserted |
| §1.3 | `PCG_Roadmap.md` | applied — both status lines now read W.b closed, next W-aux.h |
| §2.1 | `map-pipeline-by-layers-ssot.md` | applied — `MapGenerationPreset` registered in §Configuration Assets (N5.b) |
| §2.2 | `map-pipeline-by-layers-ssot.md` | applied — M2a-9 (a) conditioned, (d-bis) added |
| §2.3 | `map-pipeline-by-layers-ssot.md` | applied at the end of the Phase L block — the `riverThresholdFraction` / `biomeRiverFlowNorm = 0` asymmetry is now recorded, deliberately not fixed |
| §3.1 | `SSoT_CONTRACTS.md` | applied — additive key extension declared not a serialization break |
| §3.2 | `SSoT_CONTRACTS.md` | applied — seven-row M2.a promotion verdict table |
| §4.1 | `CURRENT_STATE.md` | applied — `moistureModulation` observation closed |
| §4.2 | `CURRENT_STATE.md` | applied — `waterThreshold01` observation closed |
| §4.3 | `CURRENT_STATE.md` | applied **with the §3.1 correction** — three new observations |
| §5 | `changelog-ssot.md` | applied — W.b entry inserted as the head entry |

### `Phase_T2_Pending_Doc_Updates.md` — **PARTIALLY APPLIED. LIVE.**

| Item | Target | Disposition |
|---|---|---|
| §1.1 | `PCG_Roadmap.md` | applied — Phase T2 branch inserted before `### Phase I` |
| §1.2 | `PCG_Roadmap.md` | applied — snapshot carries T1 (cross-referenced) and T2 |
| §1.3 | `PCG_Roadmap.md` | applied — Phase T1 annotated with the open decision |
| §1.4 | `PCG_Roadmap.md` | **BLOCKED — `Phase_T2_Design.md` does not exist.** Anchor verified unique and ready |
| §2.1 | `CURRENT_STATE.md` | applied **after re-anchor**, see §3.2 |
| §2.2 | `CURRENT_STATE.md` | applied — T2 added to the deferred/optional list |
| §3 | `SSoT_INDEX.md` | **BLOCKED — same reason. Also needs re-anchoring**: this session edited the very lines its SEARCH targets |
| §4 | — | **RESOLVED by user decision: `research/`.** The file move is a package operation and was not performed here |
| §5 | — | no changelog / coverage-matrix / supersession entry, per the queue's own reasoning. Concurred: adding a planning branch changes neither semantics nor authority |

**Blocker restated, per the queue header:** do not archive this queue. §1.4 and §3 exist
nowhere else, and both become applicable the moment `Phase_T2_Design.md` is written.

---

## 5. Governed documents edited this session

| Document | Edits | Source |
|---|---|---|
| `PCG_Roadmap.md` | 8 | W.b §1.1, §1.2 ×2, §1.3 ×2; T2 §1.1, §1.2, §1.3 |
| `CURRENT_STATE.md` | 5 | W.b §4.1, §4.2, §4.3; T2 §2.1, §2.2 |
| `map-pipeline-by-layers-ssot.md` | 4 | W.b §2.1, §2.2 ×2, §2.3 |
| `SSoT_CONTRACTS.md` | 2 | W.b §3.1, §3.2 |
| `changelog-ssot.md` | 1 | W.b §5 |
| `SSoT_INDEX.md` | 2 | **not queue-derived — authored this session**, see §6 |

Plus the two queue files themselves (status headers and in-place disposition notes).

### Short local update loop — how each concept was walked

- **W.b parameter surface.** Primary home per `coverage-matrix.md` is the pipeline SSoT →
  `map-pipeline-by-layers-ssot.md` §Configuration Assets and §M2a-9 first (step 3);
  `CURRENT_STATE.md` next, since active status and open observations changed (step 4);
  `changelog-ssot.md`, since a contract clause and the serialization posture changed
  (step 5); cross-cutting rules to `SSoT_CONTRACTS.md`. No document was replaced or
  absorbed, so no `supersession-map.md` entry (step 6); this is not a salvage pass, so no
  `migration-log.md` entry (step 7).
- **Phase T2 branch.** Planning only. Primary home is the roadmap; `CURRENT_STATE.md`
  touched solely for adapter-track *status*, not for implementation claims. Steps 5-7
  deliberately not taken, per T2 §5 — a planning branch changes neither semantics nor
  authority.

---

## 6. Index edits are mine, not a queue's

`SSoT_INDEX.md` received two edits authored in this session, not carried from any queue:
registering `Phase_T2_Pending_Doc_Updates.md` as live, archiving
`W_b_Pending_Doc_Updates.md`, listing this ledger, and recording a **registration gap** —
both queues consumed today existed, were live, and appeared in neither index list.

That gap is the finding worth keeping. A pending queue is unapplied governed content: if
it is not registered, the next session cannot know it is owed. The index instruction added
is that queues get registered when *written*, not when consumed. These edits carry no
queue's authority behind them and should be reviewed as new drafting.

---

## 7. Still blocked, unchanged from 2026-08-20

| Item | Blocker | What would unblock it |
|---|---|---|
| `Phase_Q_Design.md` §3.1 — starter asset naming and path | open decision `Phase_Q_Pending_Doc_Updates.md` §9.1 | a user decision. Not raised this session |
| `changelog-ssot.md` — W.a world-scale golden | the hash values exist in no governed file, test constant or captured log | re-running the seed 56 @ 64×64 world-preset capture. Code work, out of scope for a documentation session |
| `Phase_T2_Pending_Doc_Updates.md` §1.4 and §3 | `Phase_T2_Design.md` does not exist | writing that design document |

Neither Q nor W was touched. Both remain live with their blockers intact.

---

## 8. Reported, not fixed

- **`coverage-matrix.md` L57 contradicts `SSoT_INDEX.md`.** The row for the
  W-aux.c…W-aux.f / X1.a application record locates the six queues "under
  `planning/active/`" with status "Active until archived", while the index says they were
  moved to `planning/archive/` on 2026-08-20. One of the two governed spine documents is
  stale. Not corrected here: outside both queues, and the fix depends on where the files
  actually are on disk, which this session cannot see. A row for the W.b and T2 queues is
  also missing.
- **`changelog-ssot.md` carries a duplicate empty heading.** `## Phase F3b —
  Height-Coherent Hills (Clean Break)` / `Date: 2026-04-08` appears twice in immediate
  succession, the first with no body. Pre-existing, unrelated to these queues, harmless to
  the W.b insertion (which went in at the head of the file).
- **Corner-height research artifact.** Filed to `research/` by user decision. The user
  notes it is the basis for the T2.3 implementation. Influence is not authority: the model
  becomes implementation authority only when restated in `Phase_T2_Design.md` §T2.3 as this
  package's own decisions, with the artifact cited as source. Recorded in the queue at §4.
- Everything in the 2026-08-20 ledger §6 (`Stage_BaseTerrain2D` parity, wizard test gap,
  `Vegetation` runtime golden gap, `Default_MapPreset` @256 `Height` goldens not recaptured)
  remains open. None of it is documentation work.

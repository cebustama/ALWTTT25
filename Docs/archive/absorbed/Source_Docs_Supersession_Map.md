# Source_Docs_Supersession_Map — ALWTTT Governance Migration

**Status:** Active migration trace doc  
**Doc Type:** Archive / redirect map  
**Authority:** Historical  
**Purpose:** Record where important pre-governance ALWTTT docs now point inside the governed documentation system.

---

## 1. Why this file exists

The current governed docs tree does **not** keep every old source doc in-place.

That is intentional: the old material was preserved separately as snapshot/source input during migration.

This file gives the durable answer to:
- where an older doc's authority went
- which governed doc replaced it
- whether the old doc is still useful as reference, planning, or archive only

---

## 2. Supersession map

| Previous source doc | Previous role | Governed home now | Status now | Notes |
|---|---|---|---|---|
| `canon/SSoT_Combat.md` | combat delta snapshot | `CURRENT_STATE.md` + `systems/SSoT_Gig_Combat_Core.md` | superseded snapshot | keep only as migration evidence |
| `canon/Roadmap_Combat.md` | combat roadmap | `planning/active/Roadmap_Combat_MVP.md` | planning-only | old path no longer primary |
| `canon/Appendix_Authoring_DataContracts.md` | card authoring contracts | `systems/SSoT_Card_Authoring_Contracts.md` | absorbed into SSoT | tooling/import truth promoted |
| `reference/Card.md` | mixed card design/runtime truth | `systems/SSoT_Card_System.md` | superseded by SSoT | current gameplay semantics live in systems/ |
| `reference/Gig_Combat.md` | mixed combat spec | `systems/SSoT_Gig_Combat_Core.md` | superseded by SSoT | combat economy/phase truth promoted |
| `reference/Gig.md` | encounter structure | `systems/SSoT_Gig_Encounter.md` | superseded by SSoT | encounter identity now has its own home |
| `reference/AudienceMember.md` | audience entity/reaction truth | `systems/SSoT_Audience_and_Reactions.md` | superseded by SSoT | audience contracts promoted |
| `reference/StatusEffects.md` | status runtime truth | `systems/SSoT_Status_Effects.md` | superseded by SSoT | status meaning promoted |
| `reference/StatusEffects_Primitives_with_References.md` | CSO primitive catalog | `reference/CSO_Primitives_Catalog.md` | retained as reference | support catalog, not primary authority |
| `backlog/ideas/loopscore_songhype_vibe.md` | scoring semantics | `systems/SSoT_Scoring_and_Meters.md` | superseded by SSoT | scoring pipeline promoted |
| `backlog/ideas/gig_pipeline_summary.md` | runtime gig-flow summary | `runtime/SSoT_Runtime_Flow.md` | superseded by runtime SSoT | useful as migration source only |
| `reference/subsystems/Composition Pipeline/SSoT_Runtime_CompositionSession_Bridge.md` | ALWTTT runtime bridge | `runtime/SSoT_Runtime_CompositionSession_Integration.md` | superseded by runtime SSoT | ALWTTT-side runtime truth promoted |
| `reference/subsystems/Composition Pipeline/SSoT_CompositionCards_TrackStyleBundles.md` | mixed ALWTTT/package doc | `systems/SSoT_Card_System.md` + `runtime/SSoT_Runtime_CompositionSession_Integration.md` + `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md` | split | one doc was doing too many jobs |
| `reference/subsystems/Composition Pipeline/SSoT_CompositionAuthoringTools.md` | package-heavy tools doc | MidiGenPlay docs + ALWTTT boundary docs | external/reference only | ALWTTT must reference, not redefine |
| `reference/subsystems/Composition Pipeline/SSoT_Composer_BackingChordTrack.md` | package composer internals | MidiGenPlay docs | external authority | no ALWTTT primary home |
| `reference/subsystems/Composition Pipeline/SSoT_Composer_RhythmTrack.md` | package composer internals | MidiGenPlay docs | external authority | no ALWTTT primary home |
| `archive/ALWTTT_MidiGenPlay_TS_Normalization_Roadmap.md` | historical cross-project planning | MidiGenPlay history / ALWTTT archive context | historical only | not current package or game authority |

---

## 3. Snapshot rule

The pre-governance snapshot remains useful as a historical backup, but it is no longer needed as a routine lookup surface.

For snapshot policy, use:
- `archive/SNAPSHOT_RETENTION_POLICY.md`

For old->new path lookup, use this file.

---

## 4. Operational use

If someone arrives with an old ALWTTT doc name:
1. look it up here
2. jump to the governed home
3. only return to the old source snapshot if historical comparison is needed

This keeps the migration safe without allowing the previous tree to remain silent authority.

---

## 5. Remaining cleanup rule

If a future batch absorbs another old source doc, update this file and:
- `SSoT_INDEX.md`
- `coverage-matrix.md` if a primary home changed
- `changelog-ssot.md` if the authority map changed semantically

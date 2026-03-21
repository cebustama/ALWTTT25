# changelog-ssot — ALWTTT

This changelog records **semantic/documentary changes**.
Cosmetic edits should not be logged here.

---

## 2026-03-18 — Governance migration Batch 01 initialized

### Added
- root governed docs spine:
  - `README.md`
  - `SSoT_INDEX.md`
  - `SSoT_CONTRACTS.md`
  - `CURRENT_STATE.md`
  - `coverage-matrix.md`
  - `changelog-ssot.md`

### Established documentary rules
- one major concept must have one primary home
- planning/reference/archive cannot silently override SSoTs
- `CURRENT_STATE.md` is a live state layer, not a replacement for subsystem SSoTs

### Established cross-project boundary rule
- ALWTTT owns gameplay/runtime/integration truth
- MidiGenPlay owns package internals
- shared concepts must be split into boundary contracts rather than duplicated authority

### Key classification decision
- `MidiMusicManager` is to be documented as **ALWTTT runtime/integration truth**, not as MidiGenPlay package truth

### Migration impact
- future subsystem batches now have a stable governed target
- current previous docs remain source material until subsystem SSoTs are promoted

## 2026-03-18 — Governance migration Batch 02 promoted first subsystem SSoTs

### Added
- `systems/README.md`
- `systems/SSoT_Gig_Combat_Core.md`
- `systems/SSoT_Card_System.md`
- `systems/SSoT_Card_Authoring_Contracts.md`
- `planning/README.md`
- `planning/combat/Combat_MVP_Roadmap.md`

### Promoted / reclassified
- previous combat truth was consolidated into `systems/SSoT_Gig_Combat_Core.md`
- previous card truth was consolidated into `systems/SSoT_Card_System.md`
- previous appendix/data-contract truth was promoted into `systems/SSoT_Card_Authoring_Contracts.md`
- previous combat roadmap was reclassified as planning-only

### Key documentary decisions
- combat economy/phase/resource truth now has a single governed home
- card gameplay semantics now has a single governed home
- authoring/import contracts were split cleanly from gameplay/runtime semantics
- the effect-first card model is treated as the current primary card model
- legacy `CardData`-style material remains a documented risk, not silent authority

### Migration impact
- ALWTTT now has its first real subsystem SSoTs instead of only root governance docs
- the next migration can focus on runtime/music integration without reopening combat/card authority

## 2026-03-19 — Governance migration Batch 03 promoted runtime and music-integration authority

### Added
- `runtime/README.md`
- `runtime/SSoT_Runtime_Flow.md`
- `runtime/SSoT_Runtime_CompositionSession_Integration.md`
- `integrations/README.md`
- `integrations/midigenplay/README.md`
- `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md`
- `integrations/midigenplay/SSoT_ALWTTT_MidiMusicManager_Integration.md`

### Promoted / reclassified
- previous gig/runtime-flow truth was consolidated into `runtime/SSoT_Runtime_Flow.md`
- previous ALWTTT-owned runtime bridge truth was consolidated into `runtime/SSoT_Runtime_CompositionSession_Integration.md`
- previous mixed composition-pipeline docs were formally split into:
  - ALWTTT runtime truth
  - ALWTTT ↔ MidiGenPlay boundary truth
  - package-owned material that must be referenced rather than duplicated

### Key documentary decisions
- `GigManager`, `CompositionSession`, `SongConfigBuilder`, and loop/part/song feedback were promoted as ALWTTT runtime authority surface
- `MidiMusicManager` now has an explicit governed home as ALWTTT runtime/integration truth
- composition-card runtime meaning was separated from package-internal composer/generation details
- the ALWTTT ↔ MidiGenPlay ownership split is now explicit rather than implied by mixed docs

### Migration impact
- ALWTTT no longer depends on mixed composition pipeline docs as silent primary authority
- the next migration can focus on audience/status/scoring without reopening the ALWTTT vs MidiGenPlay boundary question

## 2026-03-19 — Hardening micro-pass for ALWTTT ↔ MidiGenPlay boundary

### Modified
- `runtime/SSoT_Runtime_CompositionSession_Integration.md`
- `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md`
- `integrations/midigenplay/README.md`

### Added
- `integrations/midigenplay/ALWTTT_Uses_MidiGenPlay_Quick_Path.md`

### Key hardening decisions
- made the **source-of-truth split** explicit:
  - ALWTTT owns editable/session truth before handoff
  - MidiGenPlay owns package-side runtime song truth after handoff
- clarified that these are complementary layers rather than competing authorities
- added a one-page quick-path guide so a developer can understand the end-to-end integration flow without reading multiple long docs first

## 2026-03-19 — Governance migration Batch 04 promoted audience, status, and scoring authority

### Added
- `systems/SSoT_Audience_and_Reactions.md`
- `systems/SSoT_Status_Effects.md`
- `systems/SSoT_Scoring_and_Meters.md`
- `reference/README.md`
- `reference/CSO_Primitives_Catalog.md`

### Promoted / reclassified
- previous audience truth from `reference/AudienceMember.md` was promoted into `systems/SSoT_Audience_and_Reactions.md`
- previous status-runtime truth from `reference/StatusEffects.md` was promoted into `systems/SSoT_Status_Effects.md`
- previous scoring semantics from `backlog/ideas/loopscore_songhype_vibe.md` were promoted into `systems/SSoT_Scoring_and_Meters.md`
- the broader CSO primitive catalog was retained as reference rather than as primary runtime authority

### Key documentary decisions
- audience, status, and scoring now have separate primary homes
- persuasion progress was kept distinct from song momentum
- status ontology/reference material was separated from live runtime status truth
- the reference catalog was allowed to stay broad without competing with the live status SSoT

### Migration impact
- ALWTTT no longer needed reference/backlog docs as silent authority for audience/status/scoring
- the governed tree gained complete gameplay-facing subsystem coverage

## 2026-03-19 — Governance migration Batch 05 promoted encounter structure and cleanup traceability

### Added
- `systems/SSoT_Gig_Encounter.md`
- `archive/README.md`
- `archive/absorbed/README.md`
- `archive/absorbed/Source_Docs_Supersession_Map.md`

### Promoted / reclassified
- previous encounter-level truth from `reference/Gig.md` was promoted into `systems/SSoT_Gig_Encounter.md`
- the old source-doc set is now explicitly treated as snapshot/trace material rather than a silent second docs tree
- a durable supersession map now records where pre-governance doc names point in the governed system

### Key documentary decisions
- encounter structure is now separated from both combat economy and runtime execution
- `Gig` now has a governed home for:
  - roster framing
  - song-count structure
  - gig-scoped state
  - victory/failure conditions
  - encounter modifiers
- cleanup/redirect handling was moved into `archive/absorbed/` instead of leaving old names implied only through chat history
- the current governed docs tree can now answer “where did this old doc go?” without re-opening the snapshot manually

### Migration impact
- ALWTTT no longer has a missing primary home for encounter-level truth
- the remaining migration work became normalization/final replacement rather than creation of major new subsystem authorities

## 2026-03-19 — Governance migration Batch 06 normalized the final tree and closed the snapshot migration

### Added
- `planning/active/README.md`
- `planning/archive/README.md`
- `planning/active/Roadmap_Combat_MVP.md`
- `planning/combat/README.md`
- `reference/README.md` (restored in the normalized tree)
- `reference/CSO_Primitives_Catalog.md` (restored in the normalized tree)
- `archive/snapshots/README.md`
- `archive/SNAPSHOT_RETENTION_POLICY.md`

### Restored / aligned
- `systems/SSoT_Audience_and_Reactions.md`
- `systems/SSoT_Status_Effects.md`
- `systems/SSoT_Scoring_and_Meters.md`

### Modified
- `README.md`
- `SSoT_INDEX.md`
- `CURRENT_STATE.md`
- `coverage-matrix.md`
- `archive/README.md`
- `archive/absorbed/Source_Docs_Supersession_Map.md`

### Key documentary decisions
- the actual tree was brought back into alignment with the root governance docs
- the active roadmap home is now explicitly `planning/active/`
- the previous combat-roadmap path is now only a compatibility pointer
- the pre-governance snapshot is kept as historical backup, not as active authority
- the migration is now functionally complete and replacement-ready

### Migration impact
- ALWTTT now has a coherent governed docs tree that can replace routine use of the snapshot
- remaining work after this batch is ordinary documentation maintenance, not migration

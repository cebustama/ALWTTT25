# changelog-ssot — ALWTTT

This changelog records **semantic/documentary changes**.
Cosmetic edits should not be logged here.

---

## 2026-04-14 — M1.2 closure: Status icon pipeline SO migration + refactor

### Semantic changes

**SSoT_Status_Effects:**
- §2.2 updated: "authored/theme layer" now explicitly includes icon sprite presentation.
- §3 runtime ownership expanded to include icon presentation (sprite authority on StatusEffectSO, event-driven rendering on CharacterCanvas).
- §3.2 migration note updated: M1.2 icon pipeline closure recorded. Legacy icon calls removed from `MusicianBase.OnBreakdown` and `AudienceCharacterBase.IsBlocked` setter (Decision E3: blocked is sprite-tint only).
- §3.3 added: icon presentation authority specification. Sprite authority on `StatusEffectSO.IconSprite`. Rendering path documented end-to-end. Design decisions enumerated (no lookup table asset, direct prefab on canvas, lazy lifecycle, warning on missing sprite/prefab, tooltip content deferred to M1.3).
- §4 catalogue section: M1.2 catalogue validation fix documented (`delayCall` deferral + import-worker skip). M1.2 asset hygiene documented (auto-rename).
- §10 update rule: icon presentation authority added to the list of changes that require updating this SSoT.

**CURRENT_STATE:**
- §1 Project foundation: new "Status icon pipeline — SO-based" block added with M1.2 closure summary.
- §2 Active work: "Status Icons + Tooltip pipeline — audit needed" block removed (M1.2 closed). New "QA-readiness gap" block added describing the actual blocker (Dev Mode, not icons). New "Card effect description text — known bug" item captured.
- §3 What is next: reframed around critical path to QA-readiness. M1.5 Dev Mode, M1.7 hover highlight, M1.8 icon animations, M1.3 tooltips, M1.1 deck editor polish, M2 composition validation.
- §4 Residual risks: "StatusIconsData uses legacy enum" removed (fixed). "Multi-turn status validation pending" added (deferred to M1.5).
- §5 Pending docs: "Archive headers" removed (M1.6 already closed per prior changelog). "Dev Mode design SSoT" added as the next expected doc.

**Roadmap_ALWTTT:**
- Milestone 1 introduction revised: goal and demo pitch reframed around "testable by general-audience testers."
- New "Priority order" block added at top of M1 establishing critical-path reordering: M1.5 → M1.7 → M1.8 → M1.3 → M1.1. Reasoning recorded.
- M1.2 marked complete with outcome-beyond-scope notes (StatusIconsData removed, auto-rename, catalogue validation fix, Obsolete marker on legacy ApplyStatus).
- M1.3 scope expanded to explicitly include the card effect description text bug fix.
- M1.5 scope expanded: transparent audience reaction/ability display. Design questions added for session start. Proposal to start M1.5 with a detailed sub-roadmap session.
- M1.7 Character hover highlight added as new task (code-only, game feel).
- M1.8 Status icon animations added as new task (code-only, game feel).
- M1.6 marked complete (closed 2026-04-08; was already done but not marked).
- Definition of Done updated: M1.2 items checked. New items added for M1.7, M1.8, M1.3 text-fix, and M1.5 audience-reaction transparency.
- M2 demo pitch: added explicit "Unblocked by: M1.5 Dev Mode" note.

### Authority changes

**Icon pipeline authority clarified:**
- Sprite authority: `StatusEffectSO.IconSprite` (owned by SSoT_Status_Effects §3.3).
- Rendering path authority: `CharacterCanvas` event subscription to `StatusEffectContainer` (owned by SSoT_Status_Effects §3.3).
- The former `StatusIconsData` lookup asset is deleted. No longer an authority-bearing file.

### Structural changes

**Removed files:**
- `Assets/Scripts/Data/UI/StatusIconsData.cs` — the lookup table class is deleted. `StatusIconsData` and `StatusIconData` types no longer exist.
- `StatusIconsData` asset instance in the project — removed from musician/audience prefab references.

**Modified files (code):**
- `StatusEffectSO.cs` — added `iconSprite` field + `IconSprite` property under new `[Header("Presentation")]`. Added editor-only auto-rename via `EditorApplication.delayCall`. Rename format: `StatusEffect_{DisplayName}_{EffectId}`.
- `StatusEffectCatalogueSO.cs` — `OnValidate` now defers real validation to `delayCall` and skips entirely during `AssetDatabase.IsAssetImportWorkerProcess()`. Fixes spurious "empty StatusKey" errors on prefab selection.
- `StatusIconBase.cs` — `SetStatus` signature changed from `(StatusIconData)` to `(Sprite)`. `MyStatusIconData` property removed, replaced with `CurrentSprite`.
- `CharacterCanvas.cs` — `statusIconsData` field removed. `statusIconBasePrefab` direct field added under new `[Header("Status Icons")]`. `TryCreateIcon` reads sprite from `_boundContainer.TryGet(id).Definition.IconSprite`. Keyword-based tooltip iteration stripped; `ShowTooltipInfo()` is a stub pending M1.3. Public `ShowTooltipInfo`/`HideTooltipInfo` methods preserved for `ITooltipTargetBase` compliance, corrected to call `TooltipManager.ShowTooltip`/`HideTooltip` (the real API). `BindStatusContainer` subscribes to `OnStatusApplied`/`OnStatusChanged`/`OnStatusCleared`.
- `CharacterStats.cs` — icon delegate subscriptions in `Setup()` and `Dispose()` removed. Legacy status dict and turn triggers retained (they drive non-icon legacy behavior only).
- `MusicianBase.cs` — `OnBreakdown` no longer calls `stats.ApplyStatus(StatusType.Breakdown, 1)`. `BuildCharacter` calls `bandCharacterCanvas.BindStatusContainer(Statuses)`.
- `AudienceCharacterBase.cs` — `IsBlocked` setter no longer calls `stats.ApplyStatus/ClearStatus(StatusType.Blocked)`. Sprite tint is the only visual indicator (Decision E3). `BuildCharacter` calls `AudienceCharacterCanvas.BindStatusContainer(Statuses)`.
- `BandCharacterStats.cs` — `ApplyStatus(StatusType, int)` marked `[Obsolete]`. No behavior change.

### Lifecycle

- M1.2 (Status Icons pipeline migration) closed.
- M1.6 (Archive superseded planning docs) retroactively marked closed in the roadmap (was already done but unmarked).
- Multi-turn status smoke tests (T4, T5, T7, T8, T9 from the M1.2 test plan) deferred to M1.5 closure — they require infinite-turn tooling that does not yet exist.
- M1.5 elevated to critical-path priority. The build is technically complete but not QA-ready without it.
- Card effect description text bug (`CharacterStatusId` enum names showing instead of `StatusEffectSO.DisplayName`) added to M1.3 scope.

### Operational changes

- Project instructions gain a new "Smoke test requirement for gameplay changes" section (operational classification). Any batch that affects gameplay, runtime behavior, or visible player-facing state must include a bounded set of visual/gameplay smoke tests before closing. Each test specifies setup, action, expected observable result, and fail criterion. Regression tests required for any intentionally removed behavior. Tests that cannot be run through normal gameplay must be explicitly deferred with a named target (typically Dev Mode / M1.5).

---

## 2026-04-08 — SSoT_Editor_Authoring_Tools.md created and activated (M1.4)

### Authority changes

**New governed SSoT activated:**
- `systems/SSoT_Editor_Authoring_Tools.md` — promoted from **planned** to **active**.
- Covers four editor tools: Card Editor, Deck Editor, Status Effect Wizard, Chord Progression Catalogue Wizard.
- Documents supporting services: `CardAssetFactory`, `MusicianCatalogService`, `DeckJsonImportService`, `DeckCardCreationService`, `DeckValidationService`, `DeckAssetSaveService`.
- Documents composition classifier (`CompositionCardClassifier`) and descriptors (`PartActionDescriptor`, `TrackActionDescriptor`) as editor-relevant runtime utilities.
- Known gaps section maps to M1 roadmap tasks (M1.1–M1.3, M1.5).

**SSoT_INDEX updated:** Editor tools row changed from `**planned**` to `active`.

**coverage-matrix updated:** Editor authoring tools row changed from `(no governed doc yet)` / `**planned**` to `systems/SSoT_Editor_Authoring_Tools.md` / `active`.

**CURRENT_STATE updated:** §1 documentation line updated. §2 editor tooling documentation marked complete. §5 pending docs list updated.

### Scope boundary established
- `SSoT_Editor_Authoring_Tools.md` owns tool capabilities, workflows, and known gaps.
- `SSoT_Card_Authoring_Contracts.md` retains authority over data contracts, JSON schema, and effect-list representation.
- No overlap or duplication between the two documents.

### Lifecycle
- M1.4 (Editor tooling documentation) can be marked complete in `Roadmap_ALWTTT.md`.

---

## 2026-04-08 — Project scope broadened from Combat MVP to full ALWTTT game project

### Authority changes

**Roadmap authority replaced:**
- `planning/active/Roadmap_Combat_MVP.md` → archived to `planning/archive/`. Superseded by `planning/active/Roadmap_ALWTTT.md`.
- `Roadmap_Combat_MVP_Closure_Actionable.md` → archived to `planning/archive/`. All phases complete; historical record only.
- `ALWTTT_DeckEditorWindow_Roadmap_Proposal.md` → archived to `planning/archive/`. Phases 0–6 substantially complete; remaining polish items captured in `Roadmap_ALWTTT.md` M1.1.
- New active roadmap: `planning/active/Roadmap_ALWTTT.md` — project-wide milestone-based roadmap with DoD checklists and demo-readiness checks.

**Coverage matrix updated:**
- Roadmap row now points to `Roadmap_ALWTTT.md`.
- New row added: "Editor authoring tools" — planned primary home `systems/SSoT_Editor_Authoring_Tools.md`.

**SSoT_INDEX updated:**
- Planning docs section restructured into "Active planning docs" and "Archived planning docs" with supersession pointers.
- Transitional compatibility path for `planning/combat/` removed (no longer needed).
- `SSoT_Editor_Authoring_Tools.md` registered as planned.

### Operational changes

**CURRENT_STATE reframed:**
- §1 changed from "Combat MVP closed" single-slice framing to "Project foundation" covering combat baseline, composition surface, editor tools, and documentation state.
- §2 changed from Phase 4 completion record to active work: Deck Editor polish, Status Icons/Tooltips audit, editor tooling documentation.
- §3 changed from "Post-MVP work (not blocking closure)" to forward-looking "What is next": Dev Mode scene, composition validation.
- §4 updated with new risk: StatusIconsData uses legacy StatusType enum, disconnected from all Combat MVP statuses.
- New open item added: true card copies in decks (current runtime deduplicates by reference).

### New findings recorded

- `StatusIconsData` and `StatusIconData` are keyed on legacy `StatusType` enum. None of the six Combat MVP statuses exist in that enum. Icon pipeline is disconnected from working status effects.
- Card tooltip system is keyword-based only (`CardDefinition.Keywords` → `TooltipManager`). No connection to card effects or status effects.
- Deck Editor phases 0–6 from the original roadmap proposal are substantially implemented. Remaining work: better filters, card preview info, cross-tool integration.

### Lifecycle decisions
- Combat MVP roadmaps archived as completed historical records.
- DeckEditorWindow Roadmap Proposal archived as substantially complete; remaining items absorbed into new roadmap M1.
- Claude Project scope broadened from Combat MVP focus to full ALWTTT game project.
- Project instructions, name, and description updated to reflect full game scope.

---

## 2026-03-23 — Combat MVP Phase 4 closure

### Semantic changes

**SSoT_Gig_Combat_Core:**
- Composure reset timing corrected: clears at each `PlayerTurnStart` tick, not per-song. These are not equivalent.
- Breakdown consequences corrected and expanded: Cohesion−1 → LoseGig if Cohesion≤0 → Shaken application → Stress reset. Ordering and short-circuit now documented.
- Stress reset formula corrected: `floor(StressMax * breakdownStressResetFraction)` (default 0.5, configurable). Previous doc said `ceil(StressMax / 2)`.
- Shaken duration corrected: expires at start of Audience Turn N+1 (AudienceTurnStart tick), not "until end of next Song" as previously stated. Active through one full song cycle from application.
- Shaken restrictions (Action card block, Composure penalty) reclassified as design intent — not enforced in runtime. Explicitly noted.
- Added §6.4 Exposed: each stack adds 0.25 to incoming stress multiplier on musicians. No audience path.
- Added §6.5 Feedback DoT: musician-only in AudienceTurnRoutine. Audience version explicitly deferred.
- §11 replaced: stale validation gaps removed, implementation status table added.

**SSoT_Status_Effects:**
- §3 expanded with tick timing system documentation: PlayerTurnStart=8, AudienceTurnStart=9, StartOfTurn=1 (legacy).
- §3.2 added: dual status system documented. Legacy StatusType path and current SO+container path both exist. New work goes through SO+container only.
- §5 rewritten: abstract placeholders replaced with concrete canonical MVP set. All six statuses now specified with primitives, keys, SO config, tick timing, combat meaning, and validation status.
- Shaken (§5.4) added as new canonical status. AudienceTurnStart tick. Restrictions pending.
- Exposed (§5.5) added as new canonical status.
- Feedback (§5.6) added as new canonical status. Musician-only, audience deferred.

**SSoT_Card_System:**
- DrawCardsSpec confirmed implemented and validated (Phase 2). Added to built-ins table.
- Performer rule (§8.1) clarified: performer = card owner via FixedPerformerType; Self = card owner. Validated in Fix 3.7a.
- Built-ins section (§6.2) restructured as a table with implementation/validation status.

**SSoT_Gig_Encounter:**
- §7.3 Failure rule: added implementation note. Method is `GigManager.LoseGig()` (public). Called from `MusicianBase.OnBreakdown`. No method named `TriggerGigLoss` exists.

**SSoT_Runtime_Flow:**
- §4 expanded with CompositionSession bypass: `ExecuteGigPhase()` returns early when `_session != null`. Documented as deliberate decoupling, not a bug. Added as runtime invariant #7.

**CURRENT_STATE:**
- §1 updated to reflect Combat MVP closed.
- §2 replaced with Phase 4 completion record (all decisions A–H).
- §3 replaced with post-MVP work ordered by value. Composition session testing listed as highest priority.
- §4 updated with current non-blocking open items.

### Authority changes
None. All existing authority assignments are unchanged.

### Lifecycle decisions
- Audience Feedback DoT: explicitly deferred. Requires Stress path on AudienceCharacterBase.
- Shaken restrictions: design decision deferred. Status applies and expires correctly; restrictions are a follow-up pass.
- Shaken SO Tick Timing changed from PlayerTurnStart → AudienceTurnStart. Duration is now one full song cycle, not one player turn.

---

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

---

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
- the current governed docs tree can now answer "where did this old doc go?" without re-opening the snapshot manually

### Migration impact
- ALWTTT no longer has a missing primary home for encounter-level truth
- the remaining migration work became normalization/final replacement rather than creation of major new subsystem authorities

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

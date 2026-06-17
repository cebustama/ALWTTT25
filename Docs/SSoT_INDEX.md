# SSoT_INDEX — ALWTTT

This file is the master index for governed documentation.

It defines:
- which docs are authoritative,
- which docs are planning/reference/archive only,
- where cross-project boundaries live,
- and how the old source tree maps into the governed system.

---

## Root governance docs

| Document | Role | Status |
|---|---|---|
| `README.md` | root entry + authority order | active |
| `SSoT_CONTRACTS.md` | documentary governance rules | active |
| `CURRENT_STATE.md` | validated project baseline + active work + next steps | active |
| `coverage-matrix.md` | authority lookup | active |
| `changelog-ssot.md` | semantic/documentary change history | active |
| `SSoT_INDEX.md` | authority map / index | active |

---

## Active governed subsystem SSoTs

### Systems

| Document | Primary scope | Status |
|---|---|---|
| `systems/SSoT_Gig_Combat_Core.md` | gig/combat rules, meters, loop/song hooks, turn structure | active |
| `systems/SSoT_Card_System.md` | card model, action vs composition semantics, payload boundaries | active |
| `systems/SSoT_Card_Authoring_Contracts.md` | authoring/import/editor contracts for cards and statuses | active |
| `systems/SSoT_Audience_and_Reactions.md` | audience entities, tastes, intention/reaction contracts, persuasion progress | active |
| `systems/SSoT_Status_Effects.md` | runtime status truth and catalogue-facing gameplay semantics | active |
| `systems/SSoT_Scoring_and_Meters.md` | loop score, song hype, vibe, and meter-relationship semantics | active |
| `systems/SSoT_Gig_Encounter.md` | encounter-level gig structure, victory/failure, rosters, modifiers, resolution envelope | active |
| `systems/SSoT_Editor_Authoring_Tools.md` | editor tools inventory, capabilities, supporting services, known gaps | active |
| `systems/SSoT_Dev_Mode.md` | Dev Mode tooling: compile-time gating, overlay, infinite turns, hand-visibility bridge | active |
| `systems/SSoT_Audio.md` | ALWTTT-side audio: SFX subsystem (card-direct + bus-sensory, single AudioManager sink), music-mix model (per-musician axis, global music, master SFX, persistence), audio boundary | active |

> **M4.6F-2 navigation note (2026-05-07):** the four Gig SOs (`GigFlowSettingsSO`, `MeterTuningSO`, `GigPresentationSO`, `GigDevSettingsSO`) are governed under `SSoT_Gig_Combat_Core` (config locality §12). The renamed `GigSetupRosterSO` is governed under `SSoT_Gig_Encounter` (§7.5). `MeterTuningSO` is also referenced by `SSoT_Scoring_and_Meters` for semantic conversion meaning.

### Runtime

| Document | Primary scope | Status |
|---|---|---|
| `runtime/SSoT_Runtime_Flow.md` | runtime managers, phase flow, deck/hand → play → execute → resolve | active |
| `runtime/SSoT_Runtime_CompositionSession_Integration.md` | ALWTTT-side composition/session runtime bridge | active |

### Integrations

| Document | Primary scope | Status |
|---|---|---|
| `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md` | explicit ownership split and contract boundary | active |

---

## Active planning docs

| Document | Scope | Status |
|---|---|---|
| `planning/active/Roadmap_ALWTTT.md` | project-wide milestone roadmap | active |
| `planning/active/Roadmap_Audio.md` | audio work-stream sub-roadmap: bus model + batch sequence (SFX-FIX, OST, Ambience, per-character profiles) + decisions ledger | active |
| `planning/active/Design_Demo_Cut_v1.md` | demo-cut planning: run shape, coverage matrix, mechanics introduced, validation | active |
| `planning/active/Design_Starter_Deck_v1.md` | first starter deck design: composition principle, per-card axis assignments, tuning rationale | active |
| `planning/active/Design_Audience_Status_v1.md` | audience-side status design intent (§4 Captivated remains active; §3 + §5 superseded) | partial |
| `planning/active/Design_Tutorial_System_v0_1.md` | tutorial system: scope, trigger model, presentation, UX, demo-cut + Phase C dialogue inventory, DoD per Standing Directive #3 (2026-05-23) | active |
| `planning/active/Design_Vertical_Slice_v0_1.md` | Phase C scope: ship hub, venues, audience archetypes + state machine, boss design, scene transitions (D-RUN-1..6 locked 2026-05-23) | active |
| `planning/active/Design_Sensory_Contract_v0_1.md` | Sensory Contract operational expansion: bus design, audit-table placeholder, smoke/fire VFX integration plan for S3, consumer inventory (D2 expansion 2026-05-23) | active |
| `planning/Design_Project_Directives_v0_1.md` | standing project-level design directives (D1 promoted 2026-05-20; D2 + D3 promoted on declaration 2026-05-23) | standing |
| `planning/Design_Pending_Effects_v1.md` | post-MVP first-batch design pillar: song-scoped accumulator layer | long-term |
| `planning/Design_Tempo_Identity_v1.md` | long-term post-MVP direction: tempo-coupled card identity | long-term |
| `planning/Design_Song_Parts_Library_v0_1.md` | long-term post-MVP direction: stored & repeatable song Parts | long-term |
| `integrations/midigenplay/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` | observable musical expressive surface for composition cards across the MidiGenPlay boundary; 26-axis matrix + 5 documented gaps + per-musician SO whitelist precedence (2026-05-20) | active |

---

## Archived planning docs

| Document | Original scope | Status | Superseded by |
|---|---|---|---|
| `planning/archive/Roadmap_Combat_MVP.md` | combat MVP short roadmap | complete / archived | `Roadmap_ALWTTT.md` (Combat MVP recorded as completed milestone) |
| `planning/archive/Roadmap_Combat_MVP_Closure_Actionable.md` | combat MVP closure phases (5 phases, all complete) | complete / archived | `Roadmap_ALWTTT.md` + `CURRENT_STATE.md` §1 |
| `planning/archive/ALWTTT_DeckEditorWindow_Roadmap_Proposal.md` | Deck Editor design, requirements, phased roadmap (phases 0–6 complete) | substantially complete / archived | `Roadmap_ALWTTT.md` M1.1 (remaining polish items) |
| `planning/archive/M1_5_Dev_Mode_Sub_Roadmap.md` | Dev Mode milestone sub-roadmap; Phases 1–3 closed via M1 closure 2026-04-26; Phases 4 + 5 not implemented (effectively dropped at M1 closure) | complete (Phases 1–3) / dropped (Phases 4–5) / archived | `systems/SSoT_Dev_Mode.md` (Dev Mode implementation authority) |

---

## Supporting governed folders

| Folder / doc | Role | Status |
|---|---|---|
| `planning/active/` | live roadmaps and future sequencing | active |
| `planning/archive/` | completed/superseded planning | active |
| `reference/` | explanatory/support material that does not override SSoTs | active |
| `archive/` | historical/superseded/absorbed traceability | active |
| `archive/absorbed/Source_Docs_Supersession_Map.md` | durable old→new redirect map | active |
| `archive/SNAPSHOT_RETENTION_POLICY.md` | policy for keeping the pre-governance snapshot as backup only | active |

---

## Transitional source docs from the previous set

These are important source inputs, but they are **not** the governed structure.

| Previous doc | Current role in migration | Governed home now |
|---|---|---|
| `canon/SSoT_Combat.md` | delta snapshot only | `CURRENT_STATE.md` + `systems/SSoT_Gig_Combat_Core.md` |
| `canon/Roadmap_Combat.md` | planning-only source | `planning/archive/Roadmap_Combat_MVP.md` (archived) |
| `canon/Appendix_Authoring_DataContracts.md` | source promoted in Batch 02 | `systems/SSoT_Card_Authoring_Contracts.md` |
| `reference/Gig_Combat.md` | source absorbed in Batch 02 | `systems/SSoT_Gig_Combat_Core.md` |
| `reference/Card.md` | source absorbed in Batch 02 | `systems/SSoT_Card_System.md` |
| `reference/Gig.md` | source absorbed in Batch 05 | `systems/SSoT_Gig_Encounter.md` |
| `reference/AudienceMember.md` | source absorbed in Batch 04 | `systems/SSoT_Audience_and_Reactions.md` |
| `reference/StatusEffects.md` | source absorbed in Batch 04 | `systems/SSoT_Status_Effects.md` |
| `reference/StatusEffects_Primitives_with_References.md` | source retained as governed reference | `reference/CSO_Primitives_Catalog.md` |
| `backlog/ideas/loopscore_songhype_vibe.md` | source absorbed in Batch 04 | `systems/SSoT_Scoring_and_Meters.md` |
| `backlog/ideas/gig_pipeline_summary.md` | runtime source absorbed in Batch 03 | `runtime/SSoT_Runtime_Flow.md` |
| `reference/subsystems/Composition Pipeline/SSoT_Runtime_CompositionSession_Bridge.md` | source promoted in Batch 03 | `runtime/SSoT_Runtime_CompositionSession_Integration.md` |
| `reference/subsystems/Composition Pipeline/SSoT_CompositionCards_TrackStyleBundles.md` | mixed source doc | split across card/runtime/boundary docs |
| `reference/subsystems/Composition Pipeline/SSoT_CompositionAuthoringTools.md` | package-heavy reference | MidiGenPlay docs + ALWTTT boundary docs |
| `reference/subsystems/Composition Pipeline/SSoT_Composer_*` | package-owned composer internals | MidiGenPlay docs |
| `archive/ALWTTT_MidiGenPlay_TS_Normalization_Roadmap.md` | historical cross-project planning | archive/reference only |

For the durable old→new crosswalk, use:
- `archive/absorbed/Source_Docs_Supersession_Map.md`

---

## Explicit non-authority categories

These are useful, but are **not** primary truth:
- roadmap docs
- backlog docs
- research prompts
- historical progress reports
- archived snapshots
- legacy docs

Those must not silently define current implementation truth.

---

## Local update reminder

After every meaningful technical change:

1. Identify what concept actually changed.
2. Find its primary home in `coverage-matrix.md`.
3. Update that primary SSoT first.
4. Then apply the follow-up rules:
   - update `CURRENT_STATE.md` if operational reality or active focus changed
   - update `changelog-ssot.md` if meaning, contract, authority, or interpretation changed
   - update `coverage-matrix.md` only if the concept's primary home changed
   - update reference docs only if support/navigation material changed

A technical change is not complete until the required documentation updates are done.

Minimum decision batch for doc-update triage:
- `SSoT_INDEX.md`
- `coverage-matrix.md`
- `CURRENT_STATE.md`
- `changelog-ssot.md`
- the suspected primary SSoT(s) for the changed concept

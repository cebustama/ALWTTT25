# coverage-matrix — ALWTTT

This matrix maps each major concept to its authoritative home.

| Concept / subsystem | Primary authority now | Intended final authority | Status | Notes |
|---|---|---|---|---|
| Root documentation rules | `SSoT_CONTRACTS.md` | `SSoT_CONTRACTS.md` | active | documentary governance is rooted here |
| Current implementation slice | `CURRENT_STATE.md` | `CURRENT_STATE.md` | active | live status layer only |
| Card gameplay semantics | `systems/SSoT_Card_System.md` | `systems/SSoT_Card_System.md` | active | action vs composition, payload semantics, targeting, runtime role |
| Card authoring/import contracts | `systems/SSoT_Card_Authoring_Contracts.md` | `systems/SSoT_Card_Authoring_Contracts.md` | active | promoted from prior appendix |
| Gig/combat core rules | `systems/SSoT_Gig_Combat_Core.md` | `systems/SSoT_Gig_Combat_Core.md` | active | replaces split authority between reference + canon delta |
| Gig encounter structure | `systems/SSoT_Gig_Encounter.md` | `systems/SSoT_Gig_Encounter.md` | active | encounter-level structure has its own governed home |
| Gig setup roster (decks/encounters/audience pool/generic catalog/max count) | `systems/SSoT_Gig_Encounter.md` | `systems/SSoT_Gig_Encounter.md` | active | M4.6F-2: hosted on `GigSetupRosterSO` (renamed from `GigSetupConfigData`) |
| Gig flow settings + setup defaults (JamRules, Action card gating, Gig End behavior, setup-screen defaults, MeterTuning, GigPresentation, GigDevSettings) | `systems/SSoT_Gig_Combat_Core.md` | `systems/SSoT_Gig_Combat_Core.md` | active | M4.6F-2: split across `GigFlowSettingsSO` + `MeterTuningSO` + `GigPresentationSO` + `GigDevSettingsSO`; setup-screen consumption surface in `SSoT_Gig_Encounter.md` §7.5 |
| Audience member / reactions | `systems/SSoT_Audience_and_Reactions.md` | `systems/SSoT_Audience_and_Reactions.md` | active | persuasion progress, preferences, intentions, reaction contracts |
| Status system | `systems/SSoT_Status_Effects.md` | `systems/SSoT_Status_Effects.md` | active | runtime status truth, catalogue boundary, canonical MVP statuses |
| LoopScore / SongHype / Vibe | `systems/SSoT_Scoring_and_Meters.md` | `systems/SSoT_Scoring_and_Meters.md` | active | scoring pipeline semantics and conversion relationships |
| Editor authoring tools | `systems/SSoT_Editor_Authoring_Tools.md` | `systems/SSoT_Editor_Authoring_Tools.md` | active | Card Editor, Deck Editor, Status Effect Wizard, Chord Progression Catalogue, supporting services; incl. CE-L1 "Generate with LLM" panel (2026-06-11) — mechanism reference in `reference/Report_CardLLM_Pipeline.md` |
| Dev Mode tooling | `systems/SSoT_Dev_Mode.md` | `systems/SSoT_Dev_Mode.md` | active | `ALWTTT_DEV` gating, F12 overlay, infinite turns, convinced reset, between-song hand reset, hand-visibility bridge |
| Audio: SFX sink + sensory SFX keys + sound inventory | `systems/SSoT_Audio.md` | `systems/SSoT_Audio.md` | active | AudioManager sink, SensorySfxType, SoundBankSO; migrated from Sensory Contract §5A (2026-06-15) |
| Audio: music-mix model (per-musician axis, global music, master SFX) | `systems/SSoT_Audio.md` | `systems/SSoT_Audio.md` | active | AudioMixSettingsSO + GigManager mix wiring; global music migrated off GameplayData (M-AUDIO-MIX) |
| Audio: OST playback bus (`MusicDirector` + `OstCatalogSO` + `OstTrackId`) | `systems/SSoT_Audio.md` | `systems/SSoT_Audio.md` | active | dedicated singleton, two-source crossfade, scene→track map; Music level scales gig + OST (AUDIO-OST 2026-06-16) |
| Audio: ambience bus (crowd loop, SFX group) | `systems/SSoT_Audio.md` | `systems/SSoT_Audio.md` | active | self-provisioned ambienceSource on AudioManager; masterSfx × ambienceLevel × fade; gig-driven duck/return (AUDIO-AMBIENCE 2026-06-16) |
| Audio: per-character reaction SFX (`CharacterSfxProfileSO`) | `systems/SSoT_Audio.md` | `systems/SSoT_Audio.md` | active | per-character clip source for `ReactionPositive/Negative` on `AudienceCharacterData.sfxProfile`; per-polarity `SoundBankSO` fallback; no new SFX key (AUDIO-CHAR-PROFILES phase 1, 2026-06-16) |
| Audio: per-ability SFX (inline `abilitySfx`) | `systems/SSoT_Audio.md` | `systems/SSoT_Audio.md` | active | inline `abilitySfx` on `AudienceAbilityData`, fired once at activation in `AudienceCharacterBase.AbilityRoutine` (immediate/single-source); no new SFX key, no profile map, no musician field (AUDIO-CHAR-PROFILES-2, 2026-06-16) |
| Audio work-stream sequencing | `planning/active/Roadmap_Audio.md` | `planning/active/Roadmap_Audio.md` | planning-only | bus model + batch sequence; not current-state |
| CSO primitive catalog | `reference/CSO_Primitives_Catalog.md` | `reference/CSO_Primitives_Catalog.md` | reference | supporting catalog, not primary runtime authority |
| Runtime phase flow | `runtime/SSoT_Runtime_Flow.md` | `runtime/SSoT_Runtime_Flow.md` | active | gig runtime orchestration and phase flow |
| Composition session runtime bridge | `runtime/SSoT_Runtime_CompositionSession_Integration.md` | `runtime/SSoT_Runtime_CompositionSession_Integration.md` | active | ALWTTT-owned live composition runtime contract |
| First-time tutorial + guided jam | `planning/Design_Tutorial_System_v0_1.md` | `planning/Design_Tutorial_System_v0_1.md` (runtime-authoritative once shipped) | active (shipped S4) | Runtime in `Assets/Scripts/Tutorial/`. Consumes semantic bus events; the bus SFX/FT side stays governed by `SSoT_Audio.md`. firedDialogs persisted in PersistentGameplayData. |
| ALWTTT ↔ MidiGenPlay authority split | `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md` | `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md` | active | explicit ownership split now governed |
| `MidiMusicManager` integration behavior | `runtime/SSoT_Runtime_CompositionSession_Integration.md` (§3.4) | `runtime/SSoT_Runtime_CompositionSession_Integration.md` (§3.4) | active | ALWTTT-side game runtime integration component; truth absorbed into Runtime CompositionSession Integration SSoT (former standalone `SSoT_ALWTTT_MidiMusicManager_Integration.md` removed) |
| Live roadmap / next sequencing | `planning/active/Roadmap_ALWTTT.md` | `planning/active/Roadmap_ALWTTT.md` | planning-only | replaces Combat MVP roadmap; project-wide milestone sequencing |
| Pre-governance source-doc redirects | `archive/absorbed/Source_Docs_Supersession_Map.md` | `archive/absorbed/Source_Docs_Supersession_Map.md` | archive support | durable old→new map, not subsystem authority |
| Snapshot retention policy | `archive/SNAPSHOT_RETENTION_POLICY.md` | `archive/SNAPSHOT_RETENTION_POLICY.md` | archive policy | governs how the snapshot is kept without restoring authority |
| MidiGenPlay package internals | MidiGenPlay docs | MidiGenPlay docs | external authority | ALWTTT must reference, not duplicate |
| Legacy docs / superseded models | `archive/` | `archive/` | archive | keep for context, never as primary truth |

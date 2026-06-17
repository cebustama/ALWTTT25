# Design_Project_Directives — ALWTTT

**Status:** Standing design directives — apply continuously, not per-batch.
**Created:** 2026-05-15
**Rule:** This document lists project-level design intent that should bias all design and implementation decisions. It is not a planning roadmap and does not override subsystem SSoTs. It DOES express preferences that the project applies when in conflict with authoring convenience.

---

## Scope

Project-level design directives that:
- bias decisions across all batches and milestones,
- survive the closure of any single batch,
- inform recurring trade-offs (e.g. reuse existing asset vs author a new one),
- should be considered for promotion into project-level instructions when stable.

Directives differ from invariants (which are hard constraints owned by SSoTs) and from roadmap items (which are scoped work). A directive is a *standing preference* with operational consequences.

---

## D1 — Sound design priority

**Declared:** 2026-05-15

**Statement.** ALWTTT is a game about music bands. Sound design is a maximum design priority. When in conflict with authoring convenience, code economy, or visual polish, sound design wins.

**Reasoning.** The game's identity, marketing pitch, and player retention all rest on audio fidelity. The procedural music coupling to gameplay (cards → composition → audible song) is the project's distinguishing feature against other deckbuilders. Reused or generic audio undermines that distinction every time it ships.

**Operational consequences.**

1. **Per-musician instrument identity is preferred over generic reuse.**
   - Example: Sibi's voice on Singing Field — D-DCP-5=β path (new `InstrumentEffect` SO authored for Sibi) chosen over α (reuse `Bass / Guitar / Synth`). The β path costs +1 SO + audition time; the α path would have silently homogenized Sibi's voice with C2's parts. β wins on directive D1.
   - Future musicians (Conito bass, Ziggy vocals) should each receive their own `InstrumentEffect` SO at authoring time, not at polish time.

2. **Audio identity is designed at design time, not in polish.**
   - New audience archetypes, statuses, and venues should consider their audio signature at the same time as their mechanic and visual design.
   - "We'll add the sound later" is the failure mode this directive blocks.

3. **Demo packaging prefers PD-3=C over PD-3=B when feasible.**
   - The demo's core appeal is audible. A pitch package without a playable build forces viewers to imagine the audio from screenshots and a 60-90s video. A playable build lets them hear the music respond to their cards. Build packaging is preferred even at the cost of one more sub-batch and resolving the MidiGenPlay Player-build follow-up.

4. **Sound Designer collaborator is a desired addition.**
   - Per PD-4 captured in `Roadmap_ALWTTT.md §6`: a Sound Designer collaborator is pending. The directive D1 should bias future budget / team decisions in favor of formalizing the role rather than deferring.

5. **Audio regressions are surfaced explicitly.**
   - A code change that produces correct gameplay but degrades the audio experience (e.g. stem cache invalidation that homogenizes a track that should have been preserved) should be flagged at batch closure, not absorbed silently.

**Non-consequences.**

- This directive does NOT override hard invariants from SSoTs. The MidiGenPlay boundary remains the MidiGenPlay boundary; sound priority does not justify reaching into package internals.
- This directive does NOT mean every batch must touch audio. It means audio is a first-class consideration when audio is a relevant axis.
- This directive does NOT mean unlimited audio scope. Trade-offs still apply (e.g. demo timeline against audio polish), but the trade-off is documented, not invisible.

**Promoted 2026-05-20.**
Threshold "2-3 batches with positive outcomes" satisfied at lower bound (2/2-3): B3-content-sibi closed 2026-05-20 (β path of new `InstrumentEffect` carrier — audibly distinct Sibi voice shipped via `Fantasia` MIDIInstrumentSO); B3-content-sibi-followup closed 2026-05-20 (musician-pool path preserving per-musician identity — latent `MusicianProfileData` SO-whitelist infrastructure activated, no new SerializeField required). Promoted to standing project guidance via doc-apply session decision A on 2026-05-20. **Pending on user side:** add D1 to the project-level instructions panel (manual UI action). This document retains the canonical articulation; subsequent batches treat D1 as standing guidance, not as a candidate under evaluation.

**Cross-references.**
- `Roadmap_ALWTTT.md §5.3` (Sibi instrument identity item #11.5).
- `Roadmap_ALWTTT.md §6` (Pitch deck PD-3 preference).
- `CURRENT_STATE.md §4` (Sound design priority bullet).
- `integrations/midigenplay/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` (axis inventory; what audio surface is actually addressable).
- `planning/Design_Tempo_Identity_v1.md` (a related long-term design pillar that this directive supports).

---

## D2 — Sensory Contract

**Declared:** 2026-05-23 (planning reframe).

**Statement.** Every player-visible state change MUST produce at minimum a floating-text (FT) sensory artifact. FT + SFX is the preferred baseline. FT + SFX + (animator / shader / particle) is the ideal.

**Reasoning.** ALWTTT's gameplay is dense (meters, statuses, audience reactions, song hype, multiple card kinds). Without consistent sensory artifacts, players miss what's happening, and the demo video doesn't show what the player can't see. The directive bins sensory work into a single rule so feature work cannot ship a state change without at least its FT artifact authored.

**Operational consequences.**

1. **State changes without sensory artifacts are closure blockers.**
   - "We'll add the sound later" is the failure mode this directive blocks (overlapping with D1, but framed at the state-change granularity rather than the audio-quality granularity).
   - Per-batch smoke tests must verify sensory presence for any new player-visible state change.

2. **The Sensory Event Bus is the implementation surface.**
   - Single bus, strongly-typed events, multiple consumers. See `planning/active/Design_Sensory_Contract_v0_1.md §3` for the design intent and S2 deliverable scope.
   - Existing direct-call sensory sites migrate to the bus in S2; new sensory work uses the bus.

3. **S1 visual-only exception (one-time, documented).**
   - S1 (B3-slate-F) ships per-audience FT only; SFX coverage for `ResolveLoopEffect` outputs lands in S3. This is the one documented exception to FT + SFX preferred baseline during the demo-cut sequence. After S2 lands, the exception closes.

4. **Phase C features are compliant from the start.**
   - Audience state machine (Phase C / S7) is designed with sensory artifacts in place per `Design_Vertical_Slice_v0_1.md §6.3`. Boss + closing sequence (S8) likewise.

5. **Relation to D1 (Sound Design Priority).**
   - D1 is "sound design is a maximum priority"; D2 is "every state change has a sensory artifact, sound preferred". D1 sets the quality bar for audio when audio is in scope; D2 sets the coverage floor for sensory presence regardless of polish state. Both apply simultaneously.

**Non-consequences.**

- D2 does NOT mandate every state change have all three (FT + SFX + animator). Floor is FT; the rest are stretch.
- D2 does NOT apply to non-visible state (cache invalidations, debug-only state, scene-transition internals).
- D2 does NOT override SSoT invariants; sensory work cannot violate runtime contracts.

**Promotion candidate.** Promoted on declaration to standing directive status (no batches-with-positive-outcomes threshold; the reframe identified the gap as a demo-cut blocker, not a candidate to evaluate). **Pending on user side:** add D2 to the project-level instructions panel.

**Cross-references.**
- `planning/active/Design_Sensory_Contract_v0_1.md` (operational expansion — bus design, audit table, smoke/fire VFX integration plan).
- `Roadmap_ALWTTT.md §5.5` (Phase B DoD, S1-S5 sequencing).
- `Roadmap_ALWTTT.md §7.4` (Phase C DoD).

---

## D3 — Tutorial-as-mandatory

**Declared:** 2026-05-23 (planning reframe).

**Statement.** Every demo-cut feature MUST have tutorial coverage by S4 closure. Every Phase C feature MUST have tutorial coverage by S8 closure. Coverage = first-time-played explanation triggered on the player's first encounter with the mechanic, via the system in `planning/active/Design_Tutorial_System_v0_1.md`.

**Reasoning.** Playtest feedback at the 2026-05-23 reframe surfaced that the demo's rules feel too complex without explanation. Asset image 1 (confirmed ship pilot / band manager per D-RUN-5) is the diegetic voice. A standing directive prevents tutorial coverage from sliding into "polish later" — every new mechanic is closed only when an authored tutorial trigger exists for it.

**Operational consequences.**

1. **No new player-facing mechanic ships without a tutorial trigger.**
   - This is the symmetric companion to D2: D2 covers sensory presence; D3 covers explanatory presence.
   - "Learn by doing" is an acceptable exemption ONLY when explicitly documented in the owning design doc (e.g. `Design_Demo_Cut_v1.md §2.4` allows S4 to merge / drop draft dialogues with rationale).

2. **Tutorial coverage matrix per milestone.**
   - Demo cut: 5-8 trigger inventory in `Design_Tutorial_System_v0_1.md §6`.
   - Phase C: 5 reserved triggers across S6-S8 in `Design_Tutorial_System_v0_1.md §7`.
   - Each session's closure verifies its tutorial coverage as part of DoD.

3. **Pilot integration is the diegetic seam.**
   - Asset image 1 is the standing portrait for all tutorial dialogues. No separate character-intro batch is needed; integration folds into S4 + S6 per `Design_Vertical_Slice_v0_1.md §8`.

4. **Skip + revisit + reset are non-negotiable UX.**
   - D-TUT-2: skip mid-dialogue and revisit from pause menu both ship in S4. Reset (clear HashSet) ships in S4.

**Non-consequences.**

- D3 does NOT mandate exhaustive tutorial coverage of every micro-interaction. Scope is "core mechanics first-time encounter" per D-TUT-1 (basic only, extensible infra).
- D3 does NOT mandate localization. Localization seam is deferred (matches current project state).
- D3 does NOT mandate audio narration on dialogues.
- D3 does NOT override SSoT invariants; tutorial wiring cannot violate runtime contracts.

**Promotion candidate.** Promoted on declaration (same rationale as D2 — identified at reframe as a demo-cut blocker, not a candidate). **Pending on user side:** add D3 to the project-level instructions panel.

**Cross-references.**
- `planning/active/Design_Tutorial_System_v0_1.md` (operational expansion — trigger model, presentation, UX, dialogue inventory, DoD).
- `planning/active/Design_Sensory_Contract_v0_1.md §3` (event bus; tutorial as consumer).
- `planning/active/Design_Demo_Cut_v1.md §2.4 / §5.1` (demo-cut coverage matrix and DoD criterion).
- `planning/active/Design_Vertical_Slice_v0_1.md §9` (Phase C tutorial coverage).
- `Roadmap_ALWTTT.md §5.5` (Phase B DoD).
- `Roadmap_ALWTTT.md §7.4` (Phase C DoD).

---

## Future directives (placeholder)

Add directives here as they emerge from project work. Each directive should follow the D1 / D2 / D3 template: declared date, statement, reasoning, operational consequences, non-consequences, promotion candidate flag, cross-references.

Three standing directives active at time of this update (2026-05-23):
- **D1** Sound Design Priority (promoted 2026-05-20).
- **D2** Sensory Contract (promoted on declaration 2026-05-23).
- **D3** Tutorial-as-mandatory (promoted on declaration 2026-05-23).

Candidate areas for future directives:
- Authoring tool affordances (authoring-via-wizard vs raw-Inspector preference).
- Documentation discipline (SSoT-first vs code-first for new subsystems).
- Demo / showcase content policy (what may be shown vs deferred).
- Persistence / save-system policy (when meta-progression eventually lands).

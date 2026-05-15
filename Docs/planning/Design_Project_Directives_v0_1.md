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

**Promotion candidate.**
This directive should be considered for promotion into project-level instructions once it has biased 2-3 batch decisions with positive outcomes. Until then it lives here as a tracked standing directive.

**Cross-references.**
- `Roadmap_ALWTTT.md §5.3` (Sibi instrument identity item #11.5).
- `Roadmap_ALWTTT.md §6` (Pitch deck PD-3 preference).
- `CURRENT_STATE.md §4` (Sound design priority bullet).
- `planning/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` (axis inventory; what audio surface is actually addressable).
- `planning/Design_Tempo_Identity_v1.md` (a related long-term design pillar that this directive supports).

---

## Future directives (placeholder)

Add directives here as they emerge from project work. Each directive should follow the D1 template: declared date, statement, reasoning, operational consequences, non-consequences, promotion candidate flag, cross-references.

Candidate areas where additional directives may eventually be useful:
- Authoring tool affordances (e.g. preference for authoring-via-wizard vs raw-Inspector).
- Documentation discipline (e.g. SSoT-first vs code-first for new subsystems).
- Demo / showcase content policy (e.g. what may be shown vs deferred).

None active beyond D1 at time of creation.

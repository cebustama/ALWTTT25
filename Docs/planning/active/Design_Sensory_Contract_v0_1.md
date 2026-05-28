# Design_Sensory_Contract_v0_1 — ALWTTT

**Status:** Active design — codifies Standing Directive #2 (Sensory Contract). Operational expansion authored at the 2026-05-23 planning reframe; bus infrastructure lands in S2, sensory coverage pass in S3.
**Scope:** Project-wide rule for player-visible state changes + infrastructure (event bus) + audit + integration plan for S2-S3.
**Classification:** `reference (planning)` — **not a SSoT**. The directive itself lives at `planning/Design_Project_Directives_v0_1.md §D2`; this doc is the operational expansion (consumer inventory, bus design, audit table, smoke/fire VFX integration plan).
**Created:** 2026-05-23

---

## 1. Purpose

The 2026-05-23 planning reframe identified that ALWTTT's player-visible state changes are inconsistently surfaced: some produce floating text (FT), some don't; some have SFX, most don't; almost none have animator / shader / particle artifacts. This is both a gameplay-readability problem (player misses what's happening) and a marketing problem (demo video shows only what the player can see).

This doc codifies the **Sensory Contract** — the standing rule that every player-visible state change must produce at least one sensory artifact — and proposes the **event-bus infrastructure** that makes the rule implementable at scale (multi-consumer, strongly-typed, decoupled from emission sites).

---

## 2. Contract statement (Standing Directive #2)

> Every player-visible state change MUST produce at minimum a floating-text (FT) sensory artifact. FT + SFX is the preferred baseline. FT + SFX + (animator / shader / particle) is the ideal.

The directive itself is recorded at `planning/Design_Project_Directives_v0_1.md §D2`. This section restates it so this doc stands alone for design conversations.

### Player-visible state changes (in scope)

- Card play (effect resolution).
- Meter changes: Vibe, Stress, Composure, Flow, SongHype, Cohesion, Inspiration.
- Status effect apply / tick / decay / expire (musician AND audience side).
- Audience reaction (per-audience response to loop).
- Audience state transition (idle / hostile / vibing — Phase C addition).
- Song-end vibe conversion.
- SFX threshold crossings (lights / smoke / fire — existing mechanic).
- Turn / phase transitions.
- Win / loss / draw outcomes.

### Non-events (out of scope — directive exempt)

- Internal cache invalidations.
- Debug-only state.
- Background data marshalling.
- Anything happening during a fade or scene transition where the player is not in interactive control.

### Exception clauses (documented, narrow)

- **S1 visual-only exception.** S1 (B3-slate-F) ships per-audience FT only; SFX for `ResolveLoopEffect` outputs lands in S3. This is the one explicitly-documented exception to the FT+SFX preferred baseline during the demo-cut sequence. See B3-slate-F constraints in the 2026-05-23 rehydration prompt.
- **Pre-existing gaps** (status icons without sound, etc.): closed during S3 sensory coverage pass. The audit table in §4 tracks them.

---

## 3. Event bus design (planned for S2 — high-level only here)

S2 owns the implementation. This section locks design intent, not code.

### Concept

```
SensoryEventBus (singleton or service-located)
    Publish<TEvent>(TEvent evt) where TEvent : ISensoryEvent
    Subscribe<TEvent>(Action<TEvent> handler)
    Unsubscribe<TEvent>(Action<TEvent> handler)
```

### Event types (initial inventory — refined in S2)

| Event type | Producer | Notes |
| --- | --- | --- |
| `CardPlayedEvent` | `CardBase.ExecuteEffects` / `CardUseRoutine` | carries card ref + result summary |
| `MeterChangedEvent` | per-meter mutator paths (`ApplyIncomingStressWithComposure`, `AddCurrentInspiration`, `ApplyIncomingVibe`, etc.) | carries meter type + old / new / delta |
| `StatusAppliedEvent` | `StatusEffectContainer.Apply` | carries target + status SO + initial stacks |
| `StatusTickEvent` | catalogue-specific tick paths (e.g. Earworm vibe-tick in `GigManager.AudienceTurnRoutine`) | carries target + status SO |
| `StatusDecayedEvent` / `StatusExpiredEvent` | `StatusEffectContainer.Tick` decay branches | carries target + status SO + reason |
| `AudienceReactionEvent` | `GigManager.TriggerAudienceMicroReactions` → `ResolveLoopEffect` | carries audience + impression sign/magnitude (post-S1) |
| `AudienceStateChangedEvent` | Phase C audience state machine | carries audience + old / new state |
| `SongEndedEvent` | `GigManager.RunSongVibeResolution` | carries per-audience vibe deltas |
| `SfxStageCrossedEvent` | `GigManager.FireSongHypeStage` | carries stage index + SFX tag |
| `TurnPhaseChangedEvent` | turn / phase machine | carries old / new phase |
| `GigOutcomeEvent` | `GigManager.WinGig` / `LoseGig` | carries outcome + summary |

### Consumers (S2 onwards)

| Consumer | Subscribes to | Status |
| --- | --- | --- |
| `FloatingTextMidiListener` (existing FT pipeline) | meter / status / audience events | retrofitted in S2 |
| `FxManager` (existing SFX pipeline) | meter / status / audience events (S3 coverage) | expanded in S3 |
| `StageLightAnimator` (existing) | `SfxStageCrossedEvent` | retrofitted in S2 |
| `BackgroundContainer` (existing) | `SfxStageCrossedEvent` | retrofitted in S2 |
| `TutorialController` (S2-new — per Design_Tutorial_System §3) | most event types | new consumer; first non-sensory subscriber |
| `StageVfxController` (S3-new) | `SfxStageCrossedEvent` (smoke / fire) | new consumer; §5 |
| Future animator / shader / particle consumers | per-feature | added per-feature in S3 and Phase C |

### Strict typing rationale

Avoids stringly-typed event topics; consumers subscribe to the event type and the compiler enforces shape. Slower than direct calls but the per-frame cost is negligible at our event volume (single-digit publishes per turn, low double-digits during song-end).

### Coexistence with direct calls during transition

- **S1 (B3-slate-F)** lands BEFORE S2 (event bus). S1 ships per-audience FT emission via direct calls to the existing FT pipeline (`FloatingTextMidiListener` API or similar).
- **S2** introduces the event bus AND migrates S1's direct calls to bus events. After S2, new sensory work goes through the bus by default.
- **S3-onwards** never uses direct calls for sensory artifacts unless documented.

This S1→S2 ordering is the one explicit exception to "everything goes through the bus" and is acceptable because (a) S1 is a single small site, (b) S2 owns the migration as part of its DoD.

---

## 4. Audit table (PLACEHOLDER — filled during S2 sensory coverage audit)

This table is the starting skeleton. S2's first deliverable is to fill it completely by walking the codebase, identifying every player-visible state change, and recording its current sensory coverage. The "Gap" column is what S2 / S3 close.

| State change | Producer (code site) | FT? | SFX? | Anim/Shader/Particle? | Gap → owning session |
| --- | --- | --- | --- | --- | --- |
| Card played | `CardBase.ExecuteEffects` | partial | no | partial (Use anim) | S2 audit + S3 SFX |
| Vibe change (audience) | `ApplyIncomingVibe` | yes (post-S1) | no | no | S3 SFX |
| Stress change (musician) | `ApplyIncomingStressWithComposure` | yes | no | partial (status icon if Composure crosses 0) | S3 SFX |
| Composure change | `ModifyStressSpec` (negative) / status apply | yes | no | partial (icon, per M1.8) | S3 SFX |
| Status applied | `StatusEffectContainer.Apply` | partial | no | yes (icon appear anim, per M1.8) | S2 audit + S3 SFX |
| Status tick (Earworm vibe pulse) | `GigManager.AudienceTurnRoutine` Earworm block | yes | no | no | S3 SFX + animator |
| Status decay / expire | `StatusEffectContainer.Tick` | partial | no | yes (icon disappear anim, per M1.8) | S2 audit |
| SongHype meter | `GigManager.AddSongHype` | yes | no | yes (existing SFX lights/smoke/fire on stage crossing) | mostly compliant; S2 audit |
| Stage crossing (lights/smoke/fire) | `GigManager.FireSongHypeStage` | yes ("+N Vibe!" floater) | yes (existing) | partial (lights present; smoke/fire VFX = S3 work, §5) | S3 VFX |
| Audience state change | Phase C — see Design_Vertical_Slice §6.3 | n/a (designed compliant) | n/a | n/a | Phase C (S7) |
| Song-end vibe conversion | `GigManager.RunSongVibeResolution` | yes | no | partial | S2 audit + S3 SFX |
| Turn phase change | turn machine | partial | no | partial | S2 audit |
| Win / loss outcome | `WinGig` / `LoseGig` | yes (panel) | partial | partial | S2 audit |

**Format note.** Each row's "Gap → owning session" column lists where the gap is closed. S2 fills the FT column completeness; S3 fills the SFX and most of the animator/shader/particle columns; Phase C feature work fills its own rows from the start (per Standing Directive #2).

---

## 5. Smoke / fire VFX integration plan (S3, asset #4)

S3 lands the smoke and fire particle / animation artifacts for SFX threshold crossings.

### Background

Existing mechanic (shipped pre-2026-05-23):
- Stage 1 (lights) — `BackgroundContainer.ActivateSFX("lights")` + `StageLightAnimator` color/intensity ramp.
- Stage 2 (smoke) — `BackgroundContainer.ActivateSFX("smoke")` — currently no VFX bound.
- Stage 3 (fire) — `BackgroundContainer.ActivateSFX("fire")` — currently no VFX bound.

The SFX→FlatVibe bonus (`+3 / +6 / +10` Vibe) already fires on stage crossings (per §5.3.5 demo cut prep). Visual lights work; smoke and fire are silent.

### Integration plan (S3)

- New consumer (likely `StageVfxController` or extension of `BackgroundContainer`) subscribes to `SfxStageCrossedEvent`.
- **Stage 1**: existing lights kick. No new VFX.
- **Stage 2**: smoke particle system + optional light-tint shift. Asset #4 (per the 2026-05-23 asset drop) provides the smoke sprite/texture.
- **Stage 3**: fire particle system + intensity ramp. Asset #4 also provides fire visuals.
- Per-venue overrides supported (Phase C venues may want different intensities — see Design_Vertical_Slice §5).

### S3 also closes broader SFX gaps

Per the audit table in §4, S3 is the session where SFX coverage lands for events identified as "SFX gap" — not only smoke/fire but also card-play SFX, status-apply SFX, stress/vibe-change SFX, etc. Smoke/fire is the most visible deliverable; SFX coverage is the broader deliverable.

---

## 6. Consumers (forward refs)

| Consumer | Lives at | Role |
| --- | --- | --- |
| `FloatingTextMidiListener` | existing | primary FT producer; subscribes to meter / status / audience events in S2 |
| `FxManager` | existing | primary SFX producer; expanded coverage in S3 |
| `StageLightAnimator` | existing | lights animation on stage 1 crossing |
| `BackgroundContainer` | existing | SFX tag dispatcher; may absorb `StageVfxController` role or run alongside it (S3 decision) |
| `TutorialController` | S2-new (per Design_Tutorial_System §3) | bus subscriber; uses events as trigger sources for first-time-played dialogues |
| `StageVfxController` (working name) | S3-new | smoke / fire particle systems on stage 2 / 3 crossings |
| future animator-trigger consumer | per-feature | landed in S3 and Phase C as needed |
| future shader-flash consumer | per-feature | landed if any meter / status wants a screen-edge flash |
| future particle consumer | per-feature | landed if any state change wants a particle burst |

---

## 7. Smoke test discipline

Per the project's standing rule (project instructions, "Smoke test requirement for gameplay changes"):
- **S1** includes per-audience FT smoke test (already in B3-slate-F batch plan).
- **S2** includes event-bus pub/sub smoke tests + audit-table coverage of every existing consumer.
- **S3** includes per-event sensory-artifact smoke tests across the audit table (FT and SFX presence; animator/shader/particle where shipped).
- **S4** includes tutorial-trigger smoke tests verifying first-time-fire on bus events.
- **Phase C** sessions each include sensory-compliance smoke tests for their new mechanics.

**Failed sensory tests = closure blocker for the owning session.** No "we'll add the sound later"; that's the failure mode the directive explicitly blocks (per `Design_Project_Directives_v0_1.md §D1.2`).

---

## 8. Forward refs

- `planning/Design_Project_Directives_v0_1.md §D2` (Sensory Contract standing directive — this doc is its operational expansion).
- `planning/Design_Project_Directives_v0_1.md §D1` (Sound Design Priority — overlaps with §D2 on the SFX dimension; §D1 is "sound is a maximum design priority", §D2 is "every state change has a sensory artifact at minimum").
- `planning/active/Design_Tutorial_System_v0_1.md §3` (tutorial as bus consumer).
- `planning/active/Design_Vertical_Slice_v0_1.md §6.3` (audience state machine; Sensory Contract compliance designed in from the start).
- `Roadmap_ALWTTT.md §5.5` (Phase B DoD — Sensory Contract compliance criterion implicit in the per-session DoDs).
- `Roadmap_ALWTTT.md §7` (Phase C — every feature compliant per directive).
- `CURRENT_STATE.md §3` (next-active S1-S8 sequence; S2 = event-bus foundation, S3 = sensory coverage pass).

---

## 9. Open questions deferred to S2

- Exact singleton vs service-locator pattern for the bus (S2 implementation).
- Whether existing direct-call sensory sites are migrated all-at-once or incrementally during S2 (S2 design).
- Whether per-event throttling is needed (S2 — likely not at our event volume).
- Whether the bus emits to multiple subscribers in deterministic order or arbitrary order (S2 — likely arbitrary; if any consumer needs ordering, that consumer manages its own internal scheduling).
- Exact SO format for new event types if any are SO-defined vs code-defined (S2 — likely code-defined record/struct types, but TBD).

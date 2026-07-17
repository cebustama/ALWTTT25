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

## 3. Event bus design (S2 — as-built)

S2 shipped the bus. **S4 (2026-06-17)** added six more semantic event types
(consumed by the tutorial controller). This section is as-built for the eight
implemented event types (marked ✅); the remaining rows are still forward-looking.

### Concept (as-built)

```
SensoryEventBus : MonoBehaviour       // singleton, static accessor (D-S2-1=A)
    Publish<TEvent>(TEvent evt)   where TEvent : ISensoryEvent
    Subscribe<TEvent>(Action<TEvent> handler)
    Unsubscribe<TEvent>(Action<TEvent> handler)
```

**As-built decisions.**
- **D-S2-4=A** — `ISensoryEvent` is an empty marker interface; event types are
  `readonly struct`s; `Publish<TEvent>` is generic with
  `where TEvent : ISensoryEvent`, so publishes are zero-alloc and the compiler
  enforces shape. Semantic payloads only — presentation (FT text/colour, future
  SFX) is never carried on the event.
- **D-S2-1=A + D-S2-INIT=C** — MonoBehaviour singleton with static accessor,
  mirroring `FxManager`. Hardened for init-order independence: the accessor is
  lazy/auto-creating (never null in play mode), the component carries
  `[DefaultExecutionOrder(-100)]` so its `Awake` runs before consumers'
  `OnEnable`, and it emits init-confirmation logs. This is a refinement of
  D-S2-1=A, not a reversal. (Root cause it fixed: a one-shot `OnEnable`
  subscription racing against a not-yet-initialized bus produced intermittent
  silent non-subscription.)

### Event types (full inventory — only the two marked ✅ are implemented in S2)

| Event type | Producer | Notes |
| --- | --- | --- |
| ✅ `CardPlayedEvent` | `DeckManager.OnCardPlayed` | **S4-shipped (producer corrected — see note below).** Carries card Definition + IsComposition / IsAction / InspirationCost. Drives jam beats 1/2/5 + the action-card trigger. |
| `MeterChangedEvent` | per-meter mutator paths (`ApplyIncomingStressWithComposure`, `AddCurrentInspiration`, `ApplyIncomingVibe`, etc.) | **NOT shipped; forward inventory.** S4 deliberately did not add a generic Inspiration-delta meter event: beat 2 reuses `CardPlayedEvent` (InspirationCost > 0) and beat 3 reuses `LoopResolvedEvent` (loop-scoped gain), avoiding a meter event that would over-/mis-fire on session-start and part-advance resets. |
| ✅ `StatusAppliedEvent` | `StatusEffectContainer.Apply` (after `OnStatusApplied`) | **S4-shipped.** Global mirror of the per-container event; carries status + delta stacks. |
| `StatusTickEvent` | catalogue-specific tick paths (e.g. Earworm vibe-tick in `GigManager.AudienceTurnRoutine`) | carries target + status SO |
| `StatusDecayedEvent` / `StatusExpiredEvent` | `StatusEffectContainer.Tick` decay branches | carries target + status SO + reason |
| ✅ `AudienceReactionEvent` | `GigManager` loop-finished path → `ResolveLoopEffect` | **S2-shipped.** Carries audience ref/index/id, raw + clamped impression [-2..2], full `LoopFeedbackContext` |
| `AudienceStateChangedEvent` | Phase C audience state machine | carries audience + old / new state |
| ✅ `SongEndVibeEvent` (planned as `SongEndedEvent`) | `GigManager.RunSongVibeResolution` | **S2-shipped.** Renamed + richer than planned: carries audience ref/index/id, base/intended/applied delta, Flow stacks + multiplier, `BlockedByIndifference` flag |
| ✅ `SfxStageCrossedEvent` | `GigManager.FireSongHypeStage` | **S3-audio-shipped.** Stage index + SFX tag; published after VFX + bonus (D-SA-5); consumed by `SensoryAudioAdapter`. VFX stays on the direct path (D-S3-6=A). |
| `TurnPhaseChangedEvent` | turn / phase machine | carries old / new phase |
| ✅ `GigOutcomeEvent` | `GigManager.ResolveGigOutcomeAndEnd` | **S4-shipped.** Carries Won; normal-flow only (editor Debug Win/Lose menus bypass by design). |
| ✅ `LoopResolvedEvent` | `GigManager.OnCompositionLoopFinished` | **S4-shipped.** Bridges `CompositionSession.LoopFinished`; carries `LoopFeedbackContext`. Beat 3 keys on `InspirationGainedThisLoop` (track-derived per-loop gain). |
| ✅ `AudienceTurnStartedEvent` | `GigManager` phase transition (after `OnEnemyTurnStarted?.Invoke()`) | **S4-shipped.** Bridges the C# turn-start event; drives the audience-action beat (only after a SongHype stage has crossed). |
| ✅ `RewardChoiceOpenedEvent` | `GigManager.WinGig` (reward-screen open, both branches) | **S5h-shipped.** Parameterless; published only when ≥1 reward box was built (not on the skip path). Consumed by `SensoryAudioAdapter` (→ `SensorySfxType.RewardOpened`) and `TutorialController` (`tut_first_reward_choice`). *(Row backfilled 2026-07-13 — S5h-DOC.)* |
| ✅ `MusicianStressHitEvent` | `BandCharacterStats.ApplyIncomingStressWithComposure` (once per incoming hit > 0) | **TUT-R2-shipped.** Semantic; carries stats + `Absorbed` (Composure) + `Applied` (post-Composure, post-Exposed). One publish site covers every Stress producer (all sources funnel through this method — the `StatusAppliedEvent` rationale, D-S4-SRC=A). Drives `tut_musician_breakdown` (first fire with `Applied > 0`). Adds no SFX key. *(Row backfilled 2026-07-13 — OPT-1.)* |
| ✅ `AudienceBlockedEvent` | `AudienceCharacterBase.IsBlocked` setter (false→true transition only) | **TUT-R2-shipped.** Semantic; exists because Blocked is a sprite-tint bool since M1.2 (E3), not an SO status — `StatusAppliedEvent` never fires for it (code-truth deviation from TUT-R1 §4.1). Drives `tut_status_blocked_front`. Adds no SFX key. *(Row backfilled 2026-07-13 — OPT-1.)* |
| ✅ `AudienceVibeImpactEvent` | `CardBase.ExecuteEffects` → `ModifyVibeSpec` branch | **JUICE-PW-shipped.** Published **once per audience target** at effect resolution. Carries audience ref/index/id, performer, card, base/final/applied delta, `FanoutIndex`/`TargetCount`. First bus event to add an SFX key (`CardVibeImpact`). |

**S2 as-built scope (D-S2-2=A).** Exactly two event types ship in S2:
`AudienceReactionEvent` and `SongEndVibeEvent` (the S1 sites). The rest of this
table is the forward inventory for S3 and Phase C; do not treat the unmarked
rows as implemented. Both shipped events live in `Assets/Scripts/Sensory/Events/`.

> **CardPlayedEvent producer (corrected S4).** The producer is **`DeckManager.OnCardPlayed`**
> (`Assets/Scripts/Managers/DeckManager.cs`), NOT `CardBase.ExecuteEffects` / `CardUseRoutine`
> as originally inventoried. That funnel never sees composition cards — they bypass `CardBase.Use`
> (HandController.cs:582-583) and route via `GigManager.TryPlayCompositionCard`; HandController
> calls `OnCardPlayed` after the composition play. `DeckManager.OnCardPlayed` is the one site
> reached by both action/SFX cards (via `CardBase.Use → OnCardPlayed`) and composition cards, so
> it observes every played card exactly once. The earlier attribution would have left jam beats
> 1, 2 & 5 dead. (D-S4-PRODUCER.) The S4 events are additive *semantic* events — they add no
> `SensorySfxType` key and do not touch the SFX sink.

> **Card Vibe impact (JUICE-PW, 2026-07-13).** `CardPlayedEvent` is deliberately **not** the
> carrier for card-effect presentation. It is published from `DeckManager.OnCardPlayed`, i.e.
> **after** `ExecuteEffects` has run, it fires once for the whole card, and it carries no
> per-target delta — so it cannot express "+N landed on member 2, member 3 blocked it".
> `AudienceVibeImpactEvent` is published **inside** the `ModifyVibeSpec` branch, per target,
> after the delta is applied and `appliedVibe` is known.
>
> Two consequences are load-bearing:
> - **Timing guarantee.** Because the publish sits inside effect resolution and
>   `DeckManager.OnCardPlayed` runs at the tail of `CardUseRoutine`, the impact FX is
>   *structurally* earlier than anything keyed on `CardPlayedEvent` — including
>   `TutorialGuidedDriver`'s beat-8 `TutorialLoopHoldGate.Release()`. The player always sees the
>   finisher land before the held loop is let go.
> - **Presentation split (D3=A).** The AoE fan-out is **visual per-member, audio once**:
>   `SensoryFxAdapter` spawns one floater per target (staggered by `FanoutIndex ×`
>   `SensoryFtPresentation.VibeImpactStaggerStep`) and kicks each landed target's animator, plus the
>   performer's on `FanoutIndex == 0`; `SensoryAudioAdapter` plays exactly one sting, because
>   `SensorySfxPresentation.ForCardVibeImpact` returns a key only for `FanoutIndex == 0`.
>
> **FT form.** `-N` cyan when it lands; `INDIFFERENT` grey when blocked (same word/colour as the
> song-end blocked surface — one concept, one presentation). The **short** `-N` is deliberate:
> song-end keeps the long `-N Vibe`, so a late finisher and the song-end wave stay readable when
> they overlap. Blocked members get **no** kick and no particle burst — the grey floater carries it.
> Presentation lives in `SensoryFtPresentation.TryBuildVibeImpactFt`; gameplay only publishes.

### Consumers

| Consumer | Subscribes to | Status |
| --- | --- | --- |
| ✅ `SensoryFxAdapter` (S2-new) | `AudienceReactionEvent` + `SongEndVibeEvent` + `AudienceVibeImpactEvent` (JUICE-PW) | **S2-shipped; Spawn since S3a.** Thin bus→FT adapter (D-S2-6=A). The S2 VerifyOnly mode is retained as a serialized fallback but the shipped mode is **Spawn**. JUICE-PW added the card-impact handler: staggered per-member FT + the procedural impact kick / particle burst on the landed target and on the performer (`FanoutIndex == 0`) — the adapter is therefore no longer FT-only. |
| `SensoryFtPresentation` (S2-new, static helper) | n/a (not a subscriber) | **S2-shipped.** Single source of impression→text/colour + the song-end FT builder; consumed by BOTH the GigManager direct calls and the adapter so the two paths cannot drift before S3 deletes the direct calls (D-S2-7=A) |
| ✅ `SensoryAudioAdapter` (S3-audio-new) | `AudienceReactionEvent` + `SongEndVibeEvent` + `SfxStageCrossedEvent` + `RewardChoiceOpenedEvent` (S5h) + `AudienceVibeImpactEvent` (JUICE-PW) | **S3-audio-shipped.** Thin bus→audio adapter (D-SA-4=A); resolves each event to a `SensorySfxType` via `SensorySfxPresentation`, plays through `AudioManager` (the shared sink). Scene-placed on the gig Listeners object, mirroring `SensoryFxAdapter`. The card-impact handler plays **once per card**, not once per AoE target. |
| ✅ `SensorySfxPresentation` (S3-audio-new, static helper) | n/a (not a subscriber) | **S3-audio-shipped.** Single source of event→`SensorySfxType` selection (audio analogue of `SensoryFtPresentation`); returns null for intentionally-silent surfaces (neutral reaction). |
| `FloatingTextMidiListener` (existing FT pipeline) | — | NOT retrofitted in S2; S3+ if ever (its MIDI-note FT is a separate concern) |
| `FxManager` (existing SFX pipeline) | meter / status / audience events (S3 coverage) | S3 |
| `StageLightAnimator` (existing) | `SfxStageCrossedEvent` | S3 (not retrofitted in S2) |
| `BackgroundContainer` (existing) | `SfxStageCrossedEvent` | S3 (not retrofitted in S2) |
| `TutorialController` (per Design_Tutorial_System §3) | most event types | **deferred to S4** (D-S2-5); typed `Subscribe<T>` already accepts it without redesign |
| `StageVfxController` (S3-new) | `SfxStageCrossedEvent` (smoke / fire) | new consumer; §5 |
| Future animator / shader / particle consumers | per-feature | added per-feature in S3 and Phase C |

### Strict typing rationale

Avoids stringly-typed event topics; consumers subscribe to the event type and the compiler enforces shape. Slower than direct calls but the per-frame cost is negligible at our event volume (single-digit publishes per turn, low double-digits during song-end).

### Coexistence with direct calls during transition

- **S1 (B3-slate-F)** lands BEFORE S2 (event bus). S1 ships per-audience FT emission via direct calls to the existing FT pipeline.
- **S2 (as-built, D-S2-3=A coexistence).** The bus publishes fire **alongside** the existing direct `FxManager.SpawnFloatingText` calls at the two S1 sites — the direct calls are **retained**, not migrated. The S2 subscriber (`SensoryFxAdapter`) runs in VerifyOnly mode so it does not duplicate the on-screen FT. S1 visual output is therefore bit-identical after S2.
- **S3** is where the migration completes: delete the two GigManager direct calls and flip `SensoryFxAdapter` to Spawn. From S3 onward, new sensory work goes through the bus by default and direct calls are not used for sensory artifacts unless documented.

The S1→S2→S3 ordering is the one explicit, time-boxed exception to "everything goes through the bus": S1 is a single small site, S2 adds the bus in parallel, and S3 owns the deletion as part of its DoD.

---

## 4. Audit table (S3a — full sensory-coverage audit)

> **As-built note (2026-06-14, S3a).** The full audit deferred from S2 is now done.
> The two former coexistence rows are **bus-only** (direct FT calls deleted). The
> "SFX (audio)?" column is uniformly deferred to **S3-audio** (D-S3-1=B) — no audio
> subsystem exists yet. Animator/particle gaps that S3a closed are marked as-built;
> "partial" FT/anim entries beyond S3a's scope are future polish, not S3a debt.

> **Terminology note.** "SFX" is overloaded in this project: the codebase's
> `BackgroundContainer.ActivateSFX` means **stage visual FX** (lights/smoke/fire);
> the directive's "SFX" (this column, §5.4, D1) means **audio**. The two are
> distinct subsystems — the audio one is unbuilt and owned by S3-audio.

> **(S3-audio close, 2026-06-14)** The "SFX (audio)?" column is now FILLED for card-play + the three bus surfaces (shipped — placeholder clips; final intentional SFX = D1 follow-up). Remaining "no" rows are deferred to future `SensorySfxType` additions. The S3a note above ("no audio subsystem exists yet") is superseded.

> **(S5h, 2026-07-07 · JUICE-PW, 2026-07-13)** Two `SensorySfxType` keys added since S3-audio:
> `RewardOpened` (mapped by `SensorySfxPresentation.ForRewardOpened()`; clip authoring optional —
> warn-once acceptable per D-SA-2) and `CardVibeImpact` (see the card-effect Vibe row below).
> Key authority: `SSoT_Audio.md` §3 + invariant 18.

| State change | Producer (code site) | FT? | SFX (audio)? | Anim/Particle? | Remaining gap → owner |
| --- | --- | --- | --- | --- | --- |
| Card played | `CardBase.ExecuteEffects` | partial (per-effect; see the Vibe row) | card-direct (`CardBase.Use` → `PlayOneShot(AudioType)`, opt-in by type); shipped (placeholder) | partial (Use anim) | final SFX = D1 follow-up. **A card may instead sound at *impact* rather than at *drop*** — see the Vibe row + `SSoT_Audio` §3/§7 inv. 18 |
| Audience reaction (per-loop) | `GigManager` loop-finished path | yes (incl. neutral "…"); **bus-only (S3a: direct deleted, Spawn)** | bus → `ReactionPositive/Negative` (neutral FT-only); shipped (placeholder) | reaction anim **deferred (D-F-5b — no reaction state; only ability "Tantrum")**; FT outline done (D-S3-2) | final SFX = D1 follow-up; reaction anim → future |
| Audience ability play (e.g. Kid "Tantrum") | `AudienceCharacterBase.AbilityRoutine` | yes (action-name FT) | no | **yes (S3a: ability trigger restored, D-F-5a)** | audio → S3-audio |
| Vibe change (audience) — **card effect** | `CardBase.ExecuteEffects` → `ModifyVibeSpec` → `ApplyIncomingVibe` | **yes (JUICE-PW: bus, per target — `-N` / `INDIFFERENT`)** | **yes (JUICE-PW: bus → `CardVibeImpact`, one sting per card)**; shipped (placeholder clip) | **yes (JUICE-PW: `CharacterAnimator.PlayImpactKick` + particle burst on landed target *and* performer)** | final SFX = D1 follow-up (the shipped clip is a placeholder) |
| Vibe change (audience) — other callers | `ApplyIncomingVibe` (Earworm tick, song-end, audience actions) | yes — via their own rows (Earworm tick / song-end conversion) | see those rows | see those rows | — |
| Stress change (musician) | `ApplyIncomingStressWithComposure` | yes | no | partial (status icon if Composure crosses 0) | audio → S3-audio |
| Composure change | `ModifyStressSpec` (neg) / status apply | yes | no | partial (icon, M1.8) | audio → S3-audio |
| Status applied | `StatusEffectContainer.Apply` | partial | no | yes (icon appear anim, M1.8) | audio → S3-audio |
| Status tick (Earworm vibe pulse) | `GigManager.AudienceTurnRoutine` Earworm | yes | no | no | audio → S3-audio |
| Status decay / expire | `StatusEffectContainer.Tick` | partial | no | yes (icon disappear anim, M1.8) | audio → S3-audio |
| SongHype meter | `GigManager.AddSongHype` | yes | no | yes (stage VFX) | audio → S3-audio |
| Stage crossing (lights/smoke/fire) | `GigManager.FireSongHypeStage` | yes ("+N Vibe!") | bus → `StageCross{Lights,Smoke,Fire}` (D-SA-5); shipped (placeholder) | **yes — lights+smoke+fire wired (S3a: `BackgroundRoot.SetSmoke/SetFire`); performance-only (D-S3-7)** | final SFX = D1 follow-up; smoke/fire animation TODO (§5) |
| Audience state change | Phase C — see Design_Vertical_Slice §6.3 | n/a | n/a | n/a | Phase C (S7) |
| Song-end vibe conversion | `GigManager.RunSongVibeResolution` | yes — **bus-only (S3a: direct deleted; D-S3-5 int overload = S1 drift)** | bus → `SongEndVibe/SongEndBlocked`; shipped (placeholder) | partial | final SFX = D1 follow-up |
| Turn phase change | turn machine | partial | no | partial | audio → S3-audio |
| Win / loss outcome | `WinGig` / `LoseGig` | yes (panel) | no | partial | audio → S3-audio |

**Format note.** S2 filled the FT column; S3a completed the audit, deleted the coexistence direct calls, and closed the animator (ability) + stage-VFX rows. The audio column is owned wholesale by S3-audio (D-S3-1=B). Phase C feature rows are compliant from the start per Standing Directive #2.

> **Correction recorded (JUICE-PW-DOC, 2026-07-13).** The pre-JUICE-PW row asserted FT **yes** /
> SFX **no** / anim **no** for `ApplyIncomingVibe` as a whole. The FT "yes" was inherited from the
> Earworm-tick and song-end callers (which have their own rows); the **card-effect** caller produced
> **no floating text at all** — only the Vibe-bar animation. That is the gap JUICE-PW closed. See
> the sweep note in `changelog-ssot.md` (2026-07-13, JUICE-PW).

> **S2 scope note (2026-06-14).** S2 was deliberately narrowed to bus + 2 event
> types + 2 publish sites + 1 subscriber and did **not** walk the whole codebase
> to fill this table, despite the original §3/§4 wording implying it would. Only
> the two S2 rows below carry an as-built emission-path update. The **full
> sensory-coverage audit lands in S3** alongside SFX coverage.

> **SUPERSEDED — retired 2026-07-13 (JUICE-PW-DOC, OPT-2=B).** The S2 "starting skeleton"
> audit table that lived here disagreed with the S3a as-built table above (two tables defining
> the same coverage — a one-concept-one-authority violation). The as-built table above is the
> only §4 audit. The retired skeleton survives in git history and in the 2026-06-14 entries of
> `changelog-ssot.md`.

---

## 5. Smoke / fire VFX integration plan (S3, asset #4)

> **S3 outcome (as-built, 2026-06-14).** Smoke and fire are wired:
> `BackgroundContainer.ActivateSFX("smoke"/"fire")` dispatches to per-venue
> `BackgroundRoot.SetSmoke` / `SetFire` (null-guarded; venues without those roots
> stay lights-only), and `DeactivateAllSFX` clears lights+smoke+fire on the song
> boundary. Crossings are **performance-only** (D-S3-7): the `StartingSongHype`
> seed no longer fires them. The proposed `SfxStageCrossedEvent` + `StageVfxController`
> bus consumer was **not** built (D-S3-6=A) — stage VFX stay on the direct
> `ActivateSFX` path; revisit only if audio needs the signal.
>
> **TODO (visual polish, not blocking).** Smoke/fire currently pop in as static
> sprites and read poorly against the rotating stage lights. They need an
> appear/loop animation (fade-in + drift/flicker) to match the lights' motion.

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

## 5A. Audio subsystem → moved

> **Migrated 2026-06-15 (M-AUDIO-MIX).** The audio subsystem (two-paths-one-sink SFX,
> `AudioActionType` vs `SensorySfxType` key split, `SoundBankSO` inventory + coverage, null-safety)
> is now governed in **`systems/SSoT_Audio.md §3`**, together with the music-mix model (§4),
> persistence (§5), and the audio boundary (§6). The S3-audio decisions (D-SA-1..7) are recorded
> there, as are the AUDIO-SFX-FIX changes (opt-in-by-type card SFX, caller-controlled jitter,
> app-wide SFX level with the UI bus included). This planning doc keeps the bus/FT/audit material
> (§3, §4) and the §D2 directive; the audio implementation home is `SSoT_Audio.md`.

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

# Design_Tutorial_System_v0_1 — ALWTTT

**Status:** **Implemented in S4 (2026-06-17).** Smoke suite complete (ST-S4-1..11 + QUEUE/PERSIST/RESET/REVISIT/OPP/NODIR/GATE; D-S4-DEDUP double-show fix). Originally active design — all implementation landed in S4 (tutorial controller + overlay + bus-event extension + dialogue authoring). The 2026-05-23 reframe originally split this across S2 (event-bus consumer wiring) and S4 (authoring); S2 closed 2026-06-14 with `TutorialController` deferred to S4 (D-S2-5), so S4 is the single implementation home. *Updated 2026-06-16 (TUT-JAM-SEQ); 2026-06-17 (S4 closure).*
**Scope:** Tutorial system for ALWTTT, demo cut (S4) + vertical slice (S6-S8).
**Classification:** `reference (planning)` — **not a SSoT**. Becomes runtime-authoritative when shipped; this doc is then retained as historical rationale.
**Created:** 2026-05-23

**Rule:** Tutorial-as-mandatory is Standing Directive #3 (`planning/Design_Project_Directives_v0_1.md §D3`). Every demo-cut feature MUST have tutorial coverage by S4 closure. Every Phase C feature MUST have tutorial coverage by S8 closure. This doc defines the system that makes the rule implementable.

---

## 1. Purpose

The 2026-05-23 planning reframe surfaced a demo-cut blocker: playtest feedback indicates the rules feel too complex without explanation. The new character in asset image 1 (confirmed as ship pilot / band manager per D-RUN-5) becomes the diegetic voice for tutorials.

This doc captures the design intent and the five decisions locked at reframe (D-TUT-1..5), so that S4 executes against a shared baseline rather than rederiving scope each time. The jam (live composition) is additionally taught as a guided sequence — see §6A (D-TUT-6..11, added 2026-06-16).

---

## 2. Scope (D-TUT-1 = basic mechanics only, extensible infra)

### In scope for demo cut (S4 authoring)

- First-time-played explanations for the player's first encounter with each core mechanic.
- ~5-8 trigger points (full inventory in §6 below): action card play, composition card play, inspiration spend, hand draw, audience reaction, song-end resolution, status applied, win condition.
- Skip-mid-dialogue (D-TUT-2).
- Revisit-from-pause (D-TUT-2).

### In scope for vertical slice (S6-S8 extension, per D-TUT-5)

- ~5 additional dialogues covering run-structure mechanics: ship hub intro, reward selection, audience state machine, first boss encounter, run-complete sequence.
- Same infrastructure as demo cut; only new trigger IDs + new dialogue assets.

### Out of scope (deferred — not for demo cut, not for vertical slice)

- Multi-step branching tutorials.
- Conditional / state-dependent tutorial paths (beyond first-time-trigger).
- Localization infrastructure beyond what the project already has.
- In-game help / encyclopedia system (separate concern).
- Audio narration on dialogues.
- Per-mood pilot portrait variants (vertical slice may revisit; deferred at reframe).

### Extensible-infra principle

D-TUT-1 splits "basic mechanics only" (content scope) from "extensible infra" (architecture scope). The trigger model (§3) and the dialog presentation (§4) MUST admit new dialogues by SO-authoring or data-table extension without code change. New triggers wired through the Sensory Event Bus (§3) — no per-mechanic bespoke wiring inside `TutorialController`.

> **D-TUT-6 exception (2026-06-16, TUT-JAM-SEQ).** The jam (live composition) is the one core mechanic taught as a layered **guided sequence** rather than a single first-time popup — see §6A. This is a *scoped* exception to D-TUT-1 "basic mechanics only" (the jam is the core mechanic, layered); it is **not** license to inflate coverage. The authoring discipline (few dialogs, sharp text) still governs every beat's text.

---

## 3. Trigger model (D-TUT-3 = first-time HashSet)

### Mechanism

- `TutorialController` holds `HashSet<string> firedDialogs`, in-memory at runtime and serialized via `PersistentGameplayData` for cross-session persistence.
- Each dialogue has a unique string ID (e.g. `"tut_first_action_card"`). IDs are author-stable; renaming an ID retires the dialogue from the player's set and re-fires it on next encounter, which is acceptable as an authoring escape hatch but should not be exercised without intent.
- On each potential trigger: controller checks `firedDialogs.Contains(id)` → if true, skip silently; if false, show the dialogue and add the ID to the set.

### Persistence

- Demo cut: persist across sessions. Player completes the tutorial once; no re-fire on subsequent runs.
- Vertical slice: same semantics; the set grows monotonically as new mechanics are encountered for the first time.

### Hooks (relationship to Sensory Event Bus)

Tutorial triggers consume events from the Sensory Event Bus (`planning/active/Design_Sensory_Contract_v0_1.md §3`). `TutorialController` subscribes to event types it cares about (CardPlayedEvent, StatusAppliedEvent, AudienceReactionEvent, etc.) and checks the candidate trigger ID inside the handler.

**S1 / S2 / S4 ordering note (updated 2026-06-16, TUT-JAM-SEQ).** S1 (B3-slate-F) landed before S2 (event bus). S2 (closed 2026-06-14) introduced the bus and migrated S1's direct FT calls to bus events, **but shipped only three event types** — `AudienceReactionEvent`, `SongEndVibeEvent`, `SfxStageCrossedEvent` — and **deferred the tutorial controller to S4** (D-S2-5). The tutorial controller therefore comes online in **S4**, which must additionally **extend the bus** with the events the tutorial triggers consume (`CardPlayedEvent`, an Inspiration-delta signal, a per-loop-gain signal). The originally-assumed "S4 registers dialogues against *existing* bus events" does **not** hold — most consumed events are not yet shipped. See §6A's event-reality check and the S4 implementation plan.

### Reset path

Pause menu "Reset tutorials" button clears the `firedDialogs` set (with a confirmation prompt to avoid accidental resets). Useful for tutorial-content authoring iteration and for testers requesting a re-run.

---

## 4. Presentation (D-TUT-4 = portrait + dialog box, Neow-style)

### Layout

- **Portrait box.** Bottom-left or upper-left of the screen (S4 layout pass picks one consistently). Asset image 1 (pilot character) renders here.
- **Dialog box.** Adjacent to the portrait (right side or screen-center if portrait is corner-mounted). Text appears in a typed-out reveal animation (per Slay the Spire's Neow convention).
- **Continue prompt.** Click anywhere / press Enter to advance to next page or to dismiss.
- **Skip button.** Top-right corner of dialog box; single click dismisses immediately.

### Pilot portrait asset

- Single static portrait sufficient for demo cut (no expression variants).
- Vertical slice may want mood / state variants (e.g. proud after boss win, concerned during a tough stretch); deferred at reframe — no asset-pipeline pre-commitment.

### Behavior

- Tutorials are modal. While shown:
  - Cards become undraggable.
  - Audience turns are suspended.
  - Animations / SFX in progress continue but no new gameplay events fire.
- Dismissing (continue or skip) resumes game state immediately. No fade.
- **Single-modal queue (D-TUT-10, 2026-06-16).** At most one tutorial modal is shown at a time. If more than one first-time trigger fires within a single event resolution (e.g. a composition card play that both enters the jam *and* spends inspiration — beats 1 and 2), the controller **queues** them and shows them sequentially in **authored priority order** on dismiss. This is a FIFO presentation queue, not an ordered runtime state machine (see §6A architecture note).

---

## 5. UX (D-TUT-2 = skip + revisitable from pause)

### Skip

- Per-dialogue skip button (top-right of dialog box).
- Skip records the dialogue as fired (same as a normal completion). Player who skips does not re-encounter it; they revisit via pause menu if they want to re-read.

### Revisit from pause

- New "Tutorials" submenu in pause UI.
- Lists all fired dialogues, grouped by category (Cards / Meters / Jam / Audience / Run / Boss).
- Click → replay dialogue. Replay is modal (same presentation as first-time), but does not affect any gameplay state and does not re-trigger.
- **Already-fired-only (D-TUT-11, 2026-06-16).** The submenu lists **only dialogues the player has already encountered** — no read-ahead / browse-everything. This matches the "teach to act, not to fully understand" principle (§6A) and avoids an awkward empty/greyed state for opportunistic beats (e.g. beat 5, which may never fire). A forward-reference encyclopedia remains the deferred D-TUT-7 rich codex (Phase C).

### Reset

- "Reset tutorials" button at bottom of submenu.
- Confirmation prompt: "This will re-show every tutorial on its next trigger. Continue?"
- Clears the entire `firedDialogs` HashSet on confirm.

---

## 6. Demo cut dialogue list (DRAFT — not authored)

The following is a trigger inventory and topic outline. **Final dialogue text is S4 work** — this table is a scope-locking artifact, not a script.

| ID | Trigger event | Topic | Tone |
| --- | --- | --- | --- |
| `tut_welcome_to_gig` | first gig start (GigStartedEvent) | Intro + orientation | warm, encouraging |
| `tut_first_action_card` | first action-card play (CardPlayedEvent where IsAction) | What action cards do; when to play them | brief, mechanical |
| `tut_first_composition_card` | first composition-card play (CardPlayedEvent where IsComposition) | What composition cards do; how they shape the song | brief, music-flavored |
| `tut_first_inspiration_spend` | first card play that spends >0 inspiration | Inspiration as the play resource | brief |
| `tut_first_audience_action` | first audience turn after first SongHype stage crossing | Audience pressure; Vibe meter; convincing | reassuring |
| `tut_first_song_end` | first SongEndedEvent | Loop score → SongHype → Vibe conversion | clarifying |
| `tut_first_status_applied` | first StatusAppliedEvent (musician OR audience) | Status effects exist; icons explain them; right-click for detail | one-line |
| `tut_first_gig_won` | first GigOutcomeEvent (win) | Win recognition + tease of run structure (full run-context lands in `tut_ship_hub_intro` at S6) | celebratory |

8 entries drafted. **Some may merge or drop during S4 authoring** if playtest shows the player learns by doing for a given mechanic. Authoring discipline: prefer fewer dialogues with sharper text over more dialogues with diluted text.

> *(2026-06-16, TUT-JAM-SEQ)* The composition mechanics are no longer taught as loose popups; they are taught as a **guided sequence over the first song** — see **§6A (D-TUT-6)**. Three rows above (`tut_first_composition_card`, `tut_first_inspiration_spend`, `tut_first_song_end`) are folded in as beats 1 / 2 / 6 of that sequence; the rest stay standalone.

---

## 6A. Jam tutorial sequence (D-TUT-6 / D-TUT-8)

**Principle.** The jam (live composition: `CompositionSession` / `SongCompositionUI` / `MidiMusicManager`) is the core mechanic, taught in layers over the demo's **first song** (1 part, 4 loops per `Design_Demo_Cut_v1.md §1.1`). Guiding rule: **teach to act, not to fully understand** — minimum on first contact; depth deferred to the revisit menu (§5) / a later codex. This is a **scoped exception to D-TUT-1 "basics only"** (the jam is the core mechanic, layered); it is **not** license to inflate coverage — the authoring discipline (few dialogs, sharp text) still governs the text.

**Architecture (D-TUT-10).** The "sequence" is an **authoring / narrative** construct, not a runtime state machine. The six beats are **independent first-time triggers** that fire in natural gameplay order, reusing the D-TUT-3 `firedDialogs` HashSet — gameplay causality enforces the arc (you cannot reach song-end before loops play, cannot gain per-loop inspiration before playing a composition card, etc.). The only added controller requirement is the **single-modal queue** (§4): if multiple first-time triggers fire in one event resolution, show them sequentially in authored priority order. No ordered step-state.

**Beats.** Final text is S4 work; the table below is a scope-locking artifact, not a script.

| # | Trigger ID | Bus event (status) | Theme | Tone | Text scope (one-page max) |
| --- | --- | --- | --- | --- | --- |
| 1 | `tut_first_composition_card` *(reused)* | `CardPlayedEvent` where IsComposition — **shipped S4** (producer `DeckManager.OnCardPlayed`) | Entering the jam: composition cards build the song live | brief, music-flavored | "This is the jam — you build the song as you play. Composition cards add to it (a track, an instrument). Play one and hear it join." Do **not** explain inspiration / tracks / feedback yet. |
| 2 | `tut_first_inspiration_spend` *(reused)* | `CardPlayedEvent` (InspirationCost > 0) — **shipped S4** (reuses CardPlayedEvent; no separate meter event, D-S4-BUS=B) | Inspiration is the play cost | brief | "Composition cards cost Inspiration (song-panel counter). You start with 3; spend it to shape the song." Gain side deferred to beat 3. |
| 3 | `tut_first_loop_inspiration` *(NEW)* | `LoopResolvedEvent` (InspirationGainedThisLoop > 0) — **shipped S4** (bridges `CompositionSession.LoopFinished`) | Tracks pay you back each loop | encouraging | "Every track you've added feeds Inspiration back each time the loop comes around — build the song and it funds itself." Grounded in `TrackEntry.inspirationGenerated` + `CompositionSession.HandleLoopFinished`. |
| 4 | `tut_first_sfx_stage` *(NEW)* | `SfxStageCrossedEvent` — **SHIPPED (S3-audio)** | The song's building → the stage reacts | hype | "The crowd's heating up — the stage lights as the song's hype climbs. Keep it going for bigger reactions." Do **not** teach the +Vibe math. Distinct from audience-turn pressure (`tut_first_audience_action`). |
| 5 | `tut_first_sound_card` *(NEW, **OPPORTUNISTIC**)* | `CardPlayedEvent` + `CompositionCardClassifier` (tempo / modulation) — **shipped S4**; cards in demo deck (Starter_Deck v1.1: Push It / Half Time / Key Lift; Default Mode stripped of tempo in S4, so it no longer triggers this) | "Sound cards" reshape the music, not the meters | brief, curious | "That card changed the song's speed / its key — sound cards reshape the music itself, not your meters." **Must not promise an audible direction** (per MGP-ALWTTT-MOD-DIR-1, Key Lift's octave is non-deterministic): say "changes the key," not "raises the key." May never fire (cards not guaranteed drawn / present) — revisit-only fallback. |
| 6 | `tut_first_song_end` *(reused)* | `SongEndVibeEvent` — **SHIPPED (S2)** | The payoff: everything converts to Vibe | clarifying | "Song's done. The hype you built converts into Vibe on the crowd — Vibe is how you win them over." The jam→combat bridge. |

**Event reality check — RESOLVED (S4, 2026-06-17).** All six beats now have shipped bus events. S4 added `CardPlayedEvent` (beats 1/2/5; producer `DeckManager.OnCardPlayed`; beats 2/5 keyed on InspirationCost / classifier) and `LoopResolvedEvent` (beat 3); beats 4/6 already had `SfxStageCrossedEvent` / `SongEndVibeEvent`. No generic Inspiration-delta `MeterChangedEvent` was added — beat 2 reuses CardPlayedEvent and beat 3 reuses LoopResolvedEvent (D-S4-BUS=B). S4 was bus-extension + overlay + authoring, as planned.

### 6A.1 Reconciliation with the §6 standalone triggers (D-TUT-9 / O1)

| §6 trigger | Disposition | Why |
| --- | --- | --- |
| `tut_welcome_to_gig` | **standalone** | Gig orientation; pre-jam. |
| `tut_first_action_card` | **standalone** | Action cards are combat-side, not the jam. |
| `tut_first_composition_card` | **→ jam beat 1** | Entry point to the jam. |
| `tut_first_inspiration_spend` | **→ jam beat 2** | Inspiration is the jam economy. |
| `tut_first_audience_action` | **standalone** | Audience pressure / Vibe meter = combat-side. Its trigger ("first audience turn after first SongHype stage crossing") shares its *signal source* with beat 4 but fires on the **next audience turn**, not on the crossing — the modal queue keeps them ordered; they are **not** merged (stage escalation ≠ audience pressure). |
| `tut_first_song_end` | **→ jam beat 6** | Song-end conversion is the jam payoff. |
| `tut_first_status_applied` | **standalone** | Status system; cross-cutting. |
| `tut_first_gig_won` | **standalone** | Win condition. |

Result: **3 of 8** fold into the jam sequence (beats 1 / 2 / 6); **5** stay standalone; **3 new** beats added (3 / 4 / 5). Net new IDs: `tut_first_loop_inspiration`, `tut_first_sfx_stage`, `tut_first_sound_card`.

### 6A.2 Decisions registered

- **D-TUT-6 = A** — the jam gets its own guided sequence over the 1st song (scoped exception to D-TUT-1).
- **D-TUT-7 = C** — no rich illustrated codex now; the D-TUT-2 revisit menu serves as "codex-lite"; rich codex deferred to Phase C / post-demo unless playtest forces it.
- **D-TUT-8 = A** — sequence runs **live** on the 1st real song, fired by Sensory Event Bus events; not a scripted sandbox. Beat 5 (tempo / key) is opportunistic.
- **D-TUT-9 = A** (O1) — fold / standalone split above.
- **D-TUT-10 = A** (O2) — independent first-time triggers in natural order (reuse the D-TUT-3 HashSet) + single-modal queue (§4); **no** ordered step-state.
- **D-TUT-11 = A** (O3) — revisit menu stays **already-fired-only** (no read-ahead). A browse-everything reference remains the deferred D-TUT-7 rich codex.

### 6A.2b S4 implementation decisions (2026-06-17)

- **D-S4-BUS = B** — beats 1/2/5 ride `CardPlayedEvent`; beat 3 rides `LoopResolvedEvent` (bridge of `CompositionSession.LoopFinished`). No `MeterChangedEvent` in S4 (avoids over-/mis-fire on session-start + part-advance resets).
- **D-S4-SRC = A** — pure-bus: `StatusAppliedEvent`, `GigStartedEvent`, `GigOutcomeEvent`, `AudienceTurnStartedEvent` added; one subscription surface (8 events) in `TutorialController`.
- **D-S4-PRODUCER** — `CardPlayedEvent` producer is `DeckManager.OnCardPlayed` (the one site reached by both action and composition cards; corrects `Design_Sensory_Contract §3`).
- **D-S4-DEDUP = B** — `TutorialController` tracks the on-screen trigger id (`_showingId`) and skips re-enqueue of the showing dialog, closing a double-show hole (a dialog is dequeued before it is marked fired). Found in S4 smoke testing; `SongEndVibeEvent` fires once per audience member by design, which exposed it.
- **D3 = B** — spotlight is a hand-written UI cutout shader (`ALWTTT/UI/TutorialSpotlight`), a positionable inverted sprite-mask; chosen over four-quad / shadergraph for text-authorability + version portability.
- **D4 = A** — bubble auto-placed opposite the highlight *(gated behind `autoPlaceBubbleBySide`, default OFF after the captain-flip fix; default is upright bottom-left per D7)*. **D5 = SO** (dialogue data). **D6** — portrait at the chosen path. **D7** — bottom-left portrait corner.
- **Gate** — audience turn cooperatively suspended (`TutorialModalGate`, one `WaitUntil` in `AudienceTurnRoutine`) + `HandController` drag-lock. The composition loop / MIDI is **not** frozen under a modal (avoids desync); loop events queue.
- **D-S4-PAUSE = A** — no jam-loop pause for the demo. Option C handed to MidiGenPlay as MGP-PAUSE (parallel, non-blocking); ALWTTT-side MGP-PAUSE-ALWTTT blocked on it.
- **D-S4-HAND = A** — no curated/locked opening hand for tutorial pacing in S4; option B is OPTIONAL post-S5.
- **D-S4-REVISIT-TEST = A** — RESET/REVISIT exercised via a throwaway `TutorialDevHook` (removed at close), since no in-game pause/settings menu exists yet to host `TutorialRevisitPanel`.

### 6A.3 Inherited opens — now S4 (migrated from §9; S2 closed without touching them)

SO vs JSON for dialogue data · portrait sprite asset path · layout corner (bottom-left vs upper-left). Plus the **NEW** S4-open infra decisions U1 (highlight-mask technique — gated on `GigCanvas` render mode), U2 (captain-bubble placement / orientation), and D-S4-BUS (bus-event granularity) — see the S4 implementation plan (TUT-JAM-SEQ deliverables).

---

## 7. Vertical slice extension (D-TUT-5 = ~5 dialogues spread S6-S8)

Per Standing Directive #3, every Phase C feature gets tutorial coverage. Draft inventory:

| ID | Phase C session | Topic |
| --- | --- | --- |
| `tut_ship_hub_intro` | S6 | First entry to ship hub between gigs. Pilot introduces themselves as crew / manager. Sets up the run-structure framing the demo deliberately deferred. |
| `tut_first_reward_choice` | S6 | Reward selection mechanics (scoped to whatever reward flow S6 ships, even if stub). |
| `tut_audience_state_machine` | S7 | "See that crowd? They start neutral. Push them right and they'll vibe; push wrong and they'll heckle." Explains idle / hostile / vibing states. |
| `tut_first_boss_encounter` | S8 | "This is the headliner — they hit harder and have tricks the regular crowd doesn't." Frames the boss as a phase-shift encounter. |
| `tut_run_complete` | S8 | "You finished the run — that's a real gig." Closes the loop on the run-structure framing started at `tut_ship_hub_intro`. |

5 entries reserved for Phase C. IDs claimed now so S4 infrastructure can validate the registry on load; **dialogue text is authored per-session** (S6 ships its two, S7 ships its one, S8 ships its two).

---

## 8. Acceptance / DoD per Standing Directive #3

### Demo cut (S4 closure)

> **Status: all items below met — S4 closed 2026-06-17.** Smoke suite complete (ST-S4-1..11 + QUEUE/PERSIST/RESET/REVISIT/OPP/NODIR/GATE PASS; D-S4-DEDUP=B double-show fix; 9-negative code-verified per D1=A; 11 dialogues authored + seeded). Checklist retained for historical reference.

- [ ] 5-8 demo-cut tutorial dialogues authored and registered against bus events — including the 6-beat jam sequence (§6A).
- [ ] First-time-trigger fires on first encounter of each mechanic. Validated by smoke test per dialogue.
- [ ] Single-modal queue: two first-time triggers in one resolution show sequentially in authored order, no stack / soft-lock (D-TUT-10).
- [ ] Opportunistic beat 5 (`tut_first_sound_card`) stays unfired and the run completes cleanly when no tempo / modulation card is played.
- [ ] Skip button works mid-dialogue (no soft-locks, no event leaks).
- [ ] Pause-menu revisit lists all fired dialogues (already-fired-only, D-TUT-11) and replays them correctly.
- [ ] Reset clears the HashSet with confirmation; subsequent triggers re-fire.
- [ ] Persistence: dialogues do not re-fire across sessions.
- [ ] All triggers wired through the Sensory Event Bus — **including the bus events S4 must add** for beats 1 / 2 / 3 / 5 (`CardPlayedEvent`, Inspiration-delta, per-loop-gain).
- [ ] Standing Directive #3 satisfied: every demo-cut feature has tutorial coverage by S4 close. Verified by walking the `Design_Demo_Cut_v1.md §2.4` coverage matrix and confirming a tutorial trigger exists for each row.

### Vertical slice (S8 closure)

- [ ] 5 Phase C dialogues authored (ship hub intro, reward, audience state machine, first boss, run complete).
- [ ] All Phase C features have tutorial coverage by S8 close. Verified by walking `Design_Vertical_Slice_v0_1.md §3` per-session DoD lists and confirming a tutorial trigger exists per new mechanic.
- [ ] Standing Directive #3 remains satisfied across Phase C.

---

## 9. Open questions — now S4 (was S2 / S4; reframed 2026-06-16)

> S2 closed 2026-06-14 with the tutorial controller deferred to S4 (D-S2-5), so the items below that were tagged "S2" are now **S4** opens.

- Asset path conventions for portrait sprite (**S4** — likely `Assets/Resources/Data/Tutorial/Portraits/`).
- ScriptableObject vs JSON for dialogue data (**S4**). Lean SO for consistency with `CardDefinition` / `StatusEffectSO` patterns unless dialogue volume justifies the editor-iteration overhead of JSON.
- Back-button on multi-page dialogues (S4 — only matters if any S4 dialogue exceeds one page).
- Layout pass picking portrait corner (S4 — bottom-left vs upper-left consistency).
- Localization seam (deferred — current project has no localization infrastructure; deferring matches Out-of-Scope §2).
- **NEW (S4 open, TUT-JAM-SEQ):** U1 highlight-mask technique (confirm `GigCanvas` render mode first); U2 captain-bubble placement / orientation; D-S4-BUS bus-event granularity. Detailed in the S4 implementation plan.

---

## 10. Cross-references

- `planning/active/Design_Sensory_Contract_v0_1.md §3` (event bus; tutorial as consumer; only 3 event types shipped — see §6A event-reality check).
- `planning/active/Design_Vertical_Slice_v0_1.md §9` (tutorial coverage during Phase C).
- `planning/Design_Project_Directives_v0_1.md §D3` (Tutorial-as-mandatory standing directive — this doc is its operational expansion).
- `planning/active/Design_Demo_Cut_v1.md §2.4` (demo-cut coverage matrix — Tutorial coverage rows, incl. the two TUT-JAM-SEQ additions).
- `Roadmap_ALWTTT.md §5.5` (Phase B DoD criterion: tutorial coverage).
- `Roadmap_ALWTTT.md §7` (Phase C; ship hub + tutorial extension).
- `CURRENT_STATE.md §1` (S4 = next-active; the single tutorial-implementation home after the S2→S4 reframe).
- `changelog-ssot.md` (2026-06-16 TUT-JAM-SEQ entry — design decisions D-TUT-6..11).

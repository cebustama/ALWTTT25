# Design_Tutorial_System_v0_1 — ALWTTT

**Status:** Active design — opens for S2 (foundation / event-bus consumer wiring) and S4 (dialogue authoring) per the 2026-05-23 planning reframe.
**Scope:** Tutorial system for ALWTTT, demo cut (S4) + vertical slice (S6-S8).
**Classification:** `reference (planning)` — **not a SSoT**. Becomes runtime-authoritative when shipped; this doc is then retained as historical rationale.
**Created:** 2026-05-23

**Rule:** Tutorial-as-mandatory is Standing Directive #3 (`planning/Design_Project_Directives_v0_1.md §D3`). Every demo-cut feature MUST have tutorial coverage by S4 closure. Every Phase C feature MUST have tutorial coverage by S8 closure. This doc defines the system that makes the rule implementable.

---

## 1. Purpose

The 2026-05-23 planning reframe surfaced a demo-cut blocker: playtest feedback indicates the rules feel too complex without explanation. The new character in asset image 1 (confirmed as ship pilot / band manager per D-RUN-5) becomes the diegetic voice for tutorials.

This doc captures the design intent and the five decisions locked at reframe (D-TUT-1..5), so that S2 (event-bus consumer / infrastructure wiring) and S4 (dialogue authoring) execute against a shared baseline rather than rederiving scope each time.

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

**S1 / S2 ordering note.** S1 (B3-slate-F) lands BEFORE S2 (event bus). For S1's `ResolveLoopEffect` work, per-audience FT emission uses direct calls to the existing FT pipeline. S2 introduces the bus and migrates S1's direct calls to bus events. Tutorial controller comes online during S2 as the first new bus consumer. S4 authoring then registers dialogues against existing bus events.

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

---

## 5. UX (D-TUT-2 = skip + revisitable from pause)

### Skip

- Per-dialogue skip button (top-right of dialog box).
- Skip records the dialogue as fired (same as a normal completion). Player who skips does not re-encounter it; they revisit via pause menu if they want to re-read.

### Revisit from pause

- New "Tutorials" submenu in pause UI.
- Lists all fired dialogues, grouped by category (Cards / Meters / Audience / Run / Boss).
- Click → replay dialogue. Replay is modal (same presentation as first-time), but does not affect any gameplay state and does not re-trigger.

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

5 entries reserved for Phase C. IDs claimed now so S2 infrastructure can validate the registry on load; **dialogue text is authored per-session** (S6 ships its two, S7 ships its one, S8 ships its two).

---

## 8. Acceptance / DoD per Standing Directive #3

### Demo cut (S4 closure)

- [ ] 5-8 demo-cut tutorial dialogues authored and registered against bus events.
- [ ] First-time-trigger fires on first encounter of each mechanic. Validated by smoke test per dialogue.
- [ ] Skip button works mid-dialogue (no soft-locks, no event leaks).
- [ ] Pause-menu revisit lists all fired dialogues and replays them correctly.
- [ ] Reset clears the HashSet with confirmation; subsequent triggers re-fire.
- [ ] Persistence: dialogues do not re-fire across sessions.
- [ ] All triggers wired through the Sensory Event Bus (S2 dependency satisfied).
- [ ] Standing Directive #3 satisfied: every demo-cut feature has tutorial coverage by S4 close. Verified by walking the `Design_Demo_Cut_v1.md §2` coverage matrix and confirming a tutorial trigger exists for each row.

### Vertical slice (S8 closure)

- [ ] 5 Phase C dialogues authored (ship hub intro, reward, audience state machine, first boss, run complete).
- [ ] All Phase C features have tutorial coverage by S8 close. Verified by walking `Design_Vertical_Slice_v0_1.md §3` per-session DoD lists and confirming a tutorial trigger exists per new mechanic.
- [ ] Standing Directive #3 remains satisfied across Phase C.

---

## 9. Open questions deferred to S2 / S4

- Asset path conventions for portrait sprite (S2 — likely `Assets/Resources/Data/Tutorial/Portraits/`).
- ScriptableObject vs JSON for dialogue data (S2). Lean SO for consistency with `CardDefinition` / `StatusEffectSO` patterns unless dialogue volume justifies the editor-iteration overhead of JSON.
- Back-button on multi-page dialogues (S4 — only matters if any S4 dialogue exceeds one page).
- Layout pass picking portrait corner (S4 — bottom-left vs upper-left consistency).
- Localization seam (deferred — current project has no localization infrastructure; deferring matches Out-of-Scope §2).

---

## 10. Cross-references

- `planning/active/Design_Sensory_Contract_v0_1.md §3` (event bus; tutorial as consumer).
- `planning/active/Design_Vertical_Slice_v0_1.md §9` (tutorial coverage during Phase C).
- `planning/Design_Project_Directives_v0_1.md §D3` (Tutorial-as-mandatory standing directive — this doc is its operational expansion).
- `planning/active/Design_Demo_Cut_v1.md §2` (demo-cut coverage matrix — Tutorial coverage row).
- `Roadmap_ALWTTT.md §5.5` (Phase B DoD criterion: tutorial coverage).
- `Roadmap_ALWTTT.md §7` (Phase C; ship hub + tutorial extension).
- `CURRENT_STATE.md §3` (next-active S1-S8 sequence; S2 is the foundation session for this doc).

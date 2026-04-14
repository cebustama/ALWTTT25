# CURRENT_STATE — ALWTTT

This file tracks the currently validated project baseline, active work, and immediate next steps.

---

## 1. Project foundation

### Combat MVP — complete (2026-03-23)
- Deck/hand pipeline operating in play mode.
- All four card effect types working end-to-end: `ModifyVibe`, `ModifyStress`, `ApplyStatusEffect`, `DrawCards`.
- Composure absorption via `ApplyIncomingStressWithComposure`.
- Breakdown → Cohesion−1 + Stress reset + Shaken application. LoseGig at Cohesion ≤ 0.
- Exposed stress multiplier and Feedback DoT (musician-only) wired.
- Tick timing: PlayerTurnStart (musicians) + AudienceTurnStart (audience).
- Six SO status entries in catalogue: `flow`, `composure`, `exposed`, `feedback`, `choke`, `shaken`.
- All Phase 4 decisions (A–H) resolved and implemented.

### Composition / music surface — exists, not yet validated end-to-end
- `GigManager`, `MidiMusicManager`, `CompositionSession`, `SongConfigBuilder`, `LoopScoreCalculator`.
- `test_pass_turn` composition card functional — routes through session pipeline and advances turn.
- CompositionSession bypass of phase machine documented (see `SSoT_Runtime_Flow`).
- Not yet tested: composition cards with real gameplay effects producing audible song changes.

### Status icon pipeline — SO-based (M1.2, complete 2026-04-14)
- Sprite authority on `StatusEffectSO.IconSprite`. Lookup asset (`StatusIconsData`) removed.
- `CharacterCanvas` subscribes to `StatusEffectContainer` events and renders icons directly from the container's definition.
- Lazy icon lifecycle: created on first application, destroyed on clear. Stack count text updates on every change.
- `BindStatusContainer` wiring on `MusicianBase.BuildCharacter` and `AudienceCharacterBase.BuildCharacter`.
- `StatusEffectSO` auto-renames its asset file to `StatusEffect_{DisplayName}_{EffectId}` on change.
- `StatusEffectCatalogueSO` validation deferred via `delayCall` — eliminated spurious "empty StatusKey" errors from import-worker serialization-order races.
- See `SSoT_Status_Effects.md` §3.3 for the authoritative specification.

### Editor authoring tools
- **Card Editor** (`CardEditorWindow`) — single card authoring, JSON batch import. Menu: ALWTTT → Cards → Card Editor.
- **Deck Editor** (`DeckEditorWindow`) — deck authoring with JSON import (reference existing + create new cards), catalogue browser, save/save-as, GigSetup registration, JSON export. Core functional; polish items remain (see §2).
- **Status Effect Wizard** (`StatusEffectWizardWindow`) — status SO authoring against the catalogue.
- Supporting services: `DeckJsonImportService`, `DeckCardCreationService`, `DeckValidationService`, `DeckAssetSaveService`.
- The `ALWTTT_DeckEditorWindow_Roadmap_Proposal.md` phases 0–6 are substantially complete. Remaining items captured in `Roadmap_ALWTTT.md` M1.

### Documentation
Governance migration complete. All subsystem SSoTs active and replacement-ready, including `SSoT_Editor_Authoring_Tools.md` (activated 2026-04-08).

---

## 2. Active work

### QA-readiness gap — not yet addressable without Dev Mode
The Combat MVP is technically complete but not yet testable in a playtesting sense. Normal turns complete and the baseline loop runs, but the following cannot be iterated without tooling:

- Composition card asset authoring and audible verification during live loops.
- Audience reaction and ability transparency (what did the audience just do and why).
- Card Inspiration value balancing.
- Card effect balance tuning.
- Composition card music generation tuning for real-time song-loop gameplay.
- Multi-turn status states (Choke decay, Shaken expiry across a song cycle, Feedback DoT accumulation).

**Dev Mode (M1.5) is the unblocker.** Without infinite turns, runtime stat editing, and card spawning, playtest iteration has no practical loop. This is now the critical path for moving the project from "technically works" to "QA-ready."

### Deck Editor — polish pass (non-blocking)
Core functionality is implemented. Remaining items:
- Better catalogue filters (by musician, by kind, by effect type — currently only action/composition + text search).
- Card preview info in staged card list (effect summary, cost, kind badge — currently shows card name only).
- Cross-tool integration: Open in Card Editor, Ping Card in Project.

### Card effect description text — known bug
Card tooltips generated for `ApplyStatusEffect` effects currently show `CharacterStatusId` enum names (e.g. `DamageUpFlat`) instead of `StatusEffectSO.DisplayName` (e.g. `Flow`). Pending fix in a small follow-up batch; needs `ApplyStatusEffectSpec.cs` or the card description builder attached.

### Editor tooling documentation — complete
`SSoT_Editor_Authoring_Tools.md` created and registered. Covers Card Editor, Deck Editor, Status Effect Wizard, Chord Progression Catalogue Wizard, all supporting services, file locations, and known gaps mapped to M1 tasks.

---

## 3. What is next

1. **M1.5 Dev Mode gig scene** — the critical path for QA-readiness. Sandbox for card and composition testing. Infinite turns, spawn cards into hand, edit gameplay variables (Inspiration, Score, musician/audience stats), toggle encounter modifiers, transparent audience reaction display. Primary testing bed for card balance, composition card generation, and audience tuning.

2. **M1.7 Character hover highlight** (new, code-only, small) — sprite outline or color tint on pointer enter/exit via `CharacterBase.OnPointerEnter/Exit`. Game-feel prerequisite for general audience testing.

3. **M1.8 Status icon animations** (new, code-only, small) — appear/change/expire animations on `StatusIconBase` via coroutines. Game-feel prerequisite for general audience testing.

4. **M1.3 Tooltip pipeline extension** — SO-derived status tooltip content, card effect descriptions, composition card musical modifier summary.

5. **M1.1 Deck Editor polish** — deferred to lower priority than game-feel and dev tooling.

6. **M2 Composition session validation** — unblocked by M1.5. First real composition testing deck, end-to-end composition card testing with audible song changes, timing and deck speed design questions.

---

## 4. Open items and risks

### Open items (non-blocking)
- **Shaken restrictions:** status applies and expires correctly; no gameplay gate checks it yet. Design decision still open (intended: cannot play Action cards during action window, 50% Composure penalty).
- **Audience Feedback DoT:** no Stress path on `AudienceCharacterBase`. Deferred until audience pressure model expands.
- **Composure penalty during Shaken:** design intent only; not code-enforced.
- **True card copies in decks:** current runtime deduplicates by reference. `BandDeckData` stores a flat card list. If deckbuilding needs multiple copies of a card, the deck contract must evolve (see `Roadmap_ALWTTT.md` future milestones).
- **Card effect text displays enum names:** `ApplyStatusEffect` card text shows `CharacterStatusId` values instead of `StatusEffectSO.DisplayName`. Cosmetic but surfaces in every test. Small follow-up batch.

### Residual risks
- **Legacy `StatusType` coexistence (retained):** legacy enum coexists with the SO container for any non-icon callers. New work goes through SO container only. `BandCharacterStats.ApplyStatus(StatusType)` is marked `[Obsolete]`.
- **CompositionSession bypass untested (retained):** phase machine bypass during composition is documented but not yet tested with a real composition card that has gameplay effects. M2 scope.
- **Multi-turn status validation pending:** Choke decay (T5), Shaken expiry across a song cycle (T7), and Feedback DoT accumulation (T8) could not be reliably validated in M1.2 without Dev Mode. Icons and wiring are code-verified; full runtime validation deferred to M1.5 closure.

---

## 5. Docs that must be edited next

After the next meaningful technical change, edit:
- the primary affected SSoT
- `CURRENT_STATE.md` if the active operational slice changed
- `changelog-ssot.md` if meaning/authority changed
- `coverage-matrix.md` only if the primary home changed

Pending new documentation:
- Dev Mode design SSoT — to be created at the start of M1.5 planning. Recommended path: `systems/SSoT_Dev_Mode.md` (active, project-scope).

---

## 6. Working rule

`CURRENT_STATE.md` answers:
- what is the project foundation
- what is active now
- what comes next
- what is blocked or at risk
- which docs need editing next

It does **not** replace subsystem SSoTs.

# Roadmap — ALWTTT

**Status:** Planning only — does not define implementation truth  
**Last updated:** 2026-04-14  
**Rule:** This document tracks recommended work sequencing. It does not override subsystem SSoTs or CURRENT_STATE.

---

## Standing objective

Always maintain a working, showable build. Every milestone should produce something demonstrable to publishers or potential community members.

---

## Completed milestones

### Combat MVP ✅ (closed 2026-03-23)
Full combat loop with four card effect types, six SO statuses, composure/breakdown/cohesion path, tick system, and testing deck validation. See `planning/archive/Roadmap_Combat_MVP_Closure_Actionable.md` for detailed phase history.

### Deck Editor core ✅ (phases 0–6 substantially complete)
JSON import (reference existing + create new cards), staged deck model, catalogue browser with search/filter, save/save-as with validation, GigSetup registration, JSON export. See `planning/archive/ALWTTT_DeckEditorWindow_Roadmap_Proposal.md` for original proposal and decisions.

---

## Milestone 1 — Authoring & Testing Infrastructure

**Goal:** All tools needed to rapidly design, test, iterate, and present cards, statuses, and encounters to general-audience testers.

**Demo pitch:** "Here's how we design cards and test them in real-time — the authoring pipeline that lets us iterate fast, with feedback clear enough that anyone can playtest."

**Priority order (revised 2026-04-14):** The critical path is now game-feel + dev tooling first, polish last. Target ordering:

1. **M1.5** Dev Mode gig scene — the unblocker for all QA-ready testing.
2. **M1.7** Character hover highlight — small, code-only game feel.
3. **M1.8** Status icon animations — small, code-only game feel.
4. **M1.3** Tooltip pipeline extension — content legibility.
5. **M1.1** Deck Editor polish — authoring comfort.

Reasoning: the build is technically complete but not testable by general audiences. Game feel and dev tooling gaps are the reason; Deck Editor polish is not on that critical path.

### 1.1 — Deck Editor polish
- Better catalogue filters (by musician, by kind, by effect type).
- Card preview info in staged card list (effect summary, cost, kind badge).
- Cross-tool integration: Open in Card Editor, Ping Card in Project.
- Final validation pass.

### 1.2 — Status Icons pipeline migration ✅ (closed 2026-04-14)
- ~~Migrate `StatusIconsData` / `StatusIconData` from legacy `StatusType` enum to the SO-based system (`CharacterStatusId` or `StatusEffectSO` reference).~~
- ~~Create or assign icons for the six Combat MVP statuses.~~
- ~~Stack count display on character canvas (musician + audience).~~

**Outcome beyond original scope:** The `StatusIconsData` indirection layer was removed entirely. Sprite authority collapsed onto `StatusEffectSO.IconSprite`. `CharacterCanvas` now reads sprites directly from the `StatusEffectContainer`. Additional polish delivered:
- `StatusEffectSO` auto-renames its asset file to `StatusEffect_{DisplayName}_{EffectId}` on edit.
- `StatusEffectCatalogueSO.OnValidate` deferred via `delayCall` and skips import-worker runs — fixed spurious "empty StatusKey" errors on prefab selection.
- `BandCharacterStats.ApplyStatus(StatusType)` marked `[Obsolete]`.

**Deferred to M1.5 for validation:** Multi-turn smoke tests (choke decay, shaken expiry across a song cycle, feedback DoT accumulation) — blocked on infinite-turn tooling.

See `SSoT_Status_Effects.md` §3.3 for the authoritative icon pipeline specification.

### 1.3 — Tooltip pipeline extension
- Extend tooltip system to show card effect descriptions (not just keyword-based).
- Add status effect tooltip content (what the status does, current stacks, duration). Source from `StatusEffectSO.DisplayName` and a description field to be added.
- Composition card tooltip info (musical modifier summary).
- Fix card effect description text showing `CharacterStatusId` enum names instead of `StatusEffectSO.DisplayName`.

### 1.4 — Editor tooling documentation ✅
- ~~Create `SSoT_Editor_Authoring_Tools.md` — governed doc covering all editor tools, what they do, how to use them, and current limitations.~~
- ~~Register in `SSoT_INDEX.md` and `coverage-matrix.md`.~~
- Completed 2026-04-08. Doc covers four tools, six supporting services, file locations, known gaps mapped to M1 tasks.

### 1.5 — Dev Mode gig scene ← **critical path**
- New scene or mode toggle on existing gig scene.
- Infinite turns (no forced gig end).
- Spawn any card into hand from a catalogue browser.
- Edit gameplay variables at runtime: Inspiration, LoopScore, SongHype, BandCohesion.
- Edit per-character stats: Stress, MaxStress, Composure stacks, any status stacks.
- Edit audience stats: Vibe, persuasion progress.
- Transparent audience reaction/ability display (what action is being taken, on what target, with what value).
- Toggle encounter modifiers on/off.
- Usable as primary sandbox for card testing, composition testing, and balance tuning.

**Design questions to resolve at M1.5 session start:**
- Scene-level dev panel vs overlay component vs editor inspector window — each has different tradeoffs for build-shipping vs editor-only use.
- Runtime injection surface: how does Dev Mode mutate `GigManager`, `HandController`, `DeckManager`, and `CompositionSession` without leaking dev hooks into release builds?
- Composition testing: can Dev Mode spawn a composition card into a live loop and observe audible mutation? This is the M2 unblocker.

**Proposal:** Start M1.5 with a detailed sub-roadmap session before implementation.

### 1.6 — Archive superseded planning docs ✅ (closed 2026-04-08)
- ~~Add archive headers to `Roadmap_Combat_MVP.md`, `Roadmap_Combat_MVP_Closure_Actionable.md`, and `ALWTTT_DeckEditorWindow_Roadmap_Proposal.md`.~~
- ~~Move to `planning/archive/`.~~

### 1.7 — Character hover highlight (new, 2026-04-14)
- Small, code-only game-feel task. No art assets required.
- Sprite outline, color tint, or scale pulse on `CharacterBase.OnPointerEnter/Exit`.
- Candidate implementations: material property on the SpriteRenderer, secondary sprite child scaled slightly larger, or Unity's built-in outline shader variant.
- Applies to both musicians and audience members.
- Prerequisite for general-audience testing — without it, the scene feels dead on mouse-over.

### 1.8 — Status icon animations (new, 2026-04-14)
- Small, code-only game-feel task.
- `StatusIconBase` gains `PlayAppear()`, `PlayChangeFlash()`, and `FadeOutAndDestroy()` coroutine methods.
- Scale/alpha tweens via hand-rolled `IEnumerator` with `Mathf.SmoothStep` (or DOTween if present).
- `CharacterCanvas.TryCreateIcon` calls `PlayAppear()`.
- `CharacterCanvas.HandleStatusChanged` calls `PlayChangeFlash()` on stack delta.
- `CharacterCanvas.HandleStatusCleared` calls `FadeOutAndDestroy()` instead of immediate `Destroy`.
- Prerequisite for general-audience testing — status changes must feel tactile, not silent.

### Definition of Done
- [ ] Deck Editor: catalogue filters work (by musician, by kind, by effect type)
- [ ] Deck Editor: staged card list shows effect summary, cost, kind badge
- [ ] Deck Editor: Open in Card Editor and Ping Card actions functional
- [x] StatusIconsData migrated to SO-based system; legacy StatusType references removed from icon pipeline
- [x] Icons assigned for all six Combat MVP statuses
- [x] Stack count visible on musician and audience character UI
- [ ] Card tooltips show effect descriptions (not just keywords)
- [ ] Status effect tooltips show name, description, stacks, duration
- [ ] Card effect description text uses `DisplayName` instead of `CharacterStatusId` enum names
- [x] `SSoT_Editor_Authoring_Tools.md` created, registered in index and coverage matrix
- [ ] Dev Mode scene functional: infinite turns, card spawning, stat editing
- [ ] Dev Mode scene tested: can play arbitrary cards and observe all meter/status changes
- [ ] Dev Mode scene surfaces audience reaction/ability transparency
- [ ] Character hover highlight functional on musicians and audience
- [ ] Status icons animate on appear, change, and expire
- [x] Superseded planning docs archived with headers
- [ ] CURRENT_STATE updated to reflect milestone completion

### Demo-readiness check
- **Demonstrable:** Full authoring-to-testing pipeline. Create a card in Card Editor → add it to a deck in Deck Editor → load Dev Mode → spawn it → see effects with animated status icons, hover highlights, and tooltips. General audience testers can drive the game without developer supervision.
- **Viewer sees:** A professional-looking tool pipeline. Fast iteration loop. Clear visual feedback on all game state. Responsive, tactile game feel.
- **Acceptable rough edges:** UI polish on Dev Mode (functional over pretty). Tooltip formatting may be basic.
- **Must fix before showing:** Status icons must display correctly for all six statuses. Card spawning must work reliably. Hover highlights and icon animations must function. Audience reactions must be readable.

---

## Milestone 2 — Composition Session Validation

**Goal:** Prove that composition cards work end-to-end with real music and real decks. The core musical deckbuilder pitch.

**Demo pitch:** "Play cards, hear the song change."

**Unblocked by:** M1.5 Dev Mode. Composition iteration requires the ability to spawn composition cards into live loops and observe audible mutation.

### 2.1 — Real composition testing deck
- Design a varied composition deck with meaningful musical choices.
- Include DrawCards effects for deck speed (ensure players can cycle through composition cards efficiently).
- Test with multiple track/style bundle combinations.

### 2.2 — End-to-end composition testing
- Play composition cards during a live loop and verify audible song changes.
- Validate that `CompositionCardPayload.modifierEffects` produce expected musical mutations.
- Validate that gameplay effects on composition cards fire correctly through the normal card pipeline.
- Test the separation between musical modifier pipeline and gameplay effect pipeline.

### 2.3 — Timing and deck speed design
- Resolve open design question: when can composition cards be played relative to the loop cycle?
- Resolve deck speed question: how many composition cards should a player see per loop? Per song?
- Document findings as design decisions in the relevant SSoTs.

### 2.4 — Composition UI feedback
- Composition card tooltips show musical modifier summary.
- Visual feedback when a composition card changes the active song model.
- Loop/song progress visible during composition phase.

### Definition of Done
- [ ] Real composition deck designed and imported (not a stub deck)
- [ ] Composition cards produce audible changes when played during a live loop
- [ ] Musical modifier and gameplay effect pipelines confirmed independent
- [ ] Deck speed and timing design questions resolved and documented
- [ ] Composition UI shows what changed after a card play
- [ ] At least one complete play-through: start gig → play composition cards → hear song evolve → song ends with score
- [ ] CURRENT_STATE and relevant SSoTs updated

### Demo-readiness check
- **Demonstrable:** A full gig where the player shapes a song through card choices.
- **Viewer sees:** Cards played → song audibly changes → score reflects composition quality.
- **Acceptable rough edges:** Limited musical variety (small bundle set). Basic composition UI.
- **Must fix before showing:** Song must audibly change. Score must reflect card choices. No silent failures.

---

## Milestone 3 — Combat & Status Polish

**Goal:** Combat loop feels complete, readable, and satisfying. All status effects have visible consequences.

**Demo pitch:** "A full gig with clear feedback on every action and consequence."

### 3.1 — Shaken restrictions enforcement
- Resolve design decision: what Shaken prevents (intended: cannot play Action cards during action window).
- Implement Composure penalty during Shaken (intended: 50% reduction).
- Update `SSoT_Status_Effects` and `SSoT_Gig_Combat_Core`.

### 3.2 — Audience pressure expansion
- Implement Stress path on `AudienceCharacterBase`.
- Enable Feedback DoT on audience members.
- Design and implement audience-side status effects (crowd-control, etc.).

### 3.3 — UI readability pass
- Status icons visible and correct for all statuses (builds on M1 migration).
- Meter visibility: Stress bars, Composure shields, Flow/Exposed indicators.
- Turn phase indicators (whose turn, what phase).
- Card play feedback (what happened when a card was played).

### 3.4 — Encounter variety (initial)
- Design 2–3 distinct encounter configurations with different audience rosters and modifiers.
- Test via Dev Mode scene.
- Validate encounter-level victory/failure conditions.

### Definition of Done
- [ ] Shaken restrictions enforced at runtime; design decision documented
- [ ] Composure penalty during Shaken implemented
- [ ] Audience Stress path exists; Feedback DoT works on audience
- [ ] At least one audience-side status effect designed and implemented
- [ ] All meters visually represented on character UI
- [ ] Turn phase clearly indicated in UI
- [ ] 2–3 distinct encounters playable and testable
- [ ] CURRENT_STATE and relevant SSoTs updated

### Demo-readiness check
- **Demonstrable:** A full gig where combat decisions have visible consequences and the encounter feels like a real challenge.
- **Viewer sees:** Health bars, status icons, audience reactions, turn flow, win/loss conditions.
- **Acceptable rough edges:** Limited encounter variety. Audience AI may be simple.
- **Must fix before showing:** All status icons must display. Win/loss must trigger cleanly. No meter desync.

---

## Future milestones (scope only, not yet sequenced)

### Deck Contract Evolution
- True card copies: evolve `BandDeckData` from flat `List<CardDefinition>` to `List<BandDeckEntry>` with copy counts.
- Update `PersistentGameplayData.SetBandDeck` to respect multiplicity.
- Update Deck Editor UI to support honest copy counts.
- This is prerequisite for real deckbuilding progression.

### Progression & Meta
- Run structure (map, node types, rewards).
- Deck evolution across encounters.
- Musician recruitment and band composition.
- Unlock progression.

### Encounter Design & Balance
- Broader encounter roster with varied audience archetypes.
- Boss encounters.
- Difficulty scaling.
- Card balance tuning informed by Dev Mode testing.

### Music & Identity
- Broader track/style bundle library.
- Genre identity through composition choices.
- Band personality / musician traits affecting composition.

### Production & Polish
- Art, animation, sound design.
- Tutorial / onboarding.
- Publisher-ready vertical slice.

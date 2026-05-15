# Roadmap — ALWTTT

**Status:** Planning only — does not define implementation truth  
**Last updated:** 2026-05-15 (Phase B B2.5 closed; B3 expanded with 2-archetype audience pool + Indifference status; Demo cut prep batch added §5.3.5; Marketing stream / Pitch deck refresh added §6)
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

### Milestone 1 — Authoring & Testing Infrastructure ✅ (closed 2026-04-26)
All tools needed to rapidly design, test, iterate, and present cards, statuses, and encounters to general-audience testers. Includes: Status Icons pipeline (M1.2), editor tooling documentation (M1.4), Dev Mode infinite turns + card spawner + stat editing (M1.5 Phases 1–3), archived superseded docs (M1.6), character hover highlight (M1.7), status icon animations (M1.8), tooltip pipeline with stacked hover + right-click detail (M1.3a/b/c, M1.10), card sizing refactor (M1.9), Deck Editor polish with catalogue filters + cross-tool Edit + last-used folders (M1.1). See below for full scope and DoD.

---

## Milestone 1 — Authoring & Testing Infrastructure

**Goal:** All tools needed to rapidly design, test, iterate, and present cards, statuses, and encounters to general-audience testers.

**Demo pitch:** "Here's how we design cards and test them in real-time — the authoring pipeline that lets us iterate fast, with feedback clear enough that anyone can playtest."

**Priority order (revised 2026-04-23):** Game-feel prerequisites are done (M1.5 Phases 1–2, M1.7, M1.8 all closed). M1.3 was originally a single batch; during scoping on 2026-04-20 it was expanded beyond its original scope (card-hover stacked tooltips, right-click detail modal, card sizing), and the user decomposed it into five sequenced batches. Current M1 sequence:

1. **M1.3a** Status descriptions + central effect-to-text builder + card-face text fix + per-icon character tooltip. ✅ (closed 2026-04-23).
2. **M1.3c** Card-hover stacked tooltip panel (Monster Train-style). ✅ (closed 2026-04-23).
3. **M1.10** Right-click card detail view modal — the home for full composition detail and any text that no longer fits the card face. ✅ (closed 2026-04-23).
4. **M1.3b** `SpecialKeywords` enum + data audit + JSON importer improvements. ✅ (closed 2026-04-23).
5. **M1.9** Card size + hover growth refactor — `HandController` gains code-configurable base and hover sizes with curve reflow.
6. **M1.5 Phase 3** Dev Mode stat & state editing — P3.1, P3.2, P3.3a, and P3.3b all closed. Phase 3 complete (P3.4 deferred).
7. **M1.1** Deck Editor polish — authoring comfort. ✅ (closed 2026-04-26).

Reasoning: M1.3a is the lowest-risk step that fixes the enum-name bug visible in every test and establishes the central builder. M1.3c (stacked tooltips) unblocks the hover-UX conversation. M1.10 provides the home for cut composition text. M1.3b then removes legacy keywords with confidence. M1.9 is polish that wants stable content underneath it. M1.5 Phase 3 is a developer affordance, not player-facing. M1.1 is authoring comfort.

### 1.1 — Deck Editor polish ✅ (closed 2026-04-26)
- Better catalogue filters (by musician, by kind, by effect type).
- Card preview info in staged card list (effect summary, cost, kind badge).
- Cross-tool integration: Open in Card Editor, Ping Card in Project.
- Final validation pass.

**Delivered:**
- Catalogue gains Musician dropdown (`MusicianCharacterType` popup) and Effect dropdown (`All / HasStress / HasVibe / HasStatus / HasDraw`). `ApplyCatalogueFilter` applies both alongside existing search + kind toggles.
- Catalogue and staged-list rows show `[A ★1]` cost badge and plain-text effect summary via `GetPlainEffectSummary` (action: effect list; composition: primary kind + modifier count).
- Edit button on both catalogue and staged-list rows calls `CardEditorWindow.OpenAndSelect(CardDefinition)`, which resolves the owning musician, loads the catalogue, and selects the entry.
- `DeckValidationService` gains "no Composition cards" and "no Action cards" warnings.
- `DoSaveAs` remembers last-used folder via `EditorPrefs`.
- ST-M11-1 (filters narrow results) PASS, ST-M11-2 (Edit opens correct card) PASS 2026-04-26.

### 1.2 — Status Icons pipeline migration ✅ (closed 2026-04-14)
- ~~Migrate `StatusIconsData` / `StatusIconData` from legacy `StatusType` enum to the SO-based system.~~
- ~~Create or assign icons for the six Combat MVP statuses.~~
- ~~Stack count display on character canvas (musician + audience).~~

**Outcome beyond original scope:** `StatusIconsData` indirection removed. Sprite authority on `StatusEffectSO.IconSprite`. `CharacterCanvas` reads directly from `StatusEffectContainer`. Additional polish: auto-rename of SO asset, catalogue validation fix, `BandCharacterStats.ApplyStatus(StatusType)` marked `[Obsolete]`.

**Multi-turn validation — outcome against M1.5 Phase 2 (2026-04-20):**
- T5 Choke decay at `PlayerTurnStart` tick — ✅ PASSED.
- T8 Feedback DoT accumulation — ✅ PASSED (after tick-timing correction).
- T7 Shaken expiry across a song cycle — ✅ PASSED (M1.5 Phase 3.1, 2026-04-23).

See `SSoT_Status_Effects.md` §3.3 and `SSoT_Dev_Mode.md` §9.3.

### 1.3 — Tooltip pipeline extension (decomposed into five sub-batches)

Originally scoped as a single batch. Expanded 2026-04-20 after UX review (card size, composition face density, Monster Train-style hover stacks, right-click preview). Split into 1.3a, 1.3b, 1.3c plus 1.9 and 1.10.

**M1.3a — Status descriptions + effect-to-text builder + card-face fix + per-icon tooltip. ✅ (closed 2026-04-23.)**

Delivered:
- `StatusEffectSO.Description` field (`[TextArea(2,4)]`, rich-text friendly).
- `CardEffectDescriptionBuilder` static class under `ALWTTT.Cards.Effects` — single owner of card-effect text formatting (ApplyStatusEffect / ModifyVibe / ModifyStress / DrawCards). `CharacterStatusId` enum-name leak eliminated.
- `CardDefinitionDescriptionExtensions.cs` delegates action-card descriptions to the builder.
- `StatusIconBase.cs` gained `IPointerEnter/Exit` + `BindTooltipSource(def, container, id)`. Per-icon hover renders `{DisplayName}` (+ `×N`) + `Description`.
- `CharacterCanvas.TryCreateIcon` wires `BindTooltipSource` right after `SetStatus`.
- `Description` text authored on the 6 canonical status SOs: Flow, Composure, Choke, Shaken, Exposed, Feedback.

SSoT edits for `SSoT_Status_Effects.md` §3.3 and `SSoT_Card_System.md` §10 applied at M1.3c closure.

**Deferred:** stats-panel tooltips (Charm / Technique / Emotion) — deferred alongside the contextual-stats feature itself.

**M1.3b — `SpecialKeywords` audit. ✅ (closed 2026-04-23.)**

Delivered:
- `SpecialKeywords` enum cleaned to 7 canonical values. 6 legacy entries removed (`Chill`, `Skeptical`, `Heckled`, `Hooked`, `Blocked`, `Stunned`). Card assets cleaned.
- `SpecialKeywordData` asset populated with descriptions for `Consume`, `Exhaust`, `Ethereal`.
- JSON importer gained `keywords` string array (case-insensitive, unknown values warned and skipped).
- JSON batch wrapper gained `defaultEntry` — shared catalog-entry defaults for batch import.
- Exhaust coherence warning: `Debug.LogWarning` when `exhaustAfterPlay` bool and `Exhaust` keyword diverge.
- Card Editor create wizard resets `Kind` to `Action` on open (fixes dual-button UX trap).
- All ST-M13b-1..8 pass.

Keyword model documented in `SSoT_Card_System.md` §3.3. JSON schema documented in `SSoT_Card_Authoring_Contracts.md` §5.3, §5.7, §5.8, §7.4.

Deferred: keyword-driven runtime behavior (retire `ExhaustAfterPlay` bool → `Keywords.Contains` check). Not yet scheduled.

**M1.3c — Card-hover stacked tooltip panel (Monster Train-style). ✅ (closed 2026-04-23.)**

Delivered:
- `CardBase.ShowTooltipInfo()` aggregates: unique `StatusEffectSO`s referenced by `ApplyStatusEffectSpec` in the payload effects + declared `CardDefinition.Keywords`. One tooltip call per unique source. Dedupe via `HashSet<StatusEffectSO>`. Display order: keywords first, statuses second.
- Mouse-follow positioning (static anchor path removed after diagnosis — WorldToScreenPoint on canvas-edge RectTransform through HandCamera produced off-screen coords).
- `TooltipController` prefab: `VerticalLayoutGroup` (Upper Left, spacing 5, ControlChildSize Width+Height, padding 5) + `ContentSizeFitter` (Horizontal + Vertical = Preferred Size).
- Card Editor `AddEffect` bug fixed: `GenericMenu` callback now calls `ApplyModifiedProperties` + `SetDirty` immediately. Fixes effect authoring for both Action and Composition payloads.
- All ST-M13c-1..7 pass.

Deferred: raw Inspector `[SerializeReference]` drawer (M1.1), composition face `Effects` display (M4 design decision).

SSoT edits applied at closure: `SSoT_Status_Effects.md` §3.3, `SSoT_Card_System.md` §10.

**M1.9 — Card sizing refactor.**

`HandController` grows code-configurable sizing that preserves existing Bezier + tilt + selection-pop behavior:
- Serialized `cardBaseScale` (default 1.0) and `cardHoverScale` (default 1.25) + `scaleLerpSpeed` (default 12).
- Per-frame `cardTransform.localScale` lerps toward `baseScale` or `hoverScale` based on `mouseHoveringOnSelected || onDraggedCard`.
- Curve width parameters (`curveStart`, `curveEnd`, `handSize`) multiplied by a new `handScaleFactor` so growing the base size does not cause card overlap. Curve reflow on `InitHand` uses the scaled values.
- No prefab scale changes required — all sizing is driven from `HandController` fields.
- Verification: existing selection-pop, tilt, drag, and drop-zone behavior unchanged.

✅ **Closed 2026-04-23.** Serialized `cardBaseScale` (1.0), `cardHoverScaleMultiplier` (1.25, relative), `scaleLerpSpeed` (12). Per-frame `localScale` lerp. Curve reflow via `HandScaleFactor`. Proportional pop-up, fanning, hover threshold. Per-frame `UpdateCurvePoints` fixes pre-existing bug where moving the GO broke the curve. `RecalculateCurve()` + `OnValidate` for live tuning. All ST-M19-1..8 pass.

**M1.10 — Right-click card detail view.**

New modal UI for full-detail card inspection:
- New `CardDetailView` prefab: large card visual, dim-background overlay, full text (composition cards show full modifier list with `fx.GetLabel()` here, plus style-bundle asset name, part labels, musician id).
- New `CardDetailViewController` singleton: `Show(CardDefinition)` / `Hide()`.
- `CardBase.OnPointerDown` intercepts right mouse button → calls `Show` on the controller. Left-click behavior unchanged.
- `HandController.DisableDragging()` called while modal is open; re-enable on close.
- Dismiss: click anywhere outside the card, Esc, or right-click again.
- Home for any text cut from the card face in M1.3b.

✅ **Closed 2026-04-23.** `CardDetailViewController` singleton, `GetDetailDescription()`, right-click intercept on `CardBase.OnPointerDown`. Composition detail renders full modifier list via `PartEffect.GetLabel()`, style-bundle name, scope/timing tags, and `CardPayload.Effects`. Dismiss via Esc/background click. Cosmetic polish deferred (word-break, panel overflow).

### 1.4 — Editor tooling documentation ✅ (closed 2026-04-08)
- ~~Create `SSoT_Editor_Authoring_Tools.md`.~~
- ~~Register in `SSoT_INDEX.md` and `coverage-matrix.md`.~~

### 1.5 — Dev Mode gig scene

**Phase 1 ✅ (closed 2026-04-17)** — Infinite turns + overlay + hand-visibility bridge. See `systems/SSoT_Dev_Mode.md`.

**Phase 2 ✅ (closed 2026-04-20)** — Card spawner. Overlay tab toolbar, `DevCardCatalogueTab`, `DeckManager.DevSpawnCardToHand` + `CanDevSpawnToHand` gate. See `systems/SSoT_Dev_Mode.md` §§3, 6, 8.4–8.6, 9.2–9.3, 11.

**Phase 3 — stat & state editing (in progress)**

**Phase 3.1 — ✅ (closed 2026-04-23).** Breakdown entry point: Stats tab, musician selector, `DevForceBreakdown`. T7 Shaken expiry validated — M1.2 multi-turn gap fully closed. See `SSoT_Dev_Mode.md` §12.

**Phase 3.2 — ✅ (closed 2026-04-23).** Gig-wide stat editing: SongHype/Inspiration sliders + Cohesion stepper. Three `DevSet…` wrappers on `GigManager` + `DevSetCurrentInspiration` on `CompositionSession` for live-session routing. Dev Mode principle codified: symmetric consequences (Dev cohesion 0 → `LoseGig`, Infinite-Turns suppression inherited). See `SSoT_Dev_Mode.md` §13.

**Phase 3.3a — ✅ (closed 2026-04-23).** Per-character stat editing + Flow gig-wide extension. Per-Character section on Stats tab (musician: Stress/MaxStress/Composure; audience: Vibe/MaxVibe). Flow added to Gig-Wide Stats. New `DevSet…` methods on `BandCharacterStats`, `AudienceCharacterStats`, and `GigManager` (`DevAddFlowToAllMusicians`). Threshold helpers (`CheckBreakdownThreshold`, `CheckConvincedThreshold`) extracted so Dev and play paths share a single trigger. `AudienceCharacterStats.DevResetConvinced` implementation landed (resolves pre-existing doc-vs-code drift). Latent finding: `HealthBarController.SetCurrentValue(duration=0f)` no-ops the visual bar; `0.1f` workaround used. ST-P33a-1..10 passed. See `SSoT_Dev_Mode.md` §14.

**Phase 3.3b — ✅ (closed 2026-04-24).** Status apply/remove picker on Per-Character section. Active-status readout with `[−1]`/`[Clear]`, catalogue-backed `[◄][►]` picker with `[+1]` apply. No production-class patches. Gameplay-flag asymmetry documented (§15.3). Catalogue scope finding: shared catalogue on musician/audience prefabs — recommend split (asset-only). ST-P33b-1..10 passed. See `SSoT_Dev_Mode.md` §15.

Deferred:
- P3.4 audience transparency panel.
- Encounter modifier toggles.

Open micro-batches: *(none — both closed 2026-04-24 as joint MB1+MB2 batch)*

**MB1 + MB2 — closed 2026-04-24.**
- **MB1** corrected `GigManager.DevSetBandCohesion` code-vs-SSoT drift. One-line `LoseGig()` dispatch added + XML comment rewritten to match §13.2/§13.3. ST-P32-4/-5 retroactively invalidated (recorded PASS 2026-04-23 was not an honest observation — dispatch never existed in code before MB1). Re-validated via ST-MB1-1..4 (all PASS). See `SSoT_Dev_Mode.md` §9.5 + §9.8.
- **MB2** split the shared `StatusEffectCatalogueSO` into `StatusEffectCatalogue_Musicians.asset` (6 canonical musician statuses) and `StatusEffectCatalogue_Audience.asset` (empty at MVP; Earworm populates at M4.3). Musician and audience prefabs reassigned. Zero code change. Resolves §15.4 finding. ST-MB2-1..6 all PASS. See `SSoT_Dev_Mode.md` §9.9 + §15.4. Minor UX polish deferred: distinguish null-catalogue from empty-catalogue picker fallback text (currently both show "no catalogue — assign on prefab").

### 1.6 — Archive superseded planning docs ✅ (closed 2026-04-08)

### 1.7 — Character hover highlight ✅ (closed 2026-04-20)
URP 2D outline shader, `SpriteOutlineController`, `CharacterBase` wiring. `BandCharacterCanvas` contextual-stats path present but disabled at prefab level.

### 1.8 — Status icon animations ✅ (closed 2026-04-20)
`StatusIconBase.PlayAppear()` / `PlayDisappear()`. `CanvasGroup` required. Inspector-tunable durations and AnimationCurves. Race-safe detach-before-disappear in `CharacterCanvas.HandleStatusCleared`.

### 1.9 — Card sizing refactor
See M1.3 decomposition above.

### 1.10 — Right-click card detail view
See M1.3 decomposition above.

### Definition of Done

- [x] M1.2 Status Icons pipeline migration
- [x] M1.4 Editor tooling documentation
- [x] M1.5 Phase 1 Dev Mode infinite turns
- [x] M1.5 Phase 2 Dev Mode card spawner
- [x] M1.6 Archive superseded planning docs
- [x] M1.7 Character hover highlight
- [x] M1.8 Status icon animations
- [x] Composition card face simplified (applied 2026-04-21)
- [x] **M1.3a** Status descriptions + central effect-to-text builder + card-face enum-name fix + per-icon character status tooltip (closed 2026-04-23)
- [x] **M1.3c** Card-hover stacked tooltip panel with auto-derived status + keyword aggregation (closed 2026-04-23)
- [x] **M1.10** Right-click card detail modal (closed 2026-04-23)
- [x] **M1.3b** `SpecialKeywords` enum + data asset audit + JSON importer improvements (closed 2026-04-23)
- [x] **M1.9** Card sizing refactor (base + hover scale configurable from HandController) ✅ (closed 2026-04-23)
- [x] M1.5 Phase 3.1 Dev Mode Breakdown entry point (T7 passed 2026-04-23)
- [x] M1.5 Phase 3.2 Dev Mode gig-wide stat editing (ST-P32-1..7 passed 2026-04-23)
- [x] M1.5 Phase 3.3a Dev Mode per-character stat editing + Flow gig-wide (ST-P33a-1..10 passed 2026-04-23)
- [x] M1.5 Phase 3.3b Dev Mode status apply/remove picker (ST-P33b-1..10 passed 2026-04-24)
- [x] M1.1 Deck Editor polish items (ST-M11-1..2 passed 2026-04-26)
- [x] CURRENT_STATE and relevant SSoTs updated at each batch closure

### Demo-readiness check
- **Demonstrable:** Create a card in Card Editor → add it to a deck in Deck Editor → load Dev Mode → spawn it → see effects with animated status icons, hover highlights, readable tooltips, and the ability to inspect any card via right-click. General-audience testers can drive the game without developer supervision.
- **Viewer sees:** Professional-looking tool pipeline. Fast iteration loop. Clear visual feedback on all game state. Responsive, tactile game feel. Every card and status explains itself on hover or right-click.
- **Acceptable rough edges:** Dev Mode overlay is IMGUI (functional over pretty). Contextual stats hidden by choice until post-MVP tuning.
- **Must fix before showing:** Status icons correct for all six statuses. Card spawning reliable. Hover highlights and icon animations functional. Card text readable on the face. Tooltips appear reliably on hover. Right-click detail works on any card. Audience reactions readable.

---

## Milestone 2 — Composition Session Validation

**Goal:** Prove that composition cards work end-to-end with real music and real decks.

**Demo pitch:** "Play cards, hear the song change."

**Unblocked by:** M1.5 Dev Mode Phases 1 and 2 — both closed.

### 2.1 — Real composition testing deck
- Design a varied composition deck with meaningful musical choices.
- Include DrawCards effects for deck speed.
- Test with multiple track/style bundle combinations.

### 2.2 — End-to-end composition testing
- Play composition cards during a live loop and verify audible song changes.
- Validate that `CompositionCardPayload.modifierEffects` produce expected musical mutations.
- Validate that gameplay effects on composition cards fire correctly through the normal card pipeline.
- Test the separation between musical modifier pipeline and gameplay effect pipeline.

### 2.3 — Timing and deck speed design
- Resolve open design question: when can composition cards be played relative to the loop cycle?
- Resolve deck speed question: how many composition cards per loop? Per song?
- Document findings in the relevant SSoTs.

### 2.4 — Composition UI feedback
- Composition card tooltips show musical modifier summary (auto-derived via M1.3a/c or via M1.10 detail modal for the full list).
- Visual feedback when a composition card changes the active song model.
- Loop/song progress visible during composition phase.

### Definition of Done
- [ ] Real composition deck designed and imported
- [ ] Composition cards produce audible changes when played during a live loop
- [ ] Musical modifier and gameplay effect pipelines confirmed independent
- [ ] Deck speed and timing design questions resolved and documented
- [ ] Composition UI shows what changed after a card play
- [ ] At least one complete play-through: start gig → play composition cards → hear song evolve → song ends with score
- [ ] CURRENT_STATE and relevant SSoTs updated

### Demo-readiness check
- **Demonstrable:** A full gig where the player shapes a song through card choices.
- **Viewer sees:** Cards played → song audibly changes → score reflects composition quality.
- **Acceptable rough edges:** Limited musical variety. Basic composition UI.
- **Must fix before showing:** Song must audibly change. Score must reflect card choices. No silent failures.

---

## Milestone 3 — Combat & Status Polish

**Goal:** Combat loop feels complete, readable, and satisfying. All status effects have visible consequences.

**Demo pitch:** "A full gig with clear feedback on every action and consequence."

### 3.1 — Shaken restrictions enforcement
- Resolve design decision: what Shaken prevents.
- Implement Composure penalty during Shaken.
- Update `SSoT_Status_Effects` and `SSoT_Gig_Combat_Core`.

### 3.2 — Audience pressure expansion
- Implement Stress path on `AudienceCharacterBase`.
- Enable Feedback DoT on audience members.
- Additional audience-side status effects beyond those delivered in M4.3 (Earworm). Candidate follow-ons: `Captivated` (Vibe multiplier, CSO primitive `DamageTakenUpMultiplier`, identity status for Ziggy) and defensive/resistance statuses for encounter variety. See `planning/Design_Audience_Status_v1.md`.

### 3.3 — UI readability pass
- Status icons visible and correct for all statuses.
- Meter visibility: Stress bars, Composure shields, Flow/Exposed indicators.
- Turn phase indicators.
- Card play feedback.

### 3.4 — Encounter variety (initial)
- Design 2–3 distinct encounter configurations.
- Test via Dev Mode scene.
- Validate encounter-level victory/failure conditions.

### Definition of Done
- [ ] Shaken restrictions enforced at runtime; design decision documented
- [ ] Composure penalty during Shaken implemented
- [ ] Audience Stress path exists; Feedback DoT works on audience
- [ ] At least one audience-side status effect designed and implemented (delivered via M4.3 — Earworm)
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

## Milestone 4 — Starter Deck Foundations

**Goal:** Deliver a real starter deck that honestly represents the game's composition-first identity, with a jugable loop built on 2 musicians (Robot C2 + Sibi), the first audience-side status (Earworm), a coherent Flow model across card domains, and a deck contract that supports multiple copies of a card.

**Demo pitch:** "Start a run, play a gig with your band, hear the music shaped by your card choices, convince the audience."

**Scope note:** this milestone groups a set of batches that were previously a mix of planned and future items. The starter deck design pass (closed 2026-04-21, documented in `planning/Design_Starter_Deck_v1.md`) surfaced that several apparently-independent items are actually a single design envelope: C1 must be fixed before encounter tuning; Flow behavior must be coherent across card domains before the starter can be designed around it; audience-side statuses must exist for Sibi's identity; deck copies must be supported for the 12/7 starter composition; bidirectional guaranteed draws must exist so the 8:4 ratio never produces empty phases. **All decisions are design-space until their batches land in code. No SSoT promoted.** `planning/Design_Starter_Deck_v1.md` and `planning/Design_Audience_Status_v1.md` are the working references, subject to playtest revision.

**Roster scope:** Robot C2 (drummer / drum machine) and Sibi (keyboardist, worm-like entity with psychic affinity). Conito (bassist — flight + electricity) and Ziggy (vocalist — multiharmony) are deferred to post-MVP roster expansion, sequenced later under their own work. Bass pipeline validation is not on the M4 critical path.

**Sequencing:** M4 is sequenced **after M1 closure**. M1.3 decomposition (M1.3a, M1.3c, M1.10, M1.3b, M1.9) is complete. Remaining M1 work: M1.5 Phase 3.3, M1.1. Within M4, M4.1 is the first batch; M4.2 and M4.3 can run in parallel; M4.4 and M4.5 can run in parallel after M4.1; M4.6 (authoring) depends on all previous.

### 4.1 — C1 fix: unified Stress path ✅ (closed 2026-04-26)

Route `AddStressAction.DoAction` through `MusicianBase.Stats.ApplyIncomingStressWithComposure` (the helper already used by `CardBase.ExecuteEffects` on `ModifyStressSpec` positive). Composure absorbs audience pressure correctly post-fix.

**Delivered:** `AddStressAction.cs` — pattern match narrowed to `BandCharacterStats`, call changed from `AddStress(amount, duration)` to `ApplyIncomingStressWithComposure(targetCharacter.Statuses, amount, duration)`. Debug log added showing `Incoming / Absorbed / Applied`. One file, three lines changed. ST-M41-1 (Composure absorbs), ST-M41-2 (Exposed amplifies), ST-M41-3 (card regression), ST-M41-4 (Breakdown triggers) — all PASS 2026-04-26.

Scope:
- Modify `CharacterActionProcessor.GetAction(CardActionType.AddStress).DoAction` (or the `AddStressAction` class directly, depending on structure) to route through the helper.
- Grep existing encounter assets in `GigSetupConfigData.availableEncounters` / `EncounterData` for any that were tuned against current-broken behavior. Low risk, but requires a check.
- Update `SSoT_Gig_Combat_Core.md` §5.4 and §6.2 to document the unified path at batch closure.
- Smoke tests: audience `AddStressAction` against musician with N Composure stacks → Composure absorbs first, remainder applies; Exposed multiplier still applies correctly; Breakdown still triggers on overflow.

### 4.2 — Flow bifurcation + adaptive LoopScoreCalculator ✅ (2026-04-28)

**Delivered:**
- Flow bifurcated by card domain: Action cards use performer's individual Flow stacks as flat Vibe bonus (`flowActionVibeBonusPerStack = 1`); Composition cards and Song End use band-wide Flow stacks as Vibe multiplier (`flowVibeMultiplier = 0.08f`). All Inspector-tuneable.
- Legacy Flow → SongHype path retired and removed from code (3 fields, 1 code block deleted).
- `LoopScoreCalculator` rewritten with adaptive scoring: `LoopScoringMode` enum (RoleNormalization / MusicianParticipation), `LoopScoringConfig` + `HypeThresholds` Inspector-tuneable structs.
- `possibleRoleCount` and `totalMusicians` auto-detected at gig start from deck composition cards and band roster.
- Bug fix: Backing tracks (`TrackRole.Backing`) were invisible to the scorer — `HasBacking` added to `LoopFeedbackContext`.
- Field renames with `[FormerlySerializedAs]` for serialization safety.

**Smoke tests:** ST-M42-1/1c/3/4/5/9/10/11 passed. ST-M42-2 deferred (no composition card with ModifyVibe). ST-M42-6/7/8 deferred (need 2-musician gig — musician picker in Gig Setup not yet implemented).

**Files changed:** `LoopScoreCalculator.cs` (full replacement), `LoopFeedbackContext.cs` (+HasBacking), `GigManager.cs` (field changes + InitLoopScoringConfig + scoring calls + song-end Flow path), `CardBase.cs` (bifurcated ModifyVibe + per-performer Flow + GetPerformerFlowStacks helper).

Docs updated at closure: `SSoT_Gig_Combat_Core.md` §6.1/§8.1/§11, `SSoT_Scoring_and_Meters.md` §3/§7.1, `SSoT_Status_Effects.md` §5.1, `CURRENT_STATE.md`, `changelog-ssot.md`, `Design_Starter_Deck_v1.md` §9 #3.

### 4.3 — Earworm: first audience-side status

Implement one audience-side status: **Earworm**.
- Key `"earworm"`, CSO primitive `DamageOverTime`.
- Scope: single audience member.
- Tick `AudienceTurnStart`: audience gains `+N Vibe` where N = current stacks, then decay 1 stack.
- StackMode `Additive`, DecayMode `LinearStacks`.
- Fantasía: the song gets stuck in their head — they convince themselves passively.

Implementation:
- New SO `StatusEffect_Earworm_DamageOverTime.asset` with appropriate fields, added to the status catalogue.
- New icon asset.
- Hook in `GigManager.AudienceTurnRoutine`: iterate `CurrentAudienceCharacterList`, for each with Earworm stacks, call `audience.AudienceStats.AddVibe(stacks)`. Decay handled by container tick automatically.
- Smoke tests: applying Earworm N stacks → audience gains N Vibe on next `AudienceTurnStart`, stack count decrements by 1; repeats across turns until stacks = 0; Earworm + `ModifyVibe` direct in same turn do not double-count.

**Out of scope (deferred):**
- `Captivated` (CSO `DamageTakenUpMultiplier`, Ziggy's identity status) — deferred to roster expansion.
- `ApplyIncomingVibe` helper — not needed for Earworm; deferred alongside Captivated. Hook point documented in `planning/Design_Audience_Status_v1.md`.

Docs at closure: `SSoT_Status_Effects.md` new §5.7 (Earworm spec), `SSoT_Audience_and_Reactions.md` §8 and §10 (remove "audience statuses optional for MVP"; Earworm is the first active audience-side status).

### 4.4 — Deck Contract Evolution (card copies)

Evolve `BandDeckData` from `List<CardDefinition>` to `List<BandDeckEntry> { card, count }` (or equivalent multiset representation). `PersistentGameplayData.SetBandDeck` respects multiplicity. `CardAcquisitionFlags.starterCopies` runtime consumption shipped 2026-05-02 in M4.6-prep batch (2) (`PersistentGameplayData.SetBandDeckFromMusicians` consumes `MusicianCardEntry.starterCopies` × `MusicianCardEntry.IsStarter` directly; `BandDeckEntry.count` remains the multiplicity carrier on the legacy `BandDeckData` asset path).

Scope:
- Data contract change on `BandDeckData`.
- Runtime: `PersistentGameplayData.SetBandDeck` iterates entries and adds `count` references to `CurrentActionCards` / `CurrentCompositionCards`.
- Deck Editor UI: staged deck list shows `×N` badge, supports increment/decrement. JSON import/export honors `count`. JSON schema gains a `count` field per card entry (default 1).
- Migration: existing `BandDeckData` assets import cleanly as `count = 1` entries.
- Smoke tests: deck with explicit `×3 Steady Beat` loads into a runtime `DrawPile` with 3 independent `CardDefinition` references; shuffling and drawing treats each correctly; discarding one does not remove the other two.

Docs at closure: `SSoT_Card_Authoring_Contracts.md` (starterCopies is runtime-consumed), `SSoT_Card_System.md` (deck multiplicity semantics).

### 4.5 — Bidirectional guaranteed draws ✅ (closed 2026-04-30)

**Resolution.** Option 1 (predicate-based filtered draw on `DeckManager`) + subtractive budget rule. The roadmap's "two symmetric hooks" framing collapses to a single site at PlayerTurn entry because the action window and composition window open simultaneously in current implementation; there is no separate composition-phase-entry callable in `GigManager`. If a future redesign separates the two windows into distinct phase transitions, the hook split is a future refactor.

Two symmetric hooks in the phase transition pipeline:
- **On composition phase entry:** if `HandController.Hand` contains zero composition cards and at least one composition card exists in `DrawPile ∪ DiscardPile`, force-draw one.
- **On between-songs / action window entry:** symmetric. Zero action cards in hand + at least one available → force-draw one.

Both windows open simultaneously today, so both guarantees evaluate at the same hook site (`ExecuteGigPhase(PlayerTurn)`).

Scope as shipped:
- `GigManager.ExecuteGigPhase(PlayerTurn)`: one-line wrapper swap from `DrawCards(DrawCount)` to `DrawCardsForPlayerTurn(DrawCount)`.
- `DeckManager`: new `DrawCardsForPlayerTurn` (3-phase subtractive algorithm), `DrawCardFiltered`, `HandHas`, `PilesHave`, `LastTurnGuaranteeSummary`. Total drawn ≤ `DrawCount` (subtractive rule, no extra hand size); composition wins when budget cannot fit both guarantees.
- `DevModeController`: always-on overlay readout `M4.5 last draw: needs=[CA] reserved=N fired=[CA] drawn=K/B`.
- Smoke tests run: ST-M45-1 (comp guarantee fires) ✅, ST-M45-2 (action guarantee fires) ✅, ST-M45-4 (subtractive budget across 10 turns) ✅, ST-M45-8 (reshuffle during filtered draw) ✅. ST-M45-3 (both guarantees same turn) covered by inference from -1 + -2; ST-M45-5/6 dropped as redundant; ST-M45-7 deferred (no `ExhaustAfterPlay` content yet).

Does not depend on M4.4 but benefits from it (composition cards have multiple copies, making "at least one exists in piles" near-always true).

Authority: `SSoT_Runtime_Flow.md §4.2` and `§8 invariant 9`.

### 4.6 — Starter Deck v1 authoring

Author and register the 12-card / 7-unique / 2-musician starter deck per `planning/Design_Starter_Deck_v1.md`.

Preconditions: M4.1 (for honest tuning), M4.2 (Flow model consistent), M4.3 (Earworm exists for Mind Tap), M4.4 (copies supported), M4.5 (guaranteed draws), runtime tuning values received from the user, and verification of `CompositionCardPayload.effects` support (gates `Four on the Floor`'s `ApplyStatusEffect(flow)` co-effect — if unsupported, relocate effect to an action card).

Scope:
- Author the 7 unique `CardDefinition` + payload assets (Warm Up, Take Five, Mind Tap, Steady Beat, Four on the Floor, Synth Pad, Hook Theme).
- Assemble `StarterDeck_v1.asset` via Deck Editor with the copies as specified (`×2 Warm Up`, `×1 Take Five`, `×1 Mind Tap`, `×3 Steady Beat`, `×2 Four on the Floor`, `×2 Synth Pad`, `×1 Hook Theme`).
- Register in `GigSetupConfigData.availableBandDecks`.
- Catalogue → starter-deck auto-assembly shipped 2026-05-02 (M4.6-prep batch (2)). `CardAcquisitionFlags.starterCopies` is the per-card copy count for auto-assembled decks via `PersistentGameplayData.SetBandDeckFromMusicians`. Authoring `StarterDeck_v1.asset` (this M4.6 batch) is the alternative legacy path via `BandDeckData` asset; the gig setup toggle (`useMusicianStartersToggle`, default ON) selects between them. M4.6 demo will use the auto-assembly path.
- Smoke tests ST-SD-1..6 per `Design_Starter_Deck_v1.md` (deck loads with correct multiplicities, reshuffle preserves counts, Mind Tap applies Earworm with correct stacks, Four on the Floor applies Flow on play, composition cards repeat across songs without runtime warnings, full gig plays end-to-end).

### 4.6-followup — Mini-milestone (opened 2026-05-06)

Five batches surfaced during M4.6 starter deck cleanup smoke testing (`Design_Starter_Deck_v1.md §10` ST-SD-1..8). Gate the M4.6 demo. Ordered by dependency + quick-win:

1. **M4.6F-1 — Action card double-discard ✅ (closed 2026-05-07).** Bug class misdiagnosed at intake as reshuffle/pile lifecycle. Root cause was upstream: action cards triggered `OnCardPlayed` from both `HandController.PlayCard` and `CardBase.Use`/`CardUseRoutine`, doubling discard. Fix: gate `HandController.PlayCard`'s `OnCardPlayed` call to `IsComposition` only. Smoke ST-DOUBLE-1/2/3 + ST-RESHUFFLE-1/2/3 all PASS. New invariant in `SSoT_Card_System.md §9.3` + manifest. See §1 closure block in `CURRENT_STATE.md`.
2. **M4.6F-2 — GigSettings multi-SO refactor ✅ (closed 2026-05-07).** Five competing settings homes (`GameplayData`, `GigSetupConfigData`, `GigManager`, `JamRules` struct, `PersistentGameplayData`) consolidated to four SOs on the GigManager side (`GigFlowSettingsSO` + `MeterTuningSO` + `GigPresentationSO` + `GigDevSettingsSO`) plus renamed `GigSetupRosterSO` (was `GigSetupConfigData`). Scene-instance refs stayed on `GigManager`. Façade properties preserved. `GameplayData↔PGD` duplication deferred (D4). Smoke ST-F2-S1/2/3/6/7/8 PASS, S4 PASS with pre-F-2 floating-text caveat, S5 expected FAIL (per-loop draw is F-3). See `M4_6_F2_Doc_Updates.md` and §1 closure block in `CURRENT_STATE.md`.
3. **M4.6F-3 — Per-loop draw + per-loop inspiration hook + canonical AddCurrentInspiration ✅ (closed 2026-05-08).** New `GigFlowSettingsSO.DrawPerLoop` field. Per-loop draw + per-loop inspiration consumption fire from `GigManager.OnCompositionLoopFinished` (host-owned subscriber, respects `CompositionSession` deck-non-mutation invariant). `CompositionSession.AddCurrentInspiration(int) → int` promoted to canonical session-budget mutator (clamps to MaxInspiration, mirrors to PD). Track-derived per-loop gain refactored through it. `+N` badge displays un-clamped track contribution. `JamRules.drawPerPart` flagged UNUSED (F-5 cleanup). Smoke ST-F3-S1..S7 + S4b PASS, S4c FAIL DEFERRED → MB3. Side-findings: §13.4 Dev surface drift (→ MB3), session-start dual-siting (→ MB3), F-2 D4 follow-up (post-demo). See `M4_6_F3_Doc_Updates.md` and §1 closure block in `CURRENT_STATE.md`.

3.5. **MB3 — Dev surface drift correction + session-start dual-siting fix.** ✅ RESOLVED 2026-05-08. Implemented all four surfaces under `#if ALWTTT_DEV`. Carry-over reset semantic implemented via private `ResolveSessionStartInspiration` helper; symmetric across `Begin / ConfirmCurrentPartAndStart / AdvanceToNextPart`. ST-MB3-3 INVALID (lifecycle clarification surfaced); ST-MB3-5/6/7 deferred to loop-game-flow milestone (no current encounter exposes `inspirationPerPart=0`). ST-MB3-1/2/4/8 PASS. Closes ST-F3-S4c. See `SSoT_Dev_Mode.md` §13.4 / §9.10.

3.6. **MB4 — Action-card inspiration session routing (+ MB4-diag).** ✅ RESOLVED 2026-05-08. Closed user-reported critical bug "action cards are NOT consuming Inspiration". `CardBase.SpendInspiration` and `GenerateInspiration` now route through new `GigManager.AdjustInspiration(int delta)` public wrapper, which delegates to `CompositionSession.AddCurrentInspiration` when a session is active. MB4-diag adds `GigManager.IsCompositionSessionActive` Dev-getter and Stats-tab raw `[PD/Session]` readout. Behavior tightening: clamp-at-0 on action-card spend. ST-MB4-1..5 PASS. **F-followup queue exhausted post-MB4.** Open MB5 candidate (action-card cost gate) and the comp-card build-phase mirror remain as parked items for the loop-game-flow milestone. See `SSoT_Dev_Mode.md` §13.4 / §9.11.

4. **M4.6F-4 — SongOrchestrator IndexOutOfRange + ALWTTT-side defense ✅ STAGE A RESOLVED 2026-05-08.** Stage A shipped production-quality defense (try-catch around `generator.Orchestrator.GenerateSinglePart` in `MidiMusicManager.RenderSinglePart`; returns failure tuple integrating with `PlaySinglePartLoop`'s existing graceful-fail branch) + production-quality D3-B within-part recursion guard in `CompositionSession.HandleLoopFinished` (mirrors `AdvanceToNextPart`'s `if (secs <= 0f) End();` pattern) + `[F-4]`-tagged diagnostic logs at both boundary sides. ST-F4-S1/S6 PASS, S3 PASS-vacuous, S2 DEFERRED-non-repro (IOOR did not surface; defense correctly silent), S5 BLOCKED-OUT-OF-SCOPE (package-internal `MidiGenPlayConfig` build errors unrelated to F-4 — separate MidiGenPlay-project batch). Stage B parked-until-natural-repro: reopens automatically if `[F-4][MMM]` LogError fires during playtest; otherwise diag logs strip at M4.6 demo closure (retroactive D5-C). Defense + D3-B guard stay permanent. See `CURRENT_STATE.md` §1 closure block.
5. **M4.6F-5 — Composition per-loop pending workflow ✅ ABSORBED into Phase B B1 (2026-05-09).** Originally framed as a model change (rewrite `CompositionSession.HandleLoopFinished` for within-part rotation; UI rename "Part A / Part B" → "Current loop / Next loop"). During Phase B planning the user clarified that per-loop card resolution **already works** in the current zone (cards in current → replace track → effect at next loop). The complex piece — *next zone* (planning a future part) — is being simplified out, not implemented. Phase B B1 disables next zone, agrandar current zone to full-screen, model collapses to per-loop-only. Code-name `Part` keeps current meaning; future Song Parts Library (`planning/Design_Song_Parts_Library_v0_1.md`) remains a long-term intent without forced rename pressure. Closure happens when B1 lands. See §5 Phase B and `CURRENT_STATE.md` §1 Phase A close block.

After F-1..F-4 + F-5-absorbed: Phase A is closed. Phase B (Gameplay loop polish) opens — see §5 below. After Phase B exit: demo cut + ST-M42-6/7/8/9 + batch (5) Runtime tuning original (parked).

## 5 — Phase B — Gameplay loop polish (opened 2026-05-09)

**Status:** Active (B1 closed 2026-05-12; B2 closed 2026-05-13; B2.5 closed 2026-05-15; B3 next).
**Goal:** Take the working Phase A pre-demo build to a true demo with track persistence between loops, expanded UI feedback, content/balance polish, and animation polish.

**Demo pitch:** "Start a run. Play a gig with your 2-musician band. Each card you play *changes* the music — and the parts you didn't change *stay the same*. Watch the song hype build, see the venue light up, hear the audience react. Convince the crowd, or break under their pressure."

**Scope note.** Phase B reframes M4.6 as Phase A demo. The "M4.6 starter deck v1 authored end-to-end" original DoD item carries over — content is authored, runtime auto-assembles, gigs play. Phase B focuses on *how it feels to play*, not *whether it plays*.

**Sequencing.** Strict left-to-right: B1 → B2 → B3 → demo readiness review. B1 is foundational and gates B2/B3. B2 and B3 could in principle run in parallel after B1 lands, but the team is one developer + agent, so sequential.

### 5.1 — B1 — Loop model simplification + track persistence + UI rework ✅ (closed 2026-05-12)

Foundational. Estimated ~300-400 LoC ALWTTT-side. 1 long session or 2 shorter (split point: stem-cache foundation, then UI rework).

**Decisions locked at open:**
- D1=C: Phase A formally closed; Phase B opens with own identity.
- D2=B: Per-track persistence simple. Tracks not touched persist verbatim across loops; structural changes (TS, key, measures) → full regen.
- D3=A: B2 monolithic (UI feedback + animation in one batch); fallback split if pesado.
- D4=B: Audience Member Wizard Editor deferred post-demo.
- D5=run-complete: Spike confirmed feasibility ALWTTT-side without MidiGenPlay boundary violation. Stem cache mechanism + DryWetMidi merge + F-4 Stage A interaction designed.
- D6=A: Per-track stem cache scope (each track invalidates independently; per-musician would re-introduce intra-musician regen surprises).
- D7=B: Cache lifetime per-song. Resets on song boundary. Persists across loops + part transitions within a song.
- α/β=β: Phase A close + Phase B open landed as a clean separate doc batch (already shipped).

**Internal ordering (#7 → #0 → #1, #2 → #8):**
1. **#7 Stem cache + merge.** New stem cache co-located with `MidiMusicManager` keyed on `(trackIdentity, trackInputsHash, partMeterHash)`. On render call: hash track inputs, check cache, decide which tracks need re-render. Tracks unchanged reuse cached MIDI bytes; tracks changed render via existing `RenderSinglePart` path (MidiGenPlay still renders all tracks, but we replace stems only for changed tracks). Final playable file built via DryWetMidi merge of stems. F-4 Stage A try-catch remains outermost; on catch all stems for the part invalidate. Stem cache resets per song (D7=B).
2. **#0 Disable next zone + agrandar current zone.** UI removal. Next-zone GameObjects/scripts disabled or removed; current zone takes full screen. Card click handler: only one drop target.
3. **#1 + #2 Composition session UI rework.** Show currently-playing track per musician with Inspiration-next badge to the right; when player plays composition card, "program" the next-loop track is visualized with pending state.
4. **#8 No discard on play.** Single-condition change: don't discard hand cards when CompositionSession starts.

**Files (likely):**
- `MidiMusicManager.cs` — stem cache field + new helpers + DryWetMidi merge.
- `CompositionSession.cs` — possibly minor adjustments (call site of RenderSinglePart unchanged in shape; stem cache is opaque to it).
- Composition UI files (current/next zone controllers) — names TBD; user to provide via files.txt.
- `GigManager.cs` or wherever the CompositionSession start is wired — for #8 discard behavior.

**DoD:**
- Per-track persistence demonstrably works in playtest: play a rhythm composition card, hear rhythm change, hear chord progression NOT change.
- Structural-change invalidation works: play a meter card (TS shift), hear the whole loop regen.
- Next zone UI removed; current zone full-screen.
- Hand persists across CompositionSession start.
- F-4 Stage A defense not regressed; stem cache invalidates on catch (safe regression to merged-file path).
- Smoke tests ST-B1-S1..S8 (or similar bounded set) covering: persistence, structural invalidation, song-boundary cache reset, F-4 catch invalidation, F-1/F-3 regression clean.

**Closed 2026-05-12.** All internal items shipped: #7 stem cache + #0 next-zone disable + #1+#2 UI rework + #8 hand-discard configurability + #7.1 instrument pin + D-J draw-on-play. Boundary respected — no `SongConfig` field added; hash travels ALWTTT-side as a per-call parameter per D-E=α'. Smoke tests ST-B1-S1..S10 PASS (S4 DEFERRED-no-hook; reopens automatically if F-4 LogError fires during playtest). F-5 invariant promoted to `SSoT_Runtime_CompositionSession_Integration §8` per D-K=α. See `CURRENT_STATE.md` §1 closure block and `changelog-ssot.md` 2026-05-12 entry.

### 5.2 — B2 — Polish layer (feedback + animation) ✅ (closed 2026-05-13)

Aditivo, low risk, depends on B1 landed. Default monolithic; fallback split B2a (UI feedback) + B2b (animation) if pesado.

**Items:**
- **#3 Tooltip miniature on track labels.** Hover over current/next track label → minicard preview. Reuse `CardUI` + tooltip system.
- **#4 Inspiration markers pop-up animation.** When `AdjustInspiration` mutates the value, marker briefly scales + flashes.
- **#5 Expanded floating text.** Composition events ("MAJOR CHANGE", "MINOR CHANGE", "FUNKY VIBE"), audience exclamations ("WOW", "AMAZING", "YEAH", "WOOO"), multipliers with icons ("×2 EARWORM", "+5 FLOW"). Reuse existing FloatingText system.
- **#6 SongHype thresholds → venue SFX.** 1/3 → lights, 2/3 → smoke machines, 3/3 → fire. SFX already implemented; just call from the right hook (likely `GigManager.AddSongHype` or a new threshold-detection helper).
- **#14 Robot animation polish.** Pop-up on beats (sprite scale ease in/out instead of jump).
- **#15 Worm animation polish.** Vertical stretch up + compress down on beats.
- **#16 Worm instrument separate animator.** GameObject already separate; needs own Animator component.

**DoD:**
- All six feedback items observable in a normal playtest.
- Animation feel review user-confirmed.
- Smoke tests ST-B2-S1..S6 (one per item).

**Closure (2026-05-13):** Six items shipped monolithically per D3=A; fallback split not triggered. Two mid-batch decisions added: D-Inspiration-Pool=A (action card cost gating bug fix, single-pool semantics) and D-FxChangeDetect=A (`CompositionFxConfigSO` + diff-driven floating text classifier). 3 new + 10 modified files, ~700-900 LoC. Smoke tests S1-S3 PASS, S4 PASS-with-deferral, S5 PASS-with-caveat, S6 PASS. 16 items parked to new B2.5 batch (correctness, content, cleanup, design gaps). No SSoT authority changes. F-1 / F-3 / F-4 Stage A / F-5 invariants not regressed. See `CURRENT_STATE.md §1` B2 closure block, `changelog-ssot.md` 2026-05-13 entry, and the new B2.5 batch open.

### 5.2.5 — B2.5 — Polish refinements + cleanup ✅ (closed 2026-05-15)

Aditivo, depends on B2. Closes the cleanup-and-correctness batch parked at B2 closure. 11 mandatory items + item 16 shipped; items #4, #5, #6 (content-dependent) and #12-15 (design gaps) explicitly deferred to playtest / B3.

**Items shipped:**
- **Correctness (3/3):** #1 Earworm per-holder stagger with `yield return waitDelay`; #2 `BackgroundContainer.DeactivateAllSFX` + hooks in `OnCompositionSongFinished` and `ResetSongHype`; #3 `CharacterBase.BroadcastBPM(int)` cascading to all sub-animators via `GetComponentsInChildren<CharacterAnimator>`.
- **Cleanup (5/5):** #7 stale `TEST TEST TEST` log; #8 `CardBase.OnPointerDown` log spam; #9 B2 debug logs across `MinicardTooltipController`, `SongTrackElementUI`, `FxManager`, plus the `clamped = 2;` forced-clamp in `GigManager.TriggerAudienceMicroReactions` (audience exclamations now correctly placeholder-zero pending B3 `ResolveLoopEffect`); #10 audience action floating text normalized to Vector2/Color overload; #11 `TimeSignature.ToString()` consistency verified (no code change).
- **Design gaps (1/5):** #16 Tonality FxEntry kept per D-3=B (cost of keeping is essentially nil; removing would create migration debt on existing assets). Inline comment tightened to document the design-hook intent.

**Decisions locked:**
- **D-1=A** Earworm staggered with `yield return waitDelay`; M4.3 invariants preserved.
- **D-2=A** `BroadcastBPM(int)` helper on `CharacterBase` cascading to children; body-only animator settings stay author-controlled.
- **D-3=B** Tonality FxEntry kept with tightened comment.
- **D-4=keep** M4.3 `[Earworm]` feature log preserved (per-holder cost minimal, aids future debugging).
- **D-5=A → refined by D-8=A.** D-5 initially moved `ResetSongHype()` to `OnCompositionSongFinished` for visual coherence; introduced macro-Vibe regression (caught by ST-S6 diagnostic — `_songHype` zeroed before `RunSongVibeResolution` could read it). D-8 surgically split: only `DeactivateAllSFX()` stays at song-end (lights-off-at-audio-end UX preserved); full `ResetSongHype()` moved back to `AudienceTurnRoutine` AFTER vibe consumption.
- **D-6=A** Hand-discard default flipped to `true` (code + asset). Production behavior: hand discards between turns by default.
- **D-7=A** `DiscardHand` ghost cards. Production path now synchronous + sweep strays + immediate destroy, mirroring `DevForceHandResetToDiscard`. Fixes ghost GameObjects from async `CardBase.Discard()` path with `IsPlayable`/`IsExhausted` gates.

**Hypothesis correction (mandatory).** The B2 closure language asserted "real Earworm tick lives elsewhere in StatusEffectSO" as the reason for deferring item #1. Code inspection during B2.5 showed this was incorrect: `StatusEffectContainer.Tick` only decays stacks via `DecayMode`; no separate gameplay-payload tick site exists. `StatusEffectWizardWindow.cs:250` confirms this in its inspector helpbox. The bespoke vibe-gain block in `GigManager.AudienceTurnRoutine` IS the only Earworm tick site. Actual issue: synchronous spawn pile-up at audience-turn-start. Fix was visual pacing, not relocation. See `CURRENT_STATE.md §1` B2.5 closure block for full corrective note.

**Files changed:** 10 modified (`GigManager.cs`, `CharacterBase.cs`, `BackgroundContainer.cs`, `AudienceCharacterBase.cs`, `CardBase.cs`, `MinicardTooltipController.cs`, `SongTrackElementUI.cs`, `FxManager.cs`, `SongCompositionUI.cs`, `DeckManager.cs`). 1 asset value change (`GigFlowSettings.asset`). 2 in-scene/asset corrections done as B3-cand-A/B during S1 playtest (Mind Tap payload target, AudienceMemberPosList reorder).

**Smoke tests:** ST-B2.5-S1 PASS (Earworm stagger), S2 PASS (lights clear between songs), S2b PASS (lights off at exact audio-end), S3 PASS (BPM cascade to sub-animator), S4 PASS (hand discard toggle), S5 PASS (no ghost cards), S6 PASS (macro-Vibe applied visually).

**Items deferred from B2.5:**
- Content-dependent: #4 per-venue smoke/fire VFX (art-dependent), #5 CompositionFxConfigSO default tuning (playtest), #6 animation feel tuning (playtest).
- Design gaps: #12 TempoScale diff in `SelectFxEntry`, #13 hasExplicit flags on PartEntry, #14 `PartActionKind.NoOp`, #15 `#if ALWTTT_DEV` gate on `DevAddSongHype`/`DevResetSongHype`.

**B3 candidate slate accumulated:** A (Mind Tap — done), B (PosList — done), C (target-type validation), D (effect labels in default Inspector), E-lite (Blocked tooltip), F (real `ResolveLoopEffect`), G (filter draws during composition session, per D-B3-DrawFilter=B), H (Always-action discard semantics), I (ParentActive=False warning during draws), plus design gaps #12-15.

No MidiGenPlay internals. No SSoT authority changes. B1 + B2 invariants preserved. Phase B remains operational (not governance).

### 5.3 — B3 — Content + design

Aditivo, depends on B1. Authoring + design.

**Items (gameplay content):**
- **#9 Inspiration cost/gen balance pass.** Cover cost 0/1/2/3 and generated 0/1/2/3 across the deck. 4/4 cards most common; 3/4 next; 6/8 next; 5/4 rare and powerful. Major/minor chord progressions get simple distinguishing effects.
- **#10 BPM cards.** Rhythm composition cards with effects: `+/- BPM`, `2× BPM`. Touches `RhythmCardConfigSO` or a new effect type that mutates `cfg.BeatsPerMinute`.
- **#11 Modulation cards.** Chord progression cards with key-shift effect (modulation). Other tracks should persist via B1 stem cache; only the chord stem reflects the new key when played.
- **#11.5 Sibi musical identity — `InstrumentEffect` on Singing Field (added 2026-05-15, D-DCP-5=β).** Sound-design priority: Singing Field card carries a per-card `InstrumentEffect` SO authored specifically for Sibi's voice (new asset, not one of the existing `Bass/Guitar/Synth`). Specific MIDI program selected at authoring time via audition. Establishes the precedent of *per-musician instrument identity*; C2's analogous identity left as TBD (not blocking demo).
- **#12 Audience pool authoring — 2 archetypes shipped together (expanded 2026-05-15, DC-2=Custom).** Promotes the original "1 designed audience member with 3 abilities" to a 2-archetype encounter that the demo will use.
  - **Cool Dude — 3 abilities total.** Spawns at the back of the audience (so positional movement is visible to viewers).
    1. **Move One Step.** Parameterize `AudienceMoveToFrontAction` with `stepsPerTurn: int` (default 1). One position forward per turn. Replaces the existing jump-to-front behavior.
    2. **Heckle.** Single-musician composed action: `ApplyStatusEffect(exposed, 1, MusicianCharacter)` + `AddStressAction(N, MusicianCharacter)`. Covers single-target Stress + adds Exposed coverage to the demo.
    3. **Indifference (self).** New audience-side `StatusEffectSO` mirroring Composure's pattern but blocking *all* incoming Vibe (D-DCP-6=A semantic). Requires implementing the deferred `ApplyIncomingVibe` helper on `AudienceCharacterStats` (the hook documented in `CURRENT_STATE.md §4` open-items and `planning/Design_Audience_Status_v1.md`). Pattern mirrors M4.1's `ApplyIncomingStressWithComposure` fix.
  - **Kid — 2 abilities total.**
    1. **(Existing) band-wide Stress ability** preserved.
    2. **New buff on Cool Dude's outgoing Stress.** Applies a new audience-side stacking status (tentatively `Egged On`) to Cool Dude that increases the Stress amount of Heckle per stack. Tuning (`+N stress per stack`) deferred to authoring time. The buff creates a targeting decision for the player: convince Kids first to disarm Cool Dude, but Cool Dude is physically harder to reach (Move One Step keeps him at the back).
  - **Encounter authored:** 2× Kid + 1× Cool Dude as the demo encounter, saved as a `GigEncounterSO` asset registered in `GigSetupRoster.AvailableEncounters`. This is the encounter the Demo cut prep batch (§5.3.5) wires into `DemoLaunchConfigSO`.

**Items deferred:**
- **#13 Audience Member Wizard Editor** — D4=B, post-demo.

**Decisions to lock at B3 open (audience pool sub-scope):**
- **D-CoolDude-1** Move One Step via parameterization (`stepsPerTurn`), not a new action subclass.
- **D-CoolDude-2** Heckle composed from `ApplyStatusEffectSpec(exposed)` + `AddStressAction`, not a single new spec.
- **D-CoolDude-3** Indifference blocks *all* incoming Vibe (song-end conversion + Earworm tick + direct ModifyVibe). Per D-DCP-6=A.
- **D-Kid-buff** New `Egged On` status on Cool Dude (audience-side outgoing-Stress modifier). `+N stress per stack`, N tunable.
- **D-Sibi-instrument** β path: new `InstrumentEffect` SO for Singing Field. Specific MIDI program at authoring time.

**DoD:**
- Deck balance feels intentional in playtest.
- BPM cards demonstrate; modulation cards demonstrate (with persistence behind them).
- Sibi's voice instrument audibly distinct from C2's parts.
- Both audience archetypes playable; the demo encounter (2×Kid + 1×CoolDude) produces interesting targeting decisions.
- `ApplyIncomingVibe` is the single canonical path for ALL incoming Vibe on audience (no bypass).
- Indifference + Earworm interaction visible in playtest (Earworm tick suppressed on Indifferent target).
- Smoke tests ST-B3-S1..S7 (one per item: balance, BPM, modulation, Sibi instrument, Move One Step, Heckle + Exposed, Indifference + ApplyIncomingVibe + Earworm interaction, Egged On buff, encounter integration).

### 5.3.5 — Demo cut prep (opened 2026-05-15)

Mini-batch dedicated to wiring the demo build entry path and shipping demo-specific tuning + the SFX→FlatVibe mechanic. Depends on B3 close.

**Goal:** Build de demo arranca sin interacción de setup. Encuentro de demo (2×Kid + 1×CoolDude, authored in B3) es ganable (~60-80% win rate) con el starter deck post-B3. SFX activations producen un bonus FlatVibe visible que recompensa al jugador por superar SongHype thresholds.

**Decisions locked at open (DC-1 through DC-6):**
- **DC-1=C** Quick-start flag on `GigDevSettingsSO` (D-DCP-1=A: dev-side flag locality).
- **DC-2=Custom** Audience pool = 2× Kid + 1× Cool Dude (authored in B3 #12).
- **DC-3=Custom** 4 songs × 1 part × 4 loops/part + SFX→FlatVibe new mechanic.
- **DC-4=B (moderate)** Initial Inspiration=3, per-loop=1; refinable in playtest.
- **DC-5=B** Batch placed between B3 close and §5.4 Demo readiness review.
- **D-DCP-2=A** SFX bonus defaults: Stage1=+3, Stage2=+6, Stage3=+10 Vibe. Scaled / "encore" feeling. Tunable on `GigPresentationSO`.

**Items:**
1. **Quick-start path.** New flag `autoStartFromDefaults: bool` on `GigDevSettingsSO` + serialized reference to a `DemoLaunchConfigSO`. `GigSetupController.Start()` auto-invokes `OnStartPressed()` when both are set. Production builds keep flag off → normal Gig Setup interaction preserved.
2. **`DemoLaunchConfigSO` new SO.** Baked: roster (C2 + Sibi), encounter reference (the B3 #12 encounter), `requiredSongCount=4`, `JamRules` overrides (1 part × 4 loops/part), `initialGigInspiration=3`, `inspirationPerLoop=1`.
3. **SFX→FlatVibe mechanic.** Three new tunable floats on `GigPresentationSO` (`sfxBonusVibeStage1/2/3`, defaults 3/6/10). `GigManager.AddSongHype` applies the bonus on upward threshold crossing (hooks into the existing `_songHypeStage` tracker from B2 #6). Bonus is applied **post-Flow** — flat addition at end of resolution, not Flow-scalable. Floating text "+N Vibe!" on band canvas (not per-audience).
4. **Tuning + validation.** 8-10 playthroughs **with the B3-authored audience pool (2×Kid + 1×CoolDude)**. Adjust Inspiration values and/or SFX bonus values until target win rate 60-80%.
5. **Coverage matrix doc.** New planning doc `planning/Design_Demo_Cut_v1.md` listing implemented effects / modifiers / statuses and whether each is represented in the demo content, with rationale. Target cobertura: CardEffectSpec 4/4 (100%); StatusEffect 5/7 (71%, +3 over pre-batch via Cool Dude's Heckle + Indifference + Kid's Egged On); PartEffect families ~75% (Meter ✓, Tempo via B3 #10, Tonality via B3 #11, Instrument via B3 #11.5).

**Out of scope:**
- Authoring audience abilities (lives in B3 #12 expanded).
- Deck balance pass (B3 #9).
- Pitch deck refresh (separate stream §6).

**DoD:**
- Demo build entry: zero clicks from launch to action window of song 1.
- 4-song gig completes end-to-end with starter deck + demo encounter.
- SFX bonus fires audibly + visibly on each upward threshold crossing.
- Win rate measured ≥60% across 8-10 playthroughs.
- `Design_Demo_Cut_v1.md` shipped with coverage matrix.
- Smoke tests ST-DCP-S1..S5 (entry path, encounter wiring, SFX bonus visibility, tuning win-rate validation, coverage matrix completeness).

**Files (likely):**
- `GigDevSettingsSO.cs` (new `autoStartFromDefaults` + `demoLaunchConfig` fields).
- `DemoLaunchConfigSO.cs` (new SO type).
- `GigSetupController.cs` (auto-start branch in `Start`).
- `GigPresentationSO.cs` (3 new tunable floats).
- `GigManager.cs` (SFX bonus integration in `AddSongHype` / `AddSongHypeCore`).
- New SO assets: `DemoLaunchConfig.asset`, demo encounter `.asset`.
- `planning/Design_Demo_Cut_v1.md` (new doc).

### 5.4 — Demo readiness review

Post-B3. Confirms demo cut. Items checked:
- Persistence between loops works as designed.
- UI feedback polish lands.
- Animation polish lands.
- Content balance feels right.
- New audience member produces interesting decisions.
- F-1, F-3, F-4, MB1-MB4, M4.5 invariants all clean.

If pass: Phase B closed; demo cut tagged; publishers/community can be shown.
If gaps: targeted follow-up batches before cut.

### 5.5 — Phase B Definition of Done

- [x] B1 (loop simplification + persistence + UI rework) closed (2026-05-12)
- [x] B2 (feedback + animation polish) closed (2026-05-13)
- [x] B2.5 (polish refinements + cleanup) closed (2026-05-15)
- [ ] B3 (balance + new content + 2-archetype audience pool + Indifference status) closed
- [ ] Demo cut prep closed (§5.3.5)
- [ ] Demo readiness review passed (§5.4)
- [ ] No F-1/F-3/F-4 invariant regressions
- [ ] CURRENT_STATE + Roadmap + changelog reflect closure
- [ ] No SSoT promotions or authority changes for §5.3.5 (operational); B3 requires `SSoT_Status_Effects.md` + `SSoT_Audience_and_Reactions.md` edits at closure (Indifference + `ApplyIncomingVibe` are real semantic additions)

## 6 — Marketing stream — Pitch deck refresh (opened 2026-05-15)

**Status:** Non-governance, non-code stream. Parallels Phase B. Does not affect SSoTs, manifests, coverage matrices, or contracts. Tracked here for visibility; primary artifacts live under `planning/marketing/`.

**Goal:** Replace the August 2025 `GoblinzStudio.pdf` deck with a v2 reflecting the current state of the project (demo cut as "today's product", broader vision as "post-funding roadmap"), packaged with gameplay video + (target) playable build.

**Decisions locked at open:**
- **PD-1=C** Borrador antes del demo cut; versión final tras §5.4. Per interpretation α (confirmed): borrador es sesión informal sin slot de roadmap; el batch formal post-§5.4 produce versión final.
- **PD-2=B** Demo cut como "today's product"; visión amplia separada explícitamente como "post-funding roadmap".
- **PD-3=C target / B minimum** Apuntar a deck + video + playable build empaquetada; aceptar B (deck + video) si C resulta bloqueado por Player-build issues de MidiGenPlay.
- **PD-5=B** Batch propio post-§5.4, en serie con el resto del roadmap.

**Sub-batches:**
- **A — Audit + outline + draft text (no media).** Puede ejecutarse como sesión informal pre-§5.4 (per PD-1=C interpretation α). Locks structure, positioning, and copy. Captures Cristian Pretty Soon Games meeting context if it lands before §5.4.
- **B — Media capture from demo cut.** Screenshots, GIFs cortos, 60-90s gameplay video. Requires demo cut shipped (post-§5.4).
- **C — Final assembly + packaging.** Deck PDF + video + (target) build packaged. Old deck archived to `planning/marketing/archive/GoblinzStudio.pdf`.

**Deliverables:**
- `planning/marketing/pitch_deck_v2.pdf` (replaces `GoblinzStudio.pdf`).
- `planning/marketing/pitch_video_v2.mp4` (60-90s).
- (Target) Build packaged (Steam key, itch.io, or local build).

**Information already collected (PD-4):**
- **Partners:** BCS Studios (art), Abstract Digital (architecture + QA + porting), CoverSolutions (composer — Sebastián Sanhueza), Bamer29 (additional composer).
- **Core team:** Claudio (director + dev + game design + multi-role); Matías (artist). Composers as collaborators. Possible Sound Designer pending.
- **Timeline:** EA target 2027, v1.0 target 2028.
- **Funding ask:** ~€200k (research-based against current publisher offers; specific figure to refine at sub-batch A).
- **Target audience:** Cristian (BCS Studios CEO) meeting with Pretty Soon Games (Jakub Radkowski, CEO) at Digital Dragons 2026 is the immediate test case. Goblinz Publishing as warm contact. Primary purpose: internal reference for any publisher opportunity.

**Out of scope:**
- Any code change.
- Any SSoT, contract, or governance change.
- Production timeline replanning (separate concern; this batch only reflects what timeline we set).
- Funding strategy beyond pitch positioning.

**Risks:**
- **PD-3=C requires Player-build packaging.** MidiGenPlay package-internal `MidiGenPlayConfig.GetChordWriteFolder` / `GetProfileForTonality` Player-build errors are an open follow-up (see `CURRENT_STATE.md §3` post-demo follow-ups). If unresolved, fallback to PD-3=B (deck + video, no playable build).

**DoD:**
- v2 deck PDF complete, internally reviewed.
- Video captures key demo gameplay beats (composition card → music change, audience targeting, SFX threshold bonus, win/lose resolution).
- Old deck (`GoblinzStudio.pdf`) archived to `planning/marketing/archive/`.
- Communication kit assembled (deck + video + optional build link) ready for outreach.

## Post-MVP — Pending Effects system (planned, first post-MVP batch)

Scope: ship the song-scoped accumulator layer described in `planning/Design_Pending_Effects_v1.md`. First user is deferred Earworm; multiplier cards introduced as content. Bucket lives on `CompositionSession`, resolves on `OnCompositionSongFinished` after `RunSongVibeResolution` and before audience-turn `Tick(AudienceTurnStart)`. Conditional resolution slot present in the data structure but hardcoded to always-resolve in MVP+1.

Out of scope for this batch:
- generalization to pending Vibe / Stress / Flow / Cohesion (subsequent batches),
- conditional resolution predicate beyond the always-true MVP+1 placeholder,
- tempo-coupled multiplier cards (long-term, see below).

Definition of done: at least one new pending-applier card and one multiplier card playable in a normal gig; bucket resolves correctly at song end; smoke tests cover bucket lifecycle (reset, accumulate, multiply, resolve, no leak across songs).

Docs at closure: new SSoT `SSoT_Pending_Effects.md` (or equivalent location), `SSoT_Card_System.md` updated to recognize pending-effect specs as a category, `SSoT_Status_Effects.md §5.7` Earworm note that a pending-applier authoring path exists, `SSoT_Runtime_CompositionSession_Integration.md` updated for song-scoped bucket lifecycle, planning doc `Design_Pending_Effects_v1.md` partially superseded.

## Post-MVP — Tempo-coupled card identity (design direction, no implementation slot)

Long-term design pillar, deferred. Captured in `planning/Design_Tempo_Identity_v1.md`. Influences starter deck and per-musician catalog design choices via flavor / naming / archetype lean (see `Design_Starter_Deck_v1.md`); no runtime work scheduled. Implementation, when it eventually begins, will be downstream of: M4.6 demo closure, Pending Effects shipping, and enough playtest evidence to know what existing meters feel like before adding tempo as an input axis.

### Definition of Done

- [x] M4.1 Fix C1 — `AddStressAction` unified through `ApplyIncomingStressWithComposure` (ST-M41-1..4 passed 2026-04-26)
- [x] M4.2 Flow bifurcation + adaptive LoopScoreCalculator (ST-M42-1/1c/3/4/5/9/10/11 passed 2026-04-28)
- [x] M4.3 Earworm status implemented end-to-end (ST-M43-1a/1b/2/3/4/5/6/7/8 PASS 2026-04-28)
- [x] M4.4 Deck Contract Evolution — card copies honored at runtime and in Deck Editor (closed 2026-04-29, ST-M44-1..8 PASS, ST-M44-9 deferred, ST-M44-10 N/A)
- [x] M4.5 Bidirectional guaranteed draws
- [ ] M4.6 Starter deck v1 authored, registered, and validated end-to-end
- [ ] M4.6-followup mini-milestone (F-1..F-5) — opened 2026-05-06, gates the M4.6 demo
- [x] `CompositionCardPayload.effects` support verified (2026-04-23, ST-M13c-6)
- [ ] Runtime tuning values received and applied (blocks M4.6 tuning pass only, not earlier batches)
- [ ] Relevant SSoTs updated at each batch closure (full map in `CURRENT_STATE.md` §5 and this milestone)

### Demo-readiness check

- **Demonstrable:** A full gig played with the 2-musician starter band. Player draws cards, plays composition cards that shape the song, plays action cards between songs, applies Earworm to an audience member who then passively gains Vibe on subsequent audience turns, sees Flow stack on Four on the Floor and feel it amplify both action-card vibe and the next song-end conversion. Wins by convincing all audience members, or loses on Cohesion.
- **Viewer sees:** A band of 2 characters on stage. Cards being played, effects resolving with visible numbers and icons. Music audibly changes as composition cards are played. Audience members fill their Vibe meter progressively, with at least one showing an Earworm icon. Clear win/lose resolution.
- **Acceptable rough edges:** 2-musician band is smaller than the final design (Conito + Ziggy deferred). Encounter variety limited to Heckler + Critic placeholders until M3.4. Composition audible-variety is constrained by the narrow composition card pool.
- **Must fix before showing:** Flow must behave consistently across both paths (no double-dipping, no silent zero). Earworm must apply, persist, tick, and decay correctly. Deck copies must load and shuffle correctly. No silent failures on card play. Mind Tap's Earworm application visible via icon and audible/visible Vibe gain on subsequent audience turn. Guaranteed-draw fallbacks must not produce empty phases.

---

## Future milestones (scope only, not yet sequenced)

### Roster Expansion
- Bring Conito (bassist — flight + electricity) into the band. Prerequisite: Bass pipeline validation (currently not on any critical path).
- Bring Ziggy (vocalist — multiharmony) into the band. Prerequisite: `Captivated` audience-side status (CSO `DamageTakenUpMultiplier`) and `ApplyIncomingVibe` helper on `AudienceCharacterStats`. Both deferred from M4.3 to here; design intent recorded in `planning/Design_Audience_Status_v1.md`.
- Per-musician identity cards for Conito and Ziggy (Action + Composition).
- Starter deck revision to 4-musician composition (likely returns to the 8:4 or 7:5 ratios with 4 identity actions and 4 identity compositions).

### Progression & Meta
- Run structure (map, node types, rewards).
- Deck evolution across encounters.
- Musician recruitment and band composition.
- Unlock progression.

### Encounter Design & Balance
- Broader encounter roster beyond Heckler + Critic placeholders.
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

### Authoring tooling QoL (batch (3)) ✅ (closed 2026-05-03)

Editor-only batch promoting authoring ergonomics surfaced during M4.6-prep batch (2) smoke tests. All `#if UNITY_EDITOR` guarded, zero runtime impact. Three deliverables shipped across one new file (`CardInventoryWindow.cs`) and two modified files (`CardEditorWindow.cs`, `DeckEditorWindow.cs`).

Shipped scope:
- **Per-row Starter / Copies columns (3.A)** on `MusicianCardCatalogData` entries list in `CardEditorWindow`. Each row's selection button is now preceded by a `Starter` checkbox (~38 px) and a `Copies` IntField (~40 px, greyed when Starter is off, clamped to ≥1 on commit). Both controls write through `SerializedObject` → `entries[i].flags` / `entries[i].starterCopies` with `ApplyModifiedProperties()` per frame, giving Undo + asset-dirty parity with the right-side inspector. The `[S]` flag indicator is dropped from row labels; the checkbox column is the canonical indicator. `[R]` and `[L]` retained.
- **`CardInventoryWindow` (3.B)** registered at `ALWTTT/Cards/Card Inventory` (priority 12). Four toolbar-selected views: All `CardDefinition` assets; All `MusicianCardCatalogData` (per-asset summary with entry count + starter count + total starter copies); One specific musician catalogue (full entry list, musician via toolbar dropdown); All `GenericCardCatalogSO` (each rendered with full entry list since `Entries` reuses `MusicianCardEntry`). Per-view `Print` (multi-line `Debug.Log`) and `Export JSON` (`EditorUtility.SaveFilePanel` → `JsonUtility.ToJson(_, prettyPrint: true)` → file + auto-reveal). Export schema is human-readable / informational, not designed for round-trip through `DeckJsonImportService`.
- **Toolbar Print buttons (3.C)** on `CardEditorWindow` (after the Registries Ping button, disabled when no catalog loaded) and `DeckEditorWindow` (between Export JSON and Clear All). Card Editor Print produces a `=== CARD EDITOR — CATALOG DUMP ===` block; Deck Editor Print produces a `=== DECK EDITOR — STAGED DECK DUMP ===` block using `StagedCardEntry.ResolvedCard` (handles both existing and pending cards) and reports per-row `count` for M4.4 multiplicity.

Decision matrix at open: D1 menu path → `ALWTTT/Cards/Card Inventory` (priority 12); D2 export schema → human-readable informational; D3 "Validate `CardBase` prefab variants" appendix → **deferred** (logged in `SSoT_Editor_Authoring_Tools.md §14.5` as a candidate authoring-tool addition for a future QoL pass); D4 per-row layout density → fixed widths (Starter 38 px / Copies 40 px); D5 silent disappearance on filter interaction → accepted (matches right-side inspector convention); D6 Card Editor Print button placement → toolbar (not entries-list header).

Smoke tests ST-AT3-1..8 all PASS:
- ST-AT3-1 per-row Starter toggle commits to asset and persists across reload — PASS
- ST-AT3-2 Copies field disable + clamp to 1 on commit — PASS
- ST-AT3-3 filter interaction silent disappearance — PASS
- ST-AT3-4 Undo reverts both flag and copies as one step — PASS
- ST-AT3-5 CardInventoryWindow all four views populate, Print + Export succeed — PASS (`inv1.json`/`inv2.json`/`inv3.json`/`inv4.json` exports verified)
- ST-AT3-6 Print buttons on both windows produce formatted multi-line output — PASS
- ST-AT3-7 regression: per-row controls do not steal selection — PASS
- ST-AT3-8 dogfood acceptance: cleanup workflow materially faster than right-side inspector — PASS ("very good cleanup process")

**Critical scope honesty.** Batch (3) ships the *tooling* needed to execute the M4.6 starter-deck cleanup. The *content cleanup itself* (pruning the four musician catalogues from their pre-batch state of 28 entries all StarterDeck-flagged to the 12-card / 7-unique / 2-musician Cantante+Sibi composition specified in `Design_Starter_Deck_v1.md §4`) is a **separate follow-up**. ST-AT3-8 demonstrated the workflow on at least one musician but the test does not assert that all four catalogues match the design spec. Pre-batch-(3) snapshot in this session's `inv2.json` provides a clean before-state baseline. The pre-demo blocker tracked in `CURRENT_STATE.md §4` is now **structurally tractable** but **content-status undetermined**; recommended verification: re-export `CardInventoryWindow > All Musician Catalogs` post-cleanup and diff against `Design_Starter_Deck_v1.md §4`.

Docs at closure: `SSoT_Editor_Authoring_Tools.md` updated (§3 inventory row added, §4.6/§4.7 Card Editor sections added, §5.7 Deck Editor section added, new §8 `CardInventoryWindow` section inserted, §9–§15 renumbered, §13 file list and §14.5 prefab-variant validator candidate added); `CURRENT_STATE.md` §1 closure block + §3 M4.6 line update + §4 batch (3) bullet flipped to RESOLVED + all-starter-flagged bullet rewritten + §5 entry; this Roadmap entry; `changelog-ssot.md` new top entry. No SSoT contract change. No `ssot_manifest.yaml` change. No `coverage-matrix.md` change. No new authority introduced — `CardInventoryWindow` is operational tooling, not a contract owner.

### Inventory viewer fixes (UI-fix-A + UI-fix-B) ✅ (closed 2026-05-02)

Combined closure of two UI-fix batches surfaced during M4.6-prep batch (2) smoke tests. Both pre-existing, both player-facing, both demo-relevant for M4.6.

**UI-fix-A — Inventory viewer prefab NRE.** `CardBase.SetCard` at `CardBase.cs:77` threw `NullReferenceException` on Draw/Discard/Hand pile open. Root cause: inventory canvas instantiates `CardUI.prefab` (an empty subclass `CardUI : CardBase {}` assigned to `InventoryCanvas.cardUIPrefab`); two `[SerializeField]` TMP refs were unassigned (`inspirationCostTextField`, `inspirationGenTextField`). Asset-only fix on `CardUI.prefab` — wired both refs. `CardBase.SetCard` kept strict (no defensive null guards). Smoke tests ST-INV-1..6 PASS. Structural finding parked in `CURRENT_STATE.md §4`: the two-prefab arrangement is the recurrence vector for unwired-`SerializeField` bugs; cleanup options (α) collapse to one prefab, (β) Prefab Variant logged.

**UI-fix-B — Inventory scrollbar functional.** ScrollRect snap-back / no visible scrollbar despite content overflow. Root cause: `Content` had `ContentSizeFitter` but no `LayoutGroup` to feed it preferred height; `Viewport` had `Mask` + a disabled `Image` (broken masking). Fix is asset-only on `InventoryCanvas.prefab` plus a small code edit on `InventoryCanvas.cs`. Asset edits: `VerticalLayoutGroup` on `Content`; `LayoutElement` on `FilterPanel` (PreferredHeight=100), `CardSpawnRoot` (PreferredHeight=2050), `SongSpawnRoot` (PreferredHeight=800); `RectMask2D` replaces `Mask`+disabled `Image` on `Viewport`; `CardSpawnRoot` Grid padding Top trim. Code edits: `[SerializeField] ScrollRect scrollRect`; at end of `SetCards`/`SetSongs`, `Canvas.ForceUpdateCanvases() + LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content) + verticalNormalizedPosition=1f`. Smoke tests ST-SCR-1/3/4/6/7 PASS, ST-SCR-2 FAIL ACCEPTED as paper cut (vacuous overflow with fixed `LayoutElement` height — follow-up via dynamic height computation logged in `CURRENT_STATE.md §4`), ST-SCR-5 DEFERRED-by-construction (no Songs inventory content reachable).

Docs at closure: `CURRENT_STATE.md` §1 two new closure blocks + §4 open-item closures and three park-lot additions; this entry; `changelog-ssot.md` combined top entry. No SSoT change. No `ssot_manifest.yaml` change. No authority change.
### Gig Setup roster pickers (merged (1)/(4)) ✅ (closed 2026-05-04)

Bidirectional band + audience multi-select pickers shipped in the Gig Setup scene. Closes the open items *"Musician picker in Gig Setup"* (surfaced M4.2, 2026-04-28) and *"Gig Setup roster pickers"* (deferred from M4.3 surfacing). `pd.MusicianList` is now mutated by the picker before auto-assembly runs; `GigEncounterSO.audienceMemberList` becomes the *default* audience composition with a per-run override path via `GigEncounterSO.BuildRuntime(audienceOverride)` and `RunConfig.audienceOverride`.

**Shipped scope:**
- Two new files (`MusicianPickerRow.cs`, `AudiencePickerRow.cs`) + matching prefabs.
- Five modified: `PersistentGameplayData.cs` (new `SetBandRoster(IList<MusicianBase>)`), `GigSetupConfigData.cs` (new `availableAudienceCharacters`, `maxAudienceCount`), `GigEncounterSO.cs` (new `BuildRuntime(IList<AudienceCharacterData>)` overload, regression-safe null fallback), `GigRunContext.cs` (new `RunConfig.audienceOverride`), `GigSetupController.cs` (picker fields, build/handler logic, validation, override decision; new serialized `gameplayData` field defensive against `Awake`-order singleton issues).
- GigSetupScene prefab + GigSetupConfig SO populated.

**Decision matrix:** D1=B (new `pd.SetBandRoster` method); D2=A (audience pool field on `GigSetupConfigData`); D3=B (toggle-list UI for both pickers); D4=remember-last + reset-on-encounter-swap; D5=band 1-4 / audience 1-`MaxAudienceCount` (band warns at 1, blocks at 0 or >4); D6=B+C combined (`BuildRuntime(audienceOverride)` overload + `RunConfig.audienceOverride` field); D7=A (single merged batch).

**Audience-override decision rule** is multiset-blind on baked duplicates (post-batch fix): the picker UI dedups `AudienceCharacterData` by reference, so a no-customization run produces `pickedCount == bakedSet.Count` (unique-count). `DiffersFromEncounterAudience` builds `bakedSet` first, then compares against `pickedCount`. Encounters with duplicate baked entries (e.g., `[A, A, B]`) preserve duplicates at runtime when user does not customize; override stays null and `BuildRuntime` falls back to baked. When user customizes, multiplicity is lost for that run (picker UI is single-row-per-unique-SO). Multiplicity-aware picker UI (per-row count input + multiset-aware comparator) is tracked as a future batch (6).

**Smoke tests** ST-M46p4-1..10 all PASS (10/10):
- ST-M46p4-1 band picker basic — PASS
- ST-M46p4-2 auto-assembly content respects picker — PASS (with spec addendum: generic catalog contributions are expected on top of per-musician)
- ST-M46p4-3 empty band guard — PASS
- ST-M46p4-4 single-musician warning (non-blocking) — PASS
- ST-M46p4-5 audience picker basic + override (`override=True` path) — PASS
- ST-M46p4-6 audience override null path regression (`override=False`) — PASS
- ST-M46p4-7 audience max-count enforcement — PASS
- ST-M46p4-8 encounter-swap audience reset — PASS
- ST-M46p4-9 legacy regression (band picker + `BandDeckData` dropdown) — PASS
- ST-M46p4-10 multiset-blind override preserves baked duplicates (added during validation) — PASS

**Side-findings:**
- `GameplayData` null at `Awake` time → reworked to prefer serialized field, `GameManager.Instance.GameplayData` as defensive fallback. Note: `GameplayData` on `GameManager` is an instance property, not static.
- `RectTransform`-parenting warning → `Instantiate` + `SetParent(content, worldPositionStays: false)` pattern applied to both pickers.
- Multiset-blind comparator (option-B fix, ~5 LoC) applied during validation. ST-M46p4-10 added to validate.
- Audience picker multiplicity follow-up → tracked as batch (6) Audience picker multiplicity. Not blocking M4.6 demo gate. Touches `AudiencePickerRow` (per-row count input UI), `GigSetupController.BuildAudiencePicker` (compute baked counts), `GigSetupController.GetSelectedAudience` (multiset materialization), `DiffersFromEncounterAudience` (multiset comparison replaces multiset-blind workaround), and adds 3+ smoke tests.

**ST-M42-6/7/8/9** (deferred from M4.2 closure 2026-04-28 because they required a 2-musician band selection mechanism) are now **unblocked** but not yet executed. They may run in parallel with M4.6 demo prep or post-demo.

Docs at closure: `SSoT_Gig_Encounter.md` §7 new "Roster picker boundary" section + §§7-11 → §§8-12 renumber; `SSoT_Editor_Authoring_Tools.md` new §16 "Configurable runtime SO surfaces (Inspector-only)" with §16.1 `GigSetupConfigData`; `Design_Starter_Deck_v1.md` new §3.3 "Selection mechanism"; `CURRENT_STATE.md` §1 closure block + §3 M4.6 line update + §4 two open items flipped to RESOLVED; this Roadmap entry; `changelog-ssot.md` new top entry; `ssot_manifest.yaml` new entries under `SSoT_Gig_Encounter`.

### M4.6-prep cleanup — Starter deck authoring + Card Editor tooling ✅ (closed 2026-05-06)

Closes the pre-demo blocker that was tracked since M4.6-prep batch (2). 10 cards authored from scratch via JSON Import targeting Robot (4 cards) + Gusano (4 cards) + Generic (2 cards), final composition: Robot 4/4/5, Gusano 4/4/4, Generic 2/2/3 — matching `Design_Starter_Deck_v1.md §4`. Existing test/scaffold cards in Robot/Gusano subfolders deleted by user. Cantante (7/7) and Conito (10/10) catalogs intentionally untouched but inert (not in M4 demo roster).

**Two Card Editor tooling patches shipped alongside:**
- Patch 1 — Status dropdown classified (`DrawStatusEffectPicker` reads both `StatusCatalogueMusicians` + `StatusCatalogueAudience`, hierarchical `Musicians/...` / `Audience/...` paths via `DropdownButton + GenericMenu`). Closes the `Card Editor inline effects-block UI on legacy catalogue alias` open item from `CURRENT_STATE.md §4`.
- Patch 2 — Catalog Source toggle (`CatalogSource { Musician, Generic }`; in Generic mode auto-loads `GenericCardCatalogSO` via name-heuristic; entry list rendered with batch (3.A) per-row Starter UI; write paths NOT Generic-aware in this iteration).

**Smoke tests** ST-SD-1..8: 7/8 PASS, 1 reclassified DEFERRED-by-design. ST-SD-7 (Singing Field inherits progression) failed because Wormus Minor (Backing) and Singing Field (Melody) both have `FixedPerformerType: Sibi` and the runtime model enforces "one musician = one track active at a time" — second card replaces first. Model invariant, not cleanup defect. Test re-formulation deferred to roster expansion.

**Post-closure verification:** Patch 2's flagged latent-bug concern (toggle-to-Generic not clearing loaded musician state) was verified resolved in code at apply time — `CardEditorWindow.cs:244-249` correctly clears `_loadedCatalog` and `_loadedMusicianData` on switch-to-Generic; no fix required.

**Findings during smoke testing opened a new mini-milestone**, M4.6-followup (5 batches F-1..F-5). See §4.6-followup above. The M4.6 demo gate is now blocked on F-1..F-5 closing rather than on starter deck cleanup itself.

Docs at closure: `CURRENT_STATE.md` §1 new closure block + §3 M4.6 line update + §4 two items flipped to RESOLVED + 8 new bullets added + §5 entry; this Roadmap entry; `changelog-ssot.md` new top entry; `SSoT_Editor_Authoring_Tools.md` new §4.9 (Catalog Source toggle + classified status dropdown). New planning doc `planning/Design_Song_Parts_Library_v0_1.md` (status: planning-only, future design intent for Song Parts Library — Intro/Verse/Chorus/Outro stored & repeatable). No SSoT contract change. No `ssot_manifest.yaml`, `coverage-matrix.md`, `SSoT_INDEX.md`, `SSoT_CONTRACTS.md` change.

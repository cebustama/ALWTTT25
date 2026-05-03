# Roadmap — ALWTTT

**Status:** Planning only — does not define implementation truth  
**Last updated:** 2026-05-03 (M4.6-prep batch (3) Authoring tooling QoL closed)
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

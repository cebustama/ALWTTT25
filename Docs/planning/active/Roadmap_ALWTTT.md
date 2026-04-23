# Roadmap — ALWTTT

**Status:** Planning only — does not define implementation truth  
**Last updated:** 2026-04-23  
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

**Priority order (revised 2026-04-23):** Game-feel prerequisites are done (M1.5 Phases 1–2, M1.7, M1.8 all closed). M1.3 was originally a single batch; during scoping on 2026-04-20 it was expanded beyond its original scope (card-hover stacked tooltips, right-click detail modal, card sizing), and the user decomposed it into five sequenced batches. Current M1 sequence:

1. **M1.3a** Status descriptions + central effect-to-text builder + card-face text fix + per-icon character tooltip. ✅ (closed 2026-04-23).
2. **M1.3c** Card-hover stacked tooltip panel (Monster Train-style). ✅ (closed 2026-04-23).
3. **M1.10** Right-click card detail view modal — the home for full composition detail and any text that no longer fits the card face. ✅ (closed 2026-04-23).
4. **M1.3b** `SpecialKeywords` enum + data audit + JSON importer improvements. ✅ (closed 2026-04-23).
5. **M1.9** Card size + hover growth refactor — `HandController` gains code-configurable base and hover sizes with curve reflow.
6. **M1.5 Phase 3** Dev Mode stat & state editing — sub-roadmap recommended before implementation.
7. **M1.1** Deck Editor polish — authoring comfort.

Reasoning: M1.3a is the lowest-risk step that fixes the enum-name bug visible in every test and establishes the central builder. M1.3c (stacked tooltips) unblocks the hover-UX conversation. M1.10 provides the home for cut composition text. M1.3b then removes legacy keywords with confidence. M1.9 is polish that wants stable content underneath it. M1.5 Phase 3 is a developer affordance, not player-facing. M1.1 is authoring comfort.

### 1.1 — Deck Editor polish
- Better catalogue filters (by musician, by kind, by effect type).
- Card preview info in staged card list (effect summary, cost, kind badge).
- Cross-tool integration: Open in Card Editor, Ping Card in Project.
- Final validation pass.

### 1.2 — Status Icons pipeline migration ✅ (closed 2026-04-14)
- ~~Migrate `StatusIconsData` / `StatusIconData` from legacy `StatusType` enum to the SO-based system.~~
- ~~Create or assign icons for the six Combat MVP statuses.~~
- ~~Stack count display on character canvas (musician + audience).~~

**Outcome beyond original scope:** `StatusIconsData` indirection removed. Sprite authority on `StatusEffectSO.IconSprite`. `CharacterCanvas` reads directly from `StatusEffectContainer`. Additional polish: auto-rename of SO asset, catalogue validation fix, `BandCharacterStats.ApplyStatus(StatusType)` marked `[Obsolete]`.

**Multi-turn validation — outcome against M1.5 Phase 2 (2026-04-20):**
- T5 Choke decay at `PlayerTurnStart` tick — ✅ PASSED.
- T8 Feedback DoT accumulation — ✅ PASSED (after tick-timing correction).
- T7 Shaken expiry across a song cycle — ⏸️ DEFERRED to M1.5 Phase 3.

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

**Phase 3 — stat & state editing (next on critical path)**
- Runtime edit of Inspiration, LoopScore, SongHype, BandCohesion.
- Per-character stat edits (Stress/MaxStress, Vibe, persuasion progress, status stacks).
- Encounter modifier toggles.
- Audience reaction/ability transparency panel.
- **Breakdown entry point** — force `MusicianBase.OnBreakdown()` on a selected musician. Unblocks T7 Shaken validation.
- Sub-roadmap session recommended before implementation.

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
- [ ] M1.5 Phase 3 Dev Mode stat/state editing + Breakdown entry point (unblocks T7)
- [ ] M1.1 Deck Editor polish items
- [ ] CURRENT_STATE and relevant SSoTs updated at each batch closure

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

**Sequencing:** M4 is sequenced **after M1 closure**. M1.3 decomposition (M1.3a, M1.3c, M1.10, M1.3b, M1.9) is complete. Remaining M1 work: M1.5 Phase 3, M1.1. Within M4, M4.1 is the first batch; M4.2 and M4.3 can run in parallel; M4.4 and M4.5 can run in parallel after M4.1; M4.6 (authoring) depends on all previous.

### 4.1 — C1 fix: unified Stress path (bloqueante)

Route `AddStressAction.DoAction` through `MusicianBase.Stats.ApplyIncomingStressWithComposure` (the helper already used by `CardBase.ExecuteEffects` on `ModifyStressSpec` positive). Composure absorbs audience pressure correctly post-fix.

Scope:
- Modify `CharacterActionProcessor.GetAction(CardActionType.AddStress).DoAction` (or the `AddStressAction` class directly, depending on structure) to route through the helper.
- Grep existing encounter assets in `GigSetupConfigData.availableEncounters` / `EncounterData` for any that were tuned against current-broken behavior. Low risk, but requires a check.
- Update `SSoT_Gig_Combat_Core.md` §5.4 and §6.2 to document the unified path at batch closure.
- Smoke tests: audience `AddStressAction` against musician with N Composure stacks → Composure absorbs first, remainder applies; Exposed multiplier still applies correctly; Breakdown still triggers on overflow.

### 4.2 — Flow bifurcation + `LoopScoreCalculator` retune for 2 musicians

Flow applies differently by card domain:
- **Action cards** with `ModifyVibeSpec` positive: keep current per-card flat bonus. `finalDelta = baseDelta + bandFlowStacks × flowBonusPerStackPerCard`.
- **Composition cards** with `ModifyVibeSpec` positive: new multiplier branch. `finalDelta = round(baseDelta × (1 + bandFlowStacks × flowVibeMultiplier))`.
- **Song End conversion** in `GigManager.ComputeSongVibeDeltas`: new multiplier applied to per-audience `VibeDelta`. `VibeDelta_i = round(baseVibe × impressionFactor × (1 + bandFlowStacks × flowVibeMultiplier))`. Read `bandFlowStacks` before the song-end Flow reset.
- Retire Flow → SongHype multiplier path (documented but never active in runtime). Do not re-activate.

Initial tuning:
- `flowVibeMultiplier = 0.08f` (tunable, surfaced in Inspector).
- `flowBonusPerStackPerCard` keeps current value.

`LoopScoreCalculator` retune for 2-musician baseline:
- `ComputeLoopScore` currently awards +3 per active role (Rhythm/Bass/Harmony/Melody). With C2 + Sibi, Bass role is systematically absent and Melody may be absent (depending on which composition cards dominate the hand). Retune thresholds in `ComputeHypeDelta`, or adjust the per-role weight, so a 2-musician band does not systematically sit in the "neutral" or "slightly bad" bands.
- Exact new values decided at implementation time with reference to runtime telemetry.

Docs at closure: `SSoT_Gig_Combat_Core.md` §6.1, `SSoT_Scoring_and_Meters.md` §7.1.

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

Evolve `BandDeckData` from `List<CardDefinition>` to `List<BandDeckEntry> { card, count }` (or equivalent multiset representation). `PersistentGameplayData.SetBandDeck` respects multiplicity. `CardAcquisitionFlags.starterCopies` stops being authoring-only metadata and is consumed by runtime.

Scope:
- Data contract change on `BandDeckData`.
- Runtime: `PersistentGameplayData.SetBandDeck` iterates entries and adds `count` references to `CurrentActionCards` / `CurrentCompositionCards`.
- Deck Editor UI: staged deck list shows `×N` badge, supports increment/decrement. JSON import/export honors `count`. JSON schema gains a `count` field per card entry (default 1).
- Migration: existing `BandDeckData` assets import cleanly as `count = 1` entries.
- Smoke tests: deck with explicit `×3 Steady Beat` loads into a runtime `DrawPile` with 3 independent `CardDefinition` references; shuffling and drawing treats each correctly; discarding one does not remove the other two.

Docs at closure: `SSoT_Card_Authoring_Contracts.md` (starterCopies is runtime-consumed), `SSoT_Card_System.md` (deck multiplicity semantics).

### 4.5 — Bidirectional guaranteed draws

Two symmetric hooks in the phase transition pipeline:
- **On composition phase entry:** if `HandController.Hand` contains zero composition cards and at least one composition card exists in `DrawPile ∪ DiscardPile`, force-draw one. Resolution strategy for "how to draw only a composition card" when draw is typically random: TBD at implementation (options: priority draw, filtered reshuffle, temporary pile partition). Decision made in batch.
- **On between-songs / action window entry:** symmetric. Zero action cards in hand + at least one available → force-draw one.

Scope:
- `GigManager` phase transition hooks, or wherever the corresponding phase entry currently lives.
- `DeckManager` extension to support filtered draw (or an equivalent).
- Smoke tests: 8 Composition + 4 Action starter, hand size 5. Simulate 100 turns — verify that composition phase never starts with zero composition cards in hand, and action window never starts with zero action cards (given at least one exists in piles).

Does not depend on M4.4 but benefits from it (composition cards have multiple copies, making "at least one exists in piles" near-always true).

### 4.6 — Starter Deck v1 authoring

Author and register the 12-card / 7-unique / 2-musician starter deck per `planning/Design_Starter_Deck_v1.md`.

Preconditions: M4.1 (for honest tuning), M4.2 (Flow model consistent), M4.3 (Earworm exists for Mind Tap), M4.4 (copies supported), M4.5 (guaranteed draws), runtime tuning values received from the user, and verification of `CompositionCardPayload.effects` support (gates `Four on the Floor`'s `ApplyStatusEffect(flow)` co-effect — if unsupported, relocate effect to an action card).

Scope:
- Author the 7 unique `CardDefinition` + payload assets (Warm Up, Take Five, Mind Tap, Steady Beat, Four on the Floor, Synth Pad, Hook Theme).
- Assemble `StarterDeck_v1.asset` via Deck Editor with the copies as specified (`×2 Warm Up`, `×1 Take Five`, `×1 Mind Tap`, `×3 Steady Beat`, `×2 Four on the Floor`, `×2 Synth Pad`, `×1 Hook Theme`).
- Register in `GigSetupConfigData.availableBandDecks`.
- Smoke tests ST-SD-1..6 per `Design_Starter_Deck_v1.md` (deck loads with correct multiplicities, reshuffle preserves counts, Mind Tap applies Earworm with correct stacks, Four on the Floor applies Flow on play, composition cards repeat across songs without runtime warnings, full gig plays end-to-end).

### Definition of Done

- [ ] M4.1 Fix C1 — `AddStressAction` unified through `ApplyIncomingStressWithComposure`
- [ ] M4.2 Flow bifurcation (flat on Action, multiplier on Composition + Song End) + `LoopScoreCalculator` 2-musician retune
- [ ] M4.3 Earworm status implemented end-to-end
- [ ] M4.4 Deck Contract Evolution — card copies honored at runtime and in Deck Editor
- [ ] M4.5 Bidirectional guaranteed draws
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

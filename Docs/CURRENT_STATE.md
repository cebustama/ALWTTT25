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

### Composition / music surface — exists, not yet validated end-to-end
- `GigManager`, `MidiMusicManager`, `CompositionSession`, `SongConfigBuilder`, `LoopScoreCalculator`.
- CompositionSession bypass of phase machine documented (see `SSoT_Runtime_Flow`).
- Not yet tested: composition cards with real gameplay effects producing audible song changes.

### Status icon pipeline — SO-based (M1.2, complete 2026-04-14)
- Sprite authority on `StatusEffectSO.IconSprite`. Lookup asset removed.
- `CharacterCanvas` subscribes to `StatusEffectContainer` events and renders directly from the container's definition.
- Lazy icon lifecycle. Stack count text updates on every change.
- See `SSoT_Status_Effects.md` §3.3.
- **M1.2 multi-turn validation gap:** T5 Choke decay ✅, T8 Feedback DoT ✅ via Dev Mode Phase 2. T7 Shaken expiry deferred to Dev Mode Phase 3 — see §4.

### Dev Mode Phase 1 — complete (2026-04-17)
Infinite turns, F12 IMGUI overlay, hand-visibility bridge. `ALWTTT_DEV` scripting define guards all Dev Mode code. See `SSoT_Dev_Mode.md`.

### Dev Mode Phase 2 — complete (2026-04-20)
Card spawner: Catalogue tab in the overlay, `DeckManager.DevSpawnCardToHand`, gated by `CanDevSpawnToHand` (PlayerTurn + MaxCardsOnHand + hand visibility). Decision U1 codified: spawned cards enter the deck on discard/reshuffle (accepted pollution).

### Latent multi-song action window bug — fixed (2026-04-20)
`GigManager._actionWindowOpen` and `_isBetweenSongs` now re-asserted at every `ExecuteGigPhase(PlayerTurn)` entry. Affected any multi-song gig (production and Dev Mode). See `SSoT_Runtime_Flow.md` §4.1 for the flag lifecycle table.

### Character hover highlight — M1.7 complete (2026-04-20)
URP 2D sprite outline shader, `SpriteOutlineController` (MaterialPropertyBlock, batching-safe). `CharacterBase.OnPointerEnter/Exit` wired. `BandCharacterCanvas` contextual stats present but disabled at prefab level.

### Status icon animations — M1.8 complete (2026-04-20)
`StatusIconBase.PlayAppear()` / `PlayDisappear()`. `[RequireComponent(CanvasGroup)]`. Inspector-tunable durations (default 1s) + AnimationCurves. Race-safe detach-before-disappear in `CharacterCanvas.HandleStatusCleared`. Smoke tests ST-M18-1..5 passed.

### Composition card face description — shortened (2026-04-21)
`BuildCompositionDescription` updated to role/part + `N modifier(s)` count badge only. Style-bundle asset filename no longer appears on the card face. Full modifier list and style-bundle reference will live in the right-click detail view (M1.10).

### M1.3a — complete (2026-04-23)
Card-effect text pipeline rebuilt and per-icon status tooltips wired.
- `StatusEffectSO.Description` field added (`[TextArea]`, 1–2 sentences).
- New `CardEffectDescriptionBuilder` static class under `ALWTTT.Cards.Effects` — single owner of card-effect text formatting for `ApplyStatusEffect`, `ModifyVibe`, `ModifyStress`, `DrawCards`. Uses TMP rich-text colors (buff green, debuff red, numbers amber), hides zero-delta effects, resolves target-type phrasing.
- `CardDefinitionDescriptionExtensions.GetDescription` action branch delegates to the builder. Enum-name leak (`CharacterStatusId` values surfacing on cards with `ApplyStatusEffect`) eliminated.
- `StatusIconBase` gained `IPointerEnter/Exit` handlers + `BindTooltipSource(StatusEffectSO, StatusEffectContainer, CharacterStatusId)`. Hovering a status icon on a character now shows `{DisplayName}` (or `{DisplayName} ×N` when stacked) with authored Description body.
- `CharacterCanvas.TryCreateIcon` wires the tooltip source after `SetStatus`.
- Description text authored on the six canonical status SOs: `flow`, `composure`, `choke`, `shaken`, `exposed`, `feedback`.
- `CardEffectSpec` remains data-only per `SSoT_Card_System.md` §6.1. Formatting is cross-cutting, held centrally.

### M1.3c — complete (2026-04-23)
Card-hover stacked tooltips (Monster Train-style).
- `CardBase.ShowTooltipInfo()` aggregates keywords (via `SpecialKeywordData`) + unique `StatusEffectSO`s extracted from `CardDefinition.Payload.Effects` filtered to `ApplyStatusEffectSpec.status`. Dedupe via `HashSet<StatusEffectSO>`. Display order: keywords first, statuses second.
- Mouse-follow positioning. Position bug root-caused (WorldToScreenPoint on canvas-edge RectTransform through HandCamera produced ~20000px screen coords on a 2560×1440 screen) and fixed by switching to mouse-follow mode.
- Card Editor `AddEffect` bug fixed: `GenericMenu` callback now calls `ApplyModifiedProperties` + `SetDirty` immediately. Fixes effect authoring for both Action and Composition payloads.
- `TooltipController` prefab: `VerticalLayoutGroup` (Upper Left, spacing 5, ControlChildSize Width+Height, padding 5) + `ContentSizeFitter` (Preferred Size on both axes).
- All seven smoke tests pass (ST-M13c-1..7).
- Deferred: raw Inspector `[SerializeReference]` drawer for `CardEffectSpec` (M1.1), composition card face `Effects` display (M4 design decision).
- SSoT edits applied at closure: `SSoT_Status_Effects.md` §3.3, `SSoT_Card_System.md` §10.

### M1.10 — complete (2026-04-23)
Right-click card detail view modal.
- `CardDetailViewController` singleton at `Assets/Scripts/UI/CardDetailViewController.cs`. Dedicated Screen Space – Overlay canvas (sort order 100), dim background with dismiss button, full card detail panel.
- `CardDefinitionDescriptionExtensions.GetDetailDescription()` added — composition cards show primary kind, style-bundle name, full modifier list via `PartEffect.GetLabel()` with scope/timing, and `CardPayload.Effects`.
- `CardBase.OnPointerDown` intercepts right-click → `Toggle(CardDefinition)`. Left-click unchanged.
- `HandController.DisableDragging()` while modal open; `EnableDragging()` on dismiss (Esc, background click, or right-click toggle).
- Smoke tests ST-M110-1..3, 6, 7 pass. ST-M110-4/5 retired (overlay blocks card input by design — close-then-reopen is the intended flow). ST-M110-8 retired (precondition impossible).
- Cosmetic items deferred: "COMPOSITION" word-break, panel overflow on long modifier lists.

### M1.3b — complete (2026-04-23)
SpecialKeywords enum + data audit, JSON importer improvements, Card Editor default fix.
- `SpecialKeywords` enum cleaned to 7 canonical values: `Stress`, `Vibe`, `Convinced`, `Tall` (resource/mechanic/audience) + `Consume`, `Exhaust`, `Ethereal` (card-trait). 6 legacy entries that duplicated status effects removed (`Chill`, `Skeptical`, `Heckled`, `Hooked`, `Blocked`, `Stunned`). Card assets cleaned of stale references.
- `SpecialKeywordData` asset populated with descriptions for `Consume`, `Exhaust`, `Ethereal`. Total 7 entries, one per enum value.
- JSON importer gained `keywords` string array on `CardJsonImport` DTO. Case-insensitive parsing, unknown values warned and skipped.
- JSON batch wrapper gained `defaultEntry` on `CardBatchJsonImport`. Merges into cards with absent/empty-flags entries. `JsonUtility` default-construction handled via `flags` discriminator.
- Exhaust coherence warning: `Debug.LogWarning` when `exhaustAfterPlay` bool and `Exhaust` keyword diverge. Non-blocking.
- Card Editor create wizard resets `Kind` to `Action` on open (fixes dual-button UX trap).
- All eight smoke tests pass (ST-M13b-1..8).
- Keyword model documented in `SSoT_Card_System.md` §3.3. JSON schema additions documented in `SSoT_Card_Authoring_Contracts.md` §5.3, §5.7, §5.8, §7.4.

### M1.9 — complete (2026-04-23)
Card sizing refactor in `HandController`.
- Serialized fields: `cardBaseScale` (float, default 1.0), `cardHoverScaleMultiplier` (float, default 1.25, relative to base), `scaleLerpSpeed` (float, default 12).
- Per-frame `localScale` lerp: cards smoothly grow to `cardBaseScale × cardHoverScaleMultiplier` on hover/drag, return to `cardBaseScale` otherwise.
- Curve reflow: `curveStart.x`, `curveEnd.x`, `handSize.x` multiplied by `HandScaleFactor` (= `cardBaseScale`). Cards at rest don't overlap when base scale changes.
- Proportional scaling: pop-up offset, fanning factor, hover-detection threshold all scale with `cardBaseScale`.
- `UpdateCurvePoints()` runs every frame — Bézier control points and raycast plane recompute from `transform.position`, so moving the `HandController` GameObject at runtime works correctly. Pre-existing bug where the curve didn't follow the GO is fixed.
- `AddCardToHand` sets initial `localScale` to `cardBaseScale` immediately (no pop-in flash).
- `RecalculateCurve()` public method + `OnValidate` (editor-only, play mode) for live Inspector tuning.
- All eight smoke tests pass (ST-M19-1..8) + GO-move verification.
- Temp debug logs tagged `[M1.9]` (12 markers) for diagnostics; strip later.

### Editor authoring tools
- **Card Editor** (`CardEditorWindow`) — single card authoring, JSON batch import.
- **Deck Editor** (`DeckEditorWindow`) — deck authoring with JSON import, catalogue browser, save/save-as, GigSetup registration, JSON export. Core functional; polish items remain.
- **Status Effect Wizard** (`StatusEffectWizardWindow`) — status SO authoring. HelpBox hint corrected 2026-04-20 to point at wired tick timings only.
- **Chord Progression Catalogue Wizard** (`ChordProgressionCatalogueWizard`).
- See `SSoT_Editor_Authoring_Tools.md`.

### Documentation
Governance migration complete. All subsystem SSoTs active and replacement-ready.

---

## 2. Active work

### M1.3 decomposition — five sequenced batches (2026-04-21)
Original M1.3 scope expanded after UX review and split into five batches. Order: **M1.3a ✅ → M1.3c ✅ → M1.10 ✅ → M1.3b ✅ → M1.9 ✅**. All five batches closed 2026-04-23. See `Roadmap_ALWTTT.md` §1.3 for full scope per batch.

- **M1.3a — closed 2026-04-23.** See §1.
- **M1.3c — closed 2026-04-23.** See §1.
- **M1.10 — closed 2026-04-23.** See §1.
- **M1.3b — closed 2026-04-23.** See §1.
- **M1.9 — closed 2026-04-23.** See §1.

### M4 — Starter Deck Foundations (new track, scoped 2026-04-21, sequenced post-M1)
Design pass closed this date: 2-musician starter (Robot C2 + Sibi), 12 cards / 7 unique, ratio 8 Composition : 4 Action, Flow bifurcation (flat on Action, multiplier on Composition + Song End), Earworm as first audience-side status (assigned to Sibi), `BandDeckData` multiplicity support, bidirectional guaranteed-draw mitigation, C1 fix bloqueante. **No code or governed SSoTs modified yet.** See `planning/Design_Starter_Deck_v1.md`, `planning/Design_Audience_Status_v1.md`, and `Roadmap_ALWTTT.md` Milestone 4 for full scope and batch sequence. Sequenced after M1 closure — M1.3 decomposition complete, M1.5 Phase 3 and M1.1 remain.

### Dev Mode Phase 3+ — stat & state editing (next on critical path)
Runtime editing of gig-wide and per-character stats; encounter modifiers; audience transparency panel; **Breakdown entry point** (unblocks T7 Shaken validation). Sub-roadmap recommended before implementation.

### Deck Editor — polish pass (non-blocking)
Better catalogue filters, card preview info in staged list, cross-tool integration (Open in Card Editor, Ping Card in Project).

### Contextual stats on hover — feature disabled (2026-04-20)
`BandCharacterCanvas` hover-to-show-stats path present in code but disabled at prefab level (`statsCanvasGroup` / `statsRoot` unassigned, `StatsRoot` GameObject off). Silent no-op. Revisit when visual density is tuned.

### Editor tooling documentation — complete
`SSoT_Editor_Authoring_Tools.md` active and registered.

---

## 3. What is next

1. **M1.5 Phase 3** — Dev Mode stat & state editing + Breakdown entry point. Sub-roadmap session recommended. **Critical path.**
2. **M1.1** — Deck Editor polish.
3. **M4 Starter Deck Foundations (new track)** — sequenced after M1 closure. First batch: **M4.1 Fix C1** (`AddStressAction` → `ApplyIncomingStressWithComposure`). Full sequence in `Roadmap_ALWTTT.md` Milestone 4.
4. **M2 Composition session validation** — unblocked by M1.5 Phases 1–2.

---

## 4. Open items and risks

### Open items (non-blocking)
- **Shaken restrictions:** status applies and expires correctly; no gameplay gate yet. Design decision still open.
- **Audience Feedback DoT:** no Stress path on `AudienceCharacterBase`. Deferred.
- **Composure penalty during Shaken:** design intent only; not code-enforced.
- **True card copies in decks:** runtime deduplicates by reference. Promoted to active work as M4.4 (Deck Contract Evolution) under the starter-deck track.
- **T7 Shaken expiry validation:** deferred to Dev Mode Phase 3. Unblock path: Dev entry point to force `MusicianBase.OnBreakdown()`.
- **Choke-on-stunned design decision:** T5 surfaced that `HandController.TryResolveCardTarget` refuses stunned musicians. MVP decision: keep the refusal.
- **`CardActionTiming` default excludes PlayerTurn:** documented in `SSoT_Dev_Mode.md` §8.4 and `SSoT_Card_Authoring_Contracts.md` §3.4.
- **C1 — `AddStressAction` bypasses Composure (audit finding 2026-03-20, reaffirmed as bloqueante 2026-04-21):** audience's main pressure channel routes through `musicianStats.AddStress()` directly, not through `ApplyIncomingStressWithComposure`. Composure does not absorb audience pressure. Invalidates honest encounter tuning. Scheduled as M4.1 — the first batch of the starter-deck track and prerequisite for any encounter balance work.
- **`CompositionCardPayload.effects` support — verified (2026-04-23):** ST-M13c-6 confirmed that `CardPayload.Effects` on composition cards works end-to-end (status tooltip appears on hover, effect authored via Card Editor). `Four on the Floor`'s `ApplyStatusEffect(flow)` co-effect is viable.
- **Raw Inspector `[SerializeReference]` drawer for `CardEffectSpec`:** Unity's default property drawer doesn't show a type menu for new list elements. Card Editor window is the intended authoring path. Defer custom drawer to M1.1.
- **Composition card face does not surface `CardPayload.Effects`:** by design (2026-04-21 simplification). Tooltip covers discoverability. Design question for M4 when composition cards with effects ship in player content.
- **Runtime tuning values pending from user:** `maxVibeFromSongHype`, `MaxCardsOnHand`, draw-per-turn, `flowBonusPerStackPerCard`. Required for calibrating VibeGoals of Heckler/Critic encounter archetypes and for validating the Flow-bifurcation formula. Does not block M4.1 (Fix C1) or the doc-writing work; does block the starter v1 authoring tuning pass in M4.6.
- **Keyword-driven runtime behavior (surfaced M1.3b, 2026-04-23):** `ExhaustAfterPlay` bool and `Exhaust` keyword are currently independent. Planned resolution: retire per-keyword bools in favor of `Keywords.Contains(...)` checks, making the keywords list the single source of both tooltip and runtime behavior. Touches the card-play pipeline. Not yet scheduled.

### Residual risks
- **GigManager flag lifecycle surveillance:** `_isSongPlaying` was not observed to drift but a symmetric single-use-per-gig pattern may exist elsewhere. Low-priority audit recommended.
- **Status icon animation pause behavior:** icon animations use `Time.deltaTime`. If a future pause feature sets `Time.timeScale = 0`, icon popups freeze. Switch to `Time.unscaledDeltaTime` if pause-transparent animations become desired.
- **Composition face minimal display:** the shortened face only shows role/part + modifier count. M1.10 detail modal now provides full inspection. Cosmetic items remain: "COMPOSITION" word-break on narrow panels, panel overflow on cards with many modifiers. Neither blocks gameplay testing.
- **M4 roster reduction (2-musician starter) intentionally narrows MVP demo (2026-04-21 design decision):** starter band is C2 + Sibi only. Conito and Ziggy deferred to post-MVP roster expansion. Demo will show a band that is smaller than the final design; this is deliberate and scoped to reduce art and tuning cost. Documented in `planning/Design_Starter_Deck_v1.md`.
- **`LoopScoreCalculator` tuning with 2 musicians (surfaced 2026-04-21):** `LoopScoreCalculator.ComputeLoopScore` currently awards +3 per role present (Rhythm/Bass/Harmony/Melody). With a 2-musician band, Bass and potentially Melody roles are not covered by the band roster, losing up to +6 points of base LoopScore. `ComputeHypeDelta` piecewise thresholds will misfire until retuned for the reduced-role baseline. Sub-task inside M4.2.
- **Flow path split across card domains (2026-04-21 design decision, not yet in runtime):** Action cards keep the current per-card flat `ModifyVibe` bonus; Composition cards gain a multiplier branch; `ComputeSongVibeDeltas` gains a multiplier. The originally-documented Flow → SongHype multiplier path never activated in runtime and is retired officially at M4.2 closure. Until then, code and planned design diverge — this is expected and intentional, not drift.
- **`ApplyIncomingVibe` deferred helper:** the audience-side equivalent of `ApplyIncomingStressWithComposure`. Not implemented in MVP because Earworm (the only audience status in the starter) does not modify incoming Vibe; it generates Vibe on tick. Hook point identified and documented in `planning/Design_Audience_Status_v1.md` for when Captivated lands with Ziggy.

---

## 5. Docs that must be edited next

After the next meaningful technical change, edit:
- the primary affected SSoT
- `CURRENT_STATE.md` if the active operational slice changed
- `changelog-ssot.md` if meaning/authority changed
- `coverage-matrix.md` only if the primary home changed

No pending M1.3 or M1.9 doc edits. All M1.3a, M1.3c, M1.3b SSoT layers applied at their respective closures. M1.9 is presentation-only — no subsystem SSoT changes required.

Pending semantic doc edits from the M4 design pass (held until their respective M4 batches land in code):
- `SSoT_Gig_Combat_Core.md` §5.4, §6.2 — unified Stress path post-M4.1 (both card path and audience action path through `ApplyIncomingStressWithComposure`).
- `SSoT_Gig_Combat_Core.md` §6.1 — Flow combat meaning bifurcation (flat on Action, multiplier on Composition + Song End); explicit retirement of Flow → SongHype path. Post-M4.2.
- `SSoT_Scoring_and_Meters.md` §7.1 — mirror the Flow update. Post-M4.2.
- `SSoT_Status_Effects.md` — new §5.7 `Earworm` with full spec. Post-M4.3.
- `SSoT_Audience_and_Reactions.md` §8, §10 — remove "audience statuses optional for MVP"; add Earworm as the first active audience-side status. Post-M4.3.
- `SSoT_Card_Authoring_Contracts.md` — `starterCopies` is now consumed by runtime (no longer authoring-only metadata). Post-M4.4.
- `SSoT_Card_System.md` — deck multiplicity semantics updated from set-like to multiset. Post-M4.4.
- `ssot_manifest.yaml` — update hard invariants of the affected SSoTs after M4.2 and M4.4.

Planning docs added for M4 this session:
- `planning/Design_Starter_Deck_v1.md` — full starter deck design. Active.
- `planning/Design_Audience_Status_v1.md` — Earworm spec + Captivated deferred design intent + `ApplyIncomingVibe` hook. Active.

---

## 6. Working rule

`CURRENT_STATE.md` answers:
- what is the project foundation
- what is active now
- what comes next
- what is blocked or at risk
- which docs need editing next

It does **not** replace subsystem SSoTs.

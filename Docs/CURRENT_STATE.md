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
- **M1.2 multi-turn validation:** All three deferred tests closed. T5 Choke decay ✅ (Phase 2), T8 Feedback DoT ✅ (Phase 2), T7 Shaken expiry ✅ (Phase 3.1).

### Dev Mode Phase 1 — complete (2026-04-17)
Infinite turns, F12 IMGUI overlay, hand-visibility bridge. `ALWTTT_DEV` scripting define guards all Dev Mode code. See `SSoT_Dev_Mode.md`.

### Dev Mode Phase 2 — complete (2026-04-20)
Card spawner: Catalogue tab in the overlay, `DeckManager.DevSpawnCardToHand`, gated by `CanDevSpawnToHand` (PlayerTurn + MaxCardsOnHand + hand visibility). Decision U1 codified: spawned cards enter the deck on discard/reshuffle (accepted pollution).

### Dev Mode Phase 3.1 — complete (2026-04-23)
Breakdown entry point: Stats tab in overlay, musician selector, `MusicianBase.DevForceBreakdown()` via natural stress path (`DevResetBreakdown` + `AddStress(MaxStress)`). Re-triggerable. T7 Shaken expiry validated — M1.2 multi-turn validation gap fully closed. See `SSoT_Dev_Mode.md` §12.

### Dev Mode Phase 3.2 — complete (2026-04-23)
Gig-wide stat editing: Stats tab gains a Gig-Wide Stats section with SongHype slider, Inspiration slider, Cohesion stepper. Three wrappers on `GigManager` (`DevSetSongHype`, `DevSetInspiration`, `DevSetBandCohesion`) plus `LiveInspiration`/`MaxSongHype` getters. `CompositionSession` gains `DevSetCurrentInspiration` so the Inspiration slider affects the live session budget, not just PD. Dev Mode principle codified: Dev writes reproduce natural consequences — `DevSetBandCohesion(0)` dispatches `LoseGig()` (suppressed under Infinite Turns, same as the natural Breakdown path). **Code-vs-SSoT drift discovered and corrected 2026-04-24 via MB1:** the `LoseGig()` dispatch was never actually in code on 2026-04-23 despite ST-P32-4/-5 being recorded as PASS. MB1 added the dispatch + corrected the stale XML comment; re-validated via ST-MB1-1..4. See `SSoT_Dev_Mode.md` §9.5 + §9.8. Architectural finding surfaced: Inspiration is dual-sited (PD + `CompositionSession._currentInspiration`); see `SSoT_Dev_Mode.md` §13.4. See `SSoT_Dev_Mode.md` §13.

### Dev Mode Phase 3.3a — complete (2026-04-23)
Per-character stat editing + Flow gig-wide extension. Stats tab gains a Per-Character section with musician (Stress slider, MaxStress stepper, Composure stepper) and audience (Vibe slider, MaxVibe stepper) editors. Gig-Wide Stats section gains a Flow stepper (uniform ± applied to every musician's `DamageUpFlat` stacks; aggregate read via `GigManager.TotalFlowStacks`). New DevSet methods: `BandCharacterStats.DevSetCurrentStress/DevSetMaxStress`, `AudienceCharacterStats.DevSetCurrentVibe/DevSetMaxVibe`, `GigManager.DevAddFlowToAllMusicians`. Shared threshold helpers (`CheckBreakdownThreshold`, `CheckConvincedThreshold`) extracted so Dev and play paths cannot drift. Side-resolution: `AudienceCharacterStats.DevResetConvinced` implementation landed (previously doc-declared but unimplemented — resolved a silent `ALWTTT_DEV` compile break in `DevModeController.ResetConvincedAudience`). Latent finding: `HealthBarController.SetCurrentValue(duration=0f)` doesn't propagate the final value to the visual bar; workaround is a `0.1f` duration in Dev setters (see `SSoT_Dev_Mode.md` §14.5). ST-P33a-1..10 all passed. See `SSoT_Dev_Mode.md` §14.

### Dev Mode Phase 3.3b — complete (2026-04-24)
Status apply/remove picker on Per-Character section of Stats tab. Active-status readout with `[−1]`/`[Clear]` per row. Catalogue-backed `[◄][►]` picker with `[+1]` apply. No production-class patches — uses existing `StatusEffectContainer.Apply`/`Clear` API directly. Known limitation: gameplay flags (`IsConvinced`, `IsBreakdown`) not triggered by picker — use dedicated Dev actions for full consequences. Finding: shared catalogue on musician/audience prefabs shows all statuses to both; recommend splitting into separate catalogue SOs (asset-only change, zero code). ST-P33b-1..10 all passed. See `SSoT_Dev_Mode.md` §15.

### MB1 + MB2 — closed (2026-04-24)
Two micro-batches closed jointly. **MB1** corrected the `DevSetBandCohesion` code-vs-SSoT drift: real code never dispatched `LoseGig()` despite §13.2/§13.3 and ST-P32-4/-5 claims. One-line dispatch added + XML comment rewritten. ST-P32-4/-5 retroactively invalidated; re-validated as ST-MB1-1..4. See `SSoT_Dev_Mode.md` §9.8. **MB2** split the shared `StatusEffectCatalogueSO` into `_Musicians` (6 canonical statuses) and `_Audience` (empty at MVP; Earworm populates at M4.3). Musician and audience prefabs reassigned. No code change. ST-MB2-1..6 all passed. `SSoT_Dev_Mode.md` §15.4 marked resolved. See `SSoT_Dev_Mode.md` §9.9. **Open-micro-batches list now empty.**

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
- **Card Editor** (`CardEditorWindow`) — single card authoring, JSON batch import, per-row Starter / Copies columns + toolbar Print button (batch (3), 2026-05-03).
- **Deck Editor** (`DeckEditorWindow`) — deck authoring with JSON import, catalogue browser, save/save-as, GigSetup registration, JSON export, toolbar Print button (batch (3), 2026-05-03). Core functional; polish items remain.
- **Card Inventory** (`CardInventoryWindow`) — read-only inventory browser for `CardDefinition` / `MusicianCardCatalogData` / `GenericCardCatalogSO` assets, with Print to Console + Export JSON per view. New batch (3), 2026-05-03.
- **Status Effect Wizard** (`StatusEffectWizardWindow`) — status SO authoring. HelpBox hint corrected 2026-04-20 to point at wired tick timings only.
- **Chord Progression Catalogue Wizard** (`ChordProgressionCatalogueWizard`).
- See `SSoT_Editor_Authoring_Tools.md`.

### Documentation
Governance migration complete. All subsystem SSoTs active and replacement-ready.

---

### M1.1 — Deck Editor polish — complete (2026-04-26)
Catalogue gains musician + effect-type filters. Staged and catalogue rows show cost badge + plain-text effect summary. Edit button calls `CardEditorWindow.OpenAndSelect`. Validation warns on missing action/composition cards. Save As remembers last-used folder. ST-M11-1..2 passed.

### Milestone 1 — Authoring & Testing Infrastructure — complete (2026-04-26)
All M1 DoD items checked. Full tool pipeline: Card Editor → Deck Editor → Dev Mode → play with animated icons, hover tooltips, right-click detail, stat editing, status apply/remove picker. General-audience testers can drive the game without developer supervision.

### M4.1 — Fix C1: unified Stress path — complete (2026-04-26)
`AddStressAction.DoAction` now routes through `ApplyIncomingStressWithComposure`. Composure absorbs audience pressure, Exposed amplifies it, Breakdown triggers on overflow. Audit finding C1 (2026-03-20) resolved. ST-M41-1..4 passed.

### M4.2 — Flow bifurcation + adaptive LoopScoreCalculator — complete (2026-04-28)
Flow bifurcated by card domain: Action cards use performer's individual Flow stacks as flat Vibe bonus; Composition cards and Song End use band-wide Flow stacks as Vibe multiplier (`flowVibeMultiplier = 0.08f`). Legacy Flow → SongHype path retired and removed from code. `LoopScoreCalculator` rewritten with adaptive scoring: `LoopScoringMode` enum (RoleNormalization / MusicianParticipation), `LoopScoringConfig` + `HypeThresholds` Inspector-tuneable structs, `possibleRoleCount` and `totalMusicians` auto-detected at gig start. Backing tracks now visible to scorer (`HasBacking` added to `LoopFeedbackContext`). Fields renamed with `[FormerlySerializedAs]` for serialization safety. ST-M42-1/1c/3/4/5/9/10/11 passed. ST-M42-2 deferred (no composition card with ModifyVibe in deck). ST-M42-6/7/8 deferred (need 2-musician gig, blocked on musician picker in Gig Setup).

- M4.3 (2026-04-28): Earworm — first active audience-side status. SO `StatusEffect_Earworm_DamageOverTime.asset` in `StatusEffectCatalogue_Audience`. Runtime hook in `GigManager.AudienceTurnRoutine` reads stacks → `AddVibe(stacks)` → container `Tick(AudienceTurnStart)` decays. Skips `IsBlocked`; ticks harmlessly on `IsConvinced`. Validated end-to-end via Dev picker and `TestEarworm.asset` card path.

### M4.6-prep batch (2) — Per-musician starter deck auto-assembly — complete (2026-05-02)
Runtime path that materializes the gig deck from each musician's `MusicianCardCatalogData` (starter-flagged entries, expanded by `starterCopies`) plus an optional `GenericCardCatalogSO` for "Owner: Any" cards. Closes the open item *"Per-musician starter decks"* tracked since M4.2 surfacing (2026-04-28). Closes Roadmap §4.4 deferred line "*`CardAcquisitionFlags.starterCopies` runtime consumption deferred to M4.6 when catalogue → starter-deck auto-assembly is implemented.*" 1 new file (`GenericCardCatalogSO.cs`), 4 modified (`PersistentGameplayData.cs`, `GigRunContext.cs`, `GigSetupConfigData.cs`, `GigSetupController.cs`). Decision matrix: D1 location → new method `PersistentGameplayData.SetBandDeckFromMusicians(IList<MusicianCharacterData>, GenericCardCatalogSO)`; D2a generic cards → new `GenericCardCatalogSO` (separate SO type, reuses `MusicianCardEntry`); D2b zero-copies-with-starter-flag → warn + skip; D3 `availableBandDecks` → demoted to dev fallback via new `useMusicianStartersToggle` (default ON); D4 roster source → use `pd.MusicianList` as-is, picker batch deferred to merged (1)/(4); D5 `MusicianCharacterData.BaseActionCards`/`BaseCompositionCards` → reframed as transitional helpers already deriving from `CardCatalog`, no dual-siting; D6 deck label → new `RunConfig.deckLabel` string. Provenance contract: per-musician contributions populate `musicianGrantedActionCards`/`musicianGrantedCompositionCards`; generic-catalogue contributions do NOT populate provenance, so `RemoveMusicianFromBand` correctly leaves them in the deck when a musician departs mid-run. Subtle case: when the same `CardDefinition` lives in both a per-musician catalog and the generic catalog, removal strips the per-musician copy and leaves the generic copy — correct per the contract (provenance follows contribution path, not card identity). Smoke tests ST-M46p2-1/2/3/5/6/7/8 PASS via console verification + temporary `[ContextMenu]` scaffold on `GigManager` (removed at closure); ST-M46p2-4 DEFERRED-by-construction (`MusicianCatalogService.TryAddEntry` editor-time clamps `starterCopies` to `Mathf.Max(1, …)` and `MusicianCardEntry.starterCopies` carries `[Min(1)]`, making the `starterCopies = 0 + StarterDeck-flagged` state unreachable from tooling; warn-and-skip code path is structurally identical to ST-M46p2-3's `skippedNoCatalog` path which PASSED). Side-finding: Card Editor's per-row UX for flagging starter cards (proposed bulk-action toolbar, then refined to per-row toggle column on the entries list) queued as batch (3). Side-finding: pre-existing `CardBase.SetCard` NRE at `CardBase.cs:77` when opening Draw/Discard/Hand inventory viewers (likely unassigned `inspirationCostTextField` reference on inventory card prefab) — surfaced during smoke tests, not caused by batch (2), queued as separate UI-fix batch.

### M4.6-prep UI-fix-A — Inventory viewer prefab NRE — complete (2026-05-02)
Closes the inventory-viewer NRE surfaced during M4.6-prep batch (2) smoke tests. Inventory canvas instantiates `CardUI.prefab` (an empty subclass `CardUI : CardBase {}` assigned to `InventoryCanvas.cardUIPrefab`); two `[SerializeField]` TMP refs on the prefab's `Card UI (Script)` component were unassigned: `inspirationCostTextField` and `inspirationGenTextField`. `CardBase.SetCard` (line 77 of the cited stack) writes to those fields unconditionally, producing the NRE on Draw/Discard/Hand pile open. Asset-only fix: wired both refs to the corresponding TMP_Text children on `CardUI.prefab`. `CardBase.SetCard` kept strict (no defensive null guards added — strict failure surfaces future authoring drift loudly). Smoke tests ST-INV-1..6 PASS (ST-INV-5 PASSED with both Action and Composition cards in mixed-pile view; ST-INV-6 confirmed gameplay card prefab unchanged, ruling out wrong-prefab edit). Structural finding parked: `CardUI : CardBase {}` empty subclass formalizes a two-prefab arrangement (gameplay card prefab + `CardUI.prefab`), which is the recurrence vector for unwired-`SerializeField` bugs on the inventory side. See §4 Open items for the parking note. No code shipped, no SSoT change.

### M4.6-prep UI-fix-B — Inventory scrollbar functional — complete (2026-05-02)
Closes the inventory ScrollRect snap-back / no-scrollbar symptom surfaced immediately after UI-fix-A. Root cause was layered: `Content` had `ContentSizeFitter` (Vertical=Preferred Size) but no `LayoutGroup` to feed it preferred height; and `Viewport` had `Mask` + a disabled `Image` (broken masking, would have manifested as card bleed once scrolling worked). Fix is asset-only on `InventoryCanvas.prefab` plus a small code edit on `InventoryCanvas.cs`. Asset edits: added `VerticalLayoutGroup` to `Content` (Padding 0 / Spacing 0 / Child Alignment Upper Center / Control Child Size W=ON H=OFF / Force Expand W=ON H=OFF); added `LayoutElement` to `FilterPanel` (Min Height=100, Preferred Height=100), to `CardSpawnRoot` (Preferred Height=2050), to `SongSpawnRoot` (Preferred Height=800); replaced `Mask` + disabled `Image` on `Viewport` with `RectMask2D`; reduced `CardSpawnRoot` Grid Layout Group Padding Top 150→50 (cosmetic). The `LayoutElement` strategy was required because `GridLayoutGroup` on a stretch-anchored `RectTransform` inside a `ContentSizeFitter` does not reliably report preferred height to its parent — explicit `LayoutElement.preferredHeight` bypasses this. Code edits in `InventoryCanvas.cs`: added `using UnityEngine.UI;`, added `[SerializeField] private ScrollRect scrollRect;` field (wired to the `Scroll View` GameObject in the prefab), and at the end of `SetCards` and `SetSongs` (after population) added a null-guarded reset block: `Canvas.ForceUpdateCanvases(); LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content); scrollRect.verticalNormalizedPosition = 1f;` — the `ForceUpdateCanvases` + `ForceRebuildLayoutImmediate` pair guards against the timing race where `verticalNormalizedPosition` samples stale Content bounds before the layout pass runs. Smoke tests ST-SCR-1/3/4/6/7 PASS, ST-SCR-2 FAIL ACCEPTED as paper cut (vacuous overflow: with `CardSpawnRoot.LayoutElement.preferredHeight = 2050` fixed, near-empty piles still produce overflow → scrollbar appears unnecessarily; cosmetic, follow-up via dynamic-height computation), ST-SCR-5 DEFERRED-by-construction (no Songs inventory content reachable in current build). See §4 Open items for the paper-cut note and FilterPanel-scrolls-with-content deferral.

### M4.6-prep batch (3) — Authoring tooling QoL — complete (2026-05-03)
Editor-only batch promoting authoring ergonomics surfaced during M4.6-prep batch (2) smoke tests. Closes the open item *"Card Editor per-row starter UX (queued as batch (3), surfaced 2026-05-02)"*. Three deliverables shipped in three files (one new, two modified), all `#if UNITY_EDITOR` guarded, zero runtime impact.

**(3.A) Per-row Starter / Copies columns on `CardEditorWindow`.** The catalog entry list's row rendering loop (formerly a single `GUILayout.Toggle(isSelected, label, "Button")` per entry) now renders each row as a horizontal scope containing: a `Starter` checkbox (~38 px), a `Copies` IntField (~40 px, greyed when Starter is off), and the existing selection button with a recomposed label (`[S]` flag dropped from the label since the checkbox column is the canonical indicator; `[R]` and `[L]` retained). Both inline controls write through `SerializedObject(_loadedCatalog)` → `entries.GetArrayElementAtIndex(i)` → `FindPropertyRelative("flags" / "starterCopies")` with a single `ApplyModifiedProperties()` per frame, giving Undo registration and asset-dirty propagation identical to the right-side inspector path. Clamp on commit: `if (newCopies < 1) newCopies = 1;` (mirrors the `[Min(1)]` attribute on `MusicianCardEntry.starterCopies` and `MusicianCatalogService.TryAddEntry`'s `Mathf.Max(1, …)`). IMGUI controls consume their own input events, so clicking the inline checkbox/IntField on a non-selected row does not change `_selectedEntryIndex` (the row's name button remains the only selection target).

**(3.B) `CardInventoryWindow` (new file).** New editor window registered at `ALWTTT/Cards/Card Inventory` (priority 12, immediately after Card Editor and Deck Editor). Four toolbar-selected views: All `CardDefinition` assets in project; All `MusicianCardCatalogData` with per-asset summary (entry count + starter count + total starter copies); One specific musician catalogue (full entry list, musician selected via toolbar dropdown); All `GenericCardCatalogSO` assets (each rendered with its full entry list since `GenericCardCatalogSO.Entries` reuses `MusicianCardEntry`). Each view supports `Print` (multi-line `Debug.Log`) and `Export JSON` (`EditorUtility.SaveFilePanel` → `JsonUtility.ToJson(_, prettyPrint: true)` → file written + `EditorUtility.RevealInFinder`). The export schema is human-readable / informational, not designed to round-trip through `DeckJsonImportService`.

**(3.C) Toolbar Print buttons on `CardEditorWindow` and `DeckEditorWindow`.** Both windows gain a `Print` button on their existing toolbars. Card Editor: appended to the toolbar's actions cluster after the Registries Ping button (`GUILayout.Space(10)` separator); disabled when no catalog is loaded; produces a `=== CARD EDITOR — CATALOG DUMP ===` block with musician, asset path, entry count, starter count + total starter copies summary, and one line per entry (id, kind, flags, copies, unlockId). Deck Editor: inserted between `Export JSON` and `Clear All`; produces a `=== DECK EDITOR — STAGED DECK DUMP ===` block with asset path, deckId, displayName, description, entry count + total copies summary, and one line per entry using `StagedCardEntry.ResolvedCard` to handle existing and pending cards uniformly (`[NEW]` suffix for pending entries; `×{count}` for M4.4 multiplicity).

Decision matrix at open: D1 menu path → `ALWTTT/Cards/Card Inventory` (priority 12) accepted; D2 export schema → human-readable informational accepted; D3 "Validate `CardBase` prefab variants" appendix → punted (logged as candidate authoring-tool addition in `SSoT_Editor_Authoring_Tools.md §14.5`); D4 per-row layout density → fixed pixel widths accepted (Starter 38 px, Copies 40 px); D5 silent disappearance on filter interaction accepted; D6 Card Editor Print button placement → toolbar (not entries-list header) accepted.

Smoke tests ST-AT3-1..8 all PASS:
- ST-AT3-1 (per-row Starter toggle commits to asset, persists across reload) PASS;
- ST-AT3-2 (Copies field disable + clamp to 1 on commit) PASS;
- ST-AT3-3 (filter interaction silent disappearance) PASS;
- ST-AT3-4 (Undo reverts both flag and copies as one step) PASS;
- ST-AT3-5 (CardInventoryWindow all four views populate, Print + Export succeed for each — verified via `inv1.json` / `inv2.json` / `inv3.json` / `inv4.json` exports) PASS;
- ST-AT3-6 (Print buttons on both windows produce formatted multi-line output — Card Editor verified on Conito catalog dump, Deck Editor formatter uses `ResolvedCard` and `count`) PASS;
- ST-AT3-7 (regression: per-row controls do not steal selection) PASS;
- ST-AT3-8 (dogfood acceptance: Cantante cleanup workflow materially faster than right-side inspector) PASS, "very good cleanup process" reported.

**Critical scope honesty.** Batch (3) ships the *tooling* needed to execute the M4.6 starter-deck cleanup. The *content cleanup itself* — pruning the four musician catalogues from their current 28-entries-all-Starter-flagged state to the 12-card / 7-unique / 2-musician Cantante+Sibi composition specified in `Design_Starter_Deck_v1.md §4` — is a **separate follow-up**. ST-AT3-8 demonstrated the workflow on at least one musician but the test does not assert that all four catalogues now match the design spec. The pre-demo blocker tracked as the "all-starter-flagged catalog content" item in §4 is now **structurally tractable** but **content-status undetermined**; a fresh `CardInventoryWindow > All Musician Catalogs > Export JSON` snapshot compared against `Design_Starter_Deck_v1.md §4` is the recommended next verification step. Side-finding: the inventory exports captured during ST-AT3-5 (pre-cleanup, snapshotted in this session's outputs) provide a clean before-state baseline for that comparison.

### Latent multi-song action window bug — fixed (2026-04-20)
`GigManager._actionWindowOpen` and `_isBetweenSongs` now re-asserted at every `ExecuteGigPhase(PlayerTurn)` entry. Affected any multi-song gig (production and Dev Mode). See `SSoT_Runtime_Flow.md` §4.1 for the flag lifecycle table.

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

**M4.3 — Earworm (2026-04-28).** First audience-side status implemented. Side fixes shipped: `ALWTTTProjectRegistriesSO` extended to expose both musicians and audience catalogues (`[FormerlySerializedAs]` preserved existing serialized reference); Card Editor JSON importer (`ApplyEffectsJson`) rewritten to probe both catalogues via `registries.TryGetStatusEffectByKey`; toolbar warning expanded to call out the specific missing field. `CardBase.ExecuteEffects` apply-time log expanded with `StatusKey` + `DisplayName` alongside primitive id (disambiguates shared-primitive variants). New `[Earworm]` tick log in `GigManager.AudienceTurnRoutine`. Initial patch shipped with a copy-paste duplicate `Tick(AudienceTurnStart)` block producing -2/turn decay; caught by ST-M43-2/3 stack-count observation; fixed by deletion before closure.

### Dev Mode Phase 3 — stat & state editing (in progress)
P3.1 ✅ + P3.2 ✅ + P3.3a ✅ + P3.3b ✅ (all closed). Phase 3 complete. P3.4 audience transparency panel deferred. Encounter modifier toggles deferred.

### Deck Editor — polish pass ✅ (closed 2026-04-26)
Catalogue filters (musician, effect type), card preview info, cross-tool Edit button, last-used save folder, enhanced validation. See §1.

### Contextual stats on hover — feature disabled (2026-04-20)
`BandCharacterCanvas` hover-to-show-stats path present in code but disabled at prefab level (`statsCanvasGroup` / `statsRoot` unassigned, `StatsRoot` GameObject off). Silent no-op. Revisit when visual density is tuned.

### Editor tooling documentation — complete
`SSoT_Editor_Authoring_Tools.md` active and registered. Updated 2026-05-03 with batch (3) additions: §3 inventory row, §4.6/§4.7 Card Editor batch (3) sections, §5.7 Deck Editor Print button, new §8 `CardInventoryWindow`, §13 file list, §14.5 prefab-variant validator candidate.

---

## 3. What is next

1. **M4.6** — Starter deck v1 authoring. Author and register the 12-card / 7-unique / 2-musician starter deck per `planning/Design_Starter_Deck_v1.md`. Depends on remaining §9 open questions + merged Gig Setup pickers (musicians + audience). (`DeckCardCreationService` parallel registries fix closed 2026-05-01 as M4.6-prep-A. Per-musician starter deck auto-assembly closed 2026-05-02 as M4.6-prep batch (2). Authoring tooling QoL closed 2026-05-03 as M4.6-prep batch (3) — per-row Starter UI + Card Inventory + Print buttons now in place.) Demo gate. **Pre-demo blocker (structurally tractable, content-status undetermined):** test catalogs currently have all entries flagged `StarterDeck` (28 total entries: Cantante 7, Conito 10, Gusano 6, Robot 5 — captured pre-batch-(3) in `inv2.json` snapshot). Per-row Starter UI on `CardEditorWindow` (batch (3.A)) makes the cleanup materially faster; `CardInventoryWindow > All Musician Catalogs > Export JSON` provides a verification snapshot path. The cleanup itself needs to be executed and re-verified against `Design_Starter_Deck_v1.md §4` before demo.

2. **M2 Composition session validation** — unblocked by M1.5 Phases 1–2.

---

## 4. Open items and risks

### Open items (non-blocking)
- **Shaken restrictions:** status applies and expires correctly; no gameplay gate yet. Design decision still open.
- **Audience Feedback DoT:** no Stress path on `AudienceCharacterBase`. Deferred.
- **Composure penalty during Shaken:** design intent only; not code-enforced.
- **True card copies in decks:** RESOLVED by M4.4 (closed 2026-04-29). `BandDeckData` is now a multiset; `PersistentGameplayData.SetBandDeck` expands counts into independent references; pile lifecycle preserves identity per reference. See `SSoT_Card_System.md §13` and `SSoT_Card_Authoring_Contracts.md §5.10`.
- **M4.5 architectural decision (filtered-draw mechanism):** RESOLVED 2026-04-30. Option 1 (predicate-based filtered draw on `DeckManager`) + subtractive budget rule. Two-hook framing collapses to single PlayerTurn-entry site because action and composition windows open simultaneously. Composition wins tie-break. See `SSoT_Runtime_Flow.md §4.2` and §1 M4.5 closure block.
- **M1.2 multi-turn validation gap** — fully closed (T5/T8 Phase 2, T7 Phase 3.1). No remaining deferred tests.
- **Choke-on-stunned design decision:** T5 surfaced that `HandController.TryResolveCardTarget` refuses stunned musicians. MVP decision: keep the refusal.
- **`CardActionTiming` default excludes PlayerTurn:** documented in `SSoT_Dev_Mode.md` §8.4 and `SSoT_Card_Authoring_Contracts.md` §3.4.
- **C1 — resolved (2026-04-26).** `AddStressAction` now routes through `ApplyIncomingStressWithComposure`. See §1 M4.1 closure block.
- **`CompositionCardPayload.effects` support — verified (2026-04-23):** ST-M13c-6 confirmed that `CardPayload.Effects` on composition cards works end-to-end (status tooltip appears on hover, effect authored via Card Editor). `Four on the Floor`'s `ApplyStatusEffect(flow)` co-effect is viable.
- **Raw Inspector `[SerializeReference]` drawer for `CardEffectSpec`:** Unity's default property drawer doesn't show a type menu for new list elements. Card Editor window is the intended authoring path. Defer custom drawer to M1.1.
- **Composition card face does not surface `CardPayload.Effects`:** by design (2026-04-21 simplification). Tooltip covers discoverability. Design question for M4 when composition cards with effects ship in player content.
- **Runtime tuning values pending from user:** `maxVibeFromSongHype`, `MaxCardsOnHand`, draw-per-turn. Required for calibrating VibeGoals of Heckler/Critic encounter archetypes. Flow tuning values now landed (`flowActionVibeBonusPerStack = 1`, `flowVibeMultiplier = 0.08f`, Inspector-tuneable). Does not block M4.3; does block the starter v1 authoring tuning pass in M4.6.
- **Keyword-driven runtime behavior (surfaced M1.3b, 2026-04-23):** `ExhaustAfterPlay` bool and `Exhaust` keyword are currently independent. Planned resolution: retire per-keyword bools in favor of `Keywords.Contains(...)` checks, making the keywords list the single source of both tooltip and runtime behavior. Touches the card-play pipeline. Not yet scheduled.
- **Inspiration dual-siting (surfaced M1.5 P3.2, 2026-04-23):** `pd.CurrentInspiration` and `CompositionSession._currentInspiration` are not continuously synchronized. The session's field is the one the composition cost gate and UI actually read. Dev Mode's `DevSetInspiration` writes to both. Finding only — no gameplay contract change. Potential follow-up: one-line note in `SSoT_Gig_Combat_Core.md` §4.2 to surface this implementation reality. See `SSoT_Dev_Mode.md` §13.4.
- **Musician picker in Gig Setup (surfaced M4.2, 2026-04-28):** no way to select which musicians are in the band from the Gig Setup screen. Roster is currently hardcoded via `PersistentGameplayData`. Blocks 2-musician testing (ST-M42-6/7/8/9 deferred). Likely M4.6 or standalone QoL.
- **Per-musician starter decks — RESOLVED (2026-05-02, M4.6-prep batch (2)).** `PersistentGameplayData.SetBandDeckFromMusicians` materializes the deck from each musician's `CardCatalog` (starter-flagged entries × `starterCopies`) plus an optional `GenericCardCatalogSO` from `GigSetupConfig.GenericStarterCatalog`. Toggle in Gig Setup scene (`useMusicianStartersToggle`, default ON) selects between auto-assembly and the legacy `BandDeckData` dropdown path. Provenance: per-musician contributions tracked, generic contributions not tracked. See §1 batch (2) closure block.
- **Gig Setup roster pickers (deferred from M4.3 surfacing).** Bidirectional band + audience multi-select picker in Gig Setup Scene — currently single-character selection paths only. Recommended: standalone batch ahead of M4.6, or rolled into M4.6 starter authoring. Touches `GigSetupController.cs`, `GigSetupConfigData.cs`, audience selection plumbing, new selection UI. **Note (2026-05-02):** auto-assembly batch (2) consumes `pd.MusicianList` as-is; this picker batch will mutate `pd.MusicianList` before `OnStartPressed` and auto-assembly will pick up the change for free. No coupling beyond that.
- **Card Editor inline effects-block UI on legacy catalogue alias (surfaced 2026-05-01, M4.6-prep-A audit).** `CardEditorWindow.cs:1267, 1305` call `DrawEffectsBlock(effectsProp, _registries?.StatusCatalogue)`, which only exposes the musicians catalogue. Same MB2 friction shape as the JSON-import path — the inline effect-row dropdown for picking a status when editing a card directly cannot see audience-side statuses (e.g. `earworm`). Affects authoring UX, not import resolution. No timeline; track until it bites. Out of scope for M4.6-prep-A.
- **All-starter-flagged catalog content currently in test data (M4.6 demo blocker, surfaced 2026-05-02; tooling resolved 2026-05-03 via batch (3); content cleanup pending).** During M4.6-prep batch (2) smoke tests, every entry in every musician's `MusicianCardCatalogData` was flagged `StarterDeck` to produce a non-empty auto-assembled deck. The pre-batch-(3) snapshot (`inv2.json` exported during ST-AT3-5, 2026-05-03) confirms the state: Cantante 7/7 starter, Conito 10/10 starter, Gusano 6/6 starter, Robot 5/5 starter — 28 entries total, all StarterDeck-flagged. M4.6 demo specifically requires the 12-card / 7-unique / 2-musician (Cantante + Sibi) composition per `Design_Starter_Deck_v1.md §4`. **Tool is now in place** to execute the cleanup efficiently (per-row Starter UI from batch (3.A), CardInventoryWindow from batch (3.B), Export JSON for verification snapshots). **Content cleanup itself is the remaining task before demo:** un-flag non-starter entries on each musician's catalog so only the designed cards remain starter-flagged. Verification path: re-export `CardInventoryWindow > All Musician Catalogs` and `> One Musician` for Cantante and Sibi → diff against `Design_Starter_Deck_v1.md §4`. Tracked as M4.6 pre-demo task.
- **`UnlockedByDefault` flag is editor-authoring-only (surfaced 2026-05-02, M4.6-prep batch (2) audit).** `CardAcquisitionFlags.UnlockedByDefault` has no runtime gameplay consumption today. Every reference is in editor code (Card Editor filter pills, validation warnings, JSON import validation, default value for new entries). Auto-assembly only consults `IsStarter`. The `UnlockedByDefault` + `unlockId` pair currently documents authorial intent for a future meta-progression / unlock system; no gameplay code reads them. Not a bug — flagged so future readers don't assume runtime enforcement that doesn't exist. Runtime consumption deferred to whenever a meta-progression batch lands.
- **Inventory viewer NRE on Draw/Discard/Hand pile open — RESOLVED (2026-05-02, M4.6-prep UI-fix-A).** `CardBase.SetCard` at `CardBase.cs:77` no longer throws because `CardUI.prefab`'s previously-unassigned `inspirationCostTextField` and `inspirationGenTextField` `[SerializeField]` refs are now wired. Asset-only fix on `CardUI.prefab`. `CardBase.SetCard` kept strict. See §1 UI-fix-A closure block.
- **`CardUI : CardBase {}` empty subclass — two-prefab arrangement (surfaced 2026-05-02, M4.6-prep UI-fix-A; appendix to batch (3) deferred 2026-05-03).** `CardUI` is a degenerate empty subclass of `CardBase` that exists solely to serve as a separate prefab GameObject's MonoBehaviour. The inventory canvas instantiates `CardUI.prefab` while gameplay instantiates the gameplay card prefab; both prefabs must independently wire every `[SerializeField]` field declared on `CardBase`. This is the recurrence vector for the UI-fix-A NRE class — any future TMP/Image field added to `CardBase` must be wired on both prefabs or the inventory side will NRE. Candidate cleanups (logged, not scheduled): (α) collapse to a single prefab with view-only mode driven by `SetCard(def, isPlayable=false)` — lowest drift risk; (β) make `CardUI.prefab` a Prefab Variant of the gameplay prefab so `CardBase` field additions inherit automatically — lower-risk migration than (α). Candidate appendix to batch (3) — "Validate `CardBase` prefab variants" Card Editor action that reflects over `[SerializeField]` fields and reports unwired refs at authoring time — was considered at batch (3) open and **explicitly deferred** (D3); logged in `SSoT_Editor_Authoring_Tools.md §14.5` as a candidate authoring-tool addition for a future QoL pass.
- **Inventory scrollbar appears even with near-empty piles — paper cut (surfaced 2026-05-02, M4.6-prep UI-fix-B; ST-SCR-2 FAIL ACCEPTED).** `CardSpawnRoot` carries a fixed `LayoutElement.preferredHeight = 2050` so `Content` always reports overflow to `ScrollRect`, regardless of how many cards are actually displayed. Cosmetic only — does not affect functionality. Follow-up: replace the fixed value with a runtime computation in `InventoryCanvas.SetCards` based on active card count × grid params (`grid.cellSize.y`, `grid.spacing.y`, `grid.padding.top + grid.padding.bottom`, columns from `grid.constraintCount`). ~10 lines, computes `LayoutElement.preferredHeight` after population. Not blocking M4.6 demo.
- **FilterPanel scrolls with content (decision D-A deferred from M4.6-prep UI-fix-B, 2026-05-02).** `FilterPanel` lives inside `Content` under `VerticalLayoutGroup`, so it scrolls along with `CardSpawnRoot`/`SongSpawnRoot`. FilterPanel currently only contains TitleText (no functional filter chips), so scroll-with-content is harmless. Revisit when filters become functional: move FilterPanel out of `Content` and make it a sibling of `Scroll View` under `Midground` for sticky behavior.
- **Card Editor per-row starter UX — RESOLVED (2026-05-03, M4.6-prep batch (3)).** Batch (3.A) ships per-row `Starter` checkbox + `Copies` IntField columns on the catalog entry list, both via `SerializedObject` for Undo + dirty propagation parity with the right-side inspector. Batch (3.B) ships `CardInventoryWindow` (read-only viewer with Print + Export per view). Batch (3.C) ships toolbar Print buttons on Card Editor and Deck Editor. Smoke tests ST-AT3-1..8 all PASS. ST-AT3-8 dogfood acceptance confirmed the cleanup workflow is materially faster than the right-side inspector path. See §1 batch (3) closure block.
- **Pending Effects system (post-MVP, scheduled first).** Song-scoped accumulator layer where cards add to a pending bucket during a song and resolve at song end. First user: deferred Earworm. Mid-song multiplier cards become a content axis. Generalizes to pending Vibe / Stress / Flow / Cohesion. Does not affect M4.6 starter deck — Mind Tap and any other Earworm-applying starter card stay immediate-effect. Planning doc: `planning/Design_Pending_Effects_v1.md`. Implementation slot: first post-MVP gameplay batch immediately following M4.6 demo closure.

- **Tempo-coupled card identity (post-MVP, long-term, no implementation slot).** Design direction making tempo a gameplay input — cards prefer / require / shift tempo, producing fast-favoring vs slow-favoring deck identities ("metal" / "fast jazz" / etc.). Downstream of M4.6 closure, Pending Effects landing, and meter-stack playtest. No runtime commitment. Influences starter deck and per-musician catalog design now via flavor / naming / archetype lean — see `Design_Starter_Deck_v1.md` for tempo-lean notes per musician. Planning doc: `planning/Design_Tempo_Identity_v1.md`.

### Residual risks
- **GigManager flag lifecycle surveillance:** `_isSongPlaying` was not observed to drift but a symmetric single-use-per-gig pattern may exist elsewhere. Low-priority audit recommended.
- **Status icon animation pause behavior:** icon animations use `Time.deltaTime`. If a future pause feature sets `Time.timeScale = 0`, icon popups freeze. Switch to `Time.unscaledDeltaTime` if pause-transparent animations become desired.
- **Composition face minimal display:** the shortened face only shows role/part + modifier count. M1.10 detail modal now provides full inspection. Cosmetic items remain: "COMPOSITION" word-break on narrow panels, panel overflow on cards with many modifiers. Neither blocks gameplay testing.
- **M4 roster reduction (2-musician starter) intentionally narrows MVP demo (2026-04-21 design decision):** starter band is C2 + Sibi only. Conito and Ziggy deferred to post-MVP roster expansion. Demo will show a band that is smaller than the final design; this is deliberate and scoped to reduce art and tuning cost. Documented in `planning/Design_Starter_Deck_v1.md`.

- **`ApplyIncomingVibe` deferred helper:** the audience-side equivalent of `ApplyIncomingStressWithComposure`. Not implemented in MVP because Earworm (the only audience status in the starter) does not modify incoming Vibe; it generates Vibe on tick. Hook point identified and documented in `planning/Design_Audience_Status_v1.md` for when Captivated lands with Ziggy.

---

## 5. Docs that must be edited next

After the next meaningful technical change, edit:
- the primary affected SSoT
- `CURRENT_STATE.md` if the active operational slice changed
- `changelog-ssot.md` if meaning/authority changed
- `coverage-matrix.md` only if the primary home changed

No pending M1.5 doc edits. All P3 phases closed. Open-micro-batches list empty after MB1+MB2 closure. M1.9 is presentation-only — no subsystem SSoT changes required. M1.5 Phase 3.3b doc edits applied at closure (`SSoT_Dev_Mode.md` §3/§6/§9.7/§15, `CURRENT_STATE.md`, `Roadmap_ALWTTT.md`, `changelog-ssot.md`). MB1+MB2 doc edits applied at joint closure (`SSoT_Dev_Mode.md` §9.5 correction + §9.8 + §9.9 + §15.4 resolution, `CURRENT_STATE.md` §1 P3.2 amendment + new closure block + §3 next-up, `Roadmap_ALWTTT.md` §1.5 open-micro-batches cleared + header date bumped, `changelog-ssot.md` 2026-04-24 joint-closure entry with ST-P32-4/-5 honesty correction).

Pending semantic doc edits from the M4 design pass (held until their respective M4 batches land in code):
- `SSoT_Gig_Combat_Core.md` §5.4, §6.2 — unified Stress path post-M4.1 (both card path and audience action path through `ApplyIncomingStressWithComposure`).
- `SSoT_Status_Effects.md` — new §5.7 `Earworm` with full spec. Post-M4.3.
- `SSoT_Audience_and_Reactions.md` §8, §10 — remove "audience statuses optional for MVP"; add Earworm as the first active audience-side status. Post-M4.3.
- `SSoT_Card_Authoring_Contracts.md` §5.7 + new §5.10 + §7.1 — applied 2026-04-29 (M4.4 closure). `starterCopies` clarified as authoring-only at M4.4 with M4.6 runtime-consumption note; new §5.10 covers deck-level multiplicity contract; §7.1 stage invariants note the per-entry `count` on `StagedCardEntry`.
- `SSoT_Card_System.md` new §13 — applied 2026-04-29 (M4.4 closure). Deck multiplicity model documented (multiset shape, runtime expansion, pile-lifecycle invariance, lazy legacy migration). §12 boundaries list updated. M4.5 cross-reference paragraph appended 2026-04-30.
- `SSoT_Runtime_Flow.md` §4.2 + §8 invariant 9 — applied 2026-04-30 (M4.5 closure). New §4.2 "Bidirectional guaranteed draws" documents subtractive rule, three-phase algorithm, hook collapse, tie-break, observability, exhaustion case. New invariant 9 in §8.
- `ssot_manifest.yaml` — applied 2026-04-29 (M4.4 closure). New invariants on `SSoT_Card_System.md` (deck is multiset; runtime expands to flat references) and `SSoT_Card_Authoring_Contracts.md` (JSON deck entries support `count`; duplicate `cardId` combines additively). Applied 2026-04-30 (M4.5 closure). New invariant on `SSoT_Runtime_Flow.md` (subtractive guaranteed-draw rule). M4.2 invariants update remains pending.
- `SSoT_Card_Authoring_Contracts.md` §5.9 — applied 2026-05-01 (M4.6-prep-A closure). Stale "parallel `DeckCardCreationService` path still consults a single catalogue field" footnote removed; the section now describes a single, unified MB2-aware editor toolchain. `CURRENT_STATE.md` §1 + §3 + §4 + §5 + `changelog-ssot.md` updated; `ssot_manifest.yaml`, `coverage-matrix.md`, `Roadmap_ALWTTT.md`, `SSoT_Editor_Authoring_Tools.md` intentionally unchanged.
- M4.6-prep batch (2) closure (applied 2026-05-02): `CURRENT_STATE.md` §1 closure block + §3 M4.6 dependency line update + §4 open-item closures and additions (Draw Pile NRE, batch (3) queue, all-starter-flagged catalog blocker, `UnlockedByDefault` editor-only note) + §5 (this line); `Roadmap_ALWTTT.md` §4.4 line 371 + §4.6 line 412 marked shipped, two new Future Milestones added (Authoring tooling QoL = batch (3); Inventory viewer prefab fix); `SSoT_Card_Authoring_Contracts.md` new §5.11 (per-musician starter deck auto-assembly contract); `ssot_manifest.yaml` Card_Authoring_Contracts entry gains one invariant on auto-assembly; `changelog-ssot.md` new top entry. `coverage-matrix.md`, `SSoT_Editor_Authoring_Tools.md`, `SSoT_INDEX.md`, `SSoT_Card_System.md` intentionally unchanged (no new editor tool, no new subsystem, no authority change, no runtime pile-lifecycle change).
- M4.6-prep UI-fix-A + UI-fix-B joint closure (applied 2026-05-02): `CURRENT_STATE.md` §1 two new closure blocks (UI-fix-A inventory NRE; UI-fix-B inventory scrollbar) + §4 open-items: inventory NRE bullet flipped to RESOLVED with closure pointer, three new park-lot bullets added (`CardUI : CardBase` empty-subclass two-prefab vector with cleanup options α/β logged; inventory-scrollbar paper cut with dynamic-height follow-up; FilterPanel-scrolls-with-content D-A deferral); `Roadmap_ALWTTT.md` Future Milestones: `Inventory viewer prefab fix (UI-fix batch)` entry retitled to combined `Inventory viewer fixes (UI-fix-A + UI-fix-B)` and marked shipped 2026-05-02; `changelog-ssot.md` new combined top entry covering both batches with ST-INV-1..6 PASS + ST-SCR-1/3/4/6/7 PASS / ST-SCR-2 FAIL ACCEPTED / ST-SCR-5 DEFERRED. `ssot_manifest.yaml`, `coverage-matrix.md`, `SSoT_INDEX.md`, all systems SSoTs intentionally unchanged (no contract, authority, or governance change — UI-asset wiring + a localized ScrollRect helper edit on `InventoryCanvas.cs`).
- M4.6-prep batch (3) closure (applied 2026-05-03): `SSoT_Editor_Authoring_Tools.md` §3 inventory row added (Card Inventory), §4.6 (per-row Starter / Copies columns) + §4.7 (Card Editor Print button) + §5.7 (Deck Editor Print button) added, new §8 `CardInventoryWindow` full section inserted, §9–§15 renumbered, §13 file location summary updated, §14.5 prefab-variant validator candidate logged. `CURRENT_STATE.md` §1 new closure block (M4.6-prep batch (3) — Authoring tooling QoL — complete) inserted after the UI-fix-B block; §1 Editor authoring tools list updated; §3 line 1 M4.6 entry updated to note batch (3) closure and the structurally-tractable / content-status-undetermined nature of the all-starter-flagged blocker; §4 open-items: "Card Editor per-row starter UX" bullet flipped from queued → RESOLVED with closure pointer, "all-starter-flagged catalog content" bullet rewritten to distinguish *tooling resolved* from *content cleanup pending*; "`CardUI : CardBase {}` empty subclass" bullet updated to record the D3 deferral of the prefab-variant validator appendix; §5 (this line). `Roadmap_ALWTTT.md` Future Milestones: `Authoring tooling QoL (batch (3))` entry marked ✅ (closed 2026-05-03) with closure notes and smoke-test summary; header `Last updated` line bumped to 2026-05-03. `changelog-ssot.md` new top entry. `ssot_manifest.yaml`, `coverage-matrix.md`, `SSoT_INDEX.md`, all systems SSoTs intentionally unchanged (no new authority, no new contract, no new subsystem — operational tooling only).

Pending low-priority doc edits surfaced by M1.5 P3.2:
- `SSoT_Gig_Combat_Core.md` §4.2 — one-line note on Inspiration dual-siting (PD vs session's live budget). Optional; not scheduled.

Planning docs added for M4 this session:
- `planning/Design_Starter_Deck_v1.md` — full starter deck design. Active. Amended 2026-04-24 with "Design principle: mínimas cartas, máxima expresividad" section (primary home for the principle). Substantially revised 2026-04-26 with axis-resolution session: per-card axis assignments locked for all 7 composition cards (C2 four meter cards on axis 7, Sibi two backing cards on axis 13, Sibi one melody card on axis 23); v0 cards Steady Beat / Four on the Floor / Synth Pad / Hook Theme retired in favor of Default Mode / Waltz Protocol / Pentameter / Compound Cycle / Wormus Minor / Wormus Major / Singing Field; aggregate counts preserved (12 cards / 8 composition + 4 action / 5 C2 + 3 Sibi); §9 #1 (CompositionCardPayload.effects) closed retroactively per ST-M13c-6; §9 #5 closed; §9 #7 / #8 / #9 added.
- `planning/Design_Audience_Status_v1.md` — Earworm spec + Captivated deferred design intent + `ApplyIncomingVibe` hook. Active.

Integration reference docs added 2026-04-24:
- `planning/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` — single-source reference mapping the observable musical expressive surface available to ALWTTT composition cards against MidiGenPlay package contracts. 26-axis matrix, observed precedences, per-role bundle contracts, 5 documented gaps (all with decisions deferred). Operationalizes the design principle captured in Design_Starter_Deck_v1.md. Planning/reference — not governed SSoT.

---

## 6. Working rule

`CURRENT_STATE.md` answers:
- what is the project foundation
- what is active now
- what comes next
- what is blocked or at risk
- which docs need editing next

It does **not** replace subsystem SSoTs.

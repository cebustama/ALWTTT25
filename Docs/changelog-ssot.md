# changelog-ssot — ALWTTT

This changelog records **semantic/documentary changes**.
Cosmetic edits should not be logged here.

## 2026-04-23f — M1.9 closure: card sizing refactor

### Operational changes

**HandController card sizing (new, applies from M1.9 closure):**
- Serialized `cardBaseScale` (1.0), `cardHoverScaleMultiplier` (1.25, relative to base), `scaleLerpSpeed` (12). Per-frame `localScale` lerp toward `base × multiplier` on hover/drag, `base` otherwise.
- `HandScaleFactor` (= `cardBaseScale`) multiplied into curve endpoints and hand bounds at init. Cards at rest don't overlap when base scale changes.
- Pop-up offset, fanning factor, hover-detection threshold scale proportionally with `cardBaseScale`.
- `UpdateCurvePoints()` runs per frame — Bézier control points and raycast plane recompute from current `transform.position`. Fixes pre-existing bug where moving the `HandController` GameObject broke the curve.
- `AddCardToHand` sets initial `localScale` to `cardBaseScale` (no pop-in flash).
- `RecalculateCurve()` public method + `OnValidate` (editor-only, play mode) for live Inspector tuning.

**CURRENT_STATE:** M1.9 closed in §1. §2 M1.3 decomposition all ✅. §3 renumbered: M1.5 Phase 3 becomes critical path.

**Roadmap_ALWTTT:** M1.9 DoD checked. Closure line added. M4 sequencing note updated. Phase 3 unblocked.

### Lifecycle

- M1.9 closed 2026-04-23. M1.3 decomposition fully complete (5/5 batches).
- M1 remaining: M1.5 Phase 3 (Dev Mode stat editing + Breakdown), M1.1 (Deck Editor polish).
- No semantic or authority changes — presentation-only batch.

---

## 2026-04-23e — M1.3b closure: SpecialKeywords audit + JSON importer improvements

### Semantic changes

**SpecialKeywords model clarified and cleaned (applies from M1.3b closure):**
- `SpecialKeywords` enum reduced from 13 to 7 canonical values. Two categories formalized: card-trait keywords (`Consume`, `Exhaust`, `Ethereal`) and resource/mechanic/audience keywords (`Stress`, `Vibe`, `Convinced`, `Tall`). 6 legacy entries that duplicated status-effect concepts removed (`Chill`, `Skeptical`, `Heckled`, `Hooked`, `Blocked`, `Stunned`). Card assets cleaned of stale references.
- `SpecialKeywordData` asset populated with authored descriptions for `Consume`, `Exhaust`, `Ethereal`.
- Keyword model documented in `SSoT_Card_System.md` new §3.3: canonical inventory (§3.3.1), modeling rule that status effects are NOT keywords (§3.3.2), runtime coupling gap between `ExhaustAfterPlay` bool and `Exhaust` keyword (§3.3.3).

**JSON importer improvements (applies from M1.3b closure):**
- `CardJsonImport` DTO gained `keywords` string array field. Case-insensitive enum parsing. Unknown values emit `Debug.LogWarning` and are skipped; import is not blocked.
- `CardBatchJsonImport` gained `defaultEntry` field (`EntryJson`). When present, applied to any card in the batch whose own `entry` block is absent or has null/empty `flags`. Per-card `entry` overrides the default entirely. `JsonUtility` default-construction handled via `flags` discriminator.
- Exhaust coherence warning: non-blocking `Debug.LogWarning` when `exhaustAfterPlay` bool and `Exhaust` keyword diverge in either direction.

**Card Editor create wizard default (applies from M1.3b closure):**
- `_newKind` resets to `CardAssetFactory.CreateCardKind.Action` each time the create wizard is opened. Prevents serialized state from defaulting to Composition after previous use. Fixes the dual-button UX trap documented in CURRENT_STATE §4.

**SSoT_Card_System.md:**
- New §3.3 `SpecialKeywords`: canonical inventory, modeling rule, runtime coupling gap.

**SSoT_Card_Authoring_Contracts.md:**
- §5.3 expanded: `keywords` added to optional top-level fields with usage details.
- New §5.7 batch wrapper schema: `defaultEntry`, merge rule, `EntryJson` fields.
- New §5.8 keyword coherence rules: exhaust warning behavior.
- New §7.4 create wizard defaults.

**CURRENT_STATE:**
- §1 new "M1.3b — complete (2026-04-23)" block.
- §2 M1.3b marked ✅ closed. Decomposition status updated.
- §3 M1.3b removed; M1.9 promoted to item 1.
- §4 "Card Editor dual-button UX trap" removed (fixed). "Raw Inspector drawer" updated (M1.3b reference removed, deferred to M1.1). New item: "Keyword-driven runtime behavior" follow-up.
- §5 updated: no pending M1.3 doc edits.

**Roadmap_ALWTTT:**
- M1.3b DoD checkbox marked ✅ with closure date.
- M1.3b scope block gains closure notes and delivered list.
- M1.10 marked ✅ in priority sequence list.

### Authority changes

- `SSoT_Card_System.md` §3.3 established as primary authority for SpecialKeywords conceptual model (what keywords exist, what categories they belong to, what is NOT a keyword).
- `SSoT_Card_Authoring_Contracts.md` §5.7 established as primary authority for JSON batch wrapper schema including `defaultEntry`.
- `SSoT_Card_Authoring_Contracts.md` §5.8 established as primary authority for keyword coherence warning behavior.

### Structural changes

- `Assets/Scripts/Cards/Editor/CardEditorWindow.cs` — create wizard toggle block modified (3 lines: open-guard resets `_newKind` to Action).
- `Assets/Scripts/Cards/Editor/CardEditorWindow_JsonImport.cs` — `keywords` field added to `CardJsonImport` DTO; `defaultEntry` field added to `CardBatchJsonImport`; keywords application + coherence warning in `TryStageCardFromDto`; `defaultEntry` merge in `TryParseJsonToCardDtos`.
- `SpecialKeywordData` asset — 3 entries populated (Consume, Exhaust, Ethereal).

### Smoke tests

- ST-M13b-1 (Exhaust keyword tooltip on hover) ✅
- ST-M13b-2 (multiple keyword tooltips) ✅
- ST-M13b-3 (no orphan tooltips from removed keywords) ✅
- ST-M13b-4 (JSON import with keywords) ✅
- ST-M13b-5 (exhaust coherence warning fires) ✅
- ST-M13b-6 (batch defaultEntry inheritance) ✅
- ST-M13b-7 (create wizard Kind defaults to Action) ✅
- ST-M13b-8 (unknown keyword warning) ✅

### Lifecycle

- M1.3b closed 2026-04-23.
- Next on critical path: M1.9 (HandController sizing refactor).
- M1 decomposition status: M1.3a ✅, M1.3c ✅, M1.10 ✅, M1.3b ✅, M1.9 next.

### Operational changes

- None. No authority relocations beyond new subsections added to existing SSoTs.

---

## 2026-04-23d — M1.10 closure: right-click card detail view modal

### Semantic changes

**Card detail modal (new, applies from M1.10 closure):**
- `CardDetailViewController` singleton (`ALWTTT.UI`, `Assets/Scripts/UI/CardDetailViewController.cs`). Manages a dedicated Screen Space – Overlay canvas with dim background and dismiss button. `Show(CardDefinition)` / `Hide()` / `Toggle(CardDefinition)` API.
- `CardBase.OnPointerDown` now discriminates `PointerEventData.InputButton.Right` vs left. Right-click calls `CardDetailViewController.Toggle(CardDefinition)`. Left-click retains existing behavior (`HideTooltipInfo()`).
- `HandController.DisableDragging()` called on modal open; `EnableDragging()` on dismiss.
- Dismiss paths: background click (Button on DimBackground), Esc key in `Update()`, right-click toggle on same card.

**Card detail description (new, applies from M1.10 closure):**
- `CardDefinitionDescriptionExtensions.GetDetailDescription()` added. Action cards: delegates to `CardEffectDescriptionBuilder.BuildList` (same as card face). Composition cards: primary kind header, style-bundle asset name (Track), part custom label + musician id (Part), full modifier list via `PartEffect.GetLabel()` with scope/timing tags, and `CardPayload.Effects` via builder.

**SSoT_Card_System.md §10.3 (new):**
- Documents the card detail modal as the third card-information surface.

**CURRENT_STATE:**
- §1 new "M1.10 — complete" block.
- §2 M1.10 marked ✅ closed.
- §3 M1.10 removed from "What is next"; M1.3b promoted to item 1.
- §4 composition-face risk updated (M1.10 now provides full inspection).

**Roadmap_ALWTTT:**
- M1.10 DoD checkbox marked ✅ with closure date.
- M1.10 scope block gains closure notes.

### Authority changes

- `CardDefinitionDescriptionExtensions.GetDetailDescription()` established as owner of composition detail text formatting. `GetDescription()` (card-face) unchanged.
- `CardDetailViewController` established as third runtime surface for card information, after card-face (§10.1) and card-hover tooltips (§10.2). Read-only consumer of `PartEffect.GetLabel()`, `CardEffectDescriptionBuilder`, `TrackActionDescriptor`, `PartActionDescriptor`.

### Structural changes

- `Assets/Scripts/UI/CardDetailViewController.cs` — new file.
- `Assets/Scripts/Cards/Extensions/CardDefinitionDescriptionExtensions.cs` — `GetDetailDescription()` added, `BuildCompositionDetailDescription()` private helper added. Existing `GetDescription()` and `BuildCompositionDescription()` unchanged.
- `Assets/Scripts/Cards/CardBase.cs` — `OnPointerDown` body modified (right-click branch added). No other changes.
- Gig Scene hierarchy — `CardDetailCanvas` GameObject added with `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `CardDetailViewController` components. Children: `DimBackground` (Image + Button), `CardDetailPanel` with TMP fields.

### Smoke tests

- ST-M110-1 (action card right-click opens modal) ✅
- ST-M110-2 (Esc dismisses) ✅
- ST-M110-3 (background click dismisses) ✅
- ST-M110-4 retired (overlay blocks card input by design)
- ST-M110-5 retired (same reason)
- ST-M110-6 (composition detail shows full modifier list) ✅
- ST-M110-7 (left-click unaffected) ✅
- ST-M110-8 retired (precondition impossible)

### Lifecycle

- M1.10 closed 2026-04-23.
- Next on critical path: M1.3b (`SpecialKeywords` audit).
- M1 decomposition status: M1.3a ✅, M1.3c ✅, M1.10 ✅, M1.3b next, M1.9 last.

### Operational changes

- None. No authority relocations.

---

## 2026-04-23c — M1.3c closure: card-hover stacked tooltips, position fix, editor fix

### Semantic changes

**CardBase card-hover tooltip — position fix (applies from M1.3c closure):**
- Static-anchor path (`tooltipAnchor` + `HandController.Cam` + `WorldToScreenPoint`) removed. Root cause: `TooltipAnchor` RectTransform at canvas anchor (1,1) resolved to world position ~(97, 82, -14), producing screen coords ~(24552, 19951) on a 2560×1440 screen. HandCamera FOV and distance made this projection internally consistent but far off-screen.
- Replaced with mouse-follow mode: `ShowTooltip` calls omit anchor and camera args. `TooltipController.SetPosition` uses `Input.mousePosition` (existing `isFollowEnabled` path). Tooltip stack appears near the cursor, which is already on the card.
- `tooltipAnchor` serialized field and `Card3D/CardCanvas/TooltipAnchor` prefab child remain in place (no-op at runtime). Available for future static-anchor work if needed.

**Card Editor AddEffect fix (applies from M1.3c closure):**
- `CardEditorWindow.AddEffect()` now calls `effectsProp.serializedObject.ApplyModifiedProperties()` and `EditorUtility.SetDirty()` inside the GenericMenu callback. Previously, the callback ran outside the OnGUI pass's `BeginChangeCheck`/`EndChangeCheck` scope, so the modification was applied to a stale `SerializedObject` and never committed. Fix applies to both Action and Composition payloads.

**SSoT_Status_Effects.md §3.3 (bundled M1.3a + M1.3c layers):**
- Replaced the M1.2 "tooltip content not wired" stub with full specification: `StatusEffectSO.Description` field, two runtime hosts (`StatusIconBase` per-icon hover, `CardBase` card-hover extraction), wiring details, data ownership.

**SSoT_Card_System.md §10 (bundled M1.3a + M1.3c layers):**
- Expanded from a 4-line policy note into two subsections: §10.1 card-face text rendering (`CardEffectDescriptionBuilder` as single owner, composition face density decision) and §10.2 card-hover tooltips (extraction path, dedupe, ordering, layout).

**CURRENT_STATE:**
- §2 M1.3c marked ✅ closed.
- §3 item 1 becomes M1.10 (right-click card detail modal).
- §4 resolved items removed (position bug, pending SSoT layers).
- §5 pending doc edits cleared.

**Roadmap_ALWTTT:**
- M1.3c marked ✅ closed with closure notes.

### Authority changes

- `SSoT_Status_Effects.md` §3.3 now documents both tooltip runtime hosts. No authority relocation — `StatusEffectSO` remains the single source of description text.
- `SSoT_Card_System.md` §10 now documents `CardEffectDescriptionBuilder` (§10.1) and the card-hover extraction path (§10.2). No authority relocation — builder was already the de facto owner since M1.3a.

### Structural changes

- `CardBase.cs` — `ShowTooltipInfo()` simplified (anchor/cam lines removed, ShowTooltip calls use mouse-follow).
- `TooltipController.cs` — diagnostic log removed.
- `CardEditorWindow.cs` — `AddEffect()` commits immediately via `ApplyModifiedProperties`.

### Findings recorded

- Raw Inspector `[SerializeReference]` drawer: Unity default doesn't show type picker for `CardEffectSpec` list elements. Card Editor is the intended path. Defer custom drawer to M1.3b/M1.1.
- Composition card face does not surface `CardPayload.Effects`: by design. Tooltip covers discoverability. Design question for M4.

### Lifecycle

- M1.3c closed 2026-04-23. All ST-M13c-1..7 pass.
- Next on critical path: M1.10 (right-click card detail modal).
- M1.3 decomposition status: M1.3a ✅, M1.3c ✅, M1.10 next, M1.3b after, M1.9 last.

## 2026-04-21b — Starter deck design pass: M4 track scoped, 2-musician roster, Flow bifurcation, first audience-side status, C1 promoted

### Semantic changes

**Starter deck design (new, applies to planning only — no runtime yet):**
- Starter band scope set to 2 musicians: Robot C2 (drummer / drum machine) and Sibi (keyboardist, worm-like entity with psychic affinity). Conito and Ziggy are explicitly deferred to a post-MVP roster expansion track. Bass pipeline validation is not on the M4 critical path.
- Starter deck target composition: 12 cards, 7 unique, ratio 8 Composition : 4 Action. Four generics (Warm Up, Take Five, Hype It Up — note Hype It Up omitted in final table, kept in design-space only — Mind Tap is Sibi's identity-action + Earworm applicator), three musician-identity cards across Action and Composition (Mind Tap, Steady Beat, Four on the Floor, Synth Pad, Hook Theme). Copies distribution asymmetric to reinforce C2 as the "engine" (5 composition copies) and Sibi as the "color" (3 composition copies). Authoritative card table in `planning/Design_Starter_Deck_v1.md`.
- **Flow bifurcation decision:** Flow applies as a flat per-card bonus on **Action cards** (current behavior, retained) and as a multiplier on **Composition cards** and at **Song End conversion** (both new). `flowVibeMultiplier = 0.08f` as initial tuning. The originally-documented Flow → SongHype multiplier path is explicitly retired and will not be re-activated.
- **Audience-side status scope for MVP:** Earworm (CSO primitive `DamageOverTime`, scope: single audience member, tick `AudienceTurnStart`: audience gains `+N Vibe` per N stacks then decay 1). Assigned to Sibi on thematic grounds (worm → earworm). `Captivated` (CSO `DamageTakenUpMultiplier`, intended identity status for Ziggy) and `ApplyIncomingVibe` helper explicitly deferred to roster expansion.
- **C1 audit finding (2026-03-20) promoted to bloqueante prerequisite:** `AddStressAction` bypass of `ApplyIncomingStressWithComposure` invalidates encounter tuning. Scheduled as M4.1, the first batch of the starter-deck track.
- **Bidirectional guaranteed draws decision:** on composition phase entry, if no composition card in hand and at least one exists in piles, force-draw one. Symmetric on action window entry. Adopted as mitigation so the 8:4 ratio never produces empty phases without dictating the ratio itself.
- **Deck Contract Evolution promoted from "Future milestones" to active work:** `BandDeckData` moves from `List<CardDefinition>` to a multiset representation. `CardAcquisitionFlags.starterCopies` stops being authoring-only metadata. Required for the starter's `×3 Steady Beat` / `×2 Four on the Floor` / `×2 Synth Pad` / `×2 Warm Up` copies.

**Roadmap_ALWTTT:**
- New **Milestone 4 — Starter Deck Foundations** added after M3. Six batches: M4.1 Fix C1 → M4.2 Flow bifurcation + `LoopScoreCalculator` 2-musician retune → M4.3 Earworm → M4.4 Deck Contract Evolution → M4.5 Bidirectional guaranteed draws → M4.6 Starter deck v1 authoring. M4.2 and M4.3 parallelizable; M4.4 and M4.5 parallelizable after M4.1; M4.6 depends on all previous.
- M4 is sequenced **after M1 closure**; it does not compete with in-flight M1.3c / M1.10 / M1.3b / M1.9.
- "Future milestones" section renamed "Roster Expansion" as first sub-entry; Conito + Ziggy + Captivated + `ApplyIncomingVibe` recorded there.
- M3.2 Audience pressure expansion cross-references M4.3 (Earworm is the first audience-side status delivered; Captivated and encounter-variety statuses are the M3.2 follow-ons).

**CURRENT_STATE:**
- §2 new block for M4 (new track, scoped, sequenced post-M1).
- §3 item 7 adds "M4 Starter Deck Foundations (new track) — first batch M4.1 Fix C1" in the "what is next" list.
- §4 open items: C1 promoted from audit-finding footnote to explicit bloqueante with schedule; new open items for `CompositionCardPayload.effects` verification and runtime tuning values pending.
- §4 residual risks: M4 roster reduction (2-musician starter), `LoopScoreCalculator` retune gap, Flow path divergence between current runtime and planned design, `ApplyIncomingVibe` deferral.
- §5 pending doc edits: the full map of SSoT edits that become due at each M4 batch closure (§6.1, §6.2, §5.4 of `SSoT_Gig_Combat_Core.md`; §7.1 of `SSoT_Scoring_and_Meters.md`; new §5.7 of `SSoT_Status_Effects.md`; §8 and §10 of `SSoT_Audience_and_Reactions.md`; `SSoT_Card_Authoring_Contracts.md`; `SSoT_Card_System.md`; `ssot_manifest.yaml`).
- §5 new planning docs added: `planning/Design_Starter_Deck_v1.md`, `planning/Design_Audience_Status_v1.md`.

### Authority changes

- No SSoT promoted, no SSoT retired. The M4 design pass lives in `planning/` (not in `systems/` or `runtime/`) and is explicitly marked as design-space subject to playtest revision.
- `Roadmap_ALWTTT.md` is the primary home for M4 sequencing and scope.
- `planning/Design_Starter_Deck_v1.md` is the primary home for starter deck composition, musician identity mapping, and per-card tuning rationale until the starter is authored (M4.6), at which point the primary home migrates to the `.asset` files themselves with `Design_Starter_Deck_v1.md` retained as historical rationale.
- `planning/Design_Audience_Status_v1.md` is the primary home for the Earworm spec until M4.3 closure, at which point the primary home migrates to `SSoT_Status_Effects.md` §5.7 and `planning/Design_Audience_Status_v1.md` is retained as historical rationale. The deferred portions (`Captivated`, `ApplyIncomingVibe`) remain in the planning doc until they enter roster expansion scope.
- No changes to `SSoT_INDEX.md`, `coverage-matrix.md`, or `ssot_manifest.yaml` required yet. `ssot_manifest.yaml` hard_invariants will need updates after M4.2 (Flow bifurcation) and M4.4 (deck multiplicity) — noted in `CURRENT_STATE.md` §5.

### Structural changes

- None in code. This entry records a design pass, not an implementation batch.
- Two new planning documents created:
  - `Docs/planning/Design_Starter_Deck_v1.md`
  - `Docs/planning/Design_Audience_Status_v1.md`

### Lifecycle

- M4 Starter Deck Foundations scoped 2026-04-21. Not started. Sequenced post-M1 closure.
- Within M4, six batches scoped with dependencies; none started.
- C1 (first surfaced in `ALWTTT_Combat_MVP_Audit_Final.md` on 2026-03-20 as Tier-1 closure criterion, recorded but not scheduled at the time) is now scheduled as M4.1.
- Deck Contract Evolution (previously under "Future milestones" since the Combat MVP phase) is promoted to M4.4 on the strength of the starter design requiring copies.
- Captivated audience status and `ApplyIncomingVibe` helper remain in planning, deferred to Roster Expansion (post-MVP track).
- Earworm (newly scoped audience-side status) is scheduled as M4.3 and will have its own semantic SSoT entry at batch closure.

### Operational changes

- No scope change to M1 decomposition (still M1.3a ✅ → M1.3c → M1.10 → M1.3b → M1.9).
- M1.3c remains the current critical-path item. M4.1 becomes the next critical-path item after full M1 closure.
- No existing deliverable cancelled. No existing assumption reversed.

### Findings recorded

**Mechanics / code findings surfaced during the design pass:**
- `AddStressAction` bypasses `ApplyIncomingStressWithComposure` (C1 — already on record from the 2026-03-20 audit; reaffirmed and scheduled).
- `ResolveLoopEffect()` returns 0 — audience `impressionFactor` is always 1.0 at song end. Not changed by M4; noted in design doc as a limit of current Vibe conversion honesty.
- `LoopScoreCalculator.ComputeLoopScore` awards +3 per active role (Rhythm/Bass/Harmony/Melody). With a 2-musician band (no Bass, possibly no Melody on some turns), the baseline LoopScore sits systematically lower, which makes the piecewise `ComputeHypeDelta` thresholds misfire against a thinner band. Retune bundled into M4.2.
- Flow is currently applied only as a per-card flat bonus in `CardBase.ExecuteEffects` → `ComputeBandFlowStacks`. It is NOT applied in `GigManager.ComputeSongVibeDeltas`. The originally-documented Flow → SongHype multiplier code exists but is disabled by default. M4.2 rationalizes this into the bifurcated model and retires the disabled path officially.
- `BandDeckData.Cards : List<CardDefinition>` is deduplicated by reference in `PersistentGameplayData.SetBandDeck`. True copies are not a runtime concept today. M4.4 closes this.
- `CompositionCardPayload.effects` support for `CardEffectSpec` alongside the musical payload is consistent with SSoT but not code-verified. Gates `Four on the Floor` in M4.6 authoring.

**Planning decisions (not findings — design choices made during the session):**
- Earworm assignation to Sibi on thematic pun grounds (worm → earworm). Reassignable without mechanic change if playtest disagrees.
- Asymmetric copies distribution (C2 5 composition : Sibi 3 composition) to reinforce narrative roles.
- Starter deck tuning values (VibeGoals for Heckler/Critic encounter placeholders, `flowVibeMultiplier = 0.08f`) are first-guesses and will be revised at M4.6 tuning pass against real runtime values.

### Pending doc edits

All held until their respective M4 batches land in code:
- `SSoT_Gig_Combat_Core.md` §5.4, §6.2 — unified Stress path. Post-M4.1.
- `SSoT_Gig_Combat_Core.md` §6.1 — Flow combat meaning bifurcation; retirement of Flow → SongHype path. Post-M4.2.
- `SSoT_Scoring_and_Meters.md` §7.1 — Flow update, mirror of the above. Post-M4.2.
- `SSoT_Status_Effects.md` — new §5.7 Earworm. Post-M4.3.
- `SSoT_Audience_and_Reactions.md` §8, §10 — Earworm as first active audience-side status; remove "audience statuses optional for MVP". Post-M4.3.
- `SSoT_Card_Authoring_Contracts.md` — `starterCopies` is runtime-consumed. Post-M4.4.
- `SSoT_Card_System.md` — deck multiplicity semantics. Post-M4.4.
- `ssot_manifest.yaml` hard_invariants — update after M4.2 and M4.4.

---

## 2026-04-23b — M1.3c: card-hover stacked tooltips code landed, position bug open

### Semantic changes

**CardBase card-hover tooltip model (new, applies from M1.3c):**
- `CardBase.ShowTooltipInfo()` now aggregates tooltips from two sources in one call sequence: (1) `CardDefinition.Keywords` resolved against `TooltipManager.SpecialKeywordData`, (2) unique `StatusEffectSO`s extracted from `CardDefinition.Payload.Effects` filtered to `ApplyStatusEffectSpec.status`. Dedupe via `HashSet<StatusEffectSO>`. Display order: keywords first, statuses second.
- Previous behavior (early-return when `Keywords.Count <= 0`, keyword-only iteration) is replaced. Keyword-only cards continue to show keyword tooltips unchanged; cards with status-bearing effects now also surface those tooltips with authored headers + `StatusEffectSO.Description` bodies.
- New serialized field `CardBase.tooltipAnchor` (protected `Transform`). When unset, falls back to `descriptionRoot`. Card prefabs assign a top-right anchor Transform intended to sit just outside the card face.

**CURRENT_STATE:**
- §1 "M1.3a — complete" retained.
- §2 M1.3c entry rewritten: code landed, functional smoke tests ST-M13c-1 and ST-M13c-2 pass, position bug outstanding, remaining smoke tests deferred, doc updates held to batch closure.
- §3 "What is next" re-numbered: M1.3c closure is now item 1.
- §4 new open item: Card Editor dual-button UX trap (findings below).
- §4 new residual risk: `SSoT_Status_Effects.md` §3.3 and `SSoT_Card_System.md` §10 now carry two pending layers (M1.3a authority + M1.3c extraction path).
- §4 new residual risk: `TooltipController.SetPosition` clamp assumes pivot (0, 0).
- §5 pending doc edits restated to cover both M1.3a and M1.3c layers in one update at batch closure.

### Authority changes

- `CardBase` confirmed as the assembly point for multi-source card hover tooltips. It reads `CardDefinition.Keywords` and `CardDefinition.Payload.Effects` but does not own the data in either.
- `StatusEffectSO` tooltip surfacing now has two runtime hosts: `StatusIconBase` (per-character-icon hover, from M1.3a) and `CardBase` (card-hover via payload effect extraction, from M1.3c). The SO itself remains the single source of Description text.
- Owner docs to be updated at M1.3c closure: `SSoT_Status_Effects.md` §3.3 (cross-reference `CardBase` as second host) and `SSoT_Card_System.md` §10 (document the extraction path).

### Structural changes

- `Assets/Scripts/Cards/CardBase.cs` — refactored `ShowTooltipInfo()`, added `tooltipAnchor` serialized field. Existing public overload at the bottom of the tooltip region left in place (no longer called by the parameterless path; flagged for future cleanup if nothing external references it).
- TooltipController prefab — `VerticalLayoutGroup` (Upper Left, spacing 5, ControlChildSize Width+Height, padding 5) + `ContentSizeFitter` (Horizontal + Vertical = Preferred Size) present and verified.
- Card3D prefab — `CardCanvas/TooltipAnchor` child Transform added.

### Findings recorded

**Tooltip position bug (open, root cause not yet confirmed):**
Tooltip stack renders at screen top-right corner, top tooltip clipped against the screen top edge. Card is bottom-center. Functional content is correct. Four candidate causes, to be disambiguated by runtime data collection before any code change:
1. `tooltipAnchor` SerializeField unassigned on the Card3D prefab variant, falling back to a `descriptionRoot` whose world position does not correspond to the card face.
2. `DeckManager.HandController.Cam` resolving to a camera other than the one that renders Card3D — `WorldToScreenPoint` then returns nonsense.
3. `TooltipController.canvasRectTransform` resolution or canvas render-mode mismatch, producing an `anchoredPos` that doesn't land under the screen-space anchor target.
4. The clamp in `TooltipController.SetPosition` pinning the stack to `canvas.width - tooltip.width` / `canvas.height - tooltip.height` because the stacked container is now larger than expected along one axis.
Next session opens with three targeted logs (tooltipAnchor assignment, active camera name+position, runtime `lastStaticTarget.position` / `WorldToScreenPoint` / canvas rect + scale / tooltip rect values). Likely fix is a prefab assignment or a one-line `SetPosition` correction.

**JSON importer findings (encountered while producing M1.3c test cards):**
- The JSON importer's top-level `CardJsonImport` DTO has no `keywords` field. Authoring a card with `SpecialKeywords` through JSON is not currently possible. Adding it is ~15 lines in `CardEditorWindow_JsonImport.ApplyCardJsonToDefinition` — bundle into M1.3b as optional sub-item M1.3b-pre.
- The Card Editor window's manual "Create Card + Payload" panel (separate from the JSON staging area) defaults to `Kind: Composition` and sits visually adjacent to the JSON path's `Save (Create Assets)` button. Observed misclick pattern during testing: pressing `Create` on the manual panel instead of `Save (Create Assets)` on the JSON panel produces a blank Composition card rather than saving the staged JSON (Action) card. No save-path bug; the two UIs are independent. Cost to harden is one default-value change — folded into M1.3b as a secondary step.

### Lifecycle

- M1.3c moves from "next" to "in flight, position bug outstanding".
- M1.3c doc updates (SSoT §3.3 and §10 layers) explicitly held until batch closes, bundled with the M1.3a layers left pending.
- ST-M13c-1 and ST-M13c-2 pass functionally (content correct; position fails). ST-M13c-3/4/5/6/7 deferred.

### Operational changes

- No authority relocations. No scope change to M1.3 decomposition.

---

## 2026-04-23a — M1.3a closure: status descriptions, card-effect text builder, per-icon tooltips

### Semantic changes

**Card-effect rendering pipeline (new, applies from M1.3a):**
- Card faces no longer display `CharacterStatusId` enum names on `ApplyStatusEffect` effects. `CardDefinitionDescriptionExtensions.GetDescription` action branch delegates to `CardEffectDescriptionBuilder.BuildList(action.Effects, stats)`.
- `CardEffectDescriptionBuilder` is the single owner of card-effect text formatting. Handles `ApplyStatusEffect`, `ModifyVibe`, `ModifyStress`, `DrawCards`. Uses TMP rich-text tokens (buff `#8FD694`, debuff `#D6858F`, numbers `#FFD084`). Zero-delta effects render as empty strings and are filtered out. Target-type phrasing centralized.
- `CardEffectSpec` remains data-only (`SSoT_Card_System.md` §6.1 unchanged). No virtual `Describe()` method added — formatting is cross-cutting and held centrally.

**Status-effect descriptions (new, applies from M1.3a):**
- `StatusEffectSO` gained a `description` field (`[TextArea(2, 4)]`) with a public `Description` getter, shown in status tooltips.
- Description text authored on the six canonical status SOs: `flow`, `composure`, `choke`, `shaken`, `exposed`, `feedback`.

**Per-icon status tooltip surface (new, applies from M1.3a):**
- `StatusIconBase` implements `IPointerEnterHandler` / `IPointerExitHandler` and gained `BindTooltipSource(StatusEffectSO, StatusEffectContainer, CharacterStatusId)`.
- Hovering a status icon on a character canvas shows `{DisplayName}` (or `{DisplayName} ×N` when stacks > 1) as header and the SO's `Description` as body, via `TooltipManager.ShowTooltip`.
- `CharacterCanvas.TryCreateIcon` calls `BindTooltipSource` immediately after `SetStatus`, on every icon created.

**CURRENT_STATE:**
- §1 new "M1.3a — complete (2026-04-23)" block summarizing the pipeline rebuild and the six authored SOs.
- §2 M1.3 decomposition block: M1.3a marked ✅.
- §3 item 1 removed (was M1.3a); remaining items renumbered.

### Authority changes

- `CardEffectDescriptionBuilder` established as single owner of card-effect text formatting. Owner SSoT at its closure: `SSoT_Card_System.md` §10.
- `StatusEffectSO.Description` established as single source of status tooltip body text. Owner SSoT: `SSoT_Status_Effects.md` §3.3.
- `StatusIconBase` established as the first tooltip host for `StatusEffectSO`s (per-character icon hover).

### Structural changes

- `Assets/Scripts/Status/StatusEffectSO.cs` — added `description` field + `Description` getter under `[Header("Presentation")]`.
- `Assets/Scripts/Cards/Effects/CardEffectDescriptionBuilder.cs` — new static class under `ALWTTT.Cards.Effects`.
- `Assets/Scripts/Cards/CardDefinitionDescriptionExtensions.cs` — action branch delegates to the builder.
- `Assets/Scripts/UI/StatusIconBase.cs` — added `IPointerEnter/ExitHandler`, `BindTooltipSource`, and backing fields for `_definition`, `_container`, `_boundId`.
- `Assets/Scripts/Characters/CharacterCanvas.cs` — one-line addition inside `TryCreateIcon` right after `SetStatus`.
- Six canonical status SO assets authored with `Description` text.

### Lifecycle

- M1.3a closed 2026-04-23. Confirmed by user.
- Pending SSoT edits (`SSoT_Status_Effects.md` §3.3, `SSoT_Card_System.md` §10) deliberately held until M1.3c closure to bundle both batches' meaning into one edit — flagged in `CURRENT_STATE.md` §5.
- Composition card face simplification (landed out-of-batch 2026-04-21) remains part of the shipped M1.3 scope and is not re-listed here.

### Operational changes

- No scope change to M1.3 decomposition.
- Next on the critical path becomes M1.3c.

## 2026-04-21 — M1.3 decomposition: five-batch split + SpecialKeywords audit findings

### Semantic changes

**Roadmap_ALWTTT:**
- `Last updated` bumped to 2026-04-21.
- M1 priority order revised. Original single M1.3 batch expanded 2026-04-20 beyond its initial scope (card-hover stacked tooltips, right-click detail modal, card sizing); 2026-04-21 decomposed into five sequenced sub-batches: M1.3a → M1.3c → M1.10 → M1.3b → M1.9. Reasoning recorded inline.
- §1.3 rewritten as a parent section describing the decomposition, each sub-batch with its own scope block. Composition card face cleanup recorded as already applied 2026-04-21 (part of what was originally in 1.3b; batch collapsed accordingly).
- §1.9 and §1.10 promoted from "not-yet-scheduled game-feel polish" notes into full roadmap entries with scope.
- Definition of Done updated: composition-face simplification checked; new unchecked items for M1.3a, M1.3c, M1.10, M1.3b, M1.9 in that order.
- Demo-readiness check updated to reference right-click detail and card readability explicitly.

**CURRENT_STATE:**
- §1 new "Composition card face description — shortened (2026-04-21)" block documenting the `BuildCompositionDescription` update to role/part + count badge.
- §2 active work: single "M1.3 Tooltip pipeline extension" line replaced with a "M1.3 decomposition — five sequenced batches" block listing M1.3a–M1.9 with per-batch scope summaries.
- §3 "What is next" rewritten to reflect the 5-batch sequence followed by M1.5 Phase 3, M1.1, M2.
- §4 new residual risk: composition face minimal display relies on M1.10 for full-detail fallback; M1.10 is correspondingly prioritized.
- §5 pending doc edits updated: `SSoT_Status_Effects.md` §3.3 and `SSoT_Card_System.md` §10 will need edits when M1.3a lands.

### Authority changes

**Keyword conceptual model clarified (batch-scope decision, applies from M1.3b onward):**
- Status-effect tooltips are auto-derived by `CardEffectDescriptionBuilder` and `StatusIconBase` from payload effect lists + `StatusEffectSO` descriptions. They do NOT route through `SpecialKeywords`.
- `SpecialKeywords` is reserved for two categories: card-trait keywords (`Exhaust`, `Consume`, `Ethereal`) and resource/mechanic/audience keywords (`Stress`, `Vibe`, `Convinced`, `Tall`).
- Any `SpecialKeywords` entry that duplicates a status-effect concept is legacy and slated for removal in M1.3b.

**Card-effect description rendering authority (applies from M1.3a onward):**
- `CardEffectDescriptionBuilder` (new static class under `ALWTTT.Cards.Effects`) becomes the single owner of card-effect text formatting.
- `CardEffectSpec` remains data-only per `SSoT_Card_System.md` §6.1 — no virtual `Describe()` method added. Formatting is cross-cutting concern handled centrally.
- Owner doc: `SSoT_Card_System.md` §10 to be updated at M1.3a closure.

### Structural changes

None in this entry — decomposition is a planning-only operation. Structural changes will be recorded at each sub-batch's closure.

### Findings recorded

**`SpecialKeywords` audit inventory (to be actioned in M1.3b):**

Current enum has 13 values. `SpecialKeywordData` asset has 10 populated entries. Classification against the new model:

| Keyword | Current populated? | Disposition |
|---|---|---|
| `Stress` | Yes | Keep — resource/mechanic |
| `Vibe` | Yes | Keep — resource/mechanic |
| `Chill` | Yes | Remove — duplicates Composure status |
| `Skeptical` | Yes | Remove — duplicates a status concept |
| `Heckled` | Yes | Remove — duplicates Feedback status |
| `Hooked` | Yes | Remove — duplicates a status concept |
| `Blocked` | Yes | Remove or convert — legacy audience state, now sprite-tint per M1.2 Decision E3 |
| `Stunned` | Yes | Remove — duplicates Shaken/DisableActions status |
| `Convinced` | Yes | Keep — audience win-state mechanic, no status equivalent |
| `Tall` | Yes | Keep — audience layout trait, unique |
| `Consume` | No | Populate — card trait |
| `Exhaust` | No | Populate — card trait; already referenced by `CardDefinition.ExhaustAfterPlay` |
| `Ethereal` | No | Populate — card trait |

M1.3b additional step: grep `CardDefinition` assets for `Keywords` references to the 6 removed values and strip them.

### Lifecycle

- M1.3 (as a single batch) formally decomposed. No longer a monolithic milestone.
- Composition card face simplification landed out-of-batch on 2026-04-21 (strictly a content change to `CardDefinitionDescriptionExtensions.BuildCompositionDescription`).
- M1.3a code is ready to apply; the full batch including authored descriptions on the 6 status SOs awaits the next working session.

### Operational changes

- Project scope unchanged. This entry reorganizes planned work but does not move any authority.

---

---
## 2026-04-20d — M1.8 closure: Status icon appear / disappear animations

### Semantic changes

**CURRENT_STATE:**
- §1 new block "Status icon animations — M1.8 complete (2026-04-20)" documenting the appear/disappear popups, the required `CanvasGroup`, the configurable duration and AnimationCurve envelopes, and the race-safe dictionary detach-before-disappear pattern in `CharacterCanvas.HandleStatusCleared`.
- §3 "What is next" reordered: M1.8 removed, M1.3 promoted to #1.

### Authority changes
None.

### Structural changes

**Modified:**
- `StatusIconBase.cs` — gained `PlayAppear()` and `PlayDisappear()` coroutine methods. Now `[RequireComponent(typeof(CanvasGroup))]`. Added serialized fields: `appearDuration`, `disappearDuration` (default 1s each, `[Min(0.01f)]`), and four `AnimationCurve` fields (`appearScaleCurve`, `disappearScaleCurve`, `appearAlphaCurve`, `disappearAlphaCurve`) with designer-facing defaults. Added `IsDisappearing` flag, `_activeAnimation` coroutine handle, and idempotent interruption semantics. `PlayAppear` sets starting visual state synchronously before starting the coroutine to avoid a one-frame flash of fully-visible icon. `PlayDisappear` is terminal — the GameObject self-destroys when the animation completes. Pure coroutines, no DOTween dependency.
- `CharacterCanvas.cs` — `TryCreateIcon` now calls `clone.PlayAppear()` after `SetStatus` and dictionary registration. `HandleStatusCleared` now removes the icon from `_activeIcons` BEFORE calling `PlayDisappear` on the detached icon. This makes the icon lifecycle race-safe: if the same status is re-applied during a disappear animation, `HandleStatusApplied` sees an empty dictionary slot and creates a fresh icon; the old one keeps animating as a detached GameObject and self-destroys when its animation completes.

**Prefab setup (no code):**
- `StatusIconBase` prefab automatically gains a `CanvasGroup` component at compile time via `[RequireComponent]`. Verify after Unity reimport.

### Findings / decisions
- Default durations set to 1s per design preference (2026-04-20), with in-code note that 0.2–0.4s is typical for snappy UI feedback. Durations and curves are Inspector-tunable per prefab without code changes.
- Change-flash on stack delta explicitly **out of M1.8 scope**. May be added later as a separate batch if visual feedback on stack changes proves necessary during general-audience testing.
- `Time.deltaTime` used (not `unscaledDeltaTime`) — matches pattern used by `CharacterAnimator`, `ForegroundAnimator`, `StageLightAnimator`. If future pause logic (`Time.timeScale = 0`) requires status icon animations to continue during pause, switch to `unscaledDeltaTime`.

### Smoke tests
- ST-M18-1 (appear popup visible with scale overshoot + alpha ramp) ✅
- ST-M18-2 (disappear popup terminal, GameObject destroyed after animation) ✅
- ST-M18-3 (stack updates mid-appear do not re-trigger animation; `HandleStatusApplied` only updates text) ✅
- ST-M18-4 (apply + clear + re-apply race: old icon animates out as detached while new icon animates in; no dict collision) ✅
- ST-M18-5 (configurable duration tunable to ~0.25s without breaking animation) ✅

### Lifecycle
- M1.8 closed. M1.3 (Tooltip pipeline extension) now top of priority list. M1.5 Phase 3 (Dev Mode stat/state editing, Breakdown entry point, T7 unblock) sequenced after M1.3.

---


## 2026-04-20c — M1.7 closure: Character hover highlight + BandCharacterCanvas hardening

### Semantic changes

**CURRENT_STATE:**
- §1 new block "Character hover highlight — M1.7 complete (2026-04-20)" documenting the URP shader, `SpriteOutlineController` component, `CharacterBase` wiring, and the hardened `BandCharacterCanvas` contextual-stats path.
- §1 Editor authoring tools: `StatusEffectWizardWindow` HelpBox hint correction noted.
- §3 "What is next" reordered: M1.7 removed, M1.8 promoted to #1.
- §5 "Pending small doc edits" cleared (nothing outstanding).

**SSoT_Status_Effects:**
- No changes in this entry. Feedback / Choke / actionTiming edits belong to the 2026-04-20 micro-batch below.

### Authority changes
None.

### Structural changes

**Added:**
- `Assets/Shaders/ALWTTT/SpriteOutlineURP.shader` — URP 2D sprite outline shader. 8-neighbor alpha-border sampling, pixel-space outline width controlled via `_OutlineWidth` material property. Tags `"RenderPipeline" = "UniversalPipeline"` and pass `"LightMode" = "Universal2D"`.
- `Assets/Scripts/Characters/SpriteOutlineController.cs` — `MonoBehaviour` that toggles outline via `MaterialPropertyBlock`. No material instantiation. SRP batching preserved. Exposes `SetOutline(bool)` idempotent API.

**Modified:**
- `CharacterBase.cs` — new serialized field `SpriteOutlineController outlineController` under `[Header("Hover Highlight (M1.7)")]`. `OnPointerEnter` / `OnPointerExit` call `SetOutline(true/false)` on the controller if assigned. Null-safe: unassigned field is a silent no-op.
- `BandCharacterCanvas.cs` — new optional serialized field `GameObject statsRoot`. `Awake` added: forces initial hidden state via `ApplyStatsVisibility(false)` to prevent scene-load flicker before `BuildCharacter` runs. `UpdateStats` now null-safe on the three text fields. `ShowContextual` / `HideContextual` refactored through a private `ApplyStatsVisibility(bool)` method that toggles both the CanvasGroup alpha and the optional statsRoot GameObject.
- `StatusEffectWizardWindow.cs` — HelpBox text at line 250 rewritten. No longer suggests `EndOfTurn`; explicitly enumerates `EndOfTurn`, `StartOfLoop`, `EndOfLoop`, `OnAction`, `OnHit`, `OnTakeDamage` as declared-but-not-wired values. See 2026-04-20 micro-batch for the SSoT justification.

**Operational / asset setup (not code):**
- Character sprite GameObjects require `SpriteOutlineController` component and a material built from the new shader on their `SpriteRenderer`. `CharacterBase.outlineController` field needs per-prefab assignment.

### Findings during closure

- **Latent multi-song action window bug surfaced** during M1.7 Dev Mode smoke tests. Tracked as a separate 2026-04-20b entry below; scope kept distinct because it is a fix, not game-feel work.
- **URP import-time warnings on `_RefractionTex*`** appear when assigning the new material to a `SpriteRenderer`. They originate from Unity's URP 2D import pass trying to reconcile global properties with custom sprite shaders. No runtime impact; outline renders correctly. Silencing the warnings would require declaring the properties as hidden dummies in the shader — deferred as low-value polish.

### Smoke tests
- ST-M17-1 (musician hover outline) ✅
- ST-M17-2 (audience hover outline, including coexistence with `IsBlocked` tint) ✅
- ST-M17-3 (batching preserved via MaterialPropertyBlock — no material instance growth on repeated hover) ✅
- ST-M17-4 (null `outlineController` is a silent no-op, no NRE) ✅

### Lifecycle
- M1.7 closed. M1.8 (Status icon animations) now top of priority list.

---

## 2026-04-20b — Latent multi-song action window bug fixed

### Semantic changes

**SSoT_Runtime_Flow:**
- §4 Phase 0: brief note added on where the per-gig flag initialization lives (`SetupGig()`), with forward reference to §4.1 for per-turn lifecycle.
- §4 Phase 1: new subsection **§4.1 "Action window flag lifecycle"**. Table of the three governing flags (`_isSongPlaying`, `_actionWindowOpen`, `_isBetweenSongs`) with their meanings. Per-turn lifecycle rule stated explicitly: `_actionWindowOpen = true` and `_isBetweenSongs = true` are re-asserted at the top of `ExecuteGigPhase(PlayerTurn)` after the completion early-exit check. Historical note on the pre-2026-04-20 latent bug documented for future readers.
- §5.1 Action cards: cross-reference to §4.1 added.
- §8 Runtime invariants: new **invariant #8** — "Action window flags are per-PlayerTurn, not per-gig." Carries forward mandate: any future gig-flow code that adds new single-use-per-gig flags must document its lifecycle in §4.

**CURRENT_STATE:**
- §1 new block "Latent multi-song action window bug — fixed (2026-04-20)".
- §4 Residual risks: new entry "GigManager flag lifecycle — surveillance item" — other single-use-per-gig flag patterns may exist in `GigManager` flag set; low-priority audit recommended.

### Operational changes
- `GigManager.ExecuteGigPhase(PlayerTurn)`: two lines inserted after the completion early-exit check, before the status tick and draw logic. Both `_actionWindowOpen` and `_isBetweenSongs` are set to `true` at every PlayerTurn entry. `_isSongPlaying` was not observed to drift and is not touched.

### Authority changes
None.

### Findings
- Pre-existing bug, not caused by any work done in this project cycle. `_actionWindowOpen` was initialized once in `SetupGig()` and flipped to `false` on the first `OnPlayPressed`, never re-opened. The second and all subsequent songs of any multi-song gig (`RequiredSongCount ≥ 2`) had all action cards silently blocked by the `CanPlayActionCard` gate. Surfaced during M1.7 Dev Mode smoke testing because Dev Mode Phase 2 was the first tooling that allowed rapid iteration across multiple songs. Affected production multi-song gigs equally.

### Smoke tests
- ST-FIX-1 (normal multi-song gig, infinite turns OFF — song 1 → audience turn → song 2 PlayerTurn action card play) ✅
- ST-FIX-2 (Dev Mode infinite multi-song — action cards remain playable across an arbitrary number of song cycles) ✅

### Lifecycle
- This is a gameplay fix, not a milestone closure. Does not advance any milestone directly but unblocked M1.7 smoke testing.

---

## 2026-04-20 — Micro-batch: Phase 2 findings closure + wizard hint fix

### Semantic changes

**SSoT_Status_Effects:**
- §5.3 Choke: design decision added. Choke-on-stunned refusal is intentional for MVP. Documented conditions under which it would change.
- §5.6 Feedback: full rewrite. Previously documented as "no decay by design" based on T8 observation; root cause identified as a misconfigured SO tick timing (`EndOfTurn` is declared but not invoked by the phase machine). Feedback now correctly decays as poison-like: damage on audience turn using current stacks, decay 1 stack at start of next player turn. Validation history added.

**SSoT_Card_Authoring_Contracts:**
- New §3.4 "Action timing and testability" added. Documents that the default `actionTiming` excludes PlayerTurn and that debug cards authored for Dev Mode spawn must use `actionTiming: Always`. Cross-references Dev Mode SSoT §8.4 and §11.4.

### Operational changes

- `StatusEffect_Feedback_Damage Over Time.asset`: `Tick Timing` changed from `EndOfTurn` → `PlayerTurnStart`. Gameplay change: Feedback stacks now decay turn-to-turn; 3 stacks deal 6 total damage over 3 audience turns.
- `StatusEffectWizardWindow.cs`: HelpBox hint around decay + tick timing rewritten to stop suggesting `EndOfTurn` (which is not wired in the runtime). New text enumerates the unwired timings as a warning.

### Authority changes

None.

### Structural changes

- No files added or removed.

### Findings / residual

- Verified via grep that no other StatusEffectSO asset uses an unwired tick timing value (2, 3, 4, 5, 6, or 7). [NOTE: append "N hits found, pending follow-up" here if grep surfaces extras. Remove this note if grep is clean.]

### Lifecycle

- Three open documentation gaps from Phase 2 closure (`Feedback decay not documented`, `Choke-on-stunned design question`, `actionTiming default on debug cards`) are all resolved. `CURRENT_STATE.md §4` "Open documentation gaps" subsection can be removed.
- Unwired tick timings (`EndOfTurn`, `StartOfLoop`, `EndOfLoop`, `OnAction`, `OnHit`, `OnTakeDamage`) remain declared in the enum. Not removed because enum-value stability matters for serialized SO assets. Wizard hint now warns against their use.

---

## 2026-04-20 — M1.5 Phase 2 closure: Dev Mode card spawner + deferred M1.2 tests (2/3)

### Semantic changes

**SSoT_Dev_Mode:**
- §1 Purpose updated: Phase 2 delivered the first interactive runtime mutation (card spawning from the full catalogue).
- §3 Overlay: window grown from `340×220` to `480×380`; tab toolbar (`Infinite` / `Catalogue`) added; Catalogue tab content documented (search + kind toggles + gate status + scrollable list delegating to `DeckManager.DevSpawnCardToHand`).
- §5 Hand-visibility gap: new Phase 2 corollary paragraph. `CanDevSpawnToHand` refuses to spawn while `DrawTransform.activeInHierarchy=false` — the spawn gate is the mirror of the Phase 1 reset-path bridge. Prevents re-entry of the ghost-card state Phase 1 fixes.
- §6 Entry points: new file row `Assets/Scripts/DevMode/DevCardCatalogueTab.cs`. `DeckManager.cs` row expanded with `using ALWTTT.Enums;` (unconditional), `DevSpawnCardToHand(CardDefinition) : bool`, `CanDevSpawnToHand()` and `CanDevSpawnToHand(out string reason)`.
- §8 Caveats: three new subsections added — §8.4 `CardActionTiming` default excludes PlayerTurn (blocked T7 validation); §8.5 spawn-accepted-pollution (decision U1: spawned cards enter the deck after reshuffle, accepted for simplicity); §8.6 `test_pass_turn` obsolete (authored under pre-`PrimaryKind` contract, removed 2026-04-20).
- §9 Smoke-test coverage: reorganized into §9.1 Phase 1, §9.2 Phase 2 (ST-P2-1..7 all passed 2026-04-20), §9.3 Multi-turn M1.2 deferred tests (T5 passed with bonus finding on stunned-target-unplayable; T8 passed with finding that Feedback has no tick decay by design; T7 deferred to Phase 3 with documented unblock path).
- §10 Update rule: added triggers for Phase 2 surfaces (tab toolbar, spawn-gate predicate, new Dev-prefixed methods).
- **New §11 Phase 2 — card spawner:** capability summary, catalogue source (`GameplayData.AllCardsList`), spawn pipeline, gate predicate ordering, spawned-card lifecycle (decision U1), overlay filter-state caching, explicit list of what Phase 2 unblocks vs. what it does not.

**CURRENT_STATE:**
- §1 project foundation: new "Dev Mode Phase 2 — complete (2026-04-20)" block added. M1.2 status annotated with "2 of 3 deferred tests closed via Dev Mode Phase 2 (T5 Choke decay ✅, T8 Feedback DoT ✅). T7 Shaken expiry deferred to Dev Mode Phase 3".
- §2 active work: "Dev Mode Phase 2 — card spawner (next)" block replaced by "Dev Mode Phase 3+ — stat & state editing (next, not yet sequenced)". Phase 3 scope expanded with a new Breakdown entry point to unblock T7. Sub-roadmap session recommended before Phase 3 implementation.
- §3 what is next: M1.5 Phase 2 removed from the ordered list; M1.7 Character hover promoted to item 1; M1.5 Phase 3 added at position 4.
- §4 open items and risks: new "Open documentation gaps (surfaced 2026-04-20 during Phase 2 testing)" subsection — (1) Feedback decay not documented in `SSoT_Status_Effects.md` §5.6; (2) Choke-on-stunned design decision pending; (3) `CardActionTiming` default documented in `SSoT_Dev_Mode.md` §8.4, may warrant a note in `SSoT_Card_Authoring_Contracts.md`. "Multi-turn status validation pending" risk rewritten as "T7 Shaken validation deferred to Phase 3" with unblock path.
- §5 docs that must be edited next: pending small doc edits listed — `SSoT_Status_Effects.md` §5.6 (Feedback no-decay) and §5.3 (Choke-on-stunned, pending design decision).

**Roadmap_ALWTTT:**
- `Last updated` bumped to 2026-04-20.
- M1 priority order revised: M1.5 Phase 1 and 2 now closed; M1.7 / M1.8 / M1.3 / M1.5-Phase-3 / M1.1 is the revised sequence. Reasoning updated.
- §1.2 "Deferred to M1.5 for validation" note rewritten with outcomes: T5 ✅ PASSED, T8 ✅ PASSED, T7 ⏸️ DEFERRED to M1.5 Phase 3. Cross-references to `SSoT_Dev_Mode.md` §9.3.
- §1.5 Phase 2 bullet block rewritten from "(next)" to "✅ (closed 2026-04-20)" with summary of delivered surface and `test_pass_turn` removal note.
- §1.5 Phase 3 promoted from "scope only, not yet sequenced" to "next, not yet sequenced" with new Breakdown entry point added to scope. Sub-roadmap recommended.
- §2 Milestone 2 "Unblocked by" reworded: "M1.5 Dev Mode Phases 1 and 2 — both now closed".
- Definition of Done: four items flipped to checked — Phase 2 card spawner; Dev Mode scene tested for arbitrary card play + meter/status changes; CURRENT_STATE updated. New unchecked item: "Dev Mode scene supports Breakdown entry point (Phase 3; unblocks T7)".

### Authority changes

**Phase 2 spawn-gate authority established:**
- The fact that `CanDevSpawnToHand` is the single gate for any Dev Mode runtime spawn (PlayerTurn-only, MaxCardsOnHand-respecting, hand-visibility-respecting) is now governed in `SSoT_Dev_Mode.md` §11.4. Overlay code must not mutate hand state except through `DevSpawnCardToHand`. Any new Phase-3+ runtime mutation that adds cards to the hand must route through this same gate or justify divergence explicitly.

**Decision U1 codified:**
- Spawned cards enter `HandPile` only, and `DiscardPile` on play, and `DrawPile` on next reshuffle. This is "accepted pollution" — decided 2026-04-20 during the Phase 2 sub-roadmap. Codified in `SSoT_Dev_Mode.md` §8.5 and §11.5. Ephemeral-spawn semantics require an explicit Phase 3+ override and must not branch the normal path.

### Structural changes

**New files:**
- `Assets/Scripts/DevMode/DevCardCatalogueTab.cs` — Phase 2 IMGUI helper. File-level `#if ALWTTT_DEV`. Static class; static filter state; renders Catalogue tab body into the existing `DevModeController` window. No runtime mutation outside its single call to `DeckManager.DevSpawnCardToHand`.

**Modified files (code):**
- `Assets/Scripts/DevMode/DevModeController.cs` — window default grown from `340×220` to `480×380`; `_activeTab` int + `GUILayout.Toolbar` added to `DrawWindow`; Phase 1 body extracted into `DrawInfiniteTab()` (byte-identical); Catalogue tab dispatches to `DevCardCatalogueTab.Draw()`; placeholder "Phase 2 — coming next" label removed.
- `Assets/Scripts/Managers/DeckManager.cs` — `using ALWTTT.Enums;` added (unconditional, needed by `GigPhase` reference in `CanDevSpawnToHand`; free in production). Three methods added inside existing `#if ALWTTT_DEV` region: `DevSpawnCardToHand(CardDefinition) : bool`, `CanDevSpawnToHand() : bool`, `CanDevSpawnToHand(out string reason) : bool`.

**Removed assets:**
- `test_pass_turn.asset` + `test_pass_turn_Payload.asset` — composition card authored 2026-03-23 under the pre-`PrimaryKind` contract. `SongCompositionUI.ApplyCardToPart` now rejects `PrimaryKind == None` (`"unsupported PrimaryKind 'None'"` → returns false), so the card could not legally advance a composition. Removed 2026-04-20; ST-P2-4 re-validated with the `Waltz` composition card from `TestDeck_FullCoverage`. If a "no-op composition card" concept is needed again, it requires an explicit design decision to extend `ApplyCardToPart`.

**No governance file changes needed:**
- `SSoT_INDEX.md` — no new row (SSoT_Dev_Mode already registered in Phase 1).
- `coverage-matrix.md` — no new row ("Dev Mode tooling" already registered; primary home unchanged).

### Lifecycle

- M1.5 Phase 2 (Dev Mode card spawner) closed.
- M1.5 Phase 3 (stat & state editing, Breakdown entry point) is now the active next step in the M1.5 track. A sub-roadmap session is recommended before implementation.
- M1.2 multi-turn status validation gap: 2 of 3 deferred tests closed (T5 Choke decay, T8 Feedback DoT). T7 Shaken expiry re-pointed from "Phase 2" to "Phase 3" with a documented unblock path (Dev entry point to force Breakdown, or a debug Shaken card with `actionTiming: "Always"`).
- Critical-path M1 ordering revised: M1.7 character hover highlight is now the top priority on the path to general-audience testing readiness.

### Operational changes

- None.

### Findings surfaced during Phase 2 testing (not yet applied — tracked as doc gaps in CURRENT_STATE §5)

- **Feedback decay documentation gap.** `SSoT_Status_Effects.md` §5.6 documents Feedback's tick timing and per-stack damage but does not state that the status has no decay. T8 validated that stacks persist turn-to-turn until cleared externally; the SSoT should make this explicit. Small doc edit, not batched in this closure.
- **Choke-on-stunned design question.** T5 surfaced that `HandController.TryResolveCardTarget` refuses to target a stunned musician, blocking re-application of Choke stacks. Whether this is intentional (stun is binary, stacks are for duration only) or a restriction that should be lifted is undecided. Document in `SSoT_Status_Effects.md` §5.3 when resolved.
- **`CardActionTiming` default on debug cards.** Action cards without an explicit `actionTiming` inherit a default that excludes PlayerTurn, making them unplayable via Phase 2 spawn. Documented in `SSoT_Dev_Mode.md` §8.4. May warrant a note in `SSoT_Card_Authoring_Contracts.md` when testing-card authoring conventions are formalized.

### Deferred / known limitations

- T7 Shaken expiry test remains deferred; unblock path documented in `SSoT_Dev_Mode.md` §9.3 and captured in Phase 3 scope.
- `test_pass_turn` removed; "no-op composition card" concept not currently representable. No reintroduction scheduled.
- `CardBase.Discard()` GameObject accumulation under `DiscardPos` now worsens visibly under Phase 2 usage (spawning creates and discards cards quickly). Still cosmetic — gameplay state remains correct. Out of scope; documented in `SSoT_Dev_Mode.md` §8.2.

---

## 2026-04-17 — M1.5 Phase 1 closure: Dev Mode (infinite turns + hand-visibility bridge)

### Semantic changes

**SSoT_Dev_Mode (new):**
- Created and activated as the primary authority for Dev Mode tooling.
- Documents `ALWTTT_DEV` scripting-define gating strategy (no asmdef; file-level + block-level `#if` guards; production builds strip cleanly).
- Documents F12 IMGUI overlay, `DevModeController` singleton, `_overlayScale` inspector scale.
- Documents Infinite Turns runtime semantics: `IsGigComplete` always false, `WinGig`/`LoseGig` suppressed with log, convinced-audience auto-reset at PlayerTurn start, hard hand reset + visibility re-enable in `OnCompositionSessionEnded`.
- §5 documents the load-bearing hand-visibility gap: CompositionSession calls `ShowHand(false)` during setup; single-song production code never re-shows because the gig ends. Dev Mode bridges that gap explicitly. Any future multi-song production path must bridge the same gap.
- §8 documents Phase 1 caveats: direct Gig Scene entry path (bypasses GigSetupScene) is out of scope; `CardBase.Discard()` GameObject accumulation under `DiscardPos` is cosmetic and deferred; `GigPhase.SongPerformance` appears unused in logs.

**CURRENT_STATE:**
- §1 new "Dev Mode Phase 1 — complete (2026-04-17)" block added, summarizing delivered capability and smoke-test pass.
- §2 "QA-readiness gap" block replaced by "Dev Mode Phase 2 — card spawner (next)". Multi-turn status validation tests (T5/T7/T8) re-pointed to Phase 2 rather than Phase 1.
- §3 "What is next" item 1 reframed from "M1.5 Dev Mode" to "M1.5 Dev Mode — Phase 2 card spawner".
- §4 "Multi-turn status validation pending" risk updated to reflect Phase 2 as the target, not general M1.5.
- §5 "Dev Mode design SSoT" pending-doc line removed (now delivered).

**Roadmap_ALWTTT:**
- `Last updated` bumped to 2026-04-17.
- §1.5 body restructured into Phase 1 (closed), Phase 2 (next), Phase 3+ (scope only). Phase 1 summary lists delivered capability and references `SSoT_Dev_Mode.md`.
- §1.2 "Deferred to M1.5 for validation" note updated: multi-turn smoke tests re-pointed to Phase 2 (card spawner needed for arbitrary starting conditions).
- Definition of Done: "Dev Mode scene functional" item split into "Phase 1 ✅" (checked) and "Phase 2 card spawner" (unchecked).

### Authority changes

**New governed SSoT activated:**
- `systems/SSoT_Dev_Mode.md` is the primary authority for Dev Mode tooling. Registered in `SSoT_INDEX.md` (Systems table) and `coverage-matrix.md` ("Dev Mode tooling" row).

**Hand-visibility bridge authority established:**
- The fact that CompositionSession hides the hand via `ShowHand(false)` and that production code has no symmetric re-show between song-end and the next PlayerTurn is now governed in `SSoT_Dev_Mode.md` §5. Any production change that introduces a non-Dev-Mode multi-song path must bridge that gap and update this SSoT.

### Structural changes

**New files:**
- `Assets/Scripts/DevMode/DevModeController.cs` — singleton, F12 overlay, infinite-turns state, `OnPlayerTurnStartInfiniteMode`, `ResetConvincedAudience`. File-level `#if ALWTTT_DEV`.
- `Docs/systems/SSoT_Dev_Mode.md` — new governed SSoT.

**Modified files (code, block-level `#if ALWTTT_DEV` patches only):**
- `Assets/Scripts/Managers/GigManager.cs` — five patches: `using ALWTTT.DevMode`; `IsGigComplete` bypass; `ExecuteGigPhase(PlayerTurn)` diagnostic logs + completion-check bypass + `OnPlayerTurnStartInfiniteMode` call; `WinGig` and `LoseGig` early-return suppression; `OnCompositionSessionEnded` diagnostic dump + `DevForceHandResetToDiscard` + `SetHandVisible(true)`.
- `Assets/Scripts/Managers/DeckManager.cs` — `DevForceHandResetToDiscard()` method; `DrawCards` entry diagnostic log block.
- `Assets/Scripts/Characters/AudienceCharacterStats.cs` — `DevResetConvinced()` method.

**Modified governance files:**
- `SSoT_INDEX.md` — new Systems row for `systems/SSoT_Dev_Mode.md`.
- `coverage-matrix.md` — new "Dev Mode tooling" row.

### Lifecycle

- M1.5 Phase 1 (Dev Mode: infinite turns + overlay + hand-visibility bridge) closed.
- M1.5 Phase 2 (card spawner) is now the active next step on the critical path to QA-readiness.
- Multi-turn status validation smoke tests from M1.2 (Choke decay T5, Shaken expiry T7, Feedback DoT accumulation T8) remain deferred; target re-pointed from "M1.5" generally to "M1.5 Phase 2 specifically".

### Operational changes

- None.

### Deferred / known limitations

- Direct Gig Scene entry path (bypassing GigSetupScene) is a known ghost-card reproduction under Infinite Turns. Out of Phase 1 scope. Documented in `SSoT_Dev_Mode.md` §8.1.
- `CardBase.Discard()` reparents GameObjects to `DiscardPos` without destroying them. Cosmetic accumulation in Infinite Turns. Documented in `SSoT_Dev_Mode.md` §8.2.
- `GigPhase.SongPerformance` appears unused in runtime logs. Separate runtime cleanup, out of Dev Mode scope. Documented in `SSoT_Dev_Mode.md` §8.3.

---

## 2026-04-14 — M1.2 closure: Status icon pipeline SO migration + refactor

### Semantic changes

**SSoT_Status_Effects:**
- §2.2 updated: "authored/theme layer" now explicitly includes icon sprite presentation.
- §3 runtime ownership expanded to include icon presentation (sprite authority on StatusEffectSO, event-driven rendering on CharacterCanvas).
- §3.2 migration note updated: M1.2 icon pipeline closure recorded. Legacy icon calls removed from `MusicianBase.OnBreakdown` and `AudienceCharacterBase.IsBlocked` setter (Decision E3: blocked is sprite-tint only).
- §3.3 added: icon presentation authority specification. Sprite authority on `StatusEffectSO.IconSprite`. Rendering path documented end-to-end. Design decisions enumerated (no lookup table asset, direct prefab on canvas, lazy lifecycle, warning on missing sprite/prefab, tooltip content deferred to M1.3).
- §4 catalogue section: M1.2 catalogue validation fix documented (`delayCall` deferral + import-worker skip). M1.2 asset hygiene documented (auto-rename).
- §10 update rule: icon presentation authority added to the list of changes that require updating this SSoT.

**CURRENT_STATE:**
- §1 Project foundation: new "Status icon pipeline — SO-based" block added with M1.2 closure summary.
- §2 Active work: "Status Icons + Tooltip pipeline — audit needed" block removed (M1.2 closed). New "QA-readiness gap" block added describing the actual blocker (Dev Mode, not icons). New "Card effect description text — known bug" item captured.
- §3 What is next: reframed around critical path to QA-readiness. M1.5 Dev Mode, M1.7 hover highlight, M1.8 icon animations, M1.3 tooltips, M1.1 deck editor polish, M2 composition validation.
- §4 Residual risks: "StatusIconsData uses legacy enum" removed (fixed). "Multi-turn status validation pending" added (deferred to M1.5).
- §5 Pending docs: "Archive headers" removed (M1.6 already closed per prior changelog). "Dev Mode design SSoT" added as the next expected doc.

**Roadmap_ALWTTT:**
- Milestone 1 introduction revised: goal and demo pitch reframed around "testable by general-audience testers."
- New "Priority order" block added at top of M1 establishing critical-path reordering: M1.5 → M1.7 → M1.8 → M1.3 → M1.1. Reasoning recorded.
- M1.2 marked complete with outcome-beyond-scope notes (StatusIconsData removed, auto-rename, catalogue validation fix, Obsolete marker on legacy ApplyStatus).
- M1.3 scope expanded to explicitly include the card effect description text bug fix.
- M1.5 scope expanded: transparent audience reaction/ability display. Design questions added for session start. Proposal to start M1.5 with a detailed sub-roadmap session.
- M1.7 Character hover highlight added as new task (code-only, game feel).
- M1.8 Status icon animations added as new task (code-only, game feel).
- M1.6 marked complete (closed 2026-04-08; was already done but not marked).
- Definition of Done updated: M1.2 items checked. New items added for M1.7, M1.8, M1.3 text-fix, and M1.5 audience-reaction transparency.
- M2 demo pitch: added explicit "Unblocked by: M1.5 Dev Mode" note.

### Authority changes

**Icon pipeline authority clarified:**
- Sprite authority: `StatusEffectSO.IconSprite` (owned by SSoT_Status_Effects §3.3).
- Rendering path authority: `CharacterCanvas` event subscription to `StatusEffectContainer` (owned by SSoT_Status_Effects §3.3).
- The former `StatusIconsData` lookup asset is deleted. No longer an authority-bearing file.

### Structural changes

**Removed files:**
- `Assets/Scripts/Data/UI/StatusIconsData.cs` — the lookup table class is deleted. `StatusIconsData` and `StatusIconData` types no longer exist.
- `StatusIconsData` asset instance in the project — removed from musician/audience prefab references.

**Modified files (code):**
- `StatusEffectSO.cs` — added `iconSprite` field + `IconSprite` property under new `[Header("Presentation")]`. Added editor-only auto-rename via `EditorApplication.delayCall`. Rename format: `StatusEffect_{DisplayName}_{EffectId}`.
- `StatusEffectCatalogueSO.cs` — `OnValidate` now defers real validation to `delayCall` and skips entirely during `AssetDatabase.IsAssetImportWorkerProcess()`. Fixes spurious "empty StatusKey" errors on prefab selection.
- `StatusIconBase.cs` — `SetStatus` signature changed from `(StatusIconData)` to `(Sprite)`. `MyStatusIconData` property removed, replaced with `CurrentSprite`.
- `CharacterCanvas.cs` — `statusIconsData` field removed. `statusIconBasePrefab` direct field added under new `[Header("Status Icons")]`. `TryCreateIcon` reads sprite from `_boundContainer.TryGet(id).Definition.IconSprite`. Keyword-based tooltip iteration stripped; `ShowTooltipInfo()` is a stub pending M1.3. Public `ShowTooltipInfo`/`HideTooltipInfo` methods preserved for `ITooltipTargetBase` compliance, corrected to call `TooltipManager.ShowTooltip`/`HideTooltip` (the real API). `BindStatusContainer` subscribes to `OnStatusApplied`/`OnStatusChanged`/`OnStatusCleared`.
- `CharacterStats.cs` — icon delegate subscriptions in `Setup()` and `Dispose()` removed. Legacy status dict and turn triggers retained (they drive non-icon legacy behavior only).
- `MusicianBase.cs` — `OnBreakdown` no longer calls `stats.ApplyStatus(StatusType.Breakdown, 1)`. `BuildCharacter` calls `bandCharacterCanvas.BindStatusContainer(Statuses)`.
- `AudienceCharacterBase.cs` — `IsBlocked` setter no longer calls `stats.ApplyStatus/ClearStatus(StatusType.Blocked)`. Sprite tint is the only visual indicator (Decision E3). `BuildCharacter` calls `AudienceCharacterCanvas.BindStatusContainer(Statuses)`.
- `BandCharacterStats.cs` — `ApplyStatus(StatusType, int)` marked `[Obsolete]`. No behavior change.

### Lifecycle

- M1.2 (Status Icons pipeline migration) closed.
- M1.6 (Archive superseded planning docs) retroactively marked closed in the roadmap (was already done but unmarked).
- Multi-turn status smoke tests (T4, T5, T7, T8, T9 from the M1.2 test plan) deferred to M1.5 closure — they require infinite-turn tooling that does not yet exist.
- M1.5 elevated to critical-path priority. The build is technically complete but not QA-ready without it.
- Card effect description text bug (`CharacterStatusId` enum names showing instead of `StatusEffectSO.DisplayName`) added to M1.3 scope.

### Operational changes

- Project instructions gain a new "Smoke test requirement for gameplay changes" section (operational classification). Any batch that affects gameplay, runtime behavior, or visible player-facing state must include a bounded set of visual/gameplay smoke tests before closing. Each test specifies setup, action, expected observable result, and fail criterion. Regression tests required for any intentionally removed behavior. Tests that cannot be run through normal gameplay must be explicitly deferred with a named target (typically Dev Mode / M1.5).

---

## 2026-04-08 — SSoT_Editor_Authoring_Tools.md created and activated (M1.4)

### Authority changes

**New governed SSoT activated:**
- `systems/SSoT_Editor_Authoring_Tools.md` — promoted from **planned** to **active**.
- Covers four editor tools: Card Editor, Deck Editor, Status Effect Wizard, Chord Progression Catalogue Wizard.
- Documents supporting services: `CardAssetFactory`, `MusicianCatalogService`, `DeckJsonImportService`, `DeckCardCreationService`, `DeckValidationService`, `DeckAssetSaveService`.
- Documents composition classifier (`CompositionCardClassifier`) and descriptors (`PartActionDescriptor`, `TrackActionDescriptor`) as editor-relevant runtime utilities.
- Known gaps section maps to M1 roadmap tasks (M1.1–M1.3, M1.5).

**SSoT_INDEX updated:** Editor tools row changed from `**planned**` to `active`.

**coverage-matrix updated:** Editor authoring tools row changed from `(no governed doc yet)` / `**planned**` to `systems/SSoT_Editor_Authoring_Tools.md` / `active`.

**CURRENT_STATE updated:** §1 documentation line updated. §2 editor tooling documentation marked complete. §5 pending docs list updated.

### Scope boundary established
- `SSoT_Editor_Authoring_Tools.md` owns tool capabilities, workflows, and known gaps.
- `SSoT_Card_Authoring_Contracts.md` retains authority over data contracts, JSON schema, and effect-list representation.
- No overlap or duplication between the two documents.

### Lifecycle
- M1.4 (Editor tooling documentation) can be marked complete in `Roadmap_ALWTTT.md`.

---

## 2026-04-08 — Project scope broadened from Combat MVP to full ALWTTT game project

### Authority changes

**Roadmap authority replaced:**
- `planning/active/Roadmap_Combat_MVP.md` → archived to `planning/archive/`. Superseded by `planning/active/Roadmap_ALWTTT.md`.
- `Roadmap_Combat_MVP_Closure_Actionable.md` → archived to `planning/archive/`. All phases complete; historical record only.
- `ALWTTT_DeckEditorWindow_Roadmap_Proposal.md` → archived to `planning/archive/`. Phases 0–6 substantially complete; remaining polish items captured in `Roadmap_ALWTTT.md` M1.1.
- New active roadmap: `planning/active/Roadmap_ALWTTT.md` — project-wide milestone-based roadmap with DoD checklists and demo-readiness checks.

**Coverage matrix updated:**
- Roadmap row now points to `Roadmap_ALWTTT.md`.
- New row added: "Editor authoring tools" — planned primary home `systems/SSoT_Editor_Authoring_Tools.md`.

**SSoT_INDEX updated:**
- Planning docs section restructured into "Active planning docs" and "Archived planning docs" with supersession pointers.
- Transitional compatibility path for `planning/combat/` removed (no longer needed).
- `SSoT_Editor_Authoring_Tools.md` registered as planned.

### Operational changes

**CURRENT_STATE reframed:**
- §1 changed from "Combat MVP closed" single-slice framing to "Project foundation" covering combat baseline, composition surface, editor tools, and documentation state.
- §2 changed from Phase 4 completion record to active work: Deck Editor polish, Status Icons/Tooltips audit, editor tooling documentation.
- §3 changed from "Post-MVP work (not blocking closure)" to forward-looking "What is next": Dev Mode scene, composition validation.
- §4 updated with new risk: StatusIconsData uses legacy StatusType enum, disconnected from all Combat MVP statuses.
- New open item added: true card copies in decks (current runtime deduplicates by reference).

### New findings recorded

- `StatusIconsData` and `StatusIconData` are keyed on legacy `StatusType` enum. None of the six Combat MVP statuses exist in that enum. Icon pipeline is disconnected from working status effects.
- Card tooltip system is keyword-based only (`CardDefinition.Keywords` → `TooltipManager`). No connection to card effects or status effects.
- Deck Editor phases 0–6 from the original roadmap proposal are substantially implemented. Remaining work: better filters, card preview info, cross-tool integration.

### Lifecycle decisions
- Combat MVP roadmaps archived as completed historical records.
- DeckEditorWindow Roadmap Proposal archived as substantially complete; remaining items absorbed into new roadmap M1.
- Claude Project scope broadened from Combat MVP focus to full ALWTTT game project.
- Project instructions, name, and description updated to reflect full game scope.

---

## 2026-03-23 — Combat MVP Phase 4 closure

### Semantic changes

**SSoT_Gig_Combat_Core:**
- Composure reset timing corrected: clears at each `PlayerTurnStart` tick, not per-song. These are not equivalent.
- Breakdown consequences corrected and expanded: Cohesion−1 → LoseGig if Cohesion≤0 → Shaken application → Stress reset. Ordering and short-circuit now documented.
- Stress reset formula corrected: `floor(StressMax * breakdownStressResetFraction)` (default 0.5, configurable). Previous doc said `ceil(StressMax / 2)`.
- Shaken duration corrected: expires at start of Audience Turn N+1 (AudienceTurnStart tick), not "until end of next Song" as previously stated. Active through one full song cycle from application.
- Shaken restrictions (Action card block, Composure penalty) reclassified as design intent — not enforced in runtime. Explicitly noted.
- Added §6.4 Exposed: each stack adds 0.25 to incoming stress multiplier on musicians. No audience path.
- Added §6.5 Feedback DoT: musician-only in AudienceTurnRoutine. Audience version explicitly deferred.
- §11 replaced: stale validation gaps removed, implementation status table added.

**SSoT_Status_Effects:**
- §3 expanded with tick timing system documentation: PlayerTurnStart=8, AudienceTurnStart=9, StartOfTurn=1 (legacy).
- §3.2 added: dual status system documented. Legacy StatusType path and current SO+container path both exist. New work goes through SO+container only.
- §5 rewritten: abstract placeholders replaced with concrete canonical MVP set. All six statuses now specified with primitives, keys, SO config, tick timing, combat meaning, and validation status.
- Shaken (§5.4) added as new canonical status. AudienceTurnStart tick. Restrictions pending.
- Exposed (§5.5) added as new canonical status.
- Feedback (§5.6) added as new canonical status. Musician-only, audience deferred.

**SSoT_Card_System:**
- DrawCardsSpec confirmed implemented and validated (Phase 2). Added to built-ins table.
- Performer rule (§8.1) clarified: performer = card owner via FixedPerformerType; Self = card owner. Validated in Fix 3.7a.
- Built-ins section (§6.2) restructured as a table with implementation/validation status.

**SSoT_Gig_Encounter:**
- §7.3 Failure rule: added implementation note. Method is `GigManager.LoseGig()` (public). Called from `MusicianBase.OnBreakdown`. No method named `TriggerGigLoss` exists.

**SSoT_Runtime_Flow:**
- §4 expanded with CompositionSession bypass: `ExecuteGigPhase()` returns early when `_session != null`. Documented as deliberate decoupling, not a bug. Added as runtime invariant #7.

**CURRENT_STATE:**
- §1 updated to reflect Combat MVP closed.
- §2 replaced with Phase 4 completion record (all decisions A–H).
- §3 replaced with post-MVP work ordered by value. Composition session testing listed as highest priority.
- §4 updated with current non-blocking open items.

### Authority changes
None. All existing authority assignments are unchanged.

### Lifecycle decisions
- Audience Feedback DoT: explicitly deferred. Requires Stress path on AudienceCharacterBase.
- Shaken restrictions: design decision deferred. Status applies and expires correctly; restrictions are a follow-up pass.
- Shaken SO Tick Timing changed from PlayerTurnStart → AudienceTurnStart. Duration is now one full song cycle, not one player turn.

---

## 2026-03-19 — Governance migration Batch 06 normalized the final tree and closed the snapshot migration

### Added
- `planning/active/README.md`
- `planning/archive/README.md`
- `planning/active/Roadmap_Combat_MVP.md`
- `planning/combat/README.md`
- `reference/README.md` (restored in the normalized tree)
- `reference/CSO_Primitives_Catalog.md` (restored in the normalized tree)
- `archive/snapshots/README.md`
- `archive/SNAPSHOT_RETENTION_POLICY.md`

### Restored / aligned
- `systems/SSoT_Audience_and_Reactions.md`
- `systems/SSoT_Status_Effects.md`
- `systems/SSoT_Scoring_and_Meters.md`

### Modified
- `README.md`
- `SSoT_INDEX.md`
- `CURRENT_STATE.md`
- `coverage-matrix.md`
- `archive/README.md`
- `archive/absorbed/Source_Docs_Supersession_Map.md`

### Key documentary decisions
- the actual tree was brought back into alignment with the root governance docs
- the active roadmap home is now explicitly `planning/active/`
- the previous combat-roadmap path is now only a compatibility pointer

---

## 2026-03-19 — Governance migration Batch 05 promoted encounter structure and cleanup traceability

### Added
- `systems/SSoT_Gig_Encounter.md`
- `archive/README.md`
- `archive/absorbed/README.md`
- `archive/absorbed/Source_Docs_Supersession_Map.md`

### Promoted / reclassified
- previous encounter-level truth from `reference/Gig.md` was promoted into `systems/SSoT_Gig_Encounter.md`
- the old source-doc set is now explicitly treated as snapshot/trace material rather than a silent second docs tree
- a durable supersession map now records where pre-governance doc names point in the governed system

### Key documentary decisions
- encounter structure is now separated from both combat economy and runtime execution
- `Gig` now has a governed home for:
  - roster framing
  - song-count structure
  - gig-scoped state
  - victory/failure conditions
  - encounter modifiers
- cleanup/redirect handling was moved into `archive/absorbed/` instead of leaving old names implied only through chat history
- the current governed docs tree can now answer "where did this old doc go?" without re-opening the snapshot manually

### Migration impact
- ALWTTT no longer has a missing primary home for encounter-level truth
- the remaining migration work became normalization/final replacement rather than creation of major new subsystem authorities

## 2026-03-19 — Governance migration Batch 04 promoted audience, status, and scoring authority

### Added
- `systems/SSoT_Audience_and_Reactions.md`
- `systems/SSoT_Status_Effects.md`
- `systems/SSoT_Scoring_and_Meters.md`
- `reference/README.md`
- `reference/CSO_Primitives_Catalog.md`

### Promoted / reclassified
- previous audience truth from `reference/AudienceMember.md` was promoted into `systems/SSoT_Audience_and_Reactions.md`
- previous status-runtime truth from `reference/StatusEffects.md` was promoted into `systems/SSoT_Status_Effects.md`
- previous scoring semantics from `backlog/ideas/loopscore_songhype_vibe.md` were promoted into `systems/SSoT_Scoring_and_Meters.md`
- the broader CSO primitive catalog was retained as reference rather than as primary runtime authority

### Key documentary decisions
- audience, status, and scoring now have separate primary homes
- persuasion progress was kept distinct from song momentum
- status ontology/reference material was separated from live runtime status truth
- the reference catalog was allowed to stay broad without competing with the live status SSoT

### Migration impact
- ALWTTT no longer needed reference/backlog docs as silent authority for audience/status/scoring
- the governed tree gained complete gameplay-facing subsystem coverage

## 2026-03-19 — Hardening micro-pass for ALWTTT ↔ MidiGenPlay boundary

### Modified
- `runtime/SSoT_Runtime_CompositionSession_Integration.md`
- `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md`
- `integrations/midigenplay/README.md`

### Added
- `integrations/midigenplay/ALWTTT_Uses_MidiGenPlay_Quick_Path.md`

### Key hardening decisions
- made the **source-of-truth split** explicit:
  - ALWTTT owns editable/session truth before handoff
  - MidiGenPlay owns package-side runtime song truth after handoff
- clarified that these are complementary layers rather than competing authorities
- added a one-page quick-path guide so a developer can understand the end-to-end integration flow without reading multiple long docs first

## 2026-03-19 — Governance migration Batch 03 promoted runtime and music-integration authority

### Added
- `runtime/README.md`
- `runtime/SSoT_Runtime_Flow.md`
- `runtime/SSoT_Runtime_CompositionSession_Integration.md`
- `integrations/README.md`
- `integrations/midigenplay/README.md`
- `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md`
- `integrations/midigenplay/SSoT_ALWTTT_MidiMusicManager_Integration.md`

### Promoted / reclassified
- previous gig/runtime-flow truth was consolidated into `runtime/SSoT_Runtime_Flow.md`
- previous ALWTTT-owned runtime bridge truth was consolidated into `runtime/SSoT_Runtime_CompositionSession_Integration.md`
- previous mixed composition-pipeline docs were formally split into:
  - ALWTTT runtime truth
  - ALWTTT ↔ MidiGenPlay boundary truth
  - package-owned material that must be referenced rather than duplicated

### Key documentary decisions
- `GigManager`, `CompositionSession`, `SongConfigBuilder`, and loop/part/song feedback were promoted as ALWTTT runtime authority surface
- `MidiMusicManager` now has an explicit governed home as ALWTTT runtime/integration truth
- composition-card runtime meaning was separated from package-internal composer/generation details
- the ALWTTT ↔ MidiGenPlay ownership split is now explicit rather than implied by mixed docs

### Migration impact
- ALWTTT no longer depends on mixed composition pipeline docs as silent primary authority
- the next migration can focus on audience/status/scoring without reopening the ALWTTT vs MidiGenPlay boundary question

## 2026-03-18 — Governance migration Batch 02 promoted first subsystem SSoTs

### Added
- `systems/README.md`
- `systems/SSoT_Gig_Combat_Core.md`
- `systems/SSoT_Card_System.md`
- `systems/SSoT_Card_Authoring_Contracts.md`
- `planning/README.md`
- `planning/combat/Combat_MVP_Roadmap.md`

### Promoted / reclassified
- previous combat truth was consolidated into `systems/SSoT_Gig_Combat_Core.md`
- previous card truth was consolidated into `systems/SSoT_Card_System.md`
- previous appendix/data-contract truth was promoted into `systems/SSoT_Card_Authoring_Contracts.md`
- previous combat roadmap was reclassified as planning-only

### Key documentary decisions
- combat economy/phase/resource truth now has a single governed home
- card gameplay semantics now has a single governed home
- authoring/import contracts were split cleanly from gameplay/runtime semantics
- the effect-first card model is treated as the current primary card model
- legacy `CardData`-style material remains a documented risk, not silent authority

### Migration impact
- ALWTTT now has its first real subsystem SSoTs instead of only root governance docs
- the next migration can focus on runtime/music integration without reopening combat/card authority

## 2026-03-18 — Governance migration Batch 01 initialized

### Added
- root governed docs spine:
  - `README.md`
  - `SSoT_INDEX.md`
  - `SSoT_CONTRACTS.md`
  - `CURRENT_STATE.md`
  - `coverage-matrix.md`
  - `changelog-ssot.md`

### Established documentary rules
- one major concept must have one primary home
- planning/reference/archive cannot silently override SSoTs
- `CURRENT_STATE.md` is a live state layer, not a replacement for subsystem SSoTs

### Established cross-project boundary rule
- ALWTTT owns gameplay/runtime/integration truth
- MidiGenPlay owns package internals
- shared concepts must be split into boundary contracts rather than duplicated authority

### Key classification decision
- `MidiMusicManager` is to be documented as **ALWTTT runtime/integration truth**, not as MidiGenPlay package truth

### Migration impact
- future subsystem batches now have a stable governed target
- current previous docs remain source material until subsystem SSoTs are promoted

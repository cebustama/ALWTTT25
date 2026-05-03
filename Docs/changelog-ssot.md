# changelog-ssot — ALWTTT

This changelog records **semantic/documentary changes**.
Cosmetic edits should not be logged here.

2026-05-03 — M4.6-prep batch (3) closure: Authoring tooling QoL
Editor-only batch promoting authoring ergonomics surfaced during M4.6-prep batch (2) smoke tests. Three deliverables shipped: per-row Starter / Copies columns on CardEditorWindow's catalog entry list; new CardInventoryWindow (read-only viewer for CardDefinition / MusicianCardCatalogData / GenericCardCatalogSO assets, with Print + Export per view); toolbar Print buttons on CardEditorWindow and DeckEditorWindow. All #if UNITY_EDITOR guarded. Zero runtime touch.
Semantic changes
SSoT_Editor_Authoring_Tools:

§3 tool inventory table — new row for Card Inventory (CardInventoryWindow, menu path ALWTTT → Cards → Card Inventory, primary purpose: read-only browser for card-related assets with Print + Export per view). Header sentence updated from "All four are #if UNITY_EDITOR gated" to "All five are #if UNITY_EDITOR gated".
§4 Card Editor — new §4.6 "Per-row Starter / Copies columns (batch (3), 2026-05-03)" documenting the inline checkbox + IntField columns, the SerializedObject + ApplyModifiedProperties commit path, the Mathf.Max(1, …) clamp, the IMGUI input-isolation property that prevents inline controls from stealing row selection, the silent-disappearance behavior under the Starter filter, and the single-step Undo guarantee. New §4.7 "Print button (batch (3), 2026-05-03)" documenting the toolbar Print button, its disabled-when-no-catalog-loaded behavior, and the === CARD EDITOR — CATALOG DUMP === output shape. Existing Registries surface section renumbered to §4.8 (no content change).
§5 Deck Editor — §5.2 layout block updated to mention the toolbar Print button. §5.4 deck contract updated to reflect M4.4 multiplicity (was previously phrased as "unique card lists (Option A)"; now correctly notes multiset shape with per-entry count). §5.6 DTOs section updated to document StagedCardEntry's existingCard / pendingCard / count / ResolvedCard shape. New §5.7 "Print button (batch (3), 2026-05-03)" documenting the toolbar Print button and the === DECK EDITOR — STAGED DECK DUMP === output shape, including the use of ResolvedCard to handle both existing and pending entries uniformly and the [NEW] suffix + ×{count} per-row format.
New §8 "Card Inventory Window (CardInventoryWindow) — batch (3), 2026-05-03" inserted between Chord Progression Catalogue Wizard and Card asset factory. Documents what the tool does (read-only viewer; does not mutate assets), the four-view layout (All CardDefinitions / All Musician Catalogs / One Musician / All Generic Catalogs), Print to Console output shapes per view, Export JSON schemas per view (informational, not designed for round-trip through DeckJsonImportService), asset discovery via AssetDatabase.FindAssets("t:{TypeName}"), and the boundary note that this is a viewer only — Card Editor and Deck Editor remain the authority for editing.
Sections previously numbered §6 / §7 / §8 / §9 / §10 / §11 / §12 / §13 / §14 renumbered to §6 / §7 / §9 / §10 / §11 / §12 / §13 / §14 / §15 to accommodate the new §8 insertion. (§6 Status Effect Wizard, §7 Chord Progression Catalogue Wizard unchanged; §9 Card asset factory was §8; §10 Musician catalog service was §9, content unchanged but a sentence appended noting that the per-row UX from §4.6 does not call into this service because it mutates existing entries via SerializedObject directly; §11–§15 renumbered without content change beyond the cross-reference within.)
§13 File location summary — CardInventoryWindow.cs added to the Assets/Scripts/Cards/Editor/ listing with a (batch (3), 2026-05-03) annotation.
§14.1, §14.2 Known gaps — pre-existing items annotated as RESOLVED with closure dates (M1.2 / M1.3a/b/c respectively); no semantic change, just clarity.
§14.5 Known gaps — new entry "Inventory viewer two-prefab arrangement (logged 2026-05-02 from UI-fix-A; appendix to batch (3) deferred 2026-05-03)" inserted to record the explicit D3 deferral of the prefab-variant validator appendix and to log it as a candidate authoring-tool addition for a future QoL pass. References the CURRENT_STATE.md §4 CardUI : CardBase {} empty-subclass two-prefab vector and the (α) collapse / (β) Prefab Variant cleanup options.
§14.7 Known gaps — pre-existing "True card copies in decks" item annotated as RESOLVED (M4.4); no semantic change.

CURRENT_STATE:

§1 new closure block "M4.6-prep batch (3) — Authoring tooling QoL — complete (2026-05-03)" inserted after the M4.6-prep UI-fix-B block. Documents all three deliverables (3.A per-row UI / 3.B CardInventoryWindow / 3.C Print buttons), the decision matrix resolutions D1–D6, the smoke-test results (ST-AT3-1..8 all PASS with per-test outcome lines), and the critical scope honesty note distinguishing tooling resolved from content cleanup pending.
§1 Editor authoring tools list updated: Card Editor and Deck Editor entries gain "per-row Starter / Copies columns + toolbar Print button (batch (3), 2026-05-03)" and "toolbar Print button (batch (3), 2026-05-03)" annotations respectively; new bullet for Card Inventory added.
§3 What is next, line 1 (the M4.6 entry): updated to record batch (3) closure in the parenthetical chain, and to rewrite the "Pre-demo blocker" sentence to note that the per-row Starter UI and CardInventoryWindow > All Musician Catalogs > Export JSON make the cleanup tractable while explicitly flagging that the cleanup itself still needs execution and re-verification against Design_Starter_Deck_v1.md §4. Pre-batch-(3) inv2.json snapshot referenced as the before-state baseline.
§4 Open items: "Card Editor per-row starter UX (queued as batch (3), surfaced 2026-05-02)" bullet flipped to RESOLVED with closure pointer to §1; "all-starter-flagged catalog content currently in test data" bullet rewritten to distinguish tooling resolved 2026-05-03 via batch (3) from content cleanup pending with explicit pre-batch state captured (Cantante 7/7, Conito 10/10, Gusano 6/6, Robot 5/5 — 28 starter entries total per inv2.json snapshot) and a verification path noted (re-export → diff against Design_Starter_Deck_v1.md §4); "CardUI : CardBase {} empty subclass" bullet updated to record the explicit D3 deferral of the prefab-variant validator appendix (logged in SSoT_Editor_Authoring_Tools.md §14.5).
§5 pending doc edits: new entry "M4.6-prep batch (3) closure (applied 2026-05-03)" added with the full file-change inventory and reasoning for files left unchanged.

Roadmap_ALWTTT:

Header Last updated line bumped to 2026-05-03 (M4.6-prep batch (3) Authoring tooling QoL closed).
Future Milestones: existing Authoring tooling QoL (batch (3) — queued post-M4.6-prep batch (2), pre-merged-(1)/(4)) entry rewritten as Authoring tooling QoL (batch (3)) ✅ (closed 2026-05-03) with full closure block: shipped scope (3.A/3.B/3.C), decision matrix resolutions (D1–D6 including the explicit D3 deferral), smoke-test summary table (ST-AT3-1..8 all PASS), critical scope honesty paragraph, and docs-at-closure inventory.

Authority changes
None. No SSoT promoted, retired, or moved. CardInventoryWindow is operational tooling, not a contract owner. SSoT_Editor_Authoring_Tools.md remains the single authority for editor-tool inventory and capabilities; the new §8 is a normal additive section under that authority.
Operational changes
New runtime code files:

Assets/Scripts/Cards/Editor/CardInventoryWindow.cs — single-file editor window, #if UNITY_EDITOR guarded, ALWTTT.Cards.Editor namespace. ~220 lines including JSON wrap structs and the four view-rendering methods. Asset discovery via AssetDatabase.FindAssets("t:{TypeName}") per render frame (no caching; acceptable for editor-only tool with low-tens-of-assets project scale). Export uses JsonUtility.ToJson(_, prettyPrint: true) + EditorUtility.RevealInFinder. The BuildFullCatalog helper is shared between Single Musician and All Generic Catalogs export paths (both consume List<MusicianCardEntry>).

Modified runtime code files:

Assets/Scripts/Cards/Editor/CardEditorWindow.cs — three changes:

Entry-list rendering loop (formerly lines 559–587) replaced with a horizontal-scope per-row layout containing the Starter checkbox + Copies IntField + selection button. Uses SerializedObject(_loadedCatalog) constructed once per OnGUI pass with entriesArrSp = catSo.FindProperty("entries"); per-row writes via entriesArrSp.GetArrayElementAtIndex(i).FindPropertyRelative("flags" / "starterCopies"); single catSo.ApplyModifiedProperties() after the loop. Defensive fallback (disabled placeholder controls) when the catalog is null.
Toolbar Print button inserted in DrawToolbar after the Registries Ping button, with GUILayout.Space(10) separator and EditorGUI.DisabledScope(_loadedCatalog == null) gating.
PrintLoadedCatalog() private method added between the entry-list render block and PassesFilters. Uses System.Text.StringBuilder (already in usings).


Assets/Scripts/Cards/Editor/DeckEditorWindow.cs — two changes:

Toolbar Print button inserted between Export JSON (line 193) and Clear All (line 196). Unguarded (Print is useful even on empty staged decks for "why is my deck empty" diagnostics).
PrintStagedDeck() private method added between DoExportJson (ends line 854) and DoAddToGigSetup (starts line 856). Uses StagedCardEntry.ResolvedCard to handle both existing and pending entries; reports per-row count for M4.4 multiplicity; emits [NEW] suffix on pending entries.



No prefab edits. No [SerializeField] field additions. No data-asset edits.
Smoke test results
IDDescriptionResultST-AT3-1Per-row Starter toggle commits to asset, persists across reloadPASSST-AT3-2Copies field disable + clamp to 1 on commitPASSST-AT3-3Filter interaction silent disappearancePASSST-AT3-4Undo reverts both flag and copies as one stepPASSST-AT3-5CardInventoryWindow all four views populate, Print + Export succeedPASS (inv1.json/inv2.json/inv3.json/inv4.json exports verified)ST-AT3-6Print buttons on both windows produce formatted multi-line outputPASSST-AT3-7Regression: per-row controls do not steal selectionPASSST-AT3-8Dogfood acceptance: cleanup workflow materially faster than right-side inspectorPASS ("very good cleanup process")
Side-findings

D3 prefab-variant validator deferral (logged for future QoL pass). A "Validate CardBase prefab variants" Card Editor action was considered as a candidate appendix to batch (3) and explicitly deferred. Such an action would reflect over [SerializeField] fields on CardBase and report unwired refs across all prefabs that carry a CardBase (or subclass) MonoBehaviour, catching the UI-fix-A NRE class of bug at authoring time. Logged in SSoT_Editor_Authoring_Tools.md §14.5 and CURRENT_STATE.md §4 (under the CardUI : CardBase {} empty-subclass bullet).
Pre-batch-(3) inventory snapshot captured. CardInventoryWindow > All Musician Catalogs > Export JSON ran during ST-AT3-5 produced inv2.json as a clean before-state baseline for the all-starter-flagged content. Confirmed values: Cantante 7/7 starter, Conito 10/10 starter, Gusano 6/6 starter, Robot 5/5 starter — 28 entries total. This baseline is the diff anchor for verifying the actual content cleanup against Design_Starter_Deck_v1.md §4 once it is executed.
Critical scope honesty. Batch (3) closure ships the tooling. The content cleanup itself (pruning the four catalogues to the design spec) is a separate task that may have been partially executed during ST-AT3-8 dogfood but is not asserted by the smoke test set. The pre-demo blocker tracked in CURRENT_STATE.md §4 is now structurally tractable but content-status undetermined.

Files unchanged (with reasoning)

All systems SSoTs (SSoT_Card_System.md, SSoT_Card_Authoring_Contracts.md, SSoT_Status_Effects.md, SSoT_Gig_Combat_Core.md, SSoT_Audience_and_Reactions.md, SSoT_Scoring_and_Meters.md, SSoT_Gig_Encounter.md, SSoT_Runtime_Flow.md, SSoT_Runtime_Song_Model_and_Config.md, SSoT_Runtime_Generation_Orchestration.md, SSoT_Runtime_CompositionSession_Integration.md, SSoT_ALWTTT_MidiGenPlay_Boundary.md, SSoT_Composer_Backing_Track.md, SSoT_Composer_Rhythm_Track.md, SSoT_Composer_Melody_Track.md, SSoT_Dev_Mode.md) — batch (3) is operational tooling only. No card semantics, scoring contracts, status semantics, runtime gameplay invariants, audience contracts, composer contracts, or boundary contracts changed. Asset semantics for MusicianCardCatalogData, GenericCardCatalogSO, CardDefinition, and BandDeckData are unchanged; only the editor surfaces operating on them.
coverage-matrix.md — no new authority concept; CardInventoryWindow is operational tooling under the existing SSoT_Editor_Authoring_Tools.md authority. No coverage matrix update needed.
SSoT_INDEX.md — no structural or authority change.
ssot_manifest.yaml — no new invariants. The existing manifest entries on SSoT_Editor_Authoring_Tools.md remain accurate; the batch added new sections under that authority but did not add new contract-shaped invariants.
SSoT_CONTRACTS.md — no contract change.

Lifecycle
M4.6-prep open queue updated:

Inventory viewer NRE [✅ closed 2026-05-02, UI-fix-A]
Inventory scrollbar fix [✅ closed 2026-05-02, UI-fix-B]
Batch (3) Authoring tooling QoL [✅ closed 2026-05-03]
Merged (1)/(4) Gig Setup roster pickers [next]
Batch (5) Runtime tuning [queued, awaiting tuning values]
M4.6 demo gate

Critical-path next step on M4.6: merged (1)/(4) Gig Setup roster pickers (band + audience multi-select) is now the front of the queue, with batch (5) Runtime tuning blocked on values from user (maxVibeFromSongHype, MaxCardsOnHand, draw-per-turn). Pre-demo content cleanup of the four catalogues to Design_Starter_Deck_v1.md §4 spec is independently tractable now that batch (3) tooling exists; recommended to execute and re-verify before the demo gate.

2026-05-02 — M4.6-prep UI-fix-A + UI-fix-B joint closure: Inventory viewer NRE + Inventory scrollbar functional
Two UI-fix batches closed jointly the same day, both surfaced during M4.6-prep batch (2) smoke tests (2026-05-02). Both pre-existing bugs, both player-facing, both demo-relevant for M4.6. Combined entry because they share a closure pass and overlapping doc updates.
Semantic changes
None. Neither batch touched governed SSoTs, contracts, or authority. UI-fix-A is asset-wiring on CardUI.prefab; UI-fix-B is asset-wiring on InventoryCanvas.prefab plus a localized helper edit on InventoryCanvas.cs (population-time ScrollRect reset). No card semantics, scoring contracts, status semantics, or runtime gameplay invariants changed.
CURRENT_STATE:

§1 two new closure blocks inserted after the M4.6-prep batch (2) block: "M4.6-prep UI-fix-A — Inventory viewer prefab NRE — complete (2026-05-02)" and "M4.6-prep UI-fix-B — Inventory scrollbar functional — complete (2026-05-02)". UI-fix-A documents the two unwired TMP refs (inspirationCostTextField, inspirationGenTextField) on CardUI.prefab (where CardUI : CardBase {} is an empty subclass), the asset-only fix, the strict-SetCard decision, ST-INV-1..6 PASS, and the structural risk parking pointer. UI-fix-B documents the layered root cause (Content has ContentSizeFitter but no LayoutGroup → preferred height 0; Viewport has Mask + disabled Image → broken masking), the full asset-edit set (VerticalLayoutGroup on Content; LayoutElement on FilterPanel/CardSpawnRoot/SongSpawnRoot; RectMask2D replaces Mask+disabled Image; CardSpawnRoot padding trim), the code-edit shape (scrollRect field + Canvas.ForceUpdateCanvases() + LayoutRebuilder.ForceRebuildLayoutImmediate + verticalNormalizedPosition=1f reset block in SetCards and SetSongs), the rationale for LayoutElement.preferredHeight over auto-sizing (GridLayoutGroup on stretch-anchored RectTransform inside ContentSizeFitter doesn't propagate preferred height reliably), and the smoke-test results (ST-SCR-1/3/4/6/7 PASS, ST-SCR-2 FAIL ACCEPTED, ST-SCR-5 DEFERRED).
§4 Open items: inventory NRE bullet flipped from open → RESOLVED with closure pointer to §1 UI-fix-A block. Three new park-lot bullets added: (a) CardUI : CardBase {} empty-subclass two-prefab arrangement is the recurrence vector for unwired-[SerializeField] bugs; cleanup options (α) collapse to one prefab with view-only mode and (β) CardUI.prefab as Prefab Variant logged; candidate appendix to batch (3) for a "Validate CardBase prefab variants" Card Editor action; (b) inventory scrollbar appears even with near-empty piles (ST-SCR-2 FAIL ACCEPTED) due to fixed LayoutElement.preferredHeight = 2050; cosmetic paper cut, follow-up via dynamic height computation in InventoryCanvas.SetCards (~10 lines from grid.cellSize.y, grid.spacing.y, grid.padding, grid.constraintCount × active card count); not blocking M4.6 demo; (c) FilterPanel scrolls with content (decision D-A from UI-fix-B planning, deferred); FilterPanel currently only contains TitleText so harmless; revisit when filters become functional by moving FilterPanel out of Content.
§5 pending doc edits: new entry "M4.6-prep UI-fix-A + UI-fix-B joint closure (applied 2026-05-02)" added with full file-change inventory and reasoning for files left unchanged.

Roadmap_ALWTTT:

Header Last updated line bumped to 2026-05-02 (M4.6-prep UI-fix-A + UI-fix-B closed).
Future Milestones section: existing Inventory viewer prefab fix (UI-fix batch — queued, pre-M4.6 demo) entry replaced by Inventory viewer fixes (UI-fix-A + UI-fix-B) ✅ (closed 2026-05-02) — combines both batches with their root causes, fix shapes, and smoke-test outcomes. The Authoring tooling QoL (batch (3)) entry is unchanged.

Authority changes
None. No SSoT promoted, retired, or moved. UI-asset wiring and InventoryCanvas.cs helper additions do not constitute contract changes.
Operational changes
Modified runtime code files:

InventoryCanvas.cs — added using UnityEngine.UI; (already present is enough; this batch adds the LayoutRebuilder consumer). Added [Header("Scroll")] + [SerializeField] private ScrollRect scrollRect; field. Both SetCards and SetSongs append a null-guarded reset block at the end (after the existing population loops):

csharp  if (scrollRect != null)
  {
      Canvas.ForceUpdateCanvases();
      LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
      scrollRect.verticalNormalizedPosition = 1f;
  }
The ForceUpdateCanvases + ForceRebuildLayoutImmediate pair guards against the timing race where verticalNormalizedPosition samples stale Content bounds before the layout pass runs.
Modified prefab assets:

CardUI.prefab (UI-fix-A) — wired the previously-unassigned inspirationCostTextField and inspirationGenTextField [SerializeField] refs on the Card UI (Script) component to the corresponding TMP_Text children. No structural change.
InventoryCanvas.prefab (UI-fix-B) — multiple component additions/replacements:

Content: added VerticalLayoutGroup (Padding 0/Spacing 0/Child Alignment Upper Center/Control Child Size Width=ON Height=OFF/Force Expand Width=ON Height=OFF). Existing ContentSizeFitter (Vertical Fit = Preferred Size) retained.
FilterPanel: added LayoutElement (Min Height=100, Preferred Height=100). Declares its height to the layout system so VerticalLayoutGroup respects it correctly.
CardSpawnRoot: added LayoutElement (Preferred Height=2050). Required because GridLayoutGroup on a stretch-anchored RectTransform inside ContentSizeFitter does not reliably propagate preferred height to its parent layout system. Grid Layout Group Padding Top reduced 150→50 (cosmetic, removes dead space above row 1 now that FilterPanel sits above CardSpawnRoot via VerticalLayoutGroup).
SongSpawnRoot: added LayoutElement (Preferred Height=800). Defensive parity with CardSpawnRoot for when songs become reachable.
Viewport: removed disabled Image and Mask components (Mask required an enabled Graphic which it didn't have, producing the editor warning "Masking disabled due to Graphic component being disabled"). Added RectMask2D — masks rectangularly without requiring a Graphic, lower per-frame cost, canonical Unity ScrollView pattern.
Scroll View: ScrollRect's new Vertical Scrollbar slot wired to existing Scrollbar Vertical GameObject. Visibility set to AutoHideAndExpandViewport (already correctly configured before batch). The new scrollRect field on InventoryCanvas (Script) component is wired to this Scroll View GameObject.



Smoke test results
UI-fix-A — ST-INV-1..6:
IDDescriptionResultST-INV-1Hand pile inventory open — sprite/name/desc/type/cost/gen all render correctly, no NREPASSST-INV-2Draw pile inventory open — samePASS (verified with 16-card draw pile)ST-INV-3Discard pile inventory open — samePASS (verified with mixed Action+Composition discard pile)ST-INV-4Close + reopen — cards re-render cleanly, no NRE on second open, no duplicatesPASSST-INV-5Mixed Action + Composition display — Action shows CardType enum string, Composition shows literal COMPOSITIONPASS (Discard Pile screenshot showed "Test: Flow +2" cards with CHR tag and "Waltz" card with COMPOSITION tag)ST-INV-6Regression: gameplay card display unchangedPASS (gameplay hand renders identically to pre-batch)
UI-fix-B — ST-SCR-1..7:
IDDescriptionResultST-SCR-1Overflow Draw Pile shows scrollbar, content scrolls smoothly without snap-backPASSST-SCR-2Underflow Discard Pile (1–3 cards) hides scrollbarFAIL ACCEPTED — vacuous overflow due to fixed CardSpawnRoot.LayoutElement.preferredHeight = 2050. Cosmetic. Follow-up via dynamic-height computation logged in CURRENT_STATE.md §4ST-SCR-3Mouse wheel scrolls vertical contentPASSST-SCR-4Reset to top on reopen — close inventory after scrolling, reopen → top of gridPASSST-SCR-5Songs inventory scrolls similarlyDEFERRED-by-construction — no Songs inventory content reachable in current buildST-SCR-6Regression: gameplay hand layout unchangedPASSST-SCR-7Masking holds during scroll — cards clip cleanly at Viewport edges (regression guard for RectMask2D fix)PASS
Side-findings

CardUI : CardBase {} empty subclass formalizes a two-prefab arrangement. Inventory canvas's cardUIPrefab field is typed CardBase but the assigned prefab carries a CardUI MonoBehaviour. The subclass adds zero behavior; its only role is to let inventory and gameplay use separate prefabs. Every [SerializeField] field added to CardBase going forward must be wired on both prefabs or the inventory side will NRE. Logged in CURRENT_STATE.md §4 with cleanup options (α) collapse and (β) Prefab Variant. Candidate appendix to batch (3): "Validate CardBase prefab variants" Card Editor action that reflects over [SerializeField] fields and reports unwired refs.
Vacuous-overflow paper cut. CardSpawnRoot.LayoutElement.preferredHeight = 2050 is a hard-coded value chosen to overflow Viewport for any reasonable deck size (12–20 cards). Under-fills the design intent for empty/near-empty piles where the scrollbar appears unnecessarily. Logged in CURRENT_STATE.md §4. Follow-up: replace fixed value with InventoryCanvas.SetCards runtime computation from grid params × active card count (~10 lines).
FilterPanel scrolls with content (decision D-A deferred). During UI-fix-B planning, the choice between sticky-header behavior (FilterPanel always visible) vs. scroll-with-content behavior (FilterPanel scrolls along with cards) was deferred because FilterPanel currently only contains TitleText (no functional filter chips). Logged in CURRENT_STATE.md §4. Revisit when filters become functional: move FilterPanel out of Content and make it a sibling of Scroll View under Midground for sticky behavior.

Files unchanged (with reasoning)

All systems SSoTs (SSoT_Card_System.md, SSoT_Card_Authoring_Contracts.md, SSoT_Editor_Authoring_Tools.md, SSoT_Status_Effects.md, SSoT_Gig_Combat_Core.md, SSoT_Audience_and_Reactions.md, SSoT_Scoring_and_Meters.md, SSoT_Gig_Encounter.md, SSoT_Runtime_Flow.md, SSoT_Runtime_Song_Model_and_Config.md, SSoT_Runtime_Generation_Orchestration.md, SSoT_Runtime_CompositionSession_Integration.md, SSoT_ALWTTT_MidiGenPlay_Boundary.md, SSoT_Composer_Backing_Track.md, SSoT_Composer_Rhythm_Track.md, SSoT_Composer_Melody_Track.md, SSoT_Dev_Mode.md) — neither batch touched any system contract, runtime invariant, or authoring contract. UI-asset wiring + a localized population-time helper edit on InventoryCanvas.cs. No subsystem semantics changed.
coverage-matrix.md — no new subsystem; no authority change.
SSoT_INDEX.md — no structural or authority change.
ssot_manifest.yaml — no new invariants. The existing manifest entries remain accurate.
SSoT_CONTRACTS.md — no contract change.

Lifecycle
Two M4.6-prep UI-fix batches closed jointly. M4.6-prep open queue updated: Inventory viewer NRE [✅] · Inventory scrollbar fix [✅] · Batch (3) Authoring tooling QoL [next] · Merged (1)/(4) Gig Setup roster pickers · Batch (5) Runtime tuning · M4.6 demo gate.

## 2026-05-02 — M4.6-prep batch (2) closure: Per-musician starter deck auto-assembly

### Semantic changes

**SSoT_Card_Authoring_Contracts:**
- New §5.11 "Per-musician starter deck auto-assembly (M4.6-prep batch (2))" added. Documents the runtime selection rule for both per-musician (`MusicianCardCatalogData` → `IsStarter` filter × `starterCopies` expansion) and generic (`GenericCardCatalogSO`) contributions; `starterCopies` semantics including editor-time clamps and runtime defensive warn-skip; the provenance contract (per-musician populates `musicianGrantedActionCards`/`musicianGrantedCompositionCards`, generic does not); the subtle case where the same `CardDefinition` lives in both catalog types and behaves correctly under `RemoveMusicianFromBand`; deck reset semantics; the relationship between `MusicianCharacterData.BaseActionCards`/`BaseCompositionCards` (transitional helpers, both paths read `CardCatalog` as single source of truth); the `useMusicianStartersToggle` selection mechanism between auto-assembly and the legacy `BandDeckData` path; and the `RunConfig.deckLabel` logging convention. The §5.10 (M4.4 deck multiplicity) section is unchanged; §5.11 is additive.

**CURRENT_STATE:**
- §1 new closure block "M4.6-prep batch (2) — Per-musician starter deck auto-assembly — complete (2026-05-02)" inserted after the M4.6-prep-A block. Documents the six decisions D1–D6 with the resolutions chosen, the provenance contract, the smoke-test results table (six PASS, one DEFERRED-by-construction), the queued batch (3) (Card Editor per-row starter UX), and the queued separate UI-fix batch (Draw Pile NRE).
- §3 line 183: M4.6 entry updated. The dependency phrase "+ per-musician starter decks" replaced with parenthetical closure marker referencing M4.6-prep batch (2). M4.6 dependencies now read "remaining §9 open questions + merged Gig Setup pickers (musicians + audience)" only. New explicit pre-demo blocker added: test catalogs currently have all entries flagged `StarterDeck` for tooling validation; must be cleaned up to the designed 12-card composition before demo.
- §4 Open items: per-musician starter decks bullet swapped from open → RESOLVED with closure pointer to §1 batch (2) block. Gig Setup roster pickers bullet retained but annotated 2026-05-02 with note that auto-assembly will pick up the picker batch's `MusicianList` mutation for free. Four new bullets added: (a) all-starter-flagged catalog content as M4.6 demo blocker; (b) `UnlockedByDefault` flag is editor-authoring-only with no runtime consumption today (defensive note for future readers); (c) inventory viewer NRE on Draw/Discard/Hand pile open (`CardBase.SetCard:77`, pre-existing prefab-wiring bug surfaced during smoke tests, promoted to roadmap as a fix batch); (d) Card Editor per-row starter UX queued as batch (3), promoted to roadmap as Future Milestone — Authoring tooling QoL.
- §5 pending doc edits: M4.6-prep batch (2) closure line added with the full file-change inventory and reasoning for files left unchanged.

**Roadmap_ALWTTT:**
- §4.4 line 371: `CardAcquisitionFlags.starterCopies` runtime consumption marked shipped as M4.6-prep batch (2). The pre-2026-05-02 wording "deferred to M4.6 when catalogue → starter-deck auto-assembly is implemented" replaced with a closure marker pointing to `PersistentGameplayData.SetBandDeckFromMusicians` and clarifying that `BandDeckEntry.count` remains the multiplicity carrier on the legacy `BandDeckData` asset path.
- §4.6 line 412: catalogue → starter-deck auto-assembly bullet marked shipped 2026-05-02. Now states that `CardAcquisitionFlags.starterCopies` is the per-card copy count for auto-assembled decks; the legacy `BandDeckData` path remains via the `useMusicianStartersToggle`; M4.6 demo will use the auto-assembly path.
- Future milestones: two new entries appended after Production & Polish. **Authoring tooling QoL (batch (3))** — editor-only, ~200 lines, scoped to per-row starter toggle column on `MusicianCardCatalogData` entries, new `CardInventoryWindow` for cross-asset listing, and "Print to console" toolbar buttons on Card Editor + Deck Editor. Slots after batch (2), before merged (1)/(4). **Inventory viewer prefab fix (UI-fix batch)** — pre-existing `CardBase.SetCard` NRE on inventory viewer open, surfaced during batch (2) smoke tests, must be fixed before next demo per Demo-readiness check; ~5-minute prefab fix once the wiring gap is identified.

**ssot_manifest:**
- `Docs/systems/SSoT_Card_Authoring_Contracts.md` entry: one new hard invariant added covering per-musician starter deck auto-assembly. Documents the `SetBandDeckFromMusicians` selection rule for both catalogue types, the provenance partition (per-musician populates `musicianGrantedActionCards`/`musicianGrantedCompositionCards`; generic does not), the runtime consumption of `CardAcquisitionFlags.starterCopies`, and the legacy/auto-assembly toggle via `RunConfig.useMusicianStarters`. The pre-existing M4.4 invariant on `BandDeckEntry.count` and additive duplicate combining is unchanged; the new invariant is additive.

### Authority changes

None. No SSoT promoted or retired. The auto-assembly contract finds its authoritative home in the existing `SSoT_Card_Authoring_Contracts.md` because it governs the data shape of how starter decks are derived from authored catalogs (consistent with §5.10's deck-multiplicity contract). Runtime mechanics of pile lifecycle remain in `SSoT_Card_System.md §13` (no changes there). `MusicianCardCatalogData` and `GenericCardCatalogSO` remain governed under `Assets/Scripts/Cards/`, already covered by the existing manifest entry's `governs` glob. No new SSoT created; new section is additive within an existing one.

### Operational changes

**New runtime code file:**
- `GenericCardCatalogSO.cs` (`Assets/Scripts/Cards/GenericCardCatalogSO.cs`). New `ScriptableObject` type for "Owner: Any" generic starter cards. Reuses `MusicianCardEntry` as the entry type (no schema duplication). `[CreateAssetMenu]` registered under "ALWTTT/Cards/Generic Card Catalog".

**Modified runtime code files:**
- `PersistentGameplayData.cs` — new method `SetBandDeckFromMusicians(IList<MusicianCharacterData>, GenericCardCatalogSO)` (~165 lines). Branches in `ApplyRunConfig` on the new `config.useMusicianStarters` flag: when ON, builds roster from `MusicianList` and calls the new method; when OFF, calls the existing `SetBandDeck(BandDeckData)`. The closing `ApplyRunConfig` log line updated to use `config.deckLabel ?? config.bandDeck?.name ?? "<unset>"` instead of just `config.bandDeck?.name`. No changes to `SetBandDeck(BandDeckData)`'s implementation.
- `GigRunContext.cs` — `RunConfig` gains `useMusicianStarters : bool` and `deckLabel : string` fields. `BeginRun` log line updated to use `_current?.deckLabel ?? _current?.bandDeck?.name`.
- `GigSetupConfigData.cs` — new `genericStarterCatalog : GenericCardCatalogSO` serialized field + `GenericStarterCatalog` accessor. `availableBandDecks` and `AvailableEncounters` retained unchanged as the dev-fallback path's source.
- `GigSetupController.cs` — new `useMusicianStartersToggle : Toggle` UI ref. `OnStartPressed` reworked: branches at the top on `useMusicianStartersToggle.isOn` (default ON in scene). Auto-assembly path skips the dropdown lookup, runs an empty-roster pre-flight guard (errors and refuses to start if `MusicianList` is empty), builds `deckLabel` as `<auto:idA+idB+...>`. Legacy path resolves `selectedDeck` from the dropdown as before, builds `deckLabel` as the asset name. Both paths populate `RunConfig.useMusicianStarters` and `RunConfig.deckLabel` accordingly. Closing `[GigSetup] Starting gig` log line updated with `Deck=<deckLabel>, AutoAssembly=<bool>`.

**Smoke test results table (ST-M46p2-1 through -8):**

| ID | Description | Result |
|---|---|---|
| ST-M46p2-1 | Auto-assembly basic — toggle ON, populated catalogs, gig starts with correct totals | PASS — `Action=20 (per-musician=20, generic=0), Composition=8 (per-musician=8, generic=0), musicians=4`, all three log sites show `Deck=<auto:3+1+4+2>` |
| ST-M46p2-2 | Legacy regression — toggle OFF, `BandDeckData` selected from dropdown, identical to pre-batch behavior | PASS — `[PersistentGameplayData] SetBandDeck -> Action=17, Composition=5 (Deck='TestDeck_FullCoverage', uniqueEntries=22)`, auto-assembly log line absent, `AutoAssembly=False` |
| ST-M46p2-3 | Null catalog graceful — clear one musician's `CardCatalog`, auto-assembly continues with warning | PASS — warning fires (`musician 'Conito' (id=1) has no CardCatalog. Skipping contribution.`), `skippedNoCatalog=1`, Action drops 20→10, no NRE |
| ST-M46p2-4 | `starterCopies = 0 + StarterDeck` warn-skip | DEFERRED-by-construction. Editor-time clamps (`MusicianCatalogService.TryAddEntry` applies `Max(1,…)`, `[Min(1)]` on `MusicianCardEntry.starterCopies`) make the state unreachable from tooling. Defensive code path is structurally identical to ST-M46p2-3's PASSED `skippedNoCatalog` branch. |
| ST-M46p2-5 | Generic catalog contribution — non-null `GenericStarterCatalog`, generic counts visible in summary | PASS — generic count visible in summary, per-musician counts unchanged, gig started, generic card present in deck |
| ST-M46p2-6 | Provenance on `RemoveMusicianFromBand` — verify per-musician cards stripped, generic cards survive | PASS — Action 22→12 (drop=10 = Conito's per-musician contribution), Composition unchanged, generic card (`new_conito_vibe_single ×2`) still present after removal, `RemoveMusicianFromBand('1')` returned `True`. Verified via temporary `[ContextMenu]` scaffold on `GigManager` (removed at closure). |
| ST-M46p2-7 | Empty roster guard — toggle ON, `MusicianList` cleared, gig refuses to start | PASS — error fires verbatim (`Auto-assembly enabled but PersistentGameplayData.MusicianList is empty. Cannot start gig. ...`), `BeginRun` and `SetBandDeckFromMusicians` did not fire, scene did not navigate |
| ST-M46p2-8 | Deck label consistency across `[GigRunContext] BeginRun`, `[PersistentGameplayData] ApplyRunConfig`, `[GigSetup] Starting gig` log lines | PASS — all three lines consistently render `Deck=<auto:3+1+2+4>` (auto path) or `Deck=TestDeck_FullCoverage` (legacy path) |

### Side-findings (non-blocking)

- **Pre-existing inventory viewer NRE.** `CardBase.SetCard:77` throws `NullReferenceException` when the inventory canvas instantiates cards on Draw/Discard/Hand pile open. Stack ends at `inspirationCostTextField.text = ...`. Likely unassigned UI text reference on the inventory's card display prefab, introduced when inspiration cost/gen fields were added to `CardBase`. Not caused by batch (2) — surfaced during ST-M46p2-2 click-through. Promoted to roadmap as a separate UI-fix batch.
- **Card Editor per-row starter UX.** Flagging entries one-at-a-time via the right-side inspector's `EnumFlagsField` dropdown is tedious for multi-card catalog setup. Per-row `Starter` toggle column on the entries list proposed and accepted as the cleaner shape (vs. the originally-proposed bulk toolbar). Promoted to roadmap as Future Milestone — Authoring tooling QoL (batch (3)).
- **All-starter-flagged catalog content.** Smoke tests required all entries flagged starter to verify the auto-assembly path; the resulting ~28-card test deck is much larger than the designed 12-card M4.6 starter. Marked as M4.6 pre-demo blocker in `CURRENT_STATE.md §3` and §4.

### Files unchanged (with reasoning)

- `coverage-matrix.md` — no new subsystem; auto-assembly is a new code path within the existing `Cards`/`Data` subsystems already governed.
- `SSoT_Editor_Authoring_Tools.md` — no new editor tool. Batch (2) is runtime-only (the `useMusicianStartersToggle` is a single Toggle UI element wired to existing `GigSetupController`, not a new authoring tool). Batch (3), when it lands, will introduce `CardInventoryWindow` and toolbar buttons; that closure will update this doc.
- `SSoT_INDEX.md` — no authority change. The auto-assembly contract finds its home in the existing `SSoT_Card_Authoring_Contracts.md`.
- `SSoT_Card_System.md` — runtime pile lifecycle (DrawPile/HandPile/DiscardPile) is unchanged; auto-assembly produces the same flat `CardDefinition` references the legacy path produces. No section in `SSoT_Card_System.md` is contradicted or extended by batch (2).

### Smoke tests

ST-M46p2-1 through -8 results table above. Verification approach: console log inspection for -1 through -5 and -7 and -8; temporary `[ContextMenu]` scaffold on `GigManager` for -6 (provenance check, removed at closure); editor-time defensive structure check by inspection for -4.

---

## 2026-05-01 — M4.6-prep-A closure: DeckCardCreationService MB2 catalogue migration

### Semantic changes

**SSoT_Card_Authoring_Contracts:**
- §5.9 "Audience-side status authoring (M4.3)": stale footnote removed (`Note: the parallel DeckCardCreationService path still consults a single catalogue field — fix tracked in CURRENT_STATE.md §4, scheduled for M4.6 prep.`). Replaced with a closing paragraph stating that the Card Editor and Deck Editor JSON import paths apply the contract uniformly via the registries helpers. The section now describes a single, unified MB2-aware editor toolchain with no remaining single-catalogue holdouts.

**CURRENT_STATE:**
- §1 new closure block "M4.6-prep-A — DeckCardCreationService MB2 catalogue migration — complete (2026-05-01)" inserted after the M4.5 block. Documents shipped scope (3 editor-only files: `DeckCardCreationService.cs`, `DeckJsonImportService.cs`, `DeckEditorWindow.cs:754-757`), the strict-resolution policy decision (local `TryResolveStatusByKey` name-fallback removed for parity with Card Editor), and EV-1..EV-5 verification outcomes (EV-1/2/3/5 PASS via Deck Editor UI; EV-4 PASS by code inspection because `DeckEditorWindow.ResolveRegistries` makes the null-path UI-unreachable; EV-3 is the explicit regression case for audience-side `earworm` resolution from deck JSON).
- §3 line 180: M4.6 entry updated. The dependency phrase "+ `DeckCardCreationService` parallel registries fix" replaced with parenthetical closure marker referencing M4.6-prep-A.
- §4 Open items: `DeckCardCreationService` parallel registries fix bullet removed (closed). New bullet added: Card Editor inline effects-block UI on legacy catalogue alias — surfaced during M4.6-prep-A audit. `CardEditorWindow.cs:1267, 1305` still call `DrawEffectsBlock(effectsProp, _registries?.StatusCatalogue)`, exposing only the musicians catalogue to the inline effect-row picker. Same MB2 friction shape, smaller surface, authoring-UX only (not import resolution). Logged, not scheduled.
- §5 pending doc edits: `SSoT_Card_Authoring_Contracts.md` §5.9 + this changelog + the §1/§3/§4 edits marked applied for M4.6-prep-A.

### Authority changes

None. No SSoT promoted or retired. The MB2 catalogue split's authoritative home remains `SSoT_Card_Authoring_Contracts.md §5.9`. M4.6-prep-A closes a code-side migration gap that the existing §5.9 contract had been silently overstating since M4.3.

### Operational changes

**Modified code files (editor-only, `#if UNITY_EDITOR` guarded):**
- `DeckCardCreationService.cs` — `TryStageNewCard(DeckCardEntryJson, ALWTTTProjectRegistriesSO, out StagedCardEntry, out string)` (was `StatusEffectCatalogueSO`). Internal `ApplyEffectsJson` signature migrated identically. `ApplyStatusEffect` branch ports `registries.TryGetStatusEffectByKey` / `TryGetStatusEffectByPrimitive` from `CardEditorWindow_JsonImport.ApplyEffectsJson`. Error wording aligned ("ApplyStatusEffect requires ALWTTTProjectRegistriesSO loaded in the Registries field"). Local `TryResolveStatusByKey` (name + filename fuzzy fallback against a single catalogue) deleted.
- `DeckJsonImportService.cs` — `Import(string, ALWTTTProjectRegistriesSO)` signature + local capture rename + pass-through to `TryStageNewCard`. `using ALWTTT.Status;` left in (now technically unused; harmless hint, no warning).
- `DeckEditorWindow.cs:754-757` — `var catalogue = _registries?.StatusCatalogue;` line dropped. `DeckJsonImportService.Import(_jsonText, _registries)` called directly. The legacy alias `_registries?.StatusCatalogue` still exists on `ALWTTTProjectRegistriesSO` for non-editor callers and the Card Editor's inline effects-block UI (see open-items note in `CURRENT_STATE.md §4`).

**Editor verification log (EV-1 through EV-5):**
- EV-1 PASS — Deck JSON with reference-only entries imports identically to pre-patch behavior (Sibi Major Chords ×2 staged correctly).
- EV-2 PASS — Deck JSON with new card creating `ApplyStatusEffect(statusKey: "flow")` resolves the musicians-side Flow SO; staged row reads `▸ Flow +1`.
- EV-3 PASS — Deck JSON with new card creating `ApplyStatusEffect(statusKey: "earworm", targetType: "AllAudienceCharacters")` resolves the audience-side Earworm SO; staged row reads `▸ Earworm +1`. **Regression case for the bug being fixed.** Pre-patch, this exact JSON would have errored with "No StatusEffectSO found for statusKey 'earworm'".
- EV-4 PASS by code inspection — patched error string `"effects[i]: ApplyStatusEffect requires ALWTTTProjectRegistriesSO loaded in the Registries field."` is correct and present in the patched file. Cannot be triggered via the Deck Editor UI in any project containing at least one `ALWTTTProjectRegistriesSO` asset because `DeckEditorWindow.ResolveRegistries` (called immediately before `DeckJsonImportService.Import`) auto-finds any such asset via Resources or `AssetDatabase.FindAssets`. The null-check is retained for non-UI callers and parity with Card Editor.
- EV-5 PASS — Deck JSON with new card creating `ApplyStatusEffect(effectId: 100)` (`DamageUpFlat`) resolves to the same Flow SO that EV-2's `statusKey: "flow"` resolved to (because Flow's `IsDefaultVariant: true` and its primitive is `DamageUpFlat`). Confirms `TryGetStatusEffectByPrimitive` is on the integer path.

### Smoke tests

Editor verification (EV-1..EV-5) only. No runtime gameplay change; no smoke tests required per the project's smoke-test scope rule. EV-3 stands as the regression test.

### Files unchanged

`ssot_manifest.yaml`, `coverage-matrix.md`, `Roadmap_ALWTTT.md`, `SSoT_Editor_Authoring_Tools.md`. Manifest invariant on the MB2 catalogue split (`ssot_manifest.yaml:222`) already covers the policy generally; no new invariant introduced. Coverage matrix unaffected (no new subsystem). Roadmap §4.6 has no explicit `DeckCardCreationService` checkbox to tick (the prep was a `CURRENT_STATE.md`-tracked open item, not a roadmap milestone). `SSoT_Editor_Authoring_Tools.md` describes `DeckCardCreationService` only by responsibility (line 154); no wording was specific to single-catalogue handling.

---

## 2026-04-30 — M4.5 closure: Bidirectional guaranteed draws

### Semantic changes

**SSoT_Runtime_Flow:**
- New §4.2 "Bidirectional guaranteed draws (M4.5, 2026-04-30)" inserted under Phase 1, after §4.1. Documents: subtractive rule (total drawn ≤ `min(DrawCount, MaxCardsOnHand - HandPile.Count, DrawPile.Count + DiscardPile.Count)`); three-phase algorithm (normal draws holding back reserved slots → re-evaluate → fill leftover slots); filtered draw mechanism (`DrawCardFiltered` with reshuffle-on-empty fallback); composition-wins tie-break when `effectiveBudget < reserved`; hook-collapse rationale (action and composition windows open simultaneously today); per-fire and per-turn observability; exhaustion case behavior.
- §8 Runtime invariants: new invariant 9 — "Hand draw at PlayerTurn entry guarantees ≥1 Action and ≥1 Composition card in hand when `DrawPile ∪ DiscardPile` allow, without exceeding `DrawCount` or `MaxCardsOnHand`." Carries forward the subtractive contract and the composition-first tie-break.

**SSoT_Card_System:**
- §13 "Deck multiplicity (M4.4)": new closing paragraph cross-references the M4.5 runtime guarantee. Notes that multiplicity (M4.4) makes the "piles allow" precondition near-universal in practice for the v1 starter. Mechanism authority remains `SSoT_Runtime_Flow.md §4.2`.

**CURRENT_STATE:**
- §1 new closure block "M4.5 — Bidirectional guaranteed draws — complete (2026-04-30)" inserted after the M4.4 block. Documents shipped scope: 3 code files touched (`DeckManager.cs`, `GigManager.cs`, `DevModeController.cs`); algorithm summary; tie-break rule; hook-collapse note; observability surface (per-fire log lines + per-turn summary + Dev Mode overlay readout); test results (ST-M45-1/2/4/8 PASS, -3 inferred via Path B, -5/-6 dropped, -7 deferred).
- §3 "What is next": M4.5 line removed (closed); M4.6 promoted to top as the demo gate.
- §4 Open items: M4.5 architectural decision moved from open to RESOLVED with chosen mechanism (Option 1 + subtractive) and tie-break.
- §5 pending doc edits: `SSoT_Runtime_Flow.md`, `SSoT_Card_System.md` §13 cross-reference, and `ssot_manifest.yaml` entries marked applied for M4.5.

**Roadmap_ALWTTT:**
- §4.5 reworded as closure block. Chosen mechanism documented (Option 1 + subtractive, composition-wins tie-break). Hook-collapse rationale added. Smoke-test outcomes recorded inline.
- §DoD: M4.5 checkbox ticked.

**ssot_manifest.yaml:**
- New invariant on `Docs/systems/SSoT_Runtime_Flow.md`: bidirectional guaranteed-draw subtractive rule.

**coverage-matrix.md:** no change. The "Runtime phase flow" row already points to `SSoT_Runtime_Flow.md` as authority; M4.5 does not introduce a new subsystem or change a primary home. (Note: an earlier batch plan listed a coverage-matrix row addition; that was an error in interpretation of the matrix's format and is corrected here.)

### Authority changes

None. No SSoT promoted or retired. The M4.5 milestone's authoritative home is `SSoT_Runtime_Flow.md §4.2`. `SSoT_Card_System.md §13` carries a cross-reference paragraph only — multiplicity remains the authority of `SSoT_Card_System.md`, the guarantee mechanism remains the authority of `SSoT_Runtime_Flow.md`. The roadmap's "two symmetric hooks" framing is intentionally collapsed to one site in current implementation; a future hook split is reserved for if/when the action window and composition window separate into distinct phase transitions.

### Operational changes

**Modified code files:**
- `DeckManager.cs` — new public surface: `DrawCardsForPlayerTurn(int)`, `DrawCardFiltered(Func<CardDefinition,bool>, string)`, `HandHas(Func<CardDefinition,bool>)`, `PilesHave(Func<CardDefinition,bool>)`, `LastTurnGuaranteeSummary` accessor. New private members: `_lastTurnGuaranteeSummary` field, static `FindRandomMatchIndex(List<CardDefinition>, Func<CardDefinition,bool>)`. New methods sit between `DiscardHand()` and the existing `#if ALWTTT_DEV` block. No existing methods modified.
- `GigManager.cs` — one-line replacement inside `ExecuteGigPhase(PlayerTurn)`: `DeckManager.DrawCards(pd.DrawCount)` → `DeckManager.DrawCardsForPlayerTurn(pd.DrawCount)`. Three SSoT-authority comment lines added above the call.
- `DevModeController.cs` — 11-line readout block added inside the existing `gm != null` guard, immediately after the verbose pile-counts block. Reads `DeckManager.Instance.LastTurnGuaranteeSummary` and renders `M4.5 last draw: …`. Always visible (independent of `_verboseLogs`).

### Findings

- The roadmap's "two symmetric hooks" framing was based on a model where the composition phase entry and the action window entry are distinct phase transitions. Current implementation does not separate them — `ExecuteGigPhase(PlayerTurn)` opens both windows simultaneously. The closure preserves the bidirectional guarantee while collapsing the hook to one site. If a future redesign separates the windows, the hook split is a future refactor; the SSoT documents the collapse explicitly so the divergence is not silent.
- The reservoir-style `FindRandomMatchIndex` is `O(n)` per filtered draw call, where `n` is `DrawPile.Count`. At hand-scale piles (12-card starter), this is trivial. Documented as the chosen complexity profile because it keeps the predicate side fully generic for future filtered-draw use cases (musician-tagged, status-gated, etc.).
- Observability surface (per-fire `[M4.5 GuaranteeComp]`/`[GuaranteeAction]` + per-turn summary line + always-on Dev Mode overlay readout) is the primary evidence path for ST-M45-3 (both guarantees same turn). Path B was chosen — direct rigging of `DrawPile` to a specific composition is not in current Dev Mode tooling and was not added in this batch. Any turn where the live overlay shows `needs=[CA] fired=[CA]` is direct evidence of the both-fire case.

### Smoke tests

- ST-M45-1 (composition guarantee fires when needed) ✅
- ST-M45-2 (action guarantee fires when needed) ✅
- ST-M45-3 (both guarantees same turn) — covered by inference from ST-M45-1 + ST-M45-2 (Path B).
- ST-M45-4 (subtractive: total drawn = budget across 10 turns infinite Dev Mode; implicit no-fire baseline) ✅
- ST-M45-5 (re-evaluation: unused reserved slot becomes normal draw) — dropped as redundant with ST-M45-4 invariant.
- ST-M45-6 (no-fire baseline on leftover-hand turn) — dropped as redundant with ST-M45-4 implicit no-fire observations.
- ST-M45-7 (empty-domain in piles is silent) — deferred. No starter card has `ExhaustAfterPlay = true`; case unreachable in normal play.
- ST-M45-8 (reshuffle during filtered draw) ✅

### Lifecycle

M4.5 batch closed. Next milestone is M4.6 (Starter Deck v1 authoring) — the demo gate. Carried-over open items unchanged: `DeckCardCreationService` parallel registries fix (M4.6 prep), `TryResolveStatusEffectFromKey` dead-code cleanup, Gig Setup roster pickers, Pending Effects (post-MVP), Tempo-coupled card identity (post-MVP, unscheduled).

---

## 2026-04-29 — M4.4 closure: Deck Contract Evolution (card copies)

### Semantic changes

**SSoT_Card_System:**
- New §13 "Deck multiplicity (M4.4)". Documents: multiset shape (`BandDeckData` as `List<BandDeckEntry>`), runtime expansion in `PersistentGameplayData.SetBandDeck`, pile-lifecycle identity preservation per reference, lazy legacy migration via `[FormerlySerializedAs("cards")]` + `legacyCards` fallback path, `EnumerateCards()` helper for flat consumers.
- §12 "This SSoT owns" list extended with "deck-level multiplicity (multiset shape, runtime expansion, pile-lifecycle invariance under play and reshuffle)".

**SSoT_Card_Authoring_Contracts:**
- §5.7 `EntryJson.starterCopies` bullet clarified: authoring-only at M4.4; runtime consumption scheduled for M4.6 when catalogue → starter-deck auto-assembly lands.
- New §5.10 "Deck-level multiplicity (M4.4)". Documents: per-entry `count` field default 1; duplicate `cardId` references combine additively with non-blocking warning; duplicate `kind`-bearing ids remain a hard error with pendingCard cleanup; full round-trip through Export/Import.
- §7.1 stage invariants: bullet added — staged card slots carry a per-entry `count` editable inline via `−`/`+` controls.

**CURRENT_STATE:**
- §1 M4.4 closure block added.
- §3 "What is next": M4.4 line removed (closed); M4.5 promoted to top with architectural decision flagged; M4.6 preconditions updated to include `DeckCardCreationService` parallel registries fix.
- §4: "True card copies in decks" item moved from open to RESOLVED with cross-references.
- §5 pending doc edits: M4.4-related entries (`SSoT_Card_Authoring_Contracts.md`, `SSoT_Card_System.md`, `ssot_manifest.yaml`) marked applied.

**Roadmap_ALWTTT:**
- §4.4: `starterCopies` runtime-consumption sentence reworded — deferred to M4.6.
- §4.6 scope: bullet added for `starterCopies` becoming source of `BandDeckEntry.count` if catalogue → starter-deck auto-assembly is built in M4.6.
- §DoD: M4.4 checkbox ticked.

### Authority changes

None. No SSoT promoted or retired. The line at `changelog-ssot.md` 2026-03-21 design pass that read `BandDeckData.Cards : List<CardDefinition>` is deduplicated by reference in `PersistentGameplayData.SetBandDeck`. True copies are not a runtime concept today. M4.4 closes this. — is now satisfied; left in place as historical record.

### Operational changes

**New code files:**
- `Assets/Scripts/Data/Cards/Configs/BandDeckEntry.cs` — `[Serializable]` type pairing a `CardDefinition` with `[Min(1)] int count = 1`.

**Modified code files:**
- `BandDeckData.cs` — multiset shape: new `List<BandDeckEntry> entries` field; legacy `cards` field renamed to `legacyCards` with `[FormerlySerializedAs("cards")]`; new `Entries` accessor with lazy legacy fallback; new `EnumerateCards()` helper. Pre-M4.4 `Cards` property removed; consumers migrated to `Entries` / `EnumerateCards()`.
- `PersistentGameplayData.cs` — `SetBandDeck` rewritten to iterate `bandDeck.Entries` and expand counts into independent references on `currentActionCards` / `currentCompositionCards`. Pre-M4.4 dedup-by-reference (`if (!currentActionCards.Contains(card))`) removed. Log line updated to report per-domain totals + uniqueEntries.
- `GigManager.cs` — two consumer sites in deck-source resolution (`RunContextBandDeck` and `Auto` fallback) now call `bandDeck.EnumerateCards()` instead of `bandDeck.Cards`.
- `DeckEditorDtos.cs` — `DeckCardEntryJson.count` field added (default 1). `StagedCardEntry.count` field added with `[SerializeField]`. `FromExisting` defaults count=1; `FromPending` reads `dto.count`.
- `DeckJsonImportService.cs` — `seenCards: HashSet<string>` replaced with `byId: Dictionary<string, StagedCardEntry>`. Reference-existing duplicate cardId → combine additively + warning. Create-new duplicate id → hard error with pendingCard/Payload cleanup. `Export` emits `count` per entry.
- `DeckAssetSaveService.cs` — `WriteFields` rewritten to write the new `entries` SerializedProperty (`card` + `count` sub-properties per element). Legacy `legacyCards` array cleared on every save (asset upgrade path).
- `DeckEditorWindow.cs` — `DoLoadDeck` reads `Entries` and copies count to staged entries. Staged-list header reads `"N cards, K unique"`. `DrawStagedCardRow` adds `×N` badge + `−` / `+` mini-buttons (`−` at count==1 removes the entry, equivalent to Remove). Catalogue Add button is no longer disabled when card is already staged; label changes to `+1` and click increments count.

### Structural changes

- One new code file (`BandDeckEntry.cs`).
- No file deletions, renames, or moves.
- `BandDeckData` field rename `cards` → `legacyCards` is `[FormerlySerializedAs]`-safe; existing `BandDeck - Test 1.asset` (and any other pre-M4.4 deck assets) read correctly without re-authoring.

### Findings / residual

- Mid-batch compile-error fix: initial Batch 1 missed two `.Cards` consumers in `GigManager.cs` (RunContextBandDeck, Auto fallback) and two in `DeckEditorWindow.DoLoadDeck`. Fixed in same session before Batch 2 by introducing `BandDeckData.EnumerateCards()` helper and patching `DoLoadDeck` to iterate `Entries`. Lesson: greps for `bandDeck.Cards` should have been `\.Cards\b` to catch arbitrary local variable names.
- ST-M44-9 deferred (duplicate `kind`-id error path). The branch is preserved in code (`byId.ContainsKey` check + cleanup) but not exercised in the test deck. Validation remains the M4.3 baseline + reasoning from code review.
- ST-M44-10 N/A. TestEarworm card not present in `TestingDeck_CombatMVP`; M4.3 audience-status authoring path is unchanged structurally by M4.4 (the `ApplyStatusEffect` resolution path lives in card creation, not deck shape).
- `legacyCards` is cleared on every Save through Deck Editor. Loading without saving leaves the asset's legacy field intact — intentional (zero-touch on existing assets) but worth noting if anyone manually inspects an asset post-M4.4 and is surprised to see both `entries` empty and `legacyCards` populated. They saw a Load, not a Save.

### Lifecycle

- M4.4 closed 2026-04-29. Critical path advances: **M4.5 (next) → M4.6 (demo target).**
- Open items unchanged from M4.3 closure: `DeckCardCreationService` parallel registries fix (M4.6 prep), `TryResolveStatusEffectFromKey` dead code cleanup, Gig Setup roster pickers (pre-M4.6 or rolled-in), Pending Effects (post-MVP first), Tempo-coupled card identity (post-MVP, unscheduled).

## 2026-04-28 — Post-MVP planning: Pending Effects + Tempo-coupled card identity

### Authority changes

- Two new planning docs added under `planning/` (per project planning-track convention; precedent: `Design_Audience_Status_v1.md`, `Design_Starter_Deck_v1.md`):
  - `Design_Pending_Effects_v1.md` — proposes a song-scoped accumulator layer where cards add to a pending bucket during a song and resolve at song end. First user: deferred Earworm. Scheduled as the **first post-MVP gameplay system batch** immediately following M4.6 demo closure.
  - `Design_Tempo_Identity_v1.md` — captures a long-term design direction making tempo a gameplay input axis (fast/slow card preferences, deck archetype identities). **No implementation slot.** Influences starter deck and per-musician catalog design choices now via flavor / naming / archetype lean.

### Semantic changes

None to existing systems. Both new docs are planning-track additions and do not modify implemented behavior or any current SSoT.

**Roadmap_ALWTTT:**
- New post-MVP sections appended: "Post-MVP — Pending Effects system (planned, first post-MVP batch)" and "Post-MVP — Tempo-coupled card identity (design direction, no implementation slot)". Both reference the new planning docs.

**Design_Starter_Deck_v1:**
- New subsection added on "Tempo-lean as design intent (not runtime)" — flags that starter card naming, flavor, and per-musician catalog shape should soft-reflect tempo identity now even though tempo coupling is not implemented. Includes a placeholder for per-musician tempo-lean sketches to be authored during the M4.6 starter design pass.

**CURRENT_STATE §4:**
- Two open items appended: Pending Effects system (post-MVP, scheduled first) and Tempo-coupled card identity (post-MVP, long-term, no implementation slot). Both reference the new planning docs.

### Operational changes

None. No code change. No SSoT semantic change. Mind Tap (M4.6) remains an immediate Earworm applier; Earworm itself remains unchanged from the M4.3 spec in `SSoT_Status_Effects.md §5.7`.

### Structural changes

- Two new files in `planning/`. No file deletions, renames, or moves.

### Findings / residual

- Per-musician tempo-lean sketches in `Design_Starter_Deck_v1.md` are flagged as a placeholder, to be filled in during M4.6 starter design authoring. Not a blocker — starter cards can be authored without it; the placeholder ensures the question gets asked during that pass.
- The new planning docs follow the same supersession pattern as `Design_Audience_Status_v1.md`: when implementation eventually happens, the relevant sections get a "superseded by SSoT_X.md §Y" banner and the SSoT becomes authoritative.

### Lifecycle

- Post-MVP planning track is now non-empty. The next post-MVP batch (after M4.6 closure) is provisionally **Pending Effects implementation** per `Design_Pending_Effects_v1.md`. Tempo-coupled card identity remains explicitly unscheduled; it influences current design choices but does not block any milestone.
- Critical path unchanged: **M4.4 → M4.5 → M4.6 (demo).** This batch does not touch implementation work.

## 2026-04-28 — M4.3 closure: Earworm — first audience-side status

### Semantic changes

**SSoT_Status_Effects:**
- New §5.7 Earworm. Full spec: primitive `DamageOverTime`, key `"earworm"`, audience-only, `AudienceTurnStart` tick timing, `Additive`/`LinearStacks`, `MaxStacks=99`, `IsBuff=false`. Documents read-then-decay ordering in `GigManager.AudienceTurnRoutine`, `N(N+1)/2` Vibe-curve semantics, Flow non-amplification, `IsBlocked` skip, harmless tick on `IsConvinced`, and variant relationship with Feedback (shared `DamageOverTime` primitive, separate catalogues post-MB2).
- Validation history records ST-M43-1a..8 PASS plus the duplicate-Tick block fix iteration.
- §5.7 supersedes `planning/Design_Audience_Status_v1.md` §3.

**SSoT_Audience_and_Reactions:**
- §8: paragraph added — audience members may carry player-applied statuses that shape their audience-turn state; runtime owned by Status SSoT; surface extends through `StatusEffectCatalogue_Audience`.
- §10: MVP rules bullet replaced — audience-side statuses are now part of MVP baseline (Earworm is the first), not optional.

**SSoT_Card_Authoring_Contracts:**
- Note added that JSON `ApplyStatusEffect` resolution probes both `StatusCatalogueMusicians` and `StatusCatalogueAudience` on `ALWTTTProjectRegistriesSO`. Audience-side card authoring requires both catalogue fields populated on the registries asset.

**SSoT_Editor_Authoring_Tools:**
- Note added that `ALWTTTProjectRegistriesSO` exposes `StatusCatalogueMusicians` / `StatusCatalogueAudience` post-MB2. Legacy `StatusCatalogue` alias retained (returns musicians catalogue) for source compatibility with pre-split callers.

### Authority changes

- `planning/Design_Audience_Status_v1.md` §3 (Earworm) marked **superseded by `SSoT_Status_Effects.md` §5.7** with closure date 2026-04-28. §4 (Captivated) and §5 (`ApplyIncomingVibe`) retain authority — both remain in roster-expansion scope.

### Operational changes

- New SO asset: `Assets/Resources/Data/Status/StatusEffect_Earworm_DamageOverTime.asset` (`statusKey: "earworm"`, primitive: `DamageOverTime`, default variant of audience catalogue).
- New icon sprite asset (`bicho icono_0` per asset reference).
- `StatusEffectCatalogue_Audience.asset` populated with the Earworm SO (was empty post-MB2).
- New dev-only test card: `TestEarworm.asset` in Gusano's musician card folder (`actionTiming: Always`, `inspirationCost: 0`, `[TEST]` prefix on DisplayName). Retained as a regression harness for future audience-status work; out of starter deck.
- `GigManager.cs`: new Earworm read-and-apply block inserted at the top of `AudienceTurnRoutine`, before the existing `Tick(AudienceTurnStart)` loop. Reads `Statuses.TryGet(CharacterStatusId.DamageOverTime)`, disambiguates by `StatusKey == "earworm"`, calls `AddVibe(stacks)`, emits `[Earworm]` tick log with before/after Vibe.
- `CardBase.cs`: apply-time `[Effects]` log expanded — now logs `StatusKey` + `DisplayName` alongside primitive `EffectId`, disambiguating shared-primitive variants in console output.
- `ALWTTTProjectRegistriesSO.cs`: refactored — `statusEffectCatalogue` field renamed to `statusEffectCatalogueMusicians` (with `[FormerlySerializedAs]` preserving existing reference), new `statusEffectCatalogueAudience` field added. New properties `StatusCatalogueMusicians` / `StatusCatalogueAudience`. New helpers `TryGetStatusEffectByKey` / `TryGetStatusEffectByPrimitive` probe both catalogues musicians-first. Legacy `StatusCatalogue` alias retained (returns musicians catalogue) for source compatibility.
- `CardEditorWindow.cs`: toolbar warning helpbox expanded — enumerates which specific registries field is missing (CSO Primitive Database / Musicians catalogue / Audience catalogue) instead of a generic "missing references" message.
- `CardEditorWindow_JsonImport.cs`: `ApplyEffectsJson` signature changed to take `ALWTTTProjectRegistriesSO` instead of `StatusEffectCatalogueSO`. `ApplyStatusEffect` branch resolves status via `registries.TryGetStatusEffectByKey` / `…ByPrimitive` (probes both catalogues). Stale private helper `TryResolveStatusEffectFromKey` left in place — unused after this batch, deferred for later cleanup.

### Structural changes

- Two new asset files (Earworm SO, Earworm icon).
- One new audience-side catalogue entry.
- One new dev-only test card asset.
- No new code files.

### Findings / residual

- M4.3 patch initially shipped with a copy-paste duplicate `Tick(AudienceTurnStart)` block in `AudienceTurnRoutine`, producing -2/turn decay (stacks halved each audience turn instead of decrementing by 1). Caught by ST-M43-2/3 stack-count observation; resolved by deletion of the duplicate block. Logged here as artefact for traceability — not an SSoT semantic change.
- Side issue surfaced (deferred): `DeckCardCreationService` / Deck Editor still uses single-catalogue lookup. Same MB2-shaped friction Card Editor had. Will block any audience-status reference in Deck Editor JSON. Tracked in `CURRENT_STATE.md §4`; recommended fix at M4.6 prep as a tooling-readiness step.
- Side issue surfaced (deferred): Gig Setup Scene supports only single-character roster paths for both band and audience. Bidirectional multi-select picker requested. Tracked in `CURRENT_STATE.md §4`; recommended slot is a standalone batch ahead of M4.6, or rolled into M4.6.
- Stale dead code: `TryResolveStatusEffectFromKey(StatusEffectCatalogueSO, ...)` in `CardEditorWindow_JsonImport.cs` no longer has callers after the registries refactor. Not removed in this batch — separate cleanup.

### Lifecycle

- M4.3 closed 2026-04-28. Earworm validated end-to-end.
- Next batch on critical path: **M4.4 — Deck Contract Evolution (card copies)**.
- Roster Expansion (post-MVP track) remains unchanged in scope: Captivated + `ApplyIncomingVibe` still deferred to roster expansion; design intent retained in `planning/Design_Audience_Status_v1.md` §4 / §5.

## 2026-04-28 — M4.2 Flow bifurcation + adaptive LoopScoreCalculator

### Semantic changes

**SSoT_Gig_Combat_Core:**
- §6.1 Flow: rewritten. Combat meaning changed from "amplifies Loop → SongHype conversion" to bifurcated Vibe interaction (flat per-performer on Action, multiplier band-wide on Composition + Song End). Flow → SongHype path explicitly retired and removed.
- §8.1 Loop → SongHype: updated. Flow no longer modifies this delta. Adaptive role-budget scoring with `LoopScoringMode` enum and auto-detected `possibleRoleCount` / `totalMusicians` documented.
- §11 Implementation status: 4 new rows (Flow bifurcation, per-performer Flow, adaptive LoopScore, Flow→SongHype retired).

**SSoT_Scoring_and_Meters:**
- §3 LoopScore: expanded with current formula (§3.2) and HypeDelta conversion (§3.3). Adaptive scoring documented with RoleNormalization and MusicianParticipation modes.
- §7.1 Flow: rewritten from "interacts with LoopScore → SongHype layer" to "interacts with the Vibe layer, bifurcated by card domain."

**SSoT_Status_Effects:**
- §5.1 Flow: combat meaning rewritten to match bifurcated model. Validation references updated with M4.2 smoke tests.

**CURRENT_STATE:**
- §1: M4.2 closure block added.
- §3: next-up rewritten — M4.3 Earworm is the new top item.
- §4: LoopScoreCalculator tuning risk removed (resolved). Flow path split risk removed (resolved). Runtime tuning values bullet updated (Flow values landed). Two new items added: musician picker in Gig Setup, per-musician starter decks.
- §5: M4.2 pending doc edits removed (applied).

### Authority changes

None. No SSoT promoted or retired.

### Structural changes

**Modified code files:**
- `LoopScoreCalculator.cs` — full replacement. `LoopScoringMode` enum, `LoopScoringConfig` struct, `HypeThresholds` struct, adaptive `ComputeRoleBudgetScore` (RoleNormalization + MusicianParticipation), `CountDistinctRoles` (bitfield), `CountDistinctMusicians` (HashSet). Both `ComputeLoopScore` and `ComputeHypeDelta` now take config parameters.
- `LoopFeedbackContext.cs` — added `HasBacking` convenience property.
- `GigManager.cs` — retired 3 legacy Flow→SongHype fields + dead code block. Renamed Flow fields with `[FormerlySerializedAs]`. Added `flowVibeMultiplier`, `LoopScoringConfig`, `HypeThresholds` serialized fields. Added `InitLoopScoringConfig()` (auto-detects possibleRoleCount + totalMusicians at gig start). Song-end Flow path changed from flat bonus to multiplier.
- `CardBase.cs` — `ModifyVibeSpec` handler bifurcated: Action cards use per-performer Flow (new `GetPerformerFlowStacks` helper), Composition cards use band-wide Flow multiplier. Debug logs tagged `[Flow→Vibe:Flat]` / `[Flow→Vibe:Mult]`.

### Lifecycle

M4.2 closed. M4 critical path advances to M4.3 (Earworm).

## 2026-04-26 — M1.1 closure + M1 milestone closure + M4.1 Fix C1 closure

### Semantic changes

**Roadmap_ALWTTT:**
- `Last updated` bumped to `2026-04-26 (M1 closed, M4.1 closed)`.
- M1.1 marked complete. Closure detail added with deliverables and smoke test references.
- M1 moved to Completed milestones section with summary.
- M1 DoD: M1.1 and CURRENT_STATE items checked.
- M4.1 marked complete. Closure detail added with deliverables and smoke test references.
- M4.1 DoD item checked.

**CURRENT_STATE:**
- §1: M1.1 closure block, M1 milestone closure block, M4.1 closure block added.
- §2: Deck Editor polish marked complete.
- §3: next-up rewritten — M4.2 Flow bifurcation is the new top item. M1.1 and M4.1 removed.
- §4: C1 open item resolved.

**SSoT_Gig_Combat_Core:**
- §5.4 Stress: implementation note added documenting the unified path via `ApplyIncomingStressWithComposure`. All positive Stress callers (card effects, audience actions, DoT ticks) now route through this single entry point.
- §6.2 Composure: rule added noting audience-path absorption is now active (M4.1).

### Authority changes

None. No SSoT promoted, no SSoT retired, no primary home moved.

### Structural changes

**Modified code files:**
- `DeckEditorWindow.cs` — full replacement. Catalogue filters (musician + effect type), card preview in rows (cost + effect summary), Edit button, last-used save folder via EditorPrefs, enhanced validation.
- `DeckValidationService.cs` — added composition/action ratio warnings.
- `CardEditorWindow.cs` — `OpenAndSelect(CardDefinition)` + `NavigateToCard(CardDefinition)` added for cross-tool integration.
- `AddStressAction.cs` — `AddStress(amount, duration)` replaced with `ApplyIncomingStressWithComposure(statuses, amount, duration)`. Pattern match narrowed to `BandCharacterStats`. Debug log shows Incoming/Absorbed/Applied.

### Lifecycle

- M1 milestone closed. All DoD items checked.
- M4.1 closed. C1 audit finding (2026-03-20) fully resolved.
- Next on critical path: M4.2 (Flow bifurcation + LoopScoreCalculator retune).
- M4.2 and M4.3 are parallelizable; M4.4 and M4.5 are parallelizable after M4.1.
- M4.6 depends on all previous M4 batches.

---

## 2026-04-26 — Starter deck axis resolution: per-card axis assignments locked, design pivot from repetition to variety

### Semantic changes

**`Design_Starter_Deck_v1.md` — substantial revision.** The TBDs in §5.5–§5.8 are resolved into seven new card subsections (§5.5–§5.13). The v0 design (Steady Beat ×3 / Four on the Floor ×2 / Synth Pad ×2 / Hook Theme ×1) is replaced by a per-axis card set that operationalizes the *mínimas cartas, máxima expresividad* principle captured 2026-04-24:

- **C2 four meter cards on axis 7** (Meter / time signature, ✅ via `MeterEffect` PartEffect → `PartConfig.TimeSignature`):
  - Default Mode *(working name)* — 4/4, ×2 copies, with `ApplyStatusEffect(flow, +1, Self)` co-effect on play (sole Flow source in the deck).
  - Waltz Protocol *(working name)* — 3/4, ×1.
  - Pentameter *(working name)* — 5/4, ×1.
  - Compound Cycle *(working name)* — 6/8, ×1.
- **Sibi two backing cards on axis 13** (Chord progression palette, ✅ via `BackingCardConfigSO.progressionPalette` with `tonalities`-restricted entries that override `part.Tonality` per matrix §6.2):
  - Wormus Minor — minor-mode palette (Aeolian/Dorian/Phrygian-leaning), ×1.
  - Wormus Major — major-mode palette (Ionian/Mixolydian/Lydian-leaning), ×1.
- **Sibi one melody card on axis 23** (Phrase palette, ✅ via `MelodyCardConfigSO.phrasePaletteOverride`; inherits Wormus's progression via shared-progression mechanic, matrix §6.1):
  - Singing Field *(working name)* — ×1.

**Aggregate counts preserved from v0:** 12 cards / 8 composition + 4 action / 5 C2 composition copies / 3 Sibi composition copies. Internal distribution restructured. Uniqueness ratio shifts from 12/7 to 12/10 — heavier on variety, lighter on duplication, by design.

**Identity reframes:**
- C2: "engine, more copies, repeats the same card" → **"picks the song's clock"** (meter-axis controller). Character-sketch lore integrated (cyborg-armored math-jazz scholar, "música genuina con fallos," immortal to play music forever).
- Sibi: "Earworm applicator" → **"mode-keeper + Earworm applicator"** (mode controller via Wormus cards, plus existing single-target audience manipulator). Lore integrated (hive-mind worm from the Asteroid Singing Fields). Identity coherence noted: her Action card plants Earworm on audience, her Composition melody card plants memorable phrase shape on song — both register *make this stick* at different scales.

**TrackRole correction captured.** v0 design intent had Sibi targeting `TrackRole.Harmony`. Per the matrix §5.2 finding from 2026-04-24, `BackingCardConfigSO` is the canonical chord-progression home; `HarmonyCardConfigSO` is "how to harmonize an existing line," not "what progression to play." Wormus cards now target `TrackRole.Backing` correctly.

**Open-questions reorganization:**
- §9 #1 (`CompositionCardPayload.effects` support) — **CLOSED retroactively**, citing ST-M13c-6 verification 2026-04-23. Was originally a gating risk for Four on the Floor's Flow co-effect; now confirmed working end-to-end. Default Mode inherits the verified mechanism.
- §9 #5 (Hook Theme classification) — **CLOSED.** Hook Theme retired; replaced by Singing Field which targets `TrackRole.Melody` unambiguously.
- §9 #7 (Singing Field phrase palette content) — **NEW.** Specific `PhrasePaletteSO` asset deferred to authoring.
- §9 #8 (Wormus palette content) — **NEW.** Specific `ChordProgressionData` entries deferred to authoring.
- §9 #9 (card naming pass) — **NEW.** Five working-name placeholders to lock at authoring time.

**Smoke tests revised (§10):**
- ST-SD-4 retargeted from Four on the Floor to Default Mode (same expected behavior, different carrier card).
- ST-SD-5 rewritten as listener test: 4 songs with 4 different meters must be metrically distinguishable to an untrained observer.
- ST-SD-6 new: listener test for Wormus Minor vs Major mode contrast.
- ST-SD-7 new: progression-inheritance test (Wormus + Singing Field combo, melody must follow established progression).
- ST-SD-8 = old ST-SD-6 (full gig), unchanged.

**Cross-card emergents documented (§6.5, new):** Wormus + Singing Field synergy (automatic via shared-progression mechanic), Wormus tonality-override dominance (uncontested in starter), C2 meter axis × Sibi mode axis independence (4 × 2 = 8 distinct song-shape combinations from 7 composition cards).

### Structural changes

**Modified doc files:**
- `planning/Design_Starter_Deck_v1.md` — substantial revision (header date, §1, §2.3, §3.1, §4 table + counts + asymmetry prose + uniqueness-ratio note, §5.5–§5.13 replacing old §5.5–§5.8, §6.2, §6.3, §6.5 new, §8, §9 reorganization, §10 smoke tests, §11 sources).

**No code changes. No asset changes. No SSoT changes. No new docs created.**

**No PK additions.** All evidence-base files (`MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md`, `Design_Starter_Deck_v1.md`, the 14 MidiGenPlay sources, the 5 MidiGenPlay SSoTs) were already in PK from the 2026-04-24 closure.

### Authority changes

**No authority changes.** `Design_Starter_Deck_v1.md` remains the primary home for starter-deck design intent. The *mínimas cartas, máxima expresividad* principle still lives there. The expressive-surface doc still references but does not own the principle.

No SSoT promoted, no SSoT retired, no primary home moved.

### Doc edits applied (this closure)

- `Design_Starter_Deck_v1.md` — substantial revision (this closure).
- `CURRENT_STATE.md` §4 — minor refinement of the LoopScoreCalculator residual-risk note (Melody role coverage now in design, conditional on Singing Field draw).
- `CURRENT_STATE.md` §5 — "Planning docs added for M4 this session" gains a 2026-04-26 amendment line.
- `changelog-ssot.md` — this entry.

**Deliberately not applied:**
- `SSoT_INDEX.md` — `Design_Starter_Deck_v1.md` row unchanged (still listed as active planning).
- `Roadmap_ALWTTT.md` — M4.6 still gated on M4.1–M4.5 and on the remaining §9 questions; no milestone scope or sequence change.
- `coverage-matrix.md` — planning-class docs aren't indexed there per observed conventions.
- `ssot_manifest.yaml` — planning-class amendments aren't tracked there per observed conventions.
- `MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` — its §10 update triggers don't fire (no gap closed, no new field, no precedence shift, no `MusicalIdentityPackSO` promotion).

### Lifecycle

- M4.6 design layer is now resolved. Starter deck has concrete per-card axis assignments, carrier types, and effect specs.
- M4.6 authoring is still gated on: M4.1 (Fix C1), M4.2 (Flow bifurcation + LoopScoreCalculator retune), M4.3 (Earworm), M4.4 (Deck Contract Evolution), M4.5 (Bidirectional guaranteed draws), and on the remaining §9 questions: #2 (composition pipeline vocabulary confirmation), #3 (post-retune values), #4 (runtime tuning values from user), #6 (Mind Tap target enforcement), #7 (Singing Field phrase palette content), #8 (Wormus palette content), #9 (final card naming pass).
- No milestone opened, closed, or re-scoped.
- No new batch tracked. The session functioned as a design pre-resolution for M4.6, removing in-conversation ambiguity from the starter-deck design.

### Known continuity gaps (items not promoted to any doc)

- **Tentative card names** — Default Mode, Waltz Protocol, Pentameter, Compound Cycle, Singing Field. Captured in `Design_Starter_Deck_v1.md` §9 #9 as an authoring-time question. Wormus Minor / Wormus Major are locked.
- **Phrase palette characterization** — what exactly makes a `PhraseArchetypeSO` set "hook-shaped" (motif repetition? inter-phrase intervals? contour constraints?) is not specified. Captured in §9 #7 as authoring-time question.
- **Progression palette specifics** — which `ChordProgressionData` entries belong in each Wormus palette is not specified. Captured in §9 #8 as authoring-time question.
- **Inspiration generation tuning** — current uniform gen 3 across C2 rhythm cards may overweight Default Mode (Flow + gen 3 is strictly best). Captured in §8 as a tuning knob with explicit re-tuning hint (drop Default Mode to gen 2 if it dominates pick rate). Not a doc change pending.

## 2026-04-24 — New planning/reference doc + design maxim capture: MidiGenPlay expressive surface for ALWTTT cards

### Semantic changes

**New planning/reference doc:**
- `planning/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` — first single-source reference mapping the observable musical expressive surface available to ALWTTT composition cards against MidiGenPlay package contracts.
- Status: planning/reference — **not** a governed SSoT. References but does not redefine authority held by `SSoT_ALWTTT_MidiGenPlay_Boundary`, `SSoT_Runtime_CompositionSession_Integration`, `SSoT_Card_System`, `SSoT_Card_Authoring_Contracts`, and the 5 MidiGenPlay-side runtime/composer SSoTs.
- Primary contents: matrix of 26 expressive axes with ALWTTT carrier (PartEffect or styleBundle), SongConfig field reached, package SO consumed, and per-card controllability status (✅/⚠/❌); observed precedences from composer code; per-role bundle contracts; cross-track emergent mechanics; identity pack composition routes; 5 documented contractual gaps.
- §1 Purpose includes an explicit pointer to the design maxim (see below) as the principle the matrix operationalizes.

**New design principle captured:**
- `planning/Design_Starter_Deck_v1.md` gains a new section "Design principle: mínimas cartas, máxima expresividad" stating the composition-card authoring principle that motivated this session. Declares one-axis-per-card, maximum-contrast-between-cards-of-same-musician, and the blind-listener acceptance test as binding for the starter deck and recommended default going forward. References the new expressive-surface doc §3 as evidence base for applying the principle.
- Primary home for the principle: `Design_Starter_Deck_v1.md`. The new expressive-surface doc's §1 Purpose carries a pointer to it but does not duplicate authority.

**Key observations captured (previously implicit, now explicit):**
- `BackingCardConfigSO` is the canonical home for chord-progression selection. `HarmonyCardConfigSO` is thin by design, not a gap (it carries leading + strategy for armonizing existing lines, not progression selection).
- `TrackActionDescriptor.styleBundle` and `List<PartEffect> modifierEffects` are parallel complementary channels on a composition card — one does not substitute the other.
- A chosen progression is shared across tracks of the same Part via `GenContext.SetProgressionForPart`, enabling one Backing card to coordinate harmony across Backing + Melody + Bass simultaneously.
- A progression with a restricted `tonalities` list can override `PartConfig.Tonality` at generation time (composer mutates `part.Tonality` in place).
- `TonalityProfileSO` is resolved package-side via `GenContext.GetTonalityProfileForPart`. Only `PartConfig.Tonality` (enum) crosses the handoff; the rich profile does not. **No per-card injection channel exists today** — gap §8.1 in the new doc.
- `EmotionMusicalData` functions as an existing prototype of a complete identity pack (emotion-keyed rather than mode/genre-keyed).

**Gaps documented (all decisions explicitly deferred — no roadmap promotion):**
- §8.1 `TonalityProfileSO` not injectable per card — deferred; default profiles already produce distinct enough sonic results via characteristic degrees + vamp candidates + cadence rules for most identity-design cases.
- §8.2 No `ProgressionEffect` / `PaletteEffect` as `PartEffect` — deferred; `TrackActionDescriptor.styleBundle` via `BackingCardConfigSO` covers main design cases.
- §8.3 `RhythmCardConfigSO.styleIdOverride` semantically ambiguous (source has `// How would this work?` literal comment) — recommend not authoring cards against this field until closed.
- §8.4 `DensityEffect` / `FeelEffect` present in `PartEffect.cs` (inline sealed subclasses) but audible wiring not observed in composer code — UI-truth only (card face label via `GetLabel()` works, audible effect not verified).
- §8.5 `RhythmCardConfigSO` feel/density fields (`kickDensity`, `snareGhostNoteChance`, `hatSubdivisionBias`, `fillEveryNMeasures`, `lastMeasuresAsFill`) not semantically closed — tracked in MidiGenPlay roadmap Phase 9.

### Structural changes

**New doc file:**
- `planning/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` — created.

**Modified doc file:**
- `planning/Design_Starter_Deck_v1.md` — gains new "Design principle" section (see §1 of the session closure bundle for exact text).

**No code changes. No asset changes. No other existing doc semantic changes.**

**PK additions (project knowledge attachments for cross-project analysis):**

14 MidiGenPlay source files (ScriptableObjects that define the expressive surface + the handoff payload):
- `TrackStyleBundleSO.cs`, `RhythmCardConfigSO.cs`, `BackingCardConfigSO.cs`, `HarmonyCardConfigSO.cs`, `MelodyCardConfigSO.cs`
- `TonalityProfileSO.cs`, `ChordProgressionData.cs`, `ChordProgressionPaletteSO.cs`, `ChordProgressionLibrarySO.cs`
- `MelodicStyleSO.cs`, `PhrasePaletteSO.cs`, `PhraseArchetypeSO.cs`
- `EmotionMusicalData.cs`
- `SongConfig.cs`

5 MidiGenPlay SSoTs (observable contracts, not implementations):
- `SSoT_Runtime_Song_Model_and_Config.md`
- `SSoT_Composer_Backing_Track.md`
- `SSoT_Composer_Rhythm_Track.md`
- `SSoT_Composer_Melody_Track.md`
- `SSoT_Runtime_Generation_Orchestration.md`

Not uploaded (deliberately excluded for context discipline): composer `.cs` files, strategies, music theory utilities, editor windows, soundfonts, authoring docs beyond the two consulted (Chord Progressions, Rhythm Patterns).

### Authority changes

**One new design principle gains authoritative home.**

- Primary home for "mínimas cartas, máxima expresividad" is `Design_Starter_Deck_v1.md`. The principle is binding for starter deck authoring and recommended default for future composition-card authoring.
- The new expressive-surface doc references the principle but does not claim authority over it (one concept, one authority).

No other primary homes changed. Primary homes for every other concept touched remain unchanged:
- Card structure → `SSoT_Card_System`
- Card authoring contracts → `SSoT_Card_Authoring_Contracts`
- ALWTTT ↔ MidiGenPlay ownership → `SSoT_ALWTTT_MidiGenPlay_Boundary`
- Composition session bridge → `SSoT_Runtime_CompositionSession_Integration`
- Package runtime song model → `SSoT_Runtime_Song_Model_and_Config` (MidiGenPlay)
- Per-role composer contracts → `SSoT_Composer_{Rhythm,Backing,Melody}_Track` (MidiGenPlay)

### Doc edits applied (this closure)

- `SSoT_INDEX.md` — "Active planning docs" table gains one row for the new doc.
- `CURRENT_STATE.md` §5 — "Planning docs added" list gains one line for the new doc.
- `Design_Starter_Deck_v1.md` — gains new "Design principle" section.

**Deliberately not applied:**
- `coverage-matrix.md` — the design maxim lives in a planning doc, not a subsystem SSoT; planning-class authorities are not indexed in the coverage matrix per observed conventions.
- `Roadmap_ALWTTT.md` — no milestone scope or sequence change.
- `ssot_manifest.yaml` — no governed doc added; planning-class additions are not tracked there per observed conventions.
- `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §5 — new doc is not a legacy mixed doc; no classification needed.

### Lifecycle

- No milestone opened, closed, or re-scoped.
- No new batch tracked.
- M4.6 authoring status is unchanged (still gated on M4.1–M4.5 + runtime tuning values). The new doc and the captured design principle will inform the TBDs inside `Design_Starter_Deck_v1.md` §5.5–§5.8 when those batches unlock — and the next session (opt-2 design resolution) will begin that work preemptively.

### Known continuity gaps (items not promoted to any doc)

Session discussion outputs held in conversation only. Flagged here so future sessions can ask whether to capture:

- **Proposed axis choices for C2 and Sibi starter cards** — mid-session proposals (e.g., Synth Pad → minor-focused progression palette, Hook Theme → major-focused, Steady Beat on base 4/4, Four on the Floor as groove variant). Not accepted; live discussion only. Next session (opt-2 design resolution) will reconsider these from scratch against the now-captured maxim and the new matrix.

## 2026-04-24 — New planning/reference doc: MidiGenPlay expressive surface for ALWTTT cards

### Semantic changes

**New planning/reference doc:**
- `planning/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` — first single-source reference mapping the observable musical expressive surface available to ALWTTT composition cards against MidiGenPlay package contracts.
- Status: planning/reference — **not** a governed SSoT. References but does not redefine authority held by `SSoT_ALWTTT_MidiGenPlay_Boundary`, `SSoT_Runtime_CompositionSession_Integration`, `SSoT_Card_System`, `SSoT_Card_Authoring_Contracts`, and the 5 MidiGenPlay-side runtime/composer SSoTs.
- Primary contents: matrix of 26 expressive axes with ALWTTT carrier (PartEffect or styleBundle), SongConfig field reached, package SO consumed, and per-card controllability status (✅/⚠/❌); observed precedences from composer code; per-role bundle contracts; cross-track emergent mechanics; identity pack composition routes; 5 documented contractual gaps.

**Key observations captured (previously implicit, now explicit):**
- `BackingCardConfigSO` is the canonical home for chord-progression selection. `HarmonyCardConfigSO` is thin by design, not a gap (it carries leading + strategy for armonizing existing lines, not progression selection).
- `TrackActionDescriptor.styleBundle` and `List<PartEffect> modifierEffects` are parallel complementary channels on a composition card — one does not substitute the other.
- A chosen progression is shared across tracks of the same Part via `GenContext.SetProgressionForPart`, enabling one Backing card to coordinate harmony across Backing + Melody + Bass simultaneously.
- A progression with a restricted `tonalities` list can override `PartConfig.Tonality` at generation time (composer mutates `part.Tonality` in place).
- `TonalityProfileSO` is resolved package-side via `GenContext.GetTonalityProfileForPart`. Only `PartConfig.Tonality` (enum) crosses the handoff; the rich profile does not. **No per-card injection channel exists today** — gap §8.1 in the new doc.
- `EmotionMusicalData` functions as an existing prototype of a complete identity pack (emotion-keyed rather than mode/genre-keyed).

**Gaps documented (all decisions explicitly deferred — no roadmap promotion):**
- §8.1 `TonalityProfileSO` not injectable per card — deferred; default profiles already produce distinct enough sonic results via characteristic degrees + vamp candidates + cadence rules for most identity-design cases.
- §8.2 No `ProgressionEffect` / `PaletteEffect` as `PartEffect` — deferred; `TrackActionDescriptor.styleBundle` via `BackingCardConfigSO` covers main design cases.
- §8.3 `RhythmCardConfigSO.styleIdOverride` semantically ambiguous (source has `// How would this work?` literal comment) — recommend not authoring cards against this field until closed.
- §8.4 `DensityEffect` / `FeelEffect` present in `PartEffect.cs` (inline sealed subclasses) but audible wiring not observed in composer code — UI-truth only (card face label via `GetLabel()` works, audible effect not verified).
- §8.5 `RhythmCardConfigSO` feel/density fields (`kickDensity`, `snareGhostNoteChance`, `hatSubdivisionBias`, `fillEveryNMeasures`, `lastMeasuresAsFill`) not semantically closed — tracked in MidiGenPlay roadmap Phase 9.

### Structural changes

**New doc file:**
- `planning/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` — created.

**No code changes. No asset changes. No existing doc semantic changes.**

**PK additions (project knowledge attachments for cross-project analysis):**

14 MidiGenPlay source files (ScriptableObjects that define the expressive surface + the handoff payload):
- `TrackStyleBundleSO.cs`, `RhythmCardConfigSO.cs`, `BackingCardConfigSO.cs`, `HarmonyCardConfigSO.cs`, `MelodyCardConfigSO.cs`
- `TonalityProfileSO.cs`, `ChordProgressionData.cs`, `ChordProgressionPaletteSO.cs`, `ChordProgressionLibrarySO.cs`
- `MelodicStyleSO.cs`, `PhrasePaletteSO.cs`, `PhraseArchetypeSO.cs`
- `EmotionMusicalData.cs`
- `SongConfig.cs`

5 MidiGenPlay SSoTs (observable contracts, not implementations):
- `SSoT_Runtime_Song_Model_and_Config.md`
- `SSoT_Composer_Backing_Track.md`
- `SSoT_Composer_Rhythm_Track.md`
- `SSoT_Composer_Melody_Track.md`
- `SSoT_Runtime_Generation_Orchestration.md`

Not uploaded (deliberately excluded for context discipline): composer `.cs` files, strategies, music theory utilities, editor windows, soundfonts, authoring docs beyond the two consulted (Chord Progressions, Rhythm Patterns).

### Authority changes

**None.**

The new doc is planning/reference, not SSoT. It does not claim primary authority over any concept; it consolidates cross-references to already-governed homes. Primary homes for every concept touched remain unchanged:
- Card structure → `SSoT_Card_System`
- Card authoring contracts → `SSoT_Card_Authoring_Contracts`
- ALWTTT ↔ MidiGenPlay ownership → `SSoT_ALWTTT_MidiGenPlay_Boundary`
- Composition session bridge → `SSoT_Runtime_CompositionSession_Integration`
- Package runtime song model → `SSoT_Runtime_Song_Model_and_Config` (MidiGenPlay)
- Per-role composer contracts → `SSoT_Composer_{Rhythm,Backing,Melody}_Track` (MidiGenPlay)

### Doc edits applied (this closure)

- `SSoT_INDEX.md` — "Active planning docs" table gains one row for the new doc.
- `CURRENT_STATE.md` §5 — "Planning docs added" list gains one line for the new doc.

**Deliberately not applied:**
- `coverage-matrix.md` — no primary home changed.
- `Roadmap_ALWTTT.md` — no milestone scope or sequence change.
- `ssot_manifest.yaml` — no governed doc added; planning-class additions are not tracked there per observed conventions.
- `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §5 — new doc is not a legacy mixed doc; no classification needed.

### Lifecycle

- No milestone opened, closed, or re-scoped.
- No new batch tracked.
- M4.6 authoring status is unchanged (still gated on M4.1–M4.5 + runtime tuning values). The new doc is a design reference that will inform the TBDs inside `planning/Design_Starter_Deck_v1.md` §5.5–§5.8 when those batches unlock.

### Known continuity gaps (items not promoted to any doc)

Session discussion outputs held in conversation only. Flagged here so future sessions can ask whether to capture:

- **Proposed axis choices for C2 and Sibi starter cards** — mid-session proposals (e.g., Synth Pad → minor-focused progression palette, Hook Theme → major-focused, Steady Beat on base 4/4, Four on the Floor as groove variant). Not accepted; live discussion only. Natural home if locked: `planning/Design_Starter_Deck_v1.md` §5.5–§5.8.
- **Design maxim "mínimas cartas, máxima expresividad"** — the design principle that motivated this session. Captured implicitly in the new doc's framing. Natural home if promoted to explicit principle: `planning/Design_Starter_Deck_v1.md` intro or `planning/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` §1.

## 2026-04-24 — MB1 + MB2 joint closure: DevSetBandCohesion dispatch alignment + catalogue split

### Semantic changes

**SSoT_Dev_Mode:**
- §9.5 (P3.2 smoke-test table): ST-P32-4 and ST-P32-5 marked retroactively invalidated with pointer to §9.8. The 2026-04-23 PASS records were not honest observations — `GigManager.DevSetBandCohesion` never contained the `LoseGig()` dispatch described in §13.2/§13.3 until MB1 added it.
- §9.8 new: MB1 re-validation table (ST-MB1-1..4, all PASS 2026-04-24). Dispatch ON/OFF Infinite Turns, non-zero regression, production-build compile.
- §9.9 new: MB2 catalogue-split validation table (ST-MB2-1..6, all PASS 2026-04-24). Picker contents per character type, three status-key regressions (Shaken, Flow, Feedback), scene-reload reference integrity.
- §15.4 Catalogue scope finding: **Recommended resolution** replaced with **Resolved 2026-04-24 (MB2)** block specifying the two new catalogue assets, their contents, the empty-catalogue fallback-text UX note, and the deferred polish (distinguish null vs empty).

**CURRENT_STATE:**
- §1 Phase 3.2 block: amended to flag the code-vs-SSoT drift discovered and corrected 2026-04-24 via MB1. Historical claim about `DevSetBandCohesion(0)` dispatching `LoseGig()` annotated with MB1 pointer.
- §1 new block after P3.3b: "MB1 + MB2 — closed (2026-04-24)" summarizing both closures and pointing to `SSoT_Dev_Mode.md` §9.8, §9.9, §15.4. Declares the open-micro-batches list empty.
- §3: open-micro-batches item removed. M1.1 promoted to top of what-is-next. M1.1 decomposition (a/b/c) called out inline.
- §5: opening paragraph extended to log MB1+MB2 doc-edit application.

**Roadmap_ALWTTT:**
- §1.5 Open micro-batches block: replaced with "(none — both closed 2026-04-24)" and a two-paragraph joint-closure summary covering MB1 and MB2 outcomes + smoke-test references.
- `Last updated` bumped to `2026-04-24 (MB1+MB2 closure)`.

### Findings recorded (honesty correction)

- **ST-P32-4 and ST-P32-5 were never honest passes.** Recorded as PASS on 2026-04-23 (P3.2 closure), but the `LoseGig()` dispatch they claim to validate was never in code until MB1 landed 2026-04-24. Root cause: test observation error at P3.2 closure, or test recorded as aspirational/expected rather than observed. The symmetric-consequences principle (§13.3) was formulated correctly at P3.2; only the code and the test record were wrong. No feature depended on the dispatch during the interim (no user-facing bug). Retroactively invalidated; re-validated via ST-MB1-1..4.
- **Picker empty-catalogue fallback text is slightly misleading post-MB2.** `DrawStatusPicker` (line 388) treats `catalogue == null` and `catalogue.Effects.Count == 0` the same way — both show `(no catalogue — assign on prefab)`. After MB2, audience characters have a non-null but empty catalogue, so the fallback text lies about the prefab state. Harmless, but recorded as deferred UX polish in `SSoT_Dev_Mode.md` §15.4.

### Structural changes

**Modified files (code):**
- `Assets/Scripts/Managers/GigManager.cs` — `DevSetBandCohesion(int)` body gains `if (pd.BandCohesion == 0) LoseGig();` at the end. XML doc comment rewritten to match §13.2/§13.3 (symmetric-consequences principle, Infinite-Turns suppression branch explicit). Two-line net change. Still inside `#if ALWTTT_DEV`.

**New asset files:**
- `StatusEffectCatalogue_Musicians.asset` — 6 canonical SO references (flow, composure, choke, shaken, exposed, feedback).
- `StatusEffectCatalogue_Audience.asset` — empty `Effects` list at MVP. Earworm will be added at M4.3.

**Reassigned prefab references:**
- Every musician prefab: `CharacterBase.statusCatalogue` → `StatusEffectCatalogue_Musicians`.
- Every audience prefab: `CharacterBase.statusCatalogue` → `StatusEffectCatalogue_Audience`.
- Original `StatusEffectCatalogue.asset` retained in project (not deleted) pending confirmation no other references exist.

### Authority changes

None. §13.2/§13.3 (symmetric-consequences principle) was correctly documented since P3.2. MB1 brought the code into alignment with existing authority. MB2 is asset configuration only.

### Lifecycle

- MB1 closed.
- MB2 closed.
- Open-micro-batches list empty.
- M1 has one remaining item: M1.1 Deck Editor polish (decomposed into M1.1a/b/c per prior session decision).
- After M1 closure: M4 Starter Deck Foundations, first batch M4.1 Fix C1.

## 2026-04-24 — M1.5 Phase 3.3b closure: Dev Mode status apply/remove picker

### Semantic changes

**SSoT_Dev_Mode:**
- §3 Stats tab: block retitled "Stats tab (Phase 3.1–3.3b)". Per-Character section extended with status picker: active-status readout (`[−1]`/`[Clear]` per row) + catalogue-backed `[◄][►]` picker with `[+1]` apply.
- §6 Entry points: `DevStatsTab.cs` row reworded for P3.1+P3.2+P3.3a+P3.3b. `DrawStatusPicker(CharacterBase, ref int)` added. `using ALWTTT.Characters` directive noted. No new production-class entry points — phase uses existing `StatusEffectContainer` public API.
- §9.7 new: Phase 3.3b smoke-test results ST-P33b-1..10 (all PASS 2026-04-24).
- §15 new: Phase 3.3b — status apply/remove picker. Capability, no-production-patch rationale, gameplay-flag asymmetry (§15.3), catalogue scope finding (§15.4), smoke-test pointer, unblocks.

**CURRENT_STATE:**
- §1: new "Dev Mode Phase 3.3b — complete" block with closure summary and pointer to `SSoT_Dev_Mode.md` §15.
- §2: Phase 3 line updated — all phases closed; P3.4 deferred.
- §3: next-up list updated — top item is open micro-batches (comment drift + catalogue split), then M1.1.
- §5: doc-edit tracking paragraph updated to reflect P3.3b closure.

**Roadmap_ALWTTT:**
- §1.5: Phase 3.3b closure block added. Remaining list restructured to Deferred + Open micro-batches. Priority order item 6 marked complete.
- DoD: P3.3b item checked (ST-P33b-1..10 passed 2026-04-24).
- `Last updated` bumped to `2026-04-24 (P3.3b closure)`.

### Findings recorded

- **Shared catalogue on musician/audience prefabs:** the status picker shows all catalogue entries regardless of character type (musician-only statuses like Flow appear on audience members). Applying cross-type statuses is harmless but confusing. Recommended resolution: split into separate catalogue SOs — asset/prefab config only, no code change. Tracked as open micro-batch in `Roadmap_ALWTTT.md` §1.5.

### Structural changes

**Modified files (code):**
- `Assets/Scripts/DevMode/DevStatsTab.cs` — `DrawStatusPicker(CharacterBase, ref int)` method added. Static fields `_musicianStatusPickerIndex`, `_audienceStatusPickerIndex`. Call sites in `DrawMusicianEditor` (after Composure stepper) and `DrawAudienceEditor` (after MaxVibe stepper). `using ALWTTT.Characters` directive added.

### Authority changes

None. P3.3b does not introduce new authority — the `StatusEffectContainer` API and `CharacterBase.StatusCatalogue` property pre-existed. The picker is a UI surface only.

### Lifecycle

- M1.5 Phase 3.3b closed.
- M1.5 Phase 3 complete (P3.4 deferred).
- Open micro-batches: `DevSetBandCohesion` comment drift; catalogue split for musician/audience prefabs.
- Next M1 item: M1.1 Deck Editor polish.
- After M1 closure: M4 Starter Deck Foundations (first batch: M4.1 Fix C1).

## 2026-04-23 — M1.5 Phase 3.3a closure: Dev Mode per-character stat editing + Flow gig-wide

### Semantic changes

**SSoT_Dev_Mode:**
- §3 Stats tab: block retitled "Stats tab (Phase 3.1–3.3a)". Gig-Wide bullet extended with Flow stepper (uniform ± applied to every musician's `DamageUpFlat` stacks; aggregate via `GigManager.TotalFlowStacks`). New Per-Character bullet: musician (Stress slider, MaxStress stepper, Composure stepper via `TempShieldTurn` stacks) + audience (Vibe slider, MaxVibe stepper). Placeholder slot text removed.
- §6 Entry points: GigManager patches count 6 → 7. New seventh patch bullet for Phase 3.3a additions (`TotalFlowStacks`, `DevAddFlowToAllMusicians`). `DevStatsTab.cs` row reworded for P3.1+P3.2+P3.3a. `BandCharacterStats.cs` row expanded with Phase 3.3a surface (`CheckBreakdownThreshold`, `DevSetCurrentStress`, `DevSetMaxStress`). `AudienceCharacterStats.cs` row expanded with Phase 3.3a surface (`CheckConvincedThreshold`, `DevSetCurrentVibe`, `DevSetMaxVibe`) and folder-path corrected (`Characters/Audience/...`); Phase 3.1 `DevResetConvinced` annotated as implementation-landed-in-P3.3a.
- §9.6 new: Phase 3.3a smoke-test results ST-P33a-1..10 (all PASS 2026-04-23).
- §10 Update rule: new bullet for the Dev-setter animation-duration convention (`0.1f` as HealthBarController workaround).
- §13.1 small amendment: notes Flow row added in P3.3a (pointer to §14.4).
- §14 new: Phase 3.3a — per-character stat editing + Flow gig-wide. Capability, entry points, Composure-as-status-stack explanation, Flow gig-wide semantics, animation-duration workaround, smoke-test pointer, `DevResetConvinced` side-resolution, unblocks.

**CURRENT_STATE:**
- §1: new "Dev Mode Phase 3.3a — complete" block with closure summary, DevSet surface list, HealthBarController workaround note, and pointer to `SSoT_Dev_Mode.md` §14.
- §2: Phase 3 active-work line updated — P3.1, P3.2, and P3.3a closed; P3.3b remaining.
- §3: next-up list updated — top item is M1.5 Phase 3.3b.

**Roadmap_ALWTTT:**
- §1 priority order: Phase 3 line reads "P3.1, P3.2, and P3.3a closed; P3.3b remaining".
- §1.5: Phase 3.3a closure block added after Phase 3.2. "Remaining" list restructured — P3.3 replaced by P3.3b; micro-batch entry added for the `DevSetBandCohesion` comment drift.
- DoD: P3.3 line split — P3.3a item checked (ST-P33a-1..10 passed 2026-04-23); new P3.3b unchecked item.
- `Last updated` bumped to `2026-04-23 (P3.3a closure)`.

### Authority changes

**Threshold-check single source of truth (new):**
- `BandCharacterStats.CheckBreakdownThreshold()` and `AudienceCharacterStats.CheckConvincedThreshold()` are the single authoritative triggers for their respective sticky state transitions. `AddStress`, `AddVibe`, and all P3.3a `DevSet…` methods route through these helpers. This closes a previously-latent drift risk: Dev-path threshold logic could have silently diverged from play-path threshold logic. Governed in `SSoT_Dev_Mode.md` §14.2.

**Dev-setter animation-duration convention (new):**
- All P3.3a DevSet methods pass `duration: 0.1f` to the underlying clamp-setters. This is a documented workaround for `HealthBarController.SetCurrentValue(duration=0f)` not propagating the final value to the visual fill. Governed in `SSoT_Dev_Mode.md` §14.5 and §10. Revertable to `0f` when the underlying component is fixed.

### Doc-vs-code drift resolved

**`DevResetConvinced` implementation landed:**
- `SSoT_Dev_Mode.md` §6 and §7 have declared `AudienceCharacterStats.DevResetConvinced` as an existing Dev entry point since P3.1 closure. The method was never implemented. `DevModeController.ResetConvincedAudience` called it — compile-failed silently under `ALWTTT_DEV`. P3.3a added the implementation (sets `IsConvinced = false`, `ClearStatus(StatusType.Convinced)`) matching the §7 contract. Authority unchanged; code now matches.

### Structural changes

**Modified files (code):**
- `Assets/Scripts/Characters/Band/BandCharacterStats.cs` — `CheckBreakdownThreshold()` private helper extracted. `AddStress` refactored to call it. `#if ALWTTT_DEV` block extended with `DevSetCurrentStress(int)` and `DevSetMaxStress(int)`.
- `Assets/Scripts/Characters/Audience/AudienceCharacterStats.cs` — `CheckConvincedThreshold()` private helper extracted. `AddVibe` refactored to call it. New `#if ALWTTT_DEV` block with `DevResetConvinced()`, `DevSetCurrentVibe(int)`, `DevSetMaxVibe(int)`.
- `Assets/Scripts/Managers/GigManager.cs` — Phase 3.2 `#if ALWTTT_DEV` block extended: `TotalFlowStacks` getter + `DevAddFlowToAllMusicians(int delta)` method. Pre-guards `Apply(-N)` on zero-stack containers.
- `Assets/Scripts/DevMode/DevStatsTab.cs` — `DrawPerCharacterSection` method added (with `DrawMusicianEditor`, `DrawAudienceEditor`, `ApplyComposureDelta` helpers). Flow row added to `DrawGigWideSection`. Using directives extended (`ALWTTT.Characters.Audience`, `ALWTTT.Status`). Audience name display uses `AudienceCharacterData.CharacterName` (asymmetric with musician's `CharacterName` property).

### Findings recorded (not yet promoted to gameplay SSoT)

- **`HealthBarController.SetCurrentValue(duration=0f)` doesn't propagate the final value.** Latent bug in the UI component. No gameplay path uses `duration=0f`, so previously invisible. Exposed by P3.3a's Dev-driven jump-cut paths. Workaround (`0.1f` duration) used in P3.3a Dev setters. Recommend standalone ticket; pick up when HealthBarController is touched for another reason. Not a dedicated batch.
- **`GigManager.DevSetBandCohesion` code comment drift.** Inline comment says "intentionally does NOT dispatch LoseGig" while method body does dispatch `LoseGig()` on 0, consistent with `SSoT_Dev_Mode.md` §13.3 (symmetric-consequences principle). Comment is stale post-P3.2 reversal. Proposed as a separate micro-batch post-P3.3a — one-line comment fix, no behavior change.

### Lifecycle

- M1.5 Phase 3.3a closed.
- M1.5 Phase 3 remaining: P3.3b status apply/remove picker. P3.4 deferred.
- Post-P3.3a micro-batch: `DevSetBandCohesion` comment reconciliation.
- Next milestone after M1.5 closure: M1.1 Deck Editor polish.

## 2026-04-23 — M1.5 Phase 3.2 closure: Dev Mode gig-wide stat editing

### Semantic changes

**SSoT_Dev_Mode:**
- §3 Stats tab: block retitled "Stats tab (Phase 3.1–3.2)". New Gig-Wide Stats bullet added: SongHype slider `[0, MaxSongHype]`, Inspiration slider bound to `LiveInspiration`, BandCohesion stepper (floor 0, no upper cap). Idle-epsilon note for the SongHype slider.
- §6 Entry points: GigManager patches count changed 5 → 6. New patch bullet for the Phase 3.2 block (`MaxSongHype`, `LiveInspiration`, `DevSetSongHype`, `DevSetInspiration`, `DevSetBandCohesion`). `DevStatsTab.cs` row reworded for P3.1+P3.2. New `CompositionSession.cs` row under Modified Production Files: `CurrentInspiration` + `DevSetCurrentInspiration`.
- §10 Update rule: `CompositionSession` added to the gameplay-class list. New bullet for the `LiveInspiration` routing contract.
- §9.5 new: Phase 3.2 smoke-test results ST-P32-1..7 (all PASS 2026-04-23).
- §13 new: Phase 3.2 — gig-wide stat editing. Capability, entry points, the Dev Mode symmetric-consequences principle, the Inspiration dual-siting architectural finding, smoke-test pointer, unblocks.

**CURRENT_STATE:**
- §1: new "Dev Mode Phase 3.2 — complete" block with closure summary and pointer to `SSoT_Dev_Mode.md` §13.
- §2: Phase 3 active-work line updated — P3.1 and P3.2 closed; P3.3 remaining.
- §3: next-up list updated — top item is M1.5 Phase 3.3.

**Roadmap_ALWTTT:**
- §1.5: Phase 3.2 closure block added next to Phase 3.1; "Remaining" list shortened (P3.2 removed).
- §1 priority order: Phase 3 line reads "P3.1 and P3.2 closed; P3.3 remaining".
- DoD: P3.2 item checked (ST-P32-1..7 passed 2026-04-23); new P3.3 unchecked item.
- `Last updated` bumped to `2026-04-23 (P3.2 closure)`.

### Authority changes

**Dev Mode principle codified — symmetric consequences (new):**
- Governed in `SSoT_Dev_Mode.md` §13.3. Dev Mode mutations reproduce the natural gameplay consequences of the same state change. `DevSetBandCohesion(0)` dispatches `LoseGig()` the same way the natural Breakdown path does. Infinite-Turns suppression in `LoseGig`/`WinGig` is the correct escape hatch for scrubbing without gig-ending. This reverses the pre-implementation draft rule (which had specified no LoseGig dispatch from the Dev setter); the reversal is documented here as the first application of the principle.

**`LiveInspiration` routing contract established:**
- `SSoT_Dev_Mode.md` §13.2 + §13.4. During an active `CompositionSession`, the live Inspiration budget is `_session._currentInspiration`, not `pd.CurrentInspiration`. Dev reads/writes route to the session-active copy when one exists, and to PD otherwise, and write to both on set. This is the first governed statement of the Inspiration dual-siting reality in the code.

### Structural changes

**Modified files (code):**
- `Assets/Scripts/Managers/GigManager.cs` — new `#if ALWTTT_DEV` block after `AddSongHype`: `MaxSongHype` getter, `LiveInspiration` getter, `DevSetSongHype(float)`, `DevSetInspiration(int)` (routes to session when active), `DevSetBandCohesion(int)` (dispatches `LoseGig()` on 0).
- `Assets/Scripts/Runtime/CompositionSession.cs` — new `#if ALWTTT_DEV` block: `CurrentInspiration` getter + `DevSetCurrentInspiration(int)` method (sets `_currentInspiration`, calls `_ctx?.CompositionUI?.SetInspiration`).
- `Assets/Scripts/DevMode/DevStatsTab.cs` — `DrawGigWideSection` method added. Reads `gm.LiveInspiration` for the Inspiration slider (not `pd.CurrentInspiration`). Idle-epsilon on SongHype slider.

### Findings recorded (not yet promoted to gameplay SSoT)

- **Inspiration dual-siting.** `pd.CurrentInspiration` and `CompositionSession._currentInspiration` are not continuously synchronized. The session's field is reset at `Begin()` / `ConfirmCurrentPartAndStart()` to `_rules.inspirationPerPart` and is the value the composition cost gate and UI actually read. The PD field is persistent between sessions but invisible to gameplay during composition. `SSoT_Gig_Combat_Core` §4.2 may want a one-line note surfacing this implementation reality — not done in this batch. Deferred as a future doc pass.

### Lifecycle

- M1.5 Phase 3.2 closed.
- M1.5 Phase 3 remaining: P3.3 per-character stat editing. P3.4 deferred.
- M1 remaining after Phase 3: M1.1 Deck Editor polish.
- After M1: M4.1 Fix C1.

## 2026-04-23 — M1.5 Phase 3.1 closure: Dev Mode Breakdown entry point + T7 Shaken validation

### Semantic changes

**SSoT_Dev_Mode:**
- §3 Overlay: Stats tab added to tab toolbar (Infinite / Catalogue / Stats). Stats tab content documented: Breakdown section with musician selector grid, stress/status readout, Force Breakdown button.
- §6 Entry points: new file row `DevStatsTab.cs`. New method rows `BandCharacterStats.DevResetBreakdown()`, `MusicianBase.DevForceBreakdown()`.
- §9.3: T7 Shaken expiry moved from DEFERRED to ✅ PASSED (2026-04-23).
- §10 Update rule: Phase 3.1 triggers added.
- New §12 Phase 3.1 — Breakdown entry point: capability summary, entry points, unblocks, smoke tests.

**CURRENT_STATE:**
- §1: new "Dev Mode Phase 3.1 — complete" block. M1.2 multi-turn validation gap annotation updated to fully closed (T5/T8/T7 all passed).
- §2: Dev Mode Phase 3+ block updated to "Phase 3 — in progress", P3.1 closed, P3.2/P3.3 remaining.
- §3: M1.5 Phase 3 "what is next" entry updated.
- §4: T7 deferred risk replaced with "fully closed" note.

**Roadmap_ALWTTT:**
- §1.2: T7 outcome updated to PASSED.
- §1.5: Phase 3.1 closure line added.
- DoD: Breakdown entry point item checked.
- Last updated bumped.

### Structural changes

**New files:**
- `Assets/Scripts/DevMode/DevStatsTab.cs` — Phase 3.1 IMGUI helper. File-level `#if ALWTTT_DEV`. Static class; renders Stats tab body with Breakdown section. Musician selector grid, status readout from `StatusEffectContainer.Active`, Force Breakdown button dispatching to `MusicianBase.DevForceBreakdown()`.

**Modified files (code):**
- `Assets/Scripts/DevMode/DevModeController.cs` — `TabNames` array extended with "Stats". `DrawWindow` switch extended with `case 2: DevStatsTab.Draw()`.
- `Assets/Scripts/Characters/Band/BandCharacterStats.cs` — `DevResetBreakdown()` added inside `#if ALWTTT_DEV` block. Sets `IsBreakdown = false`.
- `Assets/Scripts/Characters/Band/MusicianBase.cs` — `DevForceBreakdown()` added inside `#if ALWTTT_DEV` block. Calls `DevResetBreakdown()` then `AddStress(MaxStress)`.

### Authority changes

- T7 Shaken expiry validated. M1.2 multi-turn validation gap fully closed — no remaining deferred tests from M1.2.
- Stats tab registered as Phase 3.1 overlay surface. P3.2/P3.3 sections will extend the same tab.

### Lifecycle

- M1.5 Phase 3.1 closed.
- M1.5 Phase 3 remaining: P3.2 gig-wide stat editing, P3.3 per-character stat editing. P3.4 deferred.
- M1 remaining after Phase 3: M1.1 Deck Editor polish.
- After M1: M4.1 Fix C1.

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

# SSoT — Tooltip Pipeline

**Status:** ACTIVE v1.0 — created by TIP-1-DOC (2026-09-15) under D-TIP-1=A. Registered in
`SSoT_INDEX.md`, `ssot_manifest.yaml` (three hard invariants), `coverage-matrix.md` and
`Repo_Tree_Index.md` in the same session, because an authority change that is not registered is not
an authority change.
**Path:** `Docs/systems/SSoT_Tooltip_Pipeline.md`
**Class:** subsystem_ssot

## 0. Scope and authority

This document owns **how a tooltip panel is shown, refreshed, placed and hidden**: the entries of
`TooltipManager`, the placement modes of `TooltipController`, the panel lifecycle of `TooltipText`,
and the anchoring contract every hover surface relies on.

It does **not** own:
- **what the text says** — `systems/SSoT_Game_Text.md` (storage, resolution, tags) and the
  meaning SSoTs behind it (Status_Effects, Card_System, Audience_and_Reactions);
- **which surface shows which tooltip and when** — the surface's own SSoT
  (`SSoT_Card_System.md` §10.2 card hover, `SSoT_Audience_and_Reactions.md` §6.4 audience canvas,
  `planning/Design_Vibe_Telegraph_v0_1.md` §10 C1/C3 hovers, `SSoT_Game_Text.md` §4 concept tags).

Governed code (by **file**, not by folder — the folder `Assets/Scripts/UI/Tooltips` is shared with
`SSoT_Game_Text.md`, which keeps `GameText.cs`, `GameTextTemplateIds.cs`,
`ConceptTooltipResolver.cs`, `ConceptTagRenderer.cs`):

| File | Role |
|---|---|
| `Assets/Scripts/UI/Tooltips/TooltipManager.cs` | entries: Show / Update / Hide; owns the panel pool and the fade |
| `Assets/Scripts/UI/Tooltips/TooltipController.cs` | placement: cursor-follow or static target; clamps |
| `Assets/Scripts/UI/Tooltips/TooltipText.cs` | one panel: header + body + background |
| `Assets/Scripts/UI/Tooltips/ITooltipTargetBase.cs`, `I2DTooltipTarget.cs` | consumer interface |
| `Assets/Scripts/UI/Tooltips/EconPipTooltipTarget.cs`, `VibeReadoutTooltipTarget.cs`, `VibeBarTooltipTarget.cs` | small target seams (behaviour of what they show belongs to their surface SSoT) |

Why a document of its own (D-TIP-1=A, 2026-09-15): the pipeline had eight consumers and no owner.
`ssot_manifest.yaml` assigned the folder to `SSoT_Game_Text.md`, whose §4 explicitly declined
ownership («TIP-1 owns it»); the anchoring rule lived in a *planning* note (Telegraph §10.6); and
the song panel (`TrackHoverPanel`, `CompositionContextRowUI`) wrote its own placement rather than
consume the pipeline. That is the failure mode an unowned pipeline produces.

## 1. Components and scene wiring

- `TooltipManager` — singleton, `DontDestroyOnLoad`, lives in **ALWTTTCore** on a
  **Screen Space – Overlay** canvas (sort order 10, above the tutorial overlay, below the card
  detail modal — D-TAG-8=A, `SSoT_Card_System.md` §10.3). Holds the `TooltipController`, the
  `TooltipText` prefab, the `CanvasGroup` used for the fade, and the active-language
  `ConceptGlossarySO` seat (D-TXT3-5=A — the seat is governed here, its *contents* by Game_Text).
- `TooltipController` — the column. `VerticalLayoutGroup` (Upper Left, spacing 5, control child
  size W+H) + `ContentSizeFitter` (Preferred, both axes): **content-sized**, which is the root of
  the assign→rebuild→measure→place rule (§4). Pivot and anchors `(0,0)`.
- `TooltipText` — one panel instance per stacked entry; pooled, never destroyed; header and body
  each toggle their own GameObject when empty.

## 2. Entries — the contract (D-TIP-3=B)

| Entry | Effect on the column | Fade | Placement |
|---|---|---|---|
| `ShowTooltip(body, header, target?, cam?, delay?, screenOffset?, growDown?, kind?)` | **pushes** one more panel (count+1), coloured by `kind` (§2.1) | restarts | re-evaluated from the arguments |
| `UpdateTooltip(body, header, panelIndex=0) : bool` | **rewrites** panel `panelIndex` in place; count unchanged; `false` if that panel is not shown | none | unchanged |
| `HideTooltip()` | clears: count 0, all panels inactive, alpha 0 | stops | — |
| `ShownTooltipCount` | read-only count | — | — |

**Stacking is a capability, not a leak (F-TXT4-11, F-BN-7 reframed).** `ShowTooltip` × N without
a `Hide` shows N panels in one column. Two surfaces depend on it: the card hover (one panel per
keyword and per unique status, `SSoT_Card_System.md` §10.2) and the audience canvas (status +
intention). A surface that wants exactly one panel calls `HideTooltip` before `ShowTooltip` (the
tutorial concept-tag hover, `SSoT_Game_Text.md` §4, `GAMETEXT-HOVER-SINGLE-PANEL`). A surface
whose content changes while hovered calls `UpdateTooltip` and falls back to `ShowTooltip` when it
returns `false` (the C1 readout, `VibeReadoutTooltipTarget`).

Why not a `stack: bool` parameter (D-TIP-3 option C) or a rename to `PushTooltip` (option A): both
would touch eight call sites to express a behaviour none of them wanted changed. The
explicitness lives in this table and in the file header of `TooltipManager.cs`; the new
entry (`UpdateTooltip`) is the only new behaviour.

**Panel width: 280 px (CARD-FACE-3, D3=B, 2026-09-22).** `TooltipText`'s `LayoutElement.preferredWidth`
went 250 → **280**, the only pipeline asset CARD-FACE-3 touched and an explicit exception to D-CF-2
(«the tooltip pipeline is not touched without an explicit decision»). Reason, from
`SSoT_Card_System.md` §10.2: the card-hover header binds glyph and name with U+00A0, and at 250 the
unit «glyph + Acompañamiento» overflowed by about one character, which made TMP break the word by
character. 280 was chosen over 300 because no other header in the catalogue came close to the limit
(ST-CF3-7), and over shrinking the header font because that bought ~20 px and cost the header/body
hierarchy. Every consumer of §5 is affected: panels are wider and wrap into fewer lines.
Regression-checked at ST-CF3-12 (audience stack of two panels and the C1 Vibe readout: complete, not
clipped at the screen edge).

### 2.1 Panel colour by kind (GIG-UI-1, 2026-11-03)

Each pushed panel carries a `TooltipKind` (`Default`, `Ability`, `Tastes`, `Blocked`, `Musician`,
`Status`, `Warning`). Colours (background, header, body) live in **one asset**, `TooltipStyleSO`
(`Assets/Settings/TooltipStyle.asset`), assigned on the `TooltipManager` prefab: tuning against the
gig art happens in one Inspector, not across prefabs (Claudio's request, Monster Train-style reading:
dark fill separates the panel from the art, the header colour says which kind it is).

- `Default`, or no asset → the `TooltipText` prefab look. `TooltipText.ApplyStyle` caches the prefab
  colours and restores them, so a pooled panel never keeps the previous kind's colour.
- A kind with no row in the asset → built-in starting colours in `TooltipStyleSO.BuiltIn`
  (`Musician`, `Status`, `Warning`) until someone adds the row.
- `ITooltipTargetBase.ShowTooltipInfo` keeps its six-parameter signature; the kind travels through
  a separate `CharacterCanvas.ShowTooltipInfo(..., TooltipKind)` overload (a seventh parameter on the
  interface member broke `CharacterCanvas`, CS0535).
- The asset also holds `audienceColumnOffset` (§3, grow-down anchor of the audience column).
- `Concept = 7` (S4b): panel appended for a linked concept; until the asset has a row it uses the
  `Status` style.

### 2.2 Concept links in every panel (S4b, D7=A, 2026-11-11)

- **Decorate on entry.** `ShowTooltip` and `UpdateTooltip` pass the body through
  `ConceptTagRenderer.Decorate` with the manager's `conceptColor` / `conceptBold` /
  `conceptUnderline`. Unknown ids warn once (`[S4b] Unknown concept link`). This is the only
  decoration point; callers send raw `<link=id>` text.
- **Append in `LateUpdate`.** `ShowTooltip` queues the ids its body links; after every caller of the
  frame has built its column, one `Concept` panel per id is pushed at the **end** of the column
  (`appendLinkedPanels`, `maxLinkedPanels` = 3). A concept whose name already heads a shown panel is
  skipped (prefix match: «Compostura x3» counts as Compostura). **Depth 1:** appended bodies are
  decorated but their links are not followed. `UpdateTooltip` never queues (the live Vibe readout
  calls it every refresh). `HideTooltip` clears the queue.
- **Consequence for surfaces that wanted one panel:** the tutorial concept hover (§5, «no, by
  invariant») now gets the panels linked by the concept's own body too — see **F-TIP-7**.

### 2.3 Concept panels on demand (S9c, D3=C, 2026-11-14)

- **Rule.** A linked concept panel (§2.2) shows by itself only in the first `autoShowFirstTimes`
  (= 2) hovers that bring it up, counted **per concept id, once per hover**, in PlayerPrefs
  (`alwttt.concept_seen.<id>`). After that it is **held back**: the column ends in a one-line hint
  (`ui_tip_concepts_hint`, «Mantén Shift para ver los conceptos») and the held-back panels appear
  while Shift (left or right) is held, and leave when it is released.
- **Order invariant.** Hint and on-demand panels are always the **last** panels of the column.
  `ShowTooltip` removes them before pushing; the next `LateUpdate` puts them back.
- **Scope.** Only linked concept panels. A card's own keyword and status panels are unchanged.
- **Switch.** `conceptPanelsOnDemand` (on), `conceptKey`/`conceptKeyAlt`, `showConceptHint` on
  `TooltipManager`. Logs: `[S9c] Concepto '…' visto n/2 veces sin Shift`, `[S9c] Shift: …`.
- **Reset.** Clearing PlayerPrefs (or changing the Company Name, which moves them) restarts the
  count — a new player sees the concepts again.

## 3. Placement (D-TIP-2=B)

Two modes, chosen per `ShowTooltip` call:

- **Cursor-follow** — no target: the column's bottom-left tracks `Input.mousePosition` every
  frame.
- **Static target** — the column's bottom-left sits at the target's **screen point** plus
  `screenOffset` (pixels), re-evaluated every frame (a target that moves or rescales is
  followed).

- **Static target, grow-down** (`growDown: true`, GIG-UI-1) — the target's screen point plus
  offset is the column's **top-left** instead of its bottom-left: the column hangs down from the
  anchor, so its first panel stays put however many panels follow. Used by the audience column
  (§5), anchored to the top-right corner of the member's body collider. Clamps are unchanged.

The target's screen point is obtained from its **root canvas**, inferred by the controller — the
caller never declares a coordinate space, because the space is a property of the transform, not
of the call site:

| Root canvas of the target | Projection |
|---|---|
| Screen Space – Overlay | `.position` **is** the screen point; no camera |
| Screen Space – Camera | `cam` argument, else `canvas.worldCamera`, else `Camera.main` |
| World Space, or no canvas | `cam` argument, else `Camera.main` (pre-TIP-1 behaviour, unchanged) |
| any of the above with no camera resolvable | falls back to cursor-follow (pre-TIP-1 behaviour) |

This closes **F-BN-8**: before TIP-1 every static target went through
`Camera.main.WorldToScreenPoint`, right for the world-space character canvases and wrong for an
overlay `RectTransform`, whose pixel position projected as if it were metres came back out of
range and pinned the panel to a corner. The runtime anchor of D-BN-18=B″ (`Design_Vibe_Telegraph`
§10.6) existed only to route around that and is **retired**.

**Clamp order rule.** Canvas-local position = screen point / canvas scale. Four clamps keep the
column inside the tooltip canvas; when they conflict the one applied **last** wins, and the order
is right → left, bottom → top, so **left and top win**: the column's first line, at its top-left,
carries the identity (the header) and is the last thing to sacrifice. This is the same rule
`TrackHoverPanel.PlaceNextTo` validated in camera (SONG-PANEL-1b): there, top wins over the
hand-zone clamp for the same reason. The pipeline clamps against the **canvas edge** only; it has
no notion of game zones (hand, audience). Those clamps are a surface decision and today exist only
in `TrackHoverPanel` (§5).

## 4. Lifecycle: assign → rebuild → measure → place

The column is content-sized, so after `SetText` its `rect` lags one layout pass behind. A clamp
computed against the stale rect places the panel with the **previous** tooltip's size (the shape
of **F-SP1-12**). Every text assignment — `ShowTooltip` and `UpdateTooltip` — therefore ends in
`TooltipController.Reposition()`: `LayoutRebuilder.ForceRebuildLayoutImmediate` on the column,
then one placement pass. **Unconditionally**: placement must not depend on hover history. `Update`
keeps placing every frame afterwards, which is why the stale-size frame was invisible in the fade
case (alpha ≈ 0) and would have been a visible one-frame jump in the update-in-place case.

Fade: `ShowTooltip` starts a coroutine that drives the manager's `CanvasGroup.alpha` 0 → 1 over
`showDelayTime` (0.5 s) through `fadeCurve`. Each `ShowTooltip` restarts it (stacked pushes in the
same frame are harmless). `UpdateTooltip` does not touch it. `HideTooltip` stops all coroutines
and drops alpha to 0.

## 5. Consumers (census 2026-09-15)

Through the pipeline:

| Surface | Calls | Mode | Stacks? |
|---|---|---|---|
| `CardBase.ShowTooltipInfo` (card hover) | Show × (keywords + unique statuses); unplayable panel with `kind: Warning` | cursor | yes, by design (§10.2) |
| `AudienceCharacterCanvas.RefreshHoverTooltip` (icon **or** body hover, GIG-UI-1) | Hide → Show × (Blocked, Ability, Tastes if revealed), one kind each | static, world-space, **grow-down** from the collider anchor | yes, by design |
| `BandCharacterCanvas.ShowTooltipInfo` (musician summary) | Show on `descriptionRoot`, `kind: Musician` | static, world-space | yes (status icons stack under it) |
| `AudienceCharacterCanvas.ShowVibeTooltip` via `VibeBarTooltipTarget` | Hide column → Show (`Default`) | static, world-space | no (composite stands down, D-BN-15) |
| `StatusIconBase` | Show on `transform`, `kind: Status` | static, world-space | no |
| `EconPipTooltipTarget` | Show | (as authored) | no |
| `VibeReadoutTooltipTarget` (C1) | Show on own `transform` + offset; then **Update** per refresh | static, **overlay** | no |
| `TutorialOverlayView` (concept tags) | Hide → Show, no target | cursor | no, by invariant — **broken by §2.2 when the body links (F-TIP-7)** |

**Audience hover, one content for two triggers (GIG-UI-1, 2026-11-03).** The member can be hovered
on the intent icon (UI pointer on the world-space canvas) or on the body (physics raycast in
`CharacterBase.Update`). Both set a flag and call the same `RefreshHoverTooltip`, which hides and
rebuilds the column, so overlapping icon and body never produce duplicate panels and both show the
same thing. This **reverses PRES-1 / D-PRES1-3=A** (ability and tastes in one panel): separate,
coloured panels read faster. `EconPipTooltipTarget` stays in the census but its pips are hidden in
the prefab (D-UI-3=A, `SSoT_Card_System.md` §10.5).

**Which character the pointer is on (GIG-UI-1).** Body hover and card targeting
(`HandController`) both go through `CharacterBase.RaycastTopmost`: of all colliders the ray hits,
the character **drawn on top** (sorting layer, then `sortingOrder`, then distance) wins. Plain
`Physics.Raycast` returned the collider nearest the camera in 3D, so where a back-stage musician's
box overlapped a front-stage one, the back one stole hover and targets. Not a pipeline component;
recorded here because it decides which tooltip opens.

**Outside the pipeline — conformant independent implementations:** `TrackHoverPanel` and
`CompositionContextRowUI` (song panel). They anchor to a row, not the cursor, and clamp against
two **game zones** the pipeline knows nothing about. They are governed where they are
(`SSoT_Card_System.md` §10.1 / `SSoT_Game_Text.md` §1 rows 7 and 9). Folding them into the
pipeline would require the pipeline to learn game-zone clamps and row anchoring; recorded as
**F-TIP-5** (open, not scheduled), not as a defect.

## 6. Relationship with other documents

- `systems/SSoT_Game_Text.md` — owns the text and the glossary seat's contents; its §4 hover
  invariant is expressed *in terms of* this document's entries (Hide → one Show, no target).
- `SSoT_Card_System.md` §10.2 — owns *what* the card hover stacks; this document owns *that*
  stacking exists and how it is placed.
- `SSoT_Audience_and_Reactions.md` §6.4 — owns the audience hover surfaces, including F-TXT4-8
  (bar visibility vs. interactivity), which is **not** a pipeline defect.
- `planning/Design_Vibe_Telegraph_v0_1.md` §10.6 — historical record of D-BN-18 B → B′ → B″; the
  anchoring truth now lives in §3 here.

## 7. Findings and open items

- **F-TIP-1** — `VibeBarTooltipTarget.cs` carries one non-ASCII byte (a mojibake dash) in its
  class comment: F-T6-1 family. Not edited by TIP-1; fix is a one-glyph replacement + `nonASCII=0`.
- **F-TIP-2 — VERIFIED AND CONFIRMED (TIP-1-DOC).** `GigCanvas.SetVibeReadout` pushes hardcoded ES
  player text (`"Vibe de la canción"` plus the sentences built in `BuildReadoutTooltip`, D-S5f-7=A)
  into the C1 tooltip, and **no row of `SSoT_Game_Text.md` §1 covered it**. It is now **population
  11**, monolingual and OPEN. Consequence recorded there: the SONG-PANEL-1 closing claim «met for
  code-authored text» is **narrowed**, not restated — code-authored text is resolved *in the
  surfaces that were censused*, and `GigCanvas` is a presentation file whose player text is a
  by-product of a telemetry feature. Assigned to **VIBE-READ-1**, which rewrites that function
  anyway for F-TIP-6. Not a pipeline defect; recorded here because the pipeline is what carries it.
- **F-TIP-3** — `TooltipText.GetLongestTextLength` reads `headerText.text` even when the header
  GameObject is inactive, so a stale header can enable the wrap `LayoutElement`. Cosmetic; recorded.
- **F-TIP-4** — `screenOffset` on `VibeReadoutTooltipTarget` is read when the hover opens, not
  every frame (the runtime anchor re-read it per frame). Live tuning needs a re-hover. Accepted.
- **F-TIP-5** — song-panel surfaces outside the pipeline (§5). Open, unscheduled.
- **F-TIP-6 — the Vibe breakdown prints steps that change nothing. OPEN → VIBE-READ-1.** Observed at
  ST-TIP-10: four of six lines inert (`× 1,00` for tastes, Flow and Captivated, `+ 0` for SFX), and
  the same shape on the C1 readout (`+ SFX del local → 0`). This **reverses by observation** the
  criterion written on `AudienceCharacterCanvas.BuildVibeText` itself — *every line is shown even
  when neutral: the shape teaches the model* — on the ground that the pedagogy is paid at every
  hover and collected once; the same way D-BN-9 was reverted. Agreed rule: suppress a line when its
  **output equals its input**, with the first line (Hype) and the last (final) always present and
  the `Blocked` / `Indifferent` branches untouched, because those explain a zero. **Not a pipeline
  change**: the pipeline carries whatever text it is handed, and `Design_Vibe_Telegraph_v0_1.md`
  §10.2 + `SSoT_Audience_and_Reactions.md` §6.4 own the content. Recorded here only because the
  smoke that found it belongs to this batch.
- **F-TXT4-8** — assigned to `SSoT_Audience_and_Reactions.md` §6.4; untouched by TIP-1.
- **ST-TXT4-13** — inline sprite in a tooltip: still deferred until a sprite atlas exists.

- **F-TIP-7 — the tutorial concept hover can now stack (S4b, OPEN, unverified).** §2.2 appends the
  concepts linked by any pushed body. A tutorial hover over «Stress» shows its body, which links
  Colapso, so a second panel is appended. Not seen in a smoke. If it reads wrong, give `ShowTooltip`
  a `followLinks: false` parameter for that surface.

## 8. Update rule

Update this document when a change affects: the entries or their stacking semantics; the
placement modes or the projection table; the clamp order rule; the assign→rebuild→measure→place
rule; the fade; the governed file list; or when a surface joins or leaves the pipeline.

## 9. Change record

| Date | Batch | Change |
|---|---|---|
| 2026-09-15 | TIP-1 / TIP-1-DOC | Document created under **D-TIP-1=A**, reversing BIGNUM-1-DOC's decision not to create it — with a new argument, not a new preference: that batch did not touch the contract and this one did, and SONG-PANEL-1 had since added a third tooltip surface that wrote its own placement. Closes **F-BN-7** (`UpdateTooltip`, D-TIP-3=B) and **F-BN-8** (anchor space inferred from the root canvas, D-TIP-2=B); retires the **D-BN-18=B″** runtime anchor; reframes **F-TXT4-11** from defect to declared capability. Records the clamp order rule and the assign → rebuild → measure → place rule. Consumer census (8 in, 2 conformant outside). Findings F-TIP-1..6. Authority taken over eight files previously governed wholesale by `SSoT_Game_Text.md`. ST-TIP-1..10 PASS. |
| 2026-09-22 (doc 2026-09-25) | CARD-FACE-3 / CARD-FACE-3-DOC | §2 → panel width 250 → **280** (`TooltipText` `LayoutElement.preferredWidth`), D3=B, explicit exception to D-CF-2. No transport contract changed; no invariant touched. |
| 2026-11-11 | S4b (DEMO-1211, LEGIB-1b hover views) | New §2.2: concept links decorated in every panel and linked concepts appended as `TooltipKind.Concept` panels (depth 1, max 3, dedup by header). `Concept = 7` in §2.1. §5 census: tutorial row flagged (F-TIP-7, unverified). Musician hover column: body and icon show the same column. |
| 2026-11-03 | GIG-UI-1 (DEMO-1211 S2) | §2 `ShowTooltip` gains `growDown` and `kind`; new §2.1 (`TooltipStyleSO` / `TooltipKind`, one colour asset). §3 grow-down placement. §5 census: audience column (icon and body show the same, reverses D-PRES1-3=A), musician summary, Warning on cards, Status on icons; `CharacterBase.RaycastTopmost` (topmost drawn character wins hover and targeting). Played by Claudio in the editor. |
| 2026-11-14 | S9c DEMO-FIX-1 | New §2.3: linked concept panels on demand (first 2 hovers per concept by themselves, then Shift; hint line last). Smoke 3 played by Claudio. |

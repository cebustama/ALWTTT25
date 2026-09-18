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
| `ShowTooltip(body, header, target?, cam?, delay?, screenOffset?)` | **pushes** one more panel (count+1) | restarts | re-evaluated from the arguments |
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

## 3. Placement (D-TIP-2=B)

Two modes, chosen per `ShowTooltip` call:

- **Cursor-follow** — no target: the column's bottom-left tracks `Input.mousePosition` every
  frame.
- **Static target** — the column's bottom-left sits at the target's **screen point** plus
  `screenOffset` (pixels), re-evaluated every frame (a target that moves or rescales is
  followed).

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
| `CardBase.ShowTooltipInfo` (card hover) | Show × (keywords + unique statuses) | cursor | yes, by design (§10.2) |
| `CharacterCanvas` / `AudienceCharacterCanvas` (body hover) | Show × (status, intention, …) on `descriptionRoot` | static, world-space | yes, by design |
| `AudienceCharacterCanvas.ShowVibeTooltip` via `VibeBarTooltipTarget` | Show on `descriptionRoot` | static, world-space | no (composite stands down, D-BN-15) |
| `AudienceCharacterCanvas.ShowBlockedTooltip` | Show | static, world-space | no |
| `StatusIconBase` | Show on `transform` | static, world-space | no |
| `EconPipTooltipTarget` | Show | (as authored) | no |
| `VibeReadoutTooltipTarget` (C1) | Show on own `transform` + offset; then **Update** per refresh | static, **overlay** | no |
| `TutorialOverlayView` (concept tags) | Hide → Show, no target | cursor | no, by invariant |

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

## 8. Update rule

Update this document when a change affects: the entries or their stacking semantics; the
placement modes or the projection table; the clamp order rule; the assign→rebuild→measure→place
rule; the fade; the governed file list; or when a surface joins or leaves the pipeline.

## 9. Change record

| Date | Batch | Change |
|---|---|---|
| 2026-09-15 | TIP-1 / TIP-1-DOC | Document created under **D-TIP-1=A**, reversing BIGNUM-1-DOC's decision not to create it — with a new argument, not a new preference: that batch did not touch the contract and this one did, and SONG-PANEL-1 had since added a third tooltip surface that wrote its own placement. Closes **F-BN-7** (`UpdateTooltip`, D-TIP-3=B) and **F-BN-8** (anchor space inferred from the root canvas, D-TIP-2=B); retires the **D-BN-18=B″** runtime anchor; reframes **F-TXT4-11** from defect to declared capability. Records the clamp order rule and the assign → rebuild → measure → place rule. Consumer census (8 in, 2 conformant outside). Findings F-TIP-1..6. Authority taken over eight files previously governed wholesale by `SSoT_Game_Text.md`. ST-TIP-1..10 PASS. |

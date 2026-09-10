# SSoT_Game_Text — ALWTTT

**Class:** subsystem SSoT · **Created:** 2026-09-10 (TXT-2, D-TAG-9=A) · **Status:** active
**Last substantive change:** 2026-09-10 (TXT-3) — populations 2 and 3 **moved** into the glossary;
§3 collapsed to a single registry. See §9 for the change record.

**Scope.** This document owns the *plumbing* of player-facing text in ALWTTT: which registry
stores which text, how text points at a concept, how a concept resolves to a tooltip, and how the
authoring pipeline round-trips it. It does **not** own what any concept means — that stays with the
subsystem SSoTs (`SSoT_Status_Effects.md`, `SSoT_Card_System.md`, `SSoT_Scoring_and_Meters.md`,
`SSoT_Gig_Combat_Core.md`). This split is the reason the document exists: TXT-2 needed a home for
"where does the text live" without that becoming a second opinion on "what does Vibe mean".

**Why this is an authority document and not a section of `SSoT_Editor_Authoring_Tools.md`.** The
tool inventory documents a window; §3 below documents a *runtime* resolution order that decides
which of three registries answers a hover. That is code truth with a precedence rule, and
precedence rules that live in a tool inventory get lost. `SSoT_Editor_Authoring_Tools.md` §20
remains the operational home of `GameTextWindow`; this document is referenced from it, not
duplicated into it.

---

## 1. Population map — where player-facing text lives today

Three populations carry player text today. Two more are named here because they are **not** solved
and every localization decision has to count them.

| # | Population | Storage | Language slots | Authoring surface | Authority for meaning |
|---|---|---|---|---|---|
| 1 | Tutorial copy (`revisitTitle`, `pages[]`, `mechanicText`) | `TutorialDialogSO` inside `TutorialDialogCatalogSO`, one catalog per language | **N** (D-S5f-2=B, D2=B) | `GameTextWindow ▸ Tutorial` + CSV | `Design_Tutorial_System_v0_3.md` |
| 2 | **Concept text — meters, game objects, statuses and card keywords** | `ConceptGlossarySO`, one asset per language | **N** | `GameTextWindow ▸ Concepts` + CSV | the subsystem SSoT of each concept; this asset stores wording only |
| 3 | Card names (`CardDefinition.displayName`) | `CardDefinition` | **1** | `GameTextWindow ▸ Cards` (read-only) | `SSoT_Card_System.md` |
| 4 | **Card descriptions — no storage at all** | generated in code by `CardEffectDescriptionBuilder` from the effect specs (`SSoT_Card_System.md` §10.1) | **1**, implicitly English | none | — |
| 5 | **Hardcoded strings in C#** | literals in components — the Vibe-breakdown lines, the effectiveness labels and `ShowBlockedTooltip` copy in `AudienceCharacterCanvas` (D-S5f-7=A / D-S5f-8=A) | **1**, implicitly Spanish | none | — |

**Population 2 is the TXT-3 result, and it is a MOVE.** Until 2026-09-10 status text lived on
`StatusEffectSO` (`DisplayName` / `Description`) and card-keyword text on `SpecialKeywordData`
(`contentText`), each with one language slot — the asymmetry recorded as **T-TAG-1**. TXT-3 moved
all of it into the glossary (**D-TXT3-0=B**, an authority move, never a copy):

- `StatusEffectSO.description` was **removed**. `StatusEffectSO.displayName` was **redefined as a
  developer label** — asset file name (`StatusEffect_{DisplayName}_{EffectId}` via `OnValidate`),
  log lines, editor lists, and the seed for `statusKey`. **No player surface may read it.**
- `SpecialKeywordData` (asset and class) was **retired**. The `SpecialKeywords` enum remains the id
  space for card keywords; it needs no registry asset to carry text.
- The rejected alternative was per-language fields on `StatusEffectSO`: it puts translation inside a
  gameplay asset and requires a script change per language.

Glossary ids for the moved text are the keys those registries already owned: a status entry's id is
its `StatusKey`, a keyword entry's id is its enum name lowercased. Nothing new had to be invented,
and a status that gains an entry gains it under the name the rest of the codebase already uses.

**Four player surfaces consume population 2**, all through `ConceptTooltipResolver` (§3): the status
icon hover (`StatusIconBase`), the card hover (`CardBase.ShowTooltipInfo`), the `<link=id>` tag in
tutorial text, and the **status floater** (`SensoryFtPresentation.TryBuildStatusAppliedFt`, TXT-3).
The floater is the one that is easy to forget, because it reads a *name* and not a tooltip; it was
found only at batch close (**F-TXT-3-1**) and would otherwise have drawn `+COMPOSURE` over a Spanish
tooltip reading «Compostura».

**Populations 3, 4 and 5 are findings, not designs.** 3 and 4 are **F-TXT-3-5** (card names and card
descriptions have one implicit language); 5 is **F-TXT-2-1**, unreachable by any authoring tool and
invisible to parity checks. None of the three is fixed by a per-SO scheme, including TXT-3's. All
three are mandatory inputs to **O-TXT-3** and the scope of the batch that follows it (**TXT-4**).

## 2. Concept tags — syntax

A concept tag marks, inside player-facing text, a word or phrase naming a game concept. The tag
carries an **id**, never a definition.

```
<link=ID>visible text</link>
```

- `ID` matches `[a-z0-9_]+`, is **language-neutral**, and is identical across language columns. The
  visible text is translated; the id is not. `<link="ID">` is accepted and normalised by TMP; the
  unquoted form is canonical (it keeps quote-doubling out of CSV cells).
- The stored form is **TMP-native**. A surface that renders the raw string without the decorator
  paints the visible text plain — a bare `<link>` has no visual effect — never literal brackets.
  This is the whole reason `<link>` was chosen over a private syntax (**D-TAG-1=A′**): the failure
  mode of a forgotten preprocessor is invisible instead of broken.
- Decoration happens in exactly one place, `ConceptTagRenderer.Decorate`, called by a surface
  **after** it resolves `{$token}`s. Known ids become
  `<link=ID><color=#HEX><u>text</u></color></link>`; colour and underline are serialized fields on
  the consuming surface (`TutorialOverlayView.conceptColor`, `conceptUnderline`). Unknown ids are
  left untouched — plain, no hover — and logged once per id per session.
- Tokens (`{$…}`, `TutorialTokenResolver`) and tags are independent mechanisms; both survive the CSV
  round-trip unchanged, and a token may appear inside a tag's visible text.
- Authoring rule: tag the **first** occurrence of a concept in a cell; later mentions stay plain.

## 3. Resolution (D-TXT3-4=A) — one registry

`ConceptTooltipResolver` answers from **one** source: the `ConceptGlossarySO` of the running
language, keyed by id, case-insensitive.

| Entry point | Caller | Key |
|---|---|---|
| `Resolve(id, out header, out body)` | tag surfaces (`TutorialOverlayView`) | the id inside `<link=id>` |
| `TryGetStatusText(StatusEffectSO, …)` | `StatusIconBase`, `CardBase`, `SensoryFtPresentation` | `StatusKey` |
| `TryGetKeywordText(SpecialKeywords, …)` | `CardBase` | enum name, lowercased |

**Where the glossary comes from.** An instance built with a glossary uses it (tutorial surfaces get
theirs by inspector, next to their catalog). Otherwise, and always for the static entry points, the
resolver reads `TooltipManager.ConceptGlossary` — a serialized field on the manager, which is the
seat `SpecialKeywordData` used to occupy (**D-TXT3-5=A**). `TooltipManager` is a
`DontDestroyOnLoad` singleton, so any surface capable of showing a tooltip has it; reading it at
resolve time rather than at construction is deliberate, because the manager may not exist yet when a
consumer builds its resolver.

**The single registry is the invariant.** TXT-2's invariant was the *order* of three registries;
this document's invariant since TXT-3 is that there is only one place a concept's wording can come
from. No migration fallback to `StatusEffectSO` or the keyword asset was retained: with the move
applied atomically there is nothing to fall back to, and a dormant fallback is an invitation to
re-fill the field it reads.

**Miss policy — visible, never silent.** An id with no glossary entry resolves to `header = the raw
id`, `body = ""`, and logs **once per id per session**. It never falls back to
`StatusEffectSO.displayName`: that would quietly make the developer label player text again, which
is precisely what TXT-3 removed. A tag surface additionally leaves the word undecorated (§2), so a
missing concept reads as plain text rather than a broken link.

**Coverage replaces collision.** Under TXT-2, an id claimed by both a status and the glossary was an
error (`DUPLICATE: … owns it`) and the fix was deleting the glossary entry. Since TXT-3 that overlap
is **required**: every `StatusKey` and every `SpecialKeywords` name must have an entry, with name and
description, in every glossary. The gap — not the overlap — is the error, and it is reported as a
block (`COVERAGE`) rather than per row, because a missing id has no row to badge
(`SSoT_Editor_Authoring_Tools.md` §20.11).

**Language pairing is unenforced (R-1).** `TooltipManager` and `TutorialController` each receive a
glossary by inspector, and nothing checks that they match — runtime never reads `languageCode`
(D-S5f-2=B) and TXT-3 did not break that rule to add a check. Mismatched assignment shows a
Spanish tutorial with English tooltips, which is exactly the symptom TXT-3 removed. The batch that
lets `TutorialController` drop its own field and fall through to the manager (**TXT-3b**) closes
this by construction.

## 4. Hover behaviour (D-TAG-5=D)

`TutorialOverlayView.Update` tests the pointer against link geometry with
`TMP_TextUtilities.FindIntersectingLink`. No raycast is involved, so the modal's `blocksRaycasts`
and the tutorial input gates are irrelevant, and characters not yet revealed by the typewriter
(`maxVisibleCharacters`) do not intersect and cannot be hovered.

On a link **change** — enter, leave, or a jump straight from one link to the next — the surface
performs exactly one `TooltipManager.HideTooltip()` followed by at most one
`ShowTooltip(body, header)` with **no target transform**.

Both halves of that sentence are load-bearing against the two open defects of TIP-1:

- `TooltipManager.ShowTooltip` appends a panel per call and only `HideTooltip` resets the counter
  (**F-BN-7**). Because a change is always Hide→Show and never Show→Show, panels cannot stack.
- Passing no transform routes `TooltipController.SetFollowPos()` to its cursor-following branch, so
  the static-anchor projection that misbehaves under Screen Space – Overlay (**F-BN-8**) is never
  entered.

The fade-in therefore replays on every new word. That cost was accepted rather than paid for by
touching `TooltipManager`, which TIP-1 owns. **TIP-1 remains open and independent of this document.**

`Hide()` and `ShowPage()` both clear the hover, so no tooltip survives a closed modal or a page turn.

**Canvas order (D-TAG-8=A, applied 2026-09-10).** The tooltip canvas must sort **above** the tutorial
overlay canvas, and the card-detail modal canvas must remain above the tooltip canvas
(`SSoT_Card_System.md` §10.3). Verified at ST-TAG-1: with the original order the tooltip rendered
behind the dialog box and read as "no tooltip".

## 5. Vocabulary (D-TAG-3=B, extended by TXT-3)

**36 ids, one registry.** The three groups below describe where an id *comes from*, not where its
text lives — since TXT-3 all of it lives in `ConceptGlossarySO`.

| Group | Id origin | ids |
|---|---|---|
| Status (12) | `StatusEffectSO.StatusKey` | `captivated` · `choke` · `composure` · `earworm` · `exposed` · `feedback` · `flow` · `hyped` · `indifference` · `shaken` · `spotlight` · `voltage` |
| Card keyword (7) | `SpecialKeywords` enum name, lowercased | `consume` · `convinced` · `ethereal` · `exhaust` · `stress` · `tall` · `vibe` |
| Glossary-native (17) | nowhere else — meters and game objects | `hype` · `inspiration` · `cohesion` · `gig` · `song` · `loop` · `period` · `play` · `track` · `song_end` · `stage` · `budget` · `composition_card` · `action_card` · `sound_card` · `breakdown` · `blocked` |

An id in the first two groups is **required** to exist (§3, coverage). An id in the third exists
because someone authored it. Only 24 of the 36 are currently reachable by a `<link=id>` tag; the
other 12 are reached by an icon hover, a card hover or a floater. **An id with no tag is not an
orphan** — the surface that consumes it just is not text.

`blocked` stays glossary-native because the Blocked tint has no `StatusEffectSO` and no icon
(Decision E3, `SSoT_Status_Effects.md`). If one is ever created with `StatusKey == "blocked"`, the
entry stops being glossary-native and becomes the status entry for that key — same id, same text,
no edit required. That is a consequence of the single registry that the TXT-2 rule (delete the
glossary entry) got exactly backwards.

**Surfaces in v1 (D-TAG-4).** Tag text: `TutorialDialogSO.mechanicText` only (24 ids, EN + ES).
Capability: any surface may call `ConceptTagRenderer.Decorate`; voiced pages are untagged by choice,
card descriptions and status descriptions are untagged and out of scope until a later batch.

## 6. Failure modes

| Situation | Editor | Runtime |
|---|---|---|
| id in the glossary of no language | `TAG unknown: id` | plain text, no hover, one warning per id per session |
| `StatusKey` or keyword name with no glossary entry | `COVERAGE — status 'x' — missing: es` (block) + `NO GLOSSARY <lang>` on the Status Effects tab | tooltip/floater shows the raw id, one warning per id per session |
| entry present in one language, absent in the other | `MISSING <lang>` | that language shows the raw id |
| tag present in one language, absent in the other | `MECHANIC TAGS differ` / `TAGS differ` | that language renders the word plain |
| `<link=` without `</link>` | `TAG unclosed <lang>` | TMP extends the link to the end of the text |
| glossary not assigned on `TooltipManager` | — | every status/keyword surface shows raw ids; one log line per id |
| glossary not assigned on a tag consumer | — | tag ids render plain; one log line at `SetConceptSources` |
| `TooltipManager` absent | — | tags render styled, hover shows nothing |
| glossaries of two different languages assigned to manager and tutorial | — | **nothing** — R-1, §3 |

Parity of tags is checked exactly like parity of `{$token}`s: the **set** of ids must match across
languages. A translation that silently drops a tag is a hole in the UI that reading the copy would
never reveal.

## 7. Open decisions and findings

- **O-TXT-3 (inherited from TXT-1) — Unity Localization or project-owned catalogs.** Still deferred.
  TXT-2 supplied the case TXT-1 said it was waiting for and the answer taken was project-owned
  (D-TAG-2b=(i)); TXT-3 then consolidated **two** categories onto that pattern instead of three
  registries, which makes a future migration one conversion rather than three — the cost moved, it
  did not disappear. The decision must still count populations 3, 4 and 5 of §1, which no per-SO or
  per-glossary scheme reaches.
- **TXT-4 (opened by this batch).** Card names, generated card descriptions and the hardcoded
  literals. Not a repeat of TXT-3: population 4 has **no strings to move**, so the batch has to
  create the per-language fragment registry that `CardEffectDescriptionBuilder` composes from.
- **TXT-3b.** `TutorialOverlayView.SetConceptSources` still calls the `[Obsolete]` two-argument
  constructor, and `TutorialController` still holds `conceptStatusCatalogues`. Compiles with one
  `CS0618` warning by design. Closing it also closes R-1 (§3).
- **F-TXT-3-6 — card-face information density.** Keywords resolve correctly on hover but do not
  appear on the card face or in the detail modal; track type and modifiers occupy the space. This is
  presentation, not text plumbing (`SSoT_Card_System.md` §10.1/§10.3), and any move of information
  from face to tooltip raises the stakes of **TIP-1**, still open.
- **D-TAG-10 — CLOSED 2026-09-10 by TXT-3.** T-TAG-1 resolved: no player-facing surface reads a
  single-slot text field for statuses or keywords.

## 8. Update rule

Update this document when a change affects: which population stores a given kind of player text; the
tag syntax; the resolution path or its registry; the miss policy or the coverage rule; the hover
contract or its relationship to TIP-1; the parity checks; or the CSV interchange contract for text.

**Related authority.** `SSoT_Editor_Authoring_Tools.md` §20 — `GameTextWindow` behaviour and the CSV
codec. `Design_Tutorial_System_v0_3.md` §5A — tutorial copy voice and `mechanicText` register.
`SSoT_Status_Effects.md` §3.3 — status meaning, icon hosts, and the explicit cession of player text.
`SSoT_Card_System.md` §3.3, §10.2 — keyword meaning and card-hover assembly.
`Design_Sensory_Contract_v0_1.md` — the status floater as a text surface (planning).

## 9. Change record

| Date | Batch | Change |
|---|---|---|
| 2026-09-10 | TXT-2 | Document created (D-TAG-9=A). Tag syntax, three-registry resolution order, hover contract, parity, CSV. |
| 2026-09-10 | TXT-3 | §1 populations 2 and 3 **moved** into the glossary and the table renumbered; §3 rewritten from a three-registry order to a single registry with a miss policy and a coverage rule; §5 vocabulary 24 → 36 ids in three origin groups; §6 rewritten around coverage and language pairing; §7 D-TAG-10 closed, TXT-4 / TXT-3b / F-TXT-3-6 opened. `StatusEffectSO.description` and `SpecialKeywordData` no longer exist. ST-TXT3-1..12 PASS. |

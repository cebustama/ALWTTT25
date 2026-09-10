# SSoT_Game_Text — ALWTTT

**Class:** subsystem SSoT · **Created:** 2026-09-10 (TXT-2, D-TAG-9=A) · **Status:** active

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

Four populations, established by TXT-1, TUT-TXT-1 and TXT-2, plus one that predates all of them.

| # | Population | Storage | Language slots | Authoring surface | Authority for meaning |
|---|---|---|---|---|---|
| 1 | Tutorial copy (`revisitTitle`, `pages[]`, `mechanicText`) | `TutorialDialogSO` inside `TutorialDialogCatalogSO`, one catalog per language | **N** (D-S5f-2=B, D2=B) | `GameTextWindow ▸ Tutorial` + CSV | `Design_Tutorial_System_v0_3.md` |
| 2 | Status player text (`DisplayName`, `Description`) | `StatusEffectSO` | **1** | `GameTextWindow ▸ Status Effects` (read-only) | `SSoT_Status_Effects.md` §3.3 |
| 3 | Card keyword text | `SpecialKeywordData` (`SpecialKeywordBase.contentText`) | **1** | Inspector only | `SSoT_Card_System.md` §3.3 |
| 4 | Concept glossary (meters + game objects) | `ConceptGlossarySO`, one asset per language | **N** | `GameTextWindow ▸ Concepts` + CSV | the subsystem SSoT of each concept; this asset stores wording only |
| 5 | **Hardcoded strings in C#** | literals in components — e.g. the Spanish Vibe-breakdown lines and `ShowBlockedTooltip` copy in `AudienceCharacterCanvas` (D-S5f-7=A) | **1**, implicitly Spanish | none | — |

Population 5 is a finding, not a design: **F-TXT-2-1**, recorded at TXT-2. It is unreachable by any
authoring tool and invisible to parity checks. It is not migrated by TXT-3; it is registered so the
next localization decision counts it.

Card **descriptions** are a sixth case with no storage at all: they are generated in code by
`CardEffectDescriptionBuilder` (`SSoT_Card_System.md` §10.1). Giving them a language slot is part of
the same open decision, **O-TXT-3**.

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

## 3. Resolution order (D-TAG-2=C) — the load-bearing rule

`ConceptTooltipResolver.Resolve(id)` consults, **in this order, first hit wins**:

| Order | Registry | Key | Header / body | Languages |
|---|---|---|---|---|
| 1 | `StatusEffectSO` via the configured `StatusEffectCatalogueSO` list | `StatusKey`, case-insensitive | `DisplayName` / `Description` | 1 |
| 2 | `TooltipManager.SpecialKeywordData` | `SpecialKeywords` enum name, case-insensitive | `GetHeader()` / `GetContent()` | 1 |
| 3 | `ConceptGlossarySO` | `ConceptEntry.id` | `DisplayName` / `Description` | N |

**The order is the invariant, not an implementation detail.** The glossary is new and is consulted
last, so an entry there can never shadow a home that already existed. The glossary owns **only**
ids no other registry owns. A concept therefore has one authority even if someone duplicates an id
by mistake, and the mistake is visible rather than silent: `GameTextWindow ▸ Concepts` flags a
glossary id that a status key or keyword also claims as `DUPLICATE: … owns it`. The fix is always
deleting the glossary entry, never the other one.

Consumers hand the resolver its language-bound sources. `TutorialController` holds `conceptGlossary`
(same language as its `catalog`) and `conceptStatusCatalogues` (musician + audience) and passes both
to `TutorialOverlayView.SetConceptSources` in `OnEnable`; keywords are read from
`TooltipManager.Instance` at resolve time, because that singleton may not exist when the resolver is
constructed.

**Known asymmetry (T-TAG-1).** Rows 1 and 2 have a single language slot, so in the Spanish build the
seven ids they own (`flow`, `composure`, `earworm`, `shaken`, `vibe`, `stress`, `convinced`) display
English text while the seventeen glossary ids display Spanish. This is accepted for playtest
(**D-TAG-10=A**) and resolved by **TXT-3** (§7).

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

## 5. Vocabulary v1 (D-TAG-3=B)

| Registry | ids |
|---|---|
| keyword | `vibe` · `stress` · `convinced` |
| status | `flow` · `composure` · `earworm` · `shaken` |
| glossary — meters | `hype` · `inspiration` · `cohesion` |
| glossary — game objects | `gig` · `song` · `loop` · `period` · `play` · `track` · `song_end` · `stage` · `budget` · `composition_card` · `action_card` · `sound_card` · `breakdown` · `blocked` |

`blocked` lives in the glossary because the Blocked tint has no `StatusEffectSO` and no icon
(Decision E3, `SSoT_Status_Effects.md`). If one is ever created with `StatusKey == "blocked"`, the
window flags the glossary entry and the entry is deleted.

**Surfaces in v1 (D-TAG-4).** Text: `TutorialDialogSO.mechanicText` only (24 ids, EN + ES).
Capability: any surface may call `ConceptTagRenderer.Decorate`; voiced pages are untagged by choice,
card descriptions and status descriptions are untagged and out of scope until a later batch.

## 6. Failure modes

| Situation | Editor | Runtime |
|---|---|---|
| id in no registry | `TAG unknown: id` | plain text, no hover, one warning per id per session |
| id in glossary **and** status/keyword | `DUPLICATE: … owns it` | status/keyword text shown (§3 order) |
| tag present in one language, absent in the other | `MECHANIC TAGS differ` / `TAGS differ` | that language renders the word plain |
| `<link=` without `</link>` | `TAG unclosed <lang>` | TMP extends the link to the end of the text |
| glossary not assigned on the consumer | — | glossary ids render plain; one log line at `SetConceptSources` |
| `TooltipManager` absent | — | tags render styled, hover shows nothing |

Parity of tags is checked exactly like parity of `{$token}`s: the **set** of ids must match across
languages. A translation that silently drops a tag is a hole in the UI that reading the copy would
never reveal.

## 7. Open decisions

- **O-TXT-3 (inherited from TXT-1) — Unity Localization or project-owned catalogs.** Still deferred.
  TXT-2 supplied the case TXT-1 said it was waiting for and the answer taken was project-owned
  (D-TAG-2b=(i)), which reuses the TXT-1 pattern but makes a future migration cost two categories
  instead of one. The decision now also has to count population 5 (§1), which no per-SO scheme
  reaches.
- **D-TAG-10 → TXT-3.** Moving populations 2 and 3 into the glossary so status and keyword text gain
  language slots. This is an authority **move**, not a copy: `StatusEffectSO.description` stops being
  player text, `displayName` is redefined as a developer label used in logs and the editor, and the
  resolution order of §3 collapses toward a single registry. The rejected alternative was per-language
  fields on `StatusEffectSO`, which puts translation inside a gameplay asset and requires a script
  change per language.

## 8. Update rule

Update this document when a change affects: which population stores a given kind of player text; the
tag syntax; the resolution order or its registries; the hover contract or its relationship to TIP-1;
the parity checks; or the CSV interchange contract for text.

**Related authority.** `SSoT_Editor_Authoring_Tools.md` §20 — `GameTextWindow` behaviour and the CSV
codec. `Design_Tutorial_System_v0_3.md` §5A — tutorial copy voice and `mechanicText` register.
`SSoT_Status_Effects.md` §3.3 — status meaning and tooltip hosts. `SSoT_Card_System.md` §3.3, §10.2 —
keyword meaning and card-hover assembly.

# SSoT_Game_Text — ALWTTT

**Class:** subsystem SSoT · **Created:** 2026-09-10 (TXT-2, D-TAG-9=A) · **Status:** active
**Last substantive change:** 2026-09-16 (TEXT-VIBE-1) — the authority question the map had been
carrying since TEXT-6 is **answered**: **D-SP1-6=B — an asset does not hold player text; it holds the
datum the id is derived from.** Rows 8 and 11 are resolved, row 10 is resolved for meter and tempo,
and the remaining third of row 10 is the only population still open. Templates 73 → **102**, of which
13 are **derived from two package-owned enums** rather than declared one by one — the first coverage
set in this document that is enumerated from a type instead of typed by a person. See §9 for the
change record.

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

**Eleven populations carry player text today; ten and two thirds are per-language.** The census has
grown three times after being declared complete — the sixth was absent from the TXT-1 census, found
at TXT-4 close and resolved at TEXT-5; the seventh and eighth were found at TEXT-6; the ninth and
tenth at SONG-PANEL-1b, in a producer no earlier census had opened; the eleventh at TIP-1-DOC, in a
presentation file. §7 and the notes below keep the census lessons, which outlive the fixes.
**One population is OPEN** after TEXT-VIBE-1 — the `RoleEntry.displayName` third of row 10, whose
consumer was not attested in the session that resolved the other two thirds — plus three loose
literals recorded in §7 without a row of their own (F-TV-1, F-TV-4) and one candidate population
whose producer has not been read yet (F-TV-7). This document still does not call the census closed:
*a population found is not a census completed*, and TEXT-VIBE-1 added four findings while closing
five.

**Rows 8 and 10 were one problem, and TEXT-VIBE-1 answered it once: D-SP1-6=B.** Both were
player-visible text typed into a ScriptableObject field with a single language slot, no editor tab,
no CSV and no coverage check. Migrating one and leaving the other would have set two precedents for a
single question — *may an asset hold player text at all, or must it hold an id the glossary
resolves?* — which is a decision about **authority over authored content**, not a literal swap.

**The answer, and it is the precedent for every future asset with a text slot: neither.** The asset
holds **no id either**; it holds the datum the id is *derived* from, and the field is **deleted**.
- Row 8: `FxEntry.label` is gone. Which template a floater prints follows from **which entry the
  classifier selected** (`SongCompositionUI.FxTextIdOf`), so the category *is* the key and there is
  nothing left to author.
- Row 10: `MeterEntry.displayName` and `TempoEntry.displayName` are gone. The chip label is
  `ui_ctx_meter_<value>` / `ui_ctx_tempo_<value>`, derived from the enum value the lookup already
  receives.

**Why deletion rather than an authored id field.** An id typed into an inspector is one edit away
from being wrong, and a wrong id is invisible to coverage (see §8, the asymmetry rule) — the same
argument SONG-PANEL-1 used to **delete** three `[SerializeField] string` defaults instead of migrating
them. The observed labels were themselves the evidence: `Four Four (FourFour)` and `Fast (Fast)`
duplicated the enum name *inside* the authored string, which is precisely what a derived id makes
unwritable. **A residual third of row 10 stays open**: `RoleEntry.displayName` is untouched because
its consumer (`SongTrackElementUI` / `TrackHoverPanel`) was not read in that session — and under the
producer-census rule (F-SP1-1) an unread consumer is not a migration, it is a guess.

| # | Population | Storage | Language slots | Authoring surface | Authority for meaning |
|---|---|---|---|---|---|
| 1 | Tutorial copy (`revisitTitle`, `pages[]`, `mechanicText`) | `TutorialDialogSO` inside `TutorialDialogCatalogSO`, one catalog per language | **N** (D-S5f-2=B, D2=B) | `GameTextWindow ▸ Tutorial` + CSV | `Design_Tutorial_System_v0_3.md` |
| 2 | **Concept text — meters, game objects, statuses and card keywords** | `ConceptGlossarySO`, one asset per language | **N** | `GameTextWindow ▸ Concepts` + CSV | the subsystem SSoT of each concept; this asset stores wording only |
| 3 | **Card names** | `ConceptGlossarySO.cardNames`, same assets, own list (D-TXT4-4) | **N** | `GameTextWindow ▸ Cards` + CSV | `SSoT_Card_System.md` |
| 4 | **Card descriptions — stored as fragments, never as sentences** | `ConceptGlossarySO.templates`, composed at call time by `CardEffectDescriptionBuilder` (`SSoT_Card_System.md` §10.1) | **N** | `GameTextWindow ▸ Templates` + CSV | `SSoT_Card_System.md` §10.1 |
| 5 | **The `AudienceCharacterCanvas` literals — RESOLVED (F-TXT-2-1)** | `ConceptGlossarySO.templates` under the `ui_aud_*` ids, plus the `blocked` concept entry | **N** | `GameTextWindow ▸ Templates` + CSV | `SSoT_Audience_and_Reactions.md` §6.4 |
| 6 | **Audience intention and ability text — RESOLVED (F-TXT4-7, TEXT-5)** | `ConceptGlossarySO.entries`, keyed by `AudienceAbilityData.abilityId` (D-T5-1a=A): entry **name** = tooltip header, entry **description** = tooltip body | **N** | `GameTextWindow ▸ Concepts` + CSV | `SSoT_Audience_and_Reactions.md` §7, §8 |
| 7 | **Song-panel track hover literals — RESOLVED (F-T5-2, TEXT-6)** | `ConceptGlossarySO.templates` under the `ui_track_*` ids, plus the reused `ui_card_detail_modifiers` (D-T6-3=B) | **N** | `GameTextWindow ▸ Templates` + CSV | `SSoT_Card_System.md` §10.1 (track rows), this document §5 |
| 8 | **Composition floater labels — RESOLVED (F-T6-9, TEXT-VIBE-1)** | `ConceptGlossarySO.templates` under ten `ui_fx_*` ids; `FxEntry.label` **deleted** and the id chosen in code by `SongCompositionUI.FxTextIdOf` from the selected entry (D-SP1-6=B) | **N** | `GameTextWindow ▸ Templates` + CSV | this document §5; which category fires is `SongCompositionUI` diff classification (B2/#3) |
| 9 | **Song-panel context-row hover literals — RESOLVED (F-SP1-8, SONG-PANEL-1b)** | `ConceptGlossarySO.templates` under the `ui_ctx_*` ids | **N** | `GameTextWindow ▸ Templates` + CSV | this document §5; loop/part meaning in `SSoT_Runtime_CompositionSession_Integration.md` |
| 10 | **Strip chip labels — RESOLVED for meter and tempo, OPEN for role (F-SP1-13, TEXT-VIBE-1)** | meter/tempo: `ConceptGlossarySO.templates` under **derived** ids `ui_ctx_meter_<value>` / `ui_ctx_tempo_<value>`, the two `displayName` fields **deleted** and the lookups stripped of their `out string label`. **Still open:** `CompositionStripThemeSO.RoleEntry.displayName`, monolingual, consumer not yet censused | **N** (meter, tempo) · **1** (role) | `GameTextWindow ▸ Templates` + CSV; role still Inspector-only | this document §5. `MoodTag` and `Tonality` are **enum names, not asset text** — census correction, they belong to F-TXT4-2 → CARD-FACE-1 |

| 11 | **C1 Vibe readout tooltip — RESOLVED (F-TIP-2, TEXT-VIBE-1)** | `ConceptGlossarySO.templates` under six `ui_readout_*` ids, including the header; `GigCanvas.BuildReadoutTooltip` composes them and keeps only the line break as layout | **N** | `GameTextWindow ▸ Templates` + CSV | `planning/Design_Vibe_Telegraph_v0_1.md` §10.1 (what the readout means); wording lives here |

**Row 11 is a census failure, not a new surface (TIP-1-DOC, 2026-09-15).** The C1 readout hover shipped at BIGNUM-1 (2026-09-09), two batches *before* SONG-PANEL-1 declared the closing criterion «met for code-authored text». It was missed because every census since TXT-1 has walked the text-producing subsystems — glossary consumers, card surfaces, the audience canvas, the song panel — and `GigCanvas` is a **presentation** file whose player text is a by-product of a telemetry feature. So the SONG-PANEL-1 claim is **narrowed here rather than restated**: code-authored text is resolved *in the surfaces then censused*. The method rule this adds to §8: a batch that adds a **hover, floater or readout** adds a population row when it ships, because the census will not find it later by walking text files. Assigned to **VIBE-READ-1**, which rewrites that function anyway for F-TIP-6 — migrating it in a batch that already edits it costs one pass instead of two. **Closed at TEXT-VIBE-1 (2026-09-16)**, and the one-pass argument held: the same edit that suppressed the inert `+ SFX del local → 0` line replaced the five Spanish literals with `ui_readout_*`. The migration did **not** wait on D-SP1-6, and the reason is worth keeping: row 11 is text **in code**, the shape populations 5, 7 and 9 have resolved by template since TXT-4; D-SP1-6 governs **assets**. Scoping a code-authored population to an asset-authority decision would have delayed it for a dependency it does not have.

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

**Six player surfaces consume population 2** (§3): the status icon hover (`StatusIconBase`), the
card hover (`CardBase.ShowTooltipInfo`), the `<link=id>` tag in tutorial text, the **status floater**
(`SensoryFtPresentation.TryBuildStatusAppliedFt`, TXT-3), the **keyword line of the detail modal**
(`CardDetailViewController`, via `GameText.ConceptName` — D-T5-4=A, TEXT-5) and the **intention
hover** of the audience canvas (TEXT-5, population 6 shares this list's registry). The first four go
through `ConceptTooltipResolver`; the last two ask `GameText` for the same entries (§3.1).
The floater is the one that is easy to forget, because it reads a *name* and not a tooltip; it was
found only at batch close (**F-TXT-3-1**) and would otherwise have drawn `+COMPOSURE` over a Spanish
tooltip reading «Compostura».

**Populations 7 and 8 are what TEXT-6 added to the map, and they were found by two different
methods.** Population 7 was known as a defect (F-T5-2) before it was a population: three literals seen
in a screenshot. Reading the function that builds the panel instead of the screenshot showed **seven**
literals plus one compiled out of player builds (F-T6-2, F-T6-3). The lesson is narrower than the
TXT-1 one and worth keeping separately: **census the function that composes the surface, not the frame
that was captured** — a screenshot only proves what was on screen in that state, and a hover panel has
states (placeholder, pending, levelled, no instrument) that no single frame shows.

Population 8 was found the other way: not on screen at all, but in code, while tracing where
population 7's neighbours got their text. It is the same shape as population 6 — **player text
authored as data, reached by no literal the census could grep and no field it enumerated** — and it is
listed here unresolved on purpose. A population whose row says OPEN is cheaper than a census that
claims to be complete for the third time. Its status is **code truth from a PK copy, not observed
play**: `SongCompositionUI` reads `entry.label` and spawns a floater with it; no sweep has yet
confirmed which labels a player actually sees.

**One more thing population 7 exposed, and it is not a literal (F-T6-8).** The track row's own label is
written at runtime by `SongCompositionUI`: after an `InstrumentEffect` resolves, `track.info` is
**overwritten** with `"{styleBundle.name} - {instrumentName}"`, so the row and its hover show an asset
name where the glossary card name should be, while the minicard two centimetres away shows the
glossary name (observed 2026-09-14, TEXT-6 close). This is a **`GAMETEXT-NO-DEV-LABEL` violation by
data, not by literal**: no text registry is bypassed, a field that feeds a player surface is simply
assigned developer-facing content downstream of it. Neither the TXT-1 literal census nor the TEXT-6
surface census could see it, because it is neither a string in a UI file nor a text field on an SO:
it is an assignment. → F-T6-8, TEXT-6b.

**Populations 3, 4 and 5 are the TXT-4 result, and 3 is also a MOVE.** The criterion is identical to
the one applied to `StatusEffectSO.displayName` at TXT-3, and so is the consequence:

- `CardDefinition.displayName` is now a **developer label** — inspector lists, logs, editor tooling,
  asset identification. **No player surface may read it.** The card name a player sees comes from
  `ConceptGlossarySO.cardNames`, resolved by `GameText.CardName` (§3). A player-facing read of
  `displayName` is a governance regression, not a shortcut; **F-TXT4-1** was exactly that regression
  living undetected inside `CardEffectDescriptionBuilder` since TXT-3, and it accumulated no debt
  only because all 12 status labels happened to match the glossary.
- **The three residual readers were closed at TEXT-5** (**F-TXT4-3**). The played-card floater
  (`SensoryFtPresentation`), the detail-modal header (`CardDetailViewController`) and the song-panel
  track label (`SongCompositionUI`) all resolve through `GameText.CardName` now, so the rule in the
  bullet above finally describes the code and not just the intent. **Zero live player-surface
  readers of a developer label** is the condition D-DOC-1=C required, and it is why
  `GAMETEXT-NO-DEV-LABEL` was promoted to a hard invariant in `ssot_manifest.yaml` at TEXT-5-DOC
  rather than at TXT-4: an invariant that fails on every run trains its reader to skip the finding.
  The evidence that the batch moved **readers** and not the registry is the card-name fingerprint,
  byte-unchanged across TEXT-5 (§9) — if it had moved, some edit would have written into the
  glossary instead of reading from it.

**Population 4 has no stored sentence, and that is the design, not a gap.** What the glossary stores
are **fragments** — 47 template ids with positional placeholders (D-TXT4-3=b′), declared in
`GameTextTemplateIds.All`. `CardEffectDescriptionBuilder` decides which fragment a given effect spec
needs and fills the placeholders from the spec's own numbers. The reason to store fragments instead
of finished descriptions is that a **balance change must not require re-authoring text**: retuning a
spec from 2 to 3 changes the rendered line without touching any language column. The cost is that a
translator never sees a whole sentence, which is why placeholder parity is enforced as hard as tag
parity (§6).

**Population 6 was a finding before it was a design (F-TXT4-7, resolved at TEXT-5).** Audience
ability names and intention bodies were player text living in monolingual fields on content SOs; a
Spanish build showed «Tantrum / This Character intends to inflict Stress.» over a character whose
every other surface was Spanish. **Why the census missed it for three batches:** TXT-1 censused text
by walking *code files* for string literals and SO text fields under `Assets/Scripts`. This
population is authored as data in content assets, reached by no literal and by no field the census
enumerated, and it surfaced only when a full Spanish dump made the remaining English visible. The
rule this leaves outlives the fix and is the mirror of the one F-TXT-3-1 left: **census by what a
player surface reads, and census assets as well as code.**

**How it was solved (D-T5-1a=A, D-T5-1=A).** `AudienceAbilityData` — a nested class in
`AudienceCharacterData.cs`, not a file of its own — gained a stable `abilityId`, and the pair
(ability name, intention body) became **one concept entry**: name in the tooltip header, description
in the tooltip body. One entry rather than two is the right shape because the two halves are never
shown apart; splitting them would have created an id whose only consumer is half a panel.
`abilityName` and `AudienceIntentionData.contentText` joined `StatusEffectSO.displayName` and
`CardDefinition.displayName` as **developer labels**.

**Population 9 is the fourth producer of the same panel, and no census had opened it (F-SP1-1).**
Populations 7 and 9 render two rows of one screen. The track-row census (TEXT-6) read
`TrackHoverPanel`; the context row is built by `CompositionContextRowUI`, a sibling file, and no
batch had read it until a Spanish sweep photographed «2 musicians silent» on a passing screenshot.
This is **F-T6-2 for the fourth time**, with a variation worth naming: the earlier three undercounted
*within* a producer, this one missed a *producer*. The habit that prevents it is not "count from the
function" but **"enumerate the producers of the surface first, then count inside each"** — a panel is
not a file.

**Population 9 also carried a defect that only a template could expose (F-SP1-10).** The silent-
musician line read `"{n} musicians silent"` with no singular branch, so one musician printed
«1 musicians silent». While the string was a literal, the missing branch was invisible; splitting it
into `ui_ctx_musician_silent_one` / `_many` — the shape `ui_card_modifier_one` / `_many` already
used — makes the ungrammatical form **unwritable**. Migration is not only translation: it forces the
branches a literal was hiding to be declared.

**The key is the id, not the name**, for the reason every earlier population already demonstrated: a
name is content and moves when the writing moves, an id is identity. Keying by id also makes a
*shared* id legal — two audience characters authoring the same ability (D-T5-5=A) get one entry
instead of two copies that drift — which is why the seeding tool reports a repeated id as `SHARED`
and not as `DUPLICATE`. A warning that fires on correct content is a warning people learn to ignore.

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

### 2.1 Rich text — declared subset (TXT-4, D-TXT4-6)

Authored cells may contain TMP rich text, but only from a **declared** set:
`{sprite, color, b, i, u, size, link}`. Anything else is flagged in the editor (`TAG unsupported`)
and left alone at runtime. There is **no runtime strip pass**: an unsupported tag reaches TMP as
written, so the failure mode is a visibly wrong glyph in the editor's preview, not a silent
divergence between what the author typed and what shipped.

**Sprites are referenced by name, never by index** (D-TXT4-5). `<sprite name="tt_drums">` is legal;
`<sprite=3>` is an authoring error and is badged `SPRITE index`. The reason is mechanical: a TMP
sprite atlas renumbers its entries on reimport, so an index is a reference that silently retargets
the day someone adds a glyph, while a name survives. The editor additionally badges `SPRITE unknown`
for a name absent from the atlas.

**One Sprite Asset, global, registered in TMP Settings** — not one per text family. The rejected
alternative was per-family assets assigned as fallbacks on each consuming component, which makes
every new text surface a wiring step and makes a missing glyph a per-prefab defect. The cost of the
global asset is a single flat namespace, which is why ids carry prefixes by domain: `tt_` track
type, `mod_` modifier, `mt_` meter. Sprite ids live in a namespace **separate** from concept ids —
a `<link=flow>` and a `<sprite name="mt_flow">` are two different things and are checked separately.

**Target phrases do not start with a definite article** (authoring rule from **F-TXT4-5**). The
`tgt_*` fragments are inserted into a sentence whose preposition is chosen by the template that
precedes them, and Spanish contracts `a + el → al`. Because the fragment cannot know what precedes
it, «el objetivo» produced «a el objetivo» in four cards. Templates resolve **argument order**, not
morphology, and every fragment library eventually meets this wall; the containment is to keep the
article on the template side.

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

### 3.1 Two entry points, one registry (TXT-4)

Since TXT-4 the glossary answers two kinds of question, through two static entry points:

| Entry point | Answers | Keyed by |
|---|---|---|
| `ConceptTooltipResolver` | **hovers** — "what does this concept mean" | concept id / `StatusKey` / keyword name |
| `GameText` | **everything else** — card names, template fragments, a concept's *name* when it appears inside a composed sentence, and **numbers inside composed lines** (`GameText.Number`, TEXT-5) | card id, template id, concept id |

Both read the **same asset from the same seat** — `TooltipManager.ConceptGlossary` — and both read it
**at call time**, not at construction. Two entry points are not two registries: there is still
exactly one place a string can come from, and splitting the API is what stops a composed sentence
from having to fake a hover to get a status name. This is also why «Cautivado» inside a Vibe
breakdown line is not a second home for the Captivated name: the line asks `GameText` for the same
`StatusKey` entry the icon hover asks `ConceptTooltipResolver` for.

**Numbers are text too (D-T5-2=B, TEXT-5).** `GameText.Number(value, format)` formats through the
running glossary's `NumberFormat`, derived from a `numberCulture` field on the asset; with no
glossary assigned it falls back to `InvariantCulture`, **explicitly**. The decimal separator is a
property of the language, so it travels with the language asset. The rejected alternative was a
`CultureInfo` chosen at the presentation layer, which is what F-TXT4-10 turned out to be already
(§7): an unqualified `ToString("0.00")` reads the *machine's* culture, so the rendered line depends
on the player's OS rather than on the language they selected — the same build printing «1.00» on one
machine and «1,00» on another. An unknown culture code logs one warning and degrades to invariant;
it never throws, because a mistyped culture in an asset must not take a hover down.

**One declared exception:** `GameText.SetEditorFallback`, wrapped in `#if UNITY_EDITOR`, lets editor
tooling point at a chosen glossary when no `TooltipManager` exists in an editor-only context. It is
the **only** point in the codebase where a glossary is selected by something other than the
manager's serialized field, it cannot compile into a build, and it is **not a precedent**: any
runtime code that wants a different glossary is asking for a second seat, which R-1 already shows is
the failure this system is built to avoid.

## 4. Hover behaviour (D-TAG-5=D)

`TutorialOverlayView.Update` tests the pointer against link geometry with
`TMP_TextUtilities.FindIntersectingLink`. No raycast is involved, so the modal's `blocksRaycasts`
and the tutorial input gates are irrelevant, and characters not yet revealed by the typewriter
(`maxVisibleCharacters`) do not intersect and cannot be hovered.

On a link **change** — enter, leave, or a jump straight from one link to the next — the surface
performs exactly one `TooltipManager.HideTooltip()` followed by at most one
`ShowTooltip(body, header)` with **no target transform**.

Both halves of that sentence were written as a way **around** two open defects; since **TIP-1
(2026-09-15) both are closed**, and the sentence survives as a description of what this surface
wants rather than of what it was avoiding:

- `TooltipManager.ShowTooltip` **pushes** a panel per call — now a declared contract
  (`TOOLTIP-SHOW-IS-A-PUSH`, `systems/SSoT_Tooltip_Pipeline.md` §2), not the leak recorded as
  **F-BN-7**. A concept hover wants exactly one panel, so it hides before it shows. Unchanged
  behaviour, different reason.
- Passing no transform takes the cursor-following branch, which is what a hover over running text
  wants: there is no stable rectangle to anchor to. **F-BN-8** (static anchors under a Screen
  Space – Overlay canvas projected through `Camera.main`) is closed at TIP-1 by inferring the
  anchor space from the target's root canvas, so this surface could now pass a transform — and
  deliberately does not.

The fade-in still replays on every new word. That is now a **choice**, not a cost avoided:
`TooltipManager.UpdateTooltip` (TIP-1) can rewrite a shown panel with no fade, and a concept hover
does not use it, because a new word is a new tooltip and the fade is the cue that it changed.
**Authority for the pipeline itself is `systems/SSoT_Tooltip_Pipeline.md`** (entries §2, placement
§3, lifecycle §4); this document owns the text that travels through it.

`Hide()` and `ShowPage()` both clear the hover, so no tooltip survives a closed modal or a page turn.

**Canvas order (D-TAG-8=A, applied 2026-09-10).** The tooltip canvas must sort **above** the tutorial
overlay canvas, and the card-detail modal canvas must remain above the tooltip canvas
(`SSoT_Card_System.md` §10.3). Verified at ST-TAG-1: with the original order the tooltip rendered
behind the dialog box and read as "no tooltip".

## 5. Vocabulary (D-TAG-3=B, extended by TXT-3, TXT-4 and TEXT-5)

**One registry, three lists: 43 concepts + 60 card names + 102 templates.** The three lists are
separate id spaces inside the same per-language asset, each with its own tab, coverage check and
fingerprint.

**Since TEXT-VIBE-1 the template id set is no longer a literal list, and that is a contract, not an
implementation detail (D5).** `GameTextTemplateIds.All` is built at type initialisation as
`Fixed` (89 hand-declared ids) **plus one id per value of `TimeSignature` and one per value of
`TempoRange`**, both MidiGenPlay-owned enums, giving 102 today. Two consequences follow and both are
the point: a package version that **adds** a meter turns the Templates tab red instead of shipping an
enum name to the player; and **no person types that part of the list**, which is what stops the
failure recorded as F-TV-5. The ids are derived from the enum **name**, lower-cased — string
coupling, deliberately, for the reason `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.10 already gives for
chord qualities: a package-side **rename** must degrade to a visible miss, never to a compile error
in ALWTTT. A derived id that no card can reach is **live but inert**, the `ui_track_level`
precedent, and is not deleted. The groups below describe where a **concept** id comes from, not where its text lives —
since TXT-3 all of it lives in `ConceptGlossarySO`.

| Group | Id origin | ids |
|---|---|---|
| Status (12) | `StatusEffectSO.StatusKey` | `captivated` · `choke` · `composure` · `earworm` · `exposed` · `feedback` · `flow` · `hyped` · `indifference` · `shaken` · `spotlight` · `voltage` |
| Card keyword (7) | `SpecialKeywords` enum name, lowercased | `consume` · `convinced` · `ethereal` · `exhaust` · `stress` · `tall` · `vibe` |
| Ability (7) | `AudienceAbilityData.abilityId` (TEXT-5, D-T5-1a=A) | **enumerated from content assets, not listed here** — `ab_` prefix by convention; seven at TEXT-5 close, one of them shared by two characters (D-T5-5=A) |
| Glossary-native (17) | nowhere else — meters and game objects | `hype` · `inspiration` · `cohesion` · `gig` · `song` · `loop` · `period` · `play` · `track` · `song_end` · `stage` · `budget` · `composition_card` · `action_card` · `sound_card` · `breakdown` · `blocked` |

An id in the first **three** groups is **required** to exist (§3, coverage). An id in the fourth
exists because someone authored it. Only 24 of the 43 are currently reachable by a `<link=id>` tag;
the rest are reached by an icon hover, a card hover, a floater or an intention hover. **An id with
no tag is not an orphan** — the surface that consumes it just is not text.

**The ability group is the only one whose id set is not declared in code**, and that is deliberate
(TEXT-5). Status keys arrive from `StatusEffectSO` assets through a catalogue, keyword ids from an
enum, template ids from `GameTextTemplateIds.All`; ability ids exist only as authored strings inside
`AudienceCharacterData` assets. `Assets/Scripts/Editor/Text/AudienceAbilityTextIndex.cs` therefore
**walks the assets** to build the coverage set, which is why this document gives the count and not
the list: writing seven ids here would create a fourth place to keep in sync, and the enumerator
exists precisely so that adding an audience character adds its ability to the coverage check with no
edit anywhere — including here. The failure mode this design has to name explicitly is the one
`GameTextTemplateIds.All` has in reverse: an ability whose `abilityId` is **empty** cannot be keyed
at all, so the enumerator emits the sentinel `EMPTY:<asset>[i]` and coverage fails loudly on it (§6)
rather than skipping a row that has no id to report.

`blocked` stays glossary-native because the Blocked tint has no `StatusEffectSO` and no icon
(Decision E3, `SSoT_Status_Effects.md`). If one is ever created with `StatusKey == "blocked"`, the
entry stops being glossary-native and becomes the status entry for that key — same id, same text,
no edit required. That is a consequence of the single registry that the TXT-2 rule (delete the
glossary entry) got exactly backwards.

**Card names (60 ids, TXT-4).** The id space is the card's stable identity, not its English name.
One entry per `CardDefinition` in the catalogue, in every language; a card with no entry shows its
raw id (§6), never its developer label.

**Templates (73 ids — 47 at TXT-4, `ui_card_type_composition` at TEXT-5, six `ui_track_*` at
TEXT-6, eighteen at SONG-PANEL-1 and one at SONG-PANEL-1c).** The id set is
declared in **`GameTextTemplateIds.All`**, which is
simultaneously the production list and the editor's coverage set. This double duty is the thing to
remember: **a template that is not in `All` is invisible to the coverage check.** It can be authored,
imported, fingerprinted and shipped while the editor reports full coverage, because coverage is
computed against `All` and not against whatever the asset happens to contain. Adding a template is
therefore two edits — the id in `All`, the text in every language — and doing only the second is the
failure mode this note exists to name.

The TEXT-5 addition earns its place by demonstrating what the list is for. The `"COMPOSITION"`
literal existed in **two** files — `CardBase.typeTextField` and
`CardDetailViewController.cardTypeText` — and TXT-4 had counted only one. Both now call
`ui_card_type_composition` (D-T5-3=A): two hardcoded homes of one string collapse into one id, which
is the entire argument for a template list over a literal.

**The TEXT-6 addition shows the other half of the rule: when NOT to add an id (D-T6-3=B).** The track
hover needed seven words migrated. Six became new `ui_track_*` ids — `ui_track_no_track`,
`ui_track_pending`, `ui_track_level`, `ui_track_level_max`, `ui_track_inspiration_per_loop`,
`ui_track_instrument`. The seventh, the panel's `Modifiers` header, reused
**`ui_card_detail_modifiers`**, the id the detail modal already owned for the same word naming the same
concept. A new `ui_track_modifiers` would have been the cheaper edit and the wrong one: two ids for one
concept drift the moment a translator renders one of them differently, and the glossary has no check
that would catch it — coverage proves an id has text in every language, never that two ids agree.
**One concept, one id, even across surfaces.** The boundary that decides is the concept, not the panel:
`Instrument:` got its own id despite living beside the modal's `Style:` because no existing id named an
instrument.

**What stayed in code at TEXT-6, and why that is not a gap.** The separator `" · "`, the `": "` after a
label and the `"-"` placeholder are **punctuation**, not words: they do not change between Spanish and
English, and a template whose entire content is a colon is a translation liability, not an asset. The
`TrackRole` enum name printed on line 1 stays under F-TXT4-2 → CARD-FACE-1. The `Bundle:` line stays a
literal because it is compiled out of player builds by `#if ALWTTT_DEV` (**D-T6-4=A**): it is a
diagnostic and never reaches a player surface, which is also the precise reason the screenshot that
opened F-T5-2 could show it at all — that capture was a DEV build (F-T6-3). If that line ever becomes
runtime-toggled instead of compile-time-excluded, it becomes a player surface again and needs an id;
**D-T6-4 is to be re-opened by any batch that makes the toggle a runtime one**, not silently kept.
*(SONG-PANEL-1 tested exactly this condition and it did not trigger — see the confirmation note
below.)*

**SONG-PANEL-1 added eighteen ids and confirmed D-T6-4 rather than reversing it.** Eleven cover the
song panel's own producer (`ui_song_title_default`, `ui_song_theme`, `ui_song_theme_default`,
`ui_part_label`, `ui_part_intro`, `ui_part_outro`, `ui_part_solo`, `ui_track_lead_in`,
`ui_track_finale`, `ui_track_improvisation`, `ui_fx_chord_fallback`); seven cover the context row
(`ui_ctx_*`), with `ui_ctx_loop_no_total` added at 1c. Three rules came out of it:

- **Three serialized English defaults were deleted, not migrated.** `defaultSongTitle`,
  `defaultTheme` and `defaultPartLabel` were `[SerializeField] string` fields holding player text in
  the inspector — the shape of populations 8 and 10, inside a C# file. Replacing the *field* with a
  glossary read removes the surface; migrating the *literal* would have kept an English default one
  inspector edit away from returning.
- **A glossary read has an initialisation order a literal does not.** The two title/theme lines moved
  from `Awake` to `Start`: `GameText.Format` resolves through `TooltipManager.Instance`, and `Awake`
  order across scene objects is undefined, so half the loads would have shown the miss text. This is
  the recurring cost of moving text from code to data, and it is paid per call site, not once.
- **Part labels are letters everywhere.** The old code produced `Part A` for index 0 and `Part 2` for
  index 1 — two schemes in two functions. A parameterised template forces one; `ui_part_label` takes
  the **letter**. `SongConfigBuilder` consumes `PartEntry.label` as the package-side `Part.Name`, so
  that name is now localised; it is internal (logs only), recorded as a risk if anything ever indexes
  by it.

**D-T6-4=A is CONFIRMED, not reversed (D-SP1-2=B).** The re-open condition §5 wrote at TEXT-6 was
*"if that line ever becomes runtime-toggled"*. SONG-PANEL-1 added the toggle **inside** the
`#if ALWTTT_DEV` block: in a player build the serialized field does not exist, so `Bundle:` remains
unreachable and stays out of the glossary. The same reasoning decided the percussion floaters
(D-SP1-4=B): `"Kick"`, `"Snare"`, `"HH cl"`, `"Cymb"`, `"Tom"`, `"Perc"` are RFX-1 diagnostics whose
two gates ship OFF, so the gate became compile-time and no id was minted. Only `Chord (…)`, reachable
by default, got one. **Giving an id to developer text would invert `GAMETEXT-NO-DEV-LABEL`**: an id is
a promise that the string is player text, enters coverage, and must be translated.

**A BOM is not a guarantee; ASCII source is (F-SP1-2).** F-T6-1 was closed at TEXT-6 by transcoding
`TrackHoverPanel.cs` to UTF-8 with BOM. SONG-PANEL-1 found the file **back in Windows-1252**, and the
byte sweep after the fix showed why: the replacement landed with `nonASCII=0` but **`BOM=False`** —
Unity's import stripped it. The BOM did not survive an editor round-trip; the absence of non-ASCII
bytes did. **Standing rule, strengthened:** a source file that reaches the text pipeline writes every
non-ASCII glyph as an escape (`"\u00B7"`), and the closing check is **`nonASCII=0`**, not `BOM=True`.
A BOM is still written, as reinforcement.

**Two of the six ids are authored, covered and unreachable today (ST-T6-3 / ST-T6-3R, untestable).**
`ui_track_level` and `ui_track_level_max` are only formatted when `d.level >= 2`, and track levels are
not implemented yet (`Design_Track_Card_Levels_v0_1.md` is planning). They are recorded here as
**live but inert**, exactly as ST-T5-5 was: the path is unverified, not absent. The alternative —
withholding the ids until the mechanic lands — would have left two English literals in the file for
the sake of a coverage number.

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
| card id or template id with no entry (TXT-4) | `MISSING <lang>` on its own tab; coverage block for a template id in `GameTextTemplateIds.All` | the **raw id** is rendered, one warning per id per session |
| sprite name absent from the atlas (TXT-4) | `SPRITE unknown` | TMP draws nothing for that tag |
| sprite set differs across languages (TXT-4) | `SPRITES differ` | one language loses an icon |
| `<sprite=N>` by index (TXT-4) | `SPRITE index` | renders until the atlas is reimported, then retargets |
| placeholder set differs across languages (TXT-4) | `PLACEHOLDERS differ` | a number the player needs is missing, or an argument lands in the wrong slot |
| rich-text tag outside the declared subset (TXT-4) | `TAG unsupported` | passed to TMP as written |
| ability id with no glossary entry (TEXT-5) | `COVERAGE — ability 'ab_x' — missing: es` (block) | the intention hover shows the raw id, one warning per id per session; **never** `abilityName` |
| `abilityId` empty on an authored ability (TEXT-5) | `COVERAGE — ability 'EMPTY:<asset>[i]' — missing: en, es` (block) | the hover keys the glossary on `""`, misses, and renders an empty header — the sentinel exists so the editor reports it before the build does |

Parity of tags is checked exactly like parity of `{$token}`s: the **set** of ids must match across
languages. A translation that silently drops a tag is a hole in the UI that reading the copy would
never reveal. **TXT-4 extends the same rule to sprites and to placeholders, in all three tables**
(D-TXT4-7), for the same reason: both are invisible to someone reading the copy, and a dropped
placeholder is worse than a dropped word because it removes a number rather than a nuance.

**`GameText`'s miss policy is `ConceptTooltipResolver`'s** (§3): the raw id, one warning per id per
session, and **never** a fall back to a developer label. A visible raw id is a bug report the player
files for free; a silently substituted English `displayName` is a bug that looks like a translation
gap and survives for months.

> **Operational note on the warning cache.** "One warning per id per session" means per **domain
> reload**, not per play session. A second Play after a first one that already logged an id prints
> nothing. **"Zero warnings" is therefore only evidence after a domain reload** — a clean Console on
> the second run proves nothing about coverage. Recompile, then run, then read.

## 7. Open decisions and findings

- **O-TXT-3 — CLOSED 2026-09-11 by TXT-4 (D-TXT4-2 = project-owned pattern).** The decision point
  TXT-1 described — the first category needing a second language — arrived three times (TXT-2
  concepts, TXT-3 statuses, TXT-4 cards) and the answer each time was the project-owned pattern.
  TXT-4 closed it in writing rather than deferring a fourth time, with the case complete for the
  first time: all six populations now sit on one asset family (five at TXT-4, the sixth at TEXT-5),
  so a migration to Unity
  Localization converts **three lists in one asset shape**, not five unrelated schemes. Presence of
  `com.unity.localization` in `Packages/manifest.json` was **verified absent**, which removes the
  "we might already have it" branch. **New review trigger:** a third language, or a real external
  translator. Those are the conditions under which String Tables buy something the CSV loop does
  not — a translator UI and a memory — and neither exists today.
- **TEXT-5 — CLOSED 2026-09-11 (code and data), doc-pass TEXT-5-DOC 2026-09-14.** Everything TXT-4
  left behind: F-TXT4-3, F-TXT4-7, F-TXT4-10 and the migrable half of F-TXT4-2, all closed by
  applying the pattern TXT-4 had already built. Decisions: **D-T5-1a=A** (`abilityId` is the key),
  **D-T5-1=A** (one concept entry per ability), **D-T5-2=B** (number culture travels on the asset),
  **D-T5-3=A** (both `"COMPOSITION"` literals, not one), **D-T5-4=A** (modal keywords resolve
  through the glossary), **D-T5-5=A** (a shared ability id is correct, not a duplicate),
  **D-T5-6=A** (an EN errata corrected — a **declared EN deviation**, second after D-TXT4-8 and on
  the same precedent). **ST-T5-1..12 PASS** except **ST-T5-5, deferred**: no card in the catalogue
  carries a keyword, so the modal keyword line has nothing to render; the step is correct and inert
  until F-TXT-3-6 is closed. R1–R3 were specified and not run.
- **TEXT-6 — CLOSED 2026-09-14 (code, data and doc-pass in one session).** It carried F-T5-1 and
  F-T5-2 and closed both, but the batch is worth reading for what it found rather than for what it
  fixed: **the two findings it inherited were both undercounted, and in the same way.** F-T5-1 said
  two generic ability bodies; the export showed **seven of seven**. F-T5-2 said three literals; the
  function showed **seven**, plus one behind `#if ALWTTT_DEV`. Both had been counted from a sample —
  a glance at the entries, a screenshot — rather than from the producer. Decisions: **D-T6-1=C**
  (ability bodies carry a character hook *and* the mechanic, never numbers and never `<link=>`),
  **D-T6-2=B** (the Spanish sweep becomes its own batch), **D-T6-3=B** (reuse an existing id when the
  concept is the same; six new `ui_track_*` for what has no home), **D-T6-4=A** (`Bundle:` stays a
  literal while it is compile-time DEV-only), **D-T6-5=A** (a shared ability id whose assets disagree
  on target gets scope-free wording; aligning the assets is a content decision, not a text one).
  **Third declared EN deviation** — seven ability bodies rewritten in English — on the D-TXT4-8 /
  D-T5-6 precedent; golden C does not cover them, since it contains card text only.
  **ST-T6-1..12 PASS** except **ST-T6-3 / ST-T6-3R, untestable**: track levels are not implemented,
  so the level branch is unreachable (§5). **Fingerprints at close:** Concepts
  `1bfbc3755e8ad348c3d5a6d4633f00ac4d90661e` (43 × 2) · Templates
  `e26fead6736ef4c7ea69d75f8badc28c6f1bd6fb` (54 × 2) · Cards `7012d489…` **byte-unchanged for the
  third consecutive batch**. **Golden C diff: 0 hunks.**
- **SONG-PANEL-1 (with 1b and 1c) — CLOSED 2026-09-14.** Ran as TEXT-6b + HUD-COMP-2 merged
  (D-NEXT-1=B). It closed F-T6-8, migrated both song-panel producers, made the hover legible and
  screen-safe, and **judged the closing criterion**. Decisions: **D-SP1-1=A** (the row shows the card
  name; the applied instrument already lives in the hover), **D-SP1-2=B** (the dev toggle lives inside
  the `#if`, confirming D-T6-4=A), **D-SP1-3=B** (migrate the producer census, not the four literals
  the batch prompt inherited), **D-SP1-4=B** (percussion floaters are diagnostics; only the chord
  fallback gets an id), **D-SP1-5=C** (the log gate is a `GigManager` contract, not the dev SO),
  **D-SP1-6=B deferred to TEXT-7** (the theme SO should hold an id, not text). **The count it was
  given was wrong by a factor of three** (F-SP1-1) and a whole producer was missing; both are recorded
  as method, not as blame. **ST-SP1-1..16 PASS** except ST-SP1-5 and ST-SP1-6, **untestable**: no
  Intro/Outro card and no multi-part song exist to exercise them, so the ids are live but inert, the
  same status as `ui_track_level`. **Fingerprints at close:** Concepts
  `1bfbc3755e8ad348c3d5a6d4633f00ac4d90661e` (43 × 2) · Cards `7012d489…` **byte-unchanged for the
  fourth consecutive batch** · Templates `af11363bd984f5c43072520ae6ebd06a6fdd803c` (73 × 2).
  **Golden C diff: 0 hunks.**
- **TEXT-VIBE-1 — CLOSED 2026-09-16. The authority question is answered and three populations
  come off the map.** Fuses VIBE-READ-1 and TEXT-7 because they shared a *question*, not a file:
  rows 8, 10 and 11 all ask whether a place that is not the glossary may hold player text.
  **D-SP1-6=B, in its strongest form — the asset holds neither the text nor an id, but the datum the
  id is derived from, and the text field is deleted** (§1). **D1 (F-TIP-6) applied:** the Vibe
  breakdown suppresses a line when **its output equals its input**, compared on **raw** values in the
  int domain each step emits — not on the formatted string, because two lines can print the same and
  differ in the third decimal, and not on «the multiplier is 1,00», which would keep a 1,004 that
  does move the number and would need a second rule for `+ 0`. Hype and the song-end line always
  print; `Blocked` and `Indifferent` are untouched, because they explain a zero. This **reverses by
  observation** the BIGNUM-1 criterion written on the function itself, exactly as D-BN-9 was reversed;
  authority `planning/Design_Vibe_Telegraph_v0_1.md` §10.2. **D3=A:** row 11 migrated in the same
  pass that rewrote its producer. Templates 73 → **102**, of which 13 are derived (§5).
  **Closes F-T6-9, F-SP1-13 (two thirds), F-TIP-2, F-TIP-6, F-TV-2 and F-TV-3; opens F-TV-1, F-TV-4,
  F-TV-5, F-TV-6 and F-TV-7.** **Fingerprints at close:** Concepts
  `1bfbc3755e8ad348c3d5a6d4633f00ac4d90661e` (43 × 2, unchanged) · Cards `7012d489…` (60 × 2,
  **byte-unchanged for the sixth consecutive batch**) · Templates
  `37516c1c1e4bac50a9dcd7bd96da840b2e402ff7` (102 × 2). **Golden diff: NOT RUN** — see F-TV-6.
  **ST-TV-1..12 PASS, no deferrals**, which itself corrects a standing note: ST-TV-4 exercised
  Indifference through the Dev Mode picker, so **ST-TXT4-14's «no content applies Indifference» is a
  content statement, not an untestability statement**.
- **TEXT-7 — SUPERSEDED by TEXT-VIBE-1 (2026-09-16). Opened by SONG-PANEL-1b; it owned the two
  asset-authored populations, together.**
  Carries **F-T6-9** (population 8, `CompositionFxConfigSO.FxEntry.label`) and **F-SP1-13**
  (population 10, `CompositionStripThemeSO` chip labels), which are the same decision asked twice,
  plus the six `CanApply` rejection strings in `SongCompositionUI` whose surface is **still
  unclassified** (log or screen — the grep that would settle it was not run). The decision it must
  open with is **D-SP1-6**: does an asset hold player text, or an id the glossary resolves? Recommended
  **B** (the asset holds an id), with the observed evidence in its favour — the theme labels render as
  `Four Four (FourFour)`, `Fast (Fast)`, `Happy`, `Sad`: the enum name is **duplicated inside the
  authored string**, which is what an id would have made impossible. **Outcome (TEXT-VIBE-1): B, and
  stronger than recommended** — not an authored id but a derived one, with the field deleted. Two
  riders of this bullet did **not** survive contact: the six `CanApply` rejection strings were not
  reclassified (still unclassified, log or screen), and `Happy` / `Sad` turned out **not to be asset
  text at all** — `MoodTag` is an enum printed by `ToString()`, so that third of the evidence belonged
  to F-TXT4-2 the whole time.
- **F-TXT4-2 — enum names on player surfaces. OPEN → CARD-FACE-1.** `TrackRole`, `PartAction`,
  `CardType`, `scope`/`timing`, style-bundle asset names, keys and time signatures reach the player
  as `ToString()` of an enum or of an asset name. **These are not strings**, so no text registry
  reaches them; the fix is symbolic (icons), which is why the finding is filed against CARD-FACE-1
  and not against a text batch. The migrable exception — the `"COMPOSITION"` literal — is **closed
  by TEXT-5** (D-T5-3=A): it lived in **two** files, not the one this bullet named, and both now
  call `ui_card_type_composition`. What remains under F-TXT4-2 is the enum half only, and the
  **enum half is explicitly outside `GAMETEXT-NO-DEV-LABEL`** (`ssot_manifest.yaml`): an enum name
  on a player surface is not a developer *label*, it is a missing symbol.
- **F-TXT4-3 — three surfaces read `CardDefinition.displayName`. CLOSED 2026-09-11 by TEXT-5.** All
  three resolve through `GameText.CardName`; §1. Closing it is what made the
  `GAMETEXT-NO-DEV-LABEL` promotion legitimate.
- **F-TXT4-7 — sixth text population. CLOSED 2026-09-11 by TEXT-5** (D-T5-1a=A, D-T5-1=A). §1 and
  §5, ability group.
- **F-TXT4-10 — decimal separator taken from the *machine's* culture. CLOSED 2026-09-11 by TEXT-5**
  (D-T5-2=B). **The title this finding carried until now was wrong and the correction matters:** it
  said *invariant* culture, but an unqualified `ToString("0.00")` uses **`CurrentCulture`**. Invariant
  would at least have been deterministic; `CurrentCulture` makes the printed line depend on the
  machine running the build, so the same Spanish build could show «× 1.00» on one computer and
  «× 1,00» on another and neither would be reproducible from the asset. `GameText.Number` now
  formats through the glossary's `numberCulture` with an **explicit** invariant fallback (§3.1).
- **D-TXT4-8 — declared EN deviation.** The `fx_grant_bonus_loop` / `fx_grant_bonus_loop_solo`
  templates changed the **English** wording of the bonus-loop line so it states what the effect does
  instead of naming the spec. This is the one place where TXT-4 did not preserve EN byte-for-byte;
  it was verified in isolation and is the only 2-line difference between goldens B and C (§9).
- **TXT-3b.** `TutorialOverlayView.SetConceptSources` still calls the `[Obsolete]` two-argument
  constructor, and `TutorialController` still holds `conceptStatusCatalogues`. Compiles with one
  `CS0618` warning by design. Closing it also closes R-1 (§3).
- **F-TXT-3-6 — card-face information density.** Keywords resolve correctly on hover but do not
  appear on the card face; the detail modal now renders them **through the glossary** (D-T5-4=A,
  TEXT-5) but no card in the catalogue carries a keyword, so the line is live and empty — which is
  why **ST-T5-5 is deferred rather than "not applicable"**: the path is unverified, not absent.
  Track type and modifiers occupy the face space. This is presentation, not text plumbing
  (`SSoT_Card_System.md` §10.1/§10.3), and any move of information from face to tooltip raises the
  stakes of **TIP-1**, still open.
- **F-T5-1 — generic ability bodies. CLOSED 2026-09-14 by TEXT-6, with its count corrected from two
  to seven (F-T6-6).** All seven were generic: four said the character intends to inflict Stress,
  three that it intends to use a special move — i.e. they restated the intention icon shown beside
  them and added nothing. They now describe what the ability does, written against the **action
  lists read out of the five `AudienceCharacterData` assets**, not against the ability names
  (D-T6-1=C). The authority for what an intention *means* remains
  `SSoT_Audience_and_Reactions.md` §7, which now carries the verified action table.
- **F-T5-2 — untranslated literals in the song panel. CLOSED 2026-09-14 by TEXT-6.** The literals
  were in `TrackHoverPanel.cs` (the inference held; `SongTrackElementUI.cs` had none), and there
  were **seven, not three** (F-T6-2). All seven now resolve through population 7. **The closing
  criterion it was attached to — *the Spanish build shows no English except enum names* — is still
  NOT met**, and this is the second batch to record it unmet rather than declare it closed. What
  remains is enumerated under F-T6-8, F-T6-9 and the TEXT-6b bullet; the difference from TEXT-5 is
  that the remainder is now a **list**, not an unknown.
  **Third reading, 2026-09-14 (SONG-PANEL-1):** still not met, and now the list is **short and every
  item has a batch**. What remains is not overlooked text: it is populations 8 and 10 (player text
  authored in assets, TEXT-7) and the six unclassified `CanApply` strings. **Every code literal in
  the song panel is migrated.** The criterion is recorded as *met for code-authored text, open for
  asset-authored text* — which is a more useful statement than the boolean it started as, and is the
  reason the next batch is a decision about assets rather than another sweep.
- **F-T6-1 — the file was not UTF-8, and it compiled by luck. CLOSED 2026-09-14 by TEXT-6.**
  `TrackHoverPanel.cs` was Windows-1252 **without a BOM**, and its `·` separators were the raw byte
  `0xB7`. With no BOM the C# compiler tries UTF-8, fails to decode, and falls back to the machine's
  ANSI code page — so the file's meaning depended on the *build machine's locale*, and on a
  differently configured machine those separators would have become mojibake or a decode error. It
  compiled on the dev box, which is exactly why nothing caught it. Fixed by transcoding to UTF-8
  with BOM and writing the separator as the escape `"\u00B7"`, so the **source stays ASCII** and the
  question cannot recur for this file. **Standing rule this leaves:** a UI file that needs a
  non-ASCII glyph writes it as an escape, and any file that reaches the text pipeline is UTF-8 with
  BOM. Worth noting the glyph choice is separately constrained: the LiberationSans SDF atlas ships
  Latin-1, so `·` (U+00B7) renders and `—` (U+2014) would draw a missing-glyph box.
  **RE-OPENED and re-closed 2026-09-14 by SONG-PANEL-1 (F-SP1-2).** The file was found back in
  Windows-1252: TEXT-6 closed this finding without ever verifying the artefact **on disk**. The byte
  sweep after the second fix explains the recurrence and corrects the rule — the replacement landed
  with `nonASCII=0` but `BOM=False`, because Unity's import strips the BOM. **The BOM is not the
  defence; the absence of non-ASCII bytes is.** Two sibling files were in the same state
  (`CompositionStripDriver.cs`, `CompositionContextRowUI.cs`, F-SP1-7) and one more remains
  (`LoopsTimerUI.cs` and others are clean; see the sweep in `CURRENT_STATE.md`). §5 carries the
  strengthened rule and §8 the closing check that would have caught it the first time.
- **F-T6-2 — a screenshot is not a census. CLOSED (as a finding) 2026-09-14 by TEXT-6.** F-T5-2
  counted three literals because three were visible in one captured frame. The producer function had
  seven, four of which only appear in states the capture did not contain (`no track yet`,
  `applies next loop`, `Lv {0} / {1}`, `max`). Recorded as a rule in §1 rather than as a defect.
- **F-T6-3 — the capture that opened F-T5-2 was a DEV build. CLOSED 2026-09-14 by TEXT-6.** `Bundle:`
  is inside `#if ALWTTT_DEV` and cannot appear in a player build. Consequence for method, not for
  code: **the Spanish-build criterion is judged in a player build**, and a DEV capture over-reports
  it. D-T6-4=A follows from this (§5).
- **F-T6-4 — a shared ability id whose two assets disagree. OPEN → content.** `ab_tantrum` is
  authored by both `Kid` and `Kid-TestTaster` (correctly shared, D-T5-5=A) but with **different
  `actionTargetType`** — `AllMusicians` on Kid, `AllAudienceCharacters` on Kid-TestTaster — so one
  glossary entry describes two different mechanics. TEXT-6 handled it in **text** (D-T6-5=A: wording
  carries no scope) which is a containment, not a fix. **The general rule this exposes:** a shared id
  is correct only while the sharers agree on what the ability does, and nothing checks that. Deciding
  whether the test asset should be aligned belongs to content/encounter work, not to a text batch.
- **F-T6-5 — the telegraphed number and the ability body can disagree. OPEN → AUD/PRES.** The value
  drawn beside the intention icon is `NextAbility.ActionList[0].ActionValue` — the **first** action's
  value, whatever that action is. `ab_heckle` shows `x1` (the Exposed stacks) and then deals 2
  Stress; `ab_block_the_view` shows `x1` (a step count) under a Special-move icon. This is the
  mechanical reason D-T6-1=C forbids numbers in ability bodies: a body saying "2 Stress" beside a
  telegraph saying `x1` would make the panel contradict itself. Presentation defect, not text.
- **F-T6-6 — F-T5-1 was undercounted (two of seven → seven of seven).** Recorded so the pattern is
  visible next to F-T6-2: **both** inherited findings were counted from a sample. The cheap habit
  that prevents it is to count from the producer — the export, the function — before writing the
  finding down.
- **F-T6-7 — `musicianId` is a visible fallback. OPEN (low), no batch assigned.** `RowData` falls
  back to `musicianId` when `musicianName` is empty, on the row and in the hover. Pre-existing,
  conformant as a fallback (the same shape as the raw-id policy in §6), recorded so it is not
  rediscovered as a defect.
- **F-T6-8 — the card name is overwritten by an asset name at runtime. CLOSED 2026-09-14 by
  SONG-PANEL-1** (D-SP1-1=A). `SongCompositionUI` reassigned `track.info` to
  `"{styleBundle.name} - {instrumentName}"` after an `InstrumentEffect` resolved, so the row showed
  `BassCard_W2_Improv - Fingered Bass` while the minicard beside it showed `Walking Bass`. **A
  `GAMETEXT-NO-DEV-LABEL` violation by data, not by literal** — see §1. **The fix was a deletion, and
  the reasoning generalises:** the defect was not badly written text but a *second writer* of a field
  that already had a meaning. Any fix that leaves a second write alive — gated, formatted, translated —
  keeps the failure class, because the next effect that wants to "reflect something" in the row will
  take the same path. The rule that replaces it is stronger than the patch: **`TrackEntry.info` is
  written once, when the track is created, and never again.** The applied instrument already had its
  own field (`overrideMelodicInstrument`) and its own reader (the hover), so the overwrite was
  redundant as well as wrong. Verified against a *second* `InstrumentEffect` on the same track
  (ST-SP1-2), which is where a gate-shaped fix would have failed.
- **F-T6-9 — an eighth text population: floater labels authored on config SOs. CLOSED 2026-09-16 by
  TEXT-VIBE-1** (D-SP1-6=B; ten `ui_fx_*` ids, the `label` field deleted, §1 row 8). Its own warning
  — *do not let it ride along in a literals batch* — was honoured: it was resolved by an authority
  decision, not by a swap. **Two of the twelve entries received no id and that is the decision:**
  `Tonality` and `Modifier` are design hooks **no code path selects**, so an id for them would be
  coverage dead weight; wiring the hook is the moment to add it. The original text follows.**
- **F-T6-9 (original record) — floater labels authored on config SOs.**
  `CompositionFxConfigSO.FxEntry.label` is player-visible text with **one language slot, no editor
  tab, no CSV and no coverage check** — the shape of population 6 before TEXT-5 fixed it.
  **Status: code truth, not observed** — read in a fresh PK copy of `SongCompositionUI`, never
  confirmed in play. It is listed as population 8 rather than folded into TEXT-6b's literal list
  because migrating it is not a literal swap: it needs an id per entry and a decision about whether
  authored FX labels belong in the glossary at all, or whether these floaters should be symbolic
  (the CARD-FACE-1 answer). **Do not let it ride along in a literals batch.**
- **F-SP1-1 — the producer census missed a producer. Recorded as method.** The batch prompt listed
  four remaining literals in `SongCompositionUI`; the producer census found **fourteen**, and a
  later Spanish sweep found a whole second producer (`CompositionContextRowUI`, population 9). This
  is the fourth occurrence of F-T6-2 and the first where the miss was a *file*, not a branch. §1
  carries the corrected habit: enumerate the producers of a surface, then count inside each.
- **F-SP1-2 — a BOM does not survive the editor; ASCII purity does. CLOSED, elevated to a rule.**
  See F-T6-1 above and §5.
- **F-SP1-3 — `"Kick"` was a family, not a literal. CLOSED 2026-09-14** (D-SP1-4=B). Two producers in
  `FloatingTextMidiListener` emit percussion labels: `OnDrumKick` (`"Kick"`) and `PercLabel` (seven
  labels). Migrating only the one the prompt named would have created two `"Kick"` strings, one in
  the glossary and one literal — the exact failure D-T6-3=B exists to prevent.
- **F-SP1-4 — `SongCompositionUI.cs` had no BOM.** Valid UTF-8, so not the F-T6-1 failure, but the
  same exposure. BOM added. Registered because the byte sweep is now a closing check (§8).
- **F-SP1-5 — a dev-only accessor gated non-dev logging, and the player build did not compile.
  CLOSED 2026-09-14** (D-SP1-5=C). `GigManager.DevSettings` is compiled out of production by DBG-C1,
  yet five log gates in three files (`HandController`, `DeckManager`, `CompositionSession`) read it
  unguarded; removing the `ALWTTT_DEV` tag for the first time surfaced all five at once. The callers
  never needed the ScriptableObject — they needed two booleans, now `GigManager.LogsEnabled` /
  `VerboseLogsEnabled`, compiled in every build. Authority for the contract:
  `SSoT_Dev_Mode.md`. **Not a text finding**, recorded here because a text batch found it.
- **F-SP1-6 — a private helper was swept inside `#if ALWTTT_DEV` by S5f while its caller was not.
  CLOSED 2026-09-14.** `DevSetSongHypeAbsolute` is called by the in-scene debug slider, which is
  guarded at *runtime*, not at compile time. Moved out of the block; the public Dev wrappers stay in.
  See `SSoT_Dev_Mode.md`.
- **F-SP1-7 — `CompositionContextRowUI.cs` was Windows-1252. CLOSED 2026-09-14 by SONG-PANEL-1b.**
  Six `0xB7` and one `0x97`. Same fix, same rule as F-SP1-2.
- **F-SP1-8 — population 9: the context-row hover was entirely in English. CLOSED 2026-09-14 by
  SONG-PANEL-1b.** Seven `ui_ctx_*` ids, eight with 1c. Found by a Spanish sweep, not by a census —
  see F-SP1-1.
- **F-SP1-9 — Overload pays for a loop another card can take. OPEN → action economy.** Observed
  2026-09-14: Overload played on loop 2 extended the part and scheduled a solo; a second Conito card
  played on loop 3 kept the extra loop but **replaced its content**. Consistent with the one-track-per
  -(musician, role) model, and still a design hole: the card buys a loop it does not own. Three
  options are on the table — playable only on the final loop; playable always but extending only on
  the final loop (recommended, with the card face telegraphing which); or the extra loop **reserved**
  for the solo, which attacks the cause rather than the timing. Authority: `Design_Action_Economy_v1.md`.
  **Not a text finding.**
- **F-SP1-10 — `"1 musicians silent"`: a literal hid a missing branch. CLOSED 2026-09-14** by the
  one/many split. See §1, population 9.
- **F-SP1-11 — `"Loop 1 de 0"`: a count asserting a total that does not exist. CLOSED 2026-09-14 by
  SONG-PANEL-1c.** `loopTotal` is 0 until `TryGetLoopProgressForUI` answers. With no total, the total
  is not printed (`ui_ctx_loop_no_total`) — the same principle D3=C applies to the mood chip: before
  the render, do not assert what cannot be backed. Found in the reporter's screenshots, not reported.
- **F-SP1-12 — the hover panel was placed using the *previous* tooltip's size. CLOSED 2026-09-14 by
  SONG-PANEL-1c.** `TrackHoverPanel` is content-sized, so `panel.rect` lags one layout pass behind the
  text just assigned; `PlaceNextTo` reads height and width to clamp, so hovering a short track row and
  then the taller context row placed the context panel with the track panel's height and it rode off
  the top edge. Going straight to the context row worked by accident (the stale size was the
  prefab's). Fixed by forcing a layout rebuild after the text is set and before measuring —
  unconditionally, because **placement must not depend on hover history**. The same
  show-measure-place ordering is the shape of **F-BN-8**, which is why this is worth reading before
  TIP-1 rather than only here.
- **F-SP1-13 — population 10: chip labels authored on `CompositionStripThemeSO`. CLOSED for meter and
  tempo 2026-09-16 by TEXT-VIBE-1; OPEN for `RoleEntry.displayName` → CARD-FACE-1.**
  `TryGetMeter` / `TryGetTempo` / `TryGetMood` return authored strings printed verbatim by
  `CompositionContextRowUI`: observed as `Four Four (FourFour)`, `Fast (Fast)`, `Happy`, `Sad`. One
  language slot, no tab, no CSV, no coverage — the shape of row 8, and **scoped with it, not
  separately** (§1). The observed strings are themselves an argument for D-SP1-6=B: the enum name is
  duplicated inside the authored label, which an id would have made impossible. **Two census
  corrections at close, and both change what the finding was about:** `Happy` / `Sad` are `MoodTag`
  **enum names**, never asset text (→ F-TXT4-2), and the finding named two `displayName` fields when
  the asset carries **three** — `RoleEntry.displayName` was never counted. The third stays open on
  purpose: its consumer was not read, and migrating an unread consumer is a guess with a template id
  attached.
- **F-TV-1 — `"Songs left: {n}"` in `GigCanvas.SetSongsLeft`. OPEN → CARD-FACE-1.** English on a
  player surface, in the very file that carried population 11, found by the producer census of that
  file. **Deliberately not migrated (D4=B):** its first call runs from `OnEnable` on a canvas in the
  persistent scene, before `TooltipManager.Instance` is guaranteed — the `Awake`→`Start` lesson of
  SONG-PANEL-1 — and a `GameText.Format` miss there would burn the id's warn-once on a failure that
  is not one. Migrating it needs `CanvasBase.cs` to establish where a safe `Start` is. Planned id:
  `ui_gig_songs_left`.
- **F-TV-2 — two more files were Windows-1252. CLOSED 2026-09-16 by TEXT-VIBE-1.**
  `CompositionFxConfigSO.cs` and `CompositionStripThemeSO.cs` carried raw `0x97` / `0xA7` bytes in
  comments with no BOM and compiled only through the compiler's code-page fallback — the F-SP1-7 /
  F-T6-1 shape, third and fourth occurrences. Both shipped from that batch as **whole-file
  replacements** with `nonASCII=0`. **The recurrence is the finding:** this failure has now appeared
  once per batch that opened a file nobody had opened since it was written, which means it is a
  property of the repo's history, not of any one author.
- **F-TV-3 — a literal left behind inside a population declared RESOLVED. CLOSED 2026-09-16 by
  TEXT-VIBE-1.** `AudienceCharacterCanvas.VibeOnlyHeader = "Vibe"` survived TXT-4's migration of the
  13 literals of population 5 because it is a `const` used as a tooltip **header**, and the census
  counted body lines. Resolved without a new id by reading the existing `vibe` keyword concept
  (D-T6-3=B). **The lesson is about the word RESOLVED:** a population marked resolved is not
  re-censused, so a literal that survives the migration that closed it may never be looked for again.
- **F-TV-4 — `$"+{bonusInt} Vibe!"` in `GigManager.ApplySfxBonusVibe`. OPEN → CARD-FACE-1.** A banked
  venue-SFX floater built by string interpolation, in a 4 150-line runtime file. Reachable as soon as
  one SFX stage is unlocked; observed during ST-TV-7. Not migrated here because `GigManager.cs` was
  outside the batch's file set, which is exactly the containment F-T6-9 asked for. Planned id:
  `ui_fx_sfx_vibe_banked`. **It is the second time in two days that a player-facing floater was found
  inside a file whose subject is not text**, which is what §8's ship-time registration rule exists to
  prevent.
- **F-TV-5 — coverage is asymmetric, and a hand-written id list can only fail on the silent side.
  Recorded as method.** A missing id is reported in the `COVERAGE` block; **an id present in the asset
  but absent from `All` is invisible** (`SSoT_Editor_Authoring_Tools.md` §20.12). When the id set is
  transcribed by a person rather than enumerated, the two errors arrive **together** — the invented id
  displaces the real one — and only one of them is reported. It happened in this batch: a meter id was
  transcribed from a screenshot as `ui_ctx_meter_fiveeight`, an enum value that does not exist, while
  `ui_ctx_meter_fivefour` — reachable in the demo, a starter-deck C2 card sets it — got no row. The
  rule this leaves: **import a derived id set from the window's own coverage output, never from a
  written list, and after any import of new ids check both directions**, rows-missing and rows-extra.
- **F-TV-6 — the regression oracle's baseline was not retained. OPEN (process).** §8 check 4 could not
  be run at TEXT-VIBE-1 close: `golden C` no longer exists. The dump is worthless alone — its value is
  the **diff** — so an oracle whose baseline lives in a downloads folder is an oracle that works until
  someone tidies up. Substituted at close by three weaker checks on the new dump (60 cards, matching
  the `60 × 2` of the Cards fingerprint; **zero raw-id tokens**, so no template miss reaches card
  text; `name == devLabel` in all 60) plus the argument from construction that **none of the 29 new
  ids is consumed by the card builder** — no `fx_*`, no `tgt_*`, no `ui_card_*`. That is an argument,
  not a measurement, and is recorded as such. **The 2026-09-16 dump is promoted to `golden D`** and
  must live under version control, not in a downloads folder.
- **F-TV-7 — `PartEffect.GetLabel()` is an uncensused producer of player text. OPEN → CARD-FACE-1.**
  The detail modal prints one line per modifier from each `PartEffect`'s `GetLabel()`
  (`SSoT_Card_System.md` §10.3). The `golden D` dump shows `Meter 5/4`, `Tempo: Slow` (×7),
  `Tempo ×0.66`, `Mode Ionian`, `Mod → degree Mediant ↑`, `Random Instrument (2)` (×3),
  `Set Instrument: Fingered Bass`. **The enum half is already F-TXT4-2; the literal half is new** —
  `Meter`, `Tempo:`, `Mode`, `Random Instrument`, `Set Instrument:`, `degree` are English words
  written in code, in the effect classes: a location neither census method visits. **It also creates a
  second home for one concept**: the context row now prints `5/4` from `ui_ctx_meter_fourfour`'s
  sibling while the modal prints `Meter 5/4` from code — the exact divergence D-T6-3=B exists to
  prevent. Whether these are literals or `ToString()` cannot be stated as code truth: the producer
  file was not attached. **Request it explicitly in CARD-FACE-1 before counting.**
- **F-TXT-2-1 — CLOSED 2026-09-11 by TXT-4.** The 13 `AudienceCharacterCanvas` literals are now
  `ui_aud_*` templates plus the `blocked` concept; §1 population 5.
- **F-TXT-3-5 — CLOSED 2026-09-11 by TXT-4.** Card names and card descriptions are per-language;
  §1 populations 3 and 4. The three residual `displayName` readers are tracked separately as
  F-TXT4-3.
- **F-TXT4-11 — panel stacking is a wanted feature on another surface. OPEN → TIP-1.** TXT-4
  observed `TooltipManager.ShowTooltip` stacking panels in the live build (status + intention, two
  panels) and observed that this is exactly what the audience canvas wants. **§4 is not affected and
  is not amended here**: the single-panel contract governs concept hovers in the tutorial overlay and
  remains a hard invariant. What changes is the framing of **F-BN-7** for the batch that owns it —
  it is a capability with no opt-out, not a pure defect.
- **F-TXT4-8 — bar visibility and bar interactivity are decoupled. OPEN → TIP-1.** Recorded here
  because it blocked two TXT-4 smokes; the home for the fix is
  `SSoT_Audience_and_Reactions.md` §6.4.
- **D-TAG-10 — CLOSED 2026-09-10 by TXT-3.** T-TAG-1 resolved: no player-facing surface reads a
  single-slot text field for statuses or keywords.

## 8. Update rule

Update this document when a change affects: which population stores a given kind of player text; the
tag syntax; **the declared rich-text vocabulary or the sprite naming rules (§2.1)**; the resolution
path, its registry or its entry points; **the template id set (`GameTextTemplateIds.All`)**; the miss
policy or the coverage rule; **the ability-id coverage enumerator or the set of content classes it
walks (§5)**; the hover contract or its relationship to `systems/SSoT_Tooltip_Pipeline.md`; the parity checks; or the CSV
interchange contract for text. **Also update it when a new population is found, including one that is
found and left open** (TEXT-6, row 8): the map's value is that it lists what is unsolved, and a
population kept in a batch's notes instead of in the table is a population the next census will miss
again. **And when player text is discovered to be assigned at runtime rather than authored**
(F-T6-8): that is a third failure shape beside "literal in code" and "text field on an SO", and the
census methods for the first two do not find it. **And when a surface turns out to have more than one
producer** (F-SP1-1): populations 7 and 9 are two files rendering two rows of one panel, and the
census that read only the first reported the panel done. **And when a template id set stops being a
literal list** (TEXT-VIBE-1, D5): `GameTextTemplateIds.All` now enumerates two package-owned enums at
type initialisation, so the id set has a *rule* as well as a membership, and the rule is what this
document has to carry — §5.

**Closing checks this document now requires of a batch that touches player text.**

1. **Bytes on disk.** Every `.cs` the batch delivered is verified with the byte sweep before close —
   the criterion is **`nonASCII=0`**, not `BOM=True` (F-SP1-2). F-T6-1 was closed twice because the
   first close verified the *artefact sent*, not the *file in the repo*.
2. **Producers, then literals.** Enumerate the producers of the surface, then census each (F-SP1-1).
3. **Fingerprints, all three.** A batch that changes none of Concepts / Cards states so explicitly;
   an unexpectedly unchanged fingerprint is as informative as a changed one.
4. **Golden diff.** Unchanged unless the batch declares an EN deviation. **The baseline is part of the
   check** (F-TV-6): a dump without the previous dump proves nothing, so the current golden lives under
   version control and every batch that regenerates it says which golden it diffed against. Current
   baseline: **`golden D`, 2026-09-16** (`golden C` was lost, and the TEXT-VIBE-1 check went unrun).
5. **Both directions of coverage** (F-TV-5). The `COVERAGE` block reports ids that are **missing**; an
   id present in the asset and absent from `All` is **invisible** to it. After importing new ids, check
   rows-missing *and* rows-extra — the second by filtering the tab by id prefix and counting against the
   enumerator. A transcribed id set fails on both sides at once and reports only one.
6. **A derived id family is imported from the window, not from a document.** When ids come from an
   enumerator, the authoritative list of *values* is the tab's own coverage output. No `.md`, no batch
   prompt and no chat message is a source for that list.

**Regression oracle.** `ALWTTT ▸ Text ▸ Dump Card Text (EN/ES)` writes the full rendered card text
for every card in both languages. Its value is the **diff**, not the dump: TXT-4's proof that the EN
build was unchanged is `golden A → B` (60 hunks, all of them `name:`, with `name:` equal to
`devLabel:` in all 60) and `golden B → C` (2 lines, both the declared D-TXT4-8 deviation). Any batch
that touches templates, the builder or card names regenerates it and diffs before closing.

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
| 2026-09-11 | TEXT-5 | §1 **all six populations solved**: row 6 moved into `ConceptGlossarySO.entries` keyed by `AudienceAbilityData.abilityId` (D-T5-1a=A, D-T5-1=A), F-TXT4-3 closed, population-2 consumers 4 → 6. §3.1 → `GameText` gains **numbers** (`GameText.Number`, D-T5-2=B) and the glossary gains `numberCulture`. §5 → a **fourth concept group, Ability (7)**, whose id set is enumerated from content assets by `AudienceAbilityTextIndex` rather than declared in code (with the `EMPTY:` sentinel that follows from that); counts 36 → **43** concepts and 47 → **48** templates (`ui_card_type_composition`, D-T5-3=A, **two** literal homes not one). §6 → two ability coverage rows. §7 → F-TXT4-3, F-TXT4-7 and F-TXT4-10 **closed** (the last with its **title corrected**: `CurrentCulture`, not invariant culture), the migrable half of F-TXT4-2 closed, TEXT-5 recorded with D-T5-1a/1/2/3/4/5/6, **TEXT-6 opened with F-T5-1 and F-T5-2**, ST-T5-5 recorded as deferred. §8 → one new trigger. **Closing criterion NOT met** and recorded as such (F-T5-2). **Fingerprints at close:** Concepts `7e22a03fa95f3c9f7d18ed93b86e1be826551f3f` (43 × 2) · Cards `7012d4898044ef3e67c5bd2587be50786e31815b` (60 × 2, **byte-unchanged since TXT-4** — the proof that the batch moved readers, not the registry) · Templates `feb4add82dfa4d478c7c6a48a744c1c895d43ca5` (48 × 2). **Golden C diff: 0 hunks.** ST-T5-1..12 PASS except ST-T5-5 (deferred, F-TXT-3-6); R1–R3 specified, not run. `GAMETEXT-NO-DEV-LABEL` promoted to a hard invariant in `ssot_manifest.yaml` (D-DOC-1=C criterion met). |
| 2026-09-11 | TXT-4 | §1 rewritten: population 3 **moved** into `ConceptGlossarySO.cardNames` (`CardDefinition.displayName` → developer label, same criterion as TXT-3), population 4 redefined as **stored fragments composed at call time**, population 5 **resolved** (`ui_aud_*` + `blocked`), **population 6 added** (F-TXT4-7). **§2.1 new** — declared rich-text subset, sprites by name, one global Sprite Asset, the `tgt_*` article rule (F-TXT4-5). **§3.1 new** — two entry points (`ConceptTooltipResolver`, `GameText`), one registry, one seat, with the `SetEditorFallback` exception declared. §5 → three id spaces (36 + 60 + 47) and the `GameTextTemplateIds.All` double duty. §6 → five new editor failure modes, the `GameText` miss policy, and the domain-reload note on the warning cache. §7 → **O-TXT-3 CLOSED** (D-TXT4-2, project-owned); F-TXT-2-1 and F-TXT-3-5 closed; F-TXT4-2/3/7/10 and TEXT-5 opened; D-TXT4-8 EN deviation declared. §8 → two new triggers. **Fingerprints at close:** Concepts `6632813b43573214aa32fdfc5b5ae20680c24e8f` (36 × 2, unchanged since TXT-3) · Cards `7012d4898044ef3e67c5bd2587be50786e31815b` (60 × 2) · Templates `d6ffe2128873cc3ead794136fc5879650c6ecc80` (47 × 2). ST-TXT4-1..12 PASS; ST-TXT4-13 (inline sprite) and ST-TXT4-14 (Indifference line) deferred with reason. |
| 2026-09-14 | SONG-PANEL-1 / 1b / 1c | §1 → **two new populations**: row 9 (context-row hover literals, RESOLVED via `ui_ctx_*`) and row 10 (**OPEN** — chip labels authored on `CompositionStripThemeSO`), header 8 → 10; **rows 8 and 10 explicitly scoped together** as one authority question about asset-authored text; two method notes added (a surface can have several producers, F-SP1-1; a literal can hide a missing grammatical branch, F-SP1-10). §5 → templates 54 → **73**; the three SONG-PANEL-1 rules (serialized English defaults are **deleted**, not migrated; a glossary read has an `Awake`/`Start` order a literal does not; part labels unified to letters); **D-T6-4=A CONFIRMED** by D-SP1-2=B after the re-open condition was tested and did not trigger; **the BOM rule corrected** — ASCII source is the defence, `nonASCII=0` the check (F-SP1-2). §7 → **F-T6-8 CLOSED** with the generalised rule (`TrackEntry.info` is written once), **F-T6-1 RE-OPENED and re-closed** (TEXT-6 never verified the file on disk), **SONG-PANEL-1 closed** with D-SP1-1..5 and **TEXT-7 opened** owning populations 8 + 10 together; F-SP1-1..13 recorded, of which F-SP1-9 (Overload's extra loop) and F-SP1-13 remain OPEN. §8 → one new trigger and **four closing checks**. **Fingerprints at close:** Concepts `1bfbc3755e8ad348c3d5a6d4633f00ac4d90661e` (43 × 2) · Cards `7012d4898044ef3e67c5bd2587be50786e31815b` (60 × 2, **byte-unchanged for the fourth consecutive batch**) · Templates `af11363bd984f5c43072520ae6ebd06a6fdd803c` (73 × 2). **Golden C diff: 0 hunks.** ST-SP1-1..16 PASS except ST-SP1-5 / ST-SP1-6 (untestable: no Intro/Outro card, no multi-part song). **Closing criterion: met for code-authored text, open for asset-authored text.** |
| 2026-09-16 | TEXT-VIBE-1 | §1 → the authority question is **answered** (D-SP1-6=B, strongest form: the asset holds the **datum**, the text field is deleted); rows 8 and 11 **RESOLVED**, row 10 resolved for meter and tempo with `RoleEntry.displayName` left open and its consumer named as unread; two census corrections recorded (`MoodTag` was never asset text; the theme carries **three** text fields, not two); the count line goes to «one population open» plus three loose literals and one unread candidate. §5 → templates 73 → **102** and the **derived id family** contract (D5): `All` = `Fixed` + one id per `TimeSignature` + one per `TempoRange`, coupled by enum **name** for the §8.10 boundary reason, with inert ids kept. §7 → **TEXT-VIBE-1 closed**, TEXT-7 superseded, **F-T6-9 CLOSED**, **F-SP1-13 closed in two thirds**, F-TIP-2 / F-TIP-6 closed, and **F-TV-1..7 opened** — of which three are method findings (F-TV-5 coverage asymmetry, F-TV-6 the oracle's lost baseline, F-TV-3 «RESOLVED means nobody looks again»). §8 → one new trigger and **two new closing checks** (both directions of coverage; a derived family is imported from the window). **Fingerprints at close:** Concepts `1bfbc3755e8ad348c3d5a6d4633f00ac4d90661e` (43 × 2) · Cards `7012d4898044ef3e67c5bd2587be50786e31815b` (60 × 2, **byte-unchanged for the sixth consecutive batch**) · Templates `37516c1c1e4bac50a9dcd7bd96da840b2e402ff7` (102 × 2). **Golden diff NOT RUN** (F-TV-6); `golden D` established. **ST-TV-1..12 PASS, no deferrals.** |
| 2026-09-15 | TIP-1-DOC | §1 → **eleventh population** (row 11, C1 readout tooltip, OPEN as F-TIP-2) and the count line corrected to 11/8/3; the SONG-PANEL-1 closing claim **narrowed**, with the method rule that a new hover/floater/readout registers its own row at ship time. §4 rewritten: F-BN-7 and F-BN-8 **closed** at TIP-1, so the Hide→Show contract is restated as conduct instead of as a workaround, and `UpdateTooltip` is named as the entry this surface deliberately does not use. §8 trigger repointed from «TIP-1» to the new pipeline SSoT. Authority moved out: the folder `Assets/Scripts/UI/Tooltips` is no longer governed wholesale by this document — `ssot_manifest.yaml` splits it by file, leaving `GameText.cs`, `GameTextTemplateIds.cs`, `ConceptTooltipResolver.cs` and `ConceptTagRenderer.cs` here. No template, id, fingerprint or glossary entry changed. |
| 2026-09-14 | TEXT-6 | §1 → **two new populations**: row 7 (song-panel track hover literals, RESOLVED via `ui_track_*` + reuse of `ui_card_detail_modifiers`) and row 8 (**OPEN** — floater labels authored on `CompositionFxConfigSO`, monolingual, no tab/CSV/coverage); header 6 → 8 populations, and the census is explicitly **not** declared closed. Three method notes added: census the producer and not the captured frame (F-T6-2), a DEV capture over-reports the Spanish criterion (F-T6-3), and **player text can be assigned at runtime** rather than authored (F-T6-8 — `track.info` overwritten with `"{styleBundle.name} - {instrumentName}"`, observed on camera; a `GAMETEXT-NO-DEV-LABEL` violation by **data**, not by literal). §5 → templates 48 → **54**; the **when-NOT-to-add-an-id** rule (D-T6-3=B, one concept one id across surfaces); what stays in code and why (punctuation, enum names, `#if ALWTTT_DEV`); `ui_track_level` / `_level_max` recorded **live but inert** (track levels unimplemented). §7 → **F-T5-1 and F-T5-2 CLOSED**, both with **corrected counts** (2→7 bodies, 3→7 literals); **TEXT-6 closed** with D-T6-1/2/3/4/5 and a **third declared EN deviation** (seven ability bodies, D-TXT4-8 / D-T5-6 precedent); **TEXT-6b opened**; F-T6-1..9 recorded (F-T6-1 the file was Windows-1252 without BOM and compiled only by code-page fallback — CLOSED; F-T6-4 shared id with divergent assets, F-T6-5 telegraph reads `ActionList[0]`, F-T6-8, F-T6-9 **OPEN**). §8 → two new triggers. **Fingerprints at close:** Concepts `1bfbc3755e8ad348c3d5a6d4633f00ac4d90661e` (43 × 2) · Cards `7012d4898044ef3e67c5bd2587be50786e31815b` (60 × 2, **byte-unchanged for the third consecutive batch**) · Templates `e26fead6736ef4c7ea69d75f8badc28c6f1bd6fb` (54 × 2). **Golden C diff: 0 hunks.** ST-T6-1..12 PASS except ST-T6-3 / ST-T6-3R (untestable: mechanic absent). **Closing criterion still NOT met**, now with an exact remainder list. |

# CSV — Composition Session Validation & Content Sub-Roadmap

**Status:** active (opened 2026-07-18; CSV-1 + CSV-2 + CSV-1b + CSV-1c closed 2026-07-18; **CSV-4 closed PARTIALLY 2026-07-20** — listening pass and naming application deferred)
**Classification:** planning-only. This document is **not** implementation authority.
Implemented truth lives in the governed SSoTs named per batch; current operational
reality lives in `CURRENT_STATE.md`.
**Arc label:** `CSV` (Composition Session Validation).
**Predecessor:** MGP-ALWTTT-DBG consumer arc (DBG-C1 + DBG-C2), closed 2026-07-17 —
see `planning/active/Roadmap_ALWTTT_Debug_Seams.md`.

---

## 1. Purpose

The DBG arc gave ALWTTT a read/write debug seam into the composition pipeline
(`SSoT_Dev_Mode.md` §18). What it did **not** give is the ability to (a) see the
composition asset inventory as a whole, (b) audition instruments, (c) audition a
catalogue card's musical side, or (d) judge whether the shipped musical content is
any good. This arc closes that gap and then spends the resulting visibility on
content quality, bass-pipeline validation, and the third band member.

Two larger design systems (song persistence, track evolution on card replay) are
registered here so they are traceable, but they are **queued behind** the tooling and
content work and each will need its own design pass before implementation.

Relationship to the demo cut: this arc is **parallel to / behind S5i**. It does not
displace the S5i → S5j sequence that gates the demo cut. The one exception is
**CSV-6** (Conito + starter restructure), which changes the starter deck contract and
therefore interacts with S5i tuning — see §6.

---

## 2. Requirements registry

Every requirement raised at arc open, with its ownership side and the batch that owns it.
Requirement IDs are stable; batch assignment may move.

| ID | Requirement | Owner | Batch |
| --- | --- | --- | --- |
| **CR-1** | Persist a played song (parts, tracks, seed / resulting rolls) so it can be reloaded and heard again | Mixed — ALWTTT owns the pre-handoff truth, MidiGenPlay owns bit-reproducibility | CSV-7 |
| **CR-2** | Playing the same composition card on an already-formed track should **evolve** it rather than be a no-op or a plain replace. Variants raised: parallel-key change (C → Cm), relative-key change (C → Am), re-roll articulation, substitute 1–2 chords, swap to another progression in the same key/mode | Mixed — game meaning is ALWTTT, musical transform is composer-side | CSV-8 |
| **CR-3** | Assign melodic / percussion instruments from the Dev Composition tab | ALWTTT | CSV-2 |
| **CR-4** | Curate the instrument catalogue — identify and retire instruments that sound bad | Mixed — assets are package-side, the keep/cut judgement is game-side | CSV-4 |
| **CR-5** | Walking-bass basslines | **MidiGenPlay** | Ask `MGP-ALWTTT-WBASS-1` |
| **CR-6** | Third band member: Conito (bassist). Focused bass testing; each Bassline card should have ≥5 accepted bassline options | ALWTTT | CSV-6 |
| **CR-6b** | Starter deck restructured to 4 cards × 3 musicians = 12 (2 composition, 1 action, 1 free-type finisher with inspiration cost); the rest move to rewards; ≥2 clearly distinct, validated composition cards per musician | ALWTTT — **contract change** to `Design_Starter_Deck_v1.md` §4 | CSV-6 |
| **CR-7** | Validate bassline generation — reports of basslines not following the progression or ending early | Observation is ALWTTT; **causes are package-side** | CSV-5 → asks. **Priority recalibrated 2026-07-20 (D-CSV-23):** the 8-measure content standard in CR-10 extinguishes `BASS-GAP` on most future content by construction, so `MGP-ALWTTT-BASSFILL-1` drops from *audible demo defect* to *robustness gap* — see §5. Not withdrawn |
| **CR-8** | Review every existing composition option one by one; retire/repair progressions and patterns that are boring or over-repetitive | ALWTTT (curation) over package assets | CSV-4 |
| **CR-9** | Inventory window / wizard for the whole composition inventory (style bundles, patterns, progressions, instruments) with filtering and export-to-log; establish naming conventions, which currently do not exist | ALWTTT (editor tooling) | CSV-1 (tooling) + CSV-4 (conventions) |
| **CR-10** | Chord content standard: progressions default to **8 measures** (matching the part) so the chord articulator has two passes over the same material and can vary without sounding random; the Modal palette dissolves into Core Major / Core Minor by tonic so each palette carries modal colour instead of being restricted to diatonic major/minor | ALWTTT (content) | **CSV-6** |

---

## 3. Decisions locked at arc open (2026-07-18)

| ID | Decision | Resolution |
| --- | --- | --- |
| **D-CSV-1** | What opens first | **A** — tooling first: CSV-1 + CSV-2 in one session. Nothing downstream can be judged without an inventory and an instrument A/B surface. |
| **D-CSV-2** | How a song is persisted (CR-1) | **A** — persist *input + seed* (the pre-handoff `SongCompositionUI` model + `DevPinnedSongSeed`), not a serialized `SongConfig`. Governed by `SSoT_Runtime_Song_Model_and_Config.md` §3.1: `SongConfig` is package-side truth **after** handoff; the game-side editable/session truth before handoff is `SongCompositionUI` + `CompositionSession`. Persisting the package-side object would invert that ownership. MIDI export, if the package already exposes a `MidiFile` write path, is an acceptable cheap extra — it satisfies "hear it again" but not "load it again". |
| **D-CSV-3** | Default key relationship on card replay (CR-2) | **A** — root-preserving parallel change (C → Cm) as the default; per-card authoring as a later extension. Rationale: predictability for the player and a telegraph-able label. The relative-minor option is musically nicer but breaks the "this card does this thing" mapping. Locked as *default*, not as the full CR-2 design — the four evolution variants are still an open design surface for CSV-8. |
| **D-CSV-4** | Starter deck shape (CR-6b) | **Accepted as stated:** 4 × 3 = 12 (2 composition, 1 action, 1 free-type finisher with inspiration cost). This supersedes `Design_Starter_Deck_v1.md` §4 (17 documented / 19 actual) and dissolves the open Sibi-Bassline `StarterDeck`-flag debt. Applied at CSV-6, not before. |
| **D-CSV-5** | How a dev instrument override reaches the render (resolved CSV-2, 2026-07-18) | **A, refined.** Write `TrackEntry.override*Instrument` directly — the fields already participate in `trackInputsHash`, so the stem cache stays coherent by construction and no bypass machinery is needed; `SongCompositionUI` / `SongConfigBuilder` / `MidiMusicManager` are untouched. **Refinement forced by code inspection:** assign and clear must route through a new `CompositionSession.DevInvalidateForInstrumentOverride(partIndex)` that invalidates with `keepInstruments: **false**` — mirroring the instrument-**card** path, not the `DevOverrideStamp` pattern path. The pattern path preserves `PartCache.resolvedMelInstByTrack`, which is re-fed into the next render as `instrumentOverrides`; keeping it would let a stale voice beat the new pick. Card supersession (`ApplyInstrumentEffect`) is accepted and detected, not fought. Homes: `SSoT_Dev_Mode.md` §18.9 + `SSoT_Runtime_CompositionSession_Integration.md` §8 inv 9. |
| **D-CSV-6** | Documentary home for a read-only browser over package-owned assets (resolved CSV-1, 2026-07-18) | **A.** `SSoT_Editor_Authoring_Tools.md` §17 (ALWTTT), mirroring `CardInventoryWindow`. `SSoT_Authoring_Tools.md` §4 assigns package documentation to tools that *author or edit* package assets; this one authors nothing. Documenting it package-side would have promoted a read-only game-side curation tool into package documentation authority, against the boundary rule. |
| **D-CSV-11** | Are `ChordProgressionData.Measures` / `TimeSignature` authored at all? (resolved CSV-1c, 2026-07-18) | **Dismissed — yes, they are authored.** The `Measures = 0` / `TimeSignature = FourFour` observation was an artifact of a sample that contained only the dead set. The corrected export shows the 30 live progressions carrying real values (measures 1/2/4/5/6/8; TS FourFour ×24, ThreeFour ×2, FiveFour, SixEight). No package ask. **Consequence: `BASS-GAP` is a real curation signal, and it fires on 27 of the 30 live progressions** — see §4.1.1. |
| **D-CSV-12** | How pattern discovery is fixed (resolved CSV-1c, 2026-07-18) | **A + B.** Union `PatternRepositoryResources` / `InstrumentRepositoryResources` with `AssetDatabase.FindAssets<T>()`, plus a reference harvest over palettes, libraries and style bundles. Verified: chord progressions 13 → **48**, and the harvest was a no-op (`HARVESTED = 0`), meaning the AssetDatabase union alone was sufficient. Option **C** (change `resourcesPatternsRoot` or move the assets) was deliberately NOT taken here — it is the correct fix for **runtime** discovery, not for the window, and it is an asset/package-config change. It is now registered as **D-CSV-14**. |
| **D-CSV-10** | Compile gate for `CompositionInventoryWindow` (resolved CSV-1, 2026-07-18) | **A** — `#if UNITY_EDITOR && ALWTTT_DEV`, literal compliance with the batch constraint. Relaxing to plain `UNITY_EDITOR` (the `CardInventoryWindow` precedent) is a one-line change with identical zero-ship footprint; revisit if curation work is wanted without the dev define. **Note:** an earlier draft of this decision was mislabelled `D-CSV-7`, which collides with the naming-ownership question already registered under that ID. `D-CSV-7` retains its original meaning; the gate decision is `D-CSV-10`. |
| **D-CSV-7** | Who owns asset naming conventions once they exist (resolved CSV-4, 2026-07-20) | **A — asset ownership is location-based.** `Assets/` is ALWTTT's; `Packages/` is MidiGenPlay's. ALWTTT never renames or deletes package-side; it files asks. **Rider: naming authority ≠ moving authority** — a rename must not change an asset's position relative to a Resources scan root (that is D-CSV-14, CSV-5). Convention drafted in `planning/Design_Asset_Naming_v0_1.md`; application is CSV-4b. |
| **D-CSV-15** | Melody patterns vs phrase palettes — is the pattern mechanism dead? (resolved CSV-4, 2026-07-20) | **Both mechanisms retained.** Melody is phrase-driven in current card content (every `MelodyCardConfigSO` uses `phrasePaletteOverride`), *and* `MelodyCardConfigSO.patternOverride` is retained **deliberately** as the landing surface for a future MIDI-import path (human-composed DAW melodies → game-readable melody patterns). Neither mechanism is deprecated. The two authored local patterns are kept. This resolves worklist item B. |
| **D-CSV-16** | Card → bundle reverse index (resolved CSV-4, 2026-07-20) | **A, pending execution.** The index is owed. It moved from *nice to have* to **blocking**: with the test bundles deleted, liveness of the Modal and Test palettes could only be established from the user's statement, not from tooling. Scope **not yet assigned to a batch**. Gap surface: `SSoT_Editor_Authoring_Tools.md` §17.10. |
| **D-CSV-18** | What instrument curation actually operates on (resolved CSV-4, 2026-07-20) | **A.** All 79 instruments report `source: pkg`, so curation targets the **pools** (`InstrumentRules` + per-musician whitelists, ALWTTT-owned), never the assets. Asset-level defects become package asks (D-BAG-3). **Nothing sounds in the demo without an explicit listening verdict** — and that verdict is currently blocked, see §4.1.1. **Escucha entregada (CONT-B, 2026-07-31).** El veredicto que esta decisión declaraba bloqueado ya existe: los ocho bajos se distinguen, los slaps leen tras la corrección de rango, y Synth 1/Synth 2 cumplen su separación sub/melódico. **Único desequilibrio medido:** Fingered por debajo de Slap, y la causa es de **payload, no de instrumento** — los boosts de Pocket son exclusivos de las lanes slap/pop. Consecuencia para esta decisión: la curación a nivel de pool no resuelve desequilibrios que nacen de la configuración de carta. Ver **D9** (§6.2 de `CONT-B_Doc_Diffs`). |
| **D-CSV-19** | Where rename/lifecycle operations live (resolved CSV-4, 2026-07-20) | **A.** The renamer is a **separate editor window**, not a mode inside `CompositionInventoryWindow`. The inventory window keeps its read-only invariant (§17.2 / ST-CSV-7) so it stays trustworthy as the verification surface before and after a rename run. Batch label **CSV-4b**. |
| **D-CSV-21** | Scope of the listening pass (resolved CSV-4, 2026-07-20 — **superseded**) | **C, then superseded by D-CSV-23.** Listening scope was set to the 14 reachable progressions (Core Major 8 + Core Minor 6); Test's 4 were cut without listening; Modal's 10 were deferred. D-CSV-23 then redirected Modal from *deferred* to *merged*, so this is recorded as **resolved-and-superseded**, not as live guidance. |
| **D-CSV-22** | Order of CSV-3 vs the melody investigation (resolved CSV-4, 2026-07-20) | **B.** CSV-3 runs **before** the melody investigation, because R2a is the instrument that makes the investigation tractable. |
| **D-CSV-23** | Chord content standard (resolved CSV-4, 2026-07-20) | **A+B.** **(A)** Default progression length becomes **8 measures**, matching the 8-measure part, applied to *new and repaired* content — **not** a mass re-authoring of the existing 4-measure set. **(B)** The Modal palette is dissolved into Core Major / Core Minor by tonic, so each palette carries modal colour rather than being restricted to diatonic major/minor. Registered as **CR-10**; executed at **CSV-6**, not CSV-4. |
| **D-CSV-8** | R2a: live session model or throwaway shadow? (resolved CSV-3, 2026-07-22) | **A — live model.** The change is real and persistent for the loop; it reuses the seeded `PlaySinglePartLoop`, no shadow model, no second playback channel. |
| **D-CSV-13** | §18.6 dev Backing dropdown source (resolved CSV-3, 2026-07-22) | **A — stays `PatternRepositoryResources`-fed** (runtime-honest) + in-tab notice. The list is empty/small **by measurement** until the CSV-5 scan-root fix (D-CSV-14); switching to the inventory union was rejected (editor-only `AssetDatabase` in a runtime tab). The question dissolves once D-CSV-14 lands. |
| **D-MEL-1** | Who owns a part's meter (resolved CSV-3, 2026-07-22) | **A.** A rhythm card carries a matching `MeterEffect`; a part's meter is a model-construction default (`FourFour`) mutated only by a `MeterEffect` (Pentameter precedent). Homes: `SSoT_Runtime_CompositionSession_Integration.md` §12 + `SSoT_Card_Authoring_Contracts.md` §5.16. |
| **D-CSV-24** | Economy of an injected audition track (resolved CSV-3, 2026-07-22) | **B — economy-neutral.** Injected tracks are excluded from `EvalPerLoopInsp`; a genuine play on the same key reclaims; the set clears at song boundary. Option A (no exclusion) accepted first, superseded to B for parity with a real play. |

**Open at arc open**

| ID | Question | Where it resolves |
| --- | --- | --- |
| **D-CSV-9** | The full CR-2 evolution surface: which of the four variants ship, and whether replay-evolution is card-authored or global | CSV-8, needs its own design pass |
| **D-CSV-14** | Whether the Resources scan roots are corrected (move assets, or repoint `MidiGenPlayConfig.resourcesPatternsRoot` / add roots) so the runtime repositories can see the in-use content. **Scope reduced 2026-07-20 (CSV-4):** `Packages/.../Resources/ScriptableObjects/Chord Progressions/` no longer exists — it moved to `Samples/` in MidiGenPlay 1.1.0 — so the three pattern roots `Patterns/{Chords,Drums,Melodies}` are now the **only** package-side scan roots, and the remaining mismatch is **exclusively Assets-side and ALWTTT's to fix**: local chords live under `ScriptableObjects/Chord Progressions/{Major,Minor,Modal,Tests}`, and two local melody patterns sit under `Patterns/Melody` (singular) while the correct root is `Patterns/Melodies` (plural) — evidenced by `Test_Scale_Melody_4-4_4m_14n`, authored in the plural folder and *not* flagged `OFF-ROOT`. **No longer cross-boundary.** **Resolución probable (CONT-B, 2026-07-31) — verificar antes de cerrar.** La consolidación de contenido de CONT-B movió los 44 assets de progresión a `Assets/Resources/ScriptableObjects/Patterns/Chords`, la raíz que escanean los repositorios; el inventario reporta **cero `OFF-ROOT`** en assets locales de progresión y percusión. La verificación indirecta es T0.1 de la pasada de escucha (PASS): el dropdown de Backing del Dev Mode ofrece contenido local real, que era el síntoma medido. **Pendiente:** confirmar que los dos patrones de melodía bajo `Patterns/Melody` (singular) siguen siendo el residuo, y cerrar formalmente. Con D-CSV-14 cerrada, **D-CSV-13 se disuelve** tal como su propia resolución anticipaba. | CSV-5 |

---

## 4. Batch sequence

Strict left-to-right; no two open in parallel. Rationale for the order: you cannot
curate what you cannot list (CSV-1 before CSV-4), you cannot judge an instrument you
cannot A/B (CSV-2 before CSV-4), you cannot judge a card without hearing it (CSV-3
before CSV-4), and Conito is gated on the bass pipeline being trustworthy (CSV-5
before CSV-6) — which is the prerequisite `Roadmap_ALWTTT.md` §Future milestones →
Roster Expansion already states.

| Batch | Scope | Type | Owning doc(s) on close | Status |
| --- | --- | --- | --- | --- |
| **CSV-1** | Composition Inventory Window — read-only browser over every composition asset family, with filters, derived health columns, Print + Export JSON | Editor tooling | `SSoT_Editor_Authoring_Tools.md` §17 (D-CSV-6=A locked) | **closed 2026-07-18** |
| **CSV-2** | Dev instrument overrides in the Composition tab — melodic + percussion pickers per track, sibling of §18.4 | Dev/runtime (`#if ALWTTT_DEV`) | `SSoT_Dev_Mode.md` §18.9 | **closed 2026-07-18** |
| **CSV-1b** | Inventory palette-discovery fix — union the palette/library store scan with `AssetDatabase.FindAssets` | Editor tooling (micro-batch) | `SSoT_Editor_Authoring_Tools.md` §17.4/§17.7 | **closed 2026-07-18** |
| **CSV-1c** | Pattern + instrument discovery union, reference harvest, `OFF-ROOT`/`HARVESTED` flags, Export All | Editor tooling (micro-batch) | `SSoT_Editor_Authoring_Tools.md` §17.3/§17.4/§17.6 | **closed 2026-07-18** |
| **CSV-3** | R2a debug-play — audition any catalogue card's musical side, no cost, no gameplay effects. **Scope amended 2026-07-20:** plus resolved-meter / resolved-tonality read surfaces and the **melody-path investigation** (§4.1.1 item 7). Owns **D-CSV-8**, **D-CSV-13**, **D-MEL-1** | Dev/runtime (`#if ALWTTT_DEV`) | `SSoT_Dev_Mode.md` §18 + `SSoT_Runtime_CompositionSession_Integration.md` | **closed 2026-07-22** (code + smokes; doc-pass CSV-3-DOC) |
| **CSV-4** | Curation & naming pass — walk the inventory, produce keep/cut lists for progressions, drum patterns, melody patterns and instruments; establish naming conventions | Design / content | `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §4.3/§8.2 · `SSoT_Editor_Authoring_Tools.md` §17 · new `planning/Design_Asset_Naming_v0_1.md` | **partially closed 2026-07-20** — executed: worklist A, MGP-BAGGAGE-1 (filed + resolved package-side, 1.1.0 adopted), the 183-asset re-baseline, the naming-convention draft, D-CSV-7/15/16/18/19/21/22/23, green gig smoke. **Deferred out:** the listening pass (blocked) and the naming application (CSV-4b) |
| **CSV-4b** | Naming-convention application — separate renamer window (D-CSV-19=A), `Samples/` origin classification in `CompositionInventoryWindow` (§17.6), and the `Patterns/Melody` → `Patterns/Melodies` alignment if CSV-5 has not taken it | Editor tooling + content | `SSoT_Editor_Authoring_Tools.md` §17 + `planning/Design_Asset_Naming_v0_1.md` | queued |
| **CSV-5** | Bassline validation (`BASS-VAL-1`) — reproduce and characterise the two reported bass defects; file the package asks | Validation | Boundary §4.3 + a findings report | queued |
| **CSV-6** | Conito + starter restructure (`ROSTER-CONITO-1`) — third musician, ≥5 accepted basslines per Bassline card, starter deck to 4 × 3. **Scope amended 2026-07-20:** also executes **CR-10 / D-CSV-23** — the 8-measure progression standard for new and repaired content, and the Modal-palette merge into Core Major / Core Minor by tonic | Content + contract change | `Design_Starter_Deck_v1.md` §4, `SSoT_Card_Authoring_Contracts.md` | queued |
| **CSV-7** | Song persistence (`SONGSAVE-1`) — save/load a played song from the pre-handoff model + seed | System design + implementation | `SSoT_Runtime_CompositionSession_Integration.md` | queued (design pass first) |
| **CSV-8** | Track evolution on card replay (`CARD-EVOLVE-1`) | System design + implementation, cross-boundary | new `planning/Design_Track_Evolution_v0_1.md` | queued (design pass first) |

> **Blocking note LIFTED (2026-07-22, CSV-3).** The melody path is validated — the finding
> closed as not-a-bug (§4.1.1 item 7), ownership assigned D-MEL-1=A. The listening pass
> (worklist D) is **unblocked** and returns to the CSV-4 continuation.

### 4.1 CSV-1 — Composition Inventory Window

Read-only editor window, direct sibling of `CardInventoryWindow.cs` (toolbar view
selector → list → `Print` to console + `Export JSON` via `SaveFilePanel`). No asset is
mutated by this batch.

Asset families to enumerate: track style bundles, drum patterns, chord progressions
(+ libraries + palettes), melody patterns / phrase archetypes / phrase palettes,
melodic instruments, percussion instruments.

Read paths already exist and must be reused rather than re-implemented:
`PatternRepositoryResources.Get{Drum,ChordProgression,Melody}Patterns(ts)`,
`TrackPatternConfigStoreResources<T>("Drums"/"Chords"/"Melodies")`, and
`InstrumentRepositoryResources` (`IInstrumentRepository.GetMelodicInstruments()` /
`GetPercussionInstruments()`, which already merge the package path and the local
configurable root with de-duplication).

**Closed 2026-07-18.** Delivered as `CompositionInventoryWindow` (seven views, filters, health
flags, Print + Export JSON, Names Report). Documented in `SSoT_Editor_Authoring_Tools.md` §17
(D-CSV-6=A). Real exports produced and read — see §4.1.1. One defect logged and deferred to
**CSV-1b**: palette discovery under-reports, so the `refs`/`ORPHAN` columns are unverified.

Derived "health" columns are the point of the batch — they are what turns a list into
a curation worklist. Candidates: measures/TS vs typical part length (this is the
column that surfaces CR-7's "ends early" symptom statically), orphan status (not
referenced by any palette or style bundle), content-duplicate detection, and for
instruments the soundfont / bank / patch triple plus `octaveMin`/`octaveMax`.

### 4.1.1 Inventory findings, cleanup and re-baseline (2026-07-20, post-CSV-4)

**This subsection replaces the CSV-1c version wholesale (2026-07-20).** The earlier
text stated a 230-asset inventory and an unactioned A–E worklist; both are superseded.
The pre-fix 181-asset export and the 230- and 218-asset exports are **void** — they must
not be cited as current. Only the **183-asset** post-1.1.0 export set is current.

> **Superado 2026-08-08 (MANIFEST-1 / D3=A).** La línea base vigente es el export de
> **232 assets** del 2026-08-08; el conjunto de 183 pasa a histórico. Tabla por familia y
> lectura de salud en `SSoT_Editor_Authoring_Tools.md` §17.12. **D-CSV-14 queda verificada y
> cerrable:** los únicos `OFF-ROOT` locales son los dos patrones de melodía bajo
> `Patterns/Melody` (singular); los otros cinco son package-side bajo `Samples/`.
> El índice inverso carta→bundle (**D-CSV-16**) recibe por fin lote: **CSV-4c**.

**1. Baseline: 230 → 218 → 183.**

Three measured export sets exist. `230` was the CSV-1c corrected export; `218` is
post-worklist-A; `183` is post-MidiGenPlay-1.1.0 and is the only current one.

| Family | 218 | 183 | Delta explanation |
| --- | --- | --- | --- |
| Chord progressions | 42 | 33 | −8 `ChordProgression-Default*`, −1 `Test Progression` (both package-side, MGP-BAGGAGE-1) |
| Drum patterns | 35 | 27 | −8 `DrumPattern-Default*` (package-side) |
| Melody patterns | 14 | 3 | −12 empty package patterns, **+1** new local (`Test_Scale_Melody_4-4_4m_14n`) |
| Chord palettes | 5 | 4 | −`Test Palette` (package-side) |
| Drum palettes | 6 | 5 | −`DrumPatternPalette` (package-side) |
| Style bundles | 24 | 19 | −5 local test bundles (ALWTTT-side, listed below) |
| Instruments (melodic + percussion) | 79 | 79 | unchanged |
| Phrase palettes / archetypes | 3 / 9 | 3 / 9 | unchanged |

**2. What ALWTTT deleted (worklist A, executed 2026-07-18 → 2026-07-20).**

Twelve local orphan assets, exactly as specified by the pre-replacement version of this
subsection — six chord progressions and six drum patterns:

| # | Asset | Path | Why |
| --- | --- | --- | --- |
| 1 | `Prog_Ionian_FourFour_4m_0_4_0_0-…` | `Assets/Resources/ScriptableObjects/Patterns/Chords/` | orphan, `Measures=0`, generated name |
| 2 | `Prog_Ionian_FourFour_4m_0_4_5_1-…` | same | orphan, `Measures=0` |
| 3 | `Prog_Ionian_ThreeFour_4m_0_3_3_0-…` | same | orphan, `Measures=0`, DUP#2 |
| 4 | `Untitled` | same | orphan, `Measures=0` |
| 5 | `Untitled 1` | same | orphan, `Measures=0` |
| 6 | `1 chord 1 measure` | `…/Chord Progressions/Tests/` | orphan, 1 chord |
| 7 | `Drum_4-4_1m_BDSN` | `…/Patterns/Drums/` | orphan test asset |
| 8 | `Drum_4-4_2m_CHBDSNOH` | same | orphan test asset |
| 9 | `Drum_6-1_2m_CHBDSNOH` | same | orphan + `ALL-SILENT` |
| 10 | `LLMTest` | same | orphan, authoring-test residue |
| 11 | `TestSmokeSMR7` | same | orphan, smoke-test residue |
| 12 | `DrumPattern-DefaultFourFour` | `…/ScriptableObjects/Drum Patterns/` | orphan, `ALL-SILENT`, `OFF-ROOT`, shadows a package asset name |

Deleting #1–#5 emptied `Assets/Resources/ScriptableObjects/Patterns/Chords/`; that
local folder was removed with them.

**Verified against the post-deletion export:** counts dropped exactly as predicted, no
LIVE asset lost a reference, and no LIVE asset gained `ORPHAN`.

**Five local style bundles were also deleted** — test content, not starter content:
`Backing Card Config [I – IV – V – I]`, `Backing Card Config [I – vi – IV – V]`,
`2_Composition_001_CompositionPayload 1_Backing_StyleBundle`,
`2_Composition_001_CompositionPayload_Backing_StyleBundle`,
`2CBacking001TestProg_Payload_Backing_StyleBundle`.

**One new local melody pattern was authored:** `Test_Scale_Melody_4-4_4m_14n`, in
`Assets/Resources/ScriptableObjects/Patterns/Melodies/` — the **correct** scan root
(plural). Contrast the two OFF-ROOT local assets under `Patterns/Melody` (singular),
which remain D-CSV-14's residue.

**3. What MidiGenPlay retired (MGP-BAGGAGE-1, package-side).**

Worklist item C is resolved. MidiGenPlay **1.1.0** retired 33 assets and moved 8 to
`Samples/ExampleCatalogue/ChordProgressions/`. Consumer re-export verifies `EMPTY` /
`NO-LANES` / `ALL-SILENT` / `OVERFLOW` at **zero** across all package-origin assets.
Full lifecycle record: `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.2.

**4. Reachability map, corrected.**

`Chord Palette - Test` is now `ORPHAN` — its bundle was deleted — so its 4 progressions
are dead. Live backing content is `Backing Card Config - Core Major` (8 progressions)
and `- Core Minor` (6). `Backing Card Config - Modal` (10) exists but **no card
references it**.

**Reachable set = 14 of 33 progressions.**

> This reachability was established **from the user's statement, not from tooling.**
> The window still has no card → bundle reverse index, so it cannot confirm or refute
> it. That gap is **D-CSV-16**, and it is the reason the decision moved from *nice to
> have* to *blocking*.

**Live drum set is 26, not 27.** `DNB 4-4 2m test` hung off the package
`DrumPatternPalette`, which was orphan and has been deleted; the asset is `ORPHAN` in
the 183 export. `Drum_4-2_2m_ACSN` is live only via the `Rhythm - Card Config SO`
bundle, whose own reachability is unverified (again D-CSV-16).

**5. The `Samples/` discovery nuance — expected, not a regression.**

The package handoff §7.2 predicted the consumer count would drop by the 8 moved assets.
It did not, and that is **correct**: `CompositionInventoryWindow` discovers via
`AssetDatabase.FindAssets<T>()`, which is deliberately broader than `Resources`. The
move achieved its purpose — `Resources.LoadAll` no longer returns them, closing the
selection risk in handoff §1 — but the inventory will list them permanently unless the
window distinguishes the origin. Fix scheduled as a **CSV-4b** rider; surface specified
in `SSoT_Editor_Authoring_Tools.md` §17.6 (third origin `sample`).

**6. Worklist status.**

| Item | Status |
| --- | --- |
| **A** — 12 local orphan deletions | **executed and verified** (2026-07-18 → 2026-07-20) |
| **B** — the 2 local melody patterns | **resolved by D-CSV-15** — both mechanisms retained, both patterns kept |
| **C** — package baggage (28) | **resolved by MGP-BAGGAGE-1** — MidiGenPlay 1.1.0 |
| **D** — listening pass | **UNBLOCKED** (CSV-3 closed the melody finding as not-a-bug; ownership D-MEL-1=A) — returns to the CSV-4 continuation |
| **E** — naming | **drafted** (`planning/Design_Asset_Naming_v0_1.md`); application deferred to **CSV-4b** |

**7. Finding carried out of CSV-4 — CLOSED as not-a-bug (CSV-3, 2026-07-22).**

The 6/8 + Core Minor + Singing Field observation was a **meter collision by
construction** — Core Minor holds **zero** 6/8 progressions. Runs A/B showed **no
divergence**; ST-CSV3-6 confirmed C2a healthy (Core Minor aligns the part to Aeolian
when tonalities are authored). The engine is meter- and tonality-consistent when content
is authored correctly. **Not a bug; no package ask** (`MGP-ALWTTT-MEL-ORDER-1` not filed;
recorded in `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §4.3). Ownership resolved by
**D-MEL-1=A** (rhythm card carries the meter via a `MeterEffect`). Recorded ALWTTT-side in
`SSoT_Runtime_CompositionSession_Integration.md` §12 (rewritten CLOSED) and
`SSoT_Card_Authoring_Contracts.md` §5.16.

**8. Standing CR-7 measurement, re-read.**

The CSV-1c measurement — `BASS-GAP` on 27 of 30 live progressions — remains the origin
evidence for `MGP-ALWTTT-BASSFILL-1`, but the live set has changed and the content
standard has changed with it (CR-10 / D-CSV-23). The ask's priority is recalibrated in
§5; it is **not** withdrawn.

### 4.2 CSV-2 — Dev instrument overrides

The Composition tab (§18.4) overrides *patterns* only. Instruments today are resolved
by `SongConfigBuilder.FromUI` (randomised per render for the no-override path) and
constrained by `InstrumentRules.GetPermittedMelodic` (per-musician SO whitelist, then
`InstrumentType` filter). CR-3 adds a dev picker per track.

Note the asymmetry with §18.4 that makes D-CSV-5 a real decision: **override
instrument GUIDs already participate in `trackInputsHash`**, whereas pattern
overrides deliberately do not. So the pattern-override cache-bypass design is *not*
automatically the right shape here — a hash-participating instrument override keeps
the cache coherent and makes A/B comparison fair.

Second known interaction: `SongCompositionUI.ApplyInstrumentEffect` clears and
re-applies `TrackEntry.override*Instrument` on the matching tracks whenever an
`InstrumentEffect` card is played. A dev override written into `TrackEntry` is
therefore clobbered by a later instrument card on the same track. Not fatal — but it
must be a documented consequence, not a surprise.

UI should offer the full catalogue for counterfactual probing and annotate entries
outside `GetPermittedMelodic` for the musician/role, mirroring the `(off-band)`
convention established in §18.6.

**Closed 2026-07-18 (D-CSV-5=A refined).** Both anticipated interactions above were confirmed
in code and resolved as written: the hash-participation asymmetry made option A correct, and
the `ApplyInstrumentEffect` clobber is now a detected-and-reported *supersession* rather than a
surprise. One interaction the batch open did **not** anticipate surfaced during implementation —
the pattern-override stamp path invalidates with `keepInstruments: true`, which would have let a
stale resolved voice beat the new pick; the fix mirrors the instrument-card invalidation instead.
Surface in `SSoT_Dev_Mode.md` §18.9, cache semantics in
`SSoT_Runtime_CompositionSession_Integration.md` §8 inv 9. ST-CSV-1..8 PASS.

### 4.3 CSV-3 — R2a debug-play

Implements `Design_Composition_Debug_Tab_v0_1.md` §4. Per that doc: almost entirely
ALWTTT-owned, **no new MidiGenPlay API required**, and it is the concrete form of the
reserved `CompositionSession.DevInjectCompositionCard` (M1.5 Phase 5). Applies only
`CompositionCardPayload.modifierEffects`, never `CardPayload.effects`, never cost.
D-CSV-8 (live model vs shadow model) resolves here.

**Scope amended 2026-07-20 (CSV-4 close).** CSV-3 additionally owns **resolved-meter /
resolved-tonality read surfaces** and the **melody-path investigation** (§4.1.1 item 7):
does a part's meter silently default to 4/4, and do `part.Tonality`/`part.RootNote` diverge
from the progression's chord events? R2a is the instrument that makes that investigation
tractable, which is why D-CSV-22=B puts this batch first.

**Closed 2026-07-22.** R2a card debug-play (`DevInjectCompositionCard`, musical side only —
D-CSV-8=A live model, D-CSV-24=B economy-neutral) + the resolved meter/tonality/root read
line shipped `#if ALWTTT_DEV`, production byte-identical. **D-CSV-13=A** (Backing dropdown
stays repository-fed + notice). The melody-path investigation is **CLOSED as not-a-bug**
(§4.1.1 item 7); ownership assigned **D-MEL-1=A**, so the **CSV-4 listening pass is now
UNBLOCKED** (worklist D). Deferred doc-pass applied as **CSV-3-DOC**. Smokes ST-CSV3-1..9 +
5b/5c PASS.

### 4.4 CSV-4 — Curation & naming pass

A design/content session, not a code session. Inputs: the CSV-1 export, the CSV-2
instrument A/B surface, the CSV-3 card audition. Intended outputs: a keep/cut/repair list
per asset family, a naming convention document, and the applied renames. Absorbs CR-4 and
CR-8. Feeds `Roadmap_ALWTTT.md` §Future milestones → Music & Identity ("broader
track/style bundle library").

**Partially closed 2026-07-20.** What actually landed: worklist A executed and verified,
MGP-BAGGAGE-1 filed and resolved package-side (MidiGenPlay 1.1.0 adopted), the inventory
re-baselined to **183**, the naming convention **drafted**
(`planning/Design_Asset_Naming_v0_1.md`), decisions D-CSV-7 / 15 / 16 / 18 / 19 / 21 / 22 /
23 locked, and a green gig smoke pass.

**What did not land, and why.** The keep/cut/repair list per asset family requires the
listening pass, which was **blocked** by the melody finding (§4.1.1 item 7) — a verdict
issued against a mis-rendering engine blames the asset. **That block is now LIFTED
(CSV-3, 2026-07-22):** the finding closed as not-a-bug and the listening pass is unblocked,
so it returns to the CSV-4 continuation. And the **applied renames** were
split out to **CSV-4b** under D-CSV-19=A, because the renamer is a separate window rather
than a mode inside the read-only inventory window. CR-4 and CR-8 therefore remain
**partially absorbed**: their decision layer is settled, their content layer is not.

### 4.5 CSV-5 — Bassline validation

The two reported symptoms already have documented package-side causes in
`SSoT_Composer_Bass_Track.md` §1, recorded there as pre-existing and deliberately
unchanged:

- **"Ends early"** ← *single pass, no repeat-to-fill*: unlike the backing composer, the
  bass renders each progression event once at its absolute step and does not repeat
  the progression to cover the part length. A progression shorter than the part leaves
  the bass silent for the remainder.
- **"Doesn't follow the progression"** ← *normalization-order hazard*: the bass sees
  the TS-normalized runtime clone only if the backing track composed first
  (track-list order); otherwise it consumes the raw cached/authored progression.
  Order-dependent, therefore intermittent.

The ALWTTT side owns **confirmation**, not repair: reproduce both under a pinned seed,
capture the `chd:` dump plus the reported `usesSharedProgression` / `progressionRoman`,
and file. Deliverable is a reproducible findings report and two package asks.

### 4.6 CSV-6 — Conito + starter restructure

Gated on CSV-5. Two halves: the musician (profile, instrument whitelists per role,
identity cards, ≥5 accepted bassline options per Bassline card) and the contract
change (D-CSV-4). The second half rewrites `Design_Starter_Deck_v1.md` §4 and
dissolves the standing Sibi `StarterDeck`-flag debt recorded in `CURRENT_STATE.md` §4.

### 4.7 CSV-7 / CSV-8 — the two design-heavy batches

Both need a design pass before an implementation batch is opened. CSV-8 in particular
should evaluate all four CR-2 evolution variants together rather than shipping the
key-change in isolation, and should check what the existing package transient hints
already afford — `PartConfig.ChordInversionHints` and the directional modulation hint
(`PreviousRootNote` + `ModulationOctaveHint`, `SSoT_Runtime_Song_Model_and_Config.md`
§1.1) are both one-shot per-render inputs that a replay-evolution could drive without
any new package surface.

---

## 5. Cross-project asks (MidiGenPlay)

Filed following the established `SEED-1` / `MOD-DIR-1` / `ARTIC-1` pattern. Registered
ALWTTT-side in `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §4.3 while open; they move to §8
(delivered/adopted log) on delivery.

| Ask | Requirement | Origin | Status |
| --- | --- | --- | --- |
| **MGP-ALWTTT-BASSFILL-1** | Bass composer: repeat-to-fill the progression across the part length, and remove the composer-order dependency on TS normalization | CR-7 | to file at CSV-5. **Priority recalibrated 2026-07-20 (D-CSV-23)** — see the note below |
| **MGP-ALWTTT-WBASS-1** | Bass composer: walking-bass note selection (a conducting line between chord roots) as an additional selection mode | CR-5 | to file at CSV-5 |
| **MGP-ALWTTT-ARTIC-1** | Randomized chord articulation (pre-existing, from DEMO-FIXES / DF-ARTIC) | DF-ARTIC | open, pre-existing |
| **MGP-MIX-1** (package-side **D-BAG-3**) | Per-instrument mix balance: a documented per-track / per-musician gain that **composes multiplicatively** with the package-authored `volume01`, applied via CC7 rather than velocity | CSV-4 instrument review | **open** — ALWTTT input delivered 2026-07-20; registered in Boundary §4.3; not closed by 1.1.0 |
| **CONT-B ask 1 — latin kit mappings** *(label applier-assigned)* | Los 9 kits reportan `percussionMappings: 8`; ninguno mapea `Claves` (75), `OpenHiConga` (63) ni `Cowbell` (56), y `Cowbell` no declara sustitutos de familia (efecto medido: 3 de 4 lanes muteadas en un patrón latin válido; warn+mute es correcto, el hueco es de contenido). Petición: ampliar los mapeos, al menos en Standard y en un kit de percusión, y declarar sustitutos para `Cowbell` | CONT-B (F-KIT-1, T4.5 FAIL) | **to file** — via `CONT-B_Returns_MidiGenPlay_2026-07-31.md` (pending) |
| **CONT-B ask 2 — glyph velocity tiers** *(label applier-assigned)* | El tier de ghost (`o` = 50) queda demasiado por debajo del golpe normal (medido en funk, jazz y country). Petición: revisar el valor, o exponerlo como parámetro por lane o por asset. Mitigación consumer-side: subir `defaultVelocity` de la lane (desplaza los tres tiers a la vez). **Verificar primero si `DPE-IMPORT-1` ya expone velocity por lane** (CONT-B §9c) | CONT-B (F-VEL-1) | **to file** — via the returns doc (pending) |
| **CONT-B ask 3 — card-level gain/velocity on `BasslineCardConfigSO`** *(label applier-assigned)* | El plano de mezcla del consumidor gana por `(musicianId, TrackRole)` y no distingue dos instrumentos del mismo rol; los boosts de Pocket solo afectan a lanes slap/pop, así que un Fingered no puede igualar nivel sin tocar el `volume01` compartido | CONT-B (D9) | **to file** — via the returns doc (pending) |

**BASSFILL-1 priority, recalibrated 2026-07-20 (D-CSV-23).** The ask was filed on the
measurement "27 of 30 live progressions trigger `BASS-GAP`". ALWTTT has since set an
8-measure content standard aligned to the 8-measure part (CR-10), which extinguishes the
flag on most future content **by construction**. BASSFILL-1 therefore drops from
*audible demo defect* to *robustness gap*: the underlying behaviour is still a silent
failure mode — any later-authored 4-measure progression, or any 16-measure part,
reproduces it — but it no longer blocks the demo and **should not outrank MGP-MIX-1**.
**Preferred remedy, if addressed:** a generation-time *warning* when the progression
does not cover the part, rather than automatic fill. A progression that ends and leaves
air can be an intentional musical choice.

**MGP-BAGGAGE-1** was filed and resolved inside CSV-4 and has therefore already moved to
Boundary §8.2 (delivered/adopted log). It is not listed above.

CR-2's musical transforms may generate a further ask at CSV-8; deliberately not
pre-filed, since the ALWTTT-side design has to exist first.

**Doc-correction note to MidiGenPlay (R0 V4, filed ALWTTT-side 2026-07-31 at R0-package apply; queue home per R0 P8):**

> `Documentation~/runtime/SSoT_Composer_Bass_Track.md` — the line *"`degreeAccidental` is ignored (same recorded gap as the backing grid path)"* is accurate about the bass but **mischaracterizes the backing side**. `ChordTrackComposer` honors `degreeAccidental` at both render sites (grid loop root transpose + roman prefix; `RenderFromProgression` with the same handling, guarded on `!= 0` for byte-identity), and `SSoT_Composer_Backing_Track.md` documents this with `ChordMarkerParityTests` coverage. Suggested correction: drop the parenthetical or restate it as *"unlike the backing composer, which applies it at both render sites."* Found during ALWTTT R0 verification V4, 2026-07-23.

---

## 6. Relationship to the committed sequence

This arc does **not** displace the demo cut. `CURRENT_STATE.md` §3 keeps S5i → S5j as
the gate; Phase C (S6–S8) sits behind that.

The one real interaction is **CSV-6**: restructuring the starter deck to 4 × 3 changes
the content S5i is tuning. Two coherent orders exist — run S5i first on the current
starter and accept that CSV-6 partially invalidates the tuning, or land CSV-6 first and
tune the final shape once. Not decided here; flag it when either batch opens.

CSV-1 through CSV-5 are tooling, validation and content work with no gameplay-facing
runtime change, so they can interleave with S5i without contention.

---

## 7. Out of scope for this arc

- MidiGenPlay package internals. Everything package-side leaves as an ask (§5).
- Authoring *new* patterns/progressions via package editors — those tools exist and are
  package-governed (`SSoT_Authoring_Tools.md` §3). This arc curates and inventories;
  it does not re-implement authoring.
- Ziggy / `Captivated` (roster expansion beyond Conito).
- Meta-progression and the ladder.

---

## 8. Governed homes

| Concept | Primary home |
| --- | --- |
| Dev Composition tab surface (§18 + CSV-2/CSV-3 additions) | `systems/SSoT_Dev_Mode.md` |
| Cache keying, hashes, dev override semantics | `runtime/SSoT_Runtime_CompositionSession_Integration.md` §8 |
| Package constraints + cross-project asks | `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md` §4.3 / §8 |
| Editor tooling inventory | `SSoT_Editor_Authoring_Tools.md` (§17 for the Composition Inventory Window; D-CSV-6=A) |
| Starter deck contract | `planning/Design_Starter_Deck_v1.md` §4 |
| Card authoring schema (if CR-2 adds a field) | `SSoT_Card_Authoring_Contracts.md` §9 extension rule |
| Track evolution design | `planning/Design_Track_Evolution_v0_1.md` (to be created at CSV-8) |
| Asset naming convention (Assets-side) | `planning/Design_Asset_Naming_v0_1.md` (created CSV-4, 2026-07-20; planning, non-normative) |

---

## 9. Update triggers

Update this sub-roadmap when: a batch opens or closes; a requirement (CR-*) is
re-scoped, split, or dropped; an open decision (`D-CSV-*`, `D-MEL-*`) resolves; a cross-project
ask is filed, delivered, or declined; or the relationship to the S5i/S5j sequence
changes.

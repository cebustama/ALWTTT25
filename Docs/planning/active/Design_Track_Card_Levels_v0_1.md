# Design — Track Card Levels v0.1

**Status:** Spec CLOSED at R0 (2026-07-23) — planning only, subject to playtest revision (N5). Assigned batch: **R7** (`RosterExpansion_Sub_Roadmap.md`).
**v0.2 (2026-07-23, R0).** §7 open questions resolved (D-R0-7); §6 +INSP overlap resolved (D-R0-8=A, reserve); §3 updated with the R0 verification outcomes (V1 residual, V4 correction). Decision homes: `Design_Starter_Deck_v2.md` §6 (summary) + this doc (full spec).
**Scope:** Re-playing an already-rendered composition card levels its track up instead of being a dead play. Mechanic definition, expressibility limits, runtime/authoring sketch, gameplay hooks, open questions.
**Classification:** `reference (planning)` — **not a SSoT**. At R7 closure the implementing SSoTs take authority (`SSoT_Runtime_CompositionSession_Integration.md`, `SSoT_Card_System.md`, `SSoT_Card_Authoring_Contracts.md`); this note is retained as rationale.
**Created:** 2026-07-23 (Roster Expansion planning session; idea by Matías).
**Placement:** `Docs/planning/active/Design_Track_Card_Levels_v0_1.md`

---

## 1. Problem

Composition cards are content-bearing: once a card's track is rendered in the looping part, holding a duplicate (or redrawing the same card) has no purpose — same-role replay is a same-content replace. This creates dead cards in hand and works against card velocity. The starter deliberately ships duplicates (Wormus ×2, D-STARTER-1=B) — today the second copy is redundant once the first is on a track.

## 2. Mechanic

When a composition card is played and the target `(musicianId, role)` track's `sourceCardDefinition` **is the same card**, the play is a **level-up** instead of a replace: the track advances to the card's next authored level (content variant), up to a cap (working assumption: 3). A *different* card of the same role keeps today's replace semantics; a different role keeps add semantics (BASS-1 unchanged).

Worked exemplar (Wormus Major, authored by Matías 2026-07-23):

Lvl 1 — base:

```
| G     | D     | Em    | C     |
| I     | V     | vi    | IV    |
```

Lvl 2 — voice-leading color:

```
| G     | D/F#  | Em    | C   Cm |
| I     | V6    | vi    | IV  iv |
```

Lvl 3 — final form:

```
| Gmaj7 | B7    | Em7   | Am7  D7 |
| Imaj7 | V7/vi | vi7   | ii7  V7 |
```

## 3. Expressibility limits (verified 2026-07-23)

`RomanProgressionParser` alphabet: Major, Minor, Diminished, Augmented, **Dominant7, Major7, Minor7, HalfDiminished7, Diminished7, Sus2, Sus4**. Consequences for the exemplar:

- **Lvl 3 is expressible** as authored degree/quality events: `Imaj7` = I + Major7 · `V7/vi` = **III + Dominant7** (the secondary-dominant *label* is not a concept; its sound is) · `vi7` = vi + Minor7 · `ii7` = ii + Minor7 · `V7` = V + Dominant7.
- **Lvl 2 partially degrades:** `iv` = IV + Minor ✅; **`D/F#` (slash inversion) is NOT expressible** — the voicer owns inversions (`SSoT_Composer_Backing_Track` §7), so lvl 2 authors `V` and accepts whatever inversion the voicer picks. Level-2 color must come from quality/degree changes (e.g. the added `iv`), not from bass-note dictation.
- **Chromatic roots** (bII etc.) — **recorded gap corrected at R0 (V4).** The *backing* composer **does** honor `degreeAccidental`: root transpose plus roman prefix at both render sites (`ChordTrackComposer` grid loop and `RenderFromProgression`, the latter guarded on `!= 0` for byte-identity, covered by `ChordMarkerParityTests`). The *bass* composer genuinely ignores it (zero consumption in `BassTrackComposer`). Because a 4-musician band normally renders Conito's bass over the shared progression, a chromatic-root level would desync bass against backing. **Constraint stands — level content is diatonic-root only — but the reason is band composition, not the backing path.** Conditional MGP ask (sub-roadmap §8 #4) narrows to bass-side accidental consumption.
- **7th-quality render path — structurally verified at R0 (V1 residual).** `ChordQualityResolver` enumerates the full alphabet as distinct cases; both backing render sites voice per-event quality via `GetChordNoteNames(degreeRoot, e.quality)`, and the melody chord-tone path consumes the same call. Quality therefore reaches voicing per event on every path the lvl3 exemplar uses. **Not yet audited:** the interval table itself (`MusicTheory.GetChordNoteNames`) and an audible spot-check — both are R7 pilot-smoke items, not blockers.
- Extended/added chords (9/11/13, add) are outside the alphabet — do not author them.

Levels are therefore **content selection, not generation changes**: each level is a separate `ChordProgressionData` asset (or per-level palette) authored inside the verified alphabet. Zero MidiGenPlay modification.

## 4. Runtime sketch (ALWTTT)

- **State:** `int level` on `TrackEntry`, sibling of `sourceCardDefinition` (the identity BASS-1 already keys on).
- **Branch:** in `SongCompositionUI.TryAddOrReplaceTrackOnPart` — match on `(musicianId, role)` **and** `sourceCardDefinition == played card` → increment level (clamped) and swap in the level-N bundle content; else existing replace/add semantics.
- **Cache duty (hard requirement):** level must participate in — or force invalidation of — every cached identity: stem-cache keys, `_partBundleCache` replay entries (incl. the D-DBG5 resolved-identity mirror), instrument pins. Note BASS-1 D3=A already disables part-cache pins for multi-track-musician parts; the level branch must not silently resurrect stale renders. Smoke test: level-up → next loop audibly renders level-N, replay paths republish level-N truth.
- **Interactions that come for free (verify, don't rebuild):** level-up is a composition play → consumes the ECON-1 composition budget and **is denied on the final loop** (inv 11) — both correct and desired; deck copies (Wormus ×2) become the natural level-up ammunition (maxim N7 reinforced).

## 5. Authoring sketch

- `TrackAction` gains an optional ordered per-level list (bundle or progression/palette reference per level ≥2); absent list = card has no levels (today's behavior, default).
- Schema change goes through `SSoT_Card_Authoring_Contracts §9` (four-layer rule: spec/DTO + JSON parser + runtime + description builder) and the Card Editor / JSON import path (`styleBundleCreate` precedent from BASS-CARD-1).
- Pilot content (campaign scope): **Wormus Major + Wormus Minor lvl 2–3 only**. Other cards adopt levels in later content passes.

## 6. Gameplay hooks

- **+INSP per level above 1 — RESERVED (D-R0-8=A, 2026-07-23).** R7 ships levels with musical payoff only; no `AddInspirationPerLoopSpec` level-awareness. The +INSP-per-loop lever stays exclusively Vamp / In the Pocket's, so the economy is not double-levered mid-S5i tuning. A level-up is already strictly better than a dead play (richer content on a budget-legal use of a duplicate copy). Revisit only if playtest shows levels are under-rewarded.
- **Complexity:** the LoopScore complexity term is currently **inert** (`LoopTrackSnapshot`/`TotalComplexity` untouched, D-INSP-4, owned by S5i). Track level is its first honest input candidate (level ⇒ complexity ⇒ audience-reaction weight). **Coordinate with S5i** — that batch owns the term; do not activate it unilaterally from R7.

## 7. Resolutions (D-R0-7, locked 2026-07-23)

| Question | Resolution |
|---|---|
| Level lifetime | **Per-part.** A fresh part starts at level 1. Matches cache scoping; per-song/per-gig memory is a later buff knob, not v1. |
| Max level | **3.** |
| Different card replaces a leveled track | **Level state discarded.** State keys on `sourceCardDefinition`; a new identity enters at level 1. |
| Does level-up refresh `PartEffect`s / co-effects | **Yes — a level-up is a normal composition play in every respect except the track branch.** `modifierEffects` and `CardPayload.Effects` execute exactly as on a first play; no suppression code. Moot for the Wormus pilot (neither card carries them), but the rule is stated so R7 does not invent one. |
| UI representation | **REVERTIDA en HUD-COMP-1 (D8=B, 2026-08-26) — ver la nota bajo la tabla.** En reposo, **pips** verdes (`#8FD694`), hasta 3, **sin pips en Lv1**. En hover, texto `Lv n / 3`, con sufijo `· max` en Lv3. El floater de subida es un glifo **`▲` sin texto propio, local a la fila**. *(Resolución original, sustituida: badge de numeral romano II / III más floater "LEVEL UP!".)* |
| +INSP economy | **Reserved** — see §6 (D-R0-8=A). The LoopScore complexity term stays the intended honest hook, filed to its S5i owner; R7 does not activate it unilaterally. |
| Do Action-domain cards ever level | **No.** Confirms the v0.1 non-goal: composition/Track cards only. |
| Scope | **Generic mechanic + Wormus Major/Minor pilot only** (confirmed, unchanged). |

### 7.1 Reversión explícita de la representación UI (HUD-COMP-1, D8=B, 2026-08-26)

**Esto revierte una decisión previa de este mismo documento y se registra como tal, no en
silencio.** La resolución de D-R0-7 (2026-07-23) especificaba un numeral romano; la
implementación de HUD-COMP-1 entrega pips.

**Motivo.** El numeral romano **colisiona con los grados armónicos** (I, V, vi) que el juego ya
usa para acordes: el mismo símbolo significaría dos cosas en la misma pantalla, y una de ellas
en la tira de composición, que es donde el jugador lee armonía. Además la spec de la tira
impone **texto cero en reposo** (D5), y `Lv 1 / 3` se lee como carencia en una pista que
simplemente es normal — de ahí que Lv1 no lleve pips (D2).

**Nota de implementación para R7.** El cap `= 3` se consume desde `RowData.maxLevel`, y la UI
**ya soporta niveles con `level` ausente** (se interpreta Lv1, sin pips). R7 sólo tiene que
rellenar el campo: **el nivel de pista no está implementado**, la UI lo soporta y el modelo no
lo expone todavía. Ver `SSoT_Gig_Combat_Core.md` §15.3.

**Alcance del floater `▲`.** Es **local a la fila** y no pasa por `SensoryEventBus`. Registrado
como excepción, no como precedente tácito: `Design_Sensory_Contract_v0_1.md` §"Excepciones
registradas" (D-DOC-3, abierta para R7).

## 8. Non-goals

No MidiGenPlay changes (levels select authored content). No procedural "complexification" of progressions. No level persistence across gigs (meta-progression owns that space). No retro-leveling of every existing card in-campaign (Wormus pilot only).

## 9. Update rule

R0 spec closure applied 2026-07-23 (v0.2: §7 resolved, §6 resolved, §3 verifications). Next update at R7 open/close. At R7 closure, authority moves to the implementing SSoTs; this note is retained as rationale (same lifecycle as `Design_Starter_Deck_v1.md`).

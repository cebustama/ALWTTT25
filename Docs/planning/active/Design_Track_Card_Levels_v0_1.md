# Design — Track Card Levels v0.1

**Status:** Active design proposal — planning only, subject to R0 refinement and playtest revision. Assigned batch: **R7** (`RosterExpansion_Sub_Roadmap.md`).
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
- **Chromatic roots** (bII etc.): `degreeAccidental` is recorded as ignored on grid consumption paths — scope level content to diatonic roots, or trigger the conditional MGP ask (sub-roadmap §8 #4).
- **Residual verification (R0):** confirm the voicing/render path honors all 7th qualities end-to-end (parser acceptance ≠ audited render); extended/added chords (9/11/13, add) are outside the alphabet — do not author them.

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

- **+INSP per level above 1:** natural extension of the existing track-scoped derivation point (`AddInspirationPerLoopSpec.SumFor` over `TrackEntry.sourceCardDefinition`, DF-INSPLOOP D-INSP-1=D) — make the sum level-aware. **Overlap warning:** Vamp / In the Pocket already occupy the +INSP-per-loop lever; R0 resolves coexist / replace / reserve so the economy lever is not duplicated.
- **Complexity:** the LoopScore complexity term is currently **inert** (`LoopTrackSnapshot`/`TotalComplexity` untouched, D-INSP-4, owned by S5i). Track level is its first honest input candidate (level ⇒ complexity ⇒ audience-reaction weight). **Coordinate with S5i** — that batch owns the term; do not activate it unilaterally from R7.

## 7. Open questions (R0)

Level lifetime: per-part only (a fresh part starts at lvl 1) vs remembered per-song vs per-gig · max level (3 assumed) · what a *different* card replacing a leveled track does to level state (reset assumed) · UI representation (level badge on `SongTrackElementUI` / composer row; floating text on level-up via the existing composition-FX diff path) · does level-up refresh the card's `PartEffect`s or only track content · +INSP economy resolution (§6) · whether Action-domain cards ever level (out of scope v0.1 — composition/Track cards only).

## 8. Non-goals

No MidiGenPlay changes (levels select authored content). No procedural "complexification" of progressions. No level persistence across gigs (meta-progression owns that space). No retro-leveling of every existing card in-campaign (Wormus pilot only).

## 9. Update rule

Update at R0 (spec closure: §7 resolved) and at R7 open/close. At R7 closure, authority moves to the implementing SSoTs; this note is retained as rationale (same lifecycle as `Design_Starter_Deck_v1.md`).

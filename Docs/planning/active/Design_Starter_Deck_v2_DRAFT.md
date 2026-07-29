# Design_Starter_Deck_v2 — ALWTTT (DRAFT, R0)

**Status:** Locked design — R0 output (2026-07-23), D-R0-1..12 **locked by user 2026-07-23** (D-R0-2=B amended: Compound Cycle 6/8 into starter, Waltz Protocol 3/4 to reward). Planning only; numbers subject to playtest revision (N5).
**Scope:** 4-musician starter deck (Sibi, C2, Conito, Zig), finisher layer, per-musician reward slate, per-musician identity map with tempo lean, Track Card Levels resolution summary, singalong design.
**Classification:** `reference (planning)` — **not a SSoT**. Same lifecycle as `Design_Starter_Deck_v1.md`: when R4–R8 authoring closes, the `.asset` files become runtime-authoritative and this doc is retained as rationale.
**Supersession:** supersedes `Design_Starter_Deck_v1.md` **as forward design intent** at R8 closure; v1 remains the authoritative record of the S5-era 17-card demo starter until then (D1=C — the live demo front S5i→S5j runs on v1 untouched).
**Placement:** `Docs/planning/active/Design_Starter_Deck_v2.md`
**Campaign home:** `Docs/planning/active/RosterExpansion_Sub_Roadmap.md` (batches, decisions D1–D6, asks §8).

---

## 1. Design principles carried forward

E1 (*mínimas cartas, máxima expresividad* — one axis of contrast per composition card), E2 (blind-listener test), N7 (exact-copy duplication is a legitimate onboarding/consistency tool), E5 (budget = tempo, cost = spike), N6 (soft synergy default), N12 (every legal band viable). Home: `Design_Game_And_Card_Maxims_v0_1.md`.

New in v2: the **symmetric kit shape** — every musician ships exactly *2 composition + 1 action + 1 finisher* in the starter. The shape itself is an onboarding device: once the player reads one musician's kit, they've read all four.

---

## 2. Identity map — 4 musicians (axes + tempo lean)

Closes the v1 "tempo lean" placeholder (`Design_Tempo_Identity_v1.md` §5). Leans are design intent (naming, flavor, catalog shape), not runtime.

| Musician | Fantasy | Expressive axes owned (matrix #) | Identity status / mechanic | Tempo lean |
|---|---|---|---|---|
| **Robot C2** (drums) | Immortal math-jazz machine; picks the song's clock | Meter (7), tempo scale (6), drum patterns/palettes (15–17, rewards) | **Flow** (momentum) + **Spotlight** (taunt/tank) | **The tempo-shifter.** C2 has no preferred speed — C2 *sets* it (Push It / Half Time are his). Improvisational-control archetype. |
| **Sibi** (keys) | Hive-mind worm from the Singing Fields; shapes what the audience expects and remembers | Progression palette (13), tonality-via-palette (1), modulation (8) | **Earworm** (audience DoT) + taste-reading (Read the Room) | **Slow / hypnotic.** Earworms incubate; her payoff accrues over turns. Slow-favoring. |
| **Conito** (bass) | Flight + electricity; the groove engine running hot | Bass articulation figures (Bassline bundle: Block / ArpeggioUp / Offbeat / PerBeat), instrument patch (9/11 — slap, nylon) | **Voltage** (self-charging counter) → **Overload** (bonus loop + solo) | **Fast / opportunistic.** High card velocity, self-cost plays, d'n'b-funk energy. Fast-favoring. |
| **Zig** (vocals) | The frontman; multiharmony, crowd communion | Melody pattern/strategy/phrase/leading (21–24), harmony (25–26, finisher) | **Captivated** (incoming-Vibe amplifier) + Singalong | **Build-and-drop / anthemic.** Moderate tempo, payoff at the big vocal moments; reward paths bracket him slow (Torch Song) and fast (Motor Mouth). |

Axis-collision note: v1 gave Sibi the melody hook (Singing Field). In v2 the **starter's** sole melody voice is Zig (sung); Singing Field moves to Sibi's reward pool as an *instrumental* keys hook — the two-melody texture becomes an earned reward state, not a day-one default (D-R0-6).

---

## 3. Starter deck v2 — 22 cards / 18 unique (D-R0-6=B)

Domain split: 11 Composition / 11 Action-domain (7 basic actions incl. generics + 4 finishers).

| # | Card | Copies | Domain | Owner | Effect (authoritative for v2) | Cost / gen | Origin |
|---|---|---|---|---|---|---|---|
| 1 | Warm Up | 2 | Action | Any | `DrawCards(2)` | 0 / — | v1, conserved |
| 2 | Take Five | 1 | Action | Any | `ModifyStress(−3, Self)` | 0 / — | v1, conserved |
| 3 | Default Mode | 2 | Comp | C2 | Rhythm. `MeterEffect(4/4)` + `ApplyStatusEffect(flow, +1, Self)` on play | 0 / 3 | v1, conserved |
| 4 | Compound Cycle | 1 | Comp | C2 | Rhythm. `MeterEffect(6/8)` — promoted from reward pool (D-R0-2=B: compound meter is the starter's odd-meter anchor; blind-listener contrast vs 4/4 is maximal — simple duple vs compound triple) | 0 / 3 | v1 pool, promoted |
| 5 | Keep Cool | 1 | Action | C2 | `ApplyStatusEffect(composure, +3, **Musician**)` — retargeted Self→Musician (D-R0-3) | 0 / — | v1, modified |
| 6 | **Spotlight** *(working)* | 1 | **Finisher** | C2 | New taunt status on C2: audience `Musician`/`RandomMusician` targeting redirects to C2 for 1 audience turn (`ResolveTargetsFor` hook, R4) | 2 / — | new, R4 |
| 7 | Wormus Minor | 2 | Comp | Sibi | Backing. Minor-mode `progressionPalette` — **levels 2–3 authored (Levels pilot, R7)** | 0 / 2 | v1, conserved + leveled |
| 8 | Wormus Major | 2 | Comp | Sibi | Backing. Major-mode `progressionPalette` — **levels 2–3 authored (Levels pilot, R7)** | 0 / 2 | v1, conserved + leveled |
| 9 | Mind Tap | 1 | Action | Sibi | `ModifyVibe(+5, AudienceCharacter)` + `ApplyStatusEffect(earworm, +2, AudienceCharacter)` | 0 / — | v1, conserved |
| 10 | **Psychic Wave v2** | 1 | **Finisher** | Sibi | `ModifyVibe(+5, AllAudienceCharacters)` + **`ApplyStatusEffect(earworm, +Y, AllAudienceCharacters)`** (Y first guess: 2; blocked members excluded per `CardBase` target filter). Full-screen mask VFX (R4) | 3 / — | v1, upgraded R4 |
| 11 | **Finger Bass v1** *(working)* | 1 | Comp | Conito | Bassline. `BasslineCardConfigSO` — finger figure set (Block / PerBeat lean), finger-bass patch. v1 approximation; walk = ask §8 #1 | 0 / 2 | new, R2 |
| 12 | **Slap Bass v1** *(working)* | 1 | Comp | Conito | Bassline. `BasslineCardConfigSO` — `Offbeat`/`ArpeggioUp` figure lean, Slap Bass patch. Pocket-coupling = ask §8 #2 | 0 / 2 | new, R2 |
| 13 | **Static Rush** *(working)* | 1 | Action | Conito | `DrawCards(2)` + `ModifyStress(+1, Self)` (D-R0-4) | 0 / — | new, R2 |
| 14 | **Overload** | 1 | **Finisher (Action domain, D-R0-5)** | Conito | Requires **Voltage ≥ 3** on Conito (consumed): one bonus loop this song + one-loop Conito guitar solo track with channel duck, full revert after (R5 guarded API) | 2 / — | new, R5 |
| 15 | **Rise Up** *(working)* | 1 | Comp | Zig | Melody. `MelodyCardConfigSO.patternOverride` = authored ascending-degree `MelodyPatternData` (degree-based ⇒ key/mode-adaptive; tiles to part length per D-MEL5.1). Sung glide via Pink Trombone | 0 / 3 | new, R3 |
| 16 | **Showtime** *(working)* | 1 | Comp | Zig | Melody. `phrasePaletteOverride` — anthemic phrase palette (existing archetypes) | 0 / 3 | new, R3 |
| 17 | **Wink** | 1 | Action | Zig | `ApplyStatusEffect(captivated, +2, AudienceCharacter)` — first Captivated sender (amplification layer in `ApplyIncomingVibe`, R1) | 0 / — | new, R1 |
| 18 | **Double Harmony** (Tier A) | 1 | **Finisher** | Zig | Adds a Harmony-role MIDI track harmonizing the current melody (`HarmonyCardConfigSO`; composer exists package-side; listening validation owed at R6 open). Tier B (2nd sung voice) deferred | 3 / 3 | new, R6 |

**Voltage generation rule (Conito passive, R5):** each successful Conito card play (any domain) grants Conito +1 Voltage (Additive, no decay, cap ~9). Overload is the only consumer in v2; Amp Up (pool) accelerates it.

**Explicit Voltage generation (D-R0-5 rider, locked 2026-07-23):** Voltage is a standard SO-catalogue status (counter primitive, Additive, no decay), so explicit card-driven generation is supported **today** via the existing `ApplyStatusEffectSpec` — Amp Up requires zero new spec code. The only new runtime in R5 is the per-play passive hook and the Overload threshold/consumer; future Voltage-granting cards are pure authoring.

**Moved starter → reward pool (D-R0-6 + D-R0-2=B; conserved per D2=A, placement change only):** Waltz Protocol, Push It, Half Time (C2) · Key Lift, Singing Field (Sibi). **Promoted pool → starter:** Compound Cycle (C2). Net starter size unchanged (1:1 swap).

### Derived counts
- Composition 11: DM×2 + Compound Cycle + Wormus×4 + Finger + Slap + Rise Up + Showtime.
- Actions 7: Warm Up×2, Take Five, Keep Cool, Mind Tap, Static Rush, Wink.
- Finishers 4: Spotlight, Psychic Wave v2, Overload, Double Harmony — one per musician, each a distinct mechanic (redirect / AoE status climax / bonus-loop solo / added voice). Closes D-ECON-6=DEFER's populated-finisher-layer requirement.
- Per musician: C2 5 · Sibi 6 · Conito 4 · Zig 4 · generic 3.

### Uniqueness / comprehension note
18 unique vs v1's 15. Mitigations: symmetric kit shape (§1), multiplicities on the two anchor comps, guaranteed-draw fallbacks, tutorial. R8 re-runs the comprehension lens; sanctioned fallback = add exact copies of the most legible cards (N7), starting with Finger Bass and Rise Up (each musician's "default" comp).

---

## 4. Reward slate (soft paths, D5=A)

Slate slots authored in full; pre-existing pool cards and starter→pool moves are **extras** (D-R0-11).

| Musician | Path A (comp) | Path B (comp) | Status action | Extras already in pool |
|---|---|---|---|---|
| **Sibi** | **Jazz Palette** *(working)* — palette of 7th-quality progressions (V1: full alphabet incl. maj7/m7/dom7/ø7) | **Andaluza** *(working)* — Phrygian palette of authored Andalusian-cadence progressions with explicit qualities (`iv | III | II | I(Major)` family) — **V2 fallback path, no enum change** | **Hive Hum** *(working)* — `ApplyStatusEffect(earworm, +4, AudienceCharacter)`, cost 1: the pure incubator (no instant Vibe — distinct from Mind Tap) | Vamp, Key Lift, Singing Field |
| **C2** | **Jazz Kit** *(working)* — jazz `DrumPatternPaletteSO` (skill `rhythm-pattern-generator`) | **Neuro Kit** *(working)* — d'n'b `DrumPatternPaletteSO` (same skill) | **Lock In** *(working)* — `ApplyStatusEffect(flow, +2, Self)`, cost 1: pure momentum spike | In the Pocket, Waltz Protocol, Pentameter, Push It, Half Time |
| **Conito** | **Bossa Corda** *(working)* — Backing, nylon-guitar patch + arpeggio/offbeat figure + bossa-suited palette (v1 approx; true split = ask §8 #3) | **Tapping v1** *(working)* — Melody, scale-degree arpeggio figures fitting the palette's progressions (chord-aware = ask §8 #5; degrades gracefully) | **Amp Up** *(working)* — `ApplyStatusEffect(voltage, +2, Self)`, cost 0: Overload accelerator | — |
| **Zig** | **Torch Song** *(working)* — ballad: SustainLeadIn-heavy phrase palette + legato leading (slow-lean) | **Motor Mouth** *(working)* — patter: BurstThenHold/dense-EvenFlow palette + repetition directives (fast-lean) | **Singalong** — see §5 | — |
| **Any** | — | — | **Read the Room** *(working)* — `RevealPreferencesSpec(AudienceCharacter)`: reveals TastePreferences on the audience canvas. Sibi-owned, RewardPool, R4 (D-R0-1) | — |

Pool total after campaign: ~20 cards across four musicians — enough for N9 (skip is a real choice) to bite.

---

## 5. Singalong (Zig reward action) — D-R0-9

- **Effect:** `ApplyStatusEffect(captivated, +1, AllAudienceCharacters)`, cost 1. Carries Zig's identity status per the campaign requirement.
- **Sequence:** play → one-shot authored vocal phrase (`SingerVoiceDirector` one-shot API, R6 groundwork) → crowd response: GM Choir-Aahs echo of the phrase + crowd SFX → status lands.
- **Timing:** playable in any action window (no phase-gate code); pre-song is the *ideal* staging but unenforced.
- **Degradation ladder:** (1) phrase + echo + status → (2) echo + SFX + status (no one-shot API) → (3) SFX + status. Mechanics identical at every rung.
- **Ceiling combo (N4):** Singalong → Psychic Wave v2 amplified by AoE Captivated — the campaign's intended "poggable" pairing (the Earworm × Captivated ceiling from maxim N4, now with a concrete two-card recipe).

---

## 6. Track Card Levels — R0 resolution summary (full spec: `Design_Track_Card_Levels_v0_1.md` → v0.2 at doc-update)

Locked at R0 (D-R0-7/8): lifetime **per-part** · max level **3** · replace-by-different-card **discards level state** · level-up is **a normal composition play** (budget, final-loop denial, co-effects/modifierEffects re-execute; no suppression) · UI = roman badge on `SongTrackElementUI` + LEVEL UP! floater · Action cards never level · pilot = **Wormus Major/Minor only** · **+INSP-per-level RESERVED** (no economy hook at R7; complexity hook filed to the S5i owner as the intended scoring input) · level content **diatonic-root only** (V4: bass ignores `degreeAccidental`; backing honors it, so the constraint is band-composition-driven, not backing-driven) · slash inversions not authored (voicer owns inversions — lvl2 color comes from quality/degree changes).

---

## 7. Verification record (R0, 2026-07-23)

| Item | Result | Method |
|---|---|---|
| V1 residual | Structurally verified — per-event quality voiced at both backing render sites + melody chord-tone path (`GetChordNoteNames(degreeRoot, e.quality)`); full alphabet in `ChordQualityResolver`. Interval-table audit (MusicTheory host file) + listening spot-check → **R7 pilot smoke** | Code read: `ChordQualityResolver.cs`, `ChordTrackComposer.cs` L525/L1407, `MelodyTrackComposer.cs` L284 |
| V2 | **FAIL** — Tonality enum = 7 diatonic modes only. Fallback locked: Andalusian progressions by explicit degree+quality over Phrygian (Andaluza card). Ask #4 not triggered by V2 | Code read: `ChordProgressionData.cs` L172–178 (exhaustive switch) |
| V3 residual | **RESOLVED** — authored melody loop tiles by raw beats to part length, truncating final partial repeat; meter mismatch warns (D-MEL5.1=A) | Code read: `MelodyTrackComposer.cs` L575–679 |
| V4 | **Recorded gap partially stale** — backing honors `degreeAccidental` on both paths (+ parity tests); **bass ignores it** (confirmed). Level content stays diatonic; ask #4 narrows to bass-side. Bass SSoT parenthetical stale → cross-boundary doc note filed to MidiGenPlay project | Code read: `ChordTrackComposer.cs` L524/L1405, `BassTrackComposer.cs` (zero hits); `SSoT_Composer_Backing_Track.md`, `SSoT_Composer_Bass_Track.md` L33 |
| V5 | Structurally verified — shared target-list resolution for all specs; `AllAudienceCharacters` branch live (blocked members excluded). Runtime confirmation stays R4 smoke | Code read: `CardBase.cs` L560–600 |

---

## 8. Open at v2 draft (owners assigned)

- Interval-table audit + 7th-quality listening spot-check → **R7** (file `MusicTheory.cs` host as shared project file first).
- `ApplyStatusEffect × AllAudienceCharacters` runtime smoke → **R4**.
- Dual-melody mix validation (Sibi Singing Field + Zig melody as reward state) → **R3** (band-scale singer verification) + **R8** (full-pool smokes).
- Keep Cool retarget tutorial regression → **R4**.
- Draw/hand economy retune for 22 cards / 4 musicians → **R8**.
- Action:composition ratio observation (11:11 vs v1's 6:11) → **R8** comprehension/tuning lens; correction = composition copies (N7).
- Naming pass on all *(working)* names → authoring time per batch.
- Finisher cost band (D-R0-12 first guesses) → R8 full-band tuning.

## 9. Update rule

Update at each campaign batch close that authors v2 content (R1–R8). At R8 closure: authored `.asset` files become runtime-authoritative; this doc retained as rationale; v1 fully historical; update `Roadmap_ALWTTT.md`, `CURRENT_STATE.md`, `changelog-ssot.md`, sub-roadmap per its §12.

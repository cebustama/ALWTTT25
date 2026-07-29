# RosterExpansion_Sub_Roadmap — ALWTTT

**Status:** Planning-only. Decomposes `Roadmap_ALWTTT.md` → Future Milestones → *Roster Expansion* into an executable campaign (R0–R8). Does **not** define implementation truth.
**Pattern:** Same role as `S5_DemoCutClose_Sub_Roadmap.md` / `M1_5_Dev_Mode_Sub_Roadmap.md` — a durable multi-batch plan + decision ledger; per-batch **deep scope + rehydration prompt are generated at batch-open**, not duplicated here.
**Classification:** `roadmap` (planning-only) — not a SSoT.
**Created:** 2026-07-23 (feasibility + planning session). Per D6=A this document is the single consolidation home for that session; the detailed per-card reasoning lives in the session record and in the thematic design notes it points to.
**Placement:** `Docs/planning/active/RosterExpansion_Sub_Roadmap.md`

---

## 0. Position and phasing (D1=C)

The campaign redesigns the starter deck to cover the **4 band musicians** (Sibi, C2, Conito, Zig) and populates the deferred **finisher layer** (D-ECON-6=DEFER, `SSoT_Gig_Combat_Core.md §14.6`). It became executable because both hard prerequisites of the original Roster Expansion entry resolved:

- **Bass pipeline — validated.** BASS-1 + BASS-CARD-1 (2026-07-12): tracks keyed `(musicianId, role)`, `BasslineCardConfigSO`, role-typed `styleBundleCreate` authoring.
- **`ApplyIncomingVibe` — shipped** (2026-05-18, B3) and already the canonical path for all card-sourced positive Vibe (`CardBase.cs`) and Earworm ticks. Only **Captivated** itself remains → batch R1.
- **Singer voice — live** (SINGER-1, 2026-07-21). `SSoT_Singer_Voice.md §8` already names *"Zig's self-harmony finisher"* as the intended first consumer of voice slot 2.

**Phasing rule (D1=C).** The live demo front stays **S5i → S5j** untouched.

- **Interleavable with S5i:** R0 (design, no code) and R1–R3 (enablers). Interleaving-safety argument: new cards live in the **Conito / Cantante catalogs**, which are out of the demo roster and therefore excluded *by construction* from `BuildRewardCardPool` (band-scoped, `PersistentGameplayData`); the Captivated amplification layer is inert without a sender in the demo roster. The S5i tuning baseline (17-card / 2-musician starter) is not perturbed.
- **Post-S5j only:** R4+ — anything touching the starter deck, the finisher layer, the tutorial script (Psychic Waves is the guided finisher, TUT-REBUILD beat 8), or session runtime invariants.

---

## 1. Campaign requirements (spec, 2026-07-23)

Per musician (×4):

- **Starter identity:** 2 distinctive composition cards + 1 distinctive action card + 1 finisher card with a per-musician unique mechanic (finisher = `inspirationCost > 0`, per §14.6).
- **Rewards:** 2 "path" composition cards (two different musical directions per musician; **soft paths per D5=A** — both coexist in the flat per-musician reward pool, no exclusivity mechanic) + 1 action card carrying a status associated with the musician.

Plus one new cross-cutting mechanic in-campaign (**Track Card Levels**, §7 / batch R7) and two registered-only ideas out of campaign (§7).

---

## 2. Decision ledger (campaign-level)

Per-batch decisions get their own `D-RX-*` labels at batch open; these are the campaign-level ones.

- **D1 = C** — Hybrid phasing: R0 + non-demo enablers (R1–R3) may interleave with S5i; starter-v2 / finisher / tutorial-touching batches (R4+) open only after the S5j tag.
- **D2 = A** — Reuse the existing card baseline where it already satisfies the spec (Wormus ×2 pair, Default Mode, Keep Cool, Psychic Waves-extended, Waltz Protocol, reward-pool cards); author only the gaps. No from-scratch re-author.
- **D3 = A** — Conito bass ships **v1 approximations** now (root-note bass + articulation figures + slap/nylon patches); the fidelity items (chord-tone walk, pocket-coupling, bossa split) are **MidiGenPlay asks**, not ALWTTT work (§8).
- **D4 = A** — Double Harmony ships **Tier A** (MIDI Harmony-role track; composer exists package-side) in-campaign; **Tier B** (second sung voice, slot 2) is a follow-up gated on the deferred cap=2 Dev Mode validation and the ~21% two-voice DSP budget.
- **D5 = A** — Reward "paths" are **soft**: both direction cards coexist in the per-musician pool (`RewardPool ∩ UnlockedByDefault`, owned excluded per D9). Exclusive branching is a Progression & Meta mechanic, retrofittable.
- **D6 = A** — Documentation packaging: this sub-roadmap is the single consolidation home (compact verdict table §5); no separate feasibility-eval document. Thematic design notes exist only for content that feeds batches or registers ideas (§7).

---

## 3. Batch sequence

| Batch | Mode | Scope (one line) | Phasing | Depends on |
|---|---|---|---|---|
| **R0** | GAME DESIGN | `Design_Starter_Deck_v2` draft: 4-musician identity map (axes + tempo lean, closing the v1 placeholder), closed card list (~30: effects, costs, targets, levels, starter-vs-pool split under maxims E1/N7), Track Card Levels spec finalization, singalong design, remaining verifications (§9), minor-decision resolution (§10) | Now (no code) | — |
| **R1** | IMPLEMENTATION | Zig enablement: **Captivated** (amplification layer in `ApplyIncomingVibe` + SO + icon) + **Wink** card + Cantante catalog cleanup (7/7 inert → spec) | Interleavable | R0 |
| **R2** | IMPLEMENTATION | Conito enablement: profile/instruments (bass + guitars), **Finger Bass v1** + **Slap Bass v1** (`BasslineCardConfigSO` via `styleBundleCreate`), **Draw** card, Conito catalog cleanup (10/10 inert → spec), first bass-in-gig validation, **file MGP asks** (§8 #1–#3) | Interleavable (∥ R3) | R0 |
| **R3** | IMPL / CONTENT | Zig composition cards: ascending-degree `MelodyPatternData` (verbatim `patternOverride`) + scale-phrase palette; singer verification in a 3–4-musician band (mix, channel, mute) | Interleavable (∥ R2) | R0 |
| **R4** | IMPLEMENTATION | Finishers I: **Psychic Wave v2** (add `ApplyStatusEffect(earworm, Y, AllAudienceCharacters)`; full-screen mask VFX on `TutorialSpotlight.shader` base; **tutorial beat-8 + JUICE-PW regression**) + **C2 Spotlight/Taunt** (counter status + `ResolveTargetsFor` redirect hook, 1 audience turn) | Post-S5j | S5j tag |
| **R5** | IMPLEMENTATION | **Conito Overload** (own batch): counter status (no decay) + ≥3 threshold hook + guarded bonus-loop API (`_loopsRemainingForPart`) + one-loop-scoped solo track (Conito Melody, guitar) + channel duck/restore + revert. Opens with a session-invariant review (§5 note) | Post-S5j | R2, S5j |
| **R6** | IMPLEMENTATION | **Double Harmony Tier A** (Harmony-role card + listening validation + dual per-track particle FX via `IMidiNoteListener`) + **`SingerVoiceDirector` one-shot API** (shared groundwork for singalong; Tier B + expression-input rider queued behind cap=2 validation) | Post-S5j | R3, S5j |
| **R7** | IMPLEMENTATION | **Track Card Levels** mechanic (state on `TrackEntry`, level-up branch in `TryAddOrReplaceTrackOnPart`, cache-invalidation duty, INSP/complexity hooks) + pilot content (Wormus Major/Minor lvl2–3). Spec: `planning/active/Design_Track_Card_Levels_v0_1.md`. May file MGP ask §8 #4 if alphabet gaps bite | Post-S5j | R0 (spec), S5j |
| **R8** | CONTENT / TEST | Rewards for all 4 (palettes via skills: jazz / Phrygian / jazz-vs-EDM drums; bossa v1 + tapping-or-degradation) + **Singalong** (on R6 one-shot API) + starter v2 registration + full-band smokes (4 musicians, full pool) + campaign doc closure | Last | R4–R7 |

Compression note: R3→R1 merge and per-musician reward distribution into R2/R3/R6 can shrink the campaign to ~7 batches at the cost of less-bounded batches. **R5 must stay solo** (invariant-touching).

---

## 4. Requirement × musician coverage map

| | Comp 1 | Comp 2 | Action | Finisher | Reward comp A | Reward comp B | Reward action (status) |
|---|---|---|---|---|---|---|---|
| **Sibi** | Wormus Major ×2 ✅ exists | Wormus Minor ×2 ✅ exists | Reveal card (name TBD — "Mind Tap" occupied) — R1-or-R4 | Psychic Wave v2 — R4 | Jazz chord palette — R8 | Phrygian / Phrygian-dominant / flamenco palette — R8 (V2 gate) | Earworm-carrying action — R8 |
| **C2** | Default Mode (4/4) ✅ exists | 6/8 slot — D-STARTER-2 revisit (R0) | Targetable Composure (Keep Cool variant, `Musician`) — R4-or-R8 | Spotlight/Taunt tank — R4 | Jazz drum palette — R8 | EDM (d'n'b) drum palette — R8 | Composure/Flow-carrying action — R8 |
| **Conito** | Finger Bass v1 — R2 | Slap Bass v1 — R2 | Draw X (identity refinement in R0) — R2 | Overload — R5 | Bossa backing, nylon guitar (v1) — R8 | Tapping melody (chord-aware gate, §9 V3) — R8 | Overload-synergy action — R8 |
| **Zig** | Ascending-degree pattern — R3 | Scale-phrase palette — R3 | Wink (Captivated) — R1 | Double Harmony Tier A — R6 | **UNDEFINED — gap, R0 must propose** | **UNDEFINED — gap, R0 must propose** | **Singalong** — R8 (status TBD in R0) |

---

## 5. Verdict table (feasibility, 2026-07-23 session)

Effort: **A** = authoring/content only · **B** = authoring + bounded ALWTTT code · **C** = own runtime batch · **MGP** = MidiGenPlay feature (out of scope here; ask).

| Item | Verdict | Scope | Effort |
|---|---|---|---|
| Sibi Wormus Major / Minor | Already shipped (v1.1, ×2 each, major/minor `progressionPalette`) | — | 0 |
| Sibi reveal action | Taste data exists (`TastePreferences`, 4 axes: Tempo / density / TS / Tonality). Needs new `CardEffectSpec` (`RevealPreferencesSpec`) via `SSoT_Card_Authoring_Contracts §9` four-layer rule + `AudienceCharacterCanvas` TMP surface. **Name collision:** existing Mind Tap = `ModifyVibe(+5)` + `Earworm(+2)` | ALWTTT | B |
| Sibi Psychic Wave v2 | Psychic Waves exists (cost 3, `ModifyVibe +5, AllAudienceCharacters`). Add `ApplyStatusEffectSpec(earworm, Y, AllAudienceCharacters)` — target verified in spec + `CardBase`. VFX: full-screen circular mask + color inversion; `TutorialSpotlight.shader` reusable base. **Regression duty:** tutorial beat 8 + JUICE-PW presentation (per-target `AudienceVibeImpactEvent`, `CardVibeImpact` sting) | ALWTTT | A payload + B VFX |
| C2 4/4 | Default Mode exists; optional `DrumPatternPaletteSO` upgrade (palette runtime wired 2026-06-04) | ALWTTT | 0–A |
| C2 6/8 | Compound Cycle exists **in reward pool** (D-STARTER-2=B). Starter placement = R0 decision (revert vs distinct card vs keep-as-reward) | ALWTTT | A + decision |
| C2 targetable Composure | Keep Cool exists (`Self`). `targetType = Musician` variant is pure authoring (verified) | ALWTTT | A |
| C2 Spotlight/Taunt finisher | New status + redirect hook in `AudienceCharacterBase.ResolveTargetsFor` (`Musician`/`RandomMusician` → C2, 1 audience turn). No CSO redirect primitive → append (enum append-only) or bespoke key + runtime check. VFX = animation trigger | ALWTTT | B/C |
| Conito Finger Bass | Bassline role + `BasslineCardConfigSO` exist (figures: Block, arpeggio pulse, Offbeat stabs, PerBeat). **1st–5th–8ve–3rd walk NOT implemented** — recorded package-side candidate (seeded-variation batch, CA roadmap) → ask §8 #1 | ALWTTT v1 / MGP walk | A + ask |
| Conito Slap Bass | Timbre = patch (Slap Bass 1/2 in soundfont). Octave/pulse ≈ Offbeat/PerBeat. **Rhythm-track following (pocket) unsupported** (bass renders shared progression, single pass, no cross-track read; `patternOverride` on bass = warn+ignore D-DBG4=A) → ask §8 #2 | ALWTTT v1 / MGP pocket | A + ask |
| Conito Draw X | `DrawCardsSpec` exists (Warm Up). Identity overlap with the generic → refine in R0 (draw + rider) | ALWTTT | A |
| Conito Overload finisher | Counter status (Additive, no decay — supported) + threshold hook (new) + bonus loop (`_loopsRemainingForPart++` trivial per the dev infinite-loop precedent, but needs a guarded API: final-loop lock inv 11, per-loop-insp exclusion — D-CSV-24 economy-neutral pattern as precedent for the solo track, F-3 draws, ECON-1 Seam-C refill on the extra loop, TLM-1 loop counts) + solo track (Conito Melody, guitars via `profile.leadInstruments`) + duck/restore (`SetChannelVolume`; Highlight×mute-family risk) + **one-loop-scoped revert (new pattern** — mutations are persistent in part cache today) | ALWTTT | **C** |
| Zig ascending-note comp | `MelodyCardConfigSO.patternOverride` plays `MelodyPatternData` verbatim; **patterns are degree-based** (ScaleDegree + octave offset, pitch resolved vs Part tonality/root — verified) → key/mode-adaptive by construction. Sung: Pink Trombone glide (`pitchLeadSeconds`/`leadFullInterval`) renders the sweep. Verify pattern-Measures vs part length at authoring | ALWTTT | A |
| Zig scale-phrase comp | `PhrasePaletteSO` + existing archetypes (EvenFlow / BurstThenHold / SustainLeadIn) + `MelodicStyleSO` | ALWTTT | A |
| Zig Wink (Captivated) | Designed (`Design_Audience_Status_v1 §4`, `DamageTakenUpMultiplier`, ×(1+0.25N)). `ApplyIncomingVibe` already canonical → only the amplification layer + SO + icon + card remain | ALWTTT | B (small) |
| Zig Double Harmony | **Tier A:** Harmony role exists end-to-end package-side (`HarmonyTrackComposerFactory`, `NearestDifferentChordTone`, two-pass orchestration reading Melody guide notes D-MEL4.4; readback does not report Harmony ID-2=A → **listening validation owed at batch open**). **Tier B:** second sung voice = slot-2 intended consumer; needs Director role-filter extension (Melody/Lead only today) + cap=2 validation (deferred to Dev Mode) + 2-voice DSP budget | ALWTTT | A/B (Tier A) · B/C (Tier B) |
| Track Card Levels | New mechanic; spec note `Design_Track_Card_Levels_v0_1.md`. Alphabet verified rich enough for the lvl3 exemplar minus slash chords (§9 V1) | ALWTTT | C (R7) |
| Fill Window (C2) | Registered idea, post-campaign — `planning/Design_Fill_Window_v0_1.md` | cross-cutting | C+ |
| Singer expression input | Registered idea, post-campaign (candidate Tier-B rider) — `planning/Design_Singer_Expression_Input_v0_1.md` | ALWTTT | B |
| Singalong (Zig reward action) | Mini scripted event: short authored phrase → `SingerVoiceDirector` **one-shot API** (new consumer-side entry; today the Director arms only from `LoopPlaybackStarting`) → crowd response as GM Choir-Aahs echo + crowd SFX (cheap path; avoids voice-2 budget and the open singer mixer-bypass follow-up). Pre-song PlayerTurn window. Gameplay effect + carried status TBD in R0 | ALWTTT | B/C (R8, on R6 API) |

---

## 6. Reward slate — collisions and gaps (input to R0)

- **Sibi:** jazz chord palette · Phrygian/Phrygian-dominant/flamenco palette (Phrygian confirmed in the tonality enum via Wormus Minor; **Phrygian dominant unverified** → V2; fallback = authored Andalusian-cadence progressions by degree/quality) · Earworm action. Existing in pool: **Vamp** (+INS) — R0 decides whether it counts toward the slate.
- **C2:** proposal resolving the 3/4–5/4 collision (3/4 = starter Waltz Protocol; 5/4 = reward Pentameter already): the two path slots = **jazz vs EDM (d'n'b) `DrumPatternPaletteSO` cards** (skill `rhythm-pattern-generator` covers both genres); Pentameter retained as an existing extra. Existing in pool: **Compound Cycle, Pentameter, In the Pocket**.
- **Conito:** bossa backing on nylon guitar (v1 approx: arpeggio/offbeat figure + nylon patch + suitable palette; true bossa split = ask §8 #3) · tapping melody arpeggiating the *current* chord — **chord-aware resolution does not exist** (degree patterns resolve vs tonality/root, not vs the progression event; §9 V3) → v1 = scale-degree arpeggio figures that fit the palette's progressions, or ask §8 #5 · Overload-synergy action.
- **Zig:** **two composition-reward directions UNDEFINED** (gap — R0 must propose) · Singalong as the reward-action slot (must carry a status per the requirement — candidate: Earworm-to-all or Captivated; R0 decides).
- **Cross-cutting:** +INSP-per-level (Levels mechanic) overlaps the existing +INSP lever (Vamp / In the Pocket, `AddInspirationPerLoopSpec`). R0 resolves coexist / replace / reserve.

---

## 7. New mechanics and registered ideas

- **Track Card Levels** — in campaign (R7). Spec + expressibility analysis: `planning/active/Design_Track_Card_Levels_v0_1.md`. Solves the dead-composition-card problem (re-playing an already-rendered card levels the track instead of doing nothing meaningful).
- **Fill Window** — registered, **not scheduled**: `planning/Design_Fill_Window_v0_1.md`. End-of-loop timed window for fill cards; conflicts with the "mutations never touch the playing loop" invariant → overlay-vs-next-loop analysis in the note. Candidate C2 "path" post-campaign; the windowed-timing primitive is reusable.
- **Singer Expression Input** — registered, **not scheduled**: `planning/Design_Singer_Expression_Input_v0_1.md`. Player input drives live voice levers; the SSoT's "concrete consumer" condition is met by design here. Natural rider of Double Harmony Tier B.

---

## 8. MidiGenPlay asks — pending, not filed

Filing rule: asks are filed **with acceptance criteria** at the batch that owns the demand (R2 for #1–#3, R7/R8 for #4–#5), never as intentions. They join the existing pending item `MGP-ALWTTT-ARTIC-1` (DF-ARTIC) in the cross-boundary queue. None are redesigned here (boundary rule).

1. **Bass chord-tone walk figures** (1st–5th–8ve–3rd cycling) — formal demand on the package-side recorded candidate (seeded-variation batch, CA roadmap). *(R2)*
2. **Bass pocket-coupling** (bassline reads/follows the Rhythm track) — new cross-track feature. *(R2)*
3. **Bossa bass/upper split** — formal demand on the package-side deferred CA-T2 item (register-selective emission). *(R2; consumed by R8 content)*
4. **Conditional — chord alphabet / chromatic degrees:** only if R0 verifications V1-residual/V2/V4 fail for the target level content (e.g. Phrygian dominant tonality; `degreeAccidental` on grid paths). *(R7)*
5. **Conditional — chord-aware melody resolution** (pattern degrees resolved against the sounding chord, or an arpeggio melody strategy) for the tapping reward. *(R8)*

---

## 9. Verifications owed to R0

- **V1 — Chord quality alphabet: RESOLVED with caveats (2026-07-23).** `RomanProgressionParser` supports Major, Minor, Diminished, Augmented, **Dominant7, Major7, Minor7, HalfDiminished7, Diminished7, Sus2, Sus4**. The lvl3 exemplar (`Imaj7 | V7/vi | vi7 | ii7 V7`) is expressible (V7/vi = degree III + Dominant7 quality; iv = degree IV + Minor). **Not expressible:** slash-chord inversions (voicer owns inversions) — lvl2 `D/F#` degrades to `V`. Residual: confirm the voicing/rendering path honors all 7th qualities end-to-end (parser acceptance ≠ audited render).
- **V2 — Phrygian dominant** in the tonality enum (Phrygian itself confirmed). Fallback: Andalusian-cadence authored progressions.
- **V3 — Melody patterns: HALF-RESOLVED.** Degree-based + octave offset, runtime-resolved vs Part tonality/root (verified) → ascending card is fully adaptive. **Chord-aware resolution does not exist** → tapping reward gates on ask #5 or degrades to scale-degree figures. Residual: pattern `Measures` vs part length behavior.
- **V4 — `degreeAccidental`** ignored on grid consumption paths (recorded gap, backing + bass SSoTs) — impact only on truly chromatic-root level content; scope level authoring to diatonic roots or trigger ask #4.
- **V5 — `ApplyStatusEffectSpec` + `AllAudienceCharacters`** at runtime (target verified in code; low risk; covered by R4 smoke).

---

## 10. Open items at R0 (beyond §9)

Sibi reveal-card naming (Mind Tap occupied) · C2 6/8 starter placement (D-STARTER-2 revisit) · Conito Draw identity vs Warm Up · Overload card domain (Action vs Composition; final-loop lock interaction — Action recommended) · starter-vs-pool split of ~30 cards + starter v2 size/ratio (maxims E1/N7; S5i comprehension lens) · Levels scope (generic mechanic + Wormus-only pilot) + level-state lifetime (per-part? per-song?) + max level + UI badge · +INSP-per-level vs Vamp/In the Pocket · singalong effect/status/timing · Zig's two composition-reward directions · whether existing pool cards (Vamp, In the Pocket, Compound Cycle, Pentameter) count toward the reward slate.

---

## 11. R0 rehydration prompt

```
Mode: GAME DESIGN. R0 — Starter Deck v2 (4 músicos) + mecánicas nuevas: consolidación de diseño.

Context: Campaña Roster Expansion planificada (este sub-roadmap, 2026-07-23; D1=C:
R1–R3 intercalables con S5i, R4+ post-S5j). Evaluación de viabilidad cerrada (§5):
todo ALWTTT salvo fidelidad de bajo (walk, pocket, bossa split) y chord-aware melody
(asks §8). Alfabeto de acordes verificado rico (V1: 7as/sus disponibles; slash NO).
MelodyPatternData por grados (adaptativo a tonalidad; NO chord-aware).

Decisions locked: D1=C · D2=A (reusar baseline) · D3=A (bajo v1 + asks) · D4=A
(Harmony Tier A) · D5=A (paths blandos) · D6=A (consolidación en sub-roadmap).

Open at batch open: lista completa en §10 + verificaciones residuales §9 (V1
residual, V2, V3 residual, V4, V5).

Inputs: 1) RosterExpansion_Sub_Roadmap.md (este doc) · 2) Design_Track_Card_Levels_
v0_1.md · 3) Design_Starter_Deck_v1.md · 4) MidiGenPlay_Expressive_Surface_for_
ALWTTT_Cards.md · 5) Design_Audience_Status_v1.md · 6) SSoT_Gig_Combat_Core.md §14 ·
7) SSoT_Singer_Voice.md · 8) SSoT_Composer_Bass_Track.md + SSoT_Composer_Backing_
Track.md (§ alfabeto/calidades) · 9) Design_Game_And_Card_Maxims_v0_1.md.

Task: 1) Mapa de identidad 4 músicos (ejes matriz expresiva + tempo lean, cerrando
el placeholder v1). 2) Lista cerrada de cartas (~30): efecto autoritativo, coste/gen,
target, nivel(es), starter-vs-pool. 3) Spec final de niveles (estado, authoring,
ganchos INSP/complejidad, invalidación de caché) dentro del alfabeto verificado.
4) Diseño singalong (secuencia, efecto/status, degradación). 5) Resolver §10 y
residuales §9. 6) Tamaño/ratio starter v2 (maxims E1/N7). 7) Borrador
Design_Starter_Deck_v2.

Constraints: planning-only, cero código; internals de MidiGenPlay no se rediseñan
(asks con criterios de aceptación, se archivan en R2/R7/R8); baseline S5i intocable;
cartas existentes que cumplen se conservan (D2=A); Fill Window y Expression Input
NO entran en la campaña (registradas).

Deliverables: Design_Starter_Deck_v2 (draft) · spec de niveles cerrada (update de
Design_Track_Card_Levels o anexo v2) · verificaciones resueltas con método · diffs
propuestos de sub-roadmap/Roadmap/changelog · agenda R1–R8 confirmada.

Closure exit criteria: v2 draft completo; abiertos resueltos o diferidos con dueño;
doc-update propuesto, no aplicado (fase separada).
```

---

## 12. Update rule

Update this document at every campaign batch open/close (status column, decisions promoted from `D-RX-*` ledgers, asks filed, verdicts corrected by implementation evidence). When R8 closes: this doc's batch table becomes historical record; `Design_Starter_Deck_v2` authored assets become runtime-authoritative (same lifecycle as v1); update `Roadmap_ALWTTT.md`, `CURRENT_STATE.md`, `changelog-ssot.md`, and retire the campaign from "Next active".

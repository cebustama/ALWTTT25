# TS Normalization (Normalized Bar Time) — Roadmap (v8)

**Updated:** 2026-03-07

This document tracks execution of **Time Signature Adaptation via Normalized Bar Time (Static Helper)**.

---

## Policy (authoritative)

- **Part.TimeSignature is the authority.**
- **MeterEffect** (if present) prevails.
- Else use **MidiGenPlayConfig.defaultTimeSignature**.
- **No track/pattern changes Part TS “by surprise”.**
- **No asset mutation:** any adaptation must happen on **runtime clones** only.

---

## Contract (Normalized Bar Time)

Represent timings as **fraction of bar** when converting between time signatures:

- `bars = steps / StepsPerMeasure(srcTS, srcSub)`
- `dstSteps = Quantize(bars * StepsPerMeasure(dstTS, dstSub))`

This preserves “half a bar stays half a bar” across meters (4/4 → 6/8, 5/4, etc.).

---

## Fix log (critical bug found during validation)

### Symptom (observed in 6/8)
- Rhythm loop length was correct (`lenTicks=2304`), but the Backing/Chords track ended early (audible “half-loop” / misalignment).
- In diagnostics, Backing reported a smaller `lastTick` and/or large tail silence inside the pattern grid.

### Root cause
`ChordTrackComposer` had two render paths:
1) **Main authored path in `Compose(...)`** (when `prog.events` exists)
2) `RenderFromProgression(...)` (used by procedural composition)

The TS reprojection logic (`NormalizeProgressionForPartIfNeeded`) was applied in **RenderFromProgression**, but **not** in the main `Compose(...)` path.
As a result, when a 4/4 chord progression asset was used under a 6/8 Part:
- the progression stayed `progTS=FourFour sub=1`
- the grid used `partTS=SixEight`
- chord start/length steps were interpreted on the wrong grid → chords “finished early” vs the loop.

### What was changed to fix it
**ChordTrackComposer**
- Added (or finalized) `NormalizeProgressionForPartIfNeeded(part, prog)`:
  - Reprojects chord **start steps** using **normalized bar time**
  - Upsamples harmonic resolution to `minHarmonicSubdivisions` (default 4)
  - Rebuilds lengths from consecutive starts to avoid overlaps after quantization
  - Returns a **runtime clone** (never mutates assets)
- **Called normalization inside `Compose(...)`** immediately after resolving `prog` and before computing the time grid and rendering MIDI.
- Upgraded the `GenContext` progression cache to the normalized clone when the cache contained the unnormalized asset (so other tracks stay coherent).
- Added minimal diagnostics:
  - `PRE-NORM / POST-NORM` (shows TS/subdivisions change)
  - `GRID ... tailSteps` (detects end-of-pattern silence)
- Confirmed beat-unit-aware time conversion is used for MIDI time (`beatSpan = GetBeatSpan(part.TimeSignature)`).

**ChordProgressionData**
- `FindChordEventAt(...)` uses beat-unit ticks via `GetBeatSpan(ts)` so cross-track chord lookup stays aligned in 6/8 / 5/4.

### Pass criteria (after fix)
- `POST-NORM progTS=SixEight sub=4 changed=True` when using a 4/4 progression under 6/8.
- `tailSteps=0`
- Backing and Rhythm both merge to the same part length (`lastTick == lenTicks`).

---

## Implemented deliverables (DONE)

### 1) Static helper (MidiGenPlay.MusicTheory)
- `GetBeatSpan(TimeSignature ts)` ✅
- `StepsPerMeasure(TimeSignature ts, int subdivisions)` ✅
- `StepsToBars(...)` ✅
- `BarsToSteps(...)` + `QuantizeMode` ✅
- `BarsToLengthSteps(...)` ✅

### 2) Config knobs (MidiGenPlayConfig)
- `minHarmonicSubdivisions` (default 4) ✅
- optional placeholders: `enableAccentSnap`, `accentSnapToleranceSteps` ✅ (not used in MVP)

### 3) ChordTrackComposer
- Beat-unit aware timing (no Quarter assumption) ✅
- Reproject `ChordProgressionData` from authoring TS → Part TS by normalized bars ✅
- Harmonic upsample to `minHarmonicSubdivisions` ✅
- No asset mutation (runtime clone) ✅
- Procedural progressions stamp `prog.TimeSignature = part.TimeSignature` ✅
- **Bugfix:** normalization applied in `Compose(...)` path + cache upgrade + tailSteps logs ✅

### 4) ChordProgressionData
- `FindChordEventAt(...)` uses beat-unit ticks via `GetBeatSpan(ts)` ✅

---

## DoD / validation checklist

1) **4/4 baseline**
   - Part TS=4/4, measures=4, authored prog 4/4
   - Expected: identical feel vs pre-change (no reprojection needed).

2) **6/8 with MeterEffect**
   - Part TS=6/8, measures=8 (or 4), authored prog 4/4
   - Expected:
     - reprojection occurs (FourFour → SixEight, sub 1 → ≥4)
     - `tailSteps=0`
     - merged `lastTick == lenTicks`.

3) **5/4**
   - Part TS=5/4, measures=4, authored prog 4/4
   - Expected: no crashes, no negative lengths, musical result acceptable.

4) **Already-authored TS**
   - Part TS=SixEight, progression TS=SixEight
   - Expected: `changed=False` and no reprojection side-effects.

5) **Cross-track lookup**
   - Any system using `FindChordEventAt` in 6/8/5/4 stays aligned.

---

## Minimal logs expected (when reprojection happens)

- `[ChordTrackComposer] PRE-NORM progTS=... sub=... partTS=...`
- `[ChordTrackComposer] NormalizedBarTime reprojection: ... TS a → b | sub x → y | measures=n`
- `[ChordTrackComposer] POST-NORM progTS=... sub=... changed=True`
- `[ChordTrackComposer] GRID ... tailSteps=0`


---

## New milestones (added in v4)

These milestones extend the current “always correct timing” foundation with:
1) **Better musical choices first** (pick a progression authored for the Part TS when available),
2) **Optional feel control** (meter grouping / accent snap), and
3) **Authoring + repository coverage** for 3/4, 5/4, and more.

---

## Phase 6 — Two-step progression resolution (TS-first, ranked fallback, then adapt)

**Goal:** When selecting a chord progression, prefer one authored for the **Part.TimeSignature**.
If none exists, pick the **most convenient** alternative (ranked heuristic), then rely on **Normalized Bar Time** adaptation.

### Strategy (two-step + ranked fallback)
1) **Exact TS match (Tier A)**  
   Choose progression where `prog.TimeSignature == part.TimeSignature`.
   - **Library/template path:** Tier A remains a hard gate if exact TS templates exist.
   - **Palette override path:** Tier A can now be disabled per `ChordProgressionPaletteSO` via `preferExactTsMatches=false` for testing/forced fallback scenarios.

2) **Ranked “nearest TS” fallback (Tier B)**  
   If Tier A is empty — or intentionally skipped by palette policy — score candidates and pick the best one, then run `NormalizeProgressionForPartIfNeeded(...)`.

3) **Last-resort fallback (Tier C)**  
   If the library/palette is tiny or candidate scoring collapses, pick *any* suitable progression and adapt.

### Ranked fallback heuristic (Tier B) — scoring rules (incremental, ship in slices)

We score each candidate progression with a simple additive score (higher is better).
This is designed to be easy to implement and to improve musical plausibility quickly.

#### B1) Bar-duration equivalence (strong preference)
Prefer candidates with the **same bar duration** as the target part:

- `barLenQ = beatsPerMeasure * (4.0 / beatUnit)`  (bar length in quarter-notes)
- Prefer minimal `abs(barLenQ_src - barLenQ_dst)`.

Examples:
- **3/4 ↔ 6/8**: both have `barLenQ = 3.0` → very good fallback.
- **5/4 ↔ 10/8**: both have `barLenQ = 5.0` → good fallback *if you ever author 10/8*.

#### B2) Beat-unit (denominator) compatibility
If bar-length ties, prefer the same **beat unit** (denominator):
- Prefer `beatUnit_src == beatUnit_dst` (e.g., 4/4→5/4 better than 6/8→5/4 if both exist).

#### B3) Odd-with-odd / even-with-even (numerator parity)
If still tied, prefer matching numerator parity:
- odd target (5/4, 7/4) prefers odd candidates; even target prefers even candidates.

#### B4) Numerator “closeness”
Small penalty for large numerator jumps:
- prefer smaller `abs(n_src - n_dst)` (e.g., 4/4 is usually closer to 5/4 than 12/8, unless B1 says otherwise).

#### B5) Harmonic resolution (subdivisions)
Prefer progressions that need **less upsampling**:
- prefer higher `prog.subdivisions` (or at least `>= minHarmonicSubdivisions`).

#### B6) (Optional) Group-count compatibility (when grouping exists)
If the part has a grouping preset (Phase 7), prefer candidates whose **typical chord segmentation** matches the number of groups.

Example intuition:
- Target **5/4 with grouping 3+2** has **2 groups** → a 4/4 progression that often changes chord **2 times per bar** tends to adapt more plausibly than one with 4 chord changes per bar.

> This is optional because it requires a cheap “segmentation estimate” (e.g., count chord-starts per bar in the candidate).

### Where to implement (incremental)
- **Library path**: `ChordTrackComposer.PickTemplateForPart(...)`
  - Add Tier A filter (exact TS).
  - If empty, apply Tier B scoring (B1–B5 first; B6 later).
- **Card override path**: `BackingCardConfigSO.PickProgressionOverride(...)`
  - Add overload `PickProgressionOverride(rng, desiredTS)` or wrapper called by composer.
  - Apply same Tier A / Tier B logic within the override pool.
  - Read `ChordProgressionPaletteSO.preferExactTsMatches` to optionally skip Tier A in the palette path.

### Diagnostics (minimal)
Add one log line:
- `[ChordTrackComposer] PROG_PICK tier=A|B|C progTS=... partTS=... score=...`
- Palette path should also report whether `preferExactTsMatches` was enabled or whether Tier A was intentionally skipped.

### DoD
- With at least one 5/4 progression in library/palette:
  - a 5/4 Part picks it (tier=A) and does **not** need TS reprojection.
- With no 5/4 progressions available:
  - a 5/4 Part still works via tier=B or C + reprojection; **no tail silence**, **no early end**.
- Verify 3/4 Parts:
  - If 3/4 progression exists, choose tier=A.
  - Else, prefer 6/8 (tier=B via bar-duration equivalence).

**Complexity:** Low–Medium (B1–B5 are straightforward; B6 requires a tiny bit more analysis of candidate events).

---

## Phase 7 (Optional) — Meter grouping presets (accent options) + snap

**Goal:** Provide *feel* options inside a fixed TS (e.g., 5/4 as 3+2 or 2+3),
and let chord starts optionally snap to those accent boundaries (within a tolerance).

### Grouping model
- `int[] grouping` that sums to the TS numerator (`tsNum`).
- Accent boundaries are the cumulative starts: `0, g1, g1+g2, ...`

### Preset examples (initial set)
- 3/4: `[3]` (or `[1,1,1]` if you want explicit beats)
- 5/4: `[3,2]`, `[2,3]`
- 7/4: `[3,4]`, `[4,3]`, `[2,2,3]`, `[2,3,2]`
- 12/8 (flamenco-style feel examples):
  - `[3,3,2,2,2]` (boundaries 0,3,6,8,10,12)
  - `[2,2,2,3,3]` (alternative feel)

### Files (new + touched)
- New: `MeterGroupingEffect.cs : PartEffect` (or extend `MeterEffect` with optional grouping)
- `SongCompositionUI.cs` (PartEntry stores grouping + preset id)
- `SongConfig.cs` (PartConfig stores grouping)
- `SongConfigBuilder.cs` (copy grouping)
- `ChordTrackComposer.cs` (optional “accent snap” pass on chord starts AFTER reprojection)

### Snap behavior (MVP)
- Only affects **startStep** (never changes pattern length).
- If `enableAccentSnap`:
  - if chord start is within `accentSnapToleranceSteps` of a boundary, snap to boundary
- Then rebuild lengths from consecutive starts (same as current reprojection) to avoid overlaps.

### DoD
- In 5/4:
  - grouping `[3,2]` produces preferred boundaries (0,3,5 beats) and chord starts snap there (when within tolerance).
- Disabled state: no behavioral change.

**Complexity:** Medium (needs data plumbing UI→SongConfig plus snap pass).

---

## Phase 8 — Authoring + repository coverage for 3/4 and 5/4

**Goal:** Make sure we can *actually* store and discover progressions authored in 3/4 and 5/4,
so Phase 6 has real material to pick.

### Requirements
- `ChordProgressionData.TimeSignature` is reliably set during authoring (editor tool + imports).
- Repository / library indexing can filter by TS (or at least exposes TS for scoring).

### Deliverables
- Add a small seed set of progressions:
  - at least 3–5 progressions in 3/4
  - at least 3–5 progressions in 5/4 (include both 3+2 and 2+3 feels if possible)

### DoD
- You can create and save a progression asset with `TimeSignature=ThreeFour` or `FiveFour`
  and it appears in the repository/library and can be chosen by Phase 6.

**Complexity:** Low–Medium (mostly tooling + content, minimal runtime code).

---

## Suggested execution order (from “always correct” → “more musical”)
1) Phase 6 (Two-step TS-first selection)
2) Phase 8 (Authoring coverage for 3/4 + 5/4) — can be done in parallel with 6
3) Phase 7 (Grouping presets + accent snap) — optional polish pass


### Tier B scoring heuristic (v1)

When no exact TS templates exist, candidates are ranked using a multiplicative heuristic (before runtime reprojection):

**Signals (rough priority):**
- **Bar-duration equivalence** (strong): prefer templates whose bar duration matches the part (e.g., **3/4 ↔ 6/8**; both are 3 quarter-notes per bar).
- **Same beat-unit / denominator** (medium): prefer same denominator when bar duration ties are not available.
- **Odd-with-odd / even-with-even** (mild): prefer same numerator parity.
- **Numerator closeness** (mild): prefer smaller |num(part) − num(template)|.
- **Higher subdivisions** (mild): prefer templates with higher `prog.subdivisions` (less quantization artifacts after reprojection).
- **Chord-start density vs grouping count** (mild, useful for 5/4): prefer templates whose **chord starts per bar** match the part’s **default grouping count**.
  - Example: **5/4** default grouping **3+2** has 2 groups → prefer templates with **2 chord starts per bar** (common in 4/4 “two chords per bar” patterns).

**Default grouping presets (used for scoring; Phase 7 will use them for accent snap):**
- 4/4 → [2,2]
- 3/4 → [3]
- 6/8 → [3,3]
- 5/4 → [3,2] (alt: [2,3])
- 7/8 → [2,2,3]
- 12/8 → [3,3,2,2,2] (flamenco-ish)

**Notes**
- This heuristic affects only **which template we pick first**. We still adapt to Part TS using **Normalized Bar Time** and we never mutate assets.


---

## Progress update (v7) — 2026-03-06

### Phase 6 — Two-step progression resolution (TS-first, ranked fallback) — IMPLEMENTED

**Library path (`ChordTrackComposer.PickTemplateForPart`)**
- Tier A **hard gate**: if any exact TS templates exist (`prog.TimeSignature == part.TimeSignature`), selection is restricted to those.
- Tier B ranked fallback: if no exact TS exists, candidates are scored using the Tier B heuristic (bar-duration equivalence, beat-unit match, parity, numerator closeness, subdivisions, chord-density vs grouping count).
- Diagnostics: template selection logs should report tier A vs tier B (verbose).

**Card override path (`BackingCardConfigSO.PickProgressionOverride`)**
- Added TS-aware overload:
  - `PickProgressionOverride(rng, desiredTimeSignature, settings, verbose)`
- Tier A exact TS within the override pool/palette candidates.
- Tier B ranked fallback when exact TS is absent.
- Tier C fallback to the palette’s native weighted pick if palette candidates cannot be introspected (reflection best-effort).
- Added palette-level policy toggle:
  - `ChordProgressionPaletteSO.preferExactTsMatches`
  - `true` = preserve normal Tier A exact-match behavior
  - `false` = intentionally skip Tier A and force Tier B/Tier C behavior for validation and forced-adaptation scenarios
- Important: fixed C# overload collisions (tuple-name + generic-constraint signature issue) by:
  - using a single Roulette helper (clamping weights), and
  - renaming reflection helpers to avoid CS0111.

**Wiring (must be true in current code)**
- `ChordTrackComposer.Compose` must call the TS-aware overload (not the legacy `PickProgressionOverride(rng)`):
  - `prog = backingStyle.PickProgressionOverride(rng, part.TimeSignature, _settings, verbose: _settings?.logGenerator == true);`

### TS normalization call-site — VERIFIED REQUIRED

`NormalizeProgressionForPartIfNeeded(part, prog)` must be invoked before grid/time conversion:
- in `Compose(...)` after `prog` is resolved (override/cache/pattern) and before rendering, and
- optionally (defensive) at the start of `RenderFromProgression(...)`.

This ensures the “if not compatible, adapt existing” objective is true for both authored and procedural paths.

---

## What remains (next objectives)

### Phase 6 Validation (must-do before moving on)
Before continuing roadmap work, validate that **all DONE items are truly complete**:

1) **Exact TS picking**
   - Provide at least one 5/4 (or 3/4) progression in the library/palette.
   - With `preferExactTsMatches=True`, confirm Tier A is selected.
   - Validation target: `tsChanged=False`; `subChanged` may still be `True` if upsampling to `minHarmonicSubdivisions` is required.

2) **Fallback heuristic**
   - With no 3/4 progressions available but 6/8 present, confirm 3/4 parts prefer 6/8 (Tier B) before reprojection.
   - Also validate the forced-fallback case by setting `preferExactTsMatches=False` on a palette that *does* contain an exact TS candidate; Tier A should be skipped intentionally.

3) **Adaptation correctness**
   - 4/4 → 6/8 and 4/4 → 5/4 must show:
     - reprojection occurs (TS changes + sub upsample)
     - `tailSteps=0`
     - merged `lastTick == lenTicks`

4) **Determinism**
   - Same seed / same ctx.rng → same selection.

### Phase 7 (Optional) — Meter grouping presets + accent snap
Not implemented yet (still optional):
- grouping presets stored in SongConfig / UI
- post-reprojection chord-start snapping to accent boundaries with tolerance

### Phase 8 — Authoring + repository coverage for 3/4 and 5/4
Not implemented yet:
- ensure authoring tools reliably set `ChordProgressionData.TimeSignature`
- create seed library content (3–5 progressions for 3/4 and 5/4)

### Technical debt / hardening (recommended)
- Replace reflection-based palette candidate extraction with an explicit palette API (if possible).
- Add minimal automated tests (golden logs / deterministic picks / no-tail invariants).

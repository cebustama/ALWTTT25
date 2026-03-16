# ALWTTT × MidiGenPlay — Milestone Roadmap
## Backing vs Rhythm Track Generation + Rhythm Authoring MVP

**Generated:** 2026-03-07  
**Scope:** Bring Rhythm up to Backing’s end-to-end quality: *Option A wiring*, deterministic generation, meter-correct timing, and a production-usable Rhythm authoring tool based on the same dual-mode pattern already validated in `ChordProgressionEditorWindow`.

---
## Progress (living report)

- ✅ **Phase 0 — Baseline audit + instrumentation** — **COMPLETED**
  - Added a consolidated Phase0 snapshot log in `RhythmTrackComposer.Compose()` (guarded by `settings.logGenerator`).
  - Verified runtime resolution paths via real card plays: **Procedural(no pattern)**, **Pattern(Grid)**, and **No StyleBundle + TrackParameters.RhythmRecipe**.
  - Outcome: logs now make it obvious which authoring inputs were used and why; no heavy allocations when logging is off.

### Phase 0 process (very short)
- Implemented `LogPhase0Snapshot(...)` + minimal `Compose()` wiring (one snapshot per path).
- Ran targeted playtests to cover the key cases and validated snapshots against inspector values.

- ✅ **Phase 1 — Determinism fix (seeded generation must be stable)** — **COMPLETED**
  - `RhythmTrackComposer`: replaced Unity RNG with `ctx.rng` for style selection (seeded by `SongOrchestrator`).
  - Added `max <= min` safety wrapper so behavior matches Unity int `Range` edge-case.

- ✅ **Phase 2 — Meter correctness fix (beat unit aware)** — **COMPLETED**
  - `RhythmTrackComposer`: beat-unit-aware timing in procedural + grid + legacy paths.
  - `SongOrchestrator`: beat-unit-aware `partTicks` (fixes 6/8 loop-length mismatch / “half-loop silence”).
  - Verified in-game with `MeterEffect` → **6/8** + `styleIdOverride="shuffle_6_8_default"`: loop length now matches the authored rhythmic content.

### Phase 1–2 process (very short)
- Phase 1: made style selection deterministic by switching to `ctx.rng`, then re-ran the same seed to confirm stable `chosenStyleId`.
- Phase 2: fixed beat-unit timing in the composer, discovered loop length mismatch in 6/8, then patched `SongOrchestrator` tick math to match beat unit semantics.



- ✅ **Phase 3 — Grid signature safety (Normalized Bar Time for DrumPatternData)** — **COMPLETED**
  - `RhythmTrackComposer`: added **Phase 3 normalization** for GRID-authored `DrumPatternData` when its `beatsPerMeasure` doesn’t match the **Part** meter.
  - Contract aligned with the Chord TS Normalization milestone:
    - **Part.TimeSignature is authoritative**
    - **No asset mutation** (runtime clone)
    - Remap hits by **normalized bar time** (preserve relative position inside each measure)

### Phase 3 process (very short)
- Reused the same mental model as chord progression TS adaptation: “events keep their *relative position in the bar*”.
- Implemented `NormalizeGridPatternForPartIfNeeded(...)` to clone + remap lane step hits onto the Part’s grid before `ComposeFromGrid`.
- Verified by running a 6/8 Part with non-6/8-authored grid patterns and confirming alignment (no silent drift / no footgun).

### Files modified so far
- `RhythmTrackComposer.cs` — Phase0 snapshot logging, Phase1 RNG determinism, Phase2 beat-unit timing, Phase3 grid-to-Part meter normalization (runtime clone).
- `SongOrchestrator.cs` — Phase2 beat-unit-aware part length (and metronome beat span if enabled).

---

## Milestone outcome (acceptance criteria)

By the end of this milestone:

1. A Rhythm Composition Card that references a `RhythmCardConfigSO` **drives runtime output**:
   - `patternOverride` → authored drum pattern → MIDI
   - else `recipeOverride` / `styleIdOverride` → procedural styles → MIDI
   - **phrasing + feel floats are honored** where applicable

2. Output is **deterministic** under `settings.defaultSeed` (same seed ⇒ same MIDI).

3. Timing is **meter-correct**, including meters where beat unit ≠ 4 (e.g. 6/8).

4. A usable **Rhythm authoring tool** exists for `DrumPatternData`, with **mandatory dual authoring modes**:
   - **Grid mode** (visual lane editor)
   - **Text mode** (row-based DSL)

5. Both authoring modes converge to the **same normalized persisted model**.

6. LLM support is documented and planned as an **assistive authoring layer**, but it is **not required** to close the base Rhythm editor milestone.

---

## Phase 0 — Baseline audit + instrumentation ✅ COMPLETED

**Goal:** Make it impossible to “think” Rhythm assets are used when they aren’t.

**Steps**
- Add a consolidated debug log block in `RhythmTrackComposer.Compose()` (guarded by `settings.logGenerator`) that prints:
  - Source resolution:
    - Style bundle type name
    - `patternOverride != null`
    - `recipeOverride != null`
    - `styleIdOverride` value
  - Meter resolution:
    - beats/measure + beat unit used for timing
  - Phrasing/feel values:
    - `fillEveryNMeasures`, `lastMeasuresAsFill`, `kickDensity`, `snareGhostNoteChance`, `hatSubdivisionBias`

**Requirements**
- Logging must not allocate heavily when off.

**DoD**
- Logs clearly show which path was taken and why for every Rhythm track.
- Evidence: Phase0 snapshots observed for Procedural(no pattern), Pattern(Grid), and no-StyleBundle/TrackParameters.RhythmRecipe cases.

---

## Phase 1 — Determinism fix (seeded generation must be stable) ✅ COMPLETED

**Goal:** Rhythm style selection must use `ctx.rng`, not `UnityEngine.Random`.

**Files**
- `RhythmTrackComposer.cs`

**What we changed**
- Procedural/no-pattern path now calls `RhythmStyleRegistry.Choose(...)` using a callback backed by `ctx.rng.Next(min, max)`.
- Added a small `max <= min` safety wrapper to preserve Unity’s `Random.Range(int,int)` edge behavior.

**Why this matters**
- `SongOrchestrator` seeds `ctx.rng` deterministically per part, so using it makes style selection reproducible per `defaultSeed`.

**DoD**
- Same `defaultSeed` ⇒ same `chosenStyleId` and stable MIDI output across repeated generations.

---

## Phase 2 — Meter correctness fix (beat unit aware) ✅ COMPLETED

**Goal:** Fix correctness bug: Rhythm (and loop-length math) must respect the Part time signature’s **beat unit** (e.g. 6/8 uses Eighth, not Quarter).

**Files**
- `RhythmTrackComposer.cs`
- `SongOrchestrator.cs`

**What we changed**
1. **RhythmTrackComposer**
   - Added beat-unit-aware timing resolution.
   - Replaced Quarter-based timing assumptions in procedural, grid, and legacy rendering paths.

2. **SongOrchestrator**
   - Fixed `partTicks` calculation to compute ticks per beat from the Part’s actual beat unit span.
   - Kept loop-length calculation aligned with authored meter semantics.

**Bug resolved**
- In 6/8, the rhythm MIDI track length was effectively shorter than the orchestrated loop because the orchestrator assumed Quarter beats. After the fix, rhythm coverage and loop length align.

**DoD**
- 6/8 grooves land correctly.
- Rhythm covers the full loop in 6/8.
- 4/4 behavior remains unchanged.

---

## Phase 3 — Grid signature safety (eliminate beats-per-measure mismatch footgun)

**Goal:** Prevent silent misalignment between:
- `part.TimeSignature` beats/measure and
- `DrumPatternData.beatsPerMeasure` + lane step array sizes.

**Files**
- `DrumPatternData.cs`
- `RhythmTrackComposer.cs`
- later: `DrumPatternEditorWindow.cs`

**Steps**
1. In `ComposeFromGrid`, detect mismatch between:
   - expected steps/measure from Part meter
   - actual steps/measure from `DrumPatternData`
2. MVP rule: **canonical is the Part meter at runtime**.
   - If mismatch: log warning and rebuild/pad/truncate lane arrays deterministically to expected size.
3. Extend `DrumPatternData.EnsureSizes()` (or equivalent helper) to support “ensure sizes for expected signature”.

**DoD**
- No out-of-range access.
- Mismatches are obvious and self-correcting or policy-enforced.

---

## Phase 4 — Option A completion: honor phrasing + feel floats

Right now, `patternOverride` / `recipeOverride` / `styleIdOverride` are honored, but these authoring knobs are not fully driving output yet:
- `fillEveryNMeasures`
- `lastMeasuresAsFill`
- `kickDensity`
- `snareGhostNoteChance`
- `hatSubdivisionBias`

### Phase 4A — Implement phrasing (fills)

**Goal:** Fills happen where authoring says they should.

**MVP approach**
- Apply fills when **no `patternOverride`** (procedural/styled rhythms), via deterministic post-process on the produced `MidiFile` using `ctx.rng`.
- Stretch later: optionally apply fills on top of authored patterns.

**Implementation idea**
- Compute ticks-per-measure using tempo map + beat span + beats/measure.
- Determine fill measures by:
  - `(m+1) % fillEveryNMeasures == 0` if configured
  - final K measures where `m >= measures - lastMeasuresAsFill`
- Add a simple fill overlay near the end of selected measures.
- Use DryWetMIDI note-management helpers rather than manual delta editing.

**DoD**
- Changing fill knobs clearly changes output.
- Output remains deterministic for a given seed.

### Phase 4B — Implement feel floats (kick/snare/hat)

**Goal:** Feel floats have audible effect and remain deterministic.

**MVP mapping**
- `kickDensity`: probability of extra or offbeat kicks
- `snareGhostNoteChance`: low-velocity ghost notes around backbeats
- `hatSubdivisionBias`: bias toward quarter/eighth/sixteenth hat activity

**DoD**
- Same seed + different feel values ⇒ predictably different note density/timing.
- Logs state selected feel behavior when generator logging is enabled.

---

## Phase 5 — Rhythm Authoring Tool v2 (MANDATORY): Dual Text/Grid Authoring

**Goal:** Build the Rhythm authoring tool using the same successful dual-mode pattern already proven in `ChordProgressionEditorWindow`, but adapted to **row-based rhythm authoring**.

**Why**
The original “simple lane grid editor” MVP is functional but suboptimal for fast authoring and experimentation. Rhythm authoring benefits strongly from:
- fast textual sketching
- row-by-row lane input
- rapid iteration before or alongside grid refinement

So for Rhythm, the tool must expose **two authoring modalities** that converge to the same persisted pattern data:
1. **Grid mode** — direct lane/step editing
2. **Text mode (row-based DSL)** — fast string authoring per lane/row

This is now considered the **mandatory interaction model** for `DrumPatternEditorWindow`.

**Reference**
- Use `ChordProgressionEditorWindow` as the reference implementation for:
  - dual-mode UI
  - target asset binding
  - preview/apply/save split
  - normalized-data-first architecture

**Files**
- New: `DrumPatternEditorWindow.cs`
- Update: `DrumPatternData.cs`
- Optional helpers:
  - `RhythmRowDslParser.cs`
  - `RhythmPatternPreviewBuilder.cs`
  - `RhythmPatternValidation.cs`

### Phase 5A — Data model contract and normalization

**Goal:** Define a single normalized representation that both authoring modes write into.

**Working rule**
- Persisted asset remains `DrumPatternData`.
- Editor may maintain temporary authoring state, but Apply/Save must always normalize into the same canonical lane-step representation.

**Normalized representation**
- Global timing:
  - `Measures`
  - `TimeSignature` or canonical beats-per-bar source of truth
  - `subdivisions`
- Lane content:
  - lane instrument
  - default velocity
  - step activation data
  - optional future metadata (accent / per-step velocity / flam / roll)

**Invariant**
- Text mode and Grid mode must produce the **same persisted structure**.
- Preview must operate on normalized data, not directly on raw text input.

**DoD**
- A documented normalize pipeline exists:
  - `text/grid input -> normalize -> preview -> apply/save`

### Phase 5B — Grid mode (row lanes)

**Goal:** Deliver the row-based lane editor as the visual authoring mode.

**Minimum feature set**
1. Create/load `DrumPatternData`
2. Signature controls:
   - `TimeSignature`
   - `Measures`
   - `Subdivisions`
3. Lane list:
   - add/remove lane
   - GM percussion instrument selection
   - default velocity
4. Grid:
   - X axis = steps
   - Y axis = lanes
   - click to toggle step on/off
   - clear lane / clear all
5. Apply/save to asset

**Validation**
- Prevent timing mismatch drift.
- Ensure lane step arrays always match current normalized grid size.
- Rebuild/pad/truncate safely when timing changes.

**DoD**
- A designer can author a drum beat entirely in Grid mode and play it in-game through `RhythmCardConfigSO.patternOverride`.

### Phase 5C — Text mode (MANDATORY row DSL)

**Goal:** Add a string-based row DSL so rhythms can be authored quickly without manually painting every step.

**Core idea**
Each row/lane can be authored from compact text input and then normalized into lane steps.

**Examples of supported authoring concepts**
- one input string per row/lane
- lane identity + rhythmic pattern
- explicit rests/silences
- optional accents/velocity markers later

**Initial scope**
- Text mode must target the same **global timing grid** as Grid mode.
- It must be possible to:
  - parse row strings
  - normalize to lane steps
  - preview result
  - apply result to current asset
  - round-trip back into grid editing

**Important constraint**
For this phase, the canonical timing model remains **global** to the pattern. The DSL is mandatory, but **true per-row independent meter/polymeter is not required yet**.

**Why this constraint exists**
The currently documented rhythm model is still global in timing shape, so the first implementation should not silently pretend that row-local meters already exist in runtime or persistence.

**DoD**
- A user can author a beat using row strings only.
- Parse -> normalize -> preview -> apply works reliably.
- Switching between Text and Grid does not lose authored information.

### Phase 5D — Advanced row cycles / polymeter policy decision

**Goal:** Explicitly decide whether Rhythm authoring will support real lane-local cycle lengths / polymeter.

**Problem statement**
A use case like:
- Row 1 hi-hat in 4/4
- Row 2 snare in 3/4

is not just another input syntax; it implies either:
- true row-local cycle metadata in the data model, or
- an editor-only shorthand that eventually bakes into a larger common grid.

**Decision options**
1. **Bake-to-global-grid policy**
   - row-local authoring is allowed as input shorthand
   - parser expands everything into one shared global step grid
   - runtime stays unchanged

2. **True polymeter policy**
   - extend `DrumPatternData` to allow per-lane cycle metadata
   - update preview, validation, persistence, and runtime interpretation accordingly

**Recommendation**
- Keep the base Rhythm editor milestone on **bake-to-global-grid**.
- Treat true polymeter as a later explicit extension.

**DoD**
- Roadmap and SSoT explicitly state which policy is in force.
- No ambiguity remains about whether per-row meter is “really supported” or baked during authoring.

---

## Phase 6 — LLM-Assisted Rhythm Authoring (POR IMPLEMENTAR)

**Status:** planned / not required to close the base Rhythm editor milestone.

**Goal:** Integrate a lightweight LLM authoring assistant into `DrumPatternEditorWindow`, reusing the existing editor-side LLM integration patterns already used elsewhere in the project.

**Intent**
The assistant should help transform natural-language prompts into either:
1. row-based rhythm DSL strings, or
2. a structured rhythm DTO / JSON payload

which is then parsed, validated, previewed, and only applied on explicit user confirmation.

**Files**
- Update: `DrumPatternEditorWindow.cs`
- Reuse editor LLM core package integration patterns
- Optional DTOs:
  - `RhythmPatternGenerationRequest`
  - `RhythmPatternGenerationResult`
  - `RhythmPatternDto`

### Phase 6A — UI integration

**Goal:** Add an LLM panel to the editor window.

**Minimum UI**
- prompt text area
- selected agent / config
- generate button
- raw response display
- “Use response as DSL” / “Use response as JSON”
- preview-before-apply flow

**DoD**
- Editor can send a prompt and receive a response without writing directly into the asset.

### Phase 6B — Output contracts

**Goal:** Define robust machine-readable outputs for the assistant.

**Preferred order**
1. JSON DTO contract (preferred for robustness)
2. DSL string contract (acceptable for lightweight generation / iteration)

**Validation rules**
- response must parse successfully
- timing must match current editor constraints or be normalized explicitly
- lane references must map to valid instruments/rows
- invalid output must fail safely and visibly

**DoD**
- There is a documented response contract.
- Invalid model outputs do not corrupt assets.

### Phase 6C — Prompting and agent instructions

**Goal:** Define the assistant’s exact responsibility.

**Agent responsibility**
- transform musical intent into valid Rhythm authoring structures
- respect current editor timing constraints unless explicitly asked to propose alternatives
- produce predictable, minimal, parseable output

**Non-goal**
- no direct auto-save into assets
- no hidden write-back
- no bypass of validation

**DoD**
- A reusable prompt template / agent instruction set exists.
- Generated results are stable enough for iterative authoring assistance.

---

## Phase 7 — Integration checks (end-to-end validation)

**Goal:** Prove the full authoring chain works in ALWTTT.

**Manual test checklist**
- Card with `RhythmCardConfigSO.patternOverride` plays exactly the authored pattern.
- Card with only `recipeOverride` / `styleIdOverride` generates groove + fills + feel.
- Grid-authored pattern plays correctly in-game.
- Text-authored pattern plays correctly in-game.
- Text -> Grid roundtrip preserves structure.
- Grid -> Text roundtrip preserves meaning.
- Determinism: same `defaultSeed` ⇒ identical MIDI across runs.
- Validation catches timing/grid mismatches clearly.

**Stretch checks**
- If LLM panel is enabled:
  - prompt -> DTO/DSL -> preview -> apply works
  - invalid outputs fail safely

**Automated sanity test (recommended)**
- EditMode test:
  - build minimal `SongConfig` with one Rhythm track
  - generate MIDI twice with same seed
  - assert hash equality (SHA256) or exact note list equality

**DoD**
- A regression-catcher exists for Rhythm generation determinism and timing.
- Rhythm authoring is validated as a **two-mode tool**, not just as a raw grid editor.

---

## Phase 8 — Documentation deliverables (SSoT)

**Goal:** Lock contracts to prevent drift.

**Docs**
- `SSoT_CompositionAuthoringTools.md`
  - document Rhythm dual-mode authoring as the new standard
  - document advanced row cycles / polymeter policy decision
  - document LLM support as POR IMPLEMENTAR
- `SSoT_CompositionCardTypes.md`
  - keep runtime field semantics aligned with Option A
- Optional:
  - dedicated Rhythm Authoring SSoT if row-DSL and/or polymeter grows enough
  - 1-page Rhythm pipeline summary mirroring the Backing summary style

**DoD**
- Docs reflect the exact editor architecture:
  - dual mode mandatory
  - one normalized persisted model
  - assistive LLM generation is parse/validate/preview/apply, never hidden write-back

---

## Optional files for better UX integration
Only needed if you want deeper tooling integration:
- `CardEditorWindow.cs` (add “Create/Open Drum Pattern for this RhythmCardConfigSO” in card authoring)
- shared beat-unit / timing helpers if they should be centralized across composers and editors
- optional parser helpers or DTO contracts if LLM-assisted generation becomes standardized across pattern editors

# SSoT_ALWTTT_MidiGenPlay_Boundary — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Explicit ownership split and integration boundary between ALWTTT game/runtime truth and MidiGenPlay package truth  
**Owns:** what ALWTTT may define, what it must reference, and how older mixed docs are classified  
**Does not own:** full ALWTTT subsystem semantics or MidiGenPlay package internals themselves

---

## 1. Purpose

This document exists to stop ALWTTT and MidiGenPlay from becoming **two competing primary homes for the same concept**.

It is the governing answer to:
- what belongs to ALWTTT
- what belongs to MidiGenPlay
- what remains a thin integration contract only

---

## 2. Ownership split

### 2.1 ALWTTT owns
- gig runtime orchestration
- card gameplay semantics
- composition card gameplay meaning
- active song/session runtime lifecycle
- audience/scoring/hype/vibe/stress behavior as game systems
- ALWTTT-side composition model state
- runtime rebuild/invalidation decisions observable from the game
- `MidiMusicManager` as a game runtime integration component

### 2.2 MidiGenPlay owns
- package-internal composer algorithms
- bundle-field precedence inside package generation pipelines
- repository loading/selection internals owned by the package
- generic package-side music authoring tools
- package-side TS normalization/adaptation algorithms
- generic pattern/instrument generation internals that are not game-owned runtime behavior

### 2.2.1 Source-of-truth split across the handoff
This is the most important anti-confusion rule in the integration boundary.

- **Before handoff/build**: ALWTTT owns the active editable/session truth via `SongCompositionUI` + `CompositionSession`.
- **After handoff/build**: MidiGenPlay owns the package-side runtime truth via `SongConfig` + `SongConfigManager`.

This is not duplicate authority. It is a deliberate split between:
- game-owned editing/session truth
- package-owned runtime song/generation truth

Neither side should silently redefine the other.

### 2.3 Shared integration surface
The shared surface is thin and contractual.

It includes concepts such as:
- ALWTTT building playback input from its composition model
- ALWTTT passing bundle/style/instrument choices into the music package pipeline
- ALWTTT receiving playback/generation results as part of its live gig runtime

Rule:
- ALWTTT may document the **observable contract and handoff**
- ALWTTT must not silently redefine package internals

---

## 3. Boundary rules for composition cards

### 3.1 ALWTTT-owned side
ALWTTT owns:
- what a Composition card means to gameplay
- when it can be played
- what part of the composition model it changes
- whether it also applies normal gameplay effects
- whether the card should trigger an audible rebuild path in the game runtime

### 3.2 MidiGenPlay-owned side
MidiGenPlay owns:
- how a referenced style bundle or authored musical asset is interpreted deep inside package composition/generation pipelines
- how package-side composers select, adapt, and render internal musical content after the ALWTTT handoff

---

## 4. Boundary rules for runtime playback

### 4.1 ALWTTT-owned side
ALWTTT owns:
- when a song/session exists
- when playback should start/stop/rebuild
- which game runtime component hosts playback
- how loop/part/song feedback feeds back into the game

### 4.2 MidiGenPlay-owned side
MidiGenPlay owns the package-level machinery that turns build input into generated musical output.

### 4.3 Known package constraints ALWTTT works around

Recorded here so the workaround is not mistaken for ALWTTT design, and so the request to the package is written down once.

**Per-musician stem/instrument readback (BASS-1) — RESOLVED (DBG-C1, 2026-07-17).** *Historical:* `PartRender.stemsByMusician` / `melInstByMusician` and the `RenderSinglePart` `instrumentOverrides` parameter were keyed by `musicianId`, which could not represent a musician holding more than one role-track in a part (the package returned one stem/instrument per musician, last role wins). ALWTTT degraded safely by omitting multi-track musicians from the cache and forcing a fresh render.

**Resolution.** MidiGenPlay re-keyed those surfaces by `(musicianId, TrackRole)` (`MusicianTrackKey`) in **MGP-ALWTTT-DBG-1** and added `PartRender.resolvedByTrack` + a trailing `patternOverrides` map on the same key (package contract; MGP readback SSoT). ALWTTT adopted the composite key end-to-end in **DBG-C1**: the stem/bundle/part caches, the instrument-override argument, and `ComputeTrackInputsHashesForPart` are all now `MusicianTrackKey`-keyed, and the three BASS-1 degradations are retired. Multi-track musicians are cacheable again; single-track parts stay byte-identical (BC gate verified). Detail: `runtime/SSoT_Runtime_CompositionSession_Integration.md` §8 invariant 9. DBG-C1 also consumes `resolvedByTrack` (read-only truth surface) and carries `patternOverrides` as an inert passthrough (interactive use is DBG-C2). The `chd:` per-chord marker is likewise consumed as a governed contract (MGP `SSoT_Composer_Backing_Track §2.1`) via `MidiMusicManager.GetChordTimelineSnapshot()`.

**Per-instrument mix balance (D-BAG-3 / MGP-MIX-1) — DELIVERED (MidiGenPlay 1.2.0, 2026-07-20); adoption pending. Surface, verification results, and locked decisions: §8.3.** `volume01` is `1.0` on all 70 melodic instruments. This is **unauthored, not deliberately flat**: nothing in the package or the game ever set it to anything else. It is a package-side authoring field and stays there. ALWTTT therefore has **no consumer-side per-instrument gain**, and under D-CSV-7=A (location-based asset ownership) must not edit package assets to obtain one.

A package-side seam already exists (`MidiGenerator.ApplyChannelVolume` CC7 + `IMixController`). What is missing is a *documented* per-track / per-musician gain that **composes** with `volume01` rather than replacing it.

**ALWTTT input delivered 2026-07-20:**
- **Granularity — per musician.** The model has been keyed `(musicianId, TrackRole)` end-to-end since BASS-1, and "the bass is too loud" is a sentence about a character, not about a patch.
- **Composition law — multiplicative** (`volume01 × gain`), with gain defaulting to **1.0**, so package-side loudness normalisation composes instead of being discarded.
- **Consumer-side consequence on the application point.** Velocity scaling changes timbre as well as level with soundfonts, which would invalidate the per-instrument listening verdicts D-CSV-18 requires; CC7 leaves timbre intact.

Not closed by 1.1.0; **closed by MidiGenPlay 1.2.0** (§8.3). The mix-balance batch is **no longer blocked by the missing seam** — it is gated only on the consumer-side adoption work in §8.3 (one of which, the live-plane collision, became a BAL-1 prerequisite). Origin: the CSV-4 instrument-family review; the asset-side half is closed by D-CSV-18=A (curation is pool-level, `SSoT_Editor_Authoring_Tools.md` §17.11).

**Not a constraint:** channel allocation. The package's `BuildChannelMap` already allocates per **track index**, not per musician, so two tracks belonging to one musician receive distinct channels natively. ALWTTT's `(musicianId, role)` channel stamping mirrors that.

**Melody-path meter/tonality investigation — CLOSED, NO PACKAGE ASK (CSV-3, 2026-07-22).** The 6/8 + Core Minor + Singing Field divergence (part-field resolution, `runtime/SSoT_Runtime_CompositionSession_Integration.md` §12) was a **meter collision by construction**, not a composer defect — Core Minor holds zero 6/8 progressions, runs A/B showed no divergence, and ST-CSV3-6 confirmed the melody aligns to Aeolian when tonalities are authored. `MGP-ALWTTT-MEL-ORDER-1` is **not** filed; ownership resolved ALWTTT-side by **D-MEL-1=A** (rhythm card carries the meter). Recorded so the non-ask is traceable and not re-derived.

---

## 5. Classification of older mixed docs

### 5.1 `SSoT_Runtime_CompositionSession_Bridge.md`
Classification:
- mostly **ALWTTT runtime integration truth**

New governed home:
- `runtime/SSoT_Runtime_CompositionSession_Integration.md`

### 5.2 `SSoT_CompositionCards_TrackStyleBundles.md`
Classification:
- mixed doc
- partly ALWTTT card/gameplay selection truth
- partly package-owned bundle/composer interpretation truth

New governed treatment:
- ALWTTT gameplay/runtime selection truth stays with ALWTTT card/runtime docs
- package-owned internals must be referenced, not duplicated

### 5.3 `SSoT_CompositionAuthoringTools.md`
Classification:
- package-authoring heavy / cross-project reference

New governed treatment:
- not a primary ALWTTT SSoT
- belongs as reference or in MidiGenPlay docs

### 5.4 `SSoT_Composer_BackingChordTrack.md` and `SSoT_Composer_RhythmTrack.md`
Classification:
- package-owned composer internals

New governed treatment:
- do not remain ALWTTT authorities
- reference MidiGenPlay instead

### 5.5 `ALWTTT_MidiGenPlay_TS_Normalization_Roadmap.md`
Classification:
- cross-project historical/planning material

New governed treatment:
- archive/reference only
- package-side normalization truth belongs to MidiGenPlay, not ALWTTT

---

## 6. Update rules at the boundary

### 6.1 Update ALWTTT docs when
- ALWTTT observable gameplay/runtime behavior changes
- ALWTTT changes what it sends into the music pipeline
- ALWTTT changes how it interprets playback/session feedback
- the game-owned `MidiMusicManager` contract changes

### 6.2 Update MidiGenPlay docs when
- package internals change without changing ALWTTT-observable behavior
- package algorithms/composers/tools change within their own authority surface

### 6.3 Update both sides when
- the handoff contract changes
- field meanings at the boundary change
- a previously mixed concept is reassigned to a clearer owner

---

## 7. Non-negotiable rule

**One concept must have one primary home.**

If a concept is primarily package truth, ALWTTT may reference it but must not silently redefine it.
If a concept is primarily game/runtime truth, MidiGenPlay may mention it but ALWTTT remains authority for the game-owned side.

---

## 8. Cross-project contract elements (ask lifecycle log)

Concrete, dated instances of the shared integration surface (§2.3) filed against MidiGenPlay's tracker. **§8.1–§8.8 record asks that were resolved; §8.9 is the register of asks still open (confirmed D1=A, MANIFEST-1 2026-08-08). One boundary, one home for its state.** Each entry records the lifecycle (filed → delivered → adopted) and any ALWTTT-side decision about how the delivered element is consumed.

### 8.1 MGP-ALWTTT-SEED-1 — per-render seed parameter

**Cross-project. Filed → delivered → adopted 2026-07-05 (same day).**

MidiGenPlay added an optional per-render seed parameter to `GenerateSong`/`GenerateSinglePart` (`int? seedOverride`; `null` = pre-adoption bit-identical behavior). Ownership split at this element: the **seed policy** (when a new seed is drawn, what it must not depend on) is ALWTTT truth, governed in `SSoT_Runtime_CompositionSession_Integration.md §10`; the **selection mechanism** that turns a seed into a palette pick stays MidiGenPlay truth.

MidiGenPlay's own package-side **D4** (deterministic anti-repeat exclusion) was **declined** — clone-on-pick makes exclusion-by-reference infeasible package-side. ALWTTT accepted this and operates **probabilistically** instead: with palettes of ≥6 entries, consecutive repetition is ~1/N and is accepted for the demo (`D-S5g-7=C`). If deterministic non-repetition is ever required, it returns as a package-side batch with an explicit palette-entry-identity decision.

---

### 8.2 MGP-BAGGAGE-1 — dead assets in the shipped package catalogue

**Cross-project. Filed 2026-07-20 → answered package-side → adopted 2026-07-20. RESOLVED (MidiGenPlay 1.1.0).**

ALWTTT measured 28 package assets that no consumer referenced and that in most cases could not render at all (empty, no lanes, or all-silent). ALWTTT cannot delete them — edits under `Packages/` revert on package update, and asset ownership is location-based (D-CSV-7=A) — so the finding was filed as an ask rather than actioned.

**Package-side answer.** None of the flagged assets was intentional: no runtime fallback, no editor template, no test fixture depended on any of them. **33 assets retired** and **8 moved** to `Samples/ExampleCatalogue/ChordProgressions/`, which takes them out of `Resources.LoadAll` reach. Package-side additions to ALWTTT's original list: `Test Progression`, `Melodic Style - Test 1`, and the three `_*List` container assets (kept but emptied, package-side **D-BAG-4**).

**Consumer verification (fresh re-export, 183 assets, 2026-07-20).** `EMPTY` / `NO-LANES` / `ALL-SILENT` / `OVERFLOW` are **zero** across all package-origin assets. No live reference was broken and the gig smoke pass is green.

**Standing rule adopted.** If any of those four flags reappears on a package-origin asset, it is a **package-side regression** and warrants a new ask — not consumer-side tolerance.

**Tolerated, do not re-investigate:** `ScriptableObjects/LLM/*`, `Patterns/Phrases/*`, `Tonality Profiles/*`, `Patterns/Emotions/*`, `MIDI Instruments/**`, `_SoundFont Cache.asset`, and the now-empty `Patterns/{Chords,Drums}/Palettes/` folders — these are the canonical enumeration roots fixed by MGP-ALWTTT-DBG-2, so an orphan-folder flag on them is a false positive.

**Rider — patch-index observation, closed.** The package handoff §4.1 attributed the `Poly Synth` / `Warm Pad` duplicate to an extraction artifact in `CompositionInventoryWindow`. That attribution does **not** hold: the window reads `PatchName` and `PatchIndex` **verbatim** (duplicate key `SoundFont|Bank|PatchName|PatchIndex` — four raw fields, no derivation, no 0/1 normalisation), and the 1.0.0 export showed **both** fields of `Poly Synth` carrying Warm Pad's values. The asset was mis-authored; 1.1.0 corrected it (`90 - Poly Synth` / 90, no `DUP` flag). **No ALWTTT code change is owed** — handoff §7.4 is closed as resolved package-side, not as consumer debt.

**Formerly open at this element:** the mix-gain seam (**D-BAG-3 / MGP-MIX-1**). 1.1.0 did not close it; **MidiGenPlay 1.2.0 did** — see §8.3.

---

### 8.3 MGP-MIX-1 — consumer-side mix gain (bytes plane)

**Cross-project. Filed 2026-07-20 (CSV-4, as D-BAG-3) → delivered 2026-07-20 (MidiGenPlay 1.2.0) → ADOPTED (BAL-1 closed 2026-07-22; all 7 smoke tests green on 1.2.0).**

MidiGenPlay 1.2.0 (`SongOrchestrator.cs`) adds a trailing optional `IReadOnlyDictionary<MusicianTrackKey, float> mixGains` on `GenerateSinglePart`. One entry ⇒ one **CC7 (channel volume)** event on that track's channel, inserted after its bank/patch preamble and before the part-position shift.

**Composition law.** `effectiveCc7 = clamp(round(volume01 × gain × 100), 0, 127)`. Multiplicative over the package-side `volume01` (today **1.0 on all 70 melodic instruments, unauthored**). Default `gain = 1.0` is identity (CC7=100, the GM channel-volume default). `gain = 0` mutes at playback but leaves note events in the file — stems, hashes and readback keep working. Boost saturates at `gain ≈ 1.27` (CC7=127) while `volume01` stays unauthored.

**Emission gate — the identity guarantee.** A CC7 is emitted **only** for tracks with an entry in the map. Null map, empty map, or a track without an entry ⇒ zero new events ⇒ **bit-identical to 1.1.0**. Test-pinned package-side; to be verified consumer-side by the byte-compare gates at adoption (checked, not trusted).

**Keying.** `MusicianTrackKey (musicianId, TrackRole)` — the BASS-1 / DBG-C1 composite key, same as `instrumentOverrides` / `patternOverrides` / stems. Channels never appear in the consumer surface.

**Percussion is out of v1** (package D-MIX-4=A): every Rhythm track shares MIDI channel 9, so a per-drummer CC7 is not expressible. A `TrackRole.Rhythm` entry is **warn + ignore** — same contract shape as Bassline in `patternOverrides` v1. ALWTTT does not request drum-level balance at this time (D-BAL-5=A); the live plane can level channel 9 as a block if a calibration pass demands it.

**Readback.** `PartRender.appliedCc7ByTrack : Dictionary<MusicianTrackKey, int>` — the CC7 actually emitted, entries only for gained melodic tracks. Consumed by the Dev Mode truth surface and as a cheap pre-byte assertion.

**Determinism.** Pure data path: no RNG, no seed-chain involvement. Same seed + same map ⇒ same bytes.

**Scope limit — full-song path.** `GenerateSong` does **not** take the map in v1. ALWTTT's gig loop is per-part and is covered; the jam/menu path and pre-generated `SongCacheEntry` renders (`MidiMusicManager.GenerateSongEntry`) stay ungained. **Known limitation, not an ask** — revisit only if an export or menu path needs balance.

#### Plane separation (normative for ALWTTT)

MIX-1 lives in the **bytes**: deterministic, hash-relevant, visible to the byte-compare gates. The **live-playback plane** — `IMixController` / `PassthroughMixController`, i.e. ALWTTT's M-AUDIO-MIX per-musician axis (`SSoT_Audio.md` §4) — is a separate control for ducking/highlight and player-facing balance. Neither is meant to replace the other.

#### Consumer verification — results (2026-07-21)

1. **Upgrade / compile.** ALWTTT consumes the package as a local `file:` reference (`Packages/manifest.json` → `file:D:/Projects/MidiGenPlay/MidiGenPlay`), so 1.2.0 is resolved in place with no Package Manager step and no cache. `package.json` is at `1.2.0`; `mixGains` / `appliedCc7ByTrack` present in `SongOrchestrator.cs`. **Clean-compile confirmation still owed in-editor** (expected green; the trailing optional parameter is source-compatible for the single caller).

2. **No ALWTTT-side `ISongOrchestrator` double — CONFIRMED.** Repo-wide grep in both trees: the interface is declared and implemented **package-side only** (`SongOrchestrator.cs`); `MidiGenerator` merely holds a reference. No stub needed. The trailing parameter broke nothing.

3. **MPTK identity default = 100 — CONFIRMED.** `Assets/MidiPlayer/Scripts/MPTKGameObject/MPTKChannels.cs:574` → `cc[VOLUME_MSB] = 100;` (comment: `// V2.88.2 before was 127`). This matches the package's ×100 identity scale: `gain = 1.0` (CC7=100) is level-neutral next to ungained tracks. **No report back to the package is owed.** *Standing note:* this holds for MPTK ≥ 2.88.2; a downgrade below that version reintroduces a 127 default and would break the identity assumption.

4. **Live-plane collision — CONFIRMED, then RESOLVED (BAL-1 task 0, 2026-07-22).** `PassthroughMixController.SetChannelVolume01` maps `volume01 → 0..127` and calls `_player.SetChannelVolume(channel, v127)`, and MPTK realises channel volume by writing the **same `cc[VOLUME_MSB]` register** the baked MIX-1 CC7 targets — one register per channel, last writer wins. Both planes collapse onto CC7 in ALWTTT's passthrough player; the package's "the two planes compose at the synth" holds only for a player that writes a *separate* channel multiplier, which this one does not.

   **Task-0 read corrected the mechanism recorded here originally.** Two findings from the MPTK sources (`MidiFilePlayer.cs`, `MidiSynth.cs`, `MPTKChannels.cs`):

   - **F-MPTK-1 — the channel reset is real and per-play (verified).** `MPTK_Play` → `MPTK_InitSynth` (MidiFilePlayer.cs:1109) reallocates the channel set whenever `Channels.EnableResetChannel` is true (the **default**, MPTKChannels.cs:145; MidiSynth.cs:1481), and every `MPTKChannel` ctor runs `fluid_channel_init_ctrl()` → `cc[VOLUME_MSB] = 100` (MPTKChannels.cs:310–311, 574). So MPTK wipes every channel's CC7 back to 100 on **each** play. The old "defensive against an *unverified* reset" premise is now **verified true** — the re-assert loop **cannot be retired**. (D-BAL-7.)
   - **F-MPTK-2 — the original race direction stated here was inverted.** In the play coroutine `OnEventStartPlayMidi.Invoke` fires (MidiFilePlayer.cs:1563) **before** the event do-loop processes tick-0 events. So `OnSongStartedInternal`'s immediate writes land *before* the baked preamble CC7 — the **baked CC7 is the last writer at song start**, not the re-assert. The real failure mode is the mirror image: the live plane's start-time re-assert (persisted balance) is stomped by the baked preamble, and any *later* live write (slider/highlight/restore) stomps the baked gain. Neither plane reliably survives.
   - **F-MPTK-3 (minor)** — `ResetExtension` per play also resets `MPTKChannel.Volume = 1` (a separate float multiplier our path does not use). No action.

   **Resolution (D-BAL-6=B + D-BAL-7).** Both problems dissolve with two ALWTTT-side moves in `MidiMusicManager`: (1) all musician-channel live writes route through a single boundary `WriteChannelVolume01(ch, live01)` that **composes multiplicatively** with the baked gain — `composedCc7 ≈ round(live01 × bakedGain × 100)` — so live-vs-baked stops being last-writer-wins (identity writes reproduce the baked CC7 exactly, so the re-assert is idempotent on ungained channels); and (2) the per-song re-assert is **deferred** (bounded coroutine `ReassertLiveMixAfterPreamble`, waits for `CurrentTick > 0`) so non-identity persisted balance lands *after* the tick-0 preamble and wins correctly. The loop is kept (F-MPTK-1), deferred (F-MPTK-2), and composed (D-BAL-6=B). See `SSoT_Audio.md` §4.2 for the live-plane model.

#### Cross-dependency — do not double-compensate

The D-CSV-18 listening verdicts (currently **blocked** by CSV-3 / D-MEL-1) are the input to the package-side `volume01` authoring batch (**D-MIX-6**, later version). Until it lands, all `volume01` = 1.0 and ALWTTT gains act alone; once authored, the two **compose multiplicatively** — the package normalisation flows through ALWTTT gains rather than invalidating them.

**Anti-compensation rule (normative).** ALWTTT gains express **ensemble intent** ("this bass sits back in the mix"). They must **not** be calibrated to offset per-patch loudness imbalance — that is `volume01`'s job, and the compensation would double up when D-MIX-6 lands. Deliberately undecided package-side until then: whether authored `volume01` values emit CC7 for tracks *without* a gain entry.

#### ALWTTT-side decisions locked (2026-07-21)

- **D-BAL-1=C** — gains live in a dedicated content SO (`MixGainProfileSO`), keyed `(musicianId, TrackRole)`, implicit default 1.0. Not on the character/card, and not in `AudioMixSettingsSO` — the planes stay in separate assets.
- **D-BAL-2=A** — hand-authored ensemble balance, subject to the anti-compensation rule above.
- **D-BAL-3=A** — fixed per gig, resolved at gig start. **The gain enters the track-inputs hash regardless of that lifecycle**, so stem/bundle cache replay can never serve stale CC7. Reactive in-game balance, if ever a mechanic, belongs to the live plane, not to the bytes.
- **D-BAL-4=A** — content data, versioned with the game; not in the player save. (Player-facing balance is the live plane, already persisted in `AudioMixSettingsSO`.)
- **D-BAL-5=A** — no drum-balance ask filed. Revisit after the first calibration pass, with evidence.

#### BAL-1 resolution decisions (locked 2026-07-22, code closed)

- **D-BAL-6=B** — a musician-channel live write **composes multiplicatively** with the baked bytes-plane gain at a single write boundary (`WriteChannelVolume01`): `composed01 = live01 × bakedGain01 × (100/127)`. Identity in ⇒ identity out (live 1.0 × gain 1.0 → CC7=100). Chosen over stomp+skip (planes would not compose; the pinned ~0.25 composition smoke would fail) and over a hybrid. Audible side effect: live full-volume is now CC7=100, not 127 — this is a level *correction* (it removes an existing inconsistency where live-written channels sat above MPTK's own 100 default), not a regression.
- **D-BAL-7 = keep-deferred-composed** — the `OnSongStartedInternal` re-assert loop is **not retired** (F-MPTK-1: reset is real) but is **deferred past tick-0** (F-MPTK-2) and its writes go through the composed boundary. Under D-BAL-6=B, selectivity is no longer a correctness question — identity writes are idempotent against the baked preamble.
- **D-BAL-8=A** — the `MixGainProfileSO` is resolved from a **serialized field on `GigManager`** (one profile, whole game), next to `audioMix`. A per-encounter `GigRunContext.RunConfig` override was the alternative; trivially movable later if needed.

**Implementation — BAL-1, CLOSED 2026-07-22.** `MixGainProfileSO` (content SO, `Assets/Scripts/Data/Audio/`); gig-start resolution in `GigManager.StartGig` → `MidiMusicManager.SetGigMixGains`; `mixGains:` threaded into the per-part `GenerateSinglePart` call; gain folded into `ComputeHashFromTrackEntry` (stem + bundle keys); `appliedCc7ByTrack` readback surfaced (incl. bundle-cache replay); Dev Mode → Audio Mix gains section + live-composed CC7 strip. Smoke 1–7 green on 1.2.0 (byte identity ungained; gain=1.0 identity CC7=100; gain=0 mute-without-delete; plane composition ~0.25 / ~50 at live 1.0; start-race F-MPTK-2 regression; Rhythm warn+ignore; cache replay after gain change).

---

### 8.4 MGP-ALWTTT-BASS fidelity trio — RESOLVED package-side before filing (2026-07-28)

The three bass-fidelity items registered at campaign planning as future asks
(`planning/active/RosterExpansion_Sub_Roadmap.md` §8 #1–#3, under **D3=A**: "Conito bass ships
v1 approximations now; the fidelity items are MidiGenPlay asks") were **delivered by MidiGenPlay
before ALWTTT filed them**. The cross-boundary response dated **2026-07-28** invalidated the
ALWTTT-side mirror of the bass surface. Recorded here so the non-filing is traceable and the
trio is not re-derived as open.

**Delivered surface (`BasslineCardConfigSO`, package-owned, all additive with BC defaults):**

| Ask (was) | Delivered as | ALWTTT adoption |
|---|---|---|
| #1 Bass chord-tone walk | `arpeggioToneMode` = `RepeatedNote` (default) / `ChordToneWalk` / `ImprovisedWalk` | **Adopted R2** — Finger Bass v1: `ArpeggioUp` + `arpeggioRate = PerBeat` + `ChordToneWalk` (**D-R2-4=A**) |
| #2 Bass pocket-coupling | `pocketMode = SlapPocket` + POCKET-2 refinements (`pocketSlapBoost`, `pocketPopBoost`, `pocketCustomLanes`, `pocketSlapLanes`, `pocketPopLanes`) | **Not adopted in this form.** Superseded for ALWTTT's use case by `SelfPocket` — see §8.6 |
| #3 Bossa bass/upper split | `ChordExpressionType.BassUpperSplit` | **Not adopted.** Consumer is the R8 *Bossa Corda* reward card |

**Naming trap — normative for card authors.** `ChordExpressionType` reads `Block, PerBeat,
Offbeat, Staccato, ArpeggioUp, ArpeggioDown, Random, PowerChord, Chugging, BassUpperSplit,
Bossa`. The member named **`Bossa` is NOT the register-selective split** — it designates a
different figure. The split delivered for ask #3 is **`BassUpperSplit`**. Authoring `Bossa`
expecting a split imports cleanly and sounds wrong.

**Conditional consumer duty — pocket coupling and the cache key.** `SlapPocket` makes the bass
line's rendered bytes depend on the **Rhythm track's resolved pattern**, which the Bassline
`trackInputsHash` does not include. **If any ALWTTT bundle sets `pocketMode = SlapPocket`, the
bass track's hash must be extended with the resolved Rhythm pattern inputs in the same batch**,
or the cache can replay a bass stem rendered against a since-replaced drum pattern.

**Status of that duty: DORMANT.** ALWTTT's slap content uses `SelfPocket` (§8.6), which reads no
other track and therefore does not trigger it. The duty re-arms the moment any bundle selects
`SlapPocket` — or a future `Auto` mode that falls back to rhythm coupling when a Rhythm track is
present (registered as a design idea 2026-07-31; **not requested**, precisely because it would
re-arm this duty and cost a consumer-side batch, not just a package enum member).

**One package behavior ALWTTT must not assume away:** `RomanProgressionParser` does **not** fail
on an unknown quality suffix — it warns and infers a diatonic quality. Any pipeline treating
"the import succeeded" as "the content is what I asked for" is wrong; warnings must be read.

**Package-side content defect, accepted (D-R2-5=A).** GM Slap Bass 1/2 are authored
`octaveMin = 1` / `octaveMax = 3` (roots C0–B1), which reads muddy. Corrected in MidiGenPlay's
content phase B. R2 closed with this as a known state rather than blocking. **Materially reduced
by the `SelfPocket` adoption** — the pop hits at +12 restore register contrast, so the timbre
distinction no longer depends on the muddy low octave.

**Melody, for the record (R3 input, no change):** `MelodyPatternData` remains
`(degree, octaveOffset)` and is **not** chord-aware; **D-MEL5.1=A** stands. Sub-roadmap §8 ask #5
remains open, owned by R8.

---

### 8.5 MGP-ALWTTT-BASS-SOLO-1 — host default progression for backing-less parts — ADOPTED (R2c, 2026-07-30)

**Package side (delivered, D-SOLO-SRC=A / D-SOLO-SURF=A2).** The shared progression channel had
exactly two publishers: the backing composer and the authored fallback
`SongOrchestrator.FindProgressionForPart` (which reads the **Backing** track's `Pattern`). A part
holding a Bassline / Melody / Harmony row and **no Backing row** therefore had no harmony source
at all, and the consumer rendered **silence by rule** — `ctx.GetProgressionForPart(part) ??
cfg.Parameters.Pattern`, null ⇒ empty `MidiFile`. `GenerateSinglePart` accepts a trailing
optional `ChordProgressionData defaultProgression`, pre-seeded into the per-render shared cache
before the track loop, so every harmony consumer sees it through the normal channel.

**Ownership is unchanged by adoption.** The bass still owns no harmony. `BasslineCardConfigSO`
gains **no** harmonic field, and the D-DBG4=A warn+ignore on a Bassline `patternOverride` stands.
The parameter is a **host channel into shared state**, not a bass surface.

**ALWTTT side (R2c, D-R2-6=B).** `MidiMusicManager` holds a serialized
`ChordProgressionPaletteSO defaultProgressionPalette` and resolves one progression per render
inside `RenderSinglePart`, before the cache-key computation:

- **A palette, not a single progression (D-R2-6=B).** `PickRandomProgression` (weighted,
  `cloneResult = true` — the asset is never mutated) gives minimum harmonic variety from a
  surface that already existed.
- **Determinism.** Seeded from `(seedOverride, partIndex)`, where `seedOverride` is the per-song
  render seed (§10, D-S5gb-2=B: one seed per song, stable Begin→End). Stable within a song — so
  caches stay coherent and a re-render replays the same harmony — and varied across songs.
  `CompositionSession` needed no change; the seed was already threaded.
- **Applicability guard — REWRITTEN at R2d.** The original R2c guard was "part has a harmony
  consumer **and no Backing track**", mirroring the package's D-SOLO-GUARD=A. That proxy is
  **retired**: since ORDER-1 (§8.6) the package sniffs whether the Backing row actually *carries*
  a harmony source, so Backing and a live default can now coexist. ALWTTT passes the default
  whenever the part has **any** harmony consumer (**including Backing**) and a palette is
  assigned, and lets the package's sniff decide. ALWTTT deliberately does **not** replicate that
  sniff client-side — it just changed once; duplicating it guarantees drift.
- **Cache participation.** See DIFF-R2-3.
- **Inertness.** Palette unassigned ⇒ null argument ⇒ byte-identical to pre-SOLO-1 and hash
  string unchanged. **The demo scene leaves the palette unassigned**, so the S5i baseline is
  inert by construction, not by discipline (`ST-R2c-4` / `ST-R2d-4` PASS).

**Smokes:** `ST-R2c-1..4`, `ST-R2d-2`, `ST-R2d-4` — all PASS.

---

### 8.6 MGP-ALWTTT-BASS-ORDER-1 + MGP-ALWTTT-BASS-SLAPFIG-1 — filed and delivered 2026-07-31 — BOTH ADOPTED

Two demands filed by ALWTTT on 2026-07-31 and delivered package-side the same day. Unlike the
§8.4 trio, these were genuinely filed: ORDER-1 came from a live gig failure, SLAPFIG-1 from a
content need.

**ORDER-1 — shared harmony is independent of track order.**

*The failure (F-BASS-ORDER-1, ALWTTT-observed 2026-07-30).* Track order in a part is the order
the player played the cards. Playing a Bassline card **before** a Backing card put the bass first
in the list; the package composed in list order; the bass read the shared-progression cache
before the only publisher had written to it; and `FindProgressionForPart` could not save it,
because it inspects `Parameters.Pattern` — null when the card's harmony lives in the style
bundle. Result: **null progression ⇒ empty `MidiFile` ⇒ permanent silence for that part**, not
recoverable on later loops (the shared cache is per-render). This was the hard form of the
normalization-order hazard already recorded in MGP `SSoT_Composer_Bass_Track §1`.

*Why ALWTTT could not fix it.* Reordering the track list Backing-first was rejected: the list
order feeds `ChannelRoles` / `ChannelMusicianOrder` (per-musician mute, singer duck, mix plane)
and the RNG consumption order in `SongConfigBuilder.FromUI`, so reordering moves channel
assignments and resolved instruments — it breaks byte identity for existing content. **Track list
order is consumer identity.** That is now a package-side contract (`MGP SSoT_CONTRACTS §10`).

*Delivered (D-ORD-MECH=A).* Three composition passes over the same list — PASS 0 Backing,
PASS 1 everything except Backing/Harmony, PASS 2 Harmony — with the physical merge **deferred and
performed in list-index order**, so file chunk order still follows the list even though
composition order does not. Applied to both entry points. Channel assignment, `ChannelRoles`,
`mus:` tags, per-track seeds (keyed `(role, musicianId)`, never composition order) and byte layout
are all unchanged. **Log-reading note:** the `Merged [role]` line is emitted at composition time,
so the log now starts with Backing while the file keeps list order. Intentional, not a symptom.

*Guard rewritten (D-ORD-GUARD=A).* The host `defaultProgression` is now discarded only if the
Backing row **carries** a harmony source (per-render `ChordProgressionData` override, card
`progressionOverride`, palette with ≥1 valid weighted entry, or an authored `Pattern`). An
**articulation-only Backing card** — the future bossa / ska / power-chord cards — no longer
suppresses the default; the Backing composer consumes it and, as a bonus, meter-normalizes and
re-qualifies it, which the raw SOLO-1 path did not.

**Normative precedence for the shared progression channel** (card-authoring truth):

1. `patternOverride` per-render on the Backing row (dev; imposes unconditionally)
2. Backing card: `progressionOverride`, else weighted palette pick
3. **host `defaultProgression`** (SOLO-1) — now also under an articulation-only Backing card
4. authored `Pattern` on the Backing row
5. procedural generation

**Consequence: harmonic silence is no longer a reachable state** in any part with at least one
source, regardless of play order.

*New readback (D-ORD-RB).* `PartRender.sharedProgressionSource` (`ResolvedSource`, new member
`HostDefault = 7`; values 0..6 untouched) and `PartRender.sharedProgressionAssetName` report which
source **won** the channel. ALWTTT publishes both as `MidiMusicManager.LastSharedProgressionSource`
/ `LastSharedProgressionAssetName`, stores them in the bundle-cache entry so a replay republishes
the original verdict (D-DBG5=A precedent), and logs an `[ORDER-1] harmony source=…` line.

**This readback is a verification surface, NOT a cache-key input** — see DIFF-R2-3b / invariant 9
for why the package's suggested "condition the `dp:` token on `sharedProgressionSource ==
HostDefault`" is not implementable, and what ALWTTT does instead.

*Gaps declared by the package, out of scope, recorded:* (a) the bass's private `Pattern` still
receives no meter normalization or re-qualification — it is private harmony, outside the shared
channel; (b) a palette that passes the sniff but whose pick fails on time signature at compose
time makes Backing degrade to procedural **without the suppressed default resurging** — not
silence, and the pre-existing "failed palette pick" semantics.

**SLAPFIG-1 — autonomous slap/pop figure (`PocketCouplingMode.SelfPocket = 2`).**

Delivered append-only (`Off` and `SlapPocket` byte-identical). **Reads no other track** — in
particular it never calls `GetRhythmOnsetsForPart`, verified package-side by a test pinning the
bass stem byte-identical with and without a Rhythm row. **This is the whole point: `SelfPocket`
does not arm the §8.4 cache duty; `SlapPocket` does.**

- Hit source: cyclic `selfPocketPattern` over `{Slap, Pop, Rest}` (default `[Slap, Pop]`) on a
  `selfPocketSubdivision` grid (`Beat` / `HalfBeat`) **anchored to the bar, not the chord**, so
  the figure keeps phase across chord changes — matching what absolute drum onsets did.
- Downstream verbatim from `SlapPocket`: pop = selected note **+12** with register-ceiling fold,
  percussive gate ≤ 0.5 beats, jitter refold. Timbre stays with the patch (GM Slap Bass 1/2);
  the mode supplies timing, register and dynamics.
- Velocity: chord-event velocity plus the existing `pocketSlapBoost` / `pocketPopBoost`,
  clamped 1..127. **Authoring note:** large symmetric boosts (e.g. `+64/+64`) saturate every hit
  to 127 and flatten the contour. Small and asymmetric (`0` slap / `+12` pop) is the tuned
  default (package recommendation after hearing ALWTTT's gig log; adopted).
- Degradation: empty or all-`Rest` pattern ⇒ one warning + normal figure, byte-identical to
  `Off`. Never an error, never silence.

*ALWTTT adoption (D-R2-11).* Slap Bass v1 re-authored onto `SelfPocket` with
`selfPocketPattern = [Slap, Pop]`, `selfPocketSubdivision = Beat`, boosts `0` / `+12`. This is a
**bundle field change only** — no card re-import.

*Agreed but not delivered (SLAPFIG-2):* mutes, ghost notes, hammer/pull, accents, swing placement.
v1 speaks only Slap / Pop / Rest.

**Smokes:** `ST-R2d-1` (order-independent audibility) · `ST-R2d-5` (slap/pop with no drums) ·
`ST-R2d-6` (bass stem identical with and without a Rhythm row — autonomy verified consumer-side
as well as package-side) — all PASS.

**F-BASS-ORDER-1 is CLOSED.**

### 8.7 MGP-ALWTTT-ARTIC-1 — ADOPTADO · y defecto derivado F-ARTIC-RATE-RANDOM-1 (2026-08-03)

**Adopción (registro retroactivo).** El ask de articulación aleatoria está
**implementado package-side** desde el snapshot MGP-20260729: centinela
`ChordExpressionType.Random` resuelto por evento de acorde vía
`RandomArticulationRoller` sobre stream dedicado (`SongOrchestrator.ResolveArticulationSeed`),
`randomFigureWeights` opcional por carta (SD-2=A), `randomRerollChance`, y el espejo
`ArpeggioRate.Random` en su propio substream (CA-V1, D-V1-RATE-SEL=A). Contrato:
`SSoT_Composer_Backing_Track §8.5`. `CURRENT_STATE §5` seguía listando la entrada como
debida "cuando el ask se filtre package-side"; el ask ya estaba entregado. Corregido aquí.

**Consumo ALWTTT.** El override de articulación de Dev Mode (§18.14) expone ambos
centinelas como valores seleccionables: son valores de carta legítimos y reproducibles
bajo seed pineado.

**F-ARTIC-RATE-RANDOM-1 — comportamiento OBSERVADO, contradice el contrato escrito.**
Medido 2026-08-03 (ST-CTX2B-2b, herramienta CTX-2b, seed pineado):

- `chordExpression = Offbeat` (figura concreta) + `arpeggioRate = Random` ⇒ render
  **sin articulación**; suena como `Block`.
- La misma configuración con `arpeggioRate = PerBeat` ⇒ `Offbeat` correcto. El rate es
  la única variable.
- El consumidor queda descartado como causa: `trackInputsHash` se movió, `cacheEnabled=True`,
  render fresco confirmado (creció `bundleCache`), sin `bundle HIT`; y el **mismo** plano
  de clon con rate concreto sí articula ⇒ el clon es honrado por el composer.
- **Sin warning de degradación.** §8 exige "never silent" para todo degrade.

Contradice dos afirmaciones escritas: §8 (doc del enum `ArpeggioRate`: *"Ignored by all
other expressions"* — con `Offbeat` el campo debe ser inerte) y §8.5 / D-V1-RATE-SEL=A
(substream dedicado, *"so the figure roll sequence is unaffected by this knob"*).

**Impacto de contenido ALWTTT.** Hay bundles ya autorados en esta combinación —
`Backing Card Config - Core Minor` (`expr=Random`, `rate=Random`). Pierden articulación
en silencio. Auditoría de assets **no** ejecutada (ver `CURRENT_STATE §4`, D-ARTIC-AUDIT).

**Estado:** filtrado al proyecto MidiGenPlay como **MGP-ARTIC-RATE-1** (2026-08-03,
sesión paralela). Mitigación consumer-side: aviso en la UI del Dev Mode, sin coerción del
valor. Retirar el aviso al cerrar el ask.

### 8.8 R3 — shared-harmony readbacks and adopted tonality (2026-08-08)

**Consumed surface (additions).**

- `PartRender.sharedProgressionData` — the progression object that won the shared channel,
  added alongside `sharedProgressionSource` (R2d, §8.6). ALWTTT publishes it as
  `MidiMusicManager.LastSharedProgressionData` and stores it on `PartBundleCacheEntry`, so a
  bundle replay republishes it. Like its two siblings, it is a **verification and continuity
  surface, not a cache-key input**.
- `adoptProgressionTonality` (Backing bundle field) — consumed as the second condition of the
  JAM-1 imposition guard. The authoring contract it implies is normative ALWTTT-side:
  `SSoT_Card_Authoring_Contracts.md` §5.17.

**D6=B recorded as host policy.** The package reports *what won the shared channel*; **ALWTTT
decides what "joining a jam" means.** Rationale: keeping that policy package-side would bind
every consumer of MidiGenPlay to ALWTTT's band fiction. The package answers a factual
question; the game answers a fictional one.

**Correction to the lost-name defect (amends the R2d/E3 record).** The defect affects the
**clone's Unity object `name`**, not the reported asset name. `LastSharedProgressionAssetName`
is correct on all sources — verified live on `CardOverride`
(`asset='Test - Lab Progression'`). The empty name is confirmed **only** on the `CardPalette`
path; the `CardOverride` path is unverified. **Do not state the broader claim.**

**Adopted tonality is read host-side, not requested from the package.** ALWTTT consumes the
adopted tonality by reading its own `cfg.Parts[i]` *after* the render returns, rather than
asking for a readback. No new consumed surface — but recorded here deliberately, because it
means the package's **in-place mutation of `PartConfig` during compose is load-bearing for the
host**. A future package refactor that rebuilt `PartConfig` internally, or that copied it
before mutating, would silently break JAM-2 with no compile error and no warning. If that
refactor is ever planned, ALWTTT needs a real readback first.

### 8.9 Registro de asks ABIERTOS (filed, not yet delivered) — abierto 2026-08-08

> **Nota estructural.** §8.1–§8.8 son un *delivered/adopted log*: registran asks ya resueltos.
> Los asks **abiertos** no tenían hogar en este documento y vivían dispersos entre
> `CURRENT_STATE §5` y los sub-roadmaps, que es exactamente el patrón que produjo la deriva de
> registro de MGP-ALWTTT-ARTIC-1 (§8.7: entregado package-side y seguido listando como debido).
> Esta sección centraliza el estado abierto. **Se mantiene aquí solo el hecho de la frontera**;
> el detalle técnico package-side no se gobierna en este proyecto.

| Ask | Filed | Dominio | Estado | Contenido |
|---|---|---|---|---|
| **MGP-MEL-1** | 2026-08-05 | Pipeline de melodía | Enviado | Ocho puntos (P1..P8). P1 selección de altura estancada dentro de la frase (fue bloqueante de Showtime) · P2 campos serializados inertes · P3 observabilidad del leading efectivo · P4 progresiones modales vs tonalidad de la parte · P5 viabilidad de "Rise Up adaptativa" · P6 refinamiento/documentación de la superficie de autoría de melodía · P7 propiedad de la progresión al añadir pistas a un jam en marcha · P8 `totalSlotsInPhrase` inconsistente. P6 es transversal: lo motivan tres campos serializados inertes y una tabla de precedencia no documentada. |
| **MGP-ARTIC-RATE-1** | 2026-08-03 | Backing / articulación | Enviado | Figura concreta + `arpeggioRate = Random` ⇒ render sin articulación, y **sin warning** pese a la regla "never silent". Detalle y evidencia: §8.7. |
| **MGP-CHD-ASCII-1** | 2026-08-08 | Marcador `chd:` | Enviado, **sin prioridad asignada** | ¿El marcador `chd:` debe ser **ASCII puro** o **UTF-8**? Ver §8.10 — hoy ALWTTT no puede distinguir dos causas posibles y ha mitigado consumer-side. |
| **MGP-LOG-VERBOSE-1** | 2026-08-08 | Logging del generador | Enviado | **Partir `MidiGenPlayConfig.logGenerator`.** Hoy es **un solo bit** que contiene a la vez `[MelodySlot]` (una línea por nota — el volumen dominante de la consola) y `[ChordTrack] Tonality`, de la que dependen tests del host (ST-A7, ST-J3). El host no puede silenciar el ruido sin perder un observable protegido, ni conservar el observable sin tragarse el ruido. Pedido: dos bits, o un bit por familia de línea. Contexto host: `SSoT_Dev_Mode.md` §19.1/§19.2. |

### 8.10 La tipografía de la etiqueta de acorde es propiedad de ALWTTT (LOG-1, 2026-08-08)

**Posición de frontera, en una frase: MidiGenPlay decide qué SUENA; ALWTTT decide cómo se
DELETREA en pantalla.**

**El defecto observado.** La etiqueta de acorde en pantalla mostraba `I?7` donde debía leerse
`Imaj7`. El glifo de séptima mayor (`Δ`), el círculo tachado del semidisminuido y los signos de
sostenido y bemol llegan al host ya destruidos, sustituidos por un `?` literal.

**Por qué no se puede "limpiar".** Los text-events MIDI se escriben por defecto en un alfabeto
de 7 bits, y cualquier carácter no mapeable se sustituye por `?` **en el momento de la
escritura**. Cuando el host lee los bytes, el carácter original ya no existe: ninguna limpieza
lo recupera. Y un `I7` de aspecto reparado significaría un **acorde distinto** del `Imaj7` que
realmente suena. Reparar por adivinación no es una opción aquí.

**La solución (D-LOG-1=B): no mostrar nunca el glifo del marcador.** El marcador es
autodescriptivo: además del símbolo lleva `deg` (un entero) y `quality`
(`ChordQuality.ToString()`), **ambos ASCII por construcción**. La etiqueta se reconstruye desde
ahí. Precisión de implementación, para no colapsar verdad documentada con verdad de código: el
núcleo de numeral romano se toma del campo `raw` **filtrado a numerales ASCII** (un glifo
inicial que no sea `b` o `#` ASCII se **descarta**, no se muestra), y el **sufijo** se deriva de
`quality` mediante la tabla ALWTTT-owned. El campo de símbolo del marcador no se muestra nunca.

**El acoplamiento es por CADENA, no por el enum — decisión de frontera.** La tabla de sufijos
hace `switch` sobre el **nombre en texto** de la calidad, no sobre el valor de
`MusicTheory.ChordQuality`. Motivo: **ese enum es package-owned y append-only.** Un `switch`
por valor fallaría al compilar ante un rename y se volvería silenciosamente no-exhaustivo ante
una adición. Un `switch` por cadena no puede hacer ninguna de las dos cosas: un nombre
desconocido cae al `default`, que **se reporta a sí mismo una vez por render** con el nombre
real, de modo que la tabla se completa a partir de una línea de log en vez de a partir de un
fallo de compilación en el proyecto del consumidor. Es un acoplamiento deliberadamente más
débil, elegido porque la frontera es de versionado, no de tipos.

- Calidades **confirmadas** contra la SSoT de autoría del paquete (4.1): `Major`, `Minor`,
  `Major7`, `Minor7`, `Dominant7`, `Major6`, `Minor6`, `Dominant7sus4`, `Dominant9`, `Major9`,
  `Minor9`.
- Calidades **no verificadas** contra `MusicTheory.ChordQuality`, incluidas por nombre esperado:
  `Diminished`, `Diminished7`, `Augmented`, `HalfDiminished7`, `Sus2`, `Sus4`. Si alguna está
  mal, la rama `default` dispara y el warning `[LOG-1]` imprime el nombre real. **Un `case` que
  nunca casa no cuesta nada.**

**Causa raíz: acotada a dos hipótesis, discriminador PENDIENTE.**

- **H1 — pérdida de encoding en el transporte:** el text-event MIDI destruye toda la clase de
  glifos no-ASCII al escribirse.
- **H2 — el paquete emite `?`:** el marcador ya sale del generador con el carácter sustituido.

**No se afirma ninguna de las dos.** El instrumento discriminador ya existe y está en el
código: `ReportChordTagDamage` imprime los campos **crudos** una vez por render. La lectura
decide — si `raw sym` todavía muestra un sostenido o bemol real, el transporte está bien y el
paquete escribió el `?` (H2); si `raw sym` también viene dañado, el transporte está destruyendo
la clase entera de glifos (H1). **El dato no se ha capturado todavía**, así que el ask
**MGP-CHD-ASCII-1** queda archivado **sin prioridad asignada** hasta que se lea.

**Estado de la mitigación:** consumer-side y completa. La etiqueta en pantalla es correcta hoy
con independencia de cuál de las dos hipótesis sea cierta, porque la reconstrucción no depende
del glifo. Lo que el ask decide es si el marcador `chd:` debe ser contrato ASCII-puro o
contrato UTF-8 — es decir, de quién es el bug, no si el jugador lo ve.

**Superficie adoptada, recordatorio (ver §8.8):** el host lee la tonalidad adoptada de su propio
`cfg.Parts[i]` post-render. La mutación in-place del `PartConfig` durante compose es
**load-bearing** para el host.

---

### 8.11 Reenvío MGP-TONALITY-1 — síntomas de melodía / tonalidad / compás (PRES-1b → R5, 2026-08-11)

Tres síntomas observados en runtime durante PRES-1b — notas de melodía fuera de la tonalidad
declarada, ausencia de silencios inter e intra frase, y frases que no encajan con el beat — más
la duplicación del tempo percibido en el Compound Cycle (6/8), pertenecen a **internals de
MidiGenPlay** y **no se documentan como verdad de ALWTTT** (D-PRES1c-2=A). Abiertos en el
proyecto compañero como **MGP-TONALITY-1**.

Lo único gobernable desde este lado de la frontera es **qué valores inyecta la carta en
`SongConfig`**: `TimeSignature` y tonalidad, vía `SongConfigBuilder` y el asset de la carta.

**Acción propuesta antes de arrancar el ítem compañero:** un log de verificación en
`SongConfigBuilder` que imprima los valores efectivos enviados por canción. Si el valor *enviado*
ya es incorrecto, el defecto es nuestro y no cruza la frontera; si es correcto, el ticket
compañero arranca con evidencia en lugar de con sospecha. Es la misma disciplina que §8.10 aplicó
al daño de glifos en la etiqueta de acorde: medir de qué lado nace el dato antes de asignar el
bug.

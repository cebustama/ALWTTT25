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

## 8. Cross-project contract elements (delivered/adopted log)

Concrete, dated instances of the shared integration surface (§2.3) that were filed against MidiGenPlay's tracker and resolved. Each entry records the lifecycle (filed → delivered → adopted) and any ALWTTT-side decision about how the delivered element is consumed.

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

# SSoT_Runtime_CompositionSession_Integration — ALWTTT

**Status:** Active governed SSoT  
**Scope:** ALWTTT-side runtime bridge from Composition card play to composition-model mutation, playback rebuild, and loop feedback  
**Owns:** ALWTTT observable runtime contract for `CompositionSession`, `SongCompositionUI`, `SongConfigBuilder`, and playback rebuild decisions  
**Does not own:** package-internal composer precedence, algorithmic track generation internals, or generic MidiGenPlay authoring tools

---

## 1. Purpose

This document is the primary ALWTTT authority for **how live composition works at runtime**.

It promotes the ALWTTT-owned parts of the previous runtime bridge material while deliberately removing package-owned internal generation truth.

---

## 2. Core separation that must hold

When a **Composition card** is played, two distinct pipelines may run.

### 2.1 Musical modifier pipeline
Source:
- `CompositionCardPayload.modifierEffects`

ALWTTT meaning:
- these mutate the editable composition model used for upcoming playback/build output

### 2.2 Gameplay effect pipeline
Source:
- `CardPayload.effects`

ALWTTT meaning:
- these apply immediate gameplay/systemic consequences owned by normal card runtime execution

### 2.3 Non-negotiable rule
Musical modifier effects and gameplay effects must not be silently conflated.

- musical modifiers are **not** normal gameplay effects
- gameplay effects are **not** interpreted as music-model changes unless explicitly represented on the composition side

---

## 3. ALWTTT-owned runtime surfaces

### 3.1 CompositionSession
Owns the song-scoped runtime state machine for composition and playback progression.

CompositionSession exposes `AddCurrentInspiration(int delta) → int` as the canonical session-budget mutator. It clamps to `PersistentGameplayData.MaxInspiration`, refreshes the composition UI, mirrors the result to `PersistentGameplayData.CurrentInspiration` (closing the dual-siting drift documented in `SSoT_Dev_Mode §13.4` at the production-path level), and returns the actual delta applied post-clamp. Track-derived per-loop gain (`HandleLoopFinished`) and host-driven per-loop gain (M4.6F-3 `OnCompositionLoopFinished`) both route through this method.

CompositionSession also derives and owns the per-song render seed (`_songSeed`) consumed by every `RenderSinglePart` call for that song; see §10.

### 3.2 SongCompositionUI
Owns the editable song/part/track model that composition cards mutate.

### 3.3 SongConfigBuilder
Owns the ALWTTT-side conversion from editable model state into runtime playback/build input.

### 3.4 MidiMusicManager
Owns the game-side hosting/control of playback for the current scene/session.

### 3.5 Source-of-truth boundary (must stay explicit)
There are **two different truths on two different sides of the handoff**.

- **ALWTTT game-side editable/session truth** lives in `SongCompositionUI` + `CompositionSession`.
  - This is the model/session state the player is actively editing during the gig.
  - It is the authoritative ALWTTT-side truth **before** package handoff/build.
- **MidiGenPlay package-side runtime truth** lives in `SongConfig` + `SongConfigManager`.
  - This is the package runtime representation produced **after** ALWTTT has built and handed off playback input.
  - It is authoritative for package runtime behavior, not for ALWTTT gameplay/session ownership.

These are complementary, not competing, sources of truth.

---

## 4. Canonical live-composition data flow

### Step 1 — Player plays a Composition card
The player acts through the normal ALWTTT hand/controller surface.

### Step 2 — Runtime forwards to the active CompositionSession
The gig runtime validates that a song/session context exists and routes the card into that active session.

### Step 3 — Musical modifiers mutate the ALWTTT composition model
The session/UI layer applies the card's composition-owned data to the editable song model.

Examples of observable ALWTTT-side mutations include:
- track/role activation or change
- part structure changes
- tempo / meter / tonality / root-note style changes represented on the ALWTTT model side (**how an unset part field resolves** is §12)
- instrument/style selections authored through ALWTTT-facing card payloads

### Step 4 — Gameplay effects may also execute
If the card includes normal card effects, those may apply immediate systemic consequences without redefining the music-model mutation step.

### Step 5 — Runtime decides whether playback state must rebuild
If the card changes audible output, ALWTTT runtime triggers rebuild/invalidation behavior for the relevant upcoming playback state.

### Step 6 — SongConfigBuilder rebuilds playback input
The current editable ALWTTT model is transformed into the runtime build/playback configuration handed off to MidiGenPlay.

At this handoff point, authority changes layers:
- ALWTTT still owns the gameplay/session meaning of the change
- MidiGenPlay becomes the package-side runtime owner of the built `SongConfig` state

### Step 7 — MidiMusicManager hosts playback of the rebuilt state
Playback is started, resumed, or rebuilt through the game-owned music runtime surface.

### Step 8 — Loop/part/song feedback is emitted back to ALWTTT
The session produces feedback contexts used by ALWTTT scoring, hype, audience, and encounter logic.

---

## 5. Timing semantics at the ALWTTT-observable level

ALWTTT owns the **observable meaning** of timing on composition modifiers, even if package internals later realize the audio generation.

### 5.1 Immediate
Meaning:
- the card mutates the currently relevant composition model state now
- audible result depends on the current rebuild/playback point, but the model mutation is immediate

### 5.2 OnNextLoop
Meaning:
- the card is intended to affect the next loop boundary of the current relevant musical context
- ALWTTT runtime must ensure the necessary rebuild/invalidation path exists for the effect to become audible on the intended boundary

### 5.3 OnNextPartStart
Meaning:
- the card is intended to affect the next part boundary rather than the already-committed current part

Rule:
- ALWTTT documents the intended runtime boundary behavior here
- precise generator-side implementation details do not belong in this SSoT

### 5.4 Final-loop composition lock (CARD-UX-1, 2026-07-13)

**Routing fact (code truth).** Since D-D=β retired the NextPart gesture, *every* composition drop during a running loop is normalized to `CurrentPart` and applied to `_currentPartIndex` — the part that is **currently looping** — with that part's cache invalidated. The change therefore becomes audible on the **next loop of that same part** (the Pending-Effects model, `planning/Design_Pending_Effects_v1.md`). This routing was code truth and was written nowhere; it is the premise of the rule below.

**Rule.** On the **final** loop of a part there is no next loop to render the change, so the play is pure waste. `CompositionSession.IsFinalLoopRunning` is true when a loop is running (`BuildingNextPart` | `PlayingCurrentPart`) and `_loopsRemainingForPart == 1`. `TryPlayCompositionCard` denies the play **before any spend** (no inspiration, no ECON-1 budget), and the overlay surfaces it as `UnplayableReason.FinalLoopLock` (`SSoT_Card_System.md` §10.5). A presentation-avoidance mirror in `GigManager.TryPlayCompositionCard` keeps the drop from animating into a denial.

**Exemption — held loops.** `IsFinalLoopRunning` is **false while `TutorialLoopHoldGate.IsArmed`**: a held loop *replays*, so the pending change would in fact render on the repeat. `TutorialModalGate` is **not** exempt — modals suspend audience turns and dragging (TUT-R2b FIX-2), they do **not** replay the loop.

**Demo-cut consequence.** With parts-per-song = 1 and loops-per-part = 4 (`Design_Demo_Cut_v1.md` §1.1), the only final loop of a song is the loop the guided tutorial holds at beat 8. Inside the tutorial the lock is therefore structurally **unreachable**, and the composition gate at that moment is `TutorialInputGate.SingleCardOnly` (finisher-only, `Design_Tutorial_System_v0_2.md` §4), not this lock. The lock's live consumer is **non-tutorial play** (and any future multi-part song).

---

## 6. Audible-change vs non-audible-change behavior

### 6.1 Audible changes
If a played card changes what should be heard, ALWTTT runtime must treat it as a playback-affecting mutation.

Examples:
- track/part musical structure change
- tempo / meter / tonality / modulation change
- instrument/style change that changes playback output

### 6.2 Non-audible systemic changes
A Composition card may still apply immediate gameplay effects without requiring a music rebuild.

Rule:
- a card can be musically inert yet gameplay-active
- a card can be musically active and gameplay-active at the same time

---

## 7. TrackStyleBundle and package-facing references

Composition cards may reference `TrackStyleBundleSO`-style data as part of ALWTTT gameplay/runtime selection.

ALWTTT owns:
- the gameplay meaning of choosing that bundle via a card
- the fact that the selected bundle participates in the upcoming runtime build/playback

ALWTTT does **not** own:
- the package-internal precedence rules of every bundle field
- composer algorithms that interpret those bundles after the ALWTTT handoff

Those details belong to MidiGenPlay.

---

## 8. Runtime invariants

1. `CompositionSession` is the ALWTTT runtime host for song-scoped live composition.
2. `SongCompositionUI` is the ALWTTT-side editable truth for composition model state.
3. `SongConfigBuilder` is the canonical ALWTTT-side transformation step into playback/build input.
4. Playback-affecting composition cards must trigger the correct ALWTTT rebuild/invalidation path.
5. `MidiMusicManager` is documented as a game runtime integration component, not package-owned truth.
6. Loop/part/song feedback emitted after playback belongs to the ALWTTT runtime contract.
7. Per-loop card draw and per-loop inspiration consumption are host-owned (`GigManager.OnCompositionLoopFinished`), not inside `CompositionSession.HandleLoopFinished`. `CompositionSession` remains the deck-non-mutating invariant holder per the `[Obsolete]` guards on `CompositionSession.PrepareDeck` and `ICompositionContext.Deck`. The host hook fires synchronously from `CompositionSession.HandleLoopFinished`'s `LoopFinished?.Invoke(ctx)`, before the `_loopsRemainingForPart > 0` branch.
8. `CompositionSession.AddCurrentInspiration` is the canonical session-budget mutator. All production-path inspiration deltas during an active session route through it. It clamps to `pd.MaxInspiration` and mirrors to `pd.CurrentInspiration`. Dev-path mutation (`GigManager.DevSetInspiration` → `CompositionSession.DevSetCurrentInspiration`) is a parallel surface tracked separately in `SSoT_Dev_Mode §13`.
9. **Per-track stem persistence + session-level instrument continuity** (B1, 2026-05-12; re-keyed DBG-C1, 2026-07-17). `MidiMusicManager` maintains a per-song stem cache keyed on `(musicianId, role, trackInputsHash, partMeterHash)` where `trackInputsHash` is computed ALWTTT-side from UI `TrackEntry` fields (role + StyleBundle GUID + override-melodic/percussion-instrument GUIDs + override-instrument-type) and passed as the 5th parameter of `RenderSinglePart` (`trackInputsHashByTrack`, keyed by `MusicianTrackKey`). Resolved runtime fields (`tcfg.Instrument`, `tcfg.PercussionInstrument`) are NOT in the hash — they are randomized per render by `SongConfigBuilder.FromUI` for the no-override path. Stable instrument continuity across cache invalidations within a song is maintained by `CompositionSession`'s session-level pin maps (`_sessionMelodicPin`/`_sessionPercussionPin`) keyed on `"musicianId|role|override-state"` — a **superset** of `MusicianTrackKey` (the override-state dimension the composite key cannot carry), hence untouched by the re-key. Cards with explicit SO override (`overrideMelodicInstrument`/`overridePercussionInstrument`) skip the pin (deterministic by definition). Cards with type-override pin the random pick within the type. Reset semantics: stem cache and instrument pins both clear at song boundary in `Begin()`/`End()`. Boundary: `SongConfig` (MidiGenPlay-owned) is NOT modified to carry the hash; the parameter travels as a per-call argument per `SSoT_ALWTTT_MidiGenPlay_Boundary §3`.

   **Composite keying — multi-track carve-out retired (DBG-C1, 2026-07-17).** The stem cache, the part-cache instrument pin (`PartCache.resolvedMelInstByTrack`), the `instrumentOverrides` argument, and `ComputeTrackInputsHashesForPart` are all now keyed by `(musicianId, TrackRole)`, matching the package's re-keyed readback (`PartRender.stemsByMusician` / `melInstByMusician` / `resolvedByTrack`, keyed `MusicianTrackKey` since MGP-ALWTTT-DBG-1). A musician holding two role-tracks therefore yields **two independent cache identities**, one per role. The three BASS-1 degradations are **removed**: `ComputeTrackInputsHashesForPart` no longer omits multi-track musicians, `CompositionSession` no longer strips them from the override argument or the pin map, and `MidiMusicManager` no longer flattens keyed stems/instruments back to `musicianId`. Multi-track parts are now cacheable (ST-S2 PASS: `cacheEnabled=True` with per-role hashes). The any-track-without-hash guard survives only as a general integrity gate for legacy null-map callers. Single-track parts remain byte-identical to before (the only stem-key change is the added `:{role}` segment, deterministic; BC gate verified, ST-S1 PASS).

   **Read-only render truth surface (DBG-C1, 2026-07-17).** `MidiMusicManager` publishes the last part render's package readback for observability — `LastResolvedByTrack` (`IReadOnlyDictionary<MusicianTrackKey, ResolvedTrackChoice>`), `LastPinnedByTrack`, and `LastRenderSerial`/`PartIndex`/`Bpm`/`FromCache`. The serial bumps on **every** `RenderSinglePart` return, fresh render and bundle-cache replay alike; a replay republishes the **original** render's snapshot (bytes identical ⇒ choices identical, D-DBG5=A). It is truth-only — never an input to gameplay. `GetChordTimelineSnapshot()` returns a read-only per-channel snapshot of the parsed `chd:` chord timeline (public `ChordTimelineEntry` DTO), consuming the governed `chd:` marker contract (MGP `SSoT_Composer_Backing_Track §2.1`). Consumers: `DevCompositionDebugTab` (`SSoT_Dev_Mode §18`).

   **Dev pattern overrides — cache bypass (DBG-C2, 2026-07-17).** Under `#if ALWTTT_DEV`, `CompositionSession.DevPatternOverrides` (`MusicianTrackKey → PatternDataSO`) is passed as the trailing `patternOverrides` argument of `RenderSinglePart` when non-empty (null otherwise). These overrides are **deliberately not part of any cache key.** `MidiMusicManager` bypasses both the stem cache and the part-bundle cache for any render that supplies overrides (the same one-shot mechanism used for modulation transients), so an overridden render is always produced fresh and never pollutes a cached identity. `CompositionSession` carries a monotonic `DevOverrideStamp`; each `PartCache` entry records the stamp it was rendered under, and a mismatch at loop start invalidates the entry (keepTempo + keepInstruments) so the next loop re-renders with the current overrides. Clearing all overrides restores byte-identical un-overridden output (BC gate + clear/restore regression: ST-C2-7 / ST-C2-8 PASS). Production behavior is byte-identical: `patternOverrides` is null, the bypass predicate is false, and none of the stamp machinery exists in a non-`ALWTTT_DEV` build.

   **Dev instrument overrides — hash-participating, NOT bypassed (CSV-2, 2026-07-18, D-CSV-5=A).** The instrument counterpart of the paragraph above takes the **opposite** shape, and the asymmetry is deliberate. A dev instrument pick writes `TrackEntry.overrideMelodicInstrument` / `overridePercussionInstrument` directly — the same fields an `InstrumentEffect` card writes — so it participates in `trackInputsHash` exactly as a card override does (those GUIDs are already hash inputs, first paragraph of this invariant), the stem-cache identity moves with the change, and no bypass is needed or wanted: a dev-overridden render is a legitimately distinct cache identity, unlike a pattern override which is not. **No new dictionary, no new argument, no new production API** — `SongCompositionUI`, `SongConfigBuilder`, and `MidiMusicManager` are untouched by CSV-2.

   The one non-obvious consequence is the **invalidation shape**. `CompositionSession.DevInvalidateForInstrumentOverride(partIndex)` invalidates with `keepTempo: true, keepInstrumentsOverride: **false**` — mirroring the instrument-*card* path (`ShouldKeepInstruments` → `CompositionCardClassifier.IsInstrumentCard`), **not** the `DevOverrideStamp` pattern path, which preserves instruments. Preserving them here would be a live defect: `PartCache.resolvedMelInstByTrack` survives a `keepInstruments: true` invalidation and is passed back into the next `RenderSinglePart` call as `instrumentOverrides`, so the stale resolved voice would win over the new pick. The stamp is bumped as well, so the change lands at the next loop start through the normal seeded path.

   **Bytes-plane mix gain — hash-participating (BAL-1, 2026-07-22, D-BAL-3=A).** A per-`(musicianId, TrackRole)` **gain** (`MixGainProfileSO`, resolved at gig start, held on `MidiMusicManager` as `_gigMixGains`) is folded into `ComputeHashFromTrackEntry` via a trailing gain segment (`ComputeTrackInputsHashesForPart(..., mixGains)`, threaded from `CompositionSession` as `mm.GigMixGains`). Consequences: (a) the gain enters `trackInputsHash` regardless of its per-gig lifecycle, so a gain change re-keys the stem/bundle cache and replay can never serve stale CC7; (b) the per-part render call now carries `mixGains: _gigMixGains` into `GenerateSinglePart`, and the package emits one CC7 per gained melodic track (null/empty map ⇒ byte-identical, the emission-gate identity guarantee); (c) the hash *value* format gains a trailing `|_` segment for **every** track (ungained included) — harmless, caches are session-scoped. Contract + law: `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.3; ALWTTT model: `SSoT_Audio.md` §4.6. The live-playback plane is unaffected (separate plane; composed at the `WriteChannelVolume01` boundary, §4.2 of the Audio SSoT).

   **Shared-harmony identity — hash-participating (SOLO-1 / ORDER-1 adoption, R2c + R2d, 2026-07-30/31, D-R2-6=B, D-R2-10=A).** The progression a part renders over is baked into every harmony consumer's bytes, so every cache identity derived from `partMeterHash` — stem keys, `partBundleKey`, and the per-part invalidation sweep — must move when that harmony moves. `MidiMusicManager.RenderSinglePart` appends up to two segments, **both computed before the render**: `dp:{paletteAsset}:{seed}:{partIndex}` when a host default palette is assigned (constant within a song, since seed and partIndex are fixed — it never churns mid-song), and `bk:{trackInputsHash of the Backing row}` when the part has one. **Why not the package readback:** MidiGenPlay's `PartRender.sharedProgressionSource` reports which source actually won, and the natural-seeming rule "append `dp:` only when the source is `HostDefault`" is **not implementable** — the readback is produced *by* the render, and this hash decides *whether* a render happens. Circular. The readback is therefore a verification surface (`LastSharedProgressionSource` / `LastSharedProgressionAssetName`, republished on bundle replay per D-DBG5=A, asserted by ST-R2d-1/2), never a key input; a comment in the code says so, because the "improvement" is tempting and cannot work. **The `bk:` segment closes F-HARM-STALE-1, a latent defect since B1 and unrelated to SOLO-1:** swapping the Backing card (Wormus Major → Minor) changes the *Backing* track's `trackInputsHash` but not the bass's, and previously not `partMeterHash` either — so the bass stem was served from cache with the **old chords baked in**. Silent wrong output rather than silence, which is why it survived undetected; the F-BASS-ORDER-1 investigation surfaced it. Fix verified by ST-R2d-3. **Accepted cost:** over-invalidation — swapping to an articulation-only Backing card re-renders the harmony consumers even though the effective harmony (the default) did not change. One extra render, song-scoped; strictly preferable to serving stale bytes. **BC:** both segments absent ⇒ hash string byte-identical to pre-R2c (ST-R2d-4 PASS). **Recorded limitation, not a defect:** `dp:` identifies the palette *asset*, seed and part — not the palette's contents, so editing its entries or weights mid-session does not invalidate. Harmless while these caches are per-song (D7=B).

   **Clear/restore is byte-identical (ST-CSV-3 PASS)** and the mechanism is the session pin map, not a saved render: `BuildMelodicPinKey`/`BuildPercussionPinKey` return null while an explicit override is set, so the overridden track's pin is **skipped rather than overwritten** and the pre-override voice survives in `_sessionMelodicPin`/`_sessionPercussionPin` to be re-applied when the override clears. **Card supersession is expected behavior:** `ApplyInstrumentEffect` unconditionally rewrites the three override fields, so a later instrument card takes the field back from the dev tool; the dev surface detects and reports this rather than fighting it (`SSoT_Dev_Mode §18.9`). Production behavior is unchanged: the fields exist in production and are written only by cards; the dev write path and its invalidation helper do not exist in a non-`ALWTTT_DEV` build (ST-CSV-8 PASS).

   **Resolved-identity read surface + audition economy exclusion (CSV-3, 2026-07-22, dev-only).** `MidiMusicManager` publishes `LastRenderResolvedTimeSignature` / `LastRenderResolvedTonality` / `LastRenderResolvedRootNote`, read from the `PartConfig` after generation (post `ChordTrack` step-2b alignment) and mirrored into the bundle-cache entry so replays republish the original truth (D-DBG5=A). `CompositionSession.DevInjectCompositionCard` applies a catalogue card's **musical side only**; injected tracks are held in a dev-only `_devInjectedTrackKeys` set and excluded from `EvalPerLoopInsp` (D-CSV-24=B), so audition is economy-neutral (a genuine play on the same key reclaims it; the set clears at song boundary). Both surfaces are `#if ALWTTT_DEV`; production is byte-identical.

10. **Track identity is the pair `(musicianId, role)`, not `musicianId`** (BASS-1, 2026-07-12). One musician may hold several role-tracks in the same part. Same-role card ⇒ replace; different-role card ⇒ add. Full contract in §11.

11. **A composition play is denied on the final loop of a part** (CARD-UX-1, 2026-07-13) — no subsequent loop of that part would render it, since every drop during a running loop routes to the currently looping part and becomes audible on that part's *next* loop. Exception: while a tutorial loop-hold is armed (`TutorialLoopHoldGate.IsArmed`), the held loop replays and the change does render, so the lock lifts. Denial occurs **before any inspiration or ECON-1 spend**. Full contract in §5.4. **Dev exemption (DBG-C1, 2026-07-17):** under the `#if ALWTTT_DEV` infinite composition-loop toggle (`SSoT_Dev_Mode §18`) a next render always exists, so `IsFinalLoopRunning` returns false and the deny does not apply; the production predicate is byte-identical.

12. **Singer voice seam (SINGER-1, 2026-07-21).** `CompositionSession` raises
    `event Action<ALWTTT.Music.Voice.SingerLoopContext> LoopPlaybackStarting`
    immediately before `MidiMusicManager.PlayRaw`, on **every** path through
    `PlaySinglePartLoop` (first loop, `HandleLoopFinished` replay, part advance,
    tutorial hold). The payload carries `partIndex`, the loop's `stemsByTrack`
    (`MusicianTrackKey → byte[]`, taken from `PartCache.stemsByTrack`), and the
    part's musical context (`tonality`, `rootNote`, `timeSignature`, `bpm`,
    `seconds`). Subscriber exceptions are caught and logged so a subscriber fault
    cannot kill the loop. This is the **only** `CompositionSession` edit made for
    the singer subsystem; `MidiMusicManager` is unchanged. The singer arms from a
    musician's `Melody`/`Lead` stem, anchors on `IPlayMidi.OnSongStarted` via
    `AudioSettings.dspTime`, and mutes that channel through the existing
    `SetChannelVolume` mix path. Full authority: `systems/SSoT_Singer_Voice.md`.
    Coexistence with GM MIDI is by construction — an unmatched melody stem plays
    normally (ST-V8). Boundary: no `SongConfig`/package change; the stem the
    singer consumes is the one invariant 9 already produces.

---

13. **Resolución y cacheo consumer-side del BPM de parte (recorded CTX-2a,
    2026-08-03).** El BPM es resuelto por ALWTTT, no por el paquete.
    `MidiMusicManager.RenderSinglePart` aplica esta precedencia, en este orden:
    (a) `bpmOverride` entrante gana sobre todo lo demás; si no hay,
    (b) `part.ExplicitBpm` (← `PartEntry.absoluteBpmOverride`) si tiene valor;
    (c) en su defecto `MusicTheory.GetBPMFromRange(part.TempoRange,
    TempoRule.MultiplesOfTen)`. Sobre el resultado de (b) o (c) se aplica
    `part.TempoScale` como **factor multiplicativo** cuando difiere de 1, con
    suelo de seguridad 40 BPM. Corolario: **`ExplicitBpm` ensombrece
    `TempoRange`** — mientras el primero tenga valor, el segundo es inerte, y
    cualquier herramienta que escriba `ExplicitBpm` debe devolverlo a `null`
    para restaurar la semántica de rango.

    **El BPM se resuelve una vez por parte y se cachea.**
    `CompositionSession.PlaySinglePartLoop` pasa `cache.resolvedBpm` como
    `bpmOverride` (rama (a)) siempre que sea `> 0`, de modo que la re-resolución
    **no se ejecuta** en los loops siguientes de esa parte. La invalidación de
    caché es selectiva: `InvalidatePartCache(partIndex, keepTempo)` preserva
    `resolvedBpm` cuando `keepTempo` es `true`, y `ShouldKeepTempo` devuelve
    `false` **solo** para cartas clasificadas como de tempo
    (`CompositionCardClassifier.IsTempoCard`). Las cartas de compás, tonalidad e
    instrumento, y todas las invalidaciones de Dev Mode, mantienen el BPM.
    Consecuencia operativa: un cambio del intent de tempo del modelo **no es
    audible** hasta que algo invalide con `keepTempo: false` o ponga
    `resolvedBpm = 0`.

    **Dispersión entre partes.** Como cada parte resuelve por separado, dos
    partes marcadas con el mismo `TempoRange` pueden sonar a BPM distintos. Que
    `GetBPMFromRange` sortee dentro de la banda es **INFERIDO**, no confirmado:
    la función es interna de MidiGenPlay y el espejo `MGP-20260729_*` está
    declarado obsoleto y no es autoridad. Verificar en el proyecto MidiGenPlay
    real antes de tratarlo como hecho. La magnitud de la dispersión es el dato
    que decide **D11b** (`CURRENT_STATE.md` §4), medición diferida a post-R3.

    **Boundary:** ninguna de estas reglas es package-owned. El paquete recibe un
    entero de BPM ya resuelto; la política de resolución, cacheo e invalidación
    es ALWTTT.

## 9. Out-of-scope package internals

The following are intentionally **not** governed here:
- composer-internal precedence chains
- package repository loading rules
- package authoring/editor internals
- package-side TS normalization algorithms
- how the package turns a given seed into a palette-entry pick (selection-mechanism internals)

Those belong in MidiGenPlay docs. ALWTTT's per-song render-**seed policy** — when a new seed is drawn and what it must never depend on — is ALWTTT truth and is governed in §10, not here.

---

## 10. Per-song render seed

`CompositionSession` derives a single per-song render seed (`_songSeed`) once per song, in `Begin()`, from run entropy (`unchecked((int)DateTime.UtcNow.Ticks)`). The value is logged as `[Session] SongSeed=<n>` (musical-bug repro path) and passed on every `mm.RenderSinglePart(..., seedOverride: _songSeed)` call for that song. `_songSeed` is cleared in `End()`.

**Guaranteed properties.**
- **Intra-song stability** — the same seed drives the same pick across re-renders caused by cache invalidation within the same song (smoke-tested `ST-S5gb-2`).
- **Cross-song variety** — a fresh seed is drawn per song (smoke-tested `ST-S5gb-1`).

This deliberately replaces the accidental stability previously produced by the package's constant `defaultSeed`.

**Prohibition.** The seed must never be derived from anything that changes between re-renders of the same song (a per-render clock read, a per-render counter). Doing so would break intra-song stability.

**Cache key unaffected (D-S5gb-1=A).** `trackInputsHash` (invariant 9, §8) keeps its existing meaning — player-controlled inputs only. (DBG-C1 added a `role` segment to the cache *key*, not to the hash value; the hash's meaning is unchanged.) Cross-song isolation is guaranteed by the stem-cache/pin clear in `Begin()`/`End()` already documented in invariant 9; that claim is now **verified at runtime** by `ST-S5gb-3` (2026-07-05), moving invariant 9 from documented truth to observed truth for the cross-song-isolation portion of its claim. Documented fallback if this isolation is ever found to fail: fold the seed into the cache key itself (pattern `MOD-DIR-3`).

**Package contract consumed, not redefined.** `GenerateSinglePart(..., int? seedOverride = null)`; the package resolves `baseSeed = seedOverride ?? settings.defaultSeed` once per render. `seedOverride: null` is bit-identical to pre-adoption behavior. Authority for the package-side mechanism: MidiGenPlay orchestration SSoT §5.1 (cross-project reference, read-only — not redefined here, per `SSoT_ALWTTT_MidiGenPlay_Boundary.md`).

**Dev override.** A dev-only pin surface (`CompositionSession.DevPinnedSongSeed`) exists for reproducible songs; see `SSoT_Dev_Mode.md §8.7`.

---

## 11. Multi-role tracks per musician (BASS-1, 2026-07-12)

**Invariant.** A track in `SongCompositionUI`'s editable model is identified by the pair **`(musicianId, role)`**, not by `musicianId` alone. One musician may hold several role-tracks in the same part (e.g. Backing + Melody + Bassline).

**Card semantics that follow.** `SongCompositionUI.TryAddOrReplaceTrackOnPart` matches on `t.musicianId == musicianId && t.role == role`:

- a Track card whose role **matches** an existing track of that musician **replaces** that role's track (the old retarget behavior, now correctly scoped);
- a Track card whose role **differs** **adds** a new track alongside.

**Why this was a live content bug, not only a bass blocker.** The previous musician-only lookup *retargeted* the musician's single track. A fixed-performer composition card ignores hover and always resolves onto its own musician (`HandController`; `CompositionSession` `ResolveMusicianByType`). Sibi's starter Backing card (Wormus) followed by her Melody card (Singing Field) — both `FixedPerformerType: Sibi` — therefore converted her Backing track into a Melody track, removing the song's harmony and breaking the shared-progression mechanic the starter deck is built on (`planning/Design_Starter_Deck_v1.md` §5.13). Fixed and verified (ST-BASS-9).

**Bundle-less Track cards do not create tracks (D4=A).** A Track card with `trackAction.styleBundle == null` — a PartEffect carrier such as Key Lift, Push It, or Half Time — augments the matching-role track **if one exists**, and otherwise applies only its part effect. It never mints an empty role-track: a role-track with no bundle renders as "no card" in the composer anyway, so creating one only pollutes the model and the UI. The authoring consequence is normative in `SSoT_Card_Authoring_Contracts.md` §5.14.

**Channels are per-track, not per-musician.** `MidiMusicManager` stamps `TrackConfig.Channel` from a `(musicianId, role) → channel` lookup built over the index-parallel `ChannelMusicianOrder` / `ChannelRoles` / `channelMap` lists, with a first-wins musician-only fallback for tracks whose role was absent from the Part-0 seed. A musician may therefore own several channels, and `PlaySameArrangementSubsetByMusicians` unmutes **every** channel an entrance-ordered musician owns. Budget: 15 melodic channels per part (ch 9 reserved for drums); `SongConfigBuilder` logs a warning past that.

**Instrument overrides on a multi-track musician (D2=A).** An `InstrumentEffect` with `scope = TrackOnly` applies to **every** track of the target musician whose instrument family matches the effect mode — melodic modes (`SpecificMelodic`, `InstrumentType`, `RandomFromList`) to non-Rhythm tracks, `SpecificPercussion` to Rhythm tracks. Deterministic in the sense that matters: which tracks are affected never depends on chance. A role-filtered variant is the natural extension if a card ever needs "change only the bass sound".

**`RandomFromList` — pick-once-then-persist (R2c, 2026-07-30, D-R2-7).** A fourth mode, appended to `InstrumentEffect.InstrumentTargetMode` (append-only; the serialized ints of the existing three are unchanged). It carries `List<MIDIInstrumentSO> melodicInstrumentPool` and exists because the previous surface forced a choice between one exact patch and a whole `InstrumentType` family — with no way to express "either of these two slap basses".

The pick is resolved **once per card application**, in `SongCompositionUI.ApplyInstrumentEffect` *before* the track loop, so a multi-track musician receives **one coherent instrument** rather than a per-track roll (same fiction as the other modes: hand this musician a new instrument). The chosen SO is written to `TrackEntry.overrideMelodicInstrument` — it **persists as an ordinary specific override** and is indistinguishable from `SpecificMelodic` downstream. Three consequences follow with no new machinery: the pick enters `trackInputsHash` through the existing `overrideMelodicInstrument` GUID segment (invariant 9), so cache identity moves with it; re-renders and cache replays reproduce the same instrument; and the dev-override interaction, invalidation shape and card-supersession rules of invariant 9 apply unchanged.

**RNG choice (deliberate).** `UnityEngine.Random`, **not** the per-song render seed. The roll happens at a player-driven card play, whose ordering is not reproducible from the seed in the first place; binding it to `_songSeed` would imply a reproducibility that does not exist. Uniform over the pool in v1; a weighted variant would mirror `ChordProgressionPaletteSO.WeightedEntry` if content ever needs it (registered, not scheduled).

**Degradation.** Null entries are filtered; an **empty** pool applies nothing and logs a warning (early return before any field is cleared) rather than silently wiping the track's existing override. Verified ST-R2c-5 (both pool members observed across applications; hash moves with the pick) and ST-R2c-6 (`SpecificMelodic` regression identical).

**First production consumer:** `InstrumentEffect_SlapBass`, pool `{Slap Bass 1, Slap Bass 2}` (D-R2-3).

**Pending visualization stays per-musician (accepted).** All of a musician's rows flag pending together. Conservative-correct: a re-render refreshes the whole part.

**Cache and pin degradation:** ~~see the multi-track carve-out in invariant 9 (§8).~~ Retired by DBG-C1 (2026-07-17) — multi-track musicians are cacheable under composite `(musicianId, role)` keying; see invariant 9 (§8).

---

## 12. Part field resolution — meter and tonality sources (recorded 2026-07-20 CSV-4; CLOSED 2026-07-22 CSV-3)

Recorded here because part-field resolution is ALWTTT-side model construction and had no
documented home. The finding that opened this section is **CLOSED as not-a-bug** (CSV-3,
2026-07-22); the resolution rules below are now ratified, and ownership is assigned by
**D-MEL-1=A**.

### 12.1 Part meter resolution — the model-construction 4/4 default

The `partEntry?.timeSignature ?? default` expression previously cited here is in the
**`LoopFeedbackContext` (audience) path, not the render path** — a location correction to
the CSV-4 recording. The **render** path copies `PartEntry.timeSignature` directly
(`SongConfigBuilder.FromUI`), and that field is explicitly initialized `FourFour` at every
part-creation site and mutated **only** by a `MeterEffect`.

So a part whose meter was never explicitly set renders 4/4 because that is its
**model-construction default**, not because of a silent `?? default` fallback on the render
path. **D-MEL-1=A** resolves ownership: a rhythm card that presents a non-4/4 identity must
carry a matching `MeterEffect` (Pentameter precedent). Authoring rule:
`systems/SSoT_Card_Authoring_Contracts.md` §5.16.

### 12.2 Two independent sources feed melody tonality

`MelodyTrackComposer` derives its scale from `part.Tonality` / `part.RootNote`, while the
harmonic context per span comes from the **loaded progression's chord events** — two
independent sources that can diverge. **With tonalities authored on the progression,
`ChordTrack` step-2b aligns the part** (verified: Core Minor → Aeolian, ST-CSV3-6).

This is a **read of package-side code, recorded for boundary clarity only**. The composer
is package-owned and is **not modified by ALWTTT**
(`SSoT_ALWTTT_MidiGenPlay_Boundary.md` §2.2).

### 12.3 Verdict — meter collision by construction (not-a-bug)

The 2026-07-20 observation (rhythm 6/8 + Core Minor + Singing Field: the melody follows
neither the part's meter nor its scale) was a **meter collision by construction**: Core
Minor holds **zero** 6/8 progressions, so that combination cannot render coherently. Runs
A/B showed **no divergence**, and ST-CSV3-6 confirmed C2a healthy. The engine is meter- and
tonality-consistent when content is authored correctly. **Finding CLOSED as not-a-bug; no
package ask filed** (`SSoT_ALWTTT_MidiGenPlay_Boundary.md` §4.3). Evidence classes stay
distinct: the closure rests on validated observed behavior (runs A/B), not on inference.

### 12.4 Part tempo default — model-construction `Slow` (D11=A, CTX-2a 2026-08-03)

La construcción del modelo fija el tempo de parte igual que fija el compás 4/4
(§12.1): en la declaración del campo y en cada sitio de construcción de
`PartEntry`. Desde CTX-2a el default es **`TempoRange.Slow`** (etiqueta `"Slow"`),
antes `TempoRange.Fast` / `"Very Fast"`.

**Por qué está aquí y no en Dev Mode.** Es comportamiento de producción: la
carta de modo por defecto no fija tempo, así que el default del modelo es
literalmente lo que suena en el demo. Con el anterior, los patrones de
percusión de 8 compases eran ilegibles (F-TEMPO-1) y la arquitectura 3+1 no se
percibía.

**Sitios afectados:** la declaración de `PartEntry.tempo` /
`.tempoRangeOverride`, `EnsurePartAt` y el fallback de `CreateNextDraftPart` en
`SongCompositionUI`. Intro / Outro / Solo heredan la **etiqueta** `tempo` de la
parte vecina y toman `tempoRangeOverride` del default de campo — desalineación
latente si una carta cambió el rango de la parte origen; preexistente, no
introducida por D11, no corregida aquí.

**Abierto:** cuánto más rápido debería ser `Slow` (**D11b**), y si el problema
dominante es la posición de la banda o su dispersión (§8 inv 13). Se decide con
medición, diferida a post-R3.

---

## 13. JAM — shared-harmony continuity (R3, 2026-08-08)

### 13.1 JAM-1 — the jam keeps its harmony

**Readback consumed.** `MidiMusicManager.LastSharedProgressionData` is the third readback
alongside `LastSharedProgressionSource` / `…AssetName`. It is published on a fresh render
**and** on bundle replay, and is stored on `PartBundleCacheEntry` — same D-DBG5=A logic as
its two siblings, and for the same reason: a cache replay must republish the same truth a
fresh render would, or the readback silently means "whatever the last uncached render
happened to be".

**Runtime clone, session lifetime.** The stored progression is a runtime clone: never an
asset, never serialized, does not survive a domain reload. `_jamProgressionByPart` is
cleared in `Begin()` / `End()` together with `_partCache`.

**Imposition policy (D-R3C-2=A).** Impose the stored progression on the Backing track key
**unless** either: the tonality moved since capture, **or** the Backing bundle carries
`adoptProgressionTonality`. Two conditions, and between them the coverage is complete —
pre-render mutations of the model are caught by the snapshot comparison, and compose-time
adoption is caught by the flag. The design rule behind it: **the card that moves the key
wins**; a stored jam harmony is never allowed to override a deliberate tonal move.

**Capture policy (D-R3C-4=B′).** Capture *after* the render, and skip — with a `Remove` —
when `LastSharedProgressionSource == CardOverride`. Capturing a Backing card's own
`CardOverride` would pin the part to the card that just played, which is not a jam; it is
the card repeating itself.

**Cache interaction — correcting the record.** `patternOverrides` does **not** need a
cache-key segment. `MidiMusicManager` bypasses the stem and bundle caches entirely while
that map is non-empty (D-C2-4=A, `MidiMusicManager.cs:961`). Accepted cost, scoped to the
renders that actually impose.

**Dev override precedence.** Dev pattern overrides win over JAM-1, and are copied into a
fresh map — never aliased to the static `DevPatternOverrides`.

**Known limit O3.** The Backing row is selected with `FirstOrDefault` over Backing-role
tracks. With two Backing tracks in one part the choice is arbitrary. Unreachable with
current content; recorded, not fixed.

### 13.2 JAM-2 — mode travels with the harmony

**Problem.** JAM-1 captured the progression clone alongside a tonality snapshot taken from
the **UI model**. Adoption never reaches the model (§13.3), so a Lydian progression was
stored labelled "Ionian". On imposition the Backing rendered the authored chords (package
policy `AsAuthored`) while every other track in the part was generated against the model's
scale — e.g. B major (D♯) over an A Ionian scale (D natural). Not reachable in R3 content,
because no part carried both a modal Backing and a melody; reachable as soon as melody
cards land on modal parts.

**Fix.** A second per-part map, `_jamRenderedTonalityByPart`, storing the tonality and root
the captured harmony **actually rendered in**. On imposition that tonality is written onto
`cfg.Parts[partIndex]` before `RenderSinglePart`.

- **Two maps, deliberately.** `_jamTonalitySnapByPart` tracks the **model** and answers
  *"did the player move the key since capture?"* — it is the guard that decides whether to
  impose. `_jamRenderedTonalityByPart` tracks the **render** and answers *"what mode did
  these chords sound in?"* — it is the payload to propagate. They diverge precisely when a
  Backing card adopts. Collapsing them into one field would make the guard compare a mode
  against itself and stop detecting real tonality moves, violating D-R3C-2=A. Verified by
  ST-J3.
- **No new package surface.** Adoption mutates the per-render `PartConfig` **in place**
  during compose, and that object lives inside the host-built `cfg`, still in hand after
  `RenderSinglePart` returns. `MidiMusicManager.LastRenderResolvedTonality` exists but is
  `#if ALWTTT_DEV`; reading `cfg` works in release.
- **Scope is the render, not the model.** `BuildSongConfigFromUI` rebuilds `cfg` every
  loop. D-R3C-3=A′ holds: the mode lives exactly as long as the jam entry.
- **Self-consistent under repetition.** After alignment the render is in the adopted mode,
  so the subsequent capture stores that same mode. The part stays coherently modal for the
  life of the card chain, with no drift.
- **Eviction.** The new map is cleared in `Begin()` / `End()` and `Remove`d on the B′
  non-capturable path, alongside its two siblings.
- **Log surface.** `[JAM-2] part=N aligning render tonality X/root -> Y/root (mode of the
  imposed harmony)`. Absent on renders that do not impose (ST-J6).

**Correcting the record on capture policy (B′).** Imposed harmony is republished by the
package as source `RenderOverride`, which **is** capturable, so an imposing part re-pins
itself every render. This is intended — it is what keeps the jam alive across a chain of
articulation-only cards — but it means an imposing part keeps the stem/bundle cache
bypassed for as long as the chain lasts. Accepted cost, unchanged from D-C2-4=A. Practical
consequence for testing: **any determinism test must start from a fresh song.**

### 13.3 Known limit — the audience seam is not aligned (→ D-R4-1)

`LoopFeedbackContext` is built from the UI model, so audience taste evaluation reads the
**authored** tonality, not the **sounding** one. Under modal harmony the audience sees
Ionian. This is a design question — should the crowd judge what is written or what is
played? — **not a defect**, and out of JAM-2's scope. Registered as **D-R4-1**
(`CURRENT_STATE.md` §4).

# Design_Composition_Debug_Tab — ALWTTT

**Status:** Draft requirements (planning-only; not an SSoT, not implementation authority)
**Mode:** DOCUMENTATION / GAME-DESIGN tooling requirement
**Scope:** ALWTTT-side requirements for a Dev Mode tab that surfaces per-track composition/generation information at runtime, and (later) lets a developer drive composition-only playback for sound debugging.
**Owns:** the ALWTTT-side requirement set, the display format, the Dev-Mode integration surface, and the ALWTTT/MidiGenPlay ownership split for this feature.
**Does not own:** any MidiGenPlay internal (pattern selection, progression resolution, the string→progression parser, phrase archetypes). Those are requested via the companion handoff, not defined here.

---

## 1. Purpose

Composition cards change how a gig *sounds*, but today the developer has no in-game readout of **what a played card actually resolved to** — which pattern, which progression, which instrument, at what tempo. Debugging a card that "sounds wrong" means reading Console `Debug.Log` spew or guessing.

This feature adds a Dev-Mode surface that makes the generation fingerprint of each track legible during a live gig, and eventually lets a developer audition composition changes directly (play any catalogue card's musical side, force a specific pattern, type a chord progression) without authoring cards or spending gameplay resources.

The feature is **staged**. R1 (log) is the immediate target and is mostly achievable with existing game-side data plus one bounded package readback. R2a–R2c are eventual, each with a distinct dependency profile.

---

## 2. Relationship to existing work

- **Dev Mode overlay** (`SSoT_Dev_Mode.md`): this is a new tab in the existing `DevModeController` F12 overlay, alongside `Infinite / Catalogue / Stats / Audio Mix`. It follows the established tab pattern (`internal static class Dev*Tab { public static void Draw() }`, `#if ALWTTT_DEV`).
- **M1.5 Dev Mode sub-roadmap, Phase 5** already reserves *"Composition card live injection"* (`CompositionSession.DevInjectCompositionCard(CardDefinition)` + a "Composition" overlay tab). **R2a below is the concrete spec of that reserved phase**, with the added constraint that the debug play path applies the *composition side only* (no gameplay effects, no resource spend). R1 is a prerequisite readout that Phase 5 did not previously call out.
- **MidiGenPlay boundary** (`SSoT_ALWTTT_MidiGenPlay_Boundary.md §4.3`): the package-side requests this feature generates should be recorded there next to the existing BASS-1 request, once confirmed. This doc does not edit that SSoT; see §9.

---

## 3. Requirement R1 — Per-track composition log (immediate target)

### 3.1 What it shows

For the current part, one line per track, in the format agreed in design discussion:

```
[Rhythm]  "Jazzy Drums"   | 3/4  | Room Kit    | palette: JazzBrushes (picked: pattern#2) | 142 bpm *
[Backing] "Dreamy Organ"  | Cmaj Ionian        | prog: I–iv–V7–Imaj7 → C–Fm–G7–Cmaj7 | expr: SkaStrum *
[Melody]  "Pentatonic Lead" | Cmaj Ionian      | archetype: RisingCall | inst: Nylon Guitar *
```

Convention:
- **No asterisk** = handoff intent, known the instant the card is played (game-side model truth).
- **`*` (asterisk)** = resolved value, known only after the render completes for that part.
- Empty `picked:` / `prog:` / `archetype:` fields render as `—` until the corresponding package readback exists (R1 ships partial and fills in as the readback lands).

The line content is **role-adaptive**: rhythm shows kit + drum pattern, backing shows tonality + progression + expression, melody shows archetype + instrument. A `GenerationDebugFormatter` (ALWTTT-side, static, plain-string) formats per role; a `Compact / Full` verbosity flag lives on `GigDevSettingsSO`. No per-asset template system — the role switch plus the existing `TrackActionDescriptor` / `PartActionDescriptor` field-relevance is sufficient.

### 3.2 Bifásico timing

The display updates in two phases against an existing hook: `CompositionSession` already calls `_ctx.CompositionUI.OnRenderCompleted(partIndex)` when a fresh render succeeds. That is the point where `*` fields are refreshed with resolved truth. Handoff-intent fields populate immediately on card play from the `SongCompositionUI` model.

### 3.3 Ownership map — the crux of R1

Categories: **[code]** = confirmed by reading ALWTTT source; **[obs]** = observed available; **[MGP]** = requires MidiGenPlay; **[MGP?]** = requires MidiGenPlay clarification.

| # | Field | Owner | Available now | Source / mechanism |
|---|-------|-------|---------------|--------------------|
| 1 | Track role | ALWTTT [code] | Yes | `SongCompositionUI.TrackEntry.role` |
| 2 | Style bundle name | ALWTTT [code] | Yes | `TrackEntry.styleBundle.name` |
| 3 | Instrument override (SO / type) | ALWTTT [code] | Yes | `TrackEntry.overrideMelodicInstrument / overridePercussionInstrument / overrideInstrumentType` |
| 4 | Time signature / tonality / root | ALWTTT [code] | Yes | `PartEntry.timeSignature / tonality / rootNote` |
| 5 | Tempo range / scale / explicit bpm | ALWTTT [code] | Yes | `PartEntry.tempoRangeOverride / tempoScale / absoluteBpmOverride` |
| 6 | **Resolved BPM** | ALWTTT [code] | Yes (post-render) | `PartCache.resolvedBpm`, surfaced via `OnPartBpmResolved` |
| 7 | **Resolved melodic instrument** | ALWTTT via MGP readback [code] | Yes (post-render) | `RenderSinglePart` return tuple `pinned` (`Dictionary<musicianId, MIDIInstrumentSO>`). **BASS-1 carve-out:** multi-track musicians have no reliable per-role readback (last-role-wins); see §3.4 |
| 8 | Resolved percussion instrument | ALWTTT [code] | Yes | Read from built `SongConfig` `TrackConfig.PercussionInstrument` (ALWTTT picks it in `SongConfigBuilder.FromUI`) |
| 9 | **Resolved chord progression (Roman + symbol, per chord)** | ALWTTT via in-MIDI tags [code] | In-process now; needs MMM accessor | `chd:<ch>:<roman>:<sym>:<deg>:<quality>` tags parsed into `MidiMusicManager._chordTimelineByChannel` (private). Needs a public read accessor; MGP to confirm the tag contract (§ handoff A2) |
| 10 | Resolved progression / palette **asset identity** ("picked: JazzBrushes") | MGP [MGP] | **No** | No exposure observed in the ALWTTT-visible render return. Requires readback extension |
| 11 | Resolved **rhythm pattern identity** (palette pick) | MGP [MGP] | **No** | Resolved inside `RhythmTrackComposer`; not returned. Requires readback extension |
| 12 | Resolved **melody phrase archetype** | MGP [MGP] | **No** | Resolved package-side; not returned. Requires readback extension |
| 13 | Resolved **bass pattern identity** | MGP [MGP] | **No** | Same as 11/12 for bass tracks |
| 14 | **Chord expression** (ska strum, etc.) | MGP [MGP?] | Unknown | Need MGP to say whether expression is a deterministic field of the style bundle (ALWTTT reads it from the asset directly) or resolved/randomized in the composer (needs readback) |

**R1 conclusion:** rows 1–8 are shippable ALWTTT-side today. Row 9 is available in-process and needs only an accessor. Rows 10–14 are the genuine MidiGenPlay dependency and are the core of the companion handoff.

### 3.4 BASS-1 interaction

The resolved-instrument readback and any resolved-pattern readback are keyed by `musicianId` alone, which cannot represent a musician holding two role-tracks in one part (Melody + Bassline). This is the same limitation recorded in `SSoT_ALWTTT_MidiGenPlay_Boundary §4.3` (BASS-1). The handoff therefore asks MidiGenPlay to key **all** new resolved-choice readbacks by `(musicianId, TrackRole)`, folding this feature's need into the already-open BASS-1 re-key request rather than opening a parallel one.

---

## 4. Requirement R2a — Debug-play any catalogue card (composition-only)

### 4.1 Intent

From a dropdown sourced from the already-loaded card catalogue (the band-union source used by `DevCardCatalogueTab` since DEMO-FIXES-A, with `GameplayData.AllCardsList` fallback), let a developer "play" any card's **musical side only** to audition its sound — no gameplay effects, no resource cost. Essentially: apply the card's `TrackStyle` / composition payload to the model and trigger the render, and nothing else.

### 4.2 Ownership

Almost entirely **ALWTTT-owned**. The two-pipeline separation is already contractual (`SSoT_Runtime_CompositionSession_Integration §2`): musical modifiers (`CompositionCardPayload.modifierEffects`) are distinct from gameplay effects (`CardPayload.effects`). A debug-play path applies only the former. This is the concrete form of the reserved `CompositionSession.DevInjectCompositionCard` (M1.5 Phase 5), with the "composition-only, no cost/effects" constraint made explicit.

**No new MidiGenPlay API is required** — `RenderSinglePart` already renders a track from a mutated model. MGP need only be *aware*; there is no package ask here.

### 4.3 Open ALWTTT design point
Whether debug-play mutates the live session model (visible, persistent for the loop) or a throwaway shadow model (audition without committing). Deferred to implementation; noted so it is not decided by accident.

---

## 5. Requirement R2b — Force a specific pattern per track (style override)

### 5.1 Intent

Choose an exact pattern for a track (rhythm / backing-chord / bassline / melody) from a populated dropdown, overriding whatever the style bundle would have resolved. Ideally the dropdown is populated from a runtime catalogue of existing patterns, filtered per role.

### 5.2 Ownership — two MGP asks

Patterns are MidiGenPlay-owned data types (`DrumPatternData` / `DrumPatternPaletteSO`, `ChordProgressionData` / `ChordProgressionPaletteSO`, melody `PhraseArchetypeSO`, bass pattern type TBD). Two things are needed from MGP:

- **Enumeration (handoff B):** a **runtime-queryable** catalogue of authored patterns per `TrackRole`. The existing `ChordProgressionCatalogueWizard` scans via `AssetDatabase` and is **editor-only** — unusable in a runtime Dev tab. MGP picks the representation (package registry/service, documented Resources contract, or stable per-pattern id/enum).
- **Override entry point (handoff C):** a clean per-render override channel to force a chosen pattern on a specific track — analogous to the existing per-musician `instrumentOverrides` parameter of `RenderSinglePart`. Ideally a per-`(musicianId, TrackRole)` → pattern-override map, with defined precedence (override beats style bundle).

The "type-enum populates a value-enum" idea is a UI nicety; whether it is expressible as a real C# enum depends on whether MGP exposes stable pattern ids. If patterns are only SO references, the dropdown is populated from the SO list instead. MGP decides.

---

## 6. Requirement R2c — Chord progression from a typed string

### 6.1 Intent

Type a progression string and turn it into a real progression for a track, **using the same internal mechanism as the Editor Window** (single source of truth, no reimplementation).

### 6.2 Ownership — one MGP ask (handoff D)

The string→`ChordProgressionData` parser is the MidiGenPlay Chord Progression Editor's Import-path mechanism (the same path `ChordProgressionLLMGenerator` parses through). It is currently editor-bound. MGP is asked to expose it as a **runtime-callable, `#if UNITY_EDITOR`-independent** API returning a runtime-usable `ChordProgressionData` plus parse diagnostics, accepting the same grammar the ALWTTT `chord-progression-importer` skill already emits (setup card + Roman block). The parsed result feeds the R2b (handoff C) override channel.

---

## 7. Dev-Mode integration surface

- New tab: `DevCompositionDebugTab` (`internal static class`, `Draw()`), added to `DevModeController.TabNames` and dispatched in `OnGUI`, `#if ALWTTT_DEV`.
- **R1 section:** per-part track list, one formatted line per track, refreshed on `OnRenderCompleted`; a `Copy` button (`GUIUtility.systemCopyBuffer`) exporting the part's full fingerprint block — this is the intended bridge to the Musical-Lab analysis loop (fingerprint → theory analysis → replacement pattern in importable format).
- **R2a/b/c sections:** added incrementally as the corresponding MGP surfaces land; each gated so an absent package API degrades to a disabled control with an explanatory label (the `DevCardCatalogueTab` gate-status idiom).
- Optional lightweight mirror of R1's single-line-per-track on the `SongTrackElementUI` tooltip (reusing the B2/#3 minicard tooltip), for in-canvas glances without opening the overlay. Secondary; the tab is primary.

---

## 8. Open decisions

**D1 — Where does this doc live?**
- **A**: `Docs/design/Design_Composition_Debug_Tab_v0_1.md` (design/planning bucket).
- **B**: `Docs/integrations/midigenplay/` (boundary-adjacent, since R1 rows 10–14 and all of R2b/c are package-dependent).
- **Recommendation:** A. It is primarily a Dev-Mode feature requirement; the package dependency is captured by reference to the boundary SSoT, not by co-locating there. Confirm actual folder against the repo tree.

**D2 — Do we register the package asks in the boundary SSoT now, or after MGP replies?**
- **A**: Register the four ask-groups in `SSoT_ALWTTT_MidiGenPlay_Boundary §4.3` now (next to BASS-1), marked "requested, awaiting reply".
- **B**: Wait for MGP's return handoff, then record only what is confirmed.
- **Recommendation:** A. The BASS-1 precedent is exactly "write the request down once so the workaround isn't mistaken for design." Recording the open request now is consistent and prevents re-derivation.

**D3 — R2a model target (audition vs commit).** Deferred to implementation (see §4.3). Not owed at this stage.

---

## 9. Dependencies on MidiGenPlay

All package-side needs are consolidated in the companion handoff `Handoff_To_MidiGenPlay_Composition_Debug_v0_1.md`, which requests: (A) resolved-choice readback extension, (B) runtime pattern catalogue, (C) per-track pattern override entry point, (D) runtime chord-progression string parser, (E) a structured return handoff for ALWTTT to implement against, and (F) the list of MGP PK files ALWTTT should add to its own PK.

**Boundary discipline:** this doc specifies *what ALWTTT needs and what it will do with it*. It does **not** define how MidiGenPlay resolves patterns, parses strings, or structures its catalogue. Those remain MidiGenPlay-owned and are answered by MGP, not asserted here.

---

## 10. Required documentation updates (proposed, not yet applied)

- **New doc:** this file (per D1).
- **`SSoT_ALWTTT_MidiGenPlay_Boundary.md §4.3`:** add the four package asks (per D2). Classification: *reference-only / integrative* (records an open cross-project request; changes no implemented truth).
- **`SSoT_INDEX.md` / `coverage-matrix.md`:** register the new design doc. Classification: *structural*.
- **`SSoT_Dev_Mode.md`:** no change yet — updated only when the tab is actually implemented (it would then gain a §for the Composition Debug tab). Noted here so the eventual update is not forgotten.
- **`M1_5_Dev_Mode_Sub_Roadmap.md` Phase 5:** cross-reference this doc as the spec for the reserved composition-injection phase. Classification: *lifecycle/planning*.

No `CURRENT_STATE.md` or `changelog-ssot.md` entry yet — nothing is implemented; this is a requirement + an outbound request.

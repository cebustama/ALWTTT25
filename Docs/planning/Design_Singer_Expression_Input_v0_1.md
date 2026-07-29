# Design — Singer Expression Input v0.1 (registered idea)

**Status:** Registered idea — **not scheduled**. Out of the Roster Expansion campaign; natural rider of Double Harmony **Tier B** (second sung voice) or a Zig reward.
**Scope:** Player input drives the live articulatory-voice levers (vibrato/wobble, timbre: tenseness, mouth/tongue parameters) while Zig sings, adding manual expression.
**Classification:** `reference (planning)` — not a SSoT. Subordinate to `systems/SSoT_Singer_Voice.md` wherever they touch.
**Created:** 2026-07-23 (Roster Expansion planning session; idea by Matías).
**Placement:** `Docs/planning/Design_Singer_Expression_Input_v0_1.md`

---

## 1. Concept

While the singer is active, a player input (held key / axis / pointer gesture) modulates the voice in real time — vibrato depth/speed, tenseness/brightness, mouth shape — so expression becomes something the player *performs*, not only something cards select.

## 2. Ground truth (why this is cheap)

`SSoT_Singer_Voice.md §5` establishes exactly the needed seam: the `VoiceProfileSO` is the **resting state**; *"gameplay animates the levers at runtime"* (lever doc tier 2), and per-state modulation hooks were **deliberately left absent "until a concrete consumer exists."** This idea is that consumer. No new data schema is required for v1 — gameplay code calls the live levers directly. Tier 3 (phrase-metadata automation, Phase D4) stays deferred and is explicitly **not** this.

## 3. v1 minimal shape

- One input → **1–2 levers** (`vibratoDepth` first; `tenseness` second), active **only while the singer is armed and sounding** (Director exposes the active state).
- Smoothing/attack on the lever write so input feels vocal, not switch-like.
- Visual feedback on Zig (existing `CharacterAnimator` / FX hooks) so the causality reads.
- Zero effect when the singer sits out (budget refusal, no melody stem) — input is simply inert.

## 4. Design questions (before scheduling)

Persistent input in a card game vs **bounded moments** (only during a finisher window — pairs naturally with Double Harmony Tier B or the Singalong sequence) · gameplay meaning: pure expression (audio-only) vs a mechanical hook (e.g. sustained input feeds a small Vibe/SongHype trickle — beware creating an attention tax) · input device story on target platforms · accessibility (must never be required for baseline success) · interaction with the profile's identity levers (input offsets the profile, never rewrites it).

## 5. Non-goals

No `MidiGenPlay` change (the package emits MIDI; the voice is consumer-side). No Phase-D4 `IPerformanceMetadataSink`. No per-card lever-automation schema in v1. No second-voice dependency (works with the single demo voice).

## 6. Update rule

Update when scheduled (then: batch + decision ledger + `SSoT_Singer_Voice.md` gains the gameplay-modulation contract at closure) or if the singer SSoT's lever surface changes underneath it.

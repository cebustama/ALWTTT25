# SSoT_Audio — ALWTTT

**Status:** Active — governed subsystem SSoT.
**Scope:** ALWTTT-side audio: the SFX subsystem (card-direct + bus-sensory, single `AudioManager` sink), the music-mix model (per-musician music axis, global music level, master SFX level, effective-volume composition, persistence), the OST playback bus (`MusicDirector` + `OstCatalogSO`, authored-clip music scaled by the Music level), and the ALWTTT↔MidiGenPlay audio boundary.
**Owns:** `AudioManager` as the single SFX sink; `SensorySfxType` key authority; the central sound inventory + coverage audit (`SoundBankSO`); the **per-musician music volume axis** and its effective-volume composition; the persisted audio-mix balance (`AudioMixSettingsSO`); the master-SFX level; the looping crowd-ambience bus (SFX-group `ambienceSource` on `AudioManager`); the OST playback singleton (`MusicDirector`) and OST catalogue (`OstCatalogSO` / `OstTrackId`); the audio-side ownership split with MidiGenPlay.
**Does not own:** the event bus itself (`SensoryEventBus`, `ISensoryEvent`) → `SSoT_Audience_and_Reactions` / Sensory Contract; FT presentation → Sensory Contract; `MidiMusicManager` as the playback host / channel-routing component → `SSoT_Runtime_CompositionSession_Integration` §3.4; any MidiGenPlay package internals (synth, soundfont, per-instrument attenuation) → MidiGenPlay docs.
**Authority order:** `SSoT_CONTRACTS.md` → this SSoT for audio concepts → `CURRENT_STATE.md` for active slice. Cross-project audio facts defer to `SSoT_ALWTTT_MidiGenPlay_Boundary.md`.
**Created:** 2026-06-14 (M-AUDIO-MIX). Consolidates the as-built audio subsystem from `Design_Sensory_Contract_v0_1.md §5A` (S3-audio) and adds the music-mix model + audio boundary.
**Updated:** 2026-06-16 (AUDIO-CHAR-PROFILES-2) — per-ability SFX: an inline `abilitySfx` clip on `AudienceAbilityData`, fired once at ability activation in `AudienceCharacterBase.AbilityRoutine` beside the animator trigger (single-source → immediate; no new key) (§3, invariant 17). Also backfills the phase-1 gaps in `ssot_manifest.yaml` (invariant 16 + `CharacterSfxProfileSO` governs) and `CURRENT_STATE.md`. Prior: AUDIO-CHAR-PROFILES phase 1 — per-character reaction SFX: `CharacterSfxProfileSO` as a clip source for the two reaction keys, per-polarity bank fallback (§2 row, §3, invariant 16). Prior: AUDIO-AMBIENCE — adds the SFX-group crowd-ambience bus (§3, invariant 15). Earlier: AUDIO-OST — OST playback bus (§4.5), invariants 12–14.

---

## 1. Purpose

ALWTTT has two distinct audio surfaces that were previously documented in two
places (the SFX subsystem inline in the Sensory Contract planning doc; the
per-musician music volume scattered across `MidiMusicManager` / `GigManager`
code comments). This SSoT is the single home for both, plus the audio-side of
the MidiGenPlay boundary.

Two surfaces, kept separate on purpose:

- **SFX** — one-shot sound effects for card plays and sensory-bus events. Routed
  through one sink (`AudioManager`), volume-capped by the master-SFX level.
- **Music** — the MIDI arrangement generated and played via the MidiGenPlay
  runtime. Balanced per-musician on the ALWTTT side via a channel mix; the
  package owns generation and synthesis.

---

## 2. Ownership split (audio)

| Concept | Owner | Notes |
|---|---|---|
| SFX playback sink (`AudioManager`) | **ALWTTT (this SSoT)** | single sink for both card-direct and bus-sensory SFX |
| `AudioActionType` (card SFX key) | ALWTTT — `SSoT_Card_Authoring_Contracts` | card-authored; exposed to the LLM card pipeline; stays card-only |
| `SensorySfxType` (bus SFX key) | **ALWTTT (this SSoT)** | bus surfaces; kept off card authoring |
| Sound inventory + coverage audit (`SoundBankSO`) | **ALWTTT (this SSoT)** | card profiles + sensory entries; `GetMissing()` + "Audit SFX Coverage" |
| Per-character reaction SFX (`CharacterSfxProfileSO`) | **ALWTTT (this SSoT)** | per-character clip *source* for `ReactionPositive/Negative`; assigned on `AudienceCharacterData.sfxProfile`; per-polarity `SoundBankSO` fallback; no new key (§3, invariant 16) |
| Per-musician **music** volume axis | **ALWTTT (this SSoT)** | per-musician → channels → MidiGenPlay runtime mix |
| Global music level (the **Music** level) | **ALWTTT (this SSoT)** | `AudioMixSettingsSO.GlobalMusicVolume01` (migrated from `GameplayData`); **scales gig music AND OST** — one level, two consumers (§4.5) |
| OST playback (`MusicDirector`) | **ALWTTT (this SSoT)** | dedicated singleton; authored-clip music; one track audible at a time; two owned sources → crossfade; output scaled by the Music level (§4.5) |
| OST catalogue (`OstCatalogSO`) + `OstTrackId` | **ALWTTT (this SSoT)** | id→clip+loop+`defaultLevel01` inventory; the caller (MusicDirector) owns timing — mirrors the `SoundBankSO` inventory-vs-caller split |
| Master SFX level | **ALWTTT (this SSoT)** | `AudioMixSettingsSO.MasterSfxVolume01` → `AudioManager.sfxSource.volume` |
| Crowd ambience (looping, SFX group) | **ALWTTT (this SSoT)** | `ambienceSource` on `AudioManager`; `masterSfx × ambienceLevel × fade`; gig-driven duck/return (§3) |
| `MidiMusicManager` as playback host / channel routing | ALWTTT — `SSoT_Runtime_CompositionSession_Integration` §3.4 | this SSoT references it for the mix axis; the component is governed there |
| Event bus (`SensoryEventBus`, event structs) | ALWTTT — Sensory Contract / `SSoT_Audience_and_Reactions` | this SSoT consumes bus events for SFX, does not define the bus |
| Per-instrument attenuation (`MIDIInstrumentSO.volume01`) | **MidiGenPlay** | out of scope; **not wired** by ALWTTT — see §6 |
| Synth / soundfont / MIDI rendering | **MidiGenPlay** | package internal |

---

## 3. SFX subsystem (as-built — migrated from Sensory Contract §5A, S3-audio)

> **Status: COMPLETE (placeholder clips).** Every wired surface has ≥1 clip; audio
> fires per event. Clips are placeholders; final intentional, non-generic SFX is a
> tracked follow-up (D1). Sound exists, so the Sensory Contract directive is
> satisfied; the quality upgrade is documented, not deferred-as-silent.

**Two paths, one sink (D-SA-4=A).**
- **Card-direct (opt-in by type — AUDIO-SFX-FIX / D-SFXDEF=B).** `CardBase.Use()` plays the
  card's authored `AudioType` only when it is a real sound type. `AudioActionType.Button` (the
  enum's 0-value, the default for any unset `audioType`) and `AudioActionType.None` (appended
  last so existing serialized ints don't shift) **play nothing** — a card opts *into* a sound by
  being tagged a non-default, clip-backed type. This replaced an unconditional per-play call that
  made every un-tagged card emit the `Button`-keyed clip (the heal-on-every-card / Start-button
  bug). `Button` remains the UI-click type used by `ButtonSoundPlayer` (a separate `buttonSource`
  path), never a card sound. Spec-based cards run through `CardBase.ExecuteEffects`, never the
  `CharacterAction` classes, so the SFX hook lives on the card path (corrected ST-SA-9).
- **Bus-sensory** — `SensoryAudioAdapter` subscribes to `AudienceReactionEvent`,
  `SongEndVibeEvent`, and `SfxStageCrossedEvent`; resolves each to a `SensorySfxType`
  via `SensorySfxPresentation`; plays through `AudioManager`.

Both call the single `AudioManager` sink. A future `CardPlayedEvent` bus consumer
(card→bus migration) is out of scope; if it lands, card play must not fire on both paths.

**Two keys, one authority each (D-SA-6=A).**
- `AudioActionType` (existing) — card-authored; on `CardDefinition`; card-only.
  Authority: `SSoT_Card_Authoring_Contracts`.
- `SensorySfxType` (this SSoT) — bus surfaces, kept separate so sensory tags don't leak
  into card authoring. Members: `ReactionPositive/Negative`, `SongEndVibe/Blocked`,
  `StageCrossLights/Smoke/Fire`. Neutral reaction is FT-only (`SensorySfxPresentation`
  returns null). Grows additively; the Sensory Contract §4 audit is the to-do list.

**Central inventory + coverage (D-SA-7=B).** `SoundBankSO` holds card profiles
(`SoundProfileData` SOs keyed by `AudioActionType`) + inline sensory entries (keyed by
`SensorySfxType`), with `GetMissing()` and an "Audit SFX Coverage" context menu
(operationalizes D1). `AudioManager` loads the bank at `Awake` and builds both lookup dicts.

**Null-safety.** Both `PlayOneShot` overloads warn once per missing type and no-op — a
missing profile / empty clip list is a content gap, not a crash. (Fixed a live NRE in the
prior `PlayOneShot(AudioActionType)` that logged but did not return.)

**Jitter (AUDIO-SFX-FIX #6 / D-SFX-JITTER-SCOPE=B).** SFX jitter is **caller-controlled, not a
sink property.** `AudioManager.PlayOneShot(SensorySfxType, jitter)` spreads one-shots over
`random(0, sfxMaxJitterSeconds)` (default 0.15 s, serialized on `AudioManager`; 0 = off) only when
the caller passes `jitter: true`. The sole opt-in caller is `SensoryAudioAdapter`'s
`AudienceReactionEvent` handler — one loop fans out to many audience members, so those reactions
stagger instead of stacking on one frame. Card SFX and single-source sensory cues (song-end,
stage-cross) are always immediate.

**Per-character reaction clips (AUDIO-CHAR-PROFILES phase 1 / D-CHAR-SFX=C).** A reaction's clip
*source* is resolved per reacting character before the global bank. The adapter reads the reacting
`AudienceCharacterBase`'s `AudienceCharacterData.sfxProfile` (a `CharacterSfxProfileSO` with pos/neg
clip sets); if it has a clip for the resolved polarity, that clip plays via
`AudioManager.PlayOneShot(AudioClip, jitter: true)`. Fallback is **per polarity** — an empty/absent
polarity falls back to the `SoundBankSO` entry for *that* polarity (a positive-only profile still gets
the bank's negative sting), then warn-once/no-op (invariant 3) if the bank is also empty. The
per-character SO is an alternative clip source for the existing `ReactionPositive/Negative` keys; it
introduces **no new `SensorySfxType`**. Neutral stays FT-only. Jitter is unchanged on both paths — the
reaction fan-out staggers regardless of clip source (invariant 10) — and `AudioManager` stays the dumb
sink (the new `PlayOneShot(AudioClip, jitter)` overload reuses the existing jitter coroutine).
Per-character coverage is **not** audited; the `SoundBankSO` audit remains the safety net. Ability SFX
(phase 2) is audience-only and fired from `AudienceCharacterBase` (see the per-ability paragraph below);
musicians act via cards (card-direct `AudioActionType`) and have no ability-fire surface, so
`MusicianCharacterData` deliberately gets **no** SFX profile.

**Per-ability SFX (AUDIO-CHAR-PROFILES-2 / D-CHAR-SFX-2=A, D-ABILITY-SFX-HOME=(i)).** Each audience
ability carries an inline `abilitySfx` clip on `AudienceAbilityData`, beside its `AbilityAnimationData`
trigger (one ability = one authoring spot; **no string key** — `abilityName` is a display string, not a
stable id, so a keyed map on the profile was rejected). It fires **once at ability activation** in
`AudienceCharacterBase.AbilityRoutine`, at the same site as the animator trigger, via
`AudioManager.PlayOneShot(AudioClip, jitter: false)`. The fire is **single-source → immediate** (never
jittered — jitter stays fan-out-only, invariant 10), sits *after* the stun / null-ability /
empty-actions guards (a skipped ability is silent) and *before* `PlayAbilityAnimation`, so it is
**independent of the animation guard**: an ability with a sound and no animator trigger still plays. A
missing clip no-ops in the sink (invariant 3). The clip is **profile/data-sourced, not a new bus key** —
no `SensorySfxType` is added and `AudioManager` stays the dumb sink; `CharacterSfxProfileSO` is unchanged
and stays reaction-only. A **status-apply** fire (a second clip when the status lands, option B) is
**deferred, not rejected** — it would hook the status-apply site in `CharacterActionProcessor…DoAction`,
in parallel to this hook. Per-ability coverage is **not** audited (same posture as reactions).

**UI bus under the SFX level (AUDIO-SFX-FIX).** `AudioManager.SetSfxVolume01` drives both
`sfxSource.volume` and `buttonSource.volume`, so the master-SFX level governs card SFX, sensory
SFX, *and* UI clicks.

**S3-audio decisions (recorded):** D-SA-1=A, D-SA-2=A, D-SA-3, D-SA-4=A, D-SA-5=A,
D-SA-6=A, D-SA-7=B. **AUDIO-SFX-FIX decisions:** D-SFXDEF=B, D-SFX-APPLY=A, D-SFX-JITTER-SCOPE=B,
D-SFX-JITTER-HOME (field).

**Ambience bus (looping crowd — AUDIO-AMBIENCE / D-AMB-BUS=A).** A self-provisioned, looping
`ambienceSource` on `AudioManager` (added in `Awake`, mirroring `MusicDirector`'s owned sources — the
only content to wire is `ambienceClip`). It is an **SFX-group** surface: effective volume =
`masterSfx × ambienceLevel × fade`, recomposed in `ApplyAmbienceVolume` whenever a factor changes, so
`SetSfxVolume01` scales ambience for free alongside `sfxSource` + `buttonSource`. API: `FadeInAmbience`
/ `FadeOutAmbience` / `SetAmbienceLevel01` / `StopAmbience` / `IsAmbiencePlaying`. Fades are **linear,
unscaled** (parity with the OST fades; a `timeScale==0` pause can't freeze a duck/return) — `fadeOut`
0.6 s (duck), `fadeIn` 1.2 s (return / gig open), both serialized (D-AMB-FADE=B). Duck/return are
**volume-only**: the loop keeps running silently during a song so the return has no restart transient;
only `StopAmbience` (gig exit) stops the source. A missing clip warns once + no-ops (invariant 3).

**Gig lifecycle hooks (D-AMB-HOOK=A).** The crowd is present while idle / during action-card play,
ducks while the band performs, returns at song end. The gig drives this from four sites:
`GigManager.StartGig` → `FadeInAmbience` (crowd present at gig open); `OnPlayPressed` →
`FadeOutAmbience` (band starts the song → duck); `OnCompositionSessionEnded` → `FadeInAmbience`
(song end → return, guarded by a captured `wasPlaying`); `OnDestroy` → `StopAmbience` (no bleed into
the reward/menu scene; the fade completes on the surviving `DontDestroyOnLoad` `AudioManager`). The
**return must hook the explicit end callback, not a polled flag**: `OnCompositionSessionEnded` nulls
`_session` synchronously inside `CompositionSession.Tick`, so `GigManager.Update` takes the
`_session == null` early-return and never sees the `IsLoopPlaying` `true→false` edge. (`_isSongPlaying`
also does not flicker between loops/parts within a song — the next loop restarts synchronously in
`Tick` — so the duck is a single edge.) `GigManager.cs` stays governed by its gig SSoT; this SSoT
governs the ambience concept on `AudioManager` (same split as the AUDIO-OST `DevSetGlobalMusicVolume01`
hook). Single loop now (D-AMB-CLIP=A); per-venue beds are a future `AmbienceCatalogSO` (mirroring
`OstCatalogSO` — **not** `SoundBankSO`, which is one-shot coverage).

**AUDIO-AMBIENCE decisions (recorded):** D-AMB-BUS=A, D-AMB-HOME=A, D-AMB-FADE=B, D-AMB-HOOK=A,
D-AMB-CLIP=A.

---

## 4. Music-mix model (M-AUDIO-MIX)

The MIDI arrangement is generated and played by the MidiGenPlay runtime. ALWTTT
balances it **per musician** by setting per-channel volume on the runtime mix.

### 4.1 Axes and composition

Three ALWTTT-owned axes compose into an effective per-musician music volume:

```
effective = globalMusic01 * perMusician01 * instrument01
```

- `globalMusic01` — `AudioMixSettingsSO.GlobalMusicVolume01`. One global cut applied
  to all music.
- `perMusician01` — the per-musician balance for this musician (Dev slider / persisted
  default). Held at runtime in `GigManager._musicianVolume01`.
- `instrument01` — **always `1.0`.** Reserved for a per-instrument axis that ALWTTT
  does **not** own (see §6). Documented as a constant so the formula is explicit, not
  a silent gap.

Composition site: `GigManager.ComputeEffectiveMusicianVolume01`. Application site:
`MidiMusicManager.SetMusicianVolume01(musicianId, effective)` →
`ResolveChannelsForMusician` → `IMixController.SetChannelVolume01` per channel.

### 4.2 Channel routing and timing (load-bearing)

`MidiMusicManager.ResolveChannelsForMusician` resolves a musician to channel indices
using `_channelOwners`, which is populated by `SetChannelOwners` **after** song
generation and immediately before `Play(song)`. Consequences:

- At **gig start** no song has been generated, so channels are unresolved and
  `SetMusicianVolume01` is a no-op for the mix. A balance "loaded at gig start" is
  therefore *staged*, not yet audible.
- The per-musician axis is **re-applied per song** by `GigManager.ReapplyMusicianMix`,
  called right after `Play(song)` once `SetChannelOwners` has run. This is what makes the
  persisted/dev balance land on every song (without it, the axis is not re-applied across
  song transitions).
- `MidiMusicManager.OnSongStartedInternal` re-asserts the last-known per-channel mix
  (`_lastKnownVol01`, all non-metronome channels) when playback goes live, then applies
  any pending highlight. This is defensive against an **unverified** MidiGenPlay-side
  channel-volume reset on (re)start: if the package preserves volumes it is harmless; if
  it resets them, the re-assert restores the intended balance. (This behavior is the
  ALWTTT playback host re-asserting known state; the reset itself, if any, is package
  truth and not verifiable from this side — see §6.)

### 4.3 Relationship to highlight

`MidiMusicManager.Highlight` (DuckOthers / Solo) is a separate, transient mix override
with its own snapshot/restore (`_savedVol01`). It writes `_lastKnownVol01` while active,
so the §4.2 re-assert and highlight compose correctly: the base balance is re-asserted
first, highlight (if pending/active) overrides afterward, and clearing highlight restores
the snapshot. The persisted balance is the *base*; highlight is a temporary lens over it.

### 4.4 Master SFX

`AudioMixSettingsSO.MasterSfxVolume01` drives both `AudioManager.sfxSource.volume` and
`buttonSource.volume` via `AudioManager.SetSfxVolume01` — it caps card-direct SFX, bus-sensory
SFX, **and** UI clicks (AUDIO-SFX-FIX). `musicSource` is not affected (music is the MIDI runtime,
not `musicSource`). Applied **app-wide at `AudioManager.Awake`** from the SO (D-SFX-APPLY=A):
`AudioManager` is `DontDestroyOnLoad` and present in every scene, so the SFX level is correct
from boot (Main Menu included), not only inside a gig. `GigManager.ApplyPersistedAudioMix`
re-applies it at gig start; the Dev tab drives it live.

### 4.5 OST playback bus (AUDIO-OST)

A second music surface, kept separate from gig music on purpose. "Gig music" =
generated MIDI via `MidiMusicManager` (per-channel mix, §4.1–4.3). "OST music" =
authored `AudioClip`s played by **`MusicDirector`** (D-OST-HOME=B). They never both
play music (see invariant 14).

**Ownership + transition (D-OST-HOME=B, D2=A).** `MusicDirector` is a `DontDestroyOnLoad`
singleton in `Managers/` (alongside `AudioManager` + `MidiMusicManager`). It owns **two**
`AudioSource`s that ping-pong, so `CrossfadeTo` is a real overlap, not a fade-out-then-in.
There is **no `AudioMixer`** in ALWTTT, so the per-source volume *is* the level:
`AudioSource.volume = musicLevel01 × track.defaultLevel01`. API: `Play(id, hardCut)`,
`CrossfadeTo(id, seconds)`, `Stop(immediate)`, `RefreshMusicLevel()`. Default transition is a
0.75 s crossfade; fades use unscaled time (OST is UI/menu music, independent of `timeScale`).
The dormant `AudioManager.musicSource` + `PlayMusic` (zero callers) are not used by OST;
AUDIO-OST recommends retiring them for one-home tidiness (harmless dead code if kept).

**Catalogue (D1=A).** `OstCatalogSO` is the OST inventory, keyed by the `OstTrackId` enum
(`None` = stop sentinel; `MainMenu` = first content). Each entry is `{id, clip, loop,
defaultLevel01}`. Scene→track mapping is **not** in the catalogue — it lives on the director
(see below), mirroring the `SoundBankSO` "inventory vs caller decides" split. A missing /
clipless id is a content gap (invariant 3 / warn + no-op).

**Scene reaction (D3=A).** `MusicDirector` subscribes to `SceneManager.sceneLoaded` and
consults a serialized **build-index** scene→track map (no dependency on `SceneChanger` /
`SceneData`; robust to every entry path — Start button, ESC-return-to-menu, gig restart). It
also evaluates the active scene once in `Start` (first-launch menu song, since `sceneLoaded`
does not fire for an already-active scene). **Unlisted scenes resolve to `OstTrackId.None`
(OST stops)** — this is the mechanism that guarantees gig scenes have no OST overlap.
Re-entering the same scene with the same track does not restart it.

**Music level — one level, two consumers (D4).** `AudioMixSettingsSO.GlobalMusicVolume01` (the
"Music" level, default ~0.7) scales gig music (per-channel via
`GigManager.ComputeEffectiveMusicianVolume01`) **and** OST (`AudioSource.volume` via
`MusicDirector`). `MusicDirector` reads it at play/crossfade and on `RefreshMusicLevel()`, which
`GigManager.DevSetGlobalMusicVolume01` calls so a live slider drag updates a currently-playing
OST track. The Dev "Audio Mix" tab is **gig-only** (it bails when `GigManager.Instance == null`),
so the live-drag-while-OST-plays path is not reachable in current content (the only OST track
plays in the menu, which has no `GigManager`). The level *scaling* OST is real now (read at play
time); the *live-drag-on-playing-OST* test is deferred (§8).

**OST asset pipeline (convention — D-OST-DOCS-1=A).** Source clips are mastered in the DAW and
exported as **WAV** (24-bit; include master/return effects; **not** normalized — leave ~1–3 dB
headroom). Import the WAV into Unity and set `Compression Format = Vorbis` (~q70),
`Load Type = Streaming`. Do **not** pre-convert to OGG: Unity re-encodes on import, so a
pre-converted OGG is a double lossy pass. Loudness is governed at runtime (Music level ×
per-track `defaultLevel01`), not baked per file, so tracks stay consistent as the catalogue
grows; `defaultLevel01` only attenuates (0..1), so the source must already peak near 0 dBFS.
Seamless loops are a **compositional** property (loop-compatible start/end); Vorbis priming can
add a micro-seam at the loop point — use WAV import if a sample-accurate seamless loop is required.

---

## 5. Persistence

There is **no runtime save system** in ALWTTT. The audio-mix balance is persisted as a
**design-time SO asset** (`AudioMixSettingsSO`), referenced by `GigManager`.

- **Load (gig start).** `GigManager.ApplyPersistedAudioMix` runs in `StartGig` after
  the band is built: it seeds `_musicianVolume01` from the SO's per-musician list, pushes
  `MasterSfxVolume01` to `AudioManager`, and leaves global to be read live by
  `ComputeEffectiveMusicianVolume01`. Per-musician channel CC is deferred to
  `ReapplyMusicianMix` at song start (§4.2).
- **Edit + save (Dev tab).** The Dev "Audio Mix" tab routes every change through
  `GigManager.DevSet…` wrappers, which apply the change live **and**, in the editor only,
  mark the SO dirty and save (`GigManager.PersistAudioMixInEditor`, `#if UNITY_EDITOR`).
- **Ship.** The baked asset values are the shipped default. In a player build there is no
  in-build persistence of further edits; Dev Mode (and therefore the tab) is
  `ALWTTT_DEV`-gated, so the player never edits the mix — they consume the baked default.

This is editor-time tuning whose output ships as the runtime default. It is **not** a
player-facing settings menu; an in-game audio options screen (player-writable, build-time
persistent) is a separate future feature and would need a real save layer.

### 5.1 Persistence home (D-MIX-HOME=B)

The balance is homed in a **dedicated `AudioMixSettingsSO`** holding all three concepts
(global music + master SFX + per-musician list), rather than distributed across musician
assets or piled onto `GameplayData`. Rationale: the deliverable is a tunable balance *as a
whole*; one artifact gives one inspector view, one load point, one manifest entry, and a
clean `governs:` target. `GameplayData.globalMusicVolume01` is migrated here; its single
former reader (`ComputeEffectiveMusicianVolume01`) now reads the SO. The `GameplayData`
field is left dead pending removal in a `GameplayData.cs` follow-up (one-home cleanup).

---

## 6. Audio boundary (ALWTTT ↔ MidiGenPlay)

Defers to `SSoT_ALWTTT_MidiGenPlay_Boundary.md`; the audio-specific facts:

- **ALWTTT owns the per-MUSICIAN music axis** (§4) and the SFX sink (§3). These are
  game-side runtime integration concerns.
- **MidiGenPlay owns per-INSTRUMENT attenuation** (`MIDIInstrumentSO.volume01`), synth,
  soundfont, and rendering. ALWTTT **must not wire** `MIDIInstrumentSO.volume01`: it is
  unknown whether the package applies it internally, so wiring it on the ALWTTT side risks
  **double-attenuation**. The `instrument01` factor in §4.1 is pinned to `1.0` for this
  reason. (Boundary decision, M-AUDIO-MIX.)
- Any channel-volume reset behavior on song (re)start is **package truth** and not
  verifiable from the ALWTTT side; ALWTTT's response (§4.2 re-assert) is a defensive
  host-side measure, not a claim about package behavior.

---

## 7. Invariants

1. **One SFX sink.** All SFX (card-direct + bus-sensory) play through `AudioManager`.
   No system plays one-shots elsewhere. Master SFX caps `sfxSource` only.
2. **Two SFX keys, one authority each.** `AudioActionType` is card-only
   (`SSoT_Card_Authoring_Contracts`); `SensorySfxType` is bus-only (this SSoT). Neither
   leaks into the other. `CharacterSfxProfileSO` is an alternative clip *source* for the two
   `SensorySfxType` reaction keys, not a new key (invariant 16).
3. **Missing audio is a content gap, not a crash.** Missing profile / empty clip list →
   warn-once + no-op. OST follows the same rule via `MusicDirector` (a missing / clipless
   `OstTrackId` warns + no-op; note OST warns *per-occurrence*, not warn-once — harmless at OST
   frequency).
4. **ALWTTT owns only the per-musician music axis.** `MIDIInstrumentSO.volume01` is never
   wired ALWTTT-side; `instrument01 == 1.0` in the effective-volume formula.
5. **Effective music volume = `global * perMusician * 1.0`**, composed in
   `ComputeEffectiveMusicianVolume01`, applied via `SetMusicianVolume01`.
6. **The balance re-applies per song.** `ReapplyMusicianMix` after `Play`, plus the
   `OnSongStartedInternal` re-assert, make the persisted/dev balance survive song
   transitions. A balance loaded at gig start is staged, not audible, until the first song.
7. **Persistence is an SO asset baked in-editor.** No runtime save; Dev edits persist via
   `SetDirty`/`SaveAssets` under `#if UNITY_EDITOR`; baked values are the shipped default.
8. **One home per audio concept.** Global music, master SFX, and per-musician balance live
   in `AudioMixSettingsSO`. `GameplayData.globalMusicVolume01` is removed.
9. **Card SFX is opt-in by type.** `Button` (default / UI-click type) and `None` play nothing on
   the card path; a card opts into a sound via a non-default, clip-backed `AudioActionType`.
10. **Jitter is caller-controlled (fan-out only).** Only `SensoryAudioAdapter`'s reaction handler
    passes `jitter: true`; card and single-source SFX are immediate. `sfxMaxJitterSeconds`
    (default 0.15, 0 = off) lives on `AudioManager`.
11. **Master SFX applies app-wide at `AudioManager.Awake`** and governs `sfxSource` + `buttonSource`
    (UI clicks are under the SFX level).
12. **OST plays only through `MusicDirector`, one track audible at a time.** Authored-clip music
    has a single owner; the two internal `AudioSource`s overlap only during a crossfade.
13. **The Music level scales gig music and OST.** `AudioMixSettingsSO.GlobalMusicVolume01` is one
    level with two consumers: gig music (per-channel via `GigManager.ComputeEffectiveMusicianVolume01`)
    and OST (`AudioSource.volume` via `MusicDirector`).
14. **Gig music and OST never play simultaneously.** Gig scenes are not listed in the
    `MusicDirector` scene→track map; unlisted scenes resolve to `OstTrackId.None` and stop OST, so
    entering a gig fades the menu OST out.
15. **Crowd ambience is an SFX-group loop on `AudioManager`.** Effective =
    `masterSfx × ambienceLevel × fade`; it plays while the crowd is present/idle and ducks under a
    performing song (volume-only — the loop never restarts). It is gig-scoped: started at gig open
    (`StartGig`), returned at song end (`OnCompositionSessionEnded`), stopped on teardown
    (`OnDestroy`). The song-end **return** hooks the explicit end callback, not a polled flag,
    because `_session` is nulled synchronously inside `Tick`.
16. **Reaction clips resolve per-character first, bank fallback per polarity.** A reacting
    character's `CharacterSfxProfileSO` (on `AudienceCharacterData.sfxProfile`) is consulted before
    `SoundBankSO`; an empty polarity falls back to the bank for *that* polarity, then invariant 3.
    The per-character SO is a clip *source* for the existing `ReactionPositive/Negative` keys — it
    adds no new `SensorySfxType`. Neutral stays FT-only. Jitter (invariant 10) is unchanged on both
    the profile and bank paths. Per-character coverage is not audited; the `SoundBankSO` audit
    remains the safety net. (Phase 1. Ability SFX landed in phase 2 — invariant 17. No musician SFX
    profile: musicians act via cards, not abilities.)
17. **Ability SFX fires once at activation, single-source and immediate.** An audience ability's inline
    `abilitySfx` clip (on `AudienceAbilityData`, beside its `AbilityAnimationData` trigger) plays once
    when the ability activates, in `AudienceCharacterBase.AbilityRoutine` at the animator-trigger site,
    via `AudioManager.PlayOneShot(AudioClip, jitter: false)`. It is **immediate, never jittered** (jitter
    stays fan-out-only — invariant 10), fires *after* the stun / null / empty-actions guards (a skipped
    ability is silent) and *before* `PlayAbilityAnimation` (so a sound with no animator trigger still
    plays), and **no-ops on a null clip** (invariant 3). The clip is profile/data-sourced — it adds **no
    new `SensorySfxType`** and `AudioManager` stays the dumb sink (D-ABILITY-SFX-HOME=(i), D-CHAR-SFX-2=A).
    `CharacterSfxProfileSO` is untouched (reaction-only). A status-apply fire (option B) is deferred, not
    rejected. Per-ability coverage is unaudited (same posture as reactions).

---

## 8. Validation / smoke references

Per the project smoke-test rule, M-AUDIO-MIX ships its smoke set (ST-AM-1..7:
slider→channel mapping, global/SFX scaling, persistence round-trip, cross-song persistence,
highlight override/restore, no-asset fallback) and AUDIO-SFX-FIX ships ST-SFX-1..8 (silent
default, tagged-card sounds, menu/card heal-bug fixed, UI under the SFX slider, reaction
stagger, jitter-off immediate, card immediate, slider regression) — all PASS. S3-audio's
per-event SFX smoke tests (ST-SA-A1..A4, ST-SA-9) remain the SFX-subsystem validation of
record. Audio smoke-test failures are a closure blocker for the owning batch (Sensory
Contract §7 / `Design_Project_Directives_v0_1.md §D1.2`).

AUDIO-OST ships ST-OST-1..7 (menu plays/loops on entry; one-at-a-time; stops on scene change
with no gig overlap; return-to-menu re-arms; Music level scales OST; gig music intact;
missing-clip no-crash) — all PASS. ST-OST-8 (true two-track crossfade) and the
live-Music-slider-on-playing-OST test are deferred (single OST track in content; the Dev Audio
Mix tab is gig-only).

AUDIO-AMBIENCE ships ST-AMB-1..8 (loops idle; ducks on song start; returns at song end; stays
ducked across parts [regression]; SFX slider scales it; card/UI one-shots intact [regression]; no
click/pop at duck/return; no bleed into a non-gig scene) — all PASS. No deferrals (ambience and the
Dev Audio Mix tab are both gig-scoped).

---

## 9. Forward refs / open items

- **D1 (tracked, separate):** final intentional, non-generic SFX clips replace the S3
  placeholders. The `SoundBankSO` "Audit SFX Coverage" menu is the to-do surface.
- **OST (AUDIO-OST):** `MusicDirector` owns OST playback; `OstCatalogSO`/`OstTrackId` are the
  catalogue. **Deferred tests:** true two-track crossfade (needs a 2nd OST clip); live
  Music-slider scaling a *playing* OST track (Dev Audio Mix tab is gig-only; the only OST plays in
  the menu) — reachable once gig-scene OST or a menu-accessible Music control exists.
- **AUDIO-AMBIENCE:** DONE (2026-06-16) — SFX-group crowd-ambience loop on `AudioManager` (§3);
  gig-driven duck/return; ST-AMB-1..8 PASS. **Forward:** per-venue beds → a future `AmbienceCatalogSO`
  (not `SoundBankSO`).
- **AUDIO-CHAR-PROFILES (phase 1): DONE (2026-06-16)** — `CharacterSfxProfileSO` (pos/neg reaction
  clips; neutral stays FT-only) on `AudienceCharacterData.sfxProfile`; per-character reaction
  resolution with per-polarity `SoundBankSO` fallback; `AudioManager.PlayOneShot(AudioClip, jitter)`
  seam (sink stays dumb). ST-CHAR-1..7 PASS (§3, invariant 16). Per-character coverage is unaudited
  (bank audit is the net).
- **AUDIO-CHAR-PROFILES-2 (phase 2): DONE (2026-06-16)** — per-ability SFX. An inline `abilitySfx` clip
  on `AudienceAbilityData` (**not** a map on the profile — D-ABILITY-SFX-HOME=(i)) fired once at ability
  activation in `AudienceCharacterBase.AbilityRoutine` beside the animator trigger; immediate /
  single-source (`jitter: false`), independent of the animation guard, no-op on null. No new
  `SensorySfxType`; `CharacterSfxProfileSO` unchanged (reaction-only). **No musician `sfxProfile`** —
  ability fire is audience-only; musicians use card-direct `AudioActionType` (D-CHAR-SFX-2=A). ST-ABIL-1..6
  PASS (ST-ABIL-5 deferred to Dev Mode / M1.5 — Stun not player-applicable). §3, invariant 17. Backfilled
  the phase-1 misses in `ssot_manifest.yaml` (invariant 16 + `CharacterSfxProfileSO` governs) and
  `CURRENT_STATE.md`. **Deferred (option B):** a status-apply clip at the `CharacterActionProcessor…
  DoAction` site — not built.
- **GameplayData.cs cleanup:** DONE — the dead `globalMusicVolume01` field was removed
  (M-AUDIO-MIX); the live home is `AudioMixSettingsSO`.
- **Player-facing audio options:** out of scope here; needs a real save layer (PlayerPrefs
  / JSON). Distinct from this editor-time tuning surface.
- `SSoT_ALWTTT_MidiGenPlay_Boundary.md` — cross-project ownership.
- `SSoT_Runtime_CompositionSession_Integration.md` §3.4 — `MidiMusicManager` integration / channel routing.
- `SSoT_Dev_Mode.md` — the Dev "Audio Mix" tab surface.
- `Design_Sensory_Contract_v0_1.md` — event bus, FT presentation, §4 SFX audit, §D2
  directive (this SSoT is the audio home that §5A now points to).

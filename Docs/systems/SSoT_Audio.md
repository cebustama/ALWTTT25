# SSoT_Audio — ALWTTT

**Status:** Active — governed subsystem SSoT.
**Scope:** ALWTTT-side audio: the SFX subsystem (card-direct + bus-sensory, single `AudioManager` sink), the music-mix model (per-musician music axis, global music level, master SFX level, effective-volume composition, persistence), the OST playback bus (`MusicDirector` + `OstCatalogSO`, authored-clip music scaled by the Music level), and the ALWTTT↔MidiGenPlay audio boundary. A second runtime audio source, the articulatory **singer voice**, is governed separately by `systems/SSoT_Singer_Voice.md`; it currently bypasses this SSoT's mixer routing (balance via the voice profile's `gain`), a follow-up noted there; the one plane it does follow is the song-tail plane (§4.10, R-V).
**Owns:** `AudioManager` as the single SFX sink; `SensorySfxType` key authority; the central sound inventory + coverage audit (`SoundBankSO`); the **per-musician music volume axis** and its effective-volume composition; the persisted audio-mix balance (`AudioMixSettingsSO`); the master-SFX level; the looping crowd-ambience bus (SFX-group `ambienceSource` on `AudioManager`); the OST playback singleton (`MusicDirector`) and OST catalogue (`OstCatalogSO` / `OstTrackId`); the audio-side ownership split with MidiGenPlay.
**Does not own:** the event bus itself (`SensoryEventBus`, `ISensoryEvent`) → `SSoT_Audience_and_Reactions` / Sensory Contract; FT presentation → Sensory Contract; `MidiMusicManager` as the playback host / channel-routing component → `SSoT_Runtime_CompositionSession_Integration` §3.4; any MidiGenPlay package internals (synth, soundfont, per-instrument attenuation) → MidiGenPlay docs.
**Authority order:** `SSoT_CONTRACTS.md` → this SSoT for audio concepts → `CURRENT_STATE.md` for active slice. Cross-project audio facts defer to `SSoT_ALWTTT_MidiGenPlay_Boundary.md`.
**Created:** 2026-06-14 (M-AUDIO-MIX). Consolidates the as-built audio subsystem from `Design_Sensory_Contract_v0_1.md §5A` (S3-audio) and adds the music-mix model + audio boundary.
**Updated:** 2026-11-13 (AUDIO-LOOP-1, DEMO-1211 S9b) — loop chaining without cut: a loop that follows a loop which ended on its own no longer cuts what is still ringing (crash tails, chord releases): new §4.11, seam `IMidiTransportChain` (ALWTTT-side), one local patch to Maestro (`MPTK_ClearSoundOnStart`, registry `reference/maestro-parches.md`); §6 bullet; invariant 28; §9 AUDIO-LOOP-2. Prior: 2026-10-01 (SONG-TAIL-1 + PLAY-TIMING-0) — song tail: the last loop keeps playing between songs outside the session and the next song enters on the tail's bar line: new §4.10; invariant 19 recounted to five planes (closes F-POOL-1); invariants 24–27. Prior: 2026-09-25 (PAUSE-1) — player volume plane (`PlayerAudioPrefs`, PlayerPrefs; multiplies the designed mix, never writes the SO): new §4.8; transport suspension seam (`IMidiTransportSuspend`, ALWTTT-side, `IPlayMidi` untouched): new §4.9; §4.4 design vs effective SFX; §5 persistence now two layers; §6 boundary bullet; invariants 21–23. Prior: 2026-07-22 (BAL-1) — bytes-plane mix gains adopted (MGP-MIX-1 on MidiGenPlay 1.2.0): new §4.6 (per-`(musicianId, TrackRole)` gain baked as CC7; `MixGainProfileSO`; emission gate; anti-compensation); §4.2 rewritten — the MPTK channel-volume reset is now **verified** (not assumed) and the re-assert is **deferred past tick-0** and **composed** with the baked gain via the single `WriteChannelVolume01` boundary. Contract home stays boundary §8.3. Prior: 2026-07-13 (JUICE-PW) — card Vibe-impact sensory surface: new bus event `AudienceVibeImpactEvent` + new key `CardVibeImpact` (one sting per card play, at impact not at drop; `AudioType = None` on such cards — D-PW-AUDIO), §3 + invariant 18; backfills `RewardOpened` (S5h) into the §3 key list and `RewardChoiceOpenedEvent` into the adapter's subscription list. Prior: 2026-06-16 (AUDIO-CHAR-PROFILES-2) — per-ability SFX: an inline `abilitySfx` clip on `AudienceAbilityData`, fired once at ability activation in `AudienceCharacterBase.AbilityRoutine` beside the animator trigger (single-source → immediate; no new key) (§3, invariant 17). Also backfills the phase-1 gaps in `ssot_manifest.yaml` (invariant 16 + `CharacterSfxProfileSO` governs) and `CURRENT_STATE.md`. Prior: AUDIO-CHAR-PROFILES phase 1 — per-character reaction SFX: `CharacterSfxProfileSO` as a clip source for the two reaction keys, per-polarity bank fallback (§2 row, §3, invariant 16). Prior: AUDIO-AMBIENCE — adds the SFX-group crowd-ambience bus (§3, invariant 15). Earlier: AUDIO-OST — OST playback bus (§4.5), invariants 12–14.

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
  `SongEndVibeEvent`, `SfxStageCrossedEvent`, `RewardChoiceOpenedEvent` (S5h) and
  `AudienceVibeImpactEvent` (JUICE-PW); resolves each to a `SensorySfxType`
  via `SensorySfxPresentation`; plays through `AudioManager`.

Both call the single `AudioManager` sink.

**A card can now sound on either path — never both (JUICE-PW / D-PW-AUDIO).** The old note here
said a card→bus migration was out of scope and that, if it landed, card play must not fire on both
paths. JUICE-PW lands the *effect-time* half of that: a card's **Vibe impact** is a bus surface
(`AudienceVibeImpactEvent` → `CardVibeImpact`), fired at effect resolution, while the card's
authored `AudioType` still fires at **drop** time on the card-direct path. The two are not
mutually exclusive in code, so **authoring** keeps them apart: a card whose sound should be its
impact is authored `AudioType = None` (Psychic Waves is the first). See invariant 18 and
`SSoT_Card_Authoring_Contracts` §5.15. `CardPlayedEvent` itself is still **not** an audio
producer — it stays a purely semantic tutorial event.

**Two keys, one authority each (D-SA-6=A).**
- `AudioActionType` (existing) — card-authored; on `CardDefinition`; card-only.
  Authority: `SSoT_Card_Authoring_Contracts`.
- `SensorySfxType` (this SSoT) — bus surfaces, kept separate so sensory tags don't leak
  into card authoring. Members: `ReactionPositive/Negative`, `SongEndVibe/Blocked`,
  `StageCrossLights/Smoke/Fire`, `RewardOpened` (S5h — **backfilled here 2026-07-13; the member
  shipped with S5h but was never listed**), `CardVibeImpact` (JUICE-PW). Neutral reaction is
  FT-only (`SensorySfxPresentation` returns null); so is every AoE target after the first on a
  card impact (`ForCardVibeImpact` returns null for `FanoutIndex != 0`). Grows additively; the
  Sensory Contract §4 audit is the to-do list.

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

**Card Vibe impact sting (JUICE-PW / D-PW-AUDIO, D3=A).** A card whose `ModifyVibeSpec` resolves
against the audience publishes `AudienceVibeImpactEvent` **once per target** from
`CardBase.ExecuteEffects`. `SensoryAudioAdapter` plays **one** `CardVibeImpact` sting for the whole
card, not one per target: `SensorySfxPresentation.ForCardVibeImpact` returns a key only when
`FanoutIndex == 0` and null otherwise. The sting is **single-source → immediate** (`jitter: false`;
jitter stays fan-out-only, invariant 10) — the *visual* fan-out is what staggers, in
`SensoryFxAdapter`. It fires **at impact, not at drop**, and it fires even when the first target
blocked the Vibe (the card resolved; the grey `INDIFFERENT` floater needs its audio floor per D2).
Because the card-direct `AudioType` path is still live, **a card that opts into the impact sting is
authored `AudioType = None`** — otherwise it sounds twice (invariant 18;
`SSoT_Card_Authoring_Contracts` §5.15). Shipped with a **placeholder clip** (`Telephone`), like the
rest of the S3-audio layer → D1 follow-up.

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
  any pending highlight. **The reset it defends against is now VERIFIED, not assumed
  (BAL-1, 2026-07-22):** MPTK wipes every channel's CC7 to 100 on each play
  (`MPTK_Play → MPTK_InitSynth → new MPTKChannels → fluid_channel_init_ctrl`;
  `EnableResetChannel` default true — boundary §8.3 F-MPTK-1), so the re-assert is
  required, not merely defensive. Two BAL-1 changes to this re-assert:
  - **Deferred past tick-0.** `OnEventStartPlayMidi` fires *before* the sequencer
    processes the tick-0 preamble (boundary §8.3 F-MPTK-2), so an immediate re-assert
    would land *under* the baked bytes-plane CC7 and be overwritten. The re-assert now
    runs from a bounded coroutine (`ReassertLiveMixAfterPreamble`, waits for
    `CurrentTick > 0`) so a non-identity persisted/dev balance lands *after* the preamble
    and wins.
  - **Composed, not raw.** All musician-channel live writes go through a single boundary
    `WriteChannelVolume01(ch, live01)` that composes the live value with the playing
    part's baked bytes-plane gain: `composed01 = live01 × bakedGain01 × (100/127)`
    (§4.6). Identity balance (live 1.0) reproduces the baked CC7 exactly, so the
    re-assert is idempotent on ungained channels; a non-identity balance ducks on top of
    the baked gain rather than replacing it. Metronome writes bypass this boundary.

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

**Design vs effective (PAUSE-1).** `SetSfxVolume01` receives the **design** level (SO / Dev tab) and
stores it in `_designSfx01`; `SfxVolume01` returns that design level. The level actually written to
`sfxSource`, `buttonSource` and the ambience cap is **design × player** (`PlayerAudioPrefs.Sfx01`, §4.8),
recomposed in `AudioManager.ApplySfxLevels`. With the player level at 1.0 both are identical, so the
Dev Audio Mix slider reads exactly what it did before (ST-P1-13).

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

### 4.6 Bytes-plane mix gains (BAL-1 / MGP-MIX-1) — the second plane

Distinct from the **live** per-musician axis above, ALWTTT also carries a **bytes-plane**
ensemble balance: a per-`(musicianId, TrackRole)` **gain** baked into the generated MIDI as
a CC7 event at render time. The two planes are separate controls and compose; neither
replaces the other. The **contract home is `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.3** —
this section states only the ALWTTT-side model.

- **Where it lives.** `MixGainProfileSO` (content SO, `Assets/Scripts/Data/Audio/`), keyed
  `(musicianId, TrackRole)`, implicit default 1.0. A serialized field on `GigManager`
  (D-BAL-8=A), resolved once at gig start (`StartGig → SetGigMixGains`, D-BAL-3=A).
- **Law.** `effectiveCc7 = clamp(round(volume01 × gain × 100), 0, 127)` (package-side;
  `volume01` is 1.0/unauthored today). Identity `gain = 1.0` → CC7=100; `gain = 0` mutes
  at playback but keeps note events (stems/hashes intact); saturates at `gain ≈ 1.27`.
- **Emission gate.** A CC7 is emitted **only** for tracks with a profile entry — no entry
  ⇒ no event ⇒ byte-identical output. A track's gain is folded into `trackInputsHash`
  (`ComputeHashFromTrackEntry`), so a gain change re-keys the stem/bundle cache and can
  never replay stale CC7.
- **No Rhythm.** Channel 9 is shared; `MixGainProfileSO.BuildGainMap` drops Rhythm entries
  with a warning before they reach the package (D-BAL-5=A / package D-MIX-4=A).
- **Anti-compensation (normative).** Gains express **ensemble intent** ("this bass sits
  back"), never per-patch loudness correction — that is the package `volume01`'s job and
  would double up when the package's `volume01` authoring (D-MIX-6) lands (boundary §8.3).
- **Scope.** Gig loop (per-part) only. `GenerateSong` / jam / menu paths stay ungained
  (boundary §8.3 scope limit).
- **Readback.** `MidiMusicManager.LastAppliedCc7ByTrack` (the *baked* CC7 the package
  emitted) and `GetLiveComposedCc7(channel)` (the *live-composed* CC7 last written) feed the
  Dev Mode → Audio Mix truth surface. Note the two differ by design: the baked readback does
  not move when a live slider moves (no re-render); the composed strip does.

Relationship to §4.1's axis formula: the bytes-plane gain is **not** the `instrument01`
factor (that stays 1.0 and belongs to the package — §6). It is a *separate plane* applied at
render time, composed with the live axis at the `WriteChannelVolume01` boundary.

#### Límite de granularidad del plano de ganancias (CONT-B, 2026-07-31)

`MixGainProfileSO` gana por `(musicianId, TrackRole)`. De ahí se sigue un
límite que conviene tener explícito antes de intentar usarlo como remedio
universal: **dos instrumentos distintos usados por el mismo músico en el mismo
rol no son distinguibles por este plano.** Caso medido: Fingered Bass y Slap
Bass son ambos `(<músico>, Bassline)`, y el desequilibrio entre ellos —
originado en los boosts de Pocket, exclusivos de las lanes slap/pop — no tiene
solución en la capa de mezcla.

Palancas disponibles cuando el desequilibrio es **entre instrumentos del mismo
rol**, en orden de coste:
1. `MIDIInstrumentSO.volume01` del instrumento (asset del paquete; alcance =
   todas sus apariciones).
2. Ask package-side de ganancia/velocity a nivel de carta (§7, ask 3).

Y cuando el problema es **de un músico y rol enteros** (p. ej. el piano de
backing percibido como abrasivo), el plano **sí** es la herramienta correcta y
la más quirúrgica: entrada `(músico, Backing)` con `gain < 1`, editable desde
el tab Audio Mix, sin tocar assets del paquete y sin afectar a los demás roles
del mismo músico. Si tras bajar la ganancia la aspereza persiste, la causa **no
es de nivel** sino de articulación y voicing (`chordExpression`,
`voiceLeadingOverride`), que se resuelven en la carta, no en la mezcla.

### 4.7 Solo duck (R5-d) — the third plane

A one-loop soloist needs the rest of the band to sit back. The duck is a **third multiplicative
factor composed inside `WriteChannelVolume01`**, not a save/restore of raw channel volumes:

```
composedCc7 ≈ round(live01 × bakedGain × duck01 × 100)
```

`MidiMusicManager._duck01ByChannel` (16 floats, 1 = untouched) is written only by
`SetSoloDuck(soloChannel, duck01)` / `ClearSoloDuck()`, and `ReapplyLiveVolumes()` re-sends
every channel's live intent through the boundary so a duck change takes effect at once. The
soloist's channel and the metronome are exempt.

**Why not save/restore.** Highlight (§4.3) owns a **single** snapshot slot (`_savedVol01` +
`_hasSavedMix`). A second saver would restore the other's values whenever the two interleave —
a duck applied during an active highlight, or a highlight fired mid-duck, would leave the mix
at whichever party restored last. Because the duck never touches `_lastKnownVol01` or
`_savedVol01`, the two planes **compose** instead: a duck survives a highlight save/restore
cycle unchanged and vice versa.

**D-BAL-6=B still holds.** The contract is *one write boundary*, not *two factors*: there is
still exactly one call site into `IMixController.SetChannelVolume01`, and `GetLiveComposedCc7`
still reports the CC7 actually emitted — now with the duck included, which is what a diagnostic
should show.

Lifecycle: applied after `PlayRaw` on an injected-solo loop (so the playback preamble's own
volume writes cannot undo it) and cleared at the loop boundary, at `AdvanceToNextPart` and at
`End()`. `ClearSoloDuck` is a no-op when nothing is ducked, so the redundancy is free.
`_lastKnownVol01` is seeded to 1f at Awake, so a clear can never silence a channel that was
never explicitly set.

**Tuning home:** `GigFlowSettingsSO.bonusSoloDuck01` (default 0.55, clamped 0..1) and
`bonusSoloEnabled`.

### 4.8 Player plane (PAUSE-1) — the fourth plane

The player-facing Options panel (v1: music + SFX) adds a **player plane** that multiplies on top of the
**designed** mix instead of replacing it:

```
gig music : composedCc7 ≈ round(live01 × bakedGain × duck01 × player01 × 100)
OST       : volume = globalMusic01 × player01 × track.defaultLevel01
SFX group : effective = designSfx01 × playerSfx01   (sfx + button + ambience cap)
```

- **Home:** `PlayerAudioPrefs` (static, `Assets/Scripts/Data/Audio/PlayerAudioPrefs.cs`), persisted in
  `PlayerPrefs` (keys `alwttt.audio.player_music01` / `alwttt.audio.player_sfx01`, default 1.0). Consumers
  subscribe to `PlayerAudioPrefs.Changed`; `Save()` flushes on Options close.
- **Gig music write site:** the fourth factor inside `MidiMusicManager.WriteChannelVolume01`; a change
  re-sends every channel via `ReapplyLiveVolumes()`, which reads `_lastKnownVol01`. Consequence: a singer
  mute (0) or a Highlight is **scaled, never overwritten** — unlike `SetMusicianVolume01`, which writes a
  musician's channels and would un-mute the singer's melody channel. Verified ST-P1-11 (no doubled melody).
- **Why multiplicative, not a replacing override:** a replacing override would mask every later Dev
  Audio Mix edit on any machine that ever touched Options. Two planes, two owners (designer / player).
- `GigManager.ComputeEffectiveMusicianVolume01` is **unchanged**: it still composes design-side axes only.
- **Known gaps:** the metronome channel bypasses `WriteChannelVolume01` and is not scaled (dev-only); the
  singer voice (`SSoT_Singer_Voice`) has its own source and is not scaled by the player music level.
- **Dev readout:** `GetLiveComposedCc7` (BAL-1 test 4) now includes the player factor; identical at 1.0.

### 4.9 Transport suspension (PAUSE-1)

Pause freezes gig music **in place** (D1=A). The seam is `ALWTTT.Music.IMidiTransportSuspend`
(`IsSuspended`, `Suspend()`, `Resume()`), implemented by `MidiToolkitAdapter` over MPTK's
`MPTK_Pause` / `MPTK_UnPause`. `IPlayMidi` is **declared package-side** (a repo-wide grep of `Assets/`
finds no declaration) and is **not** extended; `MidiMusicManager` discovers the seam with a type test.

- **Contract:** the tick is kept and resumes from the same value (ST-P1-2); **`IsPlaying` stays `true`
  while suspended** so every "wait while playing" loop keeps waiting instead of finishing; a suspension
  raises no end-of-song event (the adapter ignores one, with a warning, if MPTK ever sent it); `Play` and
  `Stop` clear the suspension (un-pause first, so a next song never inherits a paused sequencer).
- **Side sources:** `MidiMusicManager.SuspendPlayback` also pauses every playing `AudioSource` under the
  manager **outside** the transport's own hierarchy — in practice the dsp-anchored singer. Sync after
  resume verified (ST-P1-6).
- **Degradation, declared:** a player that does not implement the seam leaves music playing while paused,
  logged once (`[PAUSE-1] DEGRADED`).
- **Ambience (D11=C):** `AudioManager.PauseAmbience` / `ResumeAmbience` pause the crowd source in place,
  agnostic to duck/fade state; fades keep running in unscaled time by design.
- Speed control is **not** part of this seam (SPEED-1, D7=B).

### 4.10 Song tail (SONG-TAIL-1) — the fifth plane and the bar-line handoff

Between songs the band keeps playing: the last loop repeats until the next song takes over (D-ST0..D-ST7,
2026-09-27; closed 2026-10-01).

- **Lives outside the session (D-ST0).** `MidiMusicManager` replays the bytes of the last `PlayBytes`
  (`_currentPlayData`, same key — D-ST1=A) with no `CompositionSession`: no `LoopFinished`, LoopScore,
  hype or impressions. Part markers are cleared on each repeat (no `IPartInfoListener` events); chord
  markers, note routing and the beat grid behave as on the last loop.
- **Started** in `GigManager.OnCompositionSessionEnded`, after the crowd bed returns, only if
  `OnCompositionSongFinished` saw the switch ON and the transport ended naturally. **Repeats** are polled
  in `MidiMusicManager.Update` (never re-entered from `OnSongEnded`, which `PlayBytes` itself raises).
- **Baked gains survive the repeat.** `PlayBytes` resets the bytes-plane gains to identity unless a
  render queued them, while the replayed bytes still carry their baked CC7; the tail captures the last
  loop's gains and re-queues them on every repeat, or the live re-assert would stomp BAL-1's balance.
- **Level = fifth plane (D-ST3=B).** `_tail01` multiplies inside `WriteChannelVolume01`
  (`songTailLevel01`, default 0.7; 1 when no tail). The singer follows it through
  `SingerVoice.SetExternalLevel01` (R-V, `SSoT_Singer_Voice`). The crowd bed returns as before and plays
  under/with the tail.
- **Handoff on the bar line (D-ST7=C).** The tail runs through Phase 4, the audience turn and the next
  composition phase. On Play, `CompositionSession` renders at once (synchronous) and calls `PlayRaw` as
  always; while a tail plays, `PlayBytes` **holds** that playback (bytes, label, BAL-1 gains) instead of
  starting it, returns its duration, and keeps `IsAnySongPlaying()` true. `TickSongTail` starts it on the
  tail's next bar line, published by the beat grid (Integration §3.4.1 item 7), `songTailHandoffLeadMs`
  (default 30 ms) early to absorb the MPTK start latency; a bar line already passed when Play arrived moves
  it to the next one; if the transport reaches the loop end first, it starts there. `CompositionSession`
  is untouched and unaware.
- **Stops.** Victory and defeat fade (`songTailFadeSeconds`, scaled time — a pause freezes the fade);
  `GigManager.OnDestroy` cuts (MMM is DontDestroyOnLoad); `MidiMusicManager.Stop()` (pause → main menu)
  ends it first so the poll cannot restart it. The band returns to idle when the tail stops (D-ST4=A:
  `ResetStageToIdle` split into band / audience; only the audience goes idle at song end).
- **Pause.** Same transport, so it freezes in place; no repeat and no held handoff fires while suspended,
  including in the gap between loops.
- **A/B switch (D-ST6=A):** `GigFlowSettingsSO.songTailEnabled` (+ a session-only Dev override, Audio
  Mix tab). OFF is identical to the pre-tail behaviour (ST-ST-10).
- **Accepted edges (no economy impact):** the loop progress bar of a song entering by handoff can lead by
  up to one bar (`_loopStartTime` is set at Play); band BPM/performer set switch at render time, up to a
  bar before the new part is heard; a bonus-solo last loop repeats without its duck.

### 4.11 Loop chaining without cut (AUDIO-LOOP-1)

Each loop is a separate MIDI played from tick 0; the next one starts when `CompositionSession.Tick` sees
the transport stopped (2026-11-13; D1..D5 locked at batch open).

- **Cause (verified in code).** Two hard cuts sat on every loop boundary: `PlayBytes` called
  `player.Stop()` → `MPTK_Stop(stopAllSound: true)`, and Maestro's start of every MIDI
  (`ThreadInternalMidiPlaying`, also `ThreadLegacyPlay`) ran `ThreadClearAllSound(true, …)`. Both end in
  `ClearAllVoices`, which drops the active voice list at once: a crash tail or a chord release stopped dead.
  Not the generator (its end-of-part `AllSoundOff` is on the metronome channel only).
- **Seam.** `ALWTTT.Music.IMidiTransportChain` (`ChainingEnabled`, `PlayChained(bytes)`), implemented by
  `MidiToolkitAdapter`, discovered by type test like §4.9. `IPlayMidi` untouched.
- **When it chains (D2=A).** `PlayBytes` chains only when `!player.IsPlaying` — the previous MIDI ended on
  its own (loop boundary, song-tail repeat), so every note-off has been processed and only releases remain.
  Interrupting a playing MIDI (or a suspended one) keeps `Stop()` + `Play()`: a soft stop there would leave
  notes without their note-off.
- **How.** `PlayChained`: `MPTK_Stop(stopAllSound: false)`, `MPTK_ClearSoundOnStart = false` (Maestro
  patch 1), `MPTK_Play(bytes)`. Plain `Play()` sets the flag back to `true` (Maestro default). Sequencer,
  events, beat grid, markers and timing are unchanged.
- **Safety release (D3=A).** `chainTailReleaseSeconds` (default 3 s, realtime) after a chained start, the
  previous session's voices get `ThreadClearAllSound(false, previousSession)`: a forced note-off with its
  release envelope, filtered by Maestro's `IdSession`, never `ClearAllVoices`. Not cancelled by later plays
  (releasing a session with no voice left is a no-op).
- **Adapter fields.** `chainWithoutCut` (A/B: off = the old hard cut), `chainTailReleaseSeconds`,
  `logChaining` (`[AUDIO-LOOP-1] Chained play … gapMs=… voicesRinging=…` and `Safety release …`).
  `gapMs` = time from the end event to the next `PlayChained` (detection + scoring + render if any);
  `voicesRinging` reads `MPTK_StatVoiceCountActive` (`…Playing` excludes voices in release).
- **Accepted edges.** A tail keeps the channel object it started on, so live-mix writes (highlight, duck,
  balance) made after the boundary do not reach it for its last seconds. Pause still releases everything
  (`MPTK_Pause` → `ThreadClearAllSound()`), as before. The small, occasional gap between loops is NOT
  addressed: chaining is still polled per frame and a card played in the loop re-renders at the boundary
  (→ AUDIO-LOOP-2, §9).
- **Maestro patch.** `MidiFilePlayer.MPTK_ClearSoundOnStart` (default `true`, `[HideInInspector]`) guards
  the two start-of-MIDI clears; marked `[ALWTTT PATCH AUDIO-LOOP-1]`. Lost on a Maestro update: reapply from
  `Docs/reference/maestro-parches.md`.

---

## 5. Persistence

There is **no runtime save system for the designed mix**: the audio-mix balance is persisted as a
**design-time SO asset** (`AudioMixSettingsSO`), referenced by `GigManager`. Since PAUSE-1 a second,
**player** layer exists (`PlayerAudioPrefs`, PlayerPrefs, §4.8). The two never write each other:
**the player path never writes `AudioMixSettingsSO`** — in the editor, writing an SO during Play mode
mutates the asset and would leak a player choice into the shipped default.

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

This is editor-time tuning whose output ships as the runtime default. The player-facing Options
panel (PAUSE-1) does **not** edit it: it writes the player plane only (§4.8), build-time persistent via
PlayerPrefs, verified by restart in ST-P1-11 with the SO asset unchanged.

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
- **`IPlayMidi` is package-declared; the pause seam is ALWTTT-side** (PAUSE-1). ALWTTT adds
  `IMidiTransportSuspend` on its own adapter rather than extending the package interface (§4.9). No ask
  was raised; the MPTK pause API is third-party (MPTK), not MidiGenPlay.
- **The chaining seam is ALWTTT-side too** (AUDIO-LOOP-1, §4.11): `IMidiTransportChain` on the same adapter.
  The one change below the adapter is a local patch to Maestro (third-party, not MidiGenPlay); no ask to
  MidiGenPlay. The crash-on-the-downbeat idea stays a musical option in the MidiGenPlay queue (row 1b), not
  a fix.

---

## 7. Invariants

1. **One SFX sink.** All SFX (card-direct + bus-sensory) play through `AudioManager`.
   No system plays one-shots elsewhere. Master SFX caps `sfxSource` only.
2. **Two SFX keys, one authority each.** `AudioActionType` is card-only
   (`SSoT_Card_Authoring_Contracts`); `SensorySfxType` is bus-only (this SSoT). Neither
   leaks into the other. `CharacterSfxProfileSO` is an alternative clip *source* for the two
   `SensorySfxType` reaction keys, not a new key (invariant 16). Since WINK-1 (2026-08-31) it is
   also a clip source for **status-apply** audio, keyed by `StatusKey` — still no new
   `SensorySfxType` (invariant 20).
3. **Missing audio is a content gap, not a crash.** Missing profile / empty clip list →
   warn-once + no-op. OST follows the same rule via `MusicDirector` (a missing / clipless
   `OstTrackId` warns + no-op; note OST warns *per-occurrence*, not warn-once — harmless at OST
   frequency).
   **One deliberate exception (WINK-1, 2026-08-31):** a **status-apply** with no clip on either
   layer is **silent without a warn**. A status may be mute by design (most are), so warn-once
   would fire on nearly every application and train the team to ignore the channel. Recorded as a
   divergence, not normalised: every other surface keeps warn-once.
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
    passes `jitter: true`; card and single-source SFX are immediate. **WINK-1 (2026-08-31)** adds the
    second jittered handler: **status-apply**, because `StatusAppliedEvent` carries no fan-out
    index and an AoE application would otherwise stack N clips in one frame (D-WINK-3=A). `sfxMaxJitterSeconds`
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
    rejected.
    **Superseded in part by WINK-1 (2026-08-31):** the status-apply fire shipped, `CharacterSfxProfileSO`
    is no longer reaction-only, and it did **not** hook the `CharacterActionProcessor` site option B named
    — see invariant 20 and §9. Per-ability coverage is unaudited (same posture as reactions).

18. **A card sounds once, on exactly one path.** Card audio has two possible producers — the
    card-direct `AudioType` at **drop** (`CardBase.Use`) and the bus `CardVibeImpact` sting at
    **impact** (`AudienceVibeImpactEvent` → `SensoryAudioAdapter`). A card uses **one**: opting into
    the impact sting means authoring `AudioType = None` (D-PW-AUDIO; authoring rule in
    `SSoT_Card_Authoring_Contracts` §5.15). The impact sting is **once per card play, not once per
    AoE target** — `SensorySfxPresentation.ForCardVibeImpact` keys on `FanoutIndex == 0` — and is
    **immediate, never jittered** (invariant 10); the per-member *visual* fan-out carries the
    stagger. It fires on a blocked first target too (the card resolved). `CardPlayedEvent` remains
    a purely semantic event and adds no SFX key.

19. **The live music plane composes five factors, through one boundary** (R5-d; recounted SONG-TAIL-1,
    closes F-POOL-1).
    `WriteChannelVolume01` multiplies live intent × baked bytes-plane gain × solo duck × player plane
    (§4.8) × song-tail plane (§4.10). Any
    future per-channel attenuation is added as a further plane inside that method — never as a
    competing save/restore, which would collide with Highlight's single snapshot slot (§4.7 /
    §4.3).

20. **A status application sounds through two clip layers and a deliberate silence** (WINK-1,
    2026-08-31). The publish site is `StatusEffectContainer.Apply` (`StatusAppliedEvent`), which is
    the **only** point shared by both application routes — card effects
    (`CardBase.ExecuteEffects` → `trg.Statuses.Apply`) and audience actions. Resolution, in order:
    the receiving character's `CharacterSfxProfileSO.GetClipForStatus(StatusKey)` →
    `StatusEffectSO.applySfx` → **deliberate silence, no warn** (the invariant-3 exception).
    `jitter: true` on both clip layers (invariant 10). The clip plays through
    `AudioManager.PlayOneShot(AudioClip, jitter)` and adds **no new `SensorySfxType`**
    (precedent D-ABILITY-SFX-HOME=(i)); the sink stays dumb. Only applications with
    `DeltaStacks > 0` sound — a negative delta that leaves stacks standing still publishes the
    event, and announcing a loss as a gain is the failure mode that gate exists to prevent. The
    profile layer is **audience-only** today: `MusicianCharacterData` has no `sfxProfile` field, so
    musician statuses resolve `applySfx` → silence. Per-status coverage is unaudited, same posture
    as reactions and abilities.

21. **The player plane multiplies, never replaces, and never writes the SO** (PAUSE-1). Effective
    level = design × `PlayerAudioPrefs` (§4.8). Gig music applies it inside `WriteChannelVolume01` and
    re-sends through `ReapplyLiveVolumes`, so mutes and highlights are preserved.

22. **A suspended transport is still playing.** While `IMidiTransportSuspend.IsSuspended`,
    `IPlayMidi.IsPlaying` is `true` and no end-of-song event is forwarded; `Play`/`Stop` clear the
    suspension (§4.9).

23. **Pause-transparent audio is unscaled.** OST crossfades and ambience fades run on
    `Time.unscaledDeltaTime` and complete during a pause (ST-P1-12); the crowd source itself is paused in
    place (D11=C).

24. **The song tail is not a loop** (SONG-TAIL-1, D-ST0). It replays the last loop's bytes outside
    `CompositionSession` and emits no loop feedback; it never touches the session or the economy.

25. **A tail repeat keeps the last loop's baked gains.** Every repeat re-queues the BAL-1 gains
    captured at tail start; a repeat at identity gain would let the live re-assert undo the baked CC7.

26. **While a tail plays, every non-tail playback is held to the tail's next bar line** (D-ST7=C), never
    dropped and never overlapped. The held playback counts as playing (`IsAnySongPlaying`), so the session
    cannot read the wait as a loop end. Only `Stop()`, the fade, `OnDestroy` and the gig's disappearance
    end a tail; `Stop()` is final.

27. **The singer follows the tail plane and stops at the handoff** (R-V, F-ST-1). Its external level
    tracks `TailPlane01`; at the handoff the tail voice is stopped unconditionally before the new loop
    arms (`SSoT_Singer_Voice` inv. 5).

28. **A loop boundary never cuts what rings** (AUDIO-LOOP-1). When the previous MIDI ended on its own,
    the next one starts without clearing voices; tails end by themselves or by the envelope release of
    the safety net, never by `ClearAllVoices`. Interrupting a playing MIDI keeps the hard stop. Scoring,
    telemetry and «previsto = real» are untouched (transport only).

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

JUICE-PW ships ST-PW-1..10 (impact FT+SFX+anim at beat 8; FX precedes the loop resolution; FX
precedes the hold release [log order]; identical presentation outside the tutorial; Indifference →
`INDIFFERENT`, no kick; Flow-scaled delta shown; readable against the song-end FT wave; no impact
sting on non-Vibe cards [regression]; tutorial beats 3/5 intact [regression]; no scale drift on a
`scaleOnBeat` + `skipEveryNBeats` character after repeated kicks [regression]) — **all PASS**
(2026-07-13). No deferrals: ST-PW-5 ran without Dev Mode.

PAUSE-1 ships ST-P1-1..13 (2026-09-25): pause while planning; pause mid-song with the tick kept;
`_isSongPlaying` / action window after resume; loop clock; singer sync; held card returned; Esc chain
regressions ST-CF3-13 / ST-CF3-15; return-to-menu with confirmation; player volumes persisted with the
SO unchanged; unscaled fades; Dev Audio Mix design-level readout — **all PASS**; **ST-P1-3 N/A by
content** (D13=A: the composition path renders zero `part:` markers, so there is no marker crossing to
lose or duplicate). ST-P1-12 also closes AUDIO-OST's deferred **live-Music-slider-on-playing-OST** test
(the Options panel is menu-accessible).

SONG-TAIL-1 ships ST-ST-1..10 (2026-09-27/10-01): tail audible through the audience turn and composition;
singer sings the tail; no loop feedback; handoff on the bar line; held playback with a bar line already
passed; deferred singer arm; pause freezes tail, gap and handoff; tutorial beats unaffected; victory /
defeat / return-to-menu stops; OFF identical — **all PASS**. Post-close fix **F-ST-1** (tail voice kept
singing over the new song when the new part had no Melody/Lead stem) verified by T2/T3 with PLAY-TIMING-0
(T1–T4 PASS, 2026-10-01).

AUDIO-LOOP-1 (2026-11-13), played by Claudio: (1) 3-loop gig with fills, crash rings over the next
downbeat, `Chained play` log per boundary; (2) long-chord card, release without click, Big Num previsto =
real; (3) pause/resume mid-loop, resumes in place and the next loop chains — **all PASS**.

---

## 9. Forward refs / open items

- **AUDIO-LOOP-2 (registered, after the demo unless `gapMs` says otherwise):** a small, occasional gap
  between loops remains (§4.11). Render the next loop ahead and start it on a scheduled time instead of
  polling the end; shares the boundary with LASTLOOP-B (D-PLAN-14 B).

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
  DoAction` site — not built. **SHIPPED at WINK-1 (2026-08-31) — and NOT at that site.**
  See the WINK-1 bullet below for why the site moved.
- **JUICE-PW: DONE (2026-07-13)** — card **Vibe-impact** sensory surface. New bus event
  `AudienceVibeImpactEvent` (per target, published at effect resolution in `CardBase.ExecuteEffects`);
  new key `CardVibeImpact` (one sting per card play, `FanoutIndex == 0`); per-member FT + procedural
  `CharacterAnimator.PlayImpactKick` on target and performer. D-PW-AUDIO: the impact sting replaces
  the drop-time `AudioType` for such a card (Psychic Waves → `AudioType = None`). §3, invariant 18.
  ST-PW-1..10 PASS. **Clip is a placeholder (`Telephone`) → D1** (the bullet above): this is the most
  demo-visible sting in the build (the tutorial's beat-8 finisher), so it is the highest-value clip in
  the D1 backlog.
- **WINK-1: DONE (2026-08-31)** — card-play sensory beat, status-apply half. **Site reversal, recorded
  deliberately:** the deferred "option B" of this section placed the status-apply clip at the
  `CharacterActionProcessor` → `DoAction` site. That site covers **audience actions only** — statuses
  applied by cards go straight to `trg.Statuses.Apply` from `CardBase.ExecuteEffects` and would never
  have reached it, so a hook there would have shipped a sound that works for half the game and looks
  correct in review. The single point both routes share is `StatusEffectContainer.Apply`, which already
  publishes `StatusAppliedEvent`; the clip therefore rides the bus. Two clip layers (invariant 20):
  `CharacterSfxProfileSO.statusOverrides` keyed by `StatusKey` (the SO is **no longer reaction-only** —
  invariants 2 and 17 amended) and `StatusEffectSO.applySfx`. Missing clip = silence without warn
  (invariant 3 exception). No new `SensorySfxType`. `StatusAppliedEvent` grew an additive
  `StatusEffectSO Effect` field, required in the constructor so no future publisher can omit it.
  ST-W1..W7 PASS; ST-W8 deferred to TUT-REFRESH. Clips are placeholders → **D1**.
- **GameplayData.cs cleanup:** DONE — the dead `globalMusicVolume01` field was removed
  (M-AUDIO-MIX); the live home is `AudioMixSettingsSO`.
- **Player-facing audio options: DONE (PAUSE-1, 2026-09-25)** — music + SFX via `PlayerAudioPrefs`
  (§4.8). Language stays out (D-S5f-2=B). Distinct from the editor-time tuning surface.
- `SSoT_ALWTTT_MidiGenPlay_Boundary.md` — cross-project ownership.
- `SSoT_Runtime_CompositionSession_Integration.md` §3.4 — `MidiMusicManager` integration / channel routing.
- `SSoT_Dev_Mode.md` — the Dev "Audio Mix" tab surface.
- `Design_Sensory_Contract_v0_1.md` — event bus, FT presentation, §4 SFX audit, §D2
  directive (this SSoT is the audio home that §5A now points to).

# Roadmap_Audio — ALWTTT

**Status:** Active planning — audio work-stream sub-roadmap.
**Classification:** `roadmap` — **planning-only, not authority.** Implementation truth lives in `systems/SSoT_Audio.md`. This doc sequences the work and must not be treated as current-state.
**Created:** 2026-06-15 (after M-AUDIO-MIX smoke tests passed; captures the audio requirement set raised at M-AUDIO-MIX close so nothing is dropped across chats).
**Relationship:** sub-roadmap under `planning/active/Roadmap_ALWTTT.md`; analogous to the archived `M1_5_Dev_Mode_Sub_Roadmap.md`. Each batch's as-built truth is absorbed into `SSoT_Audio.md` on close.

---

## 1. Target audio architecture — the bus model

Five buses, two volume groups. The existing `AudioManager` (three `AudioSource`s, one-shots only) extends to make the implicit buses explicit. **Music** group = the global-music level; **SFX** group = the master-SFX level. This satisfies "all SFX under the SFX slider" while keeping a clean gig-vs-OST music split.

| Bus | Source / owner | Volume group | Status |
|---|---|---|---|
| **Gig music** (generated MIDI) | `MidiMusicManager` per-channel mix | Music | exists |
| **OST music** (authored clips) | `MusicDirector` (two own sources → crossfade) | Music | **exists (AUDIO-OST)** |
| **SFX** (one-shots: card + sensory) | `AudioManager.sfxSource` | SFX | exists; fixes in AUDIO-SFX-FIX |
| **Ambience** (looping crowd) | new `ambienceSource` | SFX | **exists (AUDIO-AMBIENCE)** |
| **UI** (button clicks) | `AudioManager.buttonSource` (`ButtonSoundPlayer`) | SFX *(after SFX-FIX)* | exists; not yet under the SFX slider |

Naming convention (resolves the user's gig-vs-OST ask): **"gig music"** = `MidiMusicManager` output; **"OST music"** = `MusicDirector` output. Both respond to the single **Music** level; only one OST track plays at a time.

---

## 2. Batch sequence

| ID | Scope | Depends on | Status |
|---|---|---|---|
| **M-AUDIO-MIX** | Dev Audio Mix tab + persisted balance + `SSoT_Audio.md` + ST-AM-6 highlight trigger | — | **closed** (2026-06-15) |
| **AUDIO-SFX-FIX** | default-silent SFX + fix mis-keyed `Button` profile + UI bus under SFX slider + randomized jitter (#6) | M-AUDIO-MIX | **done** (2026-06-15) |
| **AUDIO-OST** | `MusicDirector` + OST catalog SO + play/stop/crossfade-by-id + Main Menu song (#4) | M-AUDIO-MIX | **DONE (2026-06-16)** |
| **AUDIO-AMBIENCE** | looping `ambienceSource` + fade in/out + dynamic level + song-lifecycle hooks (#3) | M-AUDIO-MIX | **DONE (2026-06-16)** |
| **AUDIO-CHAR-PROFILES** (phase 1) | `CharacterSfxProfileSO` (pos/neg reaction clips) + per-character reaction resolution with per-polarity bank fallback + `PlayOneShot(AudioClip, jitter)` seam (#5) | AUDIO-SFX-FIX | **DONE (2026-06-16)** |
| **AUDIO-CHAR-PROFILES-2** (phase 2) | per-ability SFX: inline `abilitySfx` on `AudienceAbilityData`, fired at activation in `AudienceCharacterBase.AbilityRoutine` (D-ABILITY-SFX-HOME=(i)); no profile map, no musician field; status-apply fire deferred (#5 fast-follow) | AUDIO-CHAR-PROFILES (phase 1) | **DONE (2026-06-16)** |

S4 (tutorial, demo-cut milestone) is unaffected; audio batches can slot before or around it. Recommended near-term order: **SFX-FIX → OST → AMBIENCE → CHAR-PROFILES**.

---

## 3. Per-batch detail

### M-AUDIO-MIX — DONE (2026-06-15)
Dev "Audio Mix" tab (global music + per-musician + master SFX), persisted to `AudioMixSettingsSO`, loaded at gig start, re-applied per song. ST-AM-1..7 PASS (ST-AM-6 via the Solo/Duck/Clear highlight trigger; ST-AM-7 no-asset live-works with a warning). `GameplayData.globalMusicVolume01` removed (one-home cleanup). **DoD met:** balance tunable + persists + ships at a sane default (Global ≈ 0.7); docs applied.

### AUDIO-SFX-FIX — DONE (2026-06-15)
**Root cause (confirmed via log):** `CardBase.Use():91` played `PlayOneShot(AudioType)` unconditionally; un-tagged cards inherited `AudioActionType.Button` (enum 0-value) whose profile was mis-keyed to `CardSFX-HealStress`.
Shipped: `AudioActionType.None` **appended** (not `None=0` — appending avoids reordering/corrupting existing serialized AudioType ints) + `CardBase` skips `Button`/`None` → unset cards silent, tagged cards preserved; master SFX applied **app-wide at `AudioManager.Awake`** (D-SFX-APPLY=A) and `buttonSource` brought under the SFX level; jitter made **caller-controlled** (only the audience-reaction fan-out passes `jitter: true`; `sfxMaxJitterSeconds` on AudioManager, default 0.15, 0=off); diagnostic SFX log gated behind `logSfx`. Content: `ButtonSoundProfile` re-keyed off the heal clip; Draw card re-tagged off `Button`. **ST-SFX-1..8 PASS.** **Forward (AUDIO-CHAR-PROFILES):** the reaction path already carries `jitter: true`; #5 only swaps the clip source (global bank → per-character profile) — no rework.

### AUDIO-OST — DONE (2026-06-16)
New `MusicDirector` singleton owning OST playback. Shipped: `MusicDirector` (`DontDestroyOnLoad`, two owned `AudioSource`s → real crossfade, `Play`/`Stop`/`CrossfadeTo`/`RefreshMusicLevel`, scene reaction via `SceneManager.sceneLoaded` + serialized build-index map, unlisted scene → `OstTrackId.None`/stop); `OstCatalogSO` (id→clip+loop+`defaultLevel01`) keyed by new enum `OstTrackId`; `GigManager.DevSetGlobalMusicVolume01` now also calls `MusicDirector.RefreshMusicLevel()` (Music level scales gig + OST). First content: Main Menu song. There is no `AudioMixer`; OST volume is `AudioSource.volume = musicLevel01 × defaultLevel01`. Decisions locked: **D-OST-HOME=B**, **D1=A** (enum-keyed catalogue), **D2=A** (two sources, crossfade default; dormant `AudioManager.musicSource`/`PlayMusic` retired), **D3=A** (Managers/ + `sceneLoaded` map), **D4=A** (one Music level, two consumers; live-drag-on-OST deferred), **D-OST-DOCS-1=A** (asset-pipeline convention recorded in `SSoT_Audio §4.5`). **ST-OST-1..7 PASS** (ST-OST-8 + live-slider-on-OST deferred). **Forward (AUDIO-AMBIENCE):** `MusicDirector`'s fade-coroutine pattern is the reference for the ambience source.

### AUDIO-AMBIENCE — DONE (2026-06-16)
SFX-group looping crowd bed. Shipped: self-provisioned `ambienceSource` on `AudioManager` (added in `Awake`) + fade API (`FadeInAmbience`/`FadeOutAmbience`/`SetAmbienceLevel01`/`StopAmbience`); effective = `masterSfx × ambienceLevel × fade` (recomposed in `ApplyAmbienceVolume`, so `SetSfxVolume01` scales it for free); linear unscaled fades (fadeOut 0.6 / fadeIn 1.2, serialized); duck/return volume-only (loop never restarts). Gig hooks: `StartGig` FadeIn, `OnPlayPressed` duck, `OnCompositionSessionEnded` return (guarded `wasPlaying`), `OnDestroy` stop. The return hooks the explicit end callback because `OnCompositionSessionEnded` nulls `_session` synchronously inside `Tick` (polling `_isSongPlaying` would miss the end edge). Single loop (D-AMB-CLIP=A); per-venue → future `AmbienceCatalogSO`. `GigManager.cs` stays under its gig SSoT. **ST-AMB-1..8 PASS** (no deferrals). **Forward (AUDIO-CHAR-PROFILES):** independent of this; per-character SFX swaps the clip *source* for reactions.

### AUDIO-CHAR-PROFILES (phase 1) — DONE (2026-06-16)
Per-character reaction SFX. Shipped: `CharacterSfxProfileSO` (Data; pos/neg `List<AudioClip>` mirroring `SoundBankSO.SensorySoundEntry`'s clip-list + `RandomItem()` shape; `GetClipFor(SensorySfxType)` returns a random clip for `ReactionPositive/Negative`, null otherwise — reaction-only in phase 1); `AudienceCharacterData.sfxProfile` (+ `SfxProfile` getter); `AudioManager.PlayOneShot(AudioClip, bool jitter)` reusing the existing private jitter coroutine (the sink stays dumb and learns nothing about characters); `SensoryAudioAdapter.OnAudienceReaction` resolves the reacting character's profile first and falls back to the global bank **per polarity** (`jitter: true` preserved on both paths). The per-character SO is a clip *source* for the existing `ReactionPositive/Negative` keys — **no new `SensorySfxType`**; neutral stays FT-only; per-character coverage is unaudited (the `SoundBankSO` audit is the net). `MusicianCharacterData` deliberately gets no field this phase (musicians don't emit reactions; its existing `profile` is the unrelated melodic-leading config). Decisions locked: **D-CHAR-SFX=C** (phased); **SHAPE**=SO + pos/neg only (neutral FT-only) + field `sfxProfile`; **FALLBACK**=per-polarity; **SEAM**=adapter-side resolve + `AudioClip+jitter` overload. **ST-CHAR-1..7 PASS** (distinct per-character pos/neg; clean fallback for a profile-less character; per-polarity fallback on a half-authored profile; neutral-silent regression; jitter fan-out regression; single-source-cues-unchanged regression). No deferrals. **Forward → phase 2.**

### AUDIO-CHAR-PROFILES-2 (phase 2) — DONE (2026-06-16)
Per-ability SFX. Shipped: an inline `abilitySfx` `AudioClip` (+ `AbilitySfx` getter) on `AudienceAbilityData`, beside its `AbilityAnimationData` trigger; fired **once at ability activation** in `AudienceCharacterBase.AbilityRoutine`, at the animator-trigger site, via the phase-1 `AudioManager.PlayOneShot(AudioClip, bool jitter)` seam with `jitter: false`. The fire is single-source → **immediate** (jitter stays fan-out-only, invariant 10), sits *after* the stun/null/empty guards (skipped ability = silent) and *before* `PlayAbilityAnimation` so it is **independent of the animation guard** (a sound with no animator trigger still plays); a null clip no-ops in the sink (invariant 3). `CharacterSfxProfileSO` is unchanged (reaction-only) and its forward-note comment was corrected; **no `MusicianCharacterData` field** (ability fire is audience-only — musicians use card-direct `AudioActionType`, confirmed against `MusicianCharacterData` having only `cardCatalog` + the unrelated melodic `profile`). No new `SensorySfxType`. Decisions locked: **D-CHAR-SFX-2=A** (ability-level fire; option B status-apply deferred, not rejected); **D-ABILITY-SFX-HOME=(i)** (inline on `AudienceAbilityData`, not a keyed profile map — `abilityName` is a display string, not a stable key). **ST-ABIL-1..6 PASS** (correct per-character clip at activation; clean no-op when unauthored; immediate-not-jittered; fires without an animation block; reaction-path regression ST-CHAR-1..7 intact). **ST-ABIL-5 deferred** to Dev Mode / M1.5 (Stun not player-applicable; fire-site ordering correct by construction). Governance backfill (phase-1 misses, folded in): `ssot_manifest.yaml` invariant 16 + `CharacterSfxProfileSO` in governs; `CURRENT_STATE.md` §1 block. **Deferred (option B):** a status-apply clip at the `CharacterActionProcessor…DoAction` site — a parallel hook, not built.

---

## 4. Decisions ledger

**Locked (M-AUDIO-MIX):** D-VOL=B; D-AUDIO-SSOT=B; D-MIX-HOME=B (dedicated `AudioMixSettingsSO`); D-MIX-FALLBACK=B (live mix works without the asset; SO is persistence/default only); D-AUDIO-MANIFEST=yes (SSoT_Audio registered; finding F8 declares the `MidiMusicManager` mix-axis overlap).

**Locked (AUDIO-SFX-FIX):**
- **D-SFXDEF=B** — `AudioActionType.None` **appended** + `CardBase` skips `Button`/`None` (vs `None=0`, which would reorder and corrupt serialized AudioType values).
- **D-SFX-APPLY=A** — master SFX applied app-wide at `AudioManager.Awake` (vs gig-start-only).
- **D-SFX-JITTER-SCOPE=B** — jitter caller-controlled, opt-in per call; only the audience-reaction fan-out opts in.
- **D-SFX-JITTER-HOME** — `sfxMaxJitterSeconds` field on AudioManager (no new asset).

**Locked (AUDIO-OST):**
- **D-OST-HOME=B** — dedicated `MusicDirector` (vs OST methods on `AudioManager`).
- **D1=A** — `OstCatalogSO` keyed by `OstTrackId` enum `{id,clip,loop,defaultLevel01}`; scene→track map on the director, not the catalogue.
- **D2=A** — `MusicDirector` owns two `AudioSource`s, default 0.75 s crossfade; dormant `AudioManager.musicSource`/`PlayMusic` retired.
- **D3=A** — `MusicDirector` in `Managers/`; scene reaction via `SceneManager.sceneLoaded` + serialized build-index map (unlisted → `None`/stop).
- **D4=A** — one Music level (`GlobalMusicVolume01`), two consumers (gig + OST); `RefreshMusicLevel()` for live drag; live-drag-on-playing-OST test deferred (Dev tab gig-only, OST menu-only).
- **D-OST-DOCS-1=A** — OST asset pipeline recorded as a convention in `SSoT_Audio §4.5`.

**Locked (AUDIO-AMBIENCE):**
- **D-AMB-BUS=A** — ambience under the SFX group (vs its own persisted axis).
- **D-AMB-HOME=A** — `ambienceSource` + fade API on `AudioManager`; `SetSfxVolume01` recomputes it (SFX-slider scaling is free). Chosen over a dedicated director (which would have to source the SFX value out of AudioManager and be poked on every change).
- **D-AMB-FADE=B** — linear unscaled fades; `fadeOut` 0.6 s / `fadeIn` 1.2 s, both serialized (asymmetric: quick duck, gentle return; single-source ⇒ no equal-power).
- **D-AMB-HOOK=A** — duck `OnPlayPressed`, return `OnCompositionSessionEnded`, start `StartGig`, stop `OnDestroy`. Polling `_isSongPlaying` rejected for the return: `OnCompositionSessionEnded` nulls `_session` synchronously inside `Tick`, so `GigManager.Update` takes the `_session == null` early-return and bypasses the state-transition block.
- **D-AMB-CLIP=A** — single serialized loop now; per-venue → a future `AmbienceCatalogSO` (not `SoundBankSO`, which is one-shot coverage).

**Locked (AUDIO-CHAR-PROFILES phase 1):**
- **D-CHAR-SFX=C** — phased: per-character reaction SFX now (this batch), ability/status-tied SFX as a fast-follow (AUDIO-CHAR-PROFILES-2). Chosen over A (reactions only, ability work unscheduled) and B (reactions + ability in one batch), because the ability path has a real open granularity question and needs files outside the reaction batch.
- **D-CHAR-SFX-SHAPE** — `CharacterSfxProfileSO` (SO, not inline) with pos/neg reaction slots only; neutral stays FT-only (no per-member neutral noise floor); assignment field named `sfxProfile` (avoids colliding with the musician-side music `profile`). The SO declaring only pos/neg now is forward-compatible — phase 2 adds the ability map as a new serialized field with no serialization break.
- **D-CHAR-SFX-FALLBACK=per-polarity** — an empty polarity on a character profile falls back to the global bank for *that* polarity (a positive-only profile still gets the bank's negative sting); the bank's warn-once/no-op stays the safety net. Chosen over per-profile fallback, which would create silent polarities that bypass the net.
- **D-CHAR-SFX-SEAM** — resolution lives adapter-side; `AudioManager` gains a `PlayOneShot(AudioClip, bool jitter)` overload onto the existing jitter coroutine and stays the dumb sink. The per-character SO is a clip *source* for the existing `SensorySfxType` reaction keys (no new key; invariants 1/2 intact).

**Locked (AUDIO-CHAR-PROFILES-2 phase 2):**
- **D-CHAR-SFX-2=A** — ability-level fire: one clip per ability, at activation (in `AudienceCharacterBase.AbilityRoutine`, beside the animator trigger). Chosen over *status-apply* fire and *both*; the status-apply variant (a second clip when the status lands) is **deferred, not rejected** — a parallel hook at the `CharacterActionProcessor…DoAction` site, to add later if wanted.
- **D-ABILITY-SFX-HOME=(i)** — the per-ability clip lives **inline on `AudienceAbilityData`** (beside its `AbilityAnimationData` trigger), not as a keyed map on `CharacterSfxProfileSO`. Chosen because `AudienceAbilityData` has no stable ability id (`abilityName` is a rename-fragile, collision-prone display string), co-location matches the existing animator trigger, and the profile SO stays single-purpose (reaction polarity source). Palette-swap-via-one-asset (option (ii)'s benefit) is speculative at current content scale; promote later if needed (same path as `TastePreferences`→SO).

**Open:**
- none for this work-stream. Remaining: the D1 final-SFX content track (ongoing) and the deferred D-CHAR-SFX-2 option B (status-apply fire) — both optional/future.

---

## 5. Carried / folded items

- **#6 jitter** → folded into AUDIO-SFX-FIX.
- **UI bus under SFX slider** → folded into AUDIO-SFX-FIX.
- **ST-AM-6 highlight trigger** → shipped in M-AUDIO-MIX close (dev Solo/Duck/Clear in the Audio Mix tab; doubles as infra for the future highlight mechanic).
- **`GameplayData.globalMusicVolume01` dead-field removal** → M-AUDIO-MIX (edit provided).
- **D1 — final intentional, non-generic SFX** → ongoing content; the `SoundBankSO` "Audit SFX Coverage" menu + per-character profiles (AUDIO-CHAR-PROFILES) are the surfaces.
- **Player-facing audio options menu** (player-writable, build-persistent) → out of scope for this work-stream; needs a real save layer (PlayerPrefs/JSON), distinct from the editor-time mix tuning.

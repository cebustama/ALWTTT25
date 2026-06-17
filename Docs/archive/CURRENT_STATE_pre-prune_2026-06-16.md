> **SNAPSHOT — HISTÓRICO, NO AUTORITATIVO.** Copia pre-prune de `CURRENT_STATE.md` tal como estaba el **2026-06-16**, inmediatamente antes de la poda DOC-HYGIENE de §1. Este archivo **no** es el estado operativo y **no** es autoritativo — el archivo vivo es `Docs/CURRENT_STATE.md`; el historial completo por-batch está en `changelog-ssot.md`. Se conserva como snapshot de seguridad (gobernanza §15.3) porque este archivo ya fue reconstruido una vez (ver la "Recovery note" más abajo). No editar; no tratar como vigente.

---

# CURRENT_STATE — ALWTTT

This file tracks the currently validated project baseline, active work, and immediate next steps.

> **Recovery note (2026-06-12).** This file was accidentally overwritten by an insert-fragment ("Addendum para CURRENT_STATE.md §1") at some point after 2026-05-22 and was absent from project knowledge by 2026-06-01. The pre-overwrite version was recovered and the gap window (2026-05-22 → 2026-06-12) was replayed from chat-history insert blocks. Two batches in the window have **UNVERIFIED closure status** — see the `[GAP — UNVERIFIED]` stubs in §1. §1 convention going forward: new closure blocks insert at the TOP, newest-first.

---

## 1. Project foundation

### AUDIO-CHAR-PROFILES-2 — per-ability SFX — closed 2026-06-16

> Also records **AUDIO-CHAR-PROFILES phase 1** (per-character reaction SFX) as **closed 2026-06-16** — phase 1 shipped (SSoT_Audio §9 / `changelog-ssot.md` / coverage-matrix row 24) but its §1 block was omitted at the time. This block supersedes the stale "Next-active: AUDIO-CHAR-PROFILES" pointer in the AUDIO-AMBIENCE entry below, and its doc set backfills the phase-1 `ssot_manifest.yaml` miss (invariant 16 + `CharacterSfxProfileSO` governs).

Adds a per-ability one-shot SFX, fired once when an audience ability activates, beside the existing animator trigger. The clip lives **inline on `AudienceAbilityData`** (`abilitySfx`), not as a keyed map on the profile (D-ABILITY-SFX-HOME=(i)) — `abilityName` is a display string, not a stable key, and co-location matches how the animator trigger already lives on the ability. It fires in `AudienceCharacterBase.AbilityRoutine` via the phase-1 `AudioManager.PlayOneShot(AudioClip, jitter:false)` seam — single-source → immediate, after the stun/null/empty guards, before `PlayAbilityAnimation` (so a sound with no animator trigger still plays); a null clip no-ops. Audience-only: musicians act via cards (`AudioActionType`), so **no `MusicianCharacterData` field**.

**Decisions resolved.** D-CHAR-SFX-2=A (ability-level fire; option B status-apply deferred, not rejected); D-ABILITY-SFX-HOME=(i) (inline `abilitySfx` on `AudienceAbilityData`, not a profile map).

**Code.** `AudienceCharacterData.cs` — `abilitySfx` `AudioClip` (+ `AbilitySfx` getter) on `AudienceAbilityData` (additive; existing assets default null → no serialization break). `AudienceCharacterBase.cs` — one line in `AbilityRoutine` firing `AudioManager.Instance?.PlayOneShot(NextAbility.AbilitySfx, jitter: false)` before `PlayAbilityAnimation`. `CharacterSfxProfileSO.cs` — forward-note comment corrected (SO stays reaction-only; comment-only). No `AudioManager` change (reuses the phase-1 seam). `AudienceCharacterBase.cs` stays under the Audience SSoT (the audio concept is documented in SSoT_Audio §3 / invariant 17; the file is not added to SSoT_Audio governs — same precedent as the GigManager ambience hooks).

**Smoke tests.** ST-ABIL-1..6 PASS (correct per-character clip at activation; clean no-op when unauthored; immediate-not-jittered [single-source]; fires without an animation block; reaction-path regression ST-CHAR-1..7 intact). **ST-ABIL-5 deferred** to Dev Mode / M1.5 — Stun is not player-applicable in current content; the fire-site ordering (after the stun guard) is correct by construction.

**Demo-readiness.** Showable. With one authored clip (Kid → "Tantrum"), an audience ability makes its own sound at activation, in sync with its animation — a per-character beat over the generic bank. Rough edges (acceptable): clips are placeholders (D1); only authored abilities have sounds (others silent by design). Must-fix before showing: author ≥1 `abilitySfx` (all-null is silent/undemonstrative).

**Doc changes.** `SSoT_Audio.md` (§3 per-ability paragraph + reaction-para musician amend, §7 invariant 17 + invariant 16 tail, §9 phase-2 bullet, header Updated); `Roadmap_Audio.md` §2/§3/§4 (phase-2 DONE + D-CHAR-SFX-2=A / D-ABILITY-SFX-HOME=(i) locked, Open cleared); `coverage-matrix.md` (+1 per-ability row); `ssot_manifest.yaml` (SSoT_Audio **+invariant 16 backfill +invariant 17**, **+`CharacterSfxProfileSO` in governs** — phase-1 misses folded in; governs path flagged for verification); this §1 block (phase-1 omission noted above); `changelog-ssot.md`.

**Closes.** AUDIO-CHAR-PROFILES-2 (and records phase-1 closure). **Opens.** nothing new. **Next-active: S4** (tutorial / demo-cut). Audio work-stream remainder: D1 final-SFX content + the deferred D-CHAR-SFX-2 option B (status-apply fire) — both optional/future.

### AUDIO-AMBIENCE — looping crowd ambience — closed 2026-06-16

Adds a looping crowd-ambience bus under the **SFX** group (D-AMB-BUS=A): a self-provisioned `ambienceSource` on `AudioManager` with a fade API (`FadeInAmbience`/`FadeOutAmbience`/`SetAmbienceLevel01`/`StopAmbience`). Effective volume = `masterSfx × ambienceLevel × fade`, so the master-SFX slider scales it for free. Gig-driven lifecycle: crowd present while idle / during action-card play, ducks under a performing song, returns at song end, stops on gig teardown.

**Decisions resolved.** D-AMB-BUS=A (under the SFX group, not its own persisted axis); D-AMB-HOME=A (`ambienceSource` + fade API on `AudioManager`; `SetSfxVolume01` recomputes ambience — slider scaling is free; chosen over a dedicated director that would source the SFX value out of AudioManager and be poked on every change); D-AMB-FADE=B (linear unscaled fades; fadeOut 0.6 s / fadeIn 1.2 s, serialized; asymmetric quick-duck + gentle-return; single-source ⇒ no equal-power); D-AMB-HOOK=A (duck `OnPlayPressed`, return `OnCompositionSessionEnded`, start `StartGig`, stop `OnDestroy` — polling `_isSongPlaying` rejected for the return because `OnCompositionSessionEnded` nulls `_session` synchronously inside `Tick`, so `GigManager.Update` takes the `_session == null` early-return and bypasses the state-transition block); D-AMB-CLIP=A (single serialized loop now; per-venue is a future `AmbienceCatalogSO`, not `SoundBankSO`).

**Code.** `AudioManager.cs` — ambience source + fade API + `masterSfx × ambienceLevel × fade` composite; one line in `SetSfxVolume01` so the SFX slider scales it; null clip → warn-once + no-op. `GigManager.cs` — four surgical hooks: `StartGig` (FadeIn), `OnPlayPressed` (FadeOut/duck), `OnCompositionSessionEnded` (FadeIn/return, guarded by `wasPlaying`), `OnDestroy` (StopAmbience). Duck/return are volume-only (loop never restarts → no system transient). `GigManager.cs` stays under its gig SSoT (not added to SSoT_Audio governs — same precedent as the AUDIO-OST `DevSetGlobalMusicVolume01` hook). Unity-side: assign a looping crowd clip to `ambienceClip` on the AudioManager component (source is created in code; no new asset).

**Smoke tests.** ST-AMB-1..8 PASS (loops idle; ducks on song start; returns at song end; stays ducked across parts [ST-AMB-4 regression]; SFX slider scales it; existing card/UI one-shots intact [ST-AMB-6 regression]; no click/pop at duck/return; no bleed into the menu/reward scene). No deferrals — ambience and the Dev Audio Mix tab are both gig-scoped, so the slider test was reachable.

**Demo-readiness.** Showable. A viewer walks into a gig and hears a live crowd murmuring while planning + playing action cards; the crowd ducks the instant the band starts the song, then swells back when it ends; the SFX slider rides all of it. Rough edges (acceptable): one placeholder crowd loop (no per-venue beds — D-AMB-CLIP=A forward path); loop-seam quality is a clip property. Hard gate: no crowd bleed into a non-gig scene (ST-AMB-8).

**Doc changes.** `SSoT_Audio.md` (§3 ambience subsection, §2 ownership row, §7 invariant 15, §8 smoke ref, §9 forward-ref move, header Updated); `Roadmap_Audio.md` §1/§2/§3/§4 (AUDIO-AMBIENCE DONE + D-AMB-HOME/FADE/HOOK/CLIP locked, D-AMB-BUS moved to Locked); `ssot_manifest.yaml` (SSoT_Audio +invariant 15); `coverage-matrix.md` (+1 ambience row); this §1 block; `changelog-ssot.md`.

**Closes.** AUDIO-AMBIENCE. **Opens.** AUDIO-CHAR-PROFILES. **Next-active: AUDIO-CHAR-PROFILES** (`CharacterSfxProfileSO`; per-character reaction/ability SFX; D-CHAR-SFX shape open), then S4.

### AUDIO-OST — OST music playback — closed 2026-06-16

Adds an OST (authored-clip) music bus: a `MusicDirector` singleton that owns OST playback, an `OstCatalogSO` keyed by the new `OstTrackId` enum, and Main Menu song wiring. One OST track audible at a time; output scaled by the **Music** level (`AudioMixSettingsSO.GlobalMusicVolume01`) — the same level that scales gig music. Gig-vs-OST stays two managers (`MidiMusicManager` = gig music, `MusicDirector` = OST), one Music level; they never play music simultaneously.

**Decisions resolved.** D-OST-HOME=B (dedicated `MusicDirector`); D1=A (`OstCatalogSO` keyed by `OstTrackId`; scene→track map on the director); D2=A (two owned `AudioSource`s, 0.75 s crossfade default; dormant `AudioManager.musicSource`/`PlayMusic` retired); D3=A (`Managers/`; scene reaction via `SceneManager.sceneLoaded` + serialized build-index map; unlisted scene → `OstTrackId.None`/stop — this is the no-gig-overlap mechanism); D4=A (one Music level, two consumers; `RefreshMusicLevel()` called from `GigManager.DevSetGlobalMusicVolume01`); D-OST-DOCS-1=A (OST asset pipeline recorded in SSoT_Audio §4.5). No `AudioMixer` in ALWTTT — OST volume is `AudioSource.volume = musicLevel01 × defaultLevel01`.

**Code.** New `OstTrackId.cs` (Enums), `OstCatalogSO.cs` (Data/Audio), `MusicDirector.cs` (Managers). `GigManager.cs` (one line in `DevSetGlobalMusicVolume01` → `MusicDirector.Instance?.RefreshMusicLevel()`). Optional one-home cleanup: `AudioManager.cs` dead `PlayMusic` + `musicSource` field removed (zero callers) — apply if not already. Unity-side: `OstCatalog` asset (MainMenu → clip, loop, level 1.0); `MusicDirector` GameObject in the Main Menu scene with catalogue + the shared `AudioMixSettings` + binding `{0, MainMenu, Crossfade}`.

**Smoke tests.** ST-OST-1..7 PASS (menu plays/loops on entry; one-at-a-time; stops on scene change with no gig overlap; return-to-menu re-arms; Music level scales OST; gig music intact; missing-clip no-crash). ST-OST-8 (true two-track crossfade) DEFERRED (single OST track in content). Live Music-slider-on-playing-OST DEFERRED (Dev Audio Mix tab is gig-only; the only OST plays in the menu).

**Demo-readiness.** Showable. A viewer launches the game and hears a looping Main Menu theme that fades out on Start (no OST under the gig) and returns on ESC-to-menu; the Music level scales it. Rough edges (acceptable): one OST track only (no track-to-track crossfade content yet); no player audio-options menu (Dev tab is gig-only / editor-time, needs a save layer). Hard gate satisfied: no OST audible during a gig (ST-OST-3).

**Doc changes.** `SSoT_Audio.md` (§2 ownership rows, new §4.5 OST bus, §7 invariants 12–14 + #3 OST clause, §8 smoke ref, §9 forward refs, header Scope/Owns/Updated); `Roadmap_Audio.md` §1/§2/§3/§4 (AUDIO-OST DONE + D1–D4/D-OST-DOCS-1 locked); `ssot_manifest.yaml` (SSoT_Audio governs: +3 files, +3 invariants); `coverage-matrix.md` (+1 OST concept row); this §1 block; `changelog-ssot.md`.

**Closes.** AUDIO-OST. **Opens.** AUDIO-AMBIENCE. **Next-active: AUDIO-AMBIENCE** (looping crowd ambience, SFX group, D-AMB-BUS=A), then AUDIO-CHAR-PROFILES, then S4.

### AUDIO-SFX-FIX — SFX correctness + scoped jitter — closed 2026-06-15

Fixes the heal-sound-on-every-card bug and reaction saturation; brings UI clicks and the app-wide SFX level under the mixer. Root cause (confirmed via log): `CardBase.Use()` played `PlayOneShot(AudioType)` unconditionally; un-tagged cards inherited `AudioActionType.Button` (enum 0-value) whose profile was mis-keyed to `CardSFX-HealStress`.

**Decisions resolved.** D-SFXDEF=B (append `AudioActionType.None`; `CardBase` skips `Button`/`None` → unset cards silent, existing serialized ints unchanged — chosen over `None=0`, which would reorder and corrupt serialized values); D-SFX-APPLY=A (master SFX applied app-wide at `AudioManager.Awake`, not gig-start-only — the Main Menu has no GigManager); D-SFX-JITTER-SCOPE=B (jitter caller-controlled, opt-in per call; only `SensoryAudioAdapter`'s reaction fan-out passes `jitter: true`; card + single-source SFX immediate); D-SFX-JITTER-HOME (`sfxMaxJitterSeconds` field on AudioManager, default 0.15, no new asset).

**Smoke tests.** ST-SFX-1..8 PASS (no card sound unless tagged; tagged card still sounds; menu Start no longer heals; UI clicks scale with the SFX slider; reactions staggered; jitter-off immediate; card SFX immediate; M-AUDIO-MIX slider regression intact). ST-SFX-2 was a content gap (Draw card mis-tagged `Button`), not a code fault.

**Content.** `ButtonSoundProfile` re-keyed off the heal clip; Draw card re-tagged off `Button`.

**Doc changes.** `SSoT_Audio.md` §3 (opt-in-by-type + jitter + UI-under-SFX), §4.4 (app-wide SFX correction), §7 (+3 invariants); `Roadmap_Audio.md` §2/§3/§4 (AUDIO-SFX-FIX done + decisions locked); this §1 block; `changelog-ssot.md`.

**Closes.** AUDIO-SFX-FIX. **Opens.** AUDIO-OST. **Next-active: AUDIO-OST** (Main Menu song; D-OST-HOME=B), then AUDIO-AMBIENCE / AUDIO-CHAR-PROFILES, then S4.

### M-AUDIO-MIX — music-mixing tooling + persisted balance + SSoT_Audio — closed 2026-06-15

Adds a centralized Dev "Audio Mix" tab (global music + per-musician + master SFX), a persisted balance (`AudioMixSettingsSO`) loaded at gig start and re-applied per song, and creates `systems/SSoT_Audio.md` (consolidating Sensory Contract §5A + the music-mix model + the audio boundary). Solves "music sometimes too loud" via a tunable balance shipped at a sane default (Global ≈ 0.7).

**Decisions resolved.** D-VOL=B (tab + persist); D-AUDIO-SSOT=B (create SSoT_Audio); D-MIX-HOME=B (dedicated `AudioMixSettingsSO` holds global + master SFX + per-musician list; `GameplayData.globalMusicVolume01` migrated, single reader redirected, dead field removed); D-MIX-FALLBACK=B (live mix works without the asset; SO is persistence/default only); D-AUDIO-MANIFEST=yes (SSoT_Audio registered; finding F8 declares the MidiMusicManager mix-axis overlap); boundary (hard) MIDIInstrumentSO.volume01 NOT wired (instrument01 == 1.0).

**Smoke tests.** ST-AM-1..5 PASS (slider→channel mapping; global scales all music; SFX scales only effects; persistence + gig-start load; cross-song persistence). ST-AM-6 (highlight override/restore) PASS via the new Dev Solo/Duck/Clear trigger. ST-AM-7 (no-asset) works live with a "won't persist" warning.

**Demo-readiness.** Showable for tuning, not a player feature. A viewer (dev) opens F12 → Audio Mix, dials the balance live, and it ships baked. Rough edges (acceptable): editor-time persistence only (no player audio-options menu — that needs a save layer); `PersistAudioMixInEditor` saves on every drag frame (polish later).

**Doc changes.** Created `systems/SSoT_Audio.md` + `planning/active/Roadmap_Audio.md`; `SSoT_INDEX.md` (+2 rows); `ssot_manifest.yaml` (+SSoT_Audio entry, +F8, +Roadmap_Audio); `Design_Sensory_Contract_v0_1.md` §5A → pointer; `coverage-matrix.md` (+3 rows); `SSoT_Dev_Mode.md` (4-tab + Audio Mix subsection + code-map); `SSoT_Gig_Combat_Core.md` (§12 audio-mix cross-ref note); this §1 block; `changelog-ssot.md`.

**Closes.** M-AUDIO-MIX. **Opens.** Audio work-stream (see `Roadmap_Audio.md`). **Next-active: AUDIO-SFX-FIX** → AUDIO-OST → AUDIO-AMBIENCE → AUDIO-CHAR-PROFILES, then S4.

### S3-audio — Audio SFX layer — closed 2026-06-14 (placeholder clips)

Lands the audio half of D2 for card-play and the three bus surfaces, end to end. The bus is now
the sole sensory fan-out for FT *and* audio (independent handlers). Audio is the FT→SFX upgrade;
the FT floor was already met everywhere. Closes the S3 umbrella.

**Decisions.** D-SA-1=A (flesh out the existing `AudioManager`, not greenfield — `AudioActionType`
+ `AudioManager.PlayOneShot` were already in code); D-SA-2=A (infra-then-assets; placeholder tones
rejected per D1); D-SA-3 (event→`SensorySfxType` mapper); D-SA-4=A (one `AudioManager` sink, two
callers — card-direct + bus); D-SA-5=A (`SfxStageCrossedEvent`, published after VFX+bonus; VFX
stays direct, D-S3-6=A); D-SA-6=A (separate `SensorySfxType`, not an `AudioActionType` extension);
D-SA-7=B (`SoundBankSO` central inventory + `Audit SFX Coverage`).

**Code.** New `SoundBankSO`, `SensorySfxType`, `SensorySfxPresentation`, `SensoryAudioAdapter`,
`SfxStageCrossedEvent`. `AudioManager` (bank-backed; card + sensory dicts; `PlayOneShot(SensorySfxType)`;
null-safety fix — was a live NRE; warn-once-per-type). `GigManager.FireSongHypeStage` publish.
`CardBase.Use` card-play audio (corrected from the mis-targeted `AddVibeAction`, which was reverted;
redundant card-context calls recommended for removal from `AddStress`/`BlockStressAction`,
audience-context branches kept).

**Smoke tests.** ST-SA-1..9 PASS (infra; ST-SA-6 stage-publish + ST-SA-9 card-audio-layer fixed
mid-batch). ST-SA-A1..A4 PASS (audible, placeholder clips). Coverage audit caught a `SoundProfileData`
mis-keyed internally ("Heal Vibe SFX" not set to `HealVibe`); fixed.

**Demo-readiness.** Showable with sound. A viewer now hears card plays, crowd-reaction stings, a
song-end vibe flourish, and escalating lights→smoke→fire stage stings, over the procedural song.
Rough edges (acceptable, documented): SFX are **placeholders** (final authoring = D1 follow-up);
**music sometimes too loud** (D-VOL=B batch next); smoke/fire static pop-in; no audience-ability audio.

**Doc changes.** `Design_Sensory_Contract_v0_1.md` §3 + §4 + new §5A; this §1 block + §3 row +
sequence-status + §4 open items; `changelog-ssot.md`. `ssot_manifest.yaml`, `coverage-matrix.md`,
`SSoT_INDEX.md`, `SSoT_Card_Authoring_Contracts.md`, `SSoT_Card_System.md`, `Roadmap_ALWTTT.md`,
other SSoTs intentionally unchanged (audio subsystem standalone like the bus; `audioType` already
a documented card field).

**Closes.** S3-audio (and the S3 umbrella). **Next-active: music-mix batch (D-VOL=B) — which also
creates `SSoT_Audio.md` (D-AUDIO-SSOT=B) — then S4.** **Tracked follow-ups:** final intentional
SFX (D1); smoke/fire appear/loop animation; stale `LoopScoreCalculator.cs` refresh +
`LoopScoringConfig`/`HypeThresholds`.

### S3a — Sensory polish (visual) — closed 2026-06-14

S3a completes the sensory migration started in S2 and lands the visual sensory layer. The bus is now the sole FT source (both GigManager direct FxManager calls deleted, grep-confirmed; `SensoryFxAdapter` in Spawn). Audio (the "SFX" half of D2) is split out to S3-audio per D-S3-1=B.

**Decisions resolved.** D-S3-1=B (audio → dedicated S3-audio batch); D-S3-2=A (neutral "…" legibility via a TMP outline material on the FloatingText prefab — prefab-side, applied in Unity); D-S3-3=A (per-handler exception isolation on `SensoryEventBus.Publish`); D-S3-4=A (migration before audit); D-S3-5=A (song-end FT uses the int `SpawnFloatingText` overload so the S1 random-diagonal drift is preserved — the Vector2 overload would force straight-up); D-S3-6=A (stage VFX dispatch to per-venue `BackgroundRoot.SetSmoke/SetFire`; stage crossings stay on the direct `ActivateSFX` path — no `SfxStageCrossedEvent`); D-F-5a (Kid "Tantrum" ability animator trigger restored in `AudienceCharacterBase.AbilityRoutine`); **D-F-5b deferred** (per-loop *reaction* animator — no reaction animator state exists; only the ability trigger "Tantrum" does); D-S3-7=A (per-song `StartingSongHype` seeded silently — stage crossings + the SFX→Vibe bonus fire only on performance-driven hype above the seed, not during prep); D-S3-8=A (both Dev SongHype sliders now route through `DevSetSongHypeAbsolute` and fire crossings — a pre-existing Dev regression from the `AddSongHypeCore` split, not introduced by S3a).

**Code.** `GigManager.cs` (two direct FT calls + dead `ImpressionToExclamation`/`ImpressionToColor` wrappers removed; `SeedSongHype`/`StageForNormalized` added; Dev setters route through `DevSetSongHypeAbsolute`); `SensoryFxAdapter.cs` (default Mode=Spawn; song-end int overload); `SensoryEventBus.cs` (per-handler try/catch in `Publish`); `AudienceCharacterBase.cs` (`PlayAbilityAnimation` + call in `AbilityRoutine`); `BackgroundRoot.cs` (`smokeRoot`/`fireRoot` + `SetSmoke`/`SetFire`); `BackgroundContainer.cs` (`ActivateSFX` smoke/fire dispatch; `DeactivateAllSFX` clears all three). Unity-side: adapter Mode=Spawn; FloatingText outline material; per-venue smoke/fire roots assigned.

**Smoke tests.** ST-S3-1..13 + ST-S3-4 ALL PASS. Highlights: reaction + song-end FT parity with the direct calls gone (no double/missing); song-end random-diagonal drift preserved; bus isolation (a throwing subscriber does not suppress the adapter's FT); Kid Tantrum animation on its ability turn; lights→smoke→fire on performance crossings; seed no longer fires VFX/Vibe pre-performance (low-threshold repro); Dev sliders fire crossings.

**Demo-readiness.** Showable. A viewer sees crowd-reaction pop-ups, "+N Vibe" at song end, the Kid's tantrum, and escalating stage lights→smoke→fire as a song builds. Rough edges (acceptable for demo): smoke/fire are static sprite pop-ins (no animation yet — TODO in Sensory Contract §5); audio SFX absent (S3-audio); reaction-driven animator not wired (only the ability trigger exists).

**Doc changes.** `Design_Sensory_Contract_v0_1.md` §4 (full audit fill + SFX-terminology note) + §5 (S3 as-built + smoke/fire animation TODO + `SfxStageCrossedEvent` deferred); `SSoT_Audience_and_Reactions.md` §5.1 (migration → bus-only as-built; neutral-colour descriptor corrected); this §1 block; `changelog-ssot.md` (S3a entry).

**Closes.** S3a. **Opens.** S3-audio. **Next-active: S3-audio, then S4.**

### S2 — Sensory Event Bus foundation — closed 2026-06-14

S2 introduces a typed sensory event bus between gameplay and its visual/audio consumers, so future feedback (SFX, animators, tutorial) attaches as subscribers instead of being hard-wired at each gameplay site. Two scope adjustments vs the original plan: TutorialController deferred to S4, and direct-call deletion deferred to S3 (S2 is coexistence).

**Decisions resolved.** D-S2-1=A (MonoBehaviour singleton, static accessor, FxManager pattern); D-S2-2=A (two event types only: `AudienceReactionEvent` + `SongEndVibeEvent`); D-S2-3=A (coexistence — bus publishes fire alongside the retained direct FxManager calls; deletion is S3); D-S2-4=A (`ISensoryEvent` marker interface + `readonly struct` events + zero-alloc generic `Publish<T>`); D-S2-5=defer (TutorialController consumer → S4); D-S2-6=A (thin `SensoryFxAdapter`; FxManager stays a dumb spawner); D-S2-7=A (adapter VerifyOnly + impression/song-end FT mapping single-sourced into `SensoryFtPresentation` so the two emission paths cannot drift before S3); D-S2-INIT=C (init-order hardening — lazy auto-creating bus accessor + `[DefaultExecutionOrder(-100)]` + init-confirmation logs; a refinement of D-S2-1=A, fixing an intermittent silent non-subscription race).

**Code.** New `Assets/Scripts/Sensory/` — `ISensoryEvent.cs`, `SensoryEventBus.cs`, `SensoryFtPresentation.cs`, `SensoryFxAdapter.cs`; `Sensory/Events/` — `AudienceReactionEvent.cs`, `SongEndVibeEvent.cs`. `GigManager.cs` 4-edit patch (using directive; parallel publish at the loop-reaction site; impression mapping delegated to `SensoryFtPresentation`; song-end event + single-sourced FT + publish). Direct `FxManager.Instance` spawn calls retained at both sites (coexistence; grep-verified present). Scene: `SensoryEventBus` on the managers object; `SensoryFxAdapter` (VerifyOnly) on the Listeners object in the Gig Scene.

**Smoke tests.** ST-S2-1..7 PASS; ST-S2-9 PASS (init determinism across repeated play sessions/reloads — retired the intermittency). ST-S2-8 N/A (its "bus absent in play mode" premise is unreachable under D-S2-INIT=C; the publish-site null-guard remains as defensive dead code). Throwaway `TempSecondSubscriber` used for ST-S2-7 then deleted.

**Demo-readiness.** Unchanged — still showable. S2 is infrastructure; in VerifyOnly mode it produces no new on-screen output (FT still from the S1 direct calls), so the player-visible build is bit-identical to S1.

**Doc changes.** `Design_Sensory_Contract_v0_1.md` §3 (as-built; coexistence + consumer-table corrections) + §4 (two rows flagged bus+direct; full audit descoped → S3); this file §1/§3/§4/§5; `changelog-ssot.md` (S2 entry + S1 reordered above CE-L1 + S1 neutral-RGB correction).

**Closes.** S2. **Opens.** S3 (sensory polish).

### CE-L1 — LLM-assisted card authoring — landed 2026-06-11 (docs integrated 2026-06-12)

The Card Editor gained a "Generate with LLM" panel (`CardEditorWindow.LLM.cs` + the editor-only `Assets/Scripts/Cards/LLMAuthoring/` asmdef pair), shipped from the MidiGenPlay CE-L1 cross-project batch. The LLM fills structured fields only; asset refs are hard-banned; palette is chosen via seeded intent (`CardPaletteIntentResolver`); modifier effects are referenced by exact name; bundle + palette are written at the existing Save step (`ApplyLlmPlanOnSave`); the sprite is the musician default. Compiles, 77 tests green, first live smoke passed (`cmp_flow_rhythm`, palette 'Syncopated Pocket (4/4)', seed 12345, 1572/366 tokens).

Docs integrated 2026-06-12: `SSoT_Editor_Authoring_Tools.md` §4.10, `SSoT_Card_Authoring_Contracts.md` §5.12 (incl. the route-scope finding — batch JSON parses but does not resolve `composition.palette`; code-verified), `reference/Report_CardLLM_Pipeline.md` adopted, `changelog-ssot.md` + `coverage-matrix.md` + `ssot_manifest.yaml` updated.

**Active focus:** CE-L1 expressivity smoke validation (5 briefs + seed-variation reruns); findings note to the MidiGenPlay project. No code changes in that batch. This is a cross-project tooling interlude outside the S1–S8 sequence (§3).

### [GAP — UNVERIFIED] ALWTTT-PCE-PROP — drum-palette propagation (2026-06-04/05)

**Reconstructed from chat history ("Cross-project rhythm card palette propagation"); closure NOT confirmed.** MidiGenPlay closed its PCE phase (2026-06-04); the propagation batch ran on the ALWTTT side. Verified facts from the session: decisions D1=A (authoritative card→palette table lives in `SSoT_Card_System.md` §5.2.1; package mirrors), D2=A (MidiGenPlay referenced as `file:` UPM package — no code sync), D3=A (determinism = deterministic per build, package-threaded seed) locked; six paste-ready doc blocks produced (Card System §5.2.1 table, Integration §7.1 + invariant 10, Starter_Deck §5.5 correction, changelog entry, coverage-matrix row, CURRENT_STATE §1 block); bindings: Default Mode→FourOnTheFloor (asset was mis-set to SyncopatedPocket — fix required), Waltz Protocol→WaltzLilt, Pentameter→OddMeterAngular, Compound Cycle→CompoundSwing; SyncopatedPocket unbound. **Open at session end:** D-TEMPO (Push It / Half Time palette or null; recommendation null/A); paste + ST-1..ST-5 verification. **TO CONFIRM:** were the six blocks pasted, the Default Mode asset fixed, ST-1..5 run, D-TEMPO answered? If yes, replace this stub with that session's CURRENT_STATE §1 block.

### S1 — B3-slate-F: audience reactions become real — closed 2026-06-12

S1 is the first implementation batch post-reframe. Outcome differed from the batch-open expectation: `ResolveLoopEffect`, the taste schema, the GigManager routing, and the macro-Vibe modifier had already shipped in a prior B3-slate code batch (tagged [B3 D2=A]/[B3 D3=A] in code) but were never ratified into SSoTs. S1 = ratification + one small additive code change + doc backfill.

**Decisions resolved.**
- D-F-1=A — 4-axis discrete-count impression algorithm (TempoScale / role count / TimeSignature / Tonality), clamped [−2, +2]. RootNote intentionally excluded.
- D-F-2=A — Indifference gates song-end conversion only (`ApplyIncomingVibe`); loop impressions stay live and visible.
- D-F-3=β — macro modifier as impressionFactor = 1 + avg×0.25 ∈ [0.5, 1.5], multiplied on SongHype-derived baseVibe at `ComputeSongVibeDeltas`.
- D-F-4=A — word-exclamation FT (WOW!/YEAH/…/MEH/BORING); neutral "…" added.
- D-F-5=defer — audience reaction animator trigger deferred to S3.

**Code change.** `GigManager.cs` only — `ImpressionToExclamation` (impression=0 → "…", was null → no FT; defensive fallthrough also "…") + `ImpressionToColor` (neutral = muted grey 0.55/0.55/0.55, DARKER than MEH light grey 0.80/0.80/0.80 — corrected 2026-06-14 at S2 close; originally mis-recorded here as light cool grey 0.75/0.75/0.78). Closes the Sensory Contract gap: every loop impression now emits FT, including neutral.

**Smoke tests.** ST-S1-1..8 ALL PASS. Highlights: 3-audience differentiation on the same loop (Cool Dude WOW! vs 2× Kid YEAH on Waltz Protocol + 1 Sibi track); Indifference per D-F-2=A (loop FT live, song-end INDIFFERENT floater, no bar fill); Convinced regression clean; neutral "…" verified via Half Time + 1 Sibi card (Kid raw=0). Known rough edge: neutral "…" legibility against busy backgrounds — FT outline/shadow deferred to S3 sensory polish. Animator + SFX deferred to S3 per D-F-5 / D2 exception.

**Content note.** Cool Dude's TS preferred list includes 6/8 + 7/8 — dead weight in the current starter (only Waltz Protocol 3/4 fires). Harmless, intentional; not a defect.

**Doc changes.** `SSoT_Audience_and_Reactions.md` (§6 replaced, §5.1 rewritten, §5.3 added); `SSoT_Scoring_and_Meters.md` (§6.1 added); `Design_Sensory_Contract_v0_1.md` §4 audit table (audience reaction row added; vibe-change row qualifier removed); `changelog-ssot.md` top entry; this block.

**Process note.** Batch-open premise was stale — prior code batch closed without SSoT backfill, and the stale claim propagated into planning. Standing rule reinforced: no batch closes with code merged and SSoT pending.

### Planning reframe (2026-05-23) — DEMO CUT (S1-S5) + VERTICAL SLICE (S6-S8) — DOCUMENTATION-ONLY closure

Planning batch (PLANNING-REFRAME-2026-05-23). Pure docs, no code. User-side planning session on 2026-05-23 surfaced (a) new asset drop (4 new audience sprites, 2 venue references, pilot/manager character, smoke/fire VFX) and (b) tutorial coverage as a demo-cut blocker. Plan reframed from 4-session demo-cut polish to 8-session sequence across two milestones: demo cut (S1-S5) + vertical slice (S6-S8 = Phase C).

**11 decisions locked at reframe.**
- **D-TUT-1** = basic mechanics only, extensible infra.
- **D-TUT-2** = skip button + revisitable from pause menu.
- **D-TUT-3** = first-time trigger model (`HashSet<string> firedDialogs`, persisted via PD).
- **D-TUT-4** = portrait box (asset image 1) + dialog box, Neow-style.
- **D-TUT-5** = tutorial also in vertical slice (~5 extra dialogues spread S6-S8).
- **D-RUN-1** = A (narrow demo cut now; vertical slice as Phase C).
- **D-RUN-2** = β (ship-stub as dedicated S6, not folded into S5).
- **D-RUN-3** = 3 + boss ideal, 2 + boss minimum.
- **D-RUN-4** = α (boss reuses `AudienceCharacterBase`; bespoke abilities only; no new character class).
- **D-RUN-5** = image 1 confirmed as ship pilot / band manager.
- **D-RUN-6** = ship interior, space map, meta-progression, audio pass — out of the 8-session plan.

**Two new standing directives** declared at reframe and promoted on declaration. Added to `planning/Design_Project_Directives_v0_1.md`:
- **D2 Sensory Contract** — every player-visible state change produces at minimum FT; FT + SFX preferred; FT + SFX + animator/shader/particle ideal.
- **D3 Tutorial-as-mandatory** — every demo-cut feature has tutorial coverage by S4 closure; every Phase C feature by S8 closure.

Pre-existing D1 (Sound Design Priority) remains as Standing Directive #1.

**Three new planning docs shipped (planning/active/).**
- `Design_Tutorial_System_v0_1.md` — tutorial scope, trigger model, presentation, UX, demo-cut + Phase C dialogue inventory, DoD.
- `Design_Vertical_Slice_v0_1.md` — Phase C scope: ship hub, venues, audience archetypes + state machine, boss design, scene transitions.
- `Design_Sensory_Contract_v0_1.md` — Sensory Contract operational expansion: bus design, audit table placeholder, smoke/fire VFX integration plan for S3, consumer inventory.

**Four existing docs updated.**
- `Roadmap_ALWTTT.md` — §5.3 sequencing addendum (B3-slate decomposition across S1-S5); §5.5 DoD gains tutorial coverage criterion; new §7 Phase C section with DoD.
- `Design_Demo_Cut_v1.md` — status line refreshed; §2.4 Tutorial coverage row added; §5.1 Standing Directive #3 criterion added.
- `Design_Project_Directives_v0_1.md` — D2 + D3 directives appended; "Future directives" placeholder updated to reflect three standing directives.
- `CURRENT_STATE.md` — this §1 block + §3 next-active rewrite.

**Two index/log docs updated.**
- `SSoT_INDEX.md` — three new planning-active doc entries registered.
- `changelog-ssot.md` — top entry for the planning reframe.

**No code changes. No SSoT contract changes. No `ssot_manifest.yaml` change. No `coverage-matrix.md` change** (all three new docs are planning-classification, not authority).

**S1 (B3-slate-F audience reactions) opens immediately on next batch.** Original 2026-05-22 closure note about B3-slate-F being next-active is preserved by this reframe — S1 = B3-slate-F. The rest of B3-slate decomposes into S5 cleanup or explicit deferral per the §5.3 sequencing addendum.

### Phase B B3 follow-on — ALWTTT-MOD-DIR-2 + ALWTTT-MOD-DIR-3 — complete (2026-05-22)

Audible-polish follow-on to B3-content-cards' Key Lift card. Two cross-project batches landing together on the ALWTTT side, accompanied by three companion closures on the MidiGenPlay side (MGP-ALWTTT-MOD-DIR-1, 1.1, 1.2). Together they ship a directional intent for `ModulationEffect`: cards can now author Up/Down direction for the first chord of the post-modulation render, and the audible result matches the authored intent.

**ALWTTT-MOD-DIR-2** — adopted the MidiGenPlay directional surface (six decisions resolved: D-A1=A reuse package `MidiGenPlay.Composition.ModulationOctaveHint` enum directly; D-A2=A capture previous root locally in `ApplyEffectToModel` before mutation; D-A3=B stage on `PartEntry`, write+clear at `SongConfigBuilder`; D-A4=no `CompositionCardClassifier` change; D-A5=A expose `octaveHint` on SO inspector with default `Auto`; D-A6=B append glyph suffix in `GetLabel()` for non-`Auto` direction). `ModulationEffect.cs` gained `octaveHint : ModulationOctaveHint` field; `SongCompositionUI.PartEntry` gained two `[NonSerialized]` staging fields; `SongConfigBuilder.Build()` writes-and-clears the staged transients onto `PartConfig` at the sole handoff site. `ModulationEffect_KeyLift_Degree5.asset` re-authored with `octaveHint = Up`.

**ALWTTT-MOD-DIR-3** — fixed a cache-layering bug discovered during SM-DIR-5 verification. `MidiMusicManager._partBundleCache` keys on `partMeterHash` + per-track input hashes, which does not include the `[NonSerialized]` `PartConfig` transients. Same-root modulations (degree=Tonic + non-Auto hint) hit the cache and replayed pre-modulation bytes verbatim, silently dropping the directional intent. Fix: `RenderSinglePart` forces `cacheEnabled = false` when either transient is non-default. Bypass is one-shot — composer consumes and clears the transients in the same call, so a subsequent Auto render caches and replays normally. New `[Mod-DIR/CacheBypass]` log (gated on `logDebug`) provides production observability for future cache investigations.

**Smoke tests.** SM-DIR-1 (strict Up Dominant) PASS. SM-DIR-2 (strict Down Dominant) PASS. SM-DIR-3 (Auto regression, bit-identical baseline) PASS. SM-DIR-4 (chained Up modulations) PASS. SM-DIR-5 (Tonic + Up register bump) PASS after the cache-bypass fix. SM-DIR-6 (transients clear after consumption) PASS. SM-DIR-7 (range-clamp fallback) deferred — requires narrow-range debug instrument tooling not yet present.

**Demo impact.** Key Lift now audibly lifts. Pre-batch the card shifted pitch class (C → G) but the voice leader picked minimum-distance octave, so "Up a fifth" could land G3 below the previous C5 tonic — directionally ambiguous. Post-batch the first chord of the modulated render lands strictly above the previous tonic, matching the card's authored name and player intuition. No deck content changes beyond the asset re-author.

**Files modified.** `ModulationEffect.cs`, `SongCompositionUI.cs`, `SongConfigBuilder.cs`, `MidiMusicManager.cs`, `ModulationEffect_KeyLift_Degree5.asset`, `integrations/midigenplay/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md`, `changelog-ssot.md`. No SSoT contract change ALWTTT-side. No `CURRENT_STATE.md` §3 rotation (B3-slate remains next-active conceptually but is superseded by the new S1-S8 sequencing produced in the 2026-05-23 planning reframe).

### Phase B B2 — Polish layer (feedback + animation) — complete (2026-05-13)

Closes the second batch of Phase B. Aditivo, low risk, monolithic per D3=A. Six items (#3, #4, #5, #6, #14, #15+#16) shipped end-to-end. Two mid-batch decisions (D-Inspiration-Pool=A, D-FxChangeDetect=A) added one cross-system bug fix and one design framework, both accepted.

**Deliverables shipped:**
- **#3 Tooltip miniature on track labels.** New `MinicardTooltipController` singleton, `SongTrackElementUI` pointer hover handlers, `TrackEntry.sourceCardDefinition` plumb through `TryAddOrReplaceTrackOnPart` → `SongPartElementUI.AddOrUpdateTrack` → row Bind. Placeholder rows never preview. Required two clamp/coordinate-space bug fixes during S1 testing; final patch lives in the file.
- **#4 Inspiration markers pop-up animation.** Reusable `UIPulseAnimator` (scale + optional color flash). `SongCompositionUI.SetInspiration` / `SetPlusInspiration` track previous values and pulse on change (gain=green, loss=red, +N=cyan). First-set suppressed. Inspiration "denied" flash on insufficient-cost via D-Inspiration-Pool=A.
- **D-Inspiration-Pool=A — action card cost gating.** Discovered mid-batch: action cards bypassed inspiration cost entirely (groove-economy check commented out at HandController.cs:687-688 and never replaced). Patched: `CompositionSession` exposes `CanAffordInspiration` / `FlashInspirationDenied`; `GigManager` exposes `CompositionSession` accessor; `HandController.TryPlayInGig` action-branch pre-checks cost, denies + flashes if short. Deduction itself stays in `CardBase.Use` (existing path); HandController only gates. Single-pool semantics: action + composition cards spend from the same pool.
- **#5 Expanded floating text:**
  - Composition events via diff-driven classifier (D-FxChangeDetect=A): `PartChangeSnapshot` captures pre-apply state, `SelectFxEntry` returns at most one `FxEntry` from `CompositionFxConfigSO` based on actual diff. Labels: TEMPO!, METER!, KEY!, MODIFIER!, INSTRUMENT!, RHYTHM!/BACKING!/MELODY!/HARMONY! for track replacement, MAJOR CHANGE for 2+ diffs. Initial-setup is silent (track first-add, tonality first-set).
  - Audience exclamations in `TriggerAudienceMicroReactions`: WOW / YEAH / silent / MEH / BORING, sign-coded color. Spawn pipeline verified; real impression generation deferred to B3 content.
  - Earworm-multiplier text: spawn block added at wrong tick site in GigManager (real Earworm tick lives elsewhere in StatusEffectSO). Mechanism verified; relocation deferred to B2.5. **[Re-characterized in B2.5 closure: the "real tick lives elsewhere" hypothesis was incorrect on inspection — `StatusEffectContainer.Tick` only decays stacks; the bespoke vibe-gain block in `GigManager` IS the only tick site. Actual issue was synchronous spawn pile-up, fixed via per-holder pacing.]**
- **#6 SongHype thresholds → venue SFX.** `GigPresentationSO` gains 3 threshold floats (defaults 0.34/0.67/1.0) + 3 sfx tags. `GigManager._songHypeStage` (monotonic, reset per song). `AddSongHype` refactored into `AddSongHypeCore` (guard-free) + `AddSongHype` (guard-respecting). Public `DevAddSongHype` / `DevResetSongHype` for testing. `BackgroundContainer.ActivateSFX` tag-dispatches: stage 1 → SetLights; stage 2/3 → SetLights with log noting per-venue hook gap (parked B2.5). DevModeController has +10% / -10% / Reset buttons.
- **#14 Robot beat-pop animation.** `CharacterAnimator.scaleOnBeat` mode. Wired on Robot prefab with `jumpOnBeat = false`.
- **#15 Worm animation.** `CharacterAnimator.stretchOnBeat` mode. Wired on Gusano body. Required `Skip Every N Beats = 1` correction during S6.
- **#16 Worm instrument sub-animator.** No code change — second `CharacterAnimator` on Gusano's instrument GO with own beat offset.

**Scene wiring locked at closure** (inspector-only):
- `MinicardTooltipController` GameObject under overlay canvas.
- `UIPulseAnimator` components on inspiration value + +N badge.
- `compositionFxAnchor` moved to world-space (S3 debug surfaced UI-vs-world coordinate mismatch).
- `CompositionFxConfigSO` asset created and wired on `SongCompositionUI`.
- `GigPresentation.asset` threshold + tag fields populated.
- Robot prefab: `scaleOnBeat=true, jumpOnBeat=false`.
- Gusano prefab: `stretchOnBeat=true, jumpOnBeat=false, skipEveryNBeats=1`.
- Second `CharacterAnimator` on Gusano's instrument GO.
- Two test `TempoEffect` assets authored (`Effect_Tempo_VeryFast`, `Effect_Tempo_VerySlow`).
- Test composition cards with tempo modifiers attached to existing Rhythm cards.

**Known limitations parked to B2.5:**
- `BackgroundRoot.SetSmoke`/`SetFire` not wired — stage 2/3 SongHype crossings visually identical to stage 1.
- Earworm-multiplier floating text spawned at wrong tick site. **[B2.5 correction: not a wrong site — same single tick site as M4.3, just synchronous pile-up. Fixed via per-holder pacing.]**
- Cross-animator BPM propagation — instrument animator stays at serialized BPM regardless of song tempo.
- `CardBase.cs:526` stale TEST log + `OnPointerDown` log spam.
- B2 debug logs in several files (diagnostic prints from S1-S5 testing).
- Audience action floating text uses old int-based API.
- `TimeSignature.ToString()` format consistency for meter diff — needs verification.
- `DevAddSongHype` / `DevResetSongHype` not gated by `#if ALWTTT_DEV`.
- Dead `Tonality` entry in `CompositionFxConfigSO` (never fires after first-set-silent refinement).

**Smoke tests:** ST-B2-S1 PASS, S2 PASS, S3 PASS, S4 PASS-with-deferral (real impressions B3, Earworm-multiplier B2.5), S5 PASS-with-caveat (stages 2/3 await BackgroundRoot extension), S6 PASS.

**Files changed:** 3 new + 10 modified (counts DevModeController and FxManager edits), ~700-900 LoC ALWTTT-side. No MidiGenPlay internals. No SSoT authority changes. F-1 / F-3 / F-4 Stage A / F-5 invariants not regressed.

### Phase B B2.5 — Polish refinements + cleanup — complete (2026-05-15)

Closes the cleanup-and-correctness batch parked at B2 closure. 11 mandatory items shipped (correctness 1-3 + cleanup 7-11) plus item 16 (Tonality FxEntry kept with tightened intent comment). Items 4, 5, 6 (content-dependent) and 12-15 (design gaps) explicitly deferred — see §4. Eight decisions locked: D-1 through D-8. Three preexisting bugs surfaced and resolved within the batch (D-7 ghost cards, D-8 macro-Vibe regression, B3-cand-I ParentActive=False captured for B3).

**Hypothesis correction (mandatory note).** The B2 closure language characterized the Earworm-multiplier deferral as "real Earworm tick lives elsewhere in StatusEffectSO" (§1 B2 block + B2 known-limitations bullet). This hypothesis was **incorrect on code inspection**. `StatusEffectContainer.Tick` only decays stacks via `DecayMode`; there is no separate gameplay-payload tick site. `StatusEffectWizardWindow.cs:250` documents this explicitly via inspector helpbox: "Other values (EndOfTurn, StartOfLoop, EndOfLoop, OnAction, OnHit, OnTakeDamage) are declared in the enum but NOT invoked by the runtime and will silently fail to decay." The bespoke Earworm vibe-gain block at `GigManager.AudienceTurnRoutine` IS the only tick site. The actual issue was synchronous spawn pile-up at audience-turn-start — N Earworm holders' floating texts firing in one frame instead of paced across the audience action cadence. Fix: per-holder `yield return waitDelay` (D-1=A). M4.3 invariants preserved (vibe-gain before audience actions, before decay; `IsBlocked` skip; `Convinced` ticks harmlessly).

**Deliverables shipped.**

Correctness:
- **#1 Earworm stagger.** Per-holder `yield return waitDelay` in `AudienceTurnRoutine`. Floating texts pace on `PerAudienceActionDelay` cadence. Non-holder audiences add no latency (early-`continue` before yield).
- **#2 `BackgroundContainer.DeactivateAllSFX`.** New method clears venue SFX (`SetLights(false)` now; per-venue `SetSmoke`/`SetFire` stubs documented for future extension). Hooked into `OnCompositionSongFinished` (split per D-8 — see below) and idempotently inside `ResetSongHype` for safety.
- **#3 Multi-animator BPM propagation.** New `CharacterBase.BroadcastBPM(int)` uses `GetComponentsInChildren<CharacterAnimator>(false)`. Replaces 6 sites in `GigManager` (lines 936, 958, 2073, 2089, 2109, 2125 in pre-edit numbering) + `AudienceCharacterBase.OnConvinced`. Body-only animator settings (`SkipEveryNBeats`, `JumpOnBeat`, `BeatOffsetBeats`, `RotateOnBeat`, `EmitOnBeat`) preserved per call site — only BPM cascades. Worm instrument sub-animator (B2 #16) now receives song tempo correctly.

Cleanup:
- **#7** `CardBase.cs:526` stale `TEST TEST TEST` log deleted.
- **#8** `CardBase.OnPointerDown` log spam (two `Debug.Log`s, including the right-click diagnostic) deleted; right-click behavior preserved.
- **#9** B2 debug logs stripped. `MinicardTooltipController` × 2 (yellow Show + lime Fade complete), `SongTrackElementUI` × 2 (cyan ENTER + EXIT), `FxManager` orange `[FxManager] FT spawned` block, `GigManager` `clamped = 2;` forced-clamp in `TriggerAudienceMicroReactions`. Audience exclamations now correctly placeholder-zero state until B3 implements `AudienceCharacterBase.ResolveLoopEffect` (per the original PASS-with-deferral on ST-B2-S4). Verified absent: magenta `SongCompositionUI` Debug.Log and cyan `[B2/#5 DEBUG]` GigManager log (rehydration referenced them; grep returned 0 hits in both).
- **#10** Audience action floating text normalized to `SpawnFloatingText(Transform, string, Vector2, Color)` overload in `AudienceCharacterBase.ExecuteActionWithTiming`.
- **#11** `TimeSignature.ToString()` format verified consistent — both `CaptureSnapshot` and `SelectFxEntry` call the same `Enum.ToString()` source with `StringComparison.Ordinal`. No code change; item closes as "verified."

Design gaps (only #16 closed; 12-15 deferred to B3):
- **#16 Tonality FxEntry.** Kept (D-3=B). Removal would have created migration debt on existing assets referencing the serialized field; cost of keeping is essentially nil. Inline comment in `SongCompositionUI.SelectFxEntry` tightened to document the design-hook intent for future first-explicit-set semantics.

**Mid-batch decisions and refinements (D-5 / D-8 narrative).** D-5=A initially moved `ResetSongHype()` to `OnCompositionSongFinished` so lights/bar/beat intensity would all reset at audio-end (clean visual coherence). This introduced a regression caught by ST-B2.5-S6 diagnostic instrumentation: `_songHype` was zeroed BEFORE `RunSongVibeResolution.ComputeSongVibeDeltas` could read `SongHype01` to compute `baseVibe`. Result: macro-Vibe deltas all zero, no Vibe applied to audiences at song end. **D-8=A surgically split D-5**: only `backgroundContainer.DeactivateAllSFX()` stays at song-end (lights-off-at-audio-end UX preserved); full `ResetSongHype()` moved back to `AudienceTurnRoutine` AFTER `RunSongVibeResolution` consumes the value. The `DeactivateAllSFX` call inside `ResetSongHype` is idempotent (already off from the song-end call). The cycle is now: audio end → lights off → AudienceTurn entry → `RunSongVibeResolution` reads SongHype → applies Vibe deltas → `ResetSongHype` zeroes remaining state → `ClearSongHype` UI hide.

**Other locked decisions.**
- **D-6=A — Hand discard default flipped to `true`.** `GigFlowSettingsSO.defaultDiscardHandBetweenTurns` code default and `GigFlowSettings.asset` value both changed from `false` to `true`. Production behavior is now "hand discards between turns" by default; per-encounter run-config override path preserved.
- **D-7=A — `DiscardHand` ghost cards.** Surfaced by D-6 (when the toggle was off, the bug was masked — nobody called `DiscardHand`). Root cause: `CardBase.Discard()` is async (`StartCoroutine(DiscardRoutine())` over `discardDuration` seconds) AND gated on `IsPlayable`/`IsExhausted` (gates silently skip without booking `OnCardDiscarded`). Production-path `DeckManager.DiscardHand()` looped through `card.Discard()` and then cleared `Hand` — leaving GameObjects in scene as ghosts that accumulated each cycle (visible as fake non-hoverable cards in hand). Fix promotes the `DevForceHandResetToDiscard` pattern to production `DiscardHand`: synchronous destroy + sweep strays under `DrawTransform` + inline `HandPile→DiscardPile` bookkeeping. Dev wrapper preserved for explicit dev use. The dev XML comment had already documented this exact bug pattern.

**Smoke tests:** ST-B2.5-S1 PASS (Earworm stagger), S2 PASS (lights clear between songs), S2b PASS (lights off at exact audio-end moment), S3 PASS (BPM propagation to sub-animator), S4 PASS (hand discard toggle respected), S5 PASS (no ghost cards across cycles), S6 PASS (macro-Vibe applied visually).

**Files changed:** 10 modified (`GigManager.cs`, `CharacterBase.cs`, `BackgroundContainer.cs`, `AudienceCharacterBase.cs`, `CardBase.cs`, `MinicardTooltipController.cs`, `SongTrackElementUI.cs`, `FxManager.cs`, `SongCompositionUI.cs`, `DeckManager.cs`). 1 asset value change (`GigFlowSettings.asset.defaultDiscardHandBetweenTurns: false→true`). 2 in-scene/asset corrections done as B3-cand-A/B during ST-B2.5-S1 playtest (Mind Tap payload target alignment, AudienceMemberPosList reordered to visual left-to-right). No MidiGenPlay internals. No SSoT authority changes. B1 + B2 invariants preserved.

**Items explicitly deferred from B2.5.**
- Content-dependent (3): #4 per-venue smoke/fire VFX (art-dependent), #5 CompositionFxConfigSO default tuning (playtest-dependent), #6 animation feel tuning (Robot pop, Worm stretch, instrument — playtest-dependent).
- Design gaps (4): #12 TempoScale diff in `SelectFxEntry`, #13 hasExplicit flags on PartEntry, #14 `PartActionKind.NoOp`, #15 `#if ALWTTT_DEV` gate on `DevAddSongHype`/`DevResetSongHype`.

**B3 candidates accumulated during B2.5:** A (Mind Tap asset fix — DONE in-batch), B (AudienceMemberPosList reorder — DONE in-batch), C (effect-target-type authoring validation), D (CustomEditor for default Inspector showing effect labels), E-lite (Blocked tooltip without icon — for the "oscurito" sprite tint legibility), F (real `AudienceCharacterBase.ResolveLoopEffect` impl — currently placeholder returning 0), G (filter draws during composition session, per D-B3-DrawFilter=B confirmed in B2.5), H (Always-action card discard semantics in SongPerformance start — if Always-cards become content), I (ParentActive=False warning during first draws — preexisting bug not blocking B2.5 but visible in logs).

### Phase B B3 — sub-batch B3-content-audience — complete (2026-05-17)

Audience-side content authoring batch. Two passes:
- **Pass 1 (code).** `AudienceMoveToFrontAction` gained `stepsPerTurn` (ActionValue, default 1) replacing snap-to-front. `BandCharacterStats.ApplyOutgoingStressWithModifiers` static helper added (hardcoded Hyped check via `DamageUpFlat` + `statusKey=="hyped"`). `AddStressAction` routes attacker through outgoing helper before calling existing M4.1 `ApplyIncomingStressWithComposure`.
- **Pass 2 (D10 dispatcher + content + targeting).** New `CharacterActionType.ApplyStatusEffect=11` + `ApplyStatusEffectAction` class + `StatusEffectSO` field on `CharacterActionData` + optional ctor param on `CharacterActionParameters`. New `ActionTargetType.AudienceTall=100` for first-tall-non-self targeting. SOs authored: Cool Dude (3 abilities: Move/Heckle/Indifference, FollowAbilityPattern ON, taste per D12, Vibe goal 25), Kid (Tantrum + Egg Him On targeting AudienceTall, Pattern ON, taste per D13, Vibe goal 10), Hyped (DamageUpFlat, Additive MaxStacks=3, LinearStacks decay PlayerTurnStart). Demo encounter SO: 2×Kid + 1×Cool Dude, configurable songs count.

**Decisions locked:** D10=A (ApplyStatusEffect dispatcher path), D11=A (Cool Dude MaxVibe 25, Kid MaxVibe 10), D12=A (Cool Dude "Block the View" name preserved), D13=A (FollowAbilityPattern ON for both), D14=B (AudienceTall=100 enum value), D-Hyped-key (statusKey="hyped" reserved, no variants), D-DCP-6=A (Indifference blocks ALL incoming Vibe).

**Smoke tests:** ST-B3d-CA-P1-1..4 PASS (Move stepsPerTurn parameterized, Hyped on AddStressAction). ST-B3d-CA-P2-1..8 PASS (D10 dispatcher, Cool Dude ability cycle, Heckle composed Base=2/Modified=3/Absorbed=0/Applied=4, Indifference blocks Vibe via 3 paths, Kid → Hyped on Cool Dude AudienceTall, Hyped=3 amplifies Heckle to Applied=7, full Composure×Hyped×Exposed pipeline composes, encounter outcomes fire).

### Phase B B3 — sub-batch B3-demo-polish — complete (2026-05-17)

Eight UX defects discovered through smoke testing the B3-content-audience demo path. All fixes shipped, F9 partial (replaced by §5.3.5 in next session).

**F1.** `RewardCanvas.BuildReward` defensive guard against empty `cardRewardList` after population. Skips reward UI and immediately finishes if no cards available. `GetCardReward` also clamps `Mathf.Min(amount, pool.Count)`. Decision D-UX1=C.

**F2.** Win/Loss panel Retry + Exit buttons. `GigCanvas` gained 4 SerializeField Button refs + 4 `System.Action` events + 4 `OnClick_*` handlers. `GigManager.LoseGig` and `WinGig` (IsFinalEncounter branch) assign Retry/Exit handlers per flow. Decision D-UX2 (Retry=scene-reload, Exit=main-menu).

**F3+F8.** Escape key + UIManager scene routing. `UIManager` gained `mainMenuSceneIndex` SerializeField + public getter, `Update()` with `Input.GetKeyDown(KeyCode.Escape)` polling + 1Hz diagnostic tick, `HandleEscapeKey` (in-gig → main menu; on main menu → quit), `QuitGame` (Application.Quit + EditorApplication.isPlaying=false in Editor). Decision D-UX3=D (simple ESC; full pause menu deferred).

**F4.** Final-song Vibe conversion. `OnCompositionSessionEnded`'s `SkipAudienceActionsAfterFinalSong` branch was bypassing `AudienceTurnRoutine` entirely, but Vibe conversion lives only there. Now routed through new helper coroutine `RunFinalSongVibeThenEnd` that runs `RunSongVibeResolution` before `ResolveGigOutcomeAndEnd`. Site 1 (legacy MIDI path) and site 2 (live composition) both patched. Decision D-F4=A.

**F5.** `PersistentGameplayData.CurrentSongIndex` carries over scene reload (PD is DontDestroyOnLoad). `HandleRetry` now resets it to 0 before `SceneManager.LoadScene`. Fans/Cohesion/GigsWon preserved across retry (demo-scope decision). Decision D-F5.

**F6.** GigCanvas defensive `lossConfirmButton == lossRetryButton/lossExitButton` same-reference guard in OnEnable/OnDisable. Decision D-F6=C (code defensive + authoring cleanup).

**F7.** `OnClick_LossConfirm` deprecated to no-op + LogWarning. Inspector OnClick UnityEvent wirings bypass the C# AddListener subscription (which F6 already guards); F7 makes the method body itself harmless. Decision D-F7=C.

**F9.** Auto-gig-setup precursor: `GigSetupController.autoStartOnLoad` SerializeField + `Start()` + `AutoStartRoutine()` coroutine that auto-invokes `OnStartPressed` after a 1-frame yield. `UIManager.SkipAutoGigStart` static flag set by `HandleEscapeKey` + `HandleExit` to suppress auto-start when user intentionally returns to main menu. **Note: F9 is an ad-hoc precursor — the proper §5.3.5 Demo cut prep batch replaces this with a `DemoLaunchConfigSO` + `GigDevSettingsSO.autoStartFromDefaults` flag. F9 patches stay in place as a working stopgap.**

**Smoke tests:** ST-B3d-UX-1..13 all PASS (empty pool reward skip, Win/Loss Retry & Exit, ESC in-gig, ESC on main menu quits, Quit button, auto-start, Skip-auto on ESC/Exit return). ST-7/8 verified in Editor (Play Mode stops) and ready for build verification.

**Demo readiness:** Showable. App launches → 1-second GigSetup pass-through → Gig with Sibi + C2 vs 2×Kid + Cool Dude. Full Combat MVP: Stress pipeline (Hyped × Composure × Exposed), 3 Vibe-block paths via Indifference, audience threats (Tantrum, Heckle, Hyped, positional Move), Win/Loss with Retry/Exit, ESC handling, Quit button.

**Known bug-watch (deferred to polish sweep):** audience hover outline not rendering; Kid Tantrum AnimatorTrigger never fires (AbilityRoutine doesn't consume `NextAbility.Animation.AnimatorTrigger`); Indifference + Hyped icon sprites unassigned (statuses apply correctly, glyph missing).

### Phase B — §5.3.5 Demo cut prep — complete (2026-05-18)

Demo build now launches from Main Menu directly into the Gig scene with zero setup interaction (single fade cycle, no flicker). Structural refactor extracts a single launch contract (`GigLauncher`) and lays the foundation for ladder mode (post-demo). One mechanical addition (SFX→FlatVibe), one playtest-driven unblock (action-card performance gate), one latent-bug fix (Fade(false) loop termination), one dead-code deletion (`GigSetupSceneManager`). F9 (the ad-hoc precursor introduced in B3-demo-polish) replaced wholesale by the new launch architecture.

**Launch architecture (D-FAST-1=C).** New `GigLauncher` static service (`Assets/Scripts/Managers/GigLauncher.cs`, ~165 lines) is the single non-Gig→Gig scene transition entry point. Atomic contract: `SetBandRoster` (optional — `bandRoster: null` preserves current pd.MusicianList for ladder carry-over) → `GigRunContext.BeginRun` → `PD.ApplyRunConfig` → `IsFinalEncounter` assign → `SceneChanger.OpenGigScene`. Bool return signals launch dispatched / aborted. New `MainMenuController` (`Assets/Scripts/UI/MainMenuController.cs`, ~120 lines, `ALWTTT.UI` namespace per D-PLACE-1=A): `OnStartPressed` branches between `TryAutoLaunch` (reads `GigDevSettings + DemoLaunchConfig`, calls `GigLauncher.Launch`) and fallback to `SceneChanger.OpenGigSetupScene`; `OnQuitPressed` → `UIManager.QuitGame`. New `DemoLaunchConfigSO` (`Assets/Scripts/Data/Gig/DemoLaunchConfigSO.cs`, ~175 lines): baked SO with `bandRoster`, `encounter`, `requiredSongCount`, `initialGigInspiration`, `inspirationPerLoop`. Provides `ToRunConfig(returnDestination)`, `ResolvedBandRoster`, `IsValid(out reason)`. `GigSetupController` auto-start scaffolding deleted (Awake-blackening, `WillAutoStart`, `Start`, `AutoStartRoutine`, `ApplyDemoLaunchConfigAndStart`, devSettings SerializeField); `OnStartPressed` launch tail (≈100 lines) extracted to `GigLauncher.Launch`. Net −130 lines on `GigSetupController`.

**SFX→FlatVibe mechanic (DC-SFX-Route=A / D-DCP-2=A).** New `GigPresentationSO.sfxBonusVibeStage{1,2,3}` floats (defaults 3/6/10) + `GetSfxBonusVibe(stage)`. `GigManager` gains `sfxBonusVibeTextSpawnRoot` SerializeField (with first-musician-TextSpawnRoot fallback), `ApplySfxBonusVibe(stage)`, `ResolveSfxBonusVibeSpawnRoot()`. `FireSongHypeStage` now invokes `ApplySfxBonusVibe` after `backgroundContainer.ActivateSFX`. Bonus is applied per-audience-member through canonical `ApplyIncomingVibe` (Indifference still blocks per-member, D-DCP-6=A invariant preserved); single aggregate "+N Vibe!" floater spawns on band canvas (suppressed if every audience member blocks).

**Action-card mid-performance unblock.** `CanPlayActionCard` performance gate relaxed: when `allowActionCardsDuringPerformance` is on, all action cards (not just `CardActionTiming.Always`) become playable. Demo enables the flag via `GigFlowSettings.allowActionCardsDuringPerformance = true`; without the relaxation, mid-composition draws of non-Always-tagged action cards stranded in hand. `CardActionTiming.Always` enum value retained for future precision-gating needs.

**Latent bug fix (UIManager).** `Fade(false)` loop termination bug fixed (`isIn ? timer >= 1f : timer <= 0f` exit + endpoint snap). Latent: `Fade(false)` coroutine never terminated, accumulated across scene transitions; visually masked by `CanvasGroup` clamping but contributed to the auto-start path's messy frame-by-frame timing observed during D-FAST-1 investigation.

**Dead code deletion.** Empty `Assets/Scripts/Managers/GigSetupSceneManager.cs` (5 lines, no body) deleted — surfaced during file-tree review.

**Decisions locked:** DC-1=C (quick-start flag locality on GigDevSettingsSO), DC-2=Custom (audience pool 2× Kid + 1× Cool Dude), DC-3=Custom (4 songs × 1 part × 4 loops/part + SFX→FlatVibe), DC-4=B (Inspiration 3/1), DC-5=B (batch between B3 close and §5.4), D-DCP-1=A (dev-side flag locality), D-DCP-2=A (SFX bonus defaults 3/6/10), D-DCP-6=A (Indifference blocks ALL incoming Vibe — preserved invariant), DC-Scene-1=existing (Main Menu scene), DC-Scene-2=A (build order: 0=MainMenu, 1=GigSetup, 2=Gig), DC-F9-fate=A (strip F9 entirely; replaced by GigLauncher), DC-SFX-Route=A (per-audience via ApplyIncomingVibe; aggregate band-canvas floater), D-FAST-1=C (extract GigLauncher; bypass GigSetup from Main Menu), D-PLACE-1=A (MainMenuController under `Scripts/UI/` with `ALWTTT.UI` namespace).

**Smoke tests:** ST-DCP-S1 PASS (auto-entry zero-click), ST-DCP-S2 PASS (encounter wiring), ST-DCP-S3 PASS (SFX→FlatVibe stage crossings), ST-DCP-S4 PASS (Indifference gating), ST-DCP-S6 PASS (action-card unblock), ST-DCP-S8 PASS (auto-launch through GigLauncher), ST-DCP-S9 PASS (manual GigSetup regression), ST-DCP-S10 PASS (Fade(false) termination). ST-DCP-S7 OBSOLETED (Awake-blackening flicker — superseded by ST-DCP-S8 after D-FAST-1=C). **ST-DCP-S5 (win-rate validation, 8-10 playthroughs) DEFERRED to §5.4 readiness review** — folds into §5.4's full clean-run smoke pass (R1 sub-item).

**Demo readiness:** Showable. Cold launch → Main Menu → Start → Gig action window 1, with the full B3 combat content (Sibi + C2 vs 2×Kid + Cool Dude, Stress pipeline, Indifference, audience threats, Win/Loss with Retry/Exit). Zero setup interaction. fadeSpeed inspector value can be bumped to compress the two fade cycles further without affecting structure.

**Files:** NEW `Assets/Scripts/Managers/GigLauncher.cs`, `Assets/Scripts/UI/MainMenuController.cs`, `Assets/Scripts/Data/Gig/DemoLaunchConfigSO.cs`, planning `Design_Demo_Cut_v1.md`. MODIFIED: `GigDevSettingsSO.cs` (+`autoStartFromDefaults`, +`demoLaunchConfig`), `GigPresentationSO.cs` (+`sfxBonusVibeStage{1,2,3}`, +`GetSfxBonusVibe`), `GigSetupController.cs` (auto-start scaffolding deleted, launch tail extracted), `GigManager.cs` (SFX→FlatVibe wiring + action-card gate relaxation), `UIManager.cs` (Fade(false) loop fix). DELETED: `GigSetupSceneManager.cs`.

### 2026-05-20 — B3-content-sibi closed + B3-content-sibi-followup closed

**B3-content-sibi.** Singing Field gained per-card Sibi voice via `InstrumentEffect_Sibi_Voice.asset` (`Assets/Resources/Data/Cards/Composition/_PartEffects/`), referencing `Fantasia` MIDIInstrumentSO from the MidiGenPlay package Synth catalog. Wired onto `Starter_singing_field_Payload.modifierEffects[0]`. ST-B3CS-1..6 all PASS.

**D-Sibi-3 retroactive correction.** The new asset is the `InstrumentEffect` carrier (ALWTTT-side), not a new `MIDIInstrumentSO`. Package-side existing `MIDIInstrumentSO` assets are reused via reference. MidiGenPlay boundary preserved.

**B3-content-sibi-followup.** Per-musician SO whitelist activated. `InstrumentRules.GetPermittedMelodic` extended with a per-role SO whitelist precedence layer above the existing `InstrumentType` filter (precedence top-down: explicit `InstrumentEffect` override > musician SO whitelist for role > musician `InstrumentType` filter > all melodic). Empty list per role → unchanged behavior. No cross-role whitelist fallback. Sibi's `MusicianProfileData.leadMelodicInstruments` populated with [Fantasia, 5th Saw Wave, Soundtrack]; `backingMelodicInstruments` left empty (authoring deferred — empty-list discipline preserves existing `InstrumentType`-filter behavior on backing role). Singing Field's `modifierEffects` cleared (carrier asset retained on disk).

ST-Pool-1..6 all PASS. ST-Pool-7 reclassified: cross-role isolation property proven by composition of ST-Pool-3 (lead bounded by populated lead whitelist) and ST-Pool-4 (backing role-routing correct via empty-list fallback to `InstrumentType` filter); simultaneous multi-track observation not testable in current codebase (one musician, one track per song). Property verified without ST-Pool-7.

**D-Sibi-Pool-Scope=γ shipped as capability.** The role-routing in `GetPermittedMelodic` supports independent per-role whitelists. Lead is exercised; backing whitelist authoring deferred (content decision, not code-blocked). The per-musician identity precedent stands on the lead side.

**Data-model finding.** `MusicianProfileData.{leadMelodicInstruments | backingMelodicInstruments}` SO-whitelist fields already existed as latent infrastructure. This batch activated them at runtime. No new SerializeField required. `InstrumentEffect` carrier remains in the type system as explicit-override tool for future per-card identity work.

**Decisions locked.** D-Sibi-3 (carrier, not new MIDIInstrumentSO), D-Sibi-Pool=A (pool on `MusicianCharacterData.Profile`), D-Sibi-Pool-Scope=γ (both lead and backing whitelists, independent per role — capability), empty-list discipline (per-role; no cross-role fallback), ST-Pool-7 disposition=A (redundant, retired).

**Directive D1 promoted (2026-05-20).** Count 2/2-3 satisfied at lower bound (B3-content-sibi + B3-content-sibi-followup). User decision A in doc-apply session. D1 promoted to standing project guidance. Canonical articulation retained in `planning/Design_Project_Directives_v0_1.md`. **Pending on user side:** add D1 to the project-level instructions panel (manual UI action — Claude cannot reach the panel from a chat).

**Files (this closure):** No code change beyond the previous implementation session (already captured: `InstrumentRules.cs` ~25 LOC net new; Sibi's `MusicianCharacterData.asset` Inspector edit on `Profile.leadMelodicInstruments`; `Starter_singing_field_Payload.asset` Inspector edit on `modifierEffects[0]`). No new files. No new SerializeFields. No new SO types. `InstrumentEffect_Sibi_Voice.asset` retained on disk as override exemplar.

### B3-content-cards — complete (2026-05-22)

Two BPM cards (Push It ×1.5, Half Time ×0.66) and one Modulation card (Key Lift, `IntervalWithinScale` degree=5) authored and integrated. No new effect mechanism — both `TempoEffect` (axes 4–6) and `ModulationEffect` (axis 8) already shipped and wired. Starter rebalanced: Wormus pair doubled (2 each), Compound Cycle + Pentameter moved to reward pool. Final starter: 4 Action + 4 C2 + 6 Sibi + 1 Cantante = 15 cards.

**Decisions locked.** D-BPM-1=A (use existing `TempoEffect`); D-BPM-2=A (`ScaleFactor`-only, two cards); D-MOD-1=A (use existing `ModulationEffect`); D-MOD-2=A (modulation = structural change per B1 `partMeterHash`; full part regen accepted, rhythm regen-but-audibly-equivalent noted); D-MOD-3=A (single Key Lift card, `IntervalWithinScale` degree=5); D-MOD-FIX=A (preserve-on-null in `TryAddOrReplaceTrackOnPart`); D-MOD-DIR=A (accept non-directional voicing for demo; cross-project ask filed as MGP-ALWTTT-MOD-DIR-1); D-STARTER-1=B (Wormus ×2 each); D-STARTER-2=B (Compound Cycle + Pentameter to reward).

**Code change.** `SongCompositionUI.TryAddOrReplaceTrackOnPart` existing-track branch — preserve `existing.styleBundle` when incoming descriptor's bundle is null. Three-line guard. No regression against current content; benefits all cards using `TrackAction` as `PartEffect` carrier.

**Asset deltas.** 3 new `CardDefinition` + payload assets (`Starter_push_it`, `Starter_half_time`, `Starter_key_lift`). 3 new `PartEffect` SO assets (`TempoEffect_PushIt`, `TempoEffect_HalfTime`, `ModulationEffect_KeyLift_Degree5`). 5 catalog edits across C2 and Sibi `MusicianCardCatalogData` (3 new entries; Wormus Minor + Wormus Major `starterCopies` 1 → 2; Compound Cycle + Pentameter `StarterDeck` flag removed, `RewardPool` flag set).

**Cross-project ask.** `MGP-ALWTTT-MOD-DIR-1` filed (directional modulation hint for `ChordTrackComposer`). ALWTTT will adopt when MidiGenPlay ships the surface; no ALWTTT-side blocker.

**Smoke tests.** S1–S8 PASS. S4 initially failed (chord progression regenerated under modulation), root-caused to track-bundle wipe on null incoming descriptor in `TryAddOrReplaceTrackOnPart`, fixed by D-MOD-FIX patch, re-passed. S4a (track-bundle preservation invariant) added and PASS. S8 (final starter composition) PASS.

### Phase A — closed (2026-05-09)

Formal closure of the pre-demo construction phase. Phase A spans the entire project history from Combat MVP (2026-03-23) through M4.6F-4 Stage A and MB4 (2026-05-08). It establishes a working, showable build with a complete combat loop, composition session integration, status effect system, audience pressure system, deck/card authoring pipeline, Dev Mode tooling, and a 2-musician starter deck (Robot C2 + Sibi Gusano).

**What Phase A delivered (high-level inventory):**
- Combat MVP (4 card effect types, 6 SO statuses, composure/breakdown/cohesion path, tick system).
- Authoring infrastructure (Card Editor + Deck Editor + Card Inventory + Status Effect Wizard + Chord Progression Wizard, all `#if UNITY_EDITOR`-guarded).
- Dev Mode (infinite turns, F12 IMGUI overlay, card spawner, per-character stat editing, status apply/remove picker, gig-wide stat editing, Inspiration session routing).
- Card system (action + composition cards, multiset deck contract per M4.4, bidirectional guaranteed draws per M4.5, M4.6F-1 single-OnCardPlayed invariant, M4.6F-3 canonical `AddCurrentInspiration` mutator).
- Composition session integration (per-loop draw, per-loop inspiration, F-4 Stage A IOOR defense + D3-B recursion guard, MB3 dual-siting fix, MB4 action-card session routing).
- Audience system (Earworm as first audience-side status, encounter pickers, multiset-blind override comparator).
- Gig settings consolidation (4-SO refactor per M4.6F-2: GigFlowSettings, MeterTuning, GigPresentation, GigDevSettings + renamed GigSetupRoster).
- Starter deck authoring (Robot 4/4/5, Gusano 4/4/4, Generic 2/2/3 — matching `Design_Starter_Deck_v1.md §4`).

**M4.6F-5 reframe.** F-5's original scope was "implement per-loop pending workflow." During Phase B planning (2026-05-09), the user clarified that per-loop card resolution **already works** in the current zone (cards in current → replace track → take effect next loop). The complex piece was the *next zone* (planning a future part). F-5 is **absorbed into Phase B B1** as "next zone simplification" — disable next zone, current zone becomes full-screen, model collapses to per-loop-only. F-5 is not closed in its original framing; it is re-scoped into Phase B's first batch.

**Phase A demo readiness check:**
- **Demonstrable:** A 2-musician gig that loads from per-musician auto-assembly, plays composition cards in current zone with per-loop replacement, applies Earworm to audience, ticks status effects, resolves through Cohesion or audience conviction, with Dev Mode available for repro.
- **Acceptable rough edges:** Persistence between loops (unrelated tracks regenerate), UI polish on tracks + Inspiration markers, animation flatness, balance gaps in Inspiration costs, narrow audience ability variety.
- **Phase A is showable as a "pre-demo."** Phase B addresses the rough edges to bring it to true demo quality.

**Decisions locked at Phase A close (Phase B opening matrix):**
- D1=C: Phase A closes formally, Phase B opens with own identity.
- D2=B: Per-track persistence between loops (simple). Tracks not touched persist verbatim. Structural changes (TS, key) → full regen.
- D3=A: B2 monolithic (UI feedback + animation in one batch); fallback split B2a/B2b if it gets unwieldy.
- D4=B: Audience Wizard Editor deferred post-demo.
- D5=run: Spike completed 2026-05-09 (see closure log).
- D6=A: Per-track stem cache scope (each track invalidates independently).
- D7=B: Stem cache lifetime is per-song (resets on song boundary, persists across loops + part transitions within a song).
- α/β=β: Phase A close + Phase B open ships as a clean separate doc batch (this one). B1 opens fresh after.

**Spike findings (D5).** Per-track persistence is feasible on the ALWTTT side without violating the MidiGenPlay boundary. Mechanism: stem cache keyed on `(trackIdentity, trackInputsHash, partMeterHash)` co-located with `MidiMusicManager`; cached MIDI bytes per track from prior renders are reused when track inputs unchanged; structural changes invalidate all stems for the part; merging via DryWetMidi. Estimated ~200-300 LoC ALWTTT-side. F-4 Stage A try-catch defense remains outermost; on catch all stems for the part invalidate (safe regression to pre-cache behavior). Full spike findings in `changelog-ssot.md` Phase A close entry.

### Phase B B1 — closed (2026-05-12)

Foundational batch of Phase B. Internal scope #7 + #0 + #1 + #2 + #8 + #7.1 (D-F=γ ad-hoc within batch) + D-J draw-on-play (mini-item added during batch).

**#7 — Per-track stem cache** in `MidiMusicManager`. Two-dict layout per D-C=α: `_stemCache` keyed on `(musicianId, trackInputsHash, partMeterHash)` for verbatim per-musician stem bytes; `_partBundleCache` keyed on `(partMeterHash, sortedTrackHashes)` for fast-path replay when nothing changed. DryWetMidi merge utility combines cached + fresh stems on partial misses. `ResetStemCache()` fires from `CompositionSession.Begin()`/`End()` per D7=B (per-song lifetime). F-4 Stage A try-catch invalidates per-part on catch via `InvalidateStemCacheForPart`.

**D-E=α' — UI-stable track inputs hash.** The trackInputsHash is computed ALWTTT-side in `SongConfigBuilder.ComputeTrackInputsHashesForPart` from the UI `TrackEntry` (role + StyleBundle GUID + override-melodic-instrument GUID + override-percussion-instrument GUID + override-instrument-type). Passed as 5th parameter to `RenderSinglePart`. Survives the random instrument resolution that happens inside `SongConfigBuilder.FromUI` for the no-override path. SongConfig field set unchanged (boundary respected per `SSoT_ALWTTT_MidiGenPlay_Boundary §3`).

**#0 — Next-zone disable per D-D=β.** Soft-disable: `HandController.GetDropZoneForLocalPoint` redirects NextPart geometry to CurrentPart; gizmo + label removed. `CompositionSession.TryPlayCompositionCard` unconditional redirect at the head of step 4. `CardDropZone.NextPart` enum value + downstream branches preserved dormant.

**#1 + #2 — Composition UI rework.** `SongCompositionUI` gains pending-track visualization (color tint, suppressed for placeholder rows) via `MarkTrackPending`/`MarkAllTracksPending`/`OnRenderCompleted` API. Trigger lives in `CompositionSession.TryPlayCompositionCard`: cards that affect part-meter (TS, tonality, root, tempo) mark ALL tracks via the `affectsPartMeter` predicate combining `IsTempoCard`/`IsTimeSignatureCard`/`IsTonalityCard`/`IsModulationCard`; pure-track cards mark only the target; clear fires from `PlaySinglePartLoop` after a successful fresh render. `SongTrackElementUI` gains inspiration-next badge (`+N` to the right, hidden for placeholders and N≤0).

**#8 — Hand-discard configurability.** Honored existing `GigFlowSettingsSO.DiscardActionCardsOnPlay` flag (default true → Action cards discard at Play to avoid loop-time noise). Configurable in the SO inspector under "Action Card Gating (MVP)" header. The intermediate "soft-disable" patch shipped earlier in the batch was reverted after playtest confirmed that keeping Action cards in hand without an enforcement gate caused confusion.

**#7.1 — Session-level instrument pin per D-F=γ.1.** `CompositionSession` maintains `_sessionMelodicPin` and `_sessionPercussionPin` keyed on `"musicianId|role|override-state"` (NONE / TYPE:<value>; specific-SO overrides skip the pin entirely). `ApplyInstrumentPins` fires before each `RenderSinglePart` call to override `tcfg.Instrument`/`tcfg.PercussionInstrument` with the pinned value when applicable. `UpdateInstrumentPins` fires after successful render. Reset alongside the stem cache in `Begin/End`. Honors cards that change instrument by explicit SO or by type (type change refreshes the random pick within the new type).

**D-J — Draw cards on Play.** New `GigFlowSettingsSO.DrawCardsOnPlay` int field (default 0 = disabled). Honored in `GigManager.OnPlayPressed` between the discard step and the `ConfirmCurrentPartAndStart` call. Configurable in the SO inspector under "Composition" header.

**Smoke tests:**
- ST-B1-S1 (per-track persistence) PASS — bundle HIT on identical replay.
- ST-B1-S2 (structural invalidation) PASS — TS change → all stems regen.
- ST-B1-S3 (song-boundary reset) PASS — Reset logs at End→Begin.
- ST-B1-S4 (F-4 catch invalidation) DEFERRED — no Dev hook to force exception; reopens automatically if `[F-4][MMM]` LogError fires during playtest.
- ST-B1-S5 (#8 hand discard honors SO flag) PASS.
- ST-B1-S5.2 (D-J draw-on-play honors SO field) PASS.
- ST-B1-S6 (inspiration-next badge) PASS.
- ST-B1-S7 (pending tint on track card) PASS.
- ST-B1-S7.1 (pending tint on TS card — Pentameter) PASS after fix to `affectsPartMeter` predicate (combines `IsTempoCard`/`IsTimeSignatureCard`/`IsTonalityCard`/`IsModulationCard`).
- ST-B1-S7.2 (no pending pre-Play) PASS.
- ST-B1-S8 (next-zone disable) PASS.
- ST-B1-S9 (instrument pin across style change Minor→Major) PASS — initial perceived failure was registro-tonal change, not instrument change; confirmed via Test A (identical replay sounds bit-identical).
- ST-B1-S10 (instrument pin honors type-override card) PASS after D-F=γ.1 refinement (pin key includes override state).
- F-1/F-3/F-4 Stage A invariants clean.

**Code:** 9 files modified ALWTTT-side: `MidiMusicManager.cs`, `CompositionSession.cs`, `SongConfigBuilder.cs`, `HandController.cs`, `GigManager.cs`, `SongCompositionUI.cs`, `SongPartElementUI.cs`, `SongTrackElementUI.cs`, `GigFlowSettingsSO.cs`. `SongConfig.cs` (MidiGenPlay-owned) untouched per boundary contract. `TrackLayoutElement.prefab` updated user-side (InspirationNextText child + wiring).

**Watch-items opened during batch:**
- `_isSongPlaying` may not engage during active composition loop; the `AllowActionCardsDuringPerformance` gate at `GigManager:1454-1462` may not enforce as intended. Side-stepped by `DiscardActionCardsOnPlay=true`. Worth verifying post-B1 if any "Action cards during loop" design returns.

**Invariant promoted.** F-5 (D-K=α) — see `SSoT_Runtime_CompositionSession_Integration §8` item 9: per-track stem persistence + session-level instrument continuity contract.

### Combat MVP — complete (2026-03-23)
- Deck/hand pipeline operating in play mode.
- All four card effect types working end-to-end: `ModifyVibe`, `ModifyStress`, `ApplyStatusEffect`, `DrawCards`.
- Composure absorption via `ApplyIncomingStressWithComposure`.
- Breakdown → Cohesion−1 + Stress reset + Shaken application. LoseGig at Cohesion ≤ 0.
- Exposed stress multiplier and Feedback DoT (musician-only) wired.
- Tick timing: PlayerTurnStart (musicians) + AudienceTurnStart (audience).
- Six SO status entries in catalogue: `flow`, `composure`, `exposed`, `feedback`, `choke`, `shaken`.

### Composition / music surface — exists, not yet validated end-to-end
- `GigManager`, `MidiMusicManager`, `CompositionSession`, `SongConfigBuilder`, `LoopScoreCalculator`.
- CompositionSession bypass of phase machine documented (see `SSoT_Runtime_Flow`).
- Not yet tested: composition cards with real gameplay effects producing audible song changes.

### Status icon pipeline — SO-based (M1.2, complete 2026-04-14)
- Sprite authority on `StatusEffectSO.IconSprite`. Lookup asset removed.
- `CharacterCanvas` subscribes to `StatusEffectContainer` events and renders directly from the container's definition.
- Lazy icon lifecycle. Stack count text updates on every change.
- See `SSoT_Status_Effects.md` §3.3.
- **M1.2 multi-turn validation:** All three deferred tests closed. T5 Choke decay ✅ (Phase 2), T8 Feedback DoT ✅ (Phase 2), T7 Shaken expiry ✅ (Phase 3.1).

### Dev Mode Phase 1 — complete (2026-04-17)
Infinite turns, F12 IMGUI overlay, hand-visibility bridge. `ALWTTT_DEV` scripting define guards all Dev Mode code. See `SSoT_Dev_Mode.md`.

### Dev Mode Phase 2 — complete (2026-04-20)
Card spawner: Catalogue tab in the overlay, `DeckManager.DevSpawnCardToHand`, gated by `CanDevSpawnToHand` (PlayerTurn + MaxCardsOnHand + hand visibility). Decision U1 codified: spawned cards enter the deck on discard/reshuffle (accepted pollution).

### Dev Mode Phase 3.1 — complete (2026-04-23)
Breakdown entry point: Stats tab in overlay, musician selector, `MusicianBase.DevForceBreakdown()` via natural stress path (`DevResetBreakdown` + `AddStress(MaxStress)`). Re-triggerable. T7 Shaken expiry validated — M1.2 multi-turn validation gap fully closed. See `SSoT_Dev_Mode.md` §12.

### Dev Mode Phase 3.2 — complete (2026-04-23)
Gig-wide stat editing: Stats tab gains a Gig-Wide Stats section with SongHype slider, Inspiration slider, Cohesion stepper. Three wrappers on `GigManager` (`DevSetSongHype`, `DevSetInspiration`, `DevSetBandCohesion`) plus `LiveInspiration`/`MaxSongHype` getters. `CompositionSession` gains `DevSetCurrentInspiration` so the Inspiration slider affects the live session budget, not just PD. Dev Mode principle codified: Dev writes reproduce natural consequences — `DevSetBandCohesion(0)` dispatches `LoseGig()` (suppressed under Infinite Turns, same as the natural Breakdown path). **Code-vs-SSoT drift discovered and corrected 2026-04-24 via MB1:** the `LoseGig()` dispatch was never actually in code on 2026-04-23 despite ST-P32-4/-5 being recorded as PASS. MB1 added the dispatch + corrected the stale XML comment; re-validated via ST-MB1-1..4. See `SSoT_Dev_Mode.md` §9.5 + §9.8. Architectural finding surfaced: Inspiration is dual-sited (PD + `CompositionSession._currentInspiration`); see `SSoT_Dev_Mode.md` §13.4. See `SSoT_Dev_Mode.md` §13.

### Dev Mode Phase 3.3a — complete (2026-04-23)
Per-character stat editing + Flow gig-wide extension. Stats tab gains a Per-Character section with musician (Stress slider, MaxStress stepper, Composure stepper) and audience (Vibe slider, MaxVibe stepper) editors. Gig-Wide Stats section gains a Flow stepper (uniform ± applied to every musician's `DamageUpFlat` stacks; aggregate read via `GigManager.TotalFlowStacks`). New DevSet methods: `BandCharacterStats.DevSetCurrentStress/DevSetMaxStress`, `AudienceCharacterStats.DevSetCurrentVibe/DevSetMaxVibe`, `GigManager.DevAddFlowToAllMusicians`. Shared threshold helpers (`CheckBreakdownThreshold`, `CheckConvincedThreshold`) extracted so Dev and play paths cannot drift. Side-resolution: `AudienceCharacterStats.DevResetConvinced` implementation landed (previously doc-declared but unimplemented — resolved a silent `ALWTTT_DEV` compile break in `DevModeController.ResetConvincedAudience`). Latent finding: `HealthBarController.SetCurrentValue(duration=0f)` doesn't propagate the final value to the visual bar; workaround is a `0.1f` duration in Dev setters (see `SSoT_Dev_Mode.md` §14.5). ST-P33a-1..10 all passed. See `SSoT_Dev_Mode.md` §14.

### Dev Mode Phase 3.3b — complete (2026-04-24)
Status apply/remove picker on Per-Character section of Stats tab. Active-status readout with `[−1]`/`[Clear]` per row. Catalogue-backed `[◄][►]` picker with `[+1]` apply. No production-class patches — uses existing `StatusEffectContainer.Apply`/`Clear` API directly. Known limitation: gameplay flags (`IsConvinced`, `IsBreakdown`) not triggered by picker — use dedicated Dev actions for full consequences. Finding: shared catalogue on musician/audience prefabs shows all statuses to both; recommend splitting into separate catalogue SOs (asset-only change, zero code). ST-P33b-1..10 all passed. See `SSoT_Dev_Mode.md` §15.

### MB1 + MB2 — closed (2026-04-24)
Two micro-batches closed jointly. **MB1** corrected the `DevSetBandCohesion` code-vs-SSoT drift: real code never dispatched `LoseGig()` despite §13.2/§13.3 and ST-P32-4/-5 claims. One-line dispatch added + XML comment rewritten. ST-P32-4/-5 retroactively invalidated; re-validated as ST-MB1-1..4. See `SSoT_Dev_Mode.md` §9.8. **MB2** split the shared `StatusEffectCatalogueSO` into `_Musicians` (6 canonical statuses) and `_Audience` (empty at MVP; Earworm populates at M4.3). Musician and audience prefabs reassigned. No code change. ST-MB2-1..6 all passed. `SSoT_Dev_Mode.md` §15.4 marked resolved. See `SSoT_Dev_Mode.md` §9.9. **Open-micro-batches list now empty.**

### Latent multi-song action window bug — fixed (2026-04-20)
`GigManager._actionWindowOpen` and `_isBetweenSongs` now re-asserted at every `ExecuteGigPhase(PlayerTurn)` entry. Affected any multi-song gig (production and Dev Mode). See `SSoT_Runtime_Flow.md` §4.1 for the flag lifecycle table.

### Character hover highlight — M1.7 complete (2026-04-20)
URP 2D sprite outline shader, `SpriteOutlineController` (MaterialPropertyBlock, batching-safe). `CharacterBase.OnPointerEnter/Exit` wired. `BandCharacterCanvas` contextual stats present but disabled at prefab level.

### Status icon animations — M1.8 complete (2026-04-20)
`StatusIconBase.PlayAppear()` / `PlayDisappear()`. `[RequireComponent(CanvasGroup)]`. Inspector-tunable durations (default 1s) + AnimationCurves. Race-safe detach-before-disappear in `CharacterCanvas.HandleStatusCleared`. Smoke tests ST-M18-1..5 passed.

### Composition card face description — shortened (2026-04-21)
`BuildCompositionDescription` updated to role/part + `N modifier(s)` count badge only. Style-bundle asset filename no longer appears on the card face. Full modifier list and style-bundle reference will live in the right-click detail view (M1.10).

### M1.3a — complete (2026-04-23)
Card-effect text pipeline rebuilt and per-icon status tooltips wired.
- `StatusEffectSO.Description` field added (`[TextArea]`, 1–2 sentences).
- New `CardEffectDescriptionBuilder` static class under `ALWTTT.Cards.Effects` — single owner of card-effect text formatting for `ApplyStatusEffect`, `ModifyVibe`, `ModifyStress`, `DrawCards`. Uses TMP rich-text colors (buff green, debuff red, numbers amber), hides zero-delta effects, resolves target-type phrasing.
- `CardDefinitionDescriptionExtensions.GetDescription` action branch delegates to the builder. Enum-name leak (`CharacterStatusId` values surfacing on cards with `ApplyStatusEffect`) eliminated.
- `StatusIconBase` gained `IPointerEnter/Exit` handlers + `BindTooltipSource(StatusEffectSO, StatusEffectContainer, CharacterStatusId)`. Hovering a status icon on a character now shows `{DisplayName}` (or `{DisplayName} ×N` when stacked) with authored Description body.
- `CharacterCanvas.TryCreateIcon` wires the tooltip source after `SetStatus`.
- Description text authored on the six canonical status SOs: `flow`, `composure`, `choke`, `shaken`, `exposed`, `feedback`.
- `CardEffectSpec` remains data-only per `SSoT_Card_System.md` §6.1. Formatting is cross-cutting, held centrally.

### M1.3c — complete (2026-04-23)
Card-hover stacked tooltips (Monster Train-style).
- `CardBase.ShowTooltipInfo()` aggregates keywords (via `SpecialKeywordData`) + unique `StatusEffectSO`s extracted from `CardDefinition.Payload.Effects` filtered to `ApplyStatusEffectSpec.status`. Dedupe via `HashSet<StatusEffectSO>`. Display order: keywords first, statuses second.
- Mouse-follow positioning. Position bug root-caused (WorldToScreenPoint on canvas-edge RectTransform through HandCamera produced ~20000px screen coords on a 2560×1440 screen) and fixed by switching to mouse-follow mode.
- Card Editor `AddEffect` bug fixed: `GenericMenu` callback now calls `ApplyModifiedProperties` + `SetDirty` immediately. Fixes effect authoring for both Action and Composition payloads.
- `TooltipController` prefab: `VerticalLayoutGroup` (Upper Left, spacing 5, ControlChildSize Width+Height, padding 5) + `ContentSizeFitter` (Preferred Size on both axes).
- All seven smoke tests pass (ST-M13c-1..7).
- Deferred: raw Inspector `[SerializeReference]` drawer for `CardEffectSpec` (M1.1), composition card face `Effects` display (M4 design decision).
- SSoT edits applied at closure: `SSoT_Status_Effects.md` §3.3, `SSoT_Card_System.md` §10.

### M1.10 — complete (2026-04-23)
Right-click card detail view modal.
- `CardDetailViewController` singleton at `Assets/Scripts/UI/CardDetailViewController.cs`. Dedicated Screen Space – Overlay canvas (sort order 100), dim background with dismiss button, full card detail panel.
- `CardDefinitionDescriptionExtensions.GetDetailDescription()` added — composition cards show primary kind, style-bundle name, full modifier list via `PartEffect.GetLabel()` with scope/timing, and `CardPayload.Effects`.
- `CardBase.OnPointerDown` intercepts right-click → `Toggle(CardDefinition)`. Left-click unchanged.
- `HandController.DisableDragging()` while modal open; `EnableDragging()` on dismiss (Esc, background click, or right-click toggle).
- Smoke tests ST-M110-1..3, 6, 7 pass. ST-M110-4/5 retired (overlay blocks card input by design — close-then-reopen is the intended flow). ST-M110-8 retired (precondition impossible).
- Cosmetic items deferred: "COMPOSITION" word-break, panel overflow on long modifier lists.

### M1.3b — complete (2026-04-23)
SpecialKeywords enum + data audit, JSON importer improvements, Card Editor default fix.
- `SpecialKeywords` enum cleaned to 7 canonical values: `Stress`, `Vibe`, `Convinced`, `Tall` (resource/mechanic/audience) + `Consume`, `Exhaust`, `Ethereal` (card-trait). 6 legacy entries that duplicated status effects removed (`Chill`, `Skeptical`, `Heckled`, `Hooked`, `Blocked`, `Stunned`). Card assets cleaned of stale references.
- `SpecialKeywordData` asset populated with descriptions for `Consume`, `Exhaust`, `Ethereal`. Total 7 entries, one per enum value.
- JSON importer gained `keywords` string array on `CardJsonImport` DTO. Case-insensitive parsing, unknown values warned and skipped.
- JSON batch wrapper gained `defaultEntry` on `CardBatchJsonImport`. Merges into cards with absent/empty-flags entries. `JsonUtility` default-construction handled via `flags` discriminator.
- Exhaust coherence warning: `Debug.LogWarning` when `exhaustAfterPlay` bool and `Exhaust` keyword diverge. Non-blocking.
- Card Editor create wizard resets `Kind` to `Action` on open (fixes dual-button UX trap).
- All eight smoke tests pass (ST-M13b-1..8).
- Keyword model documented in `SSoT_Card_System.md` §3.3. JSON schema additions documented in `SSoT_Card_Authoring_Contracts.md` §5.3, §5.7, §5.8, §7.4.

### M1.9 — complete (2026-04-23)
Card sizing refactor in `HandController`.
- Serialized fields: `cardBaseScale` (float, default 1.0), `cardHoverScaleMultiplier` (float, default 1.25, relative to base), `scaleLerpSpeed` (float, default 12).
- Per-frame `localScale` lerp: cards smoothly grow to `cardBaseScale × cardHoverScaleMultiplier` on hover/drag, return to `cardBaseScale` otherwise.
- Curve reflow: `curveStart.x`, `curveEnd.x`, `handSize.x` multiplied by `HandScaleFactor` (= `cardBaseScale`). Cards at rest don't overlap when base scale changes.
- Proportional scaling: pop-up offset, fanning factor, hover-detection threshold all scale with `cardBaseScale`.
- `UpdateCurvePoints()` runs every frame — Bézier control points and raycast plane recompute from `transform.position`, so moving the `HandController` GameObject at runtime works correctly. Pre-existing bug where the curve didn't follow the GO is fixed.
- `AddCardToHand` sets initial `localScale` to `cardBaseScale` immediately (no pop-in flash).
- `RecalculateCurve()` public method + `OnValidate` (editor-only, play mode) for live Inspector tuning.
- All eight smoke tests pass (ST-M19-1..8) + GO-move verification.
- Temp debug logs tagged `[M1.9]` (12 markers) for diagnostics; strip later.

### Editor authoring tools
- **Card Editor** (`CardEditorWindow`) — single card authoring, JSON batch import, per-row Starter / Copies columns + toolbar Print button (batch (3), 2026-05-03).
- **Deck Editor** (`DeckEditorWindow`) — deck authoring with JSON import, catalogue browser, save/save-as, GigSetup registration, JSON export, toolbar Print button (batch (3), 2026-05-03). Core functional; polish items remain.
- **Card Inventory** (`CardInventoryWindow`) — read-only inventory browser for `CardDefinition` / `MusicianCardCatalogData` / `GenericCardCatalogSO` assets, with Print to Console + Export JSON per view. New batch (3), 2026-05-03.
- **Status Effect Wizard** (`StatusEffectWizardWindow`) — status SO authoring. HelpBox hint corrected 2026-04-20 to point at wired tick timings only.
- **Chord Progression Catalogue Wizard** (`ChordProgressionCatalogueWizard`).
- See `SSoT_Editor_Authoring_Tools.md`.

### Documentation
Governance migration complete. All subsystem SSoTs active and replacement-ready.

---

### M1.1 — Deck Editor polish — complete (2026-04-26)
Catalogue gains musician + effect-type filters. Staged and catalogue rows show cost badge + plain-text effect summary. Edit button calls `CardEditorWindow.OpenAndSelect`. Validation warns on missing action/composition cards. Save As remembers last-used folder. ST-M11-1..2 passed.

### Milestone 1 — Authoring & Testing Infrastructure — complete (2026-04-26)
All M1 DoD items checked. Full tool pipeline: Card Editor → Deck Editor → Dev Mode → play with animated icons, hover tooltips, right-click detail, stat editing, status apply/remove picker. General-audience testers can drive the game without developer supervision.

### M4.1 — Fix C1: unified Stress path — complete (2026-04-26)
`AddStressAction.DoAction` now routes through `ApplyIncomingStressWithComposure`. Composure absorbs audience pressure, Exposed amplifies it, Breakdown triggers on overflow. Audit finding C1 (2026-03-20) resolved. ST-M41-1..4 passed.

### M4.2 — Flow bifurcation + adaptive LoopScoreCalculator — complete (2026-04-28)
Flow bifurcated by card domain: Action cards use performer's individual Flow stacks as flat Vibe bonus; Composition cards and Song End use band-wide Flow stacks as Vibe multiplier (`flowVibeMultiplier = 0.08f`). Legacy Flow → SongHype path retired and removed from code. `LoopScoreCalculator` rewritten with adaptive scoring: `LoopScoringMode` enum (RoleNormalization / MusicianParticipation), `LoopScoringConfig` + `HypeThresholds` Inspector-tuneable structs, `possibleRoleCount` and `totalMusicians` auto-detected at gig start. Backing tracks now visible to scorer (`HasBacking` added to `LoopFeedbackContext`). Fields renamed with `[FormerlySerializedAs]` for serialization safety. ST-M42-1/1c/3/4/5/9/10/11 passed. ST-M42-2 deferred (no composition card with ModifyVibe in deck). ST-M42-6/7/8 deferred (need 2-musician gig, blocked on musician picker in Gig Setup).

- M4.3 (2026-04-28): Earworm — first active audience-side status. SO `StatusEffect_Earworm_DamageOverTime.asset` in `StatusEffectCatalogue_Audience`. Runtime hook in `GigManager.AudienceTurnRoutine` reads stacks → `AddVibe(stacks)` → container `Tick(AudienceTurnStart)` decays. Skips `IsBlocked`; ticks harmlessly on `IsConvinced`. Validated end-to-end via Dev picker and `TestEarworm.asset` card path.

### M4.6-prep batch (2) — Per-musician starter deck auto-assembly — complete (2026-05-02)
Runtime path that materializes the gig deck from each musician's `MusicianCardCatalogData` (starter-flagged entries, expanded by `starterCopies`) plus an optional `GenericCardCatalogSO` for "Owner: Any" cards. Closes the open item *"Per-musician starter decks"* tracked since M4.2 surfacing (2026-04-28). Closes Roadmap §4.4 deferred line "*`CardAcquisitionFlags.starterCopies` runtime consumption deferred to M4.6 when catalogue → starter-deck auto-assembly is implemented.*" 1 new file (`GenericCardCatalogSO.cs`), 4 modified (`PersistentGameplayData.cs`, `GigRunContext.cs`, `GigSetupConfigData.cs`, `GigSetupController.cs`). Decision matrix: D1 location → new method `PersistentGameplayData.SetBandDeckFromMusicians(IList<MusicianCharacterData>, GenericCardCatalogSO)`; D2a generic cards → new `GenericCardCatalogSO` (separate SO type, reuses `MusicianCardEntry`); D2b zero-copies-with-starter-flag → warn + skip; D3 `availableBandDecks` → demoted to dev fallback via new `useMusicianStartersToggle` (default ON); D4 roster source → use `pd.MusicianList` as-is, picker batch deferred to merged (1)/(4); D5 `MusicianCharacterData.BaseActionCards`/`BaseCompositionCards` → reframed as transitional helpers already deriving from `CardCatalog`, no dual-siting; D6 deck label → new `RunConfig.deckLabel` string. Provenance contract: per-musician contributions populate `musicianGrantedActionCards`/`musicianGrantedCompositionCards`; generic-catalogue contributions do NOT populate provenance, so `RemoveMusicianFromBand` correctly leaves them in the deck when a musician departs mid-run. Subtle case: when the same `CardDefinition` lives in both a per-musician catalog and the generic catalog, removal strips the per-musician copy and leaves the generic copy — correct per the contract (provenance follows contribution path, not card identity). Smoke tests ST-M46p2-1/2/3/5/6/7/8 PASS via console verification + temporary `[ContextMenu]` scaffold on `GigManager` (removed at closure); ST-M46p2-4 DEFERRED-by-construction (`MusicianCatalogService.TryAddEntry` editor-time clamps `starterCopies` to `Mathf.Max(1, …)` and `MusicianCardEntry.starterCopies` carries `[Min(1)]`, making the `starterCopies = 0 + StarterDeck-flagged` state unreachable from tooling; warn-and-skip code path is structurally identical to ST-M46p2-3's `skippedNoCatalog` path which PASSED). Side-finding: Card Editor's per-row UX for flagging starter cards (proposed bulk-action toolbar, then refined to per-row toggle column on the entries list) queued as batch (3). Side-finding: pre-existing `CardBase.SetCard` NRE at `CardBase.cs:77` when opening Draw/Discard/Hand inventory viewers (likely unassigned `inspirationCostTextField` reference on inventory card prefab) — surfaced during smoke tests, not caused by batch (2), queued as separate UI-fix batch.

### M4.6-prep UI-fix-A — Inventory viewer prefab NRE — complete (2026-05-02)
Closes the inventory-viewer NRE surfaced during M4.6-prep batch (2) smoke tests. Inventory canvas instantiates `CardUI.prefab` (an empty subclass `CardUI : CardBase {}` assigned to `InventoryCanvas.cardUIPrefab`); two `[SerializeField]` TMP refs on the prefab's `Card UI (Script)` component were unassigned: `inspirationCostTextField` and `inspirationGenTextField`. `CardBase.SetCard` (line 77 of the cited stack) writes to those fields unconditionally, producing the NRE on Draw/Discard/Hand pile open. Asset-only fix: wired both refs to the corresponding TMP_Text children on `CardUI.prefab`. `CardBase.SetCard` kept strict (no defensive null guards added — strict failure surfaces future authoring drift loudly). Smoke tests ST-INV-1..6 PASS (ST-INV-5 PASSED with both Action and Composition cards in mixed-pile view; ST-INV-6 confirmed gameplay card prefab unchanged, ruling out wrong-prefab edit). Structural finding parked: `CardUI : CardBase {}` empty subclass formalizes a two-prefab arrangement (gameplay card prefab + `CardUI.prefab`), which is the recurrence vector for unwired-`SerializeField` bugs on the inventory side. See §4 Open items for the parking note. No code shipped, no SSoT change.

### M4.6-prep UI-fix-B — Inventory scrollbar functional — complete (2026-05-02)
Closes the inventory ScrollRect snap-back / no-scrollbar symptom surfaced immediately after UI-fix-A. Root cause was layered: `Content` had `ContentSizeFitter` (Vertical=Preferred Size) but no `LayoutGroup` to feed it preferred height; and `Viewport` had `Mask` + a disabled `Image` (broken masking, would have manifested as card bleed once scrolling worked). Fix is asset-only on `InventoryCanvas.prefab` plus a small code edit on `InventoryCanvas.cs`. Asset edits: added `VerticalLayoutGroup` to `Content` (Padding 0 / Spacing 0 / Child Alignment Upper Center / Control Child Size W=ON H=OFF / Force Expand W=ON H=OFF); added `LayoutElement` to `FilterPanel` (Min Height=100, Preferred Height=100), to `CardSpawnRoot` (Preferred Height=2050), to `SongSpawnRoot` (Preferred Height=800); replaced `Mask` + disabled `Image` on `Viewport` with `RectMask2D`; reduced `CardSpawnRoot` Grid Layout Group Padding Top 150→50 (cosmetic). The `LayoutElement` strategy was required because `GridLayoutGroup` on a stretch-anchored `RectTransform` inside a `ContentSizeFitter` does not reliably report preferred height to its parent — explicit `LayoutElement.preferredHeight` bypasses this. Code edits in `InventoryCanvas.cs`: added `using UnityEngine.UI;`, added `[SerializeField] private ScrollRect scrollRect;` field (wired to the `Scroll View` GameObject in the prefab), and at the end of `SetCards` and `SetSongs` (after population) added a null-guarded reset block: `Canvas.ForceUpdateCanvases(); LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content); scrollRect.verticalNormalizedPosition = 1f;` — the `ForceUpdateCanvases` + `ForceRebuildLayoutImmediate` pair guards against the timing race where `verticalNormalizedPosition` samples stale Content bounds before the layout pass runs. Smoke tests ST-SCR-1/3/4/6/7 PASS, ST-SCR-2 FAIL ACCEPTED as paper cut (vacuous overflow: with `CardSpawnRoot.LayoutElement.preferredHeight = 2050` fixed, near-empty piles still produce overflow → scrollbar appears unnecessarily; cosmetic, follow-up via dynamic-height computation), ST-SCR-5 DEFERRED-by-construction (no Songs inventory content reachable in current build). See §4 Open items for the paper-cut note and FilterPanel-scrolls-with-content deferral.

### M4.6-prep batch (3) — Authoring tooling QoL — complete (2026-05-03)
Editor-only batch promoting authoring ergonomics surfaced during M4.6-prep batch (2) smoke tests. Closes the open item *"Card Editor per-row starter UX (queued as batch (3), surfaced 2026-05-02)"*. Three deliverables shipped in three files (one new, two modified), all `#if UNITY_EDITOR` guarded, zero runtime impact.

**(3.A) Per-row Starter / Copies columns on `CardEditorWindow`.** The catalog entry list's row rendering loop (formerly a single `GUILayout.Toggle(isSelected, label, "Button")` per entry) now renders each row as a horizontal scope containing: a `Starter` checkbox (~38 px), a `Copies` IntField (~40 px, greyed when Starter is off), and the existing selection button with a recomposed label (`[S]` flag dropped from the label since the checkbox column is the canonical indicator; `[R]` and `[L]` retained). Both inline controls write through `SerializedObject(_loadedCatalog)` → `entries.GetArrayElementAtIndex(i)` → `FindPropertyRelative("flags" / "starterCopies")` with a single `ApplyModifiedProperties()` per frame, giving Undo registration and asset-dirty propagation identical to the right-side inspector path. Clamp on commit: `if (newCopies < 1) newCopies = 1;` (mirrors the `[Min(1)]` attribute on `MusicianCardEntry.starterCopies` and `MusicianCatalogService.TryAddEntry`'s `Mathf.Max(1, …)`). IMGUI controls consume their own input events, so clicking the inline checkbox/IntField on a non-selected row does not change `_selectedEntryIndex` (the row's name button remains the only selection target).

**(3.B) `CardInventoryWindow` (new file).** New editor window registered at `ALWTTT/Cards/Card Inventory` (priority 12, immediately after Card Editor and Deck Editor). Four toolbar-selected views: All `CardDefinition` assets in project; All `MusicianCardCatalogData` with per-asset summary (entry count + starter count + total starter copies); One specific musician catalogue (full entry list, musician selected via toolbar dropdown); All `GenericCardCatalogSO` assets (each rendered with its full entry list since `GenericCardCatalogSO.Entries` reuses `MusicianCardEntry`). Each view supports `Print` (multi-line `Debug.Log`) and `Export JSON` (`EditorUtility.SaveFilePanel` → `JsonUtility.ToJson(_, prettyPrint: true)` → file written + `EditorUtility.RevealInFinder`). The export schema is human-readable / informational, not designed to round-trip through `DeckJsonImportService`.

**(3.C) Toolbar Print buttons on `CardEditorWindow` and `DeckEditorWindow`.** Both windows gain a `Print` button on their existing toolbars. Card Editor: appended to the toolbar's actions cluster after the Registries Ping button (`GUILayout.Space(10)` separator); disabled when no catalog is loaded; produces a `=== CARD EDITOR — CATALOG DUMP ===` block with musician, asset path, entry count, starter count + total starter copies summary, and one line per entry (id, kind, flags, copies, unlockId). Deck Editor: inserted between `Export JSON` and `Clear All`; produces a `=== DECK EDITOR — STAGED DECK DUMP ===` block with asset path, deckId, displayName, description, entry count + total copies summary, and one line per entry using `StagedCardEntry.ResolvedCard` to handle existing and pending cards uniformly (`[NEW]` suffix for pending entries; `×{count}` for M4.4 multiplicity).

Decision matrix at open: D1 menu path → `ALWTTT/Cards/Card Inventory` (priority 12) accepted; D2 export schema → human-readable informational accepted; D3 "Validate `CardBase` prefab variants" appendix → punted (logged as candidate authoring-tool addition in `SSoT_Editor_Authoring_Tools.md §14.5`); D4 per-row layout density → fixed pixel widths accepted (Starter 38 px, Copies 40 px); D5 silent disappearance on filter interaction accepted; D6 Card Editor Print button placement → toolbar (not entries-list header) accepted.

Smoke tests ST-AT3-1..8 all PASS:
- ST-AT3-1 (per-row Starter toggle commits to asset, persists across reload) PASS;
- ST-AT3-2 (Copies field disable + clamp to 1 on commit) PASS;
- ST-AT3-3 (filter interaction silent disappearance) PASS;
- ST-AT3-4 (Undo reverts both flag and copies as one step) PASS;
- ST-AT3-5 (CardInventoryWindow all four views populate, Print + Export succeed for each — verified via `inv1.json` / `inv2.json` / `inv3.json` / `inv4.json` exports) PASS;
- ST-AT3-6 (Print buttons on both windows produce formatted multi-line output — Card Editor verified on Conito catalog dump, Deck Editor formatter uses `ResolvedCard` and `count`) PASS;
- ST-AT3-7 (regression: per-row controls do not steal selection) PASS;
- ST-AT3-8 (dogfood acceptance: Cantante cleanup workflow materially faster than right-side inspector) PASS, "very good cleanup process" reported.

**Critical scope honesty.** Batch (3) ships the *tooling* needed to execute the M4.6 starter-deck cleanup. The *content cleanup itself* — pruning the four musician catalogues from their current 28-entries-all-Starter-flagged state to the 12-card / 7-unique / 2-musician Cantante+Sibi composition specified in `Design_Starter_Deck_v1.md §4` — is a **separate follow-up**. ST-AT3-8 demonstrated the workflow on at least one musician but the test does not assert that all four catalogues now match the design spec. The pre-demo blocker tracked as the "all-starter-flagged catalog content" item in §4 is now **structurally tractable** but **content-status undetermined**; a fresh `CardInventoryWindow > All Musician Catalogs > Export JSON` snapshot compared against `Design_Starter_Deck_v1.md §4` is the recommended next verification step. Side-finding: the inventory exports captured during ST-AT3-5 (pre-cleanup, snapshotted in this session's outputs) provide a clean before-state baseline for that comparison.

### M4.6F-3 — Per-loop draw + per-loop inspiration hook + canonical AddCurrentInspiration — complete (2026-05-08)

Closes the third M4.6-followup batch. Three deliverables shipped:

1. New `GigFlowSettingsSO.DrawPerLoop` field (default 0, "0 = disabled" semantic). Hand-cap clamp delegated to `DeckManager.DrawCards`.

2. Per-loop hook in `GigManager.OnCompositionLoopFinished` (host-owned subscriber to `CompositionSession.LoopFinished`). Reads `flow.DrawPerLoop` and `pd.InspirationPerLoop`, calls `DeckManager.Instance.DrawCards(N)` and `_session.AddCurrentInspiration(N)`. Early-returns when both inputs are 0 (no log, no work). Log gated on `dev.UseLogs && dev.UseCompositionLogs`. Hook lives in GigManager rather than CompositionSession.HandleLoopFinished to respect the existing `[Obsolete]` deck-non-mutation invariant on `CompositionSession.PrepareDeck` and `ICompositionContext.Deck`.

3. `CompositionSession.AddCurrentInspiration(int delta) → int` promoted to canonical session-budget mutator. Clamps to `pd.MaxInspiration`, refreshes `CompositionUI.SetInspiration`, mirrors to `pd.CurrentInspiration`. Returns actual delta applied. Track-derived per-loop gain (`HandleLoopFinished` lines 532–540 region) refactored to route through it. The `+N` badge continues to display the un-clamped per-loop track contribution (player-facing signal of next-loop potential, independent of cap) — the actual gain is reflected only in the inspiration value itself.

Decisions locked at batch open: D1 new `drawPerLoop` field on `GigFlowSettingsSO` (not on JamRules); D2 single hook for both draw and inspiration; D3 raw `DrawCards(N)` with internal hand-cap clamp (no M4.5 subtractive guarantee); D4 default `drawPerLoop = 0`. **Resolved during batch:** D5 hook placement → `GigManager.OnCompositionLoopFinished` (Option B) to respect deck-non-mutation invariant; D6 F4a Dev slider symptom → auto-resolved by D7's consolidation at the loop-boundary level (instant-update path requires MB3 drift correction); D7 → Option A consolidated `AddCurrentInspiration` clamp + dual-mirror.

Side-findings flagged during batch:
- §13.4 Dev surface drift: four documented-but-missing surfaces (`GigManager.LiveInspiration`, `GigManager.DevSetInspiration` session routing, `CompositionSession.CurrentInspiration` getter, `CompositionSession.DevSetCurrentInspiration`). ST-P32-1..3 honesty correction needed. Bundled into MB3.
- Session-start residual dual-siting: `CompositionSession.Begin/ConfirmCurrentPartAndStart/AdvanceToNextPart` reset `_currentInspiration` to `_rules.inspirationPerPart` without PD mirror, so `pd.InitialGigInspiration` is honored in PD but ignored by the live session. Bundled into MB3 with carry-over semantic for `inspirationPerPart=0`.
- F-2 D4 follow-up surfaced: `MaxInspiration` and `MaxCardsOnHand` should move to `GigFlowSettingsSO` consistent with `DefaultInitialGigInspiration` / `DefaultInspirationPerLoop`. Post-demo priority.
- `JamRules.drawPerPart` flagged with XML `<remarks>` as UNUSED, slated for F-5 Part→Loop cleanup.

Smoke tests:
- ST-F3-S1 (baseline regression, both inputs 0) PASS — early-return silences hook.
- ST-F3-S2 (typical case, drawPerLoop=2 + inspirationPerLoop=1) PASS*. Slider drift caveat → MB3.
- ST-F3-S3 (hand-cap clamp) PASS.
- ST-F3-S4 (F-3 inspiration cap clamp) PASS.
- ST-F3-S4b (track-derived clamp regression after consolidation, with badge revert) PASS — `+3` badge correctly displays un-clamped track contribution; inspiration value clamps to MaxInspiration.
- ST-F3-S4c (Dev slider responsiveness during active session) FAIL DEFERRED — depends on MB3 drift correction.
- ST-F3-S5 (multi-loop accumulation) PASS.
- ST-F3-S6 (log gating on `useCompositionLogs`) PASS.
- ST-F3-S7 (F-1 single-discard regression with F-3 active) PASS.

Files changed:
- `Assets/Scripts/Data/Gig/GigFlowSettingsSO.cs` — new `drawPerLoop` field + `DrawPerLoop` getter.
- `Assets/Scripts/Music/CompositionSession.cs` — `using ALWTTT.Managers;` added; new canonical `AddCurrentInspiration(int) → int` method; `HandleLoopFinished` track-derived block refactored to route through it.
- `Assets/Scripts/Managers/GigManager.cs` — `OnCompositionLoopFinished` extended with F-3 hook (per-loop draw + per-loop inspiration via canonical mutator + log gate).
- `Assets/Scripts/Interfaces/ICompositionContext.cs` — XML `<remarks>` added to `JamRules.drawPerPart` flagging it UNUSED, F-5 review pointer.

**MB3 (2026-05-08) — Inspiration Dev surface drift correction + session-start dual-siting fix.** Four documented-but-missing surfaces (`GigManager.LiveInspiration`, `GigManager.DevSetInspiration` session routing, `CompositionSession.CurrentInspiration`, `CompositionSession.DevSetCurrentInspiration`) implemented and gated under `#if ALWTTT_DEV`. Added carry-over branch to `CompositionSession.Begin / ConfirmCurrentPartAndStart / AdvanceToNextPart` for `JamRules.inspirationPerPart == 0` via private `ResolveSessionStartInspiration` helper. ST-P32-2 / ST-P32-3 retroactively invalidated. ST-MB3-3 INVALID by reachability (CompositionSession is alive for the entire PlayerTurn — lifecycle clarification surfaced). ST-MB3-1/2/4/8 PASS; ST-MB3-5/6/7 deferred to loop-game-flow milestone. Closes ST-F3-S4c. See `SSoT_Dev_Mode.md` §13.4 / §9.10.

**MB4 (2026-05-08) — Action-card inspiration session routing.** `CardBase.SpendInspiration` and `CardBase.GenerateInspiration` now route through a new public `GigManager.AdjustInspiration(int delta)` wrapper that delegates to `CompositionSession.AddCurrentInspiration` when a session is active and writes PD directly otherwise. Closes the user-reported critical bug where action-card and SFX-card spend bypassed the session budget, leaving the composition UI stale. PD ↔ session ↔ comp UI now stay in sync across action, SFX, comp-card, per-loop-gain, and Dev paths. **Behavior tightening:** over-spend on action cards now clamps at 0 instead of producing negative `pd.CurrentInspiration`. The one remaining un-mirrored write site is `TryPlayCompositionCard` step 8 (comp-card spend during build phase) — preserved intentionally as the §13.4 caveat. **MB4-diag:** added `GigManager.IsCompositionSessionActive` getter and a Stats-tab raw `[PD/Session]` readout for dual-siting visibility. **Open finding:** `CanPlayActionCard` lacks an inspiration-cost gate (MB5 candidate, not scheduled). ST-MB4-1..5 all PASS. See `SSoT_Dev_Mode.md` §13.4 / §9.11.

### M4.6F-4 Stage A — SongOrchestrator IOOR defense + diagnostic + D3-B recursion guard — complete (2026-05-08)

Closes the fourth M4.6-followup batch on a Stage A scope. Three deliverables shipped, all in two files.

**Defense (D2-A, production-quality).** `MidiMusicManager.RenderSinglePart` (`Assets/Scripts/Managers/MidiMusicManager.cs`) — broad `try { ... } catch (Exception ex) { ... }` around the `generator.Orchestrator.GenerateSinglePart` call plus its serialization (merged write + per-stem write). On catch, returns `(null, null, 0f, 0, null)` — same shape as the existing `partIndex`-out-of-range early-return at line 593 — so `CompositionSession.PlaySinglePartLoop`'s pre-existing `merged == null || seconds <= 0f` branch fires unchanged. Catch handler emits `Debug.LogError` with full per-track detail (channel, role, musicianId), `ChannelRoles` and `ChannelMusicianOrder` dumps, exception type + message + stack trace. Try-catch is permanent; only the catch's diagnostic dump strips at full F-4 closure.

**D3-B within-part recursion guard (production-quality).** `CompositionSession.HandleLoopFinished` (`Assets/Scripts/Music/CompositionSession.cs`) — the within-part `if (_loopsRemainingForPart > 0)` branch now captures `PlaySinglePartLoop`'s return and calls `End()` on `secs <= 0f`, mirroring `AdvanceToNextPart`'s pattern at lines 732-733. Without this guard, a render failure mid-part would leave `_loopStartTime` / `_loopDurationSeconds` stale (PlaySinglePartLoop only updates them on success at lines 532-533) and the `Update`-tick consumer would spin re-firing HandleLoopFinished. The guard codifies an invariant that was already implicit at the AdvanceToNextPart call site. Permanent; not strip-tagged.

**`[F-4]` diagnostic logs (D4-A, temporary).** Two lime-tagged entry logs fire on every cache-miss render: `[F-4][CompSession] RenderSinglePart call: ...` immediately before `mm.RenderSinglePart(...)` (in `PlaySinglePartLoop`), and `[F-4][MMM] RenderSinglePart entry: ...` after `channelMap` is built (in `RenderSinglePart`). Counts agree across the boundary in healthy gigs (verified ST-F4-S1). The catch handler emits an `[F-4][MMM]` `LogError` with full arg dump on exception. All `[F-4]`-tagged log lines strip at full F-4 closure (Stage B); the surrounding try-catch and D3-B guard are kept.

**Decisions locked at batch open:**
- D1=A two-stage batch (Stage A diag + defense; Stage B routing parked).
- D2=A defense in `MidiMusicManager.RenderSinglePart` around `generator.Orchestrator.GenerateSinglePart` (broadened pragmatically to cover serialization too — same graceful-fail consequence).
- D3=B within-part recursion guard added in Stage A (user override of recommendation; mirrors AdvanceToNextPart pattern).
- D4=A `[F-4]`-tagged logs always-on, strip at closure (F-1 precedent).

**Stage A test results.**
- ST-F4-S1 PASS — paired `[F-4]` entry logs fire once per cache-miss render, counts agree across boundary, song completes; no LogError.
- ST-F4-S2 DEFERRED-non-repro — IOOR did not surface in test session at `loopsPerPart=4`. Defense correctly silent (no exception thrown). No arg dump captured for Stage B routing.
- ST-F4-S3 PASS-vacuous — no catch fired this session, no spin to evaluate. End-of-session editor errors on stop-play (`SerializedProperty has been Disposed`) are unrelated standard Unity inspector pattern.
- ST-F4-S4 N/A — no LogError data to evaluate.
- ST-F4-S5 BLOCKED-OUT-OF-SCOPE — Player build fails on package-internal `MidiGenPlayConfig` errors (`GetChordWriteFolder`, `GetProfileForTonality`) inside `D:\Projects\MidiGenPlay\MidiGenPlay\Runtime\CoreScripts\Services\PatternRepositoryResources.cs:87` and `\Composition\SongOrchestrator.cs:142,326`. ALWTTT-side editor compile clean. F-4 edits do not reference these methods. Tracked as a separate MidiGenPlay-project batch (rehydration prompt provided).
- ST-F4-S6 PASS — D3-B guard does not regress healthy multi-loop play (cache-hit loops continue to fire without invoking RenderSinglePart; cached duration returns > 0; guard does not trigger End()).

**Stage B parking.** Reopens automatically if `[F-4][MMM]` LogError fires during playtest. Captured arg dump routes to D5-A (ALWTTT cfg-construction fix in `SongConfigBuilder` or upstream) or D5-B (forward minimal repro to MidiGenPlay package owner). If F-4 reaches M4.6 demo closure without natural recurrence, retroactive D5-C path applies: strip `[F-4]` diagnostic logs, keep defense + D3-B guard as permanent quality improvements, declare F-4 fully closed.

**Files changed:**
- `Assets/Scripts/Managers/MidiMusicManager.cs` — `+58 lines net`. Entry log + try-catch + catch-dump LogError + return failure tuple. The original orchestrator call + serialization are now inside the try block.
- `Assets/Scripts/Music/CompositionSession.cs` — `+27 lines net`. Entry log in `PlaySinglePartLoop` (+19) and D3-B guard in `HandleLoopFinished` (+8). The within-part recursion now captures PlaySinglePartLoop's return and gates on `secs <= 0f`.

**Out-of-scope concern logged.** MidiGenPlay-side build errors on `MidiGenPlayConfig.GetChordWriteFolder` / `GetProfileForTonality` — package-internal, no ALWTTT fix path per `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §2.2. Editor compile clean (methods likely `#if UNITY_EDITOR`-gated or in editor-only assembly while package runtime calls them unguarded). Separate MidiGenPlay-project batch with full rehydration context.

### M4.6F-2 — GigSettings multi-SO refactor — complete (2026-05-07)

Closes the second M4.6-followup batch. Pure refactor: no semantic gameplay change, no new mechanics, no new content. Five competing settings homes collapsed to a clearer four-SO structure on the GigManager side plus a renamed roster SO on the Gig Setup side.

**The split:**
- `GigFlowSettingsSO` (NEW) — JamRules, Action card gating, Gig End behavior, setup-screen defaults (the former `GigSetupConfigData` "Default Values" header).
- `MeterTuningSO` (NEW) — SongHype caps/seed, Vibe/Hype balance, Flow→Vibe (bifurcated MVP), `LoopScoringConfig`, `HypeThresholds`, `breakdownStressResetFraction`.
- `GigPresentationSO` (NEW) — Audience beat curve/threshold, idle BPM, sequence pacing.
- `GigDevSettingsSO` (NEW) — Inspector-time toggles only (`useLogs`, `useCompositionLogs`, `debugSongHype`, `debugInstrumentPicker`, `debugMusicianVolume`). D6 strict scope.
- `GigSetupRosterSO` (RENAMED from `GigSetupConfigData`) — pure roster content (decks, encounters, audience pool, generic catalog, max audience).

**Decisions locked at batch open:** D1 4-SO split; D2 GigSetupConfigData split into Roster + flow defaults; D3 JamRules kept as struct on `GigFlowSettingsSO` with `CompositionSession.Begin(JamRules, …)` signature untouched; D4 `GameplayData↔PersistentGameplayData` duplication of `drawCount`/`keepInspirationBetweenTurns`/etc. deferred out of F-2; D5 hand-author the four new SO assets; D6 `GigDevSettingsSO` scoped strictly to inspector-time toggles. Scene refs (cameras, hand, position lists, scene changer, composition UI, MidiGenPlayConfig boundary, songHypeDebugSlider, background container) remain inline on `GigManager`.

**Façade properties preserved on GigManager:** `FlowActionFlatBonus`, `FlowActionVibeBonusPerStack`, `FlowVibeMultiplier`, `BreakdownStressResetFraction` — backed by `MeterTuningSO`. No external caller signature change.

**Serialization continuity:** `GigSetupRosterSO` carries `[MovedFrom(autoUpdateAPI: true, sourceClassName: "GigSetupConfigData", sourceNamespace: "ALWTTT.Data")]` so the existing `GigSetupConfig.asset` retains its serialized data when renamed in Unity. `ALWTTTProjectRegistriesSO.gigSetupRoster` and `DeckEditorWindow._gigSetupRoster` carry `[FormerlySerializedAs]` so their existing wiring survives.

**Breaking change:** `PersistentGameplayData.ApplyRunConfig(RunConfig, GigSetupConfigData)` → `ApplyRunConfig(RunConfig, GigSetupRosterSO, GigFlowSettingsSO)`. Only call site in project is `GigSetupController.OnStartPressed`. No external callers found.

**Smoke tests:** ST-F2-S1/2/3/6/7/8 PASS. ST-F2-S4 PASS with side-finding — the `(Flow ×N)` floating-text on song-end vibe resolution does not appear visually; code path is unchanged from pre-F-2 (`GigManager.RunSongVibeResolution` gated on `flowStacks > 0 && FxManager.Instance != null`). Pre-F-2 issue surfaced during F-2 validation; not in F-2 scope. ST-F2-S5 FAIL — expected; per-loop draw is the M4.6F-3 batch.

**Side-findings flagged for F-3 design:**
- `JamRules.drawPerPart` is serialized but no consumer reads it. `PersistentGameplayData.InspirationPerLoop` is assigned in `ApplyRunConfig` but no consumer reads it either. Both look like the unwired half of the per-loop story F-3 is meant to fix. F-3 will need to either add `drawPerLoop` to `GigFlowSettingsSO` or repurpose `drawPerPart`.

**Files changed:**
- 5 NEW: `GigFlowSettingsSO.cs`, `MeterTuningSO.cs`, `GigPresentationSO.cs`, `GigDevSettingsSO.cs`, `GigSetupRosterSO.cs`.
- 8 MODIFIED: `GigManager.cs`, `GigSetupController.cs`, `PersistentGameplayData.cs`, `ALWTTTProjectRegistriesSO.cs`, `DeckAssetSaveService.cs`, `DeckEditorWindow.cs`, `GigRunContext.cs`, `GenericCardCatalogSO.cs` (`GenericCardCatalogSO.cs` is xmldoc-only).
- 1 DELETED: `GigSetupConfigData.cs`.

**Asset changes:** `GigSetupConfig.asset` renamed to `GigSetupRoster.asset` in Unity (GUID-tracked rename; reference survives via `[MovedFrom]`). Four new SO assets hand-authored from pre-F-2 Inspector values: `GigFlowSettings.asset`, `MeterTuning.asset`, `GigPresentation.asset`, `GigDevSettings.asset`.

### M4.6F-1 — Action card double-discard — complete (2026-05-07)

Closes the first M4.6-followup batch. Bug class **misdiagnosed at intake** as a reshuffle/pile lifecycle defect; instrumentation routed correctly to root cause via `[F-1]` logs across `DeckManager.cs` (5 sites), `CardBase.cs` (1 site), `InventoryCanvas.cs` (1 site). Reshuffle data path was always correct; bug was upstream in the play pipeline.

**Root cause.** Two independent paths called `DeckManager.OnCardPlayed` for the same played `CardBase` instance:
- `HandController.PlayCard:580-581` — unconditional on `played == true`.
- `CardBase.Use:93` (SFX synchronous) **or** `CardBase.CardUseRoutine:131` (non-SFX deferred, after `ExecuteEffects` yields).

For action cards (Warm Up, Take Five, Mind Tap), both call sites fired. The `IsExhausted`/`IsPlayable` guards in `CardBase.Discard` did not catch the second call because `DiscardRoutine` animates over `discardDuration` before `Destroy(gameObject)`. Each play removed **two** `CardDefinition` references from `HandPile` and added **two** to `DiscardPile`. Composition cards bypass `CardBase.Use` (via `GigManager.TryPlayCompositionCard`), so they had only the HandController call and were not affected.

**Fix.** `HandController.PlayCard:580-602` — gate the `OnCardPlayed` call to `IsComposition` only. Action cards keep their internal Use-pipeline discard timing (which is correct because `CardUseRoutine` defers `OnCardPlayed` until after `ExecuteEffects` yields, ensuring effects resolve before `DiscardRoutine` destroys the card).

**Side fix at the same gate:** latent SFX action card double-discard (no SFX cards in the current deck, so user-invisible, but the bug existed in code).

**Architectural finding now documented as invariant:** each successful card play results in **exactly one** `DeckManager.OnCardPlayed` call. The single call site varies by card type (Composition → HandController.PlayCard; SFX action → CardBase.Use:93; non-SFX action → CardUseRoutine:131). Promoted to `SSoT_Card_System.md §9.3` and `ssot_manifest.yaml`. The bug was probably introduced because this invariant existed only implicitly.

**Suspicion audit at closure:**
- S-A (missing `SetPileTexts` at reshuffle): not the root cause. Cosmetic concern remains but is not blocking. Tracked as a follow-up candidate.
- S-B (duplicate `DeckManager` instance): ruled out. `Awake` log showed `FIRST instance bound. id=-107914`; every subsequent `DM_id` matched.

**Smoke tests** (six total: 3 from audit doc + 3 added for the fix, all PASS):
- ST-DOUBLE-1 — action card single-discard — PASS (one `Discard FIRING` + one `OnCardDiscarded` per Warm Up play; HandPile -1, DiscardPile +1).
- ST-DOUBLE-2 — composition card single-discard regression — PASS.
- ST-DOUBLE-3 — multiplicity preservation across gig — PASS.
- ST-RESHUFFLE-1 — full deck cycle — PASS (AFTER CLEAR `discard=0`, `DM_id` invariant).
- ST-RESHUFFLE-2 — filtered draw reshuffle — PASS.
- ST-RESHUFFLE-3 — clone regression — PASS.

**Files changed:**
- `Assets/Scripts/Controllers/HandController.cs` — `PlayCard` method, `OnCardPlayed` call gated to `IsComposition` (+21 lines net, includes inline rationale comment).

**Files temporarily instrumented and reverted at closure:** `DeckManager.cs`, `CardBase.cs`, `InventoryCanvas.cs` — all `[F-1]`-tagged logs removed.

### M4.6-prep cleanup — Starter deck authoring + Card Editor tooling — complete (2026-05-06)

Closes the pre-demo blocker tracked since M4.6-prep batch (2): test catalogs were all-starter-flagged for tooling validation; M4.6 demo requires the designed 12-card / 10-unique / 8-Composition + 4-Action composition per `Design_Starter_Deck_v1.md §4`. 10 cards authored from scratch via JSON Import (Robot 4 + Gusano 4 + Generic 2). Existing test/scaffold cards in Robot/Gusano deleted by user during authoring. Final post-cleanup state (`inv4.json` snapshot): Robot entryCount=4 starterCount=4 starterCopiesTotal=5; Gusano 4/4/4; Generic 2/2/3. Cantante and Conito catalogs untouched but inert (not in demo roster — Cantante 7 entries all starter-flagged, Conito 10 entries all starter-flagged; both cosmetically out-of-spec but not in the demo path). Style bundles `Backing Card Config - Core Minor` and `Backing Card Config - Core Major` reused; `Melody Card Config - Test` reused as placeholder for Singing Field. 4 `MeterEffect_*` part effects reused.

Smoke tests ST-SD-1..ST-SD-8 from `Design_Starter_Deck_v1.md §10`: ST-SD-1/2/3/4/5/6/8 PASS; ST-SD-7 reclassified DEFERRED-by-design — Wormus Minor (Backing) and Singing Field (Melody) both have `FixedPerformerType: Sibi`, and the runtime model enforces "one musician = one track active at a time," so the second card replaces the first. This is a model invariant, not a cleanup defect. Test re-formulation deferred to roster expansion (Sibi-Backing + future-Melody-musician).

Two Card Editor tooling patches delivered alongside the cleanup:
- **Patch 1 — Status dropdown classified.** `DrawStatusEffectPicker` now reads from both `StatusCatalogueMusicians` and `StatusCatalogueAudience` post-MB2 split. UI rendered as `EditorGUILayout.DropdownButton` + `GenericMenu` with hierarchical paths `Musicians/<DisplayName>` and `Audience/<DisplayName>` plus a `<None>` entry. Replaces the prior flat `EditorGUILayout.Popup` that only consumed the legacy musicians-only `StatusCatalogue` alias. Closes the open item `Card Editor inline effects-block UI on legacy catalogue alias` from §4.
- **Patch 2 — Catalog Source toggle.** New `CatalogSource { Musician, Generic }` toggle in toolbar; in Generic mode auto-loads the `GenericCardCatalogSO` asset via `AssetDatabase.FindAssets("t:GenericCardCatalogSO")` with a name-heuristic preference for assets without "Test" in the filename. Generic mode renders entry list with the per-row Starter/Copies UI from batch (3.A). Write paths (Create Card, JSON Import, Add Existing, Sync From Assets) are **NOT** Generic-aware in this iteration — they remain disabled when `_catalogSource == Generic`, deferred as a future tooling QoL batch (touches `CardAssetFactory.CreateCardKindParams` and `MusicianCatalogService` signatures, both currently typed to `MusicianCardCatalogData`).

**Side-finding verified at closure:** the toggle handler at `CardEditorWindow.cs:244-249` correctly clears `_loadedCatalog` and `_loadedMusicianData` on switch-to-Generic, so the previously-flagged "writes mis-target a cached Musician catalog" concern does not exist. Combined with the write-disable guard at `CardEditorWindow.cs:544-545` (writes blocked while in Generic mode), the toggle is safe in its current shape.

Asset path side-finding (cosmetic, not functional): the 10 new starter cards live under `Assets/Resources/Data/Characters/Musicians/starter_*.asset` rather than under `Robot_Cards/` or `Gusano_Cards/` subfolders. Side-effect of `CardAssetFactory`'s default output path resolution. Not functional; reorganization at user's discretion.

### M4.6-prep merged (1)/(4) — Gig Setup roster pickers — complete (2026-05-04)
Bidirectional band + audience multi-select pickers shipped in the Gig Setup scene. Closes the open items *"Musician picker in Gig Setup"* (surfaced M4.2, 2026-04-28) and *"Gig Setup roster pickers"* (deferred from M4.3 surfacing). Two new files (`MusicianPickerRow.cs`, `AudiencePickerRow.cs`) + matching prefabs; five modified (`PersistentGameplayData.cs` — new `SetBandRoster(IList<MusicianBase>)`; `GigSetupConfigData.cs` — new `availableAudienceCharacters` + `maxAudienceCount`; `GigEncounterSO.cs` — new `BuildRuntime(IList<AudienceCharacterData> audienceOverride)` overload with regression-safe null fallback; `GigRunContext.cs` — new `RunConfig.audienceOverride`; `GigSetupController.cs` — picker fields, build/handler logic, validation, override decision, new serialized `gameplayData` field). GigSetupScene prefab + GigSetupConfig SO populated.

Decision matrix: D1=B (new `pd.SetBandRoster` method, distinct from `pd.AddMusicianToBand` which is the meta/recruit path); D2=A (audience pool via new `GigSetupConfigData.availableAudienceCharacters`); D3=B (toggle-list UI for both pickers); D4=remember-last + reset-on-encounter-swap (band picker remembers `pd.MusicianList` across visits; audience picker resets to encounter's baked default on encounter swap, with warning if user had customized); D5=band 1-4 / audience 1-`MaxAudienceCount` (band warns at 1, blocks at 0 or >4; audience blocks below 1 or above `MaxAudienceCount`); D6=B+C combined (`BuildRuntime(audienceOverride)` overload + `RunConfig.audienceOverride` field); D7=A (single merged batch covering both pickers).

Roster vs deck contract: `pd.MusicianList` is now mutated by the picker before the auto-assembly path runs, so `SetBandDeckFromMusicians` correctly reads the picked roster. Legacy path (`useMusicianStartersToggle = OFF` + `BandDeckData` dropdown) honors the band picker selection without leaking auto-assembly into deck content. Roster identity (the picker) and deck content (auto-assembly or `BandDeckData`) are independent concerns.

Audience-override decision rule: `DiffersFromEncounterAudience(picked, encounter)` returns true only when the picker selection differs from the encounter's baked `AudienceMemberList`. **Multiset-blind on baked duplicates** (post-batch fix, see side-findings): the picker UI dedups `AudienceCharacterData` by reference, so a no-customization run produces `pickedCount == bakedSet.Count` (unique-count), not raw `bakedCount`. Comparator builds `bakedSet` first, then compares against `pickedCount`. Consequence: encounters with duplicate audience entries (e.g., `[A, A, B]`) preserve duplicates at runtime when the user does not customize; override stays null and `BuildRuntime` falls back to the baked list. When the user customizes, the override list cannot represent multiplicity (single picker rows) and duplicates are lost for that run. Multiplicity-aware picker UI is a future concern (tracked: M4.6-prep batch (6), see §4).

Smoke tests ST-M46p4-1 through ST-M46p4-10 all PASS:
- ST-M46p4-1 (band picker basic — Cantante+Sibi, log + stage count) PASS;
- ST-M46p4-2 (auto-assembly content respects picker — `SetBandDeckFromMusicians` log shows per-musician + generic split, no third-musician contributions) PASS, with spec addendum: generic catalog contributions are expected on top of per-musician, not a violation of the "only" clause;
- ST-M46p4-3 (empty band guard — error logged, scene does not navigate) PASS;
- ST-M46p4-4 (single-musician warning — non-blocking warning logged, gig starts with 1 musician) PASS;
- ST-M46p4-5 (audience picker basic + override — picker deviation produces `override=True` with reduced count) PASS;
- ST-M46p4-6 (audience override null path regression — no customization → `override=False`, baked list used) PASS;
- ST-M46p4-7 (audience max-count enforcement — selecting > `MaxAudienceCount` blocks gig start) PASS;
- ST-M46p4-8 (encounter-swap audience reset — picker rebuilds with new encounter's defaults, warning logged when prior customization is discarded) PASS;
- ST-M46p4-9 (legacy regression — band picker honored on `BandDeckData` dropdown path, `SetBandDeckFromMusicians` does not fire) PASS;
- ST-M46p4-10 (multiset-blind override preserves baked duplicates — added during validation after side-finding surfaced; no-customization run on `[A, A, B]` encounter produces `override=False` and runtime stage shows duplicate A) PASS.

Side-findings:
- **`GameplayData` null at `Awake` time.** `BuildMusicianPicker` initially used `GameManager.Instance.GameplayData` which returned null at `Awake` order. Reworked to prefer a serialized `gameplayData` field on `GigSetupController` (wired in inspector), with the `GameManager.Instance.GameplayData` path as defensive fallback. Note: `GameplayData` on `GameManager` is an instance property, not static; the static-looking access pattern in some other classes (e.g. `GigManager`) works because those classes shadow the type name with a `private GameManager GameManager => GameManager.Instance;` property.
- **`RectTransform`-parenting warning.** Audience picker initially produced Unity's RectTransform-parenting warning on `Instantiate(prefab, parent)`. Pattern fixed via `Instantiate` + `SetParent(content, worldPositionStays: false)`. Same pattern applied to musician picker for consistency.
- **Multiset-blind override comparator (option-B fix).** Surfaced mid-validation: original `DiffersFromEncounterAudience` compared raw `bakedCount` to `pickedCount` first, which made encounters with duplicate baked audiences always trigger override (silent multiplicity loss + misleading `override=True` log on no-customization runs). Fix: build `bakedSet` first, then compare `bakedSet.Count` (unique-count) against `pickedCount`. ~5 LoC change. ST-M46p4-10 added to validate. The picker UI itself remains single-row-per-unique-SO; multiplicity-aware UI deferred.
- **Audience picker multiplicity follow-up.** The current picker cannot represent multiplicity in the UI — toggling A removes both instances when baked = `[A, A, B]`. When the user customizes, multiplicity is lost for that run. Tracked as M4.6-prep batch (6) Audience picker multiplicity (per-row count input + multiset-aware comparator). Not blocking M4.6 demo gate.

### Latent multi-song action window bug — fixed (2026-04-20)
`GigManager._actionWindowOpen` and `_isBetweenSongs` now re-asserted at every `ExecuteGigPhase(PlayerTurn)` entry. Affected any multi-song gig (production and Dev Mode). See `SSoT_Runtime_Flow.md` §4.1 for the flag lifecycle table.

## 2. Active work

### M1.3 decomposition — five sequenced batches (2026-04-21)
Original M1.3 scope expanded after UX review and split into five batches. Order: **M1.3a ✅ → M1.3c ✅ → M1.10 ✅ → M1.3b ✅ → M1.9 ✅**. All five batches closed 2026-04-23. See `Roadmap_ALWTTT.md` §1.3 for full scope per batch.

- **M1.3a — closed 2026-04-23.** See §1.
- **M1.3c — closed 2026-04-23.** See §1.
- **M1.10 — closed 2026-04-23.** See §1.
- **M1.3b — closed 2026-04-23.** See §1.
- **M1.9 — closed 2026-04-23.** See §1.


### Phase B — Gameplay loop polish (opened 2026-05-09, in progress)

Phase B is the post-pre-demo polish phase. Goal: take the working Phase A build to a true demo with persistence, feedback, content balance, and animation polish. Three planned batches with one preceding spike (now complete).

- **Phase B Spike — complete (2026-05-09).** Confirmed per-track persistence is feasible ALWTTT-side without MidiGenPlay boundary violation. Mechanism design + estimated cost + risk assessment delivered. See §1 Phase A close block and `changelog-ssot.md`.
- **B1 — Loop model simplification + track persistence + UI rework.** Foundational, highest risk, runs first. Disables next zone (UI collapses to current-only, F-5 absorbed); ships per-track stem cache for persistence (D2=B per-track simple, D6=A per-track scope, D7=B per-song lifetime); reworks composition session UI to show current tracks + Inspiration-next + pending-track visualization; stops mid-session hand discard on play.
- **B2 — Polish layer (feedback + animation).** Aditivo, low risk, depends on B1 landed. Tooltip miniature on track labels, Inspiration markers pop-up animation, expanded floating text (composition events + audience exclamations + multipliers with icons), SongHype thresholds → venue SFX (lights/smoke/fire), Robot/Worm/instrument animation polish. Monolithic by default; split fallback B2a (UI feedback) + B2b (animation) if pesado.
- **B3 — Content + design.** Aditivo, depends on B1. Inspiration cost/gen balance pass across the deck (cover 0/1/2/3 for cost and generated), new BPM cards (rhythm composition with `+/-BPM` and `2×BPM` effects), new Modulation cards (chord progression with key modulation), 1 designed audience member with 3 distinct abilities. Audience Member Wizard Editor deferred post-demo (D4=B).

**M4.3 — Earworm (2026-04-28).** First audience-side status implemented. Side fixes shipped: `ALWTTTProjectRegistriesSO` extended to expose both musicians and audience catalogues (`[FormerlySerializedAs]` preserved existing serialized reference); Card Editor JSON importer (`ApplyEffectsJson`) rewritten to probe both catalogues via `registries.TryGetStatusEffectByKey`; toolbar warning expanded to call out the specific missing field. `CardBase.ExecuteEffects` apply-time log expanded with `StatusKey` + `DisplayName` alongside primitive id (disambiguates shared-primitive variants). New `[Earworm]` tick log in `GigManager.AudienceTurnRoutine`. Initial patch shipped with a copy-paste duplicate `Tick(AudienceTurnStart)` block producing -2/turn decay; caught by ST-M43-2/3 stack-count observation; fixed by deletion before closure.

### Dev Mode Phase 3 — stat & state editing (in progress)
P3.1 ✅ + P3.2 ✅ + P3.3a ✅ + P3.3b ✅ (all closed). Phase 3 complete. P3.4 audience transparency panel deferred. Encounter modifier toggles deferred.

### Deck Editor — polish pass ✅ (closed 2026-04-26)
Catalogue filters (musician, effect type), card preview info, cross-tool Edit button, last-used save folder, enhanced validation. See §1.

### Contextual stats on hover — feature disabled (2026-04-20)
`BandCharacterCanvas` hover-to-show-stats path present in code but disabled at prefab level (`statsCanvasGroup` / `statsRoot` unassigned, `StatsRoot` GameObject off). Silent no-op. Revisit when visual density is tuned.

### Editor tooling documentation — complete
`SSoT_Editor_Authoring_Tools.md` active and registered. Updated 2026-05-03 with batch (3) additions: §3 inventory row, §4.6/§4.7 Card Editor batch (3) sections, §5.7 Deck Editor Print button, new §8 `CardInventoryWindow`, §13 file list, §14.5 prefab-variant validator candidate.

---

## 3. Next active

**Demo cut close sequence (S1-S5) + Vertical slice (S6-S8 = Phase C).** 8 sessions total per the 2026-05-23 planning reframe (see §1 entry). Strict left-to-right sequencing; no two open in parallel.

> **Sequence status (2026-06-15):** S1, S2, S3a, S3-audio closed. The audio work-stream opened after S3-audio: **M-AUDIO-MIX** (Dev Audio Mix tab + persisted balance + `SSoT_Audio.md`) and **AUDIO-SFX-FIX** (silent-default SFX + app-wide SFX level + scoped jitter) are **closed 2026-06-15**. **Next-active: AUDIO-OST** (Main Menu song; D-OST-HOME=B), then AUDIO-AMBIENCE + AUDIO-CHAR-PROFILES — sequenced in `planning/active/Roadmap_Audio.md`. Then **S4** (tutorial content). The audio batches are not part of the S1-S5 numbering; they slot before/around S4.

### Demo cut (S1-S5)

| Session | Scope | Owning doc(s) | Status |
| --- | --- | --- | --- |
| **S1** | B3-slate-F: `AudienceCharacterSimple.ResolveLoopEffect` real implementation + per-audience FT (visual-only; SFX deferred to S3 per D2 documented exception) | `Roadmap §5.3` addendum + `Design_Sensory_Contract §3` (S1 direct-call note) | **closed 2026-06-12** (ST-S1-1..8 PASS; see §1) |
| **S2** | Sensory Event Bus foundation: bus + 2 event types (`AudienceReactionEvent`, `SongEndVibeEvent`) + thin `SensoryFxAdapter` (VerifyOnly) + `GigManager` coexistence publish (direct calls retained). TutorialController deferred to S4 (D-S2-5); direct-call deletion + adapter Spawn flip deferred to S3 (D-S2-3=A coexistence). | `Design_Sensory_Contract §3` | **closed 2026-06-14** (ST-S2-1..7 + S2-9 PASS, S2-8 N/A; see §1) |
| **S3a** | Sensory polish (visual): §4 full audit; `SensoryFxAdapter` Spawn flip + direct-call deletion; FT polish; Kid "Tantrum" animator; smoke / fire VFX on stage 2 / 3 (asset #4). Audio split out per D-S3-1=B. | `Design_Sensory_Contract §4 + §5` | **closed 2026-06-14** (ST-S3-1..13 + S3-4 PASS; see §1) |
| **S3-audio** | Audio SFX layer: `AudioManager` flesh-out + `SoundBankSO` (central inventory + coverage) + `SensorySfxType` + `SensorySfxPresentation` + `SensoryAudioAdapter`; card-play + reaction + song-end + stage-crossing wired; `SfxStageCrossedEvent`. | `Design_Sensory_Contract §4 + §5A` | **closed 2026-06-14** (ST-SA-1..9 + A1..A4 PASS; placeholder clips; see §1) |
| **S4** | Tutorial content: pilot dialogues authored (5-8 entries); pause-menu revisit + reset UX | `Design_Tutorial_System §6 + §8` | queued |
| **S5** | Balance + validation + cover refresh + reward UI + §5.4 closure (absorbs B3-slate remainder: C / D / E-lite / G / H / I + design gaps #12-#15 or explicit deferral) | `Roadmap §5.4 + §5.5` | queued |

### Vertical slice (S6-S8 = Phase C)

| Session | Scope | Owning doc(s) | Status |
| --- | --- | --- | --- |
| **S6** | Run structure: minimal ship hub scene; gig → reward → ship → next gig flow; pilot portrait integration; tutorial dialogues `tut_ship_hub_intro` + `tut_first_reward_choice` | `Design_Vertical_Slice §3.1` + `Roadmap §7.1` | queued |
| **S7** | Run content I: 2 new venues (asset images 7 + 10); 2 new encounters; 4 new audience archetypes; audience state machine (idle / hostile / vibing); tutorial dialogue `tut_audience_state_machine` | `Design_Vertical_Slice §3.2 + §6` + `Roadmap §7.2` | queued |
| **S8** | Run content II: boss (`AudienceCharacterBase` subclass per D-RUN-4=α); closing sequence; tutorial dialogues `tut_first_boss_encounter` + `tut_run_complete` | `Design_Vertical_Slice §3.3 + §7` + `Roadmap §7.3` | queued |

### Post-vertical-slice (deferred per D-RUN-6)

Ship interior batches (bar, chill, rehearsal), space map, ladder-mode formalization, meta-progression, full audio pass. Not slotted into the 8-session plan. Captured in `Roadmap §7.5` (out of scope for Phase C) and "Future milestones" section.

### Standing directives applicable across all sessions

- **D1** Sound Design Priority (`Design_Project_Directives §D1`, promoted 2026-05-20).
- **D2** Sensory Contract (`Design_Project_Directives §D2`, promoted 2026-05-23). *(line reconstructed from the reframe block — same batch, low risk)*
- **D3** Tutorial-as-mandatory (`Design_Project_Directives §D3`, promoted 2026-05-23). *(line reconstructed from the reframe block — same batch, low risk)*

## 4. Open items and risks

### Open items (non-blocking)
- **Placeholder SFX → final authoring (D1).** S3-audio shipped with placeholder clips. Final, intentional, non-generic SFX per `AudioActionType` + `SensorySfxType` is a tracked follow-up (sound exists, so §7 is satisfied; the quality upgrade is documented, not deferred-as-silent). Per-character profiles (AUDIO-CHAR-PROFILES) are the structured surface for this.
- **Music mix / "sometimes too loud" — DONE (M-AUDIO-MIX, closed 2026-06-15).** Dev "Audio Mix" tab + persisted per-musician/global/master-SFX balance shipped; `SSoT_Audio.md` created (D-AUDIO-SSOT=B). Remaining audio work (OST, ambience, per-character profiles) is sequenced in `planning/active/Roadmap_Audio.md`. Boundary held: `MIDIInstrumentSO.volume01` not wired.
- **Sensory Contract §4 full audit descoped from S2 → S3 (2026-06-14).** S2 was deliberately narrowed (bus + 2 event types + 2 publish sites + 1 subscriber) and did not walk the whole codebase to fill the §4 sensory-coverage audit, despite the original §3/§4 wording implying it would. The full audit lands in S3 alongside SFX coverage. Non-blocking; only the two S2 rows carry an as-built emission-path update today.
- **Shaken restrictions:** status applies and expires correctly; no gameplay gate yet. Design decision still open.
- **Audience Feedback DoT:** no Stress path on `AudienceCharacterBase`. Deferred.
- **Composure penalty during Shaken:** design intent only; not code-enforced.
- **True card copies in decks:** RESOLVED by M4.4 (closed 2026-04-29). `BandDeckData` is now a multiset; `PersistentGameplayData.SetBandDeck` expands counts into independent references; pile lifecycle preserves identity per reference. See `SSoT_Card_System.md §13` and `SSoT_Card_Authoring_Contracts.md §5.10`.
- **M4.5 architectural decision (filtered-draw mechanism):** RESOLVED 2026-04-30. Option 1 (predicate-based filtered draw on `DeckManager`) + subtractive budget rule. Two-hook framing collapses to single PlayerTurn-entry site because action and composition windows open simultaneously. Composition wins tie-break. See `SSoT_Runtime_Flow.md §4.2` and §1 M4.5 closure block.
- **M1.2 multi-turn validation gap** — fully closed (T5/T8 Phase 2, T7 Phase 3.1). No remaining deferred tests.
- **Choke-on-stunned design decision:** T5 surfaced that `HandController.TryResolveCardTarget` refuses stunned musicians. MVP decision: keep the refusal.
- **`CardActionTiming` default excludes PlayerTurn:** documented in `SSoT_Dev_Mode.md` §8.4 and `SSoT_Card_Authoring_Contracts.md` §3.4.
- **C1 — resolved (2026-04-26).** `AddStressAction` now routes through `ApplyIncomingStressWithComposure`. See §1 M4.1 closure block.
- **`CompositionCardPayload.effects` support — verified (2026-04-23):** ST-M13c-6 confirmed that `CardPayload.Effects` on composition cards works end-to-end (status tooltip appears on hover, effect authored via Card Editor). `Four on the Floor`'s `ApplyStatusEffect(flow)` co-effect is viable.
- **Raw Inspector `[SerializeReference]` drawer for `CardEffectSpec`:** Unity's default property drawer doesn't show a type menu for new list elements. Card Editor window is the intended authoring path. Defer custom drawer to M1.1.
- **Composition card face does not surface `CardPayload.Effects`:** by design (2026-04-21 simplification). Tooltip covers discoverability. Design question for M4 when composition cards with effects ship in player content.
- **Runtime tuning values pending from user:** `maxVibeFromSongHype`, `MaxCardsOnHand`, draw-per-turn. Required for calibrating VibeGoals of Heckler/Critic encounter archetypes. Flow tuning values now landed (`flowActionVibeBonusPerStack = 1`, `flowVibeMultiplier = 0.08f`, Inspector-tuneable). Does not block M4.3; does block the starter v1 authoring tuning pass in M4.6.
- **Keyword-driven runtime behavior (surfaced M1.3b, 2026-04-23):** `ExhaustAfterPlay` bool and `Exhaust` keyword are currently independent. Planned resolution: retire per-keyword bools in favor of `Keywords.Contains(...)` checks, making the keywords list the single source of both tooltip and runtime behavior. Touches the card-play pipeline. Not yet scheduled.
- **Inspiration dual-siting (surfaced M1.5 P3.2, 2026-04-23 — substantially closed by 2026-05-08):** `pd.CurrentInspiration` and `CompositionSession._currentInspiration` are mirrored via `CompositionSession.AddCurrentInspiration` on all canonical paths. F-3 closed comp-card per-loop gain. MB3 closed the Dev path (`LiveInspiration` / `DevSetInspiration` / `DevSetCurrentInspiration`). MB4 closed the action-card path (`GigManager.AdjustInspiration` wrapper + `CardBase.SpendInspiration` / `GenerateInspiration` rerouting). One un-mirrored write remains: `TryPlayCompositionCard` step 8 (comp-card spend during build phase) — intentionally preserved, deferred to loop-game-flow milestone. The MB4-diag `[PD/Session]` Stats-tab readout makes any divergence directly visible. Potential follow-up: one-line note in `SSoT_Gig_Combat_Core.md` §4.2 to surface this implementation reality. See `SSoT_Dev_Mode.md` §13.4.
- **Musician picker in Gig Setup — RESOLVED (2026-05-04, M4.6-prep merged (1)/(4)).** Bidirectional band picker shipped. `pd.MusicianList` is now mutated by the picker before auto-assembly runs; `pd.SetBandRoster(picked)` handles roster identity. Validation: min 1 (warns), max 4 (blocks). ST-M42-6/7/8/9 are unblocked but not yet executed; they may run in parallel with M4.6 demo prep or post-demo. See §1 closure block.
- **Per-musician starter decks — RESOLVED (2026-05-02, M4.6-prep batch (2)).** `PersistentGameplayData.SetBandDeckFromMusicians` materializes the deck from each musician's `CardCatalog` (starter-flagged entries × `starterCopies`) plus an optional `GenericCardCatalogSO` from `GigSetupConfig.GenericStarterCatalog`. Toggle in Gig Setup scene (`useMusicianStartersToggle`, default ON) selects between auto-assembly and the legacy `BandDeckData` dropdown path. Provenance: per-musician contributions tracked, generic contributions not tracked. See §1 batch (2) closure block.
- **Gig Setup roster pickers — RESOLVED (2026-05-04, M4.6-prep merged (1)/(4)).** Audience picker shipped alongside the band picker. `GigEncounterSO.audienceMemberList` is now the *default* per-encounter audience composition; `GigSetupController` produces an `audienceOverride` when picker selection differs from baked, passes it to `GigEncounterSO.BuildRuntime(audienceOverride)`. Comparator is multiset-blind on baked duplicates (encounters with `[A, A, B]` preserve duplicates at runtime when user does not customize). Encounter-swap rebuilds picker with new defaults and warns if customization is discarded. See §1 closure block. Picker UI multiplicity (per-row count input) is a future concern tracked as M4.6-prep batch (6).
- **Card Editor inline effects-block UI on legacy catalogue alias — RESOLVED (2026-05-06, M4.6-prep cleanup, Patch 1).** `DrawStatusEffectPicker` now consumes `ALWTTTProjectRegistriesSO` and reads from both `StatusCatalogueMusicians` and `StatusCatalogueAudience`. UI is `DropdownButton + GenericMenu` with `Musicians/...` and `Audience/...` hierarchical paths. See §1 cleanup closure block.
- **All-starter-flagged catalog content (M4.6 demo blocker) — RESOLVED for demo roster (2026-05-06, M4.6-prep cleanup).** Robot and Gusano catalogs cleaned and authored to spec (Robot 4/4/5, Gusano 4/4/4; Generic 2/2/3 added). Cantante (7/7) and Conito (10/10) intentionally untouched and inert — they are not in the demo roster (M4 reduced to Robot C2 + Gusano Sibi). If post-demo roster expansion brings Cantante or Conito into play, their catalogs will need analogous cleanup. See §1 cleanup closure block. Verification snapshot: `inv4.json`.
- **M4.6F-1 Action card double-discard — RESOLVED (2026-05-07).** Bug was misdiagnosed at intake as a reshuffle/pile lifecycle defect. Root cause was upstream: `HandController.PlayCard:580-581` and `CardBase.Use`/`CardUseRoutine` both called `DeckManager.OnCardPlayed` for action cards, doubling the discard (HandPile.Remove + DiscardPile.Add fired twice per play, removing two distinct entries from HandPile because pile multiplicity tracks references). Composition cards were unaffected (they bypass `CardBase.Use`). Fix: gate the `HandController.PlayCard` call to `IsComposition` only. Latent SFX action card double-discard fixed by the same gate. Suspicion S-A (missing `SetPileTexts` at reshuffle) not the cause; suspicion S-B (duplicate `DeckManager`) ruled out. Smoke ST-DOUBLE-1/2/3 + ST-RESHUFFLE-1/2/3 all PASS. New invariant in `SSoT_Card_System.md §9.3` + `ssot_manifest.yaml`. See §1 closure block.
- **M4.6F-2 GigSettings unification — multi-SO refactor — RESOLVED (2026-05-07).** Settings dispersed across five homes consolidated to four SOs on the GigManager side (`GigFlowSettingsSO`, `MeterTuningSO`, `GigPresentationSO`, `GigDevSettingsSO`) plus renamed `GigSetupRosterSO` on the Gig Setup side. `GameplayData↔PersistentGameplayData` duplication remains by design (D4 deferral). Façade properties preserved on `GigManager`. Scene refs inline. Smoke ST-F2-S1..S8 ran with expected per-loop FAIL (S5 → F-3) and a pre-F-2 floating-text visibility caveat on S4. See §1 closure block.
- **M4.6F-3 Per-loop draw + per-loop inspiration hook + canonical AddCurrentInspiration — RESOLVED (2026-05-08).** New `GigFlowSettingsSO.DrawPerLoop` field. Per-loop draw + per-loop inspiration consumption fire from `GigManager.OnCompositionLoopFinished` (host-owned subscriber to `CompositionSession.LoopFinished`, respects deck-non-mutation invariant). `CompositionSession.AddCurrentInspiration(int) → int` promoted to canonical session-budget mutator (clamps to MaxInspiration, mirrors to PD, returns actual delta). Track-derived per-loop gain refactored through it. `+N` badge displays un-clamped track contribution. `JamRules.drawPerPart` flagged UNUSED (F-5 cleanup). Smoke ST-F3-S1..S7 + S4b PASS, S4c FAIL DEFERRED → MB3. Side-findings opened: Dev surface drift (MB3), session-start dual-siting (MB3), F-2 D4 follow-up (post-demo). See §1 closure block.
- **MB3 — Dev surface drift correction + session-start dual-siting fix** RESOLVED 2026-05-08. Code +25 / docs in §13.4 / §9.10. ST-MB3-1/2/4/8 PASS; ST-MB3-3 INVALID; ST-MB3-5/6/7 deferred to loop-game-flow.
- **MB4 — Action-card inspiration session routing (+ MB4-diag readout)** RESOLVED 2026-05-08. Code +37 / −2; +21 lines diag observability. ST-MB4-1..5 PASS. Closes user-reported critical action-card bug. F-followup queue exhausted post-MB4.
- **M4.6F-5 Composition next-loop pending workflow — ABSORBED into Phase B B1 (2026-05-09).** Original framing assumed per-loop pending was new functionality; user clarified during Phase B planning that per-loop card resolution **already works** in the current zone (cards in current → replace track → effect at next loop). The complex piece — *next zone* (planning a future part) — is not closed but **simplified out**: B1 disables next zone, current zone becomes full-screen, model collapses to per-loop-only. F-5 retroactively re-scoped; closure happens when B1 lands. See §1 Phase A close block and `Roadmap_ALWTTT.md §5`.
- **Phase B B1 — Loop model simplification + track persistence + UI rework — RESOLVED 2026-05-12.** See §1 closure block. All internal items (#7 stem cache, #0 next-zone disable, #1+#2 UI rework, #8 hand-discard configurability, #7.1 instrument pin, D-J draw-on-play mini-item) shipped. Smoke tests ST-B1-S1..S10 PASS or DEFERRED with reason. F-1, F-3, F-4 Stage A invariants clean. ~600-700 LoC ALWTTT-side across 9 files. The spike (D5) estimate of 300-400 LoC was conservative; the actual delta reflects D-E=α' (UI-stable hash), D-H pending visualization, and D-F=γ.1 instrument pin refinement, which were not in the original spike scope. F-5 invariant promoted to `SSoT_Runtime_CompositionSession_Integration §8` (D-K=α).
- **Action-card playability during composition loop — watch-item (opened 2026-05-12).** `_isSongPlaying` may not engage during active composition loop; the gate at `GigManager:1454-1462` may not enforce `AllowActionCardsDuringPerformance` as intended. Side-stepped today by `DiscardActionCardsOnPlay=true` (default). Worth verifying post-B1 if any "Action cards during performance" design returns to scope.
- **Phase B B2 — Polish layer (feedback + animation) — RESOLVED (2026-05-13).** Six items shipped monolithically per D3=A. Tooltip miniatures (#3), inspiration markers pulse (#4) + denied flash (D-Inspiration-Pool=A), expanded floating text (#5: composition events with diff-driven classifier D-FxChangeDetect=A + audience exclamations + Earworm multiplier-deferred), SongHype thresholds → venue SFX (#6), Robot beat-pop (#14), Worm stretch (#15), Worm instrument sub-animator (#16). See §1 closure block.
- **Phase B B2.5 — Polish refinements + cleanup — RESOLVED (2026-05-15).** 11 mandatory items shipped (correctness 1-3 + cleanup 7-11) plus item 16 (Tonality kept). 8 decisions locked (D-1 through D-8). Three preexisting bugs surfaced and 2 resolved within batch (D-7 ghost cards, D-8 macro-Vibe regression-and-fix); B3-cand-I (ParentActive=False warning during draws) captured for B3. Mid-batch D-5 fix introduced a regression (`_songHype` zeroed before `RunSongVibeResolution` could read it) caught by ST-B2.5-S6 diagnostic and surgically split via D-8. Hypothesis correction on the B2-era "Earworm tick lives in StatusEffectSO" assertion — code inspection showed only one tick site (the bespoke block in `GigManager`); fix was visual pacing, not relocation. Smoke tests S1, S2, S2b, S3, S4, S5, S6 all PASS. See §1 closure block.
  - **B2.5 deferred — content-dependent (3):** #4 per-venue smoke/fire VFX (art), #5 CompositionFxConfigSO default tuning (playtest), #6 animation feel tuning (playtest). Each blocked on its respective input; reopens when input lands.
  - **B2.5 deferred — design gaps (4):** #12 TempoScale diff in `SelectFxEntry`, #13 hasExplicit flags on PartEntry (mirror `hasExplicitRootNote`), #14 `PartActionKind.NoOp`, #15 `#if ALWTTT_DEV` gate on `DevAddSongHype`/`DevResetSongHype`. Carried to B3 candidate slate.
- **B3 candidate slate (accumulated during B2.5):**
  - **A. Mind Tap asset fix** — DONE in-batch (Earworm target switched from `AllAudienceCharacters` to `AudienceCharacter` per `Design_Audience_Status_v1.md §3.7`).
  - **B. AudienceMemberPosList reorder** — DONE in-batch (scene-level reorder to visual left-to-right; defensive sort by X-coordinate in `BuildAudience` not added — capturable if regression recurs).
  - **C. Effect-target-type authoring validation** (cross-effect target inconsistency warning in `CardEditorWindow`). Cost ~1h.
  - **D. CustomEditor for default Inspector showing effect labels** (mirror `BuildEffectLabel` from `CardEditorWindow` to surface in raw inspector). Cost ~1h. Low priority — Card Editor Window is the intended authoring path.
  - **E-lite. Blocked tooltip without icon.** Tooltip-only path to explain the "oscurito" sprite tint (Blocked → "immune to Vibe gains from this position"). Extend `AudienceCharacterCanvas.ShowContextual` or `OnPointerEnter` to surface contextual text. ~20-30 min. No status icon — Blocked is sprite-tint-only per M1.2 Decision E3.
  - **F. Real `AudienceCharacterBase.ResolveLoopEffect` impl** — currently returns 0 (placeholder; B2.5 #9 cleanup removed the `clamped = 2;` forced-clamp that masked this). Without F, audience exclamations never fire, and per-audience macro-Vibe impression modifier stays at 1.0 (so all audiences get the same `SongHype01 × MaxVibeFromSongHype` Vibe). Content-blocker for audience expressiveness.
  - **G. Filter draws during composition session** (`OnCompositionLoopFinished` line ~1389). Per **D-B3-DrawFilter=B confirmed in B2.5**: composition cards + Always-action cards allowed; non-Always action cards excluded from per-loop draws. ~3-5 lines using existing `DrawCardFiltered` method.
  - **H. Always-action card discard semantics in SongPerformance start.** If Always-action cards become content (not in current starter), the `DiscardHandBetweenTurns` discard at `SongPerformance` phase start kills them before player can use them. Conditional discard would be needed. Not urgent until Always content exists.
  - **I. ParentActive=False warning during draws.** Preexisting bug surfaced in B2.5 logs: `DrawCards` warns `"Drew '...' but card GameObject is inactive. (ParentActive=False)"` for some early draws when `DrawTransform` parent isn't activated yet. `DrawCardFiltered` doesn't fire the warning (different ordering). Note in `GigManager:403` ("Ensure the hand is enabled BEFORE any card objects are instantiated by DrawCards") points to the right fix area. Investigation needed.
- **Phase B B3 — Content + design** (opened 2026-05-09, depends on B1). Aditivo. Inspiration cost/gen balance pass across deck — cover 0/1/2/3 for cost and generated (#9); rhythm composition cards with `+/-BPM` and `2×BPM` effects (#10); chord progression cards with key Modulation effect (#11); 1 designed audience member with 3 distinct abilities (#12). Audience Member Wizard Editor (#13) deferred post-demo per D4=B. B3 candidate slate above (A-I + design gaps #12-15) folds in during scoping.
- **F-2 D4 follow-up — `MaxInspiration` + `MaxCardsOnHand` to `GigFlowSettingsSO`** (opened 2026-05-08 per F-3 user feedback). Both fields currently live on `GameplayData` (separate SO) and `PersistentGameplayData`. Inconsistent with `DefaultInitialGigInspiration` and `DefaultInspirationPerLoop` which were consolidated to `GigFlowSettingsSO` in F-2. Post-demo priority — not gate-blocking.
- **M4.6F-4 SongOrchestrator IndexOutOfRange — STAGE A RESOLVED 2026-05-08, Stage B parked-until-natural-repro.** Stage A delivered: production-quality try-catch defense around `generator.Orchestrator.GenerateSinglePart` in `MidiMusicManager.RenderSinglePart` (+58 lines net); production-quality D3-B within-part recursion guard in `CompositionSession.HandleLoopFinished` mirroring `AdvanceToNextPart`'s `if (secs <= 0f) End();` pattern (+8 lines net); `[F-4]`-tagged diagnostic logs at both boundary sides (entry-log on call, full per-track + arg + stack-trace dump on catch). ST-F4-S1/S6 PASS; ST-F4-S3 PASS-vacuous; ST-F4-S2 DEFERRED-non-repro — IOOR did not surface this session; defense correctly silent (no exception thrown); no arg dump captured to route Stage B; Stage B reopens automatically if `[F-4][MMM]` LogError fires during playtest. ST-F4-S5 BLOCKED-OUT-OF-SCOPE — Player build fails on package-internal `MidiGenPlayConfig.GetChordWriteFolder` and `MidiGenPlayConfig.GetProfileForTonality` references inside `D:\Projects\MidiGenPlay\MidiGenPlay\Runtime\CoreScripts\Services\PatternRepositoryResources.cs:87` and `\Composition\SongOrchestrator.cs:142,326`; F-4 edits do not reference these methods; ALWTTT-side editor compile clean; tracked as separate MidiGenPlay-project batch. Defense + D3-B stay permanent; `[F-4]` diag logs strip at M4.6 demo closure (retroactive D5-C path) if no natural recurrence happens. See §1 closure block.
- **M4.6F-5 Composition next-loop pending workflow — ABSORBED into Phase B B1 (2026-05-09).** Originally opened 2026-05-06 with Lectura A confirmed (per-loop pending granularity, card played during loop N → resolves at start of loop N+1). During Phase B planning the user clarified that this behavior **already works** in the current zone (cards in current → replace track → effect at next loop). The complex piece — *next zone* (planning a future part) — is being simplified out, not implemented. B1 disables next zone, agrandar current zone to full-screen, model collapses to per-loop-only. F-5 is retroactively re-scoped; the deferred D2-A "TS transform mechanism" path remains explicitly post-Phase-B (could land if persistence proves valuable in playtest). Original code-name `Part` keeps current meaning; future Song Parts Library (planning/Design_Song_Parts_Library_v0_1.md) remains a long-term intent without forced rename pressure. See §1 Phase A close block.
- **Card Editor — Generic write-side support deferred** (opened 2026-05-06). JSON Import / Create Card / Add Existing / Sync targeting `GenericCardCatalogSO`. Touches `CardAssetFactory.CreateCardKindParams` and `MusicianCatalogService` contracts (both currently typed to `MusicianCardCatalogData`). Future tooling QoL batch.
- **Asset path layout cosmetic** (surfaced 2026-05-06). 10 new starter cards live under `Assets/Resources/Data/Characters/Musicians/starter_*.asset` rather than under `Robot_Cards/` or `Gusano_Cards/` subfolders. Side-effect of `CardAssetFactory`'s default output path resolution. Not functional; reorganization at user's discretion.
- **Cantante / Conito catalogs out-of-spec but inert** (surfaced 2026-05-06). Both catalogs (Cantante 7/7 starter, Conito 10/10 starter) are unchanged from pre-cleanup state because they are not in the M4 demo roster. If a post-demo roster expansion brings either musician into the band, their catalogs need a cleanup pass analogous to Robot/Gusano. Tracked, not blocking.
- **`UnlockedByDefault` flag is editor-authoring-only (surfaced 2026-05-02, M4.6-prep batch (2) audit).** `CardAcquisitionFlags.UnlockedByDefault` has no runtime gameplay consumption today. Every reference is in editor code (Card Editor filter pills, validation warnings, JSON import validation, default value for new entries). Auto-assembly only consults `IsStarter`. The `UnlockedByDefault` + `unlockId` pair currently documents authorial intent for a future meta-progression / unlock system; no gameplay code reads them. Not a bug — flagged so future readers don't assume runtime enforcement that doesn't exist. Runtime consumption deferred to whenever a meta-progression batch lands.
- **Inventory viewer NRE on Draw/Discard/Hand pile open — RESOLVED (2026-05-02, M4.6-prep UI-fix-A).** `CardBase.SetCard` at `CardBase.cs:77` no longer throws because `CardUI.prefab`'s previously-unassigned `inspirationCostTextField` and `inspirationGenTextField` `[SerializeField]` refs are now wired. Asset-only fix on `CardUI.prefab`. `CardBase.SetCard` kept strict. See §1 UI-fix-A closure block.
- **`CardUI : CardBase {}` empty subclass — two-prefab arrangement (surfaced 2026-05-02, M4.6-prep UI-fix-A; appendix to batch (3) deferred 2026-05-03).** `CardUI` is a degenerate empty subclass of `CardBase` that exists solely to serve as a separate prefab GameObject's MonoBehaviour. The inventory canvas instantiates `CardUI.prefab` while gameplay instantiates the gameplay card prefab; both prefabs must independently wire every `[SerializeField]` field declared on `CardBase`. This is the recurrence vector for the UI-fix-A NRE class — any future TMP/Image field added to `CardBase` must be wired on both prefabs or the inventory side will NRE. Candidate cleanups (logged, not scheduled): (α) collapse to a single prefab with view-only mode driven by `SetCard(def, isPlayable=false)` — lowest drift risk; (β) make `CardUI.prefab` a Prefab Variant of the gameplay prefab so `CardBase` field additions inherit automatically — lower-risk migration than (α). Candidate appendix to batch (3) — "Validate `CardBase` prefab variants" Card Editor action that reflects over `[SerializeField]` fields and reports unwired refs at authoring time — was considered at batch (3) open and **explicitly deferred** (D3); logged in `SSoT_Editor_Authoring_Tools.md §14.5` as a candidate authoring-tool addition for a future QoL pass.
- **Inventory scrollbar appears even with near-empty piles — paper cut (surfaced 2026-05-02, M4.6-prep UI-fix-B; ST-SCR-2 FAIL ACCEPTED).** `CardSpawnRoot` carries a fixed `LayoutElement.preferredHeight = 2050` so `Content` always reports overflow to `ScrollRect`, regardless of how many cards are actually displayed. Cosmetic only — does not affect functionality. Follow-up: replace the fixed value with a runtime computation in `InventoryCanvas.SetCards` based on active card count × grid params (`grid.cellSize.y`, `grid.spacing.y`, `grid.padding.top + grid.padding.bottom`, columns from `grid.constraintCount`). ~10 lines, computes `LayoutElement.preferredHeight` after population. Not blocking M4.6 demo.
- **FilterPanel scrolls with content (decision D-A deferred from M4.6-prep UI-fix-B, 2026-05-02).** `FilterPanel` lives inside `Content` under `VerticalLayoutGroup`, so it scrolls along with `CardSpawnRoot`/`SongSpawnRoot`. FilterPanel currently only contains TitleText (no functional filter chips), so scroll-with-content is harmless. Revisit when filters become functional: move FilterPanel out of `Content` and make it a sibling of `Scroll View` under `Midground` for sticky behavior.
- **Card Editor per-row starter UX — RESOLVED (2026-05-03, M4.6-prep batch (3)).** Batch (3.A) ships per-row `Starter` checkbox + `Copies` IntField columns on the catalog entry list, both via `SerializedObject` for Undo + dirty propagation parity with the right-side inspector. Batch (3.B) ships `CardInventoryWindow` (read-only viewer with Print + Export per view). Batch (3.C) ships toolbar Print buttons on Card Editor and Deck Editor. Smoke tests ST-AT3-1..8 all PASS. ST-AT3-8 dogfood acceptance confirmed the cleanup workflow is materially faster than the right-side inspector path. See §1 batch (3) closure block.
- **Pending Effects system (post-MVP, scheduled first).** Song-scoped accumulator layer where cards add to a pending bucket during a song and resolve at song end. First user: deferred Earworm. Mid-song multiplier cards become a content axis. Generalizes to pending Vibe / Stress / Flow / Cohesion. Does not affect M4.6 starter deck — Mind Tap and any other Earworm-applying starter card stay immediate-effect. Planning doc: `planning/Design_Pending_Effects_v1.md`. Implementation slot: first post-MVP gameplay batch immediately following M4.6 demo closure.

- **Tempo-coupled card identity (post-MVP, long-term, no implementation slot).** Design direction making tempo a gameplay input — cards prefer / require / shift tempo, producing fast-favoring vs slow-favoring deck identities ("metal" / "fast jazz" / etc.). Downstream of M4.6 closure, Pending Effects landing, and meter-stack playtest. No runtime commitment. Influences starter deck and per-musician catalog design now via flavor / naming / archetype lean — see `Design_Starter_Deck_v1.md` for tempo-lean notes per musician. Planning doc: `planning/Design_Tempo_Identity_v1.md`.

- **B3 audience pool authoring (opened 2026-05-15, expanded from #12).** Two archetypes shipped together: Cool Dude (3 abilities: Move One Step parameterized, Heckle composed `ApplyStatusEffect(exposed) + AddStress`, Indifference self-buff) + Kid (2 abilities: existing band-wide Stress, new `Egged On` buff on Cool Dude's outgoing Stress). New audience-side status `Indifference` requires implementing the deferred `ApplyIncomingVibe` helper on `AudienceCharacterStats` (mirror of M4.1 `ApplyIncomingStressWithComposure` pattern). Per D-DCP-6=A, Indifference blocks ALL incoming Vibe (song-end conversion + Earworm tick + direct ModifyVibe + future sources). Sound-design priority: Singing Field gains a per-card `InstrumentEffect` SO authored for Sibi (β path per D-DCP-5). Demo encounter (2×Kid + 1×CoolDude) authored as `GigEncounterSO`. SSoT edits required at closure: `SSoT_Status_Effects.md` new §5.8 Indifference; `SSoT_Audience_and_Reactions.md` §10 `ApplyIncomingVibe` canonical path + audience status pattern. See `Roadmap_ALWTTT.md §5.3`.

- **§5.3.5 Demo cut prep — RESOLVED (2026-05-18).** Demo build entry zero-clicks-from-launch-to-Gig via new two-scene Main Menu → Gig flow. Structural refactor extracts `GigLauncher` as the single non-Gig→Gig scene transition entry point; `MainMenuController` + `DemoLaunchConfigSO` enable auto-launch branching. SFX→FlatVibe mechanic shipped on stage crossings (per-audience via `ApplyIncomingVibe`, Indifference still blocks). Action-card mid-performance gate relaxed. `UIManager.Fade(false)` latent loop-termination bug fixed. F9 (B3-demo-polish ad-hoc precursor) replaced wholesale by the new architecture. ST-DCP-S5 win-rate validation deferred to §5.4 per DC-Close-S5=(c). See §1 closure block.

- **§5.3.5 ST-DCP-S5 win-rate validation — DEFERRED to §5.4 readiness review** (opened 2026-05-18). 8-10 playthrough win-rate validation against 60-80% target. Originally scoped to §5.3.5 closure; folded into §5.4's full clean-run smoke pass (R1 sub-item) per DC-Close-S5=(c). Three resolution paths still available at §5.4 close: (a) PASS at win-rate W% confirms §5.3.5 tuning baseline; (b) FAIL opens §5.3.5b tuning-only batch to adjust `initialGigInspiration` / `inspirationPerLoop` / `sfxBonusVibeStage{1,2,3}` on SO assets; (c) further deferral to post-demo if §5.4 scope grows.

- **§5.4 Demo readiness review (queued, opens after §5.3.5 closure applied).** Candidate sub-items per §3.2: full clean-run smoke pass (cold launch → win → exit → relaunch → loss → exit, absorbs ST-DCP-S5); polish-deferred items from prior batches (audience hover outline render, Kid Tantrum AnimatorTrigger consumption, Indifference + Hyped icon sprites); build pipeline (standalone builds produced + launched, no editor-only path leaks; MidiGenPlay Player-build errors may surface as gating, see post-demo follow-up); asset audit. Closes the demo cut tag; gates ladder mode opening.

- **Ladder mode (post-demo architectural batch, foundation laid 2026-05-18).** Foundation for multi-encounter ladder mode laid by §5.3.5's `GigLauncher` extraction. Will introduce: `LadderRunner` (`DontDestroyOnLoad`, encounter-queue holder, gig-won event subscriber); `EncounterLaunchConfigSO` family (DemoLaunchConfigSO sibling, per-encounter, designed for queuing); inter-encounter band carry-over via `bandRoster: null` to `GigLauncher.Launch`. Enables tuning of multi-gig mechanics: Cohesion, card rewards, deck modifications across encounters. Opens after §5.4 closes.

- **Pitch deck refresh (opened 2026-05-15, non-governance stream).** August 2025 pitch deck (`GoblinzStudio.pdf`) substantially obsolete (timeline window consumed, scope framing inflates aspirational features, team composition outdated, no demo screenshots). v2 batch scheduled post-§5.4 per PD-5=B; informal sub-batch A (audit + outline + draft text) can begin pre-§5.4 per PD-1=C interpretation α. Target deliverable PD-3=C (deck + video + playable build); minimum PD-3=B (deck + video). MidiGenPlay Player-build error follow-up may block PD-3=C build packaging — surfaceable risk. PD-4 commercial info captured: BCS Studios + Abstract Digital + CoverSolutions + Bamer29 partners; Claudio + Matías core team; EA 2027 / v1.0 2028 timeline; ~€200k ask; Cristian's Pretty Soon Games meeting (Digital Dragons 2026) immediate test case; Goblinz Publishing warm contact. See `Roadmap_ALWTTT.md §6`.

- **Sound design priority — standing design directive (declared 2026-05-15).** ALWTTT is a game about music bands; sound design is a maximum design priority that overrides convenience-of-authoring when in conflict. Operational consequences: per-musician instrument identity is preferred over reusing generic SOs (D-DCP-5=β path was chosen for Sibi's voice over reusing existing `Bass/Guitar/Synth`); future audience archetypes, statuses, and venues should consider audio identity at design time, not as a polish phase afterthought; PD-3=C build packaging is preferred over PD-3=B because the demo's core appeal is audible. Captured in new planning doc `planning/Design_Project_Directives_v0_1.md`. Should be considered for promotion into project-level instructions.

- **MGP-ALWTTT-MOD-DIR-1 (cross-project, non-blocking).** Directional modulation hint for `ChordTrackComposer` filed in MidiGenPlay project tracker. ALWTTT-side `ModulationEffect_KeyLift_Degree5.asset` will adopt the new `octaveHint` field when the package ships it. Current demo accepts non-directional voicing per D-MOD-DIR=A.

### Residual risks
- **GigManager flag lifecycle surveillance:** `_isSongPlaying` was not observed to drift but a symmetric single-use-per-gig pattern may exist elsewhere. Low-priority audit recommended.
- **Status icon animation pause behavior:** icon animations use `Time.deltaTime`. If a future pause feature sets `Time.timeScale = 0`, icon popups freeze. Switch to `Time.unscaledDeltaTime` if pause-transparent animations become desired.
- **Composition face minimal display:** the shortened face only shows role/part + modifier count. M1.10 detail modal now provides full inspection. Cosmetic items remain: "COMPOSITION" word-break on narrow panels, panel overflow on cards with many modifiers. Neither blocks gameplay testing.
- **M4 roster reduction (2-musician starter) intentionally narrows MVP demo (2026-04-21 design decision):** starter band is C2 + Sibi only. Conito and Ziggy deferred to post-MVP roster expansion. Demo will show a band that is smaller than the final design; this is deliberate and scoped to reduce art and tuning cost. Documented in `planning/Design_Starter_Deck_v1.md`.

- **`ApplyIncomingVibe` deferred helper:** the audience-side equivalent of `ApplyIncomingStressWithComposure`. Not implemented in MVP because Earworm (the only audience status in the starter) does not modify incoming Vibe; it generates Vibe on tick. Hook point identified and documented in `planning/Design_Audience_Status_v1.md` for when Captivated lands with Ziggy.

---

## 5. Docs that must be edited next

After the next meaningful technical change, edit:
- the primary affected SSoT
- `CURRENT_STATE.md` if the active operational slice changed
- `changelog-ssot.md` if meaning/authority changed
- `coverage-matrix.md` only if the primary home changed

No pending M1.5 doc edits. All P3 phases closed. Open-micro-batches list empty after MB1+MB2 closure. M1.9 is presentation-only — no subsystem SSoT changes required. M1.5 Phase 3.3b doc edits applied at closure (`SSoT_Dev_Mode.md` §3/§6/§9.7/§15, `CURRENT_STATE.md`, `Roadmap_ALWTTT.md`, `changelog-ssot.md`). MB1+MB2 doc edits applied at joint closure (`SSoT_Dev_Mode.md` §9.5 correction + §9.8 + §9.9 + §15.4 resolution, `CURRENT_STATE.md` §1 P3.2 amendment + new closure block + §3 next-up, `Roadmap_ALWTTT.md` §1.5 open-micro-batches cleared + header date bumped, `changelog-ssot.md` 2026-04-24 joint-closure entry with ST-P32-4/-5 honesty correction).

Pending semantic doc edits from the M4 design pass (held until their respective M4 batches land in code):
- `SSoT_Gig_Combat_Core.md` §5.4, §6.2 — unified Stress path post-M4.1 (both card path and audience action path through `ApplyIncomingStressWithComposure`).
- `SSoT_Status_Effects.md` — new §5.7 `Earworm` with full spec. Post-M4.3.
- `SSoT_Audience_and_Reactions.md` §8, §10 — remove "audience statuses optional for MVP"; add Earworm as the first active audience-side status. Post-M4.3.
- `SSoT_Card_Authoring_Contracts.md` §5.7 + new §5.10 + §7.1 — applied 2026-04-29 (M4.4 closure). `starterCopies` clarified as authoring-only at M4.4 with M4.6 runtime-consumption note; new §5.10 covers deck-level multiplicity contract; §7.1 stage invariants note the per-entry `count` on `StagedCardEntry`.
- `SSoT_Card_System.md` new §13 — applied 2026-04-29 (M4.4 closure). Deck multiplicity model documented (multiset shape, runtime expansion, pile-lifecycle invariance, lazy legacy migration). §12 boundaries list updated. M4.5 cross-reference paragraph appended 2026-04-30.
- `SSoT_Runtime_Flow.md` §4.2 + §8 invariant 9 — applied 2026-04-30 (M4.5 closure). New §4.2 "Bidirectional guaranteed draws" documents subtractive rule, three-phase algorithm, hook collapse, tie-break, observability, exhaustion case. New invariant 9 in §8.
- `ssot_manifest.yaml` — applied 2026-04-29 (M4.4 closure). New invariants on `SSoT_Card_System.md` (deck is multiset; runtime expands to flat references) and `SSoT_Card_Authoring_Contracts.md` (JSON deck entries support `count`; duplicate `cardId` combines additively). Applied 2026-04-30 (M4.5 closure). New invariant on `SSoT_Runtime_Flow.md` (subtractive guaranteed-draw rule). M4.2 invariants update remains pending.
- `SSoT_Card_Authoring_Contracts.md` §5.9 — applied 2026-05-01 (M4.6-prep-A closure). Stale "parallel `DeckCardCreationService` path still consults a single catalogue field" footnote removed; the section now describes a single, unified MB2-aware editor toolchain. `CURRENT_STATE.md` §1 + §3 + §4 + §5 + `changelog-ssot.md` updated; `ssot_manifest.yaml`, `coverage-matrix.md`, `Roadmap_ALWTTT.md`, `SSoT_Editor_Authoring_Tools.md` intentionally unchanged.
- M4.6-prep batch (2) closure (applied 2026-05-02): `CURRENT_STATE.md` §1 closure block + §3 M4.6 dependency line update + §4 open-item closures and additions (Draw Pile NRE, batch (3) queue, all-starter-flagged catalog blocker, `UnlockedByDefault` editor-only note) + §5 (this line); `Roadmap_ALWTTT.md` §4.4 line 371 + §4.6 line 412 marked shipped, two new Future Milestones added (Authoring tooling QoL = batch (3); Inventory viewer prefab fix); `SSoT_Card_Authoring_Contracts.md` new §5.11 (per-musician starter deck auto-assembly contract); `ssot_manifest.yaml` Card_Authoring_Contracts entry gains one invariant on auto-assembly; `changelog-ssot.md` new top entry. `coverage-matrix.md`, `SSoT_Editor_Authoring_Tools.md`, `SSoT_INDEX.md`, `SSoT_Card_System.md` intentionally unchanged (no new editor tool, no new subsystem, no authority change, no runtime pile-lifecycle change).
- M4.6-prep UI-fix-A + UI-fix-B joint closure (applied 2026-05-02): `CURRENT_STATE.md` §1 two new closure blocks (UI-fix-A inventory NRE; UI-fix-B inventory scrollbar) + §4 open-items: inventory NRE bullet flipped to RESOLVED with closure pointer, three new park-lot bullets added (`CardUI : CardBase` empty-subclass two-prefab vector with cleanup options α/β logged; inventory-scrollbar paper cut with dynamic-height follow-up; FilterPanel-scrolls-with-content D-A deferral); `Roadmap_ALWTTT.md` Future Milestones: `Inventory viewer prefab fix (UI-fix batch)` entry retitled to combined `Inventory viewer fixes (UI-fix-A + UI-fix-B)` and marked shipped 2026-05-02; `changelog-ssot.md` new combined top entry covering both batches with ST-INV-1..6 PASS + ST-SCR-1/3/4/6/7 PASS / ST-SCR-2 FAIL ACCEPTED / ST-SCR-5 DEFERRED. `ssot_manifest.yaml`, `coverage-matrix.md`, `SSoT_INDEX.md`, all systems SSoTs intentionally unchanged (no contract, authority, or governance change — UI-asset wiring + a localized ScrollRect helper edit on `InventoryCanvas.cs`).
- M4.6-prep batch (3) closure (applied 2026-05-03): `SSoT_Editor_Authoring_Tools.md` §3 inventory row added (Card Inventory), §4.6 (per-row Starter / Copies columns) + §4.7 (Card Editor Print button) + §5.7 (Deck Editor Print button) added, new §8 `CardInventoryWindow` full section inserted, §9–§15 renumbered, §13 file location summary updated, §14.5 prefab-variant validator candidate logged. `CURRENT_STATE.md` §1 new closure block (M4.6-prep batch (3) — Authoring tooling QoL — complete) inserted after the UI-fix-B block; §1 Editor authoring tools list updated; §3 line 1 M4.6 entry updated to note batch (3) closure and the structurally-tractable / content-status-undetermined nature of the all-starter-flagged blocker; §4 open-items: "Card Editor per-row starter UX" bullet flipped from queued → RESOLVED with closure pointer, "all-starter-flagged catalog content" bullet rewritten to distinguish *tooling resolved* from *content cleanup pending*; "`CardUI : CardBase {}` empty subclass" bullet updated to record the D3 deferral of the prefab-variant validator appendix; §5 (this line). `Roadmap_ALWTTT.md` Future Milestones: `Authoring tooling QoL (batch (3))` entry marked ✅ (closed 2026-05-03) with closure notes and smoke-test summary; header `Last updated` line bumped to 2026-05-03. `changelog-ssot.md` new top entry. `ssot_manifest.yaml`, `coverage-matrix.md`, `SSoT_INDEX.md`, all systems SSoTs intentionally unchanged (no new authority, no new contract, no new subsystem — operational tooling only).
- M4.6-prep cleanup closure (applied 2026-05-07): `CURRENT_STATE.md` §1 new closure block + §3 M4.6 dependency line update + §4 two existing items flipped to RESOLVED + 8 new bullets added (5 followup batches + Generic write-side defer + asset path cosmetic + Cantante/Conito out-of-spec) + §5 (this line); `Roadmap_ALWTTT.md` Last-updated line bumped to 2026-05-06, new "M4.6-followup mini-milestone" subsection inserted after §4.6, M4.6 closure context noted in DoD; `changelog-ssot.md` new top entry covering cleanup, Patch 1 and Patch 2 shipping, and the Patch 2 latent-bug verification; `SSoT_Editor_Authoring_Tools.md` new §4.9 "Catalog Source toggle and classified status dropdown" appended in §4 (renumbered note: existing §4.8 Registries surface remains §4.8; new section is §4.9). `ssot_manifest.yaml`, `coverage-matrix.md`, `SSoT_INDEX.md`, `SSoT_CONTRACTS.md` intentionally unchanged (no contract, authority, or invariant change — operational tooling + content authoring only).
- M4.6F-1 closure (applied 2026-05-07): `CURRENT_STATE.md` §1 new closure block + §3 M4.6 dependency line update + §4 M4.6F-1 bullet flipped to RESOLVED + §5 (this line); `Roadmap_ALWTTT.md` Last-updated bumped to 2026-05-07, F-1 entry in §4.6-followup marked ✅; `changelog-ssot.md` new top entry; `SSoT_Card_System.md` new §9.3 "OnCardPlayed pile transition contract" appended after §9.2; `ssot_manifest.yaml` new hard_invariant on Card_System ("each successful card play fires exactly one OnCardPlayed call; call site varies by card type"). `coverage-matrix.md`, `SSoT_INDEX.md`, `SSoT_CONTRACTS.md` intentionally unchanged (no authority, governance, or contract change beyond the new invariant under existing Card_System SSoT). Files instrumented during diagnostic and reverted at closure: `DeckManager.cs`, `CardBase.cs`, `InventoryCanvas.cs`. The actual fix is on `HandController.cs`.
- M4.6F-2 closure (applied 2026-05-07): `CURRENT_STATE.md` §1 new closure block + §3 M4.6 dependency line update (F-3..F-5) + §4 M4.6F-2 bullet flipped to RESOLVED + §5 (this line); `Roadmap_ALWTTT.md` Last-updated bumped to 2026-05-07, F-2 entry in §4.6-followup marked ✅; `changelog-ssot.md` new top entry; `SSoT_Gig_Encounter.md` §7.2 `setupConfig.X` references renamed + new §7.5 "Gig Setup data sources (M4.6F-2)" appended; `SSoT_Gig_Combat_Core.md` §6.3 step 4 stress-reset locality clarified + new §12 "Configuration architecture (M4.6F-2)" appended; `SSoT_Scoring_and_Meters.md` §3.3 + §7.1 + §9 amendments noting `MeterTuningSO` as the SO host; `coverage-matrix.md` two new rows for "Gig setup roster" (→ Encounter) and "Gig flow settings + setup defaults + meter tuning + presentation + dev settings" (→ Combat_Core); `ssot_manifest.yaml` Combat_Core/Encounter/Scoring_and_Meters governs lists updated, new hard_invariant on Combat_Core (scene-refs/SO split), new `known_drift_signals` F6 (deliberate dispersion documented); `SSoT_INDEX.md` Systems table footnote added for F-2 navigation. `SSoT_Card_System.md`, `SSoT_Status_Effects.md`, `SSoT_Audience_and_Reactions.md`, `SSoT_Runtime_Flow.md`, `SSoT_Runtime_CompositionSession_Integration.md`, `SSoT_ALWTTT_MidiGenPlay_Boundary.md`, `SSoT_CONTRACTS.md` intentionally unchanged. Files DELETED at closure: `GigSetupConfigData.cs`. Asset renamed in Unity: `GigSetupConfig.asset` → `GigSetupRoster.asset`.
- M4.6F-3 closure (applied 2026-05-08): `CURRENT_STATE.md` §1 new closure block + §3 M4.6 dependency line update (now F-4..F-5 + MB3) + §4 open-item closures and additions (F-3 RESOLVED; new MB3 bullet; new F-2 D4 follow-up bullet) + §5 (this line); `Roadmap_ALWTTT.md` Last-updated bumped, F-3 entry in §4.6-followup marked ✅, new MB3 entry inserted before F-4; `changelog-ssot.md` new top entry; `SSoT_Runtime_CompositionSession_Integration.md` §3.1 amendment + new §8 invariants 7 and 8; `SSoT_Gig_Combat_Core.md` §5.1 per-loop wiring note appended; `SSoT_Dev_Mode.md` §13.4 closing paragraph + §13.5 ST-P32-1..3 honesty flag; `ssot_manifest.yaml` Runtime_CompositionSession_Integration entry gains two hard_invariants. `coverage-matrix.md`, `SSoT_INDEX.md`, `SSoT_CONTRACTS.md`, `SSoT_Card_System.md`, `SSoT_Status_Effects.md`, `SSoT_Audience_and_Reactions.md`, `SSoT_Runtime_Flow.md`, `SSoT_ALWTTT_MidiGenPlay_Boundary.md` intentionally unchanged (no card, status, audience, runtime-flow, integration-boundary, contract, or coverage-routing change).
- 2026-05-08 — MB3 closed: Dev-path inspiration routing + carry-over reset semantic. Code: CompositionSession.cs (D6 log + ResolveSessionStartInspiration helper + 3 reset-site replacements), GigManager.cs (LiveInspiration getter + upgraded DevSetInspiration with session routing), DevStatsTab.cs (slider read switched to LiveInspiration). ST-MB3 (8 tests).
- 2026-05-08 — MB4 closed: Action-card spend session routing. Code: GigManager.cs (AdjustInspiration public wrapper + IsCompositionSessionActive Dev getter), CardBase.cs (SpendInspiration / GenerateInspiration replacements), DevStatsTab.cs (raw PD/Session readout). ST-MB4 (5 tests). Behavior tightening: clamp-at-0 on action-card spend.
- 2026-05-08 — M4.6F-4 Stage A closed: SongOrchestrator IOOR defense + diagnostic + D3-B recursion guard. Code: MidiMusicManager.cs (entry log + try-catch around `generator.Orchestrator.GenerateSinglePart` + catch-dump LogError + return failure tuple, +58 lines), CompositionSession.cs (entry log in PlaySinglePartLoop +19 lines, D3-B guard in HandleLoopFinished +8 lines). ST-F4-S1/S6 PASS, S3 PASS-vacuous, S2 DEFERRED-non-repro, S4 N/A, S5 BLOCKED-OUT-OF-SCOPE. Stage B parked-until-natural-repro. Out-of-scope: MidiGenPlay-side `MidiGenPlayConfig.GetChordWriteFolder` / `GetProfileForTonality` build errors; separate batch.
- 2026-05-09 — Phase A formally closed + Phase B opened. Doc-only governance batch (β path: separate from B1 code work). `CURRENT_STATE.md` §1 new Phase A close block prepended; §2 active-work paragraph rewritten from M4 framing to Phase B framing; §3 What is next rewritten with B1/B2/B3 + post-demo follow-ups; §4 F-5 bullet flipped to ABSORBED; §4 three new Phase B B1/B2/B3 bullets added; §5 (this line). `Roadmap_ALWTTT.md` Last-updated bumped; §4.6-followup item 5 (M4.6F-5) marked ABSORBED into Phase B B1; new §5 Phase B section inserted (full B1/B2/B3 outlines, scope, and DoD). `changelog-ssot.md` new top entry. Decisions locked: D1=C, D2=B, D3=A, D4=B, D5=run-complete, D6=A, D7=B, α/β=β. Spike findings recorded. No SSoT promotion or retirement, no authority change, no `coverage-matrix.md` change, no `ssot_manifest.yaml` change, no systems SSoT touched.
- §5.3.5 closure (applied 2026-05-18): `CURRENT_STATE.md` §1 new closure block (Phase B — §5.3.5 Demo cut prep — complete 2026-05-18) inserted after B3-demo-polish subsection + §3 active-work rotation to §5.4 (§5.3.5 marked closed, §5.4 promoted to next active batch, ladder mode entered parked-follow-ups list) + §4 Demo cut prep bullet flipped to RESOLVED + three new open-item bullets (§5.3.5 ST-DCP-S5 win-rate validation DEFERRED to §5.4 per DC-Close-S5=(c); §5.4 Demo readiness review queued; Ladder mode post-demo architectural batch with GigLauncher foundation noted) + §5 (this line); `Roadmap_ALWTTT.md` Last-updated bumped to 2026-05-18, §5.3.5 marked ✅, ladder-mode foreshadowed in Future Milestones; `changelog-ssot.md` new top entry; `SSoT_Gig_Combat_Core.md` new §13 (Launch contract) + §5.2 SFX→FlatVibe paragraph appended; `SSoT_Gig_Encounter.md` §7.5 restructured (renamed "Gig launch data sources", three-source enumeration; SO bullets and `ApplyRunConfig` paragraph preserved as the manual-path authority breakdown); `SSoT_Card_System.md` short note on performance-time playability gate appended to §9.1; `ssot_manifest.yaml` Gig_Combat_Core entry gains two hard_invariants (GigLauncher single entry point; SFX→FlatVibe routing). `coverage-matrix.md`, `SSoT_INDEX.md`, `SSoT_CONTRACTS.md`, `SSoT_Status_Effects.md`, `SSoT_Audience_and_Reactions.md`, `SSoT_Runtime_Flow.md`, `SSoT_Runtime_CompositionSession_Integration.md`, `SSoT_ALWTTT_MidiGenPlay_Boundary.md` intentionally unchanged (no contract, authority, governance, status, audience, runtime-flow, integration-boundary, or coverage-routing change). Planning `Design_Demo_Cut_v1.md` updated: §1.2 entry-path diagram replaced with two-scene flow; §3.2 replaced wholesale (Launch architecture — GigLauncher; old Auto-start path content gone, F9 surfaces noted as removed); new §3.3 (Action-card mid-performance unblock). Existing §3.1 (SFX → FlatVibe bonus) unchanged; bundle's §3.3 SFX-renumbering treated as redundant since §3.1 already holds that content. §2 coverage matrix preamble gains an ST-DCP-S5 deferral note explaining cells are gated on §5.4 absorption; cells preserved as `[verify]` / `✓` markers. Top-of-doc status banner updated to "§5.3.5 closed 2026-05-18. Frozen pending §5.4 absorption of ST-DCP-S5".
- §5.3.5 closure §3 ordering correction + Design_Demo_Cut §2 coverage matrix correction (applied 2026-05-18, same day): mid-application audit surfaced that §3 of the original §5.3.5 closure patch promoted §5.4 to next-active, but the Roadmap §5.4 entry positions §5.4 as "Post-B3", and only 2 of 7 B3 sub-batches had closed (B3-content-audience + B3-demo-polish). D-B3-Remainder=A locked: close B3 properly before §5.4 opens. `CURRENT_STATE.md` §3 re-ordered to put 5 B3 remainder sub-batches (B3-content-sibi, B3-content-cards, B3-slate, B3-balance, B3-validation) ahead of §5.4 with explicit material-for-S5 flagging on B3-slate F and B3-balance; `Design_Demo_Cut_v1.md` §2 PartEffect families table updated to flip aspirational `✓` markers on Tempo / Tonality to `pending-B3` (matching Instrument's existing `✗`), with header note clarifying current-vs-intended state. No code change, no SSoT change, no roadmap change (Roadmap §5.4 entry already correctly says "Post-B3"; the inconsistency was on the CURRENT_STATE side only). Decisions locked at applied closure: DC-Close-S5=(c) DEFERRED; all 14 §5.3.5 batch-open decisions locked at code-side closure (DC-1=C, DC-2=Custom, DC-3=Custom, DC-4=B, DC-5=B, D-DCP-1=A, D-DCP-2=A, D-DCP-6=A, DC-Scene-1=existing, DC-Scene-2=A, DC-F9-fate=A, DC-SFX-Route=A, D-FAST-1=C, D-PLACE-1=A).
- B3-content-sibi + B3-content-sibi-followup doc-closure (applied 2026-05-20): `CURRENT_STATE.md` §1 new closure block (2026-05-20 — B3-content-sibi closed + B3-content-sibi-followup closed) inserted between the §5.3.5 close block and the Phase A close block + §3 sub-batch rotation (B3-content-sibi ✅ + B3-content-sibi-followup ✅ + B3-content-cards promoted to next-active + items 4-6 renumbered) + §5 (this line; backfilled in planning-reorg-2026-05-20 since the original doc-closure batch did not add a §5 entry). `Roadmap_ALWTTT.md` §5.3 item #11.5 marked ✅ with two-pass summary (carrier-level voice via `InstrumentEffect_Sibi_Voice` → `Fantasia`; per-musician SO whitelist activated in `InstrumentRules.GetPermittedMelodic`). `Design_Demo_Cut_v1.md` coverage matrix Instrument row flipped from `pending-B3` to `✅ shipped` with Sibi pool details (lead = [Fantasia, 5th Saw Wave, Soundtrack]; backing pool currently empty per content-side deferral). `changelog-ssot.md` new top entry. `MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` §4.5 additive paragraph on per-musician SO whitelist precedence. `Design_Project_Directives_v0_1.md` D1 status block updated from "Promotion candidate" to "Promoted 2026-05-20" (threshold 2/2-3 satisfied at lower bound; user decision A; pending manual addition to project-level instructions panel). Decisions locked: D-Sibi-3 retroactive correction (new asset is `InstrumentEffect` carrier, not new `MIDIInstrumentSO`); D-Sibi-Pool=A (pool on `MusicianCharacterData.Profile`); D-Sibi-Pool-Scope=γ (both lead and backing whitelists, independent per role — capability shipped, lead exercised, backing authoring deferred); empty-list discipline per role (no cross-role fallback); ST-Pool-7 disposition=A (redundant — cross-role isolation property proven by ST-Pool-3 + ST-Pool-4); MidiGenPlay boundary preserved; D1 promoted. Smoke tests ST-B3CS-1..6 + ST-Pool-1..6 all PASS; ST-Pool-7 reclassified as redundant. `SSoT_INDEX.md`, `coverage-matrix.md`, `ssot_manifest.yaml`, `SSoT_CONTRACTS.md`, all systems SSoTs, all runtime SSoTs, `SSoT_ALWTTT_MidiGenPlay_Boundary.md` intentionally unchanged (no contract, authority, governance, status, audience, runtime-flow, integration-boundary, or coverage-routing change).
- planning-reorg-2026-05-20 (applied 2026-05-20, same day): structural reorganization of `planning/` and adjacent folders. Doc-only. Six decisions D-Plan-1..6 + D-Plan-7 (soundfont report move) + D-Plan-8 (full F1 closure + §5 backfill) all resolved. File moves: `planning/Design_Demo_Cut_v1.md` → `planning/active/`; `planning/active/Design_Song_Parts_Library_v0_1.md` → `planning/`; `planning/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` → `integrations/midigenplay/`; `planning/active/M1_5_Dev_Mode_Sub_Roadmap.md` → `planning/archive/` (with disposition paragraph added — Phases 1-3 closed via M1 closure 2026-04-26; Phases 4 + 5 effectively dropped); `planning/music/ALWTTT_MidiGenPlay_Soundfont_Emulation_Report_2026-03-24.md` → MidiGenPlay project (`planning/music/` folder removed from ALWTTT tree). README rewrites: `planning/README.md` and `planning/active/README.md` both rewritten to formalize the root-vs-active split per kind (root = standing/long-term pillars; active = near-term batched work); `integrations/midigenplay/README.md` rewritten with new "Docs in this folder" section listing all four files. Banner updates: `planning/active/Design_Audience_Status_v1.md` top-of-doc status banner updated (§3 + §5 superseded; §4 sole remaining active intent); §5 ⚠️ Superseded banner added mirroring §3 pattern; §4 header gains bidirectional cross-ref to Roadmap → Future Milestones → Roster Expansion. Roadmap fixes: §4.3 line about ApplyIncomingVibe deferral struck through and replaced with 2026-05-18 update (helper shipped in §5.3.5); §5.3 Demo_Cut paths repointed to `planning/active/`; `Last updated` bumped to 2026-05-20. SSoT_INDEX restructured: Active planning docs table rewritten to enumerate all 8 active planning docs by location plus the moved MidiGenPlay_Expressive entry at its new integrations/midigenplay/ home; duplicate MidiGenPlay_Expressive row deduplicated; M1_5_Dev_Mode_Sub_Roadmap row added to archived planning table; stale MidiMusicManager_Integration row removed from Integrations table. coverage-matrix updated: MidiMusicManager row repointed from the deleted standalone doc to `runtime/SSoT_Runtime_CompositionSession_Integration.md` (§3.4). Cross-reference path updates: `planning/Design_Project_Directives_v0_1.md` (MidiGenPlay_Expressive path); `planning/active/Design_Starter_Deck_v1.md` (2 path-prefix references at lines 19 and 381; `Last updated` bumped). D-Plan-6 Captivated tracking strengthening: Roadmap §4.3 line 365 stale ApplyIncomingVibe deferral fixed; bidirectional cross-ref between Audience_Status §4 and Roadmap Roster Expansion now explicit on both sides. D-Plan-8 full F1 closure: `ssot_manifest.yaml` F1 finding marked RESOLVED (severity HIGH → RESOLVED) with full closure narrative; Runtime_Flow.md SSoT notes paragraph updated to claim MidiMusicManager.cs via Runtime CompositionSession Integration §3.4; `integrations/midigenplay/ALWTTT_Uses_MidiGenPlay_Quick_Path.md §5` "Which docs to open next" link repointed; `integrations/midigenplay/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md §9` "Missing reference" subsection collapsed to a closure note. D-Plan-8 §5 backfill: this paragraph and the preceding B3-content-sibi(+followup) paragraph are the backfill entries (the previous batches did not add §5 entries in real-time). `changelog-ssot.md` new top entry covering the entire planning-reorg. `CURRENT_STATE.md §1`, `§2`, `§3`, `§4` intentionally unchanged (reorg is doc-only; operational reality unchanged). Demo readiness unchanged: still showable. No code change. No SSoT contract change. No `coverage-matrix.md` authority routing change (only the MidiMusicManager row's primary-home pointer updated).

- planning-reframe doc-closure (applied 2026-05-23, recovered entry): `CURRENT_STATE.md` §1 reframe block + §3 wholesale rewrite to S1-S8 next-active; `Roadmap_ALWTTT.md` §5.3 addendum + §5.5 DoD criterion + new §7; `Design_Demo_Cut_v1.md` §2.4 + §5.1; `Design_Project_Directives_v0_1.md` D2 + D3 appended; three new planning docs (`Design_Tutorial_System_v0_1.md`, `Design_Vertical_Slice_v0_1.md`, `Design_Sensory_Contract_v0_1.md`); `SSoT_INDEX.md` three rows; `changelog-ssot.md` top entry. Manifest, coverage-matrix, SSoT_CONTRACTS, all subsystem SSoTs intentionally unchanged.
- S2 doc-closure (applied 2026-06-14): `planning/active/Design_Sensory_Contract_v0_1.md` §3 rewritten to as-built (D-S2-4=A marker interface + readonly structs; D-S2-INIT=C lazy bus + `[DefaultExecutionOrder(-100)]` + init logs recorded as a refinement of D-S2-1=A; coexistence paragraph corrected — S2 = parallel publish, deletion = S3; consumers table corrected — only `SensoryFxAdapter` retrofitted in S2, `FloatingTextMidiListener`/`StageLightAnimator`/`BackgroundContainer` are S3+, TutorialController deferred to S4; added `SensoryFtPresentation`) + §4 (audience-reaction and song-end rows flagged `bus + direct (coexistence, D-S2-3=A)`; full sensory audit flagged DESCOPED → S3). `CURRENT_STATE.md` §1 S2 closure block prepended + S1-block neutral-RGB correction (0.75/0.75/0.78 → 0.55/0.55/0.55 darker-than-MEH); §3 S2 row closed + scope corrected (TutorialController → S4, direct-call migration → S3) + interlude note retired + S3 marked next-active; §4 new open-item (§4 audit descoped → S3); §5 (this line). `changelog-ssot.md` S2 entry prepended + S1 entry reordered above CE-L1 (newest-first) + S1 neutral-RGB corrected. `ssot_manifest.yaml`, `coverage-matrix.md`, `SSoT_INDEX.md`, `SSoT_Runtime_Flow.md`, `SSoT_Scoring_and_Meters.md` intentionally unchanged (no authority/structural change; bus is standalone, does not register through runtime managers). `SSoT_Audience_and_Reactions.md` §5.1 coexistence qualifier left as an optional minor (semantics unchanged). Code grep-verified: both `FxManager.Instance` direct-spawn calls present at the loop-reaction and the two song-end branches (coexistence intact).
- S1 doc-closure (applied 2026-06-12): `SSoT_Audience_and_Reactions.md` §6 replaced (CHR/TCH/EMT/SFX speculative axes retired; actual 4-axis taste schema documented), §5.1 rewritten (real loop-scale resolution), §5.3 added (Indifference interaction). `SSoT_Scoring_and_Meters.md` new §6.1 (impressionFactor contract). `Design_Sensory_Contract_v0_1.md` §4 audit table: audience reaction row added, vibe-change row qualifier removed. `changelog-ssot.md` top entry. `CURRENT_STATE.md` §1 gap stub replaced + §3 S1 row closed. `ssot_manifest.yaml`, `coverage-matrix.md`, `SSoT_INDEX.md` intentionally unchanged (no authority change).
- **[UNVERIFIED]** ALWTTT-PCE-PROP doc-closure (2026-06-04/05): six paste-ready blocks produced — `SSoT_Card_System.md` §5.2.1 authoritative Rhythm card→palette table, `SSoT_Runtime_CompositionSession_Integration.md` §7.1 + invariant 10, `Design_Starter_Deck_v1.md` §5.5 correction, `changelog-ssot.md` entry, `coverage-matrix.md` row, `CURRENT_STATE.md` §1 block. Confirm whether pasted; see §1 gap stub.
- CE-L1 docs integration (applied 2026-06-12): `SSoT_Editor_Authoring_Tools.md` §4 header + §4.3 + §4.5 + new §4.10 + §13 + §15; `SSoT_Card_Authoring_Contracts.md` §5.3 pointer + new §5.12 (modifierEffectNames + palette intent, route-scoped); `reference/Report_CardLLM_Pipeline.md` adopted as-is; `changelog-ssot.md` top entry; `coverage-matrix.md` Editor-authoring-tools row note; `ssot_manifest.yaml` reference entry. `SSoT_INDEX.md` intentionally unchanged (reference files are not enumerated there).
- CURRENT_STATE recovery (applied 2026-06-12): this file was overwritten by an insert-fragment after 2026-05-22; pre-overwrite version recovered, gap window replayed from chat-history insert blocks (reframe §1 block + §3 rewrite verbatim; MOD-DIR block verbatim; two `[UNVERIFIED]` gap stubs for S1 + ALWTTT-PCE-PROP pending user confirmation). Header recovery note added; newest-first-at-top convention for §1 stated explicitly. Incident should also be recorded in `changelog-ssot.md` and considered for a `ssot_manifest.yaml` known_drift_signals entry.

Pending low-priority doc edits surfaced by M1.5 P3.2:
- `SSoT_Gig_Combat_Core.md` §4.2 — one-line note on Inspiration dual-siting (PD vs session's live budget). Optional; not scheduled.

Planning docs added for M4 this session:
- `planning/Design_Starter_Deck_v1.md` — full starter deck design. Active. Amended 2026-04-24 with "Design principle: mínimas cartas, máxima expresividad" section (primary home for the principle). Substantially revised 2026-04-26 with axis-resolution session: per-card axis assignments locked for all 7 composition cards (C2 four meter cards on axis 7, Sibi two backing cards on axis 13, Sibi one melody card on axis 23); v0 cards Steady Beat / Four on the Floor / Synth Pad / Hook Theme retired in favor of Default Mode / Waltz Protocol / Pentameter / Compound Cycle / Wormus Minor / Wormus Major / Singing Field; aggregate counts preserved (12 cards / 8 composition + 4 action / 5 C2 + 3 Sibi); §9 #1 (CompositionCardPayload.effects) closed retroactively per ST-M13c-6; §9 #5 closed; §9 #7 / #8 / #9 added.
- `planning/Design_Audience_Status_v1.md` — Earworm spec + Captivated deferred design intent + `ApplyIncomingVibe` hook. Active.

Integration reference docs added 2026-04-24:
- `planning/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` — single-source reference mapping the observable musical expressive surface available to ALWTTT composition cards against MidiGenPlay package contracts. 26-axis matrix, observed precedences, per-role bundle contracts, 5 documented gaps (all with decisions deferred). Operationalizes the design principle captured in Design_Starter_Deck_v1.md. Planning/reference — not governed SSoT.

---

## 6. Working rule

`CURRENT_STATE.md` answers:
- what is the project foundation
- what is active now
- what comes next
- what is blocked or at risk
- which docs need editing next

It does **not** replace subsystem SSoTs.

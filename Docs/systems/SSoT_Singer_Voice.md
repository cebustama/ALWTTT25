# SSoT_Singer_Voice — ALWTTT

> **Class: systems SSoT (primary authority).** Governs the ALWTTT-side
> articulatory singer voice: the subsystem that sings a musician's melody
> stem instead of playing it as MIDI. This document is the authority for the
> voice budget, the per-loop arm/anchor/mute cycle, the `LoopPlaybackStarting`
> contract, and the `VoiceProfileSO` schema.
>
> **Created:** 2026-07-21 (SINGER-1 close). Predecessors: the Pink Trombone
> rendering POC (`PinkTrombone_Rendering_POC_Verdict.md` — **documento del proyecto MidiGenPlay, no de ALWTTT**; confirmado 2026-08-08, no buscarlo en `Docs/reference/`; clase research
> class) and its Session-5 lever work.
>
> **Updated:** 2026-10-01 (SONG-TAIL-1) — the Director re-arms on every song-tail loop, defers a
> session arm to the tail handoff and stops the tail voice there (F-ST-1); `SingerVoice` gains an
> external level plane that follows the tail (R-V). `CompositionSession` is still edited only by
> `LoopPlaybackStarting`.
>
> **Updated:** 2026-11-16 (VOZ-2) — the **singer v8**: `VoiceRenderer` (C# port of the Taller de
> melodías' `cantarV2`) sings zigués syllables with consonants, scoop, fall, glide and growl when
> the profile says `engine = V8_Workshop`; `SyllableBankSO` (what it says), `VoiceStylePaletteSO`
> (a melody card picks the style), live levers. `V7_CLite` stays as the opt-out. §3.1, §4, §5, §6, §8.
>
> **Authority order:** `SSoT_CONTRACTS.md` → this SSoT for singer-voice
> concepts → `CURRENT_STATE.md` for the active slice. Cross-project facts
> defer to `SSoT_ALWTTT_MidiGenPlay_Boundary.md`.

## 1. Scope and ownership

**Owns:** the `SingerVoice` runtime component (promoted from the POC
`PinkTromboneSinger` v7); the `SingerVoiceDirector` that hooks it into the gig
loop; the `VoiceProfileSO` voice-identity asset; the voice budget invariant;
the melody-channel mute contract; and the consumer-side Pink Trombone fork.

**Does NOT own:** anything in `MidiGenPlay.Runtime`. The package emits MIDI and
does not own audio synthesis. This subsystem consumes the per-musician melody
**stem** that `MidiMusicManager.RenderSinglePart` already returns; it does not
change how that stem is produced.

**Boundary.** MidiGenPlay is not modified by this subsystem — no runtime change,
no package SSoT change. The integration added exactly one ALWTTT-side seam
(`CompositionSession.LoopPlaybackStarting`, §4) and zero `MidiMusicManager`
changes. If a future need appears to require phrase-level musical metadata the
package does not expose, that is the **Phase D4 signal** (`IPerformanceMetadataSink`,
verdict §6) and it stays deferred — it is not built opportunistically.

## 2. What the subsystem is (plain terms)

Every loop, ALWTTT renders the song to MIDI and gets back the merged bytes plus
one small MIDI **stem per musician-track**. Just before playback, the singer
looks for a specific musician's `Melody`/`Lead` stem. If it finds one, it sings
those notes through a simulated vocal tract and mutes that one channel inside
MPTK, so the band plays as normal except that line, which is sung. If it finds
none — the musician has no melody this part, or no voice profile — the singer
sits out and the melody plays as ordinary GM MIDI. Coexistence with the MIDI
pipeline is therefore *by construction*: the stem filter is the only switch.

## 3. Invariants

1. **Cost is a hard design constraint, not a target.** ~10.5% DSP per voice in a
   build (POC verdict §3). `SingerVoiceDirector.ActiveVoiceCap = 1` (demo),
   `HardVoiceCap = 2` (never exceeded). A refused acquisition sits the singer out
   with a logged reason — never queued, never instantiated. The counter is a
   static reset on subsystem registration so play-mode-without-domain-reload
   cannot leak it. Verified ST-V7.
   **Singer v8 (VOZ-2, 2026-11-16):** measured in the editor with *Measure callback load*:
   Crooner load 12.7 % / peak 20.1 %, Death metal (tract 52 + growl, the heaviest style)
   13.2 % / 22.5 %; 4–7 % of real time in dotnet. **Not yet measured in a build** (VOZ-3).
   The audio thread allocates nothing: the renderer, its `Constriction[8]` buffer and the mono
   buffer exist from arm time.

2. **Monophonic.** One note at a time, last-note priority, `Melody`/`Lead` roles
   only. Inherited from the model; not an integration choice.

2b. **El filtro de rol es también un límite de alcance (registrado R6, 2026-09-01).**
   Como consecuencia directa del invariante anterior, una pista de rol `Harmony` **no** pasa
   por la voz articulatoria: su canal no se mutea y suena como instrumento GM. Verificado en
   juego con *Double Harmony* de Zig (`Melody` ch=1 muteado + Pink Trombone; `Harmony` ch=2
   sonando como GM). Esto **no es un defecto**: es el alcance Tier A aceptado en `D-R6-D4=A`.
   La segunda voz cantada es **Tier B**, y su precondición es levantar `ActiveVoiceCap` a 2 y
   medir el coste DSP de dos voces concurrentes (§3.1) — ver §8.

3. **Transport = dsp anchor (D1=A).** The singer shares no transport with MPTK.
   It arms silently, then on `IPlayMidi.OnSongStarted` captures
   `AudioSettings.dspTime` and starts its precomputed schedule from that anchor
   plus the profile's `startTrimMs`. Measured offset is constant to one DSP
   buffer (~21 ms @ 1024 samples / 48 kHz — the measurement floor, not drift;
   ST-V2). `startTrimMs` is a per-profile by-ear constant (Zig = +20). The
   pre-agreed escalation if loop-to-loop *variance* ever appears is a
   `CurrentTick`-corrected hybrid; it was not needed.

   **Pause (PAUSE-1, 2026-09-25).** Because the singer shares no transport, pausing MPTK does not stop
   it. `MidiMusicManager.SuspendPlayback` therefore also pauses the singer's playing `AudioSource`
   (every source under the manager outside the transport hierarchy) and un-pauses it on resume or on
   `Stop`. Sync after resume **verified** (ST-P1-6 PASS). **Known gap:** the player music level
   (`SSoT_Audio` §4.8) does not scale the voice — balance still comes from the profile's `gain`.

4. **Melody channel muted at runtime, per loop (D2=A).** The singer mutes its
   melody channel via `MidiMusicManager.SetChannelVolume(ch, 0)`, which writes
   through `_lastKnownVol01` so MMM's own mix reapply preserves it. The mute is
   re-asserted every `OnSongStarted` because `GigManager` repopulates musician
   volumes after `Play`. No render-path change, no re-merge; the melody's
   `IMidiNoteListener` events still fire (character animation keeps working).
   Verified ST-V5. **Known limitation:** the sit-out restore returns the channel
   to full volume and can stomp a gameplay duck / `MidiMusicManager.Highlight`
   on that channel; Highlight×mute interaction is **deferred to Dev Mode**.

5. **One re-arm per loop (D1/D2 consequence).** Card mutations never touch the
   currently playing loop; they land in the part cache and are heard on the next
   loop's fresh render. The singer therefore re-arms once per loop, at the loop
   boundary, from whatever stem the part cache holds — live regeneration needs no
   special machinery. Verified ST-V4. `loopPlayback` on the voice stays **false**
   in integration; the Director owns looping.
   **Song tail (SONG-TAIL-1, D-ST2=C):** the Director keeps the last `SingerLoopContext` and re-arms from
   it on every `MidiMusicManager.SongTailLoopStarting` — still one re-arm per loop, through the same
   `ArmForLoop` path. A session loop announced while a tail plays is **deferred** to
   `MidiMusicManager.SongTailHandoff` (arming calls `Stop()` and would silence the voice still singing the
   tail). At the handoff the tail voice is **stopped unconditionally** before arming: `ArmForLoop` can sit
   out *before* reaching `ArmFromStemBytes` (no Melody/Lead stem in the new part, budget, no notes), and
   without the explicit stop the tail voice kept singing over the new song (**F-ST-1**, fixed 2026-10-01).

6. **Fork stays consumer-side.** `Assets/PinkTrombonePOC/PinkTromboneSrc/` (~1000
   lines, MIT, zero external dependencies, edits logged `// POC-FORK(n)` 1–11 in
   `FORK_NOTES.md`). It must never enter `MidiGenPlay.Runtime`.
   **VOZ-1 (2026-11-14): POC-FORK 8–11** add the Taller de melodías extensions to the
   model: X1 constrictions + velum override (consonants), X2 per-voice tract length
   (20..88, 44 = upstream), X3 growl / roughness / breath, and fix the turbulence-point
   setter (fricatives were unreachable). **Invariant: every extension is neutral by
   default and skips its code when off** — verified byte-identical against a pre-VOZ-1
   baseline; JS = C# parity −154.8 to −158.6 dB on 9 cases
   (`reference/taller-melodia/paridad/`). Re-run the parity whenever the fork changes.
   **Ruta corregida 2026-08-08 (MANIFEST-1) contra un export del árbol del repo:
   `Assets/ThirdParty/` no existe. La sustancia del invariante —el fork es
   consumer-side, nunca dentro del paquete— se sostiene; lo que era falso era la
   ubicación. Consecuencia abierta: §7 describe el fork promovido y el arnés POC
   como dos carpetas distintas, y el árbol solo tiene una — ver `ssot_manifest.yaml`
   señal F16, pendiente de decidir si la promoción se ejecutó a medias o nunca.** Audio-path
   determinism is out of scope (a Phase D4 concern, D-POC-7=A); package MIDI
   determinism is unaffected.

## 4. The `LoopPlaybackStarting` seam

`CompositionSession` raises `event Action<SingerLoopContext> LoopPlaybackStarting`
immediately before `MidiMusicManager.PlayRaw`, on **every** path through
`PlaySinglePartLoop` (first loop, `HandleLoopFinished` replay, part advance,
tutorial hold). The payload carries `partIndex`, the loop's `stemsByTrack`
(`MusicianTrackKey → byte[]`), and the part's musical context (`tonality`,
`rootNote`, `timeSignature`, `bpm`, `seconds`) for the C-lite expressivity
`ExprContext`. Subscriber exceptions are caught and logged so a singer fault can
never kill the loop. This is the **only** edit to `CompositionSession`; it is
tagged `[SINGER-1]`.
**VOZ-2 (2026-11-16)** widens the payload, not the seam: `songSeed` (the song's seed, stable for
the whole song; 0 when none) and `voicePaletteByMusician` (the `voicePalette` of each Melody/Lead
track's source card in this part, by `musicianId`; null when none), filled by
`CompositionSession.CollectVoicePalettes`, tagged `[VOZ-2 / D6 = B]`.

Per-loop cycle in the Director:
1. `LoopPlaybackStarting` → find the opted-in musician's `Melody`/`Lead` stem;
   arm `SingerVoice` from it (main thread, precomputed schedule, voice waits
   armed and silent).
2. `OnSongStarted` → capture `dspTime`, `StartAtDspTime`, assert the channel mute.
3. Loop end → next `LoopPlaybackStarting` re-arms from the (possibly new) stem.

Song-tail cycle (SONG-TAIL-1): `SongTailLoopStarting` → re-arm from the last context → `OnSongStarted` →
anchor + mute, as above. `SongTailHandoff` → stop the tail voice → arm the deferred session context.
`SongTailStopped` → `Deactivate`. Every frame the Director passes `MidiMusicManager.TailPlane01` to
`SingerVoice.SetExternalLevel01` (1 when no tail).

## 5. `VoiceProfileSO` schema

The serialized voice identity. Schema source: `reference/PinkTrombone_Voice_Levers.md`
— **promovido a `Docs/reference/` de ALWTTT el 2026-08-08 (MANIFEST-1, D13=A)**, tal
como prescribía su propia cabecera ("viaja con el cantante cuando se promueve a
ALWTTT"); el cantante se promovió en SINGER-1. Es diseño de voz consumer-side, no un
interno del paquete. Los **seis levers** (looseness, vibratoDepth, vibratoSpeedHz,
diction, mouth, brightness) más los campos de identidad de su §2 son la superficie
diseñada, y **nada más entra en el perfil**: los ~40 campos restantes del cantante
son diagnóstico y plumbing de fixture. El `PinkTrombone_Rendering_POC_Verdict.md`
sigue siendo del proyecto MidiGenPlay (clase research)
§5. The profile is the **resting state**; gameplay animates the levers at runtime
(lever doc §3 tier 2). Tier 3 (phrase-metadata automation) is Phase D4, deferred.

- **Macro levers:** `looseness`, `vibratoDepth`, `vibratoSpeedHz`, `diction`,
  `mouth` (enum), `brightness`.
- **Identity:** `transposeSemitones`, `tensenessAtVel0/127`,
  `vibratoDelaySeconds/RampSeconds`, `pitchLeadSeconds`, `leadFullInterval`,
  `minLoudness`.
- **Transport:** `startTrimMs` (the D1=A per-profile constant).
- **Output:** `gain`.
- **v2 (VOZ-1, D-VOZ-2 = A, 2026-11-14):** `engine` (`V7_CLite` | `V8_Workshop`),
  `tractLength`, `breath`, `growl`, `roughness`, `drive`, `scoopSemitones`, `scoopMs`,
  `fallSemitones`, `glideMs`, `attackPress`, `legato`, `baseVowel`. Names and defaults
  follow the Taller de melodías («Copiar para la carta» → `propuestaVoiceProfileV2`), so an
  imported mode maps 1:1. **Read only by the singer v8 (VOZ-2); with `engine = V7_CLite`
  the profile sounds exactly as before.** `vibratoDepth` widened 0–1 → 0–3 (opera 2.6);
  values ≤ 1 are unchanged. Plan and decisions D-VOZ-1..7:
  `planning/active/ARRANQUE_estilos_de_voz.md`.
- **`syllableBank` (VOZ-2):** the `SyllableBankSO` the v8 sings (below). `ToV8Params` copies
  the profile into the renderer's `VoiceV8Params`.

### 5.1 Singer v8 (VOZ-2, 2026-11-16)

With `engine = V8_Workshop` the Director's arm goes to `VoiceRenderer`
(`Assets/Scripts/Music/Voice/V8/`, pure C#, no UnityEngine), a port of `cantarV2`
(`reference/taller-melodia/motor-canto.js`): syllables with consonants (X1), diphthongs, «mm»,
coda, scoop, fall, glide, pitch lead, attack, legato, vibrato with delay and ramp, two-pole tone
stage, tanh drive. **No C-lite in v8** (D4 = A). Decisions in `planning/archive/ARRANQUE_VOZ-2.md`.

- **Parity is the contract.** JS = C# on 13 full-render cases (−114 to −163 dB; criterion
  < −60 dB), identical syllables, lexicon and phrase ends; harness in
  `reference/taller-melodia/paridad/cantante/` (`node casos.mjs && dotnet run -c Release && node
  casos.mjs comparar`). C# does its arithmetic in double where JS does. Re-run it whenever the
  renderer, the syllables or `motor-canto.js` change.
- **Control blocks of 256 samples inside the renderer** (D3 = A), independent of the DSP buffer:
  256 / 441 / 1024 buffers give identical output, so real time = offline (VOZ-3's editor render).
- **Phrase ends from the stem** (D1 = B + D8 = B, rule F): a note ends a phrase if it is the last,
  is followed by a rest of ≥ 1 beat or lasts ≥ 2 beats, **and** ends within a beat of an even bar
  line (`SyllableBankSO.phraseGridBars = 2`; 0 = without the grid). A phrase end of ≥ 1 beat
  gets the forced fall when the profile's `fallSemitones > 0`. Measured against the workshop's
  composer: 68 % precision / 100 % recall (30 % without the grid).
- **Seeds (D2 = A):** lexicon from the bank's `lexiconSeed` (Zig's language, the same in every
  song); verse from `hash(songSeed|part|verso)`; noise from `songSeed ^ hash("voz-ruido")`. With
  the pinned song seed the same song sings the same verse (smoke 7). A bank's hardness changes
  consonants, not the words: `nase` (Crooner) → `xase` (Death metal).
- **Volume (D5 = A):** `gain` per profile, calibrated so each style's RMS equals the v7 Zig with
  diction (`paridad/cantante/gen-estilos.mjs`). The renderer does not normalize.
- **Live levers:** the renderer reads `VoiceV8Params` every block; `SingerVoice.Update` copies the
  profile plus `liveTensenessOffset`, `liveVibratoScale`, `liveGrowlOffset` (public
  `SetLiveTenseness/VibratoScale/Growl`; context menu «VOZ-2/…»). Heard on the next block. No
  gameplay consumer yet.
- **Log:** `[VOZ-2] Armed V8: profile=… [source], bank=…, songSeed, part, verseSeed, noiseSeed,
  notes, phraseEnds (grid), tract, sr, lexicon=[…]` plus the first 16 sung syllables; with
  `V7_CLite`, `[VOZ-2] Armed V7_CLite: profile=… [source]`.

**`SyllableBankSO`** (`Assets/Scripts/Data/Audio/`, D-VOZ-4 = A): `mode` = `Zigues` (from
`lexiconSeed` = 4242 and `hardness` 0–1, same draws as the workshop), `WeightedList` (syllable,
weight, `isEnd`) or `Text`; `phraseGridBars`.

**`VoiceStylePaletteSO`** (D6 = B): weighted list of profiles. A melody card's
`TrackActionDescriptor.voicePalette` makes the singer pick one with
`Rng(songSeed ^ partIndex·7919, "voz-paleta")`: one style per song and part, reproducible.
**The card's palette wins over the Director's profile** (log source `[palette …]` vs
`[musician]`).

**Assets** (menu *ALWTTT ▸ Voz ▸ Crear estilos del taller (VOZ-2)*, never overwrites):
`Voice Profiles/Estilos/` 10 V8 profiles (Zig V8, Tierno y calmado, Pop, Punk rock, Death metal,
Ópera, Crooner, Falsete, Robot, Borracho); `Bancos/` `Zigues_*` and `Silabas_*`; `Paletas/`
`Paleta_Todos_Los_Estilos`, `Paleta_Zig_Se_Emborracha`.

Defaults equal the lever doc's "Settled singer" recipe (the only listen-verified
voice). Gameplay-modulation hooks (per-state overrides) are deliberately absent
until a concrete consumer exists; section-level "when" is driven by gameplay code
calling the live levers, not by data here.

## 6. Opt-in

**Shipped (D4=B).** `SingerVoiceDirector` carries a serialized `musicianId`
(matches `MusicianTrackKey.MusicianId` = `MusicianCharacterData.CharacterId`) and
a `VoiceProfileSO`. Non-empty id + non-null profile + a matching melody stem =
the musician is sung. Current singer: **Zig** (`musicianId = "3"`).

**Diction on (S9c, D-ZIG-1 F0, 2026-11-14).** `Zig_SingerVoice.prefab` ships with
`expressivityEnabled = 1`: the vowel opens and moves on long notes and strong beats instead of one
flat vowel. Heard by Claudio (smoke 1). In `SingerVoice.VowelPresets` the preset formerly named
«Back (oh)» is renamed **«Back~a»**: it measures F1 730 / F2 1650, an «a», not an «oh». Same
values, honest name. The POC copy in `Assets/PinkTrombonePOC` is untouched. Phases F1–F3
(melodies by figure, consonants, zigués syllables) stay post-demo.

**Zig sings v8 (VOZ-2, D9 = A, 2026-11-16).** `Zig_SingerVoice.prefab` ships with
`VoiceProfile_Zig_V8` (zigués, hardness 0.5). **The switch back** (D-VOZ-7) is the Director's
`profile` field: put `VoiceProfile_Zig` back (v7 + diction). No demo card has a voice palette
yet (Rise Up and Showtime were emptied after the smokes); which card gets which palette is a
design decision for VOZ-3.

**Owed (D4=A).** A `VoiceProfileSO` field on `MusicianCharacterData` (its existing
`[Header("Audio")]` section, beside `MusicianProfileData profile`) + a Director
lookup, retiring the serialized string. Follow-up, not required for the demo.

## 7. Lifecycle and placement

- `SingerVoice` + `SingerVoiceDirector` (+ an `AudioSource` for
  `OnAudioFilterRead`) live as a **child of the `MidiMusicManager` prefab**
  (D-S1B-2=B), inheriting `DontDestroyOnLoad` from that root — exactly one singer,
  same lifetime as the manager it hooks, adapter is a sibling. Files:
  `Assets/Scripts/Music/Voice/{SingerVoice,SingerVoiceDirector}.cs`,
  `Assets/Scripts/Data/Audio/VoiceProfileSO.cs`.
- The POC folder `Assets/PinkTrombonePOC/` is retained as a **tuning harness**
  (IMGUI panel, Sustain Test, scale fixture) referencing the promoted assembly
  (D3=B). Retire after voice profiles are settled, not before.
  **Corrección 2026-08-08 (MANIFEST-1, señal F16).** Esta viñeta y §3.6 describían
  **dos** carpetas —el fork promovido en `Assets/ThirdParty/PinkTrombone/` y el
  arnés POC aparte— y el árbol del repo solo tiene **una**: el fork vive dentro del
  propio `Assets/PinkTrombonePOC/PinkTromboneSrc/`, con su `FORK_NOTES.md` y su
  `LICENSE.txt`, y los dos documentos del POC lo confirman por escrito. **La
  promoción a `ThirdParty/` nunca se ejecutó.** El invariante de §3.6 se sostiene
  en su sustancia —el fork es consumer-side y nunca entra en `MidiGenPlay.Runtime`—
  y esa parte no cambia. Lo que queda abierto no es documental sino de ciclo de
  vida: **decidir si se sigue queriendo la promoción antes de retirar el arnés**,
  porque hoy el código de producción del cantante depende de una carpeta cuyo
  nombre dice "POC".
- Retired: `PinkTromboneBackingPlayer.cs` (MPTK external-file route, obsolete —
  ALWTTT uses `IPlayMidi.Play(byte[])`) and the original `PinkTromboneSinger.cs`.

## 8. Deferred / out of scope

- `MidiMusicManager.Highlight` × mute interaction — Dev Mode validation.
- Second concurrent voice (cap = 2) — **Tier B, diferido a R8 (D-R6-6, 2026-09-01).**
  El consumidor previsto ya existe y está validado: *Double Harmony* de Zig ships en R6 como
  Tier A (pista MIDI de rol `Harmony`, sonando GM). La validación auditiva de R6 confirmó que la
  armonía es musicalmente correcta, así que Tier B deja de ser una apuesta de diseño y pasa a ser
  un problema de presupuesto: hay que levantar `ActiveVoiceCap` a 2 en Dev Mode, medir el DSP de
  dos voces (~10.5 % cada una, §3.1) y decidir cómo el director gestiona cap, duck y sincronía con
  **dos** stems del mismo músico. Nótese que hoy el director resuelve por rol `Melody`/`Lead`:
  admitir `Harmony` es un cambio de su regla de selección, no sólo del contador.
- Mixer-group routing — the singer currently bypasses `AudioMixSettings` and the player plane, with one
  exception since SONG-TAIL-1: `SingerVoice.SetExternalLevel01` (volatile target, per-sample one-pole
  smoothing ~10 ms on the audio thread; ×1f is exact, so level 1 is bit-identical) follows the song-tail
  plane (R-V);
  balance is the profile `gain`. A small follow-up.
- ~~Consonants / nasal branch~~ — **done in VOZ-2** (singer v8, §5.1). The v7 path still sings
  vowels only.
- VOZ-3: TV1/TM1 importer for styles from the workshop, «▶ con voz» and `.wav` in the Banco de
  cartas (offline `VoiceRenderer`), TV1 button in the workshop, CPU of the v8 in a build.
- Gameplay consumers of the live levers (e.g. Stress closes the throat) — none yet.
- Phase D4 `IPerformanceMetadataSink` — deferred; re-open only on repeated,
  concrete demand for phrase-level metadata from this integration.

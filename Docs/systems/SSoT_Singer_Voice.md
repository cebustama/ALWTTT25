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

2. **Monophonic.** One note at a time, last-note priority, `Melody`/`Lead` roles
   only. Inherited from the model; not an integration choice.

3. **Transport = dsp anchor (D1=A).** The singer shares no transport with MPTK.
   It arms silently, then on `IPlayMidi.OnSongStarted` captures
   `AudioSettings.dspTime` and starts its precomputed schedule from that anchor
   plus the profile's `startTrimMs`. Measured offset is constant to one DSP
   buffer (~21 ms @ 1024 samples / 48 kHz — the measurement floor, not drift;
   ST-V2). `startTrimMs` is a per-profile by-ear constant (Zig = +20). The
   pre-agreed escalation if loop-to-loop *variance* ever appears is a
   `CurrentTick`-corrected hybrid; it was not needed.

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

6. **Fork stays consumer-side.** `Assets/PinkTrombonePOC/PinkTromboneSrc/` (~1000
   lines, MIT, zero external dependencies, edits logged `// POC-FORK(n)` 1–7 in
   `FORK_NOTES.md`). It must never enter `MidiGenPlay.Runtime`.
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

Per-loop cycle in the Director:
1. `LoopPlaybackStarting` → find the opted-in musician's `Melody`/`Lead` stem;
   arm `SingerVoice` from it (main thread, precomputed schedule, voice waits
   armed and silent).
2. `OnSongStarted` → capture `dspTime`, `StartAtDspTime`, assert the channel mute.
3. Loop end → next `LoopPlaybackStarting` re-arms from the (possibly new) stem.

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

Defaults equal the lever doc's "Settled singer" recipe (the only listen-verified
voice). Gameplay-modulation hooks (per-state overrides) are deliberately absent
until a concrete consumer exists; section-level "when" is driven by gameplay code
calling the live levers, not by data here.

## 6. Opt-in

**Shipped (D4=B).** `SingerVoiceDirector` carries a serialized `musicianId`
(matches `MusicianTrackKey.MusicianId` = `MusicianCharacterData.CharacterId`) and
a `VoiceProfileSO`. Non-empty id + non-null profile + a matching melody stem =
the musician is sung. Current singer: **Zig** (`musicianId = "3"`).

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
- Second concurrent voice (cap = 2) — needs a second singer character
  (Zig's self-harmony finisher is the intended first consumer of slot 2).
- Mixer-group routing — the singer currently bypasses `AudioMixSettings`;
  balance is the profile `gain`. A small follow-up.
- Consonants / nasal branch — vowels only.
- Phase D4 `IPerformanceMetadataSink` — deferred; re-open only on repeated,
  concrete demand for phrase-level metadata from this integration.

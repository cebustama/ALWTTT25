# SSoT_Runtime_Flow — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Current ALWTTT gig runtime flow and orchestration across setup, card play, song execution, feedback, and audience response  
**Owns:** runtime phase flow, manager/session responsibilities, card entry points into runtime, loop/part/song feedback lifecycle  
**Does not own:** detailed combat tuning (`systems/SSoT_Gig_Combat_Core.md`), detailed card semantics (`systems/SSoT_Card_System.md`), or package-internal MidiGenPlay generation algorithms

---

## 1. Purpose

This document is the primary authority for **how ALWTTT runs a gig at runtime**.

It promotes runtime truth that was previously scattered across:
- `reference/Gig.md`
- `backlog/ideas/gig_pipeline_summary.md`
- `reference/subsystems/Composition Pipeline/SSoT_Runtime_CompositionSession_Bridge.md`

This SSoT is about **execution flow**, not just design intent.

---

## 2. Runtime scope boundary

This SSoT answers:
- which runtime components orchestrate a gig
- how the player moves from hand/cards into composition and playback
- when loop/part/song feedback is produced
- where audience reaction hooks are inserted
- where song-level state begins and ends

This SSoT does **not** define the full balance meaning of SongHype, Vibe, Stress, or statuses.
Those semantics live in system SSoTs.

The flow **between** gigs (menu → ship hub → map → gig → back to the ship hub, the
`GigReturnDestination`, run-state carry-over) lives in `systems/SSoT_Gig_Combat_Core.md` §13
(updated S5 NAVE-1, 2026-11-11).

---

## 3. Canonical runtime actors

### 3.1 GigManager
`GigManager` is the gig-scene orchestrator.

It owns the high-level runtime responsibilities for:
- gig setup
- phase transitions
- creating and ending the active song/session runtime
- holding references to runtime UI/services needed by the gig
- consuming loop/part/song feedback and routing it into ALWTTT gameplay state

### 3.2 DeckManager + HandController
These own the runtime hand/deck surface.

They are responsible for:
- current playable hand state
- initiating card play from the UI
- forwarding play attempts to the relevant game runtime host

**Hand tuck (S9, 2026-11-12).** While the pointer is outside a bottom band (the hand bounds plus
`tuckZoneExtraTop` / `tuckZoneExtraSide`) and no card is held or dragged, `HandController` slides the
whole curve down by `tuckDrop` (0.9), so the stage above it stays visible; back in the band, it rises.
The band is fixed in the hand's local space, so reaching for the visible card tops always raises the
hand. Switch: `tuckWhenAway`. Log: `[S9] Mano | baja (ratón fuera)` / `sube (ratón cerca)`.

### 3.3 SongCompositionUI
`SongCompositionUI` is the authoritative editable song/part/track model surface inside ALWTTT runtime.

It is responsible for:
- presenting the current composition state to the player
- receiving composition card mutations
- exposing the current model used to build playback configuration

### 3.4 CompositionSession
`CompositionSession` is the song-scoped runtime state machine.

It owns:
- part progression inside the active song
- loop counting within the active part
- composition-time resource flow owned by the current song/session
- generation of loop/part/song feedback snapshots
- ALWTTT-side composition card application lifecycle

### 3.5 SongConfigBuilder
`SongConfigBuilder` converts ALWTTT's editable composition model into the runtime playback/build configuration consumed by the music runtime.

### 3.6 MidiMusicManager
`MidiMusicManager` is an **ALWTTT runtime integration component**.

It is not the authority for package generation internals.
Its job is to host/control the music playback surface used by the gig runtime.

---

## 4. Canonical gig runtime phases

### Phase 0 — Gig setup
Runtime responsibilities:
- initialize the gig scene
- prepare band and audience runtime objects
- wire deck/hand/runtime UI references
- prepare the runtime services needed for composition and playback

Venue and placement (S9 REINA-1, 2026-11-12):
- **The venue opens before anything is shown.** `GigManager.Start` calls `PreOpenBackground` with the
  encounter (`GigRunContext` or `PersistentGameplayData.CurrentEncounter`) before the tutorial
  question, so nothing of the previous venue flashes. `BackgroundContainer` gathers every
  `BackgroundRoot` under it (`AllRoots`, inactive included) and turns on **only** the one whose
  `VenueType` matches (`SetActive(match)`); `Current` is the open one. Log `[S9] Local | VenueX → <name>`.
- **Placement per venue.** When the open `BackgroundRoot` has a `bandStageLayout`, `BuildBand` uses it
  instead of the scene's; when it has `audiencePositions`, `AudienceMemberPosList` returns them. Slot
  scale = character scale in that venue. Log `[S9] Colocación | banda=<local X|escena> | público=…`.
- **Tutorial question off.** `GigDevSettingsSO.askTutorialOptIn` (default false): when false the
  opt-in is answered «no» without asking (`[S9] Tutorial | sin pregunta…`), until the tutorial is rebuilt.
- **Dev node jump (editor / `ALWTTT_DEV` only).** On the linear map, Shift+click on a later node
  launches it (`[S9] Salto de nodo (dev) | a → b '<title>'`).

Flag initialization:
- `_actionWindowOpen = true`, `_isSongPlaying = false`, `_isBetweenSongs = true` are all set once in `SetupGig()`. Their per-turn lifecycle is governed by §4 Phase 1 below.

### Phase 1 — Between-songs action window
Purpose:
- allow non-composition card play in the song transition window when the current rules permit it
- apply immediate systemic effects before the next song composition begins

Contract:
- this is where **Action cards** belong in the baseline governed flow
- if the current implementation slice only partially exposes this window, `CURRENT_STATE.md` records the gap; this SSoT still owns the intended runtime home

#### 4.1 Action window flag lifecycle (2026-04-20)
`GigManager.CanPlayActionCard` gates action-card play through three flags:

| Flag | Meaning |
|---|---|
| `_isSongPlaying` | True while a song's performance loop is running. When true, action cards are disabled except those explicitly marked `CardActionTiming.Always` AND only if `allowActionCardsDuringPerformance`. |
| `_actionWindowOpen` | True during the player's composition/action window for the current song. Closes when the player presses Play (`OnPlayPressed`). |
| `_isBetweenSongs` | True while the gig is in a between-songs state, outside any active song performance. |

**Per-turn lifecycle rule:** `_actionWindowOpen = true` and `_isBetweenSongs = true` are re-asserted at the top of `ExecuteGigPhase(PlayerTurn)` (after the gig-completion early-exit check, before status tick and draw logic). This ensures every PlayerTurn opens a fresh action window, regardless of how many songs have preceded it in the gig.

**Historical note:** prior to 2026-04-20 these flags were set exactly once in `SetupGig()` and `_actionWindowOpen` flipped to false on the first `OnPlayPressed`, never to be reopened. The result was a latent bug: action cards became unplayable in the second and all subsequent songs of any multi-song gig (Dev Mode or production). Fixed in GigManager 2026-04-20 and validated by ST-FIX-1 (normal multi-song) and ST-FIX-2 (Dev Mode infinite turns).

#### 4.2 Bidirectional guaranteed draws (M4.5, 2026-04-30)

`ExecuteGigPhase(PlayerTurn)` calls `DeckManager.DrawCardsForPlayerTurn(DrawCount)` instead of the bare `DrawCards(DrawCount)`. The wrapper guarantees that the hand starts the turn with at least one Action card and at least one Composition card, when the piles allow it, without exceeding `DrawCount` or `MaxCardsOnHand`.

**Subtractive rule.** Total cards drawn at PlayerTurn entry never exceeds `min(DrawCount, MaxCardsOnHand - HandPile.Count, DrawPile.Count + DiscardPile.Count)`. Guarantees consume normal slots; they do not extend the hand size.

**Hand cap, single read point (DEV-TUNING-1, 2026-11-17).** Every hand-cap check in `DeckManager` reads `DeckManager.MaxHandSize` = `DevTuning.HandMax(GameplayData.MaxCardsOnHand)`; `DrawCount` at PlayerTurn entry reads `DevTuning.Int("robo.turno", DrawCount)`. Without a dev override (or without `ALWTTT_DEV`) both are the asset values. See `SSoT_Dev_Mode.md` §23.

**Three-phase algorithm.**
1. Compute `needComp = !HandHas(IsComposition) && PilesHave(IsComposition)`. Symmetric for `needAction`. Reserve one slot per need.
2. Phase 1: draw `effectiveBudget - reserved` cards through the normal random path.
3. Phase 2: re-evaluate each need against the current hand. If a Phase 1 random draw already satisfied the need, the reserved slot is freed. Otherwise call `DrawCardFiltered(predicate, reasonTag)` to draw exactly one matching card.
4. Phase 3: any reserved slots freed by re-evaluation are filled with normal random draws.

**Filtered draw.** `DeckManager.DrawCardFiltered(Func<CardDefinition,bool>, string)` scans `DrawPile` for matches, picks one uniformly at random, removes it, and adds it to the hand through the same `BuildAndGetCard` → `AddCardToHand` → `HandPile` path used by `DrawCards`. If `DrawPile` has no match but `DiscardPile` does, `ReshuffleDiscardPile()` triggers once and the scan retries. Returns `false` when no match exists in either pile, when prerequisites block the draw, or when the hand is at `MaxCardsOnHand`.

**Tie-break.** When `effectiveBudget < reserved` (e.g. budget = 1 and both guarantees needed), Composition wins. Action guarantee is skipped that turn. Rationale: an action-less turn is recoverable through composition play; a composition-less turn stalls the song.

**Hook collapse.** The roadmap's "two symmetric hooks" framing (composition phase entry + action window entry) collapses to a single site in current implementation because both windows open simultaneously at PlayerTurn entry. There is no separate composition-phase-entry callable in `GigManager`. If a future redesign separates the action window from the composition window into distinct phase transitions, the hook split happens then.

**Observability.** Each PlayerTurn emits one summary log line via `DeckManager._lastTurnGuaranteeSummary` (format: `needs=[CA] reserved=N fired=[CA] drawn=K/B`) and one `[M4.5 GuaranteeComp]` / `[M4.5 GuaranteeAction]` line per fire. The summary is exposed to the Dev Mode overlay via `DeckManager.LastTurnGuaranteeSummary` and rendered as `M4.5 last draw: …` always-on (independent of `_verboseLogs`).

**Exhaustion case.** If a domain has zero references in `DrawPile ∪ DiscardPile` (all references in `HandPile` or `ExhaustPile`, or none authored in the deck), the corresponding guarantee skips silently with a log line. Cannot violate the guarantee contract because the contract is conditional on "piles allow." With the v1 starter (no `ExhaustAfterPlay` cards), this case is not reachable in normal play.

### Phase 2 — Composition phase
Purpose:
- player edits the current song/part state through composition cards and composition UI

Runtime rule:
- composition card play enters the runtime through hand/controller -> gig runtime host -> active `CompositionSession`
- the composition model is edited before the song/part is committed to playback

### Phase 3 — Song performance phase
Purpose:
- the currently configured part/song plays
- loop-level feedback is emitted
- audience micro-reaction hooks can evaluate the current music output

Runtime rule:
- the player is no longer editing the currently committed loop in the same way as the composition phase
- loop completion emits loop-level feedback snapshots

### Phase 4 — Song end resolution
Purpose:
- finalize song-scoped feedback
- convert accumulated loop/song results into downstream game state updates
- prepare audience turn / next-song transition

### Phase 5 — Audience response phase
Purpose:
- the audience acts back on the band based on accumulated song performance and encounter rules

### Phase 6 — Next song or end gig
Purpose:
- either initialize the next song-scoped runtime session or terminate the gig encounter

### Phase machine bypass during CompositionSession
`GigManager.ExecuteGigPhase()` returns early when `_session != null` (a `CompositionSession` is active).

This means the normal phase machine logic does not execute while the composition session is running. The phase machine resumes when the session completes and `_session` is cleared.

This is a deliberate architectural separation, not a bug — it keeps the composition/session lifecycle cleanly decoupled from the phase state machine. Any runtime feature that needs to interact during an active session must hook into the session lifecycle directly, not through the phase machine.

### Song tail between songs (SONG-TAIL-1, closed 2026-10-01)

With `GigFlowSettingsSO.songTailEnabled` ON, the music does not stop between songs:

1. **Song end (Phase 4).** `OnCompositionSongFinished` sends only the audience to idle (the band keeps
   its "playing" state, D-ST4=A); `OnCompositionSessionEnded` nulls `_session`, returns the crowd bed and
   starts the **tail** — the last loop repeating outside the session (`SSoT_Audio` §4.10). The phase
   machine runs normally under it: Vibe resolution, audience turn (Phase 5), and the next PlayerTurn with
   its new session (Phase 6) all happen with the tail sounding.
2. **Play.** `OnPlayPressed` → the part renders at once; its audio is held and starts on the tail's next
   bar line (D-ST7=C). Nothing else in the flow waits for it.
3. **Other ends.** `WinGig` / `LoseGig()` fade it out; scene change cuts it (`GigManager.OnDestroy`).
4. **OFF** = previous flow: silence + crowd bed between songs, band idle at song end.

### Play timing probe (PLAY-TIMING-0, closed 2026-10-01) — log only

On every accepted Play (after the tutorial input gate, before the synchronous render),
`PlayPressTimingProbe.LogForPlay` writes one `[PLAY-TIMING]` line: the signed distance from the press to
the nearest edge of the loop that is playing — the bar line that closes it (EARLY) or the one that opened
it (LATE) (D-PT1=A, D-PT2=B) — graded in absolute ms (PERFECT / EXCELLENT / GOOD = window, else FUERA DE
VENTANA with both distances, F-PT-1), after subtracting a calibration offset (D-PT3=A). The measurement is
taken at **pointer-down** on the Play button (`Button.onClick` fires on release; D-PT4=B); `(click)` marks
the fallback. `~` marks the first frames of a loop, measured in real time before its grid publishes.
Settings: `GigFlowSettingsSO` «Play Timing Probe». **No gameplay effect**; a reward for timing is a
mechanic and belongs to Phase C (D1=A). Not gated by `ALWTTT_DEV`: switch it off in demo builds.

### Pause state (PAUSE-1, 2026-09-25)

A pause can happen **at any moment of a gig**, including mid-song (D2=A), and freezes gameplay **and**
music, resuming where both were (D1=A). It is orthogonal to the phase machine and to the session: it
changes neither `CurrentGigPhase` nor `_session`.

- **Owner:** `PauseMenuController` (`Assets/Scripts/UI/Pause/`) on `PauseCanvas`, a child of the
  `UIManager` prefab (DontDestroyOnLoad, own Canvas, sort order above the card detail modal — D3=B).
  Reached only through `UIManager.HandleEscapeKey`; it never polls input. Only pausable where
  `GigManager.Instance != null`.
- **What it freezes, in one frame:** `Time.timeScale = 0` (scaled coroutines, `Time.time`,
  `Time.deltaTime`: turn waits, the `CompositionSession` loop clock, animations); `PauseGate.IsPaused`
  (systems that read input in `Update` — `timeScale = 0` does not stop `Update` or `Input`);
  `MidiMusicManager.SuspendPlayback` (§4.9 of `SSoT_Audio`); `AudioManager.PauseAmbience`.
  A card held outside the hand is returned to it first (`HandController.ReleaseHeldCardToHand`).
- **What it does not freeze, by design:** unscaled fades (OST, ambience), the tutorial overlay's typing,
  tooltips. **D8 resolved: nothing was migrated to unscaled time** — everything that freezes is gameplay.
- **Exits that must never leave the game frozen:** Continue; "Volver al menú" (confirmed, D5=B — stops
  the MIDI **before** restoring time, then `UIManager.ChangeScene`); any non-additive scene load; the
  controller's destruction.
- **Song tail (SONG-TAIL-1):** the tail shares the transport, so it freezes in place; while suspended no
  tail repeat starts and no held handoff fires, including in the gap between two loops; the tail fade runs
  in scaled time and freezes too (ST-ST-7).
- Verified ST-P1-1, 2, 4, 5, 7, 10.
- **Feedback panel (FEEDBACK-2, 2026-11-14):** «Enviar feedback» in the pause opens
  `FeedbackPanelController` (`PauseCanvas/Root/FeedbackPanel`): text box, Send, Cancel, one privacy line,
  a status line and, only after a failure, «Copiar» with the S9c mail fallback
  (`PauseMenuController.BuildMailFallback`, D3=B). Only reachable from the pause (D2=A).
  - **Destination:** `FeedbackForm` posts to the Google Form's public `/formResponse` (`UnityWebRequest`,
    15 s timeout). Configuration is one constant, the pre-filled link with placeholder letters
    M/V/S/E/B/G; no secret in the build. Sent with the text (D1=B): version, OS, scene, band
    (`lineupId (CharacterIds)`) and gig (`encounter label · songIndex`). The log never prints the text.
  - **Survives the pause:** the coroutine yields on the request (real time, unaffected by
    `timeScale = 0`) and runs on `PauseMenuController`, so closing the panel or resuming mid-send does
    not kill it. The text is kept on Cancel, Esc and failure; it is cleared only on HTTP 200.
  - **Esc chain** (`UIManager.HandleEscapeKey`): card detail → Options → **Feedback panel** → pause.
    `restoreOriginalTextOnEscape = false` so Esc does not wipe the text.
  - **Typing guard:** `TextEntryGate.IsTyping` (a focused `TMP_InputField`) blocks the letter hotkeys
    read in `Update` (M = MidiGenPlay panel, D = instrument picker).
  - Logs `[FEEDBACK-2]`. Smokes 1–3 played by Claudio (response arrived in the sheet).

---

#### Pausa con Espacio y SFX de pausa (S10, 2026-11-26)

- **Espacio** (D9) pausa como el menú (tiempo, música, loops) pero deja la mano inspeccionable: hover y tooltips (`PauseGate.InspectOnly`).
- **SFX**: `PauseMenuController` toca `pauseSfx` al pausar y `resumeSfx` al reanudar (scratch y su reversa) por una `AudioSource` propia con
  `AudioLowPassFilter`; volumen = `pauseSfxVolume` × barra de Efectos; `pauseSfxPitch` acelera; los clips se precargan en `Start`.
  Log `[S10] SFX de pausa: … | vol … | paso bajo … Hz | velocidad x…`.

## 5. Card entry points into runtime

### 5.1 Action cards
Canonical runtime sequence:
1. player selects an action card from hand
2. runtime validates timing/targeting (`GigManager.CanPlayActionCard`, see §4.1 for flag lifecycle)
3. runtime interprets the card's gameplay effect list
4. post-play state is applied (discard/exhaust/etc. according to card rules)

### 5.2 Composition cards
Canonical runtime sequence:
1. player selects/plays a composition card from hand
2. the active gig runtime forwards the request to the active `CompositionSession`
3. ALWTTT composition data is validated and applied to the editable song model
4. any gameplay-side card effects owned by the card may also execute as immediate systemic consequences
5. if the card changes audible output, runtime rebuild/invalidation behavior is triggered according to the integration contract

---

## 6. Session lifecycle

### 6.1 Session creation
A `CompositionSession` is created per active song runtime.

Runtime invariant:
- there is at most **one active composition session per active song**.

### 6.2 Session active state
While active, the session owns:
- the current part index
- current loop index / remaining loop tracking
- current editable/committed song state needed for playback
- production of feedback contexts

### 6.3 Session completion
At song end:
- the session emits song-finished feedback
- the gig runtime consumes the result
- song-scoped runtime state is cleaned up or reset for the next song

---

## 7. Feedback lifecycle

### 7.1 Loop-level
At loop completion, runtime produces a `LoopFeedbackContext`-style payload representing the just-played loop.

Runtime use:
- loop scoring hooks
- SongHype delta computation hooks
- audience micro-reaction hooks
- loop-based inspiration/resource hooks where applicable

### 7.2 Part-level
At part completion, runtime produces a `PartFeedbackContext`-style payload for the completed part.

### 7.3 Song-level
At song completion, runtime produces a `SongFeedbackContext`-style payload.

This is the canonical runtime handoff used to drive:
- song-end conversion into audience-facing progress
- encounter-level scoring hooks
- next-phase transitions

---

## 8. Runtime invariants

1. `GigManager` owns gig-scene orchestration and phase transitions.
2. `CompositionSession` owns active song/session progression and feedback emission.
3. `SongCompositionUI` is the ALWTTT-side source of editable composition state.
4. `SongConfigBuilder` converts ALWTTT model state into music playback build input.
5. `MidiMusicManager` is part of the ALWTTT runtime surface and must be documented here/on the integration side, not as package truth.
6. Loop/part/song feedback belongs to the ALWTTT runtime contract even when downstream systems use MidiGenPlay for playback/generation.
7. `ExecuteGigPhase()` is bypassed while a `CompositionSession` is active. Phase machine and session lifecycle are explicitly decoupled.
8. **Action window flags (`_actionWindowOpen`, `_isBetweenSongs`) are per-PlayerTurn, not per-gig.** They are re-asserted to `true` at the top of `ExecuteGigPhase(PlayerTurn)` after the completion check. Any future gig-flow code that adds new single-use-per-gig flags must document its lifecycle explicitly here (see §4.1).
9. **Hand draw at PlayerTurn entry guarantees ≥1 Action and ≥1 Composition card in hand when `DrawPile ∪ DiscardPile` allow, without exceeding `DrawCount` or `MaxCardsOnHand` (M4.5, see §4.2).** The guarantee is subtractive — it consumes normal draw slots. Composition wins when budget cannot fit both. Implemented via `DeckManager.DrawCardsForPlayerTurn` + `DrawCardFiltered`.
10. **Pause is not a phase.** It never touches `CurrentGigPhase`, `_session` or the action-window flags; it
    freezes time, input (`PauseGate`), transport and ambience, and every exit restores all four in the same
    frame (see «Pause state»).

---

## 9. Migration note

This SSoT intentionally absorbs the **runtime-flow** truth from older design/runtime docs while leaving package-internal composer logic out of ALWTTT authority.
The detailed composition/playback bridge is further specified in:
- `runtime/SSoT_Runtime_CompositionSession_Integration.md`
- `integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md`

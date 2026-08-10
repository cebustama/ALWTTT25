# SSoT_Dev_Mode — ALWTTT

**Status:** Active governed SSoT
**Scope:** Dev Mode tooling for playtest iteration: scripting-define gating, overlay, infinite turns, convinced-audience reset, between-song hand reset, hand-visibility re-enable, runtime card spawning from catalogue, Breakdown entry point, gig-wide stat editing, per-character stat editing, status apply/remove picker, composition-debug tab (read + pattern overrides + instrument overrides).
**Owns:** Dev Mode compile-time gating, overlay interaction surface, infinite-turns semantics, runtime card spawn entry point, Breakdown force-trigger entry point, gig-wide stat-editing entry points (SongHype, Inspiration, BandCohesion), per-character stat-editing entry points (Stress, MaxStress, Composure, Vibe, MaxVibe, Flow), status apply/remove picker surface, composition-debug tab surface (per-track intent/resolved log, seed pin, infinite composition-loop toggle, per-track pattern overrides, per-track instrument overrides), Dev Mode entry points into gameplay managers, documented hand-visibility gap in production code.
**Does not own:** GigManager phase flow (`runtime/SSoT_Runtime_Flow.md`), CompositionSession boundary (`runtime/SSoT_Runtime_CompositionSession_Integration.md`), status system (`systems/SSoT_Status_Effects.md`), card authoring (`systems/SSoT_Card_Authoring_Contracts.md`).

---

## 1. Purpose

Dev Mode is the tooling layer that makes ALWTTT playtestable. Without it, the Combat MVP is technically complete but cannot be iterated on: every gig ends after one song, convinced audience members drop out of the turn loop, and runtime state cannot be nudged during play.

Phase 1 (2026-04-17) delivered the minimum unblocker for QA iteration: **infinite turns**, **convinced-audience reset**, and a **clean hand reset between song cycles**. Phase 2 (2026-04-20) added the first interactive runtime mutation: **arbitrary card spawning from the full catalogue into the active hand**. Phase 3.1 (2026-04-23) added the **Breakdown entry point**, closing the last deferred M1.2 validation test. Phase 3.2 (2026-04-23) added **gig-wide stat editing** (SongHype, Inspiration, BandCohesion) with session-aware Inspiration routing and symmetric `LoseGig` dispatch on Cohesion 0. Phase 3.3a (2026-04-23) added **per-character stat editing** (Stress, MaxStress, Composure on musicians; Vibe, MaxVibe on audience) and extended gig-wide editing with Flow. Phase 3.3b (2026-04-24) added the **status apply/remove picker**, closing the state-editing gap. Deferred phases (P3.4 audience transparency panel, encounter modifier toggles) build on the same infrastructure.

This SSoT is the primary authority for Dev Mode's compile-time gating, overlay behavior, runtime entry points, and known limitations.

---

## 2. Compile-time gating

Dev Mode uses a scripting define, not an assembly definition, to keep the patch surface minimal and to allow gradual coexistence with production code.

- **Scripting define:** `ALWTTT_DEV`. Set in Project Settings → Player → Other Settings → Scripting Define Symbols for the target build platform.
- **Guard style:** file-level `#if ALWTTT_DEV` at the top of dedicated Dev Mode files; block-level `#if ALWTTT_DEV ... #endif` around all Dev Mode hooks injected into production files.
- **No asmdef:** Dev Mode code lives alongside production code in the same assembly. The scripting define is the only boundary.
- **Namespace:** `ALWTTT.DevMode`.
- **Production builds:** omit `ALWTTT_DEV` from Scripting Define Symbols. All Dev Mode code and hooks disappear at compile time. No release-build leakage.

Verification: the presence of any `<color=lime>[DevMode]</color>` log line in the Console confirms Dev Mode is compiled. Absence of those lines in a Dev Mode code path means the define is not set.

---

## 3. Overlay

Dev Mode's interaction surface is an IMGUI overlay on the existing gig scene.

- **Entry point:** `DevModeController : MonoBehaviour`, singleton (`DevModeController.Instance`), placed on a persistent scene object.
- **Toggle:** `F12`. Overlay defaults to off.
- **Scale:** `_overlayScale` inspector field (`Range(1f, 4f)`, default `2.0f`). Applies a uniform `GUI.matrix` scale so IMGUI remains legible at modern resolutions.
- **Window size:** `480×380` (default). Grown in Phase 2 to accommodate the Catalogue list comfortably at any scale; Infinite tab has room to spare.
- **Tab toolbar (Phase 2+):** `GUILayout.Toolbar` at the top of the window switches between `Infinite`, `Catalogue`, `Stats`, `Audio Mix`, and `Composition` (DBG-C1). One draggable window, one toggle key, one scale — tab content switches in-place.
- **Verbose logging:** `_verboseLogs` inspector flag. When on, the overlay reports Hand/HandPile/Draw/Discard counts and the current `GigPhase`, and `OnPlayerTurnStartInfiniteMode` logs its call context.
- **Awake guard:** duplicate-instance destruction; `InfiniteTurnsEnabled` is reset on `OnDestroy` so it does not leak across scene reloads.

Tab content:

**Infinite tab (Phase 1):**
- Infinite Turns checkbox (see §4).
- "Reset Convinced Audience Now" button for manual reset (the automatic reset still runs on PlayerTurn).
- Auto-reset counter for the current gig.
- Song / Required counter, Cohesion, and (verbose) Hand/DrawPile/DiscardPile/HandPile counts + current phase.
- **Gig-outcome buttons (DEV-WINLOSE, 2026-07-16).** The Dev overlay's Infinite tab exposes **WIN** and **LOSE** buttons. They call `ALWTTT_DEV`-guarded wrappers on `GigManager`: `DevWinNormalFlow` → `WinGig` (opens `RewardCanvas` → `BuildReward(Card/Sfx)`), `DevLoseNormalFlow` → `LoseGig`, plus `DevForceWinImmediate`/`DevForceLoseImmediate` (`ReturnToMap`). These mirror the pre-existing `[ContextMenu]` debug outcome methods. **`DevGigOutcomeTracker` does NOT count these** — dev-forced outcomes bypass `GigOutcomeEvent` by design (win-rate stats reflect normal-flow gigs only). `WinGig` self-suppresses while Infinite Turns is enabled, so disable it before pressing WIN.

**Catalogue tab (Phase 2):**
- **Source (DEMO-FIXES-A, 2026-07-15, D-DF-7=A):** the list is the **runtime union of the current band's per-musician catalogs** — `PersistentGameplayData.BuildBandCardCatalog` walks `PD.MusicianList`, taking every distinct `CardDefinition` across each musician's `MusicianCharacterData.CardCatalog` (all acquisition flags; runtime read, no asset mutation). Adding a musician to the band needs no hand-wiring; out-of-roster musicians (e.g. Conito pre-onboarding) are excluded by construction, so out-of-spec catalogs never reach runtime through this path. Generic-catalog entries are not included (PD does not retain the `GigSetupRosterSO` after launch — same limit as `BuildRewardCardPool`). The header shows `Source: band union (N musicians)`, with a `↻` manual refresh (the union is cached by band count). **`GameplayData.AllCardsList` is now FALLBACK ONLY** (dev scenes with no band) and is deprecated as a hand-maintained catalogue. ST-DF-13 PASS.
- Text search + Action/Composition kind toggles (Deck-Editor parity filter set).
- Gate status line: "Ready. Hand: N/MAX (shown/total)" when spawn is allowed, otherwise "Spawn gated: <reason> ..." with the reason returned by `DeckManager.CanDevSpawnToHand(out reason)`.
- Scrollable card list — one row per filtered `CardDefinition`. Row shows kind badge (`[A]` / `[C]` / `[?]`), display name, cost, Spawn button. Spawn button routes through `DeckManager.DevSpawnCardToHand(def)`. See §11.
- Filter is cache-invalidated on dirty detection (source count, search string, toggle state); it does not reallocate every frame.

**Stats tab (Phase 3.1–3.3b):**
- Breakdown section (P3.1): musician selector grid (shows `{Name} [{Stress}/{MaxStress}]`, appends `(BD)` when `IsBreakdown` is true), stress/stun/status readout for selected musician, "Force Breakdown → {Name}" button.
- Status readout iterates `StatusEffectContainer.Active` and displays `{DisplayName}×{Stacks}` for each active entry.
- Gig-Wide Stats section (P3.2 + P3.3a): SongHype slider `[0, MaxSongHype]`, Inspiration slider `[0, MaxInspiration]` bound to `LiveInspiration` (session value when composing, PD value otherwise), BandCohesion stepper (`−`/`+`, floor 0, no upper cap), Flow stepper (`−`/`+`, uniform ±1 applied to every musician's `DamageUpFlat` stack; aggregate readout via `GigManager.TotalFlowStacks`). Sliders fire through `GigManager.DevSet…` wrappers. Slider idle-epsilon on SongHype (`0.01f`) avoids per-frame event spam. **S5f note (#15):** the Gig-Wide Stats SongHype **slider** routes through `DevSetSongHypeAbsolute` (guarded by `GigDevSettingsSO.debugSongHype`) and is unchanged — it was already gated. The separate `GigManager.DevAddSongHype` / `DevResetSongHype` wrappers (not surfaced by this slider) are compile-gated under `ALWTTT_DEV` as of S5f and stripped from non-dev builds.
- Per-Character section (P3.3a + P3.3b): musician selector grid (shares index with Breakdown section) + per-musician Stress slider, MaxStress stepper (floor 1), Composure stepper (backed by `TempShieldTurn` status stacks — see §14.3). Audience selector grid + per-audience Vibe slider, MaxVibe stepper (floor 1). All stat writes fire through `DevSet…` wrappers on the respective stats classes.
- Per-Character status editing (P3.3b): each character's subsection (musician, audience) includes a status picker below the stat controls. **Active readout:** lists all active statuses on the selected character (`{DisplayName} ×{Stacks}`) with `[−1]` (decrements via `container.Apply(def, -1)`, auto-clears at 0) and `[Clear]` (immediate full removal via `container.Clear(id)`) buttons per row. **Catalogue picker:** `[◄][►]` buttons cycle through non-null entries in the character's `StatusCatalogue.Effects`; selected entry shown as `{DisplayName} ({EffectId})`; `[+1]` button calls `container.Apply(selectedSO, 1)` with a lime `[DevMode]` log. No `DevSet…` wrappers needed — the existing `StatusEffectContainer` public API is sufficient. Falls back gracefully: "(no catalogue — assign on prefab)" when `StatusCatalogue` is null on the character.

**Audio Mix tab (M-AUDIO-MIX):**
- Global Music slider, one Per-Musician Music slider per spawned musician, and a Master SFX slider. All route through `GigManager.DevSet…` audio wrappers, which apply live and (in the editor) persist to `AudioMixSettingsSO` via `PersistAudioMixInEditor` (`#if UNITY_EDITOR` SetDirty/SaveAssets). Live mix works even with no asset wired — a hint banner reads "⚠ No AudioMixSettings wired — sliders work live but won't persist" (`GigManager.DevHasAudioMixAsset`); the SO is persistence/default only (D-MIX-FALLBACK=B).
- Highlight trigger (ST-AM-6 / future highlight mechanic): a musician picker (`[◄][►]`) + `Solo` / `Duck` / `Clear` buttons calling `MidiMusicManager.Highlight(musicianId, mode)` directly (`HighlightMode.Solo` / `DuckOthers` / `None`).
- The mix *model* (per-musician axis, effective-volume formula, persistence, boundary) is governed by `SSoT_Audio.md`; this tab is only the Dev surface.

**Composition tab (DBG-C1):**
- Read-only per-track composition debug + infinite composition-loop toggle + optional seed pin. Full semantics in §18.

---

## 4. Infinite Turns — runtime semantics

When `DevModeController.InfiniteTurnsEnabled` is true:

### 4.1 Gig never terminates by completion check
`GigManager.IsGigComplete` returns `false` unconditionally while Infinite Turns is on. The normal `CurrentSongIndex >= _requiredSongCount` check is bypassed.

### 4.2 Win/Lose suppression
Both `GigManager.WinGig` and `GigManager.LoseGig` early-return with a `[DevMode] … suppressed (infinite turns)` log. Cohesion and musician-health persistence are skipped; the gig keeps running. Normal flow resumes the moment Infinite Turns is toggled off (the next end-of-song will resolve via the normal path).

### 4.3 Convinced-audience auto-reset at PlayerTurn start
At `GigManager.ExecuteGigPhase(GigPhase.PlayerTurn)`, if Infinite Turns is on, `DevModeController.Instance.OnPlayerTurnStartInfiniteMode()` runs before musician status ticks. It iterates `GigManager.CurrentAudienceCharacterList` and calls `DevResetConvinced()` on any member whose `IsConvinced` is true. After any resets, `GigManager.RecalculateAudienceObstructions()` is invoked so seating/blocking reflects the un-convinced state.

`AudienceCharacterStats.DevResetConvinced()` performs the reset: `IsConvinced = false`, `SetCurrentVibe(0)`, `ClearStatus(StatusType.Convinced)` (legacy enum path retained for the status icon pipeline — see §7).

### 4.4 Between-song reset in `OnCompositionSessionEnded`
When a composition session ends and Infinite Turns is on, `GigManager.OnCompositionSessionEnded` runs three Dev Mode steps **in this order**:

1. **Diagnostic dump.** Logs `InfiniteTurnsEnabled`, `DeckManager.Instance` null-state, `IsGigComplete`, `skipAudienceActionsAfterFinalSong`, and `gigHand.activeSelf` before any flip.
2. **Hard hand reset.** `DeckManager.DevForceHandResetToDiscard()` destroys any `CardBase` GameObjects currently tracked in `HandController.Hand`, moves their `CardDefinition` entries from `HandPile` to `DiscardPile`, and sweeps stray `CardBase` children under `HandController.DrawTransform` that are not in the tracked hand. Returns the number of cards destroyed. Rationale: `CardBase.Discard()` gates on `IsPlayable` / `IsExhausted` and uses an async coroutine that can abandon GameObjects mid-flight when the scene transitions phases quickly. The hard reset bypasses this gate.
3. **Hand-visibility re-enable.** `SetHandVisible(true)` forces `gigHand.gameObject.SetActive(true)` and re-enables dragging. See §5.

After these three steps, control returns to the phase machine via `CurrentGigPhase = GigPhase.AudienceTurn`, and the normal gig loop continues.

---

## 5. Hand-visibility gap in production code — load-bearing fact

CompositionSession calls `ShowHand(false)` through `ICompositionContext` during session setup to hide the hand while the song is playing. In single-song gigs this is invisible: the gig ends at `OnCompositionSessionEnded` and the scene tears down.

In Infinite Turns, the gig does not end. The next PlayerTurn will call `DeckManager.DrawCards`, which instantiates `CardBase` GameObjects as children of `HandController.DrawTransform`. If `DrawTransform.activeInHierarchy` is false, the newly-instantiated cards are inactive and unusable — visually present as "ghost" sprites, not draggable, not playable.

Production code has no symmetric `ShowHand(true)` between song-end and the next PlayerTurn because the single-song path never needs one. Dev Mode Phase 1 bridges this gap explicitly in `OnCompositionSessionEnded` (see §4.4 step 3).

This is the load-bearing fact Phase 1 codified. If Infinite Turns is ever re-implemented from scratch, or if a non-Dev-Mode multi-song flow is ever added to production, the same gap must be bridged on that path.

**Phase 2 corollary.** `DeckManager.CanDevSpawnToHand` refuses to spawn while `HandController.DrawTransform.activeInHierarchy` is false. The spawn gate is the mirror of the Phase 1 bridge: it prevents re-entry of the ghost-card state that Phase 1 fixes on the reset path. See §11.

---

## 6. Entry points and modified surfaces

**New files:**
- `Assets/Scripts/DevMode/DevModeController.cs` — file-level `#if ALWTTT_DEV`. Singleton, overlay, tab toolbar, infinite-turns state, `OnPlayerTurnStartInfiniteMode`, `ResetConvincedAudience`.
- `Assets/Scripts/DevMode/DevCardCatalogueTab.cs` — file-level `#if ALWTTT_DEV`. Phase 2 static helper that renders the Catalogue tab body. Holds filter state, reads the catalogue via `PersistentGameplayData.BuildBandCardCatalog` (band union; `GameplayData.AllCardsList` fallback — DEMO-FIXES-A, D-DF-7=A), delegates spawn to `DeckManager.DevSpawnCardToHand`. No runtime mutation outside that delegation.
- `Assets/Scripts/DevMode/DevStatsTab.cs` — file-level `#if ALWTTT_DEV`. Phase 3.1/3.2/3.3a/3.3b static helper that renders the Stats tab body. Breakdown section (P3.1) + Gig-Wide Stats section (P3.2 + Flow row added P3.3a) + Per-Character section (P3.3a stat controls + P3.3b status picker). Dispatches to `GigManager.DevSet…`, `BandCharacterStats.DevSet…`, `AudienceCharacterStats.DevSet…` wrappers for stat editing, and directly to `StatusEffectContainer.Apply`/`Clear` for the Composure stepper and P3.3b status picker. Phase 3.3b additions: `DrawStatusPicker(CharacterBase, ref int)` method, `_musicianStatusPickerIndex` and `_audienceStatusPickerIndex` static fields, `using ALWTTT.Characters` directive.
- `Assets/Scripts/DevMode/DevAudioMixTab.cs` — file-level `#if ALWTTT_DEV`. M-AUDIO-MIX static helper rendering the Audio Mix tab body (global music + per-musician + master SFX sliders + a no-asset banner + the Solo/Duck/Clear highlight trigger). Routes all slider edits through `GigManager.DevSet…` audio wrappers; calls `MidiMusicManager.Highlight` directly for the trigger. No runtime mutation outside those calls.
- `GigManager` `#if ALWTTT_DEV` audio additions (M-AUDIO-MIX): `DevGlobalMusicVolume01`/`DevSetGlobalMusicVolume01`, `DevGetMusicianVolume01`/`DevSetMusicianVolume01(MusicianBase,float)`, `DevMasterSfxVolume01`/`DevSetMasterSfxVolume01`, `DevHasAudioMixAsset`, `PersistAudioMixInEditor`. Always-compiled support: `ApplyPersistedAudioMix` (StartGig), `ReapplyMusicianMix` (after Play), `_globalMusicVolume01`, the `audioMix` SO ref.
- `Assets/Scripts/DevMode/DevCompositionDebugTab.cs` — file-level `#if ALWTTT_DEV` (DBG-C1). Renders the Composition tab body: two-phase intent/resolved per-track log, optional seed field, infinite-loop toggle, Copy fingerprint, chd: dump. Read-only except the seed/toggle writes; no gameplay mutation. **(DBG-C2)** interactive controls added: per-track override dropdowns, Roman field, R2a re-render button, catalog browse. **(CSV-2, 2026-07-18)** per-track **instrument** override rows added (melodic + percussion pickers, `[dev-inst]` intent annotation, card-stomp detection, Clear-with-restore); the tab's `Clear ALL overrides` button now clears both override families. Still file-level `#if ALWTTT_DEV`. **(CTX-1/1b, 2026-07-31)** sección de override de contexto de parte (tonalidad/raíz) con Hold-across-loops y log de diagnóstico de drift; el botón `Clear ALL overrides` limpia ahora tres familias (patrón · instrumento · contexto de parte). **(CTX-2a, 2026-08-03)** sección de override de **tempo** (BPM) con Hold-across-loops y línea de lectura de BPM resuelto; el `Clear ALL overrides` limpia ahora **cuatro** familias (patrón · instrumento · contexto de parte · tempo). **(CTX-2b, 2026-08-03)** sección de **articulación** (`chordExpression` / `arpeggioRate`) sobre un plano nuevo —clon en runtime del style bundle, D-CTX2B-1=A— con Hold-across-loops de semántica estrechada; el `Clear ALL overrides` limpia ahora **cinco** familias (patrón · instrumento · contexto de parte · tempo · articulación). Sigue `#if ALWTTT_DEV` a nivel de fichero.
- `Assets/Scripts/DevMode/GenerationDebugFormatter.cs` — file-level `#if ALWTTT_DEV` (DBG-C1). Role-adaptive text formatter for the tab (intent lines, resolved lines with the `'*'` resolved-only convention, fingerprint block).

**Modified production files (block-level `#if ALWTTT_DEV` patches only):**
- `Assets/Scripts/Managers/GigManager.cs` — seven patches:
  1. `using ALWTTT.DevMode;` import.
  2. `IsGigComplete` returns false under Infinite Turns.
  3. `ExecuteGigPhase(GigPhase.PlayerTurn)` — completion-check bypass, diagnostic logs, `OnPlayerTurnStartInfiniteMode` invocation.
  4. `WinGig` and `LoseGig` — early-return suppression with log.
  5. `OnCompositionSessionEnded` — diagnostic dump, `DevForceHandResetToDiscard`, `SetHandVisible(true)`.
  6. Phase 3.2 block after `AddSongHype`: `MaxSongHype` getter, `LiveInspiration` getter, `DevSetSongHype(float)`, `DevSetInspiration(int)` (routes to `CompositionSession.DevSetCurrentInspiration` when session is active), `DevSetBandCohesion(int)` (dispatches `LoseGig()` on 0; Infinite-Turns suppression inherited from `LoseGig`).
  7. Phase 3.3a additions to the same `#if ALWTTT_DEV` block: `TotalFlowStacks` getter (public wrapper over `GetTotalFlowStacks`), `DevAddFlowToAllMusicians(int delta)` — resolves the `"flow"` SO from the first available musician catalogue and applies the delta to every musician's `StatusEffectContainer`; pre-guards `Apply(-N)` on zero-stack containers to avoid spurious `OnStatusCleared` events.
- `Assets/Scripts/Managers/DeckManager.cs`:
  - `using ALWTTT.Enums;` (unconditional — needed by `CanDevSpawnToHand` for the `GigPhase` reference; free in production builds).
  - `DevForceHandResetToDiscard()` method (Phase 1).
  - `DevSpawnCardToHand(CardDefinition) : bool` method (Phase 2) — runtime spawn entry point. Gated by `CanDevSpawnToHand` (see §11).
  - `CanDevSpawnToHand() : bool` and `CanDevSpawnToHand(out string reason) : bool` methods (Phase 2) — centralized gate predicate.
  - `DrawCards` entry — optional diagnostic log dumping `HandController.gameObject` and `DrawTransform` active-state at draw-time entry (kept for future debugging).
- `Assets/Scripts/Characters/Audience/AudienceCharacterStats.cs` — Phase 3.1 surface: `DevResetConvinced()` method (implementation landed in P3.3a — previously doc-declared but unimplemented; see §7). Phase 3.3a surface: `CheckConvincedThreshold()` private helper extracted from `AddVibe`, `DevSetCurrentVibe(int)`, `DevSetMaxVibe(int)` — all route through the shared threshold-check so Dev and play paths cannot drift.
- `Assets/Scripts/Characters/Band/BandCharacterStats.cs` — Phase 3.1 surface: `DevResetBreakdown()` method. Sets `IsBreakdown = false` so `AddStress` can re-trigger the Breakdown path. Phase 3.3a surface: `CheckBreakdownThreshold()` private helper extracted from `AddStress`, `DevSetCurrentStress(int)`, `DevSetMaxStress(int)` (floor 1, clamps Current down, re-checks threshold). Dev Mode only; production code never un-breaks a musician.
- `Assets/Scripts/Characters/Band/MusicianBase.cs` — `DevForceBreakdown()` method. Calls `DevResetBreakdown()` then `AddStress(MaxStress)`. Routes through the natural Breakdown path (Cohesion−1, Stress reset, Shaken apply, IsStunned). Re-triggerable.
- `Assets/Scripts/Music/CompositionSession.cs` — Phase 3.2 block: `CurrentInspiration` getter + `DevSetCurrentInspiration(int)` method. Sets the session's live `_currentInspiration` field and calls `_ctx.CompositionUI?.SetInspiration(value)` to refresh the composition UI. Does not write back to `PersistentGameplayData` — caller (`GigManager.DevSetInspiration`) owns that side.
- `Assets/Scripts/Music/CompositionSession.cs` — S5g seed-wiring addition (2026-07-05): `DevPinnedSongSeed` (`static int?`). When non-null, `Begin()` uses it in place of run entropy to seed `_songSeed`, producing a reproducible song render (see `SSoT_Runtime_CompositionSession_Integration.md §10`). **DBG-C1** additions: `DevInfiniteCompositionLoop` (`static bool`, dev-only) consumed by the countdown-reset branch in `HandleLoopFinished` and the `IsFinalLoopRunning` dev exemption; read-only accessors (`DevCurrentPartIndex`, `DevLoopsRemaining/TotalForPart`, `DevSongSeed`, `DevCompositionUI`); song-boundary reset of the toggle in `Begin()`/`End()`. **(DBG-C2)** dev statics `DevPatternOverrides` / `DevOverrideStamp` / `DevBumpOverrideStamp()`, accessor `DevMidiConfig`, `PartCache.devOverrideStamp` field; stamp-invalidation + `patternOverrides` pass-through in `PlaySinglePartLoop`; song-boundary clear of `DevPatternOverrides`. **(CSV-2, 2026-07-18)** two further dev-region additions: `DevResolveMusicianById(string) : MusicianBase` (forwards `ICompositionContext.ResolveMusicianById`, needed by the tab to compute `InstrumentRules.GetPermittedMelodic` for the permitted-set annotation) and `DevInvalidateForInstrumentOverride(int partIndex)` (calls `InvalidatePartCache(partIndex, keepTempo: true, keepInstrumentsOverride: **false**)` then `DevBumpOverrideStamp()`). The `keepInstruments: false` choice is load-bearing and is **not** the pattern-override stamp path — see §18.9 and `SSoT_Runtime_CompositionSession_Integration §8` inv 9. All `#if ALWTTT_DEV` except the null-passed local (production byte-identical).
- `Assets/Scripts/Managers/MidiMusicManager.cs` — **DBG-C1** read-only truth surface (`LastResolvedByTrack`/`LastPinnedByTrack`/`LastRenderSerial|PartIndex|Bpm|FromCache`) + `GetChordTimelineSnapshot()`/`ChordTimelineEntry`. Production API (only the consuming tab is dev-gated). **(DBG-C2)** cache bypass when `patternOverrides` is supplied (production API; the value is null in production).
- `Assets/Scripts/Data/Gig/GigDevSettingsSO.cs` — **DBG-C1** `CompositionDebugFull` flag (Compact/Full tab format).
- `Assets/Scripts/Managers/GigManager.cs` — **DBG-C1** `DevSettings` accessor (`#if ALWTTT_DEV`).

**Not modified by CSV-2 (deliberate, D-CSV-5=A).** `SongCompositionUI`, `SongConfigBuilder`, and `MidiMusicManager` are untouched by the instrument-override surface. The dev write reuses the `TrackEntry.override*Instrument` fields those files already own, so the existing override precedence in `SongConfigBuilder.FromUI` and the existing `trackInputsHash` participation apply unchanged. The tool adds no new production API.

All Dev Mode hooks are `#if ALWTTT_DEV`-guarded. No production behavior change when the define is absent.

Phase 3.3b does not add any new production-class entry points. The status picker operates entirely through the existing `StatusEffectContainer.Apply`/`Clear` public API and `CharacterBase.StatusCatalogue` property. No new `DevSet…` wrappers on gameplay classes.

---

## 7. Legacy `StatusType.Convinced` coexistence

`DevResetConvinced` clears the legacy `StatusType.Convinced` enum status in addition to flipping `IsConvinced`. This is deliberate. The SO-based catalogue is the authority going forward (see `SSoT_Status_Effects.md` §3.3), but some icon and UI callers still consult the legacy `statusDict`. Until those call sites migrate, Dev Mode must keep both views in sync. No new work should rely on the legacy enum.

---

## 8. Caveats and known limitations

### 8.1 Direct Gig Scene entry path (deferred)
Starting the Gig Scene directly — bypassing `GigSetupScene` — can reproduce ghost-card behavior under Infinite Turns that does not occur via the normal entry path. Cause unverified; likely related to `SetupDeck` initialization ordering when `GigSetupController` has not run. **Out of scope for Phase 1.** Dev Mode is intended to be entered via the normal scene-entry flow. If direct-start becomes needed (e.g. for editor-driven tests), a separate investigation is required.

### 8.2 `CardBase` GameObject accumulation under `DiscardPos` (cosmetic, deferred)
`CardBase.Discard()` reparents GameObjects to `DiscardPos` rather than destroying them. In single-song gigs this is invisible because scene teardown sweeps them. In Infinite Turns, over many cycles, `DiscardPos` accumulates orphaned GameObjects. Phase 2 worsens this cosmetically because spawning is the fastest way to create+discard many cards in a session. Gameplay state (`HandPile`, `DiscardPile`, `Hand`) remains correct. **Not fixed in Phase 2.** Separate ticket; candidate for destruction-on-Discard refactor later.

### 8.3 `GigPhase.SongPerformance` appears unused in logs
Neither the Phase 1 smoke runs nor historical runs show `Executing gig phase: SongPerformance`. The song plays inside `PlayerTurn` via `CompositionSession` ticks in `Update`, and `CurrentSongIndex` is updated on `OnCompositionSongFinished`. If `SongPerformance` is dead code, that is a separate runtime cleanup — out of Dev Mode scope.

### 8.4 `CardActionTiming` on debug cards (Phase 2)
Action cards without an explicit `CardActionTiming` inherit a default that does not permit play during the current PlayerTurn. Because Dev Mode spawn is PlayerTurn-gated (see §11), a debug action card whose timing excludes PlayerTurn cannot be spawned and played in one session — producing a `Cannot play action card '<n>' in current timing. Returning to hand.` log.

Any new debug card authored for Dev Mode use should set `actionTiming: "Always"` (or an explicit timing that includes PlayerTurn) in its JSON import. This was the root cause of Phase 2's T7 deferral (see §9).

### 8.5 Spawn accepted pollution (Phase 2 decision U1)
Cards spawned via `DevSpawnCardToHand` are added to `HandPile` only. On play they route through the normal `OnCardDiscarded` path to `DiscardPile`. After one reshuffle cycle the spawned card enters `DrawPile` organically — at which point it is indistinguishable from an originally-drawn card.

This is **accepted pollution**, decided 2026-04-20 in the Phase 2 sub-roadmap. The tradeoff is simplicity (one code path, identical to `DrawCards`) vs. ephemerality (requires a parallel tracked-list in `DeckManager`). If tests need ephemeral behavior, add a Phase 3 override; do not branch the normal path.

### 8.6 `test_pass_turn` composition card obsolete
`test_pass_turn` was authored 2026-03-23 as a `CompositionCardPayload` with no effects and `PrimaryKind = None`, intended to provide a zero-cost "pass turn" in the composition phase. The current `SongCompositionUI.ApplyCardToPart` rejects `PrimaryKind == None` explicitly (`"unsupported PrimaryKind 'None'"` → returns false), so the card cannot legally advance a composition.

The asset was removed on 2026-04-20 during Phase 2 closure (see changelog-ssot). ST-P2-4 was re-validated using the `Waltz` composition card from `TestDeck_FullCoverage`, which produces the same runtime surface (composition card spawned via DevSpawn, dropped in a part zone, successfully queued).

If a "no-op composition card" concept is ever needed again, it requires an explicit design decision to extend `ApplyCardToPart` to accept `PrimaryKind == None` with no modifier effects. Not scheduled.

### 8.7 `DevPinnedSongSeed` overlay control — RESOLVED (DBG-C1, 2026-07-17)
`CompositionSession.DevPinnedSongSeed` (see §6) is now wired to a seed field in the **Composition tab** (D-C1(seed)=A; §18). Set it before a song starts (read in `Begin()`) for a reproducible render — the BC-gate byte-diff (ST-S1) uses it. The prior code/debugger-only limitation is closed. (If §16 lists the seed-tab-wiring backlog item, mark it delivered by DBG-C1.)

---

## 9. Smoke-test coverage

### 9.1 Phase 1 (GigSetupScene → Gig Scene normal entry)

| Test | Subject | Result |
|---|---|---|
| ST-3 | Song transitions under Infinite Turns — no ghost cards, `DrawTransform.activeInHierarchy=True` at draw entry | ✅ PASS 2026-04-17 |
| ST-4 | Convinced-audience auto-reset at PlayerTurn start | ✅ PASS 2026-04-17 |
| ST-5 | Win/Lose suppression under Infinite Turns | ✅ PASS 2026-04-17 |
| ST-7 | Overlay telemetry tracks hand/pile counts and phase across cycles | ✅ PASS 2026-04-17 |
| Regression | Normal single-song gig unaffected when Infinite Turns is off and overlay is closed | ✅ PASS 2026-04-17 |

### 9.2 Phase 2 (card spawner)

| Test | Subject | Result |
|---|---|---|
| ST-P2-1 | PlayerTurn gate — spawn disabled outside PlayerTurn, reason surfaced in gate status line | ✅ PASS 2026-04-20 |
| ST-P2-2 | Max-hand gate — spawn disabled at `HandPile.Count >= MaxCardsOnHand` | ✅ PASS 2026-04-20 |
| ST-P2-3 | Normal play pipeline — spawned action card plays, applies effects, routes to DiscardPile | ✅ PASS 2026-04-20 |
| ST-P2-4 | Composition card spawning — spawned composition card queues into composition session correctly | ✅ PASS 2026-04-20 (validated with `Waltz`; `test_pass_turn` obsolete — see §8.6) |
| ST-P2-5 | Filter correctness — search is case-insensitive on DisplayName; kind toggles live-update | ✅ PASS 2026-04-20 |
| ST-P2-6 | Infinite-mode regression — Phase 1 behavior preserved when Phase 2 is active | ✅ PASS 2026-04-20 |
| ST-P2-7 | Production-build strip — `ALWTTT_DEV` removed → clean compile, no residual symbols | ✅ PASS 2026-04-20 |

### 9.3 Multi-turn status tests deferred from M1.2 (runnable against Phase 2)

| Test | Subject | Result |
|---|---|---|
| T5 | Choke decay at `PlayerTurnStart` tick | ✅ PASS 2026-04-20. Bonus finding: stunned targets reject further targeting via `HandController.TryResolveCardTarget`, indirectly confirming `CharacterBase.IsStunned` tracks `DisableActions` stacks. |
| T7 | Shaken expiry across a song cycle | ✅ PASS 2026-04-23 (Phase 3.1). `DevForceBreakdown` applied Shaken; status decayed and cleared at expected tick boundary. Icon removed on expiry. Musician stun cleared. |
| T8 | Feedback DoT accumulation across `AudienceTurnRoutine` | ✅ PASS 2026-04-20. Each stack applies +1 stress per AudienceTurn via `ApplyIncomingStressWithComposure`; stacks persist turn-to-turn. Finding: Feedback has no tick decay — by design; the SO is configured with `DecayMode.None`. `SSoT_Status_Effects.md` §5.6 does not document this explicitly and should be updated (tracked as an open doc gap). |

All three deferred M1.2 tests now closed. No remaining multi-turn validation gap.

### 9.4 Phase 3.1 (Breakdown entry point)

| Test | Subject | Result |
|---|---|---|
| ST-P31-1 | Force Breakdown on healthy musician — Stress jumps to max then resets to fraction, Shaken icon appears, Cohesion drops by 1, musician stunned | ✅ PASS 2026-04-23 |
| ST-P31-2 | Re-trigger Breakdown on already-broken-down musician — Cohesion drops again, Shaken re-applies | ✅ PASS 2026-04-23 |
| ST-P31-3 | Force Breakdown triggers LoseGig at Cohesion 0 (Infinite Turns OFF) | ✅ PASS 2026-04-23 |
| ST-P31-4 | Force Breakdown suppresses LoseGig (Infinite Turns ON) — gig continues | ✅ PASS 2026-04-23 |
| ST-P31-5 | Musician selector updates dynamically — (BD) tag only on broken-down musician | ✅ PASS 2026-04-23 |
| ST-P31-6 | Status readout matches active StatusEffectContainer entries | ✅ PASS 2026-04-23 |

### 9.5 Phase 3.2 (gig-wide stat editing)

| Test | Subject | Result |
|---|---|---|
| ST-P32-1 | SongHype slider affects `_songHype` and audience beat intensity live | ✅ PASS 2026-04-23 |
| ST-P32-2 | Inspiration slider clamps to `MaxInspiration` and updates composition UI live | ⚠️ Retroactively invalidated 2026-05-08 — see §9.10 ST-MB3-4 |
| ST-P32-3 | Scrubbing Inspiration unblocks a previously-uncastable cost-1 composition card | ⚠️ Retroactively invalidated 2026-05-08 — see §9.10 ST-MB3-4 / ST-MB3-8 |
| ST-P32-4 | BandCohesion stepper to 0 dispatches `LoseGig` (Infinite Turns OFF) | ⚠️ Retroactively invalidated 2026-04-24 — see §9.8 ST-MB1-1 |
| ST-P32-5 | BandCohesion stepper to 0 is suppressed (Infinite Turns ON) | ⚠️ Retroactively invalidated 2026-04-24 — see §9.8 ST-MB1-2 |
| ST-P32-6 | P3.1 Breakdown tests still pass (regression) | ✅ PASS 2026-04-23 |
| ST-P32-7 | Inspiration slider outside composition session writes PD, no crash | ✅ PASS 2026-04-23 |

### 9.6 Phase 3.3a (per-character stat editing + Flow gig-wide)

| Test | Subject | Result |
|---|---|---|
| ST-P33a-1 | Musician Stress slider → Max triggers Breakdown + Shaken (visual bar syncs via 0.1f tween) | ✅ PASS 2026-04-23 |
| ST-P33a-2 | Stress slider down after Breakdown — sticky state preserved (regression) | ✅ PASS 2026-04-23 |
| ST-P33a-3 | MaxStress stepper reduced to CurrentStress — Breakdown fires at boundary | ✅ PASS 2026-04-23 |
| ST-P33a-4 | MaxStress stepper floored at 1 | ✅ PASS 2026-04-23 |
| ST-P33a-5 | Composure stepper + applies `TempShieldTurn` stacks; icon appears; incoming Stress absorbs correctly | ✅ PASS 2026-04-23 |
| ST-P33a-6 | Composure stepper `−` disabled at 0 (no spurious `OnStatusCleared`) | ✅ PASS 2026-04-23 |
| ST-P33a-7 | Audience Vibe slider → Max triggers Convinced + status applied + Tall cleared | ✅ PASS 2026-04-23 |
| ST-P33a-8 | Audience MaxVibe reduced to CurrentVibe — Convinced fires at boundary | ✅ PASS 2026-04-23 |
| ST-P33a-9 | Flow gig-wide ± applies uniform delta to every musician; aggregate = sum | ✅ PASS 2026-04-23 |
| ST-P33a-10 | Song-end reset clears Flow and Composure (regression) | ✅ PASS 2026-04-23 |

### 9.7 Phase 3.3b (status apply/remove picker)

| Test | Subject | Result |
|---|---|---|
| ST-P33b-1 | Apply status to musician — icon appears, readout shows ×1, lime log | ✅ PASS 2026-04-24 |
| ST-P33b-2 | Stack status on musician — readout increments, no duplicate icon | ✅ PASS 2026-04-24 |
| ST-P33b-3 | Decrement via −1 — stacks decrease by 1, icon persists | ✅ PASS 2026-04-24 |
| ST-P33b-4 | Decrement to zero — auto-clear, icon removed with disappear animation | ✅ PASS 2026-04-24 |
| ST-P33b-5 | Clear button removes status entirely regardless of stack count | ✅ PASS 2026-04-24 |
| ST-P33b-6 | Apply status to audience member — icon appears, readout shows entry | ✅ PASS 2026-04-24 |
| ST-P33b-7 | Picker navigation wraps around (last → first, first → last) | ✅ PASS 2026-04-24 |
| ST-P33b-8 | No catalogue on character — active readout still works, apply section shows fallback label | ✅ PASS 2026-04-24 |
| ST-P33b-9 | Regression — Composure stepper still works, TempShieldTurn visible in active readout | ✅ PASS 2026-04-24 |
| ST-P33b-10 | Regression — Breakdown section status readout consistent with Per-Character active readout | ✅ PASS 2026-04-24 |

### 9.8 MB1 — DevSetBandCohesion dispatch alignment (2026-04-24)

Retroactive correction of §9.5. On 2026-04-24 it was discovered that `GigManager.DevSetBandCohesion` never contained the `LoseGig()` dispatch described in §13.2/§13.3, despite ST-P32-4 and ST-P32-5 being recorded as PASS on 2026-04-23. Those two entries were not honest observations. MB1 added the one-line dispatch + corrected the stale XML comment. Tests re-run under the corrected code:

| Test | Subject | Result |
|---|---|---|
| ST-MB1-1 | BandCohesion stepper to 0 dispatches `LoseGig` (Infinite Turns OFF) — loss panel appears | ✅ PASS 2026-04-24 |
| ST-MB1-2 | BandCohesion stepper to 0 is suppressed under Infinite Turns ON — lime log, gig continues | ✅ PASS 2026-04-24 |
| ST-MB1-3 | Cohesion up/down without hitting 0 — no `LoseGig` dispatch (regression) | ✅ PASS 2026-04-24 |
| ST-MB1-4 | Production build compiles with `ALWTTT_DEV` undefined; natural Breakdown → LoseGig path unchanged | ✅ PASS 2026-04-24 |

### 9.9 MB2 — catalogue split (2026-04-24)

Resolves §15.4 finding. Original `StatusEffectCatalogueSO.asset` split into `StatusEffectCatalogue_Musicians.asset` (6 canonical statuses: flow, composure, choke, shaken, exposed, feedback) and `StatusEffectCatalogue_Audience.asset` (empty at MVP; populated at M4.3 with Earworm). Musician and audience prefabs reassigned. No code change.

| Test | Subject | Result |
|---|---|---|
| ST-MB2-1 | Musician status picker lists exactly 6 canonical entries with wrap-around | ✅ PASS 2026-04-24 |
| ST-MB2-2 | Audience status picker shows graceful empty-catalogue fallback; no crash | ✅ PASS 2026-04-24 |
| ST-MB2-3 | Regression — Force Breakdown still applies Shaken (musician catalogue contains `shaken`) | ✅ PASS 2026-04-24 |
| ST-MB2-4 | Regression — `DevAddFlowToAllMusicians` still resolves `flow` key from musician catalogue | ✅ PASS 2026-04-24 |
| ST-MB2-5 | Regression — Feedback DoT applies and ticks (musician catalogue contains `feedback`) | ✅ PASS 2026-04-24 |
| ST-MB2-6 | No missing-reference warnings after scene reload — all prefab catalogue fields bound | ✅ PASS 2026-04-24 |

### 9.10 MB3 — Inspiration Dev surface drift correction + session-start carry-over (2026-05-08)

Retroactive correction of §9.5 ST-P32-2 / ST-P32-3. On 2026-05-08 MB3
discovered that `GigManager.LiveInspiration`, `GigManager.DevSetInspiration`
session routing, `CompositionSession.CurrentInspiration`, and
`CompositionSession.DevSetCurrentInspiration` had never been implemented
in code, despite all four being documented in §13.2 since 2026-04-23.
ST-P32-2 and ST-P32-3 could not have exercised the documented routing.
MB3 implements all four surfaces and adds a session-start carry-over
semantic for `JamRules.inspirationPerPart == 0` (D3). Tests run under
the corrected code:

| Test | Subject | Result |
|---|---|---|
| ST-MB3-1 | LiveInspiration returns PD value when no CompositionSession is active | ✅ PASS 2026-05-08 |
| ST-MB3-2 | LiveInspiration returns session value (not PD) during active session with forced divergence | ✅ PASS 2026-05-08 |
| ST-MB3-3 | DevSetInspiration with no active session writes pd only; sessionRouted=N | ⚠️ INVALID 2026-05-08 — test specified an unreachable precondition. CompositionSession is alive for the entire PlayerTurn (Begin to End), so "no active session at gig start" cannot occur during runtime. The Dev wrapper correctly reports `sessionRouted=Y` throughout the gig. Dev Mode is not exercised between gigs in current usage; no replacement test scheduled. See §13.4 lifecycle clarification. |
| ST-MB3-4 | DevSetInspiration with active session writes both pd and session; comp UI repaints; cost-gate respects new value; sessionRouted=Y | ✅ PASS 2026-05-08 |
| ST-MB3-5 | inspirationPerPart=0 + Begin → carry-over from pd.CurrentInspiration (D3) | ⏸ DEFERRED — depends on loop-game-flow milestone |
| ST-MB3-6 | inspirationPerPart=3 + Begin → reset to rules value (regression) | ⏸ DEFERRED — same |
| ST-MB3-7 | inspirationPerPart=0 + AdvanceToNextPart → carry-over preserved | ⏸ DEFERRED — same |
| ST-MB3-8 | ST-F3-S4c regression — Dev slider responsiveness during active session is now PASS | ✅ PASS 2026-05-08 |

### 9.11 MB4 — Action-card inspiration session routing (2026-05-08)

Closes user-reported critical bug "action cards are NOT consuming
Inspiration" (visible counter on composition UI did not move on action
play). Root cause: `CardBase.SpendInspiration` and
`CardBase.GenerateInspiration` wrote `pd.CurrentInspiration` directly,
bypassing `_session._currentInspiration` and the comp UI. This is the
symmetric dual-siting that F-3 closed for comp cards / per-loop gain
but never extended to action cards.

MB4 adds a public `GigManager.AdjustInspiration(int delta)` wrapper
that delegates to `CompositionSession.AddCurrentInspiration` when a
session is active and writes PD directly otherwise (clamped to
`[0, pd.MaxInspiration]`). Both `CardBase.SpendInspiration` (now passing
a negative delta) and `CardBase.GenerateInspiration` (passing positive)
route through this wrapper. PD ↔ session ↔ comp UI now stay in sync
across action, SFX, comp-card, per-loop-gain, and Dev paths.

MB4-diag adds a `GigManager.IsCompositionSessionActive` getter
(Dev-only) and a raw `[PD/Session]` readout below the Stats-tab
Inspiration slider, so dual-siting divergence (or convergence) is
always visible during diagnostic runs.

| Test | Subject | Result |
|---|---|---|
| ST-MB4-1 | Action card spend mirrors pd and _session; comp UI updates | ✅ PASS 2026-05-08 |
| ST-MB4-2 | Action / SFX card generate mirrors pd and _session; clamps at MaxInspiration | ✅ PASS 2026-05-08 |
| ST-MB4-3 | Over-spend clamps at 0 instead of going negative | ✅ PASS 2026-05-08 |
| ST-MB4-4 | Comp-card spend path unaffected — pd unchanged, session decrements (build-phase divergence preserved as §13.4 caveat); verified via raw readout | ✅ PASS 2026-05-08 |
| ST-MB4-5 | F-3 per-loop gain regression — pd ↔ session mirror unchanged; verified via raw readout | ✅ PASS 2026-05-08 |

### 9.12 TLM-1 — run telemetry logger (2026-07-16)

Smoke set for `DevRunTelemetryLogger` (§17). All run through normal gameplay in an `ALWTTT_DEV` build (records only write on the normal-flow `GigOutcomeEvent`).

| Test | Subject | Result |
|---|---|---|
| ST-TLM-1 | Win record — normal-flow win writes exactly one JSONL line: `won:true`, empty `lossCause`, `songsCompleted == requiredSongCount`, all audience `convinced:true`/`endVibe:0`, roster + encounterLabel populated, `playCounts` sum == `plays` length; Stats-tab "last gig written to" line appears | ✅ PASS 2026-07-16 |
| ST-TLM-2 | Loss record (unconvinced) — finishing required songs with ≥1 unconvinced member writes one line: `won:false`, `lossCause:"unconvinced_after_final_song"`, unconvinced member(s) show `endVibe > 0`/`convinced:false` matching the on-screen state at loss | ✅ PASS 2026-07-16 |
| ST-TLM-3 | Song-index correctness (confound guard) — in a multi-song gig, a card played in song 1 carries `songIndex:0` and a different card in song 2 carries `songIndex:1`; play order preserved; composition **and** action cards both present in `plays[]` (unified funnel) | ✅ PASS 2026-07-16 |
| ST-TLM-4 | Production strip — building without `ALWTTT_DEV` compiles clean, no `DevRunTelemetryLogger` symbol, no `DevTelemetry` directory created at runtime | ✅ PASS 2026-07-16 |
| ST-TLM-R1 | Regression — gig outcome, loss-panel text, session tally, and reward flow are identical with the logger active vs. the pre-TLM-1 build; the only observable deltas are the log line, the file, and the Stats-tab line (logger publishes nothing, mutates nothing) | ✅ PASS 2026-07-16 |

**Deferred (documented, not a gap in the above):** a cohesion-collapse loss (`DevSetBandCohesion` → 0) deliberately produces **no** record under D-TLM-3=A — verified by absence; a record appearing there would indicate an unexpected `GigOutcomeEvent` publish path and would itself be a failure. See §17.3.

### 9.13 DBG-C1 — composition-debug read surface + MusicianTrackKey migration (2026-07-17)

Consumer read-side half of MGP-ALWTTT-DBG (§18.1–§18.3). All PASS 2026-07-17. ST-S1 is the BC gate (single-track byte-identical under dev OFF + same seed); ST-S2 is the BASS-1-retirement positive (`cacheEnabled=True` with per-role hashes for a multi-track musician; bundle key `…@@2:Backing#…,2:Melody#…`). ST-S1..S10 cover the migration end-to-end, the read-only truth surface / serial polling, the `chd:` dump, the seed pin, the infinite-loop toggle (host hooks keep firing), the CARD-UX-1 dev exemption, the Copy fingerprint, and the production strip (ST-S10, zero footprint).

### 9.14 DBG-C2 — composition-debug interactive controls (2026-07-17)

Write/interactive half (§18.4–§18.8). All PASS 2026-07-17. ST-C2-1 override RenderOverride path (audible; DBG-OBS-1 note); ST-C2-2 Bassline veto (warn+ignore); ST-C2-3 Roman → Backing override, chd: matches; ST-C2-4 hard-fail applies nothing, verdict shown; ST-C2-5 dropdown pick ≡ free-entry render; ST-C2-6 R2a re-render bit-reproducible under pinned seed; **ST-C2-7 = BC gate** (dev OFF or all controls idle ⇒ byte-identical); **ST-C2-8 = clear/restore regression** (D-C2-4); **ST-C2-9 = production compile**, zero tab footprint.

### 9.15 CSV-1 / CSV-2 — composition inventory window + dev instrument overrides (2026-07-18)

Inventory window surface is documented in `SSoT_Editor_Authoring_Tools.md §17`; the instrument-override surface in §18.9. **All 8 PASS 2026-07-18.**

| ID | Test | Result |
| --- | --- | --- |
| **ST-CSV-1** | **BC gate** — `ALWTTT_DEV` build, seed pinned, no override ever touched: two runs of the same song produce identical Copy-fingerprints | PASS |
| **ST-CSV-2** | Melodic override applies — picking a different melodic instrument for the Melody track changes that track's voice at the next loop, fresh render, `[dev-inst]` shown, other tracks unaffected | PASS |
| **ST-CSV-3** | **Regression — clear/restore byte-identical** — fingerprint after `Clear` equals the pre-override fingerprint under a pinned seed (session-pin restore path, §18.9) | PASS |
| **ST-CSV-4** | Percussion override on Rhythm — drum voice changes, melodic tracks unaffected (family routing correct) | PASS |
| **ST-CSV-5** | Card stomp detection — an `InstrumentEffect` card on an already-dev-overridden track flips the row to `(card)` + superseded notice, drops the dev record, offers no stale restore | PASS |
| **ST-CSV-6** | Outside-permitted probing — full catalogue listed, non-permitted entries annotated, still selectable, and they render | PASS |
| **ST-CSV-7** | **Inventory window is inert** — open, cycle all seven views, Print, Export JSON ⇒ zero asset/meta modifications in VCS | PASS |
| **ST-CSV-8** | **Production compile** — build without `ALWTTT_DEV` compiles clean; window and instrument-override code absent | PASS |

**Not covered (deferred).** Pin behavior when one musician holds two melodic roles and only one is overridden — not reachable in current content; defer to CSV-3 or to a content layout that produces it.

### 9.16 CSV-3 — R2a card debug-play + resolved-identity read line (2026-07-22)

`DevInjectCompositionCard` (musical side only, live model, economy-neutral) + the `LastRenderResolved*` read line (§18.10/§18.1). **All PASS 2026-07-22.**

| ID | Test | Result |
| --- | --- | --- |
| **ST-CSV3-1** | **BC gate** — dev OFF ⇒ byte-identical | PASS |
| **ST-CSV3-2** | **BC gate** — all controls idle ⇒ byte-identical | PASS |
| **ST-CSV3-3** | Injection applies the card's musical side (primary action + `modifierEffects`) via the shared `ApplyCardDefinitionToPart` core | PASS |
| **ST-CSV3-4** | Injection **skips** inspiration check/spend, the `InspirationGenerated` one-shot, and `CardPayload.effects` | PASS |
| **ST-CSV3-5** | Economy-neutral audition — injected track excluded from `EvalPerLoopInsp`; per-loop inspiration bonus does not enter the run economy | PASS |
| **ST-CSV3-5b** | Reclaim — a genuine play on the same `(musicianId, role)` clears the audition-only mark | PASS |
| **ST-CSV3-5c** | Boundary clear — the audition key set clears at song boundary | PASS |
| **ST-CSV3-6** | C2a healthy — Core Minor aligns the part to Aeolian (`ChordTrack` step-2b), tonalities authored | PASS |
| **ST-CSV3-7** | Resolved-identity read line publishes TS/Tonality/Root actually used; alignment annotated | PASS |
| **ST-CSV3-8** | Overlay outer-scroll — Composition tab reachable past the screen bottom | PASS |
| **ST-CSV3-9** | **Production compile** — build without `ALWTTT_DEV` compiles clean; inject path + read line absent | PASS |

---


### 9.17 CTX-1 / CTX-1b — part-context override (2026-07-31)

| ID | Test | Estado |
|---|---|---|
| **ST-CTX-1** | Apply básico — parte Ionian/C → override Aeolian/A; resolved muestra el modo pedido y todos los tracks suenan en menor | PENDIENTE |
| **ST-CTX-2** | Reproducibilidad — seed pineado, dos re-renders del mismo override ⇒ fingerprint idéntico (verifica que `hasExplicitRootNote` fija la raíz) | PENDIENTE |
| **ST-CTX-3** | Regresión clear/restore — fingerprint post-Clear == pre-Apply bajo seed pineado | PENDIENTE |
| **ST-CTX-4** | Realineación — Backing = `Prog_Min_Andaluza` (tonalities=Aeolian) + petición Lydian ⇒ `aligned from intent Lydian`, sin crash | PENDIENTE |
| **ST-CTX-5** | Pisado por carta (Hold OFF) — carta de tonalidad tras Apply ⇒ `(superseded by card)`, manda la carta | PENDIENTE |
| **ST-CTX-6** | Compilación de producción sin `ALWTTT_DEV` ⇒ byte-idéntica | PENDIENTE |
| **ST-CTX-7** | Persistencia (CTX-1b) — con Hold ON, el override sobrevive ≥ 3 loops; el log de drift identifica el mecanismo que revierte | PENDIENTE |

**Uso en producción de la herramienta.** Los tests T2.1–T2.7 de la pasada de
escucha CONT-B se ejecutaron con esta sección (7/7 PASS), lo que constituye su
primera validación de campo aunque los smokes formales sigan pendientes.
### 9.18 CTX-2a — part tempo override (2026-08-03)

| ID | Test | Estado |
|---|---|---|
| **ST-CTX2A-1** | **Regresión D11** — build nuevo, canción nueva, sin cartas de tempo ⇒ la línea de lectura muestra `Range=Slow` y un `resolved` dentro de la banda Slow | PASS |
| **ST-CTX2A-2** | Apply — override 70 BPM ⇒ el loop siguiente resuelve `70` y suena al tempo pedido (verifica que el cortocircuito de `resolvedBpm` se soltó) | PASS |
| **ST-CTX2A-3** | **Regresión clear/restore** — tras Clear, `Explicit=null`, etiqueta `tempo` restaurada y `resolved` == valor pre-Apply exacto; sin residuo | PASS |
| **ST-CTX2A-4** | Persistencia — con Hold ON y loop infinito, el override sobrevive ≥3 loops; si el modelo drifta, el log `[CTX-2a] … re-asserting` lo cuenta | PASS |
| **ST-CTX2A-5** | Precedencia vs carta de escala — override 70 + Push It (×1.5) ⇒ **105** (el override compone con la escala, no la bloquea) | PASS |
| **ST-CTX2A-6** | Pisado por carta (Hold OFF) — carta de tempo Range tras Apply ⇒ `(superseded by card)`, manda la carta | PASS |
| **ST-CTX2A-7** | **Compilación de producción** sin `ALWTTT_DEV` ⇒ compila limpio; el único diff de producción del lote es el default `Slow` | PASS |

**Nota.** ST-CTX2A-7 **no** es un test de identidad de bytes respecto al build
anterior: D11=A cambia comportamiento de producción a propósito. La afirmación
verificada es que el diff de producción del lote es *exactamente* ese default.

### 9.19 CTX-2b — articulation override (2026-08-03)

| ID | Test | Estado |
|---|---|---|
| **ST-CTX2B-1** | Identidad de bytes tras Clear — seed pineado, Apply `Offbeat` → Clear ⇒ `bundle HIT` con clave idéntica a la de línea base (id 53202 restaurado) | **PASS** |
| **ST-CTX2B-2** | Apply audible — `Offbeat` sobre Electric Piano ⇒ `trackHash` movido, render fresco, contratiempo audible, sufijo `[dev-artic]` | **PASS** |
| **ST-CTX2B-3** | Trampa del hash — segundo Apply sin Clear ⇒ clon con **instance ID nuevo** (−46610 → −46700), hash nuevo, render fresco, audio del nuevo valor | **PASS** |
| **ST-CTX2B-2b** | `rate=Random` — figura concreta + rate centinela ⇒ herramienta correcta (clon, hash, render fresco) pero **figura suprimida** package-side | **PASS (herramienta)** — hallazgo F-ARTIC-RATE-RANDOM-1 |
| **ST-CTX2B-4** | Fuga de clones — Hold ON, ≥6 loops ⇒ `live dev clones: 1` constante y `id=` estable | **PASS** |
| **ST-CTX2B-5** | Pisado por carta — carta de Backing con bundle propio ⇒ `superseded by card`, clon destruido, nada restaurado | **PASS** |
| **ST-CTX2B-6** | Determinismo de `expr=Random` bajo seed pineado entre relanzamientos | **DIFERIDO** — la comparación estricta pertenece al contrato §8.5 del composer; es test package-side, no de consumidor |
| **ST-CTX2B-7** | Compilación de producción sin `ALWTTT_DEV` | **PASS** |

**Nota de instrumentación (reutilizable).** La caché de bundles de `MidiMusicManager`
es el detector de identidad de bytes: un `bundle HIT` devuelve el `mergedBytes` guardado,
el mismo array. No hace falta añadir un volcado de hash al tab.

## 10. Update rule

This SSoT must be updated when any of the following change:
- The scripting define name or gating strategy.
- The overlay's compositional surface (new controls, removed controls, changed toggle key, added/removed tab).
- The set of Dev Mode entry points into production code (new Dev-prefixed method; changed signature or gate).
- The hand-visibility bridge semantics in `OnCompositionSessionEnded`.
- The spawn-gate predicate in `CanDevSpawnToHand`.
- Stats tab content changes (new sections, new controls, layout changes).
- The run telemetry logger (§17): its bus subscriptions, record schema/field set, output path, or coverage limitations. A schema change bumps `schemaVersion` in code and this section.
- Audio Mix tab content changes (sliders, highlight trigger, persistence wiring); the mix *model* itself is governed by `SSoT_Audio.md`.
- New Dev-prefixed methods on gameplay classes (BandCharacterStats, MusicianBase, CompositionSession, etc.).
- The `LiveInspiration` routing contract (which field Dev reads/writes when composition is active vs. not) — if that rule ever changes, update §13 and this list.
- Composition tab content changes (§18): new/removed override families, a change to which model field or dictionary an override writes, a change to the cache-invalidation shape of any override family, or a change to the `'*'` / `(off-band)` / `(outside permitted set)` / `[dev-inst]` annotation conventions. A change to the *cache* semantics must also update `SSoT_Runtime_CompositionSession_Integration.md §8` inv 9.
- The Dev-setter animation-duration convention (currently `0.1f` as a workaround for `HealthBarController.SetCurrentValue(duration=0f)` no-op behavior; see §14.5). If the underlying component is fixed to handle zero durations correctly, the Dev setters may revert to `0f`.
- Any new Phase (3+) that adds or modifies runtime-mutation surfaces.

Updating `SSoT_Dev_Mode.md` typically implies companion updates to `CURRENT_STATE.md` (operational slice) and `changelog-ssot.md` (semantic/authority).

---

## 11. Phase 2 — card spawner

### 11.1 Capability

Arbitrary instantiation of any `CardDefinition` from the game's runtime catalogue into the active hand during PlayerTurn, via the normal card-play pipeline. Primary iteration surface for card balance and multi-turn status validation.

### 11.2 Catalogue source

Since **DEMO-FIXES-A (2026-07-15, D-DF-7=A)** the source is the **runtime union of the current band's per-musician catalogs** (`PersistentGameplayData.BuildBandCardCatalog` over `PD.MusicianList` → each rostered musician's `MusicianCharacterData.CardCatalog`; runtime read, no asset mutation). `GameManager.GameplayData.AllCardsList` is now **fallback only** (dev scenes with no band) and is deprecated as a hand-maintained catalogue — a Dev-Mode-only card needs adding to `AllCardsList` only for that no-band path; in a real band it appears via a rostered musician's `CardCatalog`. The deck editor's catalogue (which scans `AssetDatabase`) may still surface cards these do not — an acceptable asymmetry. Governing description: the Catalogue-tab **Source** bullet (§ Catalogue tab, Phase 2).

### 11.3 Spawn pipeline

`DeckManager.DevSpawnCardToHand(CardDefinition)` is the sole Dev Mode entry point. Overlay code (`DevCardCatalogueTab`) does not mutate hand state directly.

On a successful call:
1. `GameManager.BuildAndGetCard(def, HandController.DrawTransform)` — identical to the per-card tail of `DrawCards`.
2. `HandController.AddCardToHand(built)` — adds to the visible hand list.
3. `HandPile.Add(def)` — tracks the card as "in hand" for the runtime deck model.
4. `UIManager.GigCanvas.SetPileTexts()` if available.
5. Lime `[DevMode] DevSpawnCardToHand: '<n>' → hand=n/max handPile=n discard=n draw=n` log.

On any gate failure, `DevSpawnCardToHand` returns false and logs `DevSpawnCardToHand skipped ('<n>'): <reason>` without mutating any state. Overlay's Spawn button is `GUI.enabled = CanDevSpawnToHand()`.

### 11.4 Gate predicate

`CanDevSpawnToHand(out string reason)` is the authoritative gate. It checks, in order:
- `HandController != null` (otherwise "HandController is null").
- `HandController.DrawTransform != null` (otherwise "HandController.DrawTransform is null").
- `HandController.DrawTransform.gameObject.activeInHierarchy` (otherwise "HandController.DrawTransform inactive (hand hidden)"). This is the mirror of the Phase 1 hand-visibility bridge — see §5.
- `GameManager.GameplayData != null` (otherwise "GameManager.GameplayData is null").
- `GigManager.Instance != null` (otherwise "GigManager.Instance is null").
- `GigManager.Instance.CurrentGigPhase == GigPhase.PlayerTurn` (otherwise "Not PlayerTurn (current: <phase>)").
- `HandPile.Count < MaxCardsOnHand` (otherwise "Hand full (N/N)").

Any change to the gate's order, checks, or error strings must update this section and the `DevCardCatalogueTab` gate-status display.

### 11.5 Spawned-card lifecycle (decision U1)

Spawned cards enter `HandPile` on spawn, `DiscardPile` on play, and `DrawPile` on next reshuffle — see §8.5. This is accepted pollution.

### 11.6 Overlay filter state

`DevCardCatalogueTab` keeps the filter state (`_search`, `_filterAction`, `_filterComposition`, `_scroll`) as static fields so filter settings persist while the overlay is open across tab switches. Filter cache is invalidated on dirty detection (source count, search string, toggle state) to avoid per-frame reallocation.

### 11.7 Unblocks

Phase 2 unblocked the following that Phase 1 alone could not:
- **Multi-turn status validation tests T5/T8 from M1.2** — passed 2026-04-20 (§9.3).
- **Arbitrary starting conditions for combat tuning** — any card can be tested without deck re-authoring.
- **Composition card spawning during a live gig** — used for ST-P2-4 regression.

Phase 2 did **not** unblock T7 Shaken validation — that required the Phase 3.1 Breakdown entry point (now closed).
- Stat editing (Inspiration, LoopScore, SongHype, Cohesion, per-character stats — Phase 3 scope).
- Encounter-modifier toggles (Phase 3 scope).

---

## 12. Phase 3.1 — Breakdown entry point (2026-04-23)

### 12.1 Capability

Stats tab added to the overlay. Phase 3.1 delivers one section: Breakdown. The musician selector grid shows all spawned musicians with live stress readout and `(BD)` tags. "Force Breakdown" button triggers a full Breakdown via the natural stress path.

### 12.2 Entry points

- `BandCharacterStats.DevResetBreakdown()` — sets `IsBreakdown = false`. Dev Mode only; production code never un-breaks a musician.
- `MusicianBase.DevForceBreakdown()` — resets IsBreakdown, then `AddStress(MaxStress)`. Clamps to cap, triggers Breakdown since IsBreakdown was just cleared. Downstream effects fire normally: Cohesion−1, Stress reset to `BreakdownStressResetFraction`, Shaken applied via catalogue lookup, `IsStunned` set.
- Re-triggerable: calling twice produces two Breakdown events (two Cohesion drops, Shaken re-application).
- With Infinite Turns on, `LoseGig` from Cohesion ≤ 0 is suppressed per §4.2.

### 12.3 Unblocks

- T7 Shaken expiry validation (deferred since M1.2, 2026-04-14). Now passed — M1.2 multi-turn validation gap fully closed.

### 12.4 Smoke tests

ST-P31-1 through ST-P31-6 + T7: all passed 2026-04-23. See §9.3 and §9.4.

---

## 13. Phase 3.2 — gig-wide stat editing (2026-04-23)

### 13.1 Capability

Stats tab gains a Gig-Wide Stats section. Three live controls at P3.2 close: SongHype slider, Inspiration slider, BandCohesion stepper. Flow stepper added in P3.3a (see §14.4). Changes take effect immediately and propagate to downstream gameplay consumers (UI, cost gate, loss condition) via the natural event / routing paths — not direct UI pokes.

### 13.2 Entry points

- `GigManager.MaxSongHype` (getter, Dev-only) — upper bound for the SongHype slider.
- `GigManager.LiveInspiration` (getter, Dev-only) — returns `_session.CurrentInspiration` when a `CompositionSession` is active, otherwise `PersistentGameplayData.CurrentInspiration`. This is what the slider reads so that the displayed value matches what the card-cost gate actually uses.
- `GigManager.DevSetSongHype(float)` — clamps to `[0, maxSongHype]`, calls `UpdateAudienceBeatIntensity()`, fires `OnSongHypeChanged01`. Bypasses the `debugSongHype` early-return that `AddSongHype` observes.
- `GigManager.DevSetInspiration(int)` — clamps to `[0, pd.MaxInspiration]`. Writes to `pd.CurrentInspiration`. If `_session != null && _session.IsActive`, also calls `_session.DevSetCurrentInspiration(clamped)`. This is the routing that makes the Dev slider affect the live composition budget.
- `GigManager.DevSetBandCohesion(int)` — floor at 0, no upper cap. Writes to `pd.BandCohesion`. If the new value is 0, calls `LoseGig()`. `LoseGig`'s existing Infinite-Turns suppression branch applies — OFF triggers the loss panel, ON logs suppression and continues the gig. This is the symmetric Dev counterpart to the natural Breakdown → Cohesion−1 → LoseGig path (see `MusicianBase.OnBreakdown`).
- `CompositionSession.CurrentInspiration` (getter, Dev-only).
- `CompositionSession.DevSetCurrentInspiration(int)` — sets `_currentInspiration`, calls `_ctx?.CompositionUI?.SetInspiration(value)`. Does not write to `PersistentGameplayData` — the caller (`GigManager.DevSetInspiration`) owns that.
- `GigManager.AdjustInspiration(int delta)` (production, **not** Dev-only) — public wrapper. When `_session != null && _session.IsActive`, delegates to `CompositionSession.AddCurrentInspiration(delta)`. Otherwise writes `pd.CurrentInspiration` directly with `Mathf.Clamp(before + delta, 0, pd.MaxInspiration)`. Used by `CardBase.SpendInspiration` (negative delta) and `CardBase.GenerateInspiration` (positive delta). Returns the actual delta applied post-clamp. MB4 (2026-05-08).
- `GigManager.IsCompositionSessionActive` (getter, Dev-only) — exposes `_session != null && _session.IsActive` for the Stats-tab raw `[PD/Session]` readout. Same predicate used internally by `LiveInspiration`, `DevSetInspiration`, and `AdjustInspiration` to decide session-routing. Surfaced as a Dev-only readable for diagnostic UI. MB4-diag (2026-05-08).

**Stats-tab raw [PD/Session] readout (MB4-diag, 2026-05-08).** A single-line readout below the Inspiration slider shows `PD=N  Session=N` while a session is active and `Session=—` otherwise. The slider's primary value (`LiveInspiration`) collapses pd and session into one number for ergonomic editing; the raw readout below it surfaces the dual-siting split for diagnostic purposes. Useful for spotting comp-card build-phase divergence (§13.4 caveat) and for regression checks on `AddCurrentInspiration`'s mirror.

### 13.3 Dev Mode principle — symmetric consequences

Dev Mode mutations reproduce the natural gameplay consequences of the same state change. `DevSetBandCohesion(0)` triggers `LoseGig()` the same way a Breakdown-driven cohesion drop would. This principle resolves the question that arose during P3.2 implementation: should Dev setters be "pure value editors" or "simulate natural writes"? The latter is what makes Dev Mode useful for playtest — you're testing the real game, not a detached debug view.

The Infinite-Turns suppression already built into `LoseGig`/`WinGig` (see §4.2) is the correct escape hatch for when you want to scrub values without gig-ending.

### 13.4 Inspiration dual-siting (architectural finding)

Inspiration lives in two fields that are not continuously synchronized:

- `PersistentGameplayData.CurrentInspiration` — the persistent / between-session field. Initialized by `GigManager.StartGig` from `InitialGigInspiration`; reset at PlayerTurn start from `TurnStartingInspiration` when `KeepInspirationBetweenTurns` is false.
- `CompositionSession._currentInspiration` — the live session budget. Set at `Begin()` and `ConfirmCurrentPartAndStart()` to `_rules.inspirationPerPart`. This is the value the composition card cost gate reads (`TryPlayCompositionCard` step 1) and the value the composition UI displays (`ui.SetInspiration`).

During an active composition, writes to `pd.CurrentInspiration` alone are invisible to gameplay. The Stats-tab slider reads `LiveInspiration` (which returns whichever is authoritative for the current moment); writes go through `DevSetInspiration` which routes to both. The same routing pattern applies to action-card spend via `AdjustInspiration` (MB4).

**Closure history.**
- **M4.6F-3** closed the dual-siting on the **comp-card and per-loop-gain paths**. `CompositionSession.AddCurrentInspiration` mirrors PD ↔ session on every call. Track-derived per-loop gain (`HandleLoopFinished`) and host-driven F-3 gain (`OnCompositionLoopFinished`) both route through it.
- **MB3** (2026-05-08) closed the **Dev path**. `LiveInspiration`, `DevSetInspiration` session routing, `CompositionSession.CurrentInspiration` getter, and `CompositionSession.DevSetCurrentInspiration` were documented in §13.2 since 2026-04-23 but never implemented. ST-P32-2 / ST-P32-3 retroactively invalidated; ST-MB3-1..8 in §9.10. The Stats-tab Inspiration slider reads `LiveInspiration` and writes through `GigManager.DevSetInspiration` with session routing.
- **MB4** (2026-05-08) closed the **action-card path**. `CardBase.SpendInspiration` and `CardBase.GenerateInspiration` now route through `GigManager.AdjustInspiration`, which delegates to `CompositionSession.AddCurrentInspiration` when a session is active. PD, session, and the composition UI stay in sync on every action-card and SFX-card play. ST-MB4-1..5 in §9.11. **Behavior tightening:** over-spend now clamps at 0 instead of producing negative `pd.CurrentInspiration` (ST-MB4-3).

**Lifecycle clarification (surfaced via ST-MB3-3 INVALID).** The `CompositionSession` is alive for the **entire PlayerTurn**, not just composition playback. `StartCompositionSession` is invoked at PlayerTurn entry (`GigManager.cs` `case GigPhase.PlayerTurn:` block) and `_session` is nulled only by `OnCompositionSessionEnded` after `End()` propagates from the final part of a song. Therefore `_session != null && _session.IsActive` is `true` for the entire action-card window during a gig, including the moment immediately after `StartGig`. The "no active session" branch of `LiveInspiration` / `DevSetInspiration` / `AdjustInspiration` only fires between gigs (main menu, post-WinGig/LoseGig, pre-StartGig).

**Caveat — comp-card build-phase spend.** `TryPlayCompositionCard` step 8 (CompositionSession.cs lines 342–356 region) decrements `_session._currentInspiration` only and does not mirror to PD. This is the one remaining un-mirrored write path. ST-MB4-4 verifies that MB4 preserved this divergence (PD unchanged, session decrements) — it is intentional preservation, not a regression. A future consolidation could route this through `AddCurrentInspiration` too; deferred to the loop-game-flow milestone.

**Open finding (parked, MB5 candidate).** `GigManager.CanPlayActionCard` does not check `def.InspirationCost <= pd.CurrentInspiration`. Action cards can be played with insufficient inspiration; MB4's clamp-at-0 prevents negative pd but does not gate the play itself. Compare comp cards: `TryPlayCompositionCard` step 1 does check and refuses. MB5 candidate batch — not scheduled.

**Diagnostic surface (MB4-diag).** A Stats-tab raw `[PD/Session]` readout makes the dual-siting split (or convergence) directly visible. See §13.2.

`SSoT_Gig_Combat_Core` §4.2 may want a one-line note surfacing this implementation reality; that remains a separate doc pass.

### 13.5 Smoke tests

ST-P32-1 (SongHype slider) and ST-P32-4..7 remain valid; see §9.5.

ST-P32-2 and ST-P32-3 were recorded as PASS 2026-04-23 but the four routing surfaces those tests purported to exercise (`LiveInspiration`, `DevSetInspiration` session routing, `CompositionSession.CurrentInspiration`, `CompositionSession.DevSetCurrentInspiration`) were never implemented in code on that date. Those entries were not honest observations of the documented routing. Retroactively invalidated 2026-05-08; replacements recorded as ST-MB3-1..8 in §9.10 (with ST-MB3-3 INVALID per its precondition reachability — see lifecycle clarification in §13.4).

MB4 smoke tests (action-card path closure + raw readout) recorded as ST-MB4-1..5 in §9.11. All PASS 2026-05-08.

ST-MB3-5 / ST-MB3-6 / ST-MB3-7 (the carry-over branch for `JamRules.inspirationPerPart == 0`) are deferred to the loop-game-flow milestone — no current encounter exposes this configuration, so empirical verification waits for that batch.

### 13.6 Unblocks

Full live playtest of gig-wide meters without having to re-author cards or encounter configs. Unblocks tuning-in-play for SongHype curve shape, Inspiration tightness, and cohesion-loss sensitivity. P3.3 (per-character edits) is the remaining stat-editing gap.

---

## 14. Phase 3.3a — per-character stat editing + Flow gig-wide (2026-04-23)

### 14.1 Capability

Stats tab gains a Per-Character section. Two subsections: **Musician** (Stress slider, MaxStress stepper, Composure stepper) and **Audience** (Vibe slider, MaxVibe stepper). The existing Gig-Wide Stats section is extended with a Flow row (aggregate readout + uniform ± stepper). All writes route through the natural state-change paths — Breakdown and Convinced thresholds still fire, status icons animate, song-end reset still clears song-scoped stacks.

### 14.2 Entry points

- `BandCharacterStats.DevSetCurrentStress(int)` — clamps via `SetCurrentStress(target, duration: 0.1f)`, calls `CheckBreakdownThreshold()`. Sticky: setting Stress down after Breakdown does not un-break.
- `BandCharacterStats.DevSetMaxStress(int)` — floors at 1, clamps `CurrentStress` down via `SetCurrentStress(CurrentStress, duration: 0.1f)`, re-checks threshold.
- `BandCharacterStats.CheckBreakdownThreshold()` — private; extracted from `AddStress`. Single source of truth for the Breakdown trigger; called by `AddStress`, `DevSetCurrentStress`, `DevSetMaxStress`.
- `AudienceCharacterStats.DevSetCurrentVibe(int)` — clamps via `SetCurrentVibe(target, duration: 0.1f)`, calls `CheckConvincedThreshold()`.
- `AudienceCharacterStats.DevSetMaxVibe(int)` — floors at 1, clamps `CurrentVibe` down via `SetCurrentVibe(CurrentVibe, duration: 0.1f)`, re-checks threshold.
- `AudienceCharacterStats.CheckConvincedThreshold()` — private; extracted from `AddVibe`. Single source of truth for the Convinced trigger (sets `IsConvinced`, applies `StatusType.Convinced`, clears `StatusType.Tall`, fires `OnConvinced`).
- `GigManager.TotalFlowStacks` — public getter wrapping the existing private `GetTotalFlowStacks()`.
- `GigManager.DevAddFlowToAllMusicians(int delta)` — resolves the `"flow"` SO from the first available musician's catalogue, applies `delta` to every musician's `StatusEffectContainer`. Pre-guards `Apply(-N)` on zero-stack containers.

### 14.3 Composure as status-stack

Composure is not a first-class field on `BandCharacterStats`. It is modeled as stacks of `CharacterStatusId.TempShieldTurn` on the musician's `StatusEffectContainer`. The Per-Character section surfaces it as a meter-like stepper for parity with Stress/MaxStress, but the backing write is `statuses.Apply(composureSO, delta)`, not a setter on the stats class. The SO's authored `MaxStacks` is respected by the container's stacking policy; the UI disables the `−` button at 0 to avoid spurious `OnStatusCleared` events from the container's empty-apply path.

### 14.4 Flow gig-wide semantics

Flow is song/band-scoped in gameplay terms (see `SSoT_Gig_Combat_Core.md` §6.1) but is stored per-musician as `CharacterStatusId.DamageUpFlat` stacks. The gig-wide surface:
- **Display:** aggregate via `TotalFlowStacks` (sum across all musicians' DamageUpFlat stacks — identical to what the scoring path reads).
- **Edit:** uniform delta via `DevAddFlowToAllMusicians` — one stepper press applies ±1 to every musician's container.
- **Reset:** unchanged. `GigManager.ResetSongScopedStatuses` clears both Flow and Composure at song end, and the regression test (ST-P33a-10) confirms the Dev-added stacks reset correctly.

Editing individual musician Flow is not exposed. If per-musician Flow becomes meaningful later, the surface can be extended into the Per-Character section; until then, gig-wide is the authoritative affordance.

### 14.5 Animation-duration workaround

`DevSet…` setters pass `duration: 0.1f` (not `0f`) to the clamp-setters. `HealthBarController.SetCurrentValue(duration=0f)` does not write the final value through to the visual fill — internal state advances, the tweened bar stays at its previous position. This is a latent issue in `HealthBarController`, outside the Dev Mode scope, exposed for the first time by the Dev-driven jump-cut paths (no gameplay caller uses `0f`). The `0.1f` workaround animates over ~6 frames at 60fps — imperceptible as a delay but non-degenerate as a tween window. Revert to `0f` if/when the underlying component is fixed to write the final value on zero-duration calls. See §10 Update rule.

### 14.6 Smoke tests

ST-P33a-1 through ST-P33a-10: all passed 2026-04-23. See §9.6.

### 14.7 Side-resolution: `DevResetConvinced` implementation

P3.3a testing surfaced a pre-existing doc-vs-code drift: §6 and §7 of this SSoT have declared `AudienceCharacterStats.DevResetConvinced` as an existing Dev Mode entry point since P3.1 closure, but the method was never implemented. `DevModeController.ResetConvincedAudience` called it, which silently compile-failed only under `ALWTTT_DEV`. P3.3a adds the implementation — sets `IsConvinced = false` and `ClearStatus(StatusType.Convinced)` — which matches the contract §7 has always described. No authority change; only code caught up.

### 14.8 Unblocks

Full live playtest of per-character meters and per-musician status stacks (for the Composure case) without authoring changes. Unblocks tuning-in-play for Breakdown thresholds, audience persuasion curves, and Flow pacing. P3.3b (generic status apply/remove via picker) is the remaining state-editing gap.

---

## 15. Phase 3.3b — status apply/remove picker (2026-04-24)

### 15.1 Capability

Per-Character section gains generic status editing for both musicians and audience. The picker is embedded directly in each character's subsection (below the existing stat controls), reusing the existing character selectors. Two affordances per character:

1. **Active-status readout:** iterates `StatusEffectContainer.Active` keys, displays each entry as `{DisplayName} ×{Stacks}` with `[−1]` and `[Clear]` buttons. `[−1]` calls `container.Apply(inst.Definition, -1)`, which lets the stacking policy handle decrement and auto-clears at 0 stacks. `[Clear]` calls `container.Clear(id)` for immediate full removal.
2. **Catalogue-backed apply picker:** `[◄][►]` buttons cycle through non-null entries in `character.StatusCatalogue.Effects`. Selected entry displayed as `{DisplayName} ({EffectId})`. Wrap-around navigation. `[+1]` button calls `container.Apply(selectedSO, 1)` and emits a lime `[DevMode] StatusPicker: Applied {name} ×1 to {character}. Stacks now: {n}` log.

Graceful fallback: when `StatusCatalogue` is null on the character, the apply section shows "(no catalogue — assign on prefab)". The active-status readout still works (it reads from the container directly, not the catalogue).

### 15.2 No production-class patches

Unlike P3.3a, P3.3b does not add any `DevSet…` wrappers on gameplay classes. The existing `StatusEffectContainer` public API (`Apply`, `Clear`, `GetStacks`, `Active`) and `CharacterBase.StatusCatalogue` property provide everything the picker needs. The symmetric-consequences principle (§13.3) does not apply — status application through the container fires `OnStatusApplied`/`OnStatusChanged`/`OnStatusCleared` events, which update icons automatically via the existing `CharacterCanvas.BindStatusContainer` wiring.

### 15.3 Gameplay-flag asymmetry (known limitation)

Applying a status via the picker sets stacks on the `StatusEffectContainer` and fires icon events, but does **not** trigger gameplay-flag side effects that the natural paths would set:

- Applying `Convinced` (or the SO mapped to it) via the picker does **not** set `AudienceCharacterStats.IsConvinced = true`. The `IsConvinced` flag is only set by `CheckConvincedThreshold()`, which is called by `AddVibe` and `DevSetCurrentVibe/DevSetMaxVibe`.
- Applying `Shaken` via the picker does **not** set `BandCharacterStats.IsBreakdown = true` or trigger the Cohesion/Stress-reset/stun path. To test those, use `DevForceBreakdown` (§12).
- Clearing `DisableActions` via the picker **does** clear `IsStunned` — the getter derives from `Statuses.HasActive(CharacterStatusId.DisableActions)` and the `SyncLegacyStunFromStatuses` callback fires on `OnStatusCleared`.

This is acceptable for Dev Mode. The picker is a state-injection tool, not a gameplay-simulation tool. Users who want the full gameplay consequences should use the dedicated Dev actions (Breakdown, Vibe slider to max for Convinced, etc.).

### 15.4 Catalogue scope finding

P3.3b testing surfaced that musicians and audience may share the same `StatusEffectCatalogueSO` instance on their prefabs. This means the picker shows all statuses (including musician-only ones like Flow, Composure) on audience members and vice versa. Applying a musician-only status to an audience member is harmless (the container accepts it, an icon appears, but no gameplay code reads that primitive from audience) but is confusing.

**Resolved 2026-04-24 (MB2):** catalogue split into `StatusEffectCatalogue_Musicians.asset` (6 canonical musician statuses: flow, composure, choke, shaken, exposed, feedback) and `StatusEffectCatalogue_Audience.asset` (empty at MVP; populated at M4.3 with Earworm). Musician and audience prefabs reassigned to their respective catalogues. The picker now shows only character-type-appropriate statuses. Audience picker currently displays the `(no catalogue — assign on prefab)` fallback text from `DrawStatusPicker` because the audience catalogue is non-null but empty — this message is misleading but harmless. Minor UX polish deferred: distinguish null-catalogue from empty-catalogue fallback text. See §9.9 for validation.

### 15.5 Smoke tests

ST-P33b-1 through ST-P33b-10: all passed 2026-04-24. See §9.7.

### 15.6 Unblocks

Arbitrary status application and removal on any character without card authoring or encounter changes. Closes the state-editing gap identified in §14.8. Full Dev Mode stat/state toolset now covers: infinite turns, card spawning, gig-wide meters, per-character stats, and per-character status stacks. Remaining deferred: P3.4 audience transparency panel, encounter modifier toggles.

---

## 16. Idea backlog (future Dev Mode surfaces, planning-only)

**Status: planning-only — not scheduled, not implementation truth.** Small Dev Mode feature ideas surfaced incidentally during other batches, parked here until a batch picks one up. Listing an item here does not promote it to a committed phase or authorize implementation.

1. **Runtime control of loops-per-song**, to make testing long/short songs faster without editing SO assets.
2. **Runtime control of songs-left**, to skip to the end of a gig or extend it on demand.
3. ~~**Overlay tab wiring for `DevPinnedSongSeed`** (§8.7, §6) — currently code/debugger-only.~~ **Delivered by DBG-C1 (2026-07-17)** — seed field in the Composition tab (§18.2); §8.7 marked RESOLVED.

> **Placement note (2026-07-05).** This backlog was originally specified to land in `M1_5_Dev_Mode_Sub_Roadmap.md`. That doc is archived (`SSoT_INDEX.md` lists it under Archived planning docs, superseded by this SSoT), so the backlog was placed here instead to avoid silently reviving a retired planning surface — see `changelog-ssot.md` (2026-07-05 entry) for the reasoning. Redirect back to a fresh live Dev Mode sub-roadmap instead if the backlog outgrows this section.

---

## 17. TLM-1 — run telemetry logger (2026-07-16)

*(Placed after the §16 planning backlog to preserve §16's numbering — the §8.7 cross-reference to "§16, the idea backlog" stays valid. This is a shipped surface, not backlog.)*

### 17.1 Surface

`DevRunTelemetryLogger` — static class, `Assets/Scripts/DevMode/DevRunTelemetryLogger.cs`, **whole file inside `#if ALWTTT_DEV`**. Lifecycle owned by `DevModeController`: `Initialize()` in `Awake`, `Shutdown()` in `OnDestroy` (sibling pattern to `DevGigOutcomeTracker`). It is a **read-only** sensory-bus subscriber — it publishes nothing and mutates no game state (MidiGenPlay untouched). Subscriptions:

- `GigStartedEvent` — clears the per-gig accumulators and records `RequiredSongCount`.
- `CardPlayedEvent` — appends to the ordered play list, capturing `PD.CurrentSongIndex` **at play time** (the BALANCE-XREF confound guard — `CurrentSongIndex` only increments at song completion, so the value at a `CardPlayedEvent` is the index of the song the card was played in). Composition cards are included: `CardPlayedEvent` fires from the unified `DeckManager.OnCardPlayed` funnel, which observes both action/SFX and composition plays.
- `LoopResolvedEvent` — increments the gig's loop count.
- `GigOutcomeEvent` — assembles and writes the record.

### 17.2 Record + output

One **JSON-Lines** object per gig (`schemaVersion` 1), append-per-gig, human-readable. Fields (D-TLM-2=B):

- `schemaVersion`, `timestampUtc` (ISO-8601), `sessionId` (per-play-session GUID prefix — groups one playtest sitting), `encounterLabel` (`GigEncounter.GetLabel()`), `requiredSongCount`.
- `won`; `lossCause` (`"unconvinced_after_final_song"` for logged losses, empty on win — see §17.3 for why this is the only value).
- `songsCompleted` (`PD.CurrentSongIndex` at outcome), `loopsPlayed`.
- `roster[]` — musician `CharacterId`s (stable authored ids).
- `audience[]` — one entry per member: authored `CharacterName` + spawn index (`index`) + `endVibe`/`maxVibe`/`convinced`. **Snapshotted at `GigOutcomeEvent`, which publishes before `WinGig`/`LoseGig`**, so the end-Vibe values precede any cleanup.
- `plays[]` — ordered: `cardId` (`CardDefinition.Id`) + `songIndex`-at-play-time + `isComposition` + `inspirationCost`.
- `playCounts[]` — per-`cardId` aggregate of `plays[]`.

Output path (never under `Assets/` or `Resources/`, so no importer churn, never shipped):

- Editor: `<projectRoot>/DevTelemetry/gig_runs_YYYY-MM-DD.jsonl` — **gitignored** (`DevTelemetry/`).
- Dev Player builds: `Application.persistentDataPath/DevTelemetry/…`.

The Stats tab shows a one-line `Last gig written to: <path>` next to the existing outcome tally (`DevRunTelemetryLogger.LastWritePath`).

### 17.3 Coverage limitations (load-bearing for S5i analysis)

- **Cohesion-collapse losses are NOT logged (D-TLM-3=A).** The `MusicianBase.OnBreakdown → BandCohesion 0 → LoseGig()` path publishes no `GigOutcomeEvent` — only `GigManager.ResolveGigOutcomeAndEnd` publishes it. The existing session W/L tally shares this exact blind spot. Consequently `lossCause` is constant `"unconvinced_after_final_song"` for every logged loss. **Do not read "no cohesion-loss records" as "no cohesion losses."** The publisher-side fix is the optional rider **TLM-1b** (adds the publish + a per-gig double-fire latch + a review of the tally semantics and the `tut_first_gig_won` debug-path exposure) — open it only if S5i playtests actually hit cohesion losses.
- **Editor Debug context-menu Win/Lose** bypass `GigOutcomeEvent` by design and are not logged.
- **Partial gigs** (retry/quit mid-gig) produce no record; accumulators reset on the next `GigStartedEvent`.
- **Audience identity** in records is authored `CharacterName` + spawn index. `AudienceCharacterBase.CharacterId` embeds `GetInstanceID()` and is not stable across sessions, so it is deliberately not used as the record key.

### 17.4 Smoke coverage

ST-TLM-1..4 + ST-TLM-R1 — all PASS 2026-07-16. See §9.12.

### 17.5 Update triggers (specific to this surface)

Update this section (and bump `schemaVersion` in code) when any of: the set of subscribed bus events; the record field set or its nesting; the output path or format; or the documented coverage limitations (e.g. if TLM-1b lands and cohesion losses become loggable). Companion updates: `CURRENT_STATE.md` (operational) and `changelog-ssot.md` (semantic).

---

## 18. Composition debug tab (DBG-C1, 2026-07-17)

**Consumer half of MGP-ALWTTT-DBG, read side (D1=B).** A `Composition` tab in the Dev overlay, surfacing what the package resolved per track for the current part, plus an infinite composition-loop toggle. DBG-C2 (2026-07-17) added the interactive write half (§18.4–§18.8). `#if ALWTTT_DEV`; zero production footprint (ST-S10 PASS).

### 18.1 Per-track log — two phases
- **Intent (handoff)** — drawn every OnGUI pass from `SongCompositionUI.TrackEntry`: role, musicianId, style bundle, explicit/type instrument overrides, per-loop inspiration. Pre-render truth.
- **Resolved (last render)** — from `MidiMusicManager.LastResolvedByTrack` (package `PartRender.resolvedByTrack`), refreshed by polling `LastRenderSerial`. The serial bumps on every `RenderSinglePart` return; a bundle-cache replay republishes the **original** render's snapshot (D-DBG5=A). Header shows `fresh`/`bundle-cache replay` + serial. Observable behavior equals an `OnRenderCompleted` hook without cross-`#if` event plumbing.
- **`'*'` convention (A1, CONFIRMED 2026-07-17 against `Design_Composition_Debug_Tab_v0_1 §3.1`).** A field carries `'*'` when it is **resolved-only** truth, not predictable from intent: `ResolvedSource` ∈ {`CardPalette`, `Procedural`, `SharedProgression`}. Deterministic sources (`RenderOverride`, `CardOverride`, `TrackParameters`) render without `'*'`. The implemented per-field placement is a faithful refinement of §3.1's illustrative sample (§3.1 shows a whole-line trailing `*`; the implementation attaches `*` per resolved field, which is stricter and self-consistent).
- **Role-adaptive** (`GenerationDebugFormatter`): Rhythm → pattern/palette/style-id; Backing → pattern/palette/roman/figures; Melody → asset/per-span archetypes; Bassline → shared-progression flag/roman. Harmony not reported in v1 (ID-2=A). **Compact/Full** flag on `GigDevSettingsSO.CompositionDebugFull` (Compact = one line + counts; Full = every populated field).
- **Copy fingerprint** — `GUIUtility.systemCopyBuffer`, exports header (seed/part/bpm/replay-origin) + per-`(musician, role)` resolved lines, **always Full** regardless of the Compact flag (ST-S7 PASS).
- **chd: dump** — button logs `GetChordTimelineSnapshot()` per channel (governed chd: contract); validated against the tab's Backing roman + audible chords (ST-S5 PASS).
- **Resolved meter/tonality/root (CSV-3, 2026-07-22).** A read line publishes the TS, Tonality and Root the render **actually used**, from `MidiMusicManager.LastRenderResolved{TimeSignature,Tonality,RootNote}` (dev-only; sibling of BAL-1 `appliedCc7ByTrack`, replay-faithful per D-DBG5=A). Tonality carries `aligned from intent <X>` when `ChordTrack` step-2b alignment fired (expected); TS/Root carry `DRIFT` if resolved ≠ model intent (impossible today — a DRIFT is itself a finding).

### 18.2 Seed pin
A seed text field wires `CompositionSession.DevPinnedSongSeed` (D-C1(seed)=A; closes §8.7). Read in `Begin()` — set before the song starts. Enables the ST-S1 BC-gate byte-diff.

### 18.3 Infinite composition-loop toggle
`CompositionSession.DevInfiniteCompositionLoop` (static, dev-only). When ON, the per-part loop **countdown resets** to the full per-part value instead of advancing the part / ending the song (branch in `HandleLoopFinished`, after `LoopFinished?.Invoke`). **Per-loop host hooks keep firing (D2=A):** draw + inspiration run every loop exactly as in normal flow — the decrement, `LoopFeedbackContext`, history, and `LoopFinished` subscribers all execute, only the exhaustion branch is redirected. Toggling OFF lets the restored countdown drain normally (ST-S8/S9 PASS). Resets at song boundary in `Begin()`/`End()` — never leaks across songs; the field does not exist in production builds. **CARD-UX-1 interaction:** under infinite loop a next render always exists, so `IsFinalLoopRunning` is dev-exempted (else the final-loop composition deny would wrongly fire) — see `SSoT_Runtime_CompositionSession_Integration §8` inv 11.

### 18.4 Per-track pattern overrides (DBG-C2, 2026-07-17)
The write half of the tab. `CompositionSession.DevPatternOverrides` (`static`, dev-only, `MusicianTrackKey → PatternDataSO`) is passed to `RenderSinglePart` as `patternOverrides` whenever non-empty; **null when idle** so the production/idle path is byte-identical (D-C1-1's C1 passthrough now LIVE; BC gate ST-C2-7 PASS). Package precedence is **step 0** (override beats card override, card palette, shared cache, `TrackParameters`); the composer clones-on-apply and **warn+ignores type mismatch**.
- **UI:** one row per track. **Rhythm / Backing / Melody** get a `Pick…` dropdown + `Clear`. **Bassline** is greyed with "override Backing instead" — bass renders the shared progression and package-side Bassline overrides are warn+ignore (handoff (c)3; ST-C2-2 PASS). **Harmony** is greyed (no v1 override channel, ID-2=A).
- **Cache rule (D-C2-4=A):** overrides are **never** part of any cache key. When any override is supplied, `MidiMusicManager` bypasses the stem/bundle caches for that render (mirror of the Mod-DIR one-shot bypass), and `CompositionSession` invalidates the part's `PartCache` on `DevOverrideStamp` mismatch (keepTempo+keepInstruments — overrides change patterns, not BPM or voices). Clearing an override bumps the stamp again and the next loop returns to the un-overridden baseline (ST-C2-8 PASS). Full detail in `SSoT_Runtime_CompositionSession_Integration §8` inv 9.

### 18.5 Roman progression → Backing override (DBG-C2, D-C2-1=A)
A foldout free-text field → `ChordProgressionRuntimeImporter.TryParseRoman(roman, partTS, measures, defaultDuration, tonality, …)`. Tonality/measures(0=derive)/default-duration are editable in the tab; TS comes from the current part. On success the returned `ChordProgressionData` is placed in `DevPatternOverrides` for the Backing key (overriding backing **is** overriding the part's harmony). **Importer verdict is surfaced verbatim** — no ALWTTT-side reduction; the D-L4.5 zero-warning guard (out-of-alphabet token ⇒ hard fail, no silent downgrade) is the package's policy and is not reinterpreted here. Hard fail ⇒ nothing applied, warnings shown (ST-C2-4 PASS). The built instance is `HideFlags.DontSave`, named `Runtime: <roman>`, and **never persisted** — it is destroyed on replace/clear. chd: dump matches the applied roman (ST-C2-3 PASS).

### 18.6 Catalogue dropdowns (DBG-C2, Ask B, D-C2-2=A)
Dropdowns are populated from the **full runtime registry** via `PatternRepositoryResources.Get{Drum,ChordProgression,Melody}Patterns(ts)`, TS-filtered to the current part (counterfactual probing is the point of the tab, so band-union was rejected). Assets not reachable from the current part's assigned style bundles (direct override refs + palette entries) are annotated `(off-band)`; a dropdown pick feeds the same `DevPatternOverrides` path as a manual assignment, so a picked asset and its free-entry equivalent render identically (ST-C2-5 PASS). A `Catalog browse` foldout enumerates palettes/phrases via `TrackPatternConfigStoreResources<T>("Drums"/"Chords"/"Phrases")` (requires the E-1b/E-2b package asset moves — confirmed done 2026-07-17).

**Coverage limitation (measured 2026-07-18, CSV-1c).** Both the dropdowns and the browse foldout are **repository/store-fed**, and the repositories scan only their configured Resources roots. The CSV-1 inventory measured the consequence: **all 30 in-use chord progressions are outside those roots**, so the Backing dropdown can only offer the 13 progressions the repository resolves — every one of which is a dead asset. Drums are unaffected (drum patterns do live under `Patterns/Drums`). Playback is not affected, because palettes and style bundles hold direct references and never go through the repository. Whether the dropdown switches source, or the scan roots are corrected instead, is **D-CSV-13 / D-CSV-14** (CSV-3 / CSV-5); the inventory window's `OFF-ROOT` flag (`SSoT_Editor_Authoring_Tools §17.6`) is the measurement surface for both.

**Source decision (D-CSV-13=A, CSV-3, 2026-07-22).** The Backing dropdown **stays** `PatternRepositoryResources`-fed (runtime-honest). The list is empty/small **by measurement**, not by bug — local chord content is off-root until the CSV-5 scan-root fix (D-CSV-14); an in-tab notice says so and points to the Roman free-text override. Switching the source to the inventory union was rejected (editor-only `AssetDatabase` in a runtime tab + duplicate discovery); the question dissolves once D-CSV-14 lands.

### 18.7 R2a debug-play (DBG-C2, D-C2-3=A)
"Re-render part now" bumps `DevOverrideStamp`, which invalidates the current part's `PartCache` and forces a fresh render through the **normal seeded `PlaySinglePartLoop`** at the next loop start (working path reused — no separate playback channel). Under a pinned seed the re-render is bit-reproducible (identical Copy-fingerprint, ST-C2-6 PASS); the resolved log refreshes via `LastRenderSerial` either way. **Disambiguation:** this is the *pattern-override* debug-play. The design doc's §4 R2a ("debug-play any catalogue card's musical side") is a different, larger surface still reserved under M1.5 Phase 5 and is **not** built by DBG-C2 (built by CSV-3, §18.10).

### 18.8 Scope boundary / known follow-ups
The MGP-ALWTTT-DBG consumer arc is complete with DBG-C2. Known non-blocking follow-up **DBG-OBS-1**: the `RenderOverride` resolved line may not display `pattern=<asset>` if the package leaves `ResolvedTrackChoice.sourceAssetName` unpopulated on that source path; localized formatter fallback available if pursued (override correctness is unaffected — ST-C2-1 PASS). Smoke coverage: read side §9.13 (ST-S1..S10), write side §9.14 (ST-C2-1..9). The tab itself continued to grow after the arc closed — see §18.9 (CSV-2), smoke coverage §9.15.

### 18.9 Per-track instrument overrides (CSV-2, 2026-07-18)

Sibling of §18.4, but a **different mechanism** — the asymmetry is the point of the section.

**Mechanism (D-CSV-5=A).** A dev instrument pick writes directly into the composition model's existing fields: `SongCompositionUI.TrackEntry.overrideMelodicInstrument` / `overridePercussionInstrument` (exclusive-set, `hasOverrideInstrumentType` cleared — the same discipline `ApplyInstrumentEffect` follows). No parallel dictionary, no bypass machinery. This was chosen over a `DevPatternOverrides`-shaped separate map because **instrument-override GUIDs already participate in `trackInputsHash`** (`SongConfigBuilder.ComputeTrackInputsHashesForPart`), so the stem cache stays coherent by construction, and because the render-side precedence a dev pick needs is the precedence a card pick already gets. Consequence accepted: at the render boundary a dev override is **indistinguishable from card truth**; the tab's `[dev-inst]` suffix on the intent line is the only disambiguator, and it exists in the tab only (`GenerationDebugFormatter` is untouched).

**Cache interaction — differs from §18.4.** Pattern overrides invalidate with `keepTempo + keepInstruments` (they change patterns, not voices). An instrument override must invalidate with **`keepInstruments: false`**, because a `PartCache` entry preserved with `keepInstruments: true` retains `resolvedMelInstByTrack`, and that map is re-fed into the next `RenderSinglePart` call as the `instrumentOverrides` argument — a stale voice would override the new pick. `CompositionSession.DevInvalidateForInstrumentOverride(partIndex)` therefore mirrors the **instrument-card** invalidation path (`ShouldKeepInstruments` → `CompositionCardClassifier.IsInstrumentCard` → `keepInstruments = false`), not the pattern-stamp path, and bumps `DevOverrideStamp` so the change lands at the next loop start through the normal seeded `PlaySinglePartLoop`. Detail in `SSoT_Runtime_CompositionSession_Integration §8` inv 9.

**Clear / restore.** The tab records the pre-dev field state on first touch per `(musicianId, role)` key, so `Clear` restores whatever was there before — including a prior *card* override, not merely null. Restoration is byte-identical under a pinned seed (ST-CSV-3): the session-level pin maps are the mechanism — `BuildMelodicPinKey`/`BuildPercussionPinKey` return null while an explicit override is set, so the pin is skipped rather than overwritten and the original voice survives in `_sessionMelodicPin`/`_sessionPercussionPin` to be re-applied on clear.

**Card stomp (documented consequence, not a bug).** `SongCompositionUI.ApplyInstrumentEffect` unconditionally clears and rewrites all three override fields on every matching track when an `InstrumentEffect` card is played. A later card therefore **supersedes** a dev override on that track. The tab detects this by comparing the field against the value it applied; on mismatch the row reads `(card) <name>` with a `superseded by card` notice, the dev record is dropped, and **no restore is attempted** — card truth is newer and owns the field.

**Catalogue and permitted set.** Rhythm tracks get the full percussion catalogue; every other role gets the full melodic catalogue. Full catalogue is deliberate — counterfactual probing is the purpose. Entries outside `InstrumentRules.GetPermittedMelodic(musician, role, repo)` are annotated `(outside permitted set)` and remain selectable, mirroring the `(off-band)` convention of §18.6. There is no permitted-set rule for percussion in v1, so percussion entries are unannotated. Source is `InstrumentRepositoryResources` (already merges the package instrument root with `MidiGenPlayConfig.resourcesInstrumentsPath`, de-duped) — not re-implemented.

**Lifecycle.** Dev records live in the tab and are cleared when the session goes inactive (the `TrackEntry` objects die with the model at song end), so nothing leaks across songs. `Clear ALL overrides` clears patterns and instruments together, restoring only the instrument fields the tab still owns.

**Gate (D-CSV-10=A).** Same `#if ALWTTT_DEV` as the rest of the tab; production compile verified clean (ST-CSV-8).

### 18.10 R2a card debug-play (CSV-3, 2026-07-22, D-CSV-8=A, D-CSV-24=B)

Injects any catalogue card's **musical side only** via `CompositionSession.DevInjectCompositionCard(def, targetMusicianId, out reason)` — the concrete form of the M1.5 Phase 5 reserved surface. Applies the primary action + `CompositionCardPayload.modifierEffects` through the shared `SongCompositionUI.ApplyCardDefinitionToPart` core (extracted so no application logic is duplicated), plus the production invalidation/pending path. **Skips:** inspiration check/spend, the `InspirationGenerated` one-shot, and `CardPayload.effects`.

- **D-CSV-8=A (live model).** The change is real and persistent for the loop; it reuses the normal seeded `PlaySinglePartLoop` — no shadow model, no second playback channel.
- **D-CSV-24=B (economy-neutral).** An injected track is marked audition-only in a dev-only key set and excluded from `EvalPerLoopInsp`, so its per-loop inspiration bonus never enters the run economy. A genuine play on the same `(musicianId, role)` reclaims it; the set clears at song boundary. (Option A — no exclusion — was accepted first, then superseded to B for parity with a real play.)
- **Card source** = band union with `AllCardsList` fallback (shared with `DevCardCatalogueTab`).
- **Disambiguation.** DISTINCT from the DBG-C2 "Re-render part now" button (the pattern-override re-render, §18.7).

Gate `#if ALWTTT_DEV`; production byte-identical (ST-CSV3-1/2).

### 18.11 Overlay outer-scroll fix (CSV-3, 2026-07-22)

The F12 overlay content is wrapped in a screen-bounded outer scroll (`DevModeController`), and `GUI.DragWindow` is restricted to the title bar. Fixes the Composition tab growing past the screen bottom with no way to reach it after the R2a section was added. Applies to all tabs; cosmetic/operational only.

**Addendum de ergonomía (CTX-2b, 2026-08-03 — verificado en código 2026-08-08, D2=SÍ).**
Ancho por defecto de la ventana **480 → 720** (`_windowRect` en `DevModeController`) más un
**agarradero de redimensionado** `◢` bajo el scroll, que ajusta *solo* el ancho
(`Mathf.Clamp(..., 380f, 2000f)`; `GUILayout.Window` sigue siendo dueño del alto). El motivo
está escrito en el propio código: las líneas de la pestaña de composición —nombres de bundle,
hashes, avisos— se truncaban a media frase con 480 fijos. El arrastre de la ventana sigue
restringido a la barra de título, de modo que el agarradero y el contenido con scroll no la
mueven.

### 18.12 Part-context override — tonality / root (CTX-1, 2026-07-31)

**Qué añade.** Una sección colapsable `Part context override (tonality / root)`
entre la línea resolved-identity (§18.3/CSV-3) y OVERRIDES, con steppers de
`Tonality` y `NoteName`, Apply, Clear-con-restore y un toggle
`Hold across loops`.

**Mecanismo (extensión de D-CSV-5=A al nivel de parte).** Escribe los campos que
`SongConfigBuilder.FromUI` **ya lee** — `PartEntry.tonality`, `.rootNote`,
`.hasExplicitRootNote` — en vez de abrir un canal nuevo. Como tonalidad y raíz
participan en `partMeterHash`, el cambio invalida la caché por sí solo; el
`DevBumpOverrideStamp()` adicional fuerza el re-render en el siguiente inicio de
loop por la misma ruta que los overrides de patrón. **Onda expansiva: todos los
stems de la parte se regeneran** — idéntica a la de una carta de tonalidad
(D-H1=α), que es precisamente lo que la herramienta auditiona.

**`hasExplicitRootNote` se fuerza a `true` mientras el override está aplicado.**
Sin eso, `FromUI` tira una raíz **aleatoria** cuando el modelo nunca fijó una, y
la audición deja de ser reproducible incluso con seed pineado. El valor original
se restaura en Clear.

**Realineación del paquete es visible, no silenciosa.** Si se pide un modo que
las `tonalities` de la progresión activa excluyen, `ChordTrackComposer` (paso
2b) realinea y la línea resolved muestra `aligned from intent <X>`. **Ese
mensaje es el observable directo de la restricción de tonalidades** y se usa
como señal de test, no como fallo.

**CTX-1b — persistencia entre loops.** Observado en la primera sesión de uso: el
modelo revierte la parte a su tonalidad autorada/de carta en el loop siguiente,
de modo que una escritura única solo sobrevive a un render. **La causa concreta
no está identificada** (reconstrucción del `PartEntry` vs. reaplicación del
intent de la carta al inicio de loop). Con `Hold across loops` activado (default)
el tab re-escribe sus valores cuando detecta que el modelo se ha desviado, y
loguea `[CTX-1b] Model drifted to …; re-asserting …` con contador — ese log es
el instrumento de diagnóstico para identificar el mecanismo. **Consecuencia
documentada, no defecto: con Hold ON el override dev gana a una carta de
tonalidad**; la detección de pisado por carta solo aplica con Hold OFF.

**Guardas.** Identidad de modelo (una canción nueva reconstruye el modelo ⇒ el
registro se suelta, no hay nada válido sobre lo que restaurar) y pisado por
carta con semántica CSV-2 (se suelta **sin** restaurar; la verdad de la carta es
más reciente). `Clear ALL overrides` limpia también esta familia.

**Alcance.** `#if ALWTTT_DEV` a nivel de fichero, huella de producción cero, sin
API nueva. Ruta de producción equivalente: una carta con
`TonalityEffect(Explicit, <modo>)`, audicionable vía R2a (§18.10) — ambas rutas
escriben el mismo campo del modelo, así que lo que se oye con el override es lo
que hará la carta.

### 18.13 Part tempo override — BPM (CTX-2a, 2026-08-03)

**Qué añade.** Una sección colapsable `Part tempo override (BPM)` junto a la de
CTX-1, con stepper de BPM (±5 / ±10 + campo, clamp 40–300), Apply,
Clear-con-restore y el toggle `Hold across loops` (default ON). Encima, una
línea de lectura `BPM: resolved=… | model: Explicit=… Range=… Scale=×…` que
muestra el BPM que la última render realmente usó (`PartCache.resolvedBpm`)
junto al intent del modelo.

**Mecanismo (extensión de D-CSV-5=A al plano de tempo).** Escribe
`PartEntry.absoluteBpmOverride`, un campo que `SongConfigBuilder.FromUI` **ya
lee** (`ExplicitBpm`), en vez de abrir un canal nuevo. No escribe
`tempoRangeOverride` (así Clear restaura sin tocarlo) y **no** usa `tempoScale`:
`tempoScale` es el eje de gusto de la audiencia (`AudienceCharacterBase` compara
`ctx.TempoScale` contra umbrales cuyo default es 1.0, y las cartas Push It /
Half Time asumen línea base 1.0), de modo que usarlo como palanca de
herramienta contaminaría gameplay.

**El paso no obvio: soltar el BPM cacheado.** `CompositionSession`
cortocircuita la resolución de BPM cuando `PartCache.resolvedBpm > 0` (lo pasa
como `bpmOverride` a `RenderSinglePart`), y las invalidaciones de dev
(`DevBumpOverrideStamp` / `DevInvalidateForInstrumentOverride`) usan
`keepTempo: true`, que **preserva** ese valor. Escribir el modelo y bumpear el
stamp por sí solos **no cambian el BPM audible**. Apply pone por tanto
`cache.resolvedBpm = 0` antes de bumpear el stamp; Clear reescribe el
`resolvedBpm` capturado pre-Apply, lo que restaura el estado audible **exacto**
sin depender de que el sorteo de banda sea reproducible (no lo es
necesariamente: es interno del paquete). `0` pre-Apply ⇒ no había caché ⇒ tras
Clear se re-resuelve fresco, como antes de la sesión. Invariante de runtime
completo: `SSoT_Runtime_CompositionSession_Integration.md` §8 inv 13.

**Doble plano de persistencia (a diferencia de CTX-1).** El BPM audible
persiste **solo**, porque el cortocircuito de caché lo sostiene mientras nada
invalide con `keepTempo: false`. `Hold across loops` no existe para eso: existe
para reafirmar la **verdad del modelo** cuando este revierte (mismo mecanismo no
identificado que CTX-1b), de forma que la línea de lectura no mienta y el
override sobreviva a una invalidación que sí re-resuelva. Loguea
`[CTX-2a] Model tempo drifted to …; re-asserting … (count=N)` — mismo
instrumento de diagnóstico que CTX-1b.

**Precedencia frente a cartas de tempo (verificada, ST-CTX2A-5/6).** Las cartas
de tempo son las únicas que invalidan con `keepTempo: false`
(`CompositionSession.ShouldKeepTempo`), así que fuerzan re-resolución:
- **ScaleFactor** (Push It ×1.5 / Half Time ×0.66): **compone** con el override
  — el BPM final es `override × escala`, porque `ExplicitBpm` sigue en el modelo
  y la escala se aplica después.
- **Range / AbsoluteBpm**: con `Hold` OFF manda la carta y el registro se suelta
  **sin** restaurar (semántica CSV-2: la verdad de la carta es más reciente, se
  anota `(superseded by card)`); con `Hold` ON el reassert devuelve el override
  dev. **Consecuencia documentada, no defecto**, idéntica a CTX-1b.

**Guardas.** Identidad de modelo (canción nueva ⇒ el registro se suelta, no hay
nada válido que restaurar) y pisado por carta con semántica CSV-2.
`Clear ALL overrides` limpia también esta familia — son ya **cuatro**
(patrón · instrumento · contexto de parte · tempo).

**Alcance.** `#if ALWTTT_DEV` a nivel de fichero, huella de producción cero,
**sin API nueva** (`TryGetPartCache` / `GetOrCreatePartCache` /
`PartCache.resolvedBpm` ya eran públicos). Ruta de producción equivalente: una
carta con `TempoEffect(AbsoluteBpm, N)` — ambas rutas escriben el mismo campo
del modelo.

**Deuda anotada, no arreglada aquí.** `SongCompositionUI.ApplyEffectToModel`
escribe `tempoRangeOverride = TempoRange.Fast` como centinela en la rama
`AbsoluteBpm` de `TempoEffect`. Tras D11=A ese centinela contradice el nuevo
default. Está ensombrecido por `ExplicitBpm` (§8 inv 13), así que es inerte hoy;
se vuelve visible si alguien limpia el `ExplicitBpm` de esa parte sin limpiar el
rango.

### 18.14 Articulation override — chordExpression / arpeggioRate (CTX-2b, 2026-08-03)

Cuarto override del tab, y el primero que **no** escribe un campo que
`SongConfigBuilder` ya lea. `chordExpression` y `arpeggioRate` no existen en el modelo
de composición: viven dentro del style bundle que la carta trae
(`BackingCardConfigSO` / `BasslineCardConfigSO`). El plano es por tanto distinto
(D-CTX2B-1=A) aunque la UI reutilice el patrón CTX-1 (steppers · Apply ·
Clear-con-restore · `Hold across loops`).

**Plano: clon en runtime.** Apply hace `Instantiate()` del bundle **original**, muta
los dos campos en la copia, marca la copia `HideFlags.DontSave` y la asigna a
`TrackEntry.styleBundle`. El asset del proyecto no se toca nunca, ni transitoriamente.
Participación en el hash: automática — `SongConfigBuilder.AssetKey` usa
`GetInstanceID()`, así que el clon tiene clave propia y el render es fresco por
construcción.

**Clon fresco por Apply (invariante del plano).** Mutar el clon vigente en sitio
conserva su instance ID ⇒ `trackInputsHash` no se mueve ⇒ la caché de bundles de
`MidiMusicManager` serviría bytes rancios: un segundo Apply cambiaría la UI sin cambiar
el audio, y la herramienta fabricaría conclusiones falsas sobre el composer. Cada Apply
clona **desde el original** (nunca desde el clon anterior: así todo campo no-articulatorio
sigue siendo verdad autorada por construcción) y destruye el huérfano. La destrucción es
inmediata y segura: los renders son síncronos al inicio de loop en el hilo principal,
`OnGUI` nunca se solapa con uno, y las cachés guardan bytes, no referencias a SOs.

**Clear recupera identidad de bytes.** Restaurar la referencia original devuelve el
instance ID original ⇒ la clave de bundle original ⇒ `bundle HIT` y replay del array
cacheado. No es "suena igual": son los mismos bytes. Verificado en ST-CTX2B-1.

**Hold es más estrecho que en CTX-1b (deliberado).** Hold re-asserta el clon **solo**
cuando el modelo revierte al bundle **original** registrado. Un bundle **ajeno** significa
carta de Backing/Bassline nueva y la carta gana siempre (semántica CSV-2: soltar sin
restaurar, destruir el clon), con Hold ON o OFF. Motivo: re-asertar sobre un bundle ajeno
resucitaría la identidad musical **entera** de la carta anterior (progresión, paleta,
instrumentación de estilo), mucho más de lo que un override de dos campos debe hacer.
En CTX-1 la asimetría es aceptable porque allí se reescriben dos campos escalares.

**Ciclo de vida de los clones.** Son los primeros `ScriptableObject` de runtime creados
por el tab; un SO instanciado no muere solo. Tres puntos de liberación: fin de canción
(rama de sesión inactiva), reconstrucción del modelo (guarda de identidad, patrón
CTX-2a), y pisado por carta. `Clear ALL overrides` limpia también esta familia — son
ahora **cinco** (patrón · instrumento · contexto de parte · tempo · articulación).

**Observables en la UI** (no solo en log, por hallazgo de ST-CTX2B-3: el volcado
`[stemCache][DIAG]` sepulta la línea de Apply en la consola):
- `id=` del clon vigente en la fila — cada Apply debe mostrar un ID **nuevo**.
- `live dev clones: N` — el contador de fuga.
- `[dev-artic]` como sufijo en la línea de INTENT, misma convención que `[dev-inst]`.

**Aviso F-ARTIC-RATE-RANDOM-1.** La fila avisa en amarillo cuando se combina figura
concreta con `arpeggioRate = Random` (ver §8.7 del boundary SSoT). La herramienta
**avisa pero no corrige el valor**: una carta real puede autorarse así, y auditar lo que
la carta hace de verdad es el propósito del tab. Retirar el aviso cuando MGP-ARTIC-RATE-1
cierre.

**Alcance de la sección.** Solo se listan tracks cuyo bundle vigente sea Backing o
Bassline —son los únicos que llevan los dos campos—; `RhythmCardConfigSO` no los tiene.

**Trampa de lectura del log (anotada aquí porque se tropezó con ella en este lote).**
El segmento `dp:<paleta>` de `partMeterHash` identifica la paleta por defecto que se
**ofrece** al canal de armonía compartida, **no** la que ganó. El veredicto lo da la
línea `[ORDER-1] harmony source=… asset='…'`, y el paquete además avisa explícitamente
cuando la ignora (`[SongOrchestrator] defaultProgression … Ignoring`). Ver
`MidiMusicManager` §D-R2-10=A para por qué el candidato está en la clave.

**Deuda anotada, inerte.** El warning `OnSongStarted but key/cache missing`
(`MidiMusicManager`, `LogWarning`) se observó durante los smokes de este lote. Sin
relación con la articulación; no investigado.

---

## 19. Niveles de log (LOG-1, 2026-08-08)

Esta sección existe por una razón concreta: **es lo único que impide que un lote futuro
degrade por descuido una línea de la que depende un test.** El riesgo no es teórico. Casi
todas las líneas protegidas de abajo *parecen* ruido de diagnóstico, y todas ellas son el
observable de una verificación. Antes de mover cualquier línea de log a un tier más
silencioso, mirar esta sección.

### 19.1 Los cinco interruptores

Cinco flags, tres dueños, dos planos. No forman una jerarquía única: hay dos maestros
independientes (host de gig y `MidiMusicManager`) más un tercero package-side.

| # | Flag | Dueño / hogar | Tier | Qué gatea |
|---|---|---|---|---|
| 1 | `GigDevSettingsSO.UseLogs` | ALWTTT, asset de gig | **maestro** | El log de gig en general. Con OFF no hay consola de gig que leer. |
| 2 | `GigDevSettingsSO.UseCompositionLogs` | ALWTTT, asset de gig | maestro de dominio | El flujo de composición dentro del log de gig. |
| 3 | `GigDevSettingsSO.LogVerbose` | ALWTTT, asset de gig | **verbose (D-LOG-3=B)** | Los volcados charlatanes por render / por loop cuyo hito ya cerró: smoke de S5a, `LoopCtx` de B3, bloques `DIAG` de caché, tablas de teoría. **Default OFF.** |
| 4 | `MidiMusicManager.logDebug` | ALWTTT, campo del componente | maestro del manager | Todo el log de `MidiMusicManager`. **`[HideInInspector]`**: el valor del inspector/prefab **no decide nada**, se sobreescribe incondicionalmente en el arranque desde `MidiGenPlayConfig.logMidiMusicManager`. Se ocultó precisamente para que deje de invitar a girar un mando que no está conectado. |
| 5 | `MidiMusicManager.logVerbose` | ALWTTT, campo del componente | **verbose (D-LOG-3=B)** | Segundo tier del manager, **host-owned y NO sobrescrito en el arranque** (a diferencia de `logDebug`). Solo los volcados por render. **Default OFF.** |
| — | `MidiGenPlayConfig.logGenerator` | **MidiGenPlay (package-owned)** | maestro package-side | El log del generador. Un único bit que contiene a la vez `[MelodySlot]` (una línea por nota) y `[ChordTrack] Tonality`, de la que dependen tests host. Ver §19.4 / boundary §8.10. |

**Regla de lectura.** Una línea verbose es visible **solo** cuando su maestro **y** su
verbose están ambos en ON (`UseLogs && LogVerbose`, o `logDebug && logVerbose`). Una línea
protegida cuelga **solo** de su maestro.

**Por qué dos pares maestro/verbose y no uno.** `GigDevSettingsSO` es un asset de contenido
de gig; `MidiMusicManager` es un componente que existe también fuera de un gig. Colgar el
verbose del manager de un asset de gig lo dejaría sin gobierno en cualquier escena sin gig,
que es exactamente donde se depura composición. El coste —dos mandos en vez de uno— es
menor que el de un mando que a veces no existe.

### 19.2 Las SIETE líneas PROTEGIDAS

Cada una es el observable directo de al menos un test. **Ninguna cuelga de un flag verbose.**
Degradar cualquiera de ellas rompe la verificación que la sostiene, y la rompe *en silencio*:
el test no falla, deja de poder ejecutarse.

| Línea | Sostiene | Por qué no se puede mover |
|---|---|---|
| `[ORDER-1] part=N harmony source=… asset='…'` | ST-R2d-1, ST-A1..A7, ST-J1/J6 | Es el **veredicto** de quién ganó el canal de armonía compartida. El segmento `dp:` del hash solo dice qué se *ofreció* (§18.14). Sin esta línea no hay forma de saber qué progresión sonó. |
| `[JAM-1] part=N imposing shared progression …` | ST-J1, ST-J2, ST-J4, ST-J5 | Único observable de que la imposición ocurrió. Su **ausencia** es igual de informativa que su presencia (ST-J6 verifica que no aparece). |
| `[JAM-2] part=N aligning render tonality X/root -> Y/root` | ST-J3, ST-J6 | Único observable de la propagación de modo. ST-J6 verifica su **ausencia** en renders que no imponen: una línea que se pueda silenciar por configuración vuelve inservible un test de ausencia. |
| `[B1][stemCache] …` **SIN** `[DIAG]` | ST-CTX2B-1, ST-C1, C4, tests de identidad de bytes | `bundle HIT` es el detector de identidad de bytes del proyecto (§9.19, nota de instrumentación). Sin él no hay forma barata de afirmar "son los mismos bytes" en vez de "suena igual". |
| `[DBG-C2/CacheBypass] …` | ST-J4, ST-CTX2B-2/3 | Explica **por qué** un render fue fresco. Sin ella, un render fresco y un fallo de caché son indistinguibles en la consola. |
| `[ChordTrack] Tonality: …` | ST-A7, ST-J3, evidencia de la corrección de premisa de D-R3C-3 | Es la prueba de que el render ocurrió en el modo que se cree. **Package-owned** — ver la trampa de §19.4. |
| `Timeline ch=…` | ST-LOG-2, C4, comparación de timeline de acordes | Reporta el timeline de acordes por canal. Es donde se leyó la etiqueta de acorde dañada que originó MGP-CHD-ASCII-1. |

> **Contador.** Son **siete** líneas protegidas en total. Los comentarios de código hablan de
> **seis** porque cuentan solo las de dueño host: `[ChordTrack] Tonality` es package-owned y no
> cuelga de ningún flag ALWTTT. No es una contradicción, es una diferencia de alcance — y es
> exactamente el motivo por el que existe el ask **MGP-LOG-VERBOSE-1**.

### 19.3 LA TRAMPA: `[B1][stemCache]` vs `[B1][stemCache][DIAG]`

**Son dos líneas distintas con tiers distintos, y un `grep "[B1][stemCache]"` las confunde.**

- `[B1][stemCache]` — **PROTEGIDA.** Reporta `bundle HIT` / `stem HIT` y las claves. Cuelga
  de `logDebug` solo.
- `[B1][stemCache][DIAG]` — **VERBOSE.** Volcado de diagnóstico por parte, largo. Cuelga de
  `logDebug && logVerbose`. Es el volcado que en ST-CTX2B-3 sepultaba la línea de Apply en la
  consola, y el motivo de que §18.14 exponga observables en la UI y no solo en log.

Un lote que quiera silenciar "el volcado de stemCache" y filtre por el prefijo común apagará
también la línea protegida. **Filtrar por `[DIAG]`, nunca por `[B1][stemCache]`.** El código
lleva un comentario en el punto exacto (`MidiMusicManager.cs`, ~1006) avisando de esto; esta
sección es su autoridad documental.

### 19.4 Trazas de pila desactivadas

`AlwtttLogSetup` (`ALWTTT.Core`, `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`) llama a
`Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None)` antes de que cargue
ninguna escena.

**Medida sobre la captura de referencia de R3 (`log11.txt`): 762 de 864 líneas de consola eran
trazas de pila colgando de `Debug.Log` planos** — el 88% del volumen sin información, porque
toda línea de log del host ya va etiquetada con la clase que la emite y la traza solo repite lo
que dice la etiqueta.

- **Warning y Error conservan su traza.** Ahí sí se quiere: saltan en rutas que no se esperaba
  alcanzar.
- **No retira ni degrada ninguna línea de log.** Es el único cambio de LOG-1 con coste de
  información **cero**, por eso va primero y se mide solo, antes de degradar nada a verbose.
- **Escape:** `AlwtttLogSetup.SetPlainLogTraces(true)`, o el menú de editor
  **`ALWTTT/Debug/Log stack traces/Enable`**. Es global y dura la sesión de juego. Usarlo
  cuando haga falta localizar de dónde sale un `Debug.Log` sin etiquetar.

### 19.5 Qué se retiró y qué NO

- **Los dos logs `[F-4]` retirados** (`CompositionSession.cs`, `MidiMusicManager.cs`): el
  volcado de forma de la llamada de frontera y el volcado de ENTRY por render. El propio
  comentario del código decía "Removed at F-4 closure" y F-4 está cerrado.
- **El `try/catch` de F-4 Stage A queda INTACTO, y su volcado de error CONSERVADO.** Lo que se
  retiró son las líneas *tageadas* `[F-4]`, no la defensa. El comentario del código lo declara
  permanente. **Retirar el try/catch al leer esta sección sería un error de lectura**: el log
  era el andamio, el catch es la estructura.
- `SongConfigBuilder.Log` pasa a estar gateado (antes no tenía gate alguno).
- Gateados también los logs de `DeckManager` / `HandController` / `GameManager`.

### 19.6 Regla de actualización

Antes de mover una línea a verbose o de retirarla:

1. Comprobar si aparece en la tabla de §19.2. Si aparece, **no se mueve** sin retirar o
   reescribir antes el test que la usa, y sin actualizar esta sección y `coverage-matrix.md`.
2. Si es nueva y algún smoke la va a usar como observable, **añadirla a §19.2 en el mismo
   lote** que el test. Una línea protegida sin entrada aquí es una línea que el siguiente
   lote apagará.
3. Los volcados nuevos por render / por loop nacen **verbose por defecto**.

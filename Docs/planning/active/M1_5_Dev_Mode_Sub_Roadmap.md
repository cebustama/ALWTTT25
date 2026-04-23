# M1.5 Dev Mode — Sub-Roadmap

**Status:** Active (2026-04-16)
**Parent:** Roadmap_ALWTTT.md §1.5
**Authority:** This is a planning document. Implementation truth lives in `SSoT_Dev_Mode.md`.

---

## Goals

Dev Mode is the critical path for moving Combat MVP from "technically works" to "QA-ready."
Without it there is no practical test loop for card balance, composition iteration, audience tuning, or multi-turn status validation.

Concrete goals:
1. Enable infinite-song playtesting — no forced gig end, convinced audience resets.
2. Spawn any authored card into the live hand from a full project catalogue.
3. Edit all gameplay meters and per-character stats at runtime.
4. Make audience actions fully transparent (what happened, to whom, for how much).
5. Validate the normal composition card pipeline through Dev Mode before designing mid-loop injection (M2 unblocker).

---

## Design decisions (resolved)

### D1. Panel architecture
**Chosen:** Runtime overlay component on the existing Gig scene, gated by `ALWTTT_DEV` scripting define, toggled with F12.

Rejected alternatives:
- Duplicate scene (`GigScene_Dev`): forks encounter/deck wiring, doubles maintenance.
- EditorWindow: dies in builds, blocks non-developer playtesters.

### D2. Injection surface / build isolation
**Chosen:** Dedicated assembly (`Assets/Scripts/DevMode/ALWTTT.DevMode.asmdef`) with `defineConstraints: ["ALWTTT_DEV"]`. One-way dependency on the main ALWTTT asmdef. Manager-side additions are `internal` methods guarded by `#if ALWTTT_DEV`.

Release build is byte-identical to today when the define is absent.

### D3. Composition testing
**Chosen:** Staged approach.
- Phase 2 validates normal pipeline (spawn composition card → drop in zone → observe).
- Phase 5 adds live mid-loop injection (`CompositionSession.DevInjectCompositionCard`) after observing real session behavior.

### D4. Infinite turns (refined from user input)
"Infinite turns" means:
- No forced gig end after N songs.
- Convinced audience members reset to 0 persuasion and keep acting.
- The gig runs until the player explicitly stops it.

Implementation: guard in `ResolveGigOutcomeAndEnd` + auto-reset of `IsConvinced`/persuasion on audience members at each new song or turn boundary.

### D5. Card catalogue source
No global runtime card registry exists. Dev Mode uses `AssetDatabase.FindAssets("t:CardDefinition")` (same pattern as DeckEditorWindow.RefreshCatalogue). Extracted into `DevCardCatalogueCache`.

### D6. Encounter modifiers — Phase removed
`GigEncounterSO` has no modifier system. The roadmap item "toggle encounter modifiers on/off" has no current implementation. **Phase removed from M1.5.** If a modifier system is added later, it gets its own Dev Mode panel.

### D7. UI framework
IMGUI (`OnGUI`). Functional first, polish later.

---

## Architecture

```
Gig scene (runtime)
├── GigManager ────────────┐
├── DeckManager ───────────┤
├── HandController ────────┤  ← DevModeController reads & mutates
├── CompositionSession ────┤    via internal/guarded seam methods
├── CurrentMusicianCharacterList
└── CurrentAudienceCharacterList

[ALWTTT_DEV only]
DevModeController (MonoBehaviour, persistent in Gig scene)
├── Toggle (F12) → shows/hides DevModeOverlay
├── InfiniteTurnsEnabled → intercepts gig end + resets convinced audience
├── DevCardCatalogueCache (AssetDatabase.FindAssets)
└── Forwarding API:
        SpawnCardToHand(CardDefinition)
        SetInspiration(int), SetCohesion(int), SetSongHype(float)
        SetMusicianStress(m, int), SetMusicianMaxStress(m, int)
        SetStatusStacks(character, StatusEffectSO, int)
        SetAudienceVibe(a, int), SetPersuasionProgress(a, int)
        ResetConvincedAudience()
        [Phase 5] InjectCompositionCardLive(CardDefinition)

DevModeOverlay (IMGUI root)
├── Meter sliders (Inspiration, Cohesion, SongHype)
├── Character panels (per musician / per audience: stats + status stacks)
├── Card catalogue browser (search, action/composition filter, spawn button)
├── Audience reaction log (event stream from GigManager)
├── Infinite turns checkbox + convinced-reset indicator
└── [Phase 5] Composition injection tab
```

### Injection seams

| Seam | Owner | Phase | Guard |
|---|---|---|---|
| `GigManager.DevInfiniteTurnsEnabled` + guard in `LoseGig`, `ResolveGigOutcomeAndEnd` | GigManager | P1 | `#if ALWTTT_DEV` |
| Audience convinced-reset (reset persuasion to 0 on new song when infinite) | GigManager | P1 | `#if ALWTTT_DEV` |
| `HandController.DevAddCardToHand(CardDefinition)` | HandController | P2 | `internal` |
| `DeckManager.DevSpawnIntoHand(CardDefinition)` | DeckManager | P2 | `internal` |
| `GigManager.DevSetInspiration/Cohesion/SongHype` | GigManager / PersistentGameplayData | P3 | `internal` |
| Per-character stat setters (Stress, MaxStress, Vibe, persuasion) | BandCharacterStats / AudienceCharacterStats | P3 | `internal` |
| `GigManager.AudienceActionResolved` event | GigManager | P4 | `#if ALWTTT_DEV` |
| `CompositionSession.DevInjectCompositionCard` | CompositionSession | P5 | `internal` |

---

## Phases

### Phase 0 — Scaffolding & SSoT skeleton
- Create `Assets/Scripts/DevMode/ALWTTT.DevMode.asmdef` (defineConstraints: `ALWTTT_DEV`).
- Create `DevModeController.cs` (empty MonoBehaviour).
- Create `Docs/systems/SSoT_Dev_Mode.md` (skeleton).
- Register in `SSoT_INDEX.md`, `coverage-matrix.md`.
- Verify project compiles with and without `ALWTTT_DEV`.

**DoD:** Empty asmdef compiles. SSoT skeleton exists and is registered.

### Phase 1 — Infinite turns + overlay toggle
- `DevModeController`: F12 toggle, IMGUI overlay header.
- "Infinite turns" checkbox: suppresses `LoseGig()`, suppresses `ResolveGigOutcomeAndEnd()`, resets convinced audience to 0 persuasion at new song boundary.
- Visible "INFINITE MODE" indicator in overlay.

**DoD:** F12 shows overlay. Toggling infinite turns lets the gig run past BandCohesion ≤ 0 and past the final song. Convinced audience members reset and keep acting.

**Smoke tests:** see Phase 1 batch.

### Phase 2 — Card spawner
- `DevCardCatalogueCache`: wraps `AssetDatabase.FindAssets("t:CardDefinition")`.
- Overlay panel: search box, action/composition filter, scrollable list, "Spawn → Hand" button.
- `HandController.DevAddCardToHand` + `DeckManager.DevSpawnIntoHand` (updates HandPile + pile count HUD).

**DoD:** Any authored card can be spawned into the current hand and played normally. Verified for both action and composition cards.

**Smoke tests:** see Phase 2 batch.

### Phase 3 — Stat editors
- Overlay "Meters": Inspiration, BandCohesion, SongHype — sliders + text input.
- Overlay "Musicians": per-musician Stress / MaxStress / status-stack ±.
- Overlay "Audience": per-audience Vibe / persuasion / status-stack ±.
- Mutations go through existing APIs (`StatusEffectContainer.Apply/RemoveStacks`, `BandCharacterStats` setters) so events fire normally (icons update, HUD updates).

**DoD:** Every meter and per-character stat can be read and written from overlay; mutations propagate through normal events.

### Phase 4 — Audience reaction transparency
- `GigManager.AudienceActionResolved` event (struct payload: audience name, action name, target name, effective value).
- Overlay "Audience log": scrollable, color-coded per audience member.
- Step mode: pause between audience actions (`WaitUntil` behind `ALWTTT_DEV`).

**DoD:** Every audience action visible in order with target and value. Step mode allows pause/resume.

### Phase 5 — Composition card live injection (M2 unblocker)
- `CompositionSession.DevInjectCompositionCard(CardDefinition)`.
- Designed after Phase 2 normal-path observations.
- Overlay "Composition" tab: spawn composition card mid-loop, observe mutation.

**DoD:** Composition card played mid-loop causes audible change without crashing session. Scoped to validating mutation, not full mid-loop rules (M2).

---

## File placement

```
Assets/Scripts/DevMode/
├── ALWTTT.DevMode.asmdef
├── DevModeController.cs
├── DevModeOverlay.cs
├── Panels/
│   ├── DevMetersPanel.cs
│   ├── DevCharactersPanel.cs
│   ├── DevCardSpawnerPanel.cs
│   ├── DevAudienceLogPanel.cs
│   └── DevCompositionPanel.cs       (Phase 5)
└── Services/
    ├── DevCardCatalogueCache.cs
    └── DevAudienceLog.cs

Existing files (additive internal methods):
├── GigManager.cs
├── DeckManager.cs
├── HandController.cs
├── BandCharacterStats.cs
├── AudienceCharacterStats.cs
└── CompositionSession.cs            (Phase 5)

Docs:
├── systems/SSoT_Dev_Mode.md
├── planning/active/M1_5_Dev_Mode_Sub_Roadmap.md  (this file)
```

---

## Risks

- **Phase 5 depends on M2 observations.** Accepted; DoD is deliberately narrow.
- **Status-stack mutation timing.** Dev Mode mutations run on main thread between frames; `StatusEffectContainer` is already main-thread-only. Low risk.
- **Infinite-turn mode invalidates some gig-loop invariants** (e.g., "encounters always end"). Smoke tests after Phase 1 must confirm no null-refs on repeated audience turns.
- **M1.2 deferred smoke tests (T4, T5, T7, T8, T9)** are load-bearing. If any reveals a Combat MVP bug, M1.5 adds a Phase 1.5 for the fix.
- **`DeckData.cs` / `BandDeckData.cs` coexistence.** Not a Dev Mode problem but noted for M1.1 hygiene.

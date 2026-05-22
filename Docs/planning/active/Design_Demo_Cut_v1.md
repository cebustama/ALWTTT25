# Design — Demo Cut v1

**Status:** planning, §5.3.5 closed 2026-05-18. **Not frozen** — coverage matrix in §2 reflects intended demo coverage at §5.4 entry; B3 remainder (B3-content-sibi / B3-content-cards / B3-slate / B3-balance / B3-validation) must land before §5.4 opens per D-B3-Remainder=A. Refreezes at §5.4 entry pending ST-DCP-S5 absorption (DC-Close-S5=(c) DEFERRED).
**Scope:** ALWTTT demo build, shipped via §5.3.5 Demo cut prep.
**Authority:** planning-only. Does not promote concepts into living
governance. Implemented truth remains in `CURRENT_STATE.md` + system SSoTs.

---

## 1. Demo cut overview

The demo build is a single-encounter gig that auto-launches from the Main
Menu with zero setup interaction. The player sees Main Menu → Start →
(invisible GigSetup pass-through, ~1 frame) → Gig action window 1.

### 1.1 Run shape

| Knob | Value | Source |
| --- | --- | --- |
| Band | C2 + Sibi (musicians authored in earlier B-phase) | `DemoLaunchConfig.bandRoster` |
| Audience | 2× Kid + 1× Cool Dude | encounter asset (B3 #12) |
| Songs to win | 4 | `DemoLaunchConfig.requiredSongCount` |
| Parts per song | 1 | encounter song shape |
| Loops per part | 4 | `GigFlowSettings.jamRules.loopsPerPart` |
| Initial inspiration | 3 | `DemoLaunchConfig.initialGigInspiration` (DC-4=B) |
| Inspiration per loop | 1 | `DemoLaunchConfig.inspirationPerLoop` (DC-4=B) |
| SFX→FlatVibe bonus (stage 1/2/3) | 3 / 6 / 10 | `GigPresentation.sfxBonusVibeStage{1,2,3}` (D-DCP-2=A) |

### 1.2 Entry path (D-FAST-1=C)

```
[App launch]
   ↓ load build index 0
[Main Menu scene]
   ├─ Start button → MainMenuController.OnStartPressed
   │   ├─ TryAutoLaunch (AutoStartFromDefaults + valid DemoLaunchConfig)
   │   │   → DemoLaunchConfigSO.ToRunConfig → GigLauncher.Launch
   │   │   → SceneChanger.OpenGigScene
   │   └─ fallback → SceneChanger.OpenGigSetupScene (manual path)
   └─ Quit button → MainMenuController.OnQuitPressed → UIManager.QuitGame
       ↓ (auto-launch branch)
[Gig scene] (build index 2)
   action window of song 1
```

Net timing: 2 fade cycles, ~2s at fadeSpeed=1. No GigSetup waypoint
on the auto-launch path, no flicker. fadeSpeed inspector value can be
bumped (5 → ~0.4s, 10 → ~0.2s) without affecting structure.

ESC routing (unchanged from B3-demo-polish F3+F8):
- ESC in Gig → MainMenu.
- ESC in MainMenu → QuitGame.

The three-scene architecture explored during the §5.3.5 implementation
was discarded in favour of two scenes when the ladder-mode use case
clarified that the auto-launch pattern needs its own home (`GigLauncher`)
independent of the GigSetup scene. See §3.2.

---

## 2. Coverage matrix

Two purposes: confirm the demo exercises a meaningful breadth of the
authored content, and surface what is intentionally NOT exercised (for
post-demo content batches to target).

**ST-DCP-S5 deferral (DC-Close-S5=(c), 2026-05-18).** The "verify" cells
and validation-owner notes below were originally scoped to ST-DCP-S5
during §5.3.5 closure. Per DC-Close-S5=(c), ST-DCP-S5 is **deferred to
§5.4 readiness review** and folded into §5.4's full clean-run smoke
pass (R1 sub-item). Coverage cells remain `[verify]` / `✓` pending that
absorption. Final ratios may shift after §5.4 playthrough data lands.

**Coverage-vs-reality note (D-B3-Remainder=A, 2026-05-18).** The §5.3.5
batch landed before B3 closed (only B3-content-audience + B3-demo-polish
shipped; B3-content-sibi / B3-content-cards / B3-slate / B3-balance /
B3-validation queued). Several cells below describe content that is
*planned to land before §5.4 opens* but is **not present in the current
build**: BPM cards (#10), Modulation cards (#11), Sibi's `InstrumentEffect`
on Singing Field (#11.5). Those cells are marked `pending-B3` (was
aspirational `✓`). The matrix represents the **intended demo coverage at
§5.4 entry**, not the current §5.3.5-close coverage. Final ratios resolve
when B3 closes.

### 2.1 CardEffectSpec families — target 4/4 = 100%

| Family | Demo coverage | Source content |
| --- | --- | --- |
| ModifyStress | ✓ | Starter deck — verify |
| ModifyVibe | ✓ | Starter deck — verify |
| DrawCards | ✓ | Starter deck — verify |
| ApplyStatusEffect | ✓ | Starter deck — verify (Composure / Hyped applications) |

Validation owner: complete the "verify" cells during ST-DCP-S5 with the
actual card names from the starter deck (auto-assembled from C2 + Sibi
catalogues + GigSetupRoster.GenericStarterCatalog).

### 2.2 StatusEffect families — target 5/7 ≈ 71%

| Family | Demo coverage | Path |
| --- | --- | --- |
| Composure (musician) | ✓ | Starter deck applies; baseline Stress-mitigation |
| Exposed (musician) | ✓ | Cool Dude's Heckle ability applies (B3 #12) |
| Hyped (musician) | ✓ | Kid's Egged On ability applies (B3 #12) — verify path |
| Heckle (audience) | ✓ | Cool Dude (B3 #12) |
| Indifference (audience) | ✓ | Cool Dude conditional application (B3 #12) |
| Earworm (audience) | ✗ | Deferred — single-encounter demo doesn't reach it |
| (7th — verify) | ✗ | Deferred |

The 5/7 target matches the §5.3.5 roadmap entry: "+3 over pre-batch via
Cool Dude's Heckle + Indifference + Kid's Egged On". The 2 not exercised
are content-side deferrals, not implementation gaps.

Validation owner: name the 7th family during ST-DCP-S5 (likely Tantrum or
a buff-side variant of Egged On). Update this row when confirmed.

### 2.3 PartEffect families — target 3/4 ≈ 75% (at §5.4 entry, after B3 closes)

| Family | Demo coverage | Path |
| --- | --- | --- |
| Meter (Stress/Vibe/etc.) | ✓ | Card effects + audience reactions |
| Tempo | ✓ | Push It (×1.5), Half Time (×0.66) via existing `TempoEffect.ScaleFactor`. |
| Tonality (modulation) | ✓ | Key Lift via existing `ModulationEffect.IntervalWithinScale` degree=5. |
| Modal contrast (Wormus pair) | ✓ | Wormus Minor + Wormus Major (×2 each per D-STARTER-1=B), ~71% draw on opening hand. |
| Instrument | ✅ shipped | Sibi pool active via `MusicianProfileData` SO whitelist. Lead = [Fantasia, 5th Saw Wave, Soundtrack]; backing pool currently empty (authoring deferred — empty-list discipline preserves existing `InstrumentType`-filter behavior). Carrier `InstrumentEffect_Sibi_Voice.asset` retained as override exemplar at `Assets/Resources/Data/Cards/Composition/_PartEffects/`. |

Target 3/4 at §5.4 entry assumes B3-content-cards lands BPM (#10) and
Modulation (#11), making Tempo and Tonality reachable through the
starter deck during demo play. Instrument coverage depends on whether
the Sibi `InstrumentEffect` SO from B3-content-sibi is wired onto a
starter card that the demo actually plays (`Singing Field` is Sibi's
identity melody card and IS in the starter deck per
`Design_Starter_Deck_v1.md §4` — so once B3-content-sibi lands, this
flips to `✓` and the family ratio updates to 4/4).

Validation owner: re-evaluate cells when each B3 sub-batch closes;
final state captured by §5.4 ST-DCP-S5 playthroughs.

**Known interim limitations.**

**Modulation direction (MGP-ALWTTT-MOD-DIR-1).** Modulation cards in the current build land at a non-deterministic octave for the new root. `ModulationEffect` shifts the pitch class of `PartConfig.RootNote`; the octave is chosen by `ChordTrackComposer`'s voice leader, which minimizes voice-leading distance and routinely picks the descending neighbor for "up" modulations. A single Key Lift play may sound like an ascent, a settle, or a sidestep — interpreted as musical variety for the demo. Cross-project ask filed in MidiGenPlay tracker.

---

## 3. Mechanics introduced or modified

### 3.1 SFX → FlatVibe bonus (new)

Three new tunable values on `GigPresentationSO`:
- `sfxBonusVibeStage1` (default 3) — fires alongside the "lights" SFX tag.
- `sfxBonusVibeStage2` (default 6) — fires alongside the "smoke" SFX tag.
- `sfxBonusVibeStage3` (default 10) — fires alongside the "fire" SFX tag.

Hook site: `GigManager.FireSongHypeStage(int stage, string sfxTag)`
calls `ApplySfxBonusVibe(stage)` immediately after `backgroundContainer.ActivateSFX(sfxTag)`.

Routing (per DC-SFX-Route=A):
- Each audience member receives `+N` Vibe through `AudienceStats.ApplyIncomingVibe`.
- Indifference stacks block per-member (consistent with D-DCP-6=A: Indifference blocks ALL incoming Vibe).
- A single band-canvas "+N Vibe!" floater spawns at `sfxBonusVibeTextSpawnRoot` (with first-musician-TextSpawnRoot fallback), warm-gold colour (1, 0.85, 0.25) to visually distinguish from the per-audience cyan Vibe floaters.
- Floater suppressed if every audience member blocks the bonus (avoids misleading "+N Vibe!" with zero applied).

Bonus value is FLAT — does not pass through the Flow multiplier. The
"post-Flow" wording in the spec refers to bonus magnitude (no Flow scaling),
not temporal ordering.

### 3.2 Launch architecture — GigLauncher (D-FAST-1=C)

`GigLauncher.Launch` is the single non-Gig→Gig scene transition entry
point. Three callers (one present, one new this batch, one planned):

1. `GigSetupController.OnStartPressed` (manual picker UI path).
2. `MainMenuController.OnStartPressed` (auto-launch path, demo cut).
3. Future `LadderRunner` (multi-encounter mode, post-§5.3.5).

GigLauncher absorbs the launch tail that previously inlined in
GigSetupController.OnStartPressed: SetBandRoster (optional —
`bandRoster: null` preserves current pd.MusicianList for ladder carry-
over), GigRunContext.BeginRun, PD.ApplyRunConfig, IsFinalEncounter
assignment, SceneChanger.OpenGigScene. Stateless; bool return signals
launch dispatched / aborted.

`DemoLaunchConfigSO.ToRunConfig(returnDestination)` is the
SO-to-RunConfig conversion that auto-launch and ladder share. Manual
GigSetup builds its RunConfig from picker UI state instead.

F9 ad-hoc surfaces removed wholesale by this architecture pivot:
- `GigSetupController.autoStartOnLoad` SerializeField — gone (replaced by `GigDevSettingsSO.AutoStartFromDefaults` consulted from `MainMenuController`).
- `GigSetupController.AutoStartRoutine` coroutine (F9 version) — gone (launch tail extracted to `GigLauncher.Launch`).
- `UIManager.SkipAutoGigStart` static flag — gone (ESC routes to MainMenu; no auto-start-suppression path to defend).

### 3.3 Action-card mid-performance unblock (§5.3.5 demo unblock)

`GigManager.CanPlayActionCard` performance gate relaxed from
`flow.AllowActionCardsDuringPerformance && actionTiming == CardActionTiming.Always`
to just `flow.AllowActionCardsDuringPerformance`. Demo enables the
flag via `GigFlowSettings.allowActionCardsDuringPerformance = true`,
which lets per-loop-drawn action cards (regardless of timing tag) be
played during composition. Without the relaxation, mid-composition
draws of non-Always-tagged action cards stranded in hand — the demo
loop's "draw cards mid-composition" play pattern requires the broad
gate.

`CardActionTiming.Always` enum value retained for future precision-
gating needs; does not load-bear in the current gate logic.

---

## 4. Demo-build constraints & known acceptable rough edges

| Constraint | Rationale |
| --- | --- |
| Single encounter — no meta-progression | Demo scope. Multi-encounter wiring deferred to post-§5.4. |
| `PersistentGameplayData.IsFinalEncounter = true` forced after auto-start | Routes WinGig to WinPanel (Retry/Exit) instead of mid-run RewardCanvas → ReturnToMap. Same hack OnStartPressed already applies (B3-demo-polish A6). Removed when meta-progression sectors land. |
| Audience hover outline not rendering | Pre-existing bug. Cosmetic; not blocking. Polish-sweep deferred. |
| Kid Tantrum AnimatorTrigger never fires | `AbilityRoutine` doesn't consume `NextAbility.Animation.AnimatorTrigger`. Cosmetic; ability still resolves mechanically. Polish-sweep deferred. |
| Indifference + Hyped icon sprites unassigned | Statuses functional; icons missing on the catalogue assets. Asset wiring task, not code. Polish-sweep deferred. |

---

## 5. Validation & tuning

Win-rate target: 60–80% across 8–10 playthroughs (DC-3+DC-4 tuning baseline).

Tuning knobs available without touching code:
- `DemoLaunchConfig.initialGigInspiration` / `.inspirationPerLoop` — economy floor.
- `GigPresentation.sfxBonusVibeStage{1,2,3}` — late-game push.
- `GigFlowSettings.jamRules.loopsPerPart` — per-song length.
- `DemoLaunchConfig.requiredSongCount` — gig length.

Tuning knobs requiring code or balance batch:
- Card effect magnitudes (would touch starter deck card definitions).
- Audience archetype thresholds (would touch Kid / Cool Dude data).

---

## 6. Closure handoff

This doc freezes at §5.3.5 close. Updates after that go into a
`Design_Demo_Cut_v2.md` or are absorbed into demo-readiness review
(§5.4). The coverage matrix is the live deliverable until §5.4 confirms
or rejects the demo cut.

Post-§5.4 path:
- **If §5.4 passes:** demo cut tagged, this doc frozen, Phase B closes.
- **If §5.4 fails:** specific failure routes to a targeted §5.3.6 batch; this doc is updated only if the failure invalidates a coverage claim.

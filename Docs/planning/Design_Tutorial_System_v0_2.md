# Design_Tutorial_System_v0_2 — ALWTTT

**Status:** **S4 reactive system implemented (2026-06-17); superseded as the gig‑1 primary path by the guided curriculum (TUT‑REBUILD, 2026‑07‑08/09).** The reactive system is retained as a fallback and for post‑song‑1 coverage of the two beats with no guided equivalent (D3=B). Guided infra landed in **TUT‑R2 / R2b / R2c** (driver, input gates, scripted draw, holdLoop, D8 tokens, `MusicianStressHitEvent`, `AudienceBlockedEvent`, pulse highlights, `GigPresentationSO` telegraph toggles); **TUT‑R3** retires the 9 superseded reactive ids, runs the copy pass, and adds the highlight registry / world→screen pass. TUT‑R2 acceptance (CT1–CT6 + RT1–RT8 + ST1–ST12) was green; TUT‑R3 re‑runs ST11/ST12 after the retirement.
**Scope:** Tutorial system for ALWTTT — demo cut (guided gig‑1 curriculum + retained reactive layer) + vertical slice (S6–S8).
**Classification:** `reference (planning)` — **not a SSoT**. Becomes runtime‑authoritative when shipped; retained as historical rationale thereafter. **Copy authority note:** the authoritative, localized dialog text lives in the `TutorialDialogCatalog` seeders / `.asset` files (code truth). EN copy reproduced in §6B is the design‑approved reference; on divergence, code wins.
**Created:** 2026-05-23 · **v0_2:** 2026-07-09 (TUT‑REBUILD).
**Placement:** `Docs/planning/active/Design_Tutorial_System_v0_2.md` — moved there
2026-08-08 (MANIFEST-1, D11=A) from `Docs/planning/`. Its predecessor
`Design_Tutorial_System_v0_1.md` was archived to `Docs/planning/archive/` in the
same pass: both files had been live at once, with the successor sitting in the
*less* active folder (manifest finding **F17**). This document is the single
tutorial design home.
*History: v0_1 updated 2026-06-16 (TUT‑JAM‑SEQ), 2026-06-17 (S4 closure), 2026-07-04 (S5f localization).*

**Rule:** Tutorial‑as‑mandatory is Standing Directive #3 (`planning/Design_Project_Directives_v0_1.md §D3`). Every demo‑cut feature MUST have tutorial coverage; every Phase C feature by S8 closure. This doc defines the system that makes the rule implementable.

---

## 1. Purpose

The 2026-05-23 planning reframe surfaced a demo‑cut blocker: playtest feedback indicated the rules feel too complex without explanation. The band‑manager character (D‑RUN‑5) is the diegetic voice.

v0_1 shipped a **reactive** first‑time system (D‑TUT‑1..11, D‑S4‑\*, S5f localization). TUT‑REBUILD (2026‑07‑08/09) replaces the gig‑1 experience with a **driver‑sequenced guided curriculum** — a deliberate, teacher‑paced arc over the first song — while keeping the reactive machinery as a fallback. This doc captures both: the historical reactive design (§6, retained rationale) and the guided curriculum that supersedes it as the gig‑1 primary path (§6B).

---

## 2. Scope

### Gig‑1 primary path = the guided curriculum (§6B)

The first gig runs a **10‑beat guided curriculum** (+ `tut_composure` + a rewritten reactive layer), driver‑sequenced over the demo's first song, layered **over** the retained reactive catalog/HashSet (D3=B). **D‑TUT‑1 "basic mechanics only" still governs the copy** — the curriculum teaches the demo's core loop, not every system; the authoring discipline (few beats, sharp text) binds every line.

> **D‑S5f‑4 = B is SUPERSEDED.** v0_1 §5A deferred the guided/scripted tutorial to the vertical slice ("demo ships with the reactive S4 trigger system"). TUT‑REBUILD pulled the guided path forward into the **demo cut**. The reactive system is not removed — it is demoted to fallback + post‑song‑1 coverage.
>
> **D‑S5f‑4‑adjacent (S5f/S6 scope):** the guided curriculum being demo‑cut content also supersedes any prior assumption that live scripted pacing was a vertical‑slice‑only concern.

### In scope for demo cut
- The guided gig‑1 curriculum (§6B): 10 beats + `tut_composure` + rewritten reactives (status buff/debuff, Blocked, breakdown, gig won/lost) + the `tut_play_finisher_early` degrade variant.
- The retained reactive layer: `tut_first_sfx_stage`, `tut_first_sound_card` (no guided equivalent — see §6), plus `tut_first_reward_choice` (S5h, separate lifecycle).
- Skip mid‑dialogue (D‑TUT‑2); revisit from the **Main Menu** (D7=A, §5).
- ES + EN dual catalog (D‑S5f‑2=B).

### In scope for vertical slice (S6–S8, D‑TUT‑5)
- ~5 additional dialogs for run‑structure mechanics (ship hub, reward, audience state machine, first boss, run complete) — same infra, new ids + assets. See §7.

### Out of scope (deferred)
- Multi‑step branching beyond the driver's linear arc; conditional/state‑dependent paths beyond first‑time + the gig‑1 sequence; localization infra beyond the dual catalog; in‑game encyclopedia; audio narration; per‑mood portrait variants.

### Extensible‑infra principle (D‑TUT‑1)
The trigger model (§3) and presentation (§4) admit new dialogs by SO‑authoring / seeder extension without code change for **reactive** beats. The **guided** curriculum is a scoped exception (D‑TUT‑6 lineage): it adds a driver + gates as bespoke sequencing, justified by the jam being the core mechanic taught in layers. New guided beats are a driver change; new reactive beats are not.

---

## 3. Trigger model

### 3.1 Reactive layer (D‑TUT‑3 = first‑time HashSet)
- `TutorialController` holds `HashSet<string> firedDialogs`, in‑memory + serialized via `PersistentGameplayData` for cross‑session persistence.
- Each dialog has a unique author‑stable string id (`TutorialTriggerId` constants). Renaming an id retires it from the player's set and re‑fires — an authoring escape hatch, not to be exercised without intent.
- On each candidate trigger: `firedDialogs.Contains(id)` → skip silently if true; else show + add. The controller consumes Sensory Event Bus events (`CardPlayedEvent`, `StatusAppliedEvent`, `LoopResolvedEvent`, `SfxStageCrossedEvent`, `SongEndVibeEvent`, `AudienceTurnStartedEvent`, `GigStartedEvent`, `GigOutcomeEvent`, `RewardChoiceOpenedEvent`, and — TUT‑R2 — `MusicianStressHitEvent`, `AudienceBlockedEvent`).

### 3.2 Guided layer (TUT‑R2 / D3=B) — driver over the HashSet
The gig‑1 curriculum is a **sequence driver** (`TutorialGuidedDriver`) layered **over** the reactive catalog/HashSet. D‑TUT‑3 is **not** retired: the guided ids are ordinary `firedDialogs` entries; the driver adds sequencing, gates, and a scripted hand.

- **Runtime suppression → retirement.** During TUT‑R2/R2b/R2c the driver suppressed the superseded reactive ids (`SetSuppressedTriggers`) so the guided beats owned gig‑1. **TUT‑R3 formally retires those 9 ids** (constants + assets + call sites), so suppression is no longer needed and `SupersededIds` is emptied — the reactive duplicates can no longer re‑teach post‑song‑1 concepts the curriculum already covered.
- **ScriptedDrawQueue (D1=B, seam M4.5).** `TutorialScriptedDrawQueue` seeds specific cards via the M4.5 draw seam: the forced initial hand (beat 2) and the scripted finisher draw (beat 7). The scripted finisher draw is **immune to draw‑cadence config** (see §9, VERIFY‑TUT‑R2‑2).

**F1 — `GigStartedEvent` publishes AFTER the first `PlayerTurn` draw.** The event is deferred (`PublishGigStartedDeferred`), so the forced initial hand is filled in the driver's `Awake` (`FillForcedInitialHand`, guarded by `!HasFired(YourTurn)`), **not** on the `GigStartedEvent` handler. Any beat that must precede the first draw cannot ride `GigStartedEvent`.

**F2 — Blocked is a bool, not a status.** Audience "Blocked" is a sprite‑tint bool (`AudienceCharacterBase.IsBlocked`, M1.2/E3), **not** an SO status — so `StatusAppliedEvent` never fires for it. `tut_status_blocked_front` therefore fires from a dedicated **`AudienceBlockedEvent`** (false→true transition). Likewise `tut_musician_breakdown` fires from a dedicated **`MusicianStressHitEvent`** (published in `BandCharacterStats.ApplyIncomingStressWithComposure` when applied stress > 0), not from a generic meter event.

### 3.3 Persistence & reset
- Demo cut + vertical slice: monotonic `firedDialogs`, persisted across sessions; no re‑fire.
- Reset clears the set (confirmation prompt). Hosted in the **Main Menu** revisit panel (D7=A), not the pause menu.

---

## 4. Presentation (D‑TUT‑4 = portrait + dialog box)

### 4.1 Layout
Bottom‑left static portrait (D7); adjacent dialog box with typed reveal; click / Enter to advance or dismiss; per‑dialog skip button (top‑right). Pages capped at 2 (D‑S5f‑5), cut at the rhetorical pause.

### 4.2 Informational beats are modal; directive beats are non‑modal input gates (TUT‑R2/R2b/R2c)
Three presentation modes now coexist:

- **(a) Informational modal beat.** Standard portrait + dialog. Blocks player input while shown; audio continues.
- **(b) Directive input gate (non‑modal).** For beats where the player must **do** something (beat 3 "play a composition", beat 5 "press Play"), the driver holds the dialog and opens a **non‑modal input allow‑list** (`TutorialInputGate`): only the taught action is accepted; everything else is inert until the player performs it. A **mandatory degrade path** (D2) resolves the beat if the player is already past the taught state.
  - **Beat‑3 allow‑list (TUT‑R2c).** `PlayComposition` can be restricted to the forced‑hand **"basic" composition ids**; modifier/sound compositions (Key Lift / Push It / Half Time) are blocked in lesson 1. Two‑level degrade: if no basic composition is available, fall back.
- **(c) holdLoop (beat 8).** For the finisher beat, the driver **holds the loop** so the last‑loop window doesn't expire under the modal, and does **not** re‑grant inspiration (the affordability check must read the real economy). If the player already played the finisher, the `tut_play_finisher_early` variant (prio 81) fires instead (degrade (a) with a wink; RT5).

- **(d) Single-card directive gate (beat 8) — `SingleCardOnly` (CARD-UX-1, 2026-07-13).** Armed **together with the loop hold**: while the loop is held, the **finisher is the only playable card** — every other card is undraggable and carries the red unplayable overlay (`UnplayableReason.TutorialGate`, `SSoT_Card_System.md` §10.5). This is also what gates **compositions** in the tutorial's final loop, since the FinalLoopLock is exempt under a hold (`SSoT_Runtime_CompositionSession_Integration.md` §5.4).
  - **Guard (mandatory).** The gate arms **only if the finisher is in HAND** (`deck.HandHas`). The hold's own `available` check also accepts "in a pile"; arming a gate whose single allowed card is absent would leave **zero** playable cards inside a held loop. Released with the hold (finisher played / song end / driver disable).
  - **Scope (D6=A).** `SingleCardOnly` blocks **card drag only**; it does not block End Turn (the hold already prevents the song from advancing, and End Turn stays as an escape hatch). `BlocksEndTurn` remains `CompositionOnly | PlayOnly`.

### 4.3 Pacing model (D‑TUT‑R2b‑1 = B) — audio runs, progression holds
**Audio keeps running under a modal** (the S4 no‑freeze precedent is retained for audio, avoiding MIDI desync). **Gameplay progression is what holds:** while a dialog is on screen, the loop repeats at any boundary, and audience actions + per‑member Vibe payout **wait between steps**. This **supersedes** the v0_1 position. v0_1 §4 ("no new gameplay events fire") and §6A.2b ("the composition loop / MIDI is not frozen; loop events queue") are reconciled into a single rule:

> Informational beats are modal and non‑blocking **to audio**, but **hold gameplay progression**; directive beats are non‑modal input gates that hold progression until the taught action is performed.

The audience turn is cooperatively suspended (`TutorialModalGate` `WaitUntil` in `AudienceTurnRoutine`) + `HandController` drag‑lock.

### 4.4 Single‑modal queue (D‑TUT‑10, retained)
At most one modal shows at a time. Multiple first‑time triggers in one event resolution queue and show in **authored priority order** (FIFO presentation queue, not an ordered runtime state machine). The debounced pump coalesces triggers from one player action (e.g. an action card that applies a status *before* `DeckManager.OnCardPlayed` fires) so the lowest‑priority dialog shows first regardless of publish order.

---

## 5. UX / Revisit (D‑TUT‑2)

### 5.1 Skip
Per‑dialog skip records the dialog as fired (same as completion). Revisit to re‑read.

### 5.2 Revisit — Main Menu host (D7 = A)
- The revisit list is hosted by **`TutorialRevisitPanel` in the Main Menu** (D7=A). **Pause‑menu references are dropped** (v0_1 §5's "revisit from pause" is superseded; no in‑game pause/settings menu is required to host it).
- **Already‑fired‑only (D‑TUT‑11, retained).** Lists only encountered dialogs, grouped by category (Cards / Meters / Jam / Audience / Run / Boss). Replay is modal, gameplay‑inert, non‑re‑triggering. A browse‑everything codex remains the deferred D‑TUT‑7.
- Reset clears the whole `firedDialogs` set on confirm.

### 5.3 Highlight model — registry-first + spawn hook (TUT-R2c → CARD-UX-1)

The spotlight (D3 = `ALWTTT/UI/TutorialSpotlight`, a positionable inverted sprite-mask) resolves its target **registry-first**: `TutorialHighlightTarget` components register into a **`TutorialHighlightRegistry`** by `highlightKey`, and the controller falls back to a serialized `HighlightBinding` list only if the registry misses. **Rationale.** The controller prefab lives in **ALWTTTCore** (runtime-loaded), so it cannot serialize gig-scene references; registration decouples the runtime-loaded controller from scene objects.

- **Static targets** (UI chrome / hand area / audience area) are wired as scene components — unchanged since TUT-R2c.
- **Runtime-instantiated targets use a spawn hook (D1=C, CARD-UX-1, 2026-07-13).** `TutorialHighlightSpawnHook` calls `AddComponent<TutorialHighlightTarget>()` + `InitRuntime(key, …)` at `GigManager.BuildBand` / `BuildAudience`, `CharacterCanvas.TryCreateIcon`, and the three `DeckManager` `BuildAndGetCard` tails. A prefab-variant approach was **rejected**: it cannot cover status icons (which spawn per-status) nor cards (one prefab, key derived from `CardDefinition.Id`).
- **Card keys come from the driver's own serialized refs** (`TutorialGuidedDriver.Awake`), so the hook hardcodes no card ids. Hand-card targets must project with the **HandCamera** (`HandController.Cam`), not `Camera.main`.
- **Precision (D3=B, CARD-UX-1).** The registry is **last-registered-wins**, so duplicate keys (4 musicians all registering `musician_stress_bar`) are disambiguated by re-registering the **affected** character's target inside `TutorialController.OnMusicianStressHit` / `OnAudienceBlocked` — both events carry the character ref — before enqueue. Status icons need no such pass: they are precise by construction (they spawn exactly when the status applies).
- **World-space targets** (e.g. a Cool Dude occluding the Kid) use a **world→screen mask conversion** in `TutorialOverlayView` (`Spotlight` struct + `ApplySpotlight` + `ResolveHighlight`, plus the world fields on `TutorialHighlightTarget`). Coded in TUT-R3 / T3b, **applied in build by CARD-UX-1** (ST-R3b-2 / ST-R3b-5 green).
- Optional per-binding **`UIPulseAnimator` "pop"** while a highlight's dialog is on screen.

---

## 5A. Localization & copy voice (S5f) + copy evolution (TUT‑REBUILD)

### 5A.1 Dual‑catalog localization (D‑S5f‑2 = B)
One `TutorialDialogCatalogSO` per language, selected by inspector assignment to `TutorialController.catalog`; no runtime language mechanics (demo cut). EN: `Assets/Resources/Data/Tutorial/Dialogs/`; ES: `.../Dialogs/ES/` (**ES ships assigned**). Trigger ids, priorities, categories, and highlight keys are identical across languages; only `revisitTitle` + `pages` differ. `firedDialogs` keys on trigger id, so catalog swaps preserve progress.

**Parity guard.** Editor menu `ALWTTT/Tutorial/Validate catalog language parity` compares every catalog asset against the `TutorialTriggerId` constant set. **TUT‑R3 adds a `ReservedUnauthored` exemption** (O2=A) so intentionally‑reserved ids (`tut_audience_preferences`, `tut_flow`) are not reported as missing while still catching EN/ES divergence and true extras. *(Note: `tut_first_reward_choice` may still report missing if its dialog lives in a separate S5h catalog — a pre‑existing multi‑catalog tension; add it to the exemption if it is unauthored everywhere.)*

### 5A.2 Copy voice — Spanish (D‑S5f‑1)
Register **tú**. The manager is slightly condescending toward the player, genuinely reverent toward the music — disdain targets the rookie, never the craft. Mechanical beats stay dry; musical beats carry the poetic layer. Brevity (D‑TUT‑1) still holds.

### 5A.3 Style rule — no em dashes (TUT‑R2b)
Em dashes read as AI‑authored and are **disallowed in dialog copy**. The 18 guided/rewritten dialogs comply. The retained reactive dialogs are de‑dashed in TUT‑R3.
> **Copy‑pass scope note (TUT‑R3):** the copy pass originally targeted "the 11 S5f reactive dialogs," but the TUT‑R3 retirement deletes 9 of those 11 in the same batch. Only the **2 retained** reactives (`tut_first_sfx_stage`, `tut_first_sound_card`) survive to be de‑dashed; the other 9 need no copy work.

### 5A.4 D8 token subset (S5f‑ext preview)
The guided copy uses runtime tokens resolved by `TutorialTokenResolver`:

| Token | Resolves to |
| --- | --- |
| `{$loops_per_part}` | `GigFlowSettings.jamRules.loopsPerPart` (**4** per D‑DEMO‑1). |
| `{$inspiration_per_loop}` | the per‑loop inspiration grant (**1** per the D5 rider). |
| `{$audience_hp}` | the **max `MaxVibe`** across the encounter's audience members (see §9). |

Full multi‑language authoring window + the general `{$concept}` system remain S5f‑ext (D‑S5f‑3=B). The Tutorial Browser editor is scoped as **TUT‑R4** (event‑driven; opens when portrait/viñeta art arrives).

---

## 6. S4 reactive inventory — historical

> This section is the historical record of the v0_1 reactive design. TUT‑REBUILD supersedes it as the gig‑1 primary path (§6B). The tables are retained as rationale; the supersede/retain ledger below records what TUT‑R3 retires vs keeps.

### 6.1 Supersede / retain ledger (TUT‑REBUILD, TUT‑R1 §6 lineage)

| Reactive id | Disposition (TUT‑R3) | Superseded by / reason |
| --- | --- | --- |
| `tut_welcome_to_gig` | **RETIRED** | → `tut_jam_welcome` (beat 1) |
| `tut_first_composition_card` | **RETIRED** | → `tut_play_composition` (beat 3) |
| `tut_first_inspiration_spend` | **RETIRED** | → `tut_inspiration_economy` (beat 7) |
| `tut_first_loop_inspiration` | **RETIRED** | → `tut_inspiration_economy` (loop pays back) |
| `tut_first_song_end` | **RETIRED** | → `tut_song_end_vibe` (beat 9) |
| `tut_first_audience_action` | **RETIRED** | → `tut_audience_turn` (beat 10) |
| `tut_first_action_card` | **RETIRED** | folded into `tut_your_turn` / `tut_play_finisher` |
| `tut_first_status_applied` | **RETIRED** | → `tut_status_buff_musician` / `tut_status_debuff_audience` |
| `tut_first_gig_won` | **RETIRED** | → `tut_gig_won` |
| `tut_first_sfx_stage` | **RETAINED** | no guided equivalent (stage‑reaction beat); reactive, post‑song‑1 |
| `tut_first_sound_card` | **RETAINED** | no guided equivalent (modifier/sound cards blocked in lesson 1); opportunistic |
| `tut_first_reward_choice` | **RETAINED** | S5h reward flow; separate lifecycle from the gig‑1 curriculum |

**Retirement mechanics (TUT‑R3):** remove the 9 constants (`TutorialDialogSO`), remove the 9 reactive `TryEnqueue` call sites (`TutorialController`), empty `SupersededIds` (`TutorialGuidedDriver`), drop the 9 `Add()` calls from each `SeedDemoCut*` seeder (keeping the 2 retained), delete the 18 `.asset` files (9 × EN/ES), extend the parity guard (`ReservedUnauthored`). Re‑run ST11/ST12 (parity) after.

### 6.2 Historical reactive tables (v0_1)
*The v0_1 §6 standalone list (8 drafted rows) and §6A jam sequence (6 beats over song 1) are preserved here as the original reactive design.* Key historical decisions retained: **D‑TUT‑6=A** (jam as guided sequence — the seed of the TUT‑REBUILD curriculum), **D‑TUT‑7=C** (revisit as codex‑lite; rich codex deferred), **D‑TUT‑8=A** (live on song 1, bus‑fired), **D‑TUT‑9=A** (fold/standalone split), **D‑TUT‑10=A** (independent triggers + single‑modal queue), **D‑TUT‑11=A** (already‑fired‑only revisit). S4 impl decisions **D‑S4‑BUS=B / D‑S4‑SRC=A / D‑S4‑PRODUCER / D‑S4‑DEDUP=B / D3=B / D4=A / D5=SO / D7 bottom‑left** are retained as record (see §10).

---

## 6B. Gig‑1 guided curriculum (TUT‑REBUILD)

The first gig runs a driver‑sequenced arc over the demo's first song. **Principle (D‑TUT‑6 lineage): teach to act, not to fully understand** — minimum on first contact; depth deferred to the revisit menu. **EN copy below is the design‑approved reference; the authoritative localized text lives in the catalog seeders / `.asset` files (see Status).**

### 6B.1 The 10‑beat arc

| # | Trigger id | Bus / driver event | Gate | highlightKey | Cat / prio | EN copy (reference) |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | `tut_jam_welcome` | driver open (forced hand pre‑filled in `Awake`, F1) | — | — | Run / 10 | "Welcome to the stage, rookie. This is a jam: the band plays live and the crowd decides if you're worth anything. Your only job is to make the music talk. I'll tell you how." |
| 2 | `tut_your_turn` | player turn / forced hand present | — | `hand` | Cards / 20 | "Your turn. Your hand holds two kinds of cards: COMPOSITION cards build the song; ACTION cards protect the band and push the crowd. Music first. Always." |
| 3 | `tut_play_composition` | `CardPlayedEvent` (composition) | **input** (allow‑list: basic compositions) | `card_default_mode` | Jam / 30 | "Play a composition card: drag it onto the band. Each one adds something real to the song. It's not set dressing, it's music." |
| 4 | `tut_tracks_three` | after first composition | — | `song_panel_tracks` | Jam / 40 | "See the song panel? Three tracks: RHYTHM, BACKING and MELODY. Each composition card touches ONE of them. Drums set the pulse, chords build the world, melody is what sticks in your head." |
| 5 | `tut_press_play` | pre‑Play | **input** (Play only) | `play_button` | Jam / 50 | "Now hit Play. The song will run in a loop, and everything you play from here lands live. No rehearsal. That's how real music gets made." |
| 6 | `tut_loops_structure` | first loop / `LoopResolvedEvent` | — | `loops_bar` | Jam / 60 | "Each song runs {\$loops_per_part} loops, and every loop is a turn. The bar up top shows what's left. When the last loop ends, the song closes, and the bill comes due." |
| 7 | `tut_inspiration_economy` | per‑loop grant (**+ scripted finisher draw**) | — | `inspiration_counter` | Jam / 70 | "See that +{\$inspiration_per_loop}? Every loop feeds you {\$inspiration_per_loop} Inspiration. Some cards demand it to be played. The good ones, naturally. Save it: one just landed in your hand that's worth it." |
| 8 | `tut_play_finisher` | last loop | **holdLoop** (no inspiration re‑grant) | `card_psychic_waves` | Cards / 80 | "Last loop. Time for the closer: PSYCHIC WAVES, an ACTION card. It doesn't touch the song: it hits the WHOLE crowd's mind at once. It costs Inspiration. Play it before the loop ends. The ending is everything." |
| — | `tut_play_finisher_early` | last loop, finisher already played | degrade (a) + wink | — | Cards / 81 | "Last loop. Now you can play Psychic Wa... What? Were you even listening, rookie? You jumped the gun. Fine: the closer already landed. Next time save it for the ending, when the crowd's mind is at its softest." |
| 9 | `tut_song_end_vibe` | `SongEndVibeEvent` | — | `audience_vibe_bars` | Jam / 90 | "Song's over: all the hype you built converts into Vibe DAMAGE on the crowd. Each of them holds up to {\$audience_hp}. Drain it and they're yours: convinced. That's how you win a gig, song by song." |
| 10 | `tut_audience_turn` | `AudienceTurnStartedEvent` | — | `audience_area` | Audience / 100 | "Now it's their turn. Every character in the crowd has their own tricks: some hit your musicians' Stress, their fortitude reserve (at zero, breakdown), and others cover for each other. Watch them before your next turn." |

### 6B.2 `tut_composure` + rewritten reactive layer
The curriculum drives seven rewritten reactive beats that fire from live combat state during (and after) gig‑1:

| Trigger id | Bus event | highlightKey | Cat / prio | EN copy (reference) |
| --- | --- | --- | --- | --- |
| `tut_status_buff_musician` | `StatusAppliedEvent` (musician, `IsBuff`) | `status_icon_musician` | Meters / 110 | "See that icon over the musician? That's a status effect, and this one's working for you. Hover it and it tells you exactly what it does. Read them: the band lives on them." |
| `tut_status_debuff_audience` | `StatusAppliedEvent` (audience) | `status_icon_audience` | Meters / 112 | "You've hung an effect on the crowd: the icon under their portrait. Effects work on their own, turn after turn, no permission needed. Plant them and let the music do the rest." |
| `tut_status_blocked_front` | **`AudienceBlockedEvent`** (F2) | `status_icon_blocked` | Audience / 114 | "The big guy stepped up front and BLOCKED: while it lasts, your Vibe won't get through. Don't waste music on a wall. Wait for the guard to drop, or spend the song on the ones actually listening." |
| `tut_musician_breakdown` | **`MusicianStressHitEvent`** (F2) | `musician_stress_bar` | Meters / 116 | "Your musician's Stress took a hit. That bar is their fortitude, and it runs out: at zero, they break down and stop playing. Some action cards restore it, or shield it. Take care of your band: no band, no song." |
| `tut_composure` | `StatusAppliedEvent` (`TempShieldTurn`, musician) | `status_icon_composure` | Meters / 118 | "That's COMPOSURE: it soaks Stress damage before it touches your musician's fortitude. It lasts until your next turn, then it's gone. It's a guard, not armor. Raise it when you see the hit coming." |
| `tut_gig_won` | `GigOutcomeEvent` (won) | — | Run / 120 | "All of them convinced. Hear that? The silence right after. That's a crowd that belongs to you now. Don't let it go to your head, rookie: it was ONE gig. But it was real music." |
| `tut_gig_lost` | `GigOutcomeEvent` (lost) | — | Run / 121 | "It's over and not all of them fell. Happens. Music doesn't forgive weak endings: next time, bank Inspiration for the finish and close it out. Come on, from the top." |

**Blocked presentation (O1 = A).** Blocked is a tint bool (F2), so its copy stays **de‑iconified** — it references the guard visually, not an SO status icon. No Blocked SO icon is authored (it would re‑open the F2 divergence). Visual parity, if wanted later, is a JUICE/DEMO‑FIXES item.

### 6B.3 Driver / gate spec
- **Forced initial hand (F1).** Filled in `TutorialGuidedDriver.Awake` (`FillForcedInitialHand`, guarded by `!HasFired(YourTurn)`) — the order is Default Mode, Wormus Major, Singing Field, Warm Up, with role/domain fallbacks; a full miss hits the M4.5 guarantee (D1).
- **Scripted draws (D1=B, M4.5 seam).** Forced hand (beat 2) + scripted finisher draw (beat 7), via `TutorialScriptedDrawQueue`.
- **Input gates (beats 3/5).** `TutorialInputGate` allow‑list; beat‑3 restricted to basic composition ids (modifier/sound blocked); two‑level degrade if the taught card is unavailable.
- **holdLoop (beat 8).** `TutorialLoopHoldGate` holds the last-loop window and does not re-grant inspiration; `tut_play_finisher_early` (prio 81) covers the already-played case. **CARD-UX-1 (2026-07-13):** the hold now arms the **`SingleCardOnly`** gate alongside it (finisher-only, hand-guarded — §4.2 (d)), which is what gates compositions in that loop; the final-loop composition lock is exempt while a hold is armed.
- **Pacing (D‑TUT‑R2b‑1=B).** §4.3.
- **Ordering fixes (TUT‑R2b).** **FIX‑1:** `OnCompositionLoopFinished` publishes `LoopResolvedEvent` **before** the per‑loop inspiration grant, so the beat‑8 affordability check under‑read by one; the driver now uses request flags (`_beat7Requested`/`_beat8Requested`) that latch on the driver's *action*, independent of dialog completion (loops keep running under modals). **FIX‑2:** beat‑7 swallow corrected.
- **Copy evolution (TUT‑R2b/c v2).** Em dashes removed; the beat‑3 rhythm line moved to beat 4 (`tut_tracks_three`); the finisher named **Psychic Waves** (beat 8); the Blocked copy de‑iconified.

### 6B.4 Finisher economy (D‑DEMO‑1 = 4 loops)
With `loopsPerPart = 4`, per‑loop inspiration = 1, and finisher cost = 3, the budget is **4 vs cost 3 → margin 1** (the robustness the D‑TUT‑R1‑1=A cost‑3 choice wanted). The finisher (`Starter_psychic_waves`, `ModifyVibe +5` AoE, magnitude revised 4→5 by D‑TUT‑R2c) is AoE ⇒ no target select ⇒ ideal for the beat‑8 gate.

---

## 7. Vertical slice extension (D‑TUT‑5)

Per Standing Directive #3, every Phase C feature gets coverage. Draft inventory (ids claimed now; text authored per session):

| id | Phase C session | Topic |
| --- | --- | --- |
| `tut_ship_hub_intro` | S6 | First ship‑hub entry; manager framing of run structure. |
| `tut_first_reward_choice` | S5h/S6 | Reward selection — **authored** in the retained-reactive seeder (TUT-R3; closed a pre-existing S5h parity gap; controller enqueue in `OnRewardOpened` already wired). Teaches "pick 1 of 3 cards → joins your deck". Retained through the retirement. |
| `tut_audience_state_machine` | S7 | Crowd states (idle / hostile / vibing). |
| `tut_first_boss_encounter` | S8 | Boss as phase‑shift encounter. |
| `tut_run_complete` | S8 | Closes the run‑structure loop opened at `tut_ship_hub_intro`. |

---

## 8. Acceptance / DoD

### 8.1 Reactive demo cut (S4) — **met, historical**
S4 closed 2026‑06‑17 (ST‑S4‑1..11 + QUEUE/PERSIST/RESET/REVISIT/OPP/NODIR/GATE; D‑S4‑DEDUP fix; 11 dialogs seeded). Retained for record.

### 8.2 Guided curriculum (TUT‑REBUILD) DoD
- [x] Driver‑sequenced 10‑beat arc + `tut_composure` + rewritten reactives fire in gig‑1 (TUT‑R2; CT1–CT6 + RT1–RT8 green).
- [x] Input gates (beats 3/5) with beat‑3 allow‑list + mandatory degrade (D2); holdLoop (beat 8) with no inspiration re‑grant; `tut_play_finisher_early` variant.
- [x] `TutorialScriptedDrawQueue` over the M4.5 seam (forced hand + scripted finisher draw).
- [x] D8 tokens (`{$loops_per_part}`/`{$inspiration_per_loop}`/`{$audience_hp}`) resolve.
- [x] Pacing model D‑TUT‑R2b‑1=B (audio runs; progression holds).
- [x] Config riders applied: initial inspiration 1, per‑loop 1, draw 1/0, starter `inspirationGenerated`→0 (D‑TUT‑R2‑1=B, **confirmed applied**).
- [x] **TUT‑R3:** 9 superseded ids retired (constants + call sites + assets) with parity guard green (ST11/ST12 re‑run; parity `extra:[]`).
- [x] **TUT‑R3:** copy pass — de‑dash the 2 retained reactives; ES/EN parity of the 20 dialogs.
- [x] **TUT-R3:** Main-Menu revisit host wired (D7=A); registry highlights registered per beat; world→screen for world-space targets. **The deferred world-character + hand-card highlights were delivered by CARD-UX-1 (2026-07-13)** via the spawn hook (§5.3).
- [x] **TUT‑R3:** `Keep Cool` placement resolved — **stays C2‑owned**; generic‑catalog move deferred (D‑ECON‑GENERIC, see §9.2).

### 8.3 Vertical slice (S8) — unchanged
5 Phase C dialogs authored; coverage verified against `Design_Vertical_Slice_v0_1.md §3`.

---

## 9. Open questions

### 9.1 Resolved this batch / TUT‑REBUILD
- **S4 opens closed:** SO chosen for dialog data (matches `CardDefinition`/`StatusEffectSO`); portrait path `Assets/Resources/Data/Tutorial/Portraits/`; layout corner = bottom‑left (D7); highlight‑mask technique = `TutorialSpotlight` shader (D3); captain‑bubble = D4/D7 (auto‑place OFF by default).
- **VERIFY‑TUT‑R2‑1 → D‑TUT‑R2‑1 = B** — zero starter gens; flat‑only economy (1 initial / 1 per loop). **Confirmed applied** (O4).
- **VERIFY‑TUT‑R2‑2 → resolved** — draw cadence is config‑driven (`GigFlowSettings.DrawPerLoop`/`DrawCardsOnPlay` = 1/0); the scripted finisher draw is immune.
- **Breakdown bus source → `MusicianStressHitEvent`** (applied stress > 0).
- **`{$audience_hp}` → max `MaxVibe`** of the encounter's audience.
- **O1 = A** — Blocked stays de‑iconified (F2).
- **O2 = A** — parity guard `ReservedUnauthored` exemption for `tut_audience_preferences` / `tut_flow`.
- **D‑DEMO‑1 = 4 loops** — build stays at `loopsPerPart = 4` (finisher margin 1). The prior "loops 4→3" doc change is void.
- **CLOSED (CARD-UX-1, 2026-07-13) — world→screen highlight for world-space targets.** Coded in TUT-R3/T3b, applied in build; ST-R3b-2 / ST-R3b-5 green.
- **CLOSED (CARD-UX-1, 2026-07-13) — world-character + hand-card highlights** (the TUT-R3 deferral). Delivered by the spawn hook (D1=C, §5.3); duplicate-key precision by per-event re-registration (D3=B).

### 9.2 Still open (TUT‑R3 Tranche 3 / later)
- **`Keep Cool` catalog placement — RESOLVED (T3a):** authored as a **C2-owned** card (`FixedMusicianType`), not generic; C2 is always in the demo band (roster = C2 + Sibi). The generic-catalog move is **deferred pending D-ECON-GENERIC** (who spends the ECON-1 per-musician action budget for an `AnyMusician` card — home `Design_Action_Economy_v1` / `SSoT_Gig_Combat_Core §14`).
- **`tut_first_reward_choice` parity** — if unauthored in every catalog, add to `ReservedUnauthored`; the multi‑catalog parity model may need catalog‑scoping (pre‑existing, out of scope here).
- **Reserved ids** `tut_audience_preferences` (D6) / `tut_flow` (D‑TUT‑R1‑4) remain reserved without a trigger.
- **R1 — the beat-8 hold can arm with the finisher in a pile (opened CARD-UX-1, 2026-07-13).** The hold's `available` check accepts `HandHas || PilesHave`, and held loops grant no draw (no `LoopResolvedEvent`) ⇒ if the finisher sits in a pile because the beat-7 scripted draw failed, the loop can hold with the finisher **unreachable**. **Pre-existing since TUT-R2**; the CARD-UX-1 `SingleCardOnly` gate does not worsen it (the gate is hand-guarded and simply does not arm). One-line hardening available: `available &&= deck.HandHas(IsFinisher)`. **CLOSED (DEMO-FIXES-A, 2026-07-15, D-DF-4=A).** `TutorialGuidedDriver.FireFinisherBeat` now computes `available = finisherCard != null && deck != null && deck.HandHas(IsFinisher)` (the `|| PilesHave` term dropped). If the beat-7 scripted draw failed, beat 8 degrades via path (b) instead of arming an unreachable hold. The `SingleCardOnly` gate's own hand-guard is now redundant but retained as belt. ST-DF-6 PASS; ST-DF-7 (finisher-in-pile path) deferred to Dev Mode — not reachable through normal gameplay (the scripted draw almost always lands).
- **CT1 — highlight pulse survives the modal close (DEMO-FIXES-A, 2026-07-15).** Previously the `UIPulseAnimator` pulse died when the tutorial modal closed, so beats 3/5/8 lost the rhythmic cue the moment the player was asked to act. `TutorialController` now keeps the dialog's highlight target pulsing **while a directive is alive** (`TutorialInputGate.IsActive || TutorialLoopHoldGate.IsArmed`) via `PulseWhileDirective`, started in `OnDialogComplete` after `DialogCompleted` is invoked (covers both the beat-3/5 shape, where the gate arms inside that invoke, and beat-8, where the hold arms before the modal shows). **Pulse only** — the dim/spotlight overlay still closes with the modal (keeping the dim would obstruct play). No `TutorialDialogSO`/dialog-asset changes (TUT-R3 content untouched). Known edge: an unrelated reactive dialog completing mid-directive does not re-target the persistent pulse (no-retarget-while-running). ST-DF-8/9 PASS.

### 9.3 Gig‑open tutorial opt‑in (DEMO‑FIXES‑A, 2026‑07‑15)

The demo bypasses GigSetup (`MainMenuController` auto‑launch) and Retry reloads the Gig scene directly, so the tutorial could previously only be skipped by hand‑deactivating a GameObject — which made the final‑loop composition lock (CARD‑UX‑1) untestable and contaminated win‑rate measurement. A gig‑open modal now asks whether to run the tutorial.

- **Single source of truth:** `PersistentGameplayData.TutorialEnabled` (bool). Written ONLY by the opt‑in prompt; read ONE‑SHOT at gig open. Persists across gigs within a launch (DontDestroyOnLoad); no disk save exists, so it never crosses app launches.
- **Prompt:** `TutorialOptInPrompt` (Gig‑scene canvas). `GigManager.Start` defers `StartGig()` until answered, so the choice lands before the beat‑2 forced hand is drawn. Unwired/null prompt ⇒ gig starts immediately with the current flag (dev scenes).
- **Forced‑hand fill moved out of `Awake`:** `TutorialGuidedDriver.PrepareForGig(bool)` is the single, timing‑immune fill point. `Awake` keeps only highlight‑key registration. `GigManager` calls `PrepareForGig` after the answer (and in the dev/no‑prompt branch) — timing‑immune because it is a direct call, independent of when GigCanvas activates.
- **Runtime driver resolution (D‑DF‑8=A):** `TutorialController`/`TutorialGuidedDriver` live on `GigCanvas` in the **ALWTTTCore** scene; `GigManager` lives in the Gig scene, so a serialized cross‑scene reference is impossible. `GigManager.ResolveTutorialDriver()` resolves it at runtime via `UIManager.Instance.GigCanvas.GetComponentInChildren<TutorialGuidedDriver>(includeInactive: true)`. Null‑safe: on failure the belt guards still make the driver inert on "No"; a "Yes" merely skips the forced‑hand nicety once.
- **Belt guards (inert on "No"):** the driver's reactive path is neutralized by `PD.TutorialEnabled` reads at the top of `TryBeat`, `OnDialogCompleted`, and `OnLoopResolved` (the last covers the beat‑7 scripted finisher draw), and at `TutorialController.TryEnqueue`. `ReplayDialog` (MainMenu revisit) bypasses `TryEnqueue` and is unaffected. The subsystem GameObject stays active and simply drops events.

Smokes ST‑DF‑1/2/4/5 PASS (opt‑in appears pre‑draw; "No" ⇒ random hand, zero modals, full playable gig; "Yes" ⇒ beats 1–10 green incl. Psychic Waves impact; Retry re‑asks with last answer as default).

---

## 10. Decision ledger

**Reactive (v0_1):** D‑TUT‑1 (basics only + extensible infra) · D‑TUT‑2 (skip + revisit) · D‑TUT‑3 (first‑time HashSet) · D‑TUT‑4 (portrait + dialog) · D‑TUT‑5 (~5 Phase C dialogs) · D‑TUT‑6=A (jam guided sequence) · D‑TUT‑7=C (codex‑lite) · D‑TUT‑8=A (live, bus‑fired) · D‑TUT‑9=A (fold/standalone) · D‑TUT‑10=A (independent triggers + single‑modal queue) · D‑TUT‑11=A (already‑fired‑only revisit). D‑S4‑BUS=B · D‑S4‑SRC=A · D‑S4‑PRODUCER · D‑S4‑DEDUP=B · D3=B · D4=A · D5=SO · D6 (portrait path) · D7 (bottom‑left).
**Localization (S5f):** D‑S5f‑1 (ES tú voice) · D‑S5f‑2=B (dual catalog) · D‑S5f‑3=B (token/authoring window → S5f‑ext) · **D‑S5f‑4=B SUPERSEDED** (guided pulled into demo) · D‑S5f‑5=B (2‑page cap).
**TUT‑REBUILD (D1–D9):** D1=B (M4.5 scripted‑draw seam) · D2 (mandatory degrade paths) · D3=B (guided over reactive; D‑TUT‑3 not retired) · … · D7=A (Main‑Menu revisit host) · D8 (copy tokens) · D9 (SfxStage auto‑gate on `ShowSongHypeBar`).
**TUT‑R1:** D‑TUT‑R1‑1=A (Psychic Waves cost 3) · D‑TUT‑R1‑2 (owner Sibi) · D‑TUT‑R1‑3 (Keep Cool) · D‑TUT‑R1‑4 (`tut_flow` reserved).
**TUT‑R2/R2b/R2c:** **D‑TUT‑R2‑1=B** (starter gens→0; flat‑only economy) · **D‑TUT‑R2b‑1=B** (pacing: audio runs, progression holds) · **D‑TUT‑R2c** (naming Gran Final → **Psychic Waves**; finisher magnitude 4→5; highlight **registry** + serialized fallback; beat‑3 basic‑composition allow‑list; `tut_play_finisher_early` prio 81).
**TUT‑R3:** **O1=A** (Blocked de‑iconified) · **O2=A** (parity `ReservedUnauthored`) · **D‑DEMO‑1=4 loops** · VERIFY‑DOC‑STARTER‑1 → **6 Action / 11 Composition = 17** (not 5/12).
**CARD-UX-1:** **D1=C** (highlight registration via spawn hook, not prefab variants) · **D2=A** (final-loop composition lock; exempt while a loop-hold is armed) · **D3=B** (duplicate highlight keys disambiguated by re-registering the affected character on `MusicianStressHitEvent` / `AudienceBlockedEvent`) · **D4=A** (overlay reuses `passiveImage` / `SetInactiveMaterialState`; no new serialized field) · **D5** (the overlay's ECON-1 budget input covers statically-resolvable payers only; `AnyMusician` excluded pending D-ECON-GENERIC) · **D6=A** (`SingleCardOnly` blocks card drag only, not End Turn).
**DEMO‑FIXES‑A:** **D‑DF‑1=A** (opt‑in prompt at gig open; flag = `PD.TutorialEnabled`) · **D‑DF‑2=A** (per‑gig: re‑ask each gig open, PD stores last answer as default) · **D‑DF‑3=A** (one‑shot read at gig open; no mid‑gig re‑arm) · **D‑DF‑4=A** (beat‑8 `available = HandHas` only) · **D‑DF‑5=A** (hide cost badge at cost 0; mirror of S5e‑ext gen‑badge) · **D‑DF‑6=A** (pip tooltip via existing `TooltipManager` pipeline) · **D‑DF‑7=A** (DF‑CATALOG runtime band‑scoped catalog union; no asset mutation) · **D‑DF‑8=A** (runtime driver resolution via `UIManager.Instance.GigCanvas`; no new singleton).

---

## 11. Cross-references

- `planning/active/Design_Sensory_Contract_v0_1.md §3` (event bus; tutorial as consumer; + `MusicianStressHitEvent`, `AudienceBlockedEvent`).
- `planning/Design_Demo_Cut_v1.md §1.1` (run shape: `loopsPerPart=4`, initial inspiration 1, per‑loop 1, draw 1/0).
- `planning/Design_Starter_Deck_v1.md §4/§5` (Psychic Waves, Keep Cool; 6 Action / 11 Composition = 17).
- `planning/Design_Project_Directives_v0_1.md §D3` (Tutorial‑as‑mandatory).
- `planning/active/Design_Vertical_Slice_v0_1.md §9` · `Roadmap_ALWTTT.md §5.5, §7`.
- `CURRENT_STATE.md` (guided infra "just completed" bullet; TUT‑R3 retirement/close).
- `changelog-ssot.md` (2026‑07‑09 TUT‑REBUILD infra + TUT‑R3 entries).
- `coverage-matrix.md` line 33 (tutorial row → guided primary; doc home `Design_Tutorial_System_v0_2`).
- `TUT-REBUILD_Sub_Roadmap.md` (TUT‑R* arc home).
- Code truth: `TutorialGuidedDriver`, `TutorialController`, `TutorialDialogCatalogSO` / `TutorialDialogSO`, `TutorialInputGate` / `TutorialLoopHoldGate` / `TutorialScriptedDrawQueue`, `TutorialHighlightTarget` / `TutorialOverlayView` / `TutorialRevisitPanel`, `TutorialTokenResolver`.

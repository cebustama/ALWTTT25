# S5 — Demo Cut Close — Sub-Roadmap

**Status:** Planning-only. Decomposes `CURRENT_STATE.md §3` row **S5** and `Roadmap_ALWTTT.md §5.4 / §5.5`. Does **not** define implementation truth. Created 2026-06-18.
**Pattern:** Same role as `M1_5_Dev_Mode_Sub_Roadmap.md` — a durable multi-batch plan + decision ledger; per-batch **deep scope + rehydration prompt are generated at batch-open**, not duplicated here.

S5 is the last demo-cut session: it closes Phase B (§5.5 DoD) and tags the demo, then unblocks Phase C (S6–S8).

---

## Decisions fixed (batch-open ledger)

Vibe delivery & transparency:
- **D-S5-VIBE = B (refined)** — all Vibe delivered at song end; SFX bonus accumulates into a song-scoped `pendingVibe`; new accumulated-Vibe readout. The song-end **conversion already fires once per song** (confirmed in `GigManager`, guarded by `_lastSongFeedback`) — there is **no conversion bug**; the perceived "4 applications in 3 loops" was the mid-song SFX stage bonus (≤3) + 1 conversion.
- **D-S5-VIBE-ARCH = A** — `pendingVibe` bespoke/song-scoped, shaped so the planned Pending Effects layer can absorb it.
- **D-S5-COUNTER = B** — live projection, expanded into a per-audience transparency system (see `Design_Vibe_Telegraph_v0_1.md`).
- **D-S5-TELEGRAPH-SCOPE = B** — C1 (global `L+SFX`) → C2 (per-enemy effectiveness) → C3 (per-enemy number), in that order; degrades cleanly to C1+C2 if time-boxed.
- **D-S5-SFX-SCALE = A** — SFX bonus stays flat (not impression-scaled).
- **D-S5-TELEGRAPH-HOME = `planning/Design_Vibe_Telegraph_v0_1.md`** (created).

Card clarity:
- **D-S5-ICON = A** — resolve musician icon via `MusicianCharacterType → MusicianCharacterData.CharacterIcon` at `SetCard`; fallback no-icon for AnyMusician / generic cards.

Structure / docs:
- **D-S5-CLARITY-SPLIT → split** — Vibe-transparency work (S5a) separated from card-face/animation work (S5b). (Supersedes the earlier "single S5a" once the counter grew into a telegraph system.)
- **D-DOC1 = A** — batch scopes live in this sub-roadmap (durable source + ledger); rehydration prompts derive from it.
- **D-DOC2 = A** — CURRENT_STATE §3 gets only a minimal "decomposed → sub-roadmap" pointer now; detailed §3 + Roadmap §5 update at each batch **close**.
- **D-DOC3** — canonical labels: **S5a** Vibe + transparency · **S5b** Card clarity + animation · **S5c** Win-rate loop · **S5d** Presentation + §5.4 + close.

## Sequence & dependencies

```
S5a (Vibe + transparency)  ─┐
                            ├─→  S5c (win-rate loop)  ─→  S5d (presentation + §5.4 + close)
S5b (card clarity + anim)  ─┘
```

S5a and S5b may run in either order; **S5b recommended first** (lower risk, and it improves the legibility of the S5c playtest). S5c needs both A and B landed (correct + readable build). S5d is the gate.

---

## S5a — Vibe delivery + transparency

**Objective.** Make the Vibe→audience causal chain legible and deliver all Vibe at song end. Implements `Design_Vibe_Telegraph_v0_1.md`.

**Tasks (plan-level).**
1. Refactor `ApplySfxBonusVibe` to accumulate into a song-scoped `pendingVibe`; pay out once at song end alongside the conversion. Repurpose the mid-song `+N Vibe!` gold floater to feed the readout, not apply Vibe.
2. Add a live `avgImpression(i)` accessor (current part running impressions + closed parts).
3. C1 — global `L + SFX` readout under the SongHype bar (loop-boundary cadence).
4. C2 — per-enemy effectiveness telegraph (Super / Normal / Not-very / Immune) from live `avgImpression`.
5. C3 — per-enemy projected number on each audience (small add-on over C2).

**DoD.** All Vibe lands at song end; readout shows accurate `L + SFX`; per-enemy effectiveness + number track live impression and match the song-end deltas; no double-application; consistent with existing cyan/gold floater language.
**Smoke outline.** Verify single song-end payout (not per-loop); readout matches applied deltas; effectiveness flips with impression; blocked/indifferent shows Immune; regression: SFX total unchanged vs pre-refactor.
**Docs at close.** `Design_Demo_Cut §3.1` (SFX deferred + readout), `SSoT_Scoring_and_Meters §6` (note: SFX paid at song-end via pendingVibe; conversion was already once), coverage-matrix + SSoT_INDEX (Vibe-telegraph concept row).

## S5b — Card clarity + animation  *(recommended first)*

**Objective.** Remove the two readability blockers playtesters hit (card type, card owner) and fix the playing-animation correctness; instrument win-rate for S5c.

**Tasks (plan-level).**
1. **Item 1 — card-type backgrounds.** Toggle `Action Bg` / `Composition Bg` in `CardBase.SetCard` by `def.IsAction` / `def.IsComposition`.
2. **Item 4 — musician icon on card.** Resolve `MusicianCharacterType → MusicianCharacterData.CharacterIcon`, assign to a card `Image`; no-icon fallback.
3. **Item 3 — playing-animation gating.** Beat/playing animation only for musicians with an **active track this loop**. *First sub-task: confirm the track→musician mapping + the live active-track query in `CompositionSession` (the one real unknown).*
4. **Win-rate instrumentation.** Dev-surfaced gig-outcome readout (W/L count per session) so S5c is measurable.
5. **Prefab parity.** Add the two bg children + the icon `Image` to `CardUI.prefab` and any other card prefab (currently only in `Card3D`); wire on `CardBase`; **null-guard** (UI-fix-A NRE recurrence vector).

**DoD.** Action vs composition visually unmistakable on every card prefab; correct musician icon (or none); only actually-playing musicians animate; outcome readout works; no NRE on any card prefab.
**Smoke outline.** Each prefab shows correct bg per type; icon matches owner / absent for AnyMusician; play a loop where a subset of musicians have tracks → only those animate; inventory + gameplay prefabs both render.
**Docs at close.** `SSoT_Card_System` (card presentation: bg-by-type + owner icon).

## S5c — Win-rate tuning loop  *(iterative)*

**Objective.** Tune to win-rate 60–80% on the demo encounter and make balance feel intentional. Several short play→measure→adjust rounds (not closeable in one sitting; bounded by playtest throughput). Levers: Inspiration cost/gen, `sfxBonusVibeStage1/2/3`, `MaxVibeFromSongHype`, impression band, encounter tuning. The deterministic B3-slate remainder (C/D/E-lite/G + design gaps #12–15, or explicit deferral) can ride here as non-blocking filler.

## S5d — Presentation + §5.4 readiness + close

**Objective.** Reward UI (scope TBC — likely the demo result/victory screen, not full reward-selection, which is S6) + cover refresh; run the §5.4 readiness checklist + invariant re-check (F-1/F-3/F-4, MB1–4, M4.5); close docs (CURRENT_STATE / Roadmap §5 / changelog); tag the demo. Closing S5 closes Phase B and unblocks Phase C.

---

*Per-batch deep scope (verifiable task list + full smoke tests + final DoD) and the rehydration prompt are produced when each batch is opened (M1_5 pattern).*

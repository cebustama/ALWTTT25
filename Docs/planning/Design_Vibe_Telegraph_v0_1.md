# Design — Vibe Telegraph v0.1

**Status:** Design (planning). Target implementation: **S5a** (see `planning/active/S5_DemoCutClose_Sub_Roadmap.md`).
**Created:** 2026-06-18.
**Authority boundary:** This note owns the **presentation** of the Vibe→audience causal chain. It does **not** define the scoring math — `SSoT_Scoring_and_Meters.md §6` is authoritative for the formula, `SSoT_Audience_and_Reactions.md` for impression, `Design_Sensory_Contract_v0_1.md` for floating-text / sensory output. If this note and any of those disagree, those win.

---

## 1. Problem

In Slay the Spire / Monster Train the player sees the causal chain card → number → enemy: how much an action does, modified by buffs/debuffs, *before or as* it lands. ALWTTT currently hides this. SongHype rises, Vibe is converted at song end, and per-audience preference silently scales the result — the player has no running number to watch and no per-enemy read on "is this working on them?" Playtesters describe the Vibe as nebulous. This telegraph makes the existing math legible; it adds **no new mechanic**.

## 2. The model being visualized (presentation view of `SSoT_Scoring §6`)

Per-audience Vibe at song end (authoritative form in the Scoring SSoT):

```
vibeDelta(i) = round( baseVibe × impressionFactor(i) )      [floored at 0; no negative macro Vibe]
baseVibe         = SongHype01 × MaxVibeFromSongHype          [global, tracks current SongHype → volatile]
impressionFactor(i) = 1 + avgImpression(i) × 0.25           [avgImpression ∈ [−2,+2] → factor ∈ [0.5, 1.5]]
avgImpression(i) = mean of audience i's per-loop impressions this song
```

Plus, layered on top (per `Design_Demo_Cut §3.1`, D-S5-VIBE=B):

```
SFX bonus = flat per non-blocked member, ACCUMULATED into a song-scoped pendingVibe,
            paid at song end alongside the conversion. NOT impression-scaled (D-S5-SFX-SCALE=A).
```

Gates: `IsBlocked` / Indifferent audiences receive no Vibe. Flow contributes a band-wide song-end multiplier downstream.

**Player-facing decomposition — "L + SFX":**
- **L** = `baseVibe` (`SongHype01 × MaxVibeFromSongHype`) — the performance-driven part. **Volatile**: rises/falls with SongHype during the song. At song end it becomes per-enemy via `× impressionFactor(i)`.
- **SFX** = the accumulated flat bonus (`pendingVibe`). Same for every non-blocked enemy, **monotonic** (only grows — "banked").
- **Per-enemy preview** ≈ `round(L × impressionFactor(i)) + SFX`.

The L/SFX split is deliberate: it makes the volatility legible. SFX is "banked" (safe), L is "current performance" (can swing). A single blended number would look erratic on a bad loop; the split reads as "this much is locked in, this much depends on how you keep playing."

## 3. The three surfaces (C1 → C2 → C3)

Build in this order; each enables the next.

- **C1 — Global accumulator readout.** A number under the SongHype bar showing `L + SFX` (temporary format literally `L + SFX = N` is acceptable for S5a). The thing the player watches to know "how's it going." Updates at **loop boundaries**, not per frame (avoids flicker on the volatile L).
- **C2 — Per-enemy effectiveness telegraph.** On each audience member (anchored near their existing persuasion bar), a qualitative indicator of `impressionFactor(i)` from their **live** running `avgImpression`. Reuses existing impression data; the only new plumbing is a live `avgImpression(i)` accessor (sum of the current part's running impressions + closed parts). Highest clarity-per-effort.
- **C3 — Per-enemy projected number.** The actual `+N Vibe` each enemy will receive, live, on each audience. This is **C2 + the number** — once C2 has built the accessor and the per-enemy UI anchor, C3 is a small add-on. Same loop-boundary cadence as C1.

## 4. Effectiveness mapping (C2)

Pure visualization of the `impressionFactor(i)` bucket — no new state:

| Audience state | avgImpression | impressionFactor | Telegraph |
| --- | --- | --- | --- |
| Impressed | > 0 | > 1.0 (up to 1.5) | **Super-effective** |
| Neutral | ≈ 0 | ≈ 1.0 | **Normal** |
| Anti-impressed | < 0 | < 1.0 (down to 0.5) | **Not very effective** |
| Blocked / Indifferent | — | gated | **No effect / Immune** |

The four-state form (incl. blocked/indifferent) exists because the Vibe gate is real; the player should see "this one is unreachable right now," not a misleading low number.

## 5. Consistency with the existing sensory layer (one concept, one authority)

The impression signal already drives per-loop floating text (`AudienceReactionEvent`). The telegraph is the **persistent readout** of that same signal — not a competing source. Impression state remains owned by `SSoT_Audience_and_Reactions.md`; this telegraph only presents it. Floater colour language must stay consistent: **cyan** = per-audience song-end Vibe, **gold** = SFX band bonus (per `Design_Demo_Cut §3.1`). The C1/C2/C3 surfaces should slot into that language rather than introduce a third palette.

## 6. Decisions locked (telegraph-relevant)

- **D-S5-COUNTER = B** — live projection, expanded into this per-audience transparency system.
- **D-S5-TELEGRAPH-SCOPE = B** — C1 + C2 + C3, in order C1 → C2 → C3 (C3 closes the batch; degrades cleanly to C1+C2 if time-boxed out).
- **D-S5-SFX-SCALE = A** — SFX bonus stays flat (venue energy), only L respects per-audience preference.
- **D-S5-VIBE = B** — all Vibe delivered at song end; SFX accumulates into a song-scoped `pendingVibe`. (The conversion already fires once per song — confirmed; no conversion bug.)
- **D-S5-VIBE-ARCH = A** — `pendingVibe` is bespoke/song-scoped but shaped so the planned Pending Effects layer (`Design_Pending_Effects_v1.md`) can absorb it (single song-end payout point, no scattered logic).

## 7. Open / deferred

- **Update cadence** — loop-boundary is the v0.1 default; revisit if the playtest wants smoother feedback.
- **SFX impression-scaling** — could unify with L later (D-S5-SFX-SCALE=B) if "flat venue energy" feels inconsistent; not for the demo.
- **C3 number polish** — exact placement / formatting per enemy is a UI-feel pass once C2's anchor exists.
- **Pending Effects generalization** — the broader accumulator (deferred Earworm, pending Stress/Flow/Cohesion) stays in `Design_Pending_Effects_v1.md`, post-MVP.

## 8. Difficulty note (scope boundary)

This telegraph changes **when/how Vibe is shown and delivered**, not the totals. Accumulating the SFX bonus does not reduce Vibe, so it does not make gigs harder. "Gigs too easy" is a **tuning** problem (magnitudes: `sfxBonusVibeStage1/2/3`, `MaxVibeFromSongHype`, the impression factor band) owned by the **S5c** win-rate loop, not by this note.

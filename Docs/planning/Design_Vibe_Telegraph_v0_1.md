# Design — Vibe Telegraph v0.1

**Status:** Implemented (S5a, 2026-06-22); extended (BIGNUM-1, 2026-09-09) — see §10. This planning note is retained as the **presentation** home (shipped-doc precedent: cf. the Tutorial design note). See `planning/active/S5_DemoCutClose_Sub_Roadmap.md` §S5a.
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

**S5f addendum (2026-07-04, D-S5f-8=A) — shipped strings are Spanish.** The on-screen C2/C3 telegraph strings for the Spanish tester build are ESP: `VibeEffectiveness.SuperEffective` → "¡Súper!", `NotVeryEffective` → "Resiste", `Immune` → "Inmune", `Normal` → "Normal". Source: `AudienceCharacterCanvas.LabelFor`. The ENG labels in the table above remain the semantic mapping; ENG originals return on-screen via the S5f-ext localization pass. (Shipped in the S5f riders batch; ST-S5f-R1..R9 PASS.)

## 5. Consistency with the existing sensory layer (one concept, one authority)

The impression signal already drives per-loop floating text (`AudienceReactionEvent`). The telegraph is the **persistent readout** of that same signal — not a competing source. Impression state remains owned by `SSoT_Audience_and_Reactions.md`; this telegraph only presents it. Floater colour language must stay consistent: **cyan** = per-audience song-end Vibe, **gold** = SFX band bonus (per `Design_Demo_Cut §3.1`). The C1/C2/C3 surfaces should slot into that language rather than introduce a third palette.

## 6. Decisions locked (telegraph-relevant)

- **D-S5-COUNTER = B** — live projection, expanded into this per-audience transparency system.
- **D-S5-TELEGRAPH-SCOPE = B** — C1 + C2 + C3, in order C1 → C2 → C3 (C3 closes the batch; degrades cleanly to C1+C2 if time-boxed out).
- **D-S5-SFX-SCALE = A** — SFX bonus stays flat (venue energy), only L respects per-audience preference.
- **D-S5-VIBE = B** — all Vibe delivered at song end; SFX accumulates into a song-scoped `pendingVibe`. (The conversion already fires once per song — confirmed; no conversion bug.)
- **D-S5-VIBE-ARCH = A** — `pendingVibe` is bespoke/song-scoped but shaped so the planned Pending Effects layer (`Design_Pending_Effects_v1.md`) can absorb it (single song-end payout point, no scattered logic).

## 7. Open / deferred

- **Update cadence** — RESOLVED at BIGNUM-1 (D-BN-12=A): song-start · loop · SFX stage · **status
  applied to an audience member**. The status refresh exists so a card that changes what a member
  will take (Wink → Captivated, Cool Dude → Indifference) shows its effect when played, not one
  loop later.
- **SFX impression-scaling** — could unify with L later (D-S5-SFX-SCALE=B) if "flat venue energy" feels inconsistent; not for the demo.
- **C3 number polish** — exact placement / formatting per enemy is a UI-feel pass once C2's anchor exists.
- **Pending Effects generalization** — the broader accumulator (deferred Earworm, pending Stress/Flow/Cohesion) stays in `Design_Pending_Effects_v1.md`, post-MVP.

## 8. Difficulty note (scope boundary)

This telegraph changes **when/how Vibe is shown and delivered**, not the totals. Accumulating the SFX bonus does not reduce Vibe, so it does not make gigs harder. "Gigs too easy" is a **tuning** problem (magnitudes: `sfxBonusVibeStage1/2/3`, `MaxVibeFromSongHype`, the impression factor band) owned by the **S5c** win-rate loop, not by this note.

---

## 9. Implementation status (S5a — 2026-06-22)

Shipped. All three surfaces (C1 global readout, C2 per-enemy effectiveness, C3 per-enemy
projected number) landed per D-S5-TELEGRAPH-SCOPE=B; **D1=A** (single combined cyan
song-end floater, no `SongEndVibeEvent` struct change). SFX delivery moved to a
song-scoped `GigManager._pendingSfxVibe`, paid once at song end after the Flow multiply
(L only), flat SFX added after (D-S5-SFX-SCALE=A); mid-song application removed (no
double-apply). Surfaces: `GigCanvas.SetVibeReadout` (C1); `AudienceCharacterCanvas`
`.SetVibeTelegraph` + the `VibeEffectiveness` enum (C2/C3); `GigManager`
`.GetLiveAvgImpression` / `.ApplyFlowToLPart` / `.RefreshVibeProjection`. Verified via the
`[S5a-SMOKE]` log family — core ST-S5a-1..4, 9 PASS; ST-S5a-5/6/7 Dev-Mode-deferred;
ST-S5a-8 low-risk-confirmed. The C2 "Normal" band is literal (avgImpression == 0 within
epsilon) — the epsilon-band option in §7 stays deferred. This note remains the
presentation home; the math stays in `SSoT_Scoring_and_Meters.md` §6.

---

## 10. BIGNUM-1 (2026-09-09) — legibility pass

S5a shipped the three surfaces; playtesting showed the player still could not read what the
cards and the song were doing to the Vibe. Nothing here changes the math: `SSoT_Scoring §6`
is untouched and the total applied at song end is byte-identical (regression ST-BN-1, PASS).

### 10.1 C1 — the big number (D-BN-1=A, D-BN-4, D-BN-13=A)

- **Own visibility switch.** `GigPresentationSO.showVibeReadout`, decoupled from
  `showSongHypeBar`. D-S5f-6=B stands: gig 1 hides the bar and shows the number.
- **Size and colour scale with magnitude, measured against the crowd** (D-BN-4):
  `ratio = (L + SFX) / mean MaxVibe of the unconvinced`. 1.0 means "one average member's
  worth this song". Fixed thresholds were rejected: 25 is huge in gig 1 and a scratch in
  gig 3, and a fixed ramp would paint both the same.
- **Beat pulse** (D-BN-3=A): `VibeReadoutBeatPulse`, a `MidiListenerCanvasBase` on the same
  MIDI grid clock as `BeatPulseIndicator`. Pop strength scales with the same magnitude.
  See §10.5 for the debt.
- **Breakdown as a hover tooltip** (D-BN-13=A): the permanent `L + SFX` label is gone; the
  number stands alone and hovering it shows the band-wide chain (Hype % → L, + SFX = N, and
  what an average member can take). Per-member tastes and statuses are NOT here — they are
  per enemy and live on the bar (§10.2).

### 10.2 C3 — what each enemy actually gets (D-BN-2=A, D-BN-8=B, D-BN-14=A)

The projection walks the SAME steps as the song-end payout, gate included, via
`GigManager.BuildVibeProjection` → `ComputeLPart` (§6.1) → `ApplyFlowToLPart` (§7.1) →
`AudienceCharacterStats.PreviewIncomingVibe` (the gate). One implementation, two surfaces.
Before BIGNUM-1 the projection carried Flow and SFX but ignored Captivated (F-BN-3).

Three surfaces per member:
- the `-N` number (as in S5a), tinted with the KO colour when `Final ≥ CurrentVibe`;
- a **predicted-loss "ghost" segment** on the resistance bar, spanning
  `[current − Final, current]`. `GigManager` clears it immediately before the song-end
  `ApplyIncomingVibe`, so the real bar lerps down through where the ghost was: the
  prediction becomes the payment on screen;
- a **hover tooltip on the bar** with the step-by-step breakdown, in payout order:
  `Hype → × tastes → × Flow → + SFX → × Captivated / Indifferent → final`.

**Ghost colour (F-BN-6):** it must NOT reuse the bar's own colour at reduced alpha — tested
and indistinguishable. Use a contrasting tint; keep it consistent with the Vibe colour
language of `Design_Sensory_Contract`.

**D-BN-16=A:** `showVibeProjectedNumbers` governs the `-N` text only. The ghost is its own
surface and survives that toggle — turning the number off to reduce clutter should still
leave the bar readable, which is the point of the segment.

**F-TIP-6 — the breakdown prints steps that change nothing. OPEN (2026-09-15) → VIBE-READ-1.**
`BuildVibeText` walks the payout order and prints **every** line, including `× 1,00 tastes`,
`× 1,00 Flow (0)`, `+ 0 SFX` and `× 1,00 Captivated (0)`. The code states the reason on the
function itself — *every line is shown even when neutral: the shape teaches the model* — and that
criterion is **reversed here, by observation**, exactly as D-BN-9 was (§10.3): observed in ST-TIP-10
with four of six lines inert, the panel reads as noise and the two lines that move the number are
buried among the four that do not. The pedagogy is paid on every hover forever and collected once.
**Agreed rule:** suppress a line when its **output equals its input** — not when «the multiplier is
1,00», which would keep a 1,004 factor that does move the number and would need a second rule for
`+ 0`. Guard rails: the first line (Hype) and the last (`Al acabar la canción: −N`) always print, so
the panel can never be empty nor end without its conclusion; the `Blocked` and `Indifferent`
branches are untouched, since they *explain a zero* and are the case where the reason matters most.
The same shape exists on the C1 readout (`+ SFX del local → 0`), whose producer is
`GigCanvas.BuildReadoutTooltip` — the two are fixed together in **VIBE-READ-1**.

**APPLIED 2026-09-16 (TEXT-VIBE-1, which absorbed VIBE-READ-1). One implementation detail is part of
the rule and not below it: the comparison is on RAW values, in the integer domain each step emits.**
Per line: tastes is inert when `Mathf.RoundToInt(HypeBase) == LAfterImpression`, Flow when
`LAfterFlow == LAfterImpression`, SFX when `Subtotal == LAfterFlow`, Captivated when
`Final == Subtotal`. The tastes step rounds its float input with the **same** `Mathf.RoundToInt` that
`GigManager.ComputeLPart` uses, so the two sides of the comparison are measured the way the payout
measures them.

**Why raw and not the printed string.** The first line prints `HypeBase` as `"0.#"` while every later
step is an `int`. Comparing what is *printed* would never suppress «12,4 → × 1,00 → 12», which is
exactly the inert case ST-TIP-10 reported. **Declared consequence, accepted:** with raw comparison a
rounding that changes the displayed number without a factor moving it (12,4 → 12 at × 1,00) is no
longer shown. That is the cost of the rule, it is one comparison away from being reversed, and it is
written here so a future reader does not rediscover it as a bug. Verified by **ST-TV-5**, which exists
for this case alone.

**The C1 readout took the same rule** (`if (sfxPart != 0)`), and in the same pass its five Spanish
literals became `ui_readout_*` templates — population 11 of `systems/SSoT_Game_Text.md` §1, closed
here. Note for gig 1: venue SFX stages ship **locked** (S5h), so the suppressed state is the *default*
one a viewer sees, not an edge case. **ST-TV-1..8 PASS**, including the regression that the readout
still updates **in place** through `UpdateTooltip` without stacking a second panel
(`systems/SSoT_Tooltip_Pipeline.md` §2).

### 10.3 D-BN-9=A — tried and REVERTED (2026-09-09)

The breakdown was first composed into the character's existing hover tooltip, to honour the
PRES-1 "one hover surface per character" invariant. Shipped, observed, reverted: status +
intention + tastes + Vibe in one column was unreadable, and the block that mattered sat at
the bottom. Recorded rather than deleted so it is not re-proposed.

**D-BN-15 — bounded exception to PRES-1.** The bar now has its own hover, of the same class
as the status-icon tooltips. Unity delivers `PointerEnter` child-first, so
`VibeBarTooltipTarget` raises a flag and `AudienceCharacterCanvas.ShowTooltipInfo` stands
down for that hover. **Known limitation:** moving from the bar to the body without leaving
the character does not re-emit `Enter` on the common ancestor, so the composite tooltip does
not return until the pointer exits and re-enters. Same behaviour the status icons already
have; accepted.

### 10.4 Bars visible while predicting (D-BN-17=A)

The S5e-ext policy hides a FULL bar because "there is no information to convey". A predicted
segment IS information about a full bar, so the policy gains a prediction override rather
than an exception: a full bar stays visible while it carries a ghost, and goes back to
hidden between songs. An Immune member projects 0, so their full bar stays hidden — their
`Inmune` label already says what there is to say. Band canvases never raise the flag and
keep their previous behaviour exactly.

**Authority note (BIGNUM-1-DOC, 2026-09-09).** The S5e-ext bar-visibility policy had no SSoT
home before this batch — it lived only in a `CURRENT_STATE` batch row and a changelog entry.
The amended rule now lives in `SSoT_Gig_Combat_Core.md` §12.1, beside the `GigPresentationSO`
switches it depends on. This paragraph is the presentation rationale; §12.1 is the rule.

### 10.5 The beat clock — debt CLOSED (BEAT-1, 2026-09-27)

**Status: closed. D-BN-3=A holds in its pure form: one clock, the MIDI beat grid.**
The debt this section declared (F-BN-5) is gone. Its cause was not the `IsPlaying` guard first suspected:
the composition path plays under `jam-raw::<guid>` keys that are never cached, and the grid only started
on cached playbacks (F-P1-8, found in PAUSE-1). BEAT-1 made the grid read the bytes being played and
advance on the sequencer tick (`SSoT_Runtime_CompositionSession_Integration` §3.4.1). Retired with it:
- the fallback timer `selfTimerWhenGridSilent` (D-BN-3b=B), deleted from `VibeReadoutBeatPulse`. It was
  ON in `GigCanvas.prefab`: until BEAT-1 the readout pulsed only because of it;
- the tempo push `GigManager.ApplyBpmToStage → GigCanvas.SetReadoutTempo → SetFallbackBpm` (D-BN-3c),
  deleted. Tempo **and** time signature now arrive from the grid, so a non-4 denominator pulses correctly
  (verified in 3/4, ST-B1-2).

The `[VibePulse]` probe (`logBeats`) stays in code and ships OFF in the prefab.

### 10.6 Tooltip anchoring (D-BN-18=B″ — RETIRED 2026-09-15 by TIP-1)

**Status: historical.** The anchoring truth now lives in `systems/SSoT_Tooltip_Pipeline.md` §3.
TIP-1 closed **F-BN-8** by inferring the anchor space from the target's **root canvas**
(D-TIP-2=B): a target under a Screen Space – Overlay canvas has its `.position` taken as the
screen point with no projection at all, so `VibeReadoutTooltipTarget` anchors on its own transform
with a pixel offset. The runtime anchor GameObject described below — created, kept alive with
`DontDestroyOnLoad` and re-parked every frame — is **deleted from the code**; ST-TIP-3 verifies it
no longer exists at runtime. The paragraph is kept because the two earlier failed attempts (B and
B′) still deserve to stay recorded, and because **F-BN-9 remains true** as a Unity fact: nothing in
the persistent ALWTTTCore scene can serialize a reference into the gig scene. What changed is that
the C1 tooltip no longer needs one.

**Original record (BIGNUM-1, 2026-09-09):**

The C1 tooltip anchors on a GameObject the component **creates at runtime** and parks, each
frame while hovered, at the world point that projects back to the wanted pixel (the number's
screen position + a tunable offset). Two earlier attempts are recorded as failed so nobody
retries them: **B** (move the number) — the tail wagging the dog; **B′** (a hand-assigned
world anchor) — impossible, see **F-BN-9**. The root cause is **F-BN-8**; the general fix is
**TIP-1**.

### 10.7 Smoke coverage

**ST-BN-1..17 PASS** (ST-BN-1 is the regression that guards the "math unchanged" premise;
ST-BN-13..16 were run on 2026-09-09, after the code closed and before this doc pass).
Recorded in `coverage-matrix.md`.

**TIP-1 additions (2026-09-15).** `ST-TIP-1..10 PASS`, of which four guard this note's surfaces:
ST-TIP-1 (the C1 panel lands at its offset and stays on screen — the direct F-BN-8 regression),
ST-TIP-2 (the breakdown updates in place while hovered, no second panel and no fade replay),
ST-TIP-3 (no `VibeReadoutTooltipAnchor (runtime)` object exists any more) and ST-TIP-10 (the bar
hover still shows its own single panel and the composite still stands down — D-BN-15 intact).
ST-TIP-10 passed **and** produced F-TIP-6 above: a smoke can pass on its stated criterion and still
report what it saw.

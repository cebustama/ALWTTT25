# SSoT_Audience_and_Reactions — ALWTTT

**Status:** Active governed SSoT  
**Scope:** Audience entities, persuasion-side state, preferences, intentions, and reaction contracts for the MVP line  
**Owns:** what an Audience Member is, how audience progress is tracked, when audience reacts, how preferences and intentions are interpreted at game level  
**Does not own:** full gig phase execution (`runtime/SSoT_Runtime_Flow.md`), full scoring conversion math (`systems/SSoT_Scoring_and_Meters.md`), status ontology internals (`systems/SSoT_Status_Effects.md`)

---

## 1. Purpose

This document is the primary authority for **what the audience is and how it behaves in ALWTTT**.

It replaces the previous implicit authority of:
- `reference/AudienceMember.md`
- parts of encounter and scoring notes
- audience-related backlog summaries

This SSoT defines the audience as a **persuasion-side opponent/target**, not as a traditional enemy abstraction.

---

## 2. Core fantasy

- The player is not killing enemies; the player is **winning over people in the room**.
- Each Audience Member is an agent with:
  - a persuasion resistance track,
  - tastes/preferences,
  - a pressure pattern,
  - and a telegraphed next move.
- Audience state is part of encounter truth, but audience reaction meaning lives here.

---

## 3. Canonical Audience Member model

Each Audience Member owns these gameplay-facing concepts:

| Concept | Meaning |
|---|---|
| `Vibe` | remaining persuasion resistance (enemy-HP pool); `0` = Convinced (see §4.1) |
| `MaxVibe` | total persuasion resistance (tuning knob — a tougher member = higher `MaxVibe`); replaces the retired `VibeGoal` (see §4.2) |
| `Preferences` | bias toward or against certain performance/card styles |
| `Abilities` | audience-turn action patterns |
| `Intention` | telegraphed next action category/value |
| `State flags` | e.g. `Convinced` or encounter-specific modifiers |

Rule:
- the audience is a **reactive and pressuring system**
- it does not need a traditional HP/damage model to serve as combat structure

---

## 4. Persuasion (Vibe)

### 4.1 Vibe
`Vibe` is the canonical persuasion meter, stored **inverted** since S5e:
each member's remaining persuasion resistance (enemy HP).

Rules:
- tracked per audience member; initialized to `MaxVibe`
- **decreases** mainly at **Song End** (persuasion damage)
- persists across songs within a Gig unless an encounter-specific modifier
  says otherwise

### 4.2 Convince condition
The convince condition is **pool depletion**:

```text
Vibe <= 0
```

that member becomes **Convinced**. The former `VibeGoal` threshold concept
is retired: `MaxVibe` itself is the member's total resistance and plays the
old VibeGoal's tuning role (a tougher member = higher `MaxVibe`).

### 4.3 Convinced state
Convinced means:
- that audience member's persuasion resistance has been fully depleted
  ("conquered")
- it should no longer count as an unresolved persuasion target for encounter completion
- any residual pressure behavior is encounter-specific and must be stated explicitly by the encounter rules

**Convinced-state presentation (CONV-TINT-1 + 1b, 2026-09-27).** Presentation only; no rule changes.
- **Tint.** A convinced member's sprite takes `GigPresentationSO.ConvincedSpriteTint` (code default
  `GigPresentationSO.DefaultConvincedSpriteTint`, gold; the demo asset uses a blue close to the venue
  background, so the convinced member reads as "joining the crowd"). Priority: convinced > blocked >
  plain — on a convinced member the Blocked ("oscurito") tint is not shown.
- **Canvas concealed.** The member's whole canvas (bar and predicted segment, intention, status icons,
  C2/C3 labels, revealed-taste icon) is concealed by a state `CanvasGroup` on the root of
  `AudienceCharacterCanvas` (`SetConvincedHidden`). Each surface's own visibility rule keeps running
  underneath — the S5e-ext / D-BN-17 bar policy included — and shows whatever it decided if the member
  stops being convinced (Dev Mode only). Tooltips go through the global `TooltipManager`, outside the
  group, so they carry explicit gates: intention/taste, Vibe breakdown and "oscurito" never open on a
  convinced member.
- **Single point.** `AudienceCharacterBase.RefreshStatePresentation()` (tint + canvas) is the only writer
  of the member's `SpriteRenderer.color` and of the state group. It runs from the `IsBlocked` setter —
  the funnel that `GigManager.RecalculateAudienceObstructions` drives on every member after any convince
  or Dev un-convince — and from `OnConvinced`. If a later phase stops recalculating obstructions at those
  moments, this hook must move with it.
- **Unchanged.** `IsBlocked` keeps its gameplay value on a convinced member (AoE exclusion,
  `AudienceBlockedEvent`); per-loop reaction floaters are still emitted (D-H3=A, F-CT-5). A dedicated
  "convinced" icon is deferred; when it exists it must live outside the concealed group.

---

## 5. Reaction timing

Audience meaning spans two time scales.

### 5.1 Loop scale
At loop-finished, `GigManager.TriggerAudienceMicroReactions` calls
`ResolveLoopEffect(LoopFeedbackContext)` on every audience member. Each
returns a per-loop impression in [−2, +2] (see §6).

Player-facing surface (per Sensory Contract D2):
- Per-audience floating text spawned at the audience member's
  `TextSpawnRoot`: `WOW!` (+2, gold), `YEAH` (+1, green), `…` (0, muted
  grey — darker than MEH), `MEH` (−1, light grey), `BORING` (−2, red). Word
  exclamations, not numerals (D-F-4=A). Neutral emits FT too — no silent
  impressions.
- Audio SFX deferred to S3-audio (D-S3-1=B; no audio subsystem yet).
- Reaction animator deferred (D-F-5b): no reaction animator state exists;
  only the audience *ability* trigger (e.g. Kid "Tantrum", restored in S3a
  per D-F-5a) does.
- Neutral "…" legibility against busy venue backgrounds is addressed by a TMP
  outline material on the FloatingText prefab (D-S3-2=A, applied Unity-side).

Impressions are accumulated per audience per part
(`GigManager._audienceLoopImpressionsByPart`) and consumed at song-end (§5.2).
Emission is **bus-only** as of S3a: `GigManager` publishes `AudienceReactionEvent`
and `SensoryFxAdapter` (Spawn) is the sole FT source; the S1 direct call is
deleted (D-S3-4=A). Song-end FT uses the int `SpawnFloatingText` overload to
preserve the S1 random-diagonal drift (D-S3-5=A).

### 5.2 Song-end / audience-turn scale
At Song End:
- loop-level accumulated reaction context is converted into persuasion damage (Vibe depletion)
- then audience executes its pressure turn or equivalent encounter-side reaction step

This timing must stay aligned with:
- `systems/SSoT_Scoring_and_Meters.md`
- `runtime/SSoT_Runtime_Flow.md`

### 5.3 Indifference interaction (D-F-2=A, S1)

Indifference does **not** zero `ResolveLoopEffect` output. Loop-level
impressions remain live and visible (the player still sees how the
audience reads the music). Indifference gates only the song-end Vibe
conversion, at the canonical `ApplyIncomingVibe` path, where the blocked
conversion surfaces as an `INDIFFERENT` floater instead of `+N Vibe`.
Rationale: Indifference means "doesn't get convinced", not "doesn't
perceive music". Single gate = no double-blocking, and keeps
`LoopScoreCalculator` / SongHype decoupled from per-audience status state.

**Captivated layering (R1, 2026-07-23).** The same `ApplyIncomingVibe` gate hosts the Captivated
amplification layer, applied strictly *after* the Indifference check: blocked stays 0 regardless of
Captivated stacks. Captivated likewise does not touch `ResolveLoopEffect` output — it modulates only
what crosses the Vibe boundary, keeping the single-gate property intact. Status spec:
`SSoT_Status_Effects.md §5.8`.

**Form of the gate (BIGNUM-1, D-BN-6=A).** "One gate" is now materialised as two methods on
`AudienceCharacterStats`: `PreviewIncomingVibe(statuses, incoming)` computes the gate and
returns an `IncomingVibePreview` (applied amount, Indifference stacks, Captivated stacks and
multiplier) **without touching state**; `ApplyIncomingVibe` calls it and applies the result.
The gate is still written once. The projection/telegraph calls the preview, so the number
shown to the player and the number applied at song end cannot diverge — a UI-side copy of
the gate is prohibited (D-BN-0).

Reading, not applying, has one deliberate difference: the preview reads both statuses even
when `incoming <= 0`, so the bar tooltip can show *why* a zero is a zero.

---

## 6. Preferences (taste profiles)

Preferences are the canonical way the audience distinguishes one performance
style from another. As of S1 (B3-slate-F, 2026-06-12), they are implemented
as **taste profiles** authored inline on
`AudienceCharacterData.TastePreferences` and consumed by
`AudienceCharacterBase.ResolveLoopEffect(LoopFeedbackContext)`.

### 6.1 Taste axes (4, frozen at S1)

| Axis | Loop input | Match (+1) | Mismatch (−1) |
|---|---|---|---|
| Tempo | `LoopFeedbackContext.TempoScale` (cumulative TempoEffect ScaleFactor; 1.0 = authored default) | `tempoMatchOnFast` && TempoScale > `preferAboveTempoScale` | `tempoMismatchOnSlow` && TempoScale < `dislikeBelowTempoScale` |
| Arrangement density | `LoopFeedbackContext.ActiveTracks` | `roleCountMatchOnRich` && ActiveTracks ≥ `preferAtLeastRoles` | (one-sided; no mismatch branch) |
| Time signature | `LoopFeedbackContext.TimeSignature` | TS ∈ `preferredTimeSignatures` | TS ∈ `dislikedTimeSignatures` |
| Tonality | `LoopFeedbackContext.Tonality` | Tonality ∈ `preferredTonalities` | Tonality ∈ `dislikedTonalities` |

**Role axis (S7 ELENCO-1, 2026-11-12).** A fifth axis, `preferredRoles` / `dislikedRoles`
(`MidiGenPlay.TrackRole` lists on `TastePreferences`): +1 when any active track's role is preferred,
−1 when any is disliked, 0 when both or neither. Empty lists contribute 0, so pre-S7 assets are
unchanged. The taste panel prints the roles in Spanish (`+ Con bajo`, `- Ritmo`). Used by Rayas
(likes Bassline), Tentáculos (dislikes Rhythm), Big Guy (likes Rhythm), Influencer (likes Lead).

`RootNote` is intentionally NOT a taste axis (D-F-1, S1): pitch class without
modal context carries no archetype meaning; Tonality covers the modal axis.

**Presentation note (F-R4-2, no batch assigned).** Time-signature and tonality
values are currently shown to the player as their **enum names** (`SixEight`,
`FourFour`, `Aeolian`) — legible for the team, not for the player. A mapping to
domain text (`6/8`) is missing. Registered as F-R4-2 at R4 (2026-08-10).

### 6.2 Combination rule (D-F-1=A, discrete per-axis count)

Per loop, each enabled axis contributes +1 / −1 / 0. The sum is clamped to
**[−2, +2]** — the per-loop impression. Empty / disabled axes contribute 0,
so an asset with no taste fields set is a **neutral archetype** (always 0).
This is also the backward-compat path for any audience asset authored pre-B3.

### 6.3 Authority split

- This doc owns: axis meaning, combination rule, the [−2, +2] impression
  contract, neutral-on-empty rule.
- `systems/SSoT_Scoring_and_Meters.md` §6 owns: how accumulated impressions
  convert to Vibe at song-end (the impressionFactor multiplier).
- Asset-side thresholds (per-archetype values) are content, not contract.

### 6.4 Preference reveal (R4, 2026-08-10)

- Runtime state: `AudienceCharacterBase.PreferencesRevealed`, instance scope
  (GameObject lifetime ⇒ per-gig), **not persisted across gigs**.
- Idempotent: revealing the same member twice is a deliberate silent no-op.
- Surface (PRES-1 · D-PRES1-3=A): the taste text **composes into the single
  hover tooltip** (`AudienceCharacterCanvas.ShowTooltipInfo`), plus a discreet
  persistent icon as the "revealed" marker. One hover surface per character is
  a canvas invariant; a second persistent panel violated it and overlapped the
  "Songs left" readout.
- The tooltip composition needs **no prefab wiring** and works regardless of
  icon state. Composition also covers the phases with no active intention,
  where the tooltip previously did not appear at all — without it, revealed
  tastes would vanish between turns.
- `IsTastePanelWired` **changed meaning**: it now reports whether the ICON is
  wired, not the retired panel pair.
- Taste block header (PRES-1c · D-PRES1c-1=A): renders bold
  (`<b>— Gustos —</b>`, TMP rich text) so it reads as a section heading inside
  the shared hover tooltip rather than as one more taste line. Verified
  ST-PRES1-7b (the tag renders; it is not printed literally). ESP copy stays
  hardcoded alongside the Blocked tooltip and telegraph labels (D-S5f-7=A /
  D-S5f-8=A); all migrate together in S5f-ext. Known degradation: a body TMP
  with `richText` disabled would print the tags literally — a prefab fix, not
  code.
- Authority: the reveal spec carries **no taste data**. `AudienceCharacterData`
  owns the data; the canvas owns the presentation. A spec carrying text or axes
  would duplicate §6.1 and desynchronize on the first retune.
- **D-R4-10 closed at PRES-1.** The persistent panel is retired; its slots
  (`tastePanelRoot`, `tasteText`) survived briefly as inert fields so live
  prefab wiring would not dangle, and were removed — fields and `TastePanel`
  prefab object alike — in the R5-pre cleanup pass (2026-08-11).
- **BIGNUM-1 / D-BN-15 (2026-09-09).** The resistance bar has its own hover tooltip (the Vibe
  breakdown), a bounded exception to "one hover surface per character" of the same class as
  the status icons. While the pointer is on the bar the composite tooltip stands down
  (`VibeBarTooltipTarget` raises a flag; `AudienceCharacterCanvas.ShowTooltipInfo` yields).
  Limitation: moving bar → body without leaving the character does not restore the composite
  until re-entry. **D-BN-9=A — composing the breakdown INTO the shared tooltip — was built
  and reverted by observation**; read `planning/Design_Vibe_Telegraph_v0_1.md` §10.3 before
  re-proposing it.
- **TXT-4 (2026-09-11) — the taste block and the Vibe breakdown are glossary templates.** Every line
  of `BuildVibeText` and of the taste block is now a `ui_aud_*` template resolved through `GameText`,
  and the status names inside those lines are resolved by **`StatusKey`** through the same registry.
  «Cautivado» in a breakdown line and «Cautivado» in the status-icon tooltip are one string with one
  home; before TXT-4 the breakdown carried its own Spanish literal, which was a de facto second home
  for a status name (**F-TXT-2-1**, now closed). `blocked` stays a concept entry because the Blocked
  tint has no `StatusEffectSO` (Decision E3, `SSoT_Status_Effects.md`). The `<b>— Gustos —</b>`
  header of PRES-1c is now an authored cell, so the note above about ESP copy "migrating together in
  S5f-ext" is superseded: that migration happened, in TXT-4. Text plumbing:
  `systems/SSoT_Game_Text.md` §1, population 5.
- **F-TXT4-8 (2026-09-11) — bar visibility and bar interactivity are decoupled, and must not be.**
  `CharacterCanvas.SetBarVisible` moves `canvasGroup.alpha` and nothing else. The `CanvasGroup` had
  `Blocks Raycasts` off, so the resistance bar was drawn and never received the pointer: the D-BN-15
  tooltip above existed and was unreachable by hover. Fixed by prefab for TXT-4's smokes (one
  checkbox). **The inverse defect is still open:** with `blocksRaycasts` pinned on, a bar hidden by
  alpha 0 (D-BN-17) still eats the raycast for whatever sits underneath. The correct fix is for
  `SetBarVisible` to move alpha **and** `blocksRaycasts` together, because "invisible but clickable"
  and "visible but dead" are the same bug with the sign flipped. Owner: **TIP-1**.

---

## 7. Intentions and telegraphing

Each Audience Member may expose an **Intention** before its pressure step.

The intention exists to make the room readable.

Typical intention families:
- Stress pressure
- anti-momentum / disruption
- focus-fire / single-target pressure
- wide pressure / all-band pressure
- encounter-specific special actions

Rules:
- intentions should be visible before the audience acts
- intention is presentation of the next meaningful audience action, not hidden simulation state

**Where intention text lives (F-TXT4-7, CLOSED 2026-09-11 by TEXT-5).** The header and body a player
reads when hovering a telegraphed intention come from `ConceptGlossarySO.entries`, keyed by
**`AudienceAbilityData.abilityId`** (D-T5-1a=A): one entry per ability, its **name** rendering as the
tooltip header and its **description** as the body (D-T5-1=A). `AudienceCharacterCanvas` resolves them
with `GameText.ConceptName` / `GameText.ConceptDescription` under the pre-existing
`NextAbility != null && CurrentIntention != null` guard, which TEXT-5 did not touch — which is also
why the "ability with no intention assigned" case stays **unverified**: no content reaches it
(deferred to Dev Mode / M1.5).

`AudienceAbilityData.abilityName` and `AudienceIntentionData.contentText` are **developer labels**
since TEXT-5 and reach no player surface — `AudienceCharacterBase.AbilityRoutine` still logs
`AbilityName` when an intention sprite is missing, which is a conformant read (`ssot_manifest.yaml`,
`GAMETEXT-NO-DEV-LABEL`, stated over surfaces and not over fields). `AudienceMemberWizard` shows the
**id**, not the text; authoring the wording is a `GameTextWindow ▸ Concepts` job.

**A shared id is correct (D-T5-5=A).** Two audience characters may author the same ability and
therefore the same `abilityId`; they get **one** glossary entry, not two copies free to drift. The
seeding tool reports this as `SHARED`, never as `DUPLICATE` — a warning that fires on correct content
is a warning people learn to ignore.

**Note on file location, verified 2026-09-11:** `AudienceIntentionData` has its own file under
`Assets/Scripts/Data/Characters/Audience/`, but **`AudienceAbilityData` is a nested class inside
`AudienceCharacterData.cs`** — there is no `AudienceAbilityData.cs`, and a file request that asks for
one comes back empty.

**Authority split unchanged.** This document owns what an intention **means** and which families
exist; `systems/SSoT_Game_Text.md` §1 (population 6) and §5 (ability id group) own where the text
lives and how it resolves. ~~**Open content debt: F-T5-1** — of the seven abilities, two intention
bodies were authored as generic sentences.~~ **CLOSED 2026-09-14 by TEXT-6, and the count was wrong:
all seven were generic**, not two (F-T6-6).

### 7.1 What an intention body says (D-T6-1=C, TEXT-6)

The seven bodies now describe **what the ability does**, in a character voice, under three rules that
exist for mechanical reasons rather than stylistic ones:

- **No numbers.** The telegraphed value is already drawn beside the icon, and a body that repeats it
  would have to be re-authored on every balance change — the same argument that made card
  descriptions fragments rather than sentences (`SSoT_Game_Text.md` §1, population 4).
  ~~There is a sharper reason too: the drawn value is `ActionList[0].ActionValue` … (**F-T6-5**).~~
  **F-T6-5 CLOSED 2026-11-09 by LEGIB-1b:** the drawn number is now the projected Stress per target
  (§7.2), so `ab_heckle` shows `2`, not `x1`.
- **No target scope either (LEGIB-1b).** Who is hit is drawn beside the icon as musician icons
  (§7.2); a body that names the scope can contradict them.
- ~~**No `<link=>` tags.**~~ **Reversed 2026-11-11 (S4b, D7=A, Claudio):** bodies may link
  concepts (Stress, Vibe, Exposed…). Stacking a concept panel under the column is now a declared
  pipeline behaviour (`SSoT_Tooltip_Pipeline.md` §2.2), not an accident a text batch could cause.
  The seven bodies were linked in S4b.
- **The body may not contradict the intention icon beside it.** Where the mechanic and the icon pull
  apart — a Special-move icon over an ability that mostly repositions — the body is what reconciles
  them, which is the whole reason a generic body was worse than nothing: it restated the icon.

**The bodies were written against the assets, not against the ability names.** The action lists below
were read out of the five `AudienceCharacterData` assets at TEXT-6 and are the oracle for the wording;
an ability name is a developer label and describes nothing reliably.

| Ability id | Character | Action list (type · target · value · status) | What the body says |
|---|---|---|---|
| `ab_block_the_view` | Cool Dude (tall) | `MoveToFront · Self · 1` | moves forward one slot; whoever is shorter and behind ends up Blocked |
| `ab_indifference` | Cool Dude | `ApplyStatusEffect · Self · 2 · Indifference` (value hidden) | self-buff: no Vibe gets through while it lasts |
| `ab_heckle` | Cool Dude | `ApplyStatusEffect · Musician · 1 · Exposed` **then** `AddStress · Musician · 2` (delay 1) | one bandmate, Exposed **first**, Stress after |
| `ab_egg_him_on` | Kid | `ApplyStatusEffect · AudienceTall · 1 · Hyped` (value hidden) | buffs the tall one; their Stress hits harder |
| `ab_tantrum` | Kid **and** Kid-TestTaster | Kid `AddStress · RandomMusician · 5` · Kid-Test `AddStress · AllMusicians · 5` | Stress "to the musicians it marks" (the icons say which) |
| `ab_scathing_review` | Influencer (tall) | `AddStress · RandomMusician · 10` | one bandmate takes a lot of Stress |
| `ab_total_indifference` | Big Guy | `AddStress · RandomMusician · 20` | one bandmate takes Stress |

**Correction 2026-11-09 (LEGIB-1b, D6=A): the TEXT-6 table misread three targets.** The serialized
`actionTargetType` values are enum ordinals (`AudienceCharacter=0, Musician=1, AllAudienceCharacters=2,
AllMusicians=3, RandomAudienceCharacter=4, RandomMusician=5, Self=6, AudienceTall=100`): Kid's `5` is
**RandomMusician**, Kid-Test's `3` is **AllMusicians**, and Influencer and Big Guy are `5`, one random
musician, not the whole band. The data was right and the text wrong, so the text was fixed (ES/EN
bodies of `ab_tantrum`, `ab_scathing_review`, `ab_total_indifference`); turning them into
AllMusicians would have multiplied their damage by four. F-T6-4 below still holds with the right
targets (Kid: one musician; Kid-Test: everyone), and LEGIB-1b removes its visible cost: the icons
show the scope, the body no longer states it.

**A shared id is correct; two sharers that disagree are not (F-T6-4, D-T6-5=A).** `ab_tantrum` is
authored by two characters — legitimate under D-T5-5=A — but with **different targets**. One glossary
entry therefore describes two mechanics. TEXT-6 contained this in the wording (the body states the
effect and not its scope) and did **not** touch the assets, because which of the two is right is an
encounter-content decision, not a text one. The rule worth carrying: **a shared id is safe only while
the sharers agree on what the ability does, and nothing in the pipeline checks that** — the seeding
tool reports `SHARED` on the id, never on the behaviour behind it.

### 7.2 What the intention shows at rest (LEGIB-1b, D5=A, 2026-11-09)

**The target is fixed when the intention is shown, not when it fires.** Before LEGIB-1b the target
did not exist until the audience turn (RandomMusician rolled then; Musician read the stress ranking
then), so the screen could say *what* but never *to whom*. Now `AudienceCharacterBase.ShowNextAbility`
locks one musician per ability (`LockMusicianTarget`), shared by all its Musician/RandomMusician
actions. **Only Spotlight changes it afterwards**, at projection and at fire time alike (taunt rule,
`SSoT_Status_Effects` §5.9). AllMusicians is not locked: it is everyone. Consequence: the random roll
happens at the start of the player turn instead of mid audience turn.

**At rest (no hover), the screen shows:**
- on each audience member: the intention icon, the **projected Stress per target** (attacker modifiers
  included: Hyped) with no `x`, hidden for `hideActionValue` abilities and for abilities that deal no
  Stress, and **one icon per musician the ability touches** (`MusicianCharacterData.characterIcon`),
  damage or not (D10=A). Audience-on-audience targets draw nothing;
- on each musician who will lose fortitude: the bar with a **red segment** for the projected loss and
  the text `final/max` with `final` in red; the bar stays visible even when full.

**Previsto = real.** `StressProjector` (`Characters/StressProjection.cs`) is a dry run of the real
audience turn on copied numbers: Feedback DoT first, then each member in turn order (convinced,
inactive and stunned skipped), `ApplyOutgoingStressWithModifiers` → Composure absorbs → Exposed
multiplies (rounded up), with Exposed/Composure/Hyped applications updating the copies under the
container's stacking policy. `GigManager.LateUpdate` rebuilds it every frame of `PlayerTurn` and
`SongPerformance` and clears the musicians' segment in the audience turn. **Not projected: the song.**
A member convinced at song end skips its turn, so the bar reads "what happens if you do not convince
them". Any rule change in `AddStressAction` / `BandCharacterStats` must change the projector too.

**Hover views and Tab are S4b** (decided 2026-11-09, D8=B): see §7.3.

**Head UI on fitted variants (S7, 2026-11-12).** Variants of `AudienceCharacter - Base` inherit a
canvas laid out for Kid's height. The editor fit («Ajustar UI», `CharacterSizing.FitHeadUI`) moves the
intention and the Vibe telegraph by the difference between the sprite's top and the Base sprite's top,
puts the **status-icon row just above the head** (under the feet the card hand covers it — first try
reverted), and fits the `BoxCollider` to the sprite (cards target it; the hover column hangs from its
top-right corner). At runtime `AudienceCharacterCanvas.LateUpdate` stacks the intention **above the
status row** while it shows icons, and keeps the over-head placement while hovering when the health
bar sits at the feet; the prefab position is used only when the bar is above the head (hand-tuned
Big Guy, Influencer, Cool Dude, never fitted).

### 7.3 Who hits whom on hover (S4b, 2026-11-11)

The rest view (§7.2) says *how much* and *to whom* with icons; S4b makes the link spatial. All of it
is presentation over the same `StressProjection` — no rule changes (projection and real hit untouched).

- **View 1 — hover a musician (body or icon):** the members that will hit them get an outline in
  **that musician's** `identityColor` (D6=A) and a dashed curved line to the musician
  (`IntentLinesView`, `LineRenderer`, D3=A). Members outside the view fade their intention
  (`unfocusedIntentAlpha`, default 0).
- **View 2 — hover a member (body, intention icon or a target icon):** its targets are outlined and
  linked the same way; the ability panel opens with the **hit phrase** as its first, bold line
  (D4=A): «A Conito: 5 de Stress», «A toda la banda: 5 de Stress cada uno», and the hidden-value
  variants without a number (D2=A). Templates `ui_aud_hit_one/_each/_all/_hidden/_all_hidden`.
- **Tab held:** every line at once (D12=A). **Holding a card switches the hover views off**
  (`HandController.IsHoldingCard`): targeting owns the outline then.
- **Red damage aura (D5=A):** musicians the projection says will lose fortitude carry a red outline
  at rest — the lowest-priority channel of `SpriteOutlineController` (hover > view > damage).
  Composure that absorbs the hit removes it live.
- **Target icons (D1=B):** each face sits on a dark disc ringed in the musician's `identityColor`
  (runtime circle sprite, no asset). **Whole band = one icon (D10=C):** a ring split in the
  identity colours of the musicians hit with «×N» in the middle; 2–3 targets that are not the whole
  band overlap as an avatar stack (`targetStackOverlap`). «Whole band» means ≥ the musicians on
  stage, the same test the hit phrase uses. Hovering a target icon opens the member's column.
- **Placement:** while the member's bar is hidden, the intention group (icon + number + faces) is
  centred on the sprite and parked just above its head (`placeIntentOverHead`, `headGapWorld`);
  with the bar shown it keeps the prefab height, centred.
- **Musician hover column:** body and icon show the same column (name and details, then statuses),
  mirroring the audience rule of GIG-UI-1.
- **D-UI-3 reviewed (D2=A):** the play pips stay hidden; the unplayable panel names the spent play.

Logs: every view change writes one `[S4b]` line (`Vista 1/2`, `Tab`, `Carta cogida`, `Aura roja`,
`Frase de golpe`, `Icono de toda la banda`, `Intencion sobre la cabeza`).

---

## 8. Audience abilities

Audience abilities are packaged action patterns used during the audience reaction phase.

MVP categories:
1. **Stress pressure**  
   Main pressure channel against the band.
2. **Breakdown shaping**  
   Pressure patterns that push toward Breakdown timing.
3. **Disruption / anti-momentum**  
   Effects that conceptually fight momentum, confidence, or consistency.
4. **Special encounter actions**  
   Rare, encounter-specific exceptions.

Audience members may also carry player-applied statuses that shape their state during the audience turn — Earworm (M4.3) is the first. These statuses are not abilities; they are persistent effects whose runtime contract is owned by `systems/SSoT_Status_Effects.md` and whose tick hooks live in `GigManager.AudienceTurnRoutine`. The audience-side status surface is data-extensible through `StatusEffectCatalogue_Audience.asset`.

**Default single-target selection (R5, 2026-08-11 — D-R5-2=A).** When an audience action targets one musician and no redirect applies, the target is the musician **closest to Breakdown**: the lowest absolute `CurrentStress` (the S5e-inverted pool, 0 = Breakdown). Absolute, not proportional — Breakdown fires at 0 absolute, so remaining points are the mechanical distance to collapse; a proportional rule would sometimes pass over the musician nearer to dropping. Implemented once in `AudienceCharacterBase.SelectDefaultMusicianTarget`, which both the targeting path and the redirect floater consume, so the two cannot diverge. Between S5e and R5 the comparator was inverted and picked the healthiest musician (F-PRES1b-1); this is the focus-fire behaviour the intention families below assume, and the reason protective tools (Keep Cool retarget, Spotlight) are worth playing.

**Weakest = fewest hits to Breakdown (S7, 2026-11-12 — D3=A, amends D-R5-2).** The default
single-target rule now counts what the musician wears: score = (remaining `CurrentStress` + Composure
stacks) ÷ (1 + Exposed stacks × `ExposedStressMultiplierPerStack`); the lowest score wins, ties keep
band order. Composure absorbs first and Exposed amplifies the rest, the same order as
`ApplyIncomingStressWithComposure`. Implemented as `AudienceCharacterBase.BreakdownDistance` inside
`SelectDefaultMusicianTarget`, so every `Musician`-target ability and the redirect floater share it.
The pick is still locked when the intention is shown (§7.2); Exposed applied later in the turn does
not retarget. Log: `[S7] Weakest | <id> → <musician> | <name>=<score>, …`.

**Leader target (S7 ELENCO-1, 2026-11-12 — D5=A).** `AudienceCharacterData.isLeader` marks the boss
of a group (Big Guy in «El jefe y su banda»). `ActionTargetType.AudienceLeader` (= 101) resolves to the
first leader in the audience other than the performer; if that leader is convinced, or there is none,
the action **fizzles** (no target, no effect): convincing the boss leaves the minions with no one to cheer. Projection uses the same branch (`GetProjectedAudienceTargets`), so
Kid's and Influencer's «¡Eso, jefe!» (Hyped +1 on the leader) raise Big Guy's projected «¡BBUUU!».
Glossary concept `leader` (Jefe / Boss). Log once per fire: `[S7] <id> → leader <name>: …` or
`… none unconvinced, … fizzles`.

**Duet (S7 «Croar», 2026-11-12).** `AudienceAbilityData.duetKey`: when another member that can act
(active, unconvinced, not stunned) has a pending ability with the **same key** whose AddStress reaches
the **same musician**, each one's AddStress **base is doubled** for that musician, before Hyped.
`AudienceCharacterBase.FindDuetPartner` is pure and is called by both `StressProjector` (per target)
and the real hit (`ExecuteActionWithTiming`), so the bar shows the doubled number. Because targets are
locked at intention time, the first singer to fire still counts for the second. The frogs use
`RandomMusician` + `duet: croak`: they coincide some turns (1/N), and Spotlight pulls both onto its
holder, which forces the duet. Log: `[S7] Croak duet | <a> + <b> → <musician> | 3 ×2 = 6`.

**Targeting redirect hook (R4, 2026-08-10 — Spotlight).** `AudienceCharacterBase.ResolveTargetsFor` now has a **prior step** on the `Musician` and `RandomMusician` branches: if any musician carries an active Spotlight, the target is substituted by the holder. `AllMusicians` does not pass through the hook. This is the single targeting funnel, so every future audience ability inherits the redirect with no per-ability edit. Status semantics: `systems/SSoT_Status_Effects.md` §5.9.

This SSoT owns the audience-facing gameplay meaning of those categories.
Detailed execution timing belongs to runtime.
Any status semantics used by those abilities belong to the status SSoT.

---

## 9. Audience and encounter relationship

Audience Members live **inside** the Gig encounter, but they are not the same thing as encounter structure.

Split of authority:
- `systems/SSoT_Gig_Encounter.md` owns the encounter envelope:
  - roster framing,
  - song count,
  - victory/failure structure,
  - gig-scoped modifiers
- this doc owns what an Audience Member is and how it reacts/persuades/pressures

**The prefab names its data (S7 fix, 2026-11-12).** `GigManager.BuildAudience` instantiates
`AudienceCharacterData.CharacterPrefab`, and the clone reads `AudienceCharacterBase.audienceCharacterData`
**from the prefab**, not the asset that pointed at it. A variant of `AudienceCharacter - Base` inherits
the Base's reference (Kid): S7's four new members played as Kid until the link was set. The JSON import
now links every prefab to its own data, and «Reparar enlaces prefab ↔ datos» fixes existing ones.

---

## 10. MVP rules

For the baseline MVP, keep these rules true:

- each Audience Member has `Vibe` + `MaxVibe` (the retired `VibeGoal` folded into `MaxVibe`, see §4.2)
- audience Vibe (persuasion resistance) persists across songs within a Gig
- audience preferences are simple and readable
- audience pressure is primarily expressed through Stress-oriented abilities
- intention telegraphing is favored over hidden surprise logic
- audience-side statuses are part of the MVP baseline. Active set: **Earworm** (M4.3, first audience-side status — Vibe DoT), **Indifference** (B3 — blocks all incoming Vibe), **Captivated** (R1 — amplifies all incoming Vibe, ×(1 + N×0.25)). Future audience-side statuses extend this set through `StatusEffectCatalogue_Audience`. Semantics for all three: `SSoT_Status_Effects.md` §5.7–§5.8 (Indifference is documented alongside the `ApplyIncomingVibe` gate, §5.3 above).

---

## 11. Update rule

Update this document when a technical/design change affects:
- what counts as an Audience Member
- how persuasion resistance (Vibe) is defined
- how preferences or intentions are interpreted
- **what an intention body may say (§7.1) or what an ability's action list does**, since the body is
  written against the action list and goes stale silently when the list changes
- what categories of audience pressure exist
- what “Convinced” means at gameplay level

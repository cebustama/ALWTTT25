# Design_Audience_Status_v1 — ALWTTT

**Status:** Partially superseded — §3 (Earworm) authority migrated. §4 / §5 remain active design intent.  
**Scope:** Audience-side status effects for ALWTTT. Covers Earworm (closed M4.3, see §6), Captivated (deferred design intent), and the `ApplyIncomingVibe` helper (deferred).  
**Classification:** `reference (planning)` — **not a SSoT**. §3 Earworm authority migrated to `SSoT_Status_Effects.md` §5.7 on 2026-04-28 (M4.3 closure); §3 here is retained as historical rationale only. §4 (Captivated) and §5 (`ApplyIncomingVibe`) remain authoritative design intent until roster expansion entry.  
**Last updated:** 2026-04-28

---

## 1. Purpose

This document is the frozen design record of the first pass of audience-side statuses in ALWTTT.

It exists because, prior to this session, there were no audience-side statuses in the system despite the `StatusEffectContainer` plumbing being inherited on `AudienceCharacterBase` and the documented intent in `SSoT_Audience_and_Reactions.md` §10 that "audience-side statuses are optional and not required for the MVP baseline." The starter deck design surfaced that Sibi's mechanical identity collapses without an audience-side status, which forced the question.

It is a planning document. Mechanics here are proposals; the implementing SSoT (`SSoT_Status_Effects.md`) becomes authoritative for whatever is ultimately implemented.

---

## 2. Background — why this was not obvious from existing docs

Several facts had to be aligned before this design made sense:

1. **Audience meter is Vibe, not Stress.** The two are separate and belong to different entities. Musicians have Stress; audience has Vibe. Any audience-side status that wants to parallel the musician-side `Exposed` / `Feedback` / etc. must be interpreted on the Vibe path, not the Stress path.

2. **Audience has no Stress path.** `AudienceCharacterBase` and `AudienceCharacterStats` do not implement Stress. `Feedback DoT` on audience (documented as deferred in `SSoT_Gig_Combat_Core.md` §6.5) would require adding a Stress path first. This is not the direction M4 takes; audience statuses in M4 interpret CSO primitives on the **Vibe** path instead.

3. **CSO primitives are meter-abstract.** The `CharacterStatusPrimitiveDatabaseSO.cs` canonical list uses names like `DamageOverTime` and `DamageTakenUpMultiplier`, but these names are markers of **abstract function**, not literal damage. The CSO primitive `DamageOverTime` applied to an audience member means "meter gain over time" — on audience that's Vibe, not Stress.

4. **`StatusEffectContainer` already exists on audience.** `AudienceCharacterBase.BuildCharacter` calls `AudienceCharacterCanvas.BindStatusContainer(Statuses)` (inherited from `CharacterBase.Statuses`). Tick wiring already subscribes `stats.TriggerAllStatus` to `GigManager.OnEnemyTurnStarted`. The plumbing is ready; what's missing is the SO catalog entries, the runtime hooks in `GigManager.AudienceTurnRoutine`, and (for Captivated) an `ApplyIncomingVibe` helper on `AudienceCharacterStats`.

---

## 3. Earworm — implementing in M4.3

> **⚠️ Superseded 2026-04-28.** Authoritative spec is `SSoT_Status_Effects.md` §5.7. The text below is retained as historical design rationale. If this section conflicts with §5.7 in any detail, §5.7 wins.

### 3.1 Identity

- **Key:** `"earworm"`
- **Display name:** Earworm
- **CSO primitive:** `DamageOverTime` (interpreted on Vibe path for audience)
- **Scope:** single audience member
- **Fantasía:** the song is stuck in their head. They can't stop humming it. Without doing anything else, they convince themselves a little more each turn.

### 3.2 Runtime spec

- **Tick timing:** `AudienceTurnStart`.
- **Tick behavior:** on each `AudienceTurnStart`, for each audience member with Earworm stacks > 0, apply `audience.AudienceStats.AddVibe(stacks)` then decay 1 stack. Mirrors the musician-side `Feedback` DoT pattern in `GigManager.AudienceTurnRoutine`.
- **StackMode:** `Additive`.
- **DecayMode:** `LinearStacks`.
- **MaxStacks:** high (e.g., 99 — matches Feedback). Not a design concern at starter scope.
- **IsBuff:** false from the player's perspective this is a **debuff on the audience** in the sense that it works against the audience's ability to hold out. From the *audience's* perspective it's a buff to their Vibe. The `IsBuff` field is typically an author-facing presentational flag; pick whichever renders with the intended visual in the status icon UI (likely: show as "debuff on audience" i.e. negative for them, so `IsBuff = false`). Revisit at authoring.
- **StatusEffectSO asset path:** `Assets/Resources/Data/Status/StatusEffect_Earworm_DamageOverTime.asset` (or equivalent, following the project's naming scheme).

### 3.3 Gameplay meaning

- An audience member with N Earworm stacks will gain `N + (N-1) + (N-2) + ... + 1 = N(N+1)/2` total Vibe over the next N audience turns (absent refresh or cleanse).
- Concrete example: Mind Tap applies `+2` stacks. Audience gains `+2 Vibe` next turn (stacks → 1), `+1 Vibe` following turn (stacks → 0, icon disappears). Total Earworm contribution: `+3 Vibe` beyond the card's direct `+5` instant Vibe.
- Interacts with Flow: Earworm ticks do **not** pass through the Flow-bifurcation formula. Flow amplifies `ModifyVibeSpec` positive deltas on cards; Earworm ticks are independent DoT-style effects that call `AddVibe(stacks)` directly. This is the deliberate parallel to how musician-side Feedback works — Feedback ticks don't get amplified by Stress modifiers; they're their own source.

### 3.4 Applied by

- `Mind Tap` (Sibi identity Action card): `ApplyStatusEffect(earworm, +2, AudienceCharacter)`.
- No other card in the starter applies Earworm.

### 3.5 Why Sibi and not another musician

Thematic pun — Sibi is a worm entity, Earworm is her namesake mechanic. Originally proposed for Ziggy (vocalist — multiharmonies create hooks in real-world music theory) then reassigned to Sibi during the 2026-04-21 session. The pun won.

If future playtest suggests the pairing is weaker than alternatives, reassignment is a 2-line change on one `.asset` file (change the `FixedPerformerType` on Mind Tap). The Earworm status itself is musician-agnostic.

### 3.6 What needs to change in code to implement

1. **New SO asset:** `StatusEffect_Earworm_DamageOverTime.asset`. Fields set per §3.2. Add to `StatusEffectCatalogueSO` entries.
2. **Icon:** one new sprite, wired via `StatusEffectSO.IconSprite` field (no registry changes, per the M1.2 migration).
3. **Runtime hook in `GigManager.AudienceTurnRoutine`:** mirror the musician-side Feedback DoT loop. Pseudocode:
   ```
   foreach audience in CurrentAudienceCharacterList:
       if audience.Statuses.TryGet(CharacterStatusId.DamageOverTime, out var earwormInstance)
          and earwormInstance.Definition.Key == "earworm"
          and earwormInstance.Stacks > 0:
               audience.AudienceStats.AddVibe(earwormInstance.Stacks)
   // Decay handled by container tick on AudienceTurnStart, no explicit call needed here
   ```
   Exact hook location: same method that currently iterates musicians for Feedback ticks, extended to also iterate audience for Earworm. Order: apply Earworm Vibe gains before audience abilities execute (so the audience isn't affecting itself after gaining Vibe).
4. **Smoke tests:** §3.7.

### 3.7 Smoke tests (required at M4.3 closure)

1. **ST-M43-1 — Application.** Card with `ApplyStatusEffect(earworm, +2, AudienceCharacter)` played on an audience member. Earworm icon appears with stack count 2. Audience's Vibe unchanged on the turn of application.
2. **ST-M43-2 — Tick.** Next `AudienceTurnStart`: audience's Vibe increases by 2, Earworm icon stack count reads 1.
3. **ST-M43-3 — Decay to zero.** Following `AudienceTurnStart`: Vibe increases by 1, Earworm icon disappears (stacks reached 0).
4. **ST-M43-4 — Stacking.** Earworm +2 applied, then on the same turn (before audience turn) Earworm +3 applied again. Stack count = 5. Next audience turn: +5 Vibe, stacks → 4.
5. **ST-M43-5 — Does not affect musicians.** Authoring a card that tries to apply Earworm to a musician target should either fail validation at authoring time (preferred) or be a no-op at runtime (acceptable). Earworm has no musician-side meaning.
6. **ST-M43-6 — Convinced audience member is not re-hit.** Earworm on an audience that becomes Convinced mid-tick: either the tick is absorbed harmlessly or the status is cleared. Whichever the implementation picks, it must not produce duplicate Convinced events.
7. **ST-M43-7 — Flow does not amplify Earworm ticks.** Band has 5 Flow stacks. Earworm tick produces `+N Vibe` (N = stacks), not `+N × (1 + 5 × flowVibeMultiplier)`. Confirms the layer separation — Earworm is a DoT source, not a `ModifyVibeSpec` path.

### 3.8 Edge cases / known limits

- **No refresh semantics defined.** If Earworm +2 is applied to an audience that already has Earworm 1, does it become 3 (`Additive`) or 2 (`Replace`)? Decision: `Additive` — consistent with Feedback and with the "catchy song keeps catching more" reading. Noted in §3.2.
- **No interaction with `Convinced`.** If an audience member becomes Convinced via an Earworm tick, the Convinced state takes effect normally (`AudienceCharacterStats.AddVibe` already handles the threshold). Remaining Earworm stacks on a Convinced audience tick harmlessly (since Vibe is clamped) and decay as normal. If this produces weird visuals (icon lingering on a Convinced audience), consider auto-clearing in M3.3 UI polish pass.
- **No interaction with `IsBlocked`.** Blocked audience members are excluded from `ComputeSongVibeDeltas`; should they also be excluded from Earworm ticks? Decision: yes, for consistency. Add the `IsBlocked` check in the runtime hook.

---

## 4. Captivated — deferred design intent (for Roster Expansion)

### 4.1 Identity

- **Key:** `"captivated"`
- **Display name:** Captivated
- **CSO primitive:** `DamageTakenUpMultiplier` (interpreted on Vibe path for audience)
- **Scope:** single audience member
- **Fantasía:** they're locked in. Every push lands harder. The audience member has their attention captured by the band's performance and is less able to shrug off Vibe pushes.

### 4.2 Runtime spec (as currently designed)

- **Tick timing:** `AudienceTurnStart`, decay 1 stack.
- **Effect:** when incoming Vibe is applied via `ModifyVibeSpec` positive, route it through `AudienceCharacterStats.ApplyIncomingVibe(StatusEffectContainer, int incoming)` (new helper — see §5). If the audience member has Captivated with N stacks, the final applied Vibe is `round(incoming × (1 + N × captivatedBonusPerStack))`.
- **Initial tuning:** `captivatedBonusPerStack = 0.25f`. 2 stacks = ×1.5 amplification.
- **StackMode:** `Additive`. **DecayMode:** `LinearStacks`. **MaxStacks:** moderate (probably 5 — matches StS Vulnerable range).

### 4.3 Applied by (intended — deferred with Ziggy)

- Ziggy's identity Action card (TBD name, probably the starter card that targets single audience). When Ziggy joins the roster.
- Potentially audience-side defensive/attacker mirror statuses in M3.2 Audience pressure expansion (out of scope here).

### 4.4 Why deferred

- Sibi carries Earworm in the M4 starter, and the starter has no other Vibe-multiplier source. With Ziggy out of the starter (per the 2-musician scope decision), Captivated has no sender and no user. Implementing it for nobody is wasted batch.
- Captivated requires the `ApplyIncomingVibe` helper on `AudienceCharacterStats` (§5). Earworm does not. Implementing Earworm alone is a smaller batch.

### 4.5 Reassignability

If playtest reveals that Sibi's identity is stronger as Captivated-holder than Earworm-holder, the two statuses can be swapped between her and Ziggy (when Ziggy joins) with minimal cost. The mechanic identities are distinct enough that the bundle Sibi+Earworm and Ziggy+Captivated is one valid pairing; Sibi+Captivated and Ziggy+Earworm is another. The session converged on the former on thematic pun grounds.

---

## 5. `ApplyIncomingVibe` helper — deferred

### 5.1 Purpose

To mirror the musician-side `BandCharacterStats.ApplyIncomingStressWithComposure(StatusEffectContainer, int)` helper on the audience side. Would route all positive `ModifyVibeSpec` applications through a single method that reads Vibe-modifying statuses (currently only Captivated would use it) before calling `AddVibe`.

### 5.2 Proposed signature

```csharp
// On AudienceCharacterStats
public int ApplyIncomingVibe(StatusEffectContainer statuses, int incoming, float duration = 2f)
{
    if (incoming <= 0) return 0;
    
    int modified = incoming;
    
    // Captivated: multiply by (1 + stacks * captivatedBonusPerStack)
    if (statuses != null &&
        statuses.TryGet(CharacterStatusId.DamageTakenUpMultiplier, out var captivated) &&
        captivated != null && captivated.Stacks > 0 &&
        captivated.Definition.Key == "captivated")
    {
        float mult = 1f + (captivated.Stacks * _captivatedBonusPerStack);
        modified = Mathf.RoundToInt(incoming * mult);
    }
    
    // Future audience-side Vibe modifiers can be layered here.
    
    AddVibe(modified, duration);
    return modified;
}
```

### 5.3 Callsite change required (at Captivated implementation time)

In `CardBase.ExecuteEffects`, the `ModifyVibeSpec` positive-delta path currently calls `audience.AudienceStats.AddVibe(finalDelta)`. That call gets replaced with `audience.AudienceStats.ApplyIncomingVibe(audience.Statuses, finalDelta)`.

### 5.4 Why this is not in Earworm's scope

Earworm is a DoT generator — on tick, it calls `AddVibe(stacks)` directly. It is not an incoming-Vibe modifier. The `ApplyIncomingVibe` helper exists specifically to handle statuses that modify `ModifyVibeSpec` deltas **at the moment they're applied**. Earworm doesn't need it; Captivated does.

Implementing `ApplyIncomingVibe` in M4.3 "just in case" would be premature — it adds a code path with no user, and without Captivated's test matrix it can't be validated. Deferring it until Captivated ships is cleaner.

### 5.5 Open question for the future

When `ApplyIncomingVibe` lands, `ModifyVibeSpec` negative deltas (which currently call `RemoveVibe` directly) may or may not route through the same helper. Likely not — negative Vibe is setup/spite, not the main conversion path, and doesn't need status modulation in the near term. But worth reconsidering when the audience-side Vibe stack grows.

---

## 6. Relationship to existing SSoTs

This planning doc does not override any SSoT. At each batch closure, the implementing SSoT takes authority:

- **M4.3 closure:** `SSoT_Status_Effects.md` gains §5.7 (Earworm full spec). `SSoT_Audience_and_Reactions.md` §8 and §10 update to reflect Earworm as the first active audience-side status. This document's §3 retains rationale; the SSoT is the authoritative spec from that point onward.
- **Roster Expansion (whenever sequenced):** `SSoT_Status_Effects.md` gains §5.8 (Captivated), `AudienceCharacterStats` gains the `ApplyIncomingVibe` helper, and `CardBase.ExecuteEffects` routes through it. This document's §4 and §5 retain rationale.

Until those closures, this document is the single source of design intent for the listed statuses.

---

## 7. Source material

- Session report: `Session_Report_Starter_Deck_Design.md` (2026-04-21).
- CSO registry: `CharacterStatusPrimitiveDatabaseSO.cs` (the 21 canonical primitive mappings consulted when choosing `DamageOverTime` and `DamageTakenUpMultiplier` as the underlying primitives).
- `SSoT_Status_Effects.md` §5 (canonical MVP status set, spec template to mirror for §5.7 Earworm at M4.3 closure).
- `SSoT_Audience_and_Reactions.md` §§8, §10 (audience abilities categories, MVP rules — updated at M4.3 closure).
- `GigManager.cs` — specifically `AudienceTurnRoutine` and the existing musician-side Feedback DoT loop, which is the implementation template for Earworm's runtime hook.
- `BandCharacterStats.ApplyIncomingStressWithComposure` — the implementation template for the deferred `AudienceCharacterStats.ApplyIncomingVibe` helper.

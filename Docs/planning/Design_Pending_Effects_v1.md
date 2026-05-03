# Pending Effects — design proposal (post-MVP)

**Status:** Active design proposal — planning only. Not implemented. Scheduled as the first post-MVP gameplay system.
**Scope:** A song-scoped accumulator layer where cards add to a pending bucket during a song; the bucket resolves at song end. Earworm is the first user. Generalized to other meters later.
**Classification:** `reference (planning)` — **not a SSoT**. When this system is implemented, an SSoT (`SSoT_Pending_Effects.md` or equivalent) will be authored and this doc retains historical rationale per the project's standard supersession pattern (see `Design_Audience_Status_v1.md` as precedent).
**Last updated:** 2026-04-28
**Authors:** game design discussion 2026-04-28.

---

## 1. Position in the roadmap

This system is **post-MVP**. The current critical path is M4.4 → M4.5 → M4.6 (demo target). Pending Effects must not delay or distort that path. The earliest reasonable implementation slot is **immediately after M4.6 demo closure**, as the first post-MVP gameplay system batch.

Two reasons to schedule it first post-MVP rather than later:

1. It is the natural articulation of the project's core thematic claim: *"the song is being built; effects bake in until it lands."* A demo that ships without this mechanic shows ALWTTT as a deckbuilder with a music skin. A demo that ships with it (or with the demo's successor demo carrying it) shows ALWTTT as a deckbuilder whose mechanics genuinely emerge from music structure.
2. The accumulator infrastructure is a prerequisite for tempo-coupled card identity (see `Design_Tempo_Identity_v1.md`). Doing it first unlocks that direction.

This document does **not** propose implementing Pending Effects pre-MVP. Mind Tap (M4.6) and any other Earworm-applying card in the starter remain **immediate** appliers — the existing `ApplyStatusEffectSpec` path. Post-MVP, new pending-applier cards are introduced; existing immediate-applier cards stay immediate.

## 2. Mechanic summary

A card may **add to a pending bucket** rather than apply its effect immediately. The bucket accumulates across the song. At song end (after the song's existing Vibe resolution but before the next audience turn's status tick), the bucket resolves: the accumulated value is applied to the relevant target as if a single end-of-song application had been made.

Earworm is the first instance. A card with `AccumulatePendingEarwormSpec(amount: 2, target: AudienceCharacter)` adds `+2` to that audience member's pending Earworm bucket. If three such cards are played during the song, the bucket holds `+6`. At song end, that audience member receives an Earworm application of `+6` stacks via the existing `Statuses.Apply(earwormSO, 6)` pathway.

The bucket is **song-scoped**: it resets at song start. It does not persist across songs.

## 3. Design intent and player fantasy

ALWTTT's central thematic claim is that gameplay structure mirrors a band performing a song: parts, loops, tempo, build-up, payoff. Immediate-effect cards (the current MVP system) treat the song as a passive playback while combat happens around it. Pending-effect cards reverse that: the song *is* the combat, and cards bake into it.

Playing a pending-Earworm card mid-song reads as *"I'm planting a hook in this song that will catch them at the end."* Playing a multiplier mid-late-song reads as *"this whole song was building to this drop."* Both fantasies are unavailable in the immediate-effect system.

This also creates a strategic risk-reward shape that immediate-effect cards do not have. Pending effects are wagered on the song completing well (see §6 — conditional resolution). The player invests now for a payoff later. That's a deckbuilding axis worth the system.

## 4. Architectural sketch (non-binding)

The implementation is small. Sketch only — full design will be authored when this system enters implementation scope.

- **Bucket location:** `CompositionSession`. Song-scoped state belongs to the session, not to `GigManager`. The session already owns SongHype, part feedback, and song-end events; pending buckets join that family.
- **Bucket structure:** keyed by (target, effect type). For Earworm specifically: `Dictionary<AudienceCharacterBase, int>`. For a generalized system, a typed `PendingEffect` interface.
- **Reset:** at song start (`StartCompositionSession`), alongside SongHype reset.
- **Resolution hook:** `OnCompositionSongFinished` already fires before `AudienceTurnRoutine`. Pending resolution becomes a new branch in that path. Architectural placement: **after** `RunSongVibeResolution` (so song-end Vibe payouts are visible first) but **before** the audience `Tick(AudienceTurnStart)` block (so any newly-applied Earworm stacks are present when the audience turn ticks them).
- **New `CardEffectSpec` subclasses:** one per pending-effect type. `AccumulatePendingEarwormSpec { amount, targetType }` is the first.
- **Conditional resolution:** the bucket data structure should carry a `ResolveCondition` predicate from day one even if the predicate is hardcoded `true` in MVP+1. Cheap to add now; expensive to retrofit. See §6.

## 5. Generalization beyond Earworm

The accumulator pattern generalizes to:

- **Pending Vibe** — deferred audience Vibe gain, fires at song end. Different shape from existing immediate `ModifyVibeSpec`. Useful for "buildup" cards.
- **Pending Stress** — deferred musician stress (rare but conceivable for "exhaustion" cards that bite at song end).
- **Pending Flow** — deferred Flow gain, applied when the next song starts. Connects to the Flow-bifurcation system (M4.2).
- **Pending Cohesion damage / repair** — for encounters that punish or reward the song-as-a-whole.

Each pending effect type is a new spec, a new bucket, and a resolution branch. The system itself is the pattern; the specific effects are the content.

The first implementation batch should ship **only Earworm**. Generalization happens in subsequent batches as new pending-effect types are designed.

## 6. Open question — conditional resolution

**MVP+1 commitment:** unconditional resolution. The bucket fires every song.

**Long-term design intent:** conditional resolution. The bucket fires only on a "successful song" — definition TBD. Possible conditions:
- SongHype above a threshold at song end.
- All parts of the song played (no early termination via Breakdown / Cohesion-out).
- A custom encounter-specific condition.

The conditional version creates risk-reward: pending effects are wagered on song success. The unconditional version is just a delayed effect with no risk. The conditional version is the better long-term design; the unconditional version is the cheaper MVP+1 implementation.

**Recommended path:** ship unconditional in the first implementation batch but design the data structure with the predicate slot present. Switch the predicate from hardcoded-true to a real condition in a later batch once the playtest shape is understood.

## 7. Multiplier cards

Once the bucket exists, **multiplier cards** are obvious content:
- `Encore!` — *"Double pending Earworm on a target."* Action card, between songs ideally, expensive (3+ Inspiration).
- `Discordant` — *"Halve all pending Earworm on all audience."* Anti-pending counter for encounters that play against the player.
- `Crescendo` — *"Add 1 to pending Earworm per loop completed in this part."* Composition-card, tempo-aware (see `Design_Tempo_Identity_v1.md`).

**Balance note:** late-song multipliers are the strongest play in any pending-effect system. They must be costly. A 1-Inspiration "double pending Earworm" played in the final loop is degenerate. Cost / timing constraints are mandatory, not optional.

## 8. Authoring vocabulary (proposed)

When the system enters implementation scope, the JSON authoring vocabulary will gain:
- `effects[].type = "AccumulatePendingEarworm"` — adds to the pending bucket. Fields: `amount`, `targetType`.
- `effects[].type = "MultiplyPendingEarworm"` — multiplies the bucket. Fields: `factor`, `targetType`.
- (Future pending-effect types follow the same shape.)

These slot into the existing `EffectJson` discriminator pattern (`SSoT_Card_Authoring_Contracts.md` §5). No restructuring of the JSON schema is required.

## 9. Relationship to existing SSoTs

When implemented:
- A new SSoT (working title `SSoT_Pending_Effects.md`) will own the pattern. Likely placement: `Docs/systems/`.
- `SSoT_Card_System.md` will gain a section recognizing pending-effect specs as a category alongside immediate-effect specs.
- `SSoT_Status_Effects.md §5.7` (Earworm) will gain a note that pending-Earworm is a separate authoring path with the same target effect.
- `SSoT_Runtime_CompositionSession_Integration.md` will gain a note about song-scoped bucket lifecycle.
- `Design_Starter_Deck_v1.md` is **not** affected. The starter deck remains immediate-effect-only.

## 10. Update rule

Update this document when the post-MVP planning conversation revisits the mechanic. When this system enters implementation scope, supersede §2–§7 with the corresponding SSoT sections following the standard pattern (banner at superseded section header, header metadata flagged Partially superseded). §8 (vocabulary) and §9 (SSoT relationships) become the implementation seed.
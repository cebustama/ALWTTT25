# Tempo-coupled card identity — design direction (long-term, post-MVP)

**Status:** Active design direction — planning only. Not scheduled for implementation. Captured to influence starter deck and per-musician card-catalog choices without committing runtime to anything.
**Scope:** A pillar where cards interact with song tempo, producing meaningful "fast-favoring" vs "slow-favoring" deck identities. Frames metal/jazz/improvisational/etc. archetypes as deckbuilding axes.
**Classification:** `reference (planning)` — **not a SSoT**. Likely never becomes an SSoT directly; instead, individual implementation batches authored later will produce SSoT updates to the affected systems (cards, scoring, runtime).
**Last updated:** 2026-04-28
**Authors:** game design discussion 2026-04-28.

---

## 1. Position in the roadmap

**Long-term post-MVP.** No implementation commitment. This document exists for one purpose: **to prevent the starter deck and per-musician card catalogs from being designed in a way that closes off tempo coupling later.**

It is explicitly downstream of:
- M4.6 demo closure (Combat MVP + starter deck shipped),
- the Pending Effects system landing (`Design_Pending_Effects_v1.md`) — many tempo-coupled card behaviors compose naturally with pending-effect accumulators,
- enough playtest evidence to know what the existing meter stack actually feels like before adding tempo as a new input axis.

This document is intentionally low-detail. It does not propose mechanics; it captures direction.

## 2. Design claim

ALWTTT's most distinctive sales pitch is that gameplay genuinely emerges from music structure. The Pending Effects system establishes that *time* (parts, loops) is a gameplay axis. Tempo coupling establishes that *pace* is a gameplay axis. Together, they articulate ALWTTT as a music-themed deckbuilder where the music isn't decoration — it's the system.

Concretely: a "metal" deck (slow, heavy, payoff-on-the-drop) plays differently from a "fast jazz" deck (high cycle count, opportunistic, more loops per song). Not as flavor text — as actual mechanical behavior driven by the same cards behaving differently in different songs.

## 3. Three interaction axes (sketch)

Tempo coupling, when designed, should span at least three axes to feel real:

1. **Tempo-preferring** — card produces stronger output at one tempo end. *"Steady Beat — adds 1 SongHype; doubled at slow tempo."* Same card, conditional output.
2. **Tempo-requiring** — card is only playable at a tempo band. *"Drop — only playable at slow tempo."* Gates content behind song character.
3. **Tempo-shifting** — card moves the song's tempo when played. *"Build — increases next part's tempo by 1 band."* Player can manipulate the tempo their other cards depend on.

A single axis is gimmicky ("does +1 if slow"). Three axes give cards real strategic shape.

## 4. Why this needs the meter stack to be playtested first

"Slow songs are easier" presupposes a definition of *easier*. Today, SongHype, Vibe, Cohesion, and audience pressure are tuned without tempo as an input axis. Adding tempo as a difficulty modifier means re-tuning everything that already exists.

Implementation order, when this system lands:
1. Existing meters playtested and stable (post-M4.6, post-some-playtest).
2. Pending Effects shipped — the accumulator pattern is needed for many tempo-coupled card behaviors.
3. Tempo as a read-only contract first: cards can *read* `IGigContext.CurrentBpm` (or equivalent) and modify their behavior, but tempo is still set entirely by composition. This is the cheapest first step.
4. Tempo-shifting cards added. This is the architectural commitment — composition pipeline now has a card-influenced input.
5. Difficulty re-tuning across tempo bands.

Each step is a milestone. None can happen pre-MVP.

## 5. Influence on starter deck and per-musician card catalogs

This is the part that matters now.

The starter deck (M4.6) and the early per-musician card catalogs should be designed with tempo coupling in mind, even though tempo coupling is not implemented. Concretely:

- **Card flavor and naming** can lean into tempo identity even when the runtime doesn't yet care. *"Steady Beat"* reads as slow-favoring; *"Quick Riff"* reads as fast-favoring. When tempo coupling lands, those cards retrofit naturally.
- **Per-musician identity** should sketch a tempo lean. The current band — Ziggy / Conito / Robot C2 / Gusano — should each have a soft tempo identity in their catalogs. Even if every card in M4.6 produces immediate effects without reading tempo, the *shape of the catalog* should let later tempo-coupled additions feel like they belong.
- **Avoid card behaviors that contradict eventual tempo coupling.** Specifically: avoid cards whose effects are fundamentally incompatible with the idea of tempo as an input. Most cards are fine; flag any that aren't.
- **Document tempo lean per musician in `Design_Starter_Deck_v1.md`** as design intent, not as a runtime contract. One sentence per musician is enough.

This document **does not require any of this** — it just notes that doing it now is free, and not doing it now means starter deck cards may have to be retconned or replaced later.

## 6. Archetypes (sketches, not commitments)

Captured loosely so the language exists when later design work picks this up:

- **Metal / heavy** — slow tempo, high payoff per loop, drop-shaped songs. Pending Effects (especially multipliers) thematically central.
- **Fast jazz / bebop** — fast tempo, high loop count, opportunistic plays, more cards through the deck per song.
- **Improvisational jazz** — variable tempo within a song, tempo-shifting cards thematically central.
- **Folk / acoustic** — moderate tempo, consistency-focused, minimal swing, rewards stable plays.
- **Electronic / build-and-drop** — explicit slow→fast tempo arcs within songs, payoff at the transitions.

These are sketches. Real archetypes will emerge from playtest.

## 7. What this document explicitly does not commit to

- Tempo as a gameplay input variable in MVP. (No.)
- Re-tuning existing meters around tempo. (No.)
- Modifying composition pipeline contracts. (No.)
- Implementing tempo-coupled cards. (No.)
- Modifying `MidiGenPlay` boundary. (No.)

## 8. Update rule

Update this document when long-term design discussion revisits tempo as a gameplay axis. When implementation work finally begins, this doc is read for direction, then individual implementation batches produce SSoT updates to the systems they touch (cards, scoring, runtime, composition integration). This document does not become an SSoT itself.
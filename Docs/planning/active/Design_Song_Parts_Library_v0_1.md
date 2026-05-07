# Design — Song Parts Library (v0.1, planning only)

**Status:** Planning-only. **Not** committed scope. No batch attached. No implementation slot.
**Authority:** None. Reference / future-design intent.
**Created:** 2026-05-06
**Surfaced from:** M4.6-followup F-5 discussion (per-loop pending workflow, Lectura A confirmed).
**Sequencing:** Downstream of M4.6 demo gate, M4.6-followup closure, and probably also Pending Effects (Post-MVP first batch). No earlier slot makes sense.

---

## 1. Intent

Song structure becomes a first-class gameplay layer. The player composes a song not just as a continuous loop sequence but as a sequence of **named, repeatable Parts** — Intro, Verse, Chorus, Bridge, Outro, etc. Parts are stored mid-gig and replayed via card plays.

This is the long-form structure layer that sits *above* the per-loop pending workflow being designed in M4.6F-5. M4.6F-5 makes the loop the rotation unit; this future feature makes the **Part** a stored, named, repeatable composition unit.

---

## 2. Terminology

To avoid the current code/design name collision:

- **Loop** (after M4.6F-5 rename): the unit of rotation within a section. Card played during loop N → resolves at loop N+1. The "active" track-and-instrument configuration that loops while the song plays.
- **Part** / **SongPart** (this future design): a stored, named, repeatable unit of a song. Holds a snapshot of tracks/cards/instrumentation. Has a name (Intro / Verse / Chorus / Outro / Bridge / Solo / etc.). Played via a card or trigger; when played, becomes the current loop's active configuration.

The current code uses `Part` for the loop concept (`_currentPartIndex`, `BeginDraftNextPart`, `PartFeedbackContext`, etc.). This **must be renamed** at M4.6F-5 batch open to free the "Part" name for this future feature. See M4.6F-5 entry in `CURRENT_STATE.md §4`.

---

## 3. Core mechanic — the "Store Part" card

### 3.1 Concept

A card-effect type: **Store Current Part**. When played, it captures a snapshot of the current loop's track configuration and binds it to a named slot (the card's identity). The card is then discarded; to play that Part again, the player must redraw the card. This means stored Parts are scarce — they're consumable until reshuffle, and authoring a song with structure costs deck slots and turns.

### 3.2 Variants worth considering

- **Store-and-name card** — captures and prompts the player to name (or auto-names from a list).
- **Pre-named storage card** — "Store as Verse," "Store as Chorus" — fixed slots, no prompt.
- **Templated cards** — "Verse Card" already encodes a Part shape on authoring; played to summon that Part directly without needing a prior store action.
- **Combination** — store cards + recall cards + templated cards as three different mechanics that share the same Part data structure.

These are not exclusive; they likely become a content axis where each musician's archetype gets a different storage style.

### 3.3 The recall card

Once stored, the Part needs a way back into play. Options:

- **Same card flips** — after Store, the card's text changes ("Play Verse"); next play resolves the stored Part instead of capturing a new one.
- **Separate recall card** — Store and Play are two cards. Store has more setup cost; Play has more impact.
- **Auto-recall on draw** — if a Part is stored and the player draws an empty Part-slot, it auto-recalls (probably too automatic).

I lean toward "same card flips" for a deck-economy feel — one slot, two behaviors, payoff over multiple plays — but the game-feel decision is open.

---

## 4. Data structure (sketch only)

```csharp
// Pseudocode — final shape TBD
public class StoredSongPart
{
    public string Name;                    // "Verse", "Chorus", custom...
    public PartArchetype Archetype;        // Intro / Verse / Chorus / Bridge / Outro / Solo / Free
    public IReadOnlyList<TrackSnapshot> Tracks;  // captured at Store time
    public int LoopsAtStore;                     // how many loops it ran before being stored?
    public bool ConsumedThisGig;                 // some Parts may be one-shot
    // ... probably more fields
}
```

Open: does the snapshot store **the cards themselves** (so removing a card from the deck removes it from the stored Part) or **a frozen track configuration** (so the stored Part is durable even if the source cards are exhausted/destroyed)? Both have play-feel implications. Frozen-config feels more reliable but disconnects from deck identity. Live-card snapshot rewards deck consistency but punishes attrition.

---

## 5. Open design questions

Listed for completeness; **none of these need answers now**. They become live decisions when the feature gets a batch slot.

1. **Granularity of "current part."** Is a stored Part the snapshot of one loop, or N consecutive loops, or something the player chooses? If N consecutive loops, do they all replay sequentially when recalled, or does the player pick a loop within the Part to advance to?
2. **Modify-after-store semantics.** Once stored, can a Part be modified by playing further composition cards while it's the active loop? Or is the snapshot immutable until re-stored?
3. **Storage limit.** Does the player have unlimited Part slots, or a fixed band of slots (e.g. 4: Verse / Chorus / Bridge / Outro)? Slot-limited has stronger interaction with deck economy but constrains expression.
4. **Interaction with Pending Effects.** Pending Effects (Post-MVP first batch) accumulates song-scoped effects that resolve at song end. Do stored Parts inherit pending effects in their snapshot, or pending effects only attach to the *current* live loop? Probably the latter — Pending Effects is a song-scoped layer, Parts are within-song structural reuse.
5. **Audience reaction granularity.** Does the audience react per-Part (so familiar Parts get reduced novelty bonus on recall — repetition fatigue), per-loop (current behavior), or both? "Audience remembers your Verse" is a meaningful narrative beat; needs a numeric model.
6. **Cohesion / band reaction.** Do certain musicians prefer certain Parts? Do conflicts arise between musicians about Part choice? Probably long-term content-design surface, not core mechanic.
7. **Conditional recall.** Cards like "Play Chorus if Vibe >50" — adds gating. Probably nice-to-have.
8. **Tempo coupling.** Tempo-coupled card identity (`Design_Tempo_Identity_v1.md`) intersects: do stored Parts retain their original tempo, or adopt the current song tempo? If retained, sudden tempo shifts on recall become a card design space.
9. **Authoring layer.** How are pre-named or templated Part cards authored? `CardEditorWindow` extension? New `SongPartCardConfigSO`? Reusing existing payload types?
10. **Deck-builder UX.** When the player is choosing cards for a deck, how is the storage-vs-recall identity surfaced? Does a deck preview show "Verse slot: empty / occupied" overview?

---

## 6. Why not now

- The core composition loop (per-loop pending workflow, M4.6F-5) is itself unimplemented. Designing structure-of-structure on top of a model that isn't validated is premature.
- The Pending Effects system (planned Post-MVP first batch) shares conceptual real estate with stored Parts (both are song-scoped accumulators). Better to ship Pending Effects first, learn its shape, *then* layer Parts.
- The current 2-musician demo (Robot C2 + Gusano Sibi) does not reach the content mass where named-Parts identity reads as expressive. With 3-4 cards per Part archetype, a Verse looks just like a Chorus. Roster expansion (Cantante, Conito, Ziggy) plus per-musician card pools will provide the variety needed for Parts to feel meaningful.
- Code rename (`Part` → `Loop`) at M4.6F-5 must complete before this design's terminology is stable.

---

## 7. What this doc commits to

Nothing implementational. This is the durable record of the design intent so the M4.6F-5 rename leaves room for it. When this feature eventually gets a batch slot:

- Open the batch with a v0.2 of this doc that resolves the open questions to a coherent v1 design.
- Author one minimal-scope demo: one Store card + one Recall card + a tiny audience reaction tweak.
- Build out from there.

---

## 8. Cross-references

- `M46_Followup_Handoff_2026-05-06.md §3.5` — M4.6F-5 surfacing context.
- `Design_Pending_Effects_v1.md` — adjacent song-scoped accumulator system; potential interaction surface (see §5 Q4).
- `Design_Tempo_Identity_v1.md` — long-term tempo design layer; potential interaction surface (see §5 Q8).
- `SSoT_Runtime_CompositionSession_Integration.md` — owner of the loop / part / song lifecycle on the ALWTTT side; will need extension when this feature ships.
- `CURRENT_STATE.md §4` — the M4.6F-5 entry surfaces the rename decision that this doc depends on.

---

*End of v0.1.*

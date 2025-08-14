# Song Infusions, Stage SFX Slots & Song Evolution (ALWTTT)

This section formalizes three connected systems: **Song Infusions** (card-driven stat/effect injections on a running song), **Stage SFX Slots** (limited-capacity pre-song effects), and **Song Evolution** (persistent upgrades chosen after gigs).

---

## A. Song Infusions (Card-Driven)

**Concept.** While a song is selected to perform this turn, cards can **infuse** the song with temporary bonuses, conditions, and triggers that modify its output during the **Song Performance Phase**.

**Inputs**
- Current Song: base stats, Traits, Genre/Theme, Popularity, ModifierSlots. *(See `Song.md`)*
- Played Cards: **Action** cards that prepare the stage and band, and **Modifier** cards that attach directly to the song. *(See `Card.md`)*
- Musician Stats & Position: CHR/TCH/EMT, Front/Back bonuses. *(See `Musician.md`, `GigCombatGameplay.md`)*
- Status Effects: `Inspired`, `Hyped`, `Weakened`, etc.

**Rules**
1. **Attach** `Modifier` cards to the selected Song up to its `ModifierSlots` capacity.
2. Infusions last **until the song resolves** (unless a card specifies multi-song duration).
3. Cards may include **conditional multipliers** (e.g., *Solo Nostálgico*: x2 effect if the Song has the `Melancholic`/`Balada` trait).
4. Vibe output previews reflect **all attached infusions**, position bonuses, and status stacks.
5. Some cards add **temporary slots** (e.g., *Holographic Backdrop*: +1 Modifier Slot this turn).

**Examples**
- *Lightshow SFX* (Action → SFX): +X Groove next turn; if the next song is `Energetic`, also add +1 `Hyped` to Band.
- *Solo Nostálgico* (Modifier): +Y Vibe; **x2** if Song has `Melancholic` or `Ballad`-type trait.
- *Cambio de Vestuario* (Action): +CHR, -Groove this turn, cleanse `Weakened` on Vocalist.

---

## B. Stage SFX Slots

**Concept.** The venue/stage offers a **limited number of SFX slots** (lighting, fog, pyro, projections). Playing an **SFX card** before a song **occupies capacity** and sets ambient modifiers that influence the upcoming performance and enemy reactions.

**Structure**
- `StageSfxSlots: int` – capacity for active SFX.
- `SfxItem` – effect with slot weight (`SlotCost`) and duration (usually **until the next song resolves**).
- Capacity can be **upgraded** across the run (relics, manager upgrades, ship facilities) and can **vary per venue**.

**Rules**
1. When an SFX card is played, check `SlotCost` ≤ remaining capacity.
2. If **full**, player may **replace** an active SFX (remove previous effect; trigger replacement animation/FX).
3. Some SFX require **>1 slots** (e.g., *Mega Láser Galáctico* = 2).
4. Enemies can **react** to visible SFX (e.g., *Cynic* dislikes Pyro; *Mosher* loves Fog), creating tactical tradeoffs.
5. Certain SFX can **manipulate positioning** or visibility (e.g., Fog reduces ranged enemy accuracy; Strobes can `Stun`).

**UI/UX**
- Stage HUD shows slot capacity, active items, and remaining duration.
- Hovering an SFX shows which enemy tags **like/dislike** it and the projected Vibe delta.

---

## C. Song Evolution (Persistent)

**Concept.** Songs **level up** via XP or Inspiration gained whenever they are performed. At **End of Gig**, the player chooses **one upgrade** (by default) among: add Traits, expand slots, tune stat synergies, or tweak Popularity.

**Progression Track**
- `SongXp` → thresholds unlock upgrades (Tier I/II/III).
- Optional **Mastery** level (post-Tier III) with small, flexible perks.

**Upgrade Menu (examples)**
- **Add Trait:** `Corazón Rebelde` (+2 Groove if crowd is Hostile); `Balada Épica` (extend CHR-based effects by +1 turn).
- **Slot Tuning:** +1 `ModifierSlot` (once), or `-1 Groove cost for first Modifier` each time this song is played.
- **Stat Synergy:** +10% Vibe scaling from EMT; `Front Row Anthem` = x1.15 Vibe if Performer is Front.
- **Popularity Curve:** +N Popularity (fan magnet but may attract Haters/Critics).
- **Empire Tag:** Add `Anti-Empire` or `Pro-Empire` trait to enable narrative hooks.

**Rules of Choice**
- Default: pick **1** upgrade per gig; bosses or special events may grant **2 choices**.
- Each song can present **weighted options** based on how it was used (what cards/traits triggered during the gig).

---

## D. Turn Flow Integration (Summary)

1. **Player Phase**
   - Select Song → attach Modifiers (respecting `ModifierSlots`).
   - Play Action cards (including **SFX**, which check **Stage Slots** capacity).
   - Assign Musicians and targets; adjust positions if needed.
2. **Song Performance Phase**
   - Resolve attached modifiers and active SFX.
   - Compute Vibe, Stress, Score; apply enemy reactions.
   - Grant `SongXp` to the performed song.
3. **Audience Phase**
   - Enemies execute actions, including movement and requests.
4. **End of Gig**
   - Choose **one Song Evolution** (or a Stage SFX upgrade, depending on reward table).

> See the companion flow diagram in `ALWTTT_Song_SFX_Evolution_Flow.png`.

---

## Recommended File Placement

- **`Song.md`** → Add a new section **“Song Infusions & Evolution”** under *Musical Attributes*, documenting the per-song slot cap and the post-gig upgrade tree.
- **`Card.md`** → In **SFX Cards**, formalize **Stage SFX Slots**: capacity, replacement, multi-slot items, and enemy reactions.
- **`GigCombatGameplay.md`** → Reference both systems in the **Player Phase** and **Song Performance Phase**, including previews and resolution order.
- Optionally, create **`Song_Infusions_SFX_Evolution.md`** to centralize the system spec and link from the three files above.


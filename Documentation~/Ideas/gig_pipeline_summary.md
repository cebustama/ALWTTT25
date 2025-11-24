# ALWTTT – Current Gig Pipeline (Live Composition Version)

This document describes how the **Gig** works right now in gameplay terms, based on the new **live Song Composition pipeline** running inside the `GigManager` and `CompositionSession` systems.

It’s written from a **game design** point of view: phases, player and audience actions, resources, and how all the systems fit together to create strategy.

---

## 1. High‑Level Loop of a Gig

At a high level, a gig is now:

1. **Gig Setup**
2. **Player Composition Phase** (design a part using cards)
3. **Song Performance Phase** (loops play, micro reactions + SongHype)
4. **Audience Action Phase** (macro reactions & status effects)
5. **Next Song or End of Gig**

This cycle repeats for each song in the encounter.

From a player’s perspective:

- You **build the song in real time** with cards.
- The **music actually plays**, loop by loop.
- The **audience evaluates each loop**, generating **SongHype** and individual **impressions**.
- At the end of the song, the audience **acts back** on the band, changing **Vibe**, **Stress**, etc.

The opponent is therefore **“the room”**: the set of audience members, each with their own tastes and stats, plus the macro encounter rules in `GigEncounter` and `JamRules`.

---

## 2. Core Systems & Responsibilities

### 2.1 GigManager

**Role:** Orchestrator of the whole gig scene.

Key responsibilities:

- Spawns **band** and **audience** based on `GigEncounter`.
- Holds references to:
  - `SongCompositionUI`, `LoopsTimerUI`, `MidiGenPlayConfig`
  - `HandController` for the **gig hand**
  - Lists of `MusicianBase` and `AudienceCharacterBase`
- Owns the **gig phase machine** (`GigPhase`):
  - `PlayerTurn` → player composes
  - `SongPerformance` → song plays & micro reactions
  - `AudienceTurn` → audience actions
- Creates a **CompositionSession** using a `GigContext` that implements `ICompositionContext`.
- Listens to **composition feedback events**:
  - `LoopFinished(LoopFeedbackContext)`
  - `PartFinished(PartFeedbackContext)`
  - `SongFinished(SongFeedbackContext)`
- Owns **SongHype** and drives the **Hype bar UI**.
- Aggregates audience **loop impressions** and converts them to **Vibe** at song end.

In short: `GigManager` is the **match controller** for a gig.

---

### 2.2 CompositionSession + GigContext

**Role:** The “combat engine” for **live composition**.

- `CompositionSession` is the **state machine** for a single song:
  - Manages **parts** (Intro / Verse / Chorus / etc.).
  - Within each part, manages **loops** (N iterations of that part).
  - Tracks **Inspiration** (currency) and **per‑loop inspiration gains**.
  - Generates **LoopFeedbackContext**, **PartFeedbackContext**, and **SongFeedbackContext`** snapshots.
- `GigContext` is the gig‑specific implementation of `ICompositionContext`:
  - Gives the session access to:
    - `SongCompositionUI` (lanes, track slots, buttons)
    - `LoopsTimerUI`
    - `DeckManager` (to draw/discard cards)
    - `MidiMusicManager` and `MidiGenPlayConfig` (for actual audio)
    - The **band** (`IReadOnlyList<MusicianBase>`)
  - Controls visibility of **composition UI** and **hand**.

`CompositionSession` plus `GigContext` is what makes the **Ship composition pipeline** work identically inside the gig scene.

---

### 2.3 SongCompositionUI + HandController + CardData

**Role:** Front‑end of the “combat” system.

- The **player’s main actions** are to drag **Composition cards** from the hand onto lanes in `SongCompositionUI`.
- `HandController.TryPlayInGig` calls back into `GigManager.TryPlayCompositionCard`, which forwards the request to `_session.TryPlayCompositionCard`.
- Each **Composition card** encodes:
  - Which **musician/track role** it affects (Rhythm, Bass, Harmony, Melody, Vocals, etc.).
  - **Tempo / Time Signature / Tonality** changes via `TempoEffect` and other `PartEffect`s.
  - **Generated Inspiration** for the next part (used as *Complexity* in the scoring model).
  - Optionally, a **Synergy type** (CHR / TCH / EMT) that will matter for audience preferences.

From the player’s POV, **cards are the tactical layer**: picking which card to play on which musician, in which part, is how you shape the song and influence the crowd.

---

### 2.4 AudienceCharacterBase + AudienceCharacterStats

**Role:** Individual opponents with preferences and stats.

Each audience member has:

- `AudienceCharacterData` – static config (name, art, tastes).
- `AudienceCharacterStats` – runtime stats:
  - **Vibe** (how much they’re into the show).
  - **Stress / other status** (future extensions).
- A list of **Abilities** (Audience Actions) that are executed during **AudienceTurn**.
- A method `ResolveLoopEffect(LoopFeedbackContext loopCtx)` that turns loop info into an **impression score** in the range **[-2, 2]**.

So each audience member is, mechanically, **a tiny evaluation function** + a **scripted actor** with abilities.

---

### 2.5 LoopScoreCalculator & SongHype

**Role:** Systemic evaluation of each loop’s musical quality.

- `LoopScoreCalculator` is a **pure static class** that converts a `LoopFeedbackContext` into a **LoopScore** (float).
- It looks at things like:
  - Number of **active tracks**
  - Presence of **Rhythm / Bass / Harmony / Melody / Vocals**
  - Overall **TotalComplexity** (sum of Inspiration from track cards)
  - (Future) Synergies, errors, repetition, etc.
- That LoopScore is turned into a **ΔHype** using `ComputeHypeDelta`.
- `GigManager` adds this to `_songHype` via `AddSongHype`, which:
  - Clamps hype between 0 and `maxSongHype`.
  - Updates the **Hype bar** on the gig canvas (normalized `SongHype01`).

This gives you a **global, continuous meter** that says: *“Is the song structurally strong, independent of specific audience tastes?”*

---

## 3. Phase‑by‑Phase Gameplay Breakdown

### 3.1 Setup Phase

Triggered: when the gig scene starts.

Systems involved:

- `GigManager.StartGig()`
- `BuildBackground()`, `BuildBand()`, `BuildAudience()`
- `DeckManager.SetGameDeck()`
- Binding the **gig hand** to `DeckManager` and wiring `SongCompositionUI`.

From the player’s perspective:

- You see the **venue**, **band**, and **audience**.
- Your **deck** is prepared for the encounter.
- UI shows:
  - Hand of cards
  - Composition grid
  - Loops timer & Hype bar (if visible)

Design note: This phase is mostly **setup and framing**, but it’s also where you could surface info about **audience preferences**, encounter goals, etc., so the player starts planning.

---

### 3.2 Player Composition Phase (PlayerTurn)

Triggered by:

- `CurrentGigPhase = GigPhase.PlayerTurn` OR
- After finishing AudienceTurn and starting the next song.

Internally:

- `GigManager.StartCompositionSession()` creates a new `CompositionSession`, subscribes to its events, and calls `_session.Begin(ctx, jamRules, midiGenPlayConfig, rng)`.
- `_isBetweenSongs` is set to **true**, `_isSongPlaying` is **false**.

What the player does:

1. **Draw & play Composition cards** onto musicians/lanes in `SongCompositionUI`.
2. Cards **define the current Part**:
   - Which tracks are active (Rhythm, Bass, Melody, etc.).
   - Tempo, time signature, tonality (via `TempoEffect` & friends).
   - The complexity & Inspiration they will generate for the **next Part**.
3. When satisfied, the player presses **Play**:
   - `SongCompositionUI` calls back into `_session.ConfirmCurrentPartAndStart()`.
   - This transitions into the **Song Performance Phase** for that part.

Resources & info available:

- **Inspiration**: visible on UI, tells you how many/which cards you can afford in the *next* part.
- **Band layout**: which tracks are already present vs missing (e.g. no bass yet).
- **Loop count / part structure** from `JamRules`: how many loops this part will repeat.
- **Hype bar** (global signal of how strong the song has been so far).

Strategically, this phase is about **planning the next few loops**:

- Do you build a **safe, balanced part** (rhythm + bass + melody) to secure a reliable LoopScore?
- Or do you risk **high complexity** on a fragile musician, hoping to spike Hype?
- Do you front‑load complexity in early parts or ramp up later?

---

### 3.3 Song Performance Phase (Loops + Micro Reactions)

Triggered when the player presses **Play** for a part.

Internally, per part:

1. `CompositionSession`:
   - Locks in the current **PartConfig**.
   - Uses `MidiMusicManager` + `MidiGenPlayConfig` to **generate and play** the loops.
   - Tracks how many loops remain (`_loopsRemainingForPart`).
   - At the end of each loop, it builds a `LoopFeedbackContext` and fires `LoopFinished(loopCtx)`.

2. `GigManager.OnCompositionLoopFinished(loopCtx)`:
   - Calls `TriggerAudienceMicroReactions(loopCtx)`.

3. `TriggerAudienceMicroReactions`:
   - Feeds `loopCtx` to `LoopScoreCalculator.ComputeLoopScore`.
   - Converts the LoopScore to **ΔHype** (`ComputeHypeDelta`).
   - Updates `_songHype` and the **Hype bar**.
   - For each audience member:
     - Calls `audience.ResolveLoopEffect(loopCtx)` → **raw impression** [-2, 2].
     - Clamps to [-2, 2].
     - Logs and stores it in `_audienceLoopImpressionsByPart[partIndex][audienceIndex]`.

4. Meanwhile, `CompositionSession` updates **Inspiration** after each loop, based on the per‑loop inspiration of the current part.

Player experience:

- The song plays and **loops**.
- The player sees:
  - The **Hype bar** moving.
  - (Future) small **reaction cues** on audience members (emotes, SFX, UI pulses).

Design‑wise, this phase is the **feedback moment**:

- The player watches how the crowd reacts to the *actual audio*.
- They can mentally link **card choices → arrangement → Hype & impressions**.
- In subsequent parts, they can adjust composition to fix weaknesses (e.g. “This crowd hates slow sparse parts; let’s add bass + faster drums”).

---

### 3.4 Part End & PartFeedbackContext

When the loops for a part are done:

1. `CompositionSession` calls `PartFinished(PartFeedbackContext baseCtx)`.
   - `baseCtx` contains:
     - `PartIndex`, `PartLabel`
     - The list of `LoopFeedbackContext` for this part.

2. `GigManager.OnCompositionPartFinished(partCtx)`:
   - Looks up the aggregated **per‑audience loop impressions** for this part from `_audienceLoopImpressionsByPart[partIndex]`.
   - Creates an **enriched** `PartFeedbackContext`:
     - Same loops
     - Plus `AudienceLoopImpressions: Dictionary<int, List<int>>`
   - Stores it in `_gigPartsForCurrentSong` for song‑level aggregation.
   - Clears the raw dictionary entry for that part.
   - Fires `OnGigPartFeedbackReady(enriched)` for any listeners (UI, analytics, etc.).

From a design standpoint:

- A **part** is the smallest chunk where you can do post‑mortem analysis:
  - *“That chorus got +Hype and strong impressions from most of the room.”*
- It’s a natural hook for **between‑parts bonuses**, card refunds, or short narrative beats.

---

### 3.5 Song End & Macro Song Feedback

When the song is over (no more parts, `CompositionSession.End()` is reached):

1. `CompositionSession` fires `SongFinished(SongFeedbackContext songCtx)` containing the raw list of parts.
2. `GigManager.OnCompositionSongFinished(songCtx)`:
   - Rebuilds an **enriched `SongFeedbackContext`** from `_gigPartsForCurrentSong` (which already include audience impressions).
   - Calls `ApplySongHypeToAudience(enrichedSong)`.
   - Fires `OnGigSongFeedbackReady(enrichedSong)`.
   - Clears `_gigPartsForCurrentSong` and `_audienceLoopImpressionsByPart`.

3. `ApplySongHypeToAudience(enrichedSong)` does:

   - For each audience member, traverse all parts/loops and aggregate:
     - **TotalImpression[i]** = sum of all loop impressions (each [-2, 2]).
     - **SampleCounts[i]** = number of impressions.
   - Compute a **mean impression** per audience member.
   - Compute **baseVibe** from final `SongHype01` scaled by `maxVibeFromSongHype`.
   - Combine baseVibe + impression factor into a **Vibe delta per audience**.
   - Call `audience.Stats.AddVibe(vibeDelta)` for each.

In other words, **SongHype** is the **global quality meter**, while audience impressions are the **personal taste modifiers**. Together they define how much each person’s **Vibe** moves after the song.

This is the **macro payoff** for the entire composition phase.

---

### 3.6 Audience Action Phase (AudienceTurn)

After the composition session ends:

- `GigManager.OnCompositionSessionEnded()`:
  - Unsubscribes from session events.
  - Clears `_session`, sets `_isSongPlaying = false`, `_isBetweenSongs = false`.
  - Sets `CurrentGigPhase = GigPhase.AudienceTurn`.

`ExecuteGigPhase(GigPhase.AudienceTurn)` then:

1. Starts the **AudienceTurnRoutine()** coroutine.
2. For each `AudienceCharacterBase`:
   - Calls `ShowNextAbility()` to pick which ability they’ll use (based on cooldowns, internal logic, etc.).
   - Runs `ActionRoutine()`:
     - For each action in the ability’s `ActionList`:
       - Resolves targets (band, specific musician, global, etc.).
       - Builds `CharacterActionParameters` and executes via `CharacterActionProcessor`.
     - Actions often:
       - Increase **Band Stress**.
       - Modify **Vibe**, **Groove**, or other encounter stats.
       - Apply **status effects** (e.g. heckling, tech issues, buffs/debuffs).

3. When all audience actions are done, the gig returns to **PlayerTurn** (next song) or ends the encounter, rewarding Fans / Cohesion as defined by `GigEncounter`.

Player experience:

- Your song has finished and now the **crowd talks back**:
  - Friendly fans might buff Groove or lower Stress.
  - Hostile crowds might increase Stress or lower Vibe further.
- This gives a **clear sense of consequence** for your performance and sets the tone for the next song.

---

## 4. Strategic Layer & Player Decision‑Making

### 4.1 Information the Player Has

During a gig, the player can see or infer:

- **Band stats** and roles (who is good at CHR / TCH / EMT, who handles which track).
- The current **Inspiration** and how much each card will generate/spend.
- The current **SongHype bar** as a global quality signal.
- (Potentially, via UI) each audience member’s:
  - Preferred **track types** (likes Bass, hates Vocals, etc.).
  - Preferred **synergy types** (CHR jokey cards vs EMT emotional cards).
  - Current **Vibe**.

This is enough to create **meaningful short‑term and long‑term decisions**:

- Short term (per loop / part):
  - “I need Rhythm + Bass at least to keep LoopScore decent.”
  - “This crowd loves melody – let’s add a lead guitar line now.”
- Long term (song + gig):
  - “I want to ramp SongHype so the last chorus spikes Vibe.”
  - “If I don’t stabilize these hostile fans, their abilities will wreck my Stress next song.”

### 4.2 Core Resource Loops

1. **Inspiration Loop (Part‑to‑Part Economy)**  
   - Cards you play now **generate Inspiration** for the next part’s budget.  
   - Spending more cards now can **starve or empower** future parts.  
   - Design‑wise: this is your **mana curve / tempo** system.

2. **SongHype Loop (Loop‑to‑Song Evaluation)**  
   - Arrangement choices → **LoopScore** → **ΔHype** each loop.  
   - Hype is **cumulative across the song** and feeds directly into final Vibe.  
   - Design‑wise: this rewards **consistency and escalation**, not just one good loop.

3. **Audience Impression Loop (Taste‑based Feedback)**  
   - Each loop gives each audience member an **impression** in [-2, 2].  
   - Aggregated over parts/loops, impressions shape how much the **SongHype payoff** helps each individual.  
   - Design‑wise: this is where **crowd reading** and **adapting to preferences** matter.

4. **Vibe & Audience Actions (Meta Consequences)**  
   - Final Vibe per audience member influences how strong / positive / negative their **AudienceTurn abilities** feel.  
   - Over multiple songs, this affects **encounter success, fans gained, band cohesion**, etc.

### 4.3 Why It’s Potentially Fun

- **Expressive play:** you’re literally *writing* the music that determines combat outcomes.
- **Readable feedback:** Hype bar and (future) crowd UI make it clear when you’re doing well.
- **Asymmetric duel:** the opponent is not a monster with HP, but a **room full of personalities**.
- **Planning depth:** parts and loops create a **rhythm of commitment and evaluation**, similar to turns and rounds in a tactics game.

---

## 5. Obvious Next Design Hooks

Just to have them parked in one place:

- Make **audience preferences** explicit in UI (tooltips, icons) so players can truly “read the room”.
- Use `LoopFeedbackContext` and `PartFeedbackContext` to:
  - Show “What worked” summaries between parts.
  - Offer small bonuses or narrative beats based on part performance.
- Expand `LoopScoreCalculator` to include:
  - Repetition penalties/bonuses.
  - Synergy rewards (e.g. CHR cards played by high‑CHR musicians).
  - Stress / status effect modifiers.
- Tie **AudienceTurn abilities** more tightly to **Vibe bands** (e.g. above +10 Vibe → fans throw gifts; below -10 → heckling).

All of this can grow without changing the **core pipeline**, which is already structured around **clean feedback contexts** and **events**.

---

_End of document._


# SOLID‑WAUC Technical Report (v2)
## Supporting Card Effects + Triggered Statuses + Handwritten Card Set in **ALWTTT**

This document updates the previous SOLID‑WAUC report to include **all additional engine requirements implied by the handwritten card concepts** (Draw/Discard, Shuffle curses, OnDraw penalties, AoE, random targeting, per‑loop generation/damage, and Session/Tempo constraints).

**SOLID‑WAUC = SOLID where it pays off, Without Unnecessary Complexity.**

---

## 0) Scope: What we want to author as data

### A) Immediate **Card Effects** (non‑status)
Examples:
- Deal damage (“Deal X Vibe”)
- Draw N
- Discard N
- Shuffle a card into draw pile
- Exhaust this card after play

### B) **Triggered signature statuses**
Examples:
- *Ghostly Notes* → “Generate 2 Inspiration per loop” (+ downside)
- *Percussive Pulse* → “Deals damage per loop” (scales with Flow/Vibe)

### C) **On‑draw penalties / curses**
Examples:
- *Dead Mic* → Unplayable + “On Draw: lose 1 Inspiration”

### D) **Session / Tempo constraints (build‑shaping)**
Examples:
- *Count‑In* → sets tempo variant + time signature, may restrict future rhythm cards

---

## 1) Design constraints / Non‑goals (WAUC)

- No general effect graphs or scripting language.
- No reflection‑driven event bus.
- No monolithic “God Resolver”.
- No complex polymorphic serialization unless Unity forces it.
- Start small: implement only the actions needed by the current card set, but keep extension points clean.

---

# 2) Data Model Updates (Authoring)

## 2.1 CardEffectId (expanded)

Start minimal, but include the set required by these cards:

```csharp
public enum CardEffectId
{
    DealDamage,          // Deal N Vibe / targeted or AoE
    DrawCards,           // Draw N
    DiscardCards,        // Discard N (random or choose later)
    ApplyStatus,         // Apply Hooked / Entranced / Exposed etc (optional if you keep StatusEffectActionData separate)
    ShuffleIntoDrawPile, // Shuffle a specific card into draw pile (Feedback / DeadMic)
    SetSessionConfig,    // Count-In: tempo + time signature + constraints
    GainResource         // Gain Inspiration (immediate gain)
}
```

**Note:** `ApplyStatus` can remain in the *statusActions* list via `StatusEffectActionData` if you prefer strict separation.
However, the handwritten cards mix “deal damage” and “apply status” in one card; having both lanes is fine (see §2.3).

---

## 2.2 CardEffectActionData (expanded)

```csharp
[Serializable]
public struct CardEffectActionData
{
    public CardEffectId effectId;

    // Targeting
    public ActionTargetType targetType;     // Self, SingleEnemy, AllEnemies, RandomEnemy, AllAudience, RandomAudience...
    public TargetFilter targetFilter;       // Optional: AudienceOnly, MusicianOnly, EnemyOnly, etc.

    // Magnitudes
    public int amount;                      // damage/draw/discard/resource amount

    // Optional variants
    public DiscardMode discardMode;         // Random / Choose (start with Random)
    public int delayTurns;                  // if you already support delayed resolution
    public bool exhaustAfterPlay;            // card-level exhaust can be handled here or at Card definition level

    // Payloads (WAUC approach: small discriminated-union pattern)
    public CardRef cardToShuffle;           // for ShuffleIntoDrawPile
    public SessionConfigDelta sessionDelta; // for SetSessionConfig
}
```

### Supporting structs (authoring)

```csharp
[Serializable]
public struct CardRef
{
    public string cardId; // or GUID / Addressable key / ScriptableObject reference
}
```

```csharp
[Serializable]
public struct SessionConfigDelta
{
    public TempoVariant? tempo;         // Slow / Fast / VeryFast
    public TimeSignature? timeSignature; // 4/4, 3/4, 6/8, etc.
    public SessionConstraintFlags constraints; // e.g. LockRhythms, RestrictRhythmTier, etc.
}
```

This keeps `CardEffectActionData` data-driven, but without adding a heavy framework.

---

## 2.3 CardPayload (two-lane model remains)

Keep the two-lane approach (it’s WAUC and keeps intent explicit):

```csharp
public List<CardEffectActionData> cardEffects;      // immediate ops: draw, discard, shuffle, set session, deal damage
public List<StatusEffectActionData> statusActions;  // state ops: apply/remove stacks of CSO statuses
```

### Exhaust as data
You can implement Exhaust in either place:
- **Card definition**: `bool exhaust` on the card itself (best)
- or as a `CardEffectActionData.exhaustAfterPlay` effect

Recommendation: **Card-level exhaust**, because it’s not an effect; it’s card lifecycle.

---

# 3) Runtime Services Needed

## 3.1 Card execution context (unchanged, but add deck/session services)

```csharp
public struct CardExecutionContext
{
    public ICharacter source;
    public GigContext gig;

    public IDamageService damage;
    public ICardDrawService draw;
    public IDiscardService discard;
    public IDeckService deck;               // shuffle cards into draw pile, move between piles
    public IStatusService status;           // apply StatusEffectSO by CharacterStatusId
    public IResourceService resources;      // Inspiration, Vibe, etc.
    public ISessionConfigService session;   // tempo/TS constraints
    public ITargetingService targeting;
}
```

---

## 3.2 Resolver strategy (SOLID)

```csharp
public interface ICardEffectResolver
{
    CardEffectId Id { get; }
    void Resolve(in CardEffectActionData action, in CardExecutionContext ctx);
}
```

Implement only what you need:

- `DealDamageResolver`
- `DrawCardsResolver`
- `DiscardCardsResolver`
- `ShuffleIntoDrawPileResolver`
- `GainResourceResolver` (Inspiration +N)
- `SetSessionConfigResolver` (Count-In)

Registry + Executor (unchanged):

- `CardEffectResolverRegistry`
- `CardEffectExecutor`

---

# 4) Triggered Statuses (expanded to cover “per loop” + downsides)

## 4.1 Minimal trigger authoring (Option A)

Extend `StatusEffectSO` with an optional trigger block:

```csharp
public bool isTriggered;
public StatusTriggerType triggerType;
public TriggeredEffectActionData triggeredAction;

public bool hasDownside;
public TriggeredDownsideData downside; // WAUC: only if needed for Ghostly Notes style tradeoffs
```

### StatusTriggerType (expand a bit)

```csharp
public enum StatusTriggerType
{
    OnCardPlayed,
    OnTurnStart,
    OnTurnEnd,
    OnLoopStart,
    OnLoopEnd,
    OnSongStart,
    OnSongEnd,
    OnDrawCard
}
```

### TriggeredEffectActionData (reuse some targeting)

```csharp
[Serializable]
public struct TriggeredEffectActionData
{
    public TriggeredEffectKind kind; // DealDamage, GainResource, ApplyStatus (start with DealDamage + GainResource)
    public int amount;
    public TargetRule targetRule;    // Self, AllEnemies, AllAudience, RandomAudience...
}
```

### Downside data (WAUC)

Ghostly Notes notes mention “balance negativo” / “big trade-off”. Model as optional, constrained types:

```csharp
public enum DownsideKind
{
    ApplyStatusToSelf,     // e.g. add Exposed to self
    LoseResourceOnTrigger, // e.g. -1 Inspiration each loop
    AddCurseToDeck,        // e.g. shuffle Feedback
    RestrictActionCategory // e.g. lock rhythm actions
}

[Serializable]
public struct TriggeredDownsideData
{
    public DownsideKind kind;
    public int amount;
    public CharacterStatusId statusId; // for ApplyStatusToSelf
    public CardRef curseCard;          // for AddCurseToDeck
}
```

Keep it narrow; add more only when needed.

---

## 4.2 StatusTriggerDispatcher (runtime)

```csharp
public sealed class StatusTriggerDispatcher
{
    public void Dispatch(StatusTriggerType trigger, ICharacter source, GigContext gig);
}
```

Responsibilities:
- Scan active statuses on `source`.
- If `StatusEffectSO.isTriggered` and `triggerType` matches, resolve action + downside.
- Use existing services (damage/resources/status/deck/targeting).

**Keep `StatusEffectContainer` storage-only.**

---

# 5) Curses / Unplayables / OnDraw penalties

The handwritten set has:
- `Feedback` → Unplayable
- `Dead Mic` → Unplayable + OnDraw: Lose 1 Inspiration

### 5.1 Card-level flags (authoring)

Add to your card definition (whatever your CardSO/CardPayload wrapper is):

- `bool isUnplayable`
- `bool isCurse`
- `bool exhaustOnUse` (if needed)

### 5.2 OnDraw triggers

WAUC approach: author a small list of OnDraw effects on the card:

```csharp
public List<CardEffectActionData> onDrawEffects;
```

Then the draw service / hand service calls a dispatcher:

```csharp
public interface ICardDrawService
{
    void Draw(int n);
}

public interface ICardLifecycleService
{
    void OnCardDrawn(CardInstance card, ICharacter owner, GigContext gig);
}
```

`OnCardDrawn` executes `card.onDrawEffects` via the same `CardEffectExecutor` (reuse!).

This reuses existing resolver infrastructure (SOLID + less code).

---

# 6) Targeting requirements from the handwritten cards

You need these target patterns:

- Single Audience Member (selected)
- Random Audience Member
- All Audience Members
- All Enemies
- Random Enemy
- Self

### 6.1 Targeting service

```csharp
public interface ITargetingService
{
    IReadOnlyList<ICharacter> Resolve(ActionTargetType targetType, TargetFilter filter, ICharacter source, GigContext gig);
}
```

Start simple; add “choose target UI” later. Random selection can be done in targeting service (deterministic with seed if you want).

---

# 7) Session / Tempo system (“Count-In”)

Handwritten notes imply:
- Tempo variant: Slow / Fast / Very Fast
- Time signature: ANY or EVEN etc
- Constraints: lock out some rhythm cards / “can’t play more rhythms”
- “Starts the session”

### 7.1 Minimal session config model

```csharp
public struct SessionConfig
{
    public TempoVariant tempo;
    public TimeSignature timeSignature;
    public SessionConstraintFlags constraints;
    public bool isSessionStarted;
}
```

### 7.2 Session config service

```csharp
public interface ISessionConfigService
{
    SessionConfig Current { get; }
    void ApplyDelta(SessionConfigDelta delta);
    void StartSessionIfNeeded();
    bool IsCardAllowed(CardInstance card);
}
```

**Where constraints apply**
- Card play validation (before execution) checks `session.IsCardAllowed(card)`.

WAUC: keep constraints as flags + simple rules; don’t build a full rule engine.

---

# 8) How the handwritten cards map to the system

Below is a direct mapping of each idea to required features.

## 8.1 Cosmic Wink
- statusActions: Apply `Entranced +1` to target Audience

Needs: ApplyStatus via `StatusEffectActionData` (already).

## 8.2 Raw Scream
- statusActions: Apply `Entranced +1` to Random Audience
- statusActions: Apply `Exposed +1` (same target or random)

Needs: Random targeting + apply statuses.

## 8.3 Ghostly Notes
- statusActions: Gain `Flow +5`
- triggered status: OnLoopEnd → GainResource(Inspiration +2)
- downside: optional (LoseResource or ApplyStatus or AddCurse)

Needs: Triggered status (OnLoopEnd) + downside model.

## 8.4 Percussive Pulse
- cardEffects: GainResource(Inspiration +2)
- statusActions: Gain Flow +3
- triggered status: OnLoopEnd → DealDamage(amount) (target likely Audience/Enemies)
- scaling: “accumulates with other bonuses → Vibe payoff” (handled by your existing scaling rules)

Needs: Triggered status (OnLoopEnd) + DealDamage target rules.

## 8.5 Count-In
- cardEffects: SetSessionConfig(tempo+TS+constraints)
- statusActions: +1 Flow, +1 Composure
- starts session

Needs: Session config service + validator.

## 8.6 Triplet Pulse
- cardEffects: DealDamage(X Vibe) repeated N times where N = Flow stacks

Needs: Multi-hit execution.
WAUC: implement as either:
- a `RepeatCountSource` field on `CardEffectActionData` (Flow-based), OR
- treat as status primitive `MultiHitModifier` already in CSO, and card reads it at runtime.

Recommendation: add a small `RepeatMode` on `CardEffectActionData`:

```csharp
public RepeatMode repeatMode; // None, ByStatusStacks
public CharacterStatusId repeatStatusId;
```

## 8.7 Macro Feedback
- cardEffects: DealDamage(X)
- cardEffects: ShuffleIntoDrawPile(Feedback)

Needs: Deck service shuffle.

## 8.8 Feedback (curse)
- isUnplayable = true

Needs: card-level flags.

## 8.9 Mic Drop
- cardEffects: DealDamage(X)
- cardEffects: ShuffleIntoDrawPile(DeadMic)

Needs: deck shuffle.

## 8.10 Dead Mic (curse)
- isUnplayable = true
- onDrawEffects: LoseResource(Inspiration 1)

Needs: OnDraw pipeline.

## 8.11 Mass Vibe Blast
- cardEffects: DealDamage(N) to AllAudience (or AllEnemies)
- card-level Exhaust = true

Needs: AoE targeting + exhaust flag.

## 8.12 Hooked Riff
- cardEffects: DealDamage(N)
- statusActions: Apply `Hooked +X`

Needs: apply status + damage sequencing.

## 8.13 Improvised Jam
- cardEffects: DealDamage(N)
- cardEffects: DrawCards(N)

Needs: DrawCards effect.

---

# 9) Incremental implementation plan (updated)

## Step A — Expand Card Effects authoring (data)
1. Add `CardEffectId` (expanded list)
2. Add fields to `CardEffectActionData`: targeting, shuffle payload, session delta, repeat mode
3. Extend `CardPayload` with `List<CardEffectActionData> cardEffects`
4. Add card-level flags: `isUnplayable`, `isCurse`, `exhaustOnUse`
5. Add `onDrawEffects` list to card definition (not necessarily to payload)

✅ Exit: You can author *Macro Feedback*, *Dead Mic*, *Improvised Jam* as data.

## Step B — Implement deck/hand lifecycle services
6. Implement `IDeckService` (shuffle into draw pile, move cards between piles)
7. Implement `ICardLifecycleService.OnCardDrawn(...)` and execute `onDrawEffects` via `CardEffectExecutor`

✅ Exit: *Dead Mic* penalty works when drawn.

## Step C — Implement resolvers + executor
8. Add resolvers: DealDamage, DrawCards, DiscardCards, ShuffleIntoDrawPile, GainResource, SetSessionConfig
9. Wire `CardEffectExecutor` into “OnCardPlayed”

✅ Exit: Dagger Throw and the handwritten damage/draw/shuffle cards work.

## Step D — Triggered statuses
10. Extend `StatusEffectSO` with trigger block + (optional) downside block
11. Implement `StatusTriggerDispatcher`
12. Wire dispatch calls at loop boundaries and/or song boundaries:
   - `OnLoopEnd` for Ghostly Notes/Percussive Pulse

✅ Exit: “+2 Inspiration per loop” works and scales with stack count if desired.

## Step E — Session/Tempo constraints
13. Implement `ISessionConfigService` and `SessionConfig`
14. Apply constraints during card play validation

✅ Exit: Count-In can lock tempo/TS and restrict certain card categories.

---

# 10) Negative stack policy (unchanged)

Do not support negative stacks in the new CSO runtime.
Use explicit primitives:
- DamageUpFlat / DamageDownFlat
- DamageUpMultiplier / DamageDownMultiplier

This avoids ambiguous signed semantics.

---

## Appendix: Minimal new types checklist

### Enums
- `CardEffectId`
- `StatusTriggerType`
- `TargetFilter`
- `DiscardMode`
- `TempoVariant`
- `TimeSignature`
- `SessionConstraintFlags`
- `RepeatMode`
- `DownsideKind` (optional)

### Structs / DTOs
- `CardEffectActionData`
- `CardRef`
- `SessionConfigDelta`
- `TriggeredEffectActionData`
- `TriggeredDownsideData` (optional)
- `SessionConfig`

### Services
- `CardEffectExecutor`
- `CardEffectResolverRegistry`
- `ICardEffectResolver` + concrete resolvers
- `IDeckService`
- `ICardLifecycleService`
- `IResourceService`
- `ITargetingService`
- `ISessionConfigService`
- `StatusTriggerDispatcher`

---

End of document.

# ALWTTT – Character Status Ontology (CSO)
## Status Effects Primitives (with Genre References) & Core Sets

This document defines the canonical **Character Status Ontology (CSO)** used in *A Long Way to the Top (ALWTTT)*.

The CSO formalizes what a “status effect” is at a systemic level, separating:
- **Stable technical identifiers (EffectId)**
- **Thematic display names** (UX / fantasy / localization)
- **Behavioral parameters** (stacking, decay, timing, scope)
- **Genre references** (how similar mechanics appear in other deckbuilders)

---

## 3. Canonical Status Effect Primitives (with Genre References)

These are the full deduplicated primitives supported by the ontology.
They represent abstract gameplay functions, independent from theme or UI.

> ⚠️ If your Markdown viewer does not render tables correctly, use a GitHub/GitLab viewer or a desktop Markdown editor.
> This file uses strict pipe-table syntax.

| EffectId | Category | Abstract Function | Slay the Spire | Monster Train | Griftlands |
|----------|-----------|-------------------|----------------|----------------|--------------|
| DamageUpFlat | Offensive | +X damage per hit / action | Strength | Rage | Power |
| DamageUpMultiplier | Offensive | Multiplies outgoing damage | Strength scaling relics | Rage scaling variants | Power variants |
| DamageDownFlat | Offensive Control | -X damage from attacker | Weak (approx) | Sap | Impair |
| DamageDownMultiplier | Offensive Control | -% damage from attacker | Weak | Sap (indirect) | Impair |
| DamageTakenUpFlat | Burst | Target receives +X extra damage | Vulnerable (approx) | — | Wound |
| DamageTakenUpMultiplier | Burst | Target receives +% extra damage | Vulnerable | — | Vulnerability (Negotiation) |
| TempShieldTurn | Defense | Temporary shield, resets per turn | Block | — | Defense |
| TempShieldPersistent | Defense | Shield persists until depleted | Plated Armor | Armor | — |
| NegateNextHit | Defense | Negates next damage instance | Intangible (partial) | Damage Shield | Evasion |
| NegateNextNInstances | Defense | Negates N damage instances | Artifact (analog) | Damage Shield (stacks) | Evasion (stacks) |
| AntiShieldGain | Control | Reduces shield generation | Frail | — | Exposed |
| DamageReflection | Control | Reflects damage back | Thorns | Spikes | Counter |
| DamageOverTime | Pressure | Automatic periodic damage | Poison | Frostbite | Bleed / Heated |
| DisableActions | Tempo Control | Target cannot act | Entangle / stun-like | Dazed | Stun |
| DisableMovement | Control | Prevents movement / state changes | — | Rooted | — |
| InitiativeBoost | Tempo | Acts earlier | — | Quick | — |
| MultiHitModifier | Scaling | Grants additional hits | Multi-hit cards | Multistrike | — |
| PiercingDamage | Penetration | Ignores shields / mitigation | Penetrating attacks | Piercing | Piercing |
| DebuffImmunityStacks | Resistance | Blocks debuff application | Artifact | — | — |
| DebuffCleanse | Recovery | Removes debuffs | Cleanse effects | Purify | Cleanse cards |
| ArchetypeAmplifier | Meta | Boosts card archetypes | Form powers / relic synergies | Clan mechanics | Influence / Dominance |
| TempoAcceleration | Meta | Faster loops / turns | Energy relics / draw engines | Floor scaling engines | Momentum cards |
| ResourceGenerationModifier | Meta | Modifies resource generation | Energy / draw scaling | Ember / draw scaling | Action / draw engines |

---

## 4. Example Thematic Mapping (Display Names)

Display names are cosmetic and may change without affecting logic.

| EffectId | Example Display Name |
|-----------|----------------------|
| DamageUpFlat | Drive |
| DamageDownFlat | Offbeat |
| DamageTakenUpFlat | Exposed Groove |
| TempShieldTurn | Composure |
| TempShieldPersistent | Stage Presence |
| NegateNextHit | Perfect Timing |
| DamageOverTime | Feedback |
| DisableActions | Choke |
| DamageReflection | Backlash |
| InitiativeBoost | On The One |
| MultiHitModifier | Polyrhythm |
| DebuffCleanse | Reset |

---

## 5. Core Minimal Status Set (MVP – 5 Effects)

| Priority | EffectId | Gameplay Role |
|----------|-------------|----------------|
| 1 | DamageUpFlat | Offensive scaling |
| 2 | TempShieldTurn | Defensive mitigation |
| 3 | DamageOverTime | Long-term pressure |
| 4 | DamageTakenUpFlat | Burst windows |
| 5 | DisableActions | Tempo control |

---

## 6. Expanded Core Set (Recommended – 10 Effects)

| Tier | EffectId | Design Purpose |
|------|-------------|----------------|
| Core | DamageUpFlat | Scaling offense |
| Core | TempShieldTurn | Active defense |
| Core | DamageOverTime | Pressure / attrition |
| Core | DamageTakenUpFlat | Burst timing |
| Core | DisableActions | Tempo control |
| Depth | DamageDownFlat | Soft control |
| Depth | NegateNextHit | Precision defense |
| Depth | DebuffCleanse | Recovery valve |
| Depth | DamageReflection | Punish aggression |
| Depth | ResourceGenerationModifier | Engine identity |

---

End of document.

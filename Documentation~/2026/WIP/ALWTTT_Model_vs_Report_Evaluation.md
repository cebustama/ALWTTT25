# ALWTTT — Evaluación del modelo de datos vs requerimientos del reporte

Baseline evaluada: **CardPayload con** `effects: List<CardEffectSpec>` (polimórfico vía `SerializeReference`) **+ legacy** `statusActions`, y **StatusEffectSO con** `StatusKey` humano (para variantes) + catálogo indexado por key/primitive.

Fuente de requerimientos: `SOLID_WAUC_CardEffects_and_TriggeredStatuses_TechnicalReport_v2_HandwrittenCards.md`.

---

## Resumen ejecutivo

El modelo **tiene la base correcta** para crecer de forma SOLID/WAUC (lista extensible de efectos + referencia a `StatusEffectSO` variante por key), pero **hoy solo expresa**:

- ✅ `ApplyStatusEffect` (aplica un `StatusEffectSO` específico a un target con `stacksDelta` y `delay`)
- ✅ `DrawCards` (roba N)

Para cubrir el reporte completo faltan principalmente:

1) Nuevos `CardEffectSpec` (Damage/Discard/ShuffleCurse/Generate/SetSessionConfig/etc.)  
2) Flags y lifecycle de carta (exhaust, curse/unplayable, onDraw effects)  
3) Triggers dentro de `StatusEffectSO` (on loop, on draw, etc.) + posibles downsides  
4) Repetición declarativa (multi-hit por stacks)  
5) (Runtime) servicios/ejecutor/dispatchers que interpreten lo anterior

---

## Evaluación 1:1 por requerimiento

### A) Card Effects inmediatos (no-status)

| Requerimiento | ¿Soportado? | Qué falta (mínimo) |
|---|---:|---|
| Deal damage (“Deal X Vibe”) | ❌ | `DealDamageSpec : CardEffectSpec` con `targetType`, `amount` (+ opcional tipo/valor) |
| Draw N | ✅ | Ya existe `DrawCardsSpec { count }` |
| Discard N | ❌ | `DiscardCardsSpec` con `count` + modo (random/choose) |
| Shuffle card into draw pile (Feedback/DeadMic) | ❌ | `ShuffleIntoDrawPileSpec` con `CardRef` (id/key) + `copies` |
| Exhaust after play | ❌ | Mejor como flag en `CardDefinition` (p.ej. `exhaustOnUse`) o `ExhaustAfterPlaySpec` |
| Generate/Choose from pool | ❌ | `GenerateCardsSpec` + `ChooseFromPoolSpec` (pool rules + count + selection) |

### B) Triggered signature statuses (per loop, per song, etc.)

| Requerimiento | ¿Soportado? | Qué falta (mínimo) |
|---|---:|---|
| “Generate 2 Inspiration per loop” | ❌ | Extender `StatusEffectSO` con bloque de **trigger** (tipo trigger + acción a ejecutar) |
| “Damage per loop” + downside | ❌ | Trigger + `DealDamage` como acción + bloque optional de downside |
| On-draw penalties en status | ❌ | Trigger `OnDraw` + acción (p.ej. `GainResource(-1)`) |

> Nota: El modelo **sí** soporta “aplicar un StatusEffectSO variante” desde cartas, pero aún no describe **eventos/triggers** dentro del propio status.

### C) Curses / penalidades al robar (on-draw)

| Requerimiento | ¿Soportado? | Qué falta (mínimo) |
|---|---:|---|
| “Card unplayable” / “is curse” | ❌ | Flags en `CardDefinition` (p.ej. `isUnplayable`, `isCurse`) |
| “On Draw: lose 1 Inspiration” | ❌ | Campo `onDrawEffects: List<CardEffectSpec>` + hook runtime al robar |

### D) Session / Tempo constraints (Count-In)

| Requerimiento | ¿Soportado? | Qué falta (mínimo) |
|---|---:|---|
| Set tempo/time signature | ❌ | `SetSessionConfigSpec` + `SessionConfigDelta` |
| “IsCardAllowed” constraints | ❌ | Servicio `ISessionConfigService` + flags/config en datos para validación |

### E) Targeting (Self / Random / All / Audience / Enemies)

| Requerimiento | ¿Soportado? | Qué falta (si aplica) |
|---|---:|---|
| Targeting general | ⚠️ Parcial | Depende de qué cubre `ActionTargetType`. Si faltan patrones: ampliar enum o agregar `TargetFilter/TargetQuery` |

### F) Multi-hit / repeat por stacks (“Triplet Pulse”)

| Requerimiento | ¿Soportado? | Qué falta (mínimo) |
|---|---:|---|
| Repetir efecto N veces donde N = stacks de un status | ❌ | Mecanismo declarativo: `RepeatSpec` (envolvente) o campos `repeatMode + repeatStatusKey/id` en `DealDamageSpec` |

### G) “Two-lane model” (cardEffects + statusActions)

| Requerimiento | ¿Soportado? | Nota |
|---|---:|---|
| Separación explícita status vs otros efectos | ⚠️ Diferente | Tu modelo unifica todo en `effects`. Soporta el objetivo, pero requiere tooling/importer propio para `SerializeReference`. |

### H) Policy: “no negative stacks”

| Requerimiento | ¿Soportado? | Qué falta |
|---|---:|---|
| Evitar stacks negativos en statuses | ⚠️ No impuesto | Validación editor/importer (regla global o por status) para bloquear `stacksDelta < 0` salvo casos explícitos |

---

## Gap list (mínimo para cumplir el reporte)

1) **Specs nuevos (CardEffectSpec)**  
   - `DealDamageSpec`, `DiscardCardsSpec`, `ShuffleIntoDrawPileSpec`, `GainResourceSpec`, `GenerateCardsSpec`, `ChooseFromPoolSpec`, `SetSessionConfigSpec`

2) **Lifecycle/flags de carta**  
   - `isCurse`, `isUnplayable`, `exhaustOnUse`  
   - `onDrawEffects: List<CardEffectSpec>` (y luego quizás `onExhaustEffects`, etc.)

3) **TriggeredStatus en StatusEffectSO**  
   - `triggerType` (OnLoopEnd/OnDraw/...)  
   - `triggeredAction` (reusar mismos “kinds” que CardEffect, o DTO paralelo)  
   - `downside` opcional

4) **Repeat declarativo**  
   - envolvente `RepeatSpec` o repeat fields en ciertos specs.

5) **Runtime (fuera del modelo, pero requerido)**  
   - `CardEffectRunner` + ejecutores por tipo  
   - servicios: targeting, session config, onDraw dispatcher, etc.

---

## Qué ya está bien encaminado

- `effects` polimórfico permite crecimiento **sin mega-enums** y sin “god struct”.
- `ApplyStatusEffectSpec` referenciando `StatusEffectSO` habilita **variantes** (Strength vs SuperStrength) y composiciones (Strength + Weak) sin crear primitivas nuevas.
- El catálogo por `StatusKey` es el puente ideal para JSON/import/editor tooling.

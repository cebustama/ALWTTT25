# ALWTTT — Plan SOLID‑WAUC: Card Effects unificados + StatusEffectSO por variante

Este documento guarda el plan paso a paso (incremental + testeable) para migrar ALWTTT a un modelo de **Card Effects** unificado, donde los efectos de estado referencian un **StatusEffectSO específico** (variante), permitiendo casos como **Strength vs SuperStrength** o combinaciones **Strength + Weak** sin crear nuevas primitivas mezcladas.

---

## Objetivo

1. **Todas las cartas** tienen una lista extensible de efectos: `effects : List<CardEffectSpec>`.
2. `ApplyStatusEffect` referencia un **StatusEffectSO** (variante), no solo un `CharacterStatusId`.
3. Mantener **compatibilidad**: conservar `statusActions` como legacy por un tiempo y migrar gradualmente.
4. Soportar efectos no‑status (Draw/Discard/Generate/Shuffle Curse/etc.) como efectos de la misma lista.

---

## Decisión clave: “primitiva” vs “variante”

- `CharacterStatusId` / `EffectId` es el **primitive CSO** (qué hace en abstracto).
- **Variante** = un `StatusEffectSO` concreto con tuning/reglas específicas (stack mode, decay, caps, etc.).
- Para permitir múltiples variantes del mismo primitive, necesitamos una **identidad única por variante** (no por primitive).

---

# Fase 0 — Contrato de identidad para StatusEffectSO (habilitar variantes)

## 0.1 Modificar `StatusEffectSO.cs`

**Archivo:** `StatusEffectSO.cs`

Agregar un identificador estable propio, independiente del primitive CSO:

- `statusKey : string` (**único y estable**) → identidad de la variante.
- Mantener `effectId` como primitive CSO.

**Cambios sugeridos**
- `[SerializeField] private string statusKey;`
- `public string StatusKey => statusKey;`
- `OnValidate`:
  - si está vacío, setear un default (idealmente manual, pero puedes usar `name` como fallback).

✅ **Test rápido**
- Crea 2 `StatusEffectSO` con mismo `EffectId` pero distinto `statusKey`.
- Asegúrate que el catálogo deje de fallar por duplicado de `EffectId` (ver 0.2).

---

## 0.2 Modificar `StatusEffectCatalogueSO.cs`

**Archivo:** `StatusEffectCatalogueSO.cs`

Hoy asume unicidad por `CharacterStatusId`. Cambiar a:

- `_byKey: Dictionary<string, StatusEffectSO>` (**único**)
- opcional: `_byPrimitive: Dictionary<CharacterStatusId, List<StatusEffectSO>>` (para UX/filtrado)

**Métodos nuevos recomendados**
- `bool TryGetByKey(string key, out StatusEffectSO effect)`
- `IReadOnlyList<StatusEffectSO> GetAllByPrimitive(CharacterStatusId id)` (opcional)

**Validación**
- `OnValidate`: detectar **duplicados de `statusKey`**, no de `EffectId`.

✅ **Test rápido**
- Catálogo debe aceptar 2 variantes con el mismo `EffectId` si sus `statusKey` son distintos.
- Catálogo debe fallar si hay 2 efectos con el mismo `statusKey`.

---

# Fase 1 — Nuevo modelo “Card Effects” (sin tocar runtime todavía)

## 1.1 Crear nuevos archivos (nuevo folder sugerido)

Ruta sugerida: `ALWTTT/Cards/Effects/`

- `CardEffectSpec.cs` (base abstract)
- `ApplyStatusEffectSpec.cs`
- `DrawCardsSpec.cs` (MVP para un efecto no‑status)

Ejemplo conceptual:

- `ApplyStatusEffectSpec`:
  - `StatusEffectSO status;`
  - `ActionTargetType target;`
  - `int stacksDelta;`
  - `float delay;`

- `DrawCardsSpec`:
  - `int count;`

---

## 1.2 Modificar `CardPayload.cs`

**Archivo:** `CardPayload.cs`

Agregar:

- `[SerializeReference] private List<CardEffectSpec> effects = new();`
- `public IReadOnlyList<CardEffectSpec> Effects => effects;`

Mantener `statusActions` como legacy (deprecate luego):

- `[SerializeField] private List<StatusEffectActionData> statusActions = new();`

✅ **Test rápido**
- Abre un `ActionCardPayload` existente:
  - Debe seguir teniendo `statusActions`.
  - Debe aparecer el nuevo campo `effects` (aunque esté vacío).

---

# Fase 2 — Editor: authoring de Effects + migración desde StatusActions

## 2.1 Modificar `CardEditorWindow.cs`

**Archivo:** `CardEditorWindow.cs`

En `DrawActionPayloadEditor` y `DrawCompositionPayloadEditor`:

1. Obtener `effectsProp = so.FindProperty("effects")`
2. Dibujar:
   - `DrawEffectsBlock(effectsProp, _registries?.StatusCatalogue)`
3. Debajo, un foldout:
   - “Legacy StatusActions (deprecated)”
   - dentro: `DrawStatusActionsBlock(...)`

---

## 2.2 Implementar `DrawEffectsBlock(...)`

UI mínima recomendada:

- Botón `+ Add Effect` → `GenericMenu`:
  - ApplyStatusEffect
  - DrawCards
- Lista que recorre `effectsProp` y dibuja cada elemento según su tipo (`managedReferenceFullTypename`)
- Botón por fila: Remove

**Drawer recomendado para ApplyStatusEffectSpec**
- Selector por `StatusEffectCatalogueSO` mostrando `DisplayName` (y opcionalmente el primitive `EffectId`).
- Setear `status` (object reference) del spec.

✅ **Test rápido**
- Añadir un `ApplyStatusEffectSpec` desde el menú.
- Seleccionar un `StatusEffectSO` por nombre.
- Cambiar target / stacks / delay y confirmar que persiste.

---

## 2.3 Botón de migración “Legacy → Effects”

En el bloque Effects, agregar botón:

- `Migrate legacy StatusActions → Effects`

Estrategia:
- Si `effects` está vacío y `statusActions` tiene datos:
  - por cada `StatusEffectActionData` (effectId, target, stacks, delay)
  - resolver una variante default en catálogo (por primitive `EffectId`) **o** usar una convención:
    - primera variante de `_byPrimitive[effectId]`
    - o variante cuyo `statusKey == enumNameLower(effectId)` si quieres
  - crear `ApplyStatusEffectSpec` equivalente

✅ **Test rápido**
- En una carta con `statusActions`, migrar y verificar que aparecen effects equivalentes.

---

# Fase 3 — JSON: schema v2 + importer que llena `effects`

Tu JSON/importer actual soporta `statusActions`. Ahora quieres un schema v2 que soporte **effects**.

## 3.1 JSON schema v2 (propuesto)

Ejemplo mínimo:

```json
{
  "id": "conito_flow_boost",
  "name": "Flow Boost",
  "kind": "Action",
  "action": { "actionTiming": "BetweenSongsOnly" },

  "effects": [
    {
      "type": "ApplyStatusEffect",
      "statusKey": "flow",
      "targetType": "Self",
      "stacksDelta": 2,
      "delay": 0
    },
    {
      "type": "DrawCards",
      "count": 1
    }
  ],

  "statusActions": [
    { "effectId": 0, "targetType": "Self", "stacksDelta": 2, "delay": 0 }
  ]
}
```

**Reglas**
- Si `effects` existe y tiene elementos → usar eso.
- Si `effects` está vacío pero `statusActions` existe → convertir legacy a effects (compat).
- `statusKey` se resuelve mediante `StatusEffectCatalogueSO.TryGetByKey`.

---

## 3.2 Modificar `CardEditorWindow.JsonImport.cs`

**Archivo:** `CardEditorWindow.JsonImport.cs`

Agregar DTO:

- `EffectJson[] effects;`
- `EffectJson` contiene:
  - `string type;`
  - union de campos posibles (statusKey/targetType/stacksDelta/delay, count, etc.)

Agregar método:

- `ApplyEffectsJson(SerializedObject pso, EffectJson[] effects, out string error)`

**Implementación clave**
- `effectsProp = pso.FindProperty("effects");`
- Para cada elemento:
  - setear `managedReferenceValue = new ApplyStatusEffectSpec { ... }` o `new DrawCardsSpec { ... }`
- Resolver `statusKey` a `StatusEffectSO` usando catálogo.
- Si falla: error claro (no stage).

✅ **Test rápido**
- Pegar JSON v2 en “Create from JSON”:
  - Debe poblar `effects`.
  - Debe poder guardar assets y persistir `effects`.

---

# Fase 4 — Runtime: ejecutar Effects (sin eliminar legacy aún)

## 4.1 Crear `CardEffectRunner`

Responsabilidad:
- Ejecutar `payload.Effects` si no está vacío.
- Si está vacío, ejecutar legacy `statusActions` (transición).

## 4.2 Ejecutores por tipo (OCP)

- `ApplyStatusEffectSpec`:
  - aplicar stacks según `StatusEffectSO` variante seleccionada
- `DrawCardsSpec`:
  - robar cartas (puede ser stub/log al principio)

✅ **Test rápido**
- En playmode, al jugar una carta, loggear los efectos ejecutados y sus parámetros.

---

# Fase 5 — Migración masiva + limpieza

## 5.1 Tool de migración batch
Menú editor:

- `Tools/ALWTTT/Migrate Cards StatusActions -> Effects`

Proceso:
- recorrer assets `CardPayload`
- si `effects` vacío y `statusActions` no vacío:
  - migrar
  - opcional: limpiar `statusActions`

## 5.2 Actualizar docs
- `StatusEffects.md`: catálogo ya no enforza unicidad de `EffectId`, sino de `statusKey`.
- `Card.md`: `CardPayload` ahora tiene `effects` como representación unificada; `statusActions` queda legacy/deprecated.

---

# Notas de diseño (pragmáticas)

- **SerializeReference**: excelente para OCP, pero requiere cuidado en tooling/editor.
- Para JSON: tu pipeline es editor-only, así que puedes resolver `statusKey` a `StatusEffectSO` y guardar la referencia.
- Combinaciones como **Strength + Weak** son simplemente **2 ApplyStatusEffectSpec** en la lista de effects.
- Si en runtime quieres “agrupar” por primitive (ej. ambos son `DamageUpFlat`), puedes hacerlo a nivel del **Status Container** del personaje, no a nivel de la carta.

---

# Checklist de “mínimo viable” (para empezar a jugar con esto)

1) `statusKey` en `StatusEffectSO` + catálogo por `statusKey`  
2) `effects` en `CardPayload` con 2 tipos: ApplyStatusEffect + DrawCards  
3) `CardEditorWindow`: Add Effect + selección por catálogo + migrate legacy  
4) `JsonImport`: schema v2 con `effects` + fallback legacy  
5) `Runtime`: runner que ejecuta effects (aunque sea con logs al inicio)

---

## Referencias rápidas (archivos típicos a tocar)

- `StatusEffectSO.cs`
- `StatusEffectCatalogueSO.cs`
- `CardPayload.cs`
- `CardEditorWindow.cs`
- `CardEditorWindow.JsonImport.cs`
- (runtime) `CardEffectRunner.cs` + ejecutores por tipo
- docs: `StatusEffects.md`, `Card.md`

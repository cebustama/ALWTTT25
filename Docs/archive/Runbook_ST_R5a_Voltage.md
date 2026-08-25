# Runbook — ST-R5a — Voltage (primitiva CSO + SO)

**Clasificación:** `runbook` — no es autoridad, no es SSoT, no es roadmap.
**Batch:** R5-a · **Placement:** `Docs/planning/active/Runbook_ST_R5a_Voltage.md`
**Ámbito:** verificar que Voltage existe, se aplica, no decae, clampa, se muestra y
se limpia. **No** verifica generación automática (R5-b) ni consumo (R5-c).
**Estado esperado del sistema:** baseline S5i congelado, Voltage **sin consumidor**.

---

## 0. Preparación (una sola vez)

### 0.1 Build y defines

| # | Acción | Verificación |
|---|---|---|
| 0.1.1 | Confirmar que el define de scripting **`ALWTTT_DEV`** está activo (Project Settings → Player → Scripting Define Symbols) | La consola imprime al arrancar: `DevModeController initialized. Press F12 to toggle overlay.` |
| 0.1.2 | Compilación limpia | Cero errores. Un error en `CharacterStatusPrimitiveDatabaseSO` con `ResourceCounter` no definido significa que el enum no se guardó. |

> Sin `ALWTTT_DEV` no hay Dev Mode y **ninguno de estos tests es ejecutable** tal
> como están escritos: el spawn de cartas y el stepper de Flow viven ahí.

### 0.2 Verificación estática previa (antes de entrar a Play)

Estos cuatro chequeos son de inspector, no de juego. Si alguno falla, **parar**:
los smokes posteriores medirían un artefacto mal autorado, no el runtime.

| # | Dónde | Qué comprobar | Fallo |
|---|---|---|---|
| 0.2.1 | Asset del registro CSO → menú contextual **Populate From CSO Canonical** | La consola imprime `[CSO] Populated 27 primitive entries in <nombre>.` | Cualquier `[CSO] No canonical data mapping found for <X>` ⇒ hay un miembro del enum sin `case`. **Anotar cuál** y parar. Un conteo ≠ 27 ⇒ enum y `switch` desalineados. |
| 0.2.2 | Inspector de `StatusEffect_Voltage` | `statusKey = voltage` · `effectId = ResourceCounter (993)` · `isDefaultVariant = true` · `stackMode = Additive` · `maxStacks = 9` · `decayMode = None` · `durationTurns = 0` · `tickTiming = None` · `valueType = Flat` · `isBuff = true` | Cualquier desviación. Ojo especialmente a `isDefaultVariant` y `iconSprite`: el wizard **no** los escribe. |
| 0.2.3 | Inspector de `StatusEffect_Voltage` | `iconSprite` asignado · `displayName = Voltage` · `description` no vacía | `iconSprite` nulo ⇒ ST-R5a-1 y ST-R5a-4 son inejecutables (el estado se aplica invisible). |
| 0.2.4 | `StatusEffectCatalogue_Musicians` | El SO aparece en la lista `_effects` una sola vez | Duplicado ⇒ `RebuildCache` se queda con el primero y el segundo es un fantasma. |

### 0.3 Cartas de test

Las tres cartas dev (`DEV_Voltage_Plus1`, `DEV_Voltage_Plus4`,
`DEV_Voltage_Minus2`) deben estar:

1. Importadas y guardadas como assets.
2. Registradas en **`Conito_CardCatalogData`** con `flags = UnlockedByDefault`
   (sin `StarterDeck`, sin `RewardPool`).

**Por qué el catálogo y no el mazo:** el spawner de Dev Mode lista lo que devuelve
`PersistentGameplayData.BuildBandCardCatalog`, que recorre el `CardCatalog` de cada
músico de la banda. Una carta en el mazo pero fuera del catálogo **no aparece en la
pestaña Catalogue**. Una carta en el catálogo pero fuera del mazo sí aparece, y no
contamina ninguna run.

*Verificación:* entrar a Play, F12 → pestaña **Catalogue**, filtrar por `DEV_`. Deben
salir las tres con botón **Spawn** habilitado.

### 0.4 Arranque de sesión de test

| # | Acción |
|---|---|
| 0.4.1 | Lanzar un gig con **Conito en la banda** (obligatorio: las cartas son `FixedMusicianType = Conito`). |
| 0.4.2 | F12 → pestaña **Infinite** → activar **Infinite Turns**. Necesario para ST-R5a-2 y ST-R5a-6R: sin él el gig termina y no hay boundary de canción observable. |
| 0.4.3 | Localizar el HUD de Conito y su zona de iconos de estado. Es donde se lee el contador en todos los tests. |

> **Nota sobre Infinite Turns.** En el cierre de canción, la rama dev llama
> `DeckManager.DevForceHandResetToDiscard()`: **las cartas de la mano se destruyen**.
> Los contenedores de estado NO se tocan. Consecuencia práctica: tras cada boundary
> de canción hay que volver a hacer Spawn. Eso no invalida ningún test — de hecho es
> lo que hace limpio a ST-R5a-6R.

### 0.5 Convenciones de esta suite

- **"Contador"** = el número que `StatusIconBase.SetStatusValue` pinta en el icono.
- Un test **falla** si el resultado difiere de lo esperado *por la razón indicada*.
  Los "no-fallos conocidos" están listados al final de cada test.
- Registrar PASS/FAIL en la tabla de §7 conforme se ejecutan, no al final.

---

## 1. ST-R5a-1 — Aplicación de N stacks

**Qué prueba:** que `ApplyStatusEffectSpec` → `StatusEffectContainer.Apply` →
`CharacterCanvas.HandleStatusApplied` funciona de extremo a extremo, y que el icono
nace en la primera aplicación.

**Precondición:** §0 completo. Conito **sin** Voltage (sin icono de Voltage en su HUD).

**Pasos**

1. F12 → **Catalogue** → `DEV_Voltage_Plus1` → **Spawn**. Repetir hasta tener 3 copias
   en mano (o hasta el tope de mano; ver no-fallos).
2. Cerrar el overlay (F12).
3. Jugar la primera copia.
4. **Observar el HUD de Conito antes de seguir.**
5. Jugar la segunda copia. Si el juego la rechaza, ver no-fallos punto (b): abrir turno
   nuevo y continuar.
6. Jugar la tercera copia.

**Resultado esperado**

- Tras el paso 3: aparece un icono nuevo en el HUD de Conito, con animación de aparición
  (`PlayAppear`), mostrando **1**.
- Tras el paso 5: **2**.
- Tras el paso 6: **3**.

**Criterio de fallo**

- No aparece icono alguno **y** la consola muestra
  `[CharacterCanvas] StatusEffectSO 'StatusEffect_Voltage' (key='voltage') has no IconSprite assigned` ⇒ falta el sprite (regresión de 0.2.3, no del runtime).
- No aparece icono **sin** ese warning ⇒ el HUD no está bindeado al contenedor, o la carta
  no resolvió el SO.
- El contador no incrementa entre jugadas ⇒ `stacksDelta` mal serializado o el estado se
  está aplicando a otro portador.
- El icono aparece en el HUD de **otro** músico ⇒ `targetType` no es `Self`, o el performer
  resuelto no es Conito.

**No-fallos conocidos**

- (a) El spawn se bloquea con `Spawn gated: <razón>` si la mano está llena
  (`CanDevSpawnToHand`). No es un fallo del test: descartar/jugar y reintentar.
- (b) La segunda o tercera jugada puede ser **denegada por el presupuesto ECON-1** aunque
  el coste sea 0. El presupuesto y la inspiración son gates distintos. Cerrar turno, abrir
  el siguiente y continuar acumulando. Llegar a 3 repartido entre turnos es un PASS válido
  — y es además evidencia adelantada de ST-R5a-2.
- (c) La carta sale sin arte (no se asignó `cardSpritePath`). Cosmético.

---

## 2. ST-R5a-2 — Ausencia de decay en todos los boundaries

**Qué prueba:** que Voltage no pierde stacks en ningún tick. **Es el test más importante
de la suite** y el que el criterio de cierre exige "empírico, no asumido".

> **Por qué no basta con leer el SO.** `StatusEffectContainer.Tick` filtra así:
> `if (def.Tick != TickTiming.None && def.Tick != timing) continue;`
> Un estado con `Tick == None` **no se salta**: entra al `switch (def.Decay)` en *todos*
> los timings. Lo único que protege a Voltage es `DecayMode.None`, que hace que ese switch
> no haga nada. Si este test falla, mirar `decayMode`, **no** `tickTiming`.

**Precondición:** Conito con exactamente **3** stacks de Voltage (heredado de ST-R5a-1).
Infinite Turns ON.

**Pasos y lecturas — anotar el contador en cada fila**

| # | Boundary a cruzar | Cómo se cruza | Contador esperado |
|---|---|---|---|
| 2.1 | *(basal)* | — | **3** |
| 2.2 | Fin de turno de jugador | Pulsar fin de turno | 3 |
| 2.3 | Turno de audiencia completo | Dejar resolver a la audiencia | 3 |
| 2.4 | Apertura de turno de jugador | Esperar al nuevo PlayerTurn | 3 |
| 2.5 | Cierre de un loop | Poner la canción a sonar y dejar cerrar **un** loop entero | 3 |
| 2.6 | Cierre de una parte | Dejar completar la parte | 3 |
| 2.7 | Segundo turno de audiencia | Repetir 2.2–2.4 una vez más | 3 |

**Resultado esperado:** **3** en las siete lecturas. El icono nunca desaparece.

**Criterio de fallo**

- Cualquier decremento en cualquier fila. Anotar **en qué boundary exacto** ocurrió: eso
  identifica el `TickTiming` que está mordiendo.
- El icono desaparece ⇒ los stacks llegaron a 0; `Apply`/`Tick` llamó `Clear`.

**Chequeo cruzado obligatorio (control positivo)**

Un "no decae" es fácil de aprobar por accidente si el tick no está corriendo en absoluto.
Para descartarlo:

1. F12 → **Stats** → stepper **Flow (all)** → pulsar `+` una vez. Flow (`DamageUpFlat`) es
   song-scoped, no turn-scoped, así que sobrevive el turno.
2. Aplicar **Composure** a Conito con una carta que lo dé (`Keep Cool`, C2), o comprobar
   cualquier estado con decay activo presente en la escena.
3. Cerrar y abrir turno de jugador.
4. **Composure debe bajar/limpiarse** (se limpia explícitamente en la apertura de PlayerTurn)
   mientras **Voltage sigue en 3**.

Si Composure tampoco se mueve, el ciclo de turno no está corriendo y ST-R5a-2 **no ha
probado nada**: reejecutar.

**No-fallos conocidos**

- El icono puede reordenarse en el HUD cuando aparecen o desaparecen otros estados. La
  posición no es parte del test; el número sí.

---

## 3. ST-R5a-3 — Clamp a MaxStacks = 9

**Qué prueba:** que el techo autorado se respeta en la ruta `Additive`.

> **Por qué este test existe y qué corrige.** R5-inv encontró que la distinción entre
> `StackMode.Additive` y `StackMode.AdditiveClamped` **no existe en runtime**:
> `ApplyStackingPolicy` termina con un clamp incondicional
> (`if (inst.Stacks > max) inst.SetStacks(max);`) que se aplica a todos los modos.
> Voltage está autorado `Additive`. Este test convierte esa corrección documental
> (ledger D5) en evidencia sobre contenido real.

**Precondición:** Conito con exactamente **7** stacks de Voltage.

*Cómo llegar a 7 desde 3:* Spawn `DEV_Voltage_Plus4` × 1 y jugarla (3 → 7). Si el
presupuesto lo impide, repartir entre turnos con `DEV_Voltage_Plus1`.

**Pasos**

1. Confirmar contador = **7**.
2. Spawn `DEV_Voltage_Plus4` → jugarla.
3. Leer el contador.

**Resultado esperado:** **9**. No 11.

**Criterio de fallo**

- **11** ⇒ el clamp no se aplicó. Revisar `maxStacks` en el SO (¿quedó en el default 999?).
- El icono desaparece ⇒ `Apply` interpretó el resultado como ≤ 0.
- **9 pero sin haber pasado por 7** ⇒ el estado de partida era otro; reejecutar desde
  precondición limpia.

**Verificación adicional (barato, hazlo)**

4. Jugar `DEV_Voltage_Plus1` una vez más estando en 9.
5. Esperado: sigue en **9**, sin error en consola y sin parpadeo del icono.

---

## 4. ST-R5a-4 — El icono se limpia al llegar a 0

**Qué prueba:** la ruta `Apply(stacks ≤ 0)` → `Clear` → `OnStatusCleared` →
`CharacterCanvas.HandleStatusCleared` → `PlayDisappear`.

**Precondición:** Conito con exactamente **2** stacks de Voltage.

*Cómo llegar a 2 desde 9:* jugar `DEV_Voltage_Minus2` cuatro veces (9→7→5→3→1) no da 2.
Ruta limpia: reiniciar el gig y aplicar `DEV_Voltage_Plus1` × 2. Alternativa aceptable:
llegar a **2** por cualquier combinación y anotar la ruta usada.

**Pasos**

1. Confirmar contador = **2**.
2. Spawn `DEV_Voltage_Minus2` → jugarla.
3. Observar el HUD.

**Resultado esperado**

- El icono reproduce su animación de desaparición y **se destruye**.
- El HUD de Conito ya no muestra Voltage.
- Ningún error en consola.

**Criterio de fallo**

- El icono queda visible mostrando **0** ⇒ `Clear` no se llamó, o el canvas escuchó
  `OnStatusChanged` en vez de `OnStatusCleared`.
- El icono queda visible **sin número** ⇒ se limpió el valor pero no la instancia.
- `NullReferenceException` al desaparecer ⇒ regresión de la ruta de destrucción de iconos
  (M1.8, desenganche antes de animar).

**Verificación de re-aplicación (parte del mismo test)**

4. Sin esperar a que termine la animación, jugar `DEV_Voltage_Plus1`.
5. Esperado: aparece un icono **nuevo** mostrando **1**. El icono viejo, ya desenganchado
   del diccionario, se autodestruye por su cuenta sin colisionar.
6. Fallo: dos iconos de Voltage simultáneos que persistan tras terminar ambas animaciones.

---

## 5. ST-R5a-5 — El wizard escribe `statusKey` sin auto-find tramposo

**Qué prueba:** la primera validación real del fix D-R5-3 / F-R4-3. **Es un test de
editor, no de juego** — no requiere estar en Play.

> **Qué se rompió antes.** El wizard creaba el asset sin escribir `statusKey`. El estado
> quedaba indexado por primitiva pero **invisible para `TryGetByKey`**, así que cualquier
> carta o consumidor que lo buscara por key fallaba. El rider de D-R5-3 añadió además
> que el wizard no puede autoseleccionar catálogo: elegir "el primer `StatusEffectCatalogueSO`
> que encuentre `AssetDatabase`" es una decisión de autoría tomada en silencio por una herramienta.

**Pasos**

1. Abrir `StatusEffectWizardWindow` **en frío** (cerrarla antes si estaba abierta, para que
   `TryAutoFindAssets` corra de nuevo).
2. **Antes de tocar nada:** mirar el campo **Catalogue**.
3. Seleccionar `Effect Id = ResourceCounter`.
4. Escribir `Display Name = Voltage`.
5. **Antes de crear:** mirar el campo **Status Key**.
6. Seleccionar el catálogo `StatusEffectCatalogue_Musicians`.
7. *(Opcional, no destructivo)* Cambiar el Display Name a `Voltage Test` y volver a `Voltage`,
   para ver si la sugerencia sigue al nombre.
8. **No crear un segundo asset.** Ver "cómo cerrar este test" abajo.

**Resultado esperado**

| Paso | Esperado |
|---|---|
| 2 | El campo **Catalogue está vacío**. No hay catálogo preseleccionado. |
| 5 | El campo **Status Key muestra `voltage`** — minúsculas, sin espacios, autosugerido desde el Display Name. |
| 7 | La sugerencia se actualiza mientras el usuario no haya editado la key a mano; una vez editada, deja de auto-sobrescribirse. |

Y sobre el asset **ya creado** (verificación complementaria en el inspector, modo Debug o
en el `.asset` en texto):

- `statusKey: voltage`
- El log de creación quedó en consola con la forma
  `[StatusEffectWizard] Created 'StatusEffect_Voltage' at '<ruta>' (StatusKey='voltage') and registered in 'StatusEffectCatalogue_Musicians'.`

**Criterio de fallo**

- El campo Catalogue aparece **relleno** al abrir en frío ⇒ regresión del rider de D-R5-3.
- Status Key vacío en el paso 5 ⇒ la autosugerencia no corre.
- El asset guardado tiene `statusKey` vacío ⇒ regresión de F-R4-3. **Bloqueante para todo
  el resto de R5**: sin key no hay guarda dual posible en R5-c.
- La key guardada es `Voltage` (con mayúscula) o `voltage_` ⇒ la normalización no se aplicó
  en la escritura.

**Cómo cerrar este test sin ensuciar el proyecto**

Los pasos 1–7 son de solo lectura sobre la UI. **No pulsar Create.** Si por prueba se crea
un segundo asset, se llamará `StatusEffect_ResourceCounter 1` (el wizard nombra por
primitiva y `GenerateUniqueAssetPath` desambigua) y el wizard lo habrá **añadido al
catálogo**: borrar el asset **y** quitar la entrada del catálogo, y volver a
`RebuildCache`. Dejarlo suelto rompería ST-R5a-1..4 en la siguiente ejecución, porque
`_byKey` se quedaría con el primero visto.

---

## 6. ST-R5a-6R — Regresión: la allowlist de canción sigue intacta

**Qué prueba:** dos cosas a la vez. (1) Que Voltage **sobrevive** el boundary de canción —
la evidencia empírica de **D-R5-8 = A**. (2) Que los dos estados que sí eran song-scoped
**siguen limpiándose** — es decir, que resolver D-R5-8 no se hizo tocando el reset.

> **Qué se está regresando.** `GigManager.ResetSongScopedStatuses` no es un barrido por
> categoría: es una allowlist de dos primitivas (`DamageUpFlat` = Flow,
> `TempShieldTurn` = Composure). El nombre promete una política; el cuerpo entrega una
> lista. Un estado nuevo **no hereda** el reset. El borrador de D4 asumió lo contrario;
> este test es lo que convierte la corrección en hecho verificado.
>
> Segundo detalle de secuencia: el reset se dispara desde `StartCompositionSession`, es
> decir en el **arranque** de la canción siguiente, no al cerrar la anterior.

**Precondición**

- Infinite Turns **ON**.
- Conito con **≥ 3** stacks de Voltage. Anotar el valor exacto: `V₀`.
- Flow gig-wide con **≥ 1** stack: F12 → **Stats** → stepper `Flow (all)` → `+`.
  Anotar el valor de `Flow (all)`: `F₀`.

**Pasos**

1. Anotar `V₀` (contador de Voltage en Conito) y `F₀` (`Flow (all)` en la pestaña Stats).
2. Tocar la canción hasta el final y dejar que cierre.
3. En consola, esperar la línea
   `[DevMode] OnCompositionSessionEnded reached. InfiniteTurnsEnabled=True, ...`
   Confirma que el ciclo dev está activo y que la mano se resetea.
4. Dejar que arranque el siguiente ciclo hasta que se cree la nueva sesión de composición
   (log `[Gig] Starting new live composition session for next song.`).
5. Leer de nuevo el contador de Voltage y `Flow (all)`.

**Resultado esperado**

| Métrica | Antes | Después | Interpretación |
|---|---|---|---|
| Voltage en Conito | `V₀` | **`V₀`** (sin cambio) | Voltage es de alcance **gig** |
| `Flow (all)` | `F₀` | **0** | Flow sigue siendo song-scoped |
| Composure | — | 0 | Ya se limpiaba también en apertura de turno |

**Criterio de fallo**

- **Voltage se pierde** ⇒ o bien alguien añadió `ResourceCounter` a la allowlist (violación
  de D-R5-8=A), o hay una segunda ruta de limpieza no identificada. **Bloqueante**: invalida
  la resolución de D-R5-8 y la redacción de D4.
- **Flow sobrevive** ⇒ regresión en `ResetSongScopedStatuses` que no tiene nada que ver con
  Voltage pero rompe el meter de Flow. **Bloqueante e independiente de R5.**
- Ambos sobreviven ⇒ `ResetSongScopedStatuses` no se está llamando en absoluto; verificar
  que se alcanzó `StartCompositionSession` (log del paso 4).

**No-fallos conocidos**

- Las cartas dev desaparecen de la mano en el boundary (`DevForceHandResetToDiscard`).
  Esperado. Volver a hacer Spawn si se necesitan más pasos.
- El icono de Voltage puede reanimarse visualmente al reconstruirse el HUD entre canciones.
  Mientras el número sea `V₀`, es PASS.

---

## 7. Hoja de registro

| Test | Qué verifica | Resultado | Valor observado | Notas |
|---|---|---|---|---|
| 0.2.1 | Registro CSO repoblado (27) | ☐ PASS ☐ FAIL | | |
| 0.2.2 | Config del SO | ☐ PASS ☐ FAIL | | |
| 0.2.3 | Icono + display + descripción | ☐ PASS ☐ FAIL | | |
| 0.2.4 | Registro único en catálogo | ☐ PASS ☐ FAIL | | |
| ST-R5a-1 | Aplicar N stacks | ☐ PASS ☐ FAIL | contador final: | |
| ST-R5a-2 | Sin decay en 7 boundaries | ☐ PASS ☐ FAIL | boundary del fallo: | control positivo: ☐ |
| ST-R5a-3 | Clamp a 9 | ☐ PASS ☐ FAIL | 7 + 4 = | |
| ST-R5a-4 | Icono limpia a 0 | ☐ PASS ☐ FAIL | | re-aplicación: ☐ |
| ST-R5a-5 | Wizard escribe statusKey | ☐ PASS ☐ FAIL | key leída: | catálogo vacío al abrir: ☐ |
| ST-R5a-6R | Allowlist de canción | ☐ PASS ☐ FAIL | V₀ = __ → __ · F₀ = __ → __ | |

**Criterio de cierre de R5-a:** ST-R5a-1..5 PASS **y** ST-R5a-6R PASS **y** los cuatro
chequeos de §0.2 PASS. Un FAIL en ST-R5a-2 o ST-R5a-6R bloquea el cierre; los demás se
evalúan caso a caso.

---

## 8. Qué NO prueba esta suite (deuda explícita)

| Área | Por qué queda fuera | Dónde se cubre |
|---|---|---|
| Generación automática por jugada | El hook no existe todavía | R5-b · ST-R5b-* |
| Umbral y consumo de Voltage | No hay consumidor | R5-c · Overload |
| Colisión de dos variantes de `ResourceCounter` sobre el mismo portador | No hay segunda variante autorada; el contenedor está keyed por primitiva y la colisión es real pero inalcanzable con el contenido actual | Diferido; anotar si R8 añade otra variante |
| Persistencia de Voltage entre gigs | El fin de gig llama `ClearAllStatus` en la rama de recompensa; fuera del alcance de R5-a | Diferido |
| Comportamiento con `stacksDelta = 0` | `CardBase` hace `continue` antes de aplicar; ruta muerta por construcción | No requiere test |

---

## 9. Si algo falla — orden de diagnóstico

1. **¿Está el SO bien autorado?** Volver a §0.2. La mayoría de fallos de esta suite son de
   autoría, no de runtime.
2. **¿Resuelve la key?** Consola: si una carta dev imprime
   `No StatusEffectSO found for statusKey 'voltage'`, el problema es el catálogo o el
   `statusKey`, no el contenedor.
3. **¿Está el HUD bindeado?** Si el estado se aplica (se ve en el contenedor por debug) pero
   no hay icono, mirar `statusIconBasePrefab` en el `CharacterCanvas` de Conito y el warning
   de `IconSprite`.
4. **¿Está corriendo el ciclo de turnos?** El control positivo de §2 lo responde. Un
   "no decae" con el ciclo parado es un falso PASS.
5. **¿Es Conito el performer?** Si el icono aparece en otro músico, `targetType` o
   `performerRule` de la carta están mal.

Registrar cualquier hallazgo nuevo como `F-R5a-N` y llevarlo a
`PENDING_DOC_DIFFS_R5.md`, no a los SSoT: en R5 la documentación **acumula**, no se aplica.

# changelog-ssot — ALWTTT

This changelog records **semantic / documentary changes** (meaning, contracts, authority,
promotions/demotions, splits/merges/supersessions, implemented-semantics changes that required
doc updates). Cosmetic / grammar / formatting-only edits are not logged here.

> **Rotated 2026-07-01 (D-DOC-ROTATE=C).** The full project history from **2026-03-18 →
> 2026-06-22** (Governance migration Batch 01 through S5a) was archived **verbatim** to
> `archive/changelog-ssot_2026-03-18_to_2026-06-22.md`. This active file was restarted with the
> milestone index below + go-forward entries. Nothing was summarised destructively — the archive
> is the complete record; this file is the scannable current-window log. Newest entries at top.
> Go-forward header convention: `## YYYY-MM-DD — TITLE` (the archived newest region used a plain
> `YYYY-MM-DD — TITLE` line without `##`; do not replicate that drift here).

---

## 2026-08-26 — DOC-APPLY-3: tres paquetes de doc diffs aplicados en una pasada (CSV-4c + R5-d + HUD-COMP-1)

Clasificación: *operational* + *structural* + *semantic* + *lifecycle*. **Documentación pura:
cero cambios en código, prefabs, escenas o assets.** 43 diffs sobre 19 documentos.

**Por qué una sola sesión y en este orden.** Tres paquetes llevaban sin aplicarse hasta
solaparse sobre los mismos documentos: CSV-4c (16 diffs, desbloqueado desde 2026-08-10), R5-d
(20, HELD, código entregado 2026-08-26) y HUD-COMP-1 (7, cerrados el mismo día).
`CURRENT_STATE`, `changelog-ssot` y `coverage-matrix` recibían diffs de los tres. Se aplicó el
orden cronológico estricto **CSV-4c → R5-d → HUD-COMP-1** —la lección de DOC-APPLY-2
(2026-08-08, 50 ítems): aplicar el paquete nuevo primero produce anclas no encontradas en
cascada— y los tres ficheros compartidos se editaron **una vez cada uno, fusionando**.

**El hallazgo de la pasada: una invariante que ya estaba ocupada.** `PENDING_DOC_DIFFS_R5d.md`
pedía crear la **invariante 12** de `SSoT_Runtime_CompositionSession_Integration` §8, con ancla
marcada VERIFICADA. Para cuando se aplicó, §8 llevaba **13** invariantes: la 12 es el *singer
voice seam* (SINGER-1) y la 13 la resolución/cacheo de BPM (CTX-2a), ambas citadas desde
`coverage-matrix`, este changelog, `CURRENT_STATE` y `SSoT_Dev_Mode`. Renumerar habría roto seis
referencias vivas para respetar un número redactado antes de que el documento creciera. La
inyección de pista con alcance de render queda como **invariante 14**; la de HUD-COMP-1, como
**15**. Contenido intacto, número corregido, nota dentro del propio documento. *Es el mismo
error de clase que CSV-4c existe para no repetir: un dato que entra en documentación sin
comprobarse contra el documento real.*

**CSV-4c — el índice inverso carta→bundle (16/16).** D-CSV-16 cerrada con herramienta.
`SSoT_Editor_Authoring_Tools` §17 pasa de siete a **ocho vistas** (`Cards → Bundles`, §17.13
nueva), gana la columna `cards:` en Style Bundles, los flags `UNREACHABLE` / `UNSOURCED` /
`UNWIRED-SRC` (§17.6), dos campos en el esquema de bundles y un esquema de export nuevo
(§17.8), y §17.10 se reescribe de *hueco conocido* a *cerrado* conservando el registro
histórico íntegro. **Tres flags y no uno, a propósito:** la cadena es `bundle ← carta ←
catálogo/mazo ← músico/roster` y cada tramo roto pide una reparación distinta; colapsarlos haría
borrar un bundle correcto porque el catálogo de encima está roto. El cuarto salto **no** se
comprueba: `UNREACHABLE` es **suelo, no censo**. Primera medición: **18 de 35 bundles
alcanzables, 17 muertos, 10 de ellos Bassline**. La cifra de CSV-4 (14 de 33 progresiones)
queda **histórica y debe re-medirse, no re-citarse**. Hallazgo lateral: identificado el
generador de los nombres `*_Payload_<Role>_StyleBundle` —`CardEditorWindow`, dos rutas, ambas
derivando del nombre del payload—, lo que desbloquea el punto muerto de
`Design_Asset_Naming_v0_1` §7. También: `SSoT_Dev_Mode` §9.20 nueva (smoke ST-CSV4c-1..9).
Decisiones D-CSV4c-1=C · -2=B · -3=B · -4=B · -5=B.

**R5-d — Overload como carta Action (20/20).** Sin cambio de hogar primario.
`SSoT_Status_Effects` §5.10 gana la regla de consumo por coste de carta y **degrada el pasivo a
fallback** (`OverloadConsumerEnabled` default **OFF**, D-R5-20=B — verificado en código);
`SSoT_Card_System` §10.5 gana dos razones (`NoRunningLoop`, `Resource`) y una tabla de
precedencia renumerada a siete; `SSoT_Runtime_CompositionSession_Integration` §5.4 + inv 11
ganan la cláusula de bonus loop y §8 gana la **invariante 14** (inyección de pista con alcance de
render — un concepto que antes no existía); `SSoT_Gig_Combat_Core` §14.3 gana la excepción de
Seam C y §14.4 la viñeta de puerta ortogonal; `SSoT_Audio` gana **§4.7** (solo duck, tercer
plano) y la **invariante 19**; `SSoT_Card_Authoring_Contracts` gana §5.3a (par de coste de
recurso) y §5.6c (discriminador `GrantBonusLoop`). Decisiones D-R5-20..26 en
`RosterExpansion_Sub_Roadmap` §2; **D-R5-27 sigue abierta** y ningún diff aplicado depende de
ella.
**El coste es un campo de definición, nunca un `CardEffectSpec`** — los specs corren en
`CardBase.ExecuteEffects`, después de comprometer la jugada, así que un coste ahí permitiría
jugar la carta con el recurso vacío y fallar en silencio: carta gastada, efecto no. Y es un par
genérico `(statusKey, amount)`, no un campo `voltageCost`, porque el contenedor se indexa por
primitiva mientras la variante es la autoridad (D-R5-26=A).
**Estado documentado con marca explícita: implementado, SIN verificación de smoke.**
ST-R5d-1..15 no se han ejecutado (**D-DOC-2 = A**): se documenta con la marca en vez de retener
los diffs otro ciclo, porque llevan retenidos desde agosto y la deuda ya produjo este
solapamiento.

**HUD-COMP-1 — Composition View v1 (7/7).** `SSoT_Gig_Combat_Core` gana **§15**, la primera
definición gobernada de la tira de composición: qué muestra, identidad de fila
`"{musicianId}|{role}"` (BASS-1 en el HUD), estados de fila, aviso de loop final por **forma
además de color**, granularidad de pendiente por músico/parte, presupuesto de texto en reposo.
`SSoT_Runtime_CompositionSession_Integration` gana **§5.5** (tres seams de solo lectura) y la
**invariante 15**. **La UI muestra la tonalidad renderizada, nunca la del modelo** — las dos
divergen legítimamente cuando una carta de Backing adopta, y mostrar el modelo sería mentir
exactamente cuando el jugador necesita la verdad.
**Reversión explícita, marcada como tal:** `Design_Track_Card_Levels_v0_1` §7 sustituye el
numeral romano por **pips** (§7.1 nueva). Motivo: el numeral colisiona con los grados armónicos
(I, V, vi) que el juego ya usa para acordes — el mismo símbolo significando dos cosas en la
misma pantalla, y una de ellas en la tira donde el jugador lee armonía. No se aplicó en
silencio.
`Design_Sensory_Contract_v0_1` registra el floater `▲` como **excepción documentada** al
`SensoryEventBus` (**D-DOC-3 = A**), abierta para R7: el bus no tiene aún un tipo de evento con
granularidad de fila, e inventarlo es trabajo de R7, no de una sesión documental.
**Registrado como no implementado, deliberadamente:** el nivel de pista (R7, el campo del modelo
no existe) y el estado de fila «silenciada» (definido sin fuente de datos). **Bug conocido
diferido:** la línea `Instrument:` del hover no resuelve de forma fiable — el seam y la clave de
tres segmentos son correctos, falla el momento de la consulta.

**Deuda que NO se salda aquí.** `PENDING_DOC_DIFFS_R5.md` y sus tres addenda **no se han
aportado en ninguna sesión**; sus **siete diffs HELD** (D1, D2, D3, D7, D8, D10, D11-original)
siguen sin aplicar y su retirada es criterio de cierre explícito de R5. Hay que recuperarlos o
declararlos perdidos y re-derivarlos desde el registro de R5-a/b/c. **R5 no pasa a CLOSED:**
faltan esos siete diffs, ST-R5d-1..15 y D-R5-27.

**Deuda de código detectada, no de documentación (DD-R5d-20).** `CardLLMPromptBuilder` enumera
los discriminadores de efecto **a mano** y lista seis; el importador acepta siete desde R5-d. El
generador LLM **nunca emitirá `GrantBonusLoop`** hasta que se añada allí. A diferencia de los
alfabetos de enum de la etapa 1, que salen de `Enum.GetNames()` y se auto-actualizan, ese bloque
envejece en silencio. Registrado en `Report_CardLLM_Pipeline.md`; sin lote asignado.

**Autoridad.** Ningún concepto queda con dos hogares. `Composition_View_Spec.md` se registra
como **reference (planning), no autoridad**: la tira se define en `SSoT_Gig_Combat_Core` §15.
`ssot_manifest.yaml` se tocó por un solo motivo (DD-R5d-19): los `CardEffectSpec` se gobiernan
**por carpeta**, no por fichero, así que `GrantBonusLoopSpec.cs` no necesitaba ruta nueva — pero
la invariante que enumera el vocabulario activo llevaba tres specs de retraso y se corrigió.
`SSoT_INDEX.md` recibe el alta de referencia de HUD-COMP-1 D-06.

Ficheros retirables del PK: `CSV-4c_Doc_Diffs.md`, `PENDING_DOC_DIFFS_R5d.md`,
`PENDING_DOC_DIFFS_HUD-COMP-1.md`.

**Cierre en limpio — D-DOC-5 (mismo día, decisión de usuario).** Todo input que no está en el
Project Knowledge se declara **perdido**, no pendiente. Afecta a tres cosas y ninguna se deja
como deuda abierta:

1. **`PENDING_DOC_DIFFS_R5.md` + addenda R5-a/b/c — PERDIDOS.** Sus siete diffs HELD (D1, D2, D3,
   D7, D8, D10, D11-original) no se aplicarán. **No dejan hueco de contenido, y la razón es
   estructural, no optimista:** DOC-APPLY-R5 los retuvo precisamente porque describían el loop de
   bonus, el solo y Overload-como-carta *cuando ese código no existía*. El código se entregó en
   R5-d y quedó documentado el mismo día por los 20 diffs de `PENDING_DOC_DIFFS_R5d.md`, escritos
   contra el código real y con anclas verificadas. Los tres HELD que apuntaban a
   `SSoT_Runtime_CompositionSession_Integration` cubrían el territorio que hoy ocupan §5.4, §8
   inv 11 y §8 inv 14. Lo verdaderamente perdido es el **rationale de redacción anterior al
   código** — qué alternativas se descartaron antes de escribirlo. No es reconstruible y no se
   inventa. Registro completo: `RosterExpansion_Sub_Roadmap.md` §3.1.
2. **`Composition_View_Spec.md` — PERDIDA.** La fila que DOC-APPLY-3 acababa de dar de alta en
   `coverage-matrix` y `SSoT_INDEX` **se retira**: un índice gobernado no puede apuntar a un
   fichero que no existe. `SSoT_Gig_Combat_Core.md` §15 pasa de «resumen gobernado de una spec de
   planning» a **único registro de la tira**, y lo dice en su encabezado — no hay documento
   aguas arriba con el que reconciliar.
3. **`CLOSEOUT_HUD-COMP-1.md` — PERDIDO.** Las cifras de smoke de HUD-COMP-1 (11 PASS, ST-6b y
   ST-10 diferidos) quedan **sin fuente independiente**, sostenidas sólo por el propio paquete de
   diffs. Anotado como tal.

**Consecuencia de gobernanza: R5 queda con un único criterio de cierre pendiente — ST-R5d-1..15.**
«Retirar los cuatro ficheros `PENDING_DOC_DIFFS_R5*`» deja de ser criterio de salida.

**Regla que sale de la pérdida.** Un paquete de diffs retenido es un fichero suelto sin dueño: no
vive en carpeta gobernada, no aparece en el manifiesto, y nada obliga a adjuntarlo a la sesión
siguiente. Retener por la razón correcta y **no fijar fecha de consumo** es exactamente cómo se
pierde. En adelante: **un paquete retenido nombra en su propia cabecera el lote que lo consume; si
ese lote cierra sin consumirlo, se declara perdido en ese mismo cierre, no más tarde.**

---

## 2026-08-21 — R5-a / R5-b / R5-c: Voltage, generación pasiva y Overload pasivo (**R5 queda PARCIAL**) + DOC-APPLY-R5 (2026-08-23, parcial)

**Tres sub-fases cerradas el 2026-08-21; el lote R5 NO cierra.** Lo que sigue separa lo
construido de lo que se dio por construido y no lo estaba.

**R5-a — el estado.** `Voltage`: primitiva propia `ResourceCounter = 993` (rango Meta,
**D-R5-7=A**), `Additive` / `MaxStacks 9` / `DecayMode None` / `TickTiming None`, en
`StatusEffectCatalogue_Musicians`, portador único Conito. Contador puro: sin efecto intrínseco,
el significado lo pone el consumidor. Se rechazó reutilizar `ResourceGenerationModifier = 992`
porque Voltage **es** el recurso, no un modificador de su generación. **Alcance gig**
(**D-R5-8=A**), y la razón importa: `GigManager.ResetSongScopedStatuses` no es un barrido por
categoría sino una **allowlist literal de dos primitivas** (`DamageUpFlat` = Flow,
`TempShieldTurn` = Composure), y además se invoca desde `StartCompositionSession`, es decir al
**abrir** la canción siguiente y no al cerrar la anterior — de modo que un estado generado en los
turnos entre canciones se borraría justo cuando la canción empieza. El esqueleto original de la
ficha de Voltage afirmaba lo contrario ("el boundary de canción sí lo limpia"); la verificación
empírica lo desmintió y la entrada se **sustituyó**, no se matizó. Dos verdades de código que
nunca se habían escrito quedan fijadas en el SSoT: **`TickTiming.None` significa "en todos los
timings", no "en ninguno"** (el filtro de `Tick` solo descarta timings definidos que no coinciden;
lo que impide el decay es `DecayMode.None` en solitario), y **el contenedor está keyed por
primitiva, no por `StatusKey`** ⇒ una instancia por primitiva y portador, que es la razón de fondo
de la guarda dual y del "toda semántica nueva pide primitiva propia". Deuda de herramienta
registrada: el `StatusEffectWizardWindow` no escribe `isDefaultVariant`, `iconSprite` ni
`description`, y nombra el asset por primitiva y no por variante. **ST-R5a-1..5 + 6R PASS.**

**R5-b — la generación.** Hook pasivo **+1 Voltage por jugada genuinamente consumida de Conito**,
colocado en la rama `if (ok)` de `GigManager.TryConsumePlay` (**D-R5-5=A**). El punto no es
estético: es el único sitio por el que pasan las dos rutas de juego con el intérprete ya resuelto,
y colgar el hook del **consumo** y no del retorno evita el falso positivo de la rama
`musician == null`, que devuelve `true` sin consumir. Se rechazó suscribirse a `CardPlayedEvent`
porque ese evento **no lleva el intérprete**. Cuentan todas las jugadas consumidas, acción y
composición, **coste 0 incluido** (**D-R5-10=A**): bajo D-ECON-6=DEFER todas las starter son coste
0, así que excluirlas habría hecho Voltage inalcanzable con el contenido actual. Restricción a
Conito por identidad de músico (**D-R5-9=A**), sin marcador autorable. Interruptor en
`GigFlowSettingsSO.GenerateVoltageOnConsumedPlay` (**D-R5-12=A**, default ON, leído por jugada ⇒
conmutable en caliente) — es regla de gig, no debug. Dos conductas contraintuitivas, ambas
predichas antes de ejecutar la suite y registradas como contrato, no como hallazgo: **genera quien
paga el presupuesto**, de modo que una carta `AnyMusician` facturada a Conito genera y una carta de
Conito facturada a otro no; y con el toggle en ON las cartas `DEV_Voltage_*` dejan de ser
instrumentos neutrales (aplican su delta **y** disparan el hook), por lo que la aritmética de los
runbooks de R5-a solo vale con el toggle en OFF. Techo medido: **+2 Voltage por periodo**.
**ST-R5b-1..6 + 7R PASS.**

**R5-c — el consumidor.** `StatusEffectContainer.SpendStacks(id, n) → int` (**D-R5-18=C**): gasto
explícito de recurso, gasta `min(stacks, n)`, dispara `OnStatusChanged`/`OnStatusCleared` y **no**
publica `StatusAppliedEvent`, porque gastar un recurso no es aplicar un status. Las dos
alternativas se descartaron por conducta, no por gusto: `ConsumeOnTrigger` guarda
`Decay == ConsumeOnTrigger` y sobre un status `DecayMode.None` sería un **no-op silencioso** —
multiplicador cobrado sin pagar —, y `Apply(-n)` publicaría un delta negativo en el bus, una
afirmación falsa. Encima de esa API, **Overload pasivo**: al cierre de cada loop de composición, si
el portador tiene ≥ `OverloadThreshold` (6) stacks, gasta `OverloadCost` (6) y multiplica **la
contribución de ese loop** a SongHype por `OverloadHypeFactor` (×1.5). Tres precisiones que
costaron decisión propia. (1) El consumidor va **al inicio** de `OnCompositionLoopFinished`
(**D-R5-16=A**) porque `ComputeLoopScore`/`ComputeHypeDelta` viven dentro de
`TriggerAudienceMicroReactions`, que es la primera llamada de ese método: colocarse antes permite
afectar al loop **que acaba de cerrarse** y elimina la necesidad de estado pendiente entre loops y
de su limpieza en el boundary de canción. (2) El factor se aplica sobre **`hypeDelta`**, no sobre
el `loopScore` crudo (**D-R5-17=A**, corrige explícitamente la formulación previa "multiplicador de
LoopScore", emitida antes de leer el seam): `ComputeHypeDelta` es una función escalonada y escalar
su entrada da un efecto que a veces es nulo y a veces desproporcionado. (3) El coste se paga
siempre al cruzar el umbral y el factor solo si el delta es positivo (**D-R5-19=B**), preventivo
frente a contenido futuro con penalizaciones. `meters.SongHypeDeltaMultiplier` **no se muta**: es
configuración persistente del encuentro. Umbral 6 / coste 6 (**D-R5-14=A**) se fijó contra el dato
medido en R5-b (+2/periodo ⇒ ~3 periodos por carga), sustituyendo el 3/3 que se había asumido antes
de existir esa medición. Regla de tuning heredada: si no dispara en canciones cortas se **baja el
umbral**, nunca se sube la generación. **ST-R5c-1..9 PASS a la primera.**

**Hallazgo de gobernanza — F-R5c-4 (por qué R5 no cierra).** El alcance R5 aceptado **antes** de
escribir código —fila R5 del sub-roadmap, `D-R0-5=A` (Overload es Action-domain, **carta
jugable**), `D-R0-12` (coste 2, **Voltage ≥ 3**), `D-R5-4=A` (solo de un loop por inyección con
alcance de render), `D-R5-6=B` (el loop de bonus no refilla ECON-1)— define Overload como *carta
que concede un loop de bonus con un solo de guitarra de Conito encima de la base*. Lo construido en
R5-c es *un disparo automático que multiplica el hype ×1.5*. **D-R5-13/14/15 hicieron esa
sustitución sin citar ni una sola de las decisiones anteriores, y ninguna quedó registrada como
revertida.** Lo entregado es una **capa adicional** sobre el mismo recurso, no el finisher de R0.
Consecuencia: **R5 = PARCIAL** (**D25**), el alcance no construido pasa a **R5-d**, y la
sustitución queda registrada en el ledger (**D26**) en vez de quedar implícita. Abiertas para
R5-d: **D-R5-20** (convivencia disparo automático ↔ carta), **D-R5-21** (umbral 6 vs ≥3),
**D-R5-22** (otros consumidores de Voltage).

**DOC-APPLY-R5 (2026-08-23) — aplicación PARCIAL, deliberada.** De los 26 diffs acumulados se
aplican **19**; **siete quedan HELD** (D1, D2, D3, D7, D8, D10 y el D11 original) porque describen
el loop de bonus, el solo y Overload-como-carta — **código que no existe**. Aplicarlos habría
introducido divergencia doc↔código *a propósito*, que es justo lo que el invariante "code and docs
may diverge; when they do, identify the divergence explicitly" existe para impedir. Por la misma
razón, y **rompiendo la convención de paquetes** (el fichero de diffs se retira al aplicarse), los
cuatro ficheros `PENDING_DOC_DIFFS_R5*` **no se retiran**: se anotan con qué quedó aplicado y qué
retenido, con fecha, y se consumen al cerrar R5-d. Docs editados: `SSoT_Status_Effects.md` (§2.1 ·
**§3.0 nueva** · **§5.10 nueva** · §6) · `SSoT_Gig_Combat_Core.md` (**§3.1.1 nueva** · **§3.3.1
nueva** · §14.4) · `SSoT_Scoring_and_Meters.md` (§3.3) · `SSoT_Editor_Authoring_Tools.md` (§6.3) ·
`RosterExpansion_Sub_Roadmap.md` (ledger R5 · nota de sustitución · fila R5 PARCIAL · fila R5-d) ·
`CURRENT_STATE.md` (§1/§3/§5) · `coverage-matrix.md` · este fichero. **Sin cambios** en
`SSoT_INDEX.md`, `ssot_manifest.yaml` ni `SSoT_Runtime_CompositionSession_Integration.md` — este
último precisamente porque sus tres diffs son los HELD.

**Hallazgos menores registrados, no aplicados como diff:** **F-R5c-2** — la copia de
`LoopScoreCalculator.cs` en project files está desfasada (expone sobrecargas sin parámetros de
config que `GigManager` ya no usa); refrescar. **F-R5a-1** — la copia de `CharacterStatusId.cs`
estaba desfasada al abrir R5 (le faltaban `NegateIncomingPositive = 404` y `RedirectIncoming =
504`); refrescada en R5-a. Lección operativa: una auditoría de ids libres no puede apoyarse solo en
el enum, hay que cruzarlo con el registro CSO, que en este caso estaba más al día que el propio
contrato de serialización.

---

## 2026-08-11 — PRES-1 (+1b/1c) cierre + DOC-APPLY-PRES1 + apertura de R5

**PRES-1 cerró el 2026-08-11** (15/15 smokes PASS, 0 diferidos; ST-PRES1-1..11d). Cuatro
superficies: **Psychic Wave v2 → v4** (cover→hold→uncover ancladas al performer; inversión de
color real vía `Blend OneMinusDstColor OneMinusSrcAlpha` con fuente premultiplicada — GrabPass no
existe en URP y ningún SRP texture contiene un canvas Screen-Space-Overlay, así que leer el
framebuffer no es caro sino imposible; posición de viewport por `ComputeScreenPos` y no por UVs
de sprite, que en una `Image` sin sprite son cero en todos los vértices; radio de cobertura
calculado por jugada; producción 0.45/0.30/0.70/0.12 ≈1.45 s; **frente de tinte v2/v3 retirado**,
`Image` legacy force-disabled en `Awake`) · **floater de redirect de Spotlight** (dos ramas; el
objetivo original lo nombra el MISMO selector puro del camino normal —
`SelectDefaultMusicianTarget`, extracción behaviour-preserving para que camino y floater no
puedan divergir; la rama `RandomMusician` ancla en el protegido porque nombrar al original
exigiría tirar `Random.Range` y desplazar toda la secuencia de RNG del gig; supresión del no-op
visual con log positivo; **cierra D-R4-8**) · **reveal de gustos compuesto en el tooltip de
hover** + icono persistente (panel retirado; `IsTastePanelWired` re-semantizado — ahora informa
del icono; encabezado en negrita; **cierra D-R4-10**) · **outline de sprite M1.7 RESTAURADO como
highlight de hover** (la llamada se había perdido en la historia del repo y el proyecto trataba
la capacidad como inexistente; toggle centralizado en `CharacterBase.OnPointerEnter/Exit` para
que todo tipo de personaje presente y futuro lo herede sin edición por subclase; **no es feature
nueva** — registro de lifecycle explícito para que una auditoría futura no la lea como tal).

**Decisiones:** D-PRES1-1=B+ · D-PRES1-2=A · D-PRES1-3=A · D-PRES1-4=A · D-PRES1-5=A
(supersedida por B+) · **D-PRES1b-1=B** (revierte una recomendación previa de usar una `Image` en
`HighlightRoot`: el shader M1.7 existía en el repo) · D-PRES1b-2=A (canal `HighlightRoot`
conservado inerte y null-guardeado) · D-PRES1c-1=A · D-PRES1c-2=A (los síntomas de
melodía/tonalidad se investigan en MidiGenPlay, MGP-TONALITY-1, no aquí).

**Encoding fix (D-PRES1-4=A):** `AudienceCharacterCanvas.cs` contenía 7× U+FFFD REPLACEMENT
CHARACTER — pérdida irreversible, no un mis-encoding recuperable. Cuatro estaban en **copy ESP
visible al jugador** de la tester build (`¡Súper!`, `persuasión`, `está`, `posición`), lo que
socavaba directamente D-REPLAN-1 (comprensión en español sin asistencia); tres en comentarios.
Restaurados como UTF-8. Causa probable: guardado como CP1252/Latin-1 releído como UTF-8. **El
resto del repo no se ha barrido** — abierto en `CURRENT_STATE.md` §4.

**Hallazgo F-PRES1b-1:** `SelectDefaultMusicianTarget` elegía el `CurrentStress` absoluto más
alto; bajo el medidor invertido S5e, el músico **más sano**. El comparador no se volteó en S5e
porque el selector lee el campo crudo, fuera de la API direction-agnostic que S5e sí protegió.
**Resuelto al abrir R5 — D-R5-2=A** (más cercano al Breakdown = absoluto más bajo). Cambio de
gameplay; ST-R5pre-1..4 + regresión ST-PRES1-4/-6 debidos.

**DOC-APPLY-PRES1 (D-R5-1=A, fase de apertura de R5)** aplicó los diffs de
`PENDING_DOC_DIFFS_PRES1.md` + `PENDING_DOC_DIFFS_PRES1c.md` (ambos retirados al aplicar; HELD-1
quedó resuelto por PRES-1b, HELD-2 migrado a `CURRENT_STATE.md` §4): `SSoT_Status_Effects.md`
§5.9 · `SSoT_Audience_and_Reactions.md` §6.4 + §8 · `Design_Sensory_Contract_v0_1.md` (evento,
Psychic Wave v4, superficies de presentación de personaje, nota de logs — sigue `planning`) ·
`RosterExpansion_Sub_Roadmap.md` (ledger de apertura de R5) ·
`SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.11 · `CURRENT_STATE.md` (§1/§3/§4/§5) · esta entrada ·
`coverage-matrix.md`. **Enmiendas declaradas:** D10 se aplica como resuelto y no como pregunta
abierta (la decisión se tomó el mismo día); la viñeta del encabezado en negrita de §6.4 queda
**en espera de ST-PRES1-7b**, que verifica que TMP renderiza `<b>` en lugar de imprimirlo literal.

**R5 ABIERTO 2026-08-11** (Conito Overload — lote solo, toca invariantes de sesión). Fase R5-pre:
fix de targeting (D-R5-2=A), cierre de F-R4-3 (D-R5-3=A), retirada del log `[PRES-1][Selector]`
tras sus smokes, pasada de prefab (objeto `TastePanel` + campos muertos `tastePanelRoot` /
`tasteText`). Fase siguiente: **R5-inv**, la review de invariantes de sesión previa al núcleo de
Overload.

---

## 2026-08-10 — R4 (Finishers I) cierre + DOC-R4 (doc-update)

**R4 cerró el 2026-08-10** con las cuatro piezas entregadas: **Psychic Wave v2**
(`ApplyStatusEffect(earworm, +2, AllAudienceCharacters)` sobre el AoE de Vibe; la rama AoE
excluye `IsBlocked` — comportamiento verificado, no bug; overlay full-screen vía
`PsychicWaveOverlayController` dedicado sobre el bus) · **C2 Spotlight/Taunt** (primitivo nuevo
`RedirectIncoming = 504`; hook previo en `ResolveTargetsFor` para `Musician`/`RandomMusician`,
`AllMusicians` exento; guard doble primitivo+key; ciclo Composure ⇒ 1 turno de audiencia sin
código de expiración propio) · **Read the Room** (`RevealPreferencesSpec`, RewardPool Sibi
coste 0; el spec no transporta datos de gusto — `AudienceCharacterData` posee los datos, el
canvas la presentación) · **Keep Cool retarget** `Self`→`Musician` (regresión de tutorial
ST-R4-9 PASS). **ST-R4-1..10 PASS · V-R4-MODAL PASS** — este último salda la deuda auditiva
que R3 dejó abierta por falta de contenido: la melodía sobre parte modal resuelve contra el
modo impuesto.

**Decisiones:** D-R4-2=defer-A · D-R4-3=A · D-R4-4=A · D-R4-5=A · D-R4-6=A · D-R4-9
verificada empíricamente · **abiertas** D-R4-7 (VFX v3) / D-R4-8 (legibilidad del taunt) /
D-R4-10 (reveal a hover) · **D-R4-1 heredada, sigue abierta** — dirección decidida (el público
debe juzgar lo que suena), cableado **prohibido**; lote propio antes de R8.

**Hallazgos:** **F-R4-1** — la verificación V5 de R0 afirmaba «one shared target list serves
all specs on a card»; el código resuelve **por spec** (equivalente para `All*`, no para
`Random*`). Corrección aplicada en los **tres** sitios: `SSoT_Card_System.md` §8.2 ·
`RosterExpansion_Sub_Roadmap.md` §9 · `Design_Starter_Deck_v2_DRAFT.md` §7. · **F-R4-2** —
compás/tonalidad se muestran al jugador como nombres de enum (`SixEight`); sin lote. ·
**F-R4-3** — `StatusEffectWizardWindow` no escribe `statusKey` (assets irreconocibles sin
error); recomendado colarlo al abrir R5.

**DOC-R4 (documentación pura — sin código, sin assets, sin smokes)** aplicó los once diffs de
`PENDING_DOC_DIFFS_R4.md` (retirado al aplicar): `SSoT_Status_Effects.md` (**§5.9 Spotlight
nueva** · §5.7 Earworm appliers + salto de `IsBlocked` en AoE · §8 nota del doble registro
CSO) · `SSoT_Card_Authoring_Contracts.md` (§9 conformidad `RevealPreferencesSpec` + **nota
normativa de los dos sitios de targeting de la capa 4** · §5.6b ejemplo JSON) ·
`SSoT_Audience_and_Reactions.md` (**§6.4 reveal nueva** · §8 hook de redirección · §6.1 nota
F-R4-2) · `SSoT_Card_System.md` (§8.2 resolución por spec + exclusión `IsBlocked` · §6.2
vocabulario 5→6 specs) · `RosterExpansion_Sub_Roadmap.md` (fila R4 CLOSED · ledger R4 · §9 V5
· §8/§10 deudas saldadas) · `Design_Starter_Deck_v2_DRAFT.md` (filas 5/6/Read the Room
construidas · §7 V5 · §8 residuales) · `CURRENT_STATE.md` (§1 fila R4 · §3 planificación por
superficies S1–S6 + orden propuesto · §4 cinco abiertos nuevos + nota de proceso PK↔repo +
duplicado D-R4-1 colapsado · §5 estado post-update) · esta entrada · `coverage-matrix.md`
(Spotlight §5.9 · `RevealPreferencesSpec` · fila `PsychicWaveOverlayController` sin hogar) ·
`Design_Tutorial_System_v0_2.md` (nota del beat de Composure; TUT-REFRESH registrado como lote
propio) · `ssot_manifest.yaml` (invariantes cacheados de Status Effects y Audience — únicas
entradas tocadas; F11/F16/D-SENSORY-HOME quedan para MANIFEST-2).

**D-DOC-R4-1 = A:** `Design_Sensory_Contract_v0_1` **no** se promueve a autoridad en este
lote; `PsychicWaveOverlayController` queda registrado como **tercera** entrada sin hogar,
alimentando **D-SENSORY-HOME** (se resuelve en MANIFEST-2). Promover un documento es cambio de
estructura, no de contenido; pertenece al lote que revisa el manifiesto completo.

**Nota de proceso (regla operativa nueva):** el snapshot del PK diverge del repo y en R4 casi
provoca dos regresiones (una indetectable por el compilador). Regla: **no reescribir archivos
completos desde el PK** — parches sobre anclas o archivos vivos subidos en sesión.

Clasificaciones: semantic (D1/D3/D4, incl. corrección F-R4-1) · operational (D2/D7) ·
lifecycle (D5/D8) · reference-only (D6/D10) · structural (D9) · authority (D11).

---

## 2026-08-08 — MANIFEST-1: reparación de la gobernanza que el auditor destapó

**Documentación pura. Sin código, sin smoke tests** (regla del proyecto: los lotes
documentales no los requieren). Clasificación: *authority* + *structural* + *operational*.

`ssot_manifest.yaml` llevaba sin tocarse desde **2026-04-14** — casi cuatro meses — y **cuatro
de los siete hallazgos** del auditor en DOC-APPLY-2 §9 eran síntomas de ese único hecho. Este
lote los ataca en la causa, y de paso hace algo que nunca se había hecho: **validar el
manifiesto contra el sistema de ficheros** (export del árbol, 3.251 rutas) en vez de contra
otros documentos.

**Altas.** `systems/SSoT_Dev_Mode.md` (`subsystem_ssot`, gobierna `Assets/Scripts/DevMode`,
**doce invariantes duros levantados del documento**, incluidas las siete líneas de log
protegidas de §19 y la trampa `[B1][stemCache]` vs `[DIAG]`) · `systems/SSoT_Singer_Voice.md`
(`subsystem_ssot`, ocho invariantes) · `planning/active/Design_Composition_Debug_Tab_v0_1.md`
· `planning/Design_Game_And_Card_Maxims_v0_1.md` (saldando la deuda BALANCE-XREF) ·
`planning/active/Design_Tutorial_System_v0_2.md` · `archive/Design_Starter_Deck_v1.md` ·
`planning/archive/Design_Tutorial_System_v0_1.md` · `CONTRIBUTING.md` (`excluded`).

Que `SSoT_Dev_Mode` faltara era el hallazgo grave: LOG-1 acababa de escribir en él §19, que es
**autoridad normativa sobre qué líneas de log no se pueden degradar**, sobre un documento que
la gobernanza no reconocía. Un auditor futuro no lo habría mirado, y la protección de siete
observables de smoke dependía de eso.

**Correcciones de clase y de ruta.** `changelog-ssot.md` reclasificado `archive` → `reference`
(**D5=A**): es un documento vivo, escrito en cada cierre, y desde D-LOG-4=C es el hogar de la
definición de `snapshot-01` — declararlo archivo aplicaba «el contenido de archivo no es
autoritativo» a un fichero del que cuelga un concepto vivo. Ruta de Starter Deck v2
reconciliada contra el fichero real `_DRAFT` (**D6=B**: se corrige la entrada, no se renombra
el fichero; el sufijo es exacto hasta R8, y el propio v2 afirmaba un renombrado que **nunca
ocurrió en disco**). Aplicado por fin el fragmento **OPT-5**, debido desde 2026-07-13: el
invariante 18 de `SSoT_Audio` («una carta suena una vez, por exactamente un camino»). Cinco
rutas corregidas contra el árbol: `Design_Action_Economy_v1` vive en `planning/`,
`Design_Asset_Naming_v0_1` en `reference/`, `AlwtttLogSetup.cs` en `Assets/Scripts/Data/`,
`Design_Composition_Debug_Tab` en `planning/active/`, y los tres README de carpeta llevan
nombre prefijado. **F10 cerrada:** el manifiesto tenía razón y `SSoT_Dev_Mode §6` estaba mal
en dos rutas de código.

**Regla nueva (D8), en `SSoT_CONTRACTS §8`:** el registro de un smoke vive en la SSoT dueña del
**observable**; el changelog registra el resultado del lote; `coverage-matrix.md` **no** crece
una sección de smokes. Salda la disposición R2-11 diferida en DOC-APPLY-1 y re-diferida en
DOC-APPLY-2. El precedente ya existía y nadie lo había escrito: `SSoT_Dev_Mode §9` son
diecinueve secciones de smoke por lote, y §19.2 ata siete líneas de log a los tests que las
consumen.

**Tres contradicciones de autoridad que solo el árbol podía destapar, resueltas aquí.**
**F17** — existían **dos** documentos de tutorial vivos a la vez, v0_1 en `planning/active/` y
v0_2 en el `planning/` menos activo, con el índice apuntando al viejo y la matriz al nuevo.
Verificado que v0_2 absorbe a v0_1 entero (historia en cabecera, §6 y §6A preservados como
inventario reactivo histórico, ledger D-TUT-6..11 retenido) **antes** de archivar, no después:
v0_1 a `planning/archive/` con banner, v0_2 a `planning/active/` (**D11=A**). **F18** —
`Design_Starter_Deck_v1.md` está en `archive/` mientras el índice lo llamaba «authoritative for
the live S5 demo starter». La resolución es la contraria a la esperada: **la carpeta era
correcta y la palabra era falsa** — por la propia cláusula de clasificación de v1, desde el
cierre de M4.6 los `.asset` autorados son la autoridad de runtime y el documento es rationale
retenida. Corregidos el índice y la cláusula de supersesión de v2; el fichero no se movió
(**D10=B**). **F14** — `Roadmap_ALWTTT_Debug_Seams.md` no existe en el árbol: fila retirada del
índice (**D12**), y el registro de cierre del arco MGP-ALWTTT-DBG vive en `SSoT_Dev_Mode §18`
y en la matriz.

**Inventario re-baselinado: 232 assets** (export del 2026-08-08, D3=A). Sustituye a los 183 del
2026-07-20. Tabla por familia y lectura de salud en `SSoT_Editor_Authoring_Tools.md` §17.12.
**D-CSV-14 queda verificada y cerrable:** de los 7 `OFF-ROOT`, cinco son package-side bajo
`Samples/` y los **dos locales son exactamente** los patrones de melodía bajo `Patterns/Melody`
(singular), que es lo que la decisión pedía confirmar. Catorce progresiones marcan `BASS-GAP`,
que es justo la señal detrás del estándar de 8 compases de CR-10.

**Diseño registrado.** **D-R4-1 = RECONOCER, NO CONSUMIR**: el público sí debe juzgar la
tonalidad que suena, pero no se cablea en este demo; el límite (el seam lee tonalidad autorada)
pasa de hallazgo sin dueño a deuda con dirección decidida, y queda prohibido «resolverlo» hacia
la autorada. **D-R4-2 = A**: el calificador de canción (HAPPY/SAD/FUNKY) se **deriva** —bucket
Major/Minor por la tercera del modo sonante—, nunca se autora como fuente primaria, porque un
calificador escrito a mano se desincroniza de lo que suena y esta vez sin nada contra qué
contrastarlo. Mostrarlo destacado en la descripción de la carta es opcional y no bloquea R4.

**Señales.** F7 rebajada a LOW con evidencia verificada en disco. Cerradas: F10, F14, F17, F18.
Abiertas: **F11** (~17 documentos indexados aún ausentes del manifiesto → MANIFEST-2, después
de R4, junto con D-SENSORY-HOME), **F12** (el esquema del manifiesto diverge del que documenta
la propia skill del auditor), **F13** (tres obligaciones de DOC-APPLY-1 re-registradas con
dueño), **F15** (tres solapes de `governs` declarados deliberados tras un barrido mecánico),
**F16**, abierta por la mañana y **casi cerrada el mismo día** con confirmación del usuario y
los dos documentos del POC en mano. `GigSetupSceneManager.cs` fue **borrado el 2026-05-18** y
siempre estuvo en el registro: las notas de cierre de D-FAST-1=C en `Roadmap_ALWTTT` dicen
"Empty `GigSetupSceneManager.cs` deleted" como parte del pivote de tres escenas a dos — el
manifiesto llevaba casi tres meses gobernando un fichero inexistente. `Report_CardLLM_Pipeline.md`
**ya no existe**: su entrada queda como lápida y sus dos citas en
`SSoT_Editor_Authoring_Tools` se corrigen, con la consecuencia **registrada y no parcheada** de
que el mecanismo de siete etapas del pipeline LLM se queda **sin hogar documental** (el código
es la única descripción). Y los dos documentos del POC de Pink Trombone **pertenecen al
proyecto MidiGenPlay**. Residuo único: `Docs/planning/music/` no existe, así que el informe de
emulación de soundfont no tiene hogar. **F19 abierta y cerrada el mismo día (D13=A)**: era la pregunta que estaba
escondida debajo. `PinkTrombone_Voice_Levers.md` es la fuente del esquema de `VoiceProfileSO`
—un asset de ALWTTT— y vivía en el otro proyecto, mientras su propia cabecera declara que
"nunca entra en el PK de MidiGenPlay como autoridad" **y** que "viaja con el cantante cuando se
promueve a ALWTTT". El cantante se promovió en SINGER-1, así que el disparador que el propio
documento nombraba ya se había cumplido: **promovido a `Docs/reference/` de ALWTTT** y
registrado en el manifiesto y en `SSoT_INDEX`. No es un interno del paquete: son seis levers de
diseño de voz consumer-side, y la regla de frontera excluye los internos, no el diseño del
consumidor. El fallo que esto cierra es concreto: con el esquema al otro lado, una edición de
los levers en MidiGenPlay habría desviado `VoiceProfileSO` **sin que ninguna corrida del
auditor pudiera verlo**, porque el auditor no cruza la frontera. El `Rendering_POC_Verdict` se
queda en MidiGenPlay como research. Corolario del mismo par de documentos: **la promoción del fork a
`Assets/ThirdParty/` nunca se ejecutó** —ambos lo sitúan en `Assets/PinkTrombonePOC/`, igual
que el árbol—, así que §3.6 y §7 de Singer Voice quedan corregidas a la única carpeta que
existe. El invariante se sostiene en sustancia; lo que queda abierto es de ciclo de vida:
si se quiere la promoción antes de retirar el arnés, porque hoy el código de producción del
cantante cuelga de una carpeta llamada "POC".

Hogares: `ssot_manifest.yaml` · `SSoT_INDEX.md` · `coverage-matrix.md` · `SSoT_CONTRACTS.md §8`
· `SSoT_Dev_Mode.md` · `SSoT_Singer_Voice.md` · `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8 ·
`SSoT_Editor_Authoring_Tools.md` §17.12 · `CSV_Composition_Validation_Sub_Roadmap.md` §4.1.1 ·
`CURRENT_STATE.md` §4/§5.

---

## 2026-08-08 — LOG-1: higiene de consola, etiqueta de acorde reparada, tag `snapshot-01`

**Operacional + semántico.** ST-LOG-1..8 todas PASS.

- **Etiqueta de acorde reparada (D-LOG-1=B).** `I?7` pasa a leerse `Imaj7`. La etiqueta se
  reconstruye desde `deg` + `quality` —ambos ASCII por construcción— y **el glifo del marcador
  no se muestra nunca**: los text-events MIDI se escriben en 7 bits y sustituyen el carácter no
  mapeable por `?` **al escribir**, así que el original ya no existe cuando el host lee, y un
  `I7` de aspecto reparado significaría un acorde distinto del que suena. La tabla de sufijos
  acopla **por cadena, no por el enum `ChordQuality`**: ese enum es package-owned y append-only,
  de modo que un `switch` por valor rompería el build ante un rename y se volvería
  silenciosamente no-exhaustivo ante una adición; por cadena, un nombre desconocido cae al
  `default` y **se reporta una vez por render** con el nombre real. Home: boundary §8.10.
- **Causa raíz NO afirmada.** Queda acotada a dos hipótesis —pérdida de encoding en el
  text-event MIDI (H1) vs. el paquete emitiendo `?` (H2)— con el discriminador ya en el código
  (`ReportChordTagDamage` imprime los campos crudos) pero **el dato sin capturar**. El ask
  **MGP-CHD-ASCII-1** se archiva **sin prioridad asignada** hasta leerlo. La mitigación
  consumer-side es completa e independiente de cuál sea la causa.
- **Niveles de log (D-LOG-3=B).** `LogVerbose` en `GigDevSettingsSO` + campo propio `logVerbose`
  en `MidiMusicManager` (host-owned, **no** sobrescrito en el arranque, a diferencia de
  `logDebug`, que pasa a `[HideInInspector]` precisamente porque el arranque lo sobreescribe y
  el mando del inspector no decidía nada). **Las siete líneas protegidas no cuelgan de ningún
  flag verbose** — ST-LOG-3 es su regresión. Escritas en `SSoT_Dev_Mode.md` §19 con el test que
  sostiene cada una, más la trampa `[B1][stemCache]` vs `[B1][stemCache][DIAG]`: dos líneas con
  tiers distintos que un `grep` por el prefijo común confunde.
- **Trazas de pila de `LogType.Log` desactivadas** desde `AlwtttLogSetup`, antes de cargar
  escena. 762 de 864 líneas de la captura de referencia (`log11.txt`) eran traza sin
  información. Warning y Error las conservan. Coste de información **cero**, por eso va primero.
  Escape: `ALWTTT/Debug/Log stack traces`.
- **`[F-4]` retirado — con el `try/catch` INTACTO y su volcado de error CONSERVADO.** Se
  retiraron las dos líneas *tageadas*, no la defensa de F-4 Stage A, que el código declara
  permanente. Anotado explícitamente porque la lectura contraria es fácil y destructiva.
- **Barrido de huérfanos (D-LOG-2=B):** limitado a los que R3 creó — `Chord Palette - Test` y
  sus 4 progresiones muertas. El resto de la curación sigue siendo **CSV-6**. Recordatorio
  escrito en `SSoT_Editor_Authoring_Tools.md` §17.12: `ORPHAN` es **directo**, la transitividad
  sigue sin marcarse.
- **Dos asks nuevos a MidiGenPlay:** **MGP-CHD-ASCII-1** (¿el marcador `chd:` es contrato
  ASCII-puro o UTF-8?) y **MGP-LOG-VERBOSE-1** (partir `logGenerator`: hoy es **un solo bit**
  que contiene a la vez `[MelodySlot]` —una línea por nota— y `[ChordTrack] Tonality`, de la que
  dependen tests del host; no se puede silenciar el ruido sin perder el observable). Ambos en el
  nuevo registro de asks abiertos, boundary §8.9.

## 2026-08-08 — `snapshot-01`: qué contiene el tag (D-LOG-4=C)

**Lifecycle.** El tag `snapshot-01` es **ordinal, no descriptivo** (D-LOG-4=C): el nombre no
dice qué hay dentro, y esa es la decisión — **el contenido vive aquí, en el changelog**, no en
el nombre del tag. Un tag descriptivo envejece mal (describe la intención del día que se cortó,
no lo que acabó dentro) y obliga a renombrar historia cuando la descripción deja de ser cierta.

**`snapshot-01` (cortado 2026-08-08) contiene:**

- Todo el demo cut hasta **S5i**, sin cambios en el live front.
- **Campaña RosterExpansion: R0, R1, R2 (+R2c, +R2d) y R3 cerrados.** R3 incluye las dos comps
  de Zig (Rise Up, Showtime), **JAM-1** y **JAM-2** en `CompositionSession`, la poda de
  `Chord Palette - Modal` (7→5) y las tres cartas Wormus de banco de dev (`flags=None`).
- **CTX-2a** (default de tempo del modelo a `Slow` + override de tempo en Dev Mode) y **CTX-2b**
  (override de articulación vía clon de bundle en runtime).
- **LOG-1** completo: niveles de log, etiqueta de acorde reparada, trazas de pila desactivadas,
  `[F-4]` retirado, huérfanos de R3 barridos.
- **Excepciones al freeze de baseline S5i:** tres precedentes autorizados (D-R3-6, D-R3C-1=C,
  D-R3C-8=A).

**Qué NO contiene:** R4+ (starter-v2, finisher, lotes que tocan tutorial), la medición de
**D11b**, la resolución de **D-R4-1**, y las cuatro asks abiertas a MidiGenPlay.

**Efecto de puerta:** con el tag cortado, **R4+ queda DESBLOQUEADO** bajo D-SEQ-3=A (la puerta
es el tag snapshot, no el tag de demo-cut S5j).

## 2026-08-08 — R3 CERRADO: Zig composition cards + JAM-1 / JAM-2 (continuidad de armonía compartida)

**Semántico + operacional + lifecycle.** Cierra R3, el último enabler interleavable de la
campaña RosterExpansion. Todas las verificaciones PASS: ST-A1..A7 · ST-B1/B2 · ST-C1 · C5 ·
ST-R3-11 · C4 · ST-J1..J6.

- **Contenido.** Catálogo Cantante: **Rise Up** (patrón verbatim de 8 compases por grado,
  adaptativo a raíz y a modo) y **Showtime** (ruta procedural, operativa desde ST-R3-11 PASS),
  ambas StarterDeck, 1 copia. **`Chord Palette - Modal` podada 7 → 5** (D-R3C-5=B: dos entradas
  Skel salen de la paleta modal). Tres cartas **Wormus** —Test, Modal y Bossa— creadas con
  `flags = None`: banco de dev, invisibles para el jugador y ausentes del reward pool **por
  construcción**, no por filtro (D-R3C-6=A). C5 lo verifica con un gig completo: ni Cantante ni
  cartas Wormus en mazo ni en pool.
- **JAM-1 — la jam conserva su armonía.** Tercer readback (`LastSharedProgressionData`),
  publicado también en replay de bundle y guardado en la entrada de caché. La progresión se
  impone sobre la pista de Backing **salvo** que la tonalidad se haya movido desde la captura
  **o** el bundle de Backing adopte (D-R3C-2=A: la carta que mueve la tonalidad gana). Captura
  post-render, saltada cuando la fuente es `CardOverride` (D-R3C-4=B′: no capturar la armonía
  de la propia carta de Backing, y limpiar la entrada).
- **JAM-2 — el modo viaja con la armonía** (D-R3C-8=A, arreglado en lote como respuesta a
  F-JAM-SCALE-SPLIT). El defecto: JAM-1 guardaba la armonía junto a una instantánea de
  tonalidad tomada del **modelo de UI**, pero la adopción nunca llega al modelo, así que una
  progresión lidia quedaba etiquetada "Ionian" y al imponerse el Backing sonaba en sus acordes
  autorados mientras el resto de la parte se generaba contra otra escala. **Dos mapas, a
  propósito:** uno sigue el modelo y responde *"¿movió el jugador la tonalidad?"* (la guarda
  que decide **si** imponer); otro sigue el render y responde *"¿en qué modo sonaron estos
  acordes?"* (la carga útil que **se propaga**). Fundirlos haría que la guarda se comparase
  consigo misma y dejase de detectar movimientos de tonalidad reales — ST-J3 es su regresión.
- **Corrección de premisa: D-R3C-3 pasa a A′.** La decisión —*la carta es el modo*— sigue en
  pie; lo que era falso es la premisa escrita de que "la tonalidad de la parte persiste hasta
  que una carta con autoridad tonal la mueva". Eso es cierto del **modelo**, no de la
  **adopción**: la adopción muta el `PartConfig` por render y nunca alcanza el modelo. Se
  registra como corrección explícita, no como reemplazo silencioso.
- **Arquetipo articulation-only, verificado en juego:** misma progresión, 14 → 76 notas,
  timeline de acordes idéntico byte a byte. Diez figuras autorables con cero código.
- **Nota de determinismo (F1).** F1 cambió la secuencia de sorteo del RNG de melodía: **todo
  render con seed pineado anterior a MGP-MEL-1b deja de ser comparable en la pista de
  melodía.** Afecta a la medición de D11b. Corolario de JAM-1/B′: un test de determinismo debe
  arrancar de canción nueva, porque una parte que impone re-pinea su armonía cada render y
  mantiene la caché puenteada mientras dure la cadena.
- **Abierto tras el cierre:** **D-R4-1** (¿el público juzga la tonalidad autorada o la que
  suena? `LoopFeedbackContext` se construye desde el modelo ⇒ bajo armonía modal la audiencia
  ve Ionian; diseño, no defecto) y la verificación auditiva de melodía sobre parte modal, no
  alcanzable con el contenido actual.
- **Baseline S5i:** segunda y tercera excepción autorizada al freeze (D-R3C-1=C, D-R3C-8=A).
- Autoridad: `SSoT_Runtime_CompositionSession_Integration.md` §13 ·
  `SSoT_Card_Authoring_Contracts.md` §5.17/§5.18 · `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.8.

## Milestone index of the archived history (2026-03-18 → 2026-06-22)

Navigation only — dates + labels. Full entries are in
`archive/changelog-ssot_2026-03-18_to_2026-06-22.md`.

- **Demo-cut close — S-sequence + audio + tutorial (2026-06-12 → 2026-06-22).** S5a Vibe
  delivery + transparency (06-22) · S5b card clarity + animation (06-20) · S4 first-time
  tutorial controller + guided jam (06-17) · DOC-HYGIENE CURRENT_STATE §1 prune (06-16) ·
  TUT-JAM-SEQ guided-jam design (06-16) · audio stream: AUDIO-CHAR-PROFILES 1/2, AUDIO-AMBIENCE,
  AUDIO-OST, AUDIO-SFX-FIX, M-AUDIO-MIX + `SSoT_Audio` (06-15/16) · S3-audio SFX layer (06-14) ·
  S3a sensory polish visual (06-14) · S2 sensory event bus foundation (06-14) · S1 (B3-slate-F)
  audience reactions ratified + neutral FT (06-12).
- **CE-L1 — LLM-assisted card authoring in the Card Editor (2026-06-11).**
- **Planning reframe + B3 content + Sibi identity + modulation (2026-05-20 → 2026-05-23).**
  Planning-reframe: demo cut S1–S5 + vertical slice S6–S8, standing directives D1/D2/D3 (05-23) ·
  ALWTTT-MOD-DIR-2/3 directional-modulation hint (05-22) · B3-content-cards: BPM (Push It / Half
  Time) + Key Lift modulation (05-22) · planning-reorg doc split (05-20) · B3-content-sibi +
  followup: Sibi voice identity (05-20).
- **Phase B — gameplay loop polish (2026-05-09 → 2026-05-18).** §5.3.5 demo cut prep: GigLauncher
  + SFX→FlatVibe + zero-click entry (05-18) · B3-content-audience + B3-demo-polish: Cool Dude +
  Kid + Indifference + demo encounter (05-17) · planning batches opened: audience pool + demo
  prep + pitch deck + sound directive (05-15) · B2.5 polish refinements + cleanup (05-15) · B2
  polish layer (05-13) · B1 loop simplification + track persistence + UI rework (05-12) · Phase A
  closed / Phase B opened (05-09).
- **M4.6-followup — F-1..F-5 + MB3/MB4 (2026-05-07 → 2026-05-08).** SongOrchestrator IOOR
  defense (F-4 Stage A), action-card inspiration session routing (MB4), inspiration Dev-path
  drift + session-start carry-over (MB3), per-loop draw + canonical AddCurrentInspiration (F-3),
  GigSettings 4-SO refactor (F-2), action-card double-discard (F-1).
- **M4.6-prep — starter deck + editor tooling (2026-05-01 → 2026-05-06).** Cleanup: starter deck
  authoring + Card Editor tooling (05-06) · Gig Setup roster pickers merged (1)/(4) (05-04) ·
  authoring tooling QoL batch (3) (05-03) · UI-fix-A + UI-fix-B inventory viewer (05-02) ·
  per-musician starter deck auto-assembly batch (2) (05-02) · DeckCardCreationService MB2
  catalogue migration (05-01).
- **M4 — Starter Deck Foundations (2026-04-26 → 2026-04-30).** M4.5 bidirectional guaranteed
  draws (04-30) · M4.4 deck contract evolution / card copies (04-29) · post-MVP planning:
  Pending Effects + Tempo-coupled identity (04-28) · M4.3 Earworm first audience-side status
  (04-28) · M4.2 Flow bifurcation + adaptive LoopScoreCalculator (04-28) · M4.1 Fix C1 unified
  Stress + M1 milestone close (04-26) · starter deck axis resolution / repetition→variety pivot
  (04-26).
- **Milestone 1 — Authoring & Testing Infrastructure (2026-04-08 → 2026-04-26).** M1.1 Deck
  Editor polish + M1 close (04-26) · MB1/MB2 dispatch alignment + catalogue split (04-24) ·
  MidiGenPlay expressive-surface reference docs + design maxim (04-24) · M1.5 Phase 3.x Dev Mode
  stat/state editing (04-23/24) · M1.9 card sizing, M1.3b SpecialKeywords, M1.10 detail modal,
  M1.3c stacked tooltips, M1.3a status descriptions (04-20 → 04-23) · M1.8 status icon animations
  + M1.7 hover highlight (04-20) · M1.5 Phase 1/2 Dev Mode (04-17/20) · M1.2 status icon SO
  migration (04-14) · `SSoT_Editor_Authoring_Tools` created (M1.4, 04-08).
- **Project scope broadened Combat MVP → full ALWTTT game project (2026-04-08).**
- **Combat MVP — Phase 4 closure (2026-03-23).**
- **Governance migration + boundary hardening (2026-03-18 → 2026-03-19).** Batches 01–06
  (subsystem SSoTs, runtime + music-integration authority, audience/status/scoring authority,
  encounter structure, final-tree normalization) + ALWTTT ↔ MidiGenPlay boundary hardening
  micro-pass.

---

## 2026-08-05 — R3 (parcial): Zig composition cards

*(Entrada de estado intermedio, conservada por fidelidad histórica. El cierre de R3 está
en la entrada de 2026-08-08.)*

- Catálogo Cantante: +Rise Up, +Showtime (StarterDeck, 1 copia c/u). 7 legacy
  siguen aparcadas (`flags = None`).
- Rise Up operativa y verificada; entregable de banda de 3-4 (mezcla/canal/mute)
  cumplido.
- Showtime bloqueada package-side (MGP-MEL-1 P1/P8) — **desbloqueada el 2026-08-08**.
- **Baseline demo:** Wormus Major/Minor fijan ahora la tonalidad de la parte.
  No-regresión verificada (ST-R3-12).
- **Bug de autoría cerrado.** Rise Up se importó con `trackAction.styleBundle = null`.
  Bajo la regla BASS-1/D4=A, un Track card sin bundle **no crea pista y devuelve `true`**,
  y sobre una pista existente **preserva el bundle anterior**: dos síntomas (la carta no
  hacía nada; luego "se veía pero no sonaba") con una sola causa.
- D-R3-1..6 registradas. D-R3-4 corrige una recomendación previa del mismo lote
  (el sesgo ascendente vive en una sola capa, la directiva `AscendingOnly`).
- Ask **MGP-MEL-1** filtrado (P1..P8).

## 2026-08-03 — CTX-2a: control de tempo en el tab Composition + default del modelo a `Slow`

**Semántico + operacional.** Cierra **F-TEMPO-1** y **D11 (=A)**.

- **Producción (un cambio, efecto global).** El default de `PartEntry` pasa de
  `"Very Fast"` / `TempoRange.Fast` a `"Slow"` / `TempoRange.Slow`, en la
  declaración de los campos, en `EnsurePartAt` y en el fallback de
  `CreateNextDraftPart`. Como la carta de modo por defecto no fija tempo, ese
  default es lo que suena en el demo; con el anterior, los ocho patrones de 8
  compases de CONT-B se atropellaban y la arquitectura 3+1 no se percibía. Home:
  `SSoT_Runtime_CompositionSession_Integration.md` §12.4.
- **Dev Mode.** Sección `Part tempo override (BPM)` en `DevCompositionDebugTab`
  sobre el patrón CTX-1 (stepper ±5/±10 · Apply · Clear-con-restore ·
  `Hold across loops`), más una línea de lectura
  `BPM: resolved=… | model: Explicit=… Range=… Scale=×…`. Escribe
  `PartEntry.absoluteBpmOverride`, que `SongConfigBuilder` ya lee; **cero API
  nueva**. `Clear ALL overrides` pasa a limpiar cuatro familias. Home:
  `SSoT_Dev_Mode.md` §18.13 + §9.18.
- **El hallazgo es de runtime, no de herramienta.** `CompositionSession`
  cortocircuita la resolución de BPM mientras `PartCache.resolvedBpm > 0`, y
  todas las invalidaciones de dev preservan ese valor (`keepTempo: true`): un
  override ingenuo del modelo habría sido **inaudible**. Apply pone el caché a
  0; Clear reescribe el valor pre-Apply, lo que da restauración audible exacta
  sin depender de la reproducibilidad del sorteo de banda. Precedencia
  verificada y ahora escrita: `bpmOverride` > `ExplicitBpm` > `GetBPMFromRange`,
  con `TempoScale` multiplicando después (suelo 40); solo las cartas de tempo
  re-resuelven (`ShouldKeepTempo`). De ahí que override 70 + Push It (×1.5) dé
  **105** — compone, no bloquea (ST-CTX2A-5). Home:
  `SSoT_Runtime_CompositionSession_Integration.md` §8 **invariante 13** (nuevo).
- **Descartado:** `tempoScale` como palanca de la herramienta —
  `AudienceCharacterBase` lo lee como eje de gusto de la audiencia con línea
  base 1.0, así que es semántica de gameplay, no de frontera.
- **Verificación CTX-2b.** `chordExpression` / `arpeggioRate` **no existen en el
  modelo** (solo en `BackingCardConfigSO`, vía import de carta). CTX-2b sale del
  lote y se abre como lote propio con decisión de arquitectura owed
  (**D-CTX2B-1**), registrada en `Design_Composition_Debug_Tab_v0_1.md` §8.
- **Smokes:** ST-CTX2A-1..7 **PASS** (regresión del default · Apply · regresión
  clear/restore · persistencia ≥3 loops · precedencia con carta de escala ·
  pisado con Hold OFF · compilación de producción).
- **Cierre parcial deliberado:** la medición para **D11b** (BPM legible +
  dispersión de `Slow`) se **difiere a post-R3** por decisión de usuario. El
  lote entrega el instrumento de medida; el dato y el ajuste de bandas
  (package-side, MidiGenPlay) llegan después.
- **MidiGenPlay intacto.** No por permiso — el paquete es propio — sino porque
  ajustar la banda antes de medir se calibra a ciegas.

### CTX-2b — override dev de articulación (`chordExpression` / `arpeggioRate`) en el tab Composition

La precondición bloqueante del lote —¿la expresión es determinista del bundle o se
aleatoriza en el composer?— se resolvió como **determinista**
(`SSoT_Composer_Backing_Track` §8.1 campo persistente · §8.3 articulador RNG-free ·
§8.5 centinela `Random` en substream dedicado derivado del seed), lo que mantuvo el lote
como herramienta de **control** y no de observación.

**Plano nuevo (D-CTX2B-1=A):** es el primer override del tab que no escribe un campo que
el builder ya lea —`chordExpression` y `arpeggioRate` solo existen dentro del style
bundle— así que Apply clona el bundle en runtime, muta la copia y la asigna a
`TrackEntry.styleBundle`; el asset nunca se muta. La participación en el hash sale gratis
porque `SongConfigBuilder.AssetKey` usa `GetInstanceID()`, y por la misma razón **Clear
recupera identidad de bytes**: restaurar la referencia original devuelve la clave de caché
original y el render se sirve como `bundle HIT` del array guardado (ST-CTX2B-1).

**Invariante del plano: clon fresco por Apply** — mutar el clon vigente conservaría su
instance ID, el hash no se movería y la caché serviría bytes rancios; la herramienta
habría fabricado conclusiones falsas sobre el composer (ST-CTX2B-3 es su regresión).
**Hold se estrechó respecto a CTX-1b**: re-asserta solo contra reversión al bundle
original; un bundle ajeno (carta nueva) gana siempre, porque re-asertar reemplazaría la
identidad musical entera de la carta, no dos campos. Ciclo de vida de los clones cubierto
en tres puntos (fin de canción · rebuild de modelo · pisado), verificado sin fugas
(ST-CTX2B-4). D-CTX2B-2=A: consumer-side puro, **sin** ask nuevo por el plano.

**El hallazgo del lote es de frontera: F-ARTIC-RATE-RANDOM-1** — figura concreta +
`arpeggioRate = Random` produce render sin articulación, contradiciendo §8 (rate inerte
para figuras no-arpegio) y §8.5 (substreams independientes), y **sin warning** pese a la
regla "never silent"; filtrado como **MGP-ARTIC-RATE-1**. Mitigación consumer-side: aviso
en UI, sin coerción del valor —una carta real puede autorarse así y el tab existe para
auditar lo que la carta hace de verdad. Deuda derivada abierta: **D-ARTIC-AUDIT** (§4).
Registro retroactivo saldado: **MGP-ALWTTT-ARTIC-1** ya estaba entregado package-side
(boundary §8.7).

ST-CTX2B-1..5+7 PASS; ST-CTX2B-6 (determinismo de `Random` entre relanzamientos) diferido
como test package-side. Autoridad: `SSoT_Dev_Mode.md` §18.14/§9.19 ·
`SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.7 · `Design_Composition_Debug_Tab_v0_1.md`
§3.3 fila 14 + §8 D4.

## 2026-07-31 — CONT-B: pasada de contenido fase B + CTX-1/1b (override dev de tonalidad)

**Qué cambió semánticamente.**

1. **La paleta Modal pasa a ser modal.** Sus 10 entradas anteriores eran
   progresiones mayores con acordes prestados, no material modal, y sin
   restricción de tonalidad — el estado que la spec de fase A registró como
   `F-MODAL-SELFDEFEAT`. Las sustituyen 7 assets: 5 buques insignia
   (`AsAuthored`, `tonalities` de **un solo modo**, cadence `Modal`) y 2
   esqueletos (`DiatonicToPart` + tabla de color, `tonalities` de 6 modos sin
   Locrian). La restricción de tonalidad en los buques es **por diseño**: es
   lo que impide que un vamp dórico se toque sobre una parte que lo desmienta.

2. **`tonalities` vacío es semánticamente distinto de restringido.** Los 11
   assets Core con política `DiatonicToPart`/`DiatonicToPartFunctional` se
   dejan con la lista **vacía** (= sin restricción). Una lista de un solo modo
   sobre política DTP deja la política **inerte**: el remapeo sería siempre
   identidad. Es la misma clase de contradicción que `useColorTable` marcado
   sobre `AsAuthored`. Regla derivada: *política que traduce ⇒ lista abierta;
   política que se planta ⇒ lista cerrada.* (D5 / D3.)

3. **La marca SECDOM es declaración + seguro.** `Prog_Maj_Ragtime_SECDOM` es
   el único asset con marcas por evento. Como la validez de una dominante
   secundaria depende del modo (en eólico el ii es disminuido, en frigio el V),
   se restringe a `Ionian, Dorian, Lydian, Mixolydian` — los cuatro donde la
   cadena de quintas sobrevive entera. (D5b.)

4. **Dominantes estructurales ≠ dominantes funcionales.** `Prog_Maj_Blues8c`
   se autora **sin** marcas SECDOM pese a estar lleno de acordes de séptima
   dominante: en el blues la séptima es el idioma, no una flecha. Marcarlas
   sería mecánicamente válido y musicalmente falso (ML-7).

5. **Veredicto A3 = CONFIRMADO.** El `iv7` de `Prog_Min_BluesSoul_i7iv7` y
   `Prog_Min_iiø7V7_8c` parsea como **Dominant7** (el sufijo pelado gana al
   caso) y la escucha confirma que es la sonoridad deseada. No se corrige.

6. **Contenido local consolidado bajo las raíces de escaneo.** Los 44 assets de
   progresión viven ahora en `Patterns/Chords`. Esto **probablemente resuelve
   D-CSV-14** (el desajuste de raíces era exclusivamente Assets-side desde
   CSV-4) y con ello **dissuelve D-CSV-13**: el dropdown de Backing del Dev
   Mode ya ofrece contenido real. Confirmar formalmente al aplicar (§4).

7. **CTX-1/CTX-1b — el contexto musical de la parte es auditable sin autorar
   cartas.** Ver `SSoT_Dev_Mode.md` §18.12.

**Validación.** 29 tests de escucha, 27 PASS. `T4.5 FAIL` (F-KIT-1, mapeos de
kit ausentes) y `T4.7` parcial (F-DNB-1, granularidad). Regresiones de paleta y
de retirada, todas PASS.

**Desviaciones de la spec de fase A** (para devolución package-side): D3, D5,
D5b, D6, D8, D9 — detalle en `CONT-B_Returns_MidiGenPlay_2026-07-31.md`.

---

## 2026-07-31 — AUTH-1: Effect Editor + card-tooling cross-links + Inventory detailed print

Editor-only consolidation of the card-authoring flow, opened from R2/Conito
friction (PartEffects were Inspector-only; the Card Editor assigned but never
created or edited effects; the Inventory print showed no parameters). New
`PartEffectEditorWindow` (§18): TypeCache-discovered PartEffect family, inline
default-or-custom inspector, Create defaulting to `_PartEffects/`, Duplicate,
Delete with a `CompositionCardPayload.modifierEffects` usage scan, Find Usages,
and Export JSON whose `usedBy` array gives the family its first card → effect
reverse index. New `CardAuthoringNav` (§19) strip across Card Editor / Effect
Editor / Card Inventory / Deck Editor, with context links (Inventory Edit →
`CardEditorWindow.OpenAndSelect`; Card Editor `modifierEffects` shortcut rows →
`PartEffectEditorWindow.OpenAndSelect`; "New Effect…"). `CardInventoryWindow`
detailed print (§8.7): per-card parameter dump for Action (timing, effects with
amounts and targets) and Composition (primaryKind, track role + style bundle +
depth-1 bundle-field dump over MidiGenPlay-owned SOs, part action, modifiers
with scope/timing/label, effects), nested under catalog entries; Detailed-OFF
print and JSON export byte-preserved.

Decisions: **D-AUTH1-0** (new window ratified rather than growing the 2.7k-line
Card Editor) · **D-AUTH1-1=A** (embedded payload panel + shortcuts; no payload
window) · **D-AUTH1-2=A** (PartEffect assets only; `CardEffectSpec` editing stays
in the payload panel, since specs are `[SerializeReference]` payload data with no
asset identity) · **D-AUTH1-3=A** (static nav utility, not a navigator window) ·
**D-AUTH1-4=A** (mode-conditional `InstrumentEffect` fields as a `[CustomEditor]`,
so Inspector and Effect Editor cannot diverge). **Family relabelled D-AUTH1-\***
— `D-AUTH-1` / `D-AUTH-2` remain the S5g music-variety locks; the collision was
caught at AUTH-1 open.

**MC-AUTH1-1..10 PASS** first pass (MC-10 = regression on plain print + JSON
export; 30 PartEffect assets, 47 CardDefinitions enumerated). **AUTH-1b**
followed from three verification observations, all editor-presentation only:
truncated list names with unused window width (→ draggable splitter, expanding
columns, tooltips, filtered `N / total` counter), the always-visible
`melodicInstrumentPool` (→ D-AUTH1-4=A), and a requested Effect Editor export
(→ §18.8). **MC-AUTH1b-1..9 PASS.** Deck Editor validation errors observed
during MC-AUTH1-9 were confirmed expected (`DeckValidationService` on an empty
staged deck), not a regression.

Verification finding recorded at open and re-confirmed at close: the
**R0 / R1 / R2 Roster-Expansion doc stack is still unapplied** (four markers
checked; see the AUTH-1 diff package §0.1), so AUTH-1 makes no claim about R2's
documentary state. Runtime, the JSON/LLM import contract, and shared `test_*` /
`MVP_*` assets are untouched. Primary home: `SSoT_Editor_Authoring_Tools.md`
§3 / §4.11 / §5.8 / §8.7 / §13 / §14.9 / §15 / §18 / §19.

---

## 2026-07-31 — ROSTER-XP R2 + R2c + R2d: Conito enablement, shared-harmony resolution, `InstrumentEffect.RandomFromList`

**Type:** lifecycle/migration-integrative + semantic + operational. Second interleavable enabler of the
Roster Expansion campaign (D1=C), out of the demo roster. **R2c/R2d are the first campaign batches with
code in the build**; both surfaces are BC-gated and the demo config leaves the default-harmony palette
unassigned, so the S5i baseline is unperturbed by construction (ST-R2d-4 + ST-R2d-7 PASS).

**R2 (authoring only):** Conito profile (`Bass` backing / `Guitar` lead → `InstrumentRules` routes
`Bassline` off the backing list; 6-bass melodic whitelist for the dev pickers), two `InstrumentEffect`
assets, three cards imported and catalog-registered in `Conito_CardCatalogData` (`starter_finger_bass` =
`ArpeggioUp`/`PerBeat`/`ChordToneWalk`; `starter_slap_bass` = `Offbeat` + `SelfPocket`;
`starter_static_rush` = `DrawCards(2)` then `ModifyStress(+1, Self)`), each `StarterDeck |
UnlockedByDefault | copies 1`; the 10 legacy test entries **removed from the catalog, assets kept**
(D-R2-8=B — diverges from R1's flags→`None` precedent, recorded not normalized). The three MidiGenPlay
bass-fidelity asks (§8 #1–#3) were **resolved package-side 2026-07-28 before ALWTTT filed them**
(boundary §8.4; incl. the naming trap `BassUpperSplit` ≠ `Bossa`). **The first import of the R2 JSON
batch failed** because `modifierEffectNames` referenced two `PartEffect` assets that did not yet exist;
the importer failed at **staging**, before writing any asset, so no orphan cards, payloads or bundles
were left behind (verified by code path and inspection) — the `PartEffect` assets are a **hard
prerequisite** of the import, not a follow-up.

**R2c:** **ST-R2-1's failure was a bad test, not a bug** — a bass-only part has no harmony publisher,
so silence was correct behavior under the documented contract; the test was replaced (ST-R2c-1) and the
finding turned into the SOLO-1 adoption: MidiGenPlay's `GenerateSinglePart(..., defaultProgression)`
seam wired into `MidiMusicManager.RenderSinglePart`, sourced from a serialized
`ChordProgressionPaletteSO` (**D-R2-6=B**, weighted pick, asset never mutated), seeded from
`(songSeed, partIndex)` — stable within a song, varied across songs. Plus
`InstrumentEffect.RandomFromList` (**D-R2-7**): append-only 4th mode + `melodicInstrumentPool`,
resolved **once per card application** before the track loop and persisted as an ordinary
`overrideMelodicInstrument`, inheriting hash participation, cache coherence and supersession rules for
free; `UnityEngine.Random` on purpose (a card play is not seed-reproducible); empty pool = warn + no-op.

**R2d:** live gig exposed **F-BASS-ORDER-1** — bass card played before the backing card ⇒ the bass read
the shared-progression cache before its only publisher wrote to it ⇒ permanent silence for that part.
ALWTTT declined to fix it by reordering its own track list (**track list order is consumer identity**,
now a package contract) and filed **MGP-ALWTTT-BASS-ORDER-1**, delivered same-cycle: Backing-first
composition with list-order merge, a rewritten guard (host default discarded only when the Backing row
actually *carries* harmony), a normative five-level precedence, and a `sharedProgressionSource`
readback. ALWTTT adopted all of it, retired its `!hasBacking` proxy, and replaced the cache token with a
**pre-render shared-harmony identity** `dp:` + `bk:` (**D-R2-10=A** — the readback-based rule is not
implementable: the readback is produced by the render, and the key decides whether a render happens).
The `bk:` segment closed **F-HARM-STALE-1**, **latent since B1 and unrelated to SOLO-1**: swapping the
Backing card changed the *Backing* track's hash but not the bass's, so the bass stem replayed from cache
with the old chords baked in — silent wrong output, found only because F-BASS-ORDER-1 forced an audit of
what `partMeterHash` must represent (worth recording as an argument for auditing cache-key composition
whenever a new render input is introduced). Also filed and delivered: **MGP-ALWTTT-BASS-SLAPFIG-1** →
`PocketCouplingMode.SelfPocket`, autonomous slap/pop that reads no other track and therefore does
**not** arm the §8.4 cache duty (`SlapPocket` still does); Slap Bass v1 re-authored onto it
(**D-R2-11**), and `InstrumentEffect_SlapBass` moved to `RandomFromList {Slap Bass 1, Slap Bass 2}`
(**D-R2-3** revised).

Decisions **D-R2-1..11**. **ST-R2-2/4/5/6/7 · ST-R2c-1..6 · ST-R2d-1..7 all PASS**; ST-R2-1 superseded
by ST-R2c-1; ST-R2-3 partial (multi-role-on-one-musician variant ST-R2-3b deferred to R5 Overload,
which creates the configuration by construction). Authority: `SSoT_ALWTTT_MidiGenPlay_Boundary.md`
§8.4/§8.5/§8.6 · `SSoT_Runtime_CompositionSession_Integration.md` §8 inv 9 + §11 ·
`SSoT_Card_Authoring_Contracts.md` §5.13. Campaign home:
`planning/active/RosterExpansion_Sub_Roadmap.md`. Doc package
`RosterExpansion_R2_Doc_Diffs_2026-07-31.md` (supersedes the never-applied 2026-07-30 draft) retired on
apply.

---

## 2026-07-23 — ROSTER-XP R1: Captivated + Wink + Cantante catalog cleanup

**Type:** semantic (new canonical status) + operational. First campaign code in the build.
Demo build unchanged and still demonstrable.

**Captivated** shipped as an amplification layer inside the existing `AudienceCharacterStats.ApplyIncomingVibe`
gate — no `CardBase` change was required (B3 had already routed the positive `ModifyVibeSpec` path through the
helper in 2026-05-18) and no `GigManager` tick code was required (`StatusEffectContainer.Tick(AudienceTurnStart)`
handles `LinearStacks` decay generically). While a holder has N stacks, incoming positive Vibe becomes
`round(incoming × (1 + N × 0.25))`; the layer sits strictly **after** the Indifference gate, so
`D-DCP-6=A` ("Indifference blocks ALL incoming Vibe") is preserved unconditionally. SO authored in
`StatusEffectCatalogue_Audience` (`Additive` / `MaxStacks 5` / `LinearStacks` / `AudienceTurnStart` /
`IsBuff = false`) with a new icon; runtime guards on `StatusKey == "captivated"` alongside the
`DamageTakenUpMultiplier` primitive, mirroring the Earworm variant guard.

**Scope broadened vs the frozen design** (`Design_Audience_Status_v1 §4.2` scoped amplification to
`ModifyVibeSpec` positives): the shipped layer is **helper-wide** and amplifies every source routed through
`ApplyIncomingVibe` — cards, `AddVibeAction`, Earworm ticks, the SFX→FlatVibe stage bonus, and the song-end
macro conversion (D-R1-1=A). Tuning moved to `MeterTuningSO.captivatedVibeBonusPerStack` (surfaced as
`GigManager.CaptivatedVibeBonusPerStack`, const fallback on `AudienceCharacterStats`) rather than living on the
stats class, mirroring the Flow→Vibe tuning pattern (D-R1-2=A).

**Content:** **Wink** (Zig, Action, cost 0, `ApplyStatusEffect(captivated, +2, AudienceCharacter)`) authored
into `Cantante_CardCatalogData`. Cantante's 7 legacy starter-flagged entries were set to `flags = None`
(kept, not deleted — reversible, assets preserved; D-R1-4=A), leaving Wink as the catalog's sole starter entry.
Both are unreachable from the demo build: the Cantante catalog is outside the demo band roster and
`SetBandDeckFromMusicians` / `BuildRewardCardPool` are band-scoped — verified empirically by ST-R1-6, which
turns the D1=C interleaving-safety argument from structural into observed.

**Smokes:** ST-R1-1..6 PASS (application · amplification math and rounding, `5 → 8` at 2 stacks ·
decay 2→1→0 with icon clear · Indifference precedence, blocked stays 0 · Earworm amplification `+2 → +3`
confirming helper-wide scope · demo-inertness regression). Decisions **D-R1-1=A · D-R1-2=A · D-R1-3
(`IsBuff = false`) · D-R1-4=A**.

**Documentary side-effects:** `Design_Audience_Status_v1` becomes **fully superseded** (its last active
section, §4, migrated); `SSoT_Status_Effects §5.7` corrected — the Earworm tick has routed through
`ApplyIncomingVibe`, not `AddVibe` directly, since B3, and the doc had carried the stale claim since then;
`SSoT_Audience_and_Reactions §10` gained Indifference, which had been live since B3 without ever being listed.

Doc package `RosterExpansion_R1_Doc_Diffs_2026-07-23.md` — proposed, apply pending.

---

## 2026-07-23 — ROSTER-XP R0: starter deck v2 design closed (D-R0-1..12)

**Type:** semantic + operational + structural. Design batch; **no code, no gameplay change,
no smoke tests owed.** Live demo front (S5i → S5j on starter v1) untouched.

R0 of the Roster Expansion campaign closed. Produced `planning/active/Design_Starter_Deck_v2.md`:
a 4-musician identity map with tempo lean (closing the v1 §"Tempo-lean" placeholder — C2 the
tempo-shifter, Sibi slow/hypnotic, Conito fast/opportunistic, Zig build-and-drop), a **22-card /
18-unique** starter on a symmetric kit shape (2 composition + 1 action + 1 finisher per musician,
plus 2 generics), a fully-populated **finisher layer** (Spotlight / Psychic Wave v2 / Overload /
Double Harmony — one unique mechanic each, closing D-ECON-6=DEFER's designation gap), and a
17-slot reward slate with soft paths per D5=A.

Decisions **D-R0-1..12** locked (ledger: sub-roadmap §2). Notable: **D-R0-2=B** inverts
D-STARTER-2=B — Compound Cycle (6/8) is promoted to starter and Waltz Protocol (3/4) demoted to
reward pool (flags-only, 1:1 swap); **D-R0-5=A** puts Overload in the Action domain so it survives
the final-loop composition lock (inv 11), with a rider recording that explicit Voltage generation
needs no new spec (`ApplyStatusEffectSpec` over an SO-catalogue counter status); **D-R0-6=B**
moves Push It / Half Time / Key Lift / Singing Field to the reward pool, which also defers the
two-melody texture (Sibi keys hook + Zig sung melody) to an earned reward state; **D-R0-8=A**
reserves the +INSP-per-level hook so the economy lever is not duplicated against Vamp / In the
Pocket mid-S5i.

Verifications resolved by code read (method recorded in v2 §7): **V1** structurally verified
(full quality alphabet reaches voicing at both backing render sites and the melody chord-tone
path; interval-table audit deferred to R7 smoke) · **V2 FAIL** — `Tonality` is the seven diatonic
modes only, absorbed by an ALWTTT-side fallback (Andalusian progressions with explicit per-event
qualities over Phrygian), **no ask triggered** · **V3** resolved (authored melody loops tile by
raw beats to part length, meter mismatch warns, D-MEL5.1=A) · **V4** — the recorded gap was
**partially stale**: backing honors `degreeAccidental` on both render paths, bass does not; the
diatonic-root constraint stands for band-composition reasons and MGP ask §8 #4 narrows to
bass-side parity · **V5** structurally verified (shared target list; `AllAudienceCharacters`
excludes `IsBlocked` members — relevant to Psychic Wave v2's Earworm).

`Design_Track_Card_Levels_v0_1.md` → **v0.2**: §7 open questions resolved (per-part lifetime, max
3, discard-on-replace, level-up = normal composition play, badge + floater UI, no Action-card
levels), §6 +INSP reserved, §3 corrected with the V1/V4 outcomes.

Sub-roadmap updated: R0 row closed, R4 row expanded (Read the Room + Keep Cool retarget + V5
smoke), coverage map completed (Zig's two UNDEFINED reward slots filled with Torch Song /
Motor Mouth), §9 marked resolved, §10 collapsed, ask #4 narrowed, R1 rehydration prompt swapped in.

Classification: semantic (Levels spec closure, card-list design truth) + operational (campaign
batch state) + structural (new planning doc registered in `SSoT_INDEX` + manifest). **No SSoT
authority moved.** Applied by batch `ROSTER-XP-R0-DOC`; `RosterExpansion_R0_Doc_Diffs_2026-07-23.md`
retired on apply.

---

## 2026-07-23 — ROSTER-XP planning: Roster Expansion campaign consolidated (R0–R8)

**Type:** reference-only + operational/roadmap. Planning session; **no code, no gameplay
change, no smoke tests.**

Feasibility evaluation of the 4-musician starter redesign (spec: per musician 2 identity
composition + 1 identity action + 1 unique-mechanic finisher, plus 2 soft-path composition
rewards + 1 status-carrying action reward) closed against the live baseline. Everything is
ALWTTT-implementable except bass fidelity items — chord-tone walk (recorded package-side
candidate), pocket-coupling (new cross-track), bossa split (CA-T2 deferred) — and
chord-aware melody resolution (tapping reward): all queued as MidiGenPlay asks, filed with
acceptance criteria at R2/R8 (never redesigned ALWTTT-side). Alphabet verifications:
`RomanProgressionParser` carries Dominant7/Major7/Minor7/HalfDiminished7/Diminished7/Sus2/Sus4
(Track Card Levels lvl-3 exemplar expressible; slash inversions are not — voicer owns
inversions); `MelodyPatternData` is degree-based (tonality-adaptive, not chord-aware).

Campaign decisions **D1=C** (R0–R3 interleave with S5i; R4+ post-S5j tag — live front
unchanged) · **D2=A** (reuse existing card baseline: Wormus pair, Default Mode, Keep Cool,
Psychic Waves-extended) · **D3=A** (Conito bass v1 approximations + asks) · **D4=A**
(Double Harmony Tier A now, sung Tier B behind the deferred cap=2 validation) · **D5=A**
(soft reward paths, no exclusivity mechanic) · **D6=A** (single consolidation home).

New docs: `planning/active/RosterExpansion_Sub_Roadmap.md` (ledger + R0–R8 + verdict
table + asks + R0 rehydration) · `planning/active/Design_Track_Card_Levels_v0_1.md`
(re-play = level-up mechanic, batch R7; +INSP/complexity hooks flagged against
DF-INSPLOOP overlap and the S5i-owned inert complexity term) ·
`planning/Design_Fill_Window_v0_1.md` + `planning/Design_Singer_Expression_Input_v0_1.md`
(registered ideas, post-campaign). Roadmap Roster Expansion section repointed
(prerequisites updated: bass ✅, `ApplyIncomingVibe` ✅, Captivated → R1);
`CURRENT_STATE §3` pointer added; manifest + SSoT_INDEX rows added.
Applied 2026-07-23 by batch ROSTER-XP-DOC; `RosterExpansion_Doc_Diffs_2026-07-23.md`
retired on apply (package convention).

Classification: reference-only + operational (roadmap/structure). No SSoT authority moved.

---

## 2026-07-22 — CSV-3: R2a card debug-play + resolved meter/tonality surfaces + melody finding closed

Adds `CompositionSession.DevInjectCompositionCard` (catalogue card's musical side only — live model D-CSV-8=A, economy-neutral D-CSV-24=B: injected tracks excluded from `EvalPerLoopInsp`, reclaimed by a genuine play, cleared at song boundary) and the `MidiMusicManager.LastRenderResolved*` read line (sibling of BAL-1 CC7, replay-faithful D-DBG5=A). D-CSV-13=A (Backing dropdown stays `PatternRepositoryResources`-fed + notice). D-MEL-1=A (a rhythm card owns meter via a `MeterEffect`; meter is a model-construction `FourFour` default). **Melody-path finding CLOSED as not-a-bug** — meter collision by construction (Core Minor holds zero 6/8 progressions); runs A/B no divergence; ST-CSV3-6 confirmed C2a healthy; **no package ask** (`MGP-ALWTTT-MEL-ORDER-1` not filed). Overlay outer-scroll fix. `SongCompositionUI` gained `CanApplyDefinition` / `ApplyCardDefinitionToPart` cores (mechanical extraction, production byte-identical). Integration §12 rewritten as CLOSED with a location correction (the `?? default` is the audience/`LoopFeedbackContext` path, not the render path). Smokes ST-CSV3-1..9 + 5b/5c PASS. Gate `#if ALWTTT_DEV`. CSV-4 listening pass UNBLOCKED. Classification: semantic + operational + lifecycle. Homes: `SSoT_Dev_Mode.md` §18.1/§18.6/§18.10/§18.11/§9.16 · `SSoT_Runtime_CompositionSession_Integration.md` §8 inv 9 / §12 · `SSoT_Card_Authoring_Contracts.md` §5.16 · boundary §4.3 · `CURRENT_STATE.md` §2/§4 · `coverage-matrix.md`. Doc-pass batch: CSV-3-DOC.

---

## 2026-07-22 — BAL-1: consumer-side mix gains (bytes plane) adopted

Adopts **MGP-MIX-1** (delivered in MidiGenPlay 1.2.0) as ALWTTT's second mix
plane: a per-`(musicianId, TrackRole)` **gain** baked into the generated MIDI as
a CC7 event at render time, distinct from and composing with the live per-musician
axis. Contract home: `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.3 (flipped
pending→**ADOPTED**); ALWTTT model: `SSoT_Audio.md` §4.6.

**Task 0 — live-plane collision fix (the prerequisite).** The MPTK read corrected
the mechanism the boundary originally recorded. F-MPTK-1: MPTK resets every
channel's CC7 to 100 on **each** play (`MPTK_Play → MPTK_InitSynth → new
MPTKChannels → fluid_channel_init_ctrl`; `EnableResetChannel` default true) — the
`OnSongStartedInternal` re-assert loop is **required**, cannot be retired. F-MPTK-2:
`OnEventStartPlayMidi` fires *before* tick-0 events, so the baked preamble is the
last writer at song start (the boundary's original "re-assert overwrites the baked
CC7" direction was inverted). Fix (D-BAL-6=B + D-BAL-7): all musician-channel live
writes route through one boundary `WriteChannelVolume01` that **composes**
multiplicatively with the baked gain (`live01 × bakedGain × 100/127`; identity in ⇒
identity out), and the re-assert is **deferred** past tick-0 (`ReassertLiveMixAfterPreamble`,
waits `CurrentTick > 0`). The loop is kept, deferred, composed.

**Delivered.** `MixGainProfileSO` (content SO, keyed `(musicianId, TrackRole)`,
default 1.0, Rhythm warn+ignore); gig-start resolution (`GigManager.StartGig →
SetGigMixGains`, D-BAL-8=A serialized field); `mixGains:` threaded into the per-part
`GenerateSinglePart` call (gig loop only; `GenerateSong`/jam/menu stay ungained);
gain folded into `ComputeHashFromTrackEntry` → stem + bundle keys (D-BAL-3=A: gain
enters the hash regardless of lifecycle, cache replay can never serve stale CC7);
`appliedCc7ByTrack` readback (incl. bundle-cache replay) + a live-composed CC7
Dev-Mode strip (`GetLiveComposedCc7`).

**Decisions.** D-BAL-1=C (dedicated content SO) · D-BAL-2=A (hand-authored ensemble
intent) · D-BAL-3=A (fixed per gig, hash-participating) · D-BAL-4=A (content data,
not player save) · D-BAL-5=A (no drum ask) — all locked at batch open; D-BAL-6=B
(multiplicative live×baked compose) · D-BAL-7 (keep-defer-compose the re-assert) ·
D-BAL-8=A (GigManager serialized field) — locked at resolution.

**Smoke ST-BAL-1..7 all PASS on 1.2.0.** (1) byte identity ungained · (2) gain=1.0
audible identity, CC7=100 · (3) gain=0 mute-without-delete · (4) plane composition
~0.25 composed / ~50 at live 1.0 (the D-BAL-6=B proof; verified numerically via the
live-composed CC7 strip) · (5) start-race F-MPTK-2 regression (persisted balance holds
every song) · (6) Rhythm warn+ignore · (7) cache replay after gain change. Anti-
compensation rule normative (gains ≠ per-patch loudness correction; that is package
`volume01` / D-MIX-6). MidiGenPlay package untouched (consumed the 1.2.0 surface only).

Classification: semantic + operational + lifecycle. Authority:
`SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.3 · `SSoT_Audio.md` §4.2/§4.6 ·
`SSoT_Runtime_CompositionSession_Integration.md` §8 inv 9 · `CURRENT_STATE.md`.

---

## 2026-07-21 — SINGER-1: Pink Trombone articulatory singer integrated

New primary-authority SSoT `systems/SSoT_Singer_Voice.md`. The Pink Trombone
voice (POC, research-class verdict) is promoted into ALWTTT as an optional singer
for the melody track, scoped to the singer character (Zig, `musicianId="3"`).
It sings a musician's `Melody`/`Lead` **stem** (already returned by
`RenderSinglePart`, §8 inv 9) and mutes that channel via `SetChannelVolume`;
unmatched melodies play as GM MIDI (coexistence by construction). Transport
resolved D1=A (dsp anchor at `OnSongStarted` + per-profile `startTrimMs`; offset
constant to one DSP buffer, not drift). Budget: 1 active / 2 hard ceiling.
One `[SINGER-1]` seam added to `CompositionSession` (`LoopPlaybackStarting`
before `PlayRaw`); `MidiMusicManager` and MidiGenPlay unchanged. Fork relocated to
`Assets/ThirdParty/PinkTrombone/` (MIT, POC-FORK 1–7). `PinkTromboneBackingPlayer`
+ original `PinkTromboneSinger` retired. ST-V1..V8 PASS. Docs: new SSoT +
`SSoT_Runtime_CompositionSession_Integration` inv 12 + `SSoT_Audio` pointer +
INDEX/coverage rows. `PinkTrombone_Voice_Levers.md` relocated to `reference/`.
Open (not blocking): D4=A profile field on `MusicianCharacterData`; Highlight×mute
and cap=2 second-voice deferred to Dev Mode; mixer routing follow-up.
Classification: semantic + structural + authority + lifecycle.

## 2026-07-20 — CSV-4 (PARTIAL CLOSE): asset cleanup, MGP-BAGGAGE-1 resolution, content-standard decisions

Content + cross-boundary batch of the **CSV** arc. **No code change, no gameplay change,
no smoke tests** (documentation and content only). **CSV-4 closes PARTIALLY** — the
listening pass and the naming application are deferred out of it. No document records it
as a full close.

### Content deleted (ALWTTT-side, `Assets/`)

**Worklist A — 12 local orphan assets**, exactly as specified in the pre-replacement
sub-roadmap §4.1.1 (6 chord progressions, 6 drum patterns):
`Prog_Ionian_FourFour_4m_0_4_0_0-…` · `Prog_Ionian_FourFour_4m_0_4_5_1-…` ·
`Prog_Ionian_ThreeFour_4m_0_3_3_0-…` · `Untitled` · `Untitled 1` (all five in
`Assets/Resources/ScriptableObjects/Patterns/Chords/`) · `1 chord 1 measure`
(`…/Chord Progressions/Tests/`) · `Drum_4-4_1m_BDSN` · `Drum_4-4_2m_CHBDSNOH` ·
`Drum_6-1_2m_CHBDSNOH` · `LLMTest` · `TestSmokeSMR7` (all five in `…/Patterns/Drums/`) ·
`DrumPattern-DefaultFourFour` (`…/ScriptableObjects/Drum Patterns/`). Deleting the first
five emptied `Assets/Resources/ScriptableObjects/Patterns/Chords/`, which was removed
with them.

**5 local style bundles** — test content, not starter: `Backing Card Config [I – IV – V – I]` ·
`Backing Card Config [I – vi – IV – V]` ·
`2_Composition_001_CompositionPayload 1_Backing_StyleBundle` ·
`2_Composition_001_CompositionPayload_Backing_StyleBundle` ·
`2CBacking001TestProg_Payload_Backing_StyleBundle`.

**One asset authored:** `Test_Scale_Melody_4-4_4m_14n`, in
`Assets/Resources/ScriptableObjects/Patterns/Melodies/` — the correct (plural) scan root.

Verified against a fresh Export All: counts dropped exactly as predicted, no LIVE asset
lost a reference, no LIVE asset gained `ORPHAN`.

### Three export baselines — which are void

`230` (CSV-1c corrected) → `218` (post-worklist-A) → **`183` (post-MidiGenPlay-1.1.0)**.
**Only the 183 set is current. The 230 and 218 sets are superseded and must not be
reused.** The pre-fix 181-asset export was already void.

### MGP-BAGGAGE-1 — filed, answered package-side, adopted (same day)

ALWTTT measured 28 package assets that no consumer referenced and that in most cases
could not render. ALWTTT cannot delete them (edits under `Packages/` revert on update;
D-CSV-7=A), so the finding was filed as an ask. Package-side answer: none of the flagged
assets was intentional — no runtime fallback, no editor template, no test fixture.
**33 assets retired, 8 moved** to `Samples/ExampleCatalogue/ChordProgressions/`, taking
them out of `Resources.LoadAll` reach. Package-side additions to ALWTTT's list:
`Test Progression`, `Melodic Style - Test 1`, and the three `_*List` containers (kept but
emptied, package-side D-BAG-4). Consumer re-export verifies `EMPTY` / `NO-LANES` /
`ALL-SILENT` / `OVERFLOW` at **zero** across package-origin assets; gig smoke green.
**Standing rule adopted:** any reappearance of those four flags on a package-origin asset
is a package-side regression warranting a new ask, not consumer tolerance. Recorded in
`SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.2.

**Poly Synth correction — our export was faithful; the asset was mis-authored.** The
package handoff §4.1 attributed the `Poly Synth` / `Warm Pad` duplicate to an extraction
artifact in `CompositionInventoryWindow`. That does not hold: the window reads
`PatchName` and `PatchIndex` **verbatim** (dup key `SoundFont|Bank|PatchName|PatchIndex`,
four raw fields, no derivation, no 0/1 normalisation), and the 1.0.0 export showed **both**
fields of `Poly Synth` carrying Warm Pad's values. 1.1.0 corrected the asset
(`90 - Poly Synth` / 90, no `DUP`). **No consumer code change is owed**; handoff §7.4 is
closed as resolved package-side, not as consumer debt.

### Decisions locked

- **D-CSV-7 = A** — asset ownership is **location-based**. `Assets/` is ALWTTT's;
  `Packages/` is MidiGenPlay's. ALWTTT never renames or deletes package-side; it files
  asks. **Rider: naming authority ≠ moving authority** — a rename must not change an
  asset's position relative to a Resources scan root (that is D-CSV-14, CSV-5).
- **D-CSV-15 — both mechanisms retained.** Melody is phrase-driven in current card
  content (every `MelodyCardConfigSO` uses `phrasePaletteOverride`), *and*
  `MelodyCardConfigSO.patternOverride` is retained **deliberately** as the landing surface
  for a future MIDI-import path (human-composed DAW melodies → game-readable melody
  patterns). Neither mechanism is deprecated; both authored local patterns are kept.
- **D-CSV-16 = A, pending execution.** The card → bundle reverse index is owed, and it
  moved from *nice to have* to **blocking**: with the test bundles deleted, liveness of
  the Modal and Test palettes could only be established from the user's statement, not
  from tooling. **Scope not yet assigned to a batch.**
- **D-CSV-18 = A** — all 79 instruments are `source: pkg`, so instrument curation targets
  the **pools** (`InstrumentRules` + per-musician whitelists, ALWTTT-owned), never the
  assets. Nothing sounds in the demo without an explicit listening verdict.
- **D-CSV-19 = A** — the renamer is a **separate editor window**, not a mode inside
  `CompositionInventoryWindow`. The inventory window keeps its read-only invariant
  (§17.2 / ST-CSV-7) so it stays trustworthy as the verification surface before and after
  a rename run. Batch label **CSV-4b**.
- **D-CSV-21 = C, then superseded by D-CSV-23.** Listening scope was set to the 14
  reachable progressions (Core Major 8 + Core Minor 6), Test's 4 cut without listening,
  Modal's 10 deferred. D-CSV-23 redirected Modal from *deferred* to *merged*, so D-CSV-21
  is recorded as resolved-and-superseded, not as live guidance.
- **D-CSV-22 = B** — CSV-3 runs before the melody investigation, because R2a is the
  instrument that makes the investigation tractable.
- **D-CSV-23 = A+B** — chord content standard. **(A)** Default progression length becomes
  **8 measures**, matching the 8-measure part, applied to *new and repaired* content —
  **not** a mass re-authoring of the existing 4-measure set. **(B)** The Modal palette is
  dissolved into Core Major / Core Minor by tonic, so each palette carries modal colour
  rather than being restricted to diatonic major/minor. Registered as **CR-10**; executed
  at **CSV-6**, not CSV-4.

### Package-side registry movements

- **D-BAG-3 / MGP-MIX-1 — OPEN.** `volume01 = 1.0` on all 70 melodic instruments is
  unauthored, not deliberately flat. It is a package authoring field and stays there, so
  ALWTTT has **no consumer-side per-instrument gain** and must not edit package assets to
  get one. ALWTTT input delivered 2026-07-20: granularity **per musician** (the model is
  keyed `(musicianId, TrackRole)` end-to-end since BASS-1, and "the bass is too loud" is a
  sentence about a character); composition law **multiplicative** (`volume01 × gain`,
  gain defaulting to 1.0) so package-side loudness normalisation composes instead of being
  discarded; and a consumer-side consequence on the application point — velocity scaling
  changes timbre as well as level with soundfonts, which would invalidate the
  per-instrument listening verdicts D-CSV-18 requires, whereas CC7 leaves timbre intact.
  Not closed by 1.1.0. **Blocks any ALWTTT mix-balance batch.**
- **BASSFILL-1 recalibrated, not withdrawn.** Filed on "27 of 30 live progressions trigger
  `BASS-GAP`". The 8-measure standard (CR-10) extinguishes the flag on most future content
  by construction, so the ask drops from *audible demo defect* to *robustness gap* — still
  a silent failure mode (any later 4-measure progression, or any 16-measure part,
  reproduces it), but it no longer blocks the demo and should not outrank MGP-MIX-1.
  **Preferred remedy if addressed:** a generation-time warning when the progression does
  not cover the part, rather than automatic fill — a progression that ends and leaves air
  can be an intentional musical choice.
- **D-CSV-14 reduced.** The package-side chord-progression Resources root no longer exists
  (moved to `Samples/` in 1.1.0), so `Patterns/{Chords,Drums,Melodies}` are the only
  package-side scan roots and the remaining mismatch is **exclusively Assets-side**:
  local chords under `ScriptableObjects/Chord Progressions/*`, and two local melody
  patterns under `Patterns/Melody` (singular) while the correct root is `Patterns/Melodies`
  (plural) — evidenced by `Test_Scale_Melody_4-4_4m_14n`, authored in the plural folder and
  *not* flagged `OFF-ROOT`. Still CSV-5's; **no longer cross-boundary**.

### Reachability corrections

`Chord Palette - Test` is now `ORPHAN` (its bundle was deleted), so its 4 progressions are
dead. Live backing content is `Backing Card Config - Core Major` (8) and `- Core Minor`
(6); `- Modal` (10) exists but no card references it. **Reachable set = 14 of 33.** Live
drum set is **26, not 27** — `DNB 4-4 2m test` hung off the now-deleted package
`DrumPatternPalette`. Both figures rest on the user's statement, not on tooling (D-CSV-16).

**The `Samples/` count nuance is expected, not a regression.** The handoff §7.2 predicted
the consumer count would drop by the 8 moved assets. It did not, correctly:
`CompositionInventoryWindow` discovers via `AssetDatabase.FindAssets<T>()`, deliberately
broader than `Resources`. The move achieved its purpose — `Resources.LoadAll` no longer
returns them — but the inventory lists them permanently until the window distinguishes
origin. A third `sample` origin is specified for CSV-4b
(`SSoT_Editor_Authoring_Tools.md` §17.6).

### Finding carried out of CSV-4 — evidence classes kept separate

With rhythm 6/8 + Core Minor + Singing Field, the melody follows neither the meter nor the
scale. **This is not classified as a bug and ownership is not assigned; CSV-3 assigns it.**

- *Observed:* the divergence itself, under that combination.
- *Code truth, confirmed:* `default` on the `TimeSignature` enum is `FourFour` (member 0)
  — confirmed package-side in the MGP-BAGGAGE-1 handoff §1, where the same property
  explained why every unauthored package asset reported 4/4.
- *Inferred, unvalidated (1):* `CompositionSession` resolves
  `partEntry?.timeSignature ?? default`, so a part whose meter was never set would be
  silently 4/4.
- *Inferred, unvalidated (2):* `MelodyTrackComposer` derives its scale from
  `part.Tonality` / `part.RootNote` while harmonic context comes from the progression's
  chord events — two independent sources that can diverge.
- *Content fact:* Core Minor holds **zero** 6/8 progressions, so that combination is a
  meter collision by construction.

Recorded ALWTTT-side in `SSoT_Runtime_CompositionSession_Integration.md` §12 (new).
Meter ownership registered as **D-MEL-1** (CSV-3). **Consequence: the listening pass is
blocked** — a verdict issued against a mis-rendering engine blames the asset.

### Deferred out of CSV-4

- **Listening pass (worklist D)** — blocked by the melody finding; reopens after CSV-3.
- **Naming application (worklist E)** — convention drafted as
  `planning/Design_Asset_Naming_v0_1.md` (planning, non-normative); applied at **CSV-4b**,
  together with the `sample` origin classification and, if CSV-5 has not taken it, the
  `Patterns/Melody` → `Patterns/Melodies` alignment.

### Documents changed

`SSoT_ALWTTT_MidiGenPlay_Boundary.md` (§4.3 + new §8.2) · `CSV_Composition_Validation_Sub_Roadmap.md`
(§2 CR-10 + CR-7 rider, §3 ledger + D-MEL-1, **§4.1.1 replaced wholesale**, §4 batch table
+ CSV-4b + blocking note, §5 asks, §8 homes) · `SSoT_Editor_Authoring_Tools.md`
(§17.2, §17.6, §17.7, §17.10, new §17.11) · `SSoT_Runtime_CompositionSession_Integration.md`
(new §12) · new `planning/Design_Asset_Naming_v0_1.md` · `SSoT_INDEX.md` ·
`coverage-matrix.md` · `CURRENT_STATE.md` (§1, §2, §4, §5).

Change classes: **integrative** (package baggage disposition, 1.1.0 adoption, D-BAG-3) ·
**operational** (baseline re-measurement, ask priorities, D-CSV-14 scope) · **semantic**
(chord content standard, silent 4/4 meter default, pool-level instrument curation) ·
**structural** (new naming convention doc + its registration).

---

## 2026-07-18 — CSV-1 + CSV-2: composition inventory window + dev instrument overrides (closed; CSV arc opened)

Opening batch of the **CSV** arc (Composition Session Validation). Tooling only —
no gameplay change, no content change, **no MidiGenPlay file touched**, and no new
production API. The arc is parallel to / behind S5i, which remains the live front.

- **`CompositionInventoryWindow` (CSV-1).** New editor window
  (`Assets/Scripts/DevMode/Editor/`, `#if UNITY_EDITOR && ALWTTT_DEV`) — the first
  structured view of the composition asset inventory in either project. Seven views
  (style bundles · drum patterns · chord progressions + palettes + libraries ·
  melody patterns + phrase archetypes + phrase palettes · melodic instruments ·
  percussion instruments · Names Report), filters (TS / package-vs-local / text /
  orphan / duplicate / flagged / bundle-reachable / editable reference part
  measures), derived health flags, and `Print` + `Export JSON` per view following the
  `CardInventoryWindow` pattern. Reuses the existing read paths
  (`PatternRepositoryResources`, `TrackPatternConfigStoreResources<T>`,
  `InstrumentRepositoryResources`) rather than re-implementing them. Mutates
  nothing (ST-CSV-7).
- **Health flags as a curation worklist.** `BASS-GAP` (progression shorter than the
  reference part) is the **static face of CR-7's "bass ends early"** — the bass
  renders the progression once with no repeat-to-fill
  (`SSoT_Composer_Bass_Track §1`) while the backing tiles. Plus `SHORT-TAIL` /
  `OVERFLOW` / `BPMEAS-MISMATCH` / `NO-LANES` / `ALL-SILENT` / `ORPHAN` / `DUP#n`
  and instrument soundfont/octave/volume checks. Duplicate signatures deliberately
  exclude names and metadata so a rename cannot hide a duplicate.
- **Documentary home: D-CSV-6=A.** `SSoT_Editor_Authoring_Tools.md` §17 (ALWTTT),
  mirroring `CardInventoryWindow`. `SSoT_Authoring_Tools.md` §4 assigns package docs
  to tools that *author or edit* package assets; this one authors nothing, and
  documenting it package-side would have promoted a read-only game-side curation
  browser into package documentation authority.
- **Dev instrument overrides (CSV-2), D-CSV-5=A refined.** Per-track melodic +
  percussion pickers in the Composition tab (`SSoT_Dev_Mode.md` §18.9), siblings of
  the §18.4 pattern rows but a **different mechanism**: the pick writes
  `TrackEntry.overrideMelodicInstrument` / `overridePercussionInstrument` directly.
  Those GUIDs already participate in `trackInputsHash`, so the stem cache stays
  coherent by construction — no separate dictionary, no cache bypass, and
  `SongCompositionUI` / `SongConfigBuilder` / `MidiMusicManager` are untouched.
- **The non-obvious part: invalidation shape.** Assign/clear route through a new
  `CompositionSession.DevInvalidateForInstrumentOverride(partIndex)` that invalidates
  with **`keepInstruments: false`** — mirroring the instrument-**card** path, not the
  `DevOverrideStamp` pattern path. Preserving instruments would retain
  `PartCache.resolvedMelInstByTrack`, which is re-fed into the next
  `RenderSinglePart` as `instrumentOverrides`, letting a stale voice beat the new
  pick. Recorded in `SSoT_Runtime_CompositionSession_Integration.md` §8 inv 9 as the
  deliberate asymmetry against the pattern-override paragraph.
- **Clear/restore + card supersession.** Clear restores the pre-dev field state —
  including a prior *card* override, not merely null — byte-identically under a
  pinned seed (ST-CSV-3); the mechanism is the session pin map, which is *skipped*
  rather than overwritten while an explicit override is set. A later
  `InstrumentEffect` card takes the field back (`ApplyInstrumentEffect` rewrites
  unconditionally); the tab detects this, reports `superseded by card`, and drops its
  record without restoring — card truth is newer.
- **Full catalogue for probing.** Entries outside
  `InstrumentRules.GetPermittedMelodic(musician, role)` are annotated
  `(outside permitted set)` and remain selectable, mirroring the `(off-band)`
  convention of §18.6. Percussion has no permitted rule in v1 and is unannotated.
- **Smokes ST-CSV-1..8 all PASS** (§9.15): BC gate, melodic apply, clear/restore
  regression, percussion routing, card-stomp detection, outside-permitted probing,
  window inertness, production compile.
- **Decision-ID correction.** An earlier draft labelled the window's compile gate
  `D-CSV-7`, colliding with the naming-ownership question already registered under
  that ID in the sub-roadmap. `D-CSV-7` keeps its original meaning; the gate decision
  is **D-CSV-10=A**.
- **Open, carried forward.** **CSV-1b** — palette/library discovery goes only through
  `TrackPatternConfigStoreResources`, which scans only
  `Resources/ScriptableObjects/Patterns/<type>`, while most project palettes live
  elsewhere; the first real export found 1 drum / 1 chord / 1 phrase palette against
  5 / 4 / 2 referenced by style bundles, so `refs` and `ORPHAN` are unverified
  (38/40 drum patterns and 13/13 progressions falsely flagged). Fix is to union the
  scan with `AssetDatabase.FindAssets`; must land before CSV-4 curation.
  **D-CSV-11** — every `ChordProgressionData` reports `Measures = 0` and
  `TimeSignature = FourFour` regardless of asset name, which makes the progression
  length flags uninformative and sits upstream of CR-7's static diagnosis.
- **First-export findings** (181 assets: 114 package, 67 local) recorded in
  `CSV_Composition_Validation_Sub_Roadmap.md` §4.1.1 — drum patterns are the healthy
  family (40 assets, 5 time signatures, zero meter mismatches), the melody-pattern
  family is entirely empty placeholder (12 identical assets; melody is
  phrase-archetype-driven in practice), instrument metadata is uniformly clean
  (70 melodic, one soundfont, `volume01 = 1.0` on all) so CR-4 curation is a
  listening pass rather than a metadata pass, and naming has at least four
  competing schemes plus `Untitled` / `Untitled 1`.

Docs touched: `SSoT_Editor_Authoring_Tools.md` (§3 table, §13, §14.8/§14.9, new §17),
`SSoT_Dev_Mode.md` (header, §6, §18.8/§18.9, §9.15, §10),
`SSoT_Runtime_CompositionSession_Integration.md` (§8 inv 9),
`CSV_Composition_Validation_Sub_Roadmap.md` (batch table, decision ledger, §4.1.1,
§4.1/§4.2 closures), `CURRENT_STATE.md`, `coverage-matrix.md`.

### Rider — CSV-1b + CSV-1c (same day, 2026-07-18): inventory discovery corrected

Two micro-batches closing a reporting defect the first real export exposed. Editor-only,
read-only, no runtime code, MidiGenPlay untouched.

- **CSV-1b — palette discovery.** `TrackPatternConfigStoreResources` scans only
  `Resources/ScriptableObjects/Patterns/<type>`; most project palettes live elsewhere.
  Unioned with `AssetDatabase.FindAssets`. Drum palettes 1→6, phrase palettes 1→3,
  drum orphans 38/40→13/40, archetype orphans 6→0.
- **CSV-1c — pattern + instrument discovery (D-CSV-12=A+B).** Chords did not improve
  under CSV-1b, which diagnosed the same defect one layer down: the in-use progressions
  live under `ScriptableObjects/Chord Progressions/{Major,Minor,Modal,Tests}`, a sibling
  of `Patterns/`, never scanned. Same union applied to patterns and instruments, plus a
  reference harvest over palettes/libraries/bundles. **Chord progressions 13 → 48**;
  orphans 13/13 → 14/48 and the remainder are genuinely dead. `HARVESTED = 0` — the
  AssetDatabase union alone sufficed; the harvest stands as a safety net.
- **New flags.** `OFF-ROOT` (exists, but no runtime repository resolves it) and
  `HARVESTED` (no scan found it; present only via a reference).
- **Export All** — one folder pick writes all seven views; shares `BuildJsonForView`
  with the per-view export, which is unchanged.
- **D-CSV-11 dismissed.** `ChordProgressionData.Measures`/`TimeSignature` **are**
  authored; the universal `0` / `FourFour` reading came from a sample containing only
  dead assets. No package ask.
- **Two findings carried forward.** (1) **All 30 live chord progressions are
  `OFF-ROOT`** — `PatternRepositoryResources` resolves none of the content the game
  plays, so the §18.6 dev Backing dropdown has only ever been able to offer dead assets
  (**D-CSV-13**, CSV-3), and the underlying scan-root mismatch is **D-CSV-14** (CSV-5,
  cross-boundary; local melodies show the same shape, `Patterns/Melody` vs the
  configured `Patterns/Melodies`). Playback is unaffected — palettes and bundles hold
  direct references. (2) With D-CSV-11 dismissed, **`BASS-GAP` fires on 27 of the 30
  live progressions** (19 are 4-measure against an 8-measure default part); combined
  with the bass's single-pass no-repeat-to-fill contract this is the strongest
  pre-CSV-5 evidence for `MGP-ALWTTT-BASSFILL-1`.
- **Clean-slate worklist** in sub-roadmap §4.1.1: 12 local deletions (all orphan test
  residue), 2 design calls (the only two authored melody patterns, both unreferenced,
  in a mechanism no card uses), 28 package-baggage items that ALWTTT cannot delete.
  **Bundle cleanup is not decidable from this export** — no card → bundle reverse index
  exists (new gap `SSoT_Editor_Authoring_Tools §17.10`).

Rider docs touched: `SSoT_Editor_Authoring_Tools.md` (§17.2/§17.4/§17.6/§17.7 rewritten,
new §17.10, §14.8 resolved), `SSoT_Dev_Mode.md` (§18.6 coverage limitation),
`CSV_Composition_Validation_Sub_Roadmap.md` (§4.1.1 rewritten + worklist, batch table,
D-CSV-11 resolved / D-CSV-12 locked / D-CSV-13 measured / D-CSV-14 registered),
`CURRENT_STATE.md`, `coverage-matrix.md`.

---

## 2026-07-17 — DBG-C2: composition-debug interactive controls (closed; MGP-ALWTTT-DBG arc closed)

The write/interactive half of the composition-debug feature. Consumes only
package surfaces recorded by DBG-C1 (composite keying, the `patternOverrides`
step-0 override, the runtime Roman importer, the DBG-2 pattern repository);
**no MidiGenPlay file touched.**

- **`patternOverrides` LIVE.** `CompositionSession.DevPatternOverrides`
  (`static`, dev-only, `MusicianTrackKey → PatternDataSO`) passed to
  `RenderSinglePart` when non-empty (null idle). D-C1-1's inert C1 passthrough
  now carries values; package precedence step 0.
- **Per-track override UI.** Rhythm/Backing/Melody dropdowns from the full
  runtime registry (`PatternRepositoryResources`, TS-filtered), off-band
  assets annotated (D-C2-2=A). Bassline vetoed (shared progression, package
  warn+ignore); Harmony vetoed (no v1 channel).
- **Roman → Backing override (D-C2-1=A).** Free text →
  `ChordProgressionRuntimeImporter.TryParseRoman` (part TS + editable
  tonality/measures/default-duration) → `ChordProgressionData` Backing
  override. Importer verdict verbatim, out-of-alphabet ⇒ hard fail (nothing
  applied), instance `DontSave` and never persisted.
- **R2a debug-play (D-C2-3=A).** "Re-render part now" bumps `DevOverrideStamp`
  → fresh render through the normal seeded `PlaySinglePartLoop`; seed-pinned ⇒
  bit-reproducible. Distinct from the design doc's §4 card-injection R2a
  (still M1.5 Phase 5, unbuilt).
- **Cache interaction (D-C2-4=A).** Overrides never cache-keyed:
  `MidiMusicManager` bypasses stem/bundle caches when overrides are supplied
  (Mod-DIR-style one-shot bypass); `CompositionSession` stamp-invalidates
  `PartCache` (keepTempo+keepInstruments) on change. Clear restores baseline.
- **A1 CONFIRMED** against `Design_Composition_Debug_Tab_v0_1 §3.1` (was open
  at C1). `GenerationDebugFormatter` unchanged.
- **BC gate:** dev OFF or all controls idle ⇒ byte-identical (ST-C2-7);
  clear/restore regression (ST-C2-8); production compile zero tab footprint
  (ST-C2-9).

Decisions D-C2-1..4=A · A1 confirmed; inherited D-C1-1=A / D-C1(seed)=A /
D2=A / D3 / D-DBG1..5 / ID-1..4 / E-1..5(+1b/2b). ST-C2-1..9 PASS. Primary
homes: `SSoT_Dev_Mode.md` §18.4–§18.8 · `SSoT_Runtime_CompositionSession_Integration.md`
§8 inv 9. **Follow-up:** DBG-OBS-1 (non-blocking readback-display note).
**Arc:** MGP-ALWTTT-DBG CLOSED — write `Roadmap_ALWTTT_Debug_Seams` as the
arc-close deliverable. SSoT_INDEX + ssot_manifest: no change.

---

## 2026-07-17 — DBG-C1: MusicianTrackKey consumer migration + composition-debug read surface (closed)

Consumer read-side half (D1=B) of the MGP-ALWTTT-DBG arc. The package
(MGP-ALWTTT-DBG-1) re-keyed `PartRender.stemsByMusician` / `melInstByMusician`
/ new `resolvedByTrack` and the `RenderSinglePart` override maps by
`(musicianId, TrackRole)` (`MusicianTrackKey`), added a trailing
`patternOverrides` map, and promoted the `chd:` per-chord marker to governed
contract (MGP `SSoT_Composer_Backing_Track §2.1`). DBG-C1 is the consumer
adoption:

- **Composite keying end-to-end.** `MidiMusicManager` (stem/bundle cache,
  `RenderSinglePart` signature → `trackInputsHashByTrack` + inert trailing
  `patternOverrides`, D-C1-1=A; merged-rebuild ordering now pairs
  `ChannelMusicianOrder[i]`/`ChannelRoles[i]`), `CompositionSession`
  (`PartCache.resolvedMelInstByTrack`), `SongConfigBuilder`
  (`ComputeTrackInputsHashesForPart` per-track). The `FlattenInstrumentReport`
  + id→key expansion shims are deleted.
- **BASS-1 carve-outs retired.** `FilterOutMultiTrackMusicians` +
  `CountTracksForMusician`, the multi-track hash omission, and the `FromUI`
  multi-track pin skip are removed — multi-track musicians are cacheable again
  (ST-S2: `cacheEnabled=True`, per-role hashes; bundle key
  `…@@2:Backing#…,2:Melody#…`). Boundary §4.3 BASS-1 request → **RESOLVED**.
- **Read-only truth surface.** `MidiMusicManager.LastResolvedByTrack` /
  `LastPinnedByTrack` / `LastRenderSerial|PartIndex|Bpm|FromCache` (published
  on every render return; bundle replay republishes the original snapshot,
  D-DBG5=A) + `GetChordTimelineSnapshot()`/`ChordTimelineEntry` (production
  API over the governed chd: contract).
- **Dev surface** (`#if ALWTTT_DEV`): `DevCompositionDebugTab` +
  `GenerationDebugFormatter` — two-phase intent/resolved per-track log,
  `'*'` resolved-only convention (**A1**, per `Design_Composition_Debug_Tab_v0_1
  §3.1`, doc absent from PK — carried open), Compact/Full flag on
  `GigDevSettingsSO`, Copy fingerprint, seed pin (closes `SSoT_Dev_Mode §8.7`),
  infinite composition-loop toggle (`CompositionSession.DevInfiniteCompositionLoop`;
  countdown resets, per-loop host hooks keep firing — D2=A; `IsFinalLoopRunning`
  dev exemption so the CARD-UX-1 final-loop deny does not misfire, D3).
- **BC gate:** dev OFF + no overrides + same seed ⇒ single-track output
  byte-identical (ST-S1 PASS; the only stem-key change is a deterministic
  `:{role}` segment).

MidiGenPlay untouched (consumed the DBG package contract, redefined nothing).
Decisions D-C1-1=A · D-C1(seed)=A · D2=A · D3; inherited D-DBG1..5 / ID-1..4 /
E-1..E-5. ST-S1..S10 PASS. Primary homes:
`SSoT_Runtime_CompositionSession_Integration.md` §8 inv 9 (+ §10/§11 riders,
inv 11 dev rider) · `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §4.3 · `SSoT_Dev_Mode.md`
§18 (+ §3/§6/§8.7). **Next:** DBG-C2 (interactive controls) → arc-close
`Roadmap_ALWTTT_Debug_Seams`. SSoT_INDEX + ssot_manifest: no change.

---

## 2026-07-16 — TLM-1: `ALWTTT_DEV` run telemetry logger (closed)

**Type:** semantic (new dev surface contract) + operational (S5i unblocked) + lifecycle (TLM-1 closed; optional rider TLM-1b opened as backlog). **No gameplay/semantic change to shipped systems.** Opened by BALANCE-XREF (BR-D2=A) and slotted immediately before S5i; this entry closes it.

- **New dev surface.** `DevRunTelemetryLogger` (`Assets/Scripts/DevMode/DevRunTelemetryLogger.cs`, whole file inside `#if ALWTTT_DEV`; static, lifecycle owned by `DevModeController` `Initialize()`/`Shutdown()` — sibling pattern to `DevGigOutcomeTracker`). **Read-only** sensory-bus subscriber (`GigStartedEvent` reset + `RequiredSongCount`, `CardPlayedEvent` ordered plays, `LoopResolvedEvent` loop count, `GigOutcomeEvent` writes the record). Publishes nothing; mutates no game state; MidiGenPlay untouched.
- **Record + output.** One JSON-Lines object per gig (schemaVersion 1): `timestampUtc`, `sessionId` (per-play-session), `encounterLabel`, `requiredSongCount`, `won`, `lossCause`, `songsCompleted`, `loopsPlayed`, `roster` (musician CharacterIds), `audience[]` (authored CharacterName + spawn index + endVibe/maxVibe/convinced — snapshotted at `GigOutcomeEvent`, which fires **before** `WinGig`/`LoseGig` cleanup), `plays[]` (cardId + **song-index-at-play-time** + isComposition + inspirationCost, ordered), `playCounts[]`. Song-index-at-play-time is the mandated BALANCE-XREF confound guard (the "Madness"/SFX late-run correlation trap). Output: Editor → `<projectRoot>/DevTelemetry/gig_runs_YYYY-MM-DD.jsonl` (outside `Assets/`, gitignored); dev Player builds → `persistentDataPath/DevTelemetry/`. Never in `Resources`; strips clean from production builds. Stats tab shows a "Last gig written to: …" line.
- **Decisions.** **D-TLM-1=A** (JSON Lines over CSV — the schema is inherently nested). **D-TLM-2=B** (mandated fields + schemaVersion/timestampUtc/sessionId/requiredSongCount + per-play isComposition/inspirationCost + per-audience convinced — every addition already in an event payload or one property read). **D-TLM-3=A** (cohesion-collapse losses NOT logged: `MusicianBase.OnBreakdown → BandCohesion 0 → LoseGig()` never publishes `GigOutcomeEvent`; only `ResolveGigOutcomeAndEnd` does; same pre-existing blind spot as the session tally; `lossCause` therefore constant `"unconvinced_after_final_song"` for logged losses). Publisher-side fix deferred as **optional rider TLM-1b** (needs a per-gig double-fire latch + tally/tutorial side-effect review; open only if S5i hits cohesion losses).
- **Coverage limitations (load-bearing for S5i analysis).** Cohesion losses unlogged (above); editor Debug context-menu Win/Lose bypass the event by design; partial gigs (retry/quit mid-gig) produce no record (accumulators reset on the next `GigStartedEvent`); audience identity uses authored `CharacterName` + spawn index because `AudienceCharacterBase.CharacterId` embeds `GetInstanceID()` and is not session-stable.
- **Regression guarantee.** Logger publishes nothing and mutates no state; gig outcome with the logger active is behavior-identical (**ST-TLM-R1 PASS**). Full smoke set **ST-TLM-1..4 + ST-TLM-R1 all PASS** (win record; loss record; song-index correctness across a multi-song gig; production-build strip clean; regression).
- **Docs touched this pass.** `SSoT_Dev_Mode.md` (new §17 surface + §9.12 smoke rows; update-rule bullet added) · `CURRENT_STATE.md` §2/§3/§4/§5 · this changelog · `coverage-matrix.md` (BALANCE-XREF maxims-doc registration sweep — **partial**: `SSoT_INDEX.md` + `ssot_manifest.yaml` remain owed, absent from working set) · `S5_DemoCutClose_Sub_Roadmap.md` (TLM-1 marked closed + TLM-1b rider). **Verified unchanged:** all subsystem SSoTs except Dev Mode (no semantic/contract change), the maxims doc, all gameplay code paths.
- **Classification.** semantic (new dev-surface contract in `SSoT_Dev_Mode`), operational (live-work front advances to S5i), lifecycle (TLM-1 closed; TLM-1b optional rider parked). Not authority (no precedence change), not structural (no new SSoT; one reference row added to coverage-matrix).

---

## 2026-07-16 — BALANCE-XREF: deckbuilder balance-research integration (maxims doc + TLM-1 opened + BR-D1..4)

**Type:** reference + operational + lifecycle. **No gameplay/semantic change** — documentation-and-planning consolidation. A comprehensive research study of how successful roguelike deckbuilders (Slay the Spire 1/2, Monster Train 1/2, Griftlands; secondary: Balatro, Cobalt Core, Across the Obelisk, et al.) are designed and balanced was cross-referenced against the ALWTTT baseline (starter deck v1.3, ECON-1, S5h rewards, the S5i plan, Phase C scope). Sources were developer-primary where available (GDC 2019 "Metrics Driven Design and Balance", the STS2 AMA, Shiny Shoe / Klei interviews). This entry closes the consolidation batch.

- **New governed doc (reference / design-philosophy).** `planning/Design_Game_And_Card_Maxims_v0_1.md` — the project's consolidated game/card-design **maxims**: six existing maxims lifted from where they already lived (E1 *mínimas cartas* + E2 blind-listener test from `Design_Starter_Deck_v1`; E3 Sound-Design-Priority + E4 Sensory-Contract from `Design_Project_Directives`; E5 budget=tempo/cost=spike from ECON-1; E6 always-showable-build from the project objective) plus **twelve new maxims (N1–N12)** derived from the research, each evidence-tagged. Includes the E1↔N7 reconciliation (axis-distinctness vs. copy-count) and a compact "what the research changes for ALWTTT" section (alignments / gaps / warnings). **Classification:** `reference (design philosophy)` — *not a SSoT*; philosophy/intent that does not override any subsystem SSoT or contract. **Registration owed (not applied — files not in this working set):** `SSoT_INDEX.md`, `ssot_manifest.yaml`, `coverage-matrix.md` should index the maxims doc (mirroring how `Design_Vibe_Telegraph_v0_1` and `Design_Action_Economy_v1` are indexed). Recorded as owed in `CURRENT_STATE.md` §5.

- **Batch opened (code deferred): TLM-1 — run telemetry logger** (`planning/active/S5_DemoCutClose_Sub_Roadmap.md`, new section; slots **before S5i**). An `ALWTTT_DEV`-gated logger that writes one per-gig record (outcome + loss cause; songs/loops; per-audience end-Vibe; **cards played with song-index-at-play-time**; per-card counts) by subscribing to the existing bus (`GigOutcomeEvent`, `CardPlayedEvent`, `AudienceVibeImpactEvent`) — no new gameplay events, zero production impact. Rationale: "build the metric server early" is the study's loudest finding; S5i would otherwise run blind on the two primary metrics (pick rate, appearance-in-winning-runs). The song-index field is the mandatory confound guard against the "Madness"/late-SFX effect (late content spuriously correlates with wins). Smoke set ST-TLM-1..4 defined; DoD = valid record on win and loss, correct song-index, clean Player-build strip, no behavioral change.

- **Decisions (BALANCE-XREF ledger, in the sub-roadmap).** **BR-D1 = B** — reward *skip* will exist (StS model, deck-consistency lever); inert in the single-gig demo, so implemented in Phase C **S6** (scope recorded in `Design_Vertical_Slice_v0_1.md` §3.1). **BR-D2 = A** — TLM-1 (above). **BR-D3 = A** — owned-card reward exclusion (D9) stays absolute for the demo; revisit trigger recorded in `Design_Vertical_Slice_v0_1.md` §11 (allow duplicate/consistency rewards, maxim N7, when a multi-gig run empties the pool). **BR-D4 = B** — no dedicated replan session; consolidate now, fold TLM-1 + the S5i lenses into the existing sequence, forward-point the expensive structural items (run power curve, a break-the-game combo, a Covenant-style difficulty ladder) to Phase C / meta-progression; **S5j is not augmented** (stays §5.4 + tag). A full rebuild was rejected as broad premature rework on an accepted, near-tag baseline.

- **S5i enriched (no new work).** Three research-derived **observation lenses** added to the S5i section: **L1** comprehension at the 15-unique starter (with the N7 add-copies fallback), **L2** the poggable-moment audit (maxim N4; candidate = the deferred Earworm × Captivated combo), **L3** the zero-margin finisher D-DEMO-1 (maxim N5). Lenses on the existing playtest, backed by TLM-1.

- **Docs touched this pass.** `Design_Starter_Deck_v1.md` (v1.4 — corrected the stale "12/10 unique" note to 17/15, framed the divergence as deliberate + S5i-verified, cross-referenced the maxims doc + N7 reconciliation) · `S5_DemoCutClose_Sub_Roadmap.md` (BALANCE-XREF ledger, TLM-1 batch, sequence backbone + reconciled line, S5i lenses) · `Design_Vertical_Slice_v0_1.md` (§3.1 S6 reward-skip + run power curve; §11 BR-D3 trigger + power-curve; §12 cross-refs) · `CURRENT_STATE.md` (§2/§3/§4/§5) · this changelog · **new** `Design_Game_And_Card_Maxims_v0_1.md`. **Verified unchanged:** all subsystem SSoTs (no semantic/contract change), `Design_Action_Economy_v1.md`, `Design_Project_Directives_v0_1.md` (its D1/D2 are *referenced* by the maxims doc, not moved). **Owed, not applied (files absent from this set):** `SSoT_INDEX.md` / `ssot_manifest.yaml` / `coverage-matrix.md` maxims-doc registration; TLM-1 close docs (`SSoT_Dev_Mode.md`) when the batch runs.

- **Classification.** reference-only (the maxims doc + starter-deck note), operational (sub-roadmap sequence + S5i lenses + vertical-slice scope), lifecycle (TLM-1 opened; BR-D1..4 locked). Not semantic, not authority (no precedence change), not structural beyond adding one reference doc.

## 2026-07-16 — DF-INSPLOOP: card-gated per-loop Inspiration effect (+ DEV-WINLOSE)

**What changed (semantic).** Reintroduced per-loop Inspiration generation as a card effect, the effect S5e deferred. New `CardEffectSpec` subclass `AddInspirationPerLoopSpec` (JSON `"AddInspirationPerLoop"`, `amount ≥ 1`). Derived from `TrackEntry.sourceCardDefinition` at `CompositionSession.EvalPerLoopInsp` time via the single helper `AddInspirationPerLoopSpec.SumFor` — never written into `inspirationGenerated` (D-INSP-3=A), so S5e's project-wide zero and the LoopScore complexity-term inertness both hold by construction (D-INSP-4).

**Semantics.** Track-scoped (D-INSP-1=D): active while the carrying card's track is in the looping part; dies on same-role replacement. Additive across distinct tracks (D-INSP-2=A); `MaxInspiration` clamps the total. The flat basal grant (currently 1/loop, `GigFlowSettings.asset`; applied by `GigManager.OnCompositionLoopFinished`) is untouched — the bonus rides the session per-loop path, the same path S5e's basal generation used before it was zeroed. The global `+INS` badge now shows the total per-loop gain (basal + derived); per-track badge shows the track's derived share.

**§9 conformance.** Four layers: data (`AddInspirationPerLoopSpec.cs`) · editor (`CardEditorWindow` add-menu + generic field + label; `DeckCardCreationService` branch) · JSON/LLM (`CardEditorWindow.JsonImport` + `CardImportDtos`; `CardLLMPromptBuilder` + `CardLLMResponseHandler`) · runtime (track-binding in `EvalPerLoopInsp`; `CardBase` no-op branch). Per-track badge shows `complexity + card bonus`.

**Import hardening.** `CardEditorWindow.JsonImport.ApplyCompositionJson` now warns when a non-empty `trackAction.styleBundle` path resolves to null (previously silent), which had masked a "card played, no track created" bug during validation (null bundle → augment-only, D4=A skip).

**Content.** Two authored cards — **In the Pocket** (Composition Track / Rhythm, C2, cost 2, bundle `Rhythm - C2 - Moderate FourFour`) and **Vamp** (Composition Track / Backing, Sibi, cost 2, bundle `Backing Card Config - Core Minor`), each one `AddInspirationPerLoop` amount 1, `inspirationGenerated` 0. **D-INSP-6=A' (corrected):** entries live in the owning musician's catalog (`C2_CardCatalogData` / `Sibi_CardCatalogData`) with `RewardPool`+`UnlockedByDefault`; the reward pool (`BuildRewardCardPool`) sources `RewardPool ∩ UnlockedByDefault` per-musician (generic-catalog entries excluded), so the v1 generic-catalog placement was runtime-ineffective. Starter untouched.

**DEV-WINLOSE (companion).** Dev overlay (`DevModeController.DrawInfiniteTab`) gains WIN / LOSE buttons calling new `ALWTTT_DEV`-guarded wrappers on `GigManager` (`DevWinNormalFlow`→`WinGig`→RewardCanvas; `DevLoseNormalFlow`→`LoseGig`; plus immediate force-win/lose). Outcome-tracker bypass preserved (dev outcomes intentionally not counted). Shipped to unblock ST-9 reward-appearance testing. Home: `SSoT_Dev_Mode.md`.

**Decisions.** D-INSP-1=D · D-INSP-2=A · D-INSP-3=A · D-INSP-4 (no-touch) · D-INSP-5=A · D-INSP-6=A' · D-INSP-7=A.

**Verification.** ST-1..9 PASS (ST-1 regression: basal grant intact, badges correct; ST-3 replacement kills bonus; ST-4 self-creates after bundle assign; ST-7 LoopScore/TotalComplexity identical with/without the card, by construction; ST-9 both cards appear as rewards after A').

**Surfaced divergence.** Basal per-loop grant is 1 (`GigFlowSettings.asset`), diverging from the S5e row's "3/loop" — recorded as a CURRENT_STATE §4 open item, reconcile in S5i.

**Deferred to S5i.** Tuning of card cost/amount; cleanup of write-only dead-state `_buildingPartInspirationPerLoop`; the LoopScore complexity-term decision; basal-grant reconciliation.

**Docs touched.** `Design_Pending_Effects_v1 §11` · `SSoT_Card_Authoring_Contracts §5.6a/§9` · `SSoT_Card_System §6.2/§10.1` · `SSoT_Scoring_and_Meters §3.2` · `SSoT_Dev_Mode` (DEV-WINLOSE) · `CURRENT_STATE` (row + §4 riders + S5e-row amendment) · this changelog.

## 2026-07-15 — DEMO-FIXES-A (gig-open tutorial opt-in + demo-detail UI)

Inserted demo-cut-close batch before S5i. Code applied; ST-DF-1..6 + 8..13 PASS, ST-DF-7 deferred to Dev Mode / M1.5.

- **DEMO-TUT-TOGGLE** — gig-open modal (`TutorialOptInPrompt`) chooses tutorial on/off; single source of truth `PersistentGameplayData.TutorialEnabled` (one-shot read at gig open, per-gig re-ask, launch-scoped). `GigManager.Start` defers `StartGig` until answered; forced-hand fill moved `Awake → TutorialGuidedDriver.PrepareForGig` (timing-immune); reactive path neutralized by belt guards; driver resolved at runtime via `UIManager.GigCanvas` (D-DF-8=A, cross-scene-safe). Homes: `Design_Tutorial_System_v0_2` §9.3 + ledger.
- **R1** — beat-8 hold `available = HandHas` only (D-DF-4=A); `Design_Tutorial_System_v0_2` §9.2 closed.
- **CT1** — highlight pulse survives the modal close while a directive is armed (`TutorialController.PulseWhileDirective`); pulse only, overlay still closes.
- **DF-COST0** — hide cost badge at cost 0 (`CardBase.SetCard`, `inspirationCostBadgeRoot`, two-prefab; D-DF-5=A); mirror of S5e-ext gen-badge. Homes: `SSoT_Card_System` §10.4 + `CURRENT_STATE` §4.
- **DF-ECONTIP** — ECON-1 pip hover tooltip via existing `TooltipManager` (`EconPipTooltipTarget`, D-DF-6=A). Home: `SSoT_Gig_Combat_Core` §14.7.
- **DF-CATALOG** — Dev Mode catalogue tab sources the runtime band-catalog union (`PersistentGameplayData.BuildBandCardCatalog`, D-DF-7=A); `GameplayData.AllCardsList` demoted to fallback-only. Home: `SSoT_Dev_Mode` Catalogue tab.
- Decisions D-DF-1..8 all = A. Out of batch: `DF-INSPLOOP`, `DF-ARTIC`.

## 2026-07-13 — JUICE-PW CLOSED: card Vibe-impact sensory surface (Psychic Waves presentation)

Inserted demo-cut-close batch (`S5_DemoCutClose_Sub_Roadmap.md`). **MidiGenPlay untouched.**

**Semantic (sensory).** New bus event **`AudienceVibeImpactEvent`** (`Assets/Scripts/Sensory/Events/`), published **once per audience target** from the `ModifyVibeSpec` branch of `CardBase.ExecuteEffects`, carrying audience ref/index/id, performer, card, `BaseDelta` / `FinalDelta` (post-Flow) / `AppliedDelta`, and `FanoutIndex` / `TargetCount`. Blocked-by-Indifference is derived (`FinalDelta > 0 && AppliedDelta == 0`). **`CardPlayedEvent` was rejected as the carrier (D1=A):** it is published from `DeckManager.OnCardPlayed` *after* resolution, once per card, with no per-target delta — it cannot express "landed on two members, blocked on the third". A `FinisherPlayedEvent` was also rejected: "finisher" is a tutorial concept owned by `TutorialGuidedDriver.IsFinisher`, and a sensory event of that name would duplicate authority. **Timing is structural, not scheduled:** because the publish sits inside effect resolution and `OnCardPlayed` runs at the tail of `CardUseRoutine`, the impact FX necessarily precedes the beat-8 `TutorialLoopHoldGate.Release()` (which keys on `CardPlayedEvent`). Primary home: **`Design_Sensory_Contract_v0_1.md` §3 (event row + note) + §4 (audit rows)**.

**Semantic (audio).** New key **`SensorySfxType.CardVibeImpact`** — **one sting per card play, not one per AoE target**: `SensorySfxPresentation.ForCardVibeImpact` returns a key only for `FanoutIndex == 0` (D3=A — the *visual* fan-out is what staggers; the audio does not). Immediate, never jittered (invariant 10 unchanged). Fires even when the first target blocked (the card resolved; the grey floater needs its audio floor). **D-PW-AUDIO:** the impact sting *replaces* the drop-time sting — Psychic Waves is authored `AudioType = None`, because the card-direct and bus paths are not mutually exclusive in code. Primary home: **`SSoT_Audio.md` §3 + new invariant 18**; authoring rule: **`SSoT_Card_Authoring_Contracts.md` §5.15 (new)**.

**Presentation (FT + animation).** `SensoryFtPresentation.TryBuildVibeImpactFt` → **`-N` cyan** when it lands, **`INDIFFERENT` grey** when blocked (same word/colour as the song-end blocked surface). The **short** `-N` is deliberate: song-end keeps `-N Vibe`, so a late finisher and the song-end wave stay readable when they collide (ST-PW-7). The batch opened assuming a `"+5"` floater; that was corrected in-batch to the **S5e damage-number convention** (positive Vibe depletes the resistance pool). `SensoryFxAdapter` staggers the per-member floaters (`FanoutIndex × VibeImpactStaggerStep`) and fires `CharacterAnimator.PlayImpactKick` + a particle burst on each **landed** member and on the performer (`FanoutIndex == 0`); blocked members get no kick. **D2=B:** `PlayImpactKick` is a **procedural** one-shot (a `LateUpdate` overlay with snapshot/restore over the beat pose, so it cannot accumulate when the beat loop skips a frame or the S5b idle gate is off) — no Animator-state / clip system was introduced for one card. `CharacterSfxProfileSO` was **not** touched (it stays reaction-only, phase 1).

**Code:** `AudienceVibeImpactEvent.cs` (new), `CardBase.cs`, `SensoryFtPresentation.cs`, `SensorySfxType.cs`, `SensorySfxPresentation.cs`, `SensoryFxAdapter.cs`, `SensoryAudioAdapter.cs`, `CharacterAnimator.cs`. Assets: Psychic Waves `AudioType = None`; `SoundBankSO` gains a `CardVibeImpact` entry.

**Docs (JUICE-PW-DOC, applied 2026-07-13):** `Design_Sensory_Contract_v0_1.md` (§3 event row + note + consumers; §4 audit rows), `SSoT_Audio.md` (§3 two-paths + key list + new sting paragraph; §7 **new invariant 18**; §8 smokes; §9 forward refs; header), `SSoT_Card_Authoring_Contracts.md` (**§5.15 new**), `Design_Starter_Deck_v1.md` (§5.17), `S5_DemoCutClose_Sub_Roadmap.md` (JUICE-PW → CLOSED), `CURRENT_STATE.md` (§1/§2/§3/§4/§5), `coverage-matrix.md` (audio SFX row). **`SSoT_INDEX.md`: no change — verified** (no new governed doc, no authority reordering). **`SSoT_Card_System.md`: no change — verified** (card *semantics* are unchanged; the publish is presentation plumbing on an existing effect path). Also verified unchanged: `SSoT_Audience_and_Reactions.md`, `SSoT_Runtime_CompositionSession_Integration.md`. **`ssot_manifest.yaml`: not in the PK** — a `hard_invariant` ("a card sounds on exactly one path") was **proposed as a paste-ready fragment (OPT-5), not applied.** **Same session:** the stacked **S5h doc pass** was applied (entry below at 2026-07-07 — its blocker, this file's PK absence, had cleared) and the **DEMO-FIXES backlog registry was expanded** (planning-only; CT1 + DF-COST0/DF-ECONTIP/DF-CATALOG/DF-INSPLOOP/DF-ARTIC, user 2026-07-13 — sub-roadmap).

**Stale-statement sweep (JUICE-PW-DOC).** (1) **`Design_Sensory_Contract_v0_1.md` §4 asserted FT = "yes" for `Vibe change (audience)` / `ApplyIncomingVibe`.** False for the **card** caller: before this batch a card's Vibe effect produced *no* floating text at all (only the Vibe-bar animation). The "yes" was inherited from the Earworm-tick and song-end callers, which have their own rows. Row split and corrected. (2) **`SSoT_Audio.md` §3's `SensorySfxType` member list was missing `RewardOpened`** (shipped with S5h, 2026-07-07) and the `SensoryAudioAdapter` subscription list was missing `RewardChoiceOpenedEvent` — S5h doc-pass debt, backfilled. (3) **`Design_Sensory_Contract_v0_1.md` §3's event table was missing three shipped events** — `MusicianStressHitEvent` + `AudienceBlockedEvent` (TUT-REBUILD) and `RewardChoiceOpenedEvent` (S5h) — **backfilled (OPT-1=A, applied 2026-07-13)**. (4) **§4 carried the superseded S2 "starting skeleton" audit table below the S3a as-built one** — **retired with a SUPERSEDED marker (OPT-2=B, applied 2026-07-13)**. (5) `SSoT_Audio.md` §3's old note "a future `CardPlayedEvent` bus consumer … must not fire on both paths" is superseded: the effect-time half landed, and the no-double-fire rule is now enforced by *authoring* (`AudioType = None`), recorded as invariant 18. **Root-cause note:** items (2) and (3)'s S5h portion were not orphan drift — they were the un-applied S5h doc pass, cleared this session. **OPT-3** (the sensory carril's missing SSoT home) is **recorded as open decision D-SENSORY-HOME** in `CURRENT_STATE.md` §4, not executed. **OPT-4** (tutorial-doc rider) declined — cosmetic; the tutorial doc was untouched this pass.

**Smoke:** ST-PW-1..10 **all PASS** (2026-07-13). No deferrals — ST-PW-5 (Indifference → `INDIFFERENT`, no kick) ran without Dev Mode. ST-PW-10 is a new regression guard on the procedural kick (no scale drift on a `scaleOnBeat` + `skipEveryNBeats` character after repeated kicks).

**Open:** the `CardVibeImpact` clip is a **placeholder** (`Telephone`) → **D1**; it is the sting on the demo's beat-8 finisher, so it is the highest-value clip in that backlog. Finisher **economy** (cost 3 / magnitude 5) is untouched → **S5i**.

**Doc-diff packages retired at close:** `JUICE-PW_Doc_Diffs_2026-07-13.md` + `S5h_Doc_Diffs_2026-07-07.md`.

## 2026-07-13 — CARD-UX-1 CLOSED: unplayable-card overlay + single playability source; final-loop composition lock; spawn-hook highlights

Inserted demo-cut-close batch during S5h (`S5_DemoCutClose_Sub_Roadmap.md`). **MidiGenPlay untouched.**

**Semantic (cards).** `GigManager.EvaluateCardPlayability(CardDefinition) → UnplayableReason` {`TutorialGate`, `ActionTiming`, `FinalLoopLock`, `Inspiration`, `Budget`, `None`} is now the **single playability computation for display**. It aggregates the gates the play paths already consult and never consumes (`CanConsumePlay` / `CanAffordInspiration`, never `TryConsume`), so per-frame polling is side-effect free; `HandController` polls it and `CardBase` renders a red overlay through the existing `passiveImage` / `SetInactiveMaterialState` mechanism (**no new serialized field** — the two-prefab wiring vector stays closed, D4=A). Enum order is the precedence, deliberately: a tutorial directive outranks a domain rule. Invariant: **no consumer computes playability locally**. Primary home: **`SSoT_Card_System.md` §10.5 (new)**.

**Semantic (composition runtime).** New **final-loop composition lock** (D2=A). Code truth that had never been written down: since D-D=β retired the NextPart gesture, every composition dropped during a running loop normalizes to `CurrentPart`, applies to the *currently looping* part, and becomes audible on that part's **next** loop (the Pending-Effects model) — so on the **final** loop it never renders and the play is pure waste. `CompositionSession.IsFinalLoopRunning` + a deny in `TryPlayCompositionCard` **before any spend** (no inspiration, no ECON-1 budget), with a presentation-avoidance mirror in `GigManager.TryPlayCompositionCard`. **Exempt while a tutorial loop-hold is armed** (a held loop replays, so the change *would* render); `TutorialModalGate` is **not** exempt — modals suspend audience turns and dragging, they do not replay the loop. Primary home: **`SSoT_Runtime_CompositionSession_Integration.md` §5.4 (new) + §8 invariant 11**.

**Semantic (tutorial).** New directive gate `TutorialInputGate.SingleCardOnly` (D6=A), armed at beat 8 alongside the loop hold: the finisher becomes the only playable card. This — not the lock — is what gates *compositions* in the tutorial's final loop: with parts-per-song = 1 the demo's only final loop **is** the held loop, so the FinalLoopLock is structurally unreachable there (ST-CU-7 failed on first run; the **spec** was corrected, not the code). Hand-guarded (`deck.HandHas`) to avoid a zero-playable-card hold. Blocks card drag only, not End Turn.

**Structural (tutorial highlights).** Spawn-hook registration (D1=C): `TutorialHighlightSpawnHook` + `TutorialHighlightTarget.InitRuntime` attach highlight targets to runtime-instantiated characters, status icons, and hand cards at `GigManager.BuildBand`/`BuildAudience`, `CharacterCanvas.TryCreateIcon`, and the `DeckManager.BuildAndGetCard` tails — closing the **world-character + hand-card highlights deferred from TUT-R3/T3b**. Prefab variants were rejected (cannot cover per-status icons or a single card prefab keyed by `CardDefinition.Id`). Duplicate keys (4 musicians registering `musician_stress_bar`) are disambiguated by re-registering the **affected** character on `MusicianStressHitEvent` / `AudienceBlockedEvent` (D3=B). The T3b world→screen edits (`Spotlight` struct, `ApplySpotlight`, `ResolveHighlight`, world fields on `TutorialHighlightTarget`) are now **applied in build**.

**Scoping (ECON-1).** The overlay's budget input covers **statically-resolvable payers only** (`FixedPerformerType != None`); `AnyMusician` cards are excluded pending **D-ECON-GENERIC** (D5) — a false red on a card that *is* playable against another musician is worse than a false green on an advisory overlay, and `TryConsumePlay` remains the enforcement. ECON-1's rule is unchanged; only the *UI surface* of it is scoped. Riders: `SSoT_Gig_Combat_Core.md` §14.5 + §14.7.

**Code:** `TutorialHighlightSpawnHook.cs` (new), `TutorialHighlightTarget.cs`, `TutorialOverlayView.cs`, `TutorialController.cs`, `TutorialInputGate.cs`, `TutorialGuidedDriver.cs`, `CharacterCanvas.cs`, `GigManager.cs`, `DeckManager.cs`, `CompositionSession.cs`, `CardBase.cs`, `HandController.cs` + the card gameplay prefab (`passiveImage` red restyle).

**Docs (CARD-UX-1-DOC, applied 2026-07-13):** `SSoT_Card_System.md` (§10 intro pointer, **§10.5 new — primary home**, §12 owns-list), `SSoT_Runtime_CompositionSession_Integration.md` (**§5.4 new + §8 inv. 11 — primary home**), `SSoT_Gig_Combat_Core.md` (§14.5 + §14.7 riders), `Design_Action_Economy_v1.md` (§7 — D-ECON-GENERIC entry created; the doc had none, and it is now the cross-ref anchor), `Design_Tutorial_System_v0_2.md` (§4.2 gate (d), §5.3 rewritten, §6B.3, §8.2, §9.1/§9.2, §10 ledger), `Design_Demo_Cut_v1.md` (§1.1 consequence note), `S5_DemoCutClose_Sub_Roadmap.md` (CARD-UX-1 → CLOSED; DEMO-TUT-TOGGLE + R1 registered under DEMO-FIXES), `CURRENT_STATE.md` (§2/§3/§4), `coverage-matrix.md` (rows 9 / 32 / 33). **`SSoT_INDEX.md`: no change — verified.** No new governed doc, no authority reordering: §10.5 and §5.4 land inside existing authorities. **`ssot_manifest.yaml`: deferred pending decision (D10)** — two candidate `hard_invariants` proposed (playability computed in exactly one place; composition denied on a part's final loop); the invariants are already normative in the SSoTs, so the manifest edit is optional hardening.

**Stale-statement sweep (CARD-UX-1-DOC).** (1) **No doc asserted "compositions can be played at any time during a loop"** — the closest statements (`SSoT_Gig_Combat_Core.md` §14.2 / `Design_Action_Economy_v1.md` §4: a second composition "enters as a mid-song add and is audible from the loop after its drop (≥2)") are *consistent* with the routing fact now written into §5.4; the lock is that rule's boundary case. No correction needed. (2) The highlight-registration statements in `Design_Tutorial_System_v0_2.md` §5.3 ("registry + serialized fallback"; "world→screen … pending in TUT-R3 Tranche 3") and §8.2/§9.2 were stale and are corrected by this pass. (3) `Design_Pending_Effects_v1.md` §"Balance note" already anticipated the hazard ("a … double pending Earworm played in the final loop is degenerate. Cost / timing constraints are mandatory") — the lock is one of those timing constraints; §5.4 cross-refs it. (4) `TUT-REBUILD_Sub_Roadmap.md` (lines 3 / T3b) still points the world-character + hand-card highlights at CARD-UX-1 — literally true (that is where they were delivered), so it was left untouched; a "✅ delivered 2026-07-13" annotation is proposed but not applied.

**Smoke:** ST-R3b-2, ST-R3b-5, ST-CU-1..13 **all PASS** (2026-07-13; ST-CU-7 after the F1–F3 spec correction).

**Open:** **R1** — the beat-8 hold arms on `HandHas || PilesHave` and held loops grant no draw, so a failed beat-7 scripted draw can hold the loop with the finisher unreachable (pre-existing since TUT-R2; triaged to DEMO-FIXES). **DEMO-TUT-TOGGLE** — a gig-start "enable tutorial?" popup, also the clean way to test the final-loop lock. **D-ECON-GENERIC** — unchanged, now also gating the `AnyMusician` half of the overlay's budget input.

**Doc-diff package retired at close:** `CARD-UX-1_Doc_Diffs_2026-07-13.md`.

---

## 2026-07-12 — BASS-1 + BASS-CARD-1 — multi-role tracks per musician; Bassline card authoring

Inserted cross-cutting fix during S5h. **MidiGenPlay untouched.**

**Semantic (runtime).** A part's tracks are keyed **`(musicianId, role)`**, not `musicianId`. One musician may hold several role-tracks simultaneously (Backing + Melody + Bassline). Track card semantics: same role ⇒ replace that role's track; different role ⇒ add alongside. The old musician-only lookup *retargeted* the musician's single track. Decisions: **D-ALWTTT-FIX = A'** (the `(musicianId, role)` key), **D1 = A** (one UI row per (musician, role)), **D2 = A** (`InstrumentEffect` applies to all family-matching tracks of the target), **D3 = A** (stem cache + part-cache pin disabled for parts holding a multi-track musician; session pins carry voice consistency), **D4 = A** (a Track card with no `styleBundle` never creates a track — it augments the matching-role track if present, else applies only its part effect).

**Content bug fixed, not just the bass blocker.** Sibi's starter Backing card (Wormus) followed by her Melody card (Singing Field) — both `FixedPerformerType: Sibi`, and fixed-performer composition cards ignore hover and always resolve onto their own musician — converted her Backing track into a Melody track, removing the song's harmony and breaking the shared-progression mechanic the starter deck is designed around. Live in the shipped build; verified fixed (ST-BASS-9).

**Semantic (authoring).** `composition.trackAction.styleBundleCreate` (+ `StyleBundleCreateJson` / `BundleFieldJson` DTOs) mints a **role-typed** style bundle at Save and applies type-coerced field writes to it. Bundle type derives from `role` via the wizard's existing `ResolveBundleTypeForRole`. Mutually exclusive with `styleBundle`; requires `role`; Composition cards only; unknown field names fail loudly listing the bundle's valid fields; **banned from LLM output** (its `fields` can carry asset paths — the exact channel the §3.3 guard closes). This closes the gap that made Bassline cards unauthorable from JSON: a `BasslineCardConfigSO` carries articulation (`chordExpression` / `arpeggioRate`), not a palette, so there was nothing to point at. `"Bassline"` added to the LLM role-hint list (the *vocabulary* already accepted it — `Enum.GetNames(typeof(TrackRole))`). The GUI wizard's Bassline role preset already existed (CE-E1) but was undocumented; now documented.

**Boundary (reference).** Recorded a known MidiGenPlay constraint: `PartRender.stemsByMusician` / `melInstByMusician` and the `instrumentOverrides` parameter are musician-keyed and cannot represent a musician holding two role-tracks. ALWTTT degrades safely (cache off for affected parts) rather than patching the package; the re-key request to MidiGenPlay is written down in `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §4.3.

**Code:** `SongCompositionUI.cs`, `SongPartElementUI.cs`, `SongConfigBuilder.cs`, `CompositionSession.cs`, `MidiMusicManager.cs`, `CardImportDtos.cs`, `CardLLMResponseHandler.cs` (+5 EditMode tests), `CardEditorWindow.JsonImport.cs`, `CardEditorWindow.LLM.cs`.

**Docs (BASS-DOC-1, applied 2026-07-12):** `SSoT_Runtime_CompositionSession_Integration.md` (§8 inv. 9 carve-out + new inv. 10 + new §11 — **primary home**), `SSoT_Card_Authoring_Contracts.md` (§5.12 amended, new §5.13 + §5.14), `SSoT_Editor_Authoring_Tools.md` (§4.3 + §4.10 + §12.2), `SSoT_ALWTTT_MidiGenPlay_Boundary.md` (§4.3), `Design_Starter_Deck_v1.md` (§5.13 note + new §5.19), `planning/active/Roadmap_ALWTTT.md` (M4.6-prep closure block), `CURRENT_STATE.md`, `ssot_manifest.yaml` (2 new hard_invariants + signal F7), `coverage-matrix.md`.

**Stale-statement sweep (BASS-DOC-1).** Three statements outside the diff package asserted the superseded model and were corrected. (1) `Roadmap_ALWTTT.md` recorded ST-SD-7's failure on **2026-05-06** as *"the runtime model enforces 'one musician = one track active at a time' … Model invariant, not cleanup defect"* and deferred the test — the same bug BASS-1 fixed, diagnosed as design and left in the shipped build for two months. **ST-SD-7 is closed by ST-BASS-9.** (2) `SSoT_Editor_Authoring_Tools.md` §12.2 described `TrackActionDescriptor`'s style bundle as *"optional"* — true of the serialized field, false of the semantics under D4=A. (3) Manifest signal **F7** instructed auditors to read the presence of Integration "invariant 10" as evidence that the never-applied ALWTTT-PCE-PROP doc block was pasted; that slot is now BASS-1's, so F7 gains an explicit numbering-collision note (and PCE-PROP, RESOLVED 2026-07-06, is flagged as independently closable).

**Doc-diff packages retired at close:** `BASS_Doc_Diffs_2026-07-12.md`, `BASS-DOC-1_Extras_D10-D12_PROPOSED.md`.

**D-DOC-1 (closed at doc time):** `SSoT_Runtime_Song_Model_and_Config.md` is **not** edited. It self-declares as the authority for the *package-owned* song model, disclaims session-bridge and `MidiMusicManager` cache semantics in its §6, and is absent from `ssot_manifest.yaml` — it is cross-project reference owned by MidiGenPlay. All ALWTTT-side truth (TrackEntry identity, channel stamping, cache/pin degradation) lives in the Integration SSoT.

**Smoke:** ST-BASS-1..9 **all PASS** (2026-07-12), including ST-BASS-6 (single-track parts byte-identical; stem cache still hits on unchanged re-render) and ST-BASS-9 (Sibi Wormus + Singing Field coexist).

**Open:** Sibi's two Bassline cards (Worm Walk / Worm Pulse) carry the `StarterDeck` flag as a test convenience — starter status unresolved (`Design_Starter_Deck_v1.md` §5.19).

---

## 2026-07-09 — TUT-R3: tutorial doc pass + superseded retirement + copy pass (TUT-REBUILD close-out)

`Design_Tutorial_System` v0_1 → **v0_2** (guided curriculum = gig-1 primary; reactive demoted to fallback + post-song-1; F1/F2, pacing D-TUT-R2b-1=B, registry highlights, Main-Menu revisit host, DoD, ledger). **9 superseded reactive ids retired** (constants + controller call sites + emptied `SupersededIds` + `SeedDemoCut*` reduced to the 2 retained + 18 `.asset` deletions + parity `ReservedUnauthored`); **2 retained reactives de-dashed** (`tut_first_sfx_stage`, `tut_first_sound_card`), ES/EN parity of the 20 dialogs. **Keep Cool → C2-owned** (`FixedMusicianType`), not moved to the generic catalog — the generic-catalog move is deferred pending **D-ECON-GENERIC** (who spends the ECON-1 per-musician action budget for an `AnyMusician` card; home `Design_Action_Economy_v1` / `SSoT_Gig_Combat_Core §14`). New **`TUT-REBUILD_Sub_Roadmap.md`** created as the arc home; **CARD-UX-1 / JUICE-PW / DEMO-FIXES** registered under `S5_DemoCutClose_Sub_Roadmap`. Riders: Starter_Deck v1.3 (15→17, **6 Action / 11 Composition**; performer split corrected to 5 C2 + 6 Sibi), Demo_Cut §1.1 (initial inspiration 3→1, draw 1/0, gens→0; **loopsPerPart stays 4**). Decisions: O1=A, O2=A, D-DEMO-1=4, VERIFY-DOC-STARTER-1=6/11, D-ECON-GENERIC=C. Authored `tut_first_reward_choice` (ES+EN) into the retained-reactive seeder, closing a pre-existing S5h parity gap (the controller enqueue in `OnRewardOpened` was already wired); parity guard gains the `ReservedUnauthored` exemption for the two reserved ids → parity green. World→screen mask + per-beat highlight registration coded; static UI/hand/audience_area wired; world-character highlights + the 2 hand-card highlights deferred to CARD-UX-1. Remaining runtime: apply the retirement + world→screen in-build + ST11/ST12 + smokes. `TUT-R3_Doc_Diffs_2026-07-09.md` retired from the PK at close.

## 2026-07-09 — TUT-REBUILD infra: guided gig-1 tutorial (TUT-R2/R2b/R2c)

Guided gig-1 curriculum implemented as the primary tutorial path, layered over the retained S4 reactive system (D3=B; D-TUT-3 not retired). Infra: scripted draw queue over the M4.5 seam (forced initial hand + scripted finisher draw), directive non-modal input gates (beats 3/5, allow-list incl. a beat-3 "basic compositions" restriction), holdLoop (beat 8, no inspiration re-grant), gig-1 sequence driver with mandatory degrade paths (D2), D8 copy tokens. Two new semantic bus events: `MusicianStressHitEvent` (breakdown beat) and `AudienceBlockedEvent` — the latter because Blocked is a sprite-tint bool, not an SO status (F2), so `StatusAppliedEvent` never fired for it. Driver v2 fixed a publish-before-grant ordering bug (`OnCompositionLoopFinished` publishes `LoopResolvedEvent` before the per-loop inspiration grant, so the beat-8 affordability check under-read by one; FIX-1) + a beat-7 swallow (FIX-2). Pacing model **D-TUT-R2b-1=B**: audio keeps running under a modal (S4 no-freeze retained) but gameplay progression holds — the loop repeats at any boundary while a dialog is up, and audience actions + per-member Vibe payout wait between steps (reverses the v0_1 "progression continues under modal"). Highlight model changed to scene self-registration (`TutorialHighlightTarget` → `TutorialHighlightRegistry`) with serialized bindings as fallback; optional `UIPulseAnimator` "pop". Two starter cards added (Psychic Waves, Keep Cool; 15→17). Config riders: inspiration 1/1, draw 1/0, starter gens→0 (D-TUT-R2-1=B). Copy: no em dashes. Deviations F1/F2 and decisions D-TUT-R1-1..4, D-TUT-R2-1=B, D-TUT-R2b-1=B recorded. Superseded-id retirement + ES/EN parity + Main-Menu revisit host + doc pass in TUT-R3.

## 2026-07-07 — S5h CLOSED: end-of-gig reward screen + venue-SFX unlock (#6b-lite)

**Type:** semantic + operational + lifecycle. Batch S5h (pulled-forward presentation half of old S5d, per D-REPLAN-3). Code applied and validated; `ST-S5h-1..10` PASS. This entry is the documentation close.

- **Reward routing (D1=A, operational).** `GigManager.WinGig` final-encounter branch de-bypassed: Win → `RewardCanvas` → `WinPanel` (Retry/Exit moved into `OnRewardFinished`). De-bypasses the A6 `IsFinalEncounter → WinPanel`-only hack; the flag stays forced (single-encounter demo). `Design_Demo_Cut_v1.md` A6 row updated.
- **Reward sourcing (D2=B, semantic).** Card rewards now source from `RewardPool ∩ UnlockedByDefault` catalog flags via `PersistentGameplayData.BuildRewardCardPool`, excluding cards already in the run deck (D9). The `RewardContainerData` card-list path is retired (asset → presentation-only). `Design_Starter_Deck_v1.md` §Reward pool corrected (the "does not yet consume the RewardPool flag" sentence was made false).
- **Grant correctness (D4).** New `PersistentGameplayData.GrantRewardCard` resolves the owning musician from the band by fixed performer type and routes through `GrantCardToMusician` (fallback plain add). Fixes `ChoiceCard.OnChoice` which unconditionally added to `CurrentActionCards`, mis-filing composition rewards.
- **#6b-lite venue-SFX unlock (user scope amendment 2026-07-07, semantic).** Amends D-REPLAN-5. Venue SFX unlocked as gig rewards, sequential lights→smoke→fire (D6=A); state on `PersistentGameplayData.sfxStageUnlocked[3]`, run-scoped — reset in `ApplyRunConfig`, survives Retry, resets on fresh launch (D7=A). A **locked** threshold is inert at its SongHype crossing — no VFX, no banked Vibe, no `SfxStageCrossedEvent` (D8=A); consequently gig 1 of a fresh run has no SFX Vibe layer (`SSoT_Scoring_and_Meters.md` §6.2 updated; **S5i balance note**). SongHype bar activation gains a second source: `AnySfxUnlocked` OR the S5f `ShowSongHypeBar` toggle (S5f untouched). Full SFX-as-equipment stays Phase C.
- **Sensory + tutorial (reference).** New `RewardChoiceOpenedEvent` (published on reward-screen open, both branches, only when ≥1 box built), `SensorySfxType.RewardOpened` (mapped by `SensorySfxPresentation.ForRewardOpened()`), and `tut_first_reward_choice` (ES/EN, priority ~75). `Design_Sensory_Contract_v0_1.md` §4 updated; `Design_Tutorial_System_v0_1.md` §6 row is provisional (the tutorial rebuild re-baselines the inventory).
- **Files:** 1 new (`RewardChoiceOpenedEvent.cs`), 12 modified (`PersistentGameplayData`, `RewardType`, `RewardDatabase`, `ChoiceCard`, `RewardCanvas`, `GigManager`, `SensorySfxType`, `SensorySfxPresentation`, `SensoryAudioAdapter`, `TutorialDialogSO`, `TutorialController`, `TutorialDialogCatalogSO`). Asset/prefab: RewardCanvas prefab (presentation refs), new `SFX Reward Data.asset`, SoundBank `RewardOpened` slot, tutorial catalog re-seed.
- **Not changed:** `SSoT_Gig_Encounter.md` resolution envelope + `SSoT_Scoring_and_Meters.md` payout-timing semantics (§8) untouched — reward is a post-resolution handoff, not a meter change. SSoT_INDEX unchanged (Tutorial doc row already present; presentation debt rides CURRENT_STATE per S5b).

> **Apply note (2026-07-13, S5h-DOC).** This entry was authored 2026-07-07 in `S5h_Doc_Diffs_2026-07-07.md` and applied 2026-07-13 in the JUICE-PW-DOC session — the doc pass had been blocked on the ALWTTT `CURRENT_STATE.md` being absent from the PK, since resolved. Applied 7 of 8 diffs; **DIFF-S5h-6 was skipped per its own provisional clause** (TUT-REBUILD, closed 2026-07-10, re-baselined the dialogue inventory into `Design_Tutorial_System_v0_2.md`, and `tut_first_reward_choice` was authored in TUT-R3). References above to `Design_Tutorial_System_v0_1.md` and to a pending "tutorial rebuild" are historical. The intervening TUT-REBUILD / BASS-1 / CARD-UX-1 / JUICE-PW entries describe themselves as running "during S5h" — read that as during S5h's **doc-open window**; S5h's code closed before them. Package retired at apply.

## 2026-07-07 — ECON-1 CLOSED: per-turn play economy (1 Action + 1 Composition per musician per period)

**Type:** semantic + reference + lifecycle. Batch inserted between S5g and S5h by design decision 2026-07-06 with Matías, motivated by audience-test results. Code (T1–T6) applied and validated; `ST-ECON-1..7` PASS. This entry is the documentation close.

- **Semantic.** New `SSoT_Gig_Combat_Core.md §14` (per-turn play economy) — primary home. Rule: each musician plays at most 1 Action + 1 Composition card per period (period = pre-song PlayerTurn window, or each performance loop); pools independent (D-ECON-4=A, strict Y=1). State on `BandCharacterStats` (`Max/Remaining ×2`, `TryConsumePlay`, `OnTurnPlayBudgetChanged`; D-ECON-2=A); central gate `GigManager.CanConsumePlay/TryConsumePlay`; maxima seeded from `GigFlowSettingsSO` defaults (both 1; D-ECON-5=A). Attribution for `AnyMusician` cards: fixed → hover → SelectedMusician (D-ECON-3=A). §9 gains a one-line pointer to §14.
- **Reference.** New design-rationale doc `planning/active/Design_Action_Economy_v1.md` (subordinate to §14; SSoT wins on divergence). Registered in `SSoT_INDEX.md`, `ssot_manifest.yaml`, `coverage-matrix.md`.
- **Semantic (starter).** `Design_Starter_Deck_v1.md` v1.2: all starter Inspiration costs set to 0 per **D-ECON-6=DEFER** — Warm Up, Mind Tap, Push It, Half Time, Key Lift cost 1→0 (gen unchanged). The "finisher" layer (cost > 0 cards) is designed but its card assignments are deferred to a future batch; finisher costs to be tuned in S5i.
- **Code-truth correction.** The plan's original song-start reset anchor (`GigPhase.SongPerformance` case) was corrected during implementation to `OnPlayPressed()` — the `SongPerformance` phase case is bypassed while `_session != null` (ExecuteGigPhase TEMP guard). Recorded in §14.3. `MusicianBase` unchanged; pips live on `BandCharacterCanvas` (prefabs under `Prefabs/UI/Canvases/` and `Prefabs/Characters/Musicians/`).
- **Decisions.** D-ECON-1=A (batch slot S5g→ECON-1→S5h), D-ECON-2=A, D-ECON-3=A, D-ECON-4=A, D-ECON-5=A, **D-ECON-6=DEFER** (all starter costs → 0; finisher designation deferred).
- **Smokes.** `ST-ECON-1..7` PASS (pips lit/dim/reset across a loop boundary; budget denies a second play in a period; Inspiration gate orthogonal; Dev-spawned cards still budgeted — T0b audit). Documentation-only close — no gameplay change in this pass.

---

## 2026-07-06 — S5g CLOSED (music variety) + ECON-1 opened

**Type:** lifecycle + semantic. Closes S5g (single close, D-S5gb-3=B); opens ECON-1 (per-turn play economy) as a batch inserted before S5h.

- **S5g locks.** D-AUTH-1=A (melody procedural: 5 `PhraseArchetypeSO` parametric + `PhrasePalette_SingingField` via `MelodyCardConfigSO.phrasePaletteOverride`), D-AUTH-2=B (4 drum palettes).
- **Authoring.** 20 new `DrumPatternData` (5 per palette; DSL zero-warnings) → FourOnTheFloor / WaltzLilt / OddMeterAngular / CompoundSwing at 6 entries each, under `ScriptableObjects/Patterns/Drums/<Palette>/`. Melody: 5 archetypes + palette in `.../Melody Tracks/MelodyCardConfigs/SingingField/`; the Singing Field card carrier is `Melody Configs/Melody Singing Field - Hook.asset` (MelodyCardConfigSO).
- **Card → palette bindings.** Authoritative table now in `SSoT_Card_System.md §5.2.1` (game-side; MidiGenPlay mirrors). Default Mode → FourOnTheFloor (asset fix from SyncopatedPocket), Waltz Protocol → WaltzLilt, Pentameter/Compound Cycle (reward pool) → OddMeterAngular/CompoundSwing, Wormus Minor/Major → Core Minor (6)/Core Major (8), Singing Field → PhrasePalette_SingingField (5). SyncopatedPocket unbound. D-TEMPO=null (Push It / Half Time carry no palette; PCE §6 Option A).
- **PCE-PROP resolved.** The `[GAP — UNVERIFIED] ALWTTT-PCE-PROP` stub in `CURRENT_STATE.md §1` is resolved (bindings final, Default Mode asset fixed, ST-1..5 subsumed by ST-S5g-1..5). **Reconciliation applied:** PCE-PROP's D3=A ("deterministic per build, package-threaded seed") is superseded in spirit by the seed-variety policy (Integration SSoT §10, MGP-ALWTTT-SEED-1) — the seed is for cross-song variety with intra-song stability, not per-build reproducibility.
- **Reference-only drift fix.** Drum-pattern asset path corrected in `Palette_Card_Identity_Design.md §9` (`Patterns/Drums/` per-palette sub-folders created in S5g).
- **Smokes.** `ST-S5g-1..5` **PASS** (cross-song variety audible in progression, 4/4 and 3/4 drums, hooks; intra-song B1 regression stable). Monotony #8 killed.
- **ECON-1 opened** (inserted before S5h): per-musician play economy (1 Action + 1 Composition per period; Inspiration intact as a "finisher" layer). Design 2026-07-06 with Matías, motivated by audience-test results. Closed 2026-07-07 — see entry above.

---

## 2026-07-05 — S5g (seed-wiring sub-batch): per-song render seed + MGP-ALWTTT-SEED-1 adopted

**Type:** lifecycle. S5g remains **open** — this entry records a wiring sub-batch, not a closure. Authoring (drum patterns × TS, melody Singing Field) and closing smokes `ST-S5g-1..5` are still outstanding.

- **Authority (cross-project).** `MGP-ALWTTT-SEED-1` filed, delivered, and adopted 2026-07-05, same day: MidiGenPlay added `int? seedOverride` to `GenerateSong`/`GenerateSinglePart`. Seed **policy** is ALWTTT truth (`SSoT_Runtime_CompositionSession_Integration.md §10`); the selection mechanism stays MidiGenPlay's. Boundary entry: `SSoT_ALWTTT_MidiGenPlay_Boundary.md §8.1`.
- **Semantic.** `CompositionSession` now derives one render seed per song in `Begin()` (run entropy), passes it to every `RenderSinglePart` call for that song, and clears it in `End()`. This replaces the accidental stability of the package's constant `defaultSeed` with an explicit contract: intra-song stable, cross-song varied.
- **Operational.** Wiring implemented in `MidiMusicManager.RenderSinglePart` + `CompositionSession`. Dev override `CompositionSession.DevPinnedSongSeed` added, code/debugger-only for now (`SSoT_Dev_Mode.md §8.7`; tab-wiring tracked in the new idea backlog, §16 of the same doc).
- **Decisions locked at this batch's open.** `D-S5g-2=A`, `D-S5g-4=A`, `D-S5g-5=A`, `D-S5g-7=C`, `D-S5gb-1=A` (one line each below). `D-S5g-1`, `D-S5g-3`, `D-S5g-6`, `D-S5g-8`, `D-S5gb-2`, `D-S5gb-3` were also locked at this batch's open per the handoff note, but their one-line resolutions were not present in this session's working context — recorded here as a gap, not fabricated; pull them forward from the batch-open record at the next docs pass.
  - `D-S5g-2=A` — a future MidiGenPlay post-generation per-loop micro-variation feature was noted as a forward possibility, not actioned this batch.
  - `D-S5g-4=A` — the six PCE-PROP paste-ready doc blocks apply at S5g's close, together with the authoring pass (see Part B of this doc-update package, deferred).
  - `D-S5g-5=A` — `S5i` reframed from win-rate-only tuning to gameplay-design-analysis + structured playtest, with win-rate as one output signal among several (`S5_DemoCutClose_Sub_Roadmap.md`).
  - `D-S5g-7=C` — deterministic anti-repeat declined package-side (MidiGenPlay's own D4); ALWTTT accepts probabilistic non-repetition with ≥6-entry palettes for the demo.
  - `D-S5gb-1=A` — the `trackInputsHash` stem-cache key is unchanged by the seed; cross-song isolation continues to rely on the `Begin()`/`End()` clear (Integration SSoT invariant 9), now runtime-verified.
- **Smoke status.** `ST-S5gb-1..5` all PASS. Three are described in the source package — cross-song seed variety (`ST-S5gb-1`), intra-song stability under re-render (`ST-S5gb-2`), runtime-verified cross-song cache isolation (`ST-S5gb-3`); `ST-S5gb-4`/`ST-S5gb-5` are recorded PASS but their individual assertions are not in this session's context. Closing smokes `ST-S5g-1..5` (authoring-dependent) remain outstanding.
- **Docs touched.** `SSoT_Runtime_CompositionSession_Integration.md` (new §10 + §3.1/§9 cross-refs), `SSoT_ALWTTT_MidiGenPlay_Boundary.md` (new §8.1), `SSoT_Dev_Mode.md` (§6 entry-points bullet + new §8.7 + new §16 idea backlog), `CURRENT_STATE.md` (§2/§3 active-work line + §4 open item), `S5_DemoCutClose_Sub_Roadmap.md` (S5g status note + S5i reframe).
- **Not touched this entry.** The `[GAP — UNVERIFIED] ALWTTT-PCE-PROP` stub in `CURRENT_STATE.md §1` resolves at S5g's close, together with Part B of this doc-update package — left untouched here on purpose. **Reconciliation note for that closure:** PCE-PROP's own `D3=A` ("determinism = deterministic per build, package-threaded seed") predates and is superseded in spirit by this batch's seed-variety policy (§10 of the Integration SSoT); the close-out pass should note that supersession explicitly rather than let the two stand as unreconciled claims about what the seed is for.
- **Placement deviation.** The idea-backlog item originally specified for `M1_5_Dev_Mode_Sub_Roadmap.md` was placed in `SSoT_Dev_Mode.md §16` instead — that roadmap is archived per `SSoT_INDEX.md` (superseded by this SSoT), and adding fresh planning content to an archived doc would silently reopen a retired planning surface.

---

## 2026-07-04 — S5f CLOSED: first-gig-shape riders + formal close

- **Operational (closure).** S5f formally closed. **ST-S5f-1..9 (dialogue) all PASS** (resolves the pending confirmation noted in the dialogue entry below) and **ST-S5f-R1..R9 (riders) all PASS**. Active focus advanced to S5g; `CURRENT_STATE.md §2/§3` flipped S5f→S5g (S5g opens with the mandatory boundary-scoping step).
- **Semantic (first-gig shape, D-REPLAN-4 fold-ins).** Four riders landed (`GigManager.cs`, `GigPresentationSO.cs`, `AudienceCharacterCanvas.cs`, `AudienceCharacterBase.cs` + 1 asset):
  1. Blocked "oscurito" hover tooltip on audience sprites (`AudienceCharacterBase.OnPointerEnter/Exit` + `AudienceCharacterCanvas.ShowBlockedTooltip`/`HideBlockedTooltip`; ESP hardcoded per D-S5f-7=A; **no status icon — M1.2 E3 intact**).
  2. `#if ALWTTT_DEV` gate on `GigManager.DevAddSongHype`/`DevResetSongHype` (#15) — stripped from non-dev builds. The Dev-Mode Gig-Wide Stats SongHype **slider** (`DevSetSongHypeAbsolute`, guarded by `GigDevSettingsSO.debugSongHype`) is a separate path and was already gated.
  3. `GigPresentationSO.ShowSongHypeBar` toggle gating the single `SetSongHypeVisible(true)` call site in `GigManager.OnPlayPressed` (D-S5f-6=B; demo asset OFF for gig 1). SongHype accrual, stage SFX, and song-end Vibe conversion unaffected — only the bar + C1 "L + SFX = N" readout hide.
  4. Telegraph effectiveness labels ESP (D-S5f-8=A, `AudienceCharacterCanvas.LabelFor`): SuperEffective → "¡Súper!", NotVeryEffective → "Resiste", Immune → "Inmune", Normal → "Normal".
- **Reference / home correction (D1=A).** The D-S5f-8 telegraph-label ESP note was filed to its authoritative home **`Design_Vibe_Telegraph_v0_1.md §4`**, not `SSoT_Scoring_and_Meters §6` — the `VibeEffectiveness` enum + effectiveness mapping live in the telegraph design doc; §6 owns only the L+SFX conversion math.
- **Docs touched.** `SSoT_Gig_Combat_Core §12` (GigPresentationSO concerns cell — `showSongHypeBar` visibility), `SSoT_Dev_Mode` (#15 gate note + slider/Add-Reset distinction), `SSoT_Status_Effects §3.2` (Blocked-legend addendum), `Design_Vibe_Telegraph_v0_1.md §4` (ESP labels), `S5_DemoCutClose_Sub_Roadmap` (S5f status → CLOSED), `CURRENT_STATE` (§2/§3 active pointer + B3-slate E-lite/#15 RESOLVED).
- **Anchor-drift note.** The riders' paste-ready doc edits (`S5f_Riders_Doc_Edits_2026-07-04.md`) were reconciled against live docs at apply time: #15 had no standalone bullet (annotated inline within the design-gaps-(4) list), the §12 `GigPresentationSO` cell was shorter than the edit assumed (S5f addition appended to the actual cell text), the SSoT_Dev_Mode target documented the SongHype **slider** not the Add/Reset pair (note reworded), and the telegraph-label edit was retargeted per D1=A. **Pre-existing (separate follow-up, NOT folded into S5f):** the §12 `GigPresentationSO` cell still omits the S5a SFX→FlatVibe / SongHype-stage-threshold concerns documented in §5.2/§5.3.5.

---

## 2026-07-04 — S5f (dialogue sub-batch): Spanish onboarding + dual tutorial catalog

- **Semantic.** Tutorial copy for `tut_first_audience_action`,
  `tut_first_song_end`, `tut_first_loop_inspiration` rewritten in EN + ES to
  the S5e inverted semantics (depleting Stress/Vibe pools; fixed
  inspiration-per-loop). No pre-inversion "fill/climb the bar" language
  remains in any authored copy.
- **Structural (minor).** `TutorialDialogCatalogSO` seeder split per
  language (`SeedDemoCutDialogsEN` / `SeedDemoCutDialogsES`, parameterized
  seed dir); new ES catalog asset + 11 ES dialog assets under
  `Assets/Resources/Data/Tutorial/Dialogs/ES/`; editor-only parity check
  menu (`ALWTTT/Tutorial/Validate catalog language parity`); dialog pages
  capped at 2 per trigger (D-S5f-5=B), rhetorical-cut authoring + auto-fit
  fallback. Runtime surface unchanged; trigger ids unchanged (persisted
  `firedDialogs` compatible).
- **Decisions.** D-S5f-1 (tú, condescending/reverent voice), D-S5f-2=B
  (dual catalog), D-S5f-3=B (tokens + track/character dialogs → S5f-ext),
  D-S5f-4=B (guided tutorial → post-demo), D-S5f-5=B (2-page cap,
  rhetorical cut, auto-fit as fallback). Ledger + voice rule:
  `Design_Tutorial_System_v0_1.md §5A`.
- **Smoke status.** ST-S5f-1..9 confirmation pending as of this entry —
  this entry records the authored/structural change, not a batch-closure
  claim; `CURRENT_STATE.md §2` active-work line is intentionally left
  unchanged pending that confirmation. **(Resolved 2026-07-04: ST-S5f-1..9
  all PASS; S5f closed and `CURRENT_STATE §2` advanced to S5g — see the
  S5f-close entry above.)**

## 2026-07-02 — S5e core-semantics inversion + S5e-ext visibility rider

Semantic: SSoT_Scoring_and_Meters §5/§6/§7.3 (+§3.2 note),
SSoT_Audience_and_Reactions §4.1–4.3, SSoT_Card_Authoring_Contracts
inspiration fields — plus a C=B consistency sweep propagating the inversion
through Scoring §2.3 and Audience §2 / §3 (model table: `VibeGoal` row →
`MaxVibe`) / §4 heading / §4.2 heading / §5.2 / §10 / §11, so the retired
`VibeGoal` and pre-inversion "progress" wording are removed project-doc-wide
within these files. Stress → depleting mental-fortitude pool (0 =
Breakdown); Vibe → depleting persuasion-resistance pool (0 = Convinced);
VibeGoal concept retired into MaxVibe. Inspiration economy: fixed 3/loop
(D2), `inspirationGenerated` content-deprecated (D3), `+INS` CardEffect
deferred. Composure/Flow/SongHype/LoopScore/Cohesion semantics unchanged
(D1). LoopScore complexity term deliberately inert (D-S5e-1=A, locked).

Operational: `PersistentGameplayData` musician-seed fix (Current now seeds
at Max, not 0). Meter-bar visibility policy (hidden-if-full,
visible-if-damaged, hover reveals) and card gen-badge auto-hide (S5e-ext,
same close).

Deferred (D-S5e-DOC-D, pending): the convince-condition inversion is **not**
yet reflected in `SSoT_Gig_Combat_Core.md` or `SSoT_Gig_Encounter.md` (both
still state `Vibe >= VibeGoal`), nor in `Design_Starter_Deck_v1.md` /
`ALWTTT_Combat_MVP_Audit_Final.md`. Code is already inverted, so this is a
docs-lag flagged in CURRENT_STATE §4, not a code divergence.

All 10 S5e smoke tests + 7 S5e-ext smoke tests passed.

## 2026-07-01 — S5 REPLANNING: tester-driven re-sequence of the demo-cut close (planning-only) + changelog rotation

**Type:** lifecycle / structural (planning). No code. **No authority / SSoT / contract
promotion** — the semantic edits this session sets up land in **S5e** at that batch's close.

**Context.** The demo cut is functionally built and got its first real tester round (Spanish
testers). S5c (win-rate tuning) was deferred in favour of core gameplay / UX / legibility work.
A one-session replanning clustered 11 tester findings and re-sequenced the remaining demo-cut
work. Framed throughout as improving the existing, playable build.

**Re-sequence** (see `planning/active/S5_DemoCutClose_Sub_Roadmap.md`, ledger **D-REPLAN-1..6**):
- Four new pre-tuning batches inserted: **S5e** (meter inversion + inspiration simplification) →
  **S5f** (Spanish onboarding + first-gig shape) → **S5g** (≥5 musical patterns per composition
  card) → **S5h** (end-of-gig reward screen).
- **S5c → S5i** (win-rate tuning; content intact, repositioned after the four new batches).
- **S5d → S5j** (§5.4 readiness + tag + close); its presentation half (reward screen) was pulled
  forward to S5h per the D-REPLAN-3 split.
- **D-REPLAN-1** success signal = unassisted comprehension + non-monotonous music, **not**
  win-rate.
- **D-REPLAN-5** Phase C entry unchanged: still opens on demo-cut close / §5.4 pass; the reward
  screen moving to S5h narrows S6's reward work to selection + multi-gig carry-over.
- Deferred out of the demo cut: cross-gig SFX unlock (#6b → S6); 3rd enemy (#9) + 2nd venue
  (#10) → Phase C S7 fast-follow (assets already exist, with a "how to author enemies/venues"
  doc each); design-idea backlog (per-character action+composition split, C2 "tank" ability,
  Sibi mind-read card, breakdown rebellion).

**Docs edited this session (all planning-only).** `CURRENT_STATE.md` §2/§3/§4;
`Roadmap_ALWTTT.md` §5.4/§5.5 (stale B3 + demo-cut-prep checkboxes flipped; S5e–S5i DoD items
added; SSoT-edit line corrected) + §7.1 (S6 reward-scope note);
`S5_DemoCutClose_Sub_Roadmap.md` (D-REPLAN ledger + S5e–S5j sections + re-sequenced diagram).

**Lifecycle — changelog rotation (D-DOC-ROTATE=C).** This file was rotated at this point. The
full **2026-03-18 → 2026-06-22** history (Governance migration through S5a) was archived
**verbatim** to `archive/changelog-ssot_2026-03-18_to_2026-06-22.md`, and this active file was
restarted with the milestone index above + go-forward entries. Rationale: the changelog had
grown to ~3,961 lines / ~350 KB; per governance §E it is the full semantic history, and §15.3
forensic value is preserved by the verbatim archive while the active file stays small and
scannable. A compressed *content* summary (rotation option B) was rejected as a drift vector
(a second, divergent representation of history). Active filename is unchanged, so
`SSoT_INDEX.md` / `coverage-matrix.md` need no edit; the supersession trail (governance §18.6)
lives in the archive header + this preamble + this entry.

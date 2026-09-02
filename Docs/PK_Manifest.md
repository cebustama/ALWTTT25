# PK_Manifest — ALWTTT Project Knowledge (Capa 2 · índice de contenido del PK)

**Versión 6 — 2026-08-27.** Refleja el PK **realmente observado**: **80 ficheros** (Fase 3-A ejecutada 2026-08-27; el plan de corte de PK-CUT-1 queda **completo**). Cambios en v6: **corregida §D.4** — los tres supuestos «fantasmas» eran renombrados punto→guion bajo, no ausencias; regla nueva sobre registrar ambos nombres. Cambios en v5: alta de `Repo_Tree_Index.md` (Capa 2, 445 rutas) · nueva regla §C.5 de regeneración · corregidas las rutas de los tres `README.md` renombrados para el PK. Cambios en v4: los 16 de Fase 3-A pasan de §A a **§B.5** · `Design_Composition_Variations_v0_1.md` y `MGP_Boundary_Index.md` comprometidos al repo (§D.1 actualizada). Cambios en v3: **todas las rutas de repo verificadas contra el árbol real** (`tree.txt`, 2026-08-27) — desaparece el marcador † de ruta inferida · marcados los ficheros que **solo existen en el PK** y no en el repo · nueva §D con el resultado del cruce. Cambios de v2 respecto de v1: filas de `LoopScoreCalculator.cs` y `CharacterStatusId.cs` corregidas tras el refresco del Paso 1 (§A.1 queda vacía) · alta de los 7 ficheros del lote **RFX-1** · alta de los dos índices de Capa 2 · los 3 ficheros escapados de las Fases 1–2 pasan a §B.4 · columna **Estado** en §A para no volver a confundir «está en el PK» con «debe seguir en el PK».

**Propósito.** Este fichero dice **qué hay en el PK, con qué procedencia, y qué se retiró y dónde vive ahora**, para que un chat pueda pedir por nombre lo que no está adjunto (File Request Protocol). **No es un documento gobernado por SSoT** — es operativa del PK. No define autoridad: la autoridad vive en `SSoT_INDEX.md` y `ssot_manifest.yaml`, que describen el **repo** (D-PK-0 = A: la pertenencia al PK es presupuesto de contexto, no gobernanza).



**Cómo leer la columna "Fecha de copia".** Ningún fichero ALWTTT del PK tiene fecha de copia registrada (los mtimes del PK están a epoch). La columna "Verificado-contra" da, en su lugar, el **último marcador de lote visible en el contenido** de la copia. Regla (instrucciones v2, puerta de frescura): si un lote cerrado tocó el fichero después de ese marcador, o no hay marcador, **refresco antes de usar como verdad de código**; si se procede sin refresco, toda conclusión se etiqueta "inferida de copia posiblemente desfasada".

**Convenciones.** Rutas `.cs` sin marca proceden de `ssot_manifest.yaml` `governs:`; con `†` son deducción del `namespace`, no verificación. Rutas MGP proceden de `MGP-20260810_MANIFEST.md`.

---

## §A — En el PK

### A.1 Refresco pendiente

**Uno, abierto 2026-08-26 (RFX-1/RFX-2) — ver la tabla al final de esta sección.**
La declaración «Ninguno» de 2026-08-27 se mantiene para las dos filas históricas.

Las dos filas que ocupaban esta sección se cerraron con el Paso 1 de PK-CUT-1 y se han
verificado contra el contenido real de los ficheros:

| Fichero | Estaba | Está | Verificación |
|---|---|---|---|
| `LoopScoreCalculator.cs` | firmas pre-M4.6F-2, sin `LoopScoringConfig`/`HypeThresholds` | `ComputeLoopScore(in LoopFeedbackContext, in LoopScoringConfig)` · `ComputeHypeDelta(float, in HypeThresholds)` | ✅ coincide con la llamada de `GigManager`; **F-R5c-2 cerrada** |
| `CharacterStatusId.cs` | faltaban 404, 504, 993 | `NegateIncomingPositive = 404` · `RedirectIncoming = 504` · `ResourceGenerationModifier = 992` · `ResourceCounter = 993` | ✅ **F-R5a-1 cerrada en su faceta de PK** |

> **Por qué esta sección existe y por qué vaciarla a tiempo importa.** El 2026-08-27, una
> consulta de rutina concluyó que `CharacterStatusId.cs` seguía sin `ResourceCounter = 993`
> y lo presentó como verificado contra el fichero. El fichero ya estaba refrescado; lo que
> estaba rancio era **esta fila**. Un índice desactualizado no es un hueco pasivo: bajo modo
> retrieval sustituye al fichero como fuente aparente. Regla derivada, en §C.1: **un refresco
> no está hecho hasta que su fila lo dice.**

#### Refrescos y cierres abiertos por RFX-1 / RFX-2 (2026-08-26)

| Fichero | Acción | Estado |
|---|---|---|
| `FloatingTextMidiListener.cs` | **Refrescar.** Modificado por RFX-1 (guards `showPercussionNotes` / `showDrumKick` / `logChords` + contadores de smoke). La copia del PK es anterior al lote. | **abierto** |
| `MidiEventInterfaces.cs` | **Petición CERRADA.** Solicitado cuatro veces durante RFX-1 y RFX-2 y adjuntado por fin el 2026-08-26. `ChordEvent.notes` es `List<int>`; `MidiTaggedEvent` y `ChordEvent` son `struct` de campos públicos. **Todas las inferencias de RFX-1 sobre ambas formas resultaron correctas.** No borrar esta fila al cerrarla: es la evidencia de que la inferencia fue segura, y el único registro de que el fichero **no** entra al PK (se lee por lote, ver §B). | cerrada |
| Los 5 ficheros del lote RFX-2 (`RhythmLane.cs`, `RhythmFxConfigSO.cs`, `RhythmParticleEmitter.cs`, `RhythmParticleMidiListener.cs`, `RhythmFxTester.cs`) | **No entran al PK.** Se mantiene la exclusión decidida en RFX-1 §6. `RhythmFxTester.cs` y `RhythmFxSandbox.unity` son dev-only y además están fuera del build. | sin acción |

#### Refrescos abiertos por WINK-1 (2026-08-31)

El lote modificó código cuyas copias viven en el PK. **Un refresco no está hecho hasta que su fila
lo dice** (§C.1), así que las filas de §A.2 quedan marcadas y estas son las acciones pendientes.

| Fichero | Acción | Estado |
|---|---|---|
| `StatusEffectContainer.cs` | **Refrescar.** El publisher de `StatusAppliedEvent` pasa `effect` como cuarto argumento. | **abierto** |
| `StatusEffectSO.cs` | **Refrescar.** Campo `applySfx` + accessor `ApplySfx`. | **abierto** |
| `HandController.cs` | **Refrescar.** `using ALWTTT.Sensory` + publish de `CardPerformedEvent` tras `PlayCardOneShotAnimation`. | **abierto** |
| `MusicianBase.cs` | **Refrescar + corregir clasificación.** `PlayCardAnimationRoutine` resuelve vía `MusicianCharacterData.ResolveCardAnimation`. Además figura en §B.2 como retirado a Capa 3 pero **está presente en el PK**: corregir la contradicción al refrescar. | **abierto** |
| `CharacterCanvas.cs` | **Solo corrección de clasificación.** No lo tocó WINK-1; misma contradicción retirado-pero-presente que `MusicianBase.cs`, pendiente desde antes del lote. | **abierto** |
| `GigManager.cs` | **No está en el PK** (Capa 3, §B.2 — 171 KB). Modificado por WINK-1 (publish de composición): se anota aquí para que el próximo lote que lo pida sepa que su copia de repo debe ser posterior al 2026-08-31. | sin acción de PK |
| `SensoryFxAdapter.cs` · `SensoryAudioAdapter.cs` · `SensoryFtPresentation.cs` · `CharacterSfxProfileSO.cs` · `MusicianCharacterData.cs` | **No entran al PK.** Modificados por WINK-1; se piden por lote (Capa 3). | sin acción |
| `CardPerformedEvent.cs` · `StatusVisualDriver.cs` | **Ficheros nuevos, no entran al PK.** Costuras pequeñas y estables; se piden por lote si un lote futuro las edita. Rutas: `Assets/Scripts/Sensory/` y `Assets/Scripts/Characters/`. | sin acción |

#### Discrepancia abierta — el conjunto `MGP-20260810_*` está en el PK

**Detectada al abrir RFX-2 (2026-08-26). Necesita veredicto; no dejar sin decidir.**

El prompt de rehidratación de RFX-2 especificaba los ficheros de bajo `MGP-20260810_*` como
**solo-chat**, «nunca al PK», conforme a la regla de Capa 3. **Están en el PK ahora mismo**:
`BasslineCardConfigSO`, `BassTrackComposer`, `PitchBendWriter`, `SSoT_Composer_Bass_Track` y
del orden de sesenta hermanos. O el manifiesto está desactualizado, o la regla de Capa 3 no
se aplicó a esa importación.

**No es cosmético.** El PK corre en modo **retrieval**, y sesenta ficheros del proyecto
compañero compiten en la búsqueda contra los documentos gobernados de ALWTTT en cualquier
consulta que comparta vocabulario — «chord», «track», «pattern», «velocity». Ésa es
exactamente la degradación que la regla de higiene de retrieval existe para evitar.

Decidir y registrar **una** de estas dos:

- **retirar** el conjunto `MGP-20260810_*` a Capa 3 y dar de alta sus filas de índice en
  `MGP_Boundary_Index.md`; o
- **eximirlo** explícitamente, con la razón, para que el siguiente lote no reabra la
  pregunta.

**Nota de procedencia útil para el veredicto:** estos ficheros fueron los que resolvieron
D-S2-BASS con evidencia en vez de con conjetura (frontera §8.12). El argumento para
retirarlos no es que fueran inútiles — es que su utilidad fue **por lote**, que es
precisamente la definición de Capa 3.

### A.2 Inventario

Columna **Estado**: `queda` = núcleo permanente · `Fase 3-A — retirar (Paso 7a)` = pendiente de
la última tanda del plan de corte · `Fase 3-B — sale al cerrar <lote>` = se retira en el cierre
de ese lote · `lote RFX-1 activo` = adjunto por un lote vivo.

| Fichero | Clase | Estado | Fecha de copia | Ruta en repo | Verificado-contra (lote/fecha) | Notas |
|---|---|---|---|---|---|---|
| `ALWTTT_Uses_MidiGenPlay_Quick_Path.md` | POR-LOTE | Fase 3-B — sale al cerrar frontera (3 KB) | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | reference frontera (3 KB) |
| `AudienceCharacterBase.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Characters/AudienceCharacterBase.cs | PRES-1 / D-R5-2 (2026-08-11): SelectDefaultMusicianTarget + SpotlightRedirectEvent presentes | ok · sin lote posterior conocido |
| `AudienceCharacterData.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Data/Characters/Audience/AudienceCharacterData.cs | sin marcas reconocibles | lotes posteriores posibles: R4 (Read the Room reveal) posible |
| `CSV-4b_Name_Lookup_Audit.md` | POR-LOTE | Fase 3-B — sale al cerrar CSV-4b | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | insumo de CSV-4b (lote vivo, queued) · **no existe en el repo — solo vive aquí; comprometer antes de retirarla** |
| `CSV_Composition_Validation_Sub_Roadmap.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | sub-roadmap activo (CSV-4b/5/6) |
| `CURRENT_STATE.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | baseline operativo |
| `CardBase.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Cards/CardBase.cs | R5 marker presente; JUICE-PW/CARD-UX-1 presentes | ok · sin lote posterior conocido |
| `CardDefinition.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Cards/CardDefinition.cs | R5-d (2026-08-26): resourceCostStatusKey/Amount presentes | ok — última marca = último lote de código · sin lote posterior conocido |
| `CardEffectSpec.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Cards/Effects/CardEffectSpec.cs | sin marcas (0.4 KB, base abstracta) | sin lote posterior conocido |
| `CharacterStatusId.cs` | PERMANENTE | queda | 2026-08-26 17:41 (refrescada) | Assets/Scripts/Status/CharacterStatusId.cs | **R4 / R5-a · verificada 2026-08-27** | contiene `NegateIncomingPositive = 404`, `RedirectIncoming = 504` (R4/D-R4-3=A), `ResourceGenerationModifier = 992`, `ResourceCounter = 993` (R5-a/Voltage). F-R5a-1 **cerrada en su faceta de PK**. |
| `DeckManager.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Managers/DeckManager.cs | CARD-UX-1 (2026-07-13): BuildAndGetCard + TutorialHighlight hook | lotes posteriores posibles: DF-CATALOG 2026-07-15 (unión de catálogos) — no verificable en la copia |
| `Design_Action_Economy_v1.md` | POR-LOTE | Fase 3-B — sale al cerrar D-ECON-GENERIC | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | D-ECON-GENERIC / S5i |
| `Design_Asset_Naming_v0_1.md` | POR-LOTE | Fase 3-B — sale al cerrar CSV-4b | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | CSV-4b (aplica la convención) |
| `Design_Composition_Variations_v0_1.md` | POR-LOTE | Fase 3-B — sale al cerrar R6 / R8 | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | DRAFT no gobernado, sin fila en INDEX (huérfano PK); insumo R6/R8 — registro es hallazgo para MANIFEST-2 · **no existe en el repo — solo vive aquí; comprometer a `Docs/planning/active/` antes de retirarla** |
| `Design_Demo_Cut_v1.md` | POR-LOTE | Fase 3-B — sale al cerrar S5i / S5j | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | S5i/S5j (frente vivo) |
| `Design_Game_And_Card_Maxims_v0_1.md` | POR-LOTE | Fase 3-B — sale al cerrar S5i | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | S5i lentes de observación |
| `Design_Project_Directives_v0_1.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | directivas standing D1–D3, aplican a toda sesión |
| `Design_Sensory_Contract_v0_1.md` | POR-LOTE | Fase 3-B — sale al cerrar MANIFEST-2 | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | D-SENSORY-HOME (MANIFEST-2); R7 floater exception |
| `Design_Starter_Deck_v1.md` | POR-LOTE | Fase 3-B — sale al cerrar CSV-6 | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | retained rationale (D10=B); CSV-6 cita §4; Sibi bassline flag |
| `Design_Starter_Deck_v2_DRAFT.md` | POR-LOTE | Fase 3-B — sale al cerrar R7 / R8 | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | R7/R8 |
| `Design_Track_Card_Levels_v0_1.md` | POR-LOTE | Fase 3-B — sale al cerrar R7 | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | R7 (spec) |
| `Design_Tutorial_System_v0_2.md` | POR-LOTE | Fase 3-B — sale al cerrar TUT-REFRESH | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | TUT-REFRESH |
| `Design_Vertical_Slice_v0_1.md` | POR-LOTE | Fase 3-B — sale al cerrar S6–S8 | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | S6–S8 (queued) |
| `Documentation_Update_Loop_Local_Addendum_v0_4.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | gobernanza operativa (addendum local) |
| `GigFlowSettingsSO.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Data/Gig/GigFlowSettingsSO.cs | R5-d (2026-08-26): MaxBonusLoopsPerPart presente | ok · sin lote posterior conocido |
| `GigRunContext.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Managers/GigRunContext.cs | sin marcas de lote reconocibles | sin lote posterior conocido |
| `HandController.cs` | PERMANENTE | queda | **2026-08-29 (refrescada, R5-g)** | Assets/Scripts/Controllers/HandController.cs | **R5-g / 2026-08-29** — contiene la puerta 2b.5 (`CanPayResourceCost`) y el helper `LogGate` | **Doble corrección de fila (R5-g).** La fila anterior decía `CARD-UX-1 (2026-07-13)` mientras la copia del PK ya contenía marcadores **R5-d**: el índice iba dos meses por detrás de su propio fichero, variante benigna del incidente de §A.1 y misma familia de fallo. Verificado además en R5-g que la copia del PK y la del repo eran **byte-idénticas salvo BOM** antes del lote (R5-f no tocó el fichero) |
| `ICompositionContext.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Music/Interfaces/ICompositionContext.cs | sin marcas (1 KB) | sin lote posterior conocido |
| `LoopFeedbackContext.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Music/Context Data/LoopFeedbackContext.cs | sin marcas reconocibles | lotes posteriores posibles: R5-c / HUD-COMP-1 posibles |
| `GigMessageUI.cs` | **NUEVO — Capa 3** | no entra al PK | creado 2026-09-01 (R6) | Assets/Scripts/UI/GigMessageUI.cs | R6 (D-R6-7) — creado en el lote, sin copia previa | stopgap de feedback de denegación; ~110 líneas. Pedir fresco si un lote de HUD lo toca |
| `LoopScoreCalculator.cs` | PERMANENTE | queda | 2026-08-26 17:41 (refrescada) | Assets/Scripts/Music/LoopScoreCalculator.cs | **R5-c / M4.6F-2 · verificada 2026-08-27** | firmas `ComputeLoopScore(in ctx, in LoopScoringConfig)` y `ComputeHypeDelta(float, in HypeThresholds)` presentes → coincide con la llamada de `GigManager`. F-R5c-2 **cerrada**. Abierto: dónde se aplica el factor Overload (`SSoT_Scoring_and_Meters` §… lo pone sobre el hypeDelta; el calculator no lo recibe ⇒ vive en el llamante). No verificable sin `GigManager.cs`. |
| `MGP-20260810_CURRENT_STATE.md` | PERMANENTE | queda | 2026-08-10 (export) · last-write 2026-08-08 19:23 | MidiGenPlay: Documentation~\CURRENT_STATE.md | MGP-20260810_MANIFEST.md (SHA registrado) | estado operativo del companion; fuente para preguntas de frontera |
| `MGP-20260810_Especificacion_Contenido_FaseA.md` | POR-LOTE | Fase 3-B — sale al cerrar CSV-6 / R8 | 2026-08-10 (export) · last-write 2026-07-28 17:33 | MidiGenPlay: Documentation~\planning\Especificacion_Contenido_FaseA.md | MGP-20260810_MANIFEST.md (SHA registrado) | planning de contenido; CSV-6 / R8 |
| `MGP-20260810_MANIFEST.md` | PERMANENTE | queda | 2026-08-10 (export) · last-write 2026-08-10 | MidiGenPlay: (MANIFEST.md — el propio manifiesto del espejo) | MGP-20260810_MANIFEST.md (SHA registrado) | procedencia del espejo (rutas, last-write, SHA) — imprescindible para pedir por nombre |
| `MGP-20260810_SSoT_CONTRACTS.md` | PERMANENTE | queda | 2026-08-10 (export) · last-write 2026-08-08 19:22 | MidiGenPlay: Documentation~\SSoT_CONTRACTS.md | MGP-20260810_MANIFEST.md (SHA registrado) | contratos del companion (8 KB) |
| `MGP-20260810_SSoT_INDEX.md` | PERMANENTE | queda | 2026-08-10 (export) · last-write 2026-07-24 09:25 | MidiGenPlay: Documentation~\SSoT_INDEX.md | MGP-20260810_MANIFEST.md (SHA registrado) | autoridad del companion (3 KB) |
| `MGP-20260810_SSoT_Runtime_Song_Model_and_Config.md` | PERMANENTE | queda | 2026-08-10 (export) · last-write 2026-07-21 13:49 | MidiGenPlay: Documentation~\runtime\SSoT_Runtime_Song_Model_and_Config.md | MGP-20260810_MANIFEST.md (SHA registrado) | modelo SongConfig = objeto de handoff; citado por Integration SSoT (6 KB) |
| `MGP-20260810_package.json` | PERMANENTE | queda | 2026-08-10 (export) · last-write 2026-07-21 13:49 | MidiGenPlay: package.json | MGP-20260810_MANIFEST.md (SHA registrado) | pin de versión del paquete (0.4 KB) |
| `MeterTuningSO.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Data/Gig/MeterTuningSO.cs | R1 (2026-07-23): captivatedVibeBonusPerStack presente | sin lote posterior conocido |
| `MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` | POR-LOTE | Fase 3-B — sale al cerrar R6 / R8 | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | referencia de integración; R6/R8 contenido de cartas |
| `MultiProject_Documentation_Governance_System_v0_4.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | gobernanza normativa |
| `PersistentGameplayData.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Data/Core/PersistentGameplayData.cs | S5h (2026-07-07): BuildRewardCardPool/GrantRewardCard/AnySfxUnlocked presentes | lotes posteriores posibles: DF-CATALOG 2026-07-15 — no verificable |
| `PinkTrombone_Voice_Levers.md` | POR-LOTE | Fase 3-B — sale al cerrar R6 | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | schema VoiceProfileSO; R6 (SingerVoiceDirector) |
| `Rehydration_Prompt_Guide.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | operativa de handoff entre chats |
| `Roadmap_ALWTTT.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | roadmap activo |
| `Roadmap_Audio.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | roadmap activo |
| `RosterExpansion_Sub_Roadmap.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | sub-roadmap activo (R5-d..R8) |
| `S5_DemoCutClose_Sub_Roadmap.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | sub-roadmap activo (S5i/S5j = frente vivo) |
| `SSoT_ALWTTT_MidiGenPlay_Boundary.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva (frontera) |
| `SSoT_Audience_and_Reactions.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Audio.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_CONTRACTS.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | contracts |
| `SSoT_Card_Authoring_Contracts.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Card_System.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Dev_Mode.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Editor_Authoring_Tools.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Gig_Combat_Core.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Gig_Encounter.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_INDEX.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | índice de autoridad |
| `SSoT_Runtime_CompositionSession_Integration.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Runtime_Flow.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Scoring_and_Meters.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Singer_Voice.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Status_Effects.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SongConfigBuilder.cs` | PERMANENTE | queda | no registrada · **verificada byte-idéntica al repo 2026-09-01 (R6, `diff` contra copia fresca; R6 NO la editó)** | Assets/Scripts/Music/SongConfigBuilder.cs | BAL-1 (2026-07-22): mixGains; DBG-C1: MusicianTrackKey; **R6 2026-09-01: sin cambios** | lotes posteriores posibles: R5-d render-scope solo (§8 inv 14) — no verificable en la copia |
| `StatusEffectCatalogueSO.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Status/StatusEffectCatalogueSO.cs | statusKey presente; sin marca posterior | sin lote posterior conocido |
| `StatusEffectContainer.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Status/Runtime/StatusEffectContainer.cs | R5-a (2026-08-21): SpendStacks + MaxStacks presentes | **RANCIA desde WINK-1 (2026-08-31)** — la copia del PK publica `StatusAppliedEvent` con 3 argumentos; el código real pasa 4 (`effect`). Refresco abierto en §A.1 |
| `StatusEffectSO.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Status/StatusEffectSO.cs | R5-pre (2026-08-11): SuggestKey presente | **RANCIA desde WINK-1 (2026-08-31)** — falta `applySfx` / `ApplySfx`. Refresco abierto en §A.1 |
| `changelog-ssot.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | historia semántica |
| `coverage-matrix.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | lookup de autoridad |
| `ssot_manifest.yaml` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | manifiesto (governs:) |
| `PK_Manifest.md` | PERMANENTE (Capa 2) | queda | 2026-08-27 (v2) | `Docs/PK_Manifest.md` | PK-CUT-1 / 2026-08-27 | este fichero; no gobernado — operativa del PK |
| `MGP_Boundary_Index.md` | PERMANENTE (Capa 2) | queda | 2026-08-26 | `Docs/MGP_Boundary_Index.md` | PK-AUDIT-1 / 2026-08-26 | índice del espejo MGP-20260810; no gobernado |
| `Repo_Tree_Index.md` | PERMANENTE (Capa 2) | queda | **snapshot 2026-08-27 15:43** | `Docs/Repo_Tree_Index.md` | generado por `make-tree-unity.ps1` | 445 rutas de `Assets/Scripts`, `Assets/Editor`, `Assets/PinkTrombonePOC`, `Docs`, `Packages` + docs de raíz. Excluye código de terceros (`MidiPlayer`, `com.merry-yellow.code-assist`). Avisa de homónimos (hoy: 9 × `README.md`). **Caduca** — ver §C.5 |
| `RhythmLane.cs` | POR-LOTE | lote **RFX-1** activo — sale al cerrar RFX-1/RFX-2 | adjuntada por el lote (sin fecha registrada) | Assets/Scripts/Enums/RhythmLane.cs | RFX-1 / 2026-08-26 (marca `RFX-1` en el fichero) | enum de carril rítmico; 1 KB |
| `RhythmFxConfigSO.cs` | POR-LOTE | lote **RFX-1** activo — sale al cerrar RFX-1/RFX-2 | adjuntada por el lote (sin fecha registrada) | Assets/Scripts/Data/RhythmFxConfigSO.cs | RFX-1 / 2026-08-26 (marca `RFX-1` en el fichero) | config SO del FX rítmico; 6 KB |
| `RhythmParticleEmitter.cs` | POR-LOTE | lote **RFX-1** activo — sale al cerrar RFX-1/RFX-2 | adjuntada por el lote (sin fecha registrada) | Assets/Scripts/Music/RhythmParticleEmitter.cs | RFX-1 / 2026-08-26 (marca `RFX-1` en el fichero) | emisor de partículas; 9 KB |
| `RhythmParticleMidiListener.cs` | POR-LOTE | lote **RFX-1** activo — sale al cerrar RFX-1/RFX-2 | adjuntada por el lote (sin fecha registrada) | Assets/Scripts/Music/RhythmParticleMidiListener.cs | RFX-1 / 2026-08-26 (marca `RFX-1` en el fichero) | puente evento MIDI → emisor; 12 KB |
| `RhythmFxTester.cs` | POR-LOTE | lote **RFX-1** activo — sale al cerrar RFX-1/RFX-2 | adjuntada por el lote (sin fecha registrada) | Assets/Scripts/Music/RhythmFxTester.cs | RFX-1 / 2026-08-26 (marca `RFX-1` en el fichero) | banco de pruebas del FX; 10 KB |
| `MidiEventInterfaces.cs` | POR-LOTE | lote **RFX-1** activo — sale al cerrar RFX-1/RFX-2 | adjuntada por el lote (sin fecha registrada) | Assets/Scripts/Music/MidiEventInterfaces.cs | RFX-1 / 2026-08-26 (marca `RFX-1` en el fichero) | seam de eventos MIDI que RFX-1 consume; **sin marca RFX-1** — preexistente; 3 KB |
| `PENDING_DOC_DIFFS_RFX-1.md` | POR-LOTE (paquete retenido) | lote **RFX-1** activo | 2026-08-26 | `Docs/pending/PENDING_DOC_DIFFS_RFX-1.md`† | RFX-1 sesión 1/2, cerrada 2026-08-26 | **D-DOC-5 cumplida**: la cabecera nombra consumidor (`RFX-1-DOC` o la fase doc de RFX-2) y declara la pérdida en ese cierre si ninguno abre · **no existe en el repo — solo vive aquí; comprometer a `Docs/pending/` antes de retirarlo (D-DOC-5)** |

---

## §B — Retirados del PK

Retirar del PK **no** es archivar, degradar ni borrar (D-PK-0 = A). Todo lo listado aquí conserva su clase de autoridad en el repo y se puede pedir para el chat de un lote.

Fecha de retirada: **EJECUTADO 2026-08-26/27** (PK-CUT-1, Fases 1 y 2 completas). La **Fase 3-A no se ha ejecutado**: sus 16 ficheros siguen en §A marcados «retirar (Paso 7a)».

### B.1 — Fase 1 (riesgo cero): paquetes consumidos, duplicados, históricos absorbidos

Cómo pedirlos: **no se piden** (paquetes consumidos / duplicados); los históricos se adjuntan al chat solo para re-litigar algo cerrado.

| Fichero | KB | Clase | Motivo / dónde vive el contenido ahora |
|---|---:|---|---|
| `ALWTTT_Combat_MVP_Audit_Final.md` | 36 | HISTÓRICO | archive (manifiesto); MVP cerrado |
| `ALWTTT_DeckEditorWindow_Roadmap_Proposal.md` | 26 | HISTÓRICO | archive; fases 0–6 completas |
| `ALWTTT_MidiGenPlay_Soundfont_Emulation_Report_2026-03-24.md` | 17 | HISTÓRICO | archive; carpeta planning/music no existe (F16) |
| `CSV-4c_Doc_Diffs.md` | 32 | REDUNDANTE | paquete consumido en DOC-APPLY-3 (retirada inmediata, decisión de apertura) |
| `DOC-APPLY-2_Application_Report_2026-08-08.md` | 27 | HISTÓRICO | informe de aplicación; absorbido por changelog 2026-08-08 (MANIFEST-1) |
| `Design_Tutorial_System_v0_1.md` | 29 | HISTÓRICO | archivado 2026-08-08 (D11=A); absorbido por v0_2 |
| `How_Successful_Roguelike_Deckbuilders_Are_Designed_and_Balanced.md` | 26 | HISTÓRICO | research absorbido en Design_Game_And_Card_Maxims (BALANCE-XREF) |
| `M1_5_Dev_Mode_Sub_Roadmap.md` | 11 | HISTÓRICO | archive; Fases 1–3 cerradas, 4–5 dropped; autoridad en SSoT_Dev_Mode |
| `MGP-20260810_ALWTTT_Melody_Authoring_Pipeline.md` | 8 | HISTÓRICO | pipeline 2026-03-18; superado por SSoT_Card_Authoring_Contracts + Integration |
| `MGP-20260810_Handoff_MGP_BAGGAGE_1.md` | 15 | HISTÓRICO | ask entregado y adoptado (CSV-4, 2026-07-20) |
| `MGP-20260810_Handoff_MGP_MIX_1.md` | 6 | HISTÓRICO | ask entregado y adoptado (BAL-1, 2026-07-22) |
| `MGP-20260810_Handoff_MGP_POCKET.md` | 6 | HISTÓRICO | ask entregado y adoptado (R2d SelfPocket, 2026-07-31) |
| `MGP-20260810_SSoT_CompositionCards_TrackStyleBundles.md` | 19 | REDUNDANTE | fuente mixta repartida entre Card_System / Integration / Boundary (INDEX tabla transicional); compite en retrieval |
| `MGP-20260810_SSoT_CompositionSystem_INDEX.md` | 2 | HISTÓRICO | índice cross-project 2026-04-14, previo a la gobernanza actual |
| `MGP-20260810_SSoT_Runtime_CompositionSession_Bridge.md` | 11 | REDUNDANTE | fuente promovida en Batch 03 → SSoT_Runtime_CompositionSession_Integration (INDEX tabla transicional); compite en retrieval |
| `MGP-20260810_changelog-ssot.md` | 207 | HISTÓRICO | historia del companion (206 KB) |
| `MelodyCardConfigSO.cs` | 1 | REDUNDANTE | duplicado byte-idéntico de MGP-20260810_MelodyCardConfigSO.cs |
| `MelodyPatternData.cs` | 6 | REDUNDANTE | duplicado byte-idéntico de MGP-20260810_MelodyPatternData.cs |
| `PENDING_DOC_DIFFS_HUD-COMP-1.md` | 11 | REDUNDANTE | paquete consumido en DOC-APPLY-3 (retirada inmediata, decisión de apertura) |
| `PENDING_DOC_DIFFS_R5d.md` | 22 | REDUNDANTE | paquete consumido en DOC-APPLY-3 (retirada inmediata, decisión de apertura) |
| `Report_CardLLM_Pipeline.md` | 10 | RETIRADO DEL PK | **la fila anterior era falsa** (decía "el fichero ya no existe en el repo", lápida F16). Verificado 2026-08-28 contra `Repo_Tree_Index.md` y contra el fichero mismo: vive en `Docs/reference/Report_CardLLM_Pipeline.md`, es referencia viva y se actualizó en DOC-APPLY-R5e. Fuera del PK por presupuesto de contexto, no por muerte; pedir por FRP cuando haga falta |
| `TUT-REBUILD_Sub_Roadmap.md` | 5 | HISTÓRICO | arco cerrado 2026-07-10; TUT-REFRESH será lote nuevo |
| `ssot-drift-auditor_SKILL.md` | 5 | REDUNDANTE | la skill ya está instalada en /mnt/skills/user/; copia en PK compite en retrieval |

### B.2 — Fase 2 · código ALWTTT fuera de núcleo (D-PK-1 = C, D-PK-4 = B)

Cómo pedirlos: **copia fresca del repo al chat del lote** que los toque (FRP paso 1); **nunca reescribir un fichero completo desde una copia del PK** (nota de proceso R4). Ruta `Assets/Scripts/<carpeta>/`, inferida del `namespace` (†, no verificada contra el árbol).

**Los cuatro seams grandes (D-PK-4 = B) — se piden en TODO lote de superficie S3 y en toda pregunta de frontera:**

| Fichero | KB | Ruta | Quién los pedirá |
|---|---:|---|---|
| `GigManager.cs` | 171 | Assets/Scripts/Managers/GigManager.cs | R5-d-SMOKE, ~~R6~~, R7, cualquier lote de turno/meters. **EDITADO en R6 (2026-09-01, D-R6-7):** tres llamadas a `GigMessageUI.Show` en las puertas ECON-1 / final-loop lock / coste de recurso. Toda copia anterior a esa fecha es RANCIA |
| `CompositionSession.cs` | 100 | Assets/Scripts/Music/CompositionSession.cs | R5-d-SMOKE, ~~R6~~, R7. **EDITADO en R6 (2026-09-01):** (a) F-R6-2 — el enrutado de parte se hoisteó por encima del gate de reglas en las **dos** rutas (`TryPlayCompositionCard` y la de `CardDefinition`), que ahora llaman a la sobrecarga de 4 args de `CanApplyDefinition`; (b) D-R6-7 — el `Fail(...)` local publica en `GigMessageUI` y se añadió `StripDenialPrefix`. Toda copia anterior es RANCIA |
| `MidiMusicManager.cs` | 144 | Assets/Scripts/Managers/MidiMusicManager.cs | R5-d-SMOKE (duck), R6, CSV-5, frontera |
| `SongCompositionUI.cs` | 61 | Assets/Scripts/UI/Song Composition/SongCompositionUI.cs | **EDITADO en R6 (2026-09-01):** `TryGetHarmonyDenial` (D-R6-4=A) + sobrecarga de 4 args de `CanApplyDefinition` con `_canApplyPartIndexOverride` / `PartUnderTest` (F-R6-2) + los dos puntos de llamada del gate. Toda copia anterior es RANCIA. R7, HUD follow-ups — sin marca HUD-COMP-1 en la copia retirada: pedir SIEMPRE fresca |

**Resto, agrupado por carpeta:**

**`(raíz/UI/Controllers)`** (2 ficheros · 13 KB): `ALWTTTProjectRegistriesSO.cs`, `GameplayData.cs`

**`? (sin namespace)`** (3 ficheros · 45 KB): `ChordProgressionCatalogueWizard.cs`, `FloatingTextMidiListener.cs`, `InstrumentRules.cs`

**`Backgrounds`** (4 ficheros · 11 KB): `BackgroundContainer.cs`, `BackgroundRoot.cs`, `ForegroundAnimator.cs`, `StageLightAnimator.cs`

**`Cards`** (17 ficheros · 31 KB): `ActionCardPayload.cs`, `BandDeckData.cs`, `BandDeckEntry.cs`, `CardAcquisitionFlags.cs`, `CardDefinitionDescriptionExtensions.cs`, `CardPerformerRule.cs`, `CardUI.cs`, `ChoiceCard.cs`, `CompositionCardClassifier.cs`, `CompositionCardPayload.cs`, `GenericCardCatalogSO.cs`, `InstrumentEffect.cs`, `ModulationEffect.cs`, `MusicianCardCatalogData.cs`, `MusicianCardEntry.cs`, `PartActionDescriptor.cs`, `TrackActionDescriptor.cs`

**`Cards/Editor`** (15 ficheros · 384 KB): `CardAssetFactory.cs`, `CardAuthoringNav.cs`, `CardEditorWindow.cs`, `CardEditorWindow_JsonImport.cs`, `CardEditorWindow_LLM.cs`, `CardInventoryWindow.cs`, `CardLLMVocabularyBuilder.cs`, `DeckAssetSaveService.cs`, `DeckCardCreationService.cs`, `DeckEditorDtos.cs`, `DeckEditorWindow.cs`, `DeckJsonImportService.cs`, `DeckValidationService.cs`, `MusicianCatalogService.cs`, `PartEffectEditorWindow.cs`

**`Cards/Editor/LLMAuthoring`** (9 ficheros · 80 KB): `CardImportDtoParser.cs`, `CardImportDtos.cs`, `CardLLMFieldPlan.cs`, `CardLLMGenerator.cs`, `CardLLMPromptBuilder.cs`, `CardLLMResponseHandler.cs`, `CardLLMVocabulary.cs`, `CardPaletteDescriptorScanner.cs`, `CardPaletteIntentResolver.cs`

**`Cards/Editor/LLMAuthoring/Tests`** (1 ficheros · 16 KB): `CardLLMResponseHandlerTests.cs`

**`Cards/Effects`** (8 ficheros · 14 KB): `AddInspirationPerLoopSpec.cs`, `ApplyStatusEffectSpec.cs`, `CardEffectDescriptionBuilder.cs`, `DrawCardsSpec.cs`, `GrantBonusLoopSpec.cs`, `ModifyStressSpec.cs`, `ModifyVibeSpec.cs`, `RevealPreferencesSpec.cs`

**`Characters`** (8 ficheros · 56 KB): `AudienceCharacterCanvas.cs`, `CharacterAnimator.cs`, `CharacterBase.cs`, `CharacterCanvas.cs`, `CharacterStats.cs`, `SpriteOutlineController.cs`, `StatusStats.cs`, `VibeEffectiveness.cs`

**`Characters/Actions | Cards/CardActions`** (11 ficheros · 23 KB): `AddStressAction.cs`, `AddVibeAction.cs`, `ApplyStatusEffectAction.cs`, `AudienceMoveToFrontAction.cs`, `BlockStressAction.cs`, `BlockVibeAction.cs`, `CharacterActionData.cs`, `CharacterActionParameters.cs`, `CharacterActionProcessor.cs`, `HealStressAction.cs`, `RemoveVibeAction.cs`

**`Characters/Audience`** (2 ficheros · 13 KB): `AudienceCharacterSimple.cs`, `AudienceCharacterStats.cs`

**`Characters/Band`** (6 ficheros · 43 KB): `BandCharacterCanvas.cs`, `BandCharacterStats.cs`, `MusicianBase.cs`, `MusicianCharacterData.cs`, `MusicianCharacterSimple.cs`, `MusicianProfileData.cs`

**`Data (F16)`** (1 ficheros · 2 KB): `AlwtttLogSetup.cs`

**`Data/*`** (14 ficheros · 60 KB): `AudienceIntentionData.cs`, `AudioMixSettingsSO.cs`, `CharacterSfxProfileSO.cs`, `CompositionFxConfigSO.cs`, `CompositionStripThemeSO.cs`, `DemoLaunchConfigSO.cs`, `GigDevSettingsSO.cs`, `GigPresentationSO.cs`, `GigSetupRosterSO.cs`, `MixGainProfileSO.cs`, `OstCatalogSO.cs`, `RewardDatabase.cs`, `SoundBankSO.cs`, `SpecialKeywordData.cs`

**`DevMode`** (8 ficheros · 142 KB): `DevAudioMixTab.cs`, `DevCardCatalogueTab.cs`, `DevCompositionDebugTab.cs`, `DevGigOutcomeTracker.cs`, `DevModeController.cs`, `DevRunTelemetryLogger.cs`, `DevStatsTab.cs`, `GenerationDebugFormatter.cs`

**`DevMode/Editor`** (2 ficheros · 106 KB): `CompositionInventoryWindow.cs`, `CompositionInventoryWindow_Cards.cs`

**`Encounters`** (2 ficheros · 5 KB): `GigEncounter.cs`, `GigEncounterSO.cs`

**`Enums`** (12 ficheros · 7 KB): `ActionTargetType.cs`, `AudioActionType.cs`, `CardType.cs`, `CharacterActionType.cs`, `MoodTag.cs`, `MusicianCharacterType.cs`, `OstTrackId.cs`, `RarityType.cs`, `RewardType.cs`, `SensorySfxType.cs`, `SpecialKeywords.cs`, `StatusType.cs`

**`Interfaces`** (1 ficheros · 1 KB): `IAudienceStats.cs`

**`Managers`** (6 ficheros · 50 KB): `AudioManager.cs`, `FxManager.cs`, `GigLauncher.cs`, `MainMenuController.cs`, `MusicDirector.cs`, `UIManager.cs`

**`Sensory`** (21 ficheros · 81 KB): `AudienceBlockedEvent.cs`, `AudienceReactionEvent.cs`, `AudienceTurnStartedEvent.cs`, `AudienceVibeImpactEvent.cs`, `CardPlayedEvent.cs`, `GigOutcomeEvent.cs`, `GigStartedEvent.cs`, `ISensoryEvent.cs`, `LoopResolvedEvent.cs`, `MusicianStressHitEvent.cs`, `PsychicWaveOverlayController.cs`, `RewardChoiceOpenedEvent.cs`, `SensoryAudioAdapter.cs`, `SensoryEventBus.cs`, `SensoryFtPresentation.cs`, `SensoryFxAdapter.cs`, `SensorySfxPresentation.cs`, `SfxStageCrossedEvent.cs`, `SongEndVibeEvent.cs`, `SpotlightRedirectEvent.cs`, `StatusAppliedEvent.cs`

**`Status`** (3 ficheros · 20 KB): `CharacterStatusPrimitiveDatabaseSO.cs`, `StatusEffectActionData.cs`, `StatusEffectInstance.cs`

**`Status/Editor`** (1 ficheros · 26 KB): `StatusEffectWizardWindow.cs`

**`Tutorial`** (14 ficheros · 130 KB): `TutorialController.cs`, `TutorialDialogCatalogSO.cs`, `TutorialDialogSO.cs`, `TutorialGuidedDriver.cs`, `TutorialHighlightSpawnHook.cs`, `TutorialHighlightTarget.cs`, `TutorialInputGate.cs`, `TutorialLoopHoldGate.cs`, `TutorialModalGate.cs`, `TutorialOptInPrompt.cs`, `TutorialOverlayView.cs`, `TutorialRevisitPanel.cs`, `TutorialScriptedDrawQueue.cs`, `TutorialTokenResolver.cs`

**`UI`** (17 ficheros · 146 KB): `AudiencePickerRow.cs`, `BeatPulseIndicator.cs`, `CardDetailViewController.cs`, `CompositionContextRowUI.cs`, `CompositionStripDriver.cs`, `FloatingText.cs`, `GigCanvas.cs`, `GigSetupController.cs`, `InventoryCanvas.cs`, `MinicardTooltipController.cs`, `MusicianPickerRow.cs`, `RewardCanvas.cs`, `SongPartElementUI.cs`, `SongTrackElementUI.cs`, `StatusIconBase.cs`, `TrackHoverPanel.cs`, `UIPulseAnimator.cs`

**`UI/Tooltips`** (6 ficheros · 11 KB): `EconPipTooltipTarget.cs`, `I2DTooltipTarget.cs`, `ITooltipTargetBase.cs`, `TooltipController.cs`, `TooltipManager.cs`, `TooltipText.cs`

**`paquete MGP`** (1 ficheros · 1 KB): `MIDITrack.cs`

Excepción de clase: `MIDITrack.cs` — FUERA-ALCANCE (tipo del paquete sin prefijo `MGP-`).

### B.3 — Fase 2 · bloque MidiGenPlay no-frontera (D-PK-2 = C, D-PK-5b = retirar)

Cómo pedirlos: por nombre `MGP-20260810_<nombre>` **solo para el chat**; ver `MGP_Boundary_Index.md`, columna «cuándo pedirlo». No reentran al PK.

| Fichero | KB | Clase | Ruta en MidiGenPlay |
|---|---:|---|---|
| `MGP-20260810_BackingCardConfigSO.cs` | 10 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\BackingCardConfigSO.cs` |
| `MGP-20260810_BassTrackComposer.cs` | 102 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Composers\BassTrackComposer.cs` |
| `MGP-20260810_BasslineCardConfigSO.cs` | 26 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\BasslineCardConfigSO.cs` |
| `MGP-20260810_ChordExpressionType.cs` | 10 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\ChordExpressionType.cs` |
| `MGP-20260810_ChordProgressionData.cs` | 15 | FUERA-ALCANCE | `Runtime\CoreScripts\Data\ChordProgressionData.cs` |
| `MGP-20260810_ChordProgressionLibrarySO.cs` | 2 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\ChordProgressionLibrarySO.cs` |
| `MGP-20260810_ChordProgressionPaletteSO.cs` | 3 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\ChordProgressionPaletteSO.cs` |
| `MGP-20260810_ChordProgressionRequality.cs` | 23 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\ChordProgressionRequality.cs` |
| `MGP-20260810_ChordProgressionRuntimeImporter.cs` | 39 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\ChordProgressionRuntimeImporter.cs` |
| `MGP-20260810_ChordQualityResolver.cs` | 8 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\ChordQualityResolver.cs` |
| `MGP-20260810_CompositionReadback.cs` | 8 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\CompositionReadback.cs` |
| `MGP-20260810_DrumPatternData.cs` | 9 | FUERA-ALCANCE | `Runtime\CoreScripts\Data\DrumPatternData.cs` |
| `MGP-20260810_DrumPatternPaletteSO.cs` | 4 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\DrumPatternPaletteSO.cs` |
| `MGP-20260810_DrumPatternTextParser.cs` | 15 | FUERA-ALCANCE | `Editor\DrumPatternTextParser.cs` |
| `MGP-20260810_HarmonyCardConfigSO.cs` | 0.3 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\HarmonyCardConfigSO.cs` |
| `MGP-20260810_IPatternRepository.cs` | 0.8 | FUERA-ALCANCE | `Runtime\CoreScripts\Interfaces\IPatternRepository.cs` |
| `MGP-20260810_ITrackComposer.cs` | 0.6 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Interfaces\ITrackComposer.cs` |
| `MGP-20260810_ITrackPatternConfigStore.cs` | 2 | FUERA-ALCANCE | `Runtime\CoreScripts\Interfaces\ITrackPatternConfigStore.cs` |
| `MGP-20260810_InstrumentRepositoryResources.cs` | 2 | FUERA-ALCANCE | `Runtime\CoreScripts\Services\InstrumentRepositoryResources.cs` |
| `MGP-20260810_MIDIInstrumentSO.cs` | 1 | FUERA-ALCANCE | `Runtime\CoreScripts\ScriptableObjects\MIDIInstrumentSO.cs` |
| `MGP-20260810_MIDIPercussionInstrumentSO.cs` | 3 | FUERA-ALCANCE | `Runtime\CoreScripts\ScriptableObjects\MIDIPercussionInstrumentSO.cs` |
| `MGP-20260810_MelodicLeadingConfig.cs` | 2 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\MelodicLeadingConfig.cs` |
| `MGP-20260810_MelodicStyleSO.cs` | 7 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\MelodicStyleSO.cs` |
| `MGP-20260810_MelodyCardConfigSO.cs` | 1 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\MelodyCardConfigSO.cs` |
| `MGP-20260810_MelodyPatternData.cs` | 6 | FUERA-ALCANCE | `Runtime\CoreScripts\Data\MelodyPatternData.cs` |
| `MGP-20260810_MidiGenPlayConfig.cs` | 6 | FUERA-ALCANCE | `Runtime\CoreScripts\Data\MidiGenPlayConfig.cs` |
| `MGP-20260810_ModulationOctaveHint.cs` | 0.7 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\ModulationOctaveHint.cs` |
| `MGP-20260810_PENDING_DOC_DIFFS.md` | 11 | FUERA-ALCANCE | `Documentation~\planning\PENDING_DOC_DIFFS.md` |
| `MGP-20260810_PatternDataSO.cs` | 0.3 | FUERA-ALCANCE | `Runtime\CoreScripts\Data\PatternDataSO.cs` |
| `MGP-20260810_PatternRepositoryResources.cs` | 4 | FUERA-ALCANCE | `Runtime\CoreScripts\Services\PatternRepositoryResources.cs` |
| `MGP-20260810_PhraseArchetypeSO.cs` | 0.6 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\Phrases\PhraseArchetypeSO.cs` |
| `MGP-20260810_PhrasePaletteSO.cs` | 1 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\Phrases\PhrasePaletteSO.cs` |
| `MGP-20260810_PitchBendWriter.cs` | 12 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Articulation\PitchBendWriter.cs` |
| `MGP-20260810_RhythmCardConfigSO.cs` | 6 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\RhythmCardConfigSO.cs` |
| `MGP-20260810_RomanProgressionParser.cs` | 20 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\RomanProgressionParser.cs` |
| `MGP-20260810_SSoT_Authoring_Chord_Progressions.md` | 34 | FUERA-ALCANCE | `Documentation~\authoring\SSoT_Authoring_Chord_Progressions.md` |
| `MGP-20260810_SSoT_Authoring_Melody_Composition.md` | 28 | FUERA-ALCANCE | `Documentation~\authoring\SSoT_Authoring_Melody_Composition.md` |
| `MGP-20260810_SSoT_Authoring_Rhythm_Patterns.md` | 21 | FUERA-ALCANCE | `Documentation~\authoring\SSoT_Authoring_Rhythm_Patterns.md` |
| `MGP-20260810_SSoT_Authoring_Tools.md` | 22 | FUERA-ALCANCE | `Documentation~\authoring\SSoT_Authoring_Tools.md` |
| `MGP-20260810_SSoT_Composer_Backing_Track.md` | 58 | FUERA-ALCANCE | `Documentation~\runtime\SSoT_Composer_Backing_Track.md` |
| `MGP-20260810_SSoT_Composer_Bass_Track.md` | 52 | FUERA-ALCANCE | `Documentation~\runtime\SSoT_Composer_Bass_Track.md` |
| `MGP-20260810_SSoT_Composer_Melody_Track.md` | 23 | FUERA-ALCANCE | `Documentation~\runtime\SSoT_Composer_Melody_Track.md` |
| `MGP-20260810_SSoT_Composer_Rhythm_Track.md` | 16 | FUERA-ALCANCE | `Documentation~\runtime\SSoT_Composer_Rhythm_Track.md` |
| `MGP-20260810_SSoT_Runtime_Generation_Orchestration.md` | 26 | FUERA-ALCANCE | `Documentation~\runtime\SSoT_Runtime_Generation_Orchestration.md` |
| `MGP-20260810_SongConfig.cs` | 6 | FUERA-ALCANCE | `Runtime\CoreScripts\Data\SongConfig.cs` |
| `MGP-20260810_SongOrchestrator.cs` | 77 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\SongOrchestrator.cs` |
| `MGP-20260810_TonalityProfileSO.cs` | 6 | FUERA-ALCANCE | `Runtime\CoreScripts\Data\TonalityProfileSO.cs` |
| `MGP-20260810_TrackPatternConfigStoreResources.cs` | 7 | FUERA-ALCANCE | `Runtime\CoreScripts\Services\TrackPatternConfigStoreResources.cs` |
| `MGP-20260810_TrackStyleBundleSO.cs` | 0.5 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\TrackStyleBundleSO.cs` |
| `MGP-20260810_VoiceLeadingConfig.cs` | 4 | FUERA-ALCANCE | `Runtime\CoreScripts\Composition\Data\VoiceLeadingConfig.cs` |
| `MGP-20260810_ssot_manifest.yaml` | 170 | FUERA-ALCANCE | `Documentation~\ssot_manifest.yaml` |

### B.4 — Escapados de las Fases 1–2, retirados el 2026-08-27

Detectados al reconciliar el PK tras el Paso 6 (98 observados vs 88 esperados).

| Fichero | KB | Fase que los cubría | Clase | Motivo |
|---|---:|:-:|---|---|
| `ssot-drift-auditor_SKILL.md` | 12 | 1 | REDUNDANTE | la skill ya está instalada en `/mnt/skills/user/`; la copia competía en retrieval |
| `How_Successful_Roguelike_Deckbuilders_Are_Designed_and_Balanced.md` | 55 | 1 | HISTÓRICO | research absorbido en `Design_Game_And_Card_Maxims` (BALANCE-XREF) |
| `GigEncounter.cs` | 8 | 2 | POR-LOTE | Assets/Scripts/Encounters/GigEncounter.cs

---

## §C — Reglas de mantenimiento

**C.1 Cuándo se actualiza este fichero.**
0. **Al refrescar cualquier copia `.cs`, en el mismo acto.** Sustituir el fichero y no tocar
   su fila deja el índice afirmando algo falso sobre un fichero correcto — y bajo retrieval el
   índice se recupera primero. **Un refresco no está hecho hasta que su fila lo dice.** (Regla
   añadida el 2026-08-27 tras el incidente descrito en §A.1.)
1. **En cada cierre de lote que cambie código.** Para cada fichero de §A.2 tocado por el lote: refrescar la copia en el PK **o** diferirlo explícitamente, y actualizar la fila (columna "Verificado-contra" = `<lote> / <fecha>`). *Un lote que cambió código no está cerrado hasta que esto se hace* (instrucciones v2, FRP paso 4). Los ficheros POR-LOTE adjuntos al chat se descartan al cerrar; no reentran al PK salvo decisión justificada (rara).
2. **Al abrir un lote que adjunta ficheros al PK** (como RFX-1): dar de alta sus filas en §A.2 con Estado = `lote <X> activo` **al abrir**, no al cerrar. Un fichero en el PK sin fila es un huérfano desde el minuto uno.
3. **En cada DOC-APPLY / sesión documental.** Revisar §A.2 filas de documentos: si un doc gobernado se creó, archivó o cambió de ruta, reflejarlo aquí (fila nueva en §A, o fila en §B con "dónde vive ahora"). Los paquetes de diffs consumidos pasan a §B el mismo día que se aplican.
4. **Al ejecutar cada Fase del plan de corte.** Cambiar "propuesta" → "EJECUTADO <fecha>" en §B; mover a §B lo que salga de Fase 3 al decidirse.
5. **Al crear, borrar o mover ficheros del repo:** regenerar `Repo_Tree_Index.md` (`make-tree-unity.bat`) y actualizar la fecha de su fila en §A.2. Un índice de rutas viejo hace pedir ficheros que ya no existen — la misma familia de fallo que una fila de copia rancia, un piso más abajo. Si el aviso de **nombres duplicados** crece, revisarlo antes de adjuntar cualquiera de esos ficheros: el PK es plano y no los distingue.
6. **Al re-exportar el espejo MGP.** Sustituir el bloque `MGP-*` de §A.2 en bloque (nuevo prefijo de fecha), actualizar `MGP_Boundary_Index.md` en la misma sesión, y anotar aquí el prefijo anterior como retirado.

**C.2 Regla D-DOC-5 (paquetes retenidos/pendientes).** Un paquete de diffs, informe de retorno o handoff que entre al PK o al chat lleva **en su propia cabecera** el lote que lo consume. Si ese lote cierra sin consumirlo, **se declara perdido en ese mismo cierre** (fila en §B con motivo "declarado perdido D-DOC-5 en <lote>"), no después. Este fichero es donde esa declaración queda visible; la narrativa va al changelog. Pendientes con dueño hoy: ver `PK_Audit_Report_2026-08-26.md` §4.3 (`CTX-2a_Doc_Diffs`, `CONT-B_Returns_MidiGenPlay`, `DEMO-FIXES-A_Doc_Diffs`, `CSV-4b_Name_Lookup_Audit` → CSV-4b).

**C.3 Higiene de retrieval.** No coexisten en el PK dos ficheros cuyo contenido compita en búsqueda (duplicados byte-idénticos, fuente superada + SSoT que la absorbió, dos versiones de un diseño). Si un documento nuevo absorbe a otro, el absorbido pasa a §B en la misma sesión. Nombres de fichero descriptivos; sin sufijos ambiguos (`_v2` vs `_v2_DRAFT` es un hallazgo abierto para MANIFEST-2).

**C.4 Qué NO decide este fichero.** Clase de autoridad, promoción/degradación, rutas canónicas del repo. Eso es `SSoT_INDEX.md` / `ssot_manifest.yaml` y se cambia en lotes MANIFEST-*.


---

## §D — Cruce contra el árbol real del repo (2026-08-27)

Fuente: `tree.txt` (volcado completo) y, desde v5, `Repo_Tree_Index.md` (445 rutas curadas), ambos de `make-tree-unity.ps1`. Sustituye a la inferencia por `namespace`: **todas las rutas de §A y §B son reales**, salvo las excepciones de abajo. De aquí en adelante se consulta el índice; este cruce completo no hay que repetirlo.

### D.1 — Ficheros que solo existen en el PK (riesgo de pérdida)

Estos **no están en el árbol del repo**. Retirarlos del PK sin comprometerlos antes los destruye.

| Fichero | Estado en el PK | Acción |
|---|---|---|
| `Design_Composition_Variations_v0_1.md` | §A, Fase 3-B | ✅ entregado 2026-08-27 para comprometer a `Docs/planning/active/` |
| `CSV-4b_Name_Lookup_Audit.md` | §A, Fase 3-B | comprometer (insumo de CSV-4b) |
| `PENDING_DOC_DIFFS_RFX-1.md` | §A, lote RFX-1 | comprometer a `Docs/pending/` — es un paquete retenido bajo D-DOC-5 |
| `MGP_Boundary_Index.md` | §A, Capa 2 | ✅ entregado 2026-08-27 para comprometer a `Docs/` |
| `PK_Audit_Report_2026-08-26.md` | fuera del PK | ✅ entregado 2026-08-27 para comprometer a `Docs/audits/` |

### D.2 — Retirados en Fase 1 que tampoco están en el repo (pérdida ya consumada)

| Fichero | Lectura |
|---|---|
| `ALWTTT_MidiGenPlay_Soundfont_Emulation_Report_2026-03-24.md` | la auditoría lo clasificó HISTÓRICO asumiendo `Docs/planning/music/`; esa carpeta no existe (F16). **Perdido** salvo copia local. |
| `How_Successful_Roguelike_Deckbuilders_Are_Designed_and_Balanced.md` | research; su contenido está absorbido en `Design_Game_And_Card_Maxims` (BALANCE-XREF), pero el original **no está en el repo**. |
| `DOC-APPLY-2_Application_Report_2026-08-08.md` | `Docs/archive/` contiene `DOC-APPLY-1_Application_Report_2026-07-31.md` pero **no el 2**. Asimetría: o nunca se comprometió, o se comprometió con otro nombre. |
| `CSV-4c_Doc_Diffs.md` · `PENDING_DOC_DIFFS_R5d.md` · `PENDING_DOC_DIFFS_HUD-COMP-1.md` | paquetes **consumidos** en DOC-APPLY-3. Su desaparición es la disposición correcta (D-DOC-5); no son pérdida. |

### D.3 — Ausencias esperadas (no son hallazgo)

- **Gobernanza de taller** — `MultiProject_Documentation_Governance_System_v0_4.md`, `Documentation_Update_Loop_Local_Addendum_v0_4.md`, `Rehydration_Prompt_Guide.md`, `ssot-drift-auditor_SKILL.md`: viven fuera del repo del juego.
- **Tipos del paquete MidiGenPlay** — `MelodyCardConfigSO.cs`, `MelodyPatternData.cs`, `MIDITrack.cs`: el árbol no contiene `Packages/midigenplay`, luego el paquete se referencia desde fuera del repo.

### D.4 — Renombrados para el PK plano (CORRECCION de v3)

**v3 declaro "fantasmas" a `CardEditorWindow_JsonImport.cs`, `CardEditorWindow_LLM.cs` y
`CompositionInventoryWindow_Cards.cs` por no encontrarlos en el arbol. Era falso.** Existen, con
punto en vez de guion bajo:

| Nombre en el PK | Ruta real en el repo |
|---|---|
| `CardEditorWindow_JsonImport.cs` | `Assets/Scripts/Cards/Editor/CardEditorWindow.JsonImport.cs` |
| `CardEditorWindow_LLM.cs` | `Assets/Scripts/Cards/Editor/CardEditorWindow.LLM.cs` |
| `CompositionInventoryWindow_Cards.cs` | `Assets/Scripts/Cards/Editor/CompositionInventoryWindow.Cards.cs` |

Son **clases parciales** renombradas al entrar al PK, igual que los tres `README.md`: el punto se
sustituyo por guion bajo. La deteccion de v3 comparaba nombre contra nombre y no contemplo el
renombrado, asi que dio ausencia donde hay equivalencia. R5-d modifico
`CardEditorWindow.JsonImport.cs`, lo que confirma que el fichero esta vivo.

**Regla derivada:** al adjuntar un fichero al PK con un nombre distinto del que tiene en el repo,
su fila de §A/§B debe registrar **ambos**. Un renombrado no documentado produce, segun quien
mire, un fichero fantasma o una peticion que nadie puede satisfacer. Renombrados conocidos hoy:
los tres de arriba y los tres `README.md` (§B.5).

### D.5 — Confirmación de la Fase 3-A

Los 16 ficheros de la Fase 3-A **sí existen en el repo** (`Docs/planning/`, `Docs/planning/active/`, `Docs/reference/`, `Docs/integrations/midigenplay/`, raíz para `CONTRIBUTING.md`, `Assets/` para los tres shaders). Su retirada es segura.

### B.5 — Fase 3-A, retirada el 2026-08-27

Diseños enviados, ideas registradas sin lote, READMEs de carpeta y shaders. **Los 16 verificados presentes en el repo** antes de retirar (§D.5).

| Fichero | Ruta en repo | Cómo pedirlo |
|---|---|---|
| `CONTRIBUTING.md` | `CONTRIBUTING.md` | adjuntar al chat si un lote futuro lo retoma |
| `Design_Audience_Status_v1.md` | `Docs/planning/active/Design_Audience_Status_v1.md` | adjuntar al chat si un lote futuro lo retoma |
| `Design_Composition_Debug_Tab_v0_1.md` | `Docs/planning/active/Design_Composition_Debug_Tab_v0_1.md` | adjuntar al chat si un lote futuro lo retoma |
| `Design_Fill_Window_v0_1.md` | `Docs/planning/Design_Fill_Window_v0_1.md` | adjuntar al chat si un lote futuro lo retoma |
| `Design_Pending_Effects_v1.md` | `Docs/planning/Design_Pending_Effects_v1.md` | adjuntar al chat si un lote futuro lo retoma |
| `Design_Singer_Expression_Input_v0_1.md` | `Docs/planning/Design_Singer_Expression_Input_v0_1.md` | adjuntar al chat si un lote futuro lo retoma |
| `Design_Song_Parts_Library_v0_1.md` | `Docs/planning/Design_Song_Parts_Library_v0_1.md` | adjuntar al chat si un lote futuro lo retoma |
| `Design_Tempo_Identity_v1.md` | `Docs/planning/Design_Tempo_Identity_v1.md` | adjuntar al chat si un lote futuro lo retoma |
| `Design_Vibe_Telegraph_v0_1.md` | `Docs/planning/Design_Vibe_Telegraph_v0_1.md` | adjuntar al chat si un lote futuro lo retoma |
| `MGP-20260810_coverage-matrix.md` | `(companion MidiGenPlay)` | adjuntar al chat si un lote futuro lo retoma |
| `PsychicWaveInvert.shader` | `Assets/Shaders/PsychicWaveInvert.shader` | adjuntar al chat si un lote futuro lo retoma |
| `SpriteOutlineURP.shader` | `Assets/Shaders/SpriteOutlineURP.shader` | adjuntar al chat si un lote futuro lo retoma |
| `TutorialSpotlight.shader` | `Assets/Shaders/TutorialSpotlight.shader` | adjuntar al chat si un lote futuro lo retoma |
| `integrations_midigenplay_README.md` | `Docs/integrations/midigenplay/README.md` | **renombrado para el PK** (el repo lo llama `README.md`; 9 homónimos — ver `Repo_Tree_Index.md`). Pedir por ruta, nunca por nombre |
| `planning_README.md` | `Docs/planning/README.md` | **renombrado para el PK**; pedir por ruta |
| `planning_active_README.md` | `Docs/planning/active/README.md` | **renombrado para el PK**; pedir por ruta |

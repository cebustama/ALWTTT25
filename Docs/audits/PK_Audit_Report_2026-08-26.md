# PK_Audit_Report_2026-08-26 — PK-AUDIT-1

**Mode:** DOCUMENTATION · **Batch:** PK-AUDIT-1 · **Fecha:** 2026-08-26
**Alcance:** auditoría del Project Knowledge (PK) y construcción de los dos índices de Capa 2. Cero ediciones a documentos gobernados, cero código, cero reclasificaciones de autoridad (D-PK-0 = A).
**Deliverables hermanos:** `PK_Manifest.md` · `MGP_Boundary_Index.md`.

---

## 0. Medición de partida y método

| | Ficheros | KB (disco) | % |
|---|---:|---:|---:|
| Código `.cs` ALWTTT | 220 | 2 305 | 40,7 |
| Documentos ALWTTT (`.md`, `.yaml`, `.shader`) | 72 | 1 996 | 35,2 |
| Bloque `MGP-20260810_*` | 67 | 1 365 | 24,1 |
| **Total** | **359** | **5 666** | 100 |

- Los **359 ficheros** coinciden con la medición de apertura. Los **KB difieren** (6,46 MB medidos en la UI vs 5,67 MB en disco): la UI cuenta con otra contabilidad (probablemente tokens o metadatos). Todo este informe usa **KB en disco** y expresa los ahorros como **% del baseline en disco**; la capacidad estimada se deriva proporcionalmente del 89 % medido (capacidad ≈ 89 % × restante/5 666).
- **Las marcas de tiempo del sistema de ficheros no sirven como procedencia:** los 359 ficheros del PK llevan mtime 1979-12-31 (epoch). Toda procedencia de esta sesión procede de (a) el `MGP-20260810_MANIFEST.md` para el bloque MGP (rutas, last-write y SHA por fichero) y (b) **marcadores de lote dentro del contenido** para los `.cs` de ALWTTT (`grep` de símbolos/comentarios introducidos por lotes fechados en el changelog).
- Clase por fichero: **exactamente una** de PERMANENTE · POR-LOTE · HISTÓRICO · FUERA-ALCANCE · REDUNDANTE. Ninguno queda sin clase (un "no sé" se habría reportado; no hubo).

### Reglas de clasificación aplicadas (derivadas de las decisiones de apertura)

- **PERMANENTE** = Capa 1 tal como la definen las instrucciones v2: gobernanza, SSoTs vivas, `CURRENT_STATE` / `changelog` / `coverage-matrix` / `SSoT_INDEX` / `ssot_manifest`, roadmaps activos, directivas standing, y el núcleo `.cs` (D-PK-4). Del bloque MGP, solo documentos de frontera (D-PK-2 = C).
- **POR-LOTE** = útil y vigente, pero se adjunta al chat del lote que lo necesita (File Request Protocol). Incluye todo `.cs` fuera del núcleo (D-PK-1 = C) y los documentos de diseño/referencia que un lote concreto cita.
- **HISTÓRICO** = archivado, absorbido o consumido; su contenido vive ya en changelog/SSoT. Retirar no pierde verdad.
- **FUERA-ALCANCE** = internals de MidiGenPlay (código y SSoTs de composers/authoring/orquestación), por regla de frontera.
- **REDUNDANTE** = duplicado byte-idéntico, paquete de diffs ya consumido, o documento superado que **compite en retrieval** con la SSoT que lo absorbió.

---

## 1. Clasificación completa (Tarea 0)

| Clase | Ficheros | KB | % del PK |
|---|---:|---:|---:|
| PERMANENTE | 54 | 1 782 | 31,4 |
| POR-LOTE | 231 | 2 600 | 45,9 |
| FUERA-ALCANCE | 51 | 744 | 13,1 |
| HISTÓRICO | 15 | 432 | 7,6 |
| REDUNDANTE | 8 | 108 | 1,9 |
| **Total** | **359** | **5 666** | 100 |

Desglose que explica dónde está la palanca (confirma la conclusión de apertura: los históricos son ~8 %, no la palanca; el código fuera de núcleo y MGP no-frontera son ~62 %):

| Segmento | Ficheros | KB |
|---|---:|---:|
| `.cs` ALWTTT · núcleo PERMANENTE (D-PK-4) | 19 | 304 |
| `.cs` ALWTTT · POR-LOTE (incl. los 4 "gigantes", 476 KB) | 198 | 1 992 |
| `.cs` ALWTTT · REDUNDANTE / FUERA-ALCANCE | 3 | 9 |
| Docs ALWTTT · PERMANENTE | 29 | 1 348 |
| Docs ALWTTT · POR-LOTE | 29 | 375 |
| Docs ALWTTT · HISTÓRICO | 9 | 188 |
| Docs ALWTTT · REDUNDANTE | 4 | 70 |
| MGP · PERMANENTE (frontera, D-PK-5) | 6 | 130 |
| MGP · `.cs` FUERA-ALCANCE | 40 | 452 |
| MGP · `.md` FUERA-ALCANCE (composers / authoring / orquestación) | 10 | 291 |
| MGP · HISTÓRICO | 6 | 244 |
| MGP · REDUNDANTE | 2 | 30 |
| MGP · POR-LOTE (Fase 3) | 3 | 219 |

La tabla fichero a fichero (359 filas, con KB, fase y motivo) está en el **Apéndice A** al final de este informe.

---

## 2. D-PK-4 — núcleo `.cs` permanente (Tarea 1)

### 2.1 Método del cruce

Para cada `.cs` del PK se cruzaron tres señales:

1. **¿Lo gobierna alguna SSoT?** — bloques `governs:` de `ssot_manifest.yaml` (MANIFEST-1, 2026-08-08). El PK es plano, así que la ruta de repo se recuperó del `namespace` de cada fichero (p. ej. `ALWTTT.Cards.Effects` → `Assets/Scripts/Cards/Effects`). Resultado (por namespace + ficheros nombrados explícitamente): **~124 de los 220 `.cs` del PK caen bajo algún `governs:`** (Cards/*, Status/*, DevMode/*, Characters/Actions, Music, Encounters, Data/Gig, Data/Audio, UI/Song Composition, y los managers/personajes nombrados uno a uno). **~96 no los gobierna ninguna SSoT**, y la lista es coherente con huecos ya conocidos: `Sensory/*` (21 — D-SENSORY-HOME, el contrato sensorial sigue siendo planning), `Tutorial/*` (14 — la autoridad de tutorial es un design doc de planning), `Tooltips/*` (6), `Backgrounds/*` (4), la mayoría de `Enums/*` (11) y `Data/*` no-Gig/Audio (8), `UI` general (9), `Managers` no nombrados (5: `UIManager`, `MainMenuController`, `GigLauncher`, `MidiMusicManager`, `FxManager`). Que **`MidiMusicManager.cs` no aparezca en ningún `governs:`** siendo el seam de frontera es un hallazgo para MANIFEST-2 (el boundary SSoT lo describe, pero su entrada de manifiesto no tiene bloque `governs:` porque es clase `contracts`). En cualquier caso, la señal "gobernado" no discrimina el núcleo por sí sola: gobierna a 124, y el núcleo debe ser ~20.
2. **¿Lo toca un lote vivo?** — R5-d-SMOKE (ST-R5d-1..15 sobre `CompositionSession`, `GigManager`, `MidiMusicManager`, `GigFlowSettingsSO`, `CardDefinition`, `GrantBonusLoopSpec`, `StatusEffectContainer`), CSV-4b (renamer / `CompositionInventoryWindow` — editor), CSV-5 (validación de bajo — frontera + `MidiMusicManager`), CSV-6 (contenido + `InstrumentRules`, catálogos), R6 (`CompositionSession`, `MidiMusicManager`, `SingerVoiceDirector` — no está en el PK), R7 (`TrackEntry`/`SongCompositionUI.TryAddOrReplaceTrackOnPart`, `CompositionSession`, caché), el lote D-R4-1 (superficie S2: `LoopFeedbackContext`, `AudienceCharacterData`, `LoopScoreCalculator`), TUT-REFRESH (Tutorial/*), S6–S8 (`PersistentGameplayData`, `AudienceCharacterBase` subclass boss).
3. **¿Es un seam estable o churn?** — frecuencia con que los lotes cerrados desde 2026-07 lo tocaron (changelog).

### 2.2 Hallazgo que reordena la decisión: los tres "seams" mayores son los que más churn tienen

`GigManager.cs` (170 KB), `CompositionSession.cs` (100 KB), `MidiMusicManager.cs` (143 KB) y `SongCompositionUI.cs` (61 KB) — **476 KB, 8,4 % del PK** — aparecen en la lista de candidatos de apertura, y el dato dice que **cada lote de código desde julio los ha tocado** (BAL-1, DBG-C1/C2, SINGER-1, R2c/R2d, R3/JAM, CTX-2a, R5-a/b/c, R5-d, HUD-COMP-1). Una copia permanente de estos cuatro es, por construcción, o **siempre desfasada** (y bajo RAG se recupera sola, con aspecto de verdad — el patrón F-R5a-1/F-R5c-2) o **refrescada en cada cierre** (y entonces no es una capa "estable" que recompense la caché). La copia actual de los cuatro está al día a 2026-08-26 salvo `SongCompositionUI` (§6), pero eso es el estado de hoy, no una propiedad del fichero.

**D-PK-4 — qué hay que decidir:** si los cuatro seams grandes forman parte del núcleo permanente o se piden en cada lote que los toque.

- **Opción A — núcleo de 23 (304 KB + los 4 gigantes = 780 KB, 13,8 % del PK).** Ventaja: una pregunta ad hoc de runtime/frontera se responde sin pedir nada. Coste: churn en cada cierre, o riesgo de copia desfasada recuperada por RAG.
- **Opción B — núcleo de 19 (304 KB, 5,4 % del PK); los 4 gigantes son POR-LOTE "siempre pedidos".** Ventaja: la capa permanente no churnea y el riesgo F-R5a-1 desaparece de los ficheros donde más daño hace. Coste: cualquier chat de superficie S3 abre pidiendo 3–4 ficheros (que ya pide hoy de facto, porque el FRP exige refresco).

**Recomendación: B.** El dato (churn en todos los lotes de código) es exactamente el criterio que las instrucciones v2 dan para excluir de Capa 1 ("cached reuse rewards a baseline that does not churn"). La pérdida de conveniencia ad hoc es real pero la absorbe el FRP; el riesgo de la opción A es el que el proyecto ya ha pagado dos veces.

### 2.3 Núcleo propuesto (Opción B) — 19 ficheros, 304 KB, un motivo por fichero

| Fichero | KB | Gobernado por | Lote vivo que lo toca | Motivo |
|---|---:|---|---|---|
| `HandController.cs` | 51 | Runtime_Flow | R5-d-SMOKE (overlay), TUT-REFRESH | seam mano→jugar; sondeo de playability CARD-UX-1 |
| `DeckManager.cs` | 34 | Runtime_Flow | TUT-REFRESH (spawn hook) | pipeline mazo/mano; `BuildAndGetCard` |
| `CardBase.cs` | 37 | Card_System | R5-d-SMOKE (resourceCost en play), R7 | `ExecuteEffects` + publish sensorial + overlay |
| `CardDefinition.cs` | 7 | Card_System · Authoring_Contracts | R5-d-SMOKE, R7, CSV-4b | contrato de datos de carta; par `resourceCost*` (R5-d) |
| `CardEffectSpec.cs` | 0,4 | Authoring_Contracts | — | base abstracta de todos los `*Spec` |
| `SongConfigBuilder.cs` | 18 | Integration | R7, R6 | frontera ALWTTT→`SongConfig` |
| `ICompositionContext.cs` | 1 | Integration | — | seam de contexto de composición |
| `GigFlowSettingsSO.cs` | 9 | Gig_Combat_Core §12 | R5-d-SMOKE (`MaxBonusLoopsPerPart`) | config de flujo de gig |
| `GigRunContext.cs` | 6 | Gig_Combat_Core | S6 (run flow) | alcance de run |
| `MeterTuningSO.cs` | 3 | Scoring · Gig_Combat_Core | S5i tuning, D-R4-1 | superficie de tuning (Captivated, thresholds) |
| `LoopScoreCalculator.cs` | 2 | Scoring · Integration | D-R4-1 (S2), R5-c factor | **copia desfasada — ver §6** |
| `LoopFeedbackContext.cs` | 5 | Integration (Context Data) | D-R4-1 (S2) | entrada del scoring |
| `StatusEffectContainer.cs` | 9 | Status_Effects | R5-d-SMOKE (`SpendStacks`) | invariante SO-container |
| `StatusEffectSO.cs` | 13 | Status_Effects | R6/R8 (estados nuevos) | schema del estado |
| `StatusEffectCatalogueSO.cs` | 11 | Status_Effects | ídem | catálogo SO = invariante del proyecto |
| `CharacterStatusId.cs` | 2 | Status_Effects (contrato de ids) | R6/R8 | **copia desfasada — ver §6** |
| `PersistentGameplayData.cs` | 49 | (sin SSoT explícita; §1 CURRENT_STATE) | R8 rewards, S6 | reward pool / estado de run |
| `AudienceCharacterData.cs` | 8 | Audience | D-R4-1 (S2) | datos de audiencia / gustos |
| `AudienceCharacterBase.cs` | 30 | Audience | S7 state machine, S8 boss | runtime de audiencia; targeting D-R5-2 |

**Podados respecto a la lista de apertura:** `GigManager`, `CompositionSession`, `MidiMusicManager`, `SongCompositionUI` (→ POR-LOTE "siempre pedidos", §2.2). **Añadidos:** `MeterTuningSO`, `LoopScoreCalculator`, `LoopFeedbackContext`, `AudienceCharacterData` (superficie S2 del lote D-R4-1 — "la deuda más pesada"), `StatusEffectSO`, `StatusEffectCatalogueSO`, `CharacterStatusId` (el trío que materializa el invariante SO-container), `GigRunContext`, `CardEffectSpec`, `ICompositionContext` (seams diminutos, coste nulo).

**Se queda fuera aunque "suene" a núcleo, con motivo:** `GrantBonusLoopSpec.cs` (R5-d, pero es un `*Spec` entre nueve — se pide con R5-d-SMOKE) · `CharacterBase.cs` / `MusicianBase.cs` / `CharacterStats.cs` / `BandCharacterStats.cs` (gobernados por Gig_Combat_Core, pero ningún lote vivo los toca) · `GigPresentationSO.cs` / `GigDevSettingsSO.cs` (config, sin lote vivo) · todo `Tutorial/*` (TUT-REFRESH los pedirá en bloque) · todo `Cards/Editor/*` y `DevMode/*` (editor, sin runtime; CSV-4b los pide) · todo `Sensory/*` (presentación; superficie S1 cerrada).

---

## 3. D-PK-5 — documentos de frontera MGP (Tarea 2)

Veredicto fichero a fichero sobre los **25 `.md` + `.yaml` + `.json`** del bloque. Criterio: se queda lo que hace falta para **saber qué pedir** y para responder una pregunta de frontera desde el lado ALWTTT; sale todo lo que describe internals (presunción FUERA) o está superado por una SSoT de ALWTTT (REDUNDANTE).

| Fichero | KB | Veredicto | Motivo |
|---|---:|---|---|
| `MGP-20260810_MANIFEST.md` | 9 | **QUEDA** | procedencia del espejo (ruta, last-write, SHA por fichero); es lo que permite pedir por nombre |
| `MGP-20260810_SSoT_INDEX.md` | 3 | **QUEDA** | autoridad del companion |
| `MGP-20260810_SSoT_CONTRACTS.md` | 8 | **QUEDA** | contratos del companion |
| `MGP-20260810_CURRENT_STATE.md` | 100 | **QUEDA** | primera parada de toda pregunta de frontera; es el único doc MGP que dice qué asks están entregados |
| `MGP-20260810_SSoT_Runtime_Song_Model_and_Config.md` | 6 | **QUEDA** | `SongConfig` es el objeto de handoff; lo cita el Integration SSoT dos veces |
| `MGP-20260810_package.json` | 0,4 | **QUEDA** | pin de versión (1.2.0 en BAL-1) |
| `MGP-20260810_ssot_manifest.yaml` | 170 | **D-PK-5b — Fase 3, recomendación FUERA** | 170 KB (43 % del bloque MGP restante) de invariantes cacheados de internals; su única función de frontera —saber qué doc gobierna qué— la asume `MGP_Boundary_Index.md` |
| `MGP-20260810_coverage-matrix.md` | 32 | Fase 3, recomendación FUERA | navegación del companion; el índice cubre el caso de uso |
| `MGP-20260810_changelog-ssot.md` | 206 | HISTÓRICO | historia del companion; solo sirve para fechar un cambio package-side |
| `MGP-20260810_PENDING_DOC_DIFFS.md` | 10 | FUERA-ALCANCE | deuda documental del companion |
| `MGP-20260810_SSoT_Composer_Backing_Track.md` | 58 | FUERA-ALCANCE | internals de composer (lo cita 4× el boundary SSoT: por eso **se indexa** con "cuándo pedirlo") |
| `MGP-20260810_SSoT_Composer_Bass_Track.md` | 52 | FUERA-ALCANCE | ídem (CSV-5 lo pedirá) |
| `MGP-20260810_SSoT_Composer_Melody_Track.md` | 23 | FUERA-ALCANCE | internals |
| `MGP-20260810_SSoT_Composer_Rhythm_Track.md` | 15 | FUERA-ALCANCE | internals |
| `MGP-20260810_SSoT_Authoring_Chord_Progressions.md` | 34 | FUERA-ALCANCE | authoring package-side (CSV-6 / CONT-B lo pedirán) |
| `MGP-20260810_SSoT_Authoring_Melody_Composition.md` | 27 | FUERA-ALCANCE | authoring package-side |
| `MGP-20260810_SSoT_Authoring_Rhythm_Patterns.md` | 21 | FUERA-ALCANCE | authoring package-side |
| `MGP-20260810_SSoT_Authoring_Tools.md` | 21 | FUERA-ALCANCE | tooling package-side (el más citado desde ALWTTT: 5×; se indexa) |
| `MGP-20260810_SSoT_Runtime_Generation_Orchestration.md` | 25 | FUERA-ALCANCE | orquestación interna |
| `MGP-20260810_SSoT_Runtime_CompositionSession_Bridge.md` | 11 | REDUNDANTE | `SSoT_INDEX.md` (tabla transicional): "source promoted in Batch 03" → `SSoT_Runtime_CompositionSession_Integration`; compite en retrieval con ella |
| `MGP-20260810_SSoT_CompositionCards_TrackStyleBundles.md` | 19 | REDUNDANTE | ídem: "mixed source doc — split across card/runtime/boundary docs" |
| `MGP-20260810_SSoT_CompositionSystem_INDEX.md` | 2 | HISTÓRICO | índice cross-project de 2026-04-14, previo a la gobernanza actual |
| `MGP-20260810_ALWTTT_Melody_Authoring_Pipeline.md` | 8 | HISTÓRICO | guía de 2026-03-18; hoy `SSoT_Card_Authoring_Contracts` |
| `MGP-20260810_Handoff_MGP_BAGGAGE_1.md` | 15 | HISTÓRICO | ask adoptado (CSV-4, 2026-07-20; boundary §8.2) |
| `MGP-20260810_Handoff_MGP_MIX_1.md` | 6 | HISTÓRICO | ask adoptado (BAL-1, 2026-07-22; boundary §8.3) |
| `MGP-20260810_Handoff_MGP_POCKET.md` | 6 | HISTÓRICO | ask adoptado (R2d, 2026-07-31; boundary §8.4–8.6) |
| `MGP-20260810_Especificacion_Contenido_FaseA.md` | 16 | Fase 3, POR-LOTE | planning de contenido; CSV-6 / R8 |

**Resolución D-PK-5:** quedan **6 ficheros, 130 KB** (de 1 365). **D-PK-5b** (el `ssot_manifest.yaml` de 170 KB) se deja en Fase 3 con recomendación FUERA porque el mínimo esperado de apertura lo incluía y es una decisión de 3 % del PK; es reversible sin coste (se pide por nombre).

**Nota sobre el `.cs` de frontera:** D-PK-2 = C saca los 40 `.cs`. Dos de ellos son literalmente el **contrato de datos** que cruza la frontera —`MGP-20260810_SongConfig.cs` (5 KB) y `MGP-20260810_CompositionReadback.cs` (8 KB)—. Se respeta la decisión (salen), pero el índice los marca como "PRIMERO ante cualquier pregunta de hash / campos del handoff". Si en dos lotes se piden los dos cada vez, es dato para revisar D-PK-2 (no se revisa aquí).

---

## 4. Huérfanos en ambas direcciones (Tarea del deliverable)

### 4.1 En el PK, sin fila en ningún índice (`SSoT_INDEX` / `ssot_manifest` / `coverage-matrix`)

| Fichero | Situación | Qué hacer (hallazgo, no acción de esta sesión) |
|---|---|---|
| `Design_Composition_Variations_v0_1.md` (14 KB) | DRAFT no gobernado, autodeclarado "planning/active" sin confirmar contra el árbol; cita un `Prompt_MGP_CardExpressivity_Companion_v0_1.md` que tampoco está | **MANIFEST-2**: registrar o archivar; hasta entonces POR-LOTE (R6/R8) |
| `CSV-4b_Name_Lookup_Audit.md` | insumo de lote, no documento gobernado | POR-LOTE hasta que CSV-4b lo consuma; entonces D-DOC-5 aplica |
| `PsychicWaveInvert.shader` · `SpriteOutlineURP.shader` · `TutorialSpotlight.shader` | código, no doc; ningún `governs:` cubre `*.shader` | POR-LOTE; F16-style: el manifiesto no gobierna shaders (hallazgo menor para MANIFEST-2) |
| `MultiProject_Documentation_Governance_System_v0_4.md` · `Documentation_Update_Loop_Local_Addendum_v0_4.md` · `Rehydration_Prompt_Guide.md` | gobernanza de taller, no del repo | esperado; PERMANENTE. **Pero**: las instrucciones del proyecto citan `Documentation_Update_Loop_Template.md` (#2 de la autoridad de gobernanza) y `DOC-SALVAGE-SSoT-Workflow_DSSW_v0_2.md` (#4), y **ninguno está en el PK** — el Addendum local v0_4 está, la plantilla base no |
| `DOC-APPLY-2_Application_Report_2026-08-08.md` · los tres paquetes de diffs · `ssot-drift-auditor_SKILL.md` | informes/paquetes/skill | HISTÓRICO / REDUNDANTE (Fase 1) |

### 4.2 Nombrados por índice / manifiesto / `CURRENT_STATE`, y **no** están en el PK

| Fichero | Quién lo nombra | Lectura |
|---|---|---|
| `Docs/README.md` (autoridad raíz, "authority order" que el manifiesto dice copiar literalmente) | manifiesto, SSoT_INDEX | ausencia esperada bajo presupuesto, pero es el origen de `authority_order` — candidato a PERMANENTE si es pequeño |
| `reference/CSO_Primitives_Catalog.md` | manifiesto (reference), SSoT_INDEX | referencia viva de primitivas; POR-LOTE (R6/R8 estados nuevos) |
| `archive/absorbed/Source_Docs_Supersession_Map.md` · `archive/SNAPSHOT_RETENTION_POLICY.md` | manifiesto, SSoT_INDEX | archivo; ausencia correcta |
| `planning/archive/Roadmap_Combat_MVP*.md` · `Combat_MVP_Roadmap.md` | manifiesto (archive) | ausencia correcta |
| `Design_Starter_Deck_v2.md` | `SSoT_INDEX` fila R0 y `RosterExpansion` §… lo citan **sin** `_DRAFT`; manifiesto y PK dicen `_DRAFT` | **inconsistencia de nombre** — hallazgo para MANIFEST-2 (¿existe un v2 sin DRAFT en el repo, o la fila de INDEX está mal?) |
| `Assets/Scripts/Data/Audio/VoiceProfileSO.cs` | manifiesto `governs:` (Singer_Voice) | gobernado, no en PK; POR-LOTE (R6) — solo es una pregunta de PK |
| `Assets/Scripts/Cards/Payloads/CardPayload.cs` | manifiesto `governs:` (Authoring_Contracts) | gobernado, no en PK; el PK tiene `ActionCardPayload` / `CompositionCardPayload` — existencia del fichero base **no verificada** |
| `GigSetupSceneManager.cs` | manifiesto (Gig_Encounter) | **borrado 2026-05-18** (F16, confirmado en MANIFEST-1); la entrada del manifiesto sigue siendo lápida |

### 4.3 La firma de D-DOC-5 — paquetes/entregables referenciados como pendientes o debidos que **no están** en el PK

| Fichero | Quién lo nombra | Estado según el propio `CURRENT_STATE` | Disposición debida |
|---|---|---|---|
| `CTX-2a_Doc_Diffs_2026-08-03.md` | `CURRENT_STATE` §5 "**Pendiente de aplicar**" (10 diffs, inv 13 BPM, §12.4) | **Contradicción interna**: DOC-APPLY-3 registra que inv 13 = BPM **ya existe** en `SSoT_Runtime_CompositionSession_Integration` §8, luego al menos parte de CTX-2a se aplicó y §5 no se podó — o se aplicó parcialmente | declarar en el próximo cierre: aplicado (podar §5) o perdido (D-DOC-5) |
| `CONT-B_Returns_MidiGenPlay_2026-07-31.md` | `CURRENT_STATE` §5 "Still owed"; CSV §5 y D9 apuntan a él | entregable cross-project **nunca producido** | D-DOC-5: nombrar lote consumidor (CSV-5) o declararlo no-deliverable |
| `DEMO-FIXES-A_Doc_Diffs_2026-07-15.md` | `CURRENT_STATE` §2/§3 ("see the sub-roadmap + …") | no consta aplicado ni retirado | D-DOC-5 |
| `Prompt_MGP_CardExpressivity_Companion_v0_1.md` | `Design_Composition_Variations_v0_1` | prompt de sesión companion | no es PK; se nombra para que no quede "pendiente sin dueño" |
| `TrackRole.cs` · `MGP-TRIAGE-ALWTTT-R3_host_resolutions.md` · `Handoff_Contenido_FaseB_ALWTTT.md` | `MGP-20260810_MANIFEST.md` §"Not found" | el export de 2026-08-10 **no los encontró** | pedir al companion si un lote los necesita (`TrackRole.cs` es la enum de la clave compuesta — R7/CSV-5 la necesitarán) |
| `LLM_Projects_Setup_Guide.md` §18A | el prompt de esta sesión ("si está adjunta") | no adjunta | la mecánica RAG/caché se ha aplicado por inferencia; nada de este informe depende de su texto |

---

## 5. Plan de corte por fases (Tarea 5)

Orden por (KB liberados ÷ riesgo). Cada fase se ejecuta **completa** y se registra en `PK_Manifest.md` §B antes de pasar a la siguiente; el informe está completo antes de tocar nada (constraint de apertura).

| Fase | Contenido | Ficheros | KB | Restante | % baseline | Capacidad est. |
|---|---|---:|---:|---:|---:|---:|
| — | Partida | 359 | — | 5 666 | 100 | 89 % |
| **1 — riesgo cero** | REDUNDANTE (8) + HISTÓRICO (15): paquetes consumidos, duplicados byte-idénticos, skill duplicada, archivos/absorbidos, handoffs adoptados, changelog MGP | 23 | 540 | 5 126 | 90,5 | **~81 %** |
| **2 — D-PK-1/2** | 198 `.cs` ALWTTT fuera de núcleo (incl. los 4 gigantes, Opción B) + 1 `.cs` FUERA + 40 `.cs` MGP + 10 `.md` MGP de internals | 249 | 2 736 | 2 390 | 42,2 | **~38 %** |
| **3 — discutible** | 29 docs de diseño/referencia POR-LOTE + `Design_Composition_Variations` + 3 MGP POR-LOTE (`ssot_manifest.yaml` D-PK-5b, `coverage-matrix`, `Especificacion_FaseA`) | 33 | 608 | 1 782 | 31,4 | **~28 %** |

**Lo que queda tras las tres fases (1 782 KB, 54 ficheros)** = 29 docs de gobernanza/SSoT/roadmap + 19 `.cs` de núcleo + 6 docs de frontera MGP. Es la Capa 1 tal como la definen las instrucciones v2, más los dos índices de Capa 2 (que se suben al aplicar Fase 1: ~30–40 KB).

### Fase 1 — lista literal (ejecutable sin más decisiones)

23 ficheros · 540 KB

- `ALWTTT_Combat_MVP_Audit_Final.md` (36 KB) — archive (manifiesto); MVP cerrado
- `ALWTTT_DeckEditorWindow_Roadmap_Proposal.md` (26 KB) — archive; fases 0–6 completas
- `ALWTTT_MidiGenPlay_Soundfont_Emulation_Report_2026-03-24.md` (17 KB) — archive; carpeta planning/music no existe (F16)
- `CSV-4c_Doc_Diffs.md` (32 KB) — paquete consumido en DOC-APPLY-3 (retirada inmediata, decisión de apertura)
- `DOC-APPLY-2_Application_Report_2026-08-08.md` (27 KB) — informe de aplicación; absorbido por changelog 2026-08-08 (MANIFEST-1)
- `Design_Tutorial_System_v0_1.md` (29 KB) — archivado 2026-08-08 (D11=A); absorbido por v0_2
- `How_Successful_Roguelike_Deckbuilders_Are_Designed_and_Balanced.md` (26 KB) — research absorbido en Design_Game_And_Card_Maxims (BALANCE-XREF)
- `M1_5_Dev_Mode_Sub_Roadmap.md` (11 KB) — archive; Fases 1–3 cerradas, 4–5 dropped; autoridad en SSoT_Dev_Mode
- `MGP-20260810_ALWTTT_Melody_Authoring_Pipeline.md` (8 KB) — pipeline 2026-03-18; superado por SSoT_Card_Authoring_Contracts + Integration
- `MGP-20260810_Handoff_MGP_BAGGAGE_1.md` (15 KB) — ask entregado y adoptado (CSV-4, 2026-07-20)
- `MGP-20260810_Handoff_MGP_MIX_1.md` (6 KB) — ask entregado y adoptado (BAL-1, 2026-07-22)
- `MGP-20260810_Handoff_MGP_POCKET.md` (6 KB) — ask entregado y adoptado (R2d SelfPocket, 2026-07-31)
- `MGP-20260810_SSoT_CompositionCards_TrackStyleBundles.md` (19 KB) — fuente mixta repartida entre Card_System / Integration / Boundary (INDEX tabla transicional); compite en retrieval
- `MGP-20260810_SSoT_CompositionSystem_INDEX.md` (2 KB) — índice cross-project 2026-04-14, previo a la gobernanza actual
- `MGP-20260810_SSoT_Runtime_CompositionSession_Bridge.md` (11 KB) — fuente promovida en Batch 03 → SSoT_Runtime_CompositionSession_Integration (INDEX tabla transicional); compite en retrieval
- `MGP-20260810_changelog-ssot.md` (207 KB) — historia del companion (206 KB)
- `MelodyCardConfigSO.cs` (1 KB) — duplicado byte-idéntico de MGP-20260810_MelodyCardConfigSO.cs
- `MelodyPatternData.cs` (6 KB) — duplicado byte-idéntico de MGP-20260810_MelodyPatternData.cs
- `PENDING_DOC_DIFFS_HUD-COMP-1.md` (11 KB) — paquete consumido en DOC-APPLY-3 (retirada inmediata, decisión de apertura)
- `PENDING_DOC_DIFFS_R5d.md` (22 KB) — paquete consumido en DOC-APPLY-3 (retirada inmediata, decisión de apertura)
- `Report_CardLLM_Pipeline.md` (10 KB) — archive; el fichero ya no existe en el repo (lápida F16)
- `TUT-REBUILD_Sub_Roadmap.md` (5 KB) — arco cerrado 2026-07-10; TUT-REFRESH será lote nuevo
- `ssot-drift-auditor_SKILL.md` (5 KB) — la skill ya está instalada en /mnt/skills/user/; copia en PK compite en retrieval

Riesgo residual de Fase 1 = ninguno que afecte a un lote vivo: `TUT-REBUILD_Sub_Roadmap.md` (arco cerrado; TUT-REFRESH es lote nuevo y se pedirá con `Design_Tutorial_System_v0_2`) y `Design_Tutorial_System_v0_1.md` (archivado D11=A) son los dos únicos que podrían discutirse; ambos absorbidos por v0_2.

### Fase 2 — recomendación de ejecución

- Ejecutar **después** de que `PK_Manifest.md` y `MGP_Boundary_Index.md` estén en el PK (si no, el primer chat que abra tras el corte no sabrá qué pedir).
- Los 4 gigantes salen con Fase 2 (Opción B). Si el usuario elige la Opción A al cerrar, se quedan y el manifiesto §A los lista con la fila de refresco obligatorio en cada cierre.
- **No sale nada que un lote vivo necesite sin poder pedirlo:** todo `.cs` retirado queda con ruta en §B del manifiesto (inferida del namespace, marcada como tal) y todo MGP con ruta real del export.

### Fase 3 — caso a caso (recomendación por fichero en el Apéndice A, columna Fase = 3)

- **FUERA (recomendado):** `MGP-20260810_ssot_manifest.yaml` (170), `MGP-20260810_coverage-matrix.md` (32), los long-term/registrados (`Design_Pending_Effects`, `Design_Tempo_Identity`, `Design_Song_Parts_Library`, `Design_Fill_Window`, `Design_Singer_Expression_Input`), `Design_Vibe_Telegraph` (shipped), `Design_Audience_Status_v1` (parcial, §3/§5 superados), `Design_Composition_Debug_Tab` (R1/R2a shipped), los tres README de carpeta, `CONTRIBUTING.md`, los tres `.shader`.
- **QUEDAN hasta que su lote cierre (recomendado):** `Design_Demo_Cut_v1` + `Design_Game_And_Card_Maxims` (S5i/S5j frente vivo), `Design_Track_Card_Levels` + `Design_Starter_Deck_v2_DRAFT` (R7/R8), `Design_Asset_Naming` + `CSV-4b_Name_Lookup_Audit` (CSV-4b), `Design_Tutorial_System_v0_2` (TUT-REFRESH), `Design_Sensory_Contract` (D-SENSORY-HOME / MANIFEST-2), `Design_Vertical_Slice` (S6–S8), `Design_Action_Economy` (D-ECON-GENERIC), `Design_Starter_Deck_v1` (CSV-6 cita §4; flag Sibi bassline), `MidiGenPlay_Expressive_Surface` + `PinkTrombone_Voice_Levers` + `Design_Composition_Variations` (R6/R8), `ALWTTT_Uses_MidiGenPlay_Quick_Path` (3 KB), `MGP-20260810_Especificacion_Contenido_FaseA` (CSV-6/R8).
- Ahorro de la sub-lista FUERA ≈ 300 KB (~5 %); la sub-lista QUEDAN se retira lote a lote al cerrar cada uno (regla §C del manifiesto).

---

## 6. Lista de refresco del núcleo (Tarea 6) — SEPARADA de la retirada

Regla de las instrucciones v2: sin procedencia registrada, o con un lote cerrado posterior a la copia, **refresco antes de usar como verdad de código**. Ningún fichero ALWTTT del PK tiene fecha de copia registrada, así que la puerta se aplica a todo el núcleo; lo que discrimina es la **evidencia de desfase** encontrada en el contenido.

### 6.1 Desfase demostrado — refrescar ANTES de cualquier uso (3)

| Fichero | Evidencia | Consecuencia si se usa como está |
|---|---|---|
| `LoopScoreCalculator.cs` | La copia expone `ComputeLoopScore(in LoopFeedbackContext)` y `ComputeHypeDelta(float)`. **El `GigManager.cs` del mismo PK** (línea ~2554) llama `ComputeLoopScore(loopCtx, meters.LoopScoringConfig)` y `ComputeHypeDelta(loopScore, meters.HypeThresholds)`. La copia es **anterior a M4.6F-2 (2026-05-07)** y no lleva el factor Overload de R5-c. Contiene además un U+FFFD (HELD-2). Es F-R5c-2, "AÚN sin refrescar" en apertura — **confirmado por firma**. | el lote D-R4-1 (S2) razonaría sobre una función que no existe; cualquier propuesta de tuning de scoring partiría de la fórmula equivocada |
| `CharacterStatusId.cs` | La copia **no contiene** `NegateIncomingPositive = 404` ni `RedirectIncoming = 504` (R4, 2026-08-10) ni `ResourceCounter = 993` (R5-a, 2026-08-21). El changelog (F-R5a-1) dice "refrescada en R5-a": **ese refresco no aterrizó en el PK** (o una copia anterior lo sobrescribió). `CharacterStatusPrimitiveDatabaseSO.cs` sí los tiene — es exactamente el desfase enum-vs-registro que la lección de F-R5a-1 describe. | una auditoría de ids libres para R6/R8 volvería a proponer un id ocupado — la regresión de R4 evitada por compilación, repetida |
| `SongCompositionUI.cs` (POR-LOTE, gigante) | Última marca visible `[DF-INSPLOOP]` (2026-07-16); **ninguna marca HUD-COMP-1** aunque `CompositionStripDriver.cs` y `SongTrackElementUI.cs` del PK sí la llevan, y `SSoT_Gig_Combat_Core` §15 nombra a `SongCompositionUI` como modelo editable de la tira. No es prueba (HUD-COMP-1 pudo no editar este fichero), es sospecha fundada. | R7 (`TryAddOrReplaceTrackOnPart`, level-up) parchearía sobre una base posiblemente movida |

### 6.2 Sin evidencia de desfase, sin procedencia — verificar por `diff` en el primer lote que los toque y **registrar la fecha en el manifiesto** (16)

`HandController` (CARD-UX-1 visible; DF-* posterior no verificable) · `DeckManager` (ídem; DF-CATALOG) · `PersistentGameplayData` (S5h visible; DF-CATALOG) · `SongConfigBuilder` (BAL-1/DBG-C1 visibles; el solo de alcance de render de R5-d pudo tocarlo) · `LoopFeedbackContext` · `AudienceCharacterData` · `StatusEffectCatalogueSO` · `GigRunContext` · `MeterTuningSO` (R1 visible) · `CardEffectSpec` · `ICompositionContext` · y, con última marca = último lote de código (**estado "ok" hoy**, no propiedad estable): `CardDefinition` (R5-d) · `GigFlowSettingsSO` (R5-d) · `StatusEffectContainer` (R5-a) · `StatusEffectSO` (R5-pre) · `AudienceCharacterBase` (PRES-1/D-R5-2) · `CardBase` (R5).

Los tres gigantes que salen (`GigManager`, `CompositionSession`, `MidiMusicManager`) llevan marcas R5-d / HUD-COMP-1 (2026-08-26): **al día hoy**; irrelevante tras Fase 2 porque se piden frescos.

### 6.3 Bloque MGP

Procedencia completa y verificable (export 2026-08-10, last-write y SHA por fichero). Desfase conocido desde este lado: cualquier cambio package-side posterior al 10-08 (p. ej. la sesión paralela MGP-ARTIC-RATE-1, el reenvío MGP-TONALITY-1 del 11-08) **no está** en el espejo. Los 6 que quedan son documentos de estado; se refrescan re-exportando el espejo, nunca fichero a fichero.

### 6.4 Hallazgo lateral — U+FFFD

`grep -lP '\xef\xbf\xbd'` sobre el PK devuelve **61 ficheros** (`.cs`/`.shader`; 21 de ellos MGP; incluye `LoopScoreCalculator.cs`, `CardDefinition.cs`, `DeckManager.cs`, `GigFlowSettingsSO.cs`). El ítem abierto HELD-2 (`CURRENT_STATE` §4) dice que solo se inspeccionaron 5. **Caveat:** el PK puede haber introducido el daño en la copia; el barrido válido es el del repo con el comando de HELD-2. Es insumo, no prueba.

---

## 7. Hallazgos para otros lotes (no acciones de esta sesión)

1. **MANIFEST-2:** registrar/archivar `Design_Composition_Variations_v0_1`; resolver `Design_Starter_Deck_v2` vs `_DRAFT`; el manifiesto no gobierna `*.shader`; lápida `GigSetupSceneManager.cs`.
2. **D-DOC-5 (próximo cierre que corresponda):** disposición de `CTX-2a_Doc_Diffs_2026-08-03.md` (§5 lo lista pendiente; inv 13 ya existe), `CONT-B_Returns_MidiGenPlay_2026-07-31.md`, `DEMO-FIXES-A_Doc_Diffs_2026-07-15.md`.
3. **HELD-2:** los 61 ficheros del §6.4 como lista de partida del barrido en repo.
4. **MANIFEST-2 (manifiesto):** ~96 `.cs` del PK sin `governs:` (Sensory 21, Tutorial 14, Tooltips 6, Backgrounds 4…); `MidiMusicManager.cs` sin bloque `governs:` pese a ser el seam de frontera (la entrada del boundary SSoT es clase `contracts` y no lleva `governs:`).
5. **Gobernanza:** las instrucciones v2 citan `Documentation_Update_Loop_Template.md` y `DOC-SALVAGE-SSoT-Workflow_DSSW_v0_2.md`; ninguno está en el PK. Decidir si entran (Capa 1, pequeños) o si el Addendum local basta.
6. **F-R5a-1 reabierta en su faceta de PK:** el refresco registrado como hecho no está en el PK (§6.1).

---

## 8. Doc-update de esta sesión — PROPUESTA (aplicar solo tras confirmación)

**Clase:** operational + structural. **Ningún documento gobernado cambia de autoridad, clase o contenido semántico.**

### 8.1 `changelog-ssot.md` — nueva entrada (arriba, newest-first)

```
## 2026-08-26 — PK-AUDIT-1: auditoría del Project Knowledge y transición al modelo de tres capas

Sesión solo documental (DOCUMENTATION). Sin código, sin smokes, sin cambios de autoridad
(D-PK-0 = A: retirar del PK no es gobernanza; SSoT_INDEX.md y ssot_manifest.yaml describen el REPO).

**Qué se hizo.** Los 359 ficheros del PK (5 666 KB en disco; 89 % de capacidad) clasificados en
PERMANENTE 54 / POR-LOTE 231 / HISTÓRICO 15 / FUERA-ALCANCE 51 / REDUNDANTE 8. Construidos los
dos índices de Capa 2: `PK_Manifest.md` (§A en PK, §B retirados, §C reglas) y
`MGP_Boundary_Index.md` (67 ficheros del espejo MGP-20260810 con ruta, last-write y "cuándo
pedirlo"). Plan de corte en tres fases: Fase 1 (23 ficheros, 540 KB, riesgo cero) → ~81 %;
Fase 2 (249, 2 736 KB, D-PK-1/2) → ~38 %; Fase 3 (33, 608 KB, caso a caso) → ~28 %.

**Decisiones.** D-PK-0 = A · D-PK-1 = C · D-PK-2 = C · D-PK-3 = B (apertura). **D-PK-4 = B**
(núcleo .cs de 19 ficheros / 304 KB; GigManager, CompositionSession, MidiMusicManager y
SongCompositionUI salen del núcleo por churn en todos los lotes de código desde julio y se piden
por lote). **D-PK-5:** quedan 6 docs MGP de frontera (130 KB); **D-PK-5b** (ssot_manifest.yaml
del companion, 170 KB) en Fase 3 con recomendación FUERA.

**Hallazgos.** (1) F-R5c-2 confirmada por firma: `LoopScoreCalculator.cs` del PK es anterior a
M4.6F-2 — refresco obligatorio. (2) F-R5a-1 en su faceta de PK: el refresco de
`CharacterStatusId.cs` registrado en R5-a NO está en el PK (faltan 404/504/993). (3)
`SongCompositionUI.cs` sin marca HUD-COMP-1 — probablemente desfasada. (4) Firma D-DOC-5:
`CTX-2a_Doc_Diffs_2026-08-03.md` figura pendiente en CURRENT_STATE §5 pero inv 13 ya existe;
`CONT-B_Returns_MidiGenPlay_2026-07-31.md` y `DEMO-FIXES-A_Doc_Diffs_2026-07-15.md`
referenciados y ausentes — disposición debida. (5) `Design_Composition_Variations_v0_1.md` en PK
sin fila de índice → MANIFEST-2. (6) 61 ficheros del PK con U+FFFD → insumo de HELD-2 (caveat
de copia). (7) Ninguna copia .cs del PK tiene fecha de copia registrada; mtimes en epoch.

**Docs editados:** este fichero · `CURRENT_STATE.md` §2 (nota). Nuevos, no gobernados (Capa 2):
`PK_Manifest.md`, `MGP_Boundary_Index.md`. Informe: `PK_Audit_Report_2026-08-26.md`.
```

### 8.2 `CURRENT_STATE.md` §2 — nueva viñeta (arriba)

```
- **PK-AUDIT-1 — transición del Project Knowledge al modelo de tres capas (2026-08-26).**
  Sesión solo documental. El PK pasa a operarse como núcleo permanente (gobernanza + SSoTs +
  roadmaps activos + 19 seams `.cs`) · índices (`PK_Manifest.md`, `MGP_Boundary_Index.md`) ·
  resto bajo petición por lote (File Request Protocol). Clasificación completa y plan de corte
  en `PK_Audit_Report_2026-08-26.md`. **D-PK-4 = B** (los cuatro seams grandes se piden por
  lote) · **D-PK-5** resuelta (6 docs MGP de frontera quedan) · **D-PK-5b** abierta (manifest
  del companion, Fase 3). **Refresco obligatorio antes de uso como verdad de código:**
  `LoopScoreCalculator.cs` (F-R5c-2 confirmada por firma) · `CharacterStatusId.cs` (el refresco
  de F-R5a-1 no aterrizó en el PK) · `SongCompositionUI.cs` (sin marca HUD-COMP-1). Fase 1 del
  corte (23 ficheros, 540 KB) ejecutable sin más decisiones; Fases 2–3 tras subir los índices.
  Regla nueva del manifiesto §C: un lote que cambia código no cierra hasta actualizar las filas
  de `PK_Manifest.md` de sus ficheros de núcleo (o diferirlo explícitamente).
```

### 8.3 Sin cambios en `SSoT_INDEX.md`, `ssot_manifest.yaml`, `coverage-matrix.md`

Los dos índices de Capa 2 **no son documentos gobernados** (operativa del PK); no se registran en el índice de autoridad. Si en el futuro se decide gobernarlos, es una decisión de MANIFEST-2, no de esta sesión.

---

## Apéndice A — clasificación fichero a fichero (359)

Columna **Fase**: 1 / 2 / 3 según §5; "—" = PERMANENTE (queda).

### PERMANENTE — 54 ficheros · 1782 KB

| Fichero | KB | Fase | Motivo |
|---|---:|:-:|---|
| `AudienceCharacterBase.cs` | 31 | — | Audience governs; D-R5-2 targeting; S7 state machine / S8 boss subclass |
| `AudienceCharacterData.cs` | 8 | — | Audience governs; lote D-R4-1 (S2 tonalidad autorada vs sonante) |
| `CSV_Composition_Validation_Sub_Roadmap.md` | 45 | — | sub-roadmap activo (CSV-4b/5/6) |
| `CURRENT_STATE.md` | 170 | — | baseline operativo |
| `CardBase.cs` | 37 | — | Card_System governs; ExecuteEffects + sensory publish (JUICE-PW) + overlay (CARD-UX-1) |
| `CardDefinition.cs` | 8 | — | Card_System + Authoring_Contracts governs; R5-d resourceCost pair |
| `CardEffectSpec.cs` | 0.4 | — | base de todos los *Spec (0.4 KB); Authoring_Contracts |
| `CharacterStatusId.cs` | 2 | — | contrato de serialización de ids; R5 — COPIA DESFASADA (F-R5a-1 no aterrizó en el PK) |
| `DeckManager.cs` | 35 | — | Runtime_Flow governs; deck/hand pipeline + BuildAndGetCard spawn hook |
| `Design_Project_Directives_v0_1.md` | 12 | — | directivas standing D1–D3, aplican a toda sesión |
| `Documentation_Update_Loop_Local_Addendum_v0_4.md` | 8 | — | gobernanza operativa (addendum local) |
| `GigFlowSettingsSO.cs` | 9 | — | Gig_Combat_Core §12; R5-d MaxBonusLoopsPerPart |
| `GigRunContext.cs` | 6 | — | Gig_Combat_Core governs; run scope |
| `HandController.cs` | 52 | — | Runtime_Flow governs; play path hand→play; CARD-UX-1 playability polling |
| `ICompositionContext.cs` | 2 | — | seam de contexto de composición (1 KB) |
| `LoopFeedbackContext.cs` | 5 | — | Integration governs (Music/Context Data); lote D-R4-1 (S2) |
| `LoopScoreCalculator.cs` | 2 | — | Scoring + Integration governs; lote D-R4-1 (S2) — COPIA DESFASADA (F-R5c-2) |
| `MGP-20260810_CURRENT_STATE.md` | 101 | — | estado operativo del companion; fuente para preguntas de frontera |
| `MGP-20260810_MANIFEST.md` | 10 | — | procedencia del espejo (rutas, last-write, SHA) — imprescindible para pedir por nombre |
| `MGP-20260810_SSoT_CONTRACTS.md` | 8 | — | contratos del companion (8 KB) |
| `MGP-20260810_SSoT_INDEX.md` | 4 | — | autoridad del companion (3 KB) |
| `MGP-20260810_SSoT_Runtime_Song_Model_and_Config.md` | 6 | — | modelo SongConfig = objeto de handoff; citado por Integration SSoT (6 KB) |
| `MGP-20260810_package.json` | 0.4 | — | pin de versión del paquete (0.4 KB) |
| `MeterTuningSO.cs` | 4 | — | Scoring + Gig_Combat_Core governs; R1 captivated tuning; D-R4-1/S5i tuning |
| `MultiProject_Documentation_Governance_System_v0_4.md` | 32 | — | gobernanza normativa |
| `PersistentGameplayData.cs` | 50 | — | reward pool / run state; R8 rewards; S6 run flow |
| `Rehydration_Prompt_Guide.md` | 11 | — | operativa de handoff entre chats |
| `Roadmap_ALWTTT.md` | 97 | — | roadmap activo |
| `Roadmap_Audio.md` | 16 | — | roadmap activo |
| `RosterExpansion_Sub_Roadmap.md` | 48 | — | sub-roadmap activo (R5-d..R8) |
| `S5_DemoCutClose_Sub_Roadmap.md` | 36 | — | sub-roadmap activo (S5i/S5j = frente vivo) |
| `SSoT_ALWTTT_MidiGenPlay_Boundary.md` | 50 | — | SSoT viva (frontera) |
| `SSoT_Audience_and_Reactions.md` | 15 | — | SSoT viva |
| `SSoT_Audio.md` | 46 | — | SSoT viva |
| `SSoT_CONTRACTS.md` | 5 | — | contracts |
| `SSoT_Card_Authoring_Contracts.md` | 40 | — | SSoT viva |
| `SSoT_Card_System.md` | 32 | — | SSoT viva |
| `SSoT_Dev_Mode.md` | 123 | — | SSoT viva |
| `SSoT_Editor_Authoring_Tools.md` | 86 | — | SSoT viva |
| `SSoT_Gig_Combat_Core.md` | 31 | — | SSoT viva |
| `SSoT_Gig_Encounter.md` | 15 | — | SSoT viva |
| `SSoT_INDEX.md` | 15 | — | índice de autoridad |
| `SSoT_Runtime_CompositionSession_Integration.md` | 50 | — | SSoT viva |
| `SSoT_Runtime_Flow.md` | 14 | — | SSoT viva |
| `SSoT_Scoring_and_Meters.md` | 14 | — | SSoT viva |
| `SSoT_Singer_Voice.md` | 11 | — | SSoT viva |
| `SSoT_Status_Effects.md` | 41 | — | SSoT viva |
| `SongConfigBuilder.cs` | 19 | — | Integration SSoT governs; frontera ALWTTT→SongConfig; R7 |
| `StatusEffectCatalogueSO.cs` | 12 | — | Status_Effects governs; catálogo SO = invariante del proyecto |
| `StatusEffectContainer.cs` | 9 | — | Status_Effects governs; SpendStacks (R5-a); invariante SO-container |
| `StatusEffectSO.cs` | 13 | — | Status_Effects governs; SuggestKey (R5-pre) |
| `changelog-ssot.md` | 170 | — | historia semántica |
| `coverage-matrix.md` | 22 | — | lookup de autoridad |
| `ssot_manifest.yaml` | 92 | — | manifiesto (governs:) |

### POR-LOTE — 231 ficheros · 2600 KB

| Fichero | KB | Fase | Motivo |
|---|---:|:-:|---|
| `ALWTTTProjectRegistriesSO.cs` | 6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ALWTTT_Uses_MidiGenPlay_Quick_Path.md` | 3 | 3 | reference frontera (3 KB) |
| `ActionCardPayload.cs` | 0.7 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ActionTargetType.cs` | 0.6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AddInspirationPerLoopSpec.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AddStressAction.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AddVibeAction.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AlwtttLogSetup.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ApplyStatusEffectAction.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ApplyStatusEffectSpec.cs` | 0.8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudienceBlockedEvent.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudienceCharacterCanvas.cs` | 14 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudienceCharacterSimple.cs` | 0.1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudienceCharacterStats.cs` | 13 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudienceIntentionData.cs` | 0.6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudienceMoveToFrontAction.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudiencePickerRow.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudienceReactionEvent.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudienceTurnStartedEvent.cs` | 0.8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudienceVibeImpactEvent.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudioActionType.cs` | 0.4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudioManager.cs` | 17 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `AudioMixSettingsSO.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `BackgroundContainer.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `BackgroundRoot.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `BandCharacterCanvas.cs` | 9 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `BandCharacterStats.cs` | 15 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `BandDeckData.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `BandDeckEntry.cs` | 0.9 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `BeatPulseIndicator.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `BlockStressAction.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `BlockVibeAction.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CONTRIBUTING.md` | 5 | 3 | excluded en manifiesto; convenciones de repo |
| `CSV-4b_Name_Lookup_Audit.md` | 8 | 3 | insumo de CSV-4b (lote vivo, queued) |
| `CardAcquisitionFlags.cs` | 0.3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardAssetFactory.cs` | 8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardAuthoringNav.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardDefinitionDescriptionExtensions.cs` | 6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardDetailViewController.cs` | 6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardEditorWindow.cs` | 115 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardEditorWindow_JsonImport.cs` | 63 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardEditorWindow_LLM.cs` | 19 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardEffectDescriptionBuilder.cs` | 6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardImportDtoParser.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardImportDtos.cs` | 10 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardInventoryWindow.cs` | 31 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardLLMFieldPlan.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardLLMGenerator.cs` | 6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardLLMPromptBuilder.cs` | 13 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardLLMResponseHandler.cs` | 26 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardLLMResponseHandlerTests.cs` | 16 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardLLMVocabulary.cs` | 5 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardLLMVocabularyBuilder.cs` | 6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardPaletteDescriptorScanner.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardPaletteIntentResolver.cs` | 10 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardPerformerRule.cs` | 0.2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardPlayedEvent.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardType.cs` | 0.2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CardUI.cs` | 0.1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CharacterActionData.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CharacterActionParameters.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CharacterActionProcessor.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CharacterActionType.cs` | 0.9 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CharacterAnimator.cs` | 16 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CharacterBase.cs` | 7 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CharacterCanvas.cs` | 11 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CharacterSfxProfileSO.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CharacterStats.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CharacterStatusPrimitiveDatabaseSO.cs` | 18 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ChoiceCard.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ChordProgressionCatalogueWizard.cs` | 31 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CompositionCardClassifier.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CompositionCardPayload.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CompositionContextRowUI.cs` | 10 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CompositionFxConfigSO.cs` | 5 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CompositionInventoryWindow.cs` | 68 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CompositionInventoryWindow_Cards.cs` | 39 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CompositionSession.cs` | 100 | 2 | ídem; R5-d, R6, R7 lo tocan → siempre pedido |
| `CompositionStripDriver.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `CompositionStripThemeSO.cs` | 9 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DeckAssetSaveService.cs` | 12 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DeckCardCreationService.cs` | 24 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DeckEditorDtos.cs` | 8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DeckEditorWindow.cs` | 51 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DeckJsonImportService.cs` | 11 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DeckValidationService.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DemoLaunchConfigSO.cs` | 8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `Design_Action_Economy_v1.md` | 4 | 3 | D-ECON-GENERIC / S5i |
| `Design_Asset_Naming_v0_1.md` | 7 | 3 | CSV-4b (aplica la convención) |
| `Design_Audience_Status_v1.md` | 18 | 3 | parcial (§4); S7 archetypes |
| `Design_Composition_Debug_Tab_v0_1.md` | 16 | 3 | filas 10–13 MGP-dependientes; CSV-5 |
| `Design_Composition_Variations_v0_1.md` | 15 | 3 | DRAFT no gobernado, sin fila en INDEX (huérfano PK); insumo R6/R8 — registro es hallazgo para MANIFEST-2 |
| `Design_Demo_Cut_v1.md` | 19 | 3 | S5i/S5j (frente vivo) |
| `Design_Fill_Window_v0_1.md` | 4 | 3 | registrada, no programada |
| `Design_Game_And_Card_Maxims_v0_1.md` | 17 | 3 | S5i lentes de observación |
| `Design_Pending_Effects_v1.md` | 13 | 3 | long-term post-MVP |
| `Design_Sensory_Contract_v0_1.md` | 34 | 3 | D-SENSORY-HOME (MANIFEST-2); R7 floater exception |
| `Design_Singer_Expression_Input_v0_1.md` | 3 | 3 | registrada, no programada (R6 rider) |
| `Design_Song_Parts_Library_v0_1.md` | 9 | 3 | long-term post-MVP |
| `Design_Starter_Deck_v1.md` | 49 | 3 | retained rationale (D10=B); CSV-6 cita §4; Sibi bassline flag |
| `Design_Starter_Deck_v2_DRAFT.md` | 19 | 3 | R7/R8 |
| `Design_Tempo_Identity_v1.md` | 7 | 3 | long-term post-MVP |
| `Design_Track_Card_Levels_v0_1.md` | 10 | 3 | R7 (spec) |
| `Design_Tutorial_System_v0_2.md` | 43 | 3 | TUT-REFRESH |
| `Design_Vertical_Slice_v0_1.md` | 19 | 3 | S6–S8 (queued) |
| `Design_Vibe_Telegraph_v0_1.md` | 8 | 3 | shipped S5a; pedir para PRES-* |
| `DevAudioMixTab.cs` | 9 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DevCardCatalogueTab.cs` | 10 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DevCompositionDebugTab.cs` | 68 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DevGigOutcomeTracker.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DevModeController.cs` | 15 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DevRunTelemetryLogger.cs` | 12 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DevStatsTab.cs` | 21 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `DrawCardsSpec.cs` | 0.4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `EconPipTooltipTarget.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `FloatingText.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `FloatingTextMidiListener.cs` | 12 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ForegroundAnimator.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `FxManager.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GameplayData.cs` | 7 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GenerationDebugFormatter.cs` | 7 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GenericCardCatalogSO.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GigCanvas.cs` | 13 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GigDevSettingsSO.cs` | 6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GigEncounter.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GigEncounterSO.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GigLauncher.cs` | 7 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GigManager.cs` | 171 | 2 | tocado por R3, R5-a/b/c, R5-d, HUD-COMP-1, BAL-1, DBG-C1/C2… → siempre pedido por lote (Opción B) |
| `GigOutcomeEvent.cs` | 0.9 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GigPresentationSO.cs` | 7 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GigSetupController.cs` | 28 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GigSetupRosterSO.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GigStartedEvent.cs` | 0.8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `GrantBonusLoopSpec.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `HealStressAction.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `I2DTooltipTarget.cs` | 0.2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `IAudienceStats.cs` | 0.8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ISensoryEvent.cs` | 0.9 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ITooltipTargetBase.cs` | 0.4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `InstrumentEffect.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `InstrumentRules.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `InventoryCanvas.cs` | 5 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `LoopResolvedEvent.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MGP-20260810_Especificacion_Contenido_FaseA.md` | 16 | 3 | planning de contenido; CSV-6 / R8 |
| `MGP-20260810_coverage-matrix.md` | 32 | 3 | navegación del companion; el índice de frontera la sustituye |
| `MGP-20260810_ssot_manifest.yaml` | 170 | 3 | D-PK-5b: 170 KB de invariantes cacheados de internals; su función de navegación la asume MGP_Boundary_Index.md |
| `MainMenuController.cs` | 6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` | 27 | 3 | referencia de integración; R6/R8 contenido de cartas |
| `MidiMusicManager.cs` | 144 | 2 | ídem; R5-d duck plane, R6, toda pregunta de frontera → siempre pedido |
| `MinicardTooltipController.cs` | 8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MixGainProfileSO.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ModifyStressSpec.cs` | 0.7 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ModifyVibeSpec.cs` | 0.6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `ModulationEffect.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MoodTag.cs` | 0.8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MusicDirector.cs` | 12 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MusicianBase.cs` | 9 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MusicianCardCatalogData.cs` | 0.6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MusicianCardEntry.cs` | 0.8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MusicianCatalogService.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MusicianCharacterData.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MusicianCharacterSimple.cs` | 1.0 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MusicianCharacterType.cs` | 0.2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MusicianPickerRow.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MusicianProfileData.cs` | 5 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `MusicianStressHitEvent.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `OstCatalogSO.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `OstTrackId.cs` | 0.8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `PartActionDescriptor.cs` | 0.6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `PartEffectEditorWindow.cs` | 29 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `PinkTrombone_Voice_Levers.md` | 9 | 3 | schema VoiceProfileSO; R6 (SingerVoiceDirector) |
| `PsychicWaveInvert.shader` | 8 | 3 | presentación (superficie S1 cerrada); PRES-* |
| `PsychicWaveOverlayController.cs` | 18 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `RarityType.cs` | 0.1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `RemoveVibeAction.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `RevealPreferencesSpec.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `RewardCanvas.cs` | 9 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `RewardChoiceOpenedEvent.cs` | 0.5 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `RewardDatabase.cs` | 0.4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `RewardType.cs` | 0.2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SensoryAudioAdapter.cs` | 8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SensoryEventBus.cs` | 7 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SensoryFtPresentation.cs` | 8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SensoryFxAdapter.cs` | 13 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SensorySfxPresentation.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SensorySfxType.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SfxStageCrossedEvent.cs` | 0.9 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SongCompositionUI.cs` | 61 | 2 | HUD-COMP-1 + R7 (TryAddOrReplaceTrackOnPart) → siempre pedido; procedencia dudosa |
| `SongEndVibeEvent.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SongPartElementUI.cs` | 15 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SongTrackElementUI.cs` | 18 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SoundBankSO.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SpecialKeywordData.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SpecialKeywords.cs` | 0.3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SpotlightRedirectEvent.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SpriteOutlineController.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `SpriteOutlineURP.shader` | 5 | 3 | presentación; PRES-* |
| `StageLightAnimator.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `StatusAppliedEvent.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `StatusEffectActionData.cs` | 0.6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `StatusEffectInstance.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `StatusEffectWizardWindow.cs` | 26 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `StatusIconBase.cs` | 8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `StatusStats.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `StatusType.cs` | 0.5 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TooltipController.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TooltipManager.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TooltipText.cs` | 2 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TrackActionDescriptor.cs` | 0.5 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TrackHoverPanel.cs` | 8 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialController.cs` | 29 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialDialogCatalogSO.cs` | 23 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialDialogSO.cs` | 6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialGuidedDriver.cs` | 20 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialHighlightSpawnHook.cs` | 7 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialHighlightTarget.cs` | 7 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialInputGate.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialLoopHoldGate.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialModalGate.cs` | 1 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialOptInPrompt.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialOverlayView.cs` | 19 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialRevisitPanel.cs` | 4 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialScriptedDrawQueue.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `TutorialSpotlight.shader` | 6 | 3 | presentación / tutorial; TUT-REFRESH |
| `TutorialTokenResolver.cs` | 3 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `UIManager.cs` | 6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `UIPulseAnimator.cs` | 5 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `VibeEffectiveness.cs` | 0.6 | 2 | fuera del núcleo (D-PK-1=C); se adjunta al lote que lo toque |
| `integrations_midigenplay_README.md` | 2 | 3 | README de carpeta (excluded) |
| `planning_README.md` | 2 | 3 | README de carpeta (excluded) |
| `planning_active_README.md` | 1 | 3 | README de carpeta (excluded) |

### HISTÓRICO — 15 ficheros · 432 KB

| Fichero | KB | Fase | Motivo |
|---|---:|:-:|---|
| `ALWTTT_Combat_MVP_Audit_Final.md` | 36 | 1 | archive (manifiesto); MVP cerrado |
| `ALWTTT_DeckEditorWindow_Roadmap_Proposal.md` | 26 | 1 | archive; fases 0–6 completas |
| `ALWTTT_MidiGenPlay_Soundfont_Emulation_Report_2026-03-24.md` | 17 | 1 | archive; carpeta planning/music no existe (F16) |
| `DOC-APPLY-2_Application_Report_2026-08-08.md` | 27 | 1 | informe de aplicación; absorbido por changelog 2026-08-08 (MANIFEST-1) |
| `Design_Tutorial_System_v0_1.md` | 29 | 1 | archivado 2026-08-08 (D11=A); absorbido por v0_2 |
| `How_Successful_Roguelike_Deckbuilders_Are_Designed_and_Balanced.md` | 26 | 1 | research absorbido en Design_Game_And_Card_Maxims (BALANCE-XREF) |
| `M1_5_Dev_Mode_Sub_Roadmap.md` | 11 | 1 | archive; Fases 1–3 cerradas, 4–5 dropped; autoridad en SSoT_Dev_Mode |
| `MGP-20260810_ALWTTT_Melody_Authoring_Pipeline.md` | 8 | 1 | pipeline 2026-03-18; superado por SSoT_Card_Authoring_Contracts + Integration |
| `MGP-20260810_Handoff_MGP_BAGGAGE_1.md` | 15 | 1 | ask entregado y adoptado (CSV-4, 2026-07-20) |
| `MGP-20260810_Handoff_MGP_MIX_1.md` | 6 | 1 | ask entregado y adoptado (BAL-1, 2026-07-22) |
| `MGP-20260810_Handoff_MGP_POCKET.md` | 6 | 1 | ask entregado y adoptado (R2d SelfPocket, 2026-07-31) |
| `MGP-20260810_SSoT_CompositionSystem_INDEX.md` | 2 | 1 | índice cross-project 2026-04-14, previo a la gobernanza actual |
| `MGP-20260810_changelog-ssot.md` | 207 | 1 | historia del companion (206 KB) |
| `Report_CardLLM_Pipeline.md` | 10 | 1 | archive; el fichero ya no existe en el repo (lápida F16) |
| `TUT-REBUILD_Sub_Roadmap.md` | 5 | 1 | arco cerrado 2026-07-10; TUT-REFRESH será lote nuevo |

### FUERA-ALCANCE — 51 ficheros · 744 KB

| Fichero | KB | Fase | Motivo |
|---|---:|:-:|---|
| `MGP-20260810_BackingCardConfigSO.cs` | 10 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_BassTrackComposer.cs` | 102 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_BasslineCardConfigSO.cs` | 26 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_ChordExpressionType.cs` | 10 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_ChordProgressionData.cs` | 15 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_ChordProgressionLibrarySO.cs` | 2 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_ChordProgressionPaletteSO.cs` | 3 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_ChordProgressionRequality.cs` | 23 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_ChordProgressionRuntimeImporter.cs` | 39 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_ChordQualityResolver.cs` | 8 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_CompositionReadback.cs` | 8 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_DrumPatternData.cs` | 9 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_DrumPatternPaletteSO.cs` | 4 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_DrumPatternTextParser.cs` | 15 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_HarmonyCardConfigSO.cs` | 0.3 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_IPatternRepository.cs` | 0.8 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_ITrackComposer.cs` | 0.6 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_ITrackPatternConfigStore.cs` | 2 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_InstrumentRepositoryResources.cs` | 2 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_MIDIInstrumentSO.cs` | 1 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_MIDIPercussionInstrumentSO.cs` | 3 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_MelodicLeadingConfig.cs` | 2 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_MelodicStyleSO.cs` | 7 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_MelodyCardConfigSO.cs` | 1 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_MelodyPatternData.cs` | 6 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_MidiGenPlayConfig.cs` | 6 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_ModulationOctaveHint.cs` | 0.7 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_PENDING_DOC_DIFFS.md` | 11 | 2 | pendientes documentales del companion |
| `MGP-20260810_PatternDataSO.cs` | 0.3 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_PatternRepositoryResources.cs` | 4 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_PhraseArchetypeSO.cs` | 0.6 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_PhrasePaletteSO.cs` | 1 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_PitchBendWriter.cs` | 12 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_RhythmCardConfigSO.cs` | 6 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_RomanProgressionParser.cs` | 20 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_SSoT_Authoring_Chord_Progressions.md` | 34 | 2 | authoring package-side |
| `MGP-20260810_SSoT_Authoring_Melody_Composition.md` | 28 | 2 | authoring package-side |
| `MGP-20260810_SSoT_Authoring_Rhythm_Patterns.md` | 21 | 2 | authoring package-side |
| `MGP-20260810_SSoT_Authoring_Tools.md` | 22 | 2 | authoring package-side (tooling) |
| `MGP-20260810_SSoT_Composer_Backing_Track.md` | 58 | 2 | internals de composer |
| `MGP-20260810_SSoT_Composer_Bass_Track.md` | 52 | 2 | internals de composer |
| `MGP-20260810_SSoT_Composer_Melody_Track.md` | 23 | 2 | internals de composer |
| `MGP-20260810_SSoT_Composer_Rhythm_Track.md` | 16 | 2 | internals de composer |
| `MGP-20260810_SSoT_Runtime_Generation_Orchestration.md` | 26 | 2 | orquestación interna |
| `MGP-20260810_SongConfig.cs` | 6 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_SongOrchestrator.cs` | 77 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_TonalityProfileSO.cs` | 6 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_TrackPatternConfigStoreResources.cs` | 7 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_TrackStyleBundleSO.cs` | 0.5 | 2 | código interno del paquete (D-PK-2=C) |
| `MGP-20260810_VoiceLeadingConfig.cs` | 4 | 2 | código interno del paquete (D-PK-2=C) |
| `MIDITrack.cs` | 1 | 2 | namespace MidiGenPlay; tipo del paquete sin prefijo MGP- |

### REDUNDANTE — 8 ficheros · 108 KB

| Fichero | KB | Fase | Motivo |
|---|---:|:-:|---|
| `CSV-4c_Doc_Diffs.md` | 32 | 1 | paquete consumido en DOC-APPLY-3 (retirada inmediata, decisión de apertura) |
| `MGP-20260810_SSoT_CompositionCards_TrackStyleBundles.md` | 19 | 1 | fuente mixta repartida entre Card_System / Integration / Boundary (INDEX tabla transicional); compite en retrieval |
| `MGP-20260810_SSoT_Runtime_CompositionSession_Bridge.md` | 11 | 1 | fuente promovida en Batch 03 → SSoT_Runtime_CompositionSession_Integration (INDEX tabla transicional); compite en retrieval |
| `MelodyCardConfigSO.cs` | 1 | 1 | duplicado byte-idéntico de MGP-20260810_MelodyCardConfigSO.cs |
| `MelodyPatternData.cs` | 6 | 1 | duplicado byte-idéntico de MGP-20260810_MelodyPatternData.cs |
| `PENDING_DOC_DIFFS_HUD-COMP-1.md` | 11 | 1 | paquete consumido en DOC-APPLY-3 (retirada inmediata, decisión de apertura) |
| `PENDING_DOC_DIFFS_R5d.md` | 22 | 1 | paquete consumido en DOC-APPLY-3 (retirada inmediata, decisión de apertura) |
| `ssot-drift-auditor_SKILL.md` | 5 | 1 | la skill ya está instalada en /mnt/skills/user/; copia en PK compite en retrieval |

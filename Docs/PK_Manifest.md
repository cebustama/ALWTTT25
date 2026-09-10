# PK_Manifest — ALWTTT Project Knowledge (Capa 2 · índice de contenido del PK)

**Versión 9 — 2026-09-10 (TXT-2-DOC).** Cambios en v9: **alta de `SSoT_Game_Text.md`** en §A.2
(autoridad nueva, D-TAG-9=A) ⇒ **83 ficheros**; bloque **§A.1 → «Refrescos y altas de TXT-2»** con
las tres copias que el lote deja rancias (`TutorialOverlayView.cs` no está en el PK y no entra;
`TutorialController.cs` **sigue rancia y ahora con un segundo lote encima**; `SSoT_Card_System.md`,
`SSoT_Status_Effects.md`, `Design_Tutorial_System_v0_3.md`, `SSoT_Editor_Authoring_Tools.md`,
`CURRENT_STATE.md`, `changelog-ssot.md`, `coverage-matrix.md`, `SSoT_INDEX.md`, `ssot_manifest.yaml`
y `S5_DemoCutClose_Sub_Roadmap.md` **sustituidos por reemplazo completo**); censo en §B.2 de los
**cuatro `.cs` nuevos** del lote y de los **tres assets/CSV** que `Repo_Tree_Index.md` no puede
indexar; **F-TT-5 sigue abierta** (cuatro retiradas registradas como ejecutadas el 2026-09-05 que no
se ejecutaron) y ahora arrastra también la fila de `TutorialController.cs`. `Repo_Tree_Index.md`
**NO regenerado** — deuda de cinco lotes.

**Versión 8 — 2026-09-10 (TUT-TXT-1-DOC).** Cambios en v8: bloque **§A.1 → «Refrescos, salidas y
altas de TUT-TXT-1 / TUT-TXT-1c»** (dos copias marcadas rancias: `TutorialDialogSO.cs` y
`TutorialController.cs`; `TutorialDialogCatalogSO.cs` **sustituida por reemplazo completo**);
**corrección de la fila de TXT-1** que daba por vencida la copia del catálogo (no lo estaba: era la
fila la que estaba rancia); **hallazgo F-TT-5** — cuatro retiradas registradas como ejecutadas el
2026-09-05 que **no se ejecutaron**; alta del **CSV de recuperación** del tutorial (D-TT-7=A), que
`Repo_Tree_Index.md` no puede indexar por ser `.csv`; `Repo_Tree_Index.md` regenerado
(snapshot 2026-09-10 11:30).

**Versión 7 — 2026-09-09 (BIGNUM-1-DOC).** Cambios en v7: bloque de refrescos de **BIGNUM-1** en §A.1 (tres copias marcadas rancias: `AudienceCharacterStats.cs`, `AudienceCharacterCanvas.cs`, `CharacterCanvas.cs`) · **dos altas** en §A.2 (`VibeProjection.cs` y `Design_Vibe_Telegraph_v0_1.md`, que reingresa desde §B.5) ⇒ **82 ficheros** · censo de los cuatro ficheros nuevos del lote en §B.2 · **F-BN-0 cerrada** con una nota que fija que los encabezados de §B.2 son etiquetas de área, no rutas · deuda del índice de rutas actualizada a cuatro lotes.

**Versión 6 — 2026-08-27.** Refleja el PK **realmente observado**: **80 ficheros** (Fase 3-A ejecutada 2026-08-27; el plan de corte de PK-CUT-1 queda **completo**). Cambios en v6: **corregida §D.4** — los tres supuestos «fantasmas» eran renombrados punto→guion bajo, no ausencias; regla nueva sobre registrar ambos nombres. Cambios en v5: alta de `Repo_Tree_Index.md` (Capa 2, 445 rutas) · nueva regla §C.5 de regeneración · corregidas las rutas de los tres `README.md` renombrados para el PK. Cambios en v4: los 16 de Fase 3-A pasan de §A a **§B.5** · `Design_Composition_Variations_v0_1.md` y `MGP_Boundary_Index.md` comprometidos al repo (§D.1 actualizada). Cambios en v3: **todas las rutas de repo verificadas contra el árbol real** (`tree.txt`, 2026-08-27) — desaparece el marcador † de ruta inferida · marcados los ficheros que **solo existen en el PK** y no en el repo · nueva §D con el resultado del cruce. Cambios de v2 respecto de v1: filas de `LoopScoreCalculator.cs` y `CharacterStatusId.cs` corregidas tras el refresco del Paso 1 (§A.1 queda vacía) · alta de los 7 ficheros del lote **RFX-1** · alta de los dos índices de Capa 2 · los 3 ficheros escapados de las Fases 1–2 pasan a §B.4 · columna **Estado** en §A para no volver a confundir «está en el PK» con «debe seguir en el PK».

**Propósito.** Este fichero dice **qué hay en el PK, con qué procedencia, y qué se retiró y dónde vive ahora**, para que un chat pueda pedir por nombre lo que no está adjunto (File Request Protocol). **No es un documento gobernado por SSoT** — es operativa del PK. No define autoridad: la autoridad vive en `SSoT_INDEX.md` y `ssot_manifest.yaml`, que describen el **repo** (D-PK-0 = A: la pertenencia al PK es presupuesto de contexto, no gobernanza).



**Cómo leer la columna "Fecha de copia".** Ningún fichero ALWTTT del PK tiene fecha de copia registrada (los mtimes del PK están a epoch). La columna "Verificado-contra" da, en su lugar, el **último marcador de lote visible en el contenido** de la copia. Regla (instrucciones v2, puerta de frescura): si un lote cerrado tocó el fichero después de ese marcador, o no hay marcador, **refresco antes de usar como verdad de código**; si se procede sin refresco, toda conclusión se etiqueta "inferida de copia posiblemente desfasada".

**Convenciones.** Rutas `.cs` sin marca proceden de `ssot_manifest.yaml` `governs:`; con `†` son deducción del `namespace`, no verificación. Rutas MGP proceden de `MGP-20260810_MANIFEST.md`.

---

## §A — En el PK

### A.1 Refresco pendiente

#### Refrescos y altas de TXT-2 (2026-09-10)

| Fichero | Clase | Acción | Motivo |
|---|---|---|---|
| `SSoT_Game_Text.md` | PERMANENTE | **ALTA** en el PK (Capa 1) | autoridad nueva creada por el lote (D-TAG-9=A). Sin ella, cualquier pregunta futura sobre orden de resolución de conceptos se responde por inferencia |
| `SSoT_INDEX.md` · `ssot_manifest.yaml` · `coverage-matrix.md` · `CURRENT_STATE.md` · `changelog-ssot.md` | PERMANENTE | **reemplazo completo** | alta de la autoridad nueva + cierre del lote |
| `SSoT_Editor_Authoring_Tools.md` · `Design_Tutorial_System_v0_3.md` · `SSoT_Status_Effects.md` · `SSoT_Card_System.md` · `S5_DemoCutClose_Sub_Roadmap.md` | PERMANENTE | **reemplazo completo** | §20.11 / §5A.6 nuevas; notas de consumidor; TXT-2 cerrada y TXT-3 dada de alta |
| `TutorialController.cs` | PERMANENTE | **sigue RANCIA — segundo lote encima** | ya marcada rancia en v8 (copia 2026-09-03, editada por TUT-TXT-1 el 2026-09-09); TXT-2 le añade dos campos serializados y una línea en `OnEnable`. La copia del PK **contenía** los edits de TUT-TXT-1 pese a la marca, lo que hizo utilizable el Step 6 con verificación previa de anclas — pero eso fue suerte, no proceso. **Refrescar o ejecutar su retirada (F-TT-5) antes del próximo lote que la toque.** |
| `TutorialOverlayView.cs` | Capa 3 | **no entra** | editada por el lote; se pide por File Request Protocol cuando haga falta. Registrarla en el PK sería crear una copia que envejece |

> **Por qué la fila de `TutorialController.cs` importa más que las demás.** Es la segunda vez
> consecutiva que un lote la edita estando marcada rancia. En TXT-2 se resolvió imponiendo un
> `grep -c` de cada ancla contra el fichero del repo antes de aplicar el paso — es decir, tratando la
> copia del PK como *sospechosa* y no como verdad de código. Ese es el comportamiento correcto y
> debería seguir aplicándose mientras la fila no se cierre; lo que no debe repetirse es llegar a un
> tercer lote con la misma fila abierta.

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
| `StatusEffectSO.cs` | **Refrescar.** Campo `applySfx` + accessor `ApplySfx`. | **cerrado 2026-09-05 (TXT-1)** — copia fresca adjuntada al chat de TXT-1 y verificada por contenido (`applySfx` + `ApplySfx` + `TryScheduleAutoRename` presentes); sustituye a la copia del PK. Fila de §A.2 actualizada. |
| `HandController.cs` | **Refrescar.** `using ALWTTT.Sensory` + publish de `CardPerformedEvent` tras `PlayCardOneShotAnimation`. | **cerrado 2026-09-03** (refrescada en TUT-REDESIGN-B; verificado por contenido en DOC-TUTR-B: `CardPerformedEvent` + `ReportPlayDenied` presentes) |
| `MusicianBase.cs` | **Refrescar + corregir clasificación.** `PlayCardAnimationRoutine` resuelve vía `MusicianCharacterData.ResolveCardAnimation`. Además figura en §B.2 como retirado a Capa 3 pero **está presente en el PK**: corregir la contradicción al refrescar. | **cerrado 2026-09-04 (DOC-TUTR-B, D1=A)** — copia del PK verificada post-lote (contiene `GigLossCause`); fila de alta en §A.2; retirada de §B.2 marcada como **nunca ejecutada** |
| `CharacterCanvas.cs` | **Solo corrección de clasificación.** No lo tocó WINK-1; misma contradicción retirado-pero-presente que `MusicianBase.cs`, pendiente desde antes del lote. | **cerrado 2026-09-04 (DOC-TUTR-B, D1=A)** — fila de alta en §A.2; retirada de §B.2 marcada como nunca ejecutada |
| `GigManager.cs` | **No está en el PK** (Capa 3, §B.2 — 171 KB). Modificado por WINK-1 (publish de composición): se anota aquí para que el próximo lote que lo pida sepa que su copia de repo debe ser posterior al 2026-08-31. | sin acción de PK |
| `SensoryFxAdapter.cs` · `SensoryAudioAdapter.cs` · `SensoryFtPresentation.cs` · `CharacterSfxProfileSO.cs` · `MusicianCharacterData.cs` | **No entran al PK.** Modificados por WINK-1; se piden por lote (Capa 3). | sin acción |
| `CardPerformedEvent.cs` · `StatusVisualDriver.cs` | **Ficheros nuevos, no entran al PK.** Costuras pequeñas y estables; se piden por lote si un lote futuro las edita. Rutas: `Assets/Scripts/Sensory/` y `Assets/Scripts/Characters/`. | sin acción |

#### Refrescos y altas abiertos por BIGNUM-1 (2026-09-09)

Lote de código cerrado el 2026-09-09; documentación aplicada en BIGNUM-1-DOC el mismo día.
**Un refresco no está hecho hasta que su fila lo dice** (§C.1); las filas de §A.2 quedan marcadas
y estas son las acciones.

| Fichero | Acción | Estado |
|---|---|---|
| `GigManager.cs` | **No está en el PK** (Capa 3, §B.2 — 171 KB). **EDITADO 2026-09-09:** `ComputeLPart`, `BuildVibeProjection`, `RefreshVibeProjection`, `AverageUnconvincedMaxVibe`, hook de `StatusAppliedEvent`, `SetVibeReadout(int,int,float,float,float)`, `SetReadoutTempo`, `CurrentBpm`. **Toda copia anterior al 2026-09-09 es rancia.** | sin acción de PK · fila de §B.2 actualizada |
| `AudienceCharacterStats.cs` | **Refrescar.** EDITADO 2026-09-09 — `PreviewIncomingVibe` + `IncomingVibePreview`. Ruta real confirmada: `Assets/Scripts/Characters/AudienceCharacterStats.cs` (**F-BN-0 cerrada**, nota en §B.2). | **abierto** |
| `AudienceCharacterCanvas.cs` | **Refrescar.** EDITADO 2026-09-09 — sobrecarga con `VibeProjection`, tooltip de barra, tinte de KO, `SetPredictionVisible`. | **abierto** |
| `CharacterCanvas.cs` | **Refrescar.** EDITADO 2026-09-09 — override de predicción en la política de visibilidad (S5e-ext enmendada por D-BN-17=A). La copia de §A.2 es del alta del 2026-09-04. | **abierto** |
| `VibeEffectiveness.cs` | **Sin cambios**, confirmado con el fichero delante durante el lote. | cerrado |
| `VibeProjection.cs` | **ALTA en el PK** (Capa 1, seam file). Contrato de presentación del Vibe por miembro; pequeño y estable, lo leerá cualquier lote de UI de gig. Ruta: `Assets/Scripts/Characters/VibeProjection.cs`. | **pendiente de adjuntar** |
| `Design_Vibe_Telegraph_v0_1.md` | **ALTA en el PK** (Capa 1). Hogar de presentación de C1/C2/C3, ahora con §10; se consultará en GIG23-A y en TIP-1. Ruta: `Docs/planning/Design_Vibe_Telegraph_v0_1.md`. Reingresa desde §B.5. | **pendiente de adjuntar** (copia posterior a BIGNUM-1-DOC) |
| `VibeReadoutBeatPulse.cs` · `VibeReadoutTooltipTarget.cs` · `VibeBarTooltipTarget.cs` | **No entran al PK.** Ficheros nuevos, costuras pequeñas; se piden por lote (Capa 3). Rutas: `Assets/Scripts/UI/` y `Assets/Scripts/UI/Tooltips/`. | sin acción |
| `GigCanvas.cs` · `GigPresentationSO.cs` · `HealthBarController.cs` | **No están en el PK.** Editados por BIGNUM-1; censados en §B.2. | sin acción de PK |
| `MidiMusicManager.cs` | Editado **sólo si se aplicó la sonda `[BeatProbe]` del rider r3**. La sonda es **temporal** y la retira BEAT-1; no censar como cambio permanente. | condicional |

**Refrescos ajenos que este lote NO cierra:** `StatusEffectContainer.cs` (WINK-1 — la copia del PK
publica `StatusAppliedEvent` con 3 argumentos, el código real pasa 4) y la D5 de AMW-1
(`characterId` duplicado) → **AMW-2**, con el resultado negativo de F-GEW-4.

**Ficheros a descartar del chat al cerrar BIGNUM-1-DOC:** `GigManager.cs` · `GigCanvas.cs` ·
`AudienceCharacterCanvas.cs` · `AudienceCharacterStats.cs` · `GigPresentationSO.cs` ·
`VibeEffectiveness.cs` · `BeatPulseIndicator.cs` · `MidiListenerCanvasBase.cs` ·
`HealthBarController.cs` · `EconPipTooltipTarget.cs` · `TooltipManager.cs` ·
`TooltipController.cs` · `MidiMusicManager.cs` · `CoreLoader.cs` · `BIGNUM-1_Doc_Package.md`
(se retira al aplicarse, convención de paquetes) · todas las capturas.

#### Refrescos, ediciones y altas de TUT-REDESIGN-B / DOC-TUTR-B (2026-09-03 / 2026-09-04)

**Refrescos ejecutados en la sesión TUT-REDESIGN-B** (verificados por contenido en DOC-TUTR-B, no por afirmación):

| Fichero | Fecha de copia nueva | Verificado-contra |
|---|---|---|
| `PersistentGameplayData.cs` | 2026-09-03 (refrescada) | TUT-REDESIGN-B — `BuildRewardCardPool` (IsReward ∧ U, sin duplicados, excluye mazo) y `SetBandDeckFromMusicians` (IsStarter, no exige U) leídos como verdad de código |
| `HandController.cs` | 2026-09-03 (refrescada) | TUT-REDESIGN-B — cierra el "Refrescar" abierto de WINK-1; contiene `ReportPlayDenied` (5 sitios) |
| `TutorialGuidedDriver.cs` | 2026-09-03 (refrescada) | TUT-REDESIGN-B — mano forzada de 4, `ApplySuppressionForArcState` |
| `TutorialController.cs` | 2026-09-03 (refrescada) | TUT-REDESIGN-B — cuatro suscripciones nuevas, enrutado por `StatusKey`, `voltageOverloadThreshold` (**el paquete la daba por refrescar; la copia del PK ya lo estaba**) |
| `TutorialDialogSO.cs` | 2026-09-03 (refrescada) | TUT-REDESIGN-B — 13 constantes nuevas, `TracksByMusician`, reservados eliminados (ídem) |
| `TutorialDialogCatalogSO.cs` | 2026-09-03 (refrescada) | TUT-REDESIGN-B — dos seeders nuevos, copy retocado, exención vacía; el literal `tut_tracks_three` es deliberado (ídem) |
| `MusicianBase.cs` | 2026-09-03 (refrescada) | TUT-REDESIGN-B — `LoseGig(GigLossCause.CohesionCollapse)` (ídem) |

**Ficheros editados por el lote cuyas copias previas quedan vencidas** (Capa 3, no en el PK salvo indicación):

| Fichero | Dónde está | Anotación |
|---|---|---|
| `GigManager.cs` | Capa 3 | **EDITADO 2026-09-03:** `ReportPlayDenied`, `LoseGig(GigLossCause)`, publish de `EarwormTickEvent`, victoria anticipada + `AllAudienceConvinced`. Cualquier copia anterior al 2026-09-03 es inválida. |
| `CompositionSession.cs` | Capa 3 | **EDITADO 2026-09-03:** `Fail(msg, reason)` enruta al embudo; publish de `BonusLoopStartedEvent`. |
| `SongCompositionUI.cs` | **presente en el PK, RANCIA** (sin `TrackReplacedEvent`; figura como retirada en §B.2) | **EDITADO 2026-09-03:** publish de `TrackReplacedEvent`. **Retirar del PK** (D1=A, acción física pendiente): una copia rancia que retrieval recupera como autoritativa es la familia de fallo de §A.1. |
| `CardInventoryWindow.cs` | Capa 3 | **EDITADO 2026-09-03:** export de catálogos completo con flags. |
| `DevRunTelemetryLogger.cs` | Capa 3 | **EDITADO 2026-09-03:** `lossCause` por causa. El `<summary>` sigue afirmando causa única (deuda de comentario, `SSoT_Dev_Mode` §17.3). |
| **Ficheros nuevos** (Capa 3, no entran al PK) | — | `PlayDeniedEvent.cs`, `EarwormTickEvent.cs`, `BonusLoopStartedEvent.cs`, `TrackReplacedEvent.cs` en `Assets/Scripts/Sensory/`. |

**Hallazgo F-DOCTUTR-1 — deriva del inventario (2026-09-04).** Cruce del PK real contra §A.2:
**12 ficheros presentes sin fila**: `TutorialController.cs`, `TutorialDialogSO.cs`,
`TutorialDialogCatalogSO.cs`, `TutorialGuidedDriver.cs`, `MusicianBase.cs`, `CharacterCanvas.cs`
(los seis dados de alta hoy, D1=A), `SongCompositionUI.cs` (rancia, retirar),
`FloatingTextMidiListener.cs` (adjunto de RFX-1 con refresco abierto, ver tabla RFX), `AudienceMoveToFrontAction.cs`
(sin procedencia), `PENDING_DOC_DIFFS_TUT-REDESIGN-B.md` (consumido hoy → §B.1),
`mapa_elastic_a_plantilla.md` y `muestra_ch_elastic.json` (**ajenos al proyecto**; retirar). Y una fila
sin fichero: `GigMessageUI.cs` está censada como "no entra al PK", correcto. La retirada física es acción
del usuario; las filas ya describen la realidad.

#### Salidas, refrescos y altas de TXT-1 (2026-09-05)

**Salidas POR-LOTE ejecutadas al cerrar TXT-1** (§A.2 marcaba a los cinco `Fase 3-B — sale al cerrar
TXT-1`). Todas viven en el repo; ninguna es de las de §D.1:

| Fichero | Acción | Estado |
|---|---|---|
| `Design_Tutorial_System_v0_3.md` | **Sale del PK.** Consumido como inventario de textos por TXT-1. Vive en `Docs/planning/active/`. | ejecutada 2026-09-05 |
| `TutorialController.cs` · `TutorialGuidedDriver.cs` · `TutorialDialogSO.cs` | **Salen del PK** (Capa 3). Sin cambios en TXT-1: las copias de 2026-09-03 siguen siendo verdad de código. | ejecutadas 2026-09-05 |
| `TutorialDialogCatalogSO.cs` | **Sale del PK y su copia queda VENCIDA.** TXT-1 la editó: `languageCode` + `CanonicalTriggerIds()` / `ParityReport` / `ComputeParity()` + `ReservedUnauthored` promovido a campo estático + el `[MenuItem]` convertido en envoltorio. **Toda copia anterior al 2026-09-05 es inválida**; si vuelve al PK algún día, entra con copia post-TXT-1. | ejecutada 2026-09-05 |

> **CORRECCIÓN 2026-09-10 (TUT-TXT-1-DOC).** Las dos filas anteriores describen **intención, no
> estado**. Verificado por presencia directa al abrir TUT-TXT-1 (2026-09-09): `TutorialController.cs`,
> `TutorialGuidedDriver.cs`, `TutorialDialogSO.cs` y `TutorialDialogCatalogSO.cs` **seguían en el
> PK**; ninguna de las cuatro salidas «ejecutadas 2026-09-05» se ejecutó (la fila de
> `TutorialGuidedDriver` en §B.2 ya lo anotaba para sí misma; aplica a las cuatro). Registrado como
> **F-TT-5** en `CURRENT_STATE.md` §4. Además, la fila de `TutorialDialogCatalogSO.cs` afirmaba que
> su copia quedaba **VENCIDA** por TXT-1: **no era cierto**. La copia del PK ya era post-TXT-1
> (`languageCode`, `ComputeParity`) y resultó **byte a byte idéntica** a la copia fresca del repo
> pedida el 2026-09-09. Estaba rancia la fila, no la copia. *(Esa copia sí queda vencida ahora, por
> TUT-TXT-1c.)*

**Refresco cerrado:** `StatusEffectSO.cs` (ver la tabla de WINK-1 arriba y su fila en §A.2). Queda
**abierto** el otro refresco de WINK-1: `StatusEffectContainer.cs` (la copia del PK publica
`StatusAppliedEvent` con 3 argumentos; el código real pasa 4).

**Ficheros nuevos del lote — Capa 3, NO entran al PK.** Costuras editor-only, estables, sin
consumidores de runtime; se piden por lote si un lote futuro las edita:

| Fichero | Ruta en repo |
|---|---|
| `GameTextWindow.cs` | `Assets/Scripts/Editor/Text/GameTextWindow.cs` |
| `TutorialTextTable.cs` | `Assets/Scripts/Editor/Text/TutorialTextTable.cs` |
| `GameTextCsv.cs` | `Assets/Scripts/Editor/Text/GameTextCsv.cs` |

**Adjuntos de chat descartados al cerrar:** `CardInventoryWindow.cs`, `PartEffectEditorWindow.cs`,
`MainMenuController.cs`, `tree.txt`, `Repo_Tree_Index.md` (copia subida). De `MainMenuController.cs`
queda constancia documental de su hallazgo — no contiene strings de jugador — en `CURRENT_STATE` §4
(P-TXT-1), de modo que retirarlo no pierde la conclusión.

#### Refrescos, salidas y altas de TUT-TXT-1 / TUT-TXT-1c (2026-09-09, aplicado 2026-09-10)

**Copias del PK que este lote deja RANCIAS.** Los tres ficheros figuran como retirados el
2026-09-05 y **seguían presentes** (F-TT-5). Acción: **sustituir por la copia post-lote** si se
quieren mantener, o **retirar de verdad** — son Capa 3 por política (D-PK-1=C), así que retirar es lo
coherente; lo que **no** es admisible es dejarlos presentes y desfasados.

| Fichero | Estaba | Está | Acción |
|---|---|---|---|
| `TutorialDialogSO.cs` | 2026-09-03 (TUT-REDESIGN-B) | **EDITADO 2026-09-09:** `mechanicText` + `MechanicText` / `HasMechanicText` + `EditorSeedStructure`. `EditorSeed` sobrevive y su único uso legítimo pasa a ser crear un asset desde cero. | sustituir o retirar |
| `TutorialController.cs` | 2026-09-03 (TUT-REDESIGN-B) | **EDITADO 2026-09-09:** `showMechanicText`, `ResolveBodyPages`, `pagesOverride` en los dos call-sites de `overlay.Show` y `body=` en el log de `SHOW`. | sustituir o retirar |
| `TutorialDialogCatalogSO.cs` | 2026-09-03 = repo (verificado idéntica) | **REEMPLAZO COMPLETO 2026-09-09:** 470 → 299 líneas. 6 seeders con copy → 2 de estructura; `Add()` sin texto; `BeginSeed`/`dialogs.Clear()` retirados; paridad intacta. | sustituir o retirar |
| `TutorialGuidedDriver.cs` | 2026-09-03 | **sin cambios** en estos lotes | retirar (salida pendiente desde TXT-1) |

**Ficheros del lote que NO entran al PK** (Capa 3; `Editor/Text` ya censado arriba por TXT-1):
`TutorialOverlayView.cs` (`Assets/Scripts/Tutorial/TutorialOverlayView.cs`, editado: parámetro
`pagesOverride`) y los tres de `Assets/Scripts/Editor/Text/` (`GameTextWindow.cs`,
`TutorialTextTable.cs`, `GameTextCsv.cs`, los tres editados).

**Alta de artefacto — el CSV de recuperación del tutorial (D-TT-7=A).**
Ruta: `Assets/Resources/Data/Tutorial/` (export de `GameTextWindow`, **nombre fijo sin timestamp**).
No es un fichero del PK: es un artefacto del **repo** y la mitad «texto» de la ruta de recuperación
del tutorial (`SSoT_Editor_Authoring_Tools.md` §20.10). Se censa aquí porque
**`Repo_Tree_Index.md` no lo indexa**: el índice excluye `.csv`, así que este fichero es invisible al
índice de rutas y ningún chat lo encontrará por ahí. Regenerar el índice no lo hará aparecer.

**Adjuntos de chat descartados al cerrar:** `GameTextWindow.cs`, `GameTextCsv.cs`,
`TutorialTextTable.cs`, `TutorialOverlayView.cs`, `TutorialController.cs`,
`TutorialDialogCatalogSO.cs`, `GameText_Tutorial_20260909_2109.csv`, `tree.txt`,
`Repo_Tree_Index.md` (copia subida — su contenido se adopta como fila nueva, ver §C.1 regla 5).

**Paquete consumido:** `TUT-TXT-1_DocPackage.md` se retira del PK al aplicarse (convención de
paquetes, D-DOC-5).

#### Altas, salidas y descartes de AMW-1 / AMW-1b + DOC-APPLY-4 (2026-09-05)

**Fichero nuevo del lote AMW-1 — Capa 3, NUNCA entra al PK.**

| Fichero | Ruta en repo | Por qué no entra |
|---|---|---|
| `AudienceMemberWizard.cs` | `Assets/Scripts/Characters/Editor/AudienceMemberWizard.cs` | Herramienta de editor sin papel de seam: nada de runtime la lee y ningún lote la necesita como contexto. Su verdad documental vive en `SSoT_Editor_Authoring_Tools.md` §21, que sí está en el PK |

**Adjuntos de chat descartados al cerrar AMW-1 (2026-09-05):** `PartEffectEditorWindow.cs`,
`CardInventoryWindow.cs`, `StatusEffectWizardWindow.cs`, `CharacterActionData.cs`,
`AudienceIntentionData.cs`, `AudienceCharacterData.cs` (copia fresca; la del PK queda y es idéntica).

**Retiradas de paquetes ejecutadas en DOC-APPLY-4 (2026-09-05).** Los tres paquetes de diffs salen
del PK; sus filas de disposición están en §B.1.

| Fichero | Acción | Estado |
|---|---|---|
| `PENDING_DOC_DIFFS_TUT-REDESIGN-B.md` | **Retirar.** Aplicado el 2026-09-04; verificado por muestreo en DOC-APPLY-4. | ejecutada 2026-09-05 |
| `PENDING_DOC_DIFFS_RFX-1.md` | **Retirar.** Consumido (D-DA4-D2=A); §5 resuelto sin cambio. Existía solo en el PK, pero su contenido está íntegro en los destinos ⇒ no es pérdida. | ejecutada 2026-09-05 |
| `DocApply_AMW-1.md` | **Retirar sólo tras cerrar su 8ª edición** (banner en `Design_AudienceMemberWizard_Requirements_v0_1.md`, destino no disponible en sesión). Si se retira antes, la deuda se declara aquí con lote dueño nombrado. | **CERRADA 2026-09-06 (GEW-1, D-GEW-9)** — el banner se aplicó en GEW-1 con el fichero delante; el paquete puede retirarse sin declarar pérdida |

**Copias del PK reemplazadas por este lote** (editadas en DOC-APPLY-4; toda copia anterior al
2026-09-05 es inválida): `SSoT_Editor_Authoring_Tools.md` (§21 nueva, filas de §3 y §13) ·
`coverage-matrix.md` (fila de herramientas de editor) · `CURRENT_STATE.md` (fila de cierre AMW-1 +
actualización del riesgo D-DOC-5) · `changelog-ssot.md` (entradas AMW-1 y DOC-APPLY-4) ·
`PK_Manifest.md` (este fichero) · `ssot_manifest.yaml` (tres ediciones, D-DA4-D3=A).

**Refresco ajeno que sigue ABIERTO y que este lote no cierra:** `StatusEffectContainer.cs` (WINK-1
— la copia del PK publica `StatusAppliedEvent` con 3 argumentos; el código real pasa 4).

#### Altas, salidas y descartes de GEW-1 (2026-09-06)

**Fichero nuevo del lote — Capa 3, NUNCA entra al PK.**

| Fichero | Ruta en repo | Por qué no entra |
|---|---|---|
| `GigEncounterWizard.cs` | `Assets/Scripts/Encounters/Editor/GigEncounterWizard.cs` | Misma razón que `AudienceMemberWizard.cs`: herramienta de editor sin papel de seam, ningún consumidor de runtime, ningún lote la necesita como contexto. Su verdad documental es `SSoT_Editor_Authoring_Tools.md` §22, que sí está en el PK |

**Adjuntos de chat descartados al cerrar (2026-09-06):** `GigEncounterSO.cs`, `EncounterData.cs`,
`GigSetupRosterSO.cs`, `DemoLaunchConfigSO.cs`, `GigEncounter.cs`, `EncounterBase.cs`,
`AudienceMembers.json` (export de AMW-1), `Design_GigEncounterWizard_Requirements_v0_1.md`,
`Design_AudienceMemberWizard_Requirements_v0_1.md`, y las dos capturas de pantalla.
De los ficheros de código queda constancia documental de sus hallazgos —F-GEW-1..5— en
`SSoT_Gig_Encounter.md` §4/§7.5, en §22.11 y en el changelog, de modo que retirarlos no pierde
ninguna conclusión. **`EncounterData.cs` merece nota aparte:** su hallazgo (F-GEW-3, dos
representaciones de encuentro) es el único que abre una pregunta de autoridad; si el lote de
mapa/ladder la retoma, el fichero se vuelve a pedir por FRP.

**Democión de documentos de requisitos.**

| Fichero | Acción | Estado |
|---|---|---|
| `Design_AudienceMemberWizard_Requirements_v0_1.md` | Banner de supersesión aplicado (autoridad → §21) y movido a `Docs/planning/archive/`. Cierra la 8ª edición de `DocApply_AMW-1.md`. | ejecutada 2026-09-06 |
| `Design_GigEncounterWizard_Requirements_v0_1.md` | Banner de supersesión aplicado (autoridad → §22) y movido a `Docs/planning/archive/`. Consumido por GEW-1. | ejecutada 2026-09-06 |

**Copias del PK reemplazadas por este lote** (toda copia anterior al 2026-09-06 es inválida):
`SSoT_Editor_Authoring_Tools.md` (§22 nueva, filas de §3 y §13, nota de namespace) ·
`SSoT_Gig_Encounter.md` (§4 forma implementada, §7.5 aviso F-GEW-2) · `coverage-matrix.md` (fila de
herramientas de editor + corrección F-DA4-1 en la fila del tutorial) · `CURRENT_STATE.md` (fila de
cierre GEW-1 + nota de legibilidad del Vibe) · `changelog-ssot.md` (entrada GEW-1) ·
`ssot_manifest.yaml` (una edición: `Assets/Scripts/Encounters/Editor` en `governs:`) ·
`PK_Manifest.md` (este fichero).

**`Repo_Tree_Index.md` desactualizado — regenerar (regla §C.1 punto 5).** El snapshot vigente es del
2026-08-27 y ya no lista tres carpetas creadas después: `Assets/Scripts/Editor/Text/` (TXT-1),
`Assets/Scripts/Characters/Editor/` (AMW-1) y `Assets/Scripts/Encounters/Editor/` (GEW-1), ni los
cuatro ficheros nuevos de BIGNUM-1 (`VibeProjection.cs`, `VibeReadoutBeatPulse.cs`,
`VibeReadoutTooltipTarget.cs`, `VibeBarTooltipTarget.cs`). **Cuatro lotes** de deuda acumulada en el
mismo índice que sirve para pedir ficheros por ruta. Nota de BIGNUM-1-DOC: el snapshot del
2026-08-27, pese a la deuda, **cerró F-BN-0** — para ficheros anteriores a esa fecha sigue siendo
evidencia válida, y mejor que cualquier encabezado de §B.2 (ver la nota allí). La regeneración se
debe por los ficheros **nuevos**, no por los viejos.

**Refresco ajeno que sigue ABIERTO y que este lote no cierra:** `StatusEffectContainer.cs` (WINK-1
— la copia del PK publica `StatusAppliedEvent` con 3 argumentos; el código real pasa 4).

**Paquete retenido creado por este lote:** ninguno. GEW-1 aplicó su propio paquete de diffs en la
misma sesión (precedente AMW-1), así que no hay `PENDING_DOC_DIFFS_GEW-1` que pueda quedar huérfano
bajo D-DOC-5.

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
| `SSoT_Game_Text.md` | PERMANENTE | queda | TXT-2 (2026-09-10) | Docs/systems/SSoT_Game_Text.md | TXT-2 (2026-09-10) — creado y aplicado en la misma sesión | autoridad nueva: fontanería del texto de jugador (poblaciones, tags, orden de resolución, hover, paridad, CSV). NO es autoridad sobre el significado de ningún concepto |
| `AudienceCharacterBase.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Characters/AudienceCharacterBase.cs | PRES-1 / D-R5-2 (2026-08-11): SelectDefaultMusicianTarget + SpotlightRedirectEvent presentes | ok · sin lote posterior conocido |
| `AudienceCharacterData.cs` | PERMANENTE | queda | AMW-1 (2026-09-05) | Assets/Scripts/Data/Characters/Audience/AudienceCharacterData.cs | GEW-1 (2026-09-06): leída para derivar las columnas de la fila de público (`maxVibe`, `abilityList`, `followAbilityPattern`, `taste`); no editada. Antes: AMW-1 (2026-09-05), copia fresca comparada con la del PK — **idénticas** salvo BOM/EOL | ok · ni AMW-1 ni GEW-1 editaron el fichero (editor-only) |
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
| `Design_Tutorial_System_v0_3.md` | POR-LOTE | **RETIRADA 2026-09-05 al cerrar TXT-1** (ver §A.1, bloque TXT-1) | 2026-09-04 (renombrada v0_2 → v0_3 en DOC-TUTR-B) | Docs/planning/active/Design_Tutorial_System_v0_3.md | DOC-TUTR-B / 2026-09-04 — §6C arco de la banda de 4 | **sustituye la copia `v0_2`**: no deben coexistir (§C.3) |
| `Design_Vertical_Slice_v0_1.md` | POR-LOTE | Fase 3-B — sale al cerrar S6–S8 | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | S6–S8 (queued) |
| `Design_Vibe_Telegraph_v0_1.md` | PERMANENTE | queda (alta BIGNUM-1-DOC 2026-09-09) | 2026-09-09 | Docs/planning/Design_Vibe_Telegraph_v0_1.md | BIGNUM-1-DOC (2026-09-09): contiene §10 (§10.1–§10.7) | hogar de presentación C1/C2/C3; insumo de GIG23-A y TIP-1. Sigue clasificado `planning` en `SSoT_INDEX` — la matemática vive en `SSoT_Scoring_and_Meters` §6. Reingresa desde §B.5 |
| `VibeProjection.cs` | PERMANENTE | queda (alta BIGNUM-1-DOC 2026-09-09) | 2026-09-09 | Assets/Scripts/Characters/VibeProjection.cs | BIGNUM-1 (2026-09-09): fichero nuevo del lote | contrato de presentación del Vibe por miembro; seam pequeño y estable para lotes de UI de gig |
| `Documentation_Update_Loop_Local_Addendum_v0_4.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | gobernanza operativa (addendum local) |
| `GigFlowSettingsSO.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Data/Gig/GigFlowSettingsSO.cs | R5-d (2026-08-26): MaxBonusLoopsPerPart presente | ok · sin lote posterior conocido |
| `GigRunContext.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Managers/GigRunContext.cs | sin marcas de lote reconocibles | sin lote posterior conocido |
| `HandController.cs` | PERMANENTE | queda | **2026-09-03 (refrescada, TUT-REDESIGN-B)** | Assets/Scripts/Controllers/HandController.cs | **TUT-REDESIGN-B / 2026-09-03** — cuatro puertas de acción por `ReportPlayDenied`; `CardPerformedEvent` (WINK-1); puerta 2b.5 (R5-g) | **Doble corrección de fila (R5-g).** La fila anterior decía `CARD-UX-1 (2026-07-13)` mientras la copia del PK ya contenía marcadores **R5-d**: el índice iba dos meses por detrás de su propio fichero, variante benigna del incidente de §A.1 y misma familia de fallo. Verificado además en R5-g que la copia del PK y la del repo eran **byte-idénticas salvo BOM** antes del lote (R5-f no tocó el fichero) |
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
| `PersistentGameplayData.cs` | PERMANENTE | queda | **2026-09-03 (refrescada, TUT-REDESIGN-B)** | Assets/Scripts/Data/Core/PersistentGameplayData.cs | **TUT-REDESIGN-B / 2026-09-03** — `BuildRewardCardPool` (IsReward ∧ U) y `SetBandDeckFromMusicians` (IsStarter) leídos como verdad de código | ok |
| `TutorialGuidedDriver.cs` | POR-LOTE | **RETIRADA 2026-09-05 al cerrar TXT-1** (alta 2026-09-04, D1=A; estaba presente sin fila) | 2026-09-03 (refrescada) | Assets/Scripts/Tutorial/TutorialGuidedDriver.cs | TUT-REDESIGN-B / 2026-09-03 — `ApplySuppressionForArcState`, mano forzada de 4 | figura en §B.2 como Capa 3: la retirada nunca se ejecutó |
| `TutorialController.cs` | POR-LOTE | **RETIRADA 2026-09-05 al cerrar TXT-1** (alta 2026-09-04) | 2026-09-03 (refrescada) | Assets/Scripts/Tutorial/TutorialController.cs | TUT-REDESIGN-B / 2026-09-03 — `voltageOverloadThreshold`, suscripción a `PlayDeniedEvent` | ídem |
| `TutorialDialogSO.cs` | POR-LOTE | **RETIRADA 2026-09-05 al cerrar TXT-1** (insumo de TXT-1; alta 2026-09-04) | 2026-09-03 (refrescada) | Assets/Scripts/Tutorial/TutorialDialogSO.cs | TUT-REDESIGN-B / 2026-09-03 — 34 constantes, `TracksByMusician`, sin reservados | ídem |
| `TutorialDialogCatalogSO.cs` | POR-LOTE | **RETIRADA 2026-09-05 al cerrar TXT-1** (insumo de TXT-1; alta 2026-09-04) | 2026-09-03 (refrescada) | Assets/Scripts/Tutorial/TutorialDialogCatalogSO.cs | TUT-REDESIGN-B / 2026-09-03 — seeders `Seed TUT-REDESIGN-B dialogs EN/ES (13, band of 4)` | ídem. **La copia retirada quedó VENCIDA el 2026-09-05: TXT-1 editó el fichero** (`languageCode`, `ComputeParity`). Cualquier reingreso futuro exige copia post-TXT-1 |
| `MusicianBase.cs` | POR-LOTE | Fase 3-B — sale al cerrar TXT-1 salvo decisión de núcleo (alta 2026-09-04, D1=A) | 2026-09-03 (refrescada) | Assets/Scripts/Characters/Band/MusicianBase.cs | TUT-REDESIGN-B / 2026-09-03 — `LoseGig(GigLossCause.CohesionCollapse)`; `ResolveCardAnimation` (WINK-1) | contradicción retirado-pero-presente **resuelta**: presente y censado |
| `CharacterCanvas.cs` | POR-LOTE | Fase 3-B — retirar en el próximo corte (alta 2026-09-04, D1=A) | no registrada | Assets/Scripts/Characters/CharacterCanvas.cs | **RANCIA desde BIGNUM-1 (2026-09-09)** — el lote añadió el override de predicción (`SetPredictionVisible`) a la política de visibilidad S5e-ext; la copia del PK es anterior | contradicción retirado-pero-presente resuelta 2026-09-04; **refresco abierto §A.1 (BIGNUM-1)** — no usar como verdad de código sin copia fresca |
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
| `StatusEffectSO.cs` | PERMANENTE | queda | **2026-09-05 (refrescada, TXT-1)** | Assets/Scripts/Status/StatusEffectSO.cs | **WINK-1 / verificada 2026-09-05** — `applySfx` + `ApplySfx` presentes; también `description` `[TextArea]` y el auto-rename `StatusEffect_{DisplayName}_{EffectId}` de `OnValidate` | refresco de WINK-1 **cerrado**. El auto-rename es la razón de que la pestaña de status de `GameTextWindow` sea de sólo lectura (P-TXT-2, `CURRENT_STATE` §4) |
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
| `PENDING_DOC_DIFFS_TUT-REDESIGN-B.md` | 20 | REDUNDANTE | paquete **consumido en DOC-TUTR-B (2026-09-04)**; §12 (prompt de TXT-1) emitido como cierre de lote antes de retirarlo. **Retirada física ejecutada en DOC-APPLY-4 (2026-09-05)** tras verificar por muestreo cinco de sus once diffs contra los destinos (§1.1 · §1.2 · §1.4 · §2.1 · §10 · §11.3, todos presentes). Llevaba 24 h compitiendo en retrieval con los documentos que ya lo habían absorbido |
| `PENDING_DOC_DIFFS_RFX-1.md` | 12 | REDUNDANTE | paquete **consumido**, declarado así en **DOC-APPLY-4 (2026-09-05)**, D-DA4-D2=A. §1/§2/§3/§6 ya estaban aplicados (absorbidos por la pasada doc de RFX-2 el 2026-08-26); §4 tenía veredicto «sin cambio» registrado en la fila del contrato sensorial de `coverage-matrix.md`; **§5 resuelto en DOC-APPLY-4, también sin cambio** — `SSoT_ALWTTT_MidiGenPlay_Boundary.md` no enumera consumidores de `MidiMusicManager`, así que no había fila que añadir. **Cero contenido perdido.** Misma disposición que CSV-4c / R5-d / HUD-COMP-1 (§D.2): la desaparición es correcta, no es pérdida |
| `DocApply_AMW-1.md` | 18 | REDUNDANTE | paquete **consumido en DOC-APPLY-4 (2026-09-05)**: 7 de sus 8 ediciones aplicadas. La 8ª (banner de supersesión en `Design_AudienceMemberWizard_Requirements_v0_1.md`) **queda pendiente** porque el destino no estaba disponible en sesión — ver §A.1, bloque DOC-APPLY-4. No retirar este paquete hasta aplicar esa edición, o retirarlo declarando la 8ª como deuda con dueño nombrado |
| `Design_Tutorial_System_v0_2.md` | 46 | SUPERADO | renombrado a `v0_3.md` en DOC-TUTR-B (2026-09-04, D-TUTR-6=B); la copia v0_2 no debe quedar en el PK (§C.3) |
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
| `GigManager.cs` | 171 | Assets/Scripts/Managers/GigManager.cs | R5-d-SMOKE, ~~R6~~, R7, cualquier lote de turno/meters. **EDITADO en R6 (2026-09-01, D-R6-7):** tres llamadas a `GigMessageUI.Show` en las puertas ECON-1 / final-loop lock / coste de recurso. Toda copia anterior a esa fecha es RANCIA **EDITADO en BIGNUM-1 (2026-09-09):** `ComputeLPart`, `BuildVibeProjection`, `RefreshVibeProjection`, `AverageUnconvincedMaxVibe`, hook de `StatusAppliedEvent`, `SetVibeReadout(int,int,float,float,float)`, `SetReadoutTempo`, `CurrentBpm`. Toda copia anterior al 2026-09-09 es RANCIA. |
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

**`Characters`** (8 ficheros · 56 KB) — *`CharacterCanvas.cs`: retirada nunca ejecutada; presente y censado en §A.2 desde 2026-09-04*: `AudienceCharacterCanvas.cs`, `CharacterAnimator.cs`, `CharacterBase.cs`, `CharacterCanvas.cs`, `CharacterStats.cs`, `SpriteOutlineController.cs`, `StatusStats.cs`, `VibeEffectiveness.cs`, `VibeProjection.cs` *(alta BIGNUM-1 2026-09-09; pasa a §A.2)*

**`Characters/Actions | Cards/CardActions`** (11 ficheros · 23 KB): `AddStressAction.cs`, `AddVibeAction.cs`, `ApplyStatusEffectAction.cs`, `AudienceMoveToFrontAction.cs`, `BlockStressAction.cs`, `BlockVibeAction.cs`, `CharacterActionData.cs`, `CharacterActionParameters.cs`, `CharacterActionProcessor.cs`, `HealStressAction.cs`, `RemoveVibeAction.cs`

**`Characters/Audience`** (2 ficheros · 13 KB): `AudienceCharacterSimple.cs`, `AudienceCharacterStats.cs`

> **F-BN-0 — cerrada 2026-09-09 (BIGNUM-1-DOC).** Los encabezados en negrita de esta sección son
> **etiquetas de agrupación por área, no rutas de repo** — como deja ver `Characters/Actions |
> Cards/CardActions`, que abarca dos carpetas. Verificado contra `Repo_Tree_Index.md` (export
> mecánico, snapshot 2026-08-27): `Assets/Scripts/Characters/Audience/` existe y contiene **sólo**
> `AudienceCharacterSimple.cs`; `AudienceCharacterStats.cs` vive en
> `Assets/Scripts/Characters/AudienceCharacterStats.cs`. Lo mismo aplica al encabezado
> `Characters/Band`: esos seis ficheros están repartidos entre `Characters/`,
> `Characters/Musicians/` y `Data/Characters/`. **Para pedir un fichero por ruta se consulta el
> índice, nunca estos encabezados.**

**`Characters/Band`** (6 ficheros · 43 KB) — *`MusicianBase.cs`: retirada nunca ejecutada; presente y censado en §A.2 desde 2026-09-04*: `BandCharacterCanvas.cs`, `BandCharacterStats.cs`, `MusicianBase.cs`, `MusicianCharacterData.cs`, `MusicianCharacterSimple.cs`, `MusicianProfileData.cs`

**`Data (F16)`** (1 ficheros · 2 KB): `AlwtttLogSetup.cs`

**`Data/*`** (14 ficheros · 60 KB): `AudienceIntentionData.cs`, `AudioMixSettingsSO.cs`, `CharacterSfxProfileSO.cs`, `CompositionFxConfigSO.cs`, `CompositionStripThemeSO.cs`, `DemoLaunchConfigSO.cs`, `GigDevSettingsSO.cs`, `GigPresentationSO.cs`, `GigSetupRosterSO.cs`, `MixGainProfileSO.cs`, `OstCatalogSO.cs`, `RewardDatabase.cs`, `SoundBankSO.cs`, `SpecialKeywordData.cs` — *`GigPresentationSO.cs` EDITADO en BIGNUM-1 (2026-09-09): `showVibeReadout` nuevo; `showVibeEffectivenessLabels` y `showVibeProjectedNumbers` documentados por primera vez (F-BN-2)*

**`DevMode`** (8 ficheros · 142 KB): `DevAudioMixTab.cs`, `DevCardCatalogueTab.cs`, `DevCompositionDebugTab.cs`, `DevGigOutcomeTracker.cs`, `DevModeController.cs`, `DevRunTelemetryLogger.cs`, `DevStatsTab.cs`, `GenerationDebugFormatter.cs`

**`DevMode/Editor`** (2 ficheros · 106 KB): `CompositionInventoryWindow.cs`, `CompositionInventoryWindow_Cards.cs`

**`Encounters`** (2 ficheros · 5 KB): `GigEncounter.cs`, `GigEncounterSO.cs`

**`Enums`** (12 ficheros · 7 KB): `ActionTargetType.cs`, `AudioActionType.cs`, `CardType.cs`, `CharacterActionType.cs`, `MoodTag.cs`, `MusicianCharacterType.cs`, `OstTrackId.cs`, `RarityType.cs`, `RewardType.cs`, `SensorySfxType.cs`, `SpecialKeywords.cs`, `StatusType.cs`

**`Interfaces`** (1 ficheros · 1 KB): `IAudienceStats.cs`

**`Managers`** (6 ficheros · 50 KB): `AudioManager.cs`, `FxManager.cs`, `GigLauncher.cs`, `MainMenuController.cs`, `MusicDirector.cs`, `UIManager.cs`

**`Sensory`** (21 ficheros · 81 KB): `AudienceBlockedEvent.cs`, `AudienceReactionEvent.cs`, `AudienceTurnStartedEvent.cs`, `AudienceVibeImpactEvent.cs`, `CardPlayedEvent.cs`, `GigOutcomeEvent.cs`, `GigStartedEvent.cs`, `ISensoryEvent.cs`, `LoopResolvedEvent.cs`, `MusicianStressHitEvent.cs`, `PsychicWaveOverlayController.cs`, `RewardChoiceOpenedEvent.cs`, `SensoryAudioAdapter.cs`, `SensoryEventBus.cs`, `SensoryFtPresentation.cs`, `SensoryFxAdapter.cs`, `SensorySfxPresentation.cs`, `SfxStageCrossedEvent.cs`, `SongEndVibeEvent.cs`, `SpotlightRedirectEvent.cs`, `StatusAppliedEvent.cs`

**`Status`** (3 ficheros · 20 KB): `CharacterStatusPrimitiveDatabaseSO.cs`, `StatusEffectActionData.cs`, `StatusEffectInstance.cs`

**`Status/Editor`** (1 ficheros · 26 KB): `StatusEffectWizardWindow.cs`

**`Editor/Text`** (3 ficheros · TXT-1, 2026-09-05 — nunca han estado en el PK): `GameTextWindow.cs`, `TutorialTextTable.cs`, `GameTextCsv.cs`

**`Tutorial`** (14 ficheros · 130 KB) — *los cuatro primeros estuvieron en el PK como POR-LOTE (TXT-1) entre 2026-09-04 y su retirada el 2026-09-05; `TutorialDialogCatalogSO.cs` fue además **editado por TXT-1**, así que su copia retirada está vencida*: `TutorialController.cs`, `TutorialDialogCatalogSO.cs`, `TutorialDialogSO.cs`, `TutorialGuidedDriver.cs`, `TutorialHighlightSpawnHook.cs`, `TutorialHighlightTarget.cs`, `TutorialInputGate.cs`, `TutorialLoopHoldGate.cs`, `TutorialModalGate.cs`, `TutorialOptInPrompt.cs`, `TutorialOverlayView.cs`, `TutorialRevisitPanel.cs`, `TutorialScriptedDrawQueue.cs`, `TutorialTokenResolver.cs`

**`UI`** (17 ficheros · 146 KB): `AudiencePickerRow.cs`, `BeatPulseIndicator.cs`, `CardDetailViewController.cs`, `CompositionContextRowUI.cs`, `CompositionStripDriver.cs`, `FloatingText.cs`, `GigCanvas.cs`, `GigSetupController.cs`, `InventoryCanvas.cs`, `MinicardTooltipController.cs`, `MusicianPickerRow.cs`, `RewardCanvas.cs`, `SongPartElementUI.cs`, `SongTrackElementUI.cs`, `StatusIconBase.cs`, `TrackHoverPanel.cs`, `UIPulseAnimator.cs`, `VibeReadoutBeatPulse.cs` *(nuevo, BIGNUM-1 2026-09-09)* — *editados por BIGNUM-1 sin entrar al PK: `GigCanvas.cs`, `HealthBarController.cs`*

**`UI/Tooltips`** (6 ficheros · 11 KB): `EconPipTooltipTarget.cs`, `I2DTooltipTarget.cs`, `ITooltipTargetBase.cs`, `TooltipController.cs`, `TooltipManager.cs`, `TooltipText.cs`, `VibeReadoutTooltipTarget.cs`, `VibeBarTooltipTarget.cs` *(nuevos, BIGNUM-1 2026-09-09)*

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
5. **Al crear, borrar o mover ficheros del repo:** regenerar `Repo_Tree_Index.md` (`make-tree-unity.bat`) y actualizar la fecha de su fila en §A.2. *(Regenerado 2026-09-10 11:30, TUT-TXT-1c. **Aviso permanente:** el índice no cubre `.csv` ni assets de Unity, así que artefactos como el CSV de recuperación del tutorial no aparecerán nunca ahí; se censan en §A.1.)* Un índice de rutas viejo hace pedir ficheros que ya no existen — la misma familia de fallo que una fila de copia rancia, un piso más abajo. Si el aviso de **nombres duplicados** crece, revisarlo antes de adjuntar cualquiera de esos ficheros: el PK es plano y no los distingue.
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
| `PENDING_DOC_DIFFS_RFX-1.md` | ~~§A, lote RFX-1~~ → **§B.1, consumido 2026-09-05** | ✅ **cerrado en DOC-APPLY-4 (D-DA4-D2=A)**: el paquete estaba íntegramente absorbido en sus destinos y su último condicional (§5) se resolvió sin cambio. No requiere commit: no hay contenido que el repo no tenga ya. Se entregó igualmente una copia con banner de cierre por si se quiere archivar en `Docs/archive/doc-packages/` |
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
| `Design_Vibe_Telegraph_v0_1.md` | `Docs/planning/Design_Vibe_Telegraph_v0_1.md` | **REINGRESA AL PK el 2026-09-09 (BIGNUM-1-DOC)** — fila viva en §A.2; esta fila queda como historia de la retirada del 2026-08-27 |
| `MGP-20260810_coverage-matrix.md` | `(companion MidiGenPlay)` | adjuntar al chat si un lote futuro lo retoma |
| `PsychicWaveInvert.shader` | `Assets/Shaders/PsychicWaveInvert.shader` | adjuntar al chat si un lote futuro lo retoma |
| `SpriteOutlineURP.shader` | `Assets/Shaders/SpriteOutlineURP.shader` | adjuntar al chat si un lote futuro lo retoma |
| `TutorialSpotlight.shader` | `Assets/Shaders/TutorialSpotlight.shader` | adjuntar al chat si un lote futuro lo retoma |
| `integrations_midigenplay_README.md` | `Docs/integrations/midigenplay/README.md` | **renombrado para el PK** (el repo lo llama `README.md`; 9 homónimos — ver `Repo_Tree_Index.md`). Pedir por ruta, nunca por nombre |
| `planning_README.md` | `Docs/planning/README.md` | **renombrado para el PK**; pedir por ruta |
| `planning_active_README.md` | `Docs/planning/active/README.md` | **renombrado para el PK**; pedir por ruta |

# PK_Manifest — ALWTTT Project Knowledge (Capa 2 · índice de contenido del PK)

**Versión 18.6 — 2026-09-27 (SONG-TAIL-1, apertura: tareas 0–1).** **Ninguna alta ni baja todavía.**
`CONV-TINT-1-DOC_Package.md` consumido (v18.5 recoge su contenido); **sale del PK en el cierre de
SONG-TAIL-1**. Sustituido además por D-REG: `S5_DemoCutClose_Sub_Roadmap.md`. `CURRENT_STATE.md`,
`changelog-ssot.md` y este manifiesto vuelven a sustituirse. Las filas de código de SONG-TAIL-1 se añaden
en su cierre (v18.7).

**Versión 18.5 — 2026-09-27 (CONV-TINT-1 + 1b, cierre).** **Ninguna alta ni baja.** Refrescados
(copias del repo post-1b): `AudienceCharacterBase.cs` (primera fecha registrada de su fila) y
`AudienceCharacterCanvas.cs`. `CharacterCanvas.cs` **no** se sustituye: la copia del repo de hoy coincide con
la del PK, así que su marca RANCIA de BIGNUM-1 era falsa y se retira (F-CT-9; también era falsa la de
`AudienceCharacterCanvas.cs`). Sustituidos por el paquete `CONV-TINT-1-DOC_Package.md` (retenido en el PK desde
2026-09-27, **CONSUMIDO en SONG-TAIL-1**): `SSoT_Audience_and_Reactions.md`, `SSoT_Status_Effects.md`,
`SSoT_Gig_Combat_Core.md`, `SSoT_Card_Authoring_Contracts.md`, `SSoT_Dev_Mode.md`, `ssot_manifest.yaml`,
`coverage-matrix.md`, `CURRENT_STATE.md`, `changelog-ssot.md` y este manifiesto.

**Versión 18.4 — 2026-09-27 (DOC-POOL-1, cierre).** **Bajas:** `PAUSE-1_DocDiffs_Pending.md` y
`BEAT-1_Doc_Package.md`, ambos **CONSUMIDOS** (§B.1). **Trece documentos sustituidos por reemplazo completo**
(D1=C): `SSoT_Audio.md`, `SSoT_Runtime_Flow.md`, `SSoT_Card_System.md`, `SSoT_ALWTTT_MidiGenPlay_Boundary.md`,
`SSoT_Dev_Mode.md`, `SSoT_Singer_Voice.md`, `SSoT_Runtime_CompositionSession_Integration.md`,
`Design_Vibe_Telegraph_v0_1.md`, `SSoT_Game_Text.md`, `CURRENT_STATE.md`, `changelog-ssot.md`,
`coverage-matrix.md`, `ssot_manifest.yaml`, más este manifiesto. **F-B1-15 resuelta:** la copia del PK de este
fichero era v18.2 y no llevaba las Partes A de PAUSE-1 (pasos 34–35) ni de BEAT-1 (A1–A5), aunque sus acciones
físicas sí estaban hechas en el PK; se aplicaron aquí, primero, y por eso v18.3 existe sólo como paso intermedio
de esta sesión. **Puerta de frescura (R4):** ningún documento tocado tenía doc-pass posterior al 2026-09-25 (la
entrada más reciente del changelog era CARD-FACE-3-DOC-b y ninguna copia contenía texto de PAUSE-1 ni BEAT-1),
así que las copias del PK **eran** los documentos gobernados. `Repo_Tree_Index.md`: la copia del PK es el
snapshot **2026-09-25 19:17** (488 ficheros), posterior a PAUSE-1, con las cinco rutas nuevas; la fila de §A.2
decía 13:04 y se corrige. Ningún `.cs` tocado.

**Versión 18.3 — 2026-09-27 (BEAT-1, cierre de código).** **Baja:** `MidiEventInterfaces.cs` sale del PK
(F-B1-1: su fila de §A.2 decía POR-LOTE desde RFX-1 y otra fila decía que «no entra al PK»; la copia del
repo coincide con la del PK salvo un carácter mal codificado en un comentario, así que no se pierde verdad
de código). **Alta retenida:** `BEAT-1_Doc_Package.md`, consumidor **DOC-POOL-1**, junto a
`PAUSE-1_DocDiffs_Pending.md`; si DOC-POOL-1 cierra sin consumirlo, se declara perdido en esa clausura
(D-DOC-5). Ningún `.cs` de núcleo tocado: todos los editados por BEAT-1 son Capa 3.

**Versión 18.2 — 2026-09-25 (CARD-FACE-3-DOC-b, cierre).** **F-CF3DOC-1 cerrada:** `GameSymbolsTableMenu.cs`
leído en el chat y documentado en `SSoT_Editor_Authoring_Tools` §20.14 (D3=A); es **Capa 3** y no entra
al PK. Sustituidos por reemplazo completo: `SSoT_Editor_Authoring_Tools.md`, `ssot_manifest.yaml`,
`coverage-matrix.md`, `CURRENT_STATE.md`, `changelog-ssot.md`, `S5_DemoCutClose_Sub_Roadmap.md` (D1=A) y
este manifiesto. Puerta de frescura de S5: copia del 2026-09-21 (CARD-FACE-1/2-DOC), ningún doc-pass
posterior. **Adjunto a descartar:** `GameSymbolsTableMenu.cs`.

**Versión 18.1 — 2026-09-25 (CARD-FACE-3-DOC-b).** Salda las tres deudas diferidas por v18, las tres
**verificadas por contenido**, no por fecha: `CardBase.cs` y `GameSymbols.cs` refrescadas (su diff
contra la copia anterior es exactamente lo que CARD-FACE-3 declaró, y nada más) y `Repo_Tree_Index.md`
regenerado (snapshot 2026-09-25 13:04). **Hallazgo F-CF3DOC-1 — fichero no declarado.** El índice
regenerado trae **dos** `.cs` nuevos en `Assets/Scripts/Editor/`, no uno: `GameSymbolsSyncMenu.cs`
(declarado) y **`GameSymbolsTableMenu.cs`**, que no nombra ningún paquete, runbook ni documento del PK.
Como `ssot_manifest.yaml` gobierna la raíz de `Assets/Scripts/Editor/` **por fichero**, el fichero hoy
**no tiene autoridad**: el auditor de deriva lo vería como huérfano. Su contenido no se ha leído, así
que **no se le infiere propósito** por el nombre. Queda abierto hasta leerlo (FRP, sólo chat).
Nota menor: `Docs/planning/active` crece 1 KB entre snapshots sin que ningún doc-pass lo haya
declarado; no se investiga aquí.

**Versión 18 — 2026-09-25 (CARD-FACE-3-DOC).** Cambios en v18: **ningún alta y ninguna baja** en el
PK. **Ocho documentos sustituidos por reemplazo completo** (`SSoT_Card_System.md`, `SSoT_Game_Text.md`,
`SSoT_Tooltip_Pipeline.md`, `SSoT_Editor_Authoring_Tools.md`, `CURRENT_STATE.md`, `changelog-ssot.md`,
`coverage-matrix.md`, `ssot_manifest.yaml`) más este manifiesto. **Dos copias de núcleo quedan RANCIAS**
(`CardBase.cs`, `GameSymbols.cs`): CARD-FACE-3 las editó el 2026-09-22 y el PK conserva las copias del
2026-09-21 — comprobado por contenido en esta sesión (`OnPointerUp` sin guarda; cabecera «not wired by
CARD-FACE-1»). **No** se dan por refrescadas: el paquete proponía anotarlas como 2026-09-22 y eso habría
sido afirmar una copia que el PK no tiene. **`Repo_Tree_Index.md` con regeneración pendiente** (falta
`GameSymbolsSyncMenu.cs`). Ambas deudas se difieren con dueño: el usuario sube las dos copias del repo y
el índice regenerado, y la sesión siguiente que los reciba voltea las filas.

**R4 en este lote: sin excepción.** Los documentos se editaron sobre sus copias del PK (2026-09-21,
CARD-FACE-1/2-DOC), que ningún doc-pass ha tocado desde entonces: CARD-FACE-3 cambió código y escena, no
documentos. No se reconstruyó ningún `.cs`. El paquete `CARD-FACE-3-DOC_Documentation_Package.md` era
**adjunto de chat** (nunca estuvo en el PK) y queda **CONSUMIDO**; se descarta con los demás adjuntos.

### Ediciones de CONV-TINT-1 + 1b (2026-09-27)

| Fichero | Acción | Estado |
|---|---|---|
| `AudienceCharacterBase.cs` | **NÚCLEO, EDITADO** — `RefreshSpriteTint` + `RefreshStatePresentation` (único escritor de color y ocultación), setter de `IsBlocked` y `OnConvinced` delegan, hover «oscurito» por `IsBlockedPresented`. | **refrescada 2026-09-27** (copia del repo post-1b) |
| `AudienceCharacterCanvas.cs` | **NÚCLEO, EDITADO** — `SetConvincedHidden` (`CanvasGroup` de estado en la raíz) + dos puertas de tooltip. | **refrescada 2026-09-27** (copia del repo post-1b); cierra su marca RANCIA (que además era falsa, F-CT-9) |
| `CharacterCanvas.cs` | Leído, **no editado**. Copia del repo de hoy idéntica a la del PK. | marca RANCIA retirada (F-CT-9) |
| `GigPresentationSO.cs` | **Capa 3, EDITADO** — `convincedSpriteTint` + `DefaultConvincedSpriteTint`. Ruta: `Assets/Scripts/Data/Gig/GigPresentationSO.cs` (F-CT-7). No entra al PK; toda copia anterior al 2026-09-27 es rancia. | sin acción de PK |
| `CONV-TINT-1-DOC_Package.md` | Retenido en el PK desde 2026-09-27 (pendiente retenido, consumidor SONG-TAIL-1); **CONSUMIDO en SONG-TAIL-1**. | **se retira del PK** al cerrar SONG-TAIL-1 |
| Asset | `GigPresentation` (demo): `convincedSpriteTint` en azul de fondo. Carta `dev_convince` importada. | no son ficheros del PK |

**Adjuntos del chat a descartar:** `GigManager.cs`, `AudienceCharacterStats.cs`, `CharacterBase.cs`,
`GigPresentationSO.cs`, las copias de `AudienceCharacterBase.cs` / `AudienceCharacterCanvas.cs` /
`CharacterCanvas.cs` adjuntadas (pre-1b), ambos runbooks, capturas y salidas de grep (el paquete sale del PK en el cierre de SONG-TAIL-1).

### Ediciones de DOC-POOL-1 (2026-09-27)

| Fichero | Acción | Estado |
|---|---|---|
| Los trece documentos de v18.4 | **Sustituidos** (reemplazo completo sobre copias del PK frescas). | filas de §A.2 con fecha 2026-09-27 |
| `PAUSE-1_DocDiffs_Pending.md` · `BEAT-1_Doc_Package.md` | **CONSUMIDOS** y retirados del PK. | §B.1 |
| `HandController.cs` | Sin edición; fila §A.2 verificada por contenido (paso 35 de PAUSE-1). | sin acción |

**Adjuntos del chat a descartar al cerrar DOC-POOL-1:** ninguno (la sesión trabajó sólo con copias del PK).

### Ediciones de BEAT-1 (2026-09-27)

| Fichero | Acción | Estado |
|---|---|---|
| `MidiMusicManager.cs` · `GigManager.cs` · `GigCanvas.cs` · `VibeReadoutBeatPulse.cs` · `FloatingTextMidiListener.cs` | **Capa 3, EDITADOS.** No entran al PK. Toda copia anterior al 2026-09-27 es **rancia** (beat grid, respaldo del readout, floaters de tempo). | sin acción de PK |
| `MidiEventInterfaces.cs` | **BAJA** (F-B1-1). Ver §B.6. | retirado 2026-09-27 |
| `BEAT-1_Doc_Package.md` | **PENDIENTE RETENIDO** en el PK. Consumidor: **DOC-POOL-1**, después de `PAUSE-1_DocDiffs_Pending.md` (absorbe sus pasos 15 y 24). Si DOC-POOL-1 cierra sin consumirlo, se declara perdido en esa clausura (D-DOC-5). | **CONSUMIDO en DOC-POOL-1 (2026-09-27); retirado del PK** |
| Prefabs | `GigCanvas.prefab` (`logBeats: 0`; la línea huérfana `selfTimerWhenGridSilent` desaparece al guardar) · `MidiMusicManager.prefab` (`showTempoSignature: 0`). | no son ficheros del PK |

**Adjuntos del chat a descartar al cerrar BEAT-1:** los doce `.cs` subidos (`MidiMusicManager`, `GigManager`,
`CompositionSession`, `MidiToolkitAdapter`, `GigCanvas`, `VibeReadoutBeatPulse`, `BeatPulseIndicator`,
`UIPulseAnimator`, `MidiEventInterfaces`, `FloatingTextMidiListener`, `MusicianMidiResponder`,
`TempoTextIndicator`), `beat1_grep.txt`, `beat1_scene_census.txt`, capturas y logs. Ninguno entra al PK.

### Ediciones de PAUSE-1 (2026-09-25)

| Fichero | Acción | Estado |
|---|---|---|
| `HandController.cs` | **NÚCLEO, EDITADO 2026-09-25** — puerta `PauseGate` en `Update` y `ReleaseHeldCardToHand`. Cierra también el «a refrescar» de SONG-PANEL-1. | **refrescada 2026-09-25** (copia del repo post-PAUSE-1) |
| `MidiMusicManager.cs` · `AudioManager.cs` · `MusicDirector.cs` · `UIManager.cs` · `MainMenuController.cs` | **Capa 3, EDITADOS.** No entran al PK; se piden por FRP. | sin acción de PK |
| `MidiToolkitAdapter.cs` | **Capa 3, REEMPLAZO COMPLETO** (también pasa de cp1252 a ASCII). No entra al PK. | sin acción de PK |
| `IMidiTransportSuspend.cs` · `PlayerAudioPrefs.cs` · `PauseGate.cs` · `OptionsPanelController.cs` · `PauseMenuController.cs` | **Capa 3, NUEVOS.** No entran al PK. | en `Repo_Tree_Index.md` tras regenerar |
| `PAUSE-1_DocDiffs_Pending.md` | **PENDIENTE RETENIDO** en el PK. Consumidor: **DOC-POOL-1**; si DOC-POOL-1 cierra sin consumirlo, se declara perdido en esa clausura (D-DOC-5). | **CONSUMIDO en DOC-POOL-1 (2026-09-27); retirado del PK** |
| Prefabs / escena | `UIManager/PauseCanvas` (sort 110), `OptionsPanel.prefab`, instancia + botón en el menú principal. | no son ficheros del PK |

**Adjuntos del chat a descartar al cerrar PAUSE-1:** los diez `.cs` subidos, `Design_Tutorial_System_v0_3.md`,
`pause1_grep.txt`, la captura de la jerarquía DDOL y los logs. Ninguno entra al PK.

### Ediciones de CARD-FACE-3 / CARD-FACE-3-DOC (2026-09-22 / 2026-09-25)

| Fichero | Acción | Estado |
|---|---|---|
| `CardBase.cs` | **NÚCLEO, EDITADO 2026-09-22** — `OnPointerUp` ya no re-muestra la pila incondicionalmente: sólo con botón izquierdo y con el modal cerrado (O-CF2b-2). | **refrescada 2026-09-25 (CARD-FACE-3-DOC-b)**, verificada por contenido |
| `GameSymbols.cs` | **NÚCLEO, EDITADO 2026-09-22** — corregida la deriva de comentario que la fila §A.2 anotaba: la cabecera nombra `GameSymbolsCoverageMenu` como pase de cobertura. | **refrescada 2026-09-25 (CARD-FACE-3-DOC-b)**, verificada por contenido |
| `CardFacePresentation.cs` · `GameSymbolsCoverageMenu.cs` (`Check()` público) · `UIManager.cs` · `CardDetailViewController.cs` | **Capa 3, EDITADOS.** No entran al PK; se piden por FRP. | sin acción de PK |
| `GameSymbolsTableMenu.cs` | **Capa 3, NO DECLARADO** por el lote ni por ningún otro (F-CF3DOC-1): exportación HTML de solo lectura, autoría `SYM-TABLE-1` sin registro. Leído y documentado en §20.14 el 2026-09-25 (`governs:` +1). No entra al PK. | en `Repo_Tree_Index.md` · adjunto descartado |
| `GameSymbolsSyncMenu.cs` | **Capa 3, NUEVO** (`Assets/Scripts/Editor/`). Gobernado por `SSoT_Editor_Authoring_Tools.md` §20.14 (`governs:` +1). No entra al PK. | presente en `Repo_Tree_Index.md` (snapshot 2026-09-25 13:04) |
| Escena / prefab | `UIManager/GigCanvas/SongsLeftText` activado; `TooltipText` `preferredWidth` 280. | no son ficheros del PK |

**Adjuntos del chat a descartar al cerrar CARD-FACE-3:** los siete `.cs` del lote, `TMP_SpriteCharacter.cs`,
`TMP_SpriteGlyph.cs`, `UIManager.cs`, `manifest.json`, `packages-lock.json`, `ProjectVersion.txt` y las 14
capturas. **Al cerrar CARD-FACE-3-DOC:** `CARD-FACE-3-DOC_Documentation_Package.md`. Ninguno entra al PK.

**Versión 17 — 2026-09-21 (CARD-FACE-1/2-DOC).** Cambios en v17: **dos altas en §A.2** —
`GameSymbols.cs` y `PartEffectDescriptionBuilder.cs`, presentes en el PK **sin fila** desde CARD-FACE-1 —,
**cuatro refrescos confirmados por contenido** (`CardBase.cs`, `StatusEffectCatalogueSO.cs`,
`SongCompositionUI.cs` —cierra su deuda RANCIA— y `Repo_Tree_Index.md`, snapshot 2026-09-21 12:02),
**diez documentos sustituidos por reemplazo completo** (`SSoT_Game_Text.md`, `SSoT_Card_System.md`,
`SSoT_Editor_Authoring_Tools.md`, `SSoT_Status_Effects.md`, `SSoT_Dev_Mode.md`, `SSoT_INDEX.md`,
`ssot_manifest.yaml`, `CURRENT_STATE.md`, `changelog-ssot.md`, `coverage-matrix.md`) más
`S5_DemoCutClose_Sub_Roadmap.md` y este manifiesto.

**R4 en este lote: sin excepción.** Los documentos se editaron sobre sus copias del PK tras comprobar la
puerta de frescura: ningún doc-pass había tocado esos documentos desde TEXT-VIBE-1-DOC (2026-09-16),
porque CARD-FACE-1 y CARD-FACE-2 **retuvieron** su documentación. No se reconstruyó ningún `.cs`.

**Deriva física del PK saneada (D-PK=A).** El inventario de apertura encontró que el PK real no
coincidía con este manifiesto:
- 8 ficheros presentes sin fila.
- 5 marcados RETIRADA que seguían presentes.
- 2 filas sin fichero: `GigMessageUI.cs` y `PENDING_DOC_DIFFS_RFX-1.md`, que ya son Capa 3 o están
  retirados; sólo se anota.

Acciones:

| Fichero | Acción | Motivo |
|---|---|---|
| `PENDING_DOC_PACKAGE_CARD-FACE-1.md` | **SALE** → §B.1 | consumido en CARD-FACE-1/2-DOC (corregido con §D3 del paquete 2 antes de aplicarlo) |
| `PENDING_DOC_PACKAGE_CARD-FACE-2.md` | **SALE** → §B.1 | consumido en CARD-FACE-1/2-DOC (D1..D8) |
| `Roadmap_Update_CARD-FACE-1_close.md` | **SALE** → §B.1 | nunca aplicado; **sustituido** por la edición consolidada de S5 (D-DOC=A) |
| `GigManager.cs` (190 KB) | **SALE** | Capa 3 por §B.2; entró como adjunto de CARD-FACE-1 y se quedó. Es el mayor coste de contexto del PK |
| `AudienceCharacterStats.cs` · `AudienceMoveToFrontAction.cs` | **SALEN** | Capa 3 por §B.2 / BIGNUM-1; presentes sin fila |
| `TutorialController.cs` · `TutorialGuidedDriver.cs` · `TutorialDialogSO.cs` · `TutorialDialogCatalogSO.cs` · `Design_Tutorial_System_v0_3.md` | **SALEN (retirada física pendiente desde 2026-09-05)** | su fila ya decía RETIRADA; el PK no la siguió |
| `CharacterCanvas.cs` | queda como estaba (Fase 3-B) | CARD-FACE-2 lo leyó y **no lo editó** |

**Capa 3 de la serie (se descartan del chat, no entran al PK):** `TonalityEffect.cs`, `MoodTag.cs`,
`CompositionStripThemeSO.cs`, `CompositionContextRowUI.cs`, `PartActionDescriptor.cs`, `PartActionKind.cs`,
`CardType.cs`, `SongTrackElementUI.cs`, `TrackHoverPanel.cs`, `MeterEffect.cs`, `TempoEffect.cs`,
`GigCanvas.cs`, `PartEffectEditorWindow.cs`, `StatusEffectWizardWindow.cs`, `GameTextWindow.cs`,
`GameTextTemplateIds.cs`, `CardDefinitionDescriptionExtensions.cs`, `CardEffectDescriptionBuilder.cs`,
`SymbolMetricsTuner.cs`, `GameSymbolsDumpMenu.cs`, `GameSymbolsCoverageMenu.cs`,
`CardFacePresentation.cs`, `DevModeController.cs`, `ConceptGlossaryTextTable.cs`,
`CardDetailViewController.cs`, `CardData.cs`, `CardInventoryWindow.cs`, `DeckEditorWindow.cs`,
`MusicTheory.Tonality.cs` (MidiGenPlay: sólo chat, FRP §3), los CSV `StatusCatalogue_*`, `card_face_2.png` y
los exports intermedios de GameText. `SymbolSheetSlicer.cs`, que el paquete 1 listaba, **no existe**.

**Golden:** `CardText_en_20260921_102034.txt` → versionado como `golden_I.txt` **fuera del PK**.

**Versión 16 — 2026-09-16 (TEXT-VIBE-1 / TEXT-VIBE-1-DOC).** Cambios en v16: **ningún alta y ninguna
baja** — ni de documento ni de código. **Nueve documentos sustituidos por reemplazo completo**
(`SSoT_Game_Text.md`, `Design_Vibe_Telegraph_v0_1.md`, `SSoT_Editor_Authoring_Tools.md`,
`SSoT_ALWTTT_MidiGenPlay_Boundary.md`, `SSoT_Card_System.md`, `CURRENT_STATE.md`,
`changelog-ssot.md`, `coverage-matrix.md`) más este manifiesto. **`SSoT_INDEX.md` y
`ssot_manifest.yaml` NO se tocan y eso es un resultado, no un olvido:** el lote no creó documentos ni
movió autoridad de ficheros, y se comprobó antes de afirmarlo.

**Dos copias de núcleo quedan RANCIAS y se marcan en §A.1** (`AudienceCharacterCanvas.cs`,
`SongCompositionUI.cs`): el lote las editó y el PK conserva las copias de TEXT-5, de 2026-09-11. No se
refrescan en esta sesión porque ningún lote inmediato las necesita — CARD-FACE-1 abre por la cara de
carta y por `PartEffect`, no por estos dos —, así que la deuda se **registra con su disparador** en vez
de pagarse a ciegas: quien vuelva a anclar una edición en cualquiera de las dos pide copia fresca
primero (puerta de frescura del FRP).

**R4 en este lote: sin excepción, y con una lección nueva.** Los dos reemplazos completos de `.cs`
(`CompositionFxConfigSO.cs`, `CompositionStripThemeSO.cs`) se construyeron sobre **copias frescas del
repo adjuntadas en la misma sesión**. Los nueve documentos se editaron sobre sus copias del PK, cuya
fecha es 2026-09-15 (TIP-1-DOC) y a las que ningún lote cerrado ha tocado desde entonces: la puerta de
frescura se comprueba y pasa. **Lo que v16 añade a R4 es el caso inverso** (F-TV-5): una copia fresca
del repo garantiza la verdad del **código**, pero no la de un **dato transcrito a mano**. En este lote
la lista de ids de plantilla se escribió leyendo una captura de pantalla en vez de la salida de la
herramienta, y entró un id de un valor de enum inexistente que desplazó a uno alcanzable en la demo.
La regla que deja, ya escrita en `SSoT_Game_Text` §8: **una familia de ids derivados se importa desde
la salida de cobertura de la ventana, nunca desde un documento ni desde un mensaje de chat.**

### Ediciones de TEXT-VIBE-1 / TEXT-VIBE-1-DOC (2026-09-16)

| Fichero | Acción | Estado |
|---|---|---|
| `AudienceCharacterCanvas.cs` | **NÚCLEO, EDITADO** (dos pasos anclados: `VibeOnlyHeader` pasa a leer el concepto `vibe` — F-TV-3 —, y `BuildVibeText` suprime por salida==entrada en crudo — D1/F-TIP-6). | **fila de §A.2 marcada RANCIA** · refresco pendiente con disparador declarado |
| `SongCompositionUI.cs` | **NÚCLEO, EDITADO** (dos pasos anclados: `SpawnCompositionFx` deja de leer `entry.label` y formatea por glosario; alta de `FxTextIdOf`, mapa entrada→id por identidad de referencia). | **fila de §A.2 marcada RANCIA** · refresco pendiente con disparador declarado |
| `CompositionFxConfigSO.cs` | **Capa 3, REEMPLAZO COMPLETO** (`FxEntry.label` **borrado**, D-SP1-6=B) y transcodificado de Windows-1252 a ASCII puro (**F-TV-2**). | fila de Capa 3 · `nonASCII=0` verificado en disco |
| `CompositionStripThemeSO.cs` | **Capa 3, REEMPLAZO COMPLETO** (los `displayName` de `MeterEntry` y `TempoEntry` **borrados**; `TryGetMeter`/`TryGetTempo` pierden `out string label`; `RoleEntry.displayName` **intacto y declarado abierto**) y transcodificado (**F-TV-2**). | fila de Capa 3 · `nonASCII=0` verificado en disco |
| `CompositionContextRowUI.cs` | **Capa 3, EDITADO** (cinco pasos anclados: los dos `Bind` pierden el `out _`, el hover formatea `UiCtxMeter`/`UiCtxTempo`, dos comentarios de censo corregidos). | fila de Capa 3 actualizada · `nonASCII=0` |
| `GameTextTemplateIds.cs` | **Capa 3, EDITADO** (`All` **73 → 102** y, más importante, **deja de ser una lista literal**: `BuildAll()` enumera `TimeSignature` y `TempoRange`). | fila de Capa 3 actualizada |
| `GigCanvas.cs` | **Capa 3, EDITADO** (población 11: seis ids `ui_readout_*`, cabecera incluida; D1 en la línea de SFX). **No está en el PK** y no entra. | sin acción de PK |
| `GigManager.cs` · `VibeProjection.cs` | **LEÍDOS, no editados.** `GigManager` aportó el redondeo que fija la comparación de D1 (`ComputeLPart`, `ApplyFlowToLPart`) y destapó **F-TV-4**. | sin acción de PK; `VibeProjection.cs` conserva su fila |
| `FxManager.cs` · `CompositionStripThemeSO` consumidores | **LEÍDOS.** `FxManager` confirmó por dónde viaja el label de la población 8. **`SongTrackElementUI.cs` / `TrackHoverPanel.cs` NO se leyeron**, y por eso `RoleEntry.displayName` queda abierto. | sin acción de PK |
| Assets `ConceptGlossary_EN` / `_ES` | **Editados por import de CSV** (29 filas nuevas) + **borrado manual de un huérfano** en el Inspector de ambos (la pestaña no borra). | no son ficheros del PK |

**Adjuntos del chat a descartar al cerrar:** los ocho `.cs` del lote, los dos CSV de plantillas (A y
B), el CSV de arreglo de una fila, el export previo `GameText_Templates_20260916_1630.csv` y el
volcado `CardText_en_20260916_164613.txt` — **este último NO se descarta sin guardarlo antes**: es
`golden D`, la línea base nueva del oráculo de regresión, y su predecesor se perdió exactamente así
(**F-TV-6**). Va a control de versiones, no a Descargas.

**Versión 15 — 2026-09-15 (TIP-1 / TIP-1-DOC).** Cambios en v15: **un alta en §A.2** —
`SSoT_Tooltip_Pipeline.md`, documento **nuevo** creado por el doc-pass (D-TIP-1=A) — y **ninguna copia
de código del núcleo editada**: los tres `.cs` del lote
(`TooltipManager.cs`, `TooltipController.cs`, `VibeReadoutTooltipTarget.cs`) son **Capa 3** y se piden
por lote. **Ocho documentos sustituidos por reemplazo completo** (`SSoT_Game_Text.md`,
`SSoT_Card_System.md`, `Design_Vibe_Telegraph_v0_1.md`, `SSoT_INDEX.md`, `coverage-matrix.md`,
`ssot_manifest.yaml`, `CURRENT_STATE.md`, `changelog-ssot.md`), más `Repo_Tree_Index.md` (Capa 2) y
este manifiesto. **Nada de código entró ni salió del PK.**

**R4 en este lote: sin excepción.** Los tres reemplazos completos de `.cs` se construyeron sobre
**copias frescas del repo adjuntadas en la misma sesión**, y los diez documentos editados se editaron
**sobre sus copias del PK, que son la versión viva**: su fecha de copia es 2026-09-14 (doc-pass de
SONG-PANEL-1) y **ningún lote cerrado las ha tocado desde entonces** — TIP-1 cambió código, no
documentos. La puerta de frescura del FRP se comprueba y se pasa; no es una excepción declarada como
la de `SongCompositionUI.cs` en v14.

### Ediciones de TIP-1 / TIP-1-DOC (2026-09-15)

| Fichero | Acción | Estado |
|---|---|---|
| `SSoT_Tooltip_Pipeline.md` | **ALTA, documento nuevo** (Capa 1). Hogar del transporte de tooltips: tres entradas, tabla de proyección por root canvas, orden de clamps, ciclo asignar → reconstruir → medir → colocar, censo de ocho superficies, tres invariantes duras. Registrado en `SSoT_INDEX.md`, `ssot_manifest.yaml`, `coverage-matrix.md` y `Repo_Tree_Index.md` **en la misma sesión** (alta de documento = cambio de autoridad). | fila nueva en §A.2 |
| `SSoT_Game_Text.md` | **Sustituido.** §1 población 11 (F-TIP-2) y recuento 11/8/3; §4 reescrito (F-BN-7/F-BN-8 cerradas, conducta reenunciada); §8 disparador repuntado; §9 fila. **Pierde autoridad sobre cinco ficheros** de `Assets/Scripts/UI/Tooltips`. | fila de §A.2 con fecha nueva |
| `SSoT_Card_System.md` · `Design_Vibe_Telegraph_v0_1.md` · `SSoT_INDEX.md` · `coverage-matrix.md` · `ssot_manifest.yaml` · `CURRENT_STATE.md` · `changelog-ssot.md` | **Sustituidos** (reemplazo completo). | filas de §A.2 con fecha nueva |
| `Repo_Tree_Index.md` | **Actualizado a mano**, no regenerado: una ruta nueva en `Docs/systems` (13 → 14 ficheros, total 473 → **474**). Ningún `.cs` nuevo, borrado ni movido, así que no procede regenerar con el script. | Capa 2, sin fila de §A.2 |
| `TooltipManager.cs` · `TooltipController.cs` · `VibeReadoutTooltipTarget.cs` | **Capa 3, EDITADOS** (reemplazo completo los tres). Rutas: `Assets/Scripts/UI/Tooltips/`. Gobernados por `SSoT_Tooltip_Pipeline.md` desde este doc-pass. **No entran al PK**: son costuras de un pipeline cuyo contrato ya vive en un SSoT, que es justo lo que hace innecesaria la copia. | sin acción de PK |
| `GigCanvas.cs` · `StatusIconBase.cs` · `TooltipText.cs` · `VibeBarTooltipTarget.cs` · `ITooltipTargetBase.cs` · `I2DTooltipTarget.cs` · `TrackHoverPanel.cs` | **Capa 3, LEÍDOS y no editados.** `GigCanvas.cs` se pidió por una hipótesis falsa (el anchor de runtime no vivía ahí); el dato queda registrado para no repetir la petición. `TrackHoverPanel.cs` se leyó como **referencia de colocación** y su aritmética de clamps quedó citada en el SSoT nuevo §3. | sin acción de PK |
| Prefab `TooltipController` | **Leído en el inspector** (VerticalLayoutGroup + ContentSizeFitter, pivot/anchors (0,0); canvas Overlay sort order 10). **No modificado** por el lote. | no es fichero del PK; registrado para trazabilidad |

**Adjuntos del chat a descartar al cerrar:** los diez `.cs` del lote y las dos capturas de inspector.
Ninguno entra al PK.

**Deuda de recuento (registrada, no resuelta aquí).** La última versión que declaró un total absoluto
fue **v9 (83 ficheros, TXT-2-DOC)**; v10–v14 registraron altas y retiradas sin recalcularlo, así que el
número de v9 lleva cinco versiones sin actualizarse. Contando las filas reales de §A.2 en esta versión
salen **94**, que no cuadra con 83 + las altas registradas: o hay filas de ficheros ya retirados que
siguen en la tabla, o altas sin registrar en las cabeceras. **v15 no inventa un total**: declara su
delta (**+1 alta**, `SSoT_Tooltip_Pipeline.md`) y deja el recuento absoluto como tarea de una pasada de
auditoría del PK, que es trabajo de conciliación tabla-contra-PK-real y no de este lote. Se registra
aquí para que la próxima versión no repita el número de v9 como si fuera cierto.

**Versión 14 — 2026-09-14 (SONG-PANEL-1 / 1b / 1c).** Cambios en v14: **una copia del núcleo
editada** — `SongCompositionUI.cs`, cuyo refresco estaba previsto desde v13 y queda **cumplido**;
`FloatingTextMidiListener.cs` cierra su refresco abierto desde RFX-1 **por retirada** (es Capa 3 y no
gana nada por estar en el PK); alta del bloque §A.1 «Refrescos y ediciones de SONG-PANEL-1»; censo de
Capa 3 con **nueve ficheros editados** y tres leídos sin tocar. **Ocho documentos sustituidos por
reemplazo completo** (`SSoT_Game_Text.md`, `SSoT_Card_System.md`, `SSoT_Dev_Mode.md`,
`CURRENT_STATE.md`, `changelog-ssot.md`, `coverage-matrix.md`, `ssot_manifest.yaml`,
`Design_Action_Economy_v1.md`). **Nada entró al PK en este lote.**

**Lección de procedimiento que v14 registra (R4).** El reemplazo completo de `SongCompositionUI.cs`
se generó **desde la copia del PK**, no desde una copia fresca del repo, porque el fichero no se
editó en TEXT-6 y su fila estaba vigente. Es una **excepción declarada** a R4, mitigada con:
verificación de integridad de la copia (1651 líneas, llaves balanceadas, fin de fichero intacto),
aserción de unicidad de las veinte anclas antes de sustituir, y `git diff` obligatorio antes de
sobrescribir. Se registra para que la próxima excepción sea una decisión y no una costumbre.

### Refrescos y ediciones de SONG-PANEL-1 (2026-09-14)

| Fichero | Acción | Estado |
|---|---|---|
| `SongCompositionUI.cs` | **NÚCLEO, EDITADO** (veinte ediciones: helpers de glosario, `Awake` → `Start`, letras de parte, borrado de los tres `[SerializeField]` de texto, **supresión de la reasignación de `track.info`** — F-T6-8). Entregado como **reemplazo de fichero completo**; ver la nota R4 de arriba. | **fila de §A.2 refrescada a 2026-09-14** · refresco previsto de v13 **cumplido** |
| `FloatingTextMidiListener.cs` | **EDITADO** (gate de compilación `DevFloaters`, fallback `Chord (…)` a plantilla). Su copia del PK estaba **rancia desde RFX-1** y **no se usó**: el lote trabajó sobre copia fresca del repo. | **RETIRAR del PK** → §B, Capa 3. Cierra el refresco abierto desde RFX-1 |
| `TrackHoverPanel.cs` | **Capa 3, EDITADO** (clamps de borde de pantalla, fondo, negritas, toggle dev dentro del `#if`, rebuild de layout antes de medir) y **re-transcodificado**: estaba **de vuelta en cp1252** (F-SP1-2). | fila de Capa 3 actualizada · **`nonASCII=0` verificado en disco** |
| `CompositionContextRowUI.cs` | **Capa 3, EDITADO** (población 9 migrada, rama singular/plural, `Loop` sin total) y transcodificado (F-SP1-7). | alta de fila de Capa 3 · `nonASCII=0` verificado |
| `CompositionStripDriver.cs` | **Capa 3, EDITADO sólo en codificación.** Diff funcional cero. | fila de Capa 3 actualizada |
| `GameTextTemplateIds.cs` | **Capa 3, EDITADO** (`All` 54 → **73**). | fila de Capa 3 actualizada |
| `GigManager.cs` | **EDITADO** (`LogsEnabled` / `VerboseLogsEnabled`; `DevSetSongHypeAbsolute` fuera del `#if`). Copia fresca del repo adjunta al chat. | si tiene fila de núcleo: **refrescar**; si no, fila de Capa 3 |
| `HandController.cs`, `DeckManager.cs` | **EDITADOS** (gates de log). Copias frescas del repo. | filas de núcleo/Capa 3 **a refrescar** |
| `CompositionSession.cs` | **EDITADO** (gates de log, con cambio de semántica declarado). Copia fresca del repo. | fila de Capa 3 actualizada |
| `SongTrackElementUI.cs`, `SongPartElementUI.cs` | **LEÍDOS, no tocados.** Confirmaron que `RowData.info` es la etiqueta de fila y que D-SP1-1=A no obliga a tocar el constructor de filas. | filas de Capa 3 con «leído, no editado» |
| Prefab `TrackHoverPanel` | **Editado en el inspector** (fondo `Panel`, anchors, `edgeMargin`). | no es fichero del PK; registrado aquí para trazabilidad |

**Adjuntos del chat a descartar al cerrar:** los diez `.cs` del lote, el CSV de plantillas, el golden C
y las capturas de inspector. Ninguno entra al PK.

**Versión 13 — 2026-09-14 (TEXT-6).** Cambios en v13: **ninguna copia del núcleo del PK fue editada
por el lote** — TEXT-6 tocó dos ficheros de **Capa 3** (`GameTextTemplateIds.cs`, editado;
`TrackHoverPanel.cs`, editado y **transcodificado**) y leyó un tercero sin tocarlo
(`SongTrackElementUI.cs`). `SongCompositionUI.cs` **no se editó**, así que su fila de §A.2 sigue
vigente con fecha 2026-09-11 — pero **queda marcada como la que TEXT-6b va a editar**, y con un
hallazgo nuevo verificado contra ella (**F-T6-8**, la reasignación de `track.info`). Alta de bloque
§A.1 «Refrescos y ediciones de TEXT-6». **Siete documentos sustituidos por reemplazo completo**
(`SSoT_Game_Text.md`, `SSoT_Audience_and_Reactions.md`, `SSoT_Editor_Authoring_Tools.md`,
`coverage-matrix.md`, `CURRENT_STATE.md`, `changelog-ssot.md` y este fichero). **Adjuntos del chat de
TEXT-6 a descartar:** `TrackHoverPanel.cs`, `SongTrackElementUI.cs`, los cinco `.asset` de público,
`ActionTargetType.cs`, `CharacterActionType.cs`, los tres CSV de export, el golden y las dos capturas.
**`Repo_Tree_Index.md` no se regenera:** el lote no creó ni renombró ficheros (verificado — la única
alta es el reemplazo de un fichero existente).

**Regla que TEXT-6 confirma y conviene no perder:** `TrackHoverPanel.cs` y `SongTrackElementUI.cs`
estaban en Capa 3 con **existencia verificada y contenido no leído** desde v12, y esa anotación fue
exactamente la que permitió pedirlos por nombre exacto en el paso 1 del FRP sin adivinar rutas. Una
fila de Capa 3 que registra «existe, no leído» vale lo que cuesta.

**Versión 12 — 2026-09-14 (TEXT-5-DOC).** Cambios en v12: bloque **§A.1 → «Refrescos y altas de
TEXT-5»** con las **cinco costuras de núcleo** que el lote dejó frescas en el PK (`CardBase.cs`,
`SongCompositionUI.cs`, `AudienceCharacterCanvas.cs`, `AudienceCharacterData.cs` y el **alta** de
`GameText.cs`), el **cierre de los cuatro refrescos que TXT-4 dejó abiertos**, y el censo de los
**nueve ficheros editados o creados que NO entran al PK** (Capa 3), uno de ellos nuevo.
**`SongCompositionUI.cs` deja de estar rancia desde R6**: llevaba tres lotes marcada y bloqueando un
paso concreto de TEXT-5, y además **estaba físicamente en el PK sin fila en §A.2** mientras §B.2 la
listaba como retirada — **cuarta ocurrencia** de la familia de contradicción de `CharacterCanvas.cs`,
`MusicianBase.cs` y `AudienceCharacterCanvas.cs` (F-DOC4-2), resuelta igual: dándole fila y **sin**
ejecutar la retirada. **`GameText.cs` cambia de capa**: TXT-4 lo censó como Capa 3 y TEXT-5 lo subió al
PK porque es la entrada única de todo el texto compuesto; gana fila de alta. **`Repo_Tree_Index.md`
regenerado y sustituido** (snapshot **2026-09-14 12:30**, **473 ficheros**; el ejemplar anterior del PK
era el del 2026-09-11 14:47, 472): incorpora el fichero nuevo del lote, verificó la ruta del `governs:`
nuevo del manifiesto y confirmó que los dos candidatos de **F-T5-2** (`TrackHoverPanel.cs`,
`SongTrackElementUI.cs`) **existen** — existencia verificada, **contenido no leído**. Los trece
documentos del doc-pass **sustituidos por reemplazo completo**. Adjuntos del chat de TEXT-5:
**descartados**.

**Censo de Capa 3 de TEXT-5 (editados o creados, fuera del PK por presupuesto de contexto, no por
autoridad — regla de invariantes del proyecto):** `ConceptGlossarySO.cs` · `GameTextTemplateIds.cs` ·
`ConceptGlossaryTextTable.cs` · `GameTextWindow.cs` · `AudienceMemberWizard.cs` ·
`CardDetailViewController.cs` · `SensoryFtPresentation.cs` · `AudienceIntentionData.cs` ·
**`AudienceAbilityTextIndex.cs` (nuevo)**. Un lote que toque cobertura de texto, el wizard de público o
el modal de detalle debe pedirlos por el File Request Protocol; ninguno gana ni pierde autoridad por no
estar aquí.

**Versión 11 — 2026-09-11 (TXT-4-DOC).** Cambios en v11: bloque **§A.1 → «Refrescos, altas y
contradicciones de TXT-4»** con los dos ficheros de núcleo que el lote editó (`CardBase.cs`,
`AudienceCharacterCanvas.cs`), la **procedencia nueva de `CardDefinition.cs`** (el lote no lo editó,
pero cambió el contrato de uno de sus campos y su fila decía «no registrada»), y el censo de los
**diez ficheros editados o creados que NO entran al PK** (Capa 3), cinco de ellos nuevos. **F-DOC4-2**:
`AudienceCharacterCanvas.cs` estaba físicamente en el PK **sin fila en §A.2** y figuraba como retirada
en §B.2 — tercera ocurrencia de la misma familia tras `CharacterCanvas.cs` y `MusicianBase.cs`;
resuelta dándole fila y marcándola rancia, **sin ejecutar la retirada**, porque TIP-1 necesitará esa
costura. `SongCompositionUI.cs` **sigue rancia desde R6 y ahora bloquea un paso concreto de TEXT-5**.
Los doce documentos del doc-pass **sustituidos por reemplazo completo**. **`Repo_Tree_Index.md`
REGENERADO** (snapshot **2026-09-11 14:47**, **472 ficheros**): la deuda de **siete lotes** queda
**saldada**, y la regeneración pagó de paso tres verificaciones — las cinco rutas nuevas del
`governs:` del manifiesto, la existencia de `GrantBonusLoopSpec.cs` (F-DOC4-1) y el hecho de que
**`AudienceAbilityData.cs` no existe como fichero** (es una clase anidada en `AudienceCharacterData.cs`).

**Versión 10 — 2026-09-10 (TXT-3).** Cambios en v10: bloque **§A.1 → «Refrescos y bajas de TXT-3»**
con los dos refrescos del lote (`StatusEffectSO.cs`, `CardBase.cs` — esta última **gana procedencia
por primera vez**) y el **cierre de los dos refrescos abiertos desde BIGNUM-1** (`CharacterCanvas.cs`
y `AudienceCharacterCanvas.cs`: TXT-3 no las editó, pero se adjuntaron copias frescas del repo el
2026-09-10 y sirven de sustitución verificada); **baja de `SpecialKeywordData.cs`** del censo de §B.2
(fichero y asset eliminados del proyecto, D-TXT3-8); censo de los **ocho ficheros editados que no
entran al PK**; los once documentos del doc-pass **sustituidos por reemplazo completo**.
`Repo_Tree_Index.md` **NO regenerado** — deuda de seis lotes.

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

#### Refrescos y ediciones de TEXT-6 (2026-09-14)

Lote de código, datos y documentación cerrado en una sola sesión. **Consecuencia para el PK: casi
nada.** Lo que cambió vive en Capa 3, y la única fila del núcleo afectada lo está por un **hallazgo**,
no por una edición.

| Fichero | Acción | Estado |
|---|---|---|
| `GameTextTemplateIds.cs` | **Capa 3, EDITADO** (tres pasos anclados: comentario de familias, seis consts `ui_track_*`, `All` 48 → 54). **No entra al PK**: sigue siendo Capa 3 por presupuesto de contexto, igual que en TEXT-5. Quien lo necesite lo pide fresco. | sin fila de §A.2 · censado aquí |
| `TrackHoverPanel.cs` | **Capa 3, EDITADO y TRANSCODIFICADO** (cp1252 sin BOM → UTF-8 con BOM; F-T6-1). Entregado como **reemplazo de fichero completo**, no como anclas, porque sus separadores eran el byte crudo `0xB7` y un ancla que lo contuviera no es estable entre editores. Reconstruido desde copia fresca del repo de la misma sesión (regla R4 cumplida). | sin fila de §A.2 · censado aquí |
| `SongTrackElementUI.cs` | **Capa 3, LEÍDO y NO tocado.** Confirmó que los literales de F-T5-2 no estaban ahí: la inferencia de v12 acertó el fichero. Cero literales de jugador; su único string es un `Debug.LogWarning`. | sin fila de §A.2 · censado aquí |
| `SongCompositionUI.cs` | *(refresco cumplido en v14 — ver el bloque de SONG-PANEL-1 arriba.)* **NO editado por TEXT-6.** Su copia de 2026-09-11 sigue siendo fresca y sirvió para verificar **F-T6-8** por contenido (reasignación de `track.info` tras `InstrumentEffect`) y para dimensionar **F-T6-9** (`CompositionFxConfigSO.FxEntry.label`). **TEXT-6b SÍ lo va a editar**: su fila de §A.2 queda marcada para refresco en el cierre de ese lote. | fila de §A.2 vigente · **refresco previsto en TEXT-6b** |
| Cinco `.asset` de público (`Big Guy`, `Cool Dude`, `Influencer`, `Kid`, `Kid-TestTaster`) | **Adjuntos al chat, no editados.** Fueron el **oráculo de mecánica** de las siete descripciones: sin ellos, dos de los siete cuerpos habrían sido invención. Se descartan al cierre; la tabla de acciones verificada queda en `SSoT_Audience_and_Reactions.md` §7.1, que es donde tiene que estar para no volver a pedirlos. | descartar |
| `ActionTargetType.cs`, `CharacterActionType.cs` | **Adjuntos al chat, no editados.** Resolvieron los ordinales de los assets (`6 = Self`, `100 = AudienceTall`, `3 = AddStress`, `10 = MoveToFront`, `11 = ApplyStatusEffect`). Enums pequeños y estables; **no entran al PK** — se piden cuando hagan falta. | descartar |

**Lección de contexto del lote:** el fichero más caro de este lote no fue código sino **datos** — los
cinco `.asset`. Un export del wizard que incluyera `cardActionType / actionTargetType / actionValue /
statusEffect` por acción habría evitado adjuntarlos. Queda anotado como mejora de tooling para quien
toque `AudienceMemberWizard`.

#### Refrescos y altas de TEXT-5 (2026-09-11, aplicado 2026-09-14)

Lote de código cerrado el 2026-09-11; documentación aplicada en TEXT-5-DOC el 2026-09-14, en sesión
separada. **Todos los refrescos de este bloque están hechos**: al abrir el doc-pass las cinco copias ya
estaban en el PK y se verificaron **por contenido**, no por fecha — es la razón por la que este pase se
escribió contra verdad de código y no contra inferencia (§C.1).

| Fichero | Acción | Estado |
|---|---|---|
| `GameText.cs` | **ALTA en el PK (cambia de Capa 3 a núcleo).** TXT-4 lo censó como Capa 3; TEXT-5 lo sube porque es la **entrada única** de todo el texto compuesto y ningún lote de texto puede razonar sin él. Verificado por contenido: `Number(double, string)` presente, formatea por `Glossary.NumberFormat` con `CultureInfo.InvariantCulture` como *fallback* explícito. | **cerrado 2026-09-14** · fila de alta creada en §A.2 |
| `CardBase.cs` | **Refrescado.** Cierra el refresco abierto por TXT-4. Verificado por contenido: L99 compone la etiqueta de tipo con `GameText.Format(GameTextTemplateIds.UiCardTypeComposition)` y la rama de enum queda marcada `F-TXT4-2 → CARD-FACE-1`. El literal `"COMPOSITION"` ya no está. | **cerrado 2026-09-14** |
| `SongCompositionUI.cs` | **Refrescado + contradicción de clasificación resuelta.** Cierra la rancidez **abierta desde R6 (2026-09-01)**, tres lotes. Verificado por contenido: L344 `ALWTTT.Tooltips.GameText.CardName(data)` con la nota de que las pistas se indexan por `(musicianId, role)` y nunca por esa cadena. Además **estaba en el PK sin fila en §A.2** mientras §B.2 la listaba como retirada a Capa 3: **cuarta ocurrencia** de la familia tras `CharacterCanvas.cs`, `MusicianBase.cs` y `AudienceCharacterCanvas.cs`. Se resuelve igual — fila en §A.2, **retirada no ejecutada** — porque TEXT-6 la va a necesitar. | **cerrado 2026-09-14** · fila de alta creada en §A.2 |
| `AudienceCharacterCanvas.cs` | **Refrescado.** Cierra el refresco abierto por TXT-4. Verificado por contenido: L174–181 resuelven el hover de intención por `GameText.ConceptName` / `ConceptDescription` sobre `NextAbility.AbilityId`; L138–154 formatean los números por `GameText.Number`. | **cerrado 2026-09-14** |
| `AudienceCharacterData.cs` | **Procedencia nueva.** Su fila de §A.2 databa de AMW-1 y el fichero acaba de ganar un campo de contrato. Verificado por contenido: `abilityId` presente con el tooltip que declara el contrato (`[a-z0-9_]`, prefijo `ab_`, sembrado por `ALWTTT ▸ Text ▸ Seed Ability Ids`, id vacío = error de cobertura) y `abilityName` declarado **etiqueta de desarrollo**. | **cerrado 2026-09-14** |
| `ConceptGlossarySO.cs` · `GameTextTemplateIds.cs` · `ConceptGlossaryTextTable.cs` · `GameTextWindow.cs` · `AudienceMemberWizard.cs` · `CardDetailViewController.cs` · `SensoryFtPresentation.cs` · `AudienceIntentionData.cs` · **`AudienceAbilityTextIndex.cs`** | **No entran al PK** (Capa 3). Editados o creados por el lote; se piden por lote. El único **nuevo** es `AudienceAbilityTextIndex.cs`, ya indexado en `Repo_Tree_Index.md` (snapshot 2026-09-14 12:30) en `Assets/Scripts/Editor/Text/AudienceAbilityTextIndex.cs` y gobernado por `SSoT_Game_Text.md` en `ssot_manifest.yaml` desde este doc-pass. | sin acción de PK |

**Assets de datos tocados (no indexables por `Repo_Tree_Index.md`):** los dos `ConceptGlossary` (EN/ES)
ganan **siete entradas de concepto** (36 → 43), **una plantilla** (47 → 48, `ui_card_type_composition`)
y el campo `numberCulture`. Fingerprints al cierre: Concepts
`7e22a03fa95f3c9f7d18ed93b86e1be826551f3f` · Cards `7012d4898044ef3e67c5bd2587be50786e31815b`
(**byte-idéntico a TXT-4**) · Templates `feb4add82dfa4d478c7c6a48a744c1c895d43ca5`. Los CSV de import
del lote son **intercambio**, no se commitean como semilla (misma regla que TXT-3 y TXT-4).

**Ficheros a descartar del chat al cerrar TEXT-5-DOC:** los adjuntos de la sesión de código de TEXT-5 y
todas las capturas. `Runbook_TEXT-5.md` y `Runbook_TEXT-5-DOC.md` **se retiran al aplicarse**
(convención de paquetes).

**Dato que no llegó:** qué **dos personajes de público comparten `ab_tantrum`** (D-T5-5=A). La nota de
contenido de `SSoT_Audience_and_Reactions` §7 está escrita en genérico para no inventar nombres. Se
cierra con una frase cuando aparezca; no bloquea nada.

#### Refrescos, altas y contradicciones de TXT-4 (2026-09-11)

Lote de código cerrado el 2026-09-11; documentación aplicada en TXT-4-DOC el mismo día, en sesión
separada. **Un refresco no está hecho hasta que su fila lo dice** (§C.1).

| Fichero | Acción | Estado |
|---|---|---|
| `CardBase.cs` | **Refrescar.** EDITADO 2026-09-11 — el nombre de carta se resuelve por `GameText.CardName`; el literal `"COMPOSITION"` sigue en `typeTextField` (F-TXT4-2, migrable, va a TEXT-5). Su fila de §A.2 ganó procedencia en TXT-3 y vuelve a quedar vencida. | **cerrado 2026-09-14 por TEXT-5** — ver el bloque de TEXT-5 |
| `AudienceCharacterCanvas.cs` | **Refrescar + resolver contradicción de clasificación (F-DOC4-2).** EDITADO 2026-09-11 — los 13 literales sustituidos por plantillas `ui_aud_*` y por el concepto `blocked`. Además: **estaba presente en el PK sin fila en §A.2** mientras §B.2 lo listaba como retirado a Capa 3. Misma familia de contradicción que se resolvió para `CharacterCanvas.cs` y `MusicianBase.cs` en DOC-TUTR-B; se resuelve igual, dándole fila. | **cerrado 2026-09-14 por TEXT-5** — la fila de §A.2 deja de estar marcada RANCIA |
| `CardDefinition.cs` | **Procedencia nueva.** El lote **no** lo editó, pero su fila de §A.2 decía «no registrada» y el fichero acaba de cambiar de significado (`displayName` = etiqueta de desarrollo). Copia del 2026-09-11 adjuntada al chat del lote y verificada por contenido. | **cerrado 2026-09-11** |
| `SongCompositionUI.cs` | **Sigue RANCIA desde R6 (2026-09-01) y ahora bloquea un paso concreto.** TEXT-5 debe cambiar la etiqueta de pista de la línea 344 (F-TXT4-3) y **esa edición no es anclable sin copia fresca**: la copia del PK no contiene los cambios de R6. Tercer lote consecutivo con la fila abierta. | **cerrado 2026-09-14 por TEXT-5** — copia fresca en el PK, rancidez de R6 saldada tras tres lotes |
| `ConceptGlossarySO.cs` · `CardEffectDescriptionBuilder.cs` · `CardDefinitionDescriptionExtensions.cs` · `ConceptGlossaryTextTable.cs` · `GameTextWindow.cs` · **`GameText.cs`** · **`GameTextTemplateIds.cs`** · **`GlossarySectionTable.cs`** · **`GameTextRichText.cs`** · **`CardTextDumpMenu.cs`** | **No entran al PK** (Capa 3). Editados o creados por el lote; se piden por lote. Los cinco en negrita son **ficheros nuevos**, ya indexados en `Repo_Tree_Index.md` (snapshot 2026-09-11 14:47): `Assets/Scripts/UI/Tooltips/GameText.cs` · `.../GameTextTemplateIds.cs` · `Assets/Scripts/Editor/Text/GlossarySectionTable.cs` · `.../GameTextRichText.cs` · `.../CardTextDumpMenu.cs`. Las cinco rutas quedan gobernadas por `SSoT_Game_Text.md` en `ssot_manifest.yaml` desde este doc-pass. | sin acción de PK |

**Assets de datos tocados (no indexables por `Repo_Tree_Index.md`):** los dos `ConceptGlossary`
(EN/ES) ganan **dos listas** — `cardNames` (60) y `templates` (47) — junto a las 36 entradas de
concepto que ya tenían. Fingerprints al cierre: Concepts
`6632813b43573214aa32fdfc5b5ae20680c24e8f` · Cards `7012d4898044ef3e67c5bd2587be50786e31815b` ·
Templates `d6ffe2128873cc3ead794136fc5879650c6ecc80`. Los CSV de import del lote son **intercambio**,
no se commitean como semilla (misma regla que TXT-3: el `.asset` ya es la verdad y la ruta de
recuperación es el export de la pestaña, a diferencia del CSV de recuperación del tutorial, D-TT-7=A).

**Ficheros a descartar del chat al cerrar TXT-4-DOC:** los 24 adjuntos de la sesión de código de
TXT-4 y todas las capturas. El paquete `TXT-4_Cierre_docdiffs_y_rehidratacion.md` **se retira al
aplicarse** (convención de paquetes); `TXT-4-DOC_Runbook.md` se retira con él.

**`Repo_Tree_Index.md` REGENERADO — deuda de siete lotes SALDADA.** Snapshot **2026-09-11 14:47**,
**472 ficheros** (desde 445 en el snapshot del 2026-08-27). Actualiza también la fila de §A.2 y la
regla §C.5. Merece registro por qué se regeneró **antes** de escribir este bloque y no después: tres
afirmaciones de este doc-pass eran inferencias hasta que existió el índice — la ruta
`Assets/Scripts/Cards/Extensions/CardDefinitionDescriptionExtensions.cs` (el paquete de cierre la
daba como corrección, sin verificar), la existencia de `GrantBonusLoopSpec.cs` (F-DOC4-1) y la
**inexistencia** de `AudienceAbilityData.cs`, que el prompt de TEXT-5 pedía por nombre y que habría
devuelto vacío. Un índice de rutas no es documentación: es lo que convierte una petición de fichero
en una operación con respuesta.

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

#### Refrescos y bajas de TXT-3 (2026-09-10)

> Bloque histórico. El bloque vivo es el de TXT-4, más abajo en esta misma sección.

Lote de código **y** documentación cerrado el mismo día. Un refresco no está hecho hasta que su fila
lo dice (§C.1).

| Fichero | Acción | Estado |
|---|---|---|
| `StatusEffectSO.cs` | **Refrescar.** EDITADO 2026-09-10 — campo `description` **eliminado**; tooltip de `displayName` reescrito como etiqueta de desarrollo; cabecera de clase con la nota de TXT-3. | **cerrado 2026-09-10** — copia fresca adjuntada al chat de TXT-3 y editada por runbook |
| `CardBase.cs` | **Refrescar.** EDITADO 2026-09-10 — `ShowTooltipInfo` resuelve keywords y estados por `ConceptTooltipResolver`; ya no referencia `SpecialKeywordData` ni `.Description`. **Gana procedencia por primera vez**: su fila de §A.2 estaba «no registrada». | **cerrado 2026-09-10** |
| `CharacterCanvas.cs` | **No editado por TXT-3** (sólo enlaza el icono; ya no compone texto). Copia fresca del repo adjuntada el 2026-09-10 ⇒ **cierra el refresco abierto por BIGNUM-1** (override de predicción presente y verificado). | **cerrado 2026-09-10** — cierra la fila de BIGNUM-1 |
| `AudienceCharacterCanvas.cs` | **No editado por TXT-3** (censado para F-TXT-2-1, no migrado). Copia fresca del repo adjuntada el 2026-09-10 ⇒ **cierra el refresco abierto por BIGNUM-1**. | **cerrado 2026-09-10** — cierra la fila de BIGNUM-1 |
| `SpecialKeywordData.cs` | **BAJA.** Fichero y asset **eliminados del proyecto** (D-TXT3-8). Retirar de la lista de `Data/*` en §B.2; no estaba en el PK. | **ejecutado 2026-09-10** |
| `ConceptTooltipResolver.cs` · `TooltipManager.cs` · `StatusIconBase.cs` · `SensoryFtPresentation.cs` · `ConceptGlossaryTextTable.cs` · `GameTextWindow.cs` · `ALWTTTProjectRegistriesSO.cs` · `ConceptGlossarySO.cs` | **No entran al PK** (Capa 3). Editados por TXT-3; se piden por lote. `ConceptTooltipResolver.cs` fue **reemplazo completo**. `ALWTTTProjectRegistriesSO.cs` **no tenía procedencia registrada**: la copia del 2026-09-10 es la primera. | sin acción de PK |
| `TutorialOverlayView.cs` · `TutorialController.cs` | **No editados por TXT-3**, pero **los edita TXT-3b**. `TutorialController.cs` sigue **RANCIA con dos lotes encima** (F-TT-5). Pedir copia fresca al abrir TXT-3b, no reutilizar la del PK. | **abierto — bloqueante para TXT-3b** |

**Assets de datos tocados (no indexables por `Repo_Tree_Index.md`):** los dos `ConceptGlossary`
(EN/ES) pasan de 17 a **36 entradas**; el asset de `SpecialKeywordData` **eliminado**; el CSV
`GameText_Concepts_TXT-3_import.csv` es intercambio de lote, **no se commitea como semilla** (a
diferencia del CSV de recuperación del tutorial, D-TT-7=A — aquí el `.asset` ya es la verdad y la
ruta de recuperación es el export de la pestaña).

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
| `MidiMusicManager.cs` | ~~Editado sólo si se aplicó la sonda `[BeatProbe]` del rider r3.~~ **CERRADO 2026-09-27 (BEAT-1):** la sonda no existe en el código, estuviera aplicada o no. | cerrado |

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
| `SSoT_Tooltip_Pipeline.md` | PERMANENTE | queda | **2026-09-25 (sustituida, CARD-FACE-3-DOC)** | Docs/systems/SSoT_Tooltip_Pipeline.md | **TIP-1-DOC / 2026-09-15** — creado y aplicado en la misma sesión que el código que documenta; ruta verificada contra `Repo_Tree_Index.md` (actualizado en el mismo paso) | autoridad nueva: **transporte** de tooltips (entradas, anclaje, colocación, ciclo de vida del panel). No gobierna texto ni significado. Tres invariantes duras en `ssot_manifest.yaml`: `TOOLTIP-SHOW-IS-A-PUSH`, `TOOLTIP-ANCHOR-SPACE-IS-INFERRED`, `TOOLTIP-REBUILD-BEFORE-MEASURE` |
| `SSoT_Game_Text.md` | PERMANENTE | queda | **2026-09-27 (sustituida, DOC-POOL-1)** | Docs/systems/SSoT_Game_Text.md | **CARD-FACE-3-DOC (2026-09-25)** — §7 (O-CF2b-2, F-CF2b-1, F-CF-7 cerradas; registro del lote; F-TV-1 observada), §9. Antes: CARD-FACE-1/2-DOC (2026-09-21) — §1 a cero filas abiertas (fila 12 nueva), §2.1 vocabulario simbólico, §5 172/73, §7, §8 checks 7–8, §9 | `governs:` +2 (`GameSymbols.cs`, `PartEffectDescriptionBuilder.cs`) |
| `AudienceCharacterBase.cs` | PERMANENTE | queda | **2026-09-27 (refrescada, CONV-TINT-1b)** | Assets/Scripts/Characters/AudienceCharacterBase.cs | **CONV-TINT-1b / 2026-09-27** — copia del repo post-1b: `RefreshSpriteTint`, `RefreshStatePresentation`, `IsBlockedPresented`. Antes: PRES-1 / D-R5-2 (2026-08-11) | ok · antes de CONV-TINT-1 la fila no tenía fecha; la copia pre-runbook resultó idéntica al repo (verificado 2026-09-27) |
| `AudienceCharacterData.cs` | PERMANENTE | queda | **2026-09-11 (refrescada, TEXT-5)** | Assets/Scripts/Data/Characters/Audience/AudienceCharacterData.cs | **TEXT-5 / 2026-09-11** — verificada por contenido en TEXT-5-DOC: la clase anidada `AudienceAbilityData` tiene `abilityId` con el tooltip que declara su contrato, y `abilityName` marcado etiqueta de desarrollo. Antes: GEW-1 (2026-09-06) y AMW-1 (2026-09-05), ambos editor-only y sin editar el fichero | ok · **contiene `AudienceAbilityData` como clase anidada**: no existe `AudienceAbilityData.cs` y pedirlo por ese nombre devuelve vacío (verificado TXT-4-DOC) |
| `CSV-4b_Name_Lookup_Audit.md` | POR-LOTE | Fase 3-B — sale al cerrar CSV-4b | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | insumo de CSV-4b (lote vivo, queued) · **no existe en el repo — solo vive aquí; comprometer antes de retirarla** |
| `CSV_Composition_Validation_Sub_Roadmap.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | sub-roadmap activo (CSV-4b/5/6) |
| `CURRENT_STATE.md` | PERMANENTE | queda | **2026-09-27 (sustituida, CONV-TINT-1-DOC + SONG-TAIL-1 D-REG)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | baseline operativo |
| `CardBase.cs` | PERMANENTE | queda | **2026-09-25 (refrescada, CARD-FACE-3-DOC-b)** | Assets/Scripts/Cards/CardBase.cs | **CARD-FACE-3 / 2026-09-22** — verificada por contenido el 2026-09-25 contra la copia anterior (diff exacto, nada fuera de lo declarado): L807–L813 comentario O-CF2b-2; L816 `OnPointerUp` sale si el botón no es Left; L817–L818 sale si `CardDetailViewController.Instance.IsVisible`. `OnPointerDown` intacto (clic derecho → `HideTooltipInfo` + `Toggle`) | `OnPointerUp` con guarda de botón y de modal (O-CF2b-2 cerrada); `OnPointerDown` sigue ocultando la pila en clic derecho |
| `GameSymbols.cs` | PERMANENTE | queda — **alta 2026-09-21 (CARD-FACE-1/2-DOC)** | **2026-09-25 (refrescada, CARD-FACE-3-DOC-b)** | Assets/Scripts/UI/Tooltips/GameSymbols.cs | **CARD-FACE-3 / 2026-09-22** — verificada por contenido el 2026-09-25: el diff contra la copia anterior es sólo la cabecera L15–L16 (nombra `GameSymbolsCoverageMenu`); fuente sigue ASCII puro (0 bytes no-ASCII). Contenido de CARD-FACE-2 sin cambios: prefijos `ct_`/`pa_`/`st_`, `AllConceptIds`, `AllSpriteNames` sin `None` (D-CF-20) | seam del vocabulario simbólico. Deriva de comentario **corregida (CARD-FACE-3)**; la copia del PK ya lo refleja |
| `PartEffectDescriptionBuilder.cs` | PERMANENTE | queda — **alta 2026-09-21 (CARD-FACE-1/2-DOC)**; estaba presente sin fila | **2026-09-21 (alta, fresca, CARD-FACE-1/2-DOC)** | Assets/Scripts/Cards/Effects/PartEffectDescriptionBuilder.cs | **CARD-FACE-2 / 2026-09-21** — `TonalityEffect` mapeado (D-CF-17=D), `FamilySpriteName` / `FamilyConceptId` / `ValueOnly` | formateador único de líneas de modificador (cara + modal) |
| `AudienceCharacterCanvas.cs` | PERMANENTE | queda — alta 2026-09-11 (F-DOC4-2) | **2026-09-27 (refrescada, CONV-TINT-1b)** | Assets/Scripts/Characters/AudienceCharacterCanvas.cs | **CONV-TINT-1b / 2026-09-27** — copia del repo post-1b: `SetConvincedHidden` + puertas de tooltip. La copia previa (2026-09-11) **ya coincidía** con el repo el 2026-09-27: la marca RANCIA de TEXT-VIBE-1 era falsa (F-CT-9) | ok · contradicción retirado-pero-presente resuelta en TXT-4 (tercera ocurrencia de la familia) |
| `GameText.cs` | PERMANENTE | queda — **alta 2026-09-14 (TEXT-5)** | **2026-09-11 (fresca, TEXT-5)** | Assets/Scripts/UI/Tooltips/GameText.cs | **TEXT-5 / 2026-09-11** — verificada por contenido en TEXT-5-DOC: `CardName`, `ConceptName`, `ConceptDescription`, `StatusName`, `Format`, y **`Number(double, string)`** formateando por `Glossary.NumberFormat` con invariante explícita | **sube de Capa 3 a núcleo.** TXT-4 lo creó y lo censó como Capa 3; TEXT-5 lo promueve porque es la **entrada única** de todo el texto compuesto (la otra, `ConceptTooltipResolver`, solo responde hovers) y ningún lote de texto puede razonar sin él. Gobernado por `SSoT_Game_Text.md` vía `Assets/Scripts/UI/Tooltips` |
| `SongCompositionUI.cs` | PERMANENTE | queda — deja de estar RANCIA | **2026-09-21 (refrescada, CARD-FACE-1/2-DOC)** | Assets/Scripts/UI/Song Composition/SongCompositionUI.cs | **CARD-FACE-2 / 2026-09-21** — verificada por contenido: marcas `[CARD-FACE-2]` en L379 (etiqueta de parte por letra, `customLabel` borrado) y L396 (`ui_part_bridge`, D-CF-21) | cierra la deuda RANCIA abierta por TEXT-VIBE-1 |
| `CardDefinition.cs` | PERMANENTE | queda | **2026-09-11 (procedencia nueva, TXT-4-DOC)** | Assets/Scripts/Cards/CardDefinition.cs | **TXT-4 / 2026-09-11** — copia fresca adjuntada al chat del lote y verificada por contenido; `resourceCostStatusKey`/`Amount` (R5-d) siguen presentes. **El lote NO editó el fichero**, pero cambió lo que significa uno de sus campos | **`displayName` es etiqueta de desarrollo desde TXT-4** (D-TXT4-4), mismo contrato que `StatusEffectSO.displayName` desde TXT-3. Ninguna superficie de jugador debe leerlo; tres lo siguen haciendo (F-TXT4-3 → TEXT-5). La fila gana procedencia precisamente porque **un campo que cambia de contrato sin cambiar de forma es lo que una copia rancia no delata** |
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
| `Design_Vibe_Telegraph_v0_1.md` | PERMANENTE | queda (alta BIGNUM-1-DOC 2026-09-09) | **2026-09-27 (sustituida, DOC-POOL-1)** | Docs/planning/Design_Vibe_Telegraph_v0_1.md | BIGNUM-1-DOC (2026-09-09): contiene §10 (§10.1–§10.7) | hogar de presentación C1/C2/C3; insumo de GIG23-A y TIP-1. Sigue clasificado `planning` en `SSoT_INDEX` — la matemática vive en `SSoT_Scoring_and_Meters` §6. Reingresa desde §B.5 |
| `VibeProjection.cs` | PERMANENTE | queda (alta BIGNUM-1-DOC 2026-09-09) | 2026-09-09 | Assets/Scripts/Characters/VibeProjection.cs | BIGNUM-1 (2026-09-09): fichero nuevo del lote | contrato de presentación del Vibe por miembro; seam pequeño y estable para lotes de UI de gig |
| `Documentation_Update_Loop_Local_Addendum_v0_4.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | gobernanza operativa (addendum local) |
| `GigFlowSettingsSO.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Data/Gig/GigFlowSettingsSO.cs | R5-d (2026-08-26): MaxBonusLoopsPerPart presente | ok · sin lote posterior conocido |
| `GigRunContext.cs` | PERMANENTE | queda | no registrada | Assets/Scripts/Managers/GigRunContext.cs | sin marcas de lote reconocibles | sin lote posterior conocido |
| `HandController.cs` | PERMANENTE | queda | **2026-09-25 (refrescada, PAUSE-1)** — antes 2026-09-03 (TUT-REDESIGN-B) | Assets/Scripts/Controllers/HandController.cs | **PAUSE-1 / 2026-09-25** — verificada por contenido en DOC-POOL-1: `PauseGate.IsPaused` en `Update` (L246) y `ReleaseHeldCardToHand` (L271). Antes: TUT-REDESIGN-B / 2026-09-03 — cuatro puertas de acción por `ReportPlayDenied`; `CardPerformedEvent` (WINK-1); puerta 2b.5 (R5-g) | **Doble corrección de fila (R5-g).** La fila anterior decía `CARD-UX-1 (2026-07-13)` mientras la copia del PK ya contenía marcadores **R5-d**: el índice iba dos meses por detrás de su propio fichero, variante benigna del incidente de §A.1 y misma familia de fallo. Verificado además en R5-g que la copia del PK y la del repo eran **byte-idénticas salvo BOM** antes del lote (R5-f no tocó el fichero) |
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
| `CharacterCanvas.cs` | POR-LOTE | Fase 3-B — retirar en el próximo corte (alta 2026-09-04, D1=A) | **verificada 2026-09-27 (CONV-TINT-1b, idéntica al repo)** | Assets/Scripts/Characters/CharacterCanvas.cs | **CONV-TINT-1b / 2026-09-27** — comparada con la copia del repo: idéntica salvo BOM/CRLF; contiene el override de predicción de BIGNUM-1. La marca RANCIA de BIGNUM-1 era falsa (F-CT-9; ya la había cerrado TXT-3 el 2026-09-10 en §A.1 sin voltear esta fila) | contradicción retirado-pero-presente resuelta 2026-09-04 |
| `PinkTrombone_Voice_Levers.md` | POR-LOTE | Fase 3-B — sale al cerrar R6 | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | schema VoiceProfileSO; R6 (SingerVoiceDirector) |
| `Rehydration_Prompt_Guide.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | operativa de handoff entre chats |
| `Roadmap_ALWTTT.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | roadmap activo |
| `Roadmap_Audio.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | roadmap activo |
| `RosterExpansion_Sub_Roadmap.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | sub-roadmap activo (R5-d..R8) |
| `S5_DemoCutClose_Sub_Roadmap.md` | PERMANENTE | queda | **2026-09-27 (sustituida, SONG-TAIL-1 D-REG)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | sub-roadmap activo (S5i/S5j = frente vivo) |
| `SSoT_ALWTTT_MidiGenPlay_Boundary.md` | PERMANENTE | queda | **2026-09-27 (sustituida, DOC-POOL-1)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva (frontera) |
| `SSoT_Audience_and_Reactions.md` | PERMANENTE | queda | **2026-09-27 (sustituida, CONV-TINT-1-DOC)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Audio.md` | PERMANENTE | queda | **2026-09-27 (sustituida, DOC-POOL-1)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_CONTRACTS.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | contracts |
| `SSoT_Card_Authoring_Contracts.md` | PERMANENTE | queda | **2026-09-27 (sustituida, CONV-TINT-1-DOC)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Card_System.md` | PERMANENTE | queda | **2026-09-27 (sustituida, DOC-POOL-1)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Dev_Mode.md` | PERMANENTE | queda | **2026-09-27 (sustituida, CONV-TINT-1-DOC)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Editor_Authoring_Tools.md` | PERMANENTE | queda | **2026-09-25 (sustituida, CARD-FACE-3-DOC-b)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Gig_Combat_Core.md` | PERMANENTE | queda | **2026-09-27 (sustituida, CONV-TINT-1-DOC)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Gig_Encounter.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_INDEX.md` | PERMANENTE | queda | **2026-09-21 (sustituida, CARD-FACE-1/2-DOC)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | índice de autoridad |
| `SSoT_Runtime_CompositionSession_Integration.md` | PERMANENTE | queda | **2026-09-27 (sustituida, DOC-POOL-1)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Runtime_Flow.md` | PERMANENTE | queda | **2026-09-27 (sustituida, DOC-POOL-1)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Scoring_and_Meters.md` | PERMANENTE | queda | no registrada | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Singer_Voice.md` | PERMANENTE | queda | **2026-09-27 (sustituida, DOC-POOL-1)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SSoT_Status_Effects.md` | PERMANENTE | queda | **2026-09-27 (sustituida, CONV-TINT-1-DOC)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | SSoT viva |
| `SongConfigBuilder.cs` | PERMANENTE | queda | no registrada · **verificada byte-idéntica al repo 2026-09-01 (R6, `diff` contra copia fresca; R6 NO la editó)** | Assets/Scripts/Music/SongConfigBuilder.cs | BAL-1 (2026-07-22): mixGains; DBG-C1: MusicianTrackKey; **R6 2026-09-01: sin cambios** | lotes posteriores posibles: R5-d render-scope solo (§8 inv 14) — no verificable en la copia |
| `StatusEffectCatalogueSO.cs` | PERMANENTE | queda | **2026-09-21 (refrescada, CARD-FACE-1/2-DOC)** | Assets/Scripts/Status/StatusEffectCatalogueSO.cs | **CARD-FACE-2 (A13-FIX) / 2026-09-21** — verificada por contenido: guarda `IconSprite` ↔ `st_<key>` dentro de `ValidateCatalogueContents` (L250 ss.), un solo `OnValidate` | antes «sin marca posterior» |
| `StatusEffectContainer.cs` | **Refrescar.** El publisher de `StatusAppliedEvent` pasa `effect` como cuarto argumento. | **abierto** |
| `StatusEffectSO.cs` | PERMANENTE | queda | **2026-09-05 (refrescada, TXT-1)** | Assets/Scripts/Status/StatusEffectSO.cs | **WINK-1 / verificada 2026-09-05** — `applySfx` + `ApplySfx` presentes; también `description` `[TextArea]` y el auto-rename `StatusEffect_{DisplayName}_{EffectId}` de `OnValidate` | refresco de WINK-1 **cerrado**. El auto-rename es la razón de que la pestaña de status de `GameTextWindow` sea de sólo lectura (P-TXT-2, `CURRENT_STATE` §4) |
| `changelog-ssot.md` | PERMANENTE | queda | **2026-09-27 (sustituida, CONV-TINT-1-DOC + SONG-TAIL-1 D-REG)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | historia semántica |
| `coverage-matrix.md` | PERMANENTE | queda | **2026-09-27 (sustituida, CONV-TINT-1-DOC)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | lookup de autoridad |
| `ssot_manifest.yaml` | PERMANENTE | queda | **2026-09-27 (sustituida, CONV-TINT-1-DOC)** | (ver SSoT_INDEX / manifiesto; no verificada contra árbol en esta sesión) | contenido fechado por sus propias entradas (último lote citado: DOC-APPLY-3 2026-08-26 en los docs de gobernanza) | manifiesto (governs:) |
| `PK_Manifest.md` | PERMANENTE (Capa 2) | queda | **2026-09-27 (v18.6, SONG-TAIL-1 apertura)** | `Docs/PK_Manifest.md` | PK-CUT-1 / 2026-08-27 | este fichero; no gobernado — operativa del PK |
| `MGP_Boundary_Index.md` | PERMANENTE (Capa 2) | queda | 2026-08-26 | `Docs/MGP_Boundary_Index.md` | PK-AUDIT-1 / 2026-08-26 | índice del espejo MGP-20260810; no gobernado |
| `Repo_Tree_Index.md` | PERMANENTE (Capa 2) | queda | **snapshot 2026-09-25 19:17 (regenerado tras PAUSE-1)** — corregido en DOC-POOL-1; antes decía 13:04 | `Docs/Repo_Tree_Index.md` | generado por `make-tree-unity.ps1` | 480 → **482** rutas; `Assets/Scripts/Editor` 12 → 14. **DOC-POOL-1:** el snapshot 19:17 trae **488** ficheros, con `Assets/Scripts/UI/Pause/` y las cinco rutas de PAUSE-1. Contiene `GameSymbolsSyncMenu.cs` (CARD-FACE-3) y `GameSymbolsTableMenu.cs` (no declarado por ningún lote; F-CF3DOC-1, documentado en §20.14 el 2026-09-25); **no** contiene `SymbolSheetSlicer.cs` (nunca existió) |
| `RhythmLane.cs` | POR-LOTE | lote **RFX-1** activo — sale al cerrar RFX-1/RFX-2 | adjuntada por el lote (sin fecha registrada) | Assets/Scripts/Enums/RhythmLane.cs | RFX-1 / 2026-08-26 (marca `RFX-1` en el fichero) | enum de carril rítmico; 1 KB |
| `RhythmFxConfigSO.cs` | POR-LOTE | lote **RFX-1** activo — sale al cerrar RFX-1/RFX-2 | adjuntada por el lote (sin fecha registrada) | Assets/Scripts/Data/RhythmFxConfigSO.cs | RFX-1 / 2026-08-26 (marca `RFX-1` en el fichero) | config SO del FX rítmico; 6 KB |
| `RhythmParticleEmitter.cs` | POR-LOTE | lote **RFX-1** activo — sale al cerrar RFX-1/RFX-2 | adjuntada por el lote (sin fecha registrada) | Assets/Scripts/Music/RhythmParticleEmitter.cs | RFX-1 / 2026-08-26 (marca `RFX-1` en el fichero) | emisor de partículas; 9 KB |
| `RhythmParticleMidiListener.cs` | POR-LOTE | lote **RFX-1** activo — sale al cerrar RFX-1/RFX-2 | adjuntada por el lote (sin fecha registrada) | Assets/Scripts/Music/RhythmParticleMidiListener.cs | RFX-1 / 2026-08-26 (marca `RFX-1` en el fichero) | puente evento MIDI → emisor; 12 KB |
| `RhythmFxTester.cs` | POR-LOTE | lote **RFX-1** activo — sale al cerrar RFX-1/RFX-2 | adjuntada por el lote (sin fecha registrada) | Assets/Scripts/Music/RhythmFxTester.cs | RFX-1 / 2026-08-26 (marca `RFX-1` en el fichero) | banco de pruebas del FX; 10 KB |
| `PENDING_DOC_DIFFS_RFX-1.md` | POR-LOTE (paquete retenido) | lote **RFX-1** activo | 2026-08-26 | `Docs/pending/PENDING_DOC_DIFFS_RFX-1.md`† | RFX-1 sesión 1/2, cerrada 2026-08-26 | **D-DOC-5 cumplida**: la cabecera nombra consumidor (`RFX-1-DOC` o la fase doc de RFX-2) y declara la pérdida en ese cierre si ninguno abre · **no existe en el repo — solo vive aquí; comprometer a `Docs/pending/` antes de retirarlo (D-DOC-5)** |

---

## §B — Retirados del PK

Retirar del PK **no** es archivar, degradar ni borrar (D-PK-0 = A). Todo lo listado aquí conserva su clase de autoridad en el repo y se puede pedir para el chat de un lote.

Fecha de retirada: **EJECUTADO 2026-08-26/27** (PK-CUT-1, Fases 1 y 2 completas). La **Fase 3-A no se ha ejecutado**: sus 16 ficheros siguen en §A marcados «retirar (Paso 7a)».

### B.1 — Fase 1 (riesgo cero): paquetes consumidos, duplicados, históricos absorbidos

Cómo pedirlos: **no se piden** (paquetes consumidos / duplicados); los históricos se adjuntan al chat solo para re-litigar algo cerrado.

| Fichero | KB | Clase | Motivo / dónde vive el contenido ahora |
|---|---:|---|---|
| `BEAT-1_Doc_Package.md` | 25 | REDUNDANTE | paquete **consumido en DOC-POOL-1 (2026-09-27)**: Parte A aplicada en la misma sesión (F-B1-15), Parte B B1–B11 completa; contenido en `SSoT_Runtime_CompositionSession_Integration` §3.4.1, `Design_Vibe_Telegraph` §10.5, `SSoT_Dev_Mode` §18/§19.2, `SSoT_Game_Text`, `CURRENT_STATE` §1/§2/§4; ver changelog 2026-09-27 |
| `PAUSE-1_DocDiffs_Pending.md` | 36 | REDUNDANTE | paquete **consumido en DOC-POOL-1 (2026-09-27)**: Parte A (34–35) y Parte B (1–33) aplicadas; **pasos 15 y 24 descartados**, absorbidos por BEAT-1 B4/B7 (D10=B). Contenido en `SSoT_Audio` §4.8–§4.9, `SSoT_Runtime_Flow` «Pause state», `SSoT_Card_System`, boundary §4.1; ver changelog 2026-09-25 |
| `PENDING_DOC_PACKAGE_CARD-FACE-1.md` | 12 | REDUNDANTE | paquete **consumido en CARD-FACE-1/2-DOC (2026-09-21)**, corregido antes con §D3 del paquete 2; contenido en `SSoT_Game_Text` §1/§2.1/§5/§7, `SSoT_Card_System` §10, `SSoT_Editor_Authoring_Tools` §20.14 |
| `PENDING_DOC_PACKAGE_CARD-FACE-2.md` | 12 | REDUNDANTE | paquete **consumido en CARD-FACE-1/2-DOC (2026-09-21)**, D1..D8; ver changelog 2026-09-21 |
| `Roadmap_Update_CARD-FACE-1_close.md` | 5 | SUPERADO | paso R1 **nunca aplicado**; sustituido por la edición consolidada de `S5_DemoCutClose_Sub_Roadmap.md` (CARD-FACE-1 y 2 cerradas, CARD-FACE-3 propuesta) |
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

**`Characters`** (8 ficheros · 56 KB) — *`AudienceCharacterCanvas.cs`: retirada nunca ejecutada; presente y censado en §A.2 desde 2026-09-11 (F-DOC4-2)* — *`CharacterCanvas.cs`: retirada nunca ejecutada; presente y censado en §A.2 desde 2026-09-04*: `AudienceCharacterCanvas.cs`, `CharacterAnimator.cs`, `CharacterBase.cs`, `CharacterCanvas.cs`, `CharacterStats.cs`, `SpriteOutlineController.cs`, `StatusStats.cs`, `VibeEffectiveness.cs`, `VibeProjection.cs` *(alta BIGNUM-1 2026-09-09; pasa a §A.2)*

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

### B.6 — BEAT-1, retirado el 2026-09-27

| Fichero | Ruta en repo | Cómo pedirlo |
|---|---|---|
| `MidiEventInterfaces.cs` | `Assets/Scripts/Music/MidiEventInterfaces.cs` | por FRP, sólo chat, cuando un lote toque los contratos de oyentes MIDI (`IBeatGridListener`, `ITempoSignatureListener`, …). Copia del repo verificada el 2026-09-25 (BEAT-1): igual a la del PK salvo un carácter mal codificado en un comentario (l. 70) |

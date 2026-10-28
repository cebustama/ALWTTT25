# MAP-DEMO-1 — montaje en Unity y guion de partida

**Consumido en el cierre de MAP-DEMO-1 (2026-10-28).** Queda como registro del montaje y del guion.
**Paquete transitorio, no es documento gobernado.** Lo consume el cierre de MAP-DEMO-1. Si ese lote
cierra sin consumirlo, se declara perdido en la misma clausura (D-DOC-5).
**Generado:** 2026-10-27. Código aplicado sin compilar (Claude Code no ejecuta Unity).

## Decisiones (Claudio, 2026-10-27)

- **D-MAP-DEMO-1 = A** — `RunConfig.preserveRunState`: a partir del nodo 2, `ApplyRunConfig` no
  reconstruye el mazo ni borra los SFX desbloqueados.
- **D-MAP-DEMO-2 = A** — perder un gig resta cohesión y avanza al nodo siguiente (el nodo se consume al
  lanzarse).
- **D-MAP-DEMO-3 = A** — `SceneData.sectorMapSceneIndex` apunta a la escena nueva; `SceneChanger` no
  cambia y la escena procedural sigue en la build.
- **D-MAP-DEMO-4 = A** — cada nodo apunta a un `DemoLaunchConfigSO`.

## Código tocado

| Archivo | Cambio |
| --- | --- |
| `Assets/Scripts/Map/LinearMapController.cs` | **Nuevo.** Nodos, nave, pulso del nodo activo, Game Over por cohesión 0 o por perder la Reina (provisional hasta COH-1), lanzamiento con `GigLauncher`. |
| `Assets/Scripts/Managers/GigRunContext.cs` | `RunConfig.preserveRunState`. |
| `Assets/Scripts/Data/Core/PersistentGameplayData.cs` | `LinearMapNextNode`, `TutorialOptInAnswered` (se reinician con la partida); rama `preserveRunState` en `ApplyRunConfig`. |
| `Assets/Scripts/Managers/GigManager.cs` | La pregunta del tutorial sale una vez por partida, no en cada gig ni en Retry. |
| `Assets/Scripts/Data/Gig/GigDevSettingsSO.cs` | Interruptor `StartOnLinearMap` (prevalece sobre `AutoStartFromDefaults`). |
| `Assets/Scripts/UI/MainMenuController.cs` | Con el interruptor activo, Start reinicia la partida (`InitGameplayData` + `SetInitialDeck`) y abre el mapa. |
| `Assets/Scripts/Map/SectorNodeVisual.cs` | `UseBoundTooltipOnly`: el tooltip no busca el encuentro por índice de sector. |

## Montaje en Unity (Claudio)

1. Esperar a que compile. Si hay errores, pegar la consola.
2. `Assets/Art/mapa asterioide 1.0.png`: Texture Type **Sprite (2D and UI)**, **Max Size 8192**,
   compresión Normal. Hacer lo mismo con `tocatas A.City.png` (no se usa aún). Commitear los PNG con
   sus `.meta`.
3. Duplicar `Assets/Scenes/1 - Sector Map.unity` → `Assets/Scenes/1b - Linear Map.unity` (conserva
   cámara con `Physics2DRaycaster`, EventSystem, `SceneChanger` y el HUD). En la copia, **borrar** los
   objetos de `SectorMapManager` y `SectorMapVisual` (y sus canvases de eventos/conflictos si los hay).
4. Añadir un objeto `MapArt` con `SpriteRenderer` y el PNG del mapa; ajustar el tamaño ortográfico de
   la cámara para que se vea entero (D-MAP-3).
5. Arrastrar 3 instancias de `Assets/Prefabs/Map/NodePrefab.prefab` sobre el arte, en orden de juego,
   y un objeto vacío `StartAnchor` donde espera la nave al empezar.
6. Objeto vacío `LinearMap` + `LinearMapController`:
   - **Nodes:** los 3 nodos; `launchConfig` = `Assets/Settings/Gig/DemoLaunchConfig.asset` en los tres
     (la Reina tendrá el suyo en su batch); títulos provisionales «Gig 1», «Gig 2», «La Reina Gusano».
   - **Ship Prefab:** `Assets/Prefabs/Map/ShipPrefab.prefab`. **Start Anchor:** el objeto del paso 5.
   - **Setup Roster / Flow Settings:** los mismos assets que tiene `MainMenuController` en la escena 0.
   - **Scene Changer:** el de la escena. **Cohesion Text:** el texto de cohesión del HUD que ya existe.
   - Ajustar **Node Scale** si los nodos se ven pequeños sobre el arte.
7. Build Settings: añadir la escena y anotar su índice. En
   `Assets/Resources/Data/Settings/Scene Data.asset`, `sectorMapSceneIndex` = ese índice (antes: 1).
8. En el asset `GigDevSettings`, activar **Start On Linear Map**.

## Guion de partida (jugarlo una vez y pasar el `Editor.log`)

Valores de partida: cohesión inicial 10; el encuentro de la demo (`GigEncounter 1`, «Cool Dude & The
Kids») resta 2 al perder.

| # | Jugada | Línea de log esperada |
| --- | --- | --- |
| 1 | Menú → Start | `StartOnLinearMap → fresh run, opening map scene.` |
| 2 | — (llega el mapa) | `[LinearMap] Start \| NextNode=0/3 \| Cohesion=10` |
| 3 | Clic en el nodo 1 | `[LinearMap] Launch node 0 'Gig 1' \| … \| Boss=False \| PreserveRunState=False` y después `SetBandDeckFromMusicians ->` |
| 4 | Responder la pregunta del tutorial y **ganar** el gig; elegir una carta en la recompensa (anotar cuál) | `[GigManager] ReturnToMap \| … ReturnDest=Map` |
| 5 | — (vuelve al mapa) | `[LinearMap] Start \| NextNode=1/3 \| Cohesion=10` |
| 6 | Clic en el nodo 2 | `Launch node 1 … PreserveRunState=True` y `ApplyRunConfig: preserveRunState=true → deck and SFX unlocks carried over.` **No** debe salir la pregunta del tutorial. |
| 7 | Comprobar que la carta del paso 4 está en el mazo y **perder** (F12 → forzar derrota vale) | `ReturnToMap` con `ReturnDest=Map` |
| 8 | — (vuelve al mapa) | `[LinearMap] Start \| NextNode=2/3 \| Cohesion=8` |
| 9 | Clic en el nodo 1 o 2 (ya jugados) | `Click on node … ignored (active=2).` |
| 10 | Clic en la Reina y ganar | `Boss=True`; termina en recompensa → WinPanel. |

## Smokes humanos (≤3)

1. **El mapa se lee:** al llegar, sin explicación, se entiende cuál es el siguiente destino (nodo que
   late) y que la Reina está al final. Falla si hay que adivinar dónde hacer clic.
2. **El progreso se ve:** tras el gig 1, el nodo 1 aparece apagado, la nave está en él y el nodo 2 late.
   Falla si el mapa se ve igual que al principio.
3. **El recorrido se entiende entero:** Start → mapa → gig → mapa → … → Reina se siente como un viaje,
   no como pantallas sueltas. Juicio de Claudio; anotar qué falta (transición, texto, sonido).

## Fuera de alcance (anotado)

- Animación de la nave entre nodos y polinización del mapa (semana del 2026-11-23).
- Un escenario por nodo (`VenueType`) y la Reina con su encuentro propio (semanas del 2026-11-09/16).
- Panel de derrota, Retry y la derrota a mitad de gig por cohesión 0 (COH-1). Hoy, perder contra la
  Reina lleva a Game Over desde el mapa (provisional).
- `GigManager.HandleExit` vuelve al menú sin reiniciar la partida; Start lo compensa en el modo mapa.

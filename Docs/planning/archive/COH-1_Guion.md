# COH-1 (+ GIG-FRAME) — Guion de partida (paquete transitorio)

> Ampliado el 2026-10-29: la integración de la nueva UI del gig (la consola del manager) entra en este
> batch (montaje B). Lo de código es mínimo (pips de cohesión en `GigCanvas`); casi todo es trabajo de escena y prefab.

> Lo consume: el cierre de COH-1. Si COH-1 cierra sin jugarlo, se declara perdido en ese cierre.
> Generado: 2026-10-28. Compilación y smokes **sin verificar** (Claude Code no ejecuta Unity).

## Decisiones (Claudio, 2026-10-28)

- **D1 = en el gig:** perder contra la Reina en el mapa → panel «La Reina no se convenció» → «Fin de la gira» → Game Over.
- **D2 = sin Retry:** tras la Reina, el WinPanel solo tiene «Volver al menú».
- **D3 = HUD + X → Y:** «Cohesión: N» en el GigCanvas; el panel de derrota muestra «Cohesión X → Y».
- **D4 = por destino de vuelta:** modo demo = el gig vuelve al mapa (`returnDestination == Map`). Desde GigSetup siguen Retry y Exit.

## Montaje (una vez, en el editor)

### A. Paneles y test (COH-1)

1. Comprueba que el **Exit** del panel de derrota tiene un texto TMP hijo (el código le cambia el rótulo a «Continuar» o «Fin de la gira»). Lo mismo para el Exit del WinPanel («Volver al menú»).
2. Test Runner → EditMode → `GigLossOutcomeTests` (6 tests). Es el primer test en `Assets/Tests/Editor/`, sin asmdef (D-DIAG-3). Si NUnit no resuelve en `Assembly-CSharp-Editor`, avísame: hay que añadir un asmdef de tests.

### B. La consola del manager → `GIG-UI-1_Runbook.md`

Desde el 2026-10-29, el armado de la nueva UI (marco, HUD sobre la consola, recortes y tooltips nuevos) es
un batch propio: **`Docs/planning/active/GIG-UI-1_Runbook.md`**. COH-1 conserva solo lo suyo: los paneles
(montaje A), el guion y los smokes 1 y 2. El smoke 3 de abajo («la consola se lee») pasa a GIG-UI-1.

## Guion (una partida, con Dev Mode)

Datos: cohesión inicial 10; los 3 nodos usan `GigEncounter 1` (penalización por derrota 2).

| # | Jugada | Se ve | Línea de log esperada |
| --- | --- | --- | --- |
| 1 | Menú → Start | Mapa, «Cohesión: 10» | `[LinearMap] Start \| NextNode=0/3 \| Cohesion=10` |
| 2 | Clic en nodo 1; ya en el gig | HUD «Cohesión: 10» | — |
| 3 | Pierde el gig (Dev: lose normal o deja que acabe sin convencer) | Panel «Gig perdido» · «…pero la gira sigue. Cohesión 10 → 8.» · **un solo botón «Continuar»**, sin Retry | `[COH-1] LoseGig \| MapRun=True \| Final=False \| Cohesion 10 → 8 \| RunOver=False` |
| 4 | «Continuar» | Mapa, «Cohesión: 8», nodo 2 activo | `[LinearMap] Start \| NextNode=1/3 \| Cohesion=8` |
| 5 | Nodo 2; Dev Stats: baja la cohesión con el «−» hasta 0 | El HUD baja con cada clic; al llegar a 0: panel «La banda se separa» · «Fin de la gira» | `[COH-1] LoseGig \| MapRun=True \| Final=False \| Cohesion 0 → 0 \| RunOver=True` |
| 6 | «Fin de la gira» | Escena Game Over | `[COH-1] Game Over \| Cohesion=0` |
| 7 | Game Over → menú → Start; gana nodos 1 y 2 (Dev: win normal); en la Reina, pierde | Panel «La Reina no se convenció» · «La gira termina aquí.» (sin línea de cohesión, 2026-10-30) · «Fin de la gira» → Game Over | `[COH-1] LoseGig \| MapRun=True \| Final=True \| … \| RunOver=True` y luego `[COH-1] Game Over` |
| 8 | Menú → Start; gana los tres (Dev: win normal) | Recompensa y luego WinPanel **sin Retry**, botón «Volver al menú» → menú | `HandleExit → returning to main menu` |
| 9 | Regresión: menú → GigSetup → lanza un gig y piérdelo | Panel con **Retry y Exit** y su texto de siempre; Retry recarga el gig | `[COH-1] LoseGig \| MapRun=False \| …` |

Pégame el `Editor.log` después de jugar y lo comparo con esta tabla.

## Smokes humanos (≤3)

1. **Se entiende la derrota.** Setup: paso 3. Acción: lee el panel. Se espera que, sin explicación, se entienda que la gira sigue y cuánto bajó la cohesión. Falla si el texto confunde o el botón no se lee.
2. **Breakdown real a mitad de gig.** Setup: nodo 1 con la cohesión a 1 (Dev Stats), juega hasta que un músico se derrumbe por estrés. Se espera ver «La banda se separa» y que no se puedan jugar cartas detrás del panel. Falla si sale «Gig perdido», si se puede seguir jugando o si el panel aparece dos veces.
3. ~~La consola se lee~~ → movido a `GIG-UI-1_Runbook.md`, smoke 1.

## Cambios de código

| Archivo | Cambio |
| --- | --- |
| `Assets/Scripts/Managers/GigLossOutcome.cs` (nuevo) | Función pura: de cohesión, penalización, modo mapa y jefe → si termina la partida, cohesión después, textos y rótulo del botón |
| `Assets/Scripts/Managers/GigManager.cs` | `LoseGig()` usa `GigLossOutcome`; en el mapa hay un solo botón (Continuar → `ReturnToMap(false)` / Fin de la gira → `GoToGameOver`); guarda `_lossShown`; pasa a `EndGig`; `IsMapRun`; `GoToGameOver`; WinPanel del jefe sin Retry en el mapa |
| `Assets/Art/UI/Gig/GigFrame_placeholder.png` (nuevo) | Recorte del marco sin margen negro (GIG-FRAME); Unity crea su `.meta` |
| `Assets/Scripts/UI/GigCanvas.cs` | `cohesionTextField` + pips de cohesión (`cohesionPipRoot`, `cohesionPipPrefab`, sprites lleno/vacío; opcionales) + `SetCohesion`; `ConfigureLossButtons` y `ConfigureWinButtons` (ocultan Retry y cambian el rótulo de Exit; fuera del mapa restauran el del prefab) |
| `Assets/Scripts/Data/Core/PersistentGameplayData.cs` | Evento estático `BandCohesionChanged` en el setter |
| `Assets/Scripts/Characters/MusicianBase.cs`, `Assets/Scripts/Map/LinearMapController.cs` | Solo comentarios |
| `Assets/Tests/Editor/GigLossOutcomeTests.cs` (nuevo) | 6 tests EditMode |

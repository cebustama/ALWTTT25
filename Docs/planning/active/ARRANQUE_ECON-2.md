# Arranque ECON-2 · la Inspiración (como el Heroísmo de Midnight Suns)

Escrito el 2026-11-14 al cerrar S9c. Planning, no autoridad. Fila 4 del «Orden de entrada» de `DEMO-1211_Plan_de_Sesiones.md` (después de FEEDBACK-2). **Prioritaria** e imprescindible.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. DEMO-1211 ECON-2 — la Inspiración.

Lee Docs/planning/active/ARRANQUE_ECON-2.md y haz ECON-2. Empieza con
/buscar-referencias --all (Heroísmo de Midnight Suns, cartas que cargan y cartas que rematan,
último turno como clímax) y preséntame las decisiones en formato D antes de construir.

Contexto: S9c (DEMO-FIX-1) cerrada el 2026-11-14; FEEDBACK-2 cerrada antes de esta.
Decidido el 12-nov: D-PLAN-20 (las dos formas según la carta) y D-PLAN-21 (arte en
CARTAS-ARTE-1, la sesión siguiente).

Restricciones: logs con la etiqueta [ECON-2]; no escribir .meta; no editar MidiGenPlay;
sin commit ni push sin pedirlo; ≤3 smokes jugados por Claudio; pasos de editor uno a uno;
previsto = real en el Big Num.

Cierre: smokes jugados, docs propuestos y aplicados con OK, proyectos.md, foco.md, página
de frentes y el arranque de CARTAS-ARTE-1.
```

## La idea (diseño en `Docs/reference/ideas/2026-11-12_inspiracion_heroismo.md`)

- Toda carta de **composición** genera ★N al jugarla (`inspirationGenerated`, hoy 0 en todo el contenido).
- Toda carta **Finisher** gasta la Inspiración y **solo se juega en el último loop**: el último loop pasa de turno muerto a clímax.
- Algunas cartas dan Inspiración **por loop** (`AddInspirationPerLoopSpec`): fuera de la demo o recompensa del gig 2.

## Lista

1. **Datos:** `complexityMultiplier` a 0 en la configuración de `LoopScoreCalculator`. Si no, `inspirationGenerated` cambia el Big Num y se rompe previsto = real.
2. **Datos:** ★1–★2 en las cartas de composición de los cuatro mazos iniciales (valores de partida; se ajustan después con F5).
3. **Código, la única pieza nueva:** marca **Finisher** en `CardDefinition` que invierte `FinalLoopLock`: el Finisher solo se puede jugar mientras suena el último loop, y las demás composiciones siguen bloqueadas en él.
4. **Contenido:** 1 o 2 Finisher para la demo (¿uno por músico o uno genérico?) con coste en Inspiración.
5. **Editor y JSON de cartas:** valores por defecto al crear una carta (Composición ★1, Finisher con coste ★N).
6. **Legibilidad mínima:** la ★ visible en la cara (`inspirationGenTextField` ya existe, oculto con 0). El marco nuevo es de CARTAS-ARTE-1.

## Decisiones previsibles (formato D)

- **D1 · Cuánto da cada composición:** todas ★1 / según el instrumento (Voz ★1, Batería ★2, como el arte de Matías) / según la carta.
- **D2 · Qué hace un Finisher:** subir mucho el LoopScore del último loop / un efecto sobre el público / según la carta.
- **D3 · Cuántos Finisher en la demo** y en qué mazos.
- **D4 · Qué pasa con la Inspiración sobrante** al acabar la canción: se pierde / se guarda para la siguiente.

## Entradas

- `Docs/reference/ideas/2026-11-12_inspiracion_heroismo.md` (diseño, convención y arte de Matías).
- `Docs/planning/active/DEMO-1211_Plan_de_Sesiones.md`: D-PLAN-14, D-PLAN-20 y D-PLAN-21.
- `Docs/systems/SSoT_Card_System.md` y el SSoT del LoopScore y la Inspiración (búscalo en `SSoT_INDEX.md`).
- Código: `CardDefinition`, `CardBase.GenerateInspiration`, `LoopScoreCalculator`, `FinalLoopLock`, `AddInspirationPerLoopSpec`, `GigFlowSettings.defaultInspirationPerLoop`.
- Maquetas: `Docs/reference/ideas/img/2026-11-12_cartas_maqueta_marcos.png` y `_mano.png`.

## Smokes (≤3, jugados por Claudio)

1. Jugar composiciones: la Inspiración sube ★N por carta y el Big Num previsto sigue siendo igual al real.
2. Último loop: las composiciones normales siguen bloqueadas y el Finisher se puede jugar y gasta la Inspiración. Antes del último loop, el Finisher no se puede jugar.
3. Regresión: un gig entero sin Finisher se juega igual que antes (nada se bloquea de más).

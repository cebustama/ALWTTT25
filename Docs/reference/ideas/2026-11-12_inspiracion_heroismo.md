# La Inspiración como el Heroísmo de Midnight Suns

Idea de Claudio y Matías, 2026-11-12, al cerrar la reunión. **Prioritaria** (Claudio). Estado: **decidida**
(D-PLAN-20 y D-PLAN-21 del plan de la demo); se implementa en ECON-2 y CARTAS-ARTE-1.

## La idea

- **Toda carta de composición genera Inspiración.**
- **Toda carta Finisher la consume.**
- No hace falta código nuevo: es una **convención** y el **valor por defecto** al crear cartas nuevas.
- Calza con el arte de Matías: las cartas de composición llevan una **estrella con un número** arriba a la
  izquierda (★1 Voz, ★2 Batería) y los Finisher tienen su propio marco (cian, con un emblema de estrella).
- Referencia: **Marvel Midnight Suns**, una de las inspiraciones del juego. Allí jugar cartas normales da
  Heroísmo y las cartas heroicas lo gastan: el turno tiene un arco (preparar → rematar) sin otra regla.

## Por qué resuelve tres problemas de la reunión (✦ IA)

1. **«Las cartas se juegan sin pensar»**: jugar composición sigue sin costar nada, pero ahora carga algo; la
   pregunta pasa de «¿juego?» a «¿cuánto cargo para el final?».
2. **«En el último loop no se puede jugar nada»**: los Finisher solo se juegan en el último loop (D-PLAN-14 = A)
   y gastan lo cargado. El último loop pasa de turno muerto a clímax.
3. **Combos**: un Finisher es el remate natural de un combo (D-PLAN-8).

## Lo que ya existe en el código (comprobado el 12-nov)

- **Una vez al jugarla:** `CardDefinition.inspirationGenerated` (hoy 0 en todo el contenido desde S5e).
  `CardBase` llama a `GenerateInspiration(InspirationGenerated)` al usar la carta y la cara ya tiene un campo
  para mostrarlo (`inspirationGenTextField`, oculto con 0).
- **En cada loop mientras suena su pista:** `AddInspirationPerLoopSpec` (DF-INSPLOOP, 2026-07-16), sin efecto en
  el LoopScore.
- **Inspiración básica:** +1 por loop (`GigFlowSettings.defaultInspirationPerLoop`).
- **Trampa:** `inspirationGenerated` también alimenta el término de complejidad del LoopScore
  (`LoopScoreCalculator`, `complexityMultiplier = 0.5` × la suma de las pistas). **Hay que ponerlo a 0** en su
  configuración para que el Big Num no cambie (previsto = real). Es un cambio de datos.
- **Finisher:** no existe una marca de Finisher en el código. Hay que añadirla para invertir el bloqueo del último
  loop (`FinalLoopLock`): el Finisher solo se juega cuando corre el último loop. Es la única pieza de código.

## Decisiones (Claudio, 2026-11-12)

- **D-PLAN-20 = las dos, según la carta.** Por defecto, la composición da ★N **una vez al jugarla**
  (`inspirationGenerated`). Algunas cartas especiales dan **por loop** (`AddInspirationPerLoopSpec`); esa carta
  **queda fuera de la demo o es la recompensa del gig 2**, antes de la Reina.
- **D-PLAN-21 = sesión propia CARTAS-ARTE-1** para la cara nueva: marcos Composición / Habilidad / Finisher, la
  estrella con el número de Inspiración y las ilustraciones por instrumento, justo después de ECON-2.

## Convención para cartas nuevas (a aplicar en el editor de cartas y en el JSON)

| Tipo | Coste | Genera | Cuándo se juega |
| --- | --- | --- | --- |
| Composición | 0 | ★1–★2 al jugarla | cualquier loop menos el último |
| Habilidad (acción) | como hoy | — | cualquier loop |
| Finisher | ★N (la Inspiración cargada) | — | solo el último loop |

Los números exactos se ajustan con el menú F5 (DEV-TUNING-1) y la telemetría.

## Arte de Matías (Google Drive)

- Ilustraciones de cartas de composición por instrumento (vocalista, guitarra-bajo, batería con fondos aparte,
  órgano): https://drive.google.com/drive/folders/1L_VZ0T_rK6-8Znpewoaglolzuo5YtWx3
- Números 0–9 dibujados (amarillo con contorno azul): https://drive.google.com/drive/folders/1UtUJcoa28Jf-Mki1CwouKkPT-SEea8Oy
- Maquetas de la cara nueva y de la mano: `img/2026-11-12_cartas_maqueta_marcos.png` y `img/2026-11-12_cartas_maqueta_mano.png`.

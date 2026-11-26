# Arranque S11 · PULIDO-ARTE + MAPA-2 + gigs desde JSON + zoom

Escrito el 2026-11-26 al cerrar S10. Planning, no autoridad. Fila 11 del «Orden de entrada» de
`DEMO-1211_Plan_de_Sesiones.md`. **D19 = C (Claudio, 2026-11-26):** entra todo lo pendiente y la congelación (S12) se mueve
detrás de S11, sin fecha fija: «no hay prisa, está todo bien con los plazos».

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. DEMO-1211 S11 — PULIDO-ARTE + MAPA-2 + gigs desde JSON + zoom.

Lee Docs/planning/active/ARRANQUE_S11_PULIDO.md y la fila 11 del orden de entrada de
DEMO-1211_Plan_de_Sesiones.md. Empieza con /buscar-referencias --all (pantalla de recompensa
«escoge tu carta»; bifurcación de mapa en roguelikes; zoom de cámara dramático en combates por
turnos) y preséntame las decisiones en formato D antes de construir. Si no cabe en una sesión,
propón el corte en dos (S11a / S11b) al empezar.

Contexto: S10 cerrada y subida el 26-nov: botón Tutorial (cuarteto fijo, manos guionizadas,
Redoble Final), Dúo/Trío/Cuarteto sin diálogos de ausentes, gig 1 = nodo «?» con semilla,
Cohesión 3 y −2 al perder, pausa con Espacio con scratch, combos con la regla nueva, cascada
del Alto, Levántate cautiva al Alto, íconos de músicos en los textos.

Restricciones: logs con la etiqueta [S11]; no escribir .meta; no editar MidiGenPlay; sin commit
ni push sin pedirlo; ≤3 smokes jugados por Claudio; pasos de editor uno a uno.

Cierre: smokes jugados, docs propuestos y aplicados con OK, proyectos.md, foco.md, página de
frentes y el arranque de S12 (congelación).
```

## Qué se pide (orden propuesto: lo que ve el jugador primero)

1. **Recompensa con arte** («Escoge tu carta», arte de Matías) y **mapa polinizado al vencer a la Reina**.
2. **Tooltips basales** (D-PLAN-15): por defecto solo lo esencial; las fichas de conceptos al mantener Alt. Diseño:
   `reference/ideas/2026-11-12_tooltips_basales.md` (marca `isBasic` en el glosario).
3. **Números dibujados de Matías** (D-PLAN-16, pedido #17): TMP Sprite Asset, como `brillo` y `musicos` de S10
   (`S10_InspirationIconTool`, `S10_MusicianIconTool`).
4. **Zoom dramático** (D8 = A): Lerp de cámara a un personaje, mantener X s y volver (momentos: convencer a alguien, colapso,
   Gran Final).
5. **Gigs y enemigos desde JSON** (D4 = B): gigs que referencian público existente por id, leídos de una carpeta en la build.
   Diseño: `reference/ideas/2026-11-12_herramientas_de_testeo_en_build.md` (solo la lectura).
6. **MAPA-2**: una bifurcación tras el gig 1 (Tentáculos o El jefe y su banda; el camino no elegido se apaga) y el editor de giras
   (`reference/ideas/2026-11-12_bifurcacion_y_editor_de_giras.md`). Encaja con el nodo «?» de S10 (`LinearMapController`,
   `RandomOption`, semilla de partida).

## A tener en cuenta (de S10)

- **`Elenco_S7.json` está desfasado** del asset: Rana Azul figura con vida 18 y el asset tiene 30 (S10 la bajó de 40). Antes de
  leer gigs desde JSON (punto 5), decidir qué manda: el JSON o los assets. Reimportarlo hoy pisaría la vida.
- `cohesionPenaltyOnLoss` = 2 en los cuatro gigs de la demo y en el JSON; el tutorial usa Reintentar y no resta.
- El log de Reintentar sale dos veces (inofensivo). El texto «a el primer Alto» de la cara de Levántate pide pulido.
- Textos ✦ IA de S10 por revisar en «Textos pendientes» (tutorial, glosario de combos, «el primer Alto», «Al jugarla»).
- El SFX de pausa es provisional (scratch de Claudio); el definitivo, de frenar el vinilo, al músico del equipo.
- Aplazado fuera de S11: identidad del solo de Sibi (D10 = B, sesión de música), identidad de cartas (D17 → POST-DEMO C10),
  combos de tres y mejorables (C2), Dúo y exceso de cartas (C11).

## Criterio de cierre

Una partida completa muestra la recompensa con arte, el mapa con bifurcación (el camino no elegido queda cerrado) y el zoom en al
menos un momento; un gig se carga desde JSON en la build; los tooltips salen cortos y las fichas con Alt.

# Arranque COMBOS-1 · combos con nombre en la previsión y jugar pensando

Escrito el 2026-11-17 al cerrar DEV-TUNING-1. Planning, no autoridad. Fila 7 del «Orden de entrada» de
`DEMO-1211_Plan_de_Sesiones.md` (D-PLAN-8 = A; D-PLAN-14 = A: el Finisher cierra el combo, ya hecho en ECON-2).
Si no llega antes del 27-nov, pasa a POST-DEMO C.

**Comprobación de calendario antes de empezar.** Antes de la congelación (S12, vie 27-nov) tiene que caber
**S10** (imprescindible). Si al abrir esta sesión quedan menos de 3 sesiones hasta el 27-nov, haz **S10 primero**
y COMBOS-1 se reduce a la alternativa B (solo la prueba de coste o tope, con F5, media sesión).

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. DEMO-1211 COMBOS-1 — combos con nombre en la previsión del Big Num.

Lee Docs/planning/active/ARRANQUE_COMBOS-1.md y haz COMBOS-1. Antes, comprueba el calendario
(S10 tiene que caber antes del 27-nov). Empieza con /buscar-referencias --all (Balatro nombra la
mano antes de jugar, contadores de combo de juegos de pelea, sinergias con nombre en deckbuilders)
y preséntame las decisiones en formato D antes de construir.

Contexto: DEV-TUNING-1 cerrada el 2026-11-17 (F5 ▸ Ajustes: jugadas por turno, robo, mano, ★
inicial, colapso; F6 salta la canción con puntuación real; contador N/M de la mano; página para
testers https://claude.ai/artifact/VCSqZeSsWnZvCFUHgUoxgv). Con F5 ya se prueban sin código
«una carta en total» y la mano corta. ECON-2 ya trae los Finisher, que cierran el combo.

Restricciones: logs con la etiqueta [COMBOS-1]; no escribir .meta; no editar MidiGenPlay; sin commit
ni push sin pedirlo; ≤3 smokes jugados por Claudio; pasos de editor uno a uno (o tutorial-artefacto
si hay mucho editor); previsto = real (LEGIB-1a): ningún combo suma sin verse en la previsión.

Cierre: smokes jugados, docs propuestos y aplicados con OK, proyectos.md, foco.md, página de
frentes y el arranque de la siguiente sesión del orden de entrada (MUSICA-1, con D-GF).
```

## Qué se pide (de `reference/ideas/2026-11-05_combos_de_banda.md` y la reunión del 2026-11-12)

1. **Catálogo corto de combos con nombre (5–8)**: por ejemplo batería + bajo coordinados, melodía y guitarra al
   unísono, toda la banda entra (ya existe como bonus de LEGIB-1a), cambio de tonalidad, Finisher que cierra.
2. **Cada combo sale en la previsión del Big Num con su nombre** al pasar una carta, y en el texto flotante al
   pagarse. Pasa por la misma función que el pago: previsto = real.
3. **Jugar pensando:** el problema de Matías («se juegan todas»). Probar con F5 (una carta en total, mano corta) y,
   si hace falta, un coste o un tope en las composiciones (solo datos).

## Lo que ya existe y hay que reutilizar

- **Previsión del Big Num** (LEGIB-1a): `GigCanvas.SetVibeReadoutPreview`, `CompositionSession.TryBuildCardPreviewLoopContexts`,
  `CountTracksNewThisLoop` / `FullBandEnteredThisLoop` (el bonus de toda la banda ya es un «combo» sin nombre).
- **Textos flotantes de cambios**: `CompositionCardClassifier` + plantillas `ui_fx_*` del glosario.
- **Finisher** (ECON-2): `CompositionSession` (cola de Finisher, loop final).
- **F5 ▸ Ajustes** (`SSoT_Dev_Mode.md` §23): para la prueba de «jugar pensando» sin tocar código; añadir un ajuste
  nuevo es una línea si hace falta (por ejemplo, coste de las composiciones).
- Telemetría v3 (`factory`, `tuning`): separa partidas con ajustes.

## Decisiones previsibles (formato D)

- **D1 · Qué combos entran:** catálogo de 5 / de 8 / solo los que ya se detectan (toda la banda, cambio de tonalidad).
- **D2 · Cómo se ven:** nombre en la previsión y flotante / además un contador de combo tipo juego de pelea.
- **D3 · Valor:** Big Num plano por combo / multiplicador.
- **D4 · Jugar pensando:** solo con F5 (una en total, mano corta) / coste de composición como dato / tope por loop.
- **D5 · Alcance si el calendario aprieta:** completa / solo la prueba de coste (B).

## Smokes (≤3, jugados por Claudio)

1. Pasar una carta que completa un combo: la previsión muestra su nombre y el valor; al pagarse, el Big Num sube
   exactamente eso y aparece el flotante.
2. Regresión: una carta sin combo da el mismo Big Num que antes (LEGIB-1a intacto).
3. «Jugar pensando» con la variante elegida (F5): una canción entera; ¿quedan cartas que no se juegan por decisión?

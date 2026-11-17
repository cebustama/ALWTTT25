# Arranque DEV-TUNING-1 · menú de ajustes en el juego (F5)

Escrito el 2026-11-17 al cerrar CARTAS-ARTE-1. Planning, no autoridad. Fila 6 del «Orden de entrada» de
`DEMO-1211_Plan_de_Sesiones.md` (D-PLAN-17 = A: «entra, y pronto», antes de COMBOS-1). Si no llega antes del
27-nov, pasa a POST-DEMO A1.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. DEMO-1211 DEV-TUNING-1 — el menú de ajustes en el juego (F5).

Lee Docs/planning/active/ARRANQUE_DEV-TUNING-1.md y haz DEV-TUNING-1. Empieza con
/buscar-referencias --all (menú de opciones de dev en la build, contador de mano «8/8» de Balatro,
mano llena en Slay the Spire, botón de saltar turno para pruebas) y preséntame las decisiones en
formato D antes de construir.

Contexto: CARTAS-ARTE-1 cerrada el 2026-11-17 (la cara de Matías: tres marcos, ✦ coste, «+N»,
rótulo de pista, fichas a la derecha de la carta, degradado y candado). BATERÍA-SOLO-1 está
construida y falta mi oído (lo hago aparte, con los pasos de ARRANQUE_bateria_solo.md de
MidiGenPlay). Antes del 27-nov quedan S10 (imprescindible) y S12 (congelación); esta sesión
no puede comerse S10.

Restricciones: logs con la etiqueta [DEV-TUNING-1]; no escribir .meta; no editar MidiGenPlay;
sin commit ni push sin pedirlo; ≤3 smokes jugados por Claudio; pasos de editor uno a uno
(o tutorial-artefacto si hay mucho editor).

Cierre: smokes jugados, docs propuestos y aplicados con OK, proyectos.md, foco.md, página de
frentes y el arranque de la siguiente sesión del orden de entrada.
```

## Qué se pide (de `reference/ideas/2026-11-12_menu_de_ajustes_dev.md`)

1. **Pestaña de ajustes con tecla directa F5**, dentro del Dev Mode (`DevModeController`, IMGUI, `ALWTTT_DEV`).
2. **Ajustes:** cartas por turno y músico (una / una de cada tipo / sin límite), robo al empezar el turno y
   entre loops, máximo de la mano, Inspiración al empezar y entre loops, sin límite de canciones, vida tras el
   colapso. **Añadir uno nuevo tiene que ser una línea** (registro de ajustes).
3. **«Guardar» y «Valores por defecto».** El valor por defecto es el del asset; el menú solo guarda lo cambiado
   (JSON en `persistentDataPath`), los assets no se tocan desde la build.
4. **El máximo de la mano a la vista**, como el «8/8» de Balatro junto al abanico.
5. **Saltar la canción hasta el turno del público** (añadido en S9c, probando a la Reina).
6. Log `[DEV-TUNING-1] clave: antes → después` y marca en la telemetría si la partida usó ajustes no de fábrica.

## Lo que ya existe y hay que reutilizar

- **F12 ▸ Inspiración** (`DevInspirationRulesTab`, `InspirationRules`, guardado por máquina): mismo patrón de
  guardar y volver a fábrica (`SSoT_Dev_Mode.md` §21). El registro de ajustes puede absorberla o convivir.
- **DEV-TEST-1 y DEV-HAND-1** (§20): cartas TEST y mazo fijo en `GigDevSettings`; aviso al hacer build (§20.4).
- Economía de jugadas de ECON-1 (`Docs/planning/Design_Action_Economy_v1.md`); `MeterTuningSO` (colapso);
  `GigFlowSettingsSO` (robo y mano). Código a leer: `GigFlowSettingsSO`, `DeckManager`, `HandController`,
  `GigManager` (presupuesto de jugadas), `DevModeController`.

## Decisiones previsibles (formato D)

- **D1 · Dónde vive:** pestaña nueva del Dev Mode con F5 / ampliar la pestaña Inspiración / ventana aparte.
- **D2 · Qué ajustes entran en la demo:** todos los de la lista / solo los de economía (jugadas, robo, mano,
  Inspiración) y el resto después.
- **D3 · El «8/8» de la mano:** siempre visible / solo cuando se llena / solo en la build de testers.
- **D4 · Saltar canción:** botón en pantalla / tecla de dev / las dos.
- **D5 · Build pública y de testers:** separar ahora (símbolo propio para la telemetría) / en S12.

## Smokes (≤3, jugados por Claudio)

1. F5 abre los ajustes; cambiar «cartas por turno» a «sin límite» deja jugar varias cartas del mismo músico en
   un turno; «Valores por defecto» lo devuelve.
2. Cambiar el máximo de la mano y el robo: la mano y el contador «N/M» lo reflejan en el turno siguiente;
   cerrar y abrir el juego conserva lo guardado.
3. Saltar la canción lleva al turno del público con la puntuación de esa canción; la Reina avanza
   huevo → huevo → eclosión saltando.

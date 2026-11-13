# Arranque S9c · DEMO-FIX-1: arreglos de la demo

Escrito el 2026-11-13 al cerrar S9b (AUDIO-LOOP-1). Planning, no autoridad. Fila 3 del «Orden de entrada» de `DEMO-1211_Plan_de_Sesiones.md`.

S9c **es** DEMO-FIX-1: una sola sesión con una lista corta de arreglos. La mayoría son datos, prefabs o ajustes en el Inspector, sin mecánica nueva. Si no cabe todo, se recorta por el final de la lista.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. DEMO-1211 S9c — DEMO-FIX-1: arreglos de la demo.

Lee Docs/planning/active/ARRANQUE_S9c_DEMO-FIX-1.md y haz S9c. Empieza con
/buscar-referencias --all (feedback dentro del juego, tooltips cortos, colocación de la
banda) y preséntame las decisiones en formato D antes de construir.

Contexto: S9b (AUDIO-LOOP-1) cerrada el 2026-11-13: nada se corta entre loops
(IMidiTransportChain, parche local a Maestro en reference/maestro-parches.md). Orden de
entrada: S9c → ECON-2 (la Inspiración) → CARTAS-ARTE-1.

Restricciones: logs con la etiqueta [S9c]; no escribir .meta; no editar MidiGenPlay;
sin commit ni push sin pedirlo; ≤3 smokes jugados por Claudio; pasos de editor uno a uno.

Cierre: smokes jugados, docs propuestos y aplicados con OK, proyectos.md, foco.md, página
de frentes y el arranque de ECON-2.
```

## Lista (en orden de prioridad; recortar por el final)

1. **Zig con dicción (D-ZIG-1).** `expressivityEnabled = 1` en `Assets/Prefabs/Core/Zig_SingerVoice.prefab`. Smoke de oído: la vocal se abre en las notas largas y en los tiempos fuertes. Aprovechar para renombrar el preset «Back (oh)», que suena a «a» (mide F1 730 / F2 1650; ver `reference/taller-melodia/README.md` y `reference/zig-voz-wrapped/`).
2. **Doble Armonía fuera** del mazo inicial de Zig (`Cantante_CardCatalogData`). Está rota: casi nunca tiene una melodía de Zig en la parte de destino, aunque esto es una hipótesis sin comprobar. El canto a dos voces queda en POST-DEMO B3.
3. **Carta de Sibi que cura Stress en lugar de Read the Room** (cierra D9, abierta desde S4b). `starter_take_five` (`ModifyStress −3` a sí mismo) ya existe y no está en ningún mazo. Hay que decidir si se reutiliza o si se crea una carta propia de Sibi, y el valor.
4. **Posiciones de la banda.** Ahora están muy pegados y unos encima de otros. Son datos de `BandStageLayout` y se ajustan con la ventana Character Lineup (S7), por local (S9).
5. **Botón de feedback** que abre un correo a allwtttcmm@gmail.com, más «abrir carpeta de telemetría». Hacerlo con `mailto:`, sin servidor. Enviar desde el juego pide trabajo nuevo y aviso de privacidad: eso va después de la demo.
6. **Company Name** de Unity, que sigue en «DefaultCompany». Cambiarlo mueve `persistentDataPath`, donde viven la telemetría y los `MidiDumps`: comprobarlo y avisar.
7. **Tooltips de los músicos más cortos.** Conito muestra seis fichas. Mostrar solo los estados activos, y Stress y Cohesión solo las primeras veces. Idea de la minuta: fichas ocultas por defecto, visibles manteniendo Shift o Alt.
8. **Si cabe: nodo aleatorio del mapa** (idea de Claudio, S9): 50 % Tentáculos, 50 % Ranas y Rayas, sorteado con semilla.

## Decisiones previsibles (presentarlas en formato D)

- **D1 · La carta que cura:** `starter_take_five` tal cual / una carta de Sibi nueva / Read the Room se queda y la curación se añade.
- **D2 · El feedback:** solo `mailto:` / `mailto:` + carpeta de telemetría / formulario después de la demo.
- **D3 · Tooltips:** filtrar por estados activos / fichas con Shift / las dos cosas.
- **D4 · Company Name:** el nombre exacto y qué hacer con los datos de la ruta antigua.

## Entradas

- `Docs/reference/reuniones/2026-11-12_matias.md`: puntos 4–6, 11, 14 y 15, con las notas de código.
- `Docs/planning/active/DEMO-1211_Plan_de_Sesiones.md`: fila 3 y D-PLAN-7.
- `Docs/systems/SSoT_Singer_Voice.md` y `Docs/reference/PinkTrombone_Voice_Levers.md` (punto 1).
- `Docs/systems/SSoT_Card_System.md` y los catálogos de cartas de Zig y Sibi (puntos 2 y 3).
- `Docs/systems/SSoT_Editor_Authoring_Tools.md` §23 (Character Lineup, `BandStageLayout`) (punto 4).
- Telemetría: rutas en la minuta (puntos 5 y 6).

## Smokes (≤3, jugados por Claudio)

1. Gig con Zig: la dicción se oye en las notas largas, y en su mano no hay Doble Armonía.
2. Gig con Sibi: la carta nueva cura Stress y la banda queda bien colocada con 2, 3 y 4 músicos.
3. Menú o pausa: el botón de feedback abre el correo y la carpeta de telemetría; el tooltip de Conito es corto.

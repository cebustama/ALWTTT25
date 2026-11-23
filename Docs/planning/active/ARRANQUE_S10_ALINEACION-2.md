# Arranque S10 · ALINEACIÓN-2 + TEST-SETUP-1

Escrito el 2026-11-20 al cerrar MUSICA-1 (parcial); actualizado el 2026-11-23 al cerrar MUSICA-1c. Planning, no
autoridad. Fila 10 del «Orden de entrada» de `DEMO-1211_Plan_de_Sesiones.md`, **imprescindible**. Calendario: hasta la
congelación (S12, vie 27-nov) quedan S10 y S11; PUB-SHIELD-1 pasa a C1 de POST-DEMO. MUSICA-1b y MUSICA-1c ya entraron.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. DEMO-1211 S10 — ALINEACIÓN-2 + TEST-SETUP-1.

Lee Docs/planning/active/ARRANQUE_S10_ALINEACION-2.md y la fila 10 del orden de entrada de
DEMO-1211_Plan_de_Sesiones.md. Empieza con /buscar-referencias --all (selección de banda y mazo
coherente por formación; setup de partida desde el menú; nodos aleatorios en mapas de roguelike)
y preséntame las decisiones en formato D antes de construir.

Contexto: la música de la demo está cerrada (MUSICA-1, -1b y -1c, 20–23 nov): Zig canta el
Gran Final; Wormus gira en el fill; Conito con 5 cartas de bajo por paleta (Jazz Funk nueva);
Ziggy con paleta de voz por carta y 3 cartas nuevas de recompensa (Blues, Balada, Grito).
Todo commiteado y subido el 23-nov.

Restricciones: logs con la etiqueta [S10]; no escribir .meta; no editar MidiGenPlay; sin commit
ni push sin pedirlo; ≤3 smokes jugados por Claudio; pasos de editor uno a uno.

Cierre: smokes jugados, docs propuestos y aplicados con OK, proyectos.md, foco.md, página de
frentes y el arranque de S11.
```

## Qué se pide

1. **ALINEACIÓN-2.** Las 3 alineaciones (Dúo, Trío, Cuarteto, elegidas en el dormitorio, NAVE-2) juegan el gig 1
   **sin diálogos de músicos ausentes** en el tutorial y con un mazo coherente con quién está.
2. **TEST-SETUP-1.** GigSetup desde el menú; **gigs y enemigos leídos desde JSON** de una carpeta (D-PLAN-9 = B;
   diseño en `reference/ideas/2026-11-12_herramientas_de_testeo_en_build.md`, solo la parte de lectura).
3. **Nodo aleatorio del mapa** (pasado desde S9c, D5 = B): 50 % Tentáculos, 50 % Ranas y Rayas, con semilla.

## A tener en cuenta (de MUSICA-1)

- `1b - Linear Map` ya devuelve el gig al mapa (`gigReturnDestination = Map`); la nave queda en «Volver a la nave».
- La vida del público subió ×1,5 (menú *ALWTTT ▸ Balance*). Si los JSON de gigs traen `maxVibe`, que respeten los
  valores nuevos o se escalen con el mismo criterio.
- `GigDevSettings`: quedó encendido el mazo fijo `smoke-5` y la semilla fijada 655931081 (smoke 5 de MUSICA-1c).
  Apágalos para jugar las alineaciones con su mazo real; el aviso de build los apaga antes de S12.
- **Mazo coherente por formación:** Ziggy tiene ahora más cartas en su catálogo (Blues, Balada, Grito, de
  recompensa) y Conito una más (Jazz Funk, de recompensa). El mazo inicial no cambió.

## Criterio de cierre

Las 3 alineaciones juegan el gig 1 sin diálogos de ausentes; un gig se carga desde JSON en la build; el nodo
aleatorio sale igual con la misma semilla.

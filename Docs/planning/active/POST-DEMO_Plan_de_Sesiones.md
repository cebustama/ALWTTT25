# Después de la demo: plan por sesiones (BORRADOR)

**Estado:** borrador del 2026-11-12 (al día con D-PLAN-7..21), propuesto por el agente a pedido de Claudio («después de la demo, seguir
con un plan bien definido en sesiones, incluyendo las ideas y tareas pendientes»). **No está aceptado**:
lo cierran D-PLAN-19 y la revisión tras la demo. Hasta el 11-dic manda `DEMO-1211_Plan_de_Sesiones.md`.

**Cómo funciona.**
- Todo lo que no entre en la demo cae aquí, en su bloque, sin perderse. Una idea nueva entra en un bloque, o en
  «Sin bloque» si todavía no se sabe dónde.
- Cada sesión tiene un objetivo único y un «lo has logrado si…», como en el plan de la demo. Las sesiones de
  runtime llevan logs con su etiqueta y como máximo 3 smokes humanos.
- **Ritmo:** 3 sesiones por semana. Se elige el bloque por semanas (no se salta de bloque cada día) para no
  perder el hilo.
- **MidiGenPlay** sigue con su cola propia (`MidiGenPlay/Documentation~/planning/active/Sesiones_MidiGenPlay.md`)
  y su regla: una sesión de MidiGenPlay solo si esa semana se cumplieron las de ALWTTT. El bloque B de abajo
  es el lado de ALWTTT de esa línea.

## Bloque 0 · Escuchar la demo (semana del 14-dic)

| # | Sesión | Objetivo | Lo has logrado si… |
| --- | --- | --- | --- |
| P0 | **TRIAGE-DEMO** | Reunir el feedback de Discord y del correo, y la telemetría que manden los testers; clasificar en bloqueante / bloque / descartado | cada comentario tiene destino en este plan |
| P0b | **REPLAN** | Reordenar los bloques con lo aprendido y cerrar este borrador | el plan queda aceptado con fechas para las 4 semanas siguientes |

## Bloque A · Herramientas (multiplican cada prueba)

| # | Sesión | Objetivo | Origen |
| --- | --- | --- | --- |
| A1 | **DEV-TUNING-2** | Si el menú F5 no entró antes de la demo, hacerlo (registro de ajustes, guardar, valores por defecto); si entró, ampliar sus campos | `ideas/2026-11-12_menu_de_ajustes_dev.md` |
| A2 | **TOOLS-1: exportar y ampliar el JSON** | La lectura de gigs y enemigos desde JSON entra en S10 antes de la demo (D-PLAN-9 = B); aquí, el botón de exportar y lo que no haya llegado | `ideas/2026-11-12_herramientas_de_testeo_en_build.md` pasos 1–2 |
| A3 | **TOOLS-2: editor de enemigos y gigs en el juego** | Formularios en el panel de testeo | ídem, paso 3 |
| A4 | **ENSAYO-1: la sala de ensayo como taller** | Jugar cartas libremente, sin público, y oír las combinaciones; parte de `2 - Ship Interior`. Además (Claudio, 2026-11-14): **intercambiar las posiciones de la banda en el escenario** (quién va delante y quién atrás), guardado con la banda; es la D5 C de S7 | minuta 12-nov, punto 13; D-PLAN-13 |
| A5 | **TOOLS-3: cartas en JSON y su editor** | Formato JSON de cartas, importación y editor en el juego | ídem, paso 4 |
| A6 | **TLM-2: telemetría y feedback remotos** | Que la build mande la telemetría y el feedback a un servidor (función de la web), con aviso de privacidad | minuta 12-nov, puntos 10–11 |

## Bloque B · Música y variedad (lado de ALWTTT)

| # | Sesión | Objetivo | Origen |
| --- | --- | --- | --- |
| B1 | **MUSICA-2: niveles armónicos en el juego** | `arrange()` del taller en C#, con semilla por jugada; subir de nivel una carta = un escalón | cola de MidiGenPlay, «Niveles armónicos en el motor»; memoria «las cartas son paletas» |
| B2 | **LASTLOOP-B** | Investigar si una carta jugada en el último loop puede entrar en la siguiente barra (segundo reproductor, como SONG-TAIL-1) | D-PLAN-14 B |
| B3 | **DUET-1: Doble Armonía Tier B** | El canto a dos voces con Pink Trombone, cada voz con su perfil (Claudio, 2026-11-13), y que la carta vuelva al mazo. Prototipo de oído en el Taller de melodías («Segunda voz»: relación como el juego o intervalo fijo, a la par / eco / coro, perfil propio). Hallazgo: la regla del juego (`NearestChordToneHarmonyStrategy`, 3–14 semitonos) cruza la melodía, unas veces por encima y otras por debajo; revisar junto con la condición de juego (hipótesis de la minuta). Va con F2–F3 de la voz de Zig (`reference/zig-voz-wrapped/`) | minuta 12-nov, punto 6 · taller-melodia |
| B4 | **CANCIONES-POR-GIG** | Canciones y loops por gig según el local y el público (hoy 3 × 3 para todos) | plan de la demo, «Deseos» |
| B6 | **INSP-LOOP: la carta que da Inspiración por loop** | Si no entró como recompensa del gig 2: llevarla al pool de recompensas y ajustar | D-PLAN-20 |
| B7 | **AUDIO-LOOP-2: loops a la hora exacta** | Tras S9b queda un hueco pequeño y ocasional entre loops: el siguiente arranca cuando se nota el final (sondeo por fotograma) y, si se jugó una carta, tras renderizar. Preparar el loop siguiente antes y lanzarlo a una hora fija. Medir antes con `gapMs` del log `[AUDIO-LOOP-1]`; si molesta en la demo, adelantar | `SSoT_Audio` §4.11 y §9; misma frontera que B2 |
| B5 | **INTERACCIONES-1** | Convencidos que cantan, «boooring» entre loops | plan de la demo, «Deseos» (REPLAN-ART D3) |
| — | Cola de MidiGenPlay | Comping y melodía siguen los acentos, orden de entrada, transiciones, «Nervioso», galope, narrador musical | `Sesiones_MidiGenPlay.md` |

## Bloque C · Diseño del combate

| # | Sesión | Objetivo | Origen |
| --- | --- | --- | --- |
| C1 | **PUB-SHIELD-1** | Si no llegó antes de la demo (D-PLAN-11 = B, orden 9): público con escudo | D-PLAN-11 |
| C2 | **COMBOS-2** | Combos que se desbloquean con la relación entre músicos (sala chill) | `ideas/2026-11-05_combos_de_banda.md` |
| C3 | **CARTAS-MATIAS** | Las cartas nuevas de Matías que piden mecánica nueva | pedido #15 |
| C4 | **READ-ROOM-2** | Rediseñar Read the Room (o retirarla): que no quede obsoleta al conocer los gustos | minuta 12-nov, punto 4 |
| C5 | **NERVIOSO-1** | Qué deja el colapso a largo plazo: «Nervioso» con notas falsas | `ideas/2026-11-11_nervioso.md`, D9 |
| C6 | **OLEADAS-1** | Repertorio de gigs por oleadas (un gig por mecánica, luego mezclas) | `ideas/2026-11-11_oleadas_y_repertorio_de_gigs.md` |
| C7 | **SIBI-COLMENA** | La interacción única de la Reina con Sibi («anularlo» o «llamarlo a la colmena») | ARRANQUE S9, D3 «Después» |

## Bloque D · Mundo y contenido

| # | Sesión | Objetivo | Origen |
| --- | --- | --- | --- |
| D1 | **MAPA-2 + editor de giras** | Si no entró en la demo: la bifurcación y el editor | D-PLAN-6, `ideas/2026-11-12_bifurcacion_y_editor_de_giras.md` |
| D2 | **DORMITORIO-2** | El panel del dormitorio de Matías por piezas | pedido #11 |
| D3 | **CHATARRERO-1** | La tienda que «abre el show» y la mejora inicial (pantalla del bicho en el foco) | plan de la demo, «Deseos» |
| D4 | **POSES-1** | Poses por estado y por habilidad para todo el público | pedido #12 |
| D5 | **SALAS-1** | Las salas que falten (cocina, jardín, baño) cuando llegue el arte | pedido #7 |
| D7 | **CARTAS-ARTE-2** | Si CARTAS-ARTE-1 no entró o quedó a medias: marcos, estrella e ilustraciones de todas las cartas | D-PLAN-21 |
| D6 | **FUENTE-1** | La fuente completa dibujada de Matías | pedido #17 |

## Bloque E · Motor reutilizable

| # | Sesión | Objetivo | Origen |
| --- | --- | --- | --- |
| E1 | **GAMEKIT-0: inventario** | Qué clases de ALWTTT no dependen de música ni cartas | `ideas/2026-11-12_motor_reutilizable.md` |
| E2 | **GAMEKIT-1** | Sacar las primeras piezas (bus de eventos, Dev Mode con ajustes, telemetría) a un paquete local, cuando arranque un segundo juego que las use | ídem; D-PLAN-18 |

## Bloque F · Deuda

- `vendedor.png` duplicado; `nave espacial.png` (31 MB) en `Resources/`; `CohesionPipsView` duplica `GigCanvas.SetCohesion`.
- F-TIP-7 (fichas apiladas en el tutorial); iconos de objetivo con contorno en lugar del disco.
- *Company Name* de Unity («DefaultCompany»); separar build de testers y build pública.
- Ideas sin bloque todavía: nombres de salas siempre visibles (si M6 lo pide); colocar la banda a mano (D5 C).

## Orden recomendado (✦ IA)

0 → A (A1, A2, A4) → B1 → C (C1, C3, C4) → D, con E solo cuando haya un segundo juego. Las herramientas van
primero porque abaratan todo lo demás: con el menú F5 y los gigs en JSON, Matías puede probar balance y
contenido sin esperar a una sesión de código.

# Ideas de Claudio en MUSICA-1c (2026-11-22)

Planning, no autoridad. Salen de las respuestas de Claudio a D1–D4 de MUSICA-1c. Cada una con su destino propuesto
(pendiente de su OK para entrar en `DEMO-1211_Plan_de_Sesiones.md` o `POST-DEMO_Plan_de_Sesiones.md`).

## 1. Un walking de jazz de verdad (D1: A, B y C)

Claudio toca bajo: «es tan importante para mí… quiero que suene increíble». Hace A y B (variantes de datos, ya creadas)
y C (motor en MidiGenPlay).

Criterio de Music Lab (`Music Lab/Análisis/analysis-fly-me-to-the-moon.md` §8): raíz en el tiempo 1, 3.ª o 5.ª en el 2 y el 3,
nota cromática de aproximación a la raíz siguiente en el 4; con dedos, tono redondo y maderoso, sin slap.

Lo que ya hace `BuildWalkLine` (MidiGenPlay `BassTrackComposer.cs`, verificado en código): raíz en el 1, notas del acorde
cercanas a la anterior en el medio y aproximación cromática o de tono a la raíz siguiente al final. Cumple ese criterio.

Lo que falta para que suene a jazz (✦ IA, de la práctica del walking; por contrastar con Claudio):

| # | Qué | Dónde |
| --- | --- | --- |
| J1 | La batería: un swing en 4/4 (ride «ding, ding-a, ding», charles en el 2 y el 4). Hoy no hay ninguno en 4/4 | Datos: patrón nuevo (skill `rhythm-pattern-generator`, jazz) |
| J2 | El sonido: Acoustic Bass (GM 33) en lugar de Fingered para el walking | Datos: `InstrumentEffect` de la carta |
| J3 | Notas de paso (de la escala o cromáticas) en el 2 y el 3, no solo notas del acorde | Motor |
| J4 | Más aproximaciones: encierro (arriba y abajo del destino) y por la quinta del acorde siguiente | Motor |
| J5 | Notas de salto («skips»): un tresillo fantasma antes de un tiempo, de vez en cuando | Motor (rejilla de tresillo) |
| J6 | Dinámica de jazz: negras parejas y ligadas, apoyo leve en 2 y 4 (hoy la curva acentúa 1 y 3) | Motor |
| J7 | Contorno: líneas que suben o bajan un compás entero, no solo la nota más cercana | Motor |
| J8 | El giro (C original): en el compás del fill, una subida en corcheas o tresillos hacia el acorde siguiente | Motor |
| J9 | «Two-feel» (blancas) en la primera vuelta y «four-feel» (negras) después | Motor o datos (horario por loop) |

Bibliografía para contrastar (no consultada en la sesión): Ed Friedland, *Building Walking Bass Lines* (Hal Leonard, 1995);
Ray Brown, *Ray Brown's Bass Method* (1963); John Goldsby, *The Jazz Bass Book* (Backbeat, 2002).

**J10, jazz-funk (pregunta de Claudio, 2026-11-23):** ¿bajos que mezclen jazz y funk? Hoy las piezas existen por
separado: el ritmo funk (Phrases Funk con notas muertas en la caja; SelfPocket en semicorcheas con fantasmas, hammer-on
y pull-off) y las notas del jazz (`JazzWalkPlanner`: notas de paso, encierros, aproximaciones, contorno). No se combinan:
Phrases sustituye a la figura, y el jazz solo lee la rejilla del arpegio. Falta la costura: que el planificador de jazz
reciba el ritmo del funk (`jazzWalk.rhythmSource` = rejilla / SelfPocket / bombo). Los fantasmas siguen siendo fantasmas
y los pops, octavas. Referencias: Jaco Pastorius («Come On, Come Over», 1976), Rocco Prestia (Tower of Power,
«What Is Hip?», 1973), Paul Jackson («Chameleon», Headhunters, 1973), Marcus Miller.
D6 = hacerlo ya (2026-11-23): `jazzWalk.rhythmSource = Pocket` implementado en MidiGenPlay (por defecto Grid, sin
cambios). Variantes de prueba: Jazz Funk 1c (dedos, semicorcheas propias), Jazz Funk Slap 1c (el pocket de Slap Groove)
y Jazz Walk doble 1c (el jazz en corcheas, lo que pidió Claudio tras oír Walking doble 1c). A/B en el Banco pendiente.

**Solo de bajo (Claudio, 2026-11-23):** Walking doble 1c «idealmente sonaría cuando no hay tantas otras pistas o ninguna
otra está destacando. Podría ser incluso una especie de solo». Idea: que la línea en corcheas salga cuando el bajo es la
pista destacada (o la banda está en pocas pistas), como un solo. Destino: después de la demo, junto a la paleta por contexto.

## 2. Rise Up: formas de subir la escala (D2 = A)

Que Rise Up varíe por canción (aplicado: `figureVaryPerSong` = sí) y, además, **por modo o escala** y **por tipo de escalera**:
zigzag, saltos, corcheas, subida dramática de octavas. Referencias que pide buscar: Freddie Mercury, Chris Cornell, Mike Patton
(chillidos y gritos) y rock clásico en general.

Lo que ya existe en MidiGenPlay (`MelodyFigurePlanner.cs`): figuras Staircase, ZigZag, Leaps, Wave, Pedal, Sequence, Walk;
dirección Up; `atTop` = Octave; ritmo `RiseUp`. **Ojo:** los ajustes de TEST que lleva hoy Rise Up usan Wave con ritmo Cells,
que ondula más que sube. Falta una **paleta de melodía** (como la del bajo, MGP-BASS-PALETTE-1) para sortear la escalera
por canción. Destino: MidiGenPlay (paleta de melodía) + sesión de oído en ALWTTT; después de la demo salvo hueco.

## 3. Tempo: cartas que lo mueven y cartas que no (D4 = A, más diseño)

Aplicado hoy (A): Root Line sin tempo, Ballad Hold a Very Slow (70–100), Push It ×1,25.

Diseño de Claudio:
- La mayoría de las cartas **no** tocan el BPM. La canción sale de un rango por defecto configurable, con pesos
  (por ejemplo Slow : Moderate : Fast = 1 : 3 : 1).
- Unas pocas cartas **sí** lo mueven, y el jugador combina: una balada acelerada, con distorsión, con batería de cumbia…
  «La idea es que el jugador pueda escoger.»
- La carta lo dice: icono y/o etiqueta en el texto, del tipo **[BPM: Lento]** (vals, balada) o **[BPM: ×1,5]** / **[BPM: sube]**.

Consecuencias por decidir: quitar el tempo de Wormus Major/Minor, Walking Bass, Slap Groove y quizá Default Mode
(hoy Fast, Fast, Fast, Slow, Slow); hoy cada parte nace con Moderate fijo (`SongCompositionUI.cs:137-138`); ningún patrón
de batería tiene rango de BPM (el filtro de PATRON-BPM-1 no actúa). Destino propuesto: datos y el reparto por defecto antes
del 27-nov si cabe; la etiqueta en la carta en S11 (pulido, junto a los tooltips basales).

## 4. «Me gustó esta canción» (telemetría)

Una tecla o un botón en pantalla para que el jugador, Claudio o un tester marque una canción, y que quede guardado su código
para reproducirla y analizar por qué gustó. Encaja con lo que ya hay: `DevRunTelemetryLogger` (JSON Lines en `DevTelemetry/`,
solo con `ALWTTT_DEV`) y el volcado del gig que el Banco de cartas ya sabe comparar («Comparar con volcado del gig…»).
Guardar ese volcado más la semilla, y que el Banco lo cargue, cierra el círculo. Destino propuesto: sesión corta antes de S12
(la build de testers), o S12.

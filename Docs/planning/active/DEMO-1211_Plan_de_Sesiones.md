# Demo del 11-dic — plan por sesiones (v2, ACEPTADO)

**Estado:** **aceptado por Claudio el 2026-10-30** (D-PLAN-0..4 cerradas, abajo). Es la secuencia
vigente hacia la demo: sustituye a la tabla «Secuencia vigente» de REPLAN-DIAG en
`S5_DemoCutClose_Sub_Roadmap.md`, que conserva la lista M1–M7 (el *qué*) y apunta aquí (el *cuándo*).
D-PLAN-0 enmienda REPLAN-ART D1 («sin ramas, tienda ni terapia»): la nave y sus salas entran, las que no
funcionan como «EN DESARROLLO».
**v2 (misma tarde):** nueva filosofía de Claudio: la demo es el **esqueleto del juego real**, montado con
todo el arte que ya existe, para que se vea el avance y se imagine la experiencia final. Entra la nave
como centro, el elenco con sprites nuevos, los gritos y la Reina con su sprite.
**Criterio de la demo (D-ART-2):** se entiende sin explicarla: cómo se juega, cómo se gana, cómo se
pierde. Lo nuevo no lo sustituye: lo que no esté hecho lleva «EN DESARROLLO» y un «Volver», para que
nadie se atasque ahí.

## El esqueleto (lo que verá quien juegue)

```
Menú → NAVE (plano con salas)
          ├─ Dormitorio: elegir alineación (arte «Personajes»)            → funciona
          ├─ Cabina del capitán (Sion): mapa de Asteroide City            → funciona (mapa lineal)
          │     └─ gig 1 → recompensa («Escoge tu carta») → nave
          │     └─ gig 2 → … → Reina Gusano → mapa polinizado
          ├─ Chatarrero: tienda / «abre el show»                          → EN DESARROLLO
          ├─ Sala de ensayo, recreación, cocina, jardín, baño            → EN DESARROLLO
```

Arte disponible (`Assets/Resources/Sprites/Ship/`): plano de la nave con salas numeradas
(`image (5).png`) y la nave entera; sala de ensayo, sala recreativa, chillout y vendedor a pantalla
completa. En el plano aparecen además las pantallas de mapa, «Escoge tu carta», «Personajes» (Zit,
Conito, Sivi, A-23), el capitán Sion y las libreas (la tripulación). **Pedir a Matías los archivos
sueltos** de esas pantallas y de la pantalla de elegir con el bicho en el foco: hoy solo están dentro del
plano o en capturas.

## Supuestos

- **3 sesiones por semana** de unas 2–3 horas. Si son menos, se recorta por el orden de abajo; no se
  alargan las sesiones ni se trabaja de madrugada.
- Cada sesión de código trae su guion de partida (máx. 3 smokes humanos; regla de REPLAN-DIAG).
- La nave nueva es una **escena fina sobre un PNG** con zonas clicables, como `LinearMapController`
  (MAP-DEMO-1). No reutiliza `2 - Ship Interior` (ensayo con composición), que queda para Phase C.
- Congelación de arte: vie 20-nov. Congelación de funcionalidades: vie 27-nov.

## Sesiones

| Semana | # | Sesión | Objetivo único | Lo has logrado si… | M |
| --- | --- | --- | --- | --- | --- |
| 2–6 nov | S1 | **COH-1, cierre** ✅ 2026-11-01 | Jugar el guion de `COH-1_Guion.md` (ahora en `planning/archive/`) y pegar el `Editor.log` | Pasos 1–9 coinciden con el log; commit. **Hecho:** pasos 1–9 y smoke 2 PASS jugados por Claudio, verificados con capturas (sin `Editor.log`); de paso, MAP-DEMO-2 (paneo, camino, gigs pasados, tarjeta de la Reina) | M3 |
| | S2 | **GIG-UI-1** ✅ 2026-11-03 | Montar la consola del manager según su runbook | La cohesión se ve en el HUD y la consola se lee. **Hecho:** consola, cohesión en el HUD, columna de tooltips de colores del público (icono y cuerpo muestran lo mismo), aviso «ya usó una carta de acción/composición», indicadores de jugadas ocultos (D-UI-3=A, revisar en S4), gana el personaje dibujado encima; demo a 3 canciones × 3 loops; commit `94d3e5d`. Runbook en `planning/archive/` | M3 |
| | S3 | **LEGIB-1a: ver antes de jugar** ✅ 2026-11-06 | Al pasar el ratón por una carta, el Big Num muestra «Y → Z» | El número previsto es el que pasa (3 cartas distintas). **Hecho:** «Y → Z» siempre visible mientras suena un loop (Y = ahora; Z dorado con carta; sin flecha si no cambia); toda pista nueva sube al menos +1 y bonus de toda la banda (+3); CHECK previsto = real en todos los loops, 5 smokes jugados por Claudio | M1 |
| 9–13 nov | S4 | **LEGIB-1b: lo que recibes** ✅ 2026-11-10 | La intención del público dice qué hará y a quién (línea al objetivo); sin textos internos («AddVibe») | Una persona ajena dice qué hará cada miembro del público. **Hecho (vista 3, D8=B):** objetivo fijado al mostrar la intención (solo Spotlight lo cambia), número sin «x» + iconos de los músicos que golpea, barra del músico verde con lo que perderá en rojo y «final/max», proyección = turno real (`StressProjector`), floater con el nombre de la habilidad, gustos traducidos, textos y SSoT corregidos, colores de identidad provisionales. 3 smokes jugados por Claudio | M1 |
| | S4b | **LEGIB-1b, vistas al pasar el ratón** ✅ 2026-11-11 | Hover sobre músico (quién le pega y cuánto) y sobre miembro del público (a quién pega), con bordes del color de identidad, líneas al objetivo y Tab mantenido para verlo todo; iconos de objetivo legibles sobre el fondo | Con el ratón encima, se sabe sin leer texto quién golpea a quién. **Hecho:** vistas 1 y 2, Tab, sin vistas con carta cogida, aura roja; iconos con disco y aro, «toda la banda» en un icono «×N»; intención sobre la cabeza; frase «A Conito: 5 de Stress»; enlaces de concepto con su ficha debajo (como Monster Train); colapso → Expuesto (configurable); 8 smokes jugados por Claudio | M1 |
| | S5 | **NAVE-1** ✅ 2026-11-11 | Escena de la nave: plano con salas clicables; cada sala abre su arte con «EN DESARROLLO» y «Volver»; la cabina abre el mapa | Menú → nave → cabina → gig → vuelta a la nave, sin romper la partida. **Hecho:** escena `1c - Ship Hub` con 5 salas como datos (hover que tiñe y nombra, «EN DESARROLLO» antes del clic), fundido exterior → corte en la primera visita, el gig vuelve a la nave, «Volver a la nave» en el mapa, corazones de cohesión en el mapa; montada con un tutorial-artefacto (https://claude.ai/artifact/E1CfGu3Y3cGSidN2xHm3Y3); 3 smokes jugados por Claudio | — |
| | S6 | **NAVE-2: dormitorio = alineación** ✅ 2026-11-11 | En el dormitorio se elige la banda: 2, 3 o 4 músicos | Cada opción arranca con esa banda y su mazo; sale en la telemetría. **Hecho:** `BandLineupScreen` con Dúo / Trío / Cuarteto y combinación libre (mínimo 2), siluetas por máscara sobre el arte «Personajes», banda fija tras el primer gig, `lineupId` en la telemetría; tutorial-artefacto (https://claude.ai/artifact/Q1QdFtRifMGMSkuVqoSwXW); 3 smokes jugados por Claudio | M2 |
| 16–20 nov | S7 | **ELENCO-1** ✅ 2026-11-12 | Los 9 sprites listos entran como público; gigs 1 y 2 con sus grupos (`reference/ideas/2026-10-30_publico_con_rol.md` §7) y el objetivo «el líder». **Añadido tras S6 (D5=A):** la banda se coloca bien con 2, 3 o 4 músicos (disposición a mano por número, reparto automático de respaldo; arreglar `Count >= i` en `GigManager.BuildBand`) | Los dos gigs tienen público propio que se ve distinto y actúa distinto; las 3 alineaciones se ven bien en el escenario. **Hecho:** tres encuentros («Ranas y Rayas», «Tentáculos», «El jefe y su banda») creados desde un JSON (`Settings/Audience/Elenco_S7.json`); Big Guy es el Jefe y Kid e Influencer lo animan (`AudienceLeader`, se apaga al convencerlo); ranas con coro (×2 si coinciden en el músico); Tentáculos cuenta uno, dos, ¡TRES!; eje de roles en los gustos; el público apunta al que menos golpes aguanta (cuenta Calma y Expuesto); `BandStageLayout` con formaciones de 2, 3 y 4; ventana Character Lineup (alturas, vista en el escenario, «Ajustar UI»); tutorial-artefacto (https://claude.ai/artifact/1gCGFncSA8TtK3XG1MxqEo); todos los smokes jugados por Claudio | M4 |
| | S8 | **GRITOS-1** ✅ 2026-11-12 | Bocadillos con frases por habilidad; `{músico}` se sustituye por el objetivo. **Añadido en S7:** poses del público por habilidad (sprite mientras la anuncia y al actuar, huecos vacíos = sprite normal; pedido #12 a Matías) y el beat de Conito (flota y no rebota) | Antes de cada golpe se lee quién lo recibe; alguien se ríe. **Hecho:** bocadillo al actuar con `{musico}` = la flecha (el golpe espera 1,2 s), frases ES/EN de los 7 miembros en el glosario (lista `shouts`), variantes por músico (Robot C2 «se pierde en el tiempo»), entrada con rebote y dentro de la cámara, viñeta «ataque jefe» de Big Guy (una vez por gig), poses `pending`/`acting` vacías (= sprite normal), Conito salta con el beat sin perder el flotado; smokes 1–10, R1 y R2 jugados por Claudio | M1 |
| | S9 ✅ | **REINA-1** (cerrada 2026-11-12) | Reina Gusano con su sprite y huevo · huevo · eclosión | En la prueba, alguien explica qué hace la Reina sin ayuda | M4 |
| | S9b | **AUDIO-LOOP-1: nada se corta entre loops** (insertada el 2026-11-12, decisión de Claudio: necesaria antes de la demo) | Que el crash del cierre, y cualquier nota que siga sonando, cruce la frontera del loop sin cortarse. Causa comprobada: cada loop es un `MPTK_Play` nuevo y Maestro hace `MPTK_InitSynth` + `ThreadClearAllSound` al empezar cada MIDI. Arranque: `ARRANQUE_S9b_audio_loop.md` | En un gig de 3 canciones × 3 loops con fills, el crash del cierre suena entero al empezar el loop siguiente y ninguna nota se corta de golpe; el Big Num sigue «previsto = real» | M3 |
| 23–27 nov | S10 | **ALINEACIÓN-2 + TEST-SETUP-1** | Tutorial y mazo coherentes con cada alineación; GigSetup desde el menú | Las 3 alineaciones juegan el gig 1 sin diálogos de músicos ausentes | M2 |
| | S11 | **PULIDO-ARTE + MAPA-2** | Recompensa con el arte de «Escoge tu carta», mapa polinizado al vencer a la Reina. **D-PLAN-6:** sale «escenario por nodo» y entra MAPA-2: una bifurcación tras el gig 1 (Tentáculos o El jefe y su banda; el camino no elegido se apaga y no se vuelve) y un editor de giras (`reference/ideas/2026-11-12_bifurcacion_y_editor_de_giras.md`) | Se ve en una partida completa; el jugador elige camino y el otro queda cerrado | M5 |
| | S12 | **Congelación** (vie 27-nov) | Build interna + guion de la prueba M6 | La build arranca desde un `.exe` limpio | M7 |
| 30 nov–4 dic | S13–S15 | **Prueba M6** | ≥3 personas externas sin explicar; se mira dónde se atascan y se les pide que cuenten cómo se gana y cómo se pierde | Cada una lo sabe decir; solo se arreglan bloqueantes | M6 |
| | | Build candidata (vie 4-dic) | | | M7 |
| 7–11 dic | S16 | **Discord** | Texto, capturas, publicación (vie 11-dic) | Publicada | M7 |

**S9b insertada el 2026-11-12:** al probar los fills de MidiGenPlay (sesión 1 de su cola), el crash que cierra un loop se corta de golpe al empezar el siguiente. Claudio lo considera necesario para la demo. Cabe en la semana del 16 al 20 porque S7 se cerró antes; si no cabe, va antes que S10. No está en el orden de recorte.

**S4b insertada el 2026-11-10** (D8=B de S4): las vistas al pasar el ratón salen de S4 para que S4 cerrara con la vista en reposo probada. Empuja S5–S6 media semana; si no cabe, S4b se recorta a «iconos legibles + hover del músico».

**Abierto tras S4b (2026-11-11):**
- **D9, qué deja el colapso.** Desde S4b el colapso aplica `exposed` (configurable en `MeterTuningSO`); en la demo no hay carta que cure Stress (`starter_take_five` existe sin mazo). Recomendado: mantener Expuesto y decidir tras jugarlo; alternativa, meter Take Five en un mazo. Idea a largo plazo: «Nervioso» (`reference/ideas/2026-11-11_nervioso.md`).
- ~~D10, el público que golpea a toda la banda~~ → resuelta (C): un icono «×N» con el aro partido en los colores; caras apiladas para 2–3.
- **F-TIP-7:** el hover de conceptos del tutorial puede apilar fichas (sin comprobar).
- **Iconos de objetivo con contorno** en lugar del disco: a Claudio «no le convence» el disco; no urgente.

**Abierto tras S6 (2026-11-11):**
- **Colocación de la banda** según el número de músicos: entra en S7 (D5=A). Después de la demo: que el jugador la coloque (D5 C).
- **Panel del dormitorio de Matías** (boceto «cabina» + «cuarto de los pk»): se implementa cuando esté el arte; requisitos en el pedido #11.

**Abierto tras S5 (2026-11-11):**
- `vendedor.png` está duplicado (idéntico) en `Sprites/Ship/` y `Sprites/Ship/Rooms/`; `Rooms/` sin commitear (la nave usa `Rooms/chillout`).
- `nave espacial.png` (31 MB) está en `Resources/` y entra en la build; la nave ya no la usa.
- ESC en la nave vuelve al menú (UIManager); sin cambio.
- `CohesionPipsView` duplica `GigCanvas.SetCohesion`; unificar después de la demo.
- Idea a probar: nombres de las salas siempre visibles, tenues (referencia: corte de casa grecorromana), si alguien no descubre el hover en la prueba M6.

## Reunión con Matías del 12-nov (decidido)

Minuta: `reference/reuniones/2026-11-12_matias.md`. **Decisiones cerradas por Claudio el 12-nov** en el wrapped
de la reunión (https://claude.ai/artifact/1hfdEcKpfhHoqkdUAyF4QQ, db `decisiones`): D-PLAN-7 = A, 8 = A,
**9 = B** (en S10, leer gigs y enemigos desde JSON), 10 = A (el arte de las larvas ya está), **11 = B** (público
con escudo antes de la demo), 12 = A, 13 = A, 14 = A, 15 = A, 16 = A (los números ya están), 17 = A, 18 = A,
19 = A. Y dos más al final del día:
- **D-PLAN-20 (Inspiración como el Heroísmo de Midnight Suns, prioritaria):** toda carta de composición genera
  Inspiración y todo Finisher la consume; convención y valor por defecto al crear cartas. Por defecto, **una vez al
  jugarla** (`inspirationGenerated`, con `complexityMultiplier` a 0 para no tocar el LoopScore); algunas cartas
  dan **por loop** (`AddInspirationPerLoopSpec`), y esa carta queda fuera de la demo o es la recompensa del gig 2.
  Diseño: `reference/ideas/2026-11-12_inspiracion_heroismo.md`.
- **D-PLAN-21:** el arte nuevo de las cartas (marcos, estrella ★N, ilustraciones por instrumento) va en una
  **sesión propia, CARTAS-ARTE-1**, justo después de ECON-2.
- Correo para el botón de feedback (S9c): **allwtttcmm@gmail.com**.

La tabla de abajo conserva las propuestas tal como se presentaron.

**Cuánto hueco hay (✦ IA, 12-nov).** Hechas S1–S8 y S4b, dos semanas antes de lo previsto; S9 está en curso.
Hasta la congelación del 27-nov quedan unas 6 sesiones (a 3 por semana: 16–20 y 23–27, más lo que quede de
esta semana) para S9, S9b, S10, S11 y S12. **Sobran 1 o 2 sesiones**, no más. Por eso abajo se proponen dos
sesiones nuevas y el resto se manda a después de la demo. Si se quiere más alcance, se recorta, no se añaden horas.

| # | Qué | Propuesta | Recomendación |
| --- | --- | --- | --- |
| D-PLAN-7 | **Arreglos rápidos de la reunión** | Sesión nueva **S9c, DEMO-FIX-1** (semana 16–20, tras S9b): (a) Doble Armonía fuera del mazo inicial de Zig (está rota y su canto a dos voces es de después); (b) carta de Sibi que cura Stress a un músico, en lugar de Read the Room en su mazo inicial (Read the Room pasa al RewardPool o se aparca; cierra D9); (c) botón de feedback (correo de ALWTTT + enlace al canal de Discord + «abrir carpeta de telemetría»); (d) build: *Company Name* propio y Dev Mode (F12) oculto en la build pública, con la telemetría encendida | **A: entra.** Todo es datos o UI pequeña y cada punto quita una confusión de la prueba M6. Falta el correo de ALWTTT. `mailto:` no abre nada si el jugador no tiene cliente de correo: por eso el correo se muestra y se copia, además de abrirse |
| D-PLAN-8 | **Combos y jugar pensando** | Sesión nueva **COMBOS-1** (semana 23–27, antes de S12): catálogo corto de combos con nombre (5–8) que salen en la previsión del Big Num (`reference/ideas/2026-11-05_combos_de_banda.md`) y una prueba de coste o tope en las cartas de composición (solo datos) | **A: entra**, y en el orden de recorte justo después de las poses. Es lo que Matías más destacó, y el «se juegan todas» se nota en la prueba M6. Alternativa B: solo la prueba de coste (media sesión); C: después de la demo. **✅ Hecho en COMBOS-1 (2026-11-19); el coste quedó fuera (D5)** |
| D-PLAN-9 | **Herramientas de testeo en la build** (cartas, enemigos, gigs; guardar y exportar JSON) | Sub-roadmap propio **después de la demo**; antes, como mucho, que TEST-SETUP-1 (S10) lea gigs y enemigos de JSON de una carpeta (`reference/ideas/2026-11-12_herramientas_de_testeo_en_build.md`) | **A: después de la demo**, con el diseño escrito ahora. Son tres editores en runtime (los ScriptableObjects no se crean en la build) y el criterio de la demo no los necesita. B (solo lectura de JSON en S10) solo si S10 va holgada |
| D-PLAN-10 | **Larvas de la Reina** (Matías: lloran fuerte, mucho Stress, poca Vibe, gustos distintos a la Reina → priorizarlas) | Entran en S9 (o en una sesión corta antes del 27-nov) **si el arte llega antes de la congelación de arte (20-nov)**; si no, la Reina va sola | **A.** El diseño es bueno y encaja con lo que hay (Vibe, gustos por miembro, objetivo al más débil); sin arte se vería a medias. Pedido #14 |
| D-PLAN-11 | **Público con escudo** (defensa que no sea bloqueo ni inmunidad) | Después de la demo: pasar el escudo temporal (`Composure_TempShieldTurn`, hoy solo de músicos) al público, comprobando que absorbe Vibe | **A: después de la demo.** Pide código y balance nuevos; la demo ya tiene tres encuentros distintos |
| D-PLAN-12 | **Variedad musical** (Matías ya oyó casi todas las combinaciones) | Sesión nueva **MUSICA-1** antes del 27-nov: integrar y validar en el juego lo que ya está hecho (fills y acentos de batería, bajo que sigue los acentos, más variedad por jugada si cabe el paso de los niveles con semilla a C#), con una prueba de oído en el juego. Alcance exacto en su arranque | **A: entra.** Claudio la llama vital y casi todo ya existe; falta encenderlo y oírlo. Ver la cuenta de sesiones abajo |
| D-PLAN-13 | **Sala de ensayo como taller en el juego** (jugar cartas libremente y oír combinaciones) | Después de la demo, partiendo de `2 - Ship Interior` (ya es un ensayo con composición). Primera pieza de las herramientas de testeo (D-PLAN-9) | **A: después de la demo.** Es una escena entera; en la demo, la sala de ensayo sigue «EN DESARROLLO» |
| D-PLAN-14 | **Cartas en el último loop** | A: cartas **Finisher**, solo jugables en el último loop (las de más Inspiración; el roster ya tiene el concepto, D-R0-12). B: cualquier carta en el último loop, con la pista entrando en la siguiente barra (segundo reproductor, como SONG-TAIL-1) | **A en COMBOS-1** (el finisher cierra el combo), **B a investigar después de la demo**: toca la misma frontera de audio que S9b |
| D-PLAN-15 | **Tooltips más cortos** | Por defecto, solo lo esencial; las fichas de conceptos (Stress, Vibe, Cohesión…) al mantener una tecla. Entra en S11 (pulido). Ya había diseño: `ideas/2026-11-12_tooltips_basales.md` (marca `isBasic` en el glosario, Alt para verlas) | **A.** Es texto y un interruptor; ayuda a la prueba M6 |
| D-PLAN-16 | **Números de Matías y posiciones de la banda** | Números: *Sprite Asset* de TMP en S11 si los archivos llegan antes del 20-nov (pedido #17). Posiciones: ajustar las formaciones en S9c | **A.** Las dos cosas son de datos |

| D-PLAN-17 | **Menú de ajustes en el juego (F5)**: cartas por turno (una / una de cada tipo / sin límite), robo al empezar y entre loops, máximo de la mano (y mostrarlo, «8/8» como Balatro), inspiración, sin límite de canciones, vida tras el colapso; guardar y valores por defecto; extensible con una línea por ajuste | Sesión nueva **DEV-TUNING-1**, **antes de COMBOS-1**: con el menú se prueban las variantes de economía (el «se juegan todas») sin tocar código, y Matías puede probar en su build. Diseño: `ideas/2026-11-12_menu_de_ajustes_dev.md` | **A: entra, y pronto.** Abarata COMBOS-1 y todo el balance. Pide separar build de testers (con F5/F12) y build pública |
| D-PLAN-18 | **Motor reutilizable** sacado de ALWTTT | Después de la demo, y solo cuando arranque un segundo juego que lo use (mundo-islas es el candidato). `ideas/2026-11-12_motor_reutilizable.md` | **A: después, guiado por el segundo juego.** Extraer sin un segundo consumidor suele dar abstracciones equivocadas |
| D-PLAN-19 | **Cómo se estructura el plan**: no todo cabe en la demo, pero todo queda registrado | Hasta el 27-nov, las sesiones en el orden de abajo; lo que no llegue pasa sin perderse a `POST-DEMO_Plan_de_Sesiones.md` (borrador por bloques: herramientas, música, combate, mundo, motor, deuda) | **A.** Ver «Orden de entrada» abajo |

### Orden de entrada hasta la congelación (D-PLAN-19, definitivo el 12-nov)

Desde el 12-nov, **este orden manda sobre la tabla «Sesiones»** para lo que queda hasta el 27-nov. No hace falta
que quepa todo: se trabaja en orden y, el 27-nov, lo que no haya entrado pasa a `POST-DEMO_Plan_de_Sesiones.md`.

| Orden | Sesión | Qué | Si no llega |
| --- | --- | --- | --- |
| 1 | **S9 REINA-1** ✅ cerrada 2026-11-12 | La Reina con huevo · huevo · eclosión en la Colmena; las larvas, como **reloj** junto a ella (D8=A). **Las larvas como público** (D-PLAN-10) quedan sin hacer: sesión corta propia si se quieren antes de la demo | — imprescindible |
| 2 | **S9b AUDIO-LOOP-1** ✅ cerrada 2026-11-13 | Que nada se corte entre loops. Hecho: encadenado sin `ClearAllVoices` cuando el loop anterior terminó solo, release de seguridad a los 3 s, parche local a Maestro (`reference/maestro-parches.md`). Queda un hueco pequeño y ocasional → AUDIO-LOOP-2 en POST-DEMO B7 | — imprescindible |
| 3 | **S9c DEMO-FIX-1** ✅ cerrada 2026-11-14 | Hecho: Zig con dicción (preset «Back~a»); **Calm Down** de Sibi (−4 Stress a un músico) en lugar de Read the Room (cierra D9); Doble Armonía fuera; formaciones de 2/3/4 por local (Ziggy delante en el cuarteto) y cuatro huevos-reloj en la Reina; «Abrir carpeta de telemetría»; Company Name «ALWTTT Team»; fichas de conceptos solas 2 veces y luego con Shift; aviso al hacer build con ajustes de prueba. El feedback por `mailto:` no funcionó en el PC de Claudio: apagado → **FEEDBACK-2** (3b). Nodo aleatorio → S10 | — imprescindible |
| 3b | **FEEDBACK-2** ✅ cerrada 2026-11-14 | Hecho: panel en la pausa con cuadro de texto, «Enviar» y «Cancelar», que el juego manda por HTTP al formulario de Google con versión, sistema, escena, banda y gig (D1=B; el resumen de la telemetría quedó fuera); línea de privacidad; sin secretos en la build. Si falla, el texto se queda y «Copiar» da el respaldo de correo de S9c (D3=B). Esc cierra el panel; M y D no se disparan al escribir. Panel por menú de editor (D4=B), ES/EN (D5=A). Smokes 1–3 jugados por Claudio | se queda el `mailto:` |
| 4 | **ECON-2: la Inspiración** ✅ cerrada 2026-11-14 | Hecho: las composiciones dan ★1 **una vez al jugarlas** (pop dorado en el contador y sobre el músico; previsto = real); el **Finisher** es una carta de composición que solo se juega en el último loop, paga ★3 y añade **un** loop final donde suenan todos los Finisher jugados (solo de su músico; C2, sin solo melódico, hace un «break» con su batería) y estallan sus efectos. Overload y Psychic Waves pasan a Finisher; nuevos Gran Final (Zig) y Redoble Final (C2). Sin ★ por loop, máximo 10, Voltaje sin cobrar hasta la cara nueva; «Mantén la Calma» → «Te Cubro». **Todo ajustable en la build: F12 ▸ Inspiración** (guardado por máquina, para Matías). La carta por loop queda fuera de la demo | — imprescindible |
| 4b | **VOZ-3 + COMP-LOG-1** ✅ cerrada 2026-11-16 (sesión extra) | Hecho: estilos del Taller de melodías al juego (TV1, importador, «▶ con voz» en el Banco); **la demo sale con IL2CPP** (voz v8 8–9 % de CPU frente a 14–15 % con Mono, D9 = A); arreglos de ECON-2 (solo de Psychic Waves vacío, fila «Solo · <Finisher>»); log de composición por ejecución. Abierto para antes de la demo: **el Walking Bass que se calla** tras un par de notas (compone 32 notas; primer uso de COMP-LOG-1) y el solo de Sibi que se oye poco | — |
| 5 | **CARTAS-ARTE-1** ✅ cerrada 2026-11-17 | Hecho: la cara de Matías en tres marcos (el del Finisher, el neutro en cian hasta que llegue el suyo); coste ✦ arriba a la izquierda y «+N» grande; rótulo de la pista arriba; las 19 composiciones con ilustración a toda la carta, nombre abajo y solo los modificadores que leen los gustos; efectos en las fichas, a la derecha de la carta; degradado y candado para lo no jugable. Voltaje sin cobrar (D2 = B). Pedidos #18–#21 a Matías. Clic derecho y Voltaje → CARTAS-ARTE-2 (POST-DEMO D7) | — |
| 5b | **BATERÍA-SOLO-1** (MidiGenPlay; decidido por Claudio el 2026-11-14) — **construida 2026-11-17, falta el oído** | Un solo o *break* de batería de verdad para el Finisher de C2 (Redoble Final), con los fills de MGP-DRUM-FILL-1. Hecho: `TrackParameters.DrumSolo` en el render del loop final, tres formas y cinco caracteres, F12 ▸ Inspiración y casilla en el Banco de cartas (ALWTTT `2dbc473`). Falta: suite, smokes y oído de Claudio (pasos en `MidiGenPlay/.../ARRANQUE_bateria_solo.md`) | se queda el «break» de ECON-2 |
| 6 | **DEV-TUNING-1** ✅ cerrada 2026-11-17 | Hecho: F5 ▸ «Ajustes» (jugadas por turno: una de cada tipo / una en total / sin límite; robo al turno, al Play y entre loops; máximo de la mano; ★ inicial; vida tras el colapso; sin límite de canciones), guardar en `DevTuning.json` y valores por defecto, registro de una línea por ajuste; **F6 salta la canción** con puntuación real; contador «N/M» de la mano; telemetría con `factory`/`tuning`. Página para testers https://claude.ai/artifact/VCSqZeSsWnZvCFUHgUoxgv. Separar builds → S12 (D5 = B) | — |
| 7 | **COMBOS-1** ✅ cerrada 2026-11-19 | Hecho: una mano por loop según quién entra (¡TODA LA BANDA! +3, o SECCIÓN RÍTMICA y A MEDIA LUZ +2), **Capas** ×1,5 por loop con entrada nueva y «GRAN CIERRE» en el loop del Finisher; previsto = real también **antes de Play**; contornos naranja, absorción al Big Num y tabla con la C; mano inicial separada (4) del robo por turno (2), en F5. D5: sin coste ★1. **A POST-DEMO:** la canción hasta el Finisher (D9 = A, `reference/ideas/2026-11-19_cancion_hasta_el_finisher.md`), F5 para canciones sin límite de loops (D9 = B) y combos por relación (D6). Smokes A–C PASS | — |
| 8 | **MUSICA-1** ◐ parcial 2026-11-20 (D-GF = A: Zig canta el Gran Final; solo de Sibi con Saw Wave; solistas a 1, banda a 0,55; F5 compases por loop; vida del público ×1,5; vuelta al mapa. Smoke 1 PASS. Lo demás → **MUSICA-1b** detrás de S10, `ARRANQUE_MUSICA-1b.md`; PUB-SHIELD-1 no cabe → C1) | **D-GF (2026-11-17, por decidir; recomendado A):** el solo de Gran Final (Zig) suena con soundfont encima de la voz; A = la voz canta el solo, B = dejarlo, C = DUET-1 después de la demo. Encender y oír fills, acentos y el bajo que escucha. **Desde el 2026-11-13 es la puerta de la tanda extra de MidiGenPlay** (ALWTTT va adelantado): lo que esté listo antes del 27-nov (pruebas repetibles, melodías por figura si llegan) se enciende carta a carta con prueba de oído; lo que no suene bien se queda apagado. Opciones en `MidiGenPlay/.../Sesiones_MidiGenPlay.md` («Tanda extra»). **2026-11-17, pedido de Claudio: incluye una pasada por todas las cartas** (escucha, BPM de cada carta y rangos de tempo de sus patrones) con el Banco de cartas, que ya muestra de dónde sale cada pista (↳), la tabla de la batería y un BPM forzado; antes, PATRON-BPM-1 en MidiGenPlay | pasa a B |
| 9 | **PUB-SHIELD-1** | Público con escudo: el escudo temporal pasa al público y absorbe Vibe (D-PLAN-11 = B) | pasa a C1 |
| 10 | **S10 ALINEACIÓN-2 + TEST-SETUP-1** | Las 3 alineaciones sin diálogos de ausentes; GigSetup desde el menú; **gigs y enemigos desde JSON** (D-PLAN-9 = B); **nodo aleatorio del mapa** (50 % Tentáculos, 50 % Ranas y Rayas, con semilla; pasado desde S9c, D5=B) | — imprescindible |
| 11 | **S11 PULIDO** | Recompensa con arte, tooltips basales, números dibujados (listos); MAPA-2 solo si sobra | MAPA-2 a D1 |
| 12 | **S12 Congelación** (vie 27-nov) | Build de testers (con F5/F12) y build pública, **con IL2CPP** (VOZ-3 D9) | — imprescindible |

**Hecho fuera de orden (2026-11-13, desde la sesión 2a bis de MidiGenPlay).**

- **DEV-TEST-1:** cartas TEST con su propio bundle, que entran en la mano al empezar el gig o en cada turno.
- **DEV-HAND-1:** mazo fijo desde un JSON en `GigDevSettings`, para pruebas controladas.

Las dos son herramientas de F12 (`SSoT_Dev_Mode.md` §20). Adelantan una parte de DEV-TUNING-1 y la «prueba controlada» de TEST-SETUP-1, pero el menú F5 sigue pendiente.

También avanzó MUSICA-1: Claudio oyó en el juego, con las cartas TEST, **el bajo que toca frases sobre la batería** (MGP-BASS-PHRASE-1).

**Cuenta honesta (✦ IA).** Hasta el 27-nov caben unas 7 sesiones a 3 por semana, y los imprescindibles son 6
(1, 2, 3, 4, 10 y 12): lo previsible es que entre uno más, probablemente CARTAS-ARTE-1, que hace legible la
economía nueva. Lo demás pasa a después de la demo, en su bloque. Si se quiere más, se recorta alcance dentro de
las sesiones (por ejemplo, S10 sin el JSON), no se añaden horas.

**Cartas nuevas de Matías:** pedirle la lista (pedido #15). Las que se puedan hacer con primitivas que ya
existen entran como datos antes del 27-nov (en COMBOS-1 o DEMO-FIX-1); las que pidan mecánica nueva, después.

**Telemetría:** funciona, solo en local (un JSONL por día, una línea por gig). Detalle y rutas en la minuta.

## Orden de recorte si vamos tarde

Se recorta de arriba abajo; nunca M1, M3, M6:
1. **Poses de enemigos** (fuera del plan; cambiar de sprite por estado, ~1 sesión, si sobra).
2. **PULIDO-ARTE + MAPA-2** (S11): queda la recompensa de siempre; el mapa sigue en línea recta (tres gigs y la Reina) y el editor de giras se aplaza.
3. **GRITOS-1** (S8): solo frases genéricas, sin `{músico}`.
4. **ALINEACIÓN** (S6, S10): banda fija; el dormitorio pasa a «EN DESARROLLO».
5. **NAVE-1** (S5): la cabina se queda; las demás salas, una sola pantalla con el plano.

## Deseos (no entran salvo que sobre tiempo)

- Poses por estado del público (solo el secuaz 1 las tiene).
- Chatarrero funcional: «abre el show», la mejora inicial de la partida (pantalla del bicho en el foco).
- Interacciones musicales, convencidos que cantan, «boooring» entre loops (REPLAN-ART D3 = B).
- **Orden de sprites, automatizado si se puede** (Claudio, 2026-11-03): renombrar y ordenar los PNG
  nuevos de `Resources/Sprites/Characters/Audience/` y `Sprites/Ship/` (nombres provisionales, sin
  commitear). Encaja antes de ELENCO-1 (S7), que es quien los usa.
- **Canciones por gig según el gig, el local y el público** (Claudio, 2026-11-03). Hoy la demo usa
  3 canciones y 3 loops por parte para todos los gigs (`DemoLaunchConfig`); valores de prueba.

## Decisiones (cerradas por Claudio el 2026-10-30)

- **D-PLAN-0 = A:** la demo es el esqueleto del juego: nave como centro y salas «EN DESARROLLO».
  Enmienda REPLAN-ART D1.
- **D-PLAN-1 = A:** la alineación (2, 3 o 4 músicos) se elige en el dormitorio, con el arte
  «Personajes». La pantalla del bicho en el foco queda para el chatarrero (mejora inicial, deseo).
  Resuelve el abierto «qué dos músicos abren la demo» de REPLAN-ART: lo elige quien juega.
- **D-PLAN-2 = A:** entran como «EN DESARROLLO» todas las salas que tienen arte.
- **D-PLAN-3 = B:** público con rol solo con datos + objetivo visible en la intención + gritos; los
  disparadores, Phase C.
- **D-PLAN-4 = A:** 3 sesiones por semana; si son menos, se recorta por el orden de arriba.
- **D-PLAN-5 (2026-11-06):** el grito «Tu papá es travesti» del Miró queda fuera de GRITOS-1 (S8): es
  burla a una identidad y la demo es pública.
- **D-PLAN-6 (2026-11-12, S7):** enmienda REPLAN-ART D1 («sin ramas»): entra **una** bifurcación en el
  mapa (MAPA-2), porque añade rejugabilidad y es una base reutilizable del diseño (Claudio). Va en el
  hueco de S11 en lugar de «escenario por nodo» (D4=B), para no mover la congelación del 27-nov. Ver
  `reference/ideas/2026-11-12_bifurcacion_y_editor_de_giras.md`.

## Página para el equipo

Wrapped privado del plan (compartir desde su menú Share): https://claude.ai/artifact/GmUTRzr7FeofzvgeRxUsv4.
Si este archivo cambia, actualizar también la página. Fuente: `reference/plan-wrapped/index.html` (desde el 12-nov).

## Pendiente de arte (pedir a Matías)

Archivos sueltos de: pantalla «Personajes» (dormitorio), «Escoge tu carta», pantalla del bicho en el foco
y, si existen, las salas que faltan (cabina, dormitorio, cocina, jardín, baño). La Reina va **con larvas**
(reunión del 12-nov): arte de la eclosión y de las larvas (pedido #14), y la lista de cartas nuevas de Matías
(#15). Lista viva: `reference/reuniones/Pedidos_Arte_Matias.md`.

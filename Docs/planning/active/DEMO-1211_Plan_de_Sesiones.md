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
| | S6 | **NAVE-2: dormitorio = alineación** | En el dormitorio se elige la banda: 2, 3 o 4 músicos | Cada opción arranca con esa banda y su mazo; sale en la telemetría | M2 |
| 16–20 nov | S7 | **ELENCO-1** | Los 9 sprites listos entran como público; gigs 1 y 2 con sus grupos (`reference/ideas/2026-10-30_publico_con_rol.md` §7) y el objetivo «el líder» | Los dos gigs tienen público propio que se ve distinto y actúa distinto | M4 |
| | S8 | **GRITOS-1** | Bocadillos con frases por habilidad; `{músico}` se sustituye por el objetivo | Antes de cada golpe se lee quién lo recibe; alguien se ríe | M1 |
| | S9 | **REINA-1** | Reina Gusano con su sprite y huevo · huevo · eclosión | En la prueba, alguien explica qué hace la Reina sin ayuda | M4 |
| 23–27 nov | S10 | **ALINEACIÓN-2 + TEST-SETUP-1** | Tutorial y mazo coherentes con cada alineación; GigSetup desde el menú | Las 3 alineaciones juegan el gig 1 sin diálogos de músicos ausentes | M2 |
| | S11 | **PULIDO-ARTE** | Recompensa con el arte de «Escoge tu carta», escenario por nodo, mapa polinizado al vencer a la Reina | Se ve en una partida completa | M5 |
| | S12 | **Congelación** (vie 27-nov) | Build interna + guion de la prueba M6 | La build arranca desde un `.exe` limpio | M7 |
| 30 nov–4 dic | S13–S15 | **Prueba M6** | ≥3 personas externas sin explicar; se mira dónde se atascan y se les pide que cuenten cómo se gana y cómo se pierde | Cada una lo sabe decir; solo se arreglan bloqueantes | M6 |
| | | Build candidata (vie 4-dic) | | | M7 |
| 7–11 dic | S16 | **Discord** | Texto, capturas, publicación (vie 11-dic) | Publicada | M7 |

**S4b insertada el 2026-11-10** (D8=B de S4): las vistas al pasar el ratón salen de S4 para que S4 cerrara con la vista en reposo probada. Empuja S5–S6 media semana; si no cabe, S4b se recorta a «iconos legibles + hover del músico».

**Abierto tras S4b (2026-11-11):**
- **D9, qué deja el colapso.** Desde S4b el colapso aplica `exposed` (configurable en `MeterTuningSO`); en la demo no hay carta que cure Stress (`starter_take_five` existe sin mazo). Recomendado: mantener Expuesto y decidir tras jugarlo; alternativa, meter Take Five en un mazo. Idea a largo plazo: «Nervioso» (`reference/ideas/2026-11-11_nervioso.md`).
- ~~D10, el público que golpea a toda la banda~~ → resuelta (C): un icono «×N» con el aro partido en los colores; caras apiladas para 2–3.
- **F-TIP-7:** el hover de conceptos del tutorial puede apilar fichas (sin comprobar).
- **Iconos de objetivo con contorno** en lugar del disco: a Claudio «no le convence» el disco; no urgente.

**Abierto tras S5 (2026-11-11):**
- `vendedor.png` está duplicado (idéntico) en `Sprites/Ship/` y `Sprites/Ship/Rooms/`; `Rooms/` sin commitear (la nave usa `Rooms/chillout`).
- `nave espacial.png` (31 MB) está en `Resources/` y entra en la build; la nave ya no la usa.
- ESC en la nave vuelve al menú (UIManager); sin cambio.
- `CohesionPipsView` duplica `GigCanvas.SetCohesion`; unificar después de la demo.
- Idea a probar: nombres de las salas siempre visibles, tenues (referencia: corte de casa grecorromana), si alguien no descubre el hover en la prueba M6.

## Orden de recorte si vamos tarde

Se recorta de arriba abajo; nunca M1, M3, M6:
1. **Poses de enemigos** (fuera del plan; cambiar de sprite por estado, ~1 sesión, si sobra).
2. **PULIDO-ARTE** (S11): queda la recompensa de siempre y un solo escenario.
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

## Página para el equipo

Wrapped privado del plan (compartir desde su menú Share): https://claude.ai/artifact/GmUTRzr7FeofzvgeRxUsv4.
Si este archivo cambia, actualizar también la página.

## Pendiente de arte (pedir a Matías)

Archivos sueltos de: pantalla «Personajes» (dormitorio), «Escoge tu carta», pantalla del bicho en el foco
y, si existen, las salas que faltan (cabina, dormitorio, cocina, jardín, baño). Si la Reina va sola o con
larvas.

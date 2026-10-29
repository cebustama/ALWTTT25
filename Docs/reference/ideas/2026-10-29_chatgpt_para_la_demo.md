# Lo que el historial de ChatGPT dice de la demo — 2026-10-29

**Tipo:** referencia (registro, no autoridad). Si algo de aquí cambia el plan, el cambio va en `Docs/planning/active/S5_DemoCutClose_Sub_Roadmap.md`. Complementa `2026-10-29_desde_chatgpt.md` (ideas para después).
**Fuente:** historial de ChatGPT de Claudio (`D:\Projects\chats-chatgpt`, fuera del repo). El número entre paréntesis es el `i` de su `catalogo.csv`. Lo citado está comprobado en los mensajes de Claudio; lo marcado *Inferido* es lectura del agente.

Ordenado por los puntos M1–M7 de REPLAN-DIAG.

## M1 · Legibilidad (LEGIB-1)

- **La previsualización ya existió.** En octubre de 2024 (709, 726) el prototipo tenía `Card.Project()`: devolvía un diccionario efecto → valor que el `GigManager` emitía por evento, y `MusicianUI.GigManager_OnPreviewCard` mostraba la probabilidad de éxito y los atributos del músico, más el estrés en su barra y el aumento de engagement al arrastrar la carta sobre él. *Comprobado en el repo:* el código actual no lo tiene (el gig se reescribió sobre NueDeck en julio de 2025). La idea de calcular el efecto sin aplicarlo y emitirlo por evento sirve tal cual para D-DIAG-2.
- **Playtest real (1089, 2025-03-03):** amigos confundidos con la barra de engagement, que podía ser negativa o positiva. «It seems the bar represents, more precisely, the mood of the audience, not the engagement. There should be clear visual signs of the current "score" for the gig».
- **Polaridad confusa (2718, 2026-01-14):** «Los buffs son negativos para la banda… (se que es confuso)». *Inferido:* en la UI, marcar cada estado por a favor / en contra de la banda (color, icono), no por buff / debuff.
- **Todo pasa a la vez (2196, 2207, 2025-11-24):** al terminar la canción, el pulse de cada enemigo sube a la vez que atacan, «no hay un delay… simplemente es todo instantáneo». La propuesta de entonces: separar la puntuación por loop del hype visible y dar tiempo de lectura entre las acciones del público.

## M2 · Banda de 2 con un rol claro

- **Nombres de rol ya pensados (2657, 2026-01-10):** «roles tipo arquetipo, es decir, the Frontman, the Pulse, the Engine (u otro pero sirve xq es un robot) y the Ambience u otro más temático para Wormy (gusano psíquico)». *Comprobado en el repo:* no aparecen en `Docs/`. Para una banda de 2, dos de estos nombres en la UI dicen el rol sin explicarlo.
- **Regla de alcance (2592, 2026-01-03):** «una sola idea para cada personaje… La prioridad sigue siendo un par de cartas por personaje (al menos 3 de acción y 3 de composición), con nombre, efecto y poder jugar un concierto completo».
- **Reparto de instrumentos (2433, 2025-12-17):** el Conito al bajo, con su carta de composición como una línea de slap; el Gusano a los sintetizadores, con la progresión de acordes.

## M3 · Cohesión como única derrota

- **La cohesión cambió de papel.** En septiembre de 2024 (607): «Cohesion will play no part during the gigs». Hoy es la única derrota. *Inferido:* los textos y docs antiguos que hablen de cohesión pueden contradecir la demo; vale una búsqueda antes de congelar.
- **Cómo mostrar la penalización (1811, 2025-09-16):** ya pediste el formato «Conflicts = 1 (25%, 1 Cohesion/Conflict)». El mismo patrón (causa, probabilidad, coste) sirve para el aviso de cohesión.
- **Espirales (1050, 2025-02-18):** «Si el jugador obtiene bajo engagement y eso genera mayores consecuencias mientras más bajo, sería casi imposible remontar». *Inferido:* con una sola derrota, comprobar en la prueba que una mala canción no lleve a perder sin salida.

## M6 · Prueba de comprensión

- **Una batalla de ejemplo (2208, 2025-11-26):** ya la pensaste como tutorial: tipos de carta, cómo el loop score se convierte en hype (con el loop score oculto y la barra visible), cómo el resultado da vibe a cada miembro del público, sus habilidades y la siguiente canción.
- **El criterio era el mismo en 2024 (701):** «It can be complex only if its well explained and provides an interesting emergent experience».
- **Recogida de feedback (1194, 2025-04-10):** el plan de la demo de abril de 2025 incluía un bot de feedback en Discord y una tecla para enviar feedback desde el juego a un correo propio. Para M6 basta con las 3 personas y sus dos respuestas (cómo se gana, cómo se pierde), pero la tecla de feedback sirve para la demo pública.

## M7 · Build y texto de Discord

- **Hosting (1194):** la pregunta «¿Google Drive con el .exe o qué es lo estándar?» quedó abierta en 2025.
- **Plan de 2026 (2731, 2026-01-15):** publicar en 2027 y usar este año para «construirlo, probarlo y pulirlo, muchas veces, intentando armar una comunidad en el discord». La demo del 11 de diciembre es el primer paso de ese plan.

## Calendario: cuatro demos antes que esta

| Fecha | Demo | Lo que pasó |
| --- | --- | --- |
| sep–oct 2024 | Prototipo para publishers tras Gamescom 24 (646, 650, 673) | Dos semanas de retraso avisadas por correo; presión «un poco autoimpuesta». |
| 26 abr 2025 | Primera demo pública: 15 cartas, 3 públicos (1163, 1194) | A un mes, 6 cartas diseñadas de las necesarias para 3 mazos. |
| ago 2025 | Gamescom 25 (1336, 1401, 1590, 1597) | Rediseño grande a dos meses; plan de 18 días; mucha ansiedad la semana antes. |
| sep 2025 | Demo del Compose en la nave de ensayo (1832) | Lista de tareas para tres días. |

*Inferido:* en las cuatro, el contenido (cartas, mazos) y los cambios de diseño llegaron tarde. La congelación del 27 de noviembre y la lista cerrada M1–M7 son justo lo que faltó entonces: protegerlas es la lección más útil del historial.

Mode: IMPLEMENTATION. S9 · REINA-1 (DEMO-1211) — la Reina Gusano: huevo · huevo · eclosión.

Escrito el 2026-11-12 al cerrar S8. Planning, no autoridad. Fila S9 de `DEMO-1211_Plan_de_Sesiones.md`.

**Context.** S8 (GRITOS-1) cerró el 2026-11-12: el público grita a quién va el golpe (bocadillo al
actuar, `{musico}` = la flecha, el golpe espera), con frases en `ConceptGlossarySO.shouts` (también
por músico), viñeta a pantalla por habilidad (`shoutSplash`, una vez por gig), poses
`pendingSprite`/`actingSprite` y Conito que baila sobre su flotado. La Reina hereda todo eso. Hoy el
último nodo del mapa lineal es el jefe (`isFinalEncounter`: perder → Game Over sin Retry, ganar →
WinPanel), pero la Reina no tiene datos propios: falta el personaje, su patrón y su encuentro.

**Decisions locked at batch open.**
- Restricción M4 (Keep #113): 1–2 mecánicas legibles, sin quitar poderes al jugador.
- Patrón fijo **huevo · huevo · eclosión** (`reference/ideas/2026-10-30_publico_con_rol.md` §6): se lee
  como la cuenta de Tentáculos («cuando llega a 3, cuidado»).
- Los gritos, la viñeta y las poses van por el JSON del elenco (`SSoT_Editor_Authoring_Tools` §23.5).
- D-PLAN-5 vigente: nada de burlas a identidades en los gritos.

**Open at batch open** (formato de decisión).
- D1. Qué da cada huevo: ¿resistencia (`BlockVibe` sobre sí misma), Exaltado (Hyped, como Tentáculos)
  o las dos? Debe verse en la intención y en su número.
- D2. La eclosión: ¿Stress a toda la banda (`AllMusicians`) o al más débil? ¿Cuánto, con los números
  de los otros jefes?
- D3. «La colmena llama a Sibi»: ¿solo gritos a Sibi (variantes `byMusician`, sin código) o también
  más Stress a Sibi si está en la banda (objetivo nuevo o regla de datos)?
- D4. ¿Va sola o con larvas? Pendiente de Matías (plan, «Pendiente de arte»). Si no hay arte, sola.
- D5. Vibe máxima y gustos de la Reina (que se pueda convencer en la demo con un mazo normal).
- D6. Viñeta de la eclosión: ¿hay arte o se usa el sprite `jefe (1).png` como viñeta provisional?

**Inputs (leer del disco).**
1. `Docs/planning/active/DEMO-1211_Plan_de_Sesiones.md` (fila S9, S9b, «Pendiente de arte»).
2. `Docs/reference/ideas/2026-10-30_publico_con_rol.md` §5–§6 y `2026-11-06_desde_miro.md` (motivo
   de la Reina: la colmena trae artistas para estimularla y poliniza el lugar).
3. `Docs/systems/SSoT_Audience_and_Reactions.md` §7.2–§7.4 y §8 (objetivo, Jefe, coro, gritos).
4. `Assets/Settings/Audience/Elenco_S7.json` y `Assets/Scripts/Characters/Editor/AudienceJson.cs`
   (Tentáculos es el modelo: `ab_count_*`).
5. `Assets/Scripts/Map/LinearMapController.cs` y el launch config del nodo final (qué encuentro usa hoy).
6. `Assets/Resources/Sprites/Characters/Audience/jefe (1).png` (la Reina) y `Docs/reference/reuniones/Pedidos_Arte_Matias.md`.

**Task.**
1. `/buscar-referencias --all` sobre jefes de deckbuilder con patrón fijo y cuenta atrás (Slay the
   Spire, Monster Train) y jefas «madre de la colmena».
2. Cerrar D1–D6 con el formato de decisión.
3. La Reina en el JSON: personaje, prefab variante con su sprite y altura (Lineup), habilidades
   `ab_egg` ×2 + `ab_hatch`, textos ES/EN, gritos (con variantes para Sibi), viñeta si la hay.
4. Encuentro «La Reina Gusano» y su launch config; el nodo final del mapa apunta a él.
5. Que la intención cuente los huevos (como Tentáculos) y que la proyección de la eclosión acierte.
6. Logs `[S9]`: huevo puesto (n), eclosión (objetivos y Stress), grito a Sibi.

**Constraints.** Claude Code no corre Unity; no escribir `.meta`; no tocar MidiGenPlay; español,
registro GDC; previsto = real; poses y gritos en el JSON (el menú S8 vacía lo que no esté);
tutorial-artefacto si el montaje es largo; sin commit ni push sin pedirlo.

**Deliverables.** Datos y código mínimo; smokes con sus líneas `[S9]`; propuesta de docs (Audience §8,
Editor Tools §23, plan) sin aplicar.

**Closure exit criteria.** En la prueba, alguien explica qué hace la Reina sin ayuda (criterio del
plan); la Reina es el último nodo del mapa y ganarle lleva al WinPanel; smokes jugados por Claudio;
docs aprobados y aplicados; commit cuando Claudio lo pida; `proyectos.md`, `foco.md` y la página de
frentes al día; arranque de la siguiente (S9b AUDIO-LOOP-1 si no se hizo antes, o S10).

## Referencias de la sesión (`/buscar-referencias --all`, 2026-11-12)

- **Monster Train, Arkion** (`C:\Users\cebus\Dropbox\ALWTTT References\Pre-Gig Screen 1.png`): el jefe se
  presenta en una frase y su ficha dice «Rage +2 por stack, baja cada turno». Por eso gana de 2 en 2: con 1
  el contador nunca sube. Base de D1 (Exaltado +2 por huevo).
- **Monster Train, Seraph el Entrópico** (`…\Enemies\Screenshot 2025-08-04 200409.png`): cada habilidad del
  jefe en su propia ficha, con nombre y una línea. No se toma la carta maldita al mazo (quita poderes, Keep #113).
- **Ficha de Sibi** (`…\Characters\Sibi.jpeg`): especie con estructura de colmena y reina; «sigue siempre la
  voluntad de la colmena, para bien y para mal». Respalda los gritos a Sibi (D3) y desaconseja castigar a
  Sibi con más Stress: es el apoyo y la calma de la banda.

## Decisiones cerradas al abrir (Claudio, 2026-11-12)

- D0=A: el gig de la Reina dura 4 canciones (el público no actúa tras la última; con 3, la eclosión nunca llegaba).
- D1=C: cada huevo da Exaltado +2 (baja 1 por turno) y recupera 4 de Vibe. `BlockVibe` no sirve: pone el
  `Skeptical` antiguo, que nadie lee.
- D2: eclosión de 5 a toda la banda (7 con los dos Exaltado).
- D3=A: gritos a Sibi con un chequeo (si una habilidad sin objetivo único tiene frases para un músico que
  está en el escenario, lo llama por su nombre). **Después:** interacción única con Sibi («anularlo» un turno
  o «llamarlo a la colmena»), mecánica por diseñar.
- D4: con larvas si hay arte; sin arte en el repo todavía. **Diseño de Matías (reunión del 12-nov):** las larvas
  lloran muy fuerte (mucho Stress), tienen poca Vibe y gustos distintos a los de la Reina, así que el jugador
  tiene que convencerlas primero aunque tarde más con la Reina. Entran si el arte llega antes del 20-nov
  (D-PLAN-10, por decidir); si no, la Reina va sola.
- D5: Vibe 50, gustos tempo rápido y al menos 2 roles (por ajustar).
- D6: viñeta provisional `jefe (1).png`; pedido #14 a Matías.
- D7=A: Tentáculos se arregla igual (Exaltado +2, «¡TRES!» de 8 → 10 con Exaltado, gig de 4 canciones).

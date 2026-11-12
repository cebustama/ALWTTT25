Mode: IMPLEMENTATION. S8 · GRITOS-1 (DEMO-1211) — el público grita, posa y la banda baila.

**Context.** S7 (ELENCO-1) cerró el 2026-11-12 (commit `d6a8a67`, sin push). Hay tres gigs con público
propio: «Ranas y Rayas» (coro: misma `duetKey` y mismo músico → ×2), «Tentáculos» (uno… dos… ¡TRES!)
y «El jefe y su banda» (Big Guy `isLeader`; Kid e Influencer lo animan con `AudienceLeader`). El público
apunta al que menos golpes aguanta, la banda se coloca según cuántos son y las variantes tienen la
interfaz de cabeza ajustada («Ajustar UI»). Cada habilidad ya tiene id de glosario (nombre y texto ES/EN)
y un floater con su nombre al actuar (LEGIB-1b). Falta que **se oiga al público**: un grito antes de cada
golpe que diga a quién va.

**Decisions locked at batch open.**
- D-PLAN-3 = B: público con rol solo con datos, objetivo visible y gritos; los disparadores, Phase C.
- D-PLAN-5: el grito «Tu papá es travesti» del Miró queda fuera (burla a una identidad; demo pública).
- Recorte del plan: si vamos tarde, GRITOS-1 queda en frases genéricas, sin `{músico}`.
- Añadido en S7: **poses del público** por habilidad (huecos; el arte llega con el pedido #12 a Matías)
  y **el beat de Conito** (flota y no rebota).

**Open at batch open.**
- D1. Dónde viven las frases: ¿en el glosario por habilidad (`ab_x_shout_1..n`, ES/EN, como TEXT-5) o
  en un SO de gritos por personaje? Proponer con el formato de decisión.
- D2. Cuándo sale el bocadillo: ¿al empezar el turno del público, justo antes de cada golpe (el
  floater actual) o las dos cosas? Debe leerse quién recibe antes del golpe.
- D3. Poses: dos huecos por habilidad (`pendingSprite` mientras la anuncia, `actingSprite` al actuar),
  vacíos = sprite normal. ¿Basta con eso o hace falta una pose de «convencido»?
- D4. Conito: confirmar la causa (su `Animator Controller` de flotar parece escribir la posición del
  sprite cada fotograma y pisa el salto de `CharacterAnimator`) y elegir arreglo (mover el flotado a
  un padre, o un pulso de escala en lugar del salto).

**Inputs (leer del disco).**
1. `Docs/planning/active/DEMO-1211_Plan_de_Sesiones.md` (fila S8, D-PLAN-3 y D-PLAN-5).
2. `Docs/reference/ideas/2026-10-30_publico_con_rol.md` (frases y personalidad de cada miembro) y
   `Docs/reference/ideas/2026-11-06_desde_miro.md` (gritos del Miró).
3. `Docs/systems/SSoT_Audience_and_Reactions.md` §7–§8 y `Docs/systems/SSoT_Game_Text.md`.
4. `Assets/Scripts/Characters/AudienceCharacterBase.cs` (`ShowNextAbility`, `AbilityRoutine`, floater)
   y `AudienceCharacterCanvas.cs` (colocación sobre la cabeza, estados, intención).
5. `Assets/Scripts/Data/Characters/Audience/AudienceCharacterData.cs` (`AudienceAbilityData`).
6. `Assets/Scripts/Characters/CharacterAnimator.cs` y `Assets/Prefabs/Characters/Musicians/BandCharacter - Conito.prefab` (+ su Animator Controller).
7. `Assets/Settings/Audience/Elenco_S7.json` y `Assets/Scripts/Characters/Editor/AudienceJson.cs`
   (los gritos y las poses deberían poder escribirse en el JSON).
8. `Docs/reference/reuniones/Pedidos_Arte_Matias.md` (#12).

**Task.**
1. Ejecutar `/buscar-referencias --all` sobre bocadillos y gritos de enemigos (y poses de anuncio).
2. Cerrar D1–D4 con el formato de decisión.
3. Bocadillos: frase por habilidad con `{músico}` sustituido por el objetivo fijado (el mismo que la
   flecha; Spotlight incluido). Frases en ES y EN para los 7 miembros de los tres gigs.
4. Poses: huecos en `AudienceAbilityData`, cambio de sprite al anunciar y al actuar, vuelta al normal; en
   el JSON y en el Audience Member Wizard. Sin arte nuevo, debe verse igual que hoy.
5. Conito rebota (o pulsa) con el beat sin perder el flotado.
6. Logs `[S8]`: grito mostrado (quién, frase, objetivo), pose aplicada, beat de Conito.

**Constraints.** Claude Code no corre Unity; no escribir `.meta`; no tocar MidiGenPlay; español, registro
GDC; previsto = real (el bocadillo nombra al mismo músico que la flecha y que el golpe); tutorial-artefacto
si el montaje es largo; sin commit ni push sin pedirlo.

**Deliverables.** Código y datos; frases en el JSON; smokes con sus líneas `[S8]`; propuesta de
documentación (SSoT de público §7–§8, Game Text, herramientas §23) sin aplicar.

**Closure exit criteria.** En los tres gigs, antes de cada golpe se lee quién lo recibe y alguien se ríe
(criterio del plan); las poses vacías no cambian nada; Conito se mueve con el beat; smokes jugados por
Claudio; docs aprobados y aplicados; commit cuando Claudio lo pida; `proyectos.md`, `foco.md` y la página
de frentes al día; arranque de S9 (REINA-1). Pendiente heredado: comprobar en consola la línea
`… fizzles` tras convencer al jefe; los ojos de corazón de las variantes (a mano, cuando toque).

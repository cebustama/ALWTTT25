# Arranque · Narrador musical en el taller: forma de canción y compás de final

Preparado el 2026-11-12. Línea armónica (foco acotado de Claudio; no toma sesiones de la demo). Se abre desde `D:\Projects\ALWTTT25`. **Precondición:** hecha la prueba de oído corta de las semillas (`ARRANQUE_niveles_recursos.md`); si no, esa va primero.

## Para qué

Claudio (2026-11-12): que la banda **empiece y termine canciones**, con un «Narrador musical» que, según el loop, la canción del gig y el contexto, condiciona la generación: `[INTRO]`, `[VERSE]`, `[BRIDGE]`, `[SOLO, charId]`, `[OUTRO]` + compás de final («let ring»). Idea completa, con sus palabras y lo que ya existe en el código: `../ideas/2026-11-12_narrador_musical.md`.

Esta sesión es **D3 = A**: probarlo de oído en el taller, sin Unity.

## Leer antes

- `../ideas/2026-11-12_narrador_musical.md` (la idea, la tabla de secciones, D1–D3).
- `README.md` de esta carpeta y `template.html`: `arrange(base, nivel, nV, beats, perillas, semilla)`, «Resolver al final», remates, batería del taller, intensidad 0–5.
- `scripts/oro.js` (prueba de oro; hoy `oro-v5.json`, 2.112 casos).
- `revision-music-lab-reglas-P1-P5.md` §14–§16 (remates, semillas).
- Captura de origen: `Music Lab/Composición/2026-11-capturas.md` (sextas de los Beatles: final en `I6`, color `ivm6`).

## Lo ya decidido

- **D1 = C:** el narrador pone la forma por defecto; las cartas de parte la fuerzan. En el taller: forma automática y un modo «a mano».
- **D2 = B:** la sección también cambiará reglas de juego, pero eso se diseña en ALWTTT después de la demo. **Aquí solo se oye.**
- **D3 = A:** prototipo en el taller.
- Sin forma elegida, el taller suena exactamente como hoy (`oro-v5` idéntico).
- Reglas deterministas con semilla; nada de LLM en la generación.

## Decisiones para presentar al abrir (formato D, en lenguaje llano)

1. **Qué cambia cada sección en el taller.** Propuesta mínima: INTRO = nivel 1 y batería ligera (o solo platos); VERSE = nivel de la carta; BRIDGE = paleta o nivel contrastante; SOLO = sin adornos que tapen; OUTRO = «Resolver al final» + fill + compás de final. ¿Qué se queda y qué espera?
2. **Tipos de compás de final:** «let ring» (acorde sostenido con platos), golpe seco, ritardando, final en sexta (`I6`). ¿Cuáles en esta sesión?
3. **Forma por defecto según el número de loops:** 3 loops = INTRO–VERSE–OUTRO (la de Claudio); 4+ = ¿VERSE–BRIDGE–…? ¿Y si el último loop no se conoce (Overload)?
4. **Final de canción frente a final de gig:** ¿uno más grande para el último?
5. **Consultar a Music Lab** (regla de Claudio): cómo cierran de verdad las canciones de la Biblioteca y qué hace un puente; subagente como Music Lab.

## Tareas

1. Funciones puras: `formaPorDefecto(nLoops, cancionEnGig, totalCanciones)` → lista de etiquetas, y `aplicarSeccion(etiqueta, …)` → parámetros de `arrange()` y de la batería.
2. Compás de final en el reproductor del taller (los tipos decididos en la decisión 2).
3. Interfaz: selector de forma (auto / a mano por loop), vista de la etiqueta de cada loop mientras suena.
4. Prueba de oro: `oro-v5` idéntico sin forma; casos nuevos con forma y semilla fija (`oro-v6.json`).
5. Hoja de oído corta (estilo `prueba-oido-remates/`): 3–4 cartas con y sin forma, ¿suena a canción que empieza y termina?
6. Republicar el taller y la copia web (`/privado/taller-armonia/`).

## Cierre

- Resultado de la hoja de oído en `README.md` y en la idea.
- Actualizar `_comun/foco.md` (tangente «Narrador musical»), `_comun/proyectos.md`, y la fila del narrador en `MidiGenPlay/.../Sesiones_MidiGenPlay.md` con lo que el motor tendrá que leer (contexto de sección y compás de final).
- Commit local, sin push.

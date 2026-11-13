# Arranque · Narrador musical en el taller: forma de canción y compás de final

Preparado el 2026-11-12. Línea armónica (foco acotado de Claudio; no toma sesiones de la demo). Se abre desde `D:\Projects\ALWTTT25`. **Precondición cumplida:** la prueba de oído de las semillas se hizo el 2026-11-12 (`HANDOFF_2026-11-12_semillas.md`, §17 de `revision-music-lab-reglas-P1-P5.md`). **Orden recomendado: P6 primero** (`ARRANQUE_P6_pesos.md`), ver abajo.

## Para qué

Claudio (2026-11-12): que la banda **empiece y termine canciones**, con un «Narrador musical» que, según el loop, la canción del gig y el contexto, condiciona la generación: `[INTRO]`, `[VERSE]`, `[BRIDGE]`, `[SOLO, charId]`, `[OUTRO]` + compás de final («let ring»). Idea completa, con sus palabras y lo que ya existe en el código: `../ideas/2026-11-12_narrador_musical.md`.

Esta sesión es **D3 = A**: probarlo de oído en el taller, sin Unity.

## Leer antes

- `../ideas/2026-11-12_narrador_musical.md` (la idea, la tabla de secciones, D1–D3).
- `README.md` de esta carpeta y `template.html`: `arrange(base, nivel, nV, beats, perillas, semilla)`, «Resolver al final», remates, batería del taller, intensidad 0–5.
- `scripts/oro.js` (prueba de oro; hoy `oro-v5.json`, 2.112 casos).
- `revision-music-lab-reglas-P1-P5.md` §14–§16 (remates, semillas).
- Captura de origen: `Music Lab/Composición/2026-11-capturas.md` (sextas de los Beatles: final en `I6`, color `ivm6`).
- Captura del 2026-11-13: el **downbeat sintético** de Bill Wurtz (secciones de 4, 6, 7 o 9 compases que no se notan; instrumentación + frase que vuelve a empezar + caer en el IV), en `../ideas/2026-11-13_bill_wurtz_forma_y_armonia.md`.

## Lo ya decidido

- **D1 = C:** el narrador pone la forma por defecto; las cartas de parte la fuerzan. En el taller: forma automática y un modo «a mano».
- **D2 = B:** la sección también cambiará reglas de juego, pero eso se diseña en ALWTTT después de la demo. **Aquí solo se oye.**
- **D3 = A:** prototipo en el taller.
- Sin forma elegida, el taller suena exactamente como hoy (el oro vigente idéntico; ver abajo).
- Reglas deterministas con semilla; nada de LLM en la generación.

## Cambios tras la prueba de oído de las semillas (2026-11-12)

- **Orden: P6 antes que el narrador.** Las secciones reutilizan `arrange()` con semilla. Si la regla de identidad (la cadencia andaluza que se rompe con las semillas, D1 = B acotada por Music Lab) no está antes, el BRIDGE y el OUTRO heredan el fallo. Si Claudio elige el narrador primero, la hoja de oído no debe usar la andaluza con semilla.
- **La base de la prueba de oro depende de P6.** Si P6 va antes, partir del oro que deje P6 (v4 sin semilla, o `oro-v6` si Claudio aceptó cambiarlo; `oro-v5` rehecho). El oro de esta sesión toma **el siguiente número libre**, no `oro-v6` a ciegas.
- **«De fábrica» = la primera vez que suena la carta** (matiz de Claudio, §17). Es justo el contexto que da el narrador: en el INTRO de la primera canción la carta suena de fábrica; si la carta vuelve a sonar (otra sección, otra canción), su vuelta 1 puede variar. Decisión nueva al abrir (la 6).
- **El remate «al azar» no se ha oído a ciegas** (las 4 cartas de la hoja cierran solas por cadencia). El OUTRO con «Resolver al final» es la ocasión: la hoja de esta sesión debe incluir cartas que no cierren solas.
- **Idea relacionada:** el bajo y el piano se reparten el acorde (`../ideas/2026-11-12_bajo_y_piano_reparten_el_acorde.md`). Puede ser un rasgo de sección (INTRO solo con bajo, el piano entra sin fundamental). No entra en esta sesión: anotarlo en la tabla de secciones.
- **Pendiente menor que se puede cerrar aquí:** la copia web y el prompt de `armonia.ajustar` no conocen el remate «al azar»; como esta sesión republica la copia web, ponerlos al día.
- **Commits solo con pathspec:** no tocar los cambios de runtime de ALWTTT ni `_comun/capturas/registro.md`.

## Decisiones para presentar al abrir (formato D, en lenguaje llano)

1. **Qué cambia cada sección en el taller.** Propuesta mínima: INTRO = nivel 1 y batería ligera (o solo platos); VERSE = nivel de la carta; BRIDGE = paleta o nivel contrastante; SOLO = sin adornos que tapen; OUTRO = «Resolver al final» + fill + compás de final. ¿Qué se queda y qué espera?
2. **Tipos de compás de final:** «let ring» (acorde sostenido con platos), golpe seco, ritardando, final en sexta (`I6`). ¿Cuáles en esta sesión?
3. **Forma por defecto según el número de loops:** 3 loops = INTRO–VERSE–OUTRO (la de Claudio); 4+ = ¿VERSE–BRIDGE–…? ¿Y si el último loop no se conoce (Overload)?
4. **Final de canción frente a final de gig:** ¿uno más grande para el último?
5. **Consultar a Music Lab** (regla de Claudio): cómo cierran de verdad las canciones de la Biblioteca y qué hace un puente; subagente como Music Lab.
6. **La vuelta 1 según la sección:** ¿de fábrica solo la primera vez que suena la carta en el gig, o en el primer loop de cada canción?

## Tareas

1. Funciones puras: `formaPorDefecto(nLoops, cancionEnGig, totalCanciones)` → lista de etiquetas, y `aplicarSeccion(etiqueta, …)` → parámetros de `arrange()` y de la batería.
2. Compás de final en el reproductor del taller (los tipos decididos en la decisión 2).
3. Interfaz: selector de forma (auto / a mano por loop), vista de la etiqueta de cada loop mientras suena.
4. Prueba de oro: sin forma, el oro vigente idéntico (el que deje P6); casos nuevos con forma y semilla fija en el siguiente `oro-vN.json` libre.
5. Hoja de oído corta (estilo `prueba-oido-remates/`): 3–4 cartas con y sin forma, ¿suena a canción que empieza y termina?
6. Republicar el taller y la copia web (`/privado/taller-armonia/`).

## Cierre

- Resultado de la hoja de oído en `README.md` y en la idea.
- Actualizar `_comun/foco.md` (tangente «Narrador musical»), `_comun/proyectos.md`, y la fila del narrador en `MidiGenPlay/.../Sesiones_MidiGenPlay.md` con lo que el motor tendrá que leer (contexto de sección y compás de final).
- Commit local, sin push.

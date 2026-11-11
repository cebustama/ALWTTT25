# Narrador musical: la banda empieza y termina canciones

Idea de Claudio, 2026-11-12, al capturar el video de David Bennett sobre los acordes de sexta de los Beatles (`Music Lab/Composición/2026-11-capturas.md`). Estado: **idea, después de la demo**. No es autoridad; el plan de la demo manda (`planning/active/DEMO-1211_Plan_de_Sesiones.md`).

## Lo que dijo Claudio (tal cual)

> Como idea, agregar la opción de un último acorde "let ring" para terminar una canción. Podría hacerse a modonde cerrar una canción o cerrar efectivamente la gig, el concepto de outro o ending. Quizás eso podría "condicionar" las paletas o criterios de generación en cada loop de cada canción del gig. Internamente loop 0 = [INTRO], loop 1 = [VERSE], loop 3 = [OUTRO] + ending measure.
>
> Y con más loops se puede incorporar wl concepto de [SOLO, charId] o [BRIDGE] (que considera lo que vino antes y condiciona internamente lo que viene después). Me refiero a tener una especie de "Narrador Musical" guiando las generaciones según el contexto, el numero de loop, número de canción del gig, etc. Para que realmente se sienta como una banda tocando, improvisando entre ellos, y empezando y terminando "canciones".

## Lo que ya existe (comprobado el 2026-11-12)

- **Esqueleto de partes dormido.** `PartActionDescriptor` / `PartActionKind` (Intro, Solo con `musicianId`, Outro, Bridge, Final) y en `SongCompositionUI` las reglas `TryAddIntro` / `TryAddOutro` y desplegables de intro y outro. Pensado como **cartas de parte** que pone el jugador. En SP1 los tests ST-SP1-5/6 quedaron sin probar porque «no existen cartas Intro/Outro ni canciones multiparte»: los ids están vivos e inertes (`CURRENT_STATE.md`).
- **`partIndex` ya siembra la paleta:** la progresión sale de `ChordProgressionPaletteSO` con semilla `(songSeed, partIndex)`.
- **SONG-TAIL-1:** entre canciones la banda no se calla (el último loop se repite como cola) y al ganar o perder hay un fundido. Hoy **no hay finales**: ni de canción ni de gig.
- **Taller armónico:** ya tiene «Resolver al final» y la hoja de remates (resolución y bucle). El alfabeto tiene `6` y `m6` (Tier A de MidiGenPlay).
- **Nombre ocupado:** `MusicDirector` es el reproductor del OST (`Roadmap_Audio.md`). El narrador necesita otro nombre en el código (p. ej. `SongNarrator`).

## Notas del agente (✦ IA)

**La forma.** Un componente de ALWTTT que, antes de montar cada loop, decide una **etiqueta de sección** con lo que sabe el juego: número de canción en el gig, loop actual y cuántos quedan, qué cartas se jugaron, quién toca y cómo va el público. La etiqueta viaja a MidiGenPlay como **contexto**, y cada generador la lee:

| Etiqueta | Armonía (taller) | Batería | Densidad / entradas | Final |
| --- | --- | --- | --- | --- |
| INTRO | nivel 1, sin adornos | sin fill o solo platos | entra uno o dos (enlaza con «el orden de entrada manda») | — |
| VERSE | nivel de la carta | groove base | todos | — |
| BRIDGE | paleta contrastante o modulación; recuerda lo anterior | fill de entrada | cambia quién guía | — |
| SOLO, charId | la carta del solista; los demás acompañan | sin fills que tapen | el solista arriba, el resto baja | — |
| OUTRO | «Resolver al final» | fill de cierre | todos | **compás de final** |

Tipos de **compás de final**: acorde sostenido («let ring»), golpe seco, ritardando, fundido, y el final en sexta de *She Loves You* (`I6`). Final de canción ≠ final de gig: el del gig puede ser más grande (y sustituir al fundido de SONG-TAIL-1 al ganar).

**Reparto entre proyectos.** MidiGenPlay no sabe de gigs (regla: paquete sin juego, sin LLM; ver la memoria «LLM co-diseñador»). Así que:
- **MidiGenPlay** expone un **contexto de sección** (etiqueta, posición, intensidad, solista) que leen los generadores, y sabe tocar un compás de final.
- **ALWTTT** tiene el narrador, que decide la etiqueta. Reglas deterministas con semilla, no LLM.

**Jugador y narrador.** El esqueleto de partes era para que el jugador marcase las secciones con cartas. Propuesta: híbrido. El narrador pone la forma por defecto y una carta la fuerza (una carta «Solo» pone `[SOLO, charId]`; una «Final» adelanta el outro). Así las cartas de parte dormidas vuelven a tener sentido.

**Preguntas abiertas (de Claudio):**
1. ¿Se conoce siempre el último loop de una canción? En la demo hay 3 loops fijos, pero Overload alarga la parte. Si no se conoce, el outro se decide al entrar en el último loop conocido.
2. ¿La sección cambia también las reglas del juego (el público premia el puente, el solo da Hype al solista) o solo la música? Recomendación: primero solo música.
3. ¿El compás de final consume tiempo de juego o va durante la resolución?

**Cómo probarlo barato.** Antes de Unity, en el taller armónico: un selector de forma (INTRO–VERSE–…–OUTRO + final) que aplique esta tabla a la armonía y a la batería del taller y se pueda escuchar. Mismo método que las perillas y las semillas.

## Decisiones de Claudio (2026-11-12)

- **D1 = C, híbrido.** El narrador pone la forma por defecto y las cartas de parte la fuerzan («Solo» → `[SOLO, charId]`, «Final» adelanta el outro). Revive el esqueleto de partes dormido.
- **D2 = B, la sección cambia también las reglas.** Además de la música, cada sección puede tener efecto de juego (ideas sin decidir: el público premia el puente, el solo da Hype al solista, el final cierra la canción con un bonus). En el prototipo del taller solo se oye; el diseño de las reglas va en ALWTTT, después de la demo.
- **D3 = A, primero en el taller.** Prototipo de forma en el taller armónico web: `taller-armonia/ARRANQUE_narrador_forma.md`.
- **Sigue abierto:** si el último loop se conoce siempre (Overload alarga la parte).

## Dónde entra en la planificación

- **Demo (DEMO-1211):** no entra. Si se quiere un detalle, el único barato sería el acorde «let ring» al ganar el gig en lugar del fundido, y solo si sobra tiempo (decisión de Claudio).
- **Línea armónica (acotado):** prototipo de forma en el taller, después de la prueba de oído de las semillas.
- **MidiGenPlay:** es el paraguas de la cola (fills en la sesión 1, escuchar acentos, orden de entrada, transiciones 2 bis). Fila «Narrador musical» en `MidiGenPlay/MidiGenPlay/Documentation~/planning/active/Sesiones_MidiGenPlay.md`.
- **Conexión:** el narrador tipo storyteller de mundo-islas es el mismo patrón (un director que lee el contexto y condiciona lo que se genera).

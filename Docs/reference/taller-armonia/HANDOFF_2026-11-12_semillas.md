# Handoff · Las cartas son paletas: semillas en el taller armónico (sesión del 2026-11-11/12)

## En una frase

El taller armónico ya da **una versión distinta de cada carta en cada jugada**, reproducible por un número (la semilla). Sin semilla suena exactamente como la v4. Una prueba de oído a ciegas confirmó que la identidad aguanta en 3 de 4 cartas, encontró por qué se rompe la cadencia andaluza y dejó listo el siguiente paso (P6).

## Qué hicimos

1. **Decisiones de arranque** (`ARRANQUE_semillas_paletas.md`), todas las recomendadas:
   - D1 = A: sin semilla = v4.
   - D2 = B: un hash de enteros por decisión, nunca una cadena de tiradas.
   - D3 = B: la semilla elige el remate y dónde caen los adornos.
   - D4 = C: las versiones se guardan en el catálogo del db.
2. **Código** (`template.html`, sección de teoría):
   - `arrange(base, nivel, nV, beats, perillas, semilla)`.
   - `hash32` usa el finalizador de splitmix32. En C# se porta con `uint` y `unchecked`, sin `System.Random`.
   - Remate `"azar"` de fábrica en el N3, elegido por modo: en mayor {rico, simple, ii–V}, en menor {rico, deslizado, ii–V}.
   - Cada vuelta recorre los acordes en su propio orden barajado. La prioridad entre reglas, el presupuesto, el color y la vuelta 1 no cambian.
3. **Pruebas:**
   - `oro-v4.json`: 792 casos idénticos sin semilla.
   - `oro-v5.json`: 2.112 casos con las semillas 1, 7, 42 y 2026. La misma semilla da siempre lo mismo. Cambian 37 de 264 casos en el N2 y 85 de 264 en el N3.
4. **Página del taller** (https://claude.ai/artifact/6ABsXYisfqmwdof7PhsYik): campo «Versión», 🎲 «Otra versión», «De fábrica» y «Guardar esta versión», que guarda nivel y semilla en el catálogo.
5. **Hoja de oído a ciegas** (https://claude.ai/artifact/WYchcr4kd14YsNuxyv23HJ, fuente en `prueba-oido-semillas/`): 4 cartas × (fábrica + 3 semillas), respondida por Claudio desde el móvil.

## Qué aprendimos (§17 de `revision-music-lab-reglas-P1-P5.md`)

- **La identidad aguanta:** Pachelbel, Axis y Frigio siguen sonando a sí mismas. Pachelbel es el modelo («me encantan todas las variaciones»), y Claudio guardaría la semilla 147.
- **La andaluza se rompe:** las semillas meten un ii–V antes del V y cortan el medio tono ♭VI → V. La de fábrica la protegía por casualidad, por el orden de izquierda a derecha.
- **La variedad se queda corta:** toca P6, para que la semilla elija también qué adorno, con pesos.
- **La vuelta 1 sigue de fábrica.** En el juego, «de fábrica» significa la primera vez que suena la carta. Si se vuelve a tocar, puede variar (matiz para el port).
- **Decisión D1 = B:** una regla general que protege el paso de bajo que define la carta. Hay que acotarla con Music Lab, porque escrita a lo ancho también quitaría adornos de la versión de fábrica.

## Idea nueva de Claudio

**El bajo y los acordes se reparten el acorde** (Conito y Sibi): el bajo lleva la fundamental o una inversión y los acordes tocan sin fundamental. Hoy el bajo solo dobla la fundamental. Nota en `../ideas/2026-11-12_bajo_y_piano_reparten_el_acorde.md` y fila en la cola de MidiGenPlay. Después de la demo.

## Commits (con pathspec; sin tocar el runtime de ALWTTT ni `capturas/registro.md`)

- ALWTTT `aabfce3` (semillas) y `c65b9d1` (hoja de oído), más el commit de cierre de esta sesión.
- MidiGenPlay `5a12737` y el cierre.
- `_comun` `0c82059` y el cierre.
- Nada subido (sin push).

## Siguiente

Dos arranques listos. Claudio elige el orden:
- `ARRANQUE_P6_pesos.md`: proteger la identidad (andaluza) y que la semilla elija el adorno.
- `ARRANQUE_narrador_forma.md`: el narrador musical, con secciones y finales.

Pendiente menor: el prompt de `armonia.ajustar` y la copia web del taller no conocen el remate «al azar».

## Prompt para retomar

```
Línea armónica de ALWTTT (taller armónico). Abro desde D:\Projects\ALWTTT25.
Sigue Docs/reference/taller-armonia/ARRANQUE_P6_pesos.md (o ARRANQUE_narrador_forma.md si elijo ese).
Contexto: HANDOFF_2026-11-12_semillas.md y §17 de revision-music-lab-reglas-P1-P5.md.
Primero, las decisiones del arranque en formato D, con recomendación.
Sin semilla debe seguir dando la v4 (oro-v4, 792 casos) salvo que yo decida otra cosa.
Las dudas musicales van a Music Lab. Commits solo con pathspec: no toques los cambios
de runtime de ALWTTT ni _comun/capturas/registro.md.
```

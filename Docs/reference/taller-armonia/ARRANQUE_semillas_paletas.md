# Arranque · Las cartas son paletas: `arrange()` con semilla por jugada (§15)

Preparado el 2026-11-11. Línea armónica (foco acotado de Claudio; no toma sesiones de la demo). Se abre desde `D:\Projects\ALWTTT25`.

## Para qué

Claudio (2026-11-11): **una carta representa un patrón o una idea, no la canción definitiva.** Cada jugada debe sonar distinta, pero dentro de la identidad de la carta; más adelante quiere guardar una canción ya generada. Hoy `arrange()` es determinista y fijo: la misma progresión con el mismo nivel suena siempre igual.

## Leer antes

- `revision-music-lab-reglas-P1-P5.md`: §14 (D12–D14 y resultados de la hoja de remates) y **§15** (la propuesta).
- `template.html`, sección de teoría: `arrange(base, level, nV, beats, perillas)`, `PERILLAS_V3`, `INTENSIDAD`, `PERILLA_OPC`, el orden fijo de las reglas (remate → sus4 → menor prestado → disminuido → acorde de paso → ♭VI…).
- `scripts/oro.js` y `README.md` (prueba de oro; hoy `oro-v4.json`, 792 casos).

## Lo ya decidido

- Remate del N2: el simple. Remate del N3: al azar entre los que gustan. Grupos por modo: **mayor {rico, simple, ii–V}**, **menor {rico, deslizado, ii–V}**.
- Las reglas de identidad no cambian:
  - ningún acorde escrito desaparece;
  - no se modula;
  - hay un presupuesto por vuelta;
  - la vuelta 1 va tal cual de fábrica (`desdeVuelta` 2; D13 permite 1).

## Decisiones para presentar al abrir (formato D, en lenguaje llano)

1. **Sin semilla = v4.** Si `arrange()` no recibe semilla, da exactamente lo de hoy. Así `oro-v4` sigue idéntico y sirve de red. Recomendado: sí.
2. **El generador de azar.** Uno pequeño y de enteros, fácil de copiar en C# con el mismo resultado (por ejemplo mulberry32 o splitmix32). Nada de `Math.random` en la teoría. Así la paridad con MidiGenPlay sigue siendo posible.
3. **Hasta dónde llega el azar en esta sesión:**
   - (a) solo el remate dentro del grupo;
   - (a + b) también dónde caen los adornos (qué acorde y qué vuelta), en lugar de ir de izquierda a derecha;
   - (a + b + c) también qué adorno, con pesos.
   
   Recomendado: (a + b). La (c) son los perfiles de P6 y merece su propia hoja de oído.
4. **Guardar una versión.** Una versión es texto + perillas + semilla. Dónde se guarda: enlace (hash), navegador, o el db del artefacto (el taller ya declara `db`). Cargar el skill `artifact-capabilities` antes de tocar el db.

## Pasos

1. Escribir las reglas del azar en `revision-music-lab-reglas-P1-P5.md` §16. Para cada paso: qué decide la semilla, de qué grupo elige y qué no puede tocar. Las dudas musicales se consultan a Music Lab con un subagente.
2. Implementar en `template.html`. La firma pasa a `arrange(base, level, nV, beats, perillas, semilla)`. La perilla nueva (por ejemplo `remate: "azar"` con su grupo por modo) va apagada por defecto. `node scripts/oro.js comparar oro-v4.json` tiene que dar **792 idénticos**.
3. Oro con semillas: `oro.js` gana un modo con 3–5 semillas fijas por caso y se escribe `oro-v5.json`. Comprobar que la misma semilla da siempre lo mismo y que semillas distintas sí cambian algo en el N3.
4. En la página:
   - botón 🎲 «otra versión»;
   - la semilla visible y editable;
   - «guardar esta versión» y la lista de versiones guardadas.
   
   El deslizador de intensidad y las perillas siguen igual.
5. Prueba de oído corta, si Claudio quiere: 3–4 cartas × 3 semillas, para ver si suenan a la misma carta. Las hojas de oído fijan la semilla.
6. Cierre:
   - `README.md` al día;
   - `build.py` y publicar el taller (https://claude.ai/artifact/6ABsXYisfqmwdof7PhsYik);
   - en `Sesiones_MidiGenPlay.md`, fila «Niveles armónicos», anotar que la paridad va con semillas y con el generador elegido;
   - actualizar `_comun/foco.md` y la fila de ALWTTT en `_comun/proyectos.md`.

## Hecho cuando

- `oro-v4` sigue idéntico sin semilla.
- `oro-v5` está escrito con semillas fijas.
- La página tiene 🎲, la semilla visible y «guardar versión».
- La regla de los remates del N3 (al azar en su grupo) está encendida por decisión de Claudio.

## Fuera de esta sesión

- P6, los perfiles de estilo con pesos (salvo que se elija a + b + c).
- El port a C#.
- Las canciones largas que suenan mal: programar las notas por ventanas, §6 de `revision-music-lab-recursos-2026-11-11.md`.

## Reglas

- Commits locales con pathspec: en ALWTTT hay cambios de runtime de otras sesiones (`Assets/Scripts/...`) que no se tocan.
- No tocar `_comun/capturas/registro.md`.

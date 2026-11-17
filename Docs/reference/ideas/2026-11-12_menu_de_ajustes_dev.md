# Menú de ajustes de gameplay en el juego (F5)

Pedido de Claudio, 2026-11-12, tras la reunión con Matías. Estado: **hecha en DEV-TUNING-1 (2026-11-17)**.
Autoridad: `systems/SSoT_Dev_Mode.md` §23. Página para testers: https://claude.ai/artifact/VCSqZeSsWnZvCFUHgUoxgv.
Fuera de DEV-TUNING-1: el bocadillo de «mano llena» de Slay the Spire (POST-DEMO) y separar las builds (S12).

## Lo que se pide

Un menú dentro del juego para cambiar las variables de gameplay y los ajustes importantes sin tocar Unity:

- Cartas por turno y personaje: **una**, **una de cada tipo** (acción y composición) o **sin límite**.
- Cartas que se roban al empezar el turno y entre loops.
- Máximo de cartas en la mano. **Hay que comunicarlo en la UI** (ver referencias).
- Inspiración al empezar y entre loops.
- Interruptor: **sin límite de canciones**.
- Porcentaje de vida que queda después del colapso.
- Y otros que sirvan; **tiene que ser fácil añadir campos nuevos**.

Con **«Guardar»** (la configuración actual) y **«Valores por defecto»**, como un menú de opciones normal.
Puede ser una pestaña del Dev Mode, pero con **tecla directa (F5)**.

## Lo que ya existe (✦ IA)

- **Dev Mode** (`Assets/Scripts/DevMode/DevModeController.cs`): ventana IMGUI con F12 y pestañas
  (Infinite, Catalogue, Stats, Audio Mix, Composition Debug). Solo existe con `ALWTTT_DEV`.
- Ya hay ajustes repartidos en ScriptableObjects: `MeterTuningSO` (qué deja el colapso), `GigDevSettingsSO`,
  la economía de jugadas de ECON-1 (jugadas de composición por periodo) y el «infinite turns» del Dev Mode.

## Diseño propuesto (✦ IA)

- **Un registro de ajustes**: cada ajuste es **una línea** con nombre visible, grupo, tipo (entero con
  mínimo y máximo, decimal, interruptor u opción de una lista), cómo se lee, cómo se escribe y su valor por
  defecto. Añadir un ajuste nuevo es añadir una línea; la pestaña se dibuja sola a partir del registro.
- **El valor por defecto es el del asset** (el SO que ya manda). El menú solo guarda lo que el jugador cambia:
  un JSON en `persistentDataPath/DevTuning.json` que se aplica al arrancar. «Valores por defecto» borra ese
  JSON y vuelve a leer los SO. Así los assets nunca se modifican desde la build.
- **F5** abre el Dev Mode directamente en esta pestaña. Cada cambio deja un log `[DevTuning] clave: antes → después`,
  y la telemetría apunta si la partida se jugó con ajustes distintos de los de fábrica (si no, los datos de
  balance se mezclan).
- **Build pública frente a build de testers:** el Dev Mode (F12 y F5) va en la de testers. En la de Discord se
  quita (`ALWTTT_DEV` fuera, con la telemetría en un símbolo propio) o se esconde. Esto choca con «build sin el
  panel F12» de S9c: hay que separar las dos builds.

## Referencias

- **Balatro** (`C:\Users\cebus\Dropbox\ALWTTT References\twopaircool.png`): bajo la mano, un **«8/8»** pequeño
  entre «Play Hand» y «Discard». El máximo de la mano se ve siempre, sin texto, y ocupa muy poco. Tomar el
  contador junto al abanico, que cambie de color al llenarse. No tomar el ordenar mano.
- Slay the Spire (de memoria, no está en las colecciones): cuando la mano está llena, el personaje lo dice en
  un bocadillo y la carta robada va al descarte. Encaja con los bocadillos de S8.

## Añadido el 2026-11-14 (S9c, Claudio, probando a la Reina)

- **Saltar la canción hasta el turno del público**: botón en pantalla o hotkey de dev que termina la canción en curso (con su puntuación) y pasa directo al turno del público. Para probar lo que dura varios turnos (huevo · huevo · eclosión de la Reina, estados que caducan) sin escuchar cada canción entera. Entra en DEV-TUNING-1 junto al menú F5.
- Ya resuelto en S9c: al hacer un build salta un aviso si siguen encendidos las cartas TEST, el mazo fijo o la semilla fijada (`Assets/Scripts/Editor/DevSettingsBuildGuard.cs`).

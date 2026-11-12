# Arranque S9b · AUDIO-LOOP-1: nada se corta entre loops

Escrito el 2026-11-12 desde la sesión 1 de MidiGenPlay (acentos y fills). Planning, no autoridad. Fila S9b de `DEMO-1211_Plan_de_Sesiones.md`.

## Pegar en la sesión nueva

> Lee `Docs/planning/active/ARRANQUE_S9b_audio_loop.md` y haz S9b (AUDIO-LOOP-1). Objetivo: que el crash del cierre de un loop, y cualquier nota que siga sonando, cruce la frontera del loop sin cortarse. Empieza con `/buscar-referencias --all` y preséntame las decisiones en formato D antes de construir.

## Estado al empezar (2026-11-13)

- S9 (REINA-1) cerrada, commit `426bfbe`. S9b es la siguiente del orden de entrada; luego S9c y ECON-2.
- El bundle de ritmo de prueba con fills (`starter_default_mode_Payload_Rhythm_StyleBundle.asset`: `fillEveryNMeasures` 4, `lastMeasuresAsFill`, `crashAfterFill`) está modificado **sin commitear** en el árbol: sirve para reproducir el corte. Decidir en la sesión si se queda o se revierte.
- Idea relacionada, no para esta sesión: el eco musical de las cartas de acción (`reference/ideas/2026-11-12_accion_con_eco_musical.md`) también toca qué suena en la frontera del loop. Que el diseño de S9b no le cierre la puerta.

## Qué pasa (comprobado en el código el 2026-11-12; releer antes de afirmar)

Claudio lo oyó al probar los fills de MidiGenPlay: el crash que suena al cerrar un loop se corta de golpe al empezar el siguiente. No es del generador: es de la reproducción, y afecta a **cualquier** voz que siga sonando (colas de platos, notas largas de acordes o bajo).

1. **ALWTTT para antes de tocar.** `MidiMusicManager.PlayBytes` (≈ línea 2142) llama a `player.Stop()` y luego a `player.Play(data)` para cada loop. `MidiToolkitAdapter.Stop()` llama a `MPTK_Stop()`, que es `MPTK_Stop(stopAllSound: true)` → `ThreadClearAllSound(true)`.
2. **Maestro limpia al empezar.** `MidiFilePlayer.MPTK_Play` hace `MPTK_InitSynth()` cuando no está sonando, y el hilo de reproducción (`ThreadInternalMidiPlaying` / el arranque del MIDI, ≈ líneas 1515–1525 y 1815–1822 de `Assets/MidiPlayer/Scripts/MPTKGameObject/MidiFilePlayer.cs`) lanza `ThreadClearAllSound(true, IdSession)` antes de tocar.
3. **El generador ya pone un `AllSoundOff`** al final de cada parte, pero solo en el canal del metrónomo (`SongOrchestrator`, «Safety boundary at exact end of the part»): no es la causa.
4. **Precedente: SONG-TAIL-1** (`_tailActive`, `QueueTailHandoff` en `MidiMusicManager`): ya hay una cola de canción que espera a la barra siguiente. Leer cómo funciona antes de diseñar.

## Opciones para presentar (formato D)

- **A · Crash de entrada (MidiGenPlay, barata, solo el crash).** Con `lastMeasuresAsFill` y `crashAfterFill`, el crash va en el 1 del compás 1 de la parte (el loop siguiente) en vez de colgar del anterior. Fila 1b de `MidiGenPlay/.../Sesiones_MidiGenPlay.md`. Comprobar que el borrado asíncrono de Maestro no se coma un golpe en el tick 0. No arregla las demás notas largas.
- **B1 · No apagar al encadenar (ALWTTT).** `MPTK_Stop(stopAllSound: false)` al pasar de un loop a otro y evitar el `ThreadClearAllSound` del arranque. Lo segundo vive en el código de Maestro: tocarlo es modificar un paquete de terceros (anotar el parche, se pierde al actualizar).
- **B2 · Dos reproductores alternados (ALWTTT).** El loop k suena en el reproductor A y el k+1 en el B; arrancar B no apaga las voces de A. Más trabajo: mezcla por canal (M-AUDIO-MIX, BAL-1), metrónomo, `OnEventNotesMidi` para el Big Num y el beat, pausa (PAUSE-1).
- **B3 · `MPTK_InnerLoop` (Maestro Pro).** Si el mismo MIDI se repite, el bucle interno no reinicia el sintetizador. Solo sirve cuando el loop siguiente es idéntico (no lo es: cada repetición puede traer otros fills y otra semilla).

Recomendación de partida (a confirmar leyendo el código): A como red de seguridad inmediata y B1 si el parche de Maestro es pequeño; B2 solo si B1 no es posible.

## Restricciones

- No romper «previsto = real» del Big Num ni la telemetría.
- Etiqueta de log de la sesión: `[AUDIO-LOOP-1]` (regla de `Debug.Log` por sesión).
- Smokes jugados por Claudio (≤3): gig de 3 loops con fills (crash del cierre), una carta con acordes largos, y pausa/reanudar en medio de un loop.

# Parches locales a Maestro (MPTK Pro)

Registro de cada cambio hecho a mano en el código de Maestro (`Assets/MidiPlayer/`), paquete de terceros. **Si se actualiza Maestro, estos parches se pierden: reaplicarlos desde aquí.** Cada parche lleva en el código la marca `[ALWTTT PATCH <batch>]`; búscala con grep para comprobar que siguen.

| # | Batch | Fecha | Archivo | Qué cambia | Por qué | Quién lo usa |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | AUDIO-LOOP-1 (S9b) | 2026-11-13 | `Assets/MidiPlayer/Scripts/MPTKGameObject/MidiFilePlayer.cs` | Campo nuevo `public bool MPTK_ClearSoundOnStart = true` (`[HideInInspector]`). Las dos llamadas a `ThreadClearAllSound(true, IdSession)` del arranque de un MIDI (`ThreadLegacyPlay` y `ThreadInternalMidiPlaying`) quedan dentro de `if (MPTK_ClearSoundOnStart)`. | Ese borrado hace `ClearAllVoices`: corta de golpe las colas del MIDI anterior (crash, release de acordes) en la frontera de cada loop. Con `true` el comportamiento es el original de Maestro. | `ALWTTT.Music.MidiToolkitAdapter`: `Play()` lo pone en `true`; `PlayChained()` en `false`. |

## Cómo reaplicar el parche 1

1. En `MidiFilePlayer.cs`, después de `public ModeStopPlay MPTK_ModeStopVoice;`, añadir el campo `MPTK_ClearSoundOnStart` (valor por defecto `true`).
2. Buscar las dos apariciones de `Routine.RunCoroutine(ThreadClearAllSound(true, IdSession), Segment.RealtimeUpdate);` precedidas por `if (Application.isPlaying)` (comentario `// V2.84` encima) y envolver ese `if/else` en `if (MPTK_ClearSoundOnStart) { … }`.
3. No tocar `MPTK_Stop`, `MPTK_Pause` ni `MPTK_RePlay`: siguen limpiando como siempre.

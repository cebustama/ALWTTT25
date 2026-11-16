# Arranque: VOZ-2, el cantante v8 y las sílabas (estilos de voz)

Segunda de tres tandas del plan de estilos de voz (`ARRANQUE_estilos_de_voz.md`, D-VOZ-1..7). VOZ-1 cerró el 2026-11-14: el fork de Pink Trombone ya sabe X1–X3 (POC-FORK 8–11) con paridad JS = C#, y `VoiceProfileSO` v2 tiene los campos del taller con `engine = V7_CLite`. Compila y Zig suena igual (Claudio). Planificación, no autoridad.

> **2026-11-16:** cinco estilos nuevos (susurro, canto de garganta, nasal, canto llano, yodel) entran primero en el taller (`ARRANQUE_TALLER-MEL-A.md`); su port a C# es **VOZ-2b**, después de VOZ-2 y de la demo. Nasal usa el `velumOverride` de X1; susurro, la aspiración de `Glottis`.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. VOZ-2 — el cantante v8 y las sílabas (estilos de voz de Zig).

Contexto: VOZ-1 cerrada el 2026-11-14 (commit en main): el fork de Pink Trombone
(Assets/PinkTrombonePOC/PinkTromboneSrc) tiene X1 constricciones + velo, X2 longitud
del tracto y X3 growl/aspereza/aire (POC-FORK 8–11), neutros por defecto (byte a byte
iguales a antes) y con paridad JS = C# de −154,8 a −158,6 dB en 9 casos. VoiceProfileSO
v2 tiene los campos del taller (tractLength, breath, growl, roughness, drive, scoop,
fall, glide, attackPress, legato, baseVowel) y engine = V7_CLite. Ahora toca que el
juego cante como cantarV2 del Taller de melodías.

Decisiones cerradas (D-VOZ-1..7, ver ARRANQUE_estilos_de_voz.md):
- D-VOZ-1 = A: portar cantarV2 a C# como cantante v8, con paridad JS = C#.
- D-VOZ-4 = A: SyllableBankSO aparte (zigués por semilla y dureza, lista ponderada,
  texto); cada VoiceProfileSO apunta a un banco.
- D-VOZ-6 = A: VoiceRenderer en C# puro (sin UnityEngine), el mismo para SingerVoice
  (tiempo real) y para el editor (offline, VOZ-3); paridad con dotnet.
- D-VOZ-7 = B con opt-out: el v8 entra en la demo; engine = V7_CLite es el interruptor.

Abiertas al empezar (preséntalas en formato D antes de construir):
- Fin de frase en el juego: el stem MIDI no trae finFrase ni firma. Regla para deducirlo
  (silencio después ≥ X, última nota del loop, nota larga…) y cómo se iguala en la
  paridad (el JS recibe las mismas notas con la bandera ya calculada).
- Semillas del zigués: propuesta del wrapped = léxico desde la semilla del banco (el
  idioma de Zig, igual en todas las canciones) y verso desde la semilla de la canción.
  SingerLoopContext no trae semilla hoy: de dónde sale (MGP-REPRO-1 / CompositionSession).
- Bloque de control: cantarV2 decide cada 256 muestras; OnAudioFilterRead llega en
  bloques del tamaño del buffer de DSP. Propuesta: el renderer trocea siempre en 256
  internamente, así tiempo real = offline.
- La C-lite de v7 (M1–M3, lead por intervalo): ¿desaparece en v8 o se combina?
  (cantarV2 no la tiene.)
- Volumen: el taller normaliza el RMS entre modos; en el juego manda gain. ¿Se calibra
  un gain por perfil al importar (VOZ-3) o el renderer normaliza?

Inputs (en el repo; leer del disco):
1. Docs/reference/taller-melodia/motor-canto.js: cantarV2, parseSilaba, CONS, VOCALES,
   zigues, asignarZigues, asignarSilabas, BASE, MODOS.
2. Docs/reference/taller-melodia/motor-voz.js y paridad/ (casos.mjs, Paridad.cs: molde
   del arnés; el .csproj compila las fuentes reales del fork).
3. Assets/Scripts/Music/Voice/SingerVoice.cs (v7: ArmFromStemBytes, OnAudioFilterRead,
   tone stage) y SingerVoiceDirector.cs (SingerLoopContext, arming por loop).
4. Assets/Scripts/Data/Audio/VoiceProfileSO.cs (v2; archivo en Windows-1252: editar con
   un script que respete la codificación).
5. Assets/PinkTrombonePOC/PinkTromboneSrc/FORK_NOTES.md (POC-FORK 8–11, API nueva).
6. Docs/systems/SSoT_Singer_Voice.md (§3.6, §5, §8).
7. Docs/planning/active/ARRANQUE_estilos_de_voz.md (hallazgos y decisiones).
8. Wrapped: https://claude.ai/artifact/2531U873oDKsJNZ2fTyep7 (§6, cómo funciona el zigués).

Tarea:
1. /buscar-referencias --all sobre el problema de la sesión; enseñar las 1–5 mejores.
2. Decisiones abiertas en formato D; esperar a Claudio.
3. VoiceRenderer (C# puro): port de cantarV2 (sílabas, consonantes con X1, diptongos,
   «mm», coda, scoop, caída, glide, pitch lead, ataque, legato, vibrato con retraso y
   rampa, tone stage de dos polos por brillo, saturación tanh), troceado en 256.
4. SyllableBankSO (+ referencia en VoiceProfileSO) y el zigués en C# (FNV + mulberry32,
   mismas palabras que el taller con la misma semilla y dureza).
5. Fin de frase deducido del stem según la decisión.
6. Paridad: casos de render completo (modos del taller × melodías × semillas) JS = C#,
   criterio < −60 dB como el motor; más paridad de zigués (léxico y reparto idénticos).
7. SingerVoice usa el VoiceRenderer cuando engine = V8_Workshop; con V7_CLite, sin cambios.
   Debug.Log con etiqueta [VOZ-2] al armar (engine, banco, semillas, notas, fin de frase).
8. Un perfil de prueba (p. ej. VoiceProfile_Zig_V8) para los smokes, sin tocar el de Zig
   hasta VOZ-3.

Restricciones:
- Frontera: nada de MidiGenPlay se edita; la voz es de ALWTTT.
- Con engine = V7_CLite, el juego suena idéntico (regresión obligatoria).
- Sin asignaciones de memoria en el hilo de audio; el armado (parseo y sílabas) en el hilo
  principal, como hoy.
- Nunca escribir .meta a mano. Hay trabajo en paralelo de otra sesión en el árbol:
  hacer commit solo de los archivos de esta tanda.

Entregables: VoiceRenderer, SyllableBankSO, cambios en SingerVoice/Director/VoiceProfileSO,
arnés de paridad ampliado con resultados, smokes con sus líneas [VOZ-2], docs propuestos
(SSoT_Singer_Voice §5 y §8, CURRENT_STATE, changelog, FORK_NOTES si se toca el fork).

Cierre: compila (Claudio); paridad verde; smokes de oído jugados por Claudio con el
perfil V8 (sílabas en zigués audibles, consonantes, un modo extremo como death o punk) y
la regresión V7 idéntica; CPU de una voz v8 anotada (profiler en el editor; la medida en
build va en VOZ-3); docs aplicados; commit; arranque de VOZ-3 (importador TV1/TM1,
«▶ con voz» en el Banco de cartas, botón TV1 en el taller, Zig a V8, CPU en build).
```

## Notas para quien abra la sesión

- El JS de cantarV2 trabaja con notas en segundos (`aSegundos`) y bloques de 256; el juego tiene el MIDI del loop y el BPM. La paridad tiene que alimentar a los dos lados con la misma lista de notas (start, end, note, vel, sil, caida).
- `cantarV2` mezcla volumen con `normalizar` solo en la página; no es parte del motor.
- `TurbulencePoint.Position` ya es `double` y está en el espacio del tracto (`índice × n/44`), como en el JS.

## Estado al 2026-11-14 (sesión VOZ-2, sin cerrar: faltan los smokes de Claudio)

Decisiones de la sesión: D1 = B (fin de frase: silencio ≥ 1 tiempo, nota ≥ 2 tiempos o última; rejilla opcional `phraseGridBars`), D2 = A (léxico del banco, verso de `hash(songSeed|part)`, ruido de `songSeed`), D3 = A (bloques de 256 internos), D4 = A (sin C-lite en v8), D5 = A (gain por perfil igualando el RMS de la Zig de hoy), D6 = B (paleta `VoiceStylePaletteSO` en la carta de melodía), D7 = A (la paleta entra en VOZ-2). Además: perillas en vivo (el renderer lee el perfil en cada bloque).

Hecho:
- `Assets/Scripts/Music/Voice/V8/`: `VoiceRenderer` (cantarV2 portado, C# puro, sin asignaciones en el hilo de audio, `StartAt` para el ancla), `VoiceV8Params` (perillas + `Live*`), `VoiceV8Syllables` (fonética, zigués, lista, texto, fin de frase).
- `SyllableBankSO`, `VoiceStylePaletteSO`; `VoiceProfileSO.syllableBank` y `ToV8Params`; `TrackActionDescriptor.voicePalette`; `SingerLoopContext.songSeed` y `voicePaletteByMusician` (los rellena `CompositionSession`); el Director elige el estilo de la paleta; `SingerVoice` arma V8 o V7 según `engine`, perillas en vivo (`Update`, menú contextual «VOZ-2/…», `SetLive*`), log `[VOZ-2]`.
- Menú `ALWTTT ▸ Voz ▸ Crear estilos del taller (VOZ-2)` (`Assets/Scripts/Music/Voice/Editor/VoiceStylesMenu.cs`): 10 perfiles V8, bancos zigués y de lista, dos paletas, en `Voice Profiles/Estilos|Bancos|Paletas`.
- Paridad `Docs/reference/taller-melodia/paridad/cantante/` (`node casos.mjs && dotnet run -c Release && node casos.mjs comparar`): 13 casos, sílabas, léxico y fin de frase idénticos, audio de −114 a −163 dB; búfer de 256/441/1024 idéntico; perillas en vivo audibles al bloque siguiente; CPU en dotnet 4–7 % de tiempo real por voz.
- Regla de fin de frase medida (`medir-regla.mjs`): B 30 % de precisión / 100 % de exhaustividad; F (B + compás par) 68 % / 100 %; G (B + cada 4 compases) 100 % / 100 % pero atada a la frase de 4 compases del taller.
- Compilación comprobada fuera de Unity con una copia temporal de `Assembly-CSharp.csproj` y `Assembly-CSharp-Editor.csproj` (Build succeeded).

D8 = B (Claudio, 2026-11-14): regla F por defecto (`SyllableBankSO.phraseGridBars = 2`).

## Cerrada el 2026-11-16

Smokes 0–8 jugados por Claudio: V7 igual que antes, V8 con sílabas y consonantes, perillas en vivo, paleta en la carta (`[palette …]` frente a `[musician]`), la misma semilla da la misma voz (líneas `[VOZ-2] Armed V8` idénticas), CPU en el editor 12,7 % / pico 20,1 % (Crooner) y 13,2 % / 22,5 % (Death metal). Hallazgo de los smokes: la paleta de la carta manda sobre el perfil del Director (es lo diseñado, pero confunde al probar un perfil). D9 = A: Zig_V8 por defecto en el prefab y ninguna carta con paleta. Docs: `SSoT_Singer_Voice` §5.1 nueva, `CURRENT_STATE`, `changelog-ssot`. Siguiente: `ARRANQUE_VOZ-3.md`.

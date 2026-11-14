# Arranque: VOZ-2, el cantante v8 y las sílabas (estilos de voz)

Segunda de tres tandas del plan de estilos de voz (`ARRANQUE_estilos_de_voz.md`, D-VOZ-1..7). VOZ-1 cerró el 2026-11-14: el fork de Pink Trombone ya sabe X1–X3 (POC-FORK 8–11) con paridad JS = C#, y `VoiceProfileSO` v2 tiene los campos del taller con `engine = V7_CLite`. Compila y Zig suena igual (Claudio). Planificación, no autoridad.

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

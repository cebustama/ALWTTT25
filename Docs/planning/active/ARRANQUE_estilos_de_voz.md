# Arranque: estilos de voz como assets (Zig)

Nace de la nota de Claudio a D-MELFIG-8 (2026-11-14, sesión de melodías por figura de MidiGenPlay): «Qué tan complejo sería agregar nuevos estilos para la voz? ¿Serán assets (SOs)? Me gustaría poder probar y agregar nuevos estilos de voz fácilmente. Lo mismo con las sílabas; me da curiosidad el potencial del motor de Pink Trombone.» **Claudio pidió priorizarlo:** va como siguiente sesión extra, por delante de F2–F3 de después de la demo. Planificación, no autoridad.

## Pegar en la sesión nueva

> Lee y sigue `Docs/planning/active/ARRANQUE_estilos_de_voz.md`. Objetivo: que añadir y probar un estilo de voz de Zig sea fácil: estilos como assets, probados primero en el Taller de melodías y llevados al juego con un código, como TB1 y TM1. Empieza leyendo lo que existe y preséntame las decisiones en formato D antes de construir.

## Lo que hay (releer antes de afirmar)

- `Assets/Scripts/Data/Audio/VoiceProfileSO.cs`: ya es un asset. Las perillas ★ del taller existen en él (looseness, vibrato, tenseness, brightness, transpose, pitch lead, etc.).
- `SingerVoice.cs` v7 (port de Pink Trombone, POC-FORK 1–7); prefab `Zig_SingerVoice` con `expressivityEnabled = 0`.
- Taller de melodías (`Docs/reference/taller-melodia/`): 11 modos (Tierno, Pop, Punk, Death metal, Ópera, Crooner, Falsete, Robot, Borracho…), extensiones X1 (consonantes), X2 (longitud del tracto), X3 (growl, aspereza, aire), scoop, caída, glide, ataque, legato, saturación, vocal base; zigués; «Copiar para la carta» ya da `VoiceProfileSO` + `propuestaVoiceProfileV2`.
- Wrapped de la voz con la propuesta F0–F4 y D-ZIG-1..4: https://claude.ai/artifact/GGpSCDLpNzdx7hbDfqiUf5 (`Docs/reference/zig-voz-wrapped/`). F0 (dicción) va en S9c.

## Lo que hay que decidir

1. **Corte rápido:** modos que solo usan perillas existentes = assets nuevos sin código, más un importador del JSON del taller. ¿Cuántos modos del taller caben así?
2. **`VoiceProfileSO` v2:** qué perillas de X1–X3 entran al C# y en qué orden (cada una con paridad JS = C#, como el motor de voz: −155 dB).
3. **Sílabas:** zigués y bancos de sílabas como asset propio (`SyllableBankSO`) o dentro del perfil.
4. **Coste:** ~10,5 % de CPU por voz en build; qué extensiones lo suben.
5. **Cuándo entra en la demo:** solo lo que pase la prueba de oído con Matías.

## Hallazgos de la lectura (2026-11-14)

- **Los modos del taller no suenan con el cantante del juego.** El taller tiene dos cantantes: `cantarJuego` (= `SingerVoice` v7) y `cantarV2` (propuesta). Los modos usan `cantarV2`: sílabas con consonantes, scoop, caída, glide, ataque, legato, saturación, X2 y X3. Con las perillas ★ de hoy solo Robot se acerca; el «corte rápido» de solo assets no sirve.
- `vibratoDepth` está limitado a 0–1 en C#; Ópera usa 2,6, Crooner 1,4 y Borracho 1,2 («Copiar para la carta» lo recorta en silencio).
- El fork C# (`Assets/PinkTrombonePOC/PinkTromboneSrc`) no tiene X1 (constricciones), X2 (longitud del tracto) ni X3 (growl, aspereza, aire). Sin X1 no hay consonantes: la C-lite solo mueve vocales.
- El código TM1 ya lleva `voz` + `silabas` completos. El importador de MidiGenPlay los ignora (bien: `VoiceProfileSO` es de ALWTTT; el importador de voz va de este lado de la frontera).
- El cantante trabaja por bloques de 1024 y el fork es C# puro: un render offline con el mismo código es el mismo sonido que en el gig.
- En el juego las notas llegan de un stem MIDI, sin `finFrase` ni `firma`. El zigués los usa para cortar palabras: el C# tendrá que deducir el fin de frase del stem (silencio después o nota larga) y la paridad se mide sobre las mismas entradas.

## Decisiones (Claudio, 2026-11-14)

- **D-VOZ-1 = A.** El juego cubre lo que ya hace `cantarV2`: se porta a C# como cantante v8, con paridad JS = C#.
- **D-VOZ-2 = A.** `VoiceProfileSO` v2: el mismo asset con los campos nuevos; con valores neutros, byte-idéntico a hoy. `vibratoDepth` pasa a 0–3.
- **D-VOZ-3 = A.** Orden: X1 (consonantes) → X2 (tracto) → X3 (growl, aspereza, aire) en el fork; el fraseo (scoop, caída, glide, ataque, legato, saturación) en el cantante v8.
- **D-VOZ-4 = A.** `SyllableBankSO` aparte (zigués por semilla y dureza, lista ponderada o texto); cada perfil apunta a un banco.
- **D-VOZ-5 = A.** Importador de ALWTTT que acepta `TV1:` (solo voz y sílabas, botón nuevo en el taller), `TM1:` y el JSON de «Copiar para la carta»: normalizar → vista previa → aplicar, con Undo y «usado por».
- **D-VOZ-6 = A.** `VoiceRenderer` puro (sin UnityEngine), compartido por `SingerVoice` (tiempo real) y el editor (offline); «▶ con voz» y `.wav` en el Banco de cartas; paridad con dotnet.
- **D-VOZ-7 = B con opt-out.** El v8 entra en la demo (Matías ya aprobó la voz nueva), con un interruptor para volver al v7 + dicción si algo falla (oído o CPU en build).

## Tandas

1. **VOZ-1, el fork:** X1–X3 en C# con paridad; `vibratoDepth` 0–3; `VoiceProfileSO` v2 con campos neutros. **Hecho en código el 2026-11-14** (POC-FORK 8–11; paridad de 9 casos entre −154,8 y −158,6 dB; con todo neutro, idéntico byte a byte a antes). Falta que compile en Unity (Claudio). Hallazgo: el setter de `TurbulencePoint.Position` lanzaba siempre, así que las fricativas nunca habían funcionado en C# (POC-FORK 8).
2. **VOZ-2, cantante y sílabas:** `VoiceRenderer` con `cantarV2` portado; `SyllableBankSO` con el zigués; fin de frase deducido del stem; paridad del render completo. **Cerrada el 2026-11-16**: además, paletas de estilos en la carta (D6 = B) y perillas en vivo; Zig canta V8 por defecto; smokes 0–8 jugados por Claudio (`archive/ARRANQUE_VOZ-2.md`).
3. **VOZ-3, el flujo:** importador TV1/TM1, «▶ con voz» en el Banco, botón TV1 en el taller, interruptor v7/v8, medición de CPU en build.

Wrapped con el contexto y el plan: https://claude.ai/artifact/2531U873oDKsJNZ2fTyep7 (fuente `Docs/reference/pink-trombone-wrapped/`; incluye un generador de zigués en vivo, copia literal de `motor-canto.js`).

## Referencias (`/buscar-referencias --all`, 2026-11-14)

- **Chat de ChatGPT «Juegos con voces sintetizadas»** (2025-12-12, `D:\Projects\chats-chatgpt\textos\2363.txt`): criterio de Claudio, «no quiero que suene tan ridículo, pero lo más importante es que suene bien»; descarta Animalese puro y Banjo-Kazooie. Separa vocales afinadas (ah/oo/mm) de sílabas con consonantes, que pedían otro componente: hoy, X1. Vale como regla de aprobación de un estilo.
- **La Diva de *El quinto elemento*** (`C:\Users\cebus\Dropbox\ALWTTT References\Venues\Screenshot 2025-06-30 175728.png`, catálogo n.º 318): ópera extraterrestre ante un planeta; referencia del modo Ópera y del zigués (idioma inventado que sigue sonando a canto).
- **Marcador «Mejorar expresividad vocal en Pink Trombone»** (chat de Claude, 2026-09-05; marcadores n.º 69): origen de la C-lite. No se pudo abrir.
- Sin resultados de Simlish ni Animal Crossing en las referencias de ALWTTT.
- Idea apuntada (no es de esta tanda): los gritos del público (GRITOS-1) y las risas o abucheos con la misma garganta y perfiles propios.

# Arranque: VOZ-3, el flujo de los estilos de voz (del taller al juego y de vuelta)

Tercera y última tanda del plan de estilos de voz (`ARRANQUE_estilos_de_voz.md`, D-VOZ-1..7). VOZ-2 cerró el 2026-11-16: Zig canta en el juego como el cantante v2 del Taller de melodías (zigués con consonantes, scoop, caída, glide, growl), con 10 estilos como assets, paletas en la carta y perillas en vivo. Planificación, no autoridad.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. VOZ-3 — el flujo de los estilos de voz (taller ↔ juego).

Contexto: VOZ-2 cerrada el 2026-11-16. El juego tiene el cantante v8: VoiceRenderer
(C# puro, port de cantarV2 con paridad JS = C# en 13 casos), SyllableBankSO (zigués,
lista, texto), VoiceStylePaletteSO (la carta de melodía sortea el estilo por canción y
parte), perillas en vivo y 10 estilos creados por menú (ALWTTT ▸ Voz ▸ Crear estilos del
taller). Zig canta V8 por defecto (prefab con VoiceProfile_Zig_V8; la vuelta atrás es
VoiceProfile_Zig). Ninguna carta lleva paleta. CPU en el editor: 13,2 % / pico 22,5 %
con Death metal. Hoy, crear o ajustar un estilo exige tocar el asset a mano en Unity y
jugar un gig para oírlo: VOZ-3 cierra ese ciclo.

Decisiones cerradas (D-VOZ-1..7 en ARRANQUE_estilos_de_voz.md; VOZ-2 en
archive/ARRANQUE_VOZ-2.md):
- D-VOZ-5 = A: importador en ALWTTT que acepta TV1: (voz y sílabas), TM1: y el JSON de
  «Copiar para la carta»: normalizar → vista previa → aplicar, con Undo y «usado por».
- D-VOZ-6 = A: el mismo VoiceRenderer en tiempo real y offline (editor).
- D-VOZ-7 = B con opt-out: el v8 entra en la demo; el interruptor es el perfil del Director.

Abiertas al empezar (preséntalas en formato D antes de construir):
- Formato TV1: qué lleva exactamente (todos los campos del perfil v2 + banco: modo,
  lexiconSeed, dureza, lista o texto; ¿gain calibrado o lo calcula el importador?).
  Versionado y compatibilidad con el JSON actual (propuestaVoiceProfileV2 del taller).
- El importador ¿crea un perfil nuevo o sobrescribe uno existente? Nombres y carpeta
  (Voice Profiles/Estilos y Bancos), y qué pasa con las paletas que ya lo usan.
- Calibrar el gain al importar: ¿port de gen-estilos (RMS contra la Zig v7) en C# o
  un gain fijo que el taller ya trae?
- «▶ con voz» en el Banco de cartas: render offline de la parte A con el stem de Zig
  por el VoiceRenderer y mezcla con el MIDI de Maestro, o solo voz + .wav.
- Qué carta lleva qué paleta en la demo (decisión de diseño con Matías; puede quedar
  en «ninguna»).

Inputs (en el repo; leer del disco):
1. Docs/systems/SSoT_Singer_Voice.md (§5, §5.1, §6).
2. Docs/planning/active/ARRANQUE_estilos_de_voz.md y Docs/planning/archive/ARRANQUE_VOZ-2.md.
3. Assets/Scripts/Music/Voice/V8/*.cs, SingerVoice.cs (ArmV8), SingerVoiceDirector.cs.
4. Assets/Scripts/Data/Audio/{VoiceProfileSO (Windows-1252: editar con script que respete
   la codificación), SyllableBankSO, VoiceStylePaletteSO}.cs.
5. Assets/Scripts/Music/Voice/Editor/VoiceStylesMenu.cs (molde de creación de assets).
6. Docs/reference/taller-melodia/index.html («Copiar para la carta», ~línea 2945),
   motor-canto.js (MODOS, BASE), build.py, paridad/cantante/gen-estilos.mjs.
7. La ventana del Banco de cartas (ALWTTT ▸ Banco de cartas, MGP-BENCH-1) y el importador
   TM1 de MidiGenPlay (patrón vista previa → aplicar; solo lectura, no se edita el paquete).

Tarea:
1. /buscar-referencias --all sobre el problema (importar estilos desde una herramienta web,
   escuchar en el editor sin jugar); enseñar las 1–5 mejores.
2. Decisiones abiertas en formato D; esperar a Claudio.
3. Botón «Copiar TV1» en el Taller de melodías (index.html vía template/build) con el formato
   decidido; el artefacto y la copia de la web se republican.
4. Importador en ALWTTT (ventana de editor): pegar TV1 / TM1 / JSON → vista previa (campos,
   banco, léxico de muestra) → aplicar con Undo; «usado por» (paletas y prefabs que lo usan).
5. «▶ con voz» y «.wav» en el Banco de cartas con el VoiceRenderer offline.
6. Paridad: lo importado suena igual que en el taller (render offline del juego = JS).
7. Medir la CPU de una voz V8 en una build (Death metal, el estilo más pesado) y anotarla.
8. Debug.Log con la etiqueta [VOZ-3] en el importador y en el render offline.

Restricciones:
- Frontera: nada de MidiGenPlay se edita.
- Sin asignaciones en el hilo de audio; el render offline en el hilo principal del editor.
- Nunca escribir .meta a mano. Hay trabajo de otra sesión en el árbol: commit solo de
  los archivos de esta tanda.

Entregables: botón TV1 en el taller, importador, «▶ con voz» y .wav, paridad del importado,
CPU en build, smokes con sus líneas [VOZ-3], docs propuestos (SSoT_Singer_Voice §5.1 y §8,
SSoT_Editor_Authoring_Tools, CURRENT_STATE, changelog).

Cierre: compila (Claudio); un estilo creado en el taller se importa y suena igual en el
Banco y en el gig; CPU en build anotada (si pasa de lo aceptable, Zig vuelve a v7 para la
demo); docs aplicados; commit; cierre del plan de estilos de voz.
```

## Notas para quien abra la sesión

- El juego ya tiene todo lo que el importador escribe: `VoiceProfileSO` v2 + `syllableBank`, `SyllableBankSO`, `VoiceStylePaletteSO`. `VoiceStylesMenu.cs` crea esos assets desde una tabla: el importador es lo mismo con la tabla sacada de un código.
- La paridad de VOZ-2 (`paridad/cantante/`) ya compila el renderer en dotnet: sirve de base para la paridad de lo importado.
- Smokes de VOZ-2: una carta con paleta manda sobre el perfil del Director. Para oír un perfil concreto, la carta no debe tener paleta.
- Ideas que no son de esta tanda (apuntadas): consumidores de las perillas en vivo (Stress cierra la garganta, el vibrato sube con la Inspiración), Zig a dos voces (DUET-1, POST-DEMO), gritos del público con la misma garganta.

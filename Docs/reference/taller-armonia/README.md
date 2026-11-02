# Taller armónico

Página privada para escuchar progresiones antes de crear assets (creada el 2026-11-02, tras la prueba de escucha v1). Enlace: ver `_comun/foco.md` o el resumen de la sesión.

- **Entrada:** la cadena del Import del Chord Progression Editor. El parser de la página imita `RomanProgressionParser` y la lista blanca `AllowedSuffixes` de `ChordProgressionRuntimeImporter` (MidiGenPlay, 17 calidades: también 6, m6, 7sus4, 9, maj9 y m9). Las alteraciones se aplican al grado **del modo**, como en `BassTrackComposer` (`TransposeNoteName(scaleNames[degree], degreeAccidental)`): en Eólico, `bVII` suena como VI. La página lo avisa.
- **Controles:** modo (Reference tonality), tónica (solo para oír), compás 4/4 o 3/4, vueltas, toque del teclado, batería, BPM.
- **Niveles (propuesta D6, sin implementar en el juego; v3 tras la revisión de Music Lab, `revision-music-lab-2026-11-02.md`):** reglas deterministas sobre la progresión escrita.
  - Reglas comunes: ningún acorde escrito desaparece (lo añadido ocupa solo la segunda parte de un acorde: medio compás en 4/4, un compás en 3/4); nada cambia en la vuelta 1; sin modulación dentro de la carta.
  - Nivel 1: tal cual.
  - Nivel 2: cuatriadas de la escala; desde la vuelta 2, **un** adorno por vuelta: en la última, vuelta al primer acorde (salvo si el bucle ya cierra con cadencia V→I, IV→I, ♭VII→I o ♭II→i, o si tiene ≤2 acordes); si no, sus4 en la dominante funcional; si no, una dominante secundaria hacia un acorde diatónico que no sea la tónica.
  - Nivel 3: novenas solo si la novena está en la escala; desde la vuelta 2, **hasta dos** cambios por vuelta (uno si el bucle tiene ≤2 acordes), por orden: vuelta al inicio (Jónico: backdoor iv7–♭VII9; Eólico: iiø7–♭II7; Frigio ♭IImaj7, Dórico IV7, Lidio II7, Mixolidio ♭VII; Locrio nada), un préstamo tras el IV (iv6 en Jónico, Lidio y Mixolidio; IV7 en Eólico), sustituto tritonal hacia un acorde diatónico que no sea la tónica, ♭VImaj7 antes de volver a I (Jónico y Mixolidio, si no hay ya cadencia).
  - Pendiente (decisión B): perfiles de estilo por carta (blues, riff, pop, jazz).
- **Salida:** el bloque «setup card + Roman block» con sufijos explícitos, listo para el Import. Con nivel 2 o 3 copia todas las vueltas como un asset aparte.
- **Catálogo:** las 24 progresiones de la prueba (eventos reales de los assets y candidatas) y 9 ejemplos de estilo propuestos por el agente (✦), con grados y fuentes revisados por Music Lab. Lo que se guarda desde la página va al db del artefacto, colección `catalogo`.
- **Archivos:** `template.html` + `build.py` → `index.html`. `build.py` lee `../prueba-escucha/listen_clips.json`.

No es el sonido de MidiGenPlay: para fidelidad, *MidiGenPlay → Smoke → Composition Smoke*.

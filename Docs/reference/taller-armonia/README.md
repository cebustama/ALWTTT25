# Taller armónico

Página privada para escuchar progresiones antes de crear assets (creada el 2026-11-02, tras la prueba de escucha v1). Enlace: ver `_comun/foco.md` o el resumen de la sesión.

- **Entrada:** la cadena del Import del Chord Progression Editor. El parser de la página imita `RomanProgressionParser` y la lista blanca `AllowedSuffixes` de `ChordProgressionRuntimeImporter` (MidiGenPlay, 17 calidades: también 6, m6, 7sus4, 9, maj9 y m9). Las alteraciones se aplican al grado **del modo**, como en `BassTrackComposer` (`TransposeNoteName(scaleNames[degree], degreeAccidental)`): en Eólico, `bVII` suena como VI. La página lo avisa.
- **Controles:** modo (Reference tonality), tónica (solo para oír), compás 4/4 o 3/4, vueltas, toque del teclado, batería, BPM.
- **Niveles (propuesta D6, sin implementar en el juego):** reglas deterministas sobre la progresión escrita.
  - Nivel 1: tal cual.
  - Nivel 2: cuatriadas de la escala; desde la vuelta 2, dominantes secundarias en posiciones impares, sus4 antes del V7; ii–V de vuelta al final.
  - Nivel 3: novenas; desde la vuelta 2, sustitutos tritonales, iv6 prestado (o IV7 dórico), mediante cromática; backdoor al final; la última vuelta sube medio tono.
- **Salida:** el bloque «setup card + Roman block» con sufijos explícitos, listo para el Import. Con nivel 2 o 3 copia todas las vueltas como un asset aparte.
- **Catálogo:** las 24 progresiones de la prueba (eventos reales de los assets y candidatas) y 9 ejemplos de estilo propuestos por el agente (✦, fuentes por comprobar). Lo que se guarda desde la página va al db del artefacto, colección `catalogo`.
- **Archivos:** `template.html` + `build.py` → `index.html`. `build.py` lee `../prueba-escucha/listen_clips.json`.

No es el sonido de MidiGenPlay: para fidelidad, *MidiGenPlay → Smoke → Composition Smoke*.

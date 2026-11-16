# Arranque: TALLER-MEL-B, silabeador español y articulación de las consonantes

Segunda sesión del Taller de melodías (solo JS, `Docs/reference/taller-melodia/`). Viene después de TALLER-MEL-A, cerrada el 2026-11-16, y antes de C (melodías v2) y de D (progresiones del taller armónico). Pertenece a la línea de la voz, paralela a la demo: no pasa por delante de CARTAS-ARTE-1. Es planificación, no autoridad.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. TALLER-MEL-B — Taller de melodías: silabeador español (rr, ch)
y perilla de articulación para las consonantes (solo JS, sin tocar C#).

Leer primero:
- ALWTTT25/Docs/planning/active/ARRANQUE_TALLER-MEL-B.md (este arranque)
- ALWTTT25/Docs/reference/taller-melodia/README.md: los apartados de los modos nuevos
  y «Consonantes que se pierden»
- ALWTTT25/Docs/reference/ideas/2026-11-16_melodias_antiguas.md, «Sílabas y palabras»
- motor-voz.js: Glottis (c013/c005, intensity), fricationNoiseSource
  (createFilteredNoiseSource(1000, 0.5…)), transients
- motor-canto.js: CONS / parseSilaba, cantarV2 (preDe, turbulencia, pasa-bajos de brillo)

Contexto: en TALLER-MEL-A (ALWTTT 270da40…e70709a) entraron la biblioteca de
melodías, los 9 estilos de sílabas y 5 modos (susurro, garganta con realce y bordón
−24, nasal, canto llano, yodel por registro). En la prueba de oído, Claudio vio que
la «s» de «si» suena «ii», la c/k se pierde y la «t» suena entre «d» y «l».
Diagnóstico (D6 = A, va aquí):
(1) la voz tarda ~0,2 s en apagarse y ~0,08 s en encenderse, y las consonantes duran
75–100 ms, así que las sordas salen sonoras;
(2) el ruido de fricción está centrado en ~1 kHz, cuando una «s» real vive entre
4 y 8 kHz;
(3) el pasa-bajos de brillo corta hacia 3,9 kHz en Pop.

Construir:
1. Silabeador español: texto → sílabas (español casi 1:1 de letra a sonido),
   con rr (vibrar la punta), ch (t + sh), ll/y, qu/gu, c/z ante e/i, h muda.
   Se usa en «texto propio» y «letra original».
2. Perilla `articulacion` (0 = el motor de hoy, byte a byte):
   - ensordecer rápido en las sordas (p, t, k, s, f, sh, j);
   - ruido agudo para s, z y sh (una segunda fuente de ruido de 4–7 kHz, o desplazar
     la banda), sin que el pasa-bajos de brillo la corte;
   - soplo de aspiración breve al soltar p, t, k;
   - quizá alargar la ventana de la consonante cuando hay sitio.
   Medir: energía de 4–8 kHz en la «s», sonoridad (intensity) durante la t/k,
   antes y después.
3. Elegir su valor por modo (de oído, con Claudio) y añadirla a NUEVAS
   (solo taller hasta VOZ-2b).

Al empezar: /buscar-referencias --all (síntesis articulatoria de consonantes, fricativas,
VOT, silabeador español).

Abiertas (formato D antes de construir):
- D1: ¿El ruido agudo se hace con una segunda fuente en el motor (cambia motor-voz.js,
  cuidando la paridad 9/9) o filtrando aparte en cantarV2?
- D2: ¿La articulación entra también en el juego en VOZ-2b o queda solo en el taller?

Restricciones: no tocar C# ni assets de Unity; nunca escribir .meta; neutro byte a byte
(base.cjs: los 9 modos viejos × 3 melodías, Zig hoy, TV1 y gain); paridad del motor 9/9
y del TV1 13/13; commit solo de los archivos del taller y de la web; push y artefacto
solo con OK de Claudio.

DoD: «si», «ka» y «ta» se entienden de oído (Claudio); medición antes/después;
silabeador con rr y ch; neutro idéntico; README y build.py --web.
```

## Después

C (melodías v2: Würfelspiel, *Hanacpachap*, Caravaggio, *Ut queant laxis*, otra lectura del himno hurrita; quizá kargyraa como modo), D (progresiones del taller armónico) y VOZ-2b en C# después de la demo.

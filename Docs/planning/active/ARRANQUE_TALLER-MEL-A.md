# Arranque: TALLER-MEL-A, biblioteca de melodías, sílabas y cinco estilos de voz

Sesión del Taller de melodías (solo JS, `Docs/reference/taller-melodia/`), primera de tres (A → B silabeador español → C melodías v2). Propuesta y decisiones del 2026-11-16 en `Docs/reference/ideas/2026-11-16_melodias_antiguas.md`, sección «v2». Línea de la voz, paralela a la demo: no pasa por delante de CARTAS-ARTE-1. Planificación, no autoridad.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. TALLER-MEL-A — Taller de melodías: biblioteca de melodías,
sílabas en botones y cinco estilos de voz nuevos (solo JS, sin tocar C#).

Leer primero:
- ALWTTT25/Docs/reference/taller-melodia/README.md (qué hay y cómo se construye)
- ALWTTT25/Docs/reference/ideas/2026-11-16_melodias_antiguas.md, sección «v2»
- motor-canto.js: MODOS (~l. 345), CONS/VOCALES/parseSilaba (~l. 210), cantarV2,
  MELODIAS_FIJAS / melodiaFija
- template.html: selector de melodía (optgroups «Recetas» y «Melodías antiguas»,
  ~l. 360), chips de modos (#modos), k_silMode (~l. 167), estado()/aplicar() (~l. 733)

Contexto: el 2026-11-16 entraron tres melodías fijas (Seikilos, himno hurrita en la
lectura de Vitale, el Bosco de Hamrick) con ficha y letra; web 663836a subida; el
artefacto WRdHN5Gf839qotLhrqvyLj sigue en la versión anterior (Claudio eligió solo web).

Construir:
1. Biblioteca de melodías en lugar del desplegable: sección con categorías en chips
   (Del juego, Creadas/recetas, Antiguas; preparadas para Medievales, América, Asia,
   Clásicas, En cuadros), buscador y tarjetas (nombre, época, región, compás, ✓ letra,
   insignia de licencia 🟢 libre / 🟡 lectura moderna / 🔴 solo taller). Datos nuevos en
   MELODIAS_FIJAS: categoria, epoca, region, licencia (+ motivo). Seikilos 🟢,
   hurrita 🟡 (ritmo de Vitale), Bosco 🟡 (transcripción de Hamrick). Teclado (M) y
   código TM1 siguen funcionando; estado() guarda la elección.
2. Sílabas en botones (sección propia): zigués, del modo, letra original, texto propio,
   solfeo (do re mi fa sol la si según el grado), scat (du bi du ba…), konnakol
   (ta ka di mi por grupos de la nota en el pulso), boca cerrada (mm), vocal sola.
   Línea con las sílabas que van a sonar bajo el roll. Mantener k_silMode como dato.
   «Letra original» solo se enciende en melodías con letra.
3. Cinco modos nuevos en MODOS, con nota y fuente (verificar fuentes al escribirlas):
   - Susurro: perilla nueva `sonoridad` (0 = solo aire de aspiración).
   - Canto de garganta: bordón fijo + perilla «lengua → armónico» (la lengua se mueve
     para que F2 caiga en el armónico 6–12 que pide la melodía). Medir con
     paridad/formantes.mjs que el armónico sube de verdad.
   - Nasal (twang): perilla `nasalidad` (velo abierto todo el tiempo; usa el mismo
     velumOverride de X1 que ya existe en C#).
   - Canto llano: con las perillas que hay (sin vibrato ni scoop, legato 1, tracto 48,
     sílabas latinas).
   - Yodel: perilla `quiebre` (en saltos ≥ 5 semitonos, salto de tensión y un crack
     corto de altura).
   Neutro en las perillas nuevas = el motor de hoy, byte a byte (comprobar con
   paridad/casos.mjs y un render de cada modo viejo antes/después).

Abiertas (formato D antes de construir):
- ¿La biblioteca sustituye también a las recetas de figura o solo agrupa?
- Konnakol: ¿por pulsos de la melodía o por la agrupación métrica (METRICA-1)?
- Canto de garganta: ¿la melodía se proyecta a los armónicos posibles o se elige un
  bordón y se avisa de las notas que no existen?

DoD: las tres melodías se eligen desde la biblioteca con su insignia; los 9 estilos de
sílabas suenan; los 5 modos nuevos suenan distinto entre sí y de los 11 viejos (oídos
por Claudio); modos viejos idénticos; Node sin NaN; build.py --web; README del taller
al día; commit en ALWTTT y en la web (push solo con OK). Republicar el artefacto solo si
Claudio lo pide (se lee entero antes: es grande).

Después: B (silabeador español con rr y ch), C (melodías v2), VOZ-2b en C# (post-demo).
```

# Arranque: TALLER-MEL-A, biblioteca de melodías, sílabas y cinco estilos de voz

Sesión del Taller de melodías (solo JS, `Docs/reference/taller-melodia/`), primera de tres (A → B silabeador español → C melodías v2). Propuesta y decisiones del 2026-11-16 en `Docs/reference/ideas/2026-11-16_melodias_antiguas.md`, sección «v2». Línea de la voz, paralela a la demo: no pasa por delante de CARTAS-ARTE-1. Planificación, no autoridad.

**Revisado el 2026-11-16 tras VOZ-3 y WALKBASS.** VOZ-3 conectó el taller con el juego por el código TV1 («Copiar TV1» → *ALWTTT ▸ Voz ▸ Importar estilo*). Esta sesión no toca C#, así que todo lo nuevo es **solo del taller** hasta VOZ-2b, y no puede romper ese puente. Riesgos concretos:
- `estiloTV1` exporta **todas** las claves de `BASE`. Si las perillas nuevas entran en `BASE`, el TV1 de **todos** los modos (también los viejos) las llevará, y el importador del juego (`VoiceStyleCode.ApplyVoice`) avisará de «Campo desconocido» en cada importación.
- `SIL_TV1` convierte cualquier modo de sílabas que no conoce en `zigues` **sin avisar**: un estilo con solfeo, scat o konnakol se importaría cantando zigués.
- La paridad del importado (`paridad/cantante/resultados-tv1.json`, 13/13) tiene que seguir igual.

WALKBASS (el canal que el cantante silencia al cambiar de canción, y el registro de los bajos) no afecta al taller.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. TALLER-MEL-A — Taller de melodías: biblioteca de melodías,
sílabas en botones y cinco estilos de voz nuevos (solo JS, sin tocar C#).

Leer primero:
- ALWTTT25/Docs/planning/active/ARRANQUE_TALLER-MEL-A.md (este arranque, con los
  riesgos del TV1)
- ALWTTT25/Docs/reference/taller-melodia/README.md (qué hay y cómo se construye)
- ALWTTT25/Docs/reference/ideas/2026-11-16_melodias_antiguas.md, sección «v2»
- motor-canto.js: MELODIAS_FIJAS / melodiaFija (~l. 179), CONS / parseSilaba
  (~l. 210), MODOS (~l. 345), cantarV2 (~l. 440), estiloTV1 + SIL_TV1 (~l. 570)
- template.html: chips de modos (#modos, ~l. 143), k_silMode (~l. 167), selector de
  melodía (optgroups «Recetas» y «Melodías antiguas», ~l. 360), estado()/aplicar()
  (~l. 736)
- Solo lectura, para saber qué entiende el juego: Assets/Scripts/Music/Voice/V8/
  VoiceStyleCode.cs (ApplyVoice ignora y avisa de los campos desconocidos)

Contexto: el 2026-11-16 entraron tres melodías fijas (Seikilos, himno hurrita en la
lectura de Vitale, el Bosco de Hamrick) con ficha y letra; web 663836a subida.
VOZ-3 (ALWTTT 11c0033, web 412f576) añadió «Copiar TV1» y el importador del juego
(paridad 13/13). El artefacto WRdHN5Gf839qotLhrqvyLj sigue en una versión anterior.

Construir:
1. Biblioteca de melodías en lugar del desplegable: sección con categorías en chips
   (Del juego, Creadas/recetas, Antiguas; preparadas para Medievales, América, Asia,
   Clásicas, En cuadros), buscador y tarjetas (nombre, época, región, compás, ✓ letra,
   insignia de licencia 🟢 libre / 🟡 lectura moderna / 🔴 solo taller). Datos nuevos en
   MELODIAS_FIJAS: categoria, epoca, region, licencia (+ motivo). Seikilos 🟢,
   hurrita 🟡 (ritmo de Vitale), Bosco 🟡 (transcripción de Hamrick). El teclado (M) y
   el código TM1 siguen funcionando; estado() guarda la elección.
2. Sílabas en botones (sección propia): zigués, del modo, letra original, texto propio,
   solfeo (do re mi fa sol la si según el grado), scat (du bi du ba…), konnakol
   (ta ka di mi por grupos de la nota en el pulso), boca cerrada (mm), vocal sola.
   Línea con las sílabas que van a sonar bajo el roll. Mantener k_silMode como dato.
   «Letra original» solo se enciende en melodías con letra.
3. Cinco modos nuevos en MODOS, con nota y fuente (verificar las fuentes al escribirlas):
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
4. El puente con el juego (VOZ-3) no se rompe:
   - El TV1 de los modos viejos sale idéntico al de hoy: las perillas nuevas solo se
     escriben si no están en neutro (o van en un bloque aparte que el juego aún no lee).
     resultados-tv1.json sigue 13/13.
   - «Copiar TV1» de un modo nuevo o de un modo de sílabas que el juego no conoce
     (solfeo, scat, konnakol, mm, vocal) avisa en el taller «solo taller hasta VOZ-2b» y
     no convierte las sílabas en zigués sin decirlo (SIL_TV1).

Al empezar: /buscar-referencias --all (biblioteca con categorías, estilos de canto,
canto de garganta, yodel, susurro, konnakol).

Abiertas (formato D antes de construir):
- D1: ¿La biblioteca sustituye también a las recetas de figura o solo agrupa?
- D2: Konnakol: ¿por pulsos de la melodía o por la agrupación métrica (METRICA-1)?
- D3: Canto de garganta: ¿la melodía se proyecta a los armónicos posibles o se elige un
  bordón y se avisa de las notas que no existen?
- D4: Perillas nuevas en el TV1: ¿se omiten en neutro o van en un bloque aparte
  («soloTaller») que VOZ-2b leerá?

Restricciones: no tocar C# ni assets de Unity; nunca escribir .meta; commit solo de los
archivos del taller y de la web; push y artefacto solo con OK de Claudio.

DoD: las tres melodías se eligen desde la biblioteca con su insignia; los 9 estilos de
sílabas suenan; los 5 modos nuevos suenan distinto entre sí y de los 11 viejos (oídos
por Claudio); modos viejos idénticos; TV1 de los modos viejos idéntico y paridad del
importado 13/13; aviso «solo taller» en el TV1 de lo nuevo; Node sin NaN;
build.py --web; README del taller al día; commit en ALWTTT y en la web (push solo con
OK). Republicar el artefacto solo si Claudio lo pide (se lee entero antes: es grande).

Después: B (silabeador español con rr y ch), C (melodías v2), VOZ-2b en C# (post-demo:
el juego aprende las perillas y los modos de sílabas nuevos).
```

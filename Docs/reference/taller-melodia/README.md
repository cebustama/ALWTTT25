# Taller de melodías (voz de Zig)

Página privada para oír a Zig con el mismo Pink Trombone del juego, compararlo con melodías por figura y modos de voz, y mover la garganta en vivo. Creada el 2026-11-13 a partir del comentario de Matías («siempre es lo mismo»).

- **Dónde:** artefacto https://claude.ai/artifact/WRdHN5Gf839qotLhrqvyLj y web privada `/privado/taller-melodia/` (`python -I -X utf8 build.py --web`; enlazada desde `/privado/` y `/privado/talleres-musicales/`; web `58b05c6`, subido el 2026-11-13). Wrapped con el diagnóstico y la propuesta: https://claude.ai/artifact/GGpSCDLpNzdx7hbDfqiUf5 (`../zig-voz-wrapped/`).
- **Archivos:** `motor-voz.js` (el fork POC-FORK 1–7 en JS + extensiones X1–X3), `motor-canto.js` (melodías, sílabas, modos, cantante del juego y cantante v2), `template.html` + `build.py` → `index.html`. Los dos motores van dos veces en la página: como scripts (voz en vivo) y como fuente del Web Worker (renders).
- **Investigación:** `investigacion.md` (Pink Trombone, forks, fisiología de los estilos, contorno melódico; con lo que no se pudo verificar).

## Qué hace

- **Melodía × Voz:** Rise Up exacto (`MelodyPattern_RiseUp_44_8m`) o la melodía nueva; Zig hoy (`VoiceProfile_Zig` + prefab `Zig_SingerVoice`, `expressivityEnabled = 0`) o un modo. Teclas M y V.
- **Melodías antiguas (2026-11-16, pedido de Claudio):** categoría «Antiguas» de la biblioteca. Son el **epitafio de Seikilos** (6/8, con la letra griega cantada), el **himno hurrita h.6** en la lectura de Vitale (compás mixto 2/4–3/4, sin letra) y **la canción del trasero del Bosco** en la versión de Hamrick (4/4). Están en `MELODIAS_FIJAS` de `motor-canto.js` (`melodiaFija(id, transponer)`), con notas `[midi, inicio, duración, sílaba]` y una ficha que dice qué es seguro, qué es interpretación y de qué fuente sale cada cosa; la ficha se ve debajo del escenario.
  - Las sílabas vacías en una melodía con letra son melismas. La voz canta sin tildes («zêis» → «zeis»).
  - Sílabas «La letra original». La tónica transporta (Do = altura escrita). El acompañamiento es un bordón de tónica y quinta. Si la voz estaba en «Zig hoy», pasa al modo sugerido por la ficha.
  - «Copiar para la carta» da las notas como un `MelodyPattern` (grado, inicio, duración, velocidad) más el MIDI y las sílabas.
  - Transcripciones: Seikilos, de la partitura LilyPond de la Wikipedia inglesa (sigue a Pöhlmann y West 2001); Vitale, leída de la imagen de Commons; Hamrick, medida por análisis espectral de su grabación de 2014, porque su MIDI y su PDF ya no están en línea. Idea y plan: `../ideas/2026-11-16_melodias_antiguas.md`.
- **Biblioteca de melodías (TALLER-MEL-A, 2026-11-16, D1 = A):** sustituye al desplegable. Tiene chips de categoría (Del juego, Creadas, Antiguas; Medievales, América, Asia, Clásicas y En cuadros, vacías hasta la sesión C), un buscador y tarjetas con nombre, época, región, compás, ✓ letra e insignia de licencia (🟢 libre, 🟡 lectura moderna, 🔴 solo taller; el motivo sale al pasar el ratón). Datos en `MELODIAS_FIJAS` (`categoria`, `epoca`, `region`, `licencia`, `licenciaMotivo`): Seikilos 🟢, hurrita 🟡 (ritmo de Vitale), Bosco 🟡 (lectura de Hamrick). Rise Up, las recetas y «Nueva» son tarjetas 🟢. La barra de arriba muestra la melodía actual y lleva a la biblioteca; la tecla M y el TM1 siguen igual.
- **Sílabas en botones (TALLER-MEL-A):** sección propia con 9 estilos: zigués, del modo, letra original (solo en melodías con letra), texto propio, solfeo (do móvil por grado), scat, konnakol (por pulsos de la melodía, D2 = A: ta / ta ka / ta ki ta / ta ka di mi / ta di gi na tom), boca cerrada y vocal sola. Debajo del roll va la línea de sílabas que suenan. El dato sigue siendo `S.silMode`. Los cuatro primeros los conoce el juego; los demás son solo del taller (`SILABAS_ESTILOS`, `asignarEstiloSilabas`).
- **Modos:** Zig hoy, Zig hoy + dicción (el C-lite que ya existe en C#), Tierno y calmado, Pop, Punk rock, Death metal, Ópera, Crooner, Falsete, Robot, Borracho y, desde TALLER-MEL-A, **Susurro, Canto de garganta, Nasal (twang), Canto llano y Yodel**. Los nuevos llevan su fuente en la nota del modo. Los de chip punteado son `soloTaller`: usan perillas que el juego aún no tiene.
  - **Perillas nuevas** (`NUEVAS`, fuera de `BASE`): `sonoridad` (1; 0 = solo aire, extensión `x.voz` de la glotis en `motor-voz.js`), `nasalidad` (0; el `velumOverride` de X1), `quiebre` (0; yodel, ver abajo), `garganta` (0; lengua → armónico), `bordon` (−12 semitonos desde la tónica) y `realce` (0; solo con garganta). **Neutro = el motor de antes, byte a byte**: comprobado con el hash del render de los 9 modos viejos × 3 melodías, Zig hoy, sus TV1 y el gain (script en scratch, `base.cjs`); paridad del motor con C# 9/9 y del TV1 importado 13/13 sin cambios.
  - **Canto de garganta, medido (D3 = A):** cada nota se proyecta al armónico 6–12 más cercano del bordón (`proyectarGarganta`; el roll dibuja el armónico real y cuántos cents se movió). La lengua sola no basta: destaca los armónicos 5–6 y deja los demás por debajo de sus vecinos. Lo que funciona en Pink Trombone es estrechar la punta de la lengua (índice 32–39, diámetro 0,15–0,25) y los labios. Esas posturas están en la tabla `GARGANTA`, de 18 filas, sacada de un barrido con bordones de 98, 131 y 165 Hz. Una escala de 8 notas sobre Do3 da el armónico pedido **+8 a +14 dB** sobre sus vecinos ±2, en 8 de 8. Con un bordón de 65 Hz (el −24 inicial) solo funcionaban 2 de 8, porque mandaba F1; por eso el valor por defecto es −12. Las fuentes (Bloothooft et al. 1992, F2+F3; Adachi y Yamada 1999, la cavidad trasera) explican la anatomía; el taller imita el resultado.
  - **Prueba de oído de Claudio (2026-11-16) y arreglo (D5 = A):** el silbido «se oye muy a lo lejos, por detrás». Medido: el armónico ganaba a sus vecinos, pero quedaba **16 dB por debajo** del más fuerte (h1–h2 del bordón). Perilla nueva **`realce`** (no física, como la saturación): pasa-banda RBJ de Q 18 centrado en el armónico (se desliza 25 ms entre notas), le quita graves a la señal seca (pasa-bajos de 450 Hz restado) y aplica una ganancia de compensación. Con 0,8 (valor del modo) el armónico queda **+3,2 dB por encima de todo lo demás** de media (7 de 8 notas son el armónico más fuerte; h11, −0,6 dB) y +23 dB sobre sus vecinos. Gain del TV1, 1,33.
  - **Segunda escucha (Claudio: «el silbido un poco fuerte y el dron una octava por debajo»):** bordón −24 (~65 Hz con Do). Con un bordón de menos de 100 Hz, la melodía va a los armónicos 8–16, no a 6–12, para que el silbido siga en la zona de las posturas medidas (580–2000 Hz). El filtro tiene Q = máx(18; 1,5·n), así que su banda es más estrecha que la distancia entre armónicos. Medido con realce 0,8: el armónico queda +13 dB sobre sus vecinos (8 de 8) y unos 6 dB por debajo del bordón de media (antes, +3 dB por encima). Con bordón −12 todo da igual que antes (+3,2 dB). Antes, sin realce, el −24 no funcionaba (2 de 8, mandaba F1).
  - **Yodel, rehecho tras la prueba de oído («no noté el efecto», D5 = A):** antes solo cambiaba de registro en saltos de 5+ semitonos, que muchas melodías no tienen. Ahora el registro va por la altura con histéresis (cabeza si la nota está 2+ semitonos sobre el centro del rango, pecho si está 1+ por debajo; un salto de 5+ también cambia). **La vocal sigue al registro** (la lengua va hacia i en cabeza y hacia o en pecho, según `quiebre`), que es lo que más delata el yodel. Hay más contraste: cabeza con tensión −0,45Q y aire ×(1+1,5Q); pecho con tensión +0,18Q y más volumen. El crack es más largo: caída de volumen de 45 ms y salto de altura de 2Q semitonos que vuelve en 75 ms. Con Zig-zag, la tensión alterna entre ~0,84 (pecho) y ~0,33 (cabeza) a lo largo de la melodía.
  - **Consonantes que se pierden (prueba de oído, D6 = A → sesión B):** la «s» suena «i», la «k» se pierde y la «t» suena entre «d» y «l». Hay tres causas generales en el motor: (1) las cuerdas tardan ~0,2 s en dejar de vibrar (`intensity` baja 0,05 por bloque de 512) y la consonante dura 75–100 ms, así que las sordas salen sonoras; (2) el ruido de fricción de Pink Trombone está filtrado en ~1 kHz y una «s» real vive entre 4 y 8 kHz; (3) el pasa-bajos de `brillo` corta hacia 3,9 kHz en Pop. El arreglo es una perilla `articulación` en la sesión B: ensordecer rápido en las sordas, ruido agudo en s/z/sh y un soplo breve al soltar p/t/k. Toca el arranque de la voz, así que se mide contra la paridad con C#.
  - El gain del susurro toca el tope de 2 (su RMS es ~0,004): VOZ-2b tendrá que subir el tope o el aire.
- **Perillas:** ★ las que existen en `VoiceProfileSO`; el resto es propuesta (longitud del tracto, aire, growl, aspereza, scoop, caída, glide, ataque, legato, saturación, vocal base, sílabas).
- **Melodía nueva:** figura (escalera, zig-zag, saltos, ola, pedal, secuencia, paseo), sube/baja en grados, dirección, rango, ritmo por celdas (densidad, síncopa, silencios, swing), reglas (post-salto, acorde en 1 y 3, cadencia, firma de Zig), 16 escalas (7 existen en el juego).
- **Zigués (2026-11-13, pedido de Claudio: «inspiradas en español, pero que suene extraterrestre»):** léxico de 9 palabras por semilla con la fonotaxis del español (CV, grupos tr/kl/gr, codas), sesgado a u, i, z, sh y k; un verso de cuatro palabras que vuelve. La `dureza` de cada modo elige consonantes suaves (tierno) o duras (punk, death metal). Selector «Sílabas»: zigués (por defecto), las del modo o texto propio.
- **Segunda voz (Doble Armonía, 2026-11-13):** relación «como el juego» (`NearestChordToneHarmonyStrategy`: nota del acorde más cercana, 3–14 semitonos, desempate por la anterior) o intervalo diatónico (3.ª arriba/abajo, 6.ª, 5.ª, octava, unísono); a la par, en eco o en coro («u» largas); perfil propio o el de la voz 1; volumen y apertura estéreo; la garganta muestra la voz 1 o la 2. Dos Web Workers, uno por voz.
- **En vivo:** ScriptProcessor con la voz del modo; teclas A–Ñ, arrastrar la lengua sobre el dibujo, botones de consonante y vocal.
- **Versiones:** en este navegador; código `TM1:`; «Copiar para la carta» da `VoiceProfileSO` + `propuestaVoiceProfileV2` + `melodiaPropuesta`.

## Paridad y medidas (`paridad/`)

```
cd paridad
node casos.mjs && dotnet run -c Release && node casos.mjs comparar   # motor JS = fork C#
node formantes.mjs                                                   # F1/F2 de las vocales
node vocales.mjs                                                     # búsqueda de lengua y labios para a e i o u
```

**VOZ-1 (2026-11-14):** X1–X3 ya están en el fork C# (POC-FORK 8–11, ver `FORK_NOTES.md`). `casos.mjs` suma seis casos que las encienden (consonantes con constricciones, velo y turbulencia; tracto 38 y 52; death; aspereza con rampa de growl; punk con todo): **9 casos, −154,8 a −158,6 dB**. Con todo neutro, los tres casos originales son **idénticos byte a byte** a los de antes de VOZ-1 (`base/`, comparado con `cmp`; la carpeta se ignora en Git).

Resultado del 2026-11-13: 3 guiones (por defecto upstream y dos de cantante, 481.280 muestras): **95 % idénticas bit a bit, el resto a un ulp; error −155 dB**. El `.csproj` compila las fuentes reales de `Assets/PinkTrombonePOC/PinkTromboneSrc` (net10). Si cambia el fork, volver a correr.

**Compositor por figuras (MGP-MEL-FIGURE-1, 2026-11-14):** `paridad/figuras/` compara `componer(p)` con `MelodyFigurePlanner.cs` del paquete MidiGenPlay (el archivo real, compilado fuera de Unity):

```
cd paridad/figuras
node casos.mjs && dotnet run -c Release
```

Resultado: **5.088 casos, 129.564 notas, todos idénticos** (tiempo, duración, grado, MIDI, velocidad, compás, fin de frase y firma). Catálogo: 7 figuras × 4 direcciones × 16 escalas × 6 semillas, más 2.400 barridos con todos los controles al azar. Si cambia `componer` o el planificador, volver a correr.

Vocales calibradas contra Quilis (a 700/1300, e 450/1900, i 300/2300, o 500/950, u 320/800): a 710/1310, e 450/1910, i 310/2230, o 510/930, u 310/810 (lengua índice/diámetro y labios en `motor-canto.js`). Hallazgos: la «uh» de hoy mide F1 790 / F2 1210; el preset del juego «Back (oh)» mide 730/1650, más «a» que «o»; la «u» necesita labios redondeados.

Coste (Node, 48 kHz): ~6 % del tiempo real por voz, igual con y sin extensiones. En Unity el POC midió ~10,5 % por voz en build.

## Igual al juego y distinto

- **Igual, con prueba:** el motor de voz (arriba).
- **Igual, por lectura:** `cantarJuego` = `SingerVoice.OnAudioFilterRead` v7 con el perfil de Zig (bloques de 1024, lead por intervalo, vibrato con retraso, tone stage); el C-lite M1–M3 con `opts.diccion`.
- **Distinto:** la octava de Rise Up (en el juego, la media del instrumento); el acompañamiento es sintetizado; el volumen de la voz se iguala entre modos.
- **Propuesta:** X1 constricciones (algoritmo de `TractUI` del original web), X2 longitud del tracto, X3 growl/aspereza/aire, el cantante v2 y el compositor por figuras. Apagadas por defecto: con valores neutros el motor es el fork.

## Copiar TV1: el estilo de voz para el juego (VOZ-3, 2026-11-16)

«Copiar TV1 (estilo de voz)», en Versiones, copia `TV1:` + base64 con la voz entera (nombres del taller, sin recortar), las sílabas (zigués, lista o texto; con la letra original, su texto) y el **gain** calibrado aquí con `MC.gainDeVoz` (mismo RMS que la Zig de hoy en el juego). En Unity: *ALWTTT ▸ Voz ▸ Importar estilo*. Paridad del importado: `paridad/cantante` con `node casos.mjs tv1` (13/13). Copia de la web al día (web `412f576`); el artefacto no.

**Lo que el juego aún no lee (TALLER-MEL-A, D4 = C):** el TV1 lleva un bloque `soloTaller` solo cuando hace falta: perillas nuevas fuera del neutro (`bordon` siempre que haya canto de garganta) o un estilo de sílabas que el juego no conoce. En ese caso las sílabas viajan como `texto` con las que suenan en esta melodía, nunca como zigués en silencio, y el taller avisa «solo taller hasta VOZ-2b». El TV1 de los modos viejos sale idéntico al de antes. El importador ignora en silencio las claves de nivel superior, así que el bloque no genera avisos. `gen-estilos.mjs` salta los modos `soloTaller`; el canto llano sí entraría en la tabla del juego la próxima vez que se regenere.

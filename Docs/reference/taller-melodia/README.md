# Taller de melodías (voz de Zig)

Página privada para oír a Zig con el mismo Pink Trombone del juego, compararlo con melodías por figura y modos de voz, y mover la garganta en vivo. Creada el 2026-11-13 a partir del comentario de Matías («siempre es lo mismo»).

- **Dónde:** artefacto https://claude.ai/artifact/WRdHN5Gf839qotLhrqvyLj y web privada `/privado/taller-melodia/` (`python -I -X utf8 build.py --web`; enlazada desde `/privado/` y `/privado/talleres-musicales/`; web `58b05c6`, subido el 2026-11-13). Wrapped con el diagnóstico y la propuesta: https://claude.ai/artifact/GGpSCDLpNzdx7hbDfqiUf5 (`../zig-voz-wrapped/`).
- **Archivos:** `motor-voz.js` (el fork POC-FORK 1–7 en JS + extensiones X1–X3), `motor-canto.js` (melodías, sílabas, modos, cantante del juego y cantante v2), `template.html` + `build.py` → `index.html`. Los dos motores van dos veces en la página: como scripts (voz en vivo) y como fuente del Web Worker (renders).
- **Investigación:** `investigacion.md` (Pink Trombone, forks, fisiología de los estilos, contorno melódico; con lo que no se pudo verificar).

## Qué hace

- **Melodía × Voz:** Rise Up exacto (`MelodyPattern_RiseUp_44_8m`) o la melodía nueva; Zig hoy (`VoiceProfile_Zig` + prefab `Zig_SingerVoice`, `expressivityEnabled = 0`) o un modo. Teclas M y V.
- **Melodías antiguas (2026-11-16, pedido de Claudio):** grupo propio en el selector de melodía. Son el **epitafio de Seikilos** (6/8, con la letra griega cantada), el **himno hurrita h.6** en la lectura de Vitale (compás mixto 2/4–3/4, sin letra) y **la canción del trasero del Bosco** en la versión de Hamrick (4/4). Están en `MELODIAS_FIJAS` de `motor-canto.js` (`melodiaFija(id, transponer)`), con notas `[midi, inicio, duración, sílaba]` y una ficha que dice qué es seguro, qué es interpretación y de qué fuente sale cada cosa; la ficha se ve debajo del escenario.
  - Las sílabas vacías en una melodía con letra son melismas. La voz canta sin tildes («zêis» → «zeis»).
  - Sílabas «La letra original». La tónica transporta (Do = altura escrita). El acompañamiento es un bordón de tónica y quinta. Si la voz estaba en «Zig hoy», pasa al modo sugerido por la ficha.
  - «Copiar para la carta» da las notas como un `MelodyPattern` (grado, inicio, duración, velocidad) más el MIDI y las sílabas.
  - Transcripciones: Seikilos, de la partitura LilyPond de la Wikipedia inglesa (sigue a Pöhlmann y West 2001); Vitale, leída de la imagen de Commons; Hamrick, medida por análisis espectral de su grabación de 2014, porque su MIDI y su PDF ya no están en línea. Idea y plan: `../ideas/2026-11-16_melodias_antiguas.md`.
- **Modos:** Zig hoy, Zig hoy + dicción (el C-lite que ya existe en C#), Tierno y calmado, Pop, Punk rock, Death metal, Ópera, Crooner, Falsete, Robot, Borracho.
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

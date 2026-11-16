# Idea · Melodías antiguas y tradicionales en el taller y en el juego (2026-11-16)

Idea de Claudio, al capturar el video *What Ancient Music Actually Sounded Like (According To Archaeology)*, The Archaeologist's Notebook (Giselle), ~14-nov-2026, 30:27 (https://youtu.be/2nWf_OXdKz8): «Me interesa mucho lo de MGP/ALWTTT con melodías antiguas o tradicionales (otro ejemplo, más absurdo, la partitura en el trasero de un cuadro famoso, no recuerdo el nombre ni autor). De hecho, ¿sería posible portearlas y escucharlas primero en el taller?».

**Respuesta corta: sí.** El Taller de melodías (`../taller-melodia/`) ya canta una melodía fija tal cual (Rise Up, `MelodyPattern_RiseUp_44_8m`, en `motor-canto.js`) y acepta letra propia («Sílabas: texto propio»). Falta un catálogo de melodías fijas y su paso a carta.

## Candidatas

| Melodía | Qué se conserva | Ojo |
| --- | --- | --- |
| **Epitafio de Seikilos** (estela de mármol, s. I d.C., cerca de Aydın; hoy en el Museo Nacional de Dinamarca) | Alturas y ritmo completos (notación griega con signos de duración) y letra griega. Video, 18:43–20:30 | No es 4/4: comprobar con Music Lab cómo transcribirlo. Letra para Zig: griego transliterado |
| **Himno hurrita a Nikal** (h. 6, Ugarit, ~1400 a.C.) | Letra e instrucciones de intervalos para lira de 9 cuerdas. Video, 16:29–18:40 | **Sin ritmo** y con varias lecturas incompatibles (Kilmer, West, Dumbrill…): elegir una, declararla y no venderla como «la» melodía |
| **«La canción del trasero»**: partitura pintada en las nalgas de un condenado en el panel del Infierno de *El jardín de las delicias*, del Bosco (h. 1490–1510, Museo del Prado) | Una línea en notación cuadrada. La transcribió Amelia Hamrick en 2014 y se hizo viral | Clave y ritmo inciertos: es la lectura de Hamrick. Comprobar la fuente antes de usarla |
| **Tradicionales de dominio público** (Chile y Latinoamérica: tonadas, cuecas antiguas, cantos de bailes chinos…) | Por elegir con Music Lab y con la Biblioteca de canciones | Solo melodías de autor anónimo o de dominio público; nada de Violeta Parra ni Víctor Jara (derechos). Cantos rituales de pueblos vivos (ül mapuche, por ejemplo): no sin pensarlo bien |

## Plan (✦ IA, por decidir)

1. **Taller (solo JS):** un selector «Melodías fijas» en el Taller de melodías con datos `{nombre, fuente, lectura, compás, notas [midi, inicio, duración], letra}`. Cantadas por Zig o por cualquier modo, con la letra en sílabas, y una ficha que dice qué es seguro y qué es interpretación.
2. **A la carta:** «Copiar para la carta» da un `MelodyPattern` como el de Rise Up (grado, inicio, duración, velocidad), que se importa con el camino de TM1.
3. **En el juego:** una carta rara o un Finisher que toca una melodía de hace 2.000 años; si el compás no es 4/4, depende de MGP-METER-1 / celdas de 3/4 y 6/8.

Tamaño: el paso 1 es una sesión corta del taller. Según la regla de la cola de MidiGenPlay, va cuando la semana de ALWTTT esté cumplida; los pasos 2 y 3, después de la demo.

## Fuentes para empezar

- Pöhlmann y West, *Documents of Ancient Greek Music* (Oxford, 2001): Seikilos e himno hurrita.
- M. L. West, «The Babylonian Musical Notation and the Hurrian Melodic Texts», *Music & Letters* 75:2 (1994).
- Hamrick (2014), sobre la partitura del Bosco: buscar la publicación original antes de citarla.

## Otras ideas del mismo video

- 💡 **Quironomía:** en las tumbas egipcias, alguien dirige a la banda con gestos de la mano que marcan alturas y frases, no compases (20:38–21:40). Es lo que hace el manager: un director que no toca. Podría ser una carta o un evento: el manager «marca» la melodía y Zig la sigue.
- 💡 **Sonidos raros:** campanas *bianzhong* con dos notas según dónde se golpeen (14:36), bramadera (6:24), litófono (6:39) e hidraulis (23:26), como instrumentos de un público o de un local. Vía técnica: la nota de sintetizadores de MidiGenPlay (`MidiGenPlay/MidiGenPlay/Documentation~/research/2026-11-09-sintetizadores-en-el-motor.md`).

**Conecta con:** Music Lab (árbol evolutivo, raíces antiguas), cola de MidiGenPlay (`Sesiones_MidiGenPlay.md`), cuaderno-civilizaciones (la música de América que el video no trae), `_comun/conexiones.md` (2026-11-16).

**Estado:** paso 1 hecho el 2026-11-16, a pedido de Claudio («dale con lo del taller»). Las tres melodías ya están en el Taller de melodías: selector «Melodías antiguas», ficha, letra y bordón (ver `../taller-melodia/README.md`). Seikilos, de la partitura de Wikipedia (Pöhlmann y West); el himno hurrita, en la lectura de **Vitale** (no hay copia legible de la de West); el Bosco, medido de la grabación de Hamrick (su MIDI y su PDF ya no están en línea). Pasos 2 y 3, después de la demo.

## v2 (2026-11-16, conversación con Claudio)

Claudio: «Está maravilloso». Preguntó qué melodías son libres, qué otras propondría, y pidió una biblioteca de melodías en vez de un desplegable, cinco estilos de voz más, palabras reales y una sección de estilos de sílabas. Decisión: **guardar todo; arranque de la sesión A** en `../../planning/active/ARRANQUE_TALLER-MEL-A.md`.

### Qué se puede usar (✦ IA, no es asesoría legal)

| Capa | Cuándo es libre |
| --- | --- |
| Composición (notas y letra) | UE y Chile: autor muerto hace más de 70 años (hoy, en 1955 o antes). **España:** 80 años si murió antes del 7-dic-1987 (disp. trans. 4.ª LPI; hoy, en 1945 o antes). EE. UU.: publicada antes de 1931. México: 100 años |
| Edición o reconstrucción moderna | Puede tener derechos propios: art. 129 LPI (ediciones, 25 años) y obra derivada si hay decisiones creativas (un ritmo que el original no da) |
| Grabación | Derechos aparte (70 años en la UE); no aplica, el taller usa notas |

- Las tres de la v1: 🟢 Seikilos (todo está en la piedra); 🟡 hurrita (la tablilla es libre, el ritmo de **Vitale 1982** es suyo: para el juego, una lectura propia); 🟡 Bosco (el cuadro es libre; la transcripción de **Hamrick 2014** no tiene licencia conocida: para el juego, leer el cuadro nosotros).
- Clásica: casi todo lo anterior a 1900 es libre; partir de ediciones antiguas (IMSLP), no de urtext modernos. Ejemplo de divergencia: *La consagración de la primavera* (1913) es libre en EE. UU. y protegida en la UE hasta 2041 (Stravinsky murió en 1971).
- Tradicional anónimo: libre, pero no una versión recopilada y arreglada concreta. Violeta Parra y Víctor Jara no. Lo sagrado de pueblos vivos (ül, tayil): no por ser legal está bien (enlaza con el arqueólogo afuerino del videojuego).
- **Para la biblioteca:** insignia de licencia por melodía: 🟢 libre, 🟡 lectura moderna (solo taller hasta tener lectura propia), 🔴 solo taller.

### Candidatas v2 (fechas y autorías de memoria: comprobar antes de transcribir)

| Grupo | Melodía | Por qué |
| --- | --- | --- |
| En cuadros | Caravaggio, *Descanso en la huida a Egipto* (motete de Bauldeweyn, legible); Baude Cordier, *Belle, bonne, sage* (partitura en corazón, Códice Chantilly, h. 1400) | La familia del Bosco |
| Antiguas | Himnos délficos (128 a.C.), Mesomedes (s. II), himno de Oxirrinco (s. III); **himno hurrita en otras lecturas** (West, Kilmer, Dumbrill) | La misma tablilla en tres lecturas = la ficha hecha sonido |
| Asia | *Youlan* (guqin, s. VI–VII); *Etenraku* (gagaku) | Menos eurocentrismo |
| Medievales | *Ut queant laxis* (origen del do-re-mi); Hildegard von Bingen (efecto Matilda); *Cantigas de Santa María* (Alfonso X, Sevilla); *Sumer is icumen in* (canon → segunda voz); *Ad mortem festinamus*, *Dies irae* (¿la Reina? especulativo) | |
| América | ***Hanacpachap cussicuinin*** (1631, quechua, primera polifonía impresa en América); tonadas del ***Códice Martínez Compañón*** (Trujillo, h. 1780) | Árbol evolutivo de la música, cuaderno-civilizaciones |
| Procedurales | ***Musikalisches Würfelspiel*** (atribuido a Mozart, 1792: componer con dados); Satie, *Vexations* (840 repeticiones) | Un sistema que genera |

**Primera tanda recomendada (sesión C):** Würfelspiel, *Hanacpachap*, Caravaggio, *Ut queant laxis* y una segunda lectura del himno hurrita.

### Cinco estilos de voz nuevos (✦ IA, propuesta aceptada para A)

| Estilo | Rasgos | Qué estrena |
| --- | --- | --- |
| **Susurro** | Sin voz: solo el ruido de aspiración (tensión ≈ 0) y consonantes marcadas | Perilla `sonoridad` (mezcla voz/aire) |
| **Canto de garganta** (khöömei) | Bordón grave fijo; la melodía son los armónicos 6–12, elegidos moviendo la lengua para que F2 caiga en el armónico | Perilla «lengua → armónico» |
| **Nasal** (twang) | Velo abierto siempre, tracto corto, tensión alta, brillo | Perilla `nasalidad` (el `velumOverride` de X1, ya en el fork C#) |
| **Canto llano** (gregoriano) | Sin vibrato ni scoop, legato total, laringe baja, vocales latinas, ataque suave | Nada nuevo; pareja de las melodías antiguas |
| **Yodel** | En saltos grandes, quiebre de registro pecho ↔ falsete con un crack de tensión y altura | Perilla `quiebre` |

Sexto opcional: **Lamento** (voz quebrada, fry en los finales, sollozos). C#: canto llano con lo que hay; susurro y nasal sobre `Glottis.intensity`/aspiración y `velumOverride` del fork; garganta y yodel, lógica nueva del cantante v2 → **VOZ-2b** después de VOZ-2, con paridad y neutro = byte a byte.

### Sílabas y palabras

- Pink Trombone: bien vocales, p/b t/d k/g, m n ñ, s z f sh j; regular l (sin canal lateral), ll/y y el contraste p/b; fingidas rr (vibrar la punta) y ch (t + sh). Se entiende como un ventrílocuo: mejor si se sabe qué dice.
- Palabras reales: sí; el español va casi 1:1 de letra a sonido, un silabeador es barato (sesión **B**). Diccionario zigués con significados (la Reina, la cohesión…); posible conexión con el generador de nombres de Band Manager (especulativa: hoy trabaja en inglés).
- Sección de sílabas: zigués, del modo, letra original, texto español, solfeo, scat, konnakol (ta-ka-di-mi, sigue la agrupación métrica → METRICA-1), boca cerrada, vocal sola; línea de sílabas bajo el roll.

### Orden

A: biblioteca + sílabas en botones + 5 estilos (taller, solo JS) → B: silabeador español con rr y ch → C: melodías v2. Después de la demo: VOZ-2b (estilos nuevos en C#).

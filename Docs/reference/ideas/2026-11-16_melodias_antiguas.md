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

**Estado:** semilla

# Revisión de Music Lab: taller de batería (2026-11-06)

Revisión de los 36 patrones de `patrones.json`, hecha sobre la rejilla y no sobre los nombres. Los datos por patrón (`bpm`, `tags`, `nota`) están en `datos-music-lab.json`.

**Etiquetas.** [C] Confirmado (rejilla o fuente citada). [I] Inferido. [S] Sin respaldo.

**Convención de lectura.** «c2 paso 9» es el compás 2, paso 9. Los pasos cuentan desde 0 dentro de cada compás. Todos los `bpm` son [I]: tempos razonables para oír el patrón, no datos. Tienen respaldo externo los de drum'n'bass, disco y *Take Five* (fuentes 8, 9 y 10).

## Términos

- **Backbeat:** el golpe fuerte de caja en los pulsos débiles, lo que hace cabecear. En `FF_Rock8c`, pasos 2 y 6.
- **Ghost:** golpe de caja muy suave (`o`, velocidad 50), que se siente más que se oye. En `Syncopated Pocket (Funk)`, los pasos 2, 6, 10 y 14.
- **Spang-a-lang:** el dibujo del ride en swing, «ding, ding-da-ding». En `SP_Jazz8c`, pasos 0, 3, 5, 6, 9 y 11.
- **Parcial central:** el segundo de los tres golpes de un tresillo. El swing se apoya en el primero y el tercero; el del medio suena recto o tropezado, salvo en el shuffle de Purdie, donde es justo donde van los ghosts.
- **Clave:** el dibujo de cinco golpes en dos mitades que ordena toda la música cubana: una mitad con tres golpes y otra con dos. 3-2 quiere decir que la mitad de tres va primero.
- **Tresillo (cubano):** los tres primeros golpes de la clave, en 3+3+2. En una rejilla de 16: pasos 0, 6 y 12. No es lo mismo que el tresillo de la notación clásica.
- **Tumbao:** el ostinato del bajo o la conga en el son. En el bajo suenan solo los dos contratiempos del tresillo. En `FF_LatinSon32_8c`, pasos 6 y 12 del bombo.
- **Cáscara:** el dibujo que se toca en el casco del timbal, con forma distinta en cada lado de la clave.
- **Two-beat:** bombo y caja alternando, «boom-chick»; el trote del country y de la polca.

## Criterios para evaluar (✓ ~ ✗)

Se juzga el patrón, no el timbre. Si algo falla solo porque el kit sintético suena pobre, no cuenta contra el patrón: se anota aparte.

1. **Pulso.** ¿Puedes marcar el pulso con el pie en dos compases sin pensar? Un 6/8 en dos, un 5/4 con su 3+2 audible.
2. **Gesto del género.** ¿Está el rasgo que define el estilo, en su sitio? Backbeat en rock, ride en jazz, clave en latino, caja continua en train beat.
3. **Jerarquía.** ¿Se distinguen fuerte, normal y suave? Si todo pesa igual, o el acento cae donde no toca, resta.
4. **Hueco.** ¿Deja sitio para el bajo y la voz? Densidad razonable al tempo propuesto.
5. **Vuelta.** ¿Entra el 1 con naturalidad al repetir, o el bucle tropieza?
6. **Promesa.** ¿Es lo que dice su nombre y su paleta?

Escala: ✓ funciona y es lo que dice; ~ funciona, pero falla en un criterio o en el nombre; ✗ contradice el pulso o el gesto del género.

## Problemas, por gravedad

Solo propuestas: no se ha aplicado nada. Las rejillas propuestas son mías y están sin oír: hay que probarlas.

### Graves: el patrón no suena a lo que dice, y sale en el juego

**1. `FF_LatinClaveEngine` está fuera de clave.** Peso 1.0.
- [C] La clave en SideStick es correcta (0,6,12 | 4,8), pero el bombo en 0,7,14 no es tresillo, que sería 0,6,12. En el lado de tres, el paso 7 llega una semicorchea tarde respecto a la clave del 6 y el 14 una corchea tarde respecto a la del 12 (fuentes 1 y 2).
- [C] El charles repite cada 8 pasos, igual en ambos lados de la clave. [I] Una cáscara cambia con el lado y aquí va a la mitad de escala que la clave, que ocupa dos compases.
- Propuesta: bombo `......x.....x...` en los dos compases, igual que `FF_LatinSon32_8c`. Mientras no se confirme la cáscara (ver dudas), charles a negras o cencerro.

**2. `CS_CountryLilt68` es un 3/4 en una paleta de 6/8.** Peso 1.0.
- [C] Charles en 0,2,4, caja en 2 y 4 y el paso 3 vacío en todos los carriles. La agrupación es 2+2+2, no 3+3 (fuente 7). Junto a los demás 6/8 de la paleta cambia de pulso.
- Propuesta: A) moverlo a `WaltzLilt`; casi duplica `WL_CountryWaltz`. B) Hacerlo 6/8 de verdad (recomendada): bombo `x.....`, caja `...x..`, charles `x.xx.x`, que da el largo-corto que promete «lilt» (la caja en `x`, no en ghost).

**3. `Drum_4-2_8m_MIDI_Import_Test` tiene peso 1.0 en `FourOnTheFloor`.**
- [C] Es un asset de prueba, sin nombre, sin bombo a negras, con 29 velocidades que se redondean al exportar. [I] Parece un groove de semicorcheas escrito a escala doble: a tempo normal queda desnudo.
- Propuesta: peso 0 o sacarlo de la paleta.

**4. `CS_FunkShuffle68` no tiene shuffle.** Peso 1.0.
- [C] Doce semicorcheas parejas de charles; el shuffle es un tresillo sin la nota del medio (fuente 5). Los ghosts 9 y 11 caen fuera de las corcheas.
- Propuesta (shuffle tipo Purdie, subdivisions 1): charles `x.xx.x`, caja `.o.xo.`, bombo `x....x`. Otra opción: dejarlo y renombrarlo «funk 6/8».

### Medios: funcionan, pero no son lo que dicen

**5. `Compound Swing` y su ride anidado.** Peso 1.0.
- [C] Subdivisions 3 sobre corchea da pasos de 1/24. La SSoT de autoría ya avisa de que eso casi seguro no es el tresillo de swing buscado (`SSoT_Authoring_Rhythm_Patterns.md` §Compound meters). Suena a vals de jazz en 3/8.
- Propuesta: subdivisions 1, ride `x..x.x`, con bombo y caja en 0 y 3. Pero da nombre a la paleta: decidir antes si se quiere ese sonido.

**6. `CS_JazzCompound`: ride sin swing.** Peso 1.0.
- [C] Pasos 0,3,6,8,10: 3+3+2+2+2. Los pasos 0,6,10 son el spang-a-lang en 6/8; el 3 y el 8 lo estropean. Ghosts en 9 y en c2 paso 1 fuera de corchea.
- Propuesta: ride `x.....x...x.`, ghosts a pasos pares. Ojo: el resultado se parece a la propuesta del punto 5, habría que distinguirlos. Si se prefiere el ostinato 3+3+2+2+2, que es interesante, renombrarlo.

**7. `FF_TrainBeat` no es un train beat.** Peso 0: no suena en el juego, pero el nombre engaña.
- [C] El train beat lleva la caja continua con acentos en 2 y 4, y bombo en 1 y 3 (fuente 3). Esta rejilla es un two-beat. `FF_CountryTrain8c` sí lo es.
- Propuesta: renombrar a `FF_TwoBeat`.

**8. Parciales centrales y `OM_TakeFiveSwing`.**
- [C] Ghosts en parcial central: `WL_JazzWaltz` (c1 paso 4; c2 pasos 1 y 7), `OM_TakeFiveSwing` (paso 4 en los dos compases), `SP_Jazz8c` (c4 paso 1). El de `SP_Jazz8c` c8 paso 10 forma parte de un remate en tresillo y está bien.
- [C] En `OM_TakeFiveSwing`, una lección de Melodics pone caja en 2& y 4, tresillos suaves al final y bombo en 1. La rejilla tiene ghosts en 4 y 11, y bombo a negras (fuente 6, lección y no transcripción).
- Propuesta: mover cada ghost al paso siguiente (4→5, 1→2, 7→8). En `OM_TakeFiveSwing`, además, 11→9.

**9. `WL_JazzWaltz`: pisada en el carril equivocado.**
- [C] La pisada del 2 está en OpenHiHat (46), que suena largo. `SP_Jazz8c` usa PedalHiHat (44), el «chick» correcto (fuente 4).
- Propuesta: pasar el carril a 44.

**10. Conga de `FF_LatinSon32_8c`.** Lo demás está bien.
- [C] Fuerte en 7 y 14. A esta escala, con la clave en dos compases, se esperan abiertos en 12 y 14 y slap en 4 (fuente 2). El 7 cae fuera del pulso de corchea.
- [I] Las versiones de la conga varían, y un solo carril no distingue slap de abierto.
- Propuesta: `..o.x.....o.x.x.`.

### Menores

**11. `WL_CountryWaltz`.** [C] Caja `X` (120) sobre bombo 100: el vals pesa al revés. Propuesta: caja en `x`.

**12. Paleta `FourOnTheFloor`.** [C] De sus 9 entradas con peso, solo 2 tienen bombo en las cuatro negras (`FF_DiscoPulse`, `Four-on-the-Floor Pulse`). Es una paleta de «4/4 recto». Propuesta: renombrarla o separarla.

**13. Charles cerrado y abierto en el mismo paso.** [C] En 12 patrones: `CS_FunkShuffle68`, `CS_RockBallad68`, `DNB 4-4 2m test`, `FF_BreakbeatDense`, `FF_HalfTimeHeavy`, `FF_TrainBeat`, `OM_DjentChug`, `OM_RockDrive54`, `SP_Funk8c`, `WL_CountryWaltz`, `WL_FunkWaltz`, `WL_RockWaltz`. Un charles real no hace las dos cosas. Propuesta: quitar el cerrado en esos pasos. Para el taller: grupo de corte en el kit (el cerrado corta al abierto, como en GM), o el abierto de `FF_DiscoPulse` sonará falso.

**14. Carriles vacíos.** [C] OpenHiHat en `CS_CountryLilt68` y `FF_LatinClaveEngine`. Sin efecto sonoro.

**15. Sin `display`.** [C] `Drum_4-2_8m_MIDI_Import_Test`, `FF_HalfTimeHeavy`, `Four-on-the-Floor Pulse`, `Waltz-Pulse Lilt`, `WL_CountryWaltz`. El taller debe caer en `id`.

## La causa común: `genre_vocabulary.md`

[I] Varios errores vienen del vocabulario del skill `rhythm-pattern-generator`, porque las rejillas lo copian casi literalmente. Corregir solo los patrones dejaría el error vivo para la v2 (crear con LLM).

| Entrada | Dice | Problema |
| --- | --- | --- |
| Country, train beat | Bombo `x.x.x.x.` «every 8th», caja `.x.x.x.x` | [C] Eso es un two-beat, y con 2 pasos por pulso el bombo va a negras, no a corcheas |
| Latin, kick (tumbao) | `x......x......x.`, «ah of 2 + and of 4» | [C] El tumbao de bajo son los contratiempos del tresillo, pasos 6 y 12 |
| Latin, cáscara | `x.x.xx.xx.x.xx.x` por compás, «repeat per bar» | [I] No cambia con el lado de la clave y va a otra escala |
| Jazz, ride | `x..x.xx.xx.x` | [C] Sobra el paso 8. Se contradice con el spang-a-lang: el correcto es el de `SP_Jazz8c` |
| Jazz, caja | Posiciones 4 y 10 como «trip-of-2» | [C] Son parciales centrales. El propio archivo dice que el swing va en 2, 5, 8 y 11 |

## Dudas

- **Cáscara exacta.** [S] Las fuentes web no coinciden en los golpes. De memoria, y encaja con que contiene la clave de rumba: 3-2 en corcheas `x.xx.x.x|x.x.xx.x`. Es decir, en esta rejilla, `x...x.x...x...x.|x...x...x.x...x.`. Hay que comprobarlo con Peñalosa, *The Clave Matrix*, antes de usarlo.
- **Tempos por patrón en una paleta.** [I] `SyncopatedPocket` mezcla drum'n'bass (172), hip-hop (90) y jazz (160) sin tempo propio. Si la canción manda un BPM único, alguno sonará fuera de estilo. Además, `SP_Jazz8c` va en tresillos junto a patrones de semicorcheas: comprobar qué hacen el bajo y la melodía cuando sale. No he leído el código del selector.
- **Peso 0.** [C] Los tests del paquete confirman que nunca se elige (`DrumPatternPaletteSOTests`). Falta saber si `FF_TrainBeat` y `FF_Metal8c` están apartados a propósito.
- **Tempo de *Take Five*.** [C] Las fuentes dan entre 161 y 177; puse 168.
- **Bossa.** [C] No hay ningún patrón de bossa ni de samba en el catálogo, aunque Music Lab tiene *The Girl From Ipanema* y *Algo Contigo*, y el vocabulario la recoge. Tampoco shuffle de blues, que en el repertorio es *The Thrill Is Gone*. Idea para la v2.

## Lo que mejor está

Para calibrar el oído y el kit: `FF_Rock8c`, `FF_DiscoPulse`, `SP_Jazz8c`, `SP_DnB8c`, `SP_Funk8c`, `Syncopated Pocket (Funk)`, `FF_CountryTrain8c`, `CS_RockBallad68`.

## Fuentes

1. Clave: [Clave (rhythm), Wikipedia](https://en.wikipedia.org/wiki/Clave_(rhythm)). Golpes de la clave de son, tresillo, y que tocar fuera de clave se considera error.
2. Tumbao: [Tumbao, Wikipedia](https://en.wikipedia.org/wiki/Tumbao). Bajo en los contratiempos del tresillo; abiertos de la conga.
3. Train beat: [Popular Styles, Helbling (muestra)](https://www.helbling.com/sites/default/files/media/documents/S8024_DEMO_popstyles2.pdf). Leído solo en el extracto del buscador: no pude abrir el PDF.
4. Swing: [Rhythm Section Roles, Marshall University](https://www.marshall.edu/music/files/Rhythm-Section-Roles-in-standard-swing.pdf), [Swing Drumset Basics, The Instrumentalist](https://theinstrumentalist.com/august-2013/swing-drumset-basics). Extractos del buscador.
5. Shuffle: [The Purdie Shuffle, Tom Tom Magazine](https://tomtommag.com/2011/10/the-purdie-shuffle-part-one), [Purdie shuffle, Soundbrenner](https://www.soundbrenner.com/blogs/articles/purdie-shuffle). Extractos del buscador.
6. *Take Five*: [Melodics](https://melodics.com/learn-to-play/take-five-by-the-dave-brubeck-quartet-on-drums), [allsolos](https://allsolos.com/solo/685/joe-morello-drums-solo-on-take-five). Extractos del buscador.
7. 6/8 frente a 3/4: [Compound meter](https://www.soundbrenner.com/blogs/articles/compound-meter) y [Hemiola](https://www.soundbrenner.com/blogs/articles/hemiola), Soundbrenner. Extractos del buscador.
8. Drum'n'bass: [How to program 6 different jungle and drum 'n' bass grooves, MusicRadar](https://www.musicradar.com/how-to/how-to-program-6-different-jungle-and-drum-n-bass-grooves). Extracto del buscador.
9. Disco: [Disco beat, Soundbrenner](https://www.soundbrenner.com/blogs/articles/disco-beat). Extracto del buscador.
10. Breakbeat: [Amen break, Wikipedia](https://en.wikipedia.org/wiki/Amen_break). Unos 136 BPM y la caja retrasada en el tercer compás.
11. Cáscara: [Timbales, Wikipedia](https://en.wikipedia.org/wiki/Timbales), [Cascara, Soundbrenner](https://www.soundbrenner.com/blogs/articles/cascara). No dan una rejilla fiable.
12. Internas: `MidiGenPlay/Documentation~/authoring/SSoT_Authoring_Rhythm_Patterns.md`; `Tests/Editor/DrumPatternPaletteSOTests.cs`; `genre_vocabulary.md` del skill; en Music Lab, `analysis-since-ive-been-loving-you.md`, `analysis-the-thrill-is-gone.md` y `analysis-girl-from-ipanema.md`.

# Crítica de Music Lab al modelo de bucles de 12tone (2026-11-02)

Esta crítica la escribió un agente que hizo de Music Lab, desde la sesión de la raíz, a partir del resumen de la transcripción (`../ideas/2026-11-02_12tone_forma_de_bucles.md`); el video no se abrió. No tocó ningún archivo de ningún proyecto. Para comprobar resultados ejecutó código, que se guarda en `scripts/` (ver su README).

Etiquetas: **[C]** confirmado, con fuente o por teoría estándar; **[I]** inferido; **[A]** sin respaldo verificable; **[C, sim]** comprobado con código (el `arrange` del taller o el piloto sobre McGill Billboard).

> **Desfase en Music Lab.** `Music Lab/recursos-armonicos.md` (119 líneas) es idéntico a las primeras 119 líneas de `progression_index.md`. Lo comprobó también la sesión con `diff`. La caja de recursos (①②③) no está en el repo y hay que recuperarla de Claude Projects. El archivo está así desde el primer commit (`e4973f6`).

## Resumen

1. **El modelo es una cadena de 4 bits.** Cada frontera entre casillas es fuerte (F) o débil (D), y de ahí salen solas las anclas y los 5 tipos:
   - 0 débiles: extremo de conectores;
   - 1 débil: *cascade*;
   - 2 débiles seguidas: *disc*;
   - 2 débiles alternas: extremo señal/destino;
   - 3 o 4 débiles: apagado.

   [C, por construcción]
2. **«Fuerte = quintas y grados» coincide casi con «frecuente» en el rock.** Esos movimientos son el 82 % de los cambios de fundamental (de Clercq y Temperley 2011, tabla 5). Los semitonos son tan raros como las terceras y el modelo los cuenta como fuertes por conducción de voces: mezcla dos criterios. [C] + [I]
3. **El resultado de las posiciones es en buena parte métrico.** El piloto sobre Billboard reproduce el orden 1 > 3 > 4 > 2. Pero el 71 % de las fronteras débiles son un acorde que sigue sonando. Si solo se cuentan los destinos con salida por tercera o tritono real, el reparto queda casi plano: 36/33/21/18. [C, sim]
4. **En el taller v3, el modelo no culpa al ♭VI ni al iv6, sino a las dominantes.** El iv6 nunca cambia la forma (misma fundamental) y el ♭VImaj7 casi nunca. Las dominantes secundarias, los sustitutos tritonales y el turnaround sí la cambian: el nivel 2 de *Creep* pierde su alternancia señal/destino, y el nivel 3 de la de los 50 pasa el destino a la posición 2. [C, sim]
5. **Las paletas no corresponden a formas.** Solo «giro» destaca, con un 50 % de fronteras débiles frente a un 6–25 % en las demás. [C, sim, n = 23]
6. **Veredicto.** Sirve como diagnóstico y como guarda de identidad, pero no como etiqueta para el jugador. Como eje junto a la sorpresa, lo que aporta es la posición métrica. La división fuerte/débil se solapa en parte con la sorpresa.

## 1. Solidez

### Antecedentes
- **Schoenberg** (1954/1969) distingue movimientos ascendentes, descendentes y superfuertes (por segunda).
- **Meeùs** (2000, *MTO* 6.1) los reduce a dos: dominante (baja una quinta; sustitutos, bajar una tercera o subir una segunda) y subdominante (lo contrario). Para los dos, **bajar una tercera tiene dirección**. 12tone la llama débil e ignora la dirección.
- **De Clercq y Temperley (2011)**: en el rock, cada intervalo sube y baja casi por igual (+P4 2.266 frente a −P4 2.220). Ignorar la dirección es defendible en el rock, no en el doo-wop ni en el jazz. [C]

### El defecto más serio [I]
«Débil» junta cosas opuestas:
- las **terceras diatónicas** (`I→vi`): comparten dos notas, es una prolongación quieta;
- las **mediantes cromáticas** (`I→♭VI`, `I→III` de *Creep*): son lejanas en el *Tonal Pitch Space* de Lerdahl y suenan a sorpresa.

Por eso el modelo no distingue la isla tranquila de *Stand by Me* de la vertiginosa de *Creep*.

### Evidencia perceptiva
- No hay ningún estudio que pruebe la dicotomía tal como la formula 12tone. [A]
- Bharucha y Krumhansl (1983) y Krumhansl (1990) muestran que la relación entre acordes es asimétrica y depende de la tonalidad. Bigand, Parncutt y Lerdahl (1996) muestran que la tensión depende de la función, la aspereza y la conducción de voces. Las dos cosas contradicen la premisa de que las anclas no dependen de la tonalidad. [C, el hallazgo general]
- En el piloto, una casilla que sale de la tónica es destino el 18 % de las veces; si no es la tónica, el 10 %. [C, sim]

### Lo que falta, por gravedad [I]
1. El **acorde sostenido** como categoría propia: hoy cuenta como débil.
2. **Igual fundamental con otra calidad** (`IV→iv`, `V7sus4→V7`).
3. La **calidad y la sensible** (`V→i` frente a `v→i`).
4. La **dirección**.
5. El **bajo frente a la fundamental**: inversiones y pedales.
6. La **melodía**. Es irónico: el divorcio es una tesis melódica. Ver Nobile (2015).
7. **Tagg** (*Everyday Tonality*) ya asignaba papeles por posición métrica en los bucles. Es un antecedente que 12tone no cita, según el resumen.

### Método
- **Lo bueno:** muestreo aleatorio y reglas explícitas.
- **Lo flojo:**
  - un solo transcriptor, sin medida de acuerdo;
  - **sin modelo nulo**, de modo que el 71 % de equilibrados no dice nada por sí solo;
  - los bucles de 2 acordes tienen la forma fijada de antemano: `A A B B` siempre es señal/destino y `A B A B` siempre son conectores [C];
  - la fase la elige el transcriptor;
  - Billboard 2.0 cubre 1958–1991, así que no incluye la época EDM.
- **Piloto:** 730 canciones, de las que 460 tienen bucle. Por tamaño: 148 de 2 acordes, 123 de 3, 87 de 4, 38 de 5 y 37 de 6 a 8. Por tipo: señal/destino 30 %, *cascade* 28 %, *disc* 20 %, apagado 14 % y conectores 9 %. No es comparable con su 71/22/7.
- **Preguntas abiertas:**
  - ¿cuántos de sus 234 bucles tienen 2 acordes?
  - ¿cómo elige «el más importante»?
  - ¿qué hace con 6–8 acordes, 3/4, sus y acordes de quinta?

## 2. Aplicación a los niveles v3 [C, sim]

| Operador | ¿Cambia la forma? |
|---|---|
| `iv6` tras IV | Nunca (misma fundamental) |
| sus4 | Nunca |
| `♭VImaj7` antes de I | Casi nunca: solo entra si la llegada a I ya era débil |
| Dominante secundaria o sustituto tritonal | **Sí:** convierte una frontera débil en fuerte y mueve o crea un destino |
| Turnaround | **Sí:** el destino pasa a la posición 1 o desaparece |

- ***Creep*** (`I–III–IV–iv`): extremo señal/destino → con el nivel 2 (`VII7` antes del III), *cascade* con destino en la 3. Se pierde la identidad.
- **De los 50** (`I–vi–IV–V`): *disc* con destino en la 1 → con el nivel 3 (`♭VII7` hacia el vi), *cascade* con destino en la 2, la posición «flotante».
- **I–♭VII–IV–I y I–IV–V–vi**: el turnaround del nivel 2 los deja en todo conectores y dejan de aterrizar.
- **Mario, Préstamo ♭III y Andino `i–♭III–♭VII–i`**: el turnaround lleva el destino de la 4 a la 1.
- **Andino `i–♭III–iv–V` y Frigio**: el nivel 3 borra el único destino.

**¿Hay que quitar el ♭VI o el iv6?** No por este modelo.
- El ♭VI desnudo es una mediante cromática con color épico. Si se quiere el gesto «Mario», la versión canónica es `♭VI–♭VII–I`. Es una decisión de gusto.
- `IV→iv` es la línea cromática interior. **El modelo se equivoca al llamarlo débil.** [I]

**Guarda propuesta:** ninguna adición puede cambiar la cadena F/D de las 4 fronteras. La excepción es el turnaround de la última vuelta, que cuenta como remate y no se aplica en las cartas «giro».

**Corrección:** `CADENCE = {5, 7, 2, 11}` debería ser el conjunto de movimientos fuertes completo, {1, 2, 5, 7, 10, 11}. Así se evita el `II7` redundante en el vamp lidio y se respeta `vii°→I`.

## 3. Algoritmo reproducible (McGill Billboard 2.0)

1. **Lectura.** Leer `salami_chords.txt` sin colapsar las repeticiones, compás a compás (`.` = el acorde sigue; `N` = silencio; tener en cuenta los cambios de compás). Fundamental relativa a `# tonic` y familia de la calidad; el bajo, aparte.
2. **Bucles.** Periodo P ∈ {1, 2, 3, 4, 6, 8} compases, el mínimo posible. Condiciones: al menos 3 repeticiones seguidas, al menos 8 compases y de 2 a 8 acordes distintos. Tolerancia opcional: 1 compás distinto de cada 8.
3. **Bucle principal.** El de mayor cobertura, agrupando las rotaciones. Si empatan, el que cae en el `chorus`. Una canción está «hecha entera» sobre un bucle si este cubre al menos el 80 % de los compases.
4. **Fase.** La casilla 1 es el compás donde el bucle empieza línea con más frecuencia. Publicar también las 4 rotaciones.
5. **Casillas.** Cada casilla dura L/4. La entrada es el acorde al empezar la casilla y la salida, el acorde al terminar. Tres acordes de igual duración no encajan: se marcan como ambiguos.
6. **Fronteras.**
   - **F**: 1, 2, 5, 7, 10 u 11 semitonos.
   - **D**: 3, 4, 6, 8 o 9.
   - **H**: el acorde sigue, o cambia solo de calidad (`H*` para `IV→iv`).
   - Reportar dos versiones: H contado como D y H excluido.
7. **Bucles de 2 acordes:** aparte.
8. **Modelo nulo.** Permutar los acordes conservando las duraciones (unas 1.000 veces) y rotar la fase. Contrastar con una prueba de permutación.
9. **Salida:** un CSV por canción con el bucle, P, la fase, la cadena F/D/H, las anclas, el tipo, si el destino es la tónica y el bajo.

## 4. Formas de las paletas [C, sim]

| Paleta | Clip | Anclas | Tipo |
|---|---|---|---|
| casa mayor | Periodo I–IV–V | C C C C | extremo (conectores) |
| casa mayor | Préstamo ♭III | I S C D | *disc*, destino en 4 |
| casa mayor | I–♭VII–IV–I | S C C D | *cascade*, destino en 4 |
| casa menor | i7–IV7 | C C C C | extremo |
| casa menor | Andino i–♭III–iv–V | D S C C | *cascade*, destino en 1 |
| vuelta | De los 50 | D I S C | *disc*, destino en 1 |
| vuelta | Axis | C C D S | *cascade*, destino en 3 (el vi) |
| vuelta | ii–V–I | C C D S | *cascade*, destino en 3 |
| giro | Nubarrón ♭VI | I I S D | **apagado** |
| giro | *Creep* | D S D S | **extremo señal/destino** |
| giro | Rota I–IV–V–vi | S C C D | *cascade*, destino en 4 |
| hipnosis | Dórico vamp | C C C C | extremo |
| hipnosis | Mixo *Sweet Home* | C C C C | extremo |
| abismo | Lidio flotante | C D I S | *disc*, destino en 2 |
| abismo | Andaluza | C C C C | extremo |
| abismo | Napolitana | C C C C | extremo |

(C = conector, D = destino, S = señal, I = isla.)

- 11 de los 23 clips son todo conectores, repartidos entre casa menor, hipnosis y abismo.
- «Giro» es la única paleta con firma propia: 8 de sus 16 fronteras son débiles.
- «Todo conectores» se parece a hipnosis y abismo: nunca aterriza. [I]
- **La Axis sí cierra, pero en el vi:** la cadencia rota `V→vi` es su llegada. Esto matiza el «no cierra nunca» de `progression-axis.md` §3 y encaja con Richards (2017). [I]

## 5. Conclusión
- **Sí** como diagnóstico, en el taller y en `Tools~/surprise`.
- **Sí** como guarda de forma, no de acordes concretos.
- **No** como etiqueta de carta para el jugador.
- **Como segundo eje, solo por la posición.** En la biblioteca, llegar por movimiento débil tiene una mediana de 3,06 bits de sorpresa de estilo frente a 2,11 por movimiento fuerte (44 frente a 179 cambios), así que la división fuerte/débil se solapa con la sorpresa. Lo nuevo es dónde cae la llegada en el hipermetro, que el PPM no ve. [C, sim]

## Acciones propuestas

**Taller (ALWTTT)**
1. Mostrar la forma antes y después de cada nivel: cadena F/D, anclas y tipo. Es portar `slots()` de `scripts/shape.js`.
2. Nivel 2: ninguna adición puede cambiar la cadena F/D.
3. Nivel 3: un cambio de forma cuenta como 2 eventos y nunca se aplica en las cartas «giro».
4. `CADENCE` pasa a ser el conjunto de movimientos fuertes completo, {1, 2, 5, 7, 10, 11}.
5. No quitar el iv6 ni el ♭VI. Opción de gusto: `♭VI–♭VII` en lugar del ♭VI solo.
6. No usar la forma como etiqueta de carta.

**MidiGenPlay (`Tools~/surprise`)**
1. `loop_shape.py` con el algoritmo de la sección 3 (el piloto está en `scripts/loops12.py`).
2. Publicar el control que separa los acordes sostenidos de las terceras reales.
3. Añadir la forma como columna en `progressions.csv`.
4. Hipótesis para la prueba de escucha: los bucles de todo conectores se oyen más «hipnóticos» que los *cascade*.

**Music Lab**
1. Recuperar `recursos-armonicos.md`.
2. Proponer la casilla ③ «forma del bucle (12tone)». No suma casos.
3. Anotación pendiente de Claudio en `progression-axis.md` §3: la Axis «cierra en el vi».

## Fuentes
- de Clercq y Temperley (2011), «A corpus analysis of rock harmony», *Popular Music* 30(1), 47–70. [PDF](https://rockcorpus.midside.com/2011_paper/declercq_temperley_2011.pdf)
- Meeùs (2000), *MTO* 6.1. [Enlace](https://www.mtosmt.org/issues/mto.00.6.1/mto.00.6.1.meeus.html)
- Schoenberg (1969), *Structural Functions of Harmony* (citado a través de de Clercq y Temperley).
- Temperley (2007), *Popular Music* 26, 323–341.
- Nobile (2015), *Music Theory Spectrum* 37(2) (citado de memoria; hay que comprobarlo).
- Tagg, *Everyday Tonality* (a través de [Ethan Hein](https://ethanhein.substack.com/p/philip-taggs-everyday-tonality)).
- Richards (2017), *MTO* 23.3; Spicer (2017), *MTO* 23.2 (de memoria).
- Lerdahl (2001), *Tonal Pitch Space*.
- Bigand, Parncutt y Lerdahl (1996); Bharucha y Krumhansl (1983); Krumhansl (1990) (de memoria).
- [Backdoor progression](https://en.wikipedia.org/wiki/Backdoor_progression) y [Open Music Theory, «Popular Music»](https://pressbooks.nebraska.edu/openmusictheory/?p=558).
- Cheung et al. (2019), *Current Biology* 29(23).

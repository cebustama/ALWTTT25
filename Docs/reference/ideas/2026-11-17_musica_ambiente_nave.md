# Idea: música generativa de ambiente para la nave

Idea de Claudio del 2026-11-17, tras oír el instrumento «Reglas que escuchan a las demás» (al estilo de *Scape*) en el wrapped «Semillas de Eno» (https://claude.ai/artifact/UAoMovMkV7eJV842Y7uMvQ). Es una idea, no un plan.

**Lo que dijo Claudio:** «Me gusta muchísimo el artefacto. Sobretodo el ejemplo de Scape, me parece increíble cómo va ilando armonías con las reglas sencillas. De hecho, hasta me parece una opción para implementar con MidiGenPlay, pero no para los conciertos, sino para la música del juego. Por ejemplo, en la pantalla de la nave, antes de ingresar a ninguna habitación, el feeling de la música generada por esta herramienta me transmite esa "paz del espacio"»

## 💡 Notas del agente

- **Precedente directo:** Chilvers llegó a la música generativa por *Creatures*, y con Eno hizo la de *Spore*: música para jugadores que se quedan mucho tiempo en un sitio, donde un bucle se vuelve irritante. Es el caso del hub de la nave (S5, NAVE-1).
- **Cómo podría ser:** elementos con paletas (fondo, cielo, primer plano, tierra) y reglas que miran a los demás, como en *Scape*. Los acordes y el dron saldrían de MidiGenPlay, con la semilla de la partida.
- **Cada sala, una paleta:** al pasar por encima de una sala, entra un elemento suyo; las salas abiertas más adelante suman paletas, como los niveles de *Scape* (23:07). Encaja con `2026-11-12_niveles_de_pista.md`.
- **Fuentes:** `MidiGenPlay/Documentation~/research/2026-11-17-eno-chilvers-scape.md` y `2026-11-17-eno-variedad-1976.md`; el código del instrumento está en `MidiGenPlay/Documentation~/research/eno-musica-generativa/semillas-de-eno.html` (sección 8).
- **Gramática de Schafer** (*The Soundscape*, 1977; ficha `MidiGenPlay/Documentation~/research/2026-11-17-schafer-soundscape.md`): **keynote** = el dron del motor y del espacio, que se oye sin escucharse; **señales** = lo que pasa (una puerta, un aviso); **soundmark** = el sonido propio de cada sala. Dos ideas suyas: en Skruv (Suecia, 1975) los zumbidos eléctricos formaban una tríada y el silbato del tren la volvía séptima, «el pueblo tocaba melodías» (p. 99): el dron de la nave puede estar afinado con el acorde de MidiGenPlay. Y en Lesconil los sonidos llegaban en orden a lo largo del día (p. 216): la nave podría «dar la hora» del viaje. Su aviso: el ambient funcional es el Moozak que critica (pp. 96–98); que la música de la nave invite a escuchar, no solo a tapar.
- **Más reglas que escuchan** (Oliveros, `2026-11-capturas.md` del 2026-11-17): *The Greeting* (un tono por miembro que entra) y los tres modos de *Sound Cycles* (sola, en diálogo, juntos).
- **Cuándo:** después de la demo. No cambia el foco (CARTAS-ARTE-1).

## Segunda idea de Claudio (2026-11-17): la nave suena por capas que se desbloquean

Tras leer lo de Skruv (Schafer, p. 99: los zumbidos eléctricos de un pueblo formaban una tríada y el silbato del tren la volvía séptima).

**Lo que dijo Claudio:** «Esto me parece absolutamente genial como base para el diseño sonoro de la nave. Como la idea es que la nave vaya "desbloqueando" habitaciones, podría comenzar con solo el zumbido del motor, y tal vez "campanas" según el brillo de estrellas, sonidos asociados a estrellas fugaces, y eventualmente al activar habitaciones se van activando nuevas capas generativas de la composición con sus propias reglas. Habría que pensar cómo empezar, ya que la nave desde el comienzo tiene la consola de navegación y la habitación (para elegir el lineup). Eventualmente se agregaría la sala de ensayo, la tienda del chatarrero, y el salón común o sala chillout. También está la sala del cerebro de la nave. Lo anoto todo a modo de experimento, que se debería probar primero en web antes de llevarlo a ALWTTT.»

### 💡 Notas del agente (borrador para el experimento)

| Capa | Cuándo entra | Papel (Schafer) | Regla generativa posible |
| --- | --- | --- | --- |
| Zumbido del motor | Siempre | Keynote | Dron afinado a la tónica del acorde actual de MidiGenPlay, como en Skruv. |
| Campanas de las estrellas | Siempre | Señales suaves | La probabilidad de cada nota depende del brillo de una estrella del fondo; tono de la escala del acorde. |
| Estrellas fugaces | Siempre, raras | Señal | Un glissando o arpegio rápido cuando cruza una; solo si no suena otra señal (regla «solo si…» de *Scape*). |
| Consola de navegación | Desde el inicio | Soundmark | Pulsos o «telemetría» rítmica; en diálogo con las campanas (modo *Sound Cycles*). |
| Dormitorio (lineup) | Desde el inicio | Soundmark | Un tono por músico de la banda elegida (*The Greeting*, Oliveros). |
| Sala de ensayo | Al desbloquearse | Capa nueva | Fragmentos de las cartas del mazo, en eco lejano. |
| Tienda del chatarrero | Al desbloquearse | Capa nueva | Percusión metálica de chatarra, en los huecos (modo «sola»). |
| Salón común / chillout | Al desbloquearse | Capa nueva | Pad cálido que acompaña (modo «juntos»). |
| Cerebro de la nave | Al desbloquearse | Capa nueva | Arpegios que siguen la progresión (es quien «piensa» la armonía). |

- **Cómo empezar:** con lo que existe hoy (consola y dormitorio), la nave ya tiene cuatro capas: motor, estrellas, consola y banda. Cada sala nueva suma una capa con sus reglas, como las paletas que *Scape* desbloquea con el uso (23:07).
- **Primero en la web:** un experimento en una sola página, extendiendo el instrumento 8 de «Semillas de Eno», con botones para «desbloquear» salas. Luego, si funciona, MidiGenPlay (acordes) + ALWTTT (capas por sala).
- **Fuentes:** Schafer (`MidiGenPlay/Documentation~/research/2026-11-17-schafer-soundscape.md`), Oliveros (`2026-11-17-oliveros-*.md`), Eno y Chilvers (`2026-11-17-eno-chilvers-scape.md`).
- **Cuándo:** después de la demo, como experimento.

## Enlace (2026-11-19)

- **Secuenciador por grafo:** 60 osciladores de sierra que se disparan en cadena, con brazos a tempos distintos (patch de Max/MSP de iftah.is.my.name). Cada sala de la nave podría ser un brazo. Nota en `MidiGenPlay/Documentation~/research/2026-11-capturas.md` (2026-11-19).

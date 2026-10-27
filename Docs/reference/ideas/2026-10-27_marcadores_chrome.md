# Referencias externas de ALWTTT desde los marcadores de Chrome — 2026-10-27

**Tipo:** referencia (registro, no autoridad). Si algo de aquí cambia el plan, el cambio se aplica en su doc
gobernado (`S5_DemoCutClose_Sub_Roadmap.md`, `CURRENT_STATE.md`, SSoT correspondiente); este archivo solo lo
registra.
**Fuente:** análisis de los 2.223 marcadores de Chrome de Claudio (`D:\Projects\marcadores-chrome`, fuera del
repo). Los números `i` son las filas de `catalogo.csv` de ese proyecto.
**Objetivo que sirve:** la demo pública del 11-dic, que debe entenderse sin explicarla (M1, previsualización de
cartas, es la prioridad máxima según `S5_DemoCutClose_Sub_Roadmap.md`).

## Qué está comprobado y qué no

- **Ninguno de los vídeos de esta nota tiene transcripción** (895, 803/1017, 385/958, 902, 802, 331). Lo que se
  dice de ellos sale del título, la descripción y las fichas de `profundo/fichas_musica.md`. Lo que sí está
  leído entero es el one-pager 58 (PDF propio).
- Los documentos de Google (1114, 951, 964, 773, 795) son privados: **no se pudo abrir su contenido**. De 1114
  solo se sabe título, fecha (2016-12-19) y carpeta (`Band Manager`).
- Cada afirmación va marcada: **[ficha]** = dicho por el título o la descripción; **[inferido]** = lectura del
  agente, a verificar viendo el vídeo.

## 1. Previsualización y legibilidad (M1)

| i | Referencia | Qué aporta | Estado |
| --- | --- | --- | --- |
| 895 | Ars Technica, *How Slay the Spire's Original Interface Almost Killed the Game* | Documental sobre la interfaz original de Slay the Spire; la ficha dice que la «intent» (mostrar el siguiente movimiento enemigo) nació de playtests. | [ficha] la tesis; [inferido] que el paralelo sea el telegraph de intenciones del público de ALWTTT |
| 803 (= 1017) | Game Maker's Toolkit, *How Synergies Make Slay the Spire Fun* | Cita el «Lenticular Design» de Magic: profundidad para expertos y claridad para novatos. | [ficha]; [inferido] que cuadre con `Preferences (Synergies)` del público (one-pager 58) |
| 902 | Design Doc, *What Makes Good Rhythm Game UX?* | «Rhythm games with bad UX can be dead in the water.» ALWTTT no es de ritmo puro, pero la lectura de qué suena y cuándo importa en una demo en vivo. | [ficha] |
| 58 | One-pager propio «Gig Combat Economy» (v8, ene 2026) | Explicación del bucle en una página. Dice «Loss: Cohesion reaches 0», coherente con D-DIAG-1. | leído entero; cotejar con `CURRENT_STATE.md` por si v8 quedó atrás |

Lectura para M1 (inferida): lo que muestran 895 y 803 es la misma regla para un deckbuilder: el jugador decide
mejor si ve la consecuencia **antes** de comprometerse. Encaja con la nota #130 de Keep (rango posible de daño,
`ideas/2026-10-24_keep_ideas.md` §1) y con el problema de la reunión de arte («cuesta proyectar el daño»).
Aplicación concreta a decidir en el batch LEGIB-1, no aquí.

## 2. Estrategia y alcance de la demo

- **385 (= 958), Casey Yano, GDC 2019, *Slay the Spire: Success through Marketability*** [ficha]: valor
  añadido, comunidad temprana y claridad de propuesta. Guardado dos veces (2024 y después). Útil para el
  plan posterior a la demo (Discord como comunidad), no para el recorte actual.
- **59, Tom Francis, *15 Years of Indie Dev In 4 Bits of Advice*** (8-ene-2026) [ficha]: elegir proyecto por lo
  fácil de prototipar y preparar el prototipo para que otros lo entiendan. Respalda la lista cerrada M1–M7 y el
  tope de ≤3 smokes humanos por batch; es criterio, no tarea.
- **775, Figma, *Game Pitch Examples & Templates*** y **773, pitch de Gamescom 25** (privado): material de pitch
  para cuando se rehaga la descripción de la demo o de Steam [ficha].

## 3. Antecedentes del proyecto

- **1114, *Music Tycoon GDD*** (Google Docs, 2016-12-19, carpeta `Frisson/ALWTTT/Band Manager`): semilla de
  ALWTTT, ocho años antes de la demo. **Contenido no leído (privado).** Valor: relato del proyecto y contraste
  con el diseño actual. Acción posible: que Claudio exporte el documento a `.md` o PDF si quiere que se
  compare con REPLAN-DIAG.
- **951** (GDD de la reedición, sept 2024) y **964** (hoja *ALWTTT Cards*, oct 2024), privados: antecedentes de
  la lista actual de cartas; comprobar si alguna sigue siendo fuente o ya manda el repo.
- **795** (*Publisher Messages*, 2025), privado: sin relación con la demo del 11-dic.

## 4. Otros guardados con la etiqueta ALWTTT, descartados para la demo

- **609, keijiro/Minis** (entrada MIDI en el Input System): «only reacts to notes with a velocity higher than
  63» (leído en el README). Permitiría tocar con un teclado MIDI, pero no está en M1–M7: queda como idea
  posterior a la demo.
- **996, MPTK** (Pax Stellar): el texto descargado no corresponde al título (es un comentario); alternativa
  antigua a MidiGenPlay, no usar como fuente.
- **331** (música espacial «cósmica», Cadence Hira): vocabulario de tono para las escenas; sin uso en la demo.
- **802** (mecánicas con significado, GMTK; marcado también `colonizacion`): pertenece sobre todo al
  videojuego de la colonización.
- **1119, 2206:** sin relación con la demo (datos de gusto musical; bases del Fondo Audiovisual 2025, vencidas).

## 5. Qué hacer con esto (propuesta, sin compromiso)

1. Antes del batch LEGIB-1, ver 895 y 803 (~40 min en total) con la pregunta: «¿qué ve el jugador antes de
   decidir y cómo se dice sin texto?». Apuntar aquí lo que se confirme.
2. Si Claudio exporta 1114, 951 y 964, añadir un cotejo corto con la REPLAN-DIAG.
3. Dejar 385/958 y 59 para la planificación posterior al 11-dic.

## Addendum 2026-10-27: transcripciones leídas

Los videos ya se leyeron enteros (fichas en `D:\Projects\marcadores-chrome\profundo\fichas_videos.md`). Lo que antes venía de la descripción queda comprobado o se amplía:

- **895 (comprobado):** la primera versión de Slay the Spire mostraba la siguiente jugada del enemigo como texto, y solo al hacer clic sobre él. No servía para streaming, cargaba demasiado texto y se volvía incómoda con varios enemigos. Lo resolvió el *intent*: un icono sobre cada enemigo, con una escala visual de amenaza (la espada crece con el daño). Para ALWTTT: el intent del público siempre visible y como icono, no como texto.
- **385:** las pruebas en persona mostraron dónde se atascaba la gente. De ahí salieron los tooltips de palabras clave, el borde azul en las cartas jugables y el texto rojo cuando falta energía: legibilidad sin explicar nada. Ojo: la transcripción se corta en las preguntas del público.
- **803 (comprobado):** el «diseño lenticular» (Mark Rosewater): cartas simples por fuera, con profundidad oculta por las combinaciones. Pocas cartas que combinan dan muchas estrategias, en línea con la lista cerrada M1–M7.
- **902:** juegos de ritmo. Una ronda de práctica sin puntuar, fusionada con el tutorial, que calibra el retardo; una barra de timing que se puede apagar; y toda la escena animada al compás.
- **1230 (Portal):** pedir a quien prueba que cuente la historia de vuelta y recortar lo que no entiende. Es un método directo para la prueba de comprensión antes de la build candidata.

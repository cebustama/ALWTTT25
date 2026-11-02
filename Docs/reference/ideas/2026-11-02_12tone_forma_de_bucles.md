# La forma de los bucles de acordes (12tone, 2026)

**Tipo:** referencia (es un registro, no una autoridad). Viene de una captura de Claudio del 2026-11-02, dentro de la línea armónica acotada, «qué transmite cada patrón».
**No cambia la demo del 11 de diciembre.** Lo de la sección 3 son propuestas, no tareas.
**Etiquetas:** [V] comprobado en la transcripción o en el repo; [I] inferido.

## Fuente

- 12tone, «I Listened To 500 Songs To Prove A Point», YouTube, 30-oct-2026, 17:41 min. https://youtu.be/F7be6Aicdo0. Leída la transcripción completa en Chrome; no se guarda el texto. El guion público está enlazado en la descripción.
- Lo que dijo Claudio: «Cómo próximo paso, qué podemos aprender de este video?»

## 1. El modelo, en resumen [V]

- **Un bucle no es una progresión (min. 0:52–2:30).** Una progresión viaja hasta un cierre. El bucle *es* el lugar: un solo objeto armónico que gira y se oye desde distintos ángulos.
  - Así explica el «divorcio armónico-melódico» (término de Temperley): la melodía está «casada con el bucle», no con el acorde de cada momento.
  - Pone el ejemplo de *Fast Car*.
- **Forma = contorno + anclaje.**
  - **Contorno (2:54).** El movimiento entre dos acordes es **fuerte** por quinta justa o por grado (tono o semitono), y **débil** en lo demás, sobre todo por terceras. El tritono y los saltos disjuntos también son débiles: chocan, pero no resuelven.
  - **Anclaje (4:34).** Las funciones no dependen de la tonalidad, sino de cómo entra y sale cada acorde:
    - **isla:** entra débil y sale débil;
    - **conector:** entra fuerte y sale fuerte;
    - **señal:** entra débil y sale fuerte (abre una cadena de resoluciones);
    - **destino:** entra fuerte y sale débil (la cierra).
- **Método (9:29).**
  - Diez canciones al año entre 1976 y 2025, elegidas al azar del Billboard de fin de año.
  - Un bucle por canción, el más importante.
  - El «acorde» se cuenta como unidad de tiempo: el bucle se parte en 4 casillas. Si tiene 3 acordes, uno se duplica (movimiento débil hacia sí mismo). Si tiene 5, dos comparten casilla: el primero cuenta para la entrada y el segundo para la salida.
- **Resultados (12:03).**
  - El 47 % de las canciones tiene algún bucle, y una de cada cuatro está hecha entera sobre uno. Las dos cifras suben en los últimos 25 años.
  - **Tipos:** el 71 % son «equilibrados» (al menos un conector y un destino). De ellos, dos tercios son *cascade* (señal, conector, conector, destino) y el resto *disc* (destino, isla, señal, conector, más clásico, del doo-wop). El 22 % son «extremos» y el 7 % «apagados» (más de una isla, suenan «sin amarre»).
  - **Destinos por posición:** 100 en la 1, 64 en la 3, 48 en la 4 y 32 en la 2. La posición 2 es la del trance de la época EDM de los 2010.
  - **Casillas con dos acordes:** de 38, 35 tienen movimiento fuerte entre los dos y 33 hacen de señal o conector. Son un gesto que empuja.
- **Límites que reconoce el autor.**
  - Es una sola persona que pone a prueba su propio modelo, sin un segundo anotador.
  - Solo hay éxitos de radio.
  - Hereda los sesgos de las listas: el 70 % de las canciones las escribieron solo hombres. Los equipos con al menos una mujer, una persona no binaria o un compositor no blanco usaban más bucles, aunque la muestra es pequeña.

## 2. Qué dice sobre lo que ya tenemos [I]

- **Es un segundo eje para «qué transmite cada patrón».** La sorpresa de Cheung et al. (2019), la herramienta `MidiGenPlay/Tools~/surprise/`, mide la expectativa. La forma mide el reparto entre impulso y estructura. Los dos ejes se pueden calcular.
- **Contrasta con los niveles v3 del taller** (`../taller-armonia/README.md`):
  - **Encajan:** los adornos de medio compás son sus «dos acordes en una casilla». El sustituto tritonal (entra por semitono) y la dominante secundaria (cae una quinta) son movimiento fuerte, como en 35 de sus 38 casos.
  - **No encajan:** el `♭VImaj7 → I` (sube una tercera mayor) y el `iv6` después del IV (misma fundamental) son movimiento débil según el modelo, así que frenan en vez de empujar. Está pendiente que Music Lab diga si hay que cambiarlos o si el modelo se queda corto con la cadencia eólica y la línea cromática interior.
  - **Posible guarda nueva:** subir de nivel sin mover el destino del bucle.
- **Las paletas pueden tener forma** (especulativo, sin medir):
  - «apagado» o destino en la posición 2 ↔ *hipnosis* y *abismo*;
  - *cascade* ↔ *vuelta*.

## 3. Propuestas

1. Pedir a Music Lab que critique el modelo: solidez, literatura, aplicación a los niveles, algoritmo y formas de las paletas. **Pedido el 2026-11-02**; la respuesta irá a `../taller-armonia/`.
2. Repetir el estudio automáticamente sobre el McGill Billboard en `MidiGenPlay/Tools~/surprise/`: detectar bucles, partirlos en 4 casillas, etiquetar las funciones y comparar con su 71/22/7 y su 100/64/48/32. Va detrás de la prueba de oído de la v3 y de la encuesta.
3. Si el modelo aguanta, que cada carta declare **forma + sorpresa**. Será después de la demo.

## Para profundizar

- David Temperley (2007), «The melodic-harmonic "divorce" in rock», *Popular Music* 26(2). Es el origen del término.
- Drew Nobile (2020), *Form as Harmony in Rock Music* (Oxford). Está por verificar.
- La versión ampliada en Patreon trae el análisis por época y por tempo.

# Handoff para Music Lab: revisión del Taller armónico de ALWTTT

Prompt para pegar en el proyecto Music Lab (2026-11-02). Lo escribió el agente de ALWTTT desde la raíz de `D:\Projects`.

---

Hola. Te escribe el agente que trabaja en ALWTTT, mi videojuego. Te pido una **evaluación musical crítica**, no una validación. Si algo suena mal o está mal planteado, dilo; si algo de lo que afirmo no se sostiene, corrígelo citando la fuente.

## Contexto

ALWTTT es un deckbuilder: una banda toca canciones que se generan en tiempo real con MidiGenPlay, mi paquete de música procedural para Unity. Cada **carta de composición** elige una progresión de acordes de una «paleta» (familias como casa mayor, casa menor, vuelta, giro, hipnosis y abismo), y el público reacciona. La línea en curso se llama «**qué transmite cada patrón**»: que cada progresión declare qué quiere hacer sentir (lo familiar, un remate, una resolución, desconcierto) y que eso se pueda medir.

Lo hecho hasta ahora:
1. **Medición de la sorpresa** de cada acorde según Cheung et al. (2019), con un modelo PPM sobre el corpus Billboard y la biblioteca propia.
2. **Prueba de escucha a ciegas** con 25 muestras. Resultado: las progresiones gustan, pero casi ninguna sorprende. Mi conclusión: la biblioteca es demasiado «segura».
3. **Taller armónico**, una página web para escribir una progresión, oírla y subirla de nivel (es lo que te pido revisar).

## Qué puede tocar MidiGenPlay (restricciones reales)

- Se escribe en **grados romanos** dentro de un **modo** de referencia (Jónico, Dórico, Frigio, Lidio, Mixolidio, Eólico, Locrio), con la duración en compases: `iim7 – V7 (0.5) – Imaj7`.
- Hay **17 calidades de acorde**: mayor, menor, dim, aug, maj7, m7, 7, ø7, dim7, sus2, sus4, 6, m6, 7sus4, 9, maj9 y m9.
- **No hay** acordes de quinta (power chords), inversiones ni acordes con el bajo en otra nota (slash chords), ni 11, 13 o alteraciones (♭9, ♯11…).
- **Las alteraciones son relativas al modo**, no a la escala mayor. En Eólico, `bVII` baja el VII del modo, así que suena como el VI.
- Los compases van en 4/4 o 3/4, y el total de cada vuelta es un número entero de compases.

## Los niveles (decisión D6: la carta sube de nivel cuando la rejuegas)

La misma progresión se vuelve más rica cada vez que se repite la carta. Las reglas son deterministas y parten de lo que se escribió.

**Nivel 1:** tal cual.

**Nivel 2:**
- Las tríadas pasan a cuatriadas de la escala. Un acorde mayor que baja una quinta hacia el siguiente, y no es la tónica, pasa a 7; los demás mayores, a maj7 o a 7 según la escala. Los menores pasan a m7, y los disminuidos a ø7 o dim7.
- **Desde la segunda vuelta:**
  - un sus4 antes de cada 7 de un compás o más;
  - una **dominante secundaria** (V7 del acorde siguiente) en la última mitad de las posiciones impares.
- **En la última vuelta:** un ii–V que lleva de vuelta al primer acorde.

**Nivel 3:**
- Todo lo del nivel 2, pero con novenas.
- **Desde la segunda vuelta:**
  - **sustitutos tritonales** en lugar de las dominantes secundarias;
  - sobre el IV, un **iv6 prestado** en los modos mayores, o un **IV7 dórico** en los menores;
  - una **mediante cromática**: la vuelta a I pasa por ♭VImaj7, y la vuelta a i, por ♭IImaj7.
- **Antes de modular:** un **backdoor** (iv7–♭VII9), o bien iiø7 más el sustituto tritonal si el destino es menor.
- **La última vuelta sube medio tono** (cuando hay dos vueltas o más).

Ejemplo: la Axis en Jónico, nivel 2, con dos vueltas.
- Vuelta 1: `Imaj7 – V7 – vim7 – IVmaj7`.
- Vuelta 2: `Imaj7 – V7sus4(0.5) – V7(0.5) – vim7 – iim7(0.5) – V7(0.5)`.

Sé que en tus fichas de progresión ya usas **tres niveles** (sencilla, intermedia y reharmonizada) y que tu regla dice que los niveles 2 y 3 son propuestas y no cuentan como casos. Me interesa mucho comparar tus niveles con los míos.

## Qué te pido

1. **Las reglas de nivel.** ¿Suenan musicalmente bien como reglas generales, aplicadas a ciegas sobre cualquier progresión? ¿Cuáles fallan en qué contextos? Por ejemplo, un modal frigio o lidio, un riff de grunge o metal, un bucle de dos acordes, un blues menor. ¿Qué cambiarías, quitarías o añadirías dentro de las 17 calidades?
2. **La subida de dificultad.** ¿Es razonable la escalera 1 → 2 → 3 para que se perciba como «más sofisticada» sin perder la identidad de la progresión? ¿Hay un orden mejor, por ejemplo color antes que movimiento, o al revés?
3. **Compara con tus fichas de tres niveles**, sobre todo la Axis, I–vi–IV–V y el blues menor: dónde coinciden y dónde no, y cuál de los dos enfoques va mejor para un sistema automático.
4. **Las progresiones de estilo** que propuse para el catálogo, marcadas como «por verificar». Comprueba los grados y las atribuciones, y corrige lo que esté mal:
   - Royal road: `IVmaj7 – V7 – iiim7 – vim7`.
   - Just the Two of Us: `IVmaj7 – III7 – vim7 – vm7 (0.5) – I7 (0.5)`.
   - Isn't She Lovely: `iim7 – V7 – I6 – VI7`.
   - Ipanema, parte A: `Imaj7 (2) – II7 (2) – iim7 – bII7 – Imaj7 – bII7`.
   - Rhythm changes, parte A: `I – VI7 – iim7 – V7 – iiim7 – VI7 – iim7 – V7`, a medio compás cada uno.
   - Backdoor: `ivm7 – bVII7 – Imaj7 (2)`.
   - Teen Spirit: `I – IV – bIII – bVI`, a medio compás cada uno (en el original son acordes de quinta).
   - Riff frigio: `i (2) – II – i`, en Frigio.
   - Guantanamera: `I – IV – V7`.
5. **Huecos de estilo.** Quiero cubrir jazz, Stevie Wonder (soul), fusión japonesa (city pop y anime), Bill Wurtz, grunge, metal y bossa nova. Para cada uno, dime qué recursos armónicos lo definen y cuáles caben en las restricciones. Lo que no quepa, dilo, para proponerlo a MidiGenPlay (por ejemplo, acordes de quinta o con el bajo en otra nota).
6. **Lo que transmite cada modo.** El juego etiqueta los modos así: Jónico «alegre», Dórico «con groove», Frigio «oscuro», Lidio «soñador», Mixolidio «funky», Eólico «triste» y Locrio «tenso». ¿Se sostiene esa correspondencia? ¿Qué matices le faltan?

Responde con el mismo criterio de tus fichas: marca cada afirmación como Confirmado, Inferido o Sin respaldo, cita las fuentes y no reproduzcas letras ni tablaturas.

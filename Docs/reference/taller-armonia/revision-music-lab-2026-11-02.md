# Revisión de Music Lab al Taller armónico (2026-11-02)

Respuesta al handoff `handoff-music-lab.md`. La escribió un agente que hizo de Music Lab desde la sesión de la raíz: leyó las instrucciones, `recursos-armonicos.md` y las fichas de progresión de `D:\Projects\Music Lab`, buscó en la web y ejecutó `color` y `arrange` de `template.html` con node sobre 13 progresiones. No tocó ningún archivo. Es la revisión de un agente, no un caso observado: los niveles 2 y 3 son propuestas, según la regla de Music Lab.

Etiquetas: **[C]** confirmado (teoría estándar o fuente citada), **[I]** inferido, **[A]** sin respaldo verificable y **[C, sim]** comprobado en el simulador.

## Resumen

1. **El turnaround borra el último acorde** cuando dura un compás. La Axis pierde su IV, la andaluza su V, la Royal road su vi y el vamp lidio su II. [C, sim]
2. **Las reglas funcionales matan los modos.** V7 de, ii–V e iiø–V meten la sensible en Frigio, Dórico y Lidio, y borran el acorde característico. [C, sim]
3. **Novenas a ciegas.** Salen `iiim9` en Jónico, `im9` en Frigio (anula el ♭2) e `iim9` en Dórico. Solo debería haber 9 si la nota está en la escala. [C]
4. **La escalera mezcla color y movimiento desde el nivel 2.** Mejor: nivel 2 = color + 1 puntuación; nivel 3 = movimiento con un presupuesto de 2 eventos. Encaja con Cheung et al. (2019): una sorpresa sobre un contexto predecible. [I]
5. **La subida de medio tono no sirve en un bucle**: sube y baja en cada ciclo. Mejor como evento de final. [C, sim]
6. **Ejemplos de estilo.** Bien: Royal road, Ipanema A, Backdoor, Riff frigio y Guantanamera. Hay que corregir *Isn't She Lovely* (en Mi mayor: `vim7 – II9 – V7sus4 – I`), *Teen Spirit* y *Just the Two of Us* (los dos en Fa menor, Eólico). *Rhythm changes* solo cubre los compases 1–4.
7. **Etiquetas de modo.** Solo el orden de la alegría tiene respaldo experimental (Temperley y Tan 2013; el Lidio sale menos alegre que el Jónico). «Soñador», «funky» y «con groove» son etiquetas de estilo, no de emoción.

## 1. Las reglas aplicadas a ciegas (simulador, 2 vueltas)

| Caso | Qué sale | Diagnóstico |
|---|---|---|
| Axis N2 v2 | `Imaj7 – V7sus4(.5) – V7(.5) – vim7 – iim7(.5) – V7(.5)` | Desaparece el IV. La Axis funciona porque **no cierra**, y el N2 ya la cierra. [C, sim] + [I] |
| Axis N3 v1 | `Imaj9 – V9 – vim9 – bvm7* – VII9*` | Backdoor hacia +1; el N3 nunca tiene IV. [C, sim] |
| Andaluza N3 v1 | `im9 – VII9 – VImaj9 – iiiø7* – II7*` | Se pierde el V, la meta del tetracordo. [C, sim] |
| Riff frigio N2 v2 | `im7(2) – IImaj7(.5) – V7*(.5) – iiø7–V7…` | Mete la sensible: ♭II→i pasa a ser V7→i. Deja de ser frigio. [C, sim] |
| Riff frigio N3 | `im9` | El 9 natural borra el ♭2. [C] |
| Vamp lidio `I – II`, N3 | `Imaj9 – ivm7* – VII9*` | Desaparece el II, el acorde del ♯4. [C, sim] |
| Vamp dórico `im7 – IV7`, N3 | `im9 – iiiø7* – II7*` | Desaparece el IV mayor. [C, sim] |
| Blues mayor N2 v2 | 12 parejas `X7sus4(.5) – X7(.5)` | El sus4 salta en cada 7. [C, sim] |
| Blues mayor N3 v2 | `bVImaj7` en el c. 2; un iv6 tras cada IV | Rompe la forma de 12 compases. [C, sim] |
| Teen Spirit (Jónico) N2 | `Imaj7 – IVmaj7 – bIII7 – bVImaj7` | El grunge suena a lounge. [C, sim] + [I] |
| Royal road N2 v2 | `… iiim7 – vm7* – I7*` | Se pierde el vim7 final. [C, sim] |
| Ipanema N3 v2 | Imaj7 del c. 7 → `bVImaj7*` | Se pierde el regreso `iim7–♭II7–Imaj7`. [C, sim] |

**Lo que funciona [I]:**
- En I–vi–IV–V, nivel 2: `vim7(.5) – I7*(.5) → IVmaj7` (V7/IV).
- El nivel 2 del blues menor, casi igual al de la ficha.
- Empezar el movimiento en la vuelta 2.

**Fallos de concepto [C]:**
- El ♭II no es una mediante cromática (está a un semitono: es la napolitana; además, en Frigio es diatónico), y el código sustituye la tónica en vez de insertar el acorde.
- El IV7 dórico se aplica en Frigio y Locrio.
- En 3/4, el medio compás da 1,5 tiempos. [I]

## 2. Escalera propuesta [I]

- **Nivel 2:** cuatriadas según el estilo + **1** evento por vuelta (sus4 en la dominante funcional, o turnaround en la segunda mitad del último acorde).
- **Nivel 3:** novenas solo si están en la escala + **≤2** eventos por vuelta, elegidos de esta lista: dominante secundaria o sustituto tritonal, préstamo iv6 o IV7, ♭VI antes de I.
- **Ningún evento en la vuelta 1.**
- Cada acorde original conserva al menos su primera mitad.
- Base: Cheung et al. (2019), con más placer cuando la incertidumbre es baja y la sorpresa alta. El nivel 3 actual sube las dos a la vez. Comprobarlo con la herramienta PPM.

## 3. Comparación con las fichas de Music Lab

| | N2 de Music Lab | N2 del taller | N3 de Music Lab | N3 del taller |
|---|---|---|---|---|
| Axis | `D/F#` + `IV→iv` en medio compás | Cuatriadas; ii–V **sustituye** al IV | `Imaj7 – III7 – vim7 – iim7 V7` («ya es otra progresión») | Novenas, backdoor, iv6; sin IV |
| I–vi–IV–V | `IV→iv` + `V7` | Cuatriadas + `I7` (V/IV) | `Imaj7 – VI7 – iim7 – V7` | Novenas, ♭V7, iv6, backdoor |
| Blues menor | Séptimas, quick change, V7 en el c. 12 | Parecido | ii–V intercalados, A7→Dm7, `D♯°7`, sustituto tritonal en el c. 10 | Napolitana, IV7 en cada iv, modulación |

**Diferencias:**
- Music Lab ordena por **identidad**; el taller, por **técnica**.
- Music Lab elige recursos propios de cada progresión; el taller aplica la misma lista a todas.

**Para un sistema automático, propone un enfoque híbrido:**
- operadores deterministas con guardas de identidad;
- un perfil por familia (blues, riff, modal, funcional);
- niveles escritos a mano para las 5–10 cartas de escaparate.

Las fichas de Music Lab sirven como **prueba de oro**.

## 4. Ejemplos de estilo verificados

| Ejemplo | Veredicto | Import |
|---|---|---|
| Royal road | ✔ Wikipedia (Ramage 2023, *Music Theory Spectrum*); Yumi Arai (1974), city pop, eurobeat | Jónico `IVmaj7 – V7 – iiim7 – vim7` |
| Just the Two of Us | ⚠ En Fa menor: `D♭maj7 – C7♭9 – Fm7 – E♭m7 – A♭7` (Wikipedia). Compositores: Withers, MacDonald y Salter. En Japón, 丸サ進行 (Sheena Ringo). Duraciones [A] | Eólico `VImaj7 – V7 – im7 – viim7 (0.5) – III7 (0.5)` |
| Isn't She Lovely | ✘ Hooktheory: Mi mayor, `vi7 – V9/V – I`; el tercer acorde es una dominante sus | Jónico (Mi) `vim7 – II9 – V7sus4 – I` |
| Ipanema A | ✔ Partitura de 1963 (`Music Lab/Análisis/analysis-girl-from-ipanema.md`) | Sin cambios |
| Rhythm changes A | ⚠ Solo cubre los c. 1–4; los c. 5–8 son `I – I7 – IV – iv – I – V – I` (Wikipedia) | Jónico, A completa: `I6 (0.5) – VI7 (0.5) – iim7 (0.5) – V7 (0.5) – iiim7 (0.5) – VI7 (0.5) – iim7 (0.5) – V7 (0.5) – I (0.5) – I7 (0.5) – IV (0.5) – ivm7 (0.5) – I (0.5) – V7 (0.5) – I` |
| Backdoor | ✔ Teoría estándar; sin ejemplos verificados [A] | Sin cambios |
| Teen Spirit | ✘ Hooktheory: Fa menor, `i – iv – III – VI` (no3), con salida en Fa frigio | Eólico `i (0.5) – iv (0.5) – III (0.5) – VI (0.5)`; sin tercera: `Isus2 (0.5) – IVsus2 (0.5) – III (0.5) – VI (0.5)` [I] |
| Riff frigio | ✔ Grados correctos; «genérico del metal» [I], sin ejemplo de metal verificado. Ejemplo verificado: la salida de *Teen Spirit* | Sin cambios |
| Guantanamera | ✔ I–IV–V7; variante `I IV V IV`. Crédito de Joseíto Fernández (Tribunal Supremo de Cuba, 1993) | Sin cambios; variante `I (0.5) – IV (0.5) – V7 (0.5) – IV (0.5)` |

En Eólico, `VI` y `III` se escriben **sin** bemol.

## 5. Huecos de estilo

- **Jazz.** ✔ ii–V–I, sustituto tritonal, backdoor, °7 de paso y rhythm changes. ✘ 13, 7♭9, 7alt y bajo caminante.
- **Soul (Stevie Wonder).** ✔ 7sus4, 9 y m9. ✘ 13 y 7♯9. El IV/V se aproxima con `V7sus4`.
- **City pop y anime.** ✔ Royal road y 丸サ. ≈ IV/V. ✘ add9, 11 y 13.
- **Bill Wurtz.** [A] No hay ningún análisis verificable (2026-11-13: ya hay **una fuente secundaria por verificar**, el análisis de Pusher del coro de «La De Da…», en `../ideas/2026-11-13_bill_wurtz_forma_y_armonia.md`); con maj9, m9 y mediantes se aproxima. ✘ Bajo en otra nota.
- **Grunge.** ✘ Acordes de quinta. ♭III, ♭VI y ♭VII sobre menor; salidas frigias (Hooktheory).
- **Metal.** ✘ Acordes de quinta y pedal. i–♭II frigio y tritono i–♭V [I]; Walser (1993) sin verificar [A].
- **Bossa.** ✔ La armonía de la A. ✘ 6/9, 13 y bajo alternado.

**Pedidos a MidiGenPlay, por rendimiento [I]:**
1. `5` (acorde de quinta).
2. Bajo en otra nota (slash).
3. `7b9`.
4. `13`.
5. `add9` / `6/9`.

## 6. Etiquetas de modo

- **Respaldo experimental [C].** Temperley y Tan (2013, *Music Perception* 30(3)): la alegría baja a medida que se bajan grados, salvo el Lidio, que sale menos alegre que el Jónico. Hay que comprobar el año de la cita en la revista.
- **Jónico, Eólico y Frigio:** el orden se sostiene. «Triste» simplifica: según el tempo, el menor también suena épico o furioso. [I]
- **Etiquetas de estilo:** «soñador» (Lidio), «funky» (Mixolidio) y «con groove» (Dórico) son de estilo, no de emoción.
  - Para Mixolidio, propone «alegre sin cierre / rockero».
  - Para Dórico, «menor luminoso».
- **Locrio:** mejor «inestable» que «tenso».
- **Que falta [I]:**
  - separar la valencia de la asociación de estilo;
  - una progresión que no toca el grado característico no suena modal;
  - el tempo, el registro y el ritmo armónico pesan tanto como el modo.

## Cambios propuestos a las reglas (por prioridad)

1. **Turnaround.** Solo en la segunda mitad del último acorde. Se omite si el paso último→primero ya es una cadencia (V→I, IV→I, ♭VII→I, ♭II→i, V→i) o si el bucle tiene ≤2 acordes distintos.
2. **Nunca borrar un acorde original.** Los añadidos solo parten duraciones.
3. **Operadores modales.** Fuera de Jónico y Eólico, la aproximación usa el acorde característico:
   - Frigio: `IImaj7` / `II7`;
   - Dórico: `IV7`;
   - Lidio: `II7`;
   - Mixolidio: `bVII` / `vm7`;
   - Locrio: nada.
4. **Novenas.** Solo si `inScale(r+2)`.
5. **sus4.** Solo en la dominante funcional, una vez por vuelta.
6. **Presupuesto.** Nivel 2: ≤1 evento por vuelta; nivel 3: ≤2; ninguno en la vuelta 1.
7. **Medio tono.** Fuera de la carta: evento de final de canción. Es un recurso en retirada: ~25 % de los número 1 de 1960–1990 y uno solo en los 2010 (Wikipedia).
8. **«Mediante».** En los modos mayores, **insertar** `bVImaj7` (o `bVI – bVII`) antes de I. En los menores, quitar la napolitana; como opción, una picardía `I` al final de la canción.
9. **Préstamos.** iv6 en Jónico, Lidio y Mixolidio; IV7 solo en Eólico; nada en Frigio ni Locrio; una vez por vuelta.
10. **Perfil de estilo por carta.**
    - blues: 7 en I, IV y V;
    - riff: sin séptimas, tríada o sus2;
    - pop: tónica en `6` o tríada;
    - jazz, bossa y city pop: `maj7`.
11. **Dominantes secundarias.** Antes de un acorde diatónico que no sea la tónica, si el acorde actual dura ≥1 compás (no «posiciones impares»).
12. **3/4.** Partir en 2+1 tiempos, o no partir.

## Fuentes

- **Music Lab:**
  - `Análisis/Progresiones/progression-axis.md`, `progression-i-vi-iv-v.md` y `progression-blues-menor.md`;
  - `Análisis/analysis-girl-from-ipanema.md`.
- **Páginas web:**
  - [Royal road progression](https://en.wikipedia.org/wiki/Royal_road_progression);
  - [Just the Two of Us](https://en.wikipedia.org/wiki/Just_the_Two_of_Us_(Grover_Washington_Jr._song)) y [nippon.com, 丸サ進行](https://www.nippon.com/ja/japan-topics/g01096/);
  - [Isn't She Lovely, en Hooktheory](https://www.hooktheory.com/theorytab/view/stevie-wonder/isnt-she-lovely);
  - [Rhythm changes](https://en.wikipedia.org/wiki/Rhythm_changes);
  - [Smells Like Teen Spirit, en Hooktheory](https://www.hooktheory.com/theorytab/view/nirvana/smells-like-teen-spirit);
  - [Guantanamera](https://en.wikipedia.org/wiki/Guantanamera);
  - [Truck driver's gear change](https://en.wikipedia.org/wiki/Truck_driver%27s_gear_change).
- **Artículos:**
  - Temperley y Tan (2013), *Music Perception* 30(3) (a través de un resumen secundario; comprobar la cita);
  - Cheung et al. (2019), *Current Biology* 29(23).
- **Sin verificar [A]:**
  - ejemplos de metal en i–♭II;
  - Walser (1993);
  - análisis de Bill Wurtz;
  - las duraciones de *Just the Two of Us*;
  - el `#ivdim7` en rhythm changes;
  - ejemplos del backdoor.

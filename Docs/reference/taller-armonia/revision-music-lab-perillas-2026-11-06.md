# Revisión de Music Lab: perillas de los niveles (2026-11-06)

Respuesta a la consulta sobre cómo convertir las reglas de `arrange` (v3) en perillas que un LLM pueda ajustar. La escribió un agente que hizo de Music Lab: leyó `CLAUDE.md`, `project_instructions.md` y `recursos-armonicos.md` de `D:\Projects\Music Lab`, las fichas de `Análisis/Progresiones/`, el README del taller, `color`, `tonicApproach` y `arrange` en `template.html`, `revision-music-lab-2026-11-02.md` y `prueba-oido-v3.md`. Solo ha leído y buscado; no ha ejecutado nada. Como la revisión anterior, es una propuesta y no un caso observado: no suma casos en `recursos-armonicos.md`.

Etiquetas: **[C]** confirmado (código leído, teoría estándar o fuente citada), **[I]** inferido y **[A]** sin respaldo verificable.

## Resumen

1. **Tres perillas tal como están propuestas no reproducen la v3**, porque en el código dependen de `level` y no de una regla propia [C, código]:
   - `tonicApproach(level)`: con N2 el remate a la tónica es `V7`; con N3 es el backdoor `iv7–♭VII9` (Jónico) o `iiø7–♭II7` (Eólico). En Mixolidio, `♭VII` es tríada en N2 y `maj7` en N3.
   - `color(c, next, level === 3)`: las novenas van dentro de `color`. Novenas sin color es imposible.
   - Regla 4: la misma casilla da dominante secundaria (N2) o sustituto tritonal (N3). Las dos a la vez no tienen orden definido.
   Por eso propongo **fusionar** `color` + `novenas` en una perilla de tres valores, `dominanteSecundaria` + `sustitutoTritonal` en otra de tres valores, y convertir `vueltaAlInicio` en una perilla de tres valores (no / simple / rica).
2. **Presupuesto: de 0 a 3**, con un tope automático de `n − 1`, donde n es el número de acordes del bucle. La guarda de «≤2 acordes distintos → 1» se queda fija y no como perilla. [I]
3. **Falta una perilla para la nota 1 de la prueba de oído**: «el adorno del nivel 2 añade un acorde» (`preferirAcordeNuevo`), que adelanta la dominante secundaria sobre el sus4. [I]
4. **`tritonoSoloSiCromatica` está bien encaminada**, pero le falta decir qué pasa cuando se apaga el tritono. Propongo: si la progresión es toda de la escala, el remate hacia un acorde que no es la tónica usa el `V7` del destino (como en N2). [I]
5. Diccionario de 12 frases y 7 reglas de generación, con 10 frases de prueba, al final.

## 1. Perillas: nombres, valores y ayuda

Nombres pensados para un músico que no lee teoría. La ayuda es la línea que se ve al pasar el dedo o el ratón.

| Clave (código) | Nombre en la interfaz | Valores | v3: N2 / N3 | Ayuda (una línea) |
|---|---|---|---|---|
| `presupuesto` | **Cambios por vuelta** | 0–3 | 1 / 2 | Cuántos adornos como máximo entran en cada vuelta. 0 = solo cambia el color. |
| `desdeVuelta` | **Empieza en la vuelta** | 2–4 | 2 / 2 | La vuelta 1 siempre suena tal cual; desde esta vuelta entran los adornos. |
| `color` (fusiona `color` y `novenas`) | **Color de los acordes** | tríadas / séptimas / novenas | séptimas / novenas | Más notas por acorde: de acordes de tres notas a acordes con séptima, o con séptima y novena. Las novenas solo entran si la nota está en la escala. |
| `remate` (antes `vueltaAlInicio`) | **Remate final** | no / simple / rico | simple / rico | En la última vuelta, uno o dos acordes al final que empujan de vuelta al primero. «Simple» = un acorde de empuje; «rico» = dos, más jazz (en Do: `Fm7–B♭9` antes de `C`). |
| `sus4` | **Suspensión** | sí / no | sí / no | El acorde de empuje se queda «colgado» medio compás antes de resolver (en Do: `G7sus4 → G7`). Tensión que se suelta. |
| `paso` (fusiona `dominanteSecundaria` y `sustitutoTritonal`) | **Acorde de paso** | no / clásico / cromático | clásico / cromático | Antes de un acorde, otro que lo anuncia. «Clásico»: su dominante (en Do, `A7` antes de `Dm`). «Cromático»: llega desde medio tono arriba (`E♭7` antes de `Dm`), más jazz. |
| `ivPrestado` | **Menor prestado** | sí / no | no / sí | Después del IV, el mismo acorde en menor: una sombra agridulce (en Do: `F → Fm6`). Es el giro de *Creep*. |
| `bVI` | **Rodeo antes de volver** | sí / no | no / sí | Antes de volver al primer acorde, un acorde mayor de fuera de la escala que suena a película (en Do: `A♭maj7`). Solo en modos mayores. |
| `remateFiel` (antes `tritonoSoloSiCromatica`) | **Remate fiel a la escala** | sí / no | no / no | Si la progresión no usa acordes de fuera de la escala, el remate tampoco los usa. |
| `preferirAcordeNuevo` (nueva) | **Que se note el adorno** | sí / no | no / no | Si se puede, el adorno añade un acorde nuevo en vez de solo cambiar uno que ya está. |

Notas de nombre:
- «Suspensión» y «Acorde de paso» son términos de músico de banda, no de conservatorio. [I]
- Evito «dominante», «tritono» y «préstamo modal» en la etiqueta y los dejo en la ayuda larga (o en «Qué cambió»), según la regla de oro de Music Lab: nunca un término desnudo. [C, instrucciones]
- `ivPrestado` y el IV→iv son el recurso ① mejor validado del repo (*Algo Contigo*, *Creep*, *Telefonía*; `progression-axis.md` §6d y `progression-i-vi-iv-v.md` §(b)). Por eso el ejemplo de la ayuda es *Creep*. [C, repo]

### Equivalencias con la v3 (para la prueba de oro)

| | `presupuesto` | `desdeVuelta` | `color` | `remate` | `sus4` | `paso` | `ivPrestado` | `bVI` | `remateFiel` | `preferirAcordeNuevo` |
|---|---|---|---|---|---|---|---|---|---|---|
| N2 | 1 | 2 | séptimas | simple | sí | clásico | no | no | no | no |
| N3 | 2 | 2 | novenas | rico | no | cromático | sí | sí | no | no |

Cómo se tienen que leer en el código para que la salida sea idéntica [C, código]:
- `remate = simple` → `tonicApproach` en su rama de N2 (`V7`; Mixolidio con `♭VII` tríada). `rico` → la rama de N3 (backdoor / `iiø7–♭II7`; Mixolidio `♭VIImaj7`). Los modos Frigio, Dórico, Lidio y Locrio dan lo mismo en los dos.
- Remate hacia un acorde que no es la tónica: con `paso = cromático`, sustituto tritonal (como N3); con `clásico` o `no`, `V7` del destino (como N2).
- `paso` decide la casilla de la regla 4: `clásico` → `V7 de`; `cromático` → `♭II7 de`; `no` → nada.
- Orden fijo: remate → sus4 → menor prestado → acorde de paso → rodeo ♭VI. Con `preferirAcordeNuevo = sí`, el sus4 pasa detrás del acorde de paso (es la única perilla que toca el orden).
- La guarda «≤2 acordes distintos → presupuesto 1» queda fija (como el tope de abajo).

## 2. Límites y combinaciones

### Rangos

- **Presupuesto: 0–3.** [I]
  - Tope automático: `min(presupuesto, n − 1)` con n acordes en el bucle, y 1 si hay ≤2 acordes distintos (la guarda de la v3). Con 4 acordes, 3 adornos ya parten tres de cada cuatro: se duplica el ritmo armónico y la progresión pasa a ser otra (lo mismo que dice la ficha de la Axis de su nivel 3: «ya es otra progresión»). [C, repo] + [I]
  - El 4 no: en un bucle de 4 acordes sería partirlos todos, y el código ya no tendría sitio (cada acorde acepta un solo adorno, `used`). [C, código]
  - El 0 sí: sirve para «solo color» y como paso previo a «menos loco».
- **Empieza en la vuelta: 2–4.** La vuelta 1 tal cual es una regla común de la v3 y de la revisión anterior; no es perilla. Si `desdeVuelta` > vueltas de la carta, no pasa nada: avisar. [C, README]

### Prohibir (la interfaz o el validador no lo dejan)

- **Novenas sin séptimas**: queda resuelto al fusionar en `color`. [C, código]
- **Clásico y cromático a la vez**: resuelto al fusionar en `paso`.
- **`presupuesto` > tope**: se recorta en silencio y se dice en «Qué cambió».
- **`desdeVuelta` = 1**.

### Avisar (se deja, con una línea)

- **N2 más cargado que N3** (presupuesto, color o número de adornos activos): «El nivel 2 hace más que el 3». La escalera tiene que subir. [I]
- **Presupuesto > 0 y todos los adornos apagados**, o al revés: «No va a cambiar nada».
- **Novenas en N2**: no se prohíbe, pero se avisa de que el 2 y el 3 se van a parecer en color. Las novenas sí empastan con el sinte del taller (prueba de oído, G3). [C, prueba]
- **Menor prestado y rodeo ♭VI a la vez con presupuesto 2**: los dos son préstamos del menor paralelo; juntos oscurecen mucho una progresión mayor. Válido, pero es la combinación «más oscura» del sistema. [I]
- **Perilla sin efecto en el modo de la carta** (atenuarla, no avisar): `bVI` fuera de Jónico y Mixolidio; `ivPrestado` en Dórico, Frigio y Locrio; `remate` en Locrio. [C, código]
- **`sus4` con `paso = cromático`**: no choca (actúan en acordes distintos y `used` lo impide en el mismo), así que no hace falta prohibirlo. [C, código]

## 3. ¿Falta o sobra alguna?

**Falta:**
- **`preferirAcordeNuevo`**, por la nota 1 de la prueba: en la Cincuentas y la andaluza el N2 da `V7sus4 → V7`, que se oye poco. Con la perilla, el N2 elige antes la dominante secundaria. Alternativa que no toca el orden: activar `ivPrestado` en el N2, que es justo lo que proponen las fichas de Music Lab para el nivel 2 de la Axis y de la Cincuentas (IV→iv en medio compás). [C, repo] + [I]
- **`remate` de tres valores** (ver §1): sin él, «más jazz en el 2» no puede dar el backdoor.
- **Más adelante, no ahora:** un **perfil de estilo** (decisión B pendiente: blues / riff / pop / jazz) que solo fije valores de estas perillas. «Más rock» no se puede hacer bien sin acordes de quinta, que el alfabeto no tiene (pedido 1 a MidiGenPlay en la revisión anterior). [C]

**Sobra:** ninguna; dos se fusionan (§1).

**`tritonoSoloSiCromatica`:**
- La idea responde bien a la nota de la Royal road: el F♯7 final (`♭V7` en Do) es la única raíz de fuera de la escala en una progresión que es toda diatónica (`IVmaj7 – V7 – iiim7 – vim7`), y cae en el sitio que más se oye, la vuelta al inicio. [C, código] + [I]
- Mejor formulada así (`remateFiel`): «si ningún acorde escrito tiene la raíz fuera de la escala, el remate hacia un acorde que no es la tónica usa el `V7` del destino en vez del sustituto tritonal». En la Royal road, `C7 → Fmaj7` (V7 de IV): la sensible se queda en el acorde y la raíz C está en la escala. [I]
- No la extendería al acorde de paso del medio de la vuelta: en el Frigio y la andaluza, los dos sustitutos tritonales del N3 salieron ✓. [C, prueba]
- Valor por defecto: **no**, para que la prueba de oro salga idéntica; propuesta para después: **sí** en N3, con otra escucha de la Royal road. Un solo oyente, que conocía las reglas, no basta para cambiar el valor por defecto. [C, prueba]

## 4. Mini-diccionario para el prompt de `armonia.ajustar`

Regla general del prompt: si la frase no nombra nivel, se aplica al que está sonando; movimientos de una unidad por frase; nunca se tocan los acordes escritos ni el modo.

| Frase del usuario | Qué perillas mover |
|---|---|
| «menos loco», «más tranquilo» | `presupuesto` −1; `paso` cromático → clásico; `bVI` no; `remateFiel` sí |
| «más loco», «arriésgate» | `presupuesto` +1 (máx. 3); `paso` → cromático; `remate` → rico; `bVI` sí; `remateFiel` no |
| «más acordes de paso», «más movimiento» | `paso` ≠ no; `presupuesto` +1; `preferirAcordeNuevo` sí |
| «que se note más» (sobre todo en el 2) | `preferirAcordeNuevo` sí; o `ivPrestado` sí. No subir `color` |
| «más jazz» | `color` → novenas (N3) o séptimas (N2); `paso` → cromático; `remate` → rico |
| «menos jazz», «más crudo», «más rock» | `color` → tríadas; `paso` → clásico o no; `remate` → simple. Avisar: los acordes de quinta no existen todavía |
| «más oscuro», «más triste» | `ivPrestado` sí; `bVI` sí (los dos vienen del menor). Avisar: para oscurecer de verdad hay que cambiar el modo, y eso no es una perilla |
| «más luminoso», «más alegre» | `ivPrestado` no; `bVI` no; `paso` → clásico (las dominantes secundarias son acordes mayores) [I] |
| «más fiel», «que siga sonando a la original» | `presupuesto` −1; `remateFiel` sí; `bVI` no; `paso` cromático → clásico |
| «no me toques el final» | `remate` → no |
| «más tensión antes de volver» | `remate` → rico; `sus4` sí |
| «que cambie antes» / «que tarde más en cambiar» | `desdeVuelta` −1 (mín. 2) / +1. Si ya está en 2: explicar que la vuelta 1 va siempre tal cual |
| «el 2 y el 3 suenan igual» | Separar: bajar el N2 (`color` séptimas, `presupuesto` 1) o subir el N3 (`presupuesto` +1, `paso` cromático) |

Salida esperada: JSON con las perillas nuevas de los dos niveles y una frase de por qué, en lenguaje llano.

## 5. Crear progresiones desde una frase (`armonia.generar`)

### Reglas para el prompt

1. **Modo según el carácter**, sin promesas emocionales fuertes (solo el orden de alegría de los modos tiene respaldo experimental; Temperley y Tan 2013, revisión anterior §6) [C]:
   - alegre, pop → Jónico; triste, épico, furioso → Eólico; oscuro, metal, flamenco → Frigio;
   - «menor luminoso», funk, soul → Dórico; rock alegre «sin cierre» → Mixolidio; flotante, de ensueño → Lidio;
   - Locrio solo si se pide de forma explícita.
2. **Grados relativos al modo elegido.** En Eólico, `III`, `VI` y `VII` se escriben **sin** bemol; una alteración baja o sube el grado *del modo*. Si hace falta un acorde de fuera, escribir su alteración desde ese modo. [C, código del lector]
3. **Que el modo se oiga**: incluir el acorde característico del modo (Dórico `IV` mayor, Frigio `II` mayor, Lidio `II` mayor, Mixolidio `VII` mayor). Si no, suena a mayor o menor y el modo no se nota. [I] (revisión anterior §6)
4. **Longitud: 4 acordes** por defecto (2–8). Bucles de 2 para vamps modales y riffs; 8 o 12 solo para formas (blues de 12 compases, una A de jazz). [I]
5. **Duración en compases**: sin paréntesis = 1 compás. Usar `(0.5)` para ritmo armónico rápido (rhythm changes, Teen Spirit, cadencias de paso), `(2)` para acordes que respiran (baladas, drones). La suma del bucle debería ser una potencia de 2 (4 u 8 compases) salvo en un blues de 12. [I]
6. **Solo el alfabeto del taller**: tríada mayor (mayúsculas, sin sufijo), menor (minúsculas), `dim`, `aug`, `7`, `maj7`, `m7`, `ø7`, `dim7`, `sus2`, `sus4`, `6`, `m6`, `7sus4`, `9`, `maj9`, `m9`. Nunca `M7` (se lee m7), ni `-7` (el guion separa acordes), ni `5`, `add9`, `11`, `13`, `7b9`, `6/9` ni acordes con bajo (`/`). Si el estilo los pide, la aproximación más cercana: quinta → tríada o `sus2`; `13` → `9`; `7b9` → `7`; `6/9` → `6`. [C, código]
7. **Compás**: 4/4 por defecto; 3/4 para vals, cueca o tonada (el taller no tiene 6/8). Devolver también modo, compás y una línea de por qué, sin citar canciones que no se puedan comprobar. [C, código] + [I]

### 10 frases de prueba

| # | Frase | Qué mide | Respuesta razonable (para comparar) [I] |
|---|---|---|---|
| 1 | «Algo alegre para empezar un concierto» | Caso fácil | Jónico 4/4, `I – V – vi – IV` o parecida |
| 2 | «Triste pero con esperanza» | Mezcla de emociones | Eólico que termine en `III` o `VII`, o Jónico con `iv` |
| 3 | «Como Radiohead en *Creep*» | Referencia concreta | Jónico `I – III – IV – iv` (2 compases cada uno o 1) |
| 4 | «Una cueca» | Folclore chileno y compás | Jónico 3/4 con tónica y dominante (`I – V7`, `IV` de paso). La armonía de la cueca se apoya en la tónica y el quinto grado con séptima (Zamorano 2024) [C con reserva]; la sesquiáltera (6/8 contra 3/4) no se puede representar [I] |
| 5 | «Metal oscuro y pesado» | Frigio y límite del alfabeto (sin quintas) | Frigio `i – II` o `i – II – i – VII`, con aviso de que no hay acordes de quinta |
| 6 | «Jazz de bar a las 3 de la mañana» | Cuatriadas y ritmo armónico | Jónico `iim7 – V7 – Imaj7 – VI7` (o con `(0.5)`) |
| 7 | «Funk que no se acaba nunca» | Vamp de 2 acordes, Dórico | Dórico `im7 – IV7` |
| 8 | «Como un sueño en el que flotas» | Lidio | Lidio `Imaj7 – II` |
| 9 | «Música de flamenco» | Cadencia andaluza | Frigio `i – VII – VI – V` desde el modo, o Eólico `i – VII – VI – V` (con `V` mayor). Vigilar las alteraciones |
| 10 | «Algo que suene a final de videojuego, épico, que no se repita» | Difícil: «no se repita» choca con un bucle; ♭VI–♭VII–I | Jónico `I – ♭VI – ♭VII – I` o Eólico `VI – VII – i`; el modelo debería decir que el taller siempre hace bucle |

Para puntuar cada frase: (a) ¿parsea sin errores?; (b) ¿el modo encaja con la frase?; (c) ¿suena a lo pedido? (oído de Claudio); (d) ¿usa sufijos inexistentes o alteraciones mal puestas en Eólico o Frigio? El fallo esperable es la regla 2 (escribir `♭VI` en Eólico, que suena a doble bemol). [I]

## Fuentes

- Código: `template.html` (`color`, `tonicApproach`, `arrange`, `parse`, `ALIAS`), `README.md` del taller.
- Repo: `revision-music-lab-2026-11-02.md`, `prueba-oido-v3.md`; Music Lab: `Análisis/Progresiones/progression-axis.md`, `progression-i-vi-iv-v.md`, `progression-blues-menor.md`, `recursos-armonicos.md`.
- Cueca: [reseña del método de floreo, *Revista Musical Chilena*](https://actascoloquiogiannini.uchile.cl/index.php/RMCH/article/viewFile/12687/12976) y [Berklee PULSE, *History of the Cueca*](https://pulse.berklee.edu/content/public/pub_unit_assets/lessons/cueca/History%20of%20the%20Cueca.pdf). La búsqueda no dio una fuente clara sobre la sesquiáltera: queda [I].
- Temperley y Tan (2013) y Cheung et al. (2019): citados en la revisión anterior, no se volvieron a comprobar aquí.

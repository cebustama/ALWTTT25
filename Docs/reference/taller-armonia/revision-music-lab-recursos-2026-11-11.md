# Revisión de Music Lab · los niveles del taller frente a los recursos armónicos (2026-11-11)

Pedido de Claudio, tras el lote 1 de la Biblioteca y las siete promociones de `Music Lab/recursos-armonicos.md`. Preguntas: qué recursos usa el taller al crear los niveles 2 y 3, cuáles de Music Lab no usa, cómo aprovecharlos, cuál es el proceso para mantener el taller al día con Music Lab y cómo llega todo a MidiGenPlay.

Fuente del taller: `template.html`, sección `levels` (v3 con perillas, `PERILLAS_V3`). Fuente de Music Lab: `recursos-armonicos.md` a 2026-11-11.

## 1. Qué hace hoy el taller en cada nivel

Reglas fijas: ningún acorde escrito desaparece (lo añadido ocupa solo la segunda mitad de un acorde); la vuelta 1 suena tal cual; nada de modular dentro de la carta. Cada cambio gasta una unidad del **presupuesto** (nivel 2: 1 por vuelta; nivel 3: 2).

| # | Operación (perilla) | Qué añade | Nivel 2 | Nivel 3 |
|---|---|---|---|---|
| 1 | **Color** (`color`) | tríadas → cuatriadas de la escala; un acorde mayor que baja una quinta pasa a `7`; novenas solo si la 9.ª está en la escala | séptimas | novenas |
| 2 | **Remate** (`remate`), última vuelta, si el bucle no cierra ya por cadencia | antes de volver al primer acorde: `V7` (simple); rico en Jónico, `iv7 – ♭VII9` (backdoor); en Eólico, `iiø7 – ♭II7`; en los modos, su acorde propio (Frigio `♭IImaj7`, Dórico `IV7`, Lidio `II7`, Mixolidio `♭VII`) | simple | rico |
| 3 | **sus4** (`sus4`) | la dominante funcional empieza en `7sus4` y resuelve | sí | no |
| 4 | **Menor prestado** (`ivPrestado`) | tras el IV mayor, `iv6`; en Eólico, `IV7` dórico tras el iv | no | sí |
| 5 | **Acorde de paso** (`paso`) | hacia un acorde diatónico que no es la tónica: su `V7` (clásico) o su sustituto de tritono `♭II7` (cromático) | clásico | cromático |
| 6 | **♭VI** (`bVI`), solo Jónico y Mixolidio | `♭VImaj7` antes de volver a I | no | sí |
| — | `remateFiel`, `preferirAcordeNuevo` | ajustes de orden y de fidelidad a la escala (apagados en v3) | — | — |

## 2. Cruce con la lista de Music Lab

Leyenda: ✅ el taller lo usa · ◐ lo usa a medias · ✗ no lo usa · 🔒 no puede usarlo (falta algo en el alfabeto de MidiGenPlay, o choca con una regla del taller).

### ① Validados (14; 13 armónicos)

| Recurso de Music Lab | En el taller | Dónde / por qué no |
|---|---|---|
| Dominante secundaria | ✅ | paso clásico (`V7` del acorde siguiente) y la regla de color «mayor que baja una quinta → 7» |
| Sustitución de tritono | ✅ | paso cromático (`♭II7` del acorde siguiente); remate rico en Eólico (`♭II7 → i`) |
| Backdoor ii–V | ✅ | remate rico en Jónico (`iv7 – ♭VII9 → I`) |
| ii–V menor | ✅ | remate rico en Eólico (`iiø7`, aunque luego va al `♭II7` en vez de al `V7`) |
| Intercambio modal IV→iv | ✅ | menor prestado (`iv6` tras el IV) |
| Dominante mayorizada en tonalidad menor | ✅ | remate simple en Eólico: el `V7` mayor (con la sensible) |
| ii–V mayor | ◐ | solo pone la dominante (`V7`); nunca añade el `ii` delante. Es el recurso con más casos del lab (15) |
| Deslizamiento cromático paralelo | ◐ | aparece de rebote cuando el paso cromático cae sobre otra dominante (`A♭7 → G7`), pero no es una regla propia |
| ♭VI7 de color | ✗ | el taller tiene `♭VImaj7` (mayor séptima, en Jónico), no el `♭VI7` dominante en menor de *SIBLY*, *Cantaloupe* y *Summertime* |
| Disminuido de paso | ✗ | ninguna regla inserta un `°7` |
| Cadena de dominantes por quintas | ✗ | el color solo dominantiza acordes **mayores** que bajan una quinta; los menores (`iii`, `vi`, `ii`) se quedan menores |
| Pivote I → ii | 🔒 | es una modulación, y la regla del taller es no modular dentro de la carta |
| Línea de bajo cromática descendente | 🔒 | necesita inversiones (`C7/B♭`); el alfabeto de MidiGenPlay no las tiene |

### ② Observados (14) y ③ De teoría (5), los que importan aquí

| Recurso | En el taller | Nota |
|---|---|---|
| ♭II napolitana (②) | ✅ | remate del Frigio (`♭IImaj7`) |
| i7 / iv7 (②) | ✅ | el color en Eólico produce `im7` y `ivm7` |
| Intercambio ♭VI / ♭VII (③) | ✅ | `♭VImaj7` (nivel 3) y el `♭VII` del Mixolidio; el `♭VII9` de la backdoor |
| Cadencia auténtica V→I (③) | ✅ | remate simple |
| Giro ♭VI7–V7–i (②) | ✗ | sería el remate natural de un blues menor |
| Cliché de línea menor (②) | 🔒 | necesita `m(maj7)` |
| Contraste ♭6/♮6 (②) | ◐ | el `IV7` dórico prestado en Eólico es justo ese ♮6, pero no se pensó así |
| Dominante secundaria de resolución evadida (②), mediante cromática (②), secuencias y modulaciones (②), cierre en el relativo mayor (②), cadencia rota y plagal (③) | ✗ | no son adornos de un acorde: cambian la dirección o la forma. Algunos chocan con «no modular» |

**Resumen:** de los 13 recursos armónicos validados, el taller usa 6, usa 2 a medias, no usa 3 y tiene 2 bloqueados. Lo que le falta no es lo exótico: es lo más frecuente en el repertorio que hemos analizado (el ii–V completo, la cadena de dominantes, el disminuido de paso).

## 3. Cómo aprovechar mejor lo que ya está analizado

Propuestas ordenadas por valor y coste. Todas caben en las reglas del taller (ningún acorde desaparece, lo añadido va en la segunda mitad). Cada una sería **una perilla nueva**, apagada por defecto, para que la v3 siga sonando igual hasta que se decida encenderla.

| # | Propuesta | Recurso | Cómo, en el taller | Ejemplo de la Biblioteca |
|---|---|---|---|---|
| P1 | **ii–V completo** en el acorde de paso | ii–V mayor ① | `paso: "iiV"`: si el acorde dura al menos un compás, su segunda mitad se reparte entre el `ii` y el `V7` del acorde siguiente (gasta un cambio) | *Billie's Bounce* c. 8 (`Am7 D7 → Gm7`) |
| P2 | **Dominantizar en cadena** | Cadena de dominantes ① | `color: "dominantes"`: todo acorde (también menor) que baja una quinta hacia otro pasa a `7`; no añade acordes, solo cambia la calidad (como la regla actual, ampliada) | puente de *Oleo* (`III7–VI7–II7–V7`), *All of Me* A |
| P3 | **Disminuido de paso** | Disminuido de paso ① | `paso: "disminuido"`: si el acorde siguiente está un tono por encima, la segunda mitad pasa a `°7` un semitono por debajo del destino (`I – ♯I°7 – ii`) | *Georgia* (`C – C♯°7 – Dm`), *Billie's Bounce* c. 6, *Wave* c. 2 |
| P4 | **♭VI7 en menor** | ♭VI7 de color ①, giro ♭VI7–V7–i ② | `bVI: "7"` en Eólico y Dórico: `♭VI7` en la segunda mitad del acorde que va a `V` o a `i` | *Cantaloupe Island*, *Summertime* c. 6, *SIBLY* |
| P5 | **Remate con deslizamiento** | Deslizamiento cromático paralelo ① | `remate: "deslizado"`: el acorde de llegada precedido de la misma calidad medio tono arriba (`A♭7 → G7 → C`) | *Thrill*, *Footprints* (`F7 → E7`) |
| P6 | **Perfiles de estilo** (la decisión D8-B pendiente) | todos | un juego de perillas por estilo, sacado de la Biblioteca: «jazz» (P1+P2+P3, ii–V por todas partes), «bossa» (paso cromático, ♭VI, iv prestado), «blues menor» (P4, remate ♭VI7–V7–i) | las 15 fichas dicen qué recursos usa cada estilo |
| — | Pivote, bajo cromático, cliché | 🔒 | esperan a MidiGenPlay (modulación por secciones, inversiones, `m(maj7)`) | — |

**La propuesta que más rinde no es una regla: es usar la Biblioteca como prueba.** Una **prueba de reducción**: se toma una sección real (*Oleo* A, *Misty* A…), se le quitan los adornos hasta dejar el esqueleto diatónico (`I – vi – ii – V`), se pasa por los niveles 2 y 3, y se compara con lo que hace la canción de verdad. Mide algo que la prueba de oro no mide: si los adornos del taller se parecen a los que usan los músicos. Las fichas ya tienen el esqueleto y la versión real.

## 4. Proceso estándar: de Music Lab al taller

Cada vez que `recursos-armonicos.md` promueve o añade un recurso (la nota del lote lo dice):

1. **Mapa recurso → regla.** Se añade o actualiza su fila en la tabla del §2 de este documento (o de la revisión que la suceda): ✅ / ◐ / ✗ / 🔒, con el motivo.
2. **Ficha de regla (Music Lab).** Si es ✗ y vale la pena, Music Lab escribe la regla en una hoja `revision-music-lab-*.md`: dónde puede ir (qué acorde, qué mitad), en qué modos, qué gasta del presupuesto, en qué orden respecto a las demás, y 2–3 casos de la Biblioteca con su compás. Las dudas musicales se consultan a Music Lab (regla de Claudio).
3. **Perilla nueva en el taller, apagada.** Se implementa detrás de una perilla nueva con el valor de la v3 por defecto. La prueba de oro (`node scripts/oro.js comparar oro-v3.json`) tiene que seguir dando 792 idénticos: así se demuestra que nada de lo anterior cambió.
4. **Casos nuevos.** `node scripts/oro.js perillas` enseña la regla encendida sobre el catálogo; y la prueba de reducción, sobre las secciones de la Biblioteca donde aparece el recurso.
5. **Prueba de oído** (como la de la v3: hoja con pares A/B, Claudio escucha) antes de encenderla por defecto en un nivel o en un perfil.
6. **Nueva versión de oro.** Si se enciende por defecto, se escribe `oro-v4.json` (`oro.js escribir`), se anota el cambio en el README y se publica el taller.
7. **Paso a MidiGenPlay** (§5): la nueva versión de oro es lo que viaja.

## 5. De la página a MidiGenPlay (y de ahí a ALWTTT y otros clientes)

**Lo que MidiGenPlay ya tiene** (`Runtime/CoreScripts/Composition/`):
- `ChordProgressionRequality`: una transformación de datos determinista que corre sobre el clon de la progresión, en el sitio donde ya la leen el acompañamiento, el bajo y la melodía. Tiene **recalidad diatónica** (≈ la perilla *color*, en séptimas), una **tabla de color** (sextas, novenas, sustitución del `ii°` por el `iv`) y **dominantes secundarias por evento** (SECDOM-1, ≈ el paso clásico, pero marcadas a mano en el asset).
- `ModulationPlanner`: la dominante del tono nuevo y los acordes pivote ordenados (≈ el pivote I → ii y la modulación por tono, que el taller no puede hacer).

**Lo que falta:** los niveles en sí (la decisión D6 del juego: rejugar la carta sube el nivel). Están previstos para después de la demo.

**Propuesta, en tres piezas:**
1. **`ProgressionLevelArranger` (C#)**, un port 1:1 de `arrange()`: función pura de (progresión, modo, nivel, vuelta, perillas) → eventos. Corre en el mismo sitio que `ChordProgressionRequality` (después de ella), para que el bajo y la melodía oigan los mismos acordes. Sin azar, como la página.
2. **`HarmonicLevelProfileSO`**: las perillas como ScriptableObject, uno por nivel y, con D8-B, uno por perfil de estilo. ALWTTT elige el perfil por carta o por público; otro cliente (mundo-islas, por ejemplo) elige el suyo.
3. **Prueba de paridad con el taller.** Un test EditMode de MidiGenPlay lee el `oro-vN.json` del taller (las 33 progresiones × niveles × vueltas × compás) y exige la **misma salida** del arranger en C#. Es el puente del proceso: cuando el taller sube de versión de oro, el test falla en MidiGenPlay hasta que el port se pone al día. La página es la referencia que se escucha y se prueba rápido; el motor la sigue.

**Pedidos al alfabeto de MidiGenPlay** (desbloquean los 🔒 y vienen de la Biblioteca, `FORMATO.md` §9): acorde menor con 7.ª mayor; bajo distinto de la fundamental (inversiones y acordes con barra); 6/4; tres acordes por compás.

**Cuándo:** el port y los pedidos son sesiones de MidiGenPlay, que van después de la demo de ALWTTT (regla de Claudio: una sesión de MidiGenPlay solo si esa semana se cumplieron las de ALWTTT). Lo que sí cabe antes, en la línea armónica paralela: los pasos 1–5 del §4 en el taller (perillas P1–P5 apagadas, prueba de reducción y prueba de oído), que no tocan el juego.

## 6. Nota aparte: canciones largas en el taller

Claudio, 2026-11-11: las canciones enteras tardan en empezar o suenan con ruido. Causa probable: `play()` programa todas las notas de golpe (miles de osciladores en 32 compases × 2 vueltas). Arreglo conocido y pequeño: programar por ventanas (un planificador que mira un compás o dos por delante). No es prioritario: el taller es para progresiones.

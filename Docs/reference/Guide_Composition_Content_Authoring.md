# Guía de autoría de contenido musical — ALWTTT

**Estado:** referencia, **no normativa**. No crea ni cambia contratos; cuando contradiga un
SSoT, manda el SSoT. Fuentes primarias citadas en cada sección.
**Fecha:** 2026-10-02.
**Para quién:** compositores que trabajan **solo desde el Editor de Unity** (ventanas e
Inspector), sin tocar código.
**Ubicación propuesta en el repo:** `Docs/reference/Guide_Composition_Content_Authoring.md`.

> ### ⚠ Antes de entregar esta guía: verificar en el Editor
> Estos puntos salen de la documentación, pero no se han comprobado contra el Editor.
> Cada uno está marcado en el texto como **⚠ VERIFICAR**. Un dev debe confirmarlos una vez
> y borrar la marca.
>
> 1. Ruta de menú de **Chord Progression Editor**, **Drum Pattern Editor** y **Melody
>    Pattern Editor** (son ventanas de MidiGenPlay).
> 2. Orden exacto de botones al guardar en el Chord Progression Editor (Import → Apply To
>    Target Asset → Save).
> 3. Ruta del menú *Create* para `ChordProgressionPaletteSO` y `DrumPatternPaletteSO` (la
>    guía evita depender de ella: recomienda duplicar).
> 4. Nombre del campo de peso en las entradas de paleta (`weight` o similar).
> 5. Rótulos exactos de las secciones del tab *Composition* del Dev Mode.

---

## 0. Mapa mental en un minuto

Todo el contenido musical del juego cuelga de una cadena:

```
Carta  →  Bundle (la "configuración de estilo" de la carta)  →  Patrón o Paleta  →  Patrones
```

| Pieza | Qué es | Ejemplo |
|---|---|---|
| **Progresión** (`ChordProgressionData`) | Una secuencia de acordes en grados (I, iv, V7…), con compás y duración | `vim7 – iim7 – V7 – Imaj7` |
| **Paleta de progresiones** (`ChordProgressionPaletteSO`) | Lista de progresiones **con pesos**; el juego elige una al azar (ponderado) | `Chord Palette - Core Minor` (6 progresiones) |
| **Patrón de batería** (`DrumPatternData`) | Una rejilla de golpes por instrumento | `FF_Rock8c` |
| **Paleta de batería** (`DrumPatternPaletteSO`) | Lista ponderada de patrones de batería | `DrumPatternPalette-FourOnTheFloor` |
| **Bundle** (`*CardConfigSO`) | Lo que la carta "trae" a la canción: qué progresión/paleta/patrón usar **y cómo se toca** (articulación) | `Backing Card Config - Core Minor` |
| **Carta** (`CardDefinition`) | Lo que el jugador juega. Apunta a **un** bundle | *Wormus Minor* |

**Regla clave: la articulación vive en el bundle, no en la progresión.** Una misma
progresión puede sonar en bloque en una carta y en arpegio en otra. No se puede elegir la
articulación acorde por acorde.

Un bundle por rol de pista:

| Rol | Tipo de bundle | Qué lleva |
|---|---|---|
| Backing (acompañamiento armónico) | `BackingCardConfigSO` | progresión **o** paleta de progresiones + articulación |
| Bassline (bajo) | `BasslineCardConfigSO` | **solo** articulación (el bajo sigue la progresión compartida) |
| Rhythm (batería) | `RhythmCardConfigSO` | patrón **o** paleta de batería |
| Melody | `MelodyCardConfigSO` | patrón de melodía **o** ruta procedural (paleta de frases + estilo) |
| Harmony | `HarmonyCardConfigSO` | **nada que autorar hoy** (el paquete no lo lee) |

*Fuentes: `MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` §5; `SSoT_Card_System.md`
§5.2.1; `SSoT_Card_Authoring_Contracts.md` §5.18.*

---

## 1. Reglas de oro (léelas antes de empezar)

1. **Todo contenido nuevo dura 8 compases** (estándar CR-10 / D-CSV-23). Una progresión de 4
   compases en una parte de 8 deja el bajo en silencio la mitad del tiempo: el bajo no
   repite para rellenar.
2. **El compás va en el nombre y no cambia.** Un asset en 3/4 solo sirve en partes en 3/4.
3. **No uses `arpeggioRate = Random`.** Hoy hace que la articulación se pierda en
   silencio (defecto F-ARTIC-RATE-RANDOM-1, ask `MGP-ARTIC-RATE-1` abierto). Usa
   `PerBeat`, `Eighth` o `Sixteenth`.
4. **`Bossa` y `BassUpperSplit` no son lo mismo.** `Bossa` es la bossa auténtica;
   `BassUpperSplit` suena a ska tranquilo. Confundirlas es el error típico.
5. **Override gana a paleta.** Si un bundle de Backing tiene `progressionOverride` **y**
   `progressionPalette`, suena siempre el override y la paleta se ignora. Rellena solo uno.
6. **Editar una paleta compartida cambia todas las cartas que la usan.** Añadir una
   progresión a `Core Minor` cambia lo que suena con *Wormus Minor* desde ese momento.
   Antes de editar, mira quién la usa (§13).
7. **Progresiones modales:** política `AsAuthored` y la lista de tonalidades con el modo
   real **en primera posición**. Escribe grados planos: en Frigio el II *ya es* ♭II; escribir
   `bII` lo baja dos veces.
8. **No toques `subdivisions` a mano** en una progresión. Reescala los eventos: acordes que
   cambian muy rápido seguidos de silencio.
9. **No renombres a mano los bundles** `*_Payload_<Rol>_StyleBundle`: el Card Editor
   regenera el nombre a partir de la carta.

---

## 2. Herramientas

| Ventana | Menú | Para qué |
|---|---|---|
| Chord Progression Editor | ⚠ VERIFICAR (menú MidiGenPlay) | Crear progresiones (pegando texto) |
| Drum Pattern Editor | ⚠ VERIFICAR (menú MidiGenPlay) | Crear patrones de batería (rejilla + texto) |
| Melody Pattern Editor | ⚠ VERIFICAR (menú MidiGenPlay) | Crear patrones de melodía |
| Chord Progression Catalogue Wizard | MidiGenPlay → Chord Progression Catalogue Wizard... | Buscar progresiones por compás, tonalidad, calidad |
| Card Editor | ALWTTT → Cards → Card Editor | Crear cartas y su bundle; asignar bundles |
| Deck Editor | ALWTTT → Cards → Deck Editor | Meter cartas en mazos |
| Composition Inventory | ALWTTT → Dev → Composition Inventory | **Verificar** qué existe, qué usa qué, y qué está roto (solo lectura) |
| Dev Mode (en juego) | `F12` en Play Mode | **Escuchar** sin crear assets |

El Composition Inventory y el Dev Mode requieren el símbolo de compilación `ALWTTT_DEV`.
Si no aparecen, pídeselo a un dev.

---

## 3. Receta A — Escuchar una progresión sin crear nada

**Úsala siempre primero.** Descartar una progresión aquí cuesta segundos; descartarla
después de crear assets cuesta limpieza.

*Fuente: `SSoT_Dev_Mode.md` §18.2, §18.3, §18.5, §18.14.*

1. Entra en **Play Mode**, empieza un gig y llega a la fase de composición.
2. Asegúrate de que la parte tiene un track de **Backing**: juega *Wormus Major* o
   *Wormus Minor*, o inyéctalo desde el Dev Mode (sección de debug-play de cartas, §18.10).
   Sin track de Backing no hay fila donde aplicar la progresión.
3. Pulsa `F12` → pestaña **Composition**.
4. Escribe un número en el campo de **seed** (cualquiera) para que todo sea reproducible.
5. Activa **infinite loop**: la parte se repite hasta que lo apagues.
6. Abre la sección **Roman → Backing override** (⚠ VERIFICAR rótulo):
   - pega **solo la línea Roman** (ver formato en §4.2), sin la ficha de setup;
   - ajusta **tonalidad**, **compases** (`0` = deducir) y **duración por defecto**;
   - el compás lo toma de la parte actual.
7. Pulsa **Re-render part now**. Suena en el siguiente loop.
8. *(Opcional)* Prueba articulaciones en la sección **Articulation override**: elige
   `chordExpression` y `arpeggioRate` → **Apply**. No modifica ningún asset: trabaja
   sobre una copia temporal del bundle.
9. Para confirmar qué acordes sonaron: botón **`chd: dump`** (los escribe en la consola).
10. **Clear** (o **Clear ALL overrides**) devuelve todo a como estaba.

**Si el texto falla:** algún acorde está fuera del alfabeto (§A.2). Falla entero; no se
aplica nada. Arregla el token y repite.

**Si aparece un aviso amarillo en la articulación:** combinaste una figura concreta con
`arpeggioRate = Random`. Ver regla de oro 3.

---

## 4. Receta B — Crear una progresión nueva (asset)

*Fuentes: skill `chord-progression-importer`; `SSoT_Card_Authoring_Contracts.md` §5.17;
`Design_Asset_Naming_v0_1.md` §3.*

### 4.1 Pasos

1. Escúchala antes (Receta A).
2. Prepara el **payload** (ficha de setup + línea Roman, §4.2). Puedes escribirlo a mano o
   pedirlo con la skill `chord-progression-importer`, dándole los acordes.
3. Copia el payload **completo** al portapapeles.
4. Abre el **Chord Progression Editor** → **Create New Progression** (limpia el editor).
5. Pulsa **Import** (desde portapapeles).
6. Revisa el preview. **Debe haber cero warnings.** Un warning significa que el importador
   entendió otra cosa; no sigas.
7. Guarda (⚠ VERIFICAR orden: *Apply To Target Asset* / diálogo *Save*):
   - **Carpeta:** `Assets/Resources/ScriptableObjects/Patterns/Chords/`. Es la carpeta que
     el juego escanea; fuera de ella el asset no aparece en los selectores del Dev Mode
     (flag `OFF-ROOT`).
   - **Nombre:** `Prog_<Compás>_<N>m_<Slug>[_<Modo>]`. Ejemplos:
     `Prog_4-4_8m_PopMenor_Aeolian`, `Prog_4-4_8m_TwoFiveOne`.
8. Rellena `displayName` en el Inspector con el nombre legible (p. ej.
   `i – VI – III – VII (pop menor)`).
9. **Una progresión suelta no suena en el juego.** Hay que meterla en una paleta (Receta C
   o D) o en un bundle (Receta E).

### 4.2 Formato del payload

````markdown
**Setup (Roman mode):**

- Time signature: FourFour
- Measures (total): 8
- Default duration (measures): 2
- Reference tonality: Aeolian
- Allowed tonalities: Aeolian

**Progression (paste into the Roman string field):**

```
im – VImaj – IIImaj – VIImaj
```
````

**Reglas del texto:**

- Separador: ` – ` (espacio, **guion largo**, espacio). Un guion corto `-` es ambiguo.
- Cada acorde: alteración opcional (`b` / `#`) + grado `I`…`VII` + sufijo de calidad (§A.2).
- **Pon siempre el sufijo**, también en tríadas mayores: `Imaj`, no `I`.
- `7` solo = séptima de **dominante**. Menor séptima = `m7`. Mayor séptima = `maj7`.
  `vi7` suena como dominante; lo correcto es `vim7`.
- Duración en compases entre paréntesis si difiere de la duración por defecto: `Imaj7 (2)`,
  `V7 (0.5)`. Decimal con punto.
- Las duraciones deben **sumar exactamente** `Measures`.
- `S (1)` = un compás de silencio. Evítalo salvo que lo busques: deja hueco (flag
  `INNER-GAP`).
- No se guarda tonalidad absoluta (Do, La…). El asset es transportable: guarda grados y modo.

**Líneas opcionales de la ficha** (omítelas si no estás seguro):

| Línea | Valores | Cuándo |
|---|---|---|
| `Cadence` | `None`, `Authentic`, `Plagal`, `Half`, `Modal` | Clasificación; nunca `Authentic` junto con `DiatonicToPart` |
| `Allowed tonalities` | lista de modos separados por coma | Debe incluir la tonalidad de referencia, **en primera posición** si la carta adopta el modo |
| `Quality render policy` | `AsAuthored`, `DiatonicToPart`, `DiatonicToPartFunctional` | `AsAuthored` = suena como está escrita (vamps modales). `DiatonicToPart` = se adapta a la tonalidad de la parte |
| `Use color table` | `true` / `false` | Solo tiene efecto con `DiatonicToPart*` |

### 4.3 Ejemplos

**Pop menor, 8 compases, 2 por acorde:**

```
im – VImaj – IIImaj – VIImaj
```
Ficha: `FourFour` · `Measures 8` · `Default duration 2` · `Aeolian` · `Allowed: Aeolian`.

**ii–V–I de jazz, 8 compases, duraciones mixtas:**

```
iim7 – V7 – Imaj7 (2) – iim7 – V7 – Imaj7 (2)
```
Ficha: `FourFour` · `Measures 8` · `Default duration 1` · `Ionian` · `Cadence: Authentic` ·
`Quality render policy: AsAuthored`. Suma: 1+1+2+1+1+2 = 8. ✓

**Vamp dórico, 8 compases:**

```
im7 (2) – IVmaj (2) – im7 (2) – IVmaj (2)
```
Ficha: `FourFour` · `8` · `2` · `Dorian` · `Allowed: Dorian` · `AsAuthored`.
El IV mayor *es* el color dórico; no escribas `#IV` ni nada parecido.

---

## 5. Receta C — Añadir una progresión a una paleta existente

1. Abre el **Composition Inventory** → vista **Chord Progressions** → busca la paleta →
   **Ping**. Se selecciona en el Project.
2. **Antes de editar:** vista **Progression Content** → filtra por el nombre de la paleta.
   La línea `cards:` dice qué cartas cambiarán. Si alguna está en un mazo inicial, avisa al
   equipo: es un cambio de juego.
3. En el Inspector de la paleta, añade una entrada a `entries`:
   - `progression`: arrastra tu progresión;
   - peso (⚠ VERIFICAR nombre del campo): **mayor que 0**. Un peso de 0 equivale a no estar.
4. Comprueba el **compás**: una progresión en 3/4 dentro de una paleta usada en partes de
   4/4 no se elegirá en esas partes.
5. Guarda (`Ctrl+S`).

---

## 6. Receta D — Crear una paleta de progresiones nueva

**Lo más sencillo es duplicar una existente**, para heredar su configuración.

1. Localiza una paleta parecida (Inventory → Ping).
2. Selecciónala en el Project → `Ctrl+D` (duplicar). Queda en la misma carpeta, junto a las
   demás paletas.
3. Renómbrala: `Palette_Chords_<Ámbito>`, p. ej. `Palette_Chords_PopMenor`.
4. En el Inspector, **vacía `entries`** y añade tus progresiones con su peso (Receta C).
5. Revisa `preferExactTsMatches`. Activado = el juego prefiere progresiones con el compás
   exacto de la parte antes que adaptarlas.
6. Guarda.
7. Una paleta suelta tampoco suena: hay que asignarla a un bundle de Backing (Receta E).

*Alternativa:* menú *Create* del Project (⚠ VERIFICAR ruta). Una paleta creada desde cero
arranca con valores por defecto que hay que revisar uno por uno; por eso se recomienda
duplicar.

**Pesos:** son relativos. Pesos `1, 1, 2` = la tercera sale el doble que cada una de las
otras. Con 6 o más entradas la repetición entre canciones es baja (decisión D-S5g-7=C).

---

## 7. Receta E — Bundle de Backing: progresión + articulación

Este es el paso que combina **qué acordes** con **cómo se tocan**.

*Fuentes: `SSoT_Card_Authoring_Contracts.md` §5.13, §5.17, §5.18; boundary §8.7;
`MGP-20260810_CURRENT_STATE.md` (arco CA).*

### 7.1 Dónde está el bundle

- **Carta nueva:** Card Editor → elige el músico → botón de rol **Backing**. Crea la carta
  **y** su bundle (`<carta>_Payload_Backing_StyleBundle`) ya conectados. Es el camino más
  corto y garantiza que la carta crea una pista.
- **Carta existente:** Card Editor → selecciona la carta → en la sección de composición,
  el campo `styleBundle` → clic para seleccionarlo.
- **⚠ Un bundle puede estar compartido por varias cartas.** Inventory → vista
  **Cards → Bundles**: si el bundle aparece en dos filas, editarlo cambia las dos.

### 7.2 Campos del bundle de Backing

| Campo (Inspector) | Qué hace | Recomendación |
|---|---|---|
| `progressionOverride` | Una progresión fija | Para cartas con identidad de canción concreta |
| `progressionPalette` | Una paleta: una progresión al azar ponderado por canción | Para variedad. **No combinar con override** |
| `chordExpression` | La figura de articulación (§7.3) | — |
| `arpeggioRate` | Velocidad de arpegios y pulsos | `PerBeat` / `Eighth` / `Sixteenth`. **Nunca `Random`** (regla 3) |
| `randomFigureWeights` | Pool ponderado, solo si `chordExpression = Random` | §7.4 |
| `randomRerollChance` | Solo con `Random`: `1` = figura nueva en cada acorde; `0` = una figura por loop | — |
| `velocityJitter` | Variación sembrada de intensidad | `0` (apagado) salvo indicación del dev |
| `adoptProgressionTonality` | La parte adopta el modo de la progresión elegida | Para paletas modales. **No** usar junto con una carta que lleve `TonalityEffect` |
| `voiceLeadingOverride` | Reglas de voicing | Dejar vacío salvo indicación del dev |

Si dejas **vacíos** override **y** paleta, la carta es *articulation-only*: conserva la
progresión que ya suena y solo cambia cómo se toca ("misma progresión, ropa nueva"). Es
legal y útil (§5.18).

### 7.3 Figuras de articulación (`chordExpression`)

| Figura | Cómo suena | Notas |
|---|---|---|
| `Block` | Acorde plano, sostenido | Por defecto |
| `PerBeat` | Un ataque por tiempo | — |
| `Offbeat` | Contratiempo (upstroke ska/reggae) | — |
| `Staccato` | Ataques cortos | — |
| `ArpeggioUp` | Arpegio ascendente | Velocidad en `arpeggioRate` |
| `ArpeggioDown` | Arpegio descendente | Velocidad en `arpeggioRate` |
| `Random` | Sortea entre las seis anteriores | Ver §7.4 |
| `PowerChord` | Raíz + quinta + octava, sin tercera | — |
| `Chugging` | Palm-mute repetido | Pulso a `arpeggioRate` |
| `BassUpperSplit` | Grave en el tiempo, agudos a contratiempo | Suena a ska tranquilo |
| `Bossa` | Comping de bossa auténtico | `arpeggioRate` se ignora |

### 7.4 "Paleta de articulaciones"

No existe un asset de paleta de articulaciones. **El equivalente es
`chordExpression = Random` + `randomFigureWeights`:**

1. `chordExpression` → `Random`.
2. En `randomFigureWeights`, añade una entrada por figura con su peso. **Las entradas
   definen el pool:** una figura que no está, no sale.
3. `randomRerollChance`: `1` si quieres que cambie en cada acorde; `0` si quieres una
   figura estable durante todo el loop.
4. `arpeggioRate`: un valor **concreto** (regla 3).

**Limitación importante:** el pool solo admite las seis figuras básicas (`Block`,
`PerBeat`, `Offbeat`, `Staccato`, `ArpeggioUp`, `ArpeggioDown`). `PowerChord`, `Chugging`,
`BassUpperSplit` y `Bossa` **se ignoran** dentro del pool (decisión D-T2-POOL=A′). Si
necesitas un pool con ellas, es una petición al paquete: avisa al dev.

Si todos los pesos son 0 o la lista está vacía, el paquete usa pesos uniformes y avisa en
consola.

### 7.5 Ejemplo completo: carta "Wormus Pop"

Objetivo: progresiones de pop menor, tocadas mitad en contratiempo y mitad en arpegio,
estables durante cada loop.

1. Progresiones: `Prog_4-4_8m_PopMenor_Aeolian` y dos más (Receta B).
2. Paleta: `Palette_Chords_PopMenor` con las tres, pesos `1, 1, 1` (Receta D).
3. Card Editor → músico Sibi → botón **Backing** → crear carta.
4. En su bundle:
   - `progressionPalette` = `Palette_Chords_PopMenor`;
   - `progressionOverride` = vacío;
   - `chordExpression` = `Random`;
   - `randomFigureWeights` = `Offbeat: 1`, `ArpeggioUp: 1`;
   - `randomRerollChance` = `0`;
   - `arpeggioRate` = `Eighth`.
5. Como las progresiones son Aeolian, la carta necesita fijar el modo: o un
   `TonalityEffect_Aeolian` en sus *modifier effects* (como *Wormus Minor*), **o**
   `adoptProgressionTonality` en el bundle. **Uno de los dos, nunca ambos.**
6. Escúchala (§12) y verifícala (§13).

---

## 8. Receta F — Bundle de Bassline (articulación del bajo)

El bajo **no tiene progresión propia**: sigue la que suena en la parte. Su bundle es solo
articulación. *Fuente: `SSoT_Card_Authoring_Contracts.md` §5.13 regla 6.*

| Campo | Valores | Notas |
|---|---|---|
| `chordExpression` | igual que §7.3 | En el bajo, `Bossa`, `BassUpperSplit` y `PowerChord` se degradan a `Block`. `ArpeggioUp` suena igual que `ArpeggioDown` |
| `arpeggioRate` | `PerBeat`, `Eighth`, `Sixteenth` | Una nota por tiempo **es** `PerBeat` |
| `arpeggioToneMode` | `RepeatedNote`, `ChordToneWalk`, `ImprovisedWalk` | El walk cambia **qué notas**, no el ritmo |
| `pocketMode` | `Off`, `SlapPocket`, `SelfPocket` | **No uses `SlapPocket`** sin un dev: necesita un cambio de código. `SelfPocket` es libre |
| `pocketSlapBoost` / `pocketPopBoost` | enteros, se suman a la intensidad | Valores grandes saturan. Referencia: `0` / `+12` |
| `selfPocketPattern` / `selfPocketSubdivision` | patrón cíclico slap/pop/… | Solo con `SelfPocket` |

**Ejemplo — bajo caminante:** `chordExpression = ArpeggioUp`, `arpeggioRate = PerBeat`,
`arpeggioToneMode = ChordToneWalk`.

El timbre (slap vs fingered) **no** está en el bundle: lo pone un `InstrumentEffect` en la
carta.

---

## 9. Receta G — Crear un patrón de batería

*Fuentes: skill `rhythm-pattern-generator`; `Design_Asset_Naming_v0_1.md` §3.*

1. Abre el **Drum Pattern Editor** (⚠ VERIFICAR menú).
2. En modo **Grid**, configura:
   - compás;
   - compases: **8** para contenido nuevo;
   - subdivisiones por tiempo: `2` (corcheas), `4` (semicorcheas), `3` (tresillos);
   - **lanes**: un instrumento de percusión por fila y su intensidad por defecto.
3. Cambia a modo **Text** y pega una línea de glifos por lane, en el mismo orden.
4. Glifos:
   - `.` silencio;
   - `x` golpe a la intensidad de la lane;
   - `X` acento (120);
   - `o` ghost (50);
   - `|` separador de compás (decorativo).
5. Cada línea debe tener **exactamente** `tiempos × compases × subdivisiones` glifos (sin
   contar `|`). Si sobra o falta, el editor avisa: **cero warnings** antes de guardar.
6. Guarda en `Assets/Resources/ScriptableObjects/Patterns/Drums/`.
7. Nombre (convención aplicada): prefijo de paleta destino + estilo + `8c`, p. ej.
   `FF_Rock8c`.
   - `FF_` = FourOnTheFloor;
   - `SP_` = SyncopatedPocket;
   - `WL_` = WaltzLilt;
   - `OM_` = OddMeterAngular;
   - `CS_` = CompoundSwing.

**Ejemplo (rock 4/4, 2 compases para leerlo; el estándar es 8):** subdivisiones `2` →
4 × 2 × 2 = 16 glifos por línea.

```
x...xx..|x...x.x.
..X...X.|..X...X.
xxxxxxxx|xxxxxxxo
```
Lanes en orden: bombo, caja, hi-hat cerrado. La skill `rhythm-pattern-generator` genera
setup + glifos verificados a partir de un género.

---

## 10. Receta H — Paleta de batería

Igual que la Receta D, duplicando una `DrumPatternPalette-*` existente. Hay **una paleta por
familia rítmica y compás**: no mezcles 3/4 y 4/4 en la misma.

Las paletas en uso y sus cartas están en `SSoT_Card_System.md` §5.2.1. Por ejemplo,
`FourOnTheFloor` → *Default Mode*.

---

## 11. Receta I — Bundle de Rhythm y Melody

**Rhythm (`RhythmCardConfigSO`):**

- `patternOverride` (un patrón fijo) **o** `patternPalette` (una paleta).
- Los campos de densidad o receta (`kickDensity`, `recipeOverride`…) no tienen semántica
  cerrada: **no los toques**.
- Una carta rítmica que no es 4/4 necesita además un `MeterEffect` en la carta, o la parte
  sigue en 4/4 (precedente: *Pentameter*). Eso es diseño de carta: pídeselo al dev.

**Melody (`MelodyCardConfigSO`):** dos rutas que se excluyen.

- **Patrón fijo:** `patternOverride` = un `MelodyPatternData`. Los otros tres campos deben
  quedar **vacíos**: el patrón los anula sin avisar.
- **Procedural:** `phrasePaletteOverride` + `style` + `leadingOverride`. `patternOverride`
  debe quedar **vacío**.
- Patrones de melodía en `Assets/Resources/ScriptableObjects/Patterns/Melodies/` (plural).
- **Hoy, escribe melodías en 4/4.** En compases compuestos la melodía no llena la parte
  (ask `MGP-ALWTTT-METER-1` abierto).

**Harmony:** el bundle debe existir, pero **no lo rellenes**. El paquete no lee sus
campos.

---

## 12. Receta J — Conectar a una carta y escuchar en contexto

1. **Carta nueva:** Card Editor → músico → botón de rol (Backing / Bassline / Rhythm /
   Melody). Rellena id, nombre, coste. El bundle se crea solo; configúralo (§7, §8, §11).
2. **Carta existente con bundle nuevo:** en su sección de composición, arrastra el bundle
   al campo `styleBundle`.
3. **Para que el jugador la vea:** la carta debe estar en el catálogo del músico (la crea
   ahí el Card Editor) y, si es inicial, en el mazo (Deck Editor). Las decisiones de coste,
   copias y si es inicial son de diseño: confírmalas con el equipo.
4. **Escuchar la carta real:** Play Mode → `F12` → Composition → sección de debug-play de
   cartas → inyecta la carta. Es igual que jugarla, sin gastar inspiración.

**Alternativa por JSON** (para lotes; la monta normalmente un dev). Fragmento del bloque de
composición; el resto de la carta sigue `SSoT_Card_Authoring_Contracts.md` §5.3:

```json
{
  "composition": {
    "primaryKind": "Track",
    "trackAction": {
      "role": "Backing",
      "styleBundleCreate": {
        "requested": true,
        "fields": [
          { "name": "progressionPalette", "value": "Assets/…/Palette_Chords_PopMenor.asset" },
          { "name": "chordExpression",    "value": "Offbeat" },
          { "name": "arpeggioRate",       "value": "PerBeat" }
        ]
      }
    }
  }
}
```

Limitaciones de esta ruta:

- Un nombre de campo inexistente falla y lista los válidos.
- Las referencias a assets van por **ruta** o GUID.
- `randomFigureWeights` (una lista) no se puede escribir así: rellénalo en el Inspector.

---

## 13. Verificar con el Composition Inventory

`ALWTTT → Dev → Composition Inventory`. Es de **solo lectura**: no puede romper nada.

| Pregunta | Vista | Qué mirar |
|---|---|---|
| ¿Mi progresión suena en el juego? | **Progression Content** | Nivel `WIRED` = al menos una carta de un catálogo o mazo conectado llega a ella |
| ¿Qué acordes tiene de verdad? | **Progression Content** | La línea Roman se reconstruye de los eventos, no de lo que pegaste |
| ¿Quién usa esta paleta o bundle? | **Progression Content** (`cards:`) / **Cards → Bundles** | Lista de cartas |
| ¿Está bien guardada? | **Chord Progressions** / **Drum Patterns** | Sin `OFF-ROOT` |
| ¿Hay duplicados? | cualquiera | Flag `DUP#n` |

**Exportar:** en **Progression Content**, *Export Markdown…* genera una lista legible de
todas las progresiones con sus acordes y cartas.

**Flags que debes reconocer:**

| Flag | Significa | Qué hacer |
|---|---|---|
| `BASS-GAP` | La progresión es más corta que la parte | Llevarla a 8 compases |
| `SHORT-TAIL` / `OVERFLOW` | Los acordes no cubren, o se pasan de, la longitud declarada | Revisar las duraciones |
| `INNER-GAP` | Hueco entre acordes | Normal si usaste `S (n)`; si no, revisar |
| `OFF-ROOT` | Guardado fuera de la carpeta escaneada | Mover (pedir a un dev: afecta referencias) |
| `ORPHAN` | Nadie lo usa | Añadirlo a una paleta o borrarlo |
| `UNREACHABLE` | Bundle al que ninguna carta apunta | Conectarlo o borrarlo |
| `UNSOURCED` | La carta existe pero no está en ningún catálogo o mazo | Añadir la carta a un catálogo |
| `ORIG-COUNT` | El texto original y los eventos no tienen el mismo número de acordes | Revisar si se editó la rejilla tras importar |

> **Hueco conocido:** hoy el Inventory muestra la articulación del **bajo** (vista
> Cards → Bundles), pero **no** la del Backing. Hay un lote propuesto para añadirla. Hasta
> entonces, la articulación del Backing se revisa abriendo el bundle en el Inspector.

---

## 14. Problemas frecuentes

| Síntoma | Causa probable | Arreglo |
|---|---|---|
| Mi progresión nueva no suena nunca | No está en ninguna paleta o bundle que use una carta conectada | §13: debe salir `WIRED` |
| Suena otra progresión, no la de mi paleta | El bundle tiene también `progressionOverride` | Vaciar el override (regla 5) |
| La articulación no se nota | `arpeggioRate = Random` | Rate concreto (regla 3) |
| Pedí bossa y suena a ska | Elegiste `BassUpperSplit` | Usar `Bossa` (regla 4) |
| El bajo se calla a mitad de parte | Progresión de 4 compases en parte de 8 | 8 compases (regla 1) |
| Los acordes suenan en mayor siendo una progresión menor | Ni la carta fija el modo ni el bundle lo adopta | §7.5 paso 5 |
| Un vamp modal pierde su color | Política `DiatonicToPart` | `AsAuthored` (regla 7) |
| Acordes muy rápidos y luego silencio | Se tocó `subdivisions` | Re-importar la progresión |
| El import falla entero | Token fuera del alfabeto (p. ej. `IV9`, `add9`) | Reducir al alfabeto (§A.2) |
| Cambié una paleta y otra carta suena distinta | Paleta o bundle compartido | Duplicar en vez de editar (regla 6) |

---

## 15. Checklist de entrega al dev

Al terminar una tanda, pásale al dev:

- [ ] Lista de **assets nuevos**: progresiones, patrones, paletas y bundles, con su ruta.
- [ ] Lista de **assets editados**, sobre todo paletas compartidas, y qué cartas afectan.
- [ ] **Cartas nuevas o cambiadas** y su bundle. El dev debe actualizar la tabla de
  bindings de `SSoT_Card_System.md` §5.2.1.
- [ ] Export Markdown de **Progression Content**.
- [ ] Captura del Inventory filtrado por *Flagged* sin flags inesperados.
- [ ] Seed usada en las escuchas, para que el dev pueda reproducirlas.

---

## Apéndice A — Valores válidos

### A.1 Enumeraciones

| Campo | Valores |
|---|---|
| Time signature | `FourFour`, `ThreeFour`, `TwoFour`, `SixEight`, `NineEight`, `TwelveEight`, `FiveFour`, `SevenEight` |
| Tonality | `Ionian`, `Dorian`, `Phrygian`, `Lydian`, `Mixolydian`, `Aeolian`, `Locrian` |
| Cadence | `None`, `Authentic`, `Plagal`, `Half`, `Modal` |
| Quality render policy | `AsAuthored`, `DiatonicToPart`, `DiatonicToPartFunctional` |
| `chordExpression` | `Block`, `PerBeat`, `Offbeat`, `Staccato`, `ArpeggioUp`, `ArpeggioDown`, `Random`, `PowerChord`, `Chugging`, `BassUpperSplit`, `Bossa` |
| `arpeggioRate` | `PerBeat`, `Eighth`, `Sixteenth` (`Random` existe, pero no usar: regla 3) |

### A.2 Alfabeto de calidades para importar (texto Roman)

| Calidad | Sufijo | Ejemplo |
|---|---|---|
| Mayor | `maj` | `Imaj` |
| Menor | `m` | `vim` |
| Disminuido | `dim` | `viidim` |
| Aumentado | `aug` | `IIIaug` |
| Mayor 7 | `maj7` | `Imaj7` |
| Menor 7 | `m7` | `iim7` |
| Dominante 7 | `7` | `V7` |
| Semidisminuido | `ø7` (o `m7b5`) | `iiø7` |
| Disminuido 7 | `dim7` | `viidim7` |
| Sus2 | `sus2` | `Isus2` |
| Sus4 | `sus4` | `Vsus4` |

Novenas, sextas, `add9` y acordes con barra (`G/B`) **no se pueden importar**. Redúcelos:

- `maj9` → `maj7`;
- `m9` → `m7`;
- `6` → tríada;
- `G/B` → `G`.

El modelo de datos tiene algunas calidades más, pero el texto de importación no las acepta.
Un asset que ya las contiene renderiza bien; solo falla al re-importar el texto.

### A.3 Glifos de batería

| Glifo | Significado |
|---|---|
| `.` | silencio |
| `x` | golpe a intensidad de la lane |
| `X` | acento (120) |
| `o` | ghost (50) |
| `\|` | separador de compás |

---

## Apéndice B — Fuentes

- `SSoT_Card_Authoring_Contracts.md` §5.13, §5.14, §5.16, §5.17, §5.18
- `SSoT_Card_System.md` §5.2.1
- `SSoT_Dev_Mode.md` §18
- `SSoT_Editor_Authoring_Tools.md` §4, §17
- `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.7, §8.9
- `MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md` §5–§6
- `Design_Asset_Naming_v0_1.md`
- `MGP-20260810_CURRENT_STATE.md` (arco de articulación CA)
- Skills `chord-progression-importer` y `rhythm-pattern-generator`

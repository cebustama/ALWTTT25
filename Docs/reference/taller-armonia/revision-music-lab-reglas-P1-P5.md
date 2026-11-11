# Reglas de P1–P5 · los niveles del taller con los recursos de Music Lab (2026-11-11)

Paso 1 del arranque `ARRANQUE_niveles_recursos.md` (proceso estándar, §4 de `revision-music-lab-recursos-2026-11-11.md`). **Estado (2026-11-11, tarde):** decisiones de Claudio tomadas (§9) y **perillas programadas en `template.html`, apagadas**; `oro.js comparar oro-v3.json` sigue dando 792 idénticos. Casos de cada perilla: `node scripts/p1p5.js`. Falta la prueba de reducción y la de oído.

Fuentes: `template.html` (`arrange()`, v3 con perillas), `Music Lab/recursos-armonicos.md` (nota del 2026-11-11) y las fichas de `Music Lab/Biblioteca/jazz/`. Los compases (c.) son los de cada ficha; «A1 c. 2» = sección A1, compás 2.

## 0. Marco común (no cambia)

- Ningún acorde escrito desaparece. Lo añadido ocupa la **segunda parte** de un acorde (medio compás en 4/4, un compás en 3/4: la unidad `h`). Excepción ya existente: el sus4 ocupa la **primera** parte de la dominante.
- Cada adorno gasta **una** unidad del presupuesto de la vuelta (nivel 2: 1; nivel 3: 2), aunque añada dos acordes. Los cambios de **calidad** (el color) no gastan.
- Vuelta 1 sin adornos; ≤2 acordes distintos → 1 cambio; nada de modular.
- Todo va detrás de una perilla nueva, **apagada**: con las perillas de fábrica, `oro.js comparar oro-v3.json` sigue dando 792 idénticos.
- Sin tres acordes por compás (el alfabeto de MidiGenPlay no los tiene): por eso un ii–V necesita un acorde de al menos un compás y medio.

## 1. P1 · ii–V completo (`paso: "iiV"`)

**Qué hace.** Donde el paso clásico pone solo la dominante del acorde siguiente, P1 pone el **ii y la dominante**: el acorde anfitrión se queda con su primera parte y los dos últimos medios compases son `ii` y `V7` del destino.

`I (2) → IV` pasa a `I (1) – vm7 (½) – I7 (½) → IV`

- **Dónde:** en el acorde anterior a un destino de la escala que no sea la tónica (como el paso actual), si dura al menos `3h` (un compás y medio en 4/4; en la práctica, acordes de 2 compases).
- **Novedad respecto al paso clásico:** el paso clásico se salta los anfitriones que ya están una quinta por encima del destino (ya son su dominante). P1 **no** se los salta si el anfitrión es mayor o dominante: los parte en `ii` + él mismo como `7`. Es el caso más frecuente de la Biblioteca (el ii–V hacia el IV, que sale del propio I). Sí se los salta si el anfitrión es menor y está una quinta por encima (ya es el ii de un ii–V).
- **Si el anfitrión ya es el ii del destino** (un tono por encima, como `iii → ii`): solo se añade la `V7`.
- **Si no hay sitio para dos** (acorde de un compás): ver duda D1.
- **Calidad del ii:** ver duda D2.
- **Modos:** todos (como el paso actual).
- **Presupuesto:** una unidad.
- **Orden:** en el lugar del paso (es un valor más de esa perilla, excluye clásico y cromático).

**Casos de la Biblioteca**

| Canción, compás | Esqueleto | Lo que tocan |
|---|---|---|
| *Misty* A1 cc. 1–3 | `I (2) → IV` | `E♭maj7 \| B♭m7 E♭7 \| A♭maj7` |
| *Billie's Bounce* A cc. 3–5 | `I7 (2) → IV7` | `F7 \| Cm7 F7 \| B♭7` |
| *Billie's Bounce* A cc. 7–9 | `I7 (2) → ii` | `F7 \| Am7 D7 \| Gm7` |
| *Oleo* A1 cc. 5–6, *Satin Doll* B cc. 1–3 | `I → IV` | `Fm7 B♭7 \| E♭7`, `Gm7 C7 \| … \| Fmaj7` |

## 2. P2 · Cadena de dominantes (`cadenaDominantes: true`)

**Qué hace.** Todo acorde que baja una quinta hacia el siguiente pasa a `7`, también los menores (`iii → III7`, `vi → VI7`). Hoy el color solo lo hace con los mayores. No añade acordes.

`I (2) – iii (2) – vi (2) – ii (2)` pasa a `I – III7 – VI7 – ii`: es *All of Me* A tal cual.

- **Dónde:** cualquier acorde de la progresión, salvo la tónica (como hoy) y los disminuidos; el destino tiene que ser un acorde sin disminuir. Con novenas (nivel 3), `7 → 9` si la 9.ª está en la escala, como hoy.
- **El ii delante de la V:** ver duda D3.
- **Desde qué vuelta:** ver duda D4.
- **Modos:** ver duda D5 (todos los casos son mayores).
- **Presupuesto:** no gasta (es color).
- **Orden:** con el color, antes de los adornos. Efecto en cadena que hay que saber: crea más dominantes «que resuelven», así que el sus4 (nivel 2) y P5 tienen más sitios donde caer.

**Casos de la Biblioteca**

| Canción, compás | Esqueleto | Lo que tocan |
|---|---|---|
| *All of Me* A1 cc. 1–8 | `I – iii – vi – ii` | `Cmaj7 \| % \| E7 \| % \| A7 \| % \| Dm \| %` (el ii se queda menor) |
| *Oleo* B | `iii – vi – ii – V` | `D7 \| % \| G7 \| % \| C7 \| % \| F7 \| %` (el ii también es `7`) |
| *Wave* A1 cc. 7–10 | `iii – vi – ii – V` | `F♯7 \| B7 \| E7 \| B♭7 A7` (el ii también es `7`) |
| *Misty* A1 cc. 7–8, *Oleo* A1 cc. 3–4 | `iii – vi – ii – V` | `Gm7 C7 \| Fm7 B♭7` (solo el vi; el ii, menor) |

## 3. P3 · Disminuido de paso (`disminuido: true`)

**Qué hace.** Pone un `°7` en la segunda parte de un acorde, como peldaño cromático hacia el siguiente.

- **Variante ascendente (la base):** si el siguiente acorde está **un tono por encima**, la segunda parte es el `°7` un semitono por debajo del destino. `I → ii` pasa a `I – ♯I°7 → ii`; también `IV – ♯IV°7 → V` y `V – ♯V°7 → vi`.
- **Variante de vecino:** `IV → I` pasa a `IV – ♯IV°7 → I`. Ver duda D6.
- **Variante descendente** (`I – ♭VI°7 → v`, *Wave*): un solo caso. Ver duda D6.
- **Modos:** todos; el destino tiene que ser de la escala y no disminuido.
- **Presupuesto:** una unidad.
- **Orden:** antes del acorde de paso (es más específico: donde cabe un `°7`, gana; en los demás sitios sigue mandando el paso).

**Casos**

| Canción, compás | Variante | Lo que tocan |
|---|---|---|
| *Georgia* §5a (análisis de Music Lab, no Biblioteca) | ascendente | `Cmaj7 – C♯dim7 – G/D` (bajo Do → Do♯ → Re) |
| *Billie's Bounce* A cc. 5–7 | vecino | `B♭7 \| B°7 \| F7` |
| *Wave* A1 cc. 1–3 | descendente | `Dmaj7 \| B♭°7 \| Am7` |

## 4. P4 · ♭VI7 en menor (`bVI7Menor: true`)

**Qué hace.** En Eólico, el `VI` escrito que va a la dominante se vuelve `VI7`: el giro `♭VI7 – V7 – i` del blues menor. Es un cambio de calidad; no añade acordes.

`i – iv – VI – V7` pasa a `i – iv – VI7 – V7`

- **Dónde:** el `VI` (mayor o `maj7`) cuyo siguiente acorde es la `V`/`v`, o un acorde que lleva a ella (ver duda D7 sobre `VI → iv`).
- **Modos:** Eólico (el único con `♭VI` en la escala). En Dórico no hay `♭VI` escrito; ahí haría falta insertarlo (ver D7).
- **Presupuesto:** no gasta si es solo color.
- **Orden:** con el color, antes de los adornos. Convive con la perilla `bVI` de la v3 (`♭VImaj7`, solo Jónico y Mixolidio): no se pisan, cada una en sus modos.

**Casos**

| Canción, compás | Lo que tocan |
|---|---|
| *Thrill* §5 (análisis) | `G7 → F♯7 → Bm` (`♭VI7 – V7 – i`) |
| *Summertime* A1 cc. 5–8 | `Dm \| F7 \| F♯m7♭5 B7 \| E7` (el `VI7` va a la V por su ii–V; con reserva: el *play-along* no lo tiene) |
| *Cantaloupe Island* cc. 1–8 (Dórico) | `Fm7 ×4 \| D♭7 ×4` (escrito así; aquí no hay nada que insertar) |

## 5. P5 · Remate con deslizamiento (`remate: "deslizado"`)

**Qué hace.** En la última vuelta, antes de volver al primer acorde, pone **dos dominantes a medio tono**: `♭VI7 – V7 → I`. Las cuatro voces bajan un semitono en bloque.

- **Dónde:** el mismo sitio que el remate de hoy (segunda parte del último acorde, si el bucle no cierra ya por cadencia). Hacia un primer acorde que no es la tónica: `(destino + ♭6)7 – (destino + 5)7`.
- **Si solo cabe uno:** la `V7` (el remate simple).
- **Si el último acorde ya es el `♭VI`:** solo la `V7`.
- **Modos:** Jónico y Eólico (los que tienen `V7` en su remate). En los demás, su acorde modal, como el remate rico.
- **Presupuesto:** una unidad (es el remate).
- **Orden:** el del remate (el primero).
- **Variante como adorno, fuera del remate:** ver duda D8.

**Casos**

| Canción, compás | Lo que tocan |
|---|---|
| *Wave* A1 cc. 9–11 | `E7 \| B♭7 A7 \| Dm7` |
| *Thrill* §5 (análisis) | `G7 → F♯7 → Bm` |
| *Georgia* coda (análisis) | `A♭7 → G7` |
| *Footprints* T cc. 1–2 | `F♯m7♭5 F7 \| E7 A7` (en cadena) |

## 6. Orden total y presupuesto

Primero el color (v3 + P2 + P4), después los adornos en este orden fijo:

1. remate (no / simple / rico / **deslizado**)
2. sus4
3. menor prestado
4. **disminuido (P3)**
5. acorde de paso (no / clásico / cromático / **iiV**)
6. ♭VImaj7 (Jónico, Mixolidio)

Con `preferirAcordeNuevo` el sus4 sigue yendo detrás del paso, como hoy.

## 7. Perillas nuevas (y por qué no son las de la propuesta)

| Perilla | Valores | Fábrica |
|---|---|---|
| `paso` | + `"iiV"` | igual que v3 |
| `remate` | + `"deslizado"` | igual que v3 |
| `cadenaDominantes` (P2) | sí / no | no |
| `disminuido` (P3) | sí / no | no |
| `bVI7Menor` (P4) | sí / no | no |

**Cambio respecto a la revisión del §3:** allí P2 era `color: "dominantes"`, P3 `paso: "disminuido"` y P4 `bVI: "7"`. Las paso a perillas propias porque, como valores, se excluían con lo que tienen que acompañar: P2 dentro de `color` no se podría combinar con las novenas del nivel 3; P3 dentro de `paso` no se podría combinar con el ii–V (y el perfil «jazz» quiere P1 + P2 + P3); y cambiar `bVI` de sí/no a una lista rompería las perillas que ya están guardadas en el catálogo. P1 y P5 sí se quedan como valores porque excluyen de verdad a sus hermanos (un acorde de paso es o clásico o ii–V; un remate es uno).

## 8. Dudas musicales para Claudio

**D1 · P1, si el acorde solo dura un compás (no caben ii y V).**
- A: se pone solo la `V7` (como el paso clásico). P1 nunca hace menos que el clásico.
- B: no se pone nada. P1 solo suena donde hay ii–V completo.
- *Recomiendo A:* en progresiones de un acorde por compás, B dejaría P1 casi mudo.

**D2 · P1, calidad del ii.**
- A: la de la escala si su nota está en la escala (`Am7` en *Billie's* c. 8, `Gm7` en *Misty* A1 c. 7); si no, `m7` hacia mayor y `ø7` hacia menor.
- B: siempre según el destino: `m7` hacia mayor o dominante, `ø7` hacia menor (*Misty* B c. 7, `Gm7♭5 C7 → Fm7`).
- *Recomiendo A:* acierta en 2 de 3 casos de la Biblioteca y suena «dentro de la tonalidad»; B acierta en *Misty* B.

**D3 · P2, el ii que va a la V.**
- A: se queda menor (`I – VI7 – iim7 – V7`): *All of Me* A, *Misty*, *Oleo* A, *Satin Doll*.
- B: también se vuelve `7` (`III7 – VI7 – II7 – V7`): puente de *Oleo*, *Wave*, *Georgia*.
- *Recomiendo A:* conserva el ii–V, que es el recurso más frecuente (15 casos). B es el sabor «ragtime», más antiguo y más tenso; podría ser un perfil.

**D4 · P2 y P4, ¿desde la vuelta 1?** Hoy el color (tríadas → séptimas) suena desde la vuelta 1.
- A: P2 y P4 desde la vuelta en que empiezan los adornos (la vuelta 1 suena con las calidades escritas).
- B: desde la vuelta 1, como el resto del color.
- *Recomiendo A:* un `vi` que se vuelve `VI7` cambia más que una séptima; la vuelta 1 debe sonar a lo que se escribió.

**D5 · P2, ¿en qué modos?**
- A: solo Jónico (todos los casos son mayores).
- B: Jónico, Lidio y Mixolidio.
- *Recomiendo A,* y ampliar cuando haya casos.

**D6 · P3, ¿qué variantes?**
- A: solo la ascendente.
- B: ascendente + vecino (`IV – ♯IV°7 → I`).
- C: las tres.
- *Recomiendo B:* la de vecino es la del blues (*Billie's Bounce*); la descendente tiene un solo caso.

**D7 · P4, ¿hasta dónde?**
- A: solo el color: `VI → VI7` en Eólico cuando va a la `V`.
- B: A, y también cuando va al `iv` (*SIBLY*: `A♭7 → Fm7`).
- C: A, y además insertar `♭VI7` antes de una `V` escrita cuando no hay `VI` (en Eólico y Dórico; gasta presupuesto).
- *Recomiendo A:* es el giro con más evidencia. C se solapa con P5.

**D8 · P5, ¿también como adorno, fuera del remate?** Antes de una dominante escrita que resuelve, su primera parte se vuelve la dominante medio tono arriba (`E7 | A7` → `E7 | B♭7 A7`, *Wave* c. 10). Va en la **primera** parte, como el sus4.
- A: solo remate.
- B: remate y adorno (perilla aparte, gasta presupuesto).
- *Recomiendo A* para esta sesión: el remate no se dispara en los bucles que ya cierran por cadencia (`… V → I`), así que P5 sonará poco; si en la prueba de oído gusta, B es la siguiente.

**Una más, opcional (D9) · ¿un remate ii–V?** Hoy el remate simple pone solo `V7` antes de la tónica. Con `remate: "iiV"` sería `iim7 – V7 → I` (el turnaround de *Misty* A1 c. 8 y *Billie's Bounce* c. 12).
- A: sí, como quinto valor de `remate` (no / simple / rico / deslizado / iiV) (sale casi gratis con el código de P1).
- B: no por ahora.
- *Recomiendo A.*

## 9. Decisiones de Claudio (2026-11-11) y cómo quedaron en el código

| Duda | Decisión | En el código |
|---|---|---|
| D1 | A: con sitio para uno solo, la `V7` | `paso: "iiV"` |
| D2 | A, **con dudas**: «a veces suena mejor el ii del destino, m7♭5 hacia menores» | regla A, más la perilla **`iiDelDestino`** (apagada) para compararlas en la prueba de oído |
| D3 | A: el ii que va a la V se queda menor | `cadenaDominantes` excluye `ii → V` |
| D4 | **B: desde la vuelta 1**, «que se note de inmediato que cambió respecto al nivel anterior» | P2 y P4 van dentro de `color()`, que suena en todas las vueltas |
| D5 | A: solo Jónico | `cadenaDominantes` |
| D6 | **C: las tres variantes** | `disminuido`: sube (tono arriba), vecino (IV → I) y baja (cuarta abajo, `I – ♭VI°7 – V`). Puede usar el último acorde, porque `IV → I` cierra muchos bucles |
| D7 | **B**: también `VI → iv` | `bVI7Menor`: el `VI` que va a `V`/`v` o a `iv` |
| D8 | A: solo como remate | `remate: "deslizado"` |
| D9 | Sí | `remate: "iiV"` (Jónico `iim7–V7`, Eólico `iiø7–V7`; en los otros modos, su acorde modal) |

**Precisión de D2 (A), que cambia un poco lo escrito en el §8:** «la de la escala» solo cuenta cuando el acorde de la escala sobre esa nota es **menor o disminuido**. Si es mayor, se decide por el destino. Sin esa precisión, el ii del IV (`B♭` en Mi♭) saldría mayor, y en *Misty* A1 c. 2 es `B♭m7`.

**Detalles que salieron al programar:**
- Al hacerse dominantes más acordes con P2, el sus4 del nivel 2 cae sobre ellos (`III7sus4 – III7` en *All of Me*). Es lo esperado, pero conviene oírlo.
- El remate, de cualquier clase, no se activa si el bucle ya cierra por cadencia, y `iv → i` cuenta como cadencia plagal (regla de la v3).
- **Nombres:** el `°7` se escribe por la nota del bemol (`biidim7` en vez de `#idim7`, `bvdim7` en vez de `#ivdim7`). Suena igual y el Import lo acepta, porque es la misma fundamental. Solo cambia cómo se lee.
- `iiDelDestino` también se aplica al remate `iiV`.
- La tarea `armonia.ajustar` de la web (el prompt del LLM) todavía no conoce las perillas nuevas. Se pone al día cuando alguna se encienda.

## 10. Wrapped

Página privada con los recursos, sus reglas, ejemplos de la Biblioteca que suenan y un laboratorio con todas las perillas: https://claude.ai/artifact/YaM8WSewBob3sxL2nEYCM3. Fuente en `recursos-wrapped/` (`fuente.html` + `build.mjs`, que pega la teoría de `template.html`: lo que suena es exactamente lo que hacen los niveles). Para rehacerlo: `node recursos-wrapped/build.mjs` y volver a publicar `recursos-wrapped.html`. La «Propuesta para el oído» del laboratorio (nivel 2: ii–V y ♭VI7; nivel 3: cadena, disminuido y ♭VI7) es una hipótesis del agente para empezar la prueba de oído, no una decisión.

## 11. Prueba de reducción (2026-11-11)

`node scripts/reduccion.js md` → `reduccion-resultados.md` (tablas completas). Nueve secciones de la Biblioteca (*All of Me* A1, B y C; *Billie's Bounce*; *Misty* A1; *Oleo* A1 y B; *Wave* A1; *Summertime* A1). El agente escribió el esqueleto de cada una a mano con las reglas R1–R5 de la cabecera del script. Después pasó cada esqueleto por los niveles, en 3 vueltas y 4/4, y lo comparó con la canción en medios compases (raíz + familia: mayor, menor, dominante, ø, °). Hay tres medidas:
- **parecido:** medios compases iguales a la canción;
- **acierta:** de los medios compases donde la canción se aparta del esqueleto, cuántos reproduce el taller;
- **inventa:** de los medios compases que cambia el taller, cuántos no están en la canción.

| Configuración | Parecido | Acierta | Inventa |
|---|---|---|---|
| Nivel 1 (esqueleto) | 33 % | 0 % | — |
| N2 v3 / N3 v3 | 43 % / 43 % | 16 % / 17 % | 15 % / 31 % |
| + P2 cadena (N2 / N3) | **57 % / 56 %** | **44 % / 45 %** | 18 % / 25 % |
| + P1 ii–V | 44 % / 44 % | 18 % / 20 % | 15 % / 30 % |
| + P4 ♭VI7 | 44 % / 44 % | 18 % / 19 % | 20 % / 32 % |
| + P3, + P5, + D9 | ≈ v3 | ≈ v3 | ≈ v3 |
| N2 P1+P2+P4 (la mejor) | 59 % | 47 % | 20 % |
| N3 propuesta (P2+P3+P4) | 58 % | 47 % | 25 % |

**Lo que dice:**
1. **P2 es la que más acerca el taller al jazz:** casi triplica los adornos acertados (16 → 44 %) e inventa poco. *All of Me* A1 pasa de 46 % a 90 % y *Oleo* B, de 23 % a 73 %.
2. **P1 suma poco (+1–3 puntos).** Muchas veces el ii–V de la canción ya estaba en el esqueleto (R5), y delante de los dominantes de P2 añade ii que *All of Me* y *Oleo* B no tienen. Junto con P2, aún suma 1–2 puntos.
3. **P4 acierta donde debe:** *Summertime* pasa de 50 % a 63 % en el nivel 2.
4. **Esta prueba no puede juzgar P3, P5 ni D9, así que los juzgará el oído.** Los remates no se activan porque las secciones de jazz ya cierran por cadencia. En la canción el °7 dura un compás y en el taller medio, así que la medida solo le da media nota. Y en *Wave* la reducción quitó el acorde al que iba el `B♭°7`.
5. **El presupuesto no es el cuello de botella:** sin tope (3 cambios por vuelta) se ganan 0–2 puntos.
6. **Ninguna perilla consigue esto (lo que más falla):**
   - la tónica dominante del blues (`I7`, `IV7`: *Billie's* se queda en 18 %);
   - el `II7` antes de `iim7–V7` (*All of Me* B, *Oleo* B: D3 A lo deja menor);
   - el ii semidisminuido de un ii–V **en tonalidad menor** aunque vaya a la V mayor (*Summertime* c. 7, `F♯m7♭5 B7 → E7`): ni la regla A ni «ii según el destino» lo dan;
   - el ii–V de un compás cada acorde;
   - el vamp `im6 – iim6`.
7. **P2 se equivoca en *All of Me* B:** el `vi` que sigue a `III7` se vuelve `VI7`, pero la canción lo deja menor. En la sección A, en cambio, lo dominantiza. No hay una regla que acierte las dos.
8. Las diferencias de 1–2 puntos son ruido con 9 secciones.

**Límites:** son 9 secciones, todas de jazz. Los esqueletos los escribió el agente, y R3 da el tiempo al acorde anterior, como hace el taller, así que la prueba le es favorable. Además, un adorno desplazado medio compás cuenta como fallo entero.

## 12. D10, D11 y la hoja de oído (2026-11-11)

- **D10 (aceptada):** con `iiDelDestino`, en tonalidad menor el ii de un ii–V es siempre `m7♭5` (*Summertime* c. 7, `F♯m7♭5 B7 → E7`). Programada en `iiQ`; oro-v3 idéntico. En la reducción, N3 + P1 con el ii del destino pasa de 44 % a 45 %.
- **D11 (aceptada, apuntada para P6):** la tónica dominante del blues (`I7`, `IV7`) será un perfil «blues», no una perilla suelta. Es el fallo más grande de la reducción (*Billie's*, 18 %).
- **Hoja de oído:** https://claude.ai/artifact/Bug5CcKLJWkKWmpNfqr3Ki (fuente en `prueba-oido-p1p5/`, `node prueba-oido-p1p5/build.mjs`). Son 14 pares a ciegas (v3 frente a la perilla, con el orden barajado de forma fija por par) y la decisión de nivel para cada perilla. Las respuestas se guardan en el db del artefacto, en las colecciones `respuestas`, `decisiones` y `generales`, y el agente las lee con `ArtifactData list`.
- **Hallazgo de diseño al preparar la hoja:** en el nivel 2, P1 casi nunca sonaría. El sus4 va antes que el acorde de paso y gasta el único cambio de la vuelta, y en la última vuelta lo gasta el remate. Para tener P1 en el 2 habría que encender también `preferirAcordeNuevo`.

## 13. Resultados de la hoja de oído y v4 (2026-11-11)

Respuestas leídas del db de la hoja (https://claude.ai/artifact/Bug5CcKLJWkKWmpNfqr3Ki). ✓ = prefirió la perilla, ✗ = la v3, ~ = igual o ninguna.

| Perilla | Pares | Decisión de Claudio | Notas suyas |
|---|---|---|---|
| P2 cadena de dominantes | ✓ ✓ ~ | nivel 3 | Cincuentas: «ambas funcionan bien, las dejaría random» |
| P4 ♭VI7 en menor | ✓ ✓ | niveles 2 y 3 | blues: «me encanta este recurso» |
| P1 ii–V | ✓ ✓ | nivel 3, en vez del cromático | — |
| ii según el destino | ✓ ✓ | sí | mayor: «suenan raros los dos compases con el mismo acorde» (es el esqueleto de prueba, `I (2) – ii (2)`, no la perilla) |
| P3 disminuido | ✗ ✓ | nivel 3 | en la axis prefirió la v3; en la subida, el °7 |
| P5 remate deslizado | ✗ ~ | apagado (remate del 3: rico, v3) | mayor: «no entendí muy bien la progresión»; menor: ninguna |
| D9 remate ii–V | ✗ | apagado | «me gustó mucho esta progresión» (la de fábrica) |
| Remate del nivel 2 | — | simple (v3) | — |

Pregunta general 1 («¿algún par deja de sonar a la misma progresión?»): «Creo que no». Pregunta 2: le gustó mucho el ejercicio; quiere más variedad de progresiones, tempos y métricas, cuando se mezcle con el taller de ritmos.

**Lectura.** Los recursos que *añaden* (P2, P4, P1) ganan casi siempre; los que *rematan* (P5, D9) pierden. Los remates de la v3 ya se habían validado (16/16) y el oído no pide cambiarlos. P3 queda en el límite: se enciende en el 3 por decisión de Claudio, y conviene volver a oírlo en más contextos. Tamaño: 14 pares de un solo oyente; vale para elegir valores por defecto, no para afirmar preferencias generales.

**v4 en el código:** `PERILLAS_V3` (nombre que se mantiene) pasa a: N2 `bVI7Menor`, `iiDelDestino`; N3 `cadenaDominantes`, `paso:"iiV"`, `disminuido`, `bVI7Menor`, `iiDelDestino`. Oro `oro-v4.json`: 792 casos, 169 cambian respecto a la v3 (N3 de todos los modos y N2 en Eólico; N1 idéntico). Reducción con la v4: parecido 44 % (N2) y 59 % (N3), frente al 43 % de la v3; acierta 48 % en el N3. La hoja de oído compara siempre contra la v3 congelada (`render` de `prueba-oido-p1p5/fuente.html`), así que no cambia al cambiar los valores por defecto. Desde hoy, en la hoja se puede pulsar un acorde para que suene desde ahí (petición de Claudio).

**Pendiente:** el prompt de `armonia.ajustar` en la web no conoce las perillas nuevas; la copia web del taller (`--web`), si Claudio la quiere; P6, perfiles de estilo (blues con I7/IV7, D11).

## 14. D12–D14 y la hoja de remates (2026-11-11)

Claudio: los remates no se pudieron juzgar en la hoja P1–P5 porque sonaban al final del audio, sin oír a dónde llevaban. **El veredicto de P5 y D9 de §13 queda anulado**; se repite en una hoja propia.

- **D12 = B** (oír la resolución): el taller ya tenía «Repetir»; se añade «Resolver al final» (sin repetir, suena el primer acorde una vez más). La hoja de remates tiene las dos opciones y «Solo el final de A/B».
- **D13 = B** (variaciones desde la vuelta 1, **también en el juego**): `desdeVuelta` admite 1. Los valores de fábrica siguen en 2 (la vuelta 1 tal cual), así que el oro v4 no cambia. Es una excepción a la regla de identidad de Music Lab que Claudio decidió por su cuenta. Conviene avisar a Music Lab cuando se revise P6.
- **D14 = A** (deslizador de intensidad): `INTENSIDAD` en la parte de teoría de `template.html`: 0 tal cual (N1), 1 solo color (N2 sin cambios), 2 N2 de fábrica, 3 N3 de fábrica, 4 N3 desde la vuelta 1, 5 N3 desde la vuelta 1, sin tope (3 cambios) y «que se note el adorno». Mueve el nivel y las perillas; las perillas finas siguen debajo, y si se tocan a mano el deslizador dice «propia». En el juego: subir de nivel una carta = un paso del deslizador. MidiGenPlay lo portará como perfiles.
- **Hoja de remates** (https://claude.ai/artifact/N1cGU1fjPphhfeBZvmAE3k, fuente `prueba-oido-remates/`; base: la v4 de fábrica): 6 pares (deslizado mayor y menor, ii–V mayor y menor, rico frente a simple en el N3, ii–V en el N2) y dos decisiones (remate del N3, con la opción «al azar entre los que me gustan», y remate del N2). Respuestas en el db (`respuestas`, `decisiones`, `generales`).

**Resultados de la hoja de remates (5 de 6 pares respondidos; falta `iiv-mayor`).** Deslizado en mayor ✗ (prefiere la backdoor; nota: «hay un acorde que se me hace demasiado largo»); deslizado en menor ✓; ii–V en menor ~ («las dos me gustan, A es más raro pero igual funciona»); simple frente a rico en el N3 ✓ (prefiere el simple); ii–V en el N2 ~. Decisiones: N2 simple (sin cambio); **N3 «al azar entre los que me gustan»**, que pide una regla nueva (D15, por diseñar): un remate por carta, elegido de forma determinista (hash de la progresión) dentro de un grupo por modo. Grupo provisional: mayor {rico, simple}, menor {rico, deslizado, ii–V}.

Par `iiv-mayor` respondido después: ~ (igual). Grupo de remates del N3, según la hoja: mayor {rico, simple, ii–V}, menor {rico, deslizado, ii–V}.

## 15. Las cartas son paletas, no canciones (Claudio, 2026-11-11)

Claudio corrige la propuesta D15 de §14 («un remate por carta, siempre el mismo»): **una carta representa un patrón o idea, no la canción definitiva**. No quiere que suene siempre igual; quiere variedad en cada jugada dentro de una identidad o paleta. Más adelante quiere poder guardar una canción ya generada.

Propuesta (por decidir al abrir la sesión):
- **El azar va por jugada, con semilla.** `arrange(base, nivel, nV, beats, perillas, semilla)`: la misma semilla da la misma salida, y cada jugada usa una semilla nueva. Guardar una canción = guardar texto + perillas + semilla. Las pruebas de oro corren con semillas fijas, y la paridad con MidiGenPlay sigue siendo posible.
- **La carta define la paleta; la semilla elige dentro.** Pasos posibles:
  - (a) remate al azar dentro del grupo por modo (lo pedido en la hoja);
  - (b) qué acorde recibe cada adorno y en qué vuelta, en vez del orden fijo de izquierda a derecha;
  - (c) qué adorno entre los encendidos, con pesos (por ejemplo, ii–V 2, disminuido 1), que sería el «perfil» de P6.
- **La identidad la cuidan las reglas que ya existen:** ningún acorde escrito desaparece, no se modula, la vuelta 1 tal cual de fábrica, y presupuesto por vuelta.
- **En el taller:** botón 🎲 «otra versión», la semilla visible y «guardar esta versión».
- **Riesgo:** los recursos de la v4 se eligieron a ciegas como valores fijos. Con azar, un par puede sonar distinto cada vez, así que las hojas de oído tendrán que fijar la semilla.

## 16. Reglas del azar con semilla (sesión del 2026-11-11)

Decisiones de Claudio al abrir: **D1 = A, D2 = B, D3 = B, D4 = C** (las recomendadas).

- **D1 = A, sin semilla = v4.** Si `arrange(base, nivel, nV, beats, perillas, semilla)` no recibe semilla (`undefined`, `null` o vacía), sigue el orden fijo de izquierda a derecha y el remate «azar» vale «rico». `oro-v4.json` sigue con sus 792 casos idénticos.
- **D2 = B, un hash por decisión.** `hash32(semilla, vuelta, paso, k)` es un mezclador de enteros de 32 bits (el finalizador de splitmix32, `Math.imul` y desplazamientos sin signo; en C# da lo mismo con `uint` y `unchecked`). Cada decisión tiene su número propio y no se tira en cadena. Así, encender una perilla no cambia lo que ya decidían las demás (es la lección de Islands: el orden de las tiradas no debe importar). Nada de `Math.random` en la teoría.
- **D3 = B, qué decide la semilla:**
  - **(a) El remate, solo con `remate: "azar"`.** Elige uno de su grupo por modo, con la misma probabilidad para cada uno: si el modo tiene la tercera mayor (Jónico, Lidio, Mixolidio), entre {rico, simple, ii–V}; si la tiene menor (Eólico, Dórico, Frigio), entre {rico, deslizado, ii–V}. El grupo sale de la hoja de remates (§14); el deslizado en mayor se cayó por oído. Número: `hash32(s, 0, 1, 0) % 3`. En Dórico, Frigio y Lidio, con la tónica como destino, el deslizado y el ii–V ya daban el acorde modal (P5), así que ahí el azar casi no se oye. En Locrio no hay remate.
  - **(b) Dónde caen los adornos.** En cada vuelta, la semilla baraja el orden en que se recorren los acordes (clave `hash32(s, vuelta, 2, i)` por acorde y orden por clave). Cada regla (sus4, menor prestado, disminuido, acorde de paso, ♭VI) se recorre en ese orden en lugar de ir de izquierda a derecha. Como el presupuesto se agota, cambia qué acordes reciben adorno, y como la clave depende de la vuelta, también cambia de una vuelta a otra. Antes, las vueltas 2…n eran idénticas salvo el remate.
  - **Lo que la semilla no toca:** la prioridad entre reglas (remate → sus4 → menor prestado → disminuido → acorde de paso → ♭VI), el presupuesto, las condiciones de cada regla, el color (séptimas, novenas, cadena de dominantes, ♭VI7 en menor), la vuelta 1 de fábrica (`desdeVuelta`), «≤2 acordes distintos → 1 cambio» y «ningún acorde escrito desaparece». No hay modulaciones. **Todo lo que suena con semilla también lo puede dar el orden fijo de alguna otra progresión: la semilla no inventa recursos.**
  - (c) Qué adorno se elige, con pesos (P6): fuera de esta sesión.
- **De fábrica:** el remate del N3 pasa de «rico» a «azar» (decisión de la hoja de remates). Sin semilla da lo mismo que la v4. El N2 se queda en «simple», así que en el N2 la semilla solo mueve los adornos (b).
- **D4 = C, guardar una versión:** es una entrada del catálogo (el db `catalogo`; sin db, el navegador) con `semilla` y `nivel` además del texto y las perillas. La cadena copiada también lleva la semilla en la cabecera.
- **Dudas musicales:** ninguna nueva. (a) usa grupos ya decididos por oído y (b) solo reparte recursos ya aprobados, así que no hizo falta consultar a Music Lab. Lo que queda abierto lo resuelve la prueba de oído corta (paso 5): si 3 semillas de la misma carta suenan a la misma carta.

# Dossier — Claridad y UI: lecciones de Slay the Spire y similares para ALWTTT

**Tipo:** referencia (recopilación, no autoridad). Si algo de aquí cambia el plan, el cambio se aplica en
su doc gobernado (`S5_DemoCutClose_Sub_Roadmap.md`, `Design_Vibe_Telegraph_v0_1.md`, el SSoT que toque).
**Creado:** 2026-10-27. **Para qué:** tener en un solo sitio todo lo que el proyecto sabe sobre legibilidad
antes de abrir **LEGIB-1** (M1) y la prueba de comprensión (M6) de la demo del 11-dic.
**Versión para el equipo (página privada, compartir desde su menú Share):**
https://claude.ai/artifact/Swm8dcLjPEJpYEWL2ZpGcw. Si este archivo cambia, actualizar también la página.
**Criterio de la demo (D-ART-2):** que se entienda sin explicarla: cómo se juega, cómo se gana, cómo se
pierde. Referencia nombrada en la reunión: *Slay the Spire*, lectura inmediata de ATK/DEF.

**Respaldo de cada afirmación:**
- **[visto]**: transcripción del vídeo leída entera (fichas en
  `D:\Projects\marcadores-chrome\profundo\fichas_videos.md`).
- **[estudio]**: estudio BALANCE-XREF de 2026-07-16 (fuentes de desarrolladores; ver
  `planning/Design_Game_And_Card_Maxims_v0_1.md` §7).
- **[repo]**: decidido, construido u observado en ALWTTT (se cita el lote).
- **[nota]**: idea de Claudio en Keep (`reference/ideas/2026-10-24_keep_ideas.md`).

---

## 1. Las lecciones externas

### 1.1 Slay the Spire: la interfaz que casi mata el juego (Ars Technica, marcador 895) [visto]

- La primera versión mostraba la siguiente jugada del enemigo **como texto, y solo al hacer clic** sobre él.
- Falló por tres razones: no servía para streaming, cargaba demasiado texto y era incómoda con varios
  enemigos.
- La solución fue el **intent**: un icono siempre visible sobre cada enemigo (ataque, defensa, mixto,
  debuff, buff, desconocido). La espada **crece con el daño**: escala visual de amenaza, sin leer números.
- En las primeras pruebas locales «nadie entendía qué pasaba»; lo arreglaron mirando a gente jugar.
- **Regla:** dar la información justa para decidir (¿ataco o me protejo?) sin abrumar con números.

### 1.2 Slay the Spire: legibilidad sin explicar (GDC 2019, Casey Yano, marcador 385) [visto]

- **Palabras clave con tooltip** al pasar el cursor.
- **Borde azul** en las cartas que se pueden jugar; **texto rojo** si falta energía.
- **Aviso en el momento** en que el jugador intenta algo imposible: la ayuda llega con el error, no antes.
- Eventos cuyas opciones **explican su resultado**.
- **Probar en persona** y mirar dónde se atasca la gente; después, un bot de feedback en Discord
  (18.000 comentarios).

### 1.3 Diseño lenticular (GMTK sobre Slay the Spire, marcador 803) [visto]

- Mark Rosewater (Magic): cartas **simples en la superficie**, con profundidad que solo ve el experto.
- Pocas cartas que se combinan dan muchas estrategias (10 cartas → unas 100).
- Para la claridad: los **ganchos de sinergia** tienen que verse a primera vista (palabras clave, iconos).

### 1.4 La cadena causal a la vista (Slay the Spire y Monster Train) [estudio + repo]

- El jugador ve la cadena **carta → número → enemigo**: cuánto hace una acción, ya modificada por buffs y
  debuffs, **antes o mientras** ocurre. Es el problema que abrió `Design_Vibe_Telegraph_v0_1.md` §1:
  «los testers describen el Vibe como nebuloso».
- **Monster Train:** tooltips **apilados** al pasar por una carta (una ficha por cada estado o palabra
  clave). Se copió en M1.3c.
- **Mazo inicial con copias** (StS: 5 Strike / 4 Defend): la duplicación también es una ayuda de
  aprendizaje, porque el turno 1 se parece al turno 4 (máxima N7).
- **Neow:** diálogo que se escribe letra a letra; se tomó para el tutorial (`Design_Tutorial_System`).

### 1.5 UX de juegos de ritmo (Design Doc, marcador 902) [visto]

- En un juego de ritmo la interfaz es el juego.
- **Calibrar el retardo sin que el jugador lo sepa**, con una ronda de práctica sin puntuar fundida con
  el tutorial (Rhythm Heaven).
- Leer a primera vista: **color o forma alineados con el pulso**; no se puede leer lo que está tapado.
- En los híbridos, **toda la escena se anima al compás** (Hi-Fi Rush) o el ritmo parece un añadido
  (No Straight Roads).
- Una barra de ayuda a la sincronía **que se puede apagar**.

### 1.6 Portal: comprobar la comprensión (GDC 2008, marcador 1230) [visto]

- Probar con jugadores desde la primera semana y, al final, **pedirles que cuenten la historia de vuelta**.
  Lo que no saben contar se recorta.
- Enseñar en momentos sin estrés; dejar el clímax simple pero con presión.
- Es el método directo para M6 («cada persona sabe decir cómo se gana y cómo se pierde»).

### 1.7 Midnight Suns [nota #130]

- «Muy complicado, evitar explicaciones complejas, pocas palabras, mucho tooltip.»
- En la misma nota: «Mostrar rango posible de ENG, ej. 10 to 30, −20 to 20».

---

## 2. Qué hizo ya ALWTTT con cada lección

| Lección | En ALWTTT | Lote |
| --- | --- | --- |
| Intent del enemigo | El público telegrafía su intención antes de actuar (`SSoT_Audience_and_Reactions` §7). **Ojo:** el texto flotante muestra nombres provisionales de habilidad (`CURRENT_STATE` §4, Kid / Cool Dude). | existente |
| Cadena carta → número → enemigo | Número grande del Vibe (C1), eficacia por miembro (C2: ¡Súper! / Normal / Resiste / Inmune) y número proyectado por miembro (C3) | S5a |
| «El número que se ve es el que pasa» | La proyección pasa por el mismo cálculo que el pago real (`PreviewIncomingVibe`); segmento «fantasma» en la barra que el pago atraviesa | BIGNUM-1 |
| Escala visual de amenaza (espada que crece) | Tamaño y color del número grande relativos al público, no umbrales fijos | BIGNUM-1 (D-BN-4) |
| Borde azul / texto rojo | Capa roja en cartas no jugables, con **una sola fuente de verdad** de jugabilidad | CARD-UX-1 |
| Tooltips de palabras clave | Conceptos marcados en el texto con tooltip al pasar; texto mecánico −30 % | TXT-2 |
| Tooltips apilados (Monster Train) | Pila de hover por carta: palabras clave primero, estados después | M1.3c |
| Iconos en lugar de texto | Glifos en la cara de la carta: «notación = texto, categoría = símbolo» | CARD-FACE-1/2/3 |
| Escena animada al compás | Latido del número grande en el grid MIDI | BIGNUM-1, BEAT-1 |
| Legible sin color | Pips de loop con aviso de loop final (rombo + rojo) | HUD-COMP-1 |
| Causa → efecto visible | Jugada de carta en cuatro tiempos (intérprete, pausa, efecto en el objetivo) | WINK-1, JUICE-PW |
| Menos ruido | Ocultar el coste cuando es 0; tooltip en los pips de economía | DEMO-FIXES-A |

---

## 3. Lecciones aprendidas en casa (observación, no teoría)

Son los errores ya cometidos y registrados. Valen tanto como las externas, porque salen de mirar el
juego propio.

1. **Antes de construir, comprobar si ya existe y está apagado.** El hueco de legibilidad del Vibe era en
   parte de **visibilidad, no de ausencia**: las superficies de S5a existían, pero estaban desactivadas
   por configuración en el asset de la demo (`CURRENT_STATE` §4, BIGNUM-1).
2. **Un tooltip que lo cuenta todo no se lee.** Meter estado + intención + gustos + Vibe en el mismo
   tooltip del personaje se construyó y **se revirtió** (D-BN-9=A, `Design_Vibe_Telegraph` §10.3). Lo que
   importaba quedaba abajo. Solución: un hover propio para la barra. **No volver a proponerlo.**
3. **Las líneas que no cambian nada sobran.** El desglose mostraba `× 1,00 tastes`, `+ 0 SFX`… Con cuatro
   líneas inertes de seis, las dos que mueven el número quedaban enterradas. Regla: ocultar un paso
   cuando su salida es igual a su entrada; la primera y la última línea se muestran siempre (F-TIP-6,
   TEXT-VIBE-1). «La pedagogía se paga en cada hover para siempre y se cobra una vez.»
4. **El fantasma necesita color propio.** El mismo color de la barra con menos opacidad no se distingue
   (F-BN-6).
5. **Una barra llena se oculta, salvo que prediga algo.** La predicción es información (D-BN-17=A).
6. **Mezclar idiomas confunde.** Tooltips en inglés dentro del tutorial español (T-TAG-1 → TXT-3, TEXT-5,
   TEXT-6).
7. **Quitar poderes confunde** [nota #113, *Sifu*]. Por eso M4 pide una Reina Gusano «sin quitar
   poderes».
8. **Riesgos todavía sin medir** (lentes de S5i): un mazo inicial con 15 cartas distintas puede costar de
   entender (L1; arreglo previsto: más copias de las cartas más legibles, N7); un finisher que solo se
   paga con juego perfecto se lee como trampa (L3, Psychic Waves).

---

## 4. Lo que falta para la demo

| # | Falta | Lección que aplica |
| --- | --- | --- |
| **M1 / LEGIB-1** | Al pasar el ratón por una carta, ver su efecto en el número grande: «este loop: Hype +X → Big Num Y→Z» (D-DIAG-2 = A + C; si la previsualización salta, `ComputeHypeDelta` pasa de escalón a lineal) | 1.1, 1.4, nota #130 (rango) |
| **M3 / COH-1** | La cohesión como única derrota, **visible en el HUD** | 1.1: la amenaza siempre visible |
| **M4** | Reina Gusano con 1–2 mecánicas legibles | 1.1 (intents legibles), §3.7 |
| **M6** | ≥3 personas externas sin explicación | 1.2 (mirar dónde se atascan), 1.6 (que lo cuenten de vuelta) |

**El hueco de la reunión de arte:** «cuesta proyectar el daño que haces **y el que recibes**». Lo que
haces (Vibe) ya tiene telegraph y M1 lo completa. Lo que **recibes** (estrés y cohesión por las
intenciones del público) es la mitad DEF del modelo ATK/DEF de Slay the Spire. Ningún documento revisado
para este dossier muestra una proyección equivalente para ese lado. **Comprobarlo en el build antes de
LEGIB-1**: si falta, es candidato natural a entrar en M1 o en COH-1.

**Preguntas para abrir LEGIB-1** (salen de §1 y §3):
1. ¿Qué ve el jugador **antes** de comprometer una carta, y cómo se dice sin texto?
2. ¿Número exacto o **rango** (nota #130)? El exacto exige que lo mostrado sea lo que pasa (principio de
   D-DIAG-2); el rango acepta incertidumbre musical.
3. ¿La intención del público se lee como **icono con escala** o todavía como texto? (Además, limpiar los
   nombres provisionales.)
4. ¿Qué líneas del desglose sobran en el primer gig? (regla de §3.3).

---

## 5. Fuentes guardadas y sin leer

Están en los marcadores con la carpeta *Card Game Design* o *UI*; ninguna se ha visto. Por orden de
utilidad probable para M1:

| i | Fuente | Por qué podría servir |
| --- | --- | --- |
| 896 | Adam Millard (*The Architect of Games*), *What Makes a Great Deckbuilder?* | Panorama del género |
| 899 | GDC, *Hearthstone: 10 Bits of Design Wisdom* | Hearthstone es referente de claridad en cartas |
| 898 | Rempton Games, *How To Solve the Queen's Problem* (TCG Design Academy) | Diseño de cartas; tema no comprobado |
| 1192, 1193 | Game Dev Guide y Comp-3, tooltips en Unity | Técnica; TIP-1 ya resolvió el pipeline |
| 917 (= 1190) | Game Dev Guide, *Making UI That Looks Good In Unity* | Acabado visual, no claridad |
| 805 | Arefnue, *Nue Deck* (plantilla de deckbuilder en itch.io) | Referencia de código, no de diseño |

También citada en el estudio, pero centrada en balance y métricas, no en UI: GDC 2019 *Slay the Spire:
Metrics Driven Design and Balance* (Giovannetti).

**Propuesta:** antes de LEGIB-1, ver 899 y 896 con la misma pregunta de §4.1 y anotar aquí lo que se
confirme. Si se ven, actualizar este dossier.

## 6. Documentos de origen

- `reference/ideas/2026-10-27_marcadores_chrome.md` (con el addendum de transcripciones)
- `reference/ideas/2026-10-24_keep_ideas.md` §1
- `reference/reuniones/2026-10-23_arte.md`
- `planning/Design_Vibe_Telegraph_v0_1.md` (§1–§10)
- `planning/Design_Game_And_Card_Maxims_v0_1.md`
- `planning/active/S5_DemoCutClose_Sub_Roadmap.md` (REPLAN-ART, REPLAN-DIAG, CARD-UX-1, TXT-2…CARD-FACE-3)
- `planning/active/Roadmap_ALWTTT.md` (M1.3c)
- `systems/SSoT_Audience_and_Reactions.md` §7
- `CURRENT_STATE.md` §1 y §4

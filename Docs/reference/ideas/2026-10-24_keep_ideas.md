# Ideas de ALWTTT rescatadas de Google Keep — 2026-10-24

**Tipo:** referencia (registro, no autoridad). Si algo de aquí cambia el plan, el cambio se aplica en su
doc gobernado (`S5_DemoCutClose_Sub_Roadmap.md`, `CURRENT_STATE.md`, SSoT correspondiente); este archivo
solo lo registra.
**Fuente:** export de Google Keep de Claudio (2026-10-24), notas #81, #82, #89, #102, #106, #113, #123,
#125, #130, #131, #132 y #133, más las relacionadas #87 y #98. Índice completo en
`D:\Projects\_bandeja\keep-2026-10-24\clasificacion.md` (fuera del repo).
**Regla del proyecto, en palabras de Claudio (#132):** «Que las ideas sean tuyas, no de la IA». Las notas
se citan tal cual (erratas del móvil corregidas solo cuando impiden leer). El agente solo las ordena y
señala con qué choca o encaja cada una; no añade ideas.

---

## 0. Decisión tomada al revisar las notas (2026-10-24)

- **La demo empieza en el mapa, con clic en el primer nodo.** Resuelve el choque entre la nota #132
  («Start demo with Gig, not map», 2025) y REPLAN-ART D1 = C (mapa lineal). Registrada en
  `planning/active/S5_DemoCutClose_Sub_Roadmap.md` (REPLAN-ART).

## 1. Insumos para la demo del 11-dic (objetivo: que se entienda sin explicarla)

Solo insumos para las semanas que tocan; no cambian la secuencia de REPLAN-ART.

| Nota | Idea | Problema de la reunión que toca | Semana |
| --- | --- | --- | --- |
| #130 (2024) | «Mostrar rango posible de ENG ej 10 to 30, -20 to 20» | «Cuesta proyectar el daño que haces y el que recibes» (lectura ATK/DEF) | 2026-11-02 |
| #130 | *Midnight Suns*: «Muy complicado, evitar explicaciones complejas, pocas palabras, mucho tooltip» | Legibilidad general | 2026-11-02 |
| #123 (sep 2026) | Pelotitas de vibe que vuelan al número; «al menos una carta de coste 0, 1 y 3 […] que el efecto de las cartas con coste elevado sea claramente mejor o distintivo»; «Probar mucho y perfeccionar tutorial» | Que se lea cómo se gana; revisión de tutorial y mazo inicial | 2026-11-02 |
| #132 (2025) | «Comenzar con 1 o 2 músicos, ir armando la banda de a poco» | Banda de 2 | 2026-11-02 |
| #125 | Finishers con mecánica única por músico: C2 «apretar on time para fills»; Zig «mover la palanca o triggers para ajustar vibrato»; Sibi «revela preferencias del público»; **Conito «acumula cargas de overdrive, con 3 desbloquea un solo de guitarra»** | Roles por músico: «Conito = ¿?» | 2026-11-02 |
| #125 | «Gotita anime de "la cagué"», expresiones/emotes del músico | Legibilidad del estado del músico (encaja con stress visible) | 2026-11-02 |
| #113 (*Sifu lessons*) | «Jefes con 2 fases diferentes»; «Tall enemies que bloquean a los de atrás»; jefe doble que «se sube uno sobre el otro y le hace "buffs" a todos los de detrás»; «Confunde un poco el perder los poderes todo el tiempo» | Kit de la Reina Gusano y enemigos básicos | 2026-11-09 / 16 |
| #132 | «Gusano psíquico tiene una carta que hace olvidar la canción anterior al público. Mejorada los hace olvidar TODAS las anteriores» | Kit de la Reina Gusano / rol CONTROL de Sibi | 2026-11-09 / 16 |
| #123 | «Midsong (between loops) enemy actions, infraestructura y al menos un "gritar boooring" que suba stress a todos o a uno» | Cómo se pierde (amenaza legible) | 2026-11-09 / 16 |

⚠ La nota #113 dice que perder poderes «confunde»: si la Reina o los enemigos nuevos quitan efectos,
choca con el objetivo de comprensión.

## 2. Ya hechas o cubiertas por docs gobernados

- **Walking bass** (#125): existe como `ImprovisedWalk` en MidiGenPlay (`Design_Composition_Variations`).
- **Finishers en el mazo inicial** (#125): CR-6b (uno por músico).
- **WOW!/MEH/BORING flotantes** (#123): cubierto en el SSoT de Audience.
- **Quitar action icons, explicarlo en tooltip** (#123): rediseño de textos, iconos y tooltips (commit
  `77af577`).
- **Cohesión, stress, relax, recruit, críticos, soundfont**: hay cobertura parcial en los docs; ver
  cada SSoT antes de abrir nada.

## 3. Después de la demo (Phase C)

- **Interacciones musicales** (#123, encaja con REPLAN-ART D3 = B): «Carta en que toquen, por ejemplo,
  conito guitarra y el cantante a la vez lo mismo, o con armonías tipo metal. También puede ser un ask
  and response entre 2 músicos […] Puede ser un duelo entre batería y bajo slap».
- **Cartas de composición con etiqueta de ánimo** (#123): «(I), HAPPY […] happy (major modes), dark
  (minor modes), funky (dorian). Se asignan manualmente a las cartas». Cartas nivel banda «tipo pokeball»
  que guardan la parte para después.
- **Malas notas y quedarse en blanco** (#82, #125): «Musician se queda en blanco y no suena su parte»;
  «notas incorrectas con un GetIncorrectNotes(Tonality) method»; «implementar %malas notas». Sin
  cobertura en los docs.
- **Material musical** (#123, #106, #133): grabar con Gastón progresiones y solos en 3 niveles de
  complejidad para probar la importación; progresiones «tipo bill wurtz […] quirky o sci-fi/espacial.
  NIN, Muse, Justice, Daft Punk»; guías de armonía clásica y jazz.
- **Voz** (#132, #133, #130): «Make soundfont with my voice (clara, Hernán)»; «una o dos octavas de voz
  para generador»; vocoder o voz estilo Sims / Banjo-Kazooie. Encaja con los convencidos que cantan
  (D3 = B).
- **Acciones de escenario** (#89): agradecer, presentar la banda y a cada músico, historia de la
  canción, «Let the people sing it», «Make em Cry», lanzar púas/baquetas, «Slow down for a second»,
  «Sit down and talk».
- **Personajes del público** (#89, #132): alien con afro en primera fila que tapa a los demás (encaja
  con *tall enemies* de #113); mujer borracha muy fanática; viejo muy viejo bailando; críticos «de
  encubierto como enemigos comunes»; músico que se pelea, se va y vuelve como enemigo en el público.
- **Audiencia** (#130): umbrales de reacción (0; ±0.5); scouts que suben la importancia del gig;
  milestones de engagement visibles; aplausos y abucheos con screen shake.
- **Mapa y nodos** (#132): Compose, Gig, Band Talk, Relax, Random Encounter, Recruit, Boss Gig/Space
  Relay, Gear Traders; «al menos 2 bifurcaciones por star sector»; cohesión en el mapa; stress de todos
  en la UI; condiciones por tipo de nodo; `RandomEventCanvas`; 3 cartas por músico (common, rare,
  legendary).
- **Eventos y contenido** (#132): caja musical flotando en el espacio; carta de «tocar canción de
  nuevo» (los Mox); Space Laser Show como carta SFX definitiva; camionero espacial con armónica; Bob
  Marley en Sevilla / *Three Little Birds*; cárcel estilo Folsom Prison.
- **Sistemas de personaje** (#130): solo «Heartbroken» con aflicción; letras por LLM con tipos de rima
  (décima, aabb, abab); probabilidad de éxito por stats «lo que ocurre en XCOM con los disparos»;
  reputación como quirks/artifacts (*Monster Train*); *hangout* y habilidades en pareja (*Midnight
  Suns*). «Daily activity: Go Out» (#81): bares y conciertos para reclutar músicos conversando.
- **Comunidad** (#130): que la gente suba sus propios pools de pistas y voces.

## 4. Mundo e historia

Tres fuentes con la misma tesis, el arte frente a la extracción burocrática o artificial:

- **#102 (2025):** «El objetivo final final es derrocar al imperio artificial (IA) o (AE). Para lograrlo
  una de las bandas más famosas tiene que lograr tocar frente a los altos mandos y convencerlos con su
  música». Las bandas se separan sin morir; la «Galacene» y la «resistencia»; otros músicos juegan sus
  propias partidas simuladas; progresión multirun («music scene simulator»).
- **#132 (2025):** «A toast for the end of the age of creativity»; «Personaje IA pero full emotion y 0
  técnica. Lo programaron así para sabotearlo y se volvió músico»; «Star Sectors con humanos están en
  cuarentena»; «Tú eres la Nave espacial o la IA de la nave que se reinició y tiene "amnesia"»; batería
  pulpo con palancas dentro del traje.
- **Reunión de arte (2026-10-23):** el Imperio Nudo, burócratas que «no saben de arte, solo saben
  contratar» y «extraen energía».
- **Blueprints de historia de banda** (#82): Los Jaivas (exilio, muchos países, regreso); Nirvana
  (fama desde el segundo disco con un productor famoso, el grunge como movimiento juvenil), «Podría ser
  como el quest final del juego».

## 5. Notas antiguas y relacionadas

- **#133 (2021–2022):** origen del proyecto: «Gigs con cartas / Canciones son cartas con stats / Gig
  actions asociados a members».
- **#131 (2022), *ALWTTT demake*:** «Jukebox AI or another generative model with API».
- **#87 (2022):** juego de acción en tercera persona con un guitarrista que lanza el ampli para escalar
  y atacar. Otro juego; solo comparte tema.
- **#98 (2022):** banda de orcos de metal progresivo que detecta tus gruñidos; «Asociar a proyecto en
  que personajes tocan juntos y tienen sus sonidos asociados».

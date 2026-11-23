# Ziggy: lo que cubren sus cartas frente a lo que pueden el motor y el taller (2026-11-23)

Planning, no autoridad. Pedido de Claudio en MUSICA-1c, a imagen del análisis del bajo (cartas, paletas y niveles).
Fuentes: inventario de assets y código (ALWTTT, MidiGenPlay) y del taller de melodías (`Docs/reference/taller-melodia/`).

## Las cartas de Ziggy hoy (verificado en assets)

| Carta | Mazo | Qué suena |
| --- | --- | --- |
| Rise Up | inicial ×1 | Melodía en modo figura: Ola subiendo, semilla 7, cambia por canción. Forma libre. |
| Showtime | inicial ×2 | Modo figura: Sequence en arco, forma AAB (última prueba del Banco), final culminante. |
| Double Harmony | desbloqueada, no inicial | Segunda línea MIDI: la nota del acorde más cercana (única relación implementada). |
| Gran Final | inicial ×1 | Finisher: el solo sustituye a la melodía; Vibe +4. |
| WINK! | inicial ×1 | Acción: Captivated +2. |

- Ninguna carta de melodía es una paleta: `MelodyCardConfigSO` no tiene variantes ni niveles, al contrario que el bajo.
- Voz: siempre `VoiceProfile_Zig_V8` (el del prefab). El campo `voicePalette` de las cartas existe y el Banco y el gig lo leen, pero ninguna carta lo usa. Hay 11 estilos V8 creados y 2 paletas sin usar.

## Lo que se puede expresar

| Dimensión | En las cartas hoy | Motor del juego (MGP + voz V8) | Solo en el taller |
| --- | --- | --- | --- |
| Figura | Ola, Secuencia | 7 figuras; figura al azar por semilla | — |
| Dirección | Sube, Arco | Sube, Baja, Arco, Valle; al tope, octava | — |
| Forma de frase | Libre, AAB | Libre, AA′ (pregunta y respuesta), AAB, final culminante | — |
| Ritmo | Celdas | Celdas, ritmo Rise Up, densidad, síncopa, swing | — |
| Escalas | 7 modos | 7 modos (el planificador admite cualquiera; el enum no) | 9 más: pentatónicas, blues, armónica, frigio dominante, hirajoshi… |
| Melodías fijas | Rise Up (patrón) | Patrones de melodía | Seikilos, himno hurrita, Bosco |
| Segunda voz | MIDI, acorde más cercano | Igual (las otras relaciones caen en esa) | Voz cantada 2, intervalos fijos, eco, coro |
| Estilo de voz | Zig V8 | 12 estilos: Zig, Tierno, Pop, Punk, Death metal, Ópera, Crooner, Falsete, Robot, Borracho, Gruñido (+ Canto llano, portable sin asset) | Susurro, Canto de garganta, Nasal, Yodel (perillas nuevas, VOZ-2b) |
| Sílabas | zigués | zigués, lista con pesos, texto | solfeo, scat, konnakol, boca cerrada, vocal sola |
| Niveles / paleta | no | no (el bajo sí) | — |

## Decisiones abiertas (D10–D12, en la respuesta del 23-nov)

- **D10 = A, ampliada:** cada carta de melodía con su paleta de voz, y entre todas cubren todos los estilos V8.
  Showtime: Pop · Crooner · Ópera. Rise Up: Falsete · Zig V8 · Robot. Blues: Borracho · Gruñido · Crooner.
  Balada: Tierno · Canto llano (nuevo, del taller). Grito: Punk rock · Death metal.
- **D11 = B:** sin paleta de melodía antes de la demo. **Prioritario después de la demo:** la paleta con niveles (A),
  como la del bajo (MGP-BASS-PALETTE-1).
- **D12 = C:** cartas nuevas de recompensa: Blues (AAB, figura al azar, swing), Balada (AA′, Ola en arco, lenta),
  Grito (AA′, Saltos hacia arriba, densa). Showtime pasa a AA′ con figura al azar y final culminante.
- Herramienta: menú *MUSICA-1c ▸ Ziggy: voces y cartas nuevas* (`DemoZiggy1c.cs`). A/B pendiente.
- **Escucha 1 (23-nov):** todo muy grave → pesos de octava por paleta (`octaveWeights`, menú *Ziggy: octavas más agudas*).
  Notas «fuera del modo»: no lo eran; chocaban con el acorde prestado (♭III), que la melodía leía como diatónico.
- **D13 = A (blues):** `blueNotes` en MGP: tercera blue (♭3 ligada que sube a la 3.ª), ♭7 salvo sobre V, ♭5 de paso.
  Fuentes Music Lab: progression-blues-menor §3a, Goodbye Pork Pie Hat §2, Thrill §2/§7, SIBLY §7. No están en
  Music Lab: reglas de detrás del tiempo, frase descendente ni licks de cierre.
- **D14 = A (acordes prestados):** la melodía lee el acorde real (alteración y calidad, como el bajo y los acordes, que
  ya lo hacían). Nunca a medio tono de un tono del acorde cromático; el tiempo 1 muestra su nota de color.
- **D15 = las dos:** `holds` (sostener) y `slides` (subir ligado); CC68 en el MIDI marca las notas ligadas y la voz V8
  las canta en la misma sílaba, deslizando.
- **Escucha 2 (23-nov):** sin choques ya. Pedidos: registro más alto, final que resuelva, Rise Up que suba siempre.
  Hecho: tesitura por estilo (`tessituraMin/Max`, mediana cantada), finales de frase en nota del acorde y el último en la
  tónica si el acorde la tiene (MGP-MEL-RESOLVE-1), Rise Up con `rise = 7` (MGP-MEL-RISE-1).
- **Robot fuera** de las paletas de cartas (no encaja con Zig). **Borracho fuera** de Blues: Claudio lo quiere para un
  **estado** («Borracho», «Ido», «Desorientado») que cambie la voz de Zig a ese estilo. Idea para después de la demo
  (estado → override de la paleta de voz del cantante).
- **Escucha 3 (23-nov): PASS.** Registro bien en Showtime y Grito; finales que resuelven; Blues muy exagerado al
  final pero memorable (se queda así); Rise Up frente a Balada, sin correcciones.
- **Idea de Claudio: Grito «chug»** (Phil Anselmo, Maynard James Keenan, Mike Patton): menos melódica, varias notas
  seguidas en la misma altura, como el palm mute de la guitarra, más «cavernícola». Music Lab solo tiene el lado de la
  guitarra (palm mute y riffs tribales de Sepultura en `Repertorio/2026-11-19-guitarra-la-rabia-metal.md`; el riff
  `V–♭VII` en `Repertorio/2026-11-18-desde-lastfm.md` §5); no hay análisis de esas voces. Rasgos propuestos [I]:
  nota pedal repetida (tónica o 5.ª), celdas rítmicas de corcheas y semicorcheas con silencios, notas cortas, rango
  estrecho, vecinas de color ♭2 y ♭5 al final de la celda, acentos con la batería. Pendiente de decidir (D16).
- **D16 = A: hecho (23-nov).** Figura `Riff` en MGP (MGP-MEL-RIFF-1, sal "riff", fuera de `randomFigure`): la
  fundamental del acorde como pedal grave en corcheas con huecos (todo en la rejilla de corchea), notas cortas, ♭2/♭3/♭5 (o giros
  4→♭5, 5→♭5, ♭3→♭2) en tiempos débiles al final de cada compás, AA′ con pregunta en la 5.ª y respuesta en la
  fundamental; la última, a la tónica. Las notas de color no pasan por el ajuste a acordes (como las blue).
  Menú *MUSICA-1c ▸ Ziggy: Grito chug (Riff)*. Escucha 4: con semicorcheas la voz se iba del pulso (una
  semicorchea cantada a 130 BPM se emborrona tras su consonante); con todo en corcheas, **PASS**.
- Para después (VOZ-2b): Susurro, Canto de garganta, Nasal y Yodel; sílabas latinas del Canto llano.

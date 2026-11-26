# Niveles armónicos de pista y cobertura de estilos (2026-11-02)

Estado: semilla. Viene de la [prueba de escucha v1](../prueba-escucha/resultados/2026-11-02_analisis.md).

## Lo que dijo Claudio

> «Algo que sí me di cuenta es que realmente necesitamos variación entre loops, me gustan las progresiones que avanzan o al menos tienen más de 1 loop de 4 acordes. Lo que no se es cómo integrar ese concepto en el juego. Hace tiempo tengo pensado lo de "subir el nivel" de las pistas si se vuelve a jugar la misma carta sobre una que ya está funcionando, y tal vez podría ser algo así, al jugar la misma carta sobre una pista ya creada, se genera una "segunda vuelta" con variaciones, o se agregan acordes de paso, o se cambia la chord quality a algo distinto. Mi idea original eran 3 niveles: simple, con adornos (ii-V-i y otros recursos así), spicy (algo más loco, jazz, estilo Bill Wurtz).»

> «Creo que le vendría bien al juego no irse tan a la segura con las progresiones, además que podemos aprovechar el sistema procedural para realmente adaptar todas las pistas a la progresión, sin importar lo extraña que sea. Eso le daría carácter a las canciones, y una sensación de identidad. Sería bueno inspirarse en el jazz, Stevie Wonder, jazz fusión japonés, Bill Wurtz, incluso el grunge (Soundgarden, Alice In Chains, Nirvana) y el metal (Metallica, Megadeth) para cubrir más estilos. También bossa nova, etc.»

> «Esto de probar en páginas fuera del juego es ORO […] Una segunda versión que se me ocurre es una que se pueda ingresar el string de input en la misma página y allí se cree la progresión escuchable. También sería bueno poder escoger la root y el BPM. Creo que armar un catálogo de progresiones online ayudaría mucho.»

## Lo que ya existe

- **Nivel de pista:** la tira de composición ya tiene su UI y el floater de subida de nivel (`Design_Sensory_Contract_v0_1.md`), pero falta el campo del modelo (R7, `CURRENT_STATE.md`). La mecánica no existe.
- **Tres niveles de complejidad** ya aparecían en Keep (#123: «grabar con Gastón progresiones y solos en 3 niveles», `2026-10-24_keep_ideas.md`).

## 💡 Propuesta del agente

**El nivel de pista como transformación de la progresión, no como otra progresión.** Cada carta tiene una progresión base, y el nivel le aplica reglas de rearmonización deterministas:

| Nivel | Qué hace | Recursos |
| --- | --- | --- |
| 1 · Simple | La progresión tal como está escrita. | Triadas, ritmo armónico del asset. |
| 2 · Adornos | Segunda vuelta distinta y acordes de paso. | Séptimas y novenas; dominante secundaria o ii–V hacia el acorde siguiente; turnaround en el último compás; sus4 → resolución; aproximación cromática. |
| 3 · Spicy | Lo inesperado, sin perder la casa. | Sustitución tritonal, backdoor (iv7–♭VII7), mediantes cromáticas, intercambio modal, cadencia rota, maj7♯11, cambio de tono de medio tono en la última vuelta. |

Ventajas:
1. La biblioteca crece de forma multiplicativa (cada progresión × 3 niveles).
2. La variación entre vueltas sale de la mecánica.
3. El resto de pistas se adapta solo, porque los compositores de MidiGenPlay leen los `ChordEvent`.

**Los estilos como gramática, no como lista.** Cada estilo es una ficha con:
1. Progresiones firma con fuente.
2. Vocabulario de calidades.
3. Ritmo armónico.
4. Las transformaciones típicas del estilo (sus reglas de nivel 2 y 3).
5. Lo que el alfabeto de MidiGenPlay no puede expresar todavía (acordes de quinta para grunge y metal, acordes con bajo distinto para city pop y Stevie Wonder, 11.ª y 13.ª y alterados para jazz).

**Herramientas:** un taller web donde se escribe una progresión y se oye al momento (con tono, BPM y nivel 1/2/3), que copia el bloque de Import de MidiGenPlay, y un catálogo online etiquetado.

## Enlaces posteriores

- **2026-11-26 · Mediantes cromáticas, «wormhole» y P/L/R:** dos videos de Jaron Lopez y la teoría neorriemanniana dan una definición precisa del nivel 3 («dos pasos en la red de Riemann») y reglas de voicing para que el salto suene a banda y no a robot. Nota en MidiGenPlay: `MidiGenPlay/MidiGenPlay/Documentation~/research/2026-11-26-wormhole-y-neorriemanniana.md` (§5, §6 y §9). Incluye la sincronización C2 ↔ Sibi.

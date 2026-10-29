# Ideas de ALWTTT desde el historial de ChatGPT — 2026-10-29

**Tipo:** referencia (registro, no autoridad). Si algo de aquí cambia el plan, el cambio se aplica en su doc gobernado (`S5_DemoCutClose_Sub_Roadmap.md`, `CURRENT_STATE.md`, SSoT correspondiente); este archivo solo lo registra.
**Fuente:** historial de ChatGPT de Claudio (3.431 conversaciones, dic 2022 – oct 2026; `D:\Projects\chats-chatgpt`, fuera del repo). El número entre paréntesis es el `i` de su `catalogo.csv`. Todo lo citado está **comprobado** en los mensajes de Claudio; lo **inferido** es lectura del agente.
**No cambia la demo del 11-dic.** Son candidatas para después, salvo que Claudio decida otra cosa. ALWTTT es el proyecto más presente del historial: 555 conversaciones etiquetadas con él.

## 1. Origen

- **2024-07-03 (493), «Band Battle Gameplay Systems»:** una banda de personas con personalidades e ideologías, instrumentos y géneros distintos; la unidad del gig es la canción y el setlist depende del tiempo, el local y el público. «I'm not interested in making a guitar hero like rhythm game». Trasfondos de músico (autodidacta, formal, callejero) que cambian stats.
- **2024-07-15 (506):** sube el GDD de «an old game idea I was working on a few years ago» (el Band Manager original, ver `2026-10-27_letterboxd_band_manager.md`) y lo retoma: cómo reflejar la personalidad y la habilidad de cada miembro al crear canciones.

## 2. El giro a componer con cartas (oct–nov 2025)

- **2025-10-20 (1966):** «una especie de crisis de decisión en cuanto al game design» con MidiGenPlay generando canciones; llega a que una canción es una lista de *song parts* y cada parte puede ser una carta.
- **2025-11-07 (2084):** rediseño de `CardData`: cartas de composición que cambian tempo, métrica o tonalidad; cartas de ritmo como un vals de 3/4.
- **2025-11-10 (2096):** demasiadas ramas a la vez; la demo era solo una muestra del sistema de composición, sin loop de juego. Dos mazos: composición y acciones inmediatas, reutilizando el combate que ya funcionaba.
- **2025-11-14 (2126):** el porqué de todo el rediseño: imagina a «un eventual compositor de música» sin conocimientos de programación creando contenido con los editores.

## 3. Hallazgo de playtest que sigue vigente

**2025-03-03 (1089):** «I've been playtesting the game with some friends and there was some confusion about the engagement bar, which can be negative or positive. It seems the bar represents, more precisely, the mood of the audience, not the engagement. There should be clear visual signs of the current "score" for the gig».

*Inferido:* es el mismo problema de legibilidad que LEGIB-1 y el dossier de claridad (`Docs/reference/Dossier_Claridad_UI_Deckbuilders.md`). Vale como evidencia de jugadores reales, no solo de diseño.

## 4. Ideas pendientes (candidatas, sin decidir)

| i | Fecha | Idea de Claudio |
| --- | --- | --- |
| 757 | 2024-10-24 | Excel de cartas con género asociado, stats con los que hacen sinergia y tipo de efecto. |
| 899 | 2024-12-28 | Géneros y subgéneros que se desbloquean como un árbol de habilidades tipo Path of Exile, con 3 a 5 cartas por subgénero. |
| 665, 737 | 2024-10 | *Stage antics* como cartas: aplaudir antes de la canción al BPM, que el público cante las armonías (Pearl Jam, «Black», Pinkpop 2000), el fuego de Gene Simmons, gestos simples como secarse el sudor. |
| 1035, 1067 | 2025-02 | Acciones entre canciones y *song modifiers* sacadas de conciertos históricos y de bandas reales, ligadas a CHARM, TECHNIQUE y EMOTION. |
| 1529 | 2025-07-23 | Lema: entre sus dos favoritos, «A Long Way to the Top — "Reach the stars. Become one."». |
| 1533 | 2025-07-24 | El imperio se llama **The Conductorate**, opuesto al arte orgánico y a favor del «AI generated art slop». «I love The Conductorate». |
| 1951 | 2025-10-16 | Un `MistakeMutator` o postprocesador: según la complejidad de la canción y las cartas, un músico comete errores audibles (notas falsas, golpes fuera de tiempo). |
| 2699 | 2026-01-13 | Hooked como veneno: la canción anterior «se le quedó pegada» al público y suma Vibe cada vez que la recuerda. *Comprobado en el repo:* `Hooked` existió como estado y se eliminó con otros legacy (`changelog-ssot`). |
| 2434 | 2025-12-17 | Géneros como tipos de Pokémon para público y bandas; él mismo concluye que «sería una pesadilla comunicarlo por la UI». |
| 3351 | 2026-04-03 | Carta para subir la canción de tono entre loops. |
| 2329 | 2025-12-10 | Silbido como instrumento de un personaje (referencia: la carrera con Koopa en Bob-omb Battlefield, *Mario 64*), con vibrato por pitch bend. |
| 3293 | 2026-03-24 | El bajo «MIDI slap» del tema de selección de *Street Fighter Alpha 2* con soundfonts. |
| 2178 | 2025-11-22 | Acertar prompts rítmicos para activar fills de batería, frases melódicas y acentos. |
| 1624 | 2025-08-06 | Paneo por posición en el escenario y un «Advance» como en *Monster Train 2*: mover al músico adelante antes de un solo. Ver la nota de MidiGenPlay. |

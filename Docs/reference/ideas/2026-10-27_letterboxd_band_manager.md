# Referencias de ALWTTT desde Letterboxd y el Band Manager original — 2026-10-27

**Tipo:** referencia (registro, no autoridad). Si algo de aquí cambia el plan, el cambio se aplica en su doc gobernado (`S5_DemoCutClose_Sub_Roadmap.md`, `CURRENT_STATE.md`, SSoT correspondiente); este archivo solo lo registra.
**Fuente:** análisis del Letterboxd de Claudio (`D:\Projects\letterboxd`, fuera del repo) y lectura del código de `D:\Projects\Band Manager (SC) - LTS` (fuera del repo, solo lectura). Ficha completa: `D:\Projects\banco-ideas-juego\proyectos\band-manager.md`.
**No cambia la demo del 11-dic.** Todo esto es para después, salvo que Claudio decida otra cosa.

## 1. El ALWTTT original

**Comprobado:** `Band Manager (SC)` tiene en su `ProjectSettings.asset` el nombre `A Long Way To The Top` (Frisson Games, v0.1). La copia LTS, en Unity 2020.3.48f1, compiló en agosto de 2023. Es un tycoon de gestión de banda por días, con estudio, payday, listas de éxitos y eventos. Dentro tiene un gig simulado.

**Inferido (de los nombres en el código, sin comparar la lógica):** el gig del original ya tenía el núcleo de los SSoT actuales:
- engagement por canción (`AudienceManager.GetEngagement`) según la interpretación, la exigencia musical, las preferencias de género y la energía esperada (SSoT_Audience_and_Reactions);
- `GigAction` con efectos en engagement y energía (SSoT_Gig_Combat_Core);
- venues por tier y capacidad;
- canciones con stems por género, BPM y progresión (SSoT_Audio, y el antecedente de MidiGenPlay).

**Para después de la demo:** comparar las fórmulas de engagement del original con las de ALWTTT25. Puede haber ideas de diseño ya probadas, como la expectativa de energía del público.

## 2. La lista Band Manager References (2018-07-19)

**Comprobado** (`D:\Projects\letterboxd\fuente\lists\band-manager-references.csv`, con notas de Claudio):
- *This Is Spinal Tap* (5), *School of Rock* (5 ♥), *Whiplash* (5)
- *Pink Floyd: The Wall* (5 ♥), *A Hard Day's Night* (♥)
- *Almost Famous* (4), *Frank* (4)
- *Amadeus*, *Yellow Submarine*, *Get Him to the Greek*, *Tenacious D*, *Ray*, *8 Mile*, *Scott Pilgrim*, *That Thing You Do!*, *Popstar*, *Departures*, *Walk the Line*, *The Doors*, *Sid and Nancy*

**Inferido:** el tono que sale de la lista es la comedia de banda (Spinal Tap, Tenacious D, Popstar) más la presión de tocar (Whiplash). Para la demo, que debe entenderse sin explicación, el humor de *Spinal Tap* es una referencia de cómo contar el fracaso de un gig sin texto.

## 3. Por ver, de memoria (confirmar los datos)

- *Anvil! The Story of Anvil* (2008, Sacha Gervasi): una banda que lleva décadas intentando convencer a un público. Es la premisa de ALWTTT en documental.
- *Stop Making Sense* (1984, Jonathan Demme): ya está en la watchlist de Claudio. El concierto como puesta en escena que crece canción a canción.

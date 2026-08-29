# Design_Composition_Variations_v0_1 — Inventario de variaciones de composición por rol y personaje

**Status:** DRAFT (planning). No gobernado. No autoriza cartas ni contratos.
**Modo:** GAME DESIGN.
**Ruta propuesta:** `planning/active/Design_Composition_Variations_v0_1.md` (confirmar contra el árbol real).
**Propósito:** inventariar los "mini-sistemas" musicales ya construidos (o afirmados) en MidiGenPlay
y ALWTTT, mapearlos a ganchos de carta por personaje, y alimentar (a) el diseño de cartas de la
campaña de contenido y (b) la sesión compañera de MidiGenPlay
(`Prompt_MGP_CardExpressivity_Companion_v0_1.md`).
**Fuentes:** snapshot MGP `MGP-20260810_*` (fechado **2026-08-10** — todo lo posterior a esa fecha
NO está aquí y requiere verificación), `MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md`,
`PinkTrombone_Voice_Levers.md`, `SSoT_Singer_Voice.md`, `RosterExpansion_Sub_Roadmap.md` §5/§8,
`SSoT_Card_Authoring_Contracts.md`.

## 0. Etiquetado de verdad (obligatorio en cada fila)

| Etiqueta | Significado |
|---|---|
| **[C-MGP]** | Confirmado en el snapshot MGP 2026-08-10 (doc + coverage-matrix citados). |
| **[C-ALWTTT]** | Confirmado en código/SSoT de ALWTTT. |
| **[V]** | Afirmado (por sesión o por usuario) pero **no verificado** — pregunta para la sesión MGP. |
| **[P]** | Planificado / diseño; no construido en ningún lado. |

Regla: no autorar ninguna carta cuya capacidad distintiva no sea al menos **[C-MGP]** o
**[C-ALWTTT]**. Las **[V]** pasan por §7 antes.

## 1. Regla de frontera

Este documento **no redefine** internals de MidiGenPlay. Cada capacidad cita su doc del snapshot;
la autoridad real vive allí (`SSoT_ALWTTT_MidiGenPlay_Boundary.md`). Lo que ALWTTT posee es el
significado de gameplay (coste, condición, personaje, momento de juego) y la superficie de
handoff (`SongConfig`, `TrackStyleBundleSO` y derivados).

## 2. Roster × mecánica de identidad

Enum real: `MusicianCharacterType { Conito, Robot, Gusano, Cantante }`. Mapeo de nombres de
diseño **inferido** (confirmar): C2 → `Robot` · Sibi → `Gusano` · Zig → `Cantante`.

| Personaje | Rol musical principal | Gancho mecánico de sus cartas especiales | Estado del gancho |
|---|---|---|---|
| Conito | Bajo (Finger/Slap v1 ya construidos, R2) | **Voltage** (coste por carta, D-R5-21) | [C-ALWTTT] recurso R5-a/b; coste per-card = R5-d |
| C2 (Robot) | Batería / Rhythm | **Simple** — Inspiration estándar, sin condición ("es un robot") | [C-ALWTTT] Inspiration existe |
| Sibi (Gusano) | Soporte / campo (Psychic Wave, Singing Field) | **Condición**: haber jugado *Read the Room* (la carta existe, R4) en la ventana definida | [P] la carta existe [C-ALWTTT]; el **flag de condición** no |
| Zig (Cantante) | Melody / voz cantada (Pink Trombone) | **Estilos de voz** como assets (3 perfiles iniciales) | [P] esquema `VoiceProfile` borrador en `PinkTrombone_Voice_Levers.md` |

Nota de diseño (usuario, 2026-08-23): las cartas normales dan bases; las especiales dan
protagonismo por personaje y abren builds (full-funk, jazz-rock, metal-bossa…). La taxonomía de
coste/condición de §4 es el vehículo mecánico de esa identidad.

## 3. Catálogo de mini-sistemas por rol

### 3.1 Bassline (Conito)

| Sistema | Superficie autorable | Qué se oye | Estado | Fuente |
|---|---|---|---|---|
| **SlapPocket** (el bajo sigue a la batería) | `BasslineCardConfigSO.pocketMode = SlapPocket` + `pocketSlapBoost/PopBoost/CustomLanes/SlapLanes/PopLanes` | Slap en golpes de bombo (nota elegida), pop en caja (octava arriba), a la velocity del step de batería; eventos sin onset → figura normal | **[C-MGP]** POCKET-1/-2 (2026-07-25) | `Handoff_MGP_POCKET.md`; `SSoT_Composer_Bass_Track` §3.7–3.7.1 |
| **SelfPocket** (figura slap/pop autónoma) | `pocketMode = SelfPocket` + vocabulario `Ghost / GhostPop / HammerOn / PullOff`, `QuarterBeat` | Figura funk slap/pop sin batería; leyes de velocity-factor y gate por clase | **[C-MGP]** SLAPFIG-1/-2/-2b | `SSoT_Composer_Bass_Track` §3.7.2–3.7.3 |
| **Legato real (pitch bend)** | hammer/pull en **grados de escala** (`hammerOffsetDegrees`…) | Hammer-ons / pull-offs como gesto de bend con reset a centro | **[C-MGP]** BEND-1 | `SSoT_CONTRACTS` §11; `PitchBendWriter` |
| **Walking bass improvisado** | `arpeggioToneMode = ImprovisedWalk` | Walk jazz: el compositor pone pitches, el engine ritmo/dinámica; variación por `ResolveWalkSeed` | **[C-MGP]** WALK-2 | `SSoT_Composer_Bass_Track` §3.6bis |
| Timbre slap | Patch GM en `MIDIInstrumentSO` (Slap Bass 1/2) | Decisión de contenido consumer-side | **[C-ALWTTT]** R2 whitelist | Handoff §1 |
| Octavas fingered ("Something About Us") | ¿`BassUpperSplit` (=9)? ¿PerBeat + registro? — **camino no mapeado** | Pulso de octavas limpio, fingered | **[V]** §7-Q2 | — |
| Funk fusion con ghost/mute entre notas ("Space Cowboy") | `Ghost`/`GhostPop` existen; densidad de adorno "in between" sin verificar | Fingerstyle con notas fantasma intercaladas | parcial **[C-MGP]** + **[V]** §7-Q4 | §3.7.3 |
| Gallop / chug metal en bajo ("Iron Maiden") | `Chug` (=8) es Tier-2 del **backing**; precedente: carta de bajo con `Bossa` **degrada a Block** | Pulso palm-mute grave | **[V]** §7-Q1 | `ChordExpressionType.cs`; CURRENT_STATE (degrade) |

### 3.2 Rhythm (C2)

| Sistema | Superficie autorable | Estado | Fuente |
|---|---|---|---|
| DrumPatternData DSL — 8 géneros v1 (funk, rock, jazz, hip-hop, latin, metal, dnb, country) + sub-estilos (boom bap, trap, shuffle, blast, gallop, amen, tumbao, son, samba, train…) | Editor de texto + `patternOverride` en `RhythmCardConfigSO` | **[C-MGP]** | `SSoT_Authoring_Rhythm_Patterns` |
| Recipe procedural (hat density, modo) | `recipeOverride` | **[C-MGP]** | expressive-surface §3 fila 16 |
| Campos de densidad/feel (`kickDensity`, `snareGhostNoteChance`, `hatSubdivisionBias`, fills) | `RhythmCardConfigSO` | ⚠ gap §8.5 del expressive-surface — **[V]** §7-Q6 | expressive-surface fila 17 |
| **Canal de onsets** (fuente del pocket) | Publicado por el track Rhythm durante el compose | **[C-MGP]**; obligaciones consumer en §6 | `SSoT_Composer_Rhythm_Track` §3bis |
| Fill windows | — | **[P]** | `Design_Fill_Window_v0_1.md` |

### 3.3 Backing (progresiones + guitarra de acompañamiento)

| Sistema | Superficie autorable | Estado | Fuente |
|---|---|---|---|
| Progresión explícita / palette (Roman, TS-aware) | `progressionOverride` / `progressionPalette` | **[C-MGP]** | expressive-surface filas 12–13 |
| Articulación Tier-1: `Block / PerBeat / Offbeat (ska) / Staccato / ArpeggioUp / ArpeggioDown / Random` | `BackingCardConfigSO.chordExpression` (persistente por carta) | **[C-MGP]** CA-T1 | `ChordExpressionType.cs` |
| Tier-2 reshaping: **`PowerChord` (7)** y **`Chug` (8)** palm-mute a `ArpeggioRate` | mismo campo | **[C-MGP]** CA-T2 | íd.; `SSoT_Composer_Backing_Track` §8.6 |
| **`BassUpperSplit` (9)** y **`Bossa` (10)** auténtica (plantilla de comping 1 compás) | mismo campo | **[C-MGP]** CA-T2-BOSSA(-V2) | CURRENT_STATE 2026-07-24 |
| Voice leading override | `voiceLeadingOverride` | **[C-MGP]** | fila 14 |
| Modulación direccional (octaveHint, transients no cacheables) | `ModulationEffect` | **[C-ALWTTT]+[C-MGP]** MOD-DIR | fila 8 |
| Comping sincopado tipo Stevie Wonder | no existe figura sincopada dedicada más allá de `Offbeat` | **[V→ask probable]** §7-Q3 | — |

### 3.4 Melody / Harmony (Zig lead; solos)

| Sistema | Superficie autorable | Estado | Fuente |
|---|---|---|---|
| `MelodicStyleSO` (estrategia + directivas por frase) | `MelodyCardConfigSO.style` | **[C-MGP]** | filas 21–22 |
| `PhrasePaletteSO` + arquetipos (EvenFlow / BurstThenHold / SustainLeadIn) | `phrasePaletteOverride` | **[C-MGP]** | fila 23 |
| Patrones verbatim por grados (key/mode-adaptive por construcción) | `patternOverride` (`MelodyPatternData`) | **[C-ALWTTT]** verificación R0 | roadmap §5 (Zig) |
| Harmony Tier A (`NearestDifferentChordTone`, two-pass sobre guía de Melody) | `HarmonyCardConfigSO` | **[C-MGP]**; validación auditiva pendiente (R6) | filas 25–26 |
| Pitch bend en Melody (approach notes, scoops) | seam **documentado y NO consumido** | **[V→ask]** §7-Q5 — habilita "voz/lead con approach de walking" | `SSoT_Composer_Melody_Track` §7 |
| Solo de un loop (bonus loop de Overload) | inyección de alcance de render (R5-d) | **en curso** | rehidratación R5-d |
| Tapping v1 (arpegio por grados; chord-aware = ask §8 #5) | planificado R8 | **[P]** | roadmap §4 |

### 3.5 Voz cantada (Zig / Pink Trombone) — consumer-side, fuera del PK

| Sistema | Superficie | Estado | Fuente |
|---|---|---|---|
| 6 macro-levers: `looseness · vibratoDepth · vibratoSpeedHz · diction · mouth · brightness` (tras `characterEnabled`; OFF = v6 exacto) | Inspector del singer | **[C-ALWTTT]** medido (Session 5) | `PinkTrombone_Voice_Levers.md` §1 |
| Identidad fuera del toggle: `transposeSemitones` (registro), ventana de effort `tensenessAtVel0/127`, `pitchLeadSeconds`/`leadFullInterval` (portamento/glide) | campos raw | **[C-ALWTTT]** | íd. §2 |
| **VoiceProfile como asset** (perfiles serializables; 3 estilos iniciales) | esquema BORRADOR | **[P]** — propuesta clave del usuario: pipeline de authoring de perfiles | íd. §0 |
| Grito J. Brown / falsete Bee Gees / crooner | combinaciones de levers (p. ej. shout ≈ tenseness alto + brightness alto + looseness medio; falsete ≈ transpose alto + vibrato alto — **a diseñar y validar por oído**) | **[P]** | — |
| `SingerVoiceDirector` one-shot API · segunda voz (Tier B) | — | **[P]** R6 | roadmap R6 |

## 4. Taxonomía de coste/condición de carta

| Tipo | Personaje ancla | Soporte runtime | Nota |
|---|---|---|---|
| Inspiration (int) | universal | **[C-ALWTTT]** | gate + spend existentes |
| **Recurso por stacks** (Voltage hoy) | Conito | **R5-d** (D-R5-26): par `(statusKey, amount)` por carta, gate pre-jugada + `SpendStacks` en commit | extensible a recursos futuros sin migrar `CardDefinition` |
| **Condición de gig** ("Read the Room jugado") | Sibi | **[P]** — requiere flag rastreable + ventana (¿canción? ¿gig?) + surface en overlay | batch propio |
| Simple | C2 | n/a | identidad "robot": fiabilidad sin condición |

## 5. Candidatos de carta v1 (borrador — NO autorar aún)

Referencias musicales solo como brújula de authoring; ningún asset lleva nombres reales.
Cartas ya planificadas en el roadmap se citan, no se duplican: *Amp Up* (Voltage +2, R8),
*Bossa Corda* (R8), *Tapping v1* (R8), *Overload* (R5-d).

| Carta (prov.) | Pers. | Rol | Coste | Sistema distintivo | Capacidad | Ref. |
|---|---|---|---|---|---|---|
| Slap Groove | Conito | Bass | **Voltage 3** | SelfPocket slap/pop simple | [C-MGP] | funk básico |
| Super Slap | Conito | Bass | **Voltage 6** | SelfPocket denso + HammerOn/PullOff + boosts | [C-MGP] | RHCP "Aeroplane" |
| In the Pocket | Conito | Bass | Voltage ? | **SlapPocket** (requiere Rhythm delante — §6) | [C-MGP] + deudas §6 | — |
| Walkin' | Conito | Bass | Insp. | ImprovisedWalk | [C-MGP] | jazz |
| Octave Drive | Conito | Bass | Insp. | octavas fingered | **[V]** Q2 | Daft Punk "Something About Us" |
| Ghost Funk | Conito | Bass | Insp./Volt. | fingerstyle + ghosts intercalados | parcial + **[V]** Q4 | Jamiroquai "Space Cowboy" |
| Iron Gallop | Conito | Bass | Voltage ? | chug/gallop grave | **[V]** Q1 | Iron Maiden |
| Upstroke | C2/any | Backing | Insp. | `Offbeat` (ska) | [C-MGP] | — |
| Palm Wall | any | Backing | Insp. | `Chug` | [C-MGP] | metal |
| Bossa Comping | Conito? | Backing | Insp. | `Bossa` (10) | [C-MGP] | — (relación con *Bossa Corda* R8: decidir) |
| Wonder Groove | any | Backing | Insp. | comping sincopado | **[V→ask]** Q3 | Stevie Wonder |
| Shout! | Zig | Voz | condición/Insp. | VoiceProfile "shout" | **[P]** (pipeline perfiles) | James Brown |
| Falsetto | Zig | Voz | Insp. | VoiceProfile "falsetto" | **[P]** | Bee Gees |
| Croon | Zig | Voz | Insp. | VoiceProfile "crooner" | **[P]** | — |

## 6. "Tocar en conjunto" — ensemble play

El **pocket** es el primer mecanismo cross-track real: la carta de **bajo** decide seguir
(`pocketMode` vive en `BasslineCardConfigSO`); la de batería solo aporta la fuente de onsets.
Consecuencia de diseño: la secuencia "C2 juega ritmo → Conito juega *In the Pocket*" funciona,
y también al revés (la batería que llega tarde dispara re-render con el mismo seed — con la
*determinism caveat* registrada: pitches estables por clase, octavas no garantizadas).

**Deudas de integración ALWTTT detectadas (bloquean cartas pocket, no R5-d):**

1. **Hash duty NO implementada.** El handoff exige (§2.1) que con `pocketMode != Off` la
   identidad del patrón de batería consumido entre en el hash del track de bajo.
   `SongConfigBuilder.ComputeHashFromTrackEntry` hoy hashea `role|styleBundle|overrides|type|gain`
   — nada del ritmo. Sin esto, cambiar la batería **replaya el stem de bajo obsoleto** desde la
   caché. [C-ALWTTT: pendiente]
2. **Orden Rhythm→Bassline no garantizado.** El handoff exige (§2.2) Rhythm antes de Bassline en
   `Part.Tracks`; `SongConfigBuilder.FromUI` itera `p.tracks` en orden de modelo (= orden de
   jugada). Bajo antes que batería ⇒ render desacoplado con warning. Falta un sort estable por
   rol (o regla equivalente) en el builder. [C-ALWTTT: pendiente]

Extensión futura ([P], diseño): cadenas de especiales (especial de A habilita/potencia especial
de B), sinergia declarada en carta vs emergente por sistema. Pregunta abierta: ¿se telegrafiía
la sinergia en la UI de carta (chip "combina con…")?

## 7. Preguntas de verificación para la sesión MGP

1. **Tier-2 en bajo:** ¿`PowerChord`/`Chug` aplican a Bassline o degradan (precedente Bossa→Block)?
   Camino recomendado para un gallop metal de bajo.
2. **Octavas fingered:** ¿figura existente (`BassUpperSplit`? PerBeat+registro?) o ask nuevo?
3. **Comping sincopado** (clave 16th-funk / Stevie): ¿figura Tier-1 nueva, patrón autorable para
   backing, u otra vía? Formular como ask con criterio de aceptación.
4. **Densidad de ghosts "in between"** en SelfPocket: ¿cubierto por vocabulario actual o ask de
   densidad de adorno?
5. **PitchBendWriter en Melody** (seam §7 no consumido): coste/plan de adopción para approach
   notes de lead y de voz.
6. **Campos de feel de Rhythm** (gap §8.5 del expressive-surface): estado real de consumo.
7. **Vigencia del snapshot 2026-08-10:** ¿qué cambió desde entonces que afecte a §3?
8. Por cada capacidad usada por un candidato de §5: **ejemplo mínimo de payload/SO** de authoring.

## 8. Regla de actualización

Actualizar cuando (a) la sesión MGP responda §7 (las etiquetas [V] se resuelven a [C-MGP] o a
ask numerado en el ledger §8 del sub-roadmap), (b) se abra el batch de contenido que consuma §5,
o (c) R5-d cierre (el gancho Voltage per-card pasa a [C-ALWTTT]). Hasta entonces, este borrador
es el único registro; ningún SSoT lo referencia.

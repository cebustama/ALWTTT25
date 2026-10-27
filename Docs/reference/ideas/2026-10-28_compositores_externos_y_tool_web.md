# Compositores externos y tool web de pistas — 2026-10-28

**Tipo:** referencia (registro, no autoridad). **Para después de la demo del 11 dic.** Si algo de aquí
entra en el plan, se aplica en su doc gobernado (roadmap, `CURRENT_STATE.md`, SSoT); este archivo solo lo
registra.
**Fuente:** sesión del 2026-10-28 con Claudio. Las ideas son de Claudio (§1); lo marcado «agente» son
observaciones del agente, verificadas contra los docs de MidiGenPlay salvo donde se dice.

---

## 1. Las ideas de Claudio

1. **Sebastián Sanhueza** (chileno, amigo de Claudio, bajista del canal de YouTube *CoverSolutions*) quiere
   componer para ALWTTT. Claudio le pediría canciones cortas en MIDI, que sigan las reglas de pistas y
   usen la paleta de instrumentos del juego, para entrarlas **como patrones** y también **tal cual las
   compuso (sin random)**.
2. **Tool web, fuera de Unity**, para compositores sin conocimientos de Unity: crear pistas con las
   estructuras de datos de cada Track Type, pero simplificadas. Sería también una demo web de cómo diseñar
   pistas que funcionen bien juntas como canción.

## 2. Qué existe hoy (agente, verificado en docs)

- MidiGenPlay importa archivos MIDI a **batería, acordes y melodía**
  (`MidiGenPlay/Documentation~/authoring/SSoT_Authoring_MIDI_Import.md`).
- **«Tal cual»:** un patrón fijo en el bundle de la carta (`patternOverride` / `progressionOverride`, no
  paleta) + seed fija = mismo resultado siempre. Su canción completa = un juego de cartas, una por pista.
- **Pérdidas al importar** (§4–5 de ese SSoT): batería sin duraciones; acordes sin inversiones ni voicings
  (la articulación la pone la carta); melodía sin registro absoluto (6/8 roto: `MEL-BEATUNIT-1`); no se
  importan tempo ni compás; entrada cuantizada, sin swing.
- **El bajo no se importa** (D-MIDI4=A): sigue la progresión compartida con un patrón rítmico de carta
  (DSL `S P . - g G H L`, `authoring/SSoT_Authoring_Bass_Cards.md`). Salidas: (A) importar la línea de bajo
  como melodía con instrumento de bajo — sin probar; (B) escribirla como patrón de carta; (C) pedir a
  MidiGenPlay un patrón de bajo nota a nota (trabajo nuevo).

## 3. Hoja para Sebastián (borrador)

1. 8 compases por pieza.
2. Un compás fijo por pieza (dilo en el nombre del archivo). El tempo lo pone el juego.
3. Una pista MIDI por rol: batería, acordes, bajo, melodía. Todo cuantizado.
4. Acordes en bloque, una pista. Di la tonalidad aparte. Sin novenas, sextas, `add9` ni acordes con barra.
5. Batería General MIDI en el canal 10.
6. Sonido: el soundfont del juego, si se puede exportar (⚠ por verificar en
   `MidiGenPlay/Documentation~/planning/active/ALWTTT_MidiGenPlay_Soundfont_Emulation_Report_2026-03-24.md`).

## 4. Notas sobre la tool web (agente)

- **El contrato ya existe en texto.** Los editores de Unity ya parsean formatos de texto: rejilla DSL de
  batería, bloque Roman de acordes (skill `chord-progression-importer`), DSL de bajo. Si la web exporta
  **esos textos** (y MIDI para melodía), se pegan en Unity sin pérdidas y sin código nuevo en el juego.
- **Riesgo:** la vista previa web no es el compositor de MidiGenPlay. Si la web reimplementa la
  articulación o el bajo, las dos versiones divergen y «suena bien en la web» deja de valer. Opciones a
  decidir: preview aproximado con aviso, o exportar desde Unity unos «oráculos» (MIDI renderizado con
  seed) para comparar.
- **Validador de canción:** comprobar en la web las reglas de §3 (8 compases, alfabeto, compás) da la
  mitad del valor sin tocar el motor.
- **Conexión:** encaja con la web personal (Labs / explainers) como escaparate, y con Music Lab
  (progresiones analizadas).

# Taller de batería

Página privada para oír, evaluar y crear patrones de batería con los assets reales del proyecto (creada el 2026-11-06). Primera de la serie por tipo de pista: después, bajo y melodía con voz.

- **Dónde:** artefacto https://claude.ai/artifact/U3sY1NnqSVhN6Jp8Kefj1Q y web privada https://claudiobustamante.com/privado/taller-bateria/ (`python -I -X utf8 build.py --web`).
- **Evaluar y crear en el artefacto:** allí se guarda en su `db` (colecciones `evaluaciones` y `patrones`, con el `guid` como id) y se sincroniza entre dispositivos. En la web se queda en ese navegador.
- **Archivos:** `template.html` + `build.py` → `index.html` y `patrones.json`. `build.py` lee los assets en solo lectura y añade `datos-music-lab.json` (BPM, etiquetas y notas). Antes de publicar, la exportación se comprueba con `verificar/`.

## Qué hace

- **Catálogo:** los 36 `DrumPatternData` de `Assets/Resources/ScriptableObjects/Patterns/Drums/`, por paleta, con su peso, más «Mis patrones».
- **Tiempo:** el de `RhythmTrackComposer.ComposeFromGrid`: paso = `(60/BPM) × (4/unidad) / subdivisiones`. En 6/8 el BPM sigue contando negras, así que estos patrones suenan como en el juego, al doble de lo que se sienten. El formato no tiene swing: solo existe si está escrito en la rejilla.
- **Kit:** 22 sonidos sintetizados, cada uno renderizado una vez a un buffer en tres fuerzas y nivelado por sonoridad (los 50 ms más fuertes), no por pico. El hi-hat abierto se corta con el cerrado o el de pie. Son los 16 que usan los assets y 6 más para crear. No es el sonido de MidiGenPlay: para fidelidad, *MidiGenPlay → Smoke → Composition Smoke*.
- **Crear:** como el modo Grid de `DrumPatternEditorWindow`: compás, compases (1–16), subdivisión (1–4, el mismo límite que el editor), carriles con instrumento y velocidad normal. La casilla pasa por normal, acento (120), ghost (50) y vacía. Al cambiar la rejilla cada golpe conserva su lugar en el tiempo, y lo que no cabe se avisa y se puede deshacer. Los patrones del catálogo no se editan: se edita una copia.
- **Exportar:** el «setup card + DSL» que lee `Import from Clipboard`, generado en la página (bloque `/*EXPORT*/` de `template.html`). El nombre no viaja al asset: se pone en *Save As New Asset*.

## Verificación de la exportación

`verificar/` compila el `DrumPatternTextParser`, el `DrumPatternEditorImporter` y el `DrumPatternData` reales de MidiGenPlay (enlazados, no copiados; `UnityShim.cs` solo sustituye tipos de Unity) y hace lo que hace el editor: importar y aplicar el texto sobre carriles vacíos, con LF y CRLF.

```
cd verificar
dotnet run -c Release                                # los 36 del catálogo
node probar-export.mjs                               # la exportación de la página = build.py; 400 patrones aleatorios; reshape
dotnet run -c Release --no-build -- probar-export.json
```

Resultado del 2026-11-06: catálogo con 35 idénticos y 1 con pérdida declarada, 0 avisos. Aleatorios: 400 sin fallos (300 idénticos; 100 llevan a propósito velocidades que el texto no puede escribir, y esa pérdida coincide con la declarada), con nombres trampa como `Measures: 9`.

**Pérdida conocida:** `Drum_4-2_8m_MIDI_Import_Test` tiene 29 golpes con velocidades fuera de los tres niveles del texto. La página suena con las exactas y avisa; el Import no avisa. Al duplicarlo pasa lo mismo.

**Pegado real en Unity: validado por Claudio el 2026-11-09** (editó un patrón en la página y lo importó al juego). Siguiente paso del tema, en MidiGenPlay: acentos y fills (`MidiGenPlay/Documentation~/planning/active/ARRANQUE_acentos_y_fills.md`).

## Revisión musical

`revision-music-lab-2026-11-06.md`: criterios para ✓ ~ ✗ y patrones que no suenan a lo que dicen (`FF_LatinClaveEngine`, `CS_CountryLilt68`, `CS_FunkShuffle68`, `FF_TrainBeat`…). Nada aplicado a los assets. Causa probable, por confirmar: `genre_vocabulary.md` del skill `rhythm-pattern-generator`. Los BPM son inferidos y las rejillas que propone no se han oído.

## Para la v2

- **Modo texto:** escribir y pegar glifos, como el modo Text del editor, y pegar un bloque completo (hace falta portar `DrumPatternEditorImporter` a la página).
- **Velocidad exacta por casilla** (`[V]` en el editor). Hoy solo hay los tres niveles, y el texto tampoco lleva más.
- **Crear con LLM:** tarea en `/api/llm` con el contrato de `DrumPatternLLMPromptBuilder`, validada con el mismo importador. Reutilizar lo del taller armónico (`ARRANQUE_LLM_en_el_taller.md`) y corregir antes `genre_vocabulary.md`.
- **Decidir con los ✓ ~ ✗:** qué patrones se arreglan y si los de peso 0 vuelven.
